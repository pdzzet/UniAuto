using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.OpiSpec;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.PLCAgent.PLC;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class JobFilterService : AbstractRobotService
    {

        //Filter Funckey = "FL" + XXXX(序列號) For Cell Special (1Arm2Job)

        /// <summary> for Cell Special Check Current Step Action by Job Location. EX: Location is Arm. Action不可以為GET.
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0033")]
        public bool Filter_CurStepActionByJobLocation_For1Arm2Job(IRobotContext robotConText)
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

                #region [ Check Robot Arm Type ]

                if (curRobot.Data.ARMJOBQTY != 2)
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm2Job);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Defind 1st NormalRobotCommand ]

                DefineNormalRobotCmd cur1stRobotCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.Define_1stNormalRobotCommandInfo];

                //找不到 1st defineNormalRobotCmd 回NG
                if (cur1stRobotCmd == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get defineNormalRobotCmd!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get defineNormalRobotCmd!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Defind 2nd NormalRobotCommand ]

                DefineNormalRobotCmd cur2ndRobotCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.Define_2ndNormalRobotCommandInfo];

                //找不到 2nd defineNormalRobotCmd 回NG
                if (cur2ndRobotCmd == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get 2nd defineNormalRobotCmd!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get 2nd defineNormalRobotCmd!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_2nd_DefineNormalRobotCommandInfo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                string tmpStageID = string.Empty;
                string tmpStepAction = string.Empty;
                int tmpStepNo = 0;
                string funcName = string.Empty;

                #region [ check Step by is2ndCmdFlag ]

                if (is2ndCmdFlag == false)
                {

                    tmpStepNo = curBcsJob.RobotWIP.CurStepNo;
                    funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                }
                else
                {
                    //20151014 Modity NextStep由WIP來取得
                    tmpStepNo = curBcsJob.RobotWIP.NextStepNo;// curBcsJob.RobotWIP.CurStepNo + 1;
                    funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;
                }

                #endregion

                #region [ Get tmp Step Entity ]

                RobotRouteStep tmpRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[tmpStepNo];

                //找不到 CurStep Route 回NG
                if (tmpRouteStep == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Get curRouteStep({5})!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, tmpStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get curRouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo, tmpStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Set Parameter by is2ndCmdFlag ]

                if (is2ndCmdFlag == false)
                {
                    tmpStageID = curBcsJob.RobotWIP.CurLocation_StageID;

                }
                else
                {

                    #region [ by 1st Cmd Define Job Location(curStageID) and ArmInfo(robotArmInfo[2]) ]

                    //SPEC定義[ Wait_Proc_00028 ] 1Arm 2Job要額外處理
                    //0: None      //1: Put          //2: Get
                    //4: Exchange  //8: Put Ready    //16: Get Ready       //32: Get/Put                 
                    switch (cur1stRobotCmd.Cmd01_Command)
                    {
                        case 1:  //PUT
                        case 4:  //Exchange
                        case 32: //Get/Put

                            //Local Stage is Stage
                            tmpStageID = cur1stRobotCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0');
                            break;

                        case 2:  //Get
                        case 8:  //Put Ready
                        case 16: //Get Ready

                            //Local Stage is Stage
                            tmpStageID = eRobotCommonConst.ROBOT_HOME_STAGEID;
                            break;

                        default:

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) ({3}) 1st defineNormalRobotCmd Action({4}) is out of Range!",
                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("[{0}] can not Get 1st defineNormalRobotCmd Action({1}) is out of Range!",
                                                    MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString());

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
                            robotConText.SetReturnMessage(errMsg);

                            errCode = eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail;//add for BMS Error Monitor
                            if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                            return false;

                    }

                    #endregion

                }

                //DB定義 'PUT' / 'GET' / 'PUTREADY' / 'GETREADY'
                tmpStepAction = tmpRouteStep.Data.ROBOTACTION.ToString();

                #endregion

                if (tmpStageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                {
                    #region [ Job在Arm上  Action 只接受PUT,PUTReady,Exchange ]

                    if (tmpStepAction != eRobotCmdActionCode.PUT.ToString() &&
                        tmpStepAction != eRobotCmdActionCode.PUTREADY.ToString() &&
                        tmpStepAction != eRobotCmdActionCode.EXCHANGE.ToString())
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) Action({8}) is illegal!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                                                    tmpStageID, tmpStepNo.ToString(), tmpStepAction);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) Action({6}) is illegal!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                                tmpStageID, tmpStepNo.ToString(), tmpStepAction);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_StepAction_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        errCode = eJobFilter_ReturnCode.NG_ArmJob_StepAction_Is_Fail;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                        return false;

                    }

                    #endregion
                }
                else
                {
                    #region [ Job不在Arm上 Action 只接受GET,GETReady ]

                    if (tmpStepAction != eRobotCmdActionCode.GET.ToString() &&
                        tmpStepAction != eRobotCmdActionCode.GETREADY.ToString())
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) Action({8}) is illegal!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                                                    tmpStageID, tmpStepNo.ToString(), tmpStepAction);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) Action({6}) is illegal!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                                tmpStageID, tmpStepNo.ToString(), tmpStepAction);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NotArmJob_StepAction_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        errCode = eJobFilter_ReturnCode.NG_NotArmJob_StepAction_Is_Fail;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                        return false;

                    }

                    #endregion

                }

                #region [ Set Robot Action To DefindNormalRobotCommand by is2ndCmdFlag ]

                if (is2ndCmdFlag == false)
                {

                    cur1stRobotCmd.Cmd01_Command = GetRobotCommandActionCode(curRobot, curBcsJob, cur1stRobotCmd.Cmd01_DBRobotAction);
                    robotConText.AddParameter(eRobotContextParameter.Define_1stNormalRobotCommandInfo, cur1stRobotCmd);
                }
                else
                {
                    cur2ndRobotCmd.Cmd01_Command = GetRobotCommandActionCode(curRobot, curBcsJob, cur2ndRobotCmd.Cmd01_DBRobotAction);
                    robotConText.AddParameter(eRobotContextParameter.Define_2ndNormalRobotCommandInfo, cur2ndRobotCmd);
                }

                #endregion

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

        #region [ 20160127 之後不啟用 已改寫在SlotBlockInfo 確認的Function內 ]

        ///// <summary>Check SlotBlockInfo JobList curStep Setting UseArm is can use by Job Location. for Cell Special Arm(1Arm2Job). Not On Arm: Use Arm must Empty. On Arm:Only:Use Arm must Exist.
        ///// 
        ///// </summary>
        ///// <param name="robotConText"></param>
        ///// <returns></returns>
        //[UniAuto.UniBCS.OpiSpec.Help("FL0034")]
        //public bool Filter_CurStepUseArmByJobLocation_For1Arm2Job(IRobotContext robotConText)
        //{
        //    string strlog = string.Empty;
        //    string errMsg = string.Empty;

        //    try
        //    {

        //        #region [ Get curRobot Entity ]

        //        Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

        //        //找不到 Robot 回NG
        //        if (curRobot == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
        //                                        "L1", MethodBase.GetCurrentMethod().Name);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] can not Get Robot!",
        //                                    MethodBase.GetCurrentMethod().Name);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get curSlotBlockInfo Entity ]

        //        RobotCanControlSlotBlockInfo curSlotBlockInfo = (RobotCanControlSlotBlockInfo)robotConText[eRobotContextParameter.CurSlotBlockInfoEntity];

        //        //找不到 SlotBlockInfo 回NG
        //        if (curSlotBlockInfo == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get curSlotBlockInfo!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get curSlotBlockInfo!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_SlotBlockInfo_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get curSlotBlockInfo FrontJob and BackJob Entity ]

        //        if(curSlotBlockInfo.CurBlockCanControlJobList.Count == 0)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get curSlotBlockInfo JobInfo!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get curSlotBlockInfo JobInfo!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_SlotBlockInfo_Job_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        Job curFrontJob = null;
        //        Job curBackJob = null;

        //        foreach (Job jobEntity in curSlotBlockInfo.CurBlockCanControlJobList)
        //        {
        //            if (jobEntity.RobotWIP.CurSubLocation == eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION)
        //            {
        //                curFrontJob = jobEntity;

        //            }
        //            if (jobEntity.RobotWIP.CurSubLocation == eRobotCommonConst.ROBOT_ARM_BACK_LOCATION)
        //            {
        //                curBackJob = jobEntity;

        //            }
        //        }

        //        if (curFrontJob == null && curBackJob == null)
        //        {
        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get curSlotBlockInfo Front and Back jobInfo!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get curSlotBlockInfo  Front and Back jobInfo!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_SlotBlockInfo_Job_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ Check Robot Arm Type ]

        //        if (curRobot.Data.ARMJOBQTY != 2)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Arm Job Qty({2}) is illegal!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm2Job);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ Get 2nd Command Check Flag ]

        //        bool is2ndCmdFlag = false;

        //        try
        //        {
        //            is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
        //        }
        //        catch (Exception)
        //        {
        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Set Is2ndCmdCheckFlag!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ Get Defind 1st NormalRobotCommand ]

        //        DefineNormalRobotCmd cur1stRobotCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.Define_1stNormalRobotCommandInfo];

        //        //找不到 1st defineNormalRobotCmd 回NG
        //        if (cur1stRobotCmd == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get defineNormalRobotCmd!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get defineNormalRobotCmd!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get Defind 2nd NormalRobotCommand ]

        //        DefineNormalRobotCmd cur2ndRobotCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.Define_2ndNormalRobotCommandInfo];

        //        //找不到 2nd defineNormalRobotCmd 回NG
        //        if (cur2ndRobotCmd == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get 2nd defineNormalRobotCmd!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get 2nd defineNormalRobotCmd!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_2nd_DefineNormalRobotCommandInfo_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Check UseArm by is2ndCmdFlag(根據是CurStep or NextStep來確認Arm是否可以使用 ]

        //        //DB定義 //'UP':Upper Arm, 'LOW':Lower Arm, 'ANY':Any Arm, 'ALL':All Arm
        //        int tmpFrontStepNo = 0;
        //        string tmpFrontStageID = string.Empty;
        //        int tmpFrontLocation_SlotNo = 0;
        //        string tmpFrontSubLocation = string.Empty;

        //        int tmpBackStepNo = 0;
        //        string tmpBackStageID = string.Empty;
        //        int tmpBackLocation_SlotNo = 0;
        //        string tmpBackSubLocation = string.Empty;

        //        #region [ 取得目前Robot Arm上經過運算的資訊 ]

        //        RobotArmDoubleSubstrateInfo[] tmpRobotArmInfo = new RobotArmDoubleSubstrateInfo[curRobot.CurTempArmDoubleJobInfoList.Length];

        //        string funcName = string.Empty;

        //        //先與目前Arm上運算的資訊做同步
        //        for (int i = 0; i < tmpRobotArmInfo.Length; i++)
        //        {
        //            tmpRobotArmInfo[i] = new RobotArmDoubleSubstrateInfo();
        //            //與Robot同步 
        //            tmpRobotArmInfo[i].ArmFrontJobExist = curRobot.CurTempArmDoubleJobInfoList[i].ArmFrontJobExist;
        //            tmpRobotArmInfo[i].ArmBackJobExist = curRobot.CurTempArmDoubleJobInfoList[i].ArmBackJobExist;
        //            tmpRobotArmInfo[i].ArmDisableFlag = curRobot.CurTempArmDoubleJobInfoList[i].ArmDisableFlag;
        //        }

        //        #endregion

        //        #region [ Get Front/Back Job Step Check Parameter by is2ndCmdFlag ]

        //        if (is2ndCmdFlag == false)
        //        {

        //            #region [ Is 1st Cmd Check ]

        //            funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;

        //            //根據Cur StepNo 定義目前Front/Back Job的位置
        //            if (curFrontJob != null && curBackJob != null)
        //            {
        //                tmpFrontStepNo = curFrontJob.RobotWIP.CurStepNo;
        //                tmpFrontStageID = curFrontJob.RobotWIP.CurLocation_StageID;
        //                tmpFrontLocation_SlotNo = curFrontJob.RobotWIP.CurLocation_SlotNo;
        //                tmpFrontSubLocation = curFrontJob.RobotWIP.CurSubLocation;

        //                tmpBackStepNo = curBackJob.RobotWIP.CurStepNo;
        //                tmpBackStageID = curBackJob.RobotWIP.CurLocation_StageID;
        //                tmpBackLocation_SlotNo = curBackJob.RobotWIP.CurLocation_SlotNo;
        //                tmpBackSubLocation = curBackJob.RobotWIP.CurSubLocation;
        //            }
        //            else if (curFrontJob != null && curBackJob == null)
        //            {
        //                //Only Front
        //                tmpFrontStepNo = curFrontJob.RobotWIP.CurStepNo;
        //                tmpFrontStageID = curFrontJob.RobotWIP.CurLocation_StageID;
        //                tmpFrontLocation_SlotNo = curFrontJob.RobotWIP.CurLocation_SlotNo;
        //                tmpFrontSubLocation = curFrontJob.RobotWIP.CurSubLocation;

        //                //tmpBackStepNo = tmpFrontStepNo;
        //                tmpBackStageID = tmpFrontStageID;
        //                //tmpBackLocation_SlotNo = tmpFrontLocation_SlotNo;
        //                //tmpBackSubLocation = curBackJob.RobotWIP.CurSubLocation;
        //            }
        //            else if (curFrontJob == null && curBackJob != null)
        //            {
        //                //Only Back
        //                tmpBackStepNo = curBackJob.RobotWIP.CurStepNo;
        //                tmpBackStageID = curBackJob.RobotWIP.CurLocation_StageID;
        //                tmpBackLocation_SlotNo = curBackJob.RobotWIP.CurLocation_SlotNo;
        //                tmpBackSubLocation = curBackJob.RobotWIP.CurSubLocation;

        //                //tmpFrontStepNo = curFrontJob.RobotWIP.CurStepNo;
        //                tmpFrontStageID = tmpBackStageID;
        //                //tmpFrontLocation_SlotNo = curFrontJob.RobotWIP.CurLocation_SlotNo;
        //                //tmpFrontSubLocation = curFrontJob.RobotWIP.CurSubLocation;
        //            }

        //            #endregion

        //        }
        //        else
        //        {

        //            #region [ Is 1st Cmd Check. 根據Next StepNo 預測目前1st Cmd後front/Back Job的位置 tmpFrontLocation_SlotNo and tmpBackLocation_SlotNo ]

        //            funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;

        //            if (curFrontJob != null)
        //            {
        //                tmpFrontStepNo = curFrontJob.RobotWIP.NextStepNo;
        //            }

        //            if (curBackJob != null)
        //            {
        //                tmpBackStepNo = curBackJob.RobotWIP.NextStepNo;
        //            }

        //            #region [ by 1st Cmd Define Job Location(curStageID) and ArmInfo(robotArmInfo[4]) ]

        //            //SPEC定義1Arm 2Job要
        //            //0: None      //1: Put          //2: Get
        //            //4: Exchange  //8: Put Ready    //16: Get Ready       //32: Get/Put                 
        //            switch (cur1stRobotCmd.Cmd01_Command)
        //            {
        //                case 1:  //PUT
        //                case 4:  //Exchange
        //                case 32: //Get/Put

        //                    #region [ 1st Cmd 是PUT相關則要清空相對應的Arm資訊 ]

        //                    //有可能只有Front or Back Job 所以只要其中一片存在則同步更新
        //                    if (curFrontJob != null || curBackJob != null)
        //                    {
        //                        //Local Stage is Stage
        //                        tmpFrontStageID = cur1stRobotCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0');
        //                        tmpBackStageID = cur1stRobotCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'); ;
        //                    }

        //                    #region [ by 1st Cmd Use Arm UpDate ArmInfo and tmpLocation_SlotNo ]

        //                    //SPEC定義1Arm 2Job
        //                    //0: None              1: Upper/Left Arm       2: Lower/Left Arm     3: Left Both Arm
        //                    //4: Upper/Right Arm   5: Upper Both Arm       8: Lower/Right Arm   10: Lower Both Arm
        //                    //12: Right Both Arm
        //                    switch (cur1stRobotCmd.Cmd01_ArmSelect)
        //                    {
        //                        case 1: //Upper/Left Arm  Arm#01

        //                            //1st Cmd 是PUT相關則 Arm前後通通要淨空
        //                            tmpRobotArmInfo[0].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[0].ArmBackJobExist = eGlassExist.NoExist;

        //                            break;

        //                        case 2: //Lower/Left Arm  Arm#02

        //                            //1st Cmd 是PUT相關則 Arm前後通通要淨空
        //                            tmpRobotArmInfo[1].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[1].ArmBackJobExist = eGlassExist.NoExist;

        //                            break;

        //                        case 3: //Left Both Arm  Arm#01 & Arm#02


        //                            tmpRobotArmInfo[0].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[1].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[0].ArmBackJobExist = eGlassExist.NoExist;                                       
        //                            tmpRobotArmInfo[1].ArmBackJobExist = eGlassExist.NoExist;

        //                            break;

        //                        case 4: //Upper/Right Arm  Arm#03

        //                            //1st Cmd 是PUT相關則 Arm前後通通要淨空
        //                            tmpRobotArmInfo[2].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[2].ArmBackJobExist = eGlassExist.NoExist;

        //                            break;

        //                        case 5: //Upper Both Arm  Arm#01 & Arm#03

        //                            //1st Cmd 是PUT相關則 Arm前後通通要淨空
        //                            tmpRobotArmInfo[0].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[2].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[0].ArmBackJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[2].ArmBackJobExist = eGlassExist.NoExist;

        //                            break;

        //                        case 8: //Lower/Right Arm Arm#04

        //                            //1st Cmd 是PUT相關則 Arm前後通通要淨空
        //                            tmpRobotArmInfo[3].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[3].ArmBackJobExist = eGlassExist.NoExist;

        //                            break;

        //                        case 10: //Lower Both Arm  Arm#02 & Arm#04

        //                            //1st Cmd 是PUT相關則 Arm前後通通要淨空
        //                            tmpRobotArmInfo[0].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[2].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[0].ArmBackJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[2].ArmBackJobExist = eGlassExist.NoExist;

        //                            break;

        //                        case 12: //Right Both Arm  Arm#03 & Arm#04

        //                            //1st Cmd 是PUT相關則 Arm前後通通要淨空
        //                            tmpRobotArmInfo[2].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[3].ArmFrontJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[2].ArmBackJobExist = eGlassExist.NoExist;
        //                            tmpRobotArmInfo[3].ArmBackJobExist = eGlassExist.NoExist;

        //                            break;

        //                        default:

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {
        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) 1st defineNormalRobotCmd Action({3}) but UseArm({4}) is out of Range!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) can not Get 1st defineNormalRobotCmd Action({3}) but UseArm({4}) is out of Range!",
        //                                                    curRobot.Data.ROBOTNAME, funcName, MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

        //                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
        //                            robotConText.SetReturnMessage(errMsg);

        //                            return false;

        //                    }

        //                    #endregion

        //                    if (curFrontJob != null)
        //                    {
        //                        tmpFrontLocation_SlotNo = cur1stRobotCmd.Cmd01_TargetSlotNo;
        //                        //Sub Loction 還是出片前的位置
        //                        tmpFrontSubLocation = curFrontJob.RobotWIP.CurSubLocation;
        //                    }

        //                    if (curBackJob != null)
        //                    {
        //                        tmpBackLocation_SlotNo = cur1stRobotCmd.Cmd01_TargetSlotNo+1;
        //                        //Sub Loction 還是出片前的位置
        //                        tmpBackSubLocation = curBackJob.RobotWIP.CurSubLocation;
        //                    }

        //                    #endregion

        //                    break;

        //                case 2:  //Get
        //                case 8:  //Put Ready
        //                case 16: //Get Ready

        //                    #region [ 1st Cmd 是Get相關則要更新相對應的Arm資訊 ]

        //                    //Local Stage is Stage
        //                    tmpFrontStageID = eRobotCommonConst.ROBOT_HOME_STAGEID;
        //                    tmpBackStageID = eRobotCommonConst.ROBOT_HOME_STAGEID;

        //                    #region [ by 1st Cmd Use Arm UpDate ArmInfo ]

        //                    //SPEC定義1Arm 2Job
        //                    //0: None              1: Upper/Left Arm       2: Lower/Left Arm     3: Left Both Arm
        //                    //4: Upper/Right Arm   5: Upper Both Arm       8: Lower/Right Arm   10: Lower Both Arm
        //                    //12: Right Both Arm
        //                    switch (cur1stRobotCmd.Cmd01_ArmSelect)
        //                    {
        //                        case 1: //Upper/Left Arm  Arm#01

        //                            //根據Job 前後位置來決定更新Arm運算資訊
        //                            if (curFrontJob!=null)
        //                            {
        //                                tmpRobotArmInfo[0].ArmFrontJobExist = eGlassExist.Exist;
        //                                tmpFrontLocation_SlotNo = eCellSpecialArmSlotNo.UpperLeft_Front; //Arm#01 Front
        //                            }

        //                            if (curBackJob != null)
        //                            {
        //                                tmpRobotArmInfo[0].ArmBackJobExist = eGlassExist.Exist;
        //                                tmpBackLocation_SlotNo = eCellSpecialArmSlotNo.UpperLeft_Back;  //Arm#01 Back
        //                            }

        //                            break;

        //                        case 2: //Lower/Left Arm Arm#02

        //                            //根據Job 前後位置來決定更新Arm運算資訊
        //                            if (curFrontJob != null)
        //                            {
        //                                tmpRobotArmInfo[1].ArmFrontJobExist = eGlassExist.Exist;
        //                                tmpFrontLocation_SlotNo = eCellSpecialArmSlotNo.LowerLeft_Front; //Arm#02 Front

        //                            }

        //                            if (curBackJob != null)
        //                            {
        //                                tmpRobotArmInfo[1].ArmBackJobExist = eGlassExist.Exist;
        //                                tmpBackLocation_SlotNo = eCellSpecialArmSlotNo.LowerLeft_Back; //Arm#02 back
        //                            }

        //                            break;

        //                        //case 3: //Left Both Arm //硬體不支援 同Other異常處理

        //                        case 4: //Upper/Right Arm  Arm#03

        //                            //根據Job 前後位置來決定更新Arm運算資訊
        //                            if (curFrontJob != null)
        //                            {
        //                                tmpRobotArmInfo[2].ArmFrontJobExist = eGlassExist.Exist;
        //                                tmpFrontLocation_SlotNo = eCellSpecialArmSlotNo.UpperRight_Front; //Arm#03 Front
        //                            }

        //                            if (curBackJob != null)
        //                            {
        //                                tmpRobotArmInfo[2].ArmBackJobExist = eGlassExist.Exist;
        //                                tmpBackLocation_SlotNo = eCellSpecialArmSlotNo.UpperRight_Back; //Arm#03 back
        //                            }

        //                            break;

        //                        //case 5: //Upper Both Arm Arm#01 & Arm#03 //設定目前不支援UpperBoth 異常處理

        //                        case 8: //Lower/Right Arm  Arm#04

        //                            //根據Job 前後位置來決定更新Arm運算資訊
        //                            if (curFrontJob != null)
        //                            {
        //                                tmpRobotArmInfo[3].ArmFrontJobExist = eGlassExist.Exist;
        //                                tmpFrontLocation_SlotNo = eCellSpecialArmSlotNo.LowerRight_Front; //Arm#04 Front
        //                            }

        //                            if (curBackJob != null)
        //                            {
        //                                tmpRobotArmInfo[3].ArmBackJobExist = eGlassExist.Exist;
        //                                tmpBackLocation_SlotNo = eCellSpecialArmSlotNo.LowerLeft_Back; //Arm#04 back
        //                            }

        //                            tmpFrontLocation_SlotNo = 4; //Arm#04

        //                            break;


        //                        //case 10: //Lower Both Arm  Arm#02 & Arm#04 //設定目前不支援UpperBoth 異常處理

        //                        //case 12: //Right Both Arm  Arm#03 & Arm#04 //設定目前不支援UpperBoth 異常處理

        //                        default:

        //                            if(curFrontJob != null)

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {
        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) 1st defineNormalRobotCmd Action({3}) but UseArm({4}) is out of Range!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) can not Get 1st defineNormalRobotCmd Action({3}) but UseArm({4}) is out of Range!",
        //                                                    curRobot.Data.ROBOTNAME, funcName, MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

        //                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
        //                            robotConText.SetReturnMessage(errMsg);

        //                            return false;

        //                    }

        //                    #endregion

        //                    if (curFrontJob != null)
        //                    {
        //                        //Sub Loction 還是出片前的位置
        //                        tmpFrontSubLocation = curFrontJob.RobotWIP.CurSubLocation;
        //                    }

        //                    if (curBackJob != null)
        //                    {
        //                        //Sub Loction 還是出片前的位置
        //                        tmpBackSubLocation = curBackJob.RobotWIP.CurSubLocation;
        //                    }


        //                    #endregion

        //                    break;

        //                default:

        //                    #region[DebugLog]

        //                    if (IsShowDetialLog == true)
        //                    {
        //                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) 1st defineNormalRobotCmd Action({3}) is out of Range!",
        //                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString());

        //                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                    }

        //                    #endregion

        //                    errMsg = string.Format("[{0}][{1}] Robot({2}) can not Get 1st defineNormalRobotCmd Action({3}) is out of Range!",
        //                                            curRobot.Data.ROBOTNAME, funcName, MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString());

        //                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
        //                    robotConText.SetReturnMessage(errMsg);

        //                    return false;

        //            }

        //            #endregion

        //            #endregion

        //        }

        //        #endregion

        //        #region [ Get Front/Back Job Check Step StageID, UseArm and Action ]

        //        string tmpFrontJobStepUseArm = string.Empty;
        //        string tmpFrontJobStepAction = string.Empty;
        //        string tmpBackJobStepUseArm = string.Empty;
        //        string tmpBackJobStepAction = string.Empty;

        //        if (curFrontJob != null && curBackJob != null)
        //        {
        //            tmpFrontJobStepUseArm = curFrontJob.RobotWIP.RobotRouteStepList[tmpFrontStepNo].Data.ROBOTUSEARM.ToString().Trim();
        //            tmpFrontJobStepAction = curFrontJob.RobotWIP.RobotRouteStepList[tmpFrontStepNo].Data.ROBOTACTION.ToString().Trim();
        //            tmpBackJobStepUseArm = curBackJob.RobotWIP.RobotRouteStepList[tmpBackStepNo].Data.ROBOTUSEARM.ToString().Trim();
        //            tmpBackJobStepAction = curBackJob.RobotWIP.RobotRouteStepList[tmpBackStepNo].Data.ROBOTACTION.ToString().Trim();

        //            #region [ 比對Front and Back Job StageID, UseArm與Action必須要相同才行 ]

        //            if (tmpFrontStageID != tmpBackStageID)
        //            {
        //                #region[DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) frontJob({2},{3}) Check StepID({4}) StageID({5}) But BackJob({6},{7}) Check StepID({8}) StageID({9}) is different!",
        //                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
        //                                            tmpFrontStepNo, tmpFrontStageID, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
        //                                            tmpBackStepNo, tmpBackStageID);

        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }

        //                #endregion

        //                errMsg = string.Format("Robot({0}) frontJob({1},{2}) Check StepID({3}) StageID({4}) But BackJob({5},{6}) Check StepID({7}) StageID({8}) is different!",
        //                                            curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
        //                                            tmpFrontStepNo, tmpFrontStageID, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
        //                                            tmpBackStepNo, tmpBackStageID);

        //                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBack_LocationStageID_Is_Different);
        //                robotConText.SetReturnMessage(errMsg);

        //                return false;

        //            }

        //            if (tmpFrontJobStepAction != tmpBackJobStepAction)
        //            {

        //                #region[DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) frontJob({2},{3}) Check StepID({4}) Action({5}) But BackJob({6},{7}) Check StepID({8}) Action({9}) is different!",
        //                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
        //                                            tmpFrontStepNo, tmpFrontJobStepAction, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
        //                                            tmpBackStepNo, tmpBackJobStepAction);

        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }

        //                #endregion

        //                errMsg = string.Format("Robot({0}) frontJob({1},{2}) Check StepID({3}) Action({4}) But BackJob({5},{6}) Check StepID({7}) Action({8}) is different!",
        //                                            curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
        //                                            tmpFrontStepNo, tmpFrontJobStepAction, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
        //                                            tmpBackStepNo, tmpBackJobStepAction);

        //                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBack_Action_Is_Different);
        //                robotConText.SetReturnMessage(errMsg);

        //                return false;

        //            }

        //            if (tmpFrontJobStepUseArm != tmpBackJobStepUseArm)
        //            {

        //                #region[DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) frontJob({2},{3}) Check StepID({4}) UseArm({5}) But BackJob({6},{7}) Check StepID({8}) UseArm({9}) is different!",
        //                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
        //                                            tmpFrontStepNo, tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
        //                                            tmpBackStepNo, tmpBackJobStepUseArm);

        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }

        //                #endregion

        //                errMsg = string.Format("Robot({0}) frontJob({1},{2}) Check StepID({3}) UseArm({4}) But BackJob({5},{6}) Check StepID({7}) UseArm({8}) is different!",
        //                                            curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
        //                                            tmpFrontStepNo, tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
        //                                            tmpBackStepNo, tmpBackJobStepUseArm);

        //                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBack_UseArm_Is_Different);
        //                robotConText.SetReturnMessage(errMsg);

        //                return false;

        //            }

        //            #endregion

        //        }
        //        else if (curFrontJob != null && curBackJob == null)
        //        {
        //            //Back is Empty 等同Front
        //            tmpFrontJobStepUseArm = curFrontJob.RobotWIP.RobotRouteStepList[tmpFrontStepNo].Data.ROBOTUSEARM.ToString().Trim();
        //            tmpFrontJobStepAction = curFrontJob.RobotWIP.RobotRouteStepList[tmpFrontStepNo].Data.ROBOTACTION.ToString().Trim();
        //            tmpBackJobStepUseArm = tmpFrontJobStepUseArm;
        //            tmpBackJobStepAction = tmpFrontJobStepAction;
        //        }
        //        else if (curFrontJob == null && curBackJob != null)
        //        {
        //            //Fromt is Empty 等同Back
        //            tmpBackJobStepUseArm = curBackJob.RobotWIP.RobotRouteStepList[tmpBackStepNo].Data.ROBOTUSEARM.ToString().Trim();
        //            tmpBackJobStepAction = curBackJob.RobotWIP.RobotRouteStepList[tmpBackStepNo].Data.ROBOTACTION.ToString().Trim();
        //            tmpFrontJobStepUseArm = tmpBackJobStepUseArm;
        //            tmpFrontJobStepAction = tmpBackJobStepAction;
        //        }

        //        #endregion

        //        //定義最後選擇的Arm資訊
        //        string curAfterCheckUseArm = string.Empty;

        //        #region [ by Front/Back curStep Location Check Use Arm ]

        //        //Spec對應
        //        //0: None               //2: Lower/Left Arm  //4: Upper/Right Arm
        //        //1: Upper/Left Arm     //3: Left Both Arm   //8: Lower/Right Arm
        //        //12: Right Both Arm
        //        //當沒有FrontJob時 會以BcakJob StageID來填入值
        //        if (tmpFrontStageID != eRobotCommonConst.ROBOT_HOME_STAGEID)
        //        {
        //            //Check Not On Arm相關
        //            #region [ Job Loaction Not On Arm. Not On Arm:Only:Use Arm must Empty ]

        //            //Front/Back都是相同所以以Front處理即可
        //            switch (tmpFrontJobStepUseArm)
        //            {
        //                case eDBRobotUseArmCode.UPPER_ARM:  //Upper Left/Right(Arm#01 or Arm#03)其中之一為空即可 

        //                    #region [ StageJob Route Use Upper Arm But Upper Left(Arm#01) and Upper Right(Arm#03) Arm Job Exist ]

        //                    //根tmpArm#01 or Arm#03上不可以有片
        //                    //先判斷Arm01是否可收(沒片且有Enable)
        //                    if (tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.NoExist &&
        //                        tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.NoExist &&
        //                        tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
        //                    {
        //                        //Stage Job Check UpArm is Empty
        //                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;

        //                    }
        //                    //再判斷Arm03是否可收(沒片且有Enable)
        //                    else if (tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.NoExist &&
        //                             tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.NoExist &&
        //                             tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
        //                    {
        //                        //Stage Job Check LowArm is Empty
        //                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
        //                    }
        //                    else
        //                    {
        //                        //Stage Job but Upper Left Arm and Upper Right Arm is not Empty
        //                        if (curFrontJob != null && curFrontJob != null)
        //                        {

        //                            #region [ Front Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                            #endregion

        //                        }
        //                        else if (curFrontJob != null && curFrontJob == null)
        //                        {

        //                            #region [ Front Exist , Back Not Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                            #endregion

        //                        }
        //                        else if (curFrontJob == null && curFrontJob != null)
        //                        {

        //                            #region [ Front Not Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, 
        //                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME,
        //                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                            #endregion

        //                        }

        //                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist);
        //                        robotConText.SetReturnMessage(errMsg);

        //                        return false;

        //                    }

        //                    #endregion

        //                    break;

        //                case eDBRobotUseArmCode.LOWER_ARM:

        //                    #region [ StageJob Route Use Lower Arm But Lower Left(Arm#02) and Upper Right(Arm#04) Arm Job Exist ]
        //                    //Arm#02 or Arm#04上不可以有片
        //                    //先判斷Arm02是否可收(沒片且有Enable)
        //                    if (tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.NoExist &&
        //                        tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.NoExist &&
        //                        tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
        //                    {
        //                        //Stage Job Check UpArm is Empty
        //                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;

        //                    }
        //                    //再判斷Arm04是否可收(沒片且有Enable)
        //                    else if (tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.NoExist &&
        //                             tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.NoExist &&
        //                             tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
        //                    {
        //                        //Stage Job Check LowArm is Empty
        //                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
        //                    }
        //                    else
        //                    {
        //                        //Stage Job but Upper Left Arm and Upper Right Arm is not Empty
        //                        if (curFrontJob != null && curFrontJob != null)
        //                        {

        //                            #region [ Front Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                            #endregion

        //                        }
        //                        else if (curFrontJob != null && curFrontJob == null)
        //                        {

        //                            #region [ Front Exist , Back Not Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                            #endregion

        //                        }
        //                        else if (curFrontJob == null && curFrontJob != null)
        //                        {

        //                            #region [ Front Not Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
        //                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME,
        //                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                            #endregion

        //                        }

        //                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist);
        //                        robotConText.SetReturnMessage(errMsg);

        //                        return false;

        //                    }

        //                    #endregion

        //                    break;

        //                case eDBRobotUseArmCode.ANY_ARM:

        //                    #region [ StageJob Route Use Upper Arm But Upper Left(Arm#01) and Upper Right(Arm#03) Arm Job Exist ]
        //                    //Arm#01 or Arm#03上不可以有片
        //                    //先判斷Arm01是否可收(沒片且有Enable)
        //                    if (tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.NoExist &&
        //                        tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.NoExist &&
        //                        tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
        //                    {
        //                        //Stage Job Check UpArm is Empty
        //                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;

        //                    }
        //                    //再判斷Arm03是否可收(沒片且有Enable)
        //                    else if (tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.NoExist &&
        //                             tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.NoExist &&
        //                             tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
        //                    {
        //                        //Stage Job Check LowArm is Empty
        //                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
        //                    }
        //                    //在判斷Arm#02 or Arm#04上不可以有片
        //                    else if (tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.NoExist &&
        //                        tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.NoExist &&
        //                        tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
        //                    {
        //                        //Stage Job Check UpArm is Empty
        //                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;

        //                    }
        //                    //再判斷Arm04是否可收(沒片且有Enable)
        //                    else if (tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.NoExist &&
        //                             tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.NoExist &&
        //                             tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
        //                    {
        //                        //Stage Job Check LowArm is Empty
        //                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
        //                    }
        //                    else
        //                    {
        //                        //Stage Job but Upper Left Arm and Upper Right Arm is not Empty
        //                        if (curFrontJob != null && curFrontJob != null)
        //                        {

        //                            #region [ Front Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                            #endregion

        //                            #region [ Front Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                            #endregion

        //                        }
        //                        else if (curFrontJob != null && curFrontJob == null)
        //                        {

        //                            #region [ Front Exist , Back Not Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                            #endregion

        //                            #region [ Front Exist , Back Not Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                            #endregion

        //                        }
        //                        else if (curFrontJob == null && curFrontJob != null)
        //                        {
        //                            #region [ Front Not Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
        //                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME,
        //                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                            #endregion

        //                            #region [ Front Not Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
        //                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME,
        //                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                            #endregion

        //                        }

        //                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist);
        //                        robotConText.SetReturnMessage(errMsg);

        //                        return false;

        //                    }

        //                    #endregion

        //                    break;

        //                //case eDBRobotUseArmCode.ALL_ARM: //設定不支援視同異常處理

        //                default:

        //                    #region [ DB Setting Illegal ]

        //                    #region[DebugLog]

        //                    if (IsShowDetialLog == true)
        //                    {
        //                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) is illegal!",
        //                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
        //                                                tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction, tmpFrontJobStepUseArm);

        //                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                    }

        //                    #endregion

        //                    errMsg = string.Format("[{0}][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) is illegal!",
        //                                            MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
        //                                            tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction, tmpFrontJobStepUseArm);

        //                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_Setting_Fail);
        //                    robotConText.SetReturnMessage(errMsg);

        //                    return false;

        //                    #endregion

        //            }

        //            #endregion

        //        }
        //        else
        //        {
        //            //20160104 Check Arm相關
        //            #region [ Job Location on Arm. Must Check Use Arm(Job LocationSlot has Job).tmpFrontLocation_SlotNo and tmpBackLocation_SlotNo ]

        //            switch (tmpFrontJobStepUseArm)
        //            {
        //                case eDBRobotUseArmCode.UPPER_ARM:  //Upper Left/Right(Arm#01 or Arm#03)其中之一不為空即可 by tmpFrontLocation_SlotNo and tmpBackLocation_SlotNo 

        //                    #region [ StageJob Route Use Upper Arm But Upper Left(Arm#01) and Upper Right(Arm#03) Arm Job NotExist ]

        //                    if (curFrontJob != null && curFrontJob != null)
        //                    {

        //                        #region [ Front and Back Job Exist ]

        //                        //Arm#01 front or Back JobExist
        //                        if (((tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Front) &&
        //                             (tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Back)) &&
        //                            tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;

        //                        }
        //                        //Arm#03 front or Back JobExist
        //                        else if (((tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Front) &&
        //                                  (tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Back)) &&
        //                                 tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;

        //                        }
        //                        else
        //                        {

        //                            #region [ Front Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                            #endregion

        //                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist);
        //                            robotConText.SetReturnMessage(errMsg);

        //                            return false;
        //                        }

        //                        #endregion

        //                    }
        //                    else if (curFrontJob != null && curFrontJob == null)
        //                    {

        //                        #region [ Front Exist and Back Job Notexist ]

        //                        //Arm#01 front Exist Back Notexist
        //                        if (((tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Front) &&
        //                             (tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.NoExist)) &&
        //                            tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;

        //                        }
        //                        //Arm#03 front or Back JobExist
        //                        else if (((tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Front) &&
        //                                  (tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.NoExist)) &&
        //                                 tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;

        //                        }
        //                        else
        //                        {

        //                            #region [ Front Exist , Back Not Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                            #endregion

        //                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist);
        //                            robotConText.SetReturnMessage(errMsg);

        //                            return false;
        //                        }

        //                        #endregion

        //                    }
        //                    else if (curFrontJob != null && curFrontJob == null)
        //                    {

        //                        #region [ Front Not exist and Back Job Exist ]

        //                        //Arm#01 front or Back JobExist
        //                        if (((tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.NoExist) &&
        //                             (tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Back)) &&
        //                            tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;

        //                        }
        //                        //Arm#03 front or Back JobExist
        //                        else if (((tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.NoExist) &&
        //                                  (tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Back)) &&
        //                                 tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;

        //                        }
        //                        else
        //                        {

        //                            #region [ Front Not Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
        //                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME,
        //                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

        //                            #endregion

        //                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist);
        //                            robotConText.SetReturnMessage(errMsg);

        //                            return false;
        //                        }

        //                        #endregion

        //                    }
        //                    else
        //                    {

        //                        #region [ Front and Back Job Not Exist ]

        //                        #region[DebugLog]

        //                        if (IsShowDetialLog == true)
        //                        {
        //                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get curSlotBlockInfo Front and Back jobInfo!",
        //                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

        //                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                        }

        //                        #endregion

        //                        errMsg = string.Format("[{0}] Robot({1}) can not Get curSlotBlockInfo  Front and Back jobInfo!",
        //                                                MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_SlotBlockInfo_Job_Is_Null);
        //                        robotConText.SetReturnMessage(errMsg);

        //                        return false;

        //                        #endregion
        //                    }

        //                    #endregion

        //                    break;

        //                case eDBRobotUseArmCode.LOWER_ARM:  //Lower Left/Right(Arm#02 or Arm#04)其中之一不為空即可 by tmpFrontLocation_SlotNo and tmpBackLocation_SlotNo 

        //                    #region [ StageJob Route Use Upper Arm But Lower Left(Arm#02) and Upper Right(Arm#04) Arm Job NotExist ]

        //                    if (curFrontJob != null && curFrontJob != null)
        //                    {

        //                        #region [ Front and Back Job Exist ]

        //                        //Arm#02 front or Back JobExist
        //                        if (((tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Front) &&
        //                             (tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Back)) &&
        //                            tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;

        //                        }
        //                        //Arm#04 front or Back JobExist
        //                        else if (((tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Front) &&
        //                                  (tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Back)) &&
        //                                 tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;

        //                        }
        //                        else
        //                        {

        //                            #region [ Front Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                            #endregion

        //                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist);
        //                            robotConText.SetReturnMessage(errMsg);

        //                            return false;
        //                        }

        //                        #endregion

        //                    }
        //                    else if (curFrontJob != null && curFrontJob == null)
        //                    {

        //                        #region [ Front Exist and Back Job Notexist ]

        //                        //Arm#02 front Exist Back Notexist
        //                        if (((tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Front) &&
        //                             (tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.NoExist)) &&
        //                            tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;

        //                        }
        //                        //Arm#04 front or Back JobExist
        //                        else if (((tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Front) &&
        //                                  (tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.NoExist)) &&
        //                                 tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;

        //                        }
        //                        else
        //                        {

        //                            #region [ Front Exist , Back Not Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                        tmpFrontJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
        //                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
        //                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                            #endregion

        //                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist);
        //                            robotConText.SetReturnMessage(errMsg);

        //                            return false;
        //                        }

        //                        #endregion

        //                    }
        //                    else if (curFrontJob != null && curFrontJob == null)
        //                    {

        //                        #region [ Front Not exist and Back Job Exist ]

        //                        //Arm#02 front or Back JobExist
        //                        if (((tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.NoExist) &&
        //                             (tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Back)) &&
        //                            tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;

        //                        }
        //                        //Arm#04 front or Back JobExist
        //                        else if (((tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.NoExist) &&
        //                                  (tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Back)) &&
        //                                 tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
        //                        {

        //                            //Arm Has Job , and UpArm Has Job 要考慮Job位置!
        //                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;

        //                        }
        //                        else
        //                        {

        //                            #region [ Front Not Exist , Back Exist Log ]

        //                            #region[DebugLog]

        //                            if (IsShowDetialLog == true)
        //                            {

        //                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
        //                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
        //                                                        tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                        tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                        tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                            }

        //                            #endregion

        //                            errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
        //                                                    MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME,
        //                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
        //                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
        //                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
        //                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
        //                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

        //                            #endregion

        //                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist);
        //                            robotConText.SetReturnMessage(errMsg);

        //                            return false;
        //                        }

        //                        #endregion

        //                    }
        //                    else
        //                    {

        //                        #region [ Front and Back Job Not Exist ]

        //                        #region[DebugLog]

        //                        if (IsShowDetialLog == true)
        //                        {
        //                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get curSlotBlockInfo Front and Back jobInfo!",
        //                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

        //                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                        }

        //                        #endregion

        //                        errMsg = string.Format("[{0}] Robot({1}) can not Get curSlotBlockInfo  Front and Back jobInfo!",
        //                                                MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_SlotBlockInfo_Job_Is_Null);
        //                        robotConText.SetReturnMessage(errMsg);

        //                        return false;

        //                        #endregion
        //                    }

        //                    #endregion

        //                    break;

        //                case eDBRobotUseArmCode.ANY_ARM:

        //                    //20160104 work end

        //                    break;

        //               //case eDBRobotUseArmCode.ALL_ARM:  //設定不支援視同異常處理
        //               //     break;

        //                default:

        //                    if (curFrontJob != null)
        //                    {
        //                        //以FrontJob資訊為主
        //                        #region [ DB Setting Illegal ]

        //                        #region[DebugLog]

        //                        if (IsShowDetialLog == true)
        //                        {
        //                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Front Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) is illegal!",
        //                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
        //                                                    tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction, tmpFrontJobStepUseArm);

        //                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                        }

        //                        #endregion

        //                        errMsg = string.Format("[{0}][{1}] Robot({2}) Front Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) is illegal!",
        //                                                MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
        //                                                tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction, tmpFrontJobStepUseArm);

        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        //以BackJob資訊為主
        //                        #region [ DB Setting Illegal ]

        //                        #region[DebugLog]

        //                        if (IsShowDetialLog == true)
        //                        {
        //                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Back Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) is illegal!",
        //                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
        //                                                    tmpBackStageID, tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm);

        //                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                        }

        //                        #endregion

        //                        errMsg = string.Format("[{0}][{1}] Robot({2}) Back Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) is illegal!",
        //                                                MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
        //                                                tmpBackStageID, tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm);

        //                        #endregion

        //                    }

        //                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_Setting_Fail);
        //                    robotConText.SetReturnMessage(errMsg);

        //                    return false;                           

        //            }

        //            #endregion

        //        }

        //        #endregion

        //        #region [ Set Robot UseArm To DefindNormalRobotCommand by is2ndCmdFlag ]

        //        if (is2ndCmdFlag == false)
        //        {
        //            if (curFrontJob != null)
        //            {
        //                cur1stRobotCmd.Cmd01_ArmSelect = GetRobotUseArmCode(curRobot, curFrontJob, curAfterCheckUseArm);
        //            }
        //            else
        //            {
        //                cur1stRobotCmd.Cmd01_ArmSelect = GetRobotUseArmCode(curRobot, curBackJob, curAfterCheckUseArm);

        //            }

        //            robotConText.AddParameter(eRobotContextParameter.Define_1stNormalRobotCommandInfo, cur1stRobotCmd);
        //        }
        //        else
        //        {
        //            if (curFrontJob != null)
        //            {
        //                cur2ndRobotCmd.Cmd01_ArmSelect = GetRobotUseArmCode(curRobot, curFrontJob, curAfterCheckUseArm);
        //            }
        //            else
        //            {

        //                cur2ndRobotCmd.Cmd01_ArmSelect = GetRobotUseArmCode(curRobot, curBackJob, curAfterCheckUseArm);
        //            }

        //            robotConText.AddParameter(eRobotContextParameter.Define_2ndNormalRobotCommandInfo, cur2ndRobotCmd);
        //        }

        //        #endregion

        //        #endregion

        //        robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
        //        robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

        //        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
        //        robotConText.SetReturnMessage(ex.Message);

        //        return false;
        //    }

        //}

        #endregion

        /// <summary> Check Port Unloader Dispatch Rule by Job Grade for Normal and Cell Special Type Common Function
        ///  
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0041")]
        public bool Filter_ULDPortDipatchRuleByJobGrade(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            //20160114 add by Port FailMsg and Code for Unloader DispatchRule
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ checkStep by is2ndCmdFlag ]

                string tmpStepAction = string.Empty;
                int tmpStepNo = 0;
                string funcName = string.Empty;

                if (is2ndCmdFlag == false)
                {

                    tmpStepNo = curBcsJob.RobotWIP.CurStepNo;
                    funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                }
                else
                {

                    tmpStepNo = curBcsJob.RobotWIP.NextStepNo;
                    funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;
                }

                #endregion

                #region [ Get tmp Step Entity ]

                RobotRouteStep tmpRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[tmpStepNo];

                //找不到 CurStep Route 回NG
                if (tmpRouteStep == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Get curRouteStep({5})!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, tmpStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get curRouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo, tmpStepNo.ToString());

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
                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4})  StageIDList({5}) can not Find Stage Status is LDRQ!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Stage is must PUT]

                if (tmpRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) Check StepID({5}) Action({6}) is Not PUT!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.ROBOTACTION);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4}) Action({5}) is Not PUT!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.ROBOTACTION);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CheckStepAction_Is_Error);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Line Itme ]

                Line curline = ObjectManager.LineManager.GetLine(Workbench.ServerName);

                if (curline == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Get Line entity by ServerName({5})!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, Workbench.ServerName);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Line entity by ServerName({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo, Workbench.ServerName);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_LineByServerName_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Line Unloader Dispatch Rule List ]

                if (curline.File.UnlaoderDispatchRule.Count == 0)
                {
                    fail_ReasonCode = string.Format("JobFilterService_Filter_ULDPortDipatchRuleByJobGrade_{0}", tmpStepNo.ToString());

                    if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) JobGrade({6}), But Line has no any UnloadDispatchRule",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.JobGrade);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) JobGrade({3}), But Line has no UnloaderDispatchRule",
                        //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.JobGrade);

                        failMsg = string.Format("Job({0}_{1}) curRouteID({2}) curStepNo({3}) JobGrade({4}), But Line has no any UnloaderDispatchRule",
                                                curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.JobGrade);

                        AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) can not get UnloaderDispatchRule because Line UnloaderDisptchRule count is empty!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not get UnloaderDispatchRule because Line UnloaderDisptchRule count is empty!(can't find any UnloaderDispatchRule)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_LineUnloaderDispatchRule_Is_Empty);
                    robotConText.SetReturnMessage(errMsg);

                    if (!curRobot.CheckErrorList.ContainsKey(fail_ReasonCode))  //add for BMS Error Monitor [Special Format,mark by yang]
                        curRobot.CheckErrorList.Add(fail_ReasonCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion

                #region [ Check Job Grade by CurCanUseStageList ]

                string[] stageList = tmpRouteStep.Data.STAGEIDLIST.Split(',');
                string strUnloaddisPatchingRules = "";
                for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                {

                    #region [ Get StepStage Entity ]

                    RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);

                    if (curStepStage == null)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, tmpRouteStep.Data.STEPID, tmpRouteStep.Data.STAGEIDLIST, stageList[stageIdx]);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        errMsg = string.Format("[{0}] can not Get RobotStageInfo by StageID({1}!",
                            MethodBase.GetCurrentMethod().Name, stageList[stageIdx]);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_Stage_Is_Null);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }

                    #endregion

                    #region [ 判斷Current Step Stage 是否存在於Current LDRQ Stage List ]

                    if (curStageList.Contains(curStepStage) == false)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, tmpRouteStep.Data.STEPID, tmpRouteStep.Data.STAGEIDLIST, curStepStage.Data.STAGEID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不存在則視同判斷失敗直接記Log跳下一個Stage
                        continue;

                    }

                    #endregion

                    #region [ Check Stage is Port Type ]

                    if (curStepStage.Data.STAGETYPE != eRobotStageType.PORT)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) StageType({3}) can not Check by SetCode!",
                                                    curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, curStepStage.Data.STAGETYPE);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不是Port Stage則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }

                    Port curPort = ObjectManager.PortManager.GetPort(curStepStage.Data.STAGEID);

                    if (curPort == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Port Entity By StageID({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curStepStage.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //找不到Port Enrirt則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }

                    #endregion

                    #region [ Get Port Unloader Dispatch Rule by StageID ]

                    clsDispatchRule curdispatchRule = curline.File.UnlaoderDispatchRule[curStepStage.Data.STAGEID];

                    if (curdispatchRule == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) can not get Dispatch Rule by StageID({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curStepStage.Data.STAGEID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //找不到Port Dispatch Rule則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }

                    #endregion

                    #region [ Check Grade先Cehck 特定Grade > EMP > 最後才確認MX. 符合3種Grade設定中任意一個都可收 20160218 add for Cell Special 如果前後片Grade不同就只能去MIX Port ]

                    //201601114 add byPort FailCode for Unloader Dispatch Rule
                    //fail_ReasonCode = string.Format("JobFilterService_Filter_ULDPortDipatchRuleByJobGrade_{0}_{1}", tmpStepNo.ToString(), curStepStage.Data.STAGEID);

                    //20160218 add for Cell Special 如果前後片Grade不同就只能去MIX Port
                    if (curBcsJob.RobotWIP.OnlyToMixGradeULDFlag == false)
                    {

                        #region [ Check Match Grade and Update Priority ]

                        //if ((curdispatchRule.Grade1.ToUpper().Trim() == curBcsJob.JobGrade.Trim()) ||
                        //    (curdispatchRule.Grade2.ToUpper().Trim() == curBcsJob.JobGrade.Trim()) ||
                        //    (curdispatchRule.Grade3.ToUpper().Trim() == curBcsJob.JobGrade.Trim()))
                        if ((curdispatchRule.Grade1.ToUpper().Trim() == curBcsJob.RobotWIP.CurSendOutJobGrade.Trim()) ||
                            (curdispatchRule.Grade2.ToUpper().Trim() == curBcsJob.RobotWIP.CurSendOutJobGrade.Trim()) ||
                            (curdispatchRule.Grade3.ToUpper().Trim() == curBcsJob.RobotWIP.CurSendOutJobGrade.Trim()))
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) CurSendOutJobGrade({4}), Port({5}) UnloaderDispatchRule Grade#1({6}) Grade#2({7}) Grade#3({8}) is match!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurSendOutJobGrade, curStepStage.Data.STAGEID, curdispatchRule.Grade1.ToUpper(), curdispatchRule.Grade2.ToUpper(),
                                                        curdispatchRule.Grade3.ToUpper());
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //Match則更新stage MatchGrade Priority並記Log直接跳下一個Stage
                            lock (curStepStage)
                            {
                                curStepStage.UnloaderPortMatchSetGradePriority = eCellUnloaderDispatchRuleMatchGradePriority.IS_MATCH_PRIORITY;
                            }

                            //找到符合的Port Remove FailCode
                            RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                            continue;

                        }

                        #endregion

                        #region [ Check EMP Grade and Update Priority ]

                        if ((curdispatchRule.Grade1.ToUpper().Trim() == eRobotCommonConst.PORT_EMP_GRADE) ||
                            (curdispatchRule.Grade2.ToUpper().Trim() == eRobotCommonConst.PORT_EMP_GRADE) ||
                            (curdispatchRule.Grade3.ToUpper().Trim() == eRobotCommonConst.PORT_EMP_GRADE))
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) JobGrade({4}), Port({5}) UnloaderDispatchRule Grade#1({6}) Grade#2({7}) Grade#3({8}) is match {9} Grade!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.JobGrade, curStepStage.Data.STAGEID, curdispatchRule.Grade1.ToUpper(), curdispatchRule.Grade2.ToUpper(),
                                                        curdispatchRule.Grade3.ToUpper(), eRobotCommonConst.PORT_EMP_GRADE);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //Match則更新stage MatchGrade Priority並記Log直接跳下一個Stage
                            lock (curStepStage)
                            {
                                curStepStage.UnloaderPortMatchSetGradePriority = eCellUnloaderDispatchRuleMatchGradePriority.IS_EMP_PRIORITY;
                            }

                            //找到符合的Port Remove FailCode
                            RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                            continue;

                        }

                        #endregion

                    }

                    #region [ Check Mix Grade and Update Priority ]

                    if ((curdispatchRule.Grade1.ToUpper().Trim() == eRobotCommonConst.PORT_MX_GRADE) ||
                        (curdispatchRule.Grade2.ToUpper().Trim() == eRobotCommonConst.PORT_MX_GRADE) ||
                        (curdispatchRule.Grade3.ToUpper().Trim() == eRobotCommonConst.PORT_MX_GRADE))
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) JobGrade({4}), Port({5}) UnloaderDispatchRule Grade#1({6}) Grade#2({7}) Grade#3({8}) is match {9} Grade!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.JobGrade, curStepStage.Data.STAGEID, curdispatchRule.Grade1.ToUpper(), curdispatchRule.Grade2.ToUpper(),
                                                    curdispatchRule.Grade3.ToUpper(), eRobotCommonConst.PORT_MX_GRADE);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //Match則更新stage MatchGrade Priority並記Log直接跳下一個Stage
                        lock (curStepStage)
                        {
                            curStepStage.UnloaderPortMatchSetGradePriority = eCellUnloaderDispatchRuleMatchGradePriority.IS_MX_PRIORITY;
                        }

                        //找到符合的Port Remove FailCode
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                        continue;

                    }

                    #endregion

                    //找不到符合Port Dispatch Rule則記Log直接跳下一個Stage
                    #region [ Add To Check Fail Message To Job ]

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) JobGrade({6}), But Port({7}) UnloaderDispatchRule Grade#1({8}) Grade#2({9}) Grade#3({10}) is Mismatch or OnlyToMixGradeULDFlag is ({11})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                curBcsJob.JobGrade, curStepStage.Data.STAGEID, curdispatchRule.Grade1.ToUpper(), curdispatchRule.Grade2.ToUpper(),
                                                curdispatchRule.Grade3.ToUpper(), curBcsJob.RobotWIP.OnlyToMixGradeULDFlag);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //    if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    //    {

                    //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) JobGrade({6}), But Port({7}) UnloaderDispatchRule Grade#1({8}) Grade#2({9}) Grade#3({10}) is Mismatch or OnlyToMixGradeULDFlag is ({11})!",
                    //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,curBcsJob.RobotWIP.CurStepNo.ToString(),
                    //                                curBcsJob.JobGrade, curStepStage.Data.STAGEID, curdispatchRule.Grade1.ToUpper(), curdispatchRule.Grade2.ToUpper(),
                    //                                curdispatchRule.Grade3.ToUpper(), curBcsJob.RobotWIP.OnlyToMixGradeULDFlag);
                    //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                    //        //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) JobGrade({3}), But Port({4}) UnloaderDispatchRule Grade#1({5}) Grade#2({6}) Grade#3({7}) is Mismatch or OnlyToMixGradeULDFlag is ({8})!",
                    //        //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                    //        //                        curBcsJob.JobGrade, curStepStage.Data.STAGEID, curdispatchRule.Grade1.ToUpper(), curdispatchRule.Grade2.ToUpper(),
                    //        //                        curdispatchRule.Grade3.ToUpper(), curBcsJob.RobotWIP.OnlyToMixGradeULDFlag);

                    //        failMsg = string.Format("Job({0}_{1}) curRouteID({2}) curStepNo({3}) JobGrade({4}), But Port({5}) UnloaderDispatchRule Grade#1({6}) Grade#2({7}) Grade#3({8}) is Mismatch or OnlyToMixGradeULDFlag is ({9})!",
                    //                                curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,curBcsJob.RobotWIP.CurStepNo.ToString(),
                    //                                curBcsJob.JobGrade, curStepStage.Data.STAGEID, curdispatchRule.Grade1.ToUpper(), curdispatchRule.Grade2.ToUpper(),
                    //                                curdispatchRule.Grade3.ToUpper(), curBcsJob.RobotWIP.OnlyToMixGradeULDFlag);

                    //        AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                    //        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                    //        #endregion
                    //    }

                    #endregion
                    strUnloaddisPatchingRules += string.Format("Port({0}) UnloaderDispatchRule Grade#1({1}) Grade#2({2}) Grade#3({3}),", curStepStage.Data.STAGEID, curdispatchRule.Grade1.ToUpper(), curdispatchRule.Grade2.ToUpper(),
                                                curdispatchRule.Grade3.ToUpper());
                    curStageList.Remove(curStepStage);
                    continue;

                    #endregion



                }

                #endregion

                if (curStageList.Count > 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) Filter Unloader Dispatch Rule By Job Grade is Match!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) Filter Unloader Dispatch Rule By Job Grade is Fail! No Any Stage could Receive glass.(can't find port DispatchRule Match Grade or EM or MX)",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) JobGrade({5}) OnlyToMixGradeFlag ({6}) But {7} is Mismatch,Filter Unloader Dispatch Rule By Job Grade is Fail! No Any Stage could Receive glass.",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.JobGrade, curBcsJob.RobotWIP.OnlyToMixGradeULDFlag, strUnloaddisPatchingRules);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleByJobGrade_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleByJobGrade_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
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

        /// <summary> Unloader收片邏輯, 檢查JobData EQPFlag, 相同的Job要收在同一Cassette裡
        ///  
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0043")]
        public bool Filter_ULDPortDipatchRuleByJobEQPFlag(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            //20160114 add by Port FailMsg and Code for Unloader DispatchRule
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
            string curBcsJobEqpFlag_DCRandSorterFlag = string.Empty;
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

                #region [ Get curBcsJob Entity and Decode EQPFlag ]
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

                IDictionary<string, string> curBcsJobEqpFlag = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG, eJOBDATA.EQPFlag);
                //0:No Flag Glass,1:DCR Flag Glass,2:Sorter Flag Glass,3:DCR and Sorter Flag Glass
                curBcsJobEqpFlag_DCRandSorterFlag = curBcsJobEqpFlag["DCRandSorterFlag"];
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ checkStep by is2ndCmdFlag ]

                string tmpStepAction = string.Empty;
                int tmpStepNo = 0;
                string funcName = string.Empty;

                if (is2ndCmdFlag == false)
                {

                    tmpStepNo = curBcsJob.RobotWIP.CurStepNo;
                    funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                }
                else
                {

                    tmpStepNo = curBcsJob.RobotWIP.NextStepNo;
                    funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;
                }

                #endregion

                #region [ Get tmp Step Entity ]

                RobotRouteStep tmpRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[tmpStepNo];

                //找不到 CurStep Route 回NG
                if (tmpRouteStep == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Get curRouteStep({5})!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, tmpStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get curRouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo, tmpStepNo.ToString());

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
                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4})  StageIDList({5}) can not Find Stage Status is LDRQ!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion

                #region [ Get Stage is must PUT]

                if (tmpRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) Check StepID({5}) Action({6}) is Not PUT!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.ROBOTACTION);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4}) Action({5}) is Not PUT!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.ROBOTACTION);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CheckStepAction_Is_Error);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_CheckStepAction_Is_Error;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region [ Check Job EQP Flag by CurCanUseStageList ]
                string strStageSlotJobDCRandSorterFlags = "";
                string[] stageList = tmpRouteStep.Data.STAGEIDLIST.Split(',');

                for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                {
                    #region [ Get StepStage Entity ]

                    RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);

                    if (curStepStage == null)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, tmpRouteStep.Data.STEPID, tmpRouteStep.Data.STAGEIDLIST, stageList[stageIdx]);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        errMsg = string.Format("[{0}] can not Get RobotStageInfo by StageID({1}!",
                            MethodBase.GetCurrentMethod().Name, stageList[stageIdx]);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_Stage_Is_Null);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    curStepStage.UnloaderPortSlotJobDCRandSorterFlag = string.Empty;
                    #endregion

                    #region [ 判斷Current Step Stage 是否存在於Current LDRQ Stage List ]

                    if (curStageList.Contains(curStepStage) == false)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, tmpRouteStep.Data.STEPID, tmpRouteStep.Data.STAGEIDLIST, curStepStage.Data.STAGEID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不存在則視同判斷失敗直接記Log跳下一個Stage
                        continue;

                    }

                    #endregion

                    #region [ Check Stage is Unloading Port ]

                    if (curStepStage.Data.STAGETYPE != eRobotStageType.PORT)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) StageType({3}) can not Check by SetCode!",
                                                    curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, curStepStage.Data.STAGETYPE);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不是Port Stage則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }

                    Port curPort = ObjectManager.PortManager.GetPort(curStepStage.Data.STAGEID);

                    if (curPort == null)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Port Entity By StageID({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curStepStage.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //找不到Port Entity則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }

                    if (curPort.File.Type != ePortType.UnloadingPort)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) is not a Unloading Port!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curStepStage.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不是Unloading Port則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }
                    if (curStepStage.File.CurStageStatus != eRobotStageStatus.RECEIVE_READY)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) Stage_LDRQ_Status is not {5}!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curStepStage.Data.STAGEID, eRobotStageStatus.RECEIVE_READY);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不是Unloading Port則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }
                    #endregion

                    #region Read JobEachCassetteSlotPositionBlock
                    //L2_Port#01JobEachCassetteSlotPositionBlock
                    string trxID = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", curPort.Data.NODENO, curPort.Data.PORTNO);
                    Trx slot_position = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                    if (slot_position == null)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) Cannot Read PLC Trx({3})!",
                                                    curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, trxID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        //找不到JobEachCassetteSlotPositionBlock則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }
                    #endregion

                    #region [ Check Unloader JobEachCassetteSlotPositionBlock ]

                    //201601114 add byPort FailCode for Unloader Dispatch Rule
                    string slot_cst_seq_no = string.Empty, slot_job_seq_no = string.Empty;
                    fail_ReasonCode = string.Format("JobFilterService_Filter_ULDPortDipatchRuleByJobEQPFlag_{0}_{1}", tmpStepNo.ToString(), curStepStage.Data.STAGEID);

                    #region Get Slot CassetteSequenceNo JobSequenceNo
                    for (int slot_no = 1; slot_no < curStepStage.Data.SLOTMAXCOUNT; slot_no++)
                    {
                        string item_cst_seq_no = string.Format("SlotPosition#{0}CassetteSequenceNo", slot_no.ToString().PadLeft(3, '0'));
                        string item_job_seq_no = string.Format("SlotPosition#{0}JobSequenceNo", slot_no.ToString().PadLeft(3, '0'));
                        string cst_seq_no = slot_position.EventGroups[0].Events[0].Items[item_cst_seq_no].Value;
                        string job_seq_no = slot_position.EventGroups[0].Events[0].Items[item_job_seq_no].Value;
                        if (cst_seq_no != "0" || job_seq_no != "0")
                        {
                            slot_cst_seq_no = cst_seq_no;
                            slot_job_seq_no = job_seq_no;
                            break;
                        }
                    }
                    #endregion
                    if (slot_cst_seq_no != string.Empty && slot_job_seq_no != string.Empty)
                    {
                        Job slot_job = ObjectManager.JobManager.GetJob(slot_cst_seq_no, slot_job_seq_no);
                        if (slot_job != null)
                        {
                            IDictionary<string, string> slot_job_eqp_flag = ObjectManager.SubJobDataManager.Decode(slot_job.EQPFlag, eJOBDATA.EQPFlag);
                            string slot_job_DCRandSorterFlag = slot_job_eqp_flag["DCRandSorterFlag"];
                            //0:No Flag Glass,1:DCR Flag Glass,2:Sorter Flag Glass,3:DCR and Sorter Flag Glass
                            //有 Sorter Flag(2 or 3) 要放在一起, 沒有 Sorter Flag(0 or 1) 要放在一起
                            if (((curBcsJobEqpFlag_DCRandSorterFlag == "2" || curBcsJobEqpFlag_DCRandSorterFlag == "3") && (slot_job_DCRandSorterFlag == "2" || slot_job_DCRandSorterFlag == "3")) ||
                                ((curBcsJobEqpFlag_DCRandSorterFlag == "0" || curBcsJobEqpFlag_DCRandSorterFlag == "1") && (slot_job_DCRandSorterFlag == "0" || slot_job_DCRandSorterFlag == "1")))
                            {
                                //有料有帳且EQPFlag相符
                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) DCRandSorterFlag({4}) Find a match Job({5}) DCRandSorterFlag({6}) on Port({7})",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJobEqpFlag_DCRandSorterFlag, slot_job.JobKey, slot_job_DCRandSorterFlag, curPort.Data.PORTNO);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion
                                //找到符合的Port Remove FailCode
                                curStepStage.UnloaderPortSlotJobDCRandSorterFlag = slot_job_DCRandSorterFlag;
                                strStageSlotJobDCRandSorterFlags += string.Format("StageID({0}) curStageUnloaderPortSlotJobDCRandSorterFlag({1}) SorterFlag({2}),", curStepStage.Data.STAGEID, curStepStage.UnloaderPortSlotJobDCRandSorterFlag, getDetailSortFlag(curStepStage.UnloaderPortSlotJobDCRandSorterFlag));
                                RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                                continue;
                            }
                            else
                            {
                                //有料有帳但EQPFlag不符
                                #region [ Add To Check Fail Message To Job ]

                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) DCRandSorterFlag({6}) mismatch witch Job({7}) DCRandSorterFlag({8}) on Port({9})",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJobEqpFlag_DCRandSorterFlag, slot_job.JobKey, slot_job_DCRandSorterFlag, curPort.Data.PORTNO);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) DCRandSorterFlag({6}) mismatch with Job({7}) DCRandSorterFlag({8}) on Port({9}),please check Job DCRandSorterFlag and Unloading CST Exist Job DCRandSorterFlag is must same",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJobEqpFlag_DCRandSorterFlag, slot_job.JobKey, slot_job_DCRandSorterFlag, curPort.Data.PORTNO);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                                    //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) DCRandSorterFlag({3}) mismatch witch Job({4}) DCRandSorterFlag({5}) on Port({6})",
                                    //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJobEqpFlag_DCRandSorterFlag, slot_job.JobKey, slot_job_DCRandSorterFlag, curPort.Data.PORTNO);

                                    failMsg = string.Format("Job({0}_{1}) curRouteID({2}) curStepNo({3}) DCRandSorterFlag({4}) mismatch with Job({5}) DCRandSorterFlag({6}) on Port({7}),please check Job DCRandSorterFlag and Unloading CST Exist Job DCRandSorterFlag is must same",
                                                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                            curBcsJobEqpFlag_DCRandSorterFlag, slot_job.JobKey, slot_job_DCRandSorterFlag, curPort.Data.PORTNO);

                                    AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                    #endregion
                                }

                                #endregion
                                curStepStage.UnloaderPortSlotJobDCRandSorterFlag = slot_job_DCRandSorterFlag;
                                strStageSlotJobDCRandSorterFlags += string.Format("StageID({0}) curStageUnloaderPortSlotJobDCRandSorterFlag({1}) SorterFlag({2}),", curStepStage.Data.STAGEID, curStepStage.UnloaderPortSlotJobDCRandSorterFlag, getDetailSortFlag(curStepStage.UnloaderPortSlotJobDCRandSorterFlag));
                                curStageList.Remove(curStepStage);
                                continue;
                            }
                        }
                        else
                        {
                            //有料無帳
                            #region [ Add To Check Fail Message To Job ]

                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Slot Job({2}_{3}) on Port({4}) cannot find in WIP  (JobManager)",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, slot_cst_seq_no, slot_job_seq_no, curPort.Data.PORTNO);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Slot Job({2}_{3}) on Port({4}) cannot find in WIP (JobManager)",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, slot_cst_seq_no, slot_job_seq_no, curPort.Data.PORTNO);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                                //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) Slot Job({3}_{4}) on Port({5}) cannot find in WIP (JobManager)",
                                //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, slot_cst_seq_no, slot_job_seq_no, curPort.Data.PORTNO);

                                failMsg = string.Format("Job({0}_{1}) Slot Job({2}_{3}) on Port({4}) cannot find in WIP (JobManager)",
                                                        curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, slot_cst_seq_no, slot_job_seq_no, curPort.Data.PORTNO);

                                AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                #endregion
                            }

                            #endregion
                            curStepStage.UnloaderPortSlotJobDCRandSorterFlag = "ERROR";
                            strStageSlotJobDCRandSorterFlags += string.Format("StageID({0}) curStageUnloaderPortSlotJobDCRandSorterFlag({1}) SorterFlag({2}),", curStepStage.Data.STAGEID, curStepStage.UnloaderPortSlotJobDCRandSorterFlag, getDetailSortFlag(curStepStage.UnloaderPortSlotJobDCRandSorterFlag));
                            curStageList.Remove(curStepStage);
                            continue;
                        }
                    }
                    else
                    {
                        //空Cassette 也要比手上的flag 是不是有一致，沒有的話一樣不能取！Watson Modify 20160317
                        #region 兩片欲取玻璃不能不一致

                        #endregion

                        //空Cassette
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) Find a Empty Cassette on Port({6})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curPort.Data.PORTNO);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion
                        //找到符合的Port Remove FailCode
                        curStepStage.UnloaderPortSlotJobDCRandSorterFlag = string.Empty;
                        strStageSlotJobDCRandSorterFlags += string.Format("StageID({0}) curStageUnloaderPortSlotJobDCRandSorterFlag({1}) SorterFlag({2}),", curStepStage.Data.STAGEID, curStepStage.UnloaderPortSlotJobDCRandSorterFlag, getDetailSortFlag(curStepStage.UnloaderPortSlotJobDCRandSorterFlag));
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                        continue;
                    }
                    #endregion
                }
                #endregion

                if (curStageList.Count > 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Unloader Rule By Job EQP Flag Code is Match!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) Filter Unloader Rule By Job EQP Flag is Fail! No Any Stage could Receive glass.(0:No Flag Glass,1:DCR Flag Glass,2:Sorter Flag Glass,3:DCR and Sorter Flag Glass). 1.Sorter Flag(2 or 3) 2.Sorter Flag(0 or 1) input same unloading port",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                        curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) curBcsJob EqpFlag_DCRandSorterFlag({5}) SorterFlag({6}) But {7} Filter Unloader Rule By Job EQP Flag is Fail! No Any Stage could Receive glass.(EqpFlag_DCRandSorterFlag：(1) 0:No Flag Glass1:DCR Flag Glass,Sorter Flag =false(2) 2:Sorter Flag Glass 3:DCR and Sorter Flag Glass,Sorter Flag =true",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                           curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJobEqpFlag_DCRandSorterFlag, getDetailSortFlag(curBcsJobEqpFlag_DCRandSorterFlag), strStageSlotJobDCRandSorterFlags);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleByEQPFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleByEQPFlag_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
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

        /// <summary> Cell 1Arm2Job Sorter Mode Unloader收片邏輯, 檢查JobData Sampling Flag, 相同的Job要收在同一Cassette裡
        ///  
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0048")]
        public bool Filter_ToUnloaderBySamplingFlag(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            //20160114 add by Port FailMsg and Code for Unloader DispatchRule
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
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

                #region [ Get curBcsJob Entity and Decode EQPFlag ]
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ checkStep by is2ndCmdFlag ]

                string tmpStepAction = string.Empty;
                int tmpStepNo = 0;
                string funcName = string.Empty;

                if (is2ndCmdFlag == false)
                {

                    tmpStepNo = curBcsJob.RobotWIP.CurStepNo;
                    funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                }
                else
                {

                    tmpStepNo = curBcsJob.RobotWIP.NextStepNo;
                    funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;
                }

                #endregion

                #region [ Get tmp Step Entity ]

                RobotRouteStep tmpRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[tmpStepNo];

                //找不到 CurStep Route 回NG
                if (tmpRouteStep == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Get curRouteStep({5})!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, tmpStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job CassetteSequenceNo({1}) JobSequenceNo({2}) can not Get curRouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo, tmpStepNo.ToString());

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
                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4})  StageIDList({5}) can not Find Stage Status is LDRQ!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion

                #region [ Get Stage is must PUT]

                if (tmpRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) Check StepID({5}) Action({6}) is Not PUT!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.ROBOTACTION);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4}) Action({5}) is Not PUT!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.ROBOTACTION);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CheckStepAction_Is_Error);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_CheckStepAction_Is_Error;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region [ Check Job Sampling Flag by CurCanUseStageList ]

                string[] stageList = tmpRouteStep.Data.STAGEIDLIST.Split(',');
                string strUnloaderSamplingFlags = "";
                for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                {
                    #region [ Get StepStage Entity ]

                    RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);

                    if (curStepStage == null)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, tmpRouteStep.Data.STEPID, tmpRouteStep.Data.STAGEIDLIST, stageList[stageIdx]);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        errMsg = string.Format("[{0}] can not Get RobotStageInfo by StageID({1}!",
                            MethodBase.GetCurrentMethod().Name, stageList[stageIdx]);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_Stage_Is_Null);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    curStepStage.UnloaderPortSlotJobDCRandSorterFlag = string.Empty;
                    #endregion

                    #region [ 判斷Current Step Stage 是否存在於Current LDRQ Stage List ]

                    if (curStageList.Contains(curStepStage) == false)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, tmpRouteStep.Data.STEPID, tmpRouteStep.Data.STAGEIDLIST, curStepStage.Data.STAGEID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不存在則視同判斷失敗直接記Log跳下一個Stage
                        continue;

                    }

                    #endregion

                    #region [ Check Stage is Unloading Port ]

                    if (curStepStage.Data.STAGETYPE != eRobotStageType.PORT)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) StageType({3}) can not Check by SetCode!",
                                                    curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, curStepStage.Data.STAGETYPE);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不是Port Stage則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }

                    Port curPort = ObjectManager.PortManager.GetPort(curStepStage.Data.STAGEID);

                    if (curPort == null)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Port Entity By StageID({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curStepStage.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //找不到Port Entity則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }

                    if (curPort.File.Type != ePortType.UnloadingPort)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) is not a Unloading Port!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curStepStage.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不是Unloading Port則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }

                    #endregion

                    #region [ Check Unloader UnloaderSamplingFlag ]
                    strUnloaderSamplingFlags += string.Format("PortID({0}) UnloaderSamplingFlag({1}),", curStepStage.Data.STAGEID, curStepStage.UnloaderSamplingFlag.ToString());
                    fail_ReasonCode = string.Format("JobFilterService_Filter_ToUnloaderBySamplingFlag_ForCellSorterMode_{0}_{1}", tmpStepNo.ToString(), curStepStage.Data.STAGEID);
                    if (curBcsJob.SamplingSlotFlag == "1")
                    {
                        if (curStepStage.UnloaderSamplingFlag == RobotStage.UNLOADER_SAMPLING_FLAG.EMPTY ||
                            curStepStage.UnloaderSamplingFlag == RobotStage.UNLOADER_SAMPLING_FLAG.SAMPLING_FLAG_ON)
                        {
                            RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                            continue;
                        }
                        else
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) UnloaderSamplingFlag({3}) is not match with Job Sampling Flag({4}",
                                                        curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, curStepStage.UnloaderSamplingFlag.ToString(), curBcsJob.SamplingSlotFlag);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            curStageList.Remove(curStepStage);
                            continue;
                        }
                    }
                    else
                    {
                        if (curStepStage.UnloaderSamplingFlag == RobotStage.UNLOADER_SAMPLING_FLAG.EMPTY ||
                            curStepStage.UnloaderSamplingFlag == RobotStage.UNLOADER_SAMPLING_FLAG.SAMPLING_FLAG_OFF)
                        {
                            RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                            continue;
                        }
                        else
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) UnloaderSamplingFlag({3}) is not match with Job Sampling Flag({4}",
                                                        curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, curStepStage.UnloaderSamplingFlag.ToString(), curBcsJob.SamplingSlotFlag);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            curStageList.Remove(curStepStage);
                            continue;
                        }
                    }
                    #endregion
                }
                #endregion

                if (curStageList.Count > 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Unloader Rule By Job Sampling Flag is Match!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) curStepNo({5}) Filter Unloader Rule By Job Sampling Flag is Fail! No Any Stage could Receive glass.(can't find Job Sampling Slot Flag = Unloading port Exist Job Sampling Slot Flag)",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) curStepNo({4}) curJob's SamplingSlotFlag({5}) But {6} Filter Unloader Rule By Job Sampling Flag is Fail! No Any Stage could Receive glass.",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString()
                                           , curBcsJob.SamplingSlotFlag, strUnloaderSamplingFlags);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleByEQPFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleByEQPFlag_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
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
        /// Jackwang add 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0052")]
        public bool Filter_ULDPortDipatchRuleBySameJobGrade(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            //20160114 add by Port FailMsg and Code for Unloader DispatchRule
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
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

                #region [ Get curBcsJob Entity and Decode EQPFlag ]
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ checkStep by is2ndCmdFlag ]

                string tmpStepAction = string.Empty;
                int tmpStepNo = 0;
                string funcName = string.Empty;

                if (is2ndCmdFlag == false)
                {

                    tmpStepNo = curBcsJob.RobotWIP.CurStepNo;
                    funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                }
                else
                {

                    tmpStepNo = curBcsJob.RobotWIP.NextStepNo;
                    funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;
                }

                #endregion

                #region [ Get tmp Step Entity ]

                RobotRouteStep tmpRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[tmpStepNo];

                //找不到 CurStep Route 回NG
                if (tmpRouteStep == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Get curRouteStep({5})!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, tmpStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job CassetteSequenceNo({1}) JobSequenceNo({2}) can not Get curRouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo, tmpStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Current Stage List ]

                List<RobotStage> curStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curStepCanUseStageList == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepID({5}) StageIDList({6}) can not Find Stage Status is LDRQ!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4})  StageIDList({5}) can not Find Stage Status is LDRQ!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Stage is must PUT]

                if (tmpRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) Check StepID({5}) Action({6}) is Not PUT!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.ROBOTACTION);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4}) Action({5}) is Not PUT!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, tmpRouteStep.Data.STEPID.ToString(), tmpRouteStep.Data.ROBOTACTION);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CheckStepAction_Is_Error);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_CheckStepAction_Is_Error;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region [ Get Line Itme ]

                Line curline = ObjectManager.LineManager.GetLine(Workbench.ServerName);

                if (curline == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Get Line entity by ServerName({5})!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, Workbench.ServerName);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Line entity by ServerName({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo, Workbench.ServerName);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_LineByServerName_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Line Unloader Dispatch Rule List ]

                if (curline.File.UnlaoderDispatchRule.Count == 0)
                {
                    fail_ReasonCode = string.Format("JobFilterService_Filter_ULDPortDipatchRuleByJobGrade_{0}", tmpStepNo.ToString());

                    if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) JobGrade({6}), But Line has no UnloadDispatchRule",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.JobGrade);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) JobGrade({3}), But Line has no UnloaderDispatchRule",
                        //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.JobGrade);

                        failMsg = string.Format("Job({0}_{1}) curRouteID({2}) curStepNo({3}) JobGrade({4}), But Line has no UnloaderDispatchRule",
                                               curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                               curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.JobGrade);

                        AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get UnloaderDispatchRule because Line UnloaderDisptchRule count is empty!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get UnloaderDispatchRule because Line UnloaderDisptchRule count is empty!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_LineUnloaderDispatchRule_Is_Empty);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Job_Get_LineUnloaderDispatchRule_Is_Empty;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion



                string[] stageList = tmpRouteStep.Data.STAGEIDLIST.Split(',');
                List<RobotStage> JobSameGreadStageList = new List<RobotStage>();
                for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                {
                    #region [ Get StepStage Entity ]

                    RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);

                    if (curStepStage == null)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, tmpRouteStep.Data.STEPID, tmpRouteStep.Data.STAGEIDLIST, stageList[stageIdx]);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        errMsg = string.Format("[{0}] can not Get RobotStageInfo by StageID({1}!",
                            MethodBase.GetCurrentMethod().Name, stageList[stageIdx]);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_Stage_Is_Null);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    #endregion

                    #region [ Check Stage is Unloading Port ]

                    if (curStepStage.Data.STAGETYPE != eRobotStageType.PORT)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) StageType({3}) can not Check by SameJobGrade!",
                                                    curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, curStepStage.Data.STAGETYPE);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不是Port Stage則記Log直接跳下一個Stage
                        curStepCanUseStageList.Remove(curStepStage);
                        continue;
                    }

                    Port curPort = ObjectManager.PortManager.GetPort(curStepStage.Data.STAGEID);

                    if (curPort == null)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Port Entity By StageID({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curStepStage.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //找不到Port Entity則記Log直接跳下一個Stage
                        curStepCanUseStageList.Remove(curStepStage);
                        continue;
                    }

                    if (curPort.File.Type != ePortType.UnloadingPort)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) is not a Unloading Port!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curStepStage.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不是Unloading Port則記Log直接跳下一個Stage
                        curStepCanUseStageList.Remove(curStepStage);
                        continue;
                    }

                    #endregion

                    #region [ Get Port Unloader Dispatch Rule by StageID ]

                    clsDispatchRule curdispatchRule = curline.File.UnlaoderDispatchRule[curStepStage.Data.STAGEID];

                    if (curdispatchRule == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) can not get Dispatch Rule by StageID({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curStepStage.Data.STAGEID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //找不到Port Dispatch Rule則記Log直接跳下一個Stage
                        continue;
                    }

                    #endregion

                    #region [ Check Job特定Grade 如果前後片Grade不同就只能去MIX Port ]

                    //201601114 add byPort FailCode for Unloader Dispatch Rule
                    fail_ReasonCode = string.Format("JobFilterService_Filter_ULDPortDipatchRuleByJobGrade_{0}_{1}", tmpStepNo.ToString(), curStepStage.Data.STAGEID);

                    // Cell Special 如果前後片Grade不同就只能去MIX Port
                    if (curBcsJob.RobotWIP.OnlyToMixGradeULDFlag == true)
                    {
                        robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                        return true;
                    }
                    else
                    {
                        #region [ Check Match Grade and Update Priority ]

                        if ((curdispatchRule.Grade1.ToUpper().Trim() == curBcsJob.JobGrade.Trim()) ||
                            (curdispatchRule.Grade2.ToUpper().Trim() == curBcsJob.JobGrade.Trim()) ||
                            (curdispatchRule.Grade3.ToUpper().Trim() == curBcsJob.JobGrade.Trim()))
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) JobGrade({4}), Port({5}) UnloaderDispatchRule Grade#1({6}) Grade#2({7}) Grade#3({8}) is match!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.JobGrade, curStepStage.Data.STAGEID, curdispatchRule.Grade1.ToUpper(), curdispatchRule.Grade2.ToUpper(),
                                                        curdispatchRule.Grade3.ToUpper());
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion
                            JobSameGreadStageList.Add(curStepStage);
                            continue;

                        }

                        #endregion

                    }
                    #endregion

                }
                List<RobotStage> tempRobotStageList = new List<RobotStage>();
                //若找到与Job Grade 相同的Prot ，就只留下这个Port
                if (JobSameGreadStageList.Count > 0)
                {
                    foreach (RobotStage stage in curStepCanUseStageList)
                    {
                        if (JobSameGreadStageList.Contains(stage))
                            tempRobotStageList.Add(stage);
                    }
                }
                else  //如果沒找到相同的Grade Dispatch,就傳回true,讓下一個Filter_ULDPortDipatchRuleByJobGrade去卡EM/MX
                {
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                    return true;
                }
                curStepCanUseStageList = tempRobotStageList;
                if (curStepCanUseStageList.Count > 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Unloader Dispatch Rule By SameJobGrade Check is Match!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) Filter Unloader Dispatch Rule By SameJobGrade Check Fail! No Any Stage could Receive glass.(can't find port DispatchRule Match Grade)",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) Filter Unloader Dispatch Rule By Job SameJobGrade Check Fail! No Any Stage could Receive glass.(can't find port DispatchRule Match Grade)",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                           curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleBySameJobGrade_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleBySameJobGrade_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
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
        /// Yang
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0051")]
        public bool Filter_DRYMixNoFetchOutRule(IRobotContext robotConText)
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion
                #region[Check MIX_MODE]
                if (robotLine.File.IndexOperMode != eINDEXER_OPERATION_MODE.MIX_MODE) //if !MIX Run,return true,not check chamber mode
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Indexer Oper Mode Is Not MIX_MODE , No Need Check Chamber Mode",
                                        curRobot.Data.NODENO);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }
                    return true;

                    #endregion
                }
                #endregion
                string _curUnitNo = (string)curRobot.Context[eRobotContextParameter.UnitNo];
                string MixNochambermode = (string)curRobot.Context[eRobotContextParameter.chambermode];
                #region [Check curUnit]
                if (string.IsNullOrEmpty(_curUnitNo))
                {
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curUnit is Null!",
                                                "L4", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    errMsg = string.Format("[{0}] curUnit is Null!",
                                            MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_No_Can_Use_Chamber);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_No_Can_Use_Chamber;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }

                Unit curUnit = ObjectManager.UnitManager.GetUnit("L4", _curUnitNo);
                if (curUnit == null)
                {
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Find Unit!",
                                                "L4", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    errMsg = string.Format("[{0}] can not Find Unit!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_No_Can_Use_Chamber);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_No_Can_Use_Chamber;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                #endregion

                if (curBcsJob.ArraySpecial.ProcessType.Equals(MixNochambermode) && (curUnit.File.Status == eEQPStatus.RUN || curUnit.File.Status == eEQPStatus.IDLE))
                {
                    curRobot.ReCheck = false;
                    return true;
                }
                else
                {
                    curRobot.ReCheck = true;
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) curStepNo({5}) Filter DRY UnitNo Fetch Out Rule NG",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) curStepNo({4}) curJob's ProcessType({5}) Can't Send to Current Unit,Please Check 1.Chamber Mode,2.Unit Status ",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString()
                                           , curBcsJob.ArraySpecial.ProcessType);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Filter_DRYMixNoFetchOutRule);
                    robotConText.SetReturnMessage(errMsg);

                    //errCode = eJobFilter_ReturnCode.NG_Filter_DRYMixNoFetchOutRule;//add for BMS Error Monitor  ,mark by yang 2017/3/22
                    //if (!curRobot.CheckErrorList.ContainsKey(errCode))
                    //    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT")); 
                    return false;
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
        /// Add by Yang for CVD chamber cleaned,exchange refer to CleanOut Bit
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0056")]
        public bool Filter_CVDexchangeByCleanOut(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;
            eBitResult l1cleanout;
            eBitResult l2cleanout;
            RobotRouteStep curRouteStep = new RobotRouteStep();
            List<RobotStage> curCanUseStageList = null;
            curCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];//物件不要再组,抓之前判断传进的stagelist
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [Get curRouteStep]

                if (!is2ndCmdFlag)
                {
                    #region [ Get Current Step Entity ]
                    curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
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

                    #region [ Check CurStep Action Must GET ][好像不卡没什么影响]
                    /*
                    if (curRouteStep.Data.ROBOTACTION != eRobotCmdActionCode.GET.ToString())
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) is illegal!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.ROBOTACTION);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        errMsg = string.Format("[{0}] Job({1}_{2}) StageID({3}) StepNo({4}) Action({5}) is illegal!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.ROBOTACTION);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NotArmJob_StepAction_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                    */
                    #endregion
                }
                else
                {
                    #region [ Get Next Step Entity ]
                    curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
                    //找不到 CurStep Route 回NG
                    if (curRouteStep == null)
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get NextRouteStep({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.NextStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        errMsg = string.Format("[{0}]Job({1}_{2}) can not Get JobcurRouteStep({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                    #endregion

                    #region [ Check CurStep Action Must PUT ][好像不卡没什么影响]
                    /*
                    if (curRouteStep.Data.ROBOTACTION != eRobotCmdActionCode.PUT.ToString())
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) is illegal!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.ROBOTACTION);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        errMsg = string.Format("[{0}] Job({1}_{2}) StageID({3}) StepNo({4}) Action({5}) is illegal!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.ROBOTACTION);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NotArmJob_StepAction_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                    */
                    #endregion
                }
                #endregion

                #region[Check InterLock BitResult,目前回false的情况，是LL1 && LL2都不接受玻璃]
                l1cleanout = Check_CVD_EQInterLock_LoadLock1CleanOutBit(curRobot);
                l2cleanout = Check_CVD_EQInterLock_LoadLock2CleanOutBit(curRobot);

                if (l1cleanout == eBitResult.ON && l2cleanout == eBitResult.ON)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO({5}) EQ to EQ InterLock CleanOut Bit[{6},{7}] can not send glass to CVD !",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), l1cleanout, l2cleanout);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) EQ to EQ InterLock CleanOut Bit[{5},{6}]  can not send glass to CVD !",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                             l1cleanout, l2cleanout);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DailyCheck_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_DailyCheck_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }
                else
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) EQ to EQ InterLock CleanOut Bit [LL1({4}),LL2({5})] ",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, l1cleanout, l2cleanout);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                }
                #endregion

                #region  [传进的stagelist,过滤掉在自净的LL(stage),Filter加在LDRQ之后 ]

                if (curCanUseStageList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find Stage Status is LDRQ!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepNo({4}) StageIDList({5}) can not Find Stage Status is LDRQ!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }
                //增加防呆
                if (!curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curRouteStep.Data.STEPID))
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) {7} Rule Job RouteStepByPass Current StepNo({4}) but the Job Route max StepNo is {5} End{6}",
                            curRobot.Data.NODENO,
                            curRobot.Data.ROBOTNAME,
                            curBcsJob.CassetteSequenceNo,
                            curBcsJob.JobSequenceNo,
                            curRouteStep.Data.STEPID.ToString(),
                            curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString(),
                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH),
                            (!is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    return false;
                }

                string[] curStepCanUseStageList = curBcsJob.RobotWIP.RobotRouteStepList[curRouteStep.Data.STEPID].Data.STAGEIDLIST.Split(',');

                for (int i = 0; i < curStepCanUseStageList.Length; i++)
                {
                    RobotStage curStage;

                    #region [ Check Stage is Exist ]
                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                curRobot.Data.NODENO,
                                MethodBase.GetCurrentMethod().Name,
                                curRobot.Data.ROBOTNAME,
                                curStepCanUseStageList[i]);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }
                    #endregion

                    #region [Remove CVD Stage can not receive job By Interlock Bit  ]

                    if (curCanUseStageList.Contains(curStage))
                    {
                        if (curStage.Data.REMARKS.Equals("LL1"))
                        {
                            if (Check_CVD_EQInterLock_LoadLock1CleanOutBit(curRobot) == eBitResult.ON) curCanUseStageList.Remove(curStage);
                        }
                        else if (curStage.Data.REMARKS.Equals("LL2"))
                        {
                            if (Check_CVD_EQInterLock_LoadLock2CleanOutBit(curRobot) == eBitResult.ON) curCanUseStageList.Remove(curStage);
                        }
                        //这边remove掉CleanOut的stage Yang
                    }
                    continue;

                    #endregion
                }
                #endregion

                // robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList,curCanUseStageList);//加进context,后面filter会用 Yang
                //直接更新就好
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
        /// add by yang for RTC Job ,管控CST内RTC job count(目前for CVD CLN cleaned RTC)
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0058")]
        public bool Filter_RTCJobCount(IRobotContext robotConText)
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



                if (curRobot.noSendToCLN)  //add for stop send to CLN
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("curEQPRTCJobCount > maxPermitRTCCount!,Cant't Send Glass To CLN! ");

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("curEQPRTCJobCount > maxPermitRTCCount!,Cant't Send Glass To CLN! ");

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Exception;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                if (!curRobot.fetchforRTC)  //add for glass continue fetch out from cst to CLN
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("curEQPRTCJobCount > maxPermitFetchOutFromCSTCount!,Cant't Send Glass To CLN! ");

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("curEQPRTCJobCount > maxPermitFetchOutFromCSTCount!,Cant't Send Glass To CLN! ");

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Exception;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
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
        /// jack  （not in use）
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0055")]
        public bool Filter_DRYFetchOutByChamberMode(IRobotContext robotConText)
        {

            string strlog = string.Empty;
            string errMsg = string.Empty;
            bool MixRunFlag;
            bool ChamberNotInUseFlag;
            int curUnitNo;
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion
                #region [Get curUnit]
                string _curUnitNo = (string)curRobot.Context[eRobotContextParameter.UnitNo];
                string MixNochambermode = (string)curRobot.Context[eRobotContextParameter.chambermode];
                //  string Nextchambermode = (string)curRobot.Context[eRobotContextParameter.nextchambermode];

                // if (string.IsNullOrEmpty(MixNochambermode)||string.IsNullOrEmpty(_curUnitNo)) return false;
                if (string.IsNullOrEmpty(_curUnitNo)) return false;

                Unit curUnit = ObjectManager.UnitManager.GetUnit("L4", _curUnitNo);
                if (curUnit == null)
                {
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Find Unit!",
                                                "L4", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    errMsg = string.Format("[{0}] can not Find Unit!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_No_Can_Use_Chamber);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                if (robotLine.File.IndexOperMode != eINDEXER_OPERATION_MODE.MIX_MODE)//if !MIX Run,return true,not check chamber mode
                {
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                    return true;
                }
                if (curBcsJob.ArraySpecial.ProcessType == curRobot.File.DryRealTimeChamberMode)
                {
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                    return true;
                }
                else
                {
                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) Filter DRY FetchOut By ChamberMode Check Fail! curJob's ProcessType({5}) But curDRY EQP Want To Recive Chamber Mode({6}) ,No Any Stage could Receive glass.",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                           curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.ArraySpecial.ProcessType, curRobot.File.DryRealTimeChamberMode);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_MisMatchChamberMode);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
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
        /// Add by box.zhai for PDR 去看一下CEM是否需要出片、要片；机台PDR不在出片、要片的情况下，如果要，Filter NG，反之OK 2017/4/19
        /// 还未完成
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0060")]
        public bool Filter_PDRForCEMSampling(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

            bool PDR_RECIVEorSENDOUT=false;
            bool CEM_RECIVEorSENDOUT=false;

            RobotRouteStep curRouteStep = new RobotRouteStep();
            List<RobotStage> allRobotStageList = null;
            allRobotStageList=ObjectManager.RobotStageManager.GetRobotStages();
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

                #region PDR不收送片，CEM收送片时PDR Filter Fail

                foreach (RobotStage robotStage in allRobotStageList)
                {
                    if (robotStage.Data.STAGEID == "11" || robotStage.Data.STAGEID == "12")
                    {
                        if (robotStage.File.CurStageStatus == eRobotStageStatus.RECEIVE_READY || robotStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY || robotStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                        {
                            PDR_RECIVEorSENDOUT=true;
                            break;
                        }
                    }
                    if (robotStage.Data.STAGEID == "13")
                    {
                        if (robotStage.File.CurStageStatus == eRobotStageStatus.RECEIVE_READY || robotStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY)
                        {
                            CEM_RECIVEorSENDOUT = true;
                            break;
                        }
                    }
                }

                if (!PDR_RECIVEorSENDOUT && CEM_RECIVEorSENDOUT)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] PDR Not LDRQorULRQ && CEM LDRQorULRQ!!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] PDR Not LDRQorULRQ && CEM LDRQorULRQ!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_PDRNotRequest_CEMRequest);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

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

        [UniAuto.UniBCS.OpiSpec.Help("FL0061")]
        public bool Filter_CEMForCEMSampling(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

            bool PDR_RECIVEorSENDOUT = false;

            RobotRouteStep curRouteStep = new RobotRouteStep();
            List<RobotStage> allRobotStageList = null;
            allRobotStageList = ObjectManager.RobotStageManager.GetRobotStages();
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

                #region PDR不收送片，CEM收送片时PDR Filter Fail

                foreach (RobotStage robotStage in allRobotStageList)
                {
                    if (robotStage.Data.STAGEID == "11" || robotStage.Data.STAGEID == "12")
                    {
                        if (robotStage.File.CurStageStatus == eRobotStageStatus.RECEIVE_READY || robotStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY || robotStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                        {
                            PDR_RECIVEorSENDOUT = true;
                            break;
                        }
                    }
                }

                if (PDR_RECIVEorSENDOUT)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] PDR LDRQorULRQ!!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] PDR LDRQorULRQ!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_PDRNotRequest_CEMRequest);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

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

        //add qiumin  for DRY 控制tact time 用避免DRY连续出片导致节拍变化
        [UniAuto.UniBCS.OpiSpec.Help("FL0062")]
        public bool Filter_DRYSENDOUTTIMECONTROL(IRobotContext robotConText)
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion
                #region[Check MIX_MODE]
                if (robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE) //if !MIX Run,return true,not check chamber mode
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Indexer Oper Mode Is Not MIX_MODE , No Need Check DRY send out time",
                                        curRobot.Data.NODENO);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }
                    return true;

                    #endregion
                }
                #endregion

                int delaytime = 30;
                double  tmptime;
                Equipment eqp=ObjectManager .EquipmentManager .GetEQP (curBcsJob.CurrentEQPNo);
                tmptime = (DateTime .Now  - eqp.File.LastSendOutGlassTime).TotalSeconds;
                if (ParameterManager.Parameters.ContainsKey("DRY_SEND_OUT_CHECK"))
                {
                    int.TryParse(ParameterManager.Parameters["DRY_SEND_OUT_CHECK"].Value.ToString(), out delaytime);  
                }
                if (tmptime > delaytime) return true;
                    else return false;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        // add qiumin 20170830 for ATS400 ,robot 为4 fork and6 fork，手臂和ATS 里面只能有一片glass
        [UniAuto.UniBCS.OpiSpec.Help("FL0063")]
        public bool Filter_PortFetchOutLineNoGlass_For1Arm1Job(IRobotContext robotConText)
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

                #region [ Check CurStep Action Must GET ]

                if (curRouteStep.Data.ROBOTACTION != eRobotCmdActionCode.GET.ToString())
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.ROBOTACTION);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) StageID({3}) StepNo({4}) Action({5}) is illegal!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.ROBOTACTION);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NotArmJob_StepAction_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //     errCode = eJobFilter_ReturnCode.NG_NotArmJob_StepAction_Is_Fail;//add for BMS Error Monitor
                    //     if (!curRobot.CheckErrorList.ContainsKey(errCode))
                    //          curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region [ Check ATS400 EQ&Robot have glass ]

                #region [ Get Arm Can Control Job List ]

                List<Job> robotArmCanControlJobList;

                robotArmCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.ArmCanControlJobList];

                //當沒有設定參數時會回傳NULL,需防呆
                if (robotArmCanControlJobList == null)
                {
                    robotArmCanControlJobList = new List<Job>();
                }

                #endregion

                #region [ Get EQP info]
                Equipment  eqp=ObjectManager .EquipmentManager.GetEQP("L3");
                #endregion
                if (robotArmCanControlJobList.Count != 0||eqp.File.TotalTFTJobCount!=0||eqp.File.TotalDummyJobCount!=0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO ({5})Eqp  and Robot have glass can not Fetch Out!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo );

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO ({5}) Eqp  and Robot have glass can not Fetch Out!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_FetchOutEqpHaveGlass_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    //       errCode = eJobFilter_ReturnCode.NG_FetchOutSampingFlag_Is_Fail;//add for BMS Error Monitor
                    //      if (!curRobot.CheckErrorList.ContainsKey(errCode))
                    //       curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion

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

        // add qiumin 20170830 for ATS400 ,glass 回CST时turn flag必须off 和turn angle 必须为0
        [UniAuto.UniBCS.OpiSpec.Help("FL0064")]
        public bool Filter_PortStoreByTurnFlagAndAngle_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;
            string curBcsJobEqpFlag_TurnAngle = string.Empty;
            string curBcsJobEqpFlag_TurnFlag = string.Empty;
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

                #region [ Check CurStep Action Must GET ]
/*
                if (curRouteStep.Data.ROBOTACTION != eRobotCmdActionCode.PUT.ToString())
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.ROBOTACTION);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) StageID({3}) StepNo({4}) Action({5}) is illegal!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curRouteStep.Data.ROBOTACTION);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NotArmJob_StepAction_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //     errCode = eJobFilter_ReturnCode.NG_NotArmJob_StepAction_Is_Fail;//add for BMS Error Monitor
                    //     if (!curRobot.CheckErrorList.ContainsKey(errCode))
                    //          curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }
                */
                #endregion

                #region [ Check ATS400 TurnFlag And Angle ]

                #region [ Get TurnFlag And Angle ]

                IDictionary<string, string> curBcsJobEqpFlag = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG , eJOBDATA.EQPFlag);
                //0:Turn Angle=0,1:Turn Angle=90,2:Turn Angle=180,3:Turn Angle=270
                if (curRobot.Data.LINEID == "TCATS400")
                {
                    curBcsJobEqpFlag_TurnAngle = curBcsJobEqpFlag["TurnAngle"];
                    curBcsJobEqpFlag_TurnFlag = curBcsJobEqpFlag["TurnFlag"];
                    if (curBcsJobEqpFlag_TurnAngle == null || curBcsJobEqpFlag_TurnFlag == null)
                    {
                        return false;
                    }
                }

                #endregion

                if (curBcsJobEqpFlag_TurnAngle != "0" || curBcsJobEqpFlag_TurnFlag != "0" )
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO ({5})Job TurnFlag =({6}) And Angle=({7}) is not 0 ,can not return to CST !",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo, curBcsJobEqpFlag_TurnAngle, curBcsJobEqpFlag_TurnFlag);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO ({5})Job TurnFlag And Angle is not 0 ,can not return to CST!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_PortStoreByTurnFlagAndAngle_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    //       errCode = eJobFilter_ReturnCode.NG_FetchOutSampingFlag_Is_Fail;//add for BMS Error Monitor
                    //      if (!curRobot.CheckErrorList.ContainsKey(errCode))
                    //       curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion

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


        //add qiumin 20171017 for ela ,glass go to ela1 or ela2 check 
        [UniAuto.UniBCS.OpiSpec.Help("FL0065")]
        public bool Filter_ELAEQPTypeCheck(IRobotContext robotConText)
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region Check Run Mode Mismatch. 不需要Check
                //Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                //#region MQC Mode
                //if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)
                //{
                //    if (!curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                //    {
                //        #region[DebugLog]
                //        if (IsShowDetialLog == true)
                //        {
                //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Line Indexer Run Mode({2}) is Not Match, Filter CVD Proportinoal Rule Setting Check NG.",
                //                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, line.File.IndexOperMode.ToString());

                //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //        }
                //        #endregion
                //        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CVDFetchGlassProportionalRule_Fail);
                //        robotConText.SetReturnMessage(errMsg);

                //        return false;
                //    }
                //}
                //#endregion

                //#region MIX Mode
                //if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                //{
                //    if (!curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                //    {
                //        #region[DebugLog]
                //        if (IsShowDetialLog == true)
                //        {
                //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Line Indexer Run Mode({2}) is Not Match, Filter CVD Proportinoal Rule Setting Check NG.",
                //                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, line.File.IndexOperMode.ToString());

                //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //        }
                //        #endregion
                //        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CVDFetchGlassProportionalRule_Fail);
                //        robotConText.SetReturnMessage(errMsg);
                //        return false;
                //    }

                //    if (!curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                //    {
                //        #region[DebugLog]
                //        if (IsShowDetialLog == true)
                //        {
                //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Line Indexer Run Mode({2}) is Not Match, Filter CVD Proportinoal Rule Setting Check NG.",
                //                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, line.File.IndexOperMode.ToString());

                //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //        }
                //        #endregion
                //        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CVDFetchGlassProportionalRule_Fail);
                //        robotConText.SetReturnMessage(errMsg);
                //        return false;
                //    }
                //}
                //#endregion

                //#region Noraml Mode
                //if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE)
                //{
                //    if (!curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                //    {
                //        #region[DebugLog]
                //        if (IsShowDetialLog == true)
                //        {
                //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Line Indexer Run Mode({2}) is Not Match, Filter CVD Proportinoal Rule Setting Check NG.",
                //                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME,line.File.IndexOperMode.ToString());

                //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //        }
                //        #endregion
                //        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CVDFetchGlassProportionalRule_Fail);
                //        robotConText.SetReturnMessage(errMsg);

                //        return false;
                //    }
                //}
                //#endregion
                #endregion

                #region[MQC EQP RTC Glass fetch out again, no need check process type]
                //add by  yang 20161118 
                if (curBcsJob.RobotWIP.EQPRTCFlag)
                {
                    if (curBcsJob.ArraySpecial.ProcessType == "1")
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) EQPRTC Job CassetteSequenceNo({2}) JobSequenceNo({3}) curProcessType is({4}),not need Check!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.ArraySpecial.ProcessType);

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                        return true;
                    }
                }
                #endregion

                #region 現在的Robot CurELAEQPType 与Job的ELA1BY1Flag

                if (curBcsJob.ArraySpecial.ProcessType.Trim().Equals("1"))
                {
                    return true;
                }

                #region [ Get Stage Can Control Job List ]

                List<Job> robotStageCanControlJobList;

                robotStageCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.StageCanControlJobList];

                //當沒有設定參數時會回傳NULL,需防呆
                if (robotStageCanControlJobList == null)
                {
                    robotStageCanControlJobList = new List<Job>();
                }

                #endregion

                int ela1jobcount = robotStageCanControlJobList.Where(j => j.RobotWIP.CurStepNo == 1 && j.ArraySpecial.ELA1BY1Flag == "L4" && j.SamplingSlotFlag == "1").ToList().Count;
                int ela2jobcount = robotStageCanControlJobList.Where(j => j.RobotWIP.CurStepNo == 1 && j.ArraySpecial.ELA1BY1Flag == "L5" && j.SamplingSlotFlag == "1").ToList().Count;
                int ela12jobcount = robotStageCanControlJobList.Where(j => j.RobotWIP.CurStepNo == 1 && j.ArraySpecial.ELA1BY1Flag == "L45" && j.SamplingSlotFlag == "1").ToList().Count;
                Equipment ela1 = ObjectManager.EquipmentManager.GetEQP("L4");
                Equipment ela2 = ObjectManager.EquipmentManager.GetEQP("L5");

                #region[DebugLog 记录当前count]
                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ela1jobcount({2})  ela2jobcount({3})ela12jobcount({4}) Filter ELA EQP Type Check OK!",
                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME,ela1jobcount, ela2jobcount, ela12jobcount);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion 

                if ((ela1jobcount == 0 && ela2jobcount == 0 && ela12jobcount != 0) || (ela2jobcount == 0 && ela12jobcount == 0 && ela1jobcount != 0) || (ela1jobcount == 0 && ela12jobcount == 0 && ela2jobcount != 0))

                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ela1jobcount({2}) ela12jobcount({3}) Filter ELA EQP Type Check OK!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, ela2jobcount, ela12jobcount);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                    return true;
                }
                if (curRobot.File.CurELAEQPType == curBcsJob.ArraySpecial.ELA1BY1Flag || curRobot.File.CurELAEQPType==null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter ELA EQP Type Check OK!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                    return true;
                }
                else
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO({5})Job EQP Type({6}) is Not Match, Robot EQP Current Type are ({7})  Filter ELA EQP Type Check NG.ela1jobcount({8})  ela2jobcount({9})ela12jobcount({10})",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                        curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.ArraySpecial.ELA1BY1Flag, curRobot.File.CurELAEQPType, ela1jobcount, ela2jobcount, ela12jobcount);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) Job EQP Type({5}) is Not Match,Robot EQP Current Type are ({6}) Filter ELA EQP Type Check NG.ela1jobcount({7})  ela2jobcount({8})ela12jobcount({9})",
                                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                        curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.ArraySpecial.ELA1BY1Flag, curRobot.File.CurELAEQPType, ela1jobcount, ela2jobcount, ela12jobcount);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELAEQPTypeMismatch);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_ELAEQPTypeMismatch;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    
                    return false;
                }
                #endregion


            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }

        //add qiumin 20171017 for ela ,only ela1 glass can gto ela1,only ela2 glass can gto ela2
        [UniAuto.UniBCS.OpiSpec.Help("FL0066")]
        public bool Filter_JobELA1BY1FlagCheck_For1Arm1Job(IRobotContext robotConText)
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

                #region [ Check EQ Stage vs ELA1BY1Flag]
                string ELA1BY1Flag = curBcsJob.ArraySpecial.ELA1BY1Flag; 
                List<RobotStage> curCheckStepStageList = new List<RobotStage>();
                string[] curCheckStepStageIDList = curCheckRouteStep.Data.STAGEIDLIST.Split(',');
                if (ELA1BY1Flag != "L45")
                {
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
                            Equipment eq = ObjectManager.EquipmentManager.GetEQP(curStage.Data.NODENO);// 
                            if (ELA1BY1Flag.Equals("L4") && !eq.Data.NODENAME.ToUpper().Equals("ELA#1"))
                                continue;

                            if (ELA1BY1Flag.Equals("L5") && !eq.Data.NODENAME.ToUpper().Equals("ELA#2"))
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
                            strlog = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5}) by Job's ELA1BY1Flag({6})!",
                                                MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ELA1BY1Flag);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Check Step({3}) Stage List({4}) by Job's ELA1BY1Flag({5})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ELA1BY1Flag);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_JobProcessTypeEQRunModeCheck);
                        robotConText.SetReturnMessage(errMsg);
                        errCode = eJobFilter_ReturnCode.NG_JobProcessTypeEQRunModeCheck;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                        return false;
                    }
                }
                else
                {
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
                            Equipment eq = ObjectManager.EquipmentManager.GetEQP(curStage.Data.NODENO);// 
 

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
                            strlog = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5}) by Job's ELA1BY1Flag({6})!",
                                                MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ELA1BY1Flag);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Check Step({3}) Stage List({4}) by Job's ELA1BY1Flag({5})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ELA1BY1Flag);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_JobProcessTypeEQRunModeCheck);
                        robotConText.SetReturnMessage(errMsg);
                        errCode = eJobFilter_ReturnCode.NG_JobProcessTypeEQRunModeCheck;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                        return false;
                    }
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

        //20180502 add qiumin  for EAL 控制tact time 用避免ELA出片,错过change ，导致节拍变化
        [UniAuto.UniBCS.OpiSpec.Help("FL0067")]
        public bool Filter_ELASENDOUTTIMECONTROL(IRobotContext robotConText)
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion
                #region[Check MIX_MODE]
                if (robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE) //if !MIX Run,return true,not check chamber mode
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Indexer Oper Mode Is Not MIX_MODE , No Need Check DRY send out time",
                                        curRobot.Data.NODENO);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }
                    return true;

                    #endregion
                }
                #endregion

                Equipment ela1 = ObjectManager.EquipmentManager.GetEQP("L4");
                Equipment ela2 = ObjectManager.EquipmentManager.GetEQP("L5");
                RobotStage cln = ObjectManager.RobotStageManager.GetRobotStagebyStageID("12");

                if ((ela2.File.Status != eEQPStatus.RUN && ela2.File.Status != eEQPStatus.IDLE) || (ela1.File.Status != eEQPStatus.RUN && ela1.File.Status != eEQPStatus.IDLE) || ela1.File.EquipmentRunMode == "MQC" || ela2.File.EquipmentRunMode == "MQC"||cln.File.CurStageStatus!=eRobotStageStatus.NO_REQUEST )
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) CurStageStatus ({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cln.File.CurStageStatus);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return true;
                }
                int delaytime = 30;
                double tmptime;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curBcsJob.CurrentEQPNo);
                tmptime = (DateTime.Now - eqp.File.LastSendOutGlassTime).TotalSeconds;
                if (ParameterManager.Parameters.ContainsKey("ELA_SEND_OUT_CHECK"))
                {
                    int.TryParse(ParameterManager.Parameters["ELA_SEND_OUT_CHECK"].Value.ToString(), out delaytime);
                }
                if (tmptime > delaytime)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ELA waite CLN Time ({2})bigger Delay time({3})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, tmptime.ToString(), delaytime.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                    return true;
                }
                else
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ELA waite CLN Time ({2}) Delay time({3})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, tmptime.ToString(),delaytime.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] ELA waite CLN Time ({1}) Delay time({2})!",
                                            MethodBase.GetCurrentMethod().Name, tmptime.ToString(), delaytime.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ELADelayTime);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
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
        ///add by hujunpeng 20190228 for FCWRW100 共洗CELL DUMMY
        [UniAuto.UniBCS.OpiSpec.Help("FL0068")]
        public bool Filter_JobCurRwkCountIsMax_GotoUnloadingPort(IRobotContext robotConText)
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
                else
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) job[{2}] MaxRwkCount[{3}] CurrentRwkCount[{4}]!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME,curBcsJob.GlassChipMaskBlockID,curBcsJob.CellSpecial.MaxRwkCount,curBcsJob.CellSpecial.CurrentRwkCount);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

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
                            if (curBcsJob.CellSpecial.MaxRwkCount==curBcsJob.CellSpecial.CurrentRwkCount)
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
                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
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

    }
}