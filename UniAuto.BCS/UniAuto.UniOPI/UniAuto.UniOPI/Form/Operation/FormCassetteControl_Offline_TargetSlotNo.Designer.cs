namespace UniOPI
{
    partial class FormCassetteControl_Offline_TargetSlotNo
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
            this.tplBase = new System.Windows.Forms.TableLayoutPanel();
            this.grbSlot = new System.Windows.Forms.GroupBox();
            this.dgvCassette = new System.Windows.Forms.DataGridView();
            this.colProcessFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colSlotNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGlassID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOldTargetSlotNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTargetSlotNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.pnlSetting = new System.Windows.Forms.Panel();
            this.chkBySeqNo = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tplBase.SuspendLayout();
            this.grbSlot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCassette)).BeginInit();
            this.pnlButton.SuspendLayout();
            this.pnlSetting.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(630, 30);
            this.lblCaption.Text = "Target Slot No Setting";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tplBase);
            this.spcBase.Size = new System.Drawing.Size(690, 618);
            // 
            // tplBase
            // 
            this.tplBase.ColumnCount = 1;
            this.tplBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tplBase.Controls.Add(this.grbSlot, 0, 1);
            this.tplBase.Controls.Add(this.pnlButton, 0, 2);
            this.tplBase.Controls.Add(this.pnlSetting, 0, 0);
            this.tplBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tplBase.Location = new System.Drawing.Point(0, 0);
            this.tplBase.Name = "tplBase";
            this.tplBase.RowCount = 3;
            this.tplBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 71F));
            this.tplBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tplBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F));
            this.tplBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tplBase.Size = new System.Drawing.Size(690, 587);
            this.tplBase.TabIndex = 2;
            // 
            // grbSlot
            // 
            this.grbSlot.Controls.Add(this.dgvCassette);
            this.grbSlot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbSlot.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbSlot.Location = new System.Drawing.Point(3, 74);
            this.grbSlot.Name = "grbSlot";
            this.grbSlot.Size = new System.Drawing.Size(684, 453);
            this.grbSlot.TabIndex = 3;
            this.grbSlot.TabStop = false;
            // 
            // dgvCassette
            // 
            this.dgvCassette.AllowUserToAddRows = false;
            this.dgvCassette.AllowUserToDeleteRows = false;
            this.dgvCassette.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvCassette.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvCassette.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCassette.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvCassette.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCassette.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colProcessFlag,
            this.colSlotNo,
            this.colGlassID,
            this.colOldTargetSlotNo,
            this.colTargetSlotNo});
            this.dgvCassette.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCassette.Location = new System.Drawing.Point(3, 22);
            this.dgvCassette.Name = "dgvCassette";
            this.dgvCassette.RowHeadersVisible = false;
            this.dgvCassette.RowTemplate.Height = 24;
            this.dgvCassette.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvCassette.Size = new System.Drawing.Size(678, 428);
            this.dgvCassette.TabIndex = 17;
            // 
            // colProcessFlag
            // 
            this.colProcessFlag.HeaderText = "Process";
            this.colProcessFlag.Name = "colProcessFlag";
            this.colProcessFlag.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colProcessFlag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colProcessFlag.Width = 80;
            // 
            // colSlotNo
            // 
            this.colSlotNo.HeaderText = "Slot No";
            this.colSlotNo.Name = "colSlotNo";
            this.colSlotNo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colSlotNo.Width = 80;
            // 
            // colGlassID
            // 
            this.colGlassID.HeaderText = "Glass ID";
            this.colGlassID.Name = "colGlassID";
            this.colGlassID.Width = 150;
            // 
            // colOldTargetSlotNo
            // 
            this.colOldTargetSlotNo.HeaderText = "Old Target Slot No";
            this.colOldTargetSlotNo.Name = "colOldTargetSlotNo";
            this.colOldTargetSlotNo.Width = 180;
            // 
            // colTargetSlotNo
            // 
            this.colTargetSlotNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTargetSlotNo.HeaderText = "Target Slot No";
            this.colTargetSlotNo.Name = "colTargetSlotNo";
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnCancel);
            this.pnlButton.Controls.Add(this.btnOK);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(3, 533);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(684, 51);
            this.pnlButton.TabIndex = 5;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(335, 8);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(229, 8);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 35);
            this.btnOK.TabIndex = 22;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // pnlSetting
            // 
            this.pnlSetting.Controls.Add(this.chkBySeqNo);
            this.pnlSetting.Location = new System.Drawing.Point(3, 3);
            this.pnlSetting.Name = "pnlSetting";
            this.pnlSetting.Size = new System.Drawing.Size(684, 65);
            this.pnlSetting.TabIndex = 6;
            // 
            // chkBySeqNo
            // 
            this.chkBySeqNo.AutoSize = true;
            this.chkBySeqNo.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.chkBySeqNo.Location = new System.Drawing.Point(552, 25);
            this.chkBySeqNo.Name = "chkBySeqNo";
            this.chkBySeqNo.Size = new System.Drawing.Size(123, 25);
            this.chkBySeqNo.TabIndex = 24;
            this.chkBySeqNo.Text = "Is By Seuqnce";
            this.chkBySeqNo.UseVisualStyleBackColor = true;
            this.chkBySeqNo.CheckedChanged += new System.EventHandler(this.chkBySeqNo_CheckedChanged);
            // 
            // FormCassetteControl_Offline_TargetSlotNo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 618);
            this.Name = "FormCassetteControl_Offline_TargetSlotNo";
            this.Text = "FormCassetteControl_Offline_TargetSlotNo";
            this.Load += new System.EventHandler(this.FormCassetteControl_Offline_TargetSlotNo_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tplBase.ResumeLayout(false);
            this.grbSlot.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCassette)).EndInit();
            this.pnlButton.ResumeLayout(false);
            this.pnlSetting.ResumeLayout(false);
            this.pnlSetting.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tplBase;
        private System.Windows.Forms.GroupBox grbSlot;
        private System.Windows.Forms.DataGridView dgvCassette;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colProcessFlag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGlassID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOldTargetSlotNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTargetSlotNo;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel pnlSetting;
        private System.Windows.Forms.CheckBox chkBySeqNo;
    }
}