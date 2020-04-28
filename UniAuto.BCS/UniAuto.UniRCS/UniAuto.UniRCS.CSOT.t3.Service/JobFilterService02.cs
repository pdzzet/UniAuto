using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class JobFilterService
    {

//Filter Funckey = "FL" + XXXX(序列號

        #region All shop / All line / UDRQ Job from EQP Stage ... special check condition (expectations) [针对要出片的基板先检查(预期)]
        /// <summary>[ Wait_Proc_00040 ]針對 Exchange 以及 GETPUT 需要額外確認是否這一片 UDRQ Job 能出片
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <param name="curRobot"></param>
        /// <param name="curStepUseStage"></param>
        /// <param name="curBeforeFilterStageList"></param>
        /// <returns></returns>
        private bool CheckStageUDRQJobCondition_For1Arm1Job(IRobotContext robotConText, RobotStage curStepUseStage, ref List<RobotStage> curBeforeFilterStageList)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

            IRobotContext _tmpContext = new RobotContext();
            List<RobotStage> curFilterCanUseStageList = new List<RobotStage>();

            Robot _robot = null;
            try
            {
                //如果是要出片的時候, 要去看出片的基板是否可以出來!! 如果無法出來, 那就要會NG! [ Wait_Proc_00040 ]

                #region [ Get Robot ]
                _robot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];
                if(_robot == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!", "L1", MethodBase.GetCurrentMethod().Name);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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
                                                _robot.Data.NODENO, _robot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get JobInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion
                #region [ CHK#001 判断Arm是否允许做Exchange (on the fly or getput) -- 目前使用传入的robotConText ]
                int _match = 0;

                //20151208 modify by RealTime ArmInfo
                //foreach (RobotArmSignalSubstrateInfo _arm in _robot.File.ArmSignalSubstrateInfoList)
                bool IsArmJobExist = false;
                foreach (RobotArmSignalSubstrateInfo _arm in _robot.CurTempArmSingleJobInfoList)
                {
                    if (_arm.ArmDisableFlag == eArmDisableStatus.Disable || _arm.ArmJobExist == eGlassExist.Unknown) continue; //disable的arm不考虑
                    if (_arm.ArmJobExist == eGlassExist.Exist) //有基板的arm不考虑 (但是不包含'目前那片基板如果也是要去该stage'的arm)
                    {
                        Job _job = ObjectManager.JobManager.GetJob(_arm.ArmCSTSeq.ToString(), _arm.ArmJobSeq.ToString());
                        if (_job == null) continue; //有基板, 但是找不到job data, 问题基板, 该arm不考虑!
                        RobotRouteStep _step = _job.RobotWIP.RobotRouteStepList[_job.RobotWIP.CurStepNo];
                        if (_step == null) continue; //找不到step, 基板的route有问题, 该arm不考虑!
                        if (!_step.Data.STAGEIDLIST.Contains(curStepUseStage.Data.STAGEID)) continue; //目前要去的stage, 不在找到的stage list里面, 带着跑的基板, 该arm也不考虑!
                        IsArmJobExist = true;
                    }
                    _match++;
                }
                //带片跑会出现这种状况，如果手臂上有其他job，并且当前判断的这片job 不在手臂上 ，就不在继续对这片job 做exchange或get/put判断
                if (IsArmJobExist == true && curBcsJob.RobotWIP.CurLocation_StageID!=eRobotCommonConst.ROBOT_HOME_STAGEID)
                    _match=0;
                if (_match <= 0) //代表可以用的arm没有!!
                {
                    errMsg = string.Format("[{0}] No Arm Can Use.", MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_No_Arm_Can_Use);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_No_Arm_Can_Use;//add for BMS Error Monitor
                    if (!_robot.CheckErrorList.ContainsKey(errCode))
                        _robot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                else if (_match == 1) //代表可以用的arm只有一支!
                {
                    #region [ Get Defind 1st NormalRobotCommand ]
                    DefineNormalRobotCmd cur1stRobotCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.Define_1stNormalRobotCommandInfo];

                    //找不到 1st defineNormalRobotCmd 回NG
                    if (cur1stRobotCmd == null)
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get defineNormalRobotCmd!", _robot.Data.NODENO, _robot.Data.ROBOTNAME);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        errMsg = string.Format("[{0}] can not Get defineNormalRobotCmd!", MethodBase.GetCurrentMethod().Name);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        errCode = eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail;//add for BMS Error Monitor
                        if (!_robot.CheckErrorList.ContainsKey(errCode))
                            _robot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                        return false;
                    }
                    #endregion
                    #region [ Get Defind 2nd NormalRobotCommand ]
                    DefineNormalRobotCmd cur2ndRobotCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.Define_2ndNormalRobotCommandInfo];

                    if (cur2ndRobotCmd == null) //找不到 2nd defineNormalRobotCmd 回NG
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get 2nd defineNormalRobotCmd!", _robot.Data.NODENO, _robot.Data.ROBOTNAME);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        errMsg = string.Format("[{0}] can not Get 2nd defineNormalRobotCmd!", MethodBase.GetCurrentMethod().Name);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_2nd_DefineNormalRobotCommandInfo_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        errCode = eJobFilter_ReturnCode.NG_Get_2nd_DefineNormalRobotCommandInfo_Is_Fail;//add for BMS Error Monitor
                        if (!_robot.CheckErrorList.ContainsKey(errCode))
                            _robot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                        return false;
                    }
                    #endregion

                    switch (cur1stRobotCmd.Cmd01_Command) //第一个命令的动作!!
                    {
                        case eRobot_Trx_CommandAction.ACTION_GET:
                        //如果是单GET!
                        //return true;
                        case eRobot_Trx_CommandAction.ACTION_PUT:
                            //如果是PUT, 因为arm只有一支可以用, 所以无法做Exchange或是GetPut!
                            errMsg = string.Format("[{0}] No Arm Can Use.", MethodBase.GetCurrentMethod().Name);
                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_No_Arm_Can_Use);
                            robotConText.SetReturnMessage(errMsg);
                            errCode = eJobFilter_ReturnCode.NG_No_Arm_Can_Use;//add for BMS Error Monitor
                            if (!_robot.CheckErrorList.ContainsKey(errCode))
                                _robot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                            return false;
                    }
                }
                else //代表可以用的arm有2支或是更多! (目前每个robot最多2支arm!!)
                {
                    //return true;
                }
                #endregion

                
                _robot = null;
                using (_tmpContext as System.IDisposable)
                {
                    #region [ init for _tmpContext ]
                    _tmpContext.Clear();
                    _robot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];
                    _tmpContext.AddParameter(eRobotContextParameter.CurRobotEntity, _robot);
                    _tmpContext.AddParameter(eRobotContextParameter.UDRQ_JOB_FORECAST_CHECK, true);
                    #endregion

                    #region [ CHK#002 接下来需要去判断UDRQ要出来的基板预期的结果!! -- 使用local宣告的_tmpContext ]
                    foreach (string _info in curStepUseStage.curUDRQ_SlotList.Values)
                    {
                        #region [ Get Job ]
                        Job _job = ObjectManager.JobManager.GetJob(_info.Split('_')[0].ToString(), _info.Split('_')[1].ToString());

                        if (_job == null)
                        {
                            errMsg = string.Format("[{0}] Job({1}_{2}) Job Is Null.", MethodBase.GetCurrentMethod().Name, _job.CassetteSequenceNo, _job.JobSequenceNo);
                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                            robotConText.SetReturnMessage(errMsg);
                            errCode = eJobFilter_ReturnCode.NG_curBcsJob_Is_Null;//add for BMS Error Monitor
                            if (!_robot.CheckErrorList.ContainsKey(errCode))
                                _robot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                            return false;
                        }
                        _tmpContext.AddParameter(eRobotContextParameter.CurJobEntity, _job);
                        #endregion

                        int _curStepNo = _job.RobotWIP.CurStepNo;

                        _tmpContext.AddParameter(eRobotContextParameter.UDRQ_JOB_FORECAST_CHECK, true); //如果为真, 代表就是做 Forecast Check 了!

                        #region [ Check CurStep All RouteStepByPass Condition ]
                        if (!CheckAllRouteStepByPassCondition2(_robot, _job, _curStepNo, ref curFilterCanUseStageList, ref _tmpContext))
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check NextStepNo({4}) RouteStepByPassCondition Fail!",
                                                        _robot.Data.NODENO, _robot.Data.ROBOTNAME, _job.CassetteSequenceNo, _job.JobSequenceNo, _curStepNo.ToString());
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) Check NextStepNo({3}) RouteStepByPassCondition Fail!",
                                                    MethodBase.GetCurrentMethod().Name, _job.CassetteSequenceNo, _job.JobSequenceNo, _curStepNo.ToString());

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Chek_NextStep_RouteStepByPassCondition_Fail);
                            robotConText.SetReturnMessage(errMsg);
                            errCode = eJobFilter_ReturnCode.NG_Chek_NextStep_RouteStepByPassCondition_Fail;//add for BMS Error Monitor
                            if (!_robot.CheckErrorList.ContainsKey(errCode))
                                _robot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                            return false; //RouteStepByPass條件有問題則回覆NG
                        }
                        #endregion

                        #region [ Check CurStep All RouteStepJump Condition ]
                        if (!CheckAllRouteStepJumpCondition2(_robot, _job, _curStepNo, ref curFilterCanUseStageList, ref _tmpContext))
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check NextStepNo({4}) RouteStepJumpCondition Fail!",
                                                        _robot.Data.NODENO, _robot.Data.ROBOTNAME, _job.CassetteSequenceNo, _job.JobSequenceNo, _curStepNo.ToString());
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) Check NextStepNo({3}) RouteStepJumpCondition Fail!",
                                                    MethodBase.GetCurrentMethod().Name, _job.CassetteSequenceNo, _job.JobSequenceNo, _curStepNo.ToString());

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Chek_NextStep_RouteStepJumpCondition_Fail);
                            robotConText.SetReturnMessage(errMsg);
                            errCode = eJobFilter_ReturnCode.NG_Chek_NextStep_RouteStepJumpCondition_Fail;//add for BMS Error Monitor
                            if (!_robot.CheckErrorList.ContainsKey(errCode))
                                _robot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                            return false; //RouteStepJump條件有問題則回覆NG
                        }
                        #endregion

                        DefineNormalRobotCmd cur1stDefineCmd = new DefineNormalRobotCmd();
                        DefineNormalRobotCmd cur2ndDefineCmd = new DefineNormalRobotCmd();

                        #region [ Check CurStep All RouteStepFilter Condition ]
                        if (!CheckAllFilterConditionByStepNo2(_robot, _job, _curStepNo, cur1stDefineCmd, cur2ndDefineCmd, ref curFilterCanUseStageList, ref _tmpContext))
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check NextStepNo({4}) FilterCondition Fail!",
                                                        _robot.Data.NODENO, _robot.Data.ROBOTNAME, _job.CassetteSequenceNo, _job.JobSequenceNo, _curStepNo.ToString());
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) Check NextStepNo({3}) FilterCondition Fail!",
                                                    MethodBase.GetCurrentMethod().Name, _job.CassetteSequenceNo, _job.JobSequenceNo, _curStepNo.ToString());

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Chek_NextStep_FilterCondition_Fail);
                            robotConText.SetReturnMessage(errMsg);
                            errCode = eJobFilter_ReturnCode.NG_Chek_NextStep_FilterCondition_Fail;//add for BMS Error Monitor
                            if (!_robot.CheckErrorList.ContainsKey(errCode))
                                _robot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                            return false;
                        }
                        #endregion
                    }
                    #endregion
                }

                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }

        }


        /// <summary>根據RouteStepByPass條件判斷特定StepNo是否出現變化
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkStepNo"></param>
        /// <param name="curStageSelectInfo"></param>
        /// <param name="curBeforeFilterStageList"></param>
        /// <returns></returns>
       
        private bool CheckAllRouteStepByPassCondition_ForJobUnloadCheck_1Arm1Job(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList)
        {
            IRobotContext robotConText = new RobotContext();
            string fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;
            List<RobotStage> curCanUseStageList = new List<RobotStage>();

            try
            {

                List<RobotRuleRouteStepByPass> curRouteStepByPassList = new List<RobotRuleRouteStepByPass>();
                curRouteStepByPassList = ObjectManager.RobotManager.GetRuleRouteStepByPass(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, checkStepNo);

                int ruleCount = 0;

                if (curRouteStepByPassList != null)
                {

                    ruleCount = curRouteStepByPassList.Count;

                }

                #region[DebugLog][ Start Job All RouteStepByPass Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) nextStepNo({5}) Check 2nd Command Rule Job RouteStepByPass ListCount({6}) Start {7}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(), ruleCount.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #region [Check CurStep All RouteStepByPass Condition ]

                #region [ Initial RouteStepByPass Rule List RobotConText Info. 搭配針對File Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);

                //2nd Cmd is2ndCmdFlag is true
                robotConText.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, true);

                #region  [ RouteStepByPass前先預設目前Step都是符合條件的 ]

                string[] curStepCanUseStageList = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST.Split(',');

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

                    if (curCanUseStageList.Contains(curStage) == false)
                    {

                        curCanUseStageList.Add(curStage);

                    }

                    #endregion

                }

                #endregion

                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curCanUseStageList);

                #endregion =======================================================================================================================================================

                #region [ 如果沒有任何StepByPass則直接回覆True ]

                if (curRouteStepByPassList == null)
                {
                    #region[DebugLog][ Start Job All RouteStepByPass Function ]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job RouteStepByPass ListCount({4}) End {5}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                ruleCount.ToString(),
                                                new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //取得Stage Selct後的Can Use Stages List
                    curBeforeFilterStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                    return true;
                }

                #endregion

                #region [ Check RouteStepByPass Condition Process ]

                foreach (RobotRuleRouteStepByPass curRouteStepByPassCondition in curRouteStepByPassList)
                {
                    //Set want To Check Function Fail_ReasonCode,以Rule Job RouteStepByPass 的ObjectName與MethodName為Key來決定是否紀錄Log
                    //因為會出現同Job 確認不同Step所以FailCode要補上StepNo
                    fail_ReasonCode = string.Format("{0}_{1}_{2}", curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, checkStepNo.ToString());

                    #region[DebugLog][ Start Rule Job RouteStepByPass Function ]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass object({4}) MethodName({5}) IsEnable({6}) Start {7}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, curRouteStepByPassCondition.Data.ISENABLED,
                                                new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    if (curRouteStepByPassCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                    {
                        //將ByPass後的GOTOSTEPID
                        robotConText.AddParameter(eRobotContextParameter.RouteStepByPassGotoStepNo, curRouteStepByPassCondition.Data.GOTOSTEPID);

                        checkFlag = (bool)Invoke(curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, new object[] { robotConText });

                        if (checkFlag == false)
                        {

                            #region[DebugLog][ End Rule Job RouteStepByPass Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass Fail, object({4}) MethodName({5}) RtnCode({4})  RtnMsg({6}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME,
                                                        curRouteStepByPassCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0005 ]

                            if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass Fail, object({4}) MethodName({5}) RtnCode({6})  RtnMsg({7}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6})!",
                                //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRouteStepByPassCondition.Data.OBJECTNAME,
                                //                        curRouteStepByPassCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                failMsg = string.Format("Job({0}_{1}) RtnCode({2})  RtnMsg({3})!",
                                                        curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion

                            }

                            #endregion

                            #region[DebugLog][ End Job All StageSelct Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job RouteStepByPass ListCount({4}) End {5}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        ruleCount.ToString(),
                                                        new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //有重大異常直接結束RouteStepByPass邏輯回復NG
                            return false;

                        }
                        else
                        {

                            //Clear[ Robot_Fail_Case_E0004 ]
                            RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                            #region[DebugLog][ End Rule Job RouteStepByPass Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME,
                                                        curRouteStepByPassCondition.Data.ISENABLED,
                                                        new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                        }
                    }
                    else
                    {

                        #region[DebugLog][ End Rule Job RouteStepByPass Function ]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, curRouteStepByPassCondition.Data.ISENABLED,
                                                    new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                    }

                }

                #endregion

                #endregion

                #region[DebugLog][ Start Job All RouteStepByPass Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job RouteStepByPass ListCount({4}) End {5}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            ruleCount.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                //取得Stage Selct後的Can Use Stages List
                curBeforeFilterStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                return true;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                return false;
            }

        }

        private bool CheckAllRouteStepJumpCondition_ForJobUnloadCheck_1Arm1Job(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList)
        {
            IRobotContext robotConText = new RobotContext();
            string fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;

            try
            {

                List<RobotRuleRouteStepJump> curRouteStepJumpList = new List<RobotRuleRouteStepJump>();

                curRouteStepJumpList = ObjectManager.RobotManager.GetRuleRouteStepJump(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, checkStepNo);

                int ruleCount = 0;

                if (curRouteStepJumpList != null)
                {

                    ruleCount = curRouteStepJumpList.Count;

                }

                #region[DebugLog][ Start Job All RouteStepJump Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) nextStepNo({5}) Check 2nd Command Rule Job RouteStepJump ListCount({6}) Start {7}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(), ruleCount.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR_LENGTH));

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #region [Check CurStep All RouteStepJump Condition ]

                #region [ Initial RouteStepJump Rule List RobotConText Info. 搭配針對File Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);

                //2nd Cmd is2ndCmdFlag is true
                robotConText.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, true);

                //拿RuleRouteStepByPass之後的StageIDList來做後續處理
                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curBeforeFilterStageList);

                #endregion =======================================================================================================================================================

                #region [ 如果沒有任何StepJump Condition則直接回覆True ]

                if (curRouteStepJumpList == null)
                {
                    #region[DebugLog][ Start Job All RouteStepJump Function ]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job RouteStepJump ListCount({4}) End {5}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                ruleCount.ToString(),
                                                new string(eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR_LENGTH));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //取得RouteStepJump後的Can Use Stages List(沒Jump沿用傳入的CanUseStageList, 如果改變則要更新為新Step對應的CanUseStageList
                    //curBeforeFilterStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                    return true;
                }

                #endregion

                #region [ Check RouteStepJump Condition Process ]

                foreach (RobotRuleRouteStepJump curRouteStepJumpCondition in curRouteStepJumpList)
                {
                    //Set want To Check Function Fail_ReasonCode,以Rule Job RouteStepJump 的ObjectName與MethodName為Key來決定是否紀錄Log
                    //因為會出現同Job 確認不同Step所以FailCode要補上StepNo
                    fail_ReasonCode = string.Format("{0}_{1}_{2}", curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, checkStepNo.ToString());

                    #region[DebugLog][ Start Rule Job RouteStepJump Function ]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump object({4}) MethodName({5}) IsEnable({6}) Start {7}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, curRouteStepJumpCondition.Data.ISENABLED,
                                                new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    if (curRouteStepJumpCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                    {
                        //將Jump後的GOTOSTEPID送入處理
                        robotConText.AddParameter(eRobotContextParameter.RouteStepJumpGotoStepNo, curRouteStepJumpCondition.Data.GOTOSTEPID);

                        checkFlag = (bool)Invoke(curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, new object[] { robotConText });

                        if (checkFlag == false)
                        {

                            #region[DebugLog][ End Rule Job RouteStepJump Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump Fail, object({4}) MethodName({5}) RtnCode({4})  RtnMsg({6}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME,
                                                        curRouteStepJumpCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0005 ]

                            if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump Fail, object({4}) MethodName({5}) RtnCode({6})  RtnMsg({7}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6})!",
                                //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRouteStepJumpCondition.Data.OBJECTNAME,
                                //                        curRouteStepJumpCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                failMsg = string.Format("Job({0}_{1}) RtnCode({2})  RtnMsg({3})!",
                                                        curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion

                            }

                            #endregion

                            #region[DebugLog][ End Job All StageSelct Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job RouteStepJump ListCount({4}) End {5}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        ruleCount.ToString(),
                                                        new string(eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_END_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_END_CHAR_LENGTH));

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //有重大異常直接結束RouteStepJump邏輯回復NG
                            return false;

                        }
                        else
                        {

                            //Clear[ Robot_Fail_Case_E0004 ]
                            RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                            #region[DebugLog][ End Rule Job RouteStepJump Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME,
                                                        curRouteStepJumpCondition.Data.ISENABLED,
                                                        new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                        }
                    }
                    else
                    {

                        #region[DebugLog][ End Rule Job RouteStepJump Function ]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, curRouteStepJumpCondition.Data.ISENABLED,
                                                    new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                    }

                }

                #endregion

                #endregion

                #region[DebugLog][ Start Job All RouteStepJump Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job RouteStepJump ListCount({4}) End {5}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            ruleCount.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_END_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_END_CHAR_LENGTH));

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                //取得RouteStepJump後的Can Use Stages List(沒Jump沿用傳入的CanUseStageList, 如果改變則要更新為新Step對應的CanUseStageList
                //curBeforeFilterStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                return true;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                return false;
            }

        }

        private bool CheckAllFilterConditionByStepNo_ForJobUnloadCheck1Arm1Job(Robot curRobot, Job curBcsJob, int checkNextStepNo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefineCmd, ref List<RobotStage> curFilterStageList)
        {

            IRobotContext robotConText = new RobotContext();
            string fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;
            try
            {
                List<RobotRuleFilter> curFilterList = new List<RobotRuleFilter>();
                curFilterList = ObjectManager.RobotManager.GetRuleFilter(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, checkNextStepNo);

                #region[DebugLog][ Start Job All Filter Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job Filter ListCount({4}) Start {5}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curFilterList.Count.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_FILTER_START_CHAR, eRobotCommonConst.ALL_RULE_FILTER_START_CHAR_LENGTH));

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #region [ Check CurStep All Filter Condition ]

                #region [ Initial Filter Rule List RobotConText Info. 搭配針對File Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);

                //DB Spec define : 'PUT' / 'GET' / 'PUTREADY' / 'GETREADY'
                cur2ndDefineCmd.Cmd01_DBRobotAction = curBcsJob.RobotWIP.RobotRouteStepList[checkNextStepNo].Data.ROBOTACTION;
                //DB Spec define : 'UP':Upper Arm 'LOW':Lower Arm 'ANY':Any Arm 'ALL':All Arm
                cur2ndDefineCmd.Cmd01_DBUseArm = curBcsJob.RobotWIP.RobotRouteStepList[checkNextStepNo].Data.ROBOTUSEARM;
                cur2ndDefineCmd.Cmd01_DBStageIDList = curBcsJob.RobotWIP.RobotRouteStepList[checkNextStepNo].Data.STAGEIDLIST;
                robotConText.AddParameter(eRobotContextParameter.Define_1stNormalRobotCommandInfo, cur1stDefineCmd);
                robotConText.AddParameter(eRobotContextParameter.Define_2ndNormalRobotCommandInfo, cur2ndDefineCmd);
                robotConText.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, true);

                #region  [ Filter前先預設 Next Step都是符合條件的 ] 2015105 mark 改為在StageSelect_2nd 賦予初始值

                //string[] curStepCanUseStageList = curBcsJob.RobotWIP.RobotRouteStepList[checkNextStepNo].Data.STAGEIDLIST.Split(',');

                //for (int i = 0; i < curStepCanUseStageList.Length; i++)
                //{

                //    #region [ Check Stage is Exist ]

                //    RobotStage curStage;

                //    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                //    //找不到 Robot Stage 回NG
                //    if (curStage == null)
                //    {

                //        #region[DebugLog]

                //        if (IsShowDetialLog == true)
                //        {
                //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                //                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //        }

                //        #endregion

                //        return false;
                //    }

                //    if (curLDRQStageList.Contains(curStage) == false)
                //    {

                //        curLDRQStageList.Add(curStage);

                //    }

                //    #endregion

                //}

                ////Filter前先定義
                //robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curLDRQStageList);

                #endregion

                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curFilterStageList);

                #endregion =======================================================================================================================================================

                //[ Get Pre-Fetch condition and check if enable or disable for bypass the following logic ]
                bool _canUsePreFetchFlag = (curRobot.Context[eRobotContextParameter.CanUsePreFetchFlag].ToString() == "Y" ? true : false);

                if (_canUsePreFetchFlag) //如果有Pre-Fetch, 直接bypass filter檢查!!
                {
                    switch (cur2ndDefineCmd.Cmd01_DBRobotAction)
                    {
                        case eRobot_DB_CommandAction.ACTION_PUT:
                            cur2ndDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_PUT;
                            break;
                        case eRobot_DB_CommandAction.ACTION_MULTI_PUT:
                            cur2ndDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTI_PUT;
                            break;
                        case eRobot_DB_CommandAction.ACTION_GETPUT:
                            cur2ndDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_GETPUT;
                            break;
                        case eRobot_DB_CommandAction.ACTION_EXCHANGE:
                            cur2ndDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_EXCHANGE;
                            break;
                        default:
                            cur2ndDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE; //都沒有符合, 所以直接給 ACTION_NONE (0)
                            break;
                    }
                    cur2ndDefineCmd.Cmd01_ArmSelect = cur1stDefineCmd.Cmd01_ArmSelect;
                }
                else
                {
                    foreach (RobotRuleFilter curFilterCondition in curFilterList)
                    {
                        //Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_E0004 ] ,以Rule Job Filter 的ObjectName與MethodName為Key來決定是否紀錄Log
                        //因為會出現同Job 確認不同Step所以FailCode要補上StepNo
                        fail_ReasonCode = string.Format("{0}_{1}_{2}", curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, checkNextStepNo.ToString());

                        #region[DebugLog][ Start Rule Job Select Function ]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Filter object({4}) MethodName({5}) IsEnable({6}) Start {7}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, curFilterCondition.Data.ISENABLED,
                                                    new string(eRobotCommonConst.RULE_FILTER_START_CHAR, eRobotCommonConst.RULE_FILTER_START_CHAR_LENGTH));

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        if (curFilterCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                        {

                            checkFlag = (bool)Invoke(curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, new object[] { robotConText });

                            if (checkFlag == false)
                            {

                                #region[DebugLog][ End Rule Job Select Function ]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Filter Fail, object({4}) MethodName({5}) RtnCode({4})  RtnMsg({6}]!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Select object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME,
                                                            curFilterCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_FILTER_END_CHAR, eRobotCommonConst.RULE_FILTER_END_CHAR_LENGTH));

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0004 ]

                                if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Select Fail, object({4}) MethodName({5}) RtnCode({6})  RtnMsg({7}]!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                    //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6}]!",
                                    //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curFilterCondition.Data.OBJECTNAME,
                                    //                        curFilterCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    failMsg = string.Format("Job({0}_{1}) RtnCode({2})  RtnMsg({3}]!",
                                                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                    #endregion

                                }

                                #endregion

                                #region[DebugLog][ Start Job All Filter Function ]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job Filter ListCount({4}) End {5}",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curFilterList.Count.ToString(),
                                                            new string(eRobotCommonConst.ALL_RULE_SELECT_END_CHAR, eRobotCommonConst.ALL_RULE_FILTER_END_CHAR_LENGTH));

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                //有重大異常直接結束filter邏輯回復NG
                                return false;

                            }
                            else
                            {

                                //Clear[ Robot_Fail_Case_E0004 ]
                                RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                                #region[DebugLog][ End Rule Job Select Function ]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Filter object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME,
                                                            curFilterCondition.Data.ISENABLED,
                                                            new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                            }
                        }
                        else
                        {

                            #region[DebugLog][ End Rule Job Select Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Filter object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, curFilterCondition.Data.ISENABLED,
                                                        new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                        }

                    }
                }

                #endregion

                #region[DebugLog][ Start Job All Filter Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job Filter ListCount({4}) End {5}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curFilterList.Count.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_SELECT_END_CHAR, eRobotCommonConst.ALL_RULE_FILTER_END_CHAR_LENGTH));

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                //取得Filter後的LDRQ Staus List
                curFilterStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }


        }
        #endregion

        #region Array shop / DRY line / Special filter condition
        /// <summary>
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0019")]
        public bool Filter_ReceiveTypeCheck_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get DRY Route Info ] 後來改成 Route + Step 區分, 所以這邊就不需要再判斷了!!
                //List<string> _route = (List<string>)Invoke("RobotSpecialService", "Get_DRY_RouteInfo_For1Cmd_1Arm_1Job", new object[] { curRobot, curBcsJob });

                //if(_route == null)
                //{
                //    #region[DebugLog]
                //    if (IsShowDetialLog == true)
                //    {
                //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get DRY Route Info!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //    }
                //    #endregion

                //    errMsg = string.Format("[{0}] Robot({1}) can not Get DRY Route Info!", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                //    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DRY_RouteInfo_Is_Null);
                //    robotConText.SetReturnMessage(errMsg);

                //    return false;
                //}
                #endregion

                #region [ Get DRY Process Type Block ] 20160118-001-dd
                List<string> _lstProcessType = (List<string>)Invoke("RobotSpecialService", "Get_DRY_RrocessTypeBlock_For1Cmd_1Arm_1Job", new object[] { robotConText });

                #region [ Process Type Block 是 NULL ]
                if (_lstProcessType == null)
                {
                    errMsg = string.Format("Robot({0}) curRouteID({1})[{2}] can not Get DRY Process Type Block Info!",curRobot.Data.ROBOTNAME,curBcsJob.RobotWIP.CurRouteID, MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DRY_PROCESSTYPEBLOCK_NOT_FOUND);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_DRY_PROCESSTYPEBLOCK_NOT_FOUND;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                #endregion
                #region [ Process Type Block 没有资料 ]
                if (_lstProcessType.Count <= 0)
                {
                    errMsg = string.Format("Robot({0}) curRouteID({1})[{2}] DRY Process Type Block is empty!", curRobot.Data.ROBOTNAME,curBcsJob.RobotWIP.CurRouteID,MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DRY_PROCESSTYPEBLOCK_IS_EMPTY);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_DRY_PROCESSTYPEBLOCK_IS_EMPTY;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                #endregion)
                #endregion

                Line _line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if(_line == null)
                {
                    errMsg = string.Format("Line not-found, LINEID=[{0}]", curRobot.Data.LINEID);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                RobotStage _stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curBcsJob.RobotWIP.CurLocation_StageID);
                if (_stage == null)
                {
                    errMsg = string.Format("Stage not-found, STAGEID=[{0}]", curBcsJob.RobotWIP.CurLocation_StageID);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DRY_STAGE_NOT_FOUND);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                if (_stage.Data.STAGETYPE != "PORT")
                {
                    errMsg = string.Format("Stage type isn't PORT, STAGEID=[{0}]", curBcsJob.RobotWIP.CurLocation_StageID);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DRY_STAGETYPE_NOT_PORT);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                List<string> _curStageReceiveType = new List<string>();
                _curStageReceiveType.Clear();
                List<string> _curStageUseLists = new List<string>();
                _curStageUseLists.Clear();


                #region [ Get DRY ReceiveType ]
                System.Collections.Hashtable _receiveType = (System.Collections.Hashtable)Invoke("RobotSpecialService", "Get_DRY_ReceiveType_For1Cmd_1Arm_1Job", new object[] { curRobot });

                if (_receiveType == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get DRY ReceiveType!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("Robot({0}) curRouteID({1})[{2}] can not Get DRY ReceiveType!",curRobot.Data.ROBOTNAME,curBcsJob.RobotWIP.CurRouteID, MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DRY_Get_ReceiveType_Is_Null);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_DRY_Get_ReceiveType_Is_Null;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }

                if (curRobot.File.DryProcessTypes == null) curRobot.File.DryProcessTypes = new List<string>(); //20160107-001-dd
                curRobot.File.DryProcessTypes.Clear(); //20160107-001-dd

                foreach (string _stageID in _receiveType.Keys)
                {
                    System.Collections.Hashtable _ht = (System.Collections.Hashtable)_receiveType[_stageID];

                    //if ((int)_ht[eRobotContextParameter.DRYIFReceiveType] > 0 && !curRobot_stageID.File.DryProcessTypes.Contains(_ht[eRobotContextParameter.DRYIFReceiveType])) curRobot.File.DryProcessTypes.Add(_ht[eRobotContextParameter.DRYIFReceiveType].ToString());

                    if (int.Parse(_ht[eRobotContextParameter.DRYIFReceiveType].ToString()) <= 0 && int.Parse(_ht[eRobotContextParameter.DRYKeptReceiveType].ToString()) <= 0) continue;
                    if ((bool)_ht[eRobotContextParameter.DRYIFReceiveAbleSignal])
                    {
                        switch (_ht[eRobotContextParameter.DRYStageStatus].ToString())
                        {
                            case eRobotStageStatus.RECEIVE_READY:
                                break;
                            default: continue;
                        }

                        if (!_curStageUseLists.Contains(_stageID)) _curStageUseLists.Add(_stageID);
                        if (_curStageReceiveType.Contains(_ht[eRobotContextParameter.DRYIFReceiveType].ToString())) continue;
                        if (int.Parse(_ht[eRobotContextParameter.DRYIFReceiveType].ToString()) > 0) _curStageReceiveType.Add(_ht[eRobotContextParameter.DRYIFReceiveType].ToString());
                    }
                    else
                    {
                        switch (_ht[eRobotContextParameter.DRYStageStatus].ToString())
                        {
                            case eRobotStageStatus.NO_REQUEST:
                                //如果要卡DRY有需求, 才能從PORT取片, 這個部分就需要啟用!
                                //對TCDRY_ONLY途程來說, 有點像是Pre-Fetch功能!
                                //對TCDDC_TCDRY途程來說, 也是像Pre-Fetch功能, 只是會先針對DDC機台!!
                                if (_line.File.LineOperMode.ToUpper() == "MIX" || _line.File.LineOperMode.ToUpper() == "MIXEDRUNMODE") //eINDEXER_OPERATION_MODE.MIX_MODE)
                                {
                                    RobotStage _source = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curBcsJob.RobotWIP.CurLocation_StageID);
                                    if (_source == null) continue; //找不到source stage, 不需考虑!
                                    if (_source.Data.PREFETCHFLAG.ToString().ToUpper() != "Y") continue; //没有预取, 不需考虑!
                                }
                                break;
                            case eRobotStageStatus.RECEIVE_READY:
                            case eRobotStageStatus.SEND_OUT_READY:
                            case eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY:
                                break;
                            default: continue;
                        }

                        if (!_curStageUseLists.Contains(_stageID)) _curStageUseLists.Add(_stageID);
                        if (_curStageReceiveType.Contains(_ht[eRobotContextParameter.DRYIFReceiveType].ToString())) continue;
                        if (int.Parse(_ht[eRobotContextParameter.DRYIFReceiveType].ToString()) > 0) _curStageReceiveType.Add(_ht[eRobotContextParameter.DRYIFReceiveType].ToString());
                        if (int.Parse(_ht[eRobotContextParameter.DRYKeptReceiveType].ToString()) > 0) _curStageReceiveType.Add(_ht[eRobotContextParameter.DRYKeptReceiveType].ToString()); //雖然目前receive type為0, 但是仍然要去check上次的receive type為何?
                    }
                }

                if (_curStageReceiveType.Count <= 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]  curRouteID({1}) Robot({2}) EQP(DRY) Stage hasn't any request for glass!", curRobot.Data.NODENO,curBcsJob.RobotWIP.CurRouteID,curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("Robot({0}) curRouteID({1})[{2}] EQP(DRY) Stage hasn't any request for glass! No Receive Type!!", curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DRY_ReceiveType_Is_Zero);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_DRY_ReceiveType_Is_Zero;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }

                //20160107-001-dd
                foreach (string _s in _curStageReceiveType)
                {
                    if (curRobot.File.DryProcessTypes.Contains(_s)) continue;
                    curRobot.File.DryProcessTypes.Add(_s);
                }

                #region [ 判断目前要片的type是不是跟indxer那边可以出片 如果不在indexer可以出片的type, 就忽略! ] 20160118-001-dd
                for (int i = _curStageReceiveType.Count - 1; i>=0; i--)
                {
                    if (_lstProcessType.Contains(_curStageReceiveType[i])) continue;

                    _curStageReceiveType.Remove(_curStageReceiveType[i]);
                }

                if (_curStageReceiveType.Count <= 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) curRouteID({2}) EQP(DRY) Stage's Receive Type is mismatch with Indexer's Process Type Block", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("Robot ({0}) curRouteID ({1})[{2}] EQP(DRY) Stage's Receive Type is mismatch with Indexer's Process Type Block", curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DRY_RECEIVETYPE_PROCESSTYPEBLOCK_MISMATCH);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_DRY_RECEIVETYPE_PROCESSTYPEBLOCK_MISMATCH;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                #endregion

                #region [ 混RUN模式下需要做的特别判断 ] 20160118-001-dd
                if (_lstProcessType.Count >= 2 && (_line.File.LineOperMode.ToUpper() == "MIX" || _line.File.LineOperMode.ToUpper() == "MIXEDRUNMODE")) //eINDEXER_OPERATION_MODE.MIX_MODE)
                {
                    List<RobotStage> _stageList = ObjectManager.RobotStageManager.GetRobotStages(curRobot.Data.ROBOTNAME);
                    if (_stageList.Count() > 0)
                    {
                        foreach (RobotStage _stage2 in _stageList)
                        {
                            if (_stage2 == null) continue; //不合法(例如00=home stage), 不考虑!
                            if (_stage2.Data.STAGETYPE == "PORT") continue; //Port的不考虑!
                            if (_stage2.Data.NODENO == eRobotContextParameter.DRYNodeNo) continue; //DRY的stage不考虑!

                            switch (_stage2.File.CurStageStatus)
                            {
                                case eRobotStageStatus.RECEIVE_READY:
                                    //发要片需求, 代表没基板, 不考虑!
                                    continue;
                                case eRobotStageStatus.SEND_OUT_READY:
                                case eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY:
                                    //发出片需求, 代表有基板要出来, 需要考虑! 该基板的下个路径!!
                                    if (_stage2.curUDRQ_SlotList.Count <= 0) continue; //找不到, 不考虑!

                                    foreach (int slotNo in _stage2.curUDRQ_SlotList.Keys)
                                    {
                                        if (_stage2.curUDRQ_SlotList[slotNo] == string.Empty) continue;

                                        string[] _jobInfo = _stage2.curUDRQ_SlotList[slotNo].Split('_');

                                        if (_jobInfo == null) continue;
                                        if (_jobInfo.Length < 1) continue;

                                        Job _job2 = ObjectManager.JobManager.GetJob(_jobInfo[0].ToString(), _jobInfo[1].ToString());

                                        if (_job2 == null) continue;

                                        RobotRouteStep _step2 = _job2.RobotWIP.RobotRouteStepList[_job2.RobotWIP.NextStepNo];

                                        if (_step2 == null) continue; //找不到step, 不考虑!
                                        if (_step2.Data.STAGEIDLIST == string.Empty) continue; //没有设定stage, 不考虑!

                                        List<string> _lst3 = new List<string>(_step2.Data.STAGEIDLIST.Split(','));

                                        if (_lst3.Count <= 0) continue;

                                        for (int iii = _curStageUseLists.Count - 1; iii >= 0; iii--)
                                        {
                                            if (_lst3.Contains(_curStageUseLists[iii])) _lst3.Remove(_curStageUseLists[iii]);
                                        }

                                        if (_lst3.Count <= 0) continue; //没有, 不需要再判断!

                                        bool _havePosReq = false;
                                        foreach (string _s3 in _lst3)
                                        {
                                            RobotStage _stage3 = ObjectManager.RobotStageManager.GetRobotStagebyStageID(_s3);

                                            switch (_stage3.File.CurStageStatus)
                                            {
                                                case eRobotStageStatus.RECEIVE_READY:
                                                case eRobotStageStatus.SEND_OUT_READY:
                                                case eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY:
                                                    _havePosReq = true;
                                                    break;
                                                default: break;
                                            }

                                        }

                                        if (!_havePosReq) return false; //没有任何pos需要, 离开!!
                                    }
                                    break;
                                default: continue; //其他状态, 不考虑! 例如 NO_REQUEST!
                            }
                        }
                    }

                    //先取得目前出来的所有基板lists (包含 Arm上 以及 目前机台内 的基板!!)
                    List<Job> _armCanControlJobLists = (List<Job>)robotConText[eRobotContextParameter.ArmCanControlJobList];
                    if (_armCanControlJobLists != null)
                    {
                        if (_armCanControlJobLists.Count() >= _curStageReceiveType.Count)
                        {
                            errMsg = string.Format("[{0}] the require is matched.", MethodBase.GetCurrentMethod().Name);

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_REQ_IS_Satisfied);
                            robotConText.SetReturnMessage(errMsg);

                            return false; //目前arm上基板数量与目前DRY本体要的基板数量符合! 不需要再取片!
                        }
                    }


                    List<Job> _stageCanControlJobLists = (List<Job>)robotConText[eRobotContextParameter.StageCanControlJobList];



                    //List<Job> _stageCanControlJobLists_OrderBy = _stageCanControlJobLists.Select(s=>s.RobotWIP)
                    //OrderByDescending(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();
                }
                #endregion

                //有超过2种(含)的process type基板要做...就是Mix-Run
                //if(curRobot.File.DryProcessTypes.Count() > 1)
                if (_curStageReceiveType.Count > 1)
                {
                    if (curRobot.File.DryLastProcessType != string.Empty) //代表上次有进片!
                    {
                        if (_curStageReceiveType.Contains(curRobot.File.DryLastProcessType)) _curStageReceiveType.Remove(curRobot.File.DryLastProcessType);
                    }
                }
                #endregion

                #region [ Check Job ProcessType if match or not ] 20160118-001-dd
                if (_lstProcessType.Count >= 2 && !_curStageReceiveType.Contains(curBcsJob.ArraySpecial.ProcessType.ToString()))
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) EQP(DRY) Stage ReceiveType Mismatch!  Stage Receive Type List:({2}),StageIDList:({7})(Last Process Type=[{4}], Filtered=[{3}]), Job[{5}]'s Process Type=[{6}].", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, 
                            string.Join(",", curRobot.File.DryProcessTypes.ToArray()).ToString(), string.Join(",", _curStageReceiveType.ToArray()).ToString(), curRobot.File.DryLastProcessType, string.Format("CST={0}, Slot={1}", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString()), curBcsJob.ArraySpecial.ProcessType.ToString(),
                            string.Join(",", _curStageUseLists).ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQP(DRY) Stage ReceiveType Mismatch! Stage(DRY) Receive Type List:({2}),StageIDList:({7})(Last Process Type=[{4}], Filtered=[{3}]), Job[{5}]'s Process Type=[{6}].", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME,
                        string.Join(",", curRobot.File.DryProcessTypes.ToArray()).ToString(), string.Join(",", _curStageReceiveType.ToArray()).ToString(), curRobot.File.DryLastProcessType, string.Format("CST={0}, Slot={1}", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString()), curBcsJob.ArraySpecial.ProcessType.ToString(),
                        string.Join(",", _curStageUseLists.ToArray()).ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DRY_PrcessType_Mismatch);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_DRY_PrcessType_Mismatch;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                #endregion

                //移到result那边判断!! 20160107-001-dd
                ////if (curRobot.File.DryProcessTypes.Count() > 1)
                ////if (_curStageReceiveType.Count > 1)
                ////{
                //curRobot.File.DryLastProcessType = curBcsJob.ArraySpecial.ProcessType.ToString();
                ////}

                if (curRobot.File.DryCycleCnt > 5) //20160107-001-dd
                {
                    //curRobot.File.DryLastProcessType = string.Empty; //reset
                    curRobot.File.DryCycleCnt = 0;
                }

                #region[DebugLog]
                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter ReceiveType Check OK!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion
                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }
 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0020")]
        public bool Filter_CurStepStageIDListLDRQ_ReceiveTypeCheck(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curFilterCanUseStageList = new List<RobotStage>();
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobInfo!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get JobInfo!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Current Step Entity ]
                int tmpStepNo = 0;

                if (is2ndCmdFlag == true)
                {
                    tmpStepNo = curBcsJob.RobotWIP.NextStepNo;
                }
                else
                {
                    tmpStepNo = curBcsJob.RobotWIP.CurStepNo;
                }

                RobotRouteStep curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[tmpStepNo];

                //找不到 CurStep Route 回NG
                if (curRouteStep == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                tmpStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Job RouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            tmpStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region [ Get LDRQ Stage List ]
                curFilterCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curFilterCanUseStageList == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find Stage Status is LDRQ!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepNo({4}) StageIDList({5}) can not Find Stage Status is LDRQ!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }
                #endregion

                RobotStage _curStage = null;

                Line _line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if (_line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                {
                    //if (curRobot.File.DryLastProcessType != string.Empty)
                    //{
                    //    for (int i = curFilterCanUseStageList.Count - 1; i >= 0; i--)
                    //    {
                    //        _curStage = curFilterCanUseStageList[i];

                    //        if (_curStage.File.DownStreamLoadLockReceiveType != 0)
                    //        {
                    //            if (_curStage.File.DownStreamLoadLockReceiveType == int.Parse(curRobot.File.DryLastProcessType)) curFilterCanUseStageList.Remove(_curStage);
                    //        }
                    //        else if (_curStage.File.DryKeptLoadLockReceiveType != 0)
                    //        {
                    //            if (_curStage.File.DryKeptLoadLockReceiveType == int.Parse(curRobot.File.DryLastProcessType)) curFilterCanUseStageList.Remove(_curStage);
                    //        }
                    //    }
                    //}
                }
                else
                {
                    List<RobotStage> _stageList_UDRQ = new List<RobotStage>();
                    _stageList_UDRQ.Clear();

                    #region [ Check Stage ]
                    //2016/7/6 jack  modify 放null exception
                    bool _bPutPriority_UDRQ_first=true;
                    try { _bPutPriority_UDRQ_first = (ConstantManager[eRobotContextParameter.DRYPutPriorityUDRQ][curRobot.Data.LINEID].Value.ToString().ToLower() == "true" ? true : false); }
                    catch { }
                    

                    if (curFilterCanUseStageList.Count > 1 && _bPutPriority_UDRQ_first) //如果有超過1台stage, 則再判斷!!
                    {
                        for (int i = curFilterCanUseStageList.Count - 1; i >= 0; i--)
                        {
                            _curStage = curFilterCanUseStageList[i];

                            switch (_curStage.File.CurStageStatus)
                            {
                                case eRobotStageStatus.SEND_OUT_READY:
                                case eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY:
                                    break;
                                default: continue;
                            }

                            _stageList_UDRQ.Add(_curStage);
                        }

                        if (_stageList_UDRQ.Count() == 1)
                        {
                            for (int i = curFilterCanUseStageList.Count - 1; i >= 0; i--)
                            {
                                _curStage = curFilterCanUseStageList[i];
                                if (_curStage.Data.STAGEID == _stageList_UDRQ[0].Data.STAGEID) continue;

                                curFilterCanUseStageList.Remove(_curStage);
                            }
                        }
                    }
                    if (curFilterCanUseStageList.Count > 1 && curRobot.File.DRYLastEnterStageID > 0)
                    {
                        for (int i = curFilterCanUseStageList.Count - 1; i >= 0; i--)
                        {
                            _curStage = curFilterCanUseStageList[i];

                            if (_curStage.Data.STAGEID != curRobot.File.DRYLastEnterStageID.ToString()) continue;

                            curFilterCanUseStageList.Remove(_curStage);
                        }
                    }
                    #endregion

                    #region [ Get Current Step LDRQ Status List ]
                    for (int i = curFilterCanUseStageList.Count - 1; i >= 0; i--)
                    {
                        _curStage = curFilterCanUseStageList[i];

                        switch (_curStage.File.CurStageStatus)
                        {
                            case eRobotStageStatus.SEND_OUT_READY:
                            case eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY:
                                //如果都沒有值, 但是真的要出片, 那還是要可以取片!
                                if (_curStage.File.DownStreamLoadLockReceiveType <= 0 && _curStage.File.DryKeptLoadLockReceiveType <= 0)
                                {
                                    //如果要做GET/PUT的話, 那還是要考慮, 所以單純取片就可以pass!!
                                    List<Job> robotArmCanControlJobList = robotArmCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.ArmCanControlJobList];
                                    if (robotArmCanControlJobList.Count <= 0) continue; //Arm上沒有基板, 所以單純取片!!
                                }
                                //如果是UDRQ, 只需要考慮ReceiveType, 因為Receive Able信號不會ON!! (要出片!!)
                                if (_curStage.File.DownStreamLoadLockReceiveType > 0 && _curStage.File.DownStreamLoadLockReceiveType == int.Parse(curBcsJob.ArraySpecial.ProcessType)) continue;
                                if (_curStage.File.DryKeptLoadLockReceiveType > 0 && _curStage.File.DryKeptLoadLockReceiveType == int.Parse(curBcsJob.ArraySpecial.ProcessType)) continue;
                                break;
                            case eRobotStageStatus.RECEIVE_READY:
                                if (_curStage.File.DownStreamReceiveAbleSignal)
                                {
                                    if (_curStage.File.DownStreamLoadLockReceiveType > 0 && _curStage.File.DownStreamLoadLockReceiveType == int.Parse(curBcsJob.ArraySpecial.ProcessType)) continue;
                                }
                                break;
                        }
                        curFilterCanUseStageList.Remove(_curStage);
                    }
                    #endregion
                }

                //會直接更新所以不需要回傳值
                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }
        #endregion

        #region Array shop / ELA line / Special filter condition
        /// <summary>
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0021")]
        public bool Filter_JobProcessTypeEQRunModeCheck_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                }
                else
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }

                }

                #endregion

                #region [ Get Comon Can Use Stage List ]

                List<RobotStage> curStageSelectCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curStageSelectCanUseStageList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find curStageSelectCanUseStageList!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepNo({4}) StageIDList({5}) can not Find curStageSelectCanUseStageList!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobRouteStepByPass_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;

                }

                #endregion

                #region [ Check EQ Run Mode vs PorcessType]
                string processtype = curBcsJob.ArraySpecial.ProcessType;//0:prod, 1:MQC
                List<RobotStage> curCheckStepStageList = new List<RobotStage>();
                string[] curCheckStepStageIDList = curCheckRouteStep.Data.STAGEIDLIST.Split(',');

                for (int i = 0; i < curCheckStepStageIDList.Length; i++)
                {
                    RobotStage curStage;

                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curCheckStepStageIDList[i]);

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

                    if (curStageSelectCanUseStageList.Contains(curStage) == true)
                    {
                        Equipment eq = ObjectManager.EquipmentManager.GetEQP(curStage.Data.NODENO);//1:normal, 2:MQC
                        if (processtype.Equals("0") && !eq.File.EquipmentRunMode.ToUpper().Equals("NORMAL"))
                            continue;

                        if (processtype.Equals("1") && !eq.File.EquipmentRunMode.Equals("MQC"))
                            continue;

                        if (curCheckStepStageList.Contains(curStage) == false)
                        {
                            curCheckStepStageList.Add(curStage);
                        }
                    }
                }

                //找不到任一個符合的Stage則回覆異常
                if (curCheckStepStageList == null || curCheckStepStageList.Count == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5}) by Job's ProcessType({6})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, processtype);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Check Step({3}) Stage List({4}) by Job's ProcessType({5})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, processtype);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_JobProcessTypeEQRunModeCheck);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_JobProcessTypeEQRunModeCheck;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                #endregion

                curStageSelectCanUseStageList = curCheckStepStageList;
                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curCheckStepStageList);
                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0022")]
        public bool Filter_ELACleanStageReceiveDelayTimeCheck_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    errMsg = string.Format("[{0}]can not Get JobInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                }
                else
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }

                }

                #endregion

                #region [ Get Stage is PUT/Exchange]
                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT && curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_EXCHANGE)
                {
                    return true;
                }
                #endregion

                #region [ Get Comon Can Use Stage List ]

                List<RobotStage> curStageSelectCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curStageSelectCanUseStageList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find curStageSelectCanUseStageList!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepNo({4}) StageIDList({5}) can not Find curStageSelectCanUseStageList!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion                                

                #region [ Get ProcessTime & Check Final Receive Time vs Delay Time ]
                double delaytime = 0.0;//sec
                double compensationtime_Default = 0.0;
                double compensationtime_Recipe = 0.0;
                double compensationtime = 0.0; //from Constant varible
                Equipment ela, cln;
                int ela_ProcessTime_Default = 0;
                int ela_ProcessTime_Recipe = 0;
                int ela_ProcessTime = 0;
                int ELA_SumProcessTime = 0;
                int ELA_SumNormalMode = 0;
                int ELA_SumStatusDown = 0;
                string ela1_PPID;
                string ela2_PPID;
                string cln_PPID;
                string curByPassPPID;
                string eq_FinalReceiveGlassTime = "";
                string eq_NextReceiveGlassTime = "";
                string eq_Par = "";
                bool IsRecvDelayTimeNGFlag;

                //check ela1, ela2 recipe, if only cln process, not need check delay time for fetch glass from cst.
                ela = ObjectManager.EquipmentManager.GetEQP("L4");
                ela1_PPID = curBcsJob.PPID.Substring(ela.Data.RECIPEIDX, ela.Data.RECIPELEN);
                ela = ObjectManager.EquipmentManager.GetEQP("L5");
                ela2_PPID = curBcsJob.PPID.Substring(ela.Data.RECIPEIDX, ela.Data.RECIPELEN);
                curByPassPPID = new string('0', ela.Data.RECIPELEN);
                if (ela1_PPID == curByPassPPID && ela2_PPID == curByPassPPID)
                {
                     return true;
                }

                //MQC glass, not need check
                if (curBcsJob.ArraySpecial.ProcessType.Trim().Equals("1"))
                    return true;

                //if first glass delay time check NG, mode NG, status NG, after galss not need check and reply NG
                IsRecvDelayTimeNGFlag = (bool)curRobot.Context[eRobotContextParameter.IsRecvDelayTimeNGFlag];
                if (IsRecvDelayTimeNGFlag)
                    return false;

                #region [ get Max from QTime Def for L4 ProcessTime ]
                //get Max from QTime Def for L4 ProcessTime; 
                foreach (QtimeEntityData L4Processtime in ObjectManager.QtimeManager._entitiesDB.Values)
                {
                    if (L4Processtime.ENABLED.Equals("Y") && L4Processtime.STARTNODENO.Equals("L4") && L4Processtime.STARTEVENTMSG.Trim().Equals("RECEIVE")
                        && L4Processtime.ENDNODENO.Equals("L4") && L4Processtime.ENDEVENTMSG.Trim().Equals("RECEIVE"))
                    {
                        if (L4Processtime.STARTNODERECIPEID.Trim().Length == 0)
                        {
                            if (L4Processtime.SETTIMEVALUE > ela_ProcessTime_Default)
                            {
                                ela_ProcessTime_Default = L4Processtime.SETTIMEVALUE;
                            }
                        }
                        else
                        {
                            if (L4Processtime.STARTNODERECIPEID.Trim() == ela1_PPID.Trim())
                            {
                                if (L4Processtime.SETTIMEVALUE > ela_ProcessTime_Recipe)
                                {
                                    ela_ProcessTime_Recipe = L4Processtime.SETTIMEVALUE;
                                }
                            }
                        }
                    }
                }

                if (ela_ProcessTime_Recipe > 0)
                    ela_ProcessTime = ela_ProcessTime_Recipe;
                else if (ela_ProcessTime_Default > 0)
                    ela_ProcessTime = ela_ProcessTime_Default;
                else
                    ela_ProcessTime = 0;

                //if not define in QTime, get EQ report process time
                if (ela_ProcessTime == 0)
                {
                    string trxName = string.Format("L4_ProcessTimeBlock");
                    Trx trxProcessTime = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                    if (trxProcessTime == null)
                        return false;
                    ela_ProcessTime = int.Parse(trxProcessTime.EventGroups[0].Events[0].Items[0].Value);
                }

                ela = ObjectManager.EquipmentManager.GetEQP("L4");
                if (ela == null)
                    return false;
                if (ela.File.EquipmentRunMode.ToUpper().Equals("NORMAL"))
                {
                    ELA_SumNormalMode++;
                    if ((ela.File.Status == eEQPStatus.IDLE || ela.File.Status == eEQPStatus.RUN) && ela.File.CIMMode == eBitResult.ON && ela1_PPID != curByPassPPID)
                        ELA_SumProcessTime = ELA_SumProcessTime + ela_ProcessTime;
                    else
                        ELA_SumStatusDown++;
                }
                #endregion

                eq_Par = eq_Par + "ELA1's ProcessTime(" + ela_ProcessTime + "), RunMode(" + ela.File.EquipmentRunMode.ToUpper() + "), Status(" + ela.File.Status + "). ";

                ela_ProcessTime_Recipe = 0;
                ela_ProcessTime_Default = 0;

                #region [ get Max from QTime Def for L5 ProcessTime ]
                //get Max from QTime Def for L5 ProcessTime; 
                foreach (QtimeEntityData L5Processtime in ObjectManager.QtimeManager._entitiesDB.Values)
                {
                    if (L5Processtime.ENABLED.Equals("Y") && L5Processtime.STARTNODENO.Equals("L5") && L5Processtime.STARTEVENTMSG.Trim().Equals("RECEIVE")
                        && L5Processtime.ENDNODENO.Equals("L5") && L5Processtime.ENDEVENTMSG.Trim().Equals("RECEIVE"))
                    {
                        if (L5Processtime.STARTNODERECIPEID.Trim().Length == 0)
                        {
                            if (L5Processtime.SETTIMEVALUE > ela_ProcessTime_Default)
                            {
                                ela_ProcessTime_Default = L5Processtime.SETTIMEVALUE;
                            }
                        }
                        else
                        {
                            if (L5Processtime.STARTNODERECIPEID.Trim() == ela2_PPID.Trim())
                            {
                                if (L5Processtime.SETTIMEVALUE > ela_ProcessTime_Recipe)
                                {
                                    ela_ProcessTime_Recipe = L5Processtime.SETTIMEVALUE;
                                }
                            }
                        }
                    }
                }

                if (ela_ProcessTime_Recipe > 0)
                    ela_ProcessTime = ela_ProcessTime_Recipe;
                else if (ela_ProcessTime_Default > 0)
                    ela_ProcessTime = ela_ProcessTime_Default;
                else
                    ela_ProcessTime = 0;

                //if not define in QTime, get EQ report process time
                if (ela_ProcessTime == 0)
                {
                    string trxName = string.Format("L5_ProcessTimeBlock");
                    Trx trxProcessTime = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                    if (trxProcessTime == null)
                        return false;
                    ela_ProcessTime = int.Parse(trxProcessTime.EventGroups[0].Events[0].Items[0].Value);
                }
                ela = ObjectManager.EquipmentManager.GetEQP("L5");
                if (ela == null)
                    return false;
                if (ela.File.EquipmentRunMode.ToUpper().Equals("NORMAL"))
                {
                    ELA_SumNormalMode++;
                    if ((ela.File.Status == eEQPStatus.IDLE || ela.File.Status == eEQPStatus.RUN) && ela.File.CIMMode == eBitResult.ON && ela2_PPID != curByPassPPID)
                        ELA_SumProcessTime = ELA_SumProcessTime + ela_ProcessTime;
                    else
                        ELA_SumStatusDown++;
                }
                #endregion

                eq_Par = eq_Par + "ELA2's ProcessTime(" + ela_ProcessTime + "), RunMode(" + ela.File.EquipmentRunMode.ToUpper() + "), Status(" + ela.File.Status + "). ";

                if (ELA_SumNormalMode == 0) //No normal mode, don't fetch glass
                {
                    curRobot.Context.AddParameter(eRobotContextParameter.IsRecvDelayTimeNGFlag, true);
                    errMsg = string.Format("[{0}] Job({1}_({2}) can not Get Current Check Step({3}) Stage List({4}) Has Not Any ELA EQ Run Mode = NORMAL !",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck_ModeNotMatch);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck_ModeNotMatch;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                if (ELA_SumNormalMode == ELA_SumStatusDown) //All normal are down, don't put to clean
                {
                    curRobot.Context.AddParameter(eRobotContextParameter.IsRecvDelayTimeNGFlag, true);
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Check Step({3}) Stage List({4}) ELA EQ Run Mode = NORMAL ARE (DOWN or CIMOFF)*({5})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ELA_SumStatusDown);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck_StatusAllDown);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck_StatusAllDown;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }

                //compensationtime = (double)ParameterManager["ELA_COMPENSATION_TIME"].GetInteger(); 
                /*
                cln = ObjectManager.EquipmentManager.GetEQP("L3");
                cln_PPID = curBcsJob.PPID.Substring(cln.Data.RECIPEIDX, cln.Data.RECIPELEN);
                compensationtime = 0;
                foreach (QtimeEntityData L3Processtime in ObjectManager.QtimeManager._entitiesDB.Values)
                {
                    if (L3Processtime.ENABLED.Equals("Y") && L3Processtime.STARTNODENO.Equals("L3") && L3Processtime.STARTEVENTMSG.Trim().Equals("RECEIVE")
                        && L3Processtime.ENDNODENO.Equals("L3") && L3Processtime.ENDEVENTMSG.Trim().Equals("RECEIVE"))
                    {
                        if (L3Processtime.STARTNODERECIPEID.Trim().Length == 0)
                        {
                            if (L3Processtime.SETTIMEVALUE > (int)compensationtime_Default)
                                compensationtime_Default = (double)L3Processtime.SETTIMEVALUE;
                        }
                        else
                        {
                            if (L3Processtime.STARTNODERECIPEID.Trim() == cln_PPID.Trim())
                                if (L3Processtime.SETTIMEVALUE > (int)compensationtime_Recipe)
                                    compensationtime_Recipe = (double)L3Processtime.SETTIMEVALUE;
                        }
                    }
                }
                if (compensationtime_Recipe > 0)
                    compensationtime = compensationtime_Recipe;
                else if (compensationtime_Default > 0)
                    compensationtime = compensationtime_Default;
                else
                    compensationtime = 0;
                */
                Line line = ObjectManager.LineManager.GetLines()[0];
                if (line == null)
                {
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Check Step({3}) Stage List({54}) for Get Line Object is Null",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }
                compensationtime = (double)line.File.FetchCompensationTime;
                if (compensationtime > 0)
                    compensationtime /= (double)(ELA_SumNormalMode - ELA_SumStatusDown) / 2;
                else
                    compensationtime *= (double)(ELA_SumNormalMode - ELA_SumStatusDown) / 2;  //modify by yang 20170214 for only one ELA ,compensationtime调整

                eq_Par = eq_Par + "CompensationTime(" + compensationtime + ")";

                delaytime = (double)(ELA_SumProcessTime / ((ELA_SumNormalMode - ELA_SumStatusDown) * (ELA_SumNormalMode - ELA_SumStatusDown)));
                delaytime = delaytime + compensationtime;
                
                List<RobotStage> curCheckStepStageList = new List<RobotStage>();
                string[] curCheckStepStageIDList = curCheckRouteStep.Data.STAGEIDLIST.Split(',');

                for (int i = 0; i < curCheckStepStageIDList.Length; i++)
                {
                    RobotStage curStage;

                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curCheckStepStageIDList[i]);

                    if (curStage == null)
                    {
                        curRobot.Context.AddParameter(eRobotContextParameter.IsRecvDelayTimeNGFlag, true);
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

                    if (curStageSelectCanUseStageList.Contains(curStage) == true)
                    {
                        Equipment eq = ObjectManager.EquipmentManager.GetEQP(curStage.Data.NODENO);
                        double time;
                        if (eq.File.FinalReceiveGlassTime.Trim().Length > 0)
                        {
                            DateTime dtfinal;
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo();
                            dtFormat.ShortDatePattern = "yyyy/MM/dd HH:mm:ss";
                            eq_FinalReceiveGlassTime = eq_FinalReceiveGlassTime + eq.File.FinalReceiveGlassTime + " ";
                            eq_NextReceiveGlassTime = eq_NextReceiveGlassTime + Convert.ToDateTime(eq.File.FinalReceiveGlassTime).AddSeconds(delaytime);
                            dtfinal = Convert.ToDateTime(eq.File.FinalReceiveGlassTime, dtFormat);
                            time = (DateTime.Now - dtfinal).TotalSeconds;
                            if (time < delaytime)
                                continue;
                        }

                        if (curCheckStepStageList.Contains(curStage) == false)
                        {
                            curCheckStepStageList.Add(curStage);
                        }
                    }
                }

                //找不到任一個符合的Stage則回覆異常
                if (curCheckStepStageList == null || curCheckStepStageList.Count == 0)
                {
                    curRobot.Context.AddParameter(eRobotContextParameter.IsRecvDelayTimeNGFlag, true);
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5}) FinalReceiveGlassTime({6}) Next ReceiveTime({7}). DelayTime({8} = {9})",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, eq_FinalReceiveGlassTime, eq_NextReceiveGlassTime, delaytime, eq_Par);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Check Step({3}) Stage List({4}) FinalReceiveGlassTime({5}) Next ReceiveTime({6}). DelayTime({7} = {8})",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, eq_FinalReceiveGlassTime, eq_NextReceiveGlassTime, delaytime, eq_Par);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }
                #endregion

                curStageSelectCanUseStageList = curCheckStepStageList;
                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curCheckStepStageList);
                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0023")]
        public bool Filter_ELALowStageReceiveCheck_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                }
                else
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }

                }

                #endregion

                #region [ Get Stage is Get]
                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_GET)
                {
                    return true;
                }
                #endregion

                #region [ Get Comon Can Use Stage List ]

                List<RobotStage> curStageSelectCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curStageSelectCanUseStageList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find curStageSelectCanUseStageList!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepNo({4}) StageIDList({5}) can not Find curStageSelectCanUseStageList!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobRouteStepByPass_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region [ Get real RTC flag on By Indexer & Check cst sequence ]
                Line line;
                bool line_Odd;
                string sLineBackUpMode;
                string realRTC;
                string trxName = "L2_LineBackupMode";

                line = ObjectManager.LineManager.GetLines()[0];
                if (line == null)
                {
                    errMsg = string.Format("[{0}] can not Find Line Object !",MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                int lineno = (int)line.Data.LINEID[5];
                if (lineno % 2 == 1)
                    line_Odd = true;
                else
                    line_Odd = false;

                Trx trxLineBackupMode = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                if (trxLineBackupMode == null)
                {
                    errMsg = string.Format("[{0}] can not Find Trasaction({1}) !", MethodBase.GetCurrentMethod().Name, trxName);
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobRouteStepJump_ReturnCode.NG_Exception;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }

                /* modify for can't get job data from indexer 2013/04/14 cc.kuang
                IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG, "EQPFlag");
                if (subItem != null)
                {
                    if (!subItem.ContainsKey("BackupProcessFlag"))
                    {
                        errMsg = string.Format("[{0}] Robot({1}) can not Find BackupProcessFlag Item Define in EQPFlag !", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name);
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                }
                else
                {
                    errMsg = string.Format("[{0}] Robot({1}) can not Find EQPFlag Define !", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                */
                sLineBackUpMode = trxLineBackupMode.EventGroups[0].Events[0].Items[0].Value;
                realRTC = curBcsJob.ArraySpecial.RtcFlag;
                /* modify for can't get job data from indexer 2013/04/14 cc.kuang
                if (!sLineBackUpMode.Equals("1")) //Not BackUp Mode                  
                {
                    if (realRTC.Equals("1")) //Not BackUp Mode, realRTC flag can't receive
                    {
                        errMsg = string.Format("[{0}] Robot({1}) can not Receive RTC Job({2},{3}) in Non-BackUpMode !", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                    else
                    {
                        if (line_Odd)
                        {
                            if (int.Parse(curBcsJob.CassetteSequenceNo) > 4000) //Not BackUp Mode, Cross JOb's BackupProcessFlag Off can't receive
                            {
                                if (subItem["BackupProcessFlag"] != "1")
                                {
                                    errMsg = string.Format("[{0}] Robot({1}) BackupProcessFlag is OFF in Job({2},{3}) !", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                                    robotConText.SetReturnMessage(errMsg);
                                    return false;
                                }
                            }
                            else
                            {
                                if (curBcsJob.ArraySpecial.ProcessType.ToUpper() == "NORMAL")
                                {
                                    errMsg = string.Format("[{0}] Robot({1}) Job({2},{3}) Process Type(Normal) is Illegal Route Step !", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                                    robotConText.SetReturnMessage(errMsg);
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (int.Parse(curBcsJob.CassetteSequenceNo) < 4000) 
                            {
                                if (subItem["BackupProcessFlag"] != "1")
                                {
                                    errMsg = string.Format("[{0}] Robot({1}) BackupProcessFlag is OFF in Job({2},{3}) !", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                                    robotConText.SetReturnMessage(errMsg);
                                    return false;
                                }
                            }
                            else
                            {
                                if (curBcsJob.ArraySpecial.ProcessType.ToUpper() == "NORMAL")
                                {
                                    errMsg = string.Format("[{0}] Robot({1}) Job({2},{3}) Process Type(Normal) is Illegal Route Step !", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                                    robotConText.SetReturnMessage(errMsg);
                                    return false;
                                }
                            }
                        }
                    }
                }
                else //BackUp Mode                  
                {
                    if (!realRTC.Equals("1")) //BackUp Mode, realRTC flag only receive
                    {
                        errMsg = string.Format("[{0}] Robot({1}) can not Receive Non-RTC Job({2},{3}) in BackUpMode !", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                }
                */
                #endregion

                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0024")]
        public bool Filter_ELAUpStageReceiveCheck_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                }
                else
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }

                }

                #endregion

                #region [ Get Stage is Get]
                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_GET)
                {
                    return true;
                }
                #endregion

                #region [ Get Comon Can Use Stage List ]

                List<RobotStage> curStageSelectCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curStageSelectCanUseStageList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find curStageSelectCanUseStageList!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepNo({4}) StageIDList({5}) can not Find curStageSelectCanUseStageList!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobRouteStepByPass_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;

                }

                #endregion

                #region [ Check cst sequence ]
                Line line;
                bool line_Odd;

                line = ObjectManager.LineManager.GetLines()[0];
                if (line == null)
                {
                    errMsg = string.Format("[{0}] can not Find Line Object !", MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                int lineno = (int)line.Data.LINEID[5];
                if (lineno % 2 == 1)
                    line_Odd = true;
                else
                    line_Odd = false;

                /*
                IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG, "EQPFlag");
                if (subItem != null)
                {
                    if (!subItem.ContainsKey("BackupProcessFlag"))
                    {
                        errMsg = string.Format("[{0}] Robot({1}) can not Find BackupProcessFlag Item Define in EQPFlag !", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name);
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                }
                else
                {
                    errMsg = string.Format("[{0}] Robot({1}) can not Find EQPFlag Define !", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                */
                if (line_Odd)
                {
                    if (int.Parse(curBcsJob.CassetteSequenceNo) > 4000) 
                    {
                        errMsg = string.Format("[{0}] Job({1}_{2}) Not Belong This Line !", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                        robotConText.SetReturnMessage(errMsg);
                        errCode = eJobRouteStepJump_ReturnCode.NG_Exception;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                        return false;
                    }
                }
                else
                {
                    if (int.Parse(curBcsJob.CassetteSequenceNo) < 4000)
                    {
                        errMsg = string.Format("[{0}] Job({1}_{2}) Not Belong This Line !", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                        robotConText.SetReturnMessage(errMsg);
                        errCode = eJobRouteStepJump_ReturnCode.NG_Exception;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                        return false;
                    }
                }
                #endregion

                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20160204 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0052")]
        public bool Filter_ELARecipeCheck_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                }
                else
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }

                }

                #endregion

                #region [ Get Stage Get]
                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_GET)
                {
                    return true;
                }
                #endregion

                #region [ Get Process Recipe & Check & Set if Diff ]

                Equipment ela1, ela2;
                string ela1_PPID;
                string ela2_PPID;
                string curByPassPPID;
                string ela1_PPID_Reply;
                string ela2_PPID_Reply;
                bool IsRecipeNGFlag;

                //check ela1, ela2 recipe, if only cln process, not need check delay time for fetch glass from cst.
                ela1 = ObjectManager.EquipmentManager.GetEQP("L4");
                ela1_PPID = curBcsJob.PPID.Substring(ela1.Data.RECIPEIDX, ela1.Data.RECIPELEN);
                ela2 = ObjectManager.EquipmentManager.GetEQP("L5");
                ela2_PPID = curBcsJob.PPID.Substring(ela2.Data.RECIPEIDX, ela2.Data.RECIPELEN);
                curByPassPPID = new string('0', ela2.Data.RECIPELEN);
                if (ela1_PPID == curByPassPPID && ela2_PPID == curByPassPPID)
                {
                    return true;
                }

                //MQC glass, not need check
                if (curBcsJob.ArraySpecial.ProcessType.Trim().Equals("1"))
                {
                    return true;
                }

                //if first glass delay time check NG, mode NG, status NG, after galss not need check and reply NG
                IsRecipeNGFlag = (bool)curRobot.Context[eRobotContextParameter.IsRecipeNGFlag];
                if (IsRecipeNGFlag)
                {
                    errMsg = string.Format("[{0}] Job({1}_{2}) CurStepNo({3}) Can't Fetch for Wait NORMAL Mode ELA's ProcessRecipe Match!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELAProcessRecipeNotMatch);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_ELAProcessRecipeNotMatch;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }

                string trxName = string.Format("L4_ProcessTimeBlock");
                Trx trxProcessRecipe = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                if (trxProcessRecipe == null)
                    return false;
                ela1_PPID_Reply = trxProcessRecipe.EventGroups[0].Events[0].Items[1].Value;

                trxName = string.Format("L5_ProcessTimeBlock");
                trxProcessRecipe = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                if (trxProcessRecipe == null)
                    return false;
                ela2_PPID_Reply = trxProcessRecipe.EventGroups[0].Events[0].Items[1].Value;

                //if Diff with job's recipe, set it

                IsRecipeNGFlag = false;

                if (!ela1_PPID.Equals(curByPassPPID))
                {
                    Trx Trx = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat("L4_ProcessTimeQueryBlock") as Trx;
                    if (Trx == null)
                    {
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "L4_ProcessTimeQueryBlock Trx Not Define !");
                    }
                    else
                    {
                        if (ela1.File.EquipmentRunMode.ToUpper().Equals("NORMAL") && !ela1_PPID.Equals(ela1_PPID_Reply))
                        {
                            IsRecipeNGFlag = true;
                            Trx.EventGroups[0].Events[0].Items[0].Value = ela1_PPID;
                            Trx.TrackKey = UtilityMethod.GetAgentTrackKey();
                            SendPLCData(Trx);
                        }
                    }
                }

                if (!ela2_PPID.Equals(curByPassPPID))
                {
                    Trx Trx = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat("L5_ProcessTimeQueryBlock") as Trx;
                    if (Trx == null)
                    {
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "L5_ProcessTimeQueryBlock Trx Not Define !");
                    }
                    else
                    {
                        if (ela2.File.EquipmentRunMode.ToUpper().Equals("NORMAL") && !ela2_PPID.Equals(ela2_PPID_Reply))
                        {
                            IsRecipeNGFlag = true;
                            Trx.EventGroups[0].Events[0].Items[0].Value = ela2_PPID;
                            Trx.TrackKey = UtilityMethod.GetAgentTrackKey();
                            SendPLCData(Trx);
                        }
                    }
                }

                if (IsRecipeNGFlag)
                {
                    curRobot.Context.AddParameter(eRobotContextParameter.IsRecipeNGFlag, true);
                    errMsg = string.Format("[{0}] Job({1}_{2}) CurStepNo({3}) Can't Fetch for NORMAL Mode ELA's ProcessRecipe Not Match",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELAProcessRecipeNotMatch);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_ELAProcessRecipeNotMatch;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
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

        #endregion


        /// <summary> 確認EQP Type Stage LinkSignal Tranfer Stop Request bit is Off
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0031")]
        public bool Filter_EQPTypeStageNoStop(IRobotContext robotConText)
        {

            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curFilterCanUseStageList = null;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get 2nd Command Check Flag來決定要判斷curStep還是NextStep ]

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity ]

                int checkStepNo = 0;

                if (is2ndCmdFlag == true)
                {
                    //20151014 Modity NextStep由WIP來取得
                    checkStepNo = curBcsJob.RobotWIP.NextStepNo;// curBcsJob.RobotWIP.CurStepNo + 1;
                }
                else
                {
                    checkStepNo = curBcsJob.RobotWIP.CurStepNo;
                }

                RobotRouteStep curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo];

                //找不到 CurStep Route 回NG
                if (curRouteStep == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                checkStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Job RouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            checkStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check current Check Step Action Must PUT ]

                if (curRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.ROBOTACTION);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) Check Step({3}) Action({4}) is not PUT!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.ROBOTACTION);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_CheckNoStopStageList_StepAction_Not_PUT);
                    robotConText.SetReturnMessage(errMsg);


                    return false;

                }

                #endregion

                #region [ Get LDRQ Stage List by cur Can use Stage List ]

                curFilterCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curFilterCanUseStageList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find can Check NoStop StageList!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(), curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepNo({4}) StageIDList({5}) can not Find can Check NoStop StageList!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(), curRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_CheckNoStopStageList_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Get_CheckNoStopStageList_Is_Null;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;

                }

                #endregion

                #region [ Get Current Check Step NoStop Stage List ]

                string[] stageList = curRouteStep.Data.STAGEIDLIST.Split(',');

                for (int i = 0; i < stageList.Length; i++)
                {

                    #region [ Check Stage is Exist ]

                    RobotStage curStepUseStage;

                    curStepUseStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[i]);

                    //找不到 Robot Stage 回NG
                    if (curStepUseStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, stageList[i]);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        continue;
                    }

                    #endregion

                    //判斷Current Step Stage 是否存在於Current LDRQ Stage List
                    if (curFilterCanUseStageList.Contains(curStepUseStage) == false)
                    {
                        //不存在則視同判斷失敗直接記Log跳下一個Stage
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) check StepNo({5}) StageID({6}) is not in filter can use Stagelist!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(), curStepUseStage.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        continue;
                    }

                    if (curStepUseStage.File.DownStreamTransferStopRequestFlag == false)
                    {
                        //No Stop 則加入Can use Stage List
                        if (!curFilterCanUseStageList.Contains(curStepUseStage)) curFilterCanUseStageList.Add(curStepUseStage);

                    }
                    else
                    {
                        //Stage is Transfter Stop
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) check StepNo({5}) StageID({6}) LinkSignal Transfer Stop Request(On)!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(), curStepUseStage.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不符合條件則要從CurUsertageList移除
                        if (curFilterCanUseStageList.Contains(curStepUseStage)) curFilterCanUseStageList.Remove(curStepUseStage);

                    }

                }

                #endregion

                if (curFilterCanUseStageList.Count == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStageID({5}) StepNo({6}) StageIDList({7}) can not Find NoStop StageList!(please check Downstream EQP TransferStopRequest is Off)",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(), curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find NoStop StageList!(please check Downstream EQP TransferStopRequest is Off)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(), curRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_CheckNoStopStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_Get_CheckNoStopStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                //會直接更新所以不需要回傳值
                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }


        /// <summary>
        /// 檢查是否有PortMode符合JobJudge, 收片邏輯
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0044")]
        public bool Filter_JobJudgeUnloadingPortMode(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Current Step Entity ]
                RobotRouteStep curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                //找不到 CurStep Route 回NG
                if (curRouteStep == null)
                {

                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get JobcurRouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region [ Get Current Stage List ]
                List<RobotStage> curStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curStageList == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepID({5}) StageIDList({6}) can not Find Stage Status is LDRQ!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4})  StageIDList({5}) can not Find Stage Status is LDRQ!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                Port match_port = null;
                for (int i = curStageList.Count - 1; i >= 0; i--)
                {
                    // Port Stage 且有空 Slot
                    RobotStage stage = curStageList[i];
                    bool match = false;
                    if (stage.Data.STAGETYPE == eRobotStageType.PORT && stage.curLDRQ_EmptySlotList.Count > 0)
                    {
                        Port port = ObjectManager.PortManager.GetPort(stage.Data.LINEID, stage.Data.NODENO, stage.Data.STAGEIDBYNODE);
                        if (port != null)
                        {
                            if (port.File.Type == ePortType.UnloadingPort)
                            {
                                //if ((curBcsJob.RobotWIP.CurSendOutJobJudge == a && (port.File.Mode == ePortMode.OK || port.File.Mode == ePortMode.EMPMode)) ||
                                //    (curBcsJob.RobotWIP.CurSendOutJobJudge != job_judge_ok && (port.File.Mode == ePortMode.NG || port.File.Mode == ePortMode.EMPMode)))
                                if (ForCF_CheckJobJudge_PortMode_UnloadingPortSetting(curBcsJob, port))
                                {
                                    match_port = port;
                                    match = true;
                                }
                            }
                            if (port.File.Type == ePortType.BothPort)
                            {
                                if (curBcsJob.FromCstID == port.File.CassetteID)
                                {
                                    match_port = port;
                                    match = true;
                                }
                            }
                            else
                            {
                                #region[DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepID({5}) Port({6}) PortType is not unloading",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), stage.Data.STAGEIDBYNODE);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepID({5}) can not get port({6}) in PortManager",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), stage.Data.STAGEIDBYNODE);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                    }
                    if (!match)
                    {
                        curStageList.RemoveAt(i);
                    }
                }

                if (match_port == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) CurSendOutJobJudge({6}:{7}) Cannot find Match Port Mode(Please check 1. IF EQP's ProductTypeCheckMode is Enble,Job's Judge Must Equals To  Unloadingport's ProductType 2. Job's Judge Must Equals To UnloadingPort's Port Mode Or  UnloadingPort's Port Mode = EMP/MIX)",
                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                            curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.CurSendOutJobJudge, getDetailJobJudge(curBcsJob.RobotWIP.CurSendOutJobJudge));

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) CurSendOutJobJudge({6}:{7}) Cannot find Match Port Mode(Please check 1. IF EQP's ProductTypeCheckMode is Enble,Job's Judge Must Equals To  Unloadingport's ProductType 2. Job's Judge Must Equals To UnloadingPort's Port Mode Or  UnloadingPort's Port Mode = EMP/MIX)",
                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                            curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.CurSendOutJobJudge, getDetailJobJudge(curBcsJob.RobotWIP.CurSendOutJobJudge));

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_No_Macth_Port_Mode);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_No_Macth_Port_Mode;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                else
                {
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 過濾掉JobJudge=OK,NG,回Both port的情況,直接到Unloading port(OK,NG)
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0045")]
        public bool Filter_JobJudgeOKNG_GotoUnloadingPort(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Current Step Entity ]
                RobotRouteStep curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                //找不到 CurStep Route 回NG
                if (curRouteStep == null)
                {

                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get JobcurRouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region [ Get Current Stage List ]
                List<RobotStage> curStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curStageList == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepID({5}) StageIDList({6}) can not Find Stage Status is LDRQ!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4})  StageIDList({5}) can not Find Stage Status is LDRQ!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion

                Port match_port = null;
                for (int i = curStageList.Count - 1; i >= 0; i--)
                {
                    // Port Stage 且有空 Slot
                    RobotStage stage = curStageList[i];
                    bool match = false;
                    if (stage.Data.STAGETYPE == eRobotStageType.PORT && stage.curLDRQ_EmptySlotList.Count > 0)
                    {
                        Port port = ObjectManager.PortManager.GetPort(stage.Data.LINEID, stage.Data.NODENO, stage.Data.STAGEIDBYNODE);
                        if (port != null)
                        {
                            if (curBcsJob.RobotWIP.CurSendOutJobJudge == "1")
                            {
                                if (port.File.Type == ePortType.UnloadingPort)
                                {
                                    match_port = port;
                                    match = true;
                                }
                            }
                            if (curBcsJob.RobotWIP.CurSendOutJobJudge != "1")
                            {
                                if (port.File.Type == ePortType.UnloadingPort)
                                {
                                    match_port = port;
                                    match = true;
                                }
                            }
                        }
                        else
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepID({5}) can not get port({6}) in PortManager",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), stage.Data.STAGEIDBYNODE);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                    }
                    if (!match)
                    {
                        curStageList.RemoveAt(i);
                    }
                }

                if (match_port == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStageID({5}) StepNo({6}) CurSendOutJobJudge({7}) Cannot find Unloading port(Please check have Unloading port)",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.CurSendOutJobJudge);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) CurSendOutJobJudge({6}) Cannot find Unloading port(Please check have Unloading port)",
                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                            curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.CurSendOutJobJudge);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_No_Unloading_Port);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_No_Unloading_Port;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                else
                {
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 過濾掉JobJudge=NG,回Both port的情況,NG到Unloading port(NG),OK照樣回Both port
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0046")]
        public bool Filter_JobJudgeNG_GotoUnloadingPort(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Current Step Entity ]
                RobotRouteStep curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                //找不到 CurStep Route 回NG
                if (curRouteStep == null)
                {

                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get JobcurRouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region [ Get Current Stage List ]
                List<RobotStage> curStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curStageList == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepID({5}) StageIDList({6}) can not Find Stage Status is LDRQ!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4})  StageIDList({5}) can not Find Stage Status is LDRQ!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion

                Port match_port = null;
                for (int i = curStageList.Count - 1; i >= 0; i--)
                {
                    // Port Stage 且有空 Slot
                    RobotStage stage = curStageList[i];
                    bool match = false;
                    if (stage.Data.STAGETYPE == eRobotStageType.PORT && stage.curLDRQ_EmptySlotList.Count > 0)
                    {
                        Port port = ObjectManager.PortManager.GetPort(stage.Data.LINEID, stage.Data.NODENO, stage.Data.STAGEIDBYNODE);
                        if (port != null)
                        {
                            if (curBcsJob.RobotWIP.CurSendOutJobJudge != "1")
                            {
                                if (port.File.Type == ePortType.UnloadingPort)
                                {
                                    match_port = port;
                                    match = true;
                                }
                            }
                            else
                            {
                                match_port = port;
                                match = true;
                            }
                        }
                        else
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepID({5}) can not get port({6}) in PortManager",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), stage.Data.STAGEIDBYNODE);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                    }
                    if (!match)
                    {
                        curStageList.RemoveAt(i);
                    }
                }

                if (match_port == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3})  curRouteID({4}) curStageID({5}) StepNo({6}) CurSendOutJobJudge({7}) Cannot find Match Port Mode(Can't find Unloading port(not OK port mode,is match CurSendOutJobJudge port mode))",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.CurSendOutJobJudge);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) CurSendOutJobJudge({6}:{7}) Cannot Find  UnadingPort(Job Judge NOT OK Go To Unloading Port)",
                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                            curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.CurSendOutJobJudge, getDetailJobJudge(curBcsJob.RobotWIP.CurSendOutJobJudge));

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_JudgeNG_No_Macth_UnloadingPort);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_JudgeNG_No_Macth_UnloadingPort;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }
                else
                {
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20161017 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0057")]
        public bool Filter_ELAIndexerStageReceiveDelayTimeCheck_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    errMsg = string.Format("[{0}]can not Get JobInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

                RobotRouteStep curCheckRouteStep = null;

                if (is2ndCmdFlag == false)
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                }
                else
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];

                    //找不到 CurStep Route 回NG
                    if (curCheckRouteStep == null)
                    {

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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }

                }

                #endregion

                #region [ Get Stage is PUT/Exchange]
                if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT && curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_EXCHANGE)
                {
                    return true;
                }
                #endregion

                #region [ Get Comon Can Use Stage List ]

                List<RobotStage> curStageSelectCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curStageSelectCanUseStageList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find curStageSelectCanUseStageList!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepNo({4}) StageIDList({5}) can not Find curStageSelectCanUseStageList!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobRouteStepByPass_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region [ Get ProcessTime & Check Final Receive Time vs Delay Time ]
                double delaytime = 0.0;//sec
                double compensationtime_Default = 0.0;
                double compensationtime_Recipe = 0.0;
                double compensationtime = 0.0; //from Constant varible
                Equipment ela, cln;
                int ela_ProcessTime_Default = 0;
                int ela_ProcessTime_Recipe = 0;
                int ela_ProcessTime = 0;
                int ELA_SumProcessTime = 0;
                int ELA_SumNormalMode = 0;
                int ELA_SumStatusDown = 0;
                string ela1_PPID;
                string ela2_PPID;
                string cln_PPID;
                string curByPassPPID;
                string eq_FinalReceiveGlassTime = "";
                string eq_NextReceiveGlassTime = "";
                string eq_Par = "";
                bool IsRecvDelayTimeNGFlag;

                //check ela1, ela2 recipe, if only cln process, not need check delay time for fetch glass from cst.
                ela = ObjectManager.EquipmentManager.GetEQP("L4");
                ela1_PPID = curBcsJob.PPID.Substring(ela.Data.RECIPEIDX, ela.Data.RECIPELEN);
                ela = ObjectManager.EquipmentManager.GetEQP("L5");
                ela2_PPID = curBcsJob.PPID.Substring(ela.Data.RECIPEIDX, ela.Data.RECIPELEN);
                curByPassPPID = new string('0', ela.Data.RECIPELEN);
                if (ela1_PPID == curByPassPPID && ela2_PPID == curByPassPPID)
                {
                    return true;
                }

                //MQC glass, not need check
                if (curBcsJob.ArraySpecial.ProcessType.Trim().Equals("1"))
                    return true;

                //if first glass delay time check NG, mode NG, status NG, after galss not need check and reply NG
                IsRecvDelayTimeNGFlag = (bool)curRobot.Context[eRobotContextParameter.IsRecvDelayTimeNGFlag];
                if (IsRecvDelayTimeNGFlag)
                    return false;

                #region [ get Max from QTime Def for L4 ProcessTime ]
                //get Max from QTime Def for L4 ProcessTime; 
                foreach (QtimeEntityData L4Processtime in ObjectManager.QtimeManager._entitiesDB.Values)
                {
                    if (L4Processtime.ENABLED.Equals("Y") && L4Processtime.STARTNODENO.Equals("L4") && L4Processtime.STARTEVENTMSG.Trim().Equals("RECEIVE")
                        && L4Processtime.ENDNODENO.Equals("L4") && L4Processtime.ENDEVENTMSG.Trim().Equals("RECEIVE"))
                    {
                        if (L4Processtime.STARTNODERECIPEID.Trim().Length == 0)
                        {
                            if (L4Processtime.SETTIMEVALUE > ela_ProcessTime_Default)
                            {
                                ela_ProcessTime_Default = L4Processtime.SETTIMEVALUE;
                            }
                        }
                        else
                        {
                            if (L4Processtime.STARTNODERECIPEID.Trim() == ela1_PPID.Trim())
                            {
                                if (L4Processtime.SETTIMEVALUE > ela_ProcessTime_Recipe)
                                {
                                    ela_ProcessTime_Recipe = L4Processtime.SETTIMEVALUE;
                                }
                            }
                        }
                    }
                }

                if (ela_ProcessTime_Recipe > 0)
                    ela_ProcessTime = ela_ProcessTime_Recipe;
                else if (ela_ProcessTime_Default > 0)
                    ela_ProcessTime = ela_ProcessTime_Default;
                else
                    ela_ProcessTime = 0;

                //if not define in QTime, get EQ report process time
                if (ela_ProcessTime == 0)
                {
                    string trxName = string.Format("L4_ProcessTimeBlock");
                    Trx trxProcessTime = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                    if (trxProcessTime == null)
                        return false;
                    ela_ProcessTime = int.Parse(trxProcessTime.EventGroups[0].Events[0].Items[0].Value);
                }

                ela = ObjectManager.EquipmentManager.GetEQP("L4");
                if (ela == null)
                    return false;
                if (ela.File.EquipmentRunMode.ToUpper().Equals("NORMAL"))
                {
                    ELA_SumNormalMode++;
                    if ((ela.File.Status == eEQPStatus.IDLE || ela.File.Status == eEQPStatus.RUN) && ela.File.CIMMode == eBitResult.ON && ela1_PPID != curByPassPPID)
                        ELA_SumProcessTime = ELA_SumProcessTime + ela_ProcessTime;
                    else
                        ELA_SumStatusDown++;
                }
                #endregion

                eq_Par = eq_Par + "ELA1's ProcessTime(" + ela_ProcessTime + "), RunMode(" + ela.File.EquipmentRunMode.ToUpper() + "), Status(" + ela.File.Status + "). ";

                ela_ProcessTime_Recipe = 0;
                ela_ProcessTime_Default = 0;

                #region [ get Max from QTime Def for L5 ProcessTime ]
                //get Max from QTime Def for L5 ProcessTime; 
                foreach (QtimeEntityData L5Processtime in ObjectManager.QtimeManager._entitiesDB.Values)
                {
                    if (L5Processtime.ENABLED.Equals("Y") && L5Processtime.STARTNODENO.Equals("L5") && L5Processtime.STARTEVENTMSG.Trim().Equals("RECEIVE")
                        && L5Processtime.ENDNODENO.Equals("L5") && L5Processtime.ENDEVENTMSG.Trim().Equals("RECEIVE"))
                    {
                        if (L5Processtime.STARTNODERECIPEID.Trim().Length == 0)
                        {
                            if (L5Processtime.SETTIMEVALUE > ela_ProcessTime_Default)
                            {
                                ela_ProcessTime_Default = L5Processtime.SETTIMEVALUE;
                            }
                        }
                        else
                        {
                            if (L5Processtime.STARTNODERECIPEID.Trim() == ela2_PPID.Trim())
                            {
                                if (L5Processtime.SETTIMEVALUE > ela_ProcessTime_Recipe)
                                {
                                    ela_ProcessTime_Recipe = L5Processtime.SETTIMEVALUE;
                                }
                            }
                        }
                    }
                }

                if (ela_ProcessTime_Recipe > 0)
                    ela_ProcessTime = ela_ProcessTime_Recipe;
                else if (ela_ProcessTime_Default > 0)
                    ela_ProcessTime = ela_ProcessTime_Default;
                else
                    ela_ProcessTime = 0;

                //if not define in QTime, get EQ report process time
                if (ela_ProcessTime == 0)
                {
                    string trxName = string.Format("L5_ProcessTimeBlock");
                    Trx trxProcessTime = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                    if (trxProcessTime == null)
                        return false;
                    ela_ProcessTime = int.Parse(trxProcessTime.EventGroups[0].Events[0].Items[0].Value);
                }
                ela = ObjectManager.EquipmentManager.GetEQP("L5");
                if (ela == null)
                    return false;
                if (ela.File.EquipmentRunMode.ToUpper().Equals("NORMAL"))
                {
                    ELA_SumNormalMode++;
                    if ((ela.File.Status == eEQPStatus.IDLE || ela.File.Status == eEQPStatus.RUN) && ela.File.CIMMode == eBitResult.ON && ela2_PPID != curByPassPPID)
                        ELA_SumProcessTime = ELA_SumProcessTime + ela_ProcessTime;
                    else
                        ELA_SumStatusDown++;
                }
                #endregion

                eq_Par = eq_Par + "ELA2's ProcessTime(" + ela_ProcessTime + "), RunMode(" + ela.File.EquipmentRunMode.ToUpper() + "), Status(" + ela.File.Status + "). ";

                if (ELA_SumNormalMode == 0) //No normal mode, don't fetch glass
                {
                    curRobot.Context.AddParameter(eRobotContextParameter.IsRecvDelayTimeNGFlag, true);
                    errMsg = string.Format("[{0}] Job({1}_({2}) can not Get Current Check Step({3}) Stage List({4}) Has Not Any ELA EQ Run Mode = NORMAL !",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck_ModeNotMatch);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck_ModeNotMatch;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }

                if (ELA_SumNormalMode == ELA_SumStatusDown) //All normal are down, don't put to clean
                {
                    curRobot.Context.AddParameter(eRobotContextParameter.IsRecvDelayTimeNGFlag, true);
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Check Step({3}) Stage List({4}) ELA EQ Run Mode = NORMAL ARE (DOWN or CIMOFF)*({5})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ELA_SumStatusDown);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck_StatusAllDown);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck_StatusAllDown;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
               
                Line line = ObjectManager.LineManager.GetLines()[0];
                if (line == null)
                {
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Check Step({3}) Stage List({54}) for Get Line Object is Null",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                compensationtime = (double)line.File.FetchCompensationTime;
                if (compensationtime > 0)
                    compensationtime /= (double)(ELA_SumNormalMode - ELA_SumStatusDown) / 2;
                else
                    compensationtime *= (double)(ELA_SumNormalMode - ELA_SumStatusDown) / 2;  //modify by yang 20170214 for only one ELA ,compensationtime调整

                eq_Par = eq_Par + "CompensationTime(" + compensationtime + ")";

                delaytime = (double)(ELA_SumProcessTime / ((ELA_SumNormalMode - ELA_SumStatusDown) * (ELA_SumNormalMode - ELA_SumStatusDown)));
                delaytime = delaytime + compensationtime;

                List<RobotStage> curCheckStepStageList = new List<RobotStage>();
                string[] curCheckStepStageIDList = curCheckRouteStep.Data.STAGEIDLIST.Split(',');

                for (int i = 0; i < curCheckStepStageIDList.Length; i++)
                {
                    RobotStage curStage;

                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curCheckStepStageIDList[i]);

                    if (curStage == null)
                    {
                        curRobot.Context.AddParameter(eRobotContextParameter.IsRecvDelayTimeNGFlag, true);
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

                    if (curStageSelectCanUseStageList.Contains(curStage) == true)
                    {
                        Equipment eq = ObjectManager.EquipmentManager.GetEQP(curStage.Data.NODENO);
                        double time;
                        if (eq.File.FinalELAStageReceiveTime.Trim().Length > 0)
                        {
                            DateTime dtfinal;
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo();
                            dtFormat.ShortDatePattern = "yyyy/MM/dd HH:mm:ss";
                            eq_FinalReceiveGlassTime = eq_FinalReceiveGlassTime + eq.File.FinalELAStageReceiveTime + " ";
                            eq_NextReceiveGlassTime = eq_NextReceiveGlassTime + Convert.ToDateTime(eq.File.FinalELAStageReceiveTime).AddSeconds(delaytime);
                            dtfinal = Convert.ToDateTime(eq.File.FinalELAStageReceiveTime, dtFormat);
                            time = (DateTime.Now - dtfinal).TotalSeconds;
                            if (time < delaytime)
                                continue;
                        }

                        if (curCheckStepStageList.Contains(curStage) == false)
                        {
                            curCheckStepStageList.Add(curStage);
                        }
                    }
                }

                //找不到任一個符合的Stage則回覆異常
                if (curCheckStepStageList == null || curCheckStepStageList.Count == 0)
                {
                    curRobot.Context.AddParameter(eRobotContextParameter.IsRecvDelayTimeNGFlag, true);
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5}) FinalReceiveGlassTime({6}) Next ReceiveTime({7}). DelayTime({8} = {9})",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, eq_FinalReceiveGlassTime, eq_NextReceiveGlassTime, delaytime, eq_Par);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Check Step({3}) Stage List({4}) FinalReceiveGlassTime({5}) Next ReceiveTime({6}). DelayTime({7} = {8})",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, eq_FinalReceiveGlassTime, eq_NextReceiveGlassTime, delaytime, eq_Par);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_ELACleanStageReceiveDelayTimeCheck;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                   
                    return false;
                }
                #endregion

                curStageSelectCanUseStageList = curCheckStepStageList;
                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curCheckStepStageList);
                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }
    }
}
