using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormRobotStopReason : FormBase
    {
        string CSTSeqNo = string.Empty;
        string JobSeqNo = string.Empty;

        public FormRobotStopReason(string cstSeqNo, string jobSeqNo)
        {
            InitializeComponent();

            CSTSeqNo = cstSeqNo;
            JobSeqNo = jobSeqNo;
        }

        private void FormRobotStopReason_Load(object sender, System.EventArgs e)
        {
            try
            {
                #region Send RobotStopRunReasonRequest

                RobotStopRunReasonRequest _trx = new RobotStopRunReasonRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineName;
                _trx.BODY.CASSETTESEQNO = CSTSeqNo;
                _trx.BODY.JOBSEQNO = JobSeqNo;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region RobotStopRunReasonReply

                string _respXml = _resp.Xml;

                RobotStopRunReasonReply _reply = (RobotStopRunReasonReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data
                FillContorls(_reply);
                #endregion

                #endregion

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
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

                DataGridViewRow _row = dgvJobMessage.Rows[e.RowIndex];

                this.txtJobMsg.Text = _row.Cells[colJobMessage.Name].Value.ToString();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvRobotMessage_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

                DataGridViewRow _row = dgvRobotMessage.Rows[e.RowIndex];

                this.txtRobotMsg.Text = _row.Cells[colRobotMessage.Name].Value.ToString();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void FillContorls(RobotStopRunReasonReply DataReply)
        {
            try
            {
                dgvJobMessage.Rows.Clear();
                dgvRobotMessage.Rows.Clear();

                txtRobotName.Text = DataReply.BODY.ROBOTNAME.ToString();
                txtGlassID.Text = DataReply.BODY.GLASSID.ToString();
                txtJobSeqNo.Text = DataReply.BODY.JOBSEQNO.ToString();
                txtCstSeqNo.Text = DataReply.BODY.CASSETTESEQNO.ToString();
                txtRealStepID.Text = DataReply.BODY.REAL_STEPID.ToString();
                txtRealNextStepID.Text = DataReply.BODY.REAL_NEXT_STEPID.ToString();

                foreach (RobotStopRunReasonReply.REASONc _reason in DataReply.BODY.JOBREASONLIST)
                {
                    dgvJobMessage.Rows.Add(_reason.STOP_REASON);
                }

                foreach (RobotStopRunReasonReply.REASONc _reason in DataReply.BODY.ROBOTREASONLIST)
                {
                    dgvRobotMessage.Rows.Add(_reason.STOP_REASON);
                }

                if (dgvJobMessage.Rows.Count > 0) dgvMessage_CellClick(dgvJobMessage.Rows[0].Cells[0], new DataGridViewCellEventArgs(0, 0));
                if (dgvRobotMessage.Rows.Count > 0) dgvRobotMessage_CellClick(dgvRobotMessage.Rows[0].Cells[0], new DataGridViewCellEventArgs(0, 0));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}
