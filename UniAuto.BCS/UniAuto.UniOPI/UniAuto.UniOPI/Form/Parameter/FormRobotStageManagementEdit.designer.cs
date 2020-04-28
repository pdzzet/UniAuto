namespace UniOPI
{
    partial class FormRobotStageManagementEdit
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
            this.btnAddOK = new System.Windows.Forms.Button();
            this.btnCancelClose = new System.Windows.Forms.Button();
            this.btnOKClose = new System.Windows.Forms.Button();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.flpButton = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlAdd = new System.Windows.Forms.Panel();
            this.btnClear = new System.Windows.Forms.Button();
            this.dgvAddList = new System.Windows.Forms.DataGridView();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageIDByNode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPriority = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageReportTrxName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageJobDataTrxName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsMultiSlot = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colSlotMaxCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecipeCheckFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDummyCheckFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colGetReadyFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colPutReadyFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colPrefetchFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colRTCReworkFlag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSupportWaitFront = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colUpstreamPathTrxName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUpstreamSendPathTrxName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDownstreamPathTrxName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDownstreamReceivePathTrxName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrackDataSeqList = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCassetteType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemark = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colSlotFetchSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSlotStoreSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExchangeType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEQRobotIfType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.grbData = new System.Windows.Forms.GroupBox();
            this.pnlEdit = new System.Windows.Forms.Panel();
            this.cboEQRobotIfType = new System.Windows.Forms.ComboBox();
            this.lblEQRobotIfType = new System.Windows.Forms.Label();
            this.pnlTrackDataSeqList = new System.Windows.Forms.Panel();
            this.txtTrackDataSeqList = new System.Windows.Forms.TextBox();
            this.lblTrackDataSeqList = new System.Windows.Forms.Label();
            this.cboExchangeType = new System.Windows.Forms.ComboBox();
            this.lblExchangeType = new System.Windows.Forms.Label();
            this.flpFloatArea1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlCassetteType = new System.Windows.Forms.Panel();
            this.cboCassetteType = new System.Windows.Forms.ComboBox();
            this.lblStageCstType = new System.Windows.Forms.Label();
            this.pnlSlotFetchSeq = new System.Windows.Forms.Panel();
            this.cboSlotFetchSeq = new System.Windows.Forms.ComboBox();
            this.lblSlotFetchSeq = new System.Windows.Forms.Label();
            this.pnlSlotStoreSeq = new System.Windows.Forms.Panel();
            this.cboSlotStoreSeq = new System.Windows.Forms.ComboBox();
            this.lblSlotStoreSeq = new System.Windows.Forms.Label();
            this.pnlArmUsePriority = new System.Windows.Forms.Panel();
            this.cboArmUsePriority = new System.Windows.Forms.ComboBox();
            this.lblArmUsePriority = new System.Windows.Forms.Label();
            this.pnlUseSpecificArm = new System.Windows.Forms.Panel();
            this.cboUseSpecificArm = new System.Windows.Forms.ComboBox();
            this.lblUseSpecificArm = new System.Windows.Forms.Label();
            this.flpFloatArea2 = new System.Windows.Forms.FlowLayoutPanel();
            this.chkStageEnabled = new System.Windows.Forms.CheckBox();
            this.chkPrefetchFlag = new System.Windows.Forms.CheckBox();
            this.chkSupportWaitFront = new System.Windows.Forms.CheckBox();
            this.chkDummyCheckFlag = new System.Windows.Forms.CheckBox();
            this.chkPutReadyFlag = new System.Windows.Forms.CheckBox();
            this.chkGetReadyFlag = new System.Windows.Forms.CheckBox();
            this.chkRecipeCheckFlag = new System.Windows.Forms.CheckBox();
            this.chkIsMultiSlot = new System.Windows.Forms.CheckBox();
            this.chkRTCReworkFlag = new System.Windows.Forms.CheckBox();
            this.explanation = new System.Windows.Forms.Label();
            this.flpFloatArea3 = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlStageReportTrxName = new System.Windows.Forms.Panel();
            this.txtStageReportTrxName = new System.Windows.Forms.TextBox();
            this.lblStageReportTrxName = new System.Windows.Forms.Label();
            this.pnlStageJobDataTrxName = new System.Windows.Forms.Panel();
            this.txtStageJobDataTrxName = new System.Windows.Forms.TextBox();
            this.lblStageJobDataTrxName = new System.Windows.Forms.Label();
            this.pnlUpstreamPathTrxName = new System.Windows.Forms.Panel();
            this.txtUpstreamPathTrxName = new System.Windows.Forms.TextBox();
            this.lblUpstreamPathTrxName = new System.Windows.Forms.Label();
            this.pnlUpstreamSendPathTrxName = new System.Windows.Forms.Panel();
            this.txtUpstreamSendPathTrxName = new System.Windows.Forms.TextBox();
            this.lblUpstreamSendPathTrxName = new System.Windows.Forms.Label();
            this.pnlDownstreamPathTrxName = new System.Windows.Forms.Panel();
            this.txtDownstreamPathTrxName = new System.Windows.Forms.TextBox();
            this.lblDownstreamPathTrxName = new System.Windows.Forms.Label();
            this.pnlDownstreamReceivePathTrxName = new System.Windows.Forms.Panel();
            this.txtDownstreamReceivePathTrxName = new System.Windows.Forms.TextBox();
            this.lblDownstreamReceivePathTrxName = new System.Windows.Forms.Label();
            this.txtStageID = new System.Windows.Forms.TextBox();
            this.txtStageName = new System.Windows.Forms.TextBox();
            this.cboNode = new System.Windows.Forms.ComboBox();
            this.cboStageType = new System.Windows.Forms.ComboBox();
            this.cboRobotName = new System.Windows.Forms.ComboBox();
            this.lblStageID = new System.Windows.Forms.Label();
            this.lblRobotName = new System.Windows.Forms.Label();
            this.lblStageName = new System.Windows.Forms.Label();
            this.lblPriority = new System.Windows.Forms.Label();
            this.lblNode = new System.Windows.Forms.Label();
            this.lblStageType = new System.Windows.Forms.Label();
            this.lblSlotMaxCount = new System.Windows.Forms.Label();
            this.txtPriority = new System.Windows.Forms.TextBox();
            this.lblStageIDByNode = new System.Windows.Forms.Label();
            this.txtSlotMaxCount = new System.Windows.Forms.TextBox();
            this.txtStageIDByNode = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.flpButton.SuspendLayout();
            this.pnlAdd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddList)).BeginInit();
            this.pnlButton.SuspendLayout();
            this.grbData.SuspendLayout();
            this.pnlEdit.SuspendLayout();
            this.pnlTrackDataSeqList.SuspendLayout();
            this.flpFloatArea1.SuspendLayout();
            this.pnlCassetteType.SuspendLayout();
            this.pnlSlotFetchSeq.SuspendLayout();
            this.pnlSlotStoreSeq.SuspendLayout();
            this.pnlArmUsePriority.SuspendLayout();
            this.pnlUseSpecificArm.SuspendLayout();
            this.flpFloatArea2.SuspendLayout();
            this.flpFloatArea3.SuspendLayout();
            this.pnlStageReportTrxName.SuspendLayout();
            this.pnlStageJobDataTrxName.SuspendLayout();
            this.pnlUpstreamPathTrxName.SuspendLayout();
            this.pnlUpstreamSendPathTrxName.SuspendLayout();
            this.pnlDownstreamPathTrxName.SuspendLayout();
            this.pnlDownstreamReceivePathTrxName.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(765, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(845, 762);
            // 
            // btnAddOK
            // 
            this.btnAddOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddOK.Location = new System.Drawing.Point(327, 1);
            this.btnAddOK.Name = "btnAddOK";
            this.btnAddOK.Size = new System.Drawing.Size(90, 30);
            this.btnAddOK.TabIndex = 0;
            this.btnAddOK.Tag = "ADD";
            this.btnAddOK.Text = "ADD";
            this.btnAddOK.UseVisualStyleBackColor = true;
            this.btnAddOK.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnCancelClose
            // 
            this.btnCancelClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancelClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelClose.Location = new System.Drawing.Point(419, 3);
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
            this.btnOKClose.Location = new System.Drawing.Point(327, 2);
            this.btnOKClose.Name = "btnOKClose";
            this.btnOKClose.Size = new System.Drawing.Size(90, 30);
            this.btnOKClose.TabIndex = 1;
            this.btnOKClose.Tag = "OK";
            this.btnOKClose.Text = "OK";
            this.btnOKClose.UseVisualStyleBackColor = true;
            this.btnOKClose.Click += new System.EventHandler(this.btn_Click);
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
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 532F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Size = new System.Drawing.Size(845, 731);
            this.tlpBase.TabIndex = 5;
            // 
            // flpButton
            // 
            this.flpButton.Controls.Add(this.pnlAdd);
            this.flpButton.Controls.Add(this.dgvAddList);
            this.flpButton.Controls.Add(this.pnlButton);
            this.flpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButton.Location = new System.Drawing.Point(3, 535);
            this.flpButton.Name = "flpButton";
            this.flpButton.Size = new System.Drawing.Size(839, 193);
            this.flpButton.TabIndex = 0;
            // 
            // pnlAdd
            // 
            this.pnlAdd.Controls.Add(this.btnClear);
            this.pnlAdd.Controls.Add(this.btnAddOK);
            this.pnlAdd.Location = new System.Drawing.Point(3, 3);
            this.pnlAdd.Name = "pnlAdd";
            this.pnlAdd.Size = new System.Drawing.Size(833, 33);
            this.pnlAdd.TabIndex = 0;
            // 
            // btnClear
            // 
            this.btnClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClear.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(419, 1);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(90, 30);
            this.btnClear.TabIndex = 1;
            this.btnClear.Tag = "Clear";
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btn_Click);
            // 
            // dgvAddList
            // 
            this.dgvAddList.AllowUserToAddRows = false;
            this.dgvAddList.AllowUserToDeleteRows = false;
            this.dgvAddList.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvAddList.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvAddList.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAddList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvAddList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAddList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colServerName,
            this.colRobotName,
            this.colStageID,
            this.colStageName,
            this.colLineID,
            this.colLocalNo,
            this.colStageIDByNode,
            this.colStageType,
            this.colPriority,
            this.colStageReportTrxName,
            this.colStageJobDataTrxName,
            this.colIsMultiSlot,
            this.colSlotMaxCount,
            this.colRecipeCheckFlag,
            this.colDummyCheckFlag,
            this.colGetReadyFlag,
            this.colPutReadyFlag,
            this.colPrefetchFlag,
            this.colRTCReworkFlag,
            this.colSupportWaitFront,
            this.colUpstreamPathTrxName,
            this.colUpstreamSendPathTrxName,
            this.colDownstreamPathTrxName,
            this.colDownstreamReceivePathTrxName,
            this.colTrackDataSeqList,
            this.colCassetteType,
            this.colRemark,
            this.colStageEnabled,
            this.colSlotFetchSeq,
            this.colSlotStoreSeq,
            this.colExchangeType,
            this.colEQRobotIfType});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvAddList.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvAddList.Location = new System.Drawing.Point(3, 42);
            this.dgvAddList.MultiSelect = false;
            this.dgvAddList.Name = "dgvAddList";
            this.dgvAddList.ReadOnly = true;
            this.dgvAddList.RowHeadersVisible = false;
            this.dgvAddList.RowTemplate.Height = 24;
            this.dgvAddList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAddList.Size = new System.Drawing.Size(832, 108);
            this.dgvAddList.TabIndex = 13;
            this.dgvAddList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAddList_CellClick);
            // 
            // colServerName
            // 
            this.colServerName.DataPropertyName = "SERVERNAME";
            this.colServerName.HeaderText = "ServerName";
            this.colServerName.Name = "colServerName";
            this.colServerName.ReadOnly = true;
            this.colServerName.Visible = false;
            // 
            // colRobotName
            // 
            this.colRobotName.DataPropertyName = "ROBOTNAME";
            this.colRobotName.HeaderText = "Robot Name";
            this.colRobotName.MinimumWidth = 115;
            this.colRobotName.Name = "colRobotName";
            this.colRobotName.ReadOnly = true;
            this.colRobotName.Width = 115;
            // 
            // colStageID
            // 
            this.colStageID.DataPropertyName = "STAGEID";
            this.colStageID.HeaderText = "Stage ID";
            this.colStageID.Name = "colStageID";
            this.colStageID.ReadOnly = true;
            // 
            // colStageName
            // 
            this.colStageName.DataPropertyName = "STAGENAME";
            this.colStageName.HeaderText = "Stage Name";
            this.colStageName.Name = "colStageName";
            this.colStageName.ReadOnly = true;
            this.colStageName.Width = 130;
            // 
            // colLineID
            // 
            this.colLineID.DataPropertyName = "LINEID";
            this.colLineID.HeaderText = "LineID";
            this.colLineID.Name = "colLineID";
            this.colLineID.ReadOnly = true;
            this.colLineID.Visible = false;
            // 
            // colLocalNo
            // 
            this.colLocalNo.DataPropertyName = "NODENO";
            this.colLocalNo.HeaderText = "Local No";
            this.colLocalNo.Name = "colLocalNo";
            this.colLocalNo.ReadOnly = true;
            this.colLocalNo.Width = 95;
            // 
            // colStageIDByNode
            // 
            this.colStageIDByNode.DataPropertyName = "STAGEIDBYNODE";
            this.colStageIDByNode.HeaderText = "Stage ID By Local";
            this.colStageIDByNode.Name = "colStageIDByNode";
            this.colStageIDByNode.ReadOnly = true;
            this.colStageIDByNode.Width = 200;
            // 
            // colStageType
            // 
            this.colStageType.DataPropertyName = "STAGETYPE";
            this.colStageType.HeaderText = "Stage Type";
            this.colStageType.Name = "colStageType";
            this.colStageType.ReadOnly = true;
            this.colStageType.Width = 150;
            // 
            // colPriority
            // 
            this.colPriority.DataPropertyName = "PRIORITY";
            this.colPriority.HeaderText = "Priority";
            this.colPriority.Name = "colPriority";
            this.colPriority.ReadOnly = true;
            // 
            // colStageReportTrxName
            // 
            this.colStageReportTrxName.DataPropertyName = "STAGEREPORTTRXNAME";
            this.colStageReportTrxName.HeaderText = "ReportTrxName";
            this.colStageReportTrxName.Name = "colStageReportTrxName";
            this.colStageReportTrxName.ReadOnly = true;
            this.colStageReportTrxName.Width = 200;
            // 
            // colStageJobDataTrxName
            // 
            this.colStageJobDataTrxName.DataPropertyName = "STAGEJOBDATATRXNAME";
            this.colStageJobDataTrxName.HeaderText = "JobDataTrxName";
            this.colStageJobDataTrxName.Name = "colStageJobDataTrxName";
            this.colStageJobDataTrxName.ReadOnly = true;
            this.colStageJobDataTrxName.Width = 200;
            // 
            // colIsMultiSlot
            // 
            this.colIsMultiSlot.DataPropertyName = "ISMULTISLOT";
            this.colIsMultiSlot.FalseValue = "N";
            this.colIsMultiSlot.HeaderText = "Is Multi Slot";
            this.colIsMultiSlot.Name = "colIsMultiSlot";
            this.colIsMultiSlot.ReadOnly = true;
            this.colIsMultiSlot.TrueValue = "Y";
            // 
            // colSlotMaxCount
            // 
            this.colSlotMaxCount.DataPropertyName = "SLOTMAXCOUNT";
            this.colSlotMaxCount.HeaderText = "Slot Max Count";
            this.colSlotMaxCount.Name = "colSlotMaxCount";
            this.colSlotMaxCount.ReadOnly = true;
            this.colSlotMaxCount.Width = 200;
            // 
            // colRecipeCheckFlag
            // 
            this.colRecipeCheckFlag.DataPropertyName = "RECIPECHENCKFLAG";
            this.colRecipeCheckFlag.FalseValue = "N";
            this.colRecipeCheckFlag.HeaderText = "RecipeCheck";
            this.colRecipeCheckFlag.Name = "colRecipeCheckFlag";
            this.colRecipeCheckFlag.ReadOnly = true;
            this.colRecipeCheckFlag.TrueValue = "Y";
            // 
            // colDummyCheckFlag
            // 
            this.colDummyCheckFlag.DataPropertyName = "DUMMYCHECKFLAG";
            this.colDummyCheckFlag.FalseValue = "N";
            this.colDummyCheckFlag.HeaderText = "DummyCheck";
            this.colDummyCheckFlag.Name = "colDummyCheckFlag";
            this.colDummyCheckFlag.ReadOnly = true;
            this.colDummyCheckFlag.TrueValue = "Y";
            // 
            // colGetReadyFlag
            // 
            this.colGetReadyFlag.DataPropertyName = "GETREADYFLAG";
            this.colGetReadyFlag.FalseValue = "N";
            this.colGetReadyFlag.HeaderText = "Get Ready Flag";
            this.colGetReadyFlag.Name = "colGetReadyFlag";
            this.colGetReadyFlag.ReadOnly = true;
            this.colGetReadyFlag.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colGetReadyFlag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colGetReadyFlag.TrueValue = "Y";
            this.colGetReadyFlag.Width = 130;
            // 
            // colPutReadyFlag
            // 
            this.colPutReadyFlag.DataPropertyName = "PUTREADYFLAG";
            this.colPutReadyFlag.FalseValue = "N";
            this.colPutReadyFlag.HeaderText = "Put Ready Flag";
            this.colPutReadyFlag.Name = "colPutReadyFlag";
            this.colPutReadyFlag.ReadOnly = true;
            this.colPutReadyFlag.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colPutReadyFlag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colPutReadyFlag.TrueValue = "Y";
            this.colPutReadyFlag.Width = 150;
            // 
            // colPrefetchFlag
            // 
            this.colPrefetchFlag.DataPropertyName = "PREFETCHFLAG";
            this.colPrefetchFlag.FalseValue = "N";
            this.colPrefetchFlag.HeaderText = "Prefetch Flag";
            this.colPrefetchFlag.Name = "colPrefetchFlag";
            this.colPrefetchFlag.ReadOnly = true;
            this.colPrefetchFlag.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colPrefetchFlag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colPrefetchFlag.TrueValue = "Y";
            this.colPrefetchFlag.Width = 150;
            // 
            // colRTCReworkFlag
            // 
            this.colRTCReworkFlag.DataPropertyName = "RTCREWORKFLAG";
            this.colRTCReworkFlag.HeaderText = "RTCReworkFlag";
            this.colRTCReworkFlag.Name = "colRTCReworkFlag";
            this.colRTCReworkFlag.ReadOnly = true;
            // 
            // colSupportWaitFront
            // 
            this.colSupportWaitFront.DataPropertyName = "WAITFRONTFLAG";
            this.colSupportWaitFront.FalseValue = "N";
            this.colSupportWaitFront.HeaderText = "Support Wait Front Flag";
            this.colSupportWaitFront.Name = "colSupportWaitFront";
            this.colSupportWaitFront.ReadOnly = true;
            this.colSupportWaitFront.TrueValue = "Y";
            this.colSupportWaitFront.Width = 200;
            // 
            // colUpstreamPathTrxName
            // 
            this.colUpstreamPathTrxName.DataPropertyName = "UPSTREAMPATHTRXNAME";
            this.colUpstreamPathTrxName.HeaderText = "Upstream Path Transaction Name";
            this.colUpstreamPathTrxName.Name = "colUpstreamPathTrxName";
            this.colUpstreamPathTrxName.ReadOnly = true;
            this.colUpstreamPathTrxName.Width = 300;
            // 
            // colUpstreamSendPathTrxName
            // 
            this.colUpstreamSendPathTrxName.DataPropertyName = "UPSTREAMJOBDATAPATHTRXNAME";
            this.colUpstreamSendPathTrxName.HeaderText = "Upstream Send Jobdata Path ";
            this.colUpstreamSendPathTrxName.Name = "colUpstreamSendPathTrxName";
            this.colUpstreamSendPathTrxName.ReadOnly = true;
            this.colUpstreamSendPathTrxName.Width = 300;
            // 
            // colDownstreamPathTrxName
            // 
            this.colDownstreamPathTrxName.DataPropertyName = "DOWNSTREAMPATHTRXNAME";
            this.colDownstreamPathTrxName.HeaderText = "Downstream Path Transaction Name";
            this.colDownstreamPathTrxName.Name = "colDownstreamPathTrxName";
            this.colDownstreamPathTrxName.ReadOnly = true;
            this.colDownstreamPathTrxName.Width = 300;
            // 
            // colDownstreamReceivePathTrxName
            // 
            this.colDownstreamReceivePathTrxName.DataPropertyName = "DOWNSTREAMJOBDATAPATHTRXNAME";
            this.colDownstreamReceivePathTrxName.HeaderText = "Downstream Receive Jobdata Path ";
            this.colDownstreamReceivePathTrxName.Name = "colDownstreamReceivePathTrxName";
            this.colDownstreamReceivePathTrxName.ReadOnly = true;
            this.colDownstreamReceivePathTrxName.Width = 300;
            // 
            // colTrackDataSeqList
            // 
            this.colTrackDataSeqList.DataPropertyName = "TRACKDATASEQLIST";
            this.colTrackDataSeqList.HeaderText = "TrackData Seq List";
            this.colTrackDataSeqList.Name = "colTrackDataSeqList";
            this.colTrackDataSeqList.ReadOnly = true;
            this.colTrackDataSeqList.Width = 200;
            // 
            // colCassetteType
            // 
            this.colCassetteType.DataPropertyName = "CASSETTETYPE";
            this.colCassetteType.HeaderText = "Cassette Type";
            this.colCassetteType.Name = "colCassetteType";
            this.colCassetteType.ReadOnly = true;
            this.colCassetteType.Width = 150;
            // 
            // colRemark
            // 
            this.colRemark.DataPropertyName = "REMARKS";
            this.colRemark.HeaderText = "Remark";
            this.colRemark.Name = "colRemark";
            this.colRemark.ReadOnly = true;
            // 
            // colStageEnabled
            // 
            this.colStageEnabled.DataPropertyName = "ISENABLED";
            this.colStageEnabled.FalseValue = "N";
            this.colStageEnabled.HeaderText = "Stage Enabled";
            this.colStageEnabled.Name = "colStageEnabled";
            this.colStageEnabled.ReadOnly = true;
            this.colStageEnabled.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colStageEnabled.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colStageEnabled.TrueValue = "Y";
            this.colStageEnabled.Width = 150;
            // 
            // colSlotFetchSeq
            // 
            this.colSlotFetchSeq.DataPropertyName = "SLOTFETCHSEQ";
            this.colSlotFetchSeq.HeaderText = "Slot Fetch Sequence";
            this.colSlotFetchSeq.Name = "colSlotFetchSeq";
            this.colSlotFetchSeq.ReadOnly = true;
            this.colSlotFetchSeq.Width = 200;
            // 
            // colSlotStoreSeq
            // 
            this.colSlotStoreSeq.DataPropertyName = "SLOTSTORESEQ";
            this.colSlotStoreSeq.HeaderText = "Slot Store Sequence";
            this.colSlotStoreSeq.Name = "colSlotStoreSeq";
            this.colSlotStoreSeq.ReadOnly = true;
            this.colSlotStoreSeq.Width = 200;
            // 
            // colExchangeType
            // 
            this.colExchangeType.DataPropertyName = "EXCHANGETYPE";
            this.colExchangeType.HeaderText = "Exchange Type";
            this.colExchangeType.Name = "colExchangeType";
            this.colExchangeType.ReadOnly = true;
            this.colExchangeType.Width = 150;
            // 
            // colEQRobotIfType
            // 
            this.colEQRobotIfType.DataPropertyName = "EQROBOTIFTYPE";
            this.colEQRobotIfType.HeaderText = "EQRobotifType";
            this.colEQRobotIfType.Name = "colEQRobotIfType";
            this.colEQRobotIfType.ReadOnly = true;
            this.colEQRobotIfType.Width = 150;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnCancelClose);
            this.pnlButton.Controls.Add(this.btnOKClose);
            this.pnlButton.Location = new System.Drawing.Point(3, 156);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(833, 36);
            this.pnlButton.TabIndex = 77;
            // 
            // grbData
            // 
            this.grbData.Controls.Add(this.pnlEdit);
            this.grbData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbData.Location = new System.Drawing.Point(3, 3);
            this.grbData.Name = "grbData";
            this.grbData.Size = new System.Drawing.Size(839, 526);
            this.grbData.TabIndex = 40;
            this.grbData.TabStop = false;
            // 
            // pnlEdit
            // 
            this.pnlEdit.Controls.Add(this.cboEQRobotIfType);
            this.pnlEdit.Controls.Add(this.lblEQRobotIfType);
            this.pnlEdit.Controls.Add(this.pnlTrackDataSeqList);
            this.pnlEdit.Controls.Add(this.cboExchangeType);
            this.pnlEdit.Controls.Add(this.lblExchangeType);
            this.pnlEdit.Controls.Add(this.flpFloatArea1);
            this.pnlEdit.Controls.Add(this.explanation);
            this.pnlEdit.Controls.Add(this.flpFloatArea3);
            this.pnlEdit.Controls.Add(this.txtStageID);
            this.pnlEdit.Controls.Add(this.txtStageName);
            this.pnlEdit.Controls.Add(this.cboNode);
            this.pnlEdit.Controls.Add(this.cboStageType);
            this.pnlEdit.Controls.Add(this.cboRobotName);
            this.pnlEdit.Controls.Add(this.lblStageID);
            this.pnlEdit.Controls.Add(this.lblRobotName);
            this.pnlEdit.Controls.Add(this.lblStageName);
            this.pnlEdit.Controls.Add(this.lblPriority);
            this.pnlEdit.Controls.Add(this.lblNode);
            this.pnlEdit.Controls.Add(this.lblStageType);
            this.pnlEdit.Controls.Add(this.lblSlotMaxCount);
            this.pnlEdit.Controls.Add(this.txtPriority);
            this.pnlEdit.Controls.Add(this.lblStageIDByNode);
            this.pnlEdit.Controls.Add(this.txtSlotMaxCount);
            this.pnlEdit.Controls.Add(this.txtStageIDByNode);
            this.pnlEdit.Location = new System.Drawing.Point(6, 15);
            this.pnlEdit.Name = "pnlEdit";
            this.pnlEdit.Size = new System.Drawing.Size(833, 505);
            this.pnlEdit.TabIndex = 147;
            // 
            // cboEQRobotIfType
            // 
            this.cboEQRobotIfType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEQRobotIfType.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboEQRobotIfType.FormattingEnabled = true;
            this.cboEQRobotIfType.Location = new System.Drawing.Point(233, 64);
            this.cboEQRobotIfType.Name = "cboEQRobotIfType";
            this.cboEQRobotIfType.Size = new System.Drawing.Size(200, 34);
            this.cboEQRobotIfType.TabIndex = 169;
            // 
            // lblEQRobotIfType
            // 
            this.lblEQRobotIfType.BackColor = System.Drawing.Color.Black;
            this.lblEQRobotIfType.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblEQRobotIfType.ForeColor = System.Drawing.Color.White;
            this.lblEQRobotIfType.Location = new System.Drawing.Point(3, 66);
            this.lblEQRobotIfType.Name = "lblEQRobotIfType";
            this.lblEQRobotIfType.Size = new System.Drawing.Size(230, 25);
            this.lblEQRobotIfType.TabIndex = 168;
            this.lblEQRobotIfType.Text = "EQRobotIfType";
            this.lblEQRobotIfType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlTrackDataSeqList
            // 
            this.pnlTrackDataSeqList.Controls.Add(this.txtTrackDataSeqList);
            this.pnlTrackDataSeqList.Controls.Add(this.lblTrackDataSeqList);
            this.pnlTrackDataSeqList.Location = new System.Drawing.Point(1, 283);
            this.pnlTrackDataSeqList.Margin = new System.Windows.Forms.Padding(0);
            this.pnlTrackDataSeqList.Name = "pnlTrackDataSeqList";
            this.pnlTrackDataSeqList.Size = new System.Drawing.Size(434, 30);
            this.pnlTrackDataSeqList.TabIndex = 165;
            // 
            // txtTrackDataSeqList
            // 
            this.txtTrackDataSeqList.BackColor = System.Drawing.Color.Cyan;
            this.txtTrackDataSeqList.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTrackDataSeqList.Location = new System.Drawing.Point(232, 1);
            this.txtTrackDataSeqList.Name = "txtTrackDataSeqList";
            this.txtTrackDataSeqList.ReadOnly = true;
            this.txtTrackDataSeqList.Size = new System.Drawing.Size(201, 33);
            this.txtTrackDataSeqList.TabIndex = 165;
            this.txtTrackDataSeqList.DoubleClick += new System.EventHandler(this.txtTrackDataSeqList_DoubleClick);
            // 
            // lblTrackDataSeqList
            // 
            this.lblTrackDataSeqList.BackColor = System.Drawing.Color.Black;
            this.lblTrackDataSeqList.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblTrackDataSeqList.ForeColor = System.Drawing.Color.White;
            this.lblTrackDataSeqList.Location = new System.Drawing.Point(2, 2);
            this.lblTrackDataSeqList.Name = "lblTrackDataSeqList";
            this.lblTrackDataSeqList.Size = new System.Drawing.Size(230, 26);
            this.lblTrackDataSeqList.TabIndex = 166;
            this.lblTrackDataSeqList.Text = "Track Data Seq List";
            this.lblTrackDataSeqList.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboExchangeType
            // 
            this.cboExchangeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboExchangeType.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboExchangeType.FormattingEnabled = true;
            this.cboExchangeType.Location = new System.Drawing.Point(233, 251);
            this.cboExchangeType.Name = "cboExchangeType";
            this.cboExchangeType.Size = new System.Drawing.Size(200, 34);
            this.cboExchangeType.TabIndex = 159;
            this.cboExchangeType.SelectionChangeCommitted += new System.EventHandler(this.cboExchangeType_SelectionChangeCommitted);
            // 
            // lblExchangeType
            // 
            this.lblExchangeType.BackColor = System.Drawing.Color.Black;
            this.lblExchangeType.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblExchangeType.ForeColor = System.Drawing.Color.White;
            this.lblExchangeType.Location = new System.Drawing.Point(3, 252);
            this.lblExchangeType.Name = "lblExchangeType";
            this.lblExchangeType.Size = new System.Drawing.Size(230, 25);
            this.lblExchangeType.TabIndex = 156;
            this.lblExchangeType.Text = "Exchange Type";
            this.lblExchangeType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flpFloatArea1
            // 
            this.flpFloatArea1.Controls.Add(this.pnlCassetteType);
            this.flpFloatArea1.Controls.Add(this.pnlSlotFetchSeq);
            this.flpFloatArea1.Controls.Add(this.pnlSlotStoreSeq);
            this.flpFloatArea1.Controls.Add(this.pnlArmUsePriority);
            this.flpFloatArea1.Controls.Add(this.pnlUseSpecificArm);
            this.flpFloatArea1.Controls.Add(this.flpFloatArea2);
            this.flpFloatArea1.Location = new System.Drawing.Point(437, 0);
            this.flpFloatArea1.Margin = new System.Windows.Forms.Padding(0);
            this.flpFloatArea1.Name = "flpFloatArea1";
            this.flpFloatArea1.Size = new System.Drawing.Size(396, 313);
            this.flpFloatArea1.TabIndex = 152;
            // 
            // pnlCassetteType
            // 
            this.pnlCassetteType.Controls.Add(this.cboCassetteType);
            this.pnlCassetteType.Controls.Add(this.lblStageCstType);
            this.pnlCassetteType.Location = new System.Drawing.Point(0, 0);
            this.pnlCassetteType.Margin = new System.Windows.Forms.Padding(0);
            this.pnlCassetteType.Name = "pnlCassetteType";
            this.pnlCassetteType.Size = new System.Drawing.Size(397, 31);
            this.pnlCassetteType.TabIndex = 166;
            // 
            // cboCassetteType
            // 
            this.cboCassetteType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCassetteType.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboCassetteType.FormattingEnabled = true;
            this.cboCassetteType.Location = new System.Drawing.Point(186, 2);
            this.cboCassetteType.Name = "cboCassetteType";
            this.cboCassetteType.Size = new System.Drawing.Size(200, 34);
            this.cboCassetteType.TabIndex = 142;
            // 
            // lblStageCstType
            // 
            this.lblStageCstType.BackColor = System.Drawing.Color.Black;
            this.lblStageCstType.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStageCstType.ForeColor = System.Drawing.Color.White;
            this.lblStageCstType.Location = new System.Drawing.Point(6, 4);
            this.lblStageCstType.Name = "lblStageCstType";
            this.lblStageCstType.Size = new System.Drawing.Size(184, 25);
            this.lblStageCstType.TabIndex = 143;
            this.lblStageCstType.Text = "Cassette Type";
            this.lblStageCstType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlSlotFetchSeq
            // 
            this.pnlSlotFetchSeq.Controls.Add(this.cboSlotFetchSeq);
            this.pnlSlotFetchSeq.Controls.Add(this.lblSlotFetchSeq);
            this.pnlSlotFetchSeq.Location = new System.Drawing.Point(0, 31);
            this.pnlSlotFetchSeq.Margin = new System.Windows.Forms.Padding(0);
            this.pnlSlotFetchSeq.Name = "pnlSlotFetchSeq";
            this.pnlSlotFetchSeq.Size = new System.Drawing.Size(394, 31);
            this.pnlSlotFetchSeq.TabIndex = 1;
            // 
            // cboSlotFetchSeq
            // 
            this.cboSlotFetchSeq.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSlotFetchSeq.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboSlotFetchSeq.FormattingEnabled = true;
            this.cboSlotFetchSeq.Location = new System.Drawing.Point(186, 2);
            this.cboSlotFetchSeq.Name = "cboSlotFetchSeq";
            this.cboSlotFetchSeq.Size = new System.Drawing.Size(200, 34);
            this.cboSlotFetchSeq.TabIndex = 150;
            // 
            // lblSlotFetchSeq
            // 
            this.lblSlotFetchSeq.BackColor = System.Drawing.Color.Black;
            this.lblSlotFetchSeq.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblSlotFetchSeq.ForeColor = System.Drawing.Color.White;
            this.lblSlotFetchSeq.Location = new System.Drawing.Point(6, 4);
            this.lblSlotFetchSeq.Name = "lblSlotFetchSeq";
            this.lblSlotFetchSeq.Size = new System.Drawing.Size(180, 25);
            this.lblSlotFetchSeq.TabIndex = 149;
            this.lblSlotFetchSeq.Text = "Slot Fetch Sequence";
            this.lblSlotFetchSeq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlSlotStoreSeq
            // 
            this.pnlSlotStoreSeq.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlSlotStoreSeq.Controls.Add(this.cboSlotStoreSeq);
            this.pnlSlotStoreSeq.Controls.Add(this.lblSlotStoreSeq);
            this.pnlSlotStoreSeq.Location = new System.Drawing.Point(0, 62);
            this.pnlSlotStoreSeq.Margin = new System.Windows.Forms.Padding(0);
            this.pnlSlotStoreSeq.Name = "pnlSlotStoreSeq";
            this.pnlSlotStoreSeq.Size = new System.Drawing.Size(394, 31);
            this.pnlSlotStoreSeq.TabIndex = 2;
            // 
            // cboSlotStoreSeq
            // 
            this.cboSlotStoreSeq.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSlotStoreSeq.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboSlotStoreSeq.FormattingEnabled = true;
            this.cboSlotStoreSeq.Location = new System.Drawing.Point(186, 2);
            this.cboSlotStoreSeq.Name = "cboSlotStoreSeq";
            this.cboSlotStoreSeq.Size = new System.Drawing.Size(200, 34);
            this.cboSlotStoreSeq.TabIndex = 151;
            // 
            // lblSlotStoreSeq
            // 
            this.lblSlotStoreSeq.BackColor = System.Drawing.Color.Black;
            this.lblSlotStoreSeq.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblSlotStoreSeq.ForeColor = System.Drawing.Color.White;
            this.lblSlotStoreSeq.Location = new System.Drawing.Point(6, 4);
            this.lblSlotStoreSeq.Name = "lblSlotStoreSeq";
            this.lblSlotStoreSeq.Size = new System.Drawing.Size(180, 25);
            this.lblSlotStoreSeq.TabIndex = 150;
            this.lblSlotStoreSeq.Text = "Slot Stroe Sequence";
            this.lblSlotStoreSeq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlArmUsePriority
            // 
            this.pnlArmUsePriority.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlArmUsePriority.Controls.Add(this.cboArmUsePriority);
            this.pnlArmUsePriority.Controls.Add(this.lblArmUsePriority);
            this.pnlArmUsePriority.Location = new System.Drawing.Point(0, 93);
            this.pnlArmUsePriority.Margin = new System.Windows.Forms.Padding(0);
            this.pnlArmUsePriority.Name = "pnlArmUsePriority";
            this.pnlArmUsePriority.Size = new System.Drawing.Size(394, 31);
            this.pnlArmUsePriority.TabIndex = 3;
            // 
            // cboArmUsePriority
            // 
            this.cboArmUsePriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboArmUsePriority.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboArmUsePriority.FormattingEnabled = true;
            this.cboArmUsePriority.Location = new System.Drawing.Point(186, 2);
            this.cboArmUsePriority.Name = "cboArmUsePriority";
            this.cboArmUsePriority.Size = new System.Drawing.Size(200, 34);
            this.cboArmUsePriority.TabIndex = 150;
            // 
            // lblArmUsePriority
            // 
            this.lblArmUsePriority.BackColor = System.Drawing.Color.Black;
            this.lblArmUsePriority.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblArmUsePriority.ForeColor = System.Drawing.Color.White;
            this.lblArmUsePriority.Location = new System.Drawing.Point(6, 4);
            this.lblArmUsePriority.Name = "lblArmUsePriority";
            this.lblArmUsePriority.Size = new System.Drawing.Size(180, 25);
            this.lblArmUsePriority.TabIndex = 149;
            this.lblArmUsePriority.Text = "Arm Use Priority";
            this.lblArmUsePriority.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlUseSpecificArm
            // 
            this.pnlUseSpecificArm.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlUseSpecificArm.Controls.Add(this.cboUseSpecificArm);
            this.pnlUseSpecificArm.Controls.Add(this.lblUseSpecificArm);
            this.pnlUseSpecificArm.Location = new System.Drawing.Point(0, 124);
            this.pnlUseSpecificArm.Margin = new System.Windows.Forms.Padding(0);
            this.pnlUseSpecificArm.Name = "pnlUseSpecificArm";
            this.pnlUseSpecificArm.Size = new System.Drawing.Size(394, 31);
            this.pnlUseSpecificArm.TabIndex = 4;
            // 
            // cboUseSpecificArm
            // 
            this.cboUseSpecificArm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboUseSpecificArm.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboUseSpecificArm.FormattingEnabled = true;
            this.cboUseSpecificArm.Location = new System.Drawing.Point(186, 2);
            this.cboUseSpecificArm.Name = "cboUseSpecificArm";
            this.cboUseSpecificArm.Size = new System.Drawing.Size(200, 34);
            this.cboUseSpecificArm.TabIndex = 151;
            // 
            // lblUseSpecificArm
            // 
            this.lblUseSpecificArm.BackColor = System.Drawing.Color.Black;
            this.lblUseSpecificArm.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblUseSpecificArm.ForeColor = System.Drawing.Color.White;
            this.lblUseSpecificArm.Location = new System.Drawing.Point(6, 4);
            this.lblUseSpecificArm.Name = "lblUseSpecificArm";
            this.lblUseSpecificArm.Size = new System.Drawing.Size(180, 25);
            this.lblUseSpecificArm.TabIndex = 150;
            this.lblUseSpecificArm.Text = "Use Specific Arm";
            this.lblUseSpecificArm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flpFloatArea2
            // 
            this.flpFloatArea2.Controls.Add(this.chkStageEnabled);
            this.flpFloatArea2.Controls.Add(this.chkPrefetchFlag);
            this.flpFloatArea2.Controls.Add(this.chkSupportWaitFront);
            this.flpFloatArea2.Controls.Add(this.chkDummyCheckFlag);
            this.flpFloatArea2.Controls.Add(this.chkPutReadyFlag);
            this.flpFloatArea2.Controls.Add(this.chkGetReadyFlag);
            this.flpFloatArea2.Controls.Add(this.chkRecipeCheckFlag);
            this.flpFloatArea2.Controls.Add(this.chkIsMultiSlot);
            this.flpFloatArea2.Controls.Add(this.chkRTCReworkFlag);
            this.flpFloatArea2.Location = new System.Drawing.Point(0, 155);
            this.flpFloatArea2.Margin = new System.Windows.Forms.Padding(0);
            this.flpFloatArea2.Name = "flpFloatArea2";
            this.flpFloatArea2.Size = new System.Drawing.Size(387, 156);
            this.flpFloatArea2.TabIndex = 167;
            // 
            // chkStageEnabled
            // 
            this.chkStageEnabled.Checked = true;
            this.chkStageEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStageEnabled.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkStageEnabled.Location = new System.Drawing.Point(3, 3);
            this.chkStageEnabled.Name = "chkStageEnabled";
            this.chkStageEnabled.Size = new System.Drawing.Size(180, 25);
            this.chkStageEnabled.TabIndex = 155;
            this.chkStageEnabled.Text = "Stage Enabled ?";
            this.chkStageEnabled.UseVisualStyleBackColor = true;
            // 
            // chkPrefetchFlag
            // 
            this.chkPrefetchFlag.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkPrefetchFlag.Location = new System.Drawing.Point(189, 3);
            this.chkPrefetchFlag.Name = "chkPrefetchFlag";
            this.chkPrefetchFlag.Size = new System.Drawing.Size(180, 25);
            this.chkPrefetchFlag.TabIndex = 149;
            this.chkPrefetchFlag.Text = "Enabled Pre-fetch ?";
            this.chkPrefetchFlag.UseVisualStyleBackColor = true;
            // 
            // chkSupportWaitFront
            // 
            this.chkSupportWaitFront.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkSupportWaitFront.Location = new System.Drawing.Point(3, 34);
            this.chkSupportWaitFront.Name = "chkSupportWaitFront";
            this.chkSupportWaitFront.Size = new System.Drawing.Size(180, 25);
            this.chkSupportWaitFront.TabIndex = 153;
            this.chkSupportWaitFront.Text = "Support Wait Front ?";
            this.chkSupportWaitFront.UseVisualStyleBackColor = true;
            // 
            // chkDummyCheckFlag
            // 
            this.chkDummyCheckFlag.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkDummyCheckFlag.Location = new System.Drawing.Point(189, 34);
            this.chkDummyCheckFlag.Name = "chkDummyCheckFlag";
            this.chkDummyCheckFlag.Size = new System.Drawing.Size(180, 25);
            this.chkDummyCheckFlag.TabIndex = 148;
            this.chkDummyCheckFlag.Text = "Dummy Check ?";
            this.chkDummyCheckFlag.UseVisualStyleBackColor = true;
            // 
            // chkPutReadyFlag
            // 
            this.chkPutReadyFlag.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkPutReadyFlag.Location = new System.Drawing.Point(3, 65);
            this.chkPutReadyFlag.Name = "chkPutReadyFlag";
            this.chkPutReadyFlag.Size = new System.Drawing.Size(180, 25);
            this.chkPutReadyFlag.TabIndex = 150;
            this.chkPutReadyFlag.Text = "Enable Put Ready ?";
            this.chkPutReadyFlag.UseVisualStyleBackColor = true;
            // 
            // chkGetReadyFlag
            // 
            this.chkGetReadyFlag.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkGetReadyFlag.Location = new System.Drawing.Point(189, 65);
            this.chkGetReadyFlag.Name = "chkGetReadyFlag";
            this.chkGetReadyFlag.Size = new System.Drawing.Size(180, 25);
            this.chkGetReadyFlag.TabIndex = 152;
            this.chkGetReadyFlag.Text = "Enable Get Ready ?";
            this.chkGetReadyFlag.UseVisualStyleBackColor = true;
            // 
            // chkRecipeCheckFlag
            // 
            this.chkRecipeCheckFlag.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkRecipeCheckFlag.Location = new System.Drawing.Point(3, 96);
            this.chkRecipeCheckFlag.Name = "chkRecipeCheckFlag";
            this.chkRecipeCheckFlag.Size = new System.Drawing.Size(180, 25);
            this.chkRecipeCheckFlag.TabIndex = 154;
            this.chkRecipeCheckFlag.Text = "Recipe Check ?";
            this.chkRecipeCheckFlag.UseVisualStyleBackColor = true;
            // 
            // chkIsMultiSlot
            // 
            this.chkIsMultiSlot.Enabled = false;
            this.chkIsMultiSlot.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkIsMultiSlot.Location = new System.Drawing.Point(189, 96);
            this.chkIsMultiSlot.Name = "chkIsMultiSlot";
            this.chkIsMultiSlot.Size = new System.Drawing.Size(180, 25);
            this.chkIsMultiSlot.TabIndex = 151;
            this.chkIsMultiSlot.Text = "Stage is multi  Slot ?";
            this.chkIsMultiSlot.UseVisualStyleBackColor = true;
            this.chkIsMultiSlot.Visible = false;
            // 
            // chkRTCReworkFlag
            // 
            this.chkRTCReworkFlag.Font = new System.Drawing.Font("Calibri", 13F);
            this.chkRTCReworkFlag.Location = new System.Drawing.Point(3, 127);
            this.chkRTCReworkFlag.Name = "chkRTCReworkFlag";
            this.chkRTCReworkFlag.Size = new System.Drawing.Size(180, 25);
            this.chkRTCReworkFlag.TabIndex = 154;
            this.chkRTCReworkFlag.Text = "RTC Rework Flag";
            this.chkRTCReworkFlag.UseVisualStyleBackColor = true;
            // 
            // explanation
            // 
            this.explanation.AutoSize = true;
            this.explanation.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.explanation.ForeColor = System.Drawing.Color.Red;
            this.explanation.Location = new System.Drawing.Point(338, 230);
            this.explanation.Name = "explanation";
            this.explanation.Size = new System.Drawing.Size(120, 18);
            this.explanation.TabIndex = 154;
            this.explanation.Text = "数字越大越优先";
            // 
            // flpFloatArea3
            // 
            this.flpFloatArea3.Controls.Add(this.pnlStageReportTrxName);
            this.flpFloatArea3.Controls.Add(this.pnlStageJobDataTrxName);
            this.flpFloatArea3.Controls.Add(this.pnlUpstreamPathTrxName);
            this.flpFloatArea3.Controls.Add(this.pnlUpstreamSendPathTrxName);
            this.flpFloatArea3.Controls.Add(this.pnlDownstreamPathTrxName);
            this.flpFloatArea3.Controls.Add(this.pnlDownstreamReceivePathTrxName);
            this.flpFloatArea3.Location = new System.Drawing.Point(0, 316);
            this.flpFloatArea3.Margin = new System.Windows.Forms.Padding(0);
            this.flpFloatArea3.Name = "flpFloatArea3";
            this.flpFloatArea3.Size = new System.Drawing.Size(830, 188);
            this.flpFloatArea3.TabIndex = 153;
            // 
            // pnlStageReportTrxName
            // 
            this.pnlStageReportTrxName.Controls.Add(this.txtStageReportTrxName);
            this.pnlStageReportTrxName.Controls.Add(this.lblStageReportTrxName);
            this.pnlStageReportTrxName.Location = new System.Drawing.Point(0, 0);
            this.pnlStageReportTrxName.Margin = new System.Windows.Forms.Padding(0);
            this.pnlStageReportTrxName.Name = "pnlStageReportTrxName";
            this.pnlStageReportTrxName.Size = new System.Drawing.Size(828, 30);
            this.pnlStageReportTrxName.TabIndex = 11;
            // 
            // txtStageReportTrxName
            // 
            this.txtStageReportTrxName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStageReportTrxName.Location = new System.Drawing.Point(233, 1);
            this.txtStageReportTrxName.Name = "txtStageReportTrxName";
            this.txtStageReportTrxName.Size = new System.Drawing.Size(589, 33);
            this.txtStageReportTrxName.TabIndex = 104;
            // 
            // lblStageReportTrxName
            // 
            this.lblStageReportTrxName.BackColor = System.Drawing.Color.Black;
            this.lblStageReportTrxName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStageReportTrxName.ForeColor = System.Drawing.Color.White;
            this.lblStageReportTrxName.Location = new System.Drawing.Point(2, 2);
            this.lblStageReportTrxName.Name = "lblStageReportTrxName";
            this.lblStageReportTrxName.Size = new System.Drawing.Size(230, 26);
            this.lblStageReportTrxName.TabIndex = 105;
            this.lblStageReportTrxName.Text = "Stage\'s PLC Trx Name";
            this.lblStageReportTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlStageJobDataTrxName
            // 
            this.pnlStageJobDataTrxName.Controls.Add(this.txtStageJobDataTrxName);
            this.pnlStageJobDataTrxName.Controls.Add(this.lblStageJobDataTrxName);
            this.pnlStageJobDataTrxName.Location = new System.Drawing.Point(0, 30);
            this.pnlStageJobDataTrxName.Margin = new System.Windows.Forms.Padding(0);
            this.pnlStageJobDataTrxName.Name = "pnlStageJobDataTrxName";
            this.pnlStageJobDataTrxName.Size = new System.Drawing.Size(828, 30);
            this.pnlStageJobDataTrxName.TabIndex = 12;
            // 
            // txtStageJobDataTrxName
            // 
            this.txtStageJobDataTrxName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStageJobDataTrxName.Location = new System.Drawing.Point(233, 1);
            this.txtStageJobDataTrxName.Name = "txtStageJobDataTrxName";
            this.txtStageJobDataTrxName.Size = new System.Drawing.Size(589, 33);
            this.txtStageJobDataTrxName.TabIndex = 131;
            // 
            // lblStageJobDataTrxName
            // 
            this.lblStageJobDataTrxName.BackColor = System.Drawing.Color.Black;
            this.lblStageJobDataTrxName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStageJobDataTrxName.ForeColor = System.Drawing.Color.White;
            this.lblStageJobDataTrxName.Location = new System.Drawing.Point(2, 2);
            this.lblStageJobDataTrxName.Name = "lblStageJobDataTrxName";
            this.lblStageJobDataTrxName.Size = new System.Drawing.Size(230, 26);
            this.lblStageJobDataTrxName.TabIndex = 132;
            this.lblStageJobDataTrxName.Text = "Stage\'s PLC Job Data Trx Name";
            this.lblStageJobDataTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlUpstreamPathTrxName
            // 
            this.pnlUpstreamPathTrxName.Controls.Add(this.txtUpstreamPathTrxName);
            this.pnlUpstreamPathTrxName.Controls.Add(this.lblUpstreamPathTrxName);
            this.pnlUpstreamPathTrxName.Location = new System.Drawing.Point(0, 60);
            this.pnlUpstreamPathTrxName.Margin = new System.Windows.Forms.Padding(0);
            this.pnlUpstreamPathTrxName.Name = "pnlUpstreamPathTrxName";
            this.pnlUpstreamPathTrxName.Size = new System.Drawing.Size(828, 30);
            this.pnlUpstreamPathTrxName.TabIndex = 13;
            // 
            // txtUpstreamPathTrxName
            // 
            this.txtUpstreamPathTrxName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUpstreamPathTrxName.Location = new System.Drawing.Point(233, 1);
            this.txtUpstreamPathTrxName.Name = "txtUpstreamPathTrxName";
            this.txtUpstreamPathTrxName.Size = new System.Drawing.Size(589, 33);
            this.txtUpstreamPathTrxName.TabIndex = 129;
            // 
            // lblUpstreamPathTrxName
            // 
            this.lblUpstreamPathTrxName.BackColor = System.Drawing.Color.Black;
            this.lblUpstreamPathTrxName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblUpstreamPathTrxName.ForeColor = System.Drawing.Color.White;
            this.lblUpstreamPathTrxName.Location = new System.Drawing.Point(2, 2);
            this.lblUpstreamPathTrxName.Name = "lblUpstreamPathTrxName";
            this.lblUpstreamPathTrxName.Size = new System.Drawing.Size(230, 26);
            this.lblUpstreamPathTrxName.TabIndex = 130;
            this.lblUpstreamPathTrxName.Text = "Upstream Path Trx Name";
            this.lblUpstreamPathTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlUpstreamSendPathTrxName
            // 
            this.pnlUpstreamSendPathTrxName.Controls.Add(this.txtUpstreamSendPathTrxName);
            this.pnlUpstreamSendPathTrxName.Controls.Add(this.lblUpstreamSendPathTrxName);
            this.pnlUpstreamSendPathTrxName.Location = new System.Drawing.Point(0, 90);
            this.pnlUpstreamSendPathTrxName.Margin = new System.Windows.Forms.Padding(0);
            this.pnlUpstreamSendPathTrxName.Name = "pnlUpstreamSendPathTrxName";
            this.pnlUpstreamSendPathTrxName.Size = new System.Drawing.Size(828, 30);
            this.pnlUpstreamSendPathTrxName.TabIndex = 14;
            // 
            // txtUpstreamSendPathTrxName
            // 
            this.txtUpstreamSendPathTrxName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUpstreamSendPathTrxName.Location = new System.Drawing.Point(233, 1);
            this.txtUpstreamSendPathTrxName.Name = "txtUpstreamSendPathTrxName";
            this.txtUpstreamSendPathTrxName.Size = new System.Drawing.Size(589, 33);
            this.txtUpstreamSendPathTrxName.TabIndex = 135;
            // 
            // lblUpstreamSendPathTrxName
            // 
            this.lblUpstreamSendPathTrxName.BackColor = System.Drawing.Color.Black;
            this.lblUpstreamSendPathTrxName.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpstreamSendPathTrxName.ForeColor = System.Drawing.Color.White;
            this.lblUpstreamSendPathTrxName.Location = new System.Drawing.Point(2, 2);
            this.lblUpstreamSendPathTrxName.Name = "lblUpstreamSendPathTrxName";
            this.lblUpstreamSendPathTrxName.Size = new System.Drawing.Size(230, 26);
            this.lblUpstreamSendPathTrxName.TabIndex = 136;
            this.lblUpstreamSendPathTrxName.Text = "Upstream Send Jobdata Trx Name";
            this.lblUpstreamSendPathTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlDownstreamPathTrxName
            // 
            this.pnlDownstreamPathTrxName.Controls.Add(this.txtDownstreamPathTrxName);
            this.pnlDownstreamPathTrxName.Controls.Add(this.lblDownstreamPathTrxName);
            this.pnlDownstreamPathTrxName.Location = new System.Drawing.Point(0, 120);
            this.pnlDownstreamPathTrxName.Margin = new System.Windows.Forms.Padding(0);
            this.pnlDownstreamPathTrxName.Name = "pnlDownstreamPathTrxName";
            this.pnlDownstreamPathTrxName.Size = new System.Drawing.Size(828, 30);
            this.pnlDownstreamPathTrxName.TabIndex = 15;
            // 
            // txtDownstreamPathTrxName
            // 
            this.txtDownstreamPathTrxName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDownstreamPathTrxName.Location = new System.Drawing.Point(233, 1);
            this.txtDownstreamPathTrxName.Name = "txtDownstreamPathTrxName";
            this.txtDownstreamPathTrxName.Size = new System.Drawing.Size(589, 33);
            this.txtDownstreamPathTrxName.TabIndex = 139;
            // 
            // lblDownstreamPathTrxName
            // 
            this.lblDownstreamPathTrxName.BackColor = System.Drawing.Color.Black;
            this.lblDownstreamPathTrxName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblDownstreamPathTrxName.ForeColor = System.Drawing.Color.White;
            this.lblDownstreamPathTrxName.Location = new System.Drawing.Point(2, 2);
            this.lblDownstreamPathTrxName.Name = "lblDownstreamPathTrxName";
            this.lblDownstreamPathTrxName.Size = new System.Drawing.Size(230, 26);
            this.lblDownstreamPathTrxName.TabIndex = 140;
            this.lblDownstreamPathTrxName.Text = "Downstream Path Trx Name";
            this.lblDownstreamPathTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlDownstreamReceivePathTrxName
            // 
            this.pnlDownstreamReceivePathTrxName.Controls.Add(this.txtDownstreamReceivePathTrxName);
            this.pnlDownstreamReceivePathTrxName.Controls.Add(this.lblDownstreamReceivePathTrxName);
            this.pnlDownstreamReceivePathTrxName.Location = new System.Drawing.Point(0, 150);
            this.pnlDownstreamReceivePathTrxName.Margin = new System.Windows.Forms.Padding(0);
            this.pnlDownstreamReceivePathTrxName.Name = "pnlDownstreamReceivePathTrxName";
            this.pnlDownstreamReceivePathTrxName.Size = new System.Drawing.Size(828, 30);
            this.pnlDownstreamReceivePathTrxName.TabIndex = 16;
            // 
            // txtDownstreamReceivePathTrxName
            // 
            this.txtDownstreamReceivePathTrxName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDownstreamReceivePathTrxName.Location = new System.Drawing.Point(233, 1);
            this.txtDownstreamReceivePathTrxName.Name = "txtDownstreamReceivePathTrxName";
            this.txtDownstreamReceivePathTrxName.Size = new System.Drawing.Size(589, 33);
            this.txtDownstreamReceivePathTrxName.TabIndex = 141;
            // 
            // lblDownstreamReceivePathTrxName
            // 
            this.lblDownstreamReceivePathTrxName.BackColor = System.Drawing.Color.Black;
            this.lblDownstreamReceivePathTrxName.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDownstreamReceivePathTrxName.ForeColor = System.Drawing.Color.White;
            this.lblDownstreamReceivePathTrxName.Location = new System.Drawing.Point(2, 2);
            this.lblDownstreamReceivePathTrxName.Name = "lblDownstreamReceivePathTrxName";
            this.lblDownstreamReceivePathTrxName.Size = new System.Drawing.Size(230, 26);
            this.lblDownstreamReceivePathTrxName.TabIndex = 142;
            this.lblDownstreamReceivePathTrxName.Text = "Downstream Receive Jobdata Trx Name";
            this.lblDownstreamReceivePathTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtStageID
            // 
            this.txtStageID.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStageID.Location = new System.Drawing.Point(233, 128);
            this.txtStageID.MaxLength = 2;
            this.txtStageID.Name = "txtStageID";
            this.txtStageID.Size = new System.Drawing.Size(200, 33);
            this.txtStageID.TabIndex = 1;
            this.txtStageID.TextChanged += new System.EventHandler(this.txtStageID_TextChanged);
            this.txtStageID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtInt_KeyPress);
            // 
            // txtStageName
            // 
            this.txtStageName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStageName.Location = new System.Drawing.Point(233, 159);
            this.txtStageName.Name = "txtStageName";
            this.txtStageName.Size = new System.Drawing.Size(200, 33);
            this.txtStageName.TabIndex = 2;
            // 
            // cboNode
            // 
            this.cboNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboNode.FormattingEnabled = true;
            this.cboNode.Location = new System.Drawing.Point(233, 33);
            this.cboNode.Name = "cboNode";
            this.cboNode.Size = new System.Drawing.Size(200, 34);
            this.cboNode.TabIndex = 1;
            // 
            // cboStageType
            // 
            this.cboStageType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStageType.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboStageType.FormattingEnabled = true;
            this.cboStageType.Location = new System.Drawing.Point(233, 96);
            this.cboStageType.Name = "cboStageType";
            this.cboStageType.Size = new System.Drawing.Size(200, 34);
            this.cboStageType.TabIndex = 2;
            this.cboStageType.SelectionChangeCommitted += new System.EventHandler(this.cboStageType_SelectionChangeCommitted);
            // 
            // cboRobotName
            // 
            this.cboRobotName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRobotName.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboRobotName.FormattingEnabled = true;
            this.cboRobotName.Location = new System.Drawing.Point(233, 2);
            this.cboRobotName.Name = "cboRobotName";
            this.cboRobotName.Size = new System.Drawing.Size(200, 34);
            this.cboRobotName.TabIndex = 0;
            // 
            // lblStageID
            // 
            this.lblStageID.BackColor = System.Drawing.Color.Black;
            this.lblStageID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStageID.ForeColor = System.Drawing.Color.White;
            this.lblStageID.Location = new System.Drawing.Point(3, 129);
            this.lblStageID.Name = "lblStageID";
            this.lblStageID.Size = new System.Drawing.Size(230, 26);
            this.lblStageID.TabIndex = 95;
            this.lblStageID.Text = "Stage ID";
            this.lblStageID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRobotName
            // 
            this.lblRobotName.BackColor = System.Drawing.Color.Black;
            this.lblRobotName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblRobotName.ForeColor = System.Drawing.Color.White;
            this.lblRobotName.Location = new System.Drawing.Point(3, 4);
            this.lblRobotName.Name = "lblRobotName";
            this.lblRobotName.Size = new System.Drawing.Size(230, 26);
            this.lblRobotName.TabIndex = 93;
            this.lblRobotName.Text = "Robot Name";
            this.lblRobotName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStageName
            // 
            this.lblStageName.BackColor = System.Drawing.Color.Black;
            this.lblStageName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStageName.ForeColor = System.Drawing.Color.White;
            this.lblStageName.Location = new System.Drawing.Point(3, 160);
            this.lblStageName.Name = "lblStageName";
            this.lblStageName.Size = new System.Drawing.Size(230, 26);
            this.lblStageName.TabIndex = 97;
            this.lblStageName.Text = "Stage Name";
            this.lblStageName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPriority
            // 
            this.lblPriority.BackColor = System.Drawing.Color.Black;
            this.lblPriority.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblPriority.ForeColor = System.Drawing.Color.White;
            this.lblPriority.Location = new System.Drawing.Point(3, 222);
            this.lblPriority.Name = "lblPriority";
            this.lblPriority.Size = new System.Drawing.Size(230, 25);
            this.lblPriority.TabIndex = 101;
            this.lblPriority.Text = "Priority";
            this.lblPriority.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblNode
            // 
            this.lblNode.BackColor = System.Drawing.Color.Black;
            this.lblNode.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblNode.ForeColor = System.Drawing.Color.White;
            this.lblNode.Location = new System.Drawing.Point(3, 35);
            this.lblNode.Name = "lblNode";
            this.lblNode.Size = new System.Drawing.Size(230, 26);
            this.lblNode.TabIndex = 106;
            this.lblNode.Text = "Local No";
            this.lblNode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStageType
            // 
            this.lblStageType.BackColor = System.Drawing.Color.Black;
            this.lblStageType.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStageType.ForeColor = System.Drawing.Color.White;
            this.lblStageType.Location = new System.Drawing.Point(3, 97);
            this.lblStageType.Name = "lblStageType";
            this.lblStageType.Size = new System.Drawing.Size(230, 26);
            this.lblStageType.TabIndex = 107;
            this.lblStageType.Text = "Stage Type";
            this.lblStageType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSlotMaxCount
            // 
            this.lblSlotMaxCount.BackColor = System.Drawing.Color.Black;
            this.lblSlotMaxCount.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblSlotMaxCount.ForeColor = System.Drawing.Color.White;
            this.lblSlotMaxCount.Location = new System.Drawing.Point(3, 192);
            this.lblSlotMaxCount.Name = "lblSlotMaxCount";
            this.lblSlotMaxCount.Size = new System.Drawing.Size(230, 25);
            this.lblSlotMaxCount.TabIndex = 116;
            this.lblSlotMaxCount.Text = "Slot Max Count";
            this.lblSlotMaxCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPriority
            // 
            this.txtPriority.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPriority.Location = new System.Drawing.Point(233, 221);
            this.txtPriority.MaxLength = 5;
            this.txtPriority.Name = "txtPriority";
            this.txtPriority.Size = new System.Drawing.Size(101, 33);
            this.txtPriority.TabIndex = 5;
            this.txtPriority.Text = "0";
            this.txtPriority.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtInt_KeyPress);
            // 
            // lblStageIDByNode
            // 
            this.lblStageIDByNode.BackColor = System.Drawing.Color.Black;
            this.lblStageIDByNode.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblStageIDByNode.ForeColor = System.Drawing.Color.White;
            this.lblStageIDByNode.Location = new System.Drawing.Point(3, 191);
            this.lblStageIDByNode.Name = "lblStageIDByNode";
            this.lblStageIDByNode.Size = new System.Drawing.Size(230, 25);
            this.lblStageIDByNode.TabIndex = 99;
            this.lblStageIDByNode.Text = "Stage ID By Local";
            this.lblStageIDByNode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblStageIDByNode.Visible = false;
            // 
            // txtSlotMaxCount
            // 
            this.txtSlotMaxCount.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSlotMaxCount.Location = new System.Drawing.Point(233, 191);
            this.txtSlotMaxCount.MaxLength = 5;
            this.txtSlotMaxCount.Name = "txtSlotMaxCount";
            this.txtSlotMaxCount.Size = new System.Drawing.Size(200, 33);
            this.txtSlotMaxCount.TabIndex = 3;
            this.txtSlotMaxCount.Text = "0";
            this.txtSlotMaxCount.TextChanged += new System.EventHandler(this.txtSlotMaxCount_TextChanged);
            this.txtSlotMaxCount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtInt_KeyPress);
            // 
            // txtStageIDByNode
            // 
            this.txtStageIDByNode.Enabled = false;
            this.txtStageIDByNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStageIDByNode.Location = new System.Drawing.Point(233, 191);
            this.txtStageIDByNode.Name = "txtStageIDByNode";
            this.txtStageIDByNode.Size = new System.Drawing.Size(200, 33);
            this.txtStageIDByNode.TabIndex = 4;
            this.txtStageIDByNode.Visible = false;
            // 
            // FormRobotStageManagementEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(845, 762);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormRobotStageManagementEdit";
            this.Text = "   ";
            this.Load += new System.EventHandler(this.FormRobotStageManagementEdit_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.flpButton.ResumeLayout(false);
            this.pnlAdd.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAddList)).EndInit();
            this.pnlButton.ResumeLayout(false);
            this.grbData.ResumeLayout(false);
            this.pnlEdit.ResumeLayout(false);
            this.pnlEdit.PerformLayout();
            this.pnlTrackDataSeqList.ResumeLayout(false);
            this.pnlTrackDataSeqList.PerformLayout();
            this.flpFloatArea1.ResumeLayout(false);
            this.pnlCassetteType.ResumeLayout(false);
            this.pnlSlotFetchSeq.ResumeLayout(false);
            this.pnlSlotStoreSeq.ResumeLayout(false);
            this.pnlArmUsePriority.ResumeLayout(false);
            this.pnlUseSpecificArm.ResumeLayout(false);
            this.flpFloatArea2.ResumeLayout(false);
            this.flpFloatArea3.ResumeLayout(false);
            this.pnlStageReportTrxName.ResumeLayout(false);
            this.pnlStageReportTrxName.PerformLayout();
            this.pnlStageJobDataTrxName.ResumeLayout(false);
            this.pnlStageJobDataTrxName.PerformLayout();
            this.pnlUpstreamPathTrxName.ResumeLayout(false);
            this.pnlUpstreamPathTrxName.PerformLayout();
            this.pnlUpstreamSendPathTrxName.ResumeLayout(false);
            this.pnlUpstreamSendPathTrxName.PerformLayout();
            this.pnlDownstreamPathTrxName.ResumeLayout(false);
            this.pnlDownstreamPathTrxName.PerformLayout();
            this.pnlDownstreamReceivePathTrxName.ResumeLayout(false);
            this.pnlDownstreamReceivePathTrxName.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOKClose;
        private System.Windows.Forms.Button btnAddOK;
        private System.Windows.Forms.Button btnCancelClose;
        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.FlowLayoutPanel flpButton;
        private System.Windows.Forms.Panel pnlAdd;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.GroupBox grbData;
        private System.Windows.Forms.DataGridView dgvAddList;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStageID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStageName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStageIDByNode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStageType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPriority;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStageReportTrxName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStageJobDataTrxName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsMultiSlot;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotMaxCount;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colRecipeCheckFlag;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colDummyCheckFlag;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colGetReadyFlag;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colPutReadyFlag;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colPrefetchFlag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRTCReworkFlag;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colSupportWaitFront;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUpstreamPathTrxName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUpstreamSendPathTrxName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDownstreamPathTrxName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDownstreamReceivePathTrxName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrackDataSeqList;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCassetteType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemark;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colStageEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotFetchSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotStoreSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExchangeType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEQRobotIfType;
        private System.Windows.Forms.Panel pnlEdit;
        private System.Windows.Forms.ComboBox cboExchangeType;
        private System.Windows.Forms.Label lblExchangeType;
        private System.Windows.Forms.FlowLayoutPanel flpFloatArea1;
        private System.Windows.Forms.Label explanation;
        private System.Windows.Forms.FlowLayoutPanel flpFloatArea3;
        private System.Windows.Forms.Panel pnlStageReportTrxName;
        private System.Windows.Forms.TextBox txtStageReportTrxName;
        private System.Windows.Forms.Label lblStageReportTrxName;
        private System.Windows.Forms.Panel pnlStageJobDataTrxName;
        private System.Windows.Forms.TextBox txtStageJobDataTrxName;
        private System.Windows.Forms.Label lblStageJobDataTrxName;
        private System.Windows.Forms.Panel pnlUpstreamPathTrxName;
        private System.Windows.Forms.TextBox txtUpstreamPathTrxName;
        private System.Windows.Forms.Label lblUpstreamPathTrxName;
        private System.Windows.Forms.Panel pnlUpstreamSendPathTrxName;
        private System.Windows.Forms.TextBox txtUpstreamSendPathTrxName;
        private System.Windows.Forms.Label lblUpstreamSendPathTrxName;
        private System.Windows.Forms.Panel pnlDownstreamPathTrxName;
        private System.Windows.Forms.TextBox txtDownstreamPathTrxName;
        private System.Windows.Forms.Label lblDownstreamPathTrxName;
        private System.Windows.Forms.Panel pnlDownstreamReceivePathTrxName;
        private System.Windows.Forms.TextBox txtDownstreamReceivePathTrxName;
        private System.Windows.Forms.Label lblDownstreamReceivePathTrxName;
        private System.Windows.Forms.TextBox txtStageID;
        private System.Windows.Forms.TextBox txtStageName;
        private System.Windows.Forms.ComboBox cboNode;
        private System.Windows.Forms.ComboBox cboStageType;
        private System.Windows.Forms.ComboBox cboRobotName;
        private System.Windows.Forms.Label lblStageID;
        private System.Windows.Forms.Label lblRobotName;
        private System.Windows.Forms.Label lblStageName;
        private System.Windows.Forms.Label lblPriority;
        private System.Windows.Forms.Label lblNode;
        private System.Windows.Forms.Label lblStageType;
        private System.Windows.Forms.Label lblSlotMaxCount;
        private System.Windows.Forms.TextBox txtPriority;
        private System.Windows.Forms.Label lblStageIDByNode;
        private System.Windows.Forms.TextBox txtSlotMaxCount;
        private System.Windows.Forms.TextBox txtStageIDByNode;
        private System.Windows.Forms.Panel pnlTrackDataSeqList;
        private System.Windows.Forms.TextBox txtTrackDataSeqList;
        private System.Windows.Forms.Label lblTrackDataSeqList;
        private System.Windows.Forms.ComboBox cboEQRobotIfType;
        private System.Windows.Forms.Label lblEQRobotIfType;
        private System.Windows.Forms.Panel pnlCassetteType;
        private System.Windows.Forms.ComboBox cboCassetteType;
        private System.Windows.Forms.Label lblStageCstType;
        private System.Windows.Forms.Panel pnlSlotFetchSeq;
        private System.Windows.Forms.ComboBox cboSlotFetchSeq;
        private System.Windows.Forms.Label lblSlotFetchSeq;
        private System.Windows.Forms.Panel pnlSlotStoreSeq;
        private System.Windows.Forms.ComboBox cboSlotStoreSeq;
        private System.Windows.Forms.Label lblSlotStoreSeq;
        private System.Windows.Forms.Panel pnlArmUsePriority;
        private System.Windows.Forms.ComboBox cboArmUsePriority;
        private System.Windows.Forms.Label lblArmUsePriority;
        private System.Windows.Forms.Panel pnlUseSpecificArm;
        private System.Windows.Forms.ComboBox cboUseSpecificArm;
        private System.Windows.Forms.Label lblUseSpecificArm;
        private System.Windows.Forms.FlowLayoutPanel flpFloatArea2;
        private System.Windows.Forms.CheckBox chkStageEnabled;
        private System.Windows.Forms.CheckBox chkPrefetchFlag;
        private System.Windows.Forms.CheckBox chkSupportWaitFront;
        private System.Windows.Forms.CheckBox chkDummyCheckFlag;
        private System.Windows.Forms.CheckBox chkPutReadyFlag;
        private System.Windows.Forms.CheckBox chkGetReadyFlag;
        private System.Windows.Forms.CheckBox chkRecipeCheckFlag;
        private System.Windows.Forms.CheckBox chkIsMultiSlot;
        private System.Windows.Forms.CheckBox chkRTCReworkFlag;
    }
}