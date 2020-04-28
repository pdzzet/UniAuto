namespace UniOPI
{
    partial class FormMonitorPLC
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.spcPLC = new System.Windows.Forms.SplitContainer();
            this.pnlCombox = new System.Windows.Forms.Panel();
            this.chkDetail = new System.Windows.Forms.CheckBox();
            this.cboLocalNode = new System.Windows.Forms.ComboBox();
            this.lblLocalNode = new System.Windows.Forms.Label();
            this.btnQuery = new System.Windows.Forms.Button();
            this.gbxContent = new System.Windows.Forms.GroupBox();
            this.spcTrx = new System.Windows.Forms.SplitContainer();
            this.dgvTrxName = new System.Windows.Forms.DataGridView();
            this.colPLCTrxName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTrxData = new System.Windows.Forms.DataGridView();
            this.colGroupName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDir = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEventName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDevcode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPoints = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSkipDecode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWPoints = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBPoints = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExpression = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcPLC)).BeginInit();
            this.spcPLC.Panel1.SuspendLayout();
            this.spcPLC.Panel2.SuspendLayout();
            this.spcPLC.SuspendLayout();
            this.pnlCombox.SuspendLayout();
            this.gbxContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcTrx)).BeginInit();
            this.spcTrx.Panel1.SuspendLayout();
            this.spcTrx.Panel2.SuspendLayout();
            this.spcTrx.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTrxName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTrxData)).BeginInit();
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
            this.spcBase.Panel2.Controls.Add(this.spcPLC);
            this.spcBase.Size = new System.Drawing.Size(1130, 545);
            // 
            // spcPLC
            // 
            this.spcPLC.BackColor = System.Drawing.Color.Transparent;
            this.spcPLC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcPLC.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spcPLC.Location = new System.Drawing.Point(0, 0);
            this.spcPLC.Name = "spcPLC";
            this.spcPLC.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcPLC.Panel1
            // 
            this.spcPLC.Panel1.Controls.Add(this.pnlCombox);
            // 
            // spcPLC.Panel2
            // 
            this.spcPLC.Panel2.Controls.Add(this.gbxContent);
            this.spcPLC.Size = new System.Drawing.Size(1130, 514);
            this.spcPLC.SplitterDistance = 52;
            this.spcPLC.TabIndex = 2;
            // 
            // pnlCombox
            // 
            this.pnlCombox.Controls.Add(this.chkDetail);
            this.pnlCombox.Controls.Add(this.cboLocalNode);
            this.pnlCombox.Controls.Add(this.lblLocalNode);
            this.pnlCombox.Controls.Add(this.btnQuery);
            this.pnlCombox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCombox.Location = new System.Drawing.Point(0, 0);
            this.pnlCombox.Name = "pnlCombox";
            this.pnlCombox.Size = new System.Drawing.Size(1130, 52);
            this.pnlCombox.TabIndex = 26;
            // 
            // chkDetail
            // 
            this.chkDetail.AutoSize = true;
            this.chkDetail.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.chkDetail.Location = new System.Drawing.Point(963, 14);
            this.chkDetail.Name = "chkDetail";
            this.chkDetail.Size = new System.Drawing.Size(155, 25);
            this.chkDetail.TabIndex = 12;
            this.chkDetail.Text = "Show Detail Fields";
            this.chkDetail.UseVisualStyleBackColor = true;
            this.chkDetail.CheckedChanged += new System.EventHandler(this.chkDetail_CheckedChanged);
            // 
            // cboLocalNode
            // 
            this.cboLocalNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocalNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboLocalNode.FormattingEnabled = true;
            this.cboLocalNode.Location = new System.Drawing.Point(62, 12);
            this.cboLocalNode.Name = "cboLocalNode";
            this.cboLocalNode.Size = new System.Drawing.Size(252, 29);
            this.cboLocalNode.TabIndex = 11;
            // 
            // lblLocalNode
            // 
            this.lblLocalNode.BackColor = System.Drawing.Color.Transparent;
            this.lblLocalNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLocalNode.Location = new System.Drawing.Point(8, 17);
            this.lblLocalNode.Name = "lblLocalNode";
            this.lblLocalNode.Size = new System.Drawing.Size(69, 21);
            this.lblLocalNode.TabIndex = 10;
            this.lblLocalNode.Text = "Local";
            // 
            // btnQuery
            // 
            this.btnQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.Location = new System.Drawing.Point(323, 11);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(110, 29);
            this.btnQuery.TabIndex = 9;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // gbxContent
            // 
            this.gbxContent.Controls.Add(this.spcTrx);
            this.gbxContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxContent.Location = new System.Drawing.Point(0, 0);
            this.gbxContent.Name = "gbxContent";
            this.gbxContent.Size = new System.Drawing.Size(1130, 458);
            this.gbxContent.TabIndex = 16;
            this.gbxContent.TabStop = false;
            // 
            // spcTrx
            // 
            this.spcTrx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcTrx.Location = new System.Drawing.Point(3, 18);
            this.spcTrx.Name = "spcTrx";
            // 
            // spcTrx.Panel1
            // 
            this.spcTrx.Panel1.Controls.Add(this.dgvTrxName);
            // 
            // spcTrx.Panel2
            // 
            this.spcTrx.Panel2.Controls.Add(this.dgvTrxData);
            this.spcTrx.Size = new System.Drawing.Size(1124, 437);
            this.spcTrx.SplitterDistance = 374;
            this.spcTrx.TabIndex = 0;
            // 
            // dgvTrxName
            // 
            this.dgvTrxName.AllowUserToAddRows = false;
            this.dgvTrxName.AllowUserToDeleteRows = false;
            this.dgvTrxName.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTrxName.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvTrxName.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTrxName.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colPLCTrxName});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvTrxName.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvTrxName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTrxName.Location = new System.Drawing.Point(0, 0);
            this.dgvTrxName.Name = "dgvTrxName";
            this.dgvTrxName.ReadOnly = true;
            this.dgvTrxName.RowHeadersWidth = 5;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvTrxName.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvTrxName.RowTemplate.Height = 24;
            this.dgvTrxName.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTrxName.Size = new System.Drawing.Size(374, 437);
            this.dgvTrxName.TabIndex = 12;
            this.dgvTrxName.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTrxName_CellClick);
            // 
            // colPLCTrxName
            // 
            this.colPLCTrxName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colPLCTrxName.DataPropertyName = "PLCTRXNAME";
            this.colPLCTrxName.HeaderText = "PLC Trx Name";
            this.colPLCTrxName.Name = "colPLCTrxName";
            this.colPLCTrxName.ReadOnly = true;
            // 
            // dgvTrxData
            // 
            this.dgvTrxData.AllowUserToAddRows = false;
            this.dgvTrxData.AllowUserToDeleteRows = false;
            this.dgvTrxData.AllowUserToResizeRows = false;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvTrxData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvTrxData.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Calibri", 12.75F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTrxData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvTrxData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTrxData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colGroupName,
            this.colDir,
            this.colEventName,
            this.colDevcode,
            this.colAddress,
            this.colRAddress,
            this.colPoints,
            this.colSkipDecode,
            this.colItemName,
            this.colItemValue,
            this.colWOffset,
            this.colWPoints,
            this.colBOffset,
            this.colBPoints,
            this.colExpression});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Calibri", 11.25F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvTrxData.DefaultCellStyle = dataGridViewCellStyle6;
            this.dgvTrxData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTrxData.Location = new System.Drawing.Point(0, 0);
            this.dgvTrxData.Name = "dgvTrxData";
            this.dgvTrxData.ReadOnly = true;
            this.dgvTrxData.RowHeadersVisible = false;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvTrxData.RowsDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvTrxData.RowTemplate.Height = 24;
            this.dgvTrxData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTrxData.Size = new System.Drawing.Size(746, 437);
            this.dgvTrxData.TabIndex = 0;
            // 
            // colGroupName
            // 
            this.colGroupName.DataPropertyName = "EVENT_GROUP_NAME";
            this.colGroupName.HeaderText = "Event Group Name";
            this.colGroupName.Name = "colGroupName";
            this.colGroupName.ReadOnly = true;
            this.colGroupName.Width = 175;
            // 
            // colDir
            // 
            this.colDir.DataPropertyName = "DIR";
            this.colDir.HeaderText = "Dir";
            this.colDir.Name = "colDir";
            this.colDir.ReadOnly = true;
            this.colDir.Width = 50;
            // 
            // colEventName
            // 
            this.colEventName.DataPropertyName = "EVENT_NAME";
            this.colEventName.HeaderText = "Event Name";
            this.colEventName.Name = "colEventName";
            this.colEventName.ReadOnly = true;
            this.colEventName.Width = 140;
            // 
            // colDevcode
            // 
            this.colDevcode.DataPropertyName = "DEVCODE";
            this.colDevcode.HeaderText = "Devcode";
            this.colDevcode.Name = "colDevcode";
            this.colDevcode.ReadOnly = true;
            // 
            // colAddress
            // 
            this.colAddress.DataPropertyName = "ADDR";
            this.colAddress.HeaderText = "Address";
            this.colAddress.Name = "colAddress";
            this.colAddress.ReadOnly = true;
            this.colAddress.Width = 80;
            // 
            // colRAddress
            // 
            this.colRAddress.DataPropertyName = "REALADDR";
            this.colRAddress.HeaderText = "Real Address";
            this.colRAddress.Name = "colRAddress";
            this.colRAddress.ReadOnly = true;
            this.colRAddress.Width = 140;
            // 
            // colPoints
            // 
            this.colPoints.DataPropertyName = "POINTS";
            this.colPoints.HeaderText = "Points";
            this.colPoints.Name = "colPoints";
            this.colPoints.ReadOnly = true;
            this.colPoints.Width = 80;
            // 
            // colSkipDecode
            // 
            this.colSkipDecode.DataPropertyName = "SKIPDECODE";
            this.colSkipDecode.HeaderText = "Skip Decode";
            this.colSkipDecode.Name = "colSkipDecode";
            this.colSkipDecode.ReadOnly = true;
            this.colSkipDecode.Width = 130;
            // 
            // colItemName
            // 
            this.colItemName.DataPropertyName = "ITEM_NAME";
            this.colItemName.HeaderText = "Item Name";
            this.colItemName.MinimumWidth = 200;
            this.colItemName.Name = "colItemName";
            this.colItemName.ReadOnly = true;
            this.colItemName.Width = 200;
            // 
            // colItemValue
            // 
            this.colItemValue.DataPropertyName = "VAL";
            this.colItemValue.HeaderText = "Value";
            this.colItemValue.Name = "colItemValue";
            this.colItemValue.ReadOnly = true;
            this.colItemValue.Width = 200;
            // 
            // colWOffset
            // 
            this.colWOffset.DataPropertyName = "WOFFSET";
            this.colWOffset.HeaderText = "W Offset";
            this.colWOffset.Name = "colWOffset";
            this.colWOffset.ReadOnly = true;
            // 
            // colWPoints
            // 
            this.colWPoints.DataPropertyName = "WPOINTS";
            this.colWPoints.HeaderText = "W Points";
            this.colWPoints.Name = "colWPoints";
            this.colWPoints.ReadOnly = true;
            // 
            // colBOffset
            // 
            this.colBOffset.DataPropertyName = "BOFFSET";
            this.colBOffset.HeaderText = "B Offset";
            this.colBOffset.Name = "colBOffset";
            this.colBOffset.ReadOnly = true;
            this.colBOffset.Width = 90;
            // 
            // colBPoints
            // 
            this.colBPoints.DataPropertyName = "BPOINTS";
            this.colBPoints.HeaderText = "B Points";
            this.colBPoints.Name = "colBPoints";
            this.colBPoints.ReadOnly = true;
            this.colBPoints.Width = 90;
            // 
            // colExpression
            // 
            this.colExpression.DataPropertyName = "EXPRESSION";
            this.colExpression.HeaderText = "Expression";
            this.colExpression.Name = "colExpression";
            this.colExpression.ReadOnly = true;
            // 
            // FormMonitorPLC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1130, 545);
            this.Name = "FormMonitorPLC";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormMonitorPLC_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.spcPLC.Panel1.ResumeLayout(false);
            this.spcPLC.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcPLC)).EndInit();
            this.spcPLC.ResumeLayout(false);
            this.pnlCombox.ResumeLayout(false);
            this.pnlCombox.PerformLayout();
            this.gbxContent.ResumeLayout(false);
            this.spcTrx.Panel1.ResumeLayout(false);
            this.spcTrx.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcTrx)).EndInit();
            this.spcTrx.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTrxName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTrxData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spcPLC;
        private System.Windows.Forms.Panel pnlCombox;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.GroupBox gbxContent;
        private System.Windows.Forms.SplitContainer spcTrx;
        private System.Windows.Forms.DataGridView dgvTrxData;
        private System.Windows.Forms.ComboBox cboLocalNode;
        private System.Windows.Forms.Label lblLocalNode;
        private System.Windows.Forms.DataGridView dgvTrxName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPLCTrxName;
        private System.Windows.Forms.CheckBox chkDetail;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGroupName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDir;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEventName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDevcode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAddress;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRAddress;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSkipDecode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExpression;
    }
}