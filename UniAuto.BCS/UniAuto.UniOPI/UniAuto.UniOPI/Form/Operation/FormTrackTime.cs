using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormTrackTime : FormBase
    {
        public FormTrackTime()
        {
            InitializeComponent();
        }

        private void FormTrackTime_Load(object sender, EventArgs e)
        {
            try
            {
                InitialLocalCombox();
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
                SendtoBC_ODFTrackTimeSettingRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                int _num = 0;

                #region Check

                if (cboLocal.SelectedIndex < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Local No", MessageBoxIcon.Warning);
                    return;
                }
               
                foreach (DataGridViewRow _row in dgvData.Rows)
                {
                    if (_row.Cells[colValue.Name].Value == null)
                    {
                        ShowMessage(this, lblCaption.Text, "",string.Format("Value of {0} must have data", _row.Cells[colKey.Name].Value.ToString()), MessageBoxIcon.Warning);
                        return;
                    }

                    if (int.TryParse(_row.Cells[colValue.Name].Value.ToString(), out _num) == false)
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Value of {0} must be number", _row.Cells[colKey.Name].Value.ToString()), MessageBoxIcon.Warning);
                        return;
                    }
                }
                                
                #endregion

                string _nodeNo = ((dynamic)cboLocal.SelectedItem).NodeNo;

                #region Send ODFTrackTimeSettingChangeRequest
                ODFTrackTimeSettingChangeRequest _trx = new ODFTrackTimeSettingChangeRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = _nodeNo;

                #region unit
                string[] _items = chkUnit.CheckedItems.Cast<string>().ToArray();

                if (_items.Count() == 0)
                {
                    ODFTrackTimeSettingChangeRequest.UNITc _unit = new ODFTrackTimeSettingChangeRequest.UNITc();
                    _unit.UNITNO = "0";
                    _trx.BODY.UNITLIST.Add(_unit);
                }
                else
                {
                    string _unitNo = string.Empty;

                    foreach (string _item in _items)
                    {
                        //UNIT[{0}] - {1}
                        int.TryParse(_item.Substring(5, 2), out _num);

                        ODFTrackTimeSettingChangeRequest.UNITc _unit = new ODFTrackTimeSettingChangeRequest.UNITc();
                        _unit.UNITNO = _num.ToString();
                        _trx.BODY.UNITLIST.Add(_unit);
                    }
                }
                #endregion

                #region Delay time
                foreach (DataGridViewRow _row in dgvData.Rows)
                {
                    ODFTrackTimeSettingChangeRequest.DELAYTIMEc _delay = new ODFTrackTimeSettingChangeRequest.DELAYTIMEc();
                    _delay.SEQNO = _row.Cells[colKey.Name].Value.ToString().Substring(4);
                    _delay.VALUE = _row.Cells[colValue.Name].Value.ToString();

                    _trx.BODY.DELAYTIMELIST.Add(_delay);
                }
                #endregion

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region ODFTrackTimeSettingChangeReply
                ShowMessage(this, lblCaption.Text, "", "ODF Track Time Setting Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboLocal_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                chkUnit.Items.Clear();
                dgvData.Rows.Clear();
 
                if (cboLocal.SelectedIndex < 0)  return;

                string nodeNo = ((dynamic)cboLocal.SelectedItem).NodeNo;

                var q = (from unit in FormMainMDI.G_OPIAp.Dic_Unit.Values
                         where unit.NodeNo == nodeNo
                         select unit
                    ).ToList();


                foreach (Unit _unit in q)
                {
                    chkUnit.Items.Add(string.Format("UNIT[{0}] - {1}", _unit.UnitNo.PadLeft(2, '0'), _unit.UnitID));
                }

                dgvData.Rows.Clear();
                dgvData.Rows.Add("Down0","0");
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialLocalCombox()
        {
            try
            {
                var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                         where node.NodeNo.Equals("L7") || node.NodeNo.Equals("L11") || node.NodeNo.Equals("L13") || node.NodeNo.Equals("L14") || node.NodeNo.Equals("L16") || node.NodeNo.Equals("L19")
                         select new
                         {
                             IDNAME = string.Format("{0} - {1} - {2}", node.NodeNo, node.NodeID, node.NodeName),
                             node.NodeNo,
                             node.NodeID
                         }).ToList();

                if (q == null || q.Count == 0)
                    return;

                cboLocal.SelectedIndexChanged -= cboLocal_SelectedIndexChanged;
                cboLocal.DataSource = q;
                cboLocal.DisplayMember = "IDNAME";
                cboLocal.ValueMember = "NODENO";
                cboLocal.SelectedIndex = -1;
                cboLocal.SelectedIndexChanged += cboLocal_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void chkUnit_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                CheckedListBox _chkList = (CheckedListBox)sender;

                _chkList.ItemCheck -= chkUnit_ItemCheck;
                _chkList.SetItemCheckState(e.Index, e.NewValue);
                _chkList.ItemCheck += chkUnit_ItemCheck;

                int _gridCnt = dgvData.Rows.Count;
                int _unitCnt = _chkList.CheckedItems.Count;

                if (_gridCnt > _unitCnt + 1)
                {
                    for (int _rowIndex = _gridCnt - 1; _rowIndex > _unitCnt; _rowIndex--)
                    {
                        dgvData.Rows.RemoveAt(_rowIndex);
                    }
                }
                else if (_gridCnt < _unitCnt + 1)
                {
                    for (int _rowIndex = _gridCnt; _rowIndex < _unitCnt + 1; _rowIndex++)
                    {
                        dgvData.Rows.Add(string.Format("Down{0}",_rowIndex.ToString()), "0");
                    }
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        #region 同步 socket

        public void SendtoBC_ODFTrackTimeSettingRequest()
        {
            try
            {
                #region Send to BC ODFTrackTimeSettingRequest

                ODFTrackTimeSettingRequest _trx = new ODFTrackTimeSettingRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null)
                {
                    txtLocalName_L7.Text = string.Empty;
                    txtLocalName_L13.Text = string.Empty;
                    txtLocalName_L11.Text = string.Empty;
                    txtLocalName_L14.Text = string.Empty;

                    txtUnitNo_L7.Text = string.Empty;
                    txtUnitNo_L13.Text = string.Empty;
                    txtUnitNo_L11.Text = string.Empty;
                    txtUnitNo_L14.Text = string.Empty;

                    dgvDelayTime_L7.Rows.Clear();
                    dgvDelayTime_L13.Rows.Clear();
                    dgvDelayTime_L11.Rows.Clear();
                    dgvDelayTime_L14.Rows.Clear();
                }
                else
                {
                    string _respXml = _resp.Xml;

                    ODFTrackTimeSettingReply _odfTrackTimeSettingReply = (ODFTrackTimeSettingReply)Spec.CheckXMLFormat(_respXml);

                    #region Update Data
                    Node _node = null;
                    foreach (ODFTrackTimeSettingReply.EQUIPMENTc _eq in _odfTrackTimeSettingReply.BODY.EQUIPMENTLIST)
                    {
                        if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_eq.EQUIPMENTNO) == false)
                        {
                            ShowMessage(this, lblCaption.Text , "", string.Format("Can't find Equipment No[{0}]", _eq.EQUIPMENTNO), MessageBoxIcon.Error);
                            continue; 
                        }

                        _node = FormMainMDI.G_OPIAp.Dic_Node[_eq.EQUIPMENTNO];

                        _node.Lst_TrackTimeUnit = new System.Collections.Generic.List<string>();

                        foreach (ODFTrackTimeSettingReply.UNITc _unit in _eq.UNITLIST)
                        {
                            _node.Lst_TrackTimeUnit.Add(_unit.UNITNO);
                        }

                        _node.Dic_TrackDelayTime = new System.Collections.Generic.Dictionary<string, string>();

                        foreach (ODFTrackTimeSettingReply.DELAYTIMEc _time in _eq.DELAYTIMELIST)
                        {
                            _node.Dic_TrackDelayTime.Add(_time.SEQNO, _time.VALUE);
                        }
                    }

                    #endregion

                    #region Update Object Data

                    string _objName = string.Empty;
                    TextBox _txt = null;
                    DataGridView _dgv = null;

                    foreach (Node _n in FormMainMDI.G_OPIAp.Dic_Node.Values.Where(r => r.NodeNo.Equals("L7") || r.NodeNo.Equals("L11") || r.NodeNo.Equals("L13") || r.NodeNo.Equals("L14") || r.NodeNo.Equals("L16") || r.NodeNo.Equals("L19")))
                    {
                        _objName = string.Format("txtLocalName_{0}", _n.NodeNo);
                        _txt = tlpBase.Controls.Find(_objName, true).OfType<TextBox>().First();
                        _txt.Text = string.Format("{0} - {1}", _n.NodeNo, _n.NodeID);


                        _objName = string.Format("txtUnitNo_{0}", _n.NodeNo);
                        _txt = tlpBase.Controls.Find(_objName, true).OfType<TextBox>().First();
                        _txt.Text = string.Join(",", _n.Lst_TrackTimeUnit.ToArray());

                        _objName = string.Format("dgvDelayTime_{0}", _n.NodeNo);
                        _dgv = tlpBase.Controls.Find(_objName, true).OfType<DataGridView>().First();
                        _dgv.Rows.Clear();
                        foreach (string _key in _n.Dic_TrackDelayTime.Keys)
                        {
                            _dgv.Rows.Add(string.Format("Down{0}", _key), _n.Dic_TrackDelayTime[_key]);
                        }
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

        #endregion
    }
}
