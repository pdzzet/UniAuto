using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobotStageManagementEdit : FormBase
    {
        private SBRM_ROBOT_STAGE ObjEditData;
        private string strRemark=string.Empty;

        public FormRobotStageManagementEdit()
        {

            InitializeComponent();
            lblCaption.Text = "Robot Stage Management Edit";
        }

        public FormRobotStageManagementEdit(SBRM_ROBOT_STAGE objData)
            : this()
        {
            if (objData == null)
            {
                FormMode = UniOPI.FormMode.AddNew;

                pnlAdd.Visible = true;
                dgvAddList.Visible = true;

                Size = new Size(851, 790);
            }
            else
            {
                FormMode = UniOPI.FormMode.Modify;

                ObjEditData = objData;

                pnlAdd.Visible = false;
                dgvAddList.Visible = false;

                Size = new Size(850, 640);
            }
        }

        private void FormRobotStageManagementEdit_Load(object sender, EventArgs e)
        {
            try
            {
                this.InitialCombox();

                 #region 1 Fork 2Job的Line Type需要提供取片优先级的设定（对应DB中SBRM_ROBOT_STAGE中Remak字段），其他Line隐藏 By box.Zhai

                if (FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("CHN") ||
                    FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("SOR") ||
                    FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("CRP") ||
                    FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("RWT"))
                {
                    pnlArmUsePriority.Visible = true;
                    pnlUseSpecificArm.Visible = true;
                    SetArmUsePriority();
                    SetUseSpecificArm();
                }
                else
                {
                    pnlArmUsePriority.Visible = false;
                    pnlUseSpecificArm.Visible = false;
                }

                 #endregion

                if (FormMode == UniOPI.FormMode.Modify)
                {
                    this.txtStageID.ReadOnly = true;
                    this.cboRobotName.Enabled = false;

                    if (ObjEditData != null)
                    {
                        cboRobotName.SelectedValue = ObjEditData.ROBOTNAME;
                        cboNode.SelectedValue = ObjEditData.NODENO;
                        cboStageType.SelectedValue = ObjEditData.STAGETYPE;
                        cboCassetteType.SelectedValue = ObjEditData.CASSETTETYPE;

                        txtStageID.Text = ObjEditData.STAGEID;
                        txtStageIDByNode.Text = ObjEditData.STAGEIDBYNODE;
                        txtStageName.Text = ObjEditData.STAGENAME;
                        txtPriority.Text = ObjEditData.PRIORITY.ToString();
                        txtStageReportTrxName.Text = ObjEditData.STAGEREPORTTRXNAME;
                        txtSlotMaxCount.Text = ObjEditData.SLOTMAXCOUNT.ToString();
                        txtStageJobDataTrxName.Text = ObjEditData.STAGEJOBDATATRXNAME;
                        txtUpstreamPathTrxName.Text = ObjEditData.UPSTREAMPATHTRXNAME != null ? ObjEditData.UPSTREAMPATHTRXNAME : string.Empty;
                        txtTrackDataSeqList.Text = ObjEditData.TRACKDATASEQLIST != null ? ObjEditData.TRACKDATASEQLIST : string.Empty;
                        txtUpstreamSendPathTrxName.Text = ObjEditData.UPSTREAMJOBDATAPATHTRXNAME != null ? ObjEditData.UPSTREAMJOBDATAPATHTRXNAME : string.Empty;
                        txtDownstreamPathTrxName.Text = ObjEditData.DOWNSTREAMPATHTRXNAME != null ? ObjEditData.DOWNSTREAMPATHTRXNAME : string.Empty;
                        txtDownstreamReceivePathTrxName.Text = ObjEditData.DOWNSTREAMJOBDATAPATHTRXNAME != null ? ObjEditData.DOWNSTREAMJOBDATAPATHTRXNAME : string.Empty;
                        cboExchangeType.SelectedValue = ObjEditData.EXCHANGETYPE;
                        cboEQRobotIfType.SelectedValue = ObjEditData.EQROBOTIFTYPE;
                        cboSlotFetchSeq.SelectedValue = ObjEditData.SLOTFETCHSEQ;
                        cboSlotStoreSeq.SelectedValue = ObjEditData.SLOTSTORESEQ;
                        if (FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("CHN") ||
                            FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("SOR") ||
                            FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("CRP") ||
                            FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("RWT"))
                        {
                            string[] remark = ObjEditData.REMARKS != null ? ObjEditData.REMARKS.Split(',') : new string[0];
                            if (remark.Length >= 2)
                            {
                                cboArmUsePriority.SelectedIndex = -1;
                                cboArmUsePriority.SelectedValue = remark[0];
                                cboUseSpecificArm.SelectedValue = remark[1];
                            }
                            else if (remark.Length == 1)
                            {
                                cboArmUsePriority.SelectedIndex = -1;
                                cboArmUsePriority.SelectedValue = remark[0];
                            }
                        }
                        else
                        {
                            strRemark = ObjEditData.REMARKS != null ? ObjEditData.REMARKS : string.Empty;
                        }
                        chkDummyCheckFlag.Checked = ObjEditData.DUMMYCHECKFLAG == "Y" ? true : false;
                        chkPrefetchFlag.Checked = ObjEditData.PREFETCHFLAG == "Y" ? true : false;
                        chkPutReadyFlag.Checked = ObjEditData.PUTREADYFLAG == "Y" ? true : false;
                        chkIsMultiSlot.Checked = ObjEditData.ISMULTISLOT == "Y" ? true : false;
                        chkGetReadyFlag.Checked = ObjEditData.GETREADYFLAG == "Y" ? true : false;
                        chkSupportWaitFront.Checked = ObjEditData.WAITFRONTFLAG == "Y" ? true : false;
                        chkRecipeCheckFlag.Checked = ObjEditData.RECIPECHENCKFLAG == "Y" ? true : false;
                        chkStageEnabled.Checked = ObjEditData.ISENABLED == "Y" ? true : false;
                        chkRTCReworkFlag.Checked = ObjEditData.RTCREWORKFLAG == "Y" ? true : false;
                    }

                    this.ActiveControl = txtStageName;
                }
                else
                    this.ActiveControl = txtStageID;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btn_Click(object sender, EventArgs e)
        {
            string _remark = string.Empty;
            try
            {
                Button _btn = (Button)sender;

                int iPriority = 0;
                int iSlotCount = 0;

                switch (_btn.Tag.ToString())
                {
                    case "ADD":

                        #region Add

                        #region Add to DataGridView

                        if (CheckData() == false) return;
                        string _serverName = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        string _lineID = FormMainMDI.G_OPIAp.CurLine.LineID;
                        string _stageID = txtStageID.Text.Trim().PadLeft(2, '0');
                        string _robotName = cboRobotName.SelectedValue.ToString();
                        string _localNo = ((dynamic)cboNode.SelectedItem).NodeNo;
                        string _stageType = this.cboStageType.SelectedValue.ToString();
                        string _cassetteType = cboCassetteType.SelectedValue.ToString();
                        string _slotFetchSeq = cboSlotFetchSeq.SelectedValue.ToString();
                        string _slotStoreSeq = cboSlotStoreSeq.SelectedValue.ToString();
                        string _exchangeType = cboExchangeType.SelectedValue.ToString();
                        string _eqRobotIfType = cboEQRobotIfType.SelectedValue.ToString();
                        string _stageNoByNode = txtStageIDByNode.Text.Trim();
                        string _stageName = txtStageName.Text.Trim();
                        string _stageReportTrxName = txtStageReportTrxName.Text.Trim();
                        string _stageJobDataTrxName = txtStageJobDataTrxName.Text.Trim();
                        if (cboArmUsePriority.SelectedValue != null&&cboArmUsePriority.SelectedValue.ToString()!="")
                            _remark= cboArmUsePriority.SelectedValue.ToString().Trim();
                        if (cboUseSpecificArm.SelectedValue != null && cboArmUsePriority.SelectedValue.ToString()!= "")
                            _remark += "," + cboUseSpecificArm.SelectedValue.ToString().Trim();
                        string _trackingDataList = string.Empty;
                        if( cboStageType.SelectedValue.ToString().Equals("PORT") == false)
                            _trackingDataList = txtTrackDataSeqList.Text.Trim();
                        string _upstreamPathTrxName = txtUpstreamPathTrxName.Text.Trim();
                        string _upstreamSendPathTrxName = txtUpstreamSendPathTrxName.Text.Trim();
                        string _dnstreamPathTrxName = txtDownstreamPathTrxName.Text.Trim();
                        string _dnstreamSendPathTrxName = txtDownstreamReceivePathTrxName.Text.Trim();

                        string _dummyCheck = chkDummyCheckFlag.Checked ? "Y" : "N";
                        string _prefetchFlag = chkPrefetchFlag.Checked ? "Y" : "N";
                        string _putReadyFlag = chkPutReadyFlag.Checked ? "Y" : "N";
                        string _getReadyFlag = chkGetReadyFlag.Checked ? "Y" : "N";
                        string _isMultiSlot = chkIsMultiSlot.Checked ? "Y" : "N";
                        string _supportWaitFront = chkSupportWaitFront.Checked ? "Y" : "N";
                        string _recipeCheck = chkRecipeCheckFlag.Checked ? "Y" : "N";
                        string _stageEnable = chkStageEnabled.Checked ? "Y" : "N";
                        string _rtcReworkFlag = chkRTCReworkFlag.Checked ? "Y" : "N";
                        int.TryParse(txtPriority.Text.Trim(), out iPriority);
                        int.TryParse(txtSlotMaxCount.Text.Trim(), out iSlotCount);

                        if (CheckUniqueKeyOK(_stageID, _robotName) == false) return;

                        #region 判斷是否存下gridview內
                        if (dgvAddList.Rows.Cast<DataGridViewRow>().Where(r =>
                            r.Cells[colRobotName.Name].Value.ToString().Equals(_robotName) &&
                            r.Cells[colStageID.Name].Value.ToString().Equals(_stageID)
                            ).Count() > 0)
                        {
                            DataGridViewRow _addRow = dgvAddList.Rows.Cast<DataGridViewRow>().Where(r =>
                            r.Cells[colRobotName.Name].Value.ToString().Equals(_robotName) &&
                            r.Cells[colStageID.Name].Value.ToString().Equals(_stageID)
                                ).First();

                            string _msg = string.Format("Robot Name [{0}],Stage ID[{1}] is already insert. Please confirm whether you will overwite data ?", _robotName, _stageID);

                            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;

                            _addRow.Cells[colLocalNo.Name].Value = _localNo;
                            _addRow.Cells[colStageType.Name].Value = _stageType;
                            _addRow.Cells[colCassetteType.Name].Value = _cassetteType;
                            _addRow.Cells[colSlotFetchSeq.Name].Value = _slotFetchSeq;
                            _addRow.Cells[colSlotStoreSeq.Name].Value = _slotStoreSeq;

                            _addRow.Cells[colStageIDByNode.Name].Value = _stageNoByNode;
                            _addRow.Cells[colStageName.Name].Value = _stageName;
                            _addRow.Cells[colPriority.Name].Value = iPriority;
                            _addRow.Cells[colStageReportTrxName.Name].Value = _stageReportTrxName;
                            _addRow.Cells[colSlotMaxCount.Name].Value = iSlotCount;
                            _addRow.Cells[colStageJobDataTrxName.Name].Value = _stageJobDataTrxName;
                            _addRow.Cells[colRemark.Name].Value = _remark;
                            _addRow.Cells[colUpstreamPathTrxName.Name].Value = _upstreamPathTrxName;
                            _addRow.Cells[colUpstreamSendPathTrxName.Name].Value = _upstreamSendPathTrxName;
                            _addRow.Cells[colDownstreamPathTrxName.Name].Value = _dnstreamPathTrxName;
                            _addRow.Cells[colDownstreamReceivePathTrxName.Name].Value = _dnstreamSendPathTrxName;
                            _addRow.Cells[colExchangeType.Name].Value = _exchangeType;
                            _addRow.Cells[colEQRobotIfType.Name].Value = _eqRobotIfType;
                            _addRow.Cells[colDummyCheckFlag.Name].Value = _dummyCheck;
                            _addRow.Cells[colPrefetchFlag.Name].Value = _prefetchFlag;
                            _addRow.Cells[colGetReadyFlag.Name].Value = _getReadyFlag;
                            _addRow.Cells[colPutReadyFlag.Name].Value = _putReadyFlag;
                            _addRow.Cells[colIsMultiSlot.Name].Value = _isMultiSlot;
                            _addRow.Cells[colSupportWaitFront.Name].Value = _supportWaitFront;
                            _addRow.Cells[colRecipeCheckFlag.Name].Value = _recipeCheck;
                            _addRow.Cells[colStageEnabled.Name].Value = _stageEnable;
                            _addRow.Cells[colRTCReworkFlag.Name].Value = _rtcReworkFlag;
                        }
                        else
                        {
                            dgvAddList.Rows.Add(_serverName, _robotName, _stageID, _stageName, _lineID, _localNo, _stageNoByNode, _stageType, iPriority, _stageReportTrxName, _stageJobDataTrxName,
                                _isMultiSlot, iSlotCount, _recipeCheck, _dummyCheck, _getReadyFlag, _putReadyFlag, _prefetchFlag,_rtcReworkFlag, _supportWaitFront,
                                _upstreamPathTrxName, _upstreamSendPathTrxName, _dnstreamPathTrxName, _dnstreamSendPathTrxName, _trackingDataList,
                                _cassetteType, _remark, _stageEnable, _slotFetchSeq, _slotStoreSeq,_exchangeType,_eqRobotIfType);
                        }

                        #endregion

                        #endregion

                        #region clear data

                        ClearData();

                        #endregion

                        break;
                        #endregion

                    case "OK":

                        #region OK

                        UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                        switch (FormMode)
                        {
                            case FormMode.AddNew:

                                #region Add

                                SBRM_ROBOT_STAGE _objAdd = null;

                                foreach (DataGridViewRow _row in dgvAddList.Rows)
                                {
                                    _objAdd = new SBRM_ROBOT_STAGE();

                                    int.TryParse(_row.Cells[colPriority.Name].Value.ToString(), out iPriority);
                                    int.TryParse(_row.Cells[colSlotMaxCount.Name].Value.ToString(), out iSlotCount);

                                    _objAdd.SERVERNAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                                    _objAdd.LINEID = _row.Cells[colLineID.Name].Value.ToString();
                                    _objAdd.ROBOTNAME = _row.Cells[colRobotName.Name].Value.ToString();
                                    _objAdd.STAGEID = _row.Cells[colStageID.Name].Value.ToString();

                                    _objAdd.NODENO = _row.Cells[colLocalNo.Name].Value.ToString();
                                    _objAdd.STAGETYPE = _row.Cells[colStageType.Name].Value.ToString();
                                    _objAdd.CASSETTETYPE = _row.Cells[colCassetteType.Name].Value.ToString();
                                    _objAdd.SLOTFETCHSEQ = _row.Cells[colSlotFetchSeq.Name].Value.ToString();
                                    _objAdd.SLOTSTORESEQ = _row.Cells[colSlotStoreSeq.Name].Value.ToString();
                                    _objAdd.EQROBOTIFTYPE = _row.Cells[colEQRobotIfType.Name].Value.ToString();
                                    _objAdd.EXCHANGETYPE = _row.Cells[colExchangeType.Name].Value.ToString();
                                        
                                    _objAdd.STAGEIDBYNODE = _row.Cells[colStageIDByNode.Name].Value.ToString();
                                    _objAdd.STAGENAME = _row.Cells[colStageName.Name].Value.ToString();
                                    _objAdd.PRIORITY = iPriority;
                                    _objAdd.STAGEREPORTTRXNAME = _row.Cells[colStageReportTrxName.Name].Value.ToString();
                                    _objAdd.SLOTMAXCOUNT = iSlotCount;
                                    _objAdd.STAGEJOBDATATRXNAME = _row.Cells[colStageJobDataTrxName.Name].Value.ToString();
                                    _objAdd.REMARKS = _row.Cells[colRemark.Name].Value.ToString();
                                    _objAdd.TRACKDATASEQLIST = _row.Cells[colTrackDataSeqList.Name].Value.ToString();
                                    _objAdd.UPSTREAMPATHTRXNAME = _row.Cells[colUpstreamPathTrxName.Name].Value.ToString();
                                    _objAdd.UPSTREAMJOBDATAPATHTRXNAME = _row.Cells[colUpstreamSendPathTrxName.Name].Value.ToString();
                                    _objAdd.DOWNSTREAMPATHTRXNAME = _row.Cells[colDownstreamPathTrxName.Name].Value.ToString();
                                    _objAdd.DOWNSTREAMJOBDATAPATHTRXNAME = _row.Cells[colDownstreamReceivePathTrxName.Name].Value.ToString();

                                    _objAdd.DUMMYCHECKFLAG = _row.Cells[colDummyCheckFlag.Name].Value.ToString();
                                    _objAdd.PREFETCHFLAG = _row.Cells[colPrefetchFlag.Name].Value.ToString();
                                    _objAdd.PUTREADYFLAG = _row.Cells[colPutReadyFlag.Name].Value.ToString();
                                    _objAdd.GETREADYFLAG = _row.Cells[colGetReadyFlag.Name].Value.ToString();
                                    _objAdd.ISMULTISLOT = _row.Cells[colIsMultiSlot.Name].Value.ToString();
                                    _objAdd.WAITFRONTFLAG = _row.Cells[colSupportWaitFront.Name].Value.ToString();
                                    _objAdd.RECIPECHENCKFLAG = _row.Cells[colRecipeCheckFlag.Name].Value.ToString();
                                    _objAdd.ISENABLED = _row.Cells[colStageEnabled.Name].Value.ToString();
                                    _objAdd.RTCREWORKFLAG = _row.Cells[colRTCReworkFlag.Name].Value.ToString();
                                    _ctxBRM.SBRM_ROBOT_STAGE.InsertOnSubmit(_objAdd);
                                }

                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify

                                if (ObjEditData == null) return;

                                if (CheckData() == false) return;

                                int.TryParse(txtPriority.Text.Trim(), out iPriority);
                                int.TryParse(txtSlotMaxCount.Text.Trim(), out iSlotCount);

                                ObjEditData.NODENO = ((dynamic)cboNode.SelectedItem).NodeNo;
                                ObjEditData.STAGETYPE = cboStageType.SelectedValue.ToString();
                                ObjEditData.CASSETTETYPE = cboCassetteType.SelectedValue.ToString();
                                ObjEditData.SLOTFETCHSEQ = cboSlotFetchSeq.SelectedValue.ToString();
                                ObjEditData.SLOTSTORESEQ = cboSlotStoreSeq.SelectedValue.ToString();
                                ObjEditData.EQROBOTIFTYPE = cboEQRobotIfType.SelectedValue.ToString();
                                ObjEditData.EXCHANGETYPE = cboExchangeType.SelectedValue.ToString();

                                ObjEditData.STAGEIDBYNODE = txtStageIDByNode.Text.Trim();
                                ObjEditData.STAGENAME = txtStageName.Text.Trim();
                                ObjEditData.PRIORITY = iPriority;
                                ObjEditData.STAGEREPORTTRXNAME = txtStageReportTrxName.Text.Trim();
                                ObjEditData.SLOTMAXCOUNT = iSlotCount;
                                ObjEditData.STAGEJOBDATATRXNAME = txtStageJobDataTrxName.Text.Trim();
                                if (FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("CHN") ||
                                    FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("SOR") ||
                                    FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("CRP") ||
                                    FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("RWT"))
                                {
                                    if (cboArmUsePriority.SelectedValue != null && cboArmUsePriority.SelectedValue.ToString() != "")
                                        _remark = cboArmUsePriority.SelectedValue.ToString().Trim();
                                    if (cboUseSpecificArm.SelectedValue != null && cboArmUsePriority.SelectedValue.ToString() != "")
                                        _remark += "," + cboUseSpecificArm.SelectedValue.ToString().Trim();
                                    ObjEditData.REMARKS = _remark;
                                }
                                else
                                {
                                    ObjEditData.REMARKS = strRemark;
                                }

                                ObjEditData.TRACKDATASEQLIST = txtTrackDataSeqList.Text.Trim();
                                ObjEditData.UPSTREAMPATHTRXNAME = txtUpstreamPathTrxName.Text.Trim();
                                ObjEditData.UPSTREAMJOBDATAPATHTRXNAME = txtUpstreamSendPathTrxName.Text.Trim();
                                ObjEditData.DOWNSTREAMPATHTRXNAME = txtDownstreamPathTrxName.Text.Trim();
                                ObjEditData.DOWNSTREAMJOBDATAPATHTRXNAME = txtDownstreamReceivePathTrxName.Text.Trim();

                                ObjEditData.DUMMYCHECKFLAG = chkDummyCheckFlag.Checked ? "Y" : "N";
                                ObjEditData.PREFETCHFLAG = chkPrefetchFlag.Checked ? "Y" : "N";
                                ObjEditData.PUTREADYFLAG = chkPutReadyFlag.Checked ? "Y" : "N";
                                ObjEditData.GETREADYFLAG = chkGetReadyFlag.Checked ? "Y" : "N";
                                ObjEditData.ISMULTISLOT = chkIsMultiSlot.Checked ? "Y" : "N";
                                ObjEditData.WAITFRONTFLAG = chkSupportWaitFront.Checked ? "Y" : "N";
                                ObjEditData.RECIPECHENCKFLAG = chkRecipeCheckFlag.Checked ? "Y" : "N";
                                ObjEditData.ISENABLED = chkStageEnabled.Checked ? "Y" : "N";
                                ObjEditData.RTCREWORKFLAG = chkRTCReworkFlag.Checked ? "Y" : "N";
                                break;
                                #endregion

                            default:
                                break;
                        }

                        this.DialogResult = System.Windows.Forms.DialogResult.OK;

                        break;
                        #endregion

                    case "Cancel":

                        #region Cancel
                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                        break;
                        #endregion

                    case "Clear":

                        #region Clear
                        ClearData();
                        break;
                        #endregion

                    default:

                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtTrackDataSeqList_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                FormRobotStageManagement_TrackingData _frm = new FormRobotStageManagement_TrackingData(txtTrackDataSeqList.Text);

                DialogResult _result = _frm.ShowDialog();

                if (_result == System.Windows.Forms.DialogResult.OK)
                {
                    txtTrackDataSeqList.Text = _frm.TrackingList;
                }

                if (_frm != null) _frm.Dispose();

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        #region Event
        private void txtInt_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (!char.IsNumber(e.KeyChar) &&
                        e.KeyChar != (char)Keys.Delete && e.KeyChar != (char)Keys.Enter && e.KeyChar != (char)Keys.Back);
        }

        private void dgvAddList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow _row = dgvAddList.SelectedRows[0];

                    if (_row != null)
                    {
                        cboRobotName.SelectedValue = _row.Cells[colRobotName.Name].Value.ToString();
                        cboNode.SelectedValue = _row.Cells[colLocalNo.Name].Value.ToString();
                        cboStageType.SelectedValue = _row.Cells[colStageType.Name].Value.ToString();
                        cboCassetteType.SelectedValue = _row.Cells[colCassetteType.Name].Value.ToString();
                        cboSlotFetchSeq.SelectedValue = _row.Cells[colSlotFetchSeq.Name].Value.ToString();
                        cboSlotStoreSeq.SelectedValue = _row.Cells[colSlotStoreSeq.Name].Value.ToString();
                        cboEQRobotIfType.SelectedValue = _row.Cells[colEQRobotIfType.Name].Value.ToString();
                        cboExchangeType.SelectedValue = _row.Cells[colExchangeType.Name].Value.ToString();
                        string[] remark = _row.Cells[colRemark.Name].Value.ToString().Split(',');
                        if (remark.Length >= 2)
                        {
                            cboArmUsePriority.SelectedValue = remark[0];
                            cboUseSpecificArm.SelectedValue = remark[1];
                        }
                        else if (remark.Length == 1)
                        {
                            cboArmUsePriority.SelectedValue = remark[0];
                            cboUseSpecificArm.SelectedIndex = -1;
                        }
                        else
                        {
                            cboArmUsePriority.SelectedValue = -1;
                            cboUseSpecificArm.SelectedIndex = -1;
                        }
                        txtStageID.Text = _row.Cells[colStageID.Name].Value.ToString();
                        txtStageIDByNode.Text = _row.Cells[colStageIDByNode.Name].Value.ToString();
                        txtStageName.Text = _row.Cells[colStageName.Name].Value.ToString();
                        txtPriority.Text = _row.Cells[colPriority.Name].Value.ToString();
                        txtStageReportTrxName.Text = _row.Cells[colStageReportTrxName.Name].Value.ToString();
                        txtSlotMaxCount.Text = _row.Cells[colSlotMaxCount.Name].Value.ToString();
                        txtStageJobDataTrxName.Text = _row.Cells[colStageJobDataTrxName.Name].Value.ToString();
                        txtUpstreamPathTrxName.Text = _row.Cells[colUpstreamPathTrxName.Name].Value.ToString();
                        txtUpstreamSendPathTrxName.Text = _row.Cells[colUpstreamSendPathTrxName.Name].Value.ToString();
                        txtDownstreamPathTrxName.Text = _row.Cells[colDownstreamPathTrxName.Name].Value.ToString();
                        txtDownstreamReceivePathTrxName.Text = _row.Cells[colDownstreamReceivePathTrxName.Name].Value.ToString();
                        txtTrackDataSeqList.Text = _row.Cells[colTrackDataSeqList.Name].Value.ToString();

                        chkDummyCheckFlag.Checked = _row.Cells[colDummyCheckFlag.Name].Value.ToString() == "Y" ? true : false;
                        chkPrefetchFlag.Checked = _row.Cells[colPrefetchFlag.Name].Value.ToString() == "Y" ? true : false;
                        chkGetReadyFlag.Checked = _row.Cells[colGetReadyFlag.Name].Value.ToString() == "Y" ? true : false;
                        chkPutReadyFlag.Checked = _row.Cells[colPutReadyFlag.Name].Value.ToString() == "Y" ? true : false;
                        chkIsMultiSlot.Checked = _row.Cells[colIsMultiSlot.Name].Value.ToString() == "Y" ? true : false;
                        chkSupportWaitFront.Checked = _row.Cells[colSupportWaitFront.Name].Value.ToString() == "Y" ? true : false;
                        chkRecipeCheckFlag.Checked = _row.Cells[colRecipeCheckFlag.Name].Value.ToString() == "Y" ? true : false;
                        chkStageEnabled.Checked = _row.Cells[colStageEnabled.Name].Value.ToString() == "Y" ? true : false;
                        chkRTCReworkFlag.Checked = _row.Cells[colRTCReworkFlag.Name].Value.ToString() == "Y" ? true : false;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboStageType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboStageType.SelectedIndex == -1) return;

                string _stageType = cboStageType.SelectedValue.ToString();

                switch (_stageType)
                {
                    case "PORT":
                        txtStageReportTrxName.Text = string.Empty;
                        txtStageJobDataTrxName.Text = string.Empty;
                        txtUpstreamPathTrxName.Text = string.Empty;
                        txtUpstreamSendPathTrxName.Text = string.Empty;
                        txtDownstreamPathTrxName.Text = string.Empty;
                        txtDownstreamReceivePathTrxName.Text = string.Empty;
                        chkRecipeCheckFlag.Checked = false;
                        cboCassetteType.SelectedIndex = 1;

                        pnlTrackDataSeqList.Visible = false;
                        pnlStageReportTrxName.Visible =false;
                        pnlStageJobDataTrxName.Visible = false;
                        pnlUpstreamPathTrxName.Visible = false;
                        pnlUpstreamSendPathTrxName.Visible = false;
                        pnlDownstreamPathTrxName.Visible = false;
                        pnlDownstreamReceivePathTrxName.Visible = false;
                        chkRecipeCheckFlag.Enabled = false;
                        pnlCassetteType.Visible = true;

                        break;

                    case "STAGE":
                        //txtStageReportTrxName.Text = string.Empty;
                        //txtStageJobDataTrxName.Text = string.Empty;
                        txtUpstreamPathTrxName.Text = string.Empty;
                        txtUpstreamSendPathTrxName.Text = string.Empty;
                        txtDownstreamPathTrxName.Text = string.Empty;
                        txtDownstreamReceivePathTrxName.Text = string.Empty;
                        chkRecipeCheckFlag.Checked = false;
                        cboCassetteType.SelectedIndex = 0;

                        pnlTrackDataSeqList.Visible = true;
                        pnlStageReportTrxName.Visible = true;
                        pnlStageJobDataTrxName.Visible = true;
                        pnlUpstreamPathTrxName.Visible = false;
                        pnlUpstreamSendPathTrxName.Visible = false;
                        pnlDownstreamPathTrxName.Visible = false;
                        pnlDownstreamReceivePathTrxName.Visible = false;
                        chkRecipeCheckFlag.Enabled = false;
                        pnlCassetteType.Visible = false;

                        break;

                    case "FIXBUFFER":
                        txtStageReportTrxName.Text = string.Empty;
                        txtStageJobDataTrxName.Text = string.Empty;
                        txtUpstreamPathTrxName.Text = string.Empty;
                        txtUpstreamSendPathTrxName.Text = string.Empty;
                        txtDownstreamPathTrxName.Text = string.Empty;
                        txtDownstreamReceivePathTrxName.Text = string.Empty;
                        chkRecipeCheckFlag.Checked = false;
                        cboCassetteType.SelectedIndex = 0;

                        pnlTrackDataSeqList.Visible = true;
                        pnlStageReportTrxName.Visible = false;
                        pnlStageJobDataTrxName.Visible = false;
                        pnlUpstreamPathTrxName.Visible = false;
                        pnlUpstreamSendPathTrxName.Visible = false;
                        pnlDownstreamPathTrxName.Visible = false;
                        pnlDownstreamReceivePathTrxName.Visible = false;
                        chkRecipeCheckFlag.Enabled = false;
                        pnlCassetteType.Visible = false;

                        break;

                    case "EQUIPMENT":
                        txtStageReportTrxName.Text = string.Empty;
                        txtStageJobDataTrxName.Text = string.Empty;
                        //txtUpstreamPathTrxName.Text = string.Empty;
                        //txtUpstreamSendPathTrxName.Text = string.Empty;
                        //txtDownstreamPathTrxName.Text = string.Empty;
                        //txtDownstreamReceivePathTrxName.Text = string.Empty;
                        //chkRecipeCheckFlag.Checked = false;
                        cboCassetteType.SelectedIndex = 0;

                        pnlTrackDataSeqList.Visible = true;
                        pnlStageReportTrxName.Visible = false;
                        pnlStageJobDataTrxName.Visible = false;
                        pnlUpstreamPathTrxName.Visible = true;
                        pnlUpstreamSendPathTrxName.Visible = true;
                        pnlDownstreamPathTrxName.Visible = true;
                        pnlDownstreamReceivePathTrxName.Visible = true;
                        chkRecipeCheckFlag.Enabled = true;
                        pnlCassetteType.Visible = false;

                        break;

                    default:
                        break;
                }
                CheckButtonEnableChange();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboArmUsePriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cboUseSpecificArm.DataBindings.Clear();
                List<comboxInfo> _lstUseSpecificArm = new List<comboxInfo>();
                if (cboArmUsePriority.SelectedValue != null)
                {
                    if (cboArmUsePriority.SelectedValue.ToString().Trim() == "L")
                    {
                        _lstUseSpecificArm.Add(new comboxInfo { ITEM_ID = string.Empty, ITEM_NAME = string.Empty });
                        _lstUseSpecificArm.Add(new comboxInfo { ITEM_ID = "UL", ITEM_NAME = "Up-Left" });
                        _lstUseSpecificArm.Add(new comboxInfo { ITEM_ID = "LL", ITEM_NAME = "Lower-Left" });
                    }

                    if (cboArmUsePriority.SelectedValue.ToString().Trim() == "R")
                    {
                        _lstUseSpecificArm.Add(new comboxInfo { ITEM_ID = string.Empty, ITEM_NAME = string.Empty });
                        _lstUseSpecificArm.Add(new comboxInfo { ITEM_ID = "UR", ITEM_NAME = "Up-Right" });
                        _lstUseSpecificArm.Add(new comboxInfo { ITEM_ID = "LR", ITEM_NAME = "Lower-Right" });
                    }

                cboUseSpecificArm.DataSource = _lstUseSpecificArm.ToList();
                cboUseSpecificArm.DisplayMember = "ITEM_NAME";
                cboUseSpecificArm.ValueMember = "ITEM_ID";
                cboUseSpecificArm.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboUseSpecificArm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboArmUsePriority.SelectedValue == null)
            {
                cboUseSpecificArm.SelectedIndex = -1;
            }
        }
        #endregion

        #region Method
        private void InitialCombox()
        {
            SetRobotName();
            SetNode();
            SetStageType();
            SetCassetteType();
            SetSlotSequence(cboSlotFetchSeq);
            SetSlotSequence(cboSlotStoreSeq);
            SetExchangeType();
            SetEQRobotifType();
        }

        private void SetRobotName()
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var data = (from robot in ctxBRM.SBRM_ROBOT
                            where robot.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                            select new
                            {
                                robot.ROBOTNAME
                            }).ToList();

                if (data == null || data.Count == 0) return;

                this.cboRobotName.DataSource = data;
                this.cboRobotName.DisplayMember = "ROBOTNAME";
                this.cboRobotName.ValueMember = "ROBOTNAME";
                this.cboRobotName.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetNode()
        {

            try
            {
                var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                         select new
                         {
                             IdNmae = string.Format("{0}-{1}-{2}", node.NodeNo, node.NodeID, node.NodeName),
                             NodeNo = node.NodeNo,
                             LineID = node.LineID,
                             NodeID = node.NodeID
                         }).Distinct().ToList();

                if (q == null || q.Count == 0) return;

                cboNode.DataSource = q;
                cboNode.DisplayMember = "IdNmae";
                cboNode.ValueMember = "NodeNo";
                cboNode.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetExchangeType()
        {


            List<comboxInfo> _lstExchangeType = new List<comboxInfo>();
            _lstExchangeType.Add(new comboxInfo { ITEM_ID = "", ITEM_NAME = "No Support" });
            _lstExchangeType.Add(new comboxInfo { ITEM_ID = "EXCHANGE", ITEM_NAME = "Exchange" });
            _lstExchangeType.Add(new comboxInfo { ITEM_ID = "GETPUT", ITEM_NAME = "Get&Put" });
            _lstExchangeType.Add(new comboxInfo { ITEM_ID = "MAC_EXCHANGE", ITEM_NAME = "Marco Exchange" });

            cboExchangeType.DataSource = _lstExchangeType.ToList();
            cboExchangeType.DisplayMember = "ITEM_NAME";
            cboExchangeType.ValueMember = "ITEM_ID";
            cboExchangeType.SelectedIndex = -1;

        }

        private void SetEQRobotifType()
        {
            List<comboxInfo> _lstEQRobotIfType = new List<comboxInfo>();
            _lstEQRobotIfType.Add(new comboxInfo { ITEM_ID = "NORMAL", ITEM_NAME = "NORMAL" });
            _lstEQRobotIfType.Add(new comboxInfo { ITEM_ID = "MULTI", ITEM_NAME = "MULTI" });
            _lstEQRobotIfType.Add(new comboxInfo { ITEM_ID = "BOTH", ITEM_NAME = "BOTH" });
            _lstEQRobotIfType.Add(new comboxInfo { ITEM_ID = "BOTHGETPUT", ITEM_NAME = "BOTHGETPUT" });

            cboEQRobotIfType.DataSource = _lstEQRobotIfType.ToList();
            cboEQRobotIfType.DisplayMember = "ITEM_NAME";
            cboEQRobotIfType.ValueMember = "ITEM_ID";
            cboEQRobotIfType.SelectedIndex = -1;

        }

        private void SetStageType()
        {
            try
            {
                List<comboxInfo> _lstStageType = new List<comboxInfo>();
                _lstStageType.Add(new comboxInfo { ITEM_ID = "PORT", ITEM_NAME = "Cassette Port" });
                _lstStageType.Add(new comboxInfo { ITEM_ID = "STAGE", ITEM_NAME = "Indexer inside Stage" });
                _lstStageType.Add(new comboxInfo { ITEM_ID = "FIXBUFFER", ITEM_NAME = "Indexer Fix Buffer" });
                _lstStageType.Add(new comboxInfo { ITEM_ID = "EQUIPMENT", ITEM_NAME = "Equipment" });

                cboStageType.DataSource = _lstStageType.ToList();
                cboStageType.DisplayMember = "ITEM_NAME";
                cboStageType.ValueMember = "ITEM_ID";
                cboStageType.SelectedIndex = -1;

                cboStageType.SelectedIndexChanged += new EventHandler(cboStageType_SelectedIndexChanged);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetCassetteType()
        {
            try
            {

                List<comboxInfo> _lstCstType = new List<comboxInfo>();
                _lstCstType.Add(new comboxInfo { ITEM_ID = "", ITEM_NAME = "" });
                _lstCstType.Add(new comboxInfo { ITEM_ID = "SEQUENCE", ITEM_NAME = "Sequence Cassette" });
                _lstCstType.Add(new comboxInfo { ITEM_ID = "RANDOM", ITEM_NAME = "Random Cassette" });

                cboCassetteType.DataSource = _lstCstType.ToList();
                cboCassetteType.DisplayMember = "ITEM_NAME";
                cboCassetteType.ValueMember = "ITEM_ID";
                cboCassetteType.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetSlotSequence(ComboBox cbo)
        {
            try
            {

                List<comboxInfo> _lstSlotSeq = new List<comboxInfo>();
                _lstSlotSeq.Add(new comboxInfo { ITEM_ID = "ASC", ITEM_NAME = "ASC" });
                _lstSlotSeq.Add(new comboxInfo { ITEM_ID = "DESC", ITEM_NAME = "DESC" });

                cbo.DataSource = _lstSlotSeq.ToList();
                cbo.DisplayMember = "ITEM_NAME";
                cbo.ValueMember = "ITEM_ID";
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetArmUsePriority()
        {
            try
            {
                List<comboxInfo> _lstArmUsePriority = new List<comboxInfo>();
                _lstArmUsePriority.Add(new comboxInfo { ITEM_ID = "", ITEM_NAME = string.Empty });
                _lstArmUsePriority.Add(new comboxInfo { ITEM_ID = "L", ITEM_NAME = "Left" });
                _lstArmUsePriority.Add(new comboxInfo { ITEM_ID = "R", ITEM_NAME = "Right" });
                cboArmUsePriority.DataSource = _lstArmUsePriority.ToList();
                cboArmUsePriority.DisplayMember = "ITEM_NAME";
                cboArmUsePriority.ValueMember = "ITEM_ID";
                cboArmUsePriority.SelectedIndex = 1;

                cboArmUsePriority.SelectedIndexChanged += new EventHandler(cboArmUsePriority_SelectedIndexChanged);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetUseSpecificArm()
        {
            try
            {
                List<comboxInfo> _lstUseSpecificArm = new List<comboxInfo>();
                _lstUseSpecificArm.Add(new comboxInfo { ITEM_ID = "", ITEM_NAME = string.Empty });
                _lstUseSpecificArm.Add(new comboxInfo { ITEM_ID = "UL", ITEM_NAME = "Upper-Left" });
                _lstUseSpecificArm.Add(new comboxInfo { ITEM_ID = "LL", ITEM_NAME = "Lower-Left" });
                _lstUseSpecificArm.Add(new comboxInfo { ITEM_ID = "UR", ITEM_NAME = "Upper-Right" });
                _lstUseSpecificArm.Add(new comboxInfo { ITEM_ID = "LR", ITEM_NAME = "Lower-Right" });
                cboUseSpecificArm.DataSource = _lstUseSpecificArm.ToList();
                cboUseSpecificArm.DisplayMember = "ITEM_NAME";
                cboUseSpecificArm.ValueMember = "ITEM_ID";
                cboUseSpecificArm.SelectedIndex = -1;

                cboUseSpecificArm.SelectedIndexChanged += new EventHandler(cboUseSpecificArm_SelectedIndexChanged);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
 
        }

        private bool CheckData()
        {
            try
            {
                int iTemp = 0;

                if (cboNode.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Local No required！", MessageBoxIcon.Warning);
                    cboNode.DroppedDown = true;
                    return false;
                }
                 
                //if (string.IsNullOrEmpty(this.txtStageID.Text.Trim()))
                //{
                //    ShowMessage(this, this.lblCaption.Text, "", "Stage ID required！", MessageBoxIcon.Warning);
                //    this.txtStageID.Focus();
                //    return false;
                //}

                //int iStageID = 0;
                //int.TryParse(txtStageID.Text.Trim(), out iStageID);
                //if (iStageID == 0)
                //{
                //    ShowMessage(this, this.lblCaption.Text, "", "Stage ID must between 01 and 99！", MessageBoxIcon.Warning);
                //    this.txtStageID.Focus();
                //    return false;
                //}
                if (txtStageID.Text.Length < 2)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Stage ID length will be for 2！", MessageBoxIcon.Warning);
                    this.txtStageID.Focus();
                    return false;
                }


                if (this.cboRobotName.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Robot Name required！", MessageBoxIcon.Warning);
                    this.cboRobotName.DroppedDown = true;
                    return false;
                }

                if (this.cboEQRobotIfType.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "EQRobotIf Type required！", MessageBoxIcon.Warning);
                    this.cboEQRobotIfType.DroppedDown = true;
                    return false;
                }

                if (this.cboExchangeType.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Exchange Type required！", MessageBoxIcon.Warning);
                    this.cboExchangeType.DroppedDown = true;
                    return false;
                }

                #region Stage在所屬的Node內的編碼，目前設定與Stage ID欄位相同
                if (string.IsNullOrEmpty(this.txtStageIDByNode.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Stage ID By Local required！", MessageBoxIcon.Warning);
                    this.txtStageIDByNode.Focus();
                    return false;
                }
                #endregion

                #region Priority : Robot服務Stage的优先级 从1 <2<3<4 數字越大優先級越高
                if (int.TryParse(txtPriority.Text.Trim(), out iTemp) == false || iTemp <= 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Priority must greater than 0！", MessageBoxIcon.Warning);
                    this.txtPriority.Focus();
                    this.txtPriority.SelectAll();
                    return false;
                }
                #endregion

                #region Stage Type
                if (cboStageType.SelectedIndex == -1)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Please Choose Stage Type！", MessageBoxIcon.Warning);
                    return false;
                }
                else
                {
                    string _stageType = cboStageType.SelectedValue.ToString();

                    #region  Cassette Type =>  當Stage Type為'PORT'使用,其它值填入''
                    if ("PORT".Equals(_stageType))
                    {
                        if (cboCassetteType.SelectedIndex == -1)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Please Choose Cassette Type！", MessageBoxIcon.Warning);
                            return false;
                        }
                        else
                        {
                            if (cboCassetteType.SelectedValue.ToString() == string.Empty)
                            {
                                ShowMessage(this, this.lblCaption.Text, "", "Cassette Type can't be empty when Stage Type is port type！", MessageBoxIcon.Warning);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (cboCassetteType.SelectedValue.ToString() != string.Empty)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Cassette Type must be empty when Stage Type isn't port type！", MessageBoxIcon.Warning);
                            return false;
                        }
                    }
                    #endregion

                    #region Path Trx Name => 當StageType='EQUIPMENT'時使用，只能有下列三種狀況:1.是Upstream Send Jobdata Path Trx Name和 Upstream Path Trx Name一組2:是Downstream Path Trx Name和Downstream Receive Jobdata Path Trx Name一組 3:四個為一組，其餘方法都算錯誤
                    if (_stageType == "EQUIPMENT")
                    {
                        if (txtDownstreamPathTrxName.Text == string.Empty || txtDownstreamReceivePathTrxName.Text ==string.Empty)//3或4沒有，表1和2一定要有
                        {
                            #region txtDownStramPathTrxName and  txtDownstreamReceivePathTrxName Miss
                            if (txtDownstreamPathTrxName.Text != string.Empty || txtDownstreamReceivePathTrxName.Text != string.Empty )//當3或4其中有內容時，表缺少其中一個
                            {
                                if (txtDownstreamPathTrxName.Text != string.Empty)
                                {
                                    ShowMessage(this, this.lblCaption.Text, "", "please inpute Downstream Receive Jobdata Path Trx Name", MessageBoxIcon.Warning);
                                    txtDownstreamReceivePathTrxName.Focus();
                                    txtDownstreamReceivePathTrxName.SelectAll();
                                    return false;
                                }
                                else
                                {
                                    ShowMessage(this, this.lblCaption.Text, "", "please inpute Downstream Path Trx Name", MessageBoxIcon.Warning);
                                    txtDownstreamPathTrxName.Focus();
                                    txtDownstreamPathTrxName.SelectAll();
                                    return false;
                                }
                            }
                            #region Upstream Path Trx Name
                            if (txtUpstreamPathTrxName.Text == string.Empty)
                            {
                                ShowMessage(this, this.lblCaption.Text, "", "please inpute Upstream Path Trx Name", MessageBoxIcon.Warning);
                                txtUpstreamPathTrxName.Focus();
                                txtUpstreamPathTrxName.SelectAll();
                                return false;
                            }
                            #endregion

                            #region Upstream Send Jobdata Path Trx Name
                            if (txtUpstreamSendPathTrxName.Text == string.Empty)
                            {
                                ShowMessage(this, this.lblCaption.Text, "", "please inpute Upstream Send Jobdata Path Trx Name", MessageBoxIcon.Warning);
                                txtUpstreamSendPathTrxName.Focus();
                                txtUpstreamSendPathTrxName.SelectAll();
                                return false;
                            }
                            #endregion
                        #endregion
                        }
                        else if (txtUpstreamPathTrxName.Text == string.Empty || txtUpstreamSendPathTrxName.Text == string.Empty) //1或2沒有 ，表3和4一定要有
                        {
                            #region txtUpstreamPathTrxName and txtUpstreamSendPathTrxName Miss
                            if (txtUpstreamPathTrxName.Text != string.Empty || txtUpstreamSendPathTrxName.Text != string.Empty)//當1或2其中有內容時，表缺少其中一個
                            {
                                if (txtUpstreamSendPathTrxName.Text != string.Empty)
                                {
                                    ShowMessage(this, this.lblCaption.Text, "", "please inpute Upstream Path Trx Name", MessageBoxIcon.Warning);
                                    txtUpstreamPathTrxName.Focus();
                                    txtUpstreamPathTrxName.SelectAll();
                                    return false;
                                }
                                else
                                {
                                    ShowMessage(this, this.lblCaption.Text, "", "please inpute Upstream Send Jobdata Path Trx Name", MessageBoxIcon.Warning);
                                    txtUpstreamSendPathTrxName.Focus();
                                    txtUpstreamSendPathTrxName.SelectAll();
                                    return false;
                                }
                            }

                            #region Downstream Path Trx Name
                            if (txtDownstreamPathTrxName.Text == string.Empty)
                            {
                                ShowMessage(this, this.lblCaption.Text, "", "please inpute Downstream Path Trx Name", MessageBoxIcon.Warning);
                                txtDownstreamPathTrxName.Focus();
                                txtDownstreamPathTrxName.SelectAll();
                                return false;
                            }
                            #endregion

                            #region Downstream Receive Jobdata Path Trx Name
                            if (txtDownstreamReceivePathTrxName.Text == string.Empty)
                            {
                                ShowMessage(this, this.lblCaption.Text, "", "please inpute Downstream Receive Jobdata Path Trx Name", MessageBoxIcon.Warning);
                                txtDownstreamReceivePathTrxName.Focus();
                                txtDownstreamReceivePathTrxName.SelectAll();
                                return false;
                            }
                            #endregion
                            #endregion
                        }

                    }
                    else
                    {
                        #region Upstream Path Trx Name
                        if (txtUpstreamPathTrxName.Text != string.Empty)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Upstream Path Trx Name must be empty", MessageBoxIcon.Warning);
                            txtUpstreamPathTrxName.Text = string.Empty;
                        }
                        #endregion

                        #region Upstream Send Jobdata Path Trx Name
                        if (txtUpstreamSendPathTrxName.Text != string.Empty)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Upstream Send Jobdata Path Trx Name must be empty", MessageBoxIcon.Warning);
                            txtUpstreamSendPathTrxName.Text = string.Empty;
                        }
                        #endregion

                        #region Downstream Path Trx Name
                        if (txtDownstreamPathTrxName.Text != string.Empty)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Downstream Path Trx Name must be empty", MessageBoxIcon.Warning);
                            txtDownstreamPathTrxName.Text = string.Empty;
                        }
                        #endregion

                        #region Downstream Receive Jobdata Path Trx Name
                        if (txtDownstreamReceivePathTrxName.Text != string.Empty)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Downstream Receive Jobdata Path Trx Name must be empty", MessageBoxIcon.Warning);
                            txtDownstreamReceivePathTrxName.Text = string.Empty;
                        }
                        #endregion
                    }
                    #endregion

                    #region PLC Trx Name => for Stage Type='STAGE' User Only
                    if (_stageType == "STAGE")
                    {
                        #region Stage's PLC Trx Name
                        if (txtStageReportTrxName.Text == string.Empty)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "please inpute Stage's PLC Trx Name", MessageBoxIcon.Warning);
                            txtStageReportTrxName.Focus();
                            txtStageReportTrxName.SelectAll();
                            return false;
                        }
                        #endregion

                        #region Stage's PLC Job Data Trx Name
                        if (txtStageJobDataTrxName.Text == string.Empty)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "please inpute Stage's PLC Job Data Trx Name", MessageBoxIcon.Warning);
                            txtStageJobDataTrxName.Focus();
                            txtStageJobDataTrxName.SelectAll();
                            return false;
                        }

                        #endregion
                    }
                    #endregion
                }
                #endregion

                #region Max Slot Count
                if (int.TryParse(txtSlotMaxCount.Text.Trim(), out iTemp) == false || iTemp <= 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Slot Max count must greater than 0！", MessageBoxIcon.Warning);
                    txtSlotMaxCount.Focus();
                    txtSlotMaxCount.SelectAll();
                    return false;
                }
                #endregion

                #region Slot Fetch Sequence
                if (cboSlotFetchSeq.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Slot Fetch Sequence required！", MessageBoxIcon.Warning);
                    cboSlotFetchSeq.DroppedDown = true;
                    return false;
                }
                #endregion

                #region Slot Store Sequence
                if (cboSlotStoreSeq.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Slot Store Sequence required！", MessageBoxIcon.Warning);
                    cboSlotStoreSeq.DroppedDown = true;
                    return false;
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool CheckUniqueKeyOK(string StageID, string RobotName)
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                string _serverName = FormMainMDI.G_OPIAp.CurLine.ServerName;

                // 資料庫資料
                int rowCnt = (from stage in ctxBRM.SBRM_ROBOT_STAGE
                              where stage.SERVERNAME == _serverName && stage.STAGEID == StageID && stage.ROBOTNAME == RobotName
                              select stage).Count();
                if (rowCnt > 0)
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key : \r\n ServerName [{0}], Robot Name [{1}], Stage ID[{2}]！", _serverName, RobotName, StageID);
                    ShowMessage(this, lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return false;
                }

                //已修改未更新物件
                if (FormMode == UniOPI.FormMode.AddNew)
                {
                    var add = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_STAGE>().Where(
                        stage => stage.SERVERNAME == _serverName && stage.STAGEID == StageID && stage.ROBOTNAME == RobotName);
                    if (add.Count() > 0)
                    {
                        string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key : \r\n ServerName [{0}], Robot Name [{1}], Stage ID[{2}]！", _serverName, RobotName, StageID);
                        ShowMessage(this, lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return false;

            }
        }

        private void ClearData()
        {
            cboRobotName.SelectedIndex = -1;
            cboNode.SelectedIndex = -1;
            cboStageType.SelectedIndex = -1;
            cboCassetteType.SelectedIndex = -1;
            cboSlotFetchSeq.SelectedIndex = 0;
            cboSlotStoreSeq.SelectedIndex = 0;
            cboEQRobotIfType.SelectedIndex = 0;
            cboExchangeType.SelectedIndex = 0;
            cboArmUsePriority.SelectedIndex = -1;
            cboUseSpecificArm.SelectedIndex = -1;
            cboUseSpecificArm.DataSource = null;

            txtStageID.Text = string.Empty;
            txtStageIDByNode.Text = string.Empty;
            txtStageName.Text = string.Empty;
            txtPriority.Text = "0";
            txtStageReportTrxName.Text = string.Empty;
            txtSlotMaxCount.Text = "0";
            txtStageJobDataTrxName.Text = string.Empty;
            txtUpstreamPathTrxName.Text = string.Empty;
            txtUpstreamSendPathTrxName.Text = string.Empty;
            txtDownstreamPathTrxName.Text = string.Empty;
            txtDownstreamReceivePathTrxName.Text = string.Empty;
            txtTrackDataSeqList.Text = string.Empty;

            chkDummyCheckFlag.Checked = false;
            chkPrefetchFlag.Checked = false;
            chkPutReadyFlag.Checked = false;
            chkIsMultiSlot.Checked = false;
            chkGetReadyFlag.Checked = false;
            chkSupportWaitFront.Checked = false;
            chkRecipeCheckFlag.Checked = false;
            chkStageEnabled.Checked = true;
            chkRTCReworkFlag.Checked = false;

            this.ActiveControl = cboRobotName;
        }
        #endregion

        private void txtSlotMaxCount_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSlotMaxCount.Text) == false && int.Parse(txtSlotMaxCount.Text) > 1)
            {
                chkIsMultiSlot.Checked = true;
            }
            else
            {
                chkIsMultiSlot.Checked = false;
            }
        }

        private void cboStageType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cboStageType.SelectedValue.ToString().Equals("PORT"))
            {
                pnlSlotFetchSeq.Visible = false;
                pnlSlotStoreSeq.Visible = false;
                cboSlotFetchSeq.SelectedIndex = 0;
                cboSlotStoreSeq.SelectedIndex = 1;

            }
            else
            {
                pnlSlotFetchSeq.Visible = true;
                pnlSlotStoreSeq.Visible = true;
                cboSlotFetchSeq.SelectedIndex = -1;
                cboSlotStoreSeq.SelectedIndex = -1;

            }
        }

        private void txtStageID_TextChanged(object sender, EventArgs e)
        {
            txtStageIDByNode.Text = txtStageID.Text;
        }

        private void CheckButtonEnableChange()
        {
            foreach (Control sender in flpFloatArea2.Controls)
            {
                if  ((CheckBox)sender is CheckBox &&((CheckBox)sender).Enabled == false)
                {
                    ((CheckBox)sender).Visible = false;
                }
                else
                    ((CheckBox)sender).Visible = true;
            }
        }

        private void cboExchangeType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cboExchangeType.SelectedIndex == 0)
            {
                ShowMessage(this, lblCaption.Text, "", "No Support Exchange and Get&Put.", MessageBoxIcon.Information);
            }
        }

    }
}
