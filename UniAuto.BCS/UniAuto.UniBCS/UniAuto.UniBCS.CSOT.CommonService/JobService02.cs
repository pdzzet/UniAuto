using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Timers;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public partial class JobService
    {
        private const string JobRemoveRecoveryCommandTimeOut = "JobRemoveRecoveryCommandTimeOut";
        private const string FirstGlassCheckTimeout = "FirstGlassCheckTimeout";
        private const string DefectCodeReplyTimeOut = "DefectCodeReportReplyTimeOut";
        private const string JobDataRequestReportReplyTimeOut = "JobDataRequestReportReplyTimeOut";

        #region Job Remove Recovery Command
        public void JobRemoveRecoveryCommand(string trxid,string currentEqpNo,string cstSeq, string slotSeq, string lastGlassFlag, eJobCommand command)
        {
            try
            {
                //Jun Add 20150401 在CELL不需要RemoveRecoveryComammd功能
                Equipment chkeqp = ObjectManager.EquipmentManager.GetEQP(currentEqpNo);
                if (chkeqp != null)
                {
                    Line line = ObjectManager.LineManager.GetLine(chkeqp.Data.LINEID);
                    if (line != null)
                    {
                        if (line.Data.FABTYPE == eFabType.CELL.ToString())
                            return;
                    }
                }

                IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                foreach (Equipment eqp in eqps)
                {
                    //if (eqp.Data.NODENO == currentEqpNo) continue; 经过三厂扛把子确认 本机台也要下RemoveRecoverCommand Tom 20150202

                    if (eqp.File.CIMMode == eBitResult.OFF)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] SKIP REPORT, CST_SEQNO=[{3}] AND JOB_SEQNO=[{4}].",
                            eqp.Data.NODENO, trxid, eqp.File.CIMMode, cstSeq, slotSeq));
                        continue;
                    }
                    //20141015 cy:增加判斷給SECS機台的
                    eReportMode reportMode;
                    Enum.TryParse<eReportMode>(eqp.Data.REPORTMODE, out reportMode);
                    switch (reportMode)
                    {
                        case eReportMode.HSMS_CSOT:
                        case eReportMode.HSMS_PLC:
                            {
                                Job job = ObjectManager.JobManager.GetJob(cstSeq, slotSeq);
                                if (job != null)
                                {
                                    //(string eqpno, string eqpid, string cstseq, string slot, string glassid, string er, string lastflag, string tag)
                                    Invoke(eServiceName.CSOTSECSService, "TS2F117_H_GlassEraseRecoveryInformationSend",
                                        new object[] {eqp.Data.NODENO, eqp.Data.NODEID,
                                                  cstSeq, slotSeq, job.GlassChipMaskBlockID, 
                                                  command == eJobCommand.JOBREMOVE ? "1" : "2",
                                                  lastGlassFlag, string.Empty, trxid});

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                          string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}], SEND HSMS=[{3}].", eqp.Data.NODENO,
                                          trxid, eqp.File.CIMMode.ToString(), "TS2F117_H_GlassEraseRecoveryInformationSend"));
                                }
                                else
                                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CAN NOT FIND JOB DATA WITH CST_SEQNO=[{2}] AND JOB_SEQNO=[{3}].",
                                                                eqp.Data.NODENO, trxid, job.CassetteSequenceNo, job.JobSequenceNo));
                            }
                            break;
                        case eReportMode.PLC:
                        case eReportMode.PLC_HSMS:
                            {
                                Trx trx = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format("{0}_JobRemoveRecoveryCommand", eqp.Data.NODENO)) as Trx;

                                if (trx != null)
                                {
                                    trx.EventGroups[0].Events[0].Items[0].Value = ((int)command).ToString();
                                    trx.EventGroups[0].Events[0].Items[1].Value = cstSeq;
                                    trx.EventGroups[0].Events[0].Items[2].Value = slotSeq;
                                    trx.EventGroups[0].Events[0].Items[3].Value = lastGlassFlag;
                                    trx.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                                    trx.EventGroups[0].Events[1].Items[0].Value = "1";
                                    trx.TrackKey = trxid;
                                    SendPLCData(trx);
                                    string timerId = string.Format("{0}_{1}", eqp.Data.NODENO, JobRemoveRecoveryCommandTimeOut);
                                    if (_timerManager.IsAliveTimer(timerId))
                                    {
                                        _timerManager.TerminateTimer(timerId);
                                    }
                                    _timerManager.CreateTimer(timerId, false, ParameterManager["T1"].GetInteger(), new ElapsedEventHandler(JobRemoveRecoveryCommandTimeout), trxid);

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] JOB_COMMAND=[{2}] CST_SEQNO=[{3}] AND JOB_SEQNO=[{4}], SET BIT=[ON]",
                                                eqp.Data.NODENO, trx.TrackKey, command, cstSeq, slotSeq));
                                }
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
        private void JobRemoveRecoveryCommandTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string timeName = string.Format("{0}_{1}", sArray[0], JobRemoveRecoveryCommandTimeOut);
                //终止Timer
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_JobRemoveRecoveryCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] JOB REMOVE\\RECOVERY COMMAND REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void JobRemoveRecoveryCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 returnCode =(eReturnCode1)int.Parse( inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                   eqpNo, inputData.TrackKey, triggerBit.ToString(), (int)returnCode, returnCode.ToString()));

                //终止Timer
                string timeName = string.Format("{0}_{1}", eqpNo, JobRemoveRecoveryCommandTimeOut);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;

                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_JobRemoveRecoveryCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;

                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, GetType().Name, "JobRemoveRecoveryCommand()",
                                   string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF]", eqpNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion


        public void SetLastGlassCommand(string trxid, string currentEqpNo, Job job)
        {
            try
            {
                IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                foreach (Equipment eqp in eqps)
                {
                    if (eqp.File.CIMMode == eBitResult.OFF)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] SKIP REPORT, CST_SEQNO=[{3}] AND JOB_SEQNO=[{4}].",
                            eqp.Data.NODENO, trxid, eqp.File.CIMMode, job.CassetteSequenceNo, job.JobSequenceNo));
                        continue;
                    }
                    //20141103 cy:增加SECS機台的報告
                    eReportMode reportMode;
                    Enum.TryParse<eReportMode>(eqp.Data.REPORTMODE, out reportMode);
                    switch (reportMode)
                    {
                        case eReportMode.HSMS_PLC:
                        case eReportMode.HSMS_CSOT:
                                if (job != null)
                                {
                                    //(string eqpno, string eqpid, string cstseq,string slot,string glassid, string ppid, string tag, string trxid)
                                    Invoke(eServiceName.CSOTSECSService, "TS2F105_H_LotEndInformSend",
                                        new object[] {eqp.Data.NODENO, eqp.Data.NODEID,
                                                  job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID, 
                                                  job.PPID, string.Empty, trxid});
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                      string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}], SEND HSMS=[{3}].", eqp.Data.NODENO,
                                      trxid, eqp.File.CIMMode.ToString(), "S2F105_LotEndInformSend"));
                                }
                                else
                                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CAN NOT FIND JOB DATA WITH CST_SEQNO=[{2}] AND JOB_SEQNO=[{3}].",
                                                                eqp.Data.NODENO, trxid, job.CassetteSequenceNo, job.JobSequenceNo));
                            break;
                        case eReportMode.PLC:
                        case eReportMode.PLC_HSMS:
                            {
                                Trx trx = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format("{0}_SetLastGlassCommand", eqp.Data.NODENO)) as Trx;

                                if (trx != null)
                                {
                                    trx.EventGroups[0].Events[0].Items[0].Value = job.CassetteSequenceNo;
                                    trx.EventGroups[0].Events[0].Items[1].Value = job.JobSequenceNo;
                                    trx.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                                    trx.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                                    trx.TrackKey = trxid;
                                    SendPLCData(trx);
                                    string timerId = string.Format("{0}_SetLastGlassCommandTimeout", eqp.Data.NODENO);
                                    if (Timermanager.IsAliveTimer(timerId))
                                    {
                                        Timermanager.TerminateTimer(timerId);
                                    }
                                    Timermanager.CreateTimer(timerId, false, ParameterManager["T1"].GetInteger(), new ElapsedEventHandler(SetLastGlassCommandTimeout), trxid);

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CST_SEQNO=[{2}] AND JOB_SEQNO=[{3}], SET BIT=[ON]", 
                                                eqp.Data.NODENO, trx.TrackKey, job.CassetteSequenceNo, job.JobSequenceNo));
                                }
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

        private void SetLastGlassCommandTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string timeName = string.Format("{0}_SetLastGlassCommandTimeout", sArray[0]);
                //终止Timer
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_SetLastGlassCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET LAST GLASS COMMAND REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SetLastGlassCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 returnCode =(eReturnCode1)int.Parse( inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                   eqpNo, inputData.TrackKey, triggerBit.ToString(), (int)returnCode, returnCode.ToString()));


                //终止Timer
                string timeName = string.Format("{0}_SetLastGlassCommandTimeout", eqpNo);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                if (triggerBit == eBitResult.OFF) return;

                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_SetLastGlassCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;

                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                   string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].", eqpNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void FirstGlassCheckReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                if (ObjectManager.RobotManager.GetRobots().Count > 0)
                {
                    Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN NOT TRIGER FirstGlassCheckReport Function at BC Control Robort!", eqpNo));
                    return;
                }
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
                    FirstGlassCheckReportReply(inputData.TrackKey, eBitResult.OFF, eReturnCode1.OK, eqpNo);
                    return;
                }

                #region [拆出PLCAgent Data]
                string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                string cassetteID = inputData.EventGroups[0].Events[0].Items[1].Value;
                string portNo = inputData.EventGroups[0].Events[0].Items[2].Value.PadLeft(2, '0');
                #endregion

                #region [取得資訊]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                //Watson Modify 不能用servename去取，會有兩條line的
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN EQUIPMENTENTITY!", eqp.Data.LINEID));
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] CSTID=[{4}] PORT_NO=[{5}].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, eqp.File.CIMMode, cassetteSequenceNo, cassetteID, portNo));

                #region [Report MES] 目前只需要上報MES,無須等待回應。
                Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if(port==null)
                {
                    Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN NOT FIND PORT=[{1}] IN PORT OBJECT!", eqpNo, portNo));
                    return;
                }				
                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(cassetteSequenceNo));
                if (cst == null)
                {
                    Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN NOT FIND CAS_SEQNO=[{1}] IN CST OBJECT!", eqpNo, cassetteSequenceNo));
                    return;
                }
                else
                {
                    cst.FirstGlassCheckReport = "C1"; //wait check reply cc.kuang 2015/07/10
                }

                #region [Reply EQ] 直接下 Start，無需再行判斷。
                if (line.File.HostMode == eHostMode.OFFLINE || (port.File.Type != ePortType.BothPort && port.File.Type != ePortType.LoadingPort)) //t3 modify cc.kuang 2015/07/10
                {
                    FirstGlassCheckReportReply(inputData.TrackKey, eBitResult.ON, eReturnCode1.OK, eqpNo);
                    cst.FirstGlassCheckReport = "Y";
                    return;
                }
                #endregion

                object[] _data = new object[3]
                { 
                    inputData.TrackKey,               /*0  TrackKey*/
                    port,                             /*1  Port*/
                    cst,                              /*2  Cassette*/
                };

                //呼叫MES方法
                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                if (line.Data.LINETYPE == eLineType.CELL.CBMCL)
                    return;

                if (fabType == eFabType.CELL && port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                {
                    if (line.Data.LINEID != eLineType.CELL.CBDPI) //Watson Modiyf 20150130 For DPI Special Handle
                        Invoke(eServiceName.MESService, "BoxProcessStarted", _data);

                    lock (cst) cst.FirstGlassCheckReport = "Y";
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }
                else
                {
                    Job inCSTjob = ObjectManager.JobManager.GetJobs(cst.CassetteSequenceNo).FirstOrDefault();
                    object[] _data2 = new object[4]
                    { 
                        inputData.TrackKey,
                        port,
                        cst,
                        inCSTjob
                    };
                    Invoke(eServiceName.MESService, "LotProcessStartRequest", _data2);
                    //Invoke(eServiceName.APCService, "LotProcessStart", _data);
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void FirstGlassCheckReportReply(string trxID, eBitResult bitResut,eReturnCode1 returnCode, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_FirstGlassCheckReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                if (bitResut == eBitResult.ON)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)returnCode).ToString(); //寫入word
                }
                else
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true; // H.S.正常結束，無須填寫 Word
                }
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)bitResut).ToString(); //寫入bit
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger(); //Delay Turn On Bit 
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, FirstGlassCheckTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(FirstGlassCheckReportTimeoutForEQP), trxID);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] RETURN_CODE=[{2}], SET BIT=[{3}].",
                    eqpNo, trxID, returnCode, bitResut));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void FirstGlassCheckReportTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] FIRST GLASS CHECK REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                FirstGlassCheckReportReply(trackKey, eBitResult.OFF,eReturnCode1.OK, sArray[0]);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void JobDataRequestReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string commandNo = string.Empty;
                if (inputData.Name.Split(new char[] { '#' }).Length == 2)
                    commandNo = inputData.Name.Split(new char[] { '#' })[1];         //zxx add 20160830  for JobDataRequestReport#0X
                
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                string eqpNo = inputData.Metadata.NodeNo;

                #region [Report Off]
                string trxName = string.Format("{0}_JobDataRequestReportReply", eqpNo);
                //添加通道;
                if (string.IsNullOrEmpty(commandNo))
                {
                    trxName = string.Format("{0}_JobDataRequestReportReply", eqpNo);
                }
                else
                {
                    trxName = string.Format("{0}_JobDataRequestReportReply", eqpNo) + "#" + commandNo;
                }
                // zxx add 20160830  for JobDataRequestReport#0X

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));

                    //JobDataRequestReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    JobDataRequestReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey, commandNo);   //add by zxx 20160830 双通道
                    return;
                }
                #endregion

                #region [拆出PLCAgent Data]
                string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string jobSequenceNo = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string glassID = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                string operationID = inputData.EventGroups[0].Events[0].Items[3].Value.Trim();
                string usedFlag = inputData.EventGroups[0].Events[0].Items[4].Value.Trim();

                if (GetLinebyEquipmentNo(inputData.Metadata.NodeNo).Data.FABTYPE == eFabType.CELL.ToString())
                {
                    operationID = inputData.EventGroups[0].Events[0].Items[4].Value.Trim();  
                    usedFlag = inputData.EventGroups[0].Events[0].Items[3].Value.Trim();  //CELL Request Type	"1：By Job Number 2：By Glass ID" 與AC 廠同
                }
                #endregion

                //sy modify 20160911 MES Reply 沒有 #01 02 之分，不能只用一個直去存，會被後報的所覆蓋掉 #00 用來記錄 沒有#結尾的
                #region Add CommandNo keyup 
                string keyCommandNo = keyBoxReplyPLCKey.PanelRequestReplyCommandNo + "#" + (commandNo == string.Empty ? "00" : commandNo.PadLeft(2, '0'));  //add keyup CommandNo
                //string rep = inputData.Metadata.NodeNo;
                if (Repository.ContainsKey(keyCommandNo))
                    Repository.Remove(keyCommandNo);
                Repository.Add(keyCommandNo, commandNo +"#"+ glassID);
                #endregion

                #region [取得EQP資訊]
                Equipment eqp;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [取得Line資訊]
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", inputData.Metadata.NodeNo));
                #endregion
                string log = string.Empty;

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}] GLASSID=[{4}] OPERATION_ID=[{5}] USED_FLAG=[{6}]({7}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, cassetteSequenceNo, jobSequenceNo, glassID, operationID, usedFlag, 
                    usedFlag.Equals("1") ? "USED_BY CST_SEQNO&&JOB_SEQNO" : usedFlag.Equals("2") ? "USED_BY GLASSID"
                    : usedFlag.Equals("3") ? "USED_BY GLASSID PanelInformationRequest recipeParaCheck=N" 
                    : usedFlag.Equals("4") ? "USED_BY GLASSID PanelInformationRequest recipeParaCheck=Y": "UNKNOWN"));

                #region [Reply EQ]
                eReturnCode1 returnCode = eReturnCode1.Unknown;
                Job jobData = null;
                switch (usedFlag)
                {
                    case "1": //1：Used by Cassette Sequence No, Job Sequence No
                        jobData = ObjectManager.JobManager.GetJob(cassetteSequenceNo, jobSequenceNo);
                        if (jobData == null)
                        {
                            returnCode = eReturnCode1.NG;
                        }
                        else
                        {
                            returnCode = eReturnCode1.OK;
                        }
                        log = string.Format("CST_SEQNO=[{0}] JOB_SEQNO=[{1}]", cassetteSequenceNo, jobSequenceNo);
                        break;
                    case "2": //2：Used by Glass ID
                        jobData = ObjectManager.JobManager.GetJob(glassID);
                        if (jobData == null)
                        {
                            returnCode = eReturnCode1.NG;
                        }
                        else
                        {
                            returnCode = eReturnCode1.OK;
                        }
                        log = string.Format("CSTID=[{0}]", glassID);
                        break;
                    case "3": //3：Used by Glass ID to  PanelInformationRequest    recipeParaCheck=N 
                        #region [OFFLINE]
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                            //JobDataRequestReportReplyForPanelInformation(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, null);
                            JobDataRequestReportReplyForPanelInformation(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey,null,commandNo);  //add CommandNo 通道 by zhuxingxing  20160901
                            return;
                        }
                        #endregion

                        #region [PCS EQP Report GlassID is not Corret]
                        //20170425 : huangjiayin add for PCS BLOCKOX Type3 Request
                        if (line.Data.LINETYPE == eLineType.CELL.CCPCS)
                        {
                            Job PCS_Job = ObjectManager.JobManager.GetBlockJob(glassID);
                            if (PCS_Job == null)
                            {
                                JobDataRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, commandNo);
                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[BCS <- EQP]=[{0}] JobDataRequest<Type3>  LINENAME =[{1}],MACHINENAME =[{2}],JOBID =[{3}] is not exist!",
                                inputData.TrackKey, line.Data.LINEID, eqp.Data.NODEID, glassID));
                                return;
 
                            }
                        }

                        #endregion

                        #region [Glass is exist]
                        //jobData = ObjectManager.JobManager.GetJob(glassID);
                        //if (jobData != null)
                        //{
                        //    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",string.Format("EQUIPMENT=[{0}] GLASS IS EXIST!!", eqpNo));
                        //    JobDataRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                        //    return;
                        //}
                        #endregion
                        Invoke(eServiceName.MESService, "PanelInformationRequest", new object[6] { inputData.TrackKey, line.Data.LINEID, eqp.Data.NODEID, glassID, commandNo,"N" });//sy modify 20160911
                        return;
                    case "4": //4：Used by Glass ID to  PanelInformationRequest    recipeParaCheck=Y 目前都沒作用    
                        #region [OFFLINE]
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                            //JobDataRequestReportReplyForPanelInformation(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, null);
                            JobDataRequestReportReplyForPanelInformation(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, null,commandNo);  //add commandNo by zhuxingxing 20160901 
                            return;
                        }
                        #endregion                        
                        #region [Glass is exist]
                        //jobData = ObjectManager.JobManager.GetJob(glassID);
                        //if (jobData != null)
                        //{
                        //    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",string.Format("EQUIPMENT=[{0}] GLASS IS EXIST!!", eqpNo));
                        //    JobDataRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                        //    return;
                        //}
                        #endregion
                        Invoke(eServiceName.MESService, "PanelInformationRequest", new object[6] { inputData.TrackKey, line.Data.LINEID, eqp.Data.NODEID, glassID, commandNo, "Y" });//sy modify 20160911
                        return;
                    default:
                        
                        break;
                }

                if (jobData == null)
                {
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("EQUIPMENT=[{0}] CAN NOT FOUND JOB DATA, {1}.", eqpNo, log));
                    //JobDataRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                    JobDataRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey,commandNo);
                    return;
                }

                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)returnCode).ToString();
                outputData.EventGroups[0].Events[2].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[0].Events[2].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = inputData.TrackKey;

                outputData.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSequenceNo].Value = jobData.CassetteSequenceNo;   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobSequenceNo].Value = jobData.JobSequenceNo;   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.GroupIndex].Value = jobData.GroupIndex;   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductType].Value = jobData.ProductType.Value.ToString();   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.CSTOperationMode].Value = ((int)jobData.CSTOperationMode).ToString();   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.SubstrateType].Value = ((int)jobData.SubstrateType).ToString();   //INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.CIMMode].Value = ((int)jobData.CIMMode).ToString();   ////INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobType].Value = ((int)jobData.JobType).ToString();   ////INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobJudge].Value = jobData.JobJudge;   ////INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.SamplingSlotFlag].Value = jobData.SamplingSlotFlag;   ////INT
                //outputData.EventGroups[0].Events[0].Items["OXRInformationRequestFlag"].Value = jobData.OXRInformationRequestFlag;   ////INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.FirstRunFlag].Value = jobData.FirstRunFlag;   ////INT
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.JobGrade].Value = jobData.JobGrade;   //ASCII
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value = jobData.GlassChipMaskBlockID;   //ASCII
                outputData.EventGroups[0].Events[0].Items[eJOBDATA.PPID].Value = jobData.PPID;   //ASCII


                object[] _data = new object[4]
                        { 
                            outputData,           /*0  TrackKey*/
                            jobData,              /*1  Job*/
                            line,                 /*2  Line*/ 
                            eqpNo                 /*3  EQP No*/                         
                        };

                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if (string.IsNullOrEmpty(commandNo))
                    {
                           _data = new object[5]
                           { 
                                outputData,           /*0  TrackKey*/
                                jobData,              /*1  Job*/
                                line,                 /*2  Line*/ 
                                eqpNo,                 /*3  EQP No*/ 
                                commandNo              /*4  commandNo */
                           };
                    }
                    else
                    {
                        _data = new object[5]
                        { 
                            outputData,           /*0  TrackKey*/
                            jobData,              /*1  Job*/
                            line,                 /*2  Line*/ 
                            eqpNo,                 /*3  EQP No*/ 
                            commandNo          /*4  #0X */       //add #0X 通道 by zhuxingxing 20160901                      
                        };
                    }
                }             

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                switch (fabType)
                {
                    case eFabType.ARRAY: Invoke(eServiceName.ArraySpecialService, "JobDataRequestReportReply_Array", _data); break;
                    case eFabType.CF: Invoke(eServiceName.CFSpecialService, "JobDataRequestReportReply_CF", _data); break;
                    case eFabType.CELL: Invoke(eServiceName.CELLSpecialService, "JobDataRequestReportReply_CELL", _data); break;
                    case eFabType.MODULE: Invoke(eServiceName.MODULESpecialService, "JobDataRequestReportReply_MODULE", _data); break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                string commandNo = string.Empty;
                if (inputData.Name.Split(new char[] { '#' }).Length == 2)
                    commandNo = inputData.Name.Split(new char[] { '#' })[1];   //add 双通道请求资料 by zxx 20160830

                if (inputData.EventGroups[0].Events[2].Items[0].Value.Equals("1"))
                {
                    JobDataRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey,commandNo);
                }
            }
        }

        public void JobDataRequestReportReply(string eqpNo, eBitResult value, string trackKey, string commandNo)
        {
            try
            {
                string trxName = string.Format("{0}_JobDataRequestReportReply", eqpNo);
               
                //add 双通道的请求资料 by ZXX 20160830 
                if (string.IsNullOrEmpty(commandNo))
                {
                    trxName = string.Format("{0}_JobDataRequestReportReply", eqpNo);
                }
                else
                {
                    trxName = string.Format("{0}_JobDataRequestReportReply", eqpNo) + "#" + commandNo;
                }
                //end 

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                string log = string.Empty;
                if (outputdata != null)
                {
                    if (value == eBitResult.OFF)
                    {
                        outputdata.EventGroups[0].Events[0].IsDisable = true; //Job Data
                        outputdata.EventGroups[0].Events[1].IsDisable = true; //Return Code
                        outputdata.EventGroups[0].Events[2].Items[0].Value = ((int)eBitResult.OFF).ToString(); //Result bit

                    }
                    else
                    {
                        log = "RETURN_CODE=[NG], ";
                        outputdata.EventGroups[0].Events[0].IsDisable = true;
                        outputdata.EventGroups[0].Events[1].Items[0].Value = "2";
                        outputdata.EventGroups[0].Events[2].Items[0].Value = ((int)eBitResult.ON).ToString();
                        //outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    }

                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    //add by zhuxingxing 20160905 
                    string TimeOutName;
                    if (string.IsNullOrEmpty(commandNo))
                    {
                        TimeOutName = string.Format(eqpNo + "_" + JobDataRequestReportReplyTimeOut);
                    }
                    else
                    {
                        TimeOutName = string.Format(eqpNo + "_" + JobDataRequestReportReplyTimeOut) + "#" + commandNo;
                    }
                    if (_timerManager.IsAliveTimer(TimeOutName))
                    {
                        _timerManager.TerminateTimer(TimeOutName);
                    }
                    //end 

                    //if (_timerManager.IsAliveTimer(eqpNo + "_" + JobDataRequestReportReplyTimeOut))
                    //{
                    //    _timerManager.TerminateTimer(eqpNo + "_" + JobDataRequestReportReplyTimeOut);
                    //}

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(TimeOutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(JobDataRequestReportTimeoutForEQP), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] {2}SET BIT=[{3}].",
                        eqpNo, trackKey, log, value.ToString()));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void JobDataRequestReportReplyForPanelInformation(string eqpNo, eBitResult value, string trackKey, Job job, string commandNo)
        {
            try
            {
                string trxName = string.Format("{0}_JobDataRequestReportReply", eqpNo);
                
                // add by zhuxingxing 双通道信息内容 20160901
                if (string.IsNullOrEmpty(commandNo))
                {
                    trxName = string.Format("{0}_JobDataRequestReportReply", eqpNo);
                }
                else
                {
                    trxName = string.Format("{0}_JobDataRequestReportReply", eqpNo) + "#" + commandNo;
                }
                //end 
                #region [Timer]
                //add by zhuxingxing 20160905 TimeOut
                string TimeOutName;
                if (string.IsNullOrEmpty(commandNo))
                {
                    TimeOutName = string.Format(eqpNo + "_" + JobDataRequestReportReplyTimeOut);
                }
                else
                {
                    TimeOutName = string.Format(eqpNo + "_" + JobDataRequestReportReplyTimeOut) + "#" + commandNo;
                }
                if (_timerManager.IsAliveTimer(TimeOutName))
                {
                    _timerManager.TerminateTimer(TimeOutName);
                }
                //end

                //if (_timerManager.IsAliveTimer(eqpNo + "_" + JobDataRequestReportReplyTimeOut))
                //{
                //    _timerManager.TerminateTimer(eqpNo + "_" + JobDataRequestReportReplyTimeOut);
                //}
                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(TimeOutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(JobDataRequestReportTimeoutForEQP), trackKey);
                }
                #endregion
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                string log = string.Empty;
                #region [MES OFFLINE]
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    log = "REPLY=[MES OFFLINE] ";
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = "3";//Return Code
                    outputdata.EventGroups[0].Events[2].Items[0].Value = ((int)eBitResult.ON).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] {2}SET BIT=[{3}].",
                        eqpNo, trackKey, log, value.ToString()));
                    
                    return;
                }              
                #endregion

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eReturnCode1.OK).ToString();
                outputdata.EventGroups[0].Events[2].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputdata.EventGroups[0].Events[2].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;

                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSequenceNo].Value = job.CassetteSequenceNo;   //INT
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.JobSequenceNo].Value = job.JobSequenceNo;   //INT
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.GroupIndex].Value = job.GroupIndex;   //INT
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.ProductType].Value = job.ProductType.Value.ToString();   //INT
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.CSTOperationMode].Value = ((int)job.CSTOperationMode).ToString();   //INT
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.SubstrateType].Value = ((int)job.SubstrateType).ToString();   //INT
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.CIMMode].Value = ((int)job.CIMMode).ToString();   ////INT
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.JobType].Value = ((int)job.JobType).ToString();   ////INT
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.JobJudge].Value = job.JobJudge;   ////INT
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.SamplingSlotFlag].Value = job.SamplingSlotFlag;   ////INT
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.FirstRunFlag].Value = job.FirstRunFlag;   ////INT
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.JobGrade].Value = job.JobGrade;   //ASCII
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.Glass_Chip_Mask_BlockID].Value = job.GlassChipMaskBlockID;   //ASCII
                outputdata.EventGroups[0].Events[0].Items[eJOBDATA.PPID].Value = job.PPID;   //ASCII
                ObjectManager.JobManager.AddJob(job);
                ObjectManager.JobManager.EnqueueSave(job);
               
                object[] _data = new object[4]
                { 
                    outputdata,           /*0  TrackKey*/
                    job,              /*1  Job*/
                    line,                 /*2  Line*/ 
                    eqpNo                /*3  EQP No*/ 
                };

                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if (string.IsNullOrEmpty(commandNo))
                    {
                        _data = new object[5]
                        { 
                            outputdata,           /*0  TrackKey*/
                            job,              /*1  Job*/
                            line,                 /*2  Line*/ 
                            eqpNo,                /*3  EQP No*/ 
                            commandNo
                        };
                    }
                    else
                    {
                        _data = new object[5]
                        { 
                            outputdata,           /*0  TrackKey*/
                            job,                  /*1  Job*/
                            line,                 /*2  Line*/ 
                            eqpNo,                /*3  EQP No*/ 
                            commandNo             /*4  commandNo*/   
                        };
                    }
                }

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                switch (fabType)
                {
                    case eFabType.ARRAY: Invoke(eServiceName.ArraySpecialService, "JobDataRequestReportReply_Array", _data); break;
                    case eFabType.CF: Invoke(eServiceName.CFSpecialService, "JobDataRequestReportReply_CF", _data); break;
                    case eFabType.CELL: Invoke(eServiceName.CELLSpecialService, "JobDataRequestReportReply_CELL", _data); break;
                    case eFabType.MODULE: Invoke(eServiceName.MODULESpecialService, "JobDataRequestReportReply_MODULE", _data); break;
                }
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
                string commandNo = string.Empty;
                if (sArray[1].Split(new char[] { '#' }).Length == 2)
                    commandNo = sArray[1].Split(new char[] { '#' })[1];//zhuxingxing add 20160906 for #0X

                string timeoutName = string.Format("{0}_{1}", sArray[0], JobDataRequestReportReplyTimeOut);
                if(!string.IsNullOrEmpty(commandNo))
                {
                    timeoutName = string.Format("{0}_{1}", sArray[0], JobDataRequestReportReplyTimeOut) + "#" +commandNo;
                }

                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                string trxName = string.Format("{0}_JobDataRequestReportReply", sArray[0]);
                if(!string.IsNullOrEmpty(commandNo))
                {
                    trxName = string.Format("{0}_JobDataRequestReportReply", sArray[0]) + "#"+commandNo;
                }
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

        #region Defect Code Report
        public void DefectCodeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
                    DefectCodeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Event evt = inputData.EventGroups[0].Events[0];
                string CstSeq = evt.Items[0].Value;
                string JobSeq = evt.Items[1].Value;
                string Chipposition = evt.Items[2].Value;
                string UnitNo = evt.Items[3].Value;
                
                string DefectCodes = string.Empty;
                string log = string.Empty;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                if (line.Data.FABTYPE != eFabType.CELL.ToString())
                {
                    for (int i = 4; i < evt.Items.Count; i++)
                    {
                        log += string.Format(" #{0}=[{1}]", i - 3, evt.Items[i].Value);
                        if (string.IsNullOrEmpty(evt.Items[i].Value.Trim())) continue;
                        if (!string.IsNullOrEmpty(DefectCodes)) DefectCodes += ",";
                        DefectCodes += evt.Items[i].Value.Trim();
                    }
                }
                else
                {
                    for (int i = 4; i < evt.Items.Count; i = i + 3)
                    {
                        log += string.Format(" #{0}=[{1}]", (i / 3), evt.Items[i].Value);
                        if (string.IsNullOrEmpty(evt.Items[i].Value.Trim())) continue;
                        if (!string.IsNullOrEmpty(DefectCodes)) DefectCodes += ",";
                        DefectCodes += evt.Items[i].Value.Trim();
                    }
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SETNO=[{2}] JOB_SEQNO=[{3}] CHIP_POSITION=[{4}] UNIT_NO=[{5}] DEFECT_CODE:{6}.",
                    inputData.Metadata.NodeNo, inputData.TrackKey, CstSeq, JobSeq, Chipposition, UnitNo, log));

                DefectCodeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
                lock (eqp) eqp.File.DefectReport = true;
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                Job job = ObjectManager.JobManager.GetJob(CstSeq, JobSeq);
                if (job == null) return;
                // add by bruce 20160414 刪掉之前同樣的 Chipposition 資料
                job.DefectCodes.RemoveAll(d => d.EqpNo == eqpNo && d.UnitNo == UnitNo && d.ChipPostion == Chipposition);

                DefectCode defectcode = new DefectCode();
                defectcode.CSTSeqNo = CstSeq;
                defectcode.JobSeqNo = JobSeq;
                defectcode.EqpNo = eqpNo;
                defectcode.ChipPostion = Chipposition;
                defectcode.UnitNo = UnitNo;
                defectcode.DefectCodes = DefectCodes;
                job.DefectCodes.Add(defectcode);
                ObjectManager.JobManager.EnqueueSave(job);

                int chipposition = int.Parse(Chipposition);
                if(chipposition>job.ChipCount)
                      Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SETNO=[{2}] JOB_SEQNO=[{3}] CHIP_POSITION=[{4}] > CHIP_COUNT[{5}].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, CstSeq, JobSeq, Chipposition, job.ChipCount));

                ObjectManager.JobManager.RecordDefectCodeHistory(inputData.TrackKey, job, defectcode, eqp.Data.NODEID);

                //Watson Modify 20150310 For Defect Code.寫在Send Out 事件
               // CELLDefectCodeReport(inputData, line, eqp, job);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    DefectCodeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        public void DefectCodeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_DefectCodeReportReply") as Trx;
                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    if (_timerManager.IsAliveTimer(eqpNo + "_" + DefectCodeReplyTimeOut))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + DefectCodeReplyTimeOut);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(eqpNo + "_" + DefectCodeReplyTimeOut, false, ParameterManager["T2"].GetInteger(), 
                            new System.Timers.ElapsedEventHandler(DefectCodeReportReplyTimeOut), trackKey);
                    }
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", 
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].", eqpNo, trackKey, value.ToString()));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DefectCodeReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] DEFECT CODE REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                DefectCodeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        public Line GetLinebyEquipmentNo(string NodeNo)
        {
            Line ret = null;
            Equipment eqp = ObjectManager.EquipmentManager.GetEQP(NodeNo);
            if (eqp == null)
                return null;
            ret = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
            return ret;
        }
    }
}
