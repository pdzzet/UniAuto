namespace UniOPI
{
    partial class FormSECSVariableManagementEdit
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
            this.dgvAddList = new System.Windows.Forms.DataGridView();
            this.colLineID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTransferID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemSet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSP1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSP2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSP3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.lblQTimeID = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbtnSet = new System.Windows.Forms.RadioButton();
            this.rbtnNoUse = new System.Windows.Forms.RadioButton();
            this.lblStartNodeNo = new System.Windows.Forms.Label();
            this.txtSp3 = new System.Windows.Forms.TextBox();
            this.cmbTrID = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSp2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtItemID = new System.Windows.Forms.TextBox();
            this.txtSp1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtItemName = new System.Windows.Forms.TextBox();
            this.cmbDataType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbNode = new System.Windows.Forms.ComboBox();
            this.txtItemType = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
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
            this.groupBox2.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.flpButton.SuspendLayout();
            this.pnlAdd.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.grbData.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(789, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(849, 602);
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
            this.colDataType,
            this.colTransferID,
            this.colItemID,
            this.colItemName,
            this.colItemType,
            this.colItemSet,
            this.colDescription,
            this.colSP1,
            this.colSP2,
            this.colSP3});
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
            this.dgvAddList.RowHeadersWidth = 20;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvAddList.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvAddList.RowTemplate.Height = 24;
            this.dgvAddList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAddList.Size = new System.Drawing.Size(838, 228);
            this.dgvAddList.TabIndex = 118;
            this.dgvAddList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAddList_CellClick);
            // 
            // colLineID
            // 
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
            this.colLocalNo.Width = 120;
            // 
            // colDataType
            // 
            this.colDataType.DataPropertyName = "DATATYPE";
            this.colDataType.HeaderText = "DataType";
            this.colDataType.Name = "colDataType";
            this.colDataType.ReadOnly = true;
            this.colDataType.Width = 150;
            // 
            // colTransferID
            // 
            this.colTransferID.DataPropertyName = "TRID";
            this.colTransferID.HeaderText = "Transfer ID";
            this.colTransferID.Name = "colTransferID";
            this.colTransferID.ReadOnly = true;
            this.colTransferID.Width = 150;
            // 
            // colItemID
            // 
            this.colItemID.DataPropertyName = "ITEM_ID";
            this.colItemID.HeaderText = "Item ID";
            this.colItemID.Name = "colItemID";
            this.colItemID.ReadOnly = true;
            this.colItemID.Width = 150;
            // 
            // colItemName
            // 
            this.colItemName.DataPropertyName = "ITEM_NAME";
            this.colItemName.HeaderText = "Item Name";
            this.colItemName.Name = "colItemName";
            this.colItemName.ReadOnly = true;
            this.colItemName.Width = 150;
            // 
            // colItemType
            // 
            this.colItemType.HeaderText = "Item Type";
            this.colItemType.Name = "colItemType";
            this.colItemType.ReadOnly = true;
            this.colItemType.Width = 150;
            // 
            // colItemSet
            // 
            this.colItemSet.HeaderText = "Item Set";
            this.colItemSet.Name = "colItemSet";
            this.colItemSet.ReadOnly = true;
            this.colItemSet.Width = 150;
            // 
            // colDescription
            // 
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            this.colDescription.Width = 150;
            // 
            // colSP1
            // 
            this.colSP1.HeaderText = "SP 1";
            this.colSP1.Name = "colSP1";
            this.colSP1.ReadOnly = true;
            this.colSP1.Width = 150;
            // 
            // colSP2
            // 
            this.colSP2.HeaderText = "SP 2";
            this.colSP2.Name = "colSP2";
            this.colSP2.ReadOnly = true;
            this.colSP2.Width = 150;
            // 
            // colSP3
            // 
            this.colSP3.HeaderText = "SP 3";
            this.colSP3.Name = "colSP3";
            this.colSP3.ReadOnly = true;
            this.colSP3.Width = 150;
            // 
            // pnlEdit
            // 
            this.pnlEdit.Controls.Add(this.lblQTimeID);
            this.pnlEdit.Controls.Add(this.groupBox2);
            this.pnlEdit.Controls.Add(this.lblStartNodeNo);
            this.pnlEdit.Controls.Add(this.txtSp3);
            this.pnlEdit.Controls.Add(this.cmbTrID);
            this.pnlEdit.Controls.Add(this.label6);
            this.pnlEdit.Controls.Add(this.label1);
            this.pnlEdit.Controls.Add(this.txtSp2);
            this.pnlEdit.Controls.Add(this.label2);
            this.pnlEdit.Controls.Add(this.label7);
            this.pnlEdit.Controls.Add(this.txtItemID);
            this.pnlEdit.Controls.Add(this.txtSp1);
            this.pnlEdit.Controls.Add(this.label5);
            this.pnlEdit.Controls.Add(this.label8);
            this.pnlEdit.Controls.Add(this.txtItemName);
            this.pnlEdit.Controls.Add(this.cmbDataType);
            this.pnlEdit.Controls.Add(this.label4);
            this.pnlEdit.Controls.Add(this.cmbNode);
            this.pnlEdit.Controls.Add(this.txtItemType);
            this.pnlEdit.Controls.Add(this.txtDescription);
            this.pnlEdit.Controls.Add(this.lblDescription);
            this.pnlEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEdit.Location = new System.Drawing.Point(3, 18);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(837, 219);
            this.pnlEdit.TabIndex = 117;
            // 
            // lblQTimeID
            // 
            this.lblQTimeID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQTimeID.Location = new System.Drawing.Point(34, 13);
            this.lblQTimeID.Name = "lblQTimeID";
            this.lblQTimeID.Size = new System.Drawing.Size(124, 21);
            this.lblQTimeID.TabIndex = 95;
            this.lblQTimeID.Text = "Local";
            this.lblQTimeID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbtnSet);
            this.groupBox2.Controls.Add(this.rbtnNoUse);
            this.groupBox2.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold);
            this.groupBox2.Location = new System.Drawing.Point(479, 148);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(293, 59);
            this.groupBox2.TabIndex = 116;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Item Set";
            // 
            // rbtnSet
            // 
            this.rbtnSet.AutoSize = true;
            this.rbtnSet.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnSet.Location = new System.Drawing.Point(149, 27);
            this.rbtnSet.Name = "rbtnSet";
            this.rbtnSet.Size = new System.Drawing.Size(50, 25);
            this.rbtnSet.TabIndex = 1;
            this.rbtnSet.TabStop = true;
            this.rbtnSet.Text = "Set";
            this.rbtnSet.UseVisualStyleBackColor = true;
            // 
            // rbtnNoUse
            // 
            this.rbtnNoUse.AutoSize = true;
            this.rbtnNoUse.Checked = true;
            this.rbtnNoUse.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbtnNoUse.Location = new System.Drawing.Point(45, 26);
            this.rbtnNoUse.Name = "rbtnNoUse";
            this.rbtnNoUse.Size = new System.Drawing.Size(78, 25);
            this.rbtnNoUse.TabIndex = 0;
            this.rbtnNoUse.TabStop = true;
            this.rbtnNoUse.Text = "No Use";
            this.rbtnNoUse.UseVisualStyleBackColor = true;
            // 
            // lblStartNodeNo
            // 
            this.lblStartNodeNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStartNodeNo.Location = new System.Drawing.Point(34, 83);
            this.lblStartNodeNo.Name = "lblStartNodeNo";
            this.lblStartNodeNo.Size = new System.Drawing.Size(124, 21);
            this.lblStartNodeNo.TabIndex = 93;
            this.lblStartNodeNo.Text = "Transfer ID";
            this.lblStartNodeNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSp3
            // 
            this.txtSp3.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSp3.Location = new System.Drawing.Point(584, 77);
            this.txtSp3.Name = "txtSp3";
            this.txtSp3.Size = new System.Drawing.Size(188, 28);
            this.txtSp3.TabIndex = 114;
            // 
            // cmbTrID
            // 
            this.cmbTrID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTrID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbTrID.FormattingEnabled = true;
            this.cmbTrID.Location = new System.Drawing.Point(161, 79);
            this.cmbTrID.Name = "cmbTrID";
            this.cmbTrID.Size = new System.Drawing.Size(188, 29);
            this.cmbTrID.TabIndex = 92;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(387, 81);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(191, 21);
            this.label6.TabIndex = 115;
            this.label6.Text = "SP 3";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(34, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 21);
            this.label1.TabIndex = 97;
            this.label1.Text = "Data Type";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSp2
            // 
            this.txtSp2.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSp2.Location = new System.Drawing.Point(584, 43);
            this.txtSp2.Name = "txtSp2";
            this.txtSp2.Size = new System.Drawing.Size(188, 28);
            this.txtSp2.TabIndex = 112;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(34, 118);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 21);
            this.label2.TabIndex = 99;
            this.label2.Text = "Item ID";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(387, 47);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(191, 21);
            this.label7.TabIndex = 113;
            this.label7.Text = "SP 2";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtItemID
            // 
            this.txtItemID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtItemID.Location = new System.Drawing.Point(161, 114);
            this.txtItemID.Name = "txtItemID";
            this.txtItemID.Size = new System.Drawing.Size(188, 28);
            this.txtItemID.TabIndex = 98;
            // 
            // txtSp1
            // 
            this.txtSp1.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSp1.Location = new System.Drawing.Point(584, 9);
            this.txtSp1.Name = "txtSp1";
            this.txtSp1.Size = new System.Drawing.Size(188, 28);
            this.txtSp1.TabIndex = 110;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(34, 152);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(124, 21);
            this.label5.TabIndex = 101;
            this.label5.Text = "Item Name";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(387, 13);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(191, 21);
            this.label8.TabIndex = 111;
            this.label8.Text = "SP 1";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtItemName
            // 
            this.txtItemName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtItemName.Location = new System.Drawing.Point(161, 148);
            this.txtItemName.Name = "txtItemName";
            this.txtItemName.Size = new System.Drawing.Size(188, 28);
            this.txtItemName.TabIndex = 100;
            // 
            // cmbDataType
            // 
            this.cmbDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDataType.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbDataType.FormattingEnabled = true;
            this.cmbDataType.Location = new System.Drawing.Point(161, 44);
            this.cmbDataType.Name = "cmbDataType";
            this.cmbDataType.Size = new System.Drawing.Size(188, 29);
            this.cmbDataType.TabIndex = 107;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(34, 186);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(124, 21);
            this.label4.TabIndex = 103;
            this.label4.Text = "Item Type";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbNode
            // 
            this.cmbNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbNode.FormattingEnabled = true;
            this.cmbNode.Location = new System.Drawing.Point(161, 9);
            this.cmbNode.Name = "cmbNode";
            this.cmbNode.Size = new System.Drawing.Size(188, 29);
            this.cmbNode.TabIndex = 106;
            // 
            // txtItemType
            // 
            this.txtItemType.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtItemType.Location = new System.Drawing.Point(161, 182);
            this.txtItemType.Name = "txtItemType";
            this.txtItemType.Size = new System.Drawing.Size(188, 28);
            this.txtItemType.TabIndex = 102;
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.Location = new System.Drawing.Point(584, 111);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(188, 28);
            this.txtDescription.TabIndex = 104;
            // 
            // lblDescription
            // 
            this.lblDescription.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescription.Location = new System.Drawing.Point(433, 115);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(148, 21);
            this.lblDescription.TabIndex = 105;
            this.lblDescription.Text = "Description";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnAddOK
            // 
            this.btnAddOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddOK.Location = new System.Drawing.Point(323, 3);
            this.btnAddOK.Name = "btnAddOK";
            this.btnAddOK.Size = new System.Drawing.Size(90, 30);
            this.btnAddOK.TabIndex = 0;
            this.btnAddOK.Tag = "ADD";
            this.btnAddOK.Text = "ADD";
            this.btnAddOK.UseVisualStyleBackColor = true;
            this.btnAddOK.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnOKClose
            // 
            this.btnOKClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOKClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOKClose.Location = new System.Drawing.Point(323, 4);
            this.btnOKClose.Name = "btnOKClose";
            this.btnOKClose.Size = new System.Drawing.Size(90, 30);
            this.btnOKClose.TabIndex = 73;
            this.btnOKClose.Tag = "OK";
            this.btnOKClose.Text = "OK";
            this.btnOKClose.UseVisualStyleBackColor = true;
            this.btnOKClose.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnCancelClose
            // 
            this.btnCancelClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancelClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelClose.Location = new System.Drawing.Point(419, 4);
            this.btnCancelClose.Name = "btnCancelClose";
            this.btnCancelClose.Size = new System.Drawing.Size(90, 30);
            this.btnCancelClose.TabIndex = 1;
            this.btnCancelClose.Tag = "Cancel";
            this.btnCancelClose.Text = "Cancel";
            this.btnCancelClose.UseVisualStyleBackColor = true;
            this.btnCancelClose.Click += new System.EventHandler(this.btn_Click);
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
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 246F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Size = new System.Drawing.Size(849, 571);
            this.tlpBase.TabIndex = 6;
            // 
            // flpButton
            // 
            this.flpButton.Controls.Add(this.pnlAdd);
            this.flpButton.Controls.Add(this.dgvAddList);
            this.flpButton.Controls.Add(this.pnlButton);
            this.flpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButton.Location = new System.Drawing.Point(3, 249);
            this.flpButton.Name = "flpButton";
            this.flpButton.Size = new System.Drawing.Size(843, 319);
            this.flpButton.TabIndex = 0;
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnClear);
            this.pnlAdd.Controls.Add(this.btnAddOK);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(838, 36);
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
            this.btnClear.Click += new System.EventHandler(this.btn_Click);
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnOKClose);
            this.pnlButton.Controls.Add(this.btnCancelClose);
            this.pnlButton.Location = new System.Drawing.Point(3, 279);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(837, 36);
            this.pnlButton.TabIndex = 77;
            // 
            // grbData
            // 
            this.grbData.Controls.Add(this.pnlEdit);
            this.grbData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbData.Location = new System.Drawing.Point(3, 3);
            this.grbData.Name = "grbData";
            this.grbData.Size = new System.Drawing.Size(843, 240);
            this.grbData.TabIndex = 40;
            this.grbData.TabStop = false;
            // 
            // FormSECSVariableManagementEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(849, 602);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormSECSVariableManagementEdit";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormSECSVariableManagementEdit_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddList)).EndInit();
            this.pnlEdit.ResumeLayout(false);
            this.pnlEdit.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tlpBase.ResumeLayout(false);
            this.flpButton.ResumeLayout(false);
            this.pnlAdd.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.grbData.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancelClose;
        private System.Windows.Forms.Button btnAddOK;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtItemType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtItemName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtItemID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblQTimeID;
        private System.Windows.Forms.ComboBox cmbTrID;
        private System.Windows.Forms.Label lblStartNodeNo;
        private System.Windows.Forms.ComboBox cmbDataType;
        private System.Windows.Forms.ComboBox cmbNode;
        private System.Windows.Forms.TextBox txtSp3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtSp2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtSp1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbtnSet;
        private System.Windows.Forms.RadioButton rbtnNoUse;
        private System.Windows.Forms.Panel pnlEdit;
        private System.Windows.Forms.DataGridView dgvAddList;
        private System.Windows.Forms.Button btnOKClose;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.FlowLayoutPanel flpButton;
        private System.Windows.Forms.Panel pnlAdd;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.GroupBox grbData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTransferID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemSet;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSP1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSP2;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSP3;
    }
}