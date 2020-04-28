using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormLineControl : FormBase
    {

        public FormLineControl()
        {
            InitializeComponent();
            lblCaption.Text = "Line Control";               
        }

        private void Button_Click(object sender, EventArgs e)
        {
            try
            {
                string _msg = string.Empty ;
                string _err = string.Empty;
                string _xml = string.Empty;
                Button _btn = (Button)sender;

                switch (_btn.Name)
                {
                    case "btnRemote":
                    case "btnLocal":
                    case "btnOffline":

                        #region Line Mode Change
                        if (_btn.BackColor == Color.Lime)
                        {
                            _msg = string.Format("Line Mode was already [{0}]", _btn.Text);
                            ShowMessage(this, lblCaption.Text , "", _msg, MessageBoxIcon.Error);
                            return;
                        }

                        if (_btn.Name == "btnOffline")
                        {
                            if(FormMainMDI.G_OPIAp.CurLine.FabType=="MODULE" )
                            {
                                List<Port> allPorts = FormMainMDI.G_OPIAp.Dic_Port.Values.Where(w=>w.PortType==ePortType.LoadingPort&&w.PortStatus==ePortStatus.LoadComplete).ToList();
                                if (allPorts.Count > 0)
                                {
                                    _msg = string.Format("Port Status is LC, can't do this action!");
                                    ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);
                                    return; 
                                }
                            }
                        }


                        _msg = string.Format("Change Line Mode to [{0}].", _btn.Text);
                        if (DialogResult.No == this.QuectionMessage(this,lblCaption.Text,_msg)) return;

                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                        LineModeChangeRequest _lineModeChangeRequest = new LineModeChangeRequest();
                        _lineModeChangeRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _lineModeChangeRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _lineModeChangeRequest.BODY.LINEID = FormMainMDI.G_OPIAp.CurLine.LineID;
                        _lineModeChangeRequest.BODY.LINEMODE = ((Button)sender).Text.ToString();
                        _lineModeChangeRequest.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                        _xml = _lineModeChangeRequest.WriteToXml();


                        //15秒timeout 
                        MessageResponse _resp = this.SendRequestResponse(_lineModeChangeRequest.HEADER.TRANSACTIONID, _lineModeChangeRequest.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SocketResponseTime_MES);

                        if (_resp == null) return;

                        #region LineModeChangeReply
                        ShowMessage(this, lblCaption.Text, "", "Line Mode Change Request Send to BC Success !", MessageBoxIcon.Information);
                        #endregion
                        
                        #endregion

                        break;

                    case "btnRemote2":
                    case "btnLocal2":
                    case "btnOffline2":

                        #region Line Mode Change --- line 2
                        if (_btn.BackColor == Color.Lime)
                        {
                            _msg = string.Format("Line Mode was already [{0}]", _btn.Text);
                            ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);
                            return;
                        }

                        _msg = string.Format("Change Line Mode to [{0}].", _btn.Text);
                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text,  _msg)) return;

                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                        LineModeChangeRequest _lineModeChangeRequest2 = new LineModeChangeRequest();
                        _lineModeChangeRequest2.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _lineModeChangeRequest2.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _lineModeChangeRequest2.BODY.LINEID = FormMainMDI.G_OPIAp.CurLine.LineID2;
                        _lineModeChangeRequest2.BODY.LINEMODE = ((Button)sender).Text.ToString();
                        _lineModeChangeRequest2.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                        _xml = _lineModeChangeRequest2.WriteToXml();


                        ////15秒timeout
                        //if (!this.SendRequestResponse(_lineModeChangeRequest2.HEADER.TRANSACTIONID, _lineModeChangeRequest2.HEADER.MESSAGENAME, _xml, 15, FormMainMDI.G_OPIAp.SessionID)) return;

                        //15秒timeout 
                        MessageResponse _resp2 = this.SendRequestResponse(_lineModeChangeRequest2.HEADER.TRANSACTIONID, _lineModeChangeRequest2.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SocketResponseTime_MES);

                        if (_resp2 == null) return;

                        #region LineModeChangeReply
                        ShowMessage(this, lblCaption.Text, "", "Line Mode Change Request Send to BC Success !", MessageBoxIcon.Information);
                        #endregion

                        #endregion

                        break;

                    case "btnLineRefresh":

                        RefreshData();

                        ShowMessage(this, lblCaption.Text, "", "Reload Line Information  Send to BC Success !", MessageBoxIcon.Information);    

                        //#region Get Line Information 
                        ////if (DialogResult.Cancel == this.ConfirmPassword(this, MethodBase.GetCurrentMethod().Name)) return;
                        //LineStatusRequest _lineStatusRequest = new LineStatusRequest();
                        //_lineStatusRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        //_lineStatusRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        //_xml = _lineStatusRequest.WriteToXml();

                        ////if (!this.SendRequestResponse(_lineStatusRequest.HEADER.TRANSACTIONID, _lineStatusRequest.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SessionID)) return;

                        //FormMainMDI.SocketDriver.SendMessage(_lineStatusRequest.HEADER.TRANSACTIONID, _lineStatusRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                        //ShowMessage(this, lblCaption.Text, "", "Reload Line Information  Send to BC Success !", MessageBoxIcon.Information);    

                        //#endregion

                        //#region Get Line Table 
                        //UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                        //var _var = from data in ctx.SBRM_LINE
                        //           where data.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                        //           select data.CHECKCROSSRECIPE;

                        //if (_var.ToString() == "Y") FormMainMDI.G_OPIAp.CurLine.CrossLineRecipeCheck = true;
                        //else FormMainMDI.G_OPIAp.CurLine.CrossLineRecipeCheck = false;
                        //#endregion

                        break;

                    case "btnMESAlive":

                        #region MESAliveRequest - Are You There Request
                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text,  "Check MES Alive? [Are You There Request]")) return;
                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;
                        
                        MESAliveRequest _mesAliveRequest = new MESAliveRequest();
                        _mesAliveRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _mesAliveRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                        _xml = _mesAliveRequest.WriteToXml();

                        MessageResponse __mesAliveResponse = this.SendRequestResponse(_mesAliveRequest.HEADER.TRANSACTIONID, _mesAliveRequest.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SocketResponseTime_MES);

                        if (__mesAliveResponse == null) return;

                        #region MESAliveReply
                        ShowMessage(this, lblCaption.Text, "", "MES Alive Request Send to BC Success !", MessageBoxIcon.Information);
                        #endregion

                        #endregion

                        break;

                    case "btnForceON": 
                    case "btnForceOFF":

                        #region ForceCleanOutCommand - Force Clean Out 
                        _msg = string.Format("Set {0} signal bit {1}? ", lblForceCleanOut.Text.ToString(), _btn.Text );
                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;
           
                        ForceCleanOutCommand _forceCleanOutCommand = new ForceCleanOutCommand();
                        _forceCleanOutCommand.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _forceCleanOutCommand.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _forceCleanOutCommand.BODY.COMMAND = lblForceCleanOut.Tag.ToString();
                        _forceCleanOutCommand.BODY.STATUS = _btn.Tag.ToString();
                        _forceCleanOutCommand.BODY.USERID = FormMainMDI.G_OPIAp.LoginUserID;

                        _xml = _forceCleanOutCommand.WriteToXml();

                    
                        //15秒timeout 
                        MessageResponse _forceCleanOutCommandResponse = this.SendRequestResponse(_forceCleanOutCommand.HEADER.TRANSACTIONID, _forceCleanOutCommand.HEADER.MESSAGENAME, _xml, 0);

                        if (_forceCleanOutCommandResponse == null) return;

                        #region ForceCleanOutCommandReply
                        ShowMessage(this, lblCaption.Text, "", "Force Clean Out Command Request Send to BC Success !", MessageBoxIcon.Information);
                        #endregion

                        #endregion

                        break;

                    case "btnAbnormalForceON":
                    case "btnAbnormalForceOFF":

                        #region ForceCleanOutCommand -  Abnormal Force Clean Out
                        _msg = string.Format("Set {0} signal bit {1}? ", lblAbnormalForceCleanOut.Text.ToString(), _btn.Text);
                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text,  _msg)) return;
                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                        ForceCleanOutCommand _abnormalForceCleanOutCommand = new ForceCleanOutCommand();
                        _abnormalForceCleanOutCommand.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _abnormalForceCleanOutCommand.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _abnormalForceCleanOutCommand.BODY.COMMAND = lblAbnormalForceCleanOut.Tag.ToString();
                        _abnormalForceCleanOutCommand.BODY.STATUS = _btn.Tag.ToString();
                        _abnormalForceCleanOutCommand.BODY.USERID = FormMainMDI.G_OPIAp.LoginUserID;
                        _xml = _abnormalForceCleanOutCommand.WriteToXml();

                        //15秒timeout 
                        MessageResponse _abnormalForceCleanOutResponse = this.SendRequestResponse(_abnormalForceCleanOutCommand.HEADER.TRANSACTIONID, _abnormalForceCleanOutCommand.HEADER.MESSAGENAME, _xml, 0);

                        if (_abnormalForceCleanOutResponse == null) return;

                        #region ForceCleanOutCommandReply
                        ShowMessage(this, lblCaption.Text, "", "Abnormal Force Clean Out Command Request Send to BC Success !", MessageBoxIcon.Information);
                        #endregion

                        #endregion

                        break;

                    case "btnCrossRecipeCheckSet":

                        string _chk = chkCrossRecipeCheck.Checked ? "Y" : "N";

                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text,  string.Format("Set Cross Recipe Check [{0}]?", _chk))) return;
                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                        #region 更新DB 
                        UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;
                        SBRM_LINE _line = (from p in _ctx.SBRM_LINE
                                                   where p.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                                                   select p).SingleOrDefault();

                        SBRM_LINE _new = null;
                        if (_line != null)
                        {
                            _new = _line;
                            _new.CHECKCROSSRECIPE = _chk;
                        }
                        _ctx.SubmitChanges();                        
                        #endregion

                        #region send DatabaseReloadRequest
                         _xml = string.Empty;
                        DatabaseReloadRequest _trx = new DatabaseReloadRequest();
                        _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _trx.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                        _trx.BODY.TABLENAME = "SBRM_LINE:CHECKCROSSRECIPE";

                        _xml = _trx.WriteToXml();

                        MessageResponse _dbReloadRequestResponse = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                        if (_dbReloadRequestResponse == null) return;

                        #region DatabaseReloadReply
                        ShowMessage(this, lblCaption.Text, "", "Cross Recipe Check Change Send to BC Success !", MessageBoxIcon.Information);
                        #endregion

                        #endregion
                                                
                        FormMainMDI.G_OPIAp.CurLine.CrossLineRecipeCheck = chkCrossRecipeCheck.Checked;

                        break;

                    default :
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }
    
        private void RefreshLineInfo()
        {
            try
            {
                txtLineOperation.Text = FormMainMDI.G_OPIAp.CurLine.LineOperMode;
                txtCrossRecipeCheck.Text = FormMainMDI.G_OPIAp.CurLine.CrossLineRecipeCheck ? "Y" : "N";
                txtCassetteOperationMode.Text = FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode.ToString();
                txtDailyCheckTime.Text = FormMainMDI.G_OPIAp.CurLine.DailyCheckReportTime.ToString()+" s";

                btnOffline.BackColor = FormMainMDI.G_OPIAp.CurLine.MesControlMode == btnOffline.Text ? Color.Lime : Color.FromKnownColor(KnownColor.Control);
                btnLocal.BackColor = FormMainMDI.G_OPIAp.CurLine.MesControlMode == btnLocal.Text ? Color.Lime : Color.FromKnownColor(KnownColor.Control);
                btnRemote.BackColor = FormMainMDI.G_OPIAp.CurLine.MesControlMode == btnRemote.Text ? Color.Lime : Color.FromKnownColor(KnownColor.Control);

                btnOffline2.BackColor = FormMainMDI.G_OPIAp.CurLine.MesControlMode2 == btnOffline2.Text ? Color.Lime : Color.FromKnownColor(KnownColor.Control);
                btnLocal2.BackColor = FormMainMDI.G_OPIAp.CurLine.MesControlMode2 == btnLocal2.Text ? Color.Lime : Color.FromKnownColor(KnownColor.Control);
                btnRemote2.BackColor = FormMainMDI.G_OPIAp.CurLine.MesControlMode2 == btnRemote2.Text ? Color.Lime : Color.FromKnownColor(KnownColor.Control);

                #region  Inspection Flow Priority 
                //if (grbInspectionFlowPriority.Visible)
                //{
                //    string _localNo = string.Empty;
                //    int _priority = 0;

                //    foreach (DataGridViewRow _row in dgvInspectionFlowPriority.Rows)
                //    {
                //        _localNo = _row.Cells[colInspFlowPriorityLocalNo.Name].Value.ToString();

                //        if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_localNo))
                //        {
                //            int.TryParse(_row.Cells[colInspFlowPriority.Name].Value.ToString(),out _priority);
                //            if (_priority != FormMainMDI.G_OPIAp.Dic_Node[_localNo].FlowPriority)
                //            {
                //                _row.Cells[colInspFlowPriority.Name].Value = FormMainMDI.G_OPIAp.Dic_Node[_localNo].FlowPriority.ToString();
                //            }
                //        }
                //    }
                //}
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void FormLineControl_Load(object sender, EventArgs e)
        {
            try
            {
                txtFactoryType.Text = FormMainMDI.G_OPIAp.CurLine.FabType;
                txtLineType.Text = FormMainMDI.G_OPIAp.CurLine.LineType;
                txtLineName.Text = FormMainMDI.G_OPIAp.CurLine.ServerName;
                txtLineID.Text = FormMainMDI.G_OPIAp.CurLine.LineID;
                txtLineID2.Text = FormMainMDI.G_OPIAp.CurLine.LineID2;

                #region 判斷是否有兩條lineid ---特殊處理，不使用DB設定顯示(需與目前line mode切換放上面)
                //if (FormMainMDI.G_OPIAp.CurLine.LineID2 == string.Empty)
                //{
                //    tlpBase.RowStyles[1].Height = 0;
                //    pnlLineID2.Visible = false;
                //}
                //else
                //{
                //    tlpBase.RowStyles[1].Height = 175;
                //    pnlLineID2.Visible = true;
                //}
                #endregion

                #region TBBFG機台沒有REMOTE MODE的模式
                if (FormMainMDI.G_OPIAp.CurLine.LineType == "BFG")
                {
                    btnRemote.Visible = false;
                    btnLocal.Width = 245;
                    btnOffline.Width = 245;
                }
                #endregion

                #region 判斷是否有force clean out / abnormal force clean out / Cross Line Recipe Check / Inspection Flow Priority

                List<string> _lstUse  = FormMainMDI.G_OPIAp.CurLine.LineSpecialFun.Split(',').ToList();

                foreach (GroupBox _grb in flpSpecial.Controls.OfType<GroupBox>())
                {
                    if (_lstUse.Contains(_grb.Tag.ToString())) _grb.Visible =true ;
                    else  _grb.Visible =false ;
                }

                foreach (Panel _pnl in flpSpecialInfo.Controls.OfType<Panel>())
                {
                    if (_lstUse.Contains(_pnl.Tag.ToString())) _pnl.Visible = true;
                    else _pnl.Visible = false;
                }
                
                #endregion

                RefreshLineInfo();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnQueryForce_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender ;

                FormForceCleanOutReport _frm = new FormForceCleanOutReport(_btn.Tag.ToString());
                _frm.TopMost = true;
                _frm.ShowDialog();
                _frm.Dispose();               
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString())
                {
                    tmrRefresh.Enabled = false;
                    return;
                }

                RefreshLineInfo();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                tmrBaseRefresh.Enabled = false;
            }
        }

        private  void NumericCheck(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar);
        }

        public void RefreshData()
        {
            try
            {
                string _xml = string.Empty;
                string _err = string.Empty;

                #region Get Line Information 
                LineStatusRequest _lineStatusRequest = new LineStatusRequest();
                _lineStatusRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _lineStatusRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _xml = _lineStatusRequest.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_lineStatusRequest.HEADER.TRANSACTIONID, _lineStatusRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);                

                #endregion

                #region Get Line Table 
                UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                var _var = from data in ctx.SBRM_LINE
                            where data.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                            select data.CHECKCROSSRECIPE;

                if (_var.ToString() == "Y") FormMainMDI.G_OPIAp.CurLine.CrossLineRecipeCheck = true;
                else FormMainMDI.G_OPIAp.CurLine.CrossLineRecipeCheck = false;
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
