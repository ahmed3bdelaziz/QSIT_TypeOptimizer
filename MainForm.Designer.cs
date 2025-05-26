namespace QSIT_TypeOptimizer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.labelCategory = new System.Windows.Forms.Label();
            this.comboCategory = new System.Windows.Forms.ComboBox();
            this.dataGridViewTypes = new System.Windows.Forms.DataGridView();
            this.colFamily = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTypeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colInstanceCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupDelete = new System.Windows.Forms.GroupBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.radioDeleteExceptSelected = new System.Windows.Forms.RadioButton();
            this.radioDeleteSelected = new System.Windows.Forms.RadioButton();
            this.groupDuplicate = new System.Windows.Forms.GroupBox();
            this.labelQSITStatus = new System.Windows.Forms.Label();
            this.btnUpdateInstancesToQSIT = new System.Windows.Forms.Button();
            this.btnDuplicateRename = new System.Windows.Forms.Button();
            this.groupComments = new System.Windows.Forms.GroupBox();
            this.btnRandomizeComment = new System.Windows.Forms.Button();
            this.btnAssignCommentManual = new System.Windows.Forms.Button();
            this.txtManualComment = new System.Windows.Forms.TextBox();
            this.labelManualComment = new System.Windows.Forms.Label();
            this.groupPlace = new System.Windows.Forms.GroupBox();
            this.labelPlaceInfo = new System.Windows.Forms.Label();
            this.btnPlace = new System.Windows.Forms.Button();
            this.groupLog = new System.Windows.Forms.GroupBox();
            this.richTextBoxLog = new System.Windows.Forms.RichTextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTypes)).BeginInit();
            this.groupDelete.SuspendLayout();
            this.groupDuplicate.SuspendLayout();
            this.groupComments.SuspendLayout();
            this.groupPlace.SuspendLayout();
            this.groupLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelCategory
            // 
            this.labelCategory.AutoSize = true;
            this.labelCategory.Location = new System.Drawing.Point(20, 20);
            this.labelCategory.Name = "labelCategory";
            this.labelCategory.Size = new System.Drawing.Size(52, 13);
            this.labelCategory.TabIndex = 0;
            this.labelCategory.Text = "Category:";
            // 
            // comboCategory
            // 
            this.comboCategory.FormattingEnabled = true;
            this.comboCategory.Items.AddRange(new object[] {
            "Walls",
            "Floor",
            "Ceilings",
            "Doors",
            "Windows"});
            this.comboCategory.Location = new System.Drawing.Point(100, 17);
            this.comboCategory.Name = "comboCategory";
            this.comboCategory.Size = new System.Drawing.Size(150, 21);
            this.comboCategory.TabIndex = 1;
            // 
            // dataGridViewTypes
            // 
            this.dataGridViewTypes.AllowUserToAddRows = false;
            this.dataGridViewTypes.AllowUserToDeleteRows = false;
            this.dataGridViewTypes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colFamily,
            this.colTypeName,
            this.colInstanceCount});
            this.dataGridViewTypes.Location = new System.Drawing.Point(20, 55);
            this.dataGridViewTypes.Name = "dataGridViewTypes";
            this.dataGridViewTypes.ReadOnly = true;
            this.dataGridViewTypes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewTypes.Size = new System.Drawing.Size(700, 180);
            this.dataGridViewTypes.TabIndex = 2;
            this.dataGridViewTypes.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewTypes_CellContentClick);
            // 
            // colFamily
            // 
            this.colFamily.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colFamily.HeaderText = "Family";
            this.colFamily.MinimumWidth = 120;
            this.colFamily.Name = "colFamily";
            this.colFamily.ReadOnly = true;
            this.colFamily.Width = 120;
            // 
            // colTypeName
            // 
            this.colTypeName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colTypeName.HeaderText = "Type Name\n";
            this.colTypeName.MinimumWidth = 140;
            this.colTypeName.Name = "colTypeName";
            this.colTypeName.ReadOnly = true;
            this.colTypeName.Width = 140;
            // 
            // colInstanceCount
            // 
            this.colInstanceCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colInstanceCount.HeaderText = "Instance Count\n";
            this.colInstanceCount.MinimumWidth = 110;
            this.colInstanceCount.Name = "colInstanceCount";
            this.colInstanceCount.ReadOnly = true;
            this.colInstanceCount.Width = 110;
            // 
            // groupDelete
            // 
            this.groupDelete.Controls.Add(this.btnDelete);
            this.groupDelete.Controls.Add(this.radioDeleteExceptSelected);
            this.groupDelete.Controls.Add(this.radioDeleteSelected);
            this.groupDelete.Location = new System.Drawing.Point(20, 250);
            this.groupDelete.Name = "groupDelete";
            this.groupDelete.Size = new System.Drawing.Size(700, 65);
            this.groupDelete.TabIndex = 3;
            this.groupDelete.TabStop = false;
            this.groupDelete.Text = "Delete";
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(540, 23);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(100, 27);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // radioDeleteExceptSelected
            // 
            this.radioDeleteExceptSelected.AutoSize = true;
            this.radioDeleteExceptSelected.Location = new System.Drawing.Point(211, 25);
            this.radioDeleteExceptSelected.Name = "radioDeleteExceptSelected";
            this.radioDeleteExceptSelected.Size = new System.Drawing.Size(175, 17);
            this.radioDeleteExceptSelected.TabIndex = 1;
            this.radioDeleteExceptSelected.TabStop = true;
            this.radioDeleteExceptSelected.Text = "Delete all types except selected";
            this.radioDeleteExceptSelected.UseVisualStyleBackColor = true;
            // 
            // radioDeleteSelected
            // 
            this.radioDeleteSelected.AutoSize = true;
            this.radioDeleteSelected.Checked = true;
            this.radioDeleteSelected.Location = new System.Drawing.Point(15, 25);
            this.radioDeleteSelected.Name = "radioDeleteSelected";
            this.radioDeleteSelected.Size = new System.Drawing.Size(155, 17);
            this.radioDeleteSelected.TabIndex = 0;
            this.radioDeleteSelected.TabStop = true;
            this.radioDeleteSelected.Text = "Delete selected type(s) only";
            this.radioDeleteSelected.UseVisualStyleBackColor = true;
            // 
            // groupDuplicate
            // 
            this.groupDuplicate.Controls.Add(this.labelQSITStatus);
            this.groupDuplicate.Controls.Add(this.btnUpdateInstancesToQSIT);
            this.groupDuplicate.Controls.Add(this.btnDuplicateRename);
            this.groupDuplicate.Location = new System.Drawing.Point(20, 325);
            this.groupDuplicate.Name = "groupDuplicate";
            this.groupDuplicate.Size = new System.Drawing.Size(700, 85);
            this.groupDuplicate.TabIndex = 4;
            this.groupDuplicate.TabStop = false;
            this.groupDuplicate.Text = "Duplicate / Rename";
            // 
            // labelQSITStatus
            // 
            this.labelQSITStatus.AutoSize = true;
            this.labelQSITStatus.ForeColor = System.Drawing.Color.Red;
            this.labelQSITStatus.Location = new System.Drawing.Point(12, 60);
            this.labelQSITStatus.Name = "labelQSITStatus";
            this.labelQSITStatus.Size = new System.Drawing.Size(0, 13);
            this.labelQSITStatus.TabIndex = 2;
            // 
            // btnUpdateInstancesToQSIT
            // 
            this.btnUpdateInstancesToQSIT.Enabled = false;
            this.btnUpdateInstancesToQSIT.Location = new System.Drawing.Point(260, 25);
            this.btnUpdateInstancesToQSIT.Name = "btnUpdateInstancesToQSIT";
            this.btnUpdateInstancesToQSIT.Size = new System.Drawing.Size(330, 30);
            this.btnUpdateInstancesToQSIT.TabIndex = 1;
            this.btnUpdateInstancesToQSIT.Text = "Update Instances to QSIT Type and Randomize Comments";
            this.btnUpdateInstancesToQSIT.UseVisualStyleBackColor = true;
            // 
            // btnDuplicateRename
            // 
            this.btnDuplicateRename.Location = new System.Drawing.Point(15, 25);
            this.btnDuplicateRename.Name = "btnDuplicateRename";
            this.btnDuplicateRename.Size = new System.Drawing.Size(220, 30);
            this.btnDuplicateRename.TabIndex = 0;
            this.btnDuplicateRename.Text = "Duplicate and Rename as QSIT";
            this.btnDuplicateRename.UseVisualStyleBackColor = true;
            // 
            // groupComments
            // 
            this.groupComments.Controls.Add(this.btnRandomizeComment);
            this.groupComments.Controls.Add(this.btnAssignCommentManual);
            this.groupComments.Controls.Add(this.txtManualComment);
            this.groupComments.Controls.Add(this.labelManualComment);
            this.groupComments.Location = new System.Drawing.Point(20, 420);
            this.groupComments.Name = "groupComments";
            this.groupComments.Size = new System.Drawing.Size(700, 75);
            this.groupComments.TabIndex = 5;
            this.groupComments.TabStop = false;
            this.groupComments.Text = "Comments";
            // 
            // btnRandomizeComment
            // 
            this.btnRandomizeComment.Location = new System.Drawing.Point(420, 25);
            this.btnRandomizeComment.Name = "btnRandomizeComment";
            this.btnRandomizeComment.Size = new System.Drawing.Size(220, 27);
            this.btnRandomizeComment.TabIndex = 3;
            this.btnRandomizeComment.Text = "Assign Random Unique Comments\n\n";
            this.btnRandomizeComment.UseVisualStyleBackColor = true;
            // 
            // btnAssignCommentManual
            // 
            this.btnAssignCommentManual.Location = new System.Drawing.Point(310, 25);
            this.btnAssignCommentManual.Name = "btnAssignCommentManual";
            this.btnAssignCommentManual.Size = new System.Drawing.Size(100, 27);
            this.btnAssignCommentManual.TabIndex = 2;
            this.btnAssignCommentManual.Text = "Assign Manual";
            this.btnAssignCommentManual.UseVisualStyleBackColor = true;
            // 
            // txtManualComment
            // 
            this.txtManualComment.Location = new System.Drawing.Point(130, 27);
            this.txtManualComment.Name = "txtManualComment";
            this.txtManualComment.Size = new System.Drawing.Size(160, 20);
            this.txtManualComment.TabIndex = 1;
            // 
            // labelManualComment
            // 
            this.labelManualComment.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.labelManualComment.AutoSize = true;
            this.labelManualComment.Location = new System.Drawing.Point(15, 30);
            this.labelManualComment.Name = "labelManualComment";
            this.labelManualComment.Size = new System.Drawing.Size(92, 26);
            this.labelManualComment.TabIndex = 0;
            this.labelManualComment.Text = "Manual Comment:\n\n";
            // 
            // groupPlace
            // 
            this.groupPlace.Controls.Add(this.labelPlaceInfo);
            this.groupPlace.Controls.Add(this.btnPlace);
            this.groupPlace.Location = new System.Drawing.Point(20, 505);
            this.groupPlace.Name = "groupPlace";
            this.groupPlace.Size = new System.Drawing.Size(700, 60);
            this.groupPlace.TabIndex = 6;
            this.groupPlace.TabStop = false;
            this.groupPlace.Text = "Place Door/Window Instance";
            // 
            // labelPlaceInfo
            // 
            this.labelPlaceInfo.AutoSize = true;
            this.labelPlaceInfo.Location = new System.Drawing.Point(127, 29);
            this.labelPlaceInfo.Name = "labelPlaceInfo";
            this.labelPlaceInfo.Size = new System.Drawing.Size(264, 13);
            this.labelPlaceInfo.TabIndex = 1;
            this.labelPlaceInfo.Text = "Each placed instance gets a unique random comment.";
            // 
            // btnPlace
            // 
            this.btnPlace.Enabled = false;
            this.btnPlace.Location = new System.Drawing.Point(15, 22);
            this.btnPlace.Name = "btnPlace";
            this.btnPlace.Size = new System.Drawing.Size(100, 27);
            this.btnPlace.TabIndex = 0;
            this.btnPlace.Text = "Place";
            this.btnPlace.UseVisualStyleBackColor = true;
            // 
            // groupLog
            // 
            this.groupLog.Controls.Add(this.richTextBoxLog);
            this.groupLog.Location = new System.Drawing.Point(20, 575);
            this.groupLog.Name = "groupLog";
            this.groupLog.Size = new System.Drawing.Size(700, 110);
            this.groupLog.TabIndex = 7;
            this.groupLog.TabStop = false;
            this.groupLog.Text = "Log/Output\n\n";
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.Location = new System.Drawing.Point(10, 25);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.ReadOnly = true;
            this.richTextBoxLog.Size = new System.Drawing.Size(670, 75);
            this.richTextBoxLog.TabIndex = 0;
            this.richTextBoxLog.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 691);
            this.Controls.Add(this.groupLog);
            this.Controls.Add(this.groupPlace);
            this.Controls.Add(this.groupComments);
            this.Controls.Add(this.groupDuplicate);
            this.Controls.Add(this.groupDelete);
            this.Controls.Add(this.dataGridViewTypes);
            this.Controls.Add(this.comboCategory);
            this.Controls.Add(this.labelCategory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QSIT Type Optimizer";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTypes)).EndInit();
            this.groupDelete.ResumeLayout(false);
            this.groupDelete.PerformLayout();
            this.groupDuplicate.ResumeLayout(false);
            this.groupDuplicate.PerformLayout();
            this.groupComments.ResumeLayout(false);
            this.groupComments.PerformLayout();
            this.groupPlace.ResumeLayout(false);
            this.groupPlace.PerformLayout();
            this.groupLog.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelCategory;
        private System.Windows.Forms.ComboBox comboCategory;
        private System.Windows.Forms.DataGridView dataGridViewTypes;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFamily;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTypeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colInstanceCount;
        private System.Windows.Forms.GroupBox groupDelete;
        private System.Windows.Forms.RadioButton radioDeleteSelected;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.RadioButton radioDeleteExceptSelected;
        private System.Windows.Forms.GroupBox groupDuplicate;
        private System.Windows.Forms.Label labelQSITStatus;
        private System.Windows.Forms.Button btnUpdateInstancesToQSIT;
        private System.Windows.Forms.Button btnDuplicateRename;
        private System.Windows.Forms.GroupBox groupComments;
        private System.Windows.Forms.Button btnRandomizeComment;
        private System.Windows.Forms.Button btnAssignCommentManual;
        private System.Windows.Forms.TextBox txtManualComment;
        private System.Windows.Forms.Label labelManualComment;
        private System.Windows.Forms.GroupBox groupPlace;
        private System.Windows.Forms.Label labelPlaceInfo;
        private System.Windows.Forms.Button btnPlace;
        private System.Windows.Forms.GroupBox groupLog;
        private System.Windows.Forms.RichTextBox richTextBoxLog;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}