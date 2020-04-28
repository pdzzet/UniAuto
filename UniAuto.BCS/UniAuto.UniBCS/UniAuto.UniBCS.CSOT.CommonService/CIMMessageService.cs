using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.EntityManager;
using System.Reflection;
using System.Collections.Concurrent;
using System.Timers;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public class CIMMessageService :AbstractService
    {
        private const string Key_CIMMessageSetCommandTimeout = "{0}_CIMMessageSetCommandTimeout";
        private const string Key_CIMMessageClearCommandTimeout = "{0}_CIMMessageClearCommandTimeout";
        private const string Key_CIMMessageConfirmTimeout = "{0}_CIMMessageConfirmTimeout";

        private System.Timers.Timer m_SetTimer;
        private System.Timers.Timer m_ClearTimer;

        /// <summary>
        /// 当前的Set Command Queue
        /// Key=eqpNo
        /// </summary>
        private Dictionary<string, ConcurrentQueue<CIMMessage>> dicSetCommand = new Dictionary<string, ConcurrentQueue<CIMMessage>>();
        /// <summary>
        /// 当前的Clear Command Queue
        /// Key=eqpNo
        /// </summary>
        private Dictionary<string, ConcurrentQueue<CIMMessage>> dicClearCommand = new Dictionary<string, ConcurrentQueue<CIMMessage>>();

        public override bool Init()
        {
            InitSetTimer();
            InitClearTimer();
            return true;
        }

        private void InitSetTimer()
        {
            m_SetTimer = new System.Timers.Timer();
            m_SetTimer.AutoReset = true;
            m_SetTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnSetTimedEvent);
            m_SetTimer.Interval = 2000;
            m_SetTimer.Start();
        }

        private void InitClearTimer()
        {
            m_ClearTimer = new System.Timers.Timer();
            m_ClearTimer.AutoReset = true;
            m_ClearTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnClearTimedEvent);
            m_ClearTimer.Interval = 2000;
            m_ClearTimer.Start();
        }

        private void OnSetTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (dicSetCommand.Count > 0)
                {
                    CIMMessage message=null;
                    foreach (string key in dicSetCommand.Keys)
                    {
                        if (!dicSetCommand[key].IsEmpty)
                        {
                            if (dicSetCommand[key].TryPeek(out message))
                            {
                                if (message.IsSend == false )
                                {
                                    SendCIMMessageCommand(message);
                                }
                                else
                                {
                                    if (message.IsFinish == true)
                                    {
                                        dicSetCommand[key].TryDequeue(out message);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void OnClearTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (dicClearCommand.Count > 0)
                {
                    CIMMessage message = null;
                    foreach (string key in dicClearCommand.Keys)
                    {
                        if (!dicClearCommand[key].IsEmpty)
                        {
                            if (dicClearCommand[key].TryPeek(out message))
                            {
                                if (message.IsSend == false )
                                {
                                    SendCIMMessageCommand(message);
                                }
                                else
                                {
                                    if (message.IsFinish == true)
                                    {
                                        dicClearCommand[key].TryDequeue(out message);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region [CIM Message Set Command]
        /// <summary>
        /// 提供給MES或OPI調用的SET COMMAND
        /// </summary>
        /// <param name="eqpNo">EQUIPMENT NO 機台編號</param>
        /// <param name="cimMessage">CIM MESSAGE</param>
        /// <param name="operatorID">OPERATE ID</param>
        public void CIMMessageSetCommand(string trxID, string eqpNo,string msg,string operatorID)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                CIMMessage cimMessage = new CIMMessage(CIMMessageManager.GetMessageID().ToString(),eqp.Data.NODENO,msg,
                                            operatorID, string.Empty,eCIMMESSAGE_STATE.SET);

                if (!dicSetCommand.ContainsKey(eqpNo))
                {
                    ConcurrentQueue<CIMMessage> queue = new ConcurrentQueue<CIMMessage>();
                    queue.Enqueue(cimMessage);
                    lock (dicSetCommand)
                    {
                        dicSetCommand.Add(eqpNo, queue);
                    }
                }
                else
                {
                    dicSetCommand[eqpNo].Enqueue(cimMessage);
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                 string.Format("[EQUIPMENT=[{0})] [BCS -> EQP][{1}] RECEIVE CIM MESSAGE SET COMMAND ID=[{2}],CIM MESSAGE =[{3}).", cimMessage.NodeNo,
                cimMessage.TrxID, cimMessage.MessageID, cimMessage.Message));

                #region Mark
                              ////encode trx
                //Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_CIMMessageSetCommand") as Trx;
                //outputData.EventGroups[0].Events[0].Items[0].Value = CIMMessageManager.GetMessageID().ToString();
                //outputData.EventGroups[0].Events[0].Items[1].Value = cimMessage;
                //outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //outputData.TrackKey = UtilityMethod.GetAgentTrackKey();

                //if (dicSetCommand.ContainsKey(eqpNo))
                //{
                //    lock (dicSetCommand[eqpNo])
                //    {
                //        dicSetCommand[eqpNo].Enqueue(outputData);
                //    }
                //}
                //else
                //{
                //    Queue<Trx> q = new Queue<Trx>();
                //    lock (q)
                //    {
                //        q.Enqueue(outputData);
                //    }
                //    dicSetCommand.Add(eqpNo,q);
                //}

                ////SendPLCData(outputData);

                ////DB History
                //CIMMESSAGEHISTORY his = new CIMMESSAGEHISTORY();
                //his.MESSAGEID = cimMessageID;
                //his.MESSAGETEXT = cimMessage;
                //his.UPDATETIME = DateTime.Now;
                //his.OPERATORID = operatorID;
                //his.NODEID = eqp.Data.NODEID;
                //his.NODENO = eqp.Data.NODENO;
                //his.MESSAGESTATUS = eCIMMESSAGE_STATE.SET.ToString();
                //his.REMARK = eCIMMESSAGE_STATE.SET.ToString();

                ////CIM Message Data
                //CIMMessage cimMessageData = new CIMMessage();
                //cimMessageData.OccurDateTime = DateTime.Now;
                //cimMessageData.MessageID = cimMessageID;
                //cimMessageData.MessageStatus = his.MESSAGESTATUS;
                //cimMessageData.NodeNo = eqpNo;
                //cimMessageData.OperatorID = his.OPERATORID;
                //cimMessageData.Message = cimMessage;

                ////Keep住CIM Message
                //ObjectManager.CIMMessageManager.AddCIMMessageData(cimMessageData);

                ////HandleCIMMessage(eqp, cimMessageData, his, outputData.TrackKey);

                //string timeName = string.Format(Key_CIMMessageSetCommandTimeout, eqpNo);

                //if (_timerManager.IsAliveTimer(timeName))
                //{
                //    _timerManager.TerminateTimer(timeName);
                //}
                //_timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(CIMMessageSetCommandReplyTimeout), outputData.TrackKey);

                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CIMMessageSetCommandForCELL(string trxID, string eqpNo, string msg, string operatorID,string touchPanelNo)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                if (touchPanelNo.Trim() == string.Empty)
                    touchPanelNo = "0";

                CIMMessage cimMessage = new CIMMessage(CIMMessageManager.GetMessageID().ToString(), eqp.Data.NODENO, msg,
                                            operatorID, touchPanelNo, eCIMMESSAGE_STATE.SET);

                if (!dicSetCommand.ContainsKey(eqpNo))
                {
                    ConcurrentQueue<CIMMessage> queue = new ConcurrentQueue<CIMMessage>();
                    queue.Enqueue(cimMessage);
                    lock (dicSetCommand)
                    {
                        dicSetCommand.Add(eqpNo, queue);
                    }
                }
                else
                {
                    dicSetCommand[eqpNo].Enqueue(cimMessage);
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                 string.Format("[EQUIPMENT=[{0}] [BCS -> EQP][{1}] RECEIVE CIM MESSAGE SET COMMAND ID=[{2}],CIM MESSAGE =[{3}], TOUCH_PANEL_NO=[{4}].", cimMessage.NodeNo,
                cimMessage.TrxID, cimMessage.MessageID, cimMessage.Message,cimMessage.TouchPanelNo));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// Reply Set Command
        /// </summary>
        /// <param name="inputData"></param>
        public void CIMMessageSetCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                if (triggerBit == eBitResult.OFF) return;

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_CIMMessageSetCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                   string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] RECEIVE CIM MESSAGE SET COMMAND REPLY, SET BIT =[{2}]",
                                   eqpNo, inputData.TrackKey,triggerBit.ToString()));

                CIMMessage message = null;
                if (dicSetCommand.ContainsKey(eqpNo))
                {
                    if(!dicSetCommand[eqpNo].IsEmpty)
                    {
                        dicSetCommand[eqpNo].TryDequeue(out message);
                        message.IsFinish = true;
                    }
                    
                }

                //终止Timer
                string timeName = string.Format(Key_CIMMessageSetCommandTimeout, eqpNo);
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

        private void CIMMessageSetCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string eqpNo = sArray[0];
                string timeName = string.Format(Key_CIMMessageSetCommandTimeout, eqpNo);
                //终止Timer
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_CIMMessageSetCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                //从暂存Set Queue中清除
                if (dicSetCommand.ContainsKey(eqpNo))
                {
                    if(!dicSetCommand[eqpNo].IsEmpty)
                    {
                        CIMMessage message=null;
                        if (dicSetCommand[eqpNo].TryDequeue(out message))
                        {
                            message.IsFinish = true;
                        }
                    }
                    
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BCS REPLY, CIM MESSAGE SET COMMAND REPLY TIMEOUT SET VALUE(OFF).",
                    sArray[0], trackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

       }
        #endregion

        #region [CIM Message Clear Command]
        /// <summary>
        /// 提供给OPI调用的Clear Command
        /// </summary>
        /// <param name="eqpNo">EQUIPMENT NO 機台編號</param>
        /// <param name="cimMessageID">CIM MESSAGE ID</param>
        /// <param name="operatorID">OPERATE ID</param>
        public void CIMMessageClearCommand(string trxID, string eqpNo, string messageId, string operateID)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                CIMMessage cimMessage = new CIMMessage(messageId, eqp.Data.NODENO,"",
                                            operateID,string.Empty, eCIMMESSAGE_STATE.CLEAR);

                if (!dicClearCommand.ContainsKey(eqpNo))
                {
                    ConcurrentQueue<CIMMessage> queue = new ConcurrentQueue<CIMMessage>();
                    queue.Enqueue(cimMessage);
                    lock (dicSetCommand)
                    {
                        dicClearCommand.Add(eqpNo, queue);
                    }
                }
                else
                {
                    dicClearCommand[eqpNo].Enqueue(cimMessage);
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] RECEIVE CIM MESSAGE CLEAR COMMAND ID=[{2}). SET BIT(ON)", cimMessage.NodeNo,
                        cimMessage.TrxID, cimMessage.MessageID));

                #region Mark
                //Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_CIMMessageClearCommand") as Trx;
                //outputData.EventGroups[0].Events[0].Items[0].Value = cimMessageID;
                //outputData.EventGroups[0].Events[0].Items[1].Value = cimMessage;
                //outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //outputData.TrackKey = UtilityMethod.GetAgentTrackKey();

                //if (dicClearCommand.ContainsKey(eqpNo))
                //{
                //    lock (dicClearCommand[eqpNo])
                //    {
                //        dicClearCommand[eqpNo].Enqueue(outputData);
                //    }
                //}
                //else
                //{
                //    Queue<Trx> q = new Queue<Trx>();
                //    lock (q)
                //    {
                //        q.Enqueue(outputData);
                //    }
                //    dicClearCommand.Add(eqpNo, q);
                //}
                //SendPLCData(outputData);

                //ObjectManager.CIMMessageManager.ClearCIMMessage(eqpNo);

                //// DB History
                //CIMMESSAGEHISTORY his = new CIMMESSAGEHISTORY();
                //his.MESSAGEID = cimMessageID;
                //his.MESSAGETEXT = cimMessage;
                //his.UPDATETIME = DateTime.Now;
                //his.OPERATORID = operatorID;
                //his.NODEID = eqp.Data.NODEID;
                //his.NODENO = eqp.Data.NODENO;
                //his.MESSAGESTATUS = eCIMMESSAGE_STATE.CLEAR.ToString();
                //his.REMARK = eCIMMESSAGE_STATE.CLEAR.ToString();

                //CIMMessage cimMessageData = new CIMMessage();
                //cimMessageData.OccurDateTime = DateTime.Now;
                //cimMessageData.MessageID = cimMessageID;
                //cimMessageData.MessageStatus = his.MESSAGESTATUS;
                //cimMessageData.NodeNo = eqpNo;
                //cimMessageData.OperatorID = his.OPERATORID;
                //cimMessageData.Message = cimMessage;

                //HandleCIMMessage(eqp, cimMessageData, his, outputData.TrackKey);

                //string timeName = string.Format(Key_CIMMessageClearCommandTimeout, eqpNo);

                //if (_timerManager.IsAliveTimer(timeName))
                //{
                //    _timerManager.TerminateTimer(timeName);
                //}
                //_timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(CIMMessageClearCommandReplyTimeout), outputData.TrackKey);

                //Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Message Clear Command, ID [{2}] CIM Message [{3}], Set Bit [ON]", eqp.Data.NODENO,
                //        outputData.TrackKey, cimMessageID, cimMessage.ToString()));
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        public void CIMMessageClearCommandForCELL(string trxID, string eqpNo, string messageId, string operateID,string touchPanelNo)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                if (touchPanelNo.Trim() == string.Empty)
                    touchPanelNo = "0";

                CIMMessage cimMessage = new CIMMessage(messageId, eqp.Data.NODENO, "",
                                            operateID, touchPanelNo, eCIMMESSAGE_STATE.CLEAR);

                if (!dicClearCommand.ContainsKey(eqpNo))
                {
                    ConcurrentQueue<CIMMessage> queue = new ConcurrentQueue<CIMMessage>();
                    queue.Enqueue(cimMessage);
                    lock (dicSetCommand)
                    {
                        dicClearCommand.Add(eqpNo, queue);
                    }
                }
                else
                {
                    dicClearCommand[eqpNo].Enqueue(cimMessage);
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] RECEIVE CIM MESSAGE CLEAR COMMAND ID=[{2}]. SET BIT(ON)", cimMessage.NodeNo,
                        cimMessage.TrxID, cimMessage.MessageID));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Reply Clear Command
        /// </summary>
        /// <param name="inputData"></param>
        public void CIMMessageClearCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                if (triggerBit == eBitResult.OFF) return;

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_CIMMessageClearCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] RECEIVE CIM MESSAGE CLEAR COMMAND REPLY, SET BIT=[{2}]",
                    eqpNo, inputData.TrackKey, triggerBit.ToString()));

                CIMMessage message = null;

                if (dicClearCommand.ContainsKey(eqpNo))
                {
                    if (!dicClearCommand[eqpNo].IsEmpty)
                    {
                        dicClearCommand[eqpNo].TryDequeue(out message);
                        message.IsFinish = true;
                        //从暂存的正在进行的Set Command Queue中清除
                        ObjectManager.CIMMessageManager.ClearCIMMessage(message.NodeNo, message.MessageID);
                    }
                }

                //终止Timer
                string timeName = string.Format(Key_CIMMessageClearCommandTimeout, eqpNo);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                #region Mark
                //string timeName = string.Format(Key_CIMMessageClearCommandTimeout, inputData.Metadata.NodeNo);
                //if (_timerManager.IsAliveTimer(timeName))
                //{
                //    _timerManager.TerminateTimer(timeName);
                //}

                //if (triggerBit == eBitResult.OFF) return;

                //#region [Command Off]
                //Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(inputData.Metadata.NodeNo + "_CIMMessageClearCommand") as Trx;
                //outputdata.EventGroups[0].Events[0].IsDisable = true;
                //outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                //outputdata.TrackKey = inputData.TrackKey;

                //if (dicClearCommand.ContainsKey(eqpNo))
                //{
                //    lock (dicClearCommand[eqpNo])
                //    {
                //        outputdata = dicClearCommand[eqpNo].Dequeue() as Trx;
                //    }
                //}
                //SendPLCData(outputdata);

                //Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BCS Reply, CIM Message Clear Command Reply Set Value[OFF].", eqpNo, inputData.TrackKey));
                //#endregion
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CIMMessageClearCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string eqpNo = sArray[0].ToString();

                string timeName = string.Format(Key_CIMMessageClearCommandTimeout, eqpNo);

                //终止Timer
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_CIMMessageClearCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (dicClearCommand.ContainsKey(eqpNo))
                {
                    if (!dicClearCommand[eqpNo].IsEmpty)
                    {
                        CIMMessage cimMessage = null;
                        if(dicClearCommand[eqpNo].TryDequeue(out cimMessage))
                        {
                            cimMessage.IsFinish=true;
                            //从暂存的正在进行的Set Command Queue中清除
                            ObjectManager.CIMMessageManager.ClearCIMMessage(cimMessage.NodeNo, cimMessage.MessageID);
                        }
                    }
                }
                

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BCS REPLY, CIM MESSAGE CLEAR COMMAND REPLY TIMEOUT SET VALUE(OFF).",
                    sArray[0], trackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion

        #region [CIM Message Confirm Report]
        /// <summary>
        /// CIM Message Comfirm Report
        /// </summary>
        /// <param name="inputData">Trx對象</param>
        public void CIMMessageConfirmReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));

                string touchPanelNo= string.Empty, operatorID= string.Empty;
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    touchPanelNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                    operatorID = inputData.EventGroups[0].Events[0].Items[2].Value;
                }


                string cimMessageID = inputData.EventGroups[0].Events[0].Items[0].Value;
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                if (bitResult == eBitResult.ON)
                {
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())
                    {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MESSAGE CONFIRM REPORT. CIMMESSAGEID=[{2}) .TOUCHPANELNO=[{3}], OPERATORID=[{4}]",
                        eqpNo, inputData.TrackKey, cimMessageID, touchPanelNo, operatorID));
                    }
                    else
                    {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MESSAGE CONFIRM REPORT. CIMMESSAGEID=[{2}).",
                                    eqpNo, inputData.TrackKey, cimMessageID));
                    }
                    CIMMessageConfirmReportReply(inputData.TrackKey, eqpNo, eBitResult.ON);
             
                    //从暂存的Set Queue中清除
                    ObjectManager.CIMMessageManager.ClearCIMMessage(eqpNo, cimMessageID);
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] RECEIVE CIM MESSAGE CONFIRM REPORT BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));

                    CIMMessageConfirmReportReply(inputData.TrackKey, eqpNo,eBitResult.OFF);
                }
              
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                //參考RecipeService寫法
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    CIMMessageConfirmReportReply(inputData.TrackKey, inputData.Metadata.NodeNo, eBitResult.ON);
                    //从暂存的Set Queue中清除
                    ObjectManager.CIMMessageManager.ClearCIMMessage(inputData.Metadata.NodeNo, 
                        inputData.EventGroups[0].Events[0].Items[0].Value);
                }
            }

            #region Mark
            //try
            //{
            //    if (inputData.IsInitTrigger) return;
            //    string eqpNo = inputData.Metadata.NodeNo;
            //    string cimMessageID = inputData.EventGroups[0].Events[0].Items[0].Value.ToString();

            //    eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

            //    if (triggerBit == eBitResult.OFF) return;

            //    Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_CIMMessageConfirmReportReply") as Trx;
            //    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.ON).ToString();
            //    outputdata.TrackKey = inputData.TrackKey;
            //    SendPLCData(outputdata);

            //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Receive CIM Message Confirm Report, Set Bit [ON]",
            //        eqpNo, inputData.TrackKey));

            //    //从暂存的Set Queue中清除
            //    ObjectManager.CIMMessageManager.ClearCIMMessage(eqpNo, cimMessageID);

            //    //启用Timer
            //    string timerID = string.Format(Key_CIMMessageConfirmTimeout, eqpNo);
            //    if (Timermanager.IsAliveTimer(timerID))
            //    {
            //        Timermanager.TerminateTimer(timerID);
            //    }
            //    Timermanager.CreateTimer(timerID, false, ParameterManager["T2"].GetInteger(), new ElapsedEventHandler(CIMMessageConfirmReportReplyTimeout));

            //    #region Mark
            //    //if (triggerBit == eBitResult.OFF)
            //    //{
            //    //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //    //        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Bit [OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
            //    //    CIMMessageConfirmReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
            //    //    return;
            //    //}

            //    //CIMMessage cimMessageData = ObjectManager.CIMMessageManager.ClearCIMMessage(eqp, cimMessageID);
            //    //if (cimMessageData == null)
            //    //{
            //    //    //CIMMessageSetCommand(eqpNo, "1", "AAAAA", "A");
            //    //    throw new Exception(string.Format("Can't find CIM Message ID[{0}]!", cimMessageID));
            //    //}
            //    //else
            //    //{
            //    //    ObjectManager.CIMMessageManager.ClearCIMMessage(eqpNo);
            //    //}

            //    //// DB History
            //    //CIMMESSAGEHISTORY his = new CIMMESSAGEHISTORY();
            //    //his.MESSAGEID = cimMessageID;
            //    //his.MESSAGETEXT = cimMessageData.Message;
            //    //his.UPDATETIME = DateTime.Now;
            //    //his.OPERATORID = eqp.Data.NODEID;
            //    //his.NODEID = eqp.Data.NODEID;
            //    //his.NODENO = eqp.Data.NODENO;
            //    //his.MESSAGESTATUS = eCIMMESSAGE_STATE.COMFIRM.ToString();
            //    //his.REMARK = eCIMMESSAGE_STATE.CLEAR.ToString();

            //    //HandleCIMMessage(eqp, cimMessageData, his, inputData.TrackKey);

            //    //Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //    //    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode [{2}], CIM Message Confirm Report Message ID[{3}], Bit [ON]",
            //    //    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, cimMessageID));

            //    //CIMMessageConfirmReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            //    #endregion
            //}
            //catch (Exception ex)
            //{
            //    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //}
            #endregion
        }

        private void CIMMessageConfirmReportReply(string trxID, string eqpNo, eBitResult bit)
        {
            try
            {
                Trx output = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_CIMMessageConfirmReportReply") as Trx;
                if (output != null)
                {
                    output.TrackKey = trxID;
                    output.EventGroups[0].Events[0].Items[0].Value = ((int)bit).ToString();
                    SendPLCData(output);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,CIM MESSAGE CONFIRM REPORT REPLY SET BIT =[{2}).",
                    eqpNo, trxID, bit));

                    if (_timerManager.IsAliveTimer(string.Format(Key_CIMMessageConfirmTimeout, eqpNo)))
                    {
                        _timerManager.TerminateTimer(string.Format(Key_CIMMessageConfirmTimeout, eqpNo));
                    }
                    if (bit == eBitResult.ON)
                    {
                        _timerManager.CreateTimer(string.Format(Key_CIMMessageConfirmTimeout, eqpNo), false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(CIMMessageConfirmReportReplyTimeout), trxID);
                    }
                }
            }
            catch (System.Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

            //try
            //{
            //    Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_CIMMessageConfirmReportReply") as Trx;
            //    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
            //    outputdata.TrackKey = trxId;
            //    SendPLCData(outputdata);

            //    string timeName = string.Format(Key_CIMMessageConfirmTimeout, eqpNo);
            //    if (_timerManager.IsAliveTimer(timeName))
            //    {
            //        _timerManager.TerminateTimer(timeName);
            //    }

            //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Receive CIM Message Confirm Report, Set Bit [OFF]",
            //        eqpNo, outputdata.TrackKey));
            //}
            //catch (Exception ex)
            //{
            //    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);


            //}
        }

        private void CIMMessageConfirmReportReplyTimeout(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string eqpNo = sArray[0];
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP REPLY,CIM MESSAGE CONFIRM REPORT REPLY TIMEOUT SET BIT (OFF).", eqpNo, trackKey));

                CIMMessageConfirmReportReply(trackKey, eqpNo, eBitResult.OFF);
            }
            catch (System.Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

            //try
            //{
            //    UserTimer timer = subjet as UserTimer;
            //    string tmp = timer.TimerId;
            //    string trackKey = timer.State.ToString();
            //    string[] sArray = tmp.Split('_');
            //    string eqpNo = sArray[0];

            //    string timeName = string.Format(Key_CIMMessageConfirmTimeout, eqpNo);

            //    if (_timerManager.IsAliveTimer(timeName))
            //    {
            //        _timerManager.TerminateTimer(timeName);
            //    }

            //    Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_CIMMessageConfirmReportReply") as Trx;
            //    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
            //    outputdata.TrackKey = trackKey;

            //    SendPLCData(outputdata);

            //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BCS Reply, CIM Message Confirm Report Reply Timeout Set Value[OFF].",
            //        eqpNo, trackKey));

            //}
            //catch (Exception ex)
            //{
            //    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //}

        }
        #endregion

        #region [Common Method]
        /// <summary>
        /// 下Command指令
        /// </summary>
        /// <param name="message"></param>
        private void SendCIMMessageCommand(CIMMessage message)
        {
            try
            {
                Trx outputData = null;
                string timerID;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(message.NodeNo);
                eReportMode reportMode;
                Enum.TryParse<eReportMode>(eqp.Data.REPORTMODE, out reportMode);
                if (message.MessageStatus == eCIMMESSAGE_STATE.SET)
                {
                    switch (reportMode)
                    {
                        case eReportMode.HSMS_NIKON:
                            Invoke(eServiceName.NikonSECSService, "TS10F3_H_TerminalDisplaySingle", new object[] { eqp.Data.NODENO,eqp.Data.NODEID,
                                        message.Message, message.TrxID, string.Empty});
                            message.IsFinish = true;
                            break;
                        case eReportMode.HSMS_PLC:
                        case eReportMode.HSMS_CSOT:
                            Invoke(eServiceName.CSOTSECSService, "TS10F3_H_TerminalDisplaySingle", new object[] { eqp.Data.NODENO,eqp.Data.NODEID,
                                        message.Message, message.TrxID, string.Empty });
                            message.IsFinish = true;
                            break;

                        case eReportMode.PLC:
                        case eReportMode.PLC_HSMS:
                            SendCIMMessageCommandToPLC(message);
                            //启用Timer
                            timerID = string.Format(Key_CIMMessageSetCommandTimeout, message.NodeNo);
                            if (Timermanager.IsAliveTimer(timerID))
                            {
                                Timermanager.TerminateTimer(timerID);
                            }
                            Timermanager.CreateTimer(timerID, false, ParameterManager["T1"].GetInteger(), new ElapsedEventHandler(CIMMessageSetCommandReplyTimeout), message.TrxID);

                            //暂存到正在进行的Set Command Queue
                            ObjectManager.CIMMessageManager.AddCIMMessageData(message);
                            break;
                        default:
                            break;
                    }
                   
                    message.IsSend = true;

                    //DB History
                    SaveToDBHistory(message);
                   
                }
                else if (message.MessageStatus == eCIMMESSAGE_STATE.CLEAR)
                {
                    if(reportMode==eReportMode.PLC||reportMode==eReportMode.PLC_HSMS)
                    {
                        outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(message.NodeNo + "_CIMMessageClearCommand") as Trx;
                        if (outputData != null)
                        {
                            IList<Line> lines = ObjectManager.LineManager.GetLines();

                            if (lines == null) throw new Exception(string.Format("CAN'T FIND LINE IN LINEENTITY!"));

                            if (lines[0].Data.FABTYPE == eFabType.CELL.ToString())
                            {
                                outputData.EventGroups[0].Events[0].Items[1].Value = message.TouchPanelNo;
                            }
                            outputData.EventGroups[0].Events[0].Items[0].Value = message.MessageID;
                            outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                            //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                            outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                            outputData.TrackKey = message.TrxID;
                            SendPLCData(outputData);

                            message.IsSend = true;
                            //启用Timer
                            timerID = string.Format(Key_CIMMessageClearCommandTimeout, message.NodeNo);
                            if (Timermanager.IsAliveTimer(timerID))
                            {
                                Timermanager.TerminateTimer(timerID);
                            }
                            Timermanager.CreateTimer(timerID, false, ParameterManager["T1"].GetInteger(), new ElapsedEventHandler(CIMMessageClearCommandReplyTimeout), message.TrxID);
                            //DB History
                            
                        }
                        SaveToDBHistory(message);
                    }
                    else
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("WARN: CIM CLEAR COMMAND TRX IS NULL.[EQP=[{0}]", message.NodeNo));
                        message.IsFinish = true;
                    }
                }

                message.IsSend = true;

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        private void SendCIMMessageCommandToPLC(CIMMessage message)
        {
            Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(message.NodeNo + "_CIMMessageSetCommand") as Trx;

            IList<Line> lines = ObjectManager.LineManager.GetLines();

            if (lines == null) throw new Exception(string.Format("CAN'T FIND LINE IN LINEENTITY!"));

            if (lines[0].Data.FABTYPE == eFabType.CELL.ToString())
            {
                outputData.EventGroups[0].Events[0].Items[2].Value = message.TouchPanelNo;
            }


            if (outputData != null)
            {
                outputData.EventGroups[0].Events[0].Items[0].Value = message.MessageID;
                outputData.EventGroups[0].Events[0].Items[1].Value = message.Message;
                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.TrackKey = message.TrxID;
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                SendPLCData(outputData);
            }
            else
            {
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            string.Format("WARN: CIM SET COMMAND TRX IS NULL.[EQP=[{0}]", message.NodeNo));
            }

        }
        /// <summary>
        /// 将CIM Message 转成History存DB
        /// </summary>
        /// <param name="message"></param>
        private void SaveToDBHistory(CIMMessage message)
        {
            //Save to DB History
            CIMMESSAGEHISTORY his = new CIMMESSAGEHISTORY();
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(message.NodeNo);
                his.MESSAGEID = message.MessageID;
                his.MESSAGETEXT = message.Message;
                his.UPDATETIME = DateTime.Now;
                his.OPERATORID = message.OperatorID;
                his.NODEID = eqp.Data.NODEID;
                his.NODENO = eqp.Data.NODENO;
                if (message.MessageStatus == eCIMMESSAGE_STATE.SET.ToString())
                {
                    his.MESSAGESTATUS = eCIMMESSAGE_STATE.SET.ToString();
                    his.REMARK = "";
                }
                else if (message.MessageStatus == eCIMMESSAGE_STATE.CLEAR.ToString())
                {
                    his.MESSAGESTATUS = eCIMMESSAGE_STATE.CLEAR.ToString();
                    his.REMARK = "";
                }
                ObjectManager.CIMMessageManager.SaveCIMMessageHistoryToDB(his);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 返回指定機台的所有未Clear的CIM Message
        /// 返回一個Dictionary，Key為eqpNo，Value為該機台下的List<CIMMessage>
        /// eqpNo為空值時，返回所有的機台下的所有CIM Message
        /// </summary>
        /// <param name="eqpNo">eqpNo,默認值為空，即返回所有機台下的所有CIM Message</param>
        /// <returns></returns>
        public IDictionary<string, List<CIMMessage>> GetCIMMessageByEqpNo(string eqpNo="")
        {
            IDictionary<string, CIMMessage> dicAll = new Dictionary<string, CIMMessage>();
            //取得当前所有未Clear的CIM Message
            dicAll = ObjectManager.CIMMessageManager.GetCIMMessageData();
            //用於記錄某臺機台的所有CIM Message，採用List<>存儲
            List<CIMMessage> lstCIMMessage = new List<CIMMessage>();
            //用於記錄指定機台下的所有CIM Message，採用Dictionary存儲，其中Key為eqpNo,Value為List<CIM Message>
            IDictionary<string, List<CIMMessage>> dicCIMMessageByEqp = new Dictionary<string, List<CIMMessage>>();

            string[] sArray = new string[2];
            string strEqpKey = string.Empty;

            #region [所有機台的所有CIM Message]
            foreach (string key in dicAll.Keys)
            {
                sArray = key.Split('_');
                strEqpKey = sArray[0];//取機台編號
                if (!dicCIMMessageByEqp.ContainsKey(strEqpKey))
                {
                    lstCIMMessage.Add(dicAll[key]);
                    dicCIMMessageByEqp.Add(strEqpKey, lstCIMMessage);
                    lstCIMMessage.Clear();
                }
                else
                {
                    dicCIMMessageByEqp[strEqpKey].Add(dicAll[key]);
                }
            }
            #endregion

            if (eqpNo != string.Empty)
            {
                #region [返回指定機台的所有CIM Message]

                lstCIMMessage = dicCIMMessageByEqp[eqpNo];
                dicCIMMessageByEqp.Clear();
                dicCIMMessageByEqp.Add(eqpNo, lstCIMMessage);

                #endregion
            }
            return dicCIMMessageByEqp;
        }
        #endregion

        private void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }

    }
}
