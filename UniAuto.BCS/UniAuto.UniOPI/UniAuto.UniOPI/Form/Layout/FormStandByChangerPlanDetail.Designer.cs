namespace UniOPI
{
    partial class FormStandByChangerPlanDetail
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
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.gbChangePlan = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.cloSlotNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGlassID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSourceCSTID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTargetCSTID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHaveBeenUse = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblStandByPlanStatus = new System.Windows.Forms.Label();
            this.txtPlanStatus = new System.Windows.Forms.TextBox();
            this.lblStandByPlanID = new System.Windows.Forms.Label();
            this.txtPlanID = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.gbChangePlan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(912, 30);
            this.lblCaption.Text = "Stand By Changer Plan Detail";
            this.lblCaption.DoubleClick += new System.EventHandler(this.lblCaption_DoubleClick);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(972, 659);
            // 
            // tmrBaseRefresh
            // 
            this.tmrBaseRefresh.Tick += new System.EventHandler(this.tmrBaseRefresh_Tick);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.gbChangePlan, 0, 1);
            this.tlpBase.Controls.Add(this.pnlHeader, 0, 0);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 83F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Size = new System.Drawing.Size(972, 628);
            this.tlpBase.TabIndex = 0;
            // 
            // gbChangePlan
            // 
            this.gbChangePlan.Controls.Add(this.dgvData);
            this.gbChangePlan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbChangePlan.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbChangePlan.Location = new System.Drawing.Point(3, 86);
            this.gbChangePlan.Name = "gbChangePlan";
            this.gbChangePlan.Size = new System.Drawing.Size(966, 539);
            this.gbChangePlan.TabIndex = 24;
            this.gbChangePlan.TabStop = false;
            this.gbChangePlan.Text = "Changer Plan List";
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
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cloSlotNo,
            this.colGlassID,
            this.colSourceCSTID,
            this.colTargetCSTID,
            this.colHaveBeenUse});
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 23);
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(960, 513);
            this.dgvData.TabIndex = 12;
            // 
            // cloSlotNo
            // 
            this.cloSlotNo.DataPropertyName = "SlotNo";
            this.cloSlotNo.HeaderText = "Slot No";
            this.cloSlotNo.Name = "cloSlotNo";
            this.cloSlotNo.ReadOnly = true;
            // 
            // colGlassID
            // 
            this.colGlassID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colGlassID.DataPropertyName = "ProductName";
            this.colGlassID.HeaderText = "Glass ID";
            this.colGlassID.Name = "colGlassID";
            this.colGlassID.ReadOnly = true;
            // 
            // colSourceCSTID
            // 
            this.colSourceCSTID.DataPropertyName = "SourceCSTID";
            this.colSourceCSTID.HeaderText = "Source CST ID";
            this.colSourceCSTID.Name = "colSourceCSTID";
            this.colSourceCSTID.ReadOnly = true;
            this.colSourceCSTID.Width = 200;
            // 
            // colTargetCSTID
            // 
            this.colTargetCSTID.DataPropertyName = "TargetCSTID";
            this.colTargetCSTID.HeaderText = "Target CST ID";
            this.colTargetCSTID.Name = "colTargetCSTID";
            this.colTargetCSTID.ReadOnly = true;
            this.colTargetCSTID.Width = 200;
            // 
            // colHaveBeenUse
            // 
            this.colHaveBeenUse.DataPropertyName = "HaveBeenUse";
            this.colHaveBeenUse.HeaderText = "Have Been Use";
            this.colHaveBeenUse.Name = "colHaveBeenUse";
            this.colHaveBeenUse.ReadOnly = true;
            this.colHaveBeenUse.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colHaveBeenUse.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colHaveBeenUse.Width = 150;
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.btnClose);
            this.pnlHeader.Controls.Add(this.btnRefresh);
            this.pnlHeader.Controls.Add(this.lblStandByPlanStatus);
            this.pnlHeader.Controls.Add(this.txtPlanStatus);
            this.pnlHeader.Controls.Add(this.lblStandByPlanID);
            this.pnlHeader.Controls.Add(this.txtPlanID);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Location = new System.Drawing.Point(3, 3);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(966, 77);
            this.pnlHeader.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(867, 29);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 40);
            this.btnClose.TabIndex = 16;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(771, 29);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(90, 40);
            this.btnRefresh.TabIndex = 15;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // lblStandByPlanStatus
            // 
            this.lblStandByPlanStatus.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.lblStandByPlanStatus.ForeColor = System.Drawing.Color.Black;
            this.lblStandByPlanStatus.Location = new System.Drawing.Point(13, 42);
            this.lblStandByPlanStatus.Name = "lblStandByPlanStatus";
            this.lblStandByPlanStatus.Size = new System.Drawing.Size(147, 30);
            this.lblStandByPlanStatus.TabIndex = 7;
            this.lblStandByPlanStatus.Text = "Stand By Plan Status";
            this.lblStandByPlanStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPlanStatus
            // 
            this.txtPlanStatus.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPlanStatus.Location = new System.Drawing.Point(173, 46);
            this.txtPlanStatus.Name = "txtPlanStatus";
            this.txtPlanStatus.ReadOnly = true;
            this.txtPlanStatus.Size = new System.Drawing.Size(140, 26);
            this.txtPlanStatus.TabIndex = 8;
            this.txtPlanStatus.Tag = "";
            // 
            // lblStandByPlanID
            // 
            this.lblStandByPlanID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.lblStandByPlanID.ForeColor = System.Drawing.Color.Black;
            this.lblStandByPlanID.Location = new System.Drawing.Point(13, 10);
            this.lblStandByPlanID.Name = "lblStandByPlanID";
            this.lblStandByPlanID.Size = new System.Drawing.Size(135, 30);
            this.lblStandByPlanID.TabIndex = 5;
            this.lblStandByPlanID.Text = "Stand By Plan ID";
            this.lblStandByPlanID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPlanID
            // 
            this.txtPlanID.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPlanID.Location = new System.Drawing.Point(173, 14);
            this.txtPlanID.Name = "txtPlanID";
            this.txtPlanID.ReadOnly = true;
            this.txtPlanID.Size = new System.Drawing.Size(140, 26);
            this.txtPlanID.TabIndex = 6;
            this.txtPlanID.Tag = "";
            // 
            // FormStandByChangerPlanDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(972, 659);
            this.Name = "FormStandByChangerPlanDetail";
            this.Text = " ";
            this.Load += new System.EventHandler(this.FormStandByChangerPlanDetail_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.gbChangePlan.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblStandByPlanStatus;
        private System.Windows.Forms.TextBox txtPlanStatus;
        private System.Windows.Forms.Label lblStandByPlanID;
        private System.Windows.Forms.TextBox txtPlanID;
        private System.Windows.Forms.GroupBox gbChangePlan;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.DataGridViewTextBoxColumn cloSlotNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGlassID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSourceCSTID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTargetCSTID;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colHaveBeenUse;
    }
}