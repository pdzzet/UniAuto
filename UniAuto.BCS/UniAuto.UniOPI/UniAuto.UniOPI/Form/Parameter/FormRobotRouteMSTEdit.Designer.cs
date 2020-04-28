namespace UniOPI
{
    partial class FormRobotRouteMSTEdit
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
            this.tlpRouteCondition = new System.Windows.Forms.TableLayoutPanel();
            this.pnlGridView = new System.Windows.Forms.Panel();
            this.flpBase = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlAdd = new System.Windows.Forms.Panel();
            this.btnAddNew = new System.Windows.Forms.Button();
            this.btnNewClose = new System.Windows.Forms.Button();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colObjectKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRouteID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRouteName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colRTCModeFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDesciption = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemarks = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastUpdateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRoutePriority = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRTCForceReturnFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlNew = new System.Windows.Forms.Panel();
            this.grbBase = new System.Windows.Forms.GroupBox();
            this.pnlNewObject = new System.Windows.Forms.Panel();
            this.chkRTCForceReturnFlag = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRoutePriority = new System.Windows.Forms.TextBox();
            this.lblRoutePriority = new System.Windows.Forms.Label();
            this.chkRTCModeFlag = new System.Windows.Forms.CheckBox();
            this.cboRobotName = new System.Windows.Forms.ComboBox();
            this.txtRouteName = new System.Windows.Forms.TextBox();
            this.lblRouteName = new System.Windows.Forms.Label();
            this.txtRoubeID = new System.Windows.Forms.TextBox();
            this.lblRouteID = new System.Windows.Forms.Label();
            this.lblRobotName = new System.Windows.Forms.Label();
            this.lblRemark = new System.Windows.Forms.Label();
            this.txtRemark = new System.Windows.Forms.TextBox();
            this.chkIsEnable = new System.Windows.Forms.CheckBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpRouteCondition.SuspendLayout();
            this.pnlGridView.SuspendLayout();
            this.flpBase.SuspendLayout();
            this.pnlAdd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.pnlButton.SuspendLayout();
            this.pnlNew.SuspendLayout();
            this.grbBase.SuspendLayout();
            this.pnlNewObject.SuspendLayout();
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
            this.spcBase.Size = new System.Drawing.Size(841, 583);
            // 
            // tlpRouteCondition
            // 
            this.tlpRouteCondition.ColumnCount = 1;
            this.tlpRouteCondition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRouteCondition.Controls.Add(this.pnlGridView, 0, 1);
            this.tlpRouteCondition.Controls.Add(this.pnlNew, 0, 0);
            this.tlpRouteCondition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRouteCondition.Location = new System.Drawing.Point(0, 0);
            this.tlpRouteCondition.Name = "tlpRouteCondition";
            this.tlpRouteCondition.RowCount = 2;
            this.tlpRouteCondition.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 263F));
            this.tlpRouteCondition.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 337F));
            this.tlpRouteCondition.Size = new System.Drawing.Size(841, 552);
            this.tlpRouteCondition.TabIndex = 2;
            // 
            // pnlGridView
            // 
            this.pnlGridView.Controls.Add(this.flpBase);
            this.pnlGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridView.Location = new System.Drawing.Point(3, 266);
            this.pnlGridView.Name = "pnlGridView";
            this.pnlGridView.Size = new System.Drawing.Size(835, 331);
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
            this.flpBase.Size = new System.Drawing.Size(835, 331);
            this.flpBase.TabIndex = 0;
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnAddNew);
            this.pnlAdd.Controls.Add(this.btnNewClose);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(832, 34);
            this.pnlAdd.TabIndex = 0;
            // 
            // btnAddNew
            // 
            this.btnAddNew.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddNew.Location = new System.Drawing.Point(320, 0);
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
            this.btnNewClose.Location = new System.Drawing.Point(420, 0);
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
            this.colServerName,
            this.colRobotName,
            this.colRouteID,
            this.colRouteName,
            this.colLineType,
            this.colIsEnabled,
            this.colRTCModeFlag,
            this.colDesciption,
            this.colRemarks,
            this.colLastUpdateTime,
            this.colRoutePriority,
            this.colRTCForceReturnFlag});
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
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(832, 201);
            this.dgvData.TabIndex = 19;
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
            // colRouteName
            // 
            this.colRouteName.DataPropertyName = "ROUTENAME";
            this.colRouteName.HeaderText = "Route Name";
            this.colRouteName.Name = "colRouteName";
            this.colRouteName.ReadOnly = true;
            this.colRouteName.Width = 150;
            // 
            // colLineType
            // 
            this.colLineType.DataPropertyName = "LINETYPE";
            this.colLineType.HeaderText = "LineType";
            this.colLineType.Name = "colLineType";
            this.colLineType.ReadOnly = true;
            this.colLineType.Visible = false;
            // 
            // colIsEnabled
            // 
            this.colIsEnabled.DataPropertyName = "ISENABLED";
            this.colIsEnabled.FalseValue = "N";
            this.colIsEnabled.HeaderText = "Enabled";
            this.colIsEnabled.Name = "colIsEnabled";
            this.colIsEnabled.ReadOnly = true;
            this.colIsEnabled.TrueValue = "Y";
            // 
            // colRTCModeFlag
            // 
            this.colRTCModeFlag.DataPropertyName = "RTCMODEFLAG";
            this.colRTCModeFlag.FalseValue = "N";
            this.colRTCModeFlag.HeaderText = "RTC Flag";
            this.colRTCModeFlag.Name = "colRTCModeFlag";
            this.colRTCModeFlag.ReadOnly = true;
            this.colRTCModeFlag.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colRTCModeFlag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colRTCModeFlag.TrueValue = "Y";
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
            // colRoutePriority
            // 
            this.colRoutePriority.DataPropertyName = "ROUTEPRIORITY";
            this.colRoutePriority.HeaderText = "Route Priority";
            this.colRoutePriority.Name = "colRoutePriority";
            this.colRoutePriority.ReadOnly = true;
            this.colRoutePriority.Width = 150;
            // 
            // colRTCForceReturnFlag
            // 
            this.colRTCForceReturnFlag.DataPropertyName = "RTCFORCERETURNFLAG";
            this.colRTCForceReturnFlag.FalseValue = "N";
            this.colRTCForceReturnFlag.HeaderText = "RTC Force Return Flag";
            this.colRTCForceReturnFlag.Name = "colRTCForceReturnFlag";
            this.colRTCForceReturnFlag.ReadOnly = true;
            this.colRTCForceReturnFlag.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colRTCForceReturnFlag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colRTCForceReturnFlag.TrueValue = "Y";
            this.colRTCForceReturnFlag.Visible = false;
            this.colRTCForceReturnFlag.Width = 200;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnInsert);
            this.pnlButton.Controls.Add(this.btnClose);
            this.pnlButton.Location = new System.Drawing.Point(3, 250);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.pnlButton.Size = new System.Drawing.Size(832, 34);
            this.pnlButton.TabIndex = 1;
            // 
            // btnInsert
            // 
            this.btnInsert.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInsert.Location = new System.Drawing.Point(320, 0);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(90, 30);
            this.btnInsert.TabIndex = 17;
            this.btnInsert.Tag = "Insert";
            this.btnInsert.Text = "OK";
            this.btnInsert.UseVisualStyleBackColor = true;
            this.btnInsert.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(420, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 30);
            this.btnClose.TabIndex = 18;
            this.btnClose.Tag = "Close";
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btn_Click);
            // 
            // pnlNew
            // 
            this.pnlNew.Controls.Add(this.grbBase);
            this.pnlNew.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNew.Location = new System.Drawing.Point(3, 3);
            this.pnlNew.Name = "pnlNew";
            this.pnlNew.Size = new System.Drawing.Size(835, 257);
            this.pnlNew.TabIndex = 0;
            // 
            // grbBase
            // 
            this.grbBase.Controls.Add(this.pnlNewObject);
            this.grbBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbBase.Location = new System.Drawing.Point(0, 0);
            this.grbBase.Name = "grbBase";
            this.grbBase.Size = new System.Drawing.Size(835, 257);
            this.grbBase.TabIndex = 0;
            this.grbBase.TabStop = false;
            // 
            // pnlNewObject
            // 
            this.pnlNewObject.Controls.Add(this.chkRTCForceReturnFlag);
            this.pnlNewObject.Controls.Add(this.label1);
            this.pnlNewObject.Controls.Add(this.txtRoutePriority);
            this.pnlNewObject.Controls.Add(this.lblRoutePriority);
            this.pnlNewObject.Controls.Add(this.chkRTCModeFlag);
            this.pnlNewObject.Controls.Add(this.cboRobotName);
            this.pnlNewObject.Controls.Add(this.txtRouteName);
            this.pnlNewObject.Controls.Add(this.lblRouteName);
            this.pnlNewObject.Controls.Add(this.txtRoubeID);
            this.pnlNewObject.Controls.Add(this.lblRouteID);
            this.pnlNewObject.Controls.Add(this.lblRobotName);
            this.pnlNewObject.Controls.Add(this.lblRemark);
            this.pnlNewObject.Controls.Add(this.txtRemark);
            this.pnlNewObject.Controls.Add(this.chkIsEnable);
            this.pnlNewObject.Controls.Add(this.lblDescription);
            this.pnlNewObject.Controls.Add(this.txtDescription);
            this.pnlNewObject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNewObject.Location = new System.Drawing.Point(3, 18);
            this.pnlNewObject.Name = "pnlNewObject";
            this.pnlNewObject.Size = new System.Drawing.Size(829, 236);
            this.pnlNewObject.TabIndex = 0;
            // 
            // chkRTCForceReturnFlag
            // 
            this.chkRTCForceReturnFlag.AutoSize = true;
            this.chkRTCForceReturnFlag.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkRTCForceReturnFlag.Location = new System.Drawing.Point(449, 36);
            this.chkRTCForceReturnFlag.Name = "chkRTCForceReturnFlag";
            this.chkRTCForceReturnFlag.Size = new System.Drawing.Size(254, 26);
            this.chkRTCForceReturnFlag.TabIndex = 49;
            this.chkRTCForceReturnFlag.Text = "Enable RTC Force Return Flag ?";
            this.chkRTCForceReturnFlag.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(447, 107);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 14);
            this.label1.TabIndex = 48;
            this.label1.Text = "数字越大越优先";
            // 
            // txtRoutePriority
            // 
            this.txtRoutePriority.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtRoutePriority.Location = new System.Drawing.Point(183, 99);
            this.txtRoutePriority.MaxLength = 5;
            this.txtRoutePriority.Name = "txtRoutePriority";
            this.txtRoutePriority.Size = new System.Drawing.Size(260, 28);
            this.txtRoutePriority.TabIndex = 46;
            this.txtRoutePriority.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtRoutePriority_KeyPress);
            // 
            // lblRoutePriority
            // 
            this.lblRoutePriority.BackColor = System.Drawing.Color.Black;
            this.lblRoutePriority.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRoutePriority.ForeColor = System.Drawing.Color.White;
            this.lblRoutePriority.Location = new System.Drawing.Point(3, 100);
            this.lblRoutePriority.Name = "lblRoutePriority";
            this.lblRoutePriority.Size = new System.Drawing.Size(180, 26);
            this.lblRoutePriority.TabIndex = 47;
            this.lblRoutePriority.Text = "Route Priority";
            this.lblRoutePriority.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkRTCModeFlag
            // 
            this.chkRTCModeFlag.AutoSize = true;
            this.chkRTCModeFlag.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkRTCModeFlag.Location = new System.Drawing.Point(565, 6);
            this.chkRTCModeFlag.Name = "chkRTCModeFlag";
            this.chkRTCModeFlag.Size = new System.Drawing.Size(248, 26);
            this.chkRTCModeFlag.TabIndex = 45;
            this.chkRTCModeFlag.Text = "Enabled Robot RTC Function ?";
            this.chkRTCModeFlag.UseVisualStyleBackColor = true;
            // 
            // cboRobotName
            // 
            this.cboRobotName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRobotName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboRobotName.FormattingEnabled = true;
            this.cboRobotName.Location = new System.Drawing.Point(183, 5);
            this.cboRobotName.Name = "cboRobotName";
            this.cboRobotName.Size = new System.Drawing.Size(260, 29);
            this.cboRobotName.TabIndex = 1;
            // 
            // txtRouteName
            // 
            this.txtRouteName.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtRouteName.Location = new System.Drawing.Point(183, 68);
            this.txtRouteName.Name = "txtRouteName";
            this.txtRouteName.Size = new System.Drawing.Size(640, 28);
            this.txtRouteName.TabIndex = 3;
            // 
            // lblRouteName
            // 
            this.lblRouteName.BackColor = System.Drawing.Color.Black;
            this.lblRouteName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRouteName.ForeColor = System.Drawing.Color.White;
            this.lblRouteName.Location = new System.Drawing.Point(3, 68);
            this.lblRouteName.Name = "lblRouteName";
            this.lblRouteName.Size = new System.Drawing.Size(180, 26);
            this.lblRouteName.TabIndex = 44;
            this.lblRouteName.Text = "Route Name";
            this.lblRouteName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtRoubeID
            // 
            this.txtRoubeID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtRoubeID.Location = new System.Drawing.Point(183, 37);
            this.txtRoubeID.Name = "txtRoubeID";
            this.txtRoubeID.Size = new System.Drawing.Size(260, 28);
            this.txtRoubeID.TabIndex = 2;
            // 
            // lblRouteID
            // 
            this.lblRouteID.BackColor = System.Drawing.Color.Black;
            this.lblRouteID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRouteID.ForeColor = System.Drawing.Color.White;
            this.lblRouteID.Location = new System.Drawing.Point(3, 38);
            this.lblRouteID.Name = "lblRouteID";
            this.lblRouteID.Size = new System.Drawing.Size(180, 26);
            this.lblRouteID.TabIndex = 43;
            this.lblRouteID.Text = "Route ID";
            this.lblRouteID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRobotName
            // 
            this.lblRobotName.BackColor = System.Drawing.Color.Black;
            this.lblRobotName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRobotName.ForeColor = System.Drawing.Color.White;
            this.lblRobotName.Location = new System.Drawing.Point(3, 6);
            this.lblRobotName.Name = "lblRobotName";
            this.lblRobotName.Size = new System.Drawing.Size(180, 26);
            this.lblRobotName.TabIndex = 40;
            this.lblRobotName.Text = "Robot Name";
            this.lblRobotName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRemark
            // 
            this.lblRemark.BackColor = System.Drawing.Color.Black;
            this.lblRemark.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRemark.ForeColor = System.Drawing.Color.White;
            this.lblRemark.Location = new System.Drawing.Point(3, 131);
            this.lblRemark.Name = "lblRemark";
            this.lblRemark.Size = new System.Drawing.Size(180, 26);
            this.lblRemark.TabIndex = 39;
            this.lblRemark.Text = "Remark";
            this.lblRemark.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtRemark
            // 
            this.txtRemark.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtRemark.Location = new System.Drawing.Point(183, 130);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Size = new System.Drawing.Size(640, 28);
            this.txtRemark.TabIndex = 4;
            // 
            // chkIsEnable
            // 
            this.chkIsEnable.AutoSize = true;
            this.chkIsEnable.Checked = true;
            this.chkIsEnable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIsEnable.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkIsEnable.Location = new System.Drawing.Point(449, 6);
            this.chkIsEnable.Name = "chkIsEnable";
            this.chkIsEnable.Size = new System.Drawing.Size(106, 26);
            this.chkIsEnable.TabIndex = 6;
            this.chkIsEnable.Text = "Is Enable ?";
            this.chkIsEnable.UseVisualStyleBackColor = true;
            this.chkIsEnable.CheckedChanged += new System.EventHandler(this.chkIsEnable_CheckedChanged);
            // 
            // lblDescription
            // 
            this.lblDescription.BackColor = System.Drawing.Color.Black;
            this.lblDescription.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblDescription.ForeColor = System.Drawing.Color.White;
            this.lblDescription.Location = new System.Drawing.Point(3, 162);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(180, 26);
            this.lblDescription.TabIndex = 38;
            this.lblDescription.Text = "Description";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtDescription.Location = new System.Drawing.Point(183, 161);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(640, 70);
            this.txtDescription.TabIndex = 5;
            // 
            // FormRobotRouteMSTEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(841, 583);
            this.Name = "FormRobotRouteMSTEdit";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormRobotRouteMSTEdit_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpRouteCondition.ResumeLayout(false);
            this.pnlGridView.ResumeLayout(false);
            this.flpBase.ResumeLayout(false);
            this.pnlAdd.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.pnlButton.ResumeLayout(false);
            this.pnlNew.ResumeLayout(false);
            this.grbBase.ResumeLayout(false);
            this.pnlNewObject.ResumeLayout(false);
            this.pnlNewObject.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpRouteCondition;
        private System.Windows.Forms.Panel pnlGridView;
        private System.Windows.Forms.Panel pnlNew;
        private System.Windows.Forms.FlowLayoutPanel flpBase;
        private System.Windows.Forms.Panel pnlAdd;
        private System.Windows.Forms.Button btnAddNew;
        private System.Windows.Forms.Button btnNewClose;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox grbBase;
        private System.Windows.Forms.Panel pnlNewObject;
        private System.Windows.Forms.TextBox txtRouteName;
        private System.Windows.Forms.Label lblRouteName;
        private System.Windows.Forms.TextBox txtRoubeID;
        private System.Windows.Forms.Label lblRouteID;
        private System.Windows.Forms.Label lblRobotName;
        private System.Windows.Forms.Label lblRemark;
        private System.Windows.Forms.TextBox txtRemark;
        private System.Windows.Forms.CheckBox chkIsEnable;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.ComboBox cboRobotName;
        private System.Windows.Forms.CheckBox chkRTCModeFlag;
        private System.Windows.Forms.TextBox txtRoutePriority;
        private System.Windows.Forms.Label lblRoutePriority;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkRTCForceReturnFlag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRouteID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRouteName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineType;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsEnabled;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colRTCModeFlag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDesciption;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemarks;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastUpdateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRoutePriority;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colRTCForceReturnFlag;

    }
}