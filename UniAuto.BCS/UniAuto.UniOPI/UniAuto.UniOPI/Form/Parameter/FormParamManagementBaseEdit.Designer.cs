namespace UniOPI
{
    partial class FormParamManagementBaseEdit
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
            this.dgvAddList = new System.Windows.Forms.DataGridView();
            this.colLineID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnitNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSVID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colParameterName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSite = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRange = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOperator = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDotRatio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReportTo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExpression = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWoffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWPoints = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBitOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBPoints = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobDataItem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.txtJobData = new System.Windows.Forms.TextBox();
            this.lblJobData = new System.Windows.Forms.Label();
            this.cmbUnit = new System.Windows.Forms.ComboBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblUnitNo = new System.Windows.Forms.Label();
            this.cmbNode = new System.Windows.Forms.ComboBox();
            this.cmbOperator = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtReportTo = new System.Windows.Forms.TextBox();
            this.lblNodeNo = new System.Windows.Forms.Label();
            this.lblSvID = new System.Windows.Forms.Label();
            this.txtSvID = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.lblParameterName = new System.Windows.Forms.Label();
            this.txtBitLength = new System.Windows.Forms.TextBox();
            this.lblBitLength = new System.Windows.Forms.Label();
            this.txtBitOffsetPos = new System.Windows.Forms.TextBox();
            this.txtItem = new System.Windows.Forms.TextBox();
            this.lblBitOffsetPos = new System.Windows.Forms.Label();
            this.lblRange = new System.Windows.Forms.Label();
            this.txtWordLength = new System.Windows.Forms.TextBox();
            this.txtRange = new System.Windows.Forms.TextBox();
            this.lblWordLength = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtWordOffsetPos = new System.Windows.Forms.TextBox();
            this.lblItem = new System.Windows.Forms.Label();
            this.lblWordOffsetPos = new System.Windows.Forms.Label();
            this.txtDotRatioOperand = new System.Windows.Forms.TextBox();
            this.txtUnit = new System.Windows.Forms.TextBox();
            this.lblSite = new System.Windows.Forms.Label();
            this.lblUnit = new System.Windows.Forms.Label();
            this.txtSite = new System.Windows.Forms.TextBox();
            this.lblDotRatioOperand = new System.Windows.Forms.Label();
            this.txtParameterName = new System.Windows.Forms.TextBox();
            this.cmbExpression = new System.Windows.Forms.ComboBox();
            this.lblOperator = new System.Windows.Forms.Label();
            this.lblExpression = new System.Windows.Forms.Label();
            this.btnAddOK = new System.Windows.Forms.Button();
            this.btnOKClose = new System.Windows.Forms.Button();
            this.btnCancelClose = new System.Windows.Forms.Button();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.flpButton = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlAdd = new System.Windows.Forms.Panel();
            this.btnClear = new System.Windows.Forms.Button();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.grbData = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddList)).BeginInit();
            this.pnlEdit.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.flpButton.SuspendLayout();
            this.pnlAdd.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.grbData.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(781, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(841, 718);
            // 
            // dgvAddList
            // 
            this.dgvAddList.AllowUserToAddRows = false;
            this.dgvAddList.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAddList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvAddList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAddList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLineID,
            this.colLocalNo,
            this.colUnitNo,
            this.colSVID,
            this.colParameterName,
            this.colItem,
            this.colSite,
            this.colUnit,
            this.colRange,
            this.colOperator,
            this.colDotRatio,
            this.colReportTo,
            this.colExpression,
            this.colWoffset,
            this.colWPoints,
            this.colBitOffset,
            this.colBPoints,
            this.colDescription,
            this.colJobDataItem});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvAddList.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvAddList.Location = new System.Drawing.Point(3, 45);
            this.dgvAddList.Name = "dgvAddList";
            this.dgvAddList.ReadOnly = true;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvAddList.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvAddList.RowTemplate.Height = 24;
            this.dgvAddList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAddList.Size = new System.Drawing.Size(829, 267);
            this.dgvAddList.TabIndex = 77;
            this.dgvAddList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAddList_CellClick);
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
            // colUnitNo
            // 
            this.colUnitNo.DataPropertyName = "REPORTUNITNO";
            this.colUnitNo.HeaderText = "Unit No";
            this.colUnitNo.Name = "colUnitNo";
            this.colUnitNo.ReadOnly = true;
            // 
            // colSVID
            // 
            this.colSVID.DataPropertyName = "SVID";
            this.colSVID.HeaderText = "SVID";
            this.colSVID.Name = "colSVID";
            this.colSVID.ReadOnly = true;
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
            this.colRange.Width = 50;
            // 
            // colOperator
            // 
            this.colOperator.DataPropertyName = "OPERATOR";
            this.colOperator.HeaderText = "Operator";
            this.colOperator.Name = "colOperator";
            this.colOperator.ReadOnly = true;
            // 
            // colDotRatio
            // 
            this.colDotRatio.DataPropertyName = "DOTRATIO";
            this.colDotRatio.HeaderText = "DOT/Ratio Operand";
            this.colDotRatio.Name = "colDotRatio";
            this.colDotRatio.ReadOnly = true;
            this.colDotRatio.Width = 200;
            // 
            // colReportTo
            // 
            this.colReportTo.DataPropertyName = "REPORTTO";
            this.colReportTo.HeaderText = "Report To";
            this.colReportTo.Name = "colReportTo";
            this.colReportTo.ReadOnly = true;
            this.colReportTo.Width = 120;
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
            this.colWoffset.Width = 240;
            // 
            // colWPoints
            // 
            this.colWPoints.DataPropertyName = "WPOINTS";
            this.colWPoints.HeaderText = "Decode Word Length";
            this.colWPoints.Name = "colWPoints";
            this.colWPoints.ReadOnly = true;
            this.colWPoints.Width = 180;
            // 
            // colBitOffset
            // 
            this.colBitOffset.DataPropertyName = "BOFFSET";
            this.colBitOffset.HeaderText = "Decode bit Offset position";
            this.colBitOffset.Name = "colBitOffset";
            this.colBitOffset.ReadOnly = true;
            this.colBitOffset.Width = 220;
            // 
            // colBPoints
            // 
            this.colBPoints.DataPropertyName = "BPOINTS";
            this.colBPoints.HeaderText = "Decode bit Length";
            this.colBPoints.Name = "colBPoints";
            this.colBPoints.ReadOnly = true;
            this.colBPoints.Width = 160;
            // 
            // colDescription
            // 
            this.colDescription.DataPropertyName = "DESCRIPTION";
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            // 
            // colJobDataItem
            // 
            this.colJobDataItem.DataPropertyName = "JOBDATAITEMNAME";
            this.colJobDataItem.HeaderText = "Job Data Item Name";
            this.colJobDataItem.Name = "colJobDataItem";
            this.colJobDataItem.ReadOnly = true;
            this.colJobDataItem.Width = 180;
            // 
            // pnlEdit
            // 
            this.pnlEdit.Controls.Add(this.txtJobData);
            this.pnlEdit.Controls.Add(this.lblJobData);
            this.pnlEdit.Controls.Add(this.cmbUnit);
            this.pnlEdit.Controls.Add(this.txtDescription);
            this.pnlEdit.Controls.Add(this.lblDescription);
            this.pnlEdit.Controls.Add(this.lblUnitNo);
            this.pnlEdit.Controls.Add(this.cmbNode);
            this.pnlEdit.Controls.Add(this.cmbOperator);
            this.pnlEdit.Controls.Add(this.label1);
            this.pnlEdit.Controls.Add(this.txtReportTo);
            this.pnlEdit.Controls.Add(this.lblNodeNo);
            this.pnlEdit.Controls.Add(this.lblSvID);
            this.pnlEdit.Controls.Add(this.txtSvID);
            this.pnlEdit.Controls.Add(this.label16);
            this.pnlEdit.Controls.Add(this.lblParameterName);
            this.pnlEdit.Controls.Add(this.txtBitLength);
            this.pnlEdit.Controls.Add(this.lblBitLength);
            this.pnlEdit.Controls.Add(this.txtBitOffsetPos);
            this.pnlEdit.Controls.Add(this.txtItem);
            this.pnlEdit.Controls.Add(this.lblBitOffsetPos);
            this.pnlEdit.Controls.Add(this.lblRange);
            this.pnlEdit.Controls.Add(this.txtWordLength);
            this.pnlEdit.Controls.Add(this.txtRange);
            this.pnlEdit.Controls.Add(this.lblWordLength);
            this.pnlEdit.Controls.Add(this.label3);
            this.pnlEdit.Controls.Add(this.txtWordOffsetPos);
            this.pnlEdit.Controls.Add(this.lblItem);
            this.pnlEdit.Controls.Add(this.lblWordOffsetPos);
            this.pnlEdit.Controls.Add(this.txtDotRatioOperand);
            this.pnlEdit.Controls.Add(this.txtUnit);
            this.pnlEdit.Controls.Add(this.lblSite);
            this.pnlEdit.Controls.Add(this.lblUnit);
            this.pnlEdit.Controls.Add(this.txtSite);
            this.pnlEdit.Controls.Add(this.lblDotRatioOperand);
            this.pnlEdit.Controls.Add(this.txtParameterName);
            this.pnlEdit.Controls.Add(this.cmbExpression);
            this.pnlEdit.Controls.Add(this.lblOperator);
            this.pnlEdit.Controls.Add(this.lblExpression);
            this.pnlEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEdit.Location = new System.Drawing.Point(3, 18);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(829, 293);
            this.pnlEdit.TabIndex = 72;
            // 
            // txtJobData
            // 
            this.txtJobData.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtJobData.Location = new System.Drawing.Point(600, 254);
            this.txtJobData.Name = "txtJobData";
            this.txtJobData.Size = new System.Drawing.Size(225, 28);
            this.txtJobData.TabIndex = 16;
            // 
            // lblJobData
            // 
            this.lblJobData.BackColor = System.Drawing.Color.Black;
            this.lblJobData.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblJobData.ForeColor = System.Drawing.Color.White;
            this.lblJobData.Location = new System.Drawing.Point(420, 255);
            this.lblJobData.Name = "lblJobData";
            this.lblJobData.Size = new System.Drawing.Size(180, 25);
            this.lblJobData.TabIndex = 77;
            this.lblJobData.Text = "Job Data Item Name";
            this.lblJobData.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbUnit
            // 
            this.cmbUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUnit.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbUnit.FormattingEnabled = true;
            this.cmbUnit.Location = new System.Drawing.Point(600, 3);
            this.cmbUnit.Name = "cmbUnit";
            this.cmbUnit.Size = new System.Drawing.Size(225, 29);
            this.cmbUnit.TabIndex = 74;
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.Location = new System.Drawing.Point(185, 254);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(225, 28);
            this.txtDescription.TabIndex = 15;
            // 
            // lblDescription
            // 
            this.lblDescription.BackColor = System.Drawing.Color.Black;
            this.lblDescription.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescription.ForeColor = System.Drawing.Color.White;
            this.lblDescription.Location = new System.Drawing.Point(6, 255);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(180, 25);
            this.lblDescription.TabIndex = 45;
            this.lblDescription.Text = "Description";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUnitNo
            // 
            this.lblUnitNo.BackColor = System.Drawing.Color.Black;
            this.lblUnitNo.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnitNo.ForeColor = System.Drawing.Color.White;
            this.lblUnitNo.Location = new System.Drawing.Point(420, 5);
            this.lblUnitNo.Name = "lblUnitNo";
            this.lblUnitNo.Size = new System.Drawing.Size(180, 25);
            this.lblUnitNo.TabIndex = 75;
            this.lblUnitNo.Text = "Unit ID";
            this.lblUnitNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbNode
            // 
            this.cmbNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbNode.FormattingEnabled = true;
            this.cmbNode.Location = new System.Drawing.Point(185, 3);
            this.cmbNode.Name = "cmbNode";
            this.cmbNode.Size = new System.Drawing.Size(225, 29);
            this.cmbNode.TabIndex = 0;
            // 
            // cmbOperator
            // 
            this.cmbOperator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOperator.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbOperator.FormattingEnabled = true;
            this.cmbOperator.Items.AddRange(new object[] {
            "",
            "+",
            "-",
            "*",
            "/"});
            this.cmbOperator.Location = new System.Drawing.Point(185, 126);
            this.cmbOperator.Name = "cmbOperator";
            this.cmbOperator.Size = new System.Drawing.Size(225, 29);
            this.cmbOperator.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(6, 223);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 25);
            this.label1.TabIndex = 73;
            this.label1.Text = "ReportTo";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtReportTo
            // 
            this.txtReportTo.BackColor = System.Drawing.SystemColors.Control;
            this.txtReportTo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtReportTo.Location = new System.Drawing.Point(185, 222);
            this.txtReportTo.Name = "txtReportTo";
            this.txtReportTo.Size = new System.Drawing.Size(225, 28);
            this.txtReportTo.TabIndex = 13;
            // 
            // lblNodeNo
            // 
            this.lblNodeNo.BackColor = System.Drawing.Color.Black;
            this.lblNodeNo.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNodeNo.ForeColor = System.Drawing.Color.White;
            this.lblNodeNo.Location = new System.Drawing.Point(6, 5);
            this.lblNodeNo.Name = "lblNodeNo";
            this.lblNodeNo.Size = new System.Drawing.Size(180, 25);
            this.lblNodeNo.TabIndex = 71;
            this.lblNodeNo.Text = "Local ID";
            this.lblNodeNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSvID
            // 
            this.lblSvID.BackColor = System.Drawing.Color.Black;
            this.lblSvID.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSvID.ForeColor = System.Drawing.Color.White;
            this.lblSvID.Location = new System.Drawing.Point(6, 37);
            this.lblSvID.Name = "lblSvID";
            this.lblSvID.Size = new System.Drawing.Size(180, 25);
            this.lblSvID.TabIndex = 40;
            this.lblSvID.Text = "SVID";
            this.lblSvID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSvID
            // 
            this.txtSvID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSvID.Location = new System.Drawing.Point(185, 35);
            this.txtSvID.Name = "txtSvID";
            this.txtSvID.Size = new System.Drawing.Size(225, 28);
            this.txtSvID.TabIndex = 1;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(337, 109);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(73, 12);
            this.label16.TabIndex = 69;
            this.label16.Text = "EX: Kg, %, ℃";
            // 
            // lblParameterName
            // 
            this.lblParameterName.BackColor = System.Drawing.Color.Black;
            this.lblParameterName.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblParameterName.ForeColor = System.Drawing.Color.White;
            this.lblParameterName.Location = new System.Drawing.Point(420, 37);
            this.lblParameterName.Name = "lblParameterName";
            this.lblParameterName.Size = new System.Drawing.Size(180, 25);
            this.lblParameterName.TabIndex = 43;
            this.lblParameterName.Text = "Parameter Name";
            this.lblParameterName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtBitLength
            // 
            this.txtBitLength.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBitLength.Location = new System.Drawing.Point(600, 191);
            this.txtBitLength.Name = "txtBitLength";
            this.txtBitLength.Size = new System.Drawing.Size(225, 28);
            this.txtBitLength.TabIndex = 12;
            // 
            // lblBitLength
            // 
            this.lblBitLength.BackColor = System.Drawing.Color.Black;
            this.lblBitLength.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBitLength.ForeColor = System.Drawing.Color.White;
            this.lblBitLength.Location = new System.Drawing.Point(420, 192);
            this.lblBitLength.Name = "lblBitLength";
            this.lblBitLength.Size = new System.Drawing.Size(180, 25);
            this.lblBitLength.TabIndex = 67;
            this.lblBitLength.Text = "Decode Bit Length";
            this.lblBitLength.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtBitOffsetPos
            // 
            this.txtBitOffsetPos.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBitOffsetPos.Location = new System.Drawing.Point(600, 158);
            this.txtBitOffsetPos.Name = "txtBitOffsetPos";
            this.txtBitOffsetPos.Size = new System.Drawing.Size(225, 28);
            this.txtBitOffsetPos.TabIndex = 10;
            // 
            // txtItem
            // 
            this.txtItem.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtItem.Location = new System.Drawing.Point(185, 65);
            this.txtItem.Name = "txtItem";
            this.txtItem.Size = new System.Drawing.Size(225, 28);
            this.txtItem.TabIndex = 3;
            // 
            // lblBitOffsetPos
            // 
            this.lblBitOffsetPos.BackColor = System.Drawing.Color.Black;
            this.lblBitOffsetPos.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBitOffsetPos.ForeColor = System.Drawing.Color.White;
            this.lblBitOffsetPos.Location = new System.Drawing.Point(420, 161);
            this.lblBitOffsetPos.Name = "lblBitOffsetPos";
            this.lblBitOffsetPos.Size = new System.Drawing.Size(180, 25);
            this.lblBitOffsetPos.TabIndex = 65;
            this.lblBitOffsetPos.Text = "Decode Bit Offset Position";
            this.lblBitOffsetPos.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRange
            // 
            this.lblRange.BackColor = System.Drawing.Color.Black;
            this.lblRange.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRange.ForeColor = System.Drawing.Color.White;
            this.lblRange.Location = new System.Drawing.Point(6, 192);
            this.lblRange.Name = "lblRange";
            this.lblRange.Size = new System.Drawing.Size(180, 25);
            this.lblRange.TabIndex = 47;
            this.lblRange.Text = "Range";
            this.lblRange.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtWordLength
            // 
            this.txtWordLength.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtWordLength.Location = new System.Drawing.Point(600, 126);
            this.txtWordLength.Name = "txtWordLength";
            this.txtWordLength.Size = new System.Drawing.Size(225, 28);
            this.txtWordLength.TabIndex = 8;
            // 
            // txtRange
            // 
            this.txtRange.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRange.Location = new System.Drawing.Point(185, 191);
            this.txtRange.Name = "txtRange";
            this.txtRange.Size = new System.Drawing.Size(151, 28);
            this.txtRange.TabIndex = 11;
            this.txtRange.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNumber_KeyPress);
            // 
            // lblWordLength
            // 
            this.lblWordLength.BackColor = System.Drawing.Color.Black;
            this.lblWordLength.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWordLength.ForeColor = System.Drawing.Color.White;
            this.lblWordLength.Location = new System.Drawing.Point(420, 128);
            this.lblWordLength.Name = "lblWordLength";
            this.lblWordLength.Size = new System.Drawing.Size(180, 25);
            this.lblWordLength.TabIndex = 63;
            this.lblWordLength.Text = "Decode Word Length";
            this.lblWordLength.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(342, 203);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 12);
            this.label3.TabIndex = 48;
            this.label3.Text = "EX: 0~65535";
            // 
            // txtWordOffsetPos
            // 
            this.txtWordOffsetPos.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtWordOffsetPos.Location = new System.Drawing.Point(600, 95);
            this.txtWordOffsetPos.Name = "txtWordOffsetPos";
            this.txtWordOffsetPos.Size = new System.Drawing.Size(225, 28);
            this.txtWordOffsetPos.TabIndex = 6;
            // 
            // lblItem
            // 
            this.lblItem.BackColor = System.Drawing.Color.Black;
            this.lblItem.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItem.ForeColor = System.Drawing.Color.White;
            this.lblItem.Location = new System.Drawing.Point(6, 67);
            this.lblItem.Name = "lblItem";
            this.lblItem.Size = new System.Drawing.Size(180, 25);
            this.lblItem.TabIndex = 50;
            this.lblItem.Text = "Item";
            this.lblItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblWordOffsetPos
            // 
            this.lblWordOffsetPos.BackColor = System.Drawing.Color.Black;
            this.lblWordOffsetPos.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWordOffsetPos.ForeColor = System.Drawing.Color.White;
            this.lblWordOffsetPos.Location = new System.Drawing.Point(420, 97);
            this.lblWordOffsetPos.Name = "lblWordOffsetPos";
            this.lblWordOffsetPos.Size = new System.Drawing.Size(180, 25);
            this.lblWordOffsetPos.TabIndex = 61;
            this.lblWordOffsetPos.Text = "Decode Word Offset Position";
            this.lblWordOffsetPos.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDotRatioOperand
            // 
            this.txtDotRatioOperand.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDotRatioOperand.Location = new System.Drawing.Point(600, 222);
            this.txtDotRatioOperand.Name = "txtDotRatioOperand";
            this.txtDotRatioOperand.Size = new System.Drawing.Size(225, 28);
            this.txtDotRatioOperand.TabIndex = 14;
            // 
            // txtUnit
            // 
            this.txtUnit.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUnit.Location = new System.Drawing.Point(185, 95);
            this.txtUnit.Name = "txtUnit";
            this.txtUnit.Size = new System.Drawing.Size(151, 28);
            this.txtUnit.TabIndex = 5;
            // 
            // lblSite
            // 
            this.lblSite.BackColor = System.Drawing.Color.Black;
            this.lblSite.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSite.ForeColor = System.Drawing.Color.White;
            this.lblSite.Location = new System.Drawing.Point(420, 67);
            this.lblSite.Name = "lblSite";
            this.lblSite.Size = new System.Drawing.Size(180, 25);
            this.lblSite.TabIndex = 52;
            this.lblSite.Text = "Site";
            this.lblSite.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUnit
            // 
            this.lblUnit.BackColor = System.Drawing.Color.Black;
            this.lblUnit.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnit.ForeColor = System.Drawing.Color.White;
            this.lblUnit.Location = new System.Drawing.Point(6, 97);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(180, 25);
            this.lblUnit.TabIndex = 60;
            this.lblUnit.Text = "Unit";
            this.lblUnit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSite
            // 
            this.txtSite.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSite.Location = new System.Drawing.Point(600, 65);
            this.txtSite.Name = "txtSite";
            this.txtSite.Size = new System.Drawing.Size(225, 28);
            this.txtSite.TabIndex = 4;
            // 
            // lblDotRatioOperand
            // 
            this.lblDotRatioOperand.BackColor = System.Drawing.Color.Black;
            this.lblDotRatioOperand.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDotRatioOperand.ForeColor = System.Drawing.Color.White;
            this.lblDotRatioOperand.Location = new System.Drawing.Point(420, 223);
            this.lblDotRatioOperand.Name = "lblDotRatioOperand";
            this.lblDotRatioOperand.Size = new System.Drawing.Size(180, 25);
            this.lblDotRatioOperand.TabIndex = 58;
            this.lblDotRatioOperand.Text = "DOT/Ratio Operand";
            this.lblDotRatioOperand.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtParameterName
            // 
            this.txtParameterName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtParameterName.Location = new System.Drawing.Point(600, 35);
            this.txtParameterName.Name = "txtParameterName";
            this.txtParameterName.Size = new System.Drawing.Size(225, 28);
            this.txtParameterName.TabIndex = 2;
            // 
            // cmbExpression
            // 
            this.cmbExpression.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExpression.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbExpression.FormattingEnabled = true;
            this.cmbExpression.Items.AddRange(new object[] {
            "BIT",
            "BIN",
            "INT",
            "EXP",
            "HEX",
            "BCD",
            "SINT",
            "ASCII",
            "LONG",
            "SLONG"});
            this.cmbExpression.Location = new System.Drawing.Point(185, 160);
            this.cmbExpression.Name = "cmbExpression";
            this.cmbExpression.Size = new System.Drawing.Size(225, 29);
            this.cmbExpression.TabIndex = 9;
            // 
            // lblOperator
            // 
            this.lblOperator.BackColor = System.Drawing.Color.Black;
            this.lblOperator.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOperator.ForeColor = System.Drawing.Color.White;
            this.lblOperator.Location = new System.Drawing.Point(6, 127);
            this.lblOperator.Name = "lblOperator";
            this.lblOperator.Size = new System.Drawing.Size(180, 25);
            this.lblOperator.TabIndex = 54;
            this.lblOperator.Text = "Operator";
            this.lblOperator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblExpression
            // 
            this.lblExpression.BackColor = System.Drawing.Color.Black;
            this.lblExpression.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExpression.ForeColor = System.Drawing.Color.White;
            this.lblExpression.Location = new System.Drawing.Point(6, 161);
            this.lblExpression.Name = "lblExpression";
            this.lblExpression.Size = new System.Drawing.Size(180, 25);
            this.lblExpression.TabIndex = 56;
            this.lblExpression.Text = "Decode Format";
            this.lblExpression.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnAddOK
            // 
            this.btnAddOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddOK.Location = new System.Drawing.Point(319, 3);
            this.btnAddOK.Name = "btnAddOK";
            this.btnAddOK.Size = new System.Drawing.Size(90, 30);
            this.btnAddOK.TabIndex = 0;
            this.btnAddOK.Tag = "ADD";
            this.btnAddOK.Text = "ADD";
            this.btnAddOK.UseVisualStyleBackColor = true;
            this.btnAddOK.Click += new System.EventHandler(this.btnADD_Click);
            // 
            // btnOKClose
            // 
            this.btnOKClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOKClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOKClose.Location = new System.Drawing.Point(319, 2);
            this.btnOKClose.Name = "btnOKClose";
            this.btnOKClose.Size = new System.Drawing.Size(90, 30);
            this.btnOKClose.TabIndex = 73;
            this.btnOKClose.Tag = "OK";
            this.btnOKClose.Text = "OK";
            this.btnOKClose.UseVisualStyleBackColor = true;
            this.btnOKClose.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancelClose
            // 
            this.btnCancelClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancelClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelClose.Location = new System.Drawing.Point(419, 2);
            this.btnCancelClose.Name = "btnCancelClose";
            this.btnCancelClose.Size = new System.Drawing.Size(90, 30);
            this.btnCancelClose.TabIndex = 1;
            this.btnCancelClose.Tag = "Cancel";
            this.btnCancelClose.Text = "Cancel";
            this.btnCancelClose.UseVisualStyleBackColor = true;
            this.btnCancelClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.flpButton, 0, 1);
            this.tlpBase.Controls.Add(this.grbData, 0, 0);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 320F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Size = new System.Drawing.Size(841, 687);
            this.tlpBase.TabIndex = 4;
            // 
            // flpButton
            // 
            this.flpButton.Controls.Add(this.pnlAdd);
            this.flpButton.Controls.Add(this.dgvAddList);
            this.flpButton.Controls.Add(this.pnlButton);
            this.flpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButton.Location = new System.Drawing.Point(3, 323);
            this.flpButton.Name = "flpButton";
            this.flpButton.Size = new System.Drawing.Size(835, 361);
            this.flpButton.TabIndex = 0;
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnClear);
            this.pnlAdd.Controls.Add(this.btnAddOK);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(829, 36);
            this.pnlAdd.TabIndex = 0;
            // 
            // btnClear
            // 
            this.btnClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClear.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(419, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(90, 30);
            this.btnClear.TabIndex = 1;
            this.btnClear.Tag = "Clear";
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnOKClose);
            this.pnlButton.Controls.Add(this.btnCancelClose);
            this.pnlButton.Location = new System.Drawing.Point(3, 318);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(829, 36);
            this.pnlButton.TabIndex = 77;
            // 
            // grbData
            // 
            this.grbData.Controls.Add(this.pnlEdit);
            this.grbData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbData.Location = new System.Drawing.Point(3, 3);
            this.grbData.Name = "grbData";
            this.grbData.Size = new System.Drawing.Size(835, 314);
            this.grbData.TabIndex = 40;
            this.grbData.TabStop = false;
            // 
            // FormParamManagementBaseEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(841, 718);
            this.Name = "FormParamManagementBaseEdit";
            this.Text = "    ";
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddList)).EndInit();
            this.pnlEdit.ResumeLayout(false);
            this.pnlEdit.PerformLayout();
            this.tlpBase.ResumeLayout(false);
            this.flpButton.ResumeLayout(false);
            this.pnlAdd.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.grbData.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Button btnCancelClose;
        protected System.Windows.Forms.Button btnAddOK;
        protected System.Windows.Forms.TextBox txtBitLength;
        protected System.Windows.Forms.Label lblBitLength;
        protected System.Windows.Forms.TextBox txtBitOffsetPos;
        protected System.Windows.Forms.Label lblBitOffsetPos;
        protected System.Windows.Forms.TextBox txtWordLength;
        protected System.Windows.Forms.Label lblWordLength;
        protected System.Windows.Forms.TextBox txtWordOffsetPos;
        protected System.Windows.Forms.Label lblWordOffsetPos;
        protected System.Windows.Forms.TextBox txtUnit;
        protected System.Windows.Forms.Label lblUnit;
        protected System.Windows.Forms.Label lblDotRatioOperand;
        protected System.Windows.Forms.ComboBox cmbExpression;
        protected System.Windows.Forms.Label lblExpression;
        protected System.Windows.Forms.ComboBox cmbOperator;
        protected System.Windows.Forms.Label lblOperator;
        protected System.Windows.Forms.TextBox txtParameterName;
        protected System.Windows.Forms.TextBox txtSite;
        protected System.Windows.Forms.Label lblSite;
        protected System.Windows.Forms.TextBox txtDotRatioOperand;
        protected System.Windows.Forms.Label lblItem;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.TextBox txtRange;
        protected System.Windows.Forms.Label lblRange;
        protected System.Windows.Forms.TextBox txtItem;
        protected System.Windows.Forms.Label lblDescription;
        protected System.Windows.Forms.TextBox txtDescription;
        protected System.Windows.Forms.Label lblParameterName;
        protected System.Windows.Forms.TextBox txtSvID;
        protected System.Windows.Forms.Label lblSvID;
        protected System.Windows.Forms.Label label16;
        protected System.Windows.Forms.ComboBox cmbNode;
        protected System.Windows.Forms.Label lblNodeNo;
        protected System.Windows.Forms.Panel pnlEdit;
        protected System.Windows.Forms.Button btnOKClose;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox txtReportTo;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.FlowLayoutPanel flpButton;
        private System.Windows.Forms.GroupBox grbData;
        protected System.Windows.Forms.Panel pnlAdd;
        protected System.Windows.Forms.Panel pnlButton;
        protected System.Windows.Forms.TextBox txtJobData;
        protected System.Windows.Forms.Label lblJobData;
        protected System.Windows.Forms.ComboBox cmbUnit;
        protected System.Windows.Forms.Label lblUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSVID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParameterName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSite;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnit;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRange;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOperator;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDotRatio;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReportTo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExpression;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWoffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBitOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobDataItem;
        public System.Windows.Forms.DataGridView dgvAddList;
        protected System.Windows.Forms.Button btnClear;
    }
}