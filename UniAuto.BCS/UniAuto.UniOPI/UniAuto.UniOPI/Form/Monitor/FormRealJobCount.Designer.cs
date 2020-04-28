namespace UniOPI
{
    partial class FormRealJobCount
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
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.gbxGridData = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.tlpBottom = new System.Windows.Forms.TableLayoutPanel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTFTCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCFCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDummyCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colThroughCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colThicknessCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUVMaskCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnassembledTFTCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colITOCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNIPCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMetalOneCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTotalCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.gbxGridData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tlpBottom.SuspendLayout();
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
            this.spcBase.Size = new System.Drawing.Size(1140, 522);
            // 
            // tmrBaseRefresh
            // 
            this.tmrBaseRefresh.Tick += new System.EventHandler(this.tmrBaseRefresh_Tick);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBase.Controls.Add(this.gbxGridData, 0, 0);
            this.tlpBase.Controls.Add(this.tlpBottom, 0, 1);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Size = new System.Drawing.Size(1140, 491);
            this.tlpBase.TabIndex = 0;
            // 
            // gbxGridData
            // 
            this.gbxGridData.Controls.Add(this.dgvData);
            this.gbxGridData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxGridData.Location = new System.Drawing.Point(3, 3);
            this.gbxGridData.Name = "gbxGridData";
            this.gbxGridData.Size = new System.Drawing.Size(1134, 440);
            this.gbxGridData.TabIndex = 15;
            this.gbxGridData.TabStop = false;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
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
            this.colLocalNo,
            this.colLocalID,
            this.colTFTCount,
            this.colCFCount,
            this.colDummyCount,
            this.colThroughCount,
            this.colThicknessCount,
            this.colUVMaskCount,
            this.colUnassembledTFTCount,
            this.colITOCount,
            this.colNIPCount,
            this.colMetalOneCount,
            this.colTotalCount});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 18);
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1128, 419);
            this.dgvData.TabIndex = 11;
            // 
            // tlpBottom
            // 
            this.tlpBottom.ColumnCount = 3;
            this.tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBottom.Controls.Add(this.btnRefresh, 1, 0);
            this.tlpBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tlpBottom.Location = new System.Drawing.Point(3, 449);
            this.tlpBottom.Name = "tlpBottom";
            this.tlpBottom.RowCount = 1;
            this.tlpBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBottom.Size = new System.Drawing.Size(1134, 39);
            this.tlpBottom.TabIndex = 14;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(1016, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(95, 33);
            this.btnRefresh.TabIndex = 25;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // colLocalNo
            // 
            this.colLocalNo.HeaderText = "Local No";
            this.colLocalNo.Name = "colLocalNo";
            this.colLocalNo.ReadOnly = true;
            this.colLocalNo.Visible = false;
            this.colLocalNo.Width = 76;
            // 
            // colLocalID
            // 
            this.colLocalID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colLocalID.HeaderText = "Local Name";
            this.colLocalID.Name = "colLocalID";
            this.colLocalID.ReadOnly = true;
            // 
            // colTFTCount
            // 
            this.colTFTCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colTFTCount.HeaderText = "TFT";
            this.colTFTCount.Name = "colTFTCount";
            this.colTFTCount.ReadOnly = true;
            this.colTFTCount.Width = 50;
            // 
            // colCFCount
            // 
            this.colCFCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colCFCount.HeaderText = "CF";
            this.colCFCount.Name = "colCFCount";
            this.colCFCount.ReadOnly = true;
            this.colCFCount.Width = 50;
            // 
            // colDummyCount
            // 
            this.colDummyCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDummyCount.HeaderText = "Dummy";
            this.colDummyCount.Name = "colDummyCount";
            this.colDummyCount.ReadOnly = true;
            this.colDummyCount.Width = 60;
            // 
            // colThroughCount
            // 
            this.colThroughCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colThroughCount.HeaderText = "Through Dummy";
            this.colThroughCount.Name = "colThroughCount";
            this.colThroughCount.ReadOnly = true;
            this.colThroughCount.Width = 90;
            // 
            // colThicknessCount
            // 
            this.colThicknessCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colThicknessCount.HeaderText = "Thickness Dummy";
            this.colThicknessCount.Name = "colThicknessCount";
            this.colThicknessCount.ReadOnly = true;
            this.colThicknessCount.Width = 90;
            // 
            // colUVMaskCount
            // 
            this.colUVMaskCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUVMaskCount.HeaderText = "UV MASK";
            this.colUVMaskCount.Name = "colUVMaskCount";
            this.colUVMaskCount.ReadOnly = true;
            this.colUVMaskCount.Width = 70;
            // 
            // colUnassembledTFTCount
            // 
            this.colUnassembledTFTCount.HeaderText = "Unassembled TFT";
            this.colUnassembledTFTCount.Name = "colUnassembledTFTCount";
            this.colUnassembledTFTCount.ReadOnly = true;
            this.colUnassembledTFTCount.Width = 143;
            // 
            // colITOCount
            // 
            this.colITOCount.HeaderText = "ITO Dummy";
            this.colITOCount.Name = "colITOCount";
            this.colITOCount.ReadOnly = true;
            this.colITOCount.Width = 106;
            // 
            // colNIPCount
            // 
            this.colNIPCount.HeaderText = "NIP Dummy";
            this.colNIPCount.Name = "colNIPCount";
            this.colNIPCount.ReadOnly = true;
            this.colNIPCount.Width = 107;
            // 
            // colMetalOneCount
            // 
            this.colMetalOneCount.HeaderText = "Metal One Dummy";
            this.colMetalOneCount.Name = "colMetalOneCount";
            this.colMetalOneCount.ReadOnly = true;
            this.colMetalOneCount.Width = 147;
            // 
            // colTotalCount
            // 
            this.colTotalCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colTotalCount.HeaderText = "Total Count";
            this.colTotalCount.MinimumWidth = 130;
            this.colTotalCount.Name = "colTotalCount";
            this.colTotalCount.ReadOnly = true;
            this.colTotalCount.Width = 130;
            // 
            // FormRealJobCount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 522);
            this.Name = "FormRealJobCount";
            this.Text = "FormMonitorCount";
            this.Load += new System.EventHandler(this.FormRealJobCount_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.gbxGridData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tlpBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.TableLayoutPanel tlpBottom;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.GroupBox gbxGridData;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTFTCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCFCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDummyCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colThroughCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colThicknessCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUVMaskCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnassembledTFTCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colITOCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNIPCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMetalOneCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTotalCount;


    }
}