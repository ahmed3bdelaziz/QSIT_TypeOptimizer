// Command.cs
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms; // Required for using System.Windows.Forms.Form

namespace QSIT_TypeOptimizer
{
    [Transaction(TransactionMode.Manual)]
    public class QSITTypeOptimizerCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Obtain the current UI document, which includes selection capabilities
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;

            // Create and show the MainForm modelessly
            // IMPORTANT: Showing modelessly allows Revit to remain interactive for picking.
            MainForm form = new MainForm(uiDoc);
            form.Show(); // <--- KEY: Show() instead of ShowDialog() for modeless behavior

            // Return Result.Succeeded. The form will stay open until the user closes it.
            return Result.Succeeded;
        }
    }
}