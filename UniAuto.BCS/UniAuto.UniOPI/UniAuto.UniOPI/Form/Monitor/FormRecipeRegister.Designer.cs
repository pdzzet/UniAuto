namespace UniOPI
{
    partial class FormRecipeRegister
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
            this.spcRecipe = new System.Windows.Forms.SplitContainer();
            this.pnlCombox = new System.Windows.Forms.Panel();
            this.grbPortNo = new System.Windows.Forms.GroupBox();
            this.cboPortNo = new System.Windows.Forms.ComboBox();
            this.grbMESMode = new System.Windows.Forms.GroupBox();
            this.rdoOffline = new System.Windows.Forms.RadioButton();
            this.rdoOnlineLocal = new System.Windows.Forms.RadioButton();
            this.rdoOnlineRemote = new System.Windows.Forms.RadioButton();
            this.grbRecipeName = new System.Windows.Forms.GroupBox();
            this.cboRecipeName = new System.Windows.Forms.ComboBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.grbRegisterReturn = new System.Windows.Forms.GroupBox();
            this.tlpReturn = new System.Windows.Forms.TableLayoutPanel();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.pnlReturn = new System.Windows.Forms.Panel();
            this.txtTotalReturn = new System.Windows.Forms.TextBox();
            this.lblTotalReturn = new System.Windows.Forms.Label();
            this.pnlMemo = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlNG = new System.Windows.Forms.Panel();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.colReturn_EQPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturn_LocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturn_RecipeNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturn_CIMMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturn_Return = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturn_Msg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcRecipe)).BeginInit();
            this.spcRecipe.Panel1.SuspendLayout();
            this.spcRecipe.Panel2.SuspendLayout();
            this.spcRecipe.SuspendLayout();
            this.pnlCombox.SuspendLayout();
            this.grbPortNo.SuspendLayout();
            this.grbMESMode.SuspendLayout();
            this.grbRecipeName.SuspendLayout();
            this.grbRegisterReturn.SuspendLayout();
            this.tlpReturn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.pnlReturn.SuspendLayout();
            this.pnlMemo.SuspendLayout();
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
            this.spcRecipe.Panel2.Controls.Add(this.grbRegisterReturn);
            this.spcRecipe.Size = new System.Drawing.Size(1140, 555);
            this.spcRecipe.SplitterDistance = 70;
            this.spcRecipe.TabIndex = 2;
            // 
            // pnlCombox
            // 
            this.pnlCombox.Controls.Add(this.grbPortNo);
            this.pnlCombox.Controls.Add(this.grbMESMode);
            this.pnlCombox.Controls.Add(this.grbRecipeName);
            this.pnlCombox.Controls.Add(this.btnSend);
            this.pnlCombox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCombox.Location = new System.Drawing.Point(0, 0);
            this.pnlCombox.Name = "pnlCombox";
            this.pnlCombox.Size = new System.Drawing.Size(1140, 70);
            this.pnlCombox.TabIndex = 26;
            // 
            // grbPortNo
            // 
            this.grbPortNo.Controls.Add(this.cboPortNo);
            this.grbPortNo.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.grbPortNo.Location = new System.Drawing.Point(639, 0);
            this.grbPortNo.Name = "grbPortNo";
            this.grbPortNo.Size = new System.Drawing.Size(185, 60);
            this.grbPortNo.TabIndex = 43;
            this.grbPortNo.TabStop = false;
            this.grbPortNo.Text = "Port No";
            // 
            // cboPortNo
            // 
            this.cboPortNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPortNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboPortNo.FormattingEnabled = true;
            this.cboPortNo.Location = new System.Drawing.Point(16, 22);
            this.cboPortNo.Name = "cboPortNo";
            this.cboPortNo.Size = new System.Drawing.Size(156, 29);
            this.cboPortNo.TabIndex = 42;
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
            this.btnSend.Location = new System.Drawing.Point(1018, 22);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(110, 29);
            this.btnSend.TabIndex = 9;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // grbRegisterReturn
            // 
            this.grbRegisterReturn.Controls.Add(this.tlpReturn);
            this.grbRegisterReturn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbRegisterReturn.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.grbRegisterReturn.Location = new System.Drawing.Point(0, 0);
            this.grbRegisterReturn.Name = "grbRegisterReturn";
            this.grbRegisterReturn.Size = new System.Drawing.Size(1140, 481);
            this.grbRegisterReturn.TabIndex = 16;
            this.grbRegisterReturn.TabStop = false;
            // 
            // tlpReturn
            // 
            this.tlpReturn.ColumnCount = 1;
            this.tlpReturn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpReturn.Controls.Add(this.dgvData, 0, 1);
            this.tlpReturn.Controls.Add(this.pnlReturn, 0, 0);
            this.tlpReturn.Controls.Add(this.pnlMemo, 0, 2);
            this.tlpReturn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpReturn.Location = new System.Drawing.Point(3, 24);
            this.tlpReturn.Name = "tlpReturn";
            this.tlpReturn.RowCount = 3;
            this.tlpReturn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpReturn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpReturn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpReturn.Size = new System.Drawing.Size(1134, 454);
            this.tlpReturn.TabIndex = 14;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            this.dgvData.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12.75F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colReturn_EQPID,
            this.colReturn_LocalNo,
            this.colReturn_RecipeNo,
            this.colReturn_CIMMode,
            this.colReturn_Return,
            this.colReturn_Msg});
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 43);
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvData.Size = new System.Drawing.Size(1128, 353);
            this.dgvData.TabIndex = 13;
            // 
            // pnlReturn
            // 
            this.pnlReturn.Controls.Add(this.txtTotalReturn);
            this.pnlReturn.Controls.Add(this.lblTotalReturn);
            this.pnlReturn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlReturn.Location = new System.Drawing.Point(3, 3);
            this.pnlReturn.Name = "pnlReturn";
            this.pnlReturn.Size = new System.Drawing.Size(1128, 34);
            this.pnlReturn.TabIndex = 14;
            // 
            // txtTotalReturn
            // 
            this.txtTotalReturn.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtTotalReturn.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotalReturn.Location = new System.Drawing.Point(113, 3);
            this.txtTotalReturn.Name = "txtTotalReturn";
            this.txtTotalReturn.ReadOnly = true;
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
            // pnlMemo
            // 
            this.pnlMemo.Controls.Add(this.label4);
            this.pnlMemo.Controls.Add(this.label3);
            this.pnlMemo.Controls.Add(this.label2);
            this.pnlMemo.Controls.Add(this.label1);
            this.pnlMemo.Controls.Add(this.panel3);
            this.pnlMemo.Controls.Add(this.panel2);
            this.pnlMemo.Controls.Add(this.panel1);
            this.pnlMemo.Controls.Add(this.pnlNG);
            this.pnlMemo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMemo.Location = new System.Drawing.Point(3, 402);
            this.pnlMemo.Name = "pnlMemo";
            this.pnlMemo.Size = new System.Drawing.Size(1128, 49);
            this.pnlMemo.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(192, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 19);
            this.label4.TabIndex = 40;
            this.label4.Text = "Timeout";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(192, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(235, 19);
            this.label3.TabIndex = 39;
            this.label3.Text = "By Pass (CIM OFF / No Check / Zero)";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(27, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 19);
            this.label2.TabIndex = 38;
            this.label2.Text = "Unknown";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(26, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 19);
            this.label1.TabIndex = 37;
            this.label1.Text = "Register NG";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Pink;
            this.panel3.Location = new System.Drawing.Point(171, 27);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(19, 19);
            this.panel3.TabIndex = 36;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Gold;
            this.panel2.Location = new System.Drawing.Point(171, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(19, 19);
            this.panel2.TabIndex = 35;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Silver;
            this.panel1.Location = new System.Drawing.Point(7, 27);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(19, 19);
            this.panel1.TabIndex = 34;
            // 
            // pnlNG
            // 
            this.pnlNG.BackColor = System.Drawing.Color.Red;
            this.pnlNG.Location = new System.Drawing.Point(7, 5);
            this.pnlNG.Name = "pnlNG";
            this.pnlNG.Size = new System.Drawing.Size(19, 19);
            this.pnlNG.TabIndex = 33;
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 1000;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // colReturn_EQPID
            // 
            this.colReturn_EQPID.DataPropertyName = "EQPID";
            this.colReturn_EQPID.HeaderText = "EQPID";
            this.colReturn_EQPID.Name = "colReturn_EQPID";
            this.colReturn_EQPID.ReadOnly = true;
            this.colReturn_EQPID.Width = 150;
            // 
            // colReturn_LocalNo
            // 
            this.colReturn_LocalNo.DataPropertyName = "LocalNo";
            this.colReturn_LocalNo.HeaderText = "Local No";
            this.colReturn_LocalNo.Name = "colReturn_LocalNo";
            this.colReturn_LocalNo.ReadOnly = true;
            // 
            // colReturn_RecipeNo
            // 
            this.colReturn_RecipeNo.DataPropertyName = "Recipe No";
            this.colReturn_RecipeNo.HeaderText = "Recipe ID";
            this.colReturn_RecipeNo.Name = "colReturn_RecipeNo";
            this.colReturn_RecipeNo.ReadOnly = true;
            this.colReturn_RecipeNo.Width = 200;
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
            this.colReturn_Return.Width = 200;
            // 
            // colReturn_Msg
            // 
            this.colReturn_Msg.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colReturn_Msg.DataPropertyName = "ReturnMsg";
            this.colReturn_Msg.HeaderText = "Retrun Message";
            this.colReturn_Msg.Name = "colReturn_Msg";
            this.colReturn_Msg.ReadOnly = true;
            // 
            // FormRecipeRegister
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 586);
            this.Name = "FormRecipeRegister";
            this.Text = "FormRecipeRegister";
            this.Load += new System.EventHandler(this.FormRecipeRegister_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.spcRecipe.Panel1.ResumeLayout(false);
            this.spcRecipe.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcRecipe)).EndInit();
            this.spcRecipe.ResumeLayout(false);
            this.pnlCombox.ResumeLayout(false);
            this.grbPortNo.ResumeLayout(false);
            this.grbMESMode.ResumeLayout(false);
            this.grbRecipeName.ResumeLayout(false);
            this.grbRegisterReturn.ResumeLayout(false);
            this.tlpReturn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.pnlReturn.ResumeLayout(false);
            this.pnlReturn.PerformLayout();
            this.pnlMemo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spcRecipe;
        private System.Windows.Forms.Panel pnlCombox;
        private System.Windows.Forms.GroupBox grbMESMode;
        private System.Windows.Forms.RadioButton rdoOffline;
        private System.Windows.Forms.RadioButton rdoOnlineLocal;
        private System.Windows.Forms.RadioButton rdoOnlineRemote;
        private System.Windows.Forms.GroupBox grbRecipeName;
        private System.Windows.Forms.ComboBox cboRecipeName;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.GroupBox grbRegisterReturn;
        private System.Windows.Forms.TableLayoutPanel tlpReturn;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Panel pnlReturn;
        private System.Windows.Forms.TextBox txtTotalReturn;
        private System.Windows.Forms.Label lblTotalReturn;
        private System.Windows.Forms.GroupBox grbPortNo;
        private System.Windows.Forms.ComboBox cboPortNo;
        public System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.Panel pnlMemo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel pnlNG;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_EQPID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_LocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_RecipeNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_CIMMode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_Return;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturn_Msg;
    }
}