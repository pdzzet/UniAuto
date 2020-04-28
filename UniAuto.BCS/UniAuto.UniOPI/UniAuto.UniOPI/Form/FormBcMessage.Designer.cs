namespace UniOPI
{
    partial class FormBCSMessage
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
            this.tbcMessage = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tlpHistory = new System.Windows.Forms.TableLayoutPanel();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.dgvHisMessage = new System.Windows.Forms.DataGridView();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrxID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnOK = new System.Windows.Forms.Button();
            this.tmrShow = new System.Windows.Forms.Timer(this.components);
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.chk_AutoPop = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tbcMessage.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tlpHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHisMessage)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(745, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.chk_AutoPop);
            this.spcBase.Panel2.Controls.Add(this.btnOK);
            this.spcBase.Panel2.Controls.Add(this.tbcMessage);
            this.spcBase.Size = new System.Drawing.Size(805, 464);
            // 
            // tbcMessage
            // 
            this.tbcMessage.Controls.Add(this.tabPage1);
            this.tbcMessage.Controls.Add(this.tabPage2);
            this.tbcMessage.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbcMessage.Location = new System.Drawing.Point(5, 3);
            this.tbcMessage.Name = "tbcMessage";
            this.tbcMessage.SelectedIndex = 0;
            this.tbcMessage.Size = new System.Drawing.Size(797, 395);
            this.tbcMessage.TabIndex = 3;
            this.tbcMessage.SelectedIndexChanged += new System.EventHandler(this.tbcMessage_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.tabPage1.Controls.Add(this.txtMessage);
            this.tabPage1.Location = new System.Drawing.Point(4, 30);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(789, 361);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Message";
            // 
            // txtMessage
            // 
            this.txtMessage.AcceptsReturn = true;
            this.txtMessage.AcceptsTab = true;
            this.txtMessage.BackColor = System.Drawing.Color.White;
            this.txtMessage.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessage.ForeColor = System.Drawing.Color.Blue;
            this.txtMessage.Location = new System.Drawing.Point(3, 3);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.Size = new System.Drawing.Size(780, 355);
            this.txtMessage.TabIndex = 0;
            this.txtMessage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tlpHistory);
            this.tabPage2.Location = new System.Drawing.Point(4, 30);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(789, 361);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "History";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tlpHistory
            // 
            this.tlpHistory.ColumnCount = 1;
            this.tlpHistory.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHistory.Controls.Add(this.txtMsg, 0, 0);
            this.tlpHistory.Controls.Add(this.dgvHisMessage, 0, 1);
            this.tlpHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHistory.Location = new System.Drawing.Point(3, 3);
            this.tlpHistory.Name = "tlpHistory";
            this.tlpHistory.RowCount = 2;
            this.tlpHistory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 31.61094F));
            this.tlpHistory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 68.38906F));
            this.tlpHistory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpHistory.Size = new System.Drawing.Size(783, 355);
            this.tlpHistory.TabIndex = 2;
            // 
            // txtMsg
            // 
            this.txtMsg.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.txtMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMsg.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtMsg.Location = new System.Drawing.Point(3, 3);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ReadOnly = true;
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMsg.Size = new System.Drawing.Size(777, 106);
            this.txtMsg.TabIndex = 2;
            // 
            // dgvHisMessage
            // 
            this.dgvHisMessage.AllowUserToAddRows = false;
            this.dgvHisMessage.AllowUserToResizeRows = false;
            this.dgvHisMessage.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvHisMessage.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvHisMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHisMessage.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTime,
            this.colMessage,
            this.colTrxID});
            this.dgvHisMessage.EnableHeadersVisualStyles = false;
            this.dgvHisMessage.Location = new System.Drawing.Point(3, 115);
            this.dgvHisMessage.MultiSelect = false;
            this.dgvHisMessage.Name = "dgvHisMessage";
            this.dgvHisMessage.ReadOnly = true;
            this.dgvHisMessage.RowHeadersWidth = 24;
            this.dgvHisMessage.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvHisMessage.RowTemplate.Height = 24;
            this.dgvHisMessage.Size = new System.Drawing.Size(777, 237);
            this.dgvHisMessage.TabIndex = 1;
            this.dgvHisMessage.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvHisMessage_CellClick);
            // 
            // colTime
            // 
            this.colTime.DataPropertyName = "UPDATETIME";
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.colTime.DefaultCellStyle = dataGridViewCellStyle2;
            this.colTime.HeaderText = "Time";
            this.colTime.MinimumWidth = 150;
            this.colTime.Name = "colTime";
            this.colTime.ReadOnly = true;
            this.colTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colTime.Width = 150;
            // 
            // colMessage
            // 
            this.colMessage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colMessage.DataPropertyName = "TERMINALTEXT";
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F);
            this.colMessage.DefaultCellStyle = dataGridViewCellStyle3;
            this.colMessage.HeaderText = "Message";
            this.colMessage.Name = "colMessage";
            this.colMessage.ReadOnly = true;
            this.colMessage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colMessage.Width = 1000;
            // 
            // colTrxID
            // 
            this.colTrxID.DataPropertyName = "TRANSACTIONID";
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 11.25F);
            this.colTrxID.DefaultCellStyle = dataGridViewCellStyle4;
            this.colTrxID.HeaderText = "Transaction ID";
            this.colTrxID.Name = "colTrxID";
            this.colTrxID.ReadOnly = true;
            this.colTrxID.Width = 132;
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(350, 400);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tmrShow
            // 
            this.tmrShow.Interval = 1000;
            this.tmrShow.Tick += new System.EventHandler(this.tmrShow_Tick);
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 1000;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // chk_AutoPop
            // 
            this.chk_AutoPop.AutoSize = true;
            this.chk_AutoPop.Checked = true;
            this.chk_AutoPop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_AutoPop.Font = new System.Drawing.Font("NSimSun", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chk_AutoPop.Location = new System.Drawing.Point(467, 410);
            this.chk_AutoPop.Name = "chk_AutoPop";
            this.chk_AutoPop.Size = new System.Drawing.Size(86, 18);
            this.chk_AutoPop.TabIndex = 5;
            this.chk_AutoPop.Text = "自动弹窗";
            this.chk_AutoPop.UseVisualStyleBackColor = true;
            this.chk_AutoPop.CheckedChanged += new System.EventHandler(this.chk_AutoPop_CheckedChanged);
            // 
            // FormBCSMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(805, 464);
            this.Name = "FormBCSMessage";
            this.Text = "   ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormBCSMessage_FormClosing);
            this.spcBase.Panel2.ResumeLayout(false);
            this.spcBase.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tbcMessage.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tlpHistory.ResumeLayout(false);
            this.tlpHistory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHisMessage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tbcMessage;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dgvHisMessage;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Timer tmrShow;
        private System.Windows.Forms.TableLayoutPanel tlpHistory;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Timer tmrRefresh;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMessage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrxID;
        private System.Windows.Forms.CheckBox chk_AutoPop;
    }
}