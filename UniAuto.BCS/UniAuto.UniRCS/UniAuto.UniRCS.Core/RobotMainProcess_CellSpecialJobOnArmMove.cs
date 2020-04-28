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

        //For Type II Cell Special Robot(1Arm2Job) Function List ================================================================================================================================

        /// <summary> for Robot Type II[ One Robot has 4 Arm(Fork),Arm#01(Upper Left),Arm#02(Lower Left),Arm#03(Upper Right),Arm#04(Lower Right) ,One Arm has 2 Job Position.Can GetGetPutPut
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotAllStageList"></param>
        private void CheckRobotControlCommand_For_TypeII_JobOnArmMove(Robot curRobot, List<RobotStage> curRobotAllStageList)
        {
            bool checkFlag = false;
            string strlog = string.Empty;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
            //List<Job> robotArmCanControlJobList_OrderBy = new List<Job>();
            //List<Job> robotStageCanControlJobList_OrderBy = new List<Job>();

            List<RobotCanControlSlotBlockInfo> robotArmCanControlSlotBlockInfoList = new List<RobotCanControlSlotBlockInfo>();
            List<RobotCanControlSlotBlockInfo> robotStageCanControlSlotBlockInfoList = new List<RobotCanControlSlotBlockInfo>();

            List<RobotCanControlSlotBlockInfo> robotArmCanControlSlotBlockInfoList_OrderBy = new List<RobotCanControlSlotBlockInfo>();
            List<RobotCanControlSlotBlockInfo> robotStageCanControlSlotBlockInfoList_OrderBy = new List<RobotCanControlSlotBlockInfo>();

            try
            {

                #region [ 1. Check Can Issue Command ]

                if (!CheckCanIssueRobotCommand(curRobot)) return;

                #endregion

                #region [ 2. Get Arm Can Control Job List, Stage Can Control Job List and Update StageInfo ]

                #region [ Clear All Stage UDRQ And LDRQ Stage SlotNoList Info ]

                foreach (RobotStage stageItem in curRobotAllStageList)
                {
                    lock (stageItem)
                    {
                        stageItem.CassetteStartTime = DateTime.MinValue;
                        stageItem.UnloaderSamplingFlag = RobotStage.UNLOADER_SAMPLING_FLAG.UNKOWN;
                        stageItem.curLDRQ_EmptySlotList.Clear();
                        stageItem.curUDRQ_SlotList.Clear();

                        //20151223 add Clear
                        stageItem.curUDRQ_SlotBlockInfoList.Clear();
                        //20160112 add Clear
                        stageItem.curLDRQ_EmptySlotBlockInfoList.Clear();

                        //20160302 add for Array Only
                        stageItem.CurRecipeGroupNoList.Clear();
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
                        failMsg = string.Format("Robot({0}) can not get any Select Rule!", curRobot.Data.ROBOTNAME);

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

                ////Set 1st Job Command Info
                //cur1stJob_CommandInfo cur1StJobCommandInfo = new cur1stJob_CommandInfo();
                //curRobot.Context.AddParameter(eRobotContextParameter.Cur1stJob_CommandInfo, cur1StJobCommandInfo);
                //Set 1st Job Command Info
                cur1stSlotBlock_CommandInfo cur1StSlotBlockCommandInfo = new cur1stSlotBlock_CommandInfo();
                curRobot.Context.AddParameter(eRobotContextParameter.Cur1stSlotBlock_CommandInfo, cur1StSlotBlockCommandInfo);

                #region [ Initial Select Rule List RobotConText Info. 搭配針對Select Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] ===========

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurRobotAllStageListEntity, curRobotAllStageList);
                robotConText.AddParameter(eRobotContextParameter.ArmCanControlSlotBlockInfoList, robotArmCanControlSlotBlockInfoList);
                robotConText.AddParameter(eRobotContextParameter.StageCanControlSlotBlockInfoList, robotStageCanControlSlotBlockInfoList);

                #endregion =========================================================================================================================================================

                //此時Robot無法得知要跑哪種Route,所以只會有一筆
                foreach (string routeID in curRuleJobSelectList.Keys)
                {

                    #region [ 根據RuleJobSelect選出Can Control SlotBlockInfo List ]

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

                                    failMsg = string.Format("Robot({0}) object({1}) MethodName({2}) RtnCode({3})  RtnMsg({4}]!",
                                                            curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME, robotConText.GetReturnCode(),
                                                            robotConText.GetReturnMessage());

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

                    break; //只取Priority最優先的一筆

                }

                #endregion

                #region [ Get Arm Can Control Job List ] 20151223 mark 改為SlotBlockInfo

                //List<Job> robotArmCanControlJobList;

                //robotArmCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.ArmCanControlJobList];

                ////當沒有設定參數時會回傳NULL,需防呆
                //if (robotArmCanControlJobList == null)
                //{
                //    robotArmCanControlJobList = new List<Job>();
                //}

                #endregion

                #region [ Get Stage Can Control Job List ] 20151223 mark 改為SlotBlockInfo

                //List<Job> robotStageCanControlJobList;

                //robotStageCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.StageCanControlJobList];

                ////當沒有設定參數時會回傳NULL,需防呆
                //if (robotStageCanControlJobList == null)
                //{
                //    robotStageCanControlJobList = new List<Job>();
                //}

                #endregion

                #endregion

                #region [ 3. Update OPI Stage Display Info ]

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
                if (curRobot.File.curRobotRunMode == eRobot_RunMode.SEMI_MODE) return;
                #endregion

                #region [ 更新OPI畫面後 Check Can Control Job Exist ]
                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00006 ] 
                fail_ReasonCode = eRobot_CheckFail_Reason.GET_CAN_CONTROL_JOB_FAIL;

                if (robotArmCanControlSlotBlockInfoList.Count == 0 && robotStageCanControlSlotBlockInfoList.Count == 0) //都為0 沒有可以處理的SlotBlock, 結束這回合!!
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any can control SlotBlockInfo Job!(Please check 1.Robot Arm  would have Job 2.CST(Load or Both port,wait for process or In process) is Ready and Exist(JobEachCassetteSlotPosition and Job Existence) 3.Upstream EQP would SendOut Job)",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00006 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any can control SlotBlockInfo Job!(Please check 1.Robot Arm  would have Job 2.CST(Load or Both port,wait for process or In process) is Ready and Exist(JobEachCassetteSlotPosition and Job Existence) 3.Upstream EQP would SendOut Job)", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("RtnCode({0}) RtnMsg({1})", fail_ReasonCode, "Robot({0}) can not get any can control SlotBlockInfo Job!(Please check 1.Robot Arm  would have Job 2.CST(Load or Both port,wait for process or In process) is Ready and Exist(JobEachCassetteSlotPosition and Job Existence) 3.Upstream EQP would SendOut Job)",
                                                 curRobot.Data.ROBOTNAME);

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
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

                curRobot.Context.AddParameter(eRobotContextParameter.IsType2Flag, true); //代表是要走 TypeII 的逻辑!! 20160108-002-dd

                #region [ Handle Robot Arm Job List First ]

                if (robotArmCanControlSlotBlockInfoList.Count != 0)
                {

                    #region [ Robot Arm上有片的處理 ]

                    //取得Job所在Stage的Priority
                    UpdateSlotBlockInfoStagePriority(robotArmCanControlSlotBlockInfoList);

                    //排序 以Step越小, PortStatus In_Prcess為優先處理 .因都在Robot Arm上所以不需by Job Location StageID排序
                    //robotArmCanControlSlotBlockInfoList_OrderBy = robotArmCanControlSlotBlockInfoList.OrderByDescending(s => s.CurBlock_StepID).ThenBy(s => s.CurBlock_PortCstStatusPriority).ToList();
                    //20160629 CurBlock的StepID要從小到大,上下Arm都有片時,可以決定先到EQP或是到CST
                    //robotArmCanControlSlotBlockInfoList_OrderBy = robotArmCanControlSlotBlockInfoList.OrderBy(s => s.CurBlock_StepID).ThenBy(s => s.CurBlock_PortCstStatusPriority).ToList();

                    //判斷在Arm上的Job,如果目前== eRobotStageType.PORT,代表上次從port出來,所以排序stepID大的先,輪到先服務從EQP取到Arm上的Job
                    if (curRobot.MoveToArm == eRobotStageType.PORT)
                    {
                        robotArmCanControlSlotBlockInfoList_OrderBy = robotArmCanControlSlotBlockInfoList.OrderByDescending(s => s.CurBlock_StepID).ThenBy(s => s.CurBlock_PortCstStatusPriority).ToList();
                    }
                    else  //如果目前!= eRobotStageType.PORT,代表上次從EQP出來,所以排序stepID小的先,輪到先服務從port取到Arm上的Job
                    {
                        robotArmCanControlSlotBlockInfoList_OrderBy = robotArmCanControlSlotBlockInfoList.OrderBy(s => s.CurBlock_StepID).ThenBy(s => s.CurBlock_PortCstStatusPriority).ToList();
                    }
                    foreach (RobotCanControlSlotBlockInfo curSlotBlockInfo in robotArmCanControlSlotBlockInfoList_OrderBy)
                    {
                        if (CheckRobotArmSlotBlockInfoRouteCondition(curRobot, curSlotBlockInfo)) return; //True表示命令已產生則結束本次cycle
                    }

                    #region
                    //20160607
                    //if ((CheckDoubleArmIsNoExist(curRobot, 0) && CheckDoubleArmIsNoExist(curRobot, 2)) || (CheckDoubleArmIsNoExist(curRobot, 1) && CheckDoubleArmIsNoExist(curRobot, 3)))
                    if (CheckDoubleArmIsNoExist(curRobot, 0) || CheckDoubleArmIsNoExist(curRobot, 2) || CheckDoubleArmIsNoExist(curRobot, 1) || CheckDoubleArmIsNoExist(curRobot, 3))
                    {
                        //foreach (RobotCanControlSlotBlockInfo curSlotBlockInfo in robotArmCanControlSlotBlockInfoList_OrderBy)
                        //{
                            UpdateSlotBlockInfoStagePriority(robotStageCanControlSlotBlockInfoList);
                            robotStageCanControlSlotBlockInfoList_OrderBy = robotStageCanControlSlotBlockInfoList.OrderByDescending(s => s.CurBlock_Location_StagePriority).ThenBy(s => s.CurBlock_StepID).ThenBy(s => s.CurBlock_PortCstStatusPriority).ToList();

                            //if (curSlotBlockInfo.CurBlock_Location_StageType == eRobotStageType.STAGE || curSlotBlockInfo.CurBlock_Location_StageType == eRobotStageType.EQUIPMENT)
                            //{
                            //    List<RobotCanControlSlotBlockInfo> robotPortStageCanControlSlotBlockInfoList = robotStageCanControlSlotBlockInfoList_OrderBy.Where(block => block.CurBlock_Location_StageType == eRobotStageType.PORT).ToList();
                            //    foreach (RobotCanControlSlotBlockInfo curPortStageSlotBlockInfo in robotPortStageCanControlSlotBlockInfoList)
                            //    {
                            //        //20160607
                            //        if (CheckRobotStageSlotBlockInfoRouteCondition(curRobot, curPortStageSlotBlockInfo, robotPortStageCanControlSlotBlockInfoList.Count))
                            //        {
                            //            return; //True表示命令已產生則結束本次cycle
                            //        }
                            //    }
                            //}
                            //else
                            //{
                                List<RobotCanControlSlotBlockInfo> robotEQPStageCanControlSlotBlockInfoList = robotStageCanControlSlotBlockInfoList_OrderBy.Where(block => block.CurBlock_Location_StageType == eRobotStageType.STAGE || block.CurBlock_Location_StageType == eRobotStageType.EQUIPMENT).ToList();
                                foreach (RobotCanControlSlotBlockInfo curEQPStageSlotBlockInfo in robotEQPStageCanControlSlotBlockInfoList) //只帶片跑去EQP stage取片,不考慮帶片跑去port取片
                                {
                                    //if (CheckRobotStageSlotBlockInfoRouteCondition(curRobot, curEQPStageSlotBlockInfo, robotEQPStageCanControlSlotBlockInfoList.Count))
                                    if (CheckRobotStageSlotBlockInfoRouteCondition(curRobot, curEQPStageSlotBlockInfo))
                                    {
                                        return; //True表示命令已產生則結束本次cycle
                                    }
                                }
                            //}
                        //}

                    }
                    #endregion
                    #region [ 判斷是否上有1st Job Command尚未下命令 ]

                    //Cell Special Cmd相關
                    cur1stSlotBlock_CommandInfo curJudgeCommandInfo = (cur1stSlotBlock_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stSlotBlock_CommandInfo];

                    if (curJudgeCommandInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    {
                        //有1stJob Command 則下命令Send Robot Control Command                       
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, curJudgeCommandInfo.cur1stJob_Command });
                        return; //不管有沒有, 都直接return;
                    }

                    #endregion

                    #endregion
                }
                else
                {
                    #region [ Robot Arm上無片的處理 ]

                    UpdateSlotBlockInfoStagePriority(robotStageCanControlSlotBlockInfoList);

                    //先依Stage Priority排序越大越優先, 再依Step排序越小越優先, 最後依CurPortCstStatusPriority排序越小越優先
                    robotStageCanControlSlotBlockInfoList_OrderBy = robotStageCanControlSlotBlockInfoList.OrderByDescending(s => s.CurBlock_Location_StagePriority).ThenBy(s => s.CurBlock_StepID).ThenBy(s => s.CurBlock_PortCstStatusPriority).ToList();
                    //by Slot Block Info 來處理是否可收送片
                    foreach (RobotCanControlSlotBlockInfo curSlotBlockInfo in robotStageCanControlSlotBlockInfoList_OrderBy)
                    {
                        //20160629
                        //if (CheckRobotStageSlotBlockInfoRouteCondition(curRobot, curSlotBlockInfo, robotStageCanControlSlotBlockInfoList_OrderBy.Count))
                        if (CheckRobotStageSlotBlockInfoRouteCondition(curRobot, curSlotBlockInfo))
                        {
                            return; //True表示命令已產生則結束本次cycle
                        }
                    }

                    #region  [ 20160119 Cell Special 不支援RTC ]

                    //List<Job> _tempRobotStageCanControlJobList = new List<Job>();
                    //_tempRobotStageCanControlJobList.Clear();
                    //foreach (Job curRobotStageJob in robotStageCanControlJobList_OrderBy)
                    //{
                    //    if (curRobotStageJob.RobotWIP.RTCReworkFlag && !_tempRobotStageCanControlJobList.Contains(curRobotStageJob)) //有做过RTC的基板, 先不处理, 优先处理正常为出片的基板!!
                    //    {
                    //        _tempRobotStageCanControlJobList.Add(curRobotStageJob);
                    //        continue;
                    //    }
                    //    if (CheckRobotStageJobRouteCondition_ForGetGetPutPut(curRobot, curRobotStageJob)) return; //True表示命令已產生則結束本次cycle
                    //}
                    //if (_tempRobotStageCanControlJobList.Count > 0)
                    //{
                    //    _tempRobotStageCanControlJobList = _tempRobotStageCanControlJobList.OrderBy(s => s.RobotWIP.PreFetchFlag).ToList();
                    //    foreach (Job curRobotStageJob in _tempRobotStageCanControlJobList)
                    //    {
                    //        if (CheckRobotStageJobRouteCondition_ForGetGetPutPut(curRobot, curRobotStageJob)) return; //True表示命令已產生則結束本次cycle
                    //    }
                    //}

                    #endregion

                    #region [ 判斷是否上有1st Job Command尚未下命令 ]

                    //Cell Special Cmd相關
                    cur1stSlotBlock_CommandInfo curJudgeCommandInfo = (cur1stSlotBlock_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stSlotBlock_CommandInfo];

                    if (curJudgeCommandInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    {
                        //有1stJob Command 則下命令Send Robot Control Command                       
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, curJudgeCommandInfo.cur1stJob_Command });
                        return; //不管有沒有, 都直接return;
                    }

                    #endregion

                    #region [ 20160119 add 上述都没有可以跑的基板, 正常的部分check完成后, 接下来就要去判断...有没有要预取的基板 (前提条件要开启 预取 功能!) ]

                    if ((bool)Invoke("RobotSpecialService", "Check_PreFetch_DelayTime_For1Arm1Job", new object[] { robotConText })) //第一次或是超过delay time就可以考虑预取!!
                    {
                        //上述都没有可以跑的基板, 正常的部分check完成后, 接下来就要去判断...有没有要预取的基板 (前提条件要开启 预取 功能!) Cell Special 沒有RTC 不須判斷RTC相關邏輯
                        //虽然有开启 预取 功能, 但是仍然需要去判断其他的项目是不是有启动!! 如果有启动, 则视同 预取 没作动!! 2015-12-26
                        //robotStageCanControlSlotBlockInfoList_OrderBy = robotStageCanControlSlotBlockInfoList.Where(s => s.CurBlock_Location_StageType == eStageType.PORT).ToList();
                        //20160629 CurPortCstStatusPriority要判斷,愈小的排前面,Inprocess > Waitforprocess
                        robotStageCanControlSlotBlockInfoList_OrderBy = robotStageCanControlSlotBlockInfoList.Where(s => s.CurBlock_Location_StageType == eStageType.PORT).OrderBy(s => s.CurBlock_PortCstStatusPriority).ToList();
                        if (robotStageCanControlSlotBlockInfoList_OrderBy.Count() > 0)
                        {

                            RobotStage _curStage = null;

                            foreach (RobotCanControlSlotBlockInfo curSlotBlockInfo in robotStageCanControlSlotBlockInfoList_OrderBy)
                            {
                                _curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curSlotBlockInfo.CurBlock_Location_StageID);

                                if (!CheckPrefetchFlag(curRobot, _curStage)) continue; //如果為false, 就是沒有開啟Pre-Fetch功能!

                                //Cell Special 沒有特殊PreFetch條件
                                //_runPrefetchFlag = (bool)Invoke("RobotSpecialService", "Check_Stage_Prefetch_SpecialCondition_For1Arm1Job", new object[] { robotConText, _curStage, curSlotBlockInfo }); //判断是不是要跑 预取 功能!!
                                //if (!_runPrefetchFlag) continue; //如果为true, 才是真的要做 预取 功能!!
                                if (Workbench.LineType != eLineType.CELL.CCCRP && Workbench.LineType != eLineType.CELL.CCCRP_2) //除了CRP以外,RWT、SOR、CHN都可以預取2個block,EQP有4個stage位置
                                {
                                    if (CheckRobotStageSlotBlockInfoRouteCondition(curRobot, curSlotBlockInfo, robotStageCanControlSlotBlockInfoList_OrderBy.Count))
                                    {
                                        return; //True表示命令已產生則結束本次cycle
                                    }
                                }
                                else //CRP只能預取1個block,EQP只有2個stage位置
                                {
                                    if (CheckRobotStageSlotBlockInfoRouteCondition(curRobot, curSlotBlockInfo))
                                    {
                                        return; //True表示命令已產生則結束本次cycle
                                    }
                                }

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


        /// <summary> Check Stage上SlotBlok內的所有Job目前Step的所有Route Condition是否成立(最多4片2支Fork同時成立)
        /// 為了PreFetch 可以抽2個block,避免抽1個block就下Command,新增此method
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotStageSlotBlockInfo"></param>
        /// <returns></returns>
        private bool CheckRobotStageSlotBlockInfoRouteCondition(Robot curRobot, RobotCanControlSlotBlockInfo curRobotStageSlotBlockInfo, int robotStageCanControlSlotBlockCount)
        {
            string strlog = string.Empty;
            List<RobotStage> curFilterStageList = new List<RobotStage>();
            CellSpecialRobotCmdInfo curRobotCommand = new CellSpecialRobotCmdInfo();

            //20151229 modify by SlotBlockInfo 來處理

            try
            {

                #region [ Check CurStep RouteStepByPass Condition and 準備變更curStep ]

                if (!CheckSlotBlockInfo_AllRouteStepByPassConditionByStepNo(curRobot, curRobotStageSlotBlockInfo, curRobotStageSlotBlockInfo.CurBlock_StepID, ref curFilterStageList))
                {
                    return false; //StepByPass條件有問題則回覆NG
                }

                #endregion

                #region [ Check CurStep RouteStepJump Condition and 準備變更curStep ]

                if (!CheckSlotBlockInfo_AllRouteStepJumpConditionByStepNo(curRobot, curRobotStageSlotBlockInfo, curRobotStageSlotBlockInfo.CurBlock_StepID, ref curFilterStageList))
                {
                    return false; //StepJump條件有問題則回覆NG
                }

                #endregion

                #region [ 20160118 add Judge after ByPass and Jump curStep and NextStep 如果Route不一樣則視為有問題記Log回覆目前SlotBlockInfo NG .2ndflag因為是ByJob by Pass Jump所以Filter後還要再比對 ]

                if (CheckSlotBlockInfoJobRouteCondition(curRobot, curRobotStageSlotBlockInfo, "CheckRobotStageSlotBlockInfoRouteCondition") == false)
                {
                    return false; //BlockInfo內Front/Back Job Route有問題則回覆NG
                }

                #endregion

                #region [ *****Check 2nd Job Command 1st Action & TargetPosition Rule by 1stJob Command Info ]

                //取得 1st SlotBlock 的 Command 信息!
                cur1stSlotBlock_CommandInfo cur1stSlotBlockCmdInfo = (cur1stSlotBlock_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stSlotBlock_CommandInfo];

                if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE) //代表有命令需要處理!!
                {

                    #region [ 1st Job 2nd Command Action 必須是PUT to Stage. 如果是2nd Command不是Put則不需考慮2nd Job Command ]
                   
                    if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd02_Command != eRobot_Trx_CommandAction.ACTION_PUT)
                    {
                        //201060607
                        if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET && cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.PREFETCHFLAG == "Y")
                        {

                        }
                        else
                        {
                            //直接下命令並回true不需考慮2nd Job
                            bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                            return true;
                        }
                    }

                    #endregion

                    #region [ 1st Job 1st Command Action 必須要是Get or Multi-Get並決定1stJob會占用哪支Arm. 如果是1st Command不是Get or Multi-Get則不需考慮2nd Job Command ]

                    //1st Job Command 1st Action必須是Get from Stage.且Use Arm必須要是單Arm如果是1st Command 不是Get or Multi-Get則不需考慮2nd Job Command
                    //Cell Special Arm Select
                    //SPEC定義
                    //0: None
                    //1: Upper/Left Arm   //2: Lower/Left Arm   //3: Left Both Arm                                   
                    //4: Upper/Right Arm  //8: Lower/Right Arm  //12: Right Both Arm
                    //20160127 add 5 and 10
                    //5: Upper Both Arm 
                    //10: Lower Both Arm
                    if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_GET &&
                        cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_MULTI_GET &&
                        cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.UP_LEFT &&
                        cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.LOW_LEFT &&
                        cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.UP_RIGHT &&
                        cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.LOW_RIGHT)
                    {

                        //直接下命令並回true不需考慮2nd Job
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                        return true;
                    }

                    lock (curRobot)
                    {
                        //將1st Job Command 1st Command Action會用到的Arm Front/Back先預約起來
                        if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect == 1 || cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect == 2)
                        {
                            curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 1].ArmFrontJobExist = eGlassExist.Exist;
                            curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 1].ArmBackJobExist = eGlassExist.Exist;
                        }
                        else if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect == 4)
                        {
                            curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 2].ArmFrontJobExist = eGlassExist.Exist;
                            curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 2].ArmBackJobExist = eGlassExist.Exist;
                        }
                        else if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect == 8)
                        {
                            curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 5].ArmFrontJobExist = eGlassExist.Exist;
                            curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 5].ArmBackJobExist = eGlassExist.Exist;
                        }
                    }

                    #endregion

                    #region [ Update 1stJob 2nd Command Target StageID Empty Slotlist Info by Target SlotNo ]

                    if (cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.ContainsKey(cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd02_TargetSlotNo) == true)
                    {
                        //根據1st Job 2nd Command Target Position/SlotNo 將Target Position的EmptySlotNo預約起來(排除引用)
                        cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.Remove(cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd02_TargetSlotNo);
                    }

                    if (cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.Count == 0)
                    {
                        //20151110 add 如果1st Job NextStep設定可以CrossStage(1stJob的Target點)那還是要可以處理 
                        if (cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_CrossStageFlag != "Y")
                        {
                            //20160607
                            if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET && cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.PREFETCHFLAG == "Y"
                                && robotStageCanControlSlotBlockCount > 1)
                            {
                                //不要馬上sendCmd,還要再判斷第2片Job
                            }
                            else
                            {
                                //1st Job Command 之後就沒有空的Slot則不需要2ndJob Command
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job 2nd Command TargetStageID({3})!",
                                                        curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                //直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                                return true;
                            }
                        }

                    }

                    #endregion

                    #region [ Get 2nd SlotBlock Current Step Entity ]

                    Job curRobotStageJob = null;
                    RobotRouteStep cur2ndBlockJob_curRouteStep = null;

                    if (curRobotStageSlotBlockInfo.CurBlockCanControlJobList.Count > 0)
                    {
                        curRobotStageJob = curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0];

                        if (curRobotStageJob.RobotWIP.RobotRouteStepList.ContainsKey(curRobotStageJob.RobotWIP.CurStepNo) == true)
                        {
                            cur2ndBlockJob_curRouteStep = curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.CurStepNo];
                        }

                    }

                    //找不到 CurStep Route 回NG
                    if (cur2ndBlockJob_curRouteStep == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                    curRobotStageJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    #endregion

                    #region [ Check 2ndJob_1stCommand Can Use StageList by 1stJob_1stCommand TargetStageEntity ]

                    //By Pass & Jump之後會取得2ndJob目前Step的所有StageList. 判斷1stJob的 Source Stage 是否有在此範圍內.
                    if (curFilterStageList.Contains(cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity) == false)
                    {
                        //2nd Job目前Use StageList不和1stJob 1st Command TargetPosition 不相同 則不需要再考慮這一片
                        //因為是當下決定 所以直接記Log 可考慮不需要Debug
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) 1st TargetPodition({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) curStepNo({7}) StageList({8}) is different!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontCSTSeq.ToString(), cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobSeq.ToString(),
                                                cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo, curRobotStageJob.RobotWIP.CurStepNo.ToString(),
                                                cur2ndBlockJob_curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        return false;

                    }

                    #endregion

                    #region [ 判斷是否允許同Step跨Stage取片 ]

                    bool getJobFromAnotherStageFlag = false;

                    //比對1st Job 與2nd Job 是否可以Cross Stage 取片
                    if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_CrossStageFlag == "Y" &&
                        cur2ndBlockJob_curRouteStep.Data.CROSSSTAGEFLAG == "Y")
                    {
                        getJobFromAnotherStageFlag = true;
                    }

                    if (getJobFromAnotherStageFlag == false)
                    {
                        //目前2ndJob 所在的位置與1st Job 1st Command Target Stage不同且不允許跨Stage則不可一起出片
                        if ((curRobotStageJob.RobotWIP.CurLocation_StageID != cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID))
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) 1st TargetPodition({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) curKStepNo({7}) curLocation_StageID({8}) is different!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontCSTSeq.ToString(), cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobSeq.ToString(),
                                                    cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo, curRobotStageJob.RobotWIP.CurStepNo.ToString(),
                                                    curRobotStageJob.RobotWIP.CurLocation_StageID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return false;

                        }
                    }

                    //允許跨Stage取片則不需比對Job1 and Job2 Location StageID是否相同

                    #endregion

                    #region [ Check 2ndJob_1stCommand Action by 1stJob_1stCommand ActionCode ]

                    //Mulit-PutGet 在DB無法預設所以要有Temp轉換
                    string tmpAction = string.Empty;

                    if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_DBActionCode == eRobot_DB_CommandAction.ACTION_MULTI_GET)
                    {
                        tmpAction = eRobot_DB_CommandAction.ACTION_GET;
                    }
                    else if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_DBActionCode == eRobot_DB_CommandAction.ACTION_MULTI_PUT)
                    {
                        tmpAction = eRobot_DB_CommandAction.ACTION_PUT;
                    }
                    else
                    {
                        tmpAction = cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_DBActionCode;
                    }

                    if (cur2ndBlockJob_curRouteStep.Data.ROBOTACTION != tmpAction)
                    {
                        //2nd Job目前Step Action不和1stJob 1st Command Action 不相同 則不需要再考慮這一片
                        //因為是當下決定 所以直接記Log 可考慮不需要Debug
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) Action({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) curStepNo({7}) Action({8}) is different!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobSeq.ToString(), cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobSeq.ToString(),
                                                cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_DBActionCode, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo, curRobotStageJob.RobotWIP.CurStepNo.ToString(),
                                                cur2ndBlockJob_curRouteStep.Data.ROBOTACTION);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        return false;

                    }

                    #endregion

                }

                #endregion

                //開始Filter ============================================================================================================================================================

                DefineNormalRobotCmd cur1stDefindCmd = new DefineNormalRobotCmd();
                DefineNormalRobotCmd cur2ndDefindCmd = new DefineNormalRobotCmd();

                #region [ Check CurStep All Filter Condition . 成功的話會取得Cmd Action(Get,Put or..) UseArm(Upper, Lower....) ]

                if (!CheckSlotBlockInfo_AllFilterConditionByStepNo(curRobot, curRobotStageSlotBlockInfo, curRobotStageSlotBlockInfo.CurBlock_StepID, cur1stDefindCmd, cur2ndDefindCmd, ref curFilterStageList))
                {                  
                    return false; //Filter條件有問題則回覆NG
                }

                #endregion

                #region [ 20160118 add Judge after ByPass and Jump curStep and NextStep 如果Route不一樣則視為有問題記Log回覆目前SlotBlockInfo NG .2ndflag因為是ByJob by Pass Jump所以Filter後還要再比對 ]

                if (CheckSlotBlockInfoJobRouteCondition(curRobot, curRobotStageSlotBlockInfo, "CheckRobotStageSlotBlockInfoRouteCondition") == false)
                {
                    return false; //BlockInfo內Front/Back Job Route有問題則回覆NG
                }

                #endregion

                #region [ ***** Check 2nd Job Command 1st Action & TargetPosition Rule ]

                if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                {
                    Job curRobotStageJob = null;
                    RobotRouteStep cur2ndBlockJob_nextRouteStep = null;

                    if (curRobotStageSlotBlockInfo.CurBlockCanControlJobList.Count > 0)
                    {
                        curRobotStageJob = curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0];

                        if (curRobotStageJob.RobotWIP.RobotRouteStepList.ContainsKey(curRobotStageJob.RobotWIP.NextStepNo) == true)
                        {
                            cur2ndBlockJob_nextRouteStep = curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.NextStepNo];
                        }
                    }

                    //找不到 CurStep Route 回NG
                    if (cur2ndBlockJob_nextRouteStep == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get nextRouteStep({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                    curRobotStageJob.RobotWIP.NextStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    #region [ Check 2ndJob_1stCommand Can Use StageList by 1stJob_1stCommand TargetStageEntity ]

                    //預設不允許同Step跨Stage放片
                    bool putJobtoAnotherStageFlag = false;

                    //比對1st Job 與2nd Job 是否可以Cross
                    if (cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_CrossStageFlag == "Y" &&
                        cur2ndBlockJob_nextRouteStep.Data.CROSSSTAGEFLAG == "Y")
                    {
                        putJobtoAnotherStageFlag = true;
                    }

                    //如果2nd Job 1st Command 是PUT相關則表示 curFilterStageList是給2nd Job 1st Cmd用=>在Arm上有Job Function使用
                    //如果2nd Job 2nd Command 是PUT相關則表示 curFilterStageList是給2nd Job 2nd Cmd用=>在此使用
                    if (curFilterStageList.Contains(cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity) == false)
                    {
                        //2ndJob的Target目標不包含1stJob的Target點時的處理
                        #region [ 判斷是否允許同Step跨Stage放片,允許的話則不更新curFilterStageList ]

                        if (putJobtoAnotherStageFlag == false)
                        {

                            //2nd Job 2nd Command目前Use StageList不和1stJob 2nd Command TargetPosition 不相同 則不需要再考慮這一片
                            //因為是當下決定 所以直接記Log 可考慮不需要Debug
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) 2nd TargetPodition({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) nextStepNo({7}) StageList({8}) is different!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontCSTSeq.ToString(), cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobSeq.ToString(),
                                                    cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo, curRobotStageJob.RobotWIP.NextStepNo.ToString(),
                                                    cur2ndBlockJob_nextRouteStep.Data.STAGEIDLIST);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return false;
                        }

                        #endregion

                        //如果允許同Step跨Stage放片則維持原先的curFilterStageList
                    }
                    else
                    {
                        //2ndJob的Target目標包含1stJob的Target點時的處理

                        //20151110 add當2ndJob 2nd Cmd可去Stage包含1stJob的2nd Cmd Target Stage 且1stJob 2nd Cmd Target Stage不能再放片時,2nd Job的Target點要排除掉沒有空Slot的Stage
                        if (cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.Count == 0)
                        {
                            //20160607
                            if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET && cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.PREFETCHFLAG == "Y"
                                && robotStageCanControlSlotBlockCount > 1)
                            {
                                //不要馬上sendCmd,還要再判斷第2片Job
                            }
                            else
                            {

                                curFilterStageList.Remove(cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity);

                                if (curFilterStageList.Count == 0)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) 2nd TargetPodition({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) nextStepNo({7}) StageList({8}) can not Receive!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontCSTSeq.ToString(), cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobSeq.ToString(),
                                                        cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo, curRobotStageJob.RobotWIP.NextStepNo.ToString(),
                                                        cur2ndBlockJob_nextRouteStep.Data.STAGEIDLIST);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    return false;
                                }
                            }
                        }
                        else
                        {
                            //1stJob目的Stage還能收片則強制2nd Job只能進同樣Stage
                            curFilterStageList.Clear();
                            curFilterStageList.Add(cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity);
                        }
                    }

                    #endregion

                }

                #endregion

                #region [ Check All OrderBy Condition and define Target Position and SlotNo ]

                //Check 1st Cmd is Exist
                if (cur1stDefindCmd.Cmd01_Command == 0)
                {
                    //沒有1st Command 則記Error 離開
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        string job_str = eRobot_SlotBlock_JobsExistStatus.FRONT_BACK_EMPTY, robot_action = string.Empty, stageid_list = string.Empty;
                        switch (curRobotStageSlotBlockInfo.CurBlock_JobExistStatus)
                        {
                            case eRobot_SlotBlock_JobsExistStatus.FRONT_BACK_EXIST:
                                {
                                    Job front_job = curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0];
                                    job_str = string.Format("FrontJob({0}) BackJob({1})", curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0].JobKey, curRobotStageSlotBlockInfo.CurBlockCanControlJobList[1].JobKey);
                                    robot_action = front_job.RobotWIP.RobotRouteStepList[front_job.RobotWIP.CurStepNo].Data.ROBOTACTION;
                                    stageid_list = front_job.RobotWIP.RobotRouteStepList[front_job.RobotWIP.CurStepNo].Data.STAGEIDLIST;
                                }
                                break;
                            case eRobot_SlotBlock_JobsExistStatus.FRONT_EMPTY_BACK_EXIST:
                                {
                                    Job back_job = curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0];
                                    job_str = string.Format("BackJob({0})", curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0].JobKey);
                                    robot_action = back_job.RobotWIP.RobotRouteStepList[back_job.RobotWIP.CurStepNo].Data.ROBOTACTION;
                                    stageid_list = back_job.RobotWIP.RobotRouteStepList[back_job.RobotWIP.CurStepNo].Data.STAGEIDLIST;
                                }
                                break;
                            case eRobot_SlotBlock_JobsExistStatus.FRONT_EXIST_BACK_EMPTY:
                                {
                                    Job front_job = curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0];
                                    job_str = string.Format("FrontJob({0})", curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0].JobKey);
                                    robot_action = front_job.RobotWIP.RobotRouteStepList[front_job.RobotWIP.CurStepNo].Data.ROBOTACTION;
                                    stageid_list = front_job.RobotWIP.RobotRouteStepList[front_job.RobotWIP.CurStepNo].Data.STAGEIDLIST;
                                }
                                break;
                        }
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobExistStatus({2}, {3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) Command Action({8}) is illegal",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageSlotBlockInfo.CurBlock_JobExistStatus, job_str,
                                                curRobotStageSlotBlockInfo.CurBlock_Location_StageID, curRobotStageSlotBlockInfo.CurBlock_StepID.ToString(),
                                                robot_action,
                                                stageid_list,
                                                cur1stDefindCmd.Cmd01_Command.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }

                //Order 1st & 2nd Cmd
                if (CheckSlotBlockInfo_AllOrderByConditionByCommand(curRobot, curRobotStageSlotBlockInfo, cur1stDefindCmd, cur2ndDefindCmd, curFilterStageList) == false)
                {
                    //20151026 add Order By後Cmd 的TargetPosition or SlotNo有問題則回覆NG
                    return false;
                }

                #endregion

                #region [ by RobotArm Qty Create Command ]

                if (curRobot.Data.ARMJOBQTY == 2)
                {

                    #region [ 20160118 add Check First SlotBlockInfo Glass Command ]

                    //Check 1st Command
                    if (!PortFetchOut_FirstSlotBlockGlassCheck(curRobot, curRobotStageSlotBlockInfo, cur1stDefindCmd, false))
                    {
                        return false;
                    }

                    //Check 2nd Command
                    if (!PortFetchOut_FirstSlotBlockGlassCheck(curRobot, curRobotStageSlotBlockInfo, cur2ndDefindCmd, true))
                    {
                        return false;
                    }

                    #endregion

                    #region [ 20160118 Cell Special不支援交換片所以不需確認 Check GET/PUT Condition ]
                    ////GlobalAssemblyVersion v1.0.0.26-20151102
                    //CheckGetPutCommandCondition(curRobot, curRobotStageJob, cur1stDefindCmd); //Check 1st Command
                    //CheckGetPutCommandCondition(curRobot, curRobotStageJob, cur2ndDefindCmd, true); //Check 2nd Command
                    #endregion

                    #region [ 20160118 add Check PUTREADY Condition ]

                    //GlobalAssemblyVersion v1.0.0.26-20151109
                    CheckSlotBlockInfoPutReadyCommandCondition(curRobot, curRobotStageSlotBlockInfo, cur1stDefindCmd, false); //Check 1st Command
                    CheckSlotBlockInfoPutReadyCommandCondition(curRobot, curRobotStageSlotBlockInfo, cur2ndDefindCmd, true); //Check 2nd Command

                    #endregion

                    #region [ 20160118  Cell Special 不支援Load Port回插功能所以不須 Check RTCPUT Condition ]

                    //GlobalAssemblyVersion v1.0.0.9-20151230
                    //CheckRtcPutCommandCondition(curRobot, curRobotStageJob, cur1stDefindCmd); //Check 1st Command
                    //CheckRtcPutCommandCondition(curRobot, curRobotStageJob, cur2ndDefindCmd, true); //Check 2nd Command

                    #endregion

                    #region [ Create 1 Arm 2 Substrate Robot Command ] //20160118 以目前Block Front/Back資訊來補完資訊.不使用1st 2nd defineCommand內的CstSeq與JobSeq

                    int int1st_FrontCstSeqNo = 0;
                    int int1st_FrontJobSeqNo = 0;
                    int int1st_BackCstSeqNo = 0;
                    int int1st_BackJobSeqNo = 0;
                    int int2nd_FrontCstSeqNo = 0;
                    int int2nd_FrontJobSeqNo = 0;
                    int int2nd_BackCstSeqNo = 0;
                    int int2nd_BackJobSeqNo = 0;

                    #region [ Get Front/Back Job ]

                    foreach (Job job in curRobotStageSlotBlockInfo.CurBlockCanControlJobList)
                    {
                        switch (job.RobotWIP.CurSubLocation)
                        {

                            case eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION:

                                int.TryParse(job.CassetteSequenceNo, out int1st_FrontCstSeqNo);
                                int.TryParse(job.JobSequenceNo, out int1st_FrontJobSeqNo);
                                int.TryParse(job.CassetteSequenceNo, out int2nd_FrontCstSeqNo);
                                int.TryParse(job.JobSequenceNo, out int2nd_FrontJobSeqNo);
                                break;

                            case eRobotCommonConst.ROBOT_ARM_BACK_LOCATION:

                                int.TryParse(job.CassetteSequenceNo, out int1st_BackCstSeqNo);
                                int.TryParse(job.JobSequenceNo, out int1st_BackJobSeqNo);
                                int.TryParse(job.CassetteSequenceNo, out int2nd_BackCstSeqNo);
                                int.TryParse(job.JobSequenceNo, out int2nd_BackJobSeqNo);
                                break;

                        }
                    }

                    #endregion

                    curRobotCommand.Cmd01_Command = cur1stDefindCmd.Cmd01_Command;
                    curRobotCommand.Cmd01_ArmSelect = cur1stDefindCmd.Cmd01_ArmSelect;
                    curRobotCommand.Cmd01_TargetPosition = cur1stDefindCmd.Cmd01_TargetPosition;
                    curRobotCommand.Cmd01_TargetSlotNo = cur1stDefindCmd.Cmd01_TargetSlotNo;
                    curRobotCommand.Cmd01_FrontCSTSeq = int1st_FrontCstSeqNo;
                    curRobotCommand.Cmd01_FrontJobSeq = int1st_FrontJobSeqNo;
                    curRobotCommand.Cmd01_BackCSTSeq = int1st_BackCstSeqNo;
                    curRobotCommand.Cmd01_BackJobSeq = int1st_BackJobSeqNo;

                    curRobotCommand.Cmd02_Command = cur2ndDefindCmd.Cmd01_Command;
                    curRobotCommand.Cmd02_ArmSelect = cur2ndDefindCmd.Cmd01_ArmSelect;
                    curRobotCommand.Cmd02_TargetPosition = cur2ndDefindCmd.Cmd01_TargetPosition;
                    curRobotCommand.Cmd02_TargetSlotNo = cur2ndDefindCmd.Cmd01_TargetSlotNo;
                    curRobotCommand.Cmd02_FrontCSTSeq = int2nd_FrontCstSeqNo;
                    curRobotCommand.Cmd02_FrontJobSeq = int2nd_FrontJobSeqNo;
                    curRobotCommand.Cmd02_BackCSTSeq = int2nd_BackCstSeqNo;
                    curRobotCommand.Cmd02_BackJobSeq = int2nd_BackJobSeqNo;

                    #endregion

                    #region [ set cur 1st Job CommandInfo ]

                    if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_NONE)
                    {

                        #region [ Set 1stJob Command Info ]

                        cur1stSlotBlockCmdInfo.cur1stJob_Command = curRobotCommand;
                        cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_DBActionCode = GetRobotCommandActionDesc(curRobotCommand.Cmd01_Command);
                        cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_DBActionCode = GetRobotCommandActionDesc(curRobotCommand.Cmd02_Command);

                        if (curRobotCommand.Cmd01_TargetPosition != 0)
                        {
                            RobotStage cur1stCmdStageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotCommand.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));
                            cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity = cur1stCmdStageEntity;
                        }

                        if (curRobotCommand.Cmd02_TargetPosition != 0)
                        {
                            RobotStage cur2ndCmdStageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotCommand.Cmd02_TargetPosition.ToString().PadLeft(2, '0'));
                            cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity = cur2ndCmdStageEntity;
                        }

                        #region [ 1st Job 1st Command Action 必須要是Get or Multi-Get並決定1stJob會占用哪支Arm. 如果是1stJob 1st Command不是Get or Multi-Get則不需考慮2nd Job Command ]

                        //1st Job Command 1st Action必須是Get from Stage.且Use Arm必須要是1 or 2如果是1st Command 不是Get or Multi-Get則不需考慮2nd Job Command
                        if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_GET &&
                            cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_MULTI_GET &&
                            cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.UP_LEFT &&
                            cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.LOW_LEFT &&
                            cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.UP_RIGHT &&
                            cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.LOW_RIGHT)
                        {

                            //直接下命令並回true不需考慮2nd Job
                            bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                            return true;
                        }

                        lock (curRobot)
                        {
                            //將1st Job Command 1st Command Action會用到的Arm Front/Back先預約起來
                            if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect == 1 || cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect == 2)
                            {
                                curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 1].ArmFrontJobExist = eGlassExist.Exist;
                                curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 1].ArmBackJobExist = eGlassExist.Exist;
                            }
                            else if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect == 4) //目前只有使用到上Arm,下Arm暫時沒用到,所以上Arm JobExist,則下Arm一併判為 JobExist,避免使用到下Arm
                            {
                                curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 2].ArmFrontJobExist = eGlassExist.Exist;
                                curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 2].ArmBackJobExist = eGlassExist.Exist;
                            }
                            else if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect == 8)
                            {
                                curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 5].ArmFrontJobExist = eGlassExist.Exist;
                                curRobot.CurTempArmDoubleJobInfoList[cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 5].ArmBackJobExist = eGlassExist.Exist;
                            }
                        }

                        #endregion

                        #region [ Get 1stJob CurStep Entity and UpDate 1stJob 1stCmd CrossStageFlag ]

                        //Stage Job Must Check CurStepInfo and NextStepInfo
                        Job curRobotStageJob = null;
                        RobotRouteStep cur2ndBlockJob_curRouteStep = null;

                        try
                        {
                            curRobotStageJob = curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0];
                        }
                        catch (Exception ex)
                        {
                            //防呆

                        }

                        RobotRouteStep cur1stJob_CurRouteStepInfo = null;

                        if (curRobotStageJob.RobotWIP.RobotRouteStepList.ContainsKey(curRobotStageJob.RobotWIP.CurStepNo) == true)
                        {
                            cur1stJob_CurRouteStepInfo = curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.CurStepNo];
                        }

                        //找不到 CurStep Route 回NG
                        if (cur1stJob_CurRouteStepInfo == null)
                        {

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                        curRobotStageJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_CrossStageFlag = cur1stJob_CurRouteStepInfo.Data.CROSSSTAGEFLAG;

                        #endregion

                        #region [ Get 1stJob NextStep Entity and UpDate 1stJob 2ndCmd CrossStageFlag ]

                        //Stage Job Must Check CurStepInfo and NextStepInfo
                        RobotRouteStep cur1stJob_NextRouteStepInfo = null;

                        if (curRobotStageJob.RobotWIP.RobotRouteStepList.ContainsKey(curRobotStageJob.RobotWIP.NextStepNo) == true)
                        {
                            cur1stJob_NextRouteStepInfo = curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.NextStepNo];
                        }

                        //找不到 CurStep Route 回NG
                        if (cur1stJob_NextRouteStepInfo == null)
                        {

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get NextRouteStep({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                        curRobotStageJob.RobotWIP.NextStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_CrossStageFlag = cur1stJob_NextRouteStepInfo.Data.CROSSSTAGEFLAG;

                        #endregion

                        #region [ Update 1stJob 2nd Command Target StageID Empty Slotlist Info by Target SlotNo ]

                        if (cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.ContainsKey(cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd02_TargetSlotNo) == true)
                        {
                            //根據1st Job  2nd Command Target Position/SlotNo 將Target Position的EmptySlotNo預約起來(排除引用)
                            cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.Remove(cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd02_TargetSlotNo);
                        }

                        if (cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.Count == 0)
                        {
                            //20151110 add 如果1stJob NextStep設定可以CrossStage 那還是要可以處理   
                            if (cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_CrossStageFlag != "Y")
                            {
                                //20160607
                                if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET && cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.PREFETCHFLAG == "Y"
                                    && robotStageCanControlSlotBlockCount > 1)
                                {
                                    //不要馬上sendCmd,還要再判斷第2片Job
                                }
                                else
                                {
                                    //1st Job Command 之後就沒有空的Slot則不需要2ndJob Command
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job 2nd Command TargetStageID({3})!",
                                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    //直接下命令並回true不需考慮2nd Job
                                    bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                                    return true;
                                }

                            }
                        }

                        #endregion

                        #region [ 20160219 add for 如果Target Stage為Port Type且屬性是會根據收到第一片之後才會變化的則不判斷第二片Job馬上執行命令. EX:PortMode= EMP , PortGrade= EM ]

                        //1stSlotBlockCmdInfo 1stCmd在此時(Job On Stage)不可能是PUT所以只要判斷1stJob 2ndCmd是否為Put
                        if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd02_Command == eRobot_Trx_CommandAction.ACTION_PUT &&
                           cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGETYPE == eRobotStageType.PORT)
                        {

                            #region [ Get Port Entity by 1stSlotBlockCmdInfo 2ndCmd Target Stage ]

                            Port curTargetPort = ObjectManager.PortManager.GetPort(cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                            if (curTargetPort == null)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Target Port Entity By StageID({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                        cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                //找不到Port Entity則直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                                return true;
                            }

                            #endregion

                            //20160219 add 如果Target Port是EMP Mode or EMP Grade且是空CST則馬上下命令不需要等候第二組
                            if ((curTargetPort.File.Mode == ePortMode.EMPMode ||
                                 cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.UnloaderPortMatchSetGradePriority == eCellUnloaderDispatchRuleMatchGradePriority.IS_EMP_PRIORITY)
                                && cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.PortCassetteEmpty == RobotStage.PORTCSTEMPTY.EMPTY)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Target StageID({4}) Port Mode is ({5}) or Unloader DispatchRule Grade is (EM) and No Check 2ndJob Condition!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                        cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID, curTargetPort.File.Mode.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                //目的Port PortMode為EMP(會根據第一片變動影響後續配片邏輯)則直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                                return true;

                            }

                        }

                        //20160301 add for CCRWT Sort Mode Use Only
                        if (CheckIsCellRWT_SortMode(curRobot) == true && cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.PortCassetteEmpty == RobotStage.PORTCSTEMPTY.EMPTY)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) RWT Sort Mode Target StageID({4}) Port is Empty and No Check 2ndJob Condition!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                        cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                            //目的Port PortMode為EMP(會根據第一片變動影響後續配片邏輯)則直接下命令並回true不需考慮2nd Job
                            bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                            return true;

                        }

                        #endregion


                        #endregion

                    }
                    else
                    {


                        #region [ Judge 1st Job Command and 2nd Job Command ]

                        CellSpecialRobotCmdInfo curJudgeRobotCommand = new CellSpecialRobotCmdInfo();

                        //20160128 add for Up/Low Both Check
                        if (CheckCellSpecialBothCommandCondition(curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command, curRobotCommand) == true)
                        {
                            //符合Up/Low Both條件則只執行1st Job
                            curJudgeRobotCommand.Cmd01_FrontCSTSeq = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontCSTSeq;
                            curJudgeRobotCommand.Cmd01_FrontJobSeq = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobSeq;
                            curJudgeRobotCommand.Cmd01_FrontJobKey = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobKey;
                            curJudgeRobotCommand.Cmd01_BackCSTSeq = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackCSTSeq;
                            curJudgeRobotCommand.Cmd01_BackJobSeq = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackJobSeq;
                            curJudgeRobotCommand.Cmd01_BackJobKey = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackJobKey;
                            curJudgeRobotCommand.Cmd01_ArmSelect = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect;
                            curJudgeRobotCommand.Cmd01_Command = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command;
                            curJudgeRobotCommand.Cmd01_TargetPosition = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_TargetPosition;
                            curJudgeRobotCommand.Cmd01_TargetSlotNo = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo;
                        }
                        else
                        {
                            //1st Job Command Exist and 2nd Job Command Exist , Judge GetGet Command.用2nd Job 1st Command 更新1st Job 2nd Command
                            curJudgeRobotCommand.Cmd01_FrontCSTSeq = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontCSTSeq;
                            curJudgeRobotCommand.Cmd01_FrontJobSeq = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobSeq;
                            curJudgeRobotCommand.Cmd01_FrontJobKey = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobKey;
                            curJudgeRobotCommand.Cmd01_BackCSTSeq = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackCSTSeq;
                            curJudgeRobotCommand.Cmd01_BackJobSeq = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackJobSeq;
                            curJudgeRobotCommand.Cmd01_BackJobKey = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackJobKey;
                            curJudgeRobotCommand.Cmd01_ArmSelect = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_ArmSelect;
                            curJudgeRobotCommand.Cmd01_Command = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command;
                            curJudgeRobotCommand.Cmd01_TargetPosition = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_TargetPosition;
                            curJudgeRobotCommand.Cmd01_TargetSlotNo = cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo;

                            curJudgeRobotCommand.Cmd02_FrontCSTSeq = curRobotCommand.Cmd01_FrontCSTSeq;
                            curJudgeRobotCommand.Cmd02_FrontJobSeq = curRobotCommand.Cmd01_FrontJobSeq;
                            curJudgeRobotCommand.Cmd02_FrontJobKey = curRobotCommand.Cmd01_FrontJobKey;
                            curJudgeRobotCommand.Cmd02_BackCSTSeq = curRobotCommand.Cmd01_BackCSTSeq;
                            curJudgeRobotCommand.Cmd02_BackJobSeq = curRobotCommand.Cmd01_BackJobSeq;
                            curJudgeRobotCommand.Cmd02_BackJobKey = curRobotCommand.Cmd01_BackJobKey;
                            curJudgeRobotCommand.Cmd02_ArmSelect = curRobotCommand.Cmd01_ArmSelect;
                            curJudgeRobotCommand.Cmd02_Command = curRobotCommand.Cmd01_Command;
                            curJudgeRobotCommand.Cmd02_TargetPosition = curRobotCommand.Cmd01_TargetPosition;
                            curJudgeRobotCommand.Cmd02_TargetSlotNo = curRobotCommand.Cmd01_TargetSlotNo;
                        }
                        //Send Robot Control Command
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, curJudgeRobotCommand });

                        if (sendCmdResult == false)
                        {
                            //無法下命令就結束
                            return false;
                        }
                        else
                        {
                            return true;
                        }

                        #endregion


                    }

                    #endregion

                }
                else
                {
                    //Arm Job 針對1Arm 1Job直接回NG
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) RobotArmQty({2}) is illegal",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }

                #endregion

                //下Cmd成功才會回True
                return false;

            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        //20160607
        private bool CheckDoubleArmIsNoExist(Robot curRobot,int armNoIndex)
        {
            try
            {
                if (curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmFrontJobExist == eGlassExist.NoExist && curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmBackJobExist == eGlassExist.NoExist)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

    }
}
