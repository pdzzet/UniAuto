using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormParameterSetting : FormBase
    {
        Robot CurRobot = null;

        public FormParameterSetting()
        {
            InitializeComponent();
            lblCaption.Text = "Parameter Setting";
        }

        private void FormParameterSetting_Load(object sender, EventArgs e)
        {
            txtQueryMaxCount.Text = FormMainMDI.G_OPIAp.QueryMaxCount.ToString();
            txtBCSMessageDisplayTime.Text = FormMainMDI.G_OPIAp.BCSMessageDisplayTime.ToString();
            txtSocketResposeTime_Comm.Text = FormMainMDI.G_OPIAp.SocketResponseTime.ToString();
            txtSocketResposeTime_MES.Text = FormMainMDI.G_OPIAp.SocketResponseTime_MES.ToString();
            txtSocketResposeTime_MapDownload.Text = FormMainMDI.G_OPIAp.SocketResponseTime_MapDownload.ToString();
            txtSocketResposeTime_Query.Text = FormMainMDI.G_OPIAp.SocketResponseTime_Query.ToString();

            if (FormMainMDI.G_OPIAp.Dic_Robot.Count() > 0) CurRobot = FormMainMDI.G_OPIAp.Dic_Robot.FirstOrDefault().Value;

            //foreach (Panel _pnl in flpOPIParameter.Controls.OfType<Panel>())
            //{
            //    if (_pnl.Tag == null) continue ;

            //    if (FormMainMDI.G_OPIAp.Lst_BCSParameterItem.Contains(_pnl.Tag.ToString())) 
            //        _pnl.Visible = true ;
            //    else  _pnl.Visible = false ;
            //}

            ReloadRobot();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            try
            {
                string _xml;
                Button _btn = (Button)sender;
                string _data = string.Empty;
                string _sqlDesc = string.Empty;
                string _sqlErr = string.Empty;
                string _err = string.Empty;
                               
                switch (_btn.Text.ToString())
                {
                    case "Create File":

                        #region Create File
                        string _cstSeqNo = txtCSTSeqNo.Text.ToString().Trim();
                        string _jobSeqNo = txtJobSeqNo.Text.ToString().Trim();

                        if (_cstSeqNo == string.Empty)
                        {
                            ShowMessage(this, lblCaption.Text, "", "Please inpute CST Seq No", MessageBoxIcon.Warning);
                            txtCSTSeqNo.Focus();
                            return;
                        }

                        if (_jobSeqNo == string.Empty)
                        {
                            ShowMessage(this, lblCaption.Text, "", "Please inpute Job Seq No", MessageBoxIcon.Warning);
                            txtJobSeqNo.Focus();
                            return;
                        }
                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, string.Format("Create File - Cassette Seq No [{0}], Job Seq No [{1}]", _cstSeqNo, _jobSeqNo))) return;
                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                        CreateInspFileRequest _createInspFileRequest = new CreateInspFileRequest();

                        _createInspFileRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _createInspFileRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _createInspFileRequest.BODY.CASSETTESEQNO = _cstSeqNo;
                        _createInspFileRequest.BODY.JOBSEQNO = _jobSeqNo;

                        _xml = _createInspFileRequest.WriteToXml();

                        MessageResponse _resp = this.SendRequestResponse(_createInspFileRequest.HEADER.TRANSACTIONID, _createInspFileRequest.HEADER.MESSAGENAME, _xml, 0);

                        if (_resp == null) return;

                        #region CreateInspFileReply

                        ShowMessage(this, lblCaption.Text, "", string.Format("Create File - Cassette Seq No [{0}],Job Seq No[{1}] Send to BC Success", _cstSeqNo, _jobSeqNo), MessageBoxIcon.Information);

                        txtCSTSeqNo.Text = string.Empty;
                        txtJobSeqNo.Text = string.Empty;

                         #endregion

                        break;
                        #endregion

                    case "Reload":

                        #region BCS Reload Table
                        string _tableName=string.Empty ;
                        foreach (CheckBox _rdo in flpReload.Controls.OfType<CheckBox>())
                        {
                            if (_rdo.Checked)
                            {
                                DatabaseReloadRequest _trx = new DatabaseReloadRequest();
                                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                                _trx.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                                _trx.BODY.TABLENAME = _rdo.Tag.ToString();

                                MessageResponse _reloadResp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                                if (_reloadResp == null) continue;
                                _tableName +="["+ _rdo.Tag.ToString()+"]\r\n";
                                _rdo.Checked = false;
                            }
                        }
                        ShowMessage(this, lblCaption.Text, "", string.Format("BCS Reload {0} Send to BC Success !", _tableName), MessageBoxIcon.Information);

                        break;
                        #endregion

                    case "Refresh":

                        #region Refresh

                        RefreshOPIParameter();

                        //GetParameterBCS();

                        break;

                        #endregion

                    case "Setting":

                        #region OPI Parameter Update

                        int _num = 0;
                        string _keyWord = string.Empty;
                        string _itemValue = string.Empty;
                        UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;

                        switch (_btn.Name)
                        {
                            case "btnSetMaxCount":

                                #region History Query Max Count --分頁用

                                _keyWord = "QueryMaxCount";
                                _itemValue = txtQueryMaxCount.Text.Trim();

                                if (int.TryParse(_itemValue, out _num) == false)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Query Max Count must be a number！", MessageBoxIcon.Warning);
                                    return;
                                }

                                if (_num <= 0 || _num > 5000)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Query Max Count must be a number between 1 and 5000！", MessageBoxIcon.Warning);
                                    txtQueryMaxCount.Focus();
                                    txtQueryMaxCount.SelectAll();
                                    return;
                                }
                                break;

                                #endregion

                            case "btnBCSMessageDisplayTime":

                                #region BCS Message Display Time 
                                _keyWord = "BCSMessageDisplayTime";
                                _itemValue = txtBCSMessageDisplayTime.Text.Trim();

                                if (int.TryParse(_itemValue, out _num) == false)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "BCS Message Display Time must be a number！", MessageBoxIcon.Warning);
                                    return;
                                }

                                if (_num < 0 || _num > 99)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "BCS Message Display Time must be a number between 0 and 99！", MessageBoxIcon.Warning);
                                    txtBCSMessageDisplayTime.Focus();
                                    txtBCSMessageDisplayTime.SelectAll();
                                    return;
                                } 
                                break;
                                #endregion
                                
                            case "btnSocketResposeTime_Comm":

                                #region Common Socket Respose Time 
                                _keyWord = "SocketResponseTime_Comm";
                                _itemValue = txtSocketResposeTime_Comm.Text.Trim();

                                if (int.TryParse(_itemValue, out _num) == false)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Common Socket Respose Time must be a number！", MessageBoxIcon.Warning);
                                    return;
                                }

                                if (_num < 1000 || _num > 99999)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Common Socket Respose Time must be a number between 1000 and 99999！", MessageBoxIcon.Warning);
                                    txtSocketResposeTime_Comm.Focus();
                                    txtSocketResposeTime_Comm.SelectAll();
                                    return;
                                }                                
                                break;
                                #endregion                                

                            case "btnSocketResposeTime_MES":

                                #region MES Socket Respose Time 
                                _keyWord = "SocketResponseTime_MES";
                                _itemValue = txtSocketResposeTime_MES.Text.Trim();

                                if (int.TryParse(_itemValue, out _num) == false)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "MES Socket Respose Time must be a number！", MessageBoxIcon.Warning);
                                    return;
                                }

                                if (_num < 1000 || _num > 99999)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "MES Socket Respose Time must be a number between 1000 and 99999！", MessageBoxIcon.Warning);
                                    txtSocketResposeTime_MES.Focus();
                                    txtSocketResposeTime_MES.SelectAll();
                                    return;
                                }                                
                                break;                                

                                #endregion

                            case "btnSocketResposeTime_MapDownload":

                                #region Map Download Socket Respose Time

                                _keyWord = "SocketResponseTime_MapDownload";
                                _itemValue = txtSocketResposeTime_MapDownload.Text.Trim();

                                if (int.TryParse(_itemValue, out _num) == false)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Map Download Socket Respose Time must be a number！", MessageBoxIcon.Warning);
                                    return;
                                }

                                if (_num < 1000 || _num > 99999)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Map Download Socket Respose Time must be a number between 1000 and 99999！", MessageBoxIcon.Warning);
                                    txtSocketResposeTime_MapDownload.Focus();
                                    txtSocketResposeTime_MapDownload.SelectAll();
                                    return;
                                }
                                break;
                                #endregion

                            case "btnSocketResposeTime_Query":

                                #region Equipment Query Socket Respose Time

                                _keyWord = "SocketResponseTime_Query";
                                _itemValue = txtSocketResposeTime_Query.Text.Trim();

                                if (int.TryParse(_itemValue, out _num) == false)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Equipment Query Socket Respose Time must be a number！", MessageBoxIcon.Warning);
                                    return;
                                }

                                if (_num < 1000 || _num > 99999)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Equipment Query Socket Respose Time must be a number between 1000 and 99999！", MessageBoxIcon.Warning);
                                    txtSocketResposeTime_Query.Focus();
                                    txtSocketResposeTime_Query.SelectAll();
                                    return;
                                }
                                break;
                                #endregion

                            default :
                                break;
                        }

                        #region update SBRM_OPI_PARAMETER
                        SBRM_OPI_PARAMETER _parm = (from p in _ctx.SBRM_OPI_PARAMETER
                                                    where p.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType && p.KEYWORD == _keyWord 
                                                    select p).SingleOrDefault();

                        SBRM_OPI_PARAMETER objParam = null;

                        if (_parm == null)
                        {
                            objParam = new SBRM_OPI_PARAMETER();
                            objParam.LINETYPE = FormMainMDI.G_OPIAp.CurLine.LineType;
                            objParam.KEYWORD = _keyWord;
                            objParam.SUBKEY = _keyWord;
                            objParam.ITEMVALUE = _itemValue;
                            _ctx.SBRM_OPI_PARAMETER.InsertOnSubmit(objParam);
                        }
                        else
                        {
                            objParam = _parm;
                            objParam.ITEMVALUE = _itemValue;
                            FormMainMDI.G_OPIAp.QueryMaxCount = int.Parse(objParam.ITEMVALUE);
                        }

                        try
                        {
                            _ctx.SubmitChanges();

                            ShowMessage(this, lblCaption.Text, "", "Save Success！", MessageBoxIcon.Information);

                            RefreshOPIParameter();
                        }
                        catch (System.Data.Linq.ChangeConflictException ex)
                        {
                            _sqlErr = ex.ToString();
                            NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                        }
                        
                        #region 紀錄opi history
                        _sqlDesc = string.Format("Keyword [{0}], itemvalue [{1}]", _keyWord,_itemValue);
                        _err = UniTools.InsertOPIHistory_DB("SBRM_OPI_PARAMETER", _sqlDesc, _sqlErr);

                        if (_err != string.Empty)
                        {
                            ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                        }
                        #endregion

                        #endregion

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

        //private void BCS_Parameter_Button_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //         Button _btn = (Button)sender;

        //        int _num = 0;
        //        string _keyWord = string.Empty;
        //        string _itemValue = string.Empty;

        //        switch (_btn.Name)
        //        {

        //            case "btnELA_COMPENSATION_TIME":

        //                #region BCS Parameter.xml -> ELA_COMPENSATION_TIME --ELA COMPENSATION TIME For Put Glass to Clean

        //                _keyWord = "ELA_COMPENSATION_TIME";
        //                _itemValue = txtELA_COMPENSATION_TIME.Text.Trim();

        //                if (int.TryParse(_itemValue, out _num) == false)
        //                {
        //                    ShowMessage(this, lblCaption.Text, "", "ELA Compenstion Time must be a number！", MessageBoxIcon.Warning);
        //                    return;
        //                }

        //                if (_num < 0 || _num > 65535)
        //                {
        //                    ShowMessage(this, lblCaption.Text, "", "ELA Compenstion Time must be a number between 0 and 65535！", MessageBoxIcon.Warning);
        //                    txtELA_COMPENSATION_TIME.Focus();
        //                    txtELA_COMPENSATION_TIME.SelectAll();
        //                    return;
        //                }
        //                break;
        //                #endregion

        //            default :
        //                break;
        //        }


        //        #region Send BCSParameterDataChangeRequest

        //        BCSParameterDataChangeRequest _trx = new BCSParameterDataChangeRequest();

        //        _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
        //        _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
        //        _trx.BODY.PARAMETER_NAME = _keyWord;
        //        _trx.BODY.PARAMETER_VALUE = _itemValue;

        //        MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

        //        if (_resp == null) return;


        //        ShowMessage(this, lblCaption.Text, "", "BCS Parameter Data Send to BC Success !", MessageBoxIcon.Information);

        //        #endregion

        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
        //    }
        //}

        private void btnRobotRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                lsvRobotProcResult.Items.Clear();
                ReloadRobot();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnCopyRobot_Click(object sender, EventArgs e)
        {
            try
            {
                lsvRobotProcResult.Items.Clear();

                if (cboSourceRobotName.SelectedValue == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Source Robot Name", MessageBoxIcon.Error);
                    return;
                }

                if (cboTargetRobotName.SelectedValue == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Target Robot Name", MessageBoxIcon.Error);
                    return;
                }


                string _sourceObjKey = cboSourceRobotName.SelectedValue.ToString();
                string _targetObjKey = cboTargetRobotName.SelectedValue.ToString();
                DateTime _updateTime = DateTime.Now;

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                SBRM_ROBOT _rbSource = _ctx.SBRM_ROBOT.Where(r => r.OBJECTKEY.Equals(_sourceObjKey)).FirstOrDefault();
                SBRM_ROBOT _rbTarget = _ctx.SBRM_ROBOT.Where(r => r.OBJECTKEY.Equals(_targetObjKey)).FirstOrDefault();
                if (_rbSource == null || _rbTarget == null) return;

                #region Check Source & Target Robot
                if (_rbSource.ROBOTNAME == _rbTarget.ROBOTNAME)
                {
                    ShowMessage(this, lblCaption.Text, "", "Source Robot Name and Target Robot Name must be different!", MessageBoxIcon.Error);
                    return;
                }

                if (_rbSource.ROBOTARMQTY != _rbTarget.ROBOTARMQTY)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("Source Robot Arm Qty [{0}] and Target Robot Arm Qty [{1}] is different !", _rbSource.ROBOTARMQTY.ToString(), _rbTarget.ROBOTARMQTY.ToString()), MessageBoxIcon.Error);
                    return;
                }

                if (_rbSource.ARMJOBQTY != _rbTarget.ARMJOBQTY)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("Source Robot Arm Job Qty [{0}] and Target Robot Arm Job Qty [{1}] is different !", _rbSource.ARMJOBQTY.ToString(), _rbTarget.ARMJOBQTY.ToString()), MessageBoxIcon.Error);
                    return;
                }
                #endregion

                string _msg = string.Format("Please confirm whether you will Delete & Copy Robot Table for ServerName [{0}],Robot Name [{1}] ?", _rbTarget.SERVERNAME, _rbTarget.ROBOTNAME);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                this.Cursor = Cursors.WaitCursor;

                #region 刪除Target Robot 已存在資料
                foreach (CheckBox _chk in flpRobotTables.Controls.OfType<CheckBox>().Where(r => r.Checked.Equals(true)))
                {

                    switch (_chk.Tag.ToString())
                    {
                        case "SBRM_ROBOT_STAGE":

                            #region SBRM_ROBOT_STAGE

                            var _varDelStage = _ctx.SBRM_ROBOT_STAGE.Where(r => r.ROBOTNAME.Equals(_rbTarget.ROBOTNAME) && r.SERVERNAME.Equals(_rbTarget.SERVERNAME)).ToList();

                            _ctx.SBRM_ROBOT_STAGE.DeleteAllOnSubmit(_varDelStage);

                            break;
                            #endregion

                        case "SBRM_ROBOT_ROUTE_MST":

                            #region SBRM_ROBOT_ROUTE_MST

                            var _varDelRouteMaster = _ctx.SBRM_ROBOT_ROUTE_MST.Where(r => r.ROBOTNAME.Equals(_rbTarget.ROBOTNAME) && r.SERVERNAME.Equals(_rbTarget.SERVERNAME)).ToList();

                            _ctx.SBRM_ROBOT_ROUTE_MST.DeleteAllOnSubmit(_varDelRouteMaster);
                        
                            break;

                            #endregion

                        case "SBRM_ROBOT_ROUTE_CONDITION":

                            #region SBRM_ROBOT_ROUTE_CONDITION

                            var _varDelRouteCondition = _ctx.SBRM_ROBOT_ROUTE_CONDITION.Where(r => r.ROBOTNAME.Equals(_rbTarget.ROBOTNAME) && r.SERVERNAME.Equals(_rbTarget.SERVERNAME)).ToList();

                            _ctx.SBRM_ROBOT_ROUTE_CONDITION.DeleteAllOnSubmit(_varDelRouteCondition);

                            break;

                            #endregion

                        case "SBRM_ROBOT_ROUTE_STEP":

                            #region SBRM_ROBOT_ROUTE_STEP

                            var _varDelRouteStep = _ctx.SBRM_ROBOT_ROUTE_STEP.Where(r => r.ROBOTNAME.Equals(_rbTarget.ROBOTNAME) && r.SERVERNAME.Equals(_rbTarget.SERVERNAME)).ToList();

                            _ctx.SBRM_ROBOT_ROUTE_STEP.DeleteAllOnSubmit(_varDelRouteStep);

                            break;

                            #endregion

                        case "SBRM_ROBOT_RULE_JOB_SELECT":

                            #region SBRM_ROBOT_RULE_JOB_SELECT

                            var _varDelRuleJobSelect = _ctx.SBRM_ROBOT_RULE_JOB_SELECT.Where(r => r.ROBOTNAME.Equals(_rbTarget.ROBOTNAME) && r.SERVERNAME.Equals(_rbTarget.SERVERNAME)).ToList();

                            _ctx.SBRM_ROBOT_RULE_JOB_SELECT.DeleteAllOnSubmit(_varDelRuleJobSelect);

                            break;

                            #endregion

                        case "SBRM_ROBOT_RULE_ROUTESTEP_BYPASS":

                            #region SBRM_ROBOT_RULE_ROUTESTEP_BYPASS

                            var _varDelRouteStepByPass = _ctx.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS.Where(r => r.ROBOTNAME.Equals(_rbTarget.ROBOTNAME) && r.SERVERNAME.Equals(_rbTarget.SERVERNAME)).ToList();

                            _ctx.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS.DeleteAllOnSubmit(_varDelRouteStepByPass);

                            break;

                            #endregion

                        case "SBRM_ROBOT_RULE_FILTER":

                            #region SBRM_ROBOT_RULE_FILTER

                            var _varDelRuleFilter = _ctx.SBRM_ROBOT_RULE_FILTER.Where(r => r.ROBOTNAME.Equals(_rbTarget.ROBOTNAME) && r.SERVERNAME.Equals(_rbTarget.SERVERNAME)).ToList();

                            _ctx.SBRM_ROBOT_RULE_FILTER.DeleteAllOnSubmit(_varDelRuleFilter);

                            break;

                            #endregion

                        case "SBRM_ROBOT_PROC_RESULT_HANDLE":

                            #region SBRM_ROBOT_PROC_RESULT_HANDLE

                            var _varDelResultHandle = _ctx.SBRM_ROBOT_PROC_RESULT_HANDLE.Where(r => r.ROBOTNAME.Equals(_rbTarget.ROBOTNAME) && r.SERVERNAME.Equals(_rbTarget.SERVERNAME)).ToList();

                            _ctx.SBRM_ROBOT_PROC_RESULT_HANDLE.DeleteAllOnSubmit(_varDelResultHandle);

                            break;

                            #endregion

                        case "SBRM_ROBOT_RULE_ROUTESTEP_JUMP":

                            #region SBRM_ROBOT_RULE_ROUTESTEP_JUMP

                            var _varDelRouteStepJump = _ctx.SBRM_ROBOT_RULE_ROUTESTEP_JUMP.Where(r => r.ROBOTNAME.Equals(_rbTarget.ROBOTNAME) && r.SERVERNAME.Equals(_rbTarget.SERVERNAME)).ToList();

                            _ctx.SBRM_ROBOT_RULE_ROUTESTEP_JUMP.DeleteAllOnSubmit(_varDelRouteStepJump);

                            break;

                            #endregion

                        case "SBRM_ROBOT_RULE_ORDERBY":

                            #region SBRM_ROBOT_RULE_ORDERBY

                            var _varDelRuleOrderBy = _ctx.SBRM_ROBOT_RULE_ORDERBY.Where(r => r.ROBOTNAME.Equals(_rbTarget.ROBOTNAME) && r.SERVERNAME.Equals(_rbTarget.SERVERNAME)).ToList();

                            _ctx.SBRM_ROBOT_RULE_ORDERBY.DeleteAllOnSubmit(_varDelRuleOrderBy);

                            break;

                            #endregion

                        default:
                            break;
                    }

                    if (SubmitChanges(_ctx) == true)
                    {
                        lsvRobotProcResult.Items.Add(new ListViewItem(new string[] { "Delete  ", _rbSource.ROBOTNAME, _chk.Tag.ToString(), "  OK" }));
                        lsvRobotProcResult.Items[lsvRobotProcResult.Items.Count - 1].ForeColor = Color.Black;
                    }
                    else
                    {
                        lsvRobotProcResult.Items.Add( new ListViewItem(new string[] { "Delete  ",_rbSource.ROBOTNAME, _chk.Tag.ToString(), "  NG" }));
                        lsvRobotProcResult.Items[lsvRobotProcResult.Items.Count - 1].ForeColor = Color.Red;
                    }
                }
                #endregion

                #region Copy & Past
                foreach (CheckBox _chk in flpRobotTables.Controls.OfType<CheckBox>().Where(r => r.Checked.Equals(true)))
                {

                    switch (_chk.Tag.ToString())
                    {
                        case "SBRM_ROBOT_STAGE":

                            #region SBRM_ROBOT_STAGE

                            var _varStage = _ctx.SBRM_ROBOT_STAGE.Where(r => r.ROBOTNAME.Equals(_rbSource.ROBOTNAME) && r.SERVERNAME.Equals(_rbSource.SERVERNAME)).ToList();

                            foreach (SBRM_ROBOT_STAGE _data in _varStage)
                            {
                                SBRM_ROBOT_STAGE _objAdd = new SBRM_ROBOT_STAGE();

                                _objAdd.SERVERNAME = _rbTarget.SERVERNAME;
                                _objAdd.ROBOTNAME = _rbTarget.ROBOTNAME;
                                _objAdd.LINEID = _rbTarget.LINEID;

                                _objAdd.STAGEID = _data.STAGEID;
                                _objAdd.STAGENAME = _data.STAGENAME;
                                _objAdd.NODENO = _data.NODENO;
                                _objAdd.STAGEIDBYNODE = _data.STAGEIDBYNODE;
                                _objAdd.STAGETYPE = _data.STAGETYPE;
                                _objAdd.PRIORITY = _data.PRIORITY;
                                _objAdd.STAGEREPORTTRXNAME = _data.STAGEREPORTTRXNAME;
                                _objAdd.STAGEJOBDATATRXNAME = _data.STAGEJOBDATATRXNAME;
                                _objAdd.ISMULTISLOT = _data.ISMULTISLOT;
                                _objAdd.SLOTMAXCOUNT = _data.SLOTMAXCOUNT;
                                _objAdd.RECIPECHENCKFLAG = _data.RECIPECHENCKFLAG;
                                _objAdd.DUMMYCHECKFLAG = _data.DUMMYCHECKFLAG;
                                _objAdd.GETREADYFLAG = _data.GETREADYFLAG;
                                _objAdd.PUTREADYFLAG = _data.PUTREADYFLAG;
                                _objAdd.PREFETCHFLAG = _data.PREFETCHFLAG;
                                _objAdd.WAITFRONTFLAG = _data.WAITFRONTFLAG;
                                _objAdd.UPSTREAMPATHTRXNAME = _data.UPSTREAMPATHTRXNAME;
                                _objAdd.UPSTREAMJOBDATAPATHTRXNAME = _data.UPSTREAMJOBDATAPATHTRXNAME;
                                _objAdd.DOWNSTREAMPATHTRXNAME = _data.DOWNSTREAMPATHTRXNAME;
                                _objAdd.DOWNSTREAMJOBDATAPATHTRXNAME = _data.DOWNSTREAMJOBDATAPATHTRXNAME;
                                _objAdd.TRACKDATASEQLIST = _data.TRACKDATASEQLIST;
                                _objAdd.CASSETTETYPE = _data.CASSETTETYPE;
                                _objAdd.REMARKS = _data.REMARKS;
                                _objAdd.ISENABLED = _data.ISENABLED;
                                _objAdd.SLOTFETCHSEQ = _data.SLOTFETCHSEQ;
                                _objAdd.SLOTSTORESEQ = _data.SLOTSTORESEQ;
                                _objAdd.EXCHANGETYPE = _data.EXCHANGETYPE;
                                _objAdd.EQROBOTIFTYPE = _data.EQROBOTIFTYPE;
                                _objAdd.RTCREWORKFLAG = _data.RTCREWORKFLAG;

                                _ctx.SBRM_ROBOT_STAGE.InsertOnSubmit(_objAdd);
                            }

                            break;
                            #endregion

                        case "SBRM_ROBOT_ROUTE_MST":

                            #region SBRM_ROBOT_ROUTE_MST

                            var _varRouteMaster = _ctx.SBRM_ROBOT_ROUTE_MST.Where(r => r.ROBOTNAME.Equals(_rbSource.ROBOTNAME) && r.SERVERNAME.Equals(_rbSource.SERVERNAME)).ToList();

                            foreach (SBRM_ROBOT_ROUTE_MST _data in _varRouteMaster)
                            {
                                SBRM_ROBOT_ROUTE_MST _objAdd = new SBRM_ROBOT_ROUTE_MST();

                                _objAdd.SERVERNAME = _rbTarget.SERVERNAME;
                                _objAdd.ROBOTNAME = _rbTarget.ROBOTNAME;
                                _objAdd.LINETYPE = _rbTarget.LINETYPE;

                                _objAdd.ROUTEID = _data.ROUTEID;
                                _objAdd.ROUTENAME = _data.ROUTENAME;
                                _objAdd.DESCRIPTION = _data.DESCRIPTION;
                                _objAdd.ISENABLED = _data.ISENABLED;
                                _objAdd.REMARKS = _data.REMARKS;
                                _objAdd.RTCMODEFLAG = _data.RTCMODEFLAG;
                                _objAdd.ROUTEPRIORITY = _data.ROUTEPRIORITY;
                                _objAdd.LASTUPDATETIME = _updateTime;

                                _ctx.SBRM_ROBOT_ROUTE_MST.InsertOnSubmit(_objAdd);
                            }

                            break;

                            #endregion

                        case "SBRM_ROBOT_ROUTE_CONDITION":

                            #region SBRM_ROBOT_ROUTE_CONDITION

                            var _varRouteCondition = _ctx.SBRM_ROBOT_ROUTE_CONDITION.Where(r => r.ROBOTNAME.Equals(_rbSource.ROBOTNAME) && r.SERVERNAME.Equals(_rbSource.SERVERNAME)).ToList();

                            foreach (SBRM_ROBOT_ROUTE_CONDITION _data in _varRouteCondition)
                            {
                                SBRM_ROBOT_ROUTE_CONDITION _objAdd = new SBRM_ROBOT_ROUTE_CONDITION();

                                _objAdd.SERVERNAME = _rbTarget.SERVERNAME;
                                _objAdd.ROBOTNAME = _rbTarget.ROBOTNAME;
                                _objAdd.LINETYPE = _rbTarget.LINETYPE;

                                _objAdd.ROUTEID = _data.ROUTEID;
                                _objAdd.CONDITIONID = _data.CONDITIONID;
                                _objAdd.DESCRIPTION = _data.DESCRIPTION;
                                _objAdd.OBJECTNAME = _data.OBJECTNAME;
                                _objAdd.METHODNAME = _data.METHODNAME;
                                _objAdd.ISENABLED = _data.ISENABLED;
                                _objAdd.REMARKS = _data.REMARKS;
                                _objAdd.CONDITIONSEQ = _data.CONDITIONSEQ;
                                _objAdd.ROUTEPRIORITY = _data.ROUTEPRIORITY;
                                _objAdd.LASTUPDATETIME = _updateTime;

                                _ctx.SBRM_ROBOT_ROUTE_CONDITION.InsertOnSubmit(_objAdd);
                            }

                            break;

                            #endregion

                        case "SBRM_ROBOT_ROUTE_STEP":

                            #region SBRM_ROBOT_ROUTE_STEP

                            var _varRouteStep = _ctx.SBRM_ROBOT_ROUTE_STEP.Where(r => r.ROBOTNAME.Equals(_rbSource.ROBOTNAME) && r.SERVERNAME.Equals(_rbSource.SERVERNAME)).ToList();

                            foreach (SBRM_ROBOT_ROUTE_STEP _data in _varRouteStep)
                            {
                                SBRM_ROBOT_ROUTE_STEP _objAdd = new SBRM_ROBOT_ROUTE_STEP();

                                _objAdd.SERVERNAME = _rbTarget.SERVERNAME;
                                _objAdd.ROBOTNAME = _rbTarget.ROBOTNAME;
                                _objAdd.LINETYPE = _rbTarget.LINETYPE;

                                _objAdd.ROUTEID = _data.ROUTEID;
                                _objAdd.STEPID = _data.STEPID;
                                _objAdd.DESCRIPTION = _data.DESCRIPTION;
                                _objAdd.ROBOTACTION = _data.ROBOTACTION;
                                _objAdd.ROBOTUSEARM = _data.ROBOTUSEARM;
                                _objAdd.ROBOTRULE = _data.ROBOTRULE;
                                _objAdd.STAGEIDLIST = _data.STAGEIDLIST;
                                _objAdd.INPUTTRACKDATA = _data.INPUTTRACKDATA;
                                _objAdd.OUTPUTTRACKDATA = _data.OUTPUTTRACKDATA;
                                _objAdd.REMARKS = _data.REMARKS;
                                _objAdd.NEXTSTEPID = _data.NEXTSTEPID;
                                _objAdd.CROSSSTAGEFLAG = _data.CROSSSTAGEFLAG;
                                _objAdd.LASTUPDATETIME = _updateTime;
                               
                                _ctx.SBRM_ROBOT_ROUTE_STEP.InsertOnSubmit(_objAdd);
                            }

                            break;

                            #endregion

                        case "SBRM_ROBOT_RULE_JOB_SELECT":

                            #region SBRM_ROBOT_RULE_JOB_SELECT

                            var _varRuleJobSelect = _ctx.SBRM_ROBOT_RULE_JOB_SELECT.Where(r => r.ROBOTNAME.Equals(_rbSource.ROBOTNAME) && r.SERVERNAME.Equals(_rbSource.SERVERNAME)).ToList();

                            foreach (SBRM_ROBOT_RULE_JOB_SELECT _data in _varRuleJobSelect)
                            {
                                SBRM_ROBOT_RULE_JOB_SELECT _objAdd = new SBRM_ROBOT_RULE_JOB_SELECT();

                                _objAdd.SERVERNAME = _rbTarget.SERVERNAME;
                                _objAdd.ROBOTNAME = _rbTarget.ROBOTNAME;
                                _objAdd.LINETYPE = _rbTarget.LINETYPE;

                                _objAdd.ITEMID = _data.ITEMID;
                                _objAdd.SELECTTYPE = _data.SELECTTYPE;
                                _objAdd.DESCRIPTION = _data.DESCRIPTION;
                                _objAdd.ITEMSEQ = _data.ITEMSEQ;
                                _objAdd.OBJECTNAME = _data.OBJECTNAME;
                                _objAdd.STAGETYPE = _data.STAGETYPE;
                                _objAdd.METHODNAME = _data.METHODNAME;
                                _objAdd.ISENABLED = _data.ISENABLED;
                                _objAdd.REMARKS = _data.REMARKS;
                                _objAdd.LASTUPDATETIME = _updateTime;

                                _ctx.SBRM_ROBOT_RULE_JOB_SELECT.InsertOnSubmit(_objAdd);
                            }

                            break;

                            #endregion

                        case "SBRM_ROBOT_RULE_ROUTESTEP_BYPASS":

                            #region SBRM_ROBOT_RULE_ROUTESTEP_BYPASS

                            var _varRouteStepByPass = _ctx.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS.Where(r => r.ROBOTNAME.Equals(_rbSource.ROBOTNAME) && r.SERVERNAME.Equals(_rbSource.SERVERNAME)).ToList();

                            foreach (SBRM_ROBOT_RULE_ROUTESTEP_BYPASS _data in _varRouteStepByPass)
                            {
                                SBRM_ROBOT_RULE_ROUTESTEP_BYPASS _objAdd = new SBRM_ROBOT_RULE_ROUTESTEP_BYPASS();

                                _objAdd.SERVERNAME = _rbTarget.SERVERNAME;
                                _objAdd.ROBOTNAME = _rbTarget.ROBOTNAME;

                                _objAdd.ROUTEID = _data.ROUTEID;
                                _objAdd.STEPID = _data.STEPID;
                                _objAdd.BYPASSCONDITIONID = _data.BYPASSCONDITIONID;
                                _objAdd.DESCRIPTION = _data.DESCRIPTION;
                                _objAdd.GOTOSTEPID = _data.GOTOSTEPID;
                                _objAdd.OBJECTNAME = _data.OBJECTNAME;
                                _objAdd.METHODNAME = _data.METHODNAME;
                                _objAdd.BYPASSITEMSEQ = _data.BYPASSITEMSEQ;
                                _objAdd.ISENABLED = _data.ISENABLED;
                                _objAdd.REMARKS = _data.REMARKS;
                                _objAdd.LASTUPDATETIME = _updateTime;

                                _ctx.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS.InsertOnSubmit(_objAdd);
                            }

                            break;

                            #endregion

                        case "SBRM_ROBOT_RULE_FILTER":

                            #region SBRM_ROBOT_RULE_FILTER

                            var _varRuleFilter = _ctx.SBRM_ROBOT_RULE_FILTER.Where(r => r.ROBOTNAME.Equals(_rbSource.ROBOTNAME) && r.SERVERNAME.Equals(_rbSource.SERVERNAME)).ToList();

                            foreach (SBRM_ROBOT_RULE_FILTER _data in _varRuleFilter)
                            {
                                SBRM_ROBOT_RULE_FILTER _objAdd = new SBRM_ROBOT_RULE_FILTER();

                                _objAdd.SERVERNAME = _rbTarget.SERVERNAME;
                                _objAdd.ROBOTNAME = _rbTarget.ROBOTNAME;

                                _objAdd.ROUTEID = _data.ROUTEID;
                                _objAdd.STEPID = _data.STEPID;
                                _objAdd.ITEMID = _data.ITEMID;
                                _objAdd.ITEMSEQ = _data.ITEMSEQ;
                                _objAdd.DESCRIPTION = _data.DESCRIPTION;
                                _objAdd.OBJECTNAME = _data.OBJECTNAME;
                                _objAdd.METHODNAME = _data.METHODNAME;
                                _objAdd.ISENABLED = _data.ISENABLED;
                                _objAdd.REMARKS = _data.REMARKS;
                                _objAdd.LASTUPDATETIME = _updateTime;

                                _ctx.SBRM_ROBOT_RULE_FILTER.InsertOnSubmit(_objAdd);
                            }

                            break;

                            #endregion

                        case "SBRM_ROBOT_PROC_RESULT_HANDLE":

                            #region SBRM_ROBOT_PROC_RESULT_HANDLE

                            var _varResultHandle = _ctx.SBRM_ROBOT_PROC_RESULT_HANDLE.Where(r => r.ROBOTNAME.Equals(_rbSource.ROBOTNAME) && r.SERVERNAME.Equals(_rbSource.SERVERNAME)).ToList();

                            foreach (SBRM_ROBOT_PROC_RESULT_HANDLE _data in _varResultHandle)
                            {
                                SBRM_ROBOT_PROC_RESULT_HANDLE _objAdd = new SBRM_ROBOT_PROC_RESULT_HANDLE();

                                _objAdd.SERVERNAME = _rbTarget.SERVERNAME;
                                _objAdd.ROBOTNAME = _rbTarget.ROBOTNAME;

                                _objAdd.ROUTEID = _data.ROUTEID;
                                _objAdd.STEPID = _data.STEPID;
                                _objAdd.ITEMID = _data.ITEMID;
                                _objAdd.ITEMSEQ = _data.ITEMSEQ;
                                _objAdd.DESCRIPTION = _data.DESCRIPTION;
                                _objAdd.OBJECTNAME = _data.OBJECTNAME;
                                _objAdd.METHODNAME = _data.METHODNAME;
                                _objAdd.ISENABLED = _data.ISENABLED;
                                _objAdd.REMARKS = _data.REMARKS;
                                _objAdd.LASTUPDATETIME = _updateTime;

                                _ctx.SBRM_ROBOT_PROC_RESULT_HANDLE.InsertOnSubmit(_objAdd);
                            }

                            break;

                            #endregion

                        case "SBRM_ROBOT_RULE_ROUTESTEP_JUMP":

                            #region SBRM_ROBOT_RULE_ROUTESTEP_JUMP

                            var _varRouteStepJump = _ctx.SBRM_ROBOT_RULE_ROUTESTEP_JUMP.Where(r => r.ROBOTNAME.Equals(_rbSource.ROBOTNAME) && r.SERVERNAME.Equals(_rbSource.SERVERNAME)).ToList();

                            foreach (SBRM_ROBOT_RULE_ROUTESTEP_JUMP _data in _varRouteStepJump)
                            {
                                SBRM_ROBOT_RULE_ROUTESTEP_JUMP _objAdd = new SBRM_ROBOT_RULE_ROUTESTEP_JUMP();

                                _objAdd.SERVERNAME = _rbTarget.SERVERNAME;
                                _objAdd.ROBOTNAME = _rbTarget.ROBOTNAME;

                                _objAdd.ROUTEID = _data.ROUTEID;
                                _objAdd.STEPID = _data.STEPID;
                                _objAdd.JUMPCONDITIONID = _data.JUMPCONDITIONID;
                                _objAdd.GOTOSTEPID = _data.GOTOSTEPID;
                                _objAdd.DESCRIPTION = _data.DESCRIPTION;
                                _objAdd.OBJECTNAME = _data.OBJECTNAME;
                                _objAdd.METHODNAME = _data.METHODNAME;
                                _objAdd.JUMPITEMSEQ = _data.JUMPITEMSEQ;
                                _objAdd.ISENABLED = _data.ISENABLED;
                                _objAdd.REMARKS = _data.REMARKS;
                                _objAdd.LASTUPDATETIME = _updateTime;

                                _ctx.SBRM_ROBOT_RULE_ROUTESTEP_JUMP.InsertOnSubmit(_objAdd);
                            }

                            break;

                            #endregion

                        case "SBRM_ROBOT_RULE_ORDERBY":

                            #region SBRM_ROBOT_RULE_ORDERBY

                            var _varRuleOrderBy = _ctx.SBRM_ROBOT_RULE_ORDERBY.Where(r => r.ROBOTNAME.Equals(_rbSource.ROBOTNAME) && r.SERVERNAME.Equals(_rbSource.SERVERNAME)).ToList();

                            foreach (SBRM_ROBOT_RULE_ORDERBY _data in _varRuleOrderBy)
                            {
                                SBRM_ROBOT_RULE_ORDERBY _objAdd = new SBRM_ROBOT_RULE_ORDERBY();

                                _objAdd.SERVERNAME = _rbTarget.SERVERNAME;
                                _objAdd.ROBOTNAME = _rbTarget.ROBOTNAME;

                                _objAdd.ROUTEID = _data.ROUTEID;
                                _objAdd.STEPID = _data.STEPID;
                                _objAdd.ITEMID = _data.ITEMID;
                                _objAdd.ITEMSEQ = _data.ITEMSEQ;
                                _objAdd.DESCRIPTION = _data.DESCRIPTION;
                                _objAdd.OBJECTNAME = _data.OBJECTNAME;
                                _objAdd.METHODNAME = _data.METHODNAME;
                                _objAdd.ORDERBY = _data.ORDERBY;
                                _objAdd.ISENABLED = _data.ISENABLED;
                                _objAdd.REMARKS = _data.REMARKS;
                                _objAdd.LASTUPDATETIME = _updateTime;

                                _ctx.SBRM_ROBOT_RULE_ORDERBY.InsertOnSubmit(_objAdd);
                            }

                            break;

                            #endregion

                        default:
                            break;
                    }

                    if (SubmitChanges(_ctx) == true)
                    {
                        lsvRobotProcResult.Items.Add( new ListViewItem(new string[] { "Add     ",_rbSource.ROBOTNAME, _chk.Tag.ToString(), "  OK" }));
                        lsvRobotProcResult.Items[lsvRobotProcResult.Items.Count - 1].ForeColor = Color.Black;
                    }
                    else
                    {
                        lsvRobotProcResult.Items.Add(new ListViewItem(new string[] { "Add     ",_rbSource.ROBOTNAME, _chk.Tag.ToString(), "  NG" }));
                        lsvRobotProcResult.Items[lsvRobotProcResult.Items.Count - 1].ForeColor = Color.Red;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private bool SubmitChanges(UniBCSDataContext ctx)
        {
            string _sqlErr = string.Empty;

            try
            {
                ctx.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);

                return true;
            }
            catch (System.Data.Linq.ChangeConflictException err)
            {
                #region ChangeConflictException
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, err);

                foreach (System.Data.Linq.ObjectChangeConflict occ in ctx.ChangeConflicts)
                {
                    // 將變更的欄位寫入資料庫（合併更新）
                    occ.Resolve(System.Data.Linq.RefreshMode.KeepChanges);
                }

                try
                {
                    ctx.SubmitChanges();

                    return true;
                }
                catch (Exception ex)
                {
                    _sqlErr = ex.ToString();

                    foreach (System.Data.Linq.MemberChangeConflict _data in ctx.ChangeConflicts[0].MemberConflicts)
                    {
                        if (_data.DatabaseValue != _data.OriginalValue)
                        {
                            _sqlErr = _sqlErr + string.Format("\r\n Change Conflicts : Property '{0}': Database value: {1}, Original value {2}, Current Value:{3}", _data.Member.Name, _data.DatabaseValue, _data.OriginalValue, _data.CurrentValue);
                        }
                    }

                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                    return false;
                }
                #endregion
            }
        }

        private void chkSelectAllTable_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox _chkALL = (CheckBox)sender;

                foreach (CheckBox _chk in flpRobotTables.Controls.OfType<CheckBox>())
                {
                    _chk.Checked = _chkALL.Checked;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ReloadRobot()
        {
            try
            {
                cboTargetRobotName.DataSource = null;
                cboSourceRobotName.DataSource = null;

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                var _var = (from p in _ctx.SBRM_ROBOT
                            select new
                            {
                                Display = p.ROBOTNAME,
                                Value = p.OBJECTKEY
                            }).Distinct();

                if (_var == null ) return;

                cboSourceRobotName.DataSource = _var.ToList();

                cboSourceRobotName.DisplayMember = "Display";
                cboSourceRobotName.ValueMember = "Value";
                cboSourceRobotName.SelectedIndex = -1;

                if (CurRobot != null)
                    cboSourceRobotName.Text = CurRobot.RobotName;

                cboTargetRobotName.DataSource = _var.ToList();

                cboTargetRobotName.DisplayMember = "Display";
                cboTargetRobotName.ValueMember = "Value";
                cboTargetRobotName.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void TxtNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            UniTools.CheckTextBoxKeyPressIsInteger(sender, e);
        }

        private void RefreshOPIParameter()
        {
            try
            {
                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;

                int _num = 0;

                var _varOPIParam = _ctx.SBRM_OPI_PARAMETER.Where(d => d.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType || d.LINETYPE == "ALL");
                List<SBRM_OPI_PARAMETER> _lstSbrmOpiParam = _varOPIParam.ToList();

                foreach (SBRM_OPI_PARAMETER _param in _lstSbrmOpiParam)
                {
                    switch (_param.KEYWORD)
                    {
                        case "QueryMaxCount":
                            FormMainMDI.G_OPIAp.QueryMaxCount = (int.TryParse(_param.ITEMVALUE, out _num) == true) ? int.Parse(_param.ITEMVALUE) : 500;
                            txtQueryMaxCount.Text = FormMainMDI.G_OPIAp.QueryMaxCount.ToString();

                            break;

                        case "BCSMessageDisplayTime":
                            FormMainMDI.G_OPIAp.BCSMessageDisplayTime = (int.TryParse(_param.ITEMVALUE, out _num) == true) ? int.Parse(_param.ITEMVALUE) : 5;
                            txtBCSMessageDisplayTime.Text = FormMainMDI.G_OPIAp.BCSMessageDisplayTime.ToString();

                            break;

                        case "SocketResponseTime_Comm":
                            FormMainMDI.G_OPIAp.SocketResponseTime = (int.TryParse(_param.ITEMVALUE, out _num) == true) ? int.Parse(_param.ITEMVALUE) : 5000;
                            txtSocketResposeTime_Comm.Text = FormMainMDI.G_OPIAp.SocketResponseTime.ToString();

                            break;

                        case "SocketResponseTime_MES":
                            FormMainMDI.G_OPIAp.SocketResponseTime_MES = (int.TryParse(_param.ITEMVALUE, out _num) == true) ? int.Parse(_param.ITEMVALUE) : 15000;
                            txtSocketResposeTime_MES.Text = FormMainMDI.G_OPIAp.SocketResponseTime_MES.ToString();

                            break;

                        case "SocketResponseTime_MapDownload":
                            FormMainMDI.G_OPIAp.SocketResponseTime_MapDownload = (int.TryParse(_param.ITEMVALUE, out _num) == true) ? int.Parse(_param.ITEMVALUE) : 15000;
                            txtSocketResposeTime_MapDownload.Text = FormMainMDI.G_OPIAp.SocketResponseTime_MapDownload.ToString();

                            break;

                        case "SocketResponseTime_Query":
                            FormMainMDI.G_OPIAp.SocketResponseTime_Query = (int.TryParse(_param.ITEMVALUE, out _num) == true) ? int.Parse(_param.ITEMVALUE) : 15000;
                            txtSocketResposeTime_Query.Text = FormMainMDI.G_OPIAp.SocketResponseTime_Query.ToString();

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private DataTable GetExportData(string tableName, string nodeNo)
        {
            try
            {
                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                DataTable _dt = null;
                StringBuilder _sb = new StringBuilder();

                switch (tableName)
                {
                    case "SBRM_ALARM":
                        _sb.AppendFormat(@"
SELECT UNITNO, ALARMLEVEL, ALARMID, ALARMCODE, ALARMTEXT 
FROM {0}
WHERE NODENO = '{1}' ORDER BY OBJECTKEY", tableName, nodeNo);

                        _dt = DBConnect.GetDataTable(_ctx, tableName, _sb.ToString());
                        break;

                    case "SBRM_POSITION":
                        _sb.AppendFormat(@"
SELECT UNITTYPE, UNITNO, POSITIONNO, POSITIONNAME 
FROM {0}
WHERE NODENO = '{1}' ORDER BY OBJECTKEY", tableName, nodeNo);

                        _dt = DBConnect.GetDataTable(_ctx, tableName, _sb.ToString());
                        break;

                    case "SBRM_APCDATADOWNLOAD":
                    case "SBRM_APCDATAREPORT":
                    case "SBRM_DAILYCHECKDATA":
                    case "SBRM_ENERGYVISUALIZATIONDATA":
                    case "SBRM_PROCESSDATA":
                    case "SBRM_RECIPEPARAMETER":
                        _sb.AppendFormat(@"
SELECT SVID, PARAMETERNAME, ITEM, SITE, DESCRIPTION, 
RANGE, OPERATOR, DOTRATIO, REPORTTO, REPORTUNITNO, 
UNIT, EXPRESSION, WOFFSET, WPOINTS, BOFFSET,
BPOINTS, JOBDATAITEMNAME 
FROM {0}
WHERE NODENO = '{1}' ORDER BY OBJECTKEY", tableName, nodeNo);

                        _dt = DBConnect.GetDataTable(_ctx, tableName, _sb.ToString());
                        break;
                }

                return _dt;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                return null;
            }
        }
        
        private void DeleteExistData(string tableName, string nodeNo)
        {
            try
            {
                UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBBRMCtx;
                string serverName = FormMainMDI.G_OPIAp.CurLine.ServerName;
                string lineType = FormMainMDI.G_OPIAp.CurLine.LineType;

                switch (tableName)
                {
                    case "SBRM_ALARM":
                        #region Delete SBRM_ALARM
                        var qAlarm = (from alm in ctx.SBRM_ALARM
                                      where alm.SERVERNAME == serverName && alm.NODENO == nodeNo
                                      select alm).ToList();
                        ctx.SBRM_ALARM.DeleteAllOnSubmit(qAlarm);

                        var dAlarm = (from alm in ctx.GetChangeSet().Inserts.OfType<SBRM_ALARM>()
                                      where alm.SERVERNAME == serverName && alm.NODENO == nodeNo
                                      select alm).ToList();
                        ctx.SBRM_ALARM.DeleteAllOnSubmit(dAlarm);

                        ctx.SubmitChanges();
                        #endregion
                        break;
                    case "SBRM_POSITION":
                        #region Delete SBRM_POSITION
                        var qPosition = (from pos in ctx.SBRM_POSITION
                                         where pos.LINEID == serverName && pos.NODENO == nodeNo
                                         select pos).ToList();
                        ctx.SBRM_POSITION.DeleteAllOnSubmit(qPosition);

                        var dPosition = (from pos in ctx.GetChangeSet().Inserts.OfType<SBRM_POSITION>()
                                         where pos.LINEID == serverName && pos.NODENO == nodeNo
                                         select pos).ToList();
                        ctx.SBRM_POSITION.DeleteAllOnSubmit(dPosition);

                        ctx.SubmitChanges();
                        #endregion
                        break;
                    case "SBRM_APCDATADOWNLOAD":
                        #region Delete SBRM_APCDATADOWNLOAD
                        var qApcDownload = (from apc in ctx.SBRM_APCDATADOWNLOAD
                                            where apc.LINETYPE == lineType && apc.SERVERNAME == serverName && apc.NODENO == nodeNo
                                            select apc).ToList();
                        ctx.SBRM_APCDATADOWNLOAD.DeleteAllOnSubmit(qApcDownload);

                        var dApcDownload = (from apc in ctx.GetChangeSet().Inserts.OfType<SBRM_APCDATADOWNLOAD>()
                                            where apc.LINETYPE == lineType && apc.SERVERNAME == serverName && apc.NODENO == nodeNo
                                            select apc).ToList();
                        ctx.SBRM_APCDATADOWNLOAD.DeleteAllOnSubmit(dApcDownload);

                        ctx.SubmitChanges();
                        #endregion
                        break;
                    case "SBRM_APCDATAREPORT":
                        #region Delete SBRM_APCDATAREPORT
                        var qApcReport = (from apc in ctx.SBRM_APCDATAREPORT
                                          where apc.LINETYPE == lineType && apc.SERVERNAME == serverName && apc.NODENO == nodeNo
                                          select apc).ToList();
                        ctx.SBRM_APCDATAREPORT.DeleteAllOnSubmit(qApcReport);

                        var dApcReport = (from apc in ctx.GetChangeSet().Inserts.OfType<SBRM_APCDATAREPORT>()
                                          where apc.LINETYPE == lineType && apc.SERVERNAME == serverName && apc.NODENO == nodeNo
                                          select apc).ToList();
                        ctx.SBRM_APCDATAREPORT.DeleteAllOnSubmit(dApcReport);

                        ctx.SubmitChanges();
                        #endregion
                        break;
                    case "SBRM_DAILYCHECKDATA":
                        #region Delete SBRM_DAILYCHECKDATA
                        var qDailyCheck = (from chk in ctx.SBRM_DAILYCHECKDATA
                                           where chk.LINETYPE == lineType && chk.SERVERNAME == serverName && chk.NODENO == nodeNo
                                           select chk).ToList();
                        ctx.SBRM_DAILYCHECKDATA.DeleteAllOnSubmit(qDailyCheck);

                        var dDailyCheck = (from chk in ctx.GetChangeSet().Inserts.OfType<SBRM_DAILYCHECKDATA>()
                                           where chk.LINETYPE == lineType && chk.SERVERNAME == serverName && chk.NODENO == nodeNo
                                           select chk).ToList();
                        ctx.SBRM_DAILYCHECKDATA.DeleteAllOnSubmit(dDailyCheck);

                        ctx.SubmitChanges();
                        #endregion
                        break;
                    case "SBRM_ENERGYVISUALIZATIONDATA":
                        #region Delete SBRM_ENERGYVISUALIZATIONDATA
                        var qEnergyVisualization = (from vis in ctx.SBRM_ENERGYVISUALIZATIONDATA
                                                    where vis.LINETYPE == lineType && vis.SERVERNAME == serverName && vis.NODENO == nodeNo
                                                    select vis).ToList();
                        ctx.SBRM_ENERGYVISUALIZATIONDATA.DeleteAllOnSubmit(qEnergyVisualization);

                        var dEnergyVisualization = (from vis in ctx.GetChangeSet().Inserts.OfType<SBRM_ENERGYVISUALIZATIONDATA>()
                                                    where vis.LINETYPE == lineType && vis.SERVERNAME == serverName && vis.NODENO == nodeNo
                                                    select vis).ToList();
                        ctx.SBRM_ENERGYVISUALIZATIONDATA.DeleteAllOnSubmit(dEnergyVisualization);

                        ctx.SubmitChanges();
                        #endregion
                        break;
                    case "SBRM_PROCESSDATA":
                        #region Delete SBRM_PROCESSDATA
                        var qProcess = (from proc in ctx.SBRM_PROCESSDATA
                                        where proc.LINETYPE == lineType && proc.SERVERNAME == serverName && proc.NODENO == nodeNo
                                        select proc).ToList();
                        ctx.SBRM_PROCESSDATA.DeleteAllOnSubmit(qProcess);

                        var dProcess = (from proc in ctx.GetChangeSet().Inserts.OfType<SBRM_PROCESSDATA>()
                                        where proc.LINETYPE == lineType && proc.SERVERNAME == serverName && proc.NODENO == nodeNo
                                        select proc).ToList();
                        ctx.SBRM_PROCESSDATA.DeleteAllOnSubmit(dProcess);

                        ctx.SubmitChanges();
                        #endregion
                        break;
                    case "SBRM_RECIPEPARAMETER":
                        #region Delete SBRM_RECIPEPARAMETER
                        var qRecipeParam = (from param in ctx.SBRM_RECIPEPARAMETER
                                            where param.LINETYPE == lineType && param.SERVERNAME == serverName && param.NODENO == nodeNo
                                            select param).ToList();
                        ctx.SBRM_RECIPEPARAMETER.DeleteAllOnSubmit(qRecipeParam);

                        var dRecipeParam = (from param in ctx.GetChangeSet().Inserts.OfType<SBRM_RECIPEPARAMETER>()
                                            where param.LINETYPE == lineType && param.SERVERNAME == serverName && param.NODENO == nodeNo
                                            select param).ToList();
                        ctx.SBRM_RECIPEPARAMETER.DeleteAllOnSubmit(dRecipeParam);

                        ctx.SubmitChanges();
                        #endregion
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void lblReloadDB_DoubleClick(object sender, EventArgs e)//20161229 huangjiayin: double click select all checkbox
        {
            List<CheckBox> cbxs=new List<CheckBox>();
            cbxs.Add(cbAlarm);
            cbxs.Add(cbAPCDataDownload);
            cbxs.Add(cbAPCDataReport);
            cbxs.Add(cbDailyCheck);
            cbxs.Add(cbEachPosition);
            cbxs.Add(cbEnergyVisualization);
            cbxs.Add(cbProcessData);
            cbxs.Add(cbRecipeParameter);

            bool all_checked = false;

            foreach (CheckBox cb in cbxs)
            {
                if (!cb.Checked)
                {
                    all_checked = true;
                    break;
 
                }
            }

            foreach (CheckBox cb in cbxs)
            {
                cb.Checked = all_checked ? true : false;
            }


        }

        //public void GetParameterBCS()
        //{
        //    try
        //    {
        //        if (FormMainMDI.G_OPIAp.Lst_BCSParameterItem == null || FormMainMDI.G_OPIAp.Lst_BCSParameterItem.Count == 0) return;

        //        #region Send BCSParameterDataInfoRequest

        //        BCSParameterDataInfoRequest _trx = new BCSParameterDataInfoRequest();

        //        _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
        //        _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

        //        foreach (string _item in FormMainMDI.G_OPIAp.Lst_BCSParameterItem)
        //        {
        //            BCSParameterDataInfoRequest.PARAMETERc _param = new BCSParameterDataInfoRequest.PARAMETERc();
        //            _param.NAME = _item;
                    
        //            _trx.BODY.PARAMETERLIST.Add(_param);
        //        }

        //        MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

        //        if (_resp == null) return;
               
        //        #endregion

        //        #region BCSParameterDataInfoReply

        //        string _respXml = _resp.Xml;

        //        BCSParameterDataInfoReply _reply = (BCSParameterDataInfoReply)Spec.CheckXMLFormat(_respXml);

        //        string _objName = string.Empty ;
        //        foreach (BCSParameterDataInfoReply.PARAMETERc _replyParam in _reply.BODY.PARAMETERLIST)
        //        {
        //            _objName = string.Format("txt{0}", _replyParam.NAME);

        //            TextBox _txt = flpOPIParameter.Controls.Find(_objName, true).OfType<TextBox>().FirstOrDefault();

        //            if (_txt == null) continue;

        //            _txt.Text = _replyParam.VALUE;
        //        }

        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
        //    }
        //}
    }
}
