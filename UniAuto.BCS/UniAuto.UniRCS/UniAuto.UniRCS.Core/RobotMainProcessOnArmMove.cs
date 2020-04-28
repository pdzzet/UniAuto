using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Threading;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.MISC;


namespace UniAuto.UniRCS.Core
{
    public partial class RobotCoreService
    {

        //For Type I Normal Robot Arm GetGet-PutPut Use Function List -=======================================================================================================================================

        /// <summary> for Robot Type I[ One Robot has 2 Arm,Arm#01(Upper),Arm#02(Lower) ,One Arm has One Job Position.Can GetGetPutPut
        /// On Arm Move,帶片跑邏輯
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotAllStageList"></param>
        private void CheckRobotControlCommand_For_TypeI_ForGetGetPutPutOnArmMove(Robot curRobot, List<RobotStage> curRobotAllStageList)
        {
            bool checkFlag = false;
            string strlog = string.Empty;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
            List<Job> robotArmCanControlJobList_OrderBy = new List<Job>();
            List<Job> robotStageCanControlJobList_OrderBy = new List<Job>();

            try
            {
                #region [ 1. Check Can Issue Command ]
                if (!CheckCanIssueRobotCommand(curRobot)) return;
                #endregion

                #region [ 2. Get Arm Can Control Job List, Stage Can Control Job List and Update StageInfo ][ Wait_Proc_0003 ]

                #region [ Clear All Stage UDRQ And LDRQ Stage SlotNoList Info ]
                foreach (RobotStage stageItem in curRobotAllStageList)
                {
                    lock (stageItem)
                    {
                        stageItem.CassetteStartTime = DateTime.MinValue;
                        stageItem.UnloaderSamplingFlag = RobotStage.UNLOADER_SAMPLING_FLAG.UNKOWN;
                        stageItem.curLDRQ_EmptySlotList.Clear();
                        stageItem.curUDRQ_SlotList.Clear();
                        #region [ Clear All Port Route Info ] Watson 20160104
                        if (stageItem.Data.STAGETYPE == eRobotStageType.PORT)
                        {
                            if (curRobot.CurPortRouteIDInfo.ContainsKey(stageItem.Data.STAGEID))
                                curRobot.CurPortRouteIDInfo.Remove(stageItem.Data.STAGEID);
                            curRobot.CurPortRouteIDInfo.Add(stageItem.Data.STAGEID, eRobotCommonConst.ROBOT_ROUTE_NOUSE_NOCHECK);
                        }
                        #endregion

                        //20160302 add for Array Only
                        stageItem.CurRecipeGroupNoList.Clear();

                        //20160511 將每個RobotStage的可控Job紀錄的RecipeGroup清除
                        stageItem.AllJobRecipeGroupNoList.Clear();

                        //20160618 add for reset port's CurRouteID, assign in select function cc.kuang
                        stageItem.File.CurRouteID = string.Empty;
                    }
                }
                #endregion



                //One Robot Only One Select Rule,如有MIX Route則在Check FetchOut與Filter後 先照Route Priority排序再照STEP排序 以達到優先處理XX Route.如有其他特殊選片邏輯在特別處理
                #region [ Handle Robot Current Rule Job Select Function List ]
                Dictionary<string, List<RobotRuleSelect>> curRuleJobSelectList = ObjectManager.RobotManager.GetRouteSelect(curRobot.Data.ROBOTNAME);
                #region [ Check Select Rule Exist ]
                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00010 ] 
                fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_SELECTRULE_IS_NULL;

                if (curRuleJobSelectList == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any Select Rule!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00010 ]
                    if (!curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any cSelect Rule!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                        failMsg = string.Format("can not get any Select Rule!");

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                        #endregion
                    }
                    #endregion
                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00010 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }
                #endregion


                //20151027 add Set Robot Context
                IRobotContext robotConText = null;
                curRobot.Context = robotConText = new RobotContext();
                //Set 1st Job Command Info
                cur1stJob_CommandInfo cur1StJobCommandInfo = new cur1stJob_CommandInfo();
                curRobot.Context.AddParameter(eRobotContextParameter.Cur1stJob_CommandInfo, cur1StJobCommandInfo);

                //2016/01/26 add for reset eRobotContextParameter when need cc.kuang
                ReSetRobotContextParameter(curRobot); 

                #region [ Initial Select Rule List RobotConText Info. 搭配針對Select Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] ===========
                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurRobotAllStageListEntity, curRobotAllStageList);
                #endregion =========================================================================================================================================================

                //此時Robot無法得知要跑哪種Route,所以只會有一筆[ Wait_For_Proc_00026 ] 之後Table要拿掉RouteID以免誤解的相關處理
                foreach (string routeID in curRuleJobSelectList.Keys)
                {
                    #region [ 根據RuleJobSelect選出Can Control Job List ]
                    foreach (RobotRuleSelect curRuleJobSelect in curRuleJobSelectList[routeID])
                    {
                        //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_E0001 ] ,以Rule Job Select 的ObjectName與MethodName為Key來決定是否紀錄Log
                        fail_ReasonCode = string.Format("{0}_{1}", curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME);

                        #region[DebugLog][ Start Rule Job Select Function ]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) Start {5}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                    curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_START_CHAR, eRobotCommonConst.RULE_SELECT_START_CHAR_LENGTH));

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        if (curRuleJobSelect.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                        {
                            checkFlag = (bool)Invoke(curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME, new object[] { robotConText });

                            if (checkFlag == false)
                            {
                                #region[DebugLog][ End Rule Job Select Function ]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select Fail, object({2}) MethodName({3}) RtnCode({4})  RtnMsg({5}]!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_E0001 ]

                                if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select Fail, object({2}) MethodName({3}) RtnCode({4})  RtnMsg({5}]!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                    //failMsg = string.Format("Robot({0}) object({1}) MethodName({2}) RtnCode({3})  RtnMsg({4}]!",
                                    //                        curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME, robotConText.GetReturnCode(),
                                    //                        robotConText.GetReturnMessage());

                                    failMsg = string.Format("RtnCode({0})  RtnMsg({1})!",robotConText.GetReturnCode(),robotConText.GetReturnMessage());

                                    AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                    #endregion

                                }

                                #endregion

                                //有重大異常直接結束配片邏輯要求人員介入處理
                                //20160114 modify SEMI Mode 還是要可以執行下一個Select 條件.不須結束配片邏輯
                                if (curRobot.File.curRobotRunMode == eRobot_RunMode.AUTO_MODE)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                //Clear[ Robot_Fail_Case_E0001 ]
                                RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                                #region[DebugLog][ End Rule Job Select Function ]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

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

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                        curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion
                        }

                    }

                    #endregion
                    break; //目前只處理第一筆
                }
                #endregion

                #region [ Get Arm Can Control Job List ]

                List<Job> robotArmCanControlJobList;

                robotArmCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.ArmCanControlJobList];

                //當沒有設定參數時會回傳NULL,需防呆
                if (robotArmCanControlJobList == null)
                {
                    robotArmCanControlJobList = new List<Job>();
                }

                #endregion

                #region [ Get Stage Can Control Job List ]

                List<Job> robotStageCanControlJobList;

                robotStageCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.StageCanControlJobList];

                //當沒有設定參數時會回傳NULL,需防呆
                if (robotStageCanControlJobList == null)
                {
                    robotStageCanControlJobList = new List<Job>();
                }

                #endregion

                #endregion

                #region [ 3. Update OPI Stage Display Info ][ Wait_Proc_0005 ]
                bool sendToOPI = false;

                foreach (RobotStage stage_entity in curRobotAllStageList)
                {
                    if (stage_entity.File.StatusChangeFlag)
                    {
                        sendToOPI = true;

                        lock (stage_entity.File)
                        {
                            stage_entity.File.StatusChangeFlag = false;
                        }
                    }
                }

                if (sendToOPI)
                {
                    //通知OPI更新LayOut畫面, //20151126 add by Robot Arm Qty來區分送給OPI的狀態訊息 
                    Invoke(eServiceName.UIService, "RobotStageInfoReport", new object[] { curRobot.Data.LINEID, curRobot });
                }
                #endregion

                #region [ 如果是SEMI Mode只需做到取得目前可控制Job並更新資訊即可 (下面邏輯不需再處理了!) ]
                if (curRobot.File.curRobotRunMode == eRobot_RunMode.SEMI_MODE)
                {
                    #region Array Special for DRY
                    //20160107-001-dd
                    if (Workbench.LineType.ToString().Contains("DRY_"))
                    {
                        curRobot.File.DryLastProcessType = string.Empty; //reset
                        curRobot.File.DryCycleCnt = 0;
                        curRobot.File.DRYLastEnterStageID = 0;
                    }
                    #endregion

                    return;
                }
                #endregion

                #region [  Check Can Control Job Exist ]
                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00006 ] 
                fail_ReasonCode = eRobot_CheckFail_Reason.GET_CAN_CONTROL_JOB_FAIL;

                if (robotArmCanControlJobList.Count == 0 && robotStageCanControlJobList.Count == 0) //都為0 沒有可以處理的基板, 結束這回合!!
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any can control Job!(Please check 1.Robot Arm would have Job 2.CST(Load/Both port = wait for process/In process) is Ready and Exist(JobEachCassetteSlotExists and JobEachCassetteSlotPosition) 3.Upstream EQP would SendOut Job)",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00006 ]
                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any can control Job!(Please check 1.Robot Arm would have Job 2.CST(Load/Both port = wait for process/In process) is Ready and Exist(JobEachCassetteSlotExists and JobEachCassetteSlotPosition) 3.Upstream EQP would SendOut Job)", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("can not get any can control Job!(Please check 1.Robot Arm would have Job 2.CST(Load/Both port = wait for process/In process) is Ready and Exist(JobEachCassetteSlotExists and JobEachCassetteSlotPosition) 3.Upstream EQP wound SendOut Job)");
                        failMsg = string.Format("RtnCode({0}) RtnMsg({1})", fail_ReasonCode, "can not get any can control Job!(Please check 1.Robot Arm would have Job 2.CST(Load/Both port = wait for process/In process) is Ready and Exist(JobEachCassetteSlotExists and JobEachCassetteSlotPosition) 3.Upstream EQP would SendOut Job");
                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }
                    #endregion

                    #region Array Special for DRY
                    //20160107-001-dd
                    if (Workbench.LineType.ToString().Contains("DRY_"))
                    {
                        curRobot.File.DryLastProcessType = string.Empty; //reset
                        curRobot.File.DryCycleCnt = 0;
                        curRobot.ReCheck = false;
                        curRobot.MixNo = 1;//Add Yang 20160907
                    }
                    #endregion

                    #region CF Sorter Mode, Port上無片可抽, 回到以 Grade OK 優先
                    {
                        if (StaticContext.ContainsKey(eRobotContextParameter.SorterMode_RobotParam))
                        {
                            SorterMode_RobotParam srt_param = (SorterMode_RobotParam)StaticContext[eRobotContextParameter.SorterMode_RobotParam];
                            srt_param.LastGrade = SorterMode_RobotParam.DEFAULT_FIRST_PRIORITY_GRADE;
                        }
                    }
                    #endregion

                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00006 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }
                #endregion

                curRobot.Context.AddParameter(eRobotContextParameter.IsType2Flag, false); //代表是要走 TypeI 的逻辑!! 20160108-002-dd

                #region [ Handle Robot Arm Job List First ]
                if (robotArmCanControlJobList.Count != 0)
                {
                    #region [ Robot Arm上有片的處理 ]

                    //20151110 add 取得Job所在Stage的Priority
                    UpdateStagePriority(robotArmCanControlJobList);

                    //排序 以Step越小, PortStatus In_Prcess為優先處理 .因都在Robot Arm上所以不需by Job Location StageID排序
                    robotArmCanControlJobList_OrderBy = robotArmCanControlJobList.OrderByDescending(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();

                    foreach (Job curRobotArmJob in robotArmCanControlJobList_OrderBy)
                    {
                        if (CheckRobotArmJobRouteCondition_ForGetGetPutPut(curRobot, curRobotArmJob)) return; //True表示命令已產生則結束本次cycle
                    }
                    #region [ 判斷是否上有1st Job Command尚未下命令 ]
                    cur1stJob_CommandInfo curJudgeCommandInfo = (cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo];

                    if (curJudgeCommandInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    {
                        //有1stJob Command 則下命令Send Robot Control Command                       
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curJudgeCommandInfo.cur1stJob_Command });
                        return; //不管有沒有, 都直接return;
                    }

                    #endregion
                    #region [ Robot Arm上帶片跑 ]
                    //20160705
                    if (curRobot.CurTempArmSingleJobInfoList[0].ArmJobExist == eGlassExist.NoExist || curRobot.CurTempArmSingleJobInfoList[1].ArmJobExist == eGlassExist.NoExist)
                    {
                        UpdateStagePriority(robotStageCanControlJobList);
                        //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ThenBy(s => s.WaitForProcessTime).ToList();
                        //Arm上有片時,帶片跑,要判斷接下來要取的Job,curStepNo是不是比Arm上的Job大,代表是下一個EQP stage的Job
                        //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.Where(s => s.RobotWIP.CurStepNo > robotArmCanControlJobList_OrderBy[0].RobotWIP.CurStepNo).ToList();
                        //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.Where(s => s.RobotWIP.LastSendStageID != "" && Int32.Parse(s.RobotWIP.LastSendStageID) > 10 && (s.RobotWIP.LastSendStageID != robotArmCanControlJobList_OrderBy[0].RobotWIP.LastSendStageID)).ToList();
                        //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.Where(s => s.RobotWIP.LastSendStageID != "" && Int32.Parse(s.RobotWIP.LastSendStageID) > 10).ToList();

                        //20160803 把跟Arm上同一個出片的來源排除掉,例如留在Arm上Job是從CST出來的,那就避免帶片跑另一個Arm又取到從CST出來的Job,導致Arm上2片因為下游機台不收片,而都卡住動不了
                        //所以要跟留在Arm上的stage是不一樣的,才能帶片跑


                        //DRY job fetch out by UnitNo(only for DRY),for dry的orderby
                        Line _line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                        if (Workbench.LineType.ToString().Contains("DRY_") && _line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                        {
                            //Yang 20160819
                            Invoke("RobotSpecialService", "CheckDRYMixFetchOutByUnitNo", new object[] { curRobot, robotStageCanControlJobList });
                            robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurStepNo).ThenByDescending(s => s.RobotWIP.dryprocesstypepriority).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ThenBy(s => s.WaitForProcessTime).ToList();
                        }
                         //else    robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.Where(s => s.RobotWIP.LastSendStageID != robotArmCanControlJobList_OrderBy[0].RobotWIP.LastSendStageID).ToList();
                         //增加插队卡夹的 Prefetch 优先级Priority高于Inprocessing Modified by Zhangwei 20161020
                        else robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ThenBy(s => s.WaitForProcessTime).ToList();
                        #region [ CVD Proportional Typ Check (Array Special for CVD) ]
                        if (Workbench.LineType == eLineType.ARRAY.CVD_ULVAC || Workbench.LineType == eLineType.ARRAY.CVD_AKT)
                        {
                            //Watson Add CVD 20151001
                            Invoke("RobotSpecialService", "CheckCVDProportionalType", new object[] { curRobot, robotStageCanControlJobList_OrderBy });
                        }
                        #endregion
                        List<Job> _tempRobotStageCanControlJobList = new List<Job>();
                        _tempRobotStageCanControlJobList.Clear();
                        foreach (Job curRobotStageJob in robotStageCanControlJobList_OrderBy)
                        {
                            if (CheckRobotStageJobRouteCondition_ForGetGetPutPut(curRobot, curRobotStageJob)) return;

                            //Cassette cassett = ObjectManager.CassetteManager.GetCassette(curRobotStageJob.FromCstID);

                            //只要有CST 处于Waiting fot MES Reply FirstGlassCheck 状态，就不出片 Modified by Zhangwei 20161104
                            //if (cassett.FirstGlassCheckReport == "C2") return;
                        }
                        #region [ 判斷是否上有1st Job Command尚未下命令 ]
                         curJudgeCommandInfo = (cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo];

                        if (curJudgeCommandInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                        {
                            //有1stJob Command 則下命令Send Robot Control Command                       
                            bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curJudgeCommandInfo.cur1stJob_Command });
                            return; //不管有沒有, 都直接return;
                        }

                        #endregion
                        
                    }
                    #endregion
                    #region [ 判斷是否上有1st Job Command尚未下命令 ]
                     curJudgeCommandInfo = (cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo];

                    if (curJudgeCommandInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    {
                        //有1stJob Command 則下命令Send Robot Control Command                       
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curJudgeCommandInfo.cur1stJob_Command });
                        return; //不管有沒有, 都直接return;
                    }

                    #endregion
                    #region Array Special for DRY
                    //20160107-001-dd
                    if (Workbench.LineType.ToString().Contains("DRY_")) if (curRobot.File.DryLastProcessType != string.Empty) curRobot.File.DryCycleCnt++;
                    #endregion

                    #region Sorter Mode
                    {
                        //程式碼跑到這裡表示沒有下RobotCommand
                        Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                        if (line.Data.FABTYPE == eFabType.CF.ToString() &&
                            line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE &&
                            StaticContext.ContainsKey(eRobotContextParameter.SorterMode_RobotParam))
                        {
                            SorterMode_RobotParam srt_param = (SorterMode_RobotParam)StaticContext[eRobotContextParameter.SorterMode_RobotParam];
                            if (robotStageCanControlJobList.Count > 0)
                            {
                                // Sorter Mode 下有找到 StageCanControlJob, 但程式碼卻跑到這裡, 表示有 StageJob 沒出片
                                // 檢查StageJob, Job Grade是否與Unloader Mapping Grade相同
                                // 如果全部Job Grade都沒有與Unloader Mapping Grade相同, 就必須呼叫CassetteService
                                #region NeedToCallCassetteService
                                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
                                List<Port> ports = ObjectManager.PortManager.GetPortsByLine(curRobot.Data.LINEID);
                                bool unloader_ready = false;//有至少一個InProcess或WaitForProcess的Unloader
                                List<PortStage> mapping_port_stages = null;//與當前JobGrade符合的Port
                                Job stage_job = null;//有找到mapping port的job
                                foreach (Job job in robotStageCanControlJobList)
                                {
                                    if (!job.RobotWIP.SorterMode_OtherFilterOK)
                                        continue;// 不考慮因為 Grade 以外的 Filter 而被過濾掉的 Job, 表示不能出片的 Job 不考慮
                                    // 能出片則繼續判斷是否要退Cassette
                                    mapping_port_stages = SorterMode_JobGradeUnloaderGrade(eqp, ports, curRobotAllStageList, job, ref unloader_ready);
                                    if (mapping_port_stages.Count <= 0)
                                    {
                                        //找不到 mapping port
                                        if (unloader_ready)
                                        {
                                            //當 Unloader 有 Cassette 且 InProcess 或 WaitForProcess 時, 但仍然找不到 mapping port, 就需要呼叫 Cassette Serivce 做退 Cassette
                                            srt_param.NeedToCallCassetteService = SorterMode_RobotParam.NEED_TO_CALL_CST_SERVICE.NEED_TO_CALL;
                                            stage_job = job;
                                        }
                                    }
                                    else
                                    {
                                        srt_param.NeedToCallCassetteService = SorterMode_RobotParam.NEED_TO_CALL_CST_SERVICE.ONE_JOB_MATCH;//有找到Job Grade相同的Unloader, 不需要呼叫CassetteService
                                        stage_job = job;
                                        break;
                                    }
                                }
                                #endregion
                                if (srt_param.EnableCallCassetteService && srt_param.NeedToCallCassetteService == SorterMode_RobotParam.NEED_TO_CALL_CST_SERVICE.NEED_TO_CALL)
                                {
                                    srt_param.EnableCallCassetteService = false;//直到ProcResult_JobMoveToRobotArm_1Arm1Job_forFCSRT, 才會再變為true
                                    if (ParameterManager.Parameters.ContainsKey("ROBOT_ENABLE_CALL_CASSETTE_SERVICE"))
                                    {
                                        ParameterManager.Parameters["ROBOT_ENABLE_CALL_CASSETTE_SERVICE"].Value = srt_param.EnableCallCassetteService.ToString();
                                    }
                                    string method_name = "CassetteStoreQTimeProcessEnd";
                                    strlog = string.Format("Invoke CassetteService.{0}()", method_name);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    Invoke(eServiceName.CassetteService, method_name, new object[] { });
                                }
                                else
                                {
                                    #region Debug Log
                                    if (IsShowDetialLog == true)
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        sb.AppendFormat("Do Not Invoke CassetteStoreQTimeProcessEnd. EnableCallCassetteService({0}) NeedToCallCassetteService({1}) UnloaderReady({2})", srt_param.EnableCallCassetteService, srt_param.NeedToCallCassetteService, unloader_ready);
                                        if (stage_job != null) sb.AppendFormat("Source Job({0}, {1})", stage_job.JobKey, stage_job.JobGrade);
                                        if (mapping_port_stages != null)
                                        {
                                            sb.AppendFormat("Mapping Ports(");
                                            foreach (PortStage port_stage in mapping_port_stages)
                                                sb.AppendFormat("PortNo({0}) PortMode({1}) PortGrade({2}) EmptySlot({3}),", port_stage.Port.Data.PORTNO, port_stage.Port.File.Mode.ToString(), port_stage.Port.File.MappingGrade, port_stage.Stage.curLDRQ_EmptySlotList.Count);
                                            if (mapping_port_stages.Count > 0) sb.Remove(sb.Length - 1, 1);
                                            sb.AppendFormat(")");
                                        }
                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", sb.ToString());
                                    }
                                    #endregion
                                }
                            }
                            else
                            {
                                #region Debug Log
                                if (IsShowDetialLog == true)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendFormat("Do Not Invoke CassetteStoreQTimeProcessEnd. robotStageCanControlJobList.Count is 0");
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", sb.ToString());
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                    #endregion
                }
                else
                {
                    #region [ Robot Arm上無片的處理 ]

                    //20151110 add 取得Job所在Stage的Priority
                    UpdateStagePriority(robotStageCanControlJobList);

                    //20151110 Add For先依Stage Priority排序越大越優先, 再依Step排序越小越優先, 最後依CurPortCstStatusPriority排序越小越優先
                    //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();
                    //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();
                    //add sort by cst waitforstarttime 2016/03/29 cc.kuang
                    robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ThenBy(s => s.WaitForProcessTime).ToList();
                  
                    #region [ CVD Proportional Typ Check (Array Special for CVD) ]
                    //Add  for CVD 20160830
                    if (Workbench.LineType == eLineType.ARRAY.CVD_ULVAC || Workbench.LineType == eLineType.ARRAY.CVD_AKT)
                        //Watson Add CVD 20151001
                        Invoke("RobotSpecialService", "CheckCVDProportionalType", new object[] { curRobot, robotStageCanControlJobList_OrderBy });
                    #endregion

                    //Yang Add CVD 20161001
                    //遵循先洗完的Glass先喂给CVD
                    // mark by yang 2017/5/25 做成cfg,方便之后维护CLN+mainEQP 这种布局的line,进行temporal EQP RTC
                    //以后整理的时候可以转移下位置->special service or 做成filter(max priority)
                    #region[special temporal EQP(CLN) RTC]
                    if (ConstantManager.ContainsKey(eNoNeedSendToCLN.NONEEDSENDTOCLN))
                    {
                        if(ConstantManager[eNoNeedSendToCLN.NONEEDSENDTOCLN].Values.ContainsKey(curRobot.Data.LINEID))
                        {
                            if (ConstantManager[eNoNeedSendToCLN.NONEEDSENDTOCLN].Values.Where(s => s.Key.Equals(curRobot.Data.LINEID)).FirstOrDefault().Value.Value.Equals("true"))                 
                            {
                                //if (curRobot.Data.LINEID.Contains("300") || curRobot.Data.LINEID.Contains("400"))
                                //{

                                    //have EQP RTC, check EQP RTC glass can fetch out from cst currently
                                if (robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.EQPRTCFlag == true).Count() > 0)
                                {
                                    curRobot.CLNRTCWIP = true;
                                    Job job1 = robotStageCanControlJobList_OrderBy.FirstOrDefault();
                                    string currentrouteid = job1.RobotWIP.CurRouteID;
                                    RobotRouteCondition currentroute = ObjectManager.RobotManager.GetRouteCondition(curRobot.Data.ROBOTNAME, currentrouteid).FirstOrDefault();
                                    #region[这种dir的写法可做参考]
                                    //  Dictionary<string,List<RobotRouteCondition>> routeconditions = ObjectManager.RobotManager.GetRouteConditionsByRobotName(curRobot.Data.ROBOTNAME);

                                    //  for(int i=0;i<=routeconditions.Count();i++)
                                    //{
                                    //    List<RobotRouteCondition> checkroute = routeconditions.Values.Where(s => string.IsNullOrEmpty(s[i].Data.REMARKS)).FirstOrDefault();
                                    //    if (checkroute.Count() > 0) ;
                                    //}
                                    #endregion

                                    string _limitcount = string.Empty;
                                    string _fetchcount = string.Empty;
                                    int limitcount;
                                    int fetchcount;
                                    if (currentroute.Data.REMARKS.Contains(',')) //first value: limit RTC Count , second value:limit RTC Count which glass can fetch out from cst
                                    {
                                        _limitcount = currentroute.Data.REMARKS.Trim().Split(',')[0];
                                        _fetchcount = currentroute.Data.REMARKS.Trim().Split(',')[1];
                                    }
                                    else _limitcount = currentroute.Data.REMARKS.Trim();

                                    if (int.TryParse(_limitcount, out limitcount))  //for stop fetch out
                                    {
                                        if (robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.EQPRTCFlag == true).Count() >= limitcount)
                                            curRobot.noSendToCLN = true;
                                        else
                                            curRobot.noSendToCLN = false;
                                    }
                                    else curRobot.noSendToCLN = false;
                                    if (int.TryParse(_fetchcount, out fetchcount))   //for continue fetch out 
                                    {
                                        if (robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.EQPRTCFlag == true).Count() <= fetchcount)
                                            curRobot.fetchforRTC = true;
                                        else
                                            curRobot.fetchforRTC = false;
                                    }
                                    else curRobot.fetchforRTC = true;
                                }
                                else
                                {
                                    curRobot.CLNRTCWIP = false;
                                    curRobot.fetchforRTC = true;  //add
                                    curRobot.noSendToCLN = false;
                                }
                                }
                            //}
                        }
                    }
                    #endregion
             

                    List<Job> _tempRobotStageCanControlJobList = new List<Job>();
                    _tempRobotStageCanControlJobList.Clear();
                    foreach (Job curRobotStageJob in robotStageCanControlJobList_OrderBy)
                    {
                        //if (curRobotStageJob.RobotWIP.RTCReworkFlag && !_tempRobotStageCanControlJobList.Contains(curRobotStageJob)) //有做过RTC的基板, 先不处理, 优先处理正常为出片的基板!!
                        //{
                        //    _tempRobotStageCanControlJobList.Add(curRobotStageJob);
                        //    continue;
                        //}
                        if (CheckRobotStageJobRouteCondition_ForGetGetPutPut(curRobot, curRobotStageJob)) return; //True表示命令已產生則結束本次cycle

                        //Cassette cassett = ObjectManager.CassetteManager.GetCassette(curRobotStageJob.FromCstID);

                        //只要有CST 处于Waiting fot MES Reply FirstGlassCheck 状态，就不出片 Modified by Zhangwei 20161104
                        //if (cassett.FirstGlassCheckReport == "C2") return;

                    }
                    #region [ 判斷是否上有1st Job Command尚未下命令 ]
                    cur1stJob_CommandInfo curJudgeCommandInfo = (cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo];

                    if (curJudgeCommandInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    {
                        //有1stJob Command 則下命令Send Robot Control Command                       
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curJudgeCommandInfo.cur1stJob_Command });
                        return; //不管有沒有, 都直接return;
                    }

                    #endregion
                    //if (_tempRobotStageCanControlJobList.Count > 0)
                    //{
                    //    _tempRobotStageCanControlJobList = _tempRobotStageCanControlJobList.OrderBy(s => s.RobotWIP.PreFetchFlag).ToList();
                    //    foreach (Job curRobotStageJob in _tempRobotStageCanControlJobList)
                    //    {
                    //        if (CheckRobotStageJobRouteCondition_ForGetGetPutPut(curRobot, curRobotStageJob)) return; //True表示命令已產生則結束本次cycle
                    //    }
                    //}

                    
                    #region Prefetch
                    #region DRY Prefetch

                    //20160104, by dade, 新增逻辑, 针对DRY line的MIX mode, 不管有没有启动预取功能, 预设都是不作动!!
                    bool _doPrefetchFlag = true;
                    switch (Workbench.LineType.ToUpper())
                    {
                        case eLineType.ARRAY.DRY_ICD:
                        case eLineType.ARRAY.DRY_YAC:
                        case eLineType.ARRAY.DRY_TEL:
                            Line _line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                            if (_line == null) _doPrefetchFlag = false; //not-found, skip Prefetch
                            if (_line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE) _doPrefetchFlag = false; //MIX, skip Prefetch
                            //if (_line.File.LineOperMode.ToUpper() == "MIX" || _line.File.LineOperMode.ToUpper() == "MIXEDRUNMODE") _doPrefetchFlag = false; //MIX, skip Prefetch
                            break;
                        default: break;
                    }
                    #endregion
                    

                    if (_doPrefetchFlag && (bool)Invoke("RobotSpecialService", "Check_PreFetch_DelayTime_For1Arm1Job", new object[] { robotConText })) //第一次或是超过delay time就可以考虑预取!!
                    {
                        //上述都没有可以跑的基板, 正常的部分check完成后, 接下来就要去判断...有没有要预取的基板 (前提条件要开启 预取 功能!)
                        //虽然有开启 预取 功能, 但是仍然需要去判断其他的项目是不是有启动!! 如果有启动, 则视同 预取 没作动!! 2015-12-26
                        bool _runPrefetchFlag = false;

                        //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.CurLocation_StageType == eStageType.PORT).ToList();
                        //20160624 加入EQP預取,CurPortCstStatusPriority要判斷,愈小的排前面,Inprocess > Waitforprocess
                        //增加插队卡夹的 Prefetch 优先级Priority高于Inprocessing Modified by Zhangwei 20161020
                        robotStageCanControlJobList_OrderBy = robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.CurLocation_StageType == eStageType.PORT || s.RobotWIP.CurLocation_StageType == eStageType.EQUIPMENT).OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();

                        if (robotStageCanControlJobList_OrderBy.Count() > 0)
                        {
                            _tempRobotStageCanControlJobList.Clear();

                            RobotStage _curStage = null;
                            foreach (Job curRobotStageJob in robotStageCanControlJobList_OrderBy)
                            {
                                _curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotStageJob.RobotWIP.CurLocation_StageID);
                                if (!CheckPrefetchFlag(curRobot, _curStage)) continue; //如果為false, 就是沒有開啟Pre-Fetch功能!

                                _runPrefetchFlag = (bool)Invoke("RobotSpecialService", "Check_Stage_Prefetch_SpecialCondition_For1Arm1Job", new object[] { robotConText, _curStage, curRobotStageJob }); //判断是不是要跑 预取 功能!!
                                if (!_runPrefetchFlag) continue; //如果为true, 才是真的要做 预取 功能!!

                                //if (curRobotStageJob.RobotWIP.RTCReworkFlag && !_tempRobotStageCanControlJobList.Contains(curRobotStageJob)) //有做过RTC的基板, 先不处理, 优先处理正常为出片的基板!!
                                //{
                                //    _tempRobotStageCanControlJobList.Add(curRobotStageJob);
                                //    continue;
                                //}
                                if (CheckRobotStageJobRouteCondition_ForGetGetPutPut(curRobot, curRobotStageJob)) return; //True表示命令已產生則結束本次cycle

                                //Cassette cassett = ObjectManager.CassetteManager.GetCassette(curRobotStageJob.FromCstID);

                                //只要有CST 处于Waiting fot MES Reply FirstGlassCheck 状态，就不出片 Modified by Zhangwei 20161104
                                //if (cassett.FirstGlassCheckReport == "C2") return;
                            }
                            //if (_tempRobotStageCanControlJobList.Count > 0)
                            //{
                            //    _tempRobotStageCanControlJobList = _tempRobotStageCanControlJobList.OrderBy(s => s.RobotWIP.PreFetchFlag).ToList();
                            //    foreach (Job curRobotStageJob in _tempRobotStageCanControlJobList)
                            //    {
                            //        if (CheckRobotStageJobRouteCondition_ForGetGetPutPut(curRobot, curRobotStageJob)) return; //True表示命令已產生則結束本次cycle
                            //    }
                            //}
                        }
                    }
                    //20160812 正常一個stage一個slot,沒slot,馬上下Command;但是可能有多個slot可去,例如CST slot,可能判斷到這邊都沒下Command
                    #region [ 判斷是否上有1st Job Command尚未下命令 ]
                    curJudgeCommandInfo = (cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo];

                    if (curJudgeCommandInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    {
                        //有1stJob Command 則下命令Send Robot Control Command                       
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curJudgeCommandInfo.cur1stJob_Command });
                        return; //不管有沒有, 都直接return;
                    }
                    #endregion

                    #endregion
                    #region Array Special for DRY
                    //20160107-001-dd
                    if (Workbench.LineType.ToString().Contains("DRY_")) if (curRobot.File.DryLastProcessType != string.Empty) curRobot.File.DryCycleCnt++;
                    #endregion

                    #region Sorter Mode
                    {
                        //程式碼跑到這裡表示沒有下RobotCommand
                        Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                        if (line.Data.FABTYPE == eFabType.CF.ToString() &&
                            line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE &&
                            StaticContext.ContainsKey(eRobotContextParameter.SorterMode_RobotParam))
                        {
                            SorterMode_RobotParam srt_param = (SorterMode_RobotParam)StaticContext[eRobotContextParameter.SorterMode_RobotParam];
                            if (robotStageCanControlJobList.Count > 0)
                            {
                                // Sorter Mode 下有找到 StageCanControlJob, 但程式碼卻跑到這裡, 表示有 StageJob 沒出片
                                // 檢查StageJob, Job Grade是否與Unloader Mapping Grade相同
                                // 如果全部Job Grade都沒有與Unloader Mapping Grade相同, 就必須呼叫CassetteService
                                #region NeedToCallCassetteService
                                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
                                List<Port> ports = ObjectManager.PortManager.GetPortsByLine(curRobot.Data.LINEID);
                                bool unloader_ready = false;//有至少一個InProcess或WaitForProcess的Unloader
                                List<PortStage> mapping_port_stages = null;//與當前JobGrade符合的Port
                                Job stage_job = null;//有找到mapping port的job
                                foreach (Job job in robotStageCanControlJobList)
                                {
                                    if (!job.RobotWIP.SorterMode_OtherFilterOK)
                                        continue;// 不考慮因為 Grade 以外的 Filter 而被過濾掉的 Job, 表示不能出片的 Job 不考慮
                                    // 能出片則繼續判斷是否要退Cassette
                                    mapping_port_stages = SorterMode_JobGradeUnloaderGrade(eqp, ports, curRobotAllStageList, job, ref unloader_ready);
                                    if (mapping_port_stages.Count <= 0)
                                    {
                                        //找不到 mapping port
                                        if (unloader_ready)
                                        {
                                            //當 Unloader 有 Cassette 且 InProcess 或 WaitForProcess 時, 但仍然找不到 mapping port, 就需要呼叫 Cassette Serivce 做退 Cassette
                                            srt_param.NeedToCallCassetteService = SorterMode_RobotParam.NEED_TO_CALL_CST_SERVICE.NEED_TO_CALL;
                                            stage_job = job;
                                        }
                                    }
                                    else
                                    {
                                        srt_param.NeedToCallCassetteService = SorterMode_RobotParam.NEED_TO_CALL_CST_SERVICE.ONE_JOB_MATCH;//有找到Job Grade相同的Unloader, 不需要呼叫CassetteService
                                        stage_job = job;
                                        break;
                                    }
                                }
                                #endregion
                                if (srt_param.EnableCallCassetteService && srt_param.NeedToCallCassetteService == SorterMode_RobotParam.NEED_TO_CALL_CST_SERVICE.NEED_TO_CALL)
                                {
                                    srt_param.EnableCallCassetteService = false;//直到ProcResult_JobMoveToRobotArm_1Arm1Job_forFCSRT, 才會再變為true
                                    if (ParameterManager.Parameters.ContainsKey("ROBOT_ENABLE_CALL_CASSETTE_SERVICE"))
                                    {
                                        ParameterManager.Parameters["ROBOT_ENABLE_CALL_CASSETTE_SERVICE"].Value = srt_param.EnableCallCassetteService.ToString();
                                    }
                                    string method_name = "CassetteStoreQTimeProcessEnd";
                                    strlog = string.Format("Invoke CassetteService.{0}()", method_name);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    Invoke(eServiceName.CassetteService, method_name, new object[] { });
                                }
                                else
                                {
                                    #region Debug Log
                                    if (IsShowDetialLog == true)
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        sb.AppendFormat("Do Not Invoke CassetteStoreQTimeProcessEnd. EnableCallCassetteService({0}) NeedToCallCassetteService({1}) UnloaderReady({2})", srt_param.EnableCallCassetteService, srt_param.NeedToCallCassetteService, unloader_ready);
                                        if (stage_job != null) sb.AppendFormat("Source Job({0}, {1})", stage_job.JobKey, stage_job.JobGrade);
                                        if (mapping_port_stages != null)
                                        {
                                            sb.AppendFormat("Mapping Ports(");
                                            foreach(PortStage port_stage in mapping_port_stages)
                                                sb.AppendFormat("PortNo({0}) PortMode({1}) PortGrade({2}) EmptySlot({3}),", port_stage.Port.Data.PORTNO, port_stage.Port.File.Mode.ToString(), port_stage.Port.File.MappingGrade, port_stage.Stage.curLDRQ_EmptySlotList.Count);
                                            if (mapping_port_stages.Count > 0) sb.Remove(sb.Length - 1, 1);
                                            sb.AppendFormat(")");
                                        }
                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", sb.ToString());
                                    }
                                    #endregion
                                }
                            }
                            else
                            {
                                #region Debug Log
                                if (IsShowDetialLog == true)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendFormat("Do Not Invoke CassetteStoreQTimeProcessEnd. robotStageCanControlJobList.Count is 0");
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", sb.ToString());
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion

                    #endregion
                }
                //add by yang 2017/2/23
                if (curRobot.CheckErrorList.Where(s => s.Value.Item3.Equals("0")).Count() != 0)
                    Invoke(eServiceName.EvisorService, "AppErrorSet", new object[] { curRobot.Data.LINEID, curRobot.CheckErrorList });
                  //  Invoke(eServiceName.EvisorService, "AppErrorSet", new object[] { curRobot.Data.LINEID, curRobot.CheckErrorList.Where(s => s.Value.Item3.Equals("0")) });
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            finally
            {
                curRobot.Context = null;
            }
        }


    }

}
