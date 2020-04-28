namespace UniOPI
{
    partial class FormRobotControl
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
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.flpCommand = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpSemiCaption = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblSemiCaption = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.tlpRobotMode = new System.Windows.Forms.TableLayoutPanel();
            this.grbRobotInfo = new System.Windows.Forms.GroupBox();
            this.flpRobotInfo = new System.Windows.Forms.FlowLayoutPanel();
            this.flpRobotList = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlRobotName = new System.Windows.Forms.Panel();
            this.lblRobotName = new System.Windows.Forms.Label();
            this.txtRobotName = new System.Windows.Forms.TextBox();
            this.pnlControlMode = new System.Windows.Forms.Panel();
            this.lblControlMode = new System.Windows.Forms.Label();
            this.txtControlMode = new System.Windows.Forms.TextBox();
            this.pnlHoldStatus = new System.Windows.Forms.Panel();
            this.lblHoldStatus = new System.Windows.Forms.Label();
            this.txtHoldStatus = new System.Windows.Forms.TextBox();
            this.pnlMixRunFlag = new System.Windows.Forms.Panel();
            this.lblMixRunFlag = new System.Windows.Forms.Label();
            this.txtMixRunFlag = new System.Windows.Forms.TextBox();
            this.pnlRefreshButton = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.grbRobotControlMode = new System.Windows.Forms.GroupBox();
            this.pnlCommand = new System.Windows.Forms.Panel();
            this.pnlMixRunFlag_New = new System.Windows.Forms.Panel();
            this.btnMixRunFlag_New = new System.Windows.Forms.Button();
            this.lblMixRunFlag_New = new System.Windows.Forms.Label();
            this.rdoSameYes = new System.Windows.Forms.RadioButton();
            this.rdoSameNo = new System.Windows.Forms.RadioButton();
            this.pnlHoldStatus_New = new System.Windows.Forms.Panel();
            this.btnHoldStatus_New = new System.Windows.Forms.Button();
            this.lblHoldStatus_New = new System.Windows.Forms.Label();
            this.rdoHold = new System.Windows.Forms.RadioButton();
            this.colRelease = new System.Windows.Forms.RadioButton();
            this.pnlControlMode_New = new System.Windows.Forms.Panel();
            this.lblControlMode_New = new System.Windows.Forms.Label();
            this.rdoAUTO = new System.Windows.Forms.RadioButton();
            this.btnControlMode_New = new System.Windows.Forms.Button();
            this.rdoSEMI = new System.Windows.Forms.RadioButton();
            this.pnlRobotPic = new System.Windows.Forms.Panel();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnSendRobotCommand = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.tlpSemiCaption.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tlpRobotMode.SuspendLayout();
            this.grbRobotInfo.SuspendLayout();
            this.flpRobotInfo.SuspendLayout();
            this.pnlRobotName.SuspendLayout();
            this.pnlControlMode.SuspendLayout();
            this.pnlHoldStatus.SuspendLayout();
            this.pnlMixRunFlag.SuspendLayout();
            this.pnlRefreshButton.SuspendLayout();
            this.grbRobotControlMode.SuspendLayout();
            this.pnlCommand.SuspendLayout();
            this.pnlMixRunFlag_New.SuspendLayout();
            this.pnlHoldStatus_New.SuspendLayout();
            this.pnlControlMode_New.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1070, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(1130, 628);
            // 
            // tmrBaseRefresh
            // 
            this.tmrBaseRefresh.Tick += new System.EventHandler(this.tmrBaseRefresh_Tick);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.flpCommand, 0, 4);
            this.tlpBase.Controls.Add(this.tlpSemiCaption, 0, 3);
            this.tlpBase.Controls.Add(this.pnlHeader, 0, 0);
            this.tlpBase.Controls.Add(this.tlpRobotMode, 0, 1);
            this.tlpBase.Controls.Add(this.pnlButton, 0, 5);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 6;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpBase.Size = new System.Drawing.Size(1130, 597);
            this.tlpBase.TabIndex = 0;
            // 
            // flpCommand
            // 
            this.flpCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpCommand.Location = new System.Drawing.Point(10, 265);
            this.flpCommand.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.flpCommand.Name = "flpCommand";
            this.flpCommand.Size = new System.Drawing.Size(1117, 295);
            this.flpCommand.TabIndex = 33;
            // 
            // tlpSemiCaption
            // 
            this.tlpSemiCaption.ColumnCount = 3;
            this.tlpSemiCaption.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpSemiCaption.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSemiCaption.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpSemiCaption.Controls.Add(this.panel1, 1, 0);
            this.tlpSemiCaption.Controls.Add(this.panel2, 2, 0);
            this.tlpSemiCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSemiCaption.Location = new System.Drawing.Point(3, 225);
            this.tlpSemiCaption.Name = "tlpSemiCaption";
            this.tlpSemiCaption.RowCount = 1;
            this.tlpSemiCaption.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSemiCaption.Size = new System.Drawing.Size(1124, 34);
            this.tlpSemiCaption.TabIndex = 31;
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::UniOPI.Properties.Resources.Bg_BaseCaption;
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.Controls.Add(this.lblSemiCaption);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(30, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1064, 34);
            this.panel1.TabIndex = 0;
            // 
            // lblSemiCaption
            // 
            this.lblSemiCaption.BackColor = System.Drawing.Color.Transparent;
            this.lblSemiCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSemiCaption.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblSemiCaption.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSemiCaption.ForeColor = System.Drawing.Color.White;
            this.lblSemiCaption.Location = new System.Drawing.Point(0, 0);
            this.lblSemiCaption.Name = "lblSemiCaption";
            this.lblSemiCaption.Size = new System.Drawing.Size(1064, 34);
            this.lblSemiCaption.TabIndex = 1;
            this.lblSemiCaption.Text = "Robot Semi Command";
            this.lblSemiCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Location = new System.Drawing.Point(1094, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(30, 30);
            this.panel2.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(30, 30);
            this.panel3.TabIndex = 0;
            // 
            // pnlHeader
            // 
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Location = new System.Drawing.Point(3, 3);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1124, 1);
            this.pnlHeader.TabIndex = 30;
            // 
            // tlpRobotMode
            // 
            this.tlpRobotMode.ColumnCount = 3;
            this.tlpRobotMode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 380F));
            this.tlpRobotMode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 366F));
            this.tlpRobotMode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRobotMode.Controls.Add(this.grbRobotInfo, 1, 0);
            this.tlpRobotMode.Controls.Add(this.grbRobotControlMode, 2, 0);
            this.tlpRobotMode.Controls.Add(this.pnlRobotPic, 0, 0);
            this.tlpRobotMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRobotMode.Font = new System.Drawing.Font("Calibri", 12F);
            this.tlpRobotMode.Location = new System.Drawing.Point(3, 4);
            this.tlpRobotMode.Name = "tlpRobotMode";
            this.tlpRobotMode.RowCount = 1;
            this.tlpRobotMode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRobotMode.Size = new System.Drawing.Size(1124, 214);
            this.tlpRobotMode.TabIndex = 33;
            // 
            // grbRobotInfo
            // 
            this.grbRobotInfo.Controls.Add(this.flpRobotInfo);
            this.grbRobotInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbRobotInfo.Location = new System.Drawing.Point(383, 0);
            this.grbRobotInfo.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.grbRobotInfo.Name = "grbRobotInfo";
            this.grbRobotInfo.Size = new System.Drawing.Size(360, 211);
            this.grbRobotInfo.TabIndex = 0;
            this.grbRobotInfo.TabStop = false;
            this.grbRobotInfo.Text = "Robot Information";
            // 
            // flpRobotInfo
            // 
            this.flpRobotInfo.Controls.Add(this.flpRobotList);
            this.flpRobotInfo.Controls.Add(this.pnlRobotName);
            this.flpRobotInfo.Controls.Add(this.pnlControlMode);
            this.flpRobotInfo.Controls.Add(this.pnlHoldStatus);
            this.flpRobotInfo.Controls.Add(this.pnlMixRunFlag);
            this.flpRobotInfo.Controls.Add(this.pnlRefreshButton);
            this.flpRobotInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpRobotInfo.Location = new System.Drawing.Point(3, 23);
            this.flpRobotInfo.Name = "flpRobotInfo";
            this.flpRobotInfo.Size = new System.Drawing.Size(354, 185);
            this.flpRobotInfo.TabIndex = 0;
            // 
            // flpRobotList
            // 
            this.flpRobotList.Location = new System.Drawing.Point(3, 1);
            this.flpRobotList.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.flpRobotList.Name = "flpRobotList";
            this.flpRobotList.Size = new System.Drawing.Size(343, 28);
            this.flpRobotList.TabIndex = 36;
            // 
            // pnlRobotName
            // 
            this.pnlRobotName.Controls.Add(this.lblRobotName);
            this.pnlRobotName.Controls.Add(this.txtRobotName);
            this.pnlRobotName.Location = new System.Drawing.Point(3, 31);
            this.pnlRobotName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlRobotName.Name = "pnlRobotName";
            this.pnlRobotName.Size = new System.Drawing.Size(343, 28);
            this.pnlRobotName.TabIndex = 23;
            // 
            // lblRobotName
            // 
            this.lblRobotName.BackColor = System.Drawing.Color.Black;
            this.lblRobotName.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRobotName.ForeColor = System.Drawing.Color.White;
            this.lblRobotName.Location = new System.Drawing.Point(0, 0);
            this.lblRobotName.Name = "lblRobotName";
            this.lblRobotName.Size = new System.Drawing.Size(130, 26);
            this.lblRobotName.TabIndex = 27;
            this.lblRobotName.Text = "Robot Name";
            this.lblRobotName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtRobotName
            // 
            this.txtRobotName.Font = new System.Drawing.Font("Calibri", 11.25F);
            this.txtRobotName.Location = new System.Drawing.Point(130, 0);
            this.txtRobotName.MaxLength = 2;
            this.txtRobotName.Name = "txtRobotName";
            this.txtRobotName.ReadOnly = true;
            this.txtRobotName.Size = new System.Drawing.Size(202, 26);
            this.txtRobotName.TabIndex = 28;
            // 
            // pnlControlMode
            // 
            this.pnlControlMode.Controls.Add(this.lblControlMode);
            this.pnlControlMode.Controls.Add(this.txtControlMode);
            this.pnlControlMode.Location = new System.Drawing.Point(3, 61);
            this.pnlControlMode.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlControlMode.Name = "pnlControlMode";
            this.pnlControlMode.Size = new System.Drawing.Size(343, 28);
            this.pnlControlMode.TabIndex = 25;
            // 
            // lblControlMode
            // 
            this.lblControlMode.BackColor = System.Drawing.Color.Black;
            this.lblControlMode.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblControlMode.ForeColor = System.Drawing.Color.White;
            this.lblControlMode.Location = new System.Drawing.Point(0, 0);
            this.lblControlMode.Name = "lblControlMode";
            this.lblControlMode.Size = new System.Drawing.Size(130, 26);
            this.lblControlMode.TabIndex = 27;
            this.lblControlMode.Text = "Control Mode";
            this.lblControlMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtControlMode
            // 
            this.txtControlMode.Font = new System.Drawing.Font("Calibri", 11.25F);
            this.txtControlMode.Location = new System.Drawing.Point(130, 0);
            this.txtControlMode.MaxLength = 2;
            this.txtControlMode.Name = "txtControlMode";
            this.txtControlMode.ReadOnly = true;
            this.txtControlMode.Size = new System.Drawing.Size(202, 26);
            this.txtControlMode.TabIndex = 28;
            // 
            // pnlHoldStatus
            // 
            this.pnlHoldStatus.Controls.Add(this.lblHoldStatus);
            this.pnlHoldStatus.Controls.Add(this.txtHoldStatus);
            this.pnlHoldStatus.Location = new System.Drawing.Point(3, 91);
            this.pnlHoldStatus.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlHoldStatus.Name = "pnlHoldStatus";
            this.pnlHoldStatus.Size = new System.Drawing.Size(343, 28);
            this.pnlHoldStatus.TabIndex = 31;
            // 
            // lblHoldStatus
            // 
            this.lblHoldStatus.BackColor = System.Drawing.Color.Black;
            this.lblHoldStatus.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHoldStatus.ForeColor = System.Drawing.Color.White;
            this.lblHoldStatus.Location = new System.Drawing.Point(0, 0);
            this.lblHoldStatus.Name = "lblHoldStatus";
            this.lblHoldStatus.Size = new System.Drawing.Size(130, 26);
            this.lblHoldStatus.TabIndex = 27;
            this.lblHoldStatus.Text = "Hold Status";
            this.lblHoldStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtHoldStatus
            // 
            this.txtHoldStatus.Font = new System.Drawing.Font("Calibri", 11.25F);
            this.txtHoldStatus.Location = new System.Drawing.Point(130, 0);
            this.txtHoldStatus.MaxLength = 2;
            this.txtHoldStatus.Name = "txtHoldStatus";
            this.txtHoldStatus.ReadOnly = true;
            this.txtHoldStatus.Size = new System.Drawing.Size(202, 26);
            this.txtHoldStatus.TabIndex = 28;
            // 
            // pnlMixRunFlag
            // 
            this.pnlMixRunFlag.Controls.Add(this.lblMixRunFlag);
            this.pnlMixRunFlag.Controls.Add(this.txtMixRunFlag);
            this.pnlMixRunFlag.Location = new System.Drawing.Point(3, 121);
            this.pnlMixRunFlag.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlMixRunFlag.Name = "pnlMixRunFlag";
            this.pnlMixRunFlag.Size = new System.Drawing.Size(343, 28);
            this.pnlMixRunFlag.TabIndex = 37;
            // 
            // lblMixRunFlag
            // 
            this.lblMixRunFlag.BackColor = System.Drawing.Color.Black;
            this.lblMixRunFlag.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMixRunFlag.ForeColor = System.Drawing.Color.White;
            this.lblMixRunFlag.Location = new System.Drawing.Point(0, 0);
            this.lblMixRunFlag.Name = "lblMixRunFlag";
            this.lblMixRunFlag.Size = new System.Drawing.Size(130, 26);
            this.lblMixRunFlag.TabIndex = 27;
            this.lblMixRunFlag.Text = "Mix Run Flag";
            this.lblMixRunFlag.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtMixRunFlag
            // 
            this.txtMixRunFlag.Font = new System.Drawing.Font("Calibri", 11.25F);
            this.txtMixRunFlag.Location = new System.Drawing.Point(130, 0);
            this.txtMixRunFlag.MaxLength = 2;
            this.txtMixRunFlag.Name = "txtMixRunFlag";
            this.txtMixRunFlag.ReadOnly = true;
            this.txtMixRunFlag.Size = new System.Drawing.Size(202, 26);
            this.txtMixRunFlag.TabIndex = 28;
            // 
            // pnlRefreshButton
            // 
            this.pnlRefreshButton.Controls.Add(this.btnRefresh);
            this.pnlRefreshButton.Location = new System.Drawing.Point(3, 151);
            this.pnlRefreshButton.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlRefreshButton.Name = "pnlRefreshButton";
            this.pnlRefreshButton.Size = new System.Drawing.Size(343, 28);
            this.pnlRefreshButton.TabIndex = 30;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(134, -1);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(82, 29);
            this.btnRefresh.TabIndex = 29;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // grbRobotControlMode
            // 
            this.grbRobotControlMode.Controls.Add(this.pnlCommand);
            this.grbRobotControlMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbRobotControlMode.Font = new System.Drawing.Font("Calibri", 12F);
            this.grbRobotControlMode.Location = new System.Drawing.Point(749, 0);
            this.grbRobotControlMode.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.grbRobotControlMode.Name = "grbRobotControlMode";
            this.grbRobotControlMode.Size = new System.Drawing.Size(372, 211);
            this.grbRobotControlMode.TabIndex = 35;
            this.grbRobotControlMode.TabStop = false;
            this.grbRobotControlMode.Text = "Robot Command";
            // 
            // pnlCommand
            // 
            this.pnlCommand.Controls.Add(this.pnlMixRunFlag_New);
            this.pnlCommand.Controls.Add(this.pnlHoldStatus_New);
            this.pnlCommand.Controls.Add(this.pnlControlMode_New);
            this.pnlCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCommand.Location = new System.Drawing.Point(3, 23);
            this.pnlCommand.Name = "pnlCommand";
            this.pnlCommand.Size = new System.Drawing.Size(366, 185);
            this.pnlCommand.TabIndex = 0;
            // 
            // pnlMixRunFlag_New
            // 
            this.pnlMixRunFlag_New.Controls.Add(this.btnMixRunFlag_New);
            this.pnlMixRunFlag_New.Controls.Add(this.lblMixRunFlag_New);
            this.pnlMixRunFlag_New.Controls.Add(this.rdoSameYes);
            this.pnlMixRunFlag_New.Controls.Add(this.rdoSameNo);
            this.pnlMixRunFlag_New.Location = new System.Drawing.Point(3, 92);
            this.pnlMixRunFlag_New.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlMixRunFlag_New.Name = "pnlMixRunFlag_New";
            this.pnlMixRunFlag_New.Size = new System.Drawing.Size(343, 28);
            this.pnlMixRunFlag_New.TabIndex = 26;
            // 
            // btnMixRunFlag_New
            // 
            this.btnMixRunFlag_New.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMixRunFlag_New.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnMixRunFlag_New.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMixRunFlag_New.Location = new System.Drawing.Point(275, 0);
            this.btnMixRunFlag_New.Name = "btnMixRunFlag_New";
            this.btnMixRunFlag_New.Size = new System.Drawing.Size(60, 26);
            this.btnMixRunFlag_New.TabIndex = 26;
            this.btnMixRunFlag_New.Text = "Set";
            this.btnMixRunFlag_New.UseVisualStyleBackColor = true;
            this.btnMixRunFlag_New.Click += new System.EventHandler(this.btnMixRunFlag_New_Click);
            // 
            // lblMixRunFlag_New
            // 
            this.lblMixRunFlag_New.BackColor = System.Drawing.Color.Black;
            this.lblMixRunFlag_New.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMixRunFlag_New.ForeColor = System.Drawing.Color.White;
            this.lblMixRunFlag_New.Location = new System.Drawing.Point(0, 0);
            this.lblMixRunFlag_New.Name = "lblMixRunFlag_New";
            this.lblMixRunFlag_New.Size = new System.Drawing.Size(130, 26);
            this.lblMixRunFlag_New.TabIndex = 31;
            this.lblMixRunFlag_New.Text = "Mix Run Flag";
            this.lblMixRunFlag_New.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // rdoSameYes
            // 
            this.rdoSameYes.Font = new System.Drawing.Font("Calibri", 12F);
            this.rdoSameYes.Location = new System.Drawing.Point(133, 2);
            this.rdoSameYes.Name = "rdoSameYes";
            this.rdoSameYes.Size = new System.Drawing.Size(60, 23);
            this.rdoSameYes.TabIndex = 33;
            this.rdoSameYes.TabStop = true;
            this.rdoSameYes.Text = "Yes";
            this.rdoSameYes.UseVisualStyleBackColor = true;
            // 
            // rdoSameNo
            // 
            this.rdoSameNo.Font = new System.Drawing.Font("Calibri", 12F);
            this.rdoSameNo.Location = new System.Drawing.Point(195, 2);
            this.rdoSameNo.Name = "rdoSameNo";
            this.rdoSameNo.Size = new System.Drawing.Size(60, 23);
            this.rdoSameNo.TabIndex = 34;
            this.rdoSameNo.TabStop = true;
            this.rdoSameNo.Text = "No";
            this.rdoSameNo.UseVisualStyleBackColor = true;
            // 
            // pnlHoldStatus_New
            // 
            this.pnlHoldStatus_New.Controls.Add(this.btnHoldStatus_New);
            this.pnlHoldStatus_New.Controls.Add(this.lblHoldStatus_New);
            this.pnlHoldStatus_New.Controls.Add(this.rdoHold);
            this.pnlHoldStatus_New.Controls.Add(this.colRelease);
            this.pnlHoldStatus_New.Location = new System.Drawing.Point(3, 61);
            this.pnlHoldStatus_New.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlHoldStatus_New.Name = "pnlHoldStatus_New";
            this.pnlHoldStatus_New.Size = new System.Drawing.Size(343, 28);
            this.pnlHoldStatus_New.TabIndex = 25;
            // 
            // btnHoldStatus_New
            // 
            this.btnHoldStatus_New.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHoldStatus_New.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnHoldStatus_New.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHoldStatus_New.Location = new System.Drawing.Point(275, 0);
            this.btnHoldStatus_New.Name = "btnHoldStatus_New";
            this.btnHoldStatus_New.Size = new System.Drawing.Size(60, 26);
            this.btnHoldStatus_New.TabIndex = 26;
            this.btnHoldStatus_New.Text = "Set";
            this.btnHoldStatus_New.UseVisualStyleBackColor = true;
            this.btnHoldStatus_New.Click += new System.EventHandler(this.btnNewHoldStatus_Click);
            // 
            // lblHoldStatus_New
            // 
            this.lblHoldStatus_New.BackColor = System.Drawing.Color.Black;
            this.lblHoldStatus_New.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHoldStatus_New.ForeColor = System.Drawing.Color.White;
            this.lblHoldStatus_New.Location = new System.Drawing.Point(0, 0);
            this.lblHoldStatus_New.Name = "lblHoldStatus_New";
            this.lblHoldStatus_New.Size = new System.Drawing.Size(130, 26);
            this.lblHoldStatus_New.TabIndex = 31;
            this.lblHoldStatus_New.Text = "New Hold Status";
            this.lblHoldStatus_New.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // rdoHold
            // 
            this.rdoHold.Font = new System.Drawing.Font("Calibri", 12F);
            this.rdoHold.Location = new System.Drawing.Point(133, 2);
            this.rdoHold.Name = "rdoHold";
            this.rdoHold.Size = new System.Drawing.Size(60, 23);
            this.rdoHold.TabIndex = 33;
            this.rdoHold.TabStop = true;
            this.rdoHold.Text = "Hold";
            this.rdoHold.UseVisualStyleBackColor = true;
            // 
            // colRelease
            // 
            this.colRelease.Font = new System.Drawing.Font("Calibri", 12F);
            this.colRelease.Location = new System.Drawing.Point(195, 2);
            this.colRelease.Name = "colRelease";
            this.colRelease.Size = new System.Drawing.Size(79, 23);
            this.colRelease.TabIndex = 34;
            this.colRelease.TabStop = true;
            this.colRelease.Text = "Release";
            this.colRelease.UseVisualStyleBackColor = true;
            // 
            // pnlControlMode_New
            // 
            this.pnlControlMode_New.Controls.Add(this.lblControlMode_New);
            this.pnlControlMode_New.Controls.Add(this.rdoAUTO);
            this.pnlControlMode_New.Controls.Add(this.btnControlMode_New);
            this.pnlControlMode_New.Controls.Add(this.rdoSEMI);
            this.pnlControlMode_New.Location = new System.Drawing.Point(3, 31);
            this.pnlControlMode_New.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlControlMode_New.Name = "pnlControlMode_New";
            this.pnlControlMode_New.Size = new System.Drawing.Size(343, 28);
            this.pnlControlMode_New.TabIndex = 24;
            // 
            // lblControlMode_New
            // 
            this.lblControlMode_New.BackColor = System.Drawing.Color.Black;
            this.lblControlMode_New.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblControlMode_New.ForeColor = System.Drawing.Color.White;
            this.lblControlMode_New.Location = new System.Drawing.Point(0, 0);
            this.lblControlMode_New.Name = "lblControlMode_New";
            this.lblControlMode_New.Size = new System.Drawing.Size(130, 26);
            this.lblControlMode_New.TabIndex = 13;
            this.lblControlMode_New.Text = "New Control Mode";
            this.lblControlMode_New.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // rdoAUTO
            // 
            this.rdoAUTO.Font = new System.Drawing.Font("Calibri", 12F);
            this.rdoAUTO.Location = new System.Drawing.Point(195, 2);
            this.rdoAUTO.Name = "rdoAUTO";
            this.rdoAUTO.Size = new System.Drawing.Size(60, 23);
            this.rdoAUTO.TabIndex = 35;
            this.rdoAUTO.TabStop = true;
            this.rdoAUTO.Text = "Auto";
            this.rdoAUTO.UseVisualStyleBackColor = true;
            // 
            // btnControlMode_New
            // 
            this.btnControlMode_New.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnControlMode_New.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnControlMode_New.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnControlMode_New.Location = new System.Drawing.Point(275, 0);
            this.btnControlMode_New.Name = "btnControlMode_New";
            this.btnControlMode_New.Size = new System.Drawing.Size(60, 26);
            this.btnControlMode_New.TabIndex = 26;
            this.btnControlMode_New.Text = "Set";
            this.btnControlMode_New.UseVisualStyleBackColor = true;
            this.btnControlMode_New.Click += new System.EventHandler(this.btnNewControlMode_Click);
            // 
            // rdoSEMI
            // 
            this.rdoSEMI.Font = new System.Drawing.Font("Calibri", 12F);
            this.rdoSEMI.Location = new System.Drawing.Point(132, 2);
            this.rdoSEMI.Name = "rdoSEMI";
            this.rdoSEMI.Size = new System.Drawing.Size(60, 23);
            this.rdoSEMI.TabIndex = 34;
            this.rdoSEMI.TabStop = true;
            this.rdoSEMI.Text = "Semi";
            this.rdoSEMI.UseVisualStyleBackColor = true;
            // 
            // pnlRobotPic
            // 
            this.pnlRobotPic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlRobotPic.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlRobotPic.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlRobotPic.Location = new System.Drawing.Point(3, 33);
            this.pnlRobotPic.Name = "pnlRobotPic";
            this.pnlRobotPic.Size = new System.Drawing.Size(374, 178);
            this.pnlRobotPic.TabIndex = 34;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnSendRobotCommand);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(3, 566);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(1124, 28);
            this.pnlButton.TabIndex = 28;
            // 
            // btnSendRobotCommand
            // 
            this.btnSendRobotCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSendRobotCommand.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSendRobotCommand.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendRobotCommand.Location = new System.Drawing.Point(494, 0);
            this.btnSendRobotCommand.Name = "btnSendRobotCommand";
            this.btnSendRobotCommand.Size = new System.Drawing.Size(109, 29);
            this.btnSendRobotCommand.TabIndex = 25;
            this.btnSendRobotCommand.Text = "Send";
            this.btnSendRobotCommand.UseVisualStyleBackColor = true;
            this.btnSendRobotCommand.Click += new System.EventHandler(this.btnSendRobotCommand_Click);
            // 
            // FormRobotControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1130, 628);
            this.Name = "FormRobotControl";
            this.Text = "FormRobotControl";
            this.Load += new System.EventHandler(this.FormRobotControl_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.tlpSemiCaption.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tlpRobotMode.ResumeLayout(false);
            this.grbRobotInfo.ResumeLayout(false);
            this.flpRobotInfo.ResumeLayout(false);
            this.pnlRobotName.ResumeLayout(false);
            this.pnlRobotName.PerformLayout();
            this.pnlControlMode.ResumeLayout(false);
            this.pnlControlMode.PerformLayout();
            this.pnlHoldStatus.ResumeLayout(false);
            this.pnlHoldStatus.PerformLayout();
            this.pnlMixRunFlag.ResumeLayout(false);
            this.pnlMixRunFlag.PerformLayout();
            this.pnlRefreshButton.ResumeLayout(false);
            this.grbRobotControlMode.ResumeLayout(false);
            this.pnlCommand.ResumeLayout(false);
            this.pnlMixRunFlag_New.ResumeLayout(false);
            this.pnlHoldStatus_New.ResumeLayout(false);
            this.pnlControlMode_New.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnSendRobotCommand;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.TableLayoutPanel tlpSemiCaption;
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Label lblSemiCaption;
        private System.Windows.Forms.Panel panel2;
        public System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TableLayoutPanel tlpRobotMode;
        private System.Windows.Forms.GroupBox grbRobotControlMode;
        private System.Windows.Forms.Panel pnlCommand;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblRobotName;
        private System.Windows.Forms.TextBox txtRobotName;
        private System.Windows.Forms.Button btnControlMode_New;
        private System.Windows.Forms.Panel pnlRobotPic;
        private System.Windows.Forms.FlowLayoutPanel flpCommand;
        private System.Windows.Forms.Label lblHoldStatus_New;
        private System.Windows.Forms.RadioButton colRelease;
        private System.Windows.Forms.RadioButton rdoHold;
        private System.Windows.Forms.Panel pnlRobotName;
        private System.Windows.Forms.Panel pnlHoldStatus_New;
        private System.Windows.Forms.Button btnHoldStatus_New;
        private System.Windows.Forms.RadioButton rdoAUTO;
        private System.Windows.Forms.RadioButton rdoSEMI;
        private System.Windows.Forms.GroupBox grbRobotInfo;
        private System.Windows.Forms.FlowLayoutPanel flpRobotInfo;
        private System.Windows.Forms.Panel pnlControlMode;
        private System.Windows.Forms.Label lblControlMode;
        private System.Windows.Forms.TextBox txtControlMode;
        private System.Windows.Forms.Panel pnlHoldStatus;
        private System.Windows.Forms.Label lblHoldStatus;
        private System.Windows.Forms.TextBox txtHoldStatus;
        private System.Windows.Forms.Panel pnlRefreshButton;
        private System.Windows.Forms.Panel pnlControlMode_New;
        private System.Windows.Forms.Label lblControlMode_New;
        private System.Windows.Forms.FlowLayoutPanel flpRobotList;
        private System.Windows.Forms.Panel pnlMixRunFlag_New;
        private System.Windows.Forms.Button btnMixRunFlag_New;
        private System.Windows.Forms.Label lblMixRunFlag_New;
        private System.Windows.Forms.RadioButton rdoSameYes;
        private System.Windows.Forms.RadioButton rdoSameNo;
        private System.Windows.Forms.Panel pnlMixRunFlag;
        private System.Windows.Forms.Label lblMixRunFlag;
        private System.Windows.Forms.TextBox txtMixRunFlag;
    }
}