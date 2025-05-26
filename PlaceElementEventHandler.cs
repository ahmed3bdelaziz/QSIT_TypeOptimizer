// PlaceElementEventHandler.cs
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Linq; // Required for .OrderBy() and .FirstOrDefault()

namespace QSIT_TypeOptimizer
{
    // This class will handle the actual placement operation on Revit's main thread.
    public class PlaceElementEventHandler : IExternalEventHandler
    {
        // Properties to hold data passed from the MainForm
        public ElementId FamilySymbolId { get; set; }
        public Reference HostReference { get; set; }
        public XYZ InsertionPoint { get; set; }
        public Document RevitDocument { get; set; } // The active Revit Document

        // A delegate and event to notify the MainForm when the operation is complete or an error occurs.
        // This allows the MainForm to react (e.g., re-show itself, update grid).
        public delegate void PlacementCompletedEventHandler(bool success, string message);
        public event PlacementCompletedEventHandler PlacementCompleted;

        public void Execute(UIApplication app)
        {
            // Perform basic validation before starting the transaction
            if (RevitDocument == null || FamilySymbolId == ElementId.InvalidElementId || HostReference == null || InsertionPoint == null)
            {
                PlacementCompleted?.Invoke(false, "Missing data for placement operation.");
                return;
            }

            // Retrieve the necessary elements within the Revit API context
            FamilySymbol symbol = RevitDocument.GetElement(FamilySymbolId) as FamilySymbol;
            Element hostElement = RevitDocument.GetElement(HostReference.ElementId);

            if (symbol == null)
            {
                PlacementCompleted?.Invoke(false, $"Error: Family Symbol with ID {FamilySymbolId.IntegerValue} not found in document.");
                return;
            }
            if (hostElement == null)
            {
                PlacementCompleted?.Invoke(false, $"Error: Host element with ID {HostReference.ElementId.IntegerValue} not found in document.");
                return;
            }

            try
            {
                // All Revit API modifications MUST be wrapped within a transaction
                using (Transaction tx = new Transaction(RevitDocument, $"Place {symbol.Name}"))
                {
                    tx.Start();

                    // Ensure the FamilySymbol is active before placement
                    if (!symbol.IsActive)
                    {
                        symbol.Activate();
                        // Regenerate the document after activating the symbol (often recommended)
                        RevitDocument.Regenerate();
                    }

                    // Determine the appropriate host level. Most hosted elements use the host's level.
                    Level hostLevel = RevitDocument.GetElement(hostElement.LevelId) as Level;
                    if (hostLevel == null)
                    {
                        // Fallback: If for some reason the host has no level, try to find a default one
                        hostLevel = new FilteredElementCollector(RevitDocument)
                            .OfClass(typeof(Level))
                            .Cast<Level>()
                            .OrderBy(l => l.Elevation) // Pick the lowest level as a default
                            .FirstOrDefault();
                        if (hostLevel == null)
                        {
                            tx.RollBack(); // Rollback the transaction if no level is found
                            PlacementCompleted?.Invoke(false, "Could not determine a suitable host level for placement. Ensure your project has levels.");
                            return;
                        }
                    }

                    // Create the new family instance using an overload that supports a host element and level
                    FamilyInstance inst = RevitDocument.Create.NewFamilyInstance(
                        InsertionPoint,
                        symbol,
                        hostElement,
                        hostLevel,
                        Autodesk.Revit.DB.Structure.StructuralType.NonStructural // Specify as non-structural for Doors/Windows
                    );

                    // Assign a random comment to the newly placed instance
                    string generatedComment = "QSIT_" + Guid.NewGuid().ToString().Substring(0, 5); // Simple unique comment
                    var commentParam = inst.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                    if (commentParam != null && !commentParam.IsReadOnly && commentParam.StorageType == StorageType.String)
                    {
                        commentParam.Set(generatedComment);
                    }
                    else
                    {
                        // Log this internally if needed, but don't fail placement just for comment.
                        // For a real app, you might want more robust comment management.
                    }

                    tx.Commit(); // Commit the transaction to finalize changes
                    PlacementCompleted?.Invoke(true, $"Successfully placed a new {symbol.Name} with comment: {generatedComment}");
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions during the Revit API operation and report back to MainForm
                PlacementCompleted?.Invoke(false, $"An error occurred during placement: {ex.Message}");
            }
        }

        // Returns the name of the external event for debugging/logging purposes
        public string GetName()
        {
            return "Place Element External Event Handler";
        }
    }
}