namespace UniOPI
{
    partial class FormLineStatusRuleEdit
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvAddList = new System.Windows.Forms.DataGridView();
            this.colLineType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colConditionStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colConditionSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalList = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.lblEqpNoList = new System.Windows.Forms.Label();
            this.txtEqpList = new System.Windows.Forms.TextBox();
            this.chklstEqpNo = new System.Windows.Forms.CheckedListBox();
            this.lblConditionSeqNo = new System.Windows.Forms.Label();
            this.cmbConditionStatus = new System.Windows.Forms.ComboBox();
            this.txtConditionSeqNo = new System.Windows.Forms.TextBox();
            this.lblConditionStatus = new System.Windows.Forms.Label();
            this.btnAddOK = new System.Windows.Forms.Button();
            this.btnCancelClose = new System.Windows.Forms.Button();
            this.btnOKClose = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.flpButton = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlAdd = new System.Windows.Forms.Panel();
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
            this.lblCaption.Size = new System.Drawing.Size(661, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(721, 672);
            // 
            // dgvAddList
            // 
            this.dgvAddList.AllowUserToAddRows = false;
            this.dgvAddList.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAddList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvAddList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAddList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLineType,
            this.colConditionStatus,
            this.colConditionSeqNo,
            this.colLocalList});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvAddList.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvAddList.Location = new System.Drawing.Point(3, 45);
            this.dgvAddList.Name = "dgvAddList";
            this.dgvAddList.ReadOnly = true;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvAddList.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvAddList.RowTemplate.Height = 24;
            this.dgvAddList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAddList.Size = new System.Drawing.Size(708, 206);
            this.dgvAddList.TabIndex = 75;
            this.dgvAddList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAddList_CellClick);
            // 
            // colLineType
            // 
            this.colLineType.DataPropertyName = "LINETYPE";
            this.colLineType.HeaderText = "Line Type";
            this.colLineType.Name = "colLineType";
            this.colLineType.ReadOnly = true;
            this.colLineType.Width = 120;
            // 
            // colConditionStatus
            // 
            this.colConditionStatus.DataPropertyName = "CONDITIONSTATUS";
            this.colConditionStatus.HeaderText = "Condition Status";
            this.colConditionStatus.Name = "colConditionStatus";
            this.colConditionStatus.ReadOnly = true;
            this.colConditionStatus.Width = 140;
            // 
            // colConditionSeqNo
            // 
            this.colConditionSeqNo.DataPropertyName = "CONDITIONSEQNO";
            this.colConditionSeqNo.HeaderText = "Condition SeqNo";
            this.colConditionSeqNo.Name = "colConditionSeqNo";
            this.colConditionSeqNo.ReadOnly = true;
            this.colConditionSeqNo.Width = 140;
            // 
            // colLocalList
            // 
            this.colLocalList.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colLocalList.DataPropertyName = "EQPNOLIST";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.colLocalList.DefaultCellStyle = dataGridViewCellStyle2;
            this.colLocalList.HeaderText = "Local List";
            this.colLocalList.Name = "colLocalList";
            this.colLocalList.ReadOnly = true;
            // 
            // pnlEdit
            // 
            this.pnlEdit.Controls.Add(this.lblEqpNoList);
            this.pnlEdit.Controls.Add(this.txtEqpList);
            this.pnlEdit.Controls.Add(this.chklstEqpNo);
            this.pnlEdit.Controls.Add(this.lblConditionSeqNo);
            this.pnlEdit.Controls.Add(this.cmbConditionStatus);
            this.pnlEdit.Controls.Add(this.txtConditionSeqNo);
            this.pnlEdit.Controls.Add(this.lblConditionStatus);
            this.pnlEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEdit.Location = new System.Drawing.Point(3, 18);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(709, 310);
            this.pnlEdit.TabIndex = 74;
            // 
            // lblEqpNoList
            // 
            this.lblEqpNoList.BackColor = System.Drawing.Color.Black;
            this.lblEqpNoList.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblEqpNoList.ForeColor = System.Drawing.Color.White;
            this.lblEqpNoList.Location = new System.Drawing.Point(5, 83);
            this.lblEqpNoList.Name = "lblEqpNoList";
            this.lblEqpNoList.Size = new System.Drawing.Size(155, 25);
            this.lblEqpNoList.TabIndex = 45;
            this.lblEqpNoList.Text = "Local List";
            this.lblEqpNoList.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtEqpList
            // 
            this.txtEqpList.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEqpList.Location = new System.Drawing.Point(161, 82);
            this.txtEqpList.Name = "txtEqpList";
            this.txtEqpList.ReadOnly = true;
            this.txtEqpList.Size = new System.Drawing.Size(501, 28);
            this.txtEqpList.TabIndex = 73;
            // 
            // chklstEqpNo
            // 
            this.chklstEqpNo.CheckOnClick = true;
            this.chklstEqpNo.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chklstEqpNo.FormattingEnabled = true;
            this.chklstEqpNo.Location = new System.Drawing.Point(162, 118);
            this.chklstEqpNo.MultiColumn = true;
            this.chklstEqpNo.Name = "chklstEqpNo";
            this.chklstEqpNo.Size = new System.Drawing.Size(500, 180);
            this.chklstEqpNo.TabIndex = 72;
            this.chklstEqpNo.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chklstEqpNo_ItemCheck);
            // 
            // lblConditionSeqNo
            // 
            this.lblConditionSeqNo.BackColor = System.Drawing.Color.Black;
            this.lblConditionSeqNo.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblConditionSeqNo.ForeColor = System.Drawing.Color.White;
            this.lblConditionSeqNo.Location = new System.Drawing.Point(5, 50);
            this.lblConditionSeqNo.Name = "lblConditionSeqNo";
            this.lblConditionSeqNo.Size = new System.Drawing.Size(155, 25);
            this.lblConditionSeqNo.TabIndex = 60;
            this.lblConditionSeqNo.Text = "Condition SeqNo";
            this.lblConditionSeqNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbConditionStatus
            // 
            this.cmbConditionStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbConditionStatus.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbConditionStatus.FormattingEnabled = true;
            this.cmbConditionStatus.Location = new System.Drawing.Point(162, 16);
            this.cmbConditionStatus.Name = "cmbConditionStatus";
            this.cmbConditionStatus.Size = new System.Drawing.Size(187, 29);
            this.cmbConditionStatus.TabIndex = 1;
            // 
            // txtConditionSeqNo
            // 
            this.txtConditionSeqNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConditionSeqNo.Location = new System.Drawing.Point(161, 49);
            this.txtConditionSeqNo.Name = "txtConditionSeqNo";
            this.txtConditionSeqNo.Size = new System.Drawing.Size(188, 28);
            this.txtConditionSeqNo.TabIndex = 3;
            this.txtConditionSeqNo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtConditionSeqNo_KeyPress);
            // 
            // lblConditionStatus
            // 
            this.lblConditionStatus.BackColor = System.Drawing.Color.Black;
            this.lblConditionStatus.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblConditionStatus.ForeColor = System.Drawing.Color.White;
            this.lblConditionStatus.Location = new System.Drawing.Point(5, 18);
            this.lblConditionStatus.Name = "lblConditionStatus";
            this.lblConditionStatus.Size = new System.Drawing.Size(155, 25);
            this.lblConditionStatus.TabIndex = 71;
            this.lblConditionStatus.Text = "Condition Status";
            this.lblConditionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnAddOK
            // 
            this.btnAddOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddOK.Location = new System.Drawing.Point(263, 5);
            this.btnAddOK.Name = "btnAddOK";
            this.btnAddOK.Size = new System.Drawing.Size(90, 30);
            this.btnAddOK.TabIndex = 0;
            this.btnAddOK.Tag = "ADD";
            this.btnAddOK.Text = "Add";
            this.btnAddOK.UseVisualStyleBackColor = true;
            this.btnAddOK.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnCancelClose
            // 
            this.btnCancelClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancelClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelClose.Location = new System.Drawing.Point(359, 3);
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
            this.btnOKClose.Location = new System.Drawing.Point(263, 3);
            this.btnOKClose.Name = "btnOKClose";
            this.btnOKClose.Size = new System.Drawing.Size(90, 30);
            this.btnOKClose.TabIndex = 1;
            this.btnOKClose.Tag = "OK";
            this.btnOKClose.Text = "OK";
            this.btnOKClose.UseVisualStyleBackColor = true;
            this.btnOKClose.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnClear
            // 
            this.btnClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClear.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(359, 5);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(90, 30);
            this.btnClear.TabIndex = 74;
            this.btnClear.Tag = "Clear";
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btn_Click);
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
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 337F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Size = new System.Drawing.Size(721, 641);
            this.tlpBase.TabIndex = 7;
            // 
            // flpButton
            // 
            this.flpButton.Controls.Add(this.pnlAdd);
            this.flpButton.Controls.Add(this.dgvAddList);
            this.flpButton.Controls.Add(this.pnlButton);
            this.flpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButton.Location = new System.Drawing.Point(3, 340);
            this.flpButton.Name = "flpButton";
            this.flpButton.Size = new System.Drawing.Size(715, 298);
            this.flpButton.TabIndex = 0;
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnClear);
            this.pnlAdd.Controls.Add(this.btnAddOK);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(708, 36);
            this.pnlAdd.TabIndex = 0;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnCancelClose);
            this.pnlButton.Controls.Add(this.btnOKClose);
            this.pnlButton.Location = new System.Drawing.Point(3, 257);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(708, 36);
            this.pnlButton.TabIndex = 77;
            // 
            // grbData
            // 
            this.grbData.Controls.Add(this.pnlEdit);
            this.grbData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbData.Location = new System.Drawing.Point(3, 3);
            this.grbData.Name = "grbData";
            this.grbData.Size = new System.Drawing.Size(715, 331);
            this.grbData.TabIndex = 40;
            this.grbData.TabStop = false;
            // 
            // FormLineStatusRuleEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(721, 672);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormLineStatusRuleEdit";
            this.Text = "   ";
            this.Load += new System.EventHandler(this.FormLineStatusRuleEdit_Load);
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

        private System.Windows.Forms.TextBox txtConditionSeqNo;
        private System.Windows.Forms.Label lblConditionSeqNo;
        private System.Windows.Forms.Label lblEqpNoList;
        private System.Windows.Forms.Button btnOKClose;
        private System.Windows.Forms.Button btnAddOK;
        private System.Windows.Forms.ComboBox cmbConditionStatus;
        private System.Windows.Forms.Label lblConditionStatus;
        private System.Windows.Forms.CheckedListBox chklstEqpNo;
        private System.Windows.Forms.TextBox txtEqpList;
        private System.Windows.Forms.DataGridView dgvAddList;
        private System.Windows.Forms.Panel pnlEdit;
        private System.Windows.Forms.Button btnCancelClose;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.FlowLayoutPanel flpButton;
        private System.Windows.Forms.Panel pnlAdd;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.GroupBox grbData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colConditionStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colConditionSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalList;
    }
}