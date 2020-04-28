using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using System.Reflection;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class RobotSelectJobService
    {
        //All Robot Select Stage Job Condition Function List [ Method Name = "Select" + "_" + 對象Stage + "_" + 狀態(Job or Stage Status) ]=========================================================
        //20151207 Add New RobotSelectService03

        #region [ 20151124 add for Cell Special 1Arm2Job ]

        /// <summary> Select All Port Type Stage Can Control SlotBlockInfo List for One Command control One Arm(Two Job)
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("SL0005")]
        public bool Select_PortTypeStage_For1Cmd_1Arm_2Job(IRobotContext robotConText)
        {

            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curRobotStageList = null;
            List<RobotCanControlSlotBlockInfo> robotStageCanControlSlotBlockList;

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

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check Currnet Robot is Cell Special ]

                if (curRobot.Data.ARMJOBQTY != 2)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) ArmQty({3}) is not Cell Special!",
                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] ArmQty({2}) is not Cell Special!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_Robot_Is_Not_Cell_Special);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get CurRobot All Stage List ]

                curRobotStageList = (List<RobotStage>)robotConText[eRobotContextParameter.CurRobotAllStageListEntity];

                //找不到 Robot Stage 回NG
                if (curRobotStageList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo!",
                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get RobotStageInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_RobotStageList_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                if (curRobotStageList.Count == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo!",
                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Get RobotStageInfo is Empty!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_RobotStageList_Is_Empty);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Current Stage Can Control SlotBlockInfo List ]

                robotStageCanControlSlotBlockList = (List<RobotCanControlSlotBlockInfo>)robotConText[eRobotContextParameter.StageCanControlSlotBlockInfoList];;

                //當取不到值時則要回NG
                if (robotStageCanControlSlotBlockList == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get StageCanControlSlotBlockInfoList entity!",
                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get StageCanControlSlotBlockInfoList entity!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_StageCanControlSlotBlockInfoList_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                foreach (RobotStage stage_entity in curRobotStageList)
                {

                    //非Port Type Stage則不判斷
                    if (stage_entity.Data.STAGETYPE.ToUpper().Trim() != eRobotStageType.PORT)
                    {
                        continue;
                    }

                    Get_AllPortTypeStage_CanControlJobList_For1Cmd_1Arm_2Job(curRobot, stage_entity, robotStageCanControlSlotBlockList);

                    CheckJobEachCassetteSlotPositionBlock(stage_entity);
                }

                //不需要回傳
                //robotConText.AddParameter(eRobotContextParameter.StageCanControlJobList, robotStageCanControlJobList);
                robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eRobotSelectJob_ReturnMessage.OK_Message);


                return true;
            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }


        /// <summary> Get PortType Stage Can Control Joblist ,for Cell Special Robot(Upper Left/Right and Lower Left/Right Arm),One Cmd One Arm 2 Job Use 
        ///
        /// </summary>
        private void Get_AllPortTypeStage_CanControlJobList_For1Cmd_1Arm_2Job(Robot curRobot, RobotStage curStage, List<RobotCanControlSlotBlockInfo> curCanCtlSlotBlockInfoList)
        {
            string tmpStageStatus = string.Empty;
            string strlog = string.Empty;
            string failMsg = string.Empty;
            string fail_ReasonCode = string.Empty;

            try
            {

                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                #region [ Get Port Entity by StageID , 如果找不到則 Stage Status =NOREQ ]

                fail_ReasonCode = eRobot_CheckFail_Reason.SELECT_PORT_CAN_NOT_GET_PORT_ENTITY;

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

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00012 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not get Port Entity!",
                                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) StageNo({1}) RobotStageName({2}) can not get Port Entity!",
                        //                    curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME);

                        failMsg = string.Format("StageNo({0}) RobotStageName({1}) can not get Port Entity!",
                                                curStage.Data.STAGEID, curStage.Data.STAGENAME);

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    UpdateStageStatus_for1Arm2Job(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00012 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                }

                #endregion
                #region [Check Port Type Is Not Unknow]
                //20160825 Port Type是Unknown的 就先排除掉
                if (curPort.File.Type == ePortType.Unknown)
                {
                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                    return;
                }
                #endregion
                #region [ Check Port Enable Mode Is Enable ]

                //Create Fail ReasonCode by Port
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.SELECT_PORT_IS_NOT_ENABLE, curPort.Data.PORTID);

                if (curPort.File.EnableMode != ePortEnableMode.Enabled)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Port Enable Mode is ({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curPort.File.EnableMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00013 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Port Enable Mode is ({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curPort.File.EnableMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //20160805 將Unknow的port排除不顯示msg;Loading port要Job在才顯示msg;Both port有Job或是有空Slot才顯示msg;Unloading port有空Slot才顯示msg 
                        if (curPort.File.Type == ePortType.LoadingPort)
                        {
                            if (CheckCSTHasJobUDRQ(curStage))
                            {
                                failMsg = string.Format("StageNo({0}) StageName({1}) Port Enable Mode is ({2})!",
                                                        curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.EnableMode);
                                AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                            }
                        }
                        else if (curPort.File.Type == ePortType.UnloadingPort)
                        {
                            if (CheckEmptySlotCanLDRQ(curStage))
                            {
                                failMsg = string.Format("StageNo({0}) StageName({1}) Port Enable Mode is ({2})!",
                                                        curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.EnableMode);
                                AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                            }
                        }
                        else
                        {
                            failMsg = string.Format("StageNo({0}) StageName({1}) Port Enable Mode is ({2})!",
                                                    curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.EnableMode);
                            AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        }
                        //failMsg = string.Format("StageNo({0}) StageName({1}) Port Enable Mode is ({2})!",
                        //                        curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.EnableMode);

                        //AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        //SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    UpdateStageStatus_for1Arm2Job(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00013 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                }

                #endregion

                #region [ Check Port Down Status Is Normal(not down) ]

                //Create Fail ReasonCode by Port
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.SELECT_PORT_IS_DOWN, curPort.Data.PORTID);

                if (curPort.File.DownStatus != ePortDown.Normal)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Port DownStatus is ({4})!",
                                               curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                               curPort.File.DownStatus);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00014 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Port DownStatus is ({4})!",
                                               curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                               curPort.File.DownStatus);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //20160805 將Unknow的port排除不顯示msg;Loading port要Job在才顯示msg;Both port有Job或是有空Slot才顯示msg;Unloading port有空Slot才顯示msg 
                        if (curPort.File.Type == ePortType.LoadingPort)
                        {
                            if (CheckCSTHasJobUDRQ(curStage))
                            {
                                failMsg = string.Format("StageNo({0}) StageName({1}) Port DownStatus is ({2})!",
                                                        curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.DownStatus);
                                AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                            }
                        }
                        else if (curPort.File.Type == ePortType.UnloadingPort)
                        {
                            if (CheckEmptySlotCanLDRQ(curStage))
                            {
                                failMsg = string.Format("StageNo({0}) StageName({1}) Port DownStatus is ({2})!",
                                                        curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.DownStatus);
                                AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                            }
                        }
                        else
                        {
                            failMsg = string.Format("StageNo({0}) StageName({1}) Port DownStatus is ({2})!",
                                                    curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.DownStatus);
                            AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        }
                        //failMsg = string.Format("StageNo({0}) StageName({1}) Port DownStatus is ({2})!",
                        //                        curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.DownStatus);

                        //AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        //SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    UpdateStageStatus_for1Arm2Job(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00014 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                }

                #endregion

                #region [ Check Port Type Is Not Both Port ]

                //Create Fail ReasonCode by Port
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.SELECT_PORT_TYPE_IS_BOTH, curPort.Data.PORTID);

                if (curPort.File.Type == ePortType.BothPort)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Port Type is ({4})!",
                                               curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                               ePortType.BothPort.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00017 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Port Type is ({4})!",
                                               curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                               ePortType.BothPort.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) StageNo({1}) StageName({2}) Port Type is ({3})!",
                        //                         curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, ePortType.BothPort.ToString());

                        failMsg = string.Format("StageNo({0}) StageName({1}) Port Type is ({2})!",
                                                 curStage.Data.STAGEID, curStage.Data.STAGENAME, ePortType.BothPort.ToString());

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    UpdateStageStatus_for1Arm2Job(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00014 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                }

                #endregion

                ReadSlotInfo(curStage);

                UpdatePortStageMaxSlotCount(curStage, curPort);

                #region [ by Port Update StageStatus and Get Can control Joblist ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.SELECT_PORT_CASSETTESTATUS_NOT_WAITFORPROCESS_INPROCESS, curPort.Data.PORTID);
                //20151029 add Aborting Port可收片不可出片
                if (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING ||
                    curPort.File.CassetteStatus == eCassetteStatus.IN_PROCESSING ||
                    curPort.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                {
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                    Cassette cassette = ObjectManager.CassetteManager.GetCassette(curPort.File.CassetteID);
                    if (cassette != null)
                    {
                        curStage.CassetteStartTime = cassette.StartTime;
                    }
                    else
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not set Cassette Start Time, because CassetteID({4}) is missing in CassetteManager",
                                                                    curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.CassetteID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                    }

                    if (curPort.File.Type == ePortType.LoadingPort)
                    {
                        //Step01 Loader Port Get Can control Joblist
                        Get_LoaderPort_CanControlJoblist_For1Arm2Job(curRobot, curStage, curCanCtlSlotBlockInfoList, curPort);
                    }
                    else if (curPort.File.Type == ePortType.UnloadingPort)
                    {
                        //For Unload Port Get Can control Joblist
                        Get_UnloadPort_StageStatueInfo_For1Arm2Job(curRobot, curStage, curPort);
                    }

                }
                else
                {

                    #region [ 狀態都不符合收送片條件時則視為NOREQ並更新Stage Status ]

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) PortType({4}) CassetteStatus({5}) is illegal!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curPort.File.Type, curPort.File.CassetteStatus);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Port CassetteStatus is ({4})!",
                                               curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                               curPort.File.CassetteStatus);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                        //20160805 將Unknow的port排除不顯示msg;Loading port要Job在才顯示msg;Both port有Job或是有空Slot才顯示msg;Unloading port有空Slot才顯示msg 
                        if (curPort.File.Type == ePortType.LoadingPort)
                        {
                            if (CheckCSTHasJobUDRQ(curStage))
                            {
                                failMsg = string.Format("StageNo({0}) StageName({1}) Port CassetteStatus is ({2})!",
                                                        curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.CassetteStatus);
                                AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                            }
                        }
                        else if (curPort.File.Type == ePortType.UnloadingPort)
                        {
                            if (CheckEmptySlotCanLDRQ(curStage))
                            {
                                failMsg = string.Format("StageNo({0}) StageName({1}) Port CassetteStatus is ({2})!",
                                                        curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.CassetteStatus);
                                AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                            }
                        }
                        else
                        {
                            failMsg = string.Format("StageNo({0}) StageName({1}) Port CassetteStatus is ({2})!",
                                                    curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.CassetteStatus);
                            AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        }
                        //failMsg = string.Format("StageNo({0}) StageName({1}) Port CassetteStatus is ({2})!",
                        //                        curStage.Data.STAGEID, curStage.Data.STAGENAME, curPort.File.CassetteStatus);

                        //AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        //SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    UpdateStageStatus_for1Arm2Job(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

                    #endregion

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }

        }

        /// <summary> 確認Loader Port Slot是否有JOB存在並加入到Can Control List for Cell Special 1Arm2Job
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curCanCtlSlotBlockInfoList"></param>
        /// <param name="curPort"></param>
        private void Get_LoaderPort_CanControlJoblist_For1Arm2Job(Robot curRobot, RobotStage curStage, List<RobotCanControlSlotBlockInfo> curCanCtlSlotBlockInfoList, Port curPort)
        {

            string tmpStageStatus = string.Empty;
            string trxID = string.Empty;
            string strlog = string.Empty;
            string checkReasonCode = string.Empty;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
            int tmpCstColumnCount = 0;

            try
            {

                //預設為NoReq
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                #region [ Aborting不可出片只可收片 ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00013 ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_FETCHPORT_BUT_STATUS_IS_ABORTING, curPort.Data.PORTID);

                if (curPort.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                {
                    //Port Status Aborting 不能取片(UDRQ)則需更新Stage Status並紀錄Log
                    UpdateStageStatus_for1Arm2Job(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name);

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00013 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) Status is ({3}) can not Fetch Out Job!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, eCassetteStatus.IN_ABORTING.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) Port({1}) Status is ({2}) can not Fetch Out Job!",
                        //                         curRobot.Data.ROBOTNAME, curPort.Data.PORTID, eCassetteStatus.IN_ABORTING.ToString());

                        failMsg = string.Format("Port({0}) Status is ({1}) can not Fetch Out Job!",
                                                 curPort.Data.PORTID, eCassetteStatus.IN_ABORTING.ToString());

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00013 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                #region [ For 1Arm2Job CST Special 不可以是Random CST ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00015 ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.SELECT_PORT_CASSETTE_TYPE_IS_RANDOM, curPort.Data.PORTID);

                if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                {
                    //Stage CST Type設定為RanDom 不能取片(UDRQ)則需更新Stage Status並紀錄Log
                    UpdateStageStatus_for1Arm2Job(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name);

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00013 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) CST Type is ({3}) can not Fetch Out Job for Cell Special CST!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, eRobotStageCSTType.RANDOM_CST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) StageID({1}) CST Type is ({2}) can not Fetch Out Job for Cell Special CST!",
                        //                         curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, eRobotStageCSTType.RANDOM_CST);

                        failMsg = string.Format("StageID({0}) CST Type is ({1}) can not Fetch Out Job for Cell Special CST!",
                                                 curStage.Data.STAGEID, eRobotStageCSTType.RANDOM_CST);

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_000135 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                }

                #endregion

                #region [ Check FirstGlsssCheck, Fail表示不可出片 ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00009 ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_FETCHPORT_WAIT_FIRSTGLASSCHECK, curPort.Data.PORTID);

                if (CheckFirstGlassCheckCondition(curRobot, curPort, curStage) == false)
                {
                    //Port上CST 尚未做完FirstGlass Check.不可以取片(UDRQ)則需更新Stage Status並紀錄Log
                    UpdateStageStatus_for1Arm2Job(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name);

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00005 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) First Glass Check Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) Port({1}) First Glass Check Fail!",
                        //                         curRobot.Data.ROBOTNAME, curPort.Data.PORTID);

                        failMsg = string.Format("Port({0}) First Glass Check Fail!",
                                                curPort.Data.PORTID);

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00009 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                #region  [ Real time Get Port Slot Exist Info Trx ]

                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotExistsBlock
                trxID = string.Format("{0}_Port#{1}CassetteStatusChangeReport", curStage.Data.NODENO, curStage.Data.STAGEID);

                Trx get_CSTSlot_ExistInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                if (get_CSTSlot_ExistInfo_Trx == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not Get TrxID({4})!",
                                               curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                               trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    UpdateStageStatus_for1Arm2Job(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

                    return;
                }

                #endregion

                #region  [ Real time Get Port Slot Job Info Trx ]

                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotPositionBlock               
                trxID = GetTrx_CSTSlotJobEachPosition(curStage.Data.NODENO, curStage.Data.STAGEID);

                Trx get_CSTSlot_JobInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                if (get_CSTSlot_JobInfo_Trx == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not Get TrxID({4})!",
                                               curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                               trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    UpdateStageStatus_for1Arm2Job(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

                    return;
                }

                #endregion

                #region [ 根據Eqp設定上報的Robot FetchOut CST的順序來決定抽片. Cell Special Robot(1ArmJob僅支援 Lower to Upper) ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00016 ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.SELECT_PORT_CST_FETCHOUT_MODE_IS_NOT_SEQUENCE, curPort.Data.PORTID);

                if (curRobot.EqpRptCSTFetchSeqMode == eRobotCommonConst.DB_ORDER_BY_ASC)
                {
                    //Clear[ Robot_Fail_Case_00016 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                    #region [ Get CST Column Count ]

                    tmpCstColumnCount = GetPortTypeStageCSTColumnCount(curStage);

                    if (tmpCstColumnCount < 1)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) MaxSlotNo({4}) Cell Column Max Count({5}) can not Get Column!",
                                                   curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                   curStage.Data.SLOTMAXCOUNT.ToString(), eRobotCommonConst.CELL_1ARM2JOB_ONE_COLUMN_COUNT.ToString());
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //計算不出Column則不要取該CST以免出問題
                        return;
                    }

                    #endregion

                    #region [ by Column 取得Can ConTrol Job List ]

                    bool columnUDRQFlag = false;
                    bool cstUDRQFlag = false;

                    for (int columnIdx = 1; columnIdx <= tmpCstColumnCount; columnIdx++)
                    {
                        //如果有變化成UDRQ則會在Function內處理
                        Get_LoaderPort_Column_CanControlJoblist_ASC(curRobot, curStage, curCanCtlSlotBlockInfoList, curPort, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, 
                                                                        columnIdx, ref columnUDRQFlag);

                        //當任何一個Column UDRQ時則整個CST都可UDRQ
                        if (columnUDRQFlag == true)
                        {
                            cstUDRQFlag = true;
                        }

                    }

                    //有其中一個RobotCmdSlot UDRQ則為True
                    if (cstUDRQFlag == false)
                    {
                        //Stage內最後的Slot為空or有其他錯誤視為找不到or帳料有問題,則需更新Stage Status並紀錄Log
                        UpdateStageStatus_for1Arm2Job(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);
                    }

                    #endregion

                }
                else
                {

                    //Cell 1Arm2Job只允許Lower to Upper 抽片方式
                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00013 ]

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) EqpRptCSTFetchSeqMode({2}) is not ({3}) can not Fetch Out Job!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.EqpRptCSTFetchSeqMode, eRobotCommonConst.DB_ORDER_BY_ASC);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) EqpRptCSTFetchSeqMode({2}) is not ({3}) can not Fetch Out Job!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.EqpRptCSTFetchSeqMode, eRobotCommonConst.DB_ORDER_BY_ASC);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) EqpRptCSTFetchSeqMode({1}) is not ({2}) can not Fetch Out Job!",
                        //                         curRobot.Data.ROBOTNAME, curRobot.EqpRptCSTFetchSeqMode, eRobotCommonConst.DB_ORDER_BY_ASC);

                        failMsg = string.Format("EqpRptCSTFetchSeqMode({0}) is not ({1}) can not Fetch Out Job!",
                                                curRobot.EqpRptCSTFetchSeqMode, eRobotCommonConst.DB_ORDER_BY_ASC);

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>取得Loader Port 內Can Control JobList by CST Column Order ASC(從SlotNo 最小開始尋找)
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curCanCtlSlotBlockInfoList"></param>
        /// <param name="curPort"></param>
        /// <param name="get_CSTSlot_ExistInfo_Trx"></param>
        /// <param name="get_CSTSlot_JobInfo_Trx"></param>
        /// <param name="columnIdx"></param>
        private void Get_LoaderPort_Column_CanControlJoblist_ASC(Robot curRobot, RobotStage curStage, List<RobotCanControlSlotBlockInfo> curCanCtlSlotBlockInfoList, Port curPort,
                                                                  Trx get_CSTSlot_ExistInfo_Trx, Trx get_CSTSlot_JobInfo_Trx, int columnIdx, ref bool columnJobUDRFlag)
        {
            string tmpStageStatus = string.Empty;
            string strlog = string.Empty;
            string checkFrontReasonCode = string.Empty;
            string checkBackReasonCode = string.Empty;
            int frontSlotNo = 0;
            int backSlotNo = 0;
            //columnJobUDRFlag = false;

            try
            {
                int tmpMinSlotNo = ((columnIdx - 1) * eRobotCommonConst.CELL_1ARM2JOB_ONE_COLUMN_COUNT) + 1;
                int tmpMaxSlotNo = ((columnIdx - 1) * eRobotCommonConst.CELL_1ARM2JOB_ONE_COLUMN_COUNT) + eRobotCommonConst.CELL_1ARM2JOB_ONE_COLUMN_COUNT;

                #region [ 抽片順序為ASC SlotNo由小到大 ]

                for (int i = tmpMinSlotNo; i <= tmpMaxSlotNo; i++)
                {
                    //by RobotCmdSlotNo處理(奇數SlotNo)
                    if (i % 2 == 0)
                    {
                        continue;
                    }

                    frontSlotNo = i;
                    backSlotNo = i + 1;
                    Job findFrontJob = null;
                    Job findBackJob = null;

                    checkFrontReasonCode = Get_CSTSlot_CanControlJoblist_For1Arm2Job(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, frontSlotNo, curPort, curRobot, ref findFrontJob);
                    checkBackReasonCode = Get_CSTSlot_CanControlJoblist_For1Arm2Job(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, backSlotNo, curPort, curRobot, ref findBackJob);

                    #region [ 循序CST的處理,不可跳Slot ]

                    //SubLocation Front 找不到可出片Job
                    if (checkFrontReasonCode != ePortJobUDRQReason.REASON_OK && checkFrontReasonCode != ePortJobUDRQReason.IS_EMPTY_SLOT)
                    {
                        columnJobUDRFlag = false;
                        return;
                    }

                    //SubLocation Back 找不到可出片Job
                    if (checkBackReasonCode != ePortJobUDRQReason.REASON_OK && checkBackReasonCode != ePortJobUDRQReason.IS_EMPTY_SLOT)
                    {
                        columnJobUDRFlag = false;
                        return;
                    }

                    if (checkFrontReasonCode == ePortJobUDRQReason.IS_EMPTY_SLOT && checkBackReasonCode == ePortJobUDRQReason.IS_EMPTY_SLOT)
                    {
                        //Is Empty Slot 繼續找第二組RobotCmdSlotNo
                        continue;
                    }

                    #region [ Update CanControlSlotBlockInfo by Front & Back Job ]

                    RobotCanControlSlotBlockInfo curSlotBlockInfo = new RobotCanControlSlotBlockInfo();
                    string tmpFrontCstSeq = "0";
                    string tmpFrontJobSeq = "0";
                    string tmpBackCstSeq = "0";
                    string tmpBackJobSeq = "0";
                    string frontJobKey = string.Empty;
                    string backJobKey = string.Empty;

                    if (checkFrontReasonCode != ePortJobUDRQReason.IS_EMPTY_SLOT)
                    {
                        //Front Job Exist 以FrontJob資訊為主
                        curSlotBlockInfo.CurBlock_Location_StageID = findFrontJob.RobotWIP.CurLocation_StageID;
                        curSlotBlockInfo.CurBlock_Location_StagePriority = findFrontJob.RobotWIP.CurLocation_StagePriority;
                        curSlotBlockInfo.CurBlock_PortCstStatusPriority = findFrontJob.RobotWIP.CurPortCstStatusPriority;
                        curSlotBlockInfo.CurBlock_RobotCmdSlotNo = findFrontJob.RobotWIP.CurRobotCmdSlotNo;
                        curSlotBlockInfo.CurBlock_StepID = findFrontJob.RobotWIP.CurStepNo;
                        //20160119 add SlotBlockInfo Stage Type
                        curSlotBlockInfo.CurBlock_Location_StageType = findFrontJob.RobotWIP.CurLocation_StageType;

                        frontJobKey = string.Format("{0}_{1}", findFrontJob.CassetteSequenceNo.ToString(), findFrontJob.JobSequenceNo.ToString());

                        #region [ Add FrontJob To SlotBlockInfo Can Control Joblist ]

                        if (AddToCanControlJoblistCondition(curRobot, curStage, findFrontJob, curSlotBlockInfo.CurBlockCanControlJobList, frontJobKey, MethodBase.GetCurrentMethod().Name) == true)
                        {

                            #region [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) add CassetteSequenceNo({4}) JobSequenceNo({5}) CurRouteStepNo({6}) CurSlotNo({7}) to Can control List!",
                                                        curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                        findFrontJob.CassetteSequenceNo, findFrontJob.JobSequenceNo, findFrontJob.RobotWIP.CurStepNo.ToString(), findFrontJob.RobotWIP.CurLocation_SlotNo.ToString());

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add UDRQ SlotBlockInfo To curUDRQ_SlotBlockInfoList( Dictionary<int, Dictionary<int, string>>) ]

                            lock (curStage)
                            {

                                if (curStage.curUDRQ_SlotBlockInfoList.ContainsKey(curSlotBlockInfo.CurBlock_RobotCmdSlotNo) == false)
                                {
                                    Dictionary<int, string> curUDRQJobList = new Dictionary<int, string>();

                                    curUDRQJobList.Add(frontSlotNo, frontJobKey);

                                    curStage.curUDRQ_SlotBlockInfoList.Add(curSlotBlockInfo.CurBlock_RobotCmdSlotNo, curUDRQJobList);
                                }
                                else
                                {
                                    if (curStage.curUDRQ_SlotBlockInfoList[curSlotBlockInfo.CurBlock_RobotCmdSlotNo].ContainsKey(frontSlotNo) == false)
                                    {
                                        curStage.curUDRQ_SlotBlockInfoList[curSlotBlockInfo.CurBlock_RobotCmdSlotNo].Add(frontSlotNo, frontJobKey);

                                    }
                                }

                            }

                            #endregion

                        }
                        else
                        {
                            columnJobUDRFlag = false;
                            return;
                        }

                        tmpFrontCstSeq = findFrontJob.CassetteSequenceNo;
                        tmpFrontJobSeq = findFrontJob.JobSequenceNo;

                        #endregion

                        #region [ Add BackJob To SlotBlockInfo Can Control Joblist ]

                        if (checkBackReasonCode != ePortJobUDRQReason.IS_EMPTY_SLOT)
                        {
                            backJobKey = string.Format("{0}_{1}", findBackJob.CassetteSequenceNo.ToString(), findBackJob.JobSequenceNo.ToString());

                            #region [ Add BackJob To SlotBlockInfo Can Control Joblist ]

                            if (AddToCanControlJoblistCondition(curRobot, curStage, findBackJob, curSlotBlockInfo.CurBlockCanControlJobList, backJobKey, MethodBase.GetCurrentMethod().Name) == true)
                            {

                                #region [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) add CassetteSequenceNo({4}) JobSequenceNo({5}) CurRouteStepNo({6}) CurSlotNo({7}) to Can control List!",
                                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                            findBackJob.CassetteSequenceNo, findBackJob.JobSequenceNo, findBackJob.RobotWIP.CurStepNo.ToString(), findBackJob.RobotWIP.CurLocation_SlotNo.ToString());

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                #region [ Add UDRQ SlotBlockInfo To curUDRQ_SlotBlockInfoList( Dictionary<int, Dictionary<int, string>>) ]

                                lock (curStage)
                                {
                                    //Check CmdSlotNo Exist
                                    if (curStage.curUDRQ_SlotBlockInfoList.ContainsKey(curSlotBlockInfo.CurBlock_RobotCmdSlotNo) == false)
                                    {
                                        Dictionary<int, string> curTmpUDRQJobList = new Dictionary<int, string>();

                                        curTmpUDRQJobList.Add(backSlotNo, backJobKey);

                                        curStage.curUDRQ_SlotBlockInfoList.Add(curSlotBlockInfo.CurBlock_RobotCmdSlotNo, curTmpUDRQJobList);
                                    }
                                    else
                                    {
                                        if (curStage.curUDRQ_SlotBlockInfoList[curSlotBlockInfo.CurBlock_RobotCmdSlotNo].ContainsKey(backSlotNo) == false)
                                        {
                                            curStage.curUDRQ_SlotBlockInfoList[curSlotBlockInfo.CurBlock_RobotCmdSlotNo].Add(backSlotNo, backJobKey);

                                        }
                                    }

                                }

                                #endregion

                            }
                            else
                            {
                                columnJobUDRFlag = false;
                                return;
                            }

                            tmpBackCstSeq = findBackJob.CassetteSequenceNo;
                            tmpBackJobSeq = findBackJob.JobSequenceNo;

                            #endregion

                            //Back is Exist
                            curSlotBlockInfo.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_BACK_EXIST;

                        }
                        else
                        {
                            //Back is Empty
                            curSlotBlockInfo.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_EXIST_BACK_EMPTY;
                        }

                        #endregion

                    }
                    else
                    {
                        //Front Job NoExist 以BackJob資訊為主
                        curSlotBlockInfo.CurBlock_Location_StageID = findBackJob.RobotWIP.CurLocation_StageID;
                        curSlotBlockInfo.CurBlock_Location_StagePriority = findBackJob.RobotWIP.CurLocation_StagePriority;
                        curSlotBlockInfo.CurBlock_PortCstStatusPriority = findBackJob.RobotWIP.CurPortCstStatusPriority;
                        curSlotBlockInfo.CurBlock_RobotCmdSlotNo = findBackJob.RobotWIP.CurRobotCmdSlotNo;
                        curSlotBlockInfo.CurBlock_StepID = findBackJob.RobotWIP.CurStepNo;
                        //20160119 add SlotBlockInfo Stage Type
                        curSlotBlockInfo.CurBlock_Location_StageType = findBackJob.RobotWIP.CurLocation_StageType;

                        backJobKey = string.Format("{0}_{1}", findBackJob.CassetteSequenceNo.ToString(), findBackJob.JobSequenceNo.ToString());

                        #region [ Add BackJob To SlotBlockInfo Can Control Joblist ]

                        if (AddToCanControlJoblistCondition(curRobot, curStage, findBackJob, curSlotBlockInfo.CurBlockCanControlJobList, backJobKey, MethodBase.GetCurrentMethod().Name) == true)
                        {

                            #region [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) add CassetteSequenceNo({4}) JobSequenceNo({5}) CurRouteStepNo({6}) CurSlotNo({7}) to Can control List!",
                                                        curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                        findBackJob.CassetteSequenceNo, findBackJob.JobSequenceNo, findBackJob.RobotWIP.CurStepNo.ToString(), findBackJob.RobotWIP.CurLocation_SlotNo.ToString());

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add UDRQ SlotBlockInfo To curUDRQ_SlotBlockInfoList( Dictionary<int, Dictionary<int, string>>) ]

                            lock (curStage)
                            {
                                //Check CmdSlotNo Exist
                                if (curStage.curUDRQ_SlotBlockInfoList.ContainsKey(curSlotBlockInfo.CurBlock_RobotCmdSlotNo) == false)
                                {
                                    Dictionary<int, string> curTmp2UDRQJobList = new Dictionary<int, string>();

                                    curTmp2UDRQJobList.Add(backSlotNo, backJobKey);

                                    curStage.curUDRQ_SlotBlockInfoList.Add(curSlotBlockInfo.CurBlock_RobotCmdSlotNo, curTmp2UDRQJobList);
                                }
                                else
                                {
                                    if (curStage.curUDRQ_SlotBlockInfoList[curSlotBlockInfo.CurBlock_RobotCmdSlotNo].ContainsKey(backSlotNo) == false)
                                    {
                                        curStage.curUDRQ_SlotBlockInfoList[curSlotBlockInfo.CurBlock_RobotCmdSlotNo].Add(backSlotNo, backJobKey);

                                    }
                                }

                            }

                            #endregion

                        }
                        else
                        {
                            columnJobUDRFlag = false;
                            return;
                        }

                        tmpBackCstSeq = findBackJob.CassetteSequenceNo;
                        tmpBackJobSeq = findBackJob.JobSequenceNo;

                        #endregion                     

                        //Front is Empty
                        curSlotBlockInfo.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_EMPTY_BACK_EXIST;
                    }

                    #endregion

                    //Check Can Control SlotBlockInfo Exist
                    if (AddToCanControlSlotBlockInfolistCondition(curRobot, curStage, curSlotBlockInfo, curCanCtlSlotBlockInfoList, MethodBase.GetCurrentMethod().Name) == true)
                    {

                        #region [ Update Stage Staus Info ]

                        UpdateStageStatus_for1Arm2Job(curStage, eRobotStageStatus.SEND_OUT_READY, MethodBase.GetCurrentMethod().Name);

                        #endregion

                        columnJobUDRFlag = true;

                        return;
                    }
                    //if Front,Back不一致,直接跳出,不再比對之後Block裡的Job
                    else
                    {
                        return;
                    }
                    #endregion

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary> 當Front & Back Job同時存在時需要確認兩片的Route要為一致才可取放片
        /// 
        /// </summary>
        /// <param name="frontJob"></param>
        /// <param name="backJob"></param>
        /// <returns></returns>
        private bool CheckFrontBackJobCondition(Robot curRobot, Job frontJob, Job backJob , string curObjName , string curMethodName)
        {
            string frontJob_Fail_ReasonCode = string.Empty;
            string backJob_Fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            string failMsg = string.Empty;

            try
            {
                frontJob_Fail_ReasonCode = string.Format("{0}_{1}_{2}_{3}", curObjName, curMethodName, "CheckFrontBackJobCondition", frontJob.RobotWIP.CurStepNo.ToString());
                backJob_Fail_ReasonCode = string.Format("{0}_{1}_{2}_{3}", curObjName, curMethodName, "CheckFrontBackJobCondition", backJob.RobotWIP.CurStepNo.ToString());

                #region [ Check Front and Back RouteID ]

                if (frontJob.RobotWIP.CurRouteID.Trim() != backJob.RobotWIP.CurRouteID.Trim())
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Front Job(({2}),({3})) curRouteID({4}) But Back Job(({5}),({6})) curRouteID({7}) is different!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.CurRouteID.Trim(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurRouteID.Trim());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Front Job Check Fail Message To Job ]

                    if (!frontJob.RobotWIP.CheckFailMessageList.ContainsKey(frontJob_Fail_ReasonCode))
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Front Job(({2}),({3})) curRouteID({4}) But Back Job(({5}),({6})) curRouteID({7}) is different!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.CurRouteID.Trim(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurRouteID.Trim());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("Front Job(({1}),({2})) curRouteID({3}) But Back Job(({4}),({5})) curRouteID({6}) is different!",
                                                frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.CurRouteID.Trim(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurRouteID.Trim());

                        AddJobCheckFailMsg(frontJob, frontJob_Fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, frontJob_Fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #endregion

                    #region [ Add To Back Job Check Fail Message To Job ]

                    if (!backJob.RobotWIP.CheckFailMessageList.ContainsKey(backJob_Fail_ReasonCode))
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Front Job(({2}),({3})) curRouteID({4}) But Back Job(({5}),({6})) curRouteID({7}) is different!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.CurRouteID.Trim(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurRouteID.Trim());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) Front Job(({1}),({2})) curRouteID({3}) But Back Job(({4}),({5})) curRouteID({6}) is different!",
                        //                        curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                        //                        frontJob.RobotWIP.CurRouteID.Trim(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurRouteID.Trim());

                        failMsg = string.Format("Front Job(({0}_{1})) curRouteID({2}) But Back Job(({3}_{4})) curRouteID({5}) is different!",
                                                frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.CurRouteID.Trim(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurRouteID.Trim());

                        AddJobCheckFailMsg(frontJob, frontJob_Fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, frontJob_Fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #endregion

                    return false;
                }
                else
                {
                    //Clear frontJob Fail MSG
                    RemoveJobCheckFailMsg(frontJob, frontJob_Fail_ReasonCode);
                    //Clear BackJob Fail MSG
                    RemoveJobCheckFailMsg(backJob, backJob_Fail_ReasonCode);

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


        /// <summary> 取得Loader Port中存在且有帳的Job and Add to Can Control Job for Cell Special 1Arm 2Job
        ///
        /// </summary>
        /// <param name="curStage"></param>
        /// <param name="trxExistInfo">CST內每個Slot Position的Job Info</param>
        /// <param name="trxJobInfo">CST內每個Slot內的Exist Info</param>
        /// <param name="slotKey"></param>
        /// <param name="curRobotJobList"></param>
        /// <param name="curPort"></param>
        /// <param name="curRobot"></param>
        /// <returns></returns>
        private string Get_CSTSlot_CanControlJoblist_For1Arm2Job(RobotStage curStage, Trx trxExistInfo, Trx trxJobInfo, int slotKey, Port curPort, Robot curRobot, ref Job curBcsJob)
        {
            string jobKey = string.Empty;
            string strlog = string.Empty;
            string failMsg = string.Empty;
            string allSlotExistInfo = string.Empty;
            int job_ExistInfo = ePortSlotExistInfo.JOB_NO_EXIST;
            string jobTrx_CstSeqkey = string.Empty;
            string jobTrx_JobSeqkey = string.Empty;
            string jobTrx_GroupName = string.Empty;
            string jobTrx_EventName = string.Empty;
            int job_CstSeq = 0;
            int job_JobSeq = 0;
            string fail_ReasonCode = string.Empty;
            string tmpPortCstStatusPriority = string.Empty;
            int tmpRobotCmdSlotNo = 0;
            string tmpSubLocation = string.Empty;

            try
            {


                #region [ Check Slot Job Exist Status ]

                #region [ Port#XXJobEachCassetteSlotExistsBlock Structure ]

                //<Event name="L2_W_Port#02PortandCassetteStatusBlock" device="W" address="000101A" points="74" ENABLE="1">
                //    <Item name="PortStatus" type="INT" points="1" offset="0" VALUE="0" />
                //    <Item name="CassetteStatus" type="INT" points="1" offset="1" VALUE="0" />
                //    <Item name="CassetteSequenceNo" type="INT" points="1" offset="2" VALUE="0" />
                //    <Item name="CassetteID" type="ASCII" points="14" offset="3" VALUE="0" />
                //    <Item name="JobCountinCassette" type="INT" points="1" offset="17" VALUE="0" />
                //    <Item name="CompletedCassetteData" type="INT" points="1" offset="18" VALUE="0" />
                //    <Item name="OperatorID" type="ASCII" points="4" offset="19" VALUE="0" />
                //    <Item name="JobExistence" type="BIN" points="38" offset="23" VALUE="0" />
                //    <Item name="LoadingCassetteType" type="INT" points="1" offset="61" VALUE="0" />
                //    <Item name="QTimeFlag" type="INT" points="1" offset="62" VALUE="0" />
                //    <Item name="PartialFullFlag" type="INT" points="1" offset="63" VALUE="0" />
                //    <Item name="CassetteSettingCode" type="ASCII" points="2" offset="64" VALUE="0" />
                //    <Item name="CompletedCassetteReason" type="INT" points="1" offset="66" VALUE="0" />
                //    <Item name="MaxSlotCount" type="INT" points="1" offset="67" VALUE="0" />
                //</Event>

                #endregion

                allSlotExistInfo = trxExistInfo.EventGroups[1].Events[0].Items["JobExistence"].Value;

                #region [ 判斷是否為空 ]

                if (allSlotExistInfo.Trim() == string.Empty)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) JobExistInfo({6}) Fail! Reason(Job Exist Info is Empty)!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxExistInfo.Metadata.Name, allSlotExistInfo);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    return ePortJobUDRQReason.OTHERS;
                }

                #endregion

                #region [ Check Slot Lenth is Exist ]

                if (slotKey > allSlotExistInfo.Trim().Length)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) JobExistInfo({6}) to Check SlotNo({7}) Exist Fail! Reason(Job Exist Info can not find this SlotNo)!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxExistInfo.Metadata.Name, allSlotExistInfo, slotKey);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    return ePortJobUDRQReason.OTHERS;
                }

                #endregion

                #endregion

                //SlotKey從1開始 ,陣列從0開始
                job_ExistInfo = int.Parse(allSlotExistInfo.Substring(slotKey - 1, 1));

                #region [ Port#XXJobEachCassetteSlotPositionBlock Trx Structure ]

                //<trx name="L2_Port#01JobEachCassetteSlotPositionBlock" triggercondition="none">
                //    <eventgroup name="L2_EG_Port#01JobEachCassetteSlotPositionBlock" dir="E2B">
                //        <event name="L2_W_Port#01JobEachCassetteSlotPositionBlock" trigger="true" />
                //    </eventgroup>
                //</trx>

                //<event name="L2_W_Port#01JobEachCassetteSlotPositionBlock" devicecode="W" address="0x0001613" points="58">
                //  <itemgroup name="Port#01JobEachCassetteSlotPositionBlock" />
                //</event>

                //<itemgroup name="Port#01JobEachCassetteSlotPositionBlock">
                //  <item name="SlotPosition#001CassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="SlotPosition#001JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="SlotPosition#002CassetteSequenceNo" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="SlotPosition#002JobSequenceNo" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="SlotPosition#003CassetteSequenceNo" woffset="4" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="SlotPosition#003JobSequenceNo" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="INT" />

                jobTrx_GroupName = string.Format("{0}_EG_Port#{1}JobEachCassetteSlotPositionBlock", curStage.Data.NODENO, curStage.Data.STAGEID.PadLeft(2, '0'));
                jobTrx_EventName = string.Format("{0}_W_Port#{1}JobEachCassetteSlotPositionBlock", curStage.Data.NODENO, curStage.Data.STAGEID.PadLeft(2, '0'));
                jobTrx_CstSeqkey = string.Format("SlotPosition#{0}CassetteSequenceNo", slotKey.ToString().PadLeft(3, '0'));
                jobTrx_JobSeqkey = string.Format("SlotPosition#{0}JobSequenceNo", slotKey.ToString().PadLeft(3, '0'));

                #endregion

                job_CstSeq = int.Parse(trxJobInfo.EventGroups[jobTrx_GroupName].Events[jobTrx_EventName].Items[jobTrx_CstSeqkey].Value);
                job_JobSeq = int.Parse(trxJobInfo.EventGroups[jobTrx_GroupName].Events[jobTrx_EventName].Items[jobTrx_JobSeqkey].Value);

                #region [ Check Monitor SlotCSTSeq, SlotJOBSeq , SlotGlassExist ]

                if (job_CstSeq > 0 && job_JobSeq > 0 && job_ExistInfo == ePortSlotExistInfo.JOB_EXIST)
                {

                    #region [ 有帳有料 ]

                    jobKey = string.Format("{0}_{1}", job_CstSeq.ToString(), job_JobSeq.ToString());

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CassetteSequenceNo({7}) JobSequenceNo({8}) GlassExist({9}) JobKey=({10})!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                                                job_JobSeq.ToString(), job_ExistInfo.ToString(), jobKey);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Get Job Info by Slot CstSeq ,JobSeq ]

                    curBcsJob = ObjectManager.JobManager.GetJob(job_CstSeq.ToString(), job_JobSeq.ToString());

                    if (curBcsJob == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Can not Get Job by CassetteSequenceNo({4}) JobSequenceNo({5})!",
                                                   curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                   job_CstSeq.ToString(), job_JobSeq.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return ePortJobUDRQReason.JOB_NOT_INWIP; //Has Glass Exist But Not In WIP

                    }

                    #endregion

                    #region [ 判斷Route StepNo 是否為Complete Step ]

                    //Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_00002 ]
                    fail_ReasonCode = eJob_CheckFail_Reason.Get_CstSlotExistJob_CurStepNo_OutofMaxStepNo;
                    //最後一步尚未執行還是得確認所以是>不是>=
                    //20151014 Modify 大於等於65535則算Complete
                    //if (curBcsJob.RobotWIP.CurStepNo > curBcsJob.RobotWIP.RobotRouteStepList.Count)
                    if (curBcsJob.RobotWIP.CurStepNo >= eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) PortType({5}) But SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) is Process Complete,  CurStepNo({10}) >= Complete StepNo({11}) not In Process!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    curStage.Data.CASSETTETYPE, curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo,
                                                    curBcsJob.JobSequenceNo, job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_00002 ]

                        if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) PortType({5}) But SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) is Process Complete,  CurStepNo({10}) >= Complete StepNo({11}) not In Process!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    curStage.Data.CASSETTETYPE, curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo,
                                                    curBcsJob.JobSequenceNo, job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Job FailMsg ]

                            //failMsg = string.Format("Robot({0}) StageNo({1}) StageName({2}) StageCSTType({3}) PortType({4}) But SlotNo({5}) Job({6},{7}) Exist({8}) is Process Complete , CurStepNo({9}) >= Complete StepNo({10})",
                            //                        curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, curStage.Data.CASSETTETYPE,
                            //                        curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //                        job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO);

                            failMsg = string.Format("StageNo({0}) StageName({1}) StageCSTType({2}) PortType({3}) But SlotNo({4}) Job({5}_{6}) Exist({7}) is Process Complete , CurStepNo({8}) >= Complete StepNo({9})",
                                                    curStage.Data.STAGEID, curStage.Data.STAGENAME, curStage.Data.CASSETTETYPE,
                                                    curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO);

                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion

                        }

                        #endregion

                        return ePortJobUDRQReason.OTHERS;
                    }
                    else
                    {
                        //Clear[ Job_Fail_Case_00002 ]
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                    }

                    #endregion

                    #region [ 判斷Route是否為有效Route 如果找不到Route則不可列入可控制Joblist ]

                    RobotRouteStep curRouteStepInfo = null;
                    if (curBcsJob.RobotWIP.RobotRouteStepList == null)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Route Step List Is Null  WIP Create Is Failed , can not get RouteInfo!",
                                                   curStage.Data.NODENO, curStage.Data.ROBOTNAME);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                    }

                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == true)
                    {
                        curRouteStepInfo = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
                    }

                    //Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_00001 ]
                    fail_ReasonCode = eJob_CheckFail_Reason.Get_CstSlotExistJob_Route_Is_Fail;

                    if (curRouteStepInfo == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) can not get RouteInfo!",
                                                   curStage.Data.NODENO, curStage.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                   curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_00001 ]

                        if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) can not get RouteInfo!",
                                                   curStage.Data.NODENO, curStage.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                   curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Job FailMsg ]

                            //failMsg = string.Format("[{0}]Robot({1}) Job({2},{3}) curStepNo({4}) can not get RouteInfo!",
                            //                        MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //                        curBcsJob.RobotWIP.CurStepNo.ToString());

                            failMsg = string.Format("Job({0}_{1}) curStepNo({2}) can not get RouteInfo!",
                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion

                        }

                        #endregion

                        return ePortJobUDRQReason.CANNOT_FIND_ROUTE;
                    }
                    else
                    {
                        //Clear[ Job_Fail_Case_00001 ]
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                    }

                    #endregion

                    #region [ Update RobotJob WIP ]

                    //1Arm 2Job 需要設定RobotCmdSlotNo,SubLocation
                    tmpRobotCmdSlotNo = GetPortTypeStageRobotCmdSlotNo(slotKey);
                    tmpSubLocation = GetPortTypeStageSubLocation(slotKey);

                    //Update Port Job SendOut時的CST Status 以供排序 InProcess > WaitForProcess
                    if (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)
                    {
                        tmpPortCstStatusPriority = eLoaderPortSendOutStatus.PORT_WAIT_PROCESS;
                    }
                    else
                    {
                        tmpPortCstStatusPriority = eLoaderPortSendOutStatus.PORT_IN_PROCESS;
                    }

                    //Update Robot Job WIP條件 Location變化 , Location Cst Status Priority變化 , SendOutJob Grade變化(Equipment Type時要抓LinkSignal 上報的SendOut JobData內的Grade而不是WIP Grade)
                    if (curBcsJob.RobotWIP.CurLocation_StageID != curStage.Data.STAGEID ||
                        curBcsJob.RobotWIP.CurLocation_SlotNo != slotKey ||
                        curBcsJob.RobotWIP.CurLocation_StageType != eRobotStageType.PORT ||
                        curBcsJob.RobotWIP.CurPortCstStatusPriority != tmpPortCstStatusPriority ||
                        curBcsJob.RobotWIP.CurSendOutJobJudge != curBcsJob.JobJudge ||
                        curBcsJob.RobotWIP.CurSubLocation != tmpSubLocation ||
                        curBcsJob.RobotWIP.CurRobotCmdSlotNo != tmpRobotCmdSlotNo)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] CassetteSequenceNo({1}) JobSequenceNo({2}) Update RobotJobWIP curStageNo from ({3}) to ({4}), curSlotNo from ({5}) to ({6}) ,curStageType from ({7}) to ({8}), PortCSTStatusPriority from ({9}) to ({10}), sendOutJobJudge from ({11}) to ({12}), curSubLocation from ({13}) to ({14}), curRobotCmdSlotNo from ({15}) to ({16})!",
                                                curStage.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurLocation_StageID,
                                                curStage.Data.STAGEID, curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(), slotKey.ToString(), curBcsJob.RobotWIP.CurLocation_StageType,
                                                eRobotStageType.PORT, curBcsJob.RobotWIP.CurPortCstStatusPriority, tmpPortCstStatusPriority, curBcsJob.RobotWIP.CurSendOutJobJudge,
                                                curBcsJob.JobJudge, curBcsJob.RobotWIP.CurSubLocation, tmpSubLocation, curBcsJob.RobotWIP.CurRobotCmdSlotNo.ToString(), tmpRobotCmdSlotNo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurLocation_StageID = curStage.Data.STAGEID;
                            curBcsJob.RobotWIP.CurLocation_SlotNo = slotKey;
                            curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.PORT;
                            curBcsJob.RobotWIP.CurPortCstStatusPriority = tmpPortCstStatusPriority;
                            curBcsJob.RobotWIP.CurSendOutJobJudge = curBcsJob.JobJudge;
                            curBcsJob.RobotWIP.CurSubLocation = tmpSubLocation;
                            curBcsJob.RobotWIP.CurRobotCmdSlotNo = tmpRobotCmdSlotNo;
                        }

                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    //findBcsJob = curBcsJob;
                    return ePortJobUDRQReason.REASON_OK;

                    #endregion

                    #endregion

                }
                //20151126 暫時Mark 等候IO 更新[ Wait_Proc ]
                //else if (job_CstSeq > 0 && job_JobSeq > 0 && job_ExistInfo == ePortSlotExistInfo.JOB_NO_EXIST)
                //{

                //    #region [ 有帳無料 Has Job Info but No Exist ]

                //    #region  [DebugLog]

                //    if (IsShowDetialLog == true)
                //    {

                //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(has JobInfo but not exist)!",
                //                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                //                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                //                                job_JobSeq.ToString(), job_ExistInfo.ToString());

                //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                //    }

                //    #endregion

                //    return ePortJobUDRQReason.JOBINFO_EXIST_JOB_NOT_EXIST;

                //    #endregion

                //}
                //else if (job_CstSeq == 0 && job_JobSeq == 0 && job_ExistInfo == ePortSlotExistInfo.JOB_EXIST)
                //{

                //    #region [ 無帳有料 Has Job Exist but No Job Info ]

                //    #region  [DebugLog]

                //    if (IsShowDetialLog == true)
                //    {

                //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(Job Exist but no JobInfo)!",
                //                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                //                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                //                                job_JobSeq.ToString(), job_ExistInfo.ToString());

                //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                //    }

                //    #endregion

                //    return ePortJobUDRQReason.JOBINFO_NOT_EXIST_JOB_EXIST;

                //    #endregion

                //}
                //20151126 暫時不判斷Job Exist信號 等候IO 更新[ Wait_Proc ]
                //else if (job_CstSeq == 0 && job_JobSeq == 0 && job_ExistInfo == ePortSlotExistInfo.JOB_NO_EXIST)
                else if (job_CstSeq == 0 && job_JobSeq == 0)    //怕設備只有一開始更新JobExistence,所以不卡,job_CstSeq=0,job_JobSeq=0,就視同沒片,無帳無料
                {

                    #region [ 無帳無料 No Job Info and Job not Exist ]

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(No JobInfo and Job not Exist)!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    return ePortJobUDRQReason.IS_EMPTY_SLOT;

                    #endregion

                }
                else //剩下的,有帳無料
                {
                    //20160617
                    fail_ReasonCode = eJob_CheckFail_Reason.Get_CstSlotExistJob_CheckFetchOut_Condition_Fail;                 

                    #region [ Get Job Info by Slot CstSeq ,JobSeq ]

                    curBcsJob = ObjectManager.JobManager.GetJob(job_CstSeq.ToString(), job_JobSeq.ToString());

                    if (curBcsJob == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Can not Get Job by CassetteSequenceNo({4}) JobSequenceNo({5})!",
                                                   curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                   job_CstSeq.ToString(), job_JobSeq.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return ePortJobUDRQReason.JOB_NOT_INWIP; //Has Glass Exist But Not In WIP

                    }
                    #endregion

                    //fail_ReasonCode = string.Format("Job({0}_{1}) Glass is not exist,but Job data is exist in cst,please check Job data is not clear)!",job_CstSeq.ToString(), job_JobSeq.ToString());

                    #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_00003 ]
                    if (job_CstSeq > 0 && job_JobSeq > 0) //殘帳
                    {
                        if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(Job Data(JobEachCassetteSlotPosition) status is Illegal!Glass is not exist,but Job data is exist in cst,please check 1.Job data is not clear)!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                                                    job_JobSeq.ToString(), job_ExistInfo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            #region [ 記錄Fail Msg To OPI and Job FailMsg ]

                            //failMsg = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(Job Data(JobEachCassetteSlotPosition) status is Illegal!Glass is not exist,but Job data is exist in cst,please check 1.Job data is not clear)!",
                            //                        curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                            //                        curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                            //                        job_JobSeq.ToString(), job_ExistInfo.ToString());

                            failMsg = string.Format("RtnCode({0}) RtnMsg({1})", fail_ReasonCode, "StageNo({0}) StageName({1}) StageCSTType({2}) Get trx by TrxID({3}) SlotNo({4}) CSTSeq({5}) JobSeq({6}) GlassExist({7}) Reason(Job Data(JobEachCassetteSlotPosition) status is Illegal!Glass is not exist,but Job data is exist in cst,please check 1.Job data is not clear)!",
                                                    curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                                                    job_JobSeq.ToString(), job_ExistInfo.ToString());

                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion

                        }
                    }
                    //else
                    //{
                    //    //Clear[ Job_Fail_Case_00003 ]
                    //    RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                    //}

                    #endregion

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(Job Data(JobEachCassetteSlotPosition) status is Illegal!Glass is not exist,but Job data is exist,please check 1.Job data is not clear 2.Glass Exist and Job Data are not Mapping)!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    return ePortJobUDRQReason.OTHERS;

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return ePortJobUDRQReason.IS_EXCEPTION;
            }
        }

        ///// <summary>Update Stage Status 相關資訊 for Cell Special 1Arm2Job Get,Put. 單Arm 2片.最多4片. 01=Left Front , 02=Left Front , 03=Right Fornt , 04=Right Back
        ///// 
        ///// </summary>
        ///// <param name="curStage"></param>
        ///// <param name="newStageStatus"></param>
        ///// <param name="funcName"></param>
        ///// <param name="sendOutCstSeq01"></param>
        ///// <param name="sendOutJobSeq01"></param>
        ///// <param name="sendOutCstSeq02"></param>
        ///// <param name="sendOutJobSeq02"></param>
        ///// <param name="sendOutCstSeq03"></param>
        ///// <param name="sendOutJobSeq03"></param>
        ///// <param name="sendOutCstSeq04"></param>
        ///// <param name="sendOutJobSeq04"></param>
        //protected void UpdateStageStatus_for1Arm2Job(RobotStage curStage, string newStageStatus, string funcName,
        //                                             string sendOutCstSeq01, string sendOutJobSeq01, string sendOutCstSeq02, string sendOutJobSeq02,
        //                                             string sendOutCstSeq03, string sendOutJobSeq03, string sendOutCstSeq04, string sendOutJobSeq04)
        //{

        //    string strlog = string.Empty;

        //    try
        //    {

        //        #region [ add Check Send Out 1st RobotCmdSlotNo Job is Change for Monitor CST SlotInfo Change ]

        //        //bool sendOutJobChangeFlag = false;
        //        //string new1stSlotSendOutCstSeq = string.Empty;
        //        //string new1stSlotSendOutJobSeq = string.Empty;

        //        //if ((curStage.File.CurStageStatus == newStageStatus) &&
        //        //    (curStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY ||
        //        //     curStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY))
        //        //{
        //        //    //當Status沒有變化且是 Send or  Send_Recive時的處理
        //        //    if (curStage.curUDRQ_SlotList.Count > 0)
        //        //    {
        //        //        string old1stSendOutJobKey = string.Empty;

        //        //        //Find 1st Job Key
        //        //        if (curStage.File.CurSendOut_CSTSeq == "0" && curStage.File.CurSendOut_JobSeq == "0")
        //        //        {
        //        //            //front Slot is Empty, Use Back Slot
        //        //            old1stSendOutJobKey = string.Format("{0}_{1}", curStage.File.CurSendOut_CSTSeq02, curStage.File.CurSendOut_JobSeq02);
        //        //        }
        //        //        else
        //        //        {
        //        //            old1stSendOutJobKey = string.Format("{0}_{1}", curStage.File.CurSendOut_CSTSeq, curStage.File.CurSendOut_JobSeq);
        //        //        }

        //        //        foreach (int slotNo in curStage.curUDRQ_SlotList.Keys)
        //        //        {
        //        //            //判斷目前1st SendOut SlotNo 內容是否與之前最後紀錄的1st SendOut的job 相同, 如果不同則要更新.因為是一組一組搬送.所以只確認第一片即可
        //        //            if (curStage.curUDRQ_SlotList[slotNo] != string.Empty && curStage.curUDRQ_SlotList[slotNo] != old1stSendOutJobKey)
        //        //            {
        //        //                string[] newSendOutJobInfo = curStage.curUDRQ_SlotList[slotNo].Split('_');

        //        //                if (newSendOutJobInfo.Length > 1)
        //        //                {
        //        //                    new1stSlotSendOutCstSeq = newSendOutJobInfo[0];
        //        //                    new1stSlotSendOutJobSeq = newSendOutJobInfo[1];
        //        //                    sendOutJobChangeFlag = true;
        //        //                }
        //        //            }
        //        //            break;
        //        //        }

        //        //    }

        //        //}

        //        #endregion

        //        #region [ Stage Status Change 才需要Update ]

        //        if (curStage.File.CurStageStatus != newStageStatus)
        //        {

        //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) status change from ({4}) to ({5}), UDRQ 1st Job CassetteSequenceNo({6}) JobSequenceNo({7}), 2nd Job CassetteSequenceNo({8}) JobSequenceNo({9}), 3rd Job CassetteSequenceNo({10}) JobSequenceNo({11}), 4th Job CassetteSequenceNo({12}) JobSequenceNo({13}) by [{14}].",
        //                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
        //                                    curStage.Data.STAGENAME, curStage.File.CurStageStatus, newStageStatus, sendOutCstSeq01,
        //                                    sendOutJobSeq01, sendOutCstSeq02, sendOutJobSeq02, sendOutCstSeq03,
        //                                    sendOutJobSeq03, sendOutCstSeq04, sendOutJobSeq04, funcName);

        //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //            //for Log Quick Trace
        //            strlog = string.Format("[{0}] {1} - {2}({3}) Stage Status Change From({4}) to ({5}) ,sendOut 1st Job({6},{7}), 2nd Job({8},{9}), 3rd Job({10},{11}), 2nd Job({12},{13}) by [{14}]",
        //                                    "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
        //                                    MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
        //                                    curStage.Data.STAGENAME,
        //                                    curStage.Data.STAGEID, curStage.File.CurStageStatus, newStageStatus, sendOutCstSeq01,
        //                                    sendOutJobSeq01, sendOutCstSeq02, sendOutJobSeq02, sendOutCstSeq03,
        //                                    sendOutJobSeq03, sendOutCstSeq04, sendOutJobSeq04, funcName);

        //            Logger.LogTrxWrite(this.LogName, strlog);

        //            #region [ Update Robot Stage Entity ]

        //            lock (curStage.File)
        //            {
        //                curStage.File.CurStageStatus = newStageStatus;
        //                curStage.File.CurSendOut_CSTSeq = sendOutCstSeq01;
        //                curStage.File.CurSendOut_JobSeq = sendOutJobSeq01;
        //                curStage.File.CurSendOut_CSTSeq02 = sendOutCstSeq02;
        //                curStage.File.CurSendOut_JobSeq02 = sendOutJobSeq02;
        //                curStage.File.CurSendOut_CSTSeq03 = sendOutCstSeq03;
        //                curStage.File.CurSendOut_JobSeq03 = sendOutJobSeq03;
        //                curStage.File.CurSendOut_CSTSeq04 = sendOutCstSeq04;
        //                curStage.File.CurSendOut_JobSeq04 = sendOutJobSeq04;
        //                curStage.File.StatusChangeFlag = true;
        //            }

        //            #endregion

        //        }

        //        #endregion

        //        #region  [DebugLog]

        //        if (IsShowDetialLog == true)
        //        {

        //            //Get Current Stage Info To Log
        //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) status is ({4}) , UDRQ 1st Job CassetteSequenceNo({5}) JobSequenceNo({6}), 2nd Job CassetteSequenceNo({7}) JobSequenceNo({8}), 3rd Job CassetteSequenceNo({9}) JobSequenceNo({10}), 4th Job CassetteSequenceNo({11}) JobSequenceNo({12}) by [{13}].",
        //                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
        //                                    curStage.Data.STAGENAME, newStageStatus, sendOutCstSeq01, sendOutJobSeq01,
        //                                    sendOutCstSeq02, sendOutJobSeq02, sendOutCstSeq03,
        //                                    sendOutJobSeq03, sendOutCstSeq04, sendOutJobSeq04, funcName);

        //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //        }

        //        #endregion

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary> Stage Change時更新狀態 for Cell Special 1Arm2Job PortType Stage 
        /// 
        /// </summary>
        /// <param name="curStage"></param>
        /// <param name="newStageStatus"></param>
        /// <param name="funcName"></param>
        protected void UpdateStageStatus_for1Arm2Job(RobotStage curStage, string newStageStatus, string funcName)
        {

            string strlog = string.Empty;

            try
            {

                #region [ Stage Status Change 才需要Update ]

                if (curStage.File.CurStageStatus != newStageStatus)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) status change from ({4}) to ({5}) by [{6}].",
                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                            curStage.Data.STAGENAME, curStage.File.CurStageStatus, newStageStatus, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //for Log Quick Trace
                    strlog = string.Format("[{0}] {1} - {2}({3}) Stage Status Change From({4}) to ({5}) by [{6}]",
                                            "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                            MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                            curStage.Data.STAGENAME,
                                            curStage.Data.STAGEID, curStage.File.CurStageStatus, newStageStatus, funcName);

                    Logger.LogTrxWrite(this.LogName, strlog);

                    #region [ Update Robot Stage Entity ]

                    lock (curStage.File)
                    {
                        curStage.File.CurStageStatus = newStageStatus;
                        curStage.File.StatusChangeFlag = true;
                    }

                    #endregion

                }

                #endregion

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {

                    //Get Current Stage Info To Log
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) status is ({4}) by [{5}].",
                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                            curStage.Data.STAGENAME, newStageStatus, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> Port Type Stage 根據Job CurSlotNo來判斷RobotCmdSlotNo.以奇數為主
        /// 
        /// </summary>
        /// <param name="curSlotNo"></param>
        /// <returns></returns>
        private int GetPortTypeStageRobotCmdSlotNo(int curSlotNo)
        {
            int rtnSlotNo = 0;

            try
            {
                if (curSlotNo > 0)
                {
                    //Robot Command 在Port裡面是 001,002 =001 ,  105,106 =105 等組合
                    if (curSlotNo % 2 == 0)
                    {
                        //為偶數
                        rtnSlotNo = curSlotNo - 1;
                    }
                    else
                    {
                        //為奇數
                        rtnSlotNo = curSlotNo;
                    }
                }

                return rtnSlotNo;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return rtnSlotNo;
            }

        }

        /// <summary> Port Type Stage 根據Job CurSlotNo來判斷SubLocation.奇數為1(Front) 偶數為2(Back)
        /// 
        /// </summary>
        /// <param name="curSlotNo"></param>
        /// <returns></returns>
        private string GetPortTypeStageSubLocation(int curSlotNo)
        {
            string rtnSubLocation = string.Empty;

            try
            {
                if (curSlotNo > 0)
                {
                    //Robot Command 在Port裡面是 001,002 =001為1(Front) 002為2(Back)
                    if (curSlotNo % 2 == 0)
                    {
                        //為偶數
                        rtnSubLocation = eRobotCommonConst.ROBOT_ARM_BACK_LOCATION;
                    }
                    else
                    {
                        //為奇數
                        rtnSubLocation = eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION;
                    }
                }

                return rtnSubLocation;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return rtnSubLocation;
            }

        }

        /// <summary> 取得CST內共有多少個Column
        /// 
        /// </summary>
        /// <param name="curStage"></param>
        /// <returns></returns>
        private int GetPortTypeStageCSTColumnCount(RobotStage curStage)
        {
            int rtnCSTColumnCount = 0;

            try
            {
                if (curStage.Data.SLOTMAXCOUNT >= eRobotCommonConst.CELL_1ARM2JOB_ONE_COLUMN_COUNT)
                {
                    rtnCSTColumnCount = curStage.Data.SLOTMAXCOUNT / eRobotCommonConst.CELL_1ARM2JOB_ONE_COLUMN_COUNT;

                }

                return rtnCSTColumnCount;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return rtnCSTColumnCount;
            }

        }

        //20151223 add for 判斷是否可已加入到Can Control SlotBlock List的條件.同StageID不可出現同一CmdSlotNo
        /// <summary> 判斷是否可已加入到Can Control SlotBlock List的條件.同StageID不可出現同一CmdSlotNo
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curSlotBlockInfo"></param>
        /// <param name="curCanCtlSlotBlockInfoList"></param>
        /// <param name="jobKey"></param>
        /// <param name="funcName"></param>
        /// <returns></returns>
        private bool AddToCanControlSlotBlockInfolistCondition(Robot curRobot, RobotStage curStage, RobotCanControlSlotBlockInfo curSlotBlockInfo,
                                                                List<RobotCanControlSlotBlockInfo> curCanCtlSlotBlockInfoList, string funcName)
        {
            string strlog = string.Empty;

            try
            {
                if (curCanCtlSlotBlockInfoList == null)
                    curCanCtlSlotBlockInfoList = new List<RobotCanControlSlotBlockInfo>();

                //20160119 add 要Check SlotBlock Front/Back Route相同才可加入可控制SlotBlockInfo
                if (CheckSlotBlockInfoJobRouteCondition(curRobot, curSlotBlockInfo, "AddToCanControlSlotBlockInfolistCondition") == false)
                {
                    return false;
                }

                if (curCanCtlSlotBlockInfoList.Count == 0)
                {
                    curCanCtlSlotBlockInfoList.Add(curSlotBlockInfo);
                }
                else
                {
                    RobotCanControlSlotBlockInfo duplicateSlotBlockInfo = curCanCtlSlotBlockInfoList.FirstOrDefault(j =>( j.CurBlock_Location_StageID == curSlotBlockInfo.CurBlock_Location_StageID && j.CurBlock_RobotCmdSlotNo == curSlotBlockInfo.CurBlock_RobotCmdSlotNo));

                    if (duplicateSlotBlockInfo != null)
                    {
                        curCanCtlSlotBlockInfoList.Remove(duplicateSlotBlockInfo);
                        curCanCtlSlotBlockInfoList.Add(curSlotBlockInfo);

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}]Robot({2}) add curStageID({3}) curCmdSlotNo({4}) to Can Control SlotBlockInfo list is duplicate! duplicateInfo is removed!",
                                                    curStage.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curSlotBlockInfo.CurBlock_Location_StageID, curSlotBlockInfo.CurBlock_RobotCmdSlotNo);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                    }
                    else
                    {
                        curCanCtlSlotBlockInfoList.Add(curSlotBlockInfo);

                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }


        /// <summary>
        /// 確認Unload Port Slot是否有空Slot存在並更新Stage Status
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curPort"></param>
        private void Get_UnloadPort_StageStatueInfo_For1Arm2Job(Robot curRobot, RobotStage curStage, Port curPort)
        {
            string jobKey = string.Empty;
            string trxID = string.Empty;
            string strlog = string.Empty;
            string tmpStageStatus = string.Empty;
            string tmpCstStatusPriority = string.Empty;
            bool findEmptySlotFlag = false;//當任何一個Column LDRQ時則整個CST都可LDRQ

            try
            {
                //預設為NoReq
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;
                #region Check Cassette Type
                if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) CassetteType({4}) CassetteType can not be RANDON_CST",
                                               curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, curStage.Data.CASSETTETYPE);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    UpdateUnloadStageStatus(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);
                    return;
                }
                #endregion

                #region [ Set Unload and Both Port Receive Job Priority ]

                if (curPort.File.Type == ePortType.BothPort)
                {
                    if (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)
                    {
                        tmpCstStatusPriority = eUnloadPortReceiveStatus.BOTH_PORT_WAIT_PROCESS;
                    }
                    else
                    {
                        tmpCstStatusPriority = eUnloadPortReceiveStatus.BOTH_PORT_IN_PROCESS;
                    }
                }
                else
                {
                    if (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)
                    {
                        tmpCstStatusPriority = eUnloadPortReceiveStatus.ULD_PORT_WAIT_PROCESS;
                    }
                    else
                    {
                        tmpCstStatusPriority = eUnloadPortReceiveStatus.ULD_PORT_IN_PROCESS;
                    }
                }

                #endregion

                #region  [ Real time Get Port Slot Job Info Trx ]
                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotPositionBlock               
                trxID = GetTrx_CSTSlotJobEachPosition(curStage.Data.NODENO, curStage.Data.STAGEID);
                Trx get_CSTSlot_JobInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                if (get_CSTSlot_JobInfo_Trx == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not Get TrxID({4})!",
                                               curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                               trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                    return;
                }
                #endregion

                //根據Robot設定的放片順序來決定要從Slot01開始放在還是SlotMax開始放 ASC 从小到大(Priority 01>02>03>…) ,DESC 从大到小 (Priority 01<02<03>…), default : ASC 从小到大          
                if (curRobot.Data.SLOTSTORESEQ == eRobotCommonConst.DB_ORDER_BY_DESC)
                {
                    #region [ 放片順序為DEC SlotNo由大到小 ]
                    for (int i = curStage.Data.SLOTMAXCOUNT; i > 0; i--)
                    {
                        if ((i % 2) == 0)
                            continue;

                        #region [ Check Unlaod Port Status LDRQ ]
                        if (Check_CMDSlot_IsEmpty_For1Arm2Job(curStage, get_CSTSlot_JobInfo_Trx, i) == true)
                        {
                            //此時已經將SlotNo更新到Stage內
                            findEmptySlotFlag = true;
                        }
                        else
                        {
                            #region [ 目前Block不是Empty Block的處理 ]
                            if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                            {
                                #region [ CSType不為循序,可以選擇下一個Slot判斷是否為空Slot ]
                                //一旦發現不是空Slot 則判斷Flag是否在之前的Slot內已經找到Empty Slot
                                if (findEmptySlotFlag == true)
                                {
                                    //已找到有Empty Slot 則Status=LDRQ and Update Stage Status
                                    tmpStageStatus = eRobotStageStatus.RECEIVE_READY;
                                    UpdateUnloadStageStatus(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);
                                    return;
                                }
                                #endregion
                            }
                            else
                            {
                                #region [ CSTType為循序,清空之前的DicEmptySlotBlockInfo並繼續選擇下一個Slot ]
                                

                                //20160602
                                if (Workbench.LineType == eLineType.CELL.CCSOR || Workbench.LineType == eLineType.CELL.CCCHN || Workbench.LineType == eLineType.CELL.CCRWT || Workbench.LineType == eLineType.CELL.CCCRP || Workbench.LineType == eLineType.CELL.CCCRP_2)
                                {
                                    Dictionary<int, CellSlotBlock> curLDRQ_EmptySlotBlockInfoRemoveList = new Dictionary<int,CellSlotBlock>();
                                    foreach(var pair in curStage.curLDRQ_EmptySlotBlockInfoList)
                                    {
                                        curLDRQ_EmptySlotBlockInfoRemoveList.Add(pair.Key,pair.Value);
                                    }
                                    int _RowPriority = i % 100;
                                    if (curStage.curLDRQ_EmptySlotBlockInfoList.Count != 0)
                                    {
                                        foreach (var pair in curLDRQ_EmptySlotBlockInfoRemoveList) //把同一列的非空slot的上面不能放,例如495非空,則497,499不能放
                                        {
                                            if ((pair.Key % 100) > _RowPriority && pair.Key > i && (pair.Key - (pair.Key % 100)) == (i - _RowPriority))
                                            {
                                                curStage.curLDRQ_EmptySlotBlockInfoList.Remove(pair.Key);
                                            }
                                        }


                                    }
                                }
                                else
                                {
                                    //一旦發現不是空SlotBlockInfo則直接清空重新找EmptySlotBlock
                                    curStage.curLDRQ_EmptySlotBlockInfoList.Clear();
                                }


                                findEmptySlotFlag = false;

                                //if (findEmptySlotFlag == true)
                                //{
                                //    tmpStageStatus = eRobotStageStatus.RECEIVE_READY;
                                //    UpdateUnloadStageStatus(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);
                                //    return;
                                    
                                //}
                                //else
                                //{
                                //    tmpStageStatus = eRobotStageStatus.NO_REQUEST;
                                //    //非Empty Slot 則視為NOREQ 並將Priority設為Others
                                //    UpdateUnloadStageStatus(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);
                                //    return;
                                //}

                                #endregion
                            }
                            #endregion
                        }
                        #endregion
                    }

                    #region [ 處理最後一各Slot的判斷結果 ]
                    //當最後一片才找到Empty CST時要更新Stage Stage
                    if (findEmptySlotFlag == true)
                    {
                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;
                        UpdateUnloadStageStatus(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);
                    }
                    else
                    {
                        //最後一片非Empty Slot 則視為NOREQ 並將Priority設為Others
                        UpdateUnloadStageStatus(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);
                    }
                    #endregion
                    #endregion
                }
                else
                {
                    int tmpCstColumnCount = 0;
                    #region [ Get CST Column Count ]
                    tmpCstColumnCount = GetPortTypeStageCSTColumnCount(curStage);
                    if (tmpCstColumnCount < 1)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) MaxSlotNo({4}) Cell Column Max Count({5}) can not Get Column!",
                                                   curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                   curStage.Data.SLOTMAXCOUNT.ToString(), eRobotCommonConst.CELL_1ARM2JOB_ONE_COLUMN_COUNT.ToString());
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion
                        return;//計算不出Column則不要取該CST以免出問題
                    }
                    #endregion
                    #region [ by Column 取得 LDRQ SlotBlock ]
                    for (int columnIdx = 1; columnIdx <= tmpCstColumnCount; columnIdx++)
                    {
                        int tmpMinSlotNo = ((columnIdx - 1) * eRobotCommonConst.CELL_1ARM2JOB_ONE_COLUMN_COUNT) + 1;
                        int tmpMaxSlotNo = ((columnIdx - 1) * eRobotCommonConst.CELL_1ARM2JOB_ONE_COLUMN_COUNT) + eRobotCommonConst.CELL_1ARM2JOB_ONE_COLUMN_COUNT;

                        for (int slot_no = tmpMinSlotNo; slot_no <= tmpMaxSlotNo; slot_no++)
                        {
                            if (slot_no % 2 == 0)
                                continue;

                            #region [ Check Unlaod Port Status LDRQ ]
                            if (Check_CMDSlot_IsEmpty_For1Arm2Job(curStage, get_CSTSlot_JobInfo_Trx, slot_no) == true)
                            {
                                //此時已經將SlotNo更新到Stage內
                                findEmptySlotFlag = true;
                            }
                            else
                            {
                                break;
                            }
                            #endregion
                        }
                    }
                    #region [ 目前Block不是Empty Block的處理 ]
                    if (findEmptySlotFlag == true)
                    {
                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;
                        UpdateUnloadStageStatus(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);
                        return;
                    }
                    else
                    {
                        tmpStageStatus = eRobotStageStatus.NO_REQUEST;
                        //非Empty Slot 則視為NOREQ 並將Priority設為Others
                        UpdateUnloadStageStatus(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);
                        return;
                    }
                    #endregion
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 判斷Slot是否為空Slot並更新Stage的 LDRQ Empty SlotNo
        /// </summary>
        /// <param name="curStage"></param>
        /// <param name="trxExistInfo"></param>
        /// <param name="trxJobInfo"></param>
        /// <param name="slotKey"></param>
        /// <param name="curRobotJobList"></param>
        /// <returns></returns>
        private bool Check_CMDSlot_IsEmpty_For1Arm2Job(RobotStage curStage, Trx trxJobInfo, int cmdSlotNo)
        {
            string strlog = string.Empty;
            try
            {
                #region cmdSlotNo 必須是單數
                if ((cmdSlotNo % 2) == 0)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("cmdSlotNo({0}) must be an odd integer", cmdSlotNo);
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }
                #endregion

                #region [ Check Slot Lenth is Exist ]
                if ((cmdSlotNo + 1) * 2 > trxJobInfo.EventGroups[0].Events[0].Items.Count)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) to Check SlotNo({6}) Exist Fail! Reason(Job Position Info can not find this SlotNo)!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Name, cmdSlotNo);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }
                #endregion

                #region [ Port#XXJobEachCassetteSlotPositionBlock Trx Structure ]

                //<TrxBody name="L2_Port#01JobEachCassetteSlotPositionBlock">
                // <EventGroup name="L2_EG_Port#01JobEachCassetteSlotPositionBlock">
                //  <Event name="L2_W_Port#01JobEachCassetteSlotPositionBlock" device="W" address="0000B6A" points="1200" ENABLE="1">
                //   <Item name="SlotPosition#001CassetteSequenceNo" type="INT" points="1" offset="0" VALUE="0" />
                //   <Item name="SlotPosition#001JobSequenceNo" type="INT" points="1" offset="1" VALUE="0" />
                //   <Item name="SlotPosition#002CassetteSequenceNo" type="INT" points="1" offset="2" VALUE="0" />
                //   <Item name="SlotPosition#002JobSequenceNo" type="INT" points="1" offset="3" VALUE="0" />
                //   .......
                //   <Item name="SlotPosition#600CassetteSequenceNo" type="INT" points="1" offset="1198" VALUE="0" />
                //   <Item name="SlotPosition#600JobSequenceNo" type="INT" points="1" offset="1199" VALUE="0" />
                //  </Event>
                // </EventGroup>
                //</TrxBody>

                #endregion

                string item_front_cst_seq_no = string.Format("SlotPosition#{0}CassetteSequenceNo", cmdSlotNo.ToString().PadLeft(3, '0'));
                string item_front_job_seq_no = string.Format("SlotPosition#{0}JobSequenceNo", cmdSlotNo.ToString().PadLeft(3, '0'));
                string item_back_cst_seq_no = string.Format("SlotPosition#{0}CassetteSequenceNo", (cmdSlotNo + 1).ToString().PadLeft(3, '0'));
                string item_back_job_seq_no = string.Format("SlotPosition#{0}JobSequenceNo", (cmdSlotNo + 1).ToString().PadLeft(3, '0'));

                //20160602
                int rowsPriority = cmdSlotNo % 100;  //為了後續要排序599,499,399....橫向找Empty Slot放片

                CellSlotBlock block = new CellSlotBlock(
                    trxJobInfo.EventGroups[0].Events[0].Items[item_front_cst_seq_no].Value,
                    trxJobInfo.EventGroups[0].Events[0].Items[item_front_job_seq_no].Value,
                    trxJobInfo.EventGroups[0].Events[0].Items[item_back_cst_seq_no].Value,
                    trxJobInfo.EventGroups[0].Events[0].Items[item_back_job_seq_no].Value,
                    rowsPriority);
                
                #region [ Check Monitor SlotCSTSeq, SlotJOBSeq , SlotGlassExist ]
                if (!block.FrontJobExist && !block.BackJobExist)
                {
                    #region [ 無帳 ]
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) SlotBlock is Empty!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, cmdSlotNo.ToString().PadLeft(3, '0'));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    //Update Current Stage LDRQ Empty Slot
                    lock (curStage)
                    {
                        //add Empty SlotNo To EmptySlotNoList
                        if (curStage.curLDRQ_EmptySlotBlockInfoList.ContainsKey(cmdSlotNo) == false)
                        {
                            curStage.curLDRQ_EmptySlotBlockInfoList.Add(cmdSlotNo, block);
                        }
                    }
                    return true;
                    #endregion
                }
                else
                {
                    #region [ 有帳 ]
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) CmdSlotNo({6}) FrontSlotJob({7}_{8}) BackSlotJob({9}_{10}) is not Empty!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, cmdSlotNo.ToString().PadLeft(3, '0'), block.FrontCstSeqNo, block.FrontJobSeqNo, block.BackCstSeqNo, block.BackJobSeqNo);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                    return false;
                    #endregion
                }
                #endregion
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
