using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormEachPosition : FormBase
    {
        private class NodeItem
        {
            public string NODENO = string.Empty;
            public string NODEID = string.Empty;
            public string NODENAME = string.Empty;
            public override string ToString()
            {
                return string.Format("{0}-{1}-{2}", NODENO, NODEID, NODENAME);
            }
            public List<UnitItem> Units = new List<UnitItem>();
        }

        private class UnitItem
        {
            public string UnitID = string.Empty;
            public string UnitNo = string.Empty;
            public string UnitType;

            public override string ToString()
            {
                return string.Format("{0}-{1}", UnitNo, UnitID);
            }
        }

        BCS_EachPositionReply Cur_BCS_EachPositionReply;

        public FormEachPosition()
        {
            InitializeComponent();

            lblCaption.Text = "Each Position";
        }

        private void FormEachPosition_Load(object sender, EventArgs e)
        {
            InitialCombox_Node();

            Cur_BCS_EachPositionReply = null;

            #region CBUAM line glass id改顯示 mask id
            if (FormMainMDI.G_OPIAp.CurLine.ServerName == "CBUAM100")
                dgvData.Columns[colJobID.Name].HeaderText = "Mask ID";     
            else
                dgvData.Columns[colJobID.Name].HeaderText = "Job ID";               
            #endregion
            
        }

        private void cmbNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cmbUnit.Items.Clear();
                if (cmbNode.SelectedItem == null) return;

                 if (cmbNode.SelectedIndex < 0)  return;

                string _nodeNo = ((NodeItem)cmbNode.SelectedItem).NODENO;

                UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBCtx;

                var q = (from p in ctx.SBRM_POSITION
                        where ((from n in ctx.SBRM_NODE where n.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName select n.LINEID).Contains(p.LINEID)) && (p.UNITTYPE == "E" || p.UNITTYPE == "U") && p.NODENO == _nodeNo
                        select new  {NodeNo = p.NODENO, UnitNo = p.UNITNO, UnitType = p.UNITTYPE}).Distinct();

                SortedDictionary<string, UnitItem> units = new SortedDictionary<string, UnitItem>();

                foreach (var position in q)
                {
                    string unit_no = position.UnitNo.PadLeft(2, '0');

                    if (units.ContainsKey(unit_no)) continue;
                    
                    if (position.UnitType == "E" && position.UnitNo == "0")
                    {
                        //主node item
                        UnitItem unit_item = new UnitItem();
                        unit_item.UnitID = "Equipment";
                        unit_item.UnitNo = position.UnitNo;
                        unit_item.UnitType = position.UnitType;
                        units.Add(unit_no, unit_item);
                    }
                    else 
                    {
                        string key = string.Format("{0}{1}", position.NodeNo.PadRight(3, ' '), unit_no);

                        if (FormMainMDI.G_OPIAp.Dic_Unit.ContainsKey(key))
                        {
                            UnitItem unit_item = new UnitItem();
                            unit_item.UnitID = FormMainMDI.G_OPIAp.Dic_Unit[key].UnitID;
                            unit_item.UnitNo = position.UnitNo;
                            unit_item.UnitType = position.UnitType;
                            units.Add(unit_no, unit_item);
                        }
                        else
                        {
                            UnitItem unit_item = new UnitItem();
                            unit_item.UnitID = "";
                            unit_item.UnitNo = position.UnitNo;
                            unit_item.UnitType = position.UnitType;
                            units.Add(unit_no, unit_item);
                        }
                    }
                }

                foreach (UnitItem unit_item in units.Values)
                    cmbUnit.Items.Add(unit_item);

                if(cmbUnit.Items.Count > 0)
                    cmbUnit.SelectedIndex = 0;

                if (cmbUnit.Items.Count > 0) cmbUnit.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void cmbUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            QueryData();
        }
      
        private void dgvData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;

                if (dgvData.CurrentRow != null)
                {
                    string _cstSeqNo = dgvData.CurrentRow.Cells[colCassetteSeqNo.Name].Value.ToString();
                    string _jobSeqNo = dgvData.CurrentRow.Cells[colJobSeqNo.Name].Value.ToString();
                    string _glassID = dgvData.CurrentRow.Cells[colJobID.Name].Value.ToString();

                    if ((_cstSeqNo == string.Empty && _jobSeqNo == string.Empty) || (_cstSeqNo == "0" && _jobSeqNo == "0"))
                    {
                        ShowMessage(this, this.lblCaption.Text, "", "Cassette Seq No、Job Seq No must be Required！", MessageBoxIcon.Warning);
                        return;
                    }

                    new FormJobDataDetail(_glassID, _cstSeqNo, _jobSeqNo) { TopMost = true }.ShowDialog();
                }                          
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);               
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            QueryData();
        }

        private void QueryData()
        {
            try
            {
                if (cmbNode.SelectedItem != null )
                {
                    string _nodeNo = ((NodeItem)cmbNode.SelectedItem).NODENO;
                    string _unitNo = cmbUnit.SelectedItem == null ? "0" : ((UnitItem)cmbUnit.SelectedItem).UnitNo;
                    string _unitType = cmbUnit.SelectedItem == null ? "E" : ((UnitItem)cmbUnit.SelectedItem).UnitType;

                    SetGridViewData(_nodeNo, _unitNo,_unitType);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

     
        private void InitialCombox_Node()
        {
            try
            {
                string current_line_id = FormMainMDI.G_OPIAp.CurLine.LineID;

                UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBCtx;

                var q = from p in ctx.SBRM_POSITION
                         where ((from n in ctx.SBRM_NODE where n.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName select n.LINEID).Contains(p.LINEID)) && (p.UNITTYPE == "E" || p.UNITTYPE == "U")
                         select p;

                SortedDictionary<string, NodeItem> nodes = new SortedDictionary<string, NodeItem>();

                foreach (SBRM_POSITION position in q)
                {
                    string node_no = position.NODENO.Substring(1).PadLeft(2, '0');

                    if (nodes.ContainsKey(node_no))
                        continue;

                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(position.NODENO))
                    {
                        NodeItem node_item = new NodeItem();
                        node_item.NODENO = position.NODENO;
                        node_item.NODEID = FormMainMDI.G_OPIAp.Dic_Node[position.NODENO].NodeID;
                        node_item.NODENAME = FormMainMDI.G_OPIAp.Dic_Node[position.NODENO].NodeName;
                        nodes.Add(node_no, node_item);
                    }
                }

                cmbNode.SelectedIndexChanged -= new EventHandler(cmbNode_SelectedIndexChanged);
                cmbNode.Items.Clear();

                foreach (NodeItem node_item in nodes.Values)
                    cmbNode.Items.Add(node_item);

                cmbNode.SelectedIndex = -1;
                cmbNode.SelectedIndexChanged += new EventHandler(cmbNode_SelectedIndexChanged);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetGridViewData(string nodeNo, string unitNo,string unitType)
        {
            try
            {
                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;

                string _positionUnitNo = unitNo.PadLeft(2,'0');
             
                if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(nodeNo))
                {
                    Node _node = FormMainMDI.G_OPIAp.Dic_Node[nodeNo];

                    ////Key: position unit no (兩碼)
                    if (_node.Dic_Position.ContainsKey(_positionUnitNo) == false)
                    {
                    var _data = (from u in _ctx.SBRM_NODE
                             join n in _ctx.SBRM_POSITION on new { NODENO = u.NODENO } equals new { NODENO = n.NODENO }
                             where  u.LINEID == n.LINEID &&  u.NODENO == n.NODENO && n.UNITTYPE == unitType &&  n.UNITNO == unitNo && u.NODENO == nodeNo  && u.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName 
                             orderby n.POSITIONNO ascending
                             select new PositionInfo
                             {
                                 PositionNo = n.POSITIONNO,
                                 PositionName = n.POSITIONNAME,                                 
                                 CassetteSeqNo = "",
                                 JobSeqNo = "",
                                 JobID = "",
                             }).Distinct();

                        DataTable _dt = DBConnect.ToDataTable(_data);

                        if (_dt == null || _dt.Rows.Count == 0)
                        {
                            ShowMessage(this, lblCaption.Text, "", string.Format("Node No [{0}], Unit No [{1}] Can't find position in SBRM_POSITION", nodeNo, unitNo), MessageBoxIcon.Information);
                            return;
                        }

                        BCS_EachPositionReply _eachPositionReply = new BCS_EachPositionReply ();

                        _eachPositionReply.PositionNodeNo = nodeNo;
                        _eachPositionReply.PositionUnitNo_DB = unitNo;
                        _eachPositionReply.PositionUnitNo = _positionUnitNo;
                        _eachPositionReply.PositionUnitType = unitType;
                        if (_node.LineID.Substring(0, 2).Contains("TC"))
                            _eachPositionReply.PositionTrxNo =
                            (unitNo.PadLeft(2, '0') == "00" ? "00" : (from y in _ctx.SBRM_UNIT
                                                                      where y.UNITNO == unitNo && y.NODENO == nodeNo && y.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                                                                      select y.POSITIONPLCTRXNO).Distinct().ToList()[0]);//Yang 20160921 Select POSITIONPLCTRXNO From SBRM_UNIT by UNITNO  For Array

                        else
                            _eachPositionReply.PositionTrxNo = (unitNo.PadLeft(2, '0') == "00" ? "01" : _positionUnitNo);

                        foreach (PositionInfo _p in _data)
                        {
                            _eachPositionReply.Lst_Position.Add(_p);                            
                        }

                        _node.Dic_Position.Add(_positionUnitNo,_eachPositionReply);
                    }

                    Cur_BCS_EachPositionReply = _node.Dic_Position[_positionUnitNo];

                    dgvData.Rows.Clear();

                    foreach (PositionInfo _p in Cur_BCS_EachPositionReply.Lst_Position)
                    {
                        dgvData.Rows.Add(nodeNo, unitNo, Cur_BCS_EachPositionReply.PositionTrxNo, _p.PositionNo, _p.PositionName, _p.CassetteSeqNo, _p.JobSeqNo, _p.JobID);
                    }

                    if (dgvData.Rows.Count > 0) dgvData.Sort(dgvData.Columns[colPositionNo.Name], ListSortDirection.Ascending);
                    Send_EachPositionRequest(Cur_BCS_EachPositionReply);
                }
                else
                {
                    Cur_BCS_EachPositionReply = null;
                    ShowMessage(this, lblCaption.Text, "", string.Format("Can't find Node No [{0}] ,Unit No [{1}]", nodeNo, unitNo), MessageBoxIcon.Warning);
                    return;
                }
                tmrRefresh.Enabled = true;             
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_EachPositionRequest(BCS_EachPositionReply Position)
        {
            try
            {
                string _err = string.Empty;
                string _xml = string.Empty;

                #region Send EachPositionRequest

                EachPositionRequest _trx = new EachPositionRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = Position.PositionNodeNo;
                _trx.BODY.UNITNO =  Position.PositionUnitNo_DB;
                _trx.BODY.PLCTRXNO = (Position.PositionUnitNo == "00" ? "01" : Position.PositionTrxNo);
                _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                Position.LastRequestDate = DateTime.Now;

                Position.IsReply = false;

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

                if (Cur_BCS_EachPositionReply == null)
                {
                    tmrRefresh.Enabled = false;

                    dgvData.Rows.Clear();

                    return;
                }

                if (Cur_BCS_EachPositionReply.IsReply)
                {
                    DateTime _now = DateTime.Now;
                    TimeSpan _ts = _now.Subtract(Cur_BCS_EachPositionReply.LastRequestDate).Duration();

                    if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                    {
                        Send_EachPositionRequest(Cur_BCS_EachPositionReply);
                    }
                }

                SetJobData();

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetJobData()
        {
            try
            {
                string _key = string.Empty ;
                int _positionNo = 0;

                foreach (DataGridViewRow _row in dgvData.Rows)
                {
                    PositionInfo _position = null;

                    //SECS部分機台不會上報position no,而是上報position name，所以需改成position name查詢顯示結果
                    if (Cur_BCS_EachPositionReply.ReportPositionName)
                    {
                        _key = _row.Cells[colPositionName.Name].Value.ToString();

                        _position = Cur_BCS_EachPositionReply.Lst_Position.Find(r => r.PositionName.Equals(_key));
                    }
                    else
                    {
                        _key = _row.Cells[colPositionNo.Name].Value.ToString();

                        if (int.TryParse(_key, out _positionNo) == false) continue;

                        _position = Cur_BCS_EachPositionReply.Lst_Position.Find(r => r.PositionNo.Equals(_positionNo));
                       
                    }


                    if (_position != null)
                    {
                        _row.Cells[colCassetteSeqNo.Name].Value = _position.CassetteSeqNo;
                        _row.Cells[colJobSeqNo.Name].Value = _position.JobSeqNo;
                        _row.Cells[colJobID.Name].Value = _position.JobID;
                    }
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
