using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Forms = System.Windows.Forms;

namespace UniOPI
{
    public partial class FormSelectItem : FormBase
    {
        private List<string> _Items = new List<string>();
        public string ItemValue = string.Empty;
        private int BtnWidth = 80;
        private int BtnHeight = 40;

        public FormSelectItem(string Title, List<string> Items, int ButtonWidth, int ButtonHeight)
        {
            InitializeComponent();
            this.Text = Title;
            lblCaption.Text = Title;
            _Items = Items;
            BtnWidth = ButtonWidth;
            BtnHeight = ButtonHeight;
        }
        public FormSelectItem(string Title, List<string> Items)
        {
            InitializeComponent();
            this.Text = Title;
            lblCaption.Text = Title;
            _Items = Items;
        }

        private void FormSelectItem_Load(object sender, EventArgs e)
        {
            InitialButton();
        }

        #region Method

        private void InitialButton()
        {
            List<UcAutoButtons.ButtonInfo> lstButtonInfo = new List<UcAutoButtons.ButtonInfo>();

            foreach (string strItem in _Items)
            {
                lstButtonInfo.Add(new UcAutoButtons.ButtonInfo(strItem, strItem, btn_Click));
            }

            this.ucABtnItem.CreateButton(lstButtonInfo, BtnWidth, BtnHeight);
        }
        void btn_Click(object sender, EventArgs e)
        {
            Forms.Button btnThis = sender as Forms.Button;
            btnThis.Enabled = false;
            ItemValue = btnThis.Text;

            this.DialogResult = Forms.DialogResult.OK;
        }

        #endregion

        private void FormSelectItem_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void FormSelectItem_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
