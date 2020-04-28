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
    public partial class ucTextBox_His : UserControl, iHistBase
    {
        private ParamInfo _paramInfo;

        #region Property
        public ParamInfo Param
        {
            get { return _paramInfo; }
        }

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

        public ucTextBox_His(ParamInfo pi)
        {
            InitializeComponent();

            this._paramInfo = pi;

            this.Name = pi.FieldKey;
            Caption = pi.FieldCaption;
            InputText = pi.FieldDefault;
            Checked = (!"".Equals(InputText)) ? true : false;

            this.chkUse.Click += chkUse_Click;
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

        #region Public Methods
        public string GetCondition(ref bool isFirstCondition)
        {
            string result = string.Empty;

            if (!Checked || "".Equals(InputText))
                return string.Empty;

            result = string.Format(" {0} {1} = '{2}'",
                isFirstCondition ? "WHERE" : "AND",
                _paramInfo.FieldKey,
                InputText);
            isFirstCondition = false;

            return result;
        }
        #endregion
    }
}
