namespace UniOPI
{
    partial class FormEachPosition
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEachPosition));
            this.spcPosition = new System.Windows.Forms.SplitContainer();
            this.pnlCombox = new System.Windows.Forms.Panel();
            this.lblUnit = new System.Windows.Forms.Label();
            this.cmbUnit = new System.Windows.Forms.ComboBox();
            this.btnQuery = new System.Windows.Forms.Button();
            this.lblNode = new System.Windows.Forms.Label();
            this.cmbNode = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgvData = new UniOPI.PagedGridView();
            this.colNodeNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnitNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPositionTrxNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPositionNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPositionName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCassetteSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcPosition)).BeginInit();
            this.spcPosition.Panel1.SuspendLayout();
            this.spcPosition.Panel2.SuspendLayout();
            this.spcPosition.SuspendLayout();
            this.pnlCombox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.spcPosition);
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
            this.spcPosition.Panel2.Controls.Add(this.groupBox1);
            this.spcPosition.Size = new System.Drawing.Size(1264, 517);
            this.spcPosition.SplitterDistance = 70;
            this.spcPosition.TabIndex = 1;
            // 
            // pnlCombox
            // 
            this.pnlCombox.Controls.Add(this.lblUnit);
            this.pnlCombox.Controls.Add(this.cmbUnit);
            this.pnlCombox.Controls.Add(this.btnQuery);
            this.pnlCombox.Controls.Add(this.lblNode);
            this.pnlCombox.Controls.Add(this.cmbNode);
            this.pnlCombox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCombox.Location = new System.Drawing.Point(0, 0);
            this.pnlCombox.Name = "pnlCombox";
            this.pnlCombox.Size = new System.Drawing.Size(1264, 70);
            this.pnlCombox.TabIndex = 26;
            // 
            // lblUnit
            // 
            this.lblUnit.AutoSize = true;
            this.lblUnit.Font = new System.Drawing.Font("Calibri", 12F);
            this.lblUnit.Location = new System.Drawing.Point(326, 20);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(36, 19);
            this.lblUnit.TabIndex = 11;
            this.lblUnit.Text = "Unit";
            // 
            // cmbUnit
            // 
            this.cmbUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUnit.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbUnit.FormattingEnabled = true;
            this.cmbUnit.Location = new System.Drawing.Point(365, 15);
            this.cmbUnit.Name = "cmbUnit";
            this.cmbUnit.Size = new System.Drawing.Size(188, 29);
            this.cmbUnit.TabIndex = 10;
            this.cmbUnit.SelectedIndexChanged += new System.EventHandler(this.cmbUnit_SelectedIndexChanged);
            // 
            // btnQuery
            // 
            this.btnQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.Location = new System.Drawing.Point(1142, 19);
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
            // cmbNode
            // 
            this.cmbNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbNode.FormattingEnabled = true;
            this.cmbNode.Location = new System.Drawing.Point(62, 16);
            this.cmbNode.Name = "cmbNode";
            this.cmbNode.Size = new System.Drawing.Size(240, 29);
            this.cmbNode.TabIndex = 4;
            this.cmbNode.SelectedIndexChanged += new System.EventHandler(this.cmbNode_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dgvData);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1264, 443);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
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
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNodeNo,
            this.colUnitNo,
            this.colPositionTrxNo,
            this.colPositionNo,
            this.colPositionName,
            this.colCassetteSeqNo,
            this.colJobSeqNo,
            this.colJobID});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 18);
            this.dgvData.Name = "dgvData";
            this.dgvData.PageSize = 100;
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1258, 422);
            this.dgvData.TabIndex = 17;
            this.dgvData.Tables = ((System.ComponentModel.BindingList<System.Data.DataTable>)(resources.GetObject("dgvData.Tables")));
            this.dgvData.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvData_CellDoubleClick);
            // 
            // colNodeNo
            // 
            this.colNodeNo.DataPropertyName = "NodeNo";
            this.colNodeNo.HeaderText = "NodeNo";
            this.colNodeNo.Name = "colNodeNo";
            this.colNodeNo.ReadOnly = true;
            this.colNodeNo.Visible = false;
            // 
            // colUnitNo
            // 
            this.colUnitNo.DataPropertyName = "UnitNo";
            this.colUnitNo.HeaderText = "UnitNo";
            this.colUnitNo.Name = "colUnitNo";
            this.colUnitNo.ReadOnly = true;
            this.colUnitNo.Visible = false;
            // 
            // colPositionTrxNo
            // 
            this.colPositionTrxNo.DataPropertyName = "PositionTrxNo";
            this.colPositionTrxNo.HeaderText = "PositionTrxNo";
            this.colPositionTrxNo.Name = "colPositionTrxNo";
            this.colPositionTrxNo.ReadOnly = true;
            this.colPositionTrxNo.Visible = false;
            this.colPositionTrxNo.Width = 150;
            // 
            // colPositionNo
            // 
            this.colPositionNo.DataPropertyName = "PositionNo";
            this.colPositionNo.HeaderText = "Position No";
            this.colPositionNo.Name = "colPositionNo";
            this.colPositionNo.ReadOnly = true;
            this.colPositionNo.Width = 200;
            // 
            // colPositionName
            // 
            this.colPositionName.DataPropertyName = "PositionName";
            this.colPositionName.HeaderText = "Position Name";
            this.colPositionName.Name = "colPositionName";
            this.colPositionName.ReadOnly = true;
            this.colPositionName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colPositionName.Width = 200;
            // 
            // colCassetteSeqNo
            // 
            this.colCassetteSeqNo.DataPropertyName = "CassetteSeqNo";
            this.colCassetteSeqNo.HeaderText = "Cassette Seq No";
            this.colCassetteSeqNo.Name = "colCassetteSeqNo";
            this.colCassetteSeqNo.ReadOnly = true;
            this.colCassetteSeqNo.Width = 200;
            // 
            // colJobSeqNo
            // 
            this.colJobSeqNo.DataPropertyName = "JobSeqNo";
            this.colJobSeqNo.HeaderText = "Job Seq No";
            this.colJobSeqNo.Name = "colJobSeqNo";
            this.colJobSeqNo.ReadOnly = true;
            this.colJobSeqNo.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colJobSeqNo.Width = 200;
            // 
            // colJobID
            // 
            this.colJobID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colJobID.DataPropertyName = "JobID";
            this.colJobID.HeaderText = "Job ID";
            this.colJobID.Name = "colJobID";
            this.colJobID.ReadOnly = true;
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 1000;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // FormEachPosition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 548);
            this.Name = "FormEachPosition";
            this.Text = "FormEachPosition";
            this.Load += new System.EventHandler(this.FormEachPosition_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.spcPosition.Panel1.ResumeLayout(false);
            this.spcPosition.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcPosition)).EndInit();
            this.spcPosition.ResumeLayout(false);
            this.pnlCombox.ResumeLayout(false);
            this.pnlCombox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spcPosition;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel pnlCombox;
        private System.Windows.Forms.Label lblUnit;
        private System.Windows.Forms.ComboBox cmbUnit;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Label lblNode;
        private System.Windows.Forms.ComboBox cmbNode;
        private PagedGridView dgvData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNodeNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPositionTrxNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPositionNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPositionName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCassetteSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobID;
        public System.Windows.Forms.Timer tmrRefresh;
    }
}