namespace UniOPI
{
    partial class FormIncompleteCSTResend
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
            this.btnRefreshIncompleteCST = new System.Windows.Forms.Button();
            this.dgvIncompeteCst = new System.Windows.Forms.DataGridView();
            this.colUpdateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPortID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCassetteID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCSTSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMESTrxID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNGReason = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.grbHeader = new System.Windows.Forms.GroupBox();
            this.flpHeader = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlPortID = new System.Windows.Forms.Panel();
            this.lblPortID = new System.Windows.Forms.Label();
            this.txtPortID = new System.Windows.Forms.TextBox();
            this.pnlIncompleteDate = new System.Windows.Forms.Panel();
            this.lblIncompleteDate = new System.Windows.Forms.Label();
            this.txtIncompleteDate = new System.Windows.Forms.TextBox();
            this.pnlSampleFlag = new System.Windows.Forms.Panel();
            this.lblSampleFlag = new System.Windows.Forms.Label();
            this.txtSampleFlag = new System.Windows.Forms.TextBox();
            this.pnlCassetteID = new System.Windows.Forms.Panel();
            this.lblCassetteID = new System.Windows.Forms.Label();
            this.txtCassetteID = new System.Windows.Forms.TextBox();
            this.pnlCarrierName = new System.Windows.Forms.Panel();
            this.lblCarrierName = new System.Windows.Forms.Label();
            this.txtCarrierName = new System.Windows.Forms.TextBox();
            this.pnlLineRecipeName = new System.Windows.Forms.Panel();
            this.lblLineRecipeName = new System.Windows.Forms.Label();
            this.txtLineRecipeName = new System.Windows.Forms.TextBox();
            this.pnlPPID = new System.Windows.Forms.Panel();
            this.txtPPID = new System.Windows.Forms.TextBox();
            this.lblPPID = new System.Windows.Forms.Label();
            this.pnlHostLineRecipeName = new System.Windows.Forms.Panel();
            this.lblHostLineRecipeName = new System.Windows.Forms.Label();
            this.txtHostLineRecipeName = new System.Windows.Forms.TextBox();
            this.pnlHostPPID = new System.Windows.Forms.Panel();
            this.lblHostPPID = new System.Windows.Forms.Label();
            this.txtHostPPID = new System.Windows.Forms.TextBox();
            this.pnlReturnMsg = new System.Windows.Forms.Panel();
            this.lblReturnMsg = new System.Windows.Forms.Label();
            this.txtReturnMsg = new System.Windows.Forms.TextBox();
            this.pnlData = new System.Windows.Forms.Panel();
            this.trvData = new System.Windows.Forms.TreeView();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btn_DeleteFile = new System.Windows.Forms.Button();
            this.btn_Resend = new System.Windows.Forms.Button();
            this.btn_Save = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIncompeteCst)).BeginInit();
            this.tlpBase.SuspendLayout();
            this.grbHeader.SuspendLayout();
            this.flpHeader.SuspendLayout();
            this.pnlPortID.SuspendLayout();
            this.pnlIncompleteDate.SuspendLayout();
            this.pnlSampleFlag.SuspendLayout();
            this.pnlCassetteID.SuspendLayout();
            this.pnlCarrierName.SuspendLayout();
            this.pnlLineRecipeName.SuspendLayout();
            this.pnlPPID.SuspendLayout();
            this.pnlHostLineRecipeName.SuspendLayout();
            this.pnlHostPPID.SuspendLayout();
            this.pnlReturnMsg.SuspendLayout();
            this.pnlData.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1080, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(1140, 618);
            // 
            // btnRefreshIncompleteCST
            // 
            this.btnRefreshIncompleteCST.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnRefreshIncompleteCST.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefreshIncompleteCST.Location = new System.Drawing.Point(3, 555);
            this.btnRefreshIncompleteCST.Name = "btnRefreshIncompleteCST";
            this.btnRefreshIncompleteCST.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnRefreshIncompleteCST.Size = new System.Drawing.Size(192, 28);
            this.btnRefreshIncompleteCST.TabIndex = 17;
            this.btnRefreshIncompleteCST.Text = "Refresh";
            this.btnRefreshIncompleteCST.UseVisualStyleBackColor = true;
            this.btnRefreshIncompleteCST.Click += new System.EventHandler(this.btnRefreshIncompleteCST_Click);
            // 
            // dgvIncompeteCst
            // 
            this.dgvIncompeteCst.AllowUserToAddRows = false;
            this.dgvIncompeteCst.AllowUserToDeleteRows = false;
            this.dgvIncompeteCst.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvIncompeteCst.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvIncompeteCst.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvIncompeteCst.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvIncompeteCst.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIncompeteCst.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colUpdateTime,
            this.colPortID,
            this.colCassetteID,
            this.colCSTSeqNo,
            this.colMESTrxID,
            this.colFileName,
            this.colState,
            this.colNGReason});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvIncompeteCst.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvIncompeteCst.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvIncompeteCst.Location = new System.Drawing.Point(3, 3);
            this.dgvIncompeteCst.Name = "dgvIncompeteCst";
            this.dgvIncompeteCst.ReadOnly = true;
            this.dgvIncompeteCst.RowHeadersVisible = false;
            this.tlpBase.SetRowSpan(this.dgvIncompeteCst, 2);
            this.dgvIncompeteCst.RowTemplate.Height = 24;
            this.dgvIncompeteCst.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvIncompeteCst.Size = new System.Drawing.Size(192, 546);
            this.dgvIncompeteCst.TabIndex = 22;
            this.dgvIncompeteCst.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvIncompeteCst_CellDoubleClick);
            // 
            // colUpdateTime
            // 
            this.colUpdateTime.HeaderText = "DateTime";
            this.colUpdateTime.Name = "colUpdateTime";
            this.colUpdateTime.ReadOnly = true;
            this.colUpdateTime.Width = 125;
            // 
            // colPortID
            // 
            this.colPortID.HeaderText = "PortID";
            this.colPortID.Name = "colPortID";
            this.colPortID.ReadOnly = true;
            this.colPortID.Width = 55;
            // 
            // colCassetteID
            // 
            this.colCassetteID.HeaderText = "Cassette ID";
            this.colCassetteID.MinimumWidth = 120;
            this.colCassetteID.Name = "colCassetteID";
            this.colCassetteID.ReadOnly = true;
            this.colCassetteID.Width = 120;
            // 
            // colCSTSeqNo
            // 
            this.colCSTSeqNo.HeaderText = "CST Seq No";
            this.colCSTSeqNo.Name = "colCSTSeqNo";
            this.colCSTSeqNo.ReadOnly = true;
            this.colCSTSeqNo.Visible = false;
            this.colCSTSeqNo.Width = 120;
            // 
            // colMESTrxID
            // 
            this.colMESTrxID.HeaderText = "MES Trx ID";
            this.colMESTrxID.Name = "colMESTrxID";
            this.colMESTrxID.ReadOnly = true;
            this.colMESTrxID.Visible = false;
            this.colMESTrxID.Width = 120;
            // 
            // colFileName
            // 
            this.colFileName.HeaderText = "File Name";
            this.colFileName.Name = "colFileName";
            this.colFileName.ReadOnly = true;
            this.colFileName.Width = 120;
            // 
            // colState
            // 
            this.colState.HeaderText = "State";
            this.colState.Name = "colState";
            this.colState.ReadOnly = true;
            this.colState.Width = 80;
            // 
            // colNGReason
            // 
            this.colNGReason.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colNGReason.HeaderText = "NG Reason";
            this.colNGReason.Name = "colNGReason";
            this.colNGReason.ReadOnly = true;
            this.colNGReason.Visible = false;
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 2;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 198F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.grbHeader, 1, 0);
            this.tlpBase.Controls.Add(this.dgvIncompeteCst, 0, 0);
            this.tlpBase.Controls.Add(this.pnlData, 1, 1);
            this.tlpBase.Controls.Add(this.pnlButton, 1, 2);
            this.tlpBase.Controls.Add(this.btnRefreshIncompleteCST, 0, 2);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 3;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpBase.Size = new System.Drawing.Size(1140, 587);
            this.tlpBase.TabIndex = 5;
            // 
            // grbHeader
            // 
            this.grbHeader.Controls.Add(this.flpHeader);
            this.grbHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbHeader.Location = new System.Drawing.Point(201, 0);
            this.grbHeader.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.grbHeader.Name = "grbHeader";
            this.grbHeader.Size = new System.Drawing.Size(936, 127);
            this.grbHeader.TabIndex = 24;
            this.grbHeader.TabStop = false;
            // 
            // flpHeader
            // 
            this.flpHeader.Controls.Add(this.pnlPortID);
            this.flpHeader.Controls.Add(this.pnlIncompleteDate);
            this.flpHeader.Controls.Add(this.pnlSampleFlag);
            this.flpHeader.Controls.Add(this.pnlCassetteID);
            this.flpHeader.Controls.Add(this.pnlCarrierName);
            this.flpHeader.Controls.Add(this.pnlLineRecipeName);
            this.flpHeader.Controls.Add(this.pnlPPID);
            this.flpHeader.Controls.Add(this.pnlHostLineRecipeName);
            this.flpHeader.Controls.Add(this.pnlHostPPID);
            this.flpHeader.Controls.Add(this.pnlReturnMsg);
            this.flpHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpHeader.Location = new System.Drawing.Point(3, 18);
            this.flpHeader.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.flpHeader.Name = "flpHeader";
            this.flpHeader.Size = new System.Drawing.Size(930, 106);
            this.flpHeader.TabIndex = 20;
            // 
            // pnlPortID
            // 
            this.pnlPortID.Controls.Add(this.lblPortID);
            this.pnlPortID.Controls.Add(this.txtPortID);
            this.pnlPortID.Location = new System.Drawing.Point(1, 1);
            this.pnlPortID.Margin = new System.Windows.Forms.Padding(1);
            this.pnlPortID.Name = "pnlPortID";
            this.pnlPortID.Size = new System.Drawing.Size(111, 23);
            this.pnlPortID.TabIndex = 18;
            this.pnlPortID.Tag = "PORTID";
            // 
            // lblPortID
            // 
            this.lblPortID.BackColor = System.Drawing.Color.Black;
            this.lblPortID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPortID.ForeColor = System.Drawing.Color.White;
            this.lblPortID.Location = new System.Drawing.Point(1, 1);
            this.lblPortID.Name = "lblPortID";
            this.lblPortID.Size = new System.Drawing.Size(65, 23);
            this.lblPortID.TabIndex = 5;
            this.lblPortID.Text = "Port ID";
            this.lblPortID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPortID
            // 
            this.txtPortID.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPortID.Location = new System.Drawing.Point(65, 1);
            this.txtPortID.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtPortID.Name = "txtPortID";
            this.txtPortID.ReadOnly = true;
            this.txtPortID.Size = new System.Drawing.Size(45, 23);
            this.txtPortID.TabIndex = 0;
            this.txtPortID.Tag = "";
            this.txtPortID.Text = "01";
            // 
            // pnlIncompleteDate
            // 
            this.pnlIncompleteDate.Controls.Add(this.lblIncompleteDate);
            this.pnlIncompleteDate.Controls.Add(this.txtIncompleteDate);
            this.pnlIncompleteDate.Location = new System.Drawing.Point(114, 1);
            this.pnlIncompleteDate.Margin = new System.Windows.Forms.Padding(1);
            this.pnlIncompleteDate.Name = "pnlIncompleteDate";
            this.pnlIncompleteDate.Size = new System.Drawing.Size(293, 23);
            this.pnlIncompleteDate.TabIndex = 8;
            this.pnlIncompleteDate.Tag = "INCOMPLETEDATE";
            // 
            // lblIncompleteDate
            // 
            this.lblIncompleteDate.BackColor = System.Drawing.Color.Black;
            this.lblIncompleteDate.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIncompleteDate.ForeColor = System.Drawing.Color.White;
            this.lblIncompleteDate.Location = new System.Drawing.Point(1, 1);
            this.lblIncompleteDate.Name = "lblIncompleteDate";
            this.lblIncompleteDate.Size = new System.Drawing.Size(100, 23);
            this.lblIncompleteDate.TabIndex = 5;
            this.lblIncompleteDate.Text = "Incomplete Date";
            this.lblIncompleteDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtIncompleteDate
            // 
            this.txtIncompleteDate.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtIncompleteDate.Location = new System.Drawing.Point(100, 1);
            this.txtIncompleteDate.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtIncompleteDate.Name = "txtIncompleteDate";
            this.txtIncompleteDate.ReadOnly = true;
            this.txtIncompleteDate.Size = new System.Drawing.Size(190, 23);
            this.txtIncompleteDate.TabIndex = 0;
            this.txtIncompleteDate.Tag = "";
            this.txtIncompleteDate.Text = "2015-04-15 23:56:16.403";
            // 
            // pnlSampleFlag
            // 
            this.pnlSampleFlag.Controls.Add(this.lblSampleFlag);
            this.pnlSampleFlag.Controls.Add(this.txtSampleFlag);
            this.pnlSampleFlag.Location = new System.Drawing.Point(409, 1);
            this.pnlSampleFlag.Margin = new System.Windows.Forms.Padding(1);
            this.pnlSampleFlag.Name = "pnlSampleFlag";
            this.pnlSampleFlag.Size = new System.Drawing.Size(336, 23);
            this.pnlSampleFlag.TabIndex = 26;
            this.pnlSampleFlag.Tag = "CARRIERNAME";
            this.pnlSampleFlag.Visible = false;
            // 
            // lblSampleFlag
            // 
            this.lblSampleFlag.BackColor = System.Drawing.Color.Black;
            this.lblSampleFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSampleFlag.ForeColor = System.Drawing.Color.White;
            this.lblSampleFlag.Location = new System.Drawing.Point(1, 1);
            this.lblSampleFlag.Name = "lblSampleFlag";
            this.lblSampleFlag.Size = new System.Drawing.Size(100, 23);
            this.lblSampleFlag.TabIndex = 5;
            this.lblSampleFlag.Text = "Sample Flag";
            this.lblSampleFlag.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtSampleFlag
            // 
            this.txtSampleFlag.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSampleFlag.Location = new System.Drawing.Point(100, 1);
            this.txtSampleFlag.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtSampleFlag.Name = "txtSampleFlag";
            this.txtSampleFlag.Size = new System.Drawing.Size(150, 23);
            this.txtSampleFlag.TabIndex = 0;
            this.txtSampleFlag.Tag = "";
            this.txtSampleFlag.Text = "TBA176";
            // 
            // pnlCassetteID
            // 
            this.pnlCassetteID.Controls.Add(this.lblCassetteID);
            this.pnlCassetteID.Controls.Add(this.txtCassetteID);
            this.pnlCassetteID.Location = new System.Drawing.Point(1, 26);
            this.pnlCassetteID.Margin = new System.Windows.Forms.Padding(1);
            this.pnlCassetteID.Name = "pnlCassetteID";
            this.pnlCassetteID.Size = new System.Drawing.Size(255, 23);
            this.pnlCassetteID.TabIndex = 19;
            this.pnlCassetteID.Tag = "CASSETTEID";
            // 
            // lblCassetteID
            // 
            this.lblCassetteID.BackColor = System.Drawing.Color.Black;
            this.lblCassetteID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCassetteID.ForeColor = System.Drawing.Color.White;
            this.lblCassetteID.Location = new System.Drawing.Point(1, 1);
            this.lblCassetteID.Name = "lblCassetteID";
            this.lblCassetteID.Size = new System.Drawing.Size(100, 23);
            this.lblCassetteID.TabIndex = 5;
            this.lblCassetteID.Text = "Cassette ID";
            this.lblCassetteID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCassetteID
            // 
            this.txtCassetteID.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCassetteID.Location = new System.Drawing.Point(100, 1);
            this.txtCassetteID.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtCassetteID.Name = "txtCassetteID";
            this.txtCassetteID.Size = new System.Drawing.Size(150, 23);
            this.txtCassetteID.TabIndex = 0;
            this.txtCassetteID.Tag = "";
            this.txtCassetteID.Text = "TBA176";
            // 
            // pnlCarrierName
            // 
            this.pnlCarrierName.Controls.Add(this.lblCarrierName);
            this.pnlCarrierName.Controls.Add(this.txtCarrierName);
            this.pnlCarrierName.Location = new System.Drawing.Point(258, 26);
            this.pnlCarrierName.Margin = new System.Windows.Forms.Padding(1);
            this.pnlCarrierName.Name = "pnlCarrierName";
            this.pnlCarrierName.Size = new System.Drawing.Size(255, 23);
            this.pnlCarrierName.TabIndex = 20;
            this.pnlCarrierName.Tag = "CARRIERNAME";
            // 
            // lblCarrierName
            // 
            this.lblCarrierName.BackColor = System.Drawing.Color.Black;
            this.lblCarrierName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCarrierName.ForeColor = System.Drawing.Color.White;
            this.lblCarrierName.Location = new System.Drawing.Point(1, 1);
            this.lblCarrierName.Name = "lblCarrierName";
            this.lblCarrierName.Size = new System.Drawing.Size(100, 23);
            this.lblCarrierName.TabIndex = 5;
            this.lblCarrierName.Text = "Carrier Name";
            this.lblCarrierName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCarrierName
            // 
            this.txtCarrierName.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCarrierName.Location = new System.Drawing.Point(100, 1);
            this.txtCarrierName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtCarrierName.Name = "txtCarrierName";
            this.txtCarrierName.Size = new System.Drawing.Size(150, 23);
            this.txtCarrierName.TabIndex = 0;
            this.txtCarrierName.Tag = "";
            this.txtCarrierName.Text = "TBA176";
            // 
            // pnlLineRecipeName
            // 
            this.pnlLineRecipeName.Controls.Add(this.lblLineRecipeName);
            this.pnlLineRecipeName.Controls.Add(this.txtLineRecipeName);
            this.pnlLineRecipeName.Location = new System.Drawing.Point(515, 26);
            this.pnlLineRecipeName.Margin = new System.Windows.Forms.Padding(1);
            this.pnlLineRecipeName.Name = "pnlLineRecipeName";
            this.pnlLineRecipeName.Size = new System.Drawing.Size(250, 23);
            this.pnlLineRecipeName.TabIndex = 21;
            this.pnlLineRecipeName.Tag = "LINERECIPENAME";
            // 
            // lblLineRecipeName
            // 
            this.lblLineRecipeName.BackColor = System.Drawing.Color.Black;
            this.lblLineRecipeName.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLineRecipeName.ForeColor = System.Drawing.Color.White;
            this.lblLineRecipeName.Location = new System.Drawing.Point(1, 1);
            this.lblLineRecipeName.Name = "lblLineRecipeName";
            this.lblLineRecipeName.Size = new System.Drawing.Size(110, 23);
            this.lblLineRecipeName.TabIndex = 5;
            this.lblLineRecipeName.Text = "Line Recipe Name";
            this.lblLineRecipeName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtLineRecipeName
            // 
            this.txtLineRecipeName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.txtLineRecipeName.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLineRecipeName.Location = new System.Drawing.Point(110, 1);
            this.txtLineRecipeName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtLineRecipeName.Name = "txtLineRecipeName";
            this.txtLineRecipeName.ReadOnly = true;
            this.txtLineRecipeName.Size = new System.Drawing.Size(138, 23);
            this.txtLineRecipeName.TabIndex = 0;
            this.txtLineRecipeName.Tag = "";
            this.txtLineRecipeName.DoubleClick += new System.EventHandler(this.LineRecipeName_DoubleClick);
            // 
            // pnlPPID
            // 
            this.pnlPPID.Controls.Add(this.txtPPID);
            this.pnlPPID.Controls.Add(this.lblPPID);
            this.pnlPPID.Location = new System.Drawing.Point(1, 51);
            this.pnlPPID.Margin = new System.Windows.Forms.Padding(1);
            this.pnlPPID.Name = "pnlPPID";
            this.pnlPPID.Size = new System.Drawing.Size(665, 23);
            this.pnlPPID.TabIndex = 23;
            this.pnlPPID.Tag = "PPID";
            // 
            // txtPPID
            // 
            this.txtPPID.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPPID.Location = new System.Drawing.Point(80, 1);
            this.txtPPID.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtPPID.Name = "txtPPID";
            this.txtPPID.ReadOnly = true;
            this.txtPPID.Size = new System.Drawing.Size(582, 23);
            this.txtPPID.TabIndex = 0;
            this.txtPPID.Tag = "";
            // 
            // lblPPID
            // 
            this.lblPPID.BackColor = System.Drawing.Color.Black;
            this.lblPPID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPPID.ForeColor = System.Drawing.Color.White;
            this.lblPPID.Location = new System.Drawing.Point(1, 1);
            this.lblPPID.Name = "lblPPID";
            this.lblPPID.Size = new System.Drawing.Size(80, 23);
            this.lblPPID.TabIndex = 5;
            this.lblPPID.Text = "PPID";
            this.lblPPID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlHostLineRecipeName
            // 
            this.pnlHostLineRecipeName.Controls.Add(this.lblHostLineRecipeName);
            this.pnlHostLineRecipeName.Controls.Add(this.txtHostLineRecipeName);
            this.pnlHostLineRecipeName.Location = new System.Drawing.Point(668, 51);
            this.pnlHostLineRecipeName.Margin = new System.Windows.Forms.Padding(1);
            this.pnlHostLineRecipeName.Name = "pnlHostLineRecipeName";
            this.pnlHostLineRecipeName.Size = new System.Drawing.Size(250, 23);
            this.pnlHostLineRecipeName.TabIndex = 22;
            this.pnlHostLineRecipeName.Tag = "HOSTLINERECIPENAME";
            // 
            // lblHostLineRecipeName
            // 
            this.lblHostLineRecipeName.BackColor = System.Drawing.Color.Black;
            this.lblHostLineRecipeName.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHostLineRecipeName.ForeColor = System.Drawing.Color.White;
            this.lblHostLineRecipeName.Location = new System.Drawing.Point(1, 1);
            this.lblHostLineRecipeName.Name = "lblHostLineRecipeName";
            this.lblHostLineRecipeName.Size = new System.Drawing.Size(110, 23);
            this.lblHostLineRecipeName.TabIndex = 5;
            this.lblHostLineRecipeName.Text = "Host Recipe Name";
            this.lblHostLineRecipeName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtHostLineRecipeName
            // 
            this.txtHostLineRecipeName.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHostLineRecipeName.Location = new System.Drawing.Point(110, 1);
            this.txtHostLineRecipeName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtHostLineRecipeName.Name = "txtHostLineRecipeName";
            this.txtHostLineRecipeName.ReadOnly = true;
            this.txtHostLineRecipeName.Size = new System.Drawing.Size(138, 23);
            this.txtHostLineRecipeName.TabIndex = 0;
            this.txtHostLineRecipeName.Tag = "";
            // 
            // pnlHostPPID
            // 
            this.pnlHostPPID.Controls.Add(this.lblHostPPID);
            this.pnlHostPPID.Controls.Add(this.txtHostPPID);
            this.pnlHostPPID.Location = new System.Drawing.Point(1, 76);
            this.pnlHostPPID.Margin = new System.Windows.Forms.Padding(1);
            this.pnlHostPPID.Name = "pnlHostPPID";
            this.pnlHostPPID.Size = new System.Drawing.Size(665, 23);
            this.pnlHostPPID.TabIndex = 25;
            this.pnlHostPPID.Tag = "HOSTPPID";
            // 
            // lblHostPPID
            // 
            this.lblHostPPID.BackColor = System.Drawing.Color.Black;
            this.lblHostPPID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHostPPID.ForeColor = System.Drawing.Color.White;
            this.lblHostPPID.Location = new System.Drawing.Point(1, 1);
            this.lblHostPPID.Name = "lblHostPPID";
            this.lblHostPPID.Size = new System.Drawing.Size(80, 23);
            this.lblHostPPID.TabIndex = 5;
            this.lblHostPPID.Text = "Host PPID";
            this.lblHostPPID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtHostPPID
            // 
            this.txtHostPPID.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHostPPID.Location = new System.Drawing.Point(80, 1);
            this.txtHostPPID.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtHostPPID.Name = "txtHostPPID";
            this.txtHostPPID.ReadOnly = true;
            this.txtHostPPID.Size = new System.Drawing.Size(582, 23);
            this.txtHostPPID.TabIndex = 0;
            this.txtHostPPID.Tag = "";
            // 
            // pnlReturnMsg
            // 
            this.pnlReturnMsg.Controls.Add(this.lblReturnMsg);
            this.pnlReturnMsg.Controls.Add(this.txtReturnMsg);
            this.pnlReturnMsg.Location = new System.Drawing.Point(1, 101);
            this.pnlReturnMsg.Margin = new System.Windows.Forms.Padding(1);
            this.pnlReturnMsg.Name = "pnlReturnMsg";
            this.pnlReturnMsg.Size = new System.Drawing.Size(917, 23);
            this.pnlReturnMsg.TabIndex = 24;
            this.pnlReturnMsg.Tag = "RETURNMSG";
            // 
            // lblReturnMsg
            // 
            this.lblReturnMsg.BackColor = System.Drawing.Color.Black;
            this.lblReturnMsg.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReturnMsg.ForeColor = System.Drawing.Color.White;
            this.lblReturnMsg.Location = new System.Drawing.Point(1, 1);
            this.lblReturnMsg.Name = "lblReturnMsg";
            this.lblReturnMsg.Size = new System.Drawing.Size(110, 23);
            this.lblReturnMsg.TabIndex = 5;
            this.lblReturnMsg.Text = "Return Message";
            this.lblReturnMsg.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtReturnMsg
            // 
            this.txtReturnMsg.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtReturnMsg.Location = new System.Drawing.Point(110, 1);
            this.txtReturnMsg.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtReturnMsg.Name = "txtReturnMsg";
            this.txtReturnMsg.ReadOnly = true;
            this.txtReturnMsg.Size = new System.Drawing.Size(804, 23);
            this.txtReturnMsg.TabIndex = 0;
            this.txtReturnMsg.Tag = "";
            // 
            // pnlData
            // 
            this.pnlData.Controls.Add(this.trvData);
            this.pnlData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlData.Location = new System.Drawing.Point(201, 133);
            this.pnlData.Name = "pnlData";
            this.pnlData.Size = new System.Drawing.Size(936, 416);
            this.pnlData.TabIndex = 25;
            // 
            // trvData
            // 
            this.trvData.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.trvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trvData.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.trvData.Location = new System.Drawing.Point(0, 0);
            this.trvData.Name = "trvData";
            this.trvData.Size = new System.Drawing.Size(936, 416);
            this.trvData.TabIndex = 29;
            this.trvData.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trvData_MouseDown);
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btn_DeleteFile);
            this.pnlButton.Controls.Add(this.btn_Resend);
            this.pnlButton.Controls.Add(this.btn_Save);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(201, 555);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(936, 29);
            this.pnlButton.TabIndex = 26;
            // 
            // btn_DeleteFile
            // 
            this.btn_DeleteFile.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.btn_DeleteFile.Location = new System.Drawing.Point(798, 1);
            this.btn_DeleteFile.Name = "btn_DeleteFile";
            this.btn_DeleteFile.Size = new System.Drawing.Size(135, 28);
            this.btn_DeleteFile.TabIndex = 31;
            this.btn_DeleteFile.Tag = "DELETE";
            this.btn_DeleteFile.Text = "Delete File";
            this.btn_DeleteFile.UseVisualStyleBackColor = true;
            this.btn_DeleteFile.Click += new System.EventHandler(this.CassetteCommand_Click);
            // 
            // btn_Resend
            // 
            this.btn_Resend.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.btn_Resend.Location = new System.Drawing.Point(663, 1);
            this.btn_Resend.Name = "btn_Resend";
            this.btn_Resend.Size = new System.Drawing.Size(135, 28);
            this.btn_Resend.TabIndex = 30;
            this.btn_Resend.Tag = "RESEND";
            this.btn_Resend.Text = "Resend File";
            this.btn_Resend.UseVisualStyleBackColor = true;
            this.btn_Resend.Click += new System.EventHandler(this.CassetteCommand_Click);
            // 
            // btn_Save
            // 
            this.btn_Save.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.btn_Save.Location = new System.Drawing.Point(3, 1);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(135, 28);
            this.btn_Save.TabIndex = 26;
            this.btn_Save.Tag = "SAVE";
            this.btn_Save.Text = "Save";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // FormIncompleteCSTResend
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 618);
            this.Name = "FormIncompleteCSTResend";
            this.Text = "FormIncompleteCSTResend";
            this.Load += new System.EventHandler(this.FormIncompleteCSTResend_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvIncompeteCst)).EndInit();
            this.tlpBase.ResumeLayout(false);
            this.grbHeader.ResumeLayout(false);
            this.flpHeader.ResumeLayout(false);
            this.pnlPortID.ResumeLayout(false);
            this.pnlPortID.PerformLayout();
            this.pnlIncompleteDate.ResumeLayout(false);
            this.pnlIncompleteDate.PerformLayout();
            this.pnlSampleFlag.ResumeLayout(false);
            this.pnlSampleFlag.PerformLayout();
            this.pnlCassetteID.ResumeLayout(false);
            this.pnlCassetteID.PerformLayout();
            this.pnlCarrierName.ResumeLayout(false);
            this.pnlCarrierName.PerformLayout();
            this.pnlLineRecipeName.ResumeLayout(false);
            this.pnlLineRecipeName.PerformLayout();
            this.pnlPPID.ResumeLayout(false);
            this.pnlPPID.PerformLayout();
            this.pnlHostLineRecipeName.ResumeLayout(false);
            this.pnlHostLineRecipeName.PerformLayout();
            this.pnlHostPPID.ResumeLayout(false);
            this.pnlHostPPID.PerformLayout();
            this.pnlReturnMsg.ResumeLayout(false);
            this.pnlReturnMsg.PerformLayout();
            this.pnlData.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRefreshIncompleteCST;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.DataGridView dgvIncompeteCst;
        private System.Windows.Forms.GroupBox grbHeader;
        private System.Windows.Forms.FlowLayoutPanel flpHeader;
        private System.Windows.Forms.Panel pnlPortID;
        private System.Windows.Forms.Label lblPortID;
        private System.Windows.Forms.TextBox txtPortID;
        private System.Windows.Forms.Panel pnlIncompleteDate;
        private System.Windows.Forms.Label lblIncompleteDate;
        private System.Windows.Forms.TextBox txtIncompleteDate;
        private System.Windows.Forms.Panel pnlCassetteID;
        private System.Windows.Forms.Label lblCassetteID;
        private System.Windows.Forms.TextBox txtCassetteID;
        private System.Windows.Forms.Panel pnlCarrierName;
        private System.Windows.Forms.Label lblCarrierName;
        private System.Windows.Forms.TextBox txtCarrierName;
        private System.Windows.Forms.Panel pnlLineRecipeName;
        private System.Windows.Forms.Label lblLineRecipeName;
        private System.Windows.Forms.TextBox txtLineRecipeName;
        private System.Windows.Forms.Panel pnlPPID;
        private System.Windows.Forms.TextBox txtPPID;
        private System.Windows.Forms.Label lblPPID;
        private System.Windows.Forms.Panel pnlHostLineRecipeName;
        private System.Windows.Forms.Label lblHostLineRecipeName;
        private System.Windows.Forms.TextBox txtHostLineRecipeName;
        private System.Windows.Forms.Panel pnlHostPPID;
        private System.Windows.Forms.Label lblHostPPID;
        private System.Windows.Forms.TextBox txtHostPPID;
        private System.Windows.Forms.Panel pnlReturnMsg;
        private System.Windows.Forms.Label lblReturnMsg;
        private System.Windows.Forms.TextBox txtReturnMsg;
        private System.Windows.Forms.Panel pnlData;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Button btn_DeleteFile;
        private System.Windows.Forms.Button btn_Resend;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUpdateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPortID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCassetteID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCSTSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMESTrxID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colState;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNGReason;
        private System.Windows.Forms.Panel pnlSampleFlag;
        private System.Windows.Forms.Label lblSampleFlag;
        private System.Windows.Forms.TextBox txtSampleFlag;
        private System.Windows.Forms.TreeView trvData;
    }
}