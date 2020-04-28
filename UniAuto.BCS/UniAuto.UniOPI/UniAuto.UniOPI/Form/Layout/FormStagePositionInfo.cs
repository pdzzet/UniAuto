using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormStagePositionInfo : FormBase
    {
        RobotStage rbStage = null;
        public FormStagePositionInfo(RobotStage stage)
        {
            InitializeComponent();

            rbStage = stage;

            
        }

        private void FormStagePositionInfo_Load(object sender, System.EventArgs e)
        {
            Send_StagePositionInfoRequest();
            tmrBaseRefresh.Enabled = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            tmrBaseRefresh.Enabled = false;
            this.Close();
        }


        private void btnRefresh_Click(object sender, System.EventArgs e)
        {
            Send_StagePositionInfoRequest();
        }

        private void ChangeStageInfo()
        {
            try
            {
                if (rbStage.BC_StagePositionInfoReply != null)
                {
                    BCS_StagePositionInfoReply _stagePositionInfoReply = rbStage.BC_StagePositionInfoReply;

                    foreach (Label _lbl in flpPosition.Controls.OfType<Label>())
                    {
                        switch (_lbl.Tag.ToString())
                        {
                            case "SENDYREADY":
                                if (_stagePositionInfoReply.SendReady) _lbl.Image = Properties.Resources.Bit_Green;
                                else _lbl.Image = Properties.Resources.Bit_Sliver;
                                break;
                            case "RECEIVEREADY":
                                if (_stagePositionInfoReply.ReceiveReady) _lbl.Image = Properties.Resources.Bit_Green;
                                else _lbl.Image = Properties.Resources.Bit_Sliver;
                                break;
                            case "EXCHANGEPOSSIBLE":
                                if (_stagePositionInfoReply.ExchangePossible) _lbl.Image = Properties.Resources.Bit_Green;
                                else _lbl.Image = Properties.Resources.Bit_Sliver;
                                break;
                            //case "GLASSEXIST":
                            //    if (_stagePositionInfoReply.GlassExist) _lbl.Image = Properties.Resources.Bit_Green;
                            //    else _lbl.Image = Properties.Resources.Bit_Sliver;
                            //    break;
                            case "DOUBLEGLASSEXIST":
                                if (_stagePositionInfoReply.DoubleGlassExist) _lbl.Image = Properties.Resources.Bit_Green;
                                else _lbl.Image = Properties.Resources.Bit_Sliver;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_StagePositionInfoRequest()
        {
            try
            {
                string _err = string.Empty;

                #region Send StagePositionInfoRequest
                StagePositionInfoRequest _trx = new StagePositionInfoRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineName;
                _trx.BODY.EQUIPMENTNO = rbStage.NodeNo;
                _trx.BODY.ROBOTNAME = rbStage.RobotName;
                _trx.BODY.STAGEID = rbStage.StageID;

                string _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                #endregion

                rbStage.BC_StagePositionInfoReply.IsReply = false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void tmrBaseRefresh_Tick(object sender, System.EventArgs e)
        {
            try
            {
                #region Send Request

                if (rbStage.BC_StagePositionInfoReply != null)
                {
                    BCS_StagePositionInfoReply _stagePositionInfoReply = rbStage.BC_StagePositionInfoReply;

                    if (_stagePositionInfoReply.IsReply)
                    {
                        DateTime _now = DateTime.Now;
                        TimeSpan _ts = _now.Subtract(_stagePositionInfoReply.LastReceiveMsgDateTime).Duration();

                        if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                        {
                            Send_StagePositionInfoRequest();
                        }
                    }
                }

                #endregion

                ChangeStageInfo();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }



    }
}
