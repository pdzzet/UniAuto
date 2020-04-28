namespace UniOPI
{
    partial class FormRobotOperationMode
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
            this.tlpRobotOperationMode = new System.Windows.Forms.TableLayoutPanel();
            this.grbSetting = new System.Windows.Forms.GroupBox();
            this.cbbNewMode = new System.Windows.Forms.ComboBox();
            this.lblNewModeName = new System.Windows.Forms.Label();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblCurrentMode = new System.Windows.Forms.Label();
            this.lblCurrentModeName = new System.Windows.Forms.Label();
            this.lblAction = new System.Windows.Forms.Label();
            this.lblActionName = new System.Windows.Forms.Label();
            this.lblDesc = new System.Windows.Forms.Label();
            this.lblDescName = new System.Windows.Forms.Label();
            this.lblLocal = new System.Windows.Forms.Label();
            this.lblLocalDesc = new System.Windows.Forms.Label();
            this.dgvRobotOperMode = new System.Windows.Forms.DataGridView();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPositionNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCurMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpRobotOperationMode.SuspendLayout();
            this.grbSetting.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRobotOperMode)).BeginInit();
            this.pnlButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1074, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpRobotOperationMode);
            this.spcBase.Size = new System.Drawing.Size(1134, 629);
            // 
            // tmrBaseRefresh
            // 
            this.tmrBaseRefresh.Tick += new System.EventHandler(this.tmrBaseRefresh_Tick);
            // 
            // tlpRobotOperationMode
            // 
            this.tlpRobotOperationMode.ColumnCount = 1;
            this.tlpRobotOperationMode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRobotOperationMode.Controls.Add(this.grbSetting, 0, 2);
            this.tlpRobotOperationMode.Controls.Add(this.dgvRobotOperMode, 0, 1);
            this.tlpRobotOperationMode.Controls.Add(this.pnlButton, 0, 0);
            this.tlpRobotOperationMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRobotOperationMode.Location = new System.Drawing.Point(0, 0);
            this.tlpRobotOperationMode.Name = "tlpRobotOperationMode";
            this.tlpRobotOperationMode.RowCount = 3;
            this.tlpRobotOperationMode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tlpRobotOperationMode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRobotOperationMode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 147F));
            this.tlpRobotOperationMode.Size = new System.Drawing.Size(1134, 598);
            this.tlpRobotOperationMode.TabIndex = 0;
            // 
            // grbSetting
            // 
            this.grbSetting.Controls.Add(this.cbbNewMode);
            this.grbSetting.Controls.Add(this.lblNewModeName);
            this.grbSetting.Controls.Add(this.btnSend);
            this.grbSetting.Controls.Add(this.lblCurrentMode);
            this.grbSetting.Controls.Add(this.lblCurrentModeName);
            this.grbSetting.Controls.Add(this.lblAction);
            this.grbSetting.Controls.Add(this.lblActionName);
            this.grbSetting.Controls.Add(this.lblDesc);
            this.grbSetting.Controls.Add(this.lblDescName);
            this.grbSetting.Controls.Add(this.lblLocal);
            this.grbSetting.Controls.Add(this.lblLocalDesc);
            this.grbSetting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbSetting.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbSetting.Location = new System.Drawing.Point(3, 454);
            this.grbSetting.Name = "grbSetting";
            this.grbSetting.Size = new System.Drawing.Size(1128, 141);
            this.grbSetting.TabIndex = 15;
            this.grbSetting.TabStop = false;
            this.grbSetting.Text = "Setting";
            // 
            // cbbNewMode
            // 
            this.cbbNewMode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbbNewMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbNewMode.Font = new System.Drawing.Font("Calibri", 12F);
            this.cbbNewMode.FormattingEnabled = true;
            this.cbbNewMode.Location = new System.Drawing.Point(919, 28);
            this.cbbNewMode.Name = "cbbNewMode";
            this.cbbNewMode.Size = new System.Drawing.Size(158, 27);
            this.cbbNewMode.TabIndex = 11;
            // 
            // lblNewModeName
            // 
            this.lblNewModeName.BackColor = System.Drawing.Color.Black;
            this.lblNewModeName.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewModeName.ForeColor = System.Drawing.Color.White;
            this.lblNewModeName.Location = new System.Drawing.Point(773, 31);
            this.lblNewModeName.Name = "lblNewModeName";
            this.lblNewModeName.Size = new System.Drawing.Size(150, 23);
            this.lblNewModeName.TabIndex = 19;
            this.lblNewModeName.Text = "New Operation Mode";
            this.lblNewModeName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnSend.Location = new System.Drawing.Point(919, 63);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(158, 28);
            this.btnSend.TabIndex = 12;
            this.btnSend.Text = "Set";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblCurrentMode
            // 
            this.lblCurrentMode.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblCurrentMode.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentMode.Location = new System.Drawing.Point(550, 63);
            this.lblCurrentMode.Name = "lblCurrentMode";
            this.lblCurrentMode.Size = new System.Drawing.Size(190, 23);
            this.lblCurrentMode.TabIndex = 18;
            // 
            // lblCurrentModeName
            // 
            this.lblCurrentModeName.BackColor = System.Drawing.Color.Black;
            this.lblCurrentModeName.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentModeName.ForeColor = System.Drawing.Color.White;
            this.lblCurrentModeName.Location = new System.Drawing.Point(404, 63);
            this.lblCurrentModeName.Name = "lblCurrentModeName";
            this.lblCurrentModeName.Size = new System.Drawing.Size(150, 23);
            this.lblCurrentModeName.TabIndex = 17;
            this.lblCurrentModeName.Text = "Current Mode";
            this.lblCurrentModeName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAction
            // 
            this.lblAction.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblAction.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAction.Location = new System.Drawing.Point(196, 63);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(190, 23);
            this.lblAction.TabIndex = 16;
            // 
            // lblActionName
            // 
            this.lblActionName.BackColor = System.Drawing.Color.Black;
            this.lblActionName.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActionName.ForeColor = System.Drawing.Color.White;
            this.lblActionName.Location = new System.Drawing.Point(50, 63);
            this.lblActionName.Name = "lblActionName";
            this.lblActionName.Size = new System.Drawing.Size(150, 23);
            this.lblActionName.TabIndex = 15;
            this.lblActionName.Text = "Action";
            this.lblActionName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDesc
            // 
            this.lblDesc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDesc.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDesc.Location = new System.Drawing.Point(550, 32);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(190, 23);
            this.lblDesc.TabIndex = 14;
            // 
            // lblDescName
            // 
            this.lblDescName.BackColor = System.Drawing.Color.Black;
            this.lblDescName.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescName.ForeColor = System.Drawing.Color.White;
            this.lblDescName.Location = new System.Drawing.Point(404, 32);
            this.lblDescName.Name = "lblDescName";
            this.lblDescName.Size = new System.Drawing.Size(150, 23);
            this.lblDescName.TabIndex = 13;
            this.lblDescName.Text = "Description";
            this.lblDescName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLocal
            // 
            this.lblLocal.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblLocal.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLocal.Location = new System.Drawing.Point(196, 32);
            this.lblLocal.Name = "lblLocal";
            this.lblLocal.Size = new System.Drawing.Size(190, 23);
            this.lblLocal.TabIndex = 12;
            // 
            // lblLocalDesc
            // 
            this.lblLocalDesc.BackColor = System.Drawing.Color.Black;
            this.lblLocalDesc.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLocalDesc.ForeColor = System.Drawing.Color.White;
            this.lblLocalDesc.Location = new System.Drawing.Point(50, 32);
            this.lblLocalDesc.Name = "lblLocalDesc";
            this.lblLocalDesc.Size = new System.Drawing.Size(150, 23);
            this.lblLocalDesc.TabIndex = 11;
            this.lblLocalDesc.Text = "Local";
            this.lblLocalDesc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvRobotOperMode
            // 
            this.dgvRobotOperMode.AllowUserToAddRows = false;
            this.dgvRobotOperMode.AllowUserToDeleteRows = false;
            this.dgvRobotOperMode.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvRobotOperMode.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvRobotOperMode.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRobotOperMode.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvRobotOperMode.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRobotOperMode.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLocalNo,
            this.colLocalID,
            this.colPositionNo,
            this.colDescription,
            this.colAction,
            this.colCurMode});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvRobotOperMode.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvRobotOperMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRobotOperMode.Location = new System.Drawing.Point(3, 45);
            this.dgvRobotOperMode.Name = "dgvRobotOperMode";
            this.dgvRobotOperMode.ReadOnly = true;
            this.dgvRobotOperMode.RowHeadersVisible = false;
            this.dgvRobotOperMode.RowTemplate.Height = 24;
            this.dgvRobotOperMode.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRobotOperMode.Size = new System.Drawing.Size(1128, 403);
            this.dgvRobotOperMode.TabIndex = 14;
            this.dgvRobotOperMode.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRobotOperMode_CellClick);
            // 
            // colLocalNo
            // 
            this.colLocalNo.HeaderText = "Local";
            this.colLocalNo.Name = "colLocalNo";
            this.colLocalNo.ReadOnly = true;
            this.colLocalNo.Width = 150;
            // 
            // colLocalID
            // 
            this.colLocalID.HeaderText = "Local ID";
            this.colLocalID.Name = "colLocalID";
            this.colLocalID.ReadOnly = true;
            this.colLocalID.Width = 200;
            // 
            // colPositionNo
            // 
            this.colPositionNo.HeaderText = "Position No";
            this.colPositionNo.Name = "colPositionNo";
            this.colPositionNo.ReadOnly = true;
            this.colPositionNo.Visible = false;
            this.colPositionNo.Width = 150;
            // 
            // colDescription
            // 
            this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            // 
            // colAction
            // 
            this.colAction.HeaderText = "Action";
            this.colAction.Name = "colAction";
            this.colAction.ReadOnly = true;
            this.colAction.Width = 200;
            // 
            // colCurMode
            // 
            this.colCurMode.HeaderText = "Current Mode";
            this.colCurMode.Name = "colCurMode";
            this.colCurMode.ReadOnly = true;
            this.colCurMode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colCurMode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colCurMode.Width = 200;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnRefresh);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(3, 3);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(1128, 36);
            this.pnlButton.TabIndex = 16;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnRefresh.Location = new System.Drawing.Point(1028, 0);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 36);
            this.btnRefresh.TabIndex = 13;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // FormRobotOperationMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1134, 629);
            this.Name = "FormRobotOperationMode";
            this.Text = "FormRobotOperationMode";
            this.Load += new System.EventHandler(this.FormRobotOperationMode_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpRobotOperationMode.ResumeLayout(false);
            this.grbSetting.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRobotOperMode)).EndInit();
            this.pnlButton.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpRobotOperationMode;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ComboBox cbbNewMode;
        private System.Windows.Forms.DataGridView dgvRobotOperMode;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.GroupBox grbSetting;
        private System.Windows.Forms.Label lblLocal;
        private System.Windows.Forms.Label lblLocalDesc;
        private System.Windows.Forms.Label lblNewModeName;
        private System.Windows.Forms.Label lblCurrentMode;
        private System.Windows.Forms.Label lblCurrentModeName;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.Label lblActionName;
        private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.Label lblDescName;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPositionNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAction;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCurMode;
    }
}