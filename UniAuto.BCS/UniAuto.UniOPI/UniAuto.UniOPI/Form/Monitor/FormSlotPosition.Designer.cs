namespace UniOPI
{
    partial class FormSlotPosition
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSlotPosition));
            this.spcPosition = new System.Windows.Forms.SplitContainer();
            this.pnlCombox = new System.Windows.Forms.Panel();
            this.lblPort = new System.Windows.Forms.Label();
            this.cboPort = new System.Windows.Forms.ComboBox();
            this.btnQuery = new System.Windows.Forms.Button();
            this.lblNode = new System.Windows.Forms.Label();
            this.cboNode = new System.Windows.Forms.ComboBox();
            this.grbPosition = new System.Windows.Forms.GroupBox();
            this.dgvData = new UniOPI.PagedGridView();
            this.colNodeNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPortNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPositionDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cloPositionName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPositionNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCSTSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecipeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrackingValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSamplingSlotFlag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcPosition)).BeginInit();
            this.spcPosition.Panel1.SuspendLayout();
            this.spcPosition.Panel2.SuspendLayout();
            this.spcPosition.SuspendLayout();
            this.pnlCombox.SuspendLayout();
            this.grbPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1200, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.spcPosition);
            this.spcBase.Size = new System.Drawing.Size(1260, 592);
            // 
            // spcPosition
            // 
            this.spcPosition.BackColor = System.Drawing.Color.Transparent;
            this.spcPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcPosition.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spcPosition.Location = new System.Drawing.Point(0, 0);
            this.spcPosition.Name = "spcPosition";
            this.spcPosition.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcPosition.Panel1
            // 
            this.spcPosition.Panel1.Controls.Add(this.pnlCombox);
            // 
            // spcPosition.Panel2
            // 
            this.spcPosition.Panel2.Controls.Add(this.grbPosition);
            this.spcPosition.Size = new System.Drawing.Size(1260, 561);
            this.spcPosition.SplitterDistance = 70;
            this.spcPosition.TabIndex = 2;
            // 
            // pnlCombox
            // 
            this.pnlCombox.Controls.Add(this.lblPort);
            this.pnlCombox.Controls.Add(this.cboPort);
            this.pnlCombox.Controls.Add(this.btnQuery);
            this.pnlCombox.Controls.Add(this.lblNode);
            this.pnlCombox.Controls.Add(this.cboNode);
            this.pnlCombox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCombox.Location = new System.Drawing.Point(0, 0);
            this.pnlCombox.Name = "pnlCombox";
            this.pnlCombox.Size = new System.Drawing.Size(1260, 70);
            this.pnlCombox.TabIndex = 26;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Font = new System.Drawing.Font("Calibri", 12F);
            this.lblPort.Location = new System.Drawing.Point(377, 20);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(35, 19);
            this.lblPort.TabIndex = 11;
            this.lblPort.Text = "Port";
            // 
            // cboPort
            // 
            this.cboPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPort.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboPort.FormattingEnabled = true;
            this.cboPort.Location = new System.Drawing.Point(416, 15);
            this.cboPort.Name = "cboPort";
            this.cboPort.Size = new System.Drawing.Size(188, 29);
            this.cboPort.TabIndex = 10;
            this.cboPort.SelectedIndexChanged += new System.EventHandler(this.cboPort_SelectedIndexChanged);
            // 
            // btnQuery
            // 
            this.btnQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.Location = new System.Drawing.Point(1138, 19);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(110, 29);
            this.btnQuery.TabIndex = 9;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // lblNode
            // 
            this.lblNode.AutoSize = true;
            this.lblNode.Font = new System.Drawing.Font("Calibri", 12F);
            this.lblNode.Location = new System.Drawing.Point(16, 20);
            this.lblNode.Name = "lblNode";
            this.lblNode.Size = new System.Drawing.Size(43, 19);
            this.lblNode.TabIndex = 5;
            this.lblNode.Text = "Local";
            // 
            // cboNode
            // 
            this.cboNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboNode.FormattingEnabled = true;
            this.cboNode.Location = new System.Drawing.Point(62, 16);
            this.cboNode.Name = "cboNode";
            this.cboNode.Size = new System.Drawing.Size(300, 29);
            this.cboNode.TabIndex = 4;
            this.cboNode.SelectedIndexChanged += new System.EventHandler(this.cboNode_SelectedIndexChanged);
            // 
            // grbPosition
            // 
            this.grbPosition.Controls.Add(this.dgvData);
            this.grbPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbPosition.Location = new System.Drawing.Point(0, 0);
            this.grbPosition.Name = "grbPosition";
            this.grbPosition.Size = new System.Drawing.Size(1260, 487);
            this.grbPosition.TabIndex = 16;
            this.grbPosition.TabStop = false;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            this.dgvData.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNodeNo,
            this.colPortNo,
            this.colPositionDesc,
            this.cloPositionName,
            this.colPositionNo,
            this.colCSTSeqNo,
            this.colJobSeqNo,
            this.colJobID,
            this.colRecipeName,
            this.colPPID,
            this.colTrackingValue,
            this.colSamplingSlotFlag});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 18);
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.PageSize = 100;
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.dgvData.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1254, 466);
            this.dgvData.TabIndex = 16;
            this.dgvData.Tables = ((System.ComponentModel.BindingList<System.Data.DataTable>)(resources.GetObject("dgvData.Tables")));
            this.dgvData.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvData_CellDoubleClick);
            // 
            // colNodeNo
            // 
            this.colNodeNo.DataPropertyName = "NodeNo";
            this.colNodeNo.HeaderText = "Local No";
            this.colNodeNo.Name = "colNodeNo";
            this.colNodeNo.ReadOnly = true;
            this.colNodeNo.Visible = false;
            // 
            // colPortNo
            // 
            this.colPortNo.DataPropertyName = "PortNo";
            this.colPortNo.HeaderText = "PortNo";
            this.colPortNo.Name = "colPortNo";
            this.colPortNo.ReadOnly = true;
            this.colPortNo.Visible = false;
            // 
            // colPositionDesc
            // 
            this.colPositionDesc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colPositionDesc.DataPropertyName = "PositionDesc";
            this.colPositionDesc.HeaderText = "Position";
            this.colPositionDesc.MinimumWidth = 100;
            this.colPositionDesc.Name = "colPositionDesc";
            this.colPositionDesc.ReadOnly = true;
            this.colPositionDesc.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // cloPositionName
            // 
            this.cloPositionName.DataPropertyName = "PositionName";
            this.cloPositionName.HeaderText = "Position Name";
            this.cloPositionName.MinimumWidth = 100;
            this.cloPositionName.Name = "cloPositionName";
            this.cloPositionName.ReadOnly = true;
            this.cloPositionName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.cloPositionName.Visible = false;
            // 
            // colPositionNo
            // 
            this.colPositionNo.DataPropertyName = "PositionNo";
            this.colPositionNo.HeaderText = "Position No";
            this.colPositionNo.Name = "colPositionNo";
            this.colPositionNo.ReadOnly = true;
            this.colPositionNo.Visible = false;
            this.colPositionNo.Width = 60;
            // 
            // colCSTSeqNo
            // 
            this.colCSTSeqNo.DataPropertyName = "CassetteSeqNo";
            this.colCSTSeqNo.HeaderText = "CST SeqNo";
            this.colCSTSeqNo.Name = "colCSTSeqNo";
            this.colCSTSeqNo.ReadOnly = true;
            this.colCSTSeqNo.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colCSTSeqNo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colCSTSeqNo.Width = 150;
            // 
            // colJobSeqNo
            // 
            this.colJobSeqNo.DataPropertyName = "JobSeqNo";
            this.colJobSeqNo.HeaderText = "Job SeqNo";
            this.colJobSeqNo.Name = "colJobSeqNo";
            this.colJobSeqNo.ReadOnly = true;
            this.colJobSeqNo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colJobSeqNo.Width = 150;
            // 
            // colJobID
            // 
            this.colJobID.DataPropertyName = "JobID";
            this.colJobID.HeaderText = "Job ID";
            this.colJobID.Name = "colJobID";
            this.colJobID.ReadOnly = true;
            this.colJobID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colJobID.Width = 200;
            // 
            // colRecipeName
            // 
            this.colRecipeName.DataPropertyName = "RecipeName";
            this.colRecipeName.HeaderText = "Recipe Name";
            this.colRecipeName.Name = "colRecipeName";
            this.colRecipeName.ReadOnly = true;
            this.colRecipeName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colRecipeName.Width = 150;
            // 
            // colPPID
            // 
            this.colPPID.DataPropertyName = "PPID";
            this.colPPID.HeaderText = "PPID";
            this.colPPID.Name = "colPPID";
            this.colPPID.ReadOnly = true;
            this.colPPID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colPPID.Width = 250;
            // 
            // colTrackingValue
            // 
            this.colTrackingValue.DataPropertyName = "TrackingValue";
            this.colTrackingValue.HeaderText = "Tracking Value";
            this.colTrackingValue.Name = "colTrackingValue";
            this.colTrackingValue.ReadOnly = true;
            this.colTrackingValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colTrackingValue.Visible = false;
            // 
            // colSamplingSlotFlag
            // 
            this.colSamplingSlotFlag.DataPropertyName = "SamplingSlotFlag";
            this.colSamplingSlotFlag.HeaderText = "Sampling Slot Flag";
            this.colSamplingSlotFlag.Name = "colSamplingSlotFlag";
            this.colSamplingSlotFlag.ReadOnly = true;
            this.colSamplingSlotFlag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colSamplingSlotFlag.Visible = false;
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 1000;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // FormSlotPosition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1260, 592);
            this.Name = "FormSlotPosition";
            this.Text = "FormSlotPosition";
            this.Load += new System.EventHandler(this.FormSlotPosition_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.spcPosition.Panel1.ResumeLayout(false);
            this.spcPosition.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcPosition)).EndInit();
            this.spcPosition.ResumeLayout(false);
            this.pnlCombox.ResumeLayout(false);
            this.pnlCombox.PerformLayout();
            this.grbPosition.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spcPosition;
        private System.Windows.Forms.Panel pnlCombox;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.ComboBox cboPort;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Label lblNode;
        private System.Windows.Forms.ComboBox cboNode;
        private System.Windows.Forms.GroupBox grbPosition;
        private PagedGridView dgvData;
        public System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNodeNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPortNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPositionDesc;
        private System.Windows.Forms.DataGridViewTextBoxColumn cloPositionName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPositionNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCSTSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecipeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPPID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrackingValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSamplingSlotFlag;
    }
}