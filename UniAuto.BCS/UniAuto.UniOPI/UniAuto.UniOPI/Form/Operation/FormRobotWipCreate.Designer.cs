namespace UniOPI
{
    partial class FormRobotWipCreate
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.flpGlassInfo = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlCstSeqNo = new System.Windows.Forms.Panel();
            this.lblRobotName = new System.Windows.Forms.Label();
            this.txtRobotName = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblCstSeqNo = new System.Windows.Forms.Label();
            this.txtCstSeqNo = new System.Windows.Forms.TextBox();
            this.pnlJobSeqNo = new System.Windows.Forms.Panel();
            this.lblJobSeqNo = new System.Windows.Forms.Label();
            this.txtJobSeqNo = new System.Windows.Forms.TextBox();
            this.pnlGlassID = new System.Windows.Forms.Panel();
            this.lblGlassID = new System.Windows.Forms.Label();
            this.txtGlassID = new System.Windows.Forms.TextBox();
            this.grbNewStep = new System.Windows.Forms.GroupBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.cboCurStepID = new System.Windows.Forms.ComboBox();
            this.lblCurStepID = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.cboRouteID = new System.Windows.Forms.ComboBox();
            this.lblRouteID = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.cboNextStepID = new System.Windows.Forms.ComboBox();
            this.lblNextStepID = new System.Windows.Forms.Label();
            this.dgvRouteStep = new System.Windows.Forms.DataGridView();
            this.colStep = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotArm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRouteRule = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageNoList = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.panel2.SuspendLayout();
            this.flpGlassInfo.SuspendLayout();
            this.pnlCstSeqNo.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlJobSeqNo.SuspendLayout();
            this.pnlGlassID.SuspendLayout();
            this.grbNewStep.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRouteStep)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(825, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(885, 639);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 3;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpBase.Controls.Add(this.panel2, 1, 3);
            this.tlpBase.Controls.Add(this.flpGlassInfo, 1, 0);
            this.tlpBase.Controls.Add(this.grbNewStep, 1, 1);
            this.tlpBase.Controls.Add(this.dgvRouteStep, 1, 2);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 4;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 97F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 149F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Size = new System.Drawing.Size(885, 608);
            this.tlpBase.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnClose);
            this.panel2.Controls.Add(this.btnSave);
            this.panel2.Location = new System.Drawing.Point(13, 561);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(859, 44);
            this.panel2.TabIndex = 22;
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(153, 6);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 35);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // flpGlassInfo
            // 
            this.flpGlassInfo.AutoScroll = true;
            this.flpGlassInfo.Controls.Add(this.pnlCstSeqNo);
            this.flpGlassInfo.Controls.Add(this.panel1);
            this.flpGlassInfo.Controls.Add(this.pnlJobSeqNo);
            this.flpGlassInfo.Controls.Add(this.pnlGlassID);
            this.flpGlassInfo.Location = new System.Drawing.Point(13, 3);
            this.flpGlassInfo.Name = "flpGlassInfo";
            this.flpGlassInfo.Size = new System.Drawing.Size(859, 91);
            this.flpGlassInfo.TabIndex = 23;
            // 
            // pnlCstSeqNo
            // 
            this.pnlCstSeqNo.Controls.Add(this.lblRobotName);
            this.pnlCstSeqNo.Controls.Add(this.txtRobotName);
            this.pnlCstSeqNo.Location = new System.Drawing.Point(3, 1);
            this.pnlCstSeqNo.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlCstSeqNo.Name = "pnlCstSeqNo";
            this.pnlCstSeqNo.Size = new System.Drawing.Size(420, 30);
            this.pnlCstSeqNo.TabIndex = 8;
            this.pnlCstSeqNo.Tag = "CASSETTESEQUENCENO";
            // 
            // lblRobotName
            // 
            this.lblRobotName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblRobotName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRobotName.ForeColor = System.Drawing.Color.White;
            this.lblRobotName.Location = new System.Drawing.Point(0, 2);
            this.lblRobotName.Name = "lblRobotName";
            this.lblRobotName.Size = new System.Drawing.Size(150, 25);
            this.lblRobotName.TabIndex = 5;
            this.lblRobotName.Text = "Robot Name";
            this.lblRobotName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtRobotName
            // 
            this.txtRobotName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtRobotName.Location = new System.Drawing.Point(150, 3);
            this.txtRobotName.Name = "txtRobotName";
            this.txtRobotName.ReadOnly = true;
            this.txtRobotName.Size = new System.Drawing.Size(253, 25);
            this.txtRobotName.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblCstSeqNo);
            this.panel1.Controls.Add(this.txtCstSeqNo);
            this.panel1.Location = new System.Drawing.Point(429, 1);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(420, 30);
            this.panel1.TabIndex = 23;
            this.panel1.Tag = "CASSETTESEQUENCENO";
            // 
            // lblCstSeqNo
            // 
            this.lblCstSeqNo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblCstSeqNo.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCstSeqNo.ForeColor = System.Drawing.Color.White;
            this.lblCstSeqNo.Location = new System.Drawing.Point(0, 2);
            this.lblCstSeqNo.Name = "lblCstSeqNo";
            this.lblCstSeqNo.Size = new System.Drawing.Size(150, 25);
            this.lblCstSeqNo.TabIndex = 5;
            this.lblCstSeqNo.Text = "Cassette Sequence No";
            this.lblCstSeqNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCstSeqNo
            // 
            this.txtCstSeqNo.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtCstSeqNo.Location = new System.Drawing.Point(150, 3);
            this.txtCstSeqNo.Name = "txtCstSeqNo";
            this.txtCstSeqNo.ReadOnly = true;
            this.txtCstSeqNo.Size = new System.Drawing.Size(253, 25);
            this.txtCstSeqNo.TabIndex = 0;
            // 
            // pnlJobSeqNo
            // 
            this.pnlJobSeqNo.Controls.Add(this.lblJobSeqNo);
            this.pnlJobSeqNo.Controls.Add(this.txtJobSeqNo);
            this.pnlJobSeqNo.Location = new System.Drawing.Point(3, 33);
            this.pnlJobSeqNo.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlJobSeqNo.Name = "pnlJobSeqNo";
            this.pnlJobSeqNo.Size = new System.Drawing.Size(420, 30);
            this.pnlJobSeqNo.TabIndex = 9;
            this.pnlJobSeqNo.Tag = "JOBSEQUENCENO";
            // 
            // lblJobSeqNo
            // 
            this.lblJobSeqNo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblJobSeqNo.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblJobSeqNo.ForeColor = System.Drawing.Color.White;
            this.lblJobSeqNo.Location = new System.Drawing.Point(0, 2);
            this.lblJobSeqNo.Name = "lblJobSeqNo";
            this.lblJobSeqNo.Size = new System.Drawing.Size(150, 25);
            this.lblJobSeqNo.TabIndex = 5;
            this.lblJobSeqNo.Text = "Job Sequence No";
            this.lblJobSeqNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtJobSeqNo
            // 
            this.txtJobSeqNo.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtJobSeqNo.Location = new System.Drawing.Point(150, 3);
            this.txtJobSeqNo.Name = "txtJobSeqNo";
            this.txtJobSeqNo.ReadOnly = true;
            this.txtJobSeqNo.Size = new System.Drawing.Size(253, 25);
            this.txtJobSeqNo.TabIndex = 0;
            // 
            // pnlGlassID
            // 
            this.pnlGlassID.Controls.Add(this.lblGlassID);
            this.pnlGlassID.Controls.Add(this.txtGlassID);
            this.pnlGlassID.Location = new System.Drawing.Point(429, 33);
            this.pnlGlassID.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlGlassID.Name = "pnlGlassID";
            this.pnlGlassID.Size = new System.Drawing.Size(420, 30);
            this.pnlGlassID.TabIndex = 22;
            this.pnlGlassID.Tag = "PPID";
            // 
            // lblGlassID
            // 
            this.lblGlassID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblGlassID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGlassID.ForeColor = System.Drawing.Color.White;
            this.lblGlassID.Location = new System.Drawing.Point(0, 2);
            this.lblGlassID.Name = "lblGlassID";
            this.lblGlassID.Size = new System.Drawing.Size(150, 25);
            this.lblGlassID.TabIndex = 5;
            this.lblGlassID.Text = "Glass ID";
            this.lblGlassID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtGlassID
            // 
            this.txtGlassID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtGlassID.Location = new System.Drawing.Point(150, 3);
            this.txtGlassID.Name = "txtGlassID";
            this.txtGlassID.ReadOnly = true;
            this.txtGlassID.Size = new System.Drawing.Size(253, 25);
            this.txtGlassID.TabIndex = 0;
            // 
            // grbNewStep
            // 
            this.grbNewStep.AutoSize = true;
            this.grbNewStep.Controls.Add(this.panel5);
            this.grbNewStep.Controls.Add(this.panel3);
            this.grbNewStep.Controls.Add(this.panel4);
            this.grbNewStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbNewStep.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.grbNewStep.Location = new System.Drawing.Point(13, 100);
            this.grbNewStep.Name = "grbNewStep";
            this.grbNewStep.Size = new System.Drawing.Size(859, 143);
            this.grbNewStep.TabIndex = 24;
            this.grbNewStep.TabStop = false;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.cboCurStepID);
            this.panel5.Controls.Add(this.lblCurStepID);
            this.panel5.Location = new System.Drawing.Point(429, 28);
            this.panel5.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(420, 30);
            this.panel5.TabIndex = 26;
            this.panel5.Tag = "JOBSEQUENCENO";
            // 
            // cboCurStepID
            // 
            this.cboCurStepID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCurStepID.FormattingEnabled = true;
            this.cboCurStepID.Location = new System.Drawing.Point(150, 3);
            this.cboCurStepID.Name = "cboCurStepID";
            this.cboCurStepID.Size = new System.Drawing.Size(253, 25);
            this.cboCurStepID.TabIndex = 7;
            // 
            // lblCurStepID
            // 
            this.lblCurStepID.BackColor = System.Drawing.Color.Blue;
            this.lblCurStepID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurStepID.ForeColor = System.Drawing.Color.White;
            this.lblCurStepID.Location = new System.Drawing.Point(0, 2);
            this.lblCurStepID.Name = "lblCurStepID";
            this.lblCurStepID.Size = new System.Drawing.Size(150, 25);
            this.lblCurStepID.TabIndex = 5;
            this.lblCurStepID.Text = "Current Step ID";
            this.lblCurStepID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel3
            // 
            this.panel3.AutoScroll = true;
            this.panel3.Controls.Add(this.cboRouteID);
            this.panel3.Controls.Add(this.lblRouteID);
            this.panel3.Location = new System.Drawing.Point(3, 28);
            this.panel3.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(420, 30);
            this.panel3.TabIndex = 24;
            this.panel3.Tag = "CASSETTESEQUENCENO";
            // 
            // cboRouteID
            // 
            this.cboRouteID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRouteID.FormattingEnabled = true;
            this.cboRouteID.Location = new System.Drawing.Point(150, 3);
            this.cboRouteID.Name = "cboRouteID";
            this.cboRouteID.Size = new System.Drawing.Size(253, 25);
            this.cboRouteID.TabIndex = 6;
            this.cboRouteID.SelectedIndexChanged += new System.EventHandler(this.cboRouteID_SelectedIndexChanged);
            // 
            // lblRouteID
            // 
            this.lblRouteID.BackColor = System.Drawing.Color.Blue;
            this.lblRouteID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRouteID.ForeColor = System.Drawing.Color.White;
            this.lblRouteID.Location = new System.Drawing.Point(0, 2);
            this.lblRouteID.Name = "lblRouteID";
            this.lblRouteID.Size = new System.Drawing.Size(150, 25);
            this.lblRouteID.TabIndex = 5;
            this.lblRouteID.Text = "Route ID";
            this.lblRouteID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.cboNextStepID);
            this.panel4.Controls.Add(this.lblNextStepID);
            this.panel4.Location = new System.Drawing.Point(3, 60);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(420, 30);
            this.panel4.TabIndex = 25;
            this.panel4.Tag = "PPID";
            // 
            // cboNextStepID
            // 
            this.cboNextStepID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNextStepID.FormattingEnabled = true;
            this.cboNextStepID.Location = new System.Drawing.Point(150, 3);
            this.cboNextStepID.Name = "cboNextStepID";
            this.cboNextStepID.Size = new System.Drawing.Size(253, 25);
            this.cboNextStepID.TabIndex = 7;
            this.cboNextStepID.SelectedIndexChanged += new System.EventHandler(this.cboNextStepID_SelectedIndexChanged);
            // 
            // lblNextStepID
            // 
            this.lblNextStepID.BackColor = System.Drawing.Color.Blue;
            this.lblNextStepID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNextStepID.ForeColor = System.Drawing.Color.White;
            this.lblNextStepID.Location = new System.Drawing.Point(0, 2);
            this.lblNextStepID.Name = "lblNextStepID";
            this.lblNextStepID.Size = new System.Drawing.Size(150, 25);
            this.lblNextStepID.TabIndex = 5;
            this.lblNextStepID.Text = "Next Step ID";
            this.lblNextStepID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvRouteStep
            // 
            this.dgvRouteStep.AllowUserToAddRows = false;
            this.dgvRouteStep.AllowUserToDeleteRows = false;
            this.dgvRouteStep.AllowUserToResizeRows = false;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvRouteStep.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvRouteStep.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRouteStep.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dgvRouteStep.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRouteStep.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStep,
            this.colRobotArm,
            this.colAction,
            this.colRouteRule,
            this.colStageNoList});
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvRouteStep.DefaultCellStyle = dataGridViewCellStyle7;
            this.dgvRouteStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRouteStep.Location = new System.Drawing.Point(13, 249);
            this.dgvRouteStep.MultiSelect = false;
            this.dgvRouteStep.Name = "dgvRouteStep";
            this.dgvRouteStep.ReadOnly = true;
            this.dgvRouteStep.RowHeadersVisible = false;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvRouteStep.RowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dgvRouteStep.RowTemplate.Height = 24;
            this.dgvRouteStep.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRouteStep.Size = new System.Drawing.Size(859, 306);
            this.dgvRouteStep.TabIndex = 25;
            // 
            // colStep
            // 
            this.colStep.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colStep.DataPropertyName = "Step";
            this.colStep.HeaderText = "Step";
            this.colStep.Name = "colStep";
            this.colStep.ReadOnly = true;
            // 
            // colRobotArm
            // 
            this.colRobotArm.DataPropertyName = "UseArm";
            this.colRobotArm.HeaderText = "Robot Arm";
            this.colRobotArm.Name = "colRobotArm";
            this.colRobotArm.ReadOnly = true;
            this.colRobotArm.Width = 130;
            // 
            // colAction
            // 
            this.colAction.DataPropertyName = "Action";
            this.colAction.HeaderText = "Action";
            this.colAction.Name = "colAction";
            this.colAction.ReadOnly = true;
            // 
            // colRouteRule
            // 
            this.colRouteRule.DataPropertyName = "RouteRule";
            this.colRouteRule.HeaderText = "Route Rule";
            this.colRouteRule.Name = "colRouteRule";
            this.colRouteRule.ReadOnly = true;
            this.colRouteRule.Width = 150;
            // 
            // colStageNoList
            // 
            this.colStageNoList.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStageNoList.DataPropertyName = "stageNoList";
            this.colStageNoList.HeaderText = "Stage No List";
            this.colStageNoList.Name = "colStageNoList";
            this.colStageNoList.ReadOnly = true;
            // 
            // btnClose
            // 
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(538, 6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 35);
            this.btnClose.TabIndex = 13;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // FormRobotWipCreate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(885, 639);
            this.Name = "FormRobotWipCreate";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormRobotWipCreate_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.tlpBase.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.flpGlassInfo.ResumeLayout(false);
            this.pnlCstSeqNo.ResumeLayout(false);
            this.pnlCstSeqNo.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlJobSeqNo.ResumeLayout(false);
            this.pnlJobSeqNo.PerformLayout();
            this.pnlGlassID.ResumeLayout(false);
            this.pnlGlassID.PerformLayout();
            this.grbNewStep.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRouteStep)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.FlowLayoutPanel flpGlassInfo;
        private System.Windows.Forms.Panel pnlGlassID;
        private System.Windows.Forms.Label lblGlassID;
        private System.Windows.Forms.TextBox txtGlassID;
        private System.Windows.Forms.Panel pnlCstSeqNo;
        private System.Windows.Forms.Label lblRobotName;
        private System.Windows.Forms.TextBox txtRobotName;
        private System.Windows.Forms.Panel pnlJobSeqNo;
        private System.Windows.Forms.Label lblJobSeqNo;
        private System.Windows.Forms.TextBox txtJobSeqNo;
        private System.Windows.Forms.DataGridView dgvRouteStep;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStep;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotArm;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAction;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRouteRule;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStageNoList;
        private System.Windows.Forms.GroupBox grbNewStep;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblRouteID;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label lblNextStepID;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label lblCurStepID;
        private System.Windows.Forms.ComboBox cboRouteID;
        private System.Windows.Forms.ComboBox cboCurStepID;
        private System.Windows.Forms.ComboBox cboNextStepID;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblCstSeqNo;
        private System.Windows.Forms.TextBox txtCstSeqNo;
        private System.Windows.Forms.Button btnClose;
    }
}