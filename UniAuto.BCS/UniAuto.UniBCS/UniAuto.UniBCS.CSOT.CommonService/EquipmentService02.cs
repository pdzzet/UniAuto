using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Core;
using System.Timers;
using System.Threading;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public partial class EquipmentService
    {

        private const string IndexerOperationModeReportReplyTimeOut = "_IndexerOperationModeChangeReportReplyTimeOut";
        private const string IndexerOperationModeCommandTimeOut = "_IndexerOperationModeChangeCommandTimeOut";
        private const string EquipmentRunModeReportReplyTimeOut = "_EquipmentRunModeChangeReportReplyTimeOut";
        private const string EquipmentRunModeCommandTimeOut = "_EquipmentRunModeSetCommandTimeOut";
        private const string GlassInfoReportReplyTimeOut = "_GlassInfoChangeReportReplyTimeOut";
        private const string InspectionIdleTimeSetReportReplyTimeOut = "_InspectionIdleTimeSettingReportReplyTimeOut";
        private const string WaitCassetteStatusReportTimeout = "WaitCassetteStatusReportTimeout";
        private const string BufferWarningReportTimeout = "BufferWarningReportTimeout";
        private const string RobotFetchSequenceModeCommandTimeOut = "_RobotFetchSequenceModeCommandTimeOut";
        private const string LineBackupModeChangeTimeout = "LineBackupModeChangeTimeout"; //add by yang 2016/12/27
        #region Indexer Operation Mode Change Report
        public void IndexerOperationModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    IndexerOperationModeChangeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
                    IndexerOperationModeChangeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                string log;
                Handle_IndexerOperationMode(inputData, out log);

                Logger.LogInfoWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] {2}.", eqpNo, inputData.TrackKey, log));

                IndexerOperationModeChangeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    IndexerOperationModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        /// <summary>
        /// 更新記億體資料
        /// </summary>
        /// <param name="inputData">同IndexerOperationModeChangeReport(Trx inputData)</param>
        /// <param name="log">記錄動作者</param>
        public void IndexerOperationModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string log;
                string eqpNo = inputData.Metadata.NodeNo;

                Handle_IndexerOperationMode(inputData, out log);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] {3}.", eqpNo, sourceMethod, inputData.TrackKey, log));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void Handle_IndexerOperationMode(Trx inputData, out string log)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                eINDEXER_OPERATION_MODE value = (eINDEXER_OPERATION_MODE)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                eINDEXER_OPERATION_MODE oldvalue = line.File.IndexOperMode;

                log = string.Format("CIM_MODE=[{0}] INDEXER_OPERATION_MODE=[{1}]({2})", eqp.File.CIMMode, (int)value, value.ToString());

                lock (line)
                {
                    /*
                    if (value == eINDEXER_OPERATION_MODE.SAMPLING_MODE && (line.Data.LINETYPE == eLineType.ARRAY.CVD_AKT || line.Data.LINETYPE == eLineType.ARRAY.CVD_ULVAC))
                        line.File.IndexOperMode = eINDEXER_OPERATION_MODE.MIX_MODE; 
                    else
                        line.File.IndexOperMode = value;
                    */
                    line.File.IndexOperMode = value;
                }
                ObjectManager.LineManager.EnqueueSave(line.File);
                ObjectManager.LineManager.RecordLineHistory(inputData.TrackKey, line);
                //lock (eqp.File) eqp.File.EquipmentRunMode = ConstantManager["IDXMODE_TO_RUNMODE"][((int)value).ToString()].Value;
                //ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                Invoke(eServiceName.LineService, "CheckLineRunMode", new object[] { line.Data.LINEID });

                //切換成Cool Run Mode時, 只能在Offline的模式下
                if (value == eINDEXER_OPERATION_MODE.COOL_RUN_MODE)
                {
                    if (line.File.HostMode != eHostMode.OFFLINE)
                    {
                        lock (line.File) line.File.HostMode = eHostMode.OFFLINE;
                        ObjectManager.LineManager.EnqueueSave(line.File);

                        string tmp = "HOST MODE TURN \"OFFLINE\", BECAUSE INDEXER OPERATION MODE IS COOL RUN MODE.";
                        Logger.LogInfoWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", tmp);
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] {
                            inputData.TrackKey, line.Data.LINEID, tmp});
                        Invoke(eServiceName.MESService, "MachineControlStateChanged", new object[] { inputData.TrackKey, line.Data.LINEID, "OFFLINE" });
                    }
                }
                else if (oldvalue != value)
                {
                    ReportMES_MachineModeChange(inputData.TrackKey, line, (int)oldvalue);

                    Thread.Sleep(1000);
                    if (oldvalue == eINDEXER_OPERATION_MODE.CHANGER_MODE || value == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                    {
                        if (line.File.HostMode != eHostMode.OFFLINE)
                        {
                            if (value == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                                Invoke(eServiceName.MESService, "IndexerOperModeChanged", new object[] { UtilityMethod.GetAgentTrackKey(), line.Data.LINEID, "GLASS_CHANGER" });
                            else
                                Invoke(eServiceName.MESService, "IndexerOperModeChanged", new object[] { UtilityMethod.GetAgentTrackKey(), line.Data.LINEID, "NORMAL" });
                        }
                    }

                    #region 任何模式切成 Force Clean out 模式, 要將Force Clean Out Bit Turn On
                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                    {
                        IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                        string localNo = string.Empty;

                        foreach (Equipment e in eqps)
                        {
                            string trxName = string.Format("{0}_ForceCleanOutCommand", e.Data.NODENO);
                            Trx _trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;

                            if (line.Data.LINETYPE == eLineType.ARRAY.PHL_TITLE ||
                                line.Data.LINETYPE == eLineType.ARRAY.PHL_EDGEEXP)
                            {
                                bool normal = false, abnormal = false;

                                if (_trx != null && _trx.EventGroups[0].Events[0].Items[0].Value == "0") normal = true;

                                string trxName2 = string.Format("{0}_AbnormalForceCleanOutCommand", e.Data.NODENO);
                                Trx _trx2 = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName2, false }) as Trx;

                                if (_trx2 != null && _trx2.EventGroups[0].Events[0].Items[0].Value == "0") abnormal = true;

                                if (normal && abnormal)
                                {
                                    if (!string.IsNullOrEmpty(localNo)) localNo += ",";
                                    localNo += e.Data.NODENO;
                                }
                            }
                            else
                            {
                                if (_trx != null && _trx.EventGroups[0].Events[0].Items[0].Value == "0")
                                {
                                    _trx.EventGroups[0].Events[0].Items[0].Value = "1";
                                    _trx.TrackKey = inputData.TrackKey;
                                    SendPLCData(_trx);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(localNo))
                        {
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, 
                                string.Format("INDEXER CHANGE FORCE CLEAN OUT MODE, PLEASE MANUALLY OPERATED \"Normal/Abnormal Force Clean out command\" IN LINE CONTROL.") });
                        }
                    }
                    #endregion

                    #region 從Force Clean out 切成其他Mode時, 要將下給機台的Force Clean Out bit off掉
                    if (oldvalue == eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE && line.Data.FABTYPE == eFabType.ARRAY.ToString())
                    {
                        CancelForceCleanOut(line, inputData.TrackKey);
                    }
                    #endregion
                }

                //not changer mode, clear changer plan cc.kuang 2015/09/21
                if (oldvalue != value)
                {
                    ObjectManager.PlanManager.RemoveChangePlan();
                    ObjectManager.PlanManager.RemoveChangePlanStandby();
                    line.File.CurrentPlanID = string.Empty;
                    line.File.PlanStatus = ePLAN_STATUS.NO_PLAN;
                    ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                }

                #region OPI Service Send
                Invoke(eServiceName.UIService, "LineStatusReport", new object[] { inputData.TrackKey, line });
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void IndexerOperationModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_IndexerOperationModeChangeReportReply") as Trx;
                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    if (_timerManager.IsAliveTimer(eqpNo + "_" + IndexerOperationModeReportReplyTimeOut))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + IndexerOperationModeReportReplyTimeOut);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(eqpNo + "_" + IndexerOperationModeReportReplyTimeOut, false,
                            ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(IndexerOperationModeChangeReportReplyTimeOut), trackKey);
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

        private void IndexerOperationModeChangeReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] INDEXER OPERATION MODE CHANGE REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                IndexerOperationModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        private void CancelForceCleanOut(Line line, string trackKey)
        {
            try
            {
                IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                foreach (Equipment e in eqps)
                {
                    string trxName = string.Format("{0}_ForceCleanOutCommand", e.Data.NODENO);
                    Trx _trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                    bool dns = false;
                    if (_trx != null && _trx.EventGroups[0].Events[0].Items[0].Value == "1")
                    {
                        _trx.EventGroups[0].Events[0].Items[0].Value = "0";
                        _trx.TrackKey = trackKey;
                        SendPLCData(_trx);

                        if (e.Data.NODEATTRIBUTE.Equals("DNS")) dns = true;
                    }

                    if (line.Data.LINETYPE == eLineType.ARRAY.PHL_TITLE ||
                        line.Data.LINETYPE == eLineType.ARRAY.PHL_EDGEEXP)
                    {

                        string trxName2 = string.Format("{0}_AbnormalForceCleanOutCommand", e.Data.NODENO);
                        Trx _trx2 = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName2, false }) as Trx;

                        if (_trx2 != null && _trx2.EventGroups[0].Events[0].Items[0].Value == "1")
                        {
                            _trx2.EventGroups[0].Events[0].Items[0].Value = "0";
                            _trx2.TrackKey = trackKey;
                            SendPLCData(_trx2);

                            if (e.Data.NODEATTRIBUTE.Equals("DNS")) dns = true;
                        }
                    }

                    if (dns)
                    {
                        Invoke(eServiceName.CSOTSECSService, "TS2F111_H_ForcedCleanOutCommandSend",
                            new object[] {
								e.Data.NODENO, //eqpno
								e.Data.NODEID, //eqpid
								e.Data.NODEID, //eqptid
                                //cleanout //0: Forced Clean Out Reset.,1: Normal Forced Clean Out Set.,2: Abnormal Forced Clean Out Set.
								"0", 
								string.Empty, //tag
                                trackKey //trxid
								});
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region Indexer Operation Mode Change Command

        public void IndexerOperationModeChangeCommand(string trxid, string currentEqpNo, string IndexerOperationMode)
        {
            try
            {
                string err = string.Empty;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(currentEqpNo);
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, currentEqpNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxid, eqp.Data.LINEID, err });
                    throw new Exception(err);
                }

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT CAHNGE INDEXER OPERATION MODE!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxid, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                eINDEXER_OPERATION_MODE mode = (eINDEXER_OPERATION_MODE)int.Parse(IndexerOperationMode);

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null)
                {
                    err = string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE2, MethodBase.GetCurrentMethod().Name, eqp.Data.LINEID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxid, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                List<Port> ports = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID);
                if (line.File.IndexOperMode == mode)
                {
                    err = string.Format("The current Indexer Operation mode is {0}, can not be repeated switching!", mode);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxid, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }         
                else//add by hujunpeng 20190408 for DRY change operation mode from force clean out mode to mix mode or sampling mode
                {
                    if(line.Data.LINEID.Contains("TCDRY"))
                    {
                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                    {
                        if (mode == eINDEXER_OPERATION_MODE.SAMPLING_MODE || mode == eINDEXER_OPERATION_MODE.MIX_MODE)
                        {
                            foreach (Port port in ports)
                            {
                                if (port.File.Status == ePortStatus.LC)
                                {
                                    err = string.Format("The current Indexer Operation mode is {0}, The current port[{1}] is LC, Can not switch the mode to[{2}], Please cancel the cassette first!", line.File.IndexOperMode,port.Data.PORTNO,mode);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxid, eqp.Data.LINEID, err });
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                    return;
                                }
                            }
                        }
                    }
                    }
                }
                Trx trx = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format("{0}_IndexerOperationModeChangeCommand", eqp.Data.NODENO)) as Trx;
                if (trx != null)
                {
                    if (mode == eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                    {
                        // 寫在機台發生變化
                    }
                    else if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                    {
                        // Indexer 切成正常模式時, 必須先把Force Clean Out Bit Off
                        CancelForceCleanOut(line, trxid);
                    }

                    trx.EventGroups[0].Events[0].Items[ePLC.IndexerOperationMode].Value = IndexerOperationMode;
                    trx.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    trx.EventGroups[0].Events[1].Items[0].Value = "1";
                    trx.TrackKey = trxid;
                    SendPLCData(trx);
                    string timerId = string.Format("{0}_{1}", eqp.Data.NODENO, IndexerOperationModeCommandTimeOut);
                    if (_timerManager.IsAliveTimer(timerId))
                    {
                        _timerManager.TerminateTimer(timerId);
                    }
                    _timerManager.CreateTimer(timerId, false, ParameterManager["T1"].GetInteger(), new ElapsedEventHandler(IndexerOperationModeChangeCommandTimeOut), trxid);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] INDEXER_OPERATION_MODE=[{2}], SET BIT=[ON]",
                        eqp.Data.NODENO, trx.TrackKey, IndexerOperationMode));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void IndexerOperationModeChangeCommandTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string timeName = string.Format("{0}_{1}", sArray[0], IndexerOperationModeCommandTimeOut);
                //终止Timer
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_IndexerOperationModeChangeCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] INDEXER OPERATION MODE CHANGE COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void IndexerOperationModeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                    eqpNo, inputData.TrackKey, triggerBit.ToString(), (int)returnCode, returnCode.ToString()));

                //终止Timer
                string timeName = string.Format("{0}_{1}", eqpNo, IndexerOperationModeCommandTimeOut);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;


                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_IndexerOperationModeChangeCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;

                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, GetType().Name, "IndexerOperationModeChangeCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] SET BIT=[OFF].", eqpNo, inputData.TrackKey));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                        string.Format("EQUIPMENT=[{0}] INDEXER OPERATION MODE CHANGE COMMAND REPLY \"{1}\"!", eqp.Data.NODENO, returnCode)});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Equipment Run Mode Change Report

        public void EquipmentRunModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);
                string mode = inputData[0][0][0].Value;

                #region[取得EQP]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                    throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null)
                    throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion

                string oldrunmode = eqp.File.EquipmentRunMode;
                // mode是報給MES值, description是解釋
                string runmode = GetRunMode(line, eqpNo, mode, out description);

                if (line.Data.FABTYPE == eFabType.ARRAY.ToString())
                {
                    // Array 部份由PLC觸發的事件, 沒有報給MES, 所以填Description給OPI顯示用
                    lock (eqp) eqp.File.EquipmentRunMode = description;
                }
                else
                {
                    lock (eqp) eqp.File.EquipmentRunMode = runmode;
                    Invoke(eServiceName.LineService, "CheckLineRunMode", new object[] { line.Data.LINEID });
                    if (eqp.File.EquipmentRunMode != oldrunmode)
                    {
                        if (line.Data.FABTYPE == eFabType.CELL.ToString())
                        {
                            GetRunModeForCell(line, eqp);
                        }
                        ReportMES_MachineModeChange(inputData.TrackKey, line);
                    }
                    else if (line.Data.FABTYPE == eFabType.CELL.ToString())//CELL Special ,防止 DB資料與FILE 不同步，IsInitTrigger 同步
                    {
                        //GetRunModeForCellInit(line, eqp);
                        //20171221 该方法有问题 huangjiayin 都会重置成Normal
                        //改成调用原方法，但不上报MES
                        GetRunModeForCell(line, eqp);
                    }
                }
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] EQUIPMENT_RUN_MODE=[{3}]({4}).",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, mode, description));

                #region CF UPK Line
                //2015/8/25 Modify by Frank CF UPK Line 只有LD報Run Mode Change才需上報給MES 
                if (eqp.Data.NODEATTRIBUTE == "LD" && line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1)
                {
                    lock (line) line.File.UPKEquipmentRunMode = (eUPKEquipmentRunMode)int.Parse(mode);
                    ObjectManager.LineManager.EnqueueSave(line.File);
                    Thread.Sleep(1000);
                    Invoke(eServiceName.MESService, "MES_SwitchServerName", new object[] { ObjectManager.LineManager.GetLineID(line.Data.LINEID) });
                    Thread.Sleep(1500);
                    Invoke(eServiceName.MESService, "MachineDataUpdate", new object[] { inputData.TrackKey, line });
                    Thread.Sleep(1500);
                    Invoke(eServiceName.MESService, "PortDataUpdate", new object[] { inputData.TrackKey, line });
                }
                #endregion

                #region OPI Service Send
                if (inputData.IsInitTrigger) return;
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                Invoke(eServiceName.UIService, "LineStatusReport", new object[] { inputData.TrackKey, line });
                #endregion
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void EquipmentRunModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    EquipmentRunModeChangeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);
                string mode = inputData[0][0][0].Value;

                if (triggerBit == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
                    EquipmentRunModeChangeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                string oldrunmode = eqp.File.EquipmentRunMode;
                // mode是報給MES值, description是解釋
                string runmode = GetRunMode(line, eqpNo, mode, out description);

                if (line.Data.FABTYPE == eFabType.ARRAY.ToString())
                {
                    // Array 部份由PLC觸發的事件, 沒有報給MES, 所以填Description給OPI顯示用
                    lock (eqp) eqp.File.EquipmentRunMode = description;
                    Invoke(eServiceName.LineService, "CheckLineRunMode", new object[] { line.Data.LINEID }); 
                    //t3 MSP/ITO use for report chamber mode. 2015/09/29 cc.kuang
                    if (line != null && eqp.Data.NODENO.Equals("L4") && (line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC))
                    {
                        if (line.File.HostMode != eHostMode.OFFLINE)
                        {                            
                            if (eqp.File.EquipmentRunMode != oldrunmode)
                            {
                                ReportMES_MachineModeChange(inputData.TrackKey, line);
                            }
                            else
                            {
                                return;
                            }
                            Thread.Sleep(2000); // for wait machinemodechangereply then report chamber mode 2015/12/18 cc.kuang
                            List<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO).Where(u => u.Data.UNITTYPE == "CHAMBER").ToList();
                            if (units != null && units.Count > 1)
                            {
                                Equipment indexer = ObjectManager.EquipmentManager.GetEQP("L2");
                                ConstantItem item = null;
                                List<MesSpec.ChamberRunModeChanged.CHAMBERc> chamberList = new List<MesSpec.ChamberRunModeChanged.CHAMBERc>();
                                item = ConstantManager["ARRAY_CHAMBERMODE_PVD"][indexer.File.ProportionalRule01Type.ToString()];
                                MesSpec.ChamberRunModeChanged.CHAMBERc cb1 = new MesSpec.ChamberRunModeChanged.CHAMBERc();
                                cb1.CHAMBERNAME = units[0].Data.UNITID;
                                if (eqp.File.EquipmentRunMode.ToUpper() == "MIX")
                                    cb1.CHAMBERRUNMODE = item.Discription;
                                else
                                    cb1.CHAMBERRUNMODE = "";
                                item = ConstantManager["ARRAY_CHAMBERMODE_PVD"][indexer.File.ProportionalRule02Type.ToString()];
                                MesSpec.ChamberRunModeChanged.CHAMBERc cb2 = new MesSpec.ChamberRunModeChanged.CHAMBERc();
                                cb2.CHAMBERNAME = units[1].Data.UNITID;
                                if (eqp.File.EquipmentRunMode.ToUpper() == "MIX")
                                    cb2.CHAMBERRUNMODE = item.Discription;
                                else
                                    cb2.CHAMBERRUNMODE = "";
                                chamberList.Add(cb1);
                                chamberList.Add(cb2);
                                Invoke(eServiceName.MESService, "ChamberRunModeChanged",
                                    new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, line.File.LineOperMode, eqp.Data.NODEID, chamberList });
                            }
                        }
                    }

                    if (line != null && (eqp.Data.NODENO.Equals("L4") || eqp.Data.NODENO.Equals("L5")) && line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW)
                    {
                        Invoke(eServiceName.LineService, "CheckLineRunMode", new object[] { line.Data.LINEID });
                        if (eqp.File.EquipmentRunMode != oldrunmode)
                        {
                            if (line.File.HostMode != eHostMode.OFFLINE)
                                ReportMES_MachineModeChange(inputData.TrackKey, line);
                        }
                    }
                }
                else
                {
                    lock (eqp) eqp.File.EquipmentRunMode = runmode;
                    Invoke(eServiceName.LineService, "CheckLineRunMode", new object[] { line.Data.LINEID });
                    if (eqp.File.EquipmentRunMode != oldrunmode)
                    {
                        //T3當CF UPK的Loader改變EQPRunMode的時候, 已經觸發MachineDataUpdate報給MES, 所以不需要再上報MachineModeChange給MES, 20150918 jm.pan
                        if (eqp.Data.NODEATTRIBUTE != "UPK")
                        {
                            if (line.Data.FABTYPE == eFabType.CELL.ToString())
                            {
                                GetRunModeForCell(line, eqp);
                            }
                            ReportMES_MachineModeChange(inputData.TrackKey, line);
                        }
                    }
                }
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] EQUIPMENT_RUN_MODE=[{2}]({3}).",
                    eqpNo, inputData.TrackKey, mode, description));

                #region OPI Service Send
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                Invoke(eServiceName.UIService, "LineStatusReport", new object[] { inputData.TrackKey, line });
                #endregion

                EquipmentRunModeChangeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);

                #region CF UPK Line
                //2015/8/25 Modify by Frank CF UPK Line 只有LD報Run Mode Change才需上報給MES 
                if (eqp.Data.NODEATTRIBUTE == "UPK" && line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1)
                {
                    lock (line) line.File.UPKEquipmentRunMode = (eUPKEquipmentRunMode)int.Parse(mode);
                    ObjectManager.LineManager.EnqueueSave(line.File);
                    Thread.Sleep(1000);
                    Invoke(eServiceName.MESService, "MES_SwitchServerName", new object[] { ObjectManager.LineManager.GetLineID(line.Data.LINEID) });
                    Thread.Sleep(1500);
                    Invoke(eServiceName.MESService, "MachineDataUpdate", new object[] { inputData.TrackKey, line });
                    Thread.Sleep(1500);
                    Invoke(eServiceName.MESService, "PortDataUpdate", new object[] { inputData.TrackKey, line });
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    EquipmentRunModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }
        /// <summary>
        /// GetRunModeForCell CELL 都由EQP RUN MODE 觸發 集中 sy add 20160305
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="Line"></param>
        public void GetRunModeForCell(Line line, Equipment eqp)
        {
            if (eqp.File.EquipmentRunMode != "UNKNOW")
            {
                line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                switch (line.Data.LINETYPE)
                {
                    case eLineType.CELL.CCQUP:
                        if (eqp.File.EquipmentRunMode == "TYPEBMODE")
                        {
                            line.File.LineOperMode = eMES_LINEOPERMODE.CELL_BYPASS;
                            eqp.Data.VCRTYPE = "B";
                        }
                        else
                        {
                            line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                            eqp.Data.VCRTYPE = "A";
                        }
                        ObjectManager.EquipmentManager.UpdateDB(eqp.Data);
                        break;
                    case eLineType.CELL.CCCRP:
                    case eLineType.CELL.CCCRP_2:
                        if (eqp.File.EquipmentRunMode == "REPAIRMODE") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_REPAIR;
                        else line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                        break;
                    case eLineType.CELL.CCPTH:
                    case eLineType.CELL.CCTAM:
                    case eLineType.CELL.CCPDR:
                    case eLineType.CELL.CCRWT:
                        if (eqp.File.EquipmentRunMode == "SORTERMODE")
                        {
                            line.File.LineOperMode = eMES_LINEOPERMODE.CELL_MSORT; //20151209 MSORT閻波 不用上報20151212還是要報 MES
                            line.File.IndexOperMode = eINDEXER_OPERATION_MODE.SORTER_MODE;
                        }
                        else line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                        break;

                    case eLineType.CELL.CCGAP:
                        if (eqp.File.EquipmentRunMode == "GAPANDGMIMODE") line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                        else
                        {
                            line.File.LineOperMode = eMES_LINEOPERMODE.CELL_MSORT;
                            line.File.IndexOperMode = eINDEXER_OPERATION_MODE.SORTER_MODE;
                        }
                            break;
                    case eLineType.CELL.CCSOR:
                            if (eqp.File.EquipmentRunMode == "RANDOMMODEFORCHN") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_RANDOMMODECHN;//20160923 SORT 增加RANDOM MODE 上报 by zhuxingxing//20170112 sy modify  RANDOMMODE=>eMES_LINEOPERMODE.CELL_RANDOMMODE & RANDOMMODEFORCHN
                            else if (eqp.File.EquipmentRunMode == "RANDOMMODEFORSORT") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_RANDOMMODESOR;//20170112 sy modify SPEC目前沒有區分FOR SORT or CHN

                            else if (eqp.File.EquipmentRunMode == "VCRMODEFORCHN") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_VCRMODECHN;//20170112 sy modify SPEC目前沒有定義 & 沒有區分 SORT or CHN
                            else if (eqp.File.EquipmentRunMode == "VCRMODEFORSORT") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_VCRMODESOR;//20170112 sy modify SPEC目前沒有定義 & 沒有區分 SORT or CHN

                            else if (eqp.File.EquipmentRunMode == "CSTMIXTOTRAY") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_CSTMIXTOTRAY;//20170112 sy modify
                            else if (eqp.File.EquipmentRunMode == "CSTTOTRAYMIX") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_CSTTOTRAYMIX;//20170112 sy modify
                            else if (eqp.File.EquipmentRunMode == "CSTTOTRAYALL") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_CSTTOTRAYALL;//20170112 sy modify
                            else if (eqp.File.EquipmentRunMode == "CSTTOTRAYNG") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_SCRAP;//20170112 sy modify
                            else if (eqp.File.EquipmentRunMode == "TRAYMIXTOCST") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_TRAYMIXTOCST;//20170112 sy modify
                            else if (eqp.File.EquipmentRunMode == "TRAYTOCSTALL") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_TRAYTOCSTALL;//20170112 sy modify
                            else if (eqp.File.EquipmentRunMode == "FLAGGRADE") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_FLAGGRADE;//201703024 luojun modify

                            else if (eqp.File.EquipmentRunMode == "GRADEMODE") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_GRADE;//20170112 sy modify 原本測試要求不用上報但是SPEC 有先加上並 MARK
                            else if (eqp.File.EquipmentRunMode == "FLAGMODEMODE") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_FLAG;//20170112 sy modify 原本測試要求不用上報但是SPEC 有先加上並 MARK
                            
                            else line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                        break;
                    case eLineType.CELL.CCCHN:
                        if (eqp.File.EquipmentRunMode == "RANDOMMODEFORCHN") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_RANDOMMODECHN;     //add by zhuxingxing RANDOMMODE T3 20160923 //20170112 sy modify RANDOMMODE=> RANDOMMODEFORCHN

                        else if (eqp.File.EquipmentRunMode == "VCRMODEFORCHN") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_VCRMODECHN;//20170112 sy modify SPEC目前沒有定義 & 沒有區分 SORT or CHN

                        else if (eqp.File.EquipmentRunMode == "CSTMIXTOTRAY") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_CSTMIXTOTRAY;
                        else if (eqp.File.EquipmentRunMode == "CSTTOTRAYMIX") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_CSTTOTRAYMIX;
                        else if (eqp.File.EquipmentRunMode == "CSTTOTRAYALL") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_CSTTOTRAYALL;
                        else if (eqp.File.EquipmentRunMode == "CSTTOTRAYNG") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_SCRAP;
                        else if (eqp.File.EquipmentRunMode == "TRAYMIXTOCST") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_TRAYMIXTOCST;
                        else if (eqp.File.EquipmentRunMode == "TRAYTOCSTALL") line.File.LineOperMode = eMES_LINEOPERMODE.CELL_TRAYTOCSTALL;

                        else line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                        break;
                    case eLineType.CELL.CCPCK:
                        if (eqp.File.EquipmentRunMode == "CHANGER")
                        {
                            line.File.LineOperMode = eMES_LINEOPERMODE.CELL_CHANGER; //20160614 sy
                        }
                        else line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// GetRunModeForCellInit 更新程式 DB 與FILE 資料不同 在重啟BC 先同步 sy add 20160305
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="Line"></param>
        public void GetRunModeForCellInit(Line line, Equipment eqp)
        {
            if (eqp.File.EquipmentRunMode != "UNKNOW")
            {

               line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                switch (line.Data.LINETYPE)
                {
                    case eLineType.CELL.CCQUP:
                        if (eqp.File.EquipmentRunMode == "TYPEBMODE")
                        {
                            line.File.LineOperMode = eMES_LINEOPERMODE.CELL_BYPASS;
                            eqp.Data.VCRTYPE = "B";
                        }
                        else
                        {
                            line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                            eqp.Data.VCRTYPE = "A";
                        }
                        ObjectManager.EquipmentManager.UpdateDB(eqp.Data);
                        break;

                    default:
                        break;
                }
            }
        }
        public void EquipmentRunModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_EquipmentRunModeChangeReportReply") as Trx;
                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    if (_timerManager.IsAliveTimer(eqpNo + "_" + EquipmentRunModeReportReplyTimeOut))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + EquipmentRunModeReportReplyTimeOut);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(eqpNo + "_" + EquipmentRunModeReportReplyTimeOut, false,
                            ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(EquipmentRunModeChangeReportReplyTimeOut), trackKey);
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

        private void EquipmentRunModeChangeReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENT RUN MODE CHANGE REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                EquipmentRunModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private string GetRunMode(Line line, string eqpNo, string value, out string description)
        {
            description = string.Empty;
            ConstantItem item = null;
            if (line.Data.FABTYPE != eFabType.CELL.ToString())// shihyang add 20151023
            {
                switch (line.Data.LINETYPE)
                {
                    #region[ARRAY Rum Mode]
                    case eLineType.ARRAY.CVD_AKT:
                    case eLineType.ARRAY.CVD_ULVAC: item = ConstantManager["ARRAY_RUNMODE_CVD"][value]; break;
                    case eLineType.ARRAY.DRY_ICD:
                    case eLineType.ARRAY.DRY_TEL:
                    case eLineType.ARRAY.DRY_YAC: item = ConstantManager["ARRAY_RUNMODE_DRY"][value]; break;
                    case eLineType.ARRAY.OVNITO_CSUN:
                    case eLineType.ARRAY.OVNPL_YAC:
                    case eLineType.ARRAY.OVNSD_VIATRON: item = ConstantManager["ARRAY_RUNMODE_OVN"][value]; break;
                    case eLineType.ARRAY.TTP_VTEC: item = ConstantManager["ARRAY_RUNMODE_TPE"][value]; break;
                    case eLineType.ARRAY.MAC_CONTREL: item = ConstantManager["ARRAY_RUNMODE_MAC"][value]; break;
                    case eLineType.ARRAY.MSP_ULVAC:
                    case eLineType.ARRAY.ITO_ULVAC: item = ConstantManager["ARRAY_RUNMODE_PVD"][value]; break;
                    case eLineType.ARRAY.ELA_JSW:
                        {
                            if (eqpNo.Equals("L3"))
                                item = ConstantManager["ARRAY_BACKUPMODE_ELA"][value];
                            else 
                            item = ConstantManager["ARRAY_RUNMODE_ELA"][value];
                            break; 
                        }
                    case eLineType.ARRAY.BFG_SHUZTUNG: item = ConstantManager["ARRAY_RUNMODE_BFG"][value]; break;
                    #endregion
                    #region[CF Rum Mode]
                    case eLineType.CF.FCUPK_TYPE1:
                        {
                            if (eqpNo == "L2")
                                item = ConstantManager["CF_RUNMODE_UPK"][value];
                            else if (eqpNo == "L5")
                                item = ConstantManager["CF_RUNMODE_UPKULD"][value];
                            break;
                        }
                    case eLineType.CF.FCMAC_TYPE1: item = ConstantManager["CF_RUNMODE_MAC"][value]; break;
                    case eLineType.CF.FCREP_TYPE1: item = ConstantManager["CF_RUNMODE_REP"][value]; break;
                    case eLineType.CF.FCREP_TYPE2: item = ConstantManager["CF_RUNMODE_REP"][value]; break;
                    case eLineType.CF.FCREP_TYPE3: item = ConstantManager["CF_RUNMODE_REP"][value]; break;
                    case eLineType.CF.FCMQC_TYPE1: item = ConstantManager["CF_RUNMODE_MQC_TTP"][value]; break;
                    #endregion
                    default: item = new ConstantItem(); break;
                }
            }
            else
            {
                #region[CELL Rum Mode]
                item = ConstantManager["CELL_RUNMODE_" + line.Data.LINETYPE][value];
                #endregion
            }
            description = item.Discription;
            return item.Value;
        }
        #endregion

        #region Equipment Run Mode Set Command
        public void EquipmentRunModeSetCommand(string trxid, string currentEqpNo, string EquipemntRunMode)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(currentEqpNo);
                Trx trx = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format("{0}_EquipmentRunModeSetCommand", eqp.Data.NODENO)) as Trx;
                if (trx != null)
                {
                    trx.EventGroups[0].Events[0].Items[0].Value = EquipemntRunMode;
                    trx.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    trx.EventGroups[0].Events[1].Items[0].Value = "1";
                    trx.TrackKey = trxid;
                    SendPLCData(trx);
                    string timerId = string.Format("{0}_{1}", eqp.Data.NODENO, EquipmentRunModeCommandTimeOut);
                    if (_timerManager.IsAliveTimer(timerId))
                    {
                        _timerManager.TerminateTimer(timerId);
                    }
                    _timerManager.CreateTimer(timerId, false, ParameterManager["T1"].GetInteger(),
                        new ElapsedEventHandler(EquipmentRunModeSetCommandTimeOut), trxid);
                    string description = string.Empty;

                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    if (line != null)
                    {
                        GetRunMode(line, currentEqpNo, EquipemntRunMode, out description);
                    }

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENT_RUN_MODE=[{2}]({3}), SET BIT=[ON].",
                        eqp.Data.NODENO, trx.TrackKey, EquipemntRunMode, description));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void EquipmentRunModeSetCommandTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string timeName = string.Format("{0}_{1}", sArray[0], EquipmentRunModeCommandTimeOut);
                //终止Timer
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_EquipmentRunModeSetCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQUIPMENT RUN MODE SET COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("Equipment Run Mode Set Command Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void EquipmentRunModeSetCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                   eqpNo, inputData.TrackKey, triggerBit.ToString(), (int)returnCode, returnCode.ToString()));

                //终止Timer
                string timeName = string.Format("{0}_{1}", eqpNo, EquipmentRunModeCommandTimeOut);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                if (triggerBit == eBitResult.OFF) return;

                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_EquipmentRunModeSetCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;

                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, GetType().Name, "EquipmentRunModeSetCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].",
                    eqpNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region[Line Backup Mode Change Command, add by yang for OPI LineBackupModeChangeRequest,2016/12/27]
        public void LineBackupModeChangeCommand(string trxID, string eqpno, string set)
        {
            try
            {
                Trx outputData = null;
                #region [取得EQP資訊]
                Equipment cln = ObjectManager.EquipmentManager.GetEQP(eqpno);
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2"); //send to L2
                if (cln == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpno));
                #endregion

                string trxName = eqp.Data.NODENO + "_LineBackupModeChangeCommand";
                outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                if (outputData != null)
                {
                    outputData.EventGroups[0].Events[0].Items[0].Value = set.Equals("1") ? "2" : "1";
                    outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger(); //delay time
                    outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                    outputData.TrackKey = trxID;
                    SendPLCData(outputData);
                   
                    string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, LineBackupModeChangeTimeout);

                    if (_timerManager.IsAliveTimer(timeName))
                    {
                        _timerManager.TerminateTimer(timeName);
                    }
                    _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                   new System.Timers.ElapsedEventHandler(LineBackupModeChangeReplyTimeout), outputData.TrackKey);

                    string description = string.Empty;
                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    if (line != null)
                    {
                        GetRunMode(line, eqpno, set, out description);
                    }
                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[EQUIPMENT={1}] [BCS -> EQP][{0}], Backup Mode ({2}) Request SET BIT=[ON].",
                           outputData.TrackKey, outputData.Metadata.NodeNo, set.Equals("1") ? "Set" : "Reset"));

                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void LineBackupModeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eLineBackupReturnCode retCode = (eLineBackupReturnCode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }
                #region [取得EQP資訊]
                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, LineBackupModeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                string trxName = string.Format("{0}_LineBackupModeChangeCommand", inputData.Metadata.NodeNo);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.EventGroups[0].Events[0].IsDisable = true;
                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputData.TrackKey = inputData.TrackKey;
                SendPLCData(outputData);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[EQUIPMENT={1}] [BCS -> EQP][{0}], Backup Mode Request SET BIT=[OFF].",
                           outputData.TrackKey, outputData.Metadata.NodeNo));
                //  Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Line Backup Mode Change Command Result:({2})]",
                //     eqp.Data.NODENO, inputData.TrackKey, ReturnCode.Equals("1")?"OK":"NG"));
                
               Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,string.Format("EQUIPMENT=[{0}] Indexer Reply Line Backup Mode Change Command Return Code \"{1}\",Return Message{2}!",
                                    eqp.Data.NODENO,(int)retCode,retCode.ToString().Trim().Replace('_',' '))});
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void LineBackupModeChangeReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], LineBackupModeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_LineBackupModeChangeCommand", sArray[0]);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.EventGroups[0].Events[0].IsDisable = true;
                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputData.TrackKey = trackKey;
                SendPLCData(outputData);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Line Backup Mode Change Command Reply TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID,
                        string.Format("EQUIPMENT=[{0}] Indexer Reply Line Backup Mode Change Command TimeOut! Backup Change Request NG!", eqp.Data.NODENO)});
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Glass Info Change Report
        // Total Pitch for Daily Check Use
        public void GlassInfoChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    GlassInfoChangeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                string OldCstSeq = inputData.EventGroups[0].Events[0].Items[0].Value;
                string OldJobSeq = inputData.EventGroups[0].Events[0].Items[1].Value;
                string NewCstSeq = inputData.EventGroups[0].Events[0].Items[2].Value;
                string NewJobSeq = inputData.EventGroups[0].Events[0].Items[3].Value;
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] OLD_CST_SEQNO=[{3}] OLD_JOB_SEQNO=[{4}] NEW_CST_SEQNO=[{5}] NEW_JOB_SEQNO=[{6}].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, eqp.File.CIMMode, OldCstSeq, OldJobSeq, NewCstSeq, NewJobSeq));

                int icstNo = 0;
                switch (line.Data.FABTYPE)
                {
                    case "ARRAY":
                        int.TryParse(NewCstSeq, out icstNo);
                        if (icstNo <= 5000 || icstNo > 6000)
                        {
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("EQUIPMENT=[{0}] CASSETTE SEQUENCE NO. SHOULD BE \"5001~6000\".", eqpNo));
                            GlassInfoChangeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                            return;
                        }
                        break;
                    case "CF":
                        int.TryParse(NewCstSeq, out icstNo);
                        if (icstNo < 60000 || icstNo > 61000)
                        {
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("EQUIPMENT=[{0}] CASSETTE SEQUENCE NO. SHOULD BE \"60000~61000\".", eqpNo));
                            GlassInfoChangeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                            return;
                        }
                        break;
                }

                Job job = ObjectManager.JobManager.GetJob(OldCstSeq, OldJobSeq);
                if (job != null)
                {
                    ObjectManager.JobManager.DeleteJob(job);
                    job.WriteFlag = true; //for save the job file after delete job 2016/06/01 cc.kuang
                    job.CassetteSequenceNo = NewCstSeq;
                    job.JobSequenceNo = NewJobSeq;
                    job.SetNewJobKey(int.Parse(NewCstSeq), int.Parse(NewJobSeq)); //for Robot Control get this job 2015/12/16 cc.kuang
                    ObjectManager.JobManager.AddJob(job);
                    ObjectManager.JobManager.EnqueueSave(job);
                }
                GlassInfoChangeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    GlassInfoChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        public void GlassInfoChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_GlassInfoChangeReportReply") as Trx;
                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    if (_timerManager.IsAliveTimer(eqpNo + "_" + GlassInfoReportReplyTimeOut))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + GlassInfoReportReplyTimeOut);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(eqpNo + "_" + GlassInfoReportReplyTimeOut, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(GlassInfoChangeReportReplyTimeOut), trackKey);
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

        private void GlassInfoChangeReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] GLASS INFORMATION CHANGE REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                GlassInfoChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Inspection Idle Time Setting Report]
        public void InspectionIdleTimeSettingReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);
                string mode = inputData[0][0][0].Value;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                    throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                lock (eqp.File) eqp.File.InspectionIdleTime = int.Parse(inputData[0][0][0].Value);
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] INSPECTION_IDLE_TIME=[{3}].",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, eqp.File.InspectionIdleTime));
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void InspectionIdleTimeSettingReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    InspectionIdleTimeSettingReportUpdate(inputData, MethodBase.GetCurrentMethod() + "_Initial");
                    return;
                }

                #region [取得EQP]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null)
                    throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData[0][1][0].Value);
                #endregion
                string eqpNo = inputData.Metadata.NodeNo;
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", eqp.Data.NODENO, inputData.TrackKey));
                    InspectionIdleTimeSettingReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region[取得Inspection Idle Time]
                lock (eqp.File) eqp.File.InspectionIdleTime = int.Parse(inputData[0][0][0].Value);
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                #endregion

                //20150708 Add Frank InspectionTime=0時表示不啟用
                if (eqp.File.InspectionIdleTime == 0)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] INSPECTION_IDLE_TIME=[{2}]. (IDLE_TIME_NO_USE)",
                         eqp.Data.NODENO, inputData.TrackKey, eqp.File.InspectionIdleTime));
                }
                else
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] INSPECTION_IDLE_TIME=[{2}].",
                        eqp.Data.NODENO, inputData.TrackKey, eqp.File.InspectionIdleTime));
                }
                InspectionIdleTimeSettingReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData[0][1][0].Value.Equals("1"))
                {
                    InspectionIdleTimeSettingReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void InspectionIdleTimeSettingReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_InspectionIdleTimeSettingReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                if (outputdata != null)
                {
                    outputdata[0][0][0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, "InspectionIdleTimeSettingReportReplyTimeout");
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(InspectionIdleTimeSettingReportReplyTimeout), trackKey);
                    }

                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].", eqpNo, trackKey, value));
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void InspectionIdleTimeSettingReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] INSPECTION IDLE TIME SETTING REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                InspectionIdleTimeSettingReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Inspection Idle Time Setting Commond]

        public void InspectionIdleTimeSettingCommand(string trxid, string currentEqpNo, int inspectionIdleTime)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(currentEqpNo);
                string err = string.Empty;
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, currentEqpNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxid, eqp.Data.LINEID, err });
                    throw new Exception(err);
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND INSPECTION IDLE TIME SETTING COMMAND!",
                        MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxid, eqp.Data.LINEID, err });
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (inspectionIdleTime < 0 || inspectionIdleTime > 65535)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] INSPECTION_IDLE_TIME=[{2}] MUST BE BETWEEN \"0~65535\"!",
                        MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO, inspectionIdleTime);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxid, eqp.Data.LINEID, err });
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                Trx trx = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format("{0}_InspectionIdleTimeSettingCommand", eqp.Data.NODENO)) as Trx;
                if (trx != null)
                {
                    trx.EventGroups[0][0][0].Value = inspectionIdleTime.ToString();
                    trx.EventGroups[0][1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    trx.EventGroups[0][1][0].Value = ((int)eBitResult.ON).ToString();
                    trx.TrackKey = trxid;
                    SendPLCData(trx);
                    string timerId = string.Format("{0}_InspectionIdleTimeSettingCommandTimeOut", eqp.Data.NODENO);
                    if (Timermanager.IsAliveTimer(timerId))
                    {
                        Timermanager.TerminateTimer(timerId);
                    }
                    Timermanager.CreateTimer(timerId, false, ParameterManager["T1"].GetInteger(), new ElapsedEventHandler(InspectionIdleTimeSettingCommandTimeOut), trxid);

                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] INSPECTION_IDLE_TIME=[{2}], SET BIT=[ON].", eqp.Data.NODENO, trx.TrackKey, inspectionIdleTime.ToString()));
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void InspectionIdleTimeSettingCommandTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string timeName = string.Format("{0}_InspectionIdleTimeSettingCommandTimeOut", sArray[0]);
                //终止Timer
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_InspectionIdleTimeSettingCommand") as Trx;
                outputdata[0][0].IsDisable = true;
                outputdata[0][1][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] INSPECTION IDLE TIME SETTING COMMAND REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("Inspection Idle Time Setting Command Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void InspectionIdleTimeSettingCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult bitResult = (eBitResult)int.Parse(inputData[0][1][0].Value);
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData[0][0][0].Value);

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                   eqpNo, inputData.TrackKey, bitResult.ToString(), (int)returnCode, returnCode.ToString()));

                //终止Timer
                string timeName = string.Format("{0}_InspectionIdleTimeSettingCommandTimeOut", eqpNo);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                if (bitResult == eBitResult.OFF) return;
                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_InspectionIdleTimeSettingCommand") as Trx;
                outputdata[0][0].IsDisable = true;
                outputdata[0][1][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;

                SendPLCData(outputdata);

                LogInfo("InspectionIdleTimeSettingCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].", eqpNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Wait Cassette]
        public void WaitCassetteStatusReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                string mode = inputData.EventGroups[0].Events[0].Items[0].Value;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                eWaitCassetteStatus oldsts = eqp.File.WaitCassetteStatus;
                lock (eqp) eqp.File.WaitCassetteStatus = (eWaitCassetteStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] WAIT_CASSETTE_STATUS=[{3}]({4}).",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, (int)eqp.File.WaitCassetteStatus, eqp.File.WaitCassetteStatus.ToString()));

                if (oldsts != eqp.File.WaitCassetteStatus)
                {
                    #region [Wait Cassette Status Report]
                    if (eqp.File.WaitCassetteStatus == eWaitCassetteStatus.W_CST)
                    {
                        object[] obj = new object[]
                        {
                            inputData.TrackKey,                     /*0 TrackKey*/
                            eqp,                                  /*1 Equipment*/
                        };
                        Invoke(eServiceName.MESService, "MachineStateChanged_WaitCassetteState", obj);
                    }
                    else
                    {
                        Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format("{0}_EquipmentStatusChangeReport", eqp.Data.NODENO), true }) as Trx;
                        if (trx == null) return;

                        eEQPStatus eqpStatus = (eEQPStatus)int.Parse(trx.EventGroups[0].Events[0].Items[0].Value);
                        string eqpAlarmID = trx.EventGroups[0].Events[0].Items[1].Value;

                        string alarmText = string.Empty;
                        string alarmTime = string.Empty;
                        #region [取得目前正在發生的Alarm]
                        if (!eqpAlarmID.Equals("0"))
                        {
                            HappeningAlarm happenAlarm = ObjectManager.AlarmManager.GetAlarm(eqp.Data.NODENO, eqpAlarmID);
                            if (happenAlarm == null)
                            {
                                AlarmEntityData alarm = ObjectManager.AlarmManager.GetAlarmProfile(eqpNo, "0", eqpAlarmID);
                                if (alarm != null)
                                {
                                    alarmText = alarm.ALARMTEXT;
                                }
                                alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            }
                            else
                            {
                                alarmText = happenAlarm.Alarm.ALARMTEXT;
                                alarmTime = happenAlarm.OccurDateTime.ToString("yyyyMMddHHmmss");
                            }
                        }
                        #endregion

                        // MES Data
                        object[] obj = new object[]
                        {
                            inputData.TrackKey,                     /*0 TrackKey*/
                            eqp,                                  /*1 Equipment*/
                            eqpAlarmID.Equals("0") ? "" : eqpAlarmID, /*2 alarmID*/
                            alarmText,                             /*3 alarmText*/
                            alarmTime                              /*4 alarmTime*/
                        };
                        Invoke(eServiceName.MESService, "MachineStateChanged", obj);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void WaitCassetteStatusReportForRobot(string trackKey, string eqpNo, int waitCst)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                eWaitCassetteStatus oldsts = eqp.File.WaitCassetteStatus;

                lock (eqp) eqp.File.WaitCassetteStatus = (eWaitCassetteStatus)waitCst;

                if (oldsts != eqp.File.WaitCassetteStatus)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] WAIT_CASSETTE_STATUS=[{2}]({3}).",
                            eqp.Data.NODENO, trackKey, (int)eqp.File.WaitCassetteStatus, eqp.File.WaitCassetteStatus.ToString()));

                    if (eqp.File.WaitCassetteStatus == eWaitCassetteStatus.W_CST)
                    {
                        object[] obj = new object[]
                        {
                            trackKey,                             /*0 TrackKey*/
                            eqp,                                  /*1 Equipment*/
                        };
                        Invoke(eServiceName.MESService, "MachineStateChanged_WaitCassetteState", obj);

                        //Jun Add 20150227 因應國基哥要求, Send CIM Message給機台
                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), eqpNo, "ROBOT SEND WAIT CASSETTE MESSAGE", "", "0" });
                    }
                    else
                    {
                        Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format("{0}_EquipmentStatusChangeReport", eqp.Data.NODENO), true }) as Trx;
                        if (trx == null) return;

                        eEQPStatus eqpStatus = (eEQPStatus)int.Parse(trx.EventGroups[0].Events[0].Items[0].Value);
                        string eqpAlarmID = trx.EventGroups[0].Events[0].Items[1].Value;

                        string alarmText = string.Empty;
                        string alarmTime = string.Empty;
                        #region [取得目前正在發生的Alarm]
                        if (!eqpAlarmID.Equals("0"))
                        {
                            HappeningAlarm happenAlarm = ObjectManager.AlarmManager.GetAlarm(eqp.Data.NODENO, eqpAlarmID);
                            if (happenAlarm == null)
                            {
                                AlarmEntityData alarm = ObjectManager.AlarmManager.GetAlarmProfile(eqpNo, "0", eqpAlarmID);
                                if (alarm != null)
                                {
                                    alarmText = alarm.ALARMTEXT;
                                }
                                alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            }
                            else
                            {
                                alarmText = happenAlarm.Alarm.ALARMTEXT;
                                alarmTime = happenAlarm.OccurDateTime.ToString("yyyyMMddHHmmss");
                            }
                        }
                        #endregion

                        // MES Data
                        object[] obj = new object[]
                        {
                            trackKey,                               /*0 TrackKey*/
                            eqp,                                  /*1 Equipment*/
                            eqpAlarmID.Equals("0") ? "" : eqpAlarmID, /*2 alarmID*/
                            alarmText,                             /*3 alarmText*/
                            alarmTime                              /*4 alarmTime*/
                        };
                        Invoke(eServiceName.MESService, "MachineStateChanged", obj);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void WaitCassetteStatusReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    WaitCassetteStatusReportUpdate(inputData, MethodBase.GetCurrentMethod() + "_Initial");
                    return;
                }
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
                    WaitCassetteStatusReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                eWaitCassetteStatus oldsts = eqp.File.WaitCassetteStatus;

                lock (eqp) eqp.File.WaitCassetteStatus = (eWaitCassetteStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] WAIT_CASSETTE_STATUS=[{2}]({3}).",
                        eqp.Data.NODENO, inputData.TrackKey, (int)eqp.File.WaitCassetteStatus, eqp.File.WaitCassetteStatus.ToString()));

                WaitCassetteStatusReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);

                if (oldsts != eqp.File.WaitCassetteStatus)
                {
                    if (eqp.File.WaitCassetteStatus == eWaitCassetteStatus.W_CST)
                    {
                        object[] obj = new object[]
                        {
                            inputData.TrackKey,                     /*0 TrackKey*/
                            eqp,                                  /*1 Equipment*/
                        };
                        Invoke(eServiceName.MESService, "MachineStateChanged_WaitCassetteState", obj);

                        //20180619 by huangjiayin:FEOL特殊退卡逻辑
                        string[] _autoAbortEqps = { "CCPIL1AB", "CCPIL1HU", "CCPIL2HU", "CCODF1IU", "CCODF2IU" };
                        if (_autoAbortEqps.Contains(eqp.Data.NODEID))
                        {
                            if (ParameterManager.ContainsKey("FEOLAUTOABORTCST"))
                            {
                                if (ParameterManager["FEOLAUTOABORTCST"].GetBoolean())
                                {
                                    Invoke(eServiceName.EquipmentService, "HandleWaitCassetteStatus", obj);
                                }
                            }
                        }

                    }
                    else
                    {
                        Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { string.Format("{0}_EquipmentStatusChangeReport", eqp.Data.NODENO), true }) as Trx;
                        if (trx == null) return;

                        eEQPStatus eqpStatus = (eEQPStatus)int.Parse(trx.EventGroups[0].Events[0].Items[0].Value);
                        string eqpAlarmID = trx.EventGroups[0].Events[0].Items[1].Value;

                        string alarmText = string.Empty;
                        string alarmTime = string.Empty;
                        #region [取得目前正在發生的Alarm]
                        if (!eqpAlarmID.Equals("0"))
                        {
                            HappeningAlarm happenAlarm = ObjectManager.AlarmManager.GetAlarm(eqp.Data.NODENO, eqpAlarmID);
                            if (happenAlarm == null)
                            {
                                AlarmEntityData alarm = ObjectManager.AlarmManager.GetAlarmProfile(eqpNo, "0", eqpAlarmID);
                                if (alarm != null)
                                {
                                    alarmText = alarm.ALARMTEXT;
                                }
                                alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            }
                            else
                            {
                                alarmText = happenAlarm.Alarm.ALARMTEXT;
                                alarmTime = happenAlarm.OccurDateTime.ToString("yyyyMMddHHmmss");
                            }
                        }
                        #endregion

                        // MES Data
                        object[] obj = new object[]
                        {
                            inputData.TrackKey,                     /*0 TrackKey*/
                            eqp,                                  /*1 Equipment*/
                            eqpAlarmID.Equals("0") ? "" : eqpAlarmID, /*2 alarmID*/
                            alarmText,                             /*3 alarmText*/
                            alarmTime                              /*4 alarmTime*/
                        };
                        Invoke(eServiceName.MESService, "MachineStateChanged", obj);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void WaitCassetteStatusReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_WaitCassetteStatusReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + WaitCassetteStatusReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + WaitCassetteStatusReportTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + WaitCassetteStatusReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(WaitCassetteStatusReportReplyTimeout), trackKey);
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
        private void WaitCassetteStatusReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] WAIT CASSETTE STATUS REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));
                WaitCassetteStatusReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        //20180619 by huangjiayin
        #region PIL,ODF AutoAbortCST
        public void HandleWaitCassetteStatus(string trxId,Equipment eq)
        {
            try
            {
                Thread.Sleep(5000);
                Equipment _wEqp = ObjectManager.EquipmentManager.GetEQP(eq.Data.NODENO);
                //抓一次最新状态，如果W_CST解除，则不退卡
                if (_wEqp.File.WaitCassetteStatus == eWaitCassetteStatus.NotWaitCassette) return;
                Job _wJob = ObjectManager.JobManager.GetJob(_wEqp.File.FinalReceiveGlassID.Trim());
                //最新Log分析，卡片是先收在RB上Wait
                if (_wJob == null)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("CAN'T FIND JOB=[{0}] ,CST ABORT FAIL!", _wEqp.File.FinalReceiveGlassID.Trim()));
                    return;
                }
                //满足以下条件退Port
                //与JobType匹配的Port都是2，5状态，其他表示正在退，或者正在搬
                List<Port> _readyToAbort=null;
                if (_wEqp.Data.LINEID.Contains("CCPIL"))
                {
                    #region[PIL] by jobtype
                    if (_wJob.JobType == eJobType.TFT)
                    {
                        _readyToAbort = ObjectManager.PortManager.GetPorts(_wEqp.Data.NODEID).Where(p => p.File.Mode == ePortMode.TFT).ToList();
                    }
                    else if (_wJob.JobType == eJobType.CF)
                    {
                        _readyToAbort = ObjectManager.PortManager.GetPorts(_wEqp.Data.NODEID).Where(p => p.File.Mode == ePortMode.CF).ToList();
                    }
                    else
                    {
                        _readyToAbort = ObjectManager.PortManager.GetPorts(_wEqp.Data.NODEID).Where(p => p.File.Mode == ePortMode.Dummy).ToList();
                    }
                    #endregion
                }

                if (_wEqp.Data.LINEID.Contains("CCODF"))
                {
                    #region[ODF] by jobjudge
                    if (_wJob.JobJudge == "1")
                    {
                        _readyToAbort = ObjectManager.PortManager.GetPorts(_wEqp.Data.NODEID).Where(p => p.File.Mode == ePortMode.OK).ToList();
                    }
                    else
                    {
                        _readyToAbort = ObjectManager.PortManager.GetPorts(_wEqp.Data.NODEID).Where(p => p.File.Mode == ePortMode.NG).ToList();
                    }
                    #endregion
                }

                if (_readyToAbort == null || _readyToAbort.Count == 0)
                {
                    //无匹配Port，退了也没用
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("CAN'T FIND Match PORT, JOB TYPE=[{0}] ,CST ABORT FAIL!",_wJob.JobType.ToString()));
                }

                Port _targetPort;
                Port _alreadyPort=_readyToAbort.FirstOrDefault(p => p.File.CassetteStatus != eCassetteStatus.IN_PROCESSING);
                if (_alreadyPort != null)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("PortID=[{0}]  already in aborting or loading, not need abort!", _alreadyPort.Data.PORTID));
                    return;
                }
                _readyToAbort = _readyToAbort.Where(p => p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING).OrderBy(p => ObjectManager.CassetteManager.GetCassette(int.Parse(p.File.CassetteSequenceNo)).StartTime).ToList();
                _targetPort = _readyToAbort[0];

                string err = string.Format("Wait CST Occurs, Invoke Abort Command, PortID=[{0}], CSTID=[{1}].", _targetPort.Data.PORTID, _targetPort.File.CassetteID);
                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxId, eq.Data.LINEID, err });
                Invoke(eServiceName.CassetteService, "CassetteProcessAbort", new object[] { eq.Data.NODENO, _targetPort.Data.PORTNO });

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

 
        }


        #endregion


        #endregion

        #region [Buffer Warning]
        public void BufferWarningReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    BufferWarningReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                int bufferWarningGlassSettingCount = int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                int bufferCurrentGlassCount = int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value);
                int bufferWarning = int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value);
                int bufferStoreGlassOverAliveTime = int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value);

                lock (eqp)
                {
                    eqp.File.BufferWarningGlassSettingCount = bufferWarningGlassSettingCount;
                    eqp.File.BufferCurrentGlassCount = bufferCurrentGlassCount;
                    eqp.File.BufferWarning = bufferWarning;
                    eqp.File.BufferStoreGlassOverAliveTime = bufferStoreGlassOverAliveTime;
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] BIT=[ON] BUFFER_WARNING_GLASS_SETTING_COUNT=[{2}] BUFFER_CURRENT_GLASS_COUNT=[{3}] BUFFER_WARNING=[{4}] BUFFER_STORE_GLAS_OVER_ALIVE_TIME=[{5}].", eqp.Data.NODENO,
                        inputData.TrackKey,
                        bufferWarningGlassSettingCount,
                        bufferCurrentGlassCount,
                        bufferWarning,
                        bufferStoreGlassOverAliveTime));

                BufferWarningReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void BufferWarningReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_BufferWarningReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + BufferWarningReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + BufferWarningReportTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + BufferWarningReportTimeout, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(BufferWarningReportReplyTimeout), trackKey);
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
        private void BufferWarningReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BUFFER WARNING REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                BufferWarningReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion


        #region [Unit CIM Mode]
        public void UnitCIMMode(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                eBitResult m1 = (eBitResult)Int32.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                eBitResult m2 = (eBitResult)Int32.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eBitResult m3 = (eBitResult)Int32.Parse(inputData.EventGroups[0].Events[2].Items[0].Value);
                eBitResult m4 = (eBitResult)Int32.Parse(inputData.EventGroups[0].Events[3].Items[0].Value);
                eBitResult m5 = (eBitResult)Int32.Parse(inputData.EventGroups[0].Events[4].Items[0].Value);

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] UNIT_CIMMODE#01=[{2}] UNIT_CIMMODE#02=[{3}] UNIT_CIMMODE#03=[{4}] UNIT_CIMMODE#04=[{5}] UNIT_CIMMODE#05=[{6}].",
                    eqp.Data.NODENO, inputData.TrackKey, m1, m2, m3, m4, m5));

                lock (eqp)
                {
                    eqp.File.UnitCIMMode01 = m1;
                    eqp.File.UnitCIMMode02 = m2;
                    eqp.File.UnitCIMMode03 = m3;
                    eqp.File.UnitCIMMode04 = m4;
                    eqp.File.UnitCIMMode05 = m5;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        private void ReportMES_MachineModeChange(string trxID, Line line, int oldIndexOperMode = -1)
        {
            bool IsReport = false;
            if (oldIndexOperMode != -1)
            {
                eINDEXER_OPERATION_MODE oldMode = (eINDEXER_OPERATION_MODE)oldIndexOperMode;

                if (oldMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                {
                    if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.COOL_RUN_MODE &&
                    line.File.IndexOperMode != eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                    {
                        IsReport = true;
                    }
                }

                if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                {
                    if (oldMode != eINDEXER_OPERATION_MODE.COOL_RUN_MODE &&
                        oldMode != eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                    {
                        IsReport = true;
                    }
                }

                if (IsReport)
                {
                    Invoke(eServiceName.MESMessageService, "MachineModeChangeRequest",
                            new object[] { trxID, line.Data.LINEID });
                    return;
                }
            }
            if (line.Data.FABTYPE == eFabType.CELL.ToString())
            {
                Invoke(eServiceName.MESMessageService, "MachineModeChangeRequest", new object[] { trxID, line.Data.LINEID });
                return;
            }
            switch (line.Data.LINETYPE)
            {
                case eLineType.CF.FCUPK_TYPE1:
                case eLineType.CF.FCMAC_TYPE1://FCQMA100
                case eLineType.CF.FCPSH_TYPE1://FCQPS100
                case eLineType.CF.FCREP_TYPE1:
                case eLineType.CF.FCREP_TYPE2:
                case eLineType.CF.FCREP_TYPE3:
                case eLineType.CF.FCREW_TYPE1://FCWRW100
                case eLineType.CF.FCMQC_TYPE1:
                case eLineType.CF.FCMQC_TYPE2:
                case eLineType.CF.FCAOI_TYPE1://FCQAI100
                case eLineType.CF.FCSRT_TYPE1://FCSOR100
                case eLineType.CF.FCMSK_TYPE1://FCKCN100
                case eLineType.ARRAY.MSP_ULVAC: //add for t3 MES request, 2015/10/09 cc.kuang
                case eLineType.ARRAY.ITO_ULVAC:
                case eLineType.ARRAY.CVD_AKT:
                case eLineType.ARRAY.CVD_ULVAC:
                case eLineType.ARRAY.DRY_ICD:
                case eLineType.ARRAY.DRY_YAC:
                case eLineType.ARRAY.DRY_TEL:
                case eLineType.ARRAY.ELA_JSW:
                    Invoke(eServiceName.MESMessageService, "MachineModeChangeRequest",
                        new object[] { trxID, line.Data.LINEID });
                    break;
            }
        }


        #region Robot Fetch Sequence Mode
        #region Robot Fetch Sequence Mode Report
        public void RobotFetchSequenceModeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);
                eRobotFetchSequenceMode mode = (eRobotFetchSequenceMode)int.Parse(inputData[0][0][0].Value);

                #region[取得EQP]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                    throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                eqp.File.RobotFetchSequenceMode = mode;

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null)
                    throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                line.File.RobotFetchSeqMode = inputData[0][0][0].Value;

                #endregion

                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] ROBOT_FETCH_SEQUENCE_MODE=[{3}]({4}).",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, mode, description));

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void RobotFetchSequenceModeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    RobotFetchSequenceModeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                string eqpNo = inputData.Metadata.NodeNo;
                string description = string.Empty;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);
                eRobotFetchSequenceMode mode = (eRobotFetchSequenceMode)int.Parse(inputData[0][0][0].Value);

                if (triggerBit == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
                    RobotFetchSequenceModeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                eqp.File.RobotFetchSequenceMode = mode;

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null)
                    throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                line.File.RobotFetchSeqMode = inputData[0][0][0].Value;

                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] ROBOT_FETCH_SEQUENCE_MODE=[{2}]({3}).",
                    eqpNo, inputData.TrackKey, mode, description));


                RobotFetchSequenceModeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);


            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    RobotFetchSequenceModeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        public void RobotFetchSequenceModeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_RobotFetchSequenceModeReportReply") as Trx;
                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    if (_timerManager.IsAliveTimer(eqpNo + "_" + RobotFetchSequenceModeCommandTimeOut))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + RobotFetchSequenceModeCommandTimeOut);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(eqpNo + "_" + RobotFetchSequenceModeCommandTimeOut, false,
                            ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(RobotFetchSequenceModeReportReplyTimeOut), trackKey);
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

        private void RobotFetchSequenceModeReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ROBOT FETCH SEQUENCE MODE CHANGE REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                RobotFetchSequenceModeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region Robot Fetch Seqence Mode Command
        public void RobotFetchSequenceModeCommand(string trxid, string currentEqpNo, string RobotFetchSequenceMode)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(currentEqpNo);
                Trx trx = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format("{0}_RobotFetchSequenceModeCommand", eqp.Data.NODENO)) as Trx;
                if (trx != null)
                {
                    trx.EventGroups[0].Events[0].Items[0].Value = RobotFetchSequenceMode;
                    trx.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    trx.EventGroups[0].Events[1].Items[0].Value = "1";
                    trx.TrackKey = trxid;
                    SendPLCData(trx);
                    string timerId = string.Format("{0}_{1}", eqp.Data.NODENO, RobotFetchSequenceModeCommandTimeOut);
                    if (_timerManager.IsAliveTimer(timerId))
                    {
                        _timerManager.TerminateTimer(timerId);
                    }
                    _timerManager.CreateTimer(timerId, false, ParameterManager["T1"].GetInteger(),
                        new ElapsedEventHandler(RobotFetchSequenceModeCommandReplyTimeOut), trxid);
                    string description = string.Empty;


                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ROBOT_FETCH_SEQUENCE_MODE=[{2}]({3}), SET BIT=[ON].",
                        eqp.Data.NODENO, trx.TrackKey, RobotFetchSequenceMode, description));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void RobotFetchSequenceModeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                   eqpNo, inputData.TrackKey, triggerBit.ToString(), (int)returnCode, returnCode.ToString()));

                //终止Timer
                string timeName = string.Format("{0}_{1}", eqpNo, RobotFetchSequenceModeCommandTimeOut);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                if (triggerBit == eBitResult.OFF) return;

                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_RobotFetchSequenceModeCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;

                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, GetType().Name, "RobotFetchSequenceModeCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].",
                    eqpNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RobotFetchSequenceModeCommandReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string timeName = string.Format("{0}_{1}", sArray[0], RobotFetchSequenceModeCommandTimeOut);
                //终止Timer
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_RobotFetchSequenceModeCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ROBOT FETCH SEQUENCE MODE SET COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("Robot Fetch Sequence Mode Set Command Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #endregion
    }
}
