using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormLinkStatus : FormBase
    {

        ToolTip Tip;

        public FormLinkStatus()
        {
            InitializeComponent();

            lblCaption.Text = "Link Status";
        }

        private void FormLinkStatus_Load(object sender, EventArgs e)
        {
            this.InitialLightsLabel();

            Tip = new ToolTip();

            Tip.SetToolTip(lblBatonPassStatus_W, "SW0047-Baton pass status(own station),Stores the communication status of own station, expression= HEX");
            Tip.SetToolTip(lblBatonPassInterruption_W, "SW0048-Baton pass Interruption, Stores the cause of interrupting communication (baton pass) of the own station. expression=HEX");
            Tip.SetToolTip(lblDataLinkStop_W, "SW0049-Data Link Stop, Cause of data link stop,Stores the cause of stopping data link of the own station., expression=HEX");
            Tip.SetToolTip(lblStationLoopStatus_W, "SW0064-Checking for cable disconnections and cable insertion errors of own station When there is a cable disconnection or cable insertion error, " + Environment.NewLine + "Own station's loop status (SB0064) is turned ON Own station's loop status,Stores the transmission path status of the own station. , expression=HEX");
            Tip.SetToolTip(lblBatonPassStatus, "SW00A0 to SW00A7-Baton pass status of each station,Stores the baton pass status of each station. expression=BIN");
            Tip.SetToolTip(lblCyclicTransmissionStatus, "SW00B0 to SW00B7 - Cyclic transmission status of each station,Stores the cyclic transmission status of each station. expression=BIN");
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            Send_EquipmentDataLinkStatusRequst();
        }

        private void InitialLightsLabel()
        {
            try
            {
                Font f = new Font("Carlibri", 12);

                flpBatonPassStatus.Controls.Clear();
                flpCyclicTransmissionStatus.Controls.Clear();
                int idx = 1;

                string _desc = string.Empty;

                #region MPLC
                flpBatonPassStatus.Controls.Add(new Label()
                {
                    Name = string.Format("lblB{0}", idx.ToString("000")),
                    Text = "MPLC",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(110, 35),
                    BackColor = Color.FromKnownColor(KnownColor.ControlLight),
                    Font = f,
                    Margin = new Padding(1),
                    Tag = idx.ToString("000")
                });

                flpCyclicTransmissionStatus.Controls.Add(new Label()
                {
                    Name = string.Format("lblC{0}", idx.ToString("000")),
                    Text = "MPLC",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(110, 35),
                    BackColor = Color.FromKnownColor(KnownColor.ControlLight),
                    Font = f,
                    Margin = new Padding(1),
                    Tag = idx.ToString("000")
                });

                idx = idx + 1;

                #endregion

                #region Node
                foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                {
                    _desc = _node.NodeNo;

                    //SECS不會有對應位置，給999顯示灰色不更新
                    if (_node.ReportMode.Contains("PLC")) int.TryParse(_desc.Substring(1), out idx);
                    else idx = 999;
                    
                    flpBatonPassStatus.Controls.Add(new Label()
                    {
                        Name = string.Format("lblB{0}", idx.ToString("000")),
                        Text = _desc,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Size = new Size(110, 35),
                        BackColor = (idx==999? Color.LightSlateGray: Color.FromKnownColor(KnownColor.ControlLight)),
                        Font = f,
                        Margin = new Padding(1),
                        Tag = idx.ToString("000")

                        
                    });

                    flpCyclicTransmissionStatus.Controls.Add(new Label()
                    {
                        Name = string.Format("lblC{0}", idx.ToString("000")),
                        Text = _desc,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Size = new Size(110, 35),
                        BackColor =(idx==999? Color.LightSlateGray: Color.FromKnownColor(KnownColor.ControlLight)),
                        Font = f,
                        Margin = new Padding(1),
                        Tag = idx.ToString("000")
                    });

                    //idx = idx + 1;

                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        public void Send_EquipmentDataLinkStatusRequst()
        {
            try
            {
                string _err = string.Empty;
                EquipmentDataLinkStatusRequst _trx = new EquipmentDataLinkStatusRequst();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), out _err, FormMainMDI.G_OPIAp.SessionID);

                FormMainMDI.G_OPIAp.BC_EquipmentDataLinkStatusReply.IsReply = false;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        public void GetDataLinkStatus()
        {
            try
            {
                BCS_EquipmentDataLinkStatusReply reply = FormMainMDI.G_OPIAp.BC_EquipmentDataLinkStatusReply;

                txtBatonPassStatus_W.Text = reply.BatonPassStatus_Desc(); ;
                txtBatonPassInterruption_W.Text = reply.BatonPassInterruption_Desc();
                txtDataLinkStop_W.Text = reply.DataLinkStop_Desc();
                txtStationLoopStatus_W.Text = reply.StationLoopStatus_Desc();

                int idx = 0;
                char[] chAry = reply.BatonPassStatus_B.ToArray();
                string nodeNo = string.Empty;
                foreach (Label lbl in flpBatonPassStatus.Controls.OfType<Label>().OrderBy(d => d.Tag))
                {
                    int.TryParse(lbl.Tag.ToString(), out idx);

                    idx = idx - 1;

                    if (idx >= chAry.Length)
                        break;

                    switch (chAry[idx])
                    {
                        case '0':
                            lbl.BackColor = Color.Lime;
                            break;
                        case '1':
                            lbl.BackColor = Color.Red;
                            break;
                    }
                }

                chAry = reply.CyclicTransmissionStatus_B.ToArray();
                foreach (Label lbl in flpCyclicTransmissionStatus.Controls.OfType<Label>().OrderBy(d => d.Tag))
                {
                    int.TryParse(lbl.Tag.ToString(), out idx);

                    idx = idx - 1;

                    if (idx >= chAry.Length)
                        break;

                    switch (chAry[idx])
                    {
                        case '0':
                            lbl.BackColor = Color.Lime;
                            break;
                        case '1':
                            lbl.BackColor = Color.Red ;
                            break;
                    }

                    //idx++;
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
                if (FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString()) return;

                GetDataLinkStatus();

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }
    }
}
