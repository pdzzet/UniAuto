using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormSlotPosition : FormBase
    {
        Port CurPort;

        bool IsInitialCbo = false;

        public FormSlotPosition()
        {
            InitializeComponent();
        }

        private void FormSlotPosition_Load(object sender, EventArgs e)
        {
            CurPort = null;

            InitialCombox_Node();

            #region CBUAM line glass id改顯示 mask id
            if (FormMainMDI.G_OPIAp.CurLine.ServerName == "CBUAM100")
                dgvData.Columns[colJobID.Name].HeaderText = "Mask ID";
            else
                dgvData.Columns[colJobID.Name].HeaderText = "Job ID";
            #endregion
        }

        private void InitialCombox_Node()
        {
            try
            {
                var q = (from port in FormMainMDI.G_OPIAp.Dic_Port.Values
                         join node in FormMainMDI.G_OPIAp.Dic_Node.Values on port.NodeNo equals node.NodeNo
                         select new
                         {
                             IdNmae = string.Format("{0}-{1}-{2}", node.NodeNo, node.NodeID, node.NodeName),
                             NodeNo = node.NodeNo,
                             NodeID = node.NodeID
                         }).Distinct().ToList();

                if (q == null || q.Count == 0) return;

                cboNode.SelectedIndexChanged -= cboNode_SelectedIndexChanged;
                cboNode.DataSource = q;
                cboNode.DisplayMember = "IdNmae";
                cboNode.ValueMember = "NodeNo";
                cboNode.SelectedIndex = -1;
                cboNode.SelectedIndexChanged += cboNode_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cboPort.DataSource = null;

                if (cboNode.SelectedIndex < 0)  return;

                string nodeID = ((dynamic)cboNode.SelectedItem).NodeID;

                var q = (from port in FormMainMDI.G_OPIAp.Dic_Port.Values
                         where port.NodeID == nodeID
                         select port
                    ).ToList();

                if (q == null || q.Count == 0)
                {
                    cboPort.DataSource = null;
                    return;
                }

                cboPort.SelectedIndexChanged -= new EventHandler(cboPort_SelectedIndexChanged);
                cboPort.DataSource = q;
                cboPort.DisplayMember = "PORTID";
                cboPort.ValueMember = "PORTID";
                cboPort.SelectedIndex = -1;
                cboPort.SelectedIndexChanged += new EventHandler(cboPort_SelectedIndexChanged);

                if (IsInitialCbo==false ) cboPort.SelectedIndex = 0;
                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void cboPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            QueryData();
        }

        private void QueryData()
        {
            try
            {
                if (cboNode.SelectedItem != null && cboPort.SelectedItem != null)
                {
                    string nodeNo = ((dynamic)cboNode.SelectedItem).NodeNo;
                    string portID = ((Port)cboPort.SelectedItem).PortID;
                    string portNo = ((Port)cboPort.SelectedItem).PortNo;

                    SetGridViewData(nodeNo, portNo, portID);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetGridViewData(string nodeNo, string portNo, string portID)
        {
            try
            {
                string _portKey = nodeNo.PadRight(3, ' ') + portNo.PadRight(2, ' ');

                if (FormMainMDI.G_OPIAp.Dic_Port.ContainsKey(_portKey))
                {
                    CurPort = FormMainMDI.G_OPIAp.Dic_Port[_portKey];

                    CurPort.BC_SlotPositionReply.IsReply = true;

                    if (!CurPort.BC_SlotPositionReply.IsLoadData)
                    {
                        CurPort.BC_SlotPositionReply.IsLoadData = true;

                        UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;

                        var _data = (from p in _ctx.SBRM_PORT
                                     join n in _ctx.SBRM_POSITION on new { NODENO = p.NODENO, PORTNO = p.PORTNO } equals new { NODENO = n.NODENO, PORTNO = n.UNITNO }
                                     where p.NODENO == nodeNo && p.PORTID == portID && n.UNITTYPE == "P" && p.PORTNO == n.UNITNO && p.NODENO == n.NODENO && n.LINEID == p.LINEID && p.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                                     orderby n.POSITIONNO ascending
                                     select new SlotPosition
                                     {
                                         NodeNo = p.NODENO,
                                         PortNo = p.PORTNO,
                                         PositionDesc = string.Format("{0}:{1}", n.POSITIONNO, n.POSITIONNAME),
                                         PositionName = n.POSITIONNAME,
                                         PositionNo = n.POSITIONNO,
                                         CassetteSeqNo = "",
                                         JobSeqNo = "",
                                         JobID = "",
                                         RecipeName = "",
                                         PPID = "",
                                         TrackingValue = "",
                                         SamplingSlotFlag = ""
                                     }).Distinct();

                        DataTable _dt = DBConnect.ToDataTable(_data);

                        if (_dt == null || _dt.Rows.Count == 0)
                        {
                            ShowMessage(this, lblCaption.Text, "", string.Format("Node No [{0}], Port No [{1}] Can't find slot position in SBRM_POSITION", nodeNo, portNo), MessageBoxIcon.Information);
                            return;
                        }


                        foreach (SlotPosition _p in _data)
                        {
                            CurPort.BC_SlotPositionReply.Lst_SlotPosition.Add(_p);
                        }
                    }

                    dgvData.Rows.Clear();
                    foreach (SlotPosition _p in CurPort.BC_SlotPositionReply.Lst_SlotPosition)
                    {
                        dgvData.Rows.Add(_p.NodeNo, _p.PortNo, _p.PositionDesc, _p.PositionName, _p.PositionNo, _p.CassetteSeqNo, _p.JobSeqNo, _p.JobID, _p.RecipeName, _p.PPID, _p.TrackingValue, _p.SamplingSlotFlag);
                        AdjustGridViewRowColor(_p.TrackingValue, _p.SamplingSlotFlag);
                    }

                    Send_SlotPositionRequest(CurPort);
                }
                else
                {
                    CurPort = null;
                    ShowMessage(this, lblCaption.Text, "", string.Format("Can't find Node No [{0}] ,Port No [{1}]", nodeNo, portNo), MessageBoxIcon.Warning);
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

        private void btnQuery_Click(object sender, EventArgs e)
        {
            QueryData();
        }

        private void SetJobData()
        {
            try
            {
                int _positionNo = 0;

                foreach (DataGridViewRow _row in dgvData.Rows)
                {
                    if (int.TryParse(_row.Cells[colPositionNo.Name].Value.ToString(), out _positionNo) == false) continue;

                    SlotPosition _slot = CurPort.BC_SlotPositionReply.Lst_SlotPosition.Find(r => r.PositionNo.Equals(_positionNo));

                    if (_slot != null)
                    {
                        _row.Cells[colCSTSeqNo.Name].Value = _slot.CassetteSeqNo;
                        _row.Cells[colJobSeqNo.Name].Value = _slot.JobSeqNo;
                        _row.Cells[colJobID.Name].Value = _slot.JobID;
                        _row.Cells[colTrackingValue.Name].Value = _slot.TrackingValue;
                        _row.Cells[colSamplingSlotFlag.Name].Value = _slot.SamplingSlotFlag;
                        _row.Cells[colRecipeName.Name].Value = _slot.RecipeName;
                        _row.Cells[colPPID.Name].Value = _slot.PPID;

                        _row.DefaultCellStyle.BackColor = AdjustGridViewRowColor(_slot.TrackingValue, _slot.SamplingSlotFlag);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private Color AdjustGridViewRowColor(string trackingValue, string samplingSlotFlag)
        {
            try
            {
                #region setting color

                if (trackingValue.Contains("2"))//含 Abnormal
                {
                    return Color.Red;
                }
                else if (trackingValue.Contains("1"))//含 Normal
                {
                    return Color.Green;
                }
                else if (samplingSlotFlag.Equals("0"))
                {
                    return Color.Gray;
                }
                else if (samplingSlotFlag.Equals("1"))
                {
                    return Color.Yellow;
                }

                return Color.White;

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return Color.White;
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

                if (CurPort == null)
                {
                    tmrRefresh.Enabled = false;

                    dgvData.Rows.Clear();

                    return;
                }

                if (CurPort.BC_SlotPositionReply.IsReply)
                {
                    DateTime _now = DateTime.Now;
                    TimeSpan _ts = _now.Subtract(CurPort.BC_SlotPositionReply.LastRequestDate).Duration();

                    if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                    {
                        Send_SlotPositionRequest(CurPort);
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

        private void Send_SlotPositionRequest(Port port)
        {
            try
            {
                string _err = string.Empty;
                string _xml = string.Empty;

                SlotPositionRequest request = new SlotPositionRequest();
                request.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                request.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                request.BODY.EQUIPMENTNO = port.NodeNo;
                request.BODY.PORTNO = port.PortNo;
                request.BODY.PORTID = port.PortID;
                _xml = request.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(request.HEADER.TRANSACTIONID, request.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                CurPort.BC_SlotPositionReply.LastRequestDate = DateTime.Now;

                port.BC_SlotPositionReply.IsReply = false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public void SetSelectedNodePort(string nodeID, string portID)
        {
            try
            {
                #region Set Current Node

                IsInitialCbo = true;
                cboNode.SelectedItem = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                                        where node.NodeID == nodeID
                                        select new
                                        {
                                            IdNmae = string.Format("{0}-{1}-{2}", node.NodeNo, node.NodeID, node.NodeName),
                                            NodeNo = node.NodeNo,
                                            NodeID = node.NodeID
                                        }).First();


                #endregion

                IsInitialCbo = false;

                #region Set Current port
                cboPort.SelectedIndex = -1;
                cboPort.SelectedItem = (from port in FormMainMDI.G_OPIAp.Dic_Port.Values
                                        where port.PortID == portID
                                        select port
                    ).First();
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;

                if (dgvData.CurrentRow != null)
                {
                    string _cstSeqNo = dgvData.CurrentRow.Cells[colCSTSeqNo.Name].Value.ToString();
                    string _jobSeqNo = dgvData.CurrentRow.Cells[colJobSeqNo.Name].Value.ToString();
                    string _glassID = dgvData.CurrentRow.Cells[colJobID.Name].Value.ToString();

                    if ((_cstSeqNo == string.Empty && _jobSeqNo == string.Empty) || (_cstSeqNo =="0" && _jobSeqNo == "0"))
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

    }
}
