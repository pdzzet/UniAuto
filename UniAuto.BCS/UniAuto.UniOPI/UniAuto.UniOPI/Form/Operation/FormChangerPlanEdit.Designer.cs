namespace UniOPI
{
    partial class FormChangerPlanEdit
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
            this.pnlbuttom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.gbEditMode = new System.Windows.Forms.GroupBox();
            this.tlpChangePlan = new System.Windows.Forms.TableLayoutPanel();
            this.pnlPlan = new System.Windows.Forms.Panel();
            this.txtPlanID = new System.Windows.Forms.TextBox();
            this.lblPlanID = new System.Windows.Forms.Label();
            this.dgvChgPlan = new System.Windows.Forms.DataGridView();
            this.colOBJECTKEY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSlotNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGlassID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSourceCSTID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTargetCSTID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPlan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTargetSlotNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTargetSlotSelect = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlData = new System.Windows.Forms.Panel();
            this.rdoManual = new System.Windows.Forms.RadioButton();
            this.rdoAuto = new System.Windows.Forms.RadioButton();
            this.lblTargetSlotSelect = new System.Windows.Forms.Label();
            this.txtGlassID = new System.Windows.Forms.TextBox();
            this.lblGlassID = new System.Windows.Forms.Label();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSet = new System.Windows.Forms.Button();
            this.txtTargetCstID = new System.Windows.Forms.TextBox();
            this.lblTargetCSTID = new System.Windows.Forms.Label();
            this.txtSourceCstID = new System.Windows.Forms.TextBox();
            this.lblSourceCSTID = new System.Windows.Forms.Label();
            this.txtEndSlotNo = new System.Windows.Forms.TextBox();
            this.lblDesc = new System.Windows.Forms.Label();
            this.txtStartSlotNo = new System.Windows.Forms.TextBox();
            this.lblSlotNo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.pnlbuttom.SuspendLayout();
            this.gbEditMode.SuspendLayout();
            this.tlpChangePlan.SuspendLayout();
            this.pnlPlan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChgPlan)).BeginInit();
            this.pnlData.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(999, 30);
            this.lblCaption.Text = "Changer Plan";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(1059, 636);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.pnlbuttom, 0, 1);
            this.tlpBase.Controls.Add(this.gbEditMode, 0, 0);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 92.69841F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.301587F));
            this.tlpBase.Size = new System.Drawing.Size(1059, 605);
            this.tlpBase.TabIndex = 0;
            // 
            // pnlbuttom
            // 
            this.pnlbuttom.Controls.Add(this.btnCancel);
            this.pnlbuttom.Controls.Add(this.btnOK);
            this.pnlbuttom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlbuttom.Location = new System.Drawing.Point(3, 563);
            this.pnlbuttom.Name = "pnlbuttom";
            this.pnlbuttom.Size = new System.Drawing.Size(1053, 39);
            this.pnlbuttom.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(525, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(434, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 35);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // gbEditMode
            // 
            this.gbEditMode.Controls.Add(this.tlpChangePlan);
            this.gbEditMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbEditMode.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.gbEditMode.Location = new System.Drawing.Point(3, 3);
            this.gbEditMode.Name = "gbEditMode";
            this.gbEditMode.Size = new System.Drawing.Size(1053, 554);
            this.gbEditMode.TabIndex = 1;
            this.gbEditMode.TabStop = false;
            // 
            // tlpChangePlan
            // 
            this.tlpChangePlan.ColumnCount = 1;
            this.tlpChangePlan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpChangePlan.Controls.Add(this.pnlPlan, 0, 0);
            this.tlpChangePlan.Controls.Add(this.dgvChgPlan, 0, 2);
            this.tlpChangePlan.Controls.Add(this.pnlData, 0, 1);
            this.tlpChangePlan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpChangePlan.Location = new System.Drawing.Point(3, 24);
            this.tlpChangePlan.Name = "tlpChangePlan";
            this.tlpChangePlan.RowCount = 3;
            this.tlpChangePlan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tlpChangePlan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 99F));
            this.tlpChangePlan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpChangePlan.Size = new System.Drawing.Size(1047, 527);
            this.tlpChangePlan.TabIndex = 76;
            // 
            // pnlPlan
            // 
            this.pnlPlan.Controls.Add(this.txtPlanID);
            this.pnlPlan.Controls.Add(this.lblPlanID);
            this.pnlPlan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPlan.Location = new System.Drawing.Point(0, 0);
            this.pnlPlan.Margin = new System.Windows.Forms.Padding(0);
            this.pnlPlan.Name = "pnlPlan";
            this.pnlPlan.Size = new System.Drawing.Size(1047, 37);
            this.pnlPlan.TabIndex = 0;
            // 
            // txtPlanID
            // 
            this.txtPlanID.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtPlanID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPlanID.Location = new System.Drawing.Point(155, 4);
            this.txtPlanID.MaxLength = 8;
            this.txtPlanID.Name = "txtPlanID";
            this.txtPlanID.Size = new System.Drawing.Size(167, 28);
            this.txtPlanID.TabIndex = 74;
            this.txtPlanID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPlanID_KeyPress);
            // 
            // lblPlanID
            // 
            this.lblPlanID.BackColor = System.Drawing.Color.Black;
            this.lblPlanID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlanID.ForeColor = System.Drawing.Color.White;
            this.lblPlanID.Location = new System.Drawing.Point(9, 5);
            this.lblPlanID.Name = "lblPlanID";
            this.lblPlanID.Size = new System.Drawing.Size(150, 27);
            this.lblPlanID.TabIndex = 75;
            this.lblPlanID.Text = "Plan ID";
            this.lblPlanID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dgvChgPlan
            // 
            this.dgvChgPlan.AllowUserToAddRows = false;
            this.dgvChgPlan.AllowUserToDeleteRows = false;
            this.dgvChgPlan.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvChgPlan.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvChgPlan.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12.75F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvChgPlan.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvChgPlan.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvChgPlan.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colOBJECTKEY,
            this.colSlotNo,
            this.colGlassID,
            this.colSourceCSTID,
            this.colTargetCSTID,
            this.colLineID,
            this.colServerName,
            this.colPlan,
            this.colTargetSlotNo,
            this.colTargetSlotSelect});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvChgPlan.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvChgPlan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvChgPlan.Location = new System.Drawing.Point(3, 139);
            this.dgvChgPlan.Name = "dgvChgPlan";
            this.dgvChgPlan.RowHeadersVisible = false;
            this.dgvChgPlan.RowTemplate.Height = 24;
            this.dgvChgPlan.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvChgPlan.Size = new System.Drawing.Size(1041, 385);
            this.dgvChgPlan.TabIndex = 13;
            this.dgvChgPlan.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvChgPlan_CellEndEdit);
            this.dgvChgPlan.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgDepotInMx_EditingControlShowing);
            // 
            // colOBJECTKEY
            // 
            this.colOBJECTKEY.DataPropertyName = "OBJECTKEY";
            this.colOBJECTKEY.HeaderText = "OBJECTKEY";
            this.colOBJECTKEY.Name = "colOBJECTKEY";
            this.colOBJECTKEY.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colOBJECTKEY.Visible = false;
            // 
            // colSlotNo
            // 
            this.colSlotNo.DataPropertyName = "SLOTNO";
            this.colSlotNo.HeaderText = "Slot No";
            this.colSlotNo.Name = "colSlotNo";
            this.colSlotNo.ReadOnly = true;
            this.colSlotNo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colGlassID
            // 
            this.colGlassID.DataPropertyName = "JOBID";
            this.colGlassID.HeaderText = "Glass ID";
            this.colGlassID.Name = "colGlassID";
            this.colGlassID.ReadOnly = true;
            this.colGlassID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colGlassID.Width = 150;
            // 
            // colSourceCSTID
            // 
            this.colSourceCSTID.DataPropertyName = "SOURCECASSETTEID";
            this.colSourceCSTID.HeaderText = "Source CST ID";
            this.colSourceCSTID.Name = "colSourceCSTID";
            this.colSourceCSTID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colSourceCSTID.Width = 250;
            // 
            // colTargetCSTID
            // 
            this.colTargetCSTID.DataPropertyName = "TARGETASSETTEID";
            this.colTargetCSTID.HeaderText = "Target CST ID";
            this.colTargetCSTID.Name = "colTargetCSTID";
            this.colTargetCSTID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colTargetCSTID.Width = 250;
            // 
            // colLineID
            // 
            this.colLineID.DataPropertyName = "LINEID";
            this.colLineID.HeaderText = "Line ID";
            this.colLineID.Name = "colLineID";
            this.colLineID.ReadOnly = true;
            this.colLineID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colLineID.Visible = false;
            this.colLineID.Width = 300;
            // 
            // colServerName
            // 
            this.colServerName.DataPropertyName = "SERVERNAME";
            this.colServerName.HeaderText = "ServerName";
            this.colServerName.Name = "colServerName";
            this.colServerName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colServerName.Visible = false;
            // 
            // colPlan
            // 
            this.colPlan.DataPropertyName = "PLANID";
            this.colPlan.HeaderText = "Plan ID";
            this.colPlan.Name = "colPlan";
            this.colPlan.ReadOnly = true;
            this.colPlan.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colPlan.Visible = false;
            // 
            // colTargetSlotNo
            // 
            this.colTargetSlotNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTargetSlotNo.DataPropertyName = "TARGETSLOTNO";
            this.colTargetSlotNo.HeaderText = "Target SlotNo";
            this.colTargetSlotNo.MaxInputLength = 3;
            this.colTargetSlotNo.Name = "colTargetSlotNo";
            // 
            // colTargetSlotSelect
            // 
            this.colTargetSlotSelect.DataPropertyName = "TARGETSLOTSELECT";
            this.colTargetSlotSelect.HeaderText = "Target Slot Select";
            this.colTargetSlotSelect.Name = "colTargetSlotSelect";
            this.colTargetSlotSelect.Width = 155;
            // 
            // pnlData
            // 
            this.pnlData.Controls.Add(this.rdoManual);
            this.pnlData.Controls.Add(this.rdoAuto);
            this.pnlData.Controls.Add(this.lblTargetSlotSelect);
            this.pnlData.Controls.Add(this.txtGlassID);
            this.pnlData.Controls.Add(this.lblGlassID);
            this.pnlData.Controls.Add(this.btnDelete);
            this.pnlData.Controls.Add(this.btnSet);
            this.pnlData.Controls.Add(this.txtTargetCstID);
            this.pnlData.Controls.Add(this.lblTargetCSTID);
            this.pnlData.Controls.Add(this.txtSourceCstID);
            this.pnlData.Controls.Add(this.lblSourceCSTID);
            this.pnlData.Controls.Add(this.txtEndSlotNo);
            this.pnlData.Controls.Add(this.lblDesc);
            this.pnlData.Controls.Add(this.txtStartSlotNo);
            this.pnlData.Controls.Add(this.lblSlotNo);
            this.pnlData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlData.Location = new System.Drawing.Point(3, 40);
            this.pnlData.Name = "pnlData";
            this.pnlData.Size = new System.Drawing.Size(1041, 93);
            this.pnlData.TabIndex = 14;
            // 
            // rdoManual
            // 
            this.rdoManual.AutoSize = true;
            this.rdoManual.Location = new System.Drawing.Point(538, 13);
            this.rdoManual.Name = "rdoManual";
            this.rdoManual.Size = new System.Drawing.Size(81, 25);
            this.rdoManual.TabIndex = 90;
            this.rdoManual.Text = "Manual";
            this.rdoManual.UseVisualStyleBackColor = true;
            // 
            // rdoAuto
            // 
            this.rdoAuto.AutoSize = true;
            this.rdoAuto.Location = new System.Drawing.Point(480, 13);
            this.rdoAuto.Name = "rdoAuto";
            this.rdoAuto.Size = new System.Drawing.Size(62, 25);
            this.rdoAuto.TabIndex = 89;
            this.rdoAuto.Text = "Auto";
            this.rdoAuto.UseVisualStyleBackColor = true;
            // 
            // lblTargetSlotSelect
            // 
            this.lblTargetSlotSelect.BackColor = System.Drawing.Color.Black;
            this.lblTargetSlotSelect.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetSlotSelect.ForeColor = System.Drawing.Color.White;
            this.lblTargetSlotSelect.Location = new System.Drawing.Point(327, 12);
            this.lblTargetSlotSelect.Name = "lblTargetSlotSelect";
            this.lblTargetSlotSelect.Size = new System.Drawing.Size(150, 27);
            this.lblTargetSlotSelect.TabIndex = 88;
            this.lblTargetSlotSelect.Text = "Target Slot Select";
            this.lblTargetSlotSelect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtGlassID
            // 
            this.txtGlassID.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtGlassID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtGlassID.Location = new System.Drawing.Point(152, 43);
            this.txtGlassID.Name = "txtGlassID";
            this.txtGlassID.Size = new System.Drawing.Size(167, 28);
            this.txtGlassID.TabIndex = 87;
            this.txtGlassID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtGlassID_KeyPress);
            // 
            // lblGlassID
            // 
            this.lblGlassID.BackColor = System.Drawing.Color.Black;
            this.lblGlassID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGlassID.ForeColor = System.Drawing.Color.White;
            this.lblGlassID.Location = new System.Drawing.Point(6, 44);
            this.lblGlassID.Name = "lblGlassID";
            this.lblGlassID.Size = new System.Drawing.Size(150, 27);
            this.lblGlassID.TabIndex = 86;
            this.lblGlassID.Text = "Glass ID";
            this.lblGlassID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnDelete
            // 
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(950, 36);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(80, 35);
            this.btnDelete.TabIndex = 85;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnSet
            // 
            this.btnSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSet.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSet.Location = new System.Drawing.Point(864, 36);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(80, 35);
            this.btnSet.TabIndex = 84;
            this.btnSet.Text = "Set";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // txtTargetCstID
            // 
            this.txtTargetCstID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTargetCstID.Location = new System.Drawing.Point(735, 43);
            this.txtTargetCstID.Name = "txtTargetCstID";
            this.txtTargetCstID.Size = new System.Drawing.Size(100, 28);
            this.txtTargetCstID.TabIndex = 82;
            // 
            // lblTargetCSTID
            // 
            this.lblTargetCSTID.BackColor = System.Drawing.Color.Black;
            this.lblTargetCSTID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetCSTID.ForeColor = System.Drawing.Color.White;
            this.lblTargetCSTID.Location = new System.Drawing.Point(585, 44);
            this.lblTargetCSTID.Name = "lblTargetCSTID";
            this.lblTargetCSTID.Size = new System.Drawing.Size(150, 27);
            this.lblTargetCSTID.TabIndex = 83;
            this.lblTargetCSTID.Text = "Target CST ID";
            this.lblTargetCSTID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSourceCstID
            // 
            this.txtSourceCstID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSourceCstID.Location = new System.Drawing.Point(477, 43);
            this.txtSourceCstID.Name = "txtSourceCstID";
            this.txtSourceCstID.Size = new System.Drawing.Size(100, 28);
            this.txtSourceCstID.TabIndex = 80;
            // 
            // lblSourceCSTID
            // 
            this.lblSourceCSTID.BackColor = System.Drawing.Color.Black;
            this.lblSourceCSTID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSourceCSTID.ForeColor = System.Drawing.Color.White;
            this.lblSourceCSTID.Location = new System.Drawing.Point(327, 44);
            this.lblSourceCSTID.Name = "lblSourceCSTID";
            this.lblSourceCSTID.Size = new System.Drawing.Size(150, 27);
            this.lblSourceCSTID.TabIndex = 81;
            this.lblSourceCSTID.Text = "Source CST ID";
            this.lblSourceCSTID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtEndSlotNo
            // 
            this.txtEndSlotNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEndSlotNo.Location = new System.Drawing.Point(247, 10);
            this.txtEndSlotNo.MaxLength = 3;
            this.txtEndSlotNo.Name = "txtEndSlotNo";
            this.txtEndSlotNo.Size = new System.Drawing.Size(71, 28);
            this.txtEndSlotNo.TabIndex = 79;
            this.txtEndSlotNo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSlotNo_KeyPress);
            // 
            // lblDesc
            // 
            this.lblDesc.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDesc.Location = new System.Drawing.Point(225, 14);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(21, 21);
            this.lblDesc.TabIndex = 78;
            this.lblDesc.Text = "~";
            this.lblDesc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtStartSlotNo
            // 
            this.txtStartSlotNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStartSlotNo.Location = new System.Drawing.Point(152, 10);
            this.txtStartSlotNo.MaxLength = 3;
            this.txtStartSlotNo.Name = "txtStartSlotNo";
            this.txtStartSlotNo.Size = new System.Drawing.Size(71, 28);
            this.txtStartSlotNo.TabIndex = 76;
            this.txtStartSlotNo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSlotNo_KeyPress);
            // 
            // lblSlotNo
            // 
            this.lblSlotNo.BackColor = System.Drawing.Color.Black;
            this.lblSlotNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSlotNo.ForeColor = System.Drawing.Color.White;
            this.lblSlotNo.Location = new System.Drawing.Point(6, 11);
            this.lblSlotNo.Name = "lblSlotNo";
            this.lblSlotNo.Size = new System.Drawing.Size(150, 27);
            this.lblSlotNo.TabIndex = 77;
            this.lblSlotNo.Text = "Slot No";
            this.lblSlotNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FormChangerPlanEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 636);
            this.Name = "FormChangerPlanEdit";
            this.Text = " ";
            this.Load += new System.EventHandler(this.FormChangerPlanEdit_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.pnlbuttom.ResumeLayout(false);
            this.gbEditMode.ResumeLayout(false);
            this.tlpChangePlan.ResumeLayout(false);
            this.pnlPlan.ResumeLayout(false);
            this.pnlPlan.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChgPlan)).EndInit();
            this.pnlData.ResumeLayout(false);
            this.pnlData.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel pnlbuttom;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox gbEditMode;
        private System.Windows.Forms.TextBox txtPlanID;
        private System.Windows.Forms.Label lblPlanID;
        private System.Windows.Forms.TableLayoutPanel tlpChangePlan;
        private System.Windows.Forms.Panel pnlPlan;
        private System.Windows.Forms.DataGridView dgvChgPlan;
        private System.Windows.Forms.Panel pnlData;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.TextBox txtTargetCstID;
        private System.Windows.Forms.Label lblTargetCSTID;
        private System.Windows.Forms.TextBox txtSourceCstID;
        private System.Windows.Forms.Label lblSourceCSTID;
        private System.Windows.Forms.TextBox txtEndSlotNo;
        private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.TextBox txtStartSlotNo;
        private System.Windows.Forms.Label lblSlotNo;
        private System.Windows.Forms.Label lblGlassID;
        private System.Windows.Forms.TextBox txtGlassID;
        private System.Windows.Forms.RadioButton rdoManual;
        private System.Windows.Forms.RadioButton rdoAuto;
        private System.Windows.Forms.Label lblTargetSlotSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOBJECTKEY;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGlassID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSourceCSTID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTargetCSTID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPlan;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTargetSlotNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTargetSlotSelect;

    }
}