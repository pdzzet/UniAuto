namespace UniOPI
{
    partial class FormSubBlock
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
            this.spcSubBlock = new System.Windows.Forms.SplitContainer();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.dgvtxtOBJECTKEY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtLINEID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtSERVERNAME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtSUBBLOCKID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtSTARTEQP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtCONTROLEQP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtSTARTEVENTMSG = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtINTERLOCKNO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtNEXTSUBBLOCKEQP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtNEXTSUBBLOCKEQPLIST = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvchkENABLED = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtINTERLOCKREPLYNO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtREMARK = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlLayout = new System.Windows.Forms.Panel();
            this.tlpEdit = new System.Windows.Forms.TableLayoutPanel();
            this.lblSubBlockID = new System.Windows.Forms.Label();
            this.lblStartEQP = new System.Windows.Forms.Label();
            this.lblControlEQP = new System.Windows.Forms.Label();
            this.lblStartEventMsg = new System.Windows.Forms.Label();
            this.lblInterLockNo = new System.Windows.Forms.Label();
            this.lblNextSubBlockEQP = new System.Windows.Forms.Label();
            this.lblNextSubBlockEQPList = new System.Windows.Forms.Label();
            this.lblEnabled = new System.Windows.Forms.Label();
            this.lblInterLockReplyNo = new System.Windows.Forms.Label();
            this.lblRemark = new System.Windows.Forms.Label();
            this.txtSubBlockID = new System.Windows.Forms.TextBox();
            this.txtStartEQP = new System.Windows.Forms.TextBox();
            this.txtControlEQP = new System.Windows.Forms.TextBox();
            this.txtInterLockNo = new System.Windows.Forms.TextBox();
            this.txtNextSubBlockEQP = new System.Windows.Forms.TextBox();
            this.txtNextSubBlockEQPList = new System.Windows.Forms.TextBox();
            this.txtInterLockReplyNo = new System.Windows.Forms.TextBox();
            this.txtRemark = new System.Windows.Forms.TextBox();
            this.cmbStartEventMsg = new System.Windows.Forms.ComboBox();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.flpButtom = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnModify = new System.Windows.Forms.Button();
            this.btnEnableAll = new System.Windows.Forms.Button();
            this.btnDisableAll = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcSubBlock)).BeginInit();
            this.spcSubBlock.Panel1.SuspendLayout();
            this.spcSubBlock.Panel2.SuspendLayout();
            this.spcSubBlock.SuspendLayout();
            this.tlpBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tlpEdit.SuspendLayout();
            this.flpButtom.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1041, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.spcSubBlock);
            this.spcBase.Size = new System.Drawing.Size(1101, 668);
            // 
            // spcSubBlock
            // 
            this.spcSubBlock.BackColor = System.Drawing.Color.Transparent;
            this.spcSubBlock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcSubBlock.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.spcSubBlock.Location = new System.Drawing.Point(0, 0);
            this.spcSubBlock.Name = "spcSubBlock";
            // 
            // spcSubBlock.Panel1
            // 
            this.spcSubBlock.Panel1.Controls.Add(this.tlpBase);
            // 
            // spcSubBlock.Panel2
            // 
            this.spcSubBlock.Panel2.Controls.Add(this.flpButtom);
            this.spcSubBlock.Size = new System.Drawing.Size(1101, 637);
            this.spcSubBlock.SplitterDistance = 1006;
            this.spcSubBlock.TabIndex = 0;
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.dgvData, 0, 0);
            this.tlpBase.Controls.Add(this.pnlLayout, 0, 2);
            this.tlpBase.Controls.Add(this.tlpEdit, 0, 1);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 3;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.tlpBase.Size = new System.Drawing.Size(1006, 637);
            this.tlpBase.TabIndex = 0;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            this.dgvData.BackgroundColor = System.Drawing.Color.Gray;
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
            this.dgvtxtOBJECTKEY,
            this.dgvtxtLINEID,
            this.dgvtxtSERVERNAME,
            this.dgvtxtSUBBLOCKID,
            this.dgvtxtSTARTEQP,
            this.dgvtxtCONTROLEQP,
            this.dgvtxtSTARTEVENTMSG,
            this.dgvtxtINTERLOCKNO,
            this.dgvtxtNEXTSUBBLOCKEQP,
            this.dgvtxtNEXTSUBBLOCKEQPLIST,
            this.dgvchkENABLED,
            this.dgvtxtINTERLOCKREPLYNO,
            this.dgvtxtREMARK});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 3);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.dgvData.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1000, 231);
            this.dgvData.TabIndex = 21;
            this.dgvData.DataSourceChanged += new System.EventHandler(this.dgvData_DataSourceChanged);
            this.dgvData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvList_CellClick);
            this.dgvData.CellErrorTextChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvData_CellErrorTextChanged);
            // 
            // dgvtxtOBJECTKEY
            // 
            this.dgvtxtOBJECTKEY.DataPropertyName = "OBJECTKEY";
            this.dgvtxtOBJECTKEY.HeaderText = "OBJECTKEY";
            this.dgvtxtOBJECTKEY.Name = "dgvtxtOBJECTKEY";
            this.dgvtxtOBJECTKEY.ReadOnly = true;
            this.dgvtxtOBJECTKEY.Visible = false;
            // 
            // dgvtxtLINEID
            // 
            this.dgvtxtLINEID.DataPropertyName = "SERVERNAME";
            this.dgvtxtLINEID.HeaderText = "LINEID";
            this.dgvtxtLINEID.Name = "dgvtxtLINEID";
            this.dgvtxtLINEID.ReadOnly = true;
            this.dgvtxtLINEID.Visible = false;
            // 
            // dgvtxtSERVERNAME
            // 
            this.dgvtxtSERVERNAME.DataPropertyName = "SERVERNAME";
            this.dgvtxtSERVERNAME.HeaderText = "SERVERNAME";
            this.dgvtxtSERVERNAME.Name = "dgvtxtSERVERNAME";
            this.dgvtxtSERVERNAME.ReadOnly = true;
            this.dgvtxtSERVERNAME.Visible = false;
            // 
            // dgvtxtSUBBLOCKID
            // 
            this.dgvtxtSUBBLOCKID.DataPropertyName = "SUBBLOCKID";
            this.dgvtxtSUBBLOCKID.HeaderText = "Sub Block ID";
            this.dgvtxtSUBBLOCKID.Name = "dgvtxtSUBBLOCKID";
            this.dgvtxtSUBBLOCKID.ReadOnly = true;
            this.dgvtxtSUBBLOCKID.Width = 120;
            // 
            // dgvtxtSTARTEQP
            // 
            this.dgvtxtSTARTEQP.DataPropertyName = "STARTEQP";
            this.dgvtxtSTARTEQP.HeaderText = "Start EQP";
            this.dgvtxtSTARTEQP.Name = "dgvtxtSTARTEQP";
            this.dgvtxtSTARTEQP.ReadOnly = true;
            this.dgvtxtSTARTEQP.Width = 120;
            // 
            // dgvtxtCONTROLEQP
            // 
            this.dgvtxtCONTROLEQP.DataPropertyName = "CONTROLEQP";
            this.dgvtxtCONTROLEQP.HeaderText = "Control EQP";
            this.dgvtxtCONTROLEQP.Name = "dgvtxtCONTROLEQP";
            this.dgvtxtCONTROLEQP.ReadOnly = true;
            this.dgvtxtCONTROLEQP.Width = 120;
            // 
            // dgvtxtSTARTEVENTMSG
            // 
            this.dgvtxtSTARTEVENTMSG.DataPropertyName = "STARTEVENTMSG";
            this.dgvtxtSTARTEVENTMSG.HeaderText = "Start Event";
            this.dgvtxtSTARTEVENTMSG.Name = "dgvtxtSTARTEVENTMSG";
            this.dgvtxtSTARTEVENTMSG.ReadOnly = true;
            this.dgvtxtSTARTEVENTMSG.Width = 170;
            // 
            // dgvtxtINTERLOCKNO
            // 
            this.dgvtxtINTERLOCKNO.DataPropertyName = "INTERLOCKNO";
            this.dgvtxtINTERLOCKNO.HeaderText = "MPLC Interlock No";
            this.dgvtxtINTERLOCKNO.Name = "dgvtxtINTERLOCKNO";
            this.dgvtxtINTERLOCKNO.ReadOnly = true;
            this.dgvtxtINTERLOCKNO.Width = 160;
            // 
            // dgvtxtNEXTSUBBLOCKEQP
            // 
            this.dgvtxtNEXTSUBBLOCKEQP.DataPropertyName = "NEXTSUBBLOCKEQP";
            this.dgvtxtNEXTSUBBLOCKEQP.HeaderText = "Next Sub Block EQP";
            this.dgvtxtNEXTSUBBLOCKEQP.Name = "dgvtxtNEXTSUBBLOCKEQP";
            this.dgvtxtNEXTSUBBLOCKEQP.ReadOnly = true;
            this.dgvtxtNEXTSUBBLOCKEQP.Width = 180;
            // 
            // dgvtxtNEXTSUBBLOCKEQPLIST
            // 
            this.dgvtxtNEXTSUBBLOCKEQPLIST.DataPropertyName = "NEXTSUBBLOCKEQPLIST";
            this.dgvtxtNEXTSUBBLOCKEQPLIST.HeaderText = "Next Sub Block EQP List";
            this.dgvtxtNEXTSUBBLOCKEQPLIST.Name = "dgvtxtNEXTSUBBLOCKEQPLIST";
            this.dgvtxtNEXTSUBBLOCKEQPLIST.ReadOnly = true;
            this.dgvtxtNEXTSUBBLOCKEQPLIST.Width = 220;
            // 
            // dgvchkENABLED
            // 
            this.dgvchkENABLED.DataPropertyName = "ENABLED";
            this.dgvchkENABLED.HeaderText = "Enabled";
            this.dgvchkENABLED.Name = "dgvchkENABLED";
            this.dgvchkENABLED.ReadOnly = true;
            this.dgvchkENABLED.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // dgvtxtINTERLOCKREPLYNO
            // 
            this.dgvtxtINTERLOCKREPLYNO.DataPropertyName = "INTERLOCKREPLYNO";
            this.dgvtxtINTERLOCKREPLYNO.HeaderText = "MPLC Interlock Reply";
            this.dgvtxtINTERLOCKREPLYNO.Name = "dgvtxtINTERLOCKREPLYNO";
            this.dgvtxtINTERLOCKREPLYNO.ReadOnly = true;
            this.dgvtxtINTERLOCKREPLYNO.Width = 180;
            // 
            // dgvtxtREMARK
            // 
            this.dgvtxtREMARK.DataPropertyName = "REMARK";
            this.dgvtxtREMARK.HeaderText = "Remark";
            this.dgvtxtREMARK.Name = "dgvtxtREMARK";
            this.dgvtxtREMARK.ReadOnly = true;
            // 
            // pnlLayout
            // 
            this.pnlLayout.AutoScroll = true;
            this.pnlLayout.AutoSize = true;
            this.pnlLayout.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLayout.Location = new System.Drawing.Point(3, 240);
            this.pnlLayout.Name = "pnlLayout";
            this.pnlLayout.Size = new System.Drawing.Size(1000, 394);
            this.pnlLayout.TabIndex = 0;
            // 
            // tlpEdit
            // 
            this.tlpEdit.ColumnCount = 6;
            this.tlpEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.3F));
            this.tlpEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.2F));
            this.tlpEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.6F));
            this.tlpEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.3F));
            this.tlpEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.4F));
            this.tlpEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.2F));
            this.tlpEdit.Controls.Add(this.lblSubBlockID, 0, 0);
            this.tlpEdit.Controls.Add(this.lblStartEQP, 0, 1);
            this.tlpEdit.Controls.Add(this.lblControlEQP, 0, 2);
            this.tlpEdit.Controls.Add(this.lblStartEventMsg, 0, 3);
            this.tlpEdit.Controls.Add(this.lblInterLockNo, 2, 0);
            this.tlpEdit.Controls.Add(this.lblNextSubBlockEQP, 2, 1);
            this.tlpEdit.Controls.Add(this.lblNextSubBlockEQPList, 2, 2);
            this.tlpEdit.Controls.Add(this.lblEnabled, 2, 3);
            this.tlpEdit.Controls.Add(this.lblInterLockReplyNo, 4, 0);
            this.tlpEdit.Controls.Add(this.lblRemark, 4, 1);
            this.tlpEdit.Controls.Add(this.txtSubBlockID);
            this.tlpEdit.Controls.Add(this.txtStartEQP, 1, 1);
            this.tlpEdit.Controls.Add(this.txtControlEQP, 1, 2);
            this.tlpEdit.Controls.Add(this.txtInterLockNo, 3, 0);
            this.tlpEdit.Controls.Add(this.txtNextSubBlockEQP, 3, 1);
            this.tlpEdit.Controls.Add(this.txtNextSubBlockEQPList, 3, 2);
            this.tlpEdit.Controls.Add(this.txtInterLockReplyNo, 5, 0);
            this.tlpEdit.Controls.Add(this.txtRemark, 5, 1);
            this.tlpEdit.Controls.Add(this.cmbStartEventMsg, 1, 3);
            this.tlpEdit.Controls.Add(this.chkEnabled, 3, 3);
            this.tlpEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpEdit.Location = new System.Drawing.Point(3, 240);
            this.tlpEdit.Name = "tlpEdit";
            this.tlpEdit.RowCount = 4;
            this.tlpEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpEdit.Size = new System.Drawing.Size(1000, 1);
            this.tlpEdit.TabIndex = 22;
            // 
            // lblSubBlockID
            // 
            this.lblSubBlockID.AutoSize = true;
            this.lblSubBlockID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSubBlockID.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubBlockID.Location = new System.Drawing.Point(3, 0);
            this.lblSubBlockID.Name = "lblSubBlockID";
            this.lblSubBlockID.Size = new System.Drawing.Size(137, 1);
            this.lblSubBlockID.TabIndex = 0;
            this.lblSubBlockID.Text = "Sub Block ID";
            this.lblSubBlockID.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblStartEQP
            // 
            this.lblStartEQP.AutoSize = true;
            this.lblStartEQP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStartEQP.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStartEQP.Location = new System.Drawing.Point(3, 0);
            this.lblStartEQP.Name = "lblStartEQP";
            this.lblStartEQP.Size = new System.Drawing.Size(137, 1);
            this.lblStartEQP.TabIndex = 1;
            this.lblStartEQP.Text = "Start EQP";
            this.lblStartEQP.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblControlEQP
            // 
            this.lblControlEQP.AutoSize = true;
            this.lblControlEQP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblControlEQP.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblControlEQP.Location = new System.Drawing.Point(3, 0);
            this.lblControlEQP.Name = "lblControlEQP";
            this.lblControlEQP.Size = new System.Drawing.Size(137, 1);
            this.lblControlEQP.TabIndex = 2;
            this.lblControlEQP.Text = "Control EQP";
            this.lblControlEQP.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblStartEventMsg
            // 
            this.lblStartEventMsg.AutoSize = true;
            this.lblStartEventMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStartEventMsg.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStartEventMsg.Location = new System.Drawing.Point(3, 0);
            this.lblStartEventMsg.Name = "lblStartEventMsg";
            this.lblStartEventMsg.Size = new System.Drawing.Size(137, 1);
            this.lblStartEventMsg.TabIndex = 3;
            this.lblStartEventMsg.Text = "Start Event MSG";
            this.lblStartEventMsg.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblInterLockNo
            // 
            this.lblInterLockNo.AutoSize = true;
            this.lblInterLockNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInterLockNo.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInterLockNo.Location = new System.Drawing.Point(318, 0);
            this.lblInterLockNo.Name = "lblInterLockNo";
            this.lblInterLockNo.Size = new System.Drawing.Size(160, 1);
            this.lblInterLockNo.TabIndex = 4;
            this.lblInterLockNo.Text = "Interlock No";
            this.lblInterLockNo.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblNextSubBlockEQP
            // 
            this.lblNextSubBlockEQP.AutoSize = true;
            this.lblNextSubBlockEQP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNextSubBlockEQP.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNextSubBlockEQP.Location = new System.Drawing.Point(318, 0);
            this.lblNextSubBlockEQP.Name = "lblNextSubBlockEQP";
            this.lblNextSubBlockEQP.Size = new System.Drawing.Size(160, 1);
            this.lblNextSubBlockEQP.TabIndex = 5;
            this.lblNextSubBlockEQP.Text = "Next Sub Block EQP";
            this.lblNextSubBlockEQP.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblNextSubBlockEQPList
            // 
            this.lblNextSubBlockEQPList.AutoSize = true;
            this.lblNextSubBlockEQPList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNextSubBlockEQPList.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNextSubBlockEQPList.Location = new System.Drawing.Point(318, 0);
            this.lblNextSubBlockEQPList.Name = "lblNextSubBlockEQPList";
            this.lblNextSubBlockEQPList.Size = new System.Drawing.Size(160, 1);
            this.lblNextSubBlockEQPList.TabIndex = 6;
            this.lblNextSubBlockEQPList.Text = "Next Sub Block EQP List";
            this.lblNextSubBlockEQPList.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblEnabled
            // 
            this.lblEnabled.AutoSize = true;
            this.lblEnabled.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEnabled.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEnabled.Location = new System.Drawing.Point(318, 0);
            this.lblEnabled.Name = "lblEnabled";
            this.lblEnabled.Size = new System.Drawing.Size(160, 1);
            this.lblEnabled.TabIndex = 7;
            this.lblEnabled.Text = "Enabled";
            this.lblEnabled.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblInterLockReplyNo
            // 
            this.lblInterLockReplyNo.AutoSize = true;
            this.lblInterLockReplyNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInterLockReplyNo.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInterLockReplyNo.Location = new System.Drawing.Point(637, 0);
            this.lblInterLockReplyNo.Name = "lblInterLockReplyNo";
            this.lblInterLockReplyNo.Size = new System.Drawing.Size(167, 1);
            this.lblInterLockReplyNo.TabIndex = 8;
            this.lblInterLockReplyNo.Text = "Interlock Reply No";
            this.lblInterLockReplyNo.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblRemark
            // 
            this.lblRemark.AutoSize = true;
            this.lblRemark.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRemark.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRemark.Location = new System.Drawing.Point(637, 0);
            this.lblRemark.Name = "lblRemark";
            this.lblRemark.Size = new System.Drawing.Size(167, 1);
            this.lblRemark.TabIndex = 9;
            this.lblRemark.Text = "Remark";
            this.lblRemark.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtSubBlockID
            // 
            this.txtSubBlockID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSubBlockID.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSubBlockID.Location = new System.Drawing.Point(146, 3);
            this.txtSubBlockID.Name = "txtSubBlockID";
            this.txtSubBlockID.Size = new System.Drawing.Size(166, 23);
            this.txtSubBlockID.TabIndex = 0;
            // 
            // txtStartEQP
            // 
            this.txtStartEQP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStartEQP.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStartEQP.Location = new System.Drawing.Point(146, 3);
            this.txtStartEQP.Name = "txtStartEQP";
            this.txtStartEQP.ReadOnly = true;
            this.txtStartEQP.Size = new System.Drawing.Size(166, 23);
            this.txtStartEQP.TabIndex = 1;
            this.txtStartEQP.Click += new System.EventHandler(this.txtEQP_Enter);
            this.txtStartEQP.Enter += new System.EventHandler(this.txtEQP_Enter);
            // 
            // txtControlEQP
            // 
            this.txtControlEQP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtControlEQP.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtControlEQP.Location = new System.Drawing.Point(146, 3);
            this.txtControlEQP.Name = "txtControlEQP";
            this.txtControlEQP.ReadOnly = true;
            this.txtControlEQP.Size = new System.Drawing.Size(166, 23);
            this.txtControlEQP.TabIndex = 2;
            this.txtControlEQP.Click += new System.EventHandler(this.txtEQP_Enter);
            this.txtControlEQP.Enter += new System.EventHandler(this.txtEQP_Enter);
            // 
            // txtInterLockNo
            // 
            this.txtInterLockNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInterLockNo.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInterLockNo.Location = new System.Drawing.Point(484, 3);
            this.txtInterLockNo.Name = "txtInterLockNo";
            this.txtInterLockNo.Size = new System.Drawing.Size(147, 23);
            this.txtInterLockNo.TabIndex = 4;
            // 
            // txtNextSubBlockEQP
            // 
            this.txtNextSubBlockEQP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNextSubBlockEQP.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNextSubBlockEQP.Location = new System.Drawing.Point(484, 3);
            this.txtNextSubBlockEQP.Name = "txtNextSubBlockEQP";
            this.txtNextSubBlockEQP.ReadOnly = true;
            this.txtNextSubBlockEQP.Size = new System.Drawing.Size(147, 23);
            this.txtNextSubBlockEQP.TabIndex = 5;
            this.txtNextSubBlockEQP.Click += new System.EventHandler(this.txtEQP_Enter);
            this.txtNextSubBlockEQP.Enter += new System.EventHandler(this.txtEQP_Enter);
            // 
            // txtNextSubBlockEQPList
            // 
            this.tlpEdit.SetColumnSpan(this.txtNextSubBlockEQPList, 3);
            this.txtNextSubBlockEQPList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNextSubBlockEQPList.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNextSubBlockEQPList.Location = new System.Drawing.Point(484, 3);
            this.txtNextSubBlockEQPList.Name = "txtNextSubBlockEQPList";
            this.txtNextSubBlockEQPList.ReadOnly = true;
            this.txtNextSubBlockEQPList.Size = new System.Drawing.Size(513, 23);
            this.txtNextSubBlockEQPList.TabIndex = 6;
            this.txtNextSubBlockEQPList.Click += new System.EventHandler(this.txtEQP_Enter);
            this.txtNextSubBlockEQPList.Enter += new System.EventHandler(this.txtEQP_Enter);
            // 
            // txtInterLockReplyNo
            // 
            this.txtInterLockReplyNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInterLockReplyNo.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInterLockReplyNo.Location = new System.Drawing.Point(810, 3);
            this.txtInterLockReplyNo.Name = "txtInterLockReplyNo";
            this.txtInterLockReplyNo.Size = new System.Drawing.Size(187, 23);
            this.txtInterLockReplyNo.TabIndex = 8;
            // 
            // txtRemark
            // 
            this.txtRemark.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRemark.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRemark.Location = new System.Drawing.Point(810, 3);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Size = new System.Drawing.Size(187, 23);
            this.txtRemark.TabIndex = 9;
            // 
            // cmbStartEventMsg
            // 
            this.cmbStartEventMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbStartEventMsg.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbStartEventMsg.FormattingEnabled = true;
            this.cmbStartEventMsg.Location = new System.Drawing.Point(146, 3);
            this.cmbStartEventMsg.Name = "cmbStartEventMsg";
            this.cmbStartEventMsg.Size = new System.Drawing.Size(166, 23);
            this.cmbStartEventMsg.TabIndex = 3;
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Location = new System.Drawing.Point(484, 3);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(15, 1);
            this.chkEnabled.TabIndex = 7;
            this.chkEnabled.UseVisualStyleBackColor = true;
            // 
            // flpButtom
            // 
            this.flpButtom.Controls.Add(this.btnAdd);
            this.flpButtom.Controls.Add(this.btnDelete);
            this.flpButtom.Controls.Add(this.btnModify);
            this.flpButtom.Controls.Add(this.btnEnableAll);
            this.flpButtom.Controls.Add(this.btnDisableAll);
            this.flpButtom.Controls.Add(this.btnSave);
            this.flpButtom.Controls.Add(this.btnRefresh);
            this.flpButtom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButtom.Location = new System.Drawing.Point(0, 0);
            this.flpButtom.Name = "flpButtom";
            this.flpButtom.Size = new System.Drawing.Size(91, 637);
            this.flpButtom.TabIndex = 7;
            // 
            // btnAdd
            // 
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAdd.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(3, 3);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(80, 50);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnDelete.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(3, 55);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(80, 50);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnModify
            // 
            this.btnModify.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnModify.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModify.Location = new System.Drawing.Point(3, 107);
            this.btnModify.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(80, 50);
            this.btnModify.TabIndex = 2;
            this.btnModify.Text = "Modify";
            this.btnModify.UseVisualStyleBackColor = true;
            this.btnModify.Click += new System.EventHandler(this.btnModify_Click);
            // 
            // btnEnableAll
            // 
            this.btnEnableAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnEnableAll.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEnableAll.Location = new System.Drawing.Point(3, 159);
            this.btnEnableAll.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.btnEnableAll.Name = "btnEnableAll";
            this.btnEnableAll.Size = new System.Drawing.Size(80, 50);
            this.btnEnableAll.TabIndex = 5;
            this.btnEnableAll.Text = "Enable All";
            this.btnEnableAll.UseVisualStyleBackColor = true;
            this.btnEnableAll.Click += new System.EventHandler(this.btnEnableAll_Click);
            // 
            // btnDisableAll
            // 
            this.btnDisableAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDisableAll.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDisableAll.Location = new System.Drawing.Point(3, 211);
            this.btnDisableAll.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.btnDisableAll.Name = "btnDisableAll";
            this.btnDisableAll.Size = new System.Drawing.Size(80, 50);
            this.btnDisableAll.TabIndex = 6;
            this.btnDisableAll.Text = "Disable All";
            this.btnDisableAll.UseVisualStyleBackColor = true;
            this.btnDisableAll.Click += new System.EventHandler(this.btnDisableAll_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(3, 263);
            this.btnSave.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 50);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(3, 315);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(80, 50);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // FormSubBlock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1101, 668);
            this.Name = "FormSubBlock";
            this.Text = "FormSubBlock";
            this.Load += new System.EventHandler(this.FormSubBlock_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.spcSubBlock.Panel1.ResumeLayout(false);
            this.spcSubBlock.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcSubBlock)).EndInit();
            this.spcSubBlock.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.tlpBase.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tlpEdit.ResumeLayout(false);
            this.tlpEdit.PerformLayout();
            this.flpButtom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spcSubBlock;
        private System.Windows.Forms.Panel pnlLayout;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Button btnModify;
        private System.Windows.Forms.TableLayoutPanel tlpEdit;
        private System.Windows.Forms.Label lblSubBlockID;
        private System.Windows.Forms.Label lblStartEQP;
        private System.Windows.Forms.Label lblControlEQP;
        private System.Windows.Forms.Label lblStartEventMsg;
        private System.Windows.Forms.Label lblInterLockNo;
        private System.Windows.Forms.Label lblNextSubBlockEQP;
        private System.Windows.Forms.Label lblNextSubBlockEQPList;
        private System.Windows.Forms.Label lblEnabled;
        private System.Windows.Forms.Label lblInterLockReplyNo;
        private System.Windows.Forms.Label lblRemark;
        private System.Windows.Forms.TextBox txtSubBlockID;
        private System.Windows.Forms.TextBox txtStartEQP;
        private System.Windows.Forms.TextBox txtControlEQP;
        private System.Windows.Forms.TextBox txtInterLockNo;
        private System.Windows.Forms.TextBox txtNextSubBlockEQP;
        private System.Windows.Forms.TextBox txtNextSubBlockEQPList;
        private System.Windows.Forms.TextBox txtInterLockReplyNo;
        private System.Windows.Forms.TextBox txtRemark;
        private System.Windows.Forms.ComboBox cmbStartEventMsg;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.Button btnDisableAll;
        private System.Windows.Forms.Button btnEnableAll;
        private System.Windows.Forms.FlowLayoutPanel flpButtom;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtOBJECTKEY;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtLINEID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtSERVERNAME;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtSUBBLOCKID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtSTARTEQP;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtCONTROLEQP;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtSTARTEVENTMSG;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtINTERLOCKNO;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtNEXTSUBBLOCKEQP;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtNEXTSUBBLOCKEQPLIST;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvchkENABLED;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtINTERLOCKREPLYNO;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtREMARK;
    }
}