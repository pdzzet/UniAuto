using System;
using System.Xml;

namespace UniOPI
{
    public partial class FormTrxTest : FormBase
    {
        public string TrxID = string.Empty;
        public string MsgNmae = string.Empty;
        public string MsgData = string.Empty;
        public XmlDocument XmlDoc;

        public FormTrxTest()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            TrxID = DateTime.Now.ToString("yyyyMMddHHmmssff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            MsgNmae = txtTrxName.Text.ToString().Trim();
            MsgData = txtMessage.Text.ToString().Trim();

            XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(MsgData);

            if (MsgData == string.Empty || MsgNmae == string.Empty)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }
    }
}
