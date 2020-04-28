using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace UniOPI
{
    public partial class FormBCSMessage : FormBase
    {
        Stopwatch DisplayTimer = new Stopwatch();

        OPIInfo OPIAp { get; set; }

        bool autoPop = true;//20170929 add pop message开关

        public FormBCSMessage(OPIInfo opiAp)
        {
            InitializeComponent();

            this.dgvHisMessage.Rows.Clear();

            OPIAp = opiAp; // FormMainMDI.G_OPIAp;

            this.lblCaption.Text = string.Format("[{0}] BCS Message ", OPIAp.CurLine.ServerName);
            this.tmrRefresh.Enabled = true;             
        }

        public void ShowHistoryPageAndNewMessage()
        {
            this.tbcMessage.SelectedIndex = 1;//切換到History

            GetBCMessage();
            
            if (this.dgvHisMessage.Rows.Count > 0)
            {
                //this.dgvHisMessage.CurrentCell = this.dgvHisMessage.Rows[0].Cells["colTime"];//移動到最新列
                dgvHisMessage_CellClick(dgvHisMessage.Rows[0].Cells[0], new DataGridViewCellEventArgs(0, 0));
            }
            this.Visible = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.tmrShow.Enabled = false;
        }

        private void tmrShow_Tick(object sender, EventArgs e)
        {
            try
            {
                if (FormMainMDI.G_OPIAp.BCSMessageDisplayTime == 0)
                {
                    tmrShow.Enabled = false;
                    return;
                }

                if (DisplayTimer.Elapsed.Seconds > FormMainMDI.G_OPIAp.BCSMessageDisplayTime )
                {
                    this.btnOK.PerformClick();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void dgvHisMessage_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
                
                DataGridViewRow _row = this.dgvHisMessage.Rows[e.RowIndex];

                this.txtMsg.Text = _row.Cells[colMessage.Name].Value.ToString();               
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void tbcMessage_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                TabControl _tab = (TabControl)sender;
                
                if (_tab.SelectedIndex == 1)
                {
                    GetBCMessage();

                    if (this.dgvHisMessage.Rows.Count > 0)
                    {
                        this.dgvHisMessage.ClearSelection();

                        this.dgvHisMessage.CurrentCell = this.dgvHisMessage.Rows[0].Cells["colTime"];

                        this.txtMsg.Text = this.dgvHisMessage.Rows[0].Cells[colMessage.Name].Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            //[2014-08-23 00:00:00]  BCS Message Text
            //自動Pop最新訊息
            try
            {
                //if (FormMainMDI.G_OPIAp.Q_BCMessage.Count > 0)
                if (OPIInfo.Q_BCMessage.Count > 0)
                {
                    string msg = OPIInfo.Q_BCMessage.Dequeue();

                    string[] msgItem = msg.Split('^');

                    //TrxID,DateTime,Message
                    this.txtMessage.Text = string.Format("{0}\r\n\r\n{1}\r\n\r\n{2}", msgItem[0], msgItem[1], msgItem[2]);

                    //this.dgvHisMessage.Rows.Insert(0, new string[] { msgItem[1], msgItem[2], msgItem[0] });


                    ////保留50筆最新的紀錄
                    //if (this.dgvHisMessage.Rows.Count > 50) this.dgvHisMessage.Rows.RemoveAt(50);

                    //this.dgvHisMessage.Refresh();

                    //移動到最新列
                    //dgvHisMessage.CurrentCell = dgvHisMessage.Rows[dgvHisMessage.Rows.Count - 1].Cells["colTime"];

                    //if (this.Visible)
                    //{
                    //    this.TopMost = true;
                    //}
                    //else
                    //{
                        this.tbcMessage.SelectedIndex = 0;//FormBcMessage重新顯示時切換到Message Page
                        this.TopMost = autoPop;
                        this.Visible = autoPop;//huangjiayin modify
                    //}

                    this.tmrShow.Enabled = true;

                    this.DisplayTimer.Restart();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void FormBCSMessage_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //關閉視窗時取消
            this.Hide();
            tmrShow.Enabled = false;
        }

        private void GetBCMessage()
        {
            try
            {
                var Q_BCMessage = (from msg in OPIAp.DBBRMCtx.SBCS_TERMINALMESSAGE_TRX
                                   orderby msg.UPDATETIME descending
                                   select new { msg.UPDATETIME, msg.TERMINALTEXT, msg.TRANSACTIONID }).Take(OPIAp.QueryMaxCount).ToList();

                if (Q_BCMessage.Count == 0 || Q_BCMessage == null)
                {
                    dgvHisMessage.DataSource = null;
                    return;
                }

                dgvHisMessage.DataSource = Q_BCMessage;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void chk_AutoPop_CheckedChanged(object sender, EventArgs e)//huangjiayin add
        {
            CheckBox chk = (CheckBox)sender;
            this.autoPop = chk.Checked;
        }

    }
}
