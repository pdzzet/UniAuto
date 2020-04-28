using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobotCommandHis : FormBase
    {
        OPIInfo OPIAp { get; set; } 

        public FormRobotCommandHis(OPIInfo opiAp)
        {
            InitializeComponent();

            OPIAp = opiAp;
        }

        private void FormRobotCommandHis_Load(object sender, EventArgs e)
        {
            try
            {
                GetMessageHis();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void GetMessageHis()
        {
            try
            {
                var _rbMessage = (from msg in OPIAp.DBBRMCtx.SBCS_ROBOTMESSAGE_TRX
                                  where msg.SERVERNAME.Equals(OPIAp.CurLine.ServerName)
                                   orderby msg.UPDATETIME descending
                                   select new { msg.UPDATETIME,msg.MESSAGETYPE, msg.TERMINALTEXT, msg.TRANSACTIONID }).Take(OPIAp.QueryMaxCount).ToList();

                if (_rbMessage.Count == 0 || _rbMessage == null)
                {
                    txtMsg.Text = string.Empty;
                    dgvMessage.DataSource = null;
                    return;
                }
                else
                {
                    dgvMessage.DataSource = _rbMessage;
                    dgvMessage_CellClick(dgvMessage.Rows[0].Cells[0], new DataGridViewCellEventArgs(0, 0));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvMessage_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

                DataGridViewRow _row = this.dgvMessage.Rows[e.RowIndex];

                this.txtMsg.Text = _row.Cells[colMessage.Name].Value.ToString();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
