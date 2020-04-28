using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormPortControl : FormBase
    {
        private Port CurPort = null;

        public FormPortControl()
        {
            InitializeComponent();
        }

        private void FormPortControl_Load(object sender, EventArgs e)
        {
            try
            {
                #region  CELL 沒有 port Transfer
                if (FormMainMDI.G_OPIAp.CurLine.FabType.Equals("CELL"))
                {
                    colPortTransfer.Visible =false ;
                    pnlPortTransfer_New.Visible = false;
                    pnlPortTransfer.Visible = false;
                }
                #endregion

                #region Port Command 區域特殊command顯示 & 畫面調整

                #region Port Assignment - for CELL GAP
                if (FormMainMDI.G_OPIAp.CurLine.LineType == "GAP" || FormMainMDI.G_OPIAp.CurLine.LineType == "PDR")
                {
                    grbPortAssignment.Visible = true;
                }
                else
                {
                    grbPortAssignment.Visible = false;
                }
                #endregion

                #region  MQC InspectionFlowPriority Rule
                List<string> _mqcLineType = new List<string>(new string[] { "FCMQC_TYPE1", "FCMQC_TYPE2" });
                if (_mqcLineType.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                {
                    grbInspectionFlowPriority.Visible = true;

                    flpInspectionFlowPriority.Controls.Clear();

                    for (int i = 1; i <= FormMainMDI.G_OPIAp.Dic_Node.Values.Where(r => r.NodeAttribute.Equals("IN")).Count(); i++)
                    {
                        ucFlowPriority _priority = new ucFlowPriority(i, FormMainMDI.G_OPIAp.Dic_Node);

                        _priority.Name = "ucPriority" + i.ToString();

                        _priority.Size = new System.Drawing.Size(490, 30);

                        _priority.Tag = string.Empty;

                        flpInspectionFlowPriority.Controls.Add(_priority);
                    }
                }
                else
                {
                    grbInspectionFlowPriority.Visible = false;
                }
                #endregion

                #region 調整grbPortCommand寬度
                if (grbInspectionFlowPriority.Visible == false && grbPortAssignment.Visible == false)
                {
                    grbPortCommand.Width = 1086;
                }
                #endregion

                #endregion

                #region Load Port Data
                foreach (string _key in FormMainMDI.G_OPIAp.Dic_Port.Keys)
                {
                    Port _port = FormMainMDI.G_OPIAp.Dic_Port[_key];
                    Node _node = FormMainMDI.G_OPIAp.Dic_Node[_port.NodeNo];

                    dgvData.Rows.Add(_key, _port.NodeNo, _port.PortID, _port.PortStatus.ToString(),
                        _port.PortType.ToString(), _port.PortMode.ToString(), _port.PortEnable, _port.PortTransfer.ToString(), _port.CassetteStatus.ToString(),
                        _node.CassetteQTime.ToString() );
                }
                #endregion

                LoadPortCommand();
                //20170109 sy modify
                #region Port Assignment
                if (grbPortAssignment.Visible)
                {
                    cboPort01Assignment_New.DataSource = InitialPortAssignment(FormMainMDI.G_OPIAp.CurLine.LineType);
                    cboPort01Assignment_New.DisplayMember = "NEW_PORT_ASSIGNMENT_DESC";
                    cboPort01Assignment_New.ValueMember = "NEW_PORT_ASSIGNMENT";
                    cboPort01Assignment_New.SelectedIndex = -1;

                    cboPort02Assignment_New.DataSource = InitialPortAssignment(FormMainMDI.G_OPIAp.CurLine.LineType);
                    cboPort02Assignment_New.DisplayMember = "NEW_PORT_ASSIGNMENT_DESC";
                    cboPort02Assignment_New.ValueMember = "NEW_PORT_ASSIGNMENT";
                    cboPort02Assignment_New.SelectedIndex = -1;

                    cboPort03Assignment_New.DataSource = InitialPortAssignment(FormMainMDI.G_OPIAp.CurLine.LineType);
                    cboPort03Assignment_New.DisplayMember = "NEW_PORT_ASSIGNMENT_DESC";
                    cboPort03Assignment_New.ValueMember = "NEW_PORT_ASSIGNMENT";
                    cboPort03Assignment_New.SelectedIndex = -1;

                    cboPort04Assignment_New.DataSource = InitialPortAssignment(FormMainMDI.G_OPIAp.CurLine.LineType);
                    cboPort04Assignment_New.DisplayMember = "NEW_PORT_ASSIGNMENT_DESC";
                    cboPort04Assignment_New.ValueMember = "NEW_PORT_ASSIGNMENT";
                    cboPort04Assignment_New.SelectedIndex = -1;
                }
                #endregion

                InitialForm();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void LoadPortCommand()
        {
            try
            {
                #region Port Type
                DataTable dtPortTypes = UniTools.InitDt(new string[] { "NEW_PORT_TYPE", "NEW_PORT_TYPE_DESC" });
                foreach (string ptName in Enum.GetNames(typeof(ePortType)))
                {
                    int value = (int)Enum.Parse(typeof(ePortType), ptName);

                    if (value == 0) continue;

                    ePortType portType = (ePortType)Enum.Parse(typeof(ePortType), ptName);
                    string desc = Public.GetEnumDesc(portType);
                    DataRow drNew = dtPortTypes.NewRow();
                    drNew["NEW_PORT_TYPE"] = value;
                    drNew["NEW_PORT_TYPE_DESC"] = string.Format("{0}:{1}", value, desc);
                    dtPortTypes.Rows.Add(drNew);
                }

                cboPortType_New.DataSource = dtPortTypes;
                cboPortType_New.DisplayMember = "NEW_PORT_TYPE_DESC";
                cboPortType_New.ValueMember = "NEW_PORT_TYPE";
                cboPortType_New.SelectedIndex = -1;
                #endregion

                #region Port Mode
                DataTable dtPortMode = UniTools.InitDt(new string[] { "NEW_PORT_MODE", "NEW_PORT_MODE_DESC" });
                foreach (string pmName in Enum.GetNames(typeof(ePortMode)))
                {
                    int value = (int)Enum.Parse(typeof(ePortMode), pmName);

                    if (value == 0) continue;

                    ePortMode portMode = (ePortMode)Enum.Parse(typeof(ePortMode), pmName);
                    string desc = Public.GetEnumDesc(portMode);
                    DataRow drNew = dtPortMode.NewRow();
                    drNew["NEW_PORT_MODE"] = value;
                    drNew["NEW_PORT_MODE_DESC"] = string.Format("{0}:{1}", value, desc);
                    dtPortMode.Rows.Add(drNew);
                }

                cboPortMode_New.DataSource = dtPortMode;
                cboPortMode_New.DisplayMember = "NEW_PORT_MODE_DESC";
                cboPortMode_New.ValueMember = "NEW_PORT_MODE";
                cboPortMode_New.SelectedIndex = -1;
                #endregion

                #region Port Transfer
                DataTable dtPortTransfer = UniTools.InitDt(new string[] { "NEW_PORT_TRX", "NEW_PORT_TRX_DESC" });
                foreach (string ptName in Enum.GetNames(typeof(ePortTransfer)))
                {
                    int value = (int)Enum.Parse(typeof(ePortTransfer), ptName);

                    if (value == 0) continue;

                    ePortTransfer portTrx = (ePortTransfer)Enum.Parse(typeof(ePortTransfer), ptName);
                    string desc = Public.GetEnumDesc(portTrx);
                    DataRow drNew = dtPortTransfer.NewRow();
                    drNew["NEW_PORT_TRX"] = value;
                    drNew["NEW_PORT_TRX_DESC"] = string.Format("{0}:{1}", value, desc);
                    dtPortTransfer.Rows.Add(drNew);
                }

                cboPortTransfer_New.DataSource = dtPortTransfer;
                cboPortTransfer_New.DisplayMember = "NEW_PORT_TRX_DESC";
                cboPortTransfer_New.ValueMember = "NEW_PORT_TRX";
                cboPortTransfer_New.SelectedIndex = -1;
                #endregion

                #region Port Enable
                DataTable dtPortEnable = UniTools.InitDt(new string[] { "NEW_PORT_ENABLE", "NEW_PORT_ENABLE_DESC" });
                foreach (string peName in Enum.GetNames(typeof(ePortEnable)))
                {                    
                    int value = (int)Enum.Parse(typeof(ePortEnable), peName);

                    if (value == 0) continue;

                    ePortEnable portEnable = (ePortEnable)Enum.Parse(typeof(ePortEnable), peName);
                    string desc = Public.GetEnumDesc(portEnable);
                    DataRow drNew = dtPortEnable.NewRow();
                    drNew["NEW_PORT_ENABLE"] = value;
                    drNew["NEW_PORT_ENABLE_DESC"] = string.Format("{0}:{1}", value, desc);
                    dtPortEnable.Rows.Add(drNew);
                }

                cboPortEnable_New.DataSource = dtPortEnable;
                cboPortEnable_New.DisplayMember = "NEW_PORT_ENABLE_DESC";
                cboPortEnable_New.ValueMember = "NEW_PORT_ENABLE";
                cboPortEnable_New.SelectedIndex = -1;
                #endregion

                #region Port Assignment
                if (grbPortAssignment.Visible)
                {
                    //cboPort01Assignment_New.DataSource = InitialPortAssignment();
                    //cboPort01Assignment_New.DisplayMember = "NEW_PORT_ASSIGNMENT_DESC";
                    //cboPort01Assignment_New.ValueMember = "NEW_PORT_ASSIGNMENT";
                    //cboPort01Assignment_New.SelectedIndex = -1;

                    //cboPort02Assignment_New.DataSource = InitialPortAssignment();
                    //cboPort02Assignment_New.DisplayMember = "NEW_PORT_ASSIGNMENT_DESC";
                    //cboPort02Assignment_New.ValueMember = "NEW_PORT_ASSIGNMENT";
                    //cboPort02Assignment_New.SelectedIndex = -1;

                    //cboPort03Assignment_New.DataSource = InitialPortAssignment();
                    //cboPort03Assignment_New.DisplayMember = "NEW_PORT_ASSIGNMENT_DESC";
                    //cboPort03Assignment_New.ValueMember = "NEW_PORT_ASSIGNMENT";
                    //cboPort03Assignment_New.SelectedIndex = -1;

                    //cboPort04Assignment_New.DataSource = InitialPortAssignment();
                    //cboPort04Assignment_New.DisplayMember = "NEW_PORT_ASSIGNMENT_DESC";
                    //cboPort04Assignment_New.ValueMember = "NEW_PORT_ASSIGNMENT";
                    //cboPort04Assignment_New.SelectedIndex = -1;
                }
                #endregion
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
                if (cboPortType_New.Items.Count > 0) cboPortType_New.SelectedIndex = -1;
                if (cboPortEnable_New.Items.Count > 0) cboPortEnable_New.SelectedIndex = -1;
                if (cboPortMode_New.Items.Count > 0) cboPortMode_New.SelectedIndex = -1;
                if (cboPortTransfer_New.Items.Count > 0) cboPortTransfer_New.SelectedIndex = -1;

                txtCSTQTime_New.Text = string.Empty;

                RefreshData();

                if (grbInspectionFlowPriority.Visible)
                {
                    Send_InspectionFlowPriorityInfoRequest();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private DataTable InitialPortAssignment(string lineType)
        {
            DataTable dtPortAssignment = UniTools.InitDt(new string[] { "NEW_PORT_ASSIGNMENT", "NEW_PORT_ASSIGNMENT_DESC" });

            try 
            {                
                foreach (string peName in Enum.GetNames(typeof(ePortAssignment)))
                {
                    int value = (int)Enum.Parse(typeof(ePortAssignment), peName);

                    if (value == 0) continue;
                    if (lineType == "PDR") value = value - 2;
                    if (lineType == "PDR" && (peName == "GAP" || peName == "GMI")) continue;

                    if (lineType == "GAP" && (peName == "PDR" || peName == "CEM")) continue;

                    ePortAssignment portAssignment = (ePortAssignment)Enum.Parse(typeof(ePortAssignment), peName);
                    string desc = Public.GetEnumDesc(portAssignment);
                    DataRow drNew = dtPortAssignment.NewRow();
                    drNew["NEW_PORT_ASSIGNMENT"] = value;
                    drNew["NEW_PORT_ASSIGNMENT_DESC"] = string.Format("{0}:{1}", value, desc);
                    dtPortAssignment.Rows.Add(drNew);
                }

                return dtPortAssignment;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return dtPortAssignment;
            }
        }

        private void RefreshData()
        {
            try
            {
                string _portKey = string.Empty;
                Port _port = null;
                Node _node = null;

                foreach (DataGridViewRow dgvRow in dgvData.Rows)
                {
                    _portKey = dgvRow.Cells[colPortKey.Name].Value.ToString();
                    _port = FormMainMDI.G_OPIAp.Dic_Port[_portKey];
                    _node = FormMainMDI.G_OPIAp.Dic_Node[_port.NodeNo];

                    dgvRow.Cells[colPortStatus.Name].Value = _port.PortStatus.ToString();
                    dgvRow.Cells[colPortType.Name].Value = _port.PortType.ToString();
                    dgvRow.Cells[colPortMode.Name].Value = _port.PortMode.ToString();
                    dgvRow.Cells[colPortEnable.Name].Value = _port.PortEnable.ToString();
                    dgvRow.Cells[colPortTransfer.Name].Value = _port.PortTransfer.ToString();
                    dgvRow.Cells[colCassetteStatus.Name].Value = _port.CassetteStatus.ToString();
                    dgvRow.Cells[colCassetteQTime.Name].Value = _node.CassetteQTime.ToString();
                                   
                }

                ShowInfo(dgvData.CurrentRow);                
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

                CurPort = FormMainMDI.G_OPIAp.Dic_Port[dgvRow.Cells["colPortKey"].Value.ToString()];

                txtLocalNo.Text = dgvRow.Cells[colLocalNo.Name].Value.ToString();
                txtPortID.Text = dgvRow.Cells[colPortID.Name].Value.ToString();
                txtPortStatus.Text = dgvRow.Cells[colPortStatus.Name].Value.ToString();
                txtPortType.Text = dgvRow.Cells[colPortType.Name].Value.ToString();
                txtPortMode.Text = dgvRow.Cells[colPortMode.Name].Value.ToString();
                txtPortEnable.Text = dgvRow.Cells[colPortEnable.Name].Value.ToString();
                txtPortTransfer.Text = dgvRow.Cells[colPortTransfer.Name].Value.ToString();
                txtCSTStatus.Text = dgvRow.Cells[colCassetteStatus.Name].Value.ToString();
                txtCSTQTime.Text = dgvRow.Cells[colCassetteQTime.Name].Value.ToString();

                #region  MQC InspectionFlowPriority Rule
                if (grbInspectionFlowPriority.Visible)
                {
                    Refresh_FlowPriority();
                }
                #endregion


                #region GAP Port Assignment
                if (grbPortAssignment.Visible)
                {
                    string _key = string.Empty ;

                    foreach (Port _port in FormMainMDI.G_OPIAp.Dic_Port.Values)
                    {
                        _key = string.Format("txtPortAssignment_{0}",_port.PortNo);

                        TextBox _txt = flpPortAssignment_old.Controls.Find(_key,true).OfType<TextBox>().FirstOrDefault();

                        if (_txt == null) continue ;

                        _txt.Text = _port.PortAssignment.ToString();
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

        private void btnPortCommand_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender ;
                
                if (CurPort == null)
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, string.Empty, "Please Choose Port !", MessageBoxIcon.Warning);
                    return;
                }

                string _oldValue = string.Empty;
                string _value = string.Empty;
                string _valueDesc = string.Empty;
                string _cmdType = _btn.Tag.ToString();

                switch (_cmdType.ToUpper())
                {
                    case "TYPE": 

                        #region Port Type
                        if (cboPortType_New.SelectedValue == null)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Please choose the New Port Type", MessageBoxIcon.Error);
                            return;
                        }

                        _oldValue = ((int)CurPort.PortType).ToString();
                        _value = cboPortType_New.SelectedValue.ToString();
                        _valueDesc = cboPortType_New.Text;

                        //if (_value.Equals(_oldValue))
                        //{
                        //    ShowMessage(this, this.lblCaption.Text, "", "The Current Port Type  and New Port Type must be different!!", MessageBoxIcon.Error);
                        //    return;
                        //}
                        #endregion

                        break;

                    case "MODE":
 
                        #region Port Mode
                        if (cboPortMode_New.SelectedValue==null)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Please choose the New Port Mode", MessageBoxIcon.Error);
                            return;
                        }

                        _oldValue = ((int)CurPort.PortMode).ToString();
                        _value = cboPortMode_New.SelectedValue.ToString();
                        _valueDesc = cboPortMode_New.Text;

                        //if (_value.Equals(_oldValue))
                        //{
                        //    ShowMessage(this, this.lblCaption.Text, "", "The Current Port Mode  and New Port Mode must be different!!", MessageBoxIcon.Error);
                        //    return;
                        //}
                        #endregion

                        break;

                    case "ENABLED": 

                        #region Port Enable
                        if (cboPortEnable_New.SelectedValue == null)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Please choose the New Port Enable", MessageBoxIcon.Error);
                            return;
                        }

                        _oldValue = ((int)CurPort.PortEnable).ToString();
                        _value = cboPortEnable_New.SelectedValue.ToString();
                        _valueDesc = cboPortEnable_New.Text;

                        //if (_value.Equals(_oldValue))
                        //{
                        //    ShowMessage(this, this.lblCaption.Text, "", "The Current Por Enable  and New Port Enable must be different!!", MessageBoxIcon.Error);
                        //    return;
                        //}
                        #endregion

                        break;

                    case "TRANSFER":

                        #region Port Transfer
                        if (cboPortTransfer_New.SelectedValue == null)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Please choose the New Port Transfer", MessageBoxIcon.Error);
                            return;
                        }

                        _oldValue = ((int)CurPort.PortTransfer).ToString();
                        _value = cboPortTransfer_New.SelectedValue.ToString();
                        _valueDesc = cboPortTransfer_New.Text.ToString();

                        //if (_value.Equals(_oldValue))
                        //{
                        //    ShowMessage(this, this.lblCaption.Text, "", "The Current Por Transfer  and New Port Transfer must be different!!", MessageBoxIcon.Error);
                        //    return;
                        //}
                        #endregion

                        break;

                    case "QTIME": 

                        #region Cassette On Port Q Time
                        _value = txtCSTQTime_New.Text.ToString();

                        int _num = 0;

                        int.TryParse(_value, out _num);

                        if (_num > 65535 || _num < 0)
                        {
                            ShowMessage(this, lblCaption.Text , "", " 0 <= Cassette On Port Q Time <= 65535", MessageBoxIcon.Error);

                            txtCSTQTime_New.Text = string.Empty;

                            txtCSTQTime_New.Focus();

                            return;
                        }
                        #endregion

                        break;


                    default :
                        break;
                }

                #region Send BC Trx

                if (_btn.Tag.ToString() == "QTime")
                {
                    #region CassetteOnPortQTimeRequest
                    string msg = string.Format("Please confirm whether you will process the Cassette On Port QTime of Node:{0} into [{1}] ?", CurPort.NodeNo, _value);
                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                    if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;
                    
                    CassetteOnPortQTimeRequest _trx = new CassetteOnPortQTimeRequest();
                    _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    _trx.BODY.EQUIPMENTNO = CurPort.NodeNo;
                    _trx.BODY.QTIME = _value;

                    //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                    MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                    if (_resp == null) return;

                    #region CassetteOnPortQTimeReply
                    ShowMessage(this, lblCaption.Text, "", "Cassette On Port QTime Change Send to BC Success !", MessageBoxIcon.Information);
                    #endregion

                    #endregion
                }
                else
                {
                    #region PortCommandRequest
                    string msg = string.Format("Please confirm whether you will change Port {0}[{3}] for Node[{1}], Port[{2}] ?", _cmdType, CurPort.NodeNo, CurPort.PortID, _valueDesc);
                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                    if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;
                    
                    PortCommandRequest _trx = new PortCommandRequest();
                    _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    _trx.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                    _trx.BODY.EQUIPMENTNO = CurPort.NodeNo;
                    _trx.BODY.PORTNO = CurPort.PortNo;
                    _trx.BODY.PORTID = CurPort.PortID;
                    _trx.BODY.COMMANDTYPE = _cmdType.ToUpper();
                    _trx.BODY.PORTCOMMAND = _value;

                    //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                    MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                    if (_resp == null) return;

                    #region PortCommandReply
                    ShowMessage(this, lblCaption.Text, "", "Port Command Send to BC Success !", MessageBoxIcon.Information);
                    #endregion

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

        private void txtCSTQTime_New_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                if (txtCSTQTime_New.Text == string.Empty) return;

                int _num = 0;

                int.TryParse(_txt.Text.ToString(), out _num);


                if (_num > 65535 || _num < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", " 0 <= Cassette On Port Q Time <= 65535", MessageBoxIcon.Error);

                    txtCSTQTime_New.Text = string.Empty;

                    txtCSTQTime_New.Focus();

                    return;
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

        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            InitialForm();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                string _err = string.Empty;
                string _xml = string.Empty;

                //GAP Port Assignment需要取所有Port資訊
                if (grbPortAssignment.Visible)
                {
                    foreach (Port _port in FormMainMDI.G_OPIAp.Dic_Port.Values)
                    {
                        #region Send to BC PortStatusRequest
                        PortStatusRequest _trx = new PortStatusRequest();
                        _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _trx.BODY.EQUIPMENTNO = _port.NodeNo;
                        _trx.BODY.PORTNO = _port.PortNo;

                        _xml = _trx.WriteToXml();

                        FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                        #endregion
                    }

                    #region Send to BC EquipmentStatusRequest -- Get CassetteQTime

                    EquipmentStatusRequest _eqTrx = new EquipmentStatusRequest();
                    _eqTrx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    _eqTrx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    _eqTrx.BODY.EQUIPMENTNO = CurPort.NodeNo;

                    _xml = _eqTrx.WriteToXml();

                    FormMainMDI.SocketDriver.SendMessage(_eqTrx.HEADER.TRANSACTIONID, _eqTrx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                    #endregion
                }
                else
                {
                    #region Send to BC PortStatusRequest
                    PortStatusRequest _trx = new PortStatusRequest();
                    _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    _trx.BODY.EQUIPMENTNO = CurPort.NodeNo;
                    _trx.BODY.PORTNO = CurPort.PortNo;

                    _xml = _trx.WriteToXml();

                    FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                    #endregion

                    #region Send to BC EquipmentStatusRequest -- Get CassetteQTime

                    EquipmentStatusRequest _eqTrx = new EquipmentStatusRequest();
                    _eqTrx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    _eqTrx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    _eqTrx.BODY.EQUIPMENTNO = CurPort.NodeNo;

                    _xml = _eqTrx.WriteToXml();

                    FormMainMDI.SocketDriver.SendMessage(_eqTrx.HEADER.TRANSACTIONID, _eqTrx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                    #endregion
                }

                #region  MQC InspectionFlowPriority Rule
                if (grbInspectionFlowPriority.Visible)
                {
                    Send_InspectionFlowPriorityInfoRequest();
                }            
                #endregion

                #region  GAP Port Assignment
                if (grbPortAssignment.Visible)
                {
                    cboPort01Assignment_New.SelectedIndex = -1;
                    cboPort02Assignment_New.SelectedIndex = -1;
                    cboPort03Assignment_New.SelectedIndex = -1;
                    cboPort04Assignment_New.SelectedIndex = -1;
                }
                #endregion
                
                ShowMessage(this, lblCaption.Text, "", "Reload Port Information  Send to BC Success !", MessageBoxIcon.Information);    
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

                tmrRefresh.Enabled = false;
            }
        }

        private void Refresh_FlowPriority()
        {
            try
            {
                int _idx = 0;

                string _localNo = string.Empty;

                if (CurPort.PortNo != lblInspectionFlowPriorityPort.Tag.ToString())
                {
                    lblInspectionFlowPriorityPort.Text = string.Format("Port No [ {0} ] ", CurPort.PortNo);                    

                    lblInspectionFlowPriorityPort.Tag = CurPort.PortNo;
                }

                if (CurPort.FlowPriority == string.Empty)
                {
                    foreach (ucFlowPriority _uc in flpInspectionFlowPriority.Controls.OfType<ucFlowPriority>().OrderBy(r => r.Name))
                    {
                        if (_uc.CurrentLocalNo != "00") _uc.SetDefalutLocalNo("00");
                    }
                }
                else
                {
                    foreach (ucFlowPriority _uc in flpInspectionFlowPriority.Controls.OfType<ucFlowPriority>().OrderBy(r => r.Name))
                    {
                        _idx = (_uc.Priority - 1) * 2;
                        if (CurPort.FlowPriority.Length < (_idx + 1))
                        {
                            if (_uc.CurrentLocalNo != "00") _uc.SetDefalutLocalNo("00");
                        }
                        else
                        {
                            _localNo = CurPort.FlowPriority.Substring(_idx, 2);

                            if (_uc.CurrentLocalNo != _localNo) _uc.SetDefalutLocalNo(_localNo);
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

        private void Send_InspectionFlowPriorityInfoRequest()
        {
            try
            {
                foreach (ucFlowPriority _uc in flpInspectionFlowPriority.Controls.OfType<ucFlowPriority>().OrderBy(r => r.Name))
                {
                    _uc.ResetCombobox();
                }

                #region InspectionFlowPriorityInfoRequest
                InspectionFlowPriorityInfoRequest _trx = new InspectionFlowPriorityInfoRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurPort.NodeNo;
                _trx.BODY.PORTNO = CurPort.PortNo;

                string _xml = _trx.WriteToXml();

                string _err = string.Empty;

                if (FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID) == false)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("Inspection Flow Priority Info Request Error [{0}]", _err), MessageBoxIcon.Warning);
                }

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnInspectionFlowPrioritySet_Click(object sender, EventArgs e)
        {
            try
            {
                string _localNo = string.Empty;
                string _priority = string.Empty;
                string _desc = string.Empty;

                InspectionFlowPriorityChangeRequest _trx = new InspectionFlowPriorityChangeRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurPort.NodeNo;
                _trx.BODY.PORTNO = CurPort.PortNo;

                foreach (ucFlowPriority _uc in flpInspectionFlowPriority.Controls.OfType<ucFlowPriority>().OrderBy(r => r.Name))
                {
                    _localNo = _uc.GetChooseLocalNo();

                    if (_localNo != "00")
                    {
                        if (_priority.IndexOf(_localNo) > -1)
                        {
                            ShowMessage(this, lblCaption.Text, "", string.Format("{0}'st Priority of Local No [{1}] is duplicate", _uc.Priority.ToString(), "L" + (int.Parse(_localNo)).ToString()), MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    _priority = _priority + _localNo;
                }

                _trx.BODY.PRIORITY = _priority;

                string _msg = string.Format("Port No [{0}] Set New Priority [{1}] ? ",CurPort.PortNo,_priority);

                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                string _xml = _trx.WriteToXml();

                //15秒timeout 
                MessageResponse _inspectionFlowPriorityChangeResponse = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_inspectionFlowPriorityChangeResponse == null) return;

                #region InspectionFlowPriorityChangeReply
                ShowMessage(this, lblCaption.Text, "", "Inspection Flow Priority Change Request Send to BC Success !", MessageBoxIcon.Information);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnPortAssignment_Click(object sender, EventArgs e)
        {
            try
            {
                //同時 需切換4個port & 並且 GAP & GMI 都必須各2個port
                //20170604 by huangjiayin: PDR不需要卡2，2配置
                //Command需要卡Port状态NOCSTEXIST
                //需要记录OPIHistory
                //Download Command后，弹窗提示
                #region Check 是否選擇Assignment
                if (cboPort01Assignment_New.SelectedValue == null)
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, string.Empty, "Please Choose Port01 Assignment !", MessageBoxIcon.Warning);
                    return;
                }

                if (cboPort02Assignment_New.SelectedValue == null)
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, string.Empty, "Please Choose Port02 Assignment !", MessageBoxIcon.Warning);
                    return;
                }

                if (cboPort03Assignment_New.SelectedValue == null)
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, string.Empty, "Please Choose Port03 Assignment !", MessageBoxIcon.Warning);
                    return;
                }

                if (cboPort04Assignment_New.SelectedValue == null)
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, string.Empty, "Please Choose Port04 Assignment !", MessageBoxIcon.Warning);
                    return;
                }
                #endregion


               #region Check  GAP & GMI 都必須各2個port
                    int _gapCnt = 0, _gmiCnt = 0;
                    string _assignment01 = cboPort01Assignment_New.SelectedValue.ToString();
                    string _assignment02 = cboPort02Assignment_New.SelectedValue.ToString();
                    string _assignment03 = cboPort03Assignment_New.SelectedValue.ToString();
                    string _assignment04 = cboPort04Assignment_New.SelectedValue.ToString();

                    //1 : GAP
                    if (_assignment01 == "1") _gapCnt = _gapCnt + 1;
                    if (_assignment02 == "1") _gapCnt = _gapCnt + 1;
                    if (_assignment03 == "1") _gapCnt = _gapCnt + 1;
                    if (_assignment04 == "1") _gapCnt = _gapCnt + 1;

                    //2 : GMI
                    if (_assignment01 == "2") _gmiCnt = _gmiCnt + 1;
                    if (_assignment02 == "2") _gmiCnt = _gmiCnt + 1;
                    if (_assignment03 == "2") _gmiCnt = _gmiCnt + 1;
                    if (_assignment04 == "2") _gmiCnt = _gmiCnt + 1;

                    if (FormMainMDI.G_OPIAp.CurLine.ServerName == "CCGAP100")
                    {
                        if (_gapCnt != 2 && _gmiCnt != 2)
                        {
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, string.Empty, "GAP & GMI must be used twice !", MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    #endregion

                    #region[Check No CST EXIST]
                    foreach (Port _port in FormMainMDI.G_OPIAp.Dic_Port.Values)
                    {
                        if (_port.CassetteStatus!=eCassetteStatus.NoCassetteExist)
                        {
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, string.Empty,string.Format("Port{0} has CST, can not do assignment change!",_port.PortID), MessageBoxIcon.Warning);
                            return;
                        }
 
                    }

                    #endregion


                    #region PortAssignmentCommandRequest
                    PortAssignmentCommandRequest _trx = new PortAssignmentCommandRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                foreach (Port _port in FormMainMDI.G_OPIAp.Dic_Port.Values)
                {
                    PortAssignmentCommandRequest.ASSIGNMENTc _assignment = new PortAssignmentCommandRequest.ASSIGNMENTc();

                    _assignment.EQUIPMENTNO = _port.NodeNo;
                    _assignment.PORTNO = _port.PortNo;
                    _assignment.PORTID = _port.PortID;

                    switch (_port.PortNo)
                    {
                        case "01": _assignment.ASSIGNMENT = _assignment01; break;
                        case "02": _assignment.ASSIGNMENT = _assignment02; break;
                        case "03": _assignment.ASSIGNMENT = _assignment03; break;
                        case "04": _assignment.ASSIGNMENT = _assignment04; break;
                        default: break;
                    }

                    _trx.BODY.ASSIGNMENTLIST.Add(_assignment);

                }
                

                string _xml = _trx.WriteToXml();

                string _err = string.Empty;

                if (FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID) == false)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("Port Assignment Command Request Error [{0}]", _err), MessageBoxIcon.Warning);
                }
                else
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("Port Assignment Command Request Successfully!"), MessageBoxIcon.Information); 
                }

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
