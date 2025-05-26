// MainForm.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace QSIT_TypeOptimizer
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        // Existing placement external event
        private ExternalEvent _placeElementEvent;
        private PlaceElementEventHandler _placeElementEventHandler;

        // --- NEW: External Event for Document Operations (Modifications) ---
        private ExternalEvent _documentOperationEvent;
        private DocumentOperationEventHandler _documentOperationEventHandler;

        // --- NEW: External Event for Document Read Operations (Loading UI) ---
        private ExternalEvent _documentReadEvent;
        private DocumentReadEventHandler _documentReadEventHandler; // We'll create this next

        private const string QSIT_PREFIX = "QSIT_";
        private const BuiltInParameter INSTANCE_COMMENTS_PARAM = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;
        private Random _random = new Random(); // Used for local random comments (e.g. for temporary use in form)
        private HashSet<string> _usedComments = new HashSet<string>(); // Global tracking of used comments for random generation

        public MainForm(UIDocument uiDoc)
        {
            InitializeComponent();

            _uiDoc = uiDoc;
            _doc = uiDoc.Document;

            // Initialize ExternalEvent handlers
            _placeElementEventHandler = new PlaceElementEventHandler();
            _placeElementEventHandler.RevitDocument = _doc;
            _placeElementEventHandler.PlacementCompleted += PlaceElementEventHandler_PlacementCompleted;
            _placeElementEvent = ExternalEvent.Create(_placeElementEventHandler);

            // --- NEW: Initialize DocumentOperationEventHandler ---
            _documentOperationEventHandler = new DocumentOperationEventHandler();
            _documentOperationEventHandler.RevitDocument = _doc;
            _documentOperationEventHandler.OperationCompleted += DocumentOperationEventHandler_OperationCompleted;
            _documentOperationEvent = ExternalEvent.Create(_documentOperationEventHandler);

            // --- NEW: Initialize DocumentReadEventHandler ---
            _documentReadEventHandler = new DocumentReadEventHandler();
            _documentReadEventHandler.RevitDocument = _doc;
            _documentReadEventHandler.ReadCompleted += DocumentReadEventHandler_ReadCompleted;
            _documentReadEvent = ExternalEvent.Create(_documentReadEventHandler);

            // Subscribe to form and control events
            this.Load += MainForm_Load; // This will trigger initial data load
            comboCategory.SelectedIndexChanged += ComboCategory_SelectedIndexChanged;
            btnDelete.Click += BtnDelete_Click;
            btnDuplicateRename.Click += BtnDuplicateRename_Click;
            btnUpdateInstancesToQSIT.Click += BtnUpdateInstancesToQSIT_Click;
            btnAssignCommentManual.Click += BtnAssignCommentManual_Click;
            btnRandomizeComment.Click += BtnRandomizeComment_Click;
            btnPlace.Click += BtnPlace_Click;
            dataGridViewTypes.SelectionChanged += DataGridViewTypes_SelectionChanged;
            dataGridViewTypes.CellContentClick += dataGridViewTypes_CellContentClick;

            dataGridViewTypes.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        // --- NEW: Event handler for DocumentOperationEventHandler completion ---
        private void DocumentOperationEventHandler_OperationCompleted(bool success, string message, DocumentOperationType completedType)
        {
            if (success)
            {
                Log($"{completedType} successful: {message}");
                MessageBox.Show(message, $"{completedType} Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Log($"{completedType} failed: {message}");
                MessageBox.Show(message, $"{completedType} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Always re-load data and re-show form after a modification operation
            this.Visible = true;
            this.Activate();
            // --- EDITED: Trigger a Read Event to refresh UI after any modification ---
            TriggerDocumentRead(DocumentReadOperationType.LoadTypesAndComments);
            dataGridViewTypes.Focus();
        }

        // --- NEW: Event handler for PlaceElementEventHandler completion ---
        private void PlaceElementEventHandler_PlacementCompleted(bool success, string message)
        {
            if (success)
            {
                Log($"Placement successful: {message}");
                MessageBox.Show(message, "Placement Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Log($"Placement failed: {message}");
                MessageBox.Show(message, "Placement Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.Visible = true;
            this.Activate();
            // --- EDITED: Trigger a Read Event to refresh UI after placement ---
            TriggerDocumentRead(DocumentReadOperationType.LoadTypesAndComments);
            dataGridViewTypes.Focus();
        }

        // --- NEW: Event handler for DocumentReadEventHandler completion ---
        private void DocumentReadEventHandler_ReadCompleted(bool success, string message, DocumentReadOperationType completedType, object resultData)
        {
            if (!success)
            {
                Log($"Read operation {completedType} failed: {message}");
                MessageBox.Show($"Error reading Revit data: {message}", "Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Process resultData based on completedType
            if (completedType == DocumentReadOperationType.LoadTypesAndComments)
            {
                // This is a tuple returned by the handler (List<DataGridViewRow>, HashSet<string>)
                var data = resultData as Tuple<List<object[]>, HashSet<string>>; // Changed to object[] for DataGridViewRow data
                if (data != null)
                {
                    dataGridViewTypes.Rows.Clear();
                    foreach (var rowData in data.Item1)
                    {
                        dataGridViewTypes.Rows.Add(rowData);
                    }
                    _usedComments = data.Item2; // Update the form's global _usedComments list
                    Log($"Data loaded for '{comboCategory.SelectedItem as string}'. Grid updated.");
                }
            }
            // Add other cases if you have more read operations (e.g. specific data for other controls)

            UpdateButtonStates(); // Update button states after data is loaded
            this.Activate(); // Ensure form is active after loading
        }

        // --- NEW: Helper to trigger DocumentReadEvent ---
        private void TriggerDocumentRead(DocumentReadOperationType operationType)
        {
            this.Cursor = Cursors.WaitCursor; // Show busy cursor
            _documentReadEventHandler.OperationType = operationType;
            _documentReadEventHandler.CategoryForLoad = comboCategory.SelectedItem as string; // Pass category for load
            _documentReadEvent.Raise();
            // Form is NOT hidden here, as read operations are usually fast and don't block UI interaction heavily.
            // If they are long, you might want to hide the form.
            this.Cursor = Cursors.Default; // Reset cursor
        }

        // --- EDITED: InitializeUsedComments is now part of DocumentReadEventHandler ---
        // This method will be called via the DocumentReadEventHandler
        // private void InitializeUsedComments() { /* Moved to DocumentReadEventHandler */ }

        private int _lastDupCount = 0;
        private int _lastSkippedCount = 0;

        private void MainForm_Load(object sender, EventArgs e)
        {
            comboCategory.Items.Clear();
            comboCategory.Items.AddRange(new object[] { "Walls", "Floors", "Ceilings", "Doors", "Windows" });
            comboCategory.SelectedIndex = 0; // Select the first item by default
            // --- EDITED: Initial data load is now via External Event ---
            // LoadTypes() and InitializeUsedComments() will be triggered by DocumentReadEventHandler
            // Initial read will happen in DocumentReadEventHandler_ReadCompleted
            TriggerDocumentRead(DocumentReadOperationType.LoadTypesAndComments);
        }

        private void ComboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            // --- EDITED: Data reload is now via External Event ---
            TriggerDocumentRead(DocumentReadOperationType.LoadTypesAndComments);
            UpdateButtonStates();
        }

        // --- EDITED: LoadTypes is now part of DocumentReadEventHandler ---
        // private void LoadTypes() { /* Moved to DocumentReadEventHandler */ }
        // private void LoadTypeInstanceCounts<TType, TInst>(...) { /* Moved to DocumentReadEventHandler */ }
        // private void LoadTypeInstanceCounts<TType, TInst>(..., BuiltInCategory bic) { /* Moved to DocumentReadEventHandler */ }


        // --- MODIFICATION BUTTONS: NOW TRIGGERING DocumentOperationEvent ---

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!ValidateDocumentState()) return;

            var rows = dataGridViewTypes.SelectedRows.Cast<DataGridViewRow>().ToList();
            if (rows.Count == 0)
            {
                MessageBox.Show("Select at least one row to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool except = radioDeleteExceptSelected.Checked;
            string category = comboCategory.SelectedItem as string;

            // --- EDITED: Get data needed for the event handler ---
            // Data collection (which types to delete) is still done here as it uses grid data
            // but the actual deletion is in the event handler.
            var allTypes = GetAllTypesForCategoryInternal(category); // Use internal helper
            var toDeleteInfos = new List<Tuple<ElementId, string, string>>(); // (Id, TypeName, FamilyName)
            var selectedGridKeys = new HashSet<string>(rows.Select(r => r.Cells["colFamily"].Value + "|" + r.Cells["colTypeName"].Value));

            foreach (var t in allTypes)
            {
                string typeKey = GetElementTypeKeyInternal(t, category); // Use internal helper
                if (except ? !selectedGridKeys.Contains(typeKey) : selectedGridKeys.Contains(typeKey))
                {
                    toDeleteInfos.Add(Tuple.Create(t.Id, t.Name, GetElementTypeKeyInternal(t, category).Split('|')[0]));
                }
            }

            if (!toDeleteInfos.Any())
            {
                MessageBox.Show("No types found to delete based on current selection and mode.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // --- EDITED: Set event handler properties and raise event ---
            this.Visible = false; // Hide form before Revit interaction
            _documentOperationEventHandler.OperationType = DocumentOperationType.DeleteTypes;
            _documentOperationEventHandler.TypesToDelete = toDeleteInfos;
            _documentOperationEvent.Raise();
            // The rest will happen in DocumentOperationEventHandler_OperationCompleted
        }

        private void BtnDuplicateRename_Click(object sender, EventArgs e)
        {
            if (!ValidateDocumentState()) return;

            var selectedRows = dataGridViewTypes.SelectedRows.Cast<DataGridViewRow>().ToList();
            if (!selectedRows.Any())
            {
                MessageBox.Show("Select at least one type to duplicate.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string category = comboCategory.SelectedItem as string;
            var allCurrentTypes = GetAllTypesForCategoryInternal(category); // Use internal helper
            var selectedRevitTypeIds = new List<ElementId>();

            foreach (DataGridViewRow row in selectedRows)
            {
                string familyName = row.Cells["colFamily"].Value?.ToString();
                string typeName = row.Cells["colTypeName"].Value?.ToString();

                if (!string.IsNullOrEmpty(familyName) && !string.IsNullOrEmpty(typeName))
                {
                    var foundType = allCurrentTypes.FirstOrDefault(t => GetElementTypeKeyInternal(t, category) == (familyName + "|" + typeName)); // Use internal helper
                    if (foundType != null)
                    {
                        selectedRevitTypeIds.Add(foundType.Id);
                    }
                }
            }

            if (!selectedRevitTypeIds.Any())
            {
                MessageBox.Show("No valid types selected or found in the document for duplication.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- EDITED: Set event handler properties and raise event ---
            this.Visible = false;
            _documentOperationEventHandler.OperationType = DocumentOperationType.DuplicateRenameTypes;
            _documentOperationEventHandler.TypesToDuplicate = selectedRevitTypeIds;
            _documentOperationEvent.Raise();
        }

        private void BtnUpdateInstancesToQSIT_Click(object sender, EventArgs e)
        {
            if (!ValidateDocumentState()) return;

            string category = comboCategory.SelectedItem as string;

            var allCurrentTypes = GetAllTypesForCategoryInternal(category); // Use internal helper

            var typesMissingQSIT = new List<string>();
            var selectedOriginalTypeNamesInGrid = new HashSet<string>(
                dataGridViewTypes.SelectedRows.Cast<DataGridViewRow>()
                .Select(row => row.Cells["colTypeName"].Value?.ToString())
                .Where(s => !string.IsNullOrEmpty(s))
            );

            if (!selectedOriginalTypeNamesInGrid.Any())
            {
                MessageBox.Show("Please select types in the grid whose instances you want to update.", "No Types Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Pre-check for missing QSIT types (can remain here as it only reads grid data and existing types)
            foreach (string originalTypeName in selectedOriginalTypeNamesInGrid)
            {
                string qsitTypeName = QSIT_PREFIX + originalTypeName;
                var qsitTypeElement = allCurrentTypes.FirstOrDefault(t => t.Name == qsitTypeName);
                if (qsitTypeElement == null)
                {
                    typesMissingQSIT.Add(originalTypeName);
                }
            }

            if (typesMissingQSIT.Any())
            {
                string missingTypesMessage = string.Join(Environment.NewLine, typesMissingQSIT);
                MessageBox.Show(
                    $"The following selected types do not have a corresponding '{QSIT_PREFIX}' type in the document. Please duplicate them first:\n\n{missingTypesMessage}",
                    "Missing QSIT Types",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning
                );
                return;
            }

            // --- EDITED: Set event handler properties and raise event ---
            this.Visible = false;
            _documentOperationEventHandler.OperationType = DocumentOperationType.UpdateInstancesToQSIT;
            _documentOperationEventHandler.CategoryForUpdate = category;
            _documentOperationEventHandler.SelectedOriginalTypeNames = selectedOriginalTypeNamesInGrid;
            _documentOperationEvent.Raise();
        }

        private void BtnAssignCommentManual_Click(object sender, EventArgs e)
        {
            if (!ValidateDocumentState()) return;
            string comment = txtManualComment.Text;
            if (string.IsNullOrWhiteSpace(comment))
            {
                MessageBox.Show("Enter a comment.", "Assign Comment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedTypeKeys = GetSelectedTypeKeysFromGrid();

            if (!selectedTypeKeys.Any())
            {
                MessageBox.Show("Select at least one type in the grid to assign comments.", "Assign Comment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- EDITED: Set event handler properties and raise event ---
            this.Visible = false;
            _documentOperationEventHandler.OperationType = DocumentOperationType.AssignManualComment;
            _documentOperationEventHandler.CategoryForUpdate = comboCategory.SelectedItem as string;
            _documentOperationEventHandler.SelectedOriginalTypeNames = selectedTypeKeys; // Pass keys for instances
            _documentOperationEventHandler.ManualComment = comment;
            _documentOperationEvent.Raise();
        }

        private void BtnRandomizeComment_Click(object sender, EventArgs e)
        {
            if (!ValidateDocumentState()) return;

            var selectedTypeKeys = GetSelectedTypeKeysFromGrid();

            if (!selectedTypeKeys.Any())
            {
                MessageBox.Show("Select at least one type in the grid to assign comments.", "Assign Comment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- EDITED: Set event handler properties and raise event ---
            this.Visible = false;
            _documentOperationEventHandler.OperationType = DocumentOperationType.RandomizeComment;
            _documentOperationEventHandler.CategoryForUpdate = comboCategory.SelectedItem as string;
            _documentOperationEventHandler.SelectedOriginalTypeNames = selectedTypeKeys; // Pass keys for instances
            _documentOperationEvent.Raise();
        }

        // --- NEW: Helper to get selected type keys from the grid ---
        private HashSet<string> GetSelectedTypeKeysFromGrid()
        {
            return new HashSet<string>(
                dataGridViewTypes.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.Cells["colFamily"].Value?.ToString() + "|" + r.Cells["colTypeName"].Value?.ToString())
                .Where(s => !string.IsNullOrEmpty(s))
            );
        }

        // --- EDITED: The placement button already used an external event, this is fine ---
        private void BtnPlace_Click(object sender, EventArgs e)
        {
            if (!ValidateDocumentState()) return;

            string category = comboCategory.SelectedItem as string;
            if (category != "Doors" && category != "Windows")
            {
                MessageBox.Show("Placement is only supported for Doors and Windows.", "Placement Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridViewTypes.SelectedRows.Count != 1)
            {
                MessageBox.Show("Please select exactly one Door or Window type to place.", "Placement Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dataGridViewTypes.SelectedRows[0];
            string familyName = selectedRow.Cells["colFamily"].Value?.ToString();
            string typeName = selectedRow.Cells["colTypeName"].Value?.ToString();

            if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(typeName))
            {
                MessageBox.Show("Selected type information is incomplete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            FamilySymbol symbol = FindFamilySymbolInternal(familyName, typeName, category); // Use internal helper

            if (symbol == null)
            {
                MessageBox.Show($"Selected type '{typeName}' not found in the document.", "Placement Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                this.Visible = false;

                Reference hostFaceRef = _uiDoc.Selection.PickObject(
                    ObjectType.Face,
                    new HostWallOrCurtainPanelSelectionFilter(_doc, category),
                    "Select the host wall or curtain panel face for your door/window");

                Element hostElement = _doc.GetElement(hostFaceRef.ElementId);
                XYZ insertionPoint = hostFaceRef.GlobalPoint;

                _placeElementEventHandler.FamilySymbolId = symbol.Id;
                _placeElementEventHandler.HostReference = hostFaceRef;
                _placeElementEventHandler.InsertionPoint = insertionPoint;

                _placeElementEvent.Raise();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                Log("Placement cancelled by user.");
                MessageBox.Show("Placement cancelled.", "Placement", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Visible = true;
                this.Activate();
                // --- EDITED: Trigger a Read Event to refresh UI after cancel ---
                TriggerDocumentRead(DocumentReadOperationType.LoadTypesAndComments);
                dataGridViewTypes.Focus();
            }
            catch (Exception ex)
            {
                Log($"Error during picking: {ex.Message}");
                MessageBox.Show($"An error occurred during picking: {ex.Message}", "Picking Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Visible = true;
                this.Activate();
                // --- EDITED: Trigger a Read Event to refresh UI after error ---
                TriggerDocumentRead(DocumentReadOperationType.LoadTypesAndComments);
                dataGridViewTypes.Focus();
            }
        }

        // --- EDITED / NEW: INTERNAL helper methods that access Document data ---
        // These are *only* called from the MainForm to gather input for External Events.
        // They should NOT start transactions. They are for light-weight pre-checks or UI data prep.
        // If they cause an API context error, they would need to be moved into the External Event too.
        // For now, they are kept here for brevity and assume they only perform allowed reads.

        /// <summary>
        /// Generates a random unique comment string (e.g., "A123").
        /// This is for MainForm's internal use / displaying only, not directly for document modification.
        /// Actual comment assignment must happen in the External Event.
        /// </summary>
        private string GenerateRandomCommentInternal() // Renamed to avoid confusion
        {
            string comment;
            do
            {
                comment = ((char)('A' + _random.Next(26))) + _random.Next(1, 1000).ToString();
            }
            while (_usedComments.Contains(comment) && _usedComments.Count < (26 * 999));

            _usedComments.Add(comment);
            return comment;
        }

        /// <summary>
        /// Finds a FamilySymbol (type) by its family name, type name, and built-in category.
        /// For internal MainForm use, not starting transactions.
        /// </summary>
        private FamilySymbol FindFamilySymbolInternal(string familyName, string typeName, string category) // Renamed
        {
            BuiltInCategory bic = BuiltInCategory.INVALID;
            if (category == "Doors") bic = BuiltInCategory.OST_Doors;
            else if (category == "Windows") bic = BuiltInCategory.OST_Windows;
            else return null;

            // This is a read operation, typically allowed without a transaction in a modeless dialog
            // as long as it's not during a Revit command, but can be problematic if it triggers
            // heavy document access/lazy loading. For now, it's ok here.
            return new FilteredElementCollector(_doc)
                   .OfClass(typeof(FamilySymbol))
                   .OfCategory(bic)
                   .Cast<FamilySymbol>()
                   .FirstOrDefault(s => s.Family.Name == familyName && s.Name == typeName);
        }

        private void DataGridViewTypes_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasSel = dataGridViewTypes.SelectedRows.Count > 0;
            btnDelete.Enabled = hasSel;
            btnDuplicateRename.Enabled = hasSel;
            btnUpdateInstancesToQSIT.Enabled = hasSel;
            btnAssignCommentManual.Enabled = hasSel;
            btnRandomizeComment.Enabled = hasSel;

            bool isDoorOrWindow = comboCategory.SelectedItem is string cat
                                  && (cat == "Doors" || cat == "Windows");
            bool singleTypeSelected = (dataGridViewTypes.SelectedRows.Count == 1);

            btnPlace.Enabled = isDoorOrWindow && singleTypeSelected;
        }

        private bool ValidateDocumentState()
        {
            if (_doc == null)
            {
                MessageBox.Show("Revit Document is not available. Please open a project.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (_doc.IsReadOnly)
            {
                MessageBox.Show("The current Revit Document is read-only. Modifications are not allowed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves all ElementType objects for a given category.
        /// For internal MainForm use (e.g. for pre-checks before triggering event).
        /// </summary>
        private List<ElementType> GetAllTypesForCategoryInternal(string category) // Renamed
        {
            var collector = new FilteredElementCollector(_doc);
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

        /// <summary>
        /// Retrieves the family name and type name for a given element type.
        /// For internal MainForm use (e.g. for pre-checks).
        /// </summary>
        private string GetElementTypeKeyInternal(ElementType type, string category) // Renamed
        {
            string familyName = type.FamilyName;
            if (category == "Walls" || category == "Floors" || category == "Ceilings")
            {
                familyName = type.Category?.Name ?? type.FamilyName;
            }
            return familyName + "|" + type.Name;
        }

        private void Log(string text)
        {
            if (richTextBoxLog.InvokeRequired)
            {
                richTextBoxLog.Invoke(new Action(() => Log(text)));
            }
            else
            {
                richTextBoxLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
                richTextBoxLog.ScrollToCaret();
            }
        }

        private void dataGridViewTypes_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }

    // HostWallOrCurtainPanelSelectionFilter remains unchanged from previous version
    // It's already correctly receiving the Document in its constructor.
    public class HostWallOrCurtainPanelSelectionFilter : ISelectionFilter
    {
        private string _category;
        private Document _document;

        public HostWallOrCurtainPanelSelectionFilter(Document doc, string category)
        {
            _document = doc;
            _category = category;
        }

        public bool AllowElement(Element elem)
        {
            if (elem is Wall)
            {
                return true;
            }
            if (_category == "Windows" && elem.Category != null && elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CurtainWallPanels)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            if (refer.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_SURFACE)
            {
                Element hostCandidate = _document.GetElement(refer.ElementId);
                return AllowElement(hostCandidate);
            }
            return false;
        }
    }
}