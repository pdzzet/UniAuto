using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormClientStatus : FormBase
    {
        private const string CLIENT_INFO_REPLY="ClientInformationReply";

        public FormClientStatus()
        {
            InitializeComponent();
            lblCaption.Text = "Client Connecction Status";
        }

        private void FormClientStatus_Load(object sender, EventArgs e)
        {
            try
            {
                dgvData.DataSource = null;

                ClientInformationRequest _trx = new ClientInformationRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                
                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region ClientInformationReply

                string _respXml = _resp.Xml;

                ClientInformationReply _clientInformationReply = (ClientInformationReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data
                this.dgvData.DataSource = null;

                var q = (from com in _clientInformationReply.BODY.CLIENTLIST
                         select com);

                this.dgvData.DataSource = q.ToList();
                #endregion

                #endregion

                //有權限才能刪除user
                if (FormMainMDI.G_OPIAp.KillUserAuthority.Contains(FormMainMDI.G_OPIAp.LoginGroupID) == false)
                {
                    dgvData.Columns["DISCONNECT"].Visible = false;
                    btnSend.Visible = false;
                }
                else
                {
                    dgvData.Columns["DISCONNECT"].Visible = true;
                    btnSend.Visible = true;
                }
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
                if (dgvData.Rows.Count == 0) return;

                string _userId = string.Empty;

                List<DisconnectUsersRequest.USERc> lstUser = new List<DisconnectUsersRequest.USERc>();

                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    if (dr.Cells[DISCONNECT.Name].Value == null || dr.Cells[DISCONNECT.Name].Value.Equals(false)) continue;
                    DisconnectUsersRequest.USERc user = new DisconnectUsersRequest.USERc();
                    user.USERID = dr.Cells[USERID.Name].Value.ToString();
                    user.USERGROUP = dr.Cells[USERGROUP.Name].Value.ToString();
                    user.LOGINTIME = dr.Cells[LOGINTIME.Name].Value.ToString();
                    user.LOGINSERVERIP = dr.Cells[LOGINIP.Name].Value.ToString();
                    lstUser.Add(user);

                    _userId = _userId + (_userId == string.Empty ? "" : ",") + user.USERID;
                }

                if (_userId == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text ,string.Empty , "Please Choose User ID", MessageBoxIcon.Warning);
                    return;
                }
                string _msg = string.Format("Please confirm whether you will disconnect the user [{0}] ?", _userId);

                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, "User Confirm")) return;

                DisconnectUsersRequest _trx = new DisconnectUsersRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.USERLIST = lstUser;

                //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region DisconnectUsersReply

                ShowMessage(this, lblCaption.Text, "", "Disconnect Users Request Send to BC Success !", MessageBoxIcon.Information);  

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvData_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            try
            {
                if (e.ColumnIndex.Equals(dgvData.Columns[DISCONNECT.Name].Index))
                {

                    if (dgvData.CurrentRow.Cells[USERID.Name].Value.Equals(FormMainMDI.G_OPIAp.LoginUserID) &&
                        dgvData.CurrentRow.Cells[LOGINIP.Name].Value.Equals(FormMainMDI.G_OPIAp.LocalIPAddress) &&
                        dgvData.CurrentRow.Cells[USERGROUP.Name].Value.Equals(FormMainMDI.G_OPIAp.LoginGroupID))
                    {
                        string msg = "Can't choose the login user";
                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
