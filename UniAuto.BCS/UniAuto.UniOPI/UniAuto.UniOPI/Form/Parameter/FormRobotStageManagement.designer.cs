namespace UniOPI
{
    partial class FormRobotStageManagement
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
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.grbDetail = new System.Windows.Forms.GroupBox();
            this.flpDetail = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlStageName = new System.Windows.Forms.Panel();
            this.txtStageName = new System.Windows.Forms.TextBox();
            this.lblStageName = new System.Windows.Forms.Label();
            this.pnlStageIDByNode = new System.Windows.Forms.Panel();
            this.txtStageIDByNode = new System.Windows.Forms.TextBox();
            this.lblStageIDByNode = new System.Windows.Forms.Label();
            this.pnlStageType = new System.Windows.Forms.Panel();
            this.txtStageType = new System.Windows.Forms.TextBox();
            this.lblStageType = new System.Windows.Forms.Label();
            this.pnlPriority = new System.Windows.Forms.Panel();
            this.txtPriority = new System.Windows.Forms.TextBox();
            this.lblPriority = new System.Windows.Forms.Label();
            this.pnlSlotMaxCount = new System.Windows.Forms.Panel();
            this.txtSlotMaxCount = new System.Windows.Forms.TextBox();
            this.lblSlotMaxCount = new System.Windows.Forms.Label();
            this.pnlTrackDataSeqList = new System.Windows.Forms.Panel();
            this.txtTrackDataSeqList = new System.Windows.Forms.TextBox();
            this.lblTrackDataSeqList = new System.Windows.Forms.Label();
            this.pnlCassetteType = new System.Windows.Forms.Panel();
            this.txtCassetteType = new System.Windows.Forms.TextBox();
            this.lblCassetteType = new System.Windows.Forms.Label();
            this.pnlSlotFetchSeq = new System.Windows.Forms.Panel();
            this.txtSlotFetchSeq = new System.Windows.Forms.TextBox();
            this.lblSlotFetchSeq = new System.Windows.Forms.Label();
            this.pnlSlotStoreSeq = new System.Windows.Forms.Panel();
            this.txtSlotStoreSeq = new System.Windows.Forms.TextBox();
            this.lblSlotStoreSeq = new System.Windows.Forms.Label();
            this.pnlExchangeType = new System.Windows.Forms.Panel();
            this.txtExchangeType = new System.Windows.Forms.TextBox();
            this.lblExchangeType = new System.Windows.Forms.Label();
            this.pnlEQRobotIfType = new System.Windows.Forms.Panel();
            this.txtEQRobotIfType = new System.Windows.Forms.TextBox();
            this.lblEQRobotIfType = new System.Windows.Forms.Label();
            this.pnlArmUsePriority = new System.Windows.Forms.Panel();
            this.txtArmUsePriority = new System.Windows.Forms.TextBox();
            this.lblArmUsePriority = new System.Windows.Forms.Label();
            this.pnlUseSpecificArm = new System.Windows.Forms.Panel();
            this.txtUseSpecificArm = new System.Windows.Forms.TextBox();
            this.lblUseSpecificArm = new System.Windows.Forms.Label();
            this.pnlStageJobDataTrxName = new System.Windows.Forms.Panel();
            this.txtStageJobDataTrxName = new System.Windows.Forms.TextBox();
            this.lblStageJobDataTrxName = new System.Windows.Forms.Label();
            this.pnlStageReportTrxName = new System.Windows.Forms.Panel();
            this.txtStageReportTrxName = new System.Windows.Forms.TextBox();
            this.lblStageReportTrxName = new System.Windows.Forms.Label();
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
            this.pnlIsMultiSlot = new System.Windows.Forms.Panel();
            this.chkIsMultiSlot = new System.Windows.Forms.CheckBox();
            this.pnlRecipeCheckFlag = new System.Windows.Forms.Panel();
            this.chkRecipeCheckFlag = new System.Windows.Forms.CheckBox();
            this.pnlDummyCheckFlag = new System.Windows.Forms.Panel();
            this.chkDummyCheckFlag = new System.Windows.Forms.CheckBox();
            this.pnlGetReadyFlag = new System.Windows.Forms.Panel();
            this.chkGetReadyFlag = new System.Windows.Forms.CheckBox();
            this.pnlPutReadyFlag = new System.Windows.Forms.Panel();
            this.chkPutReadyFlag = new System.Windows.Forms.CheckBox();
            this.pnlPrefetchFlag = new System.Windows.Forms.Panel();
            this.chkPrefetchFlag = new System.Windows.Forms.CheckBox();
            this.pnlwaitFrontFlag = new System.Windows.Forms.Panel();
            this.chkwaitFrontFlag = new System.Windows.Forms.CheckBox();
            this.pnlStageEnabled = new System.Windows.Forms.Panel();
            this.chkStageEnabled = new System.Windows.Forms.CheckBox();
            this.pnlRTCReworkFlag = new System.Windows.Forms.Panel();
            this.chkRTCReworkFlag = new System.Windows.Forms.CheckBox();
            this.grbData = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
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
            this.colSlotFetchSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSlotStoreSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExchangeType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEQRobotIfTypre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpButton = new System.Windows.Forms.TableLayoutPanel();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnModify = new System.Windows.Forms.Button();
            this.pnlRefresh = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.flpRobot = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.grbDetail.SuspendLayout();
            this.flpDetail.SuspendLayout();
            this.pnlStageName.SuspendLayout();
            this.pnlStageIDByNode.SuspendLayout();
            this.pnlStageType.SuspendLayout();
            this.pnlPriority.SuspendLayout();
            this.pnlSlotMaxCount.SuspendLayout();
            this.pnlTrackDataSeqList.SuspendLayout();
            this.pnlCassetteType.SuspendLayout();
            this.pnlSlotFetchSeq.SuspendLayout();
            this.pnlSlotStoreSeq.SuspendLayout();
            this.pnlExchangeType.SuspendLayout();
            this.pnlEQRobotIfType.SuspendLayout();
            this.pnlArmUsePriority.SuspendLayout();
            this.pnlUseSpecificArm.SuspendLayout();
            this.pnlStageJobDataTrxName.SuspendLayout();
            this.pnlStageReportTrxName.SuspendLayout();
            this.pnlUpstreamPathTrxName.SuspendLayout();
            this.pnlUpstreamSendPathTrxName.SuspendLayout();
            this.pnlDownstreamPathTrxName.SuspendLayout();
            this.pnlDownstreamReceivePathTrxName.SuspendLayout();
            this.pnlIsMultiSlot.SuspendLayout();
            this.pnlRecipeCheckFlag.SuspendLayout();
            this.pnlDummyCheckFlag.SuspendLayout();
            this.pnlGetReadyFlag.SuspendLayout();
            this.pnlPutReadyFlag.SuspendLayout();
            this.pnlPrefetchFlag.SuspendLayout();
            this.pnlwaitFrontFlag.SuspendLayout();
            this.pnlStageEnabled.SuspendLayout();
            this.pnlRTCReworkFlag.SuspendLayout();
            this.grbData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tlpButton.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlRefresh.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1080, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(1140, 657);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 3;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 362F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128F));
            this.tlpBase.Controls.Add(this.grbDetail, 1, 1);
            this.tlpBase.Controls.Add(this.grbData, 0, 1);
            this.tlpBase.Controls.Add(this.tlpButton, 2, 1);
            this.tlpBase.Controls.Add(this.pnlRefresh, 0, 0);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Size = new System.Drawing.Size(1140, 626);
            this.tlpBase.TabIndex = 6;
            // 
            // grbDetail
            // 
            this.grbDetail.Controls.Add(this.flpDetail);
            this.grbDetail.Location = new System.Drawing.Point(365, 53);
            this.grbDetail.Name = "grbDetail";
            this.grbDetail.Size = new System.Drawing.Size(644, 567);
            this.grbDetail.TabIndex = 14;
            this.grbDetail.TabStop = false;
            // 
            // flpDetail
            // 
            this.flpDetail.Controls.Add(this.pnlStageName);
            this.flpDetail.Controls.Add(this.pnlStageIDByNode);
            this.flpDetail.Controls.Add(this.pnlStageType);
            this.flpDetail.Controls.Add(this.pnlPriority);
            this.flpDetail.Controls.Add(this.pnlSlotMaxCount);
            this.flpDetail.Controls.Add(this.pnlTrackDataSeqList);
            this.flpDetail.Controls.Add(this.pnlCassetteType);
            this.flpDetail.Controls.Add(this.pnlSlotFetchSeq);
            this.flpDetail.Controls.Add(this.pnlSlotStoreSeq);
            this.flpDetail.Controls.Add(this.pnlExchangeType);
            this.flpDetail.Controls.Add(this.pnlEQRobotIfType);
            this.flpDetail.Controls.Add(this.pnlArmUsePriority);
            this.flpDetail.Controls.Add(this.pnlUseSpecificArm);
            this.flpDetail.Controls.Add(this.pnlStageJobDataTrxName);
            this.flpDetail.Controls.Add(this.pnlStageReportTrxName);
            this.flpDetail.Controls.Add(this.pnlUpstreamPathTrxName);
            this.flpDetail.Controls.Add(this.pnlUpstreamSendPathTrxName);
            this.flpDetail.Controls.Add(this.pnlDownstreamPathTrxName);
            this.flpDetail.Controls.Add(this.pnlDownstreamReceivePathTrxName);
            this.flpDetail.Controls.Add(this.pnlIsMultiSlot);
            this.flpDetail.Controls.Add(this.pnlRecipeCheckFlag);
            this.flpDetail.Controls.Add(this.pnlDummyCheckFlag);
            this.flpDetail.Controls.Add(this.pnlGetReadyFlag);
            this.flpDetail.Controls.Add(this.pnlPutReadyFlag);
            this.flpDetail.Controls.Add(this.pnlPrefetchFlag);
            this.flpDetail.Controls.Add(this.pnlwaitFrontFlag);
            this.flpDetail.Controls.Add(this.pnlStageEnabled);
            this.flpDetail.Controls.Add(this.pnlRTCReworkFlag);
            this.flpDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpDetail.Location = new System.Drawing.Point(3, 18);
            this.flpDetail.Name = "flpDetail";
            this.flpDetail.Size = new System.Drawing.Size(638, 546);
            this.flpDetail.TabIndex = 149;
            // 
            // pnlStageName
            // 
            this.pnlStageName.Controls.Add(this.txtStageName);
            this.pnlStageName.Controls.Add(this.lblStageName);
            this.pnlStageName.Location = new System.Drawing.Point(3, 1);
            this.pnlStageName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlStageName.Name = "pnlStageName";
            this.pnlStageName.Size = new System.Drawing.Size(310, 30);
            this.pnlStageName.TabIndex = 183;
            // 
            // txtStageName
            // 
            this.txtStageName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtStageName.Location = new System.Drawing.Point(150, 1);
            this.txtStageName.Name = "txtStageName";
            this.txtStageName.ReadOnly = true;
            this.txtStageName.Size = new System.Drawing.Size(155, 25);
            this.txtStageName.TabIndex = 157;
            // 
            // lblStageName
            // 
            this.lblStageName.BackColor = System.Drawing.Color.Black;
            this.lblStageName.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblStageName.ForeColor = System.Drawing.Color.White;
            this.lblStageName.Location = new System.Drawing.Point(3, 1);
            this.lblStageName.Name = "lblStageName";
            this.lblStageName.Size = new System.Drawing.Size(150, 25);
            this.lblStageName.TabIndex = 156;
            this.lblStageName.Text = "Stage Name";
            this.lblStageName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlStageIDByNode
            // 
            this.pnlStageIDByNode.Controls.Add(this.txtStageIDByNode);
            this.pnlStageIDByNode.Controls.Add(this.lblStageIDByNode);
            this.pnlStageIDByNode.Location = new System.Drawing.Point(319, 1);
            this.pnlStageIDByNode.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlStageIDByNode.Name = "pnlStageIDByNode";
            this.pnlStageIDByNode.Size = new System.Drawing.Size(310, 30);
            this.pnlStageIDByNode.TabIndex = 161;
            // 
            // txtStageIDByNode
            // 
            this.txtStageIDByNode.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtStageIDByNode.Location = new System.Drawing.Point(150, 1);
            this.txtStageIDByNode.Name = "txtStageIDByNode";
            this.txtStageIDByNode.ReadOnly = true;
            this.txtStageIDByNode.Size = new System.Drawing.Size(155, 25);
            this.txtStageIDByNode.TabIndex = 108;
            // 
            // lblStageIDByNode
            // 
            this.lblStageIDByNode.BackColor = System.Drawing.Color.Black;
            this.lblStageIDByNode.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblStageIDByNode.ForeColor = System.Drawing.Color.White;
            this.lblStageIDByNode.Location = new System.Drawing.Point(3, 1);
            this.lblStageIDByNode.Name = "lblStageIDByNode";
            this.lblStageIDByNode.Size = new System.Drawing.Size(150, 25);
            this.lblStageIDByNode.TabIndex = 110;
            this.lblStageIDByNode.Text = "Stage ID By Local";
            this.lblStageIDByNode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlStageType
            // 
            this.pnlStageType.Controls.Add(this.txtStageType);
            this.pnlStageType.Controls.Add(this.lblStageType);
            this.pnlStageType.Location = new System.Drawing.Point(3, 35);
            this.pnlStageType.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlStageType.Name = "pnlStageType";
            this.pnlStageType.Size = new System.Drawing.Size(310, 30);
            this.pnlStageType.TabIndex = 162;
            // 
            // txtStageType
            // 
            this.txtStageType.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtStageType.Location = new System.Drawing.Point(150, 1);
            this.txtStageType.Name = "txtStageType";
            this.txtStageType.ReadOnly = true;
            this.txtStageType.Size = new System.Drawing.Size(155, 25);
            this.txtStageType.TabIndex = 112;
            // 
            // lblStageType
            // 
            this.lblStageType.BackColor = System.Drawing.Color.Black;
            this.lblStageType.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblStageType.ForeColor = System.Drawing.Color.White;
            this.lblStageType.Location = new System.Drawing.Point(3, 1);
            this.lblStageType.Name = "lblStageType";
            this.lblStageType.Size = new System.Drawing.Size(150, 25);
            this.lblStageType.TabIndex = 111;
            this.lblStageType.Text = "Stage Type";
            this.lblStageType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlPriority
            // 
            this.pnlPriority.Controls.Add(this.txtPriority);
            this.pnlPriority.Controls.Add(this.lblPriority);
            this.pnlPriority.Location = new System.Drawing.Point(319, 35);
            this.pnlPriority.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlPriority.Name = "pnlPriority";
            this.pnlPriority.Size = new System.Drawing.Size(310, 30);
            this.pnlPriority.TabIndex = 163;
            // 
            // txtPriority
            // 
            this.txtPriority.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtPriority.Location = new System.Drawing.Point(150, 1);
            this.txtPriority.Name = "txtPriority";
            this.txtPriority.ReadOnly = true;
            this.txtPriority.Size = new System.Drawing.Size(155, 25);
            this.txtPriority.TabIndex = 112;
            // 
            // lblPriority
            // 
            this.lblPriority.BackColor = System.Drawing.Color.Black;
            this.lblPriority.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblPriority.ForeColor = System.Drawing.Color.White;
            this.lblPriority.Location = new System.Drawing.Point(3, 1);
            this.lblPriority.Name = "lblPriority";
            this.lblPriority.Size = new System.Drawing.Size(150, 25);
            this.lblPriority.TabIndex = 114;
            this.lblPriority.Text = "Priority";
            this.lblPriority.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlSlotMaxCount
            // 
            this.pnlSlotMaxCount.Controls.Add(this.txtSlotMaxCount);
            this.pnlSlotMaxCount.Controls.Add(this.lblSlotMaxCount);
            this.pnlSlotMaxCount.Location = new System.Drawing.Point(3, 69);
            this.pnlSlotMaxCount.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlSlotMaxCount.Name = "pnlSlotMaxCount";
            this.pnlSlotMaxCount.Size = new System.Drawing.Size(310, 30);
            this.pnlSlotMaxCount.TabIndex = 166;
            // 
            // txtSlotMaxCount
            // 
            this.txtSlotMaxCount.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtSlotMaxCount.Location = new System.Drawing.Point(150, 1);
            this.txtSlotMaxCount.Name = "txtSlotMaxCount";
            this.txtSlotMaxCount.ReadOnly = true;
            this.txtSlotMaxCount.Size = new System.Drawing.Size(155, 25);
            this.txtSlotMaxCount.TabIndex = 118;
            // 
            // lblSlotMaxCount
            // 
            this.lblSlotMaxCount.BackColor = System.Drawing.Color.Black;
            this.lblSlotMaxCount.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblSlotMaxCount.ForeColor = System.Drawing.Color.White;
            this.lblSlotMaxCount.Location = new System.Drawing.Point(3, 1);
            this.lblSlotMaxCount.Name = "lblSlotMaxCount";
            this.lblSlotMaxCount.Size = new System.Drawing.Size(150, 25);
            this.lblSlotMaxCount.TabIndex = 120;
            this.lblSlotMaxCount.Text = "Slot Max Count";
            this.lblSlotMaxCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlTrackDataSeqList
            // 
            this.pnlTrackDataSeqList.Controls.Add(this.txtTrackDataSeqList);
            this.pnlTrackDataSeqList.Controls.Add(this.lblTrackDataSeqList);
            this.pnlTrackDataSeqList.Location = new System.Drawing.Point(319, 69);
            this.pnlTrackDataSeqList.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlTrackDataSeqList.Name = "pnlTrackDataSeqList";
            this.pnlTrackDataSeqList.Size = new System.Drawing.Size(310, 30);
            this.pnlTrackDataSeqList.TabIndex = 171;
            // 
            // txtTrackDataSeqList
            // 
            this.txtTrackDataSeqList.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtTrackDataSeqList.Location = new System.Drawing.Point(150, 1);
            this.txtTrackDataSeqList.Name = "txtTrackDataSeqList";
            this.txtTrackDataSeqList.ReadOnly = true;
            this.txtTrackDataSeqList.Size = new System.Drawing.Size(155, 25);
            this.txtTrackDataSeqList.TabIndex = 137;
            // 
            // lblTrackDataSeqList
            // 
            this.lblTrackDataSeqList.BackColor = System.Drawing.Color.Black;
            this.lblTrackDataSeqList.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblTrackDataSeqList.ForeColor = System.Drawing.Color.White;
            this.lblTrackDataSeqList.Location = new System.Drawing.Point(3, 1);
            this.lblTrackDataSeqList.Name = "lblTrackDataSeqList";
            this.lblTrackDataSeqList.Size = new System.Drawing.Size(150, 25);
            this.lblTrackDataSeqList.TabIndex = 138;
            this.lblTrackDataSeqList.Text = "Track Data Seq List";
            this.lblTrackDataSeqList.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlCassetteType
            // 
            this.pnlCassetteType.Controls.Add(this.txtCassetteType);
            this.pnlCassetteType.Controls.Add(this.lblCassetteType);
            this.pnlCassetteType.Location = new System.Drawing.Point(3, 103);
            this.pnlCassetteType.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlCassetteType.Name = "pnlCassetteType";
            this.pnlCassetteType.Size = new System.Drawing.Size(310, 30);
            this.pnlCassetteType.TabIndex = 180;
            // 
            // txtCassetteType
            // 
            this.txtCassetteType.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtCassetteType.Location = new System.Drawing.Point(150, 1);
            this.txtCassetteType.Name = "txtCassetteType";
            this.txtCassetteType.ReadOnly = true;
            this.txtCassetteType.Size = new System.Drawing.Size(155, 25);
            this.txtCassetteType.TabIndex = 157;
            // 
            // lblCassetteType
            // 
            this.lblCassetteType.BackColor = System.Drawing.Color.Black;
            this.lblCassetteType.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblCassetteType.ForeColor = System.Drawing.Color.White;
            this.lblCassetteType.Location = new System.Drawing.Point(3, 1);
            this.lblCassetteType.Name = "lblCassetteType";
            this.lblCassetteType.Size = new System.Drawing.Size(150, 25);
            this.lblCassetteType.TabIndex = 156;
            this.lblCassetteType.Text = "Cassette Type";
            this.lblCassetteType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlSlotFetchSeq
            // 
            this.pnlSlotFetchSeq.Controls.Add(this.txtSlotFetchSeq);
            this.pnlSlotFetchSeq.Controls.Add(this.lblSlotFetchSeq);
            this.pnlSlotFetchSeq.Location = new System.Drawing.Point(319, 103);
            this.pnlSlotFetchSeq.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlSlotFetchSeq.Name = "pnlSlotFetchSeq";
            this.pnlSlotFetchSeq.Size = new System.Drawing.Size(310, 30);
            this.pnlSlotFetchSeq.TabIndex = 170;
            // 
            // txtSlotFetchSeq
            // 
            this.txtSlotFetchSeq.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtSlotFetchSeq.Location = new System.Drawing.Point(150, 1);
            this.txtSlotFetchSeq.Name = "txtSlotFetchSeq";
            this.txtSlotFetchSeq.ReadOnly = true;
            this.txtSlotFetchSeq.Size = new System.Drawing.Size(155, 25);
            this.txtSlotFetchSeq.TabIndex = 135;
            // 
            // lblSlotFetchSeq
            // 
            this.lblSlotFetchSeq.BackColor = System.Drawing.Color.Black;
            this.lblSlotFetchSeq.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblSlotFetchSeq.ForeColor = System.Drawing.Color.White;
            this.lblSlotFetchSeq.Location = new System.Drawing.Point(3, 1);
            this.lblSlotFetchSeq.Name = "lblSlotFetchSeq";
            this.lblSlotFetchSeq.Size = new System.Drawing.Size(150, 25);
            this.lblSlotFetchSeq.TabIndex = 136;
            this.lblSlotFetchSeq.Text = "Slot Fetch Sequence";
            this.lblSlotFetchSeq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlSlotStoreSeq
            // 
            this.pnlSlotStoreSeq.Controls.Add(this.txtSlotStoreSeq);
            this.pnlSlotStoreSeq.Controls.Add(this.lblSlotStoreSeq);
            this.pnlSlotStoreSeq.Location = new System.Drawing.Point(3, 137);
            this.pnlSlotStoreSeq.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlSlotStoreSeq.Name = "pnlSlotStoreSeq";
            this.pnlSlotStoreSeq.Size = new System.Drawing.Size(310, 30);
            this.pnlSlotStoreSeq.TabIndex = 184;
            // 
            // txtSlotStoreSeq
            // 
            this.txtSlotStoreSeq.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtSlotStoreSeq.Location = new System.Drawing.Point(150, 1);
            this.txtSlotStoreSeq.Name = "txtSlotStoreSeq";
            this.txtSlotStoreSeq.ReadOnly = true;
            this.txtSlotStoreSeq.Size = new System.Drawing.Size(155, 25);
            this.txtSlotStoreSeq.TabIndex = 135;
            // 
            // lblSlotStoreSeq
            // 
            this.lblSlotStoreSeq.BackColor = System.Drawing.Color.Black;
            this.lblSlotStoreSeq.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblSlotStoreSeq.ForeColor = System.Drawing.Color.White;
            this.lblSlotStoreSeq.Location = new System.Drawing.Point(3, 1);
            this.lblSlotStoreSeq.Name = "lblSlotStoreSeq";
            this.lblSlotStoreSeq.Size = new System.Drawing.Size(150, 25);
            this.lblSlotStoreSeq.TabIndex = 136;
            this.lblSlotStoreSeq.Text = "Slot Store Sequence";
            this.lblSlotStoreSeq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlExchangeType
            // 
            this.pnlExchangeType.Controls.Add(this.txtExchangeType);
            this.pnlExchangeType.Controls.Add(this.lblExchangeType);
            this.pnlExchangeType.Location = new System.Drawing.Point(319, 137);
            this.pnlExchangeType.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlExchangeType.Name = "pnlExchangeType";
            this.pnlExchangeType.Size = new System.Drawing.Size(310, 30);
            this.pnlExchangeType.TabIndex = 185;
            // 
            // txtExchangeType
            // 
            this.txtExchangeType.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtExchangeType.Location = new System.Drawing.Point(150, 1);
            this.txtExchangeType.Name = "txtExchangeType";
            this.txtExchangeType.ReadOnly = true;
            this.txtExchangeType.Size = new System.Drawing.Size(155, 25);
            this.txtExchangeType.TabIndex = 135;
            // 
            // lblExchangeType
            // 
            this.lblExchangeType.BackColor = System.Drawing.Color.Black;
            this.lblExchangeType.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblExchangeType.ForeColor = System.Drawing.Color.White;
            this.lblExchangeType.Location = new System.Drawing.Point(3, 1);
            this.lblExchangeType.Name = "lblExchangeType";
            this.lblExchangeType.Size = new System.Drawing.Size(150, 25);
            this.lblExchangeType.TabIndex = 136;
            this.lblExchangeType.Text = "Exchange Type";
            this.lblExchangeType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlEQRobotIfType
            // 
            this.pnlEQRobotIfType.Controls.Add(this.txtEQRobotIfType);
            this.pnlEQRobotIfType.Controls.Add(this.lblEQRobotIfType);
            this.pnlEQRobotIfType.Location = new System.Drawing.Point(3, 171);
            this.pnlEQRobotIfType.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlEQRobotIfType.Name = "pnlEQRobotIfType";
            this.pnlEQRobotIfType.Size = new System.Drawing.Size(310, 30);
            this.pnlEQRobotIfType.TabIndex = 171;
            // 
            // txtEQRobotIfType
            // 
            this.txtEQRobotIfType.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtEQRobotIfType.Location = new System.Drawing.Point(150, 1);
            this.txtEQRobotIfType.Name = "txtEQRobotIfType";
            this.txtEQRobotIfType.ReadOnly = true;
            this.txtEQRobotIfType.Size = new System.Drawing.Size(155, 25);
            this.txtEQRobotIfType.TabIndex = 135;
            // 
            // lblEQRobotIfType
            // 
            this.lblEQRobotIfType.BackColor = System.Drawing.Color.Black;
            this.lblEQRobotIfType.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblEQRobotIfType.ForeColor = System.Drawing.Color.White;
            this.lblEQRobotIfType.Location = new System.Drawing.Point(3, 1);
            this.lblEQRobotIfType.Name = "lblEQRobotIfType";
            this.lblEQRobotIfType.Size = new System.Drawing.Size(150, 25);
            this.lblEQRobotIfType.TabIndex = 136;
            this.lblEQRobotIfType.Text = "EQRobotIfType";
            this.lblEQRobotIfType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlArmUsePriority
            // 
            this.pnlArmUsePriority.Controls.Add(this.txtArmUsePriority);
            this.pnlArmUsePriority.Controls.Add(this.lblArmUsePriority);
            this.pnlArmUsePriority.Location = new System.Drawing.Point(319, 171);
            this.pnlArmUsePriority.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlArmUsePriority.Name = "pnlArmUsePriority";
            this.pnlArmUsePriority.Size = new System.Drawing.Size(310, 30);
            this.pnlArmUsePriority.TabIndex = 169;
            // 
            // txtArmUsePriority
            // 
            this.txtArmUsePriority.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtArmUsePriority.Location = new System.Drawing.Point(150, 1);
            this.txtArmUsePriority.Name = "txtArmUsePriority";
            this.txtArmUsePriority.ReadOnly = true;
            this.txtArmUsePriority.Size = new System.Drawing.Size(155, 25);
            this.txtArmUsePriority.TabIndex = 131;
            // 
            // lblArmUsePriority
            // 
            this.lblArmUsePriority.BackColor = System.Drawing.Color.Black;
            this.lblArmUsePriority.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblArmUsePriority.ForeColor = System.Drawing.Color.White;
            this.lblArmUsePriority.Location = new System.Drawing.Point(3, 1);
            this.lblArmUsePriority.Name = "lblArmUsePriority";
            this.lblArmUsePriority.Size = new System.Drawing.Size(150, 25);
            this.lblArmUsePriority.TabIndex = 132;
            this.lblArmUsePriority.Text = "Arm Use Priority";
            this.lblArmUsePriority.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlUseSpecificArm
            // 
            this.pnlUseSpecificArm.Controls.Add(this.txtUseSpecificArm);
            this.pnlUseSpecificArm.Controls.Add(this.lblUseSpecificArm);
            this.pnlUseSpecificArm.Location = new System.Drawing.Point(3, 205);
            this.pnlUseSpecificArm.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlUseSpecificArm.Name = "pnlUseSpecificArm";
            this.pnlUseSpecificArm.Size = new System.Drawing.Size(310, 30);
            this.pnlUseSpecificArm.TabIndex = 168;
            // 
            // txtUseSpecificArm
            // 
            this.txtUseSpecificArm.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtUseSpecificArm.Location = new System.Drawing.Point(150, 1);
            this.txtUseSpecificArm.Name = "txtUseSpecificArm";
            this.txtUseSpecificArm.ReadOnly = true;
            this.txtUseSpecificArm.Size = new System.Drawing.Size(155, 25);
            this.txtUseSpecificArm.TabIndex = 131;
            // 
            // lblUseSpecificArm
            // 
            this.lblUseSpecificArm.BackColor = System.Drawing.Color.Black;
            this.lblUseSpecificArm.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblUseSpecificArm.ForeColor = System.Drawing.Color.White;
            this.lblUseSpecificArm.Location = new System.Drawing.Point(3, 1);
            this.lblUseSpecificArm.Name = "lblUseSpecificArm";
            this.lblUseSpecificArm.Size = new System.Drawing.Size(150, 25);
            this.lblUseSpecificArm.TabIndex = 132;
            this.lblUseSpecificArm.Text = "Use Specific Arm";
            this.lblUseSpecificArm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlStageJobDataTrxName
            // 
            this.pnlStageJobDataTrxName.Controls.Add(this.txtStageJobDataTrxName);
            this.pnlStageJobDataTrxName.Controls.Add(this.lblStageJobDataTrxName);
            this.pnlStageJobDataTrxName.Location = new System.Drawing.Point(3, 239);
            this.pnlStageJobDataTrxName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlStageJobDataTrxName.Name = "pnlStageJobDataTrxName";
            this.pnlStageJobDataTrxName.Size = new System.Drawing.Size(626, 30);
            this.pnlStageJobDataTrxName.TabIndex = 167;
            // 
            // txtStageJobDataTrxName
            // 
            this.txtStageJobDataTrxName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtStageJobDataTrxName.Location = new System.Drawing.Point(326, 1);
            this.txtStageJobDataTrxName.Name = "txtStageJobDataTrxName";
            this.txtStageJobDataTrxName.ReadOnly = true;
            this.txtStageJobDataTrxName.Size = new System.Drawing.Size(295, 25);
            this.txtStageJobDataTrxName.TabIndex = 129;
            // 
            // lblStageJobDataTrxName
            // 
            this.lblStageJobDataTrxName.BackColor = System.Drawing.Color.Black;
            this.lblStageJobDataTrxName.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblStageJobDataTrxName.ForeColor = System.Drawing.Color.White;
            this.lblStageJobDataTrxName.Location = new System.Drawing.Point(3, 1);
            this.lblStageJobDataTrxName.Name = "lblStageJobDataTrxName";
            this.lblStageJobDataTrxName.Size = new System.Drawing.Size(325, 25);
            this.lblStageJobDataTrxName.TabIndex = 130;
            this.lblStageJobDataTrxName.Text = "Stage\'s PLC Job Data";
            this.lblStageJobDataTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlStageReportTrxName
            // 
            this.pnlStageReportTrxName.Controls.Add(this.txtStageReportTrxName);
            this.pnlStageReportTrxName.Controls.Add(this.lblStageReportTrxName);
            this.pnlStageReportTrxName.Location = new System.Drawing.Point(3, 273);
            this.pnlStageReportTrxName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlStageReportTrxName.Name = "pnlStageReportTrxName";
            this.pnlStageReportTrxName.Size = new System.Drawing.Size(626, 30);
            this.pnlStageReportTrxName.TabIndex = 165;
            // 
            // txtStageReportTrxName
            // 
            this.txtStageReportTrxName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtStageReportTrxName.Location = new System.Drawing.Point(326, 1);
            this.txtStageReportTrxName.Name = "txtStageReportTrxName";
            this.txtStageReportTrxName.ReadOnly = true;
            this.txtStageReportTrxName.Size = new System.Drawing.Size(295, 25);
            this.txtStageReportTrxName.TabIndex = 117;
            // 
            // lblStageReportTrxName
            // 
            this.lblStageReportTrxName.BackColor = System.Drawing.Color.Black;
            this.lblStageReportTrxName.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblStageReportTrxName.ForeColor = System.Drawing.Color.White;
            this.lblStageReportTrxName.Location = new System.Drawing.Point(3, 1);
            this.lblStageReportTrxName.Name = "lblStageReportTrxName";
            this.lblStageReportTrxName.Size = new System.Drawing.Size(325, 25);
            this.lblStageReportTrxName.TabIndex = 119;
            this.lblStageReportTrxName.Text = "Stage\'s PLC Trx Name";
            this.lblStageReportTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlUpstreamPathTrxName
            // 
            this.pnlUpstreamPathTrxName.Controls.Add(this.txtUpstreamPathTrxName);
            this.pnlUpstreamPathTrxName.Controls.Add(this.lblUpstreamPathTrxName);
            this.pnlUpstreamPathTrxName.Location = new System.Drawing.Point(3, 307);
            this.pnlUpstreamPathTrxName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlUpstreamPathTrxName.Name = "pnlUpstreamPathTrxName";
            this.pnlUpstreamPathTrxName.Size = new System.Drawing.Size(626, 30);
            this.pnlUpstreamPathTrxName.TabIndex = 172;
            // 
            // txtUpstreamPathTrxName
            // 
            this.txtUpstreamPathTrxName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtUpstreamPathTrxName.Location = new System.Drawing.Point(326, 1);
            this.txtUpstreamPathTrxName.Name = "txtUpstreamPathTrxName";
            this.txtUpstreamPathTrxName.ReadOnly = true;
            this.txtUpstreamPathTrxName.Size = new System.Drawing.Size(295, 25);
            this.txtUpstreamPathTrxName.TabIndex = 139;
            // 
            // lblUpstreamPathTrxName
            // 
            this.lblUpstreamPathTrxName.BackColor = System.Drawing.Color.Black;
            this.lblUpstreamPathTrxName.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblUpstreamPathTrxName.ForeColor = System.Drawing.Color.White;
            this.lblUpstreamPathTrxName.Location = new System.Drawing.Point(3, 1);
            this.lblUpstreamPathTrxName.Name = "lblUpstreamPathTrxName";
            this.lblUpstreamPathTrxName.Size = new System.Drawing.Size(325, 25);
            this.lblUpstreamPathTrxName.TabIndex = 140;
            this.lblUpstreamPathTrxName.Text = "Upstream Path Trx Name";
            this.lblUpstreamPathTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlUpstreamSendPathTrxName
            // 
            this.pnlUpstreamSendPathTrxName.Controls.Add(this.txtUpstreamSendPathTrxName);
            this.pnlUpstreamSendPathTrxName.Controls.Add(this.lblUpstreamSendPathTrxName);
            this.pnlUpstreamSendPathTrxName.Location = new System.Drawing.Point(3, 341);
            this.pnlUpstreamSendPathTrxName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlUpstreamSendPathTrxName.Name = "pnlUpstreamSendPathTrxName";
            this.pnlUpstreamSendPathTrxName.Size = new System.Drawing.Size(626, 30);
            this.pnlUpstreamSendPathTrxName.TabIndex = 181;
            // 
            // txtUpstreamSendPathTrxName
            // 
            this.txtUpstreamSendPathTrxName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtUpstreamSendPathTrxName.Location = new System.Drawing.Point(326, 1);
            this.txtUpstreamSendPathTrxName.Name = "txtUpstreamSendPathTrxName";
            this.txtUpstreamSendPathTrxName.ReadOnly = true;
            this.txtUpstreamSendPathTrxName.Size = new System.Drawing.Size(295, 25);
            this.txtUpstreamSendPathTrxName.TabIndex = 157;
            // 
            // lblUpstreamSendPathTrxName
            // 
            this.lblUpstreamSendPathTrxName.BackColor = System.Drawing.Color.Black;
            this.lblUpstreamSendPathTrxName.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblUpstreamSendPathTrxName.ForeColor = System.Drawing.Color.White;
            this.lblUpstreamSendPathTrxName.Location = new System.Drawing.Point(3, 1);
            this.lblUpstreamSendPathTrxName.Name = "lblUpstreamSendPathTrxName";
            this.lblUpstreamSendPathTrxName.Size = new System.Drawing.Size(325, 25);
            this.lblUpstreamSendPathTrxName.TabIndex = 158;
            this.lblUpstreamSendPathTrxName.Text = "Upstream Send Jobdata Path Trx Name";
            this.lblUpstreamSendPathTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlDownstreamPathTrxName
            // 
            this.pnlDownstreamPathTrxName.Controls.Add(this.txtDownstreamPathTrxName);
            this.pnlDownstreamPathTrxName.Controls.Add(this.lblDownstreamPathTrxName);
            this.pnlDownstreamPathTrxName.Location = new System.Drawing.Point(3, 375);
            this.pnlDownstreamPathTrxName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlDownstreamPathTrxName.Name = "pnlDownstreamPathTrxName";
            this.pnlDownstreamPathTrxName.Size = new System.Drawing.Size(626, 30);
            this.pnlDownstreamPathTrxName.TabIndex = 173;
            // 
            // txtDownstreamPathTrxName
            // 
            this.txtDownstreamPathTrxName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtDownstreamPathTrxName.Location = new System.Drawing.Point(326, 1);
            this.txtDownstreamPathTrxName.Name = "txtDownstreamPathTrxName";
            this.txtDownstreamPathTrxName.ReadOnly = true;
            this.txtDownstreamPathTrxName.Size = new System.Drawing.Size(295, 25);
            this.txtDownstreamPathTrxName.TabIndex = 141;
            // 
            // lblDownstreamPathTrxName
            // 
            this.lblDownstreamPathTrxName.BackColor = System.Drawing.Color.Black;
            this.lblDownstreamPathTrxName.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblDownstreamPathTrxName.ForeColor = System.Drawing.Color.White;
            this.lblDownstreamPathTrxName.Location = new System.Drawing.Point(3, 1);
            this.lblDownstreamPathTrxName.Name = "lblDownstreamPathTrxName";
            this.lblDownstreamPathTrxName.Size = new System.Drawing.Size(325, 25);
            this.lblDownstreamPathTrxName.TabIndex = 142;
            this.lblDownstreamPathTrxName.Text = "Downstream Path Trx Name";
            this.lblDownstreamPathTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlDownstreamReceivePathTrxName
            // 
            this.pnlDownstreamReceivePathTrxName.Controls.Add(this.txtDownstreamReceivePathTrxName);
            this.pnlDownstreamReceivePathTrxName.Controls.Add(this.lblDownstreamReceivePathTrxName);
            this.pnlDownstreamReceivePathTrxName.Location = new System.Drawing.Point(3, 409);
            this.pnlDownstreamReceivePathTrxName.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlDownstreamReceivePathTrxName.Name = "pnlDownstreamReceivePathTrxName";
            this.pnlDownstreamReceivePathTrxName.Size = new System.Drawing.Size(626, 30);
            this.pnlDownstreamReceivePathTrxName.TabIndex = 182;
            // 
            // txtDownstreamReceivePathTrxName
            // 
            this.txtDownstreamReceivePathTrxName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtDownstreamReceivePathTrxName.Location = new System.Drawing.Point(326, 1);
            this.txtDownstreamReceivePathTrxName.Name = "txtDownstreamReceivePathTrxName";
            this.txtDownstreamReceivePathTrxName.ReadOnly = true;
            this.txtDownstreamReceivePathTrxName.Size = new System.Drawing.Size(295, 25);
            this.txtDownstreamReceivePathTrxName.TabIndex = 159;
            // 
            // lblDownstreamReceivePathTrxName
            // 
            this.lblDownstreamReceivePathTrxName.BackColor = System.Drawing.Color.Black;
            this.lblDownstreamReceivePathTrxName.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblDownstreamReceivePathTrxName.ForeColor = System.Drawing.Color.White;
            this.lblDownstreamReceivePathTrxName.Location = new System.Drawing.Point(3, 1);
            this.lblDownstreamReceivePathTrxName.Name = "lblDownstreamReceivePathTrxName";
            this.lblDownstreamReceivePathTrxName.Size = new System.Drawing.Size(325, 25);
            this.lblDownstreamReceivePathTrxName.TabIndex = 160;
            this.lblDownstreamReceivePathTrxName.Text = "Downstream Receive Jobdata Path Trx Name";
            this.lblDownstreamReceivePathTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlIsMultiSlot
            // 
            this.pnlIsMultiSlot.Controls.Add(this.chkIsMultiSlot);
            this.pnlIsMultiSlot.Location = new System.Drawing.Point(3, 443);
            this.pnlIsMultiSlot.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlIsMultiSlot.Name = "pnlIsMultiSlot";
            this.pnlIsMultiSlot.Size = new System.Drawing.Size(200, 30);
            this.pnlIsMultiSlot.TabIndex = 174;
            // 
            // chkIsMultiSlot
            // 
            this.chkIsMultiSlot.Enabled = false;
            this.chkIsMultiSlot.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkIsMultiSlot.Location = new System.Drawing.Point(10, 2);
            this.chkIsMultiSlot.Name = "chkIsMultiSlot";
            this.chkIsMultiSlot.Size = new System.Drawing.Size(190, 25);
            this.chkIsMultiSlot.TabIndex = 143;
            this.chkIsMultiSlot.Text = "Is multi  Slot";
            this.chkIsMultiSlot.UseVisualStyleBackColor = true;
            // 
            // pnlRecipeCheckFlag
            // 
            this.pnlRecipeCheckFlag.Controls.Add(this.chkRecipeCheckFlag);
            this.pnlRecipeCheckFlag.Location = new System.Drawing.Point(209, 443);
            this.pnlRecipeCheckFlag.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlRecipeCheckFlag.Name = "pnlRecipeCheckFlag";
            this.pnlRecipeCheckFlag.Size = new System.Drawing.Size(200, 30);
            this.pnlRecipeCheckFlag.TabIndex = 175;
            // 
            // chkRecipeCheckFlag
            // 
            this.chkRecipeCheckFlag.Enabled = false;
            this.chkRecipeCheckFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkRecipeCheckFlag.Location = new System.Drawing.Point(10, 2);
            this.chkRecipeCheckFlag.Name = "chkRecipeCheckFlag";
            this.chkRecipeCheckFlag.Size = new System.Drawing.Size(190, 25);
            this.chkRecipeCheckFlag.TabIndex = 145;
            this.chkRecipeCheckFlag.Text = "RecipeCheck";
            this.chkRecipeCheckFlag.UseVisualStyleBackColor = true;
            // 
            // pnlDummyCheckFlag
            // 
            this.pnlDummyCheckFlag.Controls.Add(this.chkDummyCheckFlag);
            this.pnlDummyCheckFlag.Location = new System.Drawing.Point(415, 443);
            this.pnlDummyCheckFlag.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlDummyCheckFlag.Name = "pnlDummyCheckFlag";
            this.pnlDummyCheckFlag.Size = new System.Drawing.Size(200, 30);
            this.pnlDummyCheckFlag.TabIndex = 176;
            // 
            // chkDummyCheckFlag
            // 
            this.chkDummyCheckFlag.Enabled = false;
            this.chkDummyCheckFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDummyCheckFlag.Location = new System.Drawing.Point(10, 2);
            this.chkDummyCheckFlag.Name = "chkDummyCheckFlag";
            this.chkDummyCheckFlag.Size = new System.Drawing.Size(190, 25);
            this.chkDummyCheckFlag.TabIndex = 147;
            this.chkDummyCheckFlag.Text = "DummyCheck";
            this.chkDummyCheckFlag.UseVisualStyleBackColor = true;
            // 
            // pnlGetReadyFlag
            // 
            this.pnlGetReadyFlag.Controls.Add(this.chkGetReadyFlag);
            this.pnlGetReadyFlag.Location = new System.Drawing.Point(3, 477);
            this.pnlGetReadyFlag.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlGetReadyFlag.Name = "pnlGetReadyFlag";
            this.pnlGetReadyFlag.Size = new System.Drawing.Size(200, 30);
            this.pnlGetReadyFlag.TabIndex = 177;
            // 
            // chkGetReadyFlag
            // 
            this.chkGetReadyFlag.Enabled = false;
            this.chkGetReadyFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkGetReadyFlag.Location = new System.Drawing.Point(10, 2);
            this.chkGetReadyFlag.Name = "chkGetReadyFlag";
            this.chkGetReadyFlag.Size = new System.Drawing.Size(190, 25);
            this.chkGetReadyFlag.TabIndex = 149;
            this.chkGetReadyFlag.Text = "Get Ready Flag";
            this.chkGetReadyFlag.UseVisualStyleBackColor = true;
            // 
            // pnlPutReadyFlag
            // 
            this.pnlPutReadyFlag.Controls.Add(this.chkPutReadyFlag);
            this.pnlPutReadyFlag.Location = new System.Drawing.Point(209, 477);
            this.pnlPutReadyFlag.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlPutReadyFlag.Name = "pnlPutReadyFlag";
            this.pnlPutReadyFlag.Size = new System.Drawing.Size(200, 30);
            this.pnlPutReadyFlag.TabIndex = 178;
            // 
            // chkPutReadyFlag
            // 
            this.chkPutReadyFlag.Enabled = false;
            this.chkPutReadyFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkPutReadyFlag.Location = new System.Drawing.Point(10, 2);
            this.chkPutReadyFlag.Name = "chkPutReadyFlag";
            this.chkPutReadyFlag.Size = new System.Drawing.Size(190, 25);
            this.chkPutReadyFlag.TabIndex = 151;
            this.chkPutReadyFlag.Text = "Put Ready Flag";
            this.chkPutReadyFlag.UseVisualStyleBackColor = true;
            // 
            // pnlPrefetchFlag
            // 
            this.pnlPrefetchFlag.Controls.Add(this.chkPrefetchFlag);
            this.pnlPrefetchFlag.Location = new System.Drawing.Point(415, 477);
            this.pnlPrefetchFlag.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlPrefetchFlag.Name = "pnlPrefetchFlag";
            this.pnlPrefetchFlag.Size = new System.Drawing.Size(200, 30);
            this.pnlPrefetchFlag.TabIndex = 179;
            // 
            // chkPrefetchFlag
            // 
            this.chkPrefetchFlag.Enabled = false;
            this.chkPrefetchFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkPrefetchFlag.Location = new System.Drawing.Point(10, 2);
            this.chkPrefetchFlag.Name = "chkPrefetchFlag";
            this.chkPrefetchFlag.Size = new System.Drawing.Size(190, 25);
            this.chkPrefetchFlag.TabIndex = 153;
            this.chkPrefetchFlag.Text = "Prefetch Flag";
            this.chkPrefetchFlag.UseVisualStyleBackColor = true;
            // 
            // pnlwaitFrontFlag
            // 
            this.pnlwaitFrontFlag.Controls.Add(this.chkwaitFrontFlag);
            this.pnlwaitFrontFlag.Location = new System.Drawing.Point(3, 511);
            this.pnlwaitFrontFlag.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlwaitFrontFlag.Name = "pnlwaitFrontFlag";
            this.pnlwaitFrontFlag.Size = new System.Drawing.Size(200, 30);
            this.pnlwaitFrontFlag.TabIndex = 164;
            // 
            // chkwaitFrontFlag
            // 
            this.chkwaitFrontFlag.Enabled = false;
            this.chkwaitFrontFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkwaitFrontFlag.Location = new System.Drawing.Point(10, 2);
            this.chkwaitFrontFlag.Name = "chkwaitFrontFlag";
            this.chkwaitFrontFlag.Size = new System.Drawing.Size(190, 25);
            this.chkwaitFrontFlag.TabIndex = 113;
            this.chkwaitFrontFlag.Text = "Support Wait Front Flag";
            this.chkwaitFrontFlag.UseVisualStyleBackColor = true;
            // 
            // pnlStageEnabled
            // 
            this.pnlStageEnabled.Controls.Add(this.chkStageEnabled);
            this.pnlStageEnabled.Location = new System.Drawing.Point(209, 511);
            this.pnlStageEnabled.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlStageEnabled.Name = "pnlStageEnabled";
            this.pnlStageEnabled.Size = new System.Drawing.Size(200, 30);
            this.pnlStageEnabled.TabIndex = 169;
            // 
            // chkStageEnabled
            // 
            this.chkStageEnabled.Enabled = false;
            this.chkStageEnabled.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkStageEnabled.Location = new System.Drawing.Point(10, 2);
            this.chkStageEnabled.Name = "chkStageEnabled";
            this.chkStageEnabled.Size = new System.Drawing.Size(190, 25);
            this.chkStageEnabled.TabIndex = 135;
            this.chkStageEnabled.Text = "Stage Enabled";
            this.chkStageEnabled.UseVisualStyleBackColor = true;
            // 
            // pnlRTCReworkFlag
            // 
            this.pnlRTCReworkFlag.Controls.Add(this.chkRTCReworkFlag);
            this.pnlRTCReworkFlag.Location = new System.Drawing.Point(415, 511);
            this.pnlRTCReworkFlag.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.pnlRTCReworkFlag.Name = "pnlRTCReworkFlag";
            this.pnlRTCReworkFlag.Size = new System.Drawing.Size(200, 30);
            this.pnlRTCReworkFlag.TabIndex = 186;
            // 
            // chkRTCReworkFlag
            // 
            this.chkRTCReworkFlag.Enabled = false;
            this.chkRTCReworkFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkRTCReworkFlag.Location = new System.Drawing.Point(10, 2);
            this.chkRTCReworkFlag.Name = "chkRTCReworkFlag";
            this.chkRTCReworkFlag.Size = new System.Drawing.Size(190, 25);
            this.chkRTCReworkFlag.TabIndex = 135;
            this.chkRTCReworkFlag.Text = "RTC Rework Flag";
            this.chkRTCReworkFlag.UseVisualStyleBackColor = true;
            // 
            // grbData
            // 
            this.grbData.Controls.Add(this.dgvData);
            this.grbData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbData.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbData.Location = new System.Drawing.Point(3, 53);
            this.grbData.Name = "grbData";
            this.grbData.Size = new System.Drawing.Size(356, 570);
            this.grbData.TabIndex = 23;
            this.grbData.TabStop = false;
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
            this.colServerName,
            this.colRobotName,
            this.colStageEnabled,
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
            this.colSlotFetchSeq,
            this.colSlotStoreSeq,
            this.colExchangeType,
            this.colEQRobotIfTypre});
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 23);
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(350, 544);
            this.dgvData.TabIndex = 12;
            this.dgvData.DataSourceChanged += new System.EventHandler(this.dgvData_DataSourceChanged);
            this.dgvData.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvData_CellClick);
            this.dgvData.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvData_ColumnHeaderMouseClick);
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
            this.colRobotName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colRobotName.DataPropertyName = "ROBOTNAME";
            this.colRobotName.HeaderText = "Robot Name";
            this.colRobotName.MinimumWidth = 130;
            this.colRobotName.Name = "colRobotName";
            this.colRobotName.ReadOnly = true;
            this.colRobotName.Visible = false;
            // 
            // colStageEnabled
            // 
            this.colStageEnabled.DataPropertyName = "ISENABLED";
            this.colStageEnabled.FalseValue = "N";
            this.colStageEnabled.HeaderText = "Enable";
            this.colStageEnabled.Name = "colStageEnabled";
            this.colStageEnabled.ReadOnly = true;
            this.colStageEnabled.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colStageEnabled.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colStageEnabled.TrueValue = "Y";
            this.colStageEnabled.Width = 55;
            // 
            // colStageID
            // 
            this.colStageID.DataPropertyName = "STAGEID";
            this.colStageID.HeaderText = "Stage ID";
            this.colStageID.Name = "colStageID";
            this.colStageID.ReadOnly = true;
            this.colStageID.Width = 90;
            // 
            // colStageName
            // 
            this.colStageName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStageName.DataPropertyName = "STAGENAME";
            this.colStageName.HeaderText = "Stage Name";
            this.colStageName.Name = "colStageName";
            this.colStageName.ReadOnly = true;
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
            this.colLocalNo.Width = 90;
            // 
            // colStageIDByNode
            // 
            this.colStageIDByNode.DataPropertyName = "STAGEIDBYNODE";
            this.colStageIDByNode.HeaderText = "Stage ID By Local";
            this.colStageIDByNode.Name = "colStageIDByNode";
            this.colStageIDByNode.ReadOnly = true;
            this.colStageIDByNode.Visible = false;
            // 
            // colStageType
            // 
            this.colStageType.DataPropertyName = "STAGETYPE";
            this.colStageType.HeaderText = "Stage Type";
            this.colStageType.Name = "colStageType";
            this.colStageType.ReadOnly = true;
            this.colStageType.Visible = false;
            // 
            // colPriority
            // 
            this.colPriority.DataPropertyName = "PRIORITY";
            this.colPriority.HeaderText = "Priority";
            this.colPriority.Name = "colPriority";
            this.colPriority.ReadOnly = true;
            this.colPriority.Visible = false;
            // 
            // colStageReportTrxName
            // 
            this.colStageReportTrxName.DataPropertyName = "STAGEREPORTTRXNAME";
            this.colStageReportTrxName.HeaderText = "ReportTrxName";
            this.colStageReportTrxName.Name = "colStageReportTrxName";
            this.colStageReportTrxName.ReadOnly = true;
            this.colStageReportTrxName.Visible = false;
            // 
            // colStageJobDataTrxName
            // 
            this.colStageJobDataTrxName.DataPropertyName = "STAGEJOBDATATRXNAME";
            this.colStageJobDataTrxName.HeaderText = "JobDataTrxName";
            this.colStageJobDataTrxName.Name = "colStageJobDataTrxName";
            this.colStageJobDataTrxName.ReadOnly = true;
            this.colStageJobDataTrxName.Visible = false;
            // 
            // colIsMultiSlot
            // 
            this.colIsMultiSlot.DataPropertyName = "ISMULTISLOT";
            this.colIsMultiSlot.FalseValue = "N";
            this.colIsMultiSlot.HeaderText = "Is Multi Slot";
            this.colIsMultiSlot.Name = "colIsMultiSlot";
            this.colIsMultiSlot.ReadOnly = true;
            this.colIsMultiSlot.TrueValue = "Y";
            this.colIsMultiSlot.Visible = false;
            // 
            // colSlotMaxCount
            // 
            this.colSlotMaxCount.DataPropertyName = "SLOTMAXCOUNT";
            this.colSlotMaxCount.HeaderText = "Slot Max Count";
            this.colSlotMaxCount.Name = "colSlotMaxCount";
            this.colSlotMaxCount.ReadOnly = true;
            this.colSlotMaxCount.Visible = false;
            // 
            // colRecipeCheckFlag
            // 
            this.colRecipeCheckFlag.DataPropertyName = "RECIPECHENCKFLAG";
            this.colRecipeCheckFlag.FalseValue = "N";
            this.colRecipeCheckFlag.HeaderText = "RecipeCheck";
            this.colRecipeCheckFlag.Name = "colRecipeCheckFlag";
            this.colRecipeCheckFlag.ReadOnly = true;
            this.colRecipeCheckFlag.TrueValue = "Y";
            this.colRecipeCheckFlag.Visible = false;
            // 
            // colDummyCheckFlag
            // 
            this.colDummyCheckFlag.DataPropertyName = "DUMMYCHECKFLAG";
            this.colDummyCheckFlag.FalseValue = "N";
            this.colDummyCheckFlag.HeaderText = "DummyCheck";
            this.colDummyCheckFlag.Name = "colDummyCheckFlag";
            this.colDummyCheckFlag.ReadOnly = true;
            this.colDummyCheckFlag.TrueValue = "Y";
            this.colDummyCheckFlag.Visible = false;
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
            this.colGetReadyFlag.Visible = false;
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
            this.colPutReadyFlag.Visible = false;
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
            this.colPrefetchFlag.Visible = false;
            this.colPrefetchFlag.Width = 150;
            // 
            // colRTCReworkFlag
            // 
            this.colRTCReworkFlag.DataPropertyName = "RTCREWORKFLAG";
            this.colRTCReworkFlag.HeaderText = "RTC Rework Flag";
            this.colRTCReworkFlag.Name = "colRTCReworkFlag";
            this.colRTCReworkFlag.ReadOnly = true;
            this.colRTCReworkFlag.Visible = false;
            // 
            // colSupportWaitFront
            // 
            this.colSupportWaitFront.DataPropertyName = "WAITFRONTFLAG";
            this.colSupportWaitFront.FalseValue = "N";
            this.colSupportWaitFront.HeaderText = "Support Wait Front Flag";
            this.colSupportWaitFront.Name = "colSupportWaitFront";
            this.colSupportWaitFront.ReadOnly = true;
            this.colSupportWaitFront.TrueValue = "Y";
            this.colSupportWaitFront.Visible = false;
            // 
            // colUpstreamPathTrxName
            // 
            this.colUpstreamPathTrxName.DataPropertyName = "UPSTREAMPATHTRXNAME";
            this.colUpstreamPathTrxName.HeaderText = "Upstream Path Transaction Name";
            this.colUpstreamPathTrxName.Name = "colUpstreamPathTrxName";
            this.colUpstreamPathTrxName.ReadOnly = true;
            this.colUpstreamPathTrxName.Visible = false;
            // 
            // colUpstreamSendPathTrxName
            // 
            this.colUpstreamSendPathTrxName.DataPropertyName = "UPSTREAMJOBDATAPATHTRXNAME";
            this.colUpstreamSendPathTrxName.HeaderText = "Upstream Send Jobdata Path ";
            this.colUpstreamSendPathTrxName.Name = "colUpstreamSendPathTrxName";
            this.colUpstreamSendPathTrxName.ReadOnly = true;
            this.colUpstreamSendPathTrxName.Visible = false;
            // 
            // colDownstreamPathTrxName
            // 
            this.colDownstreamPathTrxName.DataPropertyName = "DOWNSTREAMPATHTRXNAME";
            this.colDownstreamPathTrxName.HeaderText = "Downstream Path Transaction Name";
            this.colDownstreamPathTrxName.Name = "colDownstreamPathTrxName";
            this.colDownstreamPathTrxName.ReadOnly = true;
            this.colDownstreamPathTrxName.Visible = false;
            // 
            // colDownstreamReceivePathTrxName
            // 
            this.colDownstreamReceivePathTrxName.DataPropertyName = "DOWNSTREAMJOBDATAPATHTRXNAME";
            this.colDownstreamReceivePathTrxName.HeaderText = "Downstream Receive Jobdata Path ";
            this.colDownstreamReceivePathTrxName.Name = "colDownstreamReceivePathTrxName";
            this.colDownstreamReceivePathTrxName.ReadOnly = true;
            this.colDownstreamReceivePathTrxName.Visible = false;
            // 
            // colTrackDataSeqList
            // 
            this.colTrackDataSeqList.DataPropertyName = "TRACKDATASEQLIST";
            this.colTrackDataSeqList.HeaderText = "TrackData Seq List";
            this.colTrackDataSeqList.Name = "colTrackDataSeqList";
            this.colTrackDataSeqList.ReadOnly = true;
            this.colTrackDataSeqList.Visible = false;
            // 
            // colCassetteType
            // 
            this.colCassetteType.DataPropertyName = "CASSETTETYPE";
            this.colCassetteType.HeaderText = "Cassette Type";
            this.colCassetteType.Name = "colCassetteType";
            this.colCassetteType.ReadOnly = true;
            this.colCassetteType.Visible = false;
            // 
            // colRemark
            // 
            this.colRemark.DataPropertyName = "REMARKS";
            this.colRemark.HeaderText = "Remark";
            this.colRemark.Name = "colRemark";
            this.colRemark.ReadOnly = true;
            this.colRemark.Visible = false;
            // 
            // colSlotFetchSeq
            // 
            this.colSlotFetchSeq.DataPropertyName = "SLOTFETCHSEQ";
            this.colSlotFetchSeq.HeaderText = "Slot Fetch Sequence";
            this.colSlotFetchSeq.Name = "colSlotFetchSeq";
            this.colSlotFetchSeq.ReadOnly = true;
            this.colSlotFetchSeq.Visible = false;
            // 
            // colSlotStoreSeq
            // 
            this.colSlotStoreSeq.DataPropertyName = "SLOTSTORESEQ";
            this.colSlotStoreSeq.HeaderText = "Slot Store Sequence";
            this.colSlotStoreSeq.Name = "colSlotStoreSeq";
            this.colSlotStoreSeq.ReadOnly = true;
            this.colSlotStoreSeq.Visible = false;
            // 
            // colExchangeType
            // 
            this.colExchangeType.DataPropertyName = "EXCHANGETYPE";
            this.colExchangeType.HeaderText = "Exchange Type";
            this.colExchangeType.Name = "colExchangeType";
            this.colExchangeType.ReadOnly = true;
            this.colExchangeType.Visible = false;
            // 
            // colEQRobotIfTypre
            // 
            this.colEQRobotIfTypre.DataPropertyName = "EQROBOTIFTYPE";
            this.colEQRobotIfTypre.HeaderText = "EQRobotifType";
            this.colEQRobotIfTypre.Name = "colEQRobotIfTypre";
            this.colEQRobotIfTypre.ReadOnly = true;
            this.colEQRobotIfTypre.Visible = false;
            // 
            // tlpButton
            // 
            this.tlpButton.ColumnCount = 1;
            this.tlpButton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButton.Controls.Add(this.pnlButtons, 0, 0);
            this.tlpButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpButton.Location = new System.Drawing.Point(1015, 53);
            this.tlpButton.Name = "tlpButton";
            this.tlpButton.RowCount = 1;
            this.tlpButton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpButton.Size = new System.Drawing.Size(122, 570);
            this.tlpButton.TabIndex = 14;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnSave);
            this.pnlButtons.Controls.Add(this.btnAdd);
            this.pnlButtons.Controls.Add(this.btnDelete);
            this.pnlButtons.Controls.Add(this.btnModify);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButtons.Location = new System.Drawing.Point(3, 3);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(116, 564);
            this.pnlButtons.TabIndex = 0;
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(3, 160);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 50);
            this.btnSave.TabIndex = 13;
            this.btnSave.Text = "Save";
            this.btnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnAdd.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(3, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(110, 50);
            this.btnAdd.TabIndex = 8;
            this.btnAdd.Text = "Add";
            this.btnAdd.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(3, 108);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(110, 50);
            this.btnDelete.TabIndex = 9;
            this.btnDelete.Text = "Delete";
            this.btnDelete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnModify
            // 
            this.btnModify.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnModify.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModify.Location = new System.Drawing.Point(3, 56);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(110, 50);
            this.btnModify.TabIndex = 10;
            this.btnModify.Text = "Modify";
            this.btnModify.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnModify.UseVisualStyleBackColor = true;
            this.btnModify.Click += new System.EventHandler(this.btnModify_Click);
            // 
            // pnlRefresh
            // 
            this.tlpBase.SetColumnSpan(this.pnlRefresh, 2);
            this.pnlRefresh.Controls.Add(this.btnRefresh);
            this.pnlRefresh.Controls.Add(this.flpRobot);
            this.pnlRefresh.Location = new System.Drawing.Point(3, 3);
            this.pnlRefresh.Name = "pnlRefresh";
            this.pnlRefresh.Size = new System.Drawing.Size(803, 44);
            this.pnlRefresh.TabIndex = 24;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(262, 7);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(72, 33);
            this.btnRefresh.TabIndex = 9;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // flpRobot
            // 
            this.flpRobot.Location = new System.Drawing.Point(3, 5);
            this.flpRobot.Name = "flpRobot";
            this.flpRobot.Size = new System.Drawing.Size(253, 33);
            this.flpRobot.TabIndex = 0;
            // 
            // FormRobotStageManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1140, 657);
            this.Name = "FormRobotStageManagement";
            this.Text = "FormRobotStageManagement";
            this.Load += new System.EventHandler(this.FormRobotStageManagement_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.grbDetail.ResumeLayout(false);
            this.flpDetail.ResumeLayout(false);
            this.pnlStageName.ResumeLayout(false);
            this.pnlStageName.PerformLayout();
            this.pnlStageIDByNode.ResumeLayout(false);
            this.pnlStageIDByNode.PerformLayout();
            this.pnlStageType.ResumeLayout(false);
            this.pnlStageType.PerformLayout();
            this.pnlPriority.ResumeLayout(false);
            this.pnlPriority.PerformLayout();
            this.pnlSlotMaxCount.ResumeLayout(false);
            this.pnlSlotMaxCount.PerformLayout();
            this.pnlTrackDataSeqList.ResumeLayout(false);
            this.pnlTrackDataSeqList.PerformLayout();
            this.pnlCassetteType.ResumeLayout(false);
            this.pnlCassetteType.PerformLayout();
            this.pnlSlotFetchSeq.ResumeLayout(false);
            this.pnlSlotFetchSeq.PerformLayout();
            this.pnlSlotStoreSeq.ResumeLayout(false);
            this.pnlSlotStoreSeq.PerformLayout();
            this.pnlExchangeType.ResumeLayout(false);
            this.pnlExchangeType.PerformLayout();
            this.pnlEQRobotIfType.ResumeLayout(false);
            this.pnlEQRobotIfType.PerformLayout();
            this.pnlArmUsePriority.ResumeLayout(false);
            this.pnlArmUsePriority.PerformLayout();
            this.pnlUseSpecificArm.ResumeLayout(false);
            this.pnlUseSpecificArm.PerformLayout();
            this.pnlStageJobDataTrxName.ResumeLayout(false);
            this.pnlStageJobDataTrxName.PerformLayout();
            this.pnlStageReportTrxName.ResumeLayout(false);
            this.pnlStageReportTrxName.PerformLayout();
            this.pnlUpstreamPathTrxName.ResumeLayout(false);
            this.pnlUpstreamPathTrxName.PerformLayout();
            this.pnlUpstreamSendPathTrxName.ResumeLayout(false);
            this.pnlUpstreamSendPathTrxName.PerformLayout();
            this.pnlDownstreamPathTrxName.ResumeLayout(false);
            this.pnlDownstreamPathTrxName.PerformLayout();
            this.pnlDownstreamReceivePathTrxName.ResumeLayout(false);
            this.pnlDownstreamReceivePathTrxName.PerformLayout();
            this.pnlIsMultiSlot.ResumeLayout(false);
            this.pnlRecipeCheckFlag.ResumeLayout(false);
            this.pnlDummyCheckFlag.ResumeLayout(false);
            this.pnlGetReadyFlag.ResumeLayout(false);
            this.pnlPutReadyFlag.ResumeLayout(false);
            this.pnlPrefetchFlag.ResumeLayout(false);
            this.pnlwaitFrontFlag.ResumeLayout(false);
            this.pnlStageEnabled.ResumeLayout(false);
            this.pnlRTCReworkFlag.ResumeLayout(false);
            this.grbData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tlpButton.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlRefresh.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.GroupBox grbData;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.TableLayoutPanel tlpButton;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnModify;
        private System.Windows.Forms.FlowLayoutPanel flpDetail;
        private System.Windows.Forms.Label lblStageIDByNode;
        private System.Windows.Forms.TextBox txtStageIDByNode;
        private System.Windows.Forms.Label lblStageType;
        private System.Windows.Forms.Label lblPriority;
        private System.Windows.Forms.TextBox txtPriority;
        private System.Windows.Forms.CheckBox chkwaitFrontFlag;
        private System.Windows.Forms.Label lblStageReportTrxName;
        private System.Windows.Forms.TextBox txtStageReportTrxName;
        private System.Windows.Forms.Label lblSlotMaxCount;
        private System.Windows.Forms.TextBox txtSlotMaxCount;
        private System.Windows.Forms.Label lblStageJobDataTrxName;
        private System.Windows.Forms.TextBox txtStageJobDataTrxName;
        private System.Windows.Forms.Label lblUseSpecificArm;
        private System.Windows.Forms.TextBox txtUseSpecificArm;
        private System.Windows.Forms.Label lblSlotFetchSeq;
        private System.Windows.Forms.TextBox txtSlotFetchSeq;
        private System.Windows.Forms.Label lblTrackDataSeqList;
        private System.Windows.Forms.TextBox txtTrackDataSeqList;
        private System.Windows.Forms.Label lblUpstreamPathTrxName;
        private System.Windows.Forms.TextBox txtUpstreamPathTrxName;
        private System.Windows.Forms.Label lblDownstreamPathTrxName;
        private System.Windows.Forms.TextBox txtDownstreamPathTrxName;
        private System.Windows.Forms.CheckBox chkIsMultiSlot;
        private System.Windows.Forms.CheckBox chkRecipeCheckFlag;
        private System.Windows.Forms.CheckBox chkDummyCheckFlag;
        private System.Windows.Forms.CheckBox chkGetReadyFlag;
        private System.Windows.Forms.CheckBox chkPutReadyFlag;
        private System.Windows.Forms.CheckBox chkPrefetchFlag;
        private System.Windows.Forms.Label lblCassetteType;
        private System.Windows.Forms.Label lblUpstreamSendPathTrxName;
        private System.Windows.Forms.TextBox txtUpstreamSendPathTrxName;
        private System.Windows.Forms.GroupBox grbDetail;
        private System.Windows.Forms.Label lblDownstreamReceivePathTrxName;
        private System.Windows.Forms.TextBox txtDownstreamReceivePathTrxName;
        private System.Windows.Forms.Panel pnlStageIDByNode;
        private System.Windows.Forms.Panel pnlStageType;
        private System.Windows.Forms.Panel pnlPriority;
        private System.Windows.Forms.Panel pnlwaitFrontFlag;
        private System.Windows.Forms.Panel pnlStageReportTrxName;
        private System.Windows.Forms.Panel pnlSlotMaxCount;
        private System.Windows.Forms.Panel pnlStageJobDataTrxName;
        private System.Windows.Forms.Panel pnlUseSpecificArm;
        private System.Windows.Forms.Panel pnlStageEnabled;
        private System.Windows.Forms.Panel pnlSlotFetchSeq;
        private System.Windows.Forms.Panel pnlTrackDataSeqList;
        private System.Windows.Forms.Panel pnlUpstreamPathTrxName;
        private System.Windows.Forms.Panel pnlDownstreamPathTrxName;
        private System.Windows.Forms.Panel pnlIsMultiSlot;
        private System.Windows.Forms.Panel pnlRecipeCheckFlag;
        private System.Windows.Forms.Panel pnlDummyCheckFlag;
        private System.Windows.Forms.Panel pnlGetReadyFlag;
        private System.Windows.Forms.Panel pnlPutReadyFlag;
        private System.Windows.Forms.Panel pnlPrefetchFlag;
        private System.Windows.Forms.Panel pnlCassetteType;
        private System.Windows.Forms.Panel pnlUpstreamSendPathTrxName;
        private System.Windows.Forms.Panel pnlDownstreamReceivePathTrxName;
        private System.Windows.Forms.TextBox txtStageType;
        private System.Windows.Forms.TextBox txtCassetteType;
        private System.Windows.Forms.Panel pnlStageName;
        private System.Windows.Forms.TextBox txtStageName;
        private System.Windows.Forms.Label lblStageName;
        private System.Windows.Forms.FlowLayoutPanel flpRobot;
        private System.Windows.Forms.Panel pnlSlotStoreSeq;
        private System.Windows.Forms.TextBox txtSlotStoreSeq;
        private System.Windows.Forms.Label lblSlotStoreSeq;
        private System.Windows.Forms.CheckBox chkStageEnabled;
        private System.Windows.Forms.Panel pnlRefresh;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Panel pnlExchangeType;
        private System.Windows.Forms.TextBox txtExchangeType;
        private System.Windows.Forms.Label lblExchangeType;
        private System.Windows.Forms.Panel pnlEQRobotIfType;
        private System.Windows.Forms.TextBox txtEQRobotIfType;
        private System.Windows.Forms.Label lblEQRobotIfType;
        private System.Windows.Forms.Panel pnlRTCReworkFlag;
        private System.Windows.Forms.CheckBox chkRTCReworkFlag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colStageEnabled;
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
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotFetchSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotStoreSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExchangeType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEQRobotIfTypre;
        private System.Windows.Forms.Panel pnlArmUsePriority;
        private System.Windows.Forms.TextBox txtArmUsePriority;
        private System.Windows.Forms.Label lblArmUsePriority;

    }
}