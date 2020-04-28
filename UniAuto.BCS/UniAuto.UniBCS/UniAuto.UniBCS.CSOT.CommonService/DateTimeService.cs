/************************************************************
 * 1. 对时时增加PLC_HSMS模式的支持， On line 测试 TBNAN100是 BC报错  20150127 Tom
 * 
 * 
 * 
 * 
 * 
 * 
 * **********************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.OpiSpec;
using System.Threading;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public class DateTimeService : AbstractService
    {

        public const string DateTimeSetCommandTimeout = "DateTimeSetCommandTimeout";
        
        
        
        public override bool Init()
        {
            return false;
        }


        



        // 1. MES 對時, 針對所有PLC及HSMS對時
        public void MESSetAllEQDataTimeCommand(string trxid)
        {
            try
            {
                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs().Where(n=> n.File.CIMMode == eBitResult.ON).ToList<Equipment>();
                DateTimeSetCommand(eqps);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[BCS -> EQP][{0}] Synchronization Datetime .", trxid));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        // 2. OPI對時, 
        public void OPISetEQDateTimeCommand(DateTimeCalibrationRequest msg)
        {
            try
            {
                string errEQP = string.Empty;
                IList<Equipment> eqps = new List<Equipment>();
                foreach (DateTimeCalibrationRequest.EQUIPMENTc e in msg.BODY.EQUIPMENTLIST)
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(e.EQUIPMENTNO);
                    if (eqp == null)
                    {
                        errEQP = string.IsNullOrEmpty(errEQP) ? errEQP : errEQP += "," + e.EQUIPMENTNO;
                    }
                    else
                    {
                        if (eqp.File.CIMMode == eBitResult.ON)
                        {
                            eqps.Add(eqp);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(errEQP))
                {
                    errEQP = string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP2, MethodBase.GetCurrentMethod().Name, errEQP);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(),
                        msg.BODY.LINENAME, errEQP});
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", errEQP);
                    return;
                }
                DateTimeSetCommand(eqps);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void DateTimeSetCommand(IList<Equipment> eqps) 
        {
            try
            {
                string _dateTime = DateTime.Now.ToString("yyMMddHHmmss");
                string trackKey = UtilityMethod.GetAgentTrackKey();
                #region [BCS需先寫入到Word的DateTime]
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat("L1_DateTimeSetCommand") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = _dateTime;
    
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                //写完Word 后休息一下，让PLC Agent 有时间写入完成，然后开始写bit 20150520 tom
                Thread.Sleep(ParameterManager["EVENTDELAYTIME"].GetInteger());

                foreach (Equipment eqp in eqps)
                {
                    eReportMode reportMode;
                    Enum.TryParse<eReportMode>(eqp.Data.REPORTMODE, out reportMode);
                    switch (reportMode)
                    {
                        case eReportMode.PLC://只有PLC
                        case eReportMode.PLC_HSMS:// PLC 为主 SECS 为辅
                            {
                                outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqp.Data.NODENO + "_DateTimeSetCommand") as Trx;
                                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.ON).ToString();
                                //outputdata.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                                outputdata.TrackKey = trackKey;
                                SendPLCData(outputdata);

                                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, DateTimeSetCommandTimeout);
                                if (_timerManager.IsAliveTimer(timeName))
                                {
                                    _timerManager.TerminateTimer(timeName);
                                }
                                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), 
                                    new System.Timers.ElapsedEventHandler(DateTimeCommandReplyTimeout), outputdata.TrackKey);

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] SET_DATE_TIME=[{3}], SET BIT=[ON].",
                                    eqp.Data.NODENO, outputdata.TrackKey, eqp.File.CIMMode.ToString(), _dateTime));
                            }
                            break;
                        case eReportMode.HSMS_PLC: //SECS 为主 PLC为辅
                        case eReportMode.HSMS_CSOT: // CSOT Spec 为主
                            {
                                Invoke(eServiceName.CSOTSECSService, "TS2F31_H_DateandTimeSetRequest", new string[] { eqp.Data.NODENO, eqp.Data.NODEID, string.Empty, trackKey });
                            }
                            break;
                        case eReportMode.HSMS_NIKON: //NIKON Spec 为主
                            {
                                Invoke(eServiceName.NikonSECSService, "TS2F31_H_DateandTimeSetRequest", new string[] { eqp.Data.NODENO, eqp.Data.NODEID, string.Empty, trackKey });
                            }
                            break;
                        default: // DB 设定错误了
                            throw new Exception(string.Format("EQUIPMENT=[{0}] REPORT_MODE=[{1}] IS INVALID.", eqp.Data.NODENO, eqp.Data.REPORTMODE));
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// EQP Reply
        /// </summary>
        /// <param name="inputdata"></param>
        public void DateTimeSetCommandReply(Trx inputdata)
        {
            try
            {
                if (inputdata.IsInitTrigger) return;
                string eqpNo = inputdata.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputdata.EventGroups[0].Events[0].Items[0].Value);
                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [EQP -> BCS][{1}] BIT=[OFF].", eqpNo, inputdata.TrackKey));
                    return;
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [EQP -> BCS][{1}] BIT=[ON].", eqpNo, inputdata.TrackKey));

                string timeName = string.Format("{0}_{1}", inputdata.Metadata.NodeNo, DateTimeSetCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(inputdata.Metadata.NodeNo + "_DateTimeSetCommand") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputdata.TrackKey;
                SendPLCData(outputdata);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "DateTimeSetCommand()", 
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].",
                    inputdata.Metadata.NodeNo, inputdata.TrackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        //============TimeOut
        private void DateTimeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], DateTimeSetCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] DATE TIME COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_DateTimeSetCommand") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }
    }
}
