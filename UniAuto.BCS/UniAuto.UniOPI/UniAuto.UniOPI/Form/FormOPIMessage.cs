using System;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormOPIMessage : FormBase
    {
        #region Defind

        //當為Error時顯示的字樣
        private const string DISPLAY_ERR_STRING = "Error";

        #endregion

        public FormOPIMessage()
        {
            InitializeComponent();

            tmrBaseRefresh.Enabled = true;
        }

        private void SetMessage(OPIMessage opiMessage)
        {
            StringBuilder _sbMessageText = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(opiMessage.MsgCode))
            {
                _sbMessageText.Append("Code : ").AppendLine(opiMessage.MsgCode);
            }

            _sbMessageText.AppendFormat(opiMessage.MsgData);
            txtMsg.Text = _sbMessageText.ToString();

            dgvMsg.Rows.Insert(0, opiMessage.MsgType.ToString(), opiMessage.MsgDateTime, txtMsg.Text, opiMessage.MsgData, opiMessage.MsgCaption);

            //移至第一筆
            dgvMsg.CurrentCell = dgvMsg.Rows[0].Cells[colMessageBoxIcon.Name];

            //當Error則顯示紅色
            if (dgvMsg.CurrentRow.Cells[colMessageBoxIcon.Name].Value.Equals(DISPLAY_ERR_STRING))
                dgvMsg.CurrentRow.DefaultCellStyle.ForeColor = Color.Red;
            else
                dgvMsg.CurrentRow.DefaultCellStyle.ForeColor = Color.Black;

            SetIcon(opiMessage.MsgType.ToString());
            lblCaption.Text = opiMessage.MsgCaption;
        }

        private void tmrBaseRefresh_Tick(object sender, EventArgs e)
        {
            //自動Pop最新訊息
            try
            {
                OPIMessage _msg;

                while (OPIInfo.Q_OPIMessage.Count > 0)
                {
                    lock (OPIInfo.Q_OPIMessage)
                    {
                        _msg = OPIInfo.Q_OPIMessage.Dequeue();
                    }

                    SetMessage(_msg);

                    this.TopMost = true;
                    this.Visible = true;
                    
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        #region Event

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void dgvMsg_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dgvMsg.Columns[colMessage.Name].Index)
            {
                DataGridViewCell dgvc = dgvMsg.Rows[e.RowIndex].Cells[e.ColumnIndex];

                //將stackTrace的部份顯示於toolTipText
                string stackTrace = string.Empty;
                if (!string.IsNullOrWhiteSpace(dgvMsg.Rows[e.RowIndex].Cells[colStackTrace.Name].Value.ToString()))
                {
                    stackTrace = dgvMsg.Rows[e.RowIndex].Cells[colStackTrace.Name].Value.ToString(); // string.Format(" stackTrace:[{0}]", dgvMsg.Rows[e.RowIndex].Cells[colStackTrace.Name].Value.ToString());
                }
                else stackTrace = dgvMsg.Rows[e.RowIndex].Cells[colMessage.Name].Value.ToString();
                //string toolTipText = string.Format("{0}{1}", dgvMsg.Rows[e.RowIndex].Cells[colMessage.Name].Value.ToString(), stackTrace);
                dgvc.ToolTipText = stackTrace;
            }
        }        

        private void dgvMsg_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

                DataGridViewRow _row = dgvMsg.Rows[e.RowIndex];

                txtMsg.Text = _row.Cells[colMessage.Name].Value.ToString();
                lblCaption.Text  = _row.Cells[colCaption.Name].Value.ToString();

                SetIcon(_row.Cells[colMessageBoxIcon.Name].Value.ToString());

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void SetIcon(string msgType)
        {
            switch (msgType)
            {
                case "Warning":
                    lblICON.ImageIndex = 0;
                    break;
                case "Error":
                    lblICON.ImageIndex = 1;
                    break;
                case "Question":
                    lblICON.ImageIndex = 2;
                    break;
                case "Asterisk":
                case "Information":
                    lblICON.ImageIndex = 3;
                    break;
                default:
                    break;
            }
        }
        #endregion

        private void FormShowMessage_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            e.Cancel = true; //關閉視窗時取消
            this.Hide();
        }

        private void FormShowMessage_Load(object sender, System.EventArgs e)
        {
            tmrBaseRefresh.Enabled = true;
        }
    }
}
