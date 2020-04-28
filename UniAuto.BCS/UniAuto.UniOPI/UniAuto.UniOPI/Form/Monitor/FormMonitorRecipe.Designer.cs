namespace UniOPI
{
    partial class FormMonitorRecipe
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.spcRecipe = new System.Windows.Forms.SplitContainer();
            this.pnlCombox = new System.Windows.Forms.Panel();
            this.grbMESMode = new System.Windows.Forms.GroupBox();
            this.rdoOffline = new System.Windows.Forms.RadioButton();
            this.rdoOnlineLocal = new System.Windows.Forms.RadioButton();
            this.rdoOnlineRemote = new System.Windows.Forms.RadioButton();
            this.grbRecipeName = new System.Windows.Forms.GroupBox();
            this.cboRecipeName = new System.Windows.Forms.ComboBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.grbDetail = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cboLocal = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvDetail = new System.Windows.Forms.DataGridView();
            this.colParameter_LocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colParameter_RecipeNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colParameter_No = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colParameter_ParameterName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colParameter_Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colParameter_Format = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grbParameterReturn = new System.Windows.Forms.GroupBox();
            this.tlpReturn = new System.Windows.Forms.TableLayoutPanel();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colReturn_EQPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturn_LocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturn_RecipeNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturn_CIMMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturn_Return = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturn_Msg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlReturn = new System.Windows.Forms.Panel();
            this.txtTotalReturn = new System.Windows.Forms.TextBox();
            this.lblTotalReturn = new System.Windows.Forms.Label();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcRecipe)).BeginInit();
            this.spcRecipe.Panel1.SuspendLayout();
            this.spcRecipe.Panel2.SuspendLayout();
            this.spcRecipe.SuspendLayout();
            this.pnlCombox.SuspendLayout();
            this.grbMESMode.SuspendLayout();
            this.grbRecipeName.SuspendLayout();
            this.grbDetail.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetail)).BeginInit();
            this.grbParameterReturn.SuspendLayout();
            this.tlpReturn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.pnlReturn.SuspendLayout();
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
            this.spcBase.Panel2.Controls.Add(this.spcRecipe);
            this.spcBase.Size = new System.Drawing.Size(1140, 586);
            // 
            // spcRecipe
            // 
            this.spcRecipe.BackColor = System.Drawing.Color.Transparent;
            this.spcRecipe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcRecipe.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spcRecipe.Location = new System.Drawing.Point(0, 0);
            this.spcRecipe.Name = "spcRecipe";
            this.spcRecipe.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcRecipe.Panel1
            // 
            this.spcRecipe.Panel1.Controls.Add(this.pnlCombox);
            // 
            // spcRecipe.Panel2
            // 
            this.spcRecipe.Panel2.Controls.Add(this.grbDetail);
            this.spcRecipe.Panel2.Controls.Add(this.grbParameterReturn);
            this.spcRecipe.Size = new System.Drawing.Size(1140, 555);
            this.spcRecipe.SplitterDistance = 70;
            this.spcRecipe.TabIndex = 1;
            // 
            // pnlCombox
            // 
            this.pnlCombox.Controls.Add(this.grbMESMode);
            this.pnlCombox.Controls.Add(this.grbRecipeName);
            this.pnlCombox.Controls.Add(this.btnSend);
            this.pnlCombox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCombox.Location = new System.Drawing.Point(0, 0);
            this.pnlCombox.Name = "pnlCombox";
            this.pnlCombox.Size = new System.Drawing.Size(1140, 70);
            this.pnlCombox.TabIndex = 26;
            // 
            // grbMESMode
            // 
            this.grbMESMode.Controls.Add(this.rdoOffline);
            this.grbMESMode.Controls.Add(this.rdoOnlineLocal);
            this.grbMESMode.Controls.Add(this.rdoOnlineRemote);
            this.grbMESMode.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.grbMESMode.Location = new System.Drawing.Point(3, 0);
            this.grbMESMode.Name = "grbMESMode";
            this.grbMESMode.Size = new System.Drawing.Size(343, 60);
            this.grbMESMode.TabIndex = 40;
            this.grbMESMode.TabStop = false;
            this.grbMESMode.Text = "MES Mode";
            // 
            // rdoOffline
            // 
            this.rdoOffline.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.rdoOffline.Location = new System.Drawing.Point(6, 27);
            this.rdoOffline.Name = "rdoOffline";
            this.rdoOffline.Size = new System.Drawing.Size(74, 25);
            this.rdoOffline.TabIndex = 35;
            this.rdoOffline.Tag = "OFFLINE";
            this.rdoOffline.Text = "Offline";
            this.rdoOffline.UseVisualStyleBackColor = true;
            this.rdoOffline.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
            // 
            // rdoOnlineLocal
            // 
            this.rdoOnlineLocal.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.rdoOnlineLocal.Location = new System.Drawing.Point(86, 27);
            this.rdoOnlineLocal.Name = "rdoOnlineLocal";
            this.rdoOnlineLocal.Size = new System.Drawing.Size(112, 25);
            this.rdoOnlineLocal.TabIndex = 36;
            this.rdoOnlineLocal.Tag = "LOCAL";
            this.rdoOnlineLocal.Text = "Online Local";
            this.rdoOnlineLocal.UseVisualStyleBackColor = true;
            this.rdoOnlineLocal.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
            // 
            // rdoOnlineRemote
            // 
            this.rdoOnlineRemote.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.rdoOnlineRemote.Location = new System.Drawing.Point(206, 27);
            this.rdoOnlineRemote.Name = "rdoOnlineRemote";
            this.rdoOnlineRemote.Size = new System.Drawing.Size(131, 25);
            this.rdoOnlineRemote.TabIndex = 38;
            this.rdoOnlineRemote.Tag = "REMOTE";
            this.rdoOnlineRemote.Text = "Online Remote";
            this.rdoOnlineRemote.UseVisualStyleBackColor = true;
            this.rdoOnlineRemote.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
            // 
            // grbRecipeName
            // 
            this.grbRecipeName.Controls.Add(this.cboRecipeName);
            this.grbRecipeName.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.grbRecipeName.Location = new System.Drawing.Point(362, 0);
            this.grbRecipeName.Name = "grbRecipeName";
            this.grbRecipeName.Size = new System.Drawing.Size(271, 60);
            this.grbRecipeName.TabIndex = 39;
            this.grbRecipeName.TabStop = false;
            this.grbRecipeName.Text = "Recipe Name";
            // 
            // cboRecipeName
            // 
            this.cboRecipeName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboRecipeName.FormattingEnabled = true;
            this.cboRecipeName.Location = new System.Drawing.Point(16, 22);
            this.cboRecipeName.Name = "cboRecipeName";
            this.cboRecipeName.Size = new System.Drawing.Size(240, 29);
            this.cboRecipeName.TabIndex = 4;
            this.cboRecipeName.SelectedValueChanged += new System.EventHandler(this.cboRecipeName_SelectedValueChanged);
            this.cboRecipeName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cboRecipeName_KeyDown);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSend.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.Location = new System.Drawing.Point(689, 21);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(110, 29);
            this.btnSend.TabIndex = 9;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // grbDetail
            // 
            this.grbDetail.Controls.Add(this.tableLayoutPanel1);
            this.grbDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbDetail.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.grbDetail.Location = new System.Drawing.Point(640, 0);
            this.grbDetail.Name = "grbDetail";
            this.grbDetail.Size = new System.Drawing.Size(500, 481);
            this.grbDetail.TabIndex = 17;
            this.grbDetail.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dgvDetail, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(494, 454);
            this.tableLayoutPanel1.TabIndex = 15;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cboLocal);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(488, 34);
            this.panel1.TabIndex = 14;
            // 
            // cboLocal
            // 
            this.cboLocal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocal.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboLocal.FormattingEnabled = true;
            this.cboLocal.Location = new System.Drawing.Point(118, 3);
            this.cboLocal.Name = "cboLocal";
            this.cboLocal.Size = new System.Drawing.Size(252, 29);
            this.cboLocal.TabIndex = 35;
            this.cboLocal.SelectedValueChanged += new System.EventHandler(this.cboLocal_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 21);
            this.label1.TabIndex = 34;
            this.label1.Text = "Local";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dgvDetail
            // 
            this.dgvDetail.AllowUserToAddRows = false;
            this.dgvDetail.AllowUserToDeleteRows = false;
            this.dgvDetail.AllowUserToResizeRows = false;
            this.dgvDetail.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12.75F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDetail.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDetail.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDetail.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colParameter_LocalNo,
            this.colParameter_RecipeNo,
            this.colParameter_No,
            this.colParameter_ParameterName,
            this.colParameter_Value,
            this.colParameter_Format});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDetail.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDetail.Location = new System.Drawing.Point(3, 43);
            this.dgvDetail.Name = "dgvDetail";
            this.dgvDetail.ReadOnly = true;
            this.dgvDetail.RowHeadersVisible = false;
            this.dgvDetail.RowTemplate.Height = 24;
            this.dgvDetail.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDetail.Size = new System.Drawing.Size(488, 408);
            this.dgvDetail.TabIndex = 13;
            // 
            // colParameter_LocalNo
            // 
            this.colParameter_LocalNo.DataPropertyName = "EQPID";
            this.colParameter_LocalNo.HeaderText = "Local";
            this.colParameter_LocalNo.Name = "colParameter_LocalNo";
            this.colParameter_LocalNo.ReadOnly = true;
            this.colParameter_LocalNo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colParameter_LocalNo.Width = 50;
            // 
            // colParameter_RecipeNo
            // 
            this.colParameter_RecipeNo.DataPropertyName = "RecipeNo";
            this.colParameter_RecipeNo.HeaderText = "RecipeID";
            this.colParameter_RecipeNo.Name = "colParameter_RecipeNo";
            this.colParameter_RecipeNo.ReadOnly = true;
            // 
            // colParameter_No
            // 
            this.colParameter_No.DataPropertyName = "No";
            this.colParameter_No.HeaderText = "No";
            this.colParameter_No.Name = "colParameter_No";
            this.colParameter_No.ReadOnly = true;
            this.colParameter_No.Width = 50;
            // 
            // colParameter_ParameterName
            // 
            this.colParameter_ParameterName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colParameter_ParameterName.DataPropertyName = "ParameterName";
            this.colParameter_ParameterName.HeaderText = "Parameter Name";
            this.colParameter_ParameterName.MinimumWidth = 125;
            this.colParameter_ParameterName.Name = "colParameter_ParameterName";
            this.colParameter_ParameterName.ReadOnly = true;
            this.colParameter_ParameterName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colParameter_ParameterName.Width = 160;
            // 
            // colParameter_Value
            // 
            this.colParameter_Value.DataPropertyName = "Value";
            this.colParameter_Value.HeaderText = "Value";
            this.colParameter_Value.Name = "colParameter_Value";
            this.colParameter_Value.ReadOnly = true;
            this.colParameter_Value.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colParameter_Value.Width = 120;
            // 
            // colParameter_Format
            // 
            this.colParameter_Format.DataPropertyName = "Format";
            this.colParameter_Format.HeaderText = "Format";
            this.colParameter_Format.Name = "colParameter_Format";
            this.colParameter_Format.ReadOnly = true;
            this.colParameter_Format.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colParameter_Format.Width = 70;
            // 
            // grbParameterReturn
            // 
            this.grbParameterReturn.Controls.Add(this.tlpReturn);
            this.grbParameterReturn.Dock = System.Windows.Forms.DockStyle.Left;
            this.grbParameterReturn.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.grbParameterReturn.Location = new System.Drawing.Point(0, 0);
            this.grbParameterReturn.Name = "grbParameterReturn";
            this.grbParameterReturn.Size = new System.Drawing.Size(640, 481);
            this.grbParameterReturn.TabIndex = 16;
            this.grbParameterReturn.TabStop = false;
            // 
            // tlpReturn
            // 
            this.tlpReturn.ColumnCount = 1;
            this.tlpReturn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpReturn.Controls.Add(this.dgvData, 0, 1);
            this.tlpReturn.Controls.Add(this.pnlReturn, 0, 0);
            this.tlpReturn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpReturn.Location = new System.Drawing.Point(3, 24);
            this.tlpReturn.Name = "tlpReturn";
            this.tlpReturn.RowCount = 2;
            this.tlpReturn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpReturn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpReturn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpReturn.Size = new System.Drawing.Size(634, 454);
            this.tlpReturn.TabIndex = 14;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            this.dgvData.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 12.75F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colReturn_EQPID,
            this.colReturn_LocalNo,
            this.colReturn_RecipeNo,
            this.colReturn_CIMMode,
            this.colReturn_Return,
            this.colReturn_Msg});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 11.25F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 43);
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(628, 408);
            this.dgvData.TabIndex = 13;
            // 
            // colReturn_EQPID
            // 
            this.colReturn_EQPID.DataPropertyName = "EQPID";
            this.colReturn_EQPID.HeaderText = "EQPID";
            this.colReturn_EQPID.Name = "colReturn_EQPID";
            this.colReturn_EQPID.ReadOnly = true;
            this.colReturn_EQPID.Width = 90;
            // 
            // colReturn_LocalNo
            // 
            this.colReturn_LocalNo.DataPropertyName = "LocalNo";
            this.colReturn_LocalNo.HeaderText = "Local No";
            this.colReturn_LocalNo.Name = "colReturn_LocalNo";
            this.colReturn_LocalNo.ReadOnly = true;
            this.colReturn_LocalNo.Width = 95;
            // 
            // colReturn_RecipeNo
            // 
            this.colReturn_RecipeNo.DataPropertyName = "Recipe No";
            this.colReturn_RecipeNo.HeaderText = "Recipe ID";
            this.colReturn_RecipeNo.Name = "colReturn_RecipeNo";
            this.colReturn_RecipeNo.ReadOnly = true;
            this.colReturn_RecipeNo.Width = 120;
            // 
            // colReturn_CIMMode
            // 
            this.colReturn_CIMMode.DataPropertyName = "CIMMode";
            this.colReturn_CIMMode.HeaderText = "CIM Mode";
            this.colReturn_CIMMode.Name = "colReturn_CIMMode";
            this.colReturn_CIMMode.ReadOnly = true;
            this.colReturn_CIMMode.Width = 110;
            // 
            // colReturn_Return
            // 
            this.colReturn_Return.DataPropertyName = "Return";
            this.colReturn_Return.HeaderText = "Return";
            this.colReturn_Return.Name = "colReturn_Return";
            this.colReturn_Return.ReadOnly = true;
            this.colReturn_Return.Width = 90;
            // 
            // colReturn_Msg
            // 
            this.colReturn_Msg.DataPropertyName = "ReturnMsg";
            this.colReturn_Msg.HeaderText = "Retrun Message";
            this.colReturn_Msg.Name = "colReturn_Msg";
            this.colReturn_Msg.ReadOnly = true;
            this.colReturn_Msg.Width = 150;
            // 
            // pnlReturn
            // 
            this.pnlReturn.Controls.Add(this.txtTotalReturn);
            this.pnlReturn.Controls.Add(this.lblTotalReturn);
            this.pnlReturn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlReturn.Location = new System.Drawing.Point(3, 3);
            this.pnlReturn.Name = "pnlReturn";
            this.pnlReturn.Size = new System.Drawing.Size(628, 34);
            this.pnlReturn.TabIndex = 14;
            // 
            // txtTotalReturn
            // 
            this.txtTotalReturn.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtTotalReturn.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalReturn.Location = new System.Drawing.Point(113, 3);
            this.txtTotalReturn.Name = "txtTotalReturn";
            this.txtTotalReturn.Size = new System.Drawing.Size(221, 28);
            this.txtTotalReturn.TabIndex = 35;
            // 
            // lblTotalReturn
            // 
            this.lblTotalReturn.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalReturn.Location = new System.Drawing.Point(9, 6);
            this.lblTotalReturn.Name = "lblTotalReturn";
            this.lblTotalReturn.Size = new System.Drawing.Size(98, 21);
            this.lblTotalReturn.TabIndex = 34;
            this.lblTotalReturn.Text = "Total Return";
            this.lblTotalReturn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 1000;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // FormMonitorRecipe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.ClientSize = new System.Drawing.Size(1140, 586);
            this.Name = "FormMonitorRecipe";
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.spcRecipe.Panel1.ResumeLayout(false);
            this.spcRecipe.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcRecipe)).EndInit();
            this.spcRecipe.ResumeLayout(false);
            this.pnlCombox.ResumeLayout(false);
            this.grbMESMode.ResumeLayout(false);
            this.grbRecipeName.ResumeLayout(false);
            this.grbDetail.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetail)).EndInit();
            this.grbParameterReturn.ResumeLayout(false);
            this.tlpReturn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.pnlReturn.ResumeLayout(false);
            this.pnlReturn.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spcRecipe;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.GroupBox grbParameterReturn;
        private System.Windows.Forms.Panel pnlCombox;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ComboBox cboRecipeName;
        private System.Windows.Forms.GroupBox grbRecipeName;
        private System.Windows.Forms.RadioButton rdoOnlineRemote;
        private System.Windows.Forms.RadioButton rdoOnlineLocal;
        private System.Windows.Forms.RadioButton rdoOffline;
        private System.Windows.Forms.GroupBox grbMESMode;
        private System.Windows.Forms.GroupBox grbDetail;
        private System.Windows.Forms.DataGridView dgvDetail;
        private System.Windows.Forms.TableLayoutPanel tlpReturn;
        private System.Windows.Forms.Panel pnlReturn;
        private System.Windows.Forms.Label lblTotalReturn;
        private System.Windows.Forms.TextBox txtTotalReturn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboLocal;
        public System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParameter_LocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParameter_RecipeNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParameter_No;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParameter_ParameterName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParameter_Value;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParameter_Format;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_EQPID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_LocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_RecipeNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_CIMMode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_Return;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_Msg;
    }
}