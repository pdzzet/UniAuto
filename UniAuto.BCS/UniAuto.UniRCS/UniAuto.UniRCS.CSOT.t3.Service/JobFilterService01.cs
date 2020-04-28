using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class JobFilterService
    {

//Filter Funckey = "FL" + XXXX(序列號)

        /// <summary> No Any Filter Condition, alway return OK
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey 
        [UniAuto.UniBCS.OpiSpec.Help("FL0016")]
        public bool Filter_ByPass(IRobotContext robotConText)
        {
            try
            {
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

        /// <summary> Check Stage Recipe Filter Condition
        /// 如果需要Filter RecipeByPass 與 Recipe Check兩條件時，先使用(1)Filter_RecipeByPass -> (2)Filter_ReceipeCheck
        /// 會把Recipe Check Result 收集在Job.RobotWIP.StageRecipeCheckMismatch Dictionary
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0008")]
        public bool Filter_ReceipeCheck(IRobotContext robotConText)
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

                #region [ check Step by is2ndCmdFlag ]
                int tmpStepNo = 0;
                string funcName = string.Empty;
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

                #region [ Get Current Step Entity ]
                if (!curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(tmpStepNo))
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

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get curRouteStep({3})!",
                                         MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                         curBcsJob.JobSequenceNo, tmpStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

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
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

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

                #region [ Get Stage is PUT]
                string EQPMsg = string.Empty;

                if (curRouteStep.Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT)
                {

                    string[] stageList = curRouteStep.Data.STAGEIDLIST.Split(',');

                    for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                    {
                        RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);
                        
                        #region [ 防呆 ]

                        if (curStepStage == null)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST, stageList[stageIdx]);
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

                        //判斷Current Step Stage 是否存在於Current LDRQ Stage List
                        if (curStageList.Contains(curStepStage) == false)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST, curStepStage.Data.STAGEID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            //不存在則視同判斷失敗直接記Log跳下一個Stage
                            continue;
                        }

                        if (curStepStage.Data.RECIPECHENCKFLAG == "Y")
                        {
                            #region Recipe Mismatch and Recipe Auto Change and Recipe ID Check
                            string curNodePPID = string.Empty;
                            string curByPassPPID = string.Empty;
                            Equipment stageEQP = ObjectManager.EquipmentManager.GetEQP(curStepStage.Data.NODENO);

                            if (stageEQP == null)
                            {
                                #region  [DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) can not find EQP by EQPNo({4})!",
                                                                            curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.Data.NODENO);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                                return false;
                            }

                            curNodePPID = curBcsJob.PPID.Substring(stageEQP.Data.RECIPEIDX, stageEQP.Data.RECIPELEN);

                            //EQP Recipe ID Check Enable/Disable or Auto Change Enable/Diable
                            //Recipe is the same always Receive glass
                            if (stageEQP.File.CurrentRecipeID.Trim() != curNodePPID.Trim())
                            {
                                #region Recipe Auto Change Enable -->Recipe ID Check Enable/Disable
                                if (stageEQP.File.AutoRecipeChangeMode == eEnableDisable.Enable)
                                {
                                    #region Recipe ID Check Enable/Disable
                                    //(1) Recipe Different, No glass Count is Receive glass. 
                                    //(2) Recipe Different, Has glass Count is Reject glass.
                                    if (stageEQP.File.TotalTFTJobCount + stageEQP.File.TotalCFProductJobCount + stageEQP.File.TotalDummyJobCount
                                        + stageEQP.File.ThroughDummyJobCount + stageEQP.File.ThicknessDummyJobCount > 0)
                                    {
                                        //curBcsJob.RobotWIP.StageRecipeCheckMismatch.Add(curRouteStage.Data.STAGEID, "Y");
                                        #region  [DebugLog]
                                        if (IsShowDetialLog == true)
                                        {
                                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) Recipe Auto Change Enable,but EQP Total Count > 0 And  EQP Current Recipe ID[{3}] <> Glass Recipe[{4}] Check Fail!",
                                                                                    curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, stageEQP.File.CurrentRecipeID.Trim(),curNodePPID.Trim());
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }
                                        #endregion
                                        curStageList.Remove(curStepStage);

                                    }
                                    else
                                    {
                                        #region  [DebugLog]
                                        if (IsShowDetialLog == true)
                                        {
                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Recipe Check OK, Recipe Auto Change Enable,and EQP Total Count = 0.",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        } 
                                        #endregion
                                    }
                                    #endregion
                                }
                                #endregion
                                #region Recipe Auto Change Disable -->Recipe ID Check Enable/Disable
                                else
                                {
                                    #region Recipe ID Check Enable
                                    if (stageEQP.File.RecipeIDCheckMode == eEnableDisable.Enable)
                                    {   //Reject the Different Recipe Glass
                                        //curBcsJob.RobotWIP.StageRecipeCheckMismatch.Add(curRouteStage.Data.STAGEID, "Y");
                                        #region  [DebugLog]
                                        if (IsShowDetialLog == true)
                                        {
                                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) Recipe Auto Change Disable,but Recipe ID Check Enable .EQP Current Recipe ID[{3}] <> Glass Receipe[{4}] Check Fail!",
                                                                                    curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, stageEQP.File.CurrentRecipeID.Trim(),curNodePPID.Trim());
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }
                                        #endregion
                                        curStageList.Remove(curStepStage);
                                    }
                                    else
                                    {
                                        #region  [DebugLog]
                                        if (IsShowDetialLog == true)
                                        {
                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Recipe Check OK,Recipe Auto Change Disable And Recipe ID Check Disable!.",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }
                                        #endregion
                                    }
                                    #endregion
                                    }
                                #endregion
                            }
                            else
                            {
                                #region[DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Recipe Check OK,Stage Current Recipe = Glass Recipe.",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                            }
                            #endregion
                            EQPMsg += string.Format("EQP({0}):AutoRecipeChangeMode({1}) RecipeIDCheckMode({2}) CurrentRecipeID({3}). ", stageEQP.Data.NODENO, stageEQP.File.AutoRecipeChangeMode.ToString(), stageEQP.File.RecipeIDCheckMode.ToString(), stageEQP.File.CurrentRecipeID);
                        }
                        else
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Recipe Check OK,Stage RecipeCheckFlag is 'N'.",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                    }

                }
                #endregion
                if (curStageList.Count > 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Recipe Check OK!",
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3})  curRouteID({4}) curStepNO({5}) {6}JobPPID({7}) Filter Recipe Check NG!No Any Stage could Receive glass.(please check 1.EQP.File.AutoRecipeChangeMode = Off 2.EQP.File.RecipeIDCheckMode = ON 3.EQP.File.CurrentRecipeID and Job.PPID are not same)",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), EQPMsg, curBcsJob.PPID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    //20151120 add Rtn ErrMsg
                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) JobPPID({5}) But {6} Filter Recipe Check NG!No Any Stage could Receive glass.(please check 1.EQP.File.AutoRecipeChangeMode = Off 2.EQP.File.RecipeIDCheckMode = ON 3.EQP.File.CurrentRecipeID and Job.PPID are not same)",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                           curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.PPID, EQPMsg); 

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RecipeCheck_NoAnyStage_Receive);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_RecipeCheck_NoAnyStage_Receive;//add for BMS Error Monitor
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
        /// 提前check Recipe
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0053")]
        public bool Filter_Pre_RecipeCheck(IRobotContext robotConText)
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


                #region [ Get Check Next Step Entity ]
                if (!curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo))
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.NextStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get curRouteStep({3})!",
                                         MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                         curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.NextStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }


                RobotRouteStep curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];

                //找不到 CurStep Route 回NG
                if (curCheckRouteStep == null)
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

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get curRouteStep({3})!",
                                         MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                         curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.NextStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }
                #endregion



                #region [ Get Current Check  Next Step Stage List ]
                List<RobotStage> curCheckNextStepStageList = (List<RobotStage>)robotConText[eRobotContextParameter.NextStepCanUseStageList];

                if (curCheckNextStepStageList == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepID({5}) StageIDList({6}) can not Find Stage Status is LDRQ!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4})  StageIDList({5}) can not Find Stage Status is LDRQ!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion

                #region [ Get Stage is PUT]
                string EQPMsg = string.Empty;

                if (curCheckRouteStep.Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT)
                {

                    string[] stageList = curCheckRouteStep.Data.STAGEIDLIST.Split(',');

                    for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                    {
                        RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);

                        #region [ 防呆 ]

                        if (curStepStage == null)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curCheckRouteStep.Data.STEPID, curCheckRouteStep.Data.STAGEIDLIST, stageList[stageIdx]);
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

                        //判斷Current Step Stage 是否存在於Current LDRQ Stage List
                        if (curCheckNextStepStageList.Contains(curStepStage) == false)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curCheckRouteStep.Data.STEPID, curCheckRouteStep.Data.STAGEIDLIST, curStepStage.Data.STAGEID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            //不存在則視同判斷失敗直接記Log跳下一個Stage
                            continue;
                        }

                        if (curStepStage.Data.RECIPECHENCKFLAG == "Y")
                        {
                            #region Recipe Mismatch and Recipe Auto Change and Recipe ID Check
                            string curNodePPID = string.Empty;
                            string curByPassPPID = string.Empty;
                            Equipment stageEQP = ObjectManager.EquipmentManager.GetEQP(curStepStage.Data.NODENO);

                            if (stageEQP == null)
                            {
                                #region  [DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) can not find EQP by EQPNo({4})!",
                                                                            curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.Data.NODENO);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                                return false;
                            }

                            curNodePPID = curBcsJob.PPID.Substring(stageEQP.Data.RECIPEIDX, stageEQP.Data.RECIPELEN);

                            //EQP Recipe ID Check Enable/Disable or Auto Change Enable/Diable
                            //Recipe is the same always Receive glass
                            if (stageEQP.File.CurrentRecipeID.Trim() != curNodePPID.Trim())
                            {
                                #region Recipe Auto Change Enable -->Recipe ID Check Enable/Disable
                                if (stageEQP.File.AutoRecipeChangeMode == eEnableDisable.Enable)
                                {
                                    #region Recipe ID Check Enable/Disable
                                    //(1) Recipe Different, No glass Count is Receive glass. 
                                    //(2) Recipe Different, Has glass Count is Reject glass.
                                    if (stageEQP.File.TotalTFTJobCount + stageEQP.File.TotalCFProductJobCount + stageEQP.File.TotalDummyJobCount
                                        + stageEQP.File.ThroughDummyJobCount + stageEQP.File.ThicknessDummyJobCount > 0)
                                    {
                                        //curBcsJob.RobotWIP.StageRecipeCheckMismatch.Add(curRouteStage.Data.STAGEID, "Y");
                                        #region  [DebugLog]
                                        if (IsShowDetialLog == true)
                                        {
                                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) Recipe Auto Change Enable,but EQP Total Count > 0 And  EQP Current Recipe ID[{3}] <> Glass Recipe[{4}] Check Fail!",
                                                                                    curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, stageEQP.File.CurrentRecipeID.Trim(), curNodePPID.Trim());
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }
                                        #endregion
                                        curCheckNextStepStageList.Remove(curStepStage);

                                    }
                                    else
                                    {
                                        #region  [DebugLog]
                                        if (IsShowDetialLog == true)
                                        {
                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Recipe Check OK, Recipe Auto Change Enable,and EQP Total Count = 0.",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }
                                        #endregion
                                    }
                                    #endregion
                                }
                                #endregion
                                #region Recipe Auto Change Disable -->Recipe ID Check Enable/Disable
                                else
                                {
                                    #region Recipe ID Check Enable
                                    if (stageEQP.File.RecipeIDCheckMode == eEnableDisable.Enable)
                                    {   //Reject the Different Recipe Glass
                                        //curBcsJob.RobotWIP.StageRecipeCheckMismatch.Add(curRouteStage.Data.STAGEID, "Y");
                                        #region  [DebugLog]
                                        if (IsShowDetialLog == true)
                                        {
                                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) Recipe Auto Change Disable,but Recipe ID Check Enable .EQP Current Recipe ID[{3}] <> Glass Receipe[{4}] Check Fail!",
                                                                                    curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, stageEQP.File.CurrentRecipeID.Trim(), curNodePPID.Trim());
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }
                                        #endregion
                                        curCheckNextStepStageList.Remove(curStepStage);
                                    }
                                    else
                                    {
                                        #region  [DebugLog]
                                        if (IsShowDetialLog == true)
                                        {
                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Recipe Check OK,Recipe Auto Change Disable And Recipe ID Check Disable!.",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }
                                        #endregion
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                #region[DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Recipe Check OK,Stage Current Recipe = Glass Recipe.",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                            }
                            #endregion
                            EQPMsg += string.Format("EQP({0}):AutoRecipeChangeMode({1}) RecipeIDCheckMode({2}) CurrentRecipeID({3}). ", stageEQP.Data.NODENO, stageEQP.File.AutoRecipeChangeMode.ToString(), stageEQP.File.RecipeIDCheckMode.ToString(), stageEQP.File.CurrentRecipeID);
                        }
                        else
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Recipe Check OK,Stage RecipeCheckFlag is 'N'.",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                    }

                }
                #endregion
                if (curCheckNextStepStageList.Count > 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Recipe Check OK!",
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3})  curRouteID({4}) curStepNO({5}) {6}JobPPID({7}) Filter Recipe Check NG!No Any Stage could Receive glass.(please check 1.EQP.File.AutoRecipeChangeMode = Off 2.EQP.File.RecipeIDCheckMode = ON 3.EQP.File.CurrentRecipeID and Job.PPID are not same)",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), EQPMsg, curBcsJob.PPID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    //20151120 add Rtn ErrMsg
                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) JobPPID({5}) But {6} Filter Recipe Check NG!No Any Stage could Receive glass.(please check 1.EQP.File.AutoRecipeChangeMode = Off 2.EQP.File.RecipeIDCheckMode = ON 3.EQP.File.CurrentRecipeID and Job.PPID are not same)",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                           curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.PPID, EQPMsg);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RecipeCheck_NoAnyStage_Receive);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_RecipeCheck_NoAnyStage_Receive;//add for BMS Error Monitor
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

        /// <summary> Check Stage Recipe Filter Condition
        /// 如果需要Filter RecipeByPass 與 Recipe Check兩條件時，先使用(1)Filter_RecipeByPass -> (2)Filter_ReceipeCheck
        /// 會把Recipe Check Result 收集在Job.RobotWIP.StageRecipeCheckMismatch Dictionary
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0027")]
        public bool Filter_ChangerLine(IRobotContext robotConText)
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

               // Get Get Put Put
                //2 Slot is Full
                //2 Slot is Empty
                //VCR stage

                #region [ Get Stage is PUT]

                if (curRouteStep.Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT)
                {
                    string[] stageList = curRouteStep.Data.STAGEIDLIST.Split(',');

                    for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                    {
                        RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);

                        #region stageList取出的Stage 防止為空

                        if (curStepStage == null)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST, stageList[stageIdx]);
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

                        //判斷Current Step Stage 是否存在於Current LDRQ Stage List
                        if (curStageList.Contains(curStepStage) == false)
                        {
                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curRouteStep.Data.STEPID, 
                                                        curRouteStep.Data.STAGEIDLIST, curStepStage.Data.STAGEID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //不存在則視同判斷失敗直接記Log跳下一個Stage
                            continue;
                        }

                        #region [Get Port Entity]

                        Port port = ObjectManager.PortManager.GetPort(curStepStage.Data.STAGEID);
                        
                        if (port == null)
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Port By StageID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curStepStage.Data.STAGEID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Port By StageID({3})!",
                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curStepStage.Data.STAGEID);

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                            robotConText.SetReturnMessage(errMsg);
                            errCode = eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail;//add for BMS Error Monitor
                            if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                            return false;
                        }

                        #endregion

                        //port.File.JobExistenceSlot





                    }

                }
                #endregion

                #region[DebugLog]
                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Recipe Check OK!",
                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
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

        /// <summary>
        /// 檢查是否有PortMode符合JobJudge, 有符合才出片
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0028")]
        public bool Filter_JobJudgePortMode(IRobotContext robotConText)
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
                foreach (RobotStage stage in curStageList)
                {
                    // Port Stage 且有空 Slot
                    if (stage.Data.STAGETYPE == eRobotStageType.PORT && stage.curLDRQ_EmptySlotList.Count > 0)
                    {
                        Port port = ObjectManager.PortManager.GetPort(stage.Data.LINEID, stage.Data.NODENO, stage.Data.STAGEIDBYNODE);
                        if (port != null)
                        {
                            //if ((curBcsJob.RobotWIP.CurSendOutJobJudge == job_judge_ok && (port.File.Mode == ePortMode.OK || port.File.Mode == ePortMode.EMPMode)) ||
                            //    (curBcsJob.RobotWIP.CurSendOutJobJudge != job_judge_ok && (port.File.Mode == ePortMode.NG || port.File.Mode == ePortMode.EMPMode)))
                            if (ForCF_CheckJobJudge_PortMode_UnloadingPortSetting(curBcsJob, port))
                            {
                                match_port = port;
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
                    if (match_port != null)
                        break;
                }

                if (match_port == null)
                {
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4}) JobJudge({5}) Cannot find Match Port Mode(1.eqp.File.ProductTypeCheckMode=Disable,port mode= same JobJudge/EMP/MIX 2.eqp.File.ProductTypeCheckMode=Enable,port.File.ProductType=0 or =Job.ProductType,port mode= same JobJudge/EMP/MIX)",
                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, curRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.CurSendOutJobJudge);

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

        /// <summary> Check Port CST SettingCode by Job SettingCode for Normal and Cell Special Type Common Function
        ///  
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0009")]
        public bool Filter_UDDipatchRule_BySettingCode(IRobotContext robotConText)
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

                #region [ Check CST SetCode by CurCanUseStageList ]

                string[] stageList = tmpRouteStep.Data.STAGEIDLIST.Split(',');
                string strCassetteSettingCodeS = "";
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

                    Port port = ObjectManager.PortManager.GetPort(curStepStage.Data.STAGEID);

                    if (port == null)
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

                    #region [ Check Job SetCode 與 Port Stage SetCode是否相同 ]

                    //201601114 add byPort FailCode for Unloader Dispatch Rule
                    //fail_ReasonCode = string.Format("JobFilterService_Filter_UDDipatchRule_BySettingCode_{0}_{1}", tmpStepNo.ToString(), curStepStage.Data.STAGEID);

                    if (curBcsJob.CellSpecial.CassetteSettingCode != port.File.CassetteSetCode)
                    {                      

                        //找不到符合Port Dispatch Rule則記Log直接跳下一個Stage
                        #region [ Add To Check Fail Message To Job ]

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) SetCode({4}) but StageID({5}) SetCode({6}) is different!!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.CellSpecial.CassetteSettingCode, curStepStage.Data.STAGEID, port.File.CassetteSetCode);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        //{

                        //    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) SetCode({4}) but StageID({5}) SetCode({6}) is different!",
                        //                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        //                            curBcsJob.CellSpecial.CassetteSettingCode, curStepStage.Data.STAGEID, port.File.CassetteSetCode);

                        //    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        //    #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //    //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) SetCode({3}) but StageID({4}) SetCode({5}) is different!",
                        //    //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        //    //                        curBcsJob.CellSpecial.CassetteSettingCode, curStepStage.Data.STAGEID, port.File.CassetteSetCode);

                        //    failMsg = string.Format("Job({0}_{1}) SetCode({2}) but StageID({3}) SetCode({4}) is different!",
                        //                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        //                            curBcsJob.CellSpecial.CassetteSettingCode, curStepStage.Data.STAGEID, port.File.CassetteSetCode);

                        //    AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                        //    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        //    #endregion
                        //}

                        #endregion
                        strCassetteSettingCodeS += string.Format("PortID({0}) CSTSetCode({1}),", curStepStage.Data.STAGEID, port.File.CassetteSetCode);
                        curStageList.Remove(curStepStage);
                        continue;
                    }
                    else
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) SetCode({4}) and StageID({5}) SetCode({6}) is match!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.CellSpecial.CassetteSettingCode, curStepStage.Data.STAGEID, port.File.CassetteSetCode);

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //找到符合的Port SetCode Remove FailCode
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                    }

                    #endregion
                }
    
                #endregion

                if (curStageList.Count > 0)
                {
                    #region[DebugLog]
                    
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Unloader Dispatch Rule By Setting Code OK!",
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO({5}) Filter Unloader Dispatch Rule By Setting Code NG! No Any Stage could Receive glass.",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                        curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) curCSTSetCode({5}) But UnloadingPort {6} Filter Unloader Dispatch Rule By Setting Code NG! No Any Stage could Receive glass.",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,
                                           curBcsJob.RobotWIP.CurStepNo.ToString(),curBcsJob.CellSpecial.CassetteSettingCode, strCassetteSettingCodeS);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleBySettingCode_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleBySettingCode_Fail;//add for BMS Error Monitor
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

        /// <summary> Check Unloader Dispatch Rule
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0017")]
        public bool Filter_UDDipatchRule_ByJudge(IRobotContext robotConText)
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

                #region [ Get Stage is PUT]
                if (curRouteStep.Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT)
                {
                    string[] stageList = curRouteStep.Data.STAGEIDLIST.Split(',');
                    for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                    {
                        RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);

                        if (curStepStage == null)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST, stageList[stageIdx]);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            errMsg = string.Format("[{0}] can not Get RobotStageInfo by StageID({1}!",
                                MethodBase.GetCurrentMethod().Name, stageList[stageIdx]);

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_Stage_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        //判斷Current Step Stage 是否存在於Current LDRQ Stage List
                        if (curStageList.Contains(curStepStage) == false)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST, curStepStage.Data.STAGEID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            //不存在則視同判斷失敗直接記Log跳下一個Stage
                            continue;
                        }

                        Port port = ObjectManager.PortManager.GetPort(curStepStage.Data.STAGEID);
                        if (port == null)
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Port By StageID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curStepStage.Data.STAGEID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Port By StageID({3})!",
                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curStepStage.Data.STAGEID);

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleByJudge_Fail);
                            robotConText.SetReturnMessage(errMsg);

                            errCode = eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleByJudge_Fail;//add for BMS Error Monitor
                            if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                            return false;
                        }

                        if (curBcsJob.CellSpecial.CassetteSettingCode != port.File.CassetteSetCode)
                        {
                            if (!curStageList.Contains(curStepStage))
                                curStageList.Remove(curStepStage);
                        }
                    }
                }
                #endregion

                #region[DebugLog]
                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Unloader Dispatch Rule By Setting Code OK!",
                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
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

        /// <summary> CVD 抽片比例 Mix Mode下有設抽片比例，可以依比例抽片,不用切到Mix Mode下run
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0012")]
        public bool Filter_CVDProportionalRule(IRobotContext robotConText)
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
                    if(curBcsJob.ArraySpecial.ProcessType == "1")
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) EQPRTC Job CassetteSequenceNo({2}) JobSequenceNo({3}) curProcessType is({4}),not need Check!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.ArraySpecial.ProcessType);

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                        return true;
                    }
                }
                #endregion

                #region 現在的比例、是否要更換抽片比例
                eCVDIndexRunMode jobProcType = new eCVDIndexRunMode();
                //if (curBcsJob.ArraySpecial.ProcessType == "0")
                //    jobProcType = eCVDIndexRunMode.PROD;
                //else
                //    jobProcType = eCVDIndexRunMode.MQC;
                if (curRobot.Data.LINEID != "TCCVD700")
                {
                    #region 一种product和MQC混run
                    if (curBcsJob.ArraySpecial.ProcessType == "0")
                        jobProcType = eCVDIndexRunMode.PROD;
                    else
                        jobProcType = eCVDIndexRunMode.MQC;
                    #endregion
                }
                else
                {
                    //modify by hujunpeng 20190425 for CVD700新增一个product进行混run逻辑,Deng,20190823
                    #region 两种product和MQC混run
                    if (curBcsJob.ArraySpecial.ProcessType == "0")
                        jobProcType = eCVDIndexRunMode.PROD;
                    else if (curBcsJob.ArraySpecial.ProcessType == "1")
                        jobProcType = eCVDIndexRunMode.MQC;
                    else if (curBcsJob.ArraySpecial.ProcessType == "2")
                        jobProcType = eCVDIndexRunMode.PROD1;
                    else
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Current jobPorcType{0} is unknow!", curBcsJob.ArraySpecial.ProcessType));
                    #endregion
                }

                if (curRobot.File.CurCVDProportionalRule.curProportionalType == jobProcType)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter CVD Proportinoal Rule Check OK!",
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO({5}) Process Type({6}) is Not Match, EQP Current Type are ({7}):({8}) Filter CVD Proportinoal Rule Check NG.",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                        curBcsJob.RobotWIP.CurStepNo.ToString(),curBcsJob.ArraySpecial.ProcessType, curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD], curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC]);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) Process Type({5}) is Not Match, EQP Current Type are ({6}) Filter CVD Proportinoal Rule Check NG.",
                                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                        curBcsJob.RobotWIP.CurStepNo.ToString(),jobProcType.ToString(), curRobot.File.CurCVDProportionalRule.curProportionalType.ToString());
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CVDFetchGlassProportionalRule_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_CVDFetchGlassProportionalRule_Fail;//add for BMS Error Monitor
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

        /// <summary>
        /// Check WIP in current Stage Recipe ID Is 00 (ByPass)
        /// </summary>
        /// <param name="curStage">Stage</param>
        /// <param name="curJob">Job</param>
        /// <returns>Recipe IS 00 By Pass</returns>
        private bool CheckStageIsRecipeByPass(RobotStage curStage, Job curJob)
        {
            try
            {
                string curNodePPID = string.Empty;
                string curByPassPPID = string.Empty;
                string strlog = string.Empty;
                Equipment stageEQP = ObjectManager.EquipmentManager.GetEQP(curStage.Data.NODENO);

                if (stageEQP == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3})can not find EQP by EQPNo({4})!",
                                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGENAME, curStage.Data.STAGEID, curStage.Data.NODENO);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }

                curNodePPID = curJob.PPID.Substring(stageEQP.Data.RECIPEIDX, stageEQP.Data.RECIPELEN);
                curByPassPPID = new string('0', stageEQP.Data.RECIPELEN);

                if (curNodePPID == curByPassPPID)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageName({2}) StageID({3}) RecipeIndex({4}) RecipeLen({5}) ,Job CassetteSequenceNo({6}) JobSequenceNo({7}) WIP PPID({8}) But Stage PPID({9}) is by Pass!",
                                                               curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGENAME, curStage.Data.STAGEID,
                                                               stageEQP.Data.RECIPEIDX.ToString(), stageEQP.Data.RECIPELEN.ToString(), curJob.CassetteSequenceNo, curJob.JobSequenceNo, curJob.PPID, curNodePPID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return true;
                }
                else
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageNo({2}) StageName({3}) RecipeIndex({4}) RecipeLen({5}) Job CassetteSequenceNo({6}) JobSequenceNo({7}) WIP PPID({8}) and Stage PPID({9}) is not by Pass!",
                                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                                stageEQP.Data.RECIPEIDX, stageEQP.Data.RECIPELEN, curJob.CassetteSequenceNo, curJob.JobSequenceNo, curJob.PPID, curNodePPID);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// Watson add 20151019 For TTP
        /// In TCTTP Line When glass Store in TTP EQP must Check two Bit.
        /// Interface EQ2EQ InterLock Daily Check Bit and the Glass with EQP Flag Daily Check 
        /// If two bit Mismatch is False.
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns>False is not Fetch</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0015")]
        public bool Filter_CheckTTPStoreINSubChamber(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;
            eBitResult EQPFlag_ToTotalPitchSubChamberBit = eBitResult.OFF; //如果trx沒有的話，應先考慮是off的狀態，不能不給值
            eBitResult EQ2EQInterLock_DailyCheckBitBit;
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

                //IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(curBcsJob.EQPFlag, "EQPFlag");

                if (!is2ndCmdFlag)
                {
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
                        return false;
                    }
                    #endregion
                }
                else
                {
                    #region [ Get Next Step Entity ]
                    RobotRouteStep curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
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
                        errMsg = string.Format("[{0}] Job({1}_{2}) can not Get JobcurRouteStep({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                    #endregion

                    #region [ Check CurStep Action Must PUT ]
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
                    //subItem = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG, "EQPFlag");

                    #endregion
                }


                #region [ Check Daily Check Flag]
                if (Check_JobTTPDailyCheck_BitON(curBcsJob))
                {
                    EQPFlag_ToTotalPitchSubChamberBit = eBitResult.ON;
                }
                else
                {
                    EQPFlag_ToTotalPitchSubChamberBit = eBitResult.OFF;
                }
                #endregion

                EQ2EQInterLock_DailyCheckBitBit = Check_TTPEQ2EQInterlock_DailyCheckBit(curRobot);

                if (EQ2EQInterLock_DailyCheckBitBit != EQPFlag_ToTotalPitchSubChamberBit)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) EQ to EQ InterLock DailyCheck Bit[{4}] Mismatch  EQPFlag TTPSubChamber Bit[{5}] can not Store IN SubChamber EQP!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, EQ2EQInterLock_DailyCheckBitBit, EQPFlag_ToTotalPitchSubChamberBit);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) EQ to EQ InterLock DailyCheck Bit[{3}] Mismatch  EQPFlag TTPSubChamber Bit[{4}] can not Store IN SubChamber EQP!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                             EQ2EQInterLock_DailyCheckBitBit, EQPFlag_ToTotalPitchSubChamberBit);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_FetchOutSampingFlag_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_FetchOutSampingFlag_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }
                else
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) EQ to EQ InterLock DailyCheck Bit[{4}] =  EQPFlag TTPSubChamber Bit[{5}] Store In SubChamber EQP!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, EQ2EQInterLock_DailyCheckBitBit, EQPFlag_ToTotalPitchSubChamberBit);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
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

       
        /// <summary>確認目前為Cool Run Mode時,取一片時Cool Run Remain Count 不能小於1,取兩片時Cool Run Remain Count 不能小於2
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0014")]
        public bool Filter_PortFetchOutCoolRunRemainCount_For1Arm1Job(IRobotContext robotConText)
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

                #region [ Check IndexOperMode Cool Run Mode Can Not Cool Run ]
                UniAuto.UniRCS.Core.RobotCoreService.cur1stJob_CommandInfo cur1stJobCmdInfo = (UniAuto.UniRCS.Core.RobotCoreService.cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo];

                if (robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.COOL_RUN_MODE)
                {
                    if (Workbench.LineType == eLineType.ARRAY.OVNITO_CSUN || Workbench.LineType == eLineType.ARRAY.OVNPL_YAC)
                    {
                        #region OVN PL ITO 檢查 TCOVN_PL_ITO_RobotParam
                        if (((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job01 != null)
                        {
                            // OVN ITO PL 做 Both Arm Get, Both Arm Put 且已經找到第一筆 JOB, 正在找第二筆 JOB
                            if (robotLine.File.CoolRunRemainCount < 2)
                            {
                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) IndexOperMode({2}) CoolRunRemainCount({3}) Job CassetteSequenceNo({4}) JobSequenceNo({5}) curRouteID({6}) curStepNO({7}) can not CoolRunRemainCount < 2!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, robotLine.File.IndexOperMode.ToString(), robotLine.File.CoolRunRemainCount.ToString(), curBcsJob.CassetteSequenceNo,
                                                            curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                                errMsg = string.Format("[{0}] IndexOperMode({1}) CoolRunRemainCount({2}) Job({3}_{4}) curRouteID({5}) curStepNO({6}) can not CoolRunRemainCount < 2!",
                                    MethodBase.GetCurrentMethod().Name, robotLine.File.IndexOperMode.ToString(), robotLine.File.CoolRunRemainCount.ToString(), curBcsJob.CassetteSequenceNo,
                                    curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CoolRunRemainCount_Is_Fail);
                                robotConText.SetReturnMessage(errMsg);

                                errCode = eJobFilter_ReturnCode.NG_CoolRunRemainCount_Is_Fail;//add for BMS Error Monitor
                                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                return false;
                            }
                        }
                        else
                        {
                            // OVN ITO PL 做 Both Arm Get, Both Arm Put 且正在找到第一筆 JOB
                            if (robotLine.File.CoolRunRemainCount < 1)
                            {
                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) IndexOperMode({2}) CoolRunRemainCount({3}) Job CassetteSequenceNo({4}) JobSequenceNo({5}) curRouteID({6}) curStepNO({7}) can not CoolRunRemainCount < 1!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, robotLine.File.IndexOperMode.ToString(), robotLine.File.CoolRunRemainCount.ToString(), curBcsJob.CassetteSequenceNo,
                                                            curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                                errMsg = string.Format("[{0}] IndexOperMode({1}) CoolRunRemainCount({2}) Job({3}_{4}) curRouteID({5}) curStepNO({6}) can not CoolRunRemainCount < 1!",
                                    MethodBase.GetCurrentMethod().Name, robotLine.File.IndexOperMode.ToString(), robotLine.File.CoolRunRemainCount.ToString(), curBcsJob.CassetteSequenceNo,
                                    curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CoolRunRemainCount_Is_Fail);
                                robotConText.SetReturnMessage(errMsg);
                                errCode = eJobFilter_ReturnCode.NG_CoolRunRemainCount_Is_Fail;//add for BMS Error Monitor
                                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));


                                return false;
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region [ Check IndexOperMode Cool Run Mode,for GetGet case,CoolRunRemainCount can not < 2 ]
                        if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                        {
                            if (robotLine.File.CoolRunRemainCount < 2)
                            {
                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) IndexOperMode({2}) CoolRunRemainCount({3}) Job CassetteSequenceNo({4}) JobSequenceNo({5}) curRouteID({6}) curStepNO({7}) can not CoolRunRemainCount < 2!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, robotLine.File.IndexOperMode.ToString(), robotLine.File.CoolRunRemainCount.ToString(), curBcsJob.CassetteSequenceNo,
                                                            curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                                errMsg = string.Format("[{0}] IndexOperMode({1}) CoolRunRemainCount({2}) Job({3}_{4}) curRouteID({5}) curStepNO({6}) can not CoolRunRemainCount < 2!",
                                    MethodBase.GetCurrentMethod().Name, robotLine.File.IndexOperMode.ToString(), robotLine.File.CoolRunRemainCount.ToString(), curBcsJob.CassetteSequenceNo,
                                    curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CoolRunRemainCount_Is_Fail);
                                robotConText.SetReturnMessage(errMsg);

                                errCode = eJobFilter_ReturnCode.NG_CoolRunRemainCount_Is_Fail;//add for BMS Error Monitor
                                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                                return false;
                            }
                        }
                        #endregion
                        #region [ Check IndexOperMode Cool Run Mode CoolRunRemainCount can not < 1 ]
                        else if (robotLine.File.CoolRunRemainCount < 1)
                        {
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) IndexOperMode({2}) CoolRunRemainCount({3}) Job CassetteSequenceNo({4}) JobSequenceNo({5}) curRouteID({6}) curStepNO({7}) can not CoolRunRemainCount < 1!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, robotLine.File.IndexOperMode.ToString(), robotLine.File.CoolRunRemainCount.ToString(), curBcsJob.CassetteSequenceNo,
                                                        curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            errMsg = string.Format("[{0}] IndexOperMode({1}) CoolRunRemainCount({2}) Job({3}_{4}) curRouteID({5}) curStepNO({6}) can not CoolRunRemainCount < 1!",
                                MethodBase.GetCurrentMethod().Name, robotLine.File.IndexOperMode.ToString(), robotLine.File.CoolRunRemainCount.ToString(), curBcsJob.CassetteSequenceNo,
                                curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_CoolRunRemainCount_Is_Fail);
                            robotConText.SetReturnMessage(errMsg);

                            errCode = eJobFilter_ReturnCode.NG_CoolRunRemainCount_Is_Fail;//add for BMS Error Monitor
                            if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                            return false;
                        }
                        #endregion
                    }
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

        /// <summary> 設定如果啟用則當CST第一片到達EQ A之後,後續同CST的Job都要到EQ A
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0018")]
        public bool Filter_JobSendToSameEQ(IRobotContext robotConText)
        {
            // Filter_JobSendToSameEQ
            // 此Filter Method要排在Filter_CurStepStageIDListLDRQ之後
            // 若 SameEQFlag false, 則不做檢查, 只記Log, return true
            // 若 SameEQFlag true, 則過濾StepCanUseStageList之後return true
            //    若StepCanUseStageList裡沒有SameEQ則return false
            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curFilterCanUseStageList = null;
            string errCode = string.Empty;
            try
            {
                #region JobSendToSameEQ_RobotParam
                JobSendToSameEQ_RobotParam param = null;
                if (StaticContext.ContainsKey(eRobotContextParameter.JobSendToSameEQ_RobotParam) &&
                    StaticContext[eRobotContextParameter.JobSendToSameEQ_RobotParam] is JobSendToSameEQ_RobotParam)
                {
                    param = (JobSendToSameEQ_RobotParam)StaticContext[eRobotContextParameter.JobSendToSameEQ_RobotParam];
                }
                if (param == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] StaticContext has no JobSendToSameEQ_RobotParam",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }
                if (!param.SameEQFlag)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] SameEQFlag is false",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

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

                #region [ Get LDRQ Stage List ]

                curFilterCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curFilterCanUseStageList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StepCanUseStageList is null",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                tmpStageID, tmpStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepNo({4}) StepCanUseStageList is null",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            tmpStageID, tmpStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                if (tmpStageID == eRobotCommonConst.ROBOT_HOME_STAGEID && // 在 RobotArm 上
                    (tmpStepAction == eRobotCmdActionCode.PUT.ToString() ||
                     tmpStepAction == eRobotCmdActionCode.PUTREADY.ToString() ||
                     tmpStepAction == eRobotCmdActionCode.EXCHANGE.ToString()) && // 要放到Stage
                    tmpRouteStep.Data.ROBOTRULE == eRobotRouteStepRule.SELECT && // Stage多選一即可
                    curFilterCanUseStageList.Count > 0)
                {
                    // Job在Arm上, 要放到多選一的Stage, 檢查SameEQMap
                    string node_no = string.Empty;
                    if (curRobot.File.CheckMap(curBcsJob.CassetteSequenceNo, tmpStepNo, out node_no))
                    {
                        bool find_same_node = false;
                        #region find_same_node
                        foreach (RobotStage stage in curFilterCanUseStageList)
                        {
                            if (stage.Data.NODENO == node_no)
                            {
                                find_same_node = true;
                                break;
                            }
                        }
                        #endregion

                        if (find_same_node)
                        {
                            // StepCanUseStageList中有SameEQ, 過濾StepCanUseStageList只保留SameEQ
                            StringBuilder sb = new StringBuilder();
                            sb.Append("Remove Stage(");
                            for (int i = curFilterCanUseStageList.Count - 1; i >= 0; i--)
                            {
                                if (curFilterCanUseStageList[i].Data.NODENO != node_no)
                                {
                                    sb.AppendFormat("{0},", curFilterCanUseStageList[i].Data.STAGEID);
                                    curFilterCanUseStageList.RemoveAt(i);
                                }
                            }
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append(") from StepCanUseStageList");

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                //Remove Stage() from StepCanUseStageList
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", sb.ToString());
                            }

                            #endregion
                        }
                        else
                        {
                            // StepCanUseStageList中沒有SameEQ, return false
                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) [{2}] Job({3}_{4}) curRouteID({5}) curStageID({6}) StepNo({7}) StepCanUseStageList(",
                                             curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, tmpStageID, tmpStepNo);
                            foreach (RobotStage stage in curFilterCanUseStageList)
                                sb.AppendFormat("{0},", stage.Data.STAGEID);
                            sb.Remove(sb.Length - 1, 1);
                            sb.AppendFormat("), no Stage in SameEQNode({0})", node_no);

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", sb.ToString());
                            }

                            #endregion

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                            robotConText.SetReturnMessage(sb.ToString());

                            //errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                            //if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            //    curRobot.CheckErrorList.Add(errCode, Tuple.Create(sb.ToString(), curBcsJob.EQPJobID, "0", "ROBOT"));

                            return false;
                        }
                    }
                    else
                    {
                        // 同一CST還沒有Job在tmpStepNo時進入機台
                        // 暫不限制SameEQ, 等到Robot Arm Unload時就會設定SameEQMap
                        // do nothing
                    }
                }
                else
                {
                    // do nothing
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

        /// <summary>
        /// Watson add 20151210 For TTP
        /// In TCTTP Line When glass Fetch in TTP EQP must Check two Bit.
        /// Interface EQ2EQ InterLock Daily Check Bit ON  
        /// If two bit Mismatch is False.
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns>False is not Fetch</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0030")]
        public bool Filter_TTPFetchOutByDailyCheck(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            eBitResult EQ2EQInterLock_DailyCheckBitBit;
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

                //IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(curBcsJob.EQPFlag, "EQPFlag");

                if (!is2ndCmdFlag)
                {
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
                        return false;
                    }
                    #endregion
                }
                else
                {
                    #region [ Get Next Step Entity ]
                    RobotRouteStep curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
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

                    #region [ Check CurStep Action Must PUT ]
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
                    #endregion
                }

                EQ2EQInterLock_DailyCheckBitBit = Check_TTPEQ2EQInterlock_DailyCheckBit(curRobot);

                if (EQ2EQInterLock_DailyCheckBitBit == eBitResult.ON )
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO({5}) EQ to EQ InterLock DailyCheck Bit[{6}] can not Fetch out glass for Cassette!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), EQ2EQInterLock_DailyCheckBitBit);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) EQ to EQ InterLock DailyCheck Bit[{5}] can not Fetch out for Cassette !",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,curBcsJob.RobotWIP.CurStepNo.ToString(),
                                             EQ2EQInterLock_DailyCheckBitBit);

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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) EQ to EQ InterLock DailyCheck Bit [{4}] ",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, EQ2EQInterLock_DailyCheckBitBit);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
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

        /// <summary> CF MQC TTP 卡Port Flow 同時只能跑一種Flow(Route)
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20160104 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0038")]
        public bool Filter_PortFetchOutTheSameRoute(IRobotContext robotConText)
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
                                                "L2", MethodBase.GetCurrentMethod().Name);
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

                #region [ Check Robot Port Route Dictionart Info ]
                if (curRobot.CurPortRouteIDInfo.Count < 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Current Port Route is illegal!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_Current_PortRoute);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_Get_Current_PortRoute;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                #endregion

                #region [ Check Port Route the Same ? ]
                if (curRobot.CurPortRouteIDInfo.Count < 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Current Port Route is illegal!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_Current_PortRoute);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_Get_Current_PortRoute;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                #endregion

                #region 現行的Route 要抽的片是一樣的
                //string a = curBcsJob.RobotWIP.CurRouteID;

                //curRobot.Cur_CFMQCTTP_Flow_Route是不是已經做完了？可以換Route了?
                Check_CurrentRouteIsFinish(curRobot);

                if ((curRobot.Cur_CFMQCTTP_Flow_Route.Trim() ==string.Empty ) || (curBcsJob.RobotWIP.CurRouteID  ==  curRobot.Cur_CFMQCTTP_Flow_Route))
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Route ID({4}) = The Last Process Result Route({5})  OK!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curRobot.Cur_CFMQCTTP_Flow_Route);

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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Route ID({4}) <> The Last Process Result Route({5})!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curRobot.Cur_CFMQCTTP_Flow_Route);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Current_PortRouteAndJobRouteIsNotMatch);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_Current_PortRouteAndJobRouteIsNotMatch;//add for BMS Error Monitor
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

        //20160115 add FuncKey Watson 
        [UniAuto.UniBCS.OpiSpec.Help("FL0042")]
        public bool Filter_GAPULDPortDispatchRuleByAssignment(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            //Watson Add 20160118 For GAP Assignment Rule Fail Code Detail
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
            eCELLPortAssignment glassAssignment = new eCELLPortAssignment();
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

                    return false;

                }
                #endregion

                #region [ Check CST SetCode by CurCanUseStageList ]

                string strPortAlignMents = "";
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
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageID({2}) StageType({3}) can not Check by Assignment!",
                                                    curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGEID, curStepStage.Data.STAGETYPE);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        //不是Port Stage則記Log直接跳下一個Stage
                        curStageList.Remove(curStepStage);
                        continue;
                    }

                    Port port = ObjectManager.PortManager.GetPort(curStepStage.Data.STAGEID);
                    if (port == null)
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

                    #region [ Check Job Assignment 與 Port PortAssignment是否相同 ]

                    //20160118 add byPort FailCode for Unloader Dispatch Rule
                   // fail_ReasonCode = string.Format("JobFilterService_Filter_GAPUDDipatchRule_ByAssignment_{0}_{1}", tmpStepNo.ToString(), curStepStage.Data.STAGEID);

                   
                   try
                    {
                        glassAssignment = ObjectManager.PortManager.GetPort(curBcsJob.SourcePortID).File.PortAssignment;
                    }
                    catch (Exception ex)
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Glass Source Port Entity By SourcePortID({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.SourcePortID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Glass Source Port Entity By SourcePortID({3})!",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.SourcePortID);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_Stage_Is_Null);
                        robotConText.SetReturnMessage(errMsg);

                        return false;

                    }

                   if (glassAssignment != port.File.PortAssignment)
                    {

                        //找不到符合Port Dispatch Rule則記Log直接跳下一個Stage
                        #region [ Add To Check Fail Message To Job ]

                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) From Source Port({4}) Assignment({5}) but Target PortID({6}) Assignment ({7}) is different!!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.SourcePortID, glassAssignment, curStepStage.Data.STAGEID, port.File.PortAssignment);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        //if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        //{

                        //    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO({5}) From Source Port({6}) Assignment({7}) but Target PortID({8}) Assignment=({9}) is different!",
                        //                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,curBcsJob.RobotWIP.CurStepNo.ToString(),
                        //                            curBcsJob.SourcePortID, glassAssignment, curStepStage.Data.STAGEID, port.File.PortAssignment);

                        //    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        //    #region [ 記錄Fail Msg To OPI and Robot FailMsg ] 20160720 jack Mark 
                        //    //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) From Source Port({3}) Assignment=({4}) but Target PortID({5}) Assignment=({6}) is different!",
                        //    //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        //    //                        curBcsJob.SourcePortID , glassAssignment , curStepStage.Data.STAGEID, port.File.PortAssignment);

                        //    //failMsg = string.Format("Job({0}_{1}) curRouteID({2}) curStepNO({3}) From Source Port({4}) Assignment=({5}) but Target PortID({6}) Assignment=({7}) is different!",
                        //                           // curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,curBcsJob.RobotWIP.CurStepNo.ToString(),
                        //                           // curBcsJob.SourcePortID, glassAssignment, curStepStage.Data.STAGEID, port.File.PortAssignment);

                        //    //AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                        //    //SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                        //    #endregion
                        //}

                        #endregion
                        strPortAlignMents += string.Format("PortID({0}) Assignment=({1}),",curStepStage.Data.STAGEID, port.File.PortAssignment.ToString());
                        curStageList.Remove(curStepStage);
                        continue;
                    }
                    else
                    {

                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Source Port({4}) Assignment({5}) and Target PortID({6}) Assignment ({7}) is match!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.SourcePortID, glassAssignment, curStepStage.Data.STAGEID, port.File.PortAssignment);

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        //找到符合的Port Assignment Remove FailCode
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                    }

                    #endregion
                }

                #endregion

                if (curStageList.Count > 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter GAP Unloader Dispatch Rule By Assignment Port OK!",
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
                        strlog = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) Glass Assignment({5}) But {6} Filter GAP Unloader Dispatch Rule By Assignment Port NG!,No Any Port could Receive glass.",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                           curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), glassAssignment.ToString(), strPortAlignMents);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) Glass Assignment({5}) But {6} Filter GAP Unloader Dispatch Rule By Assignment Port NG!,No Any Port could Receive glass.",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, 
                                           curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), glassAssignment.ToString(), strPortAlignMents);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleByAssignment_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Job_CheckUnloaderDispatchRuleByAssignment_Fail;//add for BMS Error Monitor
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
        /// Watson add 20151219 For TTP DailyCheck Bit
        /// Get Trx EQ2EQ Interlock Bit
        /// </summary>
        /// <param name="curRobot"></param>
        /// <returns></returns>
        public eBitResult Check_TTPEQ2EQInterlock_DailyCheckBit(Robot curRobot)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string fabType = string.Empty;
            try
            {
                string trxID = string.Empty;


                #region 區分 Array or CF
                if (curRobot.Data.SERVERNAME.Length > 2)
                {
                    string prefix = curRobot.Data.SERVERNAME.Substring(0, 2).ToUpper();
                    switch (prefix)
                    {
                        case "TC":
                            fabType = eFabType.ARRAY.ToString();
                            break;
                        case "FC":
                            fabType = eFabType.CF.ToString();
                            break;
                        default:
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ServerName({2})  Error!!  Can not find FabType Trx Setting Value!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.SERVERNAME);
                            Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            return eBitResult.OFF;
                    }
                }
                #endregion
                if (fabType == eFabType.ARRAY.ToString())
                {
                    #region Array TTP Line L3_EQtoEQInterlockSetForEQP#03 -> L3_B_DailyCheckRequest

                    try   //預防檔案內無此值
                    {
                        //取得SendOut的TrxID
                        trxID = ParameterManager[eParameterXMLConstant.TTP_DAILYCHECK_TRX].GetString();
                    }
                    catch (Exception ex)
                    { }

                    if (trxID == string.Empty)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ParameterManager Read Parameters.xml Error! can not find Array Line TTP_DAILYCHECK_TRX Setting Value ({2})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, eParameterXMLConstant.TTP_DAILYCHECK_TRX);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        trxID = "L3_EQtoEQInterlockSetForEQP#03";
                    }

                    #endregion
                }
                else
                {
                    #region CF MQCTTP_Line L3_EQInterlockSetForEQP#MQCDCRPitchtoIndexer -> L3_B_DailyCheckRequestTotalPitchtoIndexer

                    try   //預防檔案內無此值
                    {
                        //取得SendOut的TrxID
                        trxID = ParameterManager[eParameterXMLConstant.CF_TTP_DAILYCHECK_TRX].GetString();
                    }
                    catch (Exception ex)
                    { }

                    if (trxID == string.Empty)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ParameterManager Read Parameters.xml Error! can not find CF Line TTP_DAILYCHECK_TRX Setting Value ({2})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, eParameterXMLConstant.TTP_DAILYCHECK_TRX);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        trxID = "L3_EQInterlockSetForEQP#MQCDCRPitchtoIndexer";
                    }

                    #endregion
                }

                #region  real time Get Trx by EQtoEQInterlock
                Trx GetJobData_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                if (GetJobData_Trx == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not find TrxID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return eBitResult.OFF;
                }
                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L3_EQtoEQInterlockSetForEQP#03" triggercondition="change">
                //  <eventgroup name="L3_EG_EQtoEQInterlockSetForEQP#03" dir="E2B">
                //    <event name="L3_B_DailyCheckRequest" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<itemgroup name="EQtoEQInterlockBlock">
                //  <item name="EquipmentStatustype" woffset="0" boffset="0" wpoints="0" bpoints="2" expression="BIT" />
                //  <item name="MaterialStatus" woffset="0" boffset="2" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="DailyCheckRequest" woffset="0" boffset="3" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#03" woffset="0" boffset="4" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#04" woffset="0" boffset="5" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#05" woffset="0" boffset="6" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="CleanerSendJobReserveforLocal#02" woffset="0" boffset="7" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="AbnormalForceCleanOut" woffset="0" boffset="8" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="LoadLock1CleanOut" woffset="0" boffset="9" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="LoadLock2CleanOut" woffset="0" boffset="10" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="ProductInfoNotify" woffset="0" boffset="11" wpoints="0" bpoints="1" expression="BIT" />
                //</itemgroup>

                string dailyCheck_Request = string.Empty;
                if (fabType == eFabType.ARRAY.ToString())
                {
                    dailyCheck_Request = GetJobData_Trx.EventGroups[0].Events[0].Items["DailyCheckRequest"].Value;
                }
                else
                {
                    dailyCheck_Request = GetJobData_Trx.EventGroups[0].Events[0].Items["DailyCheckRequestTotalPitchtoIndexer"].Value;
                }

                #endregion

                #endregion


                if (dailyCheck_Request == "1")
                {
                    return eBitResult.ON;
                }
                else
                {
                    return eBitResult.OFF;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return eBitResult.OFF;
            }

        }

        /// <summary>
        /// 確認抽片時是否還有相同的Route還沒抽完的Port(Cassette)，以curRobot.Cur_CFMQCTTP_Flow_Route為基準
        /// curRobot.Cur_CFMQCTTP_Flow_Route是上一片的Route.(When Robot ProcessResult Recode)
        /// 沒有相同就將curRobot.Cur_CFMQCTTP_Flow_Route清為空值(可以換Route抽片了)，
        /// 有的話不置換Route還要繼續卡住不抽其他的玻璃
        /// </summary>
        /// <param name="curRobot"></param>
        public void Check_CurrentRouteIsFinish(Robot curRobot)
        {
            try
            {
                List<RobotStage> curRobotStages = ObjectManager.RobotStageManager.GetRobotStages(curRobot.Data.ROBOTNAME);
                if (curRobotStages == null) 
                    return;
                if (curRobot.Cur_CFMQCTTP_Flow_Route == string.Empty)   //都沒做過也不用比了
                    return;
                foreach (RobotStage stage in curRobotStages)
                {
                    if (stage.Data.STAGETYPE != eStageType.PORT)
                        continue;
                    if ((stage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY) || (stage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY))
                    {
                        if (stage.File.CurRouteID == curRobot.Cur_CFMQCTTP_Flow_Route)
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                string strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) Status Is Running(Send Ready) And Route ID({3})  = The Last Glass Process Result Route ID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, stage.Data.STAGEID ,stage.File.CurRouteID, curRobot.Cur_CFMQCTTP_Flow_Route);

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            return;  //一定能找到還能抽片的Cassette. 就不能換掉curRobot.Cur_CFMQCTTP_Flow_Route
                        }
                        else
                            continue;
                    }
                }
                curRobot.Cur_CFMQCTTP_Flow_Route = string.Empty;  //都沒提前Return表示找不到還有跟上一片相同的Route, 是該置換的時候了
                #region[DebugLog]
                if (IsShowDetialLog == true)
                {
                    string strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) No Port is Ready Fetch out,  The Last Glass Process Result Route ID Will Clear!",
                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion
            }
            catch (Exception ex)
            {
                return;
            }
        }


        //20160302 add for Array Special Recipe Group
        [UniAuto.UniBCS.OpiSpec.Help("FL0049")]
        public bool Filter_FetchOutByRecipeGroup(IRobotContext robotConText)
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

                #region [ 取得Job所在的Stage ]

                RobotStage curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curBcsJob.RobotWIP.CurLocation_StageID);

                //找不到 Stage entity 回NG
                if (curStage == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get Stage by Job curLocation StageID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurLocation_StageID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Stage by Job curLocation StageID({1})!!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_Stage_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check Stage Type 必須要是Port ]

                if (curStage.Data.STAGETYPE != eRobotStageType.PORT)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curLocation StageID({4}) StageType({5}) is not Port!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curStage.Data.STAGETYPE);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curLocation StageID({3}) StageType({4}) is not Port!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, curStage.Data.STAGETYPE);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_SlotBlockInfo_StageType_Is_Not_Port);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_SlotBlockInfo_StageType_Is_Not_Port;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region [ Check Stage curFetchRecipeGroupNoList Is Not Empty ]

                if (curStage.CurRecipeGroupNoList.Count == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curLocation StageID({4}) RecipeGroupNoList is Empty!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curLocation StageID({3}) RecipeGroupNoList is Empty!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_GetStageRecipeGroupNoList_Is_Empty);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_GetStageRecipeGroupNoList_Is_Empty;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region [ 判斷Robot Keep的RecipeGroupNo是否有存在於目前Stage RecipeGroupNoList ]

                string checkRecipeGroupNo = string.Empty;

                if (curRobot.File.CurFetchOutJobRecipeGroupNo != string.Empty)
                {        
                         //mark by yang,不跨CST,抓当前这一片Job所在的PortStage
                    if (curStage.CurRecipeGroupNoList.Find(s => s == curRobot.File.CurFetchOutJobRecipeGroupNo) == null)
                    {
                        //找不到 以Stage Keep的為準
                        checkRecipeGroupNo = curStage.CurRecipeGroupNoList[0];

                    }
                    else
                    {
                        //找到已Robot Keep為準
                        checkRecipeGroupNo = curRobot.File.CurFetchOutJobRecipeGroupNo;

                    }


                }
                else
                {
                    //找不到 以Stage Keep的為準
                    checkRecipeGroupNo = curStage.CurRecipeGroupNoList[0];

                }

                #endregion

                #region [ Check Job RecipeGroupNo by Stage First Priority RecipeGroupNo ]

                if (checkRecipeGroupNo == curBcsJob.ArraySpecial.RecipeGroupNumber.Trim())
                {
                    //20160511 選到RecipeGroupNumber,則將目前RobotSatge(出片的CST)裡的Job remove掉
                    if (curBcsJob.SamplingSlotFlag == "1" && curBcsJob.RobotWIP.CurStepNo == 1) //判斷是不是Step=1,避免Job出片後,JobEachCassetteSlotPosition或Slot Exist沒清,而造成remove AllJobRecipeGroupNoList的Job出錯
                    {
                        if (curStage.AllJobRecipeGroupNoList.Count != 0)
                            curStage.AllJobRecipeGroupNoList.Remove(curBcsJob);

                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) RecipeGroupNo({4}) is Remove from AllJobRecipeGroupNoList!",
                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            curBcsJob.ArraySpecial.RecipeGroupNumber);

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                    }
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) RecipeGroupNo({4}) is Match!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curBcsJob.ArraySpecial.RecipeGroupNumber);

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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO({5}) RecipeGroupNo({6}) but Robot Keep RecipeGroupNo({7}), Stage First RecipeGroupNo({8}) is Mismatch!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,curBcsJob.RobotWIP.CurRouteID,curBcsJob.RobotWIP.CurStepNo.ToString(),
                                        curBcsJob.ArraySpecial.RecipeGroupNumber,curRobot.File.CurFetchOutJobRecipeGroupNo, curStage.CurRecipeGroupNoList[0]);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion



                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) RecipeGroupNo({5}) but Robot Keep RecipeGroupNo({6}), Stage First RecipeGroupNo({7}) is Mismatch!",
                                        curRobot.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                        curBcsJob.ArraySpecial.RecipeGroupNumber, curRobot.File.CurFetchOutJobRecipeGroupNo, curStage.CurRecipeGroupNoList[0]);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_GetStageRecipeGroupNoList_Is_Mismatch);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_GetStageRecipeGroupNoList_Is_Mismatch;//add for BMS Error Monitor
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

        [UniAuto.UniBCS.OpiSpec.Help("FL0050")]
        public bool Filter_FetchOutByProductType(IRobotContext robotConText)
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

                #region [ check Step by is2ndCmdFlag ]
                int tmpStepNo = 0;
                string funcName = string.Empty;
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

                #region [ Get Current Step Entity ]
                if (!curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(tmpStepNo))
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

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get curRouteStep({3})!",
                                         MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                         curBcsJob.JobSequenceNo, tmpStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

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
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

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

                #region [ Check Job Product Type]
                string strEqpProductType = "";//记录各设备ProductType
                if (curRouteStep.Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT)
                {

                    string[] stageList = curRouteStep.Data.STAGEIDLIST.Split(',');

                    //20160520
                    Equipment IndexerEQP = ObjectManager.EquipmentManager.GetEQP("L2");
                    
                    for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                    {
                        RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);

                        #region [ 防呆 ]

                        if (curStepStage == null)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST, stageList[stageIdx]);
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

                        //判斷Current Step Stage 是否存在於Current LDRQ Stage List
                        if (curStageList.Contains(curStepStage) == false)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curRouteStep.Data.STEPID, curRouteStep.Data.STAGEIDLIST, curStepStage.Data.STAGEID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            //不存在則視同判斷失敗直接記Log跳下一個Stage
                            continue;
                        }
                        Equipment stageEQP = ObjectManager.EquipmentManager.GetEQP(curStepStage.Data.NODENO);


                        if (stageEQP == null)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) can not find EQP by EQPNo({4})!",
                                                                        curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.Data.NODENO);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            errMsg = string.Format("[{0}] RobotStage({1}) StageNo({2}) can not find EQP by EQPNo({3})!",
                                                                        MethodBase.GetCurrentMethod().Name, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.Data.NODENO);
                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                            robotConText.SetReturnMessage(errMsg);
                            errCode = eJobFilter_ReturnCode.NG_Get_EQP_Is_Null;//add for BMS Error Monitor
                            if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                            return false;
                        }

                        //20160520
                        if (IndexerEQP == null)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Indexer EQP by EQPNo({2})!",
                                                                        IndexerEQP.Data.NODENO, curStepStage.Data.ROBOTNAME, IndexerEQP.Data.NODENO);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            errMsg = string.Format("[{0}] can not find Indexer EQP by EQPNo({1})!",
                                                                        MethodBase.GetCurrentMethod().Name, IndexerEQP.Data.NODENO);
                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                            robotConText.SetReturnMessage(errMsg);
                            errCode = eJobFilter_ReturnCode.NG_Get_EQP_Is_Null;//add for BMS Error Monitor
                            if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                            return false;
                        }

                        //20160520 是要先check Indexer(L2)的 ProductTypeCheckMode有沒開,不是EQP的
                        if (IndexerEQP.File.ProductTypeCheckMode == eEnableDisable.Enable)
                        {
                            strEqpProductType += string.Format("Stage{0} ProductType({1}), ", curStepStage.Data.STAGEID, stageEQP.File.ProductType);
                            if (curStepStage.File.CurStageStatus == eRobotStageStatus.RECEIVE_READY)
                            {
                                #region  [DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) CurStageStatus({4})!",
                                                                            curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.File.CurStageStatus);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                            }
                            else if (curStepStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY)
                            {
                                if (stageEQP.File.ProductType == curBcsJob.ProductType.Value)
                                {
                                    #region  [DebugLog]
                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) CurStageStatus({4}),EQP({5}) ProductType({6}),Job({7},{8}) ProductType({9} is Same) !",
                                                                                curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.File.CurStageStatus, stageEQP.Data.NODENO, stageEQP.File.ProductType, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.ProductType.Value);
                                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    curStageList.Remove(curStepStage);
                                    #region  [DebugLog]
                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) CurStageStatus({4}),EQP({5}) ProductType({6}),Job({7},{8}) ProductType({9} is not Same) !",
                                                                                curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.File.CurStageStatus, stageEQP.Data.NODENO, stageEQP.File.ProductType, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.ProductType.Value);
                                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }
                                    #endregion
                                }
                            }
                            else 
                            {
                                curStageList.Remove(curStepStage);
                                #region  [DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) CurStageStatus({4}),EQP({5}) ProductType({6}),Job({7},{8}) ProductType({9}) !",
                                                                            curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.File.CurStageStatus, stageEQP.Data.NODENO, stageEQP.File.ProductType, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.ProductType.Value);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) ProductTypeCheckMode is Disable!",
                                                                        curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                    }

                }
                #endregion
                if (curStageList.Count > 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Product Type Robot StageList is find!",
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO({5}) Filter Product Type Robot StageList is not find(please check 1.EQP(L2).File.ProductTypeCheckMode = Enable 2.Downstream EQP must have receive Job request 3.Upsteam EQP SendOut Job must => EQP.File.ProductType = Job.ProductType).",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    //20151120 add Rtn ErrMsg
                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) curProductType({5}) But {6} Filter Product Type Robot StageList is not find(please check 1. if EQP(L2).File.ProductTypeCheckMode = Enable And EQP.File.ProductType = Job.ProductType , Eqp Can Reciveable Job).",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString()
                                           , curBcsJob.ProductType.Value.ToString(), strEqpProductType);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ProductTypeCheck_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    //errCode = eJobFilter_ReturnCode.NG_ProductTypeCheck_Fail;//add for BMS Error Monitor
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
        /// 在step1 提前check ProductType
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("FL0054")]
        public bool Filter_Pre_FetchOutByProductType(IRobotContext robotConText)
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


                #region [ Get Current Next Step Entity ]
                if (!curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo))
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.NextStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get curRouteStep({3})!",
                                         MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                         curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.NextStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }


                RobotRouteStep curCheckRouteNextStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];

                //找不到 CurStep Route 回NG
                if (curCheckRouteNextStep == null)
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

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get curRouteStep({3})!",
                                         MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                         curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.NextStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }
                #endregion

                #region [ Get Current Stage List ]
                List<RobotStage> curNextStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.NextStepCanUseStageList]; ;

                if (curNextStepCanUseStageList == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepID({5}) StageIDList({6}) can not Find Stage Status is LDRQ!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurLocation_StageID, curCheckRouteNextStep.Data.STEPID.ToString(), curCheckRouteNextStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepID({4})  StageIDList({5}) can not Find Stage Status is LDRQ!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                curBcsJob.RobotWIP.CurLocation_StageID, curCheckRouteNextStep.Data.STEPID.ToString(), curCheckRouteNextStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #endregion

                #region [ Check Job Product Type]
                string strEqpProductType = "";//记录各设备ProductType
                if (curCheckRouteNextStep.Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT)
                {

                    string[] stageList = curCheckRouteNextStep.Data.STAGEIDLIST.Split(',');

                    //20160520
                    Equipment IndexerEQP = ObjectManager.EquipmentManager.GetEQP("L2");

                    for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                    {
                         RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);

                        #region [ 防呆 ]

                        if (curStepStage == null)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curCheckRouteNextStep.Data.STEPID, curCheckRouteNextStep.Data.STAGEIDLIST, stageList[stageIdx]);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            errMsg = string.Format("[{0}] can not Get RobotStageInfo by StageID({1}!",
                                MethodBase.GetCurrentMethod().Name, stageList[stageIdx]);

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_Stage_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            errCode = eJobFilter_ReturnCode.NG_Get_Stage_Is_Null;//add for BMS Error Monitor
                            if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                            return false;
                        }

                        #endregion
                        //判斷Current Step Stage 是否存在於Current LDRQ Stage List
                        if (curNextStepCanUseStageList.Contains(curStepStage) == false)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) NextStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5})!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curCheckRouteNextStep.Data.STEPID, curCheckRouteNextStep.Data.STAGEIDLIST, curStepStage.Data.STAGEID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            //不存在則視同判斷失敗直接記Log跳下一個Stage
                            continue;
                        }
                        Equipment stageEQP = ObjectManager.EquipmentManager.GetEQP(curStepStage.Data.NODENO);


                        if (stageEQP == null)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) can not find EQP by EQPNo({4})!",
                                                                        curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.Data.NODENO);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            errMsg = string.Format("[{0}] RobotStage({1}) StageNo({2}) can not find EQP by EQPNo({3})!",
                                                                        MethodBase.GetCurrentMethod().Name, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.Data.NODENO);
                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                            robotConText.SetReturnMessage(errMsg);
                            return false;
                        }

                        //20160520
                        if (IndexerEQP == null)
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Indexer EQP by EQPNo({2})!",
                                                                        IndexerEQP.Data.NODENO, curStepStage.Data.ROBOTNAME, IndexerEQP.Data.NODENO);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            errMsg = string.Format("[{0}] can not find Indexer EQP by EQPNo({1})!",
                                                                        MethodBase.GetCurrentMethod().Name, IndexerEQP.Data.NODENO);
                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                            robotConText.SetReturnMessage(errMsg);
                            return false;
                        }

                        //20160520 是要先check Indexer(L2)的 ProductTypeCheckMode有沒開,不是EQP的
                        if (IndexerEQP.File.ProductTypeCheckMode == eEnableDisable.Enable)
                        {
                            strEqpProductType += string.Format("Stage{0} ProductType({1}), ", curStepStage.Data.STAGEID, stageEQP.File.ProductType);
                            if (stageEQP.File.TotalTFTJobCount + stageEQP.File.TotalCFProductJobCount + stageEQP.File.TotalDummyJobCount
                                        + stageEQP.File.ThroughDummyJobCount + stageEQP.File.ThicknessDummyJobCount == 0)
                            {
                                #region  [DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) CurStageStatus({4})!",
                                                                            curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.File.CurStageStatus);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                            }
                            else 
                            {
                                if (stageEQP.File.ProductType == curBcsJob.ProductType.Value)
                                {
                                    #region  [DebugLog]
                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) CurStageStatus({4}),EQP({5}) ProductType({6}),Job({7},{8}) ProductType({9} is Same) !",
                                                                                curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.File.CurStageStatus, stageEQP.Data.NODENO, stageEQP.File.ProductType, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.ProductType.Value);
                                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    curNextStepCanUseStageList.Remove(curStepStage);
                                    #region  [DebugLog]
                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) CurStageStatus({4}),EQP({5}) ProductType({6}),Job({7},{8}) ProductType({9} is not Same) !",
                                                                                curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID, curStepStage.File.CurStageStatus, stageEQP.Data.NODENO, stageEQP.File.ProductType, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.ProductType.Value);
                                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }
                                    #endregion
                                }
                            }
                            
                        }
                        else
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) ProductTypeCheckMode is Disable!",
                                                                        curStepStage.Data.NODENO, curStepStage.Data.ROBOTNAME, curStepStage.Data.STAGENAME, curStepStage.Data.STAGEID);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                    }

                }
                #endregion
                if (curNextStepCanUseStageList.Count > 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Filter Product Type Robot StageList is find!",
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO({5}) Filter Product Type Robot StageList is not find(please check 1.EQP(L2).File.ProductTypeCheckMode = Enable 2.Downstream EQP must have receive Job request 3.Upsteam EQP SendOut Job must => EQP.File.ProductType = Job.ProductType).",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    //20151120 add Rtn ErrMsg
                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNO({4}) curProductType({5}) But {6} Filter Product Type Robot StageList is not find(please check 1. if EQP(L2).File.ProductTypeCheckMode = Enable And EQP.File.ProductType = Job.ProductType , Eqp Can Reciveable Job).",
                                           MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString()
                                           , curBcsJob.ProductType.Value.ToString(), strEqpProductType);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ProductTypeCheck_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_ProductTypeCheck_Fail;//add for BMS Error Monitor
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
    }
}
