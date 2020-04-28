using System;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormDenseControl_Local : FormBase
    {
        public Dense CurDense = new Dense();

        public FormDenseControl_Local()
        {
            InitializeComponent();
        }

        private void FormDenseControl_Local_Load(object sender, EventArgs e)
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
                LocalModeDenseDataRequest _trx = new LocalModeDenseDataRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PORTNO = CurDense.PortNo;
                _trx.BODY.EQUIPMENTNO = CurDense.NodeNo;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region LocalModeDenseDataReply

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
                LocalModeDenseDataReply _trx = (LocalModeDenseDataReply)Spec.CheckXMLFormat(xml);

                txtProductType.Text = _trx.BODY.PRODUCTTYPE.ToString();
                txtGlassCount01.Text = _trx.BODY.BOXGLASSCOUNT01.ToString();
                txtGlassCount02.Text = _trx.BODY.BOXGLASSCOUNT02.ToString();

                txtBoxID01.Text = _trx.BODY.BOXID01.ToString();
                txtBoxID02.Text = _trx.BODY.BOXID02.ToString();

                txtCSTSettingCode01.Text = _trx.BODY.CASSETTESETTINGCODE01.ToString();
                txtCSTSettingCode02.Text = _trx.BODY.CASSETTESETTINGCODE02.ToString();

                cboJobGrade01.Text = _trx.BODY.JOBGRADE01.ToString();
                cboJobGrade02.Text = _trx.BODY.JOBGRADE02.ToString();
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

                LocalModeDenseDataSend _trx = new LocalModeDenseDataSend();

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
