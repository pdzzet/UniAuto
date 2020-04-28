using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Reflection;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.Core.Message;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    class TrainingSample : AbstractService
    {
        private const string ProcessPauseCommandTimeout = "ProcessPauseCommandTimeout";
        private const string EQPOperationModeTimeout = "EQPOperationModeTimeout";


        // *****主动发送写入Sample*****
        public void ProcessPauseCommand(string transactionID, string eqpNo, string unitNo, string processPause)
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, eqpNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { transactionID, eqp.Data.LINEID, err });
                    throw new Exception(err);
                }
                
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND PROCESS PAUSE COMMAND!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { transactionID, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                // 1.取得Transaction Name
                string trxName = string.Format("{0}_ProcessPauseCommand", eqp.Data.NODENO);

                // 2.由PLCAgent取得Transaction对象
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                // 3.先将对象赋予初始化值
                outputdata.ClearTrxWith0();

                // 4.将数据填入对象
                // Word 值
                outputdata.EventGroups[0].Events[0].Items[0].Value = processPause;
                outputdata.EventGroups[0].Events[0].Items[1].Value = unitNo;
                // Bit 值
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                // 5.Write Word delay 200 ms then Write Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = transactionID;
                // 6. 送给PLCAgent写入
                xMessage msg = new xMessage();
                msg.Data = outputdata;
                msg.ToAgent = eAgentName.PLCAgent;
                PutMessage(msg);
                

                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, ProcessPauseCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ProcessPauseCommandReplyTimeout), outputdata.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] UNIT_NO=[{2}] PROCESS_PAUSE=[{3}], SET BIT=[ON].",
                        eqp.Data.NODENO, transactionID, unitNo, processPause));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ProcessPauseCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                if (retCode == eReturnCode1.NG)
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, inputData.Metadata.NodeNo, string.Format("{0} Process Pause Command Reply NG !", inputData.Metadata.NodeNo) });

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), (int)retCode, retCode.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, ProcessPauseCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;

                string trxName = string.Format("{0}_ProcessPauseCommand", inputData.Metadata.NodeNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Repository.Add(inputData.Name, inputData);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ProcessPauseCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], ProcessPauseCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PROCESS PAUSE COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_ProcessPauseCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("Process Pause Command Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        // *****被动触发写入Sample*****
        /// <summary>
        /// 更新記億體資料
        /// </summary>
        /// <param name="inputData">同CSTOperationModeChangeReport(Trx inputData)</param>
        /// <param name="log">記錄動作者</param>
        public void CSTOperationModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                #region [取得EQP]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion
                eCSTOperationMode cstOM;
                int value = int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                //目前EQP IO Report "1：Kind To Kind mode  2：Cassette To Cassette mode"
                //但是JOB DATA IO is " 0: Kind to Kind   1: CST to CST"
                if (value == 1)
                    cstOM = eCSTOperationMode.KTOK; //Watson Modify 20150130
                else
                    cstOM = eCSTOperationMode.CTOC; //Watson Modify 20150130

                eCSTOperationMode oldMode = eqp.File.CSTOperationMode;

                if (oldMode != cstOM)
                {
                    lock (eqp)
                    {
                        eqp.File.CSTOperationMode = cstOM;
                    }
                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                    // Report to MES
                    Invoke(eServiceName.MESService, "CassetteOperModeChanged",
                        new object[] { inputData.TrackKey, eqp.Data.LINEID, cstOM.ToString() });
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] CASSETTE_OPERATION_MODE=[{3}]({4}).",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, (int)cstOM, cstOM.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CSTOperationModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    CSTOperationModeChangeReportUpdate(inputData, "CSTOperationModeChangeReport_Initial");
                    return;
                }

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                int value = int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].",
                        inputData.Metadata.NodeNo, inputData.TrackKey));
                    CSTOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [取得EQP]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion
                eCSTOperationMode cstOM;
                //目前EQP IO Report "1：Kind To Kind mode  2：Cassette To Cassette mode"
                //但是JOB DATA IO is " 0: Kind to Kind   1: CST to CST"
                
                //
                if (value == 1)
                    cstOM = eCSTOperationMode.KTOK; 
                else
                    cstOM = eCSTOperationMode.CTOC; 

                lock (eqp)
                {
                    eqp.File.CSTOperationMode = cstOM;
                }
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                // Report to MES
                Invoke(eServiceName.MESService, "CassetteOperModeChanged",
                    new object[] { inputData.TrackKey, eqp.Data.LINEID, cstOM.ToString() });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CASSETTE_OPERATION_MODE=[{2}]({3}).",
                    eqp.Data.NODENO, inputData.TrackKey, (int)cstOM, cstOM.ToString()));

                CSTOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    CSTOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void CSTOperationModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_CSTOperationModeChangeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, "CSTOperationModeChangeReportReplyTimeout");
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CSTOperationModeChangeReportReplyTimeout), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CASSETTE OPERATION MODE CHANGE REPORT REPLY, SET BIT=[{2}].",
                        eqpNo, trackKey, value));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CSTOperationModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CASSETTE OPERATION MODE CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], sArray[1], trackKey));

                CSTOperationModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
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


        public override bool Init()
        {
            throw new NotImplementedException();
        }
    }
}
