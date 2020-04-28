namespace UniOPI
{
    partial class FormQTimeSettingEdit
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
            this.colQTimeID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartLocalID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartUnitNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartUnitID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartEventMsg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndLocalID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndUnitNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndUnitID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEndEventMsg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNGQTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemark = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStartLocalRecipeID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCFReworkQTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEnabled = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.lblReworkDesc = new System.Windows.Forms.Label();
            this.lblQTimeID = new System.Windows.Forms.Label();
            this.txtCFRWQTime = new System.Windows.Forms.TextBox();
            this.lblStartNodeNo = new System.Windows.Forms.Label();
            this.lblCFRWQTime = new System.Windows.Forms.Label();
            this.txtRemark = new System.Windows.Forms.TextBox();
            this.btnStartNodeRecipeID = new System.Windows.Forms.Button();
            this.lblRemark = new System.Windows.Forms.Label();
            this.txtStartNodeRecipeID = new System.Windows.Forms.TextBox();
            this.lblStartEvtMsg = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbStartEvtMsg = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSetTimeVal = new System.Windows.Forms.Label();
            this.cmbEndUnitNo = new System.Windows.Forms.ComboBox();
            this.txtSetTimeValue = new System.Windows.Forms.TextBox();
            this.lblEndUnitNoSet = new System.Windows.Forms.Label();
            this.cmbStartNodeNo = new System.Windows.Forms.ComboBox();
            this.cmbEndNodeNo = new System.Windows.Forms.ComboBox();
            this.lblStartUnitNoSet = new System.Windows.Forms.Label();
            this.cmbEndEvtMsg = new System.Windows.Forms.ComboBox();
            this.cmbStartUnitNo = new System.Windows.Forms.ComboBox();
            this.lblEndEvtMsg = new System.Windows.Forms.Label();
            this.txtQTimeID = new System.Windows.Forms.TextBox();
            this.lblEndNodeNo = new System.Windows.Forms.Label();
            this.btnAddOK = new System.Windows.Forms.Button();
            this.btnCancelClose = new System.Windows.Forms.Button();
            this.btnOKClose = new System.Windows.Forms.Button();
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
            this.lblCaption.Size = new System.Drawing.Size(724, 30);
            this.lblCaption.Text = "Q Time Setting";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(784, 652);
            // 
            // dgvAddList
            // 
            this.dgvAddList.AllowUserToAddRows = false;
            this.dgvAddList.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAddList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvAddList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAddList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLineID,
            this.colQTimeID,
            this.colStartLocalNo,
            this.colStartLocalID,
            this.colStartUnitNo,
            this.colStartUnitID,
            this.colStartEventMsg,
            this.colEndLocalNo,
            this.colEndLocalID,
            this.colEndUnitNo,
            this.colEndUnitID,
            this.colEndEventMsg,
            this.colNGQTime,
            this.colRemark,
            this.colStartLocalRecipeID,
            this.colCFReworkQTime,
            this.colEnabled});
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
            this.dgvAddList.RowHeadersWidth = 10;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvAddList.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvAddList.RowTemplate.Height = 24;
            this.dgvAddList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAddList.Size = new System.Drawing.Size(778, 228);
            this.dgvAddList.TabIndex = 87;
            this.dgvAddList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAddList_CellClick);
            // 
            // colLineID
            // 
            this.colLineID.HeaderText = "Line ID";
            this.colLineID.Name = "colLineID";
            this.colLineID.ReadOnly = true;
            // 
            // colQTimeID
            // 
            this.colQTimeID.DataPropertyName = "QTIMEID";
            this.colQTimeID.HeaderText = "Q Time ID";
            this.colQTimeID.Name = "colQTimeID";
            this.colQTimeID.ReadOnly = true;
            // 
            // colStartLocalNo
            // 
            this.colStartLocalNo.DataPropertyName = "STARTNODENO";
            this.colStartLocalNo.HeaderText = "Start Local No";
            this.colStartLocalNo.Name = "colStartLocalNo";
            this.colStartLocalNo.ReadOnly = true;
            this.colStartLocalNo.Width = 140;
            // 
            // colStartLocalID
            // 
            this.colStartLocalID.DataPropertyName = "STARTNODEID";
            this.colStartLocalID.HeaderText = "Start Local ID";
            this.colStartLocalID.Name = "colStartLocalID";
            this.colStartLocalID.ReadOnly = true;
            this.colStartLocalID.Width = 200;
            // 
            // colStartUnitNo
            // 
            this.colStartUnitNo.DataPropertyName = "STARTUNITNO";
            this.colStartUnitNo.HeaderText = "Start Unit No";
            this.colStartUnitNo.Name = "colStartUnitNo";
            this.colStartUnitNo.ReadOnly = true;
            this.colStartUnitNo.Width = 140;
            // 
            // colStartUnitID
            // 
            this.colStartUnitID.DataPropertyName = "STARTUNITID";
            this.colStartUnitID.HeaderText = "Start Unit ID";
            this.colStartUnitID.Name = "colStartUnitID";
            this.colStartUnitID.ReadOnly = true;
            this.colStartUnitID.Width = 150;
            // 
            // colStartEventMsg
            // 
            this.colStartEventMsg.DataPropertyName = "STARTEVENTMSG";
            this.colStartEventMsg.HeaderText = "Start Event Msg";
            this.colStartEventMsg.Name = "colStartEventMsg";
            this.colStartEventMsg.ReadOnly = true;
            this.colStartEventMsg.Width = 150;
            // 
            // colEndLocalNo
            // 
            this.colEndLocalNo.DataPropertyName = "ENDNODENO";
            this.colEndLocalNo.HeaderText = "End Local No";
            this.colEndLocalNo.Name = "colEndLocalNo";
            this.colEndLocalNo.ReadOnly = true;
            this.colEndLocalNo.Width = 130;
            // 
            // colEndLocalID
            // 
            this.colEndLocalID.HeaderText = "End Local ID";
            this.colEndLocalID.Name = "colEndLocalID";
            this.colEndLocalID.ReadOnly = true;
            this.colEndLocalID.Width = 200;
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
            this.colEndUnitID.HeaderText = "End Unit ID";
            this.colEndUnitID.Name = "colEndUnitID";
            this.colEndUnitID.ReadOnly = true;
            this.colEndUnitID.Width = 150;
            // 
            // colEndEventMsg
            // 
            this.colEndEventMsg.DataPropertyName = "ENDEVENTMSG";
            this.colEndEventMsg.HeaderText = "End Event Msg";
            this.colEndEventMsg.Name = "colEndEventMsg";
            this.colEndEventMsg.ReadOnly = true;
            this.colEndEventMsg.Width = 150;
            // 
            // colNGQTime
            // 
            this.colNGQTime.DataPropertyName = "SETTIMEVALUE";
            this.colNGQTime.HeaderText = "NG Q Time";
            this.colNGQTime.Name = "colNGQTime";
            this.colNGQTime.ReadOnly = true;
            this.colNGQTime.Width = 150;
            // 
            // colRemark
            // 
            this.colRemark.DataPropertyName = "REMARK";
            this.colRemark.HeaderText = "Remark";
            this.colRemark.Name = "colRemark";
            this.colRemark.ReadOnly = true;
            // 
            // colStartLocalRecipeID
            // 
            this.colStartLocalRecipeID.DataPropertyName = "STARTNODERECIPEID";
            this.colStartLocalRecipeID.HeaderText = "Start Local Recipe ID";
            this.colStartLocalRecipeID.Name = "colStartLocalRecipeID";
            this.colStartLocalRecipeID.ReadOnly = true;
            this.colStartLocalRecipeID.Width = 180;
            // 
            // colCFReworkQTime
            // 
            this.colCFReworkQTime.DataPropertyName = "CFRWQTIME";
            this.colCFReworkQTime.HeaderText = "Coater Rework Q Time";
            this.colCFReworkQTime.Name = "colCFReworkQTime";
            this.colCFReworkQTime.ReadOnly = true;
            this.colCFReworkQTime.Width = 200;
            // 
            // colEnabled
            // 
            this.colEnabled.DataPropertyName = "ENABLED";
            this.colEnabled.HeaderText = "Enabled";
            this.colEnabled.Name = "colEnabled";
            this.colEnabled.ReadOnly = true;
            // 
            // pnlEdit
            // 
            this.pnlEdit.Controls.Add(this.chkEnabled);
            this.pnlEdit.Controls.Add(this.lblReworkDesc);
            this.pnlEdit.Controls.Add(this.lblQTimeID);
            this.pnlEdit.Controls.Add(this.txtCFRWQTime);
            this.pnlEdit.Controls.Add(this.lblStartNodeNo);
            this.pnlEdit.Controls.Add(this.lblCFRWQTime);
            this.pnlEdit.Controls.Add(this.txtRemark);
            this.pnlEdit.Controls.Add(this.btnStartNodeRecipeID);
            this.pnlEdit.Controls.Add(this.lblRemark);
            this.pnlEdit.Controls.Add(this.txtStartNodeRecipeID);
            this.pnlEdit.Controls.Add(this.lblStartEvtMsg);
            this.pnlEdit.Controls.Add(this.label2);
            this.pnlEdit.Controls.Add(this.cmbStartEvtMsg);
            this.pnlEdit.Controls.Add(this.label1);
            this.pnlEdit.Controls.Add(this.lblSetTimeVal);
            this.pnlEdit.Controls.Add(this.cmbEndUnitNo);
            this.pnlEdit.Controls.Add(this.txtSetTimeValue);
            this.pnlEdit.Controls.Add(this.lblEndUnitNoSet);
            this.pnlEdit.Controls.Add(this.cmbStartNodeNo);
            this.pnlEdit.Controls.Add(this.cmbEndNodeNo);
            this.pnlEdit.Controls.Add(this.lblStartUnitNoSet);
            this.pnlEdit.Controls.Add(this.cmbEndEvtMsg);
            this.pnlEdit.Controls.Add(this.cmbStartUnitNo);
            this.pnlEdit.Controls.Add(this.lblEndEvtMsg);
            this.pnlEdit.Controls.Add(this.txtQTimeID);
            this.pnlEdit.Controls.Add(this.lblEndNodeNo);
            this.pnlEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEdit.Location = new System.Drawing.Point(3, 18);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(772, 273);
            this.pnlEdit.TabIndex = 86;
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkEnabled.Location = new System.Drawing.Point(414, 5);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(134, 26);
            this.chkEnabled.TabIndex = 93;
            this.chkEnabled.Text = "Q Time Enable";
            this.chkEnabled.UseVisualStyleBackColor = true;
            // 
            // lblReworkDesc
            // 
            this.lblReworkDesc.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReworkDesc.ForeColor = System.Drawing.Color.Red;
            this.lblReworkDesc.Location = new System.Drawing.Point(296, 245);
            this.lblReworkDesc.Name = "lblReworkDesc";
            this.lblReworkDesc.Size = new System.Drawing.Size(101, 21);
            this.lblReworkDesc.TabIndex = 86;
            this.lblReworkDesc.Text = "0-65535 / sec";
            this.lblReworkDesc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblQTimeID
            // 
            this.lblQTimeID.BackColor = System.Drawing.Color.Black;
            this.lblQTimeID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblQTimeID.ForeColor = System.Drawing.Color.White;
            this.lblQTimeID.Location = new System.Drawing.Point(9, 6);
            this.lblQTimeID.Name = "lblQTimeID";
            this.lblQTimeID.Size = new System.Drawing.Size(185, 25);
            this.lblQTimeID.TabIndex = 73;
            this.lblQTimeID.Text = "QTime ID";
            this.lblQTimeID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCFRWQTime
            // 
            this.txtCFRWQTime.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCFRWQTime.Location = new System.Drawing.Point(195, 238);
            this.txtCFRWQTime.MaxLength = 5;
            this.txtCFRWQTime.Name = "txtCFRWQTime";
            this.txtCFRWQTime.Size = new System.Drawing.Size(99, 28);
            this.txtCFRWQTime.TabIndex = 84;
            this.txtCFRWQTime.Text = "0";
            this.txtCFRWQTime.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNum_KeyPress);
            // 
            // lblStartNodeNo
            // 
            this.lblStartNodeNo.BackColor = System.Drawing.Color.Black;
            this.lblStartNodeNo.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStartNodeNo.ForeColor = System.Drawing.Color.White;
            this.lblStartNodeNo.Location = new System.Drawing.Point(9, 39);
            this.lblStartNodeNo.Name = "lblStartNodeNo";
            this.lblStartNodeNo.Size = new System.Drawing.Size(185, 25);
            this.lblStartNodeNo.TabIndex = 40;
            this.lblStartNodeNo.Text = "Start Local ID";
            this.lblStartNodeNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCFRWQTime
            // 
            this.lblCFRWQTime.BackColor = System.Drawing.Color.Black;
            this.lblCFRWQTime.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblCFRWQTime.ForeColor = System.Drawing.Color.White;
            this.lblCFRWQTime.Location = new System.Drawing.Point(9, 240);
            this.lblCFRWQTime.Name = "lblCFRWQTime";
            this.lblCFRWQTime.Size = new System.Drawing.Size(185, 25);
            this.lblCFRWQTime.TabIndex = 85;
            this.lblCFRWQTime.Text = "Coater Rework Q Time";
            this.lblCFRWQTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtRemark
            // 
            this.txtRemark.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRemark.Location = new System.Drawing.Point(195, 205);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Size = new System.Drawing.Size(558, 28);
            this.txtRemark.TabIndex = 4;
            // 
            // btnStartNodeRecipeID
            // 
            this.btnStartNodeRecipeID.Location = new System.Drawing.Point(647, 175);
            this.btnStartNodeRecipeID.Name = "btnStartNodeRecipeID";
            this.btnStartNodeRecipeID.Size = new System.Drawing.Size(57, 23);
            this.btnStartNodeRecipeID.TabIndex = 83;
            this.btnStartNodeRecipeID.Text = "。。。";
            this.btnStartNodeRecipeID.UseVisualStyleBackColor = true;
            this.btnStartNodeRecipeID.Click += new System.EventHandler(this.btnStartNodeRecipeID_Click);
            // 
            // lblRemark
            // 
            this.lblRemark.BackColor = System.Drawing.Color.Black;
            this.lblRemark.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRemark.ForeColor = System.Drawing.Color.White;
            this.lblRemark.Location = new System.Drawing.Point(9, 207);
            this.lblRemark.Name = "lblRemark";
            this.lblRemark.Size = new System.Drawing.Size(185, 25);
            this.lblRemark.TabIndex = 45;
            this.lblRemark.Text = "Remark";
            this.lblRemark.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtStartNodeRecipeID
            // 
            this.txtStartNodeRecipeID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStartNodeRecipeID.Location = new System.Drawing.Point(542, 173);
            this.txtStartNodeRecipeID.Name = "txtStartNodeRecipeID";
            this.txtStartNodeRecipeID.Size = new System.Drawing.Size(99, 28);
            this.txtStartNodeRecipeID.TabIndex = 81;
            // 
            // lblStartEvtMsg
            // 
            this.lblStartEvtMsg.BackColor = System.Drawing.Color.Black;
            this.lblStartEvtMsg.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStartEvtMsg.ForeColor = System.Drawing.Color.White;
            this.lblStartEvtMsg.Location = new System.Drawing.Point(9, 73);
            this.lblStartEvtMsg.Name = "lblStartEvtMsg";
            this.lblStartEvtMsg.Size = new System.Drawing.Size(185, 25);
            this.lblStartEvtMsg.TabIndex = 50;
            this.lblStartEvtMsg.Text = "Start Event Message";
            this.lblStartEvtMsg.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Black;
            this.label2.Font = new System.Drawing.Font("Calibri", 13F);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(411, 175);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 25);
            this.label2.TabIndex = 82;
            this.label2.Text = "Recipe ID";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbStartEvtMsg
            // 
            this.cmbStartEvtMsg.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStartEvtMsg.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbStartEvtMsg.FormattingEnabled = true;
            this.cmbStartEvtMsg.Location = new System.Drawing.Point(195, 71);
            this.cmbStartEvtMsg.Name = "cmbStartEvtMsg";
            this.cmbStartEvtMsg.Size = new System.Drawing.Size(558, 29);
            this.cmbStartEvtMsg.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(296, 180);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 21);
            this.label1.TabIndex = 80;
            this.label1.Text = "0-65535 / sec";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSetTimeVal
            // 
            this.lblSetTimeVal.BackColor = System.Drawing.Color.Black;
            this.lblSetTimeVal.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblSetTimeVal.ForeColor = System.Drawing.Color.White;
            this.lblSetTimeVal.Location = new System.Drawing.Point(9, 175);
            this.lblSetTimeVal.Name = "lblSetTimeVal";
            this.lblSetTimeVal.Size = new System.Drawing.Size(185, 25);
            this.lblSetTimeVal.TabIndex = 60;
            this.lblSetTimeVal.Text = "NG Q Time";
            this.lblSetTimeVal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbEndUnitNo
            // 
            this.cmbEndUnitNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEndUnitNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbEndUnitNo.FormattingEnabled = true;
            this.cmbEndUnitNo.Location = new System.Drawing.Point(542, 105);
            this.cmbEndUnitNo.Name = "cmbEndUnitNo";
            this.cmbEndUnitNo.Size = new System.Drawing.Size(212, 29);
            this.cmbEndUnitNo.TabIndex = 75;
            // 
            // txtSetTimeValue
            // 
            this.txtSetTimeValue.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSetTimeValue.Location = new System.Drawing.Point(195, 173);
            this.txtSetTimeValue.MaxLength = 5;
            this.txtSetTimeValue.Name = "txtSetTimeValue";
            this.txtSetTimeValue.Size = new System.Drawing.Size(99, 28);
            this.txtSetTimeValue.TabIndex = 3;
            this.txtSetTimeValue.Text = "0";
            this.txtSetTimeValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNum_KeyPress);
            // 
            // lblEndUnitNoSet
            // 
            this.lblEndUnitNoSet.BackColor = System.Drawing.Color.Black;
            this.lblEndUnitNoSet.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblEndUnitNoSet.ForeColor = System.Drawing.Color.White;
            this.lblEndUnitNoSet.Location = new System.Drawing.Point(411, 107);
            this.lblEndUnitNoSet.Name = "lblEndUnitNoSet";
            this.lblEndUnitNoSet.Size = new System.Drawing.Size(130, 25);
            this.lblEndUnitNoSet.TabIndex = 79;
            this.lblEndUnitNoSet.Text = "End Unit ID";
            this.lblEndUnitNoSet.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbStartNodeNo
            // 
            this.cmbStartNodeNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStartNodeNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbStartNodeNo.FormattingEnabled = true;
            this.cmbStartNodeNo.Location = new System.Drawing.Point(195, 37);
            this.cmbStartNodeNo.Name = "cmbStartNodeNo";
            this.cmbStartNodeNo.Size = new System.Drawing.Size(188, 29);
            this.cmbStartNodeNo.TabIndex = 0;
            this.cmbStartNodeNo.SelectedIndexChanged += new System.EventHandler(this.cmbStartNodeNo_SelectedIndexChanged);
            // 
            // cmbEndNodeNo
            // 
            this.cmbEndNodeNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEndNodeNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbEndNodeNo.FormattingEnabled = true;
            this.cmbEndNodeNo.Location = new System.Drawing.Point(195, 105);
            this.cmbEndNodeNo.Name = "cmbEndNodeNo";
            this.cmbEndNodeNo.Size = new System.Drawing.Size(188, 29);
            this.cmbEndNodeNo.TabIndex = 74;
            this.cmbEndNodeNo.SelectedIndexChanged += new System.EventHandler(this.cmbEndNodeNo_SelectedIndexChanged);
            // 
            // lblStartUnitNoSet
            // 
            this.lblStartUnitNoSet.BackColor = System.Drawing.Color.Black;
            this.lblStartUnitNoSet.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStartUnitNoSet.ForeColor = System.Drawing.Color.White;
            this.lblStartUnitNoSet.Location = new System.Drawing.Point(411, 39);
            this.lblStartUnitNoSet.Name = "lblStartUnitNoSet";
            this.lblStartUnitNoSet.Size = new System.Drawing.Size(130, 25);
            this.lblStartUnitNoSet.TabIndex = 71;
            this.lblStartUnitNoSet.Text = "Start Unit ID";
            this.lblStartUnitNoSet.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbEndEvtMsg
            // 
            this.cmbEndEvtMsg.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEndEvtMsg.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbEndEvtMsg.FormattingEnabled = true;
            this.cmbEndEvtMsg.Location = new System.Drawing.Point(195, 139);
            this.cmbEndEvtMsg.Name = "cmbEndEvtMsg";
            this.cmbEndEvtMsg.Size = new System.Drawing.Size(558, 29);
            this.cmbEndEvtMsg.TabIndex = 76;
            // 
            // cmbStartUnitNo
            // 
            this.cmbStartUnitNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStartUnitNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbStartUnitNo.FormattingEnabled = true;
            this.cmbStartUnitNo.Location = new System.Drawing.Point(542, 37);
            this.cmbStartUnitNo.Name = "cmbStartUnitNo";
            this.cmbStartUnitNo.Size = new System.Drawing.Size(212, 29);
            this.cmbStartUnitNo.TabIndex = 1;
            // 
            // lblEndEvtMsg
            // 
            this.lblEndEvtMsg.BackColor = System.Drawing.Color.Black;
            this.lblEndEvtMsg.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblEndEvtMsg.ForeColor = System.Drawing.Color.White;
            this.lblEndEvtMsg.Location = new System.Drawing.Point(9, 141);
            this.lblEndEvtMsg.Name = "lblEndEvtMsg";
            this.lblEndEvtMsg.Size = new System.Drawing.Size(185, 25);
            this.lblEndEvtMsg.TabIndex = 78;
            this.lblEndEvtMsg.Text = "End Event Message";
            this.lblEndEvtMsg.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtQTimeID
            // 
            this.txtQTimeID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtQTimeID.Location = new System.Drawing.Point(195, 5);
            this.txtQTimeID.Name = "txtQTimeID";
            this.txtQTimeID.Size = new System.Drawing.Size(188, 28);
            this.txtQTimeID.TabIndex = 72;
            // 
            // lblEndNodeNo
            // 
            this.lblEndNodeNo.BackColor = System.Drawing.Color.Black;
            this.lblEndNodeNo.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblEndNodeNo.ForeColor = System.Drawing.Color.White;
            this.lblEndNodeNo.Location = new System.Drawing.Point(9, 107);
            this.lblEndNodeNo.Name = "lblEndNodeNo";
            this.lblEndNodeNo.Size = new System.Drawing.Size(185, 25);
            this.lblEndNodeNo.TabIndex = 77;
            this.lblEndNodeNo.Text = "End Local ID";
            this.lblEndNodeNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            // btnCancelClose
            // 
            this.btnCancelClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancelClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelClose.Location = new System.Drawing.Point(390, 4);
            this.btnCancelClose.Name = "btnCancelClose";
            this.btnCancelClose.Size = new System.Drawing.Size(90, 30);
            this.btnCancelClose.TabIndex = 2;
            this.btnCancelClose.Tag = "Cancel";
            this.btnCancelClose.Text = "Cancel";
            this.btnCancelClose.UseVisualStyleBackColor = true;
            this.btnCancelClose.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnOKClose
            // 
            this.btnOKClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOKClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOKClose.Location = new System.Drawing.Point(284, 3);
            this.btnOKClose.Name = "btnOKClose";
            this.btnOKClose.Size = new System.Drawing.Size(90, 30);
            this.btnOKClose.TabIndex = 1;
            this.btnOKClose.Tag = "OK";
            this.btnOKClose.Text = "OK";
            this.btnOKClose.UseVisualStyleBackColor = true;
            this.btnOKClose.Click += new System.EventHandler(this.btn_Click);
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
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Size = new System.Drawing.Size(784, 621);
            this.tlpBase.TabIndex = 7;
            // 
            // flpButton
            // 
            this.flpButton.Controls.Add(this.pnlAdd);
            this.flpButton.Controls.Add(this.dgvAddList);
            this.flpButton.Controls.Add(this.pnlButton);
            this.flpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButton.Location = new System.Drawing.Point(3, 303);
            this.flpButton.Name = "flpButton";
            this.flpButton.Size = new System.Drawing.Size(778, 315);
            this.flpButton.TabIndex = 0;
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnClear);
            this.pnlAdd.Controls.Add(this.btnAddOK);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(775, 36);
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
            this.pnlButton.Controls.Add(this.btnCancelClose);
            this.pnlButton.Controls.Add(this.btnOKClose);
            this.pnlButton.Location = new System.Drawing.Point(3, 279);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(772, 36);
            this.pnlButton.TabIndex = 77;
            // 
            // grbData
            // 
            this.grbData.Controls.Add(this.pnlEdit);
            this.grbData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbData.Location = new System.Drawing.Point(3, 3);
            this.grbData.Name = "grbData";
            this.grbData.Size = new System.Drawing.Size(778, 294);
            this.grbData.TabIndex = 40;
            this.grbData.TabStop = false;
            // 
            // FormQTimeSettingEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(784, 652);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormQTimeSettingEdit";
            this.Text = "   ";
            this.Load += new System.EventHandler(this.FormQTimeSettingEdit_Load);
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

        private System.Windows.Forms.TextBox txtSetTimeValue;
        private System.Windows.Forms.Label lblSetTimeVal;
        private System.Windows.Forms.ComboBox cmbStartEvtMsg;
        private System.Windows.Forms.Label lblStartEvtMsg;
        private System.Windows.Forms.Label lblRemark;
        private System.Windows.Forms.TextBox txtRemark;
        private System.Windows.Forms.Label lblStartNodeNo;
        private System.Windows.Forms.Button btnOKClose;
        private System.Windows.Forms.Button btnAddOK;
        private System.Windows.Forms.ComboBox cmbStartUnitNo;
        private System.Windows.Forms.Label lblStartUnitNoSet;
        private System.Windows.Forms.ComboBox cmbStartNodeNo;
        private System.Windows.Forms.TextBox txtQTimeID;
        private System.Windows.Forms.Label lblQTimeID;
        private System.Windows.Forms.ComboBox cmbEndUnitNo;
        private System.Windows.Forms.Label lblEndUnitNoSet;
        private System.Windows.Forms.ComboBox cmbEndNodeNo;
        private System.Windows.Forms.ComboBox cmbEndEvtMsg;
        private System.Windows.Forms.Label lblEndEvtMsg;
        private System.Windows.Forms.Label lblEndNodeNo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStartNodeRecipeID;
        private System.Windows.Forms.TextBox txtStartNodeRecipeID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCFRWQTime;
        private System.Windows.Forms.Label lblCFRWQTime;
        private System.Windows.Forms.Panel pnlEdit;
        private System.Windows.Forms.DataGridView dgvAddList;
        private System.Windows.Forms.Button btnCancelClose;
        private System.Windows.Forms.Label lblReworkDesc;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.FlowLayoutPanel flpButton;
        private System.Windows.Forms.Panel pnlAdd;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.GroupBox grbData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQTimeID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartLocalID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartUnitID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartEventMsg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndLocalID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndUnitID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEndEventMsg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNGQTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemark;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStartLocalRecipeID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCFReworkQTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEnabled;
    }
}