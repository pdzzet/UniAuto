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
        private void CheckRobotControlCommand_For_TypeII(Robot curRobot, List<RobotStage> curRobotAllStageList)
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

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any Select Rule!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                        //failMsg = string.Format("Robot({0}) can not get any Select Rule!", curRobot.Data.ROBOTNAME);

                        failMsg = string.Format("Can not get any Select Rule!");

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

                                    //failMsg = string.Format("Robot({0}) object({1}) MethodName({2}) RtnCode({3})  RtnMsg({4}]!",
                                    //                        curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME, robotConText.GetReturnCode(),
                                    //                        robotConText.GetReturnMessage());

                                    failMsg = string.Format("RtnCode({0})  RtnMsg({1})!",
                                                            robotConText.GetReturnCode(),
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

                        //failMsg = string.Format("Robot({0}) can not get any can control SlotBlockInfo Job!(Please check 1.CST(Load or Both port,wait for process or In process) is Ready and Exist(JobEachCassetteSlotPosition and Job Existence),or 2.Upstream EQP wound SendOut Job,or Robot Arm wound have Job)",
                        //                         curRobot.Data.ROBOTNAME);

                        failMsg = string.Format("RtnCode({0}) RtnMsg({1})", fail_ReasonCode, "Can not get any can control SlotBlockInfo Job!(Please check 1.Robot Arm would have Job 2.CST(Load or Both port,wait for process or In process) is Ready and Exist(JobEachCassetteSlotPosition and Job Existence) 3.Upstream EQP would SendOut Job)");

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
                    robotArmCanControlSlotBlockInfoList_OrderBy = robotArmCanControlSlotBlockInfoList.OrderByDescending(s => s.CurBlock_StepID).ThenBy(s => s.CurBlock_PortCstStatusPriority).ToList();

                    foreach (RobotCanControlSlotBlockInfo curSlotBlockInfo in robotArmCanControlSlotBlockInfoList_OrderBy)
                    {
                        if (CheckRobotArmSlotBlockInfoRouteCondition(curRobot, curSlotBlockInfo)) return; //True表示命令已產生則結束本次cycle
                    }

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
                        robotStageCanControlSlotBlockInfoList_OrderBy = robotStageCanControlSlotBlockInfoList.Where(s => s.CurBlock_Location_StageType == eStageType.PORT).ToList();

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

                                if (CheckRobotStageSlotBlockInfoRouteCondition(curRobot, curSlotBlockInfo))
                                {
                                    return; //True表示命令已產生則結束本次cycle
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




        /// <summary> Check Arm上SlotBlok內的所有Job 目前Step的所有條件是否成立(最多4片2支Fork同時成立)
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotArmJob"></param>
        /// <returns></returns>
        private bool CheckRobotArmSlotBlockInfoRouteCondition(Robot curRobot, RobotCanControlSlotBlockInfo curRobotArmSlotBlockInfo)
        {
            string strlog = string.Empty;
            List<RobotStage> curFilterStageList = new List<RobotStage>();
            CellSpecialRobotCmdInfo curRobotCommand = new CellSpecialRobotCmdInfo();
            Job curRobotArmJob = null;
            RobotRouteStep cur2ndBlockJob_curRouteStep = null;

            try
            {

                #region [ 20160119 Check CurStep RouteStepByPass Condition and 準備變更curStep ]

                if (!CheckSlotBlockInfo_AllRouteStepByPassConditionByStepNo(curRobot, curRobotArmSlotBlockInfo, curRobotArmSlotBlockInfo.CurBlock_StepID, ref curFilterStageList))
                {
                    return false; //StepByPass條件有問題則回覆NG
                }

                #endregion

                #region [ 20160119 add Check CurStep All RouteStepJump Condition and 準備變更curStep ]

                if (!CheckSlotBlockInfo_AllRouteStepJumpConditionByStepNo(curRobot, curRobotArmSlotBlockInfo, curRobotArmSlotBlockInfo.CurBlock_StepID, ref curFilterStageList))
                {
                    return false; //StepJump條件有問題則回覆NG
                }

                #endregion
                #region [ 20160118 add Judge after ByPass and Jump curStep and NextStep 如果Route不一樣則視為有問題記Log回覆目前SlotBlockInfo NG .2ndflag因為是ByJob by Pass Jump所以Filter後還要再比對 ]

                if (CheckSlotBlockInfoJobRouteCondition(curRobot, curRobotArmSlotBlockInfo, "CheckRobotArmSlotBlockInfoRouteCondition") == false)
                {
                    return false; //BlockInfo內Front/Back Job Route有問題則回覆NG
                }

                #endregion

                #region [ 20160119 add Check 2nd Job Command 1st Action & TargetPosition Rule by 1stJob Command Info ]

                cur1stSlotBlock_CommandInfo cur1stSlotBlockCmdInfo = (cur1stSlotBlock_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stSlotBlock_CommandInfo];

                if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                {

                    #region [ 1st Job 1st Command Action 必須是PUT to Stage. 如果是1st Job 1st Command不是Put or Multi-Put則不需考慮2nd Job Command ]

                    if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_PUT)
                    {

                        //直接下命令並回true不需考慮2nd Job
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                        return true;

                    }

                    #endregion

                    //因1st Job and 2nd Job都在Arm上不需要確認Job Location是否相同.且不需要更新ArmInfo                  

                    #region [ Update 1stJob 1st Command Target StageID Empty Slotlist Info by 1stJob 1st Command Target SlotNo ]

                    if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.ContainsKey(cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo) == true)
                    {
                        //根據1st Job  1st Command Target Position/SlotNo 將Target Position的EmptySlotNo預約起來(排除引用)
                        cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.Remove(cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo);
                    }

                    if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.Count == 0)
                    {
                        //20151110 add 如果1st Job curStep設定可以CrossStage(1stJob的Target點)那還是要判斷2ndJob可以處理
                        if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_CrossStageFlag != "Y")
                        {

                            //1st Job Command 之後就沒有空的Slot則不需要2ndJob Command
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job 1st Command TargetStageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            //直接下命令並回true不需考慮2nd Job
                            bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                            return true;

                        }
                    }

                    #endregion

                    #region [ Get Current 2ndJob curStep Entity ]

                    if (curRobotArmSlotBlockInfo.CurBlockCanControlJobList.Count > 0)
                    {
                        curRobotArmJob = curRobotArmSlotBlockInfo.CurBlockCanControlJobList[0];

                        if (curRobotArmJob.RobotWIP.RobotRouteStepList.ContainsKey(curRobotArmJob.RobotWIP.CurStepNo) == true)
                        {
                            cur2ndBlockJob_curRouteStep = curRobotArmJob.RobotWIP.RobotRouteStepList[curRobotArmJob.RobotWIP.CurStepNo];
                        }
                    }

                    //找不到 CurStep Route 回NG
                    if (cur2ndBlockJob_curRouteStep == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) curSlotBlock StageID({2}) CmdSlotNo({3}) can not Get curRouteStep({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmSlotBlockInfo.CurBlock_Location_StageID,
                                                    curRobotArmSlotBlockInfo.CurBlock_RobotCmdSlotNo.ToString(), curRobotArmSlotBlockInfo.CurBlock_StepID.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    #endregion

                    //因1st Job and 2nd Job都在Arm上 所以不需要確認Source Stage是否相同

                    #region [ Check 2ndJob_1stCommand Action by 1stJob_1stCommand ActionCode ]

                    //Mulit-Put 在DB無法預設所以要有Temp轉換
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1st SlotBlock FrontJob({2},{3}) BackJob({4},{5}) Action({6}) But 2ndJob CassetteSequenceNo({7}) JobSequenceNo({8}) curStepNo({9}) Action({10}) is different!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontCSTSeq.ToString(), cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobSeq.ToString(),
                                                cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackCSTSeq.ToString(), cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackJobSeq.ToString(),
                                                cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_DBActionCode, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo, curRobotArmJob.RobotWIP.CurStepNo.ToString(),
                                                cur2ndBlockJob_curRouteStep.Data.ROBOTACTION);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        return false;

                    }

                    #endregion

                }

                #endregion

                #region [ Check CurStep All Filter Condition ]

                DefineNormalRobotCmd cur1stDefindCmd = new DefineNormalRobotCmd();
                DefineNormalRobotCmd cur2ndDefindCmd = new DefineNormalRobotCmd();

                //Arm Job Only Check curStep Filter
                if (!CheckSlotBlockInfo_AllFilterConditionByStepNo(curRobot, curRobotArmSlotBlockInfo, curRobotArmSlotBlockInfo.CurBlock_StepID, cur1stDefindCmd, cur2ndDefindCmd, ref curFilterStageList))
                {
                    //Filter條件有問題則回覆NG
                    return false;
                }

                #endregion

                #region [ 20160119 add Check 2nd Job Command 1st Action & TargetPosition Rule(curStepNo)  ]

                if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                {

                    //找不到 CurStep Route 回NG
                    if (cur2ndBlockJob_curRouteStep == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) curSlotBlock StageID({2}) CmdSlotNo({3}) can not Get curRouteStep({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmSlotBlockInfo.CurBlock_Location_StageID,
                                                    curRobotArmSlotBlockInfo.CurBlock_RobotCmdSlotNo.ToString(), curRobotArmSlotBlockInfo.CurBlock_StepID.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    //預設不允許同Step跨Stage放片
                    bool putJobtoAnotherStageFlag = false;

                    //20151110 add 比對1st Job 與2nd Job 是否可以Cross. Arm上Job為1st Cmd
                    if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_CrossStageFlag == "Y" &&
                        cur2ndBlockJob_curRouteStep.Data.CROSSSTAGEFLAG == "Y")
                    {
                        putJobtoAnotherStageFlag = true;
                    }

                    #region [ Check 2ndJob_1stCommand Can Use StageList by 1stJob_1stCommand TargetStageEntity ]

                    //如果2nd Job 1st Command 是PUT相關則表示 curFilterStageList是給2nd Job 1st Cmd用=>在此使用
                    if (curFilterStageList.Contains(cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity) == false)
                    {

                        //2ndJob的Target目標不包含1stJob的Target點時的處理
                        #region [ 判斷是否允許同Step跨Stage放片,允許的話則不更新curFilterStageList ]

                        if (putJobtoAnotherStageFlag == false)
                        {
                            //2nd Job 1st Command目前Use StageList不和1stJob 1st Command TargetPosition不相同 則不需要再考慮這一片
                            //因為是當下決定 所以直接記Log 可考慮不需要Debug
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stSlotBlock FrontJob({2},{3}) BackJob({4},{5}) 1st TargetPodition({6}) But 2ndSlotBlock CassetteSequenceNo({7}) JobSequenceNo({8}) curStepNo({9}) StageList({10}) is different!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontCSTSeq.ToString(), cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobSeq.ToString(),
                                                    cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackJobSeq.ToString(), cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackJobSeq.ToString(),
                                                    cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                                                    curRobotArmJob.RobotWIP.CurStepNo.ToString(), cur2ndBlockJob_curRouteStep.Data.STAGEIDLIST);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return false;
                        }

                        #endregion

                        //如果允許同Step跨Stage放片則維持原先的curFilterStageList

                    }
                    else
                    {

                        #region [ 2ndJob的Target目標包含1stJob的Target點時的處理 ]

                        //20151110 add當2ndJob 1st Cmd可去Stage包含1stJob的1st Cmd Target Stage 且1stJob 1st Cmd Target Stage不能再放片時,2nd Job的Target點要排除掉沒有空Slot的Stage
                        if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.Count == 0)
                        {
                            curFilterStageList.Remove(cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity);

                            if (curFilterStageList.Count == 0)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stSlotBlock FrontJob({2},{3}) BackJob({4},{5}) 1st TargetPodition({6}) But 2ndJob CassetteSequenceNo({7}) JobSequenceNo({8}) nextStepNo({9}) StageList({9}) can not Receive!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontCSTSeq.ToString(), cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_FrontJobSeq.ToString(),
                                                    cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackCSTSeq.ToString(), cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_BackJobSeq.ToString(),
                                                    cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                                                    curRobotArmJob.RobotWIP.NextStepNo.ToString(), cur2ndBlockJob_curRouteStep.Data.STAGEIDLIST);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                return false;
                            }
                        }
                        else
                        {

                            //強制2nd Job只能進同Stage
                            curFilterStageList.Clear();
                            curFilterStageList.Add(cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity);

                        }

                        #endregion

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
                        switch (curRobotArmSlotBlockInfo.CurBlock_JobExistStatus)
                        {
                            case eRobot_SlotBlock_JobsExistStatus.FRONT_BACK_EXIST:
                                {
                                    Job front_job = curRobotArmSlotBlockInfo.CurBlockCanControlJobList[0];
                                    job_str = string.Format("FrontJob({0}) BackJob({1})", curRobotArmSlotBlockInfo.CurBlockCanControlJobList[0].JobKey, curRobotArmSlotBlockInfo.CurBlockCanControlJobList[1].JobKey);
                                    robot_action = front_job.RobotWIP.RobotRouteStepList[front_job.RobotWIP.CurStepNo].Data.ROBOTACTION;
                                    stageid_list = front_job.RobotWIP.RobotRouteStepList[front_job.RobotWIP.CurStepNo].Data.STAGEIDLIST;
                                }
                                break;
                            case eRobot_SlotBlock_JobsExistStatus.FRONT_EMPTY_BACK_EXIST:
                                {
                                    Job back_job = curRobotArmSlotBlockInfo.CurBlockCanControlJobList[0];
                                    job_str = string.Format("BackJob({0})", curRobotArmSlotBlockInfo.CurBlockCanControlJobList[0].JobKey);
                                    robot_action = back_job.RobotWIP.RobotRouteStepList[back_job.RobotWIP.CurStepNo].Data.ROBOTACTION;
                                    stageid_list = back_job.RobotWIP.RobotRouteStepList[back_job.RobotWIP.CurStepNo].Data.STAGEIDLIST;
                                }
                                break;
                            case eRobot_SlotBlock_JobsExistStatus.FRONT_EXIST_BACK_EMPTY:
                                {
                                    Job front_job = curRobotArmSlotBlockInfo.CurBlockCanControlJobList[0];
                                    job_str = string.Format("FrontJob({0})", curRobotArmSlotBlockInfo.CurBlockCanControlJobList[0].JobKey);
                                    robot_action = front_job.RobotWIP.RobotRouteStepList[front_job.RobotWIP.CurStepNo].Data.ROBOTACTION;
                                    stageid_list = front_job.RobotWIP.RobotRouteStepList[front_job.RobotWIP.CurStepNo].Data.STAGEIDLIST;
                                }
                                break;
                        }
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobExistStatus({2}, {3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) Command Action({8}) is illegal",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmSlotBlockInfo.CurBlock_JobExistStatus, job_str,
                                                curRobotArmSlotBlockInfo.CurBlock_Location_StageID, curRobotArmSlotBlockInfo.CurBlock_StepID.ToString(),
                                                robot_action,
                                                stageid_list,
                                                cur1stDefindCmd.Cmd01_Command.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }

                //Order 1st & 2nd Cmd
                if (CheckSlotBlockInfo_AllOrderByConditionByCommand(curRobot, curRobotArmSlotBlockInfo, cur1stDefindCmd, cur2ndDefindCmd, curFilterStageList) == false)
                {
                    //20151026 add Order By後Cmd 的TargetPosition or SlotNo有問題則回覆NG
                    return false;
                }

                #endregion

                #region [ by RobotArm Qty Create Command ]

                if (curRobot.Data.ARMJOBQTY == 2)
                {

                    #region [ 20160119 add Check First SlotBlockInfo Glass Command ]

                    //Check 1st Command
                    if (!PortFetchOut_FirstSlotBlockGlassCheck(curRobot, curRobotArmSlotBlockInfo, cur1stDefindCmd, false))
                    {
                        return false;
                    }

                    //Check 2nd Command
                    if (!PortFetchOut_FirstSlotBlockGlassCheck(curRobot, curRobotArmSlotBlockInfo, cur2ndDefindCmd, false))
                    {
                        return false;
                    }

                    #endregion

                    #region [ 20160119 Check RTCPUT Condition ] Cell Special 不支援RTCPUT
                    ////GlobalAssemblyVersion v1.0.0.9-20151230
                    //CheckRtcPutCommandCondition(curRobot, curRobotArmJob, cur1stDefindCmd); //Check 1st Command
                    //CheckRtcPutCommandCondition(curRobot, curRobotArmJob, cur2ndDefindCmd, true); //Check 2nd Command
                    #endregion

                    #region [ 20160119 Check Multi-Single Condition ] Cell Special不支援Multi命令

                    ////Check 1st Command
                    //CheckMultiSingleCommandCondition(curRobot, curRobotArmJob, cur1stDefindCmd, false);
                    ////Check 2nd Command
                    //CheckMultiSingleCommandCondition(curRobot, curRobotArmJob, cur2ndDefindCmd, true);

                    #endregion

                    #region [ Create 1 Arm 2 Substrate ]

                    int int1st_FrontCstSeqNo = 0;
                    int int1st_FrontJobSeqNo = 0;
                    int int1st_BackCstSeqNo = 0;
                    int int1st_BackJobSeqNo = 0;
                    int int2nd_FrontCstSeqNo = 0;
                    int int2nd_FrontJobSeqNo = 0;
                    int int2nd_BackCstSeqNo = 0;
                    int int2nd_BackJobSeqNo = 0;

                    #region [ Get Front/Back Job ]

                    foreach (Job job in curRobotArmSlotBlockInfo.CurBlockCanControlJobList)
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

                        #region [ 1st Job 1st Command Action 必須要是Put or Multi-Put且因為在Arm上不需要去預約ArmInfo. 不是Put or Multi-Put則不需考慮2nd Job Command ]

                        //1st Job Command 1st Action必須是Put to Stage.且Use Arm不需考慮如果是1st Job 1st Command 不是Put or Multi-Put則不需考慮2nd Job Command
                        if (cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_PUT &&
                            cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_MULTI_PUT)
                        {

                            //直接下命令並回true不需考慮2nd Job
                            bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                            return true;

                        }

                        #endregion

                        //Arm上Job只須更新 1stJob curStep CrossStageFlag
                        #region [ Get 1stJob CurStep Entity and UpDate 1stJob 1stCmd CrossStageFlag ]

                        //Stage Job Must Check CurStepInfo and NextStepInfo
                        RobotRouteStep cur1stJob_CurRouteStepInfo = null;

                        if (curRobotArmSlotBlockInfo.CurBlockCanControlJobList.Count > 0)
                        {
                            curRobotArmJob = curRobotArmSlotBlockInfo.CurBlockCanControlJobList[0];

                            if (curRobotArmJob.RobotWIP.RobotRouteStepList.ContainsKey(curRobotArmJob.RobotWIP.CurStepNo) == true)
                            {
                                cur1stJob_CurRouteStepInfo = curRobotArmJob.RobotWIP.RobotRouteStepList[curRobotArmJob.RobotWIP.CurStepNo];
                            }
                        }

                        //找不到 CurStep Route 回NG
                        if (cur1stJob_CurRouteStepInfo == null)
                        {
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) curSlotBlock StageID({2}) CmdSlotNo({3}) can not Get curRouteStep({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmSlotBlockInfo.CurBlock_Location_StageID,
                                                        curRobotArmSlotBlockInfo.CurBlock_RobotCmdSlotNo.ToString(), curRobotArmSlotBlockInfo.CurBlock_StepID.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_CrossStageFlag = cur1stJob_CurRouteStepInfo.Data.CROSSSTAGEFLAG;

                        #endregion

                        #region [ Update 1stJob 1st Command Target StageID Empty Slotlist Info by Target SlotNo ]

                        if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.ContainsKey(cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo) == true)
                        {
                            //根據1st Job 1st Command Target Position/SlotNo 將Target Position的EmptySlotNo預約起來(排除引用)
                            cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.Remove(cur1stSlotBlockCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo);
                        }

                        //Cell Special 不支援Exchnge 所以防止Exchange異常拿掉

                        if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotBlockInfoList.Count == 0)
                        {
                            //20151110 add 如果1stJob curStep設定可以CrossStage 那還是要可以處理
                            if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_CrossStageFlag != "Y")
                            {

                                //1st Job Command 之後就沒有空的Slot則不需要2ndJob Command
                                if (cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity != null)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job 1st Command TargetStageID({3})!",
                                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);
                                }
                                else
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job and 2nd Command is null",
                                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);
                                }

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                //直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                                return true;
                            }

                        }

                        #endregion

                        #region [ 20160219 add for 如果Target Stage為Port Type且屬性是會根據收到第一片之後才會變化的則不判斷第二片Job馬上執行命令. EX:PortMode= EMP or Grade is EM ]

                        //只要判斷1stJob 1stCmd target 目的地是否為Port
                        if (cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGETYPE == eRobotStageType.PORT)
                        {

                            #region [ Get Port Entity by 1stJob 2ndCmd Target Stage ]

                            Port curTargetPort = ObjectManager.PortManager.GetPort(cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID);

                            if (curTargetPort == null)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Target Port Entity By StageID({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                                                        cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                //找不到Port Entity則直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                                return true;
                            }

                            #endregion

                            if ((curTargetPort.File.Mode == ePortMode.EMPMode ||
                                 cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.UnloaderPortMatchSetGradePriority== eCellUnloaderDispatchRuleMatchGradePriority.IS_EMP_PRIORITY) &&
                                cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.PortCassetteEmpty == RobotStage.PORTCSTEMPTY.EMPTY)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Target StageID({4}) Port Mode is ({5}) or Unloader Grade is (EM) and No Check 2ndJob Condition!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                                                        cur1stSlotBlockCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curTargetPort.File.Mode.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                //目的Port PortMode為EMP(會根據第一片變動影響後續配片邏輯)則直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                                return true;

                            }

                            //20160301 add for CCRWT Sort Mode Use Only
                            if (CheckIsCellRWT_SortMode(curRobot) == true && cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.PortCassetteEmpty == RobotStage.PORTCSTEMPTY.EMPTY)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) RWT Sort Mode Target StageID({4}) Port is Empty and No Check 2ndJob Condition!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                                                            cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                //目的Port PortMode為EMP(會根據第一片變動影響後續配片邏輯)則直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                                return true;

                            }

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

        /// <summary> Check Stage上SlotBlok內的所有Job目前Step的所有Route Condition是否成立(最多4片2支Fork同時成立)
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotStageSlotBlockInfo"></param>
        /// <returns></returns>
        private bool CheckRobotStageSlotBlockInfoRouteCondition(Robot curRobot, RobotCanControlSlotBlockInfo curRobotStageSlotBlockInfo)
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
                        //直接下命令並回true不需考慮2nd Job
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                        return true;
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
                            //1st Job Command 之後就沒有空的Slot則不需要2ndJob Command
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job 2nd Command TargetStageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            //直接下命令並回true不需考慮2nd Job
                            bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                            return true;
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

                                //1st Job Command 之後就沒有空的Slot則不需要2ndJob Command
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job 2nd Command TargetStageID({3})!",
                                                        curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, cur1stSlotBlockCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                //直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, cur1stSlotBlockCmdInfo.cur1stJob_Command });
                                return true;

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

        //20160301 Check Is CCRWT Line SortMode
        private bool CheckIsCellRWT_SortMode(Robot curRobot)
        {
            string strlog = string.Empty;

            try
            {
                if (Workbench.LineType != eLineType.CELL.CCRWT)
                {
                    //非RWT Line不需考慮是否因為Target是空CST就不考慮第二組命令
                    return false;
                }

                #region [ Get Robot EQP Entity ]

                Equipment curEQP = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                //找不到 EQP 回NG
                if (curEQP == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get EQP by NODENO({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }

                #endregion

                #region [ Get Line Entity ]

                Line line = ObjectManager.LineManager.GetLine(curEQP.Data.LINEID);

                if (line == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get Line by EQP LINEID({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;

                }

                #endregion

                #region [ Check Must CCRWT SORT Mode ]

                //- 1：Inspection Mode   - 2：Sort Mode
                string runModeDesc = string.Empty;
                string eqpRunMode = GetRunMode(line, curEQP.Data.NODEID, "2", out runModeDesc);

                if (curEQP.File.EquipmentRunMode != eqpRunMode)
                {


                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode({5})!",
                                            "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //非RWT Line不需考慮是否因為Target是空CST就不考慮第二組命令
                    return false;
                }

                #endregion

                //RWT Line Sort Mode需考慮是否因為Target是空CST就不考慮第二組命令
                return true;

            }
            catch (Exception ex)
            {
                
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        //Get Cell Run Desc (目前EQP是記錄成Desc)
        private string GetRunMode(Line line, string eqpNo, string value, out string description)
        {
            description = string.Empty;
            ConstantItem item = null;

            try
            {
                #region[CELL Rum Mode]
                item = ConstantManager["CELL_RUNMODE_" + line.Data.LINETYPE][value];
                #endregion

                description = item.Discription;
                return item.Value;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return "UnKnown";
            }

        }

        /// <summary> 根據RobotWIP.CurLocation_StageID, 更新RobotWIP.CurLocation_StagePriority for SlotBlockInfo. 注意! RobotWIP.CurLocation_StagePriority的初始值是null
        ///
        /// </summary>
        /// <param name="CanControlJobList"></param>
        private void UpdateSlotBlockInfoStagePriority(List<RobotCanControlSlotBlockInfo> curCanCtlSlotBlockInfoList)
        {

            try
            {
                foreach (RobotCanControlSlotBlockInfo tmpSlotBlockInfo in curCanCtlSlotBlockInfoList)
                {
                    #region [ Update SlotBlockInfo Priority ]

                    if (tmpSlotBlockInfo.CurBlock_Location_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                    {
                        tmpSlotBlockInfo.CurBlock_Location_StagePriority = eRobotCommonConst.ROBOT_STAGE_HIGTEST_PRIORITY;
                    }
                    else
                    {
                        RobotStage stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(tmpSlotBlockInfo.CurBlock_Location_StageID);

                        if (stage != null)
                        {
                            tmpSlotBlockInfo.CurBlock_Location_StagePriority = stage.Data.PRIORITY.ToString().PadLeft(2, '0');
                        }
                        else
                        {
                            tmpSlotBlockInfo.CurBlock_Location_StagePriority = eRobotCommonConst.ROBOT_STAGE_LOWEST_PRIORITY;
                        }

                    }

                    #endregion

                    #region [ Update SlotBlockInfo JobList Priority ]

                    foreach (Job job in tmpSlotBlockInfo.CurBlockCanControlJobList)
                    {
                        if (job.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                            job.RobotWIP.CurLocation_StagePriority = eRobotCommonConst.ROBOT_STAGE_HIGTEST_PRIORITY;
                        else
                        {
                            RobotStage stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(job.RobotWIP.CurLocation_StageID);
                            if (stage != null)
                                job.RobotWIP.CurLocation_StagePriority = stage.Data.PRIORITY.ToString().PadLeft(2, '0');
                            else
                                job.RobotWIP.CurLocation_StagePriority = eRobotCommonConst.ROBOT_STAGE_LOWEST_PRIORITY;
                        }
                    }

                    #endregion

                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary> 取得各Stage Type LDRQ時的可用的Empty SlotBlockInfo SlotNo
        /// 
        /// </summary>
        /// <param name="curLDRQStage"></param>
        /// <returns></returns>
        private int GetLDRQStageEmptySlotBlockInfoSlotNo(Robot curRobot, Job curBcsJob, int checkStepNo,
                                                            RobotStage curLDRQStage, int cur1stCmdEmptySlotNo, bool findFromSlotNoFlag)
        {
            string strlog = string.Empty;
            int tmpFromSlotNo = 0;
            string tmpLog = string.Empty;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
            bool _isPutReady = false;


            try
            {
                //Set want To Check Function Fail_ReasonCode, 以ObjectName與MethodName為Key來決定是否紀錄Log
                fail_ReasonCode = string.Format("{0}_{1}", "RobotCoreService", "GetLDRQStageEmptySlotNo");

                #region [ Check 是否為Both Port , Cell Special Port不允許Both Port ]

                if (curLDRQStage.Data.STAGETYPE == eRobotStageType.PORT)
                {
                    Port curPort = ObjectManager.PortManager.GetPort(curLDRQStage.Data.STAGEID);

                    if (curPort == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not get Port Entity!",
                                                                    curLDRQStage.Data.NODENO, curRobot.Data.ROBOTNAME, curLDRQStage.Data.STAGEID, curLDRQStage.Data.STAGENAME);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return tmpFromSlotNo;
                    }

                    if (curPort.File.Type == ePortType.BothPort)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) Port Type is Both Port is illigal!",
                                                                    curLDRQStage.Data.NODENO, curRobot.Data.ROBOTNAME, curLDRQStage.Data.STAGEID, curLDRQStage.Data.STAGENAME);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return tmpFromSlotNo;
                    }
                }

                #endregion

                #region [ Stage找尋空的SlotBlockInfo SlotNo放片 ]

                int curLDRQStageID = int.Parse(curLDRQStage.Data.STAGEID);

                //20160602
                Dictionary<int, CellSlotBlock> orderLDRQ_EmptySlotBlockInfoList = null;
                if (Workbench.LineType == eLineType.CELL.CCSOR || Workbench.LineType == eLineType.CELL.CCCHN || Workbench.LineType == eLineType.CELL.CCRWT || Workbench.LineType == eLineType.CELL.CCCRP || Workbench.LineType == eLineType.CELL.CCCRP_2)
                    orderLDRQ_EmptySlotBlockInfoList = curLDRQStage.curLDRQ_EmptySlotBlockInfoList.OrderByDescending(c => c.Value.RowsPriority).ToDictionary(k => k.Key, v => v.Value);
                else
                    orderLDRQ_EmptySlotBlockInfoList = curLDRQStage.curLDRQ_EmptySlotBlockInfoList;

                foreach (int curCmdSlotNo in orderLDRQ_EmptySlotBlockInfoList.Keys)
                //foreach (int curCmdSlotNo in curLDRQStage.curLDRQ_EmptySlotBlockInfoList.Keys)
                {

                    #region [ 判斷有空的Slot 且沒有被1st Cmd占用 ]

                    if (((curLDRQStage.curLDRQ_EmptySlotBlockInfoList[curCmdSlotNo].FrontJobExist == false) &&
                         (curLDRQStage.curLDRQ_EmptySlotBlockInfoList[curCmdSlotNo].BackJobExist == false)) &&
                         curCmdSlotNo != cur1stCmdEmptySlotNo)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) CheckStepNo({5}) Action({6}) Get LDRQ StageID({7}) empty SlotBlockInfoSlotNo({8}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID,
                                                    curCmdSlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                        return curCmdSlotNo;
                    }
                    else
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) CheckStepNo({5}) Action({6}) Get LDRQ StageID({7}) empty Slot, But CmdSlotNo({8}) frontJob({9},{10}) BackJob({11},{12}) is not Empty!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID,
                                                    curCmdSlotNo.ToString(), curLDRQStage.curLDRQ_EmptySlotBlockInfoList[curCmdSlotNo].FrontCstSeqNo,
                                                    curLDRQStage.curLDRQ_EmptySlotBlockInfoList[curCmdSlotNo].FrontJobSeqNo, curLDRQStage.curLDRQ_EmptySlotBlockInfoList[curCmdSlotNo].BackCstSeqNo,
                                                    curLDRQStage.curLDRQ_EmptySlotBlockInfoList[curCmdSlotNo].BackJobSeqNo);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                    }

                    #endregion

                }

                #endregion

                if (curLDRQStage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y") _isPutReady = true;

                #region[DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) CheckStepNo({5}) Action({6}) Get LDRQ StageID({7}) empty Slot Fail!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                            curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #region [ Add To Check Fail Message To Job ]

                if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) && !_isPutReady)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) CheckStepNo({5}) Action({6}) Get LDRQ StageID({7}) empty Slot Fail!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                            curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", tmpLog);

                    #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                    //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) StageID({3}) CheckStepNo({4}) Action({5}) Get LDRQ StageID({6}) empty Slot Fail!",
                    //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                    //                        curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                    //                        curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID);

                    failMsg = string.Format("Job({0}_{1}) StageID({2}) CheckStepNo({3}) Action({4}) Get LDRQ StageID({5}) empty Slot Fail!",
                                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                            curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID);

                    AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                    #endregion

                }

                #endregion

                return 0;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return 0;
            }

        }

        //20160118 add for Cell Special
        /// <summary>當下Robot Command時如果是Port Stage 取片且是WaitForProcess時則要確認First Glass Check = "Y" 才可出片
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkDefineCmd"></param>
        /// <param name="is2ndCmdFlag"></param>
        /// <returns></returns>
        private bool PortFetchOut_FirstSlotBlockGlassCheck(Robot curRobot, RobotCanControlSlotBlockInfo curRobotStageSlotBlockInfo, DefineNormalRobotCmd checkDefineCmd, bool is2ndCmdFlag)
        {
            string strlog = string.Empty;
            string funcName = string.Empty;
            Job curBcsJob = null;

            try
            {

                #region [ Check SlotBlockInfo Job is Exist ]

                if (curRobotStageSlotBlockInfo.CurBlockCanControlJobList.Count == 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobEntity by curSlotBlockInfo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }


                #endregion

                curBcsJob = curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0];

                if (is2ndCmdFlag == true)
                {
                    funcName = "2nd Command";
                }
                else
                {
                    funcName = "1st Command";
                }

                #region [ Check Command Action ]

                if (checkDefineCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET)
                {

                    #region [ Get TargetPosition Stage Entity ]

                    RobotStage curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(checkDefineCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get RobotStageInfo by ({2}) TargetPosition({3})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, funcName, checkDefineCmd.Cmd01_TargetPosition.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    #endregion

                    //Get Port Type 需要Check First Glass Check 
                    if (curStage.Data.STAGETYPE == eRobotStageType.PORT)
                    {

                        #region [ Get Port Entity by StageID , 如果找不到則回NG ]

                        Port curPort = ObjectManager.PortManager.GetPort(curStage.Data.STAGEID);

                        if (curPort == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not get Port Entity!",
                                                                        curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        #endregion

                        #region [ Get CST Entity by Job's CST Seq ]
                        int curCstSeq = 0;
                        int.TryParse(curBcsJob.CassetteSequenceNo, out curCstSeq);
                        Cassette curCST = ObjectManager.CassetteManager.GetCassette(curCstSeq);

                        //找不到 CST 回NG
                        if (curCST == null)
                        {

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get CST Entity by Job CstSeq({2})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        #endregion

                        #region [ Check Cst Status Must Wait For Process ]

                        if (curPort.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING)
                        {
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) CstID({3}) CassetteStatus({4}) no Check First Glass Check.",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, curPort.File.CassetteID,
                                                        curPort.File.CassetteStatus.ToString());

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return true;

                        }

                        #endregion

                        #region [ Check First Glass Condition ] 20151024 modify 改為抓Stage Keep的FirstGlassCheck 的值以避免突然跳片問題

                        //if (curCST.FirstGlassCheckReport == "C2" || curCST.FirstGlassCheckReport == "N")                      
                        if (curStage.File.CstFirstGlassCheckResult == "C2" || curStage.File.CstFirstGlassCheckResult == "N")
                        {
                            //C2:before fetch glass from cst, invoke MES.LotProcessStartRequest
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ({4}) is GET StageID({5}) but CSTID({6}) First Glass Check Report({7}) can not Fetch Out!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        funcName, curStage.Data.STAGEID, curPort.File.CassetteID, curStage.File.CstFirstGlassCheckResult);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;

                        }
                        //else if (curCST.FirstGlassCheckReport == "Y")
                        else if (curStage.File.CstFirstGlassCheckResult == "Y")
                        {
                            // Y:OK, Robort can start fetch glass from cst
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) CstID({3}) First Glass Check Report({4}).",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, curPort.File.CassetteID,
                                                        curStage.File.CstFirstGlassCheckResult);

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return true;
                        }
                        else
                        {
                            //尚未Send First Glass Check 必須要透過準備取第一片時再做FirstGlass Check 所以要回NG
                            //Invoke MESService First Glass Check
                            string trxID = UtilityMethod.GetAgentTrackKey();

                            //LotProcessStartRequest(string trxID, Port port, Cassette cst, Job job)
                            Invoke(eServiceName.MESService, "LotProcessStartRequest", new object[] { trxID, curPort, curCST, curBcsJob });

                            //理論上只會送一次 所以不須用debug
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) CstID({3}) First Glass Check Report({4}) must First Glass Check!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, curPort.File.CassetteID,
                                                        curStage.File.CstFirstGlassCheckResult);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return false;

                        }

                        #endregion

                    }
                }

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        //20160118 add for Cell Special
        /// <summary>for Cell Speical當下Robot Command時, 如果是EQP Stage 並且是有支持Pre-Fetch時, 需要將 Action 從 Put 改成 PutReady
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotStageSlotBlockInfo"></param>
        /// <param name="chkDefineCmd"></param>
        /// <param name="is2ndCmdFlag">false=1st Command, Ture=2nd Command</param>
        private void CheckSlotBlockInfoPutReadyCommandCondition(Robot curRobot, RobotCanControlSlotBlockInfo curRobotStageSlotBlockInfo, DefineNormalRobotCmd chkDefineCmd, bool is2ndCmdFlag)
        {
            string strlog = string.Empty;
            string funcName = (!is2ndCmdFlag ? "1st Command" : "2nd Command");
            RobotStage curStage = null;
            Job curBcsJob = null;

            try
            {

                #region [ Check SlotBlockInfo Job is Exist ]

                if (curRobotStageSlotBlockInfo.CurBlockCanControlJobList.Count == 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobEntity by curSlotBlockInfo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;
                }

                #endregion

                curBcsJob = curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0];

                #region [ Get TargetPosition Stage Entity ]
                curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(chkDefineCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                if (curStage == null) //找不到 Robot Stage 回NG
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get RobotStageInfo by ({2}) TargetPosition({3})!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, funcName, chkDefineCmd.Cmd01_TargetPosition.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return;
                }
                #endregion

                //[ Get Pre-Fetch & Put-Ready condition and check if enable or disable for bypass the following logic ]
                bool _canUsePreFetchFlag = CheckPrefetchFlag(curRobot);
                bool _canUsePutReadyFlag = (curStage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y" ? true : false);


                switch (chkDefineCmd.Cmd01_Command)
                {
                    case eRobot_Trx_CommandAction.ACTION_PUT:
                        if (chkDefineCmd.Cmd01_ArmSelect == 3) break; //All Arm or Both Arm ... break
                        if (curStage.Data.STAGETYPE != eRobotStageType.EQUIPMENT) break; //non-Equipment ... break
                        if (curStage.Data.EQROBOTIFTYPE != eRobotStage_RobotInterfaceType.NORMAL) break; //non-Normal ... break
                        //if (curStage.Data.EXCHANGETYPE != eRobotStage_ExchangeType.GETPUT) break; //non-GETPUT ... break
                        if (curStage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST) break; //non-NOREQ ... break (沒有需求才需要Put Ready!!)
                        if (!_canUsePreFetchFlag) break; //non-Pre-Fetch (沒有開啟Pre Fetch功能) ... break
                        if (!_canUsePutReadyFlag) break; //non-Put-Ready (沒有開啟Put Ready功能) ... break

                        //啟動條件: 前提是來源Port Stage要先有開啟 預取(Pre Fetch) 功能, 然後再來是目的EQP Stage也有開啟 Put Ready 功能才行!!
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ({4}) Action is (PUT) but StageID({5}) Pre-Fecth Change Action to (PUTREADY).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, funcName, curStage.Data.STAGEID);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        #region [ Update Command Action ]
                        chkDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_PUTREADY;
                        #endregion
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }



        //20160115 add Cell Special 1stSlotBlock_CmdInfo
        public class cur1stSlotBlock_CommandInfo
        {
            /// <summary>目前第一片Job產生的所有Job資訊
            /// 
            /// </summary>
            public CellSpecialRobotCmdInfo cur1stJob_Command;
            /// <summary>目前第一片Job 第一個命令的動作(GET/PUT...)
            /// 
            /// </summary>
            public string cur1stJob_1stCommand_DBActionCode;
            /// <summary>目前第一片Job 第一個命令存取的RobotStage資訊
            /// 
            /// </summary>
            public RobotStage cur1stJob_1stCommand_TargetStageEntity;
            /// <summary>目前第一片Job 第二個命令的動作(GET/PUT...)
            /// 
            /// </summary>
            public string cur1stJob_2ndCommand_DBActionCode;
            /// <summary>目前第一片Job 第二個命令存取的RobotStage資訊
            /// 
            /// </summary>
            public RobotStage cur1stJob_2ndCommand_TargetStageEntity;

            //20151110 add for 1st/2nd Cmd CrossStageFlag
            public string cur1stJob_1stCommand_CrossStageFlag;
            public string cur1stJob_2ndCommand_CrossStageFlag;

            public cur1stSlotBlock_CommandInfo()
            {
                cur1stJob_Command = new CellSpecialRobotCmdInfo();
                cur1stJob_1stCommand_DBActionCode = string.Empty;
                cur1stJob_1stCommand_TargetStageEntity = null;
                cur1stJob_2ndCommand_DBActionCode = string.Empty;
                cur1stJob_2ndCommand_TargetStageEntity = null;
                cur1stJob_1stCommand_CrossStageFlag = "N";
                cur1stJob_2ndCommand_CrossStageFlag = "N";
            }
        }

        //20160128 add for Cell Up/low Both Put/Get
        /// <summary>當Cell Special Robot 對同一個非Port Type Stage 下Get or Put命令時如果Slot隸屬左右Block 允許同時2支Fork進出時則改下 UpBoth or LowBoth 命令
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="check1stJobCmd"></param>
        /// <param name="check2ndJobCmd"></param>
        private bool CheckCellSpecialBothCommandCondition(Robot curRobot, CellSpecialRobotCmdInfo check1stJobCmd, CellSpecialRobotCmdInfo check2ndJobCmd)
        {
            string strlog = string.Empty;

            try
            {

                #region [ Check Command Action and UseArm is Not Both(Left Both Arm, Right Both Arm, Upper Both Arm, Lower Both Arm) ]

                if (((check1stJobCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET) || (check1stJobCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_PUT)) &&
                    ((check1stJobCmd.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.BOTH_LEFT) &&
                     (check1stJobCmd.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.BOTH_RIGHT) &&
                     (check1stJobCmd.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.UPBOTH &&
                     (check1stJobCmd.Cmd01_ArmSelect != (int)eCellSpecialRobotCmdArmSelectCode.LOWBOTH))))
                {

                    #region [ Get 1stJob cmd TargetPosition Stage Entity ]

                    RobotStage cur1stTargetStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(check1stJobCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                    //找不到 Robot Stage 回NG
                    if (cur1stTargetStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get RobotStageInfo by 1stJob 1stCommand TargetPosition({2})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, check1stJobCmd.Cmd01_TargetPosition.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    #endregion

                    //1st and 2nd Job Cmd1 Target Stage 一樣而且是非Port Type
                    if (check1stJobCmd.Cmd01_TargetPosition == check2ndJobCmd.Cmd01_TargetPosition && cur1stTargetStage.Data.STAGETYPE != eRobotStageType.PORT)
                    {

                        #region [ Update Use Arm ]

                        if((check1stJobCmd.Cmd01_ArmSelect == (int)eCellSpecialRobotCmdArmSelectCode.UP_LEFT || 
                            check1stJobCmd.Cmd01_ArmSelect == (int)eCellSpecialRobotCmdArmSelectCode.UP_RIGHT) &&
                           (check2ndJobCmd.Cmd01_ArmSelect == (int)eCellSpecialRobotCmdArmSelectCode.UP_LEFT ||
                            check2ndJobCmd.Cmd01_ArmSelect == (int)eCellSpecialRobotCmdArmSelectCode.UP_RIGHT))
                        {

                            check1stJobCmd.Cmd01_ArmSelect =(int)eCellSpecialRobotCmdArmSelectCode.UPBOTH;

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1st SlotBlockInfo FrontJob({2},{3}) BackJob({4},{5}) CmdAction({6}) UseArm({7}) and 2nd SlotBlockInfo FrontJob({8},{9}) BackJob({10},{11}) CmdAction({12}) UseArm({13}) Change UseArm to ({14}).",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, check1stJobCmd.Cmd01_FrontCSTSeq.ToString(), check1stJobCmd.Cmd01_FrontJobSeq.ToString(),
                                                        check1stJobCmd.Cmd01_BackCSTSeq.ToString(), check1stJobCmd.Cmd01_BackJobSeq.ToString(), check1stJobCmd.Cmd01_Command.ToString(), check1stJobCmd.Cmd01_ArmSelect.ToString(),
                                                        check2ndJobCmd.Cmd01_FrontCSTSeq.ToString(), check2ndJobCmd.Cmd01_FrontJobSeq.ToString(), check2ndJobCmd.Cmd01_BackJobSeq.ToString(), check2ndJobCmd.Cmd01_BackJobSeq.ToString(),
                                                        check2ndJobCmd.Cmd01_Command.ToString(), check2ndJobCmd.Cmd01_ArmSelect.ToString(),((int)eCellSpecialRobotCmdArmSelectCode.UPBOTH).ToString());

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            return true;

                        }
                        else if ((check1stJobCmd.Cmd01_ArmSelect == (int)eCellSpecialRobotCmdArmSelectCode.LOW_LEFT ||
                                  check1stJobCmd.Cmd01_ArmSelect == (int)eCellSpecialRobotCmdArmSelectCode.LOW_RIGHT) &&
                                 (check2ndJobCmd.Cmd01_ArmSelect == (int)eCellSpecialRobotCmdArmSelectCode.LOW_LEFT ||
                                  check2ndJobCmd.Cmd01_ArmSelect == (int)eCellSpecialRobotCmdArmSelectCode.LOW_LEFT))
                        {

                            check1stJobCmd.Cmd01_ArmSelect = (int)eCellSpecialRobotCmdArmSelectCode.LOWBOTH;

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1st SlotBlockInfo FrontJob({2},{3}) BackJob({4},{5}) CmdAction({6}) UseArm({7}) and 2nd SlotBlockInfo FrontJob({8},{9}) BackJob({10},{11}) CmdAction({12}) UseArm({13}) Change UseArm to ({14}).",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, check1stJobCmd.Cmd01_FrontCSTSeq.ToString(), check1stJobCmd.Cmd01_FrontJobSeq.ToString(),
                                                        check1stJobCmd.Cmd01_BackCSTSeq.ToString(), check1stJobCmd.Cmd01_BackJobSeq.ToString(), check1stJobCmd.Cmd01_Command.ToString(), check1stJobCmd.Cmd01_ArmSelect.ToString(),
                                                        check2ndJobCmd.Cmd01_FrontCSTSeq.ToString(), check2ndJobCmd.Cmd01_FrontJobSeq.ToString(), check2ndJobCmd.Cmd01_BackJobSeq.ToString(), check2ndJobCmd.Cmd01_BackJobSeq.ToString(),
                                                        check2ndJobCmd.Cmd01_Command.ToString(), check2ndJobCmd.Cmd01_ArmSelect.ToString(), ((int)eCellSpecialRobotCmdArmSelectCode.UPBOTH).ToString());


                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            return true;
                        }

                        #endregion

                    }
                }

                return false;

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

    }
}
