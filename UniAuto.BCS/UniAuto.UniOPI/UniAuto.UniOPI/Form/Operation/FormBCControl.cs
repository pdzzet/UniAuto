using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormBCControl : FormBase
    {
        private const string EQP_UNIT_NO = "00";

        public FormBCControl()
        {
            InitializeComponent();
        }

        private void FormBCControl_Load(object sender, EventArgs e)
        {
            InitialData();

            if (FormMainMDI.G_OPIAp.CurLine.FabType == "CELL")
            {
                gbProcessPauseCmd.Visible = false;
                gbProcessStop.Visible = false;
            }
            else if (FormMainMDI.G_OPIAp.CurLine.LineType == "ELA_JSW") //ELA Port Can Fetch Out Check,Deng,20190828
            {
                gbCellLineSpecialCheck.Size = new System.Drawing.Size(1077, 250);
            }
            else
            {
                gbCellLineSpecialCheck.Visible = false;
            }
        }

        private void InitialData()
        {
            try
            {
                List<comboxInfo> _lstNodeNo = new List<comboxInfo>();
                List<comboxInfo> _lstMPLCNodeNo = new List<comboxInfo>();

                foreach (KeyValuePair<string, Node> node in FormMainMDI.G_OPIAp.Dic_Node)
                {
                    _lstNodeNo.Add(new comboxInfo { ITEM_ID = node.Value.NodeNo, ITEM_NAME = node.Value.NodeName });

                    if (node.Value.InterLocks.Count() > 0)
                    {
                        _lstMPLCNodeNo.Add(new comboxInfo { ITEM_ID = node.Value.NodeNo, ITEM_NAME = node.Value.NodeName });
                    }
                }

                #region  Transfor Stop/Process Stop
                cboTransStopEQP.DataSource = _lstNodeNo.ToList();
                cboTransStopEQP.DisplayMember = "ITEM_DESC";
                cboTransStopEQP.ValueMember = "ITEM_ID";
                cboTransStopEQP.SelectedIndex = -1;

                cboProcStopEQP.DataSource = _lstNodeNo.ToList();
                cboProcStopEQP.DisplayMember = "ITEM_DESC";
                cboProcStopEQP.ValueMember = "ITEM_ID";
                cboProcStopEQP.SelectedIndex = -1;

                #endregion

                #region  Process Pause Command
                //this.cboProcPauseCmdEQP.SelectedIndexChanged -= new System.EventHandler(this.cmbProcPauseCmdEQP_SelectedIndexChanged);
                cboProcPauseCmdEQP.DataSource = _lstNodeNo.ToList();
                cboProcPauseCmdEQP.DisplayMember = "ITEM_DESC";
                cboProcPauseCmdEQP.ValueMember = "ITEM_ID";
                cboProcPauseCmdEQP.SelectedIndex = -1;
                //this.cboProcPauseCmdEQP.SelectedIndexChanged += new System.EventHandler(this.cmbProcPauseCmdEQP_SelectedIndexChanged);
                #endregion

                #region MPLC Interlock
                if (_lstMPLCNodeNo.Count() > 0)
                {
                    gbInterlock.Visible = true;

                    cboInterlockEQP.DataSource = _lstMPLCNodeNo.ToList();
                    cboInterlockEQP.DisplayMember = "ITEM_DESC";
                    cboInterlockEQP.ValueMember = "ITEM_ID";
                    cboInterlockEQP.SelectedIndex = -1;
                }
                else gbInterlock.Visible = false;

                #endregion

                #region cellspecialCheck
                //initial只做Request
                cellSpecialCheckStatus("Request",null);
                #endregion


            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        //20171225 huangjiayin add: cell sepcial check function
        //_cmd: Request; Set(Set opposite value againt now value)
        private void cellSpecialCheckStatus(string _cmd,string _cmdTypeByUI)
        {
            //20171225 huangjiayin add
            try
            {
                Line line = FormMainMDI.G_OPIAp.CurLine;
                if ((line != null && line.FabType == "CELL") || line.LineType == "ELA_JSW")//ELA Port Can Fetch Out Check,Deng,20190828
                {
                    List<string> _checkItems = new List<string>();
                    if(!string.IsNullOrEmpty(_cmdTypeByUI))
                    {
                        _checkItems.Add(_cmdTypeByUI);
                    }
                    else
                    {
                        switch (line.LineType)
                        {
                            //ELA Port Can Fetch Out Check,Deng,20190828
                            case "ELA_JSW":
                                _checkItems.Add("CellSpecialCheck:Port#1PortCanFetchOutCheckFlag");
                                _checkItems.Add("CellSpecialCheck:Port#2PortCanFetchOutCheckFlag");
                                _checkItems.Add("CellSpecialCheck:Port#3PortCanFetchOutCheckFlag");
                                _checkItems.Add("CellSpecialCheck:Port#4PortCanFetchOutCheckFlag");
                                break;

                            case "PIL":
                            case "PIL_2":
                                _checkItems.Add("CellSpecialCheck:PILPairProuctCheckFlag");
                                break;

                            case "ODF":
                            case "ODF_2":
                                _checkItems.Add("CellSpecialCheck:ODFUVMaskCheckFlag");
                                _checkItems.Add("CellSpecialCheck:ODFMAISpecialProcessDataBypassFlag");//Add By Yangzhenteng20191014
                                break;

                            case "CUT_1":
                            case "CUT_2":
                            case "CUT_3":
                            case "CUT_4":
                            case "CUT_5":
                            case "CUT_6":
                            case "CUT_7":
                            case "CUT_8":
                            case "CUT_9":
                            case "CUT_10":
                            case "CUT_11":
                            case "CUT_12":
                                _checkItems.Add("CellSpecialCheck:VCRLossCheckFlag");
                                _checkItems.Add("CellSpecialCheck:RecipeSpecialCheckFlag");//Add By Yangzhenteng20190508
                                _checkItems.Add("CellSpecialCheck:CutRecipeParameterCheckBypassFlag");//Add By Yangzhenteng20190705
                                break;
                            case "QUP":
                                _checkItems.Add("CellSpecialCheck:VCRLossCheckFlag");
                                break;
                            // Add By Yangzhenteng 20181226 For FA Loader Double Check;
                            case "POL_1":
                            case "POL_2":
                                _checkItems.Add("CellSpecialCheck:VCRLossCheckFlag");
                                _checkItems.Add("CellSpecialCheck:LoaderProductCountDoubleCheckFlag");
                                _checkItems.Add("CellSpecialCheck:RecipeSpecialCheckFlag");//Add By Yangzhenteng20190508
                                _checkItems.Add("CellSpecialCheck:SpecialEQPJobGradeUpdate");//Add By Yangzhenteng20190616
                                break;
                            case "PCK":
                                _checkItems.Add("CellSpecialCheck:VCRLossCheckFlag");
                                _checkItems.Add("CellSpecialCheck:LoaderProductCountDoubleCheckFlag");
                                break;
                            default:
                                break;
                        }
                    }

                    if (_checkItems.Count > 0)
                    {
                        //send request message
                        if (_cmd == "Request") dgvCellLineSpecialCheck.Rows.Clear();
                        foreach (string _cmdType in _checkItems)
                        {
                            BCControlCommand _trx = new BCControlCommand();
                            _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                            _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                            _trx.BODY.EQUIPMENTNO = string.Empty;
                            _trx.BODY.UNITNO = string.Empty;
                            _trx.BODY.COMMANDTYPE = _cmdType;
                            _trx.BODY.COMMAND = _cmd;
                            //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;
                            MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);
                            if (_resp == null) continue;
                            //reply
                            if (_cmd.Split(':').Length > 1)
                            {
                                if (_cmd.Split(':')[0] == "Set")
                                {
                                    ShowMessage(this, lblCaption.Text, "", "BC Control Command Change Send to BC Success !", MessageBoxIcon.Information);
                                    cellSpecialCheckStatus("Request", null);
                                    return;
                                    //Set不更新dgv, 用Request单独更新dgv
                                }
                            }
                                //refresh dgvdata...
                                BCControlCommandReply _reply = (BCControlCommandReply)Spec.CheckXMLFormat(_resp.Xml);
                             if(_reply.BODY.EQUIPMENTNO.Split(';').Length==3)
                            {
                                string[] _check = _reply.BODY.EQUIPMENTNO.Split(';');
                                string _checkItem = _check[0];
                                string _checkItemDesc = _check[1];
                                string _checkStatus = _check[2];
                                string _checkbtn = _check[2] == "Enable" ? "Set:Disable" : "Set:Enable";
                                dgvCellLineSpecialCheck.Rows.Add(_checkItem, _checkItemDesc, _checkStatus, _checkbtn);

                                    }

 
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

        private void cboProcPauseCmdEQP_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cboProcPauseCmdUnit.DataSource = null;

                if (cboProcPauseCmdEQP.SelectedIndex == -1) return;

                List<comboxInfo> _lstUnit = new List<comboxInfo>();

                _lstUnit.Add(new comboxInfo { ITEM_ID = "00", ITEM_NAME = "Equipment" });

                foreach (KeyValuePair<string, Unit> unit in FormMainMDI.G_OPIAp.Dic_Unit.Where(d => d.Value.NodeNo.Equals(cboProcPauseCmdEQP.SelectedValue.ToString())))
                {
                    _lstUnit.Add(new comboxInfo { ITEM_ID = unit.Value.UnitNo, ITEM_NAME = unit.Value.UnitID });
                }

                cboProcPauseCmdUnit.DataSource = _lstUnit.ToList();
                cboProcPauseCmdUnit.DisplayMember = "ITEM_DESC";
                cboProcPauseCmdUnit.ValueMember = "ITEM_ID";
                cboProcPauseCmdUnit.SelectedIndex = -1; 
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboProcStopEQP_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cboProcStopUnit.DataSource = null;

                if (cboProcStopEQP.SelectedIndex == -1) return;

                List<comboxInfo> lstUnit = new List<comboxInfo>();

                lstUnit.Add(new comboxInfo { ITEM_ID = "00", ITEM_NAME = "Equipment" });
                foreach (KeyValuePair<string, Unit> unit in FormMainMDI.G_OPIAp.Dic_Unit.Where(d => d.Value.NodeNo.Equals(cboProcStopEQP.SelectedValue.ToString())))
                {
                    lstUnit.Add(new comboxInfo { ITEM_ID = unit.Value.UnitNo, ITEM_NAME = unit.Value.UnitID });
                }
                cboProcStopUnit.DataSource = lstUnit.ToList();
                cboProcStopUnit.DisplayMember = "ITEM_DESC";
                cboProcStopUnit.ValueMember = "ITEM_ID";
                cboProcStopUnit.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboInterlockEQP_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                dgvInterlock.Rows.Clear();
                
                if (cboInterlockEQP.SelectedIndex == -1) return;

                Node _node;
                string _changeStatus = string.Empty;

                if (!FormMainMDI.G_OPIAp.Dic_Node.TryGetValue(cboInterlockEQP.SelectedValue.ToString(), out _node)) return;

                List<InterLock> interlocks = _node.InterLocks;

                foreach (InterLock _lock in interlocks)
                {
                    _changeStatus = (_lock.Status.ToString() == "ON" ? "OFF" : "ON");
                    dgvInterlock.Rows.Add(_lock.PLCTrxNo, _lock.Description, _lock.Status, _changeStatus);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

         private void btnBCControl_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender;
                RadioButton _rdo = null;

                string _nodeNo=string.Empty ;
                string _unitNo = EQP_UNIT_NO;
                string _cmd = string.Empty ;
                string _msg = string.Empty;

                switch (_btn.Tag.ToString())
                {
                    case "PROCESSPAUSE":

                        #region Process Pause
                       
                        if (cboProcPauseCmdEQP.SelectedIndex == -1)
                        {
                            ShowMessage(this, lblCaption.Text , "", "Please choose EQP", MessageBoxIcon.Warning);
                            cboProcPauseCmdEQP.Focus();
                            return;
                        }
                        else _nodeNo = cboProcPauseCmdEQP.SelectedValue.ToString();

                        if (cboProcPauseCmdUnit.SelectedIndex > -1)
                            _unitNo = cboProcPauseCmdUnit.SelectedValue.ToString();

                        if (!rdbProcPauseCmdPause.Checked && !rdbProcPauseCmdResume.Checked)
                        {
                            ShowMessage(this, lblCaption.Text , "", "Please choose Pause or Resume", MessageBoxIcon.Warning);
                            return;
                        }

                        if (rdbProcPauseCmdPause.Checked) _rdo = rdbProcPauseCmdPause;
                        else  _rdo = rdbProcPauseCmdResume;

                        _cmd = _rdo.Tag.ToString();

                        _msg = string.Format("Please confirm whether you will process the Process Pause Command of Node:{0} ,Unit:{1} into [{2}] ?", _nodeNo, _unitNo, _rdo.Text);

                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                        
                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text )) return;

                         #endregion

                        break;

                    case "PROCESSSTOP":

                        #region Process Stop

                        if (cboProcStopEQP.SelectedIndex == -1)
                        {
                            ShowMessage(this, lblCaption.Text , "", "Please choose EQP", MessageBoxIcon.Warning);
                            cboProcStopEQP.Focus();
                            return;
                        }
                        else _nodeNo = cboProcStopEQP.SelectedValue.ToString();

                        if (cboProcStopUnit.SelectedIndex > -1)
                            _unitNo = cboProcStopUnit.SelectedValue.ToString();

                        if (!rdbProcStopStop.Checked && !rdbProcStopRun.Checked)
                        {
                            ShowMessage(this, lblCaption.Text , "", "Please choose Stop or Run", MessageBoxIcon.Warning);
                            return;
                        }

                        if (rdbProcStopStop.Checked) _rdo = rdbProcStopStop;
                        else _rdo = rdbProcStopRun;

                        _cmd = _rdo.Tag.ToString();

                        _msg = string.Format("Please confirm whether you will process the Process Stop Command of Node:{0} ,Unit:{1} into [{2}] ?", _nodeNo, _unitNo, _rdo.Text);

                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;

                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text )) return;

                         #endregion

                        break;

                    case "TRANSFERSTOP":

                        #region Transfer Stop

                        if (cboTransStopEQP.SelectedIndex == -1)
                        {
                            ShowMessage(this, lblCaption.Text , "", "Please choose EQP", MessageBoxIcon.Warning);
                            cboTransStopEQP.Focus();
                            return;
                        }
                        else _nodeNo = cboTransStopEQP.SelectedValue.ToString();

                        if (!rdbTrxStopStop.Checked && !rdbTrxStopResume.Checked)
                        {
                            ShowMessage(this, lblCaption.Text , "", "Please choose Stop or Resume", MessageBoxIcon.Warning);
                            return;
                        }

                        if (rdbTrxStopStop.Checked) _rdo = rdbTrxStopStop;
                        else _rdo = rdbTrxStopResume;

                        _cmd = _rdo.Tag.ToString();

                        _msg = string.Format("Please confirm whether you will process the Transfer Stop Command of Node:{0}  into [{1}] ?", _nodeNo, _rdo.Text);

                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text,  _msg)) return;

                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text )) return;

                         #endregion

                        break;

                    default:
                        return;
                }

                #region send BCControlCommand

                BCControlCommand _trx = new BCControlCommand();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = _nodeNo;
                _trx.BODY.UNITNO = _unitNo;
                _trx.BODY.COMMANDTYPE = _btn.Tag.ToString();
                _trx.BODY.COMMAND = _cmd;

                //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region BCControlCommandReply
                ShowMessage(this, lblCaption.Text, "", "BC Control Command Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

         private void btnBCControlQuery_Click(object sender, EventArgs e)
         {
             try
             {
                 Button _btn = (Button)sender;

                 new FormBCControlLastCommand(_btn.Tag.ToString()) { TopMost = true }.ShowDialog();

             }
             catch (Exception ex)
             {
                 NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                 ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
             }
         }
         private void dgvInterlock_CellClick(object sender, DataGridViewCellEventArgs e)
         {
             try
             {
                 if (cboInterlockEQP.SelectedIndex == -1) return;

                 if (!dgvInterlock.Columns[e.ColumnIndex].Name.Equals(colSet.Name)) return;

                 string _newStatus = string.Empty;
                 string _statusDesc = string.Empty;
                 
                 string _nodeNo = cboInterlockEQP.SelectedValue.ToString();
                 string _plcTrxNo = dgvInterlock.CurrentRow.Cells[colPlcTrx.Name].Value.ToString();
                 string _oldStatus = dgvInterlock.CurrentRow.Cells[colStatus.Name].Value.ToString();
                 string _description = dgvInterlock.CurrentRow.Cells[colDesc.Name].Value.ToString();
                 //_plcTrxNo = _plcTrxNo.PadLeft(2, '0');

                 //if (dgvInterlock.CurrentRow.Cells[colPlcTrx.Name].Value != null)
                 //    _plcTrxNo = dgvInterlock.CurrentRow.Cells[colPlcTrx.Name].Value.ToString();

                 if (_oldStatus.ToUpper().Equals(eInterlockMode.ON.ToString()))
                 {
                     _newStatus = "0";
                     _statusDesc = eInterlockMode.OFF.ToString();
                 }
                 else
                 {
                     _newStatus = "1";
                     _statusDesc = eInterlockMode.ON.ToString();
                 }

                 string msg = string.Format("Please confirm whether you will process the MPLC Interlock of Node:{0} [{1}] into [{2}] ?", _nodeNo,_description, _statusDesc);

                 if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text,  msg)) return;
                 
                 if (DialogResult.Cancel == this.ConfirmPassword(this,lblCaption.Text )) return;

                 #region send MPLCInterlockChangeRequest
                 MPLCInterlockChangeRequest _trx = new MPLCInterlockChangeRequest();
                 _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                 _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                 _trx.BODY.EQUIPMENTNO = _nodeNo;
                 _trx.BODY.MPLCINTERLOCKNO = _plcTrxNo;
                 _trx.BODY.MPLCINTERLOCK = _newStatus;
                 _trx.BODY.PLCTRX = _plcTrxNo;

                 //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                 MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                 if (_resp == null) return;

                 #region MPLCInterlockChangeReply
                 ShowMessage(this, lblCaption.Text, "", "MPLC Interlock Change Send to BC Success !", MessageBoxIcon.Information);
                 #endregion

                 #endregion
             }
             catch (Exception ex)
             {
                 NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                 ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
             }
         }

        //20171227 huangjiayin add
         private void dgvCellSpecialCheck_CellClick(object sender, DataGridViewCellEventArgs e)
         {
             try
             {
                 if (!dgvCellLineSpecialCheck.Columns[e.ColumnIndex].Name.Equals(SetStatus.Name)) return;
                 string _cmdType = "CellSpecialCheck:" + dgvCellLineSpecialCheck.CurrentRow.Cells[CheckItem.Name].Value.ToString();
                 string _cmd = dgvCellLineSpecialCheck.CurrentRow.Cells[SetStatus.Name].Value.ToString();
                 //Allow User Setting...
                 switch (_cmdType)
                 {
                       //杨镇滕/胡均朋/朱兴星/张岩/Unicom 20190425;
                     case "CellSpecialCheck:ODFUVMaskCheckFlag":
                         string[] allowUsers = { "43965", "16767", "30381", "31582", "Unicom" };
                         if (!allowUsers.Contains(FormMainMDI.G_OPIAp.LoginUserID))
                         {
                             ShowMessage(this, MethodBase.GetCurrentMethod().Name, new Exception("Operation Not Allowed!"), MessageBoxIcon.Error);
                             return;
                         }
                         break;

                         //CIMAdmin
                     default:
                         /*
                         if (FormMainMDI.G_OPIAp.LoginGroupID!="CIMAdmin")
                         {
                             ShowMessage(this, MethodBase.GetCurrentMethod().Name, new Exception("Operation Not Allowed!"), MessageBoxIcon.Error);
                             return;
                         }
                          * */
                         break;
                 }
                 string msg = string.Format("Please confirm whether you will process the {0} into [{1}] ?", _cmdType,_cmd);

                 if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;

                 if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;
                 cellSpecialCheckStatus(_cmd, _cmdType);



             }
             catch (Exception ex)
             {
                 NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                 ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
             }
 
         }

         private void RefreshData()
         {
             try
             {
                 if (cboInterlockEQP.SelectedIndex == -1) return;
                 if (dgvInterlock.Rows.Count <= 0) return;

                 Node _node;

                 string _nodeNo = cboInterlockEQP.SelectedValue.ToString();
                 if (!FormMainMDI.G_OPIAp.Dic_Node.TryGetValue(_nodeNo, out _node)) return;

                 string _plcTrxNo = string.Empty;
                 foreach (DataGridViewRow _row in dgvInterlock.Rows)
                 {
                     _plcTrxNo = _row.Cells[colPlcTrx.Name].Value.ToString();
                     //_plcTrxNo = _plcTrxNo.PadLeft(2, '0');

                     InterLock _lock = _node.InterLocks.Find(i => i.PLCTrxNo == _plcTrxNo);

                     if (_lock == null) continue;

                     if (_lock.Status.ToString() != _row.Cells[colStatus.Name].Value.ToString())
                     {
                         _row.Cells[colStatus.Name].Value = _lock.Status.ToString();
                         _row.Cells[colSet.Name].Value = (_lock.Status.ToString() == "ON" ? "OFF" : "ON");
                     }
                 }
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
                 string _err = string.Empty;

                 if (cboInterlockEQP.SelectedIndex == -1) return;

                 Node _node;

                 if (!FormMainMDI.G_OPIAp.Dic_Node.TryGetValue(cboInterlockEQP.SelectedValue.ToString(), out _node)) return;

                 SendtoBC_EquipmentStatusRequest(_node.NodeNo);
             }
             catch (Exception ex)
             {
                 NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                 ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
             }
         }

         private void SendtoBC_EquipmentStatusRequest(string LocalNo)
         {
             try
             {
                 string _err = string.Empty;

                 #region Send to BC EquipmentStatusRequest

                 EquipmentStatusRequest _trx = new EquipmentStatusRequest();
                 _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                 _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                 _trx.BODY.EQUIPMENTNO = LocalNo;
                 string _xml = _trx.WriteToXml();

                 FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                 ShowMessage(this, lblCaption.Text, "", "Reload Equipment Information  Send to BC Success !", MessageBoxIcon.Information);  

                 #endregion
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

                 RefreshData();
             }
             catch (Exception ex)
             {
                 NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                 ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
             }
         }

    }
}
