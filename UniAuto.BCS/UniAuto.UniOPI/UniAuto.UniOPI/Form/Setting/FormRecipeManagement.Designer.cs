namespace UniOPI
{
    partial class FormRecipeManagement
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
            this.tlpRecipe = new System.Windows.Forms.TableLayoutPanel();
            this.pnlRecipeQuery = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.rdoOnlineRemote = new System.Windows.Forms.RadioButton();
            this.lblMESMode = new System.Windows.Forms.Label();
            this.rdoOnlineLocal = new System.Windows.Forms.RadioButton();
            this.rdoOffline = new System.Windows.Forms.RadioButton();
            this.dgvRecipeNo = new System.Windows.Forms.DataGridView();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecipeNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCIMMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvRecipe = new System.Windows.Forms.DataGridView();
            this.colRegister = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colParameter = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colObjectKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMesMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecipeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colREMARK = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUpdateUserId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUpdateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUpdateIP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlNormalBtn = new System.Windows.Forms.Panel();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnParameterCheck = new System.Windows.Forms.Button();
            this.btnRecipeCheck = new System.Windows.Forms.Button();
            this.btnSync = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnModify = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpRecipe.SuspendLayout();
            this.pnlRecipeQuery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipeNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipe)).BeginInit();
            this.pnlNormalBtn.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1074, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpRecipe);
            this.spcBase.Size = new System.Drawing.Size(1134, 626);
            // 
            // tlpRecipe
            // 
            this.tlpRecipe.ColumnCount = 3;
            this.tlpRecipe.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 753F));
            this.tlpRecipe.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRecipe.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 91F));
            this.tlpRecipe.Controls.Add(this.pnlRecipeQuery, 0, 0);
            this.tlpRecipe.Controls.Add(this.dgvRecipeNo, 1, 1);
            this.tlpRecipe.Controls.Add(this.dgvRecipe, 0, 1);
            this.tlpRecipe.Controls.Add(this.pnlNormalBtn, 2, 1);
            this.tlpRecipe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRecipe.Location = new System.Drawing.Point(0, 0);
            this.tlpRecipe.Name = "tlpRecipe";
            this.tlpRecipe.RowCount = 2;
            this.tlpRecipe.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tlpRecipe.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRecipe.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRecipe.Size = new System.Drawing.Size(1134, 595);
            this.tlpRecipe.TabIndex = 14;
            // 
            // pnlRecipeQuery
            // 
            this.pnlRecipeQuery.Controls.Add(this.textBox1);
            this.pnlRecipeQuery.Controls.Add(this.btnRefresh);
            this.pnlRecipeQuery.Controls.Add(this.rdoOnlineRemote);
            this.pnlRecipeQuery.Controls.Add(this.lblMESMode);
            this.pnlRecipeQuery.Controls.Add(this.rdoOnlineLocal);
            this.pnlRecipeQuery.Controls.Add(this.rdoOffline);
            this.pnlRecipeQuery.Location = new System.Drawing.Point(3, 3);
            this.pnlRecipeQuery.Name = "pnlRecipeQuery";
            this.pnlRecipeQuery.Size = new System.Drawing.Size(695, 36);
            this.pnlRecipeQuery.TabIndex = 0;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(612, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(80, 30);
            this.btnRefresh.TabIndex = 15;
            this.btnRefresh.Tag = "Refresh";
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btn_Click);
            // 
            // rdoOnlineRemote
            // 
            this.rdoOnlineRemote.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.rdoOnlineRemote.Location = new System.Drawing.Point(308, 3);
            this.rdoOnlineRemote.Name = "rdoOnlineRemote";
            this.rdoOnlineRemote.Size = new System.Drawing.Size(131, 25);
            this.rdoOnlineRemote.TabIndex = 34;
            this.rdoOnlineRemote.Tag = "REMOTE";
            this.rdoOnlineRemote.Text = "Online Remote";
            this.rdoOnlineRemote.UseVisualStyleBackColor = true;
            this.rdoOnlineRemote.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
            // 
            // lblMESMode
            // 
            this.lblMESMode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMESMode.Location = new System.Drawing.Point(6, 6);
            this.lblMESMode.Name = "lblMESMode";
            this.lblMESMode.Size = new System.Drawing.Size(105, 21);
            this.lblMESMode.TabIndex = 33;
            this.lblMESMode.Text = "MES Mode";
            this.lblMESMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // rdoOnlineLocal
            // 
            this.rdoOnlineLocal.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.rdoOnlineLocal.Location = new System.Drawing.Point(193, 4);
            this.rdoOnlineLocal.Name = "rdoOnlineLocal";
            this.rdoOnlineLocal.Size = new System.Drawing.Size(112, 25);
            this.rdoOnlineLocal.TabIndex = 32;
            this.rdoOnlineLocal.Tag = "LOCAL";
            this.rdoOnlineLocal.Text = "Online Local";
            this.rdoOnlineLocal.UseVisualStyleBackColor = true;
            this.rdoOnlineLocal.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
            // 
            // rdoOffline
            // 
            this.rdoOffline.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.rdoOffline.Location = new System.Drawing.Point(116, 4);
            this.rdoOffline.Name = "rdoOffline";
            this.rdoOffline.Size = new System.Drawing.Size(74, 25);
            this.rdoOffline.TabIndex = 31;
            this.rdoOffline.Tag = "OFFLINE";
            this.rdoOffline.Text = "Offline";
            this.rdoOffline.UseVisualStyleBackColor = true;
            this.rdoOffline.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
            // 
            // dgvRecipeNo
            // 
            this.dgvRecipeNo.AllowUserToAddRows = false;
            this.dgvRecipeNo.AllowUserToDeleteRows = false;
            this.dgvRecipeNo.AllowUserToResizeRows = false;
            this.dgvRecipeNo.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvRecipeNo.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRecipeNo.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvRecipeNo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecipeNo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLocalNo,
            this.colRecipeNo,
            this.colCIMMode});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvRecipeNo.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvRecipeNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRecipeNo.Location = new System.Drawing.Point(756, 45);
            this.dgvRecipeNo.Name = "dgvRecipeNo";
            this.dgvRecipeNo.ReadOnly = true;
            this.dgvRecipeNo.RowHeadersVisible = false;
            this.dgvRecipeNo.RowTemplate.Height = 24;
            this.dgvRecipeNo.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecipeNo.Size = new System.Drawing.Size(284, 547);
            this.dgvRecipeNo.TabIndex = 25;
            // 
            // colLocalNo
            // 
            this.colLocalNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colLocalNo.HeaderText = "Local No";
            this.colLocalNo.Name = "colLocalNo";
            this.colLocalNo.ReadOnly = true;
            this.colLocalNo.Width = 90;
            // 
            // colRecipeNo
            // 
            this.colRecipeNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colRecipeNo.HeaderText = "Recipe ID";
            this.colRecipeNo.Name = "colRecipeNo";
            this.colRecipeNo.ReadOnly = true;
            // 
            // colCIMMode
            // 
            this.colCIMMode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colCIMMode.HeaderText = "CIMMode";
            this.colCIMMode.Name = "colCIMMode";
            this.colCIMMode.ReadOnly = true;
            this.colCIMMode.Width = 70;
            // 
            // dgvRecipe
            // 
            this.dgvRecipe.AllowUserToAddRows = false;
            this.dgvRecipe.AllowUserToDeleteRows = false;
            this.dgvRecipe.AllowUserToResizeRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvRecipe.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvRecipe.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRecipe.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvRecipe.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecipe.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRegister,
            this.colParameter,
            this.colObjectKey,
            this.colMesMode,
            this.colRecipeName,
            this.colPPID,
            this.colREMARK,
            this.colUpdateUserId,
            this.colUpdateTime,
            this.colUpdateIP});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvRecipe.DefaultCellStyle = dataGridViewCellStyle5;
            this.dgvRecipe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRecipe.Location = new System.Drawing.Point(3, 45);
            this.dgvRecipe.Name = "dgvRecipe";
            this.dgvRecipe.ReadOnly = true;
            this.dgvRecipe.RowHeadersVisible = false;
            this.dgvRecipe.RowTemplate.Height = 24;
            this.dgvRecipe.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecipe.Size = new System.Drawing.Size(747, 547);
            this.dgvRecipe.TabIndex = 12;
            this.dgvRecipe.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRecipe_CellClick);
            // 
            // colRegister
            // 
            this.colRegister.HeaderText = "Register";
            this.colRegister.Name = "colRegister";
            this.colRegister.ReadOnly = true;
            this.colRegister.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colRegister.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // colParameter
            // 
            this.colParameter.HeaderText = "Parameter";
            this.colParameter.Name = "colParameter";
            this.colParameter.ReadOnly = true;
            this.colParameter.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colParameter.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // colObjectKey
            // 
            this.colObjectKey.HeaderText = "ObjectKey";
            this.colObjectKey.Name = "colObjectKey";
            this.colObjectKey.ReadOnly = true;
            this.colObjectKey.Visible = false;
            // 
            // colMesMode
            // 
            this.colMesMode.HeaderText = "MESMode";
            this.colMesMode.Name = "colMesMode";
            this.colMesMode.ReadOnly = true;
            this.colMesMode.Width = 80;
            // 
            // colRecipeName
            // 
            this.colRecipeName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colRecipeName.HeaderText = "Line Recipe Name";
            this.colRecipeName.Name = "colRecipeName";
            this.colRecipeName.ReadOnly = true;
            this.colRecipeName.Width = 150;
            // 
            // colPPID
            // 
            this.colPPID.HeaderText = "PPID";
            this.colPPID.Name = "colPPID";
            this.colPPID.ReadOnly = true;
            this.colPPID.Width = 300;
            // 
            // colREMARK
            // 
            this.colREMARK.HeaderText = "Remark";
            this.colREMARK.Name = "colREMARK";
            this.colREMARK.ReadOnly = true;
            this.colREMARK.Width = 500;
            // 
            // colUpdateUserId
            // 
            this.colUpdateUserId.HeaderText = "Update User";
            this.colUpdateUserId.Name = "colUpdateUserId";
            this.colUpdateUserId.ReadOnly = true;
            this.colUpdateUserId.Visible = false;
            this.colUpdateUserId.Width = 115;
            // 
            // colUpdateTime
            // 
            this.colUpdateTime.HeaderText = "Update Time";
            this.colUpdateTime.Name = "colUpdateTime";
            this.colUpdateTime.ReadOnly = true;
            this.colUpdateTime.Visible = false;
            this.colUpdateTime.Width = 185;
            // 
            // colUpdateIP
            // 
            this.colUpdateIP.HeaderText = "Update IP";
            this.colUpdateIP.Name = "colUpdateIP";
            this.colUpdateIP.ReadOnly = true;
            this.colUpdateIP.Visible = false;
            this.colUpdateIP.Width = 120;
            // 
            // pnlNormalBtn
            // 
            this.pnlNormalBtn.Controls.Add(this.btnExport);
            this.pnlNormalBtn.Controls.Add(this.btnParameterCheck);
            this.pnlNormalBtn.Controls.Add(this.btnRecipeCheck);
            this.pnlNormalBtn.Controls.Add(this.btnSync);
            this.pnlNormalBtn.Controls.Add(this.btnSave);
            this.pnlNormalBtn.Controls.Add(this.btnAdd);
            this.pnlNormalBtn.Controls.Add(this.btnDelete);
            this.pnlNormalBtn.Controls.Add(this.btnModify);
            this.pnlNormalBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNormalBtn.Location = new System.Drawing.Point(1046, 45);
            this.pnlNormalBtn.Name = "pnlNormalBtn";
            this.pnlNormalBtn.Size = new System.Drawing.Size(85, 547);
            this.pnlNormalBtn.TabIndex = 14;
            // 
            // btnExport
            // 
            this.btnExport.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnExport.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExport.Location = new System.Drawing.Point(5, 396);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(80, 50);
            this.btnExport.TabIndex = 16;
            this.btnExport.Tag = "Export";
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnParameterCheck
            // 
            this.btnParameterCheck.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnParameterCheck.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnParameterCheck.Location = new System.Drawing.Point(5, 245);
            this.btnParameterCheck.Name = "btnParameterCheck";
            this.btnParameterCheck.Size = new System.Drawing.Size(80, 50);
            this.btnParameterCheck.TabIndex = 15;
            this.btnParameterCheck.Tag = "Parameter Enable";
            this.btnParameterCheck.Text = "Param Enable";
            this.btnParameterCheck.UseVisualStyleBackColor = true;
            this.btnParameterCheck.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnRecipeCheck
            // 
            this.btnRecipeCheck.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRecipeCheck.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRecipeCheck.Location = new System.Drawing.Point(5, 295);
            this.btnRecipeCheck.Name = "btnRecipeCheck";
            this.btnRecipeCheck.Size = new System.Drawing.Size(80, 50);
            this.btnRecipeCheck.TabIndex = 14;
            this.btnRecipeCheck.Tag = "Recipe Check";
            this.btnRecipeCheck.Text = "Recipe Check";
            this.btnRecipeCheck.UseVisualStyleBackColor = true;
            this.btnRecipeCheck.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnSync
            // 
            this.btnSync.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSync.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSync.Location = new System.Drawing.Point(5, 345);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(80, 50);
            this.btnSync.TabIndex = 14;
            this.btnSync.Tag = "Sync";
            this.btnSync.Text = "Sync";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(3, 153);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 50);
            this.btnSave.TabIndex = 13;
            this.btnSave.Tag = "Save";
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnAdd.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(2, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(80, 50);
            this.btnAdd.TabIndex = 8;
            this.btnAdd.Tag = "Add";
            this.btnAdd.Text = "Add";
            this.btnAdd.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnDelete.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(3, 103);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(80, 50);
            this.btnDelete.TabIndex = 9;
            this.btnDelete.Tag = "Delete";
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnModify
            // 
            this.btnModify.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnModify.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModify.Location = new System.Drawing.Point(3, 53);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(80, 50);
            this.btnModify.TabIndex = 10;
            this.btnModify.Tag = "Modify";
            this.btnModify.Text = "Modify";
            this.btnModify.UseVisualStyleBackColor = true;
            this.btnModify.Click += new System.EventHandler(this.btn_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(474, 6);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 21);
            this.textBox1.TabIndex = 35;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // FormRecipeManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1134, 626);
            this.Name = "FormRecipeManagement";
            this.Text = "FormRecipeManagement";
            this.Load += new System.EventHandler(this.FormRecipeManagement_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpRecipe.ResumeLayout(false);
            this.pnlRecipeQuery.ResumeLayout(false);
            this.pnlRecipeQuery.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipeNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipe)).EndInit();
            this.pnlNormalBtn.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvRecipe;
        private System.Windows.Forms.TableLayoutPanel tlpRecipe;
        private System.Windows.Forms.Panel pnlRecipeQuery;
        private System.Windows.Forms.RadioButton rdoOnlineRemote;
        private System.Windows.Forms.Label lblMESMode;
        private System.Windows.Forms.RadioButton rdoOnlineLocal;
        private System.Windows.Forms.RadioButton rdoOffline;
        private System.Windows.Forms.DataGridView dgvRecipeNo;
        private System.Windows.Forms.Panel pnlNormalBtn;
        private System.Windows.Forms.Button btnRecipeCheck;
        private System.Windows.Forms.Button btnSync;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnModify;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnParameterCheck;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecipeNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCIMMode;
        private System.Windows.Forms.DataGridViewButtonColumn colRegister;
        private System.Windows.Forms.DataGridViewButtonColumn colParameter;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMesMode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecipeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPPID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colREMARK;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUpdateUserId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUpdateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUpdateIP;
        private System.Windows.Forms.TextBox textBox1;
    }
}