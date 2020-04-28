using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormRobotOperationMode : FormBase
    {
        Node IndexerNode = null;
        string CurPositionNo = string.Empty;

        public FormRobotOperationMode()
        {
            InitializeComponent();
        }

        private void FormRobotOperationMode_Load(object sender, EventArgs e)
        {
            try
            {
                if (FormMainMDI.G_OPIAp.CurLine.IndexerNode == null)
                {
                    btnSend.Enabled = false;
                    tmrBaseRefresh.Enabled = false;
                    ShowMessage(this, lblCaption.Text, "", "Can't find indexer equipment", MessageBoxIcon.Error);
                    return;
                }

                IndexerNode = FormMainMDI.G_OPIAp.CurLine.IndexerNode;

                #region Load Robot operation mode
                List<IndexerRobotStage> _lstStages = FormMainMDI.G_OPIAp.CurLine.BC_RobotOperationModeReply.IndexerRobotStages;
                foreach (IndexerRobotStage _stage in _lstStages)
                {
                    dgvRobotOperMode.Rows.Add(_stage.LocalNo, _stage.LocalID, _stage.RobotPosNo, _stage.RobotPosNo + ":" + _stage.Description, _stage.Direction, _stage.OperationMode.ToString());
                }

                if (dgvRobotOperMode.Rows.Count > 0)
                {
                    dgvRobotOperMode_CellClick(dgvRobotOperMode.Rows[0].Cells[0], new DataGridViewCellEventArgs(0, 0));
                }

                DataTable _dtRobotOperMode = UniTools.InitDt(new string[] { "NEW_MODE", "NEW_MODE_DESC" });

                foreach (string _operMode in Enum.GetNames(typeof(eRobotOperMode)))
                {
                    int _value = (int)Enum.Parse(typeof(eRobotOperMode), _operMode);
                    if (_value == 0) continue;

                    eRobotOperMode robotOperMode = (eRobotOperMode)Enum.Parse(typeof(eRobotOperMode), _operMode);
                    string desc = Public.GetEnumDesc(robotOperMode);

                    DataRow drNew = _dtRobotOperMode.NewRow();
                    drNew["NEW_MODE"] = _value;
                    drNew["NEW_MODE_DESC"] = string.Format("{0}:{1}", _value, desc);
                    _dtRobotOperMode.Rows.Add(drNew);
                }
                cbbNewMode.DataSource = _dtRobotOperMode;
                cbbNewMode.DisplayMember = "NEW_MODE_DESC";
                cbbNewMode.ValueMember = "NEW_MODE";
                cbbNewMode.SelectedIndex = -1;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_RobotOperatiobModeRequest()
        {
            try
            {
                string _err = string.Empty;
                string _xml = string.Empty;

                RobotOperationModeRequest _trx = new RobotOperationModeRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                
                if (FormMainMDI.G_OPIAp.CurLine.IndexerNode != null)
                {
                    _trx.BODY.EQUIPMENTNO = FormMainMDI.G_OPIAp.CurLine.IndexerNode.NodeNo;
                }
                else
                    return;
                _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                FormMainMDI.G_OPIAp.CurLine.BC_RobotOperationModeReply.LastRequestDate = DateTime.Now;

                FormMainMDI.G_OPIAp.CurLine.BC_RobotOperationModeReply.IsReply = false;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void tmrBaseRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString())
                {
                    tmrBaseRefresh.Enabled = false;
                    return;
                }

                if (FormMainMDI.G_OPIAp.CurLine.IndexerNode == null)
                {
                    tmrBaseRefresh.Enabled = false;
                    return;
                }

                BCS_RobotOperationModeReply _reply = FormMainMDI.G_OPIAp.CurLine.BC_RobotOperationModeReply;

                if (_reply.IsReply)
                {
                    DateTime _now = DateTime.Now;
                    TimeSpan _ts = _now.Subtract(_reply.LastRequestDate).Duration();

                    if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                    {
                        Send_RobotOperatiobModeRequest();
                    }
                }

                RefreshData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvRobotOperMode_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

                CurPositionNo = dgvRobotOperMode.CurrentRow.Cells[colPositionNo.Name].Value.ToString();

                lblLocal.Text = string.Format("{0} - {1}", dgvRobotOperMode.CurrentRow.Cells[colLocalNo.Name].Value.ToString(), dgvRobotOperMode.CurrentRow.Cells[colLocalID.Name].Value.ToString());
                lblDesc.Text = dgvRobotOperMode.CurrentRow.Cells[colDescription.Name].Value.ToString();
                lblAction.Text = dgvRobotOperMode.CurrentRow.Cells[colAction.Name].Value.ToString();
                lblCurrentMode.Text = dgvRobotOperMode.CurrentRow.Cells[colCurMode.Name].Value.ToString();

                cbbNewMode.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurPositionNo == string.Empty )
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Data!!", MessageBoxIcon.Error);
                    return;
                }

                if (cbbNewMode.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose the New Mode", MessageBoxIcon.Error);
                    return;
                }

                string _newMode = cbbNewMode.SelectedValue.ToString();

                string _newModeDesc = ((eRobotOperationMode)(int.Parse(_newMode))).ToString();

                string msg = string.Format("Please congirm whether you will chage the Robot operation mode of position : {0} into [{1}]", lblDesc.Text.ToString(), _newModeDesc);

                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region Send RobotOperationModeCommand
                RobotOperationModeCommand _trx = new RobotOperationModeCommand();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;

                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = IndexerNode.NodeNo;
                _trx.BODY.ROBOTPOSITIONNO = CurPositionNo;
                _trx.BODY.OPERATIONMODE = _newMode;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null)
                    return;

                ShowMessage(this, lblCaption.Text, "", "Robot Operation Mod eCommand Send to BC Success !", MessageBoxIcon.Information);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                Send_RobotOperatiobModeRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void RefreshData()
        {
            try
            {
                List<IndexerRobotStage> _lstStages = FormMainMDI.G_OPIAp.CurLine.BC_RobotOperationModeReply.IndexerRobotStages;

                foreach (DataGridViewRow _row in dgvRobotOperMode.Rows)
                {
                    string _positionNo = _row.Cells[colPositionNo.Name].Value.ToString();

                    IndexerRobotStage _stage = _lstStages.Find(s => s.RobotPosNo == _positionNo);

                    if (_stage == null) continue;

                    if (_stage.OperationMode.ToString() == _row.Cells[colCurMode.Name].Value.ToString()) continue;

                    _row.Cells[colCurMode.Name].Value = _stage.OperationMode.ToString();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
