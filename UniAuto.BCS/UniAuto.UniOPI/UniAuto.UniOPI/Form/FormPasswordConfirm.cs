using System;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormPasswordConfirm : FormBase
    {
        public FormPasswordConfirm()
        {
            InitializeComponent();
        }
        public FormPasswordConfirm(string ii)
        {
            InitializeComponent();
        }
        private void FormPasswordConfirm_Load(object sender, EventArgs e)
        {
            txtUserID.Text = FormMainMDI.G_OPIAp.LoginUserID;

            if (FormMainMDI.G_OPIAp.IsRunVshost)
            {
                txtPWD.Text = FormMainMDI.G_OPIAp.LoginPassword;
            }
        }

        private void CheckData()
        {
            string password = txtPWD.Text.ToString();
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowMessage(this, lblCaption.Text, "", "Please fill in password", MessageBoxIcon.Error);
                txtPWD.Focus();
                return;
            }
            if (!password.Equals(FormMainMDI.G_OPIAp.LoginPassword))
            {
                ShowMessage(this, lblCaption.Text, "", "The password is not correct!!", MessageBoxIcon.Error);
                txtPWD.Focus();
                return;
            }
            this.DialogResult = DialogResult.OK;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            CheckData();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void FormPasswordConfirm_Shown(object sender, EventArgs e)
        {
            txtPWD.Focus();
        }

        private void txtPWD_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                CheckData();
            }
        }


    }
}
