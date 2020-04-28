namespace UniOPI
{
    partial class FormRecipeManagement_Edit
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
            this.flpHeader = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlMESMode = new System.Windows.Forms.Panel();
            this.lblMESMode = new System.Windows.Forms.Label();
            this.rdoOffline = new System.Windows.Forms.RadioButton();
            this.rdoOnlineLocal = new System.Windows.Forms.RadioButton();
            this.pnlRecipeName = new System.Windows.Forms.Panel();
            this.txtRecipeName = new System.Windows.Forms.TextBox();
            this.lblRecipeName = new System.Windows.Forms.Label();
            this.pnlComment = new System.Windows.Forms.Panel();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.lblComment = new System.Windows.Forms.Label();
            this.grbRecipeNo = new System.Windows.Forms.GroupBox();
            this.flpRecipeNo = new System.Windows.Forms.FlowLayoutPanel();
            this.flpButton = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlAdd = new System.Windows.Forms.Panel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.dgvRecipe = new System.Windows.Forms.DataGridView();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.colMESMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecipeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colREMARK = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.flpHeader.SuspendLayout();
            this.pnlMESMode.SuspendLayout();
            this.pnlRecipeName.SuspendLayout();
            this.pnlComment.SuspendLayout();
            this.grbRecipeNo.SuspendLayout();
            this.flpButton.SuspendLayout();
            this.pnlAdd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipe)).BeginInit();
            this.pnlButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(830, 30);
            this.lblCaption.Text = "Recipe No Edit";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(890, 698);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.flpHeader, 0, 0);
            this.tlpBase.Controls.Add(this.grbRecipeNo, 0, 1);
            this.tlpBase.Controls.Add(this.flpButton, 0, 2);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 3;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 82F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 276F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Size = new System.Drawing.Size(890, 667);
            this.tlpBase.TabIndex = 0;
            // 
            // flpHeader
            // 
            this.flpHeader.Controls.Add(this.pnlMESMode);
            this.flpHeader.Controls.Add(this.pnlRecipeName);
            this.flpHeader.Controls.Add(this.pnlComment);
            this.flpHeader.Location = new System.Drawing.Point(3, 3);
            this.flpHeader.Name = "flpHeader";
            this.flpHeader.Size = new System.Drawing.Size(884, 76);
            this.flpHeader.TabIndex = 23;
            // 
            // pnlMESMode
            // 
            this.pnlMESMode.Controls.Add(this.lblMESMode);
            this.pnlMESMode.Controls.Add(this.rdoOffline);
            this.pnlMESMode.Controls.Add(this.rdoOnlineLocal);
            this.pnlMESMode.Location = new System.Drawing.Point(3, 3);
            this.pnlMESMode.Name = "pnlMESMode";
            this.pnlMESMode.Size = new System.Drawing.Size(283, 30);
            this.pnlMESMode.TabIndex = 1;
            // 
            // lblMESMode
            // 
            this.lblMESMode.BackColor = System.Drawing.Color.Black;
            this.lblMESMode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMESMode.ForeColor = System.Drawing.Color.White;
            this.lblMESMode.Location = new System.Drawing.Point(0, 3);
            this.lblMESMode.Name = "lblMESMode";
            this.lblMESMode.Size = new System.Drawing.Size(89, 25);
            this.lblMESMode.TabIndex = 12;
            this.lblMESMode.Text = "MES Mode";
            this.lblMESMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // rdoOffline
            // 
            this.rdoOffline.AutoSize = true;
            this.rdoOffline.Checked = true;
            this.rdoOffline.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.rdoOffline.Location = new System.Drawing.Point(91, 0);
            this.rdoOffline.Name = "rdoOffline";
            this.rdoOffline.Size = new System.Drawing.Size(74, 25);
            this.rdoOffline.TabIndex = 2;
            this.rdoOffline.TabStop = true;
            this.rdoOffline.Text = "Offline";
            this.rdoOffline.UseVisualStyleBackColor = true;
            // 
            // rdoOnlineLocal
            // 
            this.rdoOnlineLocal.AutoSize = true;
            this.rdoOnlineLocal.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.rdoOnlineLocal.Location = new System.Drawing.Point(167, 0);
            this.rdoOnlineLocal.Name = "rdoOnlineLocal";
            this.rdoOnlineLocal.Size = new System.Drawing.Size(112, 25);
            this.rdoOnlineLocal.TabIndex = 3;
            this.rdoOnlineLocal.Text = "Online Local";
            this.rdoOnlineLocal.UseVisualStyleBackColor = true;
            // 
            // pnlRecipeName
            // 
            this.pnlRecipeName.Controls.Add(this.txtRecipeName);
            this.pnlRecipeName.Controls.Add(this.lblRecipeName);
            this.pnlRecipeName.Location = new System.Drawing.Point(292, 3);
            this.pnlRecipeName.Name = "pnlRecipeName";
            this.pnlRecipeName.Size = new System.Drawing.Size(330, 30);
            this.pnlRecipeName.TabIndex = 0;
            // 
            // txtRecipeName
            // 
            this.txtRecipeName.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtRecipeName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRecipeName.Location = new System.Drawing.Point(107, 0);
            this.txtRecipeName.Name = "txtRecipeName";
            this.txtRecipeName.Size = new System.Drawing.Size(221, 28);
            this.txtRecipeName.TabIndex = 0;
            // 
            // lblRecipeName
            // 
            this.lblRecipeName.BackColor = System.Drawing.Color.Black;
            this.lblRecipeName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRecipeName.ForeColor = System.Drawing.Color.White;
            this.lblRecipeName.Location = new System.Drawing.Point(0, 3);
            this.lblRecipeName.Name = "lblRecipeName";
            this.lblRecipeName.Size = new System.Drawing.Size(105, 25);
            this.lblRecipeName.TabIndex = 17;
            this.lblRecipeName.Text = "Recipe Name";
            this.lblRecipeName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlComment
            // 
            this.pnlComment.Controls.Add(this.txtComment);
            this.pnlComment.Controls.Add(this.lblComment);
            this.pnlComment.Location = new System.Drawing.Point(3, 39);
            this.pnlComment.Name = "pnlComment";
            this.pnlComment.Size = new System.Drawing.Size(872, 30);
            this.pnlComment.TabIndex = 2;
            // 
            // txtComment
            // 
            this.txtComment.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtComment.Location = new System.Drawing.Point(90, 0);
            this.txtComment.Name = "txtComment";
            this.txtComment.Size = new System.Drawing.Size(779, 28);
            this.txtComment.TabIndex = 4;
            // 
            // lblComment
            // 
            this.lblComment.BackColor = System.Drawing.Color.Black;
            this.lblComment.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblComment.ForeColor = System.Drawing.Color.White;
            this.lblComment.Location = new System.Drawing.Point(0, 3);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(89, 25);
            this.lblComment.TabIndex = 16;
            this.lblComment.Text = "Comment";
            this.lblComment.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // grbRecipeNo
            // 
            this.grbRecipeNo.Controls.Add(this.flpRecipeNo);
            this.grbRecipeNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbRecipeNo.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.grbRecipeNo.Location = new System.Drawing.Point(3, 85);
            this.grbRecipeNo.Name = "grbRecipeNo";
            this.grbRecipeNo.Size = new System.Drawing.Size(884, 270);
            this.grbRecipeNo.TabIndex = 28;
            this.grbRecipeNo.TabStop = false;
            this.grbRecipeNo.Text = "Recipe ID";
            // 
            // flpRecipeNo
            // 
            this.flpRecipeNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpRecipeNo.Location = new System.Drawing.Point(3, 24);
            this.flpRecipeNo.Name = "flpRecipeNo";
            this.flpRecipeNo.Size = new System.Drawing.Size(878, 243);
            this.flpRecipeNo.TabIndex = 24;
            // 
            // flpButton
            // 
            this.flpButton.Controls.Add(this.pnlAdd);
            this.flpButton.Controls.Add(this.dgvRecipe);
            this.flpButton.Controls.Add(this.pnlButton);
            this.flpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButton.Location = new System.Drawing.Point(3, 361);
            this.flpButton.Name = "flpButton";
            this.flpButton.Size = new System.Drawing.Size(884, 303);
            this.flpButton.TabIndex = 29;
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnAdd);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(881, 36);
            this.pnlAdd.TabIndex = 29;
            // 
            // btnAdd
            // 
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAdd.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(396, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(90, 30);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Tag = "ADD";
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btn_Click);
            // 
            // dgvRecipe
            // 
            this.dgvRecipe.AllowUserToAddRows = false;
            this.dgvRecipe.AllowUserToDeleteRows = false;
            this.dgvRecipe.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvRecipe.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvRecipe.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRecipe.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvRecipe.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecipe.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMESMode,
            this.colRecipeName,
            this.colPPID,
            this.colREMARK});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvRecipe.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvRecipe.Location = new System.Drawing.Point(3, 45);
            this.dgvRecipe.Name = "dgvRecipe";
            this.dgvRecipe.ReadOnly = true;
            this.dgvRecipe.RowHeadersVisible = false;
            this.dgvRecipe.RowTemplate.Height = 24;
            this.dgvRecipe.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecipe.Size = new System.Drawing.Size(878, 217);
            this.dgvRecipe.TabIndex = 27;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnCancel);
            this.pnlButton.Controls.Add(this.btnOK);
            this.pnlButton.Location = new System.Drawing.Point(3, 268);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(878, 36);
            this.pnlButton.TabIndex = 26;
            // 
            // btnCancel
            // 
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(441, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Tag = "Cancel";
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(346, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 30);
            this.btnOK.TabIndex = 2;
            this.btnOK.Tag = "OK";
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btn_Click);
            // 
            // colMESMode
            // 
            this.colMESMode.HeaderText = "MES Mode";
            this.colMESMode.Name = "colMESMode";
            this.colMESMode.ReadOnly = true;
            this.colMESMode.Width = 150;
            // 
            // colRecipeName
            // 
            this.colRecipeName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colRecipeName.HeaderText = "RecipeName";
            this.colRecipeName.Name = "colRecipeName";
            this.colRecipeName.ReadOnly = true;
            // 
            // colPPID
            // 
            this.colPPID.HeaderText = "PPID";
            this.colPPID.Name = "colPPID";
            this.colPPID.ReadOnly = true;
            // 
            // colREMARK
            // 
            this.colREMARK.HeaderText = "Remark";
            this.colREMARK.Name = "colREMARK";
            this.colREMARK.ReadOnly = true;
            this.colREMARK.Width = 300;
            // 
            // FormRecipeManagement_Edit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(890, 698);
            this.Name = "FormRecipeManagement_Edit";
            this.Text = "   ";
            this.Load += new System.EventHandler(this.FormRecipeManagement_Edit_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.flpHeader.ResumeLayout(false);
            this.pnlMESMode.ResumeLayout(false);
            this.pnlMESMode.PerformLayout();
            this.pnlRecipeName.ResumeLayout(false);
            this.pnlRecipeName.PerformLayout();
            this.pnlComment.ResumeLayout(false);
            this.pnlComment.PerformLayout();
            this.grbRecipeNo.ResumeLayout(false);
            this.flpButton.ResumeLayout(false);
            this.pnlAdd.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipe)).EndInit();
            this.pnlButton.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.FlowLayoutPanel flpHeader;
        private System.Windows.Forms.Panel pnlRecipeName;
        private System.Windows.Forms.TextBox txtRecipeName;
        private System.Windows.Forms.Label lblRecipeName;
        private System.Windows.Forms.Panel pnlMESMode;
        private System.Windows.Forms.Label lblMESMode;
        private System.Windows.Forms.RadioButton rdoOffline;
        private System.Windows.Forms.RadioButton rdoOnlineLocal;
        private System.Windows.Forms.Panel pnlComment;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridView dgvRecipe;
        private System.Windows.Forms.GroupBox grbRecipeNo;
        private System.Windows.Forms.FlowLayoutPanel flpRecipeNo;
        private System.Windows.Forms.Panel pnlAdd;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.FlowLayoutPanel flpButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMESMode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecipeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPPID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colREMARK;
    }
}