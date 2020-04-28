using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniRCS.Core;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class RobotSpecialService
    {
        //20160126-001-dd::更新Job的RobotRTCFlag=true (当command是RTC_PUT时)
        public void Update_BCS_Job_RobotRTCFlag_For1Cmd_1Arm_1Job(Robot _robot, Job _job, DefineNormalRobotCmd _cmd)
        {
            try
            {
                if (_cmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_RTC_PUT)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        string _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Action is (PUT) Change Action to (RTC_PUT), Update RobotRTCFlag(true).",
                            _robot.Data.NODENO, _robot.Data.ROBOTNAME, _job.CassetteSequenceNo, _job.JobSequenceNo);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                    }
                    #endregion
                    lock (_job)
                    {
                        _job.RobortRTCFlag = true;
                    }
                   if (_job.RobotWIP.CurStepNo.ToString().Equals("81"))
                   {
                       lock (_job) //jump时已经给true,这里再update一次
                       {
                           _job.RobotWIP.EQPRTCFlag= true;
                       }
                           
                   }
                    ObjectManager.JobManager.EnqueueSave(_job);
                }
            }
            catch(Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region Array shop DRY line
        /// <summary>取得目前 DRY 機台上 Load Lock I/F 的 Receive Type 信息!! (Lower and Upper)
        /// 
        /// </summary>
        /// <param name="curRobot">目前的 Robot 物件</param>
        /// <returns>查詢到的 ReceiveType 信息
        /// Key :: Lower Load Lock (12.LLL) or Upper Load Lock (13.ULL)
        /// Value :: ReceiveType value 1-8 (1.MQC, 2.PS, 3.GE, 4.ILD, 5.SD, 6.PV, 7.ASH, 8.PLN)
        /// 如果 Value 為 0, 代表 Product (NOREQ) 目前對 DRY 是沒使用到!!
        /// </returns>
        public System.Collections.Hashtable Get_DRY_ReceiveType_For1Cmd_1Arm_1Job(Robot curRobot)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;

            System.Collections.Hashtable _receiveType = new System.Collections.Hashtable();
            _receiveType.Clear();

            try
            {
                List<RobotStage> _stageDRY = Get_DRY_RobotStageList(curRobot);

                foreach (RobotStage _stage in _stageDRY)
                {
                    if (_receiveType.ContainsKey(_stage.Data.STAGEID)) continue;

                    System.Collections.Hashtable _ht = new System.Collections.Hashtable();
                    _ht.Clear();
                    _ht.Add(eRobotContextParameter.DRYIFReceiveAbleSignal, _stage.File.DownStreamReceiveAbleSignal);
                    _ht.Add(eRobotContextParameter.DRYIFReceiveType, _stage.File.DownStreamLoadLockReceiveType);
                    _ht.Add(eRobotContextParameter.DRYStageStatus, _stage.File.CurStageStatus);
                    _ht.Add(eRobotContextParameter.DRYKeptReceiveType, _stage.File.DryKeptLoadLockReceiveType);

                    _receiveType.Add(_stage.Data.STAGEID, _ht);
                }

                return _receiveType;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        /// <summary>取得目前有去 DRY 機台的 Route ID 清單!?
        /// 
        /// </summary>
        /// <param name="curRobot">目前的 Robot 物件</param>
        /// <returns>
        /// 
        /// </returns>
        public List<string> Get_DRY_RouteInfo_For1Cmd_1Arm_1Job(Robot curRobot, Job curJob)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;

            List<string> _route = new List<string>();
            _route.Clear();

            try
            {
                List<RobotStage> _stageDRY = Get_DRY_RobotStageList(curRobot);

                if (_stageDRY == null || _stageDRY.Count() <= 0) return _route;

                string _stageList = string.Empty;
                foreach (RobotStage _stage in _stageDRY)
                {
                    _stageList += (_stageList == string.Empty ? _stage.Data.STAGEID : string.Format(",{0}", _stage.Data.STAGEID));
                }

                List<RobotRouteStep> _routeStep = curJob.RobotWIP.RobotRouteStepList.Values.Where(r => r.Data.STAGEIDLIST.Contains(_stageList)).ToList();

                if (_routeStep == null || _routeStep.Count() <= 0) return _route;

                foreach (RobotRouteStep _step in _routeStep)
                {
                    if (_route.Contains(_step.Data.ROUTEID)) continue;

                    _route.Add(_step.Data.ROUTEID);
                }

                return _route;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        /// <summary>更新Robot最後一次放基板到DRY機台的那個Stage信息
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        public void Update_DRY_LastEnterStageID_For1Cmd_1Arm_1Job(IRobotContext robotConText)
        {

            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                #region [ Get curRobot Entity ]
                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!", "L1", MethodBase.GetCurrentMethod().Name);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Robot!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);
                }
                #endregion

                #region [ Get Target Stage Entity ]
                RobotStage curTargetStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(robotConText[eRobotContextParameter.TargetStageID].ToString());

                //找不到 Target Stage 回NG
                if (curTargetStage == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get TargetStage!", "L1", MethodBase.GetCurrentMethod().Name);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get TargetStage!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_TargetStage_Is_Null);
                    robotConText.SetReturnMessage(errMsg);
                    return;
                }
                #endregion


                if (curTargetStage.Data.NODENO == eRobotContextParameter.DRYNodeNo) curRobot.File.DRYLastEnterStageID = int.Parse(curTargetStage.Data.STAGEID);


                robotConText.SetReturnCode(eProcResultAction_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eProcResultAction_ReturnMessage.OK_Message);
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
            }
        }

        /// <summary> 取得 DRY 機台的 Stage 信息
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <returns></returns>
        private List<RobotStage> Get_DRY_RobotStageList(Robot curRobot)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curRobotStages = null;

            List<RobotStage> _stageDRY = new List<RobotStage>();
            _stageDRY.Clear();

            try
            {
                #region [ Get CurRobot All Stage List ]
                curRobotStages = ObjectManager.RobotStageManager.GetRobotStages(curRobot.Data.ROBOTNAME);

                //找不到 Robot Stage 回 NG
                if (curRobotStages == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo!",
                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get RobotStageInfo!", MethodBase.GetCurrentMethod().Name);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return _stageDRY;
                }
                //沒有 Robot Stage 回 NG
                if (curRobotStages.Count == 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo!", curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Get RobotStageInfo is Empty!", MethodBase.GetCurrentMethod().Name);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog); ;

                    return _stageDRY;
                }
                //取得 DRY Stage Info
                foreach (RobotStage _stage in curRobotStages)
                {
                    if (_stage.Data.NODENO == eRobotContextParameter.DRYNodeNo) _stageDRY.Add(_stage);


                }
                //沒有 Robot Stage 回 NG
                if (_stageDRY.Count == 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo!", curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Get RobotStageInfo for DRY equipment is Empty!", MethodBase.GetCurrentMethod().Name);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog); ;

                    return _stageDRY;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
            return _stageDRY;
        }
        
        //20160118-001-dd
        /// <summary>  取得 DRY line  Indexer 机台上报的 Process Type Block 信息!!
        /// 
        /// </summary>
        /// <param name="_robotContext"></param>
        /// <returns></returns>
        public List<string> Get_DRY_RrocessTypeBlock_For1Cmd_1Arm_1Job(IRobotContext _robotContext)
        {
            string _eqpNo = "L2"; //indexer node no
            List<string> _lstProcessType = null;

            try
            {
                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]
                //<trx name="L2_ProcessTypeBlock" triggercondition="change">
                //  <eventgroup name="L2_EG_ProcessTypeBlock" dir="E2B">
                //    <event name="L2_W_ProcessTypeBlock" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<event name="L2_W_ProcessTypeBlock" devicecode="W" address="0x0001508" points="6">
                //  <itemgroup name="ProcessTypeBlock" />
                //</event>

                //<itemgroup name="ProcessTypeBlock">
                //  <item name="Port#01ProcessType" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="Port#02ProcessType" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="Port#03ProcessType" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="Port#04ProcessType" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="Port#05ProcessType" woffset="4" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="Port#06ProcessType" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>
                #endregion

                string _trxName = string.Format("{0}_ProcessTypeBlock", _eqpNo);
                string _grpName = string.Format("{0}_EG_ProcessTypeBlock", _eqpNo);//Event Group
                string _evtName = string.Format("{0}_W_ProcessTypeBlock", _eqpNo);//Event
                //string _itemName = "Port#{0}ProcessType";
                Trx _trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { _trxName, false }) as Trx;
                #endregion

                _lstProcessType = new List<string>();
                _lstProcessType.Clear();

                string _svLogs = string.Empty;
                foreach (Item _processType in _trx[_grpName][_evtName].Items.AllValues)
                {
                    if (_processType == null) continue;
                    _svLogs += string.Format(" {0}::{1},", _processType.Name.ToString(), _processType.Value.ToString());

                    if (int.Parse(_processType.Value.ToString()) == 0) continue;
                    if (_lstProcessType.Contains(_processType.Value.ToString())) continue;

                    _lstProcessType.Add(_processType.Value.ToString());
                }
                Logger.LogDebugWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Process Type Block=" + _svLogs);
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return _lstProcessType;
        }
        #endregion

        #region Pre-Fetch
        /// <summary> 
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        public bool Check_PreFetch_DelayTime_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;

            try
            {

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];



                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!", "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                //20170626 by huangjiayin
                #region[PDR prefetch delay special]
                if (curRobot.Data.LINETYPE == eLineType.CELL.CCPDR)
                {
                    if (_timerManager.IsAliveTimer("PDRPrefetchTimeout"))
                    {
                        return false;
                    }
                }

                #endregion




                int _temp = 60; //单位:秒
                if (ParameterManager.Parameters.ContainsKey(eRobotContextParameter.PREFETCH_DELAY_TIME)) int.TryParse(ParameterManager.Parameters[eRobotContextParameter.PREFETCH_DELAY_TIME].Value.ToString(), out _temp);

                if (curRobot.File.LastPreFetchReturnDateTime == DateTime.MinValue) return true; //第一次, 直接回true 可以做预取!

                TimeSpan _ts = new TimeSpan(DateTime.Now.Ticks - curRobot.File.LastPreFetchReturnDateTime.Ticks);
                if (_ts.TotalSeconds > _temp) return true; //超过设定的时间, 所以算 Timeout! 可以再做预取了! 

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return false;
        }
        #endregion

        #region RTC
        /// <summary>检查预取出来的基板, 是否已经超过RTC_WAIT_TIME时间!
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns>True::超时, False::未超时</returns>
        public bool Check_RTC_Timeout_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;

            try
            {

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];



                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!", "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBcsJob Entity ]

                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

                //找不到 Job 回NG
                if (curBcsJob == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobInfo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get JobInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                int _temp = 60; //单位:秒
                if (ParameterManager.Parameters.ContainsKey(eRobotContextParameter.RTC_WAIT_TIME)) int.TryParse(ParameterManager.Parameters[eRobotContextParameter.RTC_WAIT_TIME].Value.ToString(), out _temp);


                if (curBcsJob.RobotWIP.FetchOutDataTime == DateTime.MinValue) return false;

                TimeSpan _ts = new TimeSpan(DateTime.Now.Ticks - curBcsJob.RobotWIP.FetchOutDataTime.Ticks);
                if(_ts.TotalSeconds < _temp)
                {
                    //超过设定的时间, 所以算RTC Timeout! 
                    //errMsg = string.Format("[{0}] can not put glass to Stage! because the RTC timeout.", MethodBase.GetCurrentMethod().Name);

                    //robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    //robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }

        //20160525
        public bool Check_EQPRTC_Timeout_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;

            try
            {

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];



                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!", "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBcsJob Entity ]

                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

                //找不到 Job 回NG
                if (curBcsJob == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobInfo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get JobInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                int _temp = 60; //单位:秒
                if (ParameterManager.Parameters.ContainsKey(eRobotContextParameter.EQPRTC_WAIT_TIME)) int.TryParse(ParameterManager.Parameters[eRobotContextParameter.EQPRTC_WAIT_TIME].Value.ToString(), out _temp);


                if (curBcsJob.RobotWIP.StoreDateTime == DateTime.MinValue) return false;

                TimeSpan _ts = new TimeSpan(DateTime.Now.Ticks - curBcsJob.RobotWIP.StoreDateTime.Ticks);
                if (_ts.TotalSeconds < _temp)
                {
                    //超过设定的时间, 所以算RTC Timeout! 
                    //errMsg = string.Format("[{0}] can not put glass to Stage! because the RTC timeout.", MethodBase.GetCurrentMethod().Name);

                    //robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    //robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }


        /// <summary>检查预取出来的基板, 要去的目的地跟目前有UDRQ的EQP STAGE是否有配对!?
        /// 
        /// </summary>
        /// <param name="robotContext"></param>
        /// <returns></returns>
        public bool Check_ArmJob_TargetStage_UDRQ_REQ_For1Arm1Job(IRobotContext robotContext)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;

            try
            {
                #region [ Get curRobot Entity ]
                Robot curRobot = (Robot)robotContext[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!", "L1", MethodBase.GetCurrentMethod().Name);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!", MethodBase.GetCurrentMethod().Name);

                    robotContext.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotContext.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get curBcsJob Entity ]
                Job curBcsJob = (Job)robotContext[eRobotContextParameter.CurJobEntity];

                //找不到 Job 回NG
                if (curBcsJob == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobInfo!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get JobInfo!", MethodBase.GetCurrentMethod().Name);

                    robotContext.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotContext.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                if (curBcsJob.RobotWIP.PreFetchFlag <= 0) return false; //没有启动预取, 就不考虑!

                RobotRoute _route = ObjectManager.RobotManager.GetRoute(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID);

                if (_route == null) return false; //找不到route, 就不考虑!
                if (_route.Data.RTCMODEFLAG.ToString().ToUpper() != "Y") return false; //没有启动RTC, 就不考虑!
                if (_route.Data.RTCFORCERETURNFLAG.ToString().ToUpper() != "Y") return false; //没有启动强制RTC, 就不考虑!


                string[] _stages = Get_TargetStageInfo_ArmJob_For1Arm1Job(robotContext); //取得STAGE LIST!
                if (_stages.Length <= 0) return false; //没有STAGE LIST, 就不考虑!

                List<string> _canUseStageLists = new List<string>();
                _canUseStageLists.Clear();


                RobotStage _stage = null;
                for (int i = 0; i < _stages.Length; i++)
                {
                    _stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(_stages[i]);

                    switch (_stage.File.CurStageStatus)
                    {
                        case eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY: // "UDRQ_LDRQ":
                        case eRobotStageStatus.SEND_OUT_READY: // "UDRQ":
                            break;
                        default: continue; //其他状态, 就不考虑!
                    }
                    if (!_canUseStageLists.Contains(_stages[i])) _canUseStageLists.Add(_stages[i]); //取得是UDRQ的STAGE!
                }
                if (_canUseStageLists.Count() <= 0) return false; //没有UDRQ STAGE, 就不考虑!

                //if (curBcsJob.RobotWIP.PutReadyFlag == 1) //有做PUT READY
                //{
                //    if (curBcsJob.RobotWIP.PutReady_StageID != string.Empty) //有UDRQ的STAGE! 并且有put ready stage!
                //    {
                //        if (_canUseStageLists.Contains(curBcsJob.RobotWIP.PutReady_StageID)) return false; //put ready的STAGE是在UDRQ STAGE LIST里面, 就不考虑!
                //    }
                //}
                //else
                //{
                    //if (Workbench.LineType == eLineType.CF.FCMAC_TYPE1) {
                        Equipment IndexerEQP = ObjectManager.EquipmentManager.GetEQP("L2");
                        if (IndexerEQP == null)
                        {
                            
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Indexer EQP by EQPNo({2})!",
                                                                        IndexerEQP.Data.NODENO, curRobot.Data.ROBOTNAME, IndexerEQP.Data.NODENO);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            
                            
                            return false;
                        }

                        //20160520 是要先check Indexer(L2)的 ProductTypeCheckMode有沒開,不是EQP的
                        if (IndexerEQP.File.ProductTypeCheckMode == eEnableDisable.Enable)
                        {

                            List<Job> jobList = new List<Job>();
                            foreach (string stageid in _canUseStageLists)
                            {
                                RobotStage curStepUseStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageid);

                                foreach (string _info in curStepUseStage.curUDRQ_SlotList.Values)
                                {
                             
                                Job _job = ObjectManager.JobManager.GetJob(_info.Split('_')[0].ToString(), _info.Split('_')[1].ToString());
                                if (_job == null)
                                {
                                    errMsg = string.Format("[{0}] curStageID({1}) SendOutJob({2}_{3})  Is Null.", MethodBase.GetCurrentMethod().Name,stageid,_job.CassetteSequenceNo, _job.JobSequenceNo);

                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", errMsg);
                                
                                }
                                if (!jobList.Contains(_job)) jobList.Add(_job);

                            
                                }
                            }
                            if (jobList.Where(t => t.ProductType.Value == curBcsJob.ProductType.Value).Count() > 0) return false;
                            else
                            {
                                strlog = string.Format("Job({0},{1}) curStepID:{2} curProductType:{3} Check ForceRTC True,Will Force RTC", 
                                    curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString(), curBcsJob.RobotWIP.CurStepNo, curBcsJob.ProductType.Value);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                return true;//预取的job和当前stage出片的job不能做get/put ,要立刻做RTC
                            }
                        }

                   // }
                    //只有预取, 没有做put ready! 
                    return false; //有STAGE, 代表可以去, 就不考虑!
                //}
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary> 取得目前job下一步STEP会去的STAGE LIST!
        /// 
        /// </summary>
        /// <param name="robotContext"></param>
        /// <returns></returns>
        private string[] Get_TargetStageInfo_ArmJob_For1Arm1Job(IRobotContext robotContext)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;

            string[] _stages = null;

            try
            {
                #region [ Get curRobot Entity ]
                Robot curRobot = (Robot)robotContext[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!", "L1", MethodBase.GetCurrentMethod().Name);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!", MethodBase.GetCurrentMethod().Name);

                    robotContext.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotContext.SetReturnMessage(errMsg);
                    return null;
                }
                #endregion

                #region [ Get curBcsJob Entity ]
                Job curBcsJob = (Job)robotContext[eRobotContextParameter.CurJobEntity];

                //找不到 Job 回NG
                if (curBcsJob == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobInfo!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get JobInfo!", MethodBase.GetCurrentMethod().Name);

                    robotContext.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotContext.SetReturnMessage(errMsg);
                    return null;
                }
                #endregion

                RobotRouteStep _step = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
                if (_step != null) _stages = _step.Data.STAGEIDLIST.Split(',');
             
            }
            catch
            {

            }
            return _stages;
        }



        #endregion

        #region 预取的特别检查条件逻辑!! 
        /// <summary>检查stage的special condition是否符合!?
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <param name="_curStage"></param>
        /// <param name="_job"></param>
        /// <returns>true=special condition ON, otherwise special condition OFF</returns>
        public bool Check_Stage_Prefetch_SpecialCondition_For1Arm1Job(IRobotContext robotConText, RobotStage _curStage, Job _job)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;

            try
            {
                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];



                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!", "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBcsJob Entity ]
                Job curBcsJob = (_job == null ? (Job)robotConText[eRobotContextParameter.CurJobEntity] : _job);

                //找不到 Job 回NG
                if (curBcsJob == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobInfo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get JobInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region Common Check Condition -- Transfer Stop Signal
                RobotRouteStep _step = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];

                string[] _stageList = _step.Data.STAGEIDLIST.Split(',');

                RobotStage _stage = null;
                int _match = 0;
                foreach (string _stageId in _stageList)
                {
                    _stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(_stageId);

                    if (_stage == null) continue; //找不到, 不需要考虑!!
                    //if (_stage.File.DownStreamTransferStopRequestFlag) continue; //ON就不需要考虑
                    if (!_stage.File.DownStreamTransferStopRequestFlag) continue; //OFF就不需要考虑, 2016/01/07 cc.kuang

                    _match++; //Transfer Stop ON的stage数!!
                }

                //因为有可能会有多个stage, 所以需要考虑到是不是全部的stage的transfer stop signal都是ON!
                //if (_match >= _stageList.Length) return true; //只要符合条件, 下面的条件就不需要再检查!!
                if (_match >= _stageList.Length) return false; //只要符合条件, 下面的条件就不需要再检查!!, 2016/01/07 cc.kuang
                #endregion

                #region Line Special Check Condition by Line Type
                Line _line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if (_line == null) return false;

                switch (_line.Data.LINETYPE)
                {
                    case eLineType.ARRAY.DRY_ICD:
                    case eLineType.ARRAY.DRY_YAC:
                    case eLineType.ARRAY.DRY_TEL:
                    case eLineType.ARRAY.CVD_AKT:
                    case eLineType.ARRAY.CVD_ULVAC:
                         return CheckStagePrefetchSpecialCondition_IndexerOperationModeCheck(_line);
                        //add by Yang 2016/8/23  添加ForceCleanOut or AbnormalForceCleanOut时不可预取，以后的卡控，都可以放在这里卡glass是否真的可以做预取                       
                    case eLineType.ARRAY.ELA_JSW:
                        //return true; //只要符合条件, 下面的条件就不需要再检查!!
                        return CheckStagePrefetchSpecialCondition_ELAMixMode(robotConText, _curStage, _job);
                    default: break;
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }
        /// <summary>
        /// can't not prefetch when IndexerOperationMode is ForceCleanOut or AbnormalForceCleanOut,Yang
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private bool CheckStagePrefetchSpecialCondition_IndexerOperationModeCheck(Line _line)
        {
            try
            {
                if (_line == null)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Line Object is Null");
                    return false;
                }
                if (_line.File.IndexOperMode == eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE || _line.File.IndexOperMode == eINDEXER_OPERATION_MODE.ABNORMAL_FORCE_CLEAN_OUT_MODE) return false;
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>ELA Special Rule for Prefetch
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <returns></returns>
        private bool CheckStagePrefetchSpecialCondition_ELAMixMode(IRobotContext robotConText, RobotStage _curStage, Job _job)
        {
            Line line;
            List<Port> lstPort;
            IList<Job> lstJob;
            Equipment ela1, ela2, indexer;
            bool rtn = true;
            string processtype = string.Empty;
            string glassflow = string.Empty;
            try
            {
                line = ObjectManager.LineManager.GetLines()[0];
                if (line == null)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Line Object is Null");
                    return false;
                }

                //if all glass in cst is only ELA, return true   
                indexer = ObjectManager.EquipmentManager.GetEQP("L2");
                lstPort = ObjectManager.PortManager.GetPorts(indexer.Data.NODEID);
                foreach (Port port in lstPort)
                {
                    if (port.File.Status != ePortStatus.LC || (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING && port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING))
                        continue;

                    lstJob = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo);
                    if (lstJob == null || lstJob.Count == 0)
                    {
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Job List is Empty, Cassette Sequence = " + port.File.CassetteSequenceNo);
                        return false;
                    }

                    foreach (Job jb in lstJob)
                    {
                        //Mix Mode Can't Prefetch
                        if (processtype.Length == 0)
                        {
                            processtype = jb.ArraySpecial.ProcessType;
                        }
                        else
                        {
                            if (!processtype.Equals(jb.ArraySpecial.ProcessType))
                            {
                                return false;
                            }
                        }

                        //Glass Flow Diff in All CST, Can't Prefetch
                        if (glassflow.Length == 0)
                        {
                            glassflow = jb.ArraySpecial.GlassFlowType;
                        }
                        else
                        {
                            if (!glassflow.Equals(jb.ArraySpecial.GlassFlowType))
                            {
                                return false;
                            }
                        }

                        //if not ELA step only, Can't Prefetch
                        if (!jb.ArraySpecial.GlassFlowType.Equals("2") && !jb.ArraySpecial.GlassFlowType.Equals("4")
                            && !jb.ArraySpecial.GlassFlowType.Equals("6"))
                        {
                            return false;
                        }
                    }
                }

                //if not ELA step only, Can't Prefetch
                if (!_job.ArraySpecial.GlassFlowType.Equals("2") && !_job.ArraySpecial.GlassFlowType.Equals("4")
                    && !_job.ArraySpecial.GlassFlowType.Equals("6"))
                {
                    return false;
                }

                //if MQC, Can't prefetch
                processtype = _job.ArraySpecial.ProcessType;
                if (processtype.Equals("1"))
                    return false;            
               
                ela1 = ObjectManager.EquipmentManager.GetEQP("L4");
                ela2 = ObjectManager.EquipmentManager.GetEQP("L5");
                if (ela1 == null || ela2 == null)
                    return false;

                //if ELA1, ELA2 Run Mode not same, Can't prefetch
                if (ela1.File.EquipmentRunMode.ToUpper() != ela2.File.EquipmentRunMode.ToUpper())
                    return false;

                //check EQ Mode & Status, if Mode & Status Not Match, return false              
                if (processtype.Equals("1"))
                    processtype = "MQC";
                else
                    processtype = "NORMAL";

                if (ela1.File.EquipmentRunMode.ToUpper().Equals(processtype) && ela2.File.EquipmentRunMode.ToUpper().Equals(processtype))
                {
                    if ((ela1.File.Status != eEQPStatus.IDLE && ela1.File.Status != eEQPStatus.RUN) && (ela2.File.Status != eEQPStatus.IDLE && ela2.File.Status != eEQPStatus.RUN))
                        return false;
                }
                else if (ela1.File.EquipmentRunMode.ToUpper().Equals(processtype))
                {
                    if (ela1.File.Status != eEQPStatus.IDLE && ela1.File.Status != eEQPStatus.RUN)
                        return false;
                }
                else if (ela2.File.EquipmentRunMode.ToUpper().Equals(processtype))
                {
                    if (ela2.File.Status != eEQPStatus.IDLE && ela2.File.Status != eEQPStatus.RUN)
                        return false;
                }
                else
                {
                    return false;
                }

                return rtn;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }
        #endregion

       







    }




}