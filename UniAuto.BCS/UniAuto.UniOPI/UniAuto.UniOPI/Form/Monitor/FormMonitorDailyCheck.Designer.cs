namespace UniOPI
{
    partial class FormMonitorDailyCheck
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
            this.spcUtility = new System.Windows.Forms.SplitContainer();
            this.ucAutoBtnEqp = new UniOPI.UcAutoButtons();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.dgvtxtNAME = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtxtVALUE = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcUtility)).BeginInit();
            this.spcUtility.Panel1.SuspendLayout();
            this.spcUtility.Panel2.SuspendLayout();
            this.spcUtility.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(745, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.spcUtility);
            this.spcBase.Size = new System.Drawing.Size(805, 501);
            // 
            // spcUtility
            // 
            this.spcUtility.BackColor = System.Drawing.Color.Transparent;
            this.spcUtility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcUtility.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spcUtility.Location = new System.Drawing.Point(0, 0);
            this.spcUtility.Name = "spcUtility";
            // 
            // spcUtility.Panel1
            // 
            this.spcUtility.Panel1.Controls.Add(this.ucAutoBtnEqp);
            this.spcUtility.Panel1.Controls.Add(this.btnRefresh);
            // 
            // spcUtility.Panel2
            // 
            this.spcUtility.Panel2.Controls.Add(this.groupBox1);
            this.spcUtility.Size = new System.Drawing.Size(805, 470);
            this.spcUtility.SplitterDistance = 244;
            this.spcUtility.TabIndex = 2;
            // 
            // ucAutoBtnEqp
            // 
            this.ucAutoBtnEqp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucAutoBtnEqp.Location = new System.Drawing.Point(0, 0);
            this.ucAutoBtnEqp.Name = "ucAutoBtnEqp";
            this.ucAutoBtnEqp.Size = new System.Drawing.Size(244, 428);
            this.ucAutoBtnEqp.TabIndex = 19;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImage = global::UniOPI.Properties.Resources.BtnBackground_Normal;
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(0, 428);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(244, 42);
            this.btnRefresh.TabIndex = 18;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dgvData);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(557, 470);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
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
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvtxtNAME,
            this.dgvtxtVALUE});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 18);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(551, 449);
            this.dgvData.TabIndex = 11;
            // 
            // dgvtxtNAME
            // 
            this.dgvtxtNAME.DataPropertyName = "NAME";
            this.dgvtxtNAME.HeaderText = "Description";
            this.dgvtxtNAME.Name = "dgvtxtNAME";
            this.dgvtxtNAME.ReadOnly = true;
            this.dgvtxtNAME.Width = 200;
            // 
            // dgvtxtVALUE
            // 
            this.dgvtxtVALUE.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvtxtVALUE.DataPropertyName = "VALUE";
            this.dgvtxtVALUE.HeaderText = "Value";
            this.dgvtxtVALUE.Name = "dgvtxtVALUE";
            this.dgvtxtVALUE.ReadOnly = true;
            // 
            // FormMonitorDailyCheck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(805, 501);
            this.Name = "FormMonitorDailyCheck";
            this.Text = "FormMonitorDailyCheck";
            this.Shown += new System.EventHandler(this.FormMonitorDailyCheck_Shown);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.spcUtility.Panel1.ResumeLayout(false);
            this.spcUtility.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcUtility)).EndInit();
            this.spcUtility.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spcUtility;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dgvData;
        private UcAutoButtons ucAutoBtnEqp;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtNAME;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtxtVALUE;
    }
}