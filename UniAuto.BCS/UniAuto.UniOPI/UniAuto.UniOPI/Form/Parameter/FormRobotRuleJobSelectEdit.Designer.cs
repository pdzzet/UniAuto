namespace UniOPI
{
    partial class FormRobotRuleJobSelectEdit
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
            this.tlpRouteCondition = new System.Windows.Forms.TableLayoutPanel();
            this.pnlGridView = new System.Windows.Forms.Panel();
            this.flpBase = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlAdd = new System.Windows.Forms.Panel();
            this.btnNewClose = new System.Windows.Forms.Button();
            this.btnAddNew = new System.Windows.Forms.Button();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grbBase = new System.Windows.Forms.GroupBox();
            this.pnlNew = new System.Windows.Forms.Panel();
            this.txtSelectType = new System.Windows.Forms.TextBox();
            this.explanation = new System.Windows.Forms.Label();
            this.cboRobotName = new System.Windows.Forms.ComboBox();
            this.cboStageType = new System.Windows.Forms.ComboBox();
            this.txtItemSeq = new System.Windows.Forms.TextBox();
            this.lblItemSeq = new System.Windows.Forms.Label();
            this.txtItemID = new System.Windows.Forms.TextBox();
            this.lblStageType = new System.Windows.Forms.Label();
            this.lblItemID = new System.Windows.Forms.Label();
            this.lblSelectType = new System.Windows.Forms.Label();
            this.lblRobotName = new System.Windows.Forms.Label();
            this.lblRemark = new System.Windows.Forms.Label();
            this.txtRemark = new System.Windows.Forms.TextBox();
            this.chkIsEnable = new System.Windows.Forms.CheckBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.chkIsEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSelectType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDesciption = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMethodName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFuncKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemarks = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastUpdateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpRouteCondition.SuspendLayout();
            this.pnlGridView.SuspendLayout();
            this.flpBase.SuspendLayout();
            this.pnlAdd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.pnlButton.SuspendLayout();
            this.grbBase.SuspendLayout();
            this.pnlNew.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(785, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpRouteCondition);
            this.spcBase.Size = new System.Drawing.Size(845, 622);
            // 
            // tlpRouteCondition
            // 
            this.tlpRouteCondition.ColumnCount = 1;
            this.tlpRouteCondition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRouteCondition.Controls.Add(this.pnlGridView, 0, 1);
            this.tlpRouteCondition.Controls.Add(this.grbBase, 0, 0);
            this.tlpRouteCondition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRouteCondition.Location = new System.Drawing.Point(0, 0);
            this.tlpRouteCondition.Name = "tlpRouteCondition";
            this.tlpRouteCondition.RowCount = 2;
            this.tlpRouteCondition.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tlpRouteCondition.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRouteCondition.Size = new System.Drawing.Size(845, 591);
            this.tlpRouteCondition.TabIndex = 3;
            // 
            // pnlGridView
            // 
            this.pnlGridView.Controls.Add(this.flpBase);
            this.pnlGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridView.Location = new System.Drawing.Point(3, 303);
            this.pnlGridView.Name = "pnlGridView";
            this.pnlGridView.Size = new System.Drawing.Size(839, 285);
            this.pnlGridView.TabIndex = 1;
            // 
            // flpBase
            // 
            this.flpBase.Controls.Add(this.pnlAdd);
            this.flpBase.Controls.Add(this.dgvData);
            this.flpBase.Controls.Add(this.pnlButton);
            this.flpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpBase.Location = new System.Drawing.Point(0, 0);
            this.flpBase.Name = "flpBase";
            this.flpBase.Size = new System.Drawing.Size(839, 285);
            this.flpBase.TabIndex = 0;
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnNewClose);
            this.pnlAdd.Controls.Add(this.btnAddNew);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(832, 34);
            this.pnlAdd.TabIndex = 0;
            // 
            // btnNewClose
            // 
            this.btnNewClose.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNewClose.Location = new System.Drawing.Point(420, 2);
            this.btnNewClose.Name = "btnNewClose";
            this.btnNewClose.Size = new System.Drawing.Size(90, 30);
            this.btnNewClose.TabIndex = 10;
            this.btnNewClose.Tag = "Clear";
            this.btnNewClose.Text = "Clear";
            this.btnNewClose.UseVisualStyleBackColor = true;
            this.btnNewClose.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnAddNew
            // 
            this.btnAddNew.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddNew.Location = new System.Drawing.Point(322, 2);
            this.btnAddNew.Name = "btnAddNew";
            this.btnAddNew.Size = new System.Drawing.Size(90, 30);
            this.btnAddNew.TabIndex = 9;
            this.btnAddNew.Tag = "Add";
            this.btnAddNew.Text = "Add";
            this.btnAddNew.UseVisualStyleBackColor = true;
            this.btnAddNew.Click += new System.EventHandler(this.btn_Click);
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
            this.chkIsEnabled,
            this.colServerName,
            this.colRobotName,
            this.colItemID,
            this.colLineType,
            this.colStageType,
            this.colSelectType,
            this.colItemSeq,
            this.colDesciption,
            this.colObjectName,
            this.colMethodName,
            this.colFuncKey,
            this.colRemarks,
            this.colLastUpdateTime});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Location = new System.Drawing.Point(3, 43);
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
            this.dgvData.Size = new System.Drawing.Size(832, 201);
            this.dgvData.TabIndex = 20;
            this.dgvData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvData_CellClick);
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnInsert);
            this.pnlButton.Controls.Add(this.btnClose);
            this.pnlButton.Location = new System.Drawing.Point(3, 250);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(832, 34);
            this.pnlButton.TabIndex = 21;
            // 
            // btnInsert
            // 
            this.btnInsert.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInsert.Location = new System.Drawing.Point(323, 1);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(90, 30);
            this.btnInsert.TabIndex = 18;
            this.btnInsert.Tag = "OK";
            this.btnInsert.Text = "OK";
            this.btnInsert.UseVisualStyleBackColor = true;
            this.btnInsert.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(419, 1);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 30);
            this.btnClose.TabIndex = 19;
            this.btnClose.Tag = "Close";
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btn_Click);
            // 
            // grbBase
            // 
            this.grbBase.Controls.Add(this.pnlNew);
            this.grbBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbBase.Location = new System.Drawing.Point(3, 3);
            this.grbBase.Name = "grbBase";
            this.grbBase.Size = new System.Drawing.Size(839, 294);
            this.grbBase.TabIndex = 2;
            this.grbBase.TabStop = false;
            // 
            // pnlNew
            // 
            this.pnlNew.Controls.Add(this.txtSelectType);
            this.pnlNew.Controls.Add(this.explanation);
            this.pnlNew.Controls.Add(this.cboRobotName);
            this.pnlNew.Controls.Add(this.cboStageType);
            this.pnlNew.Controls.Add(this.txtItemSeq);
            this.pnlNew.Controls.Add(this.lblItemSeq);
            this.pnlNew.Controls.Add(this.txtItemID);
            this.pnlNew.Controls.Add(this.lblStageType);
            this.pnlNew.Controls.Add(this.lblItemID);
            this.pnlNew.Controls.Add(this.lblSelectType);
            this.pnlNew.Controls.Add(this.lblRobotName);
            this.pnlNew.Controls.Add(this.lblRemark);
            this.pnlNew.Controls.Add(this.txtRemark);
            this.pnlNew.Controls.Add(this.chkIsEnable);
            this.pnlNew.Controls.Add(this.lblDescription);
            this.pnlNew.Controls.Add(this.txtDescription);
            this.pnlNew.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNew.Location = new System.Drawing.Point(3, 18);
            this.pnlNew.Name = "pnlNew";
            this.pnlNew.Size = new System.Drawing.Size(833, 273);
            this.pnlNew.TabIndex = 1;
            // 
            // txtSelectType
            // 
            this.txtSelectType.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtSelectType.Location = new System.Drawing.Point(189, 41);
            this.txtSelectType.Name = "txtSelectType";
            this.txtSelectType.ReadOnly = true;
            this.txtSelectType.Size = new System.Drawing.Size(227, 28);
            this.txtSelectType.TabIndex = 36;
            this.txtSelectType.Text = "Common";
            // 
            // explanation
            // 
            this.explanation.AutoSize = true;
            this.explanation.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.explanation.ForeColor = System.Drawing.Color.Red;
            this.explanation.Location = new System.Drawing.Point(715, 80);
            this.explanation.Name = "explanation";
            this.explanation.Size = new System.Drawing.Size(91, 14);
            this.explanation.TabIndex = 35;
            this.explanation.Text = "数字越大越优先";
            // 
            // cboRobotName
            // 
            this.cboRobotName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRobotName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboRobotName.FormattingEnabled = true;
            this.cboRobotName.Location = new System.Drawing.Point(189, 9);
            this.cboRobotName.Name = "cboRobotName";
            this.cboRobotName.Size = new System.Drawing.Size(227, 29);
            this.cboRobotName.TabIndex = 1;
            this.cboRobotName.Tag = "Robot Name";
            this.cboRobotName.SelectedIndexChanged += new System.EventHandler(this.cboRobotName_SelectedIndexChanged);
            // 
            // cboStageType
            // 
            this.cboStageType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStageType.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.cboStageType.FormattingEnabled = true;
            this.cboStageType.Location = new System.Drawing.Point(599, 41);
            this.cboStageType.Name = "cboStageType";
            this.cboStageType.Size = new System.Drawing.Size(227, 29);
            this.cboStageType.TabIndex = 7;
            this.cboStageType.Tag = "Stage Type";
            // 
            // txtItemSeq
            // 
            this.txtItemSeq.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtItemSeq.Location = new System.Drawing.Point(599, 72);
            this.txtItemSeq.MaxLength = 5;
            this.txtItemSeq.Name = "txtItemSeq";
            this.txtItemSeq.Size = new System.Drawing.Size(110, 28);
            this.txtItemSeq.TabIndex = 4;
            this.txtItemSeq.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtItemSeq_KeyPress);
            // 
            // lblItemSeq
            // 
            this.lblItemSeq.BackColor = System.Drawing.Color.Black;
            this.lblItemSeq.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblItemSeq.ForeColor = System.Drawing.Color.White;
            this.lblItemSeq.Location = new System.Drawing.Point(419, 73);
            this.lblItemSeq.Name = "lblItemSeq";
            this.lblItemSeq.Size = new System.Drawing.Size(180, 25);
            this.lblItemSeq.TabIndex = 24;
            this.lblItemSeq.Text = "Item  Sequence";
            this.lblItemSeq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtItemID
            // 
            this.txtItemID.BackColor = System.Drawing.SystemColors.Control;
            this.txtItemID.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtItemID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtItemID.Location = new System.Drawing.Point(189, 72);
            this.txtItemID.Name = "txtItemID";
            this.txtItemID.ReadOnly = true;
            this.txtItemID.Size = new System.Drawing.Size(227, 28);
            this.txtItemID.TabIndex = 3;
            // 
            // lblStageType
            // 
            this.lblStageType.BackColor = System.Drawing.Color.Black;
            this.lblStageType.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStageType.ForeColor = System.Drawing.Color.White;
            this.lblStageType.Location = new System.Drawing.Point(419, 42);
            this.lblStageType.Name = "lblStageType";
            this.lblStageType.Size = new System.Drawing.Size(180, 25);
            this.lblStageType.TabIndex = 24;
            this.lblStageType.Text = "Stage Type";
            this.lblStageType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblItemID
            // 
            this.lblItemID.BackColor = System.Drawing.Color.Black;
            this.lblItemID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblItemID.ForeColor = System.Drawing.Color.White;
            this.lblItemID.Location = new System.Drawing.Point(9, 73);
            this.lblItemID.Name = "lblItemID";
            this.lblItemID.Size = new System.Drawing.Size(180, 25);
            this.lblItemID.TabIndex = 24;
            this.lblItemID.Text = "Item ID";
            this.lblItemID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSelectType
            // 
            this.lblSelectType.BackColor = System.Drawing.Color.Black;
            this.lblSelectType.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblSelectType.ForeColor = System.Drawing.Color.White;
            this.lblSelectType.Location = new System.Drawing.Point(9, 42);
            this.lblSelectType.Name = "lblSelectType";
            this.lblSelectType.Size = new System.Drawing.Size(180, 25);
            this.lblSelectType.TabIndex = 22;
            this.lblSelectType.Text = "Select Type";
            this.lblSelectType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRobotName
            // 
            this.lblRobotName.BackColor = System.Drawing.Color.Black;
            this.lblRobotName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRobotName.ForeColor = System.Drawing.Color.White;
            this.lblRobotName.Location = new System.Drawing.Point(9, 11);
            this.lblRobotName.Name = "lblRobotName";
            this.lblRobotName.Size = new System.Drawing.Size(180, 25);
            this.lblRobotName.TabIndex = 20;
            this.lblRobotName.Text = "Robot Name";
            this.lblRobotName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRemark
            // 
            this.lblRemark.BackColor = System.Drawing.Color.Black;
            this.lblRemark.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRemark.ForeColor = System.Drawing.Color.White;
            this.lblRemark.Location = new System.Drawing.Point(9, 104);
            this.lblRemark.Name = "lblRemark";
            this.lblRemark.Size = new System.Drawing.Size(180, 25);
            this.lblRemark.TabIndex = 17;
            this.lblRemark.Text = "Remark";
            this.lblRemark.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtRemark
            // 
            this.txtRemark.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtRemark.Location = new System.Drawing.Point(189, 103);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Size = new System.Drawing.Size(637, 28);
            this.txtRemark.TabIndex = 9;
            // 
            // chkIsEnable
            // 
            this.chkIsEnable.Checked = true;
            this.chkIsEnable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIsEnable.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkIsEnable.Location = new System.Drawing.Point(423, 12);
            this.chkIsEnable.Name = "chkIsEnable";
            this.chkIsEnable.Size = new System.Drawing.Size(180, 25);
            this.chkIsEnable.TabIndex = 10;
            this.chkIsEnable.Text = "Is Enable ?";
            this.chkIsEnable.UseVisualStyleBackColor = true;
            // 
            // lblDescription
            // 
            this.lblDescription.BackColor = System.Drawing.Color.Black;
            this.lblDescription.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblDescription.ForeColor = System.Drawing.Color.White;
            this.lblDescription.Location = new System.Drawing.Point(9, 198);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(180, 25);
            this.lblDescription.TabIndex = 9;
            this.lblDescription.Text = "Description";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtDescription.Location = new System.Drawing.Point(189, 197);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(638, 73);
            this.txtDescription.TabIndex = 8;
            // 
            // chkIsEnabled
            // 
            this.chkIsEnabled.DataPropertyName = "ISENABLED";
            this.chkIsEnabled.FalseValue = "N";
            this.chkIsEnabled.HeaderText = "IsEnabled";
            this.chkIsEnabled.Name = "chkIsEnabled";
            this.chkIsEnabled.ReadOnly = true;
            this.chkIsEnabled.TrueValue = "Y";
            // 
            // colServerName
            // 
            this.colServerName.DataPropertyName = "SERVERNAME";
            this.colServerName.HeaderText = "ServerName";
            this.colServerName.Name = "colServerName";
            this.colServerName.ReadOnly = true;
            this.colServerName.Visible = false;
            // 
            // colRobotName
            // 
            this.colRobotName.DataPropertyName = "ROBOTNAME";
            this.colRobotName.HeaderText = "Robot Name";
            this.colRobotName.Name = "colRobotName";
            this.colRobotName.ReadOnly = true;
            this.colRobotName.Width = 130;
            // 
            // colItemID
            // 
            this.colItemID.DataPropertyName = "ItemID";
            this.colItemID.HeaderText = "Item ID";
            this.colItemID.Name = "colItemID";
            this.colItemID.ReadOnly = true;
            // 
            // colLineType
            // 
            this.colLineType.DataPropertyName = "LINETYPE";
            this.colLineType.HeaderText = "LineType";
            this.colLineType.Name = "colLineType";
            this.colLineType.ReadOnly = true;
            this.colLineType.Visible = false;
            // 
            // colStageType
            // 
            this.colStageType.DataPropertyName = "STAGETYPE";
            this.colStageType.HeaderText = "Stage Type";
            this.colStageType.Name = "colStageType";
            this.colStageType.ReadOnly = true;
            this.colStageType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colStageType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colSelectType
            // 
            this.colSelectType.DataPropertyName = "SELECTTYPE";
            this.colSelectType.HeaderText = "Select Type";
            this.colSelectType.Name = "colSelectType";
            this.colSelectType.ReadOnly = true;
            this.colSelectType.Width = 120;
            // 
            // colItemSeq
            // 
            this.colItemSeq.DataPropertyName = "ITEMSEQ";
            this.colItemSeq.HeaderText = "Item  Sequence";
            this.colItemSeq.Name = "colItemSeq";
            this.colItemSeq.ReadOnly = true;
            this.colItemSeq.Width = 160;
            // 
            // colDesciption
            // 
            this.colDesciption.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDesciption.DataPropertyName = "DESCRIPTION";
            this.colDesciption.HeaderText = "Description";
            this.colDesciption.Name = "colDesciption";
            this.colDesciption.ReadOnly = true;
            this.colDesciption.Width = 206;
            // 
            // colObjectName
            // 
            this.colObjectName.DataPropertyName = "OBJECTNAME";
            this.colObjectName.HeaderText = "Object Name";
            this.colObjectName.Name = "colObjectName";
            this.colObjectName.ReadOnly = true;
            this.colObjectName.Width = 150;
            // 
            // colMethodName
            // 
            this.colMethodName.DataPropertyName = "METHODNAME";
            this.colMethodName.HeaderText = "Method Name";
            this.colMethodName.Name = "colMethodName";
            this.colMethodName.ReadOnly = true;
            this.colMethodName.Width = 150;
            // 
            // colFuncKey
            // 
            this.colFuncKey.HeaderText = "Func Key";
            this.colFuncKey.Name = "colFuncKey";
            this.colFuncKey.ReadOnly = true;
            this.colFuncKey.Width = 150;
            // 
            // colRemarks
            // 
            this.colRemarks.DataPropertyName = "REMARKS";
            this.colRemarks.HeaderText = "Remarks";
            this.colRemarks.Name = "colRemarks";
            this.colRemarks.ReadOnly = true;
            // 
            // colLastUpdateTime
            // 
            this.colLastUpdateTime.DataPropertyName = "LASTUPDATETIME";
            this.colLastUpdateTime.HeaderText = "Last Update Time";
            this.colLastUpdateTime.Name = "colLastUpdateTime";
            this.colLastUpdateTime.ReadOnly = true;
            this.colLastUpdateTime.Width = 200;
            // 
            // FormRobotRuleJobSelectEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(845, 622);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormRobotRuleJobSelectEdit";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormRobotRuleJobSelectEdit_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpRouteCondition.ResumeLayout(false);
            this.pnlGridView.ResumeLayout(false);
            this.flpBase.ResumeLayout(false);
            this.pnlAdd.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.pnlButton.ResumeLayout(false);
            this.grbBase.ResumeLayout(false);
            this.pnlNew.ResumeLayout(false);
            this.pnlNew.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpRouteCondition;
        private System.Windows.Forms.Panel pnlGridView;
        private System.Windows.Forms.Button btnNewClose;
        private System.Windows.Forms.Button btnAddNew;
        private System.Windows.Forms.FlowLayoutPanel flpBase;
        private System.Windows.Forms.Panel pnlAdd;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox grbBase;
        private System.Windows.Forms.Panel pnlNew;
        private System.Windows.Forms.ComboBox cboStageType;
        private System.Windows.Forms.TextBox txtItemSeq;
        private System.Windows.Forms.Label lblItemSeq;
        private System.Windows.Forms.TextBox txtItemID;
        private System.Windows.Forms.Label lblStageType;
        private System.Windows.Forms.Label lblItemID;
        private System.Windows.Forms.Label lblSelectType;
        private System.Windows.Forms.Label lblRobotName;
        private System.Windows.Forms.Label lblRemark;
        private System.Windows.Forms.TextBox txtRemark;
        private System.Windows.Forms.CheckBox chkIsEnable;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.ComboBox cboRobotName;
        private System.Windows.Forms.Label explanation;
        private System.Windows.Forms.TextBox txtSelectType;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chkIsEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStageType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSelectType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDesciption;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMethodName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFuncKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemarks;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastUpdateTime;
    }
}