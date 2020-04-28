namespace UniOPI
{
    partial class FormLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLogin));
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.lblAPInfo = new System.Windows.Forms.Label();
            this.pnlLogin = new System.Windows.Forms.Panel();
            this.tlpLoginInfo = new System.Windows.Forms.TableLayoutPanel();
            this.cboLanguage = new System.Windows.Forms.ComboBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.cboFabType = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblDateTime = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.lblFabType = new System.Windows.Forms.Label();
            this.lblLineType = new System.Windows.Forms.Label();
            this.cboServerName = new System.Windows.Forms.ComboBox();
            this.lblLineId = new System.Windows.Forms.Label();
            this.cboLineType = new System.Windows.Forms.ComboBox();
            this.lblUserId = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlLogo = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblRemark = new System.Windows.Forms.Label();
            this.lbl_hjy = new System.Windows.Forms.Label();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.tlpBase.SuspendLayout();
            this.pnlLogin.SuspendLayout();
            this.tlpLoginInfo.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpBase
            // 
            this.tlpBase.BackColor = System.Drawing.Color.Transparent;
            this.tlpBase.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.tlpBase.ColumnCount = 3;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 533F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tlpBase.Controls.Add(this.lblAPInfo, 0, 5);
            this.tlpBase.Controls.Add(this.pnlLogin, 1, 3);
            this.tlpBase.Controls.Add(this.pnlLogo, 1, 0);
            this.tlpBase.Controls.Add(this.pictureBox1, 1, 1);
            this.tlpBase.Controls.Add(this.lblRemark, 0, 1);
            this.tlpBase.Controls.Add(this.lbl_hjy, 1, 4);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Margin = new System.Windows.Forms.Padding(4);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 6;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 265F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 124F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 354F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpBase.Size = new System.Drawing.Size(1325, 828);
            this.tlpBase.TabIndex = 17;
            // 
            // lblAPInfo
            // 
            this.tlpBase.SetColumnSpan(this.lblAPInfo, 2);
            this.lblAPInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAPInfo.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAPInfo.ForeColor = System.Drawing.Color.White;
            this.lblAPInfo.Location = new System.Drawing.Point(4, 798);
            this.lblAPInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAPInfo.Name = "lblAPInfo";
            this.lblAPInfo.Size = new System.Drawing.Size(1290, 30);
            this.lblAPInfo.TabIndex = 9;
            this.lblAPInfo.Text = "Copyright © 2014 Unicom All rights reserved";
            this.lblAPInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlLogin
            // 
            this.pnlLogin.BackgroundImage = global::UniOPI.Properties.Resources.Bg_login_info;
            this.pnlLogin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlLogin.Controls.Add(this.tlpLoginInfo);
            this.pnlLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLogin.Location = new System.Drawing.Point(769, 405);
            this.pnlLogin.Margin = new System.Windows.Forms.Padding(4);
            this.pnlLogin.Name = "pnlLogin";
            this.pnlLogin.Size = new System.Drawing.Size(525, 346);
            this.pnlLogin.TabIndex = 18;
            // 
            // tlpLoginInfo
            // 
            this.tlpLoginInfo.BackColor = System.Drawing.Color.Transparent;
            this.tlpLoginInfo.ColumnCount = 4;
            this.tlpLoginInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 19F));
            this.tlpLoginInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tlpLoginInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 333F));
            this.tlpLoginInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLoginInfo.Controls.Add(this.cboLanguage, 2, 7);
            this.tlpLoginInfo.Controls.Add(this.txtPassword, 2, 6);
            this.tlpLoginInfo.Controls.Add(this.pnlButton, 1, 8);
            this.tlpLoginInfo.Controls.Add(this.cboFabType, 2, 2);
            this.tlpLoginInfo.Controls.Add(this.panel1, 1, 0);
            this.tlpLoginInfo.Controls.Add(this.txtUserId, 2, 5);
            this.tlpLoginInfo.Controls.Add(this.lblFabType, 1, 2);
            this.tlpLoginInfo.Controls.Add(this.lblLineType, 1, 3);
            this.tlpLoginInfo.Controls.Add(this.cboServerName, 2, 4);
            this.tlpLoginInfo.Controls.Add(this.lblLineId, 1, 4);
            this.tlpLoginInfo.Controls.Add(this.cboLineType, 2, 3);
            this.tlpLoginInfo.Controls.Add(this.lblUserId, 1, 5);
            this.tlpLoginInfo.Controls.Add(this.lblPassword, 1, 6);
            this.tlpLoginInfo.Controls.Add(this.lblLanguage, 1, 7);
            this.tlpLoginInfo.Controls.Add(this.panel2, 1, 1);
            this.tlpLoginInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLoginInfo.Location = new System.Drawing.Point(0, 0);
            this.tlpLoginInfo.Margin = new System.Windows.Forms.Padding(4);
            this.tlpLoginInfo.Name = "tlpLoginInfo";
            this.tlpLoginInfo.RowCount = 9;
            this.tlpLoginInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tlpLoginInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpLoginInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tlpLoginInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tlpLoginInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tlpLoginInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpLoginInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tlpLoginInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tlpLoginInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLoginInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpLoginInfo.Size = new System.Drawing.Size(525, 346);
            this.tlpLoginInfo.TabIndex = 17;
            // 
            // cboLanguage
            // 
            this.cboLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLanguage.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboLanguage.FormattingEnabled = true;
            this.cboLanguage.Location = new System.Drawing.Point(148, 259);
            this.cboLanguage.Margin = new System.Windows.Forms.Padding(4);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Size = new System.Drawing.Size(325, 32);
            this.cboLanguage.TabIndex = 13;
            // 
            // txtPassword
            // 
            this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPassword.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPassword.Location = new System.Drawing.Point(148, 217);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(325, 32);
            this.txtPassword.TabIndex = 15;
            this.txtPassword.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txt_MouseClick);
            this.txtPassword.Enter += new System.EventHandler(this.txt_Enter);
            this.txtPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPassword_KeyPress);
            // 
            // pnlButton
            // 
            this.tlpLoginInfo.SetColumnSpan(this.pnlButton, 2);
            this.pnlButton.Controls.Add(this.btnLogin);
            this.pnlButton.Controls.Add(this.btnExit);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(23, 300);
            this.pnlButton.Margin = new System.Windows.Forms.Padding(4);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(450, 42);
            this.pnlButton.TabIndex = 16;
            // 
            // btnLogin
            // 
            this.btnLogin.BackgroundImage = global::UniOPI.Properties.Resources.BtnBackground;
            this.btnLogin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogin.ForeColor = System.Drawing.Color.Yellow;
            this.btnLogin.Location = new System.Drawing.Point(215, 0);
            this.btnLogin.Margin = new System.Windows.Forms.Padding(4);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(113, 38);
            this.btnLogin.TabIndex = 18;
            this.btnLogin.Text = "OK";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackgroundImage = global::UniOPI.Properties.Resources.BtnBackground;
            this.btnExit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold);
            this.btnExit.ForeColor = System.Drawing.Color.Yellow;
            this.btnExit.Location = new System.Drawing.Point(333, 0);
            this.btnExit.Margin = new System.Windows.Forms.Padding(4);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(113, 38);
            this.btnExit.TabIndex = 19;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // cboFabType
            // 
            this.cboFabType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboFabType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFabType.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboFabType.FormattingEnabled = true;
            this.cboFabType.Location = new System.Drawing.Point(148, 54);
            this.cboFabType.Margin = new System.Windows.Forms.Padding(4);
            this.cboFabType.Name = "cboFabType";
            this.cboFabType.Size = new System.Drawing.Size(325, 32);
            this.cboFabType.TabIndex = 10;
            this.cboFabType.SelectedIndexChanged += new System.EventHandler(this.cboFabType_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.tlpLoginInfo.SetColumnSpan(this.panel1, 3);
            this.panel1.Controls.Add(this.lblDateTime);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(23, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(498, 30);
            this.panel1.TabIndex = 19;
            // 
            // lblDateTime
            // 
            this.lblDateTime.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDateTime.ForeColor = System.Drawing.Color.Gray;
            this.lblDateTime.Location = new System.Drawing.Point(268, 8);
            this.lblDateTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDateTime.Name = "lblDateTime";
            this.lblDateTime.Size = new System.Drawing.Size(191, 24);
            this.lblDateTime.TabIndex = 18;
            this.lblDateTime.Text = "2014-08-25 00:00:00";
            this.lblDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Calibri", 17.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(20, -4);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(153, 38);
            this.label1.TabIndex = 17;
            this.label1.Text = "User Login";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // txtUserId
            // 
            this.txtUserId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtUserId.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserId.Location = new System.Drawing.Point(148, 177);
            this.txtUserId.Margin = new System.Windows.Forms.Padding(4);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(325, 32);
            this.txtUserId.TabIndex = 14;
            this.txtUserId.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txt_MouseClick);
            this.txtUserId.Enter += new System.EventHandler(this.txt_Enter);
            // 
            // lblFabType
            // 
            this.lblFabType.AutoSize = true;
            this.lblFabType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFabType.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFabType.Location = new System.Drawing.Point(23, 50);
            this.lblFabType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFabType.Name = "lblFabType";
            this.lblFabType.Size = new System.Drawing.Size(117, 41);
            this.lblFabType.TabIndex = 0;
            this.lblFabType.Text = ". FabType";
            this.lblFabType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLineType
            // 
            this.lblLineType.AutoSize = true;
            this.lblLineType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLineType.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLineType.Location = new System.Drawing.Point(23, 91);
            this.lblLineType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLineType.Name = "lblLineType";
            this.lblLineType.Size = new System.Drawing.Size(117, 41);
            this.lblLineType.TabIndex = 1;
            this.lblLineType.Text = ". Line Type";
            this.lblLineType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboServerName
            // 
            this.cboServerName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboServerName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboServerName.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboServerName.FormattingEnabled = true;
            this.cboServerName.Location = new System.Drawing.Point(148, 136);
            this.cboServerName.Margin = new System.Windows.Forms.Padding(4);
            this.cboServerName.Name = "cboServerName";
            this.cboServerName.Size = new System.Drawing.Size(325, 32);
            this.cboServerName.TabIndex = 12;
            // 
            // lblLineId
            // 
            this.lblLineId.AutoSize = true;
            this.lblLineId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLineId.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLineId.Location = new System.Drawing.Point(23, 132);
            this.lblLineId.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLineId.Name = "lblLineId";
            this.lblLineId.Size = new System.Drawing.Size(117, 41);
            this.lblLineId.TabIndex = 2;
            this.lblLineId.Text = ". Line ID";
            this.lblLineId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboLineType
            // 
            this.cboLineType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboLineType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLineType.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboLineType.FormattingEnabled = true;
            this.cboLineType.Location = new System.Drawing.Point(148, 95);
            this.cboLineType.Margin = new System.Windows.Forms.Padding(4);
            this.cboLineType.Name = "cboLineType";
            this.cboLineType.Size = new System.Drawing.Size(325, 32);
            this.cboLineType.TabIndex = 11;
            this.cboLineType.SelectedIndexChanged += new System.EventHandler(this.cboLineType_SelectedIndexChanged);
            // 
            // lblUserId
            // 
            this.lblUserId.AutoSize = true;
            this.lblUserId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUserId.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserId.Location = new System.Drawing.Point(23, 173);
            this.lblUserId.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(117, 40);
            this.lblUserId.TabIndex = 3;
            this.lblUserId.Text = ". User ID";
            this.lblUserId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPassword.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPassword.Location = new System.Drawing.Point(23, 213);
            this.lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(117, 42);
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = ". Password";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLanguage
            // 
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLanguage.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLanguage.Location = new System.Drawing.Point(23, 255);
            this.lblLanguage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(117, 41);
            this.lblLanguage.TabIndex = 5;
            this.lblLanguage.Text = ". Language";
            this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel2
            // 
            this.panel2.BackgroundImage = global::UniOPI.Properties.Resources.line2;
            this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.tlpLoginInfo.SetColumnSpan(this.panel2, 2);
            this.panel2.Location = new System.Drawing.Point(23, 42);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(450, 4);
            this.panel2.TabIndex = 20;
            // 
            // pnlLogo
            // 
            this.tlpBase.SetColumnSpan(this.pnlLogo, 2);
            this.pnlLogo.Controls.Add(this.picLogo);
            this.pnlLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLogo.Location = new System.Drawing.Point(769, 4);
            this.pnlLogo.Margin = new System.Windows.Forms.Padding(4);
            this.pnlLogo.Name = "pnlLogo";
            this.pnlLogo.Size = new System.Drawing.Size(552, 257);
            this.pnlLogo.TabIndex = 19;
            // 
            // picLogo
            // 
            this.picLogo.BackgroundImage = global::UniOPI.Properties.Resources.CSOT_3;
            this.picLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picLogo.Location = new System.Drawing.Point(309, 135);
            this.picLogo.Margin = new System.Windows.Forms.Padding(4);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(232, 69);
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox1.Location = new System.Drawing.Point(769, 269);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(525, 80);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // lblRemark
            // 
            this.lblRemark.Location = new System.Drawing.Point(4, 265);
            this.lblRemark.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRemark.Name = "lblRemark";
            this.lblRemark.Size = new System.Drawing.Size(44, 39);
            this.lblRemark.TabIndex = 20;
            this.lblRemark.DoubleClick += new System.EventHandler(this.lblRemark_DoubleClick);
            // 
            // lbl_hjy
            // 
            this.lbl_hjy.Location = new System.Drawing.Point(769, 755);
            this.lbl_hjy.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl_hjy.Name = "lbl_hjy";
            this.lbl_hjy.Size = new System.Drawing.Size(44, 39);
            this.lbl_hjy.TabIndex = 21;
            this.lbl_hjy.Tag = "hjy";
            this.lbl_hjy.DoubleClick += new System.EventHandler(this.lblRemark_DoubleClick);
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // FormLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::UniOPI.Properties.Resources.Bg_Login;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1325, 828);
            this.ControlBox = false;
            this.Controls.Add(this.tlpBase);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OPI";
            this.Load += new System.EventHandler(this.FormLogin_Load);
            this.Shown += new System.EventHandler(this.FormLogin_Shown);
            this.tlpBase.ResumeLayout(false);
            this.pnlLogin.ResumeLayout(false);
            this.tlpLoginInfo.ResumeLayout(false);
            this.tlpLoginInfo.PerformLayout();
            this.pnlButton.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.pnlLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblFabType;
        private System.Windows.Forms.Label lblLineType;
        private System.Windows.Forms.Label lblLineId;
        private System.Windows.Forms.Label lblUserId;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.Label lblAPInfo;
        private System.Windows.Forms.ComboBox cboFabType;
        private System.Windows.Forms.ComboBox cboLineType;
        private System.Windows.Forms.ComboBox cboServerName;
        private System.Windows.Forms.ComboBox cboLanguage;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.TableLayoutPanel tlpLoginInfo;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Panel pnlLogin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblDateTime;
        private System.Windows.Forms.Panel pnlLogo;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.Label lblRemark;
        private System.Windows.Forms.Label lbl_hjy;
    }
}