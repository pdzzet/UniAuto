namespace UniOPI
{
    partial class FormForceCleanOutReport
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
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlBCCommand = new System.Windows.Forms.Panel();
            this.pnlUserID = new System.Windows.Forms.Panel();
            this.lblUserID = new System.Windows.Forms.Label();
            this.txtUserID = new System.Windows.Forms.TextBox();
            this.pnlLineName = new System.Windows.Forms.Panel();
            this.lblBCCommand = new System.Windows.Forms.Label();
            this.txtBCCommand = new System.Windows.Forms.TextBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBCStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEQStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.pnlBCCommand.SuspendLayout();
            this.pnlUserID.SuspendLayout();
            this.pnlLineName.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(419, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(479, 494);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 3;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Controls.Add(this.pnlButton, 1, 2);
            this.tlpBase.Controls.Add(this.pnlBCCommand, 1, 0);
            this.tlpBase.Controls.Add(this.dgvData, 1, 1);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 3;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 82F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 322F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Size = new System.Drawing.Size(479, 463);
            this.tlpBase.TabIndex = 0;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnClose);
            this.pnlButton.Location = new System.Drawing.Point(23, 407);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(433, 44);
            this.pnlButton.TabIndex = 22;
            // 
            // btnClose
            // 
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(164, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 39);
            this.btnClose.TabIndex = 12;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // pnlBCCommand
            // 
            this.pnlBCCommand.Controls.Add(this.pnlUserID);
            this.pnlBCCommand.Controls.Add(this.pnlLineName);
            this.pnlBCCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBCCommand.Location = new System.Drawing.Point(23, 3);
            this.pnlBCCommand.Name = "pnlBCCommand";
            this.pnlBCCommand.Size = new System.Drawing.Size(433, 76);
            this.pnlBCCommand.TabIndex = 23;
            // 
            // pnlUserID
            // 
            this.pnlUserID.Controls.Add(this.lblUserID);
            this.pnlUserID.Controls.Add(this.txtUserID);
            this.pnlUserID.Location = new System.Drawing.Point(7, 40);
            this.pnlUserID.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlUserID.Name = "pnlUserID";
            this.pnlUserID.Size = new System.Drawing.Size(420, 30);
            this.pnlUserID.TabIndex = 9;
            this.pnlUserID.Tag = "LOTNAME";
            // 
            // lblUserID
            // 
            this.lblUserID.BackColor = System.Drawing.Color.Black;
            this.lblUserID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserID.ForeColor = System.Drawing.Color.White;
            this.lblUserID.Location = new System.Drawing.Point(0, 2);
            this.lblUserID.Name = "lblUserID";
            this.lblUserID.Size = new System.Drawing.Size(135, 25);
            this.lblUserID.TabIndex = 5;
            this.lblUserID.Text = "User ID";
            this.lblUserID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtUserID
            // 
            this.txtUserID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtUserID.Location = new System.Drawing.Point(135, 3);
            this.txtUserID.Name = "txtUserID";
            this.txtUserID.ReadOnly = true;
            this.txtUserID.Size = new System.Drawing.Size(270, 25);
            this.txtUserID.TabIndex = 0;
            // 
            // pnlLineName
            // 
            this.pnlLineName.Controls.Add(this.lblBCCommand);
            this.pnlLineName.Controls.Add(this.txtBCCommand);
            this.pnlLineName.Location = new System.Drawing.Point(7, 8);
            this.pnlLineName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlLineName.Name = "pnlLineName";
            this.pnlLineName.Size = new System.Drawing.Size(420, 30);
            this.pnlLineName.TabIndex = 8;
            this.pnlLineName.Tag = "LOTNAME";
            // 
            // lblBCCommand
            // 
            this.lblBCCommand.BackColor = System.Drawing.Color.Black;
            this.lblBCCommand.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBCCommand.ForeColor = System.Drawing.Color.White;
            this.lblBCCommand.Location = new System.Drawing.Point(0, 2);
            this.lblBCCommand.Name = "lblBCCommand";
            this.lblBCCommand.Size = new System.Drawing.Size(135, 25);
            this.lblBCCommand.TabIndex = 5;
            this.lblBCCommand.Text = "BCS Send Status";
            this.lblBCCommand.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBCCommand
            // 
            this.txtBCCommand.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtBCCommand.Location = new System.Drawing.Point(135, 3);
            this.txtBCCommand.Name = "txtBCCommand";
            this.txtBCCommand.ReadOnly = true;
            this.txtBCCommand.Size = new System.Drawing.Size(270, 25);
            this.txtBCCommand.TabIndex = 0;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvData.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLocalNo,
            this.colBCStatus,
            this.colEQStatus});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(23, 85);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(433, 316);
            this.dgvData.TabIndex = 24;
            // 
            // colLocalNo
            // 
            this.colLocalNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colLocalNo.HeaderText = "Local No";
            this.colLocalNo.Name = "colLocalNo";
            this.colLocalNo.ReadOnly = true;
            this.colLocalNo.Width = 150;
            // 
            // colBCStatus
            // 
            this.colBCStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colBCStatus.HeaderText = "BC Status";
            this.colBCStatus.Name = "colBCStatus";
            this.colBCStatus.ReadOnly = true;
            // 
            // colEQStatus
            // 
            this.colEQStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colEQStatus.HeaderText = "EQ Status";
            this.colEQStatus.Name = "colEQStatus";
            this.colEQStatus.ReadOnly = true;
            // 
            // FormForceCleanOutReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 494);
            this.Name = "FormForceCleanOutReport";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormForceCleanOutReport_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.pnlBCCommand.ResumeLayout(false);
            this.pnlUserID.ResumeLayout(false);
            this.pnlUserID.PerformLayout();
            this.pnlLineName.ResumeLayout(false);
            this.pnlLineName.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlBCCommand;
        private System.Windows.Forms.Panel pnlLineName;
        private System.Windows.Forms.Label lblBCCommand;
        private System.Windows.Forms.TextBox txtBCCommand;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Panel pnlUserID;
        private System.Windows.Forms.Label lblUserID;
        private System.Windows.Forms.TextBox txtUserID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBCStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEQStatus;
    }
}