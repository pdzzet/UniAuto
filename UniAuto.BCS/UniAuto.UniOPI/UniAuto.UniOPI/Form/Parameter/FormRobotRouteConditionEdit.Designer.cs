namespace UniOPI
{
    partial class FormRobotRouteConditionEdit
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
            this.flpBase = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlAdd = new System.Windows.Forms.Panel();
            this.btnAddNew = new System.Windows.Forms.Button();
            this.btnNewClose = new System.Windows.Forms.Button();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colObjectKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRouteID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colConditionId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDesciption = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMethodName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFuncKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.chkIsEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colRemarks = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastUpdateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colConditionSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRoutePriority = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.grbBase = new System.Windows.Forms.GroupBox();
            this.pnlNew = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtRoutePriority = new System.Windows.Forms.TextBox();
            this.explanation = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.cboRobotName = new System.Windows.Forms.ComboBox();
            this.lblRobotName = new System.Windows.Forms.Label();
            this.cboRouteID = new System.Windows.Forms.ComboBox();
            this.txtConditionID = new System.Windows.Forms.TextBox();
            this.lblConditionID = new System.Windows.Forms.Label();
            this.lblRouteID = new System.Windows.Forms.Label();
            this.lblRemark = new System.Windows.Forms.Label();
            this.txtRemark = new System.Windows.Forms.TextBox();
            this.chkIsEnable = new System.Windows.Forms.CheckBox();
            this.lblConditionSeq = new System.Windows.Forms.Label();
            this.txtConditionSeq = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpRouteCondition.SuspendLayout();
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
            this.lblCaption.Size = new System.Drawing.Size(781, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpRouteCondition);
            this.spcBase.Size = new System.Drawing.Size(841, 595);
            // 
            // tlpRouteCondition
            // 
            this.tlpRouteCondition.ColumnCount = 1;
            this.tlpRouteCondition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRouteCondition.Controls.Add(this.flpBase, 0, 1);
            this.tlpRouteCondition.Controls.Add(this.grbBase, 0, 0);
            this.tlpRouteCondition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRouteCondition.Location = new System.Drawing.Point(0, 0);
            this.tlpRouteCondition.Name = "tlpRouteCondition";
            this.tlpRouteCondition.RowCount = 2;
            this.tlpRouteCondition.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 279F));
            this.tlpRouteCondition.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRouteCondition.Size = new System.Drawing.Size(841, 564);
            this.tlpRouteCondition.TabIndex = 1;
            // 
            // flpBase
            // 
            this.flpBase.Controls.Add(this.pnlAdd);
            this.flpBase.Controls.Add(this.dgvData);
            this.flpBase.Controls.Add(this.pnlButton);
            this.flpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpBase.Location = new System.Drawing.Point(3, 282);
            this.flpBase.Name = "flpBase";
            this.flpBase.Size = new System.Drawing.Size(835, 279);
            this.flpBase.TabIndex = 1;
            this.flpBase.Click += new System.EventHandler(this.btn_Click);
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnAddNew);
            this.pnlAdd.Controls.Add(this.btnNewClose);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(832, 35);
            this.pnlAdd.TabIndex = 19;
            // 
            // btnAddNew
            // 
            this.btnAddNew.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddNew.Location = new System.Drawing.Point(320, 2);
            this.btnAddNew.Name = "btnAddNew";
            this.btnAddNew.Size = new System.Drawing.Size(90, 30);
            this.btnAddNew.TabIndex = 9;
            this.btnAddNew.Tag = "Add";
            this.btnAddNew.Text = "Add";
            this.btnAddNew.UseVisualStyleBackColor = true;
            this.btnAddNew.Click += new System.EventHandler(this.btn_Click);
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
            this.colRobotName,
            this.colRouteID,
            this.colConditionId,
            this.colServerName,
            this.colLineType,
            this.colDesciption,
            this.colObjectName,
            this.colMethodName,
            this.colFuncKey,
            this.chkIsEnabled,
            this.colRemarks,
            this.colLastUpdateTime,
            this.colConditionSeq,
            this.colRoutePriority});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Location = new System.Drawing.Point(3, 44);
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
            this.dgvData.Size = new System.Drawing.Size(832, 190);
            this.dgvData.TabIndex = 17;
            this.dgvData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvData_CellClick);
            // 
            // colObjectKey
            // 
            this.colObjectKey.DataPropertyName = "OBJECTKEY";
            this.colObjectKey.HeaderText = "OBJECT KEY";
            this.colObjectKey.Name = "colObjectKey";
            this.colObjectKey.ReadOnly = true;
            this.colObjectKey.Visible = false;
            // 
            // colRobotName
            // 
            this.colRobotName.DataPropertyName = "ROBOTNAME";
            this.colRobotName.HeaderText = "Robot Name";
            this.colRobotName.Name = "colRobotName";
            this.colRobotName.ReadOnly = true;
            this.colRobotName.Width = 150;
            // 
            // colRouteID
            // 
            this.colRouteID.DataPropertyName = "ROUTEID";
            this.colRouteID.HeaderText = "Route ID";
            this.colRouteID.Name = "colRouteID";
            this.colRouteID.ReadOnly = true;
            this.colRouteID.Width = 120;
            // 
            // colConditionId
            // 
            this.colConditionId.DataPropertyName = "CONDITIONID";
            this.colConditionId.HeaderText = "Condition ID";
            this.colConditionId.Name = "colConditionId";
            this.colConditionId.ReadOnly = true;
            this.colConditionId.Width = 200;
            // 
            // colServerName
            // 
            this.colServerName.DataPropertyName = "SERVERNAME";
            this.colServerName.HeaderText = "ServerName";
            this.colServerName.Name = "colServerName";
            this.colServerName.ReadOnly = true;
            this.colServerName.Visible = false;
            // 
            // colLineType
            // 
            this.colLineType.DataPropertyName = "LINETYPE";
            this.colLineType.HeaderText = "LineType";
            this.colLineType.Name = "colLineType";
            this.colLineType.ReadOnly = true;
            this.colLineType.Visible = false;
            // 
            // colDesciption
            // 
            this.colDesciption.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDesciption.DataPropertyName = "MED_DEF_DESCRIPTION";
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
            this.colObjectName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colObjectName.Width = 200;
            // 
            // colMethodName
            // 
            this.colMethodName.DataPropertyName = "METHODNAME";
            this.colMethodName.HeaderText = "Method Name";
            this.colMethodName.Name = "colMethodName";
            this.colMethodName.ReadOnly = true;
            this.colMethodName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colMethodName.Width = 200;
            // 
            // colFuncKey
            // 
            this.colFuncKey.DataPropertyName = "MED_DEF_FUNCKEY";
            this.colFuncKey.HeaderText = "Function Key";
            this.colFuncKey.Name = "colFuncKey";
            this.colFuncKey.ReadOnly = true;
            this.colFuncKey.Width = 150;
            // 
            // chkIsEnabled
            // 
            this.chkIsEnabled.DataPropertyName = "ISENABLED";
            this.chkIsEnabled.FalseValue = "N";
            this.chkIsEnabled.HeaderText = "Enabled";
            this.chkIsEnabled.Name = "chkIsEnabled";
            this.chkIsEnabled.ReadOnly = true;
            this.chkIsEnabled.TrueValue = "Y";
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
            // colConditionSeq
            // 
            this.colConditionSeq.DataPropertyName = "CONDITIONSEQ";
            this.colConditionSeq.HeaderText = "Condition Sequence";
            this.colConditionSeq.Name = "colConditionSeq";
            this.colConditionSeq.ReadOnly = true;
            this.colConditionSeq.Width = 250;
            // 
            // colRoutePriority
            // 
            this.colRoutePriority.DataPropertyName = "ROUTEPRIORITY";
            this.colRoutePriority.HeaderText = "Route Priority";
            this.colRoutePriority.Name = "colRoutePriority";
            this.colRoutePriority.ReadOnly = true;
            this.colRoutePriority.Width = 200;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnClose);
            this.pnlButton.Controls.Add(this.btnOK);
            this.pnlButton.Location = new System.Drawing.Point(3, 240);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(832, 36);
            this.pnlButton.TabIndex = 18;
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(420, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 30);
            this.btnClose.TabIndex = 12;
            this.btnClose.Tag = "Close";
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnOK
            // 
            this.btnOK.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(320, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 30);
            this.btnOK.TabIndex = 11;
            this.btnOK.Tag = "OK";
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btn_Click);
            // 
            // grbBase
            // 
            this.grbBase.Controls.Add(this.pnlNew);
            this.grbBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbBase.Location = new System.Drawing.Point(3, 3);
            this.grbBase.Name = "grbBase";
            this.grbBase.Size = new System.Drawing.Size(835, 273);
            this.grbBase.TabIndex = 2;
            this.grbBase.TabStop = false;
            // 
            // pnlNew
            // 
            this.pnlNew.Controls.Add(this.label1);
            this.pnlNew.Controls.Add(this.label2);
            this.pnlNew.Controls.Add(this.txtRoutePriority);
            this.pnlNew.Controls.Add(this.explanation);
            this.pnlNew.Controls.Add(this.txtDescription);
            this.pnlNew.Controls.Add(this.cboRobotName);
            this.pnlNew.Controls.Add(this.lblRobotName);
            this.pnlNew.Controls.Add(this.cboRouteID);
            this.pnlNew.Controls.Add(this.txtConditionID);
            this.pnlNew.Controls.Add(this.lblConditionID);
            this.pnlNew.Controls.Add(this.lblRouteID);
            this.pnlNew.Controls.Add(this.lblRemark);
            this.pnlNew.Controls.Add(this.txtRemark);
            this.pnlNew.Controls.Add(this.chkIsEnable);
            this.pnlNew.Controls.Add(this.lblConditionSeq);
            this.pnlNew.Controls.Add(this.txtConditionSeq);
            this.pnlNew.Controls.Add(this.lblDescription);
            this.pnlNew.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNew.Location = new System.Drawing.Point(3, 18);
            this.pnlNew.Name = "pnlNew";
            this.pnlNew.Size = new System.Drawing.Size(829, 252);
            this.pnlNew.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(714, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 14);
            this.label1.TabIndex = 38;
            this.label1.Text = "数字越大越优先";
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Black;
            this.label2.Font = new System.Drawing.Font("Calibri", 13F);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(416, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 25);
            this.label2.TabIndex = 37;
            this.label2.Text = "Route Priority";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtRoutePriority
            // 
            this.txtRoutePriority.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtRoutePriority.Location = new System.Drawing.Point(596, 64);
            this.txtRoutePriority.MaxLength = 5;
            this.txtRoutePriority.Name = "txtRoutePriority";
            this.txtRoutePriority.ReadOnly = true;
            this.txtRoutePriority.Size = new System.Drawing.Size(114, 28);
            this.txtRoutePriority.TabIndex = 36;
            this.txtRoutePriority.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtConditionSeq_KeyPress);
            // 
            // explanation
            // 
            this.explanation.AutoSize = true;
            this.explanation.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.explanation.ForeColor = System.Drawing.Color.Red;
            this.explanation.Location = new System.Drawing.Point(714, 42);
            this.explanation.Name = "explanation";
            this.explanation.Size = new System.Drawing.Size(91, 14);
            this.explanation.TabIndex = 35;
            this.explanation.Text = "数字越大越优先";
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtDescription.Location = new System.Drawing.Point(183, 187);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(640, 60);
            this.txtDescription.TabIndex = 3;
            // 
            // cboRobotName
            // 
            this.cboRobotName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRobotName.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.cboRobotName.FormattingEnabled = true;
            this.cboRobotName.Location = new System.Drawing.Point(183, 2);
            this.cboRobotName.Name = "cboRobotName";
            this.cboRobotName.Size = new System.Drawing.Size(227, 29);
            this.cboRobotName.TabIndex = 0;
            this.cboRobotName.Tag = "Route Name";
            // 
            // lblRobotName
            // 
            this.lblRobotName.BackColor = System.Drawing.Color.Black;
            this.lblRobotName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRobotName.ForeColor = System.Drawing.Color.White;
            this.lblRobotName.Location = new System.Drawing.Point(3, 4);
            this.lblRobotName.Name = "lblRobotName";
            this.lblRobotName.Size = new System.Drawing.Size(180, 25);
            this.lblRobotName.TabIndex = 24;
            this.lblRobotName.Text = "Robot Name";
            this.lblRobotName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboRouteID
            // 
            this.cboRouteID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRouteID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.cboRouteID.FormattingEnabled = true;
            this.cboRouteID.Location = new System.Drawing.Point(183, 33);
            this.cboRouteID.Name = "cboRouteID";
            this.cboRouteID.Size = new System.Drawing.Size(227, 29);
            this.cboRouteID.TabIndex = 1;
            this.cboRouteID.Tag = "Route ID";
            this.cboRouteID.SelectionChangeCommitted += new System.EventHandler(this.cboRouteID_SelectionChangeCommitted);
            // 
            // txtConditionID
            // 
            this.txtConditionID.BackColor = System.Drawing.SystemColors.Control;
            this.txtConditionID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtConditionID.Location = new System.Drawing.Point(183, 64);
            this.txtConditionID.Name = "txtConditionID";
            this.txtConditionID.ReadOnly = true;
            this.txtConditionID.Size = new System.Drawing.Size(227, 28);
            this.txtConditionID.TabIndex = 2;
            // 
            // lblConditionID
            // 
            this.lblConditionID.BackColor = System.Drawing.Color.Black;
            this.lblConditionID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblConditionID.ForeColor = System.Drawing.Color.White;
            this.lblConditionID.Location = new System.Drawing.Point(3, 64);
            this.lblConditionID.Name = "lblConditionID";
            this.lblConditionID.Size = new System.Drawing.Size(180, 25);
            this.lblConditionID.TabIndex = 22;
            this.lblConditionID.Text = "Condition ID";
            this.lblConditionID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRouteID
            // 
            this.lblRouteID.BackColor = System.Drawing.Color.Black;
            this.lblRouteID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRouteID.ForeColor = System.Drawing.Color.White;
            this.lblRouteID.Location = new System.Drawing.Point(3, 34);
            this.lblRouteID.Name = "lblRouteID";
            this.lblRouteID.Size = new System.Drawing.Size(180, 25);
            this.lblRouteID.TabIndex = 20;
            this.lblRouteID.Text = "Route ID";
            this.lblRouteID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRemark
            // 
            this.lblRemark.BackColor = System.Drawing.Color.Black;
            this.lblRemark.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRemark.ForeColor = System.Drawing.Color.White;
            this.lblRemark.Location = new System.Drawing.Point(3, 94);
            this.lblRemark.Name = "lblRemark";
            this.lblRemark.Size = new System.Drawing.Size(180, 25);
            this.lblRemark.TabIndex = 17;
            this.lblRemark.Text = "Remark";
            this.lblRemark.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtRemark
            // 
            this.txtRemark.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtRemark.Location = new System.Drawing.Point(183, 94);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Size = new System.Drawing.Size(640, 28);
            this.txtRemark.TabIndex = 7;
            // 
            // chkIsEnable
            // 
            this.chkIsEnable.AutoSize = true;
            this.chkIsEnable.Checked = true;
            this.chkIsEnable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIsEnable.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkIsEnable.Location = new System.Drawing.Point(420, 3);
            this.chkIsEnable.Name = "chkIsEnable";
            this.chkIsEnable.Size = new System.Drawing.Size(90, 26);
            this.chkIsEnable.TabIndex = 6;
            this.chkIsEnable.Text = "IsEnable";
            this.chkIsEnable.UseVisualStyleBackColor = true;
            // 
            // lblConditionSeq
            // 
            this.lblConditionSeq.BackColor = System.Drawing.Color.Black;
            this.lblConditionSeq.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblConditionSeq.ForeColor = System.Drawing.Color.White;
            this.lblConditionSeq.Location = new System.Drawing.Point(416, 34);
            this.lblConditionSeq.Name = "lblConditionSeq";
            this.lblConditionSeq.Size = new System.Drawing.Size(180, 25);
            this.lblConditionSeq.TabIndex = 11;
            this.lblConditionSeq.Text = "Condition Sequence";
            this.lblConditionSeq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtConditionSeq
            // 
            this.txtConditionSeq.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtConditionSeq.Location = new System.Drawing.Point(596, 34);
            this.txtConditionSeq.MaxLength = 5;
            this.txtConditionSeq.Name = "txtConditionSeq";
            this.txtConditionSeq.Size = new System.Drawing.Size(114, 28);
            this.txtConditionSeq.TabIndex = 8;
            this.txtConditionSeq.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtConditionSeq_KeyPress);
            // 
            // lblDescription
            // 
            this.lblDescription.BackColor = System.Drawing.Color.Black;
            this.lblDescription.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblDescription.ForeColor = System.Drawing.Color.White;
            this.lblDescription.Location = new System.Drawing.Point(3, 187);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(180, 25);
            this.lblDescription.TabIndex = 9;
            this.lblDescription.Text = "Description";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FormRobotRouteConditionEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(841, 595);
            this.Name = "FormRobotRouteConditionEdit";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormRobotRouteConditionEdit_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpRouteCondition.ResumeLayout(false);
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
        private System.Windows.Forms.FlowLayoutPanel flpBase;
        private System.Windows.Forms.Panel pnlAdd;
        private System.Windows.Forms.Button btnAddNew;
        private System.Windows.Forms.Button btnNewClose;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox grbBase;
        private System.Windows.Forms.Panel pnlNew;
        private System.Windows.Forms.TextBox txtConditionID;
        private System.Windows.Forms.Label lblConditionID;
        private System.Windows.Forms.Label lblRouteID;
        private System.Windows.Forms.Label lblRemark;
        private System.Windows.Forms.TextBox txtRemark;
        private System.Windows.Forms.CheckBox chkIsEnable;
        private System.Windows.Forms.Label lblConditionSeq;
        private System.Windows.Forms.TextBox txtConditionSeq;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.ComboBox cboRouteID;
        private System.Windows.Forms.ComboBox cboRobotName;
        private System.Windows.Forms.Label lblRobotName;
        private System.Windows.Forms.Label explanation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtRoutePriority;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRouteID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colConditionId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDesciption;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMethodName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFuncKey;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chkIsEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemarks;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastUpdateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colConditionSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRoutePriority;
    }
}