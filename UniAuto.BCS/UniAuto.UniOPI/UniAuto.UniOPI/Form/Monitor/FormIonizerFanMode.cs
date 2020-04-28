using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormIonizerFanMode : FormBase
    {

        Node CurNode;

        public FormIonizerFanMode()
        {
            InitializeComponent();

            lblCaption.Text = "Ionizer Fan Mode";
        }

        private void FormIonizerFanMode_Load(object sender, EventArgs e)
        {
            try
            {
                CurNode = null;
                this.InitialCombox();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cmbNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                List<Label> lstLabel = (from l in tlpFan.Controls.OfType<Label>()
                                        orderby l.Name
                                        select l).ToList();

                foreach (Label lbl in lstLabel)
                {
                    lbl.BackColor = Color.FromKnownColor(KnownColor.ControlLight);
                }

                if (cmbNode.SelectedIndex < 0)
                {
                    CurNode = null;
                    ShowMessage(this, this.lblCaption.Text, "", "Local No required！", MessageBoxIcon.Warning);
                    cmbNode.DroppedDown = true;
                    return;
                }

                string _localNo = ((dynamic)cmbNode.SelectedItem).NodeNo;

                if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_localNo))
                {
                    CurNode = FormMainMDI.G_OPIAp.Dic_Node[_localNo];

                    Send_IonizerFanModeReportRequest(CurNode.NodeNo);

                    tmrRefresh.Enabled = true;
                }
                else
                {
                    CurNode = null;
                    ShowMessage(this, this.lblCaption.Text, "", string.Format("Can't find Equipment[{0}]", _localNo), MessageBoxIcon.Warning);
                    cmbNode.DroppedDown = true;
                    return;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurNode == null)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Local No required！", MessageBoxIcon.Warning);
                    cmbNode.DroppedDown = true;
                    return;
                }

                Send_IonizerFanModeReportRequest(CurNode.NodeNo);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialCombox()
        {
            try
            {
                var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                         select new
                         {
                             IdNmae = string.Format("{0}-{1}", node.NodeNo, node.NodeID),
                             NodeNo = node.NodeNo,
                             NodeID = node.NodeID
                         }).ToList();

                if (q == null || q.Count == 0) return;

                cmbNode.SelectedIndexChanged -= new EventHandler(cmbNode_SelectedIndexChanged);
                cmbNode.DataSource = q;
                cmbNode.DisplayMember = "IdNmae";
                cmbNode.ValueMember = "NodeNo";
                cmbNode.SelectedIndex = -1;
                cmbNode.SelectedIndexChanged += new EventHandler(cmbNode_SelectedIndexChanged);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void SwitchFanMode(string EnableMode)
        {
            try
            {                
                char[] chrEnableMode = EnableMode.ToArray();
                if (chrEnableMode.Length <= 0 || chrEnableMode.Length > 32)
                    return;

                List<Label> lstLabel = (from l in tlpFan.Controls.OfType<Label>()
                                        orderby l.Name
                                        select l).ToList();

                Label lbl;
                for (int idx = 0; idx < lstLabel.Count; idx++)
                {
                    lbl = lstLabel[idx];
                    switch (chrEnableMode[idx])
                    {
                        case '0':
                            lbl.BackColor = Color.Red;
                            break;
                        case '1':
                            lbl.BackColor = Color.Lime;
                            break;
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
            try
            {
                if (FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString())
                {
                    tmrRefresh.Enabled = false;
                    return;
                }

                if (CurNode == null) return;

                #region Send Request
                                
                DateTime _now = DateTime.Now;

                TimeSpan _ts = _now.Subtract(CurNode.BC_IonizerFanModeReportReply.LastRequestDate).Duration();

                if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                {

                    if (CurNode.BC_IonizerFanModeReportReply.IsReply)
                    {
                        Send_IonizerFanModeReportRequest(CurNode.NodeNo);
                    }
                }                
                
                #endregion

                SwitchFanMode(CurNode.BC_IonizerFanModeReportReply.EnableMode);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public void Send_IonizerFanModeReportRequest(string NodeNo)
        {
            try
            {
                string _err = string.Empty;

                IonizerFanModeReportRequest _trx = new IonizerFanModeReportRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = NodeNo;    

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), out _err, FormMainMDI.G_OPIAp.SessionID);

                CurNode.BC_IonizerFanModeReportReply.IsReply = false;

                CurNode.BC_IonizerFanModeReportReply.LastRequestDate = DateTime.Now;
                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }
    }
}
