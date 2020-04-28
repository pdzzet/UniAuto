namespace UniOPI
{
    partial class FormRobotCommandHis
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.dgvMessage = new System.Windows.Forms.DataGridView();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.colDateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMessageType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrxID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMessage)).BeginInit();
            this.pnlButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(921, 30);
            this.lblCaption.Text = "Robot Command History";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(981, 508);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.txtMsg, 0, 0);
            this.tlpBase.Controls.Add(this.dgvMessage, 0, 1);
            this.tlpBase.Controls.Add(this.pnlButton, 0, 2);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 3;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Size = new System.Drawing.Size(981, 477);
            this.tlpBase.TabIndex = 4;
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
            this.txtMsg.Size = new System.Drawing.Size(975, 119);
            this.txtMsg.TabIndex = 2;
            // 
            // dgvMessage
            // 
            this.dgvMessage.AllowUserToAddRows = false;
            this.dgvMessage.AllowUserToResizeRows = false;
            this.dgvMessage.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvMessage.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMessage.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDateTime,
            this.colMessage,
            this.colMessageType,
            this.colTrxID});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 12.75F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvMessage.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMessage.EnableHeadersVisualStyles = false;
            this.dgvMessage.Location = new System.Drawing.Point(3, 128);
            this.dgvMessage.MultiSelect = false;
            this.dgvMessage.Name = "dgvMessage";
            this.dgvMessage.ReadOnly = true;
            this.dgvMessage.RowHeadersWidth = 24;
            this.dgvMessage.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvMessage.RowTemplate.Height = 24;
            this.dgvMessage.Size = new System.Drawing.Size(975, 290);
            this.dgvMessage.TabIndex = 1;
            this.dgvMessage.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMessage_CellClick);
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnOK);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(3, 424);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(975, 50);
            this.pnlButton.TabIndex = 3;
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(448, 12);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // colDateTime
            // 
            this.colDateTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDateTime.DataPropertyName = "UPDATETIME";
            this.colDateTime.HeaderText = "DateTime";
            this.colDateTime.Name = "colDateTime";
            this.colDateTime.ReadOnly = true;
            this.colDateTime.Width = 200;
            // 
            // colMessage
            // 
            this.colMessage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colMessage.DataPropertyName = "TERMINALTEXT";
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11.25F);
            this.colMessage.DefaultCellStyle = dataGridViewCellStyle2;
            this.colMessage.HeaderText = "Message";
            this.colMessage.Name = "colMessage";
            this.colMessage.ReadOnly = true;
            this.colMessage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colMessage.Width = 1000;
            // 
            // colMessageType
            // 
            this.colMessageType.DataPropertyName = "MESSAGETYPE";
            this.colMessageType.HeaderText = "Message Type";
            this.colMessageType.Name = "colMessageType";
            this.colMessageType.ReadOnly = true;
            this.colMessageType.Width = 132;
            // 
            // colTrxID
            // 
            this.colTrxID.DataPropertyName = "TRANSACTIONID";
            this.colTrxID.HeaderText = "Trx ID";
            this.colTrxID.Name = "colTrxID";
            this.colTrxID.ReadOnly = true;
            this.colTrxID.Width = 73;
            // 
            // FormRobotCommandHis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(981, 508);
            this.Name = "FormRobotCommandHis";
            this.Text = "   ";
            this.Load += new System.EventHandler(this.FormRobotCommandHis_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.tlpBase.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMessage)).EndInit();
            this.pnlButton.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.DataGridView dgvMessage;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMessage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMessageType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrxID;
    }
}