﻿namespace UniOPI
{
    partial class FormRobot_Route_Step_ByPass_Edit
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
            this.tlpRoutePocResultHandle = new System.Windows.Forms.TableLayoutPanel();
            this.pnlGridView = new System.Windows.Forms.Panel();
            this.flpBase = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlAdd = new System.Windows.Forms.Panel();
            this.btnNewClose = new System.Windows.Forms.Button();
            this.btnAddNew = new System.Windows.Forms.Button();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtRemark = new System.Windows.Forms.GroupBox();
            this.pnlNew = new System.Windows.Forms.Panel();
            this.explanation = new System.Windows.Forms.Label();
            this.cboGotoStepID = new System.Windows.Forms.ComboBox();
            this.lblGoToStepID = new System.Windows.Forms.Label();
            this.lblRemark = new System.Windows.Forms.Label();
            this.txtRemarks = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtRobotName = new System.Windows.Forms.TextBox();
            this.lblByPassItemSeq = new System.Windows.Forms.Label();
            this.lblByPassConditionID = new System.Windows.Forms.Label();
            this.txtByPassItemSeq = new System.Windows.Forms.TextBox();
            this.lblStepID = new System.Windows.Forms.Label();
            this.txtStepID = new System.Windows.Forms.TextBox();
            this.txtByPassConditionID = new System.Windows.Forms.TextBox();
            this.txtRouteID = new System.Windows.Forms.TextBox();
            this.lblRouteID = new System.Windows.Forms.Label();
            this.lblRobotName = new System.Windows.Forms.Label();
            this.chkIsEnable = new System.Windows.Forms.CheckBox();
            this.colSeverName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRouteID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStepID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colByPassConditionID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDesciption = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGotoStepID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMethodName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFuncKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colByPassItemSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.chkEnable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colLastUpdateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemarks = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpRoutePocResultHandle.SuspendLayout();
            this.pnlGridView.SuspendLayout();
            this.flpBase.SuspendLayout();
            this.pnlAdd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.pnlButton.SuspendLayout();
            this.txtRemark.SuspendLayout();
            this.pnlNew.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(783, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpRoutePocResultHandle);
            this.spcBase.Size = new System.Drawing.Size(843, 600);
            // 
            // tlpRoutePocResultHandle
            // 
            this.tlpRoutePocResultHandle.ColumnCount = 1;
            this.tlpRoutePocResultHandle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoutePocResultHandle.Controls.Add(this.pnlGridView, 0, 1);
            this.tlpRoutePocResultHandle.Controls.Add(this.txtRemark, 0, 0);
            this.tlpRoutePocResultHandle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoutePocResultHandle.Location = new System.Drawing.Point(0, 0);
            this.tlpRoutePocResultHandle.Name = "tlpRoutePocResultHandle";
            this.tlpRoutePocResultHandle.RowCount = 2;
            this.tlpRoutePocResultHandle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 302F));
            this.tlpRoutePocResultHandle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoutePocResultHandle.Size = new System.Drawing.Size(843, 569);
            this.tlpRoutePocResultHandle.TabIndex = 6;
            // 
            // pnlGridView
            // 
            this.pnlGridView.Controls.Add(this.flpBase);
            this.pnlGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridView.Location = new System.Drawing.Point(3, 305);
            this.pnlGridView.Name = "pnlGridView";
            this.pnlGridView.Size = new System.Drawing.Size(837, 261);
            this.pnlGridView.TabIndex = 1;
            // 
            // flpBase
            // 
            this.flpBase.Controls.Add(this.pnlAdd);
            this.flpBase.Controls.Add(this.dgvData);
            this.flpBase.Controls.Add(this.pnlButton);
            this.flpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpBase.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flpBase.Location = new System.Drawing.Point(0, 0);
            this.flpBase.Name = "flpBase";
            this.flpBase.Size = new System.Drawing.Size(837, 261);
            this.flpBase.TabIndex = 0;
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnNewClose);
            this.pnlAdd.Controls.Add(this.btnAddNew);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(832, 34);
            this.pnlAdd.TabIndex = 0;
            // 
            // btnNewClose
            // 
            this.btnNewClose.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNewClose.Location = new System.Drawing.Point(420, 2);
            this.btnNewClose.Name = "btnNewClose";
            this.btnNewClose.Size = new System.Drawing.Size(90, 30);
            this.btnNewClose.TabIndex = 10;
            this.btnNewClose.Tag = "Clear";
            this.btnNewClose.Text = "Clear";
            this.btnNewClose.UseVisualStyleBackColor = true;
            this.btnNewClose.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnAddNew
            // 
            this.btnAddNew.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddNew.Location = new System.Drawing.Point(322, 2);
            this.btnAddNew.Name = "btnAddNew";
            this.btnAddNew.Size = new System.Drawing.Size(90, 30);
            this.btnAddNew.TabIndex = 9;
            this.btnAddNew.Tag = "Add";
            this.btnAddNew.Text = "Add";
            this.btnAddNew.UseVisualStyleBackColor = true;
            this.btnAddNew.Click += new System.EventHandler(this.btn_Click);
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToOrderColumns = true;
            this.dgvData.AllowUserToResizeColumns = false;
            this.dgvData.AllowUserToResizeRows = false;
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSeverName,
            this.colRobotName,
            this.colRouteID,
            this.colStepID,
            this.colByPassConditionID,
            this.colDesciption,
            this.colGotoStepID,
            this.colObjectName,
            this.colMethodName,
            this.colFuncKey,
            this.colByPassItemSeq,
            this.chkEnable,
            this.colLastUpdateTime,
            this.colRemarks});
            this.dgvData.Location = new System.Drawing.Point(3, 43);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(832, 173);
            this.dgvData.TabIndex = 11;
            this.dgvData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvData_CellClick);
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnInsert);
            this.pnlButton.Controls.Add(this.btnCancel);
            this.pnlButton.Location = new System.Drawing.Point(3, 222);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(832, 34);
            this.pnlButton.TabIndex = 21;
            // 
            // btnInsert
            // 
            this.btnInsert.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInsert.Location = new System.Drawing.Point(323, 1);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(90, 30);
            this.btnInsert.TabIndex = 18;
            this.btnInsert.Tag = "OK";
            this.btnInsert.Text = "OK";
            this.btnInsert.UseVisualStyleBackColor = true;
            this.btnInsert.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(419, 1);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 30);
            this.btnCancel.TabIndex = 19;
            this.btnCancel.Tag = "Close";
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btn_Click);
            // 
            // txtRemark
            // 
            this.txtRemark.Controls.Add(this.pnlNew);
            this.txtRemark.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRemark.Location = new System.Drawing.Point(3, 3);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Size = new System.Drawing.Size(837, 296);
            this.txtRemark.TabIndex = 2;
            this.txtRemark.TabStop = false;
            // 
            // pnlNew
            // 
            this.pnlNew.Controls.Add(this.explanation);
            this.pnlNew.Controls.Add(this.cboGotoStepID);
            this.pnlNew.Controls.Add(this.lblGoToStepID);
            this.pnlNew.Controls.Add(this.lblRemark);
            this.pnlNew.Controls.Add(this.txtRemarks);
            this.pnlNew.Controls.Add(this.lblDescription);
            this.pnlNew.Controls.Add(this.txtDescription);
            this.pnlNew.Controls.Add(this.txtRobotName);
            this.pnlNew.Controls.Add(this.lblByPassItemSeq);
            this.pnlNew.Controls.Add(this.lblByPassConditionID);
            this.pnlNew.Controls.Add(this.txtByPassItemSeq);
            this.pnlNew.Controls.Add(this.lblStepID);
            this.pnlNew.Controls.Add(this.txtStepID);
            this.pnlNew.Controls.Add(this.txtByPassConditionID);
            this.pnlNew.Controls.Add(this.txtRouteID);
            this.pnlNew.Controls.Add(this.lblRouteID);
            this.pnlNew.Controls.Add(this.lblRobotName);
            this.pnlNew.Controls.Add(this.chkIsEnable);
            this.pnlNew.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNew.Location = new System.Drawing.Point(3, 18);
            this.pnlNew.Name = "pnlNew";
            this.pnlNew.Size = new System.Drawing.Size(831, 275);
            this.pnlNew.TabIndex = 1;
            // 
            // explanation
            // 
            this.explanation.AutoSize = true;
            this.explanation.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.explanation.ForeColor = System.Drawing.Color.Red;
            this.explanation.Location = new System.Drawing.Point(723, 80);
            this.explanation.Name = "explanation";
            this.explanation.Size = new System.Drawing.Size(91, 14);
            this.explanation.TabIndex = 38;
            this.explanation.Text = "数字越大越优先";
            // 
            // cboGotoStepID
            // 
            this.cboGotoStepID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGotoStepID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.cboGotoStepID.FormattingEnabled = true;
            this.cboGotoStepID.Location = new System.Drawing.Point(597, 9);
            this.cboGotoStepID.Name = "cboGotoStepID";
            this.cboGotoStepID.Size = new System.Drawing.Size(96, 29);
            this.cboGotoStepID.TabIndex = 36;
            this.cboGotoStepID.Tag = "Object Name";
            // 
            // lblGoToStepID
            // 
            this.lblGoToStepID.BackColor = System.Drawing.Color.Black;
            this.lblGoToStepID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblGoToStepID.ForeColor = System.Drawing.Color.White;
            this.lblGoToStepID.Location = new System.Drawing.Point(416, 11);
            this.lblGoToStepID.Name = "lblGoToStepID";
            this.lblGoToStepID.Size = new System.Drawing.Size(180, 25);
            this.lblGoToStepID.TabIndex = 37;
            this.lblGoToStepID.Text = "Go To Step ID";
            this.lblGoToStepID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRemark
            // 
            this.lblRemark.BackColor = System.Drawing.Color.Black;
            this.lblRemark.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRemark.ForeColor = System.Drawing.Color.White;
            this.lblRemark.Location = new System.Drawing.Point(9, 105);
            this.lblRemark.Name = "lblRemark";
            this.lblRemark.Size = new System.Drawing.Size(180, 25);
            this.lblRemark.TabIndex = 34;
            this.lblRemark.Text = "Remarks";
            this.lblRemark.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtRemarks
            // 
            this.txtRemarks.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtRemarks.Location = new System.Drawing.Point(189, 103);
            this.txtRemarks.Name = "txtRemarks";
            this.txtRemarks.Size = new System.Drawing.Size(637, 28);
            this.txtRemarks.TabIndex = 35;
            // 
            // lblDescription
            // 
            this.lblDescription.BackColor = System.Drawing.Color.Black;
            this.lblDescription.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblDescription.ForeColor = System.Drawing.Color.White;
            this.lblDescription.Location = new System.Drawing.Point(9, 200);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(180, 25);
            this.lblDescription.TabIndex = 33;
            this.lblDescription.Text = "Description";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtDescription.Location = new System.Drawing.Point(189, 199);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(637, 73);
            this.txtDescription.TabIndex = 32;
            // 
            // txtRobotName
            // 
            this.txtRobotName.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtRobotName.Location = new System.Drawing.Point(189, 10);
            this.txtRobotName.Name = "txtRobotName";
            this.txtRobotName.ReadOnly = true;
            this.txtRobotName.Size = new System.Drawing.Size(224, 28);
            this.txtRobotName.TabIndex = 31;
            // 
            // lblByPassItemSeq
            // 
            this.lblByPassItemSeq.BackColor = System.Drawing.Color.Black;
            this.lblByPassItemSeq.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblByPassItemSeq.ForeColor = System.Drawing.Color.White;
            this.lblByPassItemSeq.Location = new System.Drawing.Point(416, 73);
            this.lblByPassItemSeq.Name = "lblByPassItemSeq";
            this.lblByPassItemSeq.Size = new System.Drawing.Size(180, 25);
            this.lblByPassItemSeq.TabIndex = 29;
            this.lblByPassItemSeq.Text = "Item Seq";
            this.lblByPassItemSeq.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblByPassConditionID
            // 
            this.lblByPassConditionID.BackColor = System.Drawing.Color.Black;
            this.lblByPassConditionID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblByPassConditionID.ForeColor = System.Drawing.Color.White;
            this.lblByPassConditionID.Location = new System.Drawing.Point(416, 42);
            this.lblByPassConditionID.Name = "lblByPassConditionID";
            this.lblByPassConditionID.Size = new System.Drawing.Size(180, 25);
            this.lblByPassConditionID.TabIndex = 2;
            this.lblByPassConditionID.Text = "Condition ID";
            this.lblByPassConditionID.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtByPassItemSeq
            // 
            this.txtByPassItemSeq.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtByPassItemSeq.Location = new System.Drawing.Point(597, 72);
            this.txtByPassItemSeq.MaxLength = 5;
            this.txtByPassItemSeq.Name = "txtByPassItemSeq";
            this.txtByPassItemSeq.Size = new System.Drawing.Size(120, 28);
            this.txtByPassItemSeq.TabIndex = 30;
            this.txtByPassItemSeq.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtItemSeq_KeyPress);
            // 
            // lblStepID
            // 
            this.lblStepID.BackColor = System.Drawing.Color.Black;
            this.lblStepID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStepID.ForeColor = System.Drawing.Color.White;
            this.lblStepID.Location = new System.Drawing.Point(9, 73);
            this.lblStepID.Name = "lblStepID";
            this.lblStepID.Size = new System.Drawing.Size(180, 25);
            this.lblStepID.TabIndex = 27;
            this.lblStepID.Text = "Step ID";
            this.lblStepID.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtStepID
            // 
            this.txtStepID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtStepID.Location = new System.Drawing.Point(188, 72);
            this.txtStepID.Name = "txtStepID";
            this.txtStepID.ReadOnly = true;
            this.txtStepID.Size = new System.Drawing.Size(224, 28);
            this.txtStepID.TabIndex = 28;
            // 
            // txtByPassConditionID
            // 
            this.txtByPassConditionID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtByPassConditionID.Location = new System.Drawing.Point(596, 41);
            this.txtByPassConditionID.Name = "txtByPassConditionID";
            this.txtByPassConditionID.ReadOnly = true;
            this.txtByPassConditionID.Size = new System.Drawing.Size(229, 28);
            this.txtByPassConditionID.TabIndex = 26;
            // 
            // txtRouteID
            // 
            this.txtRouteID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtRouteID.Location = new System.Drawing.Point(189, 41);
            this.txtRouteID.Name = "txtRouteID";
            this.txtRouteID.ReadOnly = true;
            this.txtRouteID.Size = new System.Drawing.Size(224, 28);
            this.txtRouteID.TabIndex = 25;
            // 
            // lblRouteID
            // 
            this.lblRouteID.BackColor = System.Drawing.Color.Black;
            this.lblRouteID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRouteID.ForeColor = System.Drawing.Color.White;
            this.lblRouteID.Location = new System.Drawing.Point(9, 42);
            this.lblRouteID.Name = "lblRouteID";
            this.lblRouteID.Size = new System.Drawing.Size(180, 25);
            this.lblRouteID.TabIndex = 22;
            this.lblRouteID.Text = "Route ID";
            this.lblRouteID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRobotName
            // 
            this.lblRobotName.BackColor = System.Drawing.Color.Black;
            this.lblRobotName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRobotName.ForeColor = System.Drawing.Color.White;
            this.lblRobotName.Location = new System.Drawing.Point(9, 11);
            this.lblRobotName.Name = "lblRobotName";
            this.lblRobotName.Size = new System.Drawing.Size(180, 25);
            this.lblRobotName.TabIndex = 20;
            this.lblRobotName.Text = "Robot Name";
            this.lblRobotName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkIsEnable
            // 
            this.chkIsEnable.Checked = true;
            this.chkIsEnable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIsEnable.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkIsEnable.Location = new System.Drawing.Point(699, 11);
            this.chkIsEnable.Name = "chkIsEnable";
            this.chkIsEnable.Size = new System.Drawing.Size(112, 25);
            this.chkIsEnable.TabIndex = 10;
            this.chkIsEnable.Text = "Is Enable ?";
            this.chkIsEnable.UseVisualStyleBackColor = true;
            // 
            // colSeverName
            // 
            this.colSeverName.DataPropertyName = "SERVERNAME";
            this.colSeverName.HeaderText = "ServerName";
            this.colSeverName.Name = "colSeverName";
            this.colSeverName.ReadOnly = true;
            this.colSeverName.Visible = false;
            this.colSeverName.Width = 101;
            // 
            // colRobotName
            // 
            this.colRobotName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colRobotName.DataPropertyName = "ROBOTNAME";
            this.colRobotName.HeaderText = "Robot Name";
            this.colRobotName.Name = "colRobotName";
            this.colRobotName.ReadOnly = true;
            this.colRobotName.Width = 130;
            // 
            // colRouteID
            // 
            this.colRouteID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colRouteID.DataPropertyName = "ROUTEID";
            this.colRouteID.HeaderText = "Route ID";
            this.colRouteID.Name = "colRouteID";
            this.colRouteID.ReadOnly = true;
            this.colRouteID.Width = 120;
            // 
            // colStepID
            // 
            this.colStepID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colStepID.DataPropertyName = "STEPID";
            this.colStepID.HeaderText = "Step ID";
            this.colStepID.Name = "colStepID";
            this.colStepID.ReadOnly = true;
            this.colStepID.Width = 95;
            // 
            // colByPassConditionID
            // 
            this.colByPassConditionID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colByPassConditionID.DataPropertyName = "BYPASSCONDITIONID";
            this.colByPassConditionID.HeaderText = "Condition ID";
            this.colByPassConditionID.Name = "colByPassConditionID";
            this.colByPassConditionID.ReadOnly = true;
            this.colByPassConditionID.Width = 200;
            // 
            // colDesciption
            // 
            this.colDesciption.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDesciption.DataPropertyName = "Description";
            this.colDesciption.HeaderText = "Description";
            this.colDesciption.Name = "colDesciption";
            this.colDesciption.ReadOnly = true;
            this.colDesciption.Width = 250;
            // 
            // colGotoStepID
            // 
            this.colGotoStepID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colGotoStepID.DataPropertyName = "GOTOSTEPID";
            this.colGotoStepID.HeaderText = "Goto Step ID";
            this.colGotoStepID.Name = "colGotoStepID";
            this.colGotoStepID.ReadOnly = true;
            this.colGotoStepID.Width = 130;
            // 
            // colObjectName
            // 
            this.colObjectName.DataPropertyName = "OBJECTNAME";
            this.colObjectName.HeaderText = "ObjectName";
            this.colObjectName.Name = "colObjectName";
            this.colObjectName.ReadOnly = true;
            this.colObjectName.Width = 121;
            // 
            // colMethodName
            // 
            this.colMethodName.DataPropertyName = "METHODNAME";
            this.colMethodName.HeaderText = "MethodName";
            this.colMethodName.Name = "colMethodName";
            this.colMethodName.ReadOnly = true;
            this.colMethodName.Width = 132;
            // 
            // colFuncKey
            // 
            this.colFuncKey.HeaderText = "Function Key";
            this.colFuncKey.Name = "colFuncKey";
            this.colFuncKey.ReadOnly = true;
            this.colFuncKey.Width = 124;
            // 
            // colByPassItemSeq
            // 
            this.colByPassItemSeq.DataPropertyName = "BYPASSITEMSEQ";
            this.colByPassItemSeq.HeaderText = "Item Seq";
            this.colByPassItemSeq.Name = "colByPassItemSeq";
            this.colByPassItemSeq.ReadOnly = true;
            this.colByPassItemSeq.Width = 96;
            // 
            // chkEnable
            // 
            this.chkEnable.DataPropertyName = "ISENABLE";
            this.chkEnable.FalseValue = "N";
            this.chkEnable.HeaderText = "Enable";
            this.chkEnable.Name = "chkEnable";
            this.chkEnable.ReadOnly = true;
            this.chkEnable.TrueValue = "Y";
            this.chkEnable.Width = 62;
            // 
            // colLastUpdateTime
            // 
            this.colLastUpdateTime.DataPropertyName = "LASTUPDATETIME";
            this.colLastUpdateTime.HeaderText = "LastUpdateTime";
            this.colLastUpdateTime.Name = "colLastUpdateTime";
            this.colLastUpdateTime.ReadOnly = true;
            this.colLastUpdateTime.Visible = false;
            this.colLastUpdateTime.Width = 148;
            // 
            // colRemarks
            // 
            this.colRemarks.HeaderText = "Remarks";
            this.colRemarks.Name = "colRemarks";
            this.colRemarks.ReadOnly = true;
            this.colRemarks.Width = 95;
            // 
            // FormRobot_Route_Step_ByPass_Edit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 600);
            this.Name = "FormRobot_Route_Step_ByPass_Edit";
            this.Text = " ";
            this.Load += new System.EventHandler(this.FormRobot_Route_Step_ByPass_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpRoutePocResultHandle.ResumeLayout(false);
            this.pnlGridView.ResumeLayout(false);
            this.flpBase.ResumeLayout(false);
            this.pnlAdd.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.pnlButton.ResumeLayout(false);
            this.txtRemark.ResumeLayout(false);
            this.pnlNew.ResumeLayout(false);
            this.pnlNew.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpRoutePocResultHandle;
        private System.Windows.Forms.Panel pnlGridView;
        private System.Windows.Forms.FlowLayoutPanel flpBase;
        private System.Windows.Forms.Panel pnlAdd;
        private System.Windows.Forms.Button btnNewClose;
        private System.Windows.Forms.Button btnAddNew;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox txtRemark;
        private System.Windows.Forms.Panel pnlNew;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtRobotName;
        private System.Windows.Forms.Label lblByPassItemSeq;
        private System.Windows.Forms.Label lblByPassConditionID;
        private System.Windows.Forms.TextBox txtByPassItemSeq;
        private System.Windows.Forms.Label lblStepID;
        private System.Windows.Forms.TextBox txtStepID;
        private System.Windows.Forms.TextBox txtByPassConditionID;
        private System.Windows.Forms.TextBox txtRouteID;
        private System.Windows.Forms.Label lblRouteID;
        private System.Windows.Forms.Label lblRobotName;
        private System.Windows.Forms.CheckBox chkIsEnable;
        private System.Windows.Forms.ComboBox cboGotoStepID;
        private System.Windows.Forms.Label lblGoToStepID;
        private System.Windows.Forms.Label lblRemark;
        private System.Windows.Forms.TextBox txtRemarks;
        private System.Windows.Forms.Label explanation;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSeverName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRouteID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStepID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colByPassConditionID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDesciption;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGotoStepID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMethodName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFuncKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colByPassItemSeq;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chkEnable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastUpdateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemarks;
    }
}