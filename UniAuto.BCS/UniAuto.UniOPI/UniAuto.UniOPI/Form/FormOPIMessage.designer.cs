namespace UniOPI
{
    partial class FormOPIMessage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOPIMessage));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpLayout = new System.Windows.Forms.TableLayoutPanel();
            this.tlpCurMsg = new System.Windows.Forms.TableLayoutPanel();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblICON = new System.Windows.Forms.Label();
            this.imgICON = new System.Windows.Forms.ImageList(this.components);
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.grbHistory = new System.Windows.Forms.GroupBox();
            this.dgvMsg = new System.Windows.Forms.DataGridView();
            this.colMessageBoxIcon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStackTrace = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCaption = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpLayout.SuspendLayout();
            this.tlpCurMsg.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.grbHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMsg)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(630, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpLayout);
            this.spcBase.Size = new System.Drawing.Size(690, 418);
            // 
            // tmrBaseRefresh
            // 
            this.tmrBaseRefresh.Tick += new System.EventHandler(this.tmrBaseRefresh_Tick);
            // 
            // tlpLayout
            // 
            this.tlpLayout.BackColor = System.Drawing.Color.Transparent;
            this.tlpLayout.ColumnCount = 1;
            this.tlpLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLayout.Controls.Add(this.tlpCurMsg, 0, 0);
            this.tlpLayout.Controls.Add(this.pnlButton, 0, 2);
            this.tlpLayout.Controls.Add(this.grbHistory, 0, 1);
            this.tlpLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLayout.Location = new System.Drawing.Point(0, 0);
            this.tlpLayout.Name = "tlpLayout";
            this.tlpLayout.RowCount = 3;
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 119F));
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpLayout.Size = new System.Drawing.Size(690, 387);
            this.tlpLayout.TabIndex = 1;
            // 
            // tlpCurMsg
            // 
            this.tlpCurMsg.BackColor = System.Drawing.Color.Transparent;
            this.tlpCurMsg.ColumnCount = 2;
            this.tlpCurMsg.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
            this.tlpCurMsg.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCurMsg.Controls.Add(this.txtMsg, 1, 0);
            this.tlpCurMsg.Controls.Add(this.panel1, 0, 0);
            this.tlpCurMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCurMsg.Location = new System.Drawing.Point(3, 3);
            this.tlpCurMsg.Name = "tlpCurMsg";
            this.tlpCurMsg.RowCount = 1;
            this.tlpCurMsg.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCurMsg.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 113F));
            this.tlpCurMsg.Size = new System.Drawing.Size(684, 113);
            this.tlpCurMsg.TabIndex = 1;
            // 
            // txtMsg
            // 
            this.txtMsg.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.txtMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMsg.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtMsg.Location = new System.Drawing.Point(96, 3);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ReadOnly = true;
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMsg.Size = new System.Drawing.Size(585, 107);
            this.txtMsg.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblICON);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(87, 63);
            this.panel1.TabIndex = 0;
            // 
            // lblICON
            // 
            this.lblICON.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblICON.ImageList = this.imgICON;
            this.lblICON.Location = new System.Drawing.Point(0, 0);
            this.lblICON.Name = "lblICON";
            this.lblICON.Size = new System.Drawing.Size(87, 63);
            this.lblICON.TabIndex = 1;
            // 
            // imgICON
            // 
            this.imgICON.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgICON.ImageStream")));
            this.imgICON.TransparentColor = System.Drawing.Color.Transparent;
            this.imgICON.Images.SetKeyName(0, "Warning-128.png");
            this.imgICON.Images.SetKeyName(1, "Error-128.png");
            this.imgICON.Images.SetKeyName(2, "Question-128.png");
            this.imgICON.Images.SetKeyName(3, "Information-128.png");
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnOK);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(3, 345);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(684, 39);
            this.pnlButton.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(279, 4);
            this.btnOK.Name = "btnOK";
            this.btnOK.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.TabIndex = 14;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // grbHistory
            // 
            this.grbHistory.Controls.Add(this.dgvMsg);
            this.grbHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbHistory.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbHistory.Location = new System.Drawing.Point(3, 122);
            this.grbHistory.Name = "grbHistory";
            this.grbHistory.Size = new System.Drawing.Size(684, 217);
            this.grbHistory.TabIndex = 0;
            this.grbHistory.TabStop = false;
            // 
            // dgvMsg
            // 
            this.dgvMsg.AllowUserToAddRows = false;
            this.dgvMsg.AllowUserToDeleteRows = false;
            this.dgvMsg.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            this.dgvMsg.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvMsg.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvMsg.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvMsg.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMsg.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMessageBoxIcon,
            this.colDateTime,
            this.colMessage,
            this.colStackTrace,
            this.colCaption});
            this.dgvMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMsg.Location = new System.Drawing.Point(3, 21);
            this.dgvMsg.Name = "dgvMsg";
            this.dgvMsg.ReadOnly = true;
            this.dgvMsg.RowHeadersVisible = false;
            this.dgvMsg.RowTemplate.Height = 24;
            this.dgvMsg.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvMsg.Size = new System.Drawing.Size(678, 193);
            this.dgvMsg.TabIndex = 13;
            this.dgvMsg.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMsg_CellClick);
            this.dgvMsg.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvMsg_CellFormatting);
            // 
            // colMessageBoxIcon
            // 
            this.colMessageBoxIcon.HeaderText = "Type";
            this.colMessageBoxIcon.Name = "colMessageBoxIcon";
            this.colMessageBoxIcon.ReadOnly = true;
            this.colMessageBoxIcon.Width = 90;
            // 
            // colDateTime
            // 
            this.colDateTime.HeaderText = "DateTime";
            this.colDateTime.Name = "colDateTime";
            this.colDateTime.ReadOnly = true;
            this.colDateTime.Width = 175;
            // 
            // colMessage
            // 
            this.colMessage.HeaderText = "Message";
            this.colMessage.Name = "colMessage";
            this.colMessage.ReadOnly = true;
            this.colMessage.Width = 450;
            // 
            // colStackTrace
            // 
            this.colStackTrace.HeaderText = "StackTrace";
            this.colStackTrace.Name = "colStackTrace";
            this.colStackTrace.ReadOnly = true;
            this.colStackTrace.Visible = false;
            // 
            // colCaption
            // 
            this.colCaption.HeaderText = "Caption";
            this.colCaption.Name = "colCaption";
            this.colCaption.ReadOnly = true;
            this.colCaption.Visible = false;
            // 
            // FormOPIMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 418);
            this.Name = "FormOPIMessage";
            this.Text = "  ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormShowMessage_FormClosing);
            this.Load += new System.EventHandler(this.FormShowMessage_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpLayout.ResumeLayout(false);
            this.tlpCurMsg.ResumeLayout(false);
            this.tlpCurMsg.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.grbHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMsg)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpLayout;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TableLayoutPanel tlpCurMsg;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblICON;
        private System.Windows.Forms.GroupBox grbHistory;
        protected System.Windows.Forms.DataGridView dgvMsg;
        private System.Windows.Forms.ImageList imgICON;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMessageBoxIcon;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMessage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStackTrace;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCaption;
    }
}