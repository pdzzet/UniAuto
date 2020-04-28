using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class ucComBox_His : UserControl, iHistBase
    {
        #region Fields
        private ParamInfo _paramInfo;
        #endregion

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
            get { return this.gbxCombox.Text; }
            set { this.gbxCombox.Text = value; }
        }
        #endregion

        public ucComBox_His(ParamInfo pi)
        {
            InitializeComponent();

            this._paramInfo = pi;

            this.Name = pi.FieldKey;
            Caption = pi.FieldCaption;
            this.BindDataSource();

            this.chkUse.Click += chkUse_Click;

            //if (Param.RelationField.ToUpper() == "Y")
            //{
 
            //}
        }

        #region Events
        private void chkUse_Click(object sender, EventArgs e)
        {
            CheckBox objChk = (CheckBox)sender;
            if (objChk.Checked)
            {
                cmbItem.Focus();
                cmbItem.DroppedDown = true;
            }
        }

        #endregion

        #region Private Methods
        public bool BindDataSource()
        {
            List<string> lstDisplay = _paramInfo.FieldDisplay.Split(new char[] { ',' }, StringSplitOptions.None).ToList<string>();
            List<string> lstValue = _paramInfo.FieldValue.Split(new char[] { ',' }, StringSplitOptions.None).ToList<string>();

            if (lstDisplay.Count != lstValue.Count)
                return false;

            DataTable dtSource = new DataTable();
            dtSource.Columns.Add("DisplayField");
            dtSource.Columns.Add("ValueField");

            DataRow drNew;
            for (int idx = 0; idx < lstDisplay.Count; idx++)
            {
                drNew = dtSource.NewRow();
                drNew["DisplayField"] = lstDisplay[idx];
                drNew["ValueField"] = lstValue[idx];
                dtSource.Rows.Add(drNew);
            }

            this.cmbItem.DataSource = dtSource;
            this.cmbItem.DisplayMember = "DisplayField";
            this.cmbItem.ValueMember = "ValueField";
            this.cmbItem.SelectedValue = _paramInfo.FieldDefault;

            //Checked = (!"".Equals(_paramInfo.FieldDefault)) ? true : false;

            return true;
        }
        #endregion

        #region Public Methods
        public string GetCondition(ref bool isFirstCondition)
        {
            string _value = string.Empty;
            string result = string.Empty;

            if (!Checked) return string.Empty;
            //重新绑定数据Default Value 为“”
            //添加防呆（人员选完一个Item查询后，手Key入其他的不存在的值[这时候SelectedIndex=-1，Value=Null]，例如A
            //再选时间区间Query，再绑定时Combox对应SelectedValue为“”，如果Combox中存在为“”的Item,这时SelectedIndex会为0，而Combox显示为A,就会找出Item为“”的History）
            if (cmbItem.SelectedIndex < 0 || (cmbItem.SelectedIndex == 0 && cmbItem.SelectedValue.ToString().Trim()==""))
            {
                if (cmbItem.Text.ToString().Trim() != string.Empty)
                    _value = cmbItem.Text.ToString().Trim();
            }
            else _value= cmbItem.SelectedValue.ToString();

            result = string.Format(" {0} {1} = '{2}'",
                isFirstCondition ? "WHERE" : "AND",
                _paramInfo.FieldKey, _value);
            isFirstCondition = false;

            return result;
        }

        #endregion
    }

    public interface iHistBase
    {
        string GetCondition(ref bool isFirstCondition);
    }
}
