using System;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormQuectionMessage : FormBase
    {
        private string QuestionMsg = string.Empty;

        public FormQuectionMessage(string caption,string quectionMsg)
        {
            InitializeComponent();

            lblCaption.Text = caption;

            QuestionMsg = quectionMsg;            
        }

        private void FormQuectionMessage_Load(object sender, EventArgs e)
        {
            txtMsg.Text = QuestionMsg;

            lblICON.ImageIndex = 2;
        }

        private void btnQYes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
        }

        private void btnQNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
        }

    }
}
