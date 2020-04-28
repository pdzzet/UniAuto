namespace UniOPI
{
    partial class FormChangerPlan
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlCombox = new System.Windows.Forms.Panel();
            this.lblPlanID = new System.Windows.Forms.Label();
            this.cmbPlanID = new System.Windows.Forms.ComboBox();
            this.gbChangePlan = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.dgvtxtOBJECTKEY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvcSlotNO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvcPlanID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvcGlassID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvcSourceCSTID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvcTargetCSTID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TargetSlotNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpButton = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAddPlan = new System.Windows.Forms.Button();
            this.btnModifyPlan = new System.Windows.Forms.Button();
            this.btnDeletePlan = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlCombox.SuspendLayout();
            this.gbChangePlan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tlpButton.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tableLayoutPanel1);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.tableLayoutPanel1.Controls.Add(this.pnlCombox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gbChangePlan, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tlpButton, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 517);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // pnlCombox
            // 
            this.pnlCombox.Controls.Add(this.lblPlanID);
            this.pnlCombox.Controls.Add(this.cmbPlanID);
            this.pnlCombox.Location = new System.Drawing.Point(3, 3);
            this.pnlCombox.Name = "pnlCombox";
            this.pnlCombox.Size = new System.Drawing.Size(700, 59);
            this.pnlCombox.TabIndex = 25;
            // 
            // lblPlanID
            // 
            this.lblPlanID.AutoSize = true;
            this.lblPlanID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlanID.Location = new System.Drawing.Point(9, 19);
            this.lblPlanID.Name = "lblPlanID";
            this.lblPlanID.Size = new System.Drawing.Size(58, 21);
            this.lblPlanID.TabIndex = 5;
            this.lblPlanID.Text = "Plan ID";
            // 
            // cmbPlanID
            // 
            this.cmbPlanID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlanID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbPlanID.FormattingEnabled = true;
            this.cmbPlanID.Location = new System.Drawing.Point(89, 16);
            this.cmbPlanID.Name = "cmbPlanID";
            this.cmbPlanID.Size = new System.Drawing.Size(213, 29);
            this.cmbPlanID.TabIndex = 4;
            this.cmbPlanID.SelectedIndexChanged += new System.EventHandler(this.cmbPlanID_SelectedIndexChanged);
            // 
            // gbChangePlan
            // 
            this.gbChangePlan.Controls.Add(this.dgvData);
            this.gbChangePlan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbChangePlan.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbChangePlan.Location = new System.Drawing.Point(3, 68);
            this.gbChangePlan.Name = "gbChangePlan";
            this.gbChangePlan.Size = new System.Drawing.Size(1125, 446);
            this.gbChangePlan.TabIndex = 23;
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
            this.dgvtxtOBJECTKEY,
            this.dgvcSlotNO,
            this.dgvcPlanID,
            this.dgvcGlassID,
            this.dgvcSourceCSTID,
            this.dgvcTargetCSTID,
            this.TargetSlotNo});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 23);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1119, 420);
            this.dgvData.TabIndex = 12;
            this.dgvData.DataSourceChanged += new System.EventHandler(this.dgvData_DataSourceChanged);
            // 
            // dgvtxtOBJECTKEY
            // 
            this.dgvtxtOBJECTKEY.DataPropertyName = "OBJECTKEY";
            this.dgvtxtOBJECTKEY.HeaderText = "OBJECTKEY";
            this.dgvtxtOBJECTKEY.Name = "dgvtxtOBJECTKEY";
            this.dgvtxtOBJECTKEY.ReadOnly = true;
            this.dgvtxtOBJECTKEY.Visible = false;
            // 
            // dgvcSlotNO
            // 
            this.dgvcSlotNO.DataPropertyName = "SLOTNO";
            this.dgvcSlotNO.HeaderText = "Slot No";
            this.dgvcSlotNO.Name = "dgvcSlotNO";
            this.dgvcSlotNO.ReadOnly = true;
            // 
            // dgvcPlanID
            // 
            this.dgvcPlanID.DataPropertyName = "PLANID";
            this.dgvcPlanID.HeaderText = "PlanID";
            this.dgvcPlanID.Name = "dgvcPlanID";
            this.dgvcPlanID.ReadOnly = true;
            this.dgvcPlanID.Visible = false;
            // 
            // dgvcGlassID
            // 
            this.dgvcGlassID.DataPropertyName = "JOBID";
            this.dgvcGlassID.HeaderText = "Glass ID";
            this.dgvcGlassID.Name = "dgvcGlassID";
            this.dgvcGlassID.ReadOnly = true;
            this.dgvcGlassID.Width = 200;
            // 
            // dgvcSourceCSTID
            // 
            this.dgvcSourceCSTID.DataPropertyName = "SOURCECASSETTEID";
            this.dgvcSourceCSTID.HeaderText = "Source CST ID";
            this.dgvcSourceCSTID.Name = "dgvcSourceCSTID";
            this.dgvcSourceCSTID.ReadOnly = true;
            this.dgvcSourceCSTID.Width = 200;
            // 
            // dgvcTargetCSTID
            // 
            this.dgvcTargetCSTID.DataPropertyName = "TARGETASSETTEID";
            this.dgvcTargetCSTID.HeaderText = "Target CST ID";
            this.dgvcTargetCSTID.Name = "dgvcTargetCSTID";
            this.dgvcTargetCSTID.ReadOnly = true;
            this.dgvcTargetCSTID.Width = 200;
            // 
            // TargetSlotNo
            // 
            this.TargetSlotNo.DataPropertyName = "TARGETSLOTNO";
            this.TargetSlotNo.HeaderText = "Target SlotNo";
            this.TargetSlotNo.Name = "TargetSlotNo";
            this.TargetSlotNo.ReadOnly = true;
            this.TargetSlotNo.Width = 200;
            // 
            // tlpButton
            // 
            this.tlpButton.ColumnCount = 1;
            this.tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButton.Controls.Add(this.panel1, 0, 1);
            this.tlpButton.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tlpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpButton.Location = new System.Drawing.Point(1134, 68);
            this.tlpButton.Name = "tlpButton";
            this.tlpButton.RowCount = 2;
            this.tlpButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 123F));
            this.tlpButton.Size = new System.Drawing.Size(127, 446);
            this.tlpButton.TabIndex = 14;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 326);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(121, 117);
            this.panel1.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnAddPlan);
            this.flowLayoutPanel1.Controls.Add(this.btnModifyPlan);
            this.flowLayoutPanel1.Controls.Add(this.btnDeletePlan);
            this.flowLayoutPanel1.Controls.Add(this.btnSave);
            this.flowLayoutPanel1.Controls.Add(this.btnRefresh);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(121, 317);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // btnAddPlan
            // 
            this.btnAddPlan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddPlan.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddPlan.Location = new System.Drawing.Point(3, 3);
            this.btnAddPlan.Name = "btnAddPlan";
            this.btnAddPlan.Size = new System.Drawing.Size(110, 50);
            this.btnAddPlan.TabIndex = 14;
            this.btnAddPlan.Text = "Add Plan";
            this.btnAddPlan.UseVisualStyleBackColor = true;
            this.btnAddPlan.Click += new System.EventHandler(this.btnAddPlan_Click);
            // 
            // btnModifyPlan
            // 
            this.btnModifyPlan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnModifyPlan.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModifyPlan.Location = new System.Drawing.Point(3, 59);
            this.btnModifyPlan.Name = "btnModifyPlan";
            this.btnModifyPlan.Size = new System.Drawing.Size(110, 50);
            this.btnModifyPlan.TabIndex = 16;
            this.btnModifyPlan.Text = "Modify Plan";
            this.btnModifyPlan.UseVisualStyleBackColor = true;
            this.btnModifyPlan.Click += new System.EventHandler(this.btnModifyPlan_Click);
            // 
            // btnDeletePlan
            // 
            this.btnDeletePlan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDeletePlan.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeletePlan.Location = new System.Drawing.Point(3, 115);
            this.btnDeletePlan.Name = "btnDeletePlan";
            this.btnDeletePlan.Size = new System.Drawing.Size(110, 50);
            this.btnDeletePlan.TabIndex = 15;
            this.btnDeletePlan.Text = "Delete Plan";
            this.btnDeletePlan.UseVisualStyleBackColor = true;
            this.btnDeletePlan.Click += new System.EventHandler(this.btnDeletePlan_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(3, 171);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 50);
            this.btnSave.TabIndex = 18;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(3, 227);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(110, 50);
            this.btnRefresh.TabIndex = 17;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // FormChangerPlan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 548);
            this.Name = "FormChangerPlan";
            this.Text = "FormChangePlan";
            this.Load += new System.EventHandler(this.FormChangePlan_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlCombox.ResumeLayout(false);
            this.pnlCombox.PerformLayout();
            this.gbChangePlan.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tlpButton.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel pnlCombox;
        private System.Windows.Forms.Label lblPlanID;
        private System.Windows.Forms.ComboBox cmbPlanID;
        private System.Windows.Forms.GroupBox gbChangePlan;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.TableLayoutPanel tlpButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnAddPlan;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnDeletePlan;
        private System.Windows.Forms.Button btnModifyPlan;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtOBJECTKEY;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvcSlotNO;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvcPlanID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvcGlassID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvcSourceCSTID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvcTargetCSTID;
        private System.Windows.Forms.DataGridViewTextBoxColumn TargetSlotNo;


    }
}