using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormParamManagementEdit_SelectReport : FormBase
    {
        private string _SelectedValue;

        public string SelectedValue
        {
            get { return _SelectedValue; }
            set
            {
                string[] stTmp = value.Split(',');
                foreach (string strKey in stTmp)
                {
                    for (int i = 0; i < chklstReportType.Items.Count; i++)
                    {
                        if (chklstReportType.Items[i].ToString() == strKey)
                        {
                            chklstReportType.SetItemChecked(i, true);
                        }
                    }
                }
            }
        }

        public FormParamManagementEdit_SelectReport()
        {
            InitializeComponent();

            this.lblCaption.Text = "Select ReportTo";
            // 'MES' / 'EDA' / 'OEE' / 'APC'
            this.chklstReportType.Items.Add("MES", false);
            this.chklstReportType.Items.Add("EDA", false);
            this.chklstReportType.Items.Add("OEE", false);
            this.chklstReportType.Items.Add("APC", false);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> lstType = new List<string>();

                _SelectedValue = "";

                for (int i = 0; i < chklstReportType.Items.Count; i++)
                {
                    if (chklstReportType.GetItemChecked(i))
                    {
                        string str = chklstReportType.Items[i].ToString();
                        lstType.Add(str);
                    }
                }

                _SelectedValue = string.Join(",", lstType.ToArray());
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
