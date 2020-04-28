namespace UniOPI
{
    partial class FormRobotManagement
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
            this.tlpRobot = new System.Windows.Forms.TableLayoutPanel();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colNodeNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNodeID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnitNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnitID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPortfetchSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSlotfetchSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPortStoreSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSlotStoreSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotArmQTY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colArmJobQty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemark = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grbSetting = new System.Windows.Forms.GroupBox();
            this.lblRobotName = new System.Windows.Forms.Label();
            this.lblRobotNameCaption = new System.Windows.Forms.Label();
            this.cboPortFetchSEQ = new System.Windows.Forms.ComboBox();
            this.cboSlotStoreSEQ = new System.Windows.Forms.ComboBox();
            this.lblPortFetchSeq = new System.Windows.Forms.Label();
            this.lblSlotStoreSeq = new System.Windows.Forms.Label();
            this.btnSetting = new System.Windows.Forms.Button();
            this.cboPortStoreSEQ = new System.Windows.Forms.ComboBox();
            this.cboSlotFetchSEQ = new System.Windows.Forms.ComboBox();
            this.lblPortStoreSeq = new System.Windows.Forms.Label();
            this.lblSlotFetchSeq = new System.Windows.Forms.Label();
            this.tlpRefresh = new System.Windows.Forms.TableLayoutPanel();
            this.btnRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpRobot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.grbSetting.SuspendLayout();
            this.tlpRefresh.SuspendLayout();
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
            this.spcBase.Panel2.Controls.Add(this.tlpRobot);
            this.spcBase.Size = new System.Drawing.Size(1260, 522);
            // 
            // tlpRobot
            // 
            this.tlpRobot.ColumnCount = 1;
            this.tlpRobot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRobot.Controls.Add(this.dgvData, 0, 1);
            this.tlpRobot.Controls.Add(this.grbSetting, 0, 2);
            this.tlpRobot.Controls.Add(this.tlpRefresh, 0, 0);
            this.tlpRobot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRobot.Location = new System.Drawing.Point(0, 0);
            this.tlpRobot.Name = "tlpRobot";
            this.tlpRobot.RowCount = 3;
            this.tlpRobot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tlpRobot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRobot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tlpRobot.Size = new System.Drawing.Size(1260, 491);
            this.tlpRobot.TabIndex = 0;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNodeNo,
            this.colNodeID,
            this.colUnitNo,
            this.colUnitID,
            this.colRobotName,
            this.colPortfetchSeq,
            this.colSlotfetchSeq,
            this.colPortStoreSeq,
            this.colSlotStoreSeq,
            this.colRobotArmQTY,
            this.colArmJobQty,
            this.colRemark});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 50);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvData.RowHeadersVisible = false;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvData.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1254, 438);
            this.dgvData.TabIndex = 0;
            this.dgvData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvData_CellClick);
            // 
            // colNodeNo
            // 
            this.colNodeNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colNodeNo.HeaderText = "NodeNo";
            this.colNodeNo.Name = "colNodeNo";
            this.colNodeNo.ReadOnly = true;
            this.colNodeNo.Visible = false;
            this.colNodeNo.Width = 80;
            // 
            // colNodeID
            // 
            this.colNodeID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colNodeID.HeaderText = "Node ID";
            this.colNodeID.Name = "colNodeID";
            this.colNodeID.ReadOnly = true;
            this.colNodeID.Width = 90;
            // 
            // colUnitNo
            // 
            this.colUnitNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUnitNo.HeaderText = "UnitNo";
            this.colUnitNo.Name = "colUnitNo";
            this.colUnitNo.ReadOnly = true;
            this.colUnitNo.Visible = false;
            this.colUnitNo.Width = 80;
            // 
            // colUnitID
            // 
            this.colUnitID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUnitID.HeaderText = "Unit ID";
            this.colUnitID.Name = "colUnitID";
            this.colUnitID.ReadOnly = true;
            this.colUnitID.Width = 83;
            // 
            // colRobotName
            // 
            this.colRobotName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colRobotName.HeaderText = "Robot Name";
            this.colRobotName.Name = "colRobotName";
            this.colRobotName.ReadOnly = true;
            this.colRobotName.Width = 122;
            // 
            // colPortfetchSeq
            // 
            this.colPortfetchSeq.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colPortfetchSeq.HeaderText = "Port Fetch Seq";
            this.colPortfetchSeq.Name = "colPortfetchSeq";
            this.colPortfetchSeq.ReadOnly = true;
            this.colPortfetchSeq.Width = 135;
            // 
            // colSlotfetchSeq
            // 
            this.colSlotfetchSeq.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colSlotfetchSeq.HeaderText = "Slot Fetch Seq";
            this.colSlotfetchSeq.Name = "colSlotfetchSeq";
            this.colSlotfetchSeq.ReadOnly = true;
            this.colSlotfetchSeq.Width = 133;
            // 
            // colPortStoreSeq
            // 
            this.colPortStoreSeq.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colPortStoreSeq.HeaderText = "Port Store Seq";
            this.colPortStoreSeq.Name = "colPortStoreSeq";
            this.colPortStoreSeq.ReadOnly = true;
            this.colPortStoreSeq.Width = 135;
            // 
            // colSlotStoreSeq
            // 
            this.colSlotStoreSeq.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colSlotStoreSeq.HeaderText = "Slot Store Seq";
            this.colSlotStoreSeq.Name = "colSlotStoreSeq";
            this.colSlotStoreSeq.ReadOnly = true;
            this.colSlotStoreSeq.Width = 135;
            // 
            // colRobotArmQTY
            // 
            this.colRobotArmQTY.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colRobotArmQTY.HeaderText = "Arm Qty";
            this.colRobotArmQTY.Name = "colRobotArmQTY";
            this.colRobotArmQTY.ReadOnly = true;
            this.colRobotArmQTY.Width = 95;
            // 
            // colArmJobQty
            // 
            this.colArmJobQty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colArmJobQty.HeaderText = "Arm Job Qty";
            this.colArmJobQty.Name = "colArmJobQty";
            this.colArmJobQty.ReadOnly = true;
            this.colArmJobQty.Width = 120;
            // 
            // colRemark
            // 
            this.colRemark.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colRemark.HeaderText = "Remark";
            this.colRemark.Name = "colRemark";
            this.colRemark.ReadOnly = true;
            this.colRemark.Width = 205;
            // 
            // grbSetting
            // 
            this.grbSetting.Controls.Add(this.lblRobotName);
            this.grbSetting.Controls.Add(this.lblRobotNameCaption);
            this.grbSetting.Controls.Add(this.cboPortFetchSEQ);
            this.grbSetting.Controls.Add(this.cboSlotStoreSEQ);
            this.grbSetting.Controls.Add(this.lblPortFetchSeq);
            this.grbSetting.Controls.Add(this.lblSlotStoreSeq);
            this.grbSetting.Controls.Add(this.btnSetting);
            this.grbSetting.Controls.Add(this.cboPortStoreSEQ);
            this.grbSetting.Controls.Add(this.cboSlotFetchSEQ);
            this.grbSetting.Controls.Add(this.lblPortStoreSeq);
            this.grbSetting.Controls.Add(this.lblSlotFetchSeq);
            this.grbSetting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbSetting.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbSetting.Location = new System.Drawing.Point(3, 494);
            this.grbSetting.Name = "grbSetting";
            this.grbSetting.Size = new System.Drawing.Size(1254, 1);
            this.grbSetting.TabIndex = 11;
            this.grbSetting.TabStop = false;
            this.grbSetting.Text = "Setting";
            // 
            // lblRobotName
            // 
            this.lblRobotName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblRobotName.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRobotName.Location = new System.Drawing.Point(201, 29);
            this.lblRobotName.Name = "lblRobotName";
            this.lblRobotName.Size = new System.Drawing.Size(190, 23);
            this.lblRobotName.TabIndex = 12;
            // 
            // lblRobotNameCaption
            // 
            this.lblRobotNameCaption.BackColor = System.Drawing.Color.Black;
            this.lblRobotNameCaption.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRobotNameCaption.ForeColor = System.Drawing.Color.White;
            this.lblRobotNameCaption.Location = new System.Drawing.Point(59, 29);
            this.lblRobotNameCaption.Name = "lblRobotNameCaption";
            this.lblRobotNameCaption.Size = new System.Drawing.Size(140, 23);
            this.lblRobotNameCaption.TabIndex = 11;
            this.lblRobotNameCaption.Text = "Robot Name";
            // 
            // cboPortFetchSEQ
            // 
            this.cboPortFetchSEQ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPortFetchSEQ.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboPortFetchSEQ.FormattingEnabled = true;
            this.cboPortFetchSEQ.Items.AddRange(new object[] {
            "ASC",
            "DESC"});
            this.cboPortFetchSEQ.Location = new System.Drawing.Point(664, 25);
            this.cboPortFetchSEQ.Name = "cboPortFetchSEQ";
            this.cboPortFetchSEQ.Size = new System.Drawing.Size(190, 27);
            this.cboPortFetchSEQ.TabIndex = 0;
            this.cboPortFetchSEQ.Visible = false;
            // 
            // cboSlotStoreSEQ
            // 
            this.cboSlotStoreSEQ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSlotStoreSEQ.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboSlotStoreSEQ.FormattingEnabled = true;
            this.cboSlotStoreSEQ.Items.AddRange(new object[] {
            "ASC",
            "DESC"});
            this.cboSlotStoreSEQ.Location = new System.Drawing.Point(201, 72);
            this.cboSlotStoreSEQ.Name = "cboSlotStoreSEQ";
            this.cboSlotStoreSEQ.Size = new System.Drawing.Size(190, 27);
            this.cboSlotStoreSEQ.TabIndex = 0;
            // 
            // lblPortFetchSeq
            // 
            this.lblPortFetchSeq.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPortFetchSeq.Location = new System.Drawing.Point(522, 26);
            this.lblPortFetchSeq.Name = "lblPortFetchSeq";
            this.lblPortFetchSeq.Size = new System.Drawing.Size(165, 23);
            this.lblPortFetchSeq.TabIndex = 2;
            this.lblPortFetchSeq.Text = "Port Fetch Sequence";
            this.lblPortFetchSeq.Visible = false;
            // 
            // lblSlotStoreSeq
            // 
            this.lblSlotStoreSeq.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSlotStoreSeq.Location = new System.Drawing.Point(59, 73);
            this.lblSlotStoreSeq.Name = "lblSlotStoreSeq";
            this.lblSlotStoreSeq.Size = new System.Drawing.Size(165, 23);
            this.lblSlotStoreSeq.TabIndex = 5;
            this.lblSlotStoreSeq.Text = "Slot Store Sequence";
            // 
            // btnSetting
            // 
            this.btnSetting.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetting.Location = new System.Drawing.Point(397, 72);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(100, 30);
            this.btnSetting.TabIndex = 10;
            this.btnSetting.Text = "SET";
            this.btnSetting.UseVisualStyleBackColor = true;
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // cboPortStoreSEQ
            // 
            this.cboPortStoreSEQ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPortStoreSEQ.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboPortStoreSEQ.FormattingEnabled = true;
            this.cboPortStoreSEQ.Items.AddRange(new object[] {
            "ASC",
            "DESC"});
            this.cboPortStoreSEQ.Location = new System.Drawing.Point(664, 72);
            this.cboPortStoreSEQ.Name = "cboPortStoreSEQ";
            this.cboPortStoreSEQ.Size = new System.Drawing.Size(190, 27);
            this.cboPortStoreSEQ.TabIndex = 0;
            this.cboPortStoreSEQ.Visible = false;
            // 
            // cboSlotFetchSEQ
            // 
            this.cboSlotFetchSEQ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSlotFetchSEQ.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboSlotFetchSEQ.FormattingEnabled = true;
            this.cboSlotFetchSEQ.Items.AddRange(new object[] {
            "ASC",
            "DESC"});
            this.cboSlotFetchSEQ.Location = new System.Drawing.Point(1046, 25);
            this.cboSlotFetchSEQ.Name = "cboSlotFetchSEQ";
            this.cboSlotFetchSEQ.Size = new System.Drawing.Size(190, 27);
            this.cboSlotFetchSEQ.TabIndex = 0;
            this.cboSlotFetchSEQ.Visible = false;
            // 
            // lblPortStoreSeq
            // 
            this.lblPortStoreSeq.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPortStoreSeq.Location = new System.Drawing.Point(522, 73);
            this.lblPortStoreSeq.Name = "lblPortStoreSeq";
            this.lblPortStoreSeq.Size = new System.Drawing.Size(165, 23);
            this.lblPortStoreSeq.TabIndex = 4;
            this.lblPortStoreSeq.Text = "Port Store Sequence";
            this.lblPortStoreSeq.Visible = false;
            // 
            // lblSlotFetchSeq
            // 
            this.lblSlotFetchSeq.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSlotFetchSeq.Location = new System.Drawing.Point(905, 26);
            this.lblSlotFetchSeq.Name = "lblSlotFetchSeq";
            this.lblSlotFetchSeq.Size = new System.Drawing.Size(165, 23);
            this.lblSlotFetchSeq.TabIndex = 3;
            this.lblSlotFetchSeq.Text = "Slot Fetch Sequence";
            this.lblSlotFetchSeq.Visible = false;
            // 
            // tlpRefresh
            // 
            this.tlpRefresh.ColumnCount = 2;
            this.tlpRefresh.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRefresh.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 137F));
            this.tlpRefresh.Controls.Add(this.btnRefresh, 1, 0);
            this.tlpRefresh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRefresh.Location = new System.Drawing.Point(3, 3);
            this.tlpRefresh.Name = "tlpRefresh";
            this.tlpRefresh.RowCount = 1;
            this.tlpRefresh.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRefresh.Size = new System.Drawing.Size(1254, 41);
            this.tlpRefresh.TabIndex = 12;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(1120, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 35);
            this.btnRefresh.TabIndex = 12;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // FormRobotManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1260, 522);
            this.Name = "FormRobotManagement";
            this.Text = "  ";
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpRobot.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.grbSetting.ResumeLayout(false);
            this.tlpRefresh.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpRobot;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Label lblPortFetchSeq;
        private System.Windows.Forms.Button btnSetting;
        private System.Windows.Forms.Label lblSlotStoreSeq;
        private System.Windows.Forms.Label lblPortStoreSeq;
        private System.Windows.Forms.Label lblSlotFetchSeq;
        private System.Windows.Forms.ComboBox cboPortFetchSEQ;
        private System.Windows.Forms.ComboBox cboSlotStoreSEQ;
        private System.Windows.Forms.ComboBox cboPortStoreSEQ;
        private System.Windows.Forms.ComboBox cboSlotFetchSEQ;
        private System.Windows.Forms.GroupBox grbSetting;
        private System.Windows.Forms.Label lblRobotName;
        private System.Windows.Forms.Label lblRobotNameCaption;
        private System.Windows.Forms.TableLayoutPanel tlpRefresh;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNodeNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNodeID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnitID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPortfetchSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotfetchSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPortStoreSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotStoreSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotArmQTY;
        private System.Windows.Forms.DataGridViewTextBoxColumn colArmJobQty;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemark;

    }
}