using System;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormCassetteCommand_PortCount : FormBase
    {
        Port CurPort;
        public int StartByCount = 0 ;

        public FormCassetteCommand_PortCount(Port port)
        {
            InitializeComponent();

            CurPort = port;
        }

        private void txtCount_KeyPress(object sender, KeyPressEventArgs e)
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

        private void txtCount_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                int _cnt = 0;

                int.TryParse(_txt.Text.ToString(), out _cnt);


                if (CurPort.PortType == ePortType.LoadingPort || CurPort.PortType == ePortType.BothPort)
                {
                    //UPK Loader不會上報glass count，只需卡不超過max count
                    if (FormMainMDI.G_OPIAp.CurLine.LineType == "FCUPK_TYPE1" || FormMainMDI.G_OPIAp.CurLine.LineType == "FCUPK_TYPE2")
                    {
                        if (_cnt > CurPort.MaxCount || _cnt < 0)
                        {
                            ShowMessage(this, lblCaption.Text , "", string.Format(" 0 < Start by Count < {0}", CurPort.MaxCount), MessageBoxIcon.Error);

                            txtCount.Text = string.Empty;

                            txtCount.Focus();

                            return;
                        }
                    }
                    else
                    {
                        if (_cnt > CurPort.PortGlassCount || _cnt < 0)
                        {
                            ShowMessage(this,lblCaption.Text , "", string.Format(" 0 < Start by Count < {0}", CurPort.PortGlassCount), MessageBoxIcon.Error);

                            txtCount.Text = string.Empty;

                            txtCount.Focus();

                            return;
                        }
                    }
                }

                //Loading Port是出幾片、Unloading Port是收幾片, Partial Full下是整個CST COUNT-PARTIAL COUNT=可下最大COUNT，而不是拿機台上報的Count當做最大片數
                if (CurPort.PortType == ePortType.UnloadingPort)
                {
                    if (_cnt > (CurPort.MaxCount-CurPort.PortGlassCount) || _cnt < 0)
                    {
                        ShowMessage(this,lblCaption.Text , "", string.Format(" 0 < Start by Count < {0}", (CurPort.MaxCount-CurPort.PortGlassCount)), MessageBoxIcon.Error);

                        txtCount.Text = string.Empty;

                        txtCount.Focus();

                        return;
                    }
                }                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                int _cnt = 0;

                int.TryParse(txtCount.Text.ToString(), out _cnt);

                if (_cnt == 0)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format(" 0 < Start by Count < {0}", CurPort.MaxCount), MessageBoxIcon.Error);

                    txtCount.Focus();

                    return;
                }

                StartByCount = _cnt;

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
