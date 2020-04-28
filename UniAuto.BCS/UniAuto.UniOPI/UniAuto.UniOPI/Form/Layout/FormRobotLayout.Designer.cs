namespace UniOPI
{
    partial class FormRobotLayout
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.flpRobotInfo = new System.Windows.Forms.FlowLayoutPanel();
            this.flpRobotPic = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlRobotPic = new System.Windows.Forms.Panel();
            this.btnRefreshStage = new System.Windows.Forms.Button();
            this.flpRobot = new System.Windows.Forms.FlowLayoutPanel();
            this.flpStage = new System.Windows.Forms.FlowLayoutPanel();
            this.flpPortStage = new System.Windows.Forms.FlowLayoutPanel();
            this.lstRobotCommand = new System.Windows.Forms.ListBox();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.tabInfo = new System.Windows.Forms.TabControl();
            this.tpRobotMessage = new System.Windows.Forms.TabPage();
            this.lsvRobotCommand = new System.Windows.Forms.ListView();
            this.colCommandType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRobotCommand = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlCommandButton = new System.Windows.Forms.Panel();
            this.btnClearCommand = new System.Windows.Forms.Button();
            this.btnQueryCommand = new System.Windows.Forms.Button();
            this.tpUnloaderDispatch = new System.Windows.Forms.TabPage();
            this.dgvDispatch = new System.Windows.Forms.DataGridView();
            this.colDispatch_PortKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDispatch_PortID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDispatch_Grade01 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDispatch_Grade02 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDispatch_Grade03 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDispatch_OperatorID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tpRBStatus = new System.Windows.Forms.TabPage();
            this.grbText = new System.Windows.Forms.GroupBox();
            this.pnlTrack_Skip = new System.Windows.Forms.Panel();
            this.lblTrack_Skip = new System.Windows.Forms.Label();
            this.pnlTrack_Abnormal = new System.Windows.Forms.Panel();
            this.pnlTrack_NoProc = new System.Windows.Forms.Panel();
            this.lblTrack_NoProc = new System.Windows.Forms.Label();
            this.lblTrack_Abnormal = new System.Windows.Forms.Label();
            this.pnlTrack_Normal = new System.Windows.Forms.Panel();
            this.lblTrack_Normal = new System.Windows.Forms.Label();
            this.grbArmLabel = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlArm_Exist = new System.Windows.Forms.Panel();
            this.pnlArm_unknown = new System.Windows.Forms.Panel();
            this.lblArm_unknown = new System.Windows.Forms.Label();
            this.lblArm_Exist = new System.Windows.Forms.Label();
            this.pnlArm_NoExist = new System.Windows.Forms.Panel();
            this.lblArm_NoExist = new System.Windows.Forms.Label();
            this.grbStageLabel = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlStage_NoReq = new System.Windows.Forms.Panel();
            this.pnlStage_UDRQ = new System.Windows.Forms.Panel();
            this.pnlStage_Unknown = new System.Windows.Forms.Panel();
            this.lblStage_Unknown = new System.Windows.Forms.Label();
            this.lblStage_UDRQ = new System.Windows.Forms.Label();
            this.pnlStage_LDRQ = new System.Windows.Forms.Panel();
            this.lblStage_NoReq = new System.Windows.Forms.Label();
            this.lblStage_LDRQ = new System.Windows.Forms.Label();
            this.grbRobotCommand = new System.Windows.Forms.GroupBox();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.flpRobotInfo.SuspendLayout();
            this.flpRobotPic.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.tabInfo.SuspendLayout();
            this.tpRobotMessage.SuspendLayout();
            this.pnlCommandButton.SuspendLayout();
            this.tpUnloaderDispatch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDispatch)).BeginInit();
            this.tpRBStatus.SuspendLayout();
            this.grbText.SuspendLayout();
            this.grbArmLabel.SuspendLayout();
            this.grbStageLabel.SuspendLayout();
            this.grbRobotCommand.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1030, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(1090, 628);
            // 
            // tmrBaseRefresh
            // 
            this.tmrBaseRefresh.Interval = 2000;
            // 
            // flpRobotInfo
            // 
            this.flpRobotInfo.AutoScroll = true;
            this.flpRobotInfo.Controls.Add(this.flpRobotPic);
            this.flpRobotInfo.Controls.Add(this.flpRobot);
            this.flpRobotInfo.Controls.Add(this.flpStage);
            this.flpRobotInfo.Controls.Add(this.flpPortStage);
            this.flpRobotInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpRobotInfo.Location = new System.Drawing.Point(4, 3);
            this.flpRobotInfo.Name = "flpRobotInfo";
            this.flpRobotInfo.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.flpRobotInfo.Size = new System.Drawing.Size(1082, 293);
            this.flpRobotInfo.TabIndex = 36;
            this.flpRobotInfo.WrapContents = false;
            // 
            // flpRobotPic
            // 
            this.flpRobotPic.Controls.Add(this.pnlRobotPic);
            this.flpRobotPic.Controls.Add(this.btnRefreshStage);
            this.flpRobotPic.Location = new System.Drawing.Point(1, 3);
            this.flpRobotPic.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.flpRobotPic.Name = "flpRobotPic";
            this.flpRobotPic.Size = new System.Drawing.Size(245, 250);
            this.flpRobotPic.TabIndex = 40;
            // 
            // pnlRobotPic
            // 
            this.pnlRobotPic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlRobotPic.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlRobotPic.Location = new System.Drawing.Point(3, 3);
            this.pnlRobotPic.Name = "pnlRobotPic";
            this.pnlRobotPic.Size = new System.Drawing.Size(245, 170);
            this.pnlRobotPic.TabIndex = 38;
            // 
            // btnRefreshStage
            // 
            this.btnRefreshStage.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefreshStage.Location = new System.Drawing.Point(100, 179);
            this.btnRefreshStage.Margin = new System.Windows.Forms.Padding(100, 3, 3, 3);
            this.btnRefreshStage.Name = "btnRefreshStage";
            this.btnRefreshStage.Size = new System.Drawing.Size(140, 30);
            this.btnRefreshStage.TabIndex = 0;
            this.btnRefreshStage.Text = "Refresh Stage Info";
            this.btnRefreshStage.UseVisualStyleBackColor = true;
            this.btnRefreshStage.Click += new System.EventHandler(this.btnRefreshStage_Click);
            // 
            // flpRobot
            // 
            this.flpRobot.Location = new System.Drawing.Point(248, 3);
            this.flpRobot.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.flpRobot.Name = "flpRobot";
            this.flpRobot.Size = new System.Drawing.Size(290, 250);
            this.flpRobot.TabIndex = 0;
            // 
            // flpStage
            // 
            this.flpStage.AutoScroll = true;
            this.flpStage.Location = new System.Drawing.Point(540, 3);
            this.flpStage.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.flpStage.Name = "flpStage";
            this.flpStage.Size = new System.Drawing.Size(290, 250);
            this.flpStage.TabIndex = 1;
            // 
            // flpPortStage
            // 
            this.flpPortStage.AutoScroll = true;
            this.flpPortStage.Location = new System.Drawing.Point(832, 3);
            this.flpPortStage.Margin = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.flpPortStage.Name = "flpPortStage";
            this.flpPortStage.Size = new System.Drawing.Size(290, 250);
            this.flpPortStage.TabIndex = 39;
            // 
            // lstRobotCommand
            // 
            this.lstRobotCommand.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.lstRobotCommand.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstRobotCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstRobotCommand.FormattingEnabled = true;
            this.lstRobotCommand.ItemHeight = 17;
            this.lstRobotCommand.Location = new System.Drawing.Point(3, 21);
            this.lstRobotCommand.Margin = new System.Windows.Forms.Padding(0);
            this.lstRobotCommand.Name = "lstRobotCommand";
            this.lstRobotCommand.Size = new System.Drawing.Size(1076, 28);
            this.lstRobotCommand.TabIndex = 0;
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 3;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.tlpBase.Controls.Add(this.flpRobotInfo, 1, 0);
            this.tlpBase.Controls.Add(this.tabInfo, 1, 2);
            this.tlpBase.Controls.Add(this.grbRobotCommand, 1, 1);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 3;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 240F));
            this.tlpBase.Size = new System.Drawing.Size(1090, 597);
            this.tlpBase.TabIndex = 0;
            // 
            // tabInfo
            // 
            this.tabInfo.Controls.Add(this.tpRobotMessage);
            this.tabInfo.Controls.Add(this.tpUnloaderDispatch);
            this.tabInfo.Controls.Add(this.tpRBStatus);
            this.tabInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabInfo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabInfo.Location = new System.Drawing.Point(4, 360);
            this.tabInfo.Name = "tabInfo";
            this.tabInfo.SelectedIndex = 0;
            this.tabInfo.Size = new System.Drawing.Size(1082, 234);
            this.tabInfo.TabIndex = 36;
            // 
            // tpRobotMessage
            // 
            this.tpRobotMessage.BackColor = System.Drawing.Color.Gray;
            this.tpRobotMessage.Controls.Add(this.lsvRobotCommand);
            this.tpRobotMessage.Controls.Add(this.pnlCommandButton);
            this.tpRobotMessage.Location = new System.Drawing.Point(4, 30);
            this.tpRobotMessage.Name = "tpRobotMessage";
            this.tpRobotMessage.Padding = new System.Windows.Forms.Padding(3);
            this.tpRobotMessage.Size = new System.Drawing.Size(1074, 190);
            this.tpRobotMessage.TabIndex = 5;
            this.tpRobotMessage.Text = "Robot Message";
            // 
            // lsvRobotCommand
            // 
            this.lsvRobotCommand.BackColor = System.Drawing.SystemColors.Window;
            this.lsvRobotCommand.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colCommandType,
            this.colRobotCommand});
            this.lsvRobotCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lsvRobotCommand.Font = new System.Drawing.Font("Courier New", 9F);
            this.lsvRobotCommand.FullRowSelect = true;
            this.lsvRobotCommand.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lsvRobotCommand.Location = new System.Drawing.Point(3, 3);
            this.lsvRobotCommand.Name = "lsvRobotCommand";
            this.lsvRobotCommand.Size = new System.Drawing.Size(1068, 156);
            this.lsvRobotCommand.TabIndex = 21;
            this.lsvRobotCommand.UseCompatibleStateImageBehavior = false;
            this.lsvRobotCommand.View = System.Windows.Forms.View.Details;
            // 
            // colCommandType
            // 
            this.colCommandType.Text = "Command Type";
            this.colCommandType.Width = 0;
            // 
            // colRobotCommand
            // 
            this.colRobotCommand.Width = 2000;
            // 
            // pnlCommandButton
            // 
            this.pnlCommandButton.Controls.Add(this.btnClearCommand);
            this.pnlCommandButton.Controls.Add(this.btnQueryCommand);
            this.pnlCommandButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlCommandButton.Location = new System.Drawing.Point(3, 159);
            this.pnlCommandButton.Name = "pnlCommandButton";
            this.pnlCommandButton.Size = new System.Drawing.Size(1068, 28);
            this.pnlCommandButton.TabIndex = 20;
            // 
            // btnClearCommand
            // 
            this.btnClearCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClearCommand.Location = new System.Drawing.Point(0, 0);
            this.btnClearCommand.Margin = new System.Windows.Forms.Padding(0);
            this.btnClearCommand.Name = "btnClearCommand";
            this.btnClearCommand.Size = new System.Drawing.Size(1038, 28);
            this.btnClearCommand.TabIndex = 20;
            this.btnClearCommand.Text = "Clear Command";
            this.btnClearCommand.UseVisualStyleBackColor = true;
            this.btnClearCommand.Click += new System.EventHandler(this.btnClearCommand_Click);
            // 
            // btnQueryCommand
            // 
            this.btnQueryCommand.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnQueryCommand.Location = new System.Drawing.Point(1038, 0);
            this.btnQueryCommand.Margin = new System.Windows.Forms.Padding(0);
            this.btnQueryCommand.Name = "btnQueryCommand";
            this.btnQueryCommand.Size = new System.Drawing.Size(30, 28);
            this.btnQueryCommand.TabIndex = 19;
            this.btnQueryCommand.Text = "History";
            this.btnQueryCommand.UseVisualStyleBackColor = true;
            this.btnQueryCommand.Click += new System.EventHandler(this.btnQueryCommand_Click);
            // 
            // tpUnloaderDispatch
            // 
            this.tpUnloaderDispatch.Controls.Add(this.dgvDispatch);
            this.tpUnloaderDispatch.Location = new System.Drawing.Point(4, 30);
            this.tpUnloaderDispatch.Name = "tpUnloaderDispatch";
            this.tpUnloaderDispatch.Padding = new System.Windows.Forms.Padding(3);
            this.tpUnloaderDispatch.Size = new System.Drawing.Size(1244, 190);
            this.tpUnloaderDispatch.TabIndex = 7;
            this.tpUnloaderDispatch.Text = "UD Dispatch";
            this.tpUnloaderDispatch.UseVisualStyleBackColor = true;
            // 
            // dgvDispatch
            // 
            this.dgvDispatch.AllowUserToAddRows = false;
            this.dgvDispatch.AllowUserToDeleteRows = false;
            this.dgvDispatch.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvDispatch.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDispatch.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvDispatch.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDispatch.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDispatch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDispatch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDispatch_PortKey,
            this.colDispatch_PortID,
            this.colDispatch_Grade01,
            this.colDispatch_Grade02,
            this.colDispatch_Grade03,
            this.colDispatch_OperatorID});
            this.dgvDispatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDispatch.Location = new System.Drawing.Point(3, 3);
            this.dgvDispatch.Name = "dgvDispatch";
            this.dgvDispatch.ReadOnly = true;
            this.dgvDispatch.RowHeadersVisible = false;
            this.dgvDispatch.RowTemplate.Height = 24;
            this.dgvDispatch.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDispatch.Size = new System.Drawing.Size(1238, 184);
            this.dgvDispatch.TabIndex = 9;
            // 
            // colDispatch_PortKey
            // 
            this.colDispatch_PortKey.HeaderText = "Port Key";
            this.colDispatch_PortKey.Name = "colDispatch_PortKey";
            this.colDispatch_PortKey.ReadOnly = true;
            this.colDispatch_PortKey.Visible = false;
            this.colDispatch_PortKey.Width = 75;
            // 
            // colDispatch_PortID
            // 
            this.colDispatch_PortID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDispatch_PortID.HeaderText = "Port ID";
            this.colDispatch_PortID.Name = "colDispatch_PortID";
            this.colDispatch_PortID.ReadOnly = true;
            // 
            // colDispatch_Grade01
            // 
            this.colDispatch_Grade01.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDispatch_Grade01.HeaderText = "Grade 01";
            this.colDispatch_Grade01.Name = "colDispatch_Grade01";
            this.colDispatch_Grade01.ReadOnly = true;
            this.colDispatch_Grade01.Width = 200;
            // 
            // colDispatch_Grade02
            // 
            this.colDispatch_Grade02.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDispatch_Grade02.HeaderText = "Grade 02";
            this.colDispatch_Grade02.MinimumWidth = 90;
            this.colDispatch_Grade02.Name = "colDispatch_Grade02";
            this.colDispatch_Grade02.ReadOnly = true;
            this.colDispatch_Grade02.Width = 200;
            // 
            // colDispatch_Grade03
            // 
            this.colDispatch_Grade03.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDispatch_Grade03.HeaderText = "Grade 03";
            this.colDispatch_Grade03.MinimumWidth = 90;
            this.colDispatch_Grade03.Name = "colDispatch_Grade03";
            this.colDispatch_Grade03.ReadOnly = true;
            this.colDispatch_Grade03.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colDispatch_Grade03.Width = 200;
            // 
            // colDispatch_OperatorID
            // 
            this.colDispatch_OperatorID.HeaderText = "Operator ID";
            this.colDispatch_OperatorID.Name = "colDispatch_OperatorID";
            this.colDispatch_OperatorID.ReadOnly = true;
            this.colDispatch_OperatorID.Width = 116;
            // 
            // tpRBStatus
            // 
            this.tpRBStatus.BackColor = System.Drawing.Color.Gray;
            this.tpRBStatus.Controls.Add(this.grbText);
            this.tpRBStatus.Controls.Add(this.grbArmLabel);
            this.tpRBStatus.Controls.Add(this.grbStageLabel);
            this.tpRBStatus.Location = new System.Drawing.Point(4, 30);
            this.tpRBStatus.Name = "tpRBStatus";
            this.tpRBStatus.Padding = new System.Windows.Forms.Padding(3);
            this.tpRBStatus.Size = new System.Drawing.Size(1074, 200);
            this.tpRBStatus.TabIndex = 6;
            this.tpRBStatus.Text = "Arm/Stage Status";
            // 
            // grbText
            // 
            this.grbText.Controls.Add(this.pnlTrack_Skip);
            this.grbText.Controls.Add(this.lblTrack_Skip);
            this.grbText.Controls.Add(this.pnlTrack_Abnormal);
            this.grbText.Controls.Add(this.pnlTrack_NoProc);
            this.grbText.Controls.Add(this.lblTrack_NoProc);
            this.grbText.Controls.Add(this.lblTrack_Abnormal);
            this.grbText.Controls.Add(this.pnlTrack_Normal);
            this.grbText.Controls.Add(this.lblTrack_Normal);
            this.grbText.ForeColor = System.Drawing.Color.White;
            this.grbText.Location = new System.Drawing.Point(470, 3);
            this.grbText.Name = "grbText";
            this.grbText.Size = new System.Drawing.Size(468, 95);
            this.grbText.TabIndex = 24;
            this.grbText.TabStop = false;
            this.grbText.Text = "Stage / Arm TextBox Color";
            // 
            // pnlTrack_Skip
            // 
            this.pnlTrack_Skip.BackColor = System.Drawing.Color.Gray;
            this.pnlTrack_Skip.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlTrack_Skip.Location = new System.Drawing.Point(253, 25);
            this.pnlTrack_Skip.Name = "pnlTrack_Skip";
            this.pnlTrack_Skip.Size = new System.Drawing.Size(50, 50);
            this.pnlTrack_Skip.TabIndex = 21;
            // 
            // lblTrack_Skip
            // 
            this.lblTrack_Skip.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrack_Skip.ForeColor = System.Drawing.Color.White;
            this.lblTrack_Skip.Location = new System.Drawing.Point(226, 72);
            this.lblTrack_Skip.Name = "lblTrack_Skip";
            this.lblTrack_Skip.Size = new System.Drawing.Size(100, 21);
            this.lblTrack_Skip.TabIndex = 22;
            this.lblTrack_Skip.Text = "Process Skip";
            this.lblTrack_Skip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlTrack_Abnormal
            // 
            this.pnlTrack_Abnormal.BackColor = System.Drawing.Color.Orange;
            this.pnlTrack_Abnormal.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlTrack_Abnormal.Location = new System.Drawing.Point(365, 25);
            this.pnlTrack_Abnormal.Name = "pnlTrack_Abnormal";
            this.pnlTrack_Abnormal.Size = new System.Drawing.Size(50, 50);
            this.pnlTrack_Abnormal.TabIndex = 11;
            // 
            // pnlTrack_NoProc
            // 
            this.pnlTrack_NoProc.BackColor = System.Drawing.Color.Yellow;
            this.pnlTrack_NoProc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlTrack_NoProc.Location = new System.Drawing.Point(30, 25);
            this.pnlTrack_NoProc.Name = "pnlTrack_NoProc";
            this.pnlTrack_NoProc.Size = new System.Drawing.Size(50, 50);
            this.pnlTrack_NoProc.TabIndex = 19;
            // 
            // lblTrack_NoProc
            // 
            this.lblTrack_NoProc.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrack_NoProc.ForeColor = System.Drawing.Color.White;
            this.lblTrack_NoProc.Location = new System.Drawing.Point(3, 72);
            this.lblTrack_NoProc.Name = "lblTrack_NoProc";
            this.lblTrack_NoProc.Size = new System.Drawing.Size(102, 21);
            this.lblTrack_NoProc.TabIndex = 20;
            this.lblTrack_NoProc.Text = "Not Processed";
            this.lblTrack_NoProc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTrack_Abnormal
            // 
            this.lblTrack_Abnormal.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrack_Abnormal.ForeColor = System.Drawing.Color.White;
            this.lblTrack_Abnormal.Location = new System.Drawing.Point(320, 72);
            this.lblTrack_Abnormal.Name = "lblTrack_Abnormal";
            this.lblTrack_Abnormal.Size = new System.Drawing.Size(145, 21);
            this.lblTrack_Abnormal.TabIndex = 14;
            this.lblTrack_Abnormal.Text = "Abnormal Processed";
            this.lblTrack_Abnormal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlTrack_Normal
            // 
            this.pnlTrack_Normal.BackColor = System.Drawing.Color.Green;
            this.pnlTrack_Normal.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlTrack_Normal.Location = new System.Drawing.Point(139, 25);
            this.pnlTrack_Normal.Name = "pnlTrack_Normal";
            this.pnlTrack_Normal.Size = new System.Drawing.Size(50, 50);
            this.pnlTrack_Normal.TabIndex = 10;
            // 
            // lblTrack_Normal
            // 
            this.lblTrack_Normal.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrack_Normal.ForeColor = System.Drawing.Color.White;
            this.lblTrack_Normal.Location = new System.Drawing.Point(94, 72);
            this.lblTrack_Normal.Name = "lblTrack_Normal";
            this.lblTrack_Normal.Size = new System.Drawing.Size(148, 21);
            this.lblTrack_Normal.TabIndex = 15;
            this.lblTrack_Normal.Text = "Normal Processed";
            this.lblTrack_Normal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grbArmLabel
            // 
            this.grbArmLabel.Controls.Add(this.label4);
            this.grbArmLabel.Controls.Add(this.label3);
            this.grbArmLabel.Controls.Add(this.label2);
            this.grbArmLabel.Controls.Add(this.panel4);
            this.grbArmLabel.Controls.Add(this.panel3);
            this.grbArmLabel.Controls.Add(this.panel2);
            this.grbArmLabel.Controls.Add(this.pnlArm_Exist);
            this.grbArmLabel.Controls.Add(this.pnlArm_unknown);
            this.grbArmLabel.Controls.Add(this.lblArm_unknown);
            this.grbArmLabel.Controls.Add(this.lblArm_Exist);
            this.grbArmLabel.Controls.Add(this.pnlArm_NoExist);
            this.grbArmLabel.Controls.Add(this.lblArm_NoExist);
            this.grbArmLabel.ForeColor = System.Drawing.Color.White;
            this.grbArmLabel.Location = new System.Drawing.Point(15, 100);
            this.grbArmLabel.Name = "grbArmLabel";
            this.grbArmLabel.Size = new System.Drawing.Size(923, 95);
            this.grbArmLabel.TabIndex = 23;
            this.grbArmLabel.TabStop = false;
            this.grbArmLabel.Text = "Arm Label Color";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(527, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(155, 21);
            this.label4.TabIndex = 26;
            this.label4.Text = "Arm Disable,Job Exist";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(347, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(190, 21);
            this.label3.TabIndex = 25;
            this.label3.Text = "Arm Disabled,Job No Exist";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(250, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 21);
            this.label2.TabIndex = 24;
            this.label2.Text = "Arm Disabled";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.Fuchsia;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel4.Location = new System.Drawing.Point(587, 22);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(50, 50);
            this.panel4.TabIndex = 23;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Orange;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel3.Location = new System.Drawing.Point(426, 22);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(50, 50);
            this.panel3.TabIndex = 22;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Blue;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Location = new System.Drawing.Point(277, 22);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(50, 50);
            this.panel2.TabIndex = 21;
            // 
            // pnlArm_Exist
            // 
            this.pnlArm_Exist.BackColor = System.Drawing.Color.Green;
            this.pnlArm_Exist.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlArm_Exist.Location = new System.Drawing.Point(190, 22);
            this.pnlArm_Exist.Name = "pnlArm_Exist";
            this.pnlArm_Exist.Size = new System.Drawing.Size(50, 50);
            this.pnlArm_Exist.TabIndex = 11;
            // 
            // pnlArm_unknown
            // 
            this.pnlArm_unknown.BackColor = System.Drawing.Color.Firebrick;
            this.pnlArm_unknown.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlArm_unknown.Location = new System.Drawing.Point(30, 22);
            this.pnlArm_unknown.Name = "pnlArm_unknown";
            this.pnlArm_unknown.Size = new System.Drawing.Size(50, 50);
            this.pnlArm_unknown.TabIndex = 19;
            // 
            // lblArm_unknown
            // 
            this.lblArm_unknown.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblArm_unknown.ForeColor = System.Drawing.Color.White;
            this.lblArm_unknown.Location = new System.Drawing.Point(14, 69);
            this.lblArm_unknown.Name = "lblArm_unknown";
            this.lblArm_unknown.Size = new System.Drawing.Size(78, 21);
            this.lblArm_unknown.TabIndex = 20;
            this.lblArm_unknown.Text = "Unknown";
            this.lblArm_unknown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblArm_Exist
            // 
            this.lblArm_Exist.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblArm_Exist.ForeColor = System.Drawing.Color.White;
            this.lblArm_Exist.Location = new System.Drawing.Point(171, 69);
            this.lblArm_Exist.Name = "lblArm_Exist";
            this.lblArm_Exist.Size = new System.Drawing.Size(85, 21);
            this.lblArm_Exist.TabIndex = 14;
            this.lblArm_Exist.Text = "Job Exist";
            this.lblArm_Exist.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlArm_NoExist
            // 
            this.pnlArm_NoExist.BackColor = System.Drawing.Color.DimGray;
            this.pnlArm_NoExist.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlArm_NoExist.Location = new System.Drawing.Point(109, 22);
            this.pnlArm_NoExist.Name = "pnlArm_NoExist";
            this.pnlArm_NoExist.Size = new System.Drawing.Size(50, 50);
            this.pnlArm_NoExist.TabIndex = 10;
            // 
            // lblArm_NoExist
            // 
            this.lblArm_NoExist.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblArm_NoExist.ForeColor = System.Drawing.Color.White;
            this.lblArm_NoExist.Location = new System.Drawing.Point(92, 69);
            this.lblArm_NoExist.Name = "lblArm_NoExist";
            this.lblArm_NoExist.Size = new System.Drawing.Size(85, 21);
            this.lblArm_NoExist.TabIndex = 15;
            this.lblArm_NoExist.Text = "Job NoExist";
            this.lblArm_NoExist.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grbStageLabel
            // 
            this.grbStageLabel.Controls.Add(this.panel1);
            this.grbStageLabel.Controls.Add(this.label1);
            this.grbStageLabel.Controls.Add(this.pnlStage_NoReq);
            this.grbStageLabel.Controls.Add(this.pnlStage_UDRQ);
            this.grbStageLabel.Controls.Add(this.pnlStage_Unknown);
            this.grbStageLabel.Controls.Add(this.lblStage_Unknown);
            this.grbStageLabel.Controls.Add(this.lblStage_UDRQ);
            this.grbStageLabel.Controls.Add(this.pnlStage_LDRQ);
            this.grbStageLabel.Controls.Add(this.lblStage_NoReq);
            this.grbStageLabel.Controls.Add(this.lblStage_LDRQ);
            this.grbStageLabel.ForeColor = System.Drawing.Color.White;
            this.grbStageLabel.Location = new System.Drawing.Point(15, 3);
            this.grbStageLabel.Name = "grbStageLabel";
            this.grbStageLabel.Size = new System.Drawing.Size(440, 95);
            this.grbStageLabel.TabIndex = 21;
            this.grbStageLabel.TabStop = false;
            this.grbStageLabel.Text = "Stage Label Color";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.MediumBlue;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Location = new System.Drawing.Point(269, 25);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(50, 50);
            this.panel1.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(245, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 21);
            this.label1.TabIndex = 15;
            this.label1.Text = "LDRQ_UDRQ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlStage_NoReq
            // 
            this.pnlStage_NoReq.BackColor = System.Drawing.Color.MediumTurquoise;
            this.pnlStage_NoReq.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlStage_NoReq.Location = new System.Drawing.Point(361, 25);
            this.pnlStage_NoReq.Name = "pnlStage_NoReq";
            this.pnlStage_NoReq.Size = new System.Drawing.Size(50, 50);
            this.pnlStage_NoReq.TabIndex = 9;
            // 
            // pnlStage_UDRQ
            // 
            this.pnlStage_UDRQ.BackColor = System.Drawing.Color.Green;
            this.pnlStage_UDRQ.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlStage_UDRQ.Location = new System.Drawing.Point(190, 25);
            this.pnlStage_UDRQ.Name = "pnlStage_UDRQ";
            this.pnlStage_UDRQ.Size = new System.Drawing.Size(50, 50);
            this.pnlStage_UDRQ.TabIndex = 11;
            // 
            // pnlStage_Unknown
            // 
            this.pnlStage_Unknown.BackColor = System.Drawing.Color.Firebrick;
            this.pnlStage_Unknown.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlStage_Unknown.Location = new System.Drawing.Point(30, 25);
            this.pnlStage_Unknown.Name = "pnlStage_Unknown";
            this.pnlStage_Unknown.Size = new System.Drawing.Size(50, 50);
            this.pnlStage_Unknown.TabIndex = 19;
            // 
            // lblStage_Unknown
            // 
            this.lblStage_Unknown.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStage_Unknown.ForeColor = System.Drawing.Color.White;
            this.lblStage_Unknown.Location = new System.Drawing.Point(14, 72);
            this.lblStage_Unknown.Name = "lblStage_Unknown";
            this.lblStage_Unknown.Size = new System.Drawing.Size(78, 21);
            this.lblStage_Unknown.TabIndex = 20;
            this.lblStage_Unknown.Text = "Unknown";
            this.lblStage_Unknown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStage_UDRQ
            // 
            this.lblStage_UDRQ.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStage_UDRQ.ForeColor = System.Drawing.Color.White;
            this.lblStage_UDRQ.Location = new System.Drawing.Point(171, 72);
            this.lblStage_UDRQ.Name = "lblStage_UDRQ";
            this.lblStage_UDRQ.Size = new System.Drawing.Size(85, 21);
            this.lblStage_UDRQ.TabIndex = 14;
            this.lblStage_UDRQ.Text = "UDRQ";
            this.lblStage_UDRQ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlStage_LDRQ
            // 
            this.pnlStage_LDRQ.BackColor = System.Drawing.Color.DimGray;
            this.pnlStage_LDRQ.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlStage_LDRQ.Location = new System.Drawing.Point(109, 25);
            this.pnlStage_LDRQ.Name = "pnlStage_LDRQ";
            this.pnlStage_LDRQ.Size = new System.Drawing.Size(50, 50);
            this.pnlStage_LDRQ.TabIndex = 10;
            // 
            // lblStage_NoReq
            // 
            this.lblStage_NoReq.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStage_NoReq.ForeColor = System.Drawing.Color.White;
            this.lblStage_NoReq.Location = new System.Drawing.Point(342, 72);
            this.lblStage_NoReq.Name = "lblStage_NoReq";
            this.lblStage_NoReq.Size = new System.Drawing.Size(85, 21);
            this.lblStage_NoReq.TabIndex = 13;
            this.lblStage_NoReq.Text = "No Request";
            this.lblStage_NoReq.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStage_LDRQ
            // 
            this.lblStage_LDRQ.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStage_LDRQ.ForeColor = System.Drawing.Color.White;
            this.lblStage_LDRQ.Location = new System.Drawing.Point(92, 72);
            this.lblStage_LDRQ.Name = "lblStage_LDRQ";
            this.lblStage_LDRQ.Size = new System.Drawing.Size(85, 21);
            this.lblStage_LDRQ.TabIndex = 15;
            this.lblStage_LDRQ.Text = "LDRQ";
            this.lblStage_LDRQ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grbRobotCommand
            // 
            this.grbRobotCommand.Controls.Add(this.lstRobotCommand);
            this.grbRobotCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbRobotCommand.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.grbRobotCommand.Location = new System.Drawing.Point(4, 302);
            this.grbRobotCommand.Name = "grbRobotCommand";
            this.grbRobotCommand.Size = new System.Drawing.Size(1082, 52);
            this.grbRobotCommand.TabIndex = 37;
            this.grbRobotCommand.TabStop = false;
            this.grbRobotCommand.Text = "Robot Command";
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 2000;
            // 
            // FormRobotLayout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1090, 628);
            this.Name = "FormRobotLayout";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormRobotLayout_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.flpRobotInfo.ResumeLayout(false);
            this.flpRobotPic.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.tabInfo.ResumeLayout(false);
            this.tpRobotMessage.ResumeLayout(false);
            this.pnlCommandButton.ResumeLayout(false);
            this.tpUnloaderDispatch.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDispatch)).EndInit();
            this.tpRBStatus.ResumeLayout(false);
            this.grbText.ResumeLayout(false);
            this.grbArmLabel.ResumeLayout(false);
            this.grbStageLabel.ResumeLayout(false);
            this.grbRobotCommand.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.FlowLayoutPanel flpRobotInfo;
        private System.Windows.Forms.FlowLayoutPanel flpRobot;
        private System.Windows.Forms.FlowLayoutPanel flpStage;
        private System.Windows.Forms.Panel pnlRobotPic;
        private System.Windows.Forms.FlowLayoutPanel flpPortStage;
        private System.Windows.Forms.TabControl tabInfo;
        private System.Windows.Forms.TabPage tpRobotMessage;
        private System.Windows.Forms.TabPage tpRBStatus;
        private System.Windows.Forms.GroupBox grbText;
        private System.Windows.Forms.Panel pnlTrack_Skip;
        private System.Windows.Forms.Label lblTrack_Skip;
        private System.Windows.Forms.Panel pnlTrack_Abnormal;
        private System.Windows.Forms.Panel pnlTrack_NoProc;
        private System.Windows.Forms.Label lblTrack_NoProc;
        private System.Windows.Forms.Label lblTrack_Abnormal;
        private System.Windows.Forms.Panel pnlTrack_Normal;
        private System.Windows.Forms.Label lblTrack_Normal;
        private System.Windows.Forms.GroupBox grbArmLabel;
        private System.Windows.Forms.Panel pnlArm_Exist;
        private System.Windows.Forms.Panel pnlArm_unknown;
        private System.Windows.Forms.Label lblArm_unknown;
        private System.Windows.Forms.Label lblArm_Exist;
        private System.Windows.Forms.Panel pnlArm_NoExist;
        private System.Windows.Forms.Label lblArm_NoExist;
        private System.Windows.Forms.GroupBox grbStageLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlStage_NoReq;
        private System.Windows.Forms.Panel pnlStage_UDRQ;
        private System.Windows.Forms.Panel pnlStage_Unknown;
        private System.Windows.Forms.Label lblStage_Unknown;
        private System.Windows.Forms.Label lblStage_UDRQ;
        private System.Windows.Forms.Panel pnlStage_LDRQ;
        private System.Windows.Forms.Label lblStage_NoReq;
        private System.Windows.Forms.Label lblStage_LDRQ;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox grbRobotCommand;
        private System.Windows.Forms.ListBox lstRobotCommand;
        private System.Windows.Forms.TabPage tpUnloaderDispatch;
        private System.Windows.Forms.ListView lsvRobotCommand;
        private System.Windows.Forms.ColumnHeader colCommandType;
        private System.Windows.Forms.ColumnHeader colRobotCommand;
        private System.Windows.Forms.Panel pnlCommandButton;
        private System.Windows.Forms.Button btnClearCommand;
        private System.Windows.Forms.Button btnQueryCommand;
        public System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.FlowLayoutPanel flpRobotPic;
        private System.Windows.Forms.Button btnRefreshStage;
        private System.Windows.Forms.DataGridView dgvDispatch;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDispatch_PortKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDispatch_PortID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDispatch_Grade01;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDispatch_Grade02;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDispatch_Grade03;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDispatch_OperatorID;

    }
}