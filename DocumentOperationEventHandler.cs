// DocumentOperationEventHandler.cs
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq; // For .ToList(), .FirstOrDefault() etc.


namespace QSIT_TypeOptimizer
{
    // Enum to define the specific operation to perform
    public enum DocumentOperationType
    {
        None,
        DeleteTypes,
        DuplicateRenameTypes,
        UpdateInstancesToQSIT,
        AssignManualComment,
        RandomizeComment
    }

    public class DocumentOperationEventHandler : IExternalEventHandler
    {
        // Properties to pass data from MainForm to the handler
        public DocumentOperationType OperationType { get; set; } = DocumentOperationType.None;
        public Document RevitDocument { get; set; }

        // Data for DeleteTypes
        public List<Tuple<ElementId, string, string>> TypesToDelete { get; set; } // (Id, TypeName, FamilyName)

        // Data for DuplicateRenameTypes
        public List<ElementId> TypesToDuplicate { get; set; } // List of original type IDs

        // Data for UpdateInstancesToQSIT
        public string CategoryForUpdate { get; set; }
        public HashSet<string> SelectedOriginalTypeNames { get; set; } // TypeNames selected in the grid

        // Data for AssignManualComment
        public string ManualComment { get; set; }

        // Common properties/delegates for all operations
        public delegate void OperationCompletedEventHandler(bool success, string message, DocumentOperationType completedType);
        public event OperationCompletedEventHandler OperationCompleted;

        public void Execute(UIApplication app)
        {
            if (RevitDocument == null)
            {
                OperationCompleted?.Invoke(false, "Revit Document is not available.", OperationType);
                return;
            }

            // Wrap all document modifications in a transaction
            using (Transaction tx = new Transaction(RevitDocument, $"QSIT: {OperationType}"))
            {
                try
                {
                    tx.Start();

                    switch (OperationType)
                    {
                        case DocumentOperationType.DeleteTypes:
                            PerformDeleteTypes(RevitDocument);
                            break;
                        case DocumentOperationType.DuplicateRenameTypes:
                            PerformDuplicateRenameTypes(RevitDocument);
                            break;
                        case DocumentOperationType.UpdateInstancesToQSIT:
                            PerformUpdateInstancesToQSIT(RevitDocument);
                            break;
                        case DocumentOperationType.AssignManualComment:
                            PerformAssignComment(RevitDocument, ManualComment);
                            break;
                        case DocumentOperationType.RandomizeComment:
                            PerformAssignComment(RevitDocument, null); // Null indicates random comment
                            break;
                        case DocumentOperationType.None:
                        default:
                            OperationCompleted?.Invoke(false, "No specific operation type defined.", OperationType);
                            return;
                    }

                    tx.Commit();
                    OperationCompleted?.Invoke(true, $"{OperationType} operation completed successfully.", OperationType);
                }
                catch (Exception ex)
                {
                    if (tx.HasStarted() && tx.GetStatus() == TransactionStatus.Started)
                    {
                        tx.RollBack(); // Rollback if an error occurred before commit
                    }
                    OperationCompleted?.Invoke(false, $"An error occurred during {OperationType} operation: {ex.Message}", OperationType);
                }
                finally
                {
                    // Reset operation type after execution
                    OperationType = DocumentOperationType.None;
                }
            }
        }

        public string GetName()
        {
            return "QSIT Document Operation Handler";
        }

        // --- Helper Methods for specific operations (called from Execute) ---

        private void PerformDeleteTypes(Document doc)
        {
            if (TypesToDelete == null || !TypesToDelete.Any())
            {
                OperationCompleted?.Invoke(false, "No types to delete.", OperationType);
                return;
            }

            int deletedCount = 0;

            foreach (var info in TypesToDelete)
            {
                ElementId typeId = info.Item1;

                // 1) Gather instance IDs for this type
                List<ElementId> instIds = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .Cast<Element>()
                    .Where(e => e.GetTypeId() == typeId)
                    .Select(e => e.Id)
                    .ToList();

                // 2) Filter out any elements that are pinned, read-only, or already gone
                List<ElementId> deletableInstIds = instIds
                    .Where(id =>
                    {
                        var e = doc.GetElement(id);
                        return e != null
                               && !e.Pinned            // not pinned
                               && e.Category != null   // has a category (system elements often have null)
                               && e.IsValidObject;     // guard against “already deleted”
                    })
                    .ToList();

                // 3) Try batch-delete, fallback to one-by-one if it blows up
                if (deletableInstIds.Count > 0)
                {
                    try
                    {
                        doc.Delete(deletableInstIds);
                    }
                    catch
                    {
                        // fallback: attempt each individually
                        foreach (var id in deletableInstIds)
                        {
                            try { doc.Delete(id); }
                            catch { /* skip @ stuck elements */ }
                        }
                    }
                }

                // 4) Finally delete the type itself (if it still exists)
                if (doc.GetElement(typeId) != null)
                {
                    try
                    {
                        doc.Delete(typeId);
                        deletedCount++;
                    }
                    catch
                    {
                        // type in use somewhere else—skip it
                    }
                }
            }

            OperationCompleted?.Invoke(
                true,
                $"Attempted deletion complete. {deletedCount} types removed.",
                OperationType);
        }

        private void PerformDuplicateRenameTypes(Document doc)
        {
            if (TypesToDuplicate == null || !TypesToDuplicate.Any())
            {
                OperationCompleted?.Invoke(false, "No types provided for duplication.", OperationType);
                return;
            }

            int duplicatedCount = 0;
            int skippedCount = 0;
            var existingTypeNamesInDoc = new FilteredElementCollector(doc)
                                            .WhereElementIsElementType()
                                            .Select(e => e.Name)
                                            .ToHashSet();

            foreach (var originalTypeId in TypesToDuplicate)
            {
                ElementType originalType = doc.GetElement(originalTypeId) as ElementType;
                if (originalType == null)
                {
                    skippedCount++;
                    continue;
                }

                if (originalType.Name.StartsWith("QSIT_"))
                {
                    skippedCount++;
                    continue;
                }

                string newName = "QSIT_" + originalType.Name;
                if (existingTypeNamesInDoc.Contains(newName))
                {
                    skippedCount++;
                    continue;
                }

                try
                {
                    ElementType newElementType = originalType.Duplicate(newName) as ElementType;
                    if (newElementType is FamilySymbol fs && !fs.IsActive)
                    {
                        fs.Activate();
                    }
                    duplicatedCount++;
                }
                catch (Exception ex)
                {
                    skippedCount++;
                    /* Log internally */
                }
            }
            OperationCompleted?.Invoke(true, $"Duplicated {duplicatedCount} types; {skippedCount} skipped.", OperationType);
        }

        private void PerformUpdateInstancesToQSIT(Document doc)
        {
            if (string.IsNullOrEmpty(CategoryForUpdate) || SelectedOriginalTypeNames == null || !SelectedOriginalTypeNames.Any())
            {
                OperationCompleted?.Invoke(false, "Missing data for updating instances.", OperationType);
                return;
            }

            int updatedCount = 0;
            int commentsAssignedCount = 0;

            var allInstances = GetInstancesForCategory(doc, CategoryForUpdate);
            var allTypes = GetAllTypesForCategory(doc, CategoryForUpdate);

            var updateCandidateMappings = new Dictionary<string, ElementId>();
            foreach (string originalTypeName in SelectedOriginalTypeNames)
            {
                string qsitTypeName = "QSIT_" + originalTypeName;
                var qsitTypeElement = allTypes.FirstOrDefault(t => t.Name == qsitTypeName);
                if (qsitTypeElement != null)
                {
                    updateCandidateMappings[originalTypeName] = qsitTypeElement.Id;
                }
                else
                {
                    // This case should ideally be caught by MainForm's pre-check
                    // If it happens here, just log and skip this type's instances
                    continue;
                }
            }

            // Temporary unique comments tracking within this specific operation
            // For a persistent _usedComments, it should be managed in MainForm and passed or managed globally.
            var tempUsedComments = new HashSet<string>();
            Random tempRandom = new Random();

            foreach (var instance in allInstances)
            {
                string instanceCurrentTypeName = GetInstanceTypeName(instance, CategoryForUpdate);

                if (updateCandidateMappings.TryGetValue(instanceCurrentTypeName, out ElementId newTypeId))
                {
                    try
                    {
                        if (instance.GetTypeId() != newTypeId)
                        {
                            instance.ChangeTypeId(newTypeId);
                            updatedCount++;
                        }

                        var commentParam = instance.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                        if (commentParam != null && !commentParam.IsReadOnly && commentParam.StorageType == StorageType.String)
                        {
                            string randomComment = GenerateRandomComment(tempRandom, tempUsedComments);
                            commentParam.Set(randomComment);
                            commentsAssignedCount++;
                        }
                    }
                    catch (Exception ex) { /* Log internally */ }
                }
            }
            OperationCompleted?.Invoke(true, $"Updated {updatedCount} instances; assigned comments to {commentsAssignedCount}.", OperationType);
        }

        private void PerformAssignComment(Document doc, string commentToAssign)
        {
            if (string.IsNullOrEmpty(CategoryForUpdate) || SelectedOriginalTypeNames == null || !SelectedOriginalTypeNames.Any())
            {
                OperationCompleted?.Invoke(false, "Missing data for assigning comments.", OperationType);
                return;
            }

            int commentsAssignedCount = 0;
            var allInstances = GetInstancesForCategory(doc, CategoryForUpdate);

            // Temporary unique comments tracking if assigning random comments
            var tempUsedComments = new HashSet<string>();
            Random tempRandom = new Random();

            foreach (var instance in allInstances)
            {
                string instanceTypeKey = GetInstanceTypeKey(instance, CategoryForUpdate);

                if (SelectedOriginalTypeNames.Contains(instanceTypeKey)) // This is now familyName|typeName
                {
                    var commentParam = instance.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                    if (commentParam != null && !commentParam.IsReadOnly && commentParam.StorageType == StorageType.String)
                    {
                        try
                        {
                            string finalComment = commentToAssign ?? GenerateRandomComment(tempRandom, tempUsedComments);
                            commentParam.Set(finalComment);
                            commentsAssignedCount++;
                        }
                        catch (Exception ex) { /* Log internally */ }
                    }
                }
            }
            OperationCompleted?.Invoke(true, $"Assigned comments to {commentsAssignedCount} instances.", OperationType);
        }

        // --- Re-implement common helper methods here for use within the event handler ---
        // These should be copied from MainForm and adapted to use 'doc' parameter.

        private List<Element> GetInstancesForCategory(Document doc, string category)
        {
            var collector = new FilteredElementCollector(doc);
            var list = new List<Element>();
            if (category == "Walls")
                list.AddRange(collector.OfClass(typeof(Wall)).OfType<Element>());
            else if (category == "Floors")
                list.AddRange(collector.OfClass(typeof(Floor)).OfType<Element>());
            else if (category == "Ceilings")
            {
                list.AddRange(collector.OfClass(typeof(Ceiling)).OfType<Element>());
            }
            else if (category == "Doors")
                list.AddRange(collector.OfCategory(BuiltInCategory.OST_Doors).WhereElementIsNotElementType().OfType<Element>());
            else if (category == "Windows")
                list.AddRange(collector.OfCategory(BuiltInCategory.OST_Windows).WhereElementIsNotElementType().OfType<Element>());
            return list;
        }

        private List<ElementType> GetAllTypesForCategory(Document doc, string category)
        {
            var collector = new FilteredElementCollector(doc);
            if (category == "Walls")
                return collector.OfClass(typeof(WallType)).OfType<ElementType>().ToList();
            if (category == "Floors")
                return collector.OfClass(typeof(FloorType)).OfType<ElementType>().ToList();
            if (category == "Ceilings")
            {
                return collector.OfClass(typeof(CeilingType)).OfType<ElementType>().ToList();
            }
            if (category == "Doors")
                return collector.OfCategory(BuiltInCategory.OST_Doors).WhereElementIsElementType().OfType<ElementType>().ToList();
            if (category == "Windows")
                return collector.OfCategory(BuiltInCategory.OST_Windows).WhereElementIsElementType().OfType<ElementType>().ToList();
            return new List<ElementType>();
        }

        // This is used by UpdateInstancesToQSIT and AssignComment, so needs to be defined here.
        // Also used by GetInstanceTypeKey
        private string GetInstanceTypeName(Element instance, string category)
        {
            ElementType type = null;
            if (category == "Walls" && instance is Wall w)
                type = w.WallType;
            else if (category == "Floors" && instance is Floor f)
                type = f.FloorType;
            else if (category == "Ceilings")
            {
                if (instance is Ceiling c)
                    type = RevitDocument.GetElement(c.GetTypeId()) as CeilingType; // Use RevitDocument from property
            }
            else if ((category == "Doors" || category == "Windows") && instance is FamilyInstance fi)
                type = fi.Symbol;

            if (type != null)
            {
                return type.Name;
            }
            return string.Empty;
        }

        // This is used by PerformAssignComment
        // Updated to use the familyName|typeName format for consistency with grid selection.
        private string GetInstanceTypeKey(Element instance, string category)
        {
            string familyName = string.Empty;
            string typeName = GetInstanceTypeName(instance, category);

            ElementType type = null;
            if (category == "Walls" && instance is Wall w) type = w.WallType;
            else if (category == "Floors" && instance is Floor f) type = f.FloorType;
            else if (category == "Ceilings") type = RevitDocument.GetElement(instance.GetTypeId()) as CeilingType;
            else if ((category == "Doors" || category == "Windows") && instance is FamilyInstance fi) type = fi.Symbol;

            if (type != null)
            {
                if (category == "Walls" || category == "Floors" || category == "Ceilings")
                    familyName = type.Category?.Name ?? type.FamilyName;
                else
                    familyName = type.FamilyName;
            }
            return familyName + "|" + typeName;
        }

        // Needs to be available for both PerformUpdateInstancesToQSIT and PerformAssignComment
        // And needs its own Random instance and tempUsedComments set
        private string GenerateRandomComment(Random rand, HashSet<string> tempUsedComments)
        {
            string comment;
            do
            {
                comment = ((char)('A' + rand.Next(26))) + rand.Next(1, 1000).ToString();
            }
            while (tempUsedComments.Contains(comment) && tempUsedComments.Count < (26 * 999));

            tempUsedComments.Add(comment);
            return comment;
        }

    }
}