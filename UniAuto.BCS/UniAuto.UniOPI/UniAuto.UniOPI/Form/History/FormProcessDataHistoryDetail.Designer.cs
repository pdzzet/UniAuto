namespace UniOPI
{
    partial class FormProcessDataHistoryDetail
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
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.dgvtxtNAME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtVALUE = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gbxGridData = new System.Windows.Forms.GroupBox();
            this.gbxBody = new System.Windows.Forms.GroupBox();
            this.gpxHeader = new System.Windows.Forms.GroupBox();
            this.txtJobID = new System.Windows.Forms.TextBox();
            this.lblJobID = new System.Windows.Forms.Label();
            this.txtNodeNo = new System.Windows.Forms.TextBox();
            this.txtCstSeqNo = new System.Windows.Forms.TextBox();
            this.txtJobSeqNo = new System.Windows.Forms.TextBox();
            this.lblNodeNo = new System.Windows.Forms.Label();
            this.lblCstSeqNo = new System.Windows.Forms.Label();
            this.lblJobSeqNo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.gbxGridData.SuspendLayout();
            this.gbxBody.SuspendLayout();
            this.gpxHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(788, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.gbxGridData);
            this.spcBase.Size = new System.Drawing.Size(848, 609);
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
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvtxtNAME,
            this.dgvtxtVALUE});
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 21);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvData.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(836, 427);
            this.dgvData.TabIndex = 11;
            // 
            // dgvtxtNAME
            // 
            this.dgvtxtNAME.DataPropertyName = "NAME";
            this.dgvtxtNAME.HeaderText = "Name";
            this.dgvtxtNAME.Name = "dgvtxtNAME";
            this.dgvtxtNAME.ReadOnly = true;
            this.dgvtxtNAME.Width = 450;
            // 
            // dgvtxtVALUE
            // 
            this.dgvtxtVALUE.DataPropertyName = "VALUE";
            this.dgvtxtVALUE.HeaderText = "Value";
            this.dgvtxtVALUE.Name = "dgvtxtVALUE";
            this.dgvtxtVALUE.ReadOnly = true;
            this.dgvtxtVALUE.Width = 483;
            // 
            // gbxGridData
            // 
            this.gbxGridData.Controls.Add(this.gbxBody);
            this.gbxGridData.Controls.Add(this.gpxHeader);
            this.gbxGridData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxGridData.Location = new System.Drawing.Point(0, 0);
            this.gbxGridData.Name = "gbxGridData";
            this.gbxGridData.Size = new System.Drawing.Size(848, 578);
            this.gbxGridData.TabIndex = 12;
            this.gbxGridData.TabStop = false;
            // 
            // gbxBody
            // 
            this.gbxBody.Controls.Add(this.dgvData);
            this.gbxBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxBody.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbxBody.Location = new System.Drawing.Point(3, 124);
            this.gbxBody.Name = "gbxBody";
            this.gbxBody.Size = new System.Drawing.Size(842, 451);
            this.gbxBody.TabIndex = 13;
            this.gbxBody.TabStop = false;
            this.gbxBody.Text = "Detail";
            // 
            // gpxHeader
            // 
            this.gpxHeader.Controls.Add(this.txtJobID);
            this.gpxHeader.Controls.Add(this.lblJobID);
            this.gpxHeader.Controls.Add(this.txtNodeNo);
            this.gpxHeader.Controls.Add(this.txtCstSeqNo);
            this.gpxHeader.Controls.Add(this.txtJobSeqNo);
            this.gpxHeader.Controls.Add(this.lblNodeNo);
            this.gpxHeader.Controls.Add(this.lblCstSeqNo);
            this.gpxHeader.Controls.Add(this.lblJobSeqNo);
            this.gpxHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.gpxHeader.Location = new System.Drawing.Point(3, 16);
            this.gpxHeader.Name = "gpxHeader";
            this.gpxHeader.Size = new System.Drawing.Size(842, 108);
            this.gpxHeader.TabIndex = 12;
            this.gpxHeader.TabStop = false;
            // 
            // txtJobID
            // 
            this.txtJobID.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtJobID.Location = new System.Drawing.Point(386, 25);
            this.txtJobID.Name = "txtJobID";
            this.txtJobID.ReadOnly = true;
            this.txtJobID.Size = new System.Drawing.Size(144, 26);
            this.txtJobID.TabIndex = 10;
            // 
            // lblJobID
            // 
            this.lblJobID.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblJobID.Location = new System.Drawing.Point(278, 27);
            this.lblJobID.Name = "lblJobID";
            this.lblJobID.Size = new System.Drawing.Size(100, 25);
            this.lblJobID.TabIndex = 9;
            this.lblJobID.Text = "Job ID";
            this.lblJobID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtNodeNo
            // 
            this.txtNodeNo.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNodeNo.Location = new System.Drawing.Point(128, 23);
            this.txtNodeNo.Name = "txtNodeNo";
            this.txtNodeNo.ReadOnly = true;
            this.txtNodeNo.Size = new System.Drawing.Size(144, 26);
            this.txtNodeNo.TabIndex = 8;
            // 
            // txtCstSeqNo
            // 
            this.txtCstSeqNo.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCstSeqNo.Location = new System.Drawing.Point(386, 59);
            this.txtCstSeqNo.Name = "txtCstSeqNo";
            this.txtCstSeqNo.ReadOnly = true;
            this.txtCstSeqNo.Size = new System.Drawing.Size(144, 26);
            this.txtCstSeqNo.TabIndex = 6;
            // 
            // txtJobSeqNo
            // 
            this.txtJobSeqNo.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtJobSeqNo.Location = new System.Drawing.Point(128, 59);
            this.txtJobSeqNo.Name = "txtJobSeqNo";
            this.txtJobSeqNo.ReadOnly = true;
            this.txtJobSeqNo.Size = new System.Drawing.Size(144, 26);
            this.txtJobSeqNo.TabIndex = 5;
            // 
            // lblNodeNo
            // 
            this.lblNodeNo.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNodeNo.Location = new System.Drawing.Point(13, 25);
            this.lblNodeNo.Name = "lblNodeNo";
            this.lblNodeNo.Size = new System.Drawing.Size(100, 25);
            this.lblNodeNo.TabIndex = 3;
            this.lblNodeNo.Text = "Local No";
            this.lblNodeNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCstSeqNo
            // 
            this.lblCstSeqNo.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCstSeqNo.Location = new System.Drawing.Point(300, 61);
            this.lblCstSeqNo.Name = "lblCstSeqNo";
            this.lblCstSeqNo.Size = new System.Drawing.Size(80, 25);
            this.lblCstSeqNo.TabIndex = 1;
            this.lblCstSeqNo.Text = "Cst Seq No";
            this.lblCstSeqNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblJobSeqNo
            // 
            this.lblJobSeqNo.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblJobSeqNo.Location = new System.Drawing.Point(13, 61);
            this.lblJobSeqNo.Name = "lblJobSeqNo";
            this.lblJobSeqNo.Size = new System.Drawing.Size(100, 25);
            this.lblJobSeqNo.TabIndex = 0;
            this.lblJobSeqNo.Text = "Job Seq No";
            this.lblJobSeqNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FormProcessDataHistoryDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 609);
            this.ControlBox = true;
            this.Name = "FormProcessDataHistoryDetail";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormProcessDataHistoryDetail_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.gbxGridData.ResumeLayout(false);
            this.gbxBody.ResumeLayout(false);
            this.gpxHeader.ResumeLayout(false);
            this.gpxHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.GroupBox gbxGridData;
        private System.Windows.Forms.GroupBox gbxBody;
        private System.Windows.Forms.GroupBox gpxHeader;
        private System.Windows.Forms.TextBox txtNodeNo;
        private System.Windows.Forms.TextBox txtCstSeqNo;
        private System.Windows.Forms.TextBox txtJobSeqNo;
        private System.Windows.Forms.Label lblNodeNo;
        private System.Windows.Forms.Label lblCstSeqNo;
        private System.Windows.Forms.Label lblJobSeqNo;
        private System.Windows.Forms.TextBox txtJobID;
        private System.Windows.Forms.Label lblJobID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtNAME;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtVALUE;

    }
}