namespace UniOPI
{
    partial class FormLinkStatus
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
            this.spcAlarm = new System.Windows.Forms.SplitContainer();
            this.pnlCombox = new System.Windows.Forms.Panel();
            this.btnQuery = new System.Windows.Forms.Button();
            this.tlpContent = new System.Windows.Forms.TableLayoutPanel();
            this.pnlCYCLETRANSMISSIONSTATUS = new System.Windows.Forms.Panel();
            this.grbCyclicTransmissionStatus = new System.Windows.Forms.GroupBox();
            this.flpCyclicTransmissionStatus = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlCyclicTransmissionStatusDesc = new System.Windows.Forms.Panel();
            this.lblCyclicTransmissionStatusDesc_1 = new System.Windows.Forms.Label();
            this.lblCyclicTransmissionStatusDesc_0 = new System.Windows.Forms.Label();
            this.lblCyclicTransmissionStatus = new System.Windows.Forms.Label();
            this.pnlBATONPASSEACHSTATION = new System.Windows.Forms.Panel();
            this.grbBatonPassStatus = new System.Windows.Forms.GroupBox();
            this.flpBatonPassStatus = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlBatonPassStatusDesc = new System.Windows.Forms.Panel();
            this.lblBatonPassStatusDesc_1 = new System.Windows.Forms.Label();
            this.lblBatonPassStatusDesc_0 = new System.Windows.Forms.Label();
            this.lblBatonPassStatus = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.txtStationLoopStatus_W = new System.Windows.Forms.TextBox();
            this.txtDataLinkStop_W = new System.Windows.Forms.TextBox();
            this.txtBatonPassInterruption_W = new System.Windows.Forms.TextBox();
            this.txtBatonPassStatus_W = new System.Windows.Forms.TextBox();
            this.lblStationLoopStatus_W = new System.Windows.Forms.Label();
            this.lblDataLinkStop_W = new System.Windows.Forms.Label();
            this.lblBatonPassInterruption_W = new System.Windows.Forms.Label();
            this.lblBatonPassStatus_W = new System.Windows.Forms.Label();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcAlarm)).BeginInit();
            this.spcAlarm.Panel1.SuspendLayout();
            this.spcAlarm.Panel2.SuspendLayout();
            this.spcAlarm.SuspendLayout();
            this.pnlCombox.SuspendLayout();
            this.tlpContent.SuspendLayout();
            this.pnlCYCLETRANSMISSIONSTATUS.SuspendLayout();
            this.grbCyclicTransmissionStatus.SuspendLayout();
            this.pnlCyclicTransmissionStatusDesc.SuspendLayout();
            this.pnlBATONPASSEACHSTATION.SuspendLayout();
            this.grbBatonPassStatus.SuspendLayout();
            this.pnlBatonPassStatusDesc.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1076, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.spcAlarm);
            this.spcBase.Size = new System.Drawing.Size(1136, 668);
            // 
            // spcAlarm
            // 
            this.spcAlarm.BackColor = System.Drawing.Color.Transparent;
            this.spcAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcAlarm.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spcAlarm.Location = new System.Drawing.Point(0, 0);
            this.spcAlarm.Name = "spcAlarm";
            this.spcAlarm.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcAlarm.Panel1
            // 
            this.spcAlarm.Panel1.Controls.Add(this.pnlCombox);
            // 
            // spcAlarm.Panel2
            // 
            this.spcAlarm.Panel2.Controls.Add(this.tlpContent);
            this.spcAlarm.Size = new System.Drawing.Size(1136, 637);
            this.spcAlarm.SplitterDistance = 55;
            this.spcAlarm.TabIndex = 1;
            // 
            // pnlCombox
            // 
            this.pnlCombox.Controls.Add(this.btnQuery);
            this.pnlCombox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCombox.Location = new System.Drawing.Point(0, 0);
            this.pnlCombox.Name = "pnlCombox";
            this.pnlCombox.Size = new System.Drawing.Size(1136, 55);
            this.pnlCombox.TabIndex = 26;
            // 
            // btnQuery
            // 
            this.btnQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.Location = new System.Drawing.Point(998, 10);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(110, 29);
            this.btnQuery.TabIndex = 9;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // tlpContent
            // 
            this.tlpContent.ColumnCount = 1;
            this.tlpContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpContent.Controls.Add(this.pnlCYCLETRANSMISSIONSTATUS, 0, 2);
            this.tlpContent.Controls.Add(this.pnlBATONPASSEACHSTATION, 0, 1);
            this.tlpContent.Controls.Add(this.panel3, 0, 0);
            this.tlpContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpContent.Location = new System.Drawing.Point(0, 0);
            this.tlpContent.Name = "tlpContent";
            this.tlpContent.RowCount = 4;
            this.tlpContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.tlpContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.tlpContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.tlpContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpContent.Size = new System.Drawing.Size(1136, 578);
            this.tlpContent.TabIndex = 17;
            // 
            // pnlCYCLETRANSMISSIONSTATUS
            // 
            this.pnlCYCLETRANSMISSIONSTATUS.Controls.Add(this.grbCyclicTransmissionStatus);
            this.pnlCYCLETRANSMISSIONSTATUS.Controls.Add(this.lblCyclicTransmissionStatus);
            this.pnlCYCLETRANSMISSIONSTATUS.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCYCLETRANSMISSIONSTATUS.Location = new System.Drawing.Point(3, 363);
            this.pnlCYCLETRANSMISSIONSTATUS.Name = "pnlCYCLETRANSMISSIONSTATUS";
            this.pnlCYCLETRANSMISSIONSTATUS.Size = new System.Drawing.Size(1130, 174);
            this.pnlCYCLETRANSMISSIONSTATUS.TabIndex = 0;
            // 
            // grbCyclicTransmissionStatus
            // 
            this.grbCyclicTransmissionStatus.Controls.Add(this.flpCyclicTransmissionStatus);
            this.grbCyclicTransmissionStatus.Controls.Add(this.pnlCyclicTransmissionStatusDesc);
            this.grbCyclicTransmissionStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbCyclicTransmissionStatus.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.grbCyclicTransmissionStatus.Location = new System.Drawing.Point(217, 0);
            this.grbCyclicTransmissionStatus.Name = "grbCyclicTransmissionStatus";
            this.grbCyclicTransmissionStatus.Size = new System.Drawing.Size(913, 174);
            this.grbCyclicTransmissionStatus.TabIndex = 3;
            this.grbCyclicTransmissionStatus.TabStop = false;
            // 
            // flpCyclicTransmissionStatus
            // 
            this.flpCyclicTransmissionStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpCyclicTransmissionStatus.Location = new System.Drawing.Point(3, 24);
            this.flpCyclicTransmissionStatus.Name = "flpCyclicTransmissionStatus";
            this.flpCyclicTransmissionStatus.Size = new System.Drawing.Size(907, 103);
            this.flpCyclicTransmissionStatus.TabIndex = 1;
            // 
            // pnlCyclicTransmissionStatusDesc
            // 
            this.pnlCyclicTransmissionStatusDesc.Controls.Add(this.label2);
            this.pnlCyclicTransmissionStatusDesc.Controls.Add(this.lblCyclicTransmissionStatusDesc_1);
            this.pnlCyclicTransmissionStatusDesc.Controls.Add(this.lblCyclicTransmissionStatusDesc_0);
            this.pnlCyclicTransmissionStatusDesc.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlCyclicTransmissionStatusDesc.Location = new System.Drawing.Point(3, 127);
            this.pnlCyclicTransmissionStatusDesc.Name = "pnlCyclicTransmissionStatusDesc";
            this.pnlCyclicTransmissionStatusDesc.Size = new System.Drawing.Size(907, 44);
            this.pnlCyclicTransmissionStatusDesc.TabIndex = 4;
            // 
            // lblCyclicTransmissionStatusDesc_1
            // 
            this.lblCyclicTransmissionStatusDesc_1.BackColor = System.Drawing.Color.Red;
            this.lblCyclicTransmissionStatusDesc_1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCyclicTransmissionStatusDesc_1.ForeColor = System.Drawing.Color.Black;
            this.lblCyclicTransmissionStatusDesc_1.Location = new System.Drawing.Point(572, 23);
            this.lblCyclicTransmissionStatusDesc_1.Name = "lblCyclicTransmissionStatusDesc_1";
            this.lblCyclicTransmissionStatusDesc_1.Size = new System.Drawing.Size(330, 20);
            this.lblCyclicTransmissionStatusDesc_1.TabIndex = 6;
            this.lblCyclicTransmissionStatusDesc_1.Text = "1: Cyclic transmission faulty station";
            this.lblCyclicTransmissionStatusDesc_1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCyclicTransmissionStatusDesc_0
            // 
            this.lblCyclicTransmissionStatusDesc_0.BackColor = System.Drawing.Color.Lime;
            this.lblCyclicTransmissionStatusDesc_0.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCyclicTransmissionStatusDesc_0.ForeColor = System.Drawing.Color.Black;
            this.lblCyclicTransmissionStatusDesc_0.Location = new System.Drawing.Point(572, 3);
            this.lblCyclicTransmissionStatusDesc_0.Name = "lblCyclicTransmissionStatusDesc_0";
            this.lblCyclicTransmissionStatusDesc_0.Size = new System.Drawing.Size(330, 20);
            this.lblCyclicTransmissionStatusDesc_0.TabIndex = 5;
            this.lblCyclicTransmissionStatusDesc_0.Text = "0: Cyclic transmission normally operating station";
            this.lblCyclicTransmissionStatusDesc_0.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCyclicTransmissionStatus
            // 
            this.lblCyclicTransmissionStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCyclicTransmissionStatus.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCyclicTransmissionStatus.Location = new System.Drawing.Point(0, 0);
            this.lblCyclicTransmissionStatus.Name = "lblCyclicTransmissionStatus";
            this.lblCyclicTransmissionStatus.Size = new System.Drawing.Size(217, 174);
            this.lblCyclicTransmissionStatus.TabIndex = 2;
            this.lblCyclicTransmissionStatus.Text = "Cyclic Transmission Status";
            this.lblCyclicTransmissionStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // pnlBATONPASSEACHSTATION
            // 
            this.pnlBATONPASSEACHSTATION.Controls.Add(this.grbBatonPassStatus);
            this.pnlBATONPASSEACHSTATION.Controls.Add(this.lblBatonPassStatus);
            this.pnlBATONPASSEACHSTATION.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBATONPASSEACHSTATION.Location = new System.Drawing.Point(3, 183);
            this.pnlBATONPASSEACHSTATION.Name = "pnlBATONPASSEACHSTATION";
            this.pnlBATONPASSEACHSTATION.Size = new System.Drawing.Size(1130, 174);
            this.pnlBATONPASSEACHSTATION.TabIndex = 1;
            // 
            // grbBatonPassStatus
            // 
            this.grbBatonPassStatus.Controls.Add(this.flpBatonPassStatus);
            this.grbBatonPassStatus.Controls.Add(this.pnlBatonPassStatusDesc);
            this.grbBatonPassStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbBatonPassStatus.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.grbBatonPassStatus.Location = new System.Drawing.Point(217, 0);
            this.grbBatonPassStatus.Name = "grbBatonPassStatus";
            this.grbBatonPassStatus.Size = new System.Drawing.Size(913, 174);
            this.grbBatonPassStatus.TabIndex = 7;
            this.grbBatonPassStatus.TabStop = false;
            // 
            // flpBatonPassStatus
            // 
            this.flpBatonPassStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpBatonPassStatus.Location = new System.Drawing.Point(3, 24);
            this.flpBatonPassStatus.Name = "flpBatonPassStatus";
            this.flpBatonPassStatus.Size = new System.Drawing.Size(907, 103);
            this.flpBatonPassStatus.TabIndex = 0;
            // 
            // pnlBatonPassStatusDesc
            // 
            this.pnlBatonPassStatusDesc.Controls.Add(this.label1);
            this.pnlBatonPassStatusDesc.Controls.Add(this.lblBatonPassStatusDesc_1);
            this.pnlBatonPassStatusDesc.Controls.Add(this.lblBatonPassStatusDesc_0);
            this.pnlBatonPassStatusDesc.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBatonPassStatusDesc.Location = new System.Drawing.Point(3, 127);
            this.pnlBatonPassStatusDesc.Name = "pnlBatonPassStatusDesc";
            this.pnlBatonPassStatusDesc.Size = new System.Drawing.Size(907, 44);
            this.pnlBatonPassStatusDesc.TabIndex = 3;
            // 
            // lblBatonPassStatusDesc_1
            // 
            this.lblBatonPassStatusDesc_1.BackColor = System.Drawing.Color.Red;
            this.lblBatonPassStatusDesc_1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBatonPassStatusDesc_1.ForeColor = System.Drawing.Color.Black;
            this.lblBatonPassStatusDesc_1.Location = new System.Drawing.Point(572, 23);
            this.lblBatonPassStatusDesc_1.Name = "lblBatonPassStatusDesc_1";
            this.lblBatonPassStatusDesc_1.Size = new System.Drawing.Size(330, 20);
            this.lblBatonPassStatusDesc_1.TabIndex = 6;
            this.lblBatonPassStatusDesc_1.Text = "1: Baton pass faulty station";
            this.lblBatonPassStatusDesc_1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBatonPassStatusDesc_0
            // 
            this.lblBatonPassStatusDesc_0.BackColor = System.Drawing.Color.Lime;
            this.lblBatonPassStatusDesc_0.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBatonPassStatusDesc_0.ForeColor = System.Drawing.Color.Black;
            this.lblBatonPassStatusDesc_0.Location = new System.Drawing.Point(572, 3);
            this.lblBatonPassStatusDesc_0.Name = "lblBatonPassStatusDesc_0";
            this.lblBatonPassStatusDesc_0.Size = new System.Drawing.Size(330, 20);
            this.lblBatonPassStatusDesc_0.TabIndex = 5;
            this.lblBatonPassStatusDesc_0.Text = "0: Baton pass normally operating station ";
            this.lblBatonPassStatusDesc_0.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBatonPassStatus
            // 
            this.lblBatonPassStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblBatonPassStatus.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBatonPassStatus.Location = new System.Drawing.Point(0, 0);
            this.lblBatonPassStatus.Name = "lblBatonPassStatus";
            this.lblBatonPassStatus.Size = new System.Drawing.Size(217, 174);
            this.lblBatonPassStatus.TabIndex = 8;
            this.lblBatonPassStatus.Text = "Baton Pass Each Station";
            this.lblBatonPassStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.txtStationLoopStatus_W);
            this.panel3.Controls.Add(this.txtDataLinkStop_W);
            this.panel3.Controls.Add(this.txtBatonPassInterruption_W);
            this.panel3.Controls.Add(this.txtBatonPassStatus_W);
            this.panel3.Controls.Add(this.lblStationLoopStatus_W);
            this.panel3.Controls.Add(this.lblDataLinkStop_W);
            this.panel3.Controls.Add(this.lblBatonPassInterruption_W);
            this.panel3.Controls.Add(this.lblBatonPassStatus_W);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1130, 174);
            this.panel3.TabIndex = 2;
            // 
            // txtStationLoopStatus_W
            // 
            this.txtStationLoopStatus_W.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStationLoopStatus_W.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStationLoopStatus_W.Location = new System.Drawing.Point(217, 135);
            this.txtStationLoopStatus_W.Name = "txtStationLoopStatus_W";
            this.txtStationLoopStatus_W.ReadOnly = true;
            this.txtStationLoopStatus_W.Size = new System.Drawing.Size(888, 28);
            this.txtStationLoopStatus_W.TabIndex = 9;
            // 
            // txtDataLinkStop_W
            // 
            this.txtDataLinkStop_W.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDataLinkStop_W.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDataLinkStop_W.Location = new System.Drawing.Point(217, 92);
            this.txtDataLinkStop_W.Name = "txtDataLinkStop_W";
            this.txtDataLinkStop_W.ReadOnly = true;
            this.txtDataLinkStop_W.Size = new System.Drawing.Size(886, 28);
            this.txtDataLinkStop_W.TabIndex = 8;
            // 
            // txtBatonPassInterruption_W
            // 
            this.txtBatonPassInterruption_W.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBatonPassInterruption_W.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBatonPassInterruption_W.Location = new System.Drawing.Point(217, 49);
            this.txtBatonPassInterruption_W.Name = "txtBatonPassInterruption_W";
            this.txtBatonPassInterruption_W.ReadOnly = true;
            this.txtBatonPassInterruption_W.Size = new System.Drawing.Size(888, 28);
            this.txtBatonPassInterruption_W.TabIndex = 7;
            // 
            // txtBatonPassStatus_W
            // 
            this.txtBatonPassStatus_W.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBatonPassStatus_W.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBatonPassStatus_W.Location = new System.Drawing.Point(217, 6);
            this.txtBatonPassStatus_W.Name = "txtBatonPassStatus_W";
            this.txtBatonPassStatus_W.ReadOnly = true;
            this.txtBatonPassStatus_W.Size = new System.Drawing.Size(888, 28);
            this.txtBatonPassStatus_W.TabIndex = 6;
            // 
            // lblStationLoopStatus_W
            // 
            this.lblStationLoopStatus_W.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStationLoopStatus_W.Location = new System.Drawing.Point(0, 139);
            this.lblStationLoopStatus_W.Name = "lblStationLoopStatus_W";
            this.lblStationLoopStatus_W.Size = new System.Drawing.Size(217, 21);
            this.lblStationLoopStatus_W.TabIndex = 5;
            this.lblStationLoopStatus_W.Text = "Station Loop Status";
            this.lblStationLoopStatus_W.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblDataLinkStop_W
            // 
            this.lblDataLinkStop_W.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDataLinkStop_W.Location = new System.Drawing.Point(0, 96);
            this.lblDataLinkStop_W.Name = "lblDataLinkStop_W";
            this.lblDataLinkStop_W.Size = new System.Drawing.Size(217, 21);
            this.lblDataLinkStop_W.TabIndex = 4;
            this.lblDataLinkStop_W.Text = "Data Link Stop";
            this.lblDataLinkStop_W.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblBatonPassInterruption_W
            // 
            this.lblBatonPassInterruption_W.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBatonPassInterruption_W.Location = new System.Drawing.Point(0, 53);
            this.lblBatonPassInterruption_W.Name = "lblBatonPassInterruption_W";
            this.lblBatonPassInterruption_W.Size = new System.Drawing.Size(217, 21);
            this.lblBatonPassInterruption_W.TabIndex = 3;
            this.lblBatonPassInterruption_W.Text = "Baton Pass Interruption";
            this.lblBatonPassInterruption_W.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblBatonPassStatus_W
            // 
            this.lblBatonPassStatus_W.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBatonPassStatus_W.Location = new System.Drawing.Point(0, 10);
            this.lblBatonPassStatus_W.Name = "lblBatonPassStatus_W";
            this.lblBatonPassStatus_W.Size = new System.Drawing.Size(217, 21);
            this.lblBatonPassStatus_W.TabIndex = 2;
            this.lblBatonPassStatus_W.Text = "Baton pass status";
            this.lblBatonPassStatus_W.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 1000;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.LightSlateGray;
            this.label1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(241, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(330, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Non-PLC interface";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.LightSlateGray;
            this.label2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(241, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(330, 20);
            this.label2.TabIndex = 8;
            this.label2.Text = "Non-PLC interface";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormLinkStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.ClientSize = new System.Drawing.Size(1136, 668);
            this.Name = "FormLinkStatus";
            this.Load += new System.EventHandler(this.FormLinkStatus_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.spcAlarm.Panel1.ResumeLayout(false);
            this.spcAlarm.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcAlarm)).EndInit();
            this.spcAlarm.ResumeLayout(false);
            this.pnlCombox.ResumeLayout(false);
            this.tlpContent.ResumeLayout(false);
            this.pnlCYCLETRANSMISSIONSTATUS.ResumeLayout(false);
            this.grbCyclicTransmissionStatus.ResumeLayout(false);
            this.pnlCyclicTransmissionStatusDesc.ResumeLayout(false);
            this.pnlBATONPASSEACHSTATION.ResumeLayout(false);
            this.grbBatonPassStatus.ResumeLayout(false);
            this.pnlBatonPassStatusDesc.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spcAlarm;
        private System.Windows.Forms.Panel pnlCombox;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel pnlBATONPASSEACHSTATION;
        private System.Windows.Forms.FlowLayoutPanel flpBatonPassStatus;
        private System.Windows.Forms.Panel pnlCYCLETRANSMISSIONSTATUS;
        private System.Windows.Forms.FlowLayoutPanel flpCyclicTransmissionStatus;
        private System.Windows.Forms.Label lblCyclicTransmissionStatus;
        private System.Windows.Forms.TextBox txtStationLoopStatus_W;
        private System.Windows.Forms.TextBox txtDataLinkStop_W;
        private System.Windows.Forms.TextBox txtBatonPassInterruption_W;
        private System.Windows.Forms.TextBox txtBatonPassStatus_W;
        private System.Windows.Forms.Label lblStationLoopStatus_W;
        private System.Windows.Forms.Label lblDataLinkStop_W;
        private System.Windows.Forms.Label lblBatonPassInterruption_W;
        private System.Windows.Forms.Label lblBatonPassStatus_W;
        private System.Windows.Forms.TableLayoutPanel tlpContent;
        private System.Windows.Forms.Panel pnlBatonPassStatusDesc;
        private System.Windows.Forms.Panel pnlCyclicTransmissionStatusDesc;
        private System.Windows.Forms.Label lblCyclicTransmissionStatusDesc_1;
        private System.Windows.Forms.Label lblCyclicTransmissionStatusDesc_0;
        private System.Windows.Forms.Label lblBatonPassStatusDesc_1;
        private System.Windows.Forms.Label lblBatonPassStatusDesc_0;
        private System.Windows.Forms.GroupBox grbBatonPassStatus;
        private System.Windows.Forms.Label lblBatonPassStatus;
        private System.Windows.Forms.GroupBox grbCyclicTransmissionStatus;
        public System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}