using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;
using System.Threading;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    [UniAuto.UniBCS.OpiSpec.Help("JobRouteStepJumpService")]
    public partial class JobRouteStepJumpService : AbstractRobotService
    {

        public override bool Init()
        {
            return true;
        }

//All Job Route Step Jump Function List [ Method Name = "RouteStepJump" + " _" +  "Condition Abbreviation" EX:"Filter_ForPVD" ]==============================================================
//RouteStepJump Funckey = "JP" + XXXX(序列號)

        public class RobotInfo
        {
            public _ErrMsg ErrMsg = new _ErrMsg();

            private Robot _robot = null;
            private Job _job = null;
            private RobotRoute _route = null;
            private RobotRouteStep _step = null;
            private bool _RTCEnabledFlag = false;
            private bool _PreFetchEnabledFlag = false;
            private bool _have2ndCmdFlag = false;



            public class _ErrMsg : IFormattable
            {
                public string _errMsg { get; set; }

                public string ToString(params string[] _params)
                {
                    return ToString(string.Empty, _params);
                }
                public string ToString(string _details, params string[] _params)
                {
                    if (_errMsg == string.Empty) return string.Empty;
                    string _temp = string.Concat(_details, _errMsg);
                    return string.Format(_temp, _params);
                }

                public string ToString(string _text, IFormatProvider provider)
                {
                    return _errMsg;
                }
                public _ErrMsg()
                {
                    _errMsg = string.Empty;
                }

            }
            private void init(IRobotContext robotConText)
            {
                _robot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];
                if (_robot == null)
                {
                    ErrMsg._errMsg = "[{0}] can not Get Robot!";
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    return;
                }
                _job = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (_job == null)
                {
                    ErrMsg._errMsg = "[{0}] can not Get Job!";
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    return;
                }

                _route = ObjectManager.RobotManager.GetRoute(_robot.Data.ROBOTNAME, _job.RobotWIP.CurRouteID);
                if (_route == null)
                {
                    ErrMsg._errMsg = "[{0}] can not Get Route!";
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Route_Is_Null);
                    return;
                }

                _RTCEnabledFlag = (_route.Data.RTCMODEFLAG.ToString().ToUpper() == "Y" ? true : false);
                _robot.Context.AddParameter(eRobotContextParameter.CanUseRTCFlag, _RTCEnabledFlag);

                _have2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];

                if (!_have2ndCmdFlag) //只有第一道命令!!
                {
                    if (_job.RobotWIP.RobotRouteStepList.ContainsKey(_job.RobotWIP.CurStepNo)) _step = _job.RobotWIP.RobotRouteStepList[_job.RobotWIP.CurStepNo];
                }
                else
                {
                    if (_job.RobotWIP.RobotRouteStepList.ContainsKey(_job.RobotWIP.NextStepNo)) _step = _job.RobotWIP.RobotRouteStepList[_job.RobotWIP.NextStepNo];
                }
                if (_step == null)
                {
                    ErrMsg._errMsg = "[{0}] can not Get Step!";
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RouteStep_Is_Null);
                    return;
                }
                
                if(_job.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                {
                    _PreFetchEnabledFlag = (_job.RobotWIP.PreFetchFlag > 0 ? true : false);
                }
                else
                {
                    RobotStage _stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(_job.RobotWIP.CurLocation_StageID);
                    _PreFetchEnabledFlag = (_stage != null && _stage.Data.PREFETCHFLAG.ToString().ToUpper() == "Y" ? true : false);
                }
             

            }
            public RobotInfo(IRobotContext robotConText)
            {
                init(robotConText);
            }


            public Robot robot { get { return _robot; } }
            public Job job { get { return _job; } }
            public RobotRoute route { get { return _route; } }
            public RobotRouteStep step { get { return _step; } }
            public bool CanUseRTCFlag { get { return _RTCEnabledFlag; } }
            public bool CanUsePreFetchFlag { get { return _PreFetchEnabledFlag; } }
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0005")]
        public bool RouteStepJump_RTC(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;

            RobotInfo _info = new RobotInfo(robotConText);



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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                if (curRobot.CurRealTimeSetCommandInfo.Cmd01_Command != 0 || curRobot.CurRealTimeSetCommandInfo.Cmd02_Command != 0)
                    return true;

                #endregion

                if (_info.ErrMsg.ToString(MethodBase.GetCurrentMethod().Name) != string.Empty)
                {
                    if (IsShowDetialLog) Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _info.ErrMsg.ToString("[EQUIPMENT={1}] [RCS <- RCS]", MethodBase.GetCurrentMethod().Name, eRobotContextParameter.MPLCLocalNo));
                    robotConText.SetReturnMessage(_info.ErrMsg.ToString(MethodBase.GetCurrentMethod().Name));

                    return false;
                }



                if (_info.CanUseRTCFlag) //有启动RTC模式!
                {
                    #region [ Get RRC GotoStepID ]
                    int _rtcGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                    if (_rtcGoToStepID == 0)
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
                                                    _info.robot.Data.NODENO, _info.robot.Data.ROBOTNAME, _info.job.CassetteSequenceNo, _info.job.JobSequenceNo, _info.step.Data.STEPID.ToString(), _rtcGoToStepID.ToString());
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Force Clean Out But GotoStepID({4}) is Fail!",
                                                 MethodBase.GetCurrentMethod().Name, _info.job.CassetteSequenceNo, _info.job.JobSequenceNo,
                                                _info.step.Data.STEPID.ToString(), _rtcGoToStepID.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }

                    //Get Change StepID 後的NextStepNO
                    if (!_info.job.RobotWIP.RobotRouteStepList.ContainsKey(_rtcGoToStepID))
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
                                                    _info.robot.Data.NODENO, _info.robot.Data.ROBOTNAME, _info.job.CassetteSequenceNo, _info.job.JobSequenceNo, _info.step.Data.STEPID.ToString(), _rtcGoToStepID.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Force Clean Out But get GotoStepID({4}) Entity Fail",
                                                 MethodBase.GetCurrentMethod().Name, _info.job.CassetteSequenceNo, _info.job.JobSequenceNo,
                                                _info.step.Data.STEPID.ToString(), _rtcGoToStepID.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                    #endregion


                    bool _updateRTCInfo = false; //默认 false 不更新 RTC 信息!!

                    //Case One :: 有启动 预取(Pre-Fetch) 模式, 去判断 RTC WAIT TIME 来决定是否时间到了需要回插!! 
                    if (_info.CanUsePreFetchFlag && _info.job.RobotWIP.FetchOutDataTime != DateTime.MinValue && _info.job.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID) //有启动 预取(Pre-Fetch) 模式!并且此时job已真正取到手臂上
                    {
                        _updateRTCInfo = (bool)Invoke("RobotSpecialService", "Check_RTC_Timeout_For1Arm1Job", new object[] { robotConText });
                    }

                    //Case Two :: 被逼片, 所以需要先回插, 这个项目 目前 pending, 细节尚需讨论 !!
                    if (!_updateRTCInfo) //如果为 true, 代表已经有条件成立, 这个就不需要再 check !!
                    {
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("Job({0},{1}) curStepID:{2}, job's last FetchOutDataTime :{3} job's LastUpdateTime:{4}", 
                                _info.job.CassetteSequenceNo.ToString(), _info.job.JobSequenceNo.ToString(), _info.job.RobotWIP.CurStepNo, _info.job.RobotWIP.FetchOutDataTime.ToString(), _info.job.LastUpdateTime.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                       
                        if (_info.route.Data.RTCFORCERETURNFLAG.ToString().ToUpper() == "Y" && _info.job.RobotWIP.FetchOutDataTime != DateTime.MinValue && _info.job.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID) //有启动强制回插功能!!,并且此时job已真正取到手臂上
                        {
                            _updateRTCInfo = (bool)Invoke("RobotSpecialService", "Check_ArmJob_TargetStage_UDRQ_REQ_For1Arm1Job", new object[] { robotConText });
                        }
                    }

                    //Case Three :: 其他Case, 细节待讨论 !!
                    if (!_updateRTCInfo) //如果为 true, 代表已经有条件成立, 这个就不需要再 check !!
                    {






                    }

                    //目前只要判断 预取 !!
                    if (_updateRTCInfo) //条件成立, 需要执行 RTC !!
                    {
                        lock (_info.job)
                        {
                            _info.job.RobotWIP.CurStepNo = _rtcGoToStepID;
                            _info.job.RobotWIP.NextStepNo = _info.job.RobotWIP.RobotRouteStepList[_rtcGoToStepID].Data.NEXTSTEPID;
                            _info.job.RobotWIP.RTCReworkFlag = true;
                        }

                        strlog = string.Format("Job(" + _info.job.CassetteSequenceNo + "," + _info.job.JobSequenceNo + ") step is jump to " + _info.job.RobotWIP.CurStepNo + ", and Robot Status is " + curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus +
                           ", job's FetchOutDataTime：" + _info.job.RobotWIP.FetchOutDataTime.ToString() +", Robot cmd01 is "  + curRobot.CurRealTimeSetCommandInfo.Cmd01_Command + ", Robot cmd02 is " + curRobot.CurRealTimeSetCommandInfo.Cmd02_Command);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(_info.job);

                        //20160504 modify,add curJumpGotoStepCanUseStageList,避免CurStep已經Jump,但是CanUseStageList沒有Jump
                        #region [ Get Jump GoTo Step Can Use StageList ]

                        RobotRouteStep curJumpGotoRouteStep = _info.job.RobotWIP.RobotRouteStepList[_rtcGoToStepID];

                        string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

                        //取得目前Step的CurCanUseStageList做關聯後清除
                        List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                        if (curJumpGotoStepCanUseStageList != null)
                        {
                            curJumpGotoStepCanUseStageList.Clear();
                        }
                        else
                        {
                            curJumpGotoStepCanUseStageList = new List<RobotStage>();
                        }


                        for (int i = 0; i < curStepCanUseStageList.Length; i++)
                        {

                            #region [ Check Stage is Exist ]

                            RobotStage curStage;

                            curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                            //找不到 Robot Stage 回NG
                            if (curStage == null)
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                return false;
                            }

                            if (curJumpGotoStepCanUseStageList.Contains(curStage) == false)
                            {

                                curJumpGotoStepCanUseStageList.Add(curStage);

                            }

                            #endregion

                        }
                        #endregion
                    }

                }


                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                //robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
            finally
            {
                _info = null;
            }

        }
        //modify by Yang 
        //20160525 Job在EQP裡,等待時間到,下游EQP不收片,則RTC回CST
        //modify 没有卡Job必须在EQP里,下游EQP不收片，则RTC回CST
        [UniAuto.UniBCS.OpiSpec.Help("JP0026")]
        public bool RouteStepJump_EQPWaitRTC(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;

            RobotInfo _info = new RobotInfo(robotConText);


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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                if (curRobot.CurRealTimeSetCommandInfo.Cmd01_Command != 0 || curRobot.CurRealTimeSetCommandInfo.Cmd02_Command != 0)
                    return true;

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

                if (_info.ErrMsg.ToString(MethodBase.GetCurrentMethod().Name) != string.Empty)
                {
                    if (IsShowDetialLog) Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _info.ErrMsg.ToString("[EQUIPMENT={1}] [RCS <- RCS]", MethodBase.GetCurrentMethod().Name, eRobotContextParameter.MPLCLocalNo));
                    robotConText.SetReturnMessage(_info.ErrMsg.ToString(MethodBase.GetCurrentMethod().Name));

                    return false;
                }
           
                #region [ Check是否不為PreFetch ][EQP也存在预取情况,所以这里还是要卡住EQP预取,不用帮忙RTC][预取的也要帮忙check]
                /*
                if (curBcsJob.RobotWIP.PreFetchFlag > 0)

                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Pre FetchFlag=({4}) can not RTC!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.PreFetchFlag.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True不須Jump

                    return true;

                }
                */
                #endregion
                
                #region [ Check Robot Arm Type ]

                if (curRobot.Data.ARMJOBQTY != 1)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Arm Job Qty({1}) is illegal!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get 2nd Command Check Flag ]

                bool is2ndCmdFlag = false;

                try
                {
                    is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by curStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by curStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;

                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    }
                }
                else
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by nextStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by nextStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
                    }

                }

                #endregion

                #region [ Check CheckStep Action Must Put ]

                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True

                    return true;
                }

                #endregion

                #region [ Get RTC GotoStepID ]
                int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (GoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is EQP RTC But GotoStepID({5}) is Fail!",
                                                _info.robot.Data.NODENO, _info.robot.Data.ROBOTNAME, _info.job.CassetteSequenceNo, _info.job.JobSequenceNo, _info.step.Data.STEPID.ToString(), GoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is EQP RTC But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, _info.job.CassetteSequenceNo, _info.job.JobSequenceNo,
                                            _info.step.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTC_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (!_info.job.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID))
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is EQP RTC But get GotoStepID({5}) Entity Fail!",
                                                _info.robot.Data.NODENO, _info.robot.Data.ROBOTNAME, _info.job.CassetteSequenceNo, _info.job.JobSequenceNo, _info.step.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is EQP RTC But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, _info.job.CassetteSequenceNo, _info.job.JobSequenceNo,
                                            _info.step.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTC_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [Check RTC Condition]

                    bool _RTCTimeout = false;
                    bool _DailyCheck = true;
                    bool _CVDCLNRTC = false;

                    #region [Array CVD 300/400，CLN出片时 考量是否RTC][1.(300/400)CLN RTC  2. (All CVD Type)ARM RTC]                                      
                        //Yang 20161001
                        //遍历RTC情况
                    if (curRobot.Data.LINETYPE.Contains("CVD_"))
                    {
                        eBitResult l1cleanout = Check_CVD_EQInterLock_LoadLock1CleanOutBit(curRobot);
                        eBitResult l2cleanout = Check_CVD_EQInterLock_LoadLock2CleanOutBit(curRobot);

                        List<RobotStage> stagelist = (List<RobotStage>)ObjectManager.RobotStageManager.GetRobotStages();
                                                                         
                        RobotStage LL1 =stagelist.Where(s=>s.Data.REMARKS.Equals("LL1")).FirstOrDefault();
                        RobotStage LL2 = stagelist.Where(s => s.Data.REMARKS.Equals("LL2")).FirstOrDefault();

                        if (curRobot.CLNRTCWIP)
                        {
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) CST Have Glass({3}) to CVD!",
                                                        curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME,curRobot.CLNRTCWIP);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            _CVDCLNRTC = true;//CST里有step去CVD的，遵循先洗完的glass先服务CVD(For CVD 300/400)
                        }
                        else
                        {
                            if (l1cleanout == eBitResult.ON && l2cleanout == eBitResult.ON
                                 && (!curBcsJob.RobotWIP.CurLocation_StageType.Equals(eRobotStageType.EQUIPMENT)))  //这边CVD都不能去并且基板不在EQP,才帮忙RTC
                            {
                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) CVD(LL1,LL2) Clean Out,Glass to CVD need RTC !",
                                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                _CVDCLNRTC = true;
                            }

                            #region[for CVD300,400 基板在EQP时的特殊考量  遍历LL1 LL2无法送片的情况]
                            //if (curRobot.Data.LINEID.Contains("CVD300") || curRobot.Data.LINEID.Contains("CVD400")) //add by yang
                            //yang 2017/5/25 换种写法
                            if (ConstantManager[eNoNeedSendToCLN.NONEEDSENDTOCLN].Values.ContainsKey(curRobot.Data.LINEID))
                            {
                                if (ConstantManager[eNoNeedSendToCLN.NONEEDSENDTOCLN].Values.Where(s => s.Key.Equals(curRobot.Data.LINEID)).FirstOrDefault().Value.Value.Equals("true"))
                                {
                                    if (l1cleanout == eBitResult.ON && LL2.File.CurStageStatus == eRobotStageStatus.NO_REQUEST)

                                        _CVDCLNRTC = true;

                                    if (l2cleanout == eBitResult.ON && LL1.File.CurStageStatus == eRobotStageStatus.NO_REQUEST)

                                        _CVDCLNRTC = true;

                                    if (LL1.File.CurStageStatus == eRobotStageStatus.NO_REQUEST && LL2.File.CurStageStatus == eRobotStageStatus.NO_REQUEST)

                                        _CVDCLNRTC = true;

                                }
                            }
                            #endregion
                        }
                    }
                    #endregion

                    #region[Check EQPRTC_WAIT_TIME]
                        if (_info.job.RobotWIP.StoreDateTime != DateTime.MinValue) //如果都沒進EQP,例如CurStep=1,NextStep=6,StoreDateTime = DateTime.MinValue,_RTCTimeout = false
                     {
                         _RTCTimeout = (bool)Invoke("RobotSpecialService", "Check_EQPRTC_Timeout_For1Arm1Job", new object[] { robotConText });
                     }
                    #endregion

                    #region [ L3 Daily Check ON && L3 Aging Disable]

                    if (curRobot.Data.LINETYPE.Contains("TTP")||curRobot.Data.LINETYPE.Contains("MQC"))
                {
                     _DailyCheck = false;
                        string[] curCheckStepStageIDList = curCheckRouteStep.Data.STAGEIDLIST.Split(',');
                        //20160525
                        RobotStage curLocationStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curBcsJob.RobotWIP.CurLocation_StageID);
                        for (int i = 0; i < curCheckStepStageIDList.Length; i++)
                        {
                            RobotStage curStage;

                            curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curCheckStepStageIDList[i]);

                            //找不到 Robot Stage 還是要繼續往下做
                            if (curStage == null)
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curCheckStepStageIDList[i]);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                return false;
                            }
                            if (curLocationStage == null)
                            {
                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Get curLocationStage({3})!",
                                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurLocation_StageID);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion
                            }
                            else if (curLocationStage.Data.NODENO == "L3" && Check_TTP_EQInterlock_DailyCheckBit(curRobot) == eBitResult.ON)
                            {
                                _DailyCheck = true;
                            }
                            #region[Daily Check ON/OFF DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageID({3}) _DailyCheck is ({4})",
                                                        curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curLocationStage.Data.STAGEID, _DailyCheck.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                }
                        #endregion

                #endregion

                 //EQP RTC時間到 ,沒有Daily Check, EQPRTCFlag OFF ,才可以RTC Jump回CST等待
                 //增加CVD CLN RTC Yang 这里放在一起判断是否真的可以jump
                    if (((_CVDCLNRTC) || (_RTCTimeout && !_DailyCheck)) && !curBcsJob.RobotWIP.EQPRTCFlag)
                    {
                        #region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

                        if (is2ndCmdFlag == false)
                        {

                            #region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

                            //Step 切換一定要紀錄Log 
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is EQP RTC Jump Step to ({5})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is EQP RTC Jump Step to ({4})!",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                            robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTC_Is_Fail);
                            robotConText.SetReturnMessage(errMsg);
                            curBcsJob.RobotWIP.TempStepNo = curBcsJob.RobotWIP.CurStepNo;
                            //有變化才記Log並存檔
                            if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
                                curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is EQP RTC Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                        curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                lock (curBcsJob)
                                {
                                    curBcsJob.RobotWIP.CurStepNo = GoToStepID;
                                    curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
                                }

                                //Save File
                                ObjectManager.JobManager.EnqueueSave(curBcsJob);

                            }

                            #endregion

                        }
                        else
                        {

                            #region [ Update NextStepID by JumpGotoSTEPID Setting ]

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is EQP RTC Jump Step to ({5})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is EQP RTC Jump Step to ({4})!",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                            robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTC_Is_Fail);
                            robotConText.SetReturnMessage(errMsg);
                            curBcsJob.RobotWIP.TempStepNo = curBcsJob.RobotWIP.NextStepNo;
                            //有變化才記Log並存檔
                            if (curBcsJob.RobotWIP.NextStepNo != GoToStepID)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is EQP RTC Jump Step to ({5})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                lock (curBcsJob)
                                {
                                    curBcsJob.RobotWIP.NextStepNo = GoToStepID;
                                }

                                //Save File
                                ObjectManager.JobManager.EnqueueSave(curBcsJob);

                            }

                            #endregion

                        }
                      

                        curBcsJob.RobotWIP.EQPRTCFlag = true;                   
                        curBcsJob.RobotWIP.RTCReworkFlag = true;
                        

                        #region [ Get Jump GoTo Step Entity ]

                        if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                        {
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        GoToStepID.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    GoToStepID.ToString());

                            robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion

                        #region [ Get Jump GoTo Step Can Use StageList ]

                        RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

                        string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

                        //取得目前Step的CurCanUseStageList做關聯後清除
                        List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                        if (curJumpGotoStepCanUseStageList != null)
                        {
                            curJumpGotoStepCanUseStageList.Clear();
                        }
                        else
                        {
                            curJumpGotoStepCanUseStageList = new List<RobotStage>();
                        }


                        for (int i = 0; i < curStepCanUseStageList.Length; i++)
                        {

                            #region [ Check Stage is Exist ]

                            RobotStage curStage;

                            curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                            //找不到 Robot Stage 回NG
                            if (curStage == null)
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                return false;
                            }

                            if (curJumpGotoStepCanUseStageList.Contains(curStage) == false)
                            {                              
                                    curJumpGotoStepCanUseStageList.Add(curStage);

                            }

                            #endregion

                        }

                        #endregion

                        #endregion
                    }

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                //robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
            finally
            {
                _info = null;
            }

        }

        /// <summary>CheckStep Is Froce Clean Out or not .If Force Cleaner Out then Jump Goto StepID
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0001")]
        public bool RouteStepJump_ForceCleanOut(IRobotContext robotConText)
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot 所屬Line Entity ]

                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (robotLine == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Line Entity!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check IndexOperMode Force clean Out Mode ]

                if (robotLine.File.IndexOperMode != eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                {
                    //非Force CleanOut Mode則不做Jump直接Reply True
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
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

                #region [ Check Robot Arm Type ]

                if (curRobot.Data.ARMJOBQTY != 1)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Arm Job Qty({1}) is illegal!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get 2nd Command Check Flag ]

                bool is2ndCmdFlag = false;

                try
                {
                    is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}]can not Set Is2ndCmdCheckFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by curStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by curStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
    
                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    }
                }
                else
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by nextStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by nextStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
                    }

                }

                #endregion

                #region [ Check CheckStep Action Must Put ]

                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True

                    return true;
                }

                #endregion

                #region [ Get Force Clean Out GotoStepID ]

                int forceCleanOutGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (forceCleanOutGoToStepID == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Force Clean Out But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(forceCleanOutGoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Force Clean Out But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

                if (is2ndCmdFlag == false)
                {

                    #region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

                    //Step 切換一定要紀錄Log 
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Force Clean Out Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOutJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != forceCleanOutGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[forceCleanOutGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Force Clean Out Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[forceCleanOutGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = forceCleanOutGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[forceCleanOutGoToStepID].Data.NEXTSTEPID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }
                else
                {

                    #region [ Update NextStepID by JumpGotoSTEPID Setting ]

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is Force Clean Out Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOutJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != forceCleanOutGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = forceCleanOutGoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }

                #region [ Get Jump GoTo Step Entity ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(forceCleanOutGoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                forceCleanOutGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            forceCleanOutGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[forceCleanOutGoToStepID];

                string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

                //取得目前Step的CurCanUseStageList做關聯後清除
                List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curJumpGotoStepCanUseStageList != null)
                {
                    curJumpGotoStepCanUseStageList.Clear();
                }
                else
                {
                    curJumpGotoStepCanUseStageList = new List<RobotStage>(); 
                }
                

                for (int i = 0; i < curStepCanUseStageList.Length; i++)
                {

                    #region [ Check Stage is Exist ]

                    RobotStage curStage;

                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    if (curJumpGotoStepCanUseStageList.Contains(curStage) == false)
                    {

                        curJumpGotoStepCanUseStageList.Add(curStage);

                    }

                    #endregion

                }

                #endregion

                //Update CurCanUseJobList 20151019 mark 不須重新指定給值
                //robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curJumpGotoStepCanUseStageList);

                #endregion

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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0002")]
        public bool RouteStepJump_VCRDisable_Cut(IRobotContext robotConText)
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot 所屬Line Entity ]

                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (robotLine == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Line Entity!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion
                #region [Line Type == BFG_SHUZTUNG ]
                if (robotLine.Data.LINETYPE != eLineType.ARRAY.BFG_SHUZTUNG)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) is not BFG_SHUZTUNG Line!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Is not BFG_SHUZTUNG Line!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
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
                #region [ Check VCR is Disabled ]
                if (CheckStageIsVCRDisable(curBcsJob)==false)
                {
                    //非VCR Disable則不做Jump直接Reply True
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }
                if (curBcsJob.RobotWIP.CurStepNo != 1)
                {
                    //不是在Step=1,則不做Jump直接Reply True
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion
                #region [ Check Robot Arm Type ]

                if (curRobot.Data.ARMJOBQTY != 1)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Arm Job Qty({1}) is illegal!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get 2nd Command Check Flag ]

                bool is2ndCmdFlag = false;

                try
                {
                    is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by curStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by curStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;

                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    }
                }
                else
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by nextStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by nextStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
                    }

                }

                #endregion

                #region [ Check CheckStep Action Must Put ]

                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True

                    return true;
                }

                #endregion

                #region [ Get GotoStepID ]

                int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (GoToStepID == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRDisable_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRDisable_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

                if (is2ndCmdFlag == false)
                {

                    #region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

                    //Step 切換一定要紀錄Log 
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}]Job({1}_{2}) curStepNo({3}) is Force Clean Out Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRDisable_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is VCR Disable Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = GoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }
                else
                {

                    #region [ Update NextStepID by JumpGotoSTEPID Setting ]

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is VCR Disable Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRDisable_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != GoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is VCR Disable Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = GoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }

                #region [ Get Jump GoTo Step Entity ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

                string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

                //取得目前Step的CurCanUseStageList做關聯後清除
                List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curJumpGotoStepCanUseStageList != null)
                {
                    curJumpGotoStepCanUseStageList.Clear();
                }
                else
                {
                    curJumpGotoStepCanUseStageList = new List<RobotStage>();
                }


                for (int i = 0; i < curStepCanUseStageList.Length; i++)
                {

                    #region [ Check Stage is Exist ]

                    RobotStage curStage;

                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    if (curJumpGotoStepCanUseStageList.Contains(curStage) == false)
                    {

                        curJumpGotoStepCanUseStageList.Add(curStage);

                    }

                    #endregion

                }

                #endregion

                //Update CurCanUseJobList 20151019 mark 不須重新指定給值
                //robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curJumpGotoStepCanUseStageList);

                #endregion

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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0006")]
        public bool RouteStepJump_VCRNotDisable_Smash(IRobotContext robotConText)
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot 所屬Line Entity ]

                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (robotLine == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Line Entity!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion
                #region [Line Type == BFG_SHUZTUNG ]
                if (robotLine.Data.LINETYPE != eLineType.ARRAY.BFG_SHUZTUNG)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) is not BFG_SHUZTUNG Line!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Is not BFG_SHUZTUNG Line!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
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
                #region [ Check VCR is Not Disabled ]

                if (CheckStageIsVCRDisable(curBcsJob) == true)
                {
                    //VCR Disable則不做Jump直接Reply True
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }
                if (curBcsJob.RobotWIP.CurStepNo != 1)
                {
                    //不是在Step=1,則不做Jump直接Reply True
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion
                #region [ Check Robot Arm Type ]

                if (curRobot.Data.ARMJOBQTY != 1)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Arm Job Qty({1}) is illegal!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get 2nd Command Check Flag ]

                bool is2ndCmdFlag = false;

                try
                {
                    is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by curStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by curStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;

                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    }
                }
                else
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by nextStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by nextStepNo({3})!",
                                                 MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
                    }

                }

                #endregion

                #region [ Check CheckStep Action Must Put ]

                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True

                    return true;
                }

                #endregion

                #region [ Get GotoStepID ]

                int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (GoToStepID == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRNotDisable_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRNotDisable_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

                if (is2ndCmdFlag == false)
                {

                    #region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

                    //Step 切換一定要紀錄Log 
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Force Clean Out Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Force Clean Out Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = GoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }
                else
                {

                    #region [ Update NextStepID by JumpGotoSTEPID Setting ]

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is Force Clean Out Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != GoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = GoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }

                #region [ Get Jump GoTo Step Entity ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

                string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

                //取得目前Step的CurCanUseStageList做關聯後清除
                List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curJumpGotoStepCanUseStageList != null)
                {
                    curJumpGotoStepCanUseStageList.Clear();
                }
                else
                {
                    curJumpGotoStepCanUseStageList = new List<RobotStage>();
                }


                for (int i = 0; i < curStepCanUseStageList.Length; i++)
                {

                    #region [ Check Stage is Exist ]

                    RobotStage curStage;

                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    if (curJumpGotoStepCanUseStageList.Contains(curStage) == false)
                    {

                        curJumpGotoStepCanUseStageList.Add(curStage);

                    }

                    #endregion

                }

                #endregion

                //Update CurCanUseJobList 20151019 mark 不須重新指定給值
                //robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curJumpGotoStepCanUseStageList);

                #endregion

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

        private bool CheckStageIsVCRDisable(Job curJob)
        {
            try
            {
                string strlog = string.Empty;
                Equipment stageEQP = ObjectManager.EquipmentManager.GetEQP("L3");
                if (stageEQP == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT=11]can not find EQP!");
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }
                if (stageEQP.File.VcrMode == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT=11] Can not find EQP VCRMode!");
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }
                foreach (eBitResult vcrmode in stageEQP.File.VcrMode)
                {
                    if (vcrmode == eBitResult.OFF)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT=11]VCR is Disable");
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        return true;
                    }
                    else
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT=11]VCR is Enable");
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0003")]
        public bool RouteStepJump_VCRNG_Cut(IRobotContext robotConText)
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot 所屬Line Entity ]

                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (robotLine == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Line Entity!",
                                            curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion
                #region [Line Type == BFG_SHUZTUNG ]
                if (robotLine.Data.LINETYPE != eLineType.ARRAY.BFG_SHUZTUNG)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) is not BFG_SHUZTUNG Line!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Is not BFG_SHUZTUNG Line!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
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
                #region [ Check VCR is NG ]
                if (curBcsJob.ArraySpecial.RtcFlag == "0")
                {
                    //非VCR NG則不做Jump直接Reply True
                    //RTCFlag=0 =>到Cut不Jump,直接Reply True;RTCFlag=1 =>Jump回CST
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion
                #region [ Check Robot Arm Type ]

                if (curRobot.Data.ARMJOBQTY != 1)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Arm Job Qty({1}) is illegal!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get 2nd Command Check Flag ]

                bool is2ndCmdFlag = false;

                try
                {
                    is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                             MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by curStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by curStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;

                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    }
                }
                else
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by nextStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by nextStepNo({3})!",
                                                curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
                    }

                }

                #endregion

                #region [ Check CheckStep Action Must Put ]

                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True

                    return true;
                }

                #endregion

                #region [ Get GotoStepID ]

                int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (GoToStepID == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRNG_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRNG_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

                if (is2ndCmdFlag == false)
                {

                    #region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

                    //Step 切換一定要紀錄Log 
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is VCR NG Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRNG_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is VCR NG Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = GoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }
                else
                {

                    #region [ Update NextStepID by JumpGotoSTEPID Setting ]

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is VCR NG Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRNG_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != GoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is VCR NG Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = GoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }

                #region [ Get Jump GoTo Step Entity ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                             MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

                string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

                //取得目前Step的CurCanUseStageList做關聯後清除
                List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curJumpGotoStepCanUseStageList != null)
                {
                    curJumpGotoStepCanUseStageList.Clear();
                }
                else
                {
                    curJumpGotoStepCanUseStageList = new List<RobotStage>();
                }


                for (int i = 0; i < curStepCanUseStageList.Length; i++)
                {

                    #region [ Check Stage is Exist ]

                    RobotStage curStage;

                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    if (curJumpGotoStepCanUseStageList.Contains(curStage) == false)
                    {

                        curJumpGotoStepCanUseStageList.Add(curStage);

                    }

                    #endregion

                }

                #endregion

                //Update CurCanUseJobList 20151019 mark 不須重新指定給值
                //robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curJumpGotoStepCanUseStageList);

                #endregion

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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0017")]
        public bool RouteStepJump_L2VCRDisable(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;

            try
            {
                Thread.Sleep(1000);

                #region [ Get curRobot Entity ]
                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];
                //找不到 Robot 回NG
                if (curRobot == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region [ Get Robot 所屬Line Entity ]
                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if (robotLine == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Line Entity!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Check L2 VCR Enable Mode ]
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                    if (eqp == null)
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Cannot GetEQP(L2)",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        errMsg = string.Format("[{0}] Cannot GetEQP(L2)",
                                                MethodBase.GetCurrentMethod().Name);

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }

                    //if (eqp.File.VcrMode.Count == 0)
                    //{
                    //    //沒有VCR, 不做Jump
                    //    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    //    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    //    return true;
                    //}

                    for (int i = 0; i < eqp.File.VcrMode.Count; i++)
                    {
                        if (eqp.File.VcrMode[i] == eBitResult.ON)
                        {
                            //有一支VCR Enable ON, 不做Jump
                            robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                            robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                            return true;
                        }
                    }
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

                #region [ Check Robot Arm Type ]
                if (curRobot.Data.ARMJOBQTY != 1)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Arm Job Qty({1}) is illegal!",
                                             MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get 2nd Command Check Flag ]
                bool is2ndCmdFlag = false;
                try
                {
                    is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                             MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]
                RobotRouteStep curCheckRouteStep = null;
                if (is2ndCmdFlag == false)
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false)
                    {
                        //找不到 CurStep Route 回NG
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by curStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by curStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;

                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
                    }
                }
                else
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false)
                    {
                        //找不到 CurStep Route 回NG
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by nextStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by nextStepNo({3})!",
                                                 MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
                    }
                }
                #endregion

                #region [ Check CheckStep Action Must Put ]
                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    //不影響所以直接回傳True
                    return true;
                }
                #endregion

                #region [ Get VCR Disable GotoStepID ]
                int goto_stepid = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];
                if (goto_stepid == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) VCR is Disable But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), goto_stepid.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) VCR is Disable But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), goto_stepid.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRDisable_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(goto_stepid) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), goto_stepid.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) VCR is Disable But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), goto_stepid.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRDisable_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

                if (is2ndCmdFlag == false)
                {
                    #region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo
                    //Step 切換一定要紀錄Log 
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), goto_stepid.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) VCR is Disable Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), goto_stepid.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOutJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != goto_stepid ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[goto_stepid].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) VCR is Disable Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), goto_stepid.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[goto_stepid].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = goto_stepid;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[goto_stepid].Data.NEXTSTEPID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }
                else
                {
                    #region [ Update NextStepID by JumpGotoSTEPID Setting ]
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), goto_stepid.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) VCR is Disable Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), goto_stepid.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_VCRNotDisable_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != goto_stepid)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) VCR is Disable Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), goto_stepid.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = goto_stepid;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(goto_stepid) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                goto_stepid.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            goto_stepid.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]
                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[goto_stepid];
                string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');
                //取得目前Step的CurCanUseStageList做關聯後清除
                List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                if (curJumpGotoStepCanUseStageList != null)
                {
                    curJumpGotoStepCanUseStageList.Clear();
                }
                else
                {
                    curJumpGotoStepCanUseStageList = new List<RobotStage>();
                }
                for (int i = 0; i < curStepCanUseStageList.Length; i++)
                {
                    #region [ Check Stage is Exist ]
                    RobotStage curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);
                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        return false;
                    }

                    if (curJumpGotoStepCanUseStageList.Contains(curStage) == false)
                    {
                        curJumpGotoStepCanUseStageList.Add(curStage);
                    }
                    #endregion
                }
                #endregion

                #endregion

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

        
        //20151209 add for Froce Return CST Without LDRQ
        /// <summary> Check是否要定時將從Arm上從CST取出的Job回送到CST去避免佔住CST
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("JP0018")]
        public bool RouteStepJump_ForceReturnCSTWithoutLDRQ(IRobotContext robotConText)
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot 所屬Line Entity ]

                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (robotLine == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Line Entity!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
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

                #region [ Check是否不為PreFetch ]

                if (curBcsJob.RobotWIP.PreFetchFlag > 0)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Pre FetchFlag=({4}) can not Froce Return CST Without LDRQ!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.PreFetchFlag.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True不須Jump

                    return true;

                }

                #endregion

                #region [ Check 是否啟用Force Return CST Without LDRQ功能 ]

                if (curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status != eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_START)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ForceReturnCSTWithoutLDRQ_Status({4}) can not Froce Return CST Without LDRQ!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True不須Jump

                    return true;

                }

                #endregion

                #region [ Check 是否超過Froce Return CST Without LDRQ 等待的時間 ]

                //防止Config設定異常,預設5分鐘300s 
                int froceReturnCSTWithoutLDRQ_TimeOut = 300;

                try
                {
                    froceReturnCSTWithoutLDRQ_TimeOut = ParameterManager[eRobotCommonConst.ROBOT_FORCE_RETURN_CST_WITHOUT_LDRQ_TIMEOUT_CONSTANT_KEY].GetInteger();
                }
                catch (Exception ex1)
                {
                    //this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex1);
                }

                double waitLDRQtime;

                waitLDRQtime = (DateTime.Now - Convert.ToDateTime(curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_MonitorStartTime)).TotalSeconds;

                if (waitLDRQtime < froceReturnCSTWithoutLDRQ_TimeOut)
                {
                    //尚未到達時間 
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ForceReturnCSTWithoutLDRQ_MonitorStartTime({4}) not Over({5})ms and can not Froce Return CST Without LDRQ!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_MonitorStartTime.ToString(), waitLDRQtime.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True不須Jump

                    return true;
                }

                //超過TimeOut 開始準備Jump

                #endregion

                #region [ Check Robot Arm Type ]

                if (curRobot.Data.ARMJOBQTY != 1)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Arm Job Qty({1}) is illegal!",
                                             MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get 2nd Command Check Flag ]

                bool is2ndCmdFlag = false;

                try
                {
                    is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by curStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by curStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;

                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    }
                }
                else
                {

                    //此Functon屬於Arm上有Job專用 ,每片進來都必須是第一筆命令
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) is2ndCmdFlag(True) can not Froce Return CST Without LDRQ!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True不須Jump

                    return true;

                }

                #endregion

                #region [ Check CheckStep Action Must Put ]

                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True

                    return true;
                }

                #endregion

                #region [ Get Force Return CST Without LDRQ GotoStepID ]

                int forceReturnCSTGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (forceReturnCSTGoToStepID == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Return CST Without LDRQ But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Force Return CST Without LDRQ But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceReturnCSTWithoutLDRQ_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(forceReturnCSTGoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Return CST Without LDRQ But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Force Return CST Without LDRQ But get GotoStepID({4}) Entity Fail",
                                             MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

                if (is2ndCmdFlag == false)
                {

                    #region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

                    //Step 切換一定要紀錄Log 
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Force Clean Out Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOutJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != forceReturnCSTGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[forceReturnCSTGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Force Clean Out Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[forceReturnCSTGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = forceReturnCSTGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[forceReturnCSTGoToStepID].Data.NEXTSTEPID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }
                else
                {
                    ////此Functon屬於Arm上有Job專用 ,每片進來都必須是第一筆命令.在此之前已經濾掉                  

                }

                #region [ Get Jump GoTo Step Entity ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(forceReturnCSTGoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                forceReturnCSTGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            forceReturnCSTGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[forceReturnCSTGoToStepID];

                string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

                //取得目前Step的CurCanUseStageList做關聯後清除
                List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curJumpGotoStepCanUseStageList != null)
                {
                    curJumpGotoStepCanUseStageList.Clear();
                }
                else
                {
                    curJumpGotoStepCanUseStageList = new List<RobotStage>();
                }


                for (int i = 0; i < curStepCanUseStageList.Length; i++)
                {

                    #region [ Check Stage is Exist ]

                    RobotStage curStage;

                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    if (curJumpGotoStepCanUseStageList.Contains(curStage) == false)
                    {

                        curJumpGotoStepCanUseStageList.Add(curStage);

                    }

                    #endregion

                }

                #endregion

                #endregion

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

        //20160603
        [UniAuto.UniBCS.OpiSpec.Help("JP0024")]
        public bool RouteStepJump_AbnormalForceCleanOut(IRobotContext robotConText)
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot 所屬Line Entity ]

                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (robotLine == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Line Entity!",
                                             MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check IndexOperMode Force clean Out Mode ]

                if (robotLine.File.IndexOperMode != eINDEXER_OPERATION_MODE.ABNORMAL_FORCE_CLEAN_OUT_MODE)
                {
                    //非Force CleanOut Mode則不做Jump直接Reply True
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
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

                #region [ Check Robot Arm Type ]

                if (curRobot.Data.ARMJOBQTY != 1)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Arm Job Qty({1}) is illegal!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get 2nd Command Check Flag ]

                bool is2ndCmdFlag = false;

                try
                {
                    is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                             MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by curStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by curStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;

                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    }
                }
                else
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by nextStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by nextStepNo({3})!",
                                                 MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
                    }

                }

                #endregion

                #region [ Check CheckStep Action Must Put ]

                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True

                    return true;
                }

                #endregion

                #region [ Get Force Clean Out GotoStepID ]

                int forceCleanOutGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (forceCleanOutGoToStepID == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Abmormal Force Clean Out But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Abmormal Force Clean Out But GotoStepID({4}) is Fail!",
                                             MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(forceCleanOutGoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Abmormal Force Clean Out But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Abmormal Force Clean Out But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

                if (is2ndCmdFlag == false)
                {

                    #region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

                    //Step 切換一定要紀錄Log 
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Abmormal Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Abmormal Force Clean Out Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOutJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != forceCleanOutGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[forceCleanOutGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Abmormal Force Clean Out Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[forceCleanOutGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = forceCleanOutGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[forceCleanOutGoToStepID].Data.NEXTSTEPID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }
                else
                {

                    #region [ Update NextStepID by JumpGotoSTEPID Setting ]

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Abmormal Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is Abmormal Force Clean Out Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOutJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != forceCleanOutGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Abmormal Force Clean Out Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceCleanOutGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = forceCleanOutGoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }

                #region [ Get Jump GoTo Step Entity ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(forceCleanOutGoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                forceCleanOutGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            forceCleanOutGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[forceCleanOutGoToStepID];

                string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

                //取得目前Step的CurCanUseStageList做關聯後清除
                List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curJumpGotoStepCanUseStageList != null)
                {
                    curJumpGotoStepCanUseStageList.Clear();
                }
                else
                {
                    curJumpGotoStepCanUseStageList = new List<RobotStage>();
                }


                for (int i = 0; i < curStepCanUseStageList.Length; i++)
                {

                    #region [ Check Stage is Exist ]

                    RobotStage curStage;

                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    if (curJumpGotoStepCanUseStageList.Contains(curStage) == false)
                    {

                        curJumpGotoStepCanUseStageList.Add(curStage);

                    }

                    #endregion

                }

                #endregion

                //Update CurCanUseJobList 20151019 mark 不須重新指定給值
                //robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curJumpGotoStepCanUseStageList);

                #endregion

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
        [UniAuto.UniBCS.OpiSpec.Help("JP0029")]// by qiumin 2016/12/12 增加dailycheck时RTC的jump，避免手臂上的glass与TTP做交互片，产生两个dailycheck片。
        public bool RouteStepJump_ForceReturnCSTWithDailyCheck(IRobotContext robotConText)
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot 所屬Line Entity ]

                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (robotLine == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Line Entity!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
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

                #region [ Check是否不為PreFetch ]

                if (curBcsJob.RobotWIP.PreFetchFlag > 0)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Pre FetchFlag=({4}) can not Froce Return CST With Daily Check!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.PreFetchFlag.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True不須Jump

                    return true;

                }

                #endregion

                #region [ Check 是否啟用Force Return CST WithDailyCheck功能 ]
                if (Check_TTP_EQInterlock_DailyCheckBit(curRobot) == eBitResult.ON)  //表示DailyCheck
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] will RTC With Daily Check , [EQUIPMENT={2}] DailyCheck Bit ON!",
                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                    }
                    #endregion
                }
                else
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}] ,JOB_SEQ_NO=[{1}] Daily Check Bit Not 'ON' ,not Change Daily Check Route.",
                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion

                #region [ Check Robot Arm Type ]

                if (curRobot.Data.ARMJOBQTY != 1)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Arm Job Qty({1}) is illegal!",
                                             MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get 2nd Command Check Flag ]

                bool is2ndCmdFlag = false;

                try
                {
                    is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false)
                    {

                        //找不到 CurStep Route 回NG
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by curStepNo({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by curStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;

                    }
                    else
                    {
                        curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    }
                }
                else
                {

                    //此Functon屬於Arm上有Job專用 ,每片進來都必須是第一筆命令
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) is2ndCmdFlag(True) can not Froce Return CST With DailyCheck!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True不須Jump

                    return true;

                }

                #endregion

                #region [ Check CheckStep Action Must Put ]

                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //不影響所以直接回傳True

                    return true;
                }

                #endregion

                #region [ Get Force Return CST With DailyCheck GotoStepID ]

                int forceReturnCSTGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (forceReturnCSTGoToStepID == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Return CST With DailyCheck But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Force Return CST With DailyCheck But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceReturnCSTWithDailyCheck_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(forceReturnCSTGoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Return CST With DailyCheck But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Force Return CST With DailyCheck But get GotoStepID({4}) Entity Fail",
                                             MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

                if (is2ndCmdFlag == false)
                {

                    #region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

                    //Step 切換一定要紀錄Log 
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is RTC with DailyCheck Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is RTC with DailyCheck Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOutJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != forceReturnCSTGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[forceReturnCSTGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Force Clean Out Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), forceReturnCSTGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[forceReturnCSTGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = forceReturnCSTGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[forceReturnCSTGoToStepID].Data.NEXTSTEPID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }
                else
                {
                    ////此Functon屬於Arm上有Job專用 ,每片進來都必須是第一筆命令.在此之前已經濾掉                  

                }

                #region [ Get Jump GoTo Step Entity ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(forceReturnCSTGoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                forceReturnCSTGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            forceReturnCSTGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[forceReturnCSTGoToStepID];

                string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

                //取得目前Step的CurCanUseStageList做關聯後清除
                List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curJumpGotoStepCanUseStageList != null)
                {
                    curJumpGotoStepCanUseStageList.Clear();
                }
                else
                {
                    curJumpGotoStepCanUseStageList = new List<RobotStage>();
                }


                for (int i = 0; i < curStepCanUseStageList.Length; i++)
                {

                    #region [ Check Stage is Exist ]

                    RobotStage curStage;

                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    if (curJumpGotoStepCanUseStageList.Contains(curStage) == false)
                    {

                        curJumpGotoStepCanUseStageList.Add(curStage);

                    }

                    #endregion

                }

                #endregion

                #endregion

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




    }
}
