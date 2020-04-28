namespace UniOPI
{
    partial class FormParamManagementBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        protected System.ComponentModel.IContainer components = null;

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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colObjectKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReportUnitNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSVID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colParameterName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSite = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRange = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOperator = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOperand = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReportTo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExpression = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWoffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWPoints = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBPoints = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobDataItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpButton = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnExport = new System.Windows.Forms.Button();
            this.pnlNormalBtn = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnModify = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlCombox = new System.Windows.Forms.Panel();
            this.btnQuery = new System.Windows.Forms.Button();
            this.lblNode = new System.Windows.Forms.Label();
            this.cmbNode = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tlpButton.SuspendLayout();
            this.panel1.SuspendLayout();
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dgvData);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(3, 70);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1127, 444);
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
            this.colObjectKey,
            this.colLineType,
            this.colServerName,
            this.colLineID,
            this.colLocalNo,
            this.colLocalID,
            this.colReportUnitNo,
            this.colSVID,
            this.colParameterName,
            this.colItem,
            this.colSite,
            this.colUnit,
            this.colRange,
            this.colOperator,
            this.colOperand,
            this.colReportTo,
            this.colExpression,
            this.colWoffset,
            this.colWPoints,
            this.colBOffset,
            this.colBPoints,
            this.colDescription,
            this.colJobDataItemName});
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
            this.dgvData.Size = new System.Drawing.Size(1121, 418);
            this.dgvData.TabIndex = 12;
            // 
            // colObjectKey
            // 
            this.colObjectKey.DataPropertyName = "OBJECTKEY";
            this.colObjectKey.HeaderText = "ObjectKey";
            this.colObjectKey.Name = "colObjectKey";
            this.colObjectKey.ReadOnly = true;
            this.colObjectKey.Visible = false;
            // 
            // colLineType
            // 
            this.colLineType.DataPropertyName = "LINETYPE";
            this.colLineType.HeaderText = "LineType";
            this.colLineType.Name = "colLineType";
            this.colLineType.ReadOnly = true;
            this.colLineType.Visible = false;
            // 
            // colServerName
            // 
            this.colServerName.DataPropertyName = "SERVERNAME";
            this.colServerName.HeaderText = "ServerName";
            this.colServerName.Name = "colServerName";
            this.colServerName.ReadOnly = true;
            this.colServerName.Visible = false;
            // 
            // colLineID
            // 
            this.colLineID.DataPropertyName = "LINEID";
            this.colLineID.HeaderText = "LineID";
            this.colLineID.Name = "colLineID";
            this.colLineID.ReadOnly = true;
            this.colLineID.Visible = false;
            // 
            // colLocalNo
            // 
            this.colLocalNo.DataPropertyName = "NODENO";
            this.colLocalNo.HeaderText = "Local No";
            this.colLocalNo.Name = "colLocalNo";
            this.colLocalNo.ReadOnly = true;
            this.colLocalNo.Width = 90;
            // 
            // colLocalID
            // 
            this.colLocalID.HeaderText = "Local ID";
            this.colLocalID.Name = "colLocalID";
            this.colLocalID.ReadOnly = true;
            // 
            // colReportUnitNo
            // 
            this.colReportUnitNo.DataPropertyName = "REPORTUNITNO";
            this.colReportUnitNo.HeaderText = "Unit No";
            this.colReportUnitNo.Name = "colReportUnitNo";
            this.colReportUnitNo.ReadOnly = true;
            this.colReportUnitNo.Width = 90;
            // 
            // colSVID
            // 
            this.colSVID.DataPropertyName = "SVID";
            this.colSVID.HeaderText = "SVID";
            this.colSVID.Name = "colSVID";
            this.colSVID.ReadOnly = true;
            this.colSVID.Width = 80;
            // 
            // colParameterName
            // 
            this.colParameterName.DataPropertyName = "PARAMETERNAME";
            this.colParameterName.HeaderText = "Parameter Name";
            this.colParameterName.Name = "colParameterName";
            this.colParameterName.ReadOnly = true;
            this.colParameterName.Width = 150;
            // 
            // colItem
            // 
            this.colItem.DataPropertyName = "ITEM";
            this.colItem.HeaderText = "Item";
            this.colItem.Name = "colItem";
            this.colItem.ReadOnly = true;
            // 
            // colSite
            // 
            this.colSite.DataPropertyName = "SITE";
            this.colSite.HeaderText = "Site";
            this.colSite.Name = "colSite";
            this.colSite.ReadOnly = true;
            // 
            // colUnit
            // 
            this.colUnit.DataPropertyName = "UNIT";
            this.colUnit.HeaderText = "Unit";
            this.colUnit.Name = "colUnit";
            this.colUnit.ReadOnly = true;
            // 
            // colRange
            // 
            this.colRange.DataPropertyName = "RANGE";
            this.colRange.HeaderText = "Range";
            this.colRange.Name = "colRange";
            this.colRange.ReadOnly = true;
            this.colRange.Width = 80;
            // 
            // colOperator
            // 
            this.colOperator.DataPropertyName = "OPERATOR";
            this.colOperator.HeaderText = "Operator";
            this.colOperator.Name = "colOperator";
            this.colOperator.ReadOnly = true;
            this.colOperator.Width = 90;
            // 
            // colOperand
            // 
            this.colOperand.DataPropertyName = "DOTRATIO";
            this.colOperand.HeaderText = "DOT/Ratio Operand";
            this.colOperand.Name = "colOperand";
            this.colOperand.ReadOnly = true;
            this.colOperand.Width = 180;
            // 
            // colReportTo
            // 
            this.colReportTo.DataPropertyName = "REPORTTO";
            this.colReportTo.HeaderText = "Report To";
            this.colReportTo.Name = "colReportTo";
            this.colReportTo.ReadOnly = true;
            // 
            // colExpression
            // 
            this.colExpression.DataPropertyName = "EXPRESSION";
            this.colExpression.HeaderText = "Decode Format";
            this.colExpression.Name = "colExpression";
            this.colExpression.ReadOnly = true;
            this.colExpression.Width = 140;
            // 
            // colWoffset
            // 
            this.colWoffset.DataPropertyName = "WOFFSET";
            this.colWoffset.HeaderText = "Decode Word Offset Position";
            this.colWoffset.Name = "colWoffset";
            this.colWoffset.ReadOnly = true;
            this.colWoffset.Width = 220;
            // 
            // colWPoints
            // 
            this.colWPoints.DataPropertyName = "WPOINTS";
            this.colWPoints.HeaderText = "Decode Word Length";
            this.colWPoints.Name = "colWPoints";
            this.colWPoints.ReadOnly = true;
            this.colWPoints.Width = 200;
            // 
            // colBOffset
            // 
            this.colBOffset.DataPropertyName = "BOFFSET";
            this.colBOffset.HeaderText = "Decode bit Offset position";
            this.colBOffset.Name = "colBOffset";
            this.colBOffset.ReadOnly = true;
            this.colBOffset.Width = 220;
            // 
            // colBPoints
            // 
            this.colBPoints.DataPropertyName = "BPOINTS";
            this.colBPoints.HeaderText = "Decode bit Length";
            this.colBPoints.Name = "colBPoints";
            this.colBPoints.ReadOnly = true;
            this.colBPoints.Width = 150;
            // 
            // colDescription
            // 
            this.colDescription.DataPropertyName = "DESCRIPTION";
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            // 
            // colJobDataItemName
            // 
            this.colJobDataItemName.DataPropertyName = "JOBDATAITEMNAME";
            this.colJobDataItemName.HeaderText = "Job Data Item Name";
            this.colJobDataItemName.Name = "colJobDataItemName";
            this.colJobDataItemName.ReadOnly = true;
            this.colJobDataItemName.Width = 200;
            // 
            // tlpButton
            // 
            this.tlpButton.ColumnCount = 1;
            this.tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButton.Controls.Add(this.panel1, 0, 1);
            this.tlpButton.Controls.Add(this.pnlNormalBtn, 0, 0);
            this.tlpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpButton.Location = new System.Drawing.Point(1136, 70);
            this.tlpButton.Name = "tlpButton";
            this.tlpButton.RowCount = 2;
            this.tlpButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 123F));
            this.tlpButton.Size = new System.Drawing.Size(125, 444);
            this.tlpButton.TabIndex = 14;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnExport);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 324);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(119, 117);
            this.panel1.TabIndex = 1;
            // 
            // btnExport
            // 
            this.btnExport.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnExport.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExport.Location = new System.Drawing.Point(4, 59);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(110, 50);
            this.btnExport.TabIndex = 10;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Visible = false;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
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
            this.pnlNormalBtn.Size = new System.Drawing.Size(119, 315);
            this.pnlNormalBtn.TabIndex = 0;
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(4, 159);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 50);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAdd.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(4, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(110, 50);
            this.btnAdd.TabIndex = 8;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(4, 211);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(110, 50);
            this.btnRefresh.TabIndex = 11;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(4, 107);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(110, 50);
            this.btnDelete.TabIndex = 9;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnModify
            // 
            this.btnModify.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnModify.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModify.Location = new System.Drawing.Point(4, 55);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(110, 50);
            this.btnModify.TabIndex = 10;
            this.btnModify.Text = "Modify";
            this.btnModify.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 131F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tlpButton, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.pnlCombox, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 517);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // pnlCombox
            // 
            this.pnlCombox.Controls.Add(this.btnQuery);
            this.pnlCombox.Controls.Add(this.lblNode);
            this.pnlCombox.Controls.Add(this.cmbNode);
            this.pnlCombox.Location = new System.Drawing.Point(3, 3);
            this.pnlCombox.Name = "pnlCombox";
            this.pnlCombox.Size = new System.Drawing.Size(450, 61);
            this.pnlCombox.TabIndex = 24;
            // 
            // btnQuery
            // 
            this.btnQuery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.Location = new System.Drawing.Point(310, 16);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(110, 29);
            this.btnQuery.TabIndex = 9;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // lblNode
            // 
            this.lblNode.AutoSize = true;
            this.lblNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNode.Location = new System.Drawing.Point(9, 19);
            this.lblNode.Name = "lblNode";
            this.lblNode.Size = new System.Drawing.Size(45, 21);
            this.lblNode.TabIndex = 5;
            this.lblNode.Text = "Local";
            // 
            // cmbNode
            // 
            this.cmbNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbNode.FormattingEnabled = true;
            this.cmbNode.Items.AddRange(new object[] {
            "+",
            "-",
            "*",
            "/"});
            this.cmbNode.Location = new System.Drawing.Point(62, 16);
            this.cmbNode.Name = "cmbNode";
            this.cmbNode.Size = new System.Drawing.Size(240, 29);
            this.cmbNode.TabIndex = 4;
            // 
            // FormParamManagementBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 548);
            this.Name = "FormParamManagementBase";
            this.Text = "FormParamCommonManagement";
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tlpButton.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.pnlNormalBtn.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlCombox.ResumeLayout(false);
            this.pnlCombox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.GroupBox groupBox2;
        protected System.Windows.Forms.DataGridView dgvData;
        protected System.Windows.Forms.TableLayoutPanel tlpButton;
        protected System.Windows.Forms.Panel pnlNormalBtn;
        protected System.Windows.Forms.Button btnAdd;
        protected System.Windows.Forms.Button btnRefresh;
        protected System.Windows.Forms.Button btnDelete;
        protected System.Windows.Forms.Button btnModify;
        protected System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.Button btnExport;
        protected System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        protected System.Windows.Forms.Panel pnlCombox;
        protected System.Windows.Forms.ComboBox cmbNode;
        protected System.Windows.Forms.Button btnQuery;
        protected System.Windows.Forms.Label lblNode;
        protected System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReportUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSVID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParameterName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSite;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnit;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRange;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOperator;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOperand;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReportTo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExpression;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWoffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobDataItemName;
    }
}