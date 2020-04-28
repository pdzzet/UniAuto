using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using System.Threading;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.EntityManager;
using System.Text.RegularExpressions;

namespace UniAuto.UniBCS.CSOT.CommonService
{

    public class RecipeService : AbstractService
    {
        const string RecipeRegisterCommand = "{0}_RecipeRegisterValidationCommand";
        const string RecipeParameterCommand = "{0}_RecipeParameterRequestCommand";
        const string RecipeRegisterCommandTimeout = "{0}_RecipeRegisterValidationCommandTimeout";
        const string RecipeParameterCommandTimeout = "{0}_RecipeParameterRequestCommandTimeout";
        const string RecipeModifyReportTimeout = "{0}_RecipeModifyReportTimeout";
        const string CurrentRecipeIDChangeReportTimeout = "{0}_CurrentRecipeIDChangeReportTimeout";


        IDictionary<string, Queue<RecipeCheckInfo>> _recipeRegisterValidationQ = new Dictionary<string, Queue<RecipeCheckInfo>>();
        Queue<RecipeCheckCommand> _queueRecipeChecCommand = new Queue<RecipeCheckCommand>();
        Queue<RecipeCheckCommand> _queueRecipeParamCommand = new Queue<RecipeCheckCommand>();
        IDictionary<string, Queue<RecipeCheckInfo>> _recipeParameterQ = new Dictionary<string, Queue<RecipeCheckInfo>>();

        //记录其他Line的Recipe Param Command
        IDictionary<string, IList<RecipeCheckInfo>> _otherLineRecipeParam = new Dictionary<string, IList<RecipeCheckInfo>>();
        // ConcurrentQueue<IList<RecipeCheckInfo>> _otherLineRecipeParam = new ConcurrentQueue<IList<RecipeCheckInfo>>();
        /// <summary>
        /// Short Cut 和Cell 跨Line 的Recipe Register 需要用到这个集合，
        /// 在记录BCS 需要跨Line 检查的RecipeID , 被跨的Line 检查完成后直接回传全部的讯息，包括Timeout的。
        /// 此集合使用 _LineName +TrxID为key 值，如果SocketAgent 回抛了跨Line的讯息 那么TrxID 需要一至 这样就可以再
        /// 这个集合中将起取出并间结果更新并从集合中删除。
        /// </summary>
        IDictionary<string, IList<RecipeCheckInfo>> _otherLineRecipeRegister = new Dictionary<string, IList<RecipeCheckInfo>>();

        IDictionary<string, RecipeCheckCommand> _waitForMES = new Dictionary<string, RecipeCheckCommand>();

        Thread _recipeRegisterThread;
        Thread _recipeParamThread;
        bool _isRuning;

        IServerAgent _plcAgent = null;
        private IServerAgent PLCAgent
        {
            get
            {
                if (_plcAgent == null)
                {
                    _plcAgent = GetServerAgent(eAgentName.PLCAgent);
                }
                return _plcAgent;
            }
        }

        public override bool Init()
        {
            _isRuning = true;
            _recipeRegisterThread = new Thread(new ThreadStart(RecipeRegisterProcess));
            _recipeRegisterThread.IsBackground = true;
            _recipeRegisterThread.Start();
            _recipeParamThread = new Thread(new ThreadStart(RecipeParamProcess));
            _recipeParamThread.IsBackground = true;
            _recipeParamThread.Start();
            return true;
        }

        public void Distory()
        {
            _isRuning = false;
            _recipeParamThread.Join();
            _recipeRegisterThread.Join();
        }

        private void SendToPLC(Trx outputData)
        {
            xMessage message = new xMessage()
            {
                ToAgent = eAgentName.PLCAgent,
                Data = outputData,
                Name = outputData.Name,
                TransactionID = outputData.TrackKey
            };
            PutMessage(message);
        }

        #region Recipe Register
        private void RecipeRegisterProcess()
        {
            while (_isRuning)
            {
                Thread.Sleep(300);
                try
                {
                    DateTime now = DateTime.Now;
                    RecipeCheckInfo rci = null;

                    foreach (string key in _recipeRegisterValidationQ.Keys)
                    {
                        if (_recipeRegisterValidationQ[key].Count > 0)
                        {
                            lock (_recipeRegisterValidationQ[key])
                            {
                                rci = _recipeRegisterValidationQ[key].Peek();
                            }
                            if (rci != null)
                            {
                                #region BC Download Time Check
                                if (new TimeSpan(DateTime.Now.Ticks - rci.CreateTime.Ticks).TotalMilliseconds >= ParameterManager["RECIPEREGISTERTIMEOUT"].GetInteger())
                                {
                                    rci.IsFinish = true;
                                    rci.Result = eRecipeCheckResult.TIMEOUT;

                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BC Download {0}_RecipeRegisterValidationCommand Timeout", rci.EQPNo, rci.TrxId));
                                    lock (_recipeRegisterValidationQ[key])
                                        _recipeRegisterValidationQ[key].Dequeue();
                                    break;
                                    // continue; //继续此Node 取出下一个
                                }
                                #endregion
                                if (rci.IsSend == false && rci.IsFinish == false)
                                {
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);

                                    if (eqp.Data.RECIPEREGVALIDATIONENABLED == "Y")  //此欄位在Node DB中設定
                                    {
                                        #region Send To EQP
                                        eReportMode reportMode;
                                        Enum.TryParse<eReportMode>(eqp.Data.REPORTMODE, out reportMode);
                                        switch (reportMode)
                                        {
                                            case eReportMode.PLC:
                                            case eReportMode.PLC_HSMS:
                                                //检查
                                                if (CheckRecipeRegisterValidationReplyIsOff(rci.EQPNo)) {
                                                    RecipeRegisterValidationCommandForPLC(key, rci);
                                                }
                                                break;
                                            case eReportMode.HSMS_CSOT:
                                            case eReportMode.HSMS_NIKON:
                                            case eReportMode.HSMS_PLC:
                                                RecipeRegisterValidationCommandForSECS(reportMode, rci);
                                                break;
                                            case eReportMode.RS232:
                                                //To Do RS232
                                                break;
                                            case eReportMode.RS485:
                                                //To Do Rs485
                                                break;
                                        }
                                        #endregion
                                    }
                                }

                                #region Dequeue Finish Command
                                if (rci.IsFinish)
                                {
                                    lock (_recipeRegisterValidationQ[key])
                                    {
                                        _recipeRegisterValidationQ[key].Dequeue();
                                    }
                                    continue;
                                }
                                #endregion
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                }
            }
        }

        #region Down Recipe Register Validation Command
        /// <summary>
        /// Send RecipeRegister validation Command For PLC
        /// </summary>
        /// <param name="key">Node No</param>
        /// <param name="rci"></param>
        private void RecipeRegisterValidationCommandForPLC(string key, RecipeCheckInfo rci)
        {
            try
            {
                Trx recipeRegisterTrx = PLCAgent.GetTransactionFormat(string.Format(RecipeRegisterCommand, rci.EQPNo)) as Trx;
                if (recipeRegisterTrx != null)
                {
                    recipeRegisterTrx.EventGroups[0].Events[0].Items[0].Value = rci.Mode.ToString();
                    //AC
                    recipeRegisterTrx.EventGroups[0].Events[0].Items[1].Value = rci.PortNo.ToString();
                    recipeRegisterTrx.EventGroups[0].Events[0].Items[2].Value = rci.RecipeID.PadRight(12, ' ');
                    if (recipeRegisterTrx.EventGroups[0].Events[0].Items.Count > 3)//cc.kuang add recipe version for t3
                    {
                        if (rci.RecipeVersion != null && rci.RecipeVersion.Length > 0)
                        {
                            recipeRegisterTrx.EventGroups[0].Events[0].Items[3].Value = rci.RecipeVersion;
                        }
                        else
                        {
                            recipeRegisterTrx.EventGroups[0].Events[0].Items[3].Value = "000000000000";
                        }
                    }
                    //CELL
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(rci.EqpID);//sy edit OPI用EqpID Vali用EQPNo去找
                    if (eqp == null) eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);

                    if (ObjectManager.LineManager.GetLine(eqp.Data.LINEID).Data.FABTYPE == eFabType.CELL.ToString())
                    {
                        recipeRegisterTrx.EventGroups[0].Events[0].Items[1].Value = rci.PortNo.ToString();
                        recipeRegisterTrx.EventGroups[0].Events[0].Items[2].Value = rci.RecipeID.PadRight(2, ' ');
                    }

                    recipeRegisterTrx.EventGroups[0].Events[1].Items[0].Value = "1";
                    recipeRegisterTrx.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    recipeRegisterTrx.TrackKey = rci.TrxId;
                    SendToPLC(recipeRegisterTrx);
                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] RECIPEREGISTERVALIDATIONCOMMAND MODE =[{2}],PORTNO =[{3}],RECIPEID =[{4}]",
                                    rci.EQPNo, rci.TrxId, rci.Mode, rci.PortNo, rci.RecipeID));
                    lock (rci) rci.IsSend = true;
                    string timerId = string.Format(RecipeRegisterCommandTimeout, rci.EQPNo);
                    if (Timermanager.IsAliveTimer(timerId))
                    {
                        Timermanager.TerminateTimer(timerId);
                    }
                    //Add By Yangzhenteng 20190107 For POL YITIAN 设备回复时间调整;
                    if (eqp.Data.LINEID.Contains("CCPOL400") || eqp.Data.LINEID.Contains("CCPOLJ00") || eqp.Data.LINEID.Contains("CCPOLK00") || eqp.Data.LINEID.Contains("CCPOLL00") ||
                        eqp.Data.LINEID.Contains("CCPOLM00") || eqp.Data.LINEID.Contains("CCPOLN00") || eqp.Data.LINEID.Contains("CCPOLP00"))
                    {
                        Timermanager.CreateTimer(timerId, false, ParameterManager["POLSpecialReciepCheckTime"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeRegisterValidationTimeoutForPLC), rci.TrxId);
                    }
                    else
                    {
                        Timermanager.CreateTimer(timerId, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeRegisterValidationTimeoutForPLC), rci.TrxId);
                    }
                    Thread.Sleep(3);//此处停止一下
                }
                else
                {
                    ////bool  result = _recipeRegisterValidationQ[key].TryDequeue(out rci);
                    //if (result)
                    //{
                    lock (rci)
                    {
                        rci.Result = eRecipeCheckResult.NG;
                        rci.IsFinish = true;
                        rci.IsSend = true;
                    }
                    //}

                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] IS NOT FOUND {0}_RECUPEREGISTERVALIDATIONCOMMAND TRX!", rci.EQPNo, rci.TrxId));
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }

        /// <summary>
        ///  Send RecipeRegister validation Command For SECS
        /// </summary>
        /// <param name="reportMode">CSOT OR NIKON</param>
        /// <param name="rci"></param>
        private void RecipeRegisterValidationCommandForSECS(eReportMode reportMode, RecipeCheckInfo rci)
        {
            try
            {
                switch (reportMode)
                {
                    case eReportMode.HSMS_CSOT:
                    case eReportMode.HSMS_PLC:
                        Invoke(eServiceName.CSOTSECSService, "TS7F73_H_RecipeIDCheck", new object[] { rci, rci.TrxId });
                        break;
                    case eReportMode.HSMS_NIKON:
                        Invoke(eServiceName.NikonSECSService, "TS7F19_H_CurrentEPPDRequest", new object[] { rci, rci.TrxId });
                        break;
                }
                lock (rci)
                    rci.IsSend = true;
                string timerId = string.Format(RecipeRegisterCommandTimeout, rci.EQPNo);
                if (Timermanager.IsAliveTimer(timerId))
                {
                    Timermanager.TerminateTimer(timerId);
                }
                Timermanager.CreateTimer(timerId, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeRegisterValidationTimeoutForSECS), rci.TrxId);

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Send Recipe Register Validation Command For BCS
        /// </summary>
        /// <param name="trxid"></param>
        /// <param name="recipeCheckInfos"></param>
        private void RecipeRegisterValidationCommandForBCS(string trxid, string lineName, IList<RecipeCheckInfo> recipeCheckInfos, IList<string> noCheckList)
        {
            try
            {
                string key = string.Format("{0}_{1}", lineName, trxid);
                if (_otherLineRecipeRegister.ContainsKey(key))
                {
                    _otherLineRecipeRegister.Remove(key);
                }
                _otherLineRecipeRegister.Add(key, recipeCheckInfos);

                foreach (RecipeCheckInfo info in recipeCheckInfos)
                {
                    info.TrxId = trxid;
                }
                string timerId = string.Format(RecipeRegisterCommandTimeout, key);
                if (Timermanager.IsAliveTimer(timerId))
                {
                    Timermanager.TerminateTimer(timerId);
                }
                Timermanager.CreateTimer(timerId, false, ParameterManager["RECIPEREGISTERBCSTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeRegisterValidationTimeoutForBCS), trxid);
                //To Do Invoke SocketService 
                //public void RecipeIDRegisterCheckRequest(string trxid, string lineName, IList<RecipeCheckInfo> recipeCheckInfos, IList<string> noCheckList)
                Invoke(eServiceName.ActiveSocketService, "RecipeIDRegisterCheckRequest", new object[] 
                {
                    trxid,
                    lineName,
                    recipeCheckInfos,
                    noCheckList
                });
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("LINENAME={0} [BCS -> BCS]=[{1}] SEND RECIPE END RECIPE REGISTER TO OTHER LINE.", lineName, trxid));
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Recipe Register Validation Timeout
        /// <summary>
        /// Recipe RegisterValidation Timeout For PLC
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        private void RecipeRegisterValidationTimeoutForPLC(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string eqpNo = sArray[0];
                if (Timermanager.IsAliveTimer(tmp))
                {
                    Timermanager.TerminateTimer(tmp);
                }
                Trx trx = PLCAgent.GetTransactionFormat(string.Format(RecipeRegisterCommand, eqpNo)) as Trx;
                trx.EventGroups[0].Events[0].IsDisable = true;
                trx.EventGroups[0].Events[1].Items[0].Value = "0";
                trx.TrackKey = trackKey;
                SendToPLC(trx);
                RecipeCheckInfo rci = null;
                if (_recipeRegisterValidationQ.ContainsKey(eqpNo))
                {
                    if (_recipeRegisterValidationQ[eqpNo].Count > 0)
                    {
                        lock (_recipeRegisterValidationQ[eqpNo])
                            rci = _recipeRegisterValidationQ[eqpNo].Dequeue();
                        if (rci != null)
                        {
                            lock (rci)
                            {
                                rci.IsFinish = true;
                                rci.Result = eRecipeCheckResult.TIMEOUT;
                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] RECIPE REGISTER COMMAND EQUIPMENT REPLY TIMEOUT RECIPEID =[{2}],PORTNO =[{3}],MODE =[{4}] SET BIT [OFF].",
                                    sArray[0], trackKey, rci.RecipeID, rci.PortNo, rci.Mode));
                                return;
                            }
                        }
                    }

                }

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Recipe RegisterValidation Timeout For SECS
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        private void RecipeRegisterValidationTimeoutForSECS(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string eqpNo = sArray[0];
                if (Timermanager.IsAliveTimer(tmp))
                {
                    Timermanager.TerminateTimer(tmp);
                }
                RecipeCheckInfo rci = null;
                if (_recipeRegisterValidationQ.ContainsKey(eqpNo))
                {

                    if (_recipeRegisterValidationQ[eqpNo].Count > 0)
                    {
                        lock (_recipeRegisterValidationQ[eqpNo])
                            rci = _recipeRegisterValidationQ[eqpNo].Dequeue();
                        if (rci != null)
                        {
                            lock (rci)
                            {
                                rci.IsFinish = true;
                                rci.Result = eRecipeCheckResult.TIMEOUT;
                            }
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] RECIPE REGISTER COMMAND EQUIPMENT REPLY TIMEOUT RECIPEID =[{2}],PORTNO =[{3}],MODE =[{4}].",
                                sArray[0], trackKey, rci.RecipeID, rci.PortNo, rci.Mode));
                            return;
                        }
                    }

                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BCS ClEAR, RECIPE REGISTER COMMAND EQUIPMENT REPLY TIMEOUT.", sArray[0], trackKey));

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Recipe Register Validation Timeout For BCS
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        private void RecipeRegisterValidationTimeoutForBCS(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string lineName = sArray[1];
                if (Timermanager.IsAliveTimer(tmp))
                {
                    Timermanager.TerminateTimer(tmp);
                }
                //此处的Key 值使用 Line +TrxiD
                string key = string.Format("{0}_{1}", lineName, trackKey);
                lock (_otherLineRecipeRegister)
                {
                    if (_otherLineRecipeRegister.ContainsKey(key))
                    {
                        IList<RecipeCheckInfo> recipeCheckInfos = _otherLineRecipeRegister[key];
                        foreach (RecipeCheckInfo info in recipeCheckInfos)
                        {
                            lock (info)
                            {
                                info.IsSend = true;
                                info.IsFinish = true;
                                info.Result = eRecipeCheckResult.TIMEOUT;
                            }
                        }
                        _otherLineRecipeRegister.Remove(key);//从集合中删除已经 Timeout 的别Line的Recipe Register Command
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                     string.Format("[LineName={0}] [BCS -> EQP]=[{1}] BCS ClEAR, RECIPE REGISTER COMMAND EQUIPMENT REPLY .", lineName, trackKey));
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Receive Recipe Register Command
        /// <summary>
        /// RecipeRegisterValidationCommand
        /// BCS By Line 做RecipeRegister
        /// RecipeCheckInfoList 每个Node 可以是多个RecipeCheckInfo 
        /// </summary>
        /// <param name="trxID">Trx ID </param>
        /// <param name="recipeInfoList"> </param>
        /// <returns></returns>
        public bool RecipeRegisterValidationCommand(string trxID, IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfos, IList<string> notCheckEqp)
        {
            try
            {
                if (recipeCheckInfos == null)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[BCS -> EQP]=[{0}] RECIPE REGISTER VALIDATION COMMAND IS NULL.", trxID));
                    return false;
                }

                //将Queue加入的Queue中等待
                lock (_queueRecipeChecCommand)
                {
                    _queueRecipeChecCommand.Enqueue(new RecipeCheckCommand(trxID, eRecipeCheckCommandType.BCS));//记录一个Command
                }
                DateTime createTime = DateTime.Now;
                foreach (string lineName in recipeCheckInfos.Keys)
                {
                    Line line = ObjectManager.LineManager.GetLine(lineName);
                    if (line == null)// 非本Line的 Recipe Check，需要用Socket 通知 其他的Line,只有Short Cut 和Cell 跨 Line 才会用到
                    {
                        //To Do Invoke SocketAgent
                        RecipeRegisterValidationCommandForBCS(trxID, lineName, recipeCheckInfos[lineName], notCheckEqp);

                        continue;//继续下一条Line
                    }
                    //分类本条Line的 Command

                    foreach (RecipeCheckInfo rci in recipeCheckInfos[lineName])
                    {
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);
                        rci.CreateTime = createTime;
                        rci.TrxId = trxID;//For Secs 需要使用
                        if (eqp == null)
                        {
                            #region Not Found Node
                            lock (rci)
                            {
                                rci.IsFinish = true;
                                rci.IsSend = true;
                                rci.Result = eRecipeCheckResult.MACHINENG;
                            }

                            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BC Download {0}_RecipeRegisterValidationCommand Node not found", rci.EQPNo, rci.TrxId));

                            continue;
                            #endregion
                        }
                        else
                        {
                            if (eqp.File.CIMMode == eBitResult.OFF)
                            {
                                #region CIM OFF
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.Result = eRecipeCheckResult.CIMOFF;
                                    rci.IsSend = true;
                                }

                                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BC Download {0}_RecipeRegisterValidationCommand CIM OFF", rci.EQPNo, rci.TrxId));
                                continue;//继续此Node 取出下一个
                                #endregion
                            }
                            else
                            {
                                if (rci.RecipeID == new string('0', eqp.Data.RECIPELEN))
                                {
                                    #region Recipe="00"
                                    lock (rci)
                                    {
                                        rci.IsFinish = true;
                                        rci.Result = eRecipeCheckResult.ZERO;  //Waton modify 20150310
                                        rci.IsSend = true;
                                    }
                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BC Download {0}_RecipeRegisterValidationCommand Recipe ID =[{2}] NO CHECK.", rci.EQPNo, rci.TrxId, rci.RecipeID));

                                    continue;
                                    #endregion//continue;//继续此Node 取出下一个
                                }
                                if (eqp.Data.RECIPEREGVALIDATIONENABLED != "Y")  //避免DB欄位為NULL也加入Check
                                {
                                    #region RECIPEREGVALIDATIONENABLED==N
                                    lock (rci)
                                    {
                                        rci.IsFinish = true;
                                        rci.Result = eRecipeCheckResult.NOCHECK;
                                        rci.IsSend = true;
                                    }
                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Equipment Recipe ID Validation Enable (N) Recipe ID =[{2}] NO CHECK.", rci.EQPNo, rci.TrxId, rci.RecipeID));

                                    continue;
                                    #endregion//continue;//继续此Node 取出下一个
                                }
                            }
                        }
                        lock (_recipeRegisterValidationQ)
                        {
                            if (_recipeRegisterValidationQ.ContainsKey(rci.EQPNo))
                            {
                                lock (_recipeRegisterValidationQ[rci.EQPNo])
                                {
                                    _recipeRegisterValidationQ[rci.EQPNo].Enqueue(rci);
                                }
                            }
                            else
                            {
                                Queue<RecipeCheckInfo> recipeCheckQueue = new Queue<RecipeCheckInfo>();
                                recipeCheckQueue.Enqueue(rci);
                                _recipeRegisterValidationQ.Add(rci.EQPNo, recipeCheckQueue);
                            }
                        }
                    }
                }

                DateTime startTime = DateTime.Now;
                while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["RECIPEREGISTERTIMEOUT"].GetInteger())
                {
                    Thread.Sleep(300);//阻塞呼叫的Function
                    bool finishflag = true;
                    foreach (string lineName in recipeCheckInfos.Keys)
                    {
                        foreach (RecipeCheckInfo rci in recipeCheckInfos[lineName])
                        {
                            finishflag = rci.IsFinish;
                            if (finishflag == false)//发现没有完成的，跳出不用检查，让线程休息一下。
                                break;
                        }
                        if (finishflag == false)//有一条没有Finish 则跳出循环休息一下，等待下一次
                        {
                            break;
                        }
                    }
                    if (finishflag)//全部检查完成。停止阻塞
                    {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[BCS -> EQP]=[{0}]RECIPE REGISTER VALIDATION COMMAND IS FINISH.", trxID));
                        break;
                    }
                }

                //RecipeCheckCommand command=null;
                lock (_queueRecipeChecCommand)
                {
                    _queueRecipeChecCommand.Dequeue();
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// Recipe Register Validation for OPI 
        /// BCS OPI 或者MES OPI 做 Recipe Register该方法会检查是否有正在做Recipe Register 如果有则将直接回复NG
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="recipeCheckInfoList"></param>
        /// <returns></returns>
        public bool RecipeRegisterValidationCommandFromUI(string trxID, IList<RecipeCheckInfo> recipeCheckInfoList)
        {
            try
            {
                if (_queueRecipeChecCommand.Count > 0)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                             string.Format("[BCS -> EQP]=[{0}] RECIPE SERVICE HAVE COMMAND BCS DO NOT EXECUTE RECIPE REGISTER.", trxID));
                    return false;
                }

                if (recipeCheckInfoList == null)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[BCS -> EQP]=[{0}] RECIPE REGISTER VALIDATION COMMAND IS NULL.", trxID));
                    return false;
                }

                DateTime createTime = DateTime.Now;

                foreach (RecipeCheckInfo rci in recipeCheckInfoList)
                {

                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);
                    rci.CreateTime = createTime;
                    rci.TrxId = trxID;//For Secs 需要使用

                    //Watson Add 20150302 For HVA EQP 
                    rci.Mode = (int)eRecipeCheckMode.Manual;  //"1：Auto ,2：Manual"

                    if (eqp == null)
                    {
                        #region Not Found Node
                        lock (rci)
                        {
                            rci.IsFinish = true;
                            rci.IsSend = true;
                            rci.Result = eRecipeCheckResult.MACHINENG;
                        }

                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BC Download {0}_RecipeRegisterValidationCommand Node not found", rci.EQPNo, rci.TrxId));

                        continue;
                        #endregion
                    }
                    else
                    {
                        if (eqp.File.CIMMode == eBitResult.OFF)
                        {
                            #region CIM OFF
                            lock (rci)
                            {
                                rci.IsFinish = true;
                                rci.Result = eRecipeCheckResult.CIMOFF;
                                rci.IsSend = true;
                            }

                            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BC Download {0}_RecipeRegisterValidationCommand CIM OFF", rci.EQPNo, rci.TrxId));
                            continue;//继续此Node 取出下一个
                            #endregion
                        }
                        else
                        {
                            if (rci.RecipeID == new string('0', eqp.Data.RECIPELEN))
                            {
                                #region Recipe="00"
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.Result = eRecipeCheckResult.NOCHECK;  //Waton modify 20150310
                                    rci.IsSend = true;
                                }
                                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BC Download {0}_RecipeRegisterValidationCommand Recipe ID =[{2}] NO CHECK.", rci.EQPNo, rci.TrxId, rci.RecipeID));

                                continue;
                                #endregion//continue;//继续此Node 取出下一个
                            }
                            if (eqp.Data.RECIPEREGVALIDATIONENABLED != "Y") //避免db欄位為null也加入CHECK
                            {
                                #region RECIPEREGVALIDATIONENABLED==N
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.Result = eRecipeCheckResult.NOCHECK;
                                    rci.IsSend = true;
                                }
                                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Equipment Recipe ID Validation Enable (N) Recipe ID =[{2}] NO CHECK.", rci.EQPNo, rci.TrxId, rci.RecipeID));

                                continue;
                                #endregion//continue;//继续此Node 取出下一个
                            }
                        }

                        lock (_recipeRegisterValidationQ)
                        {
                            if (_recipeRegisterValidationQ.ContainsKey(rci.EQPNo))
                            {
                                lock (_recipeRegisterValidationQ[rci.EQPNo])
                                    _recipeRegisterValidationQ[rci.EQPNo].Enqueue(rci);

                            }
                            else
                            {
                                Queue<RecipeCheckInfo> recipeCheckQueue = new Queue<RecipeCheckInfo>();
                                recipeCheckQueue.Enqueue(rci);
                                _recipeRegisterValidationQ.Add(rci.EQPNo, recipeCheckQueue);
                            }
                        }
                    }

                }

                DateTime startTime = DateTime.Now;
                while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["RECIPEREGISTERTIMEOUT"].GetInteger())
                {
                    Thread.Sleep(300);//阻塞呼叫的Function
                    bool finishflag = true;
                    foreach (RecipeCheckInfo rci in recipeCheckInfoList)
                    {
                        finishflag = rci.IsFinish;
                        if (finishflag == false)//发现没有完成的，跳出不用检查，让线程休息一下。
                            break;

                    }
                    if (finishflag)//全部检查完成。停止阻塞
                    {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[BCS -> EQP]=[{0}]RECIPE REGISTER VALIDATION COMMAND IS FINISH.", trxID));
                        break;
                    }
                }


                return true;
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }



        /// <summary>
        /// Recipe Register Validation From Other BCS
        /// Short Cut 和 Cell 跨Line 使用，有主线BCS 通过自己的SocketService 发送到 OtherLine的BCS上
        /// Other Line 的BCS的SocketService 将资料拆解后Invoke到自己的RecipeService 此处，主线会将
        /// Other Line的所有的RecipeCheckInfo 都传入，并包括不需要检查的MachineName. Other Line 需要自己
        /// 过滤掉不需要做Recipe Register 的机台，并上报NOCHECK
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="recipeCheckInfos">全部的RecipeCheckInfo</param>
        /// <param name="noCheckEqp">不需要做RecipeRegister的机台ID</param>
        /// <returns></returns>
        public bool RecipeRegisterValidationCommandFromBCS(string trxID, IList<RecipeCheckInfo> recipeCheckInfos, IList<string> noCheckEqp)
        {
            try
            {
                if (recipeCheckInfos == null)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[BCS -> EQP]=[{0}] RECIPE REGISTER VALIDATION COMMAND IS NULL.", trxID));
                    return false;
                }
                DateTime createTime = DateTime.Now;
                foreach (RecipeCheckInfo rci in recipeCheckInfos)
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);
                    rci.CreateTime = createTime;
                    rci.TrxId = trxID;//For Secs 需要使用

                    //Watson Add 20150302 For HVA EQP 
                    rci.Mode = (int)eRecipeCheckMode.Auto;  //"1：Auto ,2：Manual"

                    if (eqp == null)
                    {
                        #region Not Found Node
                        lock (rci)
                        {
                            rci.IsFinish = true;
                            rci.IsSend = true;
                            rci.Result = eRecipeCheckResult.MACHINENG;
                        }

                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BC Download {0}_RecipeRegisterValidationCommand Node not found", rci.EQPNo, rci.TrxId));

                        continue;
                        #endregion
                    }
                    else
                    {
                        if (eqp.File.CIMMode == eBitResult.OFF)
                        {
                            #region CIM OFF
                            lock (rci)
                            {
                                rci.IsFinish = true;
                                rci.Result = eRecipeCheckResult.CIMOFF;
                                rci.IsSend = true;
                            }

                            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BC Download {0}_RecipeRegisterValidationCommand CIM OFF", rci.EQPNo, rci.TrxId));
                            continue;//继续此Node 取出下一个
                            #endregion
                        }
                        else
                        {
                            if (rci.RecipeID == new string('0', eqp.Data.RECIPELEN))
                            {
                                #region Recipe="00"
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.Result = eRecipeCheckResult.ZERO;
                                    rci.IsSend = true;
                                }
                                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BC Download {0}_RecipeRegisterValidationCommand Recipe ID =[{2}] NO CHECK.", rci.EQPNo, rci.TrxId, rci.RecipeID));

                                continue;
                                #endregion//continue;//继续此Node 取出下一个
                            }
                            if (eqp.Data.RECIPEREGVALIDATIONENABLED != "Y") ////此欄位在Node DB中設定
                            {
                                #region RECIPEREGVALIDATIONENABLED==N
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.Result = eRecipeCheckResult.NOCHECK;
                                    rci.IsSend = true;
                                }
                                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Equipment Recipe ID Validation Enable (N) Recipe ID =[{2}] NO CHECK.", rci.EQPNo, rci.TrxId, rci.RecipeID));

                                continue;
                                #endregion//continue;//继续此Node 取出下一个
                            }
                        }
                    }

                    lock (_recipeRegisterValidationQ)
                    {
                        if (_recipeRegisterValidationQ.ContainsKey(rci.EQPNo))
                        {
                            lock (_recipeRegisterValidationQ[rci.EQPNo])
                            {
                                _recipeRegisterValidationQ[rci.EQPNo].Enqueue(rci);
                            }
                        }
                        else
                        {
                            Queue<RecipeCheckInfo> recipeCheckQueue = new Queue<RecipeCheckInfo>();
                            recipeCheckQueue.Enqueue(rci);
                            _recipeRegisterValidationQ.Add(rci.EQPNo, recipeCheckQueue);
                        }
                    }
                }

                DateTime startTime = DateTime.Now;
                while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["RECIPEREGISTERTIMEOUT"].GetInteger())
                {
                    Thread.Sleep(300);//阻塞呼叫的Function
                    bool finishflag = true;
                    foreach (RecipeCheckInfo rci in recipeCheckInfos)
                    {
                        finishflag = rci.IsFinish;
                        if (finishflag == false)//发现没有完成的，跳出不用检查，让线程休息一下。
                            break;
                    }
                    if (finishflag)//全部检查完成。停止阻塞
                    {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[BCS -> EQP]=[{0}]RECIPE REGISTER VALIDATION COMMAND IS FINISH.", trxID));
                        break;
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }
        #endregion

        #region Recipe Register Validation Command Reply
        /// <summary>
        /// PLC RecipeRegisterValidationCommandReply
        /// </summary>
        /// <param name="inputData"></param>
        public void RecipeRegisterValidationCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                if (bitResult == eBitResult.ON)
                {
                    //1. 移除Time
                    if (Timermanager.IsAliveTimer(string.Format(RecipeRegisterCommandTimeout, eqpNo)))
                    {
                        Timermanager.TerminateTimer(string.Format(RecipeRegisterCommandTimeout, eqpNo));
                    }
                    Trx recipeRegisterTrx = PLCAgent.GetTransactionFormat(string.Format(RecipeRegisterCommand, eqpNo)) as Trx;
                    if (recipeRegisterTrx != null)
                    {

                        recipeRegisterTrx.EventGroups[0].Events[0].IsDisable = true;
                        recipeRegisterTrx.EventGroups[0].Events[1].Items[0].Value = "0";
                        recipeRegisterTrx.TrackKey = inputData.TrackKey;
                        SendToPLC(recipeRegisterTrx);
                    }

                    //取得Reply 结果结束Recipe Register Command
                    RecipeCheckInfo rci = null;
                    if (_recipeRegisterValidationQ.ContainsKey(eqpNo))
                    {
                        if (_recipeRegisterValidationQ[eqpNo].Count > 0)
                        {
                            lock (_recipeRegisterValidationQ[eqpNo])
                                rci = _recipeRegisterValidationQ[eqpNo].Dequeue();
                            if (rci != null)
                            {
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.Result = inputData.EventGroups[0].Events[0].Items[0].Value.Equals("1") ? eRecipeCheckResult.OK : eRecipeCheckResult.NG;
                                }

                                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}]  {0}_RECIPEREGISTERVALIDATIONCOMMANDREPLY RESULT =[{2}]", rci.EQPNo, rci.TrxId, rci.Result));

                            }
                        }
                    }

                }
                else
                {
                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}]  {0}_RECIPEREGISTERVALIDATIONCOMMANDREPLY TRIGGER BIT [OFF].", eqpNo, inputData.TrackKey));

                }
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// SECS  Reicipe RegisterValidation Reply
        /// </summary>
        /// <param name="info"></param>
        public void RecipeRegisterValidationCommandReplyForSECS(RecipeCheckInfo info, string trxID)
        {
            try
            {
                if (Timermanager.IsAliveTimer(string.Format(RecipeRegisterCommandTimeout, info.EQPNo)))
                {
                    Timermanager.TerminateTimer(string.Format(RecipeRegisterCommandTimeout, info.EQPNo));
                }

                RecipeCheckInfo rci = null;
                if (_recipeRegisterValidationQ.ContainsKey(info.EQPNo))
                {

                    if (_recipeRegisterValidationQ[info.EQPNo].Count > 0)
                    {
                        lock (_recipeRegisterValidationQ[info.EQPNo])
                            rci = _recipeRegisterValidationQ[info.EQPNo].Dequeue();
                        if (rci != null)
                        {
                            lock (rci)
                            {
                                rci.IsFinish = true;
                                rci.Result = info.Result;
                                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}]  {0}_RECIPEREGISTERVALIDATIONCOMMANDREPLYFOR RESULT =[{2}]", rci.EQPNo, rci.TrxId, rci.Result));

                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// ShortCut Line Other BCS Reply RecipeRegisterValidationCommand 
        /// Other BCS 需要回复全部的RecipeCheckInfo的讯息，包括不需要检查的
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="recipeCheckInfos"></param>
        public void RecipeRegisterValidationCommandReplyForBCS(string trxID, string lineName, IList<RecipeCheckInfo> recipeCheckInfos)
        {
            try
            {
                StringBuilder log = new StringBuilder();
                string key = string.Format("{0}_{1}", lineName, trxID);
                string timerID = String.Format(RecipeRegisterCommandTimeout, key);
                if (Timermanager.IsAliveTimer(timerID))
                {
                    Timermanager.TerminateTimer(timerID);//停止计时
                }
                if (_otherLineRecipeRegister.ContainsKey(key))
                {
                    IList<RecipeCheckInfo> list = _otherLineRecipeRegister[key];
                    lock (_otherLineRecipeRegister)
                    {
                        _otherLineRecipeRegister.Remove(key);
                    }
                    foreach (RecipeCheckInfo info in list)
                    {
                        for (int i = 0; i < recipeCheckInfos.Count; i++)
                        {
                            if (info.EQPNo == recipeCheckInfos[i].EQPNo && info.RecipeID == recipeCheckInfos[i].RecipeID)
                            {
                                lock (info)
                                {
                                    info.Result = recipeCheckInfos[i].Result;
                                    info.IsFinish = true;
                                }

                                log.AppendFormat("{0};", info.ToString());
                                break;
                            }
                        }
                        //other BCS 没有回复直接填写NG;
                        if (info.IsFinish == false)
                        {
                            lock (info)
                            {
                                info.Result = eRecipeCheckResult.NG;
                                info.IsFinish = true;
                            }
                        }
                        log.AppendFormat("{0};", info.ToString());
                    }
                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                   string.Format("[LineName={0}] [BCS <- EQP]=[{1}] RECIPEREGISTERVALIDATIONCOMMANDREPLYFORBCS,RESULT =[{2}]",
                                           lineName, trxID, log.ToString()));
                }

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        /// <summary>
        /// Check Recipe Register Command is Empty
        /// </summary>
        /// <returns></returns>
        public bool CheckRecipeRegisterValidationCommand()
        {
            if (_queueRecipeChecCommand.Count == 0)
            {
                return true;
            }
            return false;

        }

        /// <summary>
        /// Check Recipe Parameter Command is Empty
        /// </summary>
        /// <returns></returns>
        public bool CheckRecipeParameterCommand()
        {
            if (_queueRecipeParamCommand.Count == 0)
            {
                return true; ;
            }
            return false;
        }
        #endregion

        #region Recipe Modify Report
        /// <summary>
        /// Recipe Modify Report
        /// </summary>
        /// <param name="inputData"></param>
        public void RecipeModifyReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult bitResult = (eBitResult)int.Parse(inputData[0][1][0].Value);
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                if (bitResult == eBitResult.ON)
                {
                    #region[拆出PLCAgent Data]
                    string recipeID = inputData[0][0][0].Value;
                    string versionNo = inputData[0][0][1].Value;
                    eModifyFlag modifyFlag = (eModifyFlag)int.Parse(inputData[0][0][2].Value);
                    string operatorID = inputData[0][0][3].Value;
                    #endregion

                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] RECCIPE MODIFY REPORT RECIPEID =[{2}],VERSION_NO =[{3}],MODIFYFLAG =[{4}]{5},OPERATOR =[{6}].",
                                            eqpNo, inputData.TrackKey, recipeID, versionNo, (int)modifyFlag, modifyFlag.ToString(), operatorID));
                    RecipeModifyReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);

                    RecipeCheckInfo rci = new RecipeCheckInfo(eqpNo, 0, 0, recipeID);
                    rci.TrxId = inputData.TrackKey;
                    //Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                    if (eqp != null)
                    {
                        if (modifyFlag.ToString() != "Delete")
                        {
                            rci.EqpID = eqp.Data.NODEID;
                            eRecipeCheckResult result = RecipeParameterChangeRequest(inputData.TrackKey, rci, eqp.Data.NODEID, eqp.Data.LINEID);
                            //eRecipeCheckResult result = RecipeParameterChangeRequest2(inputData.TrackKey, rci, eqp.Data.NODEID, eqp.Data.LINEID);
                            if (result == eRecipeCheckResult.NG)
                            {
                                //To Do Dowload CIM Message;
                                if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add By CELL 是不同IO 20160402
                                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), eqpNo, "Recipe Parameter MES Reply NG", "BCS", "0" });
                                else
                                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[]  { inputData.TrackKey, eqpNo, "Recipe Parameter MES Reply NG", "BCS" });
                            }
                        }
                    }
                }
                else
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] RECIPE MODIFY REPORT BIT [OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));

                    RecipeModifyReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                }
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);

                if (inputData[0][1][0].Value.Equals("1"))
                {
                    RecipeModifyReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        //public eRecipeCheckResult RecipeParameterChangeRequest2(string trxID, RecipeCheckInfo recipeCheck, string machieName, string lineName)
        //{
        //    try
        //    {
        //        RecipeCheckCommand rcc = new RecipeCheckCommand(trxID, eRecipeCheckCommandType.BCS);

        //        lock (_waitForMES)
        //        {
        //            _waitForMES.Add(trxID, rcc);//记录一个Command
        //        }
        //        DateTime createTime = DateTime.Now;
        //        recipeCheck.CreateTime = createTime;
        //        lock (_recipeParameterQ)
        //        {
        //            if (_recipeParameterQ.ContainsKey(recipeCheck.EQPNo))
        //            {
        //                _recipeParameterQ[recipeCheck.EQPNo].Enqueue(recipeCheck);
        //            }
        //            else
        //            {
        //                Queue<RecipeCheckInfo> recipeCheckQueue = new Queue<RecipeCheckInfo>();
        //                recipeCheckQueue.Enqueue(recipeCheck);
        //                _recipeParameterQ.Add(recipeCheck.EQPNo, recipeCheckQueue);
        //            }
        //        }
        //        DateTime startTime = DateTime.Now;
        //        while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["RECIPEPARAMETERTIMEOUT"].GetInteger())
        //        {
        //            Thread.Sleep(300);//阻塞呼叫的Function
        //            if (recipeCheck.IsFinish)
        //            {
        //                if (rcc.IsSend == false )
        //                {
        //                    //To Do Invoke MESService RecipeParameterChangeRequest  
        //                    Invoke(eServiceName.MESService, "RecipeParameterChangeRequest", new object[]{
        //                            trxID,
        //                            lineName,
        //                            machieName,
        //                            recipeCheck.RecipeID,
        //                            recipeCheck.Parameters
        //                    });
        //                    string timerId = string.Format("{0}_CheckRecipeParameterTimeout", trxID);
        //                    if (Timermanager.IsAliveTimer(timerId))
        //                    {
        //                        Timermanager.TerminateTimer(timerId);
        //                    }
        //                    Timermanager.CreateTimer(timerId, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeParameterRequestTimeoutForMES), trxID);
        //                    rcc.IsSend = true;
        //                }
        //                if (rcc.IsFinish)
        //                {
        //                    _waitForMES.Remove(trxID);
        //                    break;
        //                }
        //            }
        //        }
        //        return rcc.Result;

        //    }
        //    catch (System.Exception ex)
        //    {
        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return eRecipeCheckResult.NG;
        //    }
        //}

        public eRecipeCheckResult RecipeParameterChangeRequest(string trxID, RecipeCheckInfo recipeCheck, string machieName, string lineName)
        {
            try
            {
                RecipeCheckCommand rcc = new RecipeCheckCommand(trxID, eRecipeCheckCommandType.BCS);

                lock (_waitForMES)
                {
                    _waitForMES.Add(trxID, rcc);//记录一个Command
                }
                DateTime createTime = DateTime.Now;
                recipeCheck.CreateTime = createTime;
                lock (_recipeParameterQ)
                {
                    if (_recipeParameterQ.ContainsKey(recipeCheck.EQPNo))
                    {
                        _recipeParameterQ[recipeCheck.EQPNo].Enqueue(recipeCheck);
                    }
                    else
                    {
                        Queue<RecipeCheckInfo> recipeCheckQueue = new Queue<RecipeCheckInfo>();
                        recipeCheckQueue.Enqueue(recipeCheck);
                        _recipeParameterQ.Add(recipeCheck.EQPNo, recipeCheckQueue);
                    }
                }
                DateTime startTime = DateTime.Now;
                while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["RECIPEPARAMETERTIMEOUT"].GetInteger())
                {
                    Thread.Sleep(300);//阻塞呼叫的Function
                    if (recipeCheck.IsFinish)
                    {
                        if (recipeCheck.IsSend == true && recipeCheck.Result == eRecipeCheckResult.OK)
                        {
                            //To Do Invoke MESService RecipeParameterChangeRequest  
                            Invoke(eServiceName.MESService, "RecipeParameterChangeRequest", new object[]{
                                    trxID,
                                    lineName,
                                    machieName,
                                    recipeCheck.RecipeID,
                                    recipeCheck.Parameters
                            });

                            // For CELL
                            if (lineName.Contains(keyCELLPMTLINE.CBPMI) && (recipeCheck.EQPNo == "L2"))
                            {
                                Invoke(eServiceName.MESService, "RecipeParameterChangeRequest", new object[]{
                                    trxID,
                                    ParameterManager[keyCELLPTIParameter.CELL_PTI_LINEID].GetString(),
                                    machieName,
                                    recipeCheck.RecipeID,
                                    recipeCheck.Parameters}); 
                            }


                            string timerId = string.Format("{0}_CheckRecipeParameterTimeout", trxID);
                            if (Timermanager.IsAliveTimer(timerId))
                            {
                                Timermanager.TerminateTimer(timerId);
                            }
                            Timermanager.CreateTimer(timerId, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeParameterRequestTimeoutForMES), trxID);
                            recipeCheck.IsSend = true;
                        }
                        if (recipeCheck.IsFinish)
                        {
                            _waitForMES.Remove(trxID);
                            break;
                        }
                    }
                }
                return recipeCheck.Result;

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return eRecipeCheckResult.NG;
            }
        }
        /// <summary>
        /// Recipe Modify Report Reply
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <param name="bit"></param>
        /// <param name="trxID"></param>
        public void RecipeModifyReportReply(string eqpNo, eBitResult bit, string trxID)
        {
            try
            {
                Trx output = PLCAgent.GetTransactionFormat(string.Format("{0}_RecipeModifyReportReply", eqpNo)) as Trx;
                if (output != null)
                {
                    output.TrackKey = trxID;
                    output[0][0][0].Value = ((int)bit).ToString();
                    SendToPLC(output);

                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,Recipe Modify Report Reply Set Bit =[{2}].",
                        eqpNo, trxID, bit));
                    #region[Reply Time Out]
                    if (_timerManager.IsAliveTimer(string.Format(RecipeModifyReportTimeout, eqpNo)))
                    {
                        _timerManager.TerminateTimer(string.Format(RecipeModifyReportTimeout, eqpNo));
                    }
                    if (bit == eBitResult.ON)
                    {
                        _timerManager.CreateTimer(string.Format(RecipeModifyReportTimeout, eqpNo), false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(RecipeModifyReportReplyTimeout), trxID);
                    }
                    #endregion
                }
            }
            catch (System.Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RecipeModifyReportReplyTimeout(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP REPLY,RECIPE MODIFY REPORT TIMEOUT BIT [OFF].", 
                    sArray[0], trackKey));

                RecipeModifyReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (System.Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //20150711 Modify by Frank
        //private string ModifyFlagConvert(string modifyFlag)
        //{
        //    switch (modifyFlag)
        //    {
        //       case "1":
        //            return "Create";
        //        case "2":
        //            return "Modify";
        //        case "3":
        //            return "Delete";
        //        default:
        //           return "None";
        //    }
        //}

        #endregion

        #region RecipeIDCheckMode

        /// <summary>
        /// Recipe ID Check Mode
        /// </summary>
        /// <param name="inputData"></param>
        public void RecipeIDCheckMode(Trx inputData)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    LogError(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("Not found Node =[{0}]", eqpNo));
                    return;
                }
                lock (eqp)
                {
                    eqp.File.RecipeIDCheckMode = (eEnableDisable)int.Parse(inputData[0][0][0].Value);//(Enum.Parse(typeof(eEnableDisable), inputData.EventGroups[0].Events[0].Items[0].Value));
                }
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] RECIPEID CHECK MODE IS =[{2}].", eqpNo, inputData.TrackKey, eqp.File.RecipeIDCheckMode));
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region CurrentRecipeIDChangeReport

        /// <summary>
        /// 更新記億體資料
        /// </summary>
        /// <param name="inputData">同CurrentRecipeIDChangeReport(Trx inputData)</param>
        /// <param name="log">記錄動作者</param>
        public void CurrentRecipeIDChangeReportUpdate(Trx inputData, string log)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string recipeId = inputData.EventGroups[0].Events[0].Items[0].Value;
                recipeId = Regex.Replace(recipeId, @"[^\x21-\x7E]", " "); //过滤掉不可见字符 20150211 Tom

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("Can not find Node =[{0}].", eqpNo));

                if (eqp.File.CurrentRecipeID != recipeId.Trim())
                    eqp.File.CurrentRecipeID = recipeId.Trim(); //不刪空白 空白長度未知，可能會比對不成功

                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] {3} SET RECIPEID=[{2}]", eqp.Data.NODENO, inputData.TrackKey, recipeId, log));
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    CurrentRecipeIDChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        public void CurrentRecipeIDChangeReport(Trx inputData)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string recipeId = inputData.EventGroups[0].Events[0].Items[0].Value;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("Can not find Node =[{0}].", eqpNo));

                eBitResult bit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                CurrentRecipeIDChangeReportUpdate(inputData, "CurrentRecipeIDChangeReport");

                //if(eqp.File.CurrentRecipeID!=recipeId.Trim())
                //    eqp.File.CurrentRecipeID = recipeId.Trim(); //不刪空白 空白長度未知，可能會比對不成功

                if (inputData.IsInitTrigger == false)
                {
                    if (eBitResult.ON == bit)
                    {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CURRENT RECIPE ID CHANGE REPORT RECIPEID =[{2}].",
                                            eqpNo, inputData.TrackKey, recipeId));
                        CurrentRecipeIDChangeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
                    }
                    else
                    {

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CURRENT RECIPE ID CHANGE REPORT BIT [OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));

                        CurrentRecipeIDChangeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    }
                }

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    CurrentRecipeIDChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void CurrentRecipeIDChangeReportReply(string eqpNo, eBitResult bit, string trxID)
        {
            try
            {
                Trx output = PLCAgent.GetTransactionFormat(string.Format("{0}_CurrentRecipeIDChangeReportReply", eqpNo)) as Trx;
                if (output != null)
                {
                    output.TrackKey = trxID;
                    output.EventGroups[0].Events[0].Items[0].Value = ((int)bit).ToString();
                    SendToPLC(output);

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,Current Recipe ID Report Reply Set Bit =[{2}].",
                    eqpNo, trxID, bit));
                    if (_timerManager.IsAliveTimer(string.Format(CurrentRecipeIDChangeReportTimeout, eqpNo)))
                    {
                        _timerManager.TerminateTimer(string.Format(CurrentRecipeIDChangeReportTimeout, eqpNo));
                    }
                    if (bit == eBitResult.ON)
                    {
                        _timerManager.CreateTimer(string.Format(CurrentRecipeIDChangeReportTimeout, eqpNo), false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(CurrentRecipeIDChangeReportReplyTimeout), trxID);
                    }
                }
            }
            catch (System.Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CurrentRecipeIDChangeReportReplyTimeout(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP REPLY,CURRENT RECIPE ID CHANGE REPORT TIMEOUT SET BIT [OFF].", sArray[0], trackKey));

                CurrentRecipeIDChangeReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (System.Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Recipe Parameter Request Command

        private void RecipeParamProcess()
        {
            while (_isRuning)
            {
                Thread.Sleep(300);
                try
                {
                    DateTime now = DateTime.Now;
                    RecipeCheckInfo rci = null;

                    foreach (string key in _recipeParameterQ.Keys)
                    {

                        if (_recipeParameterQ[key].Count > 0)
                        {
                            lock (_recipeParameterQ[key])
                            {
                                rci = _recipeParameterQ[key].Peek();
                            }
                            if (rci != null)
                            {
                                #region BC Download Time Check
                                if (new TimeSpan(DateTime.Now.Ticks - rci.CreateTime.Ticks).TotalMilliseconds >= ParameterManager["RECIPEPARAMETERTIMEOUT"].GetInteger())
                                {
                                    lock (rci)
                                    {
                                        rci.IsFinish = true;
                                        rci.IsSend = true;
                                        rci.Result = eRecipeCheckResult.TIMEOUT;
                                    }
                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BC Download {0}_RecipeParameterRequestCommand Timeout", rci.EQPNo, rci.TrxId));
                                    lock (_recipeParameterQ)
                                        _recipeParameterQ[key].Dequeue();
                                    //continue; //继续此Node 取出下一个
                                }
                                #endregion
                                if (rci.IsSend == false && rci.IsFinish == false)
                                {
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);

                                    if (eqp != null)
                                    {
                                        #region Send To PLC
                                        eReportMode reportMode;
                                        Enum.TryParse<eReportMode>(eqp.Data.REPORTMODE, out reportMode);

                                        switch (reportMode)
                                        {
                                            case eReportMode.PLC:
                                            case eReportMode.PLC_HSMS:
                                                  //20151013 cy:雖然DRY(YAC)走的是PLC_HSMS,但他的Recipe Parameter要透過SECS去取得, 所以在這Hard Code.
                                                  if (eqp.Data.NODEATTRIBUTE.Equals("DRY"))
                                                  {
                                                        Invoke(eServiceName.CSOTSECSService, "TS7F25_H_FormattedProcessProgramRequest", new object[] { rci, rci.TrxId });
                                                        rci.IsSend = true;
                                                        string timerId = string.Format(RecipeParameterCommandTimeout, rci.EQPNo);
                                                        if (Timermanager.IsAliveTimer(timerId))
                                                        {
                                                              Timermanager.TerminateTimer(timerId);
                                                        }
                                                        Timermanager.CreateTimer(timerId, false, ParameterManager["RECIPEPARAMETER_T1"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeParameterRequestTimeoutForSECS), rci.TrxId);
                                                  }
                                                  else
                                                        if (CheckRecipeParameterRequestCommandReplyIsOff(rci.EQPNo)) {
                                                            RecipeParameterRequestCommmandForPLC(key, rci);
                                                        }                                                
                                                        break;
                                            case eReportMode.HSMS_CSOT:
                                            case eReportMode.HSMS_NIKON:
                                            case eReportMode.HSMS_PLC:
                                                RecipeParameterRequestCommandForSECS(reportMode, rci);
                                                //To Do NIKON_HSMS
                                                break;
                                            case eReportMode.RS232:
                                                //To Do RS232
                                                break;
                                            case eReportMode.RS485:
                                                //To Do Rs485
                                                break;

                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Not Found EQP
                                        lock (rci)
                                        {
                                            rci.IsFinish = true;
                                            rci.IsSend = true;
                                            rci.Result = eRecipeCheckResult.NG;
                                        }
                                        #endregion
                                    }
                                    break;//继续 下一个Node
                                }
                                else
                                    #region Dequeue Finish Command
                                    if (rci.IsFinish)
                                    {
                                        lock (_recipeParameterQ[key])
                                        {
                                            if(_recipeParameterQ[key].Count>0)
                                                _recipeParameterQ[key].Dequeue();
                                        }
                                        continue;
                                    }
                                    #endregion
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
        }

        #region Receive Recipe Parameter Request Command
        /// <summary>
        /// 此方法在上CST时做RecipeParameter Request 使用
        /// 此方法在收集完机台上报的Recipe Parameter后会直接发送MES Message  CheckRecipeParameter
        /// 并一直等到MES 回复或者Time out后才会返回
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="recipeCheckInfoList"> By  Line 放入需要做Recipe Parameter的集合</param>
        /// <param name="_noCheckEq"></param>
        /// <returns></returns>
        public eRecipeCheckResult RecipeParameterRequestCommand(string trxID, IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfoList, IList<string> noCheckEq, string PortName, string CarrierName)
        {
            try
            {
                Equipment eqp = null;
                if (recipeCheckInfoList == null)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS -> EQP]=[{0}] RECIPE PARAMETER REQUEST COMMAND IS NULL.", trxID));
                    return eRecipeCheckResult.NG;
                }

                #region 将Queue加入的Queue中等待
                RecipeCheckCommand rcc = new RecipeCheckCommand(trxID, eRecipeCheckCommandType.BCS);
                lock (_waitForMES)
                {
                    if (_waitForMES.ContainsKey(trxID))
                    {
                        _waitForMES.Remove(trxID);
                    }
                    _waitForMES.Add(trxID, rcc);//记录一个Command
                }

                lock (_queueRecipeParamCommand)//记录一下 有正在做的Command
                {
                    _queueRecipeParamCommand.Enqueue(rcc);
                }
                #endregion
                DateTime createTime = DateTime.Now;
                foreach (string lineName in recipeCheckInfoList.Keys)
                {
                    #region Short Cut Check Or Cell Cut Max
                    Line line = ObjectManager.LineManager.GetLine(lineName);
                    if (line == null)
                    {
                        RecipeParameterRequestCommandForBCS(trxID, lineName, recipeCheckInfoList[lineName], noCheckEq);
                        continue;
                    }
                    #endregion

                    #region 检查当前Line的Recipe
                    IList<RecipeCheckInfo> rciList = recipeCheckInfoList[lineName];

                    #region Add DB Setting NO Check EQP
                    foreach (Equipment nocheckeqp in ObjectManager.EquipmentManager.GetEQPsByLine(lineName))
                    {
                        if (nocheckeqp.Data.RECIPEPARAVALIDATIONENABLED == "N")
                        {
                            if (!noCheckEq.Contains(nocheckeqp.Data.NODEID))
                                noCheckEq.Add(nocheckeqp.Data.NODEID);
                        }
                    }
                    #endregion

                    foreach (RecipeCheckInfo rci in rciList)
                    {
                        rci.CreateTime = createTime;
                        rci.TrxId = trxID;//For Secs 需要使用
                        eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);

                        if (eqp != null)
                        {
                            rci.EqpID = eqp.Data.NODEID;
                            #region No Check 
                            foreach (string key in noCheckEq)//过滤掉不需要做Recipe Check的
                            {
                                if (key == eqp.Data.NODEID)
                                {
                                    rci.IsFinish = true;
                                    rci.IsSend = true;
                                    rci.Result = eRecipeCheckResult.NOCHECK;
                                    rci.CreateTime = createTime;
                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Recipe Parameter request Command No Check", rci.EQPNo, rci.TrxId));
                                    break;
                                }
                            }
                            #endregion
                            #region Recipe ="00"
                            if (rci.RecipeID.Equals(new string('0', eqp.Data.RECIPELEN)))
                            {
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.IsSend = true;
                                    rci.Result = eRecipeCheckResult.ZERO;  //Watson Modify 
                                    rci.CreateTime = createTime;
                                }
                                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Recipe Parameter request Command No Check Recipe is =[{2}]", rci.EQPNo, rci.TrxId, rci.RecipeID));
                                continue;
                            }
                            #endregion
                            #region CIMMode Off
                            if (eqp.File.CIMMode == eBitResult.OFF)
                            {
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.IsSend = true;
                                    rci.Result = eRecipeCheckResult.CIMOFF;
                                    rci.CreateTime = DateTime.Now;
                                }
                                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Recipe Parameter request Command CIM OFF.", rci.EQPNo, rci.TrxId));

                                continue;
                            }
                            #endregion
                            if (rci.IsFinish == false)
                            {
                                lock (_recipeParameterQ)
                                {
                                    if (_recipeParameterQ.ContainsKey(rci.EQPNo))
                                    {
                                        lock (_recipeParameterQ[rci.EQPNo])
                                            _recipeParameterQ[rci.EQPNo].Enqueue(rci);
                                    }
                                    else
                                    {
                                        Queue<RecipeCheckInfo> recipeCheckQueue = new Queue<RecipeCheckInfo>();
                                        recipeCheckQueue.Enqueue(rci);
                                        _recipeParameterQ.Add(rci.EQPNo, recipeCheckQueue);
                                    }
                                }
                            }
                        }
                        else // Eqp==null
                        {
                            rci.IsFinish = true;
                            rci.IsSend = true;
                            rci.Result = eRecipeCheckResult.CIMOFF;
                        }
                    }
                    #endregion
                }

                DateTime startTime = DateTime.Now;
                while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["RECIPEPARAMETERTIMEOUT"].GetInteger())
                {
                    Thread.Sleep(300);//阻塞呼叫的Function
                    #region Check Is Finish
                    bool finishflag = true;
                    IList<RecipeCheckInfo> rciList = null;
                    foreach (string line in recipeCheckInfoList.Keys)
                    {
                        rciList = recipeCheckInfoList[line];
                        foreach (RecipeCheckInfo rci in rciList)
                        {
                            finishflag = rci.IsFinish;
                            if (finishflag == false)//发现没有完成的，跳出不用检查，让线程休息一下。
                                break;
                        }
                        if (finishflag == false)
                            break;
                    }
                    if (finishflag)//全部检查完成。停止阻塞
                    {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[BCS -> EQP]=[{0}] Recipe Parameter Collect is Finish.", trxID));
                        break;
                    }
                }
                startTime = DateTime.Now;
                while(new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["MESTIMEOUT"].GetInteger())
                {
                    Thread.Sleep(300);//阻塞呼叫的Function
                    if (rcc.IsSend == false)
                    {
                        #region  Report To MES
                        if (RecipeParameterRequestToMES(trxID, recipeCheckInfoList, noCheckEq, PortName, CarrierName))
                        {
                            rcc.IsSend = true;
                        }
                        else
                        {
                            rcc.IsSend = true;
                            rcc.IsFinish = true;
                            rcc.Result = eRecipeCheckResult.NG;
                        }
                        #endregion
                    }
                    if (rcc.IsFinish)
                    {
                        #region Finish
                        lock (_waitForMES)
                        {
                            _waitForMES.Remove(trxID);
                        }
                        #endregion
                        break;
                    }
                    #endregion
                }
                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[BCS -> EQP]=[{0}] RECIPE PARAMETER REQUEST COMMAND IS FINISH.", trxID));
                lock (_queueRecipeParamCommand)//记录一下 有正在做的Command
                {
                    _queueRecipeParamCommand.Dequeue();
                }
                return rcc.Result;
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return eRecipeCheckResult.NG;
            }
            finally
            {
                if (_queueRecipeParamCommand.Count > 0)
                {
                    lock(_queueRecipeParamCommand)
                        _queueRecipeParamCommand.Dequeue();
                }
            }
        }

        public eRecipeCheckResult RecipeParameterRequestCommand(string trxID, IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfoList, IList<string> noCheckEq, Line line)
        {
            try
            {
                Equipment eqp = null;
                if (recipeCheckInfoList == null)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS -> EQP]=[{0}] RECIPE PARAMETER REQUEST COMMAND IS NULL.", trxID));
                    return eRecipeCheckResult.NG;
                }

                #region 将Queue加入的Queue中等待
                RecipeCheckCommand rcc = new RecipeCheckCommand(trxID, eRecipeCheckCommandType.BCS);
                lock (_waitForMES)
                {
                    if (_waitForMES.ContainsKey(trxID))
                    {
                        _waitForMES.Remove(trxID);
                    }
                    _waitForMES.Add(trxID, rcc);//记录一个Command
                }

                lock (_queueRecipeParamCommand)//记录一下 有正在做的Command
                {
                    _queueRecipeParamCommand.Enqueue(rcc);
                }
                #endregion
                DateTime createTime = DateTime.Now;
                foreach (string lineName in recipeCheckInfoList.Keys)
                {                    

                    #region 检查当前Line的Recipe
                    IList<RecipeCheckInfo> rciList = recipeCheckInfoList[lineName];

                    #region Add DB Setting NO Check EQP
                    foreach (Equipment nocheckeqp in ObjectManager.EquipmentManager.GetEQPsByLine(lineName))
                    {
                        if (nocheckeqp.Data.RECIPEPARAVALIDATIONENABLED == "N")
                        {
                            if (!noCheckEq.Contains(nocheckeqp.Data.NODEID))
                                noCheckEq.Add(nocheckeqp.Data.NODEID);
                        }
                    }
                    #endregion

                    foreach (RecipeCheckInfo rci in rciList)
                    {
                        rci.CreateTime = createTime;
                        rci.TrxId = trxID;//For Secs 需要使用
                        eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);

                        if (eqp != null)
                        {
                            rci.EqpID = eqp.Data.NODEID;
                            #region No Check
                            foreach (string key in noCheckEq)//过滤掉不需要做Recipe Check的
                            {
                                if (key == eqp.Data.NODEID)
                                {
                                    rci.IsFinish = true;
                                    rci.IsSend = true;
                                    rci.Result = eRecipeCheckResult.NOCHECK;
                                    rci.CreateTime = createTime;
                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Recipe Parameter request Command No Check", rci.EQPNo, rci.TrxId));
                                    break;
                                }
                            }
                            #endregion
                            #region Recipe ="00"
                            if (rci.RecipeID.Equals(new string('0', eqp.Data.RECIPELEN)))
                            {
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.IsSend = true;
                                    rci.Result = eRecipeCheckResult.ZERO;  //Watson Modify 
                                    rci.CreateTime = createTime;
                                }
                                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Recipe Parameter request Command No Check Recipe is =[{2}]", rci.EQPNo, rci.TrxId, rci.RecipeID));
                                continue;
                            }
                            #endregion
                            #region CIMMode Off
                            if (eqp.File.CIMMode == eBitResult.OFF)
                            {
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.IsSend = true;
                                    rci.Result = eRecipeCheckResult.CIMOFF;
                                    rci.CreateTime = DateTime.Now;
                                }
                                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Recipe Parameter request Command CIM OFF.", rci.EQPNo, rci.TrxId));

                                continue;
                            }
                            #endregion
                            if (rci.IsFinish == false)
                            {
                                lock (_recipeParameterQ)
                                {
                                    if (_recipeParameterQ.ContainsKey(rci.EQPNo))
                                    {
                                        lock (_recipeParameterQ[rci.EQPNo])
                                            _recipeParameterQ[rci.EQPNo].Enqueue(rci);
                                    }
                                    else
                                    {
                                        Queue<RecipeCheckInfo> recipeCheckQueue = new Queue<RecipeCheckInfo>();
                                        recipeCheckQueue.Enqueue(rci);
                                        _recipeParameterQ.Add(rci.EQPNo, recipeCheckQueue);
                                    }
                                }
                            }
                        }
                        else // Eqp==null
                        {
                            rci.IsFinish = true;
                            rci.IsSend = true;
                            rci.Result = eRecipeCheckResult.CIMOFF;
                        }
                    }
                    #endregion
                }

                DateTime startTime = DateTime.Now;
                while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["RECIPEPARAMETERTIMEOUT"].GetInteger())
                {
                    Thread.Sleep(300);//阻塞呼叫的Function
                    #region Check Is Finish
                    bool finishflag = true;
                    IList<RecipeCheckInfo> rciList = null;
                    foreach (string recipeline in recipeCheckInfoList.Keys)
                    {
                        rciList = recipeCheckInfoList[recipeline];
                        foreach (RecipeCheckInfo rci in rciList)
                        {
                            finishflag = rci.IsFinish;
                            if (finishflag == false)//发现没有完成的，跳出不用检查，让线程休息一下。
                                break;
                        }
                        if (finishflag == false)
                            break;
                    }
                    if (finishflag)//全部检查完成。停止阻塞
                    {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[BCS -> EQP]=[{0}] Recipe Parameter Collect is Finish.", trxID));
                        break;
                    }
                }
                startTime = DateTime.Now;
                while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["MESTIMEOUT"].GetInteger())
                {
                    Thread.Sleep(300);//阻塞呼叫的Function
                    if (rcc.IsSend == false)
                    {
                        #region  Report To MES
                        if (RecipeParameterRequestToMES(trxID, recipeCheckInfoList, noCheckEq, line))
                        {
                            rcc.IsSend = true;
                        }
                        else
                        {
                            rcc.IsSend = true;
                            rcc.IsFinish = true;
                            rcc.Result = eRecipeCheckResult.NG;
                        }
                        #endregion
                    }
                    if (rcc.IsFinish)
                    {
                        #region Finish
                        lock (_waitForMES)
                        {
                            _waitForMES.Remove(trxID);
                        }
                        #endregion
                        break;
                    }
                    #endregion
                }
                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[BCS -> EQP]=[{0}] RECIPE PARAMETER REQUEST COMMAND IS FINISH.", trxID));
                //20151222 cy:交給下面finally做就好
                //lock (_queueRecipeParamCommand)//记录一下 有正在做的Command
                //{
                //    _queueRecipeParamCommand.Dequeue();
                //}
                return rcc.Result;
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return eRecipeCheckResult.NG;
            }
            finally
            {
                if (_queueRecipeParamCommand.Count > 0)
                {
                    lock (_queueRecipeParamCommand)
                        _queueRecipeParamCommand.Dequeue();
                }
            }
        }

        /// <summary>
        /// Invoke MES Service CheckRecipeParameter Message 
        /// Create MES Time out Timer ID=TrxID_CheckRecipeParameterReply
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="recipeCheckInfoList"></param>
        /// <param name="noCheckEq"></param>
        /// <param name="PortName"></param>
        /// <param name="CarrierName"></param>
        /// <returns></returns>
        private bool RecipeParameterRequestToMES(string trxID, IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfoList, IList<string> noCheckEq, string PortName, string CarrierName)
        {
            try
            {
                Port port = ObjectManager.PortManager.GetPort(PortName);
                if (port != null)
                {
                    Invoke(eServiceName.MESService, "CheckRecipeParameter", new object[] { trxID, port, recipeCheckInfoList });
                    string timerId = string.Format("{0}_CheckRecipeParameterReply", trxID);
                    if (Timermanager.IsAliveTimer(timerId))
                    {
                        Timermanager.TerminateTimer(timerId);
                    }
                    Timermanager.CreateTimer(timerId, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeParameterRequestTimeoutForMES), trxID);
                    return true;
                }
                else
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod() + "()", string.Format("NOT FOUND PORT OBJECT PORTNAME =[{0}]", PortName));
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        private bool RecipeParameterRequestToMES(string trxID, IDictionary<string, IList<RecipeCheckInfo>> recipeCheckInfoList, IList<string> noCheckEq, Line line)
        {
            try
            {
                if (line != null)
                {
                    Invoke(eServiceName.MESService, "CheckRecipeParameter", new object[] { trxID, line, recipeCheckInfoList });
                    string timerId = string.Format("{0}_CheckRecipeParameterReply", trxID);
                    if (Timermanager.IsAliveTimer(timerId))
                    {
                        Timermanager.TerminateTimer(timerId);
                    }
                    Timermanager.CreateTimer(timerId, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeParameterRequestTimeoutForMES), trxID);
                    return true;
                }
                else
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod() + "()", string.Format("NOT FOUND PORT OBJECT LINENAME =[{0}]", line.Data.LINEID));
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// Receive Recipe Parameter Request Command  Form BCS ,
        /// 此处只对本Line的机台做Recipe Parameter Request ,并且不会发送MES Message 。
        /// 此方法在收集完机台上报的Recipe Parameter后才会返回并将结果填入 RecipeCheckInfo中
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="recipeCheckInfoList"></param>
        /// <param name="noCheckEqp"></param>
        /// <returns></returns>
        public bool RecipeParameterRequestCommandFromBCS(string trxID, IList<RecipeCheckInfo> recipeCheckInfoList, IList<string> noCheckEqp)
        {
            try
            {
                if (recipeCheckInfoList == null)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[BCS -> EQP]=[{0}] RECIPE PARAMETER REQUEST COMMAND IS NULL.", trxID));
                    return false;
                }
                //记录有Recipe Parameter 在做
                RecipeCheckCommand rcc = new RecipeCheckCommand(trxID, eRecipeCheckCommandType.BCS);
                lock(_queueRecipeParamCommand)
                    _queueRecipeParamCommand.Enqueue(rcc);
                DateTime createTime = DateTime.Now;
                foreach (RecipeCheckInfo rci in recipeCheckInfoList)
                {
                    rci.TrxId = trxID;
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);
                    //Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);


                    if (eqp != null)
                    {
                        rci.EqpID = eqp.Data.NODEID;
                        rci.CreateTime = createTime;
                        //Watson Add 20150302 For HVA EQP 
                        rci.Mode = (int)eRecipeCheckMode.Auto;  //"1：Auto ,2：Manual"

                        if (eqp.File.CIMMode == eBitResult.OFF)
                        {
                            #region CIMMode=off
                            lock (rci)
                            {
                                rci.IsFinish = true;
                                rci.IsSend = true;
                                rci.Result = eRecipeCheckResult.CIMOFF;
                                rci.CreateTime = DateTime.Now;
                            }
                            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                  string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Recipe Parameter request Command CIM OFF.", rci.EQPNo, rci.TrxId));
                            continue;
                        #endregion
                        }
                        else
                        {
                            #region  Recipe=00
                            if (rci.RecipeID.Equals(new string('0', eqp.Data.RECIPELEN)))
                            {
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.IsSend = true;
                                    rci.Result = eRecipeCheckResult.ZERO;
                                    rci.CreateTime = createTime;
                                }
                                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Recipe Parameter request Command No Check Recipe is =[{2}]", rci.EQPNo, rci.TrxId, rci.RecipeID));

                                continue;
                            }
                            #endregion
                            
                        }
                        

                        lock (_recipeParameterQ)
                        {
                            if (_recipeParameterQ.ContainsKey(rci.EQPNo))
                            {
                                lock (_recipeParameterQ[rci.EQPNo])
                                    _recipeParameterQ[rci.EQPNo].Enqueue(rci);
                            }
                            else
                            {
                                Queue<RecipeCheckInfo> recipeCheckQueue = new Queue<RecipeCheckInfo>();
                                recipeCheckQueue.Enqueue(rci);
                                _recipeParameterQ.Add(rci.EQPNo, recipeCheckQueue);
                            }
                        }
                    }
                    else
                    {
                        rci.IsFinish = true;
                        rci.IsSend = true;
                        rci.Result = eRecipeCheckResult.MACHINENG;
                    }
                }

                DateTime startTime = DateTime.Now;
                while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["RECIPEPARAMETERTIMEOUT"].GetInteger())
                {
                    Thread.Sleep(300);//阻塞呼叫的Function
                    bool finishflag = true;
                    foreach (RecipeCheckInfo rci in recipeCheckInfoList)
                    {
                        finishflag = rci.IsFinish;
                        if (finishflag == false)//发现没有完成的，跳出不用检查，让线程休息一下。
                            break;
                    }
                    if (finishflag)//全部检查完成。停止阻塞
                    {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[BCS -> EQP]=[{0}]RECIPE PARAMETER REQUEST COMMAND IS FINISH.", trxID));
                        break;
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
            finally
            {
                if (_queueRecipeParamCommand.Count > 0)
                {
                    lock (_queueRecipeParamCommand)
                        _queueRecipeParamCommand.Dequeue();

                }

            }
        }

        /// <summary>
        /// 此处给本机OPI或者MES OPI 下Recipe  Parameter Request 使用
        /// 此方法会在收集完机台的回复后将结果放到RecipeCheckInfo中并返回
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="recipeCheckInfoList"></param>
        /// <returns></returns>
        public bool RecipeParametreRequestCommandFromUI(string trxID, IList<RecipeCheckInfo> recipeCheckInfoList)
        {
            try
            {
                if (CheckRecipeParameterCommand()==false)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[BCS -> EQP]=[{0}] RECIPE SERVICE HAVE PARAMETER COMMANND BCS DO NOT EXECUTE RECIPE PARAMETER REQUEST.", trxID));
                    return false;
                }
                
                if (recipeCheckInfoList == null)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[BCS -> EQP]=[{0}] RECIPE PARAMETER REQUEST COMMAND IS NULL.", trxID));
                    return false;
                }
                RecipeCheckCommand rcc = new RecipeCheckCommand(trxID, eRecipeCheckCommandType.BCS);
                lock (_queueRecipeParamCommand)
                    _queueRecipeParamCommand.Enqueue(rcc);
                DateTime createTime = DateTime.Now;
                foreach (RecipeCheckInfo rci in recipeCheckInfoList)
                {
                    rci.TrxId = trxID;
                    rci.CreateTime = createTime;
                    //Watson Add 20150302 For HVA EQP 
                    rci.Mode = (int)eRecipeCheckMode.Manual;  //"1：Auto ,2：Manual"

                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);

                    if (eqp != null)
                    {
                        rci.EqpID = eqp.Data.NODEID;

                        if (rci.RecipeID.Equals(new string('0', eqp.Data.RECIPELEN)))
                        {
                            rci.IsFinish = true;
                            rci.IsSend = true;
                            rci.Result = eRecipeCheckResult.NOCHECK;
                            rci.CreateTime = createTime;
                            continue;
                        }
                        if (eqp.File.CIMMode == eBitResult.OFF)
                        {
                            rci.IsFinish = true;
                            rci.IsSend = true;
                            rci.Result = eRecipeCheckResult.CIMOFF;
                            rci.CreateTime = DateTime.Now;
                            continue;
                        }

                        #region Add DB Setting NO Check EQP
                        if (eqp.Data.RECIPEPARAVALIDATIONENABLED != "Y") ////此欄位在Node DB中設定
                        {
                            #region RECIPEPARAVALIDATIONENABLED==N
                            lock (rci)
                            {
                                rci.IsFinish = true;
                                rci.Result = eRecipeCheckResult.NOCHECK;
                                rci.IsSend = true;
                            }
                            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQUIPMENT RECIPE PARAMETER VALIDATE ENABLE (N) RECIPE ID =[{2}] NO CHECK.", rci.EQPNo, rci.TrxId, rci.RecipeID));
                            continue;
                            #endregion
                        }
                        #endregion

                        lock (_recipeParameterQ)
                        {
                            if (_recipeParameterQ.ContainsKey(rci.EQPNo))
                            {
                                lock (_recipeParameterQ[rci.EQPNo])
                                    _recipeParameterQ[rci.EQPNo].Enqueue(rci);
                            }
                            else
                            {
                                Queue<RecipeCheckInfo> recipeCheckQueue = new Queue<RecipeCheckInfo>();
                                recipeCheckQueue.Enqueue(rci);
                                _recipeParameterQ.Add(rci.EQPNo, recipeCheckQueue);
                            }
                        }
                    }
                    else
                    {
                        rci.IsFinish = true;
                        rci.IsSend = true;
                        rci.Result = eRecipeCheckResult.MACHINENG;
                    }
                }

                DateTime startTime = DateTime.Now;
                while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["RECIPEPARAMETERTIMEOUT"].GetInteger())
                {
                    Thread.Sleep(300);//阻塞呼叫的Function
                    bool finishflag = true;
                    foreach (RecipeCheckInfo rci in recipeCheckInfoList)
                    {
                        finishflag = rci.IsFinish;
                        if (finishflag == false)//发现没有完成的，跳出不用检查，让线程休息一下。
                            break;
                    }
                    if (finishflag)//全部检查完成。停止阻塞
                    {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[BCS -> EQP]=[{0}]RECIPE PARAMETER REQUEST COMMAND IS FINISH.", trxID));
                        break;
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
            finally
            {
                if (_queueRecipeParamCommand.Count > 0)
                {
                    lock (_queueRecipeParamCommand)
                        _queueRecipeParamCommand.Dequeue();

                }
            }
        }
        #endregion

        #region Download Recipe Parameter
        /// <summary>
        /// Download Current Line Recipe Parameter Request For PLC
        /// Monitor Equipment T1 Timeout TimeID= equipmentNo_RecipeParameterRequestCommandTimeout
        /// </summary>
        /// <param name="key"></param>
        /// <param name="rci"></param>
        private void RecipeParameterRequestCommmandForPLC(string key, RecipeCheckInfo rci)
        {
            try
            {
                Trx trx = PLCAgent.GetTransactionFormat(string.Format(RecipeParameterCommand, rci.EQPNo)) as Trx;
                if (trx != null)
                {

                    //AC

                    trx.EventGroups[0].Events[0].Items[0].Value = rci.RecipeID.PadRight(12, ' ');
                    //CELL
                    if (ObjectManager.EquipmentManager.GetEQP(rci.EQPNo) != null)
                    {
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(rci.EQPNo);
                        if (ObjectManager.LineManager.GetLine(eqp.Data.LINEID).Data.FABTYPE == eFabType.CELL.ToString())
                        {
                            trx.EventGroups[0].Events[0].Items[0].Value = rci.Mode.ToString();
                            trx.EventGroups[0].Events[0].Items[2].Value = rci.RecipeID.PadRight(12, ' ');
                            trx.EventGroups[0].Events[0].Items[1].Value = rci.PortNo.ToString();
                        }
                        else
                        {
                            trx.EventGroups[0].Events[0].Items[1].Value = "000000000000";  //不要清掉（以后如果MES管控versionno，再修改逻辑）
                            trx.EventGroups[0].Events[0].Items[2].Value = rci.PortNo.ToString(); //modify for AC Yang  20160929
                        }
                    }                 

                    trx.EventGroups[0].Events[1].Items[0].Value = "1";
                    trx.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    trx.TrackKey = rci.TrxId;
                    SendToPLC(trx);
                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] RECIPE PARAMETER REQUEST RECIPEID =[{2}]",
                                    rci.EQPNo, rci.TrxId, rci.RecipeID));
                    lock (rci) rci.IsSend = true;

                    string timerId = string.Format(RecipeParameterCommandTimeout, rci.EQPNo);
                    if (Timermanager.IsAliveTimer(timerId))
                    {
                        Timermanager.TerminateTimer(timerId);
                    }
                    Timermanager.CreateTimer(timerId, false, ParameterManager["RECIPEPARAMETER_T1"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeParameterRequestTimeoutForPLC), rci.TrxId);
                    Thread.Sleep(3);//此处停止一下
                }
                else
                {
                    lock (rci)
                    {
                        rci.Result = eRecipeCheckResult.NG;
                        rci.IsFinish = true;
                        rci.IsSend = true;
                    }

                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] is not found {0}_RECIPEPARAMETERREQUESTCOMMAND TRX", rci.EQPNo, rci.TrxId));
                }

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }

        /// <summary>
        /// Download Current Line Recipe Parameter Request For SECS
        /// Monitor Equipment T1 Timeout timeid=equipemntNo_RecipeParameterRequestCommandTimeout
        /// </summary>
        /// <param name="reportMode"></param>
        /// <param name="rci"></param>
        private void RecipeParameterRequestCommandForSECS(eReportMode reportMode, RecipeCheckInfo rci)
        {
            try
            {
                switch (reportMode)
                {
                    case eReportMode.HSMS_CSOT:
                    case eReportMode.HSMS_PLC:
                        Invoke(eServiceName.CSOTSECSService, "TS7F25_H_FormattedProcessProgramRequest", new object[] { rci, rci.TrxId });
                        break;
                    case eReportMode.HSMS_NIKON:
                        Invoke(eServiceName.NikonSECSService, "TS7F25_H_FormattedProcessProgramRequest", new object[] { rci, rci.TrxId });
                        break;
                }
                rci.IsSend = true;
                string timerId = string.Format(RecipeParameterCommandTimeout, rci.EQPNo);
                if (Timermanager.IsAliveTimer(timerId))
                {
                    Timermanager.TerminateTimer(timerId);
                }
                Timermanager.CreateTimer(timerId, false, ParameterManager["RECIPEPARAMETER_T1"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeParameterRequestTimeoutForSECS), rci.TrxId);

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Download Recipe Parameter Request Command For BCS
        /// for Short Cut or Cell Cut Max Line  Sent Recipe Parameter Request Command To Other Line BCS
        /// Monitor RECIPEPARAMETERBCSTIMEOUT TimerID=LineName_trxID
        /// </summary>
        /// <param name="trxid"></param>
        /// <param name="lineName"></param>
        /// <param name="recipeCheckInfos"></param>
        /// <param name="noCheckList"></param>
        private void RecipeParameterRequestCommandForBCS(string trxid, string lineName, IList<RecipeCheckInfo> recipeCheckInfos, IList<string> noCheckList)
        {
            try
            {
                string key = string.Format("{0}_{1}", lineName, trxid);
                if (_otherLineRecipeParam.ContainsKey(key))
                {
                    _otherLineRecipeParam.Remove(key);
                }
                _otherLineRecipeParam.Add(key, recipeCheckInfos);

                foreach (RecipeCheckInfo info in recipeCheckInfos)
                {
                    info.TrxId = trxid;
                }
                string timerId = string.Format(RecipeParameterCommandTimeout, key);
                if (Timermanager.IsAliveTimer(timerId))
                {
                    Timermanager.TerminateTimer(timerId);
                }
                Timermanager.CreateTimer(timerId, false, ParameterManager["RECIPEPARAMETERBCSTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(RecipeParameterRequestTimeoutForBCS), trxid);
                //To Do Invoke SocketService 
                Invoke(eServiceName.ActiveSocketService, "RecipeParameterRequest", new object[] 
                {
                    trxid,
                    lineName,
                    recipeCheckInfos,
                    noCheckList
                });
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("LINENAME={0} [BCS -> BCS]=[{1}] SEND RECIPE PARAMETER REQUEST COMMAND TO OTHER LINE.", lineName, trxid));

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Recipe Parametr Timeout
        /// <summary>
        /// Handler Recipe Parameter Request Timeout For PLC
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        private void RecipeParameterRequestTimeoutForPLC(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string eqpNo = sArray[0];
                if (Timermanager.IsAliveTimer(tmp))
                {
                    Timermanager.TerminateTimer(tmp);
                }
                Trx trx = PLCAgent.GetTransactionFormat(string.Format(RecipeParameterCommand, eqpNo)) as Trx;
                trx.EventGroups[0].Events[0].IsDisable = true;
                trx.EventGroups[0].Events[1].Items[0].Value = "0";
                trx.TrackKey = trackKey;
                SendToPLC(trx);
                RecipeCheckInfo rci = null;
                if (_recipeParameterQ.ContainsKey(eqpNo))
                {
                    lock (_recipeParameterQ[eqpNo])
                    {
                        if (_recipeParameterQ[eqpNo].Count > 0)
                        {
                            rci = _recipeParameterQ[eqpNo].Dequeue();
                            if (rci != null)
                            {
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.Result = eRecipeCheckResult.TIMEOUT;
                                }
                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] RECIPE PARAMETER REQUEST COMMAND EQUIPMENT REPLY TIIMEOUT RECIPEID =[{2}]  SET Bit [OFF].",
                                    sArray[0], trackKey, rci.RecipeID));

                            }
                        }
                    }
                }

                //Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //   string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BCS Clear, Recipe Parameter Request Command Equipemnt Reply Timeout Set Bit =[OFF].", sArray[0], trackKey));

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Handler Recipe Parameter Request Timeout For BCS
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        private void RecipeParameterRequestTimeoutForBCS(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string lineName = sArray[1];
                if (Timermanager.IsAliveTimer(tmp))
                {
                    Timermanager.TerminateTimer(tmp);
                }
                //此处的Key 值使用 Line +TrxiD
                string key = string.Format("{0}_{1}", lineName, trackKey);
                lock (_otherLineRecipeParam)
                {
                    if (_otherLineRecipeParam.ContainsKey(key))
                    {
                        IList<RecipeCheckInfo> recipeCheckInfos = _otherLineRecipeParam[key];
                        foreach (RecipeCheckInfo info in recipeCheckInfos)
                        {
                            lock (info)
                            {
                                info.IsSend = true;
                                info.IsFinish = true;
                                info.Result = eRecipeCheckResult.TIMEOUT;
                            }
                        }
                        _otherLineRecipeParam.Remove(key);//从集合中删除已经 Timeout 的别Line的Recipe Parameter Command
                    }
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                 string.Format("[LineName={0}] [BCS -> EQP]=[{1}] BCS CLEAR, RECIPE PARAMETER REQUEST COMMAND EQUIPMENT REPLY TIMEOUT.", lineName, trackKey));

                }
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RecipeParameterRequestTimeoutForMES(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();

                if (Timermanager.IsAliveTimer(tmp))
                {
                    Timermanager.TerminateTimer(tmp);
                }
                lock (_waitForMES)
                {
                    if (_waitForMES.ContainsKey(trackKey))
                    {
                        _waitForMES[trackKey].Result = eRecipeCheckResult.TIMEOUT;
                        _waitForMES[trackKey].IsFinish = true;
                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LineName={0}] [BCS -> MES] =[{1}] MES REPLY CHECKRECCIPEPARAMETER TIMEOUT.", Workbench.ServerName, trackKey));
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Handler Recipe Parameter Request Timeout For SECS
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        private void RecipeParameterRequestTimeoutForSECS(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string eqpNo = sArray[0];
                if (Timermanager.IsAliveTimer(tmp))
                {
                    Timermanager.TerminateTimer(tmp);
                }
                RecipeCheckInfo rci = null;
                lock (_recipeParameterQ)
                {
                    if (_recipeParameterQ.ContainsKey(eqpNo))
                    {
                        if (_recipeParameterQ[eqpNo].Count > 0)
                        {
                            rci = _recipeParameterQ[eqpNo].Dequeue();
                            if (rci != null)
                            {
                                lock (rci)
                                {
                                    rci.IsFinish = true;
                                    rci.Result = eRecipeCheckResult.TIMEOUT;
                                }
                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] RECIPE PARAMETER COMMAND EQUIPMENT REPLY TIMEOUT RECIPEID =[{2}].",
                                    sArray[0], trackKey, rci.RecipeID));
                                return;
                            }
                        }
                    }
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BCS CLEAR, RECIPE PARAMETER COMMAND EQUIPMENT REPLY TIMEOUT ..", sArray[0], trackKey));

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region Recipe Parameter Reply
        /// <summary>
        /// Current Line Recipe Parameter Request Reply For PLC
        /// </summary>
        /// <param name="inputData"></param>
        public void RecipeParameterRequestCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                if (bitResult == eBitResult.ON)
                {
                    //1. 移除Time
                    if (Timermanager.IsAliveTimer(string.Format(RecipeParameterCommandTimeout, eqpNo)))
                    {
                        Timermanager.TerminateTimer(string.Format(RecipeParameterCommandTimeout, eqpNo));
                    }
                    Trx trx = PLCAgent.GetTransactionFormat(string.Format(RecipeParameterCommand, eqpNo)) as Trx;
                    if (trx != null)
                    {
                        trx.EventGroups[0].Events[0].IsDisable = true;
                        trx.EventGroups[0].Events[1].Items[0].Value = "0";
                        trx.TrackKey = inputData.TrackKey;
                        SendToPLC(trx);
                    }

                    //取得Reply 结果结束Recipe Parameter  Command
                    RecipeCheckInfo rci = null;
                    lock (_recipeParameterQ)
                    {
                        if (_recipeParameterQ.ContainsKey(eqpNo))
                        {
                            if (_recipeParameterQ[eqpNo].Count > 0)
                            {
                                rci = _recipeParameterQ[eqpNo].Dequeue();
                                if (rci != null)
                                {
                                    lock (rci)
                                    {
                                        if (GetRecipeParameter(rci, inputData.EventGroups[0].Events[0].RawData))//Decode Recipe Parameter
                                        {
                                            rci.Result = eRecipeCheckResult.OK;
                                            rci.IsFinish = true;
                                        }
                                        else
                                        {
                                            rci.Result = eRecipeCheckResult.NG;
                                            rci.IsFinish = true;
                                        }
                                    }
                                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    //string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] RECIPE PARAMETER REQUESTCOMMAND REPLY.", rci.EQPNo, rci.TrxId));
                                        //T2 T3 ISSUE 以EQP Recipe Parameter Command Reply 觸發的時間當記錄的Trx Key。sy edit 20160418
                                                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] RECIPE PARAMETER REQUESTCOMMAND REPLY.", rci.EQPNo, inputData.TrackKey));

                                }
                            }
                        }
                    }
                }
                else
                {
                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] RECIPE PARAMETER REQUESTCOMMAND REPLY TRIGGER BIT OFF.", eqpNo, inputData.TrackKey));

                }
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }

        /// <summary>
        /// Current Line Recipe Parameter Request Repy For SECS
        /// </summary>
        /// 
        public void RecipeParameterRequestCommandReplyForSECS(RecipeCheckInfo info, string trxID)
        {
            try
            {
                if (Timermanager.IsAliveTimer(string.Format(RecipeParameterCommandTimeout, info.EQPNo)))
                {
                    Timermanager.TerminateTimer(string.Format(RecipeParameterCommandTimeout, info.EQPNo));
                }
                RecipeCheckInfo rci = null;
                lock (_recipeParameterQ)
                {
                    if (_recipeParameterQ.ContainsKey(info.EQPNo))
                    {
                        if (_recipeParameterQ[info.EQPNo].Count > 0)
                        {
                            rci = _recipeParameterQ[info.EQPNo].Dequeue();
                            if (rci != null)
                            {
                                lock (rci)
                                {
                                    rci.Parameters = info.Parameters;
                                    rci.IsFinish = true;
                                    rci.Result = info.Result;
                                }
                            }
                        }
                    }
                }
                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] RECIPE PARAMETER REQUEST COMMAND REPLY FOR SECS.", rci.EQPNo, rci.TrxId));

            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Other Line Recipe Parameter  Request Reply For BCS --> SocketService
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="recipeCheckInfos"></param>
        public void RecipeParameterRequestCommandReplyForBCS(string trxID, string lineName, IList<RecipeCheckInfo> recipeCheckInfos)
        {
            try
            {

                StringBuilder log = new StringBuilder();
                string key = string.Format("{0}_{1}", lineName, trxID);
                string timerID = String.Format(RecipeParameterCommandTimeout, key);
                if (Timermanager.IsAliveTimer(timerID))
                {
                    Timermanager.TerminateTimer(timerID);//停止计时
                }
                lock (_otherLineRecipeParam)
                {
                    if (_otherLineRecipeParam.ContainsKey(key))
                    {
                        IList<RecipeCheckInfo> list = _otherLineRecipeParam[key];

                        foreach (RecipeCheckInfo info in list)
                        {
                            for (int i = 0; i < recipeCheckInfos.Count; i++)
                            {
                                if (info.EQPNo == recipeCheckInfos[i].EQPNo && info.RecipeID == recipeCheckInfos[i].RecipeID)
                                {
                                    lock (info)
                                    {
                                        info.EqpID = recipeCheckInfos[i].EqpID;
                                        info.Parameters = recipeCheckInfos[i].Parameters;
                                        info.Result = recipeCheckInfos[i].Result;
                                        info.IsSend = recipeCheckInfos[i].IsSend;
                                        info.IsFinish = recipeCheckInfos[i].IsFinish;
                                    }
                                    //log.AppendFormat("{0};", info.ToString());
                                    break;
                                }
                            }
                            //other BCS 没有回复直接填写NG;
                            if (info.IsFinish == false)
                            {
                                lock (info)
                                {
                                    info.Result = eRecipeCheckResult.NG;
                                    info.IsFinish = true;
                                }
                            }

                        }

                        _otherLineRecipeParam.Remove(key);
                    }
                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                   string.Format("[LineName={0}] [BCS <- EQP]=[{1}] RECIPE PARAMETER REQUEST COMMAND REPLY FOR BCS.",
                                           lineName, trxID));
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// MES Reply Invoke this Function
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="portName"></param>
        /// <param name="carrierName"></param>
        /// <param name="result"></param>
        public void RecipeParameterMESReply(string trxID, string lineName, string portName, string carrierName, string result)
        {
            try
            {
                string timerId = string.Format("{0}_CheckRecipeParameterReply", trxID);
                if (Timermanager.IsAliveTimer(timerId))
                {
                    Timermanager.TerminateTimer(timerId);
                }
                lock (_waitForMES)
                {
                    if (_waitForMES.ContainsKey(trxID))
                    {
                        _waitForMES[trxID].Result = result == "Y" ? eRecipeCheckResult.OK : eRecipeCheckResult.NG;
                        _waitForMES[trxID].IsFinish = true;
                    }
                }

                 Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LineName={0}] [BCS <- EQP]=[{1}] RECIPE PARAMETER REQUEST COMMAND REPLY FOR BCS RESULT =[{2}].",
                                          lineName, trxID, result));
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        /// <summary>
        /// MES Reply Invoke this Function For Recipe Modify Reply
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="portName"></param>
        /// <param name="carrierName"></param>
        /// <param name="result"></param>
        public void RecipeParameterMESReply(string trxID, string lineName, string machinename, string result)
        {
            try
            {
                string timerId = string.Format("{0}_CheckRecipeParameterReply", trxID);
                if (Timermanager.IsAliveTimer(timerId))
                {
                    Timermanager.TerminateTimer(timerId);
                }
                lock (_waitForMES)
                {
                    if (_waitForMES.ContainsKey(trxID))
                    {
                        _waitForMES[trxID].Result = result == "Y" ? eRecipeCheckResult.OK : eRecipeCheckResult.NG;
                        _waitForMES[trxID].IsFinish = true;
                    }
                }
                #region[Get LINE]
                Line line = ObjectManager.LineManager.GetLine(lineName);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", lineName));
                #endregion
                if (result != "Y")
                {
                    // TODO :要打訊息到OPI上去..要秀出來提示
                    string err = "Recipe Rarameter MES Reply NG";
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, lineName, err });

                    Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(machinename);
                    if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", machinename));
                    //To Do Dowload CIM Message;
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy add By CELL 是不同IO 20160402
                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), eqp.Data.NODENO, "Recipe Parameter MES Reply NG", "BCS", "0" });
                    else
                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trxID, eqp.Data.NODENO, err, "BCS" });
                }


                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LineName={0}] [BCS <- EQP]=[{1}] RECIPE PARAMETER REQUEST COMMAND REPLY FOR BCS RESULT =[{2}].",
                                          lineName, trxID, result));
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// 拆解RecipeParameter的资料 Name,Value 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="localNo"></param>
        /// <param name="inputData"></param>
        /// <returns></returns>
        private bool GetRecipeParameter(RecipeCheckInfo info, short[] rawData)
        {
            try
            {
                //Add By yangzhenteng 20190705 For CUT Line Recipe Patameter Bypass Check;
                bool CutRecipeParameterCheckBypassFlag = ParameterManager.ContainsKey("CutRecipeParameterCheckBypassFlag") ? ParameterManager["CutRecipeParameterCheckBypassFlag"].GetBoolean() : false; 
                IList<RecipeParameter> parameters = ObjectManager.RecipeManager.GetRecipeParameter(info.EQPNo);
                string value = string.Empty;
                int startAddres = 0;
                if (parameters == null)
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}]  {0} RECIPE PARAMETER NOT FOUND.", info.EQPNo, info.TrxId));
                    return true;
                }
                StringBuilder log = new StringBuilder();
                foreach (RecipeParameter param in parameters)
                {
                    ItemExpressionEnum expression;
                    if (!Enum.TryParse(param.Data.EXPRESSION.ToUpper(), out expression))
                    {
                        continue;
                    }
                    #region decode by expression
                    switch (expression)
                    {
                        case ItemExpressionEnum.BIT:
                            value = ExpressionBIT.Decode(startAddres, int.Parse(param.Data.BOFFSET), int.Parse(param.Data.BPOINTS), rawData);
                            break;
                        case ItemExpressionEnum.ASCII:
                            value = ExpressionASCII.Decode(startAddres, int.Parse(param.Data.WOFFSET), int.Parse(param.Data.WPOINTS), int.Parse(param.Data.BOFFSET), int.Parse(param.Data.BPOINTS), rawData);
                            value = Regex.Replace(value, @"[^\x21-\x7E]|<|>|'", " ");//过滤不可显示的字符 20150211 tom
                            break;
                        case ItemExpressionEnum.BIN:
                            value = ExpressionBIN.Decode(startAddres, int.Parse(param.Data.WOFFSET), int.Parse(param.Data.WPOINTS), int.Parse(param.Data.BOFFSET), int.Parse(param.Data.BPOINTS), rawData);
                            break;
                        case ItemExpressionEnum.EXP:
                            value = ExpressionEXP.Decode(startAddres, int.Parse(param.Data.WOFFSET), int.Parse(param.Data.WPOINTS), rawData).ToString();
                            break;
                        case ItemExpressionEnum.HEX:
                            value = ExpressionHEX.Decode(startAddres, int.Parse(param.Data.WOFFSET), int.Parse(param.Data.WPOINTS), int.Parse(param.Data.BOFFSET), int.Parse(param.Data.BPOINTS), rawData);
                            break;
                        case ItemExpressionEnum.INT:
                            value = ExpressionINT.Decode(startAddres, int.Parse(param.Data.WOFFSET), int.Parse(param.Data.WPOINTS), int.Parse(param.Data.BOFFSET), int.Parse(param.Data.BPOINTS), rawData).ToString();
                            break;
                        case ItemExpressionEnum.LONG:
                            value = ExpressionLONG.Decode(startAddres, int.Parse(param.Data.WOFFSET), int.Parse(param.Data.WPOINTS), int.Parse(param.Data.BOFFSET), int.Parse(param.Data.BPOINTS), rawData).ToString();
                            break;
                        case ItemExpressionEnum.SINT:
                            value = ExpressionSINT.Decode(startAddres, int.Parse(param.Data.WOFFSET), int.Parse(param.Data.WPOINTS), int.Parse(param.Data.BOFFSET), int.Parse(param.Data.BPOINTS), rawData).ToString();
                            break;
                        case ItemExpressionEnum.SLONG:
                            value = ExpressionSLONG.Decode(startAddres, int.Parse(param.Data.WOFFSET), int.Parse(param.Data.WPOINTS), int.Parse(param.Data.BOFFSET), int.Parse(param.Data.BPOINTS), rawData).ToString();
                            break;
						case ItemExpressionEnum.BCD:
							value = ExpressionBCD.Decode(startAddres, int.Parse(param.Data.WOFFSET), int.Parse(param.Data.WPOINTS), int.Parse(param.Data.BOFFSET), int.Parse(param.Data.BPOINTS), rawData).ToString();
							break;
                        default:
                            break;
                    }
                    #endregion

                    #region 算法[modify][by yang 20170329 转不出(value超过类型值范围)报default]
                    string itemValue = string.Empty;
                    double doubleresult;
                    switch (param.Data.OPERATOR) //目前operator只有'/',only modify '/'
                    {
                        case ArithmeticOperator.PlusSign:
                            itemValue = (long.Parse(value) + long.Parse(param.Data.DOTRATIO)).ToString();
                            break;
                        case ArithmeticOperator.MinusSign:
                            itemValue = (long.Parse(value) - long.Parse(param.Data.DOTRATIO)).ToString();
                            break;
                        case ArithmeticOperator.TimesSign:
                            itemValue = (double.Parse(value) * double.Parse(param.Data.DOTRATIO)).ToString();
                            break;
                        case ArithmeticOperator.DivisionSign:

                            if (ServerName.Contains("CCCUT") && CutRecipeParameterCheckBypassFlag)
                            {
                                if (value == "9999")//9999的不进行数据转换Edit By Yangzhenteng20190705;
                                {
                                    if (double.TryParse(value, out doubleresult))
                                        itemValue = doubleresult.ToString();
                                    else
                                    {
                                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "DECODE ITEM Error: (" + param.Data.PARAMETERNAME + "), ");
                                        itemValue = "-999";  //default
                                    }
                                }
                                else//正常处理逻辑
                                {
                                    if (double.TryParse(value, out doubleresult))
                                        itemValue = (doubleresult / double.Parse(param.Data.DOTRATIO)).ToString();
                                    else
                                    {
                                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "DECODE ITEM Error: (" + param.Data.PARAMETERNAME + "), ");
                                        itemValue = "-999";  //default
                                    }                                 
                                }                          
                            }
                            else //正常处理逻辑
                            {
                                if (double.TryParse(value, out doubleresult))
                                    itemValue = (doubleresult / double.Parse(param.Data.DOTRATIO)).ToString();
                                else
                                {
                                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "DECODE ITEM Error: (" + param.Data.PARAMETERNAME + "), ");
                                    itemValue = "-999";  //default
                                } 
                            }
                            break;
                        default:
                            itemValue = value;
                            break;
                    }
                    #endregion

                    log.AppendFormat("[{0}={1}],", param.Data.PARAMETERNAME, itemValue);

                    #region Recipe Report Parameter Rule
                    //Watson Modify 20141010 For CELL ODF SUV 
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(info.EQPNo);
                    if (eqp != null)
                    {
                        if (eqp.Data.NODEID.Contains(keyCELLMachingName.CBSUV))
                        {
                            Unit unit = ObjectManager.UnitManager.GetUnit(info.EQPNo, param.Data.REPORTUNITNO.ToString());
                            if (unit != null)
                            {  //Equipmnet ID^Unit ID@ParaName
                                info.Parameters.Add(info.EqpID + "^" + unit.Data.UNITID + "@" + param.Data.PARAMETERNAME, itemValue);
                                continue;
                            }
                        }
                        #region Watson Add 20150324 For PMT L2 共用，但不同名稱
                        //if (eqp.Data.LINEID.Contains(keyCELLPMTLINE.CBPMI) && (eqp.Data.NODENO == "L2"))
                        //{
                        //    if (param.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))  //[SBRM_RECIPEPARAMETER] 需要設置LINE ID
                        //    {
                        //        string eqpID = ParameterManager[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
                        //        Unit unit = ObjectManager.UnitManager.GetUnit(info.EQPNo, param.Data.REPORTUNITNO.ToString());
                        //        if (unit != null)
                        //        {  //Equipmnet ID^Unit ID@ParaName
                        //            string unitID = ParameterManager[keyCELLPTIParameter.CELL_PTI_UNITID].GetString();
                        //            info.Parameters.Add(eqpID + "^" + unitID + "@" + param.Data.PARAMETERNAME, itemValue);
                        //            continue;
                        //        }
                        //        else
                        //        {
                        //            info.Parameters.Add(eqpID + "^" + param.Data.PARAMETERNAME, itemValue);
                        //            continue;
                        //        }
                        //    }
                        //}
                        #endregion
                    }
                    //20141124 modify by edison:info.EQID取出为Null，改成info.EQPNo
                    //Watson 20141124 Modify 在定義時就給empty 就不會是null (Modify RecipeInfo.cs EqpID = string.empty;)
                    info.Parameters.Add(info.EqpID + "^" + param.Data.PARAMETERNAME, itemValue); //Equipmnet ID^ParaName
                    #endregion
                }
                //20150402 增加记录拆解的Log Tom
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}]", info.EQPNo, info.TrxId));
                LogTrx(MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] {2}", info.EQPNo, info.TrxId, log.ToString().Replace(',', '\n')));
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        #region [AutoRecipeChangeMode]
        public void AutoRecipeChangeMode(Trx inputData)
        {
            try
            {
                //;

                #region [取得EQP]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                eEnableDisable autoRecipeChangeMode = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                if (eqp.File.AutoRecipeChangeMode != autoRecipeChangeMode)
                {
                    lock (eqp)
                    {
                        eqp.File.AutoRecipeChangeMode = autoRecipeChangeMode;
                    }
                }
                if (inputData.IsInitTrigger) return;
                //ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [EQP -> EQP]=[{1}] AUTO RECUPE CHANGE MODE =[{2}]",
                    eqp.Data.NODENO, inputData.TrackKey, autoRecipeChangeMode));
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        /// <summary>
        /// 检查PLC Type的 Recipe RegisterValidationReply 的bit 需要Off
        /// 如果Bit 不为Off 此方法返回false，其他任何情况都会true
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <returns></returns>
        private bool CheckRecipeRegisterValidationReplyIsOff(string equipmentNo) {
            try {
                string trxName = string.Format("{0}_RecipeRegisterValidationCommandReply", equipmentNo);
                Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                if (trx != null) {
                    eBitResult bitResult = (eBitResult)int.Parse(trx.EventGroups[0].Events[1].Items[0].Value);
                    if (bitResult != eBitResult.OFF)
                    {
                        LogError(MethodBase.GetCurrentMethod().Name + "()", string.Format("{0} Bit is On, Check error", trxName));
                        return false;
                    }
                    else
                    return true;
                } else
                    return true;
            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                return true;
            }
        }

        /// <summary>
        /// 检查PLC Type 的Recipe ParameterRequest Command Reply 的Bit 是否Off
        /// 如果Bit 不为Off 此方法返回false，其他任何情况都会true
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <returns>如果bit Off 返回 true，如果bit On  返回 false</returns>
        private bool CheckRecipeParameterRequestCommandReplyIsOff(string equipmentNo) {
            try {
                string trxName = string.Format("{0}_RecipeParameterRequestCommandReply", equipmentNo);
                Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                if (trx != null) {
                    eBitResult bitResult = (eBitResult)int.Parse(trx.EventGroups[0].Events[1].Items[0].Value);
                    if (bitResult != eBitResult.OFF) 
                    {
                        LogError(MethodBase.GetCurrentMethod().Name + "()", string.Format("{0} Bit is On, Check error", trxName));
                        return false;
                    }
                    return true;
                } else
                    return true;
            } catch (System.Exception ex) {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                return true;
            }
        }
    
    }
}
