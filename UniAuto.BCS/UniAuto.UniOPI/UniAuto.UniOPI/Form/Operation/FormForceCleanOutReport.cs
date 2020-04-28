using System;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormForceCleanOutReport : FormBase
    {
        string ForceCleanOutType = string.Empty;

        public FormForceCleanOutReport(string type)
        {
            InitializeComponent();

            ForceCleanOutType = type;
        }

        private void FormForceCleanOutReport_Load(object sender, EventArgs e)
        {
            try
            {
                txtBCCommand.Text = string.Empty;
                dgvData.Rows.Clear();

                switch (ForceCleanOutType)
                {
                    case "FORCECLEAN":
                        lblCaption.Text = "Force CleanOut Command Report";
                        break;

                    case "ABNORMAL":
                        lblCaption.Text = "Abnormal Force CleanOut Command Report";
                        break;
                    default:
                        break;
                }

                ForceCleanOutCommandReportRequest _trx = new ForceCleanOutCommandReportRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.COMMANDTYPE = ForceCleanOutType;
         
                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SocketResponseTime_Query);

                if (_resp == null) return;


                #region ForceCleanOutCommandReportReply

                string _respXml = _resp.Xml;

                ForceCleanOutCommandReportReply _forceCleanOutCommandReportReply = (ForceCleanOutCommandReportReply)Spec.CheckXMLFormat(_respXml);

                SetData(_forceCleanOutCommandReportReply);


                #endregion

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

        private string BitOnOff(string bit)
        {
            string ret = bit;
            if (bit == "0") ret = "OFF";
            else if (bit == "1") ret = "ON";
            return ret;
        }

        private void SetData(ForceCleanOutCommandReportReply Reply)
        {
            try
            {
                string _local = string.Empty;

                txtBCCommand.Text = BitOnOff(Reply.BODY.SETSTATUS);
                txtUserID.Text = Reply.BODY.USERID;

                foreach (ForceCleanOutCommandReportReply.COMMANDc _cmd in Reply.BODY.COMMANDLIST)
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_cmd.EQUIPMENTNO))
                    {
                        _local = _cmd.EQUIPMENTNO + " - " + FormMainMDI.G_OPIAp.Dic_Node[_cmd.EQUIPMENTNO].NodeID;
                        dgvData.Rows.Add(_local, BitOnOff(_cmd.BCSTATUS), BitOnOff(_cmd.EQSTATUS));
                    }
                    else
                    {
                        ShowMessage(this, lblCaption.Text.ToString(), "", string.Format("Can't find Local No[{0}]", _cmd.EQUIPMENTNO), MessageBoxIcon.Error);
                    }
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
