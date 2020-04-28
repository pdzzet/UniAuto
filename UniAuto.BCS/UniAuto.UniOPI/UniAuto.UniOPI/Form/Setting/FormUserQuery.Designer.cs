namespace UniOPI
{
    partial class FormUserQuery
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
            this.tlpUseQery = new System.Windows.Forms.TableLayoutPanel();
            this.dgvUserList = new System.Windows.Forms.DataGridView();
            this.colLogging = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colLogout = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dgvtxtGROUP_ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtUSER_ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtCLIENT_KEY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtUSER_NAME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtPASSWORD = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtEmail = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtLocalName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtDefaultFactoryName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtDefaultAreaName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvchkUACActive = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dgvchkACTIVE = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dgvtxtTrxDatetime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtTrxUserID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtAddDatetime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtAddUserID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlClientKey = new System.Windows.Forms.Panel();
            this.txtClientKey = new System.Windows.Forms.TextBox();
            this.lblClientKey = new System.Windows.Forms.Label();
            this.pnlPassword = new System.Windows.Forms.Panel();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.pnlActive = new System.Windows.Forms.Panel();
            this.txtActive = new System.Windows.Forms.TextBox();
            this.lblActive = new System.Windows.Forms.Label();
            this.pnlUserID = new System.Windows.Forms.Panel();
            this.txtUserID = new System.Windows.Forms.TextBox();
            this.lblUserID = new System.Windows.Forms.Label();
            this.pnlEmail = new System.Windows.Forms.Panel();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.pnlLoaclName = new System.Windows.Forms.Panel();
            this.txtLocalName = new System.Windows.Forms.TextBox();
            this.lblLocalName = new System.Windows.Forms.Label();
            this.pnlDefaultFactoryName = new System.Windows.Forms.Panel();
            this.txtDefaultFactoryName = new System.Windows.Forms.TextBox();
            this.lblDefaultFactoryName = new System.Windows.Forms.Label();
            this.pnlTrxDataTime = new System.Windows.Forms.Panel();
            this.txtDateTime = new System.Windows.Forms.TextBox();
            this.lblTrxDataTime = new System.Windows.Forms.Label();
            this.pnlAddDateTime = new System.Windows.Forms.Panel();
            this.txtAddDateTime = new System.Windows.Forms.TextBox();
            this.lblAddDateTime = new System.Windows.Forms.Label();
            this.pnlAddUserID = new System.Windows.Forms.Panel();
            this.txtAddUserID = new System.Windows.Forms.TextBox();
            this.lblAddUserID = new System.Windows.Forms.Label();
            this.btnQuery = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpUseQery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserList)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.pnlClientKey.SuspendLayout();
            this.pnlPassword.SuspendLayout();
            this.pnlActive.SuspendLayout();
            this.pnlUserID.SuspendLayout();
            this.pnlEmail.SuspendLayout();
            this.pnlLoaclName.SuspendLayout();
            this.pnlDefaultFactoryName.SuspendLayout();
            this.pnlTrxDataTime.SuspendLayout();
            this.pnlAddDateTime.SuspendLayout();
            this.pnlAddUserID.SuspendLayout();
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
            this.spcBase.Panel2.Controls.Add(this.tlpUseQery);
            this.spcBase.Size = new System.Drawing.Size(1260, 522);
            // 
            // tlpUseQery
            // 
            this.tlpUseQery.ColumnCount = 2;
            this.tlpUseQery.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 349F));
            this.tlpUseQery.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpUseQery.Controls.Add(this.dgvUserList, 0, 0);
            this.tlpUseQery.Controls.Add(this.tableLayoutPanel1, 1, 0);
            this.tlpUseQery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpUseQery.Location = new System.Drawing.Point(0, 0);
            this.tlpUseQery.Name = "tlpUseQery";
            this.tlpUseQery.RowCount = 1;
            this.tlpUseQery.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpUseQery.Size = new System.Drawing.Size(1260, 491);
            this.tlpUseQery.TabIndex = 0;
            // 
            // dgvUserList
            // 
            this.dgvUserList.AllowUserToAddRows = false;
            this.dgvUserList.AllowUserToDeleteRows = false;
            this.dgvUserList.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvUserList.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvUserList.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvUserList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvUserList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUserList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLogging,
            this.colLogout,
            this.dgvtxtGROUP_ID,
            this.dgvtxtUSER_ID,
            this.dgvtxtCLIENT_KEY,
            this.dgvtxtUSER_NAME,
            this.dgvtxtPASSWORD,
            this.dgvtxtEmail,
            this.dgvtxtLocalName,
            this.dgvtxtDefaultFactoryName,
            this.dgvtxtDefaultAreaName,
            this.dgvchkUACActive,
            this.dgvchkACTIVE,
            this.dgvtxtTrxDatetime,
            this.dgvtxtTrxUserID,
            this.dgvtxtAddDatetime,
            this.dgvtxtAddUserID});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUserList.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvUserList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUserList.Location = new System.Drawing.Point(3, 3);
            this.dgvUserList.Name = "dgvUserList";
            this.dgvUserList.ReadOnly = true;
            this.dgvUserList.RowHeadersVisible = false;
            this.dgvUserList.RowTemplate.Height = 24;
            this.dgvUserList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUserList.Size = new System.Drawing.Size(343, 485);
            this.dgvUserList.TabIndex = 23;
            this.dgvUserList.DataSourceChanged += new System.EventHandler(this.dgvUserList_DataSourceChanged);
            this.dgvUserList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUserList_CellClick);
            // 
            // colLogging
            // 
            this.colLogging.DataPropertyName = "LOGGING";
            this.colLogging.FalseValue = "N";
            this.colLogging.HeaderText = "Logging";
            this.colLogging.Name = "colLogging";
            this.colLogging.ReadOnly = true;
            this.colLogging.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colLogging.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colLogging.TrueValue = "Y";
            this.colLogging.Visible = false;
            // 
            // colLogout
            // 
            this.colLogout.DataPropertyName = "LOGOUT";
            this.colLogout.FalseValue = "N";
            this.colLogout.HeaderText = "Logout";
            this.colLogout.Name = "colLogout";
            this.colLogout.ReadOnly = true;
            this.colLogout.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colLogout.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colLogout.TrueValue = "Y";
            this.colLogout.Visible = false;
            // 
            // dgvtxtGROUP_ID
            // 
            this.dgvtxtGROUP_ID.DataPropertyName = "GROUP_ID";
            this.dgvtxtGROUP_ID.HeaderText = "Group ID";
            this.dgvtxtGROUP_ID.Name = "dgvtxtGROUP_ID";
            this.dgvtxtGROUP_ID.ReadOnly = true;
            // 
            // dgvtxtUSER_ID
            // 
            this.dgvtxtUSER_ID.DataPropertyName = "USER_ID";
            this.dgvtxtUSER_ID.HeaderText = "User ID";
            this.dgvtxtUSER_ID.Name = "dgvtxtUSER_ID";
            this.dgvtxtUSER_ID.ReadOnly = true;
            this.dgvtxtUSER_ID.Width = 120;
            // 
            // dgvtxtCLIENT_KEY
            // 
            this.dgvtxtCLIENT_KEY.DataPropertyName = "CLIENT_KEY";
            this.dgvtxtCLIENT_KEY.HeaderText = "CLIENT_KEY";
            this.dgvtxtCLIENT_KEY.Name = "dgvtxtCLIENT_KEY";
            this.dgvtxtCLIENT_KEY.ReadOnly = true;
            this.dgvtxtCLIENT_KEY.Visible = false;
            // 
            // dgvtxtUSER_NAME
            // 
            this.dgvtxtUSER_NAME.DataPropertyName = "USER_NAME";
            this.dgvtxtUSER_NAME.HeaderText = "User Name";
            this.dgvtxtUSER_NAME.Name = "dgvtxtUSER_NAME";
            this.dgvtxtUSER_NAME.ReadOnly = true;
            this.dgvtxtUSER_NAME.Width = 120;
            // 
            // dgvtxtPASSWORD
            // 
            this.dgvtxtPASSWORD.DataPropertyName = "PASSWORD";
            this.dgvtxtPASSWORD.HeaderText = "PASSWORD";
            this.dgvtxtPASSWORD.Name = "dgvtxtPASSWORD";
            this.dgvtxtPASSWORD.ReadOnly = true;
            this.dgvtxtPASSWORD.Visible = false;
            // 
            // dgvtxtEmail
            // 
            this.dgvtxtEmail.DataPropertyName = "E_MAIL";
            this.dgvtxtEmail.HeaderText = "E-Mail";
            this.dgvtxtEmail.Name = "dgvtxtEmail";
            this.dgvtxtEmail.ReadOnly = true;
            this.dgvtxtEmail.Visible = false;
            this.dgvtxtEmail.Width = 200;
            // 
            // dgvtxtLocalName
            // 
            this.dgvtxtLocalName.DataPropertyName = "LOCAL_NAME";
            this.dgvtxtLocalName.HeaderText = "Local Name";
            this.dgvtxtLocalName.Name = "dgvtxtLocalName";
            this.dgvtxtLocalName.ReadOnly = true;
            this.dgvtxtLocalName.Visible = false;
            this.dgvtxtLocalName.Width = 120;
            // 
            // dgvtxtDefaultFactoryName
            // 
            this.dgvtxtDefaultFactoryName.DataPropertyName = "DEFAULT_FACTORY_NAME";
            this.dgvtxtDefaultFactoryName.HeaderText = "Default Factory Name";
            this.dgvtxtDefaultFactoryName.Name = "dgvtxtDefaultFactoryName";
            this.dgvtxtDefaultFactoryName.ReadOnly = true;
            this.dgvtxtDefaultFactoryName.Visible = false;
            this.dgvtxtDefaultFactoryName.Width = 200;
            // 
            // dgvtxtDefaultAreaName
            // 
            this.dgvtxtDefaultAreaName.DataPropertyName = "DEFAULT_AREA_NAME";
            this.dgvtxtDefaultAreaName.HeaderText = "Default Area Name";
            this.dgvtxtDefaultAreaName.Name = "dgvtxtDefaultAreaName";
            this.dgvtxtDefaultAreaName.ReadOnly = true;
            this.dgvtxtDefaultAreaName.Visible = false;
            this.dgvtxtDefaultAreaName.Width = 180;
            // 
            // dgvchkUACActive
            // 
            this.dgvchkUACActive.DataPropertyName = "UACACTIVE";
            this.dgvchkUACActive.FalseValue = "N";
            this.dgvchkUACActive.HeaderText = "UAC Active";
            this.dgvchkUACActive.Name = "dgvchkUACActive";
            this.dgvchkUACActive.ReadOnly = true;
            this.dgvchkUACActive.TrueValue = "Y";
            this.dgvchkUACActive.Visible = false;
            // 
            // dgvchkACTIVE
            // 
            this.dgvchkACTIVE.DataPropertyName = "ACTIVE";
            this.dgvchkACTIVE.FalseValue = "N";
            this.dgvchkACTIVE.HeaderText = "Active";
            this.dgvchkACTIVE.Name = "dgvchkACTIVE";
            this.dgvchkACTIVE.ReadOnly = true;
            this.dgvchkACTIVE.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvchkACTIVE.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dgvchkACTIVE.TrueValue = "Y";
            this.dgvchkACTIVE.Visible = false;
            this.dgvchkACTIVE.Width = 60;
            // 
            // dgvtxtTrxDatetime
            // 
            this.dgvtxtTrxDatetime.DataPropertyName = "TRX_DATETIME";
            this.dgvtxtTrxDatetime.HeaderText = "Trx Datetime";
            this.dgvtxtTrxDatetime.Name = "dgvtxtTrxDatetime";
            this.dgvtxtTrxDatetime.ReadOnly = true;
            this.dgvtxtTrxDatetime.Visible = false;
            this.dgvtxtTrxDatetime.Width = 130;
            // 
            // dgvtxtTrxUserID
            // 
            this.dgvtxtTrxUserID.DataPropertyName = "TRX_USER_ID";
            this.dgvtxtTrxUserID.HeaderText = "Trx User ID";
            this.dgvtxtTrxUserID.Name = "dgvtxtTrxUserID";
            this.dgvtxtTrxUserID.ReadOnly = true;
            this.dgvtxtTrxUserID.Visible = false;
            this.dgvtxtTrxUserID.Width = 130;
            // 
            // dgvtxtAddDatetime
            // 
            this.dgvtxtAddDatetime.DataPropertyName = "ADD_DATETIME";
            this.dgvtxtAddDatetime.HeaderText = "Insert Datetime";
            this.dgvtxtAddDatetime.Name = "dgvtxtAddDatetime";
            this.dgvtxtAddDatetime.ReadOnly = true;
            this.dgvtxtAddDatetime.Visible = false;
            this.dgvtxtAddDatetime.Width = 140;
            // 
            // dgvtxtAddUserID
            // 
            this.dgvtxtAddUserID.DataPropertyName = "ADD_USER_ID";
            this.dgvtxtAddUserID.HeaderText = "Insert User ID";
            this.dgvtxtAddUserID.Name = "dgvtxtAddUserID";
            this.dgvtxtAddUserID.ReadOnly = true;
            this.dgvtxtAddUserID.Visible = false;
            this.dgvtxtAddUserID.Width = 130;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnQuery, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(352, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(905, 485);
            this.tableLayoutPanel1.TabIndex = 24;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.pnlClientKey);
            this.flowLayoutPanel1.Controls.Add(this.pnlPassword);
            this.flowLayoutPanel1.Controls.Add(this.pnlActive);
            this.flowLayoutPanel1.Controls.Add(this.pnlUserID);
            this.flowLayoutPanel1.Controls.Add(this.pnlEmail);
            this.flowLayoutPanel1.Controls.Add(this.pnlLoaclName);
            this.flowLayoutPanel1.Controls.Add(this.pnlDefaultFactoryName);
            this.flowLayoutPanel1.Controls.Add(this.pnlTrxDataTime);
            this.flowLayoutPanel1.Controls.Add(this.pnlAddDateTime);
            this.flowLayoutPanel1.Controls.Add(this.pnlAddUserID);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(902, 433);
            this.flowLayoutPanel1.TabIndex = 25;
            // 
            // pnlClientKey
            // 
            this.pnlClientKey.Controls.Add(this.txtClientKey);
            this.pnlClientKey.Controls.Add(this.lblClientKey);
            this.pnlClientKey.Location = new System.Drawing.Point(3, 3);
            this.pnlClientKey.Name = "pnlClientKey";
            this.pnlClientKey.Size = new System.Drawing.Size(385, 29);
            this.pnlClientKey.TabIndex = 0;
            // 
            // txtClientKey
            // 
            this.txtClientKey.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtClientKey.Location = new System.Drawing.Point(183, 0);
            this.txtClientKey.MaxLength = 5;
            this.txtClientKey.Name = "txtClientKey";
            this.txtClientKey.ReadOnly = true;
            this.txtClientKey.Size = new System.Drawing.Size(199, 28);
            this.txtClientKey.TabIndex = 48;
            // 
            // lblClientKey
            // 
            this.lblClientKey.BackColor = System.Drawing.Color.Black;
            this.lblClientKey.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblClientKey.ForeColor = System.Drawing.Color.White;
            this.lblClientKey.Location = new System.Drawing.Point(3, 0);
            this.lblClientKey.Name = "lblClientKey";
            this.lblClientKey.Size = new System.Drawing.Size(180, 26);
            this.lblClientKey.TabIndex = 49;
            this.lblClientKey.Text = "Client Key";
            this.lblClientKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlPassword
            // 
            this.pnlPassword.Controls.Add(this.txtPassword);
            this.pnlPassword.Controls.Add(this.lblPassword);
            this.pnlPassword.Location = new System.Drawing.Point(394, 3);
            this.pnlPassword.Name = "pnlPassword";
            this.pnlPassword.Size = new System.Drawing.Size(385, 29);
            this.pnlPassword.TabIndex = 1;
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtPassword.Location = new System.Drawing.Point(183, 0);
            this.txtPassword.MaxLength = 5;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.ReadOnly = true;
            this.txtPassword.Size = new System.Drawing.Size(199, 28);
            this.txtPassword.TabIndex = 48;
            // 
            // lblPassword
            // 
            this.lblPassword.BackColor = System.Drawing.Color.Black;
            this.lblPassword.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblPassword.ForeColor = System.Drawing.Color.White;
            this.lblPassword.Location = new System.Drawing.Point(3, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(180, 26);
            this.lblPassword.TabIndex = 49;
            this.lblPassword.Text = "Password";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlActive
            // 
            this.pnlActive.Controls.Add(this.txtActive);
            this.pnlActive.Controls.Add(this.lblActive);
            this.pnlActive.Location = new System.Drawing.Point(3, 38);
            this.pnlActive.Name = "pnlActive";
            this.pnlActive.Size = new System.Drawing.Size(385, 29);
            this.pnlActive.TabIndex = 2;
            // 
            // txtActive
            // 
            this.txtActive.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtActive.Location = new System.Drawing.Point(183, 0);
            this.txtActive.MaxLength = 5;
            this.txtActive.Name = "txtActive";
            this.txtActive.ReadOnly = true;
            this.txtActive.Size = new System.Drawing.Size(199, 28);
            this.txtActive.TabIndex = 48;
            // 
            // lblActive
            // 
            this.lblActive.BackColor = System.Drawing.Color.Black;
            this.lblActive.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblActive.ForeColor = System.Drawing.Color.White;
            this.lblActive.Location = new System.Drawing.Point(3, 0);
            this.lblActive.Name = "lblActive";
            this.lblActive.Size = new System.Drawing.Size(180, 26);
            this.lblActive.TabIndex = 49;
            this.lblActive.Text = "Active";
            this.lblActive.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlUserID
            // 
            this.pnlUserID.Controls.Add(this.txtUserID);
            this.pnlUserID.Controls.Add(this.lblUserID);
            this.pnlUserID.Location = new System.Drawing.Point(394, 38);
            this.pnlUserID.Name = "pnlUserID";
            this.pnlUserID.Size = new System.Drawing.Size(385, 29);
            this.pnlUserID.TabIndex = 7;
            // 
            // txtUserID
            // 
            this.txtUserID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtUserID.Location = new System.Drawing.Point(183, 0);
            this.txtUserID.MaxLength = 5;
            this.txtUserID.Name = "txtUserID";
            this.txtUserID.ReadOnly = true;
            this.txtUserID.Size = new System.Drawing.Size(199, 28);
            this.txtUserID.TabIndex = 48;
            // 
            // lblUserID
            // 
            this.lblUserID.BackColor = System.Drawing.Color.Black;
            this.lblUserID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblUserID.ForeColor = System.Drawing.Color.White;
            this.lblUserID.Location = new System.Drawing.Point(3, 0);
            this.lblUserID.Name = "lblUserID";
            this.lblUserID.Size = new System.Drawing.Size(180, 26);
            this.lblUserID.TabIndex = 49;
            this.lblUserID.Text = "User ID";
            this.lblUserID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlEmail
            // 
            this.pnlEmail.Controls.Add(this.txtEmail);
            this.pnlEmail.Controls.Add(this.lblEmail);
            this.pnlEmail.Location = new System.Drawing.Point(3, 73);
            this.pnlEmail.Name = "pnlEmail";
            this.pnlEmail.Size = new System.Drawing.Size(776, 29);
            this.pnlEmail.TabIndex = 3;
            // 
            // txtEmail
            // 
            this.txtEmail.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtEmail.Location = new System.Drawing.Point(183, 0);
            this.txtEmail.MaxLength = 5;
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.ReadOnly = true;
            this.txtEmail.Size = new System.Drawing.Size(590, 28);
            this.txtEmail.TabIndex = 48;
            // 
            // lblEmail
            // 
            this.lblEmail.BackColor = System.Drawing.Color.Black;
            this.lblEmail.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblEmail.ForeColor = System.Drawing.Color.White;
            this.lblEmail.Location = new System.Drawing.Point(3, 0);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(180, 26);
            this.lblEmail.TabIndex = 49;
            this.lblEmail.Text = "E-mail";
            this.lblEmail.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlLoaclName
            // 
            this.pnlLoaclName.Controls.Add(this.txtLocalName);
            this.pnlLoaclName.Controls.Add(this.lblLocalName);
            this.pnlLoaclName.Location = new System.Drawing.Point(3, 108);
            this.pnlLoaclName.Name = "pnlLoaclName";
            this.pnlLoaclName.Size = new System.Drawing.Size(385, 29);
            this.pnlLoaclName.TabIndex = 4;
            // 
            // txtLocalName
            // 
            this.txtLocalName.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtLocalName.Location = new System.Drawing.Point(183, 0);
            this.txtLocalName.MaxLength = 5;
            this.txtLocalName.Name = "txtLocalName";
            this.txtLocalName.ReadOnly = true;
            this.txtLocalName.Size = new System.Drawing.Size(199, 28);
            this.txtLocalName.TabIndex = 48;
            // 
            // lblLocalName
            // 
            this.lblLocalName.BackColor = System.Drawing.Color.Black;
            this.lblLocalName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblLocalName.ForeColor = System.Drawing.Color.White;
            this.lblLocalName.Location = new System.Drawing.Point(3, 0);
            this.lblLocalName.Name = "lblLocalName";
            this.lblLocalName.Size = new System.Drawing.Size(180, 26);
            this.lblLocalName.TabIndex = 49;
            this.lblLocalName.Text = "Local Name";
            this.lblLocalName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlDefaultFactoryName
            // 
            this.pnlDefaultFactoryName.Controls.Add(this.txtDefaultFactoryName);
            this.pnlDefaultFactoryName.Controls.Add(this.lblDefaultFactoryName);
            this.pnlDefaultFactoryName.Location = new System.Drawing.Point(394, 108);
            this.pnlDefaultFactoryName.Name = "pnlDefaultFactoryName";
            this.pnlDefaultFactoryName.Size = new System.Drawing.Size(385, 29);
            this.pnlDefaultFactoryName.TabIndex = 5;
            // 
            // txtDefaultFactoryName
            // 
            this.txtDefaultFactoryName.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtDefaultFactoryName.Location = new System.Drawing.Point(183, 0);
            this.txtDefaultFactoryName.MaxLength = 5;
            this.txtDefaultFactoryName.Name = "txtDefaultFactoryName";
            this.txtDefaultFactoryName.ReadOnly = true;
            this.txtDefaultFactoryName.Size = new System.Drawing.Size(199, 28);
            this.txtDefaultFactoryName.TabIndex = 48;
            // 
            // lblDefaultFactoryName
            // 
            this.lblDefaultFactoryName.BackColor = System.Drawing.Color.Black;
            this.lblDefaultFactoryName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblDefaultFactoryName.ForeColor = System.Drawing.Color.White;
            this.lblDefaultFactoryName.Location = new System.Drawing.Point(3, 0);
            this.lblDefaultFactoryName.Name = "lblDefaultFactoryName";
            this.lblDefaultFactoryName.Size = new System.Drawing.Size(180, 26);
            this.lblDefaultFactoryName.TabIndex = 49;
            this.lblDefaultFactoryName.Text = "Default Factory Name";
            this.lblDefaultFactoryName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlTrxDataTime
            // 
            this.pnlTrxDataTime.Controls.Add(this.txtDateTime);
            this.pnlTrxDataTime.Controls.Add(this.lblTrxDataTime);
            this.pnlTrxDataTime.Location = new System.Drawing.Point(3, 143);
            this.pnlTrxDataTime.Name = "pnlTrxDataTime";
            this.pnlTrxDataTime.Size = new System.Drawing.Size(385, 29);
            this.pnlTrxDataTime.TabIndex = 6;
            // 
            // txtDateTime
            // 
            this.txtDateTime.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtDateTime.Location = new System.Drawing.Point(183, 0);
            this.txtDateTime.MaxLength = 5;
            this.txtDateTime.Name = "txtDateTime";
            this.txtDateTime.ReadOnly = true;
            this.txtDateTime.Size = new System.Drawing.Size(199, 28);
            this.txtDateTime.TabIndex = 48;
            // 
            // lblTrxDataTime
            // 
            this.lblTrxDataTime.BackColor = System.Drawing.Color.Black;
            this.lblTrxDataTime.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblTrxDataTime.ForeColor = System.Drawing.Color.White;
            this.lblTrxDataTime.Location = new System.Drawing.Point(3, 0);
            this.lblTrxDataTime.Name = "lblTrxDataTime";
            this.lblTrxDataTime.Size = new System.Drawing.Size(180, 26);
            this.lblTrxDataTime.TabIndex = 49;
            this.lblTrxDataTime.Text = "Trx DataTime";
            this.lblTrxDataTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlAddDateTime
            // 
            this.pnlAddDateTime.Controls.Add(this.txtAddDateTime);
            this.pnlAddDateTime.Controls.Add(this.lblAddDateTime);
            this.pnlAddDateTime.Location = new System.Drawing.Point(394, 143);
            this.pnlAddDateTime.Name = "pnlAddDateTime";
            this.pnlAddDateTime.Size = new System.Drawing.Size(385, 29);
            this.pnlAddDateTime.TabIndex = 8;
            // 
            // txtAddDateTime
            // 
            this.txtAddDateTime.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtAddDateTime.Location = new System.Drawing.Point(183, 0);
            this.txtAddDateTime.MaxLength = 5;
            this.txtAddDateTime.Name = "txtAddDateTime";
            this.txtAddDateTime.ReadOnly = true;
            this.txtAddDateTime.Size = new System.Drawing.Size(199, 28);
            this.txtAddDateTime.TabIndex = 48;
            // 
            // lblAddDateTime
            // 
            this.lblAddDateTime.BackColor = System.Drawing.Color.Black;
            this.lblAddDateTime.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblAddDateTime.ForeColor = System.Drawing.Color.White;
            this.lblAddDateTime.Location = new System.Drawing.Point(3, 0);
            this.lblAddDateTime.Name = "lblAddDateTime";
            this.lblAddDateTime.Size = new System.Drawing.Size(180, 26);
            this.lblAddDateTime.TabIndex = 49;
            this.lblAddDateTime.Text = "Add DateTime";
            this.lblAddDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlAddUserID
            // 
            this.pnlAddUserID.Controls.Add(this.txtAddUserID);
            this.pnlAddUserID.Controls.Add(this.lblAddUserID);
            this.pnlAddUserID.Location = new System.Drawing.Point(3, 178);
            this.pnlAddUserID.Name = "pnlAddUserID";
            this.pnlAddUserID.Size = new System.Drawing.Size(385, 29);
            this.pnlAddUserID.TabIndex = 9;
            // 
            // txtAddUserID
            // 
            this.txtAddUserID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtAddUserID.Location = new System.Drawing.Point(183, 0);
            this.txtAddUserID.MaxLength = 5;
            this.txtAddUserID.Name = "txtAddUserID";
            this.txtAddUserID.ReadOnly = true;
            this.txtAddUserID.Size = new System.Drawing.Size(199, 28);
            this.txtAddUserID.TabIndex = 48;
            // 
            // lblAddUserID
            // 
            this.lblAddUserID.BackColor = System.Drawing.Color.Black;
            this.lblAddUserID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblAddUserID.ForeColor = System.Drawing.Color.White;
            this.lblAddUserID.Location = new System.Drawing.Point(3, 0);
            this.lblAddUserID.Name = "lblAddUserID";
            this.lblAddUserID.Size = new System.Drawing.Size(180, 26);
            this.lblAddUserID.TabIndex = 49;
            this.lblAddUserID.Text = "Add User ID";
            this.lblAddUserID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnQuery
            // 
            this.btnQuery.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnQuery.Location = new System.Drawing.Point(782, 439);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(120, 43);
            this.btnQuery.TabIndex = 26;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // FormUserQuery
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1260, 522);
            this.Name = "FormUserQuery";
            this.Text = "FormUserQuery";
            this.Load += new System.EventHandler(this.FormUserQuery_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpUseQery.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserList)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.pnlClientKey.ResumeLayout(false);
            this.pnlClientKey.PerformLayout();
            this.pnlPassword.ResumeLayout(false);
            this.pnlPassword.PerformLayout();
            this.pnlActive.ResumeLayout(false);
            this.pnlActive.PerformLayout();
            this.pnlUserID.ResumeLayout(false);
            this.pnlUserID.PerformLayout();
            this.pnlEmail.ResumeLayout(false);
            this.pnlEmail.PerformLayout();
            this.pnlLoaclName.ResumeLayout(false);
            this.pnlLoaclName.PerformLayout();
            this.pnlDefaultFactoryName.ResumeLayout(false);
            this.pnlDefaultFactoryName.PerformLayout();
            this.pnlTrxDataTime.ResumeLayout(false);
            this.pnlTrxDataTime.PerformLayout();
            this.pnlAddDateTime.ResumeLayout(false);
            this.pnlAddDateTime.PerformLayout();
            this.pnlAddUserID.ResumeLayout(false);
            this.pnlAddUserID.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpUseQery;
        private System.Windows.Forms.DataGridView dgvUserList;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Panel pnlClientKey;
        private System.Windows.Forms.TextBox txtClientKey;
        private System.Windows.Forms.Label lblClientKey;
        private System.Windows.Forms.Panel pnlPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Panel pnlActive;
        private System.Windows.Forms.TextBox txtActive;
        private System.Windows.Forms.Label lblActive;
        private System.Windows.Forms.Panel pnlEmail;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.Panel pnlLoaclName;
        private System.Windows.Forms.TextBox txtLocalName;
        private System.Windows.Forms.Label lblLocalName;
        private System.Windows.Forms.Panel pnlDefaultFactoryName;
        private System.Windows.Forms.TextBox txtDefaultFactoryName;
        private System.Windows.Forms.Label lblDefaultFactoryName;
        private System.Windows.Forms.Panel pnlTrxDataTime;
        private System.Windows.Forms.TextBox txtDateTime;
        private System.Windows.Forms.Label lblTrxDataTime;
        private System.Windows.Forms.Panel pnlUserID;
        private System.Windows.Forms.TextBox txtUserID;
        private System.Windows.Forms.Label lblUserID;
        private System.Windows.Forms.Panel pnlAddDateTime;
        private System.Windows.Forms.TextBox txtAddDateTime;
        private System.Windows.Forms.Label lblAddDateTime;
        private System.Windows.Forms.Panel pnlAddUserID;
        private System.Windows.Forms.TextBox txtAddUserID;
        private System.Windows.Forms.Label lblAddUserID;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colLogging;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colLogout;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtGROUP_ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtUSER_ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtCLIENT_KEY;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtUSER_NAME;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtPASSWORD;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtEmail;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtLocalName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtDefaultFactoryName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtDefaultAreaName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dgvchkUACActive;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dgvchkACTIVE;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtTrxDatetime;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtTrxUserID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtAddDatetime;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtAddUserID;
    }
}