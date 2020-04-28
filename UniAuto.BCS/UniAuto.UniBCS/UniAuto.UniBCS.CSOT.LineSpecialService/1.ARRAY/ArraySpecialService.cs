using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.OpiSpec;
using System.Timers;
using UniAuto.UniBCS.Core;

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
    public class ArraySpecialService : AbstractService
    {
        Thread _checkEqpProcessTime;
        bool _isRuning;
        bool threadStart;

        private const string SetRecipeGroupEndCommandTimeout = "SetRecipeGroupEndCommandTimeout";
        private const string HoldEventReportReplyTimeout = "JobHoldEventReportReplyTimeout";
        private const string ForceCleanOutTimeout = "ForceCleanOutTimeout";
        private const string AbnormalForceCleanOutTimeout = "AbnormalForceCleanOutTimeout";
        private const string RbOperationModeTimeout = "RbOperationModeTimeout";
        private const string RbOperationModeCommandTimeout = "RbOperationModeCommandTimeout";
        private const string EqpProportionRuleReportTimeout = "EquipmentFetchGlassProportionRuleReportReplyTimeout";
        private const string CstMappingDownloadTimeout = "CstControlCommandTimeout";
        private const string EquipmentFetchGlassProportionalRuleCommandTimeout = "EquipmentFetchGlassProportionalRuleCommandTimeout";
        private const string GlassGradeMappingChangeTimeout = "GlassGradeMappingChangeTimeout";
        private const string GlassGradeMappingCommandTimeout = "GlassGradeMappingCommandTimeout";
        private const string JobDataRequestReportReplyTimeOut = "JobDataRequestReportReplyTimeOut";

        private const string LineBackupModeChangeReportReplyTO = "LineBackupModeChangeReportReplyTimeout"; // add by bruce 2015/7/23
        private const string JobLineOutCheckReportReplyTO = "JobLineOutCheckReportReplyTimeOut";   //add by bruce 2015/7/23
        private const string JobLineOutReportReplyTO = "JobLineOutReportReplyTimeOut";   //add by bruce 2015/7/23
        private const string JobLineInReportReplyTO = "JobLineInReportReplyTimeOut";    // add by bruce 2015/7/23
        private const string EquipmentChamberModeReportReplyTimeOut = "_EquipmentChamberModeChangeReportReplyTimeOut";
      

        public override bool Init()
        {
            _isRuning = true;
            if (!threadStart && (_checkEqpProcessTime == null))
            {
                _checkEqpProcessTime = new Thread(new ThreadStart(CheckEqpProcessTime));
                _checkEqpProcessTime.IsBackground = true;
                _checkEqpProcessTime.Start();
                threadStart = true;
            }

            return true;
        }

        #region [Set Recipe Group]
        public void SetRecipeGroupEndCommand(Job job, string trackKey)
        {
            try
            {
                Trx outputData = null;
                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                foreach (Equipment eqp in eqps)
                {
                    eReportMode reportMode;
                    Enum.TryParse<eReportMode>(eqp.Data.REPORTMODE, out reportMode);
                    switch (reportMode)
                    {
                        case eReportMode.PLC:
                        case eReportMode.PLC_HSMS:
                            {
                                outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqp.Data.NODENO + "_SetRecipeGroupEndCommand") as Trx;
                                if (outputData != null)
                                {
                                    outputData.EventGroups[0].Events[0].Items[0].Value = job.CassetteSequenceNo;
                                    outputData.EventGroups[0].Events[0].Items[1].Value = job.JobSequenceNo;
                                    outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                                    //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                                    outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                                    outputData.TrackKey = trackKey;
                                    SendPLCData(outputData);

                                    string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, SetRecipeGroupEndCommandTimeout);

                                    if (_timerManager.IsAliveTimer(timeName))
                                    {
                                        _timerManager.TerminateTimer(timeName);
                                    }
                                    _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                                        new System.Timers.ElapsedEventHandler(SetRecipeGroupEndCommandReplyTimeout), outputData.TrackKey);

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] GlassChipMaskBlockID=[{4}], SET BIT=[ON].",
                                        eqp.Data.NODENO, trackKey, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID));
                                }
                                else
                                {
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                       string.Format("[EQUIPMENT={0}] [BCS -> EQP] CAN NOT FOUND TRX=[{0}{1}]", eqp.Data.NODENO, "_SetRecipeGroupEndCommand"));
                                }

                             }
                            break;
                        case eReportMode.HSMS_PLC:
                        case eReportMode.HSMS_CSOT:
                            {
                                //要由奇穎補這段
                                //20141015 cy補上
                                //(string eqpno, string eqpid, string cstseq, string slot, string glassid,string endflag, string tag)
                                Invoke(eServiceName.CSOTSECSService, "TS2F123_H_SetGlassRecipeGroupEndFlagSend", 
                                    new object[] { eqp.Data.NODENO, eqp.Data.NODEID, 
                                                   job.CassetteSequenceNo, job.JobSequenceNo,
                                                   job.GlassChipMaskBlockID, "0",
                                                   string.Empty, trackKey});
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SetRecipeGroupEndCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                    eqpNo, inputData.TrackKey, triggerBit.ToString(), (int)retCode, retCode.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, SetRecipeGroupEndCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));
                string err = string.Empty;
                
                switch (retCode)
                {
                    case eReturnCode1.OK: break;
                    case eReturnCode1.NG:
                        {
                            err = string.Format("[{0}] EQUIPMENT=[{1}] RETURN_CODE=[2](NG).", MethodBase.GetCurrentMethod().Name, eqpNo);
                            Logger.LogWarnWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, err });
                        }
                        break;
                    default:
                        {
                            err = string.Format("[{0}] EQUIPMENT=[{1}] RETURN_CODE=[{2}](UNKNOWN) IS INVALID.",
                                MethodBase.GetCurrentMethod().Name, eqpNo, inputData.EventGroups[0].Events[0].Items[0].Value);
                            Logger.LogWarnWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, err });
                        }
                        break;
                }
                #region [Command Off]
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(inputData.Metadata.NodeNo + "_SetRecipeGroupEndCommand") as Trx;
                outputData.EventGroups[0].Events[0].IsDisable = true;
                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputData.TrackKey = inputData.TrackKey;
                SendPLCData(outputData);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].", eqpNo, inputData.TrackKey));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        } 

        private void SetRecipeGroupEndCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], SetRecipeGroupEndCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET RECIPE GROUP END COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_SetRecipeGroupEndCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Job Hold]
        public void JobHoldEventReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                string unitNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    JobHoldEventReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] UNIT_NO=[{5}] HOLD_REASON_CODE=[{6}] OPERATOR_ID=[{7}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode,
                    inputData.EventGroups[0].Events[0].Items[0].Value,
                    inputData.EventGroups[0].Events[0].Items[1].Value,
                    inputData.EventGroups[0].Events[0].Items[2].Value,
                    inputData.EventGroups[0].Events[0].Items[3].Value,
                    inputData.EventGroups[0].Events[0].Items[4].Value));

                Job job = ObjectManager.JobManager.GetJob(
                    inputData.EventGroups[0].Events[0].Items[0].Value,
                    inputData.EventGroups[0].Events[0].Items[1].Value);

                if (job == null)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("CAN'T FIND JOB INFORMATION IN BCS, CST_SEQNO=[{0}] JOB_SEQNO=[{1}].",
                        inputData.EventGroups[0].Events[0].Items[0].Value, inputData.EventGroups[0].Events[0].Items[1].Value));
                }
                else
                {
                    #region 記錄在 Job Data
                    Unit u = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);

                    HoldInfo hold = new HoldInfo()
                    {
                        NodeNo = eqp.Data.NODENO,
                        NodeID = eqp.Data.NODEID,
                        UnitNo = unitNo,
                        UnitID = u == null ? "" : u.Data.UNITID,
                        HoldReason = inputData.EventGroups[0].Events[0].Items[3].Value,
                        OperatorID = inputData.EventGroups[0].Events[0].Items[4].Value,
                    };
                    ObjectManager.JobManager.HoldEventRecord(job, hold);
                    //Job job,string nodeId,string nodeNo, string unitNo,string portNo,string slotNo, eJobEvent eventname
                    //Invoke("JobService", "RecordJobHistory", new object[] { job, eqp.Data.NODEID, eqpNo, unitNo, "0", "0", eJobEvent.Hold ,""}); //T2 code error modify by bruce 2015/7/14
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, unitNo, "0", "0", eJobEvent.Hold.ToString(), inputData.TrackKey);
                    #endregion
                }

                JobHoldEventReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    JobHoldEventReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        public void JobHoldEventReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_JobHoldEventReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, HoldEventReportReplyTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(JobHoldEventReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[ON].",
                    eqpNo, trackKey, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void JobHoldEventReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                JobHoldEventReportReply(sArray[0], eBitResult.OFF, trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] JOB HOLD EVENT REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Force Clean out]
        public void ForceCleanOutCommand(eBitResult value, string trxID)
        {
            try
            {
                Trx outputData = null;
                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                foreach (Equipment eqp in eqps)
                {  //wucc modify 20150813  修改ForceCleanOutCommand邏輯
                    //wucc modify 20150813 在CANON,CANON不做
                    //如果 ReportMode 是HSMS_PLC或是HSMS_CSOT 則作SECS ForcedCleanOutCommandSend 不然則作PLC

                    string strNODEATTRIBUTE = eqp.Data.NODEATTRIBUTE;
                    if (strNODEATTRIBUTE.Equals("NIKON") || strNODEATTRIBUTE.Equals("CANON") )
                        continue;

                    string strReportMode = eqp.Data.REPORTMODE.ToUpper().Trim();
                    if (strReportMode.Equals("HSMS_PLC") || strReportMode.Equals("HSMS_CSOT"))
                    {
                        Invoke(eServiceName.CSOTSECSService, "TS2F111_H_ForcedCleanOutCommandSend",
                               new object[] {
                            eqp.Data.NODENO, //eqpno
                            eqp.Data.NODEID, //eqpid
                            eqp.Data.NODEID, //eqptid
                            //cleanout //0: Forced Clean Out Reset.,1: Normal Forced Clean Out Set.,2: Abnormal Forced Clean Out Set.
                            value == eBitResult.OFF ? "0" : "1", 
                            string.Empty, //tag
                            trxID //trxid  wucc  modify 20150813
                            });
                    }
                    else
                    {
                        string tranName = eqp.Data.NODENO + "_ForceCleanOutCommand";
                        outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(tranName) as Trx;
                        if (outputData != null)
                        {
                            outputData.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                            outputData.TrackKey = trxID;
                            SendPLCData(outputData);

                            if (value == eBitResult.ON)
                            {
                                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, ForceCleanOutTimeout);

                                if (_timerManager.IsAliveTimer(timeName))
                                {
                                    _timerManager.TerminateTimer(timeName);
                                }

                                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                                    new System.Timers.ElapsedEventHandler(ForceCleanOutCommandTimeout), outputData.TrackKey);
                            }
                        }
                    }
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                        eqp.Data.NODENO, trxID, value.ToString()));

                    #region [Old]
                    //switch (eqp.Data.NODEATTRIBUTE.ToUpper().Trim())
                    //{
                    //case "NIKON":
                    //case "CANON": break;
                    //default:
                    //    {
                    //        string tranName = eqp.Data.NODENO + "_ForceCleanOutCommand";
                    //        outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(tranName) as Trx;
                    //        //wucc modify 20150807 如果PLC沒有設定不直接continue,要判斷能不能執行SECS
                    //        if (outputData != null)
                    //        {
                    //            //  continue; 
                    //            outputData.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    //            outputData.TrackKey = trxID;
                    //            SendPLCData(outputData);

                    //            if (value == eBitResult.ON)
                    //            {
                    //                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, ForceCleanOutTimeout);

                    //                if (_timerManager.IsAliveTimer(timeName))
                    //                {
                    //                    _timerManager.TerminateTimer(timeName);
                    //                }

                    //                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    //                    new System.Timers.ElapsedEventHandler(ForceCleanOutCommandTimeout), outputData.TrackKey);
                    //            }
                    //        }
                    //        else if (eqp.Data.NODEATTRIBUTE.ToUpper().Trim().Equals("DNS") || eqp.Data.NODEATTRIBUTE.ToUpper().Trim().Equals("DRY") || eqp.Data.NODEATTRIBUTE.ToUpper().Trim().Equals("ATK"))
                    //        {
                    //            Invoke(eServiceName.CSOTSECSService, "TS2F111_H_ForcedCleanOutCommandSend",
                    //                new object[] {
                    //            eqp.Data.NODENO, //eqpno
                    //            eqp.Data.NODEID, //eqpid
                    //            eqp.Data.NODEID, //eqptid
                    //            //cleanout //0: Forced Clean Out Reset.,1: Normal Forced Clean Out Set.,2: Abnormal Forced Clean Out Set.
                    //            value == eBitResult.OFF ? "0" : "1", 
                    //            string.Empty, //tag
                    //            trxID //trxid  wucc  modify 20150813
                    //            });
                    //        }
                    //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                    //            eqp.Data.NODENO, trxID, value.ToString()));
                    //    }
                    //    break;
                    //}
                    #endregion
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ForceCleanOutCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] SET BIT=[{2}].",
                    eqpNo, inputData.TrackKey, triggerBit.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, ForceCleanOutTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ForceCleanOutCommandTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], ForceCleanOutTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] FORCE CLEAN OUT COMMAND REPLY TIMEOUT.",
                    sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        public void AbnormalForceCleanOutCommand(eBitResult value, string trxID)
        {
            try
            {
                Trx outputData = null;
                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                foreach (Equipment eqp in eqps)
                {
                    //wucc modify 20150813  修改ForceCleanOutCommand邏輯
                    //wucc modify 20150813 在CANON,CANON不做
                    //如果 ReportMode 是HSMS_PLC或是HSMS_CSOT 則作SECS ForcedCleanOutCommandSend 不然則作PLC
                    string strNODEATTRIBUTE = eqp.Data.NODEATTRIBUTE;
                    if (strNODEATTRIBUTE.Equals("NIKON") || strNODEATTRIBUTE.Equals("CANON") )
                        continue;

                    string strReportMode = eqp.Data.REPORTMODE.ToUpper().Trim();
                    if (strReportMode.Equals("HSMS_PLC") || strReportMode.Equals("HSMS_CSOT"))
                    {
                        Invoke(eServiceName.CSOTSECSService, "TS2F111_H_ForcedCleanOutCommandSend",
                               new object[] {
                            eqp.Data.NODENO, //eqpno
                            eqp.Data.NODEID, //eqpid
                            eqp.Data.NODEID, //eqptid
                            //cleanout //0: Forced Clean Out Reset.,1: Normal Forced Clean Out Set.,2: Abnormal Forced Clean Out Set.
                            value == eBitResult.OFF ? "0" : "2",   
                            string.Empty, //tag
                            trxID //trxid  wucc  modify 20150813
                            });
                    }
                    else
                    {
                        string tranName = eqp.Data.NODENO + "_AbnormalForceCleanOutCommand";
                        outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(tranName) as Trx;
                        if (outputData != null)
                        {
                            outputData.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                            outputData.TrackKey = trxID;
                            SendPLCData(outputData);

                            if (value == eBitResult.ON)
                            {
                                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, ForceCleanOutTimeout);

                                if (_timerManager.IsAliveTimer(timeName))
                                {
                                    _timerManager.TerminateTimer(timeName);
                                }

                                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                                    new System.Timers.ElapsedEventHandler(ForceCleanOutCommandTimeout), outputData.TrackKey);
                            }
                        }
                    }
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                        eqp.Data.NODENO, trxID, value.ToString()));
                    #region [Old]
                    //    switch (eqp.Data.NODEATTRIBUTE.ToUpper().Trim())
                    //    {
                    //        case "NIKON":
                    //        case "CANON": break;
                    //        default:
                    //            {
                    //                string tranName = eqp.Data.NODENO + "_AbnormalForceCleanOutCommand" ;

                    //                outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(tranName) as Trx;
                    //                if (outputData == null)
                    //                {
                    //                    continue;
                    //                }
                    //                outputData.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    //                outputData.TrackKey = trxID;
                    //                SendPLCData(outputData);

                    //                if (value == eBitResult.ON)
                    //                {
                    //                    string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, AbnormalForceCleanOutTimeout);

                    //                    if (_timerManager.IsAliveTimer(timeName))
                    //                    {
                    //                        _timerManager.TerminateTimer(timeName);
                    //                    }

                    //                    _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    //                        new System.Timers.ElapsedEventHandler(AbnormalForceCleanOutCommandTimeout), outputData.TrackKey);
                    //                }

                    //                if (eqp.Data.NODEATTRIBUTE.ToUpper().Trim().Equals("DNS"))
                    //                {
                    //                    Invoke(eServiceName.CSOTSECSService, "TS2F111_H_ForcedCleanOutCommandSend",
                    //                        new object[] {
                    //                    eqp.Data.NODENO, //eqpno
                    //                    eqp.Data.NODEID, //eqpid
                    //                    eqp.Data.NODEID, //eqptid
                    //                    //cleanout //0: Forced Clean Out Reset.,1: Normal Forced Clean Out Set.,2: Abnormal Forced Clean Out Set.
                    //                    value == eBitResult.OFF ? "0" : "2", 
                    //                    string.Empty, //tag
                    //                    outputData.TrackKey //trxid
                    //                    });
                    //                }
                    //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                    //                    eqp.Data.NODENO, outputData.TrackKey, value.ToString()));
                    //            }
                    //            break;
                    //    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void AbnormalForceCleanOutCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] SET BIT=[{2}].",
                    eqpNo, inputData.TrackKey, triggerBit.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, AbnormalForceCleanOutTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void AbnormalForceCleanOutCommandTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], AbnormalForceCleanOutTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ABNORMAL FORCE CLEAN OUT COMMAND REPLY TIMEOUT.",
                    sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region **[Robot Operation Mode]
        public void RobotOperationMode(Trx inputData)
        {
            try
            {
                string log = "ROBOT_OPERATION_MODE:";
                for (int i = 0; i < inputData.EventGroups[0].Events[0].Items.Count; i++)
                {
                    string positionNo = inputData.EventGroups[0].Events[0].Items[i].Name.Split('#')[1];

                    RobotPosition rp = ObjectManager.RobotPositionManager.GetRobotPosition(positionNo);
                    if (rp != null)
                    {
                        eRobotOperationMode mode = (eRobotOperationMode)int.Parse(inputData.EventGroups[0].Events[0].Items[i].Value);
                        lock (rp) rp.Mode = mode;
                        log += string.Format(" {0}=[{1}]({2})", rp.Data.DESCRIPTION, (int)rp.Mode, rp.Mode.ToString());
                    }
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [EQ_TO_EQ][{1}] {2}.", inputData.Metadata.NodeNo, inputData.TrackKey, log));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }
        #endregion

        #region **[Robot Operation Mode Change]
        public void RobotOperationModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    RobotOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [取得EQP資訊]
                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                string positionNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                eRobotOperationMode mode = (eRobotOperationMode)int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value);
                eRobotOperationAction action = (eRobotOperationAction)int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value);

                RobotPosition rp = ObjectManager.RobotPositionManager.GetRobotPosition(positionNo);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] ROBOT_OPERATION_NO=[{2}] Mode=[{3}]({4}) Action=[{5}]",
                    eqp.Data.NODENO, inputData.TrackKey, positionNo, (int)mode,mode.ToString(), action));

                RobotOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    RobotOperationModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        public void RobotOperationModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_RobotOperationModeChangeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, "RobotOperationModeChangeReportReplyTimeout");
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(RobotOperationModeChangeReportReplyTimeout), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].", eqpNo, trackKey, value));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RobotOperationModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ROBOT OPERATION MODE CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                RobotOperationModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region **[Robot Operation Mode Command]
        public void RobotOperationModeCommand(string trxid, string currentEqpNo, string positionNO, eRobotOperationMode mode, eRobotOperationAction action)
        {
            try
            {
                Trx trx = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format("{0}_RobotOperationModeCommand", currentEqpNo)) as Trx;
                if (trx != null)
                {
                    RobotPosition rp = ObjectManager.RobotPositionManager.GetRobotPosition(positionNO);

                    trx.EventGroups[0].Events[0].Items[0].Value = positionNO;
                    trx.EventGroups[0].Events[0].Items[1].Value = ((int)mode).ToString();
                    trx.EventGroups[0].Events[0].Items[2].Value = rp.Data.DIRECTION;// ((int)action).ToString();
                    trx.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    trx.EventGroups[0].Events[1].Items[0].Value = "1";
                    trx.TrackKey = trxid;
                    SendPLCData(trx);
                    string timerId = string.Format("{0}_RobotOperationModeCommandTimeOut", currentEqpNo);
                    if (Timermanager.IsAliveTimer(timerId))
                    {
                        Timermanager.TerminateTimer(timerId);
                    }
                    Timermanager.CreateTimer(timerId, false, ParameterManager["T1"].GetInteger(), new ElapsedEventHandler(RobotOperationModeCommandTimeOut), trxid);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ROBOT_POSITION_NO=[{2}] MODE=[{3}] ACTION=[{4}], SET BIT=[ON].",
                        currentEqpNo, trx.TrackKey, positionNO, mode, rp.Data.DIRECTION));
                }
                else
                {
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("CAN NOT FOUND TRX=[{0}_RobotOperationModeCommand].", currentEqpNo));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RobotOperationModeCommandTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string timeName = string.Format("{0}_RobotOperationModeCommandTimeOut", sArray[0]);
                //终止Timer
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_RobotOperationModeCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] SET BIT=[OFF].", sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void RobotOperationModeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                if (triggerBit == eBitResult.OFF) return;
                //终止Timer
                string timeName = string.Format("{0}_RobotOperationModeCommandTimeOut", eqpNo);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_RobotOperationModeCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;

                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, GetType().Name, "RobotOperationModeCommand()",
                                   string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] SET BIT=[OFF].",
                                   eqpNo, inputData.TrackKey, returnCode));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Equipment Fetch Glass Proportional Rule]
        #region **[Equipment Fetch Glass Proportional Rule Change]
        public void EqpFetchGlassProportionalRuleReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eProportionalRuleName group1 = (eProportionalRuleName)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                eProportionalRuleName group2 = (eProportionalRuleName)int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value);
                #endregion

                Repository.Add(inputData.Name, inputData);                

                string eqpNo = inputData.Metadata.NodeNo;
                #region [取得EQP資訊]
                Equipment eqp;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                if (inputData.IsInitTrigger || bitResult == eBitResult.ON)
                {
                    eqp.File.ProportionalRule01Type = int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                    eqp.File.ProportionalRule01Value = int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value);
                    eqp.File.ProportionalRule02Type = int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value);
                    eqp.File.ProportionalRule02Value = int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value);                    
                    
                    if (inputData.IsInitTrigger)
                        return;

                    //t3 MSP/ITO use for report chamber mode. 2015/09/29 cc.kuang
                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    if (line != null && (line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC))
                    {
                        Equipment pvd = ObjectManager.EquipmentManager.GetEQP("L4");
                        if (line.File.HostMode != eHostMode.OFFLINE && pvd.File.EquipmentRunMode == "Mix")
                        {
                            List<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(pvd.Data.NODENO).Where(u => u.Data.UNITTYPE == "CHAMBER").ToList();
                            if (units != null && units.Count > 1)
                            {
                                string description = string.Empty;
                                string runmode = string.Empty;
                                List<MesSpec.ChamberRunModeChanged.CHAMBERc> chamberList = new List<MesSpec.ChamberRunModeChanged.CHAMBERc>();
                                runmode = GetChamberMode(line, pvd.Data.NODENO, inputData.EventGroups[0].Events[0].Items[0].Value, out description);
                                MesSpec.ChamberRunModeChanged.CHAMBERc cb1 = new MesSpec.ChamberRunModeChanged.CHAMBERc();
                                cb1.CHAMBERNAME = units[0].Data.UNITID;
                                cb1.CHAMBERRUNMODE = description;
                                runmode = GetChamberMode(line, pvd.Data.NODENO, inputData.EventGroups[0].Events[0].Items[2].Value, out description);
                                MesSpec.ChamberRunModeChanged.CHAMBERc cb2 = new MesSpec.ChamberRunModeChanged.CHAMBERc();
                                cb2.CHAMBERNAME = units[1].Data.UNITID;
                                cb2.CHAMBERRUNMODE = description;
                                chamberList.Add(cb1);
                                chamberList.Add(cb2);
                                Invoke(eServiceName.MESService, "ChamberRunModeChanged",
                                    new object[] { inputData.TrackKey, pvd.Data.LINEID, line.File.LineOperMode, pvd.Data.NODEID, chamberList });
                            }
                        }
                    }
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    EqpFetchGlassProportionalRuleReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] 1'st PROPORTIONAL RULE: NAME=[{3}] VALUE=[{4}]; 2nd PROPORTIONAL RULE: NAME=[{5}] VALUE=[{6}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode,
                    group1.ToString(),
                    inputData.EventGroups[0].Events[0].Items[1].Value,
                    group2.ToString(),
                    inputData.EventGroups[0].Events[0].Items[3].Value));

                EqpFetchGlassProportionalRuleReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    EqpFetchGlassProportionalRuleReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        public void EqpFetchGlassProportionalRuleReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_EquipmentFetchGlassProportionalRuleReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, EqpProportionRuleReportTimeout);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(EqpFetchGlassProportionalRuleReportReplyTimeout), trackKey);
                    }
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                        eqpNo, trackKey, value));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void EqpFetchGlassProportionalRuleReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                EqpFetchGlassProportionalRuleReportReply(sArray[0], eBitResult.OFF, trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENT FETCH GLASS PROPORTIONAL RULE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region **[Equipment Fetch Glass Proportional Rule Command]
        public void EquipmentFetchGlassProportionalRuleCommand(string trxid, string eqpNo, int firstPropRuleName, int firstPropRuleValue, int secondPropRuleName, int secondPropRuleValue)
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });
                    throw new Exception(err);
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND EQUIPMENT FETCH GLASS PROPORTIONAL RULE COMMAND!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxid, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;

                }
                string trxName = string.Format("{0}_EquipmentFetchGlassProportionalRuleCommand", eqp.Data.NODENO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                if (outputdata == null)// Check Trx 
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",string.Format("Can not found Trx {0}",trxName));
                    return;

                }
                outputdata.EventGroups[0].Events[0].Items[0].Value = firstPropRuleName.ToString();
                outputdata.EventGroups[0].Events[0].Items[1].Value = firstPropRuleValue.ToString();
                outputdata.EventGroups[0].Events[0].Items[2].Value = secondPropRuleName.ToString();
                outputdata.EventGroups[0].Events[0].Items[3].Value = secondPropRuleValue.ToString();
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);

                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, EquipmentFetchGlassProportionalRuleCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(EquipmentFetchGlassProportionalRuleCommandReplyReplyTimeout), outputdata.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] 1'st PROPORTIONAL RULE: NAME=[{2}] VALUE=[{3}]; 2'nd PROPORTIONAL RULE: NAME=[{4}] VALUE=[{5}], SET BIT=[ON].", eqp.Data.NODENO,
                        outputdata.TrackKey, firstPropRuleName, firstPropRuleValue, secondPropRuleName, secondPropRuleValue));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void EquipmentFetchGlassProportionalRuleCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), (int)retCode, retCode.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, EquipmentFetchGlassProportionalRuleCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;

                string trxName = string.Format("{0}_EquipmentFetchGlassProportionalRuleCommand", inputData.Metadata.NodeNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Repository.Add(inputData.Name, inputData);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "EquipmentFetchGlassProportionalRuleCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void EquipmentFetchGlassProportionalRuleCommandReplyReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], EquipmentFetchGlassProportionalRuleCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENT FETCH GLASS PROPORTIONAL RULE COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_EquipmentFetchGlassProportionalRuleCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #endregion

        #region [Glass Grade Mapping]
        #region **[Glass Grade Mapping Change Report]
        public void GlassGradeMappingChangeReport(Trx inputData)
        {
            try
            {
                Repository.Add(inputData.Name, inputData);

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                string eqpNo = inputData.Metadata.NodeNo;
                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    GlassGradeMappingChangeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [取得EQP資訊]
                Equipment eqp;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] OK_MODE=[{3}] NG_MODE=[{4}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode,
                    inputData.EventGroups[0].Events[0].Items[0].Value,
                    inputData.EventGroups[0].Events[0].Items[1].Value));

                GlassGradeMappingChangeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    GlassGradeMappingChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void GlassGradeMappingChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_GlassGradeMappingChangeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, GlassGradeMappingChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(GlassGradeMappingChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].", eqpNo, trackKey, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void GlassGradeMappingChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                GlassGradeMappingChangeReportReply(sArray[0], eBitResult.OFF, trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] GLASS GRADE MAPPLING CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region **[Glass Grade Mapping Command]
        public void GlassGradeMappingCommand(string trackKey, string eqpNo, string okMode, string ngMode)
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, eqpNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    throw new Exception(err);
                }

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND GLASS GRADE MAPPING COMMAND!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                string trxName = string.Format("{0}_GlassGradeMappingCommand", eqp.Data.NODENO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = okMode;
                    outputdata.EventGroups[0].Events[0].Items[1].Value = ngMode;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                    //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                    outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, GlassGradeMappingCommandTimeout);

                    if (_timerManager.IsAliveTimer(timeName))
                    {
                        _timerManager.TerminateTimer(timeName);
                    }
                    _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), 
                        new System.Timers.ElapsedEventHandler(GlassGradeMappingCommandReplyTimeout), outputdata.TrackKey);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] OK_MODE=[{2}] NG_MODE=[{3}], SET BIT=[ON].", eqp.Data.NODENO,
                            outputdata.TrackKey, okMode, ngMode));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(" Can not found Trx {0}.",trxName));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void GlassGradeMappingCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), (int)retCode, retCode.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, GlassGradeMappingCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                if (triggerBit == eBitResult.OFF) return;

                string trxName = string.Format("{0}_GlassGradeMappingCommand", inputData.Metadata.NodeNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Repository.Add(inputData.Name, inputData);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "GlassGradeMappingCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void GlassGradeMappingCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], GlassGradeMappingCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] GLASS GRADE MAPPING COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_GlassGradeMappingCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion

        #region MaterialStatusChangeReport
        // 移到MaterialService
        //public void MaterialStatusChangeReport(Trx inputData)
        //{
        //    try
        //    {
        //        string eqpNo = inputData.Metadata.NodeNo;
        //        #region [拆出PLCAgent Data]
        //        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
        //        #endregion

        //        #region [取得EQP資訊]
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
        //        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
        //        #endregion

        //        #region [取得LINE資訊]
        //        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
        //        if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
        //        #endregion

        //        lock (line) line.File.Array_Material_Change = (bitResult == eBitResult.ON ? true : false);

        //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] MATERIAL_CHANGE_BIT=[{3}].",
        //            eqp.Data.NODENO, inputData.TrackKey, bitResult.ToString()));
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        #endregion

        #region JobGradeUpdateReport
        public void JobGradeUpdateReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [EQ_TO_EQ][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }

                #region [取得EQP資訊]
                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                string cstSeqNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSeqNo = inputData.EventGroups[0].Events[0].Items[1].Value;

                string grade = inputData.EventGroups[0].Events[0].Items[2].Value;

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [EQ_TO_EQ][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO[{3}] GLASS_GRADE=[{4}]",
                    eqp.Data.NODENO, inputData.TrackKey, cstSeqNo, jobSeqNo, grade));

                Job job = ObjectManager.JobManager.GetJob(cstSeqNo, jobSeqNo);

                if (job == null)
                {
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("CAN NOT FIND JOB INFORMATION! CST_SEQNO=[{0}] JOB_SEQNO=[{1}].", cstSeqNo, jobSeqNo));
                }
                else
                {
                    if (job.JobGrade.Trim() != grade.Trim())
                    {
                        lock (job) job.JobGrade = grade;
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("BCS UPDATE JOB_GRADE=[{0}], CST_SEQNO=[{1}] JOB_SEQNO=[{2}] GLASSID=[{3}].", 
                            grade, cstSeqNo, jobSeqNo, job.GlassChipMaskBlockID));
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void JobGradeUpdateReportReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [EQ_TO_EQ][{1}] BIT=[{2}].", inputData.Metadata.NodeNo,
                    inputData.TrackKey, bitResult.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region FLCModeInUseLocal --- TBILC Line Use
        public void FLCModeInUseLocal(string eqpNo, bool flcMode, string trackKey)
        {
            try
            {
                string trxName = string.Format("L2_FLCModeInUseLocal#{0}", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                
                if (outputdata != null)
                {
                    eBitResult result = flcMode ? eBitResult.ON : eBitResult.OFF;
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)result).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[BC_TO_EQ][{0}] LOCAL#{1} RUN MODE IS {2} MODE.", trackKey, eqpNo,
                        flcMode ? "FLC" : "ILC"));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region******************* Cassette Map Download Array Special ******************
        public void CassetteMapDownload_Array(Line line, Equipment eqp, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                if (line.Data.LINETYPE != eLineType.ARRAY.CAC_MYTEK)
                {
                    foreach (Job j in slotData)
                    {
                        JobDataIntoPLC(line, j, outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1]);
                    }
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        // Timeout
        private void CassetteMappingDownloadTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string[] obj = timer.State.ToString().Split('_');
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", sArray[0], sArray[1]);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                
                // modify by bruce 2015/9/16 for CAC line
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line.Data.LINETYPE != eLineType.ARRAY.CAC_MYTEK)
                {
                    outputdata.EventGroups[0].IsDisable = true;
                    outputdata.EventGroups[1].Events[0].IsDisable = true;
                    outputdata.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                    outputdata.TrackKey = obj[0];
                }
                else
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                    outputdata.TrackKey = obj[0];
                }
                SendPLCData(outputdata);
                eCstControlCmd cmd = (eCstControlCmd)int.Parse(obj[1]);
                Port port = ObjectManager.PortManager.GetPort(sArray[1]);

                if (cmd == eCstControlCmd.MapDownload)
                {
                    if (port != null)
                    {
                        Invoke(eServiceName.UIService, "CassetteMapDownloadResultReport", new object[] { port, 99 });
                    }
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] CASSETTE_CONTROL_COMMAND=[{3}] REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], obj[0], sArray[1], cmd.ToString()));

                if (port != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { obj[0], port.Data.LINEID, 
                        string.Format("Cassette{0}Reply - EQUIPMENT=[{1}] PORT=[{2}] \"T1 TIMEOUT\"", 
                        cmd, port.Data.NODENO, port.Data.PORTNO)});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Job Data Request]
        public void JobDataRequestReportReply_Array(Trx outputData, Job job, Line line, string eqpNo)
        {
            try
            {
                // 將Job Data 塞入PLC的值裡
                JobDataIntoPLC(line, job, outputData.EventGroups[0].Events[0]);

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}", eqpNo, JobDataRequestReportReplyTimeOut);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(JobDataRequestReportTimeoutForEQP), outputData.TrackKey);

                string log = string.Format("CST_SEQNO=[{0}] JOB_SEQNO=[{1}] GlassChipMaskBlockID=[{2}]", 
                                        job != null ? job.CassetteSequenceNo : "",
                                        job != null ? job.JobSequenceNo : "",
                                        job != null ? job.GlassChipMaskBlockID : "");

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] RETURN_CODE=[{2}] {3}, SET BIT=[ON].",
                    eqpNo, outputData.TrackKey, eReturnCode1.OK, log));
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

                string timeoutName = string.Format("{0}_{1}", sArray[0], JobDataRequestReportReplyTimeOut);
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
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] JOB DATA REQUEST REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], sArray[1]));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region ProcessTypeReport
        public void ProcessTypeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                 
                
                for (int i = 0; i < inputData.EventGroups[0].Events[0].Items.Count-1;i++ )
                {
                    Port port = ObjectManager.PortManager.GetPort(string.Format("0{0}",(i+1).ToString()));

                    if (port != null)
                    {
                        port.File.ProcessType = inputData.EventGroups[0].Events[0].Items[i].Value;
                        #region [取得EQP資訊]
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                        #endregion

                        ObjectManager.PortManager.EnqueueSave(port.File);

                        //Invoke(eServiceName.UIService, "CassetteMapDownloadResultReport", new object[] { port, 99 });

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP{1}] PORT=[{2}] PROCESS_TYPE=[{3}]]",
                        eqp.Data.NODENO, inputData.TrackKey, port.Data.PORTNO, port.File.ProcessType));
                    }

                    
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }
        
        #endregion

        public void ProcessTimeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [取得EQP資訊]
                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                string processTime = inputData.EventGroups[0].Events[0].Items[ePLC.ProcessTime].Value;
                
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] PROCESS TIME=[{2}]",
                    eqp.Data.NODENO, inputData.TrackKey, processTime));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void LineBackupMode(Trx inputData)
        {
            try
            {
              //  if (inputData.IsInitTrigger) return; //modify by yang 20161228
                if (inputData.IsInitTrigger) Thread.Sleep(3000);

                #region [取得EQP資訊]
                string eqpNo = inputData.Metadata.NodeNo;
                string trxid = inputData.TrackKey;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                Equipment cln = ObjectManager.EquipmentManager.GetEQP("L3");//add by yang 2016/12/27
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                eBitResult lineBackupMode = (eBitResult)int.Parse( inputData.EventGroups[0].Events[0].Items[ePLC.LineBackupModeChangeReport_LineBackupMode].Value);

                eqp.File.LineBackupMode = lineBackupMode;
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] LINE BACKUP MODE=[{2}]",
                  eqp.Data.NODENO, trxid, lineBackupMode));

                #region[yang 20161228 update CLN backup mode]
                string clnkey = eqp.File.LineBackupMode == eBitResult.ON ? "2" : "1"; //yang             
                ConstantItem item = null;
                   item = ConstantManager["ARRAY_BACKUPMODE_ELA"][clnkey];
                   lock (cln)
                        cln.File.EquipmentRunMode = item.Discription;
                   ObjectManager.EquipmentManager.EnqueueSave(cln.File);
                   Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { trxid, cln });
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }     

        #region LineBackupModeChangeReport
        public void LineBackupModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    LineBackupModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [取得EQP資訊]
                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                eEnableDisable lineBackupMode =(eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                //eRobotOperationMode mode = (eRobotOperationMode)int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value));
                //eRobotOperationAction action = (eRobotOperationAction)int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value);

                //RobotPosition rp = ObjectManager.RobotPositionManager.GetRobotPosition(positionNo);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] Line Backup Mode=[{2}]({3})",
                    eqp.Data.NODENO, inputData.TrackKey, (int)lineBackupMode, lineBackupMode.ToString()));

                LineBackupModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    LineBackupModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        public void LineBackupModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                string trxName = string.Format("{0}_LineBackupModeChangeReportReply", eqpNo);
                string returnCode = string.Empty;
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (outputdata != null)
                {
                    //if (line.File.HostMode == eHostMode.OFFLINE)  
                    //{
                        outputdata.EventGroups[0].Events[0].Items[ePLC.LineBackupModeChangeReport_ReturnCode].Value = ((int)(eReturnCode1.OK)).ToString();
                        returnCode=string.Format("[{0}]({1}) ",((int)eReturnCode1.OK).ToString(),eReturnCode1.OK.ToString());
                    //}
                    //else
                    //{
                        // to do 
                        //outputdata.EventGroups[0].Events[0].Items[ePLC.LineBackupModeChangeReport_ReturnCode].Value = "0";
                    //}

                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, LineBackupModeChangeReportReplyTO);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(LineBackupModeChangeReportReplyTimeOut), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}] Return Code={3}", 
                        eqpNo, trackKey, value,returnCode));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void LineBackupModeChangeReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] LINE BACKUP MODE CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                LineBackupModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region JobLineOutCheckReport
        public void JobLineOutCheckReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    JobLineOutCheckReportReply(inputData.Metadata.NodeNo, eBitResult.OFF,eReturnCode1.OK, inputData.TrackKey);
                    return;
                }

                #region [取得EQP資訊]
                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                string cstSeqno =inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSeqno=inputData.EventGroups[0].Events[0].Items[1].Value;
                Job job = ObjectManager.JobManager.GetJob(cstSeqno,jobSeqno);
                    

                if (job == null)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("CAN'T FIND JOB INFORMATION IN BCS, CST_SEQNO=[{0}] JOB_SEQNO=[{1}].",
                        cstSeqno, jobSeqno));                  
                }

                string JobNo = inputData.EventGroups[0].Events[0].Items[0].Value;

                
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}]",
                    eqp.Data.NODENO, inputData.TrackKey, job.CassetteSequenceNo, job.JobSequenceNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                ARRAY_ShortCutCreateWIP(line, eqp, job, inputData.TrackKey);

                //JobLineOutCheckReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    //JobLineOutCheckReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        public void JobLineOutCheckReportReply(string eqpNo, eBitResult value, eReturnCode1 returnCode, string trackKey)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                string trxName = string.Format("{0}_JobLineOutCheckReportReply", eqpNo);
                string retrunCode = string.Empty;
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                
                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[ePLC.JobLineOutCheck_ReturnCode].Value = ((int)returnCode).ToString();
                    retrunCode = string.Format("[{0}]({1})", ((int)eReturnCode1.OK).ToString(), eReturnCode1.OK.ToString());

                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)returnCode).ToString();
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, JobLineOutCheckReportReplyTO);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(JobLineOutCheckReportReplyTimeOut), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}] Return Code={3}",
                        eqpNo, trackKey, value, retrunCode));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void JobLineOutCheckReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] JOB LINE OUT CHECK REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                JobLineOutCheckReportReply(sArray[0], eBitResult.OFF,eReturnCode1.OK, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region JobLineOutReport
        public void JobLineOutReport(Trx inputData)
        {
            Job job = null;
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    JobLineOutReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [取得EQP資訊]
                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                string cstSeqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobSeqno = inputData.EventGroups[0].Events[0].Items[1].Value;
                job = ObjectManager.JobManager.GetJob(cstSeqno, jobSeqno);


                if (job == null)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("CAN'T FIND JOB INFORMATION IN BCS, CST_SEQNO=[{0}] JOB_SEQNO=[{1}].",
                        cstSeqno, jobSeqno));
                }
                else
                {
                    #region EQP FLAG:
                    IDictionary<string, string> sub = ObjectManager.SubJobDataManager.GetSubItem(eJOBDATA.EQPFlag);
                    if (sub != null)
                    {
                        if (sub.ContainsKey(eEQPFLAG.Array.BackupProcessFlag))
                        {
                            IDictionary<string, string> eqpflag = ObjectManager.SubJobDataManager.Decode(inputData.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag].Value, eJOBDATA.EQPFlag);
                            sub[eEQPFLAG.Array.BackupProcessFlag] = eqpflag[eEQPFLAG.Array.BackupProcessFlag].ToString();
                            job.ArraySpecial.BackupProcessFlag = sub[eEQPFLAG.Array.BackupProcessFlag]; //add by bruce 2016/3/16
                            job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, eJOBDATA.EQPFlag);
                        }
                        ObjectManager.JobManager.EnqueueSave(job);
                    }
                    #endregion

                    if (job.ArraySpecial.BackupProcessFlag == "1" && eqp.File.LineBackupMode== eBitResult.OFF)    //只要不是Backup Mode 的line 需將WIP remove,
                    {
                        //mark for avoid cst quit 2016/03/17 cc.kuang
                        //job.RemoveFlag = true;  //不能真得刪除，可能隨時會Regeistor
                        //job.RemoveReason = string.Format("BACKUP MODE LINE OUT, NORMAL REMOVE BY BC");

                        //ObjectManager.JobManager.EnqueueSave(job);

                        //ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, "00", "00", job.JobSequenceNo, eJobEvent.Remove.ToString());
                        
                        //Send MES Data
                        //object retVal = base.Invoke(eServiceName.MESService, "ProductScrapped", new object[]{
                        //    inputData.TrackKey, eqp.Data.LINEID, eqp, job, reasonCode });
                        ;
                    }

                    object retVal = base.Invoke(eServiceName.MESService, "ProductLineOut", new object[]{
                            inputData.TrackKey, eqp.Data.LINEID, job, eqp.Data.NODEID, "",""});

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}]",
                                eqp.Data.NODENO, inputData.TrackKey, job.CassetteSequenceNo, job.JobSequenceNo));
                }

                JobLineOutReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    JobLineOutReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        public void JobLineOutReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                string trxName = string.Format("{0}_JobLineOutReportReply", eqpNo);
                
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);


                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, JobLineOutReportReplyTO);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(JobLineOutReportReplyTimeOut), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}]",
                        eqpNo, trackKey, value));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void JobLineOutReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] JOB LINE OUT REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                JobLineOutReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region JobLineInReport
        public void JobLineInReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    JobLineInReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [取得EQP資訊]
                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                string casseqno = inputData.EventGroups[0].Events[0].Items[0].Value;
                string jobseqno = inputData.EventGroups[0].Events[0].Items[1].Value;

                Job job = ObjectManager.JobManager.GetJob(casseqno, jobseqno);

                if (job == null)
                {
                    job = NewJob(eqp.Data.LINEID, casseqno, jobseqno, inputData);
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, string.Empty, eJobEvent.EQP_NEW.ToString(), inputData.TrackKey);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("EQUIPMENT=[{0}] BCS CREATE NEW JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!",
                           eqp.Data.NODENO, casseqno, jobseqno));
                }
                else
                {
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("EQUIPMENT=[{0}] JOB DATA IS ALREADY EXIST, JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!",
                           eqp.Data.NODENO, casseqno, jobseqno));
                }

                lock (job) job.CurrentEQPNo = eqp.Data.NODENO;

                Event eVent = inputData.EventGroups[0].Events[0];


                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));

                #region 更新Job Data
                lock (job)
                {
                    // Common Data
                    job.GroupIndex = eVent.Items[eJOBDATA.GroupIndex].Value;
                    job.ProductType.Value = int.Parse(eVent.Items[eJOBDATA.ProductType].Value);
                    job.CSTOperationMode = (eCSTOperationMode)int.Parse(eVent.Items[eJOBDATA.CSTOperationMode].Value);
                    job.SubstrateType = (eSubstrateType)int.Parse(eVent.Items[eJOBDATA.SubstrateType].Value);
                    job.CIMMode = (eBitResult)int.Parse(eVent.Items[eJOBDATA.CIMMode].Value);
                    job.JobType = (eJobType)int.Parse(eVent.Items[eJOBDATA.JobType].Value);
                    job.JobJudge = eVent.Items[eJOBDATA.JobJudge].Value.ToString();
                    job.SamplingSlotFlag = eVent.Items[eJOBDATA.SamplingSlotFlag].Value.ToString();
                    job.FirstRunFlag = eVent.Items[eJOBDATA.FirstRunFlag].Value.ToString();
                    job.JobGrade = eVent.Items[eJOBDATA.JobGrade].Value.Trim();//此处需要做trim tom 2015-04-06
                    job.PPID = eVent.Items[eJOBDATA.PPID].Value.ToString();
                    job.ArraySpecial.GlassFlowType = eVent.Items[eJOBDATA.GlassFlowType].Value;
                    job.ArraySpecial.ProcessType = eVent.Items[eJOBDATA.ProcessType].Value;
                    job.LastGlassFlag = eVent.Items[eJOBDATA.LastGlassFlag].Value;
                    job.ArraySpecial.RtcFlag = eVent.Items[eJOBDATA.RTCFlag].Value;

                    // special Data
                    if (eVent.Items[eJOBDATA.MainEQInFlag] != null)
                        job.ArraySpecial.MainEQInFlag = eVent.Items[eJOBDATA.MainEQInFlag].Value;
                    if (eVent.Items[eJOBDATA.RecipeGroupNumber] !=null)
                        job.ArraySpecial.RecipeGroupNumber = eVent.Items[eJOBDATA.RecipeGroupNumber].Value;
                    
                    job.InspJudgedData = eVent.Items[eJOBDATA.InspJudgedData].Value;
                    job.TrackingData = eVent.Items[eJOBDATA.TrackingData].Value;
                    job.EQPFlag = eVent.Items[eJOBDATA.EQPFlag].Value;

                    if (eVent.Items[eJOBDATA.SourcePortNo] !=null)
                        job.ArraySpecial.SourcePortNo = eVent.Items[eJOBDATA.SourcePortNo].Value;
                    if (eVent.Items[eJOBDATA.TargetPortNo] !=null)
                        job.ArraySpecial.TargetPortNo = eVent.Items[eJOBDATA.TargetPortNo].Value;

                    if (eVent.Items[eJOBDATA.TargetCSTID] != null)
                    job.TargetCSTID = eVent.Items[eJOBDATA.TargetCSTID].Value;                    

                    if (eVent.Items[eJOBDATA.SorterGrade] != null)
                    job.ArraySpecial.SorterGrade = eVent.Items[eJOBDATA.SorterGrade].Value;

                    #region EQP FLAG:
                        IDictionary<string,string> sub = ObjectManager.SubJobDataManager.GetSubItem(eJOBDATA.EQPFlag);
                        if (sub != null)
                        {
                            if (sub.ContainsKey(eEQPFLAG.Array.BackupProcessFlag))
                            {
                                IDictionary<string, string> eqpflag = ObjectManager.SubJobDataManager.Decode(inputData.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag].Value, eJOBDATA.EQPFlag);
                                sub[eEQPFLAG.Array.BackupProcessFlag] =eqpflag[eEQPFLAG.Array.BackupProcessFlag].ToString();
                                job.ArraySpecial.BackupProcessFlag = sub[eEQPFLAG.Array.BackupProcessFlag]; //add by bruce 2016/3/16
                                job.EQPFlag = ObjectManager.SubJobDataManager.Encode(sub, eJOBDATA.EQPFlag);
                            }
                        }
                    #endregion

                    ObjectManager.JobManager.EnqueueSave(job);
                }
                #endregion
                    string unitId = string.Empty;
                    Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, "2");
                    if (unit != null)
                    {
                        unitId = unit.Data.UNITID;
                    }
                    job.RemoveFlag = false;  //Regeistor回來了
                    ObjectManager.JobManager.EnqueueSave(job);
                    ObjectManager.JobManager.RecordJobHistory(job, eqp.Data.NODEID, eqp.Data.NODENO, "", "", "", eJobEvent.Remove.ToString(), inputData.TrackKey);
                    object retVal = base.Invoke(eServiceName.MESService, "ProductLineIn", new object[]{
                    inputData.TrackKey, eqp.Data.LINEID, job, eqp.Data.NODEID,"", unitId});

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}]",
                    eqp.Data.NODENO, inputData.TrackKey, job.CassetteSequenceNo, job.JobSequenceNo));

                JobLineInReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    JobLineInReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        public void JobLineInReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                string trxName = string.Format("{0}_JobLineInReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);


                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, JobLineInReportReplyTO);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(JobLineInReportReplyTimeOut), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                        eqpNo, trackKey, value));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void JobLineInReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] JOB LINE IN REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                JobLineInReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region ForceCleanOutReport
        public void ForceCleanOutReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    ForceCleanOutReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [取得EQP資訊]
                //string eqpNo = inputData.Metadata.NodeNo;
                //Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                //if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                eForceCleanOutAction action =(eForceCleanOutAction)int.Parse( inputData.EventGroups[0].Events[0].Items[ePLC.ForceCleanOutReport_Action].Value);
                eForceCleanOutType  type = (eForceCleanOutType)int.Parse( inputData.EventGroups[0].Events[0].Items[ePLC.ForceCleanOutReport_Type].Value);

                //Trx outputData = null;
                eBitResult result = eBitResult.OFF;
                switch (action)
                {
                    case eForceCleanOutAction.Set:
                        result = eBitResult.ON;
                        break;
                    case eForceCleanOutAction.ReSet:
                        result = eBitResult.OFF;
                        break;
                    default:
                        break;
                }
                
                if (type==eForceCleanOutType.Abnormal)
                    Invoke("ArraySpecialService", "AbnormalForceCleanOutCommand", new object[] { result, inputData.TrackKey });
                else
                    Invoke("ArraySpecialService", "ForceCleanOutCommand", new object[] { result, inputData.TrackKey });


                //foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
                //{
                //    string tranName = eqp.Data.NODENO + "_ForceCleanOutCommand";

                //    outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(tranName) as Trx;
                //    if (outputData != null)
                //    {
                //        outputData.EventGroups[0].Events[0].Items[0].Value =result;
                //        outputData.TrackKey = inputData.TrackKey;
                //        SendPLCData(outputData);

                //        string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, ForceCleanOutTimeout);

                //        if (_timerManager.IsAliveTimer(timeName))
                //        {
                //            _timerManager.TerminateTimer(timeName);
                //        }

                //        _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                //            new System.Timers.ElapsedEventHandler(ForceCleanOutCommandTimeout), outputData.TrackKey);
                //    }
                //}

                Event eVent = inputData.EventGroups[0].Events[0];


                //Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                //if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));

                //Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}]",
                //    eqp.Data.NODENO, inputData.TrackKey, job.CassetteSequenceNo, job.JobSequenceNo));

                ForceCleanOutReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    ForceCleanOutReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        public void ForceCleanOutReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                string trxName = string.Format("{0}_ForceCleanOutReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);


                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[ePLC.ForceCleanOutReport_ReturnCode].Value = ((int)eBitResult.ON).ToString(); //  回覆 OK
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, JobLineInReportReplyTO);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(ForceCleanOutReportReplyTimeOut), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] RETURN CODE [OK] SET BIT=[{2}].",
                        eqpNo, trackKey, value));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void ForceCleanOutReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] FORCE CLEAN OUT REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                ForceCleanOutReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region EquipmentChamberModeChangeReport
        public void EquipmentChamberModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    EquipmentChamberModeChangeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }
               

                string chambermode1 = string.Empty;
                string chambermode2 = string.Empty;
                string chambermode3 = string.Empty;
                string chambermode4 = string.Empty;
                string chambermode5 = string.Empty;
                string chambermode6 = string.Empty;

                List<string> lstchambermode = new List<string>();
                int selchamberno = 0;
                string tid = string.Empty;
                tid = inputData.TrackKey;
                
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);
                chambermode1 = inputData.EventGroups[0].Events[0].Items[0].Value;
                chambermode2 = inputData.EventGroups[0].Events[0].Items[1].Value;
                chambermode3 = inputData.EventGroups[0].Events[0].Items[2].Value;
                chambermode4 = inputData.EventGroups[0].Events[0].Items[3].Value;
                chambermode5 = inputData.EventGroups[0].Events[0].Items[4].Value;
                chambermode6 = inputData.EventGroups[0].Events[0].Items[5].Value;
                lstchambermode.Add(chambermode1);
                lstchambermode.Add(chambermode2);
                lstchambermode.Add(chambermode3);
                lstchambermode.Add(chambermode4);
                lstchambermode.Add(chambermode5);
                lstchambermode.Add(chambermode6);

                if (triggerBit == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
                    EquipmentChamberModeChangeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqpNo);
                if (units == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN UNITENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                List<MesSpec.ChamberRunModeChanged.CHAMBERc> chamberList = new List<MesSpec.ChamberRunModeChanged.CHAMBERc>();
                bool MixFlag = false;
                string currMode = string.Empty;
                foreach (Unit u in units)
                {
                    if (u.Data.UNITTYPE == "CHAMBER")
                    {
                        u.File.OldChamberRunMode = u.File.ChamberRunMode;   //記錄前一個Chamber mode
                        u.File.ChamberRunMode = lstchambermode[selchamberno];
                        u.File.RunMode = lstchambermode[selchamberno];
                        selchamberno++;
                        if (selchamberno > 6)
                            break;
                        /*
                        switch (u.Data.UNITNO)
                        {
                            case "1":
                                u.File.ChamberRunMode = chambermode1;
                                u.File.RunMode = chambermode1;
                                break ;
                            case "2":
                                u.File.ChamberRunMode = chambermode2;
                                u.File.RunMode = chambermode2;
                                break ;
                            case "3":
                                u.File.ChamberRunMode = chambermode3;
                                u.File.RunMode = chambermode3;
                                break ;
                            case "4":
                                u.File.ChamberRunMode = chambermode4;
                                u.File.RunMode = chambermode4;
                                break ;
                            case "5":
                                u.File.ChamberRunMode = chambermode5;
                                u.File.RunMode = chambermode5;
                                break ;
                            case "6":
                                u.File.ChamberRunMode = chambermode6;
                                u.File.RunMode = chambermode6;
                                break ;
                            default :
                                break ;
                        }
                        */
                        // mode是報給MES值, description是解釋
                        string runmode = GetChamberMode(line, eqpNo, u.File.ChamberRunMode, out description);
                        /*
                        u.File.ChamberRunMode = runmode;
                        u.File.RunMode = runmode;
                        */
                        u.File.ChamberRunMode = description; //modify 2010/10/09 cc.kuang
                        u.File.RunMode = description;
                        if (currMode.Trim().Length == 0)
                        {
                            if (description.Trim().Length > 0)
                                currMode = description;
                        }
                        else
                        {
                            if (description.Trim().Length > 0)
                                if (!currMode.Equals(description.Trim()))
                                    MixFlag = true;
                        }
                        MesSpec.ChamberRunModeChanged.CHAMBERc cb = new MesSpec.ChamberRunModeChanged.CHAMBERc();
                        cb.CHAMBERNAME = u.Data.UNITID;
                        cb.CHAMBERRUNMODE = description;
                        chamberList.Add(cb);

                        ObjectManager.UnitManager.EnqueueSave(u.File);

                        ObjectManager.UnitManager.RecordUnitHistory(inputData.TrackKey,u);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] UNITNO=[{4}] EQUIPMENT_CHAMBER_MODE=[{2}]({3}).",
                        eqpNo, inputData.TrackKey, u.File.ChamberRunMode, description,u.Data.UNITNO));
                    }

                }
                

                if (line.Data.FABTYPE == eFabType.ARRAY.ToString())
                {
                    // Array 部份由PLC觸發的事件, 沒有報給MES, 所以填Description給OPI顯示用
                    lock (eqp)
                    {
                        if (line.Data.LINETYPE == eLineType.ARRAY.CVD_ULVAC ||
                            line.Data.LINETYPE == eLineType.ARRAY.CVD_AKT ||
                            line.Data.LINETYPE == eLineType.ARRAY.DRY_ICD ||
                            line.Data.LINETYPE == eLineType.ARRAY.DRY_YAC ||
                            line.Data.LINETYPE==eLineType.ARRAY.DRY_TEL)
                        {
                            if (MixFlag)
                            {
                                eqp.File.EquipmentRunMode = "MIX";
                            }
                            else
                            {
                                if (currMode.Trim().Length == 0)
                                    eqp.File.EquipmentRunMode = "NORMAL";
                                else
                                    //eqp.File.EquipmentRunMode = currMode;
                                    eqp.File.EquipmentRunMode = "NORMAL";
                            }
                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                            ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
                        }
                    }
                }



                Invoke(eServiceName.LineService, "CheckLineRunMode", new object[] { line.Data.LINEID });                

                EquipmentChamberModeChangeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);

                if (line.Data.LINETYPE == eLineType.ARRAY.CVD_AKT || line.Data.LINETYPE == eLineType.ARRAY.CVD_ULVAC ||
                    line.Data.LINETYPE == eLineType.ARRAY.DRY_ICD || line.Data.LINETYPE == eLineType.ARRAY.DRY_YAC
                    ||line.Data.LINETYPE==eLineType.ARRAY.DRY_TEL)
                {
                    Invoke(eServiceName.MESMessageService, "MachineModeChangeRequest",
                        new object[] { UtilityMethod.GetAgentTrackKey(), line.Data.LINEID }); //2015/12/17 modify send new trx key for avoid ChamberRunModeChanged trx key are the same. cc.kuang
                }
                Thread.Sleep(2000); //wait mes reply then report chamber mode

                if (line.Data.LINETYPE != eLineType.ARRAY.CVD_AKT && line.Data.LINETYPE != eLineType.ARRAY.CVD_ULVAC)
                {
                    Invoke(eServiceName.MESService, "ChamberRunModeChanged",
                    new object[] { tid, eqp.Data.LINEID, line.File.LineOperMode, eqp.Data.NODEID, chamberList });
                }

                #region OPI Service Send
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                Invoke(eServiceName.UIService, "LineStatusReport", new object[] { inputData.TrackKey, line });
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    EquipmentChamberModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        public void EquipmentChamberModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_EquipmentChamberModeChangeReportReply") as Trx;
                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = "1"; // Reply OK
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    if (_timerManager.IsAliveTimer(eqpNo + "_" + EquipmentChamberModeReportReplyTimeOut))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + EquipmentChamberModeReportReplyTimeOut);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(eqpNo + "_" + EquipmentChamberModeReportReplyTimeOut, false,
                            ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(EquipmentChamberModeChangeReportReplyTimeOut), trackKey);
                    }
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                        eqpNo, trackKey, value.ToString()));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void EquipmentChamberModeChangeReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENT RUN MODE CHANGE REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                EquipmentChamberModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private string GetChamberMode(Line line, string eqpNo, string value, out string description)
        {
            description = string.Empty;
            ConstantItem item = null;
            switch (line.Data.LINETYPE)
            {
                case eLineType.ARRAY.CVD_AKT:
                case eLineType.ARRAY.CVD_ULVAC: item = ConstantManager["ARRAY_CHAMBERMODE_CVD"][value]; break;
                case eLineType.ARRAY.DRY_ICD:
                case eLineType.ARRAY.DRY_TEL:
                case eLineType.ARRAY.DRY_YAC: item = ConstantManager["ARRAY_CHAMBERMODE_DRY"][value]; break;
                case eLineType.ARRAY.MSP_ULVAC:
                case eLineType.ARRAY.ITO_ULVAC: item = ConstantManager["ARRAY_CHAMBERMODE_PVD"][value]; break;
                default: item = new ConstantItem(); break;
            }

            description = item.Discription;
            return item.Value;
        }
        public void EquipmentChamberModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string chambermode1 = string.Empty;
                string chambermode2 = string.Empty;
                string chambermode3 = string.Empty;
                string chambermode4 = string.Empty;
                string chambermode5 = string.Empty;
                string chambermode6 = string.Empty;

                List<string> lstchambermode = new List<string>();
                int selchamberno = 0;
                string tid = string.Empty;
                tid = inputData.TrackKey;

                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);
                chambermode1 = inputData.EventGroups[0].Events[0].Items[0].Value;
                chambermode2 = inputData.EventGroups[0].Events[0].Items[1].Value;
                chambermode3 = inputData.EventGroups[0].Events[0].Items[2].Value;
                chambermode4 = inputData.EventGroups[0].Events[0].Items[3].Value;
                chambermode5 = inputData.EventGroups[0].Events[0].Items[4].Value;
                chambermode6 = inputData.EventGroups[0].Events[0].Items[5].Value;
                lstchambermode.Add(chambermode1);
                lstchambermode.Add(chambermode2);
                lstchambermode.Add(chambermode3);
                lstchambermode.Add(chambermode4);
                lstchambermode.Add(chambermode5);
                lstchambermode.Add(chambermode6);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Get EQ Object is Null",
                        eqpNo, inputData.TrackKey));

                IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqpNo);
                if (units == null)
                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Get Unit Objects is Null",
                        eqpNo, inputData.TrackKey));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null)
                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Get Linr Object is Null",
                        eqpNo, inputData.TrackKey));

                bool MixFlag = false;
                string currMode = string.Empty;
                foreach (Unit u in units)
                {
                    if (u.Data.UNITTYPE == "CHAMBER")
                    {
                        u.File.OldChamberRunMode = u.File.ChamberRunMode;   //記錄前一個Chamber mode
                        u.File.ChamberRunMode = lstchambermode[selchamberno];
                        u.File.RunMode = lstchambermode[selchamberno];
                        selchamberno++;
                        if (selchamberno > 6)
                            break;
                        // mode是報給MES值, description是解釋
                        string runmode = GetChamberMode(line, eqpNo, u.File.ChamberRunMode, out description);
                        u.File.ChamberRunMode = description; //modify 2010/10/09 cc.kuang
                        u.File.RunMode = description;
                        if (currMode.Trim().Length == 0)
                        {
                            if (description.Trim().Length > 0)
                                currMode = description;
                        }
                        else
                        {
                            if (description.Trim().Length > 0)
                                if (!currMode.Equals(description.Trim()))
                                    MixFlag = true;
                        }

                        ObjectManager.UnitManager.EnqueueSave(u.File);
                        ObjectManager.UnitManager.RecordUnitHistory(inputData.TrackKey,u);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] UNITNO=[{2}] CHAMBER_MODE=[{3}]",
                        eqpNo, inputData.TrackKey, u.Data.UNITNO, u.File.ChamberRunMode));
                    }

                }


                if (line.Data.FABTYPE == eFabType.ARRAY.ToString())
                {
                    // Array 部份由PLC觸發的事件, 沒有報給MES, 所以填Description給OPI顯示用
                    lock (eqp)
                    {
                        if (line.Data.LINETYPE == eLineType.ARRAY.CVD_ULVAC ||
                            line.Data.LINETYPE == eLineType.ARRAY.CVD_AKT ||
                            line.Data.LINETYPE == eLineType.ARRAY.DRY_ICD ||
                            line.Data.LINETYPE == eLineType.ARRAY.DRY_YAC ||
                            line.Data.LINETYPE==eLineType.ARRAY.DRY_TEL)
                        {
                            if (MixFlag)
                            {
                                eqp.File.EquipmentRunMode = "MIX";
                            }
                            else
                            {
                                if (currMode.Trim().Length == 0)
                                    eqp.File.EquipmentRunMode = "NORMAL";
                                else
                                    //eqp.File.EquipmentRunMode = currMode;
                                    eqp.File.EquipmentRunMode = "NORMAL";
                            }
                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                            ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region ARRAY ELA Short CUT
        private void ARRAY_ShortCutCreateWIP(Line line, Equipment eqp, Job job, string trxId)
        {
            if (line.Data.LINETYPE != eLineType.ARRAY.ELA_JSW) return;  //add by bruce 2015/09/30 Array ELA 跨line 使用
            if (eqp.Data.NODENO == "L2") //Indexer Turn Table
            {
                //IServerAgent activeSocketAgent = null;
                //IServerAgent passiveSocketAgent = null;
                //activeSocketAgent = GetServerAgent("ActiveSocketAgent");
                //passiveSocketAgent = GetServerAgent("PassiveSocketAgent");
                object[] _objJob = new object[1] { job };

                switch (line.Data.LINEID)
                {
                    case "TCELA100":
                    case "TCELA300":
                        Invoke(eServiceName.PassiveSocketService, "ArrayShortCut", _objJob);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> BCS][{1}] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] GLASS_ID=[{4}].", line.Data.LINEID, trxId, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID));
                        break;
                    case "TCELA200":
                        Invoke(eServiceName.ActiveSocketService, "ArrayShortCut", _objJob);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> BCS][{1}] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] GLASS_ID=[{4}].", line.Data.LINEID, trxId, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID));
                        break;
                    default:
                        break;
                }

                //if (passiveSocketAgent != null)
                //{
                //    Invoke(eServiceName.PassiveSocketService, "ArrayShortCut", _objJob);
                //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //    string.Format("[LINENAME={0}] [BCS -> BCS][{1}] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] GLASS_ID=[{4}].", line.Data.LINEID, trxId, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID));
                //}
                //else if (activeSocketAgent != null)
                //{
                //    Invoke(eServiceName.ActiveSocketService, "ArrayShortCut", _objJob);
                //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //    string.Format("[LINENAME={0}] [BCS -> BCS][{1}] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] GLASS_ID=[{4}].", line.Data.LINEID, trxId, job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID));
                //}
            }
        }
        #endregion
        private Job NewJob(string lineID, string casseqno, string jobseqno, Trx inputData)
        {
            try
            {

                int c, j;
                if (!int.TryParse(casseqno, out c)) throw new Exception(string.Format("Create Job Failed Cassette Sequence No isn't number!!", casseqno));

                if (c == 0) throw new Exception(string.Format("Create Job Failed Cassette Sequence No (0)!!", casseqno));

                if (!int.TryParse(jobseqno, out j)) throw new Exception(string.Format("Create Job Failed Job Sequence No isn't number!!", jobseqno));

                if (j == 0) throw new Exception(string.Format("Create Job Failed Job Sequence No (0)!!", jobseqno));

                Job job = new Job(c, j);

                //SECS 不會有值
                if (inputData == null)
                {
                    ObjectManager.JobManager.NewJobCreateMESDataEmpty(job);
                    ObjectManager.JobManager.AddJob(job);
                    return job;
                }

                Line line = ObjectManager.LineManager.GetLine(lineID);

                if (line == null) throw new Exception(string.Format("Create Job Failed Line[{0}] IS NOT IN LINEENTITY!!", lineID));


                //Watson Add 20150416 此值不會跟著job event作update，不會隨機台上報而改變，
                //但是NEW的JOB沒有值，需要相信機台上報的
                Event eVent = inputData.EventGroups[0].Events[0];
                //Jun Add 20150429 New Job 需要更新Glass ID by CSOT
                job.GlassChipMaskBlockID = eVent.Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value.ToString().Trim();
                job.ChipCount = int.Parse(eVent.Items[eJOBDATA.ChipCount].Value);

                string oxrInfomation = string.Empty;
                eFabType fabtype;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabtype);

                ObjectManager.JobManager.NewJobCreateMESDataEmpty(job);
                ObjectManager.JobManager.AddJob(job); //與駿龍確認過 相信機台
                return job;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void JobDataIntoPLC(Line line, Job job, Event evt)
        {
            try
            {
                evt.Items[eJOBDATA.GlassFlowType].Value = job.ArraySpecial.GlassFlowType;
                evt.Items[eJOBDATA.ProcessType].Value = job.ArraySpecial.ProcessType;
                evt.Items[eJOBDATA.LastGlassFlag].Value = job.LastGlassFlag; //t3 use cc.kuang 20150702 IPC/BC, up->low/low->up

                evt.Items[eJOBDATA.InspJudgedData].Value = job.InspJudgedData;
                evt.Items[eJOBDATA.TrackingData].Value = job.TrackingData;

                evt.Items[eJOBDATA.EQPFlag].Value = job.EQPFlag;

                evt.Items[eJOBDATA.ChipCount].Value = job.ChipCount.ToString();

                // if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.PROCESS_EQ) // modify by bruce 2015/7/15 目前Array All line 都有使用
                    evt.Items[eJOBDATA.RecipeGroupNumber].Value = job.ArraySpecial.RecipeGroupNumber;
                
                //t3 Changer Function use cc.kuang 2015/07/02
                //if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.TEST_1 || line.Data.LINETYPE == eLineType.ARRAY.CHN_SEEC) // modify by bruce 2015/7/15 不使用JobDataLineType
                if (evt.Items[eJOBDATA.TargetSequenceNo]!= null) 
                    evt.Items[eJOBDATA.TargetSequenceNo].Value = job.ArraySpecial.TargetSequenceNo;
                

                //t3 PVD use cc.kuang 2015/07/02
                //if (line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC) // modify by bruce 2015/7/15 不使用LineType
                if (evt.Items[eJOBDATA.TargetLoadLockNo]!=null)
                    evt.Items[eJOBDATA.TargetLoadLockNo].Value = job.ArraySpecial.TargetLoadLockNo;

                /* t3 not use cc.kuang 2015/07/02
                if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.TEST_1 || line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.SORT)
                    evt.Items[eJOBDATA.TargetCSTID].Value = job.TargetCSTID;
                */

                //if (line.Data.JOBDATALINETYPE == eJobDataLineType.ARRAY.SORT) // modify by bruce 2015/7/15 不使用JobDataLineType
                if (evt.Items[eJOBDATA.SorterGrade]!=null)
                    evt.Items[eJOBDATA.SorterGrade].Value = job.ArraySpecial.SorterGrade;

                //if (line.Data.JOBDATALINETYPE != eJobDataLineType.ARRAY.ABFG) // modify by bruce 2015/7/15 不使用JobDataLineType
                if (evt.Items[eJOBDATA.SourcePortNo]!= null) 
                    evt.Items[eJOBDATA.SourcePortNo].Value = job.ArraySpecial.SourcePortNo;

                //#region [OXR Information] 
                // modify by bruce 2015/7/15 T3不使用 OXR Info
                //string oxrInfo = string.Empty;
                //for (int i = 0; i < job.OXRInformation.Length; i++)
                //{
                //    if (i.Equals(56)) break;
                //    oxrInfo += ConstantManager["PLC_OXRINFO_AC"][job.OXRInformation.Substring(i, 1)].Value;
                //}
                //evt.Items[eJOBDATA.OXRInformation].Value = oxrInfo;
                //#endregion
            }
            catch(Exception ex)
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

        #region [Check EQP PROCESS TIME  ]

        //public void StartThread()
        //{
            
          
        //        if (!threadStart && (_checkEqpProcessTime == null))
        //        {
        //            _checkEqpProcessTime = new Thread(new ThreadStart(CheckEqpProcessTime));
        //            _checkEqpProcessTime.IsBackground = true;
        //            _checkEqpProcessTime.Start();
        //            threadStart = true;
        //        }
        
        //}

        /// 
        private void CheckEqpProcessTime()
        {
            while (_isRuning)
            {
                Thread.Sleep(60000);
                try
                {
                    Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName );
                    if(line.Data.FABTYPE=="ARRAY")
                    {
                    IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                    IList<Job> jobs = ObjectManager.JobManager.GetJobs();
                    List<Job> query = jobs.ToList<Job>();
                    foreach (Equipment eqp in eqps)
                    {
                   //   if (eqp.Data.NODENAME == "PVD") eqp.File.EqProcessTimeSetting = 40;  //test
                        if (eqp.Data.NODENO == "L2" || eqp.File.EqProcessTimeSetting==0) continue;
                        else
                        {
                            if (eqp.Data.NODEID.Contains("TTP"))
                            {
                                DateTime dt = DateTime.Now;
                                query = query.Where(j => j.CurrentEQPNo == eqp.Data.NODENO && ((dt - j.ArraySpecial.JobProcessStartedTime).TotalMinutes > eqp.File.EqProcessTimeSetting) && j.RemoveFlag == false && j.ArraySpecial.ProcessType.Trim() != "1" && int.Parse(j.CassetteSequenceNo) < 5000).ToList<Job>();
                                if (query.Count > 0)
                                {
                                    lock (eqp.File)
                                    {
                                        eqp.File.MESStatus = "DOWN";
                                    }
                                    string alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                                    object[] obj = new object[]          
                                {
                                    dt.ToString("yyyyMMddHHmmssffff"),       /*0 TrackKey*/
                                    eqp,                                    /*1 Equipment*/
                                    "BC2MES-0002",                          /*2 alarmID*/
                                    string.Format ("EQUIPMENT Job count [{0}]Process TIME OUT,first glassid[{1}]",query.Count,query[0].GlassChipMaskBlockID),             /*3 alarmText*/
                                    alarmTime                               /*4 alarmTime*/
                                };
                                    Invoke(eServiceName.MESService, "MachineStateChanged", obj);// 20171225 add by qiumin  eq process time out  CFM must show down

                                    continue;
                                }
                                continue;
                            }
                            else if (eqp.Data.NODEID.Contains("ITO") || eqp.Data.NODEID.Contains("MSP"))
                            {
                                DateTime dt = DateTime.Now;
                                query = query.Where(j => j.CurrentEQPNo == eqp.Data.NODENO && ((dt - j.ArraySpecial.JobProcessStartedTime).TotalMinutes > eqp.File.EqProcessTimeSetting) && j.RemoveFlag == false && j.ArraySpecial.ProcessType.Trim() != "1" && j.ArraySpecial.ProcessType.Trim() != "2").ToList<Job>();
                                if (query.Count > 0)
                                {
                                    lock (eqp.File)
                                    {
                                        eqp.File.MESStatus = "DOWN";
                                    }
                                    string alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                                    object[] obj = new object[]          
                                {
                                    dt.ToString("yyyyMMddHHmmssffff"),       /*0 TrackKey*/
                                    eqp,                                    /*1 Equipment*/
                                    "BC2MES-0002",                          /*2 alarmID*/
                                    string.Format ("EQUIPMENT Job count [{0}]Process TIME OUT,first glassid[{1}]",query.Count,query[0].GlassChipMaskBlockID),             /*3 alarmText*/
                                    alarmTime                               /*4 alarmTime*/
                                };
                                    Invoke(eServiceName.MESService, "MachineStateChanged", obj);// 20171225 add by qiumin  eq process time out  CFM must show down

                                    continue;
                                }
                                continue;
                            }
                            else
                            {
                                DateTime dt = DateTime.Now;
                                query = query.Where(j => j.CurrentEQPNo == eqp.Data.NODENO && ((dt - j.ArraySpecial.JobProcessStartedTime).TotalMinutes > eqp.File.EqProcessTimeSetting) && j.RemoveFlag == false && j.ArraySpecial.ProcessType.Trim() != "1").ToList<Job>();
                                if (query.Count > 0)
                                {
                                    lock (eqp.File)
                                    {
                                        eqp.File.MESStatus = "DOWN";
                                    }
                                    string alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                                    object[] obj = new object[]          
                                {
                                    dt.ToString("yyyyMMddHHmmssffff"),       /*0 TrackKey*/
                                    eqp,                                    /*1 Equipment*/
                                    "BC2MES-0002",                          /*2 alarmID*/
                                    string.Format ("EQUIPMENT Job count [{0}]Process TIME OUT,first glassid[{1}]",query.Count,query[0].GlassChipMaskBlockID),             /*3 alarmText*/
                                    alarmTime                               /*4 alarmTime*/
                                };
                                    Invoke(eServiceName.MESService, "MachineStateChanged", obj);// 20171225 add by qiumin  eq process time out  CFM must show down
                                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("NodeID [{0}] Job count[{1}] Porcess Time out,first glassid[{2}],CST SEQ_JOB SEQ=[{3}],JobProcessStartedTime=[{4}],JOBCUREQP=[{5}]", eqp.Data.NODEID, query.Count, query[0].GlassChipMaskBlockID,query[0].JobKey,query[0].ArraySpecial.JobProcessStartedTime,query[0].CurrentEQPNo));
                                    continue;
                                }
                                continue;

                            }
                        }

                    }
                    }
                    if(line.Data.FABTYPE=="CELL")
                    {
                        IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                        IList<Job> jobs = ObjectManager.JobManager.GetJobs();
                        List<Job> query = jobs.ToList<Job>();
                        foreach (Equipment eqp in eqps)
                        {
                            //   if (eqp.Data.NODENAME == "PVD") eqp.File.EqProcessTimeSetting = 40;  //test
                            if (eqp.Data.NODENO == "L2" || eqp.File.EqProcessTimeSetting == 0) continue;
                            else
                            {
                                if (eqp.Data.NODEID.Contains("SDP") || eqp.Data.NODEID.Contains("LCD"))
                                {
                                    IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);
                                    foreach (Unit unit in units)
                                    {
                                        DateTime dt = DateTime.Now;
                                        List<Job> query1 = query.Where(j => j.CurrentEQPNo == eqp.Data.NODENO && j.CurrentUNITNo == unit.Data.UNITNO && ((dt - j.CellSpecial.UnitProcessStartTime).TotalMinutes > eqp.File.EqProcessTimeSetting) && j.RemoveFlag == false).ToList<Job>();
                                        if (query1.Count > 0)
                                        {
                                            lock (unit.File)
                                            {
                                                unit.File.Status = eEQPStatus.STOP;
                                                unit.File.MESStatus = "DOWN";
                                            }
                                            string alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                                            object[] obj = new object[]          
                                {
                                    dt.ToString("yyyyMMddHHmmssffff"),       /*0 TrackKey*/
                                    unit,                                    /*1 unit*/
                                    "BC2MES-0002",                          /*2 alarmID*/
                                    string.Format ("EQUIPMENT Job count [{0}]Process TIME OUT,first glassid[{1}]",query1.Count,query1[0].GlassChipMaskBlockID),            /*3 alarmText*/
                                    alarmTime                               /*4 alarmTime*/
                                };
                                            Invoke(eServiceName.MESService, "UnitStateChanged", obj);// 20181119 add by hujunpeng  unit process time out  CFM must show down
                                            Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("UnitID [{0}] Job count[{1}] Porcess Time out,first glassid[{2}]", unit.Data.UNITID, query1.Count, query1[0].GlassChipMaskBlockID));
                                            continue;
                                    }
                                    
                                    }
                                    continue;
                                }
                                if (eqp.Data.NODEID.Contains("SLI"))
                                {
                                    DateTime dt = DateTime.Now;
                                    query = query.Where(j => j.CurrentEQPNo == eqp.Data.NODENO && ((dt - j.ArraySpecial.JobProcessStartedTime).TotalMinutes > eqp.File.EqProcessTimeSetting) && j.RemoveFlag == false ).ToList<Job>();
                                    if (query.Count > 0)
                                    {
                                        lock (eqp.File)
                                        {
                                            eqp.File.MESStatus = "DOWN";
                                            eqp.File.Status = eEQPStatus.STOP;
                                        }
                                        string alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                                        object[] obj = new object[]          
                                {
                                    dt.ToString("yyyyMMddHHmmssffff"),       /*0 TrackKey*/
                                    eqp,                                    /*1 Equipment*/
                                    "BC2MES-0002",                          /*2 alarmID*/
                                    string.Format ("EQUIPMENT Job count [{0}]Process TIME OUT,first glassid[{1}]",query.Count,query[0].GlassChipMaskBlockID),             /*3 alarmText*/
                                    alarmTime                               /*4 alarmTime*/
                                };
                                        Invoke(eServiceName.MESService, "MachineStateChanged", obj);// 20171225 add by qiumin  eq process time out  CFM must show down
                                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("NodeID [{0}] Job count[{1}] Porcess Time out,first glassid[{2}]", eqp.Data.NODEID, query.Count, query[0].GlassChipMaskBlockID));
                                        continue;
                                    }
                                    continue;
                                }                               
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex.ToString());

                }

            }

        }
        #endregion

        //Add By wangshengjun20191030
        #region [CurrentRecipeChangeTactTimeTransferCommand]
        public void CurrentRecipeChangeTactTimeTransferCommand(string eqpNo, string Word1 , string Word2, string trackKey)
        {
            try
            {
                string err = string.Empty;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));
                #region CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND VCR MODE CHANGE COMMAND!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(eqpNo+"_"+"CurrentRecipeChangeTactTimeTransferCommand")) as Trx;
                outputData.EventGroups[0].Events[0].Items[0].Value = Word1;
                outputData.EventGroups[0].Events[0].Items[1].Value = Word2;
                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = trackKey;
                SendPLCData(outputData);
                string timeName = string.Format(eqpNo+"_"+"CurrentRecipeChangeTactTimeTransferCommandReplyTimeout");
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(CurrentRecipeChangeTactTimeTransferCommandReplyTimeout), outputData.TrackKey);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Word1=[{2}],Word2=[{3}], SET BIT=[ON].", eqp.Data.NODENO,outputData.TrackKey, Word1,Word2));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void CurrentRecipeChangeTactTimeTransferCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] BIT=[{2}] Current Recipe Change TactTime Transfer Command Reply.",
                    eqpNo, inputData.TrackKey, triggerBit.ToString()));

                string timeName = string.Format(inputData.Metadata.NodeNo + "_" + "CurrentRecipeChangeTactTimeTransferCommandReplyTimeout");
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                if (triggerBit == eBitResult.OFF) return;

                #region [Command Off]
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(inputData.Metadata.NodeNo +"_"+ "CurrentRecipeChangeTactTimeTransferCommand")) as Trx;
                outputData.EventGroups[0].Events[0].IsDisable = true;
                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputData.TrackKey = inputData.TrackKey;
                SendPLCData(outputData);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[OFF].", eqpNo, inputData.TrackKey));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void CurrentRecipeChangeTactTimeTransferCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format(sArray[0]+"_"+"CurrentRecipeChangeTactTimeTransferCommandReplyTimeout");

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(sArray[0] + "_" + "CurrentRecipeChangeTactTimeTransferCommand")) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] Current Recipe Change TactTime Transfer Command REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion
    
    }
}
