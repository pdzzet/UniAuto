namespace UniOPI
{
    partial class FormClientStatus
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.DISCONNECT = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.USERID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.USERGROUP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LOGINTIME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LOGINIP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LOGINSERVERNAME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.pnlButtom = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tlpBase.SuspendLayout();
            this.pnlButtom.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1030, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(1090, 548);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dgvData);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1084, 456);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DISCONNECT,
            this.USERID,
            this.USERGROUP,
            this.LOGINTIME,
            this.LOGINIP,
            this.LOGINSERVERNAME});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 18);
            this.dgvData.Name = "dgvData";
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvData.Size = new System.Drawing.Size(1078, 435);
            this.dgvData.TabIndex = 13;
            this.dgvData.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dgvData_CellBeginEdit);
            // 
            // DISCONNECT
            // 
            this.DISCONNECT.DataPropertyName = "DISCONNECT";
            this.DISCONNECT.HeaderText = "Disconnect";
            this.DISCONNECT.Name = "DISCONNECT";
            this.DISCONNECT.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.DISCONNECT.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // USERID
            // 
            this.USERID.DataPropertyName = "USERID";
            this.USERID.HeaderText = "User ID";
            this.USERID.Name = "USERID";
            this.USERID.Width = 200;
            // 
            // USERGROUP
            // 
            this.USERGROUP.DataPropertyName = "USERGROUP";
            this.USERGROUP.HeaderText = "User Group";
            this.USERGROUP.Name = "USERGROUP";
            this.USERGROUP.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.USERGROUP.Width = 200;
            // 
            // LOGINTIME
            // 
            this.LOGINTIME.DataPropertyName = "LOGINTIME";
            this.LOGINTIME.HeaderText = "Login Time";
            this.LOGINTIME.Name = "LOGINTIME";
            this.LOGINTIME.Width = 200;
            // 
            // LOGINIP
            // 
            this.LOGINIP.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.LOGINIP.DataPropertyName = "LOGINSERVERIP";
            this.LOGINIP.HeaderText = "Login IP";
            this.LOGINIP.Name = "LOGINIP";
            this.LOGINIP.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // LOGINSERVERNAME
            // 
            this.LOGINSERVERNAME.DataPropertyName = "LOGINSERVERNAME";
            this.LOGINSERVERNAME.HeaderText = "Login Name";
            this.LOGINSERVERNAME.Name = "LOGINSERVERNAME";
            this.LOGINSERVERNAME.Width = 250;
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.groupBox1, 0, 0);
            this.tlpBase.Controls.Add(this.pnlButtom, 0, 1);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpBase.Size = new System.Drawing.Size(1090, 517);
            this.tlpBase.TabIndex = 19;
            // 
            // pnlButtom
            // 
            this.pnlButtom.Controls.Add(this.btnClose);
            this.pnlButtom.Controls.Add(this.btnSend);
            this.pnlButtom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButtom.Location = new System.Drawing.Point(3, 465);
            this.pnlButtom.Name = "pnlButtom";
            this.pnlButtom.Size = new System.Drawing.Size(1084, 49);
            this.pnlButtom.TabIndex = 19;
            // 
            // btnClose
            // 
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(525, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnClose.Size = new System.Drawing.Size(123, 35);
            this.btnClose.TabIndex = 25;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSend
            // 
            this.btnSend.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSend.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.Location = new System.Drawing.Point(396, 5);
            this.btnSend.Name = "btnSend";
            this.btnSend.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnSend.Size = new System.Drawing.Size(123, 35);
            this.btnSend.TabIndex = 24;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // FormClientStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1090, 548);
            this.ControlBox = true;
            this.Name = "FormClientStatus";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormClientStatus_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tlpBase.ResumeLayout(false);
            this.pnlButtom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel pnlButtom;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.DataGridViewCheckBoxColumn DISCONNECT;
        private System.Windows.Forms.DataGridViewTextBoxColumn USERID;
        private System.Windows.Forms.DataGridViewTextBoxColumn USERGROUP;
        private System.Windows.Forms.DataGridViewTextBoxColumn LOGINTIME;
        private System.Windows.Forms.DataGridViewTextBoxColumn LOGINIP;
        private System.Windows.Forms.DataGridViewTextBoxColumn LOGINSERVERNAME;
        private System.Windows.Forms.Button btnClose;
    }
}