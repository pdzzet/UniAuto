namespace UniOPI
{
    partial class FormLineStatusRule
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
            this.gbxRuleList = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colObjectKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colConditionStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colConditionSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalNoList = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalIDList = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUpdateDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOperatorID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpButton = new System.Windows.Forms.TableLayoutPanel();
            this.pnlNormalBtn = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnModify = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlCombox = new System.Windows.Forms.Panel();
            this.lblConditionStatus = new System.Windows.Forms.Label();
            this.cmbConditionStatus = new System.Windows.Forms.ComboBox();
            this.btnQuery = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.gbxRuleList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tlpButton.SuspendLayout();
            this.pnlNormalBtn.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlCombox.SuspendLayout();
            this.SuspendLayout();
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tableLayoutPanel1);
            // 
            // gbxRuleList
            // 
            this.gbxRuleList.Controls.Add(this.dgvData);
            this.gbxRuleList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxRuleList.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbxRuleList.Location = new System.Drawing.Point(3, 68);
            this.gbxRuleList.Name = "gbxRuleList";
            this.gbxRuleList.Size = new System.Drawing.Size(1125, 446);
            this.gbxRuleList.TabIndex = 23;
            this.gbxRuleList.TabStop = false;
            this.gbxRuleList.Text = "Rule List";
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
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colObjectKey,
            this.colLineType,
            this.colConditionStatus,
            this.colConditionSeqNo,
            this.colLocalNoList,
            this.colLocalIDList,
            this.colUpdateDate,
            this.colOperatorID});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 23);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1119, 420);
            this.dgvData.TabIndex = 12;
            this.dgvData.DataSourceChanged += new System.EventHandler(this.dgvData_DataSourceChanged);
            // 
            // colObjectKey
            // 
            this.colObjectKey.DataPropertyName = "OBJECTKEY";
            this.colObjectKey.HeaderText = "OBJECTKEY";
            this.colObjectKey.Name = "colObjectKey";
            this.colObjectKey.ReadOnly = true;
            this.colObjectKey.Visible = false;
            // 
            // colLineType
            // 
            this.colLineType.DataPropertyName = "LINETYPE";
            this.colLineType.HeaderText = "Line Type";
            this.colLineType.Name = "colLineType";
            this.colLineType.ReadOnly = true;
            this.colLineType.Width = 110;
            // 
            // colConditionStatus
            // 
            this.colConditionStatus.DataPropertyName = "CONDITIONSTATUS";
            this.colConditionStatus.HeaderText = "Condition Status";
            this.colConditionStatus.Name = "colConditionStatus";
            this.colConditionStatus.ReadOnly = true;
            this.colConditionStatus.Width = 160;
            // 
            // colConditionSeqNo
            // 
            this.colConditionSeqNo.DataPropertyName = "CONDITIONSEQNO";
            this.colConditionSeqNo.HeaderText = "Condition SeqNo";
            this.colConditionSeqNo.Name = "colConditionSeqNo";
            this.colConditionSeqNo.ReadOnly = true;
            this.colConditionSeqNo.Width = 160;
            // 
            // colLocalNoList
            // 
            this.colLocalNoList.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colLocalNoList.DataPropertyName = "EQPNOLIST";
            this.colLocalNoList.HeaderText = "Local No List";
            this.colLocalNoList.Name = "colLocalNoList";
            this.colLocalNoList.ReadOnly = true;
            this.colLocalNoList.Width = 206;
            // 
            // colLocalIDList
            // 
            this.colLocalIDList.HeaderText = "Local ID LIST";
            this.colLocalIDList.Name = "colLocalIDList";
            this.colLocalIDList.ReadOnly = true;
            this.colLocalIDList.Width = 200;
            // 
            // colUpdateDate
            // 
            this.colUpdateDate.DataPropertyName = "UPDATETIME";
            this.colUpdateDate.HeaderText = "Update Time";
            this.colUpdateDate.Name = "colUpdateDate";
            this.colUpdateDate.ReadOnly = true;
            this.colUpdateDate.Width = 160;
            // 
            // colOperatorID
            // 
            this.colOperatorID.DataPropertyName = "OPERATORID";
            this.colOperatorID.HeaderText = "Operator ID";
            this.colOperatorID.Name = "colOperatorID";
            this.colOperatorID.ReadOnly = true;
            this.colOperatorID.Width = 120;
            // 
            // tlpButton
            // 
            this.tlpButton.ColumnCount = 1;
            this.tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButton.Controls.Add(this.pnlNormalBtn, 0, 0);
            this.tlpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpButton.Location = new System.Drawing.Point(1134, 68);
            this.tlpButton.Name = "tlpButton";
            this.tlpButton.RowCount = 2;
            this.tlpButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 123F));
            this.tlpButton.Size = new System.Drawing.Size(127, 446);
            this.tlpButton.TabIndex = 14;
            // 
            // pnlNormalBtn
            // 
            this.pnlNormalBtn.Controls.Add(this.btnSave);
            this.pnlNormalBtn.Controls.Add(this.btnAdd);
            this.pnlNormalBtn.Controls.Add(this.btnRefresh);
            this.pnlNormalBtn.Controls.Add(this.btnDelete);
            this.pnlNormalBtn.Controls.Add(this.btnModify);
            this.pnlNormalBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNormalBtn.Location = new System.Drawing.Point(3, 3);
            this.pnlNormalBtn.Name = "pnlNormalBtn";
            this.pnlNormalBtn.Size = new System.Drawing.Size(121, 317);
            this.pnlNormalBtn.TabIndex = 0;
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(6, 171);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 50);
            this.btnSave.TabIndex = 13;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAdd.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(6, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(110, 50);
            this.btnAdd.TabIndex = 8;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(6, 227);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(110, 50);
            this.btnRefresh.TabIndex = 11;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(6, 115);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(110, 50);
            this.btnDelete.TabIndex = 9;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnModify
            // 
            this.btnModify.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnModify.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModify.Location = new System.Drawing.Point(6, 59);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(110, 50);
            this.btnModify.TabIndex = 10;
            this.btnModify.Text = "Modify";
            this.btnModify.UseVisualStyleBackColor = true;
            this.btnModify.Click += new System.EventHandler(this.btnModify_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.tableLayoutPanel1.Controls.Add(this.pnlCombox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gbxRuleList, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tlpButton, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 517);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // pnlCombox
            // 
            this.pnlCombox.Controls.Add(this.lblConditionStatus);
            this.pnlCombox.Controls.Add(this.cmbConditionStatus);
            this.pnlCombox.Controls.Add(this.btnQuery);
            this.pnlCombox.Location = new System.Drawing.Point(3, 3);
            this.pnlCombox.Name = "pnlCombox";
            this.pnlCombox.Size = new System.Drawing.Size(700, 59);
            this.pnlCombox.TabIndex = 25;
            // 
            // lblConditionStatus
            // 
            this.lblConditionStatus.AutoSize = true;
            this.lblConditionStatus.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConditionStatus.Location = new System.Drawing.Point(9, 20);
            this.lblConditionStatus.Name = "lblConditionStatus";
            this.lblConditionStatus.Size = new System.Drawing.Size(125, 21);
            this.lblConditionStatus.TabIndex = 11;
            this.lblConditionStatus.Text = "Condition Status";
            // 
            // cmbConditionStatus
            // 
            this.cmbConditionStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbConditionStatus.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbConditionStatus.FormattingEnabled = true;
            this.cmbConditionStatus.Location = new System.Drawing.Point(140, 17);
            this.cmbConditionStatus.Name = "cmbConditionStatus";
            this.cmbConditionStatus.Size = new System.Drawing.Size(154, 29);
            this.cmbConditionStatus.TabIndex = 10;
            // 
            // btnQuery
            // 
            this.btnQuery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.Location = new System.Drawing.Point(300, 17);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(110, 29);
            this.btnQuery.TabIndex = 9;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // FormLineStatusRule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 548);
            this.Name = "FormLineStatusRule";
            this.Text = "FormLineStatusRule";
            this.Load += new System.EventHandler(this.FormLineStatusRule_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.gbxRuleList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tlpButton.ResumeLayout(false);
            this.pnlNormalBtn.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlCombox.ResumeLayout(false);
            this.pnlCombox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbxRuleList;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.TableLayoutPanel tlpButton;
        private System.Windows.Forms.Panel pnlNormalBtn;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnModify;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel pnlCombox;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Label lblConditionStatus;
        private System.Windows.Forms.ComboBox cmbConditionStatus;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colConditionStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colConditionSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNoList;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalIDList;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUpdateDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOperatorID;
    }
}