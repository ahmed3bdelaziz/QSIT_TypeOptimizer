// DocumentReadEventHandler.cs
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq; // For .Cast(), .ToList(), .OrderBy(), .FirstOrDefault() etc.
using System.Windows.Forms; // Required for DataGridViewRow (if you need to pass it back as structured data)

namespace QSIT_TypeOptimizer
{
    public enum DocumentReadOperationType
    {
        None,
        LoadTypesAndComments // A single operation to load both for simplicity
    }

    public class DocumentReadEventHandler : IExternalEventHandler
    {
        public DocumentReadOperationType OperationType { get; set; } = DocumentReadOperationType.None;
        public Document RevitDocument { get; set; }
        public string CategoryForLoad { get; set; } // Category selected in MainForm

        public delegate void ReadCompletedEventHandler(bool success, string message, DocumentReadOperationType completedType, object resultData);
        public event ReadCompletedEventHandler ReadCompleted;

        public void Execute(UIApplication app)
        {
            if (RevitDocument == null)
            {
                ReadCompleted?.Invoke(false, "Revit Document is not available.", OperationType, null);
                return;
            }

            try
            {
                object result = null;
                switch (OperationType)
                {
                    case DocumentReadOperationType.LoadTypesAndComments:
                        result = PerformLoadTypesAndComments(RevitDocument, CategoryForLoad);
                        break;
                    case DocumentReadOperationType.None:
                    default:
                        ReadCompleted?.Invoke(false, "No specific read operation type defined.", OperationType, null);
                        return;
                }

                ReadCompleted?.Invoke(true, $"{OperationType} operation completed successfully.", OperationType, result);
            }
            catch (Exception ex)
            {
                ReadCompleted?.Invoke(false, $"An error occurred during {OperationType} operation: {ex.Message}", OperationType, null);
            }
            finally
            {
                OperationType = DocumentReadOperationType.None; // Reset
            }
        }

        public string GetName()
        {
            return "QSIT Document Read Handler";
        }

        // --- Helper Methods for specific read operations (called from Execute) ---

        private Tuple<List<object[]>, HashSet<string>> PerformLoadTypesAndComments(Document doc, string category)
        {
            List<object[]> rowData = new List<object[]>();
            HashSet<string> usedComments = new HashSet<string>();

            // First, initialize used comments
            var allInstances = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .ToElements();

            foreach (var inst in allInstances)
            {
                var param = inst.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                if (param != null && param.StorageType == StorageType.String && !string.IsNullOrEmpty(param.AsString()))
                {
                    usedComments.Add(param.AsString());
                }
            }

            // Then, load types and their instance counts
            if (string.IsNullOrEmpty(category)) return Tuple.Create(rowData, usedComments);

            if (category == "Walls")
            {
                LoadTypeInstanceCounts<WallType, Wall>(doc, rowData, t => t.Id, i => i.WallType.Id);
            }
            else if (category == "Floors")
            {
                LoadTypeInstanceCounts<FloorType, Floor>(doc, rowData, t => t.Id, i => i.FloorType.Id);
            }
            else if (category == "Ceilings")
            {
                LoadTypeInstanceCounts<CeilingType, Ceiling>(doc, rowData, t => t.Id, i => i.GetTypeId());
            }
            else if (category == "Doors")
            {
                LoadTypeInstanceCounts<FamilySymbol, FamilyInstance>(doc, rowData, t => t.Id, i => i.Symbol.Id, BuiltInCategory.OST_Doors);
            }
            else if (category == "Windows")
            {
                LoadTypeInstanceCounts<FamilySymbol, FamilyInstance>(doc, rowData, t => t.Id, i => i.Symbol.Id, BuiltInCategory.OST_Windows);
            }

            return Tuple.Create(rowData, usedComments);
        }

        // --- Re-implement LoadTypeInstanceCounts helpers here ---
        private void LoadTypeInstanceCounts<TType, TInst>(
            Document doc,
            List<object[]> rowData,
            Func<TType, ElementId> getTypeId,
            Func<TInst, ElementId> getInstTypeId)
            where TType : ElementType
            where TInst : Element
        {
            var types = new FilteredElementCollector(doc)
                .OfClass(typeof(TType))
                .Cast<TType>()
                .OrderBy(t => t.FamilyName)
                .ThenBy(t => t.Name)
                .ToList();

            var instances = new FilteredElementCollector(doc)
                .OfClass(typeof(TInst))
                .Cast<TInst>()
                .ToList();

            foreach (var t in types)
            {
                int count = instances.Count(i => getInstTypeId(i).IntegerValue == getTypeId(t).IntegerValue);
                string familyName = t.FamilyName;

                if (t.Category != null && (t is WallType || t is FloorType || t is CeilingType))
                {
                    familyName = t.Category.Name;
                }
                rowData.Add(new object[] { familyName, t.Name, count });
            }
        }

        private void LoadTypeInstanceCounts<TType, TInst>(
            Document doc,
            List<object[]> rowData,
            Func<TType, ElementId> getTypeId,
            Func<TInst, ElementId> getInstTypeId,
            BuiltInCategory bic)
            where TType : ElementType
            where TInst : Element
        {
            var types = new FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsElementType()
                .Cast<TType>()
                .OrderBy(t => t.FamilyName)
                .ThenBy(t => t.Name)
                .ToList();

            var instances = new FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsNotElementType()
                .Cast<TInst>()
                .ToList();

            foreach (var t in types)
            {
                int count = instances.Count(i => getInstTypeId(i).IntegerValue == getTypeId(t).IntegerValue);
                string familyName = t.FamilyName;

                rowData.Add(new object[] { familyName, t.Name, count });
            }
        }
    }
}