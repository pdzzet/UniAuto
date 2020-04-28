namespace UniOPI
{
    partial class FormCimMessage
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.lblLocalNode = new System.Windows.Forms.Label();
            this.cboLocalNode = new System.Windows.Forms.ComboBox();
            this.lblMsgText = new System.Windows.Forms.Label();
            this.txtMsgText = new System.Windows.Forms.TextBox();
            this.dgvMessage = new System.Windows.Forms.DataGridView();
            this.colDateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMessageID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTouchPanelNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMessageText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colClear = new System.Windows.Forms.DataGridViewButtonColumn();
            this.grbCIMMessage = new System.Windows.Forms.GroupBox();
            this.tlpMsg = new System.Windows.Forms.TableLayoutPanel();
            this.pnlMsg = new System.Windows.Forms.Panel();
            this.txtTouchPanelNo = new System.Windows.Forms.TextBox();
            this.lblTouchPanelNo = new System.Windows.Forms.Label();
            this.btnSend = new System.Windows.Forms.Button();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.pnlNode = new System.Windows.Forms.Panel();
            this.btnQuery = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.tbxDays = new System.Windows.Forms.TextBox();
            this.lbdDays = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMessage)).BeginInit();
            this.grbCIMMessage.SuspendLayout();
            this.tlpMsg.SuspendLayout();
            this.pnlMsg.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.pnlNode.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1078, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(1138, 622);
            // 
            // lblLocalNode
            // 
            this.lblLocalNode.BackColor = System.Drawing.Color.Transparent;
            this.lblLocalNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLocalNode.Location = new System.Drawing.Point(28, 8);
            this.lblLocalNode.Name = "lblLocalNode";
            this.lblLocalNode.Size = new System.Drawing.Size(69, 21);
            this.lblLocalNode.TabIndex = 0;
            this.lblLocalNode.Text = "Local";
            // 
            // cboLocalNode
            // 
            this.cboLocalNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocalNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboLocalNode.FormattingEnabled = true;
            this.cboLocalNode.Location = new System.Drawing.Point(82, 3);
            this.cboLocalNode.Name = "cboLocalNode";
            this.cboLocalNode.Size = new System.Drawing.Size(252, 29);
            this.cboLocalNode.TabIndex = 1;
            this.cboLocalNode.SelectedIndexChanged += new System.EventHandler(this.cboLocalNode_SelectedIndexChanged);
            // 
            // lblMsgText
            // 
            this.lblMsgText.AutoSize = true;
            this.lblMsgText.BackColor = System.Drawing.Color.Transparent;
            this.lblMsgText.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMsgText.Location = new System.Drawing.Point(40, 19);
            this.lblMsgText.Name = "lblMsgText";
            this.lblMsgText.Size = new System.Drawing.Size(71, 21);
            this.lblMsgText.TabIndex = 4;
            this.lblMsgText.Text = "Message";
            // 
            // txtMsgText
            // 
            this.txtMsgText.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMsgText.Location = new System.Drawing.Point(117, 16);
            this.txtMsgText.Name = "txtMsgText";
            this.txtMsgText.Size = new System.Drawing.Size(895, 28);
            this.txtMsgText.TabIndex = 5;
            // 
            // dgvMessage
            // 
            this.dgvMessage.AllowUserToAddRows = false;
            this.dgvMessage.AllowUserToDeleteRows = false;
            this.dgvMessage.AllowUserToResizeRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvMessage.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvMessage.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvMessage.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMessage.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDateTime,
            this.colMessageID,
            this.colTouchPanelNo,
            this.colMessageText,
            this.colClear});
            this.dgvMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMessage.Location = new System.Drawing.Point(3, 104);
            this.dgvMessage.MultiSelect = false;
            this.dgvMessage.Name = "dgvMessage";
            this.dgvMessage.ReadOnly = true;
            this.dgvMessage.RowHeadersVisible = false;
            this.dgvMessage.RowTemplate.Height = 24;
            this.dgvMessage.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMessage.Size = new System.Drawing.Size(1020, 373);
            this.dgvMessage.TabIndex = 9;
            this.dgvMessage.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMessage_CellClick);
            // 
            // colDateTime
            // 
            this.colDateTime.HeaderText = "Date Time";
            this.colDateTime.Name = "colDateTime";
            this.colDateTime.ReadOnly = true;
            this.colDateTime.Width = 150;
            // 
            // colMessageID
            // 
            this.colMessageID.HeaderText = "Message ID";
            this.colMessageID.Name = "colMessageID";
            this.colMessageID.ReadOnly = true;
            this.colMessageID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colMessageID.Width = 150;
            // 
            // colTouchPanelNo
            // 
            this.colTouchPanelNo.HeaderText = "Touch Panel No";
            this.colTouchPanelNo.Name = "colTouchPanelNo";
            this.colTouchPanelNo.ReadOnly = true;
            this.colTouchPanelNo.Width = 140;
            // 
            // colMessageText
            // 
            this.colMessageText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colMessageText.HeaderText = "Message";
            this.colMessageText.Name = "colMessageText";
            this.colMessageText.ReadOnly = true;
            // 
            // colClear
            // 
            this.colClear.HeaderText = "Clear";
            this.colClear.Name = "colClear";
            this.colClear.ReadOnly = true;
            this.colClear.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colClear.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colClear.Width = 150;
            // 
            // grbCIMMessage
            // 
            this.grbCIMMessage.Controls.Add(this.tlpMsg);
            this.grbCIMMessage.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbCIMMessage.ForeColor = System.Drawing.Color.Black;
            this.grbCIMMessage.Location = new System.Drawing.Point(53, 64);
            this.grbCIMMessage.Name = "grbCIMMessage";
            this.grbCIMMessage.Size = new System.Drawing.Size(1032, 502);
            this.grbCIMMessage.TabIndex = 10;
            this.grbCIMMessage.TabStop = false;
            // 
            // tlpMsg
            // 
            this.tlpMsg.ColumnCount = 1;
            this.tlpMsg.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMsg.Controls.Add(this.pnlMsg, 0, 0);
            this.tlpMsg.Controls.Add(this.dgvMessage, 0, 1);
            this.tlpMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMsg.Location = new System.Drawing.Point(3, 19);
            this.tlpMsg.Name = "tlpMsg";
            this.tlpMsg.RowCount = 2;
            this.tlpMsg.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 101F));
            this.tlpMsg.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMsg.Size = new System.Drawing.Size(1026, 480);
            this.tlpMsg.TabIndex = 11;
            // 
            // pnlMsg
            // 
            this.pnlMsg.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlMsg.Controls.Add(this.txtTouchPanelNo);
            this.pnlMsg.Controls.Add(this.lblTouchPanelNo);
            this.pnlMsg.Controls.Add(this.btnSend);
            this.pnlMsg.Controls.Add(this.lblMsgText);
            this.pnlMsg.Controls.Add(this.txtMsgText);
            this.pnlMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMsg.ForeColor = System.Drawing.Color.Black;
            this.pnlMsg.Location = new System.Drawing.Point(3, 3);
            this.pnlMsg.Name = "pnlMsg";
            this.pnlMsg.Size = new System.Drawing.Size(1020, 95);
            this.pnlMsg.TabIndex = 10;
            // 
            // txtTouchPanelNo
            // 
            this.txtTouchPanelNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTouchPanelNo.Location = new System.Drawing.Point(117, 49);
            this.txtTouchPanelNo.Name = "txtTouchPanelNo";
            this.txtTouchPanelNo.Size = new System.Drawing.Size(787, 28);
            this.txtTouchPanelNo.TabIndex = 11;
            // 
            // lblTouchPanelNo
            // 
            this.lblTouchPanelNo.AutoSize = true;
            this.lblTouchPanelNo.BackColor = System.Drawing.Color.Transparent;
            this.lblTouchPanelNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTouchPanelNo.Location = new System.Drawing.Point(3, 52);
            this.lblTouchPanelNo.Name = "lblTouchPanelNo";
            this.lblTouchPanelNo.Size = new System.Drawing.Size(116, 21);
            this.lblTouchPanelNo.TabIndex = 10;
            this.lblTouchPanelNo.Text = "Touch Panel No";
            // 
            // btnSend
            // 
            this.btnSend.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSend.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.Location = new System.Drawing.Point(935, 50);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(77, 30);
            this.btnSend.TabIndex = 9;
            this.btnSend.Text = "Set";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.Button_Click);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 3;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpBase.Controls.Add(this.grbCIMMessage, 1, 2);
            this.tlpBase.Controls.Add(this.pnlNode, 1, 1);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 4;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpBase.Size = new System.Drawing.Size(1138, 591);
            this.tlpBase.TabIndex = 11;
            // 
            // pnlNode
            // 
            this.pnlNode.Controls.Add(this.lbdDays);
            this.pnlNode.Controls.Add(this.tbxDays);
            this.pnlNode.Controls.Add(this.btnClearAll);
            this.pnlNode.Controls.Add(this.cboLocalNode);
            this.pnlNode.Controls.Add(this.btnQuery);
            this.pnlNode.Controls.Add(this.lblLocalNode);
            this.pnlNode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNode.Location = new System.Drawing.Point(53, 23);
            this.pnlNode.Name = "pnlNode";
            this.pnlNode.Size = new System.Drawing.Size(1032, 35);
            this.pnlNode.TabIndex = 11;
            // 
            // btnQuery
            // 
            this.btnQuery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.Location = new System.Drawing.Point(348, 2);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(97, 30);
            this.btnQuery.TabIndex = 10;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.Button_Click);
            // 
            // btnClearAll
            // 
            this.btnClearAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClearAll.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearAll.Location = new System.Drawing.Point(468, 2);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(97, 30);
            this.btnClearAll.TabIndex = 11;
            this.btnClearAll.Text = "Clear_All";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.Button_Click);
            // 
            // tbxDays
            // 
            this.tbxDays.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbxDays.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.tbxDays.Location = new System.Drawing.Point(571, 8);
            this.tbxDays.MaxLength = 1;
            this.tbxDays.Name = "tbxDays";
            this.tbxDays.Size = new System.Drawing.Size(36, 23);
            this.tbxDays.TabIndex = 12;
            this.tbxDays.Text = "1";
            this.tbxDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lbdDays
            // 
            this.lbdDays.BackColor = System.Drawing.Color.Transparent;
            this.lbdDays.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbdDays.Location = new System.Drawing.Point(613, 8);
            this.lbdDays.Name = "lbdDays";
            this.lbdDays.Size = new System.Drawing.Size(69, 21);
            this.lbdDays.TabIndex = 13;
            this.lbdDays.Text = "Days";
            // 
            // FormCimMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1138, 622);
            this.Name = "FormCimMessage";
            this.Text = "CIM Message";
            this.Load += new System.EventHandler(this.FormCimMessage_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMessage)).EndInit();
            this.grbCIMMessage.ResumeLayout(false);
            this.tlpMsg.ResumeLayout(false);
            this.pnlMsg.ResumeLayout(false);
            this.pnlMsg.PerformLayout();
            this.tlpBase.ResumeLayout(false);
            this.pnlNode.ResumeLayout(false);
            this.pnlNode.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblLocalNode;
        private System.Windows.Forms.TextBox txtMsgText;
        private System.Windows.Forms.Label lblMsgText;
        private System.Windows.Forms.ComboBox cboLocalNode;
        private System.Windows.Forms.DataGridView dgvMessage;
        private System.Windows.Forms.GroupBox grbCIMMessage;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel pnlMsg;
        private System.Windows.Forms.Panel pnlNode;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TableLayoutPanel tlpMsg;
        private System.Windows.Forms.TextBox txtTouchPanelNo;
        private System.Windows.Forms.Label lblTouchPanelNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMessageID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTouchPanelNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMessageText;
        private System.Windows.Forms.DataGridViewButtonColumn colClear;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.Label lbdDays;
        private System.Windows.Forms.TextBox tbxDays;
    }
}