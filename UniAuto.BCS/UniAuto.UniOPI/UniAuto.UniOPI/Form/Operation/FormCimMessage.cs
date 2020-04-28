using System;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormCimMessage : FormBase
    {
        string ActNodeNo=string.Empty ;

        public FormCimMessage()
        {
            InitializeComponent();
        }

        private void FormCimMessage_Load(object sender, EventArgs e)
        {
            try
            {
                if (FormMainMDI.G_OPIAp.CurLine.FabType == "CELL")
                {
                    lblTouchPanelNo.Visible = true;
                    txtTouchPanelNo.Visible = true;
                    colTouchPanelNo.Visible = true; 
                }
                else
                {
                    lblTouchPanelNo.Visible = false;
                    txtTouchPanelNo.Visible = false;
                    colTouchPanelNo.Visible = false;
                }

                foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                    cboLocalNode.Items.Add(string.Format("{0}-{1}", _node.NodeNo, _node.NodeID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void RefreshCIMMessageInfo(CIMMessageDataReply _cimMessageDataReply)
        {
            try
            {
                if (dgvMessage.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(
                           delegate
                           {
                               dgvMessage.Rows.Clear();

                               foreach (CIMMessageDataReply.MESSAGEc _msg in _cimMessageDataReply.BODY.MESSAGELIST)
                               {
                                   dgvMessage.Rows.Add(_msg.MESSAGEDATETIME, _msg.MESSAGEID, _msg.TOUCHPANELNO,_msg.MESSAGETEXT,"Clear");
                               }
                           }
                           ));
                }
                else
                {
                    dgvMessage.Rows.Clear();

                    foreach (CIMMessageDataReply.MESSAGEc _msg in _cimMessageDataReply.BODY.MESSAGELIST)
                    {
                        dgvMessage.Rows.Add(_msg.MESSAGEDATETIME, _msg.MESSAGEID, _msg.TOUCHPANELNO, _msg.MESSAGETEXT, "Clear");
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            try
            {
                string _err = string.Empty;
                string _xml = string.Empty;
                Button _btn = (Button)sender;

                if (ActNodeNo == string.Empty)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Please Choose Local No!", MessageBoxIcon.Error);
                    return;
                }

                switch (_btn.Name)
                {
                    case "btnQuery":

                        #region Query
                        
                        //"HSMS_"開頭的report mode沒有clear 功能 也無法查詢cim message
                        if (ActNodeNo != "00")
                        {
                            if (FormMainMDI.G_OPIAp.Dic_Node[ActNodeNo].ReportMode.Contains("HSMS_"))
                            {
                                ShowMessage(this, this.lblCaption.Text, "", string.Format("EQP [{0}] does not support CIM Message Clear!", ActNodeNo), MessageBoxIcon.Error);
                                return;
                            }
                        }

                        #region Get Line Information 
                        CIMMessageDataRequest _cimMessageDataRequest = new CIMMessageDataRequest();
                        _cimMessageDataRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _cimMessageDataRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _cimMessageDataRequest.BODY.EQUIPMENTNO = ActNodeNo;
                        _xml = _cimMessageDataRequest.WriteToXml();

                        //FormMainMDI.SocketDriver.SendMessage(_cimMessageDataRequest.HEADER.TRANSACTIONID, _cimMessageDataRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                        //if (FormMainMDI.SocketDriver.DicRecvMessageEvent.ContainsKey(CIM_MESSAGE_DATA_REPLY))
                        //    FormMainMDI.SocketDriver.DicRecvMessageEvent[CIM_MESSAGE_DATA_REPLY] = SocketDriver_RecvMessageEvent;
                        //else
                        //    FormMainMDI.SocketDriver.DicRecvMessageEvent.Add(CIM_MESSAGE_DATA_REPLY, SocketDriver_RecvMessageEvent);

                        //15秒timeout 
                        MessageResponse _resp = this.SendRequestResponse(_cimMessageDataRequest.HEADER.TRANSACTIONID, _cimMessageDataRequest.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SocketResponseTime_Query);

                        if (_resp == null) return;

                        #region CIMMessageDataReply
                        string _respXml = _resp.Xml;
                        CIMMessageDataReply _cimMessageDataReply = (CIMMessageDataReply)Spec.CheckXMLFormat(_respXml);
                        RefreshCIMMessageInfo(_cimMessageDataReply);
                        ShowMessage(this, lblCaption.Text, "", "CIM Message Data Request Send to BC Success !", MessageBoxIcon.Information);
                        #endregion

                        #endregion
                        break;
                        #endregion

                        //20170228 huangjiayin: clear all cim message, only remain n day

                    case "btnClearAll":

                        #region clear_all
                        //data check
                        if (!System.Text.RegularExpressions.Regex.IsMatch(tbxDays.Text, "^[0-9]$"))
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Must in 0-9 Days!!", MessageBoxIcon.Error);
                            return;
                        }

                        //get all cim message
                        int rm_days = Convert.ToInt32(tbxDays.Text);

                        foreach( Node nd in FormMainMDI.G_OPIAp.Dic_Node.Values )
                        {

                        _cimMessageDataRequest = new CIMMessageDataRequest();
                        _cimMessageDataRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _cimMessageDataRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _cimMessageDataRequest.BODY.EQUIPMENTNO = nd.NodeNo;
                        _xml = _cimMessageDataRequest.WriteToXml();
                        _resp = this.SendRequestResponse(_cimMessageDataRequest.HEADER.TRANSACTIONID, _cimMessageDataRequest.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SocketResponseTime_Query);
                        if (_resp == null) return;
                        _respXml = _resp.Xml;
                        _cimMessageDataReply = (CIMMessageDataReply)Spec.CheckXMLFormat(_respXml);

                        foreach (CIMMessageDataReply.MESSAGEc msg_ in _cimMessageDataReply.BODY.MESSAGELIST)
                        {
                            if (Convert.ToDateTime(msg_.MESSAGEDATETIME).AddDays(rm_days) > DateTime.Now) continue;

                            CIMMessageCommandRequest _cimMessageCMDRequest = new CIMMessageCommandRequest();
                            _cimMessageCMDRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                            _cimMessageCMDRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                            _cimMessageCMDRequest.BODY.EQUIPMENTNO = nd.NodeNo;
                            _cimMessageCMDRequest.BODY.COMMAND = "Clear";
                            _cimMessageCMDRequest.BODY.MESSAGEID = msg_.MESSAGEID;
                            _cimMessageCMDRequest.BODY.MESSAGETEXT = msg_.MESSAGETEXT;
                            _cimMessageCMDRequest.BODY.TOUCHPANELNO = msg_.TOUCHPANELNO;
                            _cimMessageCMDRequest.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;

                            _xml = _cimMessageCMDRequest.WriteToXml();

                            FormMainMDI.SocketDriver.SendMessage(_cimMessageCMDRequest.HEADER.TRANSACTIONID, _cimMessageCMDRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

 
                        }


                        }

                        ShowMessage(this, lblCaption.Text, "", "All cim message are cleared Success", MessageBoxIcon.Information);

#endregion

                        break;



                    case "btnSend":

                        #region CIM Message Command Request

                        if (txtMsgText.Text.Trim() == string.Empty)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Please input CIM Message!", MessageBoxIcon.Error);
                            txtMsgText.Focus();
                            return;
                        }

                        string msg = string.Format("Send CIM Messsage [{0}] to [{1}]", txtMsgText.Text.ToString(), ActNodeNo == "00" ? "All EQP" : ActNodeNo);
                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text + " Set", msg)) return;

                        CIMMessageCommandRequest _cimMessageCommandRequest = new CIMMessageCommandRequest();
                        _cimMessageCommandRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _cimMessageCommandRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _cimMessageCommandRequest.BODY.EQUIPMENTNO = ActNodeNo;
                        _cimMessageCommandRequest.BODY.COMMAND = "Set";
                        _cimMessageCommandRequest.BODY.MESSAGEID = "0";
                        _cimMessageCommandRequest.BODY.TOUCHPANELNO = txtTouchPanelNo.Text;
                        _cimMessageCommandRequest.BODY.MESSAGETEXT = txtMsgText.Text;
                        _cimMessageCommandRequest.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;

                        _xml = _cimMessageCommandRequest.WriteToXml();

                        FormMainMDI.SocketDriver.SendMessage(_cimMessageCommandRequest.HEADER.TRANSACTIONID, _cimMessageCommandRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                        #endregion

                        ShowMessage(this, "CIM Message", "", string.Format("Send CIM Messsage [{0}] to [{1}] Success", txtMsgText.Text.ToString(), ActNodeNo == "00" ? "All EQP" : ActNodeNo), MessageBoxIcon.Information);
                        txtMsgText.Text = string.Empty;
                        txtTouchPanelNo.Text = string.Empty;
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
        
        private void cboLocalNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ActNodeNo = cboLocalNode.Text.Split('-')[0].ToString().Trim();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvMessage_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

                if (dgvMessage.Columns[e.ColumnIndex].Name != "colClear") return;

                string _err = string.Empty;
                string _msgID = dgvMessage[colMessageID.Name,e.RowIndex].Value.ToString();
                string _msgText = dgvMessage[colMessageText.Name, e.RowIndex].Value.ToString();
                string _touchPanel = dgvMessage[colTouchPanelNo.Name, e.RowIndex].Value.ToString();
                string msg = string.Format("Clear Message ID [{0}]", _msgID);

                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text + " Clear", msg)) return;

                CIMMessageCommandRequest _cimMessageCommandRequest = new CIMMessageCommandRequest();
                _cimMessageCommandRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _cimMessageCommandRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _cimMessageCommandRequest.BODY.EQUIPMENTNO = ActNodeNo;
                _cimMessageCommandRequest.BODY.COMMAND = "Clear";
                _cimMessageCommandRequest.BODY.MESSAGEID = _msgID;
                _cimMessageCommandRequest.BODY.MESSAGETEXT = _msgText;
                _cimMessageCommandRequest.BODY.TOUCHPANELNO = _touchPanel;
                _cimMessageCommandRequest.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;

                string _xml = _cimMessageCommandRequest.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_cimMessageCommandRequest.HEADER.TRANSACTIONID, _cimMessageCommandRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                ShowMessage(this, lblCaption.Text , "", msg + " Success", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }
    }
}
