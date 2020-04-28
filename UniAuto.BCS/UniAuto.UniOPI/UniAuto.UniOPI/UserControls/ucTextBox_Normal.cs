using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class ucTextBox_Normal : UserControl
    {
        #region Fields
        #endregion

        #region Property
        public bool Checked
        {
            get { return this.chkUse.Checked; }
            set { this.chkUse.Checked = value; }
        }

        public string Caption
        {
            get { return this.gbxTextBox.Text; }
            set { this.gbxTextBox.Text = value; }
        }

        public string InputText
        {
            get { return this.txtInput.Text.Trim(); }
            set { this.txtInput.Text = value.Trim(); }
        }
        #endregion

        public ucTextBox_Normal(bool chkUse)
        {
            InitializeComponent();
            this.chkUse.Click += chkUse_Click;
            if (!chkUse)
            {
                this.chkUse.Visible = false;
                //this.txtInput.Dock = DockStyle.Fill;
            }
        }

        #region Events
        private void chkUse_Click(object sender, EventArgs e)
        {
            CheckBox objChk = (CheckBox)sender;
            if (objChk.Checked)
            {
                txtInput.Focus();
                txtInput.SelectAll();
            }
        }
        #endregion
    }
}
