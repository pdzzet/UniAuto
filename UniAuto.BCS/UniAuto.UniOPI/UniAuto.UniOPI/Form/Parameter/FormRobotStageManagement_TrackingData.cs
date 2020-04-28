using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobotStageManagement_TrackingData : FormBase
    {
        public string TrackingList;

        public FormRobotStageManagement_TrackingData(string trackingList)
        {
            InitializeComponent();

            TrackingList = trackingList;
        }

        private void FormRobotStageManagement_TrackingData_Load(object sender, EventArgs e)
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                CheckBox _chk;

                var _lstData = (from _job in _ctxBRM.SBRM_SUBJOBDATA
                                where _job.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType && _job.ITEMNAME == "TrackingData"
                                orderby _job.SUBITEMLOFFSET
                                select _job).ToList();

                foreach (SBRM_SUBJOBDATA _data in _lstData)
                {
                    _chk = new CheckBox();
                    _chk.Name = _data.SUBITEMLOFFSET.ToString();
                    _chk.Text = _data.SUBITEMDESC.ToString();
                    _chk.Font = new Font("Calibri", 12);
                    _chk.Size = new Size(250, 30);
                    _chk.Checked = false;

                    flpTrackingData.Controls.Add(_chk);
                }

                if (TrackingList != string.Empty)
                {
                    string[] _items = TrackingList.Split(',');

                    foreach (string _item in _items)
                    {
                        _chk = flpTrackingData.Controls.Find(_item, false).OfType<CheckBox>().First();

                        _chk.Checked = true ;
                    }
                }
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

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                string _data = string.Empty ;

                foreach (CheckBox _chk in flpTrackingData.Controls.OfType<CheckBox>())
                {
                    if (_chk.Checked)
                    {
                        _data = _data + (_data == string.Empty ? string.Empty : ",") + _chk.Name;
                    }
                }

                TrackingList = _data;

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
