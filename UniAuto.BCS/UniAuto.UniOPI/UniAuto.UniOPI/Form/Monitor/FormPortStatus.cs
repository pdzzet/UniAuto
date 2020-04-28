using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormPortStatus : FormBase
    {
        Port CurPort;
        bool IsInitialCbo = false;

        public FormPortStatus()
        {
            InitializeComponent();
        }

        private void FormPortStatus_Load(object sender, EventArgs e)
        {
            CurPort = null;

            InitialCombox();

            #region Only For Array/CF Use
            if (FormMainMDI.G_OPIAp.CurLine.FabType == "CELL")
            {
                pnlPortGrade.Visible = false;
                pnlPortProductType.Visible = false;
            }
            else
            {
                pnlPortGrade.Visible = true;
                pnlPortProductType.Visible = true;
            }
            #endregion


            #region ARRAY 顯示 Process Type
            if (FormMainMDI.G_OPIAp.CurLine.FabType == "ARRAY")
            {
                pnlPortProcessType.Visible = true;
            }
            else
            {
                pnlPortProcessType.Visible = false;
            }
            #endregion

            #region 判斷有兩條lineid 時再顯示line id
            if (FormMainMDI.G_OPIAp.CurLine.LineID2 == string.Empty)
            {
                pnlLineID.Visible = false;
            }
            else
            {
                pnlLineID.Visible = true;
            }
            #endregion
            
            dgvData.CellDoubleClick += new DataGridViewCellEventHandler(dgvData_CellDoubleClick);
            btnQuery.Click +=new EventHandler(btnQuery_Click);
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

        private void cboNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cboPort.DataSource = null;

                if (cboNode.SelectedIndex < 0) return;

                string nodeNo = ((dynamic)cboNode.SelectedItem).NodeNo;

                var q = (from port in FormMainMDI.G_OPIAp.Dic_Port.Values
                         where port.NodeNo == nodeNo
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
                cboPort.ValueMember = "PORTNO";
                cboPort.SelectedIndex = -1;
                cboPort.SelectedIndexChanged += new EventHandler(cboPort_SelectedIndexChanged);

                if (IsInitialCbo == false) cboPort.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                QueryData();
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

                if (CurPort == null)
                {
                    tmrBaseRefresh.Enabled = false;

                    ClearData();

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

                SetPortData();
                SetJobData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                QueryData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialCombox()
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
                cboNode.SelectedIndexChanged += new EventHandler(cboNode_SelectedIndexChanged);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ClearData()
        {
            try
            {
                CurPort = null;

                txtLocalNo.Text = string.Empty;
                txtPortID.Text = string.Empty;
                txtCSTSeqNo.Text = string.Empty;
                txtCSTType.Text = string.Empty;
                txtPortEnable.Text = string.Empty;
                txtPortMode.Text = string.Empty;
                txtPortStatus.Text = string.Empty;
                txtPortType.Text = string.Empty;
                txtTransferMode.Text = string.Empty;

                dgvData.Rows.Clear();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
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
                bool _haveWip = false ;
                string _portKey = nodeNo.PadRight(3, ' ') + portNo.PadRight(2, ' ');

                if (FormMainMDI.G_OPIAp.Dic_Port.ContainsKey(_portKey))
                {
                    CurPort = FormMainMDI.G_OPIAp.Dic_Port[_portKey];

                    CurPort.BC_SlotPositionReply.IsReply = true;

                    if (!CurPort.BC_SlotPositionReply.IsLoadData)
                    {
                        CurPort.BC_SlotPositionReply.IsLoadData = true;

                        for (int i = 1; i <= CurPort.MaxCount; i++)
                        {
                            SlotPosition _position = new SlotPosition();

                            _position.NodeNo = CurPort.NodeNo;
                            _position.PortNo = CurPort.PortNo;
                            _position.PositionDesc = string.Format("Port{0} Slot #{1}", CurPort.PortNo.PadLeft(2, '0'), i.ToString().PadLeft(3,'0'));  //Port01 Slot #001
                            _position.PositionName = string.Format("Port{0} Slot #{1}", CurPort.PortNo.PadLeft(2, '0'), i.ToString().PadLeft(3, '0'));
                            _position.PositionNo = i;
                            _position.CassetteSeqNo = "";
                            _position.JobSeqNo = "";
                            _position.JobID = "";
                            _position.RecipeName = "";
                            _position.PPID="";
                            _position.TrackingValue = "";
                            _position.SamplingSlotFlag = "";
                            _position.EQPRTCFlag = "";

                            CurPort.BC_SlotPositionReply.Lst_SlotPosition.Add(_position);

                        }
                        //UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;

                        //var _data = (from p in _ctx.SBRM_PORT
                        //             join n in _ctx.SBRM_POSITION on new { NODENO = p.NODENO, PORTNO = p.PORTNO } equals new { NODENO = n.NODENO, PORTNO = n.UNITNO }
                        //             where p.NODENO == nodeNo && p.PORTID == portID && n.UNITTYPE == "P" && p.PORTNO == n.UNITNO && p.NODENO == n.NODENO && n.LINEID == CurPort.LineID
                        //             orderby n.POSITIONNO ascending
                        //             select  new SlotPosition
                        //             {
                        //                 NodeNo = p.NODENO,
                        //                 PortNo = p.PORTNO,
                        //                 PositionDesc = string.Format("{0}:{1}", n.POSITIONNO, n.POSITIONNAME),
                        //                 PositionName = n.POSITIONNAME,
                        //                 PositionNo = n.POSITIONNO,
                        //                 CassetteSeqNo = "",
                        //                 JobSeqNo = "",
                        //                 JobID = "",
                        //                 RecipeName = "",
                        //                 PPID="",
                        //                 TrackingValue = "",
                        //                 SamplingSlotFlag = ""
                        //             }).Distinct();

                        //DataTable _dt = DBConnect.ToDataTable(_data);

                        //if (_dt == null || _dt.Rows.Count == 0)
                        //{
                        //    ShowMessage(this, lblCaption.Text, "", string.Format("Node No [{0}], Port No [{1}] Can't find slot position in SBRM_POSITION", nodeNo, portNo), MessageBoxIcon.Information);
                        //    return;
                        //}

                        //foreach (SlotPosition _p in _data)
                        //{
                        //    CurPort.BC_SlotPositionReply.Lst_SlotPosition.Add(_p);
                        //}
                    }

                    #region Set Data
                    txtLocalNo.Text = CurPort.NodeNo;
                    txtPortID.Text = CurPort.PortID;
                    SetPortData();

                    dgvData.Rows.Clear();
                    foreach (SlotPosition _p in CurPort.BC_SlotPositionReply.Lst_SlotPosition)
                    {
                        dgvData.Rows.Add(_p.NodeNo, _p.PortNo, _p.PositionDesc, _p.PositionName, _p.PositionNo, _p.CassetteSeqNo, _p.JobSeqNo, _p.JobID, _p.RecipeName, _p.PPID, _p.TrackingValue, _p.SamplingSlotFlag);

                        if ((_p.CassetteSeqNo == string.Empty && _p.JobSeqNo == string.Empty) || (_p.CassetteSeqNo == "0" && _p.JobSeqNo == "0")) _haveWip = false;
                        else _haveWip = true;

                        AdjustGridViewRowColor(_p.TrackingValue, _p.SamplingSlotFlag,_p.EQPRTCFlag, _haveWip);
                    }
                    #endregion

                    Send_SlotPositionRequest(CurPort);
                }
                else
                {
                    CurPort = null;
                    ShowMessage(this, lblCaption.Text, "", string.Format("Can't find Node No [{0}] ,Port No [{1}]", nodeNo, portNo), MessageBoxIcon.Warning);
                    return;
                }

                tmrBaseRefresh.Enabled = true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetPortData()
        {
            try
            {
                if (CurPort == null) return;
                txtLineID.Text = CurPort.LineID;
                txtLocalNo.Text = CurPort.NodeNo;
                txtPortID.Text = CurPort.PortID;
                txtCSTSeqNo.Text = CurPort.CassetteSeqNo;
                txtCassetteID.Text = CurPort.CassetteID;
                txtCSTType.Text = CurPort.CassetteType.ToString();
                txtCassetteStatus.Text = CurPort.CassetteStatus.ToString();
                txtPortEnable.Text = CurPort.PortEnable.ToString();
                txtPortMode.Text = CurPort.PortMode.ToString();
                txtPortStatus.Text = CurPort.PortStatus.ToString();
                txtPortDown.Text = CurPort.PortDown.ToString();
                txtPortType.Text = CurPort.PortType.ToString();
                txtTransferMode.Text = CurPort.PortTransfer.ToString();
                txtPortCount.Text = CurPort.PortGlassCount.ToString();
                txtPortGrade.Text = CurPort.PortGrade;
                txtPartialFullMode.Text = CurPort.PartialFullMode.ToString();
                txtLoadCSTType.Text = CurPort.LoadingCassetteType.ToString();
                txtPortProductType.Text = CurPort.ProductType.ToString();
                txtPortProcessType.Text = CurPort.ProcessType_Array;
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
                bool _haveWip = false;
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
                        
                        if ((_slot.CassetteSeqNo == string.Empty && _slot.JobSeqNo == string.Empty) || 
                            (_slot.CassetteSeqNo == "0" && _slot.JobSeqNo == "0")) _haveWip = false;
                        else _haveWip = true;

                        _row.DefaultCellStyle.BackColor = AdjustGridViewRowColor(_slot.TrackingValue, _slot.SamplingSlotFlag, _slot.EQPRTCFlag, _haveWip);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private Color AdjustGridViewRowColor(string trackingValue, string samplingSlotFlag,string EQPRTCFlag, bool HaveWip)
        {
            try
            {
                //2015/7/31 下午 05:15 玉明mail
               //sampling flag false (0) : Gray
               //sampling flag true (1) :
               //       1. wait for main process : Yellow  (Tracking data = 0 )
               //       2. normal process : Green
               //       3. abnormal process : Orange
              
               // modify by yang 20161107  add EQPRTCFlag
                //    EQP RTC true(1):(先判断)
                //      1. current normal process : Blue
                //      2. current abnormal process : DarkBlue

                #region setting color

                if (HaveWip == false) return Color.White;

                if (samplingSlotFlag.Equals("0")) //sampling flag False
                {
                    return Color.Gray;
                }
                else  //sampling flag True
                {
                    //Tracking Data
                    //0：Not Processed (Not In EQ)
                    //1：Normal Processed (In EQ)
                    //2：Abnormal Processed (In EQ)
                    //3：Process Skip (In EQ)
                    if (trackingValue.Contains("2") && EQPRTCFlag=="True") //EQPRTC,含 Abnormal
                        return Color.DarkBlue;
                    if (trackingValue.Contains("2"))//含 Abnormal
                        return Color.Orange;
                    if (trackingValue.Contains("1") && EQPRTCFlag=="True") //EQPRTC,含 normal process
                        return Color.Blue;
                    if (EQPRTCFlag=="True")  //包含EQPRTC,follow Abnoraml case
                        return Color.DarkBlue;
                    if (trackingValue.Contains("1"))//含  normal process
                        return Color.Green;
                    else return Color.Yellow;  
                }

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return Color.White;
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

    }
}
