namespace UniOPI
{
    partial class FormQTimeSetting
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
            this.spc = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colQTimeID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartNodeNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartLocalID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartUnitNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartUnitID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartEventMsg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndNodeNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndLocalID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndUnitNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndUnitID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndEventMsg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSetTimeValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.comRemark = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartNodeRecipeID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCFRWQTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEnabled = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpButton = new System.Windows.Forms.TableLayoutPanel();
            this.pnlNormalBtn = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnModify = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spc)).BeginInit();
            this.spc.Panel1.SuspendLayout();
            this.spc.Panel2.SuspendLayout();
            this.spc.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tlpButton.SuspendLayout();
            this.pnlNormalBtn.SuspendLayout();
            this.SuspendLayout();
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.spc);
            // 
            // spc
            // 
            this.spc.BackColor = System.Drawing.Color.Transparent;
            this.spc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spc.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.spc.Location = new System.Drawing.Point(0, 0);
            this.spc.Name = "spc";
            // 
            // spc.Panel1
            // 
            this.spc.Panel1.Controls.Add(this.groupBox2);
            // 
            // spc.Panel2
            // 
            this.spc.Panel2.Controls.Add(this.tlpButton);
            this.spc.Size = new System.Drawing.Size(1264, 517);
            this.spc.SplitterDistance = 1122;
            this.spc.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dgvData);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1122, 517);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
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
            this.colQTimeID,
            this.colStartNodeNo,
            this.colStartLocalID,
            this.colStartUnitNo,
            this.colStartUnitID,
            this.colStartEventMsg,
            this.colEndNodeNo,
            this.colEndLocalID,
            this.colEndUnitNo,
            this.colEndUnitID,
            this.colEndEventMsg,
            this.colSetTimeValue,
            this.comRemark,
            this.colStartNodeRecipeID,
            this.colCFRWQTime,
            this.colEnabled});
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
            this.dgvData.Size = new System.Drawing.Size(1116, 491);
            this.dgvData.TabIndex = 12;
            this.dgvData.DataSourceChanged += new System.EventHandler(this.dgvData_DataSourceChanged);
            // 
            // colQTimeID
            // 
            this.colQTimeID.DataPropertyName = "QTIMEID";
            this.colQTimeID.HeaderText = "Q Time ID";
            this.colQTimeID.Name = "colQTimeID";
            this.colQTimeID.ReadOnly = true;
            // 
            // colStartNodeNo
            // 
            this.colStartNodeNo.DataPropertyName = "STARTNODENO";
            this.colStartNodeNo.HeaderText = "Start Local No";
            this.colStartNodeNo.Name = "colStartNodeNo";
            this.colStartNodeNo.ReadOnly = true;
            this.colStartNodeNo.Width = 130;
            // 
            // colStartLocalID
            // 
            this.colStartLocalID.DataPropertyName = "STARTNODEID";
            this.colStartLocalID.HeaderText = "Start EQP ID";
            this.colStartLocalID.Name = "colStartLocalID";
            this.colStartLocalID.ReadOnly = true;
            this.colStartLocalID.Width = 120;
            // 
            // colStartUnitNo
            // 
            this.colStartUnitNo.DataPropertyName = "STARTUNITNO";
            this.colStartUnitNo.HeaderText = "Start Unit No";
            this.colStartUnitNo.Name = "colStartUnitNo";
            this.colStartUnitNo.ReadOnly = true;
            this.colStartUnitNo.Width = 120;
            // 
            // colStartUnitID
            // 
            this.colStartUnitID.DataPropertyName = "STARTNUNITID";
            this.colStartUnitID.HeaderText = "Start Unit ID";
            this.colStartUnitID.Name = "colStartUnitID";
            this.colStartUnitID.ReadOnly = true;
            this.colStartUnitID.Width = 120;
            // 
            // colStartEventMsg
            // 
            this.colStartEventMsg.DataPropertyName = "STARTEVENTMSG";
            this.colStartEventMsg.HeaderText = "Start Event";
            this.colStartEventMsg.Name = "colStartEventMsg";
            this.colStartEventMsg.ReadOnly = true;
            this.colStartEventMsg.Width = 150;
            // 
            // colEndNodeNo
            // 
            this.colEndNodeNo.DataPropertyName = "ENDNODENO";
            this.colEndNodeNo.HeaderText = "End Local No";
            this.colEndNodeNo.Name = "colEndNodeNo";
            this.colEndNodeNo.ReadOnly = true;
            this.colEndNodeNo.Width = 120;
            // 
            // colEndLocalID
            // 
            this.colEndLocalID.DataPropertyName = "ENDNODEID";
            this.colEndLocalID.HeaderText = "End Local ID";
            this.colEndLocalID.Name = "colEndLocalID";
            this.colEndLocalID.ReadOnly = true;
            this.colEndLocalID.Width = 120;
            // 
            // colEndUnitNo
            // 
            this.colEndUnitNo.DataPropertyName = "ENDUNITNO";
            this.colEndUnitNo.HeaderText = "End Unit No";
            this.colEndUnitNo.Name = "colEndUnitNo";
            this.colEndUnitNo.ReadOnly = true;
            this.colEndUnitNo.Width = 120;
            // 
            // colEndUnitID
            // 
            this.colEndUnitID.DataPropertyName = "ENDNUNITID";
            this.colEndUnitID.HeaderText = "End Unit ID";
            this.colEndUnitID.Name = "colEndUnitID";
            this.colEndUnitID.ReadOnly = true;
            this.colEndUnitID.Width = 120;
            // 
            // colEndEventMsg
            // 
            this.colEndEventMsg.DataPropertyName = "ENDEVENTMSG";
            this.colEndEventMsg.HeaderText = "End Event";
            this.colEndEventMsg.Name = "colEndEventMsg";
            this.colEndEventMsg.ReadOnly = true;
            this.colEndEventMsg.Width = 150;
            // 
            // colSetTimeValue
            // 
            this.colSetTimeValue.DataPropertyName = "SETTIMEVALUE";
            this.colSetTimeValue.HeaderText = "NG Q Time";
            this.colSetTimeValue.Name = "colSetTimeValue";
            this.colSetTimeValue.ReadOnly = true;
            this.colSetTimeValue.Width = 150;
            // 
            // comRemark
            // 
            this.comRemark.DataPropertyName = "REMARK";
            this.comRemark.HeaderText = "Remark";
            this.comRemark.Name = "comRemark";
            this.comRemark.ReadOnly = true;
            // 
            // colStartNodeRecipeID
            // 
            this.colStartNodeRecipeID.DataPropertyName = "STARTNODERECIPEID";
            this.colStartNodeRecipeID.HeaderText = "Recipe ID";
            this.colStartNodeRecipeID.Name = "colStartNodeRecipeID";
            this.colStartNodeRecipeID.ReadOnly = true;
            // 
            // colCFRWQTime
            // 
            this.colCFRWQTime.DataPropertyName = "CFRWQTIME";
            this.colCFRWQTime.HeaderText = "Coater Rework Q Time";
            this.colCFRWQTime.Name = "colCFRWQTime";
            this.colCFRWQTime.ReadOnly = true;
            this.colCFRWQTime.Visible = false;
            this.colCFRWQTime.Width = 150;
            // 
            // colEnabled
            // 
            this.colEnabled.DataPropertyName = "ENABLED";
            this.colEnabled.HeaderText = "Enabled";
            this.colEnabled.Name = "colEnabled";
            this.colEnabled.ReadOnly = true;
            // 
            // tlpButton
            // 
            this.tlpButton.ColumnCount = 1;
            this.tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButton.Controls.Add(this.pnlNormalBtn, 0, 0);
            this.tlpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpButton.Location = new System.Drawing.Point(0, 0);
            this.tlpButton.Name = "tlpButton";
            this.tlpButton.RowCount = 2;
            this.tlpButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tlpButton.Size = new System.Drawing.Size(138, 517);
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
            this.pnlNormalBtn.Size = new System.Drawing.Size(132, 342);
            this.pnlNormalBtn.TabIndex = 0;
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(6, 171);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 50);
            this.btnSave.TabIndex = 12;
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
            // FormQTimeSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 548);
            this.Name = "FormQTimeSetting";
            this.Text = "FormQTimeSetting";
            this.Load += new System.EventHandler(this.FormQTimeSetting_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.spc.Panel1.ResumeLayout(false);
            this.spc.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spc)).EndInit();
            this.spc.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tlpButton.ResumeLayout(false);
            this.pnlNormalBtn.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spc;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.TableLayoutPanel tlpButton;
        private System.Windows.Forms.Panel pnlNormalBtn;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnModify;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQTimeID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartNodeNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartLocalID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartUnitID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartEventMsg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndNodeNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndLocalID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndUnitID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndEventMsg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSetTimeValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn comRemark;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartNodeRecipeID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCFRWQTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEnabled;
    }
}