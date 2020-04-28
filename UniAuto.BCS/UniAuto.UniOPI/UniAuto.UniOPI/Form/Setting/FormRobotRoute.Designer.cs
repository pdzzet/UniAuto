namespace UniOPI
{
    partial class FormRobotRoute
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.pnlStageTrack = new System.Windows.Forms.Panel();
            this.dgvTarget = new System.Windows.Forms.DataGridView();
            this.colStageID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrackingData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblStageTrack = new System.Windows.Forms.Label();
            this.pnlRouteStep = new System.Windows.Forms.Panel();
            this.dgvRouteStep = new System.Windows.Forms.DataGridView();
            this.colStepID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNextStep = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colObjectKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStepRouteID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStepDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotArm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotRule = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageList = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colInputTracking = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOutputTracking = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemark = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastUpdateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnCurrentStep = new System.Windows.Forms.Button();
            this.btnNextStep = new System.Windows.Forms.Button();
            this.lblRouteStep = new System.Windows.Forms.Label();
            this.flpRadioButton = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlNormalBtn = new System.Windows.Forms.Panel();
            this.btnDeleteStep = new System.Windows.Forms.Button();
            this.btnAddRouteStep = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnDeleteRoute = new System.Windows.Forms.Button();
            this.btnSaveRoute = new System.Windows.Forms.Button();
            this.flpSetting = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlRobotArm = new System.Windows.Forms.Panel();
            this.btnArmAll = new System.Windows.Forms.Button();
            this.btnArmBoth = new System.Windows.Forms.Button();
            this.btnArmUpper = new System.Windows.Forms.Button();
            this.btnArmLow = new System.Windows.Forms.Button();
            this.lblRobotArm = new System.Windows.Forms.Label();
            this.pnlAction = new System.Windows.Forms.Panel();
            this.btnActionPut = new System.Windows.Forms.Button();
            this.btnActionGet = new System.Windows.Forms.Button();
            this.lblAction = new System.Windows.Forms.Label();
            this.pnlRobotRule = new System.Windows.Forms.Panel();
            this.btnRuleDispatch = new System.Windows.Forms.Button();
            this.btnRuleOnly = new System.Windows.Forms.Button();
            this.btnRuleSelect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlTarget = new System.Windows.Forms.Panel();
            this.chkCrossStageFlag = new System.Windows.Forms.CheckBox();
            this.btnTargetStage = new System.Windows.Forms.Button();
            this.btnTargetCassette = new System.Windows.Forms.Button();
            this.lblTarget = new System.Windows.Forms.Label();
            this.tlpTargetStage = new System.Windows.Forms.TableLayoutPanel();
            this.pnlStage = new System.Windows.Forms.Panel();
            this.lblTrack = new System.Windows.Forms.Label();
            this.flpTracking = new System.Windows.Forms.FlowLayoutPanel();
            this.flpStage = new System.Windows.Forms.FlowLayoutPanel();
            this.lblStage = new System.Windows.Forms.Label();
            this.pnlPort = new System.Windows.Forms.Panel();
            this.flpPort = new System.Windows.Forms.FlowLayoutPanel();
            this.lblPort = new System.Windows.Forms.Label();
            this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnResult_Handle = new System.Windows.Forms.Button();
            this.btnRouteStepByPass = new System.Windows.Forms.Button();
            this.btnRouteStepJump = new System.Windows.Forms.Button();
            this.btnRULE_FILTER = new System.Windows.Forms.Button();
            this.btnRULE_ORDERBY = new System.Windows.Forms.Button();
            this.btnStageSelect = new System.Windows.Forms.Button();
            this.pnlRobotRoute = new System.Windows.Forms.Panel();
            this.dgvRobotRoute = new System.Windows.Forms.DataGridView();
            this.colRoutePriority = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRouteID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblRobotRoute = new System.Windows.Forms.Label();
            this.pnlRobotPic = new System.Windows.Forms.Panel();
            this.btnNextStepSet = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.pnlStageTrack.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTarget)).BeginInit();
            this.pnlRouteStep.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRouteStep)).BeginInit();
            this.pnlNormalBtn.SuspendLayout();
            this.flpSetting.SuspendLayout();
            this.pnlRobotArm.SuspendLayout();
            this.pnlAction.SuspendLayout();
            this.pnlRobotRule.SuspendLayout();
            this.pnlTarget.SuspendLayout();
            this.tlpTargetStage.SuspendLayout();
            this.pnlStage.SuspendLayout();
            this.pnlPort.SuspendLayout();
            this.flpButtons.SuspendLayout();
            this.pnlRobotRoute.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRobotRoute)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1079, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(1139, 648);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 5;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 310F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 203F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpBase.Controls.Add(this.pnlStageTrack, 2, 1);
            this.tlpBase.Controls.Add(this.pnlRouteStep, 1, 1);
            this.tlpBase.Controls.Add(this.flpRadioButton, 0, 0);
            this.tlpBase.Controls.Add(this.pnlNormalBtn, 4, 1);
            this.tlpBase.Controls.Add(this.flpSetting, 3, 1);
            this.tlpBase.Controls.Add(this.flpButtons, 3, 3);
            this.tlpBase.Controls.Add(this.pnlRobotRoute, 0, 1);
            this.tlpBase.Controls.Add(this.pnlRobotPic, 0, 2);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 4;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 123F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F));
            this.tlpBase.Size = new System.Drawing.Size(1139, 617);
            this.tlpBase.TabIndex = 0;
            // 
            // pnlStageTrack
            // 
            this.pnlStageTrack.Controls.Add(this.dgvTarget);
            this.pnlStageTrack.Controls.Add(this.lblStageTrack);
            this.pnlStageTrack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStageTrack.Location = new System.Drawing.Point(425, 31);
            this.pnlStageTrack.Name = "pnlStageTrack";
            this.pnlStageTrack.Size = new System.Drawing.Size(197, 403);
            this.pnlStageTrack.TabIndex = 12;
            // 
            // dgvTarget
            // 
            this.dgvTarget.AllowUserToAddRows = false;
            this.dgvTarget.AllowUserToDeleteRows = false;
            this.dgvTarget.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvTarget.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvTarget.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTarget.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvTarget.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTarget.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStageID,
            this.colStageName,
            this.colTrackingData,
            this.colOffset,
            this.colLen});
            this.dgvTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTarget.Location = new System.Drawing.Point(0, 24);
            this.dgvTarget.MultiSelect = false;
            this.dgvTarget.Name = "dgvTarget";
            this.dgvTarget.ReadOnly = true;
            this.dgvTarget.RowHeadersVisible = false;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvTarget.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvTarget.RowTemplate.Height = 24;
            this.dgvTarget.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTarget.Size = new System.Drawing.Size(197, 379);
            this.dgvTarget.TabIndex = 12;
            // 
            // colStageID
            // 
            this.colStageID.HeaderText = "StageID";
            this.colStageID.Name = "colStageID";
            this.colStageID.ReadOnly = true;
            this.colStageID.Visible = false;
            this.colStageID.Width = 70;
            // 
            // colStageName
            // 
            this.colStageName.HeaderText = "Stage";
            this.colStageName.Name = "colStageName";
            this.colStageName.ReadOnly = true;
            this.colStageName.Width = 60;
            // 
            // colTrackingData
            // 
            this.colTrackingData.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colTrackingData.HeaderText = "Tracking Data";
            this.colTrackingData.Name = "colTrackingData";
            this.colTrackingData.ReadOnly = true;
            this.colTrackingData.Width = 150;
            // 
            // colOffset
            // 
            this.colOffset.HeaderText = "Offset";
            this.colOffset.Name = "colOffset";
            this.colOffset.ReadOnly = true;
            this.colOffset.Visible = false;
            // 
            // colLen
            // 
            this.colLen.HeaderText = "Len";
            this.colLen.Name = "colLen";
            this.colLen.ReadOnly = true;
            this.colLen.Visible = false;
            // 
            // lblStageTrack
            // 
            this.lblStageTrack.BackColor = System.Drawing.Color.DimGray;
            this.lblStageTrack.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStageTrack.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStageTrack.ForeColor = System.Drawing.Color.White;
            this.lblStageTrack.Location = new System.Drawing.Point(0, 0);
            this.lblStageTrack.Name = "lblStageTrack";
            this.lblStageTrack.Size = new System.Drawing.Size(197, 24);
            this.lblStageTrack.TabIndex = 8;
            this.lblStageTrack.Text = "Target";
            this.lblStageTrack.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlRouteStep
            // 
            this.pnlRouteStep.Controls.Add(this.dgvRouteStep);
            this.pnlRouteStep.Controls.Add(this.btnCurrentStep);
            this.pnlRouteStep.Controls.Add(this.btnNextStep);
            this.pnlRouteStep.Controls.Add(this.lblRouteStep);
            this.pnlRouteStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRouteStep.Location = new System.Drawing.Point(313, 31);
            this.pnlRouteStep.Name = "pnlRouteStep";
            this.pnlRouteStep.Size = new System.Drawing.Size(106, 403);
            this.pnlRouteStep.TabIndex = 2;
            // 
            // dgvRouteStep
            // 
            this.dgvRouteStep.AllowUserToAddRows = false;
            this.dgvRouteStep.AllowUserToDeleteRows = false;
            this.dgvRouteStep.AllowUserToResizeRows = false;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvRouteStep.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvRouteStep.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRouteStep.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvRouteStep.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRouteStep.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStepID,
            this.colNextStep,
            this.colObjectKey,
            this.colServerName,
            this.colRobotName,
            this.colStepRouteID,
            this.colLineType,
            this.colStepDescription,
            this.colRobotAction,
            this.colRobotArm,
            this.colRobotRule,
            this.colStageList,
            this.colInputTracking,
            this.colOutputTracking,
            this.colRemark,
            this.colLastUpdateTime});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvRouteStep.DefaultCellStyle = dataGridViewCellStyle6;
            this.dgvRouteStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRouteStep.Location = new System.Drawing.Point(0, 24);
            this.dgvRouteStep.MultiSelect = false;
            this.dgvRouteStep.Name = "dgvRouteStep";
            this.dgvRouteStep.ReadOnly = true;
            this.dgvRouteStep.RowHeadersVisible = false;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvRouteStep.RowsDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvRouteStep.RowTemplate.Height = 24;
            this.dgvRouteStep.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRouteStep.Size = new System.Drawing.Size(106, 319);
            this.dgvRouteStep.TabIndex = 24;
            this.dgvRouteStep.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRouteStep_CellClick);
            // 
            // colStepID
            // 
            this.colStepID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colStepID.DataPropertyName = "STEPID";
            this.colStepID.HeaderText = "Step";
            this.colStepID.Name = "colStepID";
            this.colStepID.ReadOnly = true;
            this.colStepID.Width = 40;
            // 
            // colNextStep
            // 
            this.colNextStep.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colNextStep.DataPropertyName = "NEXTSTEPID";
            this.colNextStep.HeaderText = "Next Step";
            this.colNextStep.Name = "colNextStep";
            this.colNextStep.ReadOnly = true;
            // 
            // colObjectKey
            // 
            this.colObjectKey.DataPropertyName = "OBJECTKEY";
            this.colObjectKey.HeaderText = "Object Key";
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
            // colRobotName
            // 
            this.colRobotName.DataPropertyName = "ROBOTNAME";
            this.colRobotName.HeaderText = "RobotName";
            this.colRobotName.Name = "colRobotName";
            this.colRobotName.ReadOnly = true;
            this.colRobotName.Visible = false;
            // 
            // colStepRouteID
            // 
            this.colStepRouteID.DataPropertyName = "ROUTEID";
            this.colStepRouteID.HeaderText = "RouteID";
            this.colStepRouteID.Name = "colStepRouteID";
            this.colStepRouteID.ReadOnly = true;
            this.colStepRouteID.Visible = false;
            // 
            // colLineType
            // 
            this.colLineType.DataPropertyName = "LINETYPE";
            this.colLineType.HeaderText = "Line Type";
            this.colLineType.Name = "colLineType";
            this.colLineType.ReadOnly = true;
            this.colLineType.Visible = false;
            // 
            // colStepDescription
            // 
            this.colStepDescription.DataPropertyName = "DESCRIPTION";
            this.colStepDescription.HeaderText = "Description";
            this.colStepDescription.Name = "colStepDescription";
            this.colStepDescription.ReadOnly = true;
            this.colStepDescription.Visible = false;
            // 
            // colRobotAction
            // 
            this.colRobotAction.DataPropertyName = "ROBOTACTION";
            this.colRobotAction.HeaderText = "Robot Action";
            this.colRobotAction.Name = "colRobotAction";
            this.colRobotAction.ReadOnly = true;
            this.colRobotAction.Visible = false;
            // 
            // colRobotArm
            // 
            this.colRobotArm.DataPropertyName = "ROBOTUSEARM";
            this.colRobotArm.HeaderText = "Robot Arm";
            this.colRobotArm.Name = "colRobotArm";
            this.colRobotArm.ReadOnly = true;
            this.colRobotArm.Visible = false;
            // 
            // colRobotRule
            // 
            this.colRobotRule.DataPropertyName = "ROBOTRULE";
            this.colRobotRule.HeaderText = "Robot Rule";
            this.colRobotRule.Name = "colRobotRule";
            this.colRobotRule.ReadOnly = true;
            this.colRobotRule.Visible = false;
            // 
            // colStageList
            // 
            this.colStageList.DataPropertyName = "STAGEIDLIST";
            this.colStageList.HeaderText = "Stage List";
            this.colStageList.Name = "colStageList";
            this.colStageList.ReadOnly = true;
            this.colStageList.Visible = false;
            // 
            // colInputTracking
            // 
            this.colInputTracking.DataPropertyName = "INPUTTRACKDATA";
            this.colInputTracking.HeaderText = "Input Tracking";
            this.colInputTracking.Name = "colInputTracking";
            this.colInputTracking.ReadOnly = true;
            this.colInputTracking.Visible = false;
            // 
            // colOutputTracking
            // 
            this.colOutputTracking.DataPropertyName = "OUTPUTTRACKDATA";
            this.colOutputTracking.HeaderText = "Output Tracking";
            this.colOutputTracking.Name = "colOutputTracking";
            this.colOutputTracking.ReadOnly = true;
            this.colOutputTracking.Visible = false;
            // 
            // colRemark
            // 
            this.colRemark.DataPropertyName = "REMARKS";
            this.colRemark.HeaderText = "Remark";
            this.colRemark.Name = "colRemark";
            this.colRemark.ReadOnly = true;
            this.colRemark.Visible = false;
            // 
            // colLastUpdateTime
            // 
            this.colLastUpdateTime.DataPropertyName = "LASTUPDATETIME";
            this.colLastUpdateTime.HeaderText = "Last Update Time";
            this.colLastUpdateTime.Name = "colLastUpdateTime";
            this.colLastUpdateTime.ReadOnly = true;
            this.colLastUpdateTime.Visible = false;
            // 
            // btnCurrentStep
            // 
            this.btnCurrentStep.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCurrentStep.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCurrentStep.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCurrentStep.Location = new System.Drawing.Point(0, 343);
            this.btnCurrentStep.Name = "btnCurrentStep";
            this.btnCurrentStep.Size = new System.Drawing.Size(106, 30);
            this.btnCurrentStep.TabIndex = 23;
            this.btnCurrentStep.Text = "Current Step";
            this.btnCurrentStep.UseVisualStyleBackColor = true;
            this.btnCurrentStep.Click += new System.EventHandler(this.btnCurrentStep_Click);
            // 
            // btnNextStep
            // 
            this.btnNextStep.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnNextStep.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnNextStep.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNextStep.Location = new System.Drawing.Point(0, 373);
            this.btnNextStep.Name = "btnNextStep";
            this.btnNextStep.Size = new System.Drawing.Size(106, 30);
            this.btnNextStep.TabIndex = 21;
            this.btnNextStep.Text = "Next Step";
            this.btnNextStep.UseVisualStyleBackColor = true;
            this.btnNextStep.Click += new System.EventHandler(this.btnNextStep_Click);
            // 
            // lblRouteStep
            // 
            this.lblRouteStep.BackColor = System.Drawing.Color.DimGray;
            this.lblRouteStep.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRouteStep.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRouteStep.ForeColor = System.Drawing.Color.White;
            this.lblRouteStep.Location = new System.Drawing.Point(0, 0);
            this.lblRouteStep.Name = "lblRouteStep";
            this.lblRouteStep.Size = new System.Drawing.Size(106, 24);
            this.lblRouteStep.TabIndex = 8;
            this.lblRouteStep.Text = "Route Step";
            this.lblRouteStep.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flpRadioButton
            // 
            this.tlpBase.SetColumnSpan(this.flpRadioButton, 4);
            this.flpRadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpRadioButton.Location = new System.Drawing.Point(3, 3);
            this.flpRadioButton.Name = "flpRadioButton";
            this.flpRadioButton.Size = new System.Drawing.Size(1033, 22);
            this.flpRadioButton.TabIndex = 0;
            // 
            // pnlNormalBtn
            // 
            this.pnlNormalBtn.Controls.Add(this.btnDeleteStep);
            this.pnlNormalBtn.Controls.Add(this.btnAddRouteStep);
            this.pnlNormalBtn.Controls.Add(this.btnRefresh);
            this.pnlNormalBtn.Controls.Add(this.btnDeleteRoute);
            this.pnlNormalBtn.Controls.Add(this.btnSaveRoute);
            this.pnlNormalBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNormalBtn.Location = new System.Drawing.Point(1042, 31);
            this.pnlNormalBtn.Name = "pnlNormalBtn";
            this.tlpBase.SetRowSpan(this.pnlNormalBtn, 3);
            this.pnlNormalBtn.Size = new System.Drawing.Size(94, 583);
            this.pnlNormalBtn.TabIndex = 8;
            // 
            // btnDeleteStep
            // 
            this.btnDeleteStep.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDeleteStep.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeleteStep.Location = new System.Drawing.Point(2, 55);
            this.btnDeleteStep.Name = "btnDeleteStep";
            this.btnDeleteStep.Size = new System.Drawing.Size(90, 50);
            this.btnDeleteStep.TabIndex = 14;
            this.btnDeleteStep.Text = "Delete Step";
            this.btnDeleteStep.UseVisualStyleBackColor = true;
            this.btnDeleteStep.Click += new System.EventHandler(this.btnDeleteStep_Click);
            // 
            // btnAddRouteStep
            // 
            this.btnAddRouteStep.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddRouteStep.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddRouteStep.Location = new System.Drawing.Point(2, 4);
            this.btnAddRouteStep.Name = "btnAddRouteStep";
            this.btnAddRouteStep.Size = new System.Drawing.Size(90, 50);
            this.btnAddRouteStep.TabIndex = 13;
            this.btnAddRouteStep.Text = "Add      Route Step";
            this.btnAddRouteStep.UseVisualStyleBackColor = true;
            this.btnAddRouteStep.Click += new System.EventHandler(this.btnAddRouteStep_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(2, 208);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(90, 50);
            this.btnRefresh.TabIndex = 11;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnDeleteRoute
            // 
            this.btnDeleteRoute.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDeleteRoute.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeleteRoute.Location = new System.Drawing.Point(2, 157);
            this.btnDeleteRoute.Name = "btnDeleteRoute";
            this.btnDeleteRoute.Size = new System.Drawing.Size(90, 50);
            this.btnDeleteRoute.TabIndex = 9;
            this.btnDeleteRoute.Text = "Delete Route";
            this.btnDeleteRoute.UseVisualStyleBackColor = true;
            this.btnDeleteRoute.Click += new System.EventHandler(this.btnDeleteRoute_Click);
            // 
            // btnSaveRoute
            // 
            this.btnSaveRoute.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSaveRoute.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveRoute.Location = new System.Drawing.Point(2, 106);
            this.btnSaveRoute.Name = "btnSaveRoute";
            this.btnSaveRoute.Size = new System.Drawing.Size(90, 50);
            this.btnSaveRoute.TabIndex = 10;
            this.btnSaveRoute.Text = "Save Route";
            this.btnSaveRoute.UseVisualStyleBackColor = true;
            this.btnSaveRoute.Click += new System.EventHandler(this.btnSaveRoute_Click);
            // 
            // flpSetting
            // 
            this.flpSetting.Controls.Add(this.pnlRobotArm);
            this.flpSetting.Controls.Add(this.pnlAction);
            this.flpSetting.Controls.Add(this.pnlRobotRule);
            this.flpSetting.Controls.Add(this.pnlTarget);
            this.flpSetting.Controls.Add(this.tlpTargetStage);
            this.flpSetting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpSetting.Location = new System.Drawing.Point(625, 28);
            this.flpSetting.Margin = new System.Windows.Forms.Padding(0);
            this.flpSetting.Name = "flpSetting";
            this.tlpBase.SetRowSpan(this.flpSetting, 2);
            this.flpSetting.Size = new System.Drawing.Size(414, 532);
            this.flpSetting.TabIndex = 10;
            // 
            // pnlRobotArm
            // 
            this.pnlRobotArm.Controls.Add(this.btnArmAll);
            this.pnlRobotArm.Controls.Add(this.btnArmBoth);
            this.pnlRobotArm.Controls.Add(this.btnArmUpper);
            this.pnlRobotArm.Controls.Add(this.btnArmLow);
            this.pnlRobotArm.Controls.Add(this.lblRobotArm);
            this.pnlRobotArm.Location = new System.Drawing.Point(3, 1);
            this.pnlRobotArm.Margin = new System.Windows.Forms.Padding(3, 1, 0, 1);
            this.pnlRobotArm.Name = "pnlRobotArm";
            this.pnlRobotArm.Size = new System.Drawing.Size(414, 77);
            this.pnlRobotArm.TabIndex = 7;
            // 
            // btnArmAll
            // 
            this.btnArmAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnArmAll.Font = new System.Drawing.Font("Cambria", 14.25F);
            this.btnArmAll.Location = new System.Drawing.Point(310, 25);
            this.btnArmAll.Name = "btnArmAll";
            this.btnArmAll.Size = new System.Drawing.Size(99, 50);
            this.btnArmAll.TabIndex = 19;
            this.btnArmAll.Tag = "ALL";
            this.btnArmAll.Text = "All";
            this.btnArmAll.UseVisualStyleBackColor = true;
            this.btnArmAll.Click += new System.EventHandler(this.Setting_Click);
            // 
            // btnArmBoth
            // 
            this.btnArmBoth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnArmBoth.Font = new System.Drawing.Font("Cambria", 14.25F);
            this.btnArmBoth.Location = new System.Drawing.Point(208, 25);
            this.btnArmBoth.Name = "btnArmBoth";
            this.btnArmBoth.Size = new System.Drawing.Size(99, 50);
            this.btnArmBoth.TabIndex = 18;
            this.btnArmBoth.Tag = "ANY";
            this.btnArmBoth.Text = "Any";
            this.btnArmBoth.UseVisualStyleBackColor = true;
            this.btnArmBoth.Click += new System.EventHandler(this.Setting_Click);
            // 
            // btnArmUpper
            // 
            this.btnArmUpper.BackColor = System.Drawing.Color.Transparent;
            this.btnArmUpper.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnArmUpper.Font = new System.Drawing.Font("Cambria", 14.25F);
            this.btnArmUpper.Location = new System.Drawing.Point(106, 25);
            this.btnArmUpper.Name = "btnArmUpper";
            this.btnArmUpper.Size = new System.Drawing.Size(99, 50);
            this.btnArmUpper.TabIndex = 17;
            this.btnArmUpper.Tag = "UP";
            this.btnArmUpper.Text = "Upper";
            this.btnArmUpper.UseVisualStyleBackColor = false;
            this.btnArmUpper.Click += new System.EventHandler(this.Setting_Click);
            // 
            // btnArmLow
            // 
            this.btnArmLow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnArmLow.Font = new System.Drawing.Font("Cambria", 14.25F);
            this.btnArmLow.Location = new System.Drawing.Point(4, 25);
            this.btnArmLow.Name = "btnArmLow";
            this.btnArmLow.Size = new System.Drawing.Size(99, 50);
            this.btnArmLow.TabIndex = 16;
            this.btnArmLow.Tag = "LOW";
            this.btnArmLow.Text = "Lower";
            this.btnArmLow.UseVisualStyleBackColor = true;
            this.btnArmLow.Click += new System.EventHandler(this.Setting_Click);
            // 
            // lblRobotArm
            // 
            this.lblRobotArm.BackColor = System.Drawing.Color.DimGray;
            this.lblRobotArm.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblRobotArm.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRobotArm.ForeColor = System.Drawing.Color.White;
            this.lblRobotArm.Location = new System.Drawing.Point(3, 0);
            this.lblRobotArm.Name = "lblRobotArm";
            this.lblRobotArm.Size = new System.Drawing.Size(409, 24);
            this.lblRobotArm.TabIndex = 9;
            this.lblRobotArm.Text = "Robot Arm";
            this.lblRobotArm.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlAction
            // 
            this.pnlAction.Controls.Add(this.btnActionPut);
            this.pnlAction.Controls.Add(this.btnActionGet);
            this.pnlAction.Controls.Add(this.lblAction);
            this.pnlAction.Location = new System.Drawing.Point(3, 80);
            this.pnlAction.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlAction.Name = "pnlAction";
            this.pnlAction.Size = new System.Drawing.Size(414, 77);
            this.pnlAction.TabIndex = 6;
            // 
            // btnActionPut
            // 
            this.btnActionPut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActionPut.Font = new System.Drawing.Font("Cambria", 14.25F);
            this.btnActionPut.Location = new System.Drawing.Point(208, 25);
            this.btnActionPut.Name = "btnActionPut";
            this.btnActionPut.Size = new System.Drawing.Size(200, 50);
            this.btnActionPut.TabIndex = 18;
            this.btnActionPut.Tag = "PUT";
            this.btnActionPut.Text = "Put";
            this.btnActionPut.UseVisualStyleBackColor = true;
            this.btnActionPut.Click += new System.EventHandler(this.Setting_Click);
            // 
            // btnActionGet
            // 
            this.btnActionGet.BackColor = System.Drawing.Color.Transparent;
            this.btnActionGet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnActionGet.Font = new System.Drawing.Font("Cambria", 14.25F);
            this.btnActionGet.Location = new System.Drawing.Point(4, 25);
            this.btnActionGet.Name = "btnActionGet";
            this.btnActionGet.Size = new System.Drawing.Size(200, 50);
            this.btnActionGet.TabIndex = 17;
            this.btnActionGet.Tag = "GET";
            this.btnActionGet.Text = "Get";
            this.btnActionGet.UseVisualStyleBackColor = false;
            this.btnActionGet.Click += new System.EventHandler(this.Setting_Click);
            // 
            // lblAction
            // 
            this.lblAction.BackColor = System.Drawing.Color.DimGray;
            this.lblAction.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblAction.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAction.ForeColor = System.Drawing.Color.White;
            this.lblAction.Location = new System.Drawing.Point(3, 0);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(409, 24);
            this.lblAction.TabIndex = 9;
            this.lblAction.Text = "Action";
            this.lblAction.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlRobotRule
            // 
            this.pnlRobotRule.Controls.Add(this.btnRuleOnly);
            this.pnlRobotRule.Controls.Add(this.btnRuleDispatch);
            this.pnlRobotRule.Controls.Add(this.btnRuleSelect);
            this.pnlRobotRule.Controls.Add(this.label1);
            this.pnlRobotRule.Location = new System.Drawing.Point(3, 159);
            this.pnlRobotRule.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlRobotRule.Name = "pnlRobotRule";
            this.pnlRobotRule.Size = new System.Drawing.Size(414, 77);
            this.pnlRobotRule.TabIndex = 8;
            // 
            // btnRuleDispatch
            // 
            this.btnRuleDispatch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRuleDispatch.Font = new System.Drawing.Font("Cambria", 14.25F);
            this.btnRuleDispatch.Location = new System.Drawing.Point(276, 25);
            this.btnRuleDispatch.Name = "btnRuleDispatch";
            this.btnRuleDispatch.Size = new System.Drawing.Size(133, 50);
            this.btnRuleDispatch.TabIndex = 21;
            this.btnRuleDispatch.Tag = "ULDDISPATCH";
            this.btnRuleDispatch.Text = "UD Dispatch";
            this.btnRuleDispatch.UseVisualStyleBackColor = true;
            this.btnRuleDispatch.Visible = false;
            this.btnRuleDispatch.Click += new System.EventHandler(this.Setting_Click);
            // 
            // btnRuleOnly
            // 
            this.btnRuleOnly.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRuleOnly.Font = new System.Drawing.Font("Cambria", 14.25F);
            this.btnRuleOnly.Location = new System.Drawing.Point(206, 25);
            this.btnRuleOnly.Name = "btnRuleOnly";
            this.btnRuleOnly.Size = new System.Drawing.Size(204, 50);
            this.btnRuleOnly.TabIndex = 18;
            this.btnRuleOnly.Tag = "ONLY";
            this.btnRuleOnly.Text = "Only";
            this.btnRuleOnly.UseVisualStyleBackColor = true;
            this.btnRuleOnly.Click += new System.EventHandler(this.Setting_Click);
            // 
            // btnRuleSelect
            // 
            this.btnRuleSelect.BackColor = System.Drawing.Color.Transparent;
            this.btnRuleSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRuleSelect.Font = new System.Drawing.Font("Cambria", 14.25F);
            this.btnRuleSelect.Location = new System.Drawing.Point(3, 25);
            this.btnRuleSelect.Name = "btnRuleSelect";
            this.btnRuleSelect.Size = new System.Drawing.Size(204, 50);
            this.btnRuleSelect.TabIndex = 17;
            this.btnRuleSelect.Tag = "SELECT";
            this.btnRuleSelect.Text = "Select";
            this.btnRuleSelect.UseVisualStyleBackColor = false;
            this.btnRuleSelect.Click += new System.EventHandler(this.Setting_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.DimGray;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(409, 24);
            this.label1.TabIndex = 9;
            this.label1.Text = "Robot Rule";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlTarget
            // 
            this.pnlTarget.Controls.Add(this.chkCrossStageFlag);
            this.pnlTarget.Controls.Add(this.btnTargetStage);
            this.pnlTarget.Controls.Add(this.btnTargetCassette);
            this.pnlTarget.Controls.Add(this.lblTarget);
            this.pnlTarget.Location = new System.Drawing.Point(3, 238);
            this.pnlTarget.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlTarget.Name = "pnlTarget";
            this.pnlTarget.Size = new System.Drawing.Size(414, 77);
            this.pnlTarget.TabIndex = 5;
            // 
            // chkCrossStageFlag
            // 
            this.chkCrossStageFlag.BackColor = System.Drawing.Color.DimGray;
            this.chkCrossStageFlag.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkCrossStageFlag.ForeColor = System.Drawing.Color.Lime;
            this.chkCrossStageFlag.Location = new System.Drawing.Point(310, 1);
            this.chkCrossStageFlag.Name = "chkCrossStageFlag";
            this.chkCrossStageFlag.Size = new System.Drawing.Size(105, 22);
            this.chkCrossStageFlag.TabIndex = 19;
            this.chkCrossStageFlag.Text = "Cross Stage";
            this.chkCrossStageFlag.UseVisualStyleBackColor = false;
            this.chkCrossStageFlag.CheckedChanged += new System.EventHandler(this.chkCrossStageFlag_CheckedChanged);
            // 
            // btnTargetStage
            // 
            this.btnTargetStage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTargetStage.Font = new System.Drawing.Font("Cambria", 14.25F);
            this.btnTargetStage.Location = new System.Drawing.Point(208, 25);
            this.btnTargetStage.Name = "btnTargetStage";
            this.btnTargetStage.Size = new System.Drawing.Size(200, 50);
            this.btnTargetStage.TabIndex = 18;
            this.btnTargetStage.Tag = "Stage";
            this.btnTargetStage.Text = "Stage";
            this.btnTargetStage.UseVisualStyleBackColor = true;
            this.btnTargetStage.Click += new System.EventHandler(this.Setting_Click);
            // 
            // btnTargetCassette
            // 
            this.btnTargetCassette.BackColor = System.Drawing.Color.Transparent;
            this.btnTargetCassette.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTargetCassette.Font = new System.Drawing.Font("Cambria", 14.25F);
            this.btnTargetCassette.Location = new System.Drawing.Point(4, 25);
            this.btnTargetCassette.Name = "btnTargetCassette";
            this.btnTargetCassette.Size = new System.Drawing.Size(200, 50);
            this.btnTargetCassette.TabIndex = 17;
            this.btnTargetCassette.Tag = "Cassette";
            this.btnTargetCassette.Text = "Cassette";
            this.btnTargetCassette.UseVisualStyleBackColor = false;
            this.btnTargetCassette.Click += new System.EventHandler(this.Setting_Click);
            // 
            // lblTarget
            // 
            this.lblTarget.BackColor = System.Drawing.Color.DimGray;
            this.lblTarget.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTarget.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTarget.ForeColor = System.Drawing.Color.White;
            this.lblTarget.Location = new System.Drawing.Point(3, 0);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(409, 24);
            this.lblTarget.TabIndex = 9;
            this.lblTarget.Text = "Target";
            this.lblTarget.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tlpTargetStage
            // 
            this.tlpTargetStage.ColumnCount = 2;
            this.tlpTargetStage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 89F));
            this.tlpTargetStage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTargetStage.Controls.Add(this.pnlStage, 1, 0);
            this.tlpTargetStage.Controls.Add(this.pnlPort, 0, 0);
            this.tlpTargetStage.Location = new System.Drawing.Point(3, 319);
            this.tlpTargetStage.Name = "tlpTargetStage";
            this.tlpTargetStage.RowCount = 1;
            this.tlpTargetStage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTargetStage.Size = new System.Drawing.Size(414, 210);
            this.tlpTargetStage.TabIndex = 3;
            // 
            // pnlStage
            // 
            this.pnlStage.Controls.Add(this.lblTrack);
            this.pnlStage.Controls.Add(this.flpTracking);
            this.pnlStage.Controls.Add(this.flpStage);
            this.pnlStage.Controls.Add(this.lblStage);
            this.pnlStage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStage.Location = new System.Drawing.Point(92, 1);
            this.pnlStage.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlStage.Name = "pnlStage";
            this.pnlStage.Size = new System.Drawing.Size(319, 208);
            this.pnlStage.TabIndex = 2;
            // 
            // lblTrack
            // 
            this.lblTrack.BackColor = System.Drawing.Color.DimGray;
            this.lblTrack.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTrack.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrack.ForeColor = System.Drawing.Color.White;
            this.lblTrack.Location = new System.Drawing.Point(116, 0);
            this.lblTrack.Name = "lblTrack";
            this.lblTrack.Size = new System.Drawing.Size(203, 24);
            this.lblTrack.TabIndex = 12;
            this.lblTrack.Text = "Tracking Data";
            this.lblTrack.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flpTracking
            // 
            this.flpTracking.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpTracking.Location = new System.Drawing.Point(116, 24);
            this.flpTracking.Name = "flpTracking";
            this.flpTracking.Size = new System.Drawing.Size(202, 180);
            this.flpTracking.TabIndex = 11;
            // 
            // flpStage
            // 
            this.flpStage.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpStage.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpStage.Location = new System.Drawing.Point(0, 24);
            this.flpStage.Name = "flpStage";
            this.flpStage.Size = new System.Drawing.Size(113, 180);
            this.flpStage.TabIndex = 10;
            // 
            // lblStage
            // 
            this.lblStage.BackColor = System.Drawing.Color.DimGray;
            this.lblStage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblStage.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStage.ForeColor = System.Drawing.Color.White;
            this.lblStage.Location = new System.Drawing.Point(0, 0);
            this.lblStage.Name = "lblStage";
            this.lblStage.Size = new System.Drawing.Size(113, 24);
            this.lblStage.TabIndex = 8;
            this.lblStage.Text = "Stage";
            this.lblStage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlPort
            // 
            this.pnlPort.Controls.Add(this.flpPort);
            this.pnlPort.Controls.Add(this.lblPort);
            this.pnlPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPort.Location = new System.Drawing.Point(1, 1);
            this.pnlPort.Margin = new System.Windows.Forms.Padding(1);
            this.pnlPort.Name = "pnlPort";
            this.pnlPort.Size = new System.Drawing.Size(87, 208);
            this.pnlPort.TabIndex = 1;
            // 
            // flpPort
            // 
            this.flpPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpPort.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpPort.Location = new System.Drawing.Point(0, 24);
            this.flpPort.Name = "flpPort";
            this.flpPort.Size = new System.Drawing.Size(87, 184);
            this.flpPort.TabIndex = 9;
            // 
            // lblPort
            // 
            this.lblPort.BackColor = System.Drawing.Color.DimGray;
            this.lblPort.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPort.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPort.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPort.ForeColor = System.Drawing.Color.White;
            this.lblPort.Location = new System.Drawing.Point(0, 0);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(87, 24);
            this.lblPort.TabIndex = 8;
            this.lblPort.Text = "Port";
            this.lblPort.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flpButtons
            // 
            this.flpButtons.AutoScroll = true;
            this.flpButtons.Controls.Add(this.btnResult_Handle);
            this.flpButtons.Controls.Add(this.btnRouteStepByPass);
            this.flpButtons.Controls.Add(this.btnRouteStepJump);
            this.flpButtons.Controls.Add(this.btnRULE_FILTER);
            this.flpButtons.Controls.Add(this.btnRULE_ORDERBY);
            this.flpButtons.Controls.Add(this.btnStageSelect);
            this.flpButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.flpButtons.Location = new System.Drawing.Point(627, 563);
            this.flpButtons.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Size = new System.Drawing.Size(410, 51);
            this.flpButtons.TabIndex = 11;
            // 
            // btnResult_Handle
            // 
            this.btnResult_Handle.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnResult_Handle.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnResult_Handle.Location = new System.Drawing.Point(0, 1);
            this.btnResult_Handle.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.btnResult_Handle.Name = "btnResult_Handle";
            this.btnResult_Handle.Size = new System.Drawing.Size(78, 48);
            this.btnResult_Handle.TabIndex = 17;
            this.btnResult_Handle.Tag = "Result Handle";
            this.btnResult_Handle.Text = "Result Handle";
            this.btnResult_Handle.UseVisualStyleBackColor = true;
            this.btnResult_Handle.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnRouteStepByPass
            // 
            this.btnRouteStepByPass.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRouteStepByPass.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRouteStepByPass.Location = new System.Drawing.Point(78, 1);
            this.btnRouteStepByPass.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.btnRouteStepByPass.Name = "btnRouteStepByPass";
            this.btnRouteStepByPass.Size = new System.Drawing.Size(81, 48);
            this.btnRouteStepByPass.TabIndex = 20;
            this.btnRouteStepByPass.Tag = "RouteStep ByPass";
            this.btnRouteStepByPass.Text = "RouteStep ByPass";
            this.btnRouteStepByPass.UseVisualStyleBackColor = true;
            this.btnRouteStepByPass.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnRouteStepJump
            // 
            this.btnRouteStepJump.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRouteStepJump.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRouteStepJump.Location = new System.Drawing.Point(159, 1);
            this.btnRouteStepJump.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.btnRouteStepJump.Name = "btnRouteStepJump";
            this.btnRouteStepJump.Size = new System.Drawing.Size(81, 48);
            this.btnRouteStepJump.TabIndex = 19;
            this.btnRouteStepJump.Tag = "RouteStep Jump";
            this.btnRouteStepJump.Text = "RouteStep Jump";
            this.btnRouteStepJump.UseVisualStyleBackColor = true;
            this.btnRouteStepJump.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnRULE_FILTER
            // 
            this.btnRULE_FILTER.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRULE_FILTER.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRULE_FILTER.Location = new System.Drawing.Point(240, 1);
            this.btnRULE_FILTER.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.btnRULE_FILTER.Name = "btnRULE_FILTER";
            this.btnRULE_FILTER.Size = new System.Drawing.Size(78, 48);
            this.btnRULE_FILTER.TabIndex = 15;
            this.btnRULE_FILTER.Tag = "Rule Filter";
            this.btnRULE_FILTER.Text = "Rule Filter";
            this.btnRULE_FILTER.UseVisualStyleBackColor = true;
            this.btnRULE_FILTER.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnRULE_ORDERBY
            // 
            this.btnRULE_ORDERBY.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRULE_ORDERBY.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRULE_ORDERBY.Location = new System.Drawing.Point(318, 1);
            this.btnRULE_ORDERBY.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.btnRULE_ORDERBY.Name = "btnRULE_ORDERBY";
            this.btnRULE_ORDERBY.Size = new System.Drawing.Size(78, 48);
            this.btnRULE_ORDERBY.TabIndex = 16;
            this.btnRULE_ORDERBY.Tag = "Rule OrderBy";
            this.btnRULE_ORDERBY.Text = "Rule OrderBy";
            this.btnRULE_ORDERBY.UseVisualStyleBackColor = true;
            this.btnRULE_ORDERBY.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnStageSelect
            // 
            this.btnStageSelect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnStageSelect.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStageSelect.Location = new System.Drawing.Point(0, 51);
            this.btnStageSelect.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.btnStageSelect.Name = "btnStageSelect";
            this.btnStageSelect.Size = new System.Drawing.Size(78, 48);
            this.btnStageSelect.TabIndex = 18;
            this.btnStageSelect.Tag = "Stage Select";
            this.btnStageSelect.Text = "Stage Select";
            this.btnStageSelect.UseVisualStyleBackColor = true;
            this.btnStageSelect.Visible = false;
            this.btnStageSelect.Click += new System.EventHandler(this.btn_Click);
            // 
            // pnlRobotRoute
            // 
            this.pnlRobotRoute.Controls.Add(this.dgvRobotRoute);
            this.pnlRobotRoute.Controls.Add(this.lblRobotRoute);
            this.pnlRobotRoute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRobotRoute.Location = new System.Drawing.Point(3, 31);
            this.pnlRobotRoute.Name = "pnlRobotRoute";
            this.pnlRobotRoute.Size = new System.Drawing.Size(304, 403);
            this.pnlRobotRoute.TabIndex = 1;
            // 
            // dgvRobotRoute
            // 
            this.dgvRobotRoute.AllowUserToAddRows = false;
            this.dgvRobotRoute.AllowUserToDeleteRows = false;
            this.dgvRobotRoute.AllowUserToResizeRows = false;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvRobotRoute.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dgvRobotRoute.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRobotRoute.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.dgvRobotRoute.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRobotRoute.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRoutePriority,
            this.colRouteID,
            this.colDescription});
            this.dgvRobotRoute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRobotRoute.Location = new System.Drawing.Point(0, 24);
            this.dgvRobotRoute.MultiSelect = false;
            this.dgvRobotRoute.Name = "dgvRobotRoute";
            this.dgvRobotRoute.ReadOnly = true;
            this.dgvRobotRoute.RowHeadersVisible = false;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvRobotRoute.RowsDefaultCellStyle = dataGridViewCellStyle10;
            this.dgvRobotRoute.RowTemplate.Height = 24;
            this.dgvRobotRoute.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRobotRoute.Size = new System.Drawing.Size(304, 379);
            this.dgvRobotRoute.TabIndex = 10;
            this.dgvRobotRoute.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRobotRoute_CellClick);
            // 
            // colRoutePriority
            // 
            this.colRoutePriority.DataPropertyName = "ROUTEPRIORITY";
            this.colRoutePriority.HeaderText = "Route Priority";
            this.colRoutePriority.Name = "colRoutePriority";
            this.colRoutePriority.ReadOnly = true;
            this.colRoutePriority.Width = 120;
            // 
            // colRouteID
            // 
            this.colRouteID.DataPropertyName = "ROUTEID";
            this.colRouteID.HeaderText = "Route ID";
            this.colRouteID.Name = "colRouteID";
            this.colRouteID.ReadOnly = true;
            // 
            // colDescription
            // 
            this.colDescription.DataPropertyName = "DESCRIPTION";
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            this.colDescription.Width = 300;
            // 
            // lblRobotRoute
            // 
            this.lblRobotRoute.BackColor = System.Drawing.Color.DimGray;
            this.lblRobotRoute.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRobotRoute.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRobotRoute.ForeColor = System.Drawing.Color.White;
            this.lblRobotRoute.Location = new System.Drawing.Point(0, 0);
            this.lblRobotRoute.Name = "lblRobotRoute";
            this.lblRobotRoute.Size = new System.Drawing.Size(304, 24);
            this.lblRobotRoute.TabIndex = 8;
            this.lblRobotRoute.Text = "Robot Route";
            this.lblRobotRoute.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlRobotPic
            // 
            this.pnlRobotPic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlRobotPic.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tlpBase.SetColumnSpan(this.pnlRobotPic, 3);
            this.pnlRobotPic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRobotPic.Location = new System.Drawing.Point(3, 440);
            this.pnlRobotPic.Name = "pnlRobotPic";
            this.tlpBase.SetRowSpan(this.pnlRobotPic, 2);
            this.pnlRobotPic.Size = new System.Drawing.Size(619, 174);
            this.pnlRobotPic.TabIndex = 9;
            // 
            // btnNextStepSet
            // 
            this.btnNextStepSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnNextStepSet.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnNextStepSet.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNextStepSet.Location = new System.Drawing.Point(0, 353);
            this.btnNextStepSet.Name = "btnNextStepSet";
            this.btnNextStepSet.Size = new System.Drawing.Size(106, 30);
            this.btnNextStepSet.TabIndex = 21;
            this.btnNextStepSet.Text = "Next Step";
            this.btnNextStepSet.UseVisualStyleBackColor = true;
            // 
            // FormRobotRoute
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1139, 648);
            this.Name = "FormRobotRoute";
            this.Text = "FormRobotRoute";
            this.Load += new System.EventHandler(this.FormRobotRoute_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.pnlStageTrack.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTarget)).EndInit();
            this.pnlRouteStep.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRouteStep)).EndInit();
            this.pnlNormalBtn.ResumeLayout(false);
            this.flpSetting.ResumeLayout(false);
            this.pnlRobotArm.ResumeLayout(false);
            this.pnlAction.ResumeLayout(false);
            this.pnlRobotRule.ResumeLayout(false);
            this.pnlTarget.ResumeLayout(false);
            this.tlpTargetStage.ResumeLayout(false);
            this.pnlStage.ResumeLayout(false);
            this.pnlPort.ResumeLayout(false);
            this.flpButtons.ResumeLayout(false);
            this.pnlRobotRoute.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRobotRoute)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.FlowLayoutPanel flpRadioButton;
        private System.Windows.Forms.Panel pnlRobotRoute;
        private System.Windows.Forms.Panel pnlRouteStep;
        private System.Windows.Forms.Label lblRouteStep;
        private System.Windows.Forms.Label lblRobotRoute;
        private System.Windows.Forms.DataGridView dgvRobotRoute;
        private System.Windows.Forms.TableLayoutPanel tlpTargetStage;
        private System.Windows.Forms.Panel pnlStage;
        private System.Windows.Forms.FlowLayoutPanel flpStage;
        private System.Windows.Forms.Label lblStage;
        private System.Windows.Forms.Panel pnlPort;
        private System.Windows.Forms.FlowLayoutPanel flpPort;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Panel pnlTarget;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.Panel pnlRobotArm;
        private System.Windows.Forms.Label lblRobotArm;
        private System.Windows.Forms.Panel pnlNormalBtn;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnDeleteRoute;
        private System.Windows.Forms.Button btnSaveRoute;
        private System.Windows.Forms.Button btnTargetStage;
        private System.Windows.Forms.Button btnTargetCassette;
        private System.Windows.Forms.Button btnArmBoth;
        private System.Windows.Forms.Button btnArmUpper;
        private System.Windows.Forms.Button btnArmLow;
        private System.Windows.Forms.Panel pnlRobotPic;
        private System.Windows.Forms.Button btnAddRouteStep;
        private System.Windows.Forms.Button btnArmAll;
        private System.Windows.Forms.FlowLayoutPanel flpSetting;
        private System.Windows.Forms.FlowLayoutPanel flpTracking;
        private System.Windows.Forms.Button btnDeleteStep;
        private System.Windows.Forms.Label lblTrack;
        private System.Windows.Forms.Panel pnlAction;
        private System.Windows.Forms.Button btnActionPut;
        private System.Windows.Forms.Button btnActionGet;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.Panel pnlRobotRule;
        private System.Windows.Forms.Button btnRuleDispatch;
        private System.Windows.Forms.Button btnRuleOnly;
        private System.Windows.Forms.Button btnRuleSelect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnResult_Handle;
        private System.Windows.Forms.Button btnRULE_ORDERBY;
        private System.Windows.Forms.Button btnRULE_FILTER;
        private System.Windows.Forms.Button btnStageSelect;
        private System.Windows.Forms.Button btnRouteStepByPass;
        private System.Windows.Forms.Button btnRouteStepJump;
        private System.Windows.Forms.Button btnNextStep;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        private System.Windows.Forms.DataGridView dgvTarget;
        private System.Windows.Forms.Panel pnlStageTrack;
        private System.Windows.Forms.Label lblStageTrack;
        private System.Windows.Forms.DataGridView dgvRouteStep;
        private System.Windows.Forms.Button btnCurrentStep;
        private System.Windows.Forms.Button btnNextStepSet;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStageID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStageName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrackingData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOffset;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLen;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStepID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNextStep;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStepRouteID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStepDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotAction;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotArm;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotRule;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStageList;
        private System.Windows.Forms.DataGridViewTextBoxColumn colInputTracking;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOutputTracking;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemark;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastUpdateTime;
        private System.Windows.Forms.CheckBox chkCrossStageFlag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRoutePriority;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRouteID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
    }
}