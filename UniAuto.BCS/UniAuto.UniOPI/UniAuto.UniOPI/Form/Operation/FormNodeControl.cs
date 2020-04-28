using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;


namespace UniOPI
{
    public partial class FormNodeControl : FormBase
    {
        #region Defind

        private Dictionary<string, DateTime> dicStartDateTime = new Dictionary<string, DateTime>();
        private List<int> timeCalibrationDisableButtonRow = new List<int>();
        #endregion
        
        Node CurNode = null;

        public FormNodeControl()
        {
            InitializeComponent();
        }

        private void FormNodeControl_Load(object sender, EventArgs e)
        {
            try
            {
                //#region Unit Run Mode
                //if (FormMainMDI.G_OPIAp.CurLine.LineUnitRunModes.Count > 0)
                //{
                //    colNewUnitRunMode.DataSource = FormMainMDI.G_OPIAp.CurLine.LineUnitRunModes.ToList();
                //    colNewUnitRunMode.DisplayMember = "RunModeDesc";
                //    colNewUnitRunMode.ValueMember = "RunModeNo";
                //}
                //#endregion

                foreach ( Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                {
                    dgvNodeData.Rows.Add(_node.NodeNo, _node.NodeID, _node.CIMMode.ToString(), _node.UpStreamInlineMode, _node.DownStreamInlineMode, _node.RecipeName,
                        _node.EQPRunMode,_node.OperationMode, _node.InspectionIdleTime.ToString(),_node.APCReport.ToString(),_node.APCReportTime.ToString(),
                        _node.EnergyReport.ToString(),_node.EnergyReportTime.ToString(),_node.OxinfoCheckMode.ToString()); //add Oxinfo check mode By   qiumin 20180607
                }

                LoadNodeCommand();

                InitialForm();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        public void InitialForm()
        {
            try
            {
                if (cboCIMMode_New.Items.Count > 0) cboCIMMode_New.SelectedIndex = -1;
                if (cboRunMode_New.Items.Count > 0) cboRunMode_New.SelectedIndex = -1;
                txtInspIdleTime_New.Text = string.Empty;
                txtAPCReport_New.Text = string.Empty;
                txtEnergyReport_New.Text = string.Empty;
                chkAPCReport_New.Checked = false;
                chkEnergyReport_New.Checked = false;

                CurNode = FormMainMDI.G_OPIAp.Dic_Node[dgvNodeData.CurrentRow.Cells[colLocalNo.Name].Value.ToString()];

                #region Inspection Idle Time Setting -- Node Attribute = "IN"表示為inspection 機台
                if (CurNode.NodeAttribute.Equals("IN") || (CurNode.NodeID.Contains("TC") && CurNode.NodeNo != "L2"))// add by qiumin 20180106 Array opi add EqProcessTimeSetting function ,both with inspection ilde time set use same button
                {
                    pnlInspIdleTime_New.Visible = true;
                    pnlInspIdleTime.Visible = true;
                    dgvNodeData.Columns[colInspIdleTime.Name].Visible = true;
                }
                else if (CurNode.NodeID.Contains("CCSDP") || CurNode.NodeID.Contains("CCLCD")||CurNode.NodeID.Contains("CCSLI"))//ADD BY hujunpeng 20181207 for odf add eqprocesstimesetting
                {
                    pnlInspIdleTime_New.Visible = true;
                    pnlInspIdleTime.Visible = true;
                    dgvNodeData.Columns[colInspIdleTime.Name].Visible = true;
                }
                else
                {
                    pnlInspIdleTime_New.Visible = false;
                    pnlInspIdleTime.Visible = false;
                    dgvNodeData.Columns[colInspIdleTime.Name].Visible = false;
                }
                #endregion
                // add by qiu 20180508 
                #region   OXINFO check button 只有CF REP 和MAC有
                if ((CurNode.NodeID.Contains("FCREP") || CurNode.NodeID.Contains("FCQMA"))&&CurNode.NodeNo !="L2")
                {
                    pnlOXINFOCHECK_NEW.Visible = true;

                }
                else
                {
                    pnlOXINFOCHECK_NEW.Visible = false;
                }
                #endregion

                #region Run Mode Setting --- RunModeUse當為Y 表示可切換run mode
                if (CurNode.UseRunMode == "Y")
                {
                    pnlRunMode.Visible = true;
                    dgvNodeData.Columns[colRunMode.Name].Visible = true;
                    pnlRunMode_New.Visible = true;

                    #region Run Mode
                    cboRunMode_New.SelectedValueChanged -= new EventHandler(cboRunMode_New_SelectedValueChanged);
                    cboRunMode_New.DataSource = CurNode.LineRunModes.ToList();
                    cboRunMode_New.DisplayMember = "RunModeDesc";
                    cboRunMode_New.ValueMember = "RunModeNo";
                    cboRunMode_New.SelectedIndex = -1;
                    cboRunMode_New.SelectedValueChanged += new EventHandler(cboRunMode_New_SelectedValueChanged);
                    #endregion
                }
                else
                {
                    pnlRunMode_New.Visible = false;

                    if (CurNode.UseRunMode == "R")
                    {
                        pnlRunMode.Visible = true;
                        dgvNodeData.Columns[colRunMode.Name].Visible = true;
                    }
                    else
                    {
                        pnlRunMode.Visible = false;
                        dgvNodeData.Columns[colRunMode.Name].Visible = false;
                    }
                }

                #region 判斷是否有unit run mode
                var _var = FormMainMDI.G_OPIAp.Dic_Unit.Values.Where(r => r.NodeNo.Equals(CurNode.NodeNo) && r.UseRunMode.Equals("Y")).ToList();

                if (_var.Count() == 0)
                {
                    pnlRunMode_New.Height = 28;

                    dgvUnitRunMode.Rows.Clear();
                    dgvUnitRunMode.Visible = false;
                }
                else
                {
                    pnlRunMode_New.Height = 155;

                    dgvUnitRunMode.Rows.Clear();
                    dgvUnitRunMode.Visible = true;

                    #region Unit Run Mode
                    if (CurNode.LineUnitRunModes.Count > 0)
                    {
                        colNewUnitRunMode.DataSource = CurNode.LineUnitRunModes.ToList();
                        colNewUnitRunMode.DisplayMember = "RunModeDesc";
                        colNewUnitRunMode.ValueMember = "RunModeNo";
                    }
                    #endregion

                    foreach (Unit _unit in _var)
                    {
                        dgvUnitRunMode.Rows.Add(_unit.UnitNo.ToString().PadLeft(2, '0'), string.Format("{0}-{1}", _unit.UnitNo, _unit.UnitID), _unit.UnitRunMode);
                    }
                }
                #endregion

                #endregion

                #region 以Node的report mode只要HSMS_xxx開頭的沒有CIM Mode切換功能 --- by 玉明 2015.09.01
                if (CurNode.ReportMode.Length > 5 && CurNode.ReportMode.Substring(0,5) =="HSMS_")
                    pnlCIMMode_New.Visible = false;
                else pnlCIMMode_New.Visible = true;
                #endregion

                lblVCRCaption.Text = string.Format("{0} - {1} VCR Status", CurNode.NodeNo, CurNode.NodeID);
                RefreshData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void LoadNodeCommand()
        {
            try
            {
                #region CIM Mode
                DataTable _dtCIMMode = UniTools.InitDt(new string[] { "NEW_CIM_MODE", "NEW_CIM_MODE_DESC" });
                foreach (string _name in Enum.GetNames(typeof(eCIMMode)))
                {
                    int _value = (int)Enum.Parse(typeof(eCIMMode), _name);

                    eCIMMode _cimMode = (eCIMMode)Enum.Parse(typeof(eCIMMode), _name);

                    string desc = Public.GetEnumDesc(_cimMode);
                    DataRow drNew = _dtCIMMode.NewRow();
                    drNew["NEW_CIM_MODE"] = _value;
                    drNew["NEW_CIM_MODE_DESC"] = string.Format("{0}:{1}", _value, desc);
                    _dtCIMMode.Rows.Add(drNew);
                }

                cboCIMMode_New.DataSource = _dtCIMMode;                
                cboCIMMode_New.ValueMember = "NEW_CIM_MODE";
                cboCIMMode_New.DisplayMember = "NEW_CIM_MODE_DESC";
                cboCIMMode_New.SelectedIndex = -1;
                #endregion

                #region Run Mode
                //cboRunMode_New.SelectedValueChanged -= new EventHandler(cboRunMode_New_SelectedValueChanged);
                //cboRunMode_New.DataSource = FormMainMDI.G_OPIAp.CurLine.LineRunModes.ToList();
                //cboRunMode_New.DisplayMember = "RunModeDesc";
                //cboRunMode_New.ValueMember = "RunModeNo";
                //cboRunMode_New.SelectedIndex = -1;
                //cboRunMode_New.SelectedValueChanged += new EventHandler(cboRunMode_New_SelectedValueChanged); 
                #endregion 
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ShowInfo(DataGridViewRow dgvRow)
        {
            try
            {
                if (dgvRow == null) return;                

                txtLocalNo.Text = dgvRow.Cells[colLocalNo.Name].Value.ToString();
                txtCIMMode.Text = dgvRow.Cells[colCIMMode.Name].Value.ToString();
                txtUPMode.Text = dgvRow.Cells[colUpLineMode.Name].Value.ToString();
                txtDNMode.Text = dgvRow.Cells[colDnLineMode.Name].Value.ToString();
                txtRecipeName.Text = dgvRow.Cells[colRecipeName.Name].Value.ToString();
                txtRunMode.Text = dgvRow.Cells[colRunMode.Name].Value.ToString();
                txtOperaionMode.Text = dgvRow.Cells[colOperationMode.Name].Value.ToString();
                txtInspIdleTime.Text = dgvRow.Cells[colInspIdleTime.Name].Value.ToString();
                txtAPCReport.Text = dgvRow.Cells[colAPCReportTime.Name].Value.ToString();
                txtEnergyReport.Text = dgvRow.Cells[colEnergyReportTime.Name].Value.ToString();
                chkAPCReport.Checked = Convert.ToBoolean(dgvRow.Cells[colAPCReport.Name].Value.ToString());
                chkEnergyReport.Checked = Convert.ToBoolean(dgvRow.Cells[colEnergyReport.Name].Value.ToString());
                txtOxinfoCheck.Text = dgvRow.Cells[colOxinfoCheckMode.Name].Value.ToString ();
                #region VCR Data
                if (dgvVCRData.Tag.ToString() != CurNode.NodeNo)
                {
                    dgvVCRData.Rows.Clear();

                    foreach (VCR _vcr in CurNode.VCRs)
                    {
                        string _set = string.Empty;
                        if (_vcr.Status == eVCRMode.ENABLE) _set = "Set Disable";
                        else _set = "Set Enable";

                        dgvVCRData.Rows.Add(CurNode.NodeNo, _vcr.VCRNO, _vcr.Status, _set);                        
                    }

                    dgvVCRData.Tag = CurNode.NodeNo;
                }
                else
                {
                    foreach (DataGridViewRow _row in dgvVCRData.Rows)
                    {
                        string _vcrNo = _row.Cells[colVCRNo.Name].Value.ToString();

                        VCR _vcr = CurNode.VCRs.Find(v => v.VCRNO.Equals(_vcrNo));

                        if (_vcr == null) continue;

                        if (_row.Cells[colVCREnable.Name].Value.ToString() == _vcr.Status.ToString()) continue ;

                        _row.Cells[colVCREnable.Name].Value = _vcr.Status.ToString();

                        if (_vcr.Status == eVCRMode.ENABLE)
                            _row.Cells[colVCREnable_New.Name].Value = "Set Disable";
                        else _row.Cells[colVCREnable_New.Name].Value = "Set Enable";
                    }
                }
                #endregion

                #region Unit Run Mode
                if (dgvUnitRunMode.Visible)
                {
                    string _unitNo=string.Empty ;

                    foreach (DataGridViewRow _row in dgvUnitRunMode.Rows)
                    {
                        _unitNo = _row.Cells[colUnitNo.Name].Value.ToString();

                        var _var = FormMainMDI.G_OPIAp.Dic_Unit.Values.Where(r => r.NodeNo.Equals(CurNode.NodeNo) && r.UseRunMode.Equals("Y") && r.UnitNo.Equals(_unitNo)).ToList();

                        if (_var.Count() == 0) continue;

                        _row.Cells[colCurrentUnitRunMode.Name].Value = ((Unit)_var.First()).UnitRunMode;
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

        private void RefreshData()
        {
            try
            {
                string _localNo = string.Empty;
                Node _node = null;

                #region Node Information
                foreach (DataGridViewRow dgvRow in dgvNodeData.Rows)
                {
                    _localNo = dgvRow.Cells[colLocalNo.Name].Value.ToString();

                    _node = FormMainMDI.G_OPIAp.Dic_Node[_localNo];

                    dgvRow.Cells[colLocalNo.Name].Value = _localNo;
                    dgvRow.Cells[colCIMMode.Name].Value = _node.CIMMode.ToString();
                    dgvRow.Cells[colUpLineMode.Name].Value = _node.UpStreamInlineMode;
                    dgvRow.Cells[colDnLineMode.Name].Value = _node.DownStreamInlineMode;
                    dgvRow.Cells[colRecipeName.Name].Value = _node.RecipeName;
                    dgvRow.Cells[colRunMode.Name].Value = _node.EQPRunMode;
                    dgvRow.Cells[colOperationMode.Name].Value = _node.OperationMode;
                    dgvRow.Cells[colInspIdleTime.Name].Value = _node.InspectionIdleTime.ToString();
                    dgvRow.Cells[colAPCReport.Name].Value = _node.APCReport.ToString();
                    dgvRow.Cells[colAPCReportTime.Name].Value = _node.APCReportTime.ToString();
                    dgvRow.Cells[colEnergyReport.Name].Value = _node.EnergyReport.ToString();
                    dgvRow.Cells[colEnergyReportTime.Name].Value = _node.EnergyReportTime.ToString();
                    dgvRow.Cells[colOxinfoCheckMode.Name].Value = _node.OxinfoCheckMode.ToString();
                }
                ShowInfo(dgvNodeData.CurrentRow);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_CIMModeChangeRequest()
        {
            try
            {
                #region Check Data

                if (cboCIMMode_New.SelectedValue == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose the New Mode", MessageBoxIcon.Error);
                    return;
                }

                string _newMode = cboCIMMode_New.SelectedValue.ToString();
                string _newModeDesc = ((eCIMMode)(int.Parse(_newMode))).ToString();


                #endregion

                string _msg = string.Format("Please confirm whether you will change the CIM Mode of Node:{0}  into [{1}] ?", CurNode.NodeNo, _newModeDesc);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region send CIMModeChangeRequest

                CIMModeChangeRequest _trx = new CIMModeChangeRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                _trx.BODY.EQUIPMENTNO = CurNode.NodeNo;
                _trx.BODY.EQUIPMENTID = CurNode.NodeID;
                _trx.BODY.CIMMODE = _newMode;

                //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region CIMModeChangeReply
                ShowMessage(this, lblCaption.Text, "", "CIM Mode Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_EquipmentRunModeSetCommand()
        {
            try
            {
                #region check Data
                if (cboRunMode_New.SelectedValue == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose the New Run Mode", MessageBoxIcon.Error);
                    return;
                }

                string _newRunMode = cboRunMode_New.SelectedValue.ToString();
                string _newRunModeDesc = cboRunMode_New.Text;

                if (_newRunMode == txtRunMode.Text.ToString())
                {
                    ShowMessage(this, lblCaption.Text, "", "The Current Run Mode and New Run Mode must be different!!", MessageBoxIcon.Error);
                    return;
                }
                #endregion

                string _msg = string.Format("Please confirm whether you will change the Run Mode of Node:{0}  into [{1}] ?", CurNode.NodeNo, _newRunModeDesc);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region send EquipmentRunModeSetCommand
                EquipmentRunModeSetCommand _trx = new EquipmentRunModeSetCommand();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurNode.NodeNo;
                _trx.BODY.COMMAND = _newRunMode;

                //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region EquipmentRunModeSetCommandReply
                ShowMessage(this, lblCaption.Text, "", "Equipment Run Mode Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_EquipmentUnitRunModeSetCommand()
        {
            try
            {
                #region check Data
                if (cboRunMode_New.SelectedValue == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose the New Run Mode", MessageBoxIcon.Error);
                    return;
                }

                foreach (DataGridViewRow _row in dgvUnitRunMode.Rows)
                {
                    if (_row.Cells[colNewUnitRunMode.Name].Value == null || _row.Cells[colNewUnitRunMode.Name].Value.ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please choose the New Unit Run Mode", MessageBoxIcon.Error);
                        return;
                    }
                }
                #endregion

                string _newRunMode = cboRunMode_New.SelectedValue.ToString();
                string _newRunModeDesc = cboRunMode_New.Text;

                string _msg = string.Format("Please confirm whether you will change the Run Mode of Node:{0}  into [{1}] ?", CurNode.NodeNo, _newRunModeDesc);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region send UnitRunModeChangeRequest
                UnitRunModeChangeRequest _trx = new UnitRunModeChangeRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurNode.NodeNo;
                _trx.BODY.NEW_RUNMODE = _newRunMode;

                UnitRunModeChangeRequest.UNITc _unit;

                int _no = 0;

                foreach (DataGridViewRow _row in dgvUnitRunMode.Rows)
                {
                    int.TryParse(_row.Cells[colUnitNo.Name].Value.ToString(),out _no);

                    _unit = new UnitRunModeChangeRequest.UNITc();
                    _unit.UNITNO = _no.ToString();
                    _unit.NEW_RUNMODE = _row.Cells[colNewUnitRunMode.Name].Value.ToString();

                    _trx.BODY.UNITLIST.Add(_unit);
                }


                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region EquipmentRunModeSetCommandReply
                ShowMessage(this, lblCaption.Text, "", "Equipment Run Mode Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_DateTimeCalibrationRequest(List<string> lstEqpNo)
        {
            try
            {
                if (lstEqpNo == null || lstEqpNo.Count == 0) return;

                string _msg = string.Empty;

                if (lstEqpNo.Count == 1)
                {
                    _msg = string.Format("Please confirm whether you will calibrate the Time of Node:{0}?", lstEqpNo[0]);
                }
                else
                {
                    _msg = "Please confirm whether you will calibrate the Time for all EQP ?";
                }

                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;

                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                List<DateTimeCalibrationRequest.EQUIPMENTc> _lstReqEqp = new List<DateTimeCalibrationRequest.EQUIPMENTc>();

                foreach (string eqpNo in lstEqpNo)
                {
                    DateTimeCalibrationRequest.EQUIPMENTc eqp = new DateTimeCalibrationRequest.EQUIPMENTc();
                    eqp.EQUIPMENTNO = eqpNo;
                    _lstReqEqp.Add(eqp);
                }

                DateTimeCalibrationRequest _trx = new DateTimeCalibrationRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                _trx.BODY.EQUIPMENTLIST = _lstReqEqp;

                //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;
                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region DateTimeCalibrationReply
                ShowMessage(this, lblCaption.Text, "", "DateTime Calibration Request Send to BC Success !", MessageBoxIcon.Information);
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_InspectionIdleTimeRequest()
        {
            try
            {
                string _idleTime = txtInspIdleTime_New.Text.ToString();

                #region check Data
                if (_idleTime == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please input Inspection Idle Time", MessageBoxIcon.Error);
                    txtInspIdleTime_New.Focus();
                    return;
                }

               /* if (_idleTime == txtInspIdleTime.Text.ToString())
                {
                    ShowMessage(this, lblCaption.Text, "", "The Current Inspection Idle Time and New Inspection Idle Time must be different!!", MessageBoxIcon.Error);
                    txtInspIdleTime_New.Focus();
                    return;
                }  modify qiumin 20180112 cancel time same check
                 */
                #endregion

                string msg = string.Format("Please confirm whether you will process the Inspection Idle Time Setting of Node:{0} into [{1}] ?", CurNode.NodeNo, _idleTime);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region send InspectionIdleTimeRequest
                InspectionIdleTimeRequest _trx = new InspectionIdleTimeRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurNode.NodeNo;
                _trx.BODY.IDLETIME = _idleTime;
                _trx.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;

                //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region InspectionIdleTimeReply
                ShowMessage(this, lblCaption.Text, "", "Inspection Idle Time Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //add by qiumin 20180508 for OXINFO CHECK FLAG setting button
        private void Send_OxinfoCheck(string btn_tag)
        {
            try
            {
                string _enable = chkOXINFOCHECK_NEW.Checked ? "Y" : "N";

                #region send EquipmentReportSettingRequest
                EquipmentReportSettingRequest _trx = new EquipmentReportSettingRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurNode.NodeNo;
                _trx.BODY.REPORTTYPE = "OXINFO";
                _trx.BODY.REPORTTIME = "60";  //无用，借用APC的message 格式
                _trx.BODY.REPORTENABLE = _enable;

                //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;
                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region EquipmentReportSettingRequestReply
                ShowMessage(this, lblCaption.Text, "", "Oxinfo Check Setting Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_APCReportRequest()
        {
            try
            {
                string _apcTimer = txtAPCReport_New.Text.ToString();
                string _enable = chkAPCReport_New.Checked ? "Y" : "N";

                int _num = 0;

                int.TryParse(_apcTimer, out _num);

                #region check Data
                if (_apcTimer == txtAPCReport.Text.ToString() && chkAPCReport.Checked == chkAPCReport_New.Checked)
                {
                    ShowMessage(this, lblCaption.Text, "", "The Current APC Report Setting and New APC Report Setting must be different!!", MessageBoxIcon.Error);
                    txtAPCReport_New.Focus();
                    return;
                }

                //100~86400000 minutes
                if (_num > 86400000 || _num < 100)
                {
                    if (_enable == "N" && _num == 0) { }
                    else
                    {
                        ShowMessage(this, lblCaption.Text, "", " 100 <= Report Time <= 86400000", MessageBoxIcon.Error);

                        txtAPCReport_New.Focus();

                        return;
                    }
                }
                #endregion

                    string msg = string.Format("Please confirm whether you will process the APC Report Setting of Node:{0} ?", CurNode.NodeNo);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region update SBRM_NODE APC Report
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_NODE _sbrmNode = null;

                var _var = from _d in ctxBRM.SBRM_NODE
                           where _d.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName &&
                                 _d.NODENO == CurNode.NodeNo
                           select _d;

                if (_var.Count() > 0)
                {
                    _sbrmNode = _var.ToList()[0];

                    _sbrmNode.APCREPORT = _enable;
                    _sbrmNode.APCREPORTTIME = _num; // int.Parse(_apcTimer);

                    ctxBRM.SubmitChanges();

                    Node _Node = FormMainMDI.G_OPIAp.Dic_Node[_sbrmNode.NODENO];

                    _Node.APCReport = _sbrmNode.APCREPORT == "Y" ? true : false;
                    _Node.APCReportTime = _sbrmNode.APCREPORTTIME;
                }
                #endregion

                #region send EquipmentReportSettingRequest
                EquipmentReportSettingRequest _trx = new EquipmentReportSettingRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurNode.NodeNo;
                _trx.BODY.REPORTTYPE = "APC";
                _trx.BODY.REPORTTIME = _apcTimer;
                _trx.BODY.REPORTENABLE = _enable;

                //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;
                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region EquipmentReportSettingRequestReply
                ShowMessage(this, lblCaption.Text, "", "APC Report Setting Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion

                #endregion

                ReloadReportData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_EnergyReportRequest(string btn_tag)
        {
            try
            {
                string _energyTimer = txtEnergyReport_New.Text.ToString();
                string _enable = chkEnergyReport_New.Checked ? "Y" : "N";

                int _num = 0;

                int.TryParse(_energyTimer, out _num);

                #region check Data
                if (_energyTimer == txtEnergyReport.Text.ToString() && chkEnergyReport.Checked == chkEnergyReport_New.Checked && btn_tag == "EnergyReport")
                {
                    ShowMessage(this, lblCaption.Text, "", "The Current Energy Report Setting and New Energy Report Setting must be different!!", MessageBoxIcon.Error);
                    txtEnergyReport_New.Focus();
                    return;
                }

                //100~86400000 minutes
                if (_num > 86400000 || _num < 100)
                {
                    if (_enable == "N" && _num == 0) { }
                    else
                    {
                        ShowMessage(this, lblCaption.Text, "", " 100 <= Report Time <= 86400000", MessageBoxIcon.Error);

                        txtEnergyReport_New.Focus();

                        return;
                    }
                }

                #endregion

                string msg = btn_tag == "EnergyReport" ? string.Format("Please confirm whether you will process the Energy Report Setting of Node:{0} ?", CurNode.NodeNo) : "Please confirm whether you will process the Energy Report Setting of All EQP ?";
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region update SBRM_NODE Energy Report
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_NODE _sbrmNode = null;

                var _var =btn_tag == "EnergyReport" ?( from _d in ctxBRM.SBRM_NODE
                           where _d.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName &&
                                 _d.NODENO == CurNode.NodeNo
                                                       select _d) : (from _d in ctxBRM.SBRM_NODE
                                                                     where _d.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName 
                                                                     select _d);

                foreach (SBRM_NODE nd in _var.ToList())//20170214: huangjiayin add setall EQP Function
                {
                    _sbrmNode = nd;

                    _sbrmNode.ENERGYREPORT = _enable;
                    _sbrmNode.ENERGYREPORTTIME = _num; // int.Parse(_energyTimer);

                    ctxBRM.SubmitChanges();

                    Node _Node = FormMainMDI.G_OPIAp.Dic_Node[nd.NODENO];

                    _Node.EnergyReport = chkEnergyReport_New.Checked;
                    _Node.EnergyReportTime = _sbrmNode.ENERGYREPORTTIME;


                #endregion

                    #region send EquipmentReportSettingRequest
                    EquipmentReportSettingRequest _trx = new EquipmentReportSettingRequest();
                    _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    _trx.BODY.EQUIPMENTNO = nd.NODENO;
                    _trx.BODY.REPORTTYPE = "ENERGY";
                    _trx.BODY.REPORTTIME = _energyTimer;
                    _trx.BODY.REPORTENABLE = _enable;

                    //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                    MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                    if (_resp == null) return;
                }

                #region EquipmentReportSettingRequestReply
                ShowMessage(this, lblCaption.Text, "", "Energy Report Setting Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion

                #endregion

                ReloadReportData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvNodeData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            InitialForm();
        }

        private void dgvUnitRunMode_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            try
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, anError.Exception.ToString());
                MessageBox.Show("dgvProduct Error happened " + anError.Context.ToString());

                if (anError.Context == DataGridViewDataErrorContexts.Commit)
                {
                    MessageBox.Show("Commit error");
                }
                if (anError.Context == DataGridViewDataErrorContexts.CurrentCellChange)
                {
                    MessageBox.Show("Cell change");
                }
                if (anError.Context == DataGridViewDataErrorContexts.Parsing)
                {
                    MessageBox.Show("parsing error");
                }
                if (anError.Context == DataGridViewDataErrorContexts.LeaveControl)
                {
                    MessageBox.Show("leave control error");
                }

                if ((anError.Exception) is ConstraintException)
                {
                    DataGridView view = (DataGridView)sender;
                    view.Rows[anError.RowIndex].ErrorText = "an error";
                    view.Rows[anError.RowIndex].Cells[anError.ColumnIndex].ErrorText = "an error";

                    anError.ThrowException = false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvVCRData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (!dgvVCRData.Columns[e.ColumnIndex].Name.Equals(colVCREnable_New.Name)) return;

                string _vcrNo = dgvVCRData.CurrentRow.Cells[colVCRNo.Name].Value.ToString();
                string _enableMode = dgvVCRData.CurrentRow.Cells[colVCREnable.Name].Value.ToString();
                string newVCRMode = string.Empty;
                string newVCRModeDesc = string.Empty;

                if (_enableMode.ToUpper().Equals(eVCRMode.ENABLE.ToString()))
                {
                    newVCRMode = "0";
                    newVCRModeDesc = eVCRMode.DISABLE.ToString();
                }
                else
                {
                    newVCRMode = "1";
                    newVCRModeDesc = eVCRMode.ENABLE.ToString();
                }

                string msg = string.Format("Please confirm whether you will process the VCR Enable of Node:{0},VCR No:{1} into [{2}] ?", CurNode.NodeNo, _vcrNo, newVCRModeDesc);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region Send to BC VCRStatusChangeRequest
                VCRStatusChangeRequest _trx = new VCRStatusChangeRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurNode.NodeNo;
                _trx.BODY.VCRNO = _vcrNo;
                _trx.BODY.VCRMODE = newVCRMode;

                //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region VCRStatusChangeReply
                ShowMessage(this, lblCaption.Text, "", "VCR Status Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ReloadReportData()
        {
            try
            {

                #region Reload APC / Energy Information
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var rstNODE = ctxBRM.SBRM_NODE.Where(r => r.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName).OrderBy(r => r.RECIPEIDX);

                foreach (SBRM_NODE _db in rstNODE)
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_db.NODENO))
                    {
                        Node _Node = FormMainMDI.G_OPIAp.Dic_Node[_db.NODENO];

                        _Node.APCReport = _db.APCREPORT == "Y" ? true : false;
                        _Node.EnergyReport = _db.ENERGYREPORT == "Y" ? true : false;
                        _Node.APCReportTime = _db.APCREPORTTIME;
                        _Node.EnergyReportTime = _db.ENERGYREPORTTIME;
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                string _err = string.Empty;

                #region Reload APC / Energy Information
                ReloadReportData();
                //UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //var rstNODE = ctxBRM.SBRM_NODE.Where(r => r.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName).OrderBy(r => r.RECIPEIDX);

                //foreach (SBRM_NODE _db in rstNODE)
                //{
                //    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_db.NODENO))
                //    {
                //        Node _Node = FormMainMDI.G_OPIAp.Dic_Node[_db.NODENO];

                //        _Node.APCReport = _db.APCREPORT == "Y" ? true : false;
                //        _Node.EnergyReport = _db.ENERGYREPORT == "Y" ? true : false;
                //        _Node.APCReportTime = _db.APCREPORTTIME;
                //        _Node.EnergyReportTime = _db.ENERGYREPORTTIME;
                //    }
                //}
                #endregion

                AllEquipmentStatusRequest _trx = new AllEquipmentStatusRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), out _err, FormMainMDI.G_OPIAp.SessionID);

                ShowMessage(this, lblCaption.Text, "", "Reload Equipment Information  Send to BC Success !", MessageBoxIcon.Information);                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender;

                switch (_btn.Tag.ToString())
                {
                    case "CIMMode":

                        Send_CIMModeChangeRequest();

                        break;

                    case "RunMode":

                        if (dgvUnitRunMode.Visible)
                        {
                            Send_EquipmentUnitRunModeSetCommand();
                        }
                        else
                        {
                            Send_EquipmentRunModeSetCommand();
                        }

                        break;

                    case "InspectionIdleTime":

                        Send_InspectionIdleTimeRequest();

                        break;

                    case "CalibrationForEQP":

                         List<string> _eqp = new List<string>();

                         _eqp.Add(CurNode.NodeNo);
                        
                         Send_DateTimeCalibrationRequest(_eqp);
                        
                        break;

                    case "CalibrationForAll":

                         List<string> _eqpAll = new List<string>();

                         foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                         {
                             _eqpAll.Add(_node.NodeNo);
                         }

                         Send_DateTimeCalibrationRequest(_eqpAll);
                        
                        break;

                    case "APCReport":

                        Send_APCReportRequest();

                        break;

                    case "EnergyReport":
                    case "EnergyReport_All"://20170214: huangjiayin add energy setting for all eqp

                        Send_EnergyReportRequest(_btn.Tag.ToString());

                        break;
                    case "OXINFOCheck":

                        Send_OxinfoCheck(_btn.Tag.ToString());

                        break;

                    default: return;                       
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                asciiCode == 8)   // Backspace
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void txtInspIdleTime_New_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                if (_txt.Text.ToString() == string.Empty) return;

                int _num = 0;

                int.TryParse(_txt.Text.ToString(), out _num);


                if (_num > 65535 || _num <0)
                {
                    ShowMessage(this, lblCaption.Text , "", "0<= Inspection Idle Time <= 65535", MessageBoxIcon.Error);

                    _txt.Text = string.Empty;

                    _txt.Focus();

                    return;
                }
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

                tmrBaseRefresh.Enabled = false;
            }
        }

        private void cboRunMode_New_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboRunMode_New.SelectedValue == null) return;
                if (dgvUnitRunMode.Visible == false) return;

                string _value = cboRunMode_New.SelectedValue.ToString();

                if (CurNode.AllowUnitRunMode.Contains(_value))
                {
                    foreach (DataGridViewRow _row in dgvUnitRunMode.Rows)
                    {
                        _row.Cells[colNewUnitRunMode.Name].Value = string.Empty;
                        _row.Cells[colNewUnitRunMode.Name].ReadOnly = false;
                    }
                }
                else
                {
                    foreach (DataGridViewRow _row in dgvUnitRunMode.Rows)
                    {
                        _row.Cells[colNewUnitRunMode.Name].Value = _value;
                        _row.Cells[colNewUnitRunMode.Name].ReadOnly = true;
                    }
                }


            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtCstSeqNO_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            } 
        }

        private void btnFileDataCreate_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtCstSeqNO.Text == "")
                {
                    MessageBox.Show("CST SeqNo is Empty!");
                    return;
                }

                FileDataCreateRequest _trx = new FileDataCreateRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.CassetteSeqNo = txtCstSeqNO.Text.ToString().Trim();
                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;
                string _respXml = _resp.Xml;

                FileDataCreateReply _fileDataCreateReply = (FileDataCreateReply)Spec.CheckXMLFormat(_respXml);
                string returnCode=_fileDataCreateReply.BODY.Result;
                if(returnCode=="OK")
                    ShowMessage(this, lblCaption.Text, "", "FileData Create Success !", MessageBoxIcon.Information);
                else if(returnCode=="NG")
                    ShowMessage(this, lblCaption.Text, "", "FileData Create Fail !", MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
