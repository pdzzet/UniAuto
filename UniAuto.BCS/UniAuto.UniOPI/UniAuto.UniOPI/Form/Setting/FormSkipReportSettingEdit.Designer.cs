namespace UniOPI
{
    partial class FormSkipReportSettingEdit
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
            this.grbData = new System.Windows.Forms.GroupBox();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.lblNodeNo = new System.Windows.Forms.Label();
            this.cmbSkipAgent = new System.Windows.Forms.ComboBox();
            this.lblSvID = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.cmbSkipReportTrx = new System.Windows.Forms.ComboBox();
            this.cmbNode = new System.Windows.Forms.ComboBox();
            this.cmbUnit = new System.Windows.Forms.ComboBox();
            this.txtSkipCondition = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvAddList = new System.Windows.Forms.DataGridView();
            this.colLocalID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnitNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnitID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSkipAgent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSkipReportTrx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSkipCondition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.flpButton = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlAdd = new System.Windows.Forms.Panel();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.grbData.SuspendLayout();
            this.pnlEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddList)).BeginInit();
            this.tlpBase.SuspendLayout();
            this.flpButton.SuspendLayout();
            this.pnlAdd.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(674, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(734, 572);
            // 
            // grbData
            // 
            this.grbData.Controls.Add(this.pnlEdit);
            this.grbData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbData.Location = new System.Drawing.Point(3, 3);
            this.grbData.Name = "grbData";
            this.grbData.Size = new System.Drawing.Size(728, 214);
            this.grbData.TabIndex = 40;
            this.grbData.TabStop = false;
            // 
            // pnlEdit
            // 
            this.pnlEdit.Controls.Add(this.lblNodeNo);
            this.pnlEdit.Controls.Add(this.cmbSkipAgent);
            this.pnlEdit.Controls.Add(this.lblSvID);
            this.pnlEdit.Controls.Add(this.label2);
            this.pnlEdit.Controls.Add(this.lblDescription);
            this.pnlEdit.Controls.Add(this.cmbSkipReportTrx);
            this.pnlEdit.Controls.Add(this.cmbNode);
            this.pnlEdit.Controls.Add(this.cmbUnit);
            this.pnlEdit.Controls.Add(this.txtSkipCondition);
            this.pnlEdit.Controls.Add(this.label1);
            this.pnlEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEdit.Location = new System.Drawing.Point(3, 18);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(722, 187);
            this.pnlEdit.TabIndex = 78;
            // 
            // lblNodeNo
            // 
            this.lblNodeNo.BackColor = System.Drawing.Color.Black;
            this.lblNodeNo.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblNodeNo.ForeColor = System.Drawing.Color.White;
            this.lblNodeNo.Location = new System.Drawing.Point(12, 7);
            this.lblNodeNo.Name = "lblNodeNo";
            this.lblNodeNo.Size = new System.Drawing.Size(175, 25);
            this.lblNodeNo.TabIndex = 71;
            this.lblNodeNo.Text = "Local ID";
            this.lblNodeNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbSkipAgent
            // 
            this.cmbSkipAgent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSkipAgent.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbSkipAgent.FormattingEnabled = true;
            this.cmbSkipAgent.Location = new System.Drawing.Point(187, 75);
            this.cmbSkipAgent.Name = "cmbSkipAgent";
            this.cmbSkipAgent.Size = new System.Drawing.Size(188, 29);
            this.cmbSkipAgent.TabIndex = 77;
            // 
            // lblSvID
            // 
            this.lblSvID.BackColor = System.Drawing.Color.Black;
            this.lblSvID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblSvID.ForeColor = System.Drawing.Color.White;
            this.lblSvID.Location = new System.Drawing.Point(12, 42);
            this.lblSvID.Name = "lblSvID";
            this.lblSvID.Size = new System.Drawing.Size(175, 25);
            this.lblSvID.TabIndex = 40;
            this.lblSvID.Text = "Unit ID";
            this.lblSvID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Black;
            this.label2.Font = new System.Drawing.Font("Calibri", 13F);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(12, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(175, 25);
            this.label2.TabIndex = 76;
            this.label2.Text = "Skip Agent";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDescription
            // 
            this.lblDescription.BackColor = System.Drawing.Color.Black;
            this.lblDescription.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblDescription.ForeColor = System.Drawing.Color.White;
            this.lblDescription.Location = new System.Drawing.Point(12, 112);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(175, 25);
            this.lblDescription.TabIndex = 45;
            this.lblDescription.Text = "Skip Report Trx ";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbSkipReportTrx
            // 
            this.cmbSkipReportTrx.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSkipReportTrx.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbSkipReportTrx.FormattingEnabled = true;
            this.cmbSkipReportTrx.Location = new System.Drawing.Point(187, 110);
            this.cmbSkipReportTrx.Name = "cmbSkipReportTrx";
            this.cmbSkipReportTrx.Size = new System.Drawing.Size(188, 29);
            this.cmbSkipReportTrx.TabIndex = 75;
            // 
            // cmbNode
            // 
            this.cmbNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbNode.FormattingEnabled = true;
            this.cmbNode.Location = new System.Drawing.Point(187, 6);
            this.cmbNode.Name = "cmbNode";
            this.cmbNode.Size = new System.Drawing.Size(188, 29);
            this.cmbNode.TabIndex = 0;
            // 
            // cmbUnit
            // 
            this.cmbUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUnit.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbUnit.FormattingEnabled = true;
            this.cmbUnit.Location = new System.Drawing.Point(187, 41);
            this.cmbUnit.Name = "cmbUnit";
            this.cmbUnit.Size = new System.Drawing.Size(188, 29);
            this.cmbUnit.TabIndex = 74;
            // 
            // txtSkipCondition
            // 
            this.txtSkipCondition.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSkipCondition.Location = new System.Drawing.Point(187, 145);
            this.txtSkipCondition.Name = "txtSkipCondition";
            this.txtSkipCondition.Size = new System.Drawing.Size(495, 28);
            this.txtSkipCondition.TabIndex = 72;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Font = new System.Drawing.Font("Calibri", 13F);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 147);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 25);
            this.label1.TabIndex = 73;
            this.label1.Text = "Skip Report Condition";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.colLocalID,
            this.colLocalNo,
            this.colUnitNo,
            this.colUnitID,
            this.colSkipAgent,
            this.colSkipReportTrx,
            this.colSkipCondition});
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
            this.dgvAddList.Size = new System.Drawing.Size(723, 224);
            this.dgvAddList.TabIndex = 79;
            this.dgvAddList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAddList_CellClick);
            // 
            // colLocalID
            // 
            this.colLocalID.DataPropertyName = "NODEID";
            this.colLocalID.HeaderText = "Local ID";
            this.colLocalID.Name = "colLocalID";
            this.colLocalID.ReadOnly = true;
            // 
            // colLocalNo
            // 
            this.colLocalNo.HeaderText = "Local No";
            this.colLocalNo.Name = "colLocalNo";
            this.colLocalNo.ReadOnly = true;
            // 
            // colUnitNo
            // 
            this.colUnitNo.DataPropertyName = "UNITNO";
            this.colUnitNo.HeaderText = "Unit No";
            this.colUnitNo.Name = "colUnitNo";
            this.colUnitNo.ReadOnly = true;
            // 
            // colUnitID
            // 
            this.colUnitID.HeaderText = "Unit ID";
            this.colUnitID.Name = "colUnitID";
            this.colUnitID.ReadOnly = true;
            // 
            // colSkipAgent
            // 
            this.colSkipAgent.DataPropertyName = "SKIPAGENT";
            this.colSkipAgent.HeaderText = "Skip Agent";
            this.colSkipAgent.Name = "colSkipAgent";
            this.colSkipAgent.ReadOnly = true;
            this.colSkipAgent.Width = 110;
            // 
            // colSkipReportTrx
            // 
            this.colSkipReportTrx.DataPropertyName = "SKIPREPORTTRX";
            this.colSkipReportTrx.HeaderText = "Skip Report Trx";
            this.colSkipReportTrx.Name = "colSkipReportTrx";
            this.colSkipReportTrx.ReadOnly = true;
            this.colSkipReportTrx.Width = 180;
            // 
            // colSkipCondition
            // 
            this.colSkipCondition.DataPropertyName = "SKIPCONDITION";
            this.colSkipCondition.HeaderText = "Skip Condition";
            this.colSkipCondition.Name = "colSkipCondition";
            this.colSkipCondition.ReadOnly = true;
            this.colSkipCondition.Width = 150;
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.grbData, 0, 0);
            this.tlpBase.Controls.Add(this.flpButton, 0, 1);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Size = new System.Drawing.Size(734, 541);
            this.tlpBase.TabIndex = 2;
            // 
            // flpButton
            // 
            this.flpButton.Controls.Add(this.pnlAdd);
            this.flpButton.Controls.Add(this.dgvAddList);
            this.flpButton.Controls.Add(this.pnlButton);
            this.flpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButton.Location = new System.Drawing.Point(3, 223);
            this.flpButton.Name = "flpButton";
            this.flpButton.Size = new System.Drawing.Size(728, 315);
            this.flpButton.TabIndex = 28;
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnClear);
            this.pnlAdd.Controls.Add(this.btnAdd);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(723, 36);
            this.pnlAdd.TabIndex = 30;
            // 
            // btnClear
            // 
            this.btnClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClear.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(360, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(90, 30);
            this.btnClear.TabIndex = 4;
            this.btnClear.Tag = "Clear";
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAdd.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(265, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(90, 30);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Tag = "ADD";
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btn_Click);
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.button1);
            this.pnlButton.Controls.Add(this.btnOK);
            this.pnlButton.Location = new System.Drawing.Point(3, 275);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(723, 36);
            this.pnlButton.TabIndex = 27;
            // 
            // button1
            // 
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button1.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(360, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 30);
            this.button1.TabIndex = 5;
            this.button1.Tag = "Cancel";
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(265, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 30);
            this.btnOK.TabIndex = 4;
            this.btnOK.Tag = "OK";
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btn_Click);
            // 
            // FormSkipReportSettingEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(734, 572);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormSkipReportSettingEdit";
            this.Text = "   ";
            this.Load += new System.EventHandler(this.Form_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.grbData.ResumeLayout(false);
            this.pnlEdit.ResumeLayout(false);
            this.pnlEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddList)).EndInit();
            this.tlpBase.ResumeLayout(false);
            this.flpButton.ResumeLayout(false);
            this.pnlAdd.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grbData;
        private System.Windows.Forms.ComboBox cmbNode;
        private System.Windows.Forms.Label lblNodeNo;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblSvID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSkipCondition;
        private System.Windows.Forms.ComboBox cmbUnit;
        private System.Windows.Forms.ComboBox cmbSkipReportTrx;
        private System.Windows.Forms.ComboBox cmbSkipAgent;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel pnlEdit;
        private System.Windows.Forms.DataGridView dgvAddList;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.FlowLayoutPanel flpButton;
        private System.Windows.Forms.Panel pnlAdd;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnitID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSkipAgent;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSkipReportTrx;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSkipCondition;
        private System.Windows.Forms.Button btnClear;
    }
}