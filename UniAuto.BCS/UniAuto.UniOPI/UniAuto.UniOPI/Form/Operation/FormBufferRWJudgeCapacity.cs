using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormBufferRWJudgeCapacity : FormBase
    {
        Node CurNode;

        public FormBufferRWJudgeCapacity()
        {
            InitializeComponent();
        }

        private void FormBufferRWJudgeCapacity_Load(object sender, EventArgs e)
        {
            if (FormMainMDI.G_OPIAp.CurLine.CV06_Node == null) lblCV06Node.Text = string.Empty;
            else
            {
                CurNode = FormMainMDI.G_OPIAp.CurLine.CV06_Node;
                lblCV06Node.Text = string.Format("[ {0} ] - [ {1} ] - {2}", CurNode.NodeNo, CurNode.NodeID, CurNode.NodeName);
            }

            txtOperatorCmd.Text = FormMainMDI.G_OPIAp.LoginUserID;
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurNode == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Can't find Node CV#06", MessageBoxIcon.Error);
                    return;
                }

                SendtoBC_BufferRWJudgeCapacityChangeReportRequest(CurNode.NodeNo);
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
                if (CheckData() == false) return;

                SendtoBC_BufferRWJudgeCapacityChangeRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                rdoEnableCmd.Checked = false;
                rdoDisableCmd.Checked = false;
                rdoChangeCmd.Checked = false;

                txtBF01Cmd.Text = string.Empty;
                txtBF02Cmd.Text = string.Empty;
                txtOperatorCmd.Text = FormMainMDI.G_OPIAp.LoginUserID;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SendtoBC_BufferRWJudgeCapacityChangeReportRequest(string LocalNo)
        {
            try
            {
                ClearQueryData();

                #region Send Buffer RW Judge Capacity Change Report Request

                BufferRWJudgeCapacityChangeReportRequest _trx = new BufferRWJudgeCapacityChangeReportRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                _trx.BODY.EQUIPMENTNO = LocalNo;


                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region BufferRWJudgeCapacityChangeReportReply

                string _respXml = _resp.Xml;

                BufferRWJudgeCapacityChangeReportReply _bufferRWJudgeCapacityChangeReportReply = (BufferRWJudgeCapacityChangeReportReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data

                if (_bufferRWJudgeCapacityChangeReportReply.BODY.BUFFERJUDGECAPACITY == "1") rdoEnableQuery.Checked =true;
                else if (_bufferRWJudgeCapacityChangeReportReply.BODY.BUFFERJUDGECAPACITY == "2") rdoDisableQuery.Checked =true;
                else if (_bufferRWJudgeCapacityChangeReportReply.BODY.BUFFERJUDGECAPACITY == "3") rdoChangeQuery.Checked =true;

                txtBF01Query.Text = _bufferRWJudgeCapacityChangeReportReply.BODY.BF01RWJUDGECAPACITY;
                txtBF02Query.Text = _bufferRWJudgeCapacityChangeReportReply.BODY.BF02RWJUDGECAPACITY;
                txtOperatorQuery.Text = _bufferRWJudgeCapacityChangeReportReply.BODY.OPERATORID;
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SendtoBC_BufferRWJudgeCapacityChangeRequest()
        {
            try
            {
                
                #region Send Buffer RW Judge Capacity Change Report Request

                BufferRWJudgeCapacityChangeRequest _trx = new BufferRWJudgeCapacityChangeRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                _trx.BODY.EQUIPMENTNO = CurNode.NodeNo;
                _trx.BODY.BUFFERJUDGECAPACITY=  GetBufferJudgeCapacity();
                _trx.BODY.BF01RWJUDGECAPACITY = txtBF01Cmd.Text.ToString().Trim() == string.Empty ? "0" : txtBF01Cmd.Text.ToString().Trim();
                _trx.BODY.BF02RWJUDGECAPACITY = txtBF02Cmd.Text.ToString().Trim() == string.Empty ? "0" : txtBF02Cmd.Text.ToString().Trim();
                _trx.BODY.OPERATORID = txtOperatorCmd.Text.ToString();


                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region BufferRWJudgeCapacityChangeReply

                ShowMessage(this, lblCaption.Text, "", "Buffer RW Judge Capacity Change Send to BC Success !", MessageBoxIcon.Information);

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ClearQueryData()
        {
            try
            {
                rdoEnableQuery.Checked = false;
                rdoDisableQuery.Checked = false;
                rdoChangeQuery.Checked = false;
                txtBF01Query.Text = string.Empty;
                txtBF02Query.Text = string.Empty;
                txtOperatorQuery.Text = string.Empty;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private bool CheckData()
        {
            try
            {
                int _num = 0;

                if (rdoEnableCmd.Checked == false && rdoDisableCmd.Checked == false && rdoChangeCmd.Checked == false)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Buffer Judge Capacity", MessageBoxIcon.Error);
                    return false ;
                }

                if (rdoEnableCmd.Checked == true || rdoChangeCmd.Checked == true)
                {
                    if (txtBF01Cmd.Text.ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please input Buffer#01 RW Judge Capacity ", MessageBoxIcon.Error);
                        txtBF01Cmd.Focus();
                        return false;
                    }

                    int.TryParse(txtBF01Cmd.Text.ToString(), out _num);

                    if (_num > 65535)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Buffer#01 RW Judge Capacity <=65535 ", MessageBoxIcon.Error);
                        txtBF01Cmd.Focus();
                        return false;
                    }

                    if (txtBF02Cmd.Text.ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please input Buffer#02 RW Judge Capacity ", MessageBoxIcon.Error);
                        txtBF02Cmd.Focus();
                        return false;
                    }

                    int.TryParse(txtBF02Cmd.Text.ToString(), out _num);

                    if (_num > 65535)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Buffer#02 RW Judge Capacity <=65535 ", MessageBoxIcon.Error);
                        txtBF02Cmd.Focus();
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return false;
            }
        }

        private string GetBufferJudgeCapacity()
        {
            if (rdoEnableCmd.Checked) return "1";
            if (rdoDisableCmd.Checked) return "2";
            if (rdoChangeCmd.Checked) return "3";
            return string.Empty;
        }

        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                asciiCode == 8)   // Backspace
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }
    }
}

