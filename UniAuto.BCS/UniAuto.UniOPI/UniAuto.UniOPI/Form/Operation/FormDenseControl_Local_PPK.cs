using System;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormDenseControl_Local_PPK : FormBase
    {
        public Dense CurDense = new Dense();

        public FormDenseControl_Local_PPK()
        {
            InitializeComponent();
        }

        private void FormDenseControl_Local_PPK_Load(object sender, EventArgs e)
        {
            try
            {
                lblPortName.Text = CurDense.PortNo.ToString().PadLeft(2, '0');

                SendtoBC_DenseDataRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SendtoBC_DenseDataRequest()
        {
            try
            {
                PPKLocalModeDenseDataRequest _trx = new PPKLocalModeDenseDataRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PORTNO = CurDense.PortNo;
                _trx.BODY.EQUIPMENTNO = CurDense.NodeNo;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region PPKLocalModeDenseDataReply

                string _respXml = _resp.Xml;

                #region Update Data
                RecvMessage_LocalDenseDataReply(_respXml);
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
      

        private void RecvMessage_LocalDenseDataReply(string xml)
        {
            try
            {
                PPKLocalModeDenseDataReply _trx = (PPKLocalModeDenseDataReply)Spec.CheckXMLFormat(xml);

                txtProductType.Text  = _trx.BODY.PRODUCTTYPE.ToString();
                txtBoxID.Text = _trx.BODY.BOXID.ToString();
                txtPaperBoxID.Text = _trx.BODY.PAPER_BOXID.ToString();
                txtBoxType.Text = (_trx.BODY.BOXTYPE == string.Empty ? eBoxType.Unknown : (eBoxType)int.Parse(_trx.BODY.BOXTYPE)).ToString();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnMapDownload_Click(object sender, EventArgs e)
        {
            try
            {
                string _err = string.Empty;

                #region LocalModeDenseDataSend

                PPKLocalModeDenseDataSend _trx = new PPKLocalModeDenseDataSend();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PORTNO = CurDense.PortNo;
                _trx.BODY.EQUIPMENTNO = CurDense.NodeNo;

                string _xml = _trx.WriteToXml();


                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                ShowMessage(this, lblCaption.Text , "", "Local Dense Command Send to BC Success !", MessageBoxIcon.Information);

                btnMapDownload.Enabled = false;
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
