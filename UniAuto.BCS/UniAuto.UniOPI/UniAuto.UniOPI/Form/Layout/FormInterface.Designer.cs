namespace UniOPI
{
    partial class FormInterface
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
            this.btnTimingChart = new System.Windows.Forms.Button();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.pblJogData = new System.Windows.Forms.Panel();
            this.dgvJob = new System.Windows.Forms.DataGridView();
            this.colDetail = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colStream = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCassetteSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGlassID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProductType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSubStrateType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobJudge = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobGrade = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrackingData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEQPFlag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.flpUp = new System.Windows.Forms.FlowLayoutPanel();
            this.flpDown = new System.Windows.Forms.FlowLayoutPanel();
            this.tmrRefreshPLC = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.pnlTopBack2.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.pblJogData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJob)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(430, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(490, 722);
            // 
            // pnlTopBack2
            // 
            this.pnlTopBack2.Controls.Add(this.btnTimingChart);
            // 
            // btnTimingChart
            // 
            this.btnTimingChart.BackgroundImage = global::UniOPI.Properties.Resources.BtnTimingChart;
            this.btnTimingChart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnTimingChart.FlatAppearance.BorderSize = 0;
            this.btnTimingChart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTimingChart.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTimingChart.ForeColor = System.Drawing.Color.Yellow;
            this.btnTimingChart.Location = new System.Drawing.Point(3, 3);
            this.btnTimingChart.Name = "btnTimingChart";
            this.btnTimingChart.Size = new System.Drawing.Size(25, 25);
            this.btnTimingChart.TabIndex = 21;
            this.btnTimingChart.UseVisualStyleBackColor = true;
            this.btnTimingChart.Click += new System.EventHandler(this.btnTimingChart_Click);
            // 
            // tlpBase
            // 
            this.tlpBase.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.InsetDouble;
            this.tlpBase.ColumnCount = 2;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBase.Controls.Add(this.pblJogData, 0, 1);
            this.tlpBase.Controls.Add(this.flpUp, 0, 0);
            this.tlpBase.Controls.Add(this.flpDown, 1, 0);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Size = new System.Drawing.Size(490, 691);
            this.tlpBase.TabIndex = 2;
            // 
            // pblJogData
            // 
            this.pblJogData.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tlpBase.SetColumnSpan(this.pblJogData, 2);
            this.pblJogData.Controls.Add(this.dgvJob);
            this.pblJogData.Location = new System.Drawing.Point(6, 579);
            this.pblJogData.Name = "pblJogData";
            this.pblJogData.Size = new System.Drawing.Size(478, 106);
            this.pblJogData.TabIndex = 2;
            // 
            // dgvJob
            // 
            this.dgvJob.AllowUserToAddRows = false;
            this.dgvJob.AllowUserToDeleteRows = false;
            this.dgvJob.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvJob.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvJob.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvJob.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvJob.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvJob.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvJob.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDetail,
            this.colStream,
            this.colAddress,
            this.colCassetteSeqNo,
            this.colJobSeqNo,
            this.colGlassID,
            this.colProductType,
            this.colSubStrateType,
            this.colJobType,
            this.colJobJudge,
            this.colJobGrade,
            this.colPPID,
            this.colTrackingData,
            this.colEQPFlag});
            this.dgvJob.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvJob.Location = new System.Drawing.Point(0, 0);
            this.dgvJob.MultiSelect = false;
            this.dgvJob.Name = "dgvJob";
            this.dgvJob.ReadOnly = true;
            this.dgvJob.RowHeadersVisible = false;
            this.dgvJob.RowTemplate.Height = 24;
            this.dgvJob.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvJob.Size = new System.Drawing.Size(474, 102);
            this.dgvJob.TabIndex = 8;
            this.dgvJob.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvJob_CellClick);
            // 
            // colDetail
            // 
            this.colDetail.HeaderText = "Detail";
            this.colDetail.Name = "colDetail";
            this.colDetail.ReadOnly = true;
            this.colDetail.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colDetail.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colDetail.Width = 75;
            // 
            // colStream
            // 
            this.colStream.HeaderText = "Stream";
            this.colStream.Name = "colStream";
            this.colStream.ReadOnly = true;
            this.colStream.Width = 85;
            // 
            // colAddress
            // 
            this.colAddress.HeaderText = "Address";
            this.colAddress.Name = "colAddress";
            this.colAddress.ReadOnly = true;
            this.colAddress.Width = 91;
            // 
            // colCassetteSeqNo
            // 
            this.colCassetteSeqNo.HeaderText = "Cassette Seq No";
            this.colCassetteSeqNo.Name = "colCassetteSeqNo";
            this.colCassetteSeqNo.ReadOnly = true;
            this.colCassetteSeqNo.Width = 146;
            // 
            // colJobSeqNo
            // 
            this.colJobSeqNo.HeaderText = "Job Seq No";
            this.colJobSeqNo.Name = "colJobSeqNo";
            this.colJobSeqNo.ReadOnly = true;
            this.colJobSeqNo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colJobSeqNo.Width = 92;
            // 
            // colGlassID
            // 
            this.colGlassID.HeaderText = "Glass ID";
            this.colGlassID.Name = "colGlassID";
            this.colGlassID.ReadOnly = true;
            this.colGlassID.Width = 90;
            // 
            // colProductType
            // 
            this.colProductType.HeaderText = "Product Type";
            this.colProductType.Name = "colProductType";
            this.colProductType.ReadOnly = true;
            this.colProductType.Width = 126;
            // 
            // colSubStrateType
            // 
            this.colSubStrateType.HeaderText = "Sub Strate Type";
            this.colSubStrateType.Name = "colSubStrateType";
            this.colSubStrateType.ReadOnly = true;
            this.colSubStrateType.Width = 143;
            // 
            // colJobType
            // 
            this.colJobType.HeaderText = "Job Type";
            this.colJobType.Name = "colJobType";
            this.colJobType.ReadOnly = true;
            this.colJobType.Width = 94;
            // 
            // colJobJudge
            // 
            this.colJobJudge.HeaderText = "Job Judge";
            this.colJobJudge.Name = "colJobJudge";
            this.colJobJudge.ReadOnly = true;
            this.colJobJudge.Width = 101;
            // 
            // colJobGrade
            // 
            this.colJobGrade.HeaderText = "Job Grade";
            this.colJobGrade.Name = "colJobGrade";
            this.colJobGrade.ReadOnly = true;
            this.colJobGrade.Width = 104;
            // 
            // colPPID
            // 
            this.colPPID.HeaderText = "PPID";
            this.colPPID.Name = "colPPID";
            this.colPPID.ReadOnly = true;
            this.colPPID.Width = 67;
            // 
            // colTrackingData
            // 
            this.colTrackingData.HeaderText = "Tracking Data";
            this.colTrackingData.Name = "colTrackingData";
            this.colTrackingData.ReadOnly = true;
            this.colTrackingData.Width = 128;
            // 
            // colEQPFlag
            // 
            this.colEQPFlag.HeaderText = "EQP Flag";
            this.colEQPFlag.Name = "colEQPFlag";
            this.colEQPFlag.ReadOnly = true;
            this.colEQPFlag.Width = 95;
            // 
            // flpUp
            // 
            this.flpUp.AutoScroll = true;
            this.flpUp.AutoSize = true;
            this.flpUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpUp.Location = new System.Drawing.Point(6, 6);
            this.flpUp.Name = "flpUp";
            this.flpUp.Size = new System.Drawing.Size(234, 564);
            this.flpUp.TabIndex = 3;
            // 
            // flpDown
            // 
            this.flpDown.AutoScroll = true;
            this.flpDown.AutoSize = true;
            this.flpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpDown.Location = new System.Drawing.Point(249, 6);
            this.flpDown.Name = "flpDown";
            this.flpDown.Size = new System.Drawing.Size(235, 564);
            this.flpDown.TabIndex = 4;
            // 
            // tmrRefreshPLC
            // 
            this.tmrRefreshPLC.Interval = 3000;
            this.tmrRefreshPLC.Tick += new System.EventHandler(this.tmrRefreshPLC_Tick);
            // 
            // FormInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::UniOPI.Properties.Resources.Bg_Main;
            this.ClientSize = new System.Drawing.Size(490, 722);
            this.ControlBox = true;
            this.Name = "FormInterface";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "   ";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormInterface_FormClosed);
            this.Load += new System.EventHandler(this.FormInterface_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.pnlTopBack2.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.tlpBase.PerformLayout();
            this.pblJogData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvJob)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTimingChart;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel pblJogData;
        private System.Windows.Forms.DataGridView dgvJob;
        private System.Windows.Forms.FlowLayoutPanel flpUp;
        private System.Windows.Forms.FlowLayoutPanel flpDown;
        private System.Windows.Forms.Timer tmrRefreshPLC;
        private System.Windows.Forms.DataGridViewButtonColumn colDetail;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStream;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAddress;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCassetteSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGlassID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSubStrateType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobJudge;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobGrade;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPPID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrackingData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEQPFlag;

    }
}