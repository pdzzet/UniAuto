using System;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormCassetteControl_ProductType_Array : FormBase
    {
        public string PorductType = string.Empty;

        public FormCassetteControl_ProductType_Array()
        {
            InitializeComponent();
            
            PorductType = "01";
            txtProductType.Text = PorductType;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void rdoNum_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                if (_rdo.Checked)
                {
                    PorductType = _rdo.Text.ToString() + PorductType.Substring(1,1);
                    txtProductType.Text = PorductType;
                }
                               
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void rdoType_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                if (_rdo.Checked)
                {
                    PorductType = PorductType.Substring(0, 1) + _rdo.Tag.ToString() ;
                    txtProductType.Text = PorductType;
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
