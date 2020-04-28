using System;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormShowMessage : FormBase
    {
        public FormShowMessage(string Caption, string Msg, MessageBoxIcon MsgIcon)
        {
            InitializeComponent();

            lblCaption.Text = Caption;
            txtMsg.Text = Msg;

            SetIcon(MsgIcon);
        }

        private void SetIcon(MessageBoxIcon msgType)
        {
            switch (msgType)
            {
                case MessageBoxIcon.Warning: // "Warning":
                    lblICON.ImageIndex = 0;
                    break;
                case MessageBoxIcon.Error: // "Error":
                    lblICON.ImageIndex = 1;
                    break;
                case MessageBoxIcon.Question: // "Question":
                    lblICON.ImageIndex = 2;
                    break;
                case MessageBoxIcon.Asterisk: // "Asterisk":
                    lblICON.ImageIndex = 3;
                    break;
                default:
                    lblICON.ImageIndex = 3;
                    break;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
