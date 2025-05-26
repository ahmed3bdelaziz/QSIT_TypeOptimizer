using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace QSIT_TypeOptimizer
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication uiApp)
        {
            const string TAB = "QSIT Tools";
            try { uiApp.CreateRibbonTab(TAB); }
            catch { /* tab already exists */ }

            // Create the panel
            var panel = uiApp.CreateRibbonPanel(TAB, "Type Optimizer");

            // Define the button
            var btnData = new PushButtonData(
                "QSIT_TypeOptimizer",                    // internal name
                "Type Optimizer",                        // displayed text
                Assembly.GetExecutingAssembly().Location,
                "QSIT_TypeOptimizer.QSITTypeOptimizerCommand"
            );

            // Load icons from embedded resources
            btnData.Image = LoadPng("Resources/qsit_16.png");  // 16×16
            btnData.LargeImage = LoadPng("Resources/qsit_32.png");  // 32×32

            btnData.ToolTip = "Bulk-optimize Revit family types: delete, duplicate/rename, comment.";

            panel.AddItem(btnData);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication uiApp) => Result.Succeeded;

        /// <summary>
        /// Helper to load an embedded PNG resource via pack URI.
        /// </summary>
        private BitmapImage LoadPng(string relativePath)
        {
            // pack URI format:
            // pack://application:,,,/AssemblyName;component/{relativePath}
            string asm = Assembly.GetExecutingAssembly().GetName().Name;
            var uri = new Uri($"pack://application:,,,/{asm};component/{relativePath}", UriKind.Absolute);
            return new BitmapImage(uri);
        }
    }
}
