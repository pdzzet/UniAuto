namespace UniOPI
{
    partial class FormRobotMethodDef
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpRobotMethodDef = new System.Windows.Forms.TableLayoutPanel();
            this.pnlRobotView = new System.Windows.Forms.Panel();
            this.pnlRuleType = new System.Windows.Forms.Panel();
            this.cboMetodRuleType = new System.Windows.Forms.ComboBox();
            this.lblRuleType = new System.Windows.Forms.Label();
            this.pnlSideBarBtn = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnModify = new System.Windows.Forms.Button();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colObjectKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colMethodRuleType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMethodName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFuncKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReMarks = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAuthor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastUpdateDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpRobotMethodDef.SuspendLayout();
            this.pnlRobotView.SuspendLayout();
            this.pnlRuleType.SuspendLayout();
            this.pnlSideBarBtn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1200, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpRobotMethodDef);
            this.spcBase.Size = new System.Drawing.Size(1260, 522);
            // 
            // tlpRobotMethodDef
            // 
            this.tlpRobotMethodDef.ColumnCount = 2;
            this.tlpRobotMethodDef.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRobotMethodDef.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tlpRobotMethodDef.Controls.Add(this.pnlRobotView, 0, 0);
            this.tlpRobotMethodDef.Controls.Add(this.pnlSideBarBtn, 0, 1);
            this.tlpRobotMethodDef.Controls.Add(this.dgvData, 0, 1);
            this.tlpRobotMethodDef.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRobotMethodDef.Location = new System.Drawing.Point(0, 0);
            this.tlpRobotMethodDef.Name = "tlpRobotMethodDef";
            this.tlpRobotMethodDef.RowCount = 2;
            this.tlpRobotMethodDef.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tlpRobotMethodDef.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRobotMethodDef.Size = new System.Drawing.Size(1260, 491);
            this.tlpRobotMethodDef.TabIndex = 0;
            // 
            // pnlRobotView
            // 
            this.pnlRobotView.Controls.Add(this.pnlRuleType);
            this.pnlRobotView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRobotView.Location = new System.Drawing.Point(3, 3);
            this.pnlRobotView.Name = "pnlRobotView";
            this.pnlRobotView.Size = new System.Drawing.Size(1124, 54);
            this.pnlRobotView.TabIndex = 15;
            // 
            // pnlRuleType
            // 
            this.pnlRuleType.Controls.Add(this.cboMetodRuleType);
            this.pnlRuleType.Controls.Add(this.lblRuleType);
            this.pnlRuleType.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlRuleType.Location = new System.Drawing.Point(745, 0);
            this.pnlRuleType.Name = "pnlRuleType";
            this.pnlRuleType.Size = new System.Drawing.Size(379, 54);
            this.pnlRuleType.TabIndex = 1;
            // 
            // cboMetodRuleType
            // 
            this.cboMetodRuleType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMetodRuleType.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboMetodRuleType.FormattingEnabled = true;
            this.cboMetodRuleType.Items.AddRange(new object[] {
            " ",
            "Select",
            "Filter",
            "OrderBy",
            "RouteCondition",
            "ResultAction",
            "StageSelect",
            "RouteStepByPass",
            "RouteStepJump"});
            this.cboMetodRuleType.Location = new System.Drawing.Point(151, 10);
            this.cboMetodRuleType.Name = "cboMetodRuleType";
            this.cboMetodRuleType.Size = new System.Drawing.Size(225, 29);
            this.cboMetodRuleType.TabIndex = 1;
            this.cboMetodRuleType.SelectedIndexChanged += new System.EventHandler(this.cboMetodRuleType_SelectedIndexChanged);
            // 
            // lblRuleType
            // 
            this.lblRuleType.AutoSize = true;
            this.lblRuleType.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRuleType.Location = new System.Drawing.Point(3, 13);
            this.lblRuleType.Name = "lblRuleType";
            this.lblRuleType.Size = new System.Drawing.Size(142, 22);
            this.lblRuleType.TabIndex = 0;
            this.lblRuleType.Text = "Method Rule Type";
            // 
            // pnlSideBarBtn
            // 
            this.pnlSideBarBtn.Controls.Add(this.btnSave);
            this.pnlSideBarBtn.Controls.Add(this.btnAdd);
            this.pnlSideBarBtn.Controls.Add(this.btnRefresh);
            this.pnlSideBarBtn.Controls.Add(this.btnDelete);
            this.pnlSideBarBtn.Controls.Add(this.btnModify);
            this.pnlSideBarBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSideBarBtn.Location = new System.Drawing.Point(1133, 63);
            this.pnlSideBarBtn.Name = "pnlSideBarBtn";
            this.pnlSideBarBtn.Size = new System.Drawing.Size(124, 425);
            this.pnlSideBarBtn.TabIndex = 14;
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(4, 159);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(113, 50);
            this.btnSave.TabIndex = 12;
            this.btnSave.Tag = "Save";
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAdd.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(4, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(113, 50);
            this.btnAdd.TabIndex = 8;
            this.btnAdd.Tag = "Add";
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(4, 211);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(113, 50);
            this.btnRefresh.TabIndex = 11;
            this.btnRefresh.Tag = "Refresh";
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(4, 107);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(113, 50);
            this.btnDelete.TabIndex = 9;
            this.btnDelete.Tag = "Delete";
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnModify
            // 
            this.btnModify.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnModify.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModify.Location = new System.Drawing.Point(4, 55);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(113, 50);
            this.btnModify.TabIndex = 10;
            this.btnModify.Tag = "Modify";
            this.btnModify.Text = "Modify";
            this.btnModify.UseVisualStyleBackColor = true;
            this.btnModify.Click += new System.EventHandler(this.btn_Click);
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colObjectKey,
            this.colIsEnabled,
            this.colMethodRuleType,
            this.colDescription,
            this.colObjectName,
            this.colMethodName,
            this.colFuncKey,
            this.colReMarks,
            this.colAuthor,
            this.colLastUpdateDate});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 63);
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1124, 425);
            this.dgvData.TabIndex = 13;
            this.dgvData.DataSourceChanged += new System.EventHandler(this.dgvData_DataSourceChanged);
            this.dgvData.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvData_ColumnHeaderMouseClick);
            // 
            // colObjectKey
            // 
            this.colObjectKey.DataPropertyName = "OBJECTKEY";
            this.colObjectKey.HeaderText = "OBJECT KEY";
            this.colObjectKey.Name = "colObjectKey";
            this.colObjectKey.ReadOnly = true;
            this.colObjectKey.Visible = false;
            // 
            // colIsEnabled
            // 
            this.colIsEnabled.DataPropertyName = "ISENABLED";
            this.colIsEnabled.FalseValue = "N";
            this.colIsEnabled.HeaderText = "Enabled";
            this.colIsEnabled.Name = "colIsEnabled";
            this.colIsEnabled.ReadOnly = true;
            this.colIsEnabled.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIsEnabled.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colIsEnabled.TrueValue = "Y";
            this.colIsEnabled.Width = 80;
            // 
            // colMethodRuleType
            // 
            this.colMethodRuleType.DataPropertyName = "METHODRULETYPE";
            this.colMethodRuleType.HeaderText = "Rule Type";
            this.colMethodRuleType.Name = "colMethodRuleType";
            this.colMethodRuleType.ReadOnly = true;
            this.colMethodRuleType.Width = 140;
            // 
            // colDescription
            // 
            this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDescription.DataPropertyName = "DESCRIPTION";
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            this.colDescription.Width = 400;
            // 
            // colObjectName
            // 
            this.colObjectName.DataPropertyName = "OBJECTNAME";
            this.colObjectName.HeaderText = "Object Name";
            this.colObjectName.Name = "colObjectName";
            this.colObjectName.ReadOnly = true;
            this.colObjectName.Width = 250;
            // 
            // colMethodName
            // 
            this.colMethodName.DataPropertyName = "METHODNAME";
            this.colMethodName.HeaderText = "Method Name";
            this.colMethodName.Name = "colMethodName";
            this.colMethodName.ReadOnly = true;
            this.colMethodName.Width = 300;
            // 
            // colFuncKey
            // 
            this.colFuncKey.DataPropertyName = "FUNCKEY";
            this.colFuncKey.HeaderText = "Function Key";
            this.colFuncKey.Name = "colFuncKey";
            this.colFuncKey.ReadOnly = true;
            this.colFuncKey.Width = 200;
            // 
            // colReMarks
            // 
            this.colReMarks.DataPropertyName = "REMARKS";
            this.colReMarks.HeaderText = "Remarks";
            this.colReMarks.Name = "colReMarks";
            this.colReMarks.ReadOnly = true;
            this.colReMarks.Width = 200;
            // 
            // colAuthor
            // 
            this.colAuthor.DataPropertyName = "AUTHOR";
            this.colAuthor.HeaderText = "Author";
            this.colAuthor.Name = "colAuthor";
            this.colAuthor.ReadOnly = true;
            this.colAuthor.Width = 200;
            // 
            // colLastUpdateDate
            // 
            this.colLastUpdateDate.DataPropertyName = "LASTUPDATEDATE";
            this.colLastUpdateDate.HeaderText = "Last Update Date";
            this.colLastUpdateDate.Name = "colLastUpdateDate";
            this.colLastUpdateDate.ReadOnly = true;
            this.colLastUpdateDate.Width = 200;
            // 
            // FormRobotMethodDef
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1260, 522);
            this.Name = "FormRobotMethodDef";
            this.Text = "FormRobotMethodDef";
            this.Load += new System.EventHandler(this.FormRobotMethodDef_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpRobotMethodDef.ResumeLayout(false);
            this.pnlRobotView.ResumeLayout(false);
            this.pnlRuleType.ResumeLayout(false);
            this.pnlRuleType.PerformLayout();
            this.pnlSideBarBtn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpRobotMethodDef;
        private System.Windows.Forms.DataGridView dgvData;
        protected System.Windows.Forms.Panel pnlSideBarBtn;
        protected System.Windows.Forms.Button btnSave;
        protected System.Windows.Forms.Button btnAdd;
        protected System.Windows.Forms.Button btnRefresh;
        protected System.Windows.Forms.Button btnDelete;
        protected System.Windows.Forms.Button btnModify;
        private System.Windows.Forms.Panel pnlRobotView;
        private System.Windows.Forms.Panel pnlRuleType;
        private System.Windows.Forms.Label lblRuleType;
        private System.Windows.Forms.ComboBox cboMetodRuleType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectKey;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMethodRuleType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMethodName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFuncKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReMarks;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAuthor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastUpdateDate;
    }
}