namespace UniOPI
{
    partial class FormAlarmStatus
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.spcAlarm = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.ucAutoBtnEqp = new UniOPI.UcAutoButtons();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colAlarmID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAlarmLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAlarmDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcAlarm)).BeginInit();
            this.spcAlarm.Panel1.SuspendLayout();
            this.spcAlarm.Panel2.SuspendLayout();
            this.spcAlarm.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(960, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.spcAlarm);
            this.spcBase.Size = new System.Drawing.Size(1020, 593);
            // 
            // spcAlarm
            // 
            this.spcAlarm.BackColor = System.Drawing.Color.Transparent;
            this.spcAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcAlarm.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spcAlarm.Location = new System.Drawing.Point(0, 0);
            this.spcAlarm.Name = "spcAlarm";
            // 
            // spcAlarm.Panel1
            // 
            this.spcAlarm.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // spcAlarm.Panel2
            // 
            this.spcAlarm.Panel2.Controls.Add(this.groupBox1);
            this.spcAlarm.Size = new System.Drawing.Size(1020, 562);
            this.spcAlarm.SplitterDistance = 244;
            this.spcAlarm.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.btnRefresh, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.ucAutoBtnEqp, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(244, 562);
            this.tableLayoutPanel1.TabIndex = 17;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImage = global::UniOPI.Properties.Resources.BtnBackground_Normal;
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(3, 523);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(238, 36);
            this.btnRefresh.TabIndex = 16;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // ucAutoBtnEqp
            // 
            this.ucAutoBtnEqp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucAutoBtnEqp.Location = new System.Drawing.Point(0, 0);
            this.ucAutoBtnEqp.Margin = new System.Windows.Forms.Padding(0);
            this.ucAutoBtnEqp.Name = "ucAutoBtnEqp";
            this.ucAutoBtnEqp.Size = new System.Drawing.Size(244, 520);
            this.ucAutoBtnEqp.TabIndex = 17;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dgvData);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(772, 562);
            this.groupBox1.TabIndex = 16;
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
            this.colAlarmID,
            this.colLocalNo,
            this.colAlarmLevel,
            this.colAlarmDesc});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.dgvData.Size = new System.Drawing.Size(766, 541);
            this.dgvData.TabIndex = 13;
            // 
            // colAlarmID
            // 
            this.colAlarmID.DataPropertyName = "Alarm ID";
            this.colAlarmID.HeaderText = "Alarm ID";
            this.colAlarmID.Name = "colAlarmID";
            this.colAlarmID.ReadOnly = true;
            this.colAlarmID.Width = 200;
            // 
            // colLocalNo
            // 
            this.colLocalNo.DataPropertyName = "Local Node";
            this.colLocalNo.HeaderText = "Local Node";
            this.colLocalNo.Name = "colLocalNo";
            this.colLocalNo.ReadOnly = true;
            this.colLocalNo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colAlarmLevel
            // 
            this.colAlarmLevel.DataPropertyName = "Alarm Level";
            this.colAlarmLevel.HeaderText = "Alarm Level";
            this.colAlarmLevel.Name = "colAlarmLevel";
            this.colAlarmLevel.ReadOnly = true;
            this.colAlarmLevel.Width = 120;
            // 
            // colAlarmDesc
            // 
            this.colAlarmDesc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colAlarmDesc.DataPropertyName = "Alarm Description";
            this.colAlarmDesc.HeaderText = "Alarm Description";
            this.colAlarmDesc.Name = "colAlarmDesc";
            this.colAlarmDesc.ReadOnly = true;
            this.colAlarmDesc.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 1000;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // FormAlarmStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 593);
            this.Name = "FormAlarmStatus";
            this.Text = "FormAlarmStatus";
            this.Shown += new System.EventHandler(this.FormAlarmStatus_Shown);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.spcAlarm.Panel1.ResumeLayout(false);
            this.spcAlarm.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcAlarm)).EndInit();
            this.spcAlarm.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spcAlarm;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private UcAutoButtons ucAutoBtnEqp;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlarmID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlarmLevel;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlarmDesc;
        public System.Windows.Forms.Timer tmrRefresh;
    }
}