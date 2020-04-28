using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobotRoute : FormBase
    {
        Robot CurRobot = null;              //目前選取的robot
        int CurrRoute_RowIndex = -1;        //目前選取的robot route row index (dgvRobotRoute row index)
        string CurRouteID = string.Empty;   //目前選取的route id
        string TrackDataDefault = "0".PadLeft(32, '0');
        int TrackingDataLength = 32;

        Dictionary<string, StageTracking> StageTrackData; //用來儲存[SBRM_ROBOT_STAGE]資料表中的[TRACKDATASEQLIST]

        Dictionary<string, StageTrackingDesc> TrackDataDesc; // Key:Offset ,Value: ITEMDESC,ITEMLENGTH

        SBRM_ROBOT_ROUTE_STEP CurRouteStepData = null;

        public FormRobotRoute()
        {
            InitializeComponent();
        }

        private void FormRobotRoute_Load(object sender, EventArgs e)
        {
            try
            {
                #region show robot image
                string _robotPath = OPIConst.RobotFolder + string.Format(FormMainMDI.G_OPIAp.CurLine.ServerName + ".png");

                if (File.Exists(_robotPath))
                {
                    pnlRobotPic.BackgroundImage = new Bitmap(_robotPath);
                }
                #endregion

                if (FormMainMDI.G_OPIAp.CurLine.FabType == "CF")
                {
                    TrackDataDefault = "0".PadLeft(16, '0');
                    TrackingDataLength = 16;
                }
                StageTrackData = new Dictionary<string, StageTracking>();
                TrackDataDesc = new Dictionary<string, StageTrackingDesc>();

                Load_TrackingDataDesc();

                Load_RobotName();

                #region  Close HorizontalScroll (Rex補
                flpStage.VerticalScroll.Enabled = true;
                flpStage.VerticalScroll.Visible = true;
                flpStage.HorizontalScroll.Enabled = false;
                flpStage.HorizontalScroll.Visible = false;
                flpStage.HorizontalScroll.Maximum = 0;
                flpStage.AutoScroll = true;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void dgvRobotRoute_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

                dgvRobotRoute.CurrentCell = dgvRobotRoute.Rows[e.RowIndex].Cells[e.ColumnIndex];

                if (e.RowIndex >= 0 && dgvRobotRoute.Rows[e.RowIndex].DataBoundItem != null)
                {
                    dgvRobotRoute.Rows[e.RowIndex].Selected = true;

                    CurrRoute_RowIndex = e.RowIndex;

                    CurRouteID = dgvRobotRoute.Rows[e.RowIndex].Cells[colRouteID.Name].Value.ToString();

                    GetGridViewData_RouteStep(CurRouteID);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvRouteStep_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && dgvRouteStep.Rows[e.RowIndex].DataBoundItem != null)
                {
                    dgvRouteStep.CurrentCell = dgvRouteStep.Rows[e.RowIndex].Cells[colStepID.Name];

                    dgvRouteStep.Rows[e.RowIndex].Selected = true;

                    CurRouteStepData = (dgvRouteStep.SelectedRows[0].DataBoundItem) as SBRM_ROBOT_ROUTE_STEP;

                    #region 如果 InputTrackData沒有值寫入00000000000000
                    if (CurRouteStepData.INPUTTRACKDATA == null || CurRouteStepData.INPUTTRACKDATA.ToString().Trim() == string.Empty)
                        CurRouteStepData.INPUTTRACKDATA = TrackDataDefault;

                    if (CurRouteStepData.OUTPUTTRACKDATA == null || CurRouteStepData.OUTPUTTRACKDATA.ToString().Trim() == string.Empty)
                        CurRouteStepData.OUTPUTTRACKDATA = TrackDataDefault;
                    #endregion

                    SetRouteStepScreen();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void rdoRobot_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is RadioButton)) return;

                RadioButton _rdo = sender as RadioButton;

                #region Reset Radio button Color
                foreach (RadioButton _rdoOld in flpRadioButton.Controls.OfType<RadioButton>())
                {
                    _rdoOld.ForeColor = Color.Black;
                }
                #endregion

                ResetRouteStepScreen();

                _rdo.ForeColor = Color.Blue;

                #region 取得current robot
                if (FormMainMDI.G_OPIAp.Dic_Robot.ContainsKey(_rdo.Name))
                    CurRobot = FormMainMDI.G_OPIAp.Dic_Robot[_rdo.Name];
                else
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Robot required！", MessageBoxIcon.Error);
                    return;
                }
                #endregion

                CurrRoute_RowIndex = -1;

                CreateObject_Stage();

                GetGridViewData_RobotRoute();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void chkStage_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is CheckBox)) return;

                if (CurRouteStepData == null) return;

                CheckBox _chk = (sender as CheckBox);

                #region 當Robot Rule為Only時，Stage選項只能點選一個    (Rex補
                if (_chk.Tag.ToString() == "Stage" && CurRouteStepData.ROBOTRULE == "ONLY")
                {
                    int _checkNumber = 0;
                    foreach (CheckBox _CheckButton in flpStage.Controls)
                    {
                        if (_CheckButton.Checked == true)
                        {
                            _checkNumber++;
                            if (_checkNumber > 1)
                            {
                                ShowMessage(this, lblCaption.Text, "", "Only Check Once!", MessageBoxIcon.Error);
                                _chk.Checked = false;
                                return;
                            }
                        }
                    }
                }
                #endregion

                _chk.AutoCheck = true;

                string _cstData = string.Empty;
                string _stageData = string.Empty;

                if (btnTargetCassette.BackColor == Color.Lime)
                {
                    _cstData = GetStageList(flpPort);
                }

                if (btnTargetStage.BackColor == Color.Lime)
                {
                    _stageData = GetStageList(flpStage);
                }

                CurRouteStepData.STAGEIDLIST = _cstData + (_cstData == string.Empty ? "" : _stageData==string.Empty?"":",") + _stageData;

                #region 顯示tracking data
                if (_chk.Tag.ToString() == "Stage")
                {
                    if (StageTrackData.ContainsKey(_chk.Name))
                    {
                        //Load Tracking checkbox
                        CreateObject_TrackingData(_chk.Name);

                        //若取消stage勾選，需清除tracking data設定
                        if (_chk.Checked == false)
                        {
                            StageTrackData[_chk.Name].UseTrackData.Clear();

                            foreach (CheckBox _chkTrack in flpTracking.Controls.OfType<CheckBox>().Where(r => r.Checked.Equals(true)))
                            {
                                _chkTrack.Checked = false;
                            }
                        }
                    }
                }
                #endregion

                #region 將目前選取的stage字改為藍色顯示
                foreach (CheckBox _chkNoChoose in flpStage.Controls.OfType<CheckBox>().Where(r => r.ForeColor.Equals(Color.Blue)))
                {
                    _chkNoChoose.ForeColor = Color.Black;
                }

                foreach (CheckBox _chkNoChoose in flpPort.Controls.OfType<CheckBox>().Where(r => r.ForeColor.Equals(Color.Blue)))
                {
                    _chkNoChoose.ForeColor = Color.Black;
                }

                _chk.ForeColor = Color.Blue;
                #endregion

                //Show Target Detail DataGridView
                ShowTargetDetail();

                Update_TrackingData(CurRouteStepData);

                SetGridViewColor();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void chkStage_Leave(object sender, EventArgs e)
        {
            try
            {
                CheckBox _chk = (sender as CheckBox);

                _chk.AutoCheck = false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void chkTracking_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is CheckBox)) return;

                CheckBox _chk = (sender as CheckBox);

                if (CurRouteStepData == null) return;

                string[] _chooseStage = CurRouteStepData.STAGEIDLIST.ToString().Split(',');

                string _stageID = _chk.Tag.ToString().Split(';')[0].ToString();
                string _trackNo = _chk.Tag.ToString().Split(';')[1].ToString();

                if (_chooseStage.Contains(_stageID) == false) { _chk.Checked = false; return; }

                if (StageTrackData.ContainsKey(_stageID))
                {
                    if (_chk.Checked)
                    {
                        if (StageTrackData[_stageID].UseTrackData.Contains(_trackNo) == false)
                            StageTrackData[_stageID].UseTrackData.Add(_trackNo);
                    }
                    else
                    {
                        if (StageTrackData[_stageID].UseTrackData.Contains(_trackNo))
                            StageTrackData[_stageID].UseTrackData.Remove(_trackNo);
                    }
                }

                //Show Target Detail DataGridView
                ShowTargetDetail();

                //將修改的track更新至對應的欄位
                Update_TrackingData(CurRouteStepData);

                SetGridViewColor();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnNextStep_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurRouteStepData == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose Step!", MessageBoxIcon.Error);
                    return;
                }

                FormRobotRouteNextStepSet _frm = new FormRobotRouteNextStepSet(CurRouteStepData, dgvRouteStep);

                _frm.ShowDialog();

                if (_frm != null) _frm.Dispose();

                SetGridViewColor();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnCurrentStep_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurRouteStepData == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose Step!", MessageBoxIcon.Error);
                    return;
                }

                FormRobotRouteCurrentStepSet _frm = new FormRobotRouteCurrentStepSet(CurRouteStepData, dgvRouteStep);

                _frm.ShowDialog();

                if (_frm != null) _frm.Dispose();

                SetGridViewColor();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnAddRouteStep_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvRobotRoute.DataSource == null) return; //判斷RoutID null離開

                if (dgvRobotRoute.Rows.Count == 0) return; //判斷RoutID無值離開

                flpTracking.Controls.Clear(); // 清除Tracking Data

                int _stepID = 0;

                int _newStepID = 0;

                foreach (DataGridViewRow _row in dgvRouteStep.Rows)
                {
                    int.TryParse(_row.Cells[0].Value.ToString(), out _stepID);

                    if (_stepID > _newStepID) _newStepID = _stepID;
                }

                #region New data
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_ROBOT_ROUTE_STEP _objAdd = new SBRM_ROBOT_ROUTE_STEP();

                _objAdd.SERVERNAME = CurRobot.ServerName;
                _objAdd.ROBOTNAME = CurRobot.RobotName;
                _objAdd.ROUTEID = CurRouteID;
                _objAdd.STEPID = _newStepID + 1;
                _objAdd.NEXTSTEPID = 65535;
                _objAdd.LINETYPE = CurRobot.LineType;
                _objAdd.DESCRIPTION = string.Empty; //this.dgvRobotRoute.CurrentRow.Cells[colDescription.Name].Value.ToString();
                _objAdd.STAGEIDLIST = string.Empty;
                _objAdd.ROBOTRULE = string.Empty;
                _objAdd.ROBOTUSEARM = string.Empty;
                _objAdd.ROBOTACTION = string.Empty;
                _objAdd.INPUTTRACKDATA = TrackDataDefault;
                _objAdd.OUTPUTTRACKDATA = TrackDataDefault;
                _objAdd.LASTUPDATETIME = DateTime.Now;
                _objAdd.CROSSSTAGEFLAG = chkCrossStageFlag.Checked ? "Y" : "N";
                _ctxBRM.SBRM_ROBOT_ROUTE_STEP.InsertOnSubmit(_objAdd);

                #endregion

                GetGridViewData_RouteStep(CurRouteID);

                dgvRouteStep.CurrentCell = dgvRouteStep.Rows[dgvRouteStep.Rows.Count - 1].Cells[0];

                DataGridViewCellEventArgs _e = new DataGridViewCellEventArgs(0, dgvRouteStep.Rows.Count - 1);

                dgvRouteStep_CellClick(dgvRouteStep, _e); //觸發Cell Click事件

                SetGridViewColor();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteStep_Click(object sender, EventArgs e)
        {
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

            try
            {
                if (CurRouteStepData == null) return; // Step無資料離開

                int _stepID = int.Parse(CurRouteStepData.STEPID.ToString());

                if (QuectionMessage(this, lblCaption.Text, string.Format("Are you sure to delete Route ID [{0}], step [{1}] records?", CurRouteID, _stepID.ToString())) == System.Windows.Forms.DialogResult.No)
                    return;

                _ctxBRM.SBRM_ROBOT_ROUTE_STEP.DeleteOnSubmit(CurRouteStepData);

                #region 有使用此next step id的更新為65535
                foreach (DataGridViewRow _row in dgvRouteStep.Rows)
                {
                    if (_row.Cells["colNextStep"].Value.ToString() == CurRouteStepData.STEPID.ToString())
                    {
                        SBRM_ROBOT_ROUTE_STEP _obj = (_row.DataBoundItem) as SBRM_ROBOT_ROUTE_STEP;

                        _obj.NEXTSTEPID = 65535;

                        _row.Cells["colNextStep"].Value = "65535";
                    }
                }
                #endregion

                Delete_RouteRelation(CurRouteID, _stepID.ToString());

                SetGridViewColor();

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //刪除所選的Route Mode對應的全部 Step 在 SBRM_ROBOT_ROUTE_STEP
        private void btnDeleteRoute_Click(object sender, EventArgs e)
        {
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
            string _sqlErr = string.Empty;
            string _sqlDesc = string.Empty;

            try
            {
                if (dgvRouteStep.Rows.Count == 0) return; // Step無資料離開

                if (QuectionMessage(this, lblCaption.Text, string.Format("Are you sure to delete [{0}] run mode all step records?", CurRouteID)) == System.Windows.Forms.DialogResult.Yes)
                {

                    var objSelDelete = from del in _ctxBRM.SBRM_ROBOT_ROUTE_STEP
                                       where del.ROBOTNAME == CurRobot.RobotName && del.ROUTEID == CurRouteID
                                       select del;

                    if (objSelDelete.Count() > 0)
                    {
                        foreach (SBRM_ROBOT_ROUTE_STEP obj in objSelDelete)
                        {
                            Delete_RouteRelation( obj.ROUTEID, obj.STEPID.ToString());

                            _ctxBRM.SBRM_ROBOT_ROUTE_STEP.DeleteOnSubmit(obj);
                        }
                    }

                    try
                    {
                        _ctxBRM.SubmitChanges();//DB Data Change
                    }
                    catch (System.Data.Linq.ChangeConflictException err)
                    {
                        #region ChangeConflictException
                        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, err);

                        foreach (System.Data.Linq.ObjectChangeConflict occ in _ctxBRM.ChangeConflicts)
                        {
                            // 將變更的欄位寫入資料庫（合併更新）
                            occ.Resolve(System.Data.Linq.RefreshMode.KeepChanges);
                        }

                        try
                        {
                            _ctxBRM.SubmitChanges();
                        }
                        catch (Exception ex)
                        {
                            _sqlErr = ex.ToString();

                            foreach (System.Data.Linq.MemberChangeConflict _data in _ctxBRM.ChangeConflicts[0].MemberConflicts)
                            {
                                if (_data.DatabaseValue != _data.OriginalValue)
                                {
                                    _sqlErr = _sqlErr + string.Format("\r\n Change Conflicts : Property '{0}': Database value: {1}, Original value {2}, Current Value:{3}", _data.Member.Name, _data.DatabaseValue, _data.OriginalValue, _data.CurrentValue);
                                }
                            }

                            NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                        }
                        #endregion
                    }

                    GetGridViewData_RouteStep(CurRouteID); //更新畫面資料

                    Public.SendDatabaseReloadRequest("SBRM_ROBOT_ROUTE_STEP"); //Reload DB Trx           

                    ShowMessage(this, this.lblCaption.Text, "", "Delete Success！", MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnSaveRoute_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            CheckChangeSave();
            //try
            //{
            //    FormMainMDI.G_OPIAp.RefreshDBBRMCtx();

            //    //由SBRM_ROBOT_STAGE資料表中取出 Port,Stage,Tracking 選項
            //    CreateObject_Stage();

            //    //由SBRM_ROBOT_ROUTE_MST資料表中取出 ROBOT ROUTE 選項
            //    GetGridViewData_RobotRoute();
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            //    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            //}
        }

        private void btn_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender;

                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                if (dgvRouteStep.Rows.Count <= 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "Route Step is empty, please add a new Step!", MessageBoxIcon.Error);
                    return;
                }

                if (CurRouteStepData == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose Step!", MessageBoxIcon.Error);
                    return;
                }


                #region  result hanle編輯儲存會同時將route設定部分同步儲存，因此編輯result hadle前須確認route設定是否完整；其餘相關設定畫面則需判斷route是否已經儲存，方可編輯                
                if (_btn.Tag.ToString() == "Result Handle")
                {
                    #region Check Route Data 是否設定完整
                    if (CheckStepData() == false) return;
                    #endregion
                }
                else
                {
                    #region Check Route Data 是否已經儲存
                    if (_ctxBRM.GetChangeSet().Inserts.Count > 0 || _ctxBRM.GetChangeSet().Updates.Count > 0 || _ctxBRM.GetChangeSet().Deletes.Count > 0)
                    {
                        ShowMessage(this, this.lblCaption.Text, "", "Please Save Robot Route First！", MessageBoxIcon.Information);
                        return;
                    }
                    #endregion
                }
                #endregion

                int _curStepID = CurRouteStepData.STEPID;

                List<int> _lstStep = new List<int>();

                foreach (DataGridViewRow _row in dgvRouteStep.Rows)
                {
                    _lstStep.Add(int.Parse(_row.Cells[colStepID.Name].Value.ToString()));
                }

                switch (_btn.Tag.ToString())
                {
                    case "Stage Select":

                        #region Stage Select

                        FormRobot_Rule_Stage_Select _selectFrm = new FormRobot_Rule_Stage_Select(CurRouteID, _curStepID, CurRobot, _lstStep);

                        _selectFrm.ShowDialog();

                        if (_selectFrm != null) _selectFrm.Dispose();

                        break;

                        #endregion

                    case "Rule Filter":

                        #region Rule Filter

                        FormRobot_Rule_Filter _filterFrm = new FormRobot_Rule_Filter(CurRouteID, _curStepID, CurRobot, _lstStep);

                        _filterFrm.ShowDialog();

                        if (_filterFrm != null) _filterFrm.Dispose();

                        break;

                        #endregion

                    case "Result Handle":

                        #region Result Handle

                        FormRobot_Poc_Result_Handle _handleFrm = new FormRobot_Poc_Result_Handle(CurRouteID, _curStepID, CurRobot, _lstStep);

                        _handleFrm.ShowDialog();

                        if (_handleFrm != null) _handleFrm.Dispose();

                        break;

                        #endregion

                    case "Rule OrderBy":

                        #region Rule OrderBy

                        FormRobot_Rule_Orderby _orderByFrm = new FormRobot_Rule_Orderby(CurRouteID, _curStepID, CurRobot, _lstStep);

                        _orderByFrm.ShowDialog();

                        if (_orderByFrm != null) _orderByFrm.Dispose();


                        break;

                        #endregion

                    case "RouteStep ByPass":

                        #region RouteStep ByPass

                        FormRobot_Route_Step_ByPass _byPassFrm = new FormRobot_Route_Step_ByPass(CurRouteID, _curStepID, CurRobot, _lstStep);

                        _byPassFrm.ShowDialog();

                        if (_byPassFrm != null) _byPassFrm.Dispose();

                        break;

                        #endregion

                    case "RouteStep Jump":

                        #region RouteStep Jump

                        FormRobot_Route_Step_Jump _jumpFrm = new FormRobot_Route_Step_Jump(CurRouteID, _curStepID, CurRobot, _lstStep);

                        _jumpFrm.ShowDialog();

                        if (_jumpFrm != null) _jumpFrm.Dispose();

                        break;

                        #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Setting_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is Button)) return;

                if (CurRouteStepData == null) return;

                if (dgvRouteStep.Rows.Count == 0 || dgvRouteStep.SelectedRows.Count == 0) return;

                Button _btn = (sender as Button);

                string _pnlName = string.Empty;

                switch (_btn.Tag.ToString())
                {
                    case "LOW":
                    case "UP":
                    case "ANY":
                    case "ALL":

                        #region Robot Arm
                        _pnlName = pnlRobotArm.Name;
                        CurRouteStepData.ROBOTUSEARM = _btn.Tag.ToString();
                        SetButtonColor(_pnlName, new List<string> { _btn.Tag.ToString() });
                        break;
                        #endregion

                    case "GET":
                    case "PUT":
                    case "PUTREADY":
                    case "GETREADY":

                        #region Action

                        _pnlName = pnlAction.Name;
                        CurRouteStepData.ROBOTACTION = _btn.Tag.ToString();
                        SetButtonColor(_pnlName, new List<string> { _btn.Tag.ToString() });

                        break;
                        #endregion

                    case "ONLY":
                    case "SELECT":
                    case "TRACKING":
                    case "SEQUENCE":
                    case "ULDDISPATCH":

                        #region Robot Route
                        _pnlName = this.pnlRobotRule.Name;
                        CurRouteStepData.ROBOTRULE = _btn.Tag.ToString();
                        SetButtonColor(_pnlName, new List<string> { _btn.Tag.ToString() });

                        #region 清除Target資料，要求user重新選擇
                        CurRouteStepData.STAGEIDLIST = string.Empty;
                        CurRouteStepData.INPUTTRACKDATA = TrackDataDefault;
                        CurRouteStepData.OUTPUTTRACKDATA = TrackDataDefault;

                        SetTargetOption(new List<string>());
                        SetButtonColor("pnlTarget", new List<string>());

                        //btnTargetCassette.BackColor = Color.Transparent;
                        //btnTargetStage.BackColor = Color.Transparent;

                        //foreach (CheckBox chk in flpPort.Controls.OfType<CheckBox>())
                        //    chk.Checked = false;

                        //foreach (CheckBox _track in flpTracking.Controls.OfType<CheckBox>())
                        //    _track.Checked = false;

                        //foreach (CheckBox chk in flpStage.Controls.OfType<CheckBox>())
                        //    chk.Checked = false;

                        //flpPort.Enabled = false;
                        //flpStage.Enabled = false;
                        //flpTracking.Enabled = false;

                        #endregion

                        break;

                        #endregion

                    case "Cassette":
                    case "Stage":

                        #region Target

                        List<string> _target = new List<string>();

                        _pnlName = pnlTarget.Name;

                        if (CurRouteStepData.ROBOTRULE == "ULDDISPATCH")
                        {
                            #region  ROBOTACTION == ULDDISPATCH 時,是預設Port與Stage都可以選的
                            //所以都按下自已,而自已已選就將自已Disable;不選,再按一下將自已Enable
                            if (_btn.BackColor == Color.Lime)//自已是否已是按下狀況
                            {
                                _btn.BackColor = Color.Transparent;
                            }
                            else
                            {
                                _btn.BackColor = Color.Lime;　//將自已顯示按下狀況
                            }

                            foreach (Button _chooseBtn in pnlTarget.Controls.OfType<Button>().Where(r => r.BackColor.Equals(Color.Lime)))
                            {
                                _target.Add(_chooseBtn.Tag.ToString());
                            }
                            #endregion
                        }
                        else
                        {
                            _target.Add(_btn.Tag.ToString());
                        }

                        #region 點選RobotRule時，若Only且Stage時CrossStageFlag不為Y，並不給選取 (rex補
                        if (btnRuleOnly.BackColor==Color.Lime  && _btn.Tag.ToString().Equals("Stage"))
                        {
                            chkCrossStageFlag.Checked = false;
                            chkCrossStageFlag.Enabled = false;
                        }
                        else
                        {
                            chkCrossStageFlag.Enabled = true;
                        }
                        #endregion

                        SetTargetOption(_target);
                        SetButtonColor(_pnlName, _target);

                        break;
                        #endregion

                    default:
                        break;
                }

                SetGridViewColor();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        // 取得stage 對應可設定的tracking data
        private void Load_TrackingDataDesc()
        {
            try
            {
                int _len = 0;
                int _offset = 0;

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                List<SBRM_SUBJOBDATA> _lstTrack = (from s in _ctx.SBRM_SUBJOBDATA
                                                   where s.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType && s.ITEMNAME == "TrackingData"
                                                   select s).ToList();

                foreach (SBRM_SUBJOBDATA _data in _lstTrack)
                {
                    int.TryParse(_data.SUBITEMLENGTH.ToString(), out _len);
                    int.TryParse(_data.SUBITEMLOFFSET.ToString(), out _offset);

                    StageTrackingDesc _tracking = new StageTrackingDesc();
                    _tracking.ItemDesc = _data.SUBITEMDESC.ToString();
                    _tracking.Offset = _offset;
                    _tracking.ItemLen = _len;
                    TrackDataDesc.Add(_data.SUBITEMLOFFSET.ToString(), _tracking);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //取得本line對應的robot Name
        private void Load_RobotName()
        {
            try
            {
                var _varRB = (from robot in FormMainMDI.G_OPIAp.Dic_Robot.Values
                              where robot.ServerName == FormMainMDI.G_OPIAp.CurLine.ServerName
                              select robot);

                if (_varRB == null || _varRB.Count() == 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "Can't find Robot Data!", MessageBoxIcon.Error);
                    pnlNormalBtn.Enabled = false;
                    return;
                }

                RadioButton _rdoBtn = null;

                foreach (Robot _rb in _varRB)
                {
                    RadioButton _rdo = new RadioButton();
                    _rdo.Name = _rb.RobotName;
                    _rdo.Text = _rb.RobotName;
                    _rdo.Width = 150;
                    _rdo.Height = flpRadioButton.Height;
                    _rdo.TextAlign = ContentAlignment.MiddleLeft;
                    _rdo.Margin = new System.Windows.Forms.Padding(0);
                    _rdo.Font = new Font("Calibri", 12.75f);
                    _rdo.Click += new EventHandler(rdoRobot_Click);
                    flpRadioButton.Controls.Add(_rdo);

                    if (_rdoBtn == null) _rdoBtn = _rdo;
                }

                _rdoBtn.PerformClick();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private string GetStageList(FlowLayoutPanel flowPanel)
        {
            try
            {
                var x = (from chk in flowPanel.Controls.OfType<CheckBox>()
                         where chk.Checked == true
                         select chk.Name).ToList();

                return string.Join(",", x.ToArray());
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        //reload & create stage check box
        public void CreateObject_Stage()
        {
            try
            {
                flpPort.Controls.Clear();
                flpPort.Enabled = false;
                flpStage.Controls.Clear();
                flpStage.Enabled = false;
                StageTrackData.Clear();

                if (CurRobot == null) return;

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                List<SBRM_ROBOT_STAGE> _lstStage = (from s in _ctx.SBRM_ROBOT_STAGE
                                                    where s.LINEID == CurRobot.LineID && s.ROBOTNAME == CurRobot.RobotName
                                                    select s).ToList();

                foreach (SBRM_ROBOT_STAGE _stage in _lstStage)
                {
                    CheckBox _chk = new CheckBox();
                    _chk.Name = _stage.STAGEID;
                    _chk.AutoCheck = false;
                    _chk.Text = string.Format("{0}:{1}", _stage.STAGEID, _stage.STAGENAME);
                    _chk.Width = 190;
                    _chk.Font = new Font("Calibri", 12f);
                    _chk.Tag = "PORT".Equals(_stage.STAGETYPE) ? "Cassette" : "Stage";
                    _chk.Click += new EventHandler(chkStage_Click);
                    _chk.Leave += new EventHandler(chkStage_Leave);

                    if ("PORT".Equals(_stage.STAGETYPE))
                    {
                        flpPort.Controls.Add(_chk);
                    }
                    else
                    {
                        flpStage.Controls.Add(_chk);

                        #region 取得該stage對應的tracking data
                        if (_stage.TRACKDATASEQLIST != null && _stage.TRACKDATASEQLIST != string.Empty)
                        {
                            StageTracking _track = new StageTracking();

                            _track.TrackDataList = _stage.TRACKDATASEQLIST.Split(',').ToList();

                            StageTrackData.Add(_stage.STAGEID, _track);
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void CreateObject_TrackingData(string StageNo)
        {
            try
            {
                if (flpTracking.Tag != null && flpTracking.Tag.ToString() == StageNo) return;

                flpTracking.Tag = StageNo;

                flpTracking.Controls.Clear();

                #region Create Tracking CheckBox
                if (StageTrackData.ContainsKey(StageNo))
                {
                    foreach (string _track in StageTrackData[StageNo].TrackDataList)
                    {
                        CheckBox _chk = new CheckBox();
                        _chk.Name = "chkTrack" + _track;
                        _chk.Tag = StageNo + ";" + _track;
                        _chk.Width = 190;
                        _chk.Font = new Font("Calibri", 11.25f);
                        _chk.Click += new EventHandler(chkTracking_Click);
                        _chk.Checked = false;

                        if (TrackDataDesc.ContainsKey(_track))
                        {
                            _chk.Text = TrackDataDesc[_track].ItemDesc;

                            if (StageTrackData[StageNo].UseTrackData.Contains(_track)) _chk.Checked = true;
                            else _chk.Checked = false;
                        }
                        else
                        {
                            _chk.Text = _track;
                        }

                        flpTracking.Controls.Add(_chk);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public void GetGridViewData_RobotRoute()
        {
            try
            {
                if (CurRobot == null) return;

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                var _mode = (from m in _ctx.SBRM_ROBOT_ROUTE_MST
                             where m.ROBOTNAME == CurRobot.RobotName
                             select m);

                DataTable _dtMode = DBConnect.GetDataTable(_ctx, "SBRM_ROBOT_ROUTE_MST", _mode);
                dgvRobotRoute.AutoGenerateColumns = false;
                dgvRobotRoute.DataSource = _dtMode;

                if (_dtMode.Rows.Count > 0)
                {
                    pnlNormalBtn.Enabled = true;
                    flpButtons.Enabled = true;
                    btnNextStep.Enabled = true;

                    int _rowIndex = (CurrRoute_RowIndex > -1 && CurrRoute_RowIndex < _dtMode.Rows.Count) ? CurrRoute_RowIndex : 0;

                    DataGridViewCellEventArgs e = new DataGridViewCellEventArgs(0, _rowIndex);

                    dgvRobotRoute_CellClick(dgvRobotRoute, e); //觸發按下事件
                }
                else
                {
                    pnlNormalBtn.Enabled = false;
                    flpButtons.Enabled = false;
                    btnNextStep.Enabled = false;

                    ShowMessage(this, lblCaption.Text, "", "Can't find SBRM_ROBOT_ROUTE_MASTER DATA!", MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //取得 RouteID 對應的所有step資料放到dgvRouteStep中
        private void GetGridViewData_RouteStep(string RouteID)
        {
            try
            {
                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                var _selData = (from m in _ctx.SBRM_ROBOT_ROUTE_STEP
                              where m.SERVERNAME == CurRobot.ServerName && m.ROBOTNAME == CurRobot.RobotName && m.ROUTEID == RouteID
                              select m);

                //已新增未更新物件
                var _addData = _ctx.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_STEP>();

                List<SBRM_ROBOT_ROUTE_STEP> _objTables = _selData.ToList();
                _objTables.AddRange(_addData.ToList());

                dgvRouteStep.AutoGenerateColumns = false;
                dgvRouteStep.DataSource = _objTables;

                if (dgvRouteStep.Rows.Count > 0)
                {
                    DataGridViewCellEventArgs e = new DataGridViewCellEventArgs(0, 0);
                    dgvRouteStep_CellClick(dgvRobotRoute, e); //觸發按下事件
                }
                else
                {
                    CurRouteStepData = null;
                    ResetRouteStepScreen();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetGridViewColor()
        {
            try
            {
                if (dgvRouteStep.DataSource == null || dgvRouteStep.Rows.Count == 0) return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                ctxBRM.GetChangeSet();

                foreach (DataGridViewRow dr in dgvRouteStep.Rows)
                {
                    SBRM_ROBOT_ROUTE_STEP obj = dr.DataBoundItem as SBRM_ROBOT_ROUTE_STEP;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_STEP>().Any(
                        msg => msg.SERVERNAME == obj.SERVERNAME && msg.ROBOTNAME == obj.ROBOTNAME && msg.ROUTEID == obj.ROUTEID && msg.STEPID == obj.STEPID))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_ROBOT_ROUTE_STEP>().Any(
                        msg => msg.SERVERNAME == obj.SERVERNAME && msg.ROBOTNAME == obj.ROBOTNAME && msg.ROUTEID == obj.ROUTEID && msg.STEPID == obj.STEPID))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_ROBOT_ROUTE_STEP>().Any(
                         msg => msg.SERVERNAME == obj.SERVERNAME && msg.ROBOTNAME == obj.ROBOTNAME && msg.ROUTEID == obj.ROUTEID && msg.STEPID == obj.STEPID))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 195);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ShowTargetDetail()
        {
            try
            {
                dgvTarget.Rows.Clear();

                #region Show Target Detail

                foreach (CheckBox _chk in flpPort.Controls.OfType<CheckBox>().Where(r => r.Checked.Equals(true)))
                {
                    dgvTarget.Rows.Add(_chk.Name, _chk.Text);
                }

                foreach (CheckBox _chk in flpStage.Controls.OfType<CheckBox>().Where(r => r.Checked.Equals(true)))
                {

                    if (StageTrackData.ContainsKey(_chk.Name))
                    {
                        if (StageTrackData[_chk.Name].UseTrackData.Count > 0)
                        {
                            foreach (string _track in StageTrackData[_chk.Name].UseTrackData)
                            {
                                if (TrackDataDesc.ContainsKey(_track))
                                {
                                    dgvTarget.Rows.Add(_chk.Name, _chk.Text, TrackDataDesc[_track].ItemDesc, TrackDataDesc[_track].Offset, TrackDataDesc[_track].ItemLen);
                                }
                                else
                                {
                                    dgvTarget.Rows.Add(_chk.Name, _chk.Text, _track);
                                }
                            }
                        }
                        else
                        {
                            dgvTarget.Rows.Add(_chk.Name, _chk.Text);
                        }
                    }
                    else
                    {
                        dgvTarget.Rows.Add(_chk.Name, _chk.Text);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Show_TrackingData(CheckBox ChkStage, string TrackingData)
        {
            try
            {
                if (!StageTrackData.ContainsKey(ChkStage.Name)) return;

                if (flpTracking.Controls.Count <= 0) return;

                #region 清除設定
                StageTrackData[ChkStage.Name].UseTrackData.Clear();
                foreach (CheckBox _track in flpTracking.Controls.OfType<CheckBox>())
                {
                    _track.Checked = false;
                }
                #endregion

                int at = 0;
                int start = 0;
                string _idx = string.Empty;

                #region 顯示Tracking Data

                if (TrackingData.IndexOf('1') > -1)
                {
                    #region 取得設定的Tracking Data並顯示對應的radio button
                    while (start < TrackingData.Length)
                    {
                        at = TrackingData.IndexOf('1', start);

                        if (at > -1)
                        {
                            Control[] _ctrl = flpTracking.Controls.Find("chkTrack" + at.ToString(), true);

                            if (_ctrl.Length > 0)
                            {
                                ((CheckBox)_ctrl[0]).Checked = true;

                                _idx = _idx + (_idx == string.Empty ? string.Empty : ",") + at.ToString();

                                StageTrackData[ChkStage.Name].UseTrackData.Add(at.ToString());
                            }
                        }
                        else break;
                        start = at + 1;
                    }
                    #endregion
                }

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Update_TrackingData(SBRM_ROBOT_ROUTE_STEP routeStepData)
        {
            try
            {
                StringBuilder _trackingData = new StringBuilder("0".PadLeft(TrackingDataLength, '0'));

                int _idx = 0;
                int _len = 0;

                #region 產生tracking data設定值
                foreach (DataGridViewRow _row in dgvTarget.Rows)
                {
                    if (_row.Cells[colTrackingData.Name].Value == null || _row.Cells[colTrackingData.Name].Value.ToString() == string.Empty) continue;
                    if (_row.Cells[colOffset.Name].Value == null || _row.Cells[colOffset.Name].Value.ToString() == string.Empty) continue;

                    int.TryParse(_row.Cells[colOffset.Name].Value.ToString(), out _idx);
                    int.TryParse(_row.Cells[colLen.Name].Value.ToString(), out _len);

                    for (int i = 0; i < _len; i++)
                    {
                        _trackingData[_idx + i] = '1';
                    }
                }
                #endregion

                #region 將TrackingData更新至對應欄位
                if (routeStepData.ROBOTACTION.ToString() == "PUT")
                {
                    routeStepData.INPUTTRACKDATA = _trackingData.ToString();
                }
                else if (routeStepData.ROBOTACTION.ToString() == "GET")
                {
                    routeStepData.OUTPUTTRACKDATA = _trackingData.ToString();
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ResetRouteStepScreen()
        {
            try
            {
                foreach (Button btn in pnlRobotArm.Controls.OfType<Button>())
                    btn.BackColor = Color.Transparent;

                foreach (Button btn in pnlAction.Controls.OfType<Button>())
                    btn.BackColor = Color.Transparent;

                foreach (Button btn in pnlRobotRule.Controls.OfType<Button>())
                    btn.BackColor = Color.Transparent;

                foreach (Button btn in pnlTarget.Controls.OfType<Button>())
                    btn.BackColor = Color.Transparent;

                foreach (CheckBox chk in flpPort.Controls.OfType<CheckBox>())
                    chk.Checked = false;

                foreach (CheckBox chk in flpStage.Controls.OfType<CheckBox>())
                    chk.Checked = false;

                flpTracking.Controls.Clear();

                dgvTarget.Rows.Clear();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetRouteStepScreen()
        {
            try
            {
                List<string> _targetType = new List<string>();

                dgvTarget.Rows.Clear();

                if (CurRouteStepData == null) return;

                #region 設定object color
                foreach (Button btn in pnlRobotArm.Controls.OfType<Button>())//設定目前資料的 ROBOTUSEARM,為目前設定按鍵設為綠色
                {
                    btn.BackColor = CurRouteStepData.ROBOTUSEARM.ToString().Equals(btn.Tag.ToString()) ? Color.Lime : Color.Transparent;
                }

                foreach (Button btn in pnlAction.Controls.OfType<Button>()) //設定目前資料的 [ROBOTACTION],為目前設定按鍵設為綠色
                {
                    btn.BackColor = CurRouteStepData.ROBOTACTION.ToString().Equals(btn.Tag.ToString()) ? Color.Lime : Color.Transparent;
                }

                foreach (Button btn in pnlRobotRule.Controls.OfType<Button>()) //設定目前資料的 [ROBOTRULE],為目前設定按鍵設為綠色
                {
                    btn.BackColor = CurRouteStepData.ROBOTRULE.ToString().Equals(btn.Tag.ToString()) ? Color.Lime : Color.Transparent;
                }

                #endregion

                #region 取出Port與Stage Checkbox清單如果是目前資料,表示為 Checked

                List<string> lstStageNoList = new List<string>(); // StageNo 暫存
                lstStageNoList.AddRange(CurRouteStepData.STAGEIDLIST.ToString().Split(',')); //取得StageNo的清單

                foreach (CheckBox chk in flpPort.Controls.OfType<CheckBox>().Union(flpStage.Controls.OfType<CheckBox>()))
                {
                    if (lstStageNoList.Contains(chk.Name)) //是否為資料中的 StageID  -- chk.Name為stage id
                    {
                        chk.Checked = true;

                        //tag用來儲存 Type( Stage /Cassette )
                        if (_targetType.Contains(chk.Tag.ToString()) == false) _targetType.Add(chk.Tag.ToString());

                        CreateObject_TrackingData(chk.Name);

                        if (CurRouteStepData.ROBOTACTION.ToString() == "PUT")
                        {
                            Show_TrackingData(chk, CurRouteStepData.INPUTTRACKDATA);
                        }
                        else if (CurRouteStepData.ROBOTACTION.ToString() == "GET")
                        {
                            Show_TrackingData(chk, CurRouteStepData.OUTPUTTRACKDATA);
                        }
                    }
                    else
                    {
                        chk.Checked = false;
                    }
                }
                #endregion

                SetButtonColor(pnlTarget.Name, _targetType);
                SetTargetOption(_targetType);
                #region 讀取RouteStep資料時 ，若Only時CrossStageFlag不為Y ，也不給選取 (rex補
                if (btnRuleOnly.BackColor == Color.Lime && btnTargetStage.BackColor == Color.Lime)
                {
                    chkCrossStageFlag.Checked = false;
                    chkCrossStageFlag.Enabled = false;
                }
                else
                {
                    chkCrossStageFlag.Enabled = true;
                    if (CurRouteStepData.CROSSSTAGEFLAG == "Y")
                        chkCrossStageFlag.Checked = true;
                    else
                        chkCrossStageFlag.Checked = false;

                }
                #endregion

                //Show Target Detail DataGridView
                ShowTargetDetail();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetButtonColor(string TypeName, List<string> ButtonText)
        {
            try
            {
                foreach (Panel _panel in flpSetting.Controls.OfType<Panel>())
                {
                    if (_panel.Name == TypeName)
                    {
                        foreach (Button btn in _panel.Controls.OfType<Button>())
                        {
                            if (ButtonText.Contains(btn.Tag.ToString()))
                                btn.BackColor = Color.Lime;
                            else
                                btn.BackColor = Color.Transparent;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private bool CheckStepData()
        {
            try
            {
                int _stepID = 0;

                foreach (DataGridViewRow _row in dgvRouteStep.Rows)
                {
                    int.TryParse(_row.Cells[colStepID.Name].Value.ToString(), out _stepID);

                    #region check fields data

                    #region ROBOTUSEARM
                    if (_row.Cells[colRobotArm.Name].Value.ToString() == string.Empty)
                    {
                        ShowMessage(this, this.lblCaption.Text, "", "Use Arm required, please click option button on [Arm] block！", MessageBoxIcon.Warning);

                        DataGridViewCellEventArgs e = new DataGridViewCellEventArgs(0, _row.Index);

                        dgvRouteStep_CellClick(dgvRobotRoute, e);

                        return false;
                    }
                    #endregion

                    #region ROBOTACTION
                    if (_row.Cells[colRobotAction.Name].Value.ToString() == string.Empty)
                    {
                        ShowMessage(this, this.lblCaption.Text, "", "Action required, please click option button on [Action] block！", MessageBoxIcon.Warning);

                        DataGridViewCellEventArgs e = new DataGridViewCellEventArgs(0, _row.Index);

                        dgvRouteStep_CellClick(dgvRobotRoute, e);

                        return false;
                    }
                    #endregion

                    #region ROBOTRULE
                    if (_row.Cells[colRobotRule.Name].Value.ToString() == string.Empty)
                    {
                        ShowMessage(this, this.lblCaption.Text, "", "Robot Rule required, please click option button on [Action] block！", MessageBoxIcon.Warning);

                        DataGridViewCellEventArgs e = new DataGridViewCellEventArgs(0, _row.Index);

                        dgvRouteStep_CellClick(dgvRobotRoute, e);

                        return false;
                    }
                    #endregion

                    #region STAGEIDLIST
                    if (_row.Cells[colStageList.Name].Value.ToString() == string.Empty)
                    {
                        ShowMessage(this, this.lblCaption.Text, "", "Stage No required, please click option button on [Target] block, and check Stage No on [Cassette/Stage] block！", MessageBoxIcon.Warning);

                        DataGridViewCellEventArgs e = new DataGridViewCellEventArgs(0, _row.Index);

                        dgvRouteStep_CellClick(dgvRobotRoute, e);

                        return false;
                    }
                    #endregion

                    #endregion
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

        private bool CheckRelationTable()
        {
            try
            {
                if (dgvRouteStep.Rows.Count <= 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "No data to save！", MessageBoxIcon.Warning);
                    return false;
                }

                int _stepID = 0;

                foreach (DataGridViewRow _row in dgvRouteStep.Rows)
                {
                    int.TryParse(_row.Cells[colStepID.Name].Value.ToString(), out _stepID);

                    #region check relation table -- SBRM_ROBOT_PROC_RESULT_HANDLE 對應每組step id需有資料
                    UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                    //var vFilter = from rb in _ctxBRM.SBRM_ROBOT_RULE_FILTER
                    //              where rb.SERVERNAME == CurRobot.ServerName && rb.ROBOTNAME == CurRobot.RobotName && rb.ROUTEID == CurRouteID && rb.STEPID == _stepID
                    //              select rb;
                    //if (vFilter.Count() == 0)
                    //{
                    //    ShowMessage(this, this.lblCaption.Text, "", string.Format("SBRM_ROBOT_RULE_FILTER lack of information StepID[{0}]", _stepID.ToString()), MessageBoxIcon.Warning);

                    //    return false;
                    //}

                    var vHandle = from rb in _ctxBRM.SBRM_ROBOT_PROC_RESULT_HANDLE
                                  where rb.SERVERNAME == CurRobot.ServerName && rb.ROBOTNAME == CurRobot.RobotName && rb.ROUTEID == CurRouteID && rb.STEPID == _stepID
                                  select rb;
                    if (vHandle.Count() == 0)
                    {
                        ShowMessage(this, this.lblCaption.Text, "", string.Format("SBRM_ROBOT_PROC_RESULT_HANDLE lack of information StepID[{0}]", _stepID.ToString()), MessageBoxIcon.Warning);

                        return false;
                    }
                    #endregion
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

        private void Delete_RouteRelation(string RouteID, string StepID)
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var vByPass = from rb in _ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS
                              where rb.SERVERNAME == CurRobot.ServerName && rb.ROBOTNAME == CurRobot.RobotName && rb.ROUTEID == RouteID && rb.STEPID == int.Parse(StepID)
                              select rb;
                if (vByPass.Count() > 0)
                {
                    foreach (SBRM_ROBOT_RULE_ROUTESTEP_BYPASS obj in vByPass)
                    {
                        _ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS.DeleteOnSubmit(obj);
                    }
                }

                var vJump = from rb in _ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_JUMP
                            where rb.SERVERNAME == CurRobot.ServerName && rb.ROBOTNAME == CurRobot.RobotName && rb.ROUTEID == RouteID && rb.STEPID == int.Parse(StepID)
                            select rb;
                if (vJump.Count() > 0)
                {
                    foreach (SBRM_ROBOT_RULE_ROUTESTEP_JUMP obj in vJump)
                    {
                        _ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_JUMP.DeleteOnSubmit(obj);
                    }
                }

                var vFilter = from rb in _ctxBRM.SBRM_ROBOT_RULE_FILTER
                              where rb.SERVERNAME == CurRobot.ServerName && rb.ROBOTNAME == CurRobot.RobotName && rb.ROUTEID == RouteID && rb.STEPID == int.Parse(StepID)
                              select rb;
                if (vFilter.Count() > 0)
                {
                    foreach (SBRM_ROBOT_RULE_FILTER obj in vFilter)
                    {
                        _ctxBRM.SBRM_ROBOT_RULE_FILTER.DeleteOnSubmit(obj);
                    }
                }

                var vOrderby = from rb in _ctxBRM.SBRM_ROBOT_RULE_ORDERBY
                               where rb.SERVERNAME == CurRobot.ServerName && rb.ROBOTNAME == CurRobot.RobotName && rb.ROUTEID == RouteID && rb.STEPID == int.Parse(StepID)
                               select rb;
                if (vOrderby.Count() > 0)
                {
                    foreach (SBRM_ROBOT_RULE_ORDERBY obj in vOrderby)
                    {
                        _ctxBRM.SBRM_ROBOT_RULE_ORDERBY.DeleteOnSubmit(obj);
                    }
                }

                var vHandle = from rb in _ctxBRM.SBRM_ROBOT_PROC_RESULT_HANDLE
                              where rb.SERVERNAME == CurRobot.ServerName && rb.ROBOTNAME == CurRobot.RobotName && rb.ROUTEID == RouteID && rb.STEPID == int.Parse(StepID)
                              select rb;
                if (vHandle.Count() > 0)
                {
                    foreach (SBRM_ROBOT_PROC_RESULT_HANDLE obj in vHandle)
                    {
                        _ctxBRM.SBRM_ROBOT_PROC_RESULT_HANDLE.DeleteOnSubmit(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetTargetOption(List<string> TargetType)
        {
            try
            {
                bool _isModify = false;

                #region 設定stage / cassette 物件是否啟用
                if (TargetType.Count() == 0)
                {
                    flpPort.Enabled = false;
                    flpStage.Enabled = false;
                    flpTracking.Enabled = false;
                }
                else
                {                    
                    if (TargetType.Contains("Stage") == false)
                    {
                        flpStage.Enabled = false;
                        flpTracking.Enabled = false;
                    }
                    else
                    {
                        flpStage.Enabled = true;
                        flpTracking.Enabled = true;
                    }

                    if (TargetType.Contains("Cassette") == false) 
                        flpPort.Enabled = false;
                    else  
                        flpPort.Enabled = true;
                }
                #endregion

                #region 清除不啟用的物件內的check box 
                if (flpStage.Enabled == false)
                {
                    foreach (CheckBox _track in flpTracking.Controls.OfType<CheckBox>().Where(r => r.Checked))
                    {
                        _track.Checked = false;
                        _isModify = true;
                    }

                    foreach (CheckBox chk in flpStage.Controls.OfType<CheckBox>().Where(r => r.Checked))
                    {
                        chk.Checked = false;
                        _isModify = true;
                    }
                }                

                if (flpPort.Enabled == false)
                {
                    foreach (CheckBox chk in flpPort.Controls.OfType<CheckBox>().Where(r => r.Checked))
                    {
                        chk.Checked = false;
                        _isModify = true;
                    }
                }
                #endregion

                if (_isModify)
                {
                    string _cstData = CurRouteStepData.STAGEIDLIST = GetStageList(flpPort);
                    string _stageData = CurRouteStepData.STAGEIDLIST = GetStageList(flpStage);

                    CurRouteStepData.STAGEIDLIST = _cstData + (_cstData == string.Empty ? "" : _stageData == string.Empty ? "" : ",") + _stageData;

                    //Show Target Detail DataGridView
                    ShowTargetDetail();

                    //將修改的track更新至對應的欄位
                    Update_TrackingData(CurRouteStepData);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private class StageTrackingDesc
        {
            public int Offset { get; set; }
            public string ItemDesc {get;set;}
            public int ItemLen {get;set;}
        }

        private class StageTracking
        {
            public List<string> TrackDataList { get; set; }  //可設定的tracking data
            public List<string> UseTrackData { get; set; }   //已設定的tracking data
            public StageTracking()
            {
                TrackDataList = new List<string>();
                UseTrackData = new List<string>();
            }
        }

        private void chkCrossStageFlag_CheckedChanged(object sender, EventArgs e)
        {        
            #region 當CrossStageFlag點選時修改CrossStageFlag數值 (rex補
            if (chkCrossStageFlag.Checked == true)
                CurRouteStepData.CROSSSTAGEFLAG = "Y";
            else
                CurRouteStepData.CROSSSTAGEFLAG = "N";        
            #endregion

        }

        private void Save()
        {
            string _sqlErr = string.Empty;
            string _sqlDesc = string.Empty;
            DateTime _updateDateTime = DateTime.Now; ;
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

            try
            {
                if (dgvRouteStep.Rows.Count <= 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "No data to save！", MessageBoxIcon.Warning);
                    return;
                }

                if (CheckStepData() == false) return;
                if (CheckRelationTable() == false) return;

                if (_ctxBRM.GetChangeSet().Inserts.Count <= 0 && _ctxBRM.GetChangeSet().Updates.Count <= 0 && _ctxBRM.GetChangeSet().Deletes.Count <= 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "No Data Change！", MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    #region add
                    foreach (object objToInsert in _ctxBRM.GetChangeSet().Inserts)
                    {
                        if (objToInsert.GetType().Name != "SBRM_ROBOTROUTE") continue;

                        SBRM_ROBOT_ROUTE_STEP _updateData = (SBRM_ROBOT_ROUTE_STEP)objToInsert;

                        _updateData.LASTUPDATETIME = _updateDateTime;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [ {0} , {1} , {2} ] ", _updateData.ROBOTNAME, _updateData.ROUTEID, _updateData.STEPID);
                    }
                    #endregion

                    #region delete
                    foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                    {
                        if (objToDelete.GetType().Name != "SBRM_ROBOT_ROUTE_STEP") continue;

                        SBRM_ROBOT_ROUTE_STEP _updateData = (SBRM_ROBOT_ROUTE_STEP)objToDelete;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [ {0} , {1} , {2} ] ", _updateData.ROBOTNAME, _updateData.ROUTEID, _updateData.STEPID);
                    }
                    #endregion

                    #region modify
                    foreach (object objToUpdate in _ctxBRM.GetChangeSet().Updates)
                    {
                        if (objToUpdate.GetType().Name != "SBRM_ROBOT_ROUTE_STEP") continue;

                        SBRM_ROBOT_ROUTE_STEP _updateData = (SBRM_ROBOT_ROUTE_STEP)objToUpdate;

                        _updateData.LASTUPDATETIME = _updateDateTime;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [ {0} , {1} , {2} ] ", _updateData.ROBOTNAME, _updateData.ROUTEID, _updateData.STEPID);
                    }
                    #endregion

                    _ctxBRM.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                }
                catch (System.Data.Linq.ChangeConflictException err)
                {
                    #region ChangeConflictException
                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, err);

                    foreach (System.Data.Linq.ObjectChangeConflict occ in _ctxBRM.ChangeConflicts)
                    {
                        // 將變更的欄位寫入資料庫（合併更新）
                        occ.Resolve(System.Data.Linq.RefreshMode.KeepChanges);
                    }

                    try
                    {
                        _ctxBRM.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        _sqlErr = ex.ToString();

                        foreach (System.Data.Linq.MemberChangeConflict _data in _ctxBRM.ChangeConflicts[0].MemberConflicts)
                        {
                            if (_data.DatabaseValue != _data.OriginalValue)
                            {
                                _sqlErr = _sqlErr + string.Format("\r\n Change Conflicts : Property '{0}': Database value: {1}, Original value {2}, Current Value:{3}", _data.Member.Name, _data.DatabaseValue, _data.OriginalValue, _data.CurrentValue);
                            }
                        }

                        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                    }
                    #endregion
                }


                #region 紀錄opi history
                string _err = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_ROUTE_STEP", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                Public.SendDatabaseReloadRequest("SBRM_ROBOT_ROUTE_STEP");

                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Robot Route Save Success！", MessageBoxIcon.Information);
                }

                GetGridViewData_RouteStep(CurRouteID);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public override void CheckChangeSave()
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                if (_ctxBRM.GetChangeSet().Inserts.Count() > 0 ||
                     _ctxBRM.GetChangeSet().Deletes.Count() > 0 ||
                     _ctxBRM.GetChangeSet().Updates.Count() > 0)
                {
                    string _msg = string.Format("Please confirm whether you will save data before change layout ?");
                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg))
                    {
                        RefreshData();
                        return;
                    }
                    else
                    {
                        Save();
                    }
                }
                else
                {
                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public void RefreshData()
        {
            try
            {
                #region Refresh

                FormMainMDI.G_OPIAp.RefreshDBBRMCtx();

                //由SBRM_ROBOT_STAGE資料表中取出 Port,Stage,Tracking 選項
                CreateObject_Stage();

                //由SBRM_ROBOT_ROUTE_MST資料表中取出 ROBOT ROUTE 選項
                GetGridViewData_RobotRoute();

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
