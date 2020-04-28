namespace UniOPI
{
    partial class FormDefectCodeReport
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
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.pnlCombox = new System.Windows.Forms.Panel();
            this.btnQuery = new System.Windows.Forms.Button();
            this.lblNode = new System.Windows.Forms.Label();
            this.cboNode = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colLocalNoID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCSTSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJobSeqNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colChipPosition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnitNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnitID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDefectCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.pnlCombox.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1205, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(1265, 709);
            // 
            // tmrBaseRefresh
            // 
            this.tmrBaseRefresh.Tick += new System.EventHandler(this.tmrBaseRefresh_Tick);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Controls.Add(this.pnlCombox, 0, 0);
            this.tlpBase.Controls.Add(this.groupBox2, 0, 1);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Size = new System.Drawing.Size(1265, 678);
            this.tlpBase.TabIndex = 4;
            // 
            // pnlCombox
            // 
            this.pnlCombox.Controls.Add(this.btnQuery);
            this.pnlCombox.Controls.Add(this.lblNode);
            this.pnlCombox.Controls.Add(this.cboNode);
            this.pnlCombox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCombox.Location = new System.Drawing.Point(3, 3);
            this.pnlCombox.Name = "pnlCombox";
            this.pnlCombox.Size = new System.Drawing.Size(1259, 59);
            this.pnlCombox.TabIndex = 25;
            // 
            // btnQuery
            // 
            this.btnQuery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.Location = new System.Drawing.Point(327, 15);
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
            // cboNode
            // 
            this.cboNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboNode.FormattingEnabled = true;
            this.cboNode.Location = new System.Drawing.Point(62, 16);
            this.cboNode.Name = "cboNode";
            this.cboNode.Size = new System.Drawing.Size(240, 29);
            this.cboNode.TabIndex = 4;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dgvData);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(3, 68);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1259, 607);
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
            this.colLocalNoID,
            this.colCSTSeqNo,
            this.colJobSeqNo,
            this.colChipPosition,
            this.colUnitNo,
            this.colUnitID,
            this.colDefectCode});
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
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1253, 581);
            this.dgvData.TabIndex = 12;
            // 
            // colLocalNoID
            // 
            this.colLocalNoID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colLocalNoID.HeaderText = "Local ";
            this.colLocalNoID.Name = "colLocalNoID";
            this.colLocalNoID.ReadOnly = true;
            this.colLocalNoID.Width = 120;
            // 
            // colCSTSeqNo
            // 
            this.colCSTSeqNo.HeaderText = "CST Seq No";
            this.colCSTSeqNo.Name = "colCSTSeqNo";
            this.colCSTSeqNo.ReadOnly = true;
            this.colCSTSeqNo.Width = 110;
            // 
            // colJobSeqNo
            // 
            this.colJobSeqNo.HeaderText = "Job Seq No";
            this.colJobSeqNo.Name = "colJobSeqNo";
            this.colJobSeqNo.ReadOnly = true;
            this.colJobSeqNo.Width = 110;
            // 
            // colChipPosition
            // 
            this.colChipPosition.HeaderText = "Chip Position";
            this.colChipPosition.Name = "colChipPosition";
            this.colChipPosition.ReadOnly = true;
            this.colChipPosition.Width = 130;
            // 
            // colUnitNo
            // 
            this.colUnitNo.HeaderText = "Unit No";
            this.colUnitNo.Name = "colUnitNo";
            this.colUnitNo.ReadOnly = true;
            // 
            // colUnitID
            // 
            this.colUnitID.HeaderText = "Unit ID";
            this.colUnitID.Name = "colUnitID";
            this.colUnitID.ReadOnly = true;
            this.colUnitID.Width = 120;
            // 
            // colDefectCode
            // 
            this.colDefectCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDefectCode.HeaderText = "Defect Code";
            this.colDefectCode.MinimumWidth = 150;
            this.colDefectCode.Name = "colDefectCode";
            this.colDefectCode.ReadOnly = true;
            // 
            // FormDefectCodeReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1265, 709);
            this.Name = "FormDefectCodeReport";
            this.Text = "FormDefectCodeReport";
            this.Load += new System.EventHandler(this.FormDefectCodeReport_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.pnlCombox.ResumeLayout(false);
            this.pnlCombox.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel pnlCombox;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Label lblNode;
        private System.Windows.Forms.ComboBox cboNode;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNoID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCSTSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJobSeqNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colChipPosition;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnitNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnitID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDefectCode;
    }
}