using System;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormPalletControl_Offline : FormBase
    {

        public Pallet CurPallet = new Pallet();

        public FormPalletControl_Offline()
        {
            InitializeComponent();
        }

        private void FormPalletControl_Offline_Load(object sender, EventArgs e)
        {
            try
            {
                txtPalletNo.Text = CurPallet.PalletNo.ToString().PadLeft(2, '0');
                txtPalletID.Text = CurPallet.PalletID.ToString();
                txtDenseBoxCount.Focus();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnSet_Click(object sender, System.EventArgs e)
        {
            try
            {
                int _intCnto = 0;

                #region Check Box ID Count
                if (string.IsNullOrWhiteSpace(txtDenseBoxCount.Text.ToString()))
                {
                    ShowMessage(this, lblCaption.Text, "", "Please key in Dense Box ID Count", MessageBoxIcon.Error);
                    txtDenseBoxCount.Focus();
                    return;
                }

                int.TryParse(txtDenseBoxCount.Text.ToString(), out _intCnto);

                if (_intCnto > 56 || _intCnto < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", " 0 <= Dense Box ID Count <= 56", MessageBoxIcon.Error);

                    txtDenseBoxCount.Text = "0";

                    txtDenseBoxCount.Focus();

                    return;
                }
                #endregion

                #region Create Dense Box ID
                dgvBox.Rows.Clear();
                for (int i = 1; i <= _intCnto; i++)                
                {
                    dgvBox.Rows.Add(i.ToString().PadLeft(2, '0'), "DENSE" + i.ToString().PadLeft(2, '0'));
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }


        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                asciiCode == 8)   // Backspace
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void btnMapDownload_Click(object sender, EventArgs e)
        {
            try
            {
                string _err = string.Empty;

                #region Check Data

                #region Dense Box id
                //if (dgvBox.Rows.Count == 0)
                //{
                //    ShowMessage(this, lblCaption.Text, "", "Please Create Dense Box ID", MessageBoxIcon.Error);

                //    return;
                //}
                #endregion

                #endregion

                #region OfflinePalletDataSend

                OfflinePalletDataSend _trx = new OfflinePalletDataSend();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PALLETNO = CurPallet.PalletNo;
                _trx.BODY.PALLETID = CurPallet.PalletID;

                OfflinePalletDataSend.DENSEBOXc _dense;

                foreach (DataGridViewRow _row in dgvBox.Rows)
                {
                    _dense = new OfflinePalletDataSend.DENSEBOXc();

                    _dense.DNESEBOXIDNO = _row.Cells[colSlotNo.Name].Value.ToString();
                    _dense.DNESEBOXID = _row.Cells[colDenseID.Name].Value.ToString();

                    _trx.BODY.DENSEBOXLIST.Add(_dense);
                }
                    
                string _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                ShowMessage(this, lblCaption.Text , "", "Offline Pallet Command Send to BC Success !", MessageBoxIcon.Information);

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
