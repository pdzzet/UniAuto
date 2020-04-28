namespace UniOPI
{
    partial class FormUserChangePassword
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
            this.gbxResult = new System.Windows.Forms.GroupBox();
            this.dgvList = new System.Windows.Forms.DataGridView();
            this.dgvtxtServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtLINETYPE = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtLineID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtResultDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gbxLineType = new System.Windows.Forms.GroupBox();
            this.chklsbLineType = new System.Windows.Forms.CheckedListBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.gbxPassword = new System.Windows.Forms.GroupBox();
            this.txtNewPassword = new System.Windows.Forms.TextBox();
            this.txtOldPassword = new System.Windows.Forms.TextBox();
            this.lblNewPassword = new System.Windows.Forms.Label();
            this.lblOldPassword = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.gbxResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvList)).BeginInit();
            this.gbxLineType.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.gbxPassword.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(908, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(968, 586);
            // 
            // gbxResult
            // 
            this.gbxResult.Controls.Add(this.dgvList);
            this.gbxResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxResult.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbxResult.Location = new System.Drawing.Point(23, 327);
            this.gbxResult.Name = "gbxResult";
            this.gbxResult.Size = new System.Drawing.Size(922, 291);
            this.gbxResult.TabIndex = 9;
            this.gbxResult.TabStop = false;
            this.gbxResult.Text = "Result";
            // 
            // dgvList
            // 
            this.dgvList.AllowUserToAddRows = false;
            this.dgvList.AllowUserToDeleteRows = false;
            this.dgvList.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvList.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvList.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvtxtServerName,
            this.dgvtxtLINETYPE,
            this.dgvtxtLineID,
            this.dgvtxtResult,
            this.dgvtxtResultDesc});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvList.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvList.Location = new System.Drawing.Point(3, 24);
            this.dgvList.Name = "dgvList";
            this.dgvList.ReadOnly = true;
            this.dgvList.RowHeadersVisible = false;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvList.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvList.RowTemplate.Height = 24;
            this.dgvList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvList.Size = new System.Drawing.Size(916, 264);
            this.dgvList.TabIndex = 29;
            // 
            // dgvtxtServerName
            // 
            this.dgvtxtServerName.DataPropertyName = "ServerName";
            this.dgvtxtServerName.HeaderText = "ServerName";
            this.dgvtxtServerName.Name = "dgvtxtServerName";
            this.dgvtxtServerName.ReadOnly = true;
            this.dgvtxtServerName.Visible = false;
            // 
            // dgvtxtLINETYPE
            // 
            this.dgvtxtLINETYPE.DataPropertyName = "LineType";
            this.dgvtxtLINETYPE.HeaderText = "Line Type";
            this.dgvtxtLINETYPE.Name = "dgvtxtLINETYPE";
            this.dgvtxtLINETYPE.ReadOnly = true;
            this.dgvtxtLINETYPE.Width = 150;
            // 
            // dgvtxtLineID
            // 
            this.dgvtxtLineID.DataPropertyName = "LineID";
            this.dgvtxtLineID.HeaderText = "Line ID";
            this.dgvtxtLineID.Name = "dgvtxtLineID";
            this.dgvtxtLineID.ReadOnly = true;
            this.dgvtxtLineID.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvtxtLineID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dgvtxtLineID.Width = 120;
            // 
            // dgvtxtResult
            // 
            this.dgvtxtResult.DataPropertyName = "Result";
            this.dgvtxtResult.HeaderText = "Result";
            this.dgvtxtResult.Name = "dgvtxtResult";
            this.dgvtxtResult.ReadOnly = true;
            this.dgvtxtResult.Width = 80;
            // 
            // dgvtxtResultDesc
            // 
            this.dgvtxtResultDesc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvtxtResultDesc.DataPropertyName = "ResultDesc";
            this.dgvtxtResultDesc.HeaderText = "Result Description";
            this.dgvtxtResultDesc.Name = "dgvtxtResultDesc";
            this.dgvtxtResultDesc.ReadOnly = true;
            // 
            // gbxLineType
            // 
            this.gbxLineType.Controls.Add(this.chklsbLineType);
            this.gbxLineType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxLineType.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbxLineType.Location = new System.Drawing.Point(23, 123);
            this.gbxLineType.Name = "gbxLineType";
            this.gbxLineType.Size = new System.Drawing.Size(922, 198);
            this.gbxLineType.TabIndex = 8;
            this.gbxLineType.TabStop = false;
            this.gbxLineType.Text = "LineType List";
            // 
            // chklsbLineType
            // 
            this.chklsbLineType.CheckOnClick = true;
            this.chklsbLineType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chklsbLineType.FormattingEnabled = true;
            this.chklsbLineType.Location = new System.Drawing.Point(3, 24);
            this.chklsbLineType.Name = "chklsbLineType";
            this.chklsbLineType.Size = new System.Drawing.Size(916, 171);
            this.chklsbLineType.TabIndex = 0;
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.Black;
            this.btnSave.Location = new System.Drawing.Point(432, 41);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(96, 35);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 3;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Controls.Add(this.gbxResult, 1, 2);
            this.tlpBase.Controls.Add(this.gbxLineType, 1, 1);
            this.tlpBase.Controls.Add(this.gbxPassword, 1, 0);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 4;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Size = new System.Drawing.Size(968, 555);
            this.tlpBase.TabIndex = 1;
            // 
            // gbxPassword
            // 
            this.gbxPassword.Controls.Add(this.btnSave);
            this.gbxPassword.Controls.Add(this.txtNewPassword);
            this.gbxPassword.Controls.Add(this.txtOldPassword);
            this.gbxPassword.Controls.Add(this.lblNewPassword);
            this.gbxPassword.Controls.Add(this.lblOldPassword);
            this.gbxPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxPassword.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbxPassword.ForeColor = System.Drawing.Color.Blue;
            this.gbxPassword.Location = new System.Drawing.Point(23, 3);
            this.gbxPassword.Name = "gbxPassword";
            this.gbxPassword.Size = new System.Drawing.Size(922, 114);
            this.gbxPassword.TabIndex = 10;
            this.gbxPassword.TabStop = false;
            // 
            // txtNewPassword
            // 
            this.txtNewPassword.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNewPassword.Location = new System.Drawing.Point(174, 63);
            this.txtNewPassword.Name = "txtNewPassword";
            this.txtNewPassword.PasswordChar = '*';
            this.txtNewPassword.Size = new System.Drawing.Size(221, 27);
            this.txtNewPassword.TabIndex = 3;
            // 
            // txtOldPassword
            // 
            this.txtOldPassword.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOldPassword.Location = new System.Drawing.Point(174, 27);
            this.txtOldPassword.Name = "txtOldPassword";
            this.txtOldPassword.PasswordChar = '*';
            this.txtOldPassword.Size = new System.Drawing.Size(221, 27);
            this.txtOldPassword.TabIndex = 2;
            // 
            // lblNewPassword
            // 
            this.lblNewPassword.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewPassword.ForeColor = System.Drawing.Color.Black;
            this.lblNewPassword.Location = new System.Drawing.Point(43, 67);
            this.lblNewPassword.Name = "lblNewPassword";
            this.lblNewPassword.Size = new System.Drawing.Size(125, 19);
            this.lblNewPassword.TabIndex = 1;
            this.lblNewPassword.Text = "New Password：";
            this.lblNewPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblOldPassword
            // 
            this.lblOldPassword.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOldPassword.ForeColor = System.Drawing.Color.Black;
            this.lblOldPassword.Location = new System.Drawing.Point(43, 31);
            this.lblOldPassword.Name = "lblOldPassword";
            this.lblOldPassword.Size = new System.Drawing.Size(125, 19);
            this.lblOldPassword.TabIndex = 0;
            this.lblOldPassword.Text = "Old Password：";
            this.lblOldPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FormUserChangePassword
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(968, 586);
            this.Name = "FormUserChangePassword";
            this.Text = "FormUserChangePassword";
            this.Load += new System.EventHandler(this.FormUserChangePassword_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.gbxResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvList)).EndInit();
            this.gbxLineType.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.gbxPassword.ResumeLayout(false);
            this.gbxPassword.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbxResult;
        private System.Windows.Forms.GroupBox gbxLineType;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.GroupBox gbxPassword;
        private System.Windows.Forms.TextBox txtNewPassword;
        private System.Windows.Forms.TextBox txtOldPassword;
        private System.Windows.Forms.Label lblNewPassword;
        private System.Windows.Forms.Label lblOldPassword;
        private System.Windows.Forms.CheckedListBox chklsbLineType;
        private System.Windows.Forms.DataGridView dgvList;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtLINETYPE;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtLineID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtResultDesc;
    }
}