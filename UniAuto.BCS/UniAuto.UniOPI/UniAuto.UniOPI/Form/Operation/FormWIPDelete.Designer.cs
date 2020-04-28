namespace UniOPI
{
    partial class FormWIPDelete
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnQuery = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnRecovery = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dgvJobData = new System.Windows.Forms.DataGridView();
            this.gbGlass = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.flpCondition = new System.Windows.Forms.FlowLayoutPanel();
            this.colChoose = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDetail = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colChangeRoute = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colStopReason = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colCreateRobotWip = new System.Windows.Forms.DataGridViewButtonColumn();
            this.LocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSlotNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCstSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGlassID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProductType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobJudge = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobGrade = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrackingData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobData)).BeginInit();
            this.gbGlass.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
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
            this.spcBase.Panel2.Controls.Add(this.tableLayoutPanel4);
            this.spcBase.Size = new System.Drawing.Size(1260, 522);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnQuery);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(1165, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(92, 55);
            this.panel2.TabIndex = 1;
            // 
            // btnQuery
            // 
            this.btnQuery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuery.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.Location = new System.Drawing.Point(0, 0);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(92, 46);
            this.btnQuery.TabIndex = 21;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // panel4
            // 
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(92, 29);
            this.panel4.TabIndex = 24;
            // 
            // btnRemove
            // 
            this.btnRemove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRemove.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnRemove.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemove.Location = new System.Drawing.Point(0, 29);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(92, 46);
            this.btnRemove.TabIndex = 25;
            this.btnRemove.Text = "Delete";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnRecovery
            // 
            this.btnRecovery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRecovery.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnRecovery.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRecovery.Location = new System.Drawing.Point(0, 75);
            this.btnRecovery.Name = "btnRecovery";
            this.btnRecovery.Size = new System.Drawing.Size(92, 46);
            this.btnRecovery.TabIndex = 26;
            this.btnRecovery.Text = "Recovery";
            this.btnRecovery.UseVisualStyleBackColor = true;
            this.btnRecovery.Click += new System.EventHandler(this.btnRecovery_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnRecovery);
            this.panel3.Controls.Add(this.btnRemove);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(1165, 64);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(92, 424);
            this.panel3.TabIndex = 14;
            // 
            // dgvJobData
            // 
            this.dgvJobData.AllowUserToAddRows = false;
            this.dgvJobData.AllowUserToDeleteRows = false;
            this.dgvJobData.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvJobData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvJobData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvJobData.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvJobData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvJobData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvJobData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colChoose,
            this.colDetail,
            this.colChangeRoute,
            this.colStopReason,
            this.colCreateRobotWip,
            this.LocalNo,
            this.colSlotNo,
            this.colCstSeqNo,
            this.colJobSeqNo,
            this.colGlassID,
            this.colProductType,
            this.colJobType,
            this.colJobJudge,
            this.colJobGrade,
            this.colPPID,
            this.colTrackingData});
            this.dgvJobData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvJobData.Location = new System.Drawing.Point(3, 23);
            this.dgvJobData.Name = "dgvJobData";
            this.dgvJobData.ReadOnly = true;
            this.dgvJobData.RowHeadersVisible = false;
            this.dgvJobData.RowTemplate.Height = 24;
            this.dgvJobData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvJobData.Size = new System.Drawing.Size(1150, 398);
            this.dgvJobData.TabIndex = 8;
            this.dgvJobData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvJobData_CellClick);
            // 
            // gbGlass
            // 
            this.gbGlass.Controls.Add(this.dgvJobData);
            this.gbGlass.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbGlass.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbGlass.Location = new System.Drawing.Point(3, 64);
            this.gbGlass.Name = "gbGlass";
            this.gbGlass.Size = new System.Drawing.Size(1156, 424);
            this.gbGlass.TabIndex = 13;
            this.gbGlass.TabStop = false;
            this.gbGlass.Text = "Total Count : 0";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 98F));
            this.tableLayoutPanel4.Controls.Add(this.panel3, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.gbGlass, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.flpCondition, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.62729F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 87.37271F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(1260, 491);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // flpCondition
            // 
            this.flpCondition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpCondition.Location = new System.Drawing.Point(3, 3);
            this.flpCondition.Name = "flpCondition";
            this.flpCondition.Size = new System.Drawing.Size(1156, 55);
            this.flpCondition.TabIndex = 1;
            // 
            // colChoose
            // 
            this.colChoose.HeaderText = "C";
            this.colChoose.Name = "colChoose";
            this.colChoose.ReadOnly = true;
            this.colChoose.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colChoose.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colChoose.Width = 43;
            // 
            // colDetail
            // 
            this.colDetail.HeaderText = "Detail";
            this.colDetail.Name = "colDetail";
            this.colDetail.ReadOnly = true;
            this.colDetail.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colDetail.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colDetail.Width = 73;
            // 
            // colChangeRoute
            // 
            this.colChangeRoute.HeaderText = "Route";
            this.colChangeRoute.Name = "colChangeRoute";
            this.colChangeRoute.ReadOnly = true;
            this.colChangeRoute.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colChangeRoute.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colChangeRoute.Width = 72;
            // 
            // colStopReason
            // 
            this.colStopReason.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colStopReason.HeaderText = "Stop Reason";
            this.colStopReason.Name = "colStopReason";
            this.colStopReason.ReadOnly = true;
            this.colStopReason.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colStopReason.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colStopReason.Width = 115;
            // 
            // colCreateRobotWip
            // 
            this.colCreateRobotWip.HeaderText = "Create Robot Wip";
            this.colCreateRobotWip.Name = "colCreateRobotWip";
            this.colCreateRobotWip.ReadOnly = true;
            this.colCreateRobotWip.Width = 94;
            // 
            // LocalNo
            // 
            this.LocalNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.LocalNo.HeaderText = "Local";
            this.LocalNo.Name = "LocalNo";
            this.LocalNo.ReadOnly = true;
            // 
            // colSlotNo
            // 
            this.colSlotNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colSlotNo.HeaderText = "Slot No";
            this.colSlotNo.Name = "colSlotNo";
            this.colSlotNo.ReadOnly = true;
            this.colSlotNo.Width = 85;
            // 
            // colCstSeqNo
            // 
            this.colCstSeqNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colCstSeqNo.HeaderText = "CST Seq No";
            this.colCstSeqNo.MinimumWidth = 90;
            this.colCstSeqNo.Name = "colCstSeqNo";
            this.colCstSeqNo.ReadOnly = true;
            this.colCstSeqNo.Width = 115;
            // 
            // colJobSeqNo
            // 
            this.colJobSeqNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colJobSeqNo.HeaderText = "Job Seq No";
            this.colJobSeqNo.MinimumWidth = 90;
            this.colJobSeqNo.Name = "colJobSeqNo";
            this.colJobSeqNo.ReadOnly = true;
            this.colJobSeqNo.Width = 110;
            // 
            // colGlassID
            // 
            this.colGlassID.HeaderText = "Glass ID";
            this.colGlassID.Name = "colGlassID";
            this.colGlassID.ReadOnly = true;
            this.colGlassID.Width = 81;
            // 
            // colProductType
            // 
            this.colProductType.HeaderText = "Product Type";
            this.colProductType.Name = "colProductType";
            this.colProductType.ReadOnly = true;
            this.colProductType.Width = 107;
            // 
            // colJobType
            // 
            this.colJobType.HeaderText = "Job Type";
            this.colJobType.Name = "colJobType";
            this.colJobType.ReadOnly = true;
            this.colJobType.Width = 82;
            // 
            // colJobJudge
            // 
            this.colJobJudge.HeaderText = "Job Judge";
            this.colJobJudge.Name = "colJobJudge";
            this.colJobJudge.ReadOnly = true;
            this.colJobJudge.Width = 88;
            // 
            // colJobGrade
            // 
            this.colJobGrade.HeaderText = "Job Grade";
            this.colJobGrade.Name = "colJobGrade";
            this.colJobGrade.ReadOnly = true;
            this.colJobGrade.Width = 90;
            // 
            // colPPID
            // 
            this.colPPID.HeaderText = "PPID";
            this.colPPID.Name = "colPPID";
            this.colPPID.ReadOnly = true;
            this.colPPID.Width = 64;
            // 
            // colTrackingData
            // 
            this.colTrackingData.HeaderText = "Tracking Data";
            this.colTrackingData.Name = "colTrackingData";
            this.colTrackingData.ReadOnly = true;
            this.colTrackingData.Width = 113;
            // 
            // FormWIPDelete
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1260, 522);
            this.Name = "FormWIPDelete";
            this.Text = "FormWIPDelete";
            this.Load += new System.EventHandler(this.FormWIPDelete_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobData)).EndInit();
            this.gbGlass.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnRecovery;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.GroupBox gbGlass;
        private System.Windows.Forms.DataGridView dgvJobData;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.FlowLayoutPanel flpCondition;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colChoose;
        private System.Windows.Forms.DataGridViewButtonColumn colDetail;
        private System.Windows.Forms.DataGridViewButtonColumn colChangeRoute;
        private System.Windows.Forms.DataGridViewButtonColumn colStopReason;
        private System.Windows.Forms.DataGridViewButtonColumn colCreateRobotWip;
        private System.Windows.Forms.DataGridViewTextBoxColumn LocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCstSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGlassID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobJudge;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobGrade;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPPID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrackingData;

    }
}