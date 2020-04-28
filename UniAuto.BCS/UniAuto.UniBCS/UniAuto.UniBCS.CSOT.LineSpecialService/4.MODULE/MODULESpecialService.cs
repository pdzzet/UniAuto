using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.MesSpec;
using System.Reflection;
using UniAuto.UniBCS.MISC;
using System;

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
    public class MODULESpecialService : AbstractService
    {
        private const string LineOutJobDataTimeout = "LineOutJobDataReportTimeout";
        private const string CstMappingDownloadTimeout = "CstControlCommandTimeout";
        private const string JobDataRequestReportReplyTimeOut = "JobDataRequestReportReplyTimeOut";

        public override bool Init()
        {
            return true;
        }

        public void ECNMode(Trx inputData)
        {
            try
            {
                #region[Get EQP]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));
                #endregion

                eEnableDisable eqpECNMode = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                if (eqp.File.EQPECNMode != eqpECNMode)
                {
                    lock (eqp)
                    {
                        eqp.File.EQPECNMode = eqpECNMode;
                    }
                }

                if (inputData.IsInitTrigger) return;

                #region[Log]
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [EQP -> EQP][{1}] ECN Mode =[{2}]", eqp.Data.NODENO, inputData.TrackKey, eqpECNMode.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void LineOutJobDataReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;


                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    LineOutJobDataReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                //Job Data Block
                #region [拆出PLCAgent Data]
                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobseqno = inputData.EventGroups[0].Events[0].Items[1].Value;
                string glsID = inputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value;

                #endregion
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, casseqno, jobseqno, glsID));

                LineOutJobDataReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                //Send MES ToDo...

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void LineOutJobDataReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_LineOutJobDataReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + LineOutJobDataTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + LineOutJobDataTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + LineOutJobDataTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(LineOutJobDataReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void LineOutJobDataReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] LINE OUT JOB DATA REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
                LineOutJobDataReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PanelandBacklightAssemblyRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;


                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    LineOutJobDataReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                //Job Data Block
                #region [拆出PLCAgent Data]
                string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                string recipeID = inputData.EventGroups[0].Events[0].Items[1].Value;
                string panelCassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                string panelJobSequenceNo = inputData.EventGroups[0].Events[0].Items[3].Value;
                string panelGlassID = inputData.EventGroups[0].Events[0].Items[4].Value;
                string materialID = inputData.EventGroups[0].Events[0].Items[5].Value;
                string consumeQty = inputData.EventGroups[0].Events[0].Items[6].Value;
                string planConsumeQty = inputData.EventGroups[0].Events[0].Items[7].Value;
                string operatorID = inputData.EventGroups[0].Events[0].Items[8].Value;

                #endregion
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] UNIT_NO=[{2}] RECIPEID=[{3}] PANEL_CASSETTE_SeqNo=[{4}] PANEL_JOB_SeqNo=[{5} PANEL_GLASSID=[{6} MATERIAL_ID=[{7} CONSUME_QTY=[{8} PLAN_CONSUME_QTY=[{9}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, recipeID, panelCassetteSequenceNo, panelJobSequenceNo, panelGlassID, materialID, consumeQty, planConsumeQty));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteMapDownload_MODULE(Equipment eqp, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region[Get  LINE]
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                if (port.File.CellCst != eBitResult.ON)
                {
                    foreach (Job job in slotData)
                    {
                        #region MODULE Special Job Data
                        outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.INSPReservations].Value = job.INSPReservations;   //BIN
                        outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.EQPReservations].Value = job.EQPReservations;   //BIN
                        outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.LastGlassFlag].Value = job.LastGlassFlag;   ////INT
                        outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.InspJudgedData].Value = job.InspJudgedData;   //BIN
                        outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.TrackingData].Value = job.TrackingData;   //BIN
                        outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.EQPFlag].Value = job.EQPFlag;   //BIN
                        outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.ChipCount].Value = job.ChipCount.ToString();   //INT

                        outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.ProductID].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.CassetteSettingCode].Value = job.CellSpecial.CassetteSettingCode;
                        outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                        switch (line.Data.JOBDATALINETYPE)
                        {
                            case eJobDataLineType.MODULE.MDABL: break;
                            case eJobDataLineType.MODULE.MDOCR: break;
                            case eJobDataLineType.MODULE.MDBLL: break;
                            case eJobDataLineType.MODULE.MDRWR: break;
                            case eJobDataLineType.MODULE.MDPAK: break;
                            default://Job Data No Use Line 
                                return;
                        }
                        #endregion
                    }
                }
                SendPLCData(outputData);
                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void CassetteMappingDownloadTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string[] obj = timer.State.ToString().Split('_');
                string[] sArray = tmp.Split('_');
                string trxName = string.Empty;

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                trxName = string.Format("{0}_Port#{1}CassetteControlCommand", sArray[0], sArray[1]);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].IsDisable = true;
                outputdata.EventGroups[1].Events[0].IsDisable = true;
                outputdata.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = obj[0];
                SendPLCData(outputdata);
                eCstControlCmd cmd = (eCstControlCmd)int.Parse(obj[1]);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] [Port={2}] EQP REPLY,  CASSETTE CONTROL COMMAND=[{3}] REPLY TIMEOUT SET BIT=[OFF].",
                    sArray[0], obj[0], sArray[1], cmd.ToString()));


                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { obj[0], ServerName, 
                        string.Format("Cassette{0}Reply - EQUIPMENT=[{1}] PORT=[{2}] \"T1 TIMEOUT\"", 
                        cmd, sArray[0], sArray[1])});
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void JobDataRequestReportReply_MODULE(Trx outputData, Job job, Line line, string eqpNo)
        {
            try
            {
                #region MODULE Special Job Data
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.INSPReservations].Value = job.INSPReservations;   //BIN
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.EQPReservations].Value = job.EQPReservations;   //BIN
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.LastGlassFlag].Value = job.LastGlassFlag;   ////INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.InspJudgedData].Value = job.InspJudgedData;   //BIN
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.TrackingData].Value = job.TrackingData;   //BIN
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag].Value = job.EQPFlag;   //BIN
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.ChipCount].Value = job.ChipCount.ToString();   //INT

                outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductID].Value = job.CellSpecial.ProductID;
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSettingCode].Value = job.CellSpecial.CassetteSettingCode;
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                switch (line.Data.JOBDATALINETYPE)
                {
                    case eJobDataLineType.MODULE.MDABL: break;
                    case eJobDataLineType.MODULE.MDOCR: break;
                    case eJobDataLineType.MODULE.MDBLL: break;
                    case eJobDataLineType.MODULE.MDRWR: break;
                    case eJobDataLineType.MODULE.MDPAK: break;
                    default:
                        return;
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}", eqpNo, JobDataRequestReportReplyTimeOut);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(JobDataRequestReportTimeoutForEQP), outputData.TrackKey);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, JOB DATA REQUEST REPORT SET BIT =[{2}], RETURN CODE=[{3}].",
                    eqpNo, outputData.TrackKey, eBitResult.ON, eReturnCode1.OK));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void JobDataRequestReportTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeoutName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], JobDataRequestReportReplyTimeOut);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                string trxName = string.Format("{0}_JobDataRequestReportReply", sArray[0]);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].IsDisable = true;
                outputdata.EventGroups[0].Events[2].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = sArray[1];
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPLY, JOB DATA REQUEST REPORT TIMEOUT SET BIT=[OFF].",
                    sArray[0], sArray[1]));

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
