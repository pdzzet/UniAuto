using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormRobotControl : FormBase
    {
        Robot CurRobot = null;

        public FormRobotControl()
        {
            InitializeComponent();
        }

        private void FormRobotControl_Load(object sender, EventArgs e)
        {
            try
            {
                #region Load Robot Image
                string _robotPath = OPIConst.RobotFolder + string.Format(FormMainMDI.G_OPIAp.CurLine.ServerName + ".png");

                if (File.Exists(_robotPath))
                {
                    pnlRobotPic.BackgroundImage = new Bitmap(_robotPath);
                }
                #endregion

                #region Check 是否有相同性質的insp機台，若有 提供same eqp flag顯示&設定 -- just for array
                //bool _useSameEQPFlag =false ;
                //if (FormMainMDI.G_OPIAp.CurLine.FabType.Equals("ARRAY"))
                //{
                //    var _var = FormMainMDI.G_OPIAp.Dic_Node.Values.Where(r => r.NodeAttribute.Equals("IN")).ToList();

                //    foreach (Node _node in _var)
                //    {
                //        if (FormMainMDI.G_OPIAp.Dic_Node.Values.Where(r => r.NodeName.Equals(_node.NodeName)).Count() > 1)
                //        {
                //            _useSameEQPFlag = true;
                //            break;
                //        }
                //    }
                //}

                pnlMixRunFlag.Visible = FormMainMDI.G_OPIAp.RobotMixRunFlag;
                pnlMixRunFlag_New.Visible = FormMainMDI.G_OPIAp.RobotMixRunFlag;
                
                #endregion

                #region Load Robot Name Radio Button
                RadioButton _firstRdo = null;
                foreach (Robot _rb in FormMainMDI.G_OPIAp.Dic_Robot.Values)
                {
                    RadioButton _rdo = new RadioButton();
                    _rdo.Name = _rb.RobotName;
                    _rdo.AutoSize = true;
                    _rdo.Text = _rb.RobotName;
                    _rdo.Checked = false;
                    _rdo.CheckedChanged +=new EventHandler(Robot_CheckedChanged);
                    flpRobotList.Controls.Add(_rdo);

                    if (_firstRdo == null) _firstRdo = _rdo;
                }

                if (_firstRdo != null) _firstRdo.Checked = true;

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                Send_RobotCurrentModeRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Robot_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                if (_rdo.Checked == false) return;

                string _robotName = _rdo.Name;

                if (CurRobot == null || _robotName != CurRobot.RobotName)
                {
                    flpCommand.Controls.Clear();

                    if (FormMainMDI.G_OPIAp.Dic_Robot.ContainsKey(_robotName))
                    {
                        CurRobot = FormMainMDI.G_OPIAp.Dic_Robot[_robotName];

                        txtRobotName.Text = CurRobot.RobotName;
                    }
                    else
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Can't find Robot Name [{0}]", _robotName), MessageBoxIcon.Error);
                        return;
                    }
                }

                #region Load Robot Command
                for (int i = 1; i <= CurRobot.RobotArmCount; i++)
                {
                    ucSemiCommand _uc = new ucSemiCommand();
                    _uc.InitialCommand(i, CurRobot);
                    _uc.Name = i.ToString() + "_SemiCommand";
                    _uc.chkChoose.CheckedChanged += new EventHandler(chkChoose_CheckedChanged);
                    flpCommand.Controls.Add(_uc);
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void chkChoose_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox _chk = (CheckBox)sender;

                string _ucName = string.Empty;
                int _seq = 0;
                int.TryParse(_chk.Parent.Name.Substring(0, 1), out _seq);

                if (_chk.Checked == false)
                {
                    #region 取消command，連同後面的command都需要取消
                    for (int i = _seq + 1; i <= 4; i++)
                    {
                        _ucName = i.ToString() + "_SemiCommand";

                        var _var = flpCommand.Controls.Find(i.ToString() + "_SemiCommand", false).OfType<ucSemiCommand>();

                        if (_var.Count() == 0) return;

                        ucSemiCommand _uc = _var.First();
                        if (_uc.chkChoose.Checked) _uc.chkChoose.Checked = false;
                    }
                    #endregion
                }
                else
                {
                    #region RobotControlMode = SEMI方可操作
                    if (CurRobot.RobotControlMode != "SEMI")
                    {
                        ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Robot Control Mode is not SEMI !", MessageBoxIcon.Error);
                        _chk.Checked = false;
                        return;
                    }                    
                    #endregion

                    #region 設定command須依照command seq no順序設定
                    for (int i = 1; i < _seq; i++)
                    {
                        _ucName = i.ToString() + "_SemiCommand";

                        var _var = flpCommand.Controls.Find(i.ToString() + "_SemiCommand", false).OfType<ucSemiCommand>();

                        if (_var.Count() == 0) return;

                        ucSemiCommand _uc = _var.First();
                        if (_uc.chkChoose.Checked == false)
                        {
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", string.Format("{0} Semi Command must be Set", i.ToString()), MessageBoxIcon.Error);
                            _chk.Checked = false;
                            return;
                        }
                    }
                    #endregion
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        
        private void btnSendRobotCommand_Click(object sender, EventArgs e)
        {
            try
            {                
                #region 判斷目前control mode是否為semi mode
                if (CurRobot == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose Robot ! ", MessageBoxIcon.Error);
                    return;
                }

                if (CurRobot.RobotControlMode != "SEMI")
                {
                    ShowMessage(this, lblCaption.Text, "", "Robot Control Mode is not SEMI Mode", MessageBoxIcon.Error);
                    return;                    
                }
                 #endregion

                int _slotNo = 0;
                int _maxNo = 0;
                Dictionary<string,RobotCommandRequest.COMMANDc> _positionSlot =new Dictionary<string,RobotCommandRequest.COMMANDc>();

                #region Get RobotCommand

                foreach (ucSemiCommand _uc in flpCommand.Controls.OfType<ucSemiCommand>())
                {
                    if (_uc.chkChoose.Checked == false) break;

                    RobotCommandRequest.COMMANDc _cmd = new UniAuto.UniBCS.OpiSpec.RobotCommandRequest.COMMANDc();

                    _cmd.COMMAND_SEQ = _uc.CommandSeqNo.ToString();
                    _cmd.ROBOT_COMMAND  = _uc.GetArmCommand();
                    _cmd.ARM_SELECT  = _uc.GetArmSelect();
                    _cmd.TARGETPOSITION  = _uc.GetTargetPosition();

                    if (_cmd.ROBOT_COMMAND == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Command required in Robot Command [{0}]！", _uc.CommandSeqNo), MessageBoxIcon.Error);
                        return;
                    }

                    if (_cmd.ARM_SELECT == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Arm Select required in Robot Command [{0}]！", _uc.CommandSeqNo), MessageBoxIcon.Error);
                        return;
                    }

                    if (_cmd.TARGETPOSITION == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Target Position required in Robot Command [{0}]！", _uc.CommandSeqNo), MessageBoxIcon.Error);
                        return;
                    }

                    _slotNo = _uc.GetTargetSlotNo();
                    _maxNo = _uc.GetTargetSlotMaxCount();
                    if (_slotNo > _maxNo || _slotNo < 1)
                    {
                        ShowMessage(new Form(), "Robot Command", "", string.Format("{0}'st Robot Command Target Slot is not Between 1~{1}！", _uc.CommandSeqNo, _maxNo.ToString()), MessageBoxIcon.Error);
                        _uc.txtTargetSlotNo.Focus();
                        _uc.txtTargetSlotNo.SelectAll();
                        return;
                    }

                    _cmd.TARGETSLOTNO = _uc.GetTargetSlotNo().ToString().PadLeft(2, '0');

                    if (_cmd.TARGETSLOTNO == "00")
                    {
                        ShowMessage(new Form(), "Robot Command", "", string.Format("{0}'st Robot Command Target Slot 1 is not Between 1~{1}！", _uc.CommandSeqNo, _uc.GetTargetSlotMaxCount().ToString()), MessageBoxIcon.Error);                 
                        return;
                    }

                    _positionSlot.Add(_uc.CommandSeqNo.ToString(), _cmd);
                }
                #endregion
               
                foreach (string _key1 in _positionSlot.Keys)
                {
                    #region TargetPos && TargetSlotNo 兩者都相同者不可下command
                    foreach (string _key2 in _positionSlot.Keys)
                    {
                        if (_key1 == _key2) continue ;

                        if (_positionSlot[_key1].TARGETPOSITION == _positionSlot[_key2].TARGETPOSITION &&
                            _positionSlot[_key1].TARGETSLOTNO == _positionSlot[_key2].TARGETSLOTNO)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", string.Format("Target Position and Target SlotNo must be different in {0}'st Robot Command & {1}'st Robot Command ! ",_key1,_key2), MessageBoxIcon.Error);
                            return;
                        }
                    }
                    #endregion

                    #region 當選擇Both Arm後,Target SlotNo不可以是偶數(2,4,,6,8,10,12,14......)
                    //"3:Both Arm" or "3:Left Both Arm", "12:Right Both Arm"
                    if (_positionSlot[_key1].ARM_SELECT == "3" || _positionSlot[_key1].ARM_SELECT == "12")
                    {
                        int.TryParse(_positionSlot[_key1].TARGETSLOTNO.ToString(), out _slotNo);

                        if ((_slotNo & 1) == 0)
                        {
                            FormMainMDI.FrmOPIMessage.ShowMessage(new Form(), "Robot Command", "", string.Format("{0}'st Robot Command Target Slot must be odd number [1,3,5...]！", _key1), MessageBoxIcon.Error);
                            return;
                        }                        
                    }
                    #endregion
                }
               
                if (_positionSlot.Count == 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "No Command to Process", MessageBoxIcon.Warning);
                    return;
                }

                #region Send RobotSemiCommandRequest
                Send_RobotSemiCommandRequest(_positionSlot);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //更新顯示Robot Control Mode
        private void Refresh_ControlMode()
        {
            try
            {
                string _robotName = string.Empty;
                string _status = string.Empty;

                if (CurRobot == null) return;

                //SEMI , AUTO
                _status = CurRobot.RobotControlMode;

                if (_status != txtControlMode.Text.ToString())
                {
                    txtControlMode.Text = CurRobot.RobotControlMode.ToString();

                    //robot command 僅在change時跟著變動，避免清掉user要下command的設定
                    if (_status == "SEMI") { rdoSEMI.Checked = true; rdoAUTO.Checked = false; }
                    else if (_status == "AUTO") { rdoAUTO.Checked = true; rdoSEMI.Checked = false; }
                }

                //Hold , Release
                _status = CurRobot.HoldStatus ? "Hold" : "Release";
                if (_status != txtHoldStatus.Text.ToString())
                {
                    txtHoldStatus.Text = _status;

                    //robot command 僅在change時跟著變動，避免清掉user要下command的設定
                    if (CurRobot.HoldStatus) { rdoHold.Checked = true; colRelease.Checked = false; }
                    else if (CurRobot.HoldStatus == false) { rdoHold.Checked = false; colRelease.Checked = true; }
                }

                //Yes , No
                _status = CurRobot.SampEQPFlag ? "Yes" : "No";
                if (_status != txtMixRunFlag.Text.ToString())
                {
                    txtMixRunFlag.Text = _status;

                    if (CurRobot.SampEQPFlag) { rdoSameYes.Checked = true; rdoSameNo.Checked = false; }
                    else if (CurRobot.SampEQPFlag == false) { rdoSameYes.Checked = false; rdoSameNo.Checked = true; }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                tmrBaseRefresh.Enabled = false;
            }
        }

        private void btnNewControlMode_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurRobot == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Robot", MessageBoxIcon.Error);
                    return;
                }

                if (rdoSEMI.Checked == false && rdoAUTO.Checked == false)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose New Control Mode", MessageBoxIcon.Error);
                    return;
                }

                string _newMode = string.Empty;

                if (rdoSEMI.Checked) _newMode = "SEMI";
                else if (rdoAUTO.Checked) _newMode = "AUTO";

                string _msg = string.Format("Please confirm whether you will change the robot Control mode [{0}] of Robot [{1}] ?", _newMode, CurRobot.RobotName);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                Send_RobotControlModeChange(_newMode);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnNewHoldStatus_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurRobot == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Robot", MessageBoxIcon.Error);
                    return;
                }

                if (rdoHold.Checked == false && colRelease.Checked == false)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Robot Hold Status", MessageBoxIcon.Error);
                    return;
                }

                string _newMode = string.Empty ;
                string _newModeDesc = string.Empty;

                if (rdoHold.Checked) { _newModeDesc = "Hold"; _newMode = "1"; }
                else if (colRelease.Checked) { _newModeDesc = "Release"; _newMode = "0"; }

                string _msg = string.Format("Please confirm whether you will change the robot Hold Status [{0}] of Robot [{1}] ?", _newModeDesc, CurRobot.RobotName);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                Send_RobotCommandHoldRequest(_newMode);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnMixRunFlag_New_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurRobot == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Robot", MessageBoxIcon.Error);
                    return;
                }

                if (rdoSameYes.Checked == false && rdoSameNo.Checked == false)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Same EQP Flag", MessageBoxIcon.Error);
                    return;
                }

                string _newMode = string.Empty;
                string _newModeDesc = string.Empty;

                if (rdoSameYes.Checked) { _newModeDesc = "Y"; _newMode = "Y"; }
                else if (rdoSameNo.Checked) { _newModeDesc = "N"; _newMode = "N"; }

                string _msg = string.Format("Please confirm whether you will change the robot Same EQP Flag [{0}] of Robot [{1}] ?", _newModeDesc, CurRobot.RobotName);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                Send_RobotJobSendToSameEQRequest(_newMode);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void tmrBaseRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString())
                {
                    tmrBaseRefresh.Enabled = false;
                    return;
                }

                Refresh_ControlMode();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        #region Send Trx

        private void Send_RobotCurrentModeRequest()
        {
            try
            {
                string _err = string.Empty;

                RobotCurrentModeRequest _trx = new RobotCurrentModeRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                string _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                ShowMessage(this, lblCaption.Text, "", "Reload Robot Information Send to BC Success !", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_RobotControlModeChange(string NewMode)
        {
            try
            {
                RobotModeChangeRequest _trx = new RobotModeChangeRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurRobot.NodeNo;
                _trx.BODY.ROBOTNAME = CurRobot.RobotName;
                _trx.BODY.ROBOTMODE = NewMode;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region RobotModeChangeReply
                ShowMessage(this, lblCaption.Text, "", "Robot Mode Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_RobotJobSendToSameEQRequest(string NewMode)
        {
            try
            {
                RobotJobSendToSameEQRequest _trx = new RobotJobSendToSameEQRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurRobot.NodeNo;
                _trx.BODY.ROBOTNAME = CurRobot.RobotName;
                _trx.BODY.SAMEEQFLAG = NewMode;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region RobotJobSendToSameEQReply
                ShowMessage(this, lblCaption.Text, "", "Robot Same EQP Flag Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_RobotCommandHoldRequest(string NewMode)
        {
            try
            {
                RobotCommandHoldRequest _trx = new RobotCommandHoldRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurRobot.NodeNo;
                _trx.BODY.ROBOTNAME = CurRobot.RobotName;
                _trx.BODY.HOLD_STATUS = NewMode;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region RobotCommandHoldReply
                ShowMessage(this, lblCaption.Text, "", "Robot Hold Status Send to BC Success !", MessageBoxIcon.Information);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_RobotSemiCommandRequest(Dictionary<string, RobotCommandRequest.COMMANDc> positionSlot)
        {
            try
            {
                #region Send RobotSemiCommandRequest
                RobotCommandRequest _trx = new RobotCommandRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurRobot.NodeNo;
                _trx.BODY.ROBOTNAME = CurRobot.RobotName;

                _trx.BODY.COMMANDLIST = new List<UniAuto.UniBCS.OpiSpec.RobotCommandRequest.COMMANDc>();

                foreach (RobotCommandRequest.COMMANDc _cmd in positionSlot.Values)
                {
                    _trx.BODY.COMMANDLIST.Add(_cmd);
                }

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region RobotSemiCommandReply
                ShowMessage(this, lblCaption.Text, "", "Robot Semi Command Send to BC Success !", MessageBoxIcon.Information);
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        } 
        #endregion





    }
}
