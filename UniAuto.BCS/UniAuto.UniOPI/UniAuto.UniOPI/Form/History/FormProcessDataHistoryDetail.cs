using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;


namespace UniOPI
{
    public partial class FormProcessDataHistoryDetail : FormBase
    {

        DataRow CurDataRow = null;

        public FormProcessDataHistoryDetail(DataRow _row)
        {
            InitializeComponent();
            lblCaption.Text = "ProcessDataHistory Detail";

            CurDataRow = _row;
        }

        #region Public Methods
        public void SetDetailData(ProcessDataHistoryReply reply)
        {
            try
            {
                #region Header Data
                txtJobSeqNo.Text = reply.BODY.JOBSEQNO;
                txtCstSeqNo.Text = reply.BODY.CSTSEQNO;
                txtJobID.Text = reply.BODY.JOBID;
                txtNodeNo.Text = reply.BODY.EQUIPMENTNO;
                #endregion

                #region Body Data
                DataTable dt = UniTools.InitDt(new string[] { "NAME", "VALUE" });
                foreach (ProcessDataHistoryReply.DATAc data in reply.BODY.DATALIST)
                {
                    DataRow drNew = dt.NewRow();
                    drNew["NAME"] = data.NAME;
                    drNew["VALUE"] = data.VALUE;
                    dt.Rows.Add(drNew);
                }

                dgvData.DataSource = dt;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
        #endregion

        private void FormProcessDataHistoryDetail_Load(object sender, EventArgs e)
        {
            try
            {
                string _err = string.Empty;


                #region Send ProcessDataHistoryRequest

                ProcessDataHistoryRequest trx = new ProcessDataHistoryRequest();
                trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                trx.BODY.CSTSEQNO = CurDataRow["CASSETTESEQNO"].ToString();
                trx.BODY.JOBSEQNO = CurDataRow["JOBSEQNO"].ToString();
                trx.BODY.EQUIPMENTNO = CurDataRow["NODEID"].ToString();
                trx.BODY.TRXID = CurDataRow["TRXID"].ToString();
                trx.BODY.FILENAME = CurDataRow["FILENAMA"].ToString();
                trx.BODY.JOBID = CurDataRow["JOBID"].ToString();

                string _xml = trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(trx.HEADER.TRANSACTIONID, trx.HEADER.MESSAGENAME, trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region JobDataReply

                string _respXml = _resp.Xml;

                ProcessDataHistoryReply _reply = (ProcessDataHistoryReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data
                SetDetailData(_reply);
                #endregion

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
