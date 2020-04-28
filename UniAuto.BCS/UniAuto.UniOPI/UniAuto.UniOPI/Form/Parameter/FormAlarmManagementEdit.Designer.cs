namespace UniOPI
{
    partial class FormAlarmManagementEdit
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grbData = new System.Windows.Forms.GroupBox();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.txtAlarmText = new System.Windows.Forms.TextBox();
            this.lblAlarmCode = new System.Windows.Forms.Label();
            this.txtAlarmID = new System.Windows.Forms.TextBox();
            this.cmbUnit = new System.Windows.Forms.ComboBox();
            this.lblNodeNo = new System.Windows.Forms.Label();
            this.lblUnit = new System.Windows.Forms.Label();
            this.cmbNode = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblAlarmLevel = new System.Windows.Forms.Label();
            this.cmbAlarmLevel = new System.Windows.Forms.ComboBox();
            this.lblAlarmID = new System.Windows.Forms.Label();
            this.dgvAddList = new System.Windows.Forms.DataGridView();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnitNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAlarmLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAlarmID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAlarmText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAddOK = new System.Windows.Forms.Button();
            this.btnOKClose = new System.Windows.Forms.Button();
            this.btnCancelClose = new System.Windows.Forms.Button();
            this.flpButton = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlAdd = new System.Windows.Forms.Panel();
            this.btnClear = new System.Windows.Forms.Button();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.grbData.SuspendLayout();
            this.pnlEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddList)).BeginInit();
            this.flpButton.SuspendLayout();
            this.pnlAdd.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(785, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(845, 572);
            // 
            // grbData
            // 
            this.grbData.Controls.Add(this.pnlEdit);
            this.grbData.Location = new System.Drawing.Point(3, 3);
            this.grbData.Name = "grbData";
            this.grbData.Size = new System.Drawing.Size(839, 169);
            this.grbData.TabIndex = 40;
            this.grbData.TabStop = false;
            // 
            // pnlEdit
            // 
            this.pnlEdit.Controls.Add(this.txtAlarmText);
            this.pnlEdit.Controls.Add(this.lblAlarmCode);
            this.pnlEdit.Controls.Add(this.txtAlarmID);
            this.pnlEdit.Controls.Add(this.cmbUnit);
            this.pnlEdit.Controls.Add(this.lblNodeNo);
            this.pnlEdit.Controls.Add(this.lblUnit);
            this.pnlEdit.Controls.Add(this.cmbNode);
            this.pnlEdit.Controls.Add(this.label3);
            this.pnlEdit.Controls.Add(this.lblAlarmLevel);
            this.pnlEdit.Controls.Add(this.cmbAlarmLevel);
            this.pnlEdit.Controls.Add(this.lblAlarmID);
            this.pnlEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEdit.Location = new System.Drawing.Point(3, 18);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(833, 182);
            this.pnlEdit.TabIndex = 72;
            // 
            // txtAlarmText
            // 
            this.txtAlarmText.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAlarmText.Location = new System.Drawing.Point(183, 115);
            this.txtAlarmText.Name = "txtAlarmText";
            this.txtAlarmText.Size = new System.Drawing.Size(640, 28);
            this.txtAlarmText.TabIndex = 4;
            // 
            // lblAlarmCode
            // 
            this.lblAlarmCode.BackColor = System.Drawing.Color.Black;
            this.lblAlarmCode.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblAlarmCode.ForeColor = System.Drawing.Color.White;
            this.lblAlarmCode.Location = new System.Drawing.Point(3, 116);
            this.lblAlarmCode.Name = "lblAlarmCode";
            this.lblAlarmCode.Size = new System.Drawing.Size(180, 25);
            this.lblAlarmCode.TabIndex = 61;
            this.lblAlarmCode.Text = "Alarm Text";
            this.lblAlarmCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtAlarmID
            // 
            this.txtAlarmID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAlarmID.Location = new System.Drawing.Point(183, 82);
            this.txtAlarmID.MaxLength = 5;
            this.txtAlarmID.Name = "txtAlarmID";
            this.txtAlarmID.Size = new System.Drawing.Size(227, 28);
            this.txtAlarmID.TabIndex = 3;
            this.txtAlarmID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNumber_KeyPress);
            // 
            // cmbUnit
            // 
            this.cmbUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUnit.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbUnit.FormattingEnabled = true;
            this.cmbUnit.Location = new System.Drawing.Point(596, 16);
            this.cmbUnit.Name = "cmbUnit";
            this.cmbUnit.Size = new System.Drawing.Size(227, 29);
            this.cmbUnit.TabIndex = 1;
            // 
            // lblNodeNo
            // 
            this.lblNodeNo.BackColor = System.Drawing.Color.Black;
            this.lblNodeNo.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblNodeNo.ForeColor = System.Drawing.Color.White;
            this.lblNodeNo.Location = new System.Drawing.Point(3, 17);
            this.lblNodeNo.Name = "lblNodeNo";
            this.lblNodeNo.Size = new System.Drawing.Size(180, 25);
            this.lblNodeNo.TabIndex = 40;
            this.lblNodeNo.Text = "Local ID";
            this.lblNodeNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUnit
            // 
            this.lblUnit.BackColor = System.Drawing.Color.Black;
            this.lblUnit.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblUnit.ForeColor = System.Drawing.Color.White;
            this.lblUnit.Location = new System.Drawing.Point(416, 17);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(180, 25);
            this.lblUnit.TabIndex = 71;
            this.lblUnit.Text = "Unit ID";
            this.lblUnit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbNode
            // 
            this.cmbNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbNode.FormattingEnabled = true;
            this.cmbNode.Location = new System.Drawing.Point(183, 16);
            this.cmbNode.Name = "cmbNode";
            this.cmbNode.Size = new System.Drawing.Size(227, 29);
            this.cmbNode.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(418, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 48;
            this.label3.Text = "1~65535";
            // 
            // lblAlarmLevel
            // 
            this.lblAlarmLevel.BackColor = System.Drawing.Color.Black;
            this.lblAlarmLevel.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblAlarmLevel.ForeColor = System.Drawing.Color.White;
            this.lblAlarmLevel.Location = new System.Drawing.Point(3, 49);
            this.lblAlarmLevel.Name = "lblAlarmLevel";
            this.lblAlarmLevel.Size = new System.Drawing.Size(180, 25);
            this.lblAlarmLevel.TabIndex = 50;
            this.lblAlarmLevel.Text = "Alarm Level";
            this.lblAlarmLevel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbAlarmLevel
            // 
            this.cmbAlarmLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAlarmLevel.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbAlarmLevel.FormattingEnabled = true;
            this.cmbAlarmLevel.Location = new System.Drawing.Point(183, 48);
            this.cmbAlarmLevel.Name = "cmbAlarmLevel";
            this.cmbAlarmLevel.Size = new System.Drawing.Size(227, 29);
            this.cmbAlarmLevel.TabIndex = 2;
            // 
            // lblAlarmID
            // 
            this.lblAlarmID.BackColor = System.Drawing.Color.Black;
            this.lblAlarmID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblAlarmID.ForeColor = System.Drawing.Color.White;
            this.lblAlarmID.Location = new System.Drawing.Point(3, 83);
            this.lblAlarmID.Name = "lblAlarmID";
            this.lblAlarmID.Size = new System.Drawing.Size(180, 25);
            this.lblAlarmID.TabIndex = 60;
            this.lblAlarmID.Text = "Alarm ID";
            this.lblAlarmID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.colLocalNo,
            this.colUnitNo,
            this.colAlarmLevel,
            this.colAlarmID,
            this.colAlarmText});
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
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAddList.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvAddList.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvAddList.RowTemplate.Height = 24;
            this.dgvAddList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAddList.Size = new System.Drawing.Size(836, 267);
            this.dgvAddList.TabIndex = 76;
            this.dgvAddList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAddList_CellClick);
            // 
            // colLocalNo
            // 
            this.colLocalNo.DataPropertyName = "NODENO";
            this.colLocalNo.HeaderText = "Local No";
            this.colLocalNo.Name = "colLocalNo";
            this.colLocalNo.ReadOnly = true;
            this.colLocalNo.Width = 110;
            // 
            // colUnitNo
            // 
            this.colUnitNo.DataPropertyName = "UNITNO";
            this.colUnitNo.HeaderText = "Unit No";
            this.colUnitNo.Name = "colUnitNo";
            this.colUnitNo.ReadOnly = true;
            // 
            // colAlarmLevel
            // 
            this.colAlarmLevel.DataPropertyName = "ALARMLEVEL";
            this.colAlarmLevel.HeaderText = "Alarm Level";
            this.colAlarmLevel.Name = "colAlarmLevel";
            this.colAlarmLevel.ReadOnly = true;
            this.colAlarmLevel.Width = 150;
            // 
            // colAlarmID
            // 
            this.colAlarmID.DataPropertyName = "ALARMID";
            this.colAlarmID.HeaderText = "Alarm ID";
            this.colAlarmID.Name = "colAlarmID";
            this.colAlarmID.ReadOnly = true;
            // 
            // colAlarmText
            // 
            this.colAlarmText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colAlarmText.DataPropertyName = "ALARMTEXT";
            this.colAlarmText.HeaderText = "Alarm Text";
            this.colAlarmText.Name = "colAlarmText";
            this.colAlarmText.ReadOnly = true;
            // 
            // btnAddOK
            // 
            this.btnAddOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddOK.Location = new System.Drawing.Point(320, 2);
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
            this.btnOKClose.Location = new System.Drawing.Point(320, 3);
            this.btnOKClose.Name = "btnOKClose";
            this.btnOKClose.Size = new System.Drawing.Size(90, 30);
            this.btnOKClose.TabIndex = 72;
            this.btnOKClose.Tag = "OK";
            this.btnOKClose.Text = "OK";
            this.btnOKClose.UseVisualStyleBackColor = true;
            this.btnOKClose.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnCancelClose
            // 
            this.btnCancelClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancelClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelClose.Location = new System.Drawing.Point(420, 3);
            this.btnCancelClose.Name = "btnCancelClose";
            this.btnCancelClose.Size = new System.Drawing.Size(90, 30);
            this.btnCancelClose.TabIndex = 1;
            this.btnCancelClose.Tag = "Cancel";
            this.btnCancelClose.Text = "Cancel";
            this.btnCancelClose.UseVisualStyleBackColor = true;
            this.btnCancelClose.Click += new System.EventHandler(this.btn_Click);
            // 
            // flpButton
            // 
            this.flpButton.Controls.Add(this.pnlAdd);
            this.flpButton.Controls.Add(this.dgvAddList);
            this.flpButton.Controls.Add(this.pnlButton);
            this.flpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButton.Location = new System.Drawing.Point(3, 178);
            this.flpButton.Name = "flpButton";
            this.flpButton.Size = new System.Drawing.Size(839, 360);
            this.flpButton.TabIndex = 0;
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnClear);
            this.pnlAdd.Controls.Add(this.btnAddOK);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(836, 36);
            this.pnlAdd.TabIndex = 0;
            // 
            // btnClear
            // 
            this.btnClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClear.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(420, 2);
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
            this.pnlButton.Location = new System.Drawing.Point(3, 318);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(836, 36);
            this.pnlButton.TabIndex = 77;
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
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 175F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Size = new System.Drawing.Size(845, 541);
            this.tlpBase.TabIndex = 3;
            // 
            // FormAlarmManagementEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(845, 572);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormAlarmManagementEdit";
            this.Text = " ";
            this.Load += new System.EventHandler(this.FormAlarmManagementEdit_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.grbData.ResumeLayout(false);
            this.pnlEdit.ResumeLayout(false);
            this.pnlEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddList)).EndInit();
            this.flpButton.ResumeLayout(false);
            this.pnlAdd.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grbData;
        private System.Windows.Forms.Label lblAlarmCode;
        private System.Windows.Forms.TextBox txtAlarmID;
        private System.Windows.Forms.Label lblAlarmID;
        private System.Windows.Forms.ComboBox cmbAlarmLevel;
        private System.Windows.Forms.Label lblAlarmLevel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtAlarmText;
        private System.Windows.Forms.Label lblNodeNo;
        private System.Windows.Forms.Button btnCancelClose;
        private System.Windows.Forms.Button btnAddOK;
        private System.Windows.Forms.ComboBox cmbUnit;
        private System.Windows.Forms.Label lblUnit;
        private System.Windows.Forms.ComboBox cmbNode;
        private System.Windows.Forms.Panel pnlEdit;
        private System.Windows.Forms.Button btnOKClose;
        private System.Windows.Forms.DataGridView dgvAddList;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlarmLevel;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlarmID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlarmText;
        private System.Windows.Forms.FlowLayoutPanel flpButton;
        private System.Windows.Forms.Panel pnlAdd;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Button btnClear;
    }
}