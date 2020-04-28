using System;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormPalletControl_Local : FormBase
    {
        public Pallet CurPallet = new Pallet();

        public FormPalletControl_Local()
        {
            InitializeComponent();
        }

        private void FormPalletControl_Local_Load(object sender, EventArgs e)
        {
            try
            {
                txtPalletNo.Text = CurPallet.PalletNo;

                SendtoBC_PalletDataRequest();
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


                #region OfflinePalletDataSend

                LocalModePalletDataSend _trx = new LocalModePalletDataSend();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PALLETNO = CurPallet.PalletNo;
                _trx.BODY.PALLETID = CurPallet.PalletID;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                ShowMessage(this, lblCaption.Text , "", "Local Pallet Command Send to BC Success !", MessageBoxIcon.Information);

                btnMapDownload.Enabled = false;

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        #region Socket Process

        private void SendtoBC_PalletDataRequest()
        {
            try
            {
                LocalModePalletDataRequest _trx = new LocalModePalletDataRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PALLETNO = CurPallet.PalletNo;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region LocalModePalletDataReply

                string _respXml = _resp.Xml;

                #region Update Data
                RecvMessage_LocalPalletDataReply(_respXml);
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }     

        public void RecvMessage_LocalPalletDataReply(string xml)
        {
            try
            {
                LocalModePalletDataReply _trx = (LocalModePalletDataReply)Spec.CheckXMLFormat(xml);

                txtPalletID.Text = _trx.BODY.PALLETID;
                txtDenseBoxCount.Text = _trx.BODY.DENSEBOXCOUNT;

                dgvBox.Rows.Clear();

                foreach (LocalModePalletDataReply.DENSEBOXc _dense in _trx.BODY.DENSEBOXLIST)
                {
                    dgvBox.Rows.Add(_dense.DNESEBOXIDNO, _dense.DNESEBOXID);
                }
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
