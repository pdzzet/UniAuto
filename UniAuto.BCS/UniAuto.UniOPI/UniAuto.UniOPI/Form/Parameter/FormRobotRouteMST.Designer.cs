namespace UniOPI
{
    partial class FormRobotRouteMST
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
            this.tlpRobotRouteMST = new System.Windows.Forms.TableLayoutPanel();
            this.pnlRobotView = new System.Windows.Forms.Panel();
            this.pnlRobot = new System.Windows.Forms.Panel();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.pnlSideBarBtn = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnModify = new System.Windows.Forms.Button();
            this.colObjectKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.chkIsEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colRTCModeFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colRTCForceReturnFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colRobotName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRouteID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRouteName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemarks = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastUpdateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRoutePriority = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpRobotRouteMST.SuspendLayout();
            this.pnlRobotView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.pnlSideBarBtn.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1200, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpRobotRouteMST);
            this.spcBase.Size = new System.Drawing.Size(1260, 522);
            // 
            // tlpRobotRouteMST
            // 
            this.tlpRobotRouteMST.ColumnCount = 2;
            this.tlpRobotRouteMST.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRobotRouteMST.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tlpRobotRouteMST.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRobotRouteMST.Controls.Add(this.pnlRobotView, 0, 0);
            this.tlpRobotRouteMST.Controls.Add(this.dgvData, 0, 1);
            this.tlpRobotRouteMST.Controls.Add(this.pnlSideBarBtn, 1, 1);
            this.tlpRobotRouteMST.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRobotRouteMST.Location = new System.Drawing.Point(0, 0);
            this.tlpRobotRouteMST.Name = "tlpRobotRouteMST";
            this.tlpRobotRouteMST.RowCount = 2;
            this.tlpRobotRouteMST.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tlpRobotRouteMST.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRobotRouteMST.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRobotRouteMST.Size = new System.Drawing.Size(1260, 491);
            this.tlpRobotRouteMST.TabIndex = 1;
            // 
            // pnlRobotView
            // 
            this.pnlRobotView.Controls.Add(this.pnlRobot);
            this.pnlRobotView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRobotView.Location = new System.Drawing.Point(3, 3);
            this.pnlRobotView.Name = "pnlRobotView";
            this.pnlRobotView.Size = new System.Drawing.Size(1129, 54);
            this.pnlRobotView.TabIndex = 19;
            // 
            // pnlRobot
            // 
            this.pnlRobot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRobot.Location = new System.Drawing.Point(0, 0);
            this.pnlRobot.Name = "pnlRobot";
            this.pnlRobot.Size = new System.Drawing.Size(1129, 54);
            this.pnlRobot.TabIndex = 0;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToOrderColumns = true;
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
            this.colObjectKey,
            this.colServerName,
            this.chkIsEnabled,
            this.colRTCModeFlag,
            this.colRTCForceReturnFlag,
            this.colRobotName,
            this.colRouteID,
            this.colRouteName,
            this.colLineType,
            this.colDescription,
            this.colRemarks,
            this.colLastUpdateTime,
            this.colRoutePriority});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 63);
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1129, 425);
            this.dgvData.TabIndex = 16;
            this.dgvData.DataSourceChanged += new System.EventHandler(this.dgvData_DataSourceChanged);
            this.dgvData.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvData_ColumnHeaderMouseClick);
            // 
            // pnlSideBarBtn
            // 
            this.pnlSideBarBtn.Controls.Add(this.panel1);
            this.pnlSideBarBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSideBarBtn.Location = new System.Drawing.Point(1138, 63);
            this.pnlSideBarBtn.Name = "pnlSideBarBtn";
            this.pnlSideBarBtn.Size = new System.Drawing.Size(119, 425);
            this.pnlSideBarBtn.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Controls.Add(this.btnAdd);
            this.panel1.Controls.Add(this.btnRefresh);
            this.panel1.Controls.Add(this.btnDelete);
            this.panel1.Controls.Add(this.btnModify);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(119, 425);
            this.panel1.TabIndex = 15;
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(4, 159);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(113, 50);
            this.btnSave.TabIndex = 12;
            this.btnSave.Tag = "Save";
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAdd.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(4, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(113, 50);
            this.btnAdd.TabIndex = 8;
            this.btnAdd.Tag = "Add";
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(4, 211);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(113, 50);
            this.btnRefresh.TabIndex = 11;
            this.btnRefresh.Tag = "Refresh";
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(4, 107);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(113, 50);
            this.btnDelete.TabIndex = 9;
            this.btnDelete.Tag = "Delete";
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnModify
            // 
            this.btnModify.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnModify.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModify.Location = new System.Drawing.Point(4, 55);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(113, 50);
            this.btnModify.TabIndex = 10;
            this.btnModify.Tag = "Modify";
            this.btnModify.Text = "Modify";
            this.btnModify.UseVisualStyleBackColor = true;
            this.btnModify.Click += new System.EventHandler(this.btn_Click);
            // 
            // colObjectKey
            // 
            this.colObjectKey.DataPropertyName = "OBJECTKEY";
            this.colObjectKey.HeaderText = "OBJECT KEY";
            this.colObjectKey.Name = "colObjectKey";
            this.colObjectKey.ReadOnly = true;
            this.colObjectKey.Visible = false;
            // 
            // colServerName
            // 
            this.colServerName.DataPropertyName = "SERVERNAME";
            this.colServerName.HeaderText = "ServerName";
            this.colServerName.Name = "colServerName";
            this.colServerName.ReadOnly = true;
            this.colServerName.Visible = false;
            // 
            // chkIsEnabled
            // 
            this.chkIsEnabled.DataPropertyName = "ISENABLED";
            this.chkIsEnabled.FalseValue = "N";
            this.chkIsEnabled.HeaderText = "Enabled";
            this.chkIsEnabled.Name = "chkIsEnabled";
            this.chkIsEnabled.ReadOnly = true;
            this.chkIsEnabled.TrueValue = "Y";
            this.chkIsEnabled.Width = 80;
            // 
            // colRTCModeFlag
            // 
            this.colRTCModeFlag.DataPropertyName = "RTCMODEFLAG";
            this.colRTCModeFlag.FalseValue = "N";
            this.colRTCModeFlag.HeaderText = "RTC Flag";
            this.colRTCModeFlag.Name = "colRTCModeFlag";
            this.colRTCModeFlag.ReadOnly = true;
            this.colRTCModeFlag.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colRTCModeFlag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colRTCModeFlag.TrueValue = "Y";
            // 
            // colRTCForceReturnFlag
            // 
            this.colRTCForceReturnFlag.DataPropertyName = "RTCFORCERETURNFLAG";
            this.colRTCForceReturnFlag.FalseValue = "N";
            this.colRTCForceReturnFlag.HeaderText = "RTC Force Return Flag";
            this.colRTCForceReturnFlag.Name = "colRTCForceReturnFlag";
            this.colRTCForceReturnFlag.ReadOnly = true;
            this.colRTCForceReturnFlag.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colRTCForceReturnFlag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colRTCForceReturnFlag.TrueValue = "Y";
            this.colRTCForceReturnFlag.Width = 200;
            // 
            // colRobotName
            // 
            this.colRobotName.DataPropertyName = "ROBOTNAME";
            this.colRobotName.HeaderText = "Robot Name";
            this.colRobotName.Name = "colRobotName";
            this.colRobotName.ReadOnly = true;
            this.colRobotName.Visible = false;
            this.colRobotName.Width = 150;
            // 
            // colRouteID
            // 
            this.colRouteID.DataPropertyName = "ROUTEID";
            this.colRouteID.HeaderText = "Route ID";
            this.colRouteID.Name = "colRouteID";
            this.colRouteID.ReadOnly = true;
            this.colRouteID.Width = 150;
            // 
            // colRouteName
            // 
            this.colRouteName.DataPropertyName = "ROUTENAME";
            this.colRouteName.HeaderText = "Route Name";
            this.colRouteName.Name = "colRouteName";
            this.colRouteName.ReadOnly = true;
            this.colRouteName.Width = 160;
            // 
            // colLineType
            // 
            this.colLineType.DataPropertyName = "LINETYPE";
            this.colLineType.HeaderText = "LineType";
            this.colLineType.Name = "colLineType";
            this.colLineType.ReadOnly = true;
            this.colLineType.Visible = false;
            // 
            // colDescription
            // 
            this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDescription.DataPropertyName = "DESCRIPTION";
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            this.colDescription.Width = 450;
            // 
            // colRemarks
            // 
            this.colRemarks.DataPropertyName = "REMARKS";
            this.colRemarks.HeaderText = "Remarks";
            this.colRemarks.Name = "colRemarks";
            this.colRemarks.ReadOnly = true;
            // 
            // colLastUpdateTime
            // 
            this.colLastUpdateTime.DataPropertyName = "LASTUPDATETIME";
            this.colLastUpdateTime.HeaderText = "Last Update Time";
            this.colLastUpdateTime.Name = "colLastUpdateTime";
            this.colLastUpdateTime.ReadOnly = true;
            this.colLastUpdateTime.Width = 190;
            // 
            // colRoutePriority
            // 
            this.colRoutePriority.DataPropertyName = "ROUTEPRIORITY";
            this.colRoutePriority.HeaderText = "Route Priority";
            this.colRoutePriority.Name = "colRoutePriority";
            this.colRoutePriority.ReadOnly = true;
            this.colRoutePriority.Width = 150;
            // 
            // FormRobotRouteMST
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1260, 522);
            this.Name = "FormRobotRouteMST";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormRobotRouteMST_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpRobotRouteMST.ResumeLayout(false);
            this.pnlRobotView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.pnlSideBarBtn.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpRobotRouteMST;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Panel pnlSideBarBtn;
        protected System.Windows.Forms.Panel panel1;
        protected System.Windows.Forms.Button btnSave;
        protected System.Windows.Forms.Button btnAdd;
        protected System.Windows.Forms.Button btnRefresh;
        protected System.Windows.Forms.Button btnDelete;
        protected System.Windows.Forms.Button btnModify;
        private System.Windows.Forms.Panel pnlRobotView;
        private System.Windows.Forms.Panel pnlRobot;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chkIsEnabled;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colRTCModeFlag;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colRTCForceReturnFlag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRouteID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRouteName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemarks;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastUpdateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRoutePriority;

    }
}