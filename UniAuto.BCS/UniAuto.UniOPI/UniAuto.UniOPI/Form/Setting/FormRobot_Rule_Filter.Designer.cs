namespace UniOPI
{
    partial class FormRobot_Rule_Filter
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
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.pnlStepView = new System.Windows.Forms.Panel();
            this.lblStepID = new System.Windows.Forms.Label();
            this.cboStepID = new System.Windows.Forms.ComboBox();
            this.txtRouteID = new System.Windows.Forms.TextBox();
            this.lblRouteID = new System.Windows.Forms.Label();
            this.txtRobotName = new System.Windows.Forms.TextBox();
            this.lblRobotName = new System.Windows.Forms.Label();
            this.colIsEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRouteID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStepID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMethodName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFuncKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemarks = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastUpdateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.flpButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.pnlStepView.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(930, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(990, 568);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 2;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 102F));
            this.tlpBase.Controls.Add(this.flpButtons, 1, 1);
            this.tlpBase.Controls.Add(this.dgvData, 0, 1);
            this.tlpBase.Controls.Add(this.pnlStepView, 0, 0);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Size = new System.Drawing.Size(990, 537);
            this.tlpBase.TabIndex = 0;
            // 
            // flpButtons
            // 
            this.flpButtons.Controls.Add(this.btnAdd);
            this.flpButtons.Controls.Add(this.btnEdit);
            this.flpButtons.Controls.Add(this.btnDelete);
            this.flpButtons.Controls.Add(this.btnSave);
            this.flpButtons.Controls.Add(this.btnRefresh);
            this.flpButtons.Controls.Add(this.btnExit);
            this.flpButtons.Location = new System.Drawing.Point(891, 63);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(96, 471);
            this.flpButtons.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnAdd.Location = new System.Drawing.Point(1, 1);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(1);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(94, 45);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Tag = "Add";
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnEdit.Location = new System.Drawing.Point(1, 48);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(1);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(94, 45);
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Tag = "Modify";
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnDelete.Location = new System.Drawing.Point(1, 95);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(1);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(94, 45);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Tag = "Delete";
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnSave.Location = new System.Drawing.Point(1, 142);
            this.btnSave.Margin = new System.Windows.Forms.Padding(1);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(94, 45);
            this.btnSave.TabIndex = 5;
            this.btnSave.Tag = "Save";
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnRefresh.Location = new System.Drawing.Point(1, 189);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(1);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(94, 45);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Tag = "Refresh";
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnExit
            // 
            this.btnExit.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnExit.Location = new System.Drawing.Point(1, 236);
            this.btnExit.Margin = new System.Windows.Forms.Padding(1);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(94, 45);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colIsEnabled,
            this.colServerName,
            this.colRobotName,
            this.colRouteID,
            this.colStepID,
            this.colItemID,
            this.colItemSeq,
            this.colDescription,
            this.colObjectName,
            this.colMethodName,
            this.colFuncKey,
            this.colRemarks,
            this.colLastUpdateTime});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 63);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 12F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.RowHeadersWidth = 5;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12F);
            this.dgvData.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(882, 471);
            this.dgvData.TabIndex = 2;
            this.dgvData.DataSourceChanged += new System.EventHandler(this.dgvData_DataSourceChanged);
            this.dgvData.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvData_ColumnHeaderMouseClick);
            // 
            // pnlStepView
            // 
            this.pnlStepView.Controls.Add(this.lblStepID);
            this.pnlStepView.Controls.Add(this.cboStepID);
            this.pnlStepView.Controls.Add(this.txtRouteID);
            this.pnlStepView.Controls.Add(this.lblRouteID);
            this.pnlStepView.Controls.Add(this.txtRobotName);
            this.pnlStepView.Controls.Add(this.lblRobotName);
            this.pnlStepView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStepView.Location = new System.Drawing.Point(3, 3);
            this.pnlStepView.Name = "pnlStepView";
            this.pnlStepView.Size = new System.Drawing.Size(882, 54);
            this.pnlStepView.TabIndex = 3;
            // 
            // lblStepID
            // 
            this.lblStepID.AutoSize = true;
            this.lblStepID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStepID.Location = new System.Drawing.Point(483, 17);
            this.lblStepID.Name = "lblStepID";
            this.lblStepID.Size = new System.Drawing.Size(62, 22);
            this.lblStepID.TabIndex = 5;
            this.lblStepID.Text = "Step ID";
            // 
            // cboStepID
            // 
            this.cboStepID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStepID.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboStepID.FormattingEnabled = true;
            this.cboStepID.Location = new System.Drawing.Point(546, 14);
            this.cboStepID.Name = "cboStepID";
            this.cboStepID.Size = new System.Drawing.Size(130, 31);
            this.cboStepID.TabIndex = 4;
            // 
            // txtRouteID
            // 
            this.txtRouteID.BackColor = System.Drawing.Color.White;
            this.txtRouteID.Font = new System.Drawing.Font("Calibri", 13F);
            this.txtRouteID.Location = new System.Drawing.Point(332, 14);
            this.txtRouteID.Name = "txtRouteID";
            this.txtRouteID.ReadOnly = true;
            this.txtRouteID.Size = new System.Drawing.Size(132, 29);
            this.txtRouteID.TabIndex = 3;
            // 
            // lblRouteID
            // 
            this.lblRouteID.AutoSize = true;
            this.lblRouteID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRouteID.Location = new System.Drawing.Point(256, 17);
            this.lblRouteID.Name = "lblRouteID";
            this.lblRouteID.Size = new System.Drawing.Size(74, 22);
            this.lblRouteID.TabIndex = 2;
            this.lblRouteID.Text = "Route ID";
            // 
            // txtRobotName
            // 
            this.txtRobotName.BackColor = System.Drawing.Color.White;
            this.txtRobotName.Font = new System.Drawing.Font("Calibri", 13F);
            this.txtRobotName.Location = new System.Drawing.Point(112, 14);
            this.txtRobotName.Name = "txtRobotName";
            this.txtRobotName.ReadOnly = true;
            this.txtRobotName.Size = new System.Drawing.Size(132, 29);
            this.txtRobotName.TabIndex = 1;
            // 
            // lblRobotName
            // 
            this.lblRobotName.AutoSize = true;
            this.lblRobotName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRobotName.Location = new System.Drawing.Point(9, 17);
            this.lblRobotName.Name = "lblRobotName";
            this.lblRobotName.Size = new System.Drawing.Size(103, 22);
            this.lblRobotName.TabIndex = 0;
            this.lblRobotName.Text = "Robot Name";
            // 
            // colIsEnabled
            // 
            this.colIsEnabled.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colIsEnabled.DataPropertyName = "ISENABLED";
            this.colIsEnabled.FalseValue = "N";
            this.colIsEnabled.HeaderText = "Enable";
            this.colIsEnabled.Name = "colIsEnabled";
            this.colIsEnabled.ReadOnly = true;
            this.colIsEnabled.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIsEnabled.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colIsEnabled.TrueValue = "Y";
            this.colIsEnabled.Width = 60;
            // 
            // colServerName
            // 
            this.colServerName.DataPropertyName = "SERVERNAME";
            this.colServerName.HeaderText = "ServerName";
            this.colServerName.Name = "colServerName";
            this.colServerName.ReadOnly = true;
            this.colServerName.Visible = false;
            this.colServerName.Width = 112;
            // 
            // colRobotName
            // 
            this.colRobotName.DataPropertyName = "ROBOTNAME";
            this.colRobotName.HeaderText = "Robot Name";
            this.colRobotName.Name = "colRobotName";
            this.colRobotName.ReadOnly = true;
            this.colRobotName.Visible = false;
            this.colRobotName.Width = 114;
            // 
            // colRouteID
            // 
            this.colRouteID.DataPropertyName = "ROUTEID";
            this.colRouteID.HeaderText = "Route ID";
            this.colRouteID.Name = "colRouteID";
            this.colRouteID.ReadOnly = true;
            this.colRouteID.Visible = false;
            this.colRouteID.Width = 90;
            // 
            // colStepID
            // 
            this.colStepID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colStepID.DataPropertyName = "STEPID";
            this.colStepID.HeaderText = "Step ID";
            this.colStepID.Name = "colStepID";
            this.colStepID.ReadOnly = true;
            this.colStepID.Width = 85;
            // 
            // colItemID
            // 
            this.colItemID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colItemID.DataPropertyName = "ITEMID";
            this.colItemID.HeaderText = "Item ID";
            this.colItemID.Name = "colItemID";
            this.colItemID.ReadOnly = true;
            this.colItemID.Width = 85;
            // 
            // colItemSeq
            // 
            this.colItemSeq.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colItemSeq.DataPropertyName = "ITEMSEQ";
            this.colItemSeq.HeaderText = "Item Seq";
            this.colItemSeq.Name = "colItemSeq";
            this.colItemSeq.ReadOnly = true;
            this.colItemSeq.Width = 95;
            // 
            // colDescription
            // 
            this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDescription.DataPropertyName = "MED_DEF_DESCRIPTION";
            this.colDescription.HeaderText = "Desctiption";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            this.colDescription.Width = 300;
            // 
            // colObjectName
            // 
            this.colObjectName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colObjectName.DataPropertyName = "OBJECTNAME";
            this.colObjectName.HeaderText = "Object Name";
            this.colObjectName.Name = "colObjectName";
            this.colObjectName.ReadOnly = true;
            this.colObjectName.Width = 150;
            // 
            // colMethodName
            // 
            this.colMethodName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colMethodName.DataPropertyName = "METHODNAME";
            this.colMethodName.HeaderText = "Method Name";
            this.colMethodName.Name = "colMethodName";
            this.colMethodName.ReadOnly = true;
            this.colMethodName.Width = 150;
            // 
            // colFuncKey
            // 
            this.colFuncKey.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colFuncKey.DataPropertyName = "MED_DEF_FUNCKEY";
            this.colFuncKey.HeaderText = "Function Key";
            this.colFuncKey.Name = "colFuncKey";
            this.colFuncKey.ReadOnly = true;
            this.colFuncKey.Width = 130;
            // 
            // colRemarks
            // 
            this.colRemarks.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colRemarks.DataPropertyName = "REMARKS";
            this.colRemarks.HeaderText = "Remark";
            this.colRemarks.Name = "colRemarks";
            this.colRemarks.ReadOnly = true;
            this.colRemarks.Visible = false;
            // 
            // colLastUpdateTime
            // 
            this.colLastUpdateTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colLastUpdateTime.DataPropertyName = "LASTUPDATETIME";
            this.colLastUpdateTime.HeaderText = "Last UpdateTime";
            this.colLastUpdateTime.Name = "colLastUpdateTime";
            this.colLastUpdateTime.ReadOnly = true;
            this.colLastUpdateTime.Width = 150;
            // 
            // FormRobot_Rule_Filter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(990, 568);
            this.Name = "FormRobot_Rule_Filter";
            this.Text = "   ";
            this.Load += new System.EventHandler(this.FormRobot_Rule_Filter_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.flpButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.pnlStepView.ResumeLayout(false);
            this.pnlStepView.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel pnlStepView;
        private System.Windows.Forms.TextBox txtRobotName;
        private System.Windows.Forms.Label lblRobotName;
        private System.Windows.Forms.TextBox txtRouteID;
        private System.Windows.Forms.Label lblRouteID;
        private System.Windows.Forms.ComboBox cboStepID;
        private System.Windows.Forms.Label lblStepID;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRouteID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStepID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMethodName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFuncKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemarks;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastUpdateTime;

    }
}