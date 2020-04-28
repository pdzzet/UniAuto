namespace UniOPI
{
    partial class FormRobotStopReason
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
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.grbJobStop = new System.Windows.Forms.GroupBox();
            this.dgvJobMessage = new System.Windows.Forms.DataGridView();
            this.colJobMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtJobMsg = new System.Windows.Forms.TextBox();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.grbRobotStop = new System.Windows.Forms.GroupBox();
            this.dgvRobotMessage = new System.Windows.Forms.DataGridView();
            this.colRobotMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtRobotMsg = new System.Windows.Forms.TextBox();
            this.flpInfo = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlRobotName = new System.Windows.Forms.Panel();
            this.lblRobotName = new System.Windows.Forms.Label();
            this.txtRobotName = new System.Windows.Forms.TextBox();
            this.pnlGlassID = new System.Windows.Forms.Panel();
            this.lblGlassID = new System.Windows.Forms.Label();
            this.txtGlassID = new System.Windows.Forms.TextBox();
            this.pnlCstSeqNo = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCstSeqNo = new System.Windows.Forms.TextBox();
            this.pnlJobSeqNo = new System.Windows.Forms.Panel();
            this.lblJobSeqNo = new System.Windows.Forms.Label();
            this.txtJobSeqNo = new System.Windows.Forms.TextBox();
            this.pnlRealStepID = new System.Windows.Forms.Panel();
            this.lblRealStepID = new System.Windows.Forms.Label();
            this.txtRealStepID = new System.Windows.Forms.TextBox();
            this.pnlRealNextStepID = new System.Windows.Forms.Panel();
            this.lblRealNextStepID = new System.Windows.Forms.Label();
            this.txtRealNextStepID = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.grbJobStop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobMessage)).BeginInit();
            this.pnlButton.SuspendLayout();
            this.grbRobotStop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRobotMessage)).BeginInit();
            this.flpInfo.SuspendLayout();
            this.pnlRobotName.SuspendLayout();
            this.pnlGlassID.SuspendLayout();
            this.pnlCstSeqNo.SuspendLayout();
            this.pnlJobSeqNo.SuspendLayout();
            this.pnlRealStepID.SuspendLayout();
            this.pnlRealNextStepID.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(830, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(890, 718);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.grbJobStop, 0, 1);
            this.tlpBase.Controls.Add(this.pnlButton, 0, 3);
            this.tlpBase.Controls.Add(this.grbRobotStop, 0, 2);
            this.tlpBase.Controls.Add(this.flpInfo, 0, 0);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 4;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tlpBase.Size = new System.Drawing.Size(890, 687);
            this.tlpBase.TabIndex = 3;
            // 
            // grbJobStop
            // 
            this.grbJobStop.Controls.Add(this.dgvJobMessage);
            this.grbJobStop.Controls.Add(this.txtJobMsg);
            this.grbJobStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbJobStop.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.grbJobStop.Location = new System.Drawing.Point(3, 93);
            this.grbJobStop.Name = "grbJobStop";
            this.grbJobStop.Size = new System.Drawing.Size(884, 270);
            this.grbJobStop.TabIndex = 5;
            this.grbJobStop.TabStop = false;
            this.grbJobStop.Text = "Job Stop Reason";
            // 
            // dgvJobMessage
            // 
            this.dgvJobMessage.AllowUserToAddRows = false;
            this.dgvJobMessage.AllowUserToResizeRows = false;
            this.dgvJobMessage.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12.75F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvJobMessage.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvJobMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvJobMessage.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colJobMessage});
            this.dgvJobMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvJobMessage.EnableHeadersVisualStyles = false;
            this.dgvJobMessage.Location = new System.Drawing.Point(3, 124);
            this.dgvJobMessage.MultiSelect = false;
            this.dgvJobMessage.Name = "dgvJobMessage";
            this.dgvJobMessage.ReadOnly = true;
            this.dgvJobMessage.RowHeadersWidth = 24;
            this.dgvJobMessage.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvJobMessage.RowTemplate.Height = 24;
            this.dgvJobMessage.Size = new System.Drawing.Size(878, 143);
            this.dgvJobMessage.TabIndex = 1;
            this.dgvJobMessage.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMessage_CellClick);
            // 
            // colJobMessage
            // 
            this.colJobMessage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11.25F);
            this.colJobMessage.DefaultCellStyle = dataGridViewCellStyle2;
            this.colJobMessage.HeaderText = "Message";
            this.colJobMessage.Name = "colJobMessage";
            this.colJobMessage.ReadOnly = true;
            this.colJobMessage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colJobMessage.Width = 1000;
            // 
            // txtJobMsg
            // 
            this.txtJobMsg.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.txtJobMsg.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtJobMsg.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtJobMsg.Location = new System.Drawing.Point(3, 24);
            this.txtJobMsg.Multiline = true;
            this.txtJobMsg.Name = "txtJobMsg";
            this.txtJobMsg.ReadOnly = true;
            this.txtJobMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtJobMsg.Size = new System.Drawing.Size(878, 100);
            this.txtJobMsg.TabIndex = 2;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnOK);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(3, 645);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(884, 39);
            this.pnlButton.TabIndex = 3;
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(430, 6);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // grbRobotStop
            // 
            this.grbRobotStop.Controls.Add(this.dgvRobotMessage);
            this.grbRobotStop.Controls.Add(this.txtRobotMsg);
            this.grbRobotStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbRobotStop.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.grbRobotStop.Location = new System.Drawing.Point(3, 369);
            this.grbRobotStop.Name = "grbRobotStop";
            this.grbRobotStop.Size = new System.Drawing.Size(884, 270);
            this.grbRobotStop.TabIndex = 4;
            this.grbRobotStop.TabStop = false;
            this.grbRobotStop.Text = "Robot Stop Reason";
            // 
            // dgvRobotMessage
            // 
            this.dgvRobotMessage.AllowUserToAddRows = false;
            this.dgvRobotMessage.AllowUserToResizeRows = false;
            this.dgvRobotMessage.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 12.75F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRobotMessage.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvRobotMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRobotMessage.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRobotMessage});
            this.dgvRobotMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRobotMessage.EnableHeadersVisualStyles = false;
            this.dgvRobotMessage.Location = new System.Drawing.Point(3, 124);
            this.dgvRobotMessage.MultiSelect = false;
            this.dgvRobotMessage.Name = "dgvRobotMessage";
            this.dgvRobotMessage.ReadOnly = true;
            this.dgvRobotMessage.RowHeadersWidth = 24;
            this.dgvRobotMessage.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvRobotMessage.RowTemplate.Height = 24;
            this.dgvRobotMessage.Size = new System.Drawing.Size(878, 143);
            this.dgvRobotMessage.TabIndex = 4;
            this.dgvRobotMessage.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRobotMessage_CellClick);
            // 
            // colRobotMessage
            // 
            this.colRobotMessage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 11.25F);
            this.colRobotMessage.DefaultCellStyle = dataGridViewCellStyle4;
            this.colRobotMessage.HeaderText = "Message";
            this.colRobotMessage.Name = "colRobotMessage";
            this.colRobotMessage.ReadOnly = true;
            this.colRobotMessage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colRobotMessage.Width = 1000;
            // 
            // txtRobotMsg
            // 
            this.txtRobotMsg.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.txtRobotMsg.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtRobotMsg.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtRobotMsg.Location = new System.Drawing.Point(3, 24);
            this.txtRobotMsg.Multiline = true;
            this.txtRobotMsg.Name = "txtRobotMsg";
            this.txtRobotMsg.ReadOnly = true;
            this.txtRobotMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRobotMsg.Size = new System.Drawing.Size(878, 100);
            this.txtRobotMsg.TabIndex = 3;
            // 
            // flpInfo
            // 
            this.flpInfo.Controls.Add(this.pnlRobotName);
            this.flpInfo.Controls.Add(this.pnlGlassID);
            this.flpInfo.Controls.Add(this.pnlCstSeqNo);
            this.flpInfo.Controls.Add(this.pnlJobSeqNo);
            this.flpInfo.Controls.Add(this.pnlRealStepID);
            this.flpInfo.Controls.Add(this.pnlRealNextStepID);
            this.flpInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpInfo.Location = new System.Drawing.Point(3, 3);
            this.flpInfo.Name = "flpInfo";
            this.flpInfo.Size = new System.Drawing.Size(884, 84);
            this.flpInfo.TabIndex = 6;
            // 
            // pnlRobotName
            // 
            this.pnlRobotName.Controls.Add(this.lblRobotName);
            this.pnlRobotName.Controls.Add(this.txtRobotName);
            this.pnlRobotName.Location = new System.Drawing.Point(3, 1);
            this.pnlRobotName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlRobotName.Name = "pnlRobotName";
            this.pnlRobotName.Size = new System.Drawing.Size(320, 25);
            this.pnlRobotName.TabIndex = 26;
            this.pnlRobotName.Tag = "JOBSEQUENCENO";
            // 
            // lblRobotName
            // 
            this.lblRobotName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblRobotName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRobotName.ForeColor = System.Drawing.Color.White;
            this.lblRobotName.Location = new System.Drawing.Point(0, 0);
            this.lblRobotName.Name = "lblRobotName";
            this.lblRobotName.Size = new System.Drawing.Size(145, 25);
            this.lblRobotName.TabIndex = 5;
            this.lblRobotName.Text = "Robot Name";
            this.lblRobotName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtRobotName
            // 
            this.txtRobotName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtRobotName.Location = new System.Drawing.Point(145, 0);
            this.txtRobotName.Name = "txtRobotName";
            this.txtRobotName.ReadOnly = true;
            this.txtRobotName.Size = new System.Drawing.Size(150, 25);
            this.txtRobotName.TabIndex = 0;
            // 
            // pnlGlassID
            // 
            this.pnlGlassID.Controls.Add(this.lblGlassID);
            this.pnlGlassID.Controls.Add(this.txtGlassID);
            this.pnlGlassID.Location = new System.Drawing.Point(329, 1);
            this.pnlGlassID.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlGlassID.Name = "pnlGlassID";
            this.pnlGlassID.Size = new System.Drawing.Size(320, 25);
            this.pnlGlassID.TabIndex = 25;
            this.pnlGlassID.Tag = "PPID";
            // 
            // lblGlassID
            // 
            this.lblGlassID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblGlassID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGlassID.ForeColor = System.Drawing.Color.White;
            this.lblGlassID.Location = new System.Drawing.Point(0, 0);
            this.lblGlassID.Name = "lblGlassID";
            this.lblGlassID.Size = new System.Drawing.Size(145, 25);
            this.lblGlassID.TabIndex = 5;
            this.lblGlassID.Text = "Glass ID";
            this.lblGlassID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtGlassID
            // 
            this.txtGlassID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtGlassID.Location = new System.Drawing.Point(145, 0);
            this.txtGlassID.Name = "txtGlassID";
            this.txtGlassID.ReadOnly = true;
            this.txtGlassID.Size = new System.Drawing.Size(150, 25);
            this.txtGlassID.TabIndex = 0;
            // 
            // pnlCstSeqNo
            // 
            this.pnlCstSeqNo.Controls.Add(this.label1);
            this.pnlCstSeqNo.Controls.Add(this.txtCstSeqNo);
            this.pnlCstSeqNo.Location = new System.Drawing.Point(3, 28);
            this.pnlCstSeqNo.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlCstSeqNo.Name = "pnlCstSeqNo";
            this.pnlCstSeqNo.Size = new System.Drawing.Size(320, 25);
            this.pnlCstSeqNo.TabIndex = 23;
            this.pnlCstSeqNo.Tag = "CASSETTESEQUENCENO";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.MediumBlue;
            this.label1.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 25);
            this.label1.TabIndex = 5;
            this.label1.Text = "Cassette Sequence No";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCstSeqNo
            // 
            this.txtCstSeqNo.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtCstSeqNo.Location = new System.Drawing.Point(145, 0);
            this.txtCstSeqNo.Name = "txtCstSeqNo";
            this.txtCstSeqNo.ReadOnly = true;
            this.txtCstSeqNo.Size = new System.Drawing.Size(150, 25);
            this.txtCstSeqNo.TabIndex = 0;
            // 
            // pnlJobSeqNo
            // 
            this.pnlJobSeqNo.Controls.Add(this.lblJobSeqNo);
            this.pnlJobSeqNo.Controls.Add(this.txtJobSeqNo);
            this.pnlJobSeqNo.Location = new System.Drawing.Point(329, 28);
            this.pnlJobSeqNo.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlJobSeqNo.Name = "pnlJobSeqNo";
            this.pnlJobSeqNo.Size = new System.Drawing.Size(320, 25);
            this.pnlJobSeqNo.TabIndex = 24;
            this.pnlJobSeqNo.Tag = "JOBSEQUENCENO";
            // 
            // lblJobSeqNo
            // 
            this.lblJobSeqNo.BackColor = System.Drawing.Color.MediumBlue;
            this.lblJobSeqNo.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblJobSeqNo.ForeColor = System.Drawing.Color.White;
            this.lblJobSeqNo.Location = new System.Drawing.Point(0, 0);
            this.lblJobSeqNo.Name = "lblJobSeqNo";
            this.lblJobSeqNo.Size = new System.Drawing.Size(145, 25);
            this.lblJobSeqNo.TabIndex = 5;
            this.lblJobSeqNo.Text = "Job Sequence No";
            this.lblJobSeqNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtJobSeqNo
            // 
            this.txtJobSeqNo.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtJobSeqNo.Location = new System.Drawing.Point(145, 0);
            this.txtJobSeqNo.Name = "txtJobSeqNo";
            this.txtJobSeqNo.ReadOnly = true;
            this.txtJobSeqNo.Size = new System.Drawing.Size(150, 25);
            this.txtJobSeqNo.TabIndex = 0;
            // 
            // pnlRealStepID
            // 
            this.pnlRealStepID.Controls.Add(this.lblRealStepID);
            this.pnlRealStepID.Controls.Add(this.txtRealStepID);
            this.pnlRealStepID.Location = new System.Drawing.Point(3, 55);
            this.pnlRealStepID.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlRealStepID.Name = "pnlRealStepID";
            this.pnlRealStepID.Size = new System.Drawing.Size(320, 25);
            this.pnlRealStepID.TabIndex = 27;
            this.pnlRealStepID.Tag = "JOBSEQUENCENO";
            // 
            // lblRealStepID
            // 
            this.lblRealStepID.BackColor = System.Drawing.Color.MediumBlue;
            this.lblRealStepID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRealStepID.ForeColor = System.Drawing.Color.White;
            this.lblRealStepID.Location = new System.Drawing.Point(0, 0);
            this.lblRealStepID.Name = "lblRealStepID";
            this.lblRealStepID.Size = new System.Drawing.Size(145, 25);
            this.lblRealStepID.TabIndex = 5;
            this.lblRealStepID.Text = "Real Time Step ID";
            this.lblRealStepID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtRealStepID
            // 
            this.txtRealStepID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtRealStepID.Location = new System.Drawing.Point(145, 0);
            this.txtRealStepID.Name = "txtRealStepID";
            this.txtRealStepID.ReadOnly = true;
            this.txtRealStepID.Size = new System.Drawing.Size(150, 25);
            this.txtRealStepID.TabIndex = 0;
            // 
            // pnlRealNextStepID
            // 
            this.pnlRealNextStepID.Controls.Add(this.lblRealNextStepID);
            this.pnlRealNextStepID.Controls.Add(this.txtRealNextStepID);
            this.pnlRealNextStepID.Location = new System.Drawing.Point(329, 55);
            this.pnlRealNextStepID.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlRealNextStepID.Name = "pnlRealNextStepID";
            this.pnlRealNextStepID.Size = new System.Drawing.Size(320, 25);
            this.pnlRealNextStepID.TabIndex = 28;
            this.pnlRealNextStepID.Tag = "JOBSEQUENCENO";
            // 
            // lblRealNextStepID
            // 
            this.lblRealNextStepID.BackColor = System.Drawing.Color.MediumBlue;
            this.lblRealNextStepID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRealNextStepID.ForeColor = System.Drawing.Color.White;
            this.lblRealNextStepID.Location = new System.Drawing.Point(0, 0);
            this.lblRealNextStepID.Name = "lblRealNextStepID";
            this.lblRealNextStepID.Size = new System.Drawing.Size(145, 25);
            this.lblRealNextStepID.TabIndex = 5;
            this.lblRealNextStepID.Text = "Real Time Next Step ID";
            this.lblRealNextStepID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtRealNextStepID
            // 
            this.txtRealNextStepID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtRealNextStepID.Location = new System.Drawing.Point(145, 0);
            this.txtRealNextStepID.Name = "txtRealNextStepID";
            this.txtRealNextStepID.ReadOnly = true;
            this.txtRealNextStepID.Size = new System.Drawing.Size(150, 25);
            this.txtRealNextStepID.TabIndex = 0;
            // 
            // FormRobotStopReason
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(890, 718);
            this.Name = "FormRobotStopReason";
            this.Text = "   ";
            this.Load += new System.EventHandler(this.FormRobotStopReason_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.grbJobStop.ResumeLayout(false);
            this.grbJobStop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobMessage)).EndInit();
            this.pnlButton.ResumeLayout(false);
            this.grbRobotStop.ResumeLayout(false);
            this.grbRobotStop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRobotMessage)).EndInit();
            this.flpInfo.ResumeLayout(false);
            this.pnlRobotName.ResumeLayout(false);
            this.pnlRobotName.PerformLayout();
            this.pnlGlassID.ResumeLayout(false);
            this.pnlGlassID.PerformLayout();
            this.pnlCstSeqNo.ResumeLayout(false);
            this.pnlCstSeqNo.PerformLayout();
            this.pnlJobSeqNo.ResumeLayout(false);
            this.pnlJobSeqNo.PerformLayout();
            this.pnlRealStepID.ResumeLayout(false);
            this.pnlRealStepID.PerformLayout();
            this.pnlRealNextStepID.ResumeLayout(false);
            this.pnlRealNextStepID.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.TextBox txtJobMsg;
        private System.Windows.Forms.DataGridView dgvJobMessage;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox grbJobStop;
        private System.Windows.Forms.GroupBox grbRobotStop;
        private System.Windows.Forms.DataGridView dgvRobotMessage;
        private System.Windows.Forms.TextBox txtRobotMsg;
        private System.Windows.Forms.FlowLayoutPanel flpInfo;
        private System.Windows.Forms.Panel pnlGlassID;
        private System.Windows.Forms.Label lblGlassID;
        private System.Windows.Forms.TextBox txtGlassID;
        private System.Windows.Forms.Panel pnlRobotName;
        private System.Windows.Forms.Label lblRobotName;
        private System.Windows.Forms.TextBox txtRobotName;
        private System.Windows.Forms.Panel pnlJobSeqNo;
        private System.Windows.Forms.Label lblJobSeqNo;
        private System.Windows.Forms.TextBox txtJobSeqNo;
        private System.Windows.Forms.Panel pnlCstSeqNo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCstSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobMessage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotMessage;
        private System.Windows.Forms.Panel pnlRealStepID;
        private System.Windows.Forms.Label lblRealStepID;
        private System.Windows.Forms.TextBox txtRealStepID;
        private System.Windows.Forms.Panel pnlRealNextStepID;
        private System.Windows.Forms.Label lblRealNextStepID;
        private System.Windows.Forms.TextBox txtRealNextStepID;
    }
}