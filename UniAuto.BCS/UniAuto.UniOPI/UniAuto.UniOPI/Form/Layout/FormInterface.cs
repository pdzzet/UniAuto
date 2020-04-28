using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormInterface : FormBase
    {
        Dictionary<int, string> DicUp_Desc;
        Dictionary<int, string> DicDown_Desc;

        string UpNode = "";
        string UpUnit = "";
        string DownNode = "";
        string DownUnit = "";
        string UpSeqNo = "";
        string DnSeqNo = "";
        string PipeName = string.Empty;

        string LinkType = string.Empty;
        string TimerCharType = string.Empty;

        public FormInterface()
        {
            InitializeComponent();
        }

        public FormInterface(string strPipeName)
        {
            InitializeComponent();

            try
            {

                PipeName = strPipeName;

                //I0200030001
                UpNode = "L" + int.Parse(strPipeName.Substring(1, 2));
                UpUnit = strPipeName.Substring(3, 2);

                DownNode = "L" + int.Parse(strPipeName.Substring(5, 2));
                DownUnit = strPipeName.Substring(7, 2);

                UpSeqNo = strPipeName.Substring(9, 2);
                DnSeqNo = strPipeName.Substring(11, 2);

                if (!FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(UpNode) || !FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(DownNode)) return;
         
                Node ndUp = FormMainMDI.G_OPIAp.Dic_Node[UpNode];

                Node ndDown = FormMainMDI.G_OPIAp.Dic_Node[DownNode];

                lblCaption.Text = ndUp.NodeName + "->" + ndDown.NodeName;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void FormInterface_Load(object sender, EventArgs e)
        {
            try
            {
                tmrRefreshPLC.Interval = 2000;
                tmrRefreshPLC.Enabled = true;


                #region Load link signal setting
                if (FormMainMDI.G_OPIAp.Dic_Pipe.ContainsKey(PipeName))
                {
                    Interface _if = FormMainMDI.G_OPIAp.Dic_Pipe[PipeName];

                    var _var = FormMainMDI.G_OPIAp.Lst_LinkSignal_Type.Find(r => r.UpStreamLocalNo.Equals(_if.UpstreamNodeNo) && r.DownStreamLocalNo.Equals(_if.DownstreamNodeNo) &&
                        r.SeqNo.Equals(_if.UpstreamSeqNo+_if.DownstreamSeqNo));

                    if (_var != null)
                    {
                        LinkType = _var.LinkType;
                        TimerCharType = _var.TimingChart;
                    }
                }

                if (LinkType !=string.Empty )
                {
                    if (FormMainMDI.G_OPIAp.Dic_LinkSignal_Desc.ContainsKey(LinkType))
                    {
                        DicUp_Desc = FormMainMDI.G_OPIAp.Dic_LinkSignal_Desc[LinkType].UpStreamBit;
                        DicDown_Desc = FormMainMDI.G_OPIAp.Dic_LinkSignal_Desc[LinkType].DownStreamBit;
                    }
                    else
                    {
                        LinkType = string.Empty;
                    }
                }

                if (LinkType == string.Empty)
                {
                    DicUp_Desc = addUp();
                    DicDown_Desc = addDown();
                }
                #endregion

                foreach (int _seq in DicUp_Desc.Keys)
                {
                    Label lblitem = new Label();
                    lblitem.Name = "lblUpBit" + _seq.ToString("00");
                    lblitem.Tag = _seq.ToString() ;
                    lblitem.Image = Properties.Resources.Bit_Sliver;
                    lblitem.ImageAlign = ContentAlignment.MiddleLeft;
                    lblitem.Text = "            " + "B000  " + DicUp_Desc[_seq];
                    lblitem.Width = 230;
                    //lblitem.AutoSize = true;//家成偷改
                    lblitem.TextAlign = ContentAlignment.MiddleLeft;
                    lblitem.Font = new Font("Cambria", 10);
                    flpUp.Controls.Add(lblitem);
                }

                foreach (int _seq in DicDown_Desc.Keys)
                {
                    Label lblitem = new Label();
                    lblitem.Name = "lblDownBit" + _seq.ToString("00");
                    lblitem.Tag = _seq.ToString();
                    lblitem.Image = Properties.Resources.Bit_Sliver;
                    lblitem.ImageAlign = ContentAlignment.MiddleLeft;
                    lblitem.Text = "            " + "B000  " + DicDown_Desc[_seq];
                    lblitem.Width = 230;
                    //lblitem.AutoSize = true;//家成偷改
                    lblitem.TextAlign = ContentAlignment.MiddleLeft;
                    lblitem.Font = new Font("Cambria", 10);
                    flpDown.Controls.Add(lblitem);
                }

                dgvJob.Rows.Clear();

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        string CountAddress(string Address, string BitOffset)
        {
            try
            {
                if (Address == string.Empty) return string.Empty;

                string strA = Convert.ToInt32(Address, 16).ToString();
                string strB = BitOffset.ToString();
                string sum = (int.Parse(strA) + int.Parse(strB)).ToString();
                string strAddr = Convert.ToString(int.Parse(sum), 16);
                return strAddr.PadLeft(4, '0').ToUpper();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                
                return "";
            }
        }

        //讓WinForm Pop時不閃爍。 
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED (等同 cp.ExStyle = cp.ExStyle | 0x02000000)
                return cp;
            }
        } 

        private void btnTimingChart_Click(object sender, EventArgs e)
        {
            FormTimingChart _frm = new FormTimingChart(TimerCharType);
            _frm.TopMost = true;

            _frm.ShowDialog();

            _frm.Dispose();
        }
   
        private void tmrRefreshPLC_Tick(object sender, EventArgs e)
        {
            try
            {
                if (FormMainMDI.G_OPIAp.Dic_Pipe.ContainsKey(PipeName) == false) return;

                Interface _if = FormMainMDI.G_OPIAp.Dic_Pipe[PipeName];

                #region Bit
                int _seq = 0;
                foreach (Label _lbl in flpUp.Controls.OfType<Label>())
                {
                    if (_lbl.Tag == null) continue;

                    int.TryParse(_lbl.Tag.ToString(), out _seq);

                    if (_if.UpstreamSignal.Substring(_seq - 1, 1) == "0")  _lbl.Image = Properties.Resources.Bit_Sliver;
                    else _lbl.Image = Properties.Resources.Bit_Green;

                    _lbl.Text = "          " + CountAddress(_if.UpstreamBitAddress, (_seq-1).ToString()) + "  " + DicUp_Desc[_seq];
                }

                foreach (Label _lbl in flpDown.Controls.OfType<Label>())
                {
                    if (_lbl.Tag == null) continue;

                    int.TryParse(_lbl.Tag.ToString(), out _seq);

                    if (_if.DownstreamSignal.Substring(_seq - 1, 1) == "0") _lbl.Image = Properties.Resources.Bit_Sliver;
                    else _lbl.Image = Properties.Resources.Bit_Green;

                    _lbl.Text = "          " + CountAddress(_if.DownstreamBitAddress, (_seq-1).ToString()) + "  " + DicDown_Desc[_seq];
                }
                #endregion

                #region JobData

                if (dgvJob.Rows.Count == 0)
                {
                    #region Add Job Data
                    foreach (JobData _job in _if.UpstreamJobData )
                    {
                        dgvJob.Rows.Add("Detail", "UP",
                            _job.JobAddress,
                            _job.CassetteSeqNo,
                            _job.JobSeqNo,
                            _job.GlassID,
                            _job.ProductType,
                            _job.SubStrateType,
                            _job.JobType,
                            _job.JobJudge,
                            _job.JobGrade,                            
                            _job.PPID,
                            _job.TrackingData,
                            _job.EQPFlag);
                    }

                    foreach (JobData _job in _if.DownstreamJobData)
                    {
                        dgvJob.Rows.Add("Detail", "DOWN",
                               _job.JobAddress,
                               _job.CassetteSeqNo,
                               _job.JobSeqNo,
                               _job.GlassID,
                               _job.ProductType,
                               _job.SubStrateType,
                               _job.JobType,
                               _job.JobJudge,
                               _job.JobGrade,                               
                               _job.PPID,
                               _job.TrackingData,
                               _job.EQPFlag);
                    }
                    #endregion
                }
                else
                { 
                    bool _find = false;

                    #region find UpstreamJobData
                    foreach (JobData _job in _if.UpstreamJobData)
                    {
                        _find = false;
                        foreach (DataGridViewRow _row in dgvJob.Rows)
                        {
                            if (_row.Cells[colAddress.Name].Value.ToString() == _job.JobAddress)
                            {
                                _row.Cells[colCassetteSeqNo.Name].Value =  _job.CassetteSeqNo;
                                _row.Cells[colJobSeqNo.Name].Value =   _job.JobSeqNo;
                                _row.Cells[colProductType.Name].Value =  _job.ProductType;
                                _row.Cells[colSubStrateType.Name].Value =  _job.SubStrateType;
                                _row.Cells[colJobType.Name].Value =  _job.JobType;
                                _row.Cells[colJobJudge.Name].Value =  _job.JobJudge;
                                _row.Cells[colJobGrade.Name].Value =  _job.JobGrade;
                                _row.Cells[colGlassID.Name].Value =  _job.GlassID;
                                _row.Cells[colPPID.Name].Value =  _job.PPID;
                                _row.Cells[colTrackingData.Name].Value =  _job.TrackingData;
                                _row.Cells[colEQPFlag.Name].Value =  _job.EQPFlag;
                                _find = true;
                            }
                        }

                        if (_find == false)
                        {
                            dgvJob.Rows.Add("Detail", "UP",
                               _job.JobAddress,
                               _job.CassetteSeqNo,
                               _job.JobSeqNo,
                               _job.GlassID,
                               _job.ProductType,
                               _job.SubStrateType,
                               _job.JobType,
                               _job.JobJudge,
                               _job.JobGrade,
                               _job.PPID,
                               _job.TrackingData,
                               _job.EQPFlag);
                        }
                    }
                    #endregion

                    #region find DownstreamJobData
                    foreach (JobData _job in _if.DownstreamJobData)
                    {
                        _find = false;
                        foreach (DataGridViewRow _row in dgvJob.Rows)
                        {
                            if (_row.Cells[colAddress.Name].Value.ToString() == _job.JobAddress)
                            {
                                _row.Cells[colCassetteSeqNo.Name].Value = _job.CassetteSeqNo;
                                _row.Cells[colJobSeqNo.Name].Value = _job.JobSeqNo;
                                _row.Cells[colProductType.Name].Value = _job.ProductType;
                                _row.Cells[colSubStrateType.Name].Value = _job.SubStrateType;
                                _row.Cells[colJobType.Name].Value = _job.JobType;
                                _row.Cells[colJobJudge.Name].Value = _job.JobJudge;
                                _row.Cells[colJobGrade.Name].Value = _job.JobGrade;
                                _row.Cells[colGlassID.Name].Value = _job.GlassID;
                                _row.Cells[colPPID.Name].Value = _job.PPID;
                                _row.Cells[colTrackingData.Name].Value = _job.TrackingData;
                                _row.Cells[colEQPFlag.Name].Value = _job.EQPFlag;
                                _find = true;
                            }
                        }

                        if (_find == false)
                        {
                            dgvJob.Rows.Add("Detail", "DOWN",
                                   _job.JobAddress,
                                   _job.CassetteSeqNo,
                                   _job.JobSeqNo,
                                   _job.GlassID,
                                   _job.ProductType,
                                   _job.SubStrateType,
                                   _job.JobType,
                                   _job.JobJudge,
                                   _job.JobGrade,
                                   _job.PPID,
                                   _job.TrackingData,
                                   _job.EQPFlag);
                        }
                    }
                    #endregion
                }
                #endregion

            }
            catch (Exception ex)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void dgvJob_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (this.dgvJob.CurrentRow != null)
                {
                    if (this.dgvJob.CurrentCell != null && dgvJob.CurrentCell.Value.ToString() == "Detail")
                    {
                        string _cstSeqNo = dgvJob.CurrentRow.Cells[colCassetteSeqNo.Name].Value.ToString();
                        string _jobSeqNo =  dgvJob.CurrentRow.Cells[colJobSeqNo.Name].Value.ToString();
                        string _glassID = dgvJob.CurrentRow.Cells[colGlassID.Name].Value.ToString();

                        if (_glassID==string.Empty  || _cstSeqNo==string.Empty || _jobSeqNo==string.Empty )
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Cassette Seq No、Job Seq No、Job ID must be Required！", MessageBoxIcon.Warning);
                            return;
                        }

                        FormJobDataDetail _frm = new FormJobDataDetail(_glassID, _cstSeqNo, _jobSeqNo);
                        _frm.TopMost = true;

                        _frm.ShowDialog();

                        _frm.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void FormInterface_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                tmrRefreshPLC.Enabled = false;

                FormMainMDI.G_OPIAp.Dic_Pipe[PipeName].IsDisplay = false;     
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private Dictionary<int, string> addUp()
        {
            Dictionary<int, string> _upBit = new Dictionary<int, string>();

            _upBit.Add(1,"Upstream Inline");
            _upBit.Add(2,"Upstream Trouble");
            _upBit.Add(3,"Send Ready");
            _upBit.Add(4,"Send");
            _upBit.Add(5,"Job Transfer");
            _upBit.Add(6,"Send Cancel");
            _upBit.Add(7,"Exchange Execute");
            _upBit.Add(8,"Double Glass");
            _upBit.Add(9,"Send Job Reserve");
            _upBit.Add(10,"Receive OK");
            _upBit.Add(11,"Spare");
            _upBit.Add(12,"Spare");
            _upBit.Add(13,"Spare");
            _upBit.Add(14,"Spare");
            _upBit.Add(15,"Spare");
            _upBit.Add(16,"Spare");
            _upBit.Add(17,"Slot Number#01");
            _upBit.Add(18,"Slot Number#02");
            _upBit.Add(19,"Slot Number#03");
            _upBit.Add(20,"Slot Number#04");
            _upBit.Add(21,"Slot Number#05");
            _upBit.Add(22,"Slot Number#06");
            _upBit.Add(23,"Spare");
            _upBit.Add(24,"Spare");
            _upBit.Add(25,"Spare");
            _upBit.Add(26,"Short Vicinity");
            _upBit.Add(27,"Long Vicinity");
            _upBit.Add(28,"Preparation Completion");
            _upBit.Add(29,"In Inspecting");
            _upBit.Add(30,"In Inspecting");
            _upBit.Add(31,"In Inspecting");
            _upBit.Add(32,"Return Mode");
            return _upBit;
        }

        private Dictionary<int, string> addDown()
        {
            Dictionary<int, string> _downBit = new Dictionary<int, string>();
            _downBit.Add(1,"Downstream Inline");
            _downBit.Add(2,"Downstream Trouble");
            _downBit.Add(3,"Receive Able");
            _downBit.Add(4,"Receive");
            _downBit.Add(5,"Job Transfer");
            _downBit.Add(6,"Receive Cancel");
            _downBit.Add(7,"Exchange Possible");
            _downBit.Add(8,"Double Glass");
            _downBit.Add(9,"Receive Job Reserve");
            _downBit.Add(10,"Spare");
            _downBit.Add(11,"Transfer Stop Request");
            _downBit.Add(12,"Dummy Glass Request");
            _downBit.Add(13,"Glass Exist");
            _downBit.Add(14,"Spare");
            _downBit.Add(15,"Spare");
            _downBit.Add(16,"Spare");
            _downBit.Add(17,"Slot Number#01");
            _downBit.Add(18,"Slot Number#02");
            _downBit.Add(19,"Slot Number#03");
            _downBit.Add(20,"Slot Number#04");
            _downBit.Add(21,"Slot Number#05");
            _downBit.Add(22,"Slot Number#06");
            _downBit.Add(23,"Glass Count#01");
            _downBit.Add(24,"Glass Count#02");
            _downBit.Add(25,"Glass Count#03");
            _downBit.Add(26,"Glass Count#04");
            _downBit.Add(27,"Glass Count#05");
            _downBit.Add(28,"Preparation Permission");
            _downBit.Add(29,"Inspection Result Update");
            _downBit.Add(30,"Spare");
            _downBit.Add(31,"Spare");
            _downBit.Add(32,"Spare");
            return _downBit;
        }

    }
}

