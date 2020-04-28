using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormDefectCodeReport : FormBase
    {
        string CurNodeNo = string.Empty;
        string CurNodeID = string.Empty;

        public FormDefectCodeReport()
        {
            InitializeComponent();
        }

        private void FormDefectCodeReport_Load(object sender, EventArgs e)
        {
            try
            {
                tmrBaseRefresh.Enabled = false;

                //取得Node Attribute為IN的資料做為ComboBox選項
                var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values  where node.NodeAttribute == "IN"select new {
                IDNAME = string.Format("{0}-{1}-{2}",node.NodeNo,node.NodeID,node.NodeName),node.NodeNo,node.NodeID
                }).ToList();

                if(q==null|| q.Count==0)
                    return;


                cboNode.DataSource=q;
                cboNode.ValueMember="IDNAME";
                cboNode.SelectedIndex=-1;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            //必須要有選擇才能Query和啟動Timer
            if (cboNode.SelectedIndex > -1)
            {
                CurNodeID = ((dynamic)cboNode.SelectedItem).NodeID;
                CurNodeNo = ((dynamic)cboNode.SelectedItem).NodeNo;
                Send_DefectCodeReport();
                tmrBaseRefresh.Enabled = true;
            }
        }

        private void tmrBaseRefresh_Tick(object sender, EventArgs e)
        {
            //Tag無資訊時停止Timer
            if (FormMainMDI.CurForm.Tag ==null || FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString())
            {
                tmrBaseRefresh.Enabled = false;
                return;
            }
            //NodeNo無值則停止Timer
            if (CurNodeNo == string.Empty)
            {
                tmrBaseRefresh.Enabled = false;
                return;
            }

            try
            {
                //從非同步的defectCodeReply取得回應
                BCS_DefectCodeReply _reply = FormMainMDI.G_OPIAp.Dic_Node[CurNodeNo].BC_DefectCodeReportReply;

                if (_reply.IsReply)
                {
                    DateTime _now = DateTime.Now;
                    TimeSpan _ts = _now.Subtract(_reply.LastRequestDate).Duration();

                    if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                    {
                        //發送請求給BC
                        Send_DefectCodeReport();
                    }
                }

                //從非同步defectCodeReply取得資料
                GetDataGridView();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            
        }

        private void Send_DefectCodeReport()
        {
            try
            {
                string _err=string.Empty;
                string _xml= string.Empty;

                DefectCodeReportRequest _trx = new DefectCodeReportRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurNodeNo;

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), out _err, FormMainMDI.G_OPIAp.SessionID);

                FormMainMDI.G_OPIAp.Dic_Node[CurNodeNo].BC_DefectCodeReportReply.IsReply = false;
                FormMainMDI.G_OPIAp.Dic_Node[CurNodeNo].BC_DefectCodeReportReply.LastRequestDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void GetDataGridView()
        {
            try
            {
                BCS_DefectCodeReply _reply = FormMainMDI.G_OPIAp.Dic_Node[CurNodeNo].BC_DefectCodeReportReply;

                while (dgvData.Rows.Count > _reply.LstDefect.Count)
                {
                    dgvData.Rows.RemoveAt(dgvData.Rows.Count - 1);
                }

                for (int i = 0; i < _reply.LstDefect.Count; i++)
                {
                    if (dgvData.RowCount < i + 1)
                        dgvData.Rows.Add(CurNodeNo + "-" + CurNodeID, _reply.LstDefect[i].CassetteSeqNo, _reply.LstDefect[i].JobSeqNo, _reply.LstDefect[i].ChipPosition, _reply.LstDefect[i].UnitNo, _reply.LstDefect[i].UnitID, _reply.LstDefect[i].DefectCode);
                    else
                    {
                        dgvData.Rows[i].Cells[colLocalNoID.Name].Value = CurNodeNo + "-" + CurNodeID;
                        dgvData.Rows[i].Cells[colCSTSeqNo.Name].Value = _reply.LstDefect[i].CassetteSeqNo;
                        dgvData.Rows[i].Cells[colJobSeqNo.Name].Value = _reply.LstDefect[i].JobSeqNo;
                        dgvData.Rows[i].Cells[colChipPosition.Name].Value = _reply.LstDefect[i].ChipPosition;
                        dgvData.Rows[i].Cells[colUnitNo.Name].Value = _reply.LstDefect[i].UnitNo;
                        dgvData.Rows[i].Cells[colUnitID.Name].Value = _reply.LstDefect[i].UnitID;
                        dgvData.Rows[i].Cells[colDefectCode.Name].Value = _reply.LstDefect[i].DefectCode;
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
