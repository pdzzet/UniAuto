using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormAlarmStatus : FormBase
    {
        private Node CurNode = null;
        private Button NowButton = null;
        private bool IsRefreshAll = false ;
        private DataTable dtAlarm = null; 

        public FormAlarmStatus()
        {
            InitializeComponent();
            lblCaption.Text = "Alarm Status";

            dtAlarm = UniTools.InitDt(new string[] { "Alarm ID", "Local Node", "Alarm Level", "Alarm Description" });
            
        }

        private void FormAlarmStatus_Shown(object sender, EventArgs e)
        {
            dgvData.DataSource = dtAlarm;
            InitialButton();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (NowButton != null) NowButton.PerformClick();
        }

        private void InitialButton()
        {
            try
            {
                List<UcAutoButtons.ButtonInfo> lstButtonInfo = new List<UcAutoButtons.ButtonInfo>();
                
                foreach (KeyValuePair<string, Node> kvpNode in FormMainMDI.G_OPIAp.Dic_Node)
                {
                    string strButtonText = string.Format("{0}-{1}", kvpNode.Value.NodeNo, kvpNode.Value.NodeID);
                    lstButtonInfo.Add(new UcAutoButtons.ButtonInfo(kvpNode.Key, strButtonText, btn_Click));
                }

                lstButtonInfo.Add(new UcAutoButtons.ButtonInfo("00", "All EQP", btn_Click));

                ucAutoBtnEqp.CreateButton(lstButtonInfo);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void btn_Click(object sender, EventArgs e)
        {
            try
            {
                NowButton = (Button)sender;

                dtAlarm.Rows.Clear();
                string strErrMsg = string.Empty;
                string _xml = string.Empty;

                if (NowButton.Name == "00") //表示all EQ
                {
                    IsRefreshAll = true; 
                    CurNode = null;

                    Send_EquipmentAlarmStatusRequest("00");
                }
                else
                {
                    if (!FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(NowButton.Name))
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Can't find Equipment[{0}]", NowButton.Name), MessageBoxIcon.Error);
                        return;
                    }

                    IsRefreshAll = false;
                    CurNode = FormMainMDI.G_OPIAp.Dic_Node[NowButton.Name];

                    Send_EquipmentAlarmStatusRequest(CurNode.NodeNo);
                }               
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void DisplayAlarmStatus()
        {
            try
            {
                if (IsRefreshAll)
                {
                    #region Refresh All EQ Alarm
                    int _alarmCnt = 0;

                    foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                    {
                        _alarmCnt = _alarmCnt + _node.BC_EquipmentAlarmStatusReply.Lst_RealAlarm.Count;
                    }

                    int rowIndex = 0;

                    if (_alarmCnt < dtAlarm.Rows.Count)
                    {
                        // 新取得的資料筆數比原來資料少時，將原來DataTable多出來的筆數刪掉
                        for (int x = dtAlarm.Rows.Count - 1; x >= _alarmCnt; x--)
                        {
                            dtAlarm.Rows[x].Delete();
                        }

                        foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                        {
                            foreach (Alarm _alarm in _node.BC_EquipmentAlarmStatusReply.Lst_RealAlarm)
                            {
                                dtAlarm.Rows[rowIndex][0] = _alarm.AlarmID;
                                dtAlarm.Rows[rowIndex][1] = _node.NodeNo + " - " + _node.NodeID;
                                dtAlarm.Rows[rowIndex][2] = _alarm.AlarmLevel;
                                dtAlarm.Rows[rowIndex][3] = _alarm.AlarmText;

                                rowIndex++;
                            }
                        }                    
                    }
                    else
                    {
                        DataRow drCurr = null;
                        foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                        {
                            foreach (Alarm _alarm in _node.BC_EquipmentAlarmStatusReply.Lst_RealAlarm)
                            {
                                if (rowIndex < dtAlarm.Rows.Count)
                                {
                                    drCurr = dtAlarm.Rows[rowIndex];

                                    drCurr[0] = _alarm.AlarmID;
                                    drCurr[1] = _node.NodeNo + " - " + _node.NodeID;
                                    drCurr[2] = _alarm.AlarmLevel;
                                    drCurr[3] = _alarm.AlarmText;
                                }
                                else
                                {
                                    // 新取得的資料筆數比原來資料多時，將多出的資料加入DataTable
                                    DataRow drNew = dtAlarm.NewRow();
                                    drNew[0] = _alarm.AlarmID;
                                    drNew[1] = _node.NodeNo + " - " + _node.NodeID;
                                    drNew[2] = _alarm.AlarmLevel;
                                    drNew[3] = _alarm.AlarmText;
                                    dtAlarm.Rows.Add(drNew);
                                }

                                rowIndex++;
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Refresh EQ Alarm
                    if (CurNode == null) return;

                    Alarm[] Cur_Alarm = CurNode.BC_EquipmentAlarmStatusReply.Lst_RealAlarm.ToArray();

                    int rowIndex = 0;

                    if (Cur_Alarm.Length < dtAlarm.Rows.Count)
                    {
                        // 新取得的資料筆數比原來資料少時，將原來DataTable多出來的筆數刪掉
                        for (int x = dtAlarm.Rows.Count - 1; x >= Cur_Alarm.Length; x--)
                        {
                            dtAlarm.Rows[x].Delete();
                        }

                        foreach (Alarm _alarm in Cur_Alarm)
                        {
                            dtAlarm.Rows[rowIndex][0] = _alarm.AlarmID;
                            dtAlarm.Rows[rowIndex][1] = CurNode.NodeNo + " - " + CurNode.NodeID;
                            dtAlarm.Rows[rowIndex][2] = _alarm.AlarmLevel;
                            dtAlarm.Rows[rowIndex][3] = _alarm.AlarmText;

                            rowIndex++;
                        }
                    }
                    else
                    {
                        DataRow drCurr = null;
                        foreach (Alarm _alarm in Cur_Alarm)
                        {
                            if (rowIndex < dtAlarm.Rows.Count)
                            {
                                drCurr = dtAlarm.Rows[rowIndex];

                                drCurr[0] = _alarm.AlarmID;
                                drCurr[1] = CurNode.NodeNo + " - " + CurNode.NodeID;
                                drCurr[2] = _alarm.AlarmLevel;
                                drCurr[3] = _alarm.AlarmText;
                            }
                            else
                            {
                                // 新取得的資料筆數比原來資料多時，將多出的資料加入DataTable
                                DataRow drNew = dtAlarm.NewRow();
                                drNew[0] = _alarm.AlarmID;
                                drNew[1] = CurNode.NodeNo + " - " + CurNode.NodeID;
                                drNew[2] = _alarm.AlarmLevel;
                                drNew[3] = _alarm.AlarmText;
                                dtAlarm.Rows.Add(drNew);
                            }

                            rowIndex++;
                        }
                    }
                    #endregion
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

                #region Send Request
                bool _isSend = false ;

                if (IsRefreshAll)
                {
                    foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                    {
                        DateTime _now = DateTime.Now;

                        TimeSpan _ts = _now.Subtract(_node.BC_EquipmentAlarmStatusReply.LastRequestDate).Duration();

                        if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                        {
                            if (_node.BC_EquipmentAlarmStatusReply.IsReply)
                            {
                                _isSend = true;

                                break;
                            }
                        }
                    }

                    if (_isSend) Send_EquipmentAlarmStatusRequest("00");
                }
                else
                {
                    if (CurNode != null) 
                    {
                        DateTime _now = DateTime.Now;

                        TimeSpan _ts = _now.Subtract(CurNode.BC_EquipmentAlarmStatusReply.LastRequestDate).Duration();

                        if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                        {

                            if (CurNode.BC_EquipmentAlarmStatusReply.IsReply)
                            {
                                Send_EquipmentAlarmStatusRequest(CurNode.NodeNo);
                            }
                        }
                    }
                }
                #endregion

                DisplayAlarmStatus();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public void Send_EquipmentAlarmStatusRequest(string NodeNo)
        {
            try
            {
                string _err = string.Empty;
                EquipmentAlarmStatusRequest _trx = new EquipmentAlarmStatusRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = NodeNo;

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), out _err, FormMainMDI.G_OPIAp.SessionID);

                if (NodeNo == "00")
                {
                    foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                    {
                        _node.BC_EquipmentAlarmStatusReply.IsReply = false;

                        _node.BC_EquipmentAlarmStatusReply.LastRequestDate = DateTime.Now;
                    }
                }
                else
                {
                    CurNode.BC_EquipmentAlarmStatusReply.IsReply = false;

                    CurNode.BC_EquipmentAlarmStatusReply.LastRequestDate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

    }
}
