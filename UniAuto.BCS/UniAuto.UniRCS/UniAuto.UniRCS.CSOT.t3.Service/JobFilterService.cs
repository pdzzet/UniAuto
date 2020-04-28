using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.OpiSpec;

namespace UniAuto.UniRCS.CSOT.t3.Service
{

    [UniAuto.UniBCS.OpiSpec.Help("JobFilterService")]
    public partial class JobFilterService : AbstractRobotService
    {
        public override bool Init()
        {
            return true;
        }

//All Job Route Step Filter Function List [ Method Name = "Filter" + " _" +  "Condition Abbreviation" EX:"Filter_ForPVD" ]==============================================================
//Filter Funckey = "FL" + XXXX(序列號)

        /// <summary> Check Job Location StageID Is Not Arm
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0001")]
        public bool Filter_JobNotOnRobotArmByJobLocation(IRobotContext robotConText)
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

                    errMsg = string.Format("[{0}] Robot({1}) can not Get JobInfo!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                if (curBcsJob.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) but curStageID({6}) is Robot Arm({7}) is illegal!",
                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                                curBcsJob.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) but curStageID({5}) is Robot Arm({6}) is illegal!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Location_Is_Robot);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_Job_Location_Is_Robot;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

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

        /// <summary> Check Current Step Action by Job Location. EX: Location is Arm. Action不可以為GET. 20160111 modify For Common Type(1Arm1Job,1Arm2Job通用)
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0002")]
        public bool Filter_FirstGlassCheckNotWaiting(IRobotContext robotConText)
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
                List<Port> portList = ObjectManager.PortManager.GetPorts();
                Cassette cst = null;
                for (int i = 0; i < portList.Count; i++)
                {
                    cst = ObjectManager.CassetteManager.GetCassette(portList[i].File.CassetteID);
                    if (cst != null) 
                    {
                        if (cst.FirstGlassCheckReport == "C2")
                        {
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("{0} is Waiting for Mes First Glass Check Result.", cst.CassetteID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("{0} is Waiting for Mes First Glass Check Result.", cst.CassetteID);

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Cassette_WaitingforFirstGlassCheck);
                            robotConText.SetReturnMessage(errMsg);
                            errCode = eJobFilter_ReturnCode.NG_Cassette_WaitingforFirstGlassCheck;//add for BMS Error Monitor
                            if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                            return false;
                        }
                    }
                    
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

        /// <summary> Check 是否有 Cst 处于等待 First Glass Check reply Added by Zhangwei 20161107
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20161107 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0059")]
        public bool Filter_CurStepActionByJobLocation_For1Arm1Job(IRobotContext robotConText)
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

                 #region [ Check Robot Arm Type ] 20160111 modify For Common Type

                 //if (curRobot.Data.ARMJOBQTY != 1)
                 //{

                 //    #region[DebugLog]

                 //    if (IsShowDetialLog == true)
                 //    {
                 //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                 //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                 //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                 //    }

                 //    #endregion

                 //    errMsg = string.Format("[{0}] Robot({1}) Arm Job Qty({2}) is illegal!",
                 //                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                 //    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                 //    robotConText.SetReturnMessage(errMsg);

                 //    return false;

                 //}

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

                     errMsg = string.Format("[{0}]can not Get defineNormalRobotCmd!",
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

                     errMsg = string.Format("[{0}]can not Get 2nd defineNormalRobotCmd!",
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
                                 strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) ({3}) 1st defineNormalRobotCmd Action({4}) is out of Range!(Command Action Error)",
                                                         curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString());

                                 Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                             }

                             #endregion

                             errMsg = string.Format("[{0}] can not Get 1st defineNormalRobotCmd Action({1}) is out of Range!(Command Action Error)",
                                                     MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString());

                             robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
                             robotConText.SetReturnMessage(errMsg);

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

                         errMsg = string.Format("[{0}]Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) Action({6}) is illegal!(Job is on Arm,only action PUT/PUTReady/Exchange)",
                                                 MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
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
                                                                     curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                     tmpStageID, tmpStepNo.ToString(), tmpStepAction);

                             Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                         }

                         #endregion

                         errMsg = string.Format("[{0}][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) Action({8}) is illegal!(Job is not on Arm,only action GET/GETReady)",
                                                 MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
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

        /// <summary>Check Job curStep Setting UseArm is can use by Job Location. for Normal Arm(1Arm1Job). Not On Arm: Use Arm must Empty. On Arm:Only:Use Arm must Exist.
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0003")]
        public bool Filter_CurStepUseArmByJobLocation_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;
            string curBcsJobEqpFlag_TurnAngle = string.Empty;

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
                //add qiumin 20170829 ,取得eqpflag为后面选择arm 用
                IDictionary<string, string> curBcsJobEqpFlag = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG, eJOBDATA.EQPFlag);
                //0:Turn Angle=0,1:Turn Angle=90,2:Turn Angle=180,3:Turn Angle=270
                if(curRobot .Data .LINEID=="TCATS400")
                {
                    curBcsJobEqpFlag_TurnAngle = curBcsJobEqpFlag["TurnAngle"];
                }

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

                #region [ Check UseArm by is2ndCmdFlag ]
               
                //DB定義 //'UP':Upper Arm, 'LOW':Lower Arm, 'ANY':Any Arm, 'ALL':All Arm
                int tmpStepNo =0 ;             
                string tmpStageID = string.Empty;
                int tmpLocation_SlotNo = 0;
                //20151208 modify by RealTime ArmInfo
                //RobotArmSignalSubstrateInfo[] tmpRobotArmInfo = new RobotArmSignalSubstrateInfo[curRobot.File.ArmSignalSubstrateInfoList.Length];
                RobotArmSignalSubstrateInfo[] tmpRobotArmInfo = new RobotArmSignalSubstrateInfo[curRobot.CurTempArmSingleJobInfoList.Length];

                string funcName = string.Empty;

                int curStepNo = 0;

                #region [ Step Check Parameter by is2ndCmdFlag ]

                for (int i = 0; i < tmpRobotArmInfo.Length; i++)
                {
                    tmpRobotArmInfo[i] = new RobotArmSignalSubstrateInfo();
                    //與Robot同步 //20151208 modify by RealTime ArmInfo
                    //tmpRobotArmInfo[i].ArmJobExist = curRobot.File.ArmSignalSubstrateInfoList[i].ArmJobExist;
                    //tmpRobotArmInfo[i].ArmDisableFlag = curRobot.File.ArmSignalSubstrateInfoList[i].ArmDisableFlag;
                    tmpRobotArmInfo[i].ArmJobExist = curRobot.CurTempArmSingleJobInfoList[i].ArmJobExist;
                    tmpRobotArmInfo[i].ArmDisableFlag = curRobot.CurTempArmSingleJobInfoList[i].ArmDisableFlag;
                }

                if (is2ndCmdFlag == false)
                {
                    //Cur StepNo
                    tmpStepNo = curBcsJob.RobotWIP.CurStepNo;
                    tmpStageID = curBcsJob.RobotWIP.CurLocation_StageID;
                    funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                    tmpLocation_SlotNo = curBcsJob.RobotWIP.CurLocation_SlotNo;
                }
                else
                {
                    //Next StepNo
                    //20151014 Modity NextStep由WIP來取得
                    tmpStepNo = curBcsJob.RobotWIP.NextStepNo; // curBcsJob.RobotWIP.CurStepNo + 1;
                    funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;
                    curStepNo = curBcsJob.RobotWIP.CurStepNo;
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

                            #region [ by 1st Cmd Use Arm UpDate ArmInfo and tmpLocation_SlotNo ]

                            //SPEC定義[ Wait_Proc_00028 ] 1Arm 2Job要額外處理
                            //0: None               //1: Upper/Left Arm    //2: Lower/Left Arm   //3: Left Both Arm 
                            //4: Upper/Right Arm    //8: Lower/Right Arm   //12: Right Both Arm
                            switch (cur1stRobotCmd.Cmd01_ArmSelect)
                            {
                                case 1: //Up Arm Unload

                                    tmpRobotArmInfo[0].ArmJobExist = eGlassExist.NoExist;
                                    break;

                                case 2: //Low Arm Unload

                                    tmpRobotArmInfo[1].ArmJobExist = eGlassExist.NoExist;
                                    break;

                                case 3: //Up and Low Arm Unload

                                    tmpRobotArmInfo[0].ArmJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[1].ArmJobExist = eGlassExist.NoExist;
                                    break;

                                default:

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) 1st defineNormalRobotCmd Action({3}) but UseArm({4}) is out of Range!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] can not Get 1st defineNormalRobotCmd Action({1}) but UseArm({2}) is out of Range!",
                                                            MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

                                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
                                    robotConText.SetReturnMessage(errMsg);
                                    errCode = eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail;//add for BMS Error Monitor
                                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                    return false;

                            }

                            #endregion

                            tmpLocation_SlotNo = cur1stRobotCmd.Cmd01_TargetSlotNo;

                            break;

                        case 2:  //Get
                        case 8:  //Put Ready
                        case 16: //Get Ready

                            //Local Stage is Stage
                            tmpStageID = eRobotCommonConst.ROBOT_HOME_STAGEID;

                            #region [ by 1st Cmd Use Arm UpDate ArmInfo ]

                            //SPEC定義[ Wait_Proc_00028 ] 1Arm 2Job要額外處理
                            //0: None               //1: Upper/Left Arm    //2: Lower/Left Arm   //3: Left Both Arm 
                            //4: Upper/Right Arm    //8: Lower/Right Arm   //12: Right Both Arm
                            switch (cur1stRobotCmd.Cmd01_ArmSelect)
                            {
                                case 1: //Up Arm load

                                    tmpRobotArmInfo[0].ArmJobExist = eGlassExist.Exist;
                                    tmpLocation_SlotNo = 1; //Arm#1

                                    break;

                                case 2: //Low Arm load

                                    tmpRobotArmInfo[1].ArmJobExist = eGlassExist.Exist;
                                    tmpLocation_SlotNo = 2; //Arm#2

                                    break;

                                case 3: //Up and Low Arm load

                                    tmpRobotArmInfo[0].ArmJobExist = eGlassExist.Exist;
                                    tmpRobotArmInfo[1].ArmJobExist = eGlassExist.Exist;
                                    tmpLocation_SlotNo = 1; //Arm#1 [ Wait_Proc_00033] Filter_CurStepUseArmByJobLocation_For1Arm1Job 當1st Cmd 是Both Get時 如何確認Job Location 位置?

                                    break;

                                default:

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) 1st defineNormalRobotCmd Action({3}) but UseArm({4}) is out of Range!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] can not Get 1st defineNormalRobotCmd Action({1}) but UseArm({2}) is out of Range!",
                                                            MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

                                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
                                    robotConText.SetReturnMessage(errMsg);
                                    errCode = eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail;//add for BMS Error Monitor
                                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                    return false;

                            }

                            #endregion

                            break;

                        default:

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) 1st defineNormalRobotCmd Action({3}) is out of Range!",
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

                #endregion

                string tmpStepUseArm = curBcsJob.RobotWIP.RobotRouteStepList[tmpStepNo].Data.ROBOTUSEARM.ToString().Trim();
                string tmpStepAction = curBcsJob.RobotWIP.RobotRouteStepList[tmpStepNo].Data.ROBOTACTION.ToString().Trim();

               

                //定義最後選擇的Arm資訊
                string curAfterCheckUseArm = string.Empty;

                #region [ by curStep Location Check Use Arm ]

                //Spec對應
                //0: None               //2: Lower/Left Arm  //4: Upper/Right Arm
                //1: Upper/Left Arm     //3: Left Both Arm   //8: Lower/Right Arm
                //12: Right Both Arm
                if (tmpStageID != eRobotCommonConst.ROBOT_HOME_STAGEID)
                {

                    #region [ Job Loaction Not On Arm. Not On Arm:Only:Use Arm must Empty ]

                    switch (tmpStepUseArm)
                    {
                        case eDBRobotUseArmCode.UPPER_ARM:

                            #region [ StageJob Route Use Up Arm But Up Arm Job Exist ]

                            //UpArm = ArmIndex = 0 .如果Job Exist or Unknown 20151016 add Arm要Enable
                            if (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Exist ||
                                tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Unknown ||
                                tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Disable)
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) setting Action({8}) UseArm({9}) but Robot Arm01 Glass ({10}), ({11})!",
                                                                            curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                            tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) setting Action({6}) UseArm({7}) but Robot Arm01 Glass ArmJobExist：({8}), ArmDisableFlag：({9})!",
                                                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                        tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString());

                                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist);
                                robotConText.SetReturnMessage(errMsg);
                                errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist;//add for BMS Error Monitor
                                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                return false;

                            }
                            else
                            {
                                curAfterCheckUseArm = eDBRobotUseArmCode.UPPER_ARM;
                            }

                            #endregion

                            break;

                        case eDBRobotUseArmCode.LOWER_ARM:

                            #region [ StageJob Route Use Low Arm But Low Arm Job Exist ]

                            //LowArm = ArmIndex = 1.如果Job Exist or Unknown 20151016 add Arm要Enable
                            if (tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Exist ||
                                tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Unknown ||
                                tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Disable)
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) setting Action({8}) UseArm({9}) but Robot Arm02 Glass ({10}), ({11})!",
                                                            curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                            tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) setting Action({6}) UseArm({7}) but Robot Arm02 Glass ArmJobExist：({8}), ArmDisableFlag：({9})!",
                                                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                                        tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist);
                                robotConText.SetReturnMessage(errMsg);
                                errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist;//add for BMS Error Monitor
                                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                return false;

                            }
                            else
                            {
                                curAfterCheckUseArm = eDBRobotUseArmCode.LOWER_ARM;
                            }

                            #endregion

                            break;

                        case eDBRobotUseArmCode.ANY_ARM:

                            #region [ StageJob Route Use Any Arm But Up and Low Arm Job Exist ] 20151016 mark 改寫法

                            ////UpArm = ArmIndex = 0, LowArm = ArmIndex = 1 , 找到其中一個為空即可
                            //if ((tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Exist &&
                            //    tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Exist) ||
                            //    (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Unknown &&
                            //    tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Unknown)||
                            //    (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Exist &&
                            //    tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Unknown)||
                            //    (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Unknown &&
                            //    tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Exist))
                            //{
                            //    //Stage Job but UpArm and LowArm is Empty
                            //    #region[DebugLog]

                            //    if (IsShowDetialLog == true)
                            //    {
                            //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) but Robot Arm01 Glass ({9}) and Arm02 Glass ({10})!",
                            //                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //                                tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(),
                            //                                tmpRobotArmInfo[1].ArmJobExist.ToString());

                            //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            //    }

                            //    #endregion

                            //    errMsg = string.Format("[{0}][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) but Robot Arm01 Glass ({9}) and Arm02 Glass ({10})!",
                            //                            MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //                            tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(),
                            //                            tmpRobotArmInfo[1].ArmJobExist.ToString());

                            //    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist);
                            //    robotConText.SetReturnMessage(errMsg);

                            //    return false;

                            //}
                            //else if (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.NoExist)
                            //{
                            //    //Stage Job Check UpArm is Empty
                            //    curAfterCheckUseArm = eDBRobotUseArmCode.UPPER_ARM;

                            //}
                            //else if (tmpRobotArmInfo[1].ArmJobExist == eGlassExist.NoExist)
                            //{
                            //    //Stage Job Check LowArm is Empty
                            //    curAfterCheckUseArm = eDBRobotUseArmCode.LOWER_ARM;
                            //}

                            #endregion

                            #region [ 20141015 Modify StageJob Route Use Any Arm But Up and Low Arm Job Exist ]

                            //UpArm = ArmIndex = 0, LowArm = ArmIndex = 1 , 找到其中一個為空即可
                            //先判斷Arm01是否可收(沒片且有Enable)
                            if (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.NoExist && tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                            {
                                if (curRobot.Data.LINEID != "TCATS400")
                                {
                                    //Stage Job Check UpArm is Empty
                                    curAfterCheckUseArm = eDBRobotUseArmCode.UPPER_ARM;
                                }   //0 和180才可以用上arm
                                else if (curBcsJobEqpFlag_TurnAngle == "2" || curBcsJobEqpFlag_TurnAngle == "0") 
                                {
                                     curAfterCheckUseArm = eDBRobotUseArmCode.UPPER_ARM;
                                }
                                else if (curBcsJobEqpFlag_TurnAngle == "1" || curBcsJobEqpFlag_TurnAngle == "3")
                                {
                                    curAfterCheckUseArm = eDBRobotUseArmCode.LOWER_ARM;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            //再判斷Arm02是否可收(沒片且有Enable)
                            else if (tmpRobotArmInfo[1].ArmJobExist == eGlassExist.NoExist && tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                            {
                                if (curRobot.Data.LINEID != "TCATS400")
                                {
                                    //Stage Job Check LowArm is Empty
                                    curAfterCheckUseArm = eDBRobotUseArmCode.LOWER_ARM;
                                }   //90 和270才可以用下arm
                                else if (curBcsJobEqpFlag_TurnAngle == "1" || curBcsJobEqpFlag_TurnAngle == "3") 
                                {
                                    curAfterCheckUseArm = eDBRobotUseArmCode.LOWER_ARM;
                                }
                            }
                            else
                            {
                                //Stage Job but UpArm and LowArm is Empty
                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) setting Action({8}) UseArm({9}) but Robot Arm01 Glass ({10}), ({11}) and Arm02 Glass ({12}), ({13})!",
                                                            curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                            tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) setting Action({6}) UseArm({7}) but Robot Arm01 Glass ArmJobExist：({8}), ArmDisableFlag：({9}) and Arm02 Glass ArmJobExist：({10}), ArmDisableFlag：({11})!",
                                                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                        tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                        tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist);
                                robotConText.SetReturnMessage(errMsg);
                                errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist;//add for BMS Error Monitor
                                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                return false;

                            }

                            #endregion

                            break;

                        case eDBRobotUseArmCode.ALL_ARM:

                            #region [ StageJob Route Use All Arm But Up or Low Arm Job Exist ]

                            //UpArm = ArmIndex = 0, LowArm = ArmIndex = 1   20151016 add Arm要Enable
                            if (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Exist ||
                                tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Exist ||
                                tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Unknown ||
                                tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Unknown ||
                                tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Disable ||
                                tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Disable)
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) setting Action({8}) UseArm({9}) but Robot Arm01 Glass ({10}), ({11}) and Arm02 Glass ({12}), ({13})!",
                                                            curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                            tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm,
                                                            tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) setting Action({6}) UseArm({7}) but Robot Arm01 Glass ArmJobExist：({8}), ArmDisableFlag：({9}) and Arm02 Glass ArmJobExist：({10}), ArmDisableFlag：({11})!",
                                                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                        tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm,
                                                        tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                        tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist);
                                robotConText.SetReturnMessage(errMsg);
                                errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist;//add for BMS Error Monitor
                                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                return false;

                            }
                            else
                            {
                                curAfterCheckUseArm = eDBRobotUseArmCode.ALL_ARM;
                            }

                            #endregion

                            break;

                        default:

                            #region [ DB Setting Illegal ]

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) setting Action({8}) UseArm({9}) is illegal!",
                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                        tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) setting Action({6}) UseArm({7}) is illegal!",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                    tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm);

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_Setting_Fail);
                            robotConText.SetReturnMessage(errMsg);
                            errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_Setting_Fail;//add for BMS Error Monitor
                            if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                            return false;

                            #endregion

                    }

                    #endregion

                }
                else
                {
                    
                    #region [ Job Location on Arm. Must Check Use Arm(Job LocationSlot has Job)

                    switch (tmpStepUseArm)
                    {
                        case eDBRobotUseArmCode.UPPER_ARM:

                            #region [ StageJob Route Use Up Arm But Up Arm Job Exist ]

                            //UpArm = ArmIndex = 0 .如果Job NoExist or Unknown  20151016 add Arm要Enable
                            if (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.NoExist || 
                                tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Unknown ||
                                tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Disable)
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) setting Action({8}) UseArm({9}) but Robot Arm01 Glass ({10}), ({11})!",
                                                            curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                            tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) setting Action({6}) UseArm({7}) but Robot Arm01 Glass ArmJobExist：({8}), ArmDisableFlag：({9})!",
                                                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                        tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString());

                                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist);
                                robotConText.SetReturnMessage(errMsg);
                                errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;//add for BMS Error Monitor
                                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                return false;

                            }
                            else
                            {
                                curAfterCheckUseArm = eDBRobotUseArmCode.UPPER_ARM;
                            }

                            #endregion

                            break;

                        case eDBRobotUseArmCode.LOWER_ARM:

                            #region [ StageJob Route Use Low Arm But Low Arm Job Exist ]

                            //LowArm = ArmIndex = 1 .如果Job NoExist or Unknown
                            if (tmpRobotArmInfo[1].ArmJobExist == eGlassExist.NoExist ||
                                tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Unknown ||
                                tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Disable)
                            {
                                //2016/04/07 解決 ELA 特殊 Jump的UseArm問題
                                //如果CurStep UseArm是Any,原本預設是找Upper,現在會對照NextStep UseArm,如果是Lower,則把Upper改為Lower
                                string curStepUseArm = curBcsJob.RobotWIP.RobotRouteStepList[curStepNo].Data.ROBOTUSEARM.ToString().Trim();
                                if (cur1stRobotCmd.Cmd01_ArmSelect == eRobot_ArmSelect.UPPER && curStepUseArm == eDBRobotUseArmCode.ANY_ARM)
                                {
                                    curAfterCheckUseArm = eDBRobotUseArmCode.LOWER_ARM;
                                    cur1stRobotCmd.Cmd01_ArmSelect = GetRobotUseArmCode(curRobot, curBcsJob, curAfterCheckUseArm);
                                    robotConText.AddParameter(eRobotContextParameter.Define_1stNormalRobotCommandInfo, cur1stRobotCmd);

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) CurStepNo({6}) form Any Arm(Upper) to UseArm({7}),because NextStepNo({8}) Use Lower Arm!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                curStepNo.ToString(), tmpStepUseArm, tmpStepNo.ToString());

                                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion
                                }
                                else
                                {
                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) setting Action({8}) UseArm({9}) but Robot Arm02 Glass ({10}), ({11})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) setting Action({6}) UseArm({7}) but Robot Arm02 Glass ArmJobExist：({8}), ArmDisableFlag：({9})!",
                                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                            tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist);
                                    robotConText.SetReturnMessage(errMsg);
                                    errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;//add for BMS Error Monitor
                                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                    return false;
                                }

                            }
                            else
                            {
                                curAfterCheckUseArm = eDBRobotUseArmCode.LOWER_ARM;
                            }

                            #endregion

                            break;

                        case eDBRobotUseArmCode.ANY_ARM:

                            #region [ StageJob Route Use Any Arm But Up and Low Arm Job Exist ] 20151016 mark

                            ////UpArm = ArmIndex = 0, LowArm = ArmIndex = 1. ArmJob Check 對應的Arm要有片. 因為下一Step也會用到此Function所以 CurLocation會有問題.也是要用tmp來處理!
                            //if ((tmpRobotArmInfo[0].ArmJobExist == eGlassExist.NoExist &&
                            //    tmpRobotArmInfo[1].ArmJobExist == eGlassExist.NoExist) ||
                            //    (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Unknown &&
                            //    tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Unknown) ||
                            //    (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.NoExist &&
                            //    tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Unknown) ||
                            //    (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Unknown &&
                            //    tmpRobotArmInfo[1].ArmJobExist == eGlassExist.NoExist) ||
                            //    (tmpRobotArmInfo[0].ArmJobExist != eGlassExist.Exist &&
                            //    tmpLocation_SlotNo == 1) ||
                            //    (tmpRobotArmInfo[1].ArmJobExist != eGlassExist.Exist &&
                            //    tmpLocation_SlotNo == 2))
                            //{

                            //    #region[DebugLog]

                            //    if (IsShowDetialLog == true)
                            //    {
                            //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) but Robot Arm01 Glass ({9}) and Arm02 Glass ({10})!",
                            //                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //                                tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(),
                            //                                tmpRobotArmInfo[1].ArmJobExist.ToString());

                            //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            //    }

                            //    #endregion

                            //    errMsg = string.Format("[{0}][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) but Robot Arm01 Glass ({9}) and Arm02 Glass ({10})!",
                            //                            MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //                            tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(),
                            //                            tmpRobotArmInfo[1].ArmJobExist.ToString());

                            //    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist);
                            //    robotConText.SetReturnMessage(errMsg);

                            //    return false;

                            //}
                            //else if (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Exist &&
                            //    tmpLocation_SlotNo == 1)
                            //{
                            //    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                            //    curAfterCheckUseArm = eDBRobotUseArmCode.UPPER_ARM;

                            //}
                            //else if (tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Exist &&
                            //    tmpLocation_SlotNo == 2)
                            //{
                            //    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                            //    curAfterCheckUseArm = eDBRobotUseArmCode.LOWER_ARM;
                            //}

                            #endregion

                            #region [ 20151016 Modify add Arm Disable Falg. StageJob Route Use Any Arm But Up and Low Arm Job Exist ]

                            if (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Exist &&
                                tmpLocation_SlotNo == 1 && tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                            {
                                if (curRobot.Data.LINEID != "TCATS400")
                                {
                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.UPPER_ARM;
                                }   //0 和180才可以用上arm
                                else if (curBcsJobEqpFlag_TurnAngle == "2" || curBcsJobEqpFlag_TurnAngle == "0")
                                {
                                    curAfterCheckUseArm = eDBRobotUseArmCode.UPPER_ARM;
                                }
                                else 
                                {

                                    return false;
                                }
                                
                            }
                            else if (tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Exist &&
                                tmpLocation_SlotNo == 2 && tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                            {
                                if (curRobot.Data.LINEID != "TCATS400")
                                {
                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.LOWER_ARM;
                                }   //90 和270才可以用下arm
                                else if (curBcsJobEqpFlag_TurnAngle == "1" || curBcsJobEqpFlag_TurnAngle == "3")
                                {
                                    curAfterCheckUseArm = eDBRobotUseArmCode.LOWER_ARM;
                                }
                                else
                                {

                                    return false;
                                }
                            }
                            else
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) setting Action({8}) UseArm({9}) but Robot Arm01 Glass ({10}), ({11}) and Arm02 Glass ({12}), ({13})!",
                                                            curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                            tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) setting Action({6}) UseArm({7}) but Robot Arm01 Glass ArmJobExist：({8}), ArmDisableFlag：({9}) and Arm02 Glass ArmJobExist：({10}), ArmDisableFlag：({11})!",
                                                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                        tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm, tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                        tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist);
                                robotConText.SetReturnMessage(errMsg);
                                errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;//add for BMS Error Monitor
                                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                return false;
                            }

                            #endregion

                            break;

                        case eDBRobotUseArmCode.ALL_ARM:

                            #region [ StageJob Route Use ALL Arm But Up and Low Arm Job NoExist ] 20151016 mark改變寫法

                            //UpArm = ArmIndex = 0, LowArm = ArmIndex = 1 . Both All Arm只要不是都為空即可 20151016 add Arm Enable
                            if ((tmpRobotArmInfo[0].ArmJobExist == eGlassExist.NoExist && tmpRobotArmInfo[1].ArmJobExist == eGlassExist.NoExist) ||
                                (tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Disable && tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Disable)||
                                (tmpRobotArmInfo[0].ArmJobExist == eGlassExist.Exist && tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Disable) ||
                                (tmpRobotArmInfo[1].ArmJobExist == eGlassExist.Exist && tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Disable))
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) setting Action({8}) UseArm({9}) but Robot Arm01 Glass ({10}), ({11}) and Arm02 Glass ({12}), ({13})!",
                                                            curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                            tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm,
                                                            tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(), 
                                                            tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) setting Action({6}) UseArm({7}) but Robot Arm01 Glass ArmJobExist：({8}), ArmDisableFlag：({9})  and Arm02 Glass ArmJobExist：({10}), ArmDisableFlag：({11})!",
                                                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                        tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm,
                                                        tmpRobotArmInfo[0].ArmJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                        tmpRobotArmInfo[1].ArmJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString());

                              //  robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist);
                                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist);  //modify by yang 2017/2/19
                                robotConText.SetReturnMessage(errMsg);
                                errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;//add for BMS Error Monitor
                                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                return false;

                            }
                            else
                            {
                                curAfterCheckUseArm = eDBRobotUseArmCode.ALL_ARM;
                            }


                            #endregion

                            break;

                        default:

                            #region [ DB Setting Illegal ]

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) StageID({6}) StepNo({7}) setting Action({8}) UseArm({9}) is illegal!",
                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                        tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StageID({4}) StepNo({5}) setting Action({6}) UseArm({7}) is illegal!",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                    tmpStageID, tmpStepNo.ToString(), tmpStepAction, tmpStepUseArm);

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_Setting_Fail);
                            robotConText.SetReturnMessage(errMsg);
                            errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_Setting_Fail;//add for BMS Error Monitor
                            if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                            return false;

                            #endregion

                    }

                    #endregion

                }

                #endregion

                #region [ Set Robot UseArm To DefindNormalRobotCommand by is2ndCmdFlag ]

                if (is2ndCmdFlag == false)
                {
                    cur1stRobotCmd.Cmd01_ArmSelect = GetRobotUseArmCode(curRobot, curBcsJob, curAfterCheckUseArm);
                    robotConText.AddParameter(eRobotContextParameter.Define_1stNormalRobotCommandInfo, cur1stRobotCmd);
                }
                else
                {
                    cur2ndRobotCmd.Cmd01_ArmSelect = GetRobotUseArmCode(curRobot, curBcsJob, curAfterCheckUseArm);
                    robotConText.AddParameter(eRobotContextParameter.Define_2ndNormalRobotCommandInfo, cur2ndRobotCmd);
                }

                #endregion

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

        /// <summary> Check Next Step Filter Condition and Set 2nd Cmd.因為會根據1stCmd來做處理.所以Priority要最小最後處理 20160111 modify for Common(1Arm1Job, 1Arm2Job)
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0007")]
        public bool Filter_NextStepAllFilterCodition_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curLDRQStageList = new List<RobotStage>();
            string errCode = string.Empty;

            try
            {

                #region UDRQ Job Forecast Check
                bool _check = false;
                if (robotConText.ContainsKey(eRobotContextParameter.UDRQ_JOB_FORECAST_CHECK)) _check = (bool)robotConText[eRobotContextParameter.UDRQ_JOB_FORECAST_CHECK];
                if(_check) //为真, 代表是要做 forecast check ! 所以, 不需要检查下面的逻辑, 直接离开!
                {
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                    return true;
                }
                #endregion

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

                #region [ Check Robot Arm Type ] 20160111 modify for Common

                //if (curRobot.Data.ARMJOBQTY != 1)
                //{

                //    #region[DebugLog]

                //    if (IsShowDetialLog == true)
                //    {
                //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //    }

                //    #endregion

                //    errMsg = string.Format("[{0}] Robot({1}) Arm Job Qty({2}) is illegal!", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                //    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                //    robotConText.SetReturnMessage(errMsg);

                //    return false;

                //}

                #endregion

                #region [ Get Current NextStep Entity ]

                //20151014 Modity NextStep由WIP來取得
                int nextStepNo = curBcsJob.RobotWIP.NextStepNo;// curBcsJob.RobotWIP.CurStepNo + 1;

                RobotRouteStep curRouteNextStep;
                if (!curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(nextStepNo))
                    curRouteNextStep = null;
                else
                    curRouteNextStep = curBcsJob.RobotWIP.RobotRouteStepList[nextStepNo];

                //找不到 CurStep Route 回NG
                if (curRouteNextStep == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                nextStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get JobcurRouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            nextStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
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

                    errMsg = string.Format("[{0}] can not Get 2nd defineNormalRobotCmd!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_2nd_DefineNormalRobotCommandInfo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ ****** Get Current LDRQ Stage List ] 20151005 mark .此時要Check的是NextStep ,不需要從RobotContext取得(此時RobotContet是CurStep的不是NextStep的)

                //curLDRQStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                //if (curLDRQStageList == null)
                //{

                //    #region[DebugLog]

                //    if (IsShowDetialLog == true)
                //    {
                //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find Stage Status is LDRQ!",
                //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                //                                curBcsJob.RobotWIP.CurLocation_StageID, nextStepNo.ToString(), curRouteNextStep.Data.STAGEIDLIST);

                //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //    }

                //    #endregion

                //    errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find Stage Status is LDRQ!",
                //                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                //                                                curBcsJob.RobotWIP.CurLocation_StageID, nextStepNo.ToString(), curRouteNextStep.Data.STAGEIDLIST);

                //    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                //    robotConText.SetReturnMessage(errMsg);

                //    return false;

                //}

                #endregion

                #region [ 20151014 add Check CurStep All RouteStepByPass Condition and 準備變更curStep ]

                //注意 此時curLDRQStageList是curStep不是NextStep ,所以應該不需要取值.
                //if (!CheckAllRouteStepByPassCondition_For2ndCommand(curRobot, curBcsJob, nextStepNo, ref curLDRQStageList))
                if (!CheckAllRouteStepByPassCondition2(curRobot, curBcsJob, nextStepNo, ref curLDRQStageList, true)) //for 2nd cmd
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) Check NextStepNo({5}) RouteStepByPassCondition Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                nextStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) Check NextStepNo({4}) RouteStepByPassCondition Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                            nextStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Chek_NextStep_RouteStepByPassCondition_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //mark by yang 2017/3/13  这个filter先不check
                 //   errCode = eJobFilter_ReturnCode.NG_Chek_NextStep_RouteStepByPassCondition_Fail;//add for BMS Error Monitor
                 //   if (!curRobot.CheckErrorList.ContainsKey(errCode))
                 //       curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    //RouteStepByPass條件有問題則回覆NG
                    return false;

                }

                #endregion

                #region [ 20151017 add Check CurStep All RouteStepJump Condition and 準備變更curStep ]

                //if (!CheckAllRouteStepJumpCondition_For2ndCommand(curRobot, curBcsJob, nextStepNo, ref curLDRQStageList))
                if (!CheckAllRouteStepJumpCondition2(curRobot, curBcsJob, nextStepNo, ref curLDRQStageList, true)) //for 2nd cmd
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) Check NextStepNo({5}) RouteStepJumpCondition Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                nextStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) Check NextStepNo({4}) RouteStepJumpCondition Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                            nextStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Chek_NextStep_RouteStepJumpCondition_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    //mark by yang 2017/3/13
                //    errCode = eJobFilter_ReturnCode.NG_Chek_NextStep_RouteStepJumpCondition_Fail;//add for BMS Error Monitor
                //    if (!curRobot.CheckErrorList.ContainsKey(errCode))
              //          curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    //RouteStepJump條件有問題則回覆NG
                    return false;
                }

                #endregion

                #region [ add Check CurStep All RouteStepFilter Condition and 準備變更curStep ]
                //if (!CheckAllFilterConditionByStepNo_For2ndCommand(curRobot, curBcsJob, nextStepNo, cur1stRobotCmd, cur2ndRobotCmd, ref curLDRQStageList))
                if (!CheckAllFilterConditionByStepNo2(curRobot, curBcsJob, nextStepNo, cur1stRobotCmd, cur2ndRobotCmd, ref curLDRQStageList, true))
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) Check NextStepNo({5}) FilterCondition Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                nextStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) Check NextStepNo({4}) FilterCondition Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                            nextStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Chek_NextStep_FilterCondition_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    //mark by yang 2017/3/13
              //      errCode = eJobFilter_ReturnCode.NG_Chek_NextStep_FilterCondition_Fail;//add for BMS Error Monitor
              //      if (!curRobot.CheckErrorList.ContainsKey(errCode))
              //          curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }
                #endregion

                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                //20150831
                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curLDRQStageList);

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

        /// <summary> Check Job Location StageID Is Arm . Has 2nd Command Check Function
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0005")]
        public bool Filter_JobOnRobotArmByJobLocation(IRobotContext robotConText)
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ by 2nd Command Check Flag define Robot Location StageID ]

                string curStageID = string.Empty;
                string funcName = string.Empty;

                if (is2ndCmdFlag == false)
                {
                    curStageID = curBcsJob.RobotWIP.CurLocation_StageID;
                    funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                }
                else
                {
                    funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;

                    #region [ Get Defind 1st Normal Robot Command ]

                    DefineNormalRobotCmd cur1stRobotCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.Define_1stNormalRobotCommandInfo];

                    //找不到 defineNormalRobotCmd 回NG
                    if (cur1stRobotCmd == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get 1st defineNormalRobotCmd!",
                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not Get 1st defineNormalRobotCmd!",
                                                MethodBase.GetCurrentMethod().Name);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }

                    #endregion

                    #region [ by 1st Cmd Define Job Location ]

                    //SPEC定義[ Wait_Proc_00028 ] 1Arm 2Job要額外處理
                    //0: None      //1: Put          //2: Get
                    //4: Exchange  //8: Put Ready    //16: Get Ready       //32: Get/Put
                    switch (cur1stRobotCmd.Cmd01_Command)
                    {
                        case 1:  //PUT
                        case 4:  //Exchange
                        case 32: //Get/Put

                            //Local Stage is Stage
                            curStageID = cur1stRobotCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0');
                            break;

                        case 2:  //PUT
                        case 8:  //Exchange
                        case 16: //Get/Put

                            //Local Stage is Stage
                            curStageID = eRobotCommonConst.ROBOT_HOME_STAGEID;
                            break;

                        default:

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) 1st defineNormalRobotCmd Action({3}) is out of Range!",
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

                #endregion

                if (curStageID != eRobotCommonConst.ROBOT_HOME_STAGEID)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) curStepNo({6}) curStageID({7}) is not Robot Arm({8})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curStageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) curStageID({5}) is not Robot Arm({6})!!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                            curStageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Location_IsNot_Robot);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Job_Location_IsNot_Robot;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

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

        /// <summary>Check Job curStep Setting TrackingData is can use by Job Location. Not On Arm: Check by Job EQP SendOut realTime TrackingData. On Arm:Only:Check by Job Keep TrackingData
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0004")]
        public bool Filter_CurStepTrackingDataByJobLoction_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            bool checkFlag = false;
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

                //Watson Add 20151217 For TTP Dailycheck 是會重工多次的機台，不需要判斷Tracking Data
                if ((Workbench.LineType == eLineType.ARRAY.TTP_VTEC || Workbench.LineType == eLineType.CF.FCMQC_TYPE1) &&
                    Check_TTP_EQInterlock_DailyCheckBit(curRobot) == eBitResult.ON)  ///TTP Line EQP Flag Daily Check Bit is 'ON' 就是需要重工，所以不需要判斷
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) DailyCheck Bit is 'ON' ,Tracking data({6}) will re-work!!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.TrackingData);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }

                string tmpStageID = string.Empty;
                string funcName = string.Empty;
                RobotRouteStep curCheckRouteStep = null;

                #region [ 20151107 Modify 如果是2nd Cmd Check則要根據1st Cmd來重新定義 Job Location ]

                //20151107 add Check Arm Type  //20160204 Mark 1Arm2Job 也是By Job來做Filter
                #region [ Check Robot Arm Type ]

                //if (curRobot.Data.ARMJOBQTY != 1)
                //{

                //    #region[DebugLog]

                //    if (IsShowDetialLog == true)
                //    {
                //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //    }

                //    #endregion

                //    errMsg = string.Format("[{0}] Robot({1}) Arm Job Qty({2}) is illegal!",
                //                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                //    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
                //    robotConText.SetReturnMessage(errMsg);

                //    return false;

                //}

                #endregion

                //20151107 add Get 2ndCmdFlag
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

                //20151107 add Get 1st CmdInfo
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
     

                #region [ Get Check Step Entity by 2nd Cmd ]
                if (is2ndCmdFlag == false)
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
                    funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                }
                else
                {
                    curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
                    funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;
                }

                //找不到 CurCheckStep Route 回NG
                if (curCheckRouteStep == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curCheckRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get JobcurCheckRouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

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

                #endregion

                #endregion


                RobotStage _nxtStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(tmpStageID);
                if (_nxtStage != null)
                {
                    if (_nxtStage.Data.RTCREWORKFLAG.ToString().ToUpper() == "Y") //要去的机台, 如果允许 RTC 重工的话, 这时收到 RTC 基板, 则不检查 tracking data, 直接回OK!
                    {
                        #region 如果有做过RTC出来的, 就不需要检查!!

                        if (curBcsJob.RobotWIP.RTCReworkFlag) //RTC基板, 不卡tracking data, 直接回OK!
                        {
                            robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                            robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                            return true;
                        }
                        #endregion
                    }
                }


                #region [ Decode Job Keep RealTimeTrackingData(EQP Type Stage:EQP Real Time SendOut JobData, Other(Arm,Port,inside,buffer Type): Job目前WIP內的TrackingData) ]

                //Job目前在EQP Stage上
                string curRealTimeTrackData = string.Empty;

                //20151107 Modify:如果是2nd Cmd則要根據1st Cmd改變位置後來判斷              
                if (tmpStageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                {
                    //Location 在Arm上則以Job Keep為主
                    curRealTimeTrackData = curBcsJob.TrackingData;
                }
                else
                {
                    #region [ Get Check StageID Entity ]

                    RobotStage curCheckStageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(tmpStageID);

                    //找不到 Robot Stage 回NG
                    if (curCheckStageEntity == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, tmpStageID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not Get RobotStageInfo by StageID({1})!",
                                                    MethodBase.GetCurrentMethod().Name, tmpStageID);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_Stage_Is_Null);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }


                    #endregion

                    //if (curBcsJob.RobotWIP.CurLocation_StageType == eRobotStageType.EQUIPMENT)
                    if (curCheckStageEntity.Data.STAGETYPE == eRobotStageType.EQUIPMENT)
                    {

                        curRealTimeTrackData = curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData;

                    }
                    else
                    {
                        curRealTimeTrackData = curBcsJob.TrackingData;

                    }

                }

                IDictionary<string, string> dicJobKeepTrackingData = ObjectManager.SubJobDataManager.Decode(curRealTimeTrackData, eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

                if (dicJobKeepTrackingData == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) can not Decode TrackingData Info by Job Keep InputTrackingData({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                curBcsJob.TrackingData);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not Decode TrackingData Info by Job Keep InputTrackingData({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.TrackingData);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region [ Decode Job Keep LastInPutTrackingData(Last InPut Stage時代表進入這個Stage的TrackingData) ]

                IDictionary<string, string> dicJobKeepLastInPutTrackingData = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.LastInPutTrackingData, eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

                if (dicJobKeepLastInPutTrackingData == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) can not Decode TrackingData Info by Job Keep InputTrackingData({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                curBcsJob.TrackingData);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not Decode TrackingData Info by Job Keep InputTrackingData({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                             curBcsJob.TrackingData);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region [ By Job Location Check TrackingData ]

                //20151107 Modify:如果是2nd Cmd則要根據1st Cmd改變位置後來判斷
                //if (curBcsJob.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                if (tmpStageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                {

                    #region [ Job On Arm , Check Job Keep TrackingData by Job RouteStep Setting InputTrackingData ]

                    #region [ Decode DB Setting InputTrackingData ]

                    IDictionary<string, string> dicDBInputTrackingData = ObjectManager.SubJobDataManager.Decode(curCheckRouteStep.Data.INPUTTRACKDATA.Trim(), eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

                    if (dicDBInputTrackingData == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) can not Decode TrackingData Info by DB Setting RouteStepNo({5}) InputTrackingData({6})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString(), curCheckRouteStep.Data.INPUTTRACKDATA.Trim());
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }
                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) can not Decode TrackingData Info by DB Setting RouteStepNo({4}) InputTrackingData({5})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString(), curCheckRouteStep.Data.INPUTTRACKDATA.Trim());

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        errCode = eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                        return false;

                    }

                    #endregion

                    #region [ Check Job Keep TrackingData by Job RouteStep Setting InputTrackingData. 在Arm時要根據DB設定來判斷Job Keep Info 不可以On 以免重工 ]

                    if (curCheckRouteStep.Data.ROBOTRULE == eRobotRouteStepRule.SELECT)
                    {

                        #region [ Step RuleType是Select的處理. 只要一個符合條件就算OK ]

                        checkFlag = false;

                        //目前長度主要分為1, 2, Oher Reserved >2.Decode之後已經是數值: EX:"11" =>"3"
                        foreach (string itemKey in dicDBInputTrackingData.Keys)
                        {
                            if (dicDBInputTrackingData[itemKey] == eRobotCommonConst.DB_SETTING_TRACKINGDATE_LEN1_ON)
                            {
                                if (dicJobKeepLastInPutTrackingData[itemKey] == "0" && dicJobKeepTrackingData[itemKey] == "0")
                                {
                                    //DB設定要On,但是WIP與EQP SendOut要同時都沒有紀錄表示不會重工 for Lenth=1
                                    checkFlag = true;
                                    break;
                                }

                            }
                            else if (dicDBInputTrackingData[itemKey] == eRobotCommonConst.DB_SETTING_TRACKINGDATE_LEN2_ON)
                            {
                                if (dicJobKeepLastInPutTrackingData[itemKey] == "0" && dicJobKeepTrackingData[itemKey] == "0")
                                {
                                    //DB設定要On,但是WIP與EQP SendOut同時都沒有紀錄表示不會重工 for Lenth=2
                                    checkFlag = true;
                                    break;
                                }
                            }
                            else
                            {
                                //其他不確認
                                //checkFlag = true;
                            }

                        }

                        #endregion

                    }
                    else
                    {

                        #region [ Step RuleType不是Select的處理. 不符合條件馬上判斷NG ]

                        //20151119 add 預設為True . 如果出現不符合狀態(重工)才回False
                        checkFlag = true;

                        //目前長度主要分為1, 2, Oher Reserved >2.Decode之後已經是數值: EX:"11" =>"3"
                        foreach (string itemKey in dicDBInputTrackingData.Keys)
                        {
                            if (dicDBInputTrackingData[itemKey] == eRobotCommonConst.DB_SETTING_TRACKINGDATE_LEN1_ON)
                            {
                                if (dicJobKeepLastInPutTrackingData[itemKey] != "0" || dicJobKeepTrackingData[itemKey] != "0")
                                {
                                    //DB設定要On,但是WIP與EQP SendOut其中之一有紀錄表示有問題會重工 for Lenth=1
                                    checkFlag = false;
                                    break;
                                }

                            }
                            else if (dicDBInputTrackingData[itemKey] == eRobotCommonConst.DB_SETTING_TRACKINGDATE_LEN2_ON)
                            {
                                if (dicJobKeepLastInPutTrackingData[itemKey] != "0" || dicJobKeepTrackingData[itemKey] != "0")
                                {
                                    //DB設定要On,但是WIP與EQP SendOut其中之一有紀錄表示有問題會重工 for Lenth=2
                                    checkFlag = false;
                                    break;
                                }
                            }
                            else
                            {
                                //其他長度不確認視同Pass
                                checkFlag = true;
                            }

                        }

                        #endregion
                    }

                    #endregion

                    if (checkFlag == false)
                    {
                        //20151119 add 多紀錄目前取到的cur1stRobotCmd.Cmd01_Command以便判斷

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curCheckStageID({5}) curCheckStepNo({6}) Last InputTrackingData({7}), curRealTimeTrackingData({8}) but DB Setting InputackingData({9}) will rework! cur1stCommand=({10}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                    tmpStageID, curCheckRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.LastInPutTrackingData, curRealTimeTrackData,
                                                    curCheckRouteStep.Data.INPUTTRACKDATA.Trim(), cur1stRobotCmd.Cmd01_Command.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3})  curCheckStageID({4}) curCheckStepNo({5}) Last InputTrackingData({6}), curRealTimeTrackingData({7}) but DB Setting InputackingData({8}) will rework! cur1stCommand=({9}).",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                tmpStageID, curCheckRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.LastInPutTrackingData, curRealTimeTrackData,
                                                curCheckRouteStep.Data.INPUTTRACKDATA.Trim(), cur1stRobotCmd.Cmd01_Command.ToString());

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_CheckTrackingData_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        errCode = eJobFilter_ReturnCode.NG_ArmJob_CheckTrackingData_Fail;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                        return false;

                    }

                    #endregion

                }
                else
                {

                    #region [ Job Not On Arm , Check Job Keep TrackingData by Job RouteStep Setting OutputTrackingData ]

                    #region [ Decode DB Setting OutputTrackingData ]

                    IDictionary<string, string> dicDBOutputTrackingData = ObjectManager.SubJobDataManager.Decode(curCheckRouteStep.Data.OUTPUTTRACKDATA.Trim(), eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

                    if (dicDBOutputTrackingData == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) can not Decode TrackingData Info by DB Setting RouteStepNo({5}) InputTrackingData({6})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString(), curCheckRouteStep.Data.INPUTTRACKDATA.Trim());
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }
                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) can not Decode TrackingData Info by DB Setting RouteStepNo({4}) InputTrackingData({5})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                    curBcsJob.RobotWIP.CurStepNo.ToString(), curCheckRouteStep.Data.INPUTTRACKDATA.Trim());

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        errCode = eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                        return false;

                    }

                    #endregion

                    #region [ 20151107 modify Check Job Keep TrackingData by Job RouteStep Setting OutputTrackingData ]

                    if (curCheckRouteStep.Data.ROBOTRULE == eRobotRouteStepRule.SELECT)
                    {
                        #region [ Step RuleType是Select的處理. 只要一個符合條件就算OK ]

                        checkFlag = false;

                        //目前長度主要分為1, 2, Oher Reserved >2 .Decode之後已經是數值: EX:"11" =>"3"
                        foreach (string itemKey in dicDBOutputTrackingData.Keys)
                        {
                            if (dicDBOutputTrackingData[itemKey] == eRobotCommonConst.DB_SETTING_TRACKINGDATE_LEN1_ON)
                            {
                                if (dicJobKeepLastInPutTrackingData[itemKey] != "0" || dicJobKeepTrackingData[itemKey] != "0")
                                {
                                    //DB設定要On,且WIP與EQP SendOut任一有紀錄表示確認OK for Lenth=1
                                    checkFlag = true;
                                    break;
                                }
                            }
                            else if (dicDBOutputTrackingData[itemKey] == eRobotCommonConst.DB_SETTING_TRACKINGDATE_LEN2_ON)
                            {
                                if (dicJobKeepLastInPutTrackingData[itemKey] != "0" || dicJobKeepTrackingData[itemKey] != "0")
                                {
                                    //DB設定要On,但是WIP與EQP  SendOut任一有紀錄表示確認OK for Lenth=2
                                    checkFlag = true;
                                    break;
                                }
                            }
                            else
                            {
                                //其他不確認
                                //checkFlag = true;
                            }

                        }

                        #endregion

                    }
                    else
                    {

                        #region [ Step RuleType不是Select的處理. 不符合條件馬上判斷NG ]

                        //20151119 add 預設為True . 如果出現不符合狀態(沒有On TrackingData表示與Route不符跳流程了)才回False
                        checkFlag = true;

                        //目前長度主要分為1, 2, Oher Reserved >2 .Decode之後已經是數值: EX:"11" =>"3"
                        foreach (string itemKey in dicDBOutputTrackingData.Keys)
                        {
                            if (dicDBOutputTrackingData[itemKey] == eRobotCommonConst.DB_SETTING_TRACKINGDATE_LEN1_ON)
                            {
                                if (dicJobKeepLastInPutTrackingData[itemKey] == "0" && dicJobKeepTrackingData[itemKey] == "0")
                                {
                                    //DB設定要On,但是WIP與EQP SendOut都沒有紀錄表示有問題 for Lenth=1
                                    checkFlag = false;
                                    break;
                                }
                            }
                            else if (dicDBOutputTrackingData[itemKey] == eRobotCommonConst.DB_SETTING_TRACKINGDATE_LEN2_ON)
                            {
                                if (dicJobKeepLastInPutTrackingData[itemKey] == "0" && dicJobKeepTrackingData[itemKey] == "0")
                                {
                                    //DB設定要On,但是WIP與EQP SendOut都沒有紀錄表示有問題 for Lenth=2
                                    checkFlag = false;
                                    break;
                                }
                            }
                            else
                            {
                                //其他長度不確認視同Pass
                                checkFlag = true;
                            }

                        }

                        #endregion

                    }

                    #endregion

                    if (checkFlag == false)
                    {
                        //20151118 add 多紀錄目前取到的cur1stRobotCmd.Cmd01_Command以便判斷

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curCheckStageID({5}) curCheckStepNo({6}) Last InputTrackingData({7}), curRealTimeTrackingData({8}) but DB Setting OutputackingData({9}) is mismatch! cur1stCommand=({10}).",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                    tmpStageID, curCheckRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.LastInPutTrackingData, curRealTimeTrackData,
                                                                    curCheckRouteStep.Data.OUTPUTTRACKDATA.Trim(), cur1stRobotCmd.Cmd01_Command.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion


                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curCheckStageID({4}) curCheckStepNo({5}) Last InputTrackingData({6}), curRealTimeTrackingData({7}) but DB Setting OutputackingData({8}) is mismatch!cur1stCommand=({9}).",
                                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                    tmpStageID, curCheckRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.LastInPutTrackingData, curRealTimeTrackData,
                                                                    curCheckRouteStep.Data.OUTPUTTRACKDATA.Trim(), cur1stRobotCmd.Cmd01_Command.ToString());

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_CheckTrackingData_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        errCode = eJobFilter_ReturnCode.NG_StageJob_CheckTrackingData_Fail;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                        return false;

                    }

                    #endregion

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

        /// <summary> Check CurStep Setting StageID List Status is LDRQ And return LDRQ StageList, No Support Exchange
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0006")]
        public bool Filter_CurStepStageIDListLDRQ(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curFilterCanUseStageList = null;
            bool _isPutReady = false;
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

                #region [ Get Current Step Entity ]

                int tmpStepNo = 0;

                if (is2ndCmdFlag == true)
                {
                    //20151014 Modity NextStep由WIP來取得
                    tmpStepNo = curBcsJob.RobotWIP.NextStepNo;// curBcsJob.RobotWIP.CurStepNo + 1;
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
                    //mark by yang 2017/3/13
                 //   errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                 //   if (!curRobot.CheckErrorList.ContainsKey(errCode))
                 //      curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region [ Check Current Step CurStageStatus ]
                List<RobotStage> _tmpCanGetPutStageList = new List<RobotStage>();
                _tmpCanGetPutStageList.Clear();

                #region[CF MQC200 ,by port connect EQ]
                if (curRobot.Data.LINEID == "FCQMA200")//add by qiumin 20171102 for CF MQC200 ,by port connect EQ
                {
                    if (curBcsJob.MesCstBody.PORTNAME == "03" || curBcsJob.MesCstBody.PORTNAME == "04")
                    {
                        //curRouteStep.Data.STAGEIDLIST = "11,12";
                        curRouteStep.Data.STAGEIDLIST = "11";
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Port({3}) connect MAC({4}) !",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.MesCstBody.PORTNAME, curRouteStep.Data.STAGEIDLIST);

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                    }
                    if (curBcsJob.MesCstBody.PORTNAME == "01" || curBcsJob.MesCstBody.PORTNAME == "02")
                    {
                        //curRouteStep.Data.STAGEIDLIST = "13";
                        curRouteStep.Data.STAGEIDLIST = "12,13";
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Port({3}) connect MAC({4}) !",
                                                  curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.MesCstBody.PORTNAME, curRouteStep.Data.STAGEIDLIST);

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                    }
                }
                #endregion

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
                                                    curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curStepUseStage.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        continue;
                    }

                    if (curStepUseStage.File.CurStageStatus == eRobotStageStatus.RECEIVE_READY)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) check StepNo({5}) StageID({6}) Status({7}) is Success!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curStepUseStage.Data.STAGEID, curStepUseStage.File.CurStageStatus);

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //Check LDRQ
                        if (!curFilterCanUseStageList.Contains(curStepUseStage)) curFilterCanUseStageList.Add(curStepUseStage);
                    }
                    else if (curStepUseStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                    {
                        //Check Both Port
                        #region [ 如果是both Port要確認是否為Source Port,如果是Stage則 ]

                        if (curStepUseStage.Data.STAGETYPE == eRobotStageType.PORT)
                        {

                            #region [ Get Port Entity by StageID ]

                            Port curPort = ObjectManager.PortManager.GetPort(curStepUseStage.Data.STAGEID);

                            if (curPort == null)
                            {

                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not get Port Entity!",
                                                                            curStepUseStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStepUseStage.Data.STAGEID, curStepUseStage.Data.STAGENAME);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                continue;
                            }

                            #endregion

                            #region [ Check Job From Port CSTID與CstSeq ]

                            if (curBcsJob.FromCstID.Trim() == curPort.File.CassetteID.Trim() &&
                               curBcsJob.CassetteSequenceNo == curPort.File.CassetteSequenceNo)
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) from CSTID({5}) check StepNo({6}) StageID({7}) Status({8}) and Port({9}) CSTID({10}) CSTSeq({11}) is Success!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.FromCstID.Trim(), tmpStepNo.ToString(), curStepUseStage.Data.STAGEID,
                                                            curStepUseStage.File.CurStageStatus, curPort.Data.PORTID, curPort.File.CassetteID.Trim(), curPort.File.CassetteSequenceNo);

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                if (curFilterCanUseStageList.Contains(curStepUseStage) == false)
                                {

                                    curFilterCanUseStageList.Add(curStepUseStage);

                                }
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

                                //20160603 for Array IndexOperMode = ABNORMAL_FORCE_CLEAN_OUT_MODE 要放到Unloading port,both port卡掉
                                if (robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.ABNORMAL_FORCE_CLEAN_OUT_MODE && robotLine.Data.FABTYPE == eFabType.ARRAY.ToString())
                                {
                                    if (curFilterCanUseStageList.Contains(curStepUseStage) == true)
                                    {
                                        #region  [DebugLog]

                                        if (IsShowDetialLog == true)
                                        {
                                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) IndexOperMode = ({2}),can't put to Both Port({3})!",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, robotLine.File.IndexOperMode, curStepUseStage.Data.STAGEID);
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion
                                        curFilterCanUseStageList.Remove(curStepUseStage);

                                    }
                                }

                            }
                            else
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) From CSTID({4}), curStageID({5}) StageStatus({6}) but Port CSTID({7}) CSTSeq({8}) is mismatch!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curBcsJob.FromCstID, curBcsJob.RobotWIP.CurLocation_StageID, curStepUseStage.File.CurStageStatus, curPort.File.CassetteID,
                                                            curPort.File.CassetteSequenceNo);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                //20151029 add 如果check Stage為Both Port,但是與目前Job From CSTID不同.則要排除
                                //從Filter Can Use Stage中移除
                                if (curFilterCanUseStageList.Contains(curStepUseStage) == true)
                                {

                                    curFilterCanUseStageList.Remove(curStepUseStage);

                                }

                            }

                            #endregion

                        }
                        else
                        {

                            #region [ 20151209 Multi-Slot stage會出現LDRQ & UDRQ狀態 ]

                            if (curStepUseStage.Data.ISMULTISLOT == "Y")
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) CheckStepID({4}) setting StageID({5}) StageType({6}) StageStatus({7}) is Success!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curRouteStep.Data.STEPID.ToString(), curStepUseStage.Data.STAGEID, curStepUseStage.Data.STAGETYPE, curStepUseStage.File.CurStageStatus);

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                if (curFilterCanUseStageList.Contains(curStepUseStage) == false)
                                {

                                    curFilterCanUseStageList.Add(curStepUseStage);

                                }

                            }
                            else
                            {

                                //非Port不可以為同時UDRQ_LDRQ ==>20151209 mark
                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) check StepNo({5}) StageID({6}) Status({7}) is not LDRQ!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curStepUseStage.Data.STAGEID, curStepUseStage.File.CurStageStatus);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                //從Filter Can Use Stage中移除
                                if (curFilterCanUseStageList.Contains(curStepUseStage) == true)
                                {

                                    curFilterCanUseStageList.Remove(curStepUseStage);

                                }

                            }

                            #endregion

                        }

                        #endregion

                    }
                    ////20160106 add for 新需求 MAC RecipeID最後一碼(第12碼)為"1"不可以Exchange
                    else if (curStepUseStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY && curStepUseStage.File.DownStreamExchangeReqFlag 
                                && curStepUseStage.MacCanNotExchangeFlag ==false)
                    //else if (curStepUseStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY && (curStepUseStage.File.DownStreamExchangeReqFlag || curStepUseStage.File.DownStreamReceiveJobReserveSignal)) //GlobalAssemblyVersion v1.0.0.26-20151027
                    {
                        if (!CheckStageUDRQJobCondition_For1Arm1Job(robotConText, curStepUseStage, ref curFilterCanUseStageList)) 
                        { if (curFilterCanUseStageList.Contains(curStepUseStage)) curFilterCanUseStageList.Remove(curStepUseStage); }//20160704 jack modify 失败Remove掉即可

                        //Check UDRQ
                        #region [ EQP Type Stage UDRQ 且要求Exchange ]
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) check StepNo({5}) StageID({6}) Status({7}) Exchange Request(On) is Success!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curStepUseStage.Data.STAGEID, curStepUseStage.File.CurStageStatus);

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //if (!curFilterCanUseStageList.Contains(curStepUseStage)) curFilterCanUseStageList.Add(curStepUseStage); //20160705因為上面改成Remove,所以Add需拿掉,不然curFilterCanUseStageList又加回來,等於沒卡到Filter
                        #endregion
                    }
                    else if (curStepUseStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY && curStepUseStage.Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_GETPUT) //GlobalAssemblyVersion v1.0.0.26-20151029
                    {
                        ////虽然是发出片需求(UDRQ)并且设定是GETPUT, 但是可不可以做, 需要看实际EQP那边是不是有没有ON Receive Job Reserve signal!
                        //if (curStepUseStage.File.DownStreamReceiveJobReserveSignal)
                        //{
                        //    //已经确定可以做GETPUT, 就需要来判断要出来的基板的下一步是否OK! NG则不允许继续!!
                        //    if (!CheckStageUDRQJobCondition_For1Arm1Job(robotConText, curRobot, curStepUseStage, ref curFilterCanUseStageList)) return false;

                        //    #region[DebugLog]
                        //    if (IsShowDetialLog == true)
                        //    {
                        //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) check StepNo({5}) StageID({6}) Status({7}) Get/Put available!",
                        //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        //                                curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curStepUseStage.Data.STAGEID, curStepUseStage.File.CurStageStatus);

                        //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        //    }
                        //    #endregion
                        //    if (!curFilterCanUseStageList.Contains(curStepUseStage)) curFilterCanUseStageList.Add(curStepUseStage); 
                        //}
                        //else
                        //{
                        //    //由于不确定EQP那边可不可以做GETPUT, 所以先移除!! 下次再确认!!
                        //    if (curFilterCanUseStageList.Contains(curStepUseStage)) curFilterCanUseStageList.Remove(curStepUseStage);
                        //    if (!_tmpCanGetPutStageList.Contains(curStepUseStage)) _tmpCanGetPutStageList.Add(curStepUseStage);
                        //}

                        //curRobot.Context.AddParameter(eRobotContextParameter.CanUsePreFetchFlag, curStepUseStage.Data.PREFETCHFLAG.ToString().ToUpper()); //如果為N, 就是沒有開啟Pre-Fetch功能!

                        if (!CheckStageUDRQJobCondition_For1Arm1Job(robotConText, curStepUseStage, ref curFilterCanUseStageList))
                        {
                            //由于确定无法做, 所以先移除! 先判断其他Stage
                            if (curFilterCanUseStageList.Contains(curStepUseStage)) curFilterCanUseStageList.Remove(curStepUseStage);
                        }
                    }
                    else
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) check StepNo({5}) StageID({6}) Status({7}) is not LDRQ!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curStepUseStage.Data.STAGEID, curStepUseStage.File.CurStageStatus);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //不符合條件則要從CurUsertageList移除
                        if (curFilterCanUseStageList.Contains(curStepUseStage)) curFilterCanUseStageList.Remove(curStepUseStage);

                        if (curStepUseStage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y") _isPutReady = true;
                    }
                }

                //if (curFilterCanUseStageList.Count() <= 0 && _tmpCanGetPutStageList.Count() > 0)
                //{
                //    //其他Stage没有可以做LDRQ/EXCHANGE, 所以还是要判断可以出片Stage (UDRQ), 该基板的下一步是否OK!?
                //    foreach (RobotStage curStepUseStage in _tmpCanGetPutStageList)
                //    {
                //        if (CheckStageUDRQJobCondition_For1Arm1Job(robotConText, curRobot, curStepUseStage, ref curFilterCanUseStageList))
                //        {
                //            #region[DebugLog]
                //            if (IsShowDetialLog == true)
                //            {
                //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) check StepNo({5}) StageID({6}) Status({7}) Get/Put available!",
                //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                //                                        curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curStepUseStage.Data.STAGEID, curStepUseStage.File.CurStageStatus);

                //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //            }
                //            #endregion
                //            if (!curFilterCanUseStageList.Contains(curStepUseStage)) curFilterCanUseStageList.Add(curStepUseStage); 
                //        }
                //    }
                //}

                #endregion

                #region [ Get Current Route Info ]
                RobotRoute _route = ObjectManager.RobotManager.GetRoute(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID);

                if (_route == null)
                {
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Job Route({3})!",
                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_Route_Fail);
                    robotConText.SetReturnMessage(errMsg);
                 //   errCode = eJobFilter_ReturnCode.NG_Job_Get_Route_Fail;//add for BMS Error Monitor
                 //   if (!curRobot.CheckErrorList.ContainsKey(errCode))
                //        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                #endregion

                #region [ 先将不在该route的EQP Stage先排除, 不需要考虑! 因为目前的route不会去该EQP!! ]
                //for (int i = curFilterCanUseStageList.Count; i >= 0;i--)
                //{
                //    if()



                //}
                #endregion

                #region FCMQC_TYPE1 DailyCheck             
                if (curRouteStep.Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT)
                {
                    //Watson Add 20151203 For TTP Dailycheck 是會重工多次的機台，不需要判斷Tracking Data
                    if ((Workbench.LineType == eLineType.ARRAY.TTP_VTEC || Workbench.LineType == eLineType.CF.FCMQC_TYPE1))
                    {
                        if (Check_TTP_EQInterlock_DailyCheckBit(curRobot) == eBitResult.OFF)  ///TTP Line EQP Flag Daily Check Bit is 'ON' 就是需要重工，所以不需要判斷
                        {
                            #region 避免重工
                            // PUT 時, 根據 Job.TrackingData, 過濾已經去過的 Stage 以避免重工
                            // Stage若有多組Tracking Data Bit, 則需每組都有ON才會過濾Stage
                            for (int i = curFilterCanUseStageList.Count - 1; i >= 0; i--)
                            {
                                RobotStage stage = curFilterCanUseStageList[i];
                                if (!string.IsNullOrEmpty(stage.Data.TRACKDATASEQLIST))
                                {
                                    string[] offset_list = stage.Data.TRACKDATASEQLIST.Split(',');
                                    int match = 0;
                                    foreach (string offset in offset_list)
                                    {
                                        int len = ObjectManager.SubJobDataManager.GetSubItemLenth(eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA, offset);
                                        string his_tracking = curBcsJob.TrackingData.Substring(int.Parse(offset), len);
                                        string real_tracking = curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData.Substring(int.Parse(offset), len);
                                        if (his_tracking.Contains("1") || real_tracking.Contains("1"))
                                        {
                                            match++;
                                        }
                                    }
                                    if (match == offset_list.Length)//Stage若有多組Tracking Data Bit, 則需每組都有ON才會過濾Stage
                                    {
                                                                        #region[DebugLog]
                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) check StepNo({5}) StageID({6}) tracking data({7}) re-work, DB TRACKDATASEQLIST({8})",
                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curFilterCanUseStageList[i].Data.STAGEID, curBcsJob.TrackingData, stage.Data.TRACKDATASEQLIST);

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }
                                    #endregion

                                        curFilterCanUseStageList.RemoveAt(i);
                                    }
                                }
                            }
                        #endregion
                        }
                        else
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Daily Check bit  ON , Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStageID({5}) check StepNo({6}) tracking data({7}) re-work",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                        curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curBcsJob.TrackingData);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }  
                    }
                }
                #endregion
                if (curFilterCanUseStageList.Count == 0)
                {
                    //if (!_isPutReady)
                    //{
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStageID({5}) StepNo({6}) StageIDList({7}) can not Find Stage Status is LDRQ!(Please check Downstream EQP is Ready Receiving Job)",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curRouteStep.Data.STAGEIDLIST);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find Stage Status is LDRQ!(Please check Downstream EQP is Ready Receiving Job)",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                    curBcsJob.RobotWIP.CurLocation_StageID, tmpStepNo.ToString(), curRouteStep.Data.STAGEIDLIST);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);
                    //}
                    //mark by yang 2017/3/13
                    //同样片超过1h未出,再加进Error List   //Check Condition
                        errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        {
                            curRobot.WaitGlass = curRobot.WaitGlass.Length == 0 ? curBcsJob.EQPJobID : curRobot.WaitGlass;                     
                            if (curRobot.WaitGlass.Equals(curBcsJob.EQPJobID)) 
                            {
                                curRobot.WaitGlassTimeSpan = curRobot.WaitGlassTimeSpan.Length == 0 ? DateTime.Now.ToString() : curRobot.WaitGlassTimeSpan;
                                TimeSpan ts = DateTime.Now.Subtract(DateTime.Parse(curRobot.WaitGlassTimeSpan));
                                if (ts.TotalHours > 1)  //不做成Config,hardcode 1h
                                {
                                    errMsg = errMsg + "!!NO LDRQ Sustains More Than 1h," + "[" + curRobot.WaitGlassTimeSpan + "]" + "Occurs!!";
                                    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                                    curRobot.WaitGlass = string.Empty;
                                    curRobot.WaitGlassTimeSpan = string.Empty;
                                }
                            }
                            
                        }

                    return false;

                }
                #region OVNITO提前开门逻辑判断
                //add by hujunpeng 20181001
                if ((curRobot.Data.LINEID == "TCOVN400" || curRobot.Data.LINEID == "TCOVN500") && curBcsJob.RobotWIP.CurStepNo == 1) // Deng,20190926,OVNITO 提前开门bug修正
                {
                    lock (curBcsJob)
                    {         
                    if (curFilterCanUseStageList.Count==2)
                    {
                        RobotStage curTargetStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID("11");
                        RobotStage curTargetStage1 = ObjectManager.RobotStageManager.GetRobotStagebyStageID("13");
                        if(curTargetStage.File.InputDateTime<curTargetStage1.File.InputDateTime)
                        {
                        curBcsJob.RobotWIP.OvnOpenTheDoorPriority = 1;
                        }
                        else
                        curBcsJob.RobotWIP.OvnOpenTheDoorPriority = 2;  
                    }
                    if(curFilterCanUseStageList.Count==1)
                    {
                        switch(curFilterCanUseStageList[0].Data.STAGEID)
                        {
                            case "11":
                            curBcsJob.RobotWIP.OvnOpenTheDoorPriority = 1;
                            break;
                            case "13":
                            curBcsJob.RobotWIP.OvnOpenTheDoorPriority = 2;
                            break;
                            default:
                            break;
                        }
                    }
                    }
                    ObjectManager.JobManager.EnqueueSave(curBcsJob);
                }
                #endregion

                //會直接更新所以不需要回傳值
                //robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curLDRQStageList);
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

        /// <summary>Check Changer Plan, 若Job不在ChangerPlan中則過濾, 並且過濾TargetStage只留下ChangerPlan中的TargetCST
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0013")]
        public bool Filter_PortFetchOutChangerPlan_For1Arm1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

            try
            {
                if (!IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    //非Changer Mode則不檢查
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }

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

                Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if (line.File.PlanStatus != ePLAN_STATUS.READY &&
                    line.File.PlanStatus != ePLAN_STATUS.START)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStageID({5}) StepNo({6}) StageIDList({7}) PlanStatus({8}) Plan is not Ready or Start",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST, line.File.PlanStatus);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) StageIDList({6}) PlanStatus({7}) Plan is not Ready or Start",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST, line.File.PlanStatus);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_PlanStatusIsNotReadyOrStart);
                    robotConText.SetReturnMessage(errMsg);

                    //errCode = eJobFilter_ReturnCode.NG_PlanStatusIsNotReadyOrStart;//add for BMS Error Monitor
                    //if (!curRobot.CheckErrorList.ContainsKey(errCode))
                    //    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #region Get Changer Plan
                string plan_id = string.Empty;
                IList<SLOTPLAN> slot_plans = ObjectManager.PlanManager.GetProductPlans(out plan_id);
                if (slot_plans == null || slot_plans.Count <= 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStageID({5}) StepNo({6}) StageIDList({7}) can not get Changer Plan in Changer Mode",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not get Changer Plan in Changer Mode",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CanNotGetAnyChangerPlan);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_CanNotGetAnyChangerPlan;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }
                #endregion

                #region [ Check Job in Changer Plan ]

                SLOTPLAN job_slot_plan = GetSlotPlanByJob(slot_plans, curBcsJob);
                if (job_slot_plan == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) StepNo({5}) not in Changer Plan can not Fetch Out!(please check 1.Job.FromCstID = plan.SOURCE_CASSETTE_ID 2.Offline:plan.SLOTNO > 0 & Job.FromSlotNo = plan.SLOTNO 3.Online:plan.SLOTNO <= 0 & plan.PRODUCT_NAME = Job.MesProduct.PRODUCTNAME)",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StepNo({4}) not in Changer Plan can not Fetch Out!(please check 1.Job.FromCstID = plan.SOURCE_CASSETTE_ID 2.Offline:plan.SLOTNO > 0 & Job.FromSlotNo = plan.SLOTNO 3.Online:plan.SLOTNO <= 0 & plan.PRODUCT_NAME = Job.MesProduct.PRODUCTNAME)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_JobNotInChangerPlan);
                    robotConText.SetReturnMessage(errMsg);

                    //errCode = eJobFilter_ReturnCode.NG_JobNotInChangerPlan;//add for BMS Error Monitor   //marked by yang 
                    //if (!curRobot.CheckErrorList.ContainsKey(errCode))
                    //    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion

                #region [ Check Target Port Stage in Changer Plan ]
                Port target_port = null;
                RobotStage target_stage = null;
                List<RobotStage> curFilterCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                if (curFilterCanUseStageList != null)
                {
                    foreach (RobotStage stage in curFilterCanUseStageList)
                    {
                        if (stage.Data.STAGETYPE != eStageType.PORT)
                            continue;

                        Port port = ObjectManager.PortManager.GetPort(stage.Data.LINEID, stage.Data.NODENO, stage.Data.STAGEIDBYNODE);
                        if (port != null && port.File.CassetteID == job_slot_plan.TARGET_CASSETTE_ID)
                        {
                            target_port = port;
                            target_stage = stage;
                            break;
                        }
                    }
                }
                if (target_port == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStageID({5}) StepNo({6}) StageIDList({7}) can not Find Target Port Stage in Changer Plan(please check port.File.CassetteID = plan.TARGET_CASSETTE_ID)",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find Target Port Stage in Changer Plan(please check port.File.CassetteID = plan.TARGET_CASSETTE_ID)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_TargetStageNotInChangerPlan);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_TargetStageNotInChangerPlan;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

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
     
        /// <summary>Check Job SamplingFlag must is "1" to Fetch Out
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0010")]
        public bool Filter_PortFetchOutSamplingFlag_For1Arm1Job(IRobotContext robotConText)
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

                #region [ Check Job Wip Sampling Flag ]

                if (curBcsJob.SamplingSlotFlag != "1")
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO ({5}) SamplingSlotFlag({6}) can not Fetch Out!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo,
                                                curBcsJob.SamplingSlotFlag);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO ({5}) SamplingSlotFlag({6}) can not Fetch Out!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurRouteID,curBcsJob.RobotWIP.CurStepNo,
                                            curBcsJob.SamplingSlotFlag);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_FetchOutSampingFlag_Is_Fail);
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

        /// <summary>Check Force Clean Out Can not Fetch Out From Port CST
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0011")]
        public bool Filter_PortFetchOutNotForceCleanOut_For1Arm1Job(IRobotContext robotConText)
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

                #region [ Check IndexOperMode Force clean Out Mode Can Not Fetch Out ]

                if (robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) IndexOperMode({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) curStepNO({6}) can not Fetch Out!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, robotLine.File.IndexOperMode.ToString(), curBcsJob.CassetteSequenceNo, 
                                                curBcsJob.JobSequenceNo
                                                , curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo
                                                );

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] IndexOperMode({1}) Job({2}_{3}) curRouteID({4}) curStepNO({5}) can not Fetch Out!",
                                            MethodBase.GetCurrentMethod().Name, robotLine.File.IndexOperMode.ToString(), curBcsJob.CassetteSequenceNo,curBcsJob.JobSequenceNo
                                            , curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_PortFetchOutNotFroceCleanOut_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_PortFetchOutNotFroceCleanOut_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

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




        /// <summary>根據RouteStepByPass條件判斷特定StepNo是否出現變化
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkStepNo"></param>
        /// <param name="curStageSelectInfo"></param>
        /// <param name="curBeforeFilterStageList"></param>
        /// <returns></returns>
        private bool CheckAllRouteStepByPassCondition_For2ndCommand(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList)
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

                            if (!curRobot.CheckErrorList.ContainsKey(fail_ReasonCode))   //add for BMS Error Monitor  [special format ,mark by yang] 
                                curRobot.CheckErrorList.Add(fail_ReasonCode, Tuple.Create(strlog, curBcsJob.EQPJobID, "0", "ROBOT"));

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

        /// <summary>根據RouteStepJump條件判斷特定StepNo是否要改為GOTOStepNo
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkStepNo"></param>
        /// <param name="curBeforeFilterStageList"></param>
        /// <returns></returns>
        private bool CheckAllRouteStepJumpCondition_For2ndCommand(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList)
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

                            if (!curRobot.CheckErrorList.ContainsKey(fail_ReasonCode))   //add for BMS Error Monitor  [special format ,mark by yang] 
                                curRobot.CheckErrorList.Add(fail_ReasonCode, Tuple.Create(strlog, curBcsJob.EQPJobID, "0", "ROBOT"));
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

        /// <summary> Check Next StepNo 所有Filter條件是否成立.並取得2nd Cmd
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkNextStepNo"></param>
        /// <param name="cur1stDefineCmd"></param>
        /// <param name="cur2ndDefineCmd"></param>
        /// <returns></returns>
        private bool CheckAllFilterConditionByStepNo_For2ndCommand(Robot curRobot, Job curBcsJob, int checkNextStepNo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefineCmd, ref List<RobotStage> curFilterStageList)
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

                #region [ Initial Filter Rule List RobotConText Info. 搭配針對File Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] ==========

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
                #region Get Pre-Fetch
                bool _canUsePreFetchFlag = false;
                if (curRobot.Context.ContainsKey(eRobotContextParameter.CanUsePreFetchFlag))
                {
                    _canUsePreFetchFlag = (curRobot.Context[eRobotContextParameter.CanUsePreFetchFlag].ToString() == "Y" ? true : false);
                }
                #endregion

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

                                if (!curRobot.CheckErrorList.ContainsKey(fail_ReasonCode))   //add for BMS Error Monitor  [special format ,mark by yang] 
                                    curRobot.CheckErrorList.Add(fail_ReasonCode, Tuple.Create(strlog, curBcsJob.EQPJobID, "0", "ROBOT"));

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

        //20160218 add for Array Changer Sampling Mode
        /// <summary>Check Array ChangerLine SamplingMode ChangePlan, 若Job不在ChangerPlan中則過濾, 並且過濾TargetStage只留下ChangerPlan中的TargetCST
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0047")]
        public bool Filter_PortFetchOutChangerPlan_ForArrayChangerSamplingMode(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

            try
            {
                if (!IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.SAMPLING_MODE))
                {
                    //非Changer Mode則不檢查
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }

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

                Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if (line.File.PlanStatus != ePLAN_STATUS.READY &&
                    line.File.PlanStatus != ePLAN_STATUS.START)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStageID({5}) StepNo({6}) StageIDList({7}) PlanStatus({8}) Plan is not Ready or Start",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST, line.File.PlanStatus);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) StageIDList({6}) PlanStatus({7}) Plan is not Ready or Start",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST, line.File.PlanStatus);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_JobOrTargetStageNotInChangerPlan);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_JobOrTargetStageNotInChangerPlan;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }

                #region Get Changer Plan
                string plan_id = string.Empty;
                IList<SLOTPLAN> slot_plans = ObjectManager.PlanManager.GetProductPlans(out plan_id);
                if (slot_plans == null || slot_plans.Count <= 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStageID({5}) curStageID({6}) StepNo({7}) StageIDList({8}) can not get Changer Plan in Changer Mode",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not get Changer Plan in Changer Mode",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_JobOrTargetStageNotInChangerPlan);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_JobOrTargetStageNotInChangerPlan;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }
                #endregion

                #region [ Check Job in Changer Plan ]

                SLOTPLAN job_slot_plan = GetSlotPlanByJob(slot_plans, curBcsJob);
                if (job_slot_plan == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) StepNo({5}) not in Changer Plan can not Fetch Out!(please check 1.Job.FromCstID = plan.SOURCE_CASSETTE_ID 2.Offline:plan.SLOTNO > 0 & Job.FromSlotNo = plan.SLOTNO 3.Online:plan.SLOTNO <= 0 & plan.PRODUCT_NAME = Job.MesProduct.PRODUCTNAME)",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) StepNo({4}) not in Changer Plan can not Fetch Out!(please check 1.Job.FromCstID = plan.SOURCE_CASSETTE_ID 2.Offline:plan.SLOTNO > 0 & Job.FromSlotNo = plan.SLOTNO 3.Online:plan.SLOTNO <= 0 & plan.PRODUCT_NAME = Job.MesProduct.PRODUCTNAME)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_JobOrTargetStageNotInChangerPlan);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_JobOrTargetStageNotInChangerPlan;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion

                #region [ Check Target Port Stage in Changer Plan ]
                Port target_port = null;
                RobotStage target_stage = null;
                List<RobotStage> curFilterCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                if (curFilterCanUseStageList != null)
                {
                    foreach (RobotStage stage in curFilterCanUseStageList)
                    {
                        if (stage.Data.STAGETYPE != eStageType.PORT)
                            continue;

                        Port port = ObjectManager.PortManager.GetPort(stage.Data.LINEID, stage.Data.NODENO, stage.Data.STAGEIDBYNODE);
                        if (port != null && port.File.CassetteID == job_slot_plan.TARGET_CASSETTE_ID)
                        {
                            target_port = port;
                            target_stage = stage;
                            break;
                        }
                    }
                }
                if (target_port == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStageID({5}) StepNo({6}) StageIDList({7}) can not Find Target Port Stage in Changer Plan(please check port.File.CassetteID = plan.TARGET_CASSETTE_ID)",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find Target Port Stage in Changer Plan(please check port.File.CassetteID = plan.TARGET_CASSETTE_ID)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_JobOrTargetStageNotInChangerPlan);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_JobOrTargetStageNotInChangerPlan;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

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




        //For Non-Use Function List -=======================================================================================================================================================================================================================

        private bool CheckAllStageSelectCondition_For2ndCommand(Robot curRobot, Job curBcsJob, int checkNextStepNo, ref JobStageSelectInfo curStageSelectInfo, ref List<RobotStage> curBeforeFilterStageList)
        {
            IRobotContext robotConText = new RobotContext();
            string fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;
            List<RobotStage> curCanUseStageList = new List<RobotStage>();

            try
            {
                List<RobotRuleStageSelect> curStageSelectList = ObjectManager.RobotManager.GetRuleStageSelect(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, checkNextStepNo);
                if (curStageSelectList == null)
                    curStageSelectList = new List<RobotRuleStageSelect>();

                #region[DebugLog][ Start Job All StageSelect Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job StageSelect ListCount({4}) Start {5}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curStageSelectList.Count.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_STAGESELECT_START_CHAR, eRobotCommonConst.ALL_RULE_STAGESELECT_START_CHAR_LENGTH));

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #region [Check CurStep All StageSelect Condition ]

                #region [ Initial StageSelect Rule List RobotConText Info. 搭配針對StageSelect Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);

                //1st Cmd is2ndCmdFlag is false
                robotConText.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, true);

                #region  [ StageSelect前先預設目前Step都是符合條件的 ]

                string[] curStepCanUseStageList = curBcsJob.RobotWIP.RobotRouteStepList[checkNextStepNo].Data.STAGEIDLIST.Split(',');

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

                #region [ Set StageSelectInfo ]

                //JobStageSelectInfo curStageSelectInfo = new JobStageSelectInfo();

                curStageSelectInfo.CurStepNo = checkNextStepNo;
                curStageSelectInfo.AfterStageSelect_StepNo = checkNextStepNo;
                robotConText.AddParameter(eRobotContextParameter.StageSelectInfo, curStageSelectInfo);

                #endregion

                #endregion =======================================================================================================================================================

                #region [ Check StageSelect Condition Process ]

                foreach (RobotRuleStageSelect curStageSelectCondition in curStageSelectList)
                {
                    //Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_E0004 ] ,以Rule Job StageSelect 的ObjectName與MethodName為Key來決定是否紀錄Log
                    //因為會出現同Job 確認不同Step所以FailCode要補上StepNo
                    fail_ReasonCode = string.Format("{0}_{1}_{2}", curStageSelectCondition.Data.OBJECTNAME, curStageSelectCondition.Data.METHODNAME, checkNextStepNo.ToString());

                    #region[DebugLog][ Start Rule Job StageSelect Function ]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job StageSelect object({4}) MethodName({5}) IsEnable({6}) Start {7}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curStageSelectCondition.Data.OBJECTNAME, curStageSelectCondition.Data.METHODNAME, curStageSelectCondition.Data.ISENABLED,
                                                new string(eRobotCommonConst.RULE_STAGESELECT_START_CHAR, eRobotCommonConst.RULE_STAGESELECT_START_CHAR_LENGTH));

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    if (curStageSelectCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                    {

                        checkFlag = (bool)Invoke(curStageSelectCondition.Data.OBJECTNAME, curStageSelectCondition.Data.METHODNAME, new object[] { robotConText });

                        if (checkFlag == false)
                        {

                            #region[DebugLog][ End Rule Job StageSelect Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job StageSelect Fail, object({4}) MethodName({5}) RtnCode({4})  RtnMsg({6}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curStageSelectCondition.Data.OBJECTNAME, curStageSelectCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job StageSelect object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curStageSelectCondition.Data.OBJECTNAME, curStageSelectCondition.Data.METHODNAME,
                                                        curStageSelectCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_STAGESELECT_END_CHAR, eRobotCommonConst.RULE_STAGESELECT_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0004 ]

                            if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job StageSelect Fail, object({4}) MethodName({5}) RtnCode({6})  RtnMsg({7}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curStageSelectCondition.Data.OBJECTNAME, curStageSelectCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6})!",
                                //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curStageSelectCondition.Data.OBJECTNAME,
                                //                        curStageSelectCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                failMsg = string.Format("Job({1}_{2}) RtnCode({2})  RtnMsg({3})!",
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

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job StageSelect ListCount({4}) End {5}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curStageSelectList.Count.ToString(),
                                                        new string(eRobotCommonConst.ALL_RULE_STAGESELECT_END_CHAR, eRobotCommonConst.ALL_RULE_STAGESELECT_END_CHAR_LENGTH));

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            if (!curRobot.CheckErrorList.ContainsKey(fail_ReasonCode))  //add for BMS Error Monitor  [Special Format,mark by yang]
                                curRobot.CheckErrorList.Add(fail_ReasonCode, Tuple.Create(strlog, curBcsJob.EQPJobID, "0", "ROBOT"));
                            //有重大異常直接結束StageSelect邏輯回復NG
                            return false;

                        }
                        else
                        {

                            //Clear[ Robot_Fail_Case_E0004 ]
                            RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                            #region[DebugLog][ End Rule Job StageSelect Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job StageSelect object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curStageSelectCondition.Data.OBJECTNAME, curStageSelectCondition.Data.METHODNAME,
                                                        curStageSelectCondition.Data.ISENABLED,
                                                        new string(eRobotCommonConst.RULE_STAGESELECT_END_CHAR, eRobotCommonConst.RULE_STAGESELECT_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                        }
                    }
                    else
                    {

                        #region[DebugLog][ End Rule Job STAGESELECT Function ]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job StageSelect object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curStageSelectCondition.Data.OBJECTNAME, curStageSelectCondition.Data.METHODNAME, curStageSelectCondition.Data.ISENABLED,
                                                    new string(eRobotCommonConst.RULE_STAGESELECT_END_CHAR, eRobotCommonConst.RULE_STAGESELECT_END_CHAR_LENGTH));

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                    }

                }

                #endregion

                #endregion

                #region[DebugLog][ Start Job All StageSelect Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 2nd Command Rule Job StageSelect ListCount({4}) End {5}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curStageSelectList.Count.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_STAGESELECT_END_CHAR, eRobotCommonConst.ALL_RULE_STAGESELECT_END_CHAR_LENGTH));

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
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

        /// <summary> 20151107改寫後不啟用
        /// 取Job所在的Stage, 檢查Stage的Tracking Data Bit在Job Tracking Data是否有On
        /// </summary>
        /// <param name="curBcsJob"></param>
        /// <param name="ErrorMessage"></param>
        /// <returns>true:DB Stage設定要On且WIP或EQP SendOut有紀錄; false:DB Stage設定要On,但是WIP與EQP SendOut都沒有紀錄表示有問題</returns>
        private bool CheckJobCurrentStageTrackingData(Job curBcsJob, out string ErrorMessage)
        {
            ErrorMessage = string.Empty;
            bool ret = false;
            try
            {
                RobotStage cur_stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curBcsJob.RobotWIP.CurLocation_StageID);
                if (cur_stage != null)
                {
                    //取Job所在的Stage, 檢查Stage的Tracking Data Bit在Job Tracking Data是否有On
                    string[] db_tracking_datas = cur_stage.Data.TRACKDATASEQLIST.Split(',');

                    string curTrackData = string.Empty;
                    if (curBcsJob.RobotWIP.CurLocation_StageType == eRobotStageType.EQUIPMENT)
                        curTrackData = curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData;
                    else
                        curTrackData = curBcsJob.TrackingData;

                    ret = true;
                    foreach (string db_tracking_data in db_tracking_datas)
                    {
                        if (string.IsNullOrEmpty(db_tracking_data)) continue;
                        int i = int.Parse(db_tracking_data);
                        if (curTrackData[i] == '1' || curTrackData[i + 1] == '1' ||
                            curBcsJob.RobotWIP.LastInPutTrackingData[i] == '1' || curBcsJob.RobotWIP.LastInPutTrackingData[i + 1] == '1')
                        {
                            // DB的Stage TRACKDATASEQLIST有設定要On
                            // 而且 Job的TrackingData或LastInPutTrackingData 有On
                            // do nothing
                        }
                        else
                        {
                            ret = false;
                            ErrorMessage = string.Format("Method[{0}] TRACKDATASEQLIST[{1}] JobTrackingData[{2}] LastInPutTrackingData[{3}]", MethodBase.GetCurrentMethod().Name, cur_stage.Data.TRACKDATASEQLIST, curTrackData, curBcsJob.RobotWIP.LastInPutTrackingData);
                            break;
                        }
                    }
                }
                else
                {
                    ErrorMessage = string.Format("Method[{0}] StageID[{1}] GetRobotStagebyStageID return null", MethodBase.GetCurrentMethod().Name, curBcsJob.RobotWIP.CurLocation_StageID);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Method[{0}] Exception[{1}]", MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return ret;
        }

        //20160603
        [UniAuto.UniBCS.OpiSpec.Help("FL0051")]
        public bool Filter_PortFetchOutNotAbnormalForceCleanOut_For1Arm1Job(IRobotContext robotConText)
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

                #region [ Check IndexOperMode Force clean Out Mode Can Not Fetch Out ]

                if (robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.ABNORMAL_FORCE_CLEAN_OUT_MODE)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) IndexOperMode({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Fetch Out!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, robotLine.File.IndexOperMode.ToString(), curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] IndexOperMode({1}) Job({2}_{3}) is not Abnormal Force Clean Out!",
                                            MethodBase.GetCurrentMethod().Name, robotLine.File.IndexOperMode.ToString(), curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_PortFetchOutNotAbnormalForceCleanOut_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_PortFetchOutNotAbnormalForceCleanOut_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

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

        #region [ 20151107 old Type Backup (Filter_CurStepTrackingDataByJobLoction_For1Arm1Job) ]

        ///// <summary>Check Job curStep Setting TrackingData is can use by Job Location. Not On Arm: Check by Job EQP SendOut realTime TrackingData. On Arm:Only:Check by Job Keep TrackingData
        ///// 
        ///// </summary>
        ///// <param name="robotConText"></param>
        ///// <returns></returns>
        //public bool Filter_CurStepTrackingDataByJobLoction_For1Arm1Job(IRobotContext robotConText)
        //{           
        //    string strlog = string.Empty;
        //    string errMsg = string.Empty;
        //    bool checkFlag = false;

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

        //        #region [ Get curBcsJob Entity ]

        //        Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

        //        //找不到 Job 回NG
        //        if (curBcsJob == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobInfo!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get JobInfo!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get Current Step Entity ]

        //        RobotRouteStep curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

        //        //找不到 CurStep Route 回NG
        //        if (curRouteStep == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.RobotWIP.CurStepNo.ToString());

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get JobcurRouteStep({4})!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurStepNo.ToString());

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Decode Job Keep TrackingData(StageType:Last Stage SendOut JobData Update or EQPType:EQP Real Time SendOut JobData) ]

        //        //Job目前在EQP Stage上
        //        string curTrackData = string.Empty;

        //        if (curBcsJob.RobotWIP.CurLocation_StageType == eRobotStageType.EQUIPMENT)
        //        {

        //            curTrackData = curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData;

        //        }
        //        else
        //        {
        //            curTrackData = curBcsJob.TrackingData;

        //        }

        //        IDictionary<string, string> dicJobKeepTrackingData = ObjectManager.SubJobDataManager.Decode(curTrackData, eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

        //        if (dicJobKeepTrackingData == null)
        //        {

        //            #region  [DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by Job Keep InputTrackingData({4})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.TrackingData);
        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //            }
        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by Job Keep InputTrackingData({4})!",
        //                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                     curBcsJob.TrackingData);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ Decode Job Keep InPutTrackingData(Last InPut Stage時代表進入這個Stage的TrackingData) ]

        //        IDictionary<string, string> dicJobKeepInPutTrackingData = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.LastInPutTrackingData, eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

        //        if (dicJobKeepInPutTrackingData == null)
        //        {

        //            #region  [DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by Job Keep InputTrackingData({4})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.TrackingData);
        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //            }
        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by Job Keep InputTrackingData({4})!",
        //                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                     curBcsJob.TrackingData);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ By Job Location Check TrackingData ]

        //        if (curBcsJob.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
        //        {

        //            #region [ Job On Arm , Check Job Keep TrackingData by Job RouteStep Setting InputTrackingData ]

        //            #region [ Decode DB Setting InputTrackingData ]

        //            IDictionary<string, string> dicDBInputTrackingData = ObjectManager.SubJobDataManager.Decode(curRouteStep.Data.INPUTTRACKDATA.Trim(), eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

        //            if (dicDBInputTrackingData == null)
        //            {

        //                #region  [DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by DB Setting RouteStepNo({4}) InputTrackingData({5})!",
        //                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.INPUTTRACKDATA.Trim());
        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //                }
        //                #endregion

        //                errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by DB Setting RouteStepNo({4}) InputTrackingData({5})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.INPUTTRACKDATA.Trim());

        //                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
        //                robotConText.SetReturnMessage(errMsg);

        //                return false;

        //            }

        //            #endregion

        //            #region [ Check Job Keep TrackingData by Job RouteStep Setting InputTrackingData ]

        //            for (int i = 0; i < dicDBInputTrackingData.Count; i++)
        //            {
        //                //目前長度主要分為1, 2, Oher Reserved >2
        //                foreach (string itemKey in dicDBInputTrackingData.Keys)
        //                {
        //                    if (dicDBInputTrackingData[itemKey] == "1" && 
        //                        dicJobKeepInPutTrackingData[itemKey] =="0" && dicJobKeepTrackingData[itemKey] =="0")
        //                    {
        //                        //DB設定要On,但是WIP與EQP SendOut都沒有紀錄表示有問題
        //                        checkFlag = false;
        //                        break;
        //                    }
        //                    else if (dicDBInputTrackingData[itemKey] == "11" &&
        //                        dicJobKeepInPutTrackingData[itemKey] == "00" && dicJobKeepTrackingData[itemKey] == "00")
        //                    {
        //                        //DB設定要On,但是WIP與EQP SendOut都沒有紀錄表示有問題
        //                        checkFlag = false;
        //                        break;
        //                    }
        //                    else
        //                    {
        //                        //其他長度不確認視同Pass
        //                        checkFlag = true;
        //                    }

        //                }

        //            }

        //            #endregion

        //            if (checkFlag == false)
        //            {

        //                #region[DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) curStepNo({5}) Last InputTrackingData({6}), curTrackingData({7}) but DB Setting InputackingData({8}) is mismatch!",
        //                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                                            curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo, curBcsJob.RobotWIP.LastInPutTrackingData, curTrackData,
        //                                                            curRouteStep.Data.INPUTTRACKDATA.Trim());

        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }

        //                #endregion

        //                errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) curStepNo({5}) Last InputTrackingData({6}), curTrackingData({7}) but DB Setting InputackingData({8}) is mismatch!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                                            curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo, curBcsJob.RobotWIP.LastInPutTrackingData, curTrackData,
        //                                                            curRouteStep.Data.INPUTTRACKDATA.Trim());

        //                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ArmJob_CheckTrackingData_Fail);
        //                robotConText.SetReturnMessage(errMsg);

        //                return false;

        //            }

        //            #endregion

        //        }
        //        else
        //        {

        //            #region [ Job Not On Arm , Check Job Keep TrackingData by Job RouteStep Setting OutputTrackingData ]

        //            #region [ Decode DB Setting OutputTrackingData ]

        //            IDictionary<string, string> dicDBOutputTrackingData = ObjectManager.SubJobDataManager.Decode(curRouteStep.Data.OUTPUTTRACKDATA.Trim(), eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

        //            if (dicDBOutputTrackingData == null)
        //            {

        //                #region  [DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by DB Setting RouteStepNo({4}) InputTrackingData({5})!",
        //                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.INPUTTRACKDATA.Trim());
        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //                }
        //                #endregion

        //                errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by DB Setting RouteStepNo({4}) InputTrackingData({5})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.INPUTTRACKDATA.Trim());

        //                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
        //                robotConText.SetReturnMessage(errMsg);

        //                return false;

        //            }

        //            #endregion

        //            #region [ 取Job所在的Stage, 檢查Stage的Tracking Data Bit在Job Tracking Data是否有On ]

        //            string tmp = string.Empty;
        //            checkFlag = CheckJobCurrentStageTrackingData(curBcsJob, out tmp);

        //            #endregion

        //            if (checkFlag == false)
        //            {

        //                #region[DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) curStepNo({5}) TrackingDataError because [{6}]",
        //                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                                            curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo, tmp);

        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }

        //                #endregion

        //                errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) curStepNo({5}) TrackingDataError because [{6}]",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                                            curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo, tmp);

        //                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_StageJob_CheckTrackingData_Fail);
        //                robotConText.SetReturnMessage(errMsg);

        //                return false;

        //            }

        //            #endregion

        //        }

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

        /// <summary>
        /// Check Cool Run Mode CoolRunRemainCount Can not less 1
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //public bool Filter_PortCoolRunRemainCount_For1Arm1Job(IRobotContext robotConText)
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

        //        #region [ Get Robot 所屬Line Entity ]

        //        Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

        //        if (robotLine == null)
        //        {

        //            #region  [DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get Line Entity!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get curBcsJob Entity ]

        //        Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

        //        //找不到 Job 回NG
        //        if (curBcsJob == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobInfo!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get JobInfo!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Check IndexOperMode Cool Run Mode Can Not Cool Run ]

        //        if (robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.COOL_RUN_MODE)
        //        {
        //            #region [ Check IndexOperMode Cool Run Mode CoolRunRemainCount can not < 1 ]
        //            if (robotLine.File.CoolRunRemainCount < 1)
        //            {
        //                #region[DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) IndexOperMode({2}) CoolRunRemainCount({3}) Job CassetteSequenceNo({4}) JobSequenceNo({5}) can not CoolRunRemainCount < 1!",
        //                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, robotLine.File.IndexOperMode.ToString(), robotLine.File.CoolRunRemainCount.ToString(), curBcsJob.CassetteSequenceNo,
        //                                            curBcsJob.JobSequenceNo);

        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }
        //                #endregion
        //                errMsg = string.Format("[{0}] Robot({1}) IndexOperMode({2}) CoolRunRemainCount({3}) Job CassetteSequenceNo({4}) JobSequenceNo({5}) can not CoolRunRemainCount < 1!",
        //                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, robotLine.File.IndexOperMode.ToString(), robotLine.File.CoolRunRemainCount.ToString(), curBcsJob.CassetteSequenceNo,
        //                    curBcsJob.JobSequenceNo);

        //                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CoolRunRemainCount_Is_Fail);
        //                robotConText.SetReturnMessage(errMsg);

        //                return false;
        //            }
        //            #endregion
        //        }

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

        /// <summary>Port FetchOut First Glass Check 
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //public bool Filter_PortFetchOutFirstGlassCheck_For1Arm1Job(IRobotContext robotConText) //20151020 mark 暫時封印
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

        //        #region [ Get curBcsJob Entity ]

        //        Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

        //        //找不到 Job 回NG
        //        if (curBcsJob == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobInfo!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get JobInfo!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get CST Entity by Job's CST Seq ]

        //        Cassette curCST = ObjectManager.CassetteManager.GetCassette(curBcsJob.CassetteSequenceNo);

        //        //找不到 CST 回NG
        //        if (curCST == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get CST Entity by Job CstSeq({2})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo.ToString());

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get CST Entity by Job CstSeq({2})!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo.ToString());

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curCST_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get Port Entity by Job's  ]

        //        //Cassette curCST = ObjectManager.CassetteManager.GetCassette(curBcsJob.CassetteSequenceNo);
        //        Port curPort = ObjectManager.PortManager.GetPort(curBcsJob.SourcePortID);

        //        //找不到 CST 回NG
        //        if (curPort == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get Port Entity by Job SourcePortID({2})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.SourcePortID);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get Port Entity by Job SourcePortID({2})!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.SourcePortID);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curCST_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Check First Glass Check Mode ]

        //        // Y:OK, Robort can start fetch glass from cst
        //        if (curCST.FirstGlassCheckReport != "Y")
        //        {
        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
        //            robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

        //            return true;
        //        }
        //        if (curCST.FirstGlassCheckReport != "C2") //C2:before fetch glass from cst, invoke MES.LotProcessStartRequest
        //        {
        //            //Invoke MESService First Glass Check
        //            string trxID = UtilityMethod.GetAgentTrackKey();

        //            //LotProcessStartRequest(string trxID, Port port, Cassette cst, Job job)
        //            Invoke(eServiceName.MESService, "LotProcessStartRequest", new object[] { trxID, curCST, curBcsJob});

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FirstGlassCheckReport({2}). Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Fetch Out!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curCST.FirstGlassCheckReport, curBcsJob.CassetteSequenceNo,
        //                                        curBcsJob.JobSequenceNo);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) FirstGlassCheckReport({2}). Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Fetch Out!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curCST.FirstGlassCheckReport, curBcsJob.CassetteSequenceNo,
        //                                    curBcsJob.JobSequenceNo);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_PortFetchOutFirstGlassCheck_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }
        //        else
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FirstGlassCheckReport({2}). Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Fetch Out!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curCST.FirstGlassCheckReport, curBcsJob.CassetteSequenceNo,
        //                                        curBcsJob.JobSequenceNo);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) FirstGlassCheckReport({2}). Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Fetch Out!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curCST.FirstGlassCheckReport, curBcsJob.CassetteSequenceNo,
        //                                    curBcsJob.JobSequenceNo);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_PortFetchOutFirstGlassCheck_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

        //        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
        //        robotConText.SetReturnMessage(ex.Message);

        //        return false;
        //    }

        //}

    }
}
