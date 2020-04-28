using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class JobStageSelectService : AbstractRobotService
    {

        public override bool Init()
        {
            return true;
        }

//All Job Stage Select Function List [ Method Name = "StageSelect" + " _" +  "Condition Abbreviation" EX:"StageSelect_RecipeByPass" ]==============================================================

        //在Filter之前判斷目前Step對應的Stage List是否需要切換. (EX: Recipe By Pass需要換一下個合適的Step , Job 不符合條件不可進入目前Step需要跳到下一個合適的Step...)

        /// <summary>CheckStep Is Recipe By Pass //20151021 mark 移至RouteStepJump
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="newStepNo"></param>
        /// <returns></returns>
        //public bool StageSelect_ReceipeByPass(IRobotContext robotConText)
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

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_curRobot_Is_Null);
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
        //                                    curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name);

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_curBcsJob_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Check Robot Arm Type ]

        //        if (curRobot.Data.ARMJOBQTY != 1)
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
        //                                    curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ Get Current StageSelect Info Entity ]

        //        JobStageSelectInfo curJobStageSelectInfo = (JobStageSelectInfo)robotConText[eRobotContextParameter.StageSelectInfo];

        //        //找不到 Job StageSelectInfo 回NG
        //        if (curJobStageSelectInfo == null)
        //        {

        //            #region[DebugLog]
        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get JobStageSelectInfo!",
        //                                        "L1", MethodBase.GetCurrentMethod().Name);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }
        //            #endregion

        //            errMsg = string.Format("[{0}] can not Get JobStageSelectInfo!",
        //                                    MethodBase.GetCurrentMethod().Name);

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_curJobStageSelectInfo_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get Current Check Step Entity from Job StageSelectInfo afterStageSelect_Step ]

        //        RobotRouteStep curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curJobStageSelectInfo.AfterStageSelect_StepNo];

        //        //找不到 CurStep Route 回NG
        //        if (curCheckRouteStep == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get AfterStageSelect_StepNo({4})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.RobotWIP.CurStepNo.ToString());

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get AfterStageSelect_StepNo({4})!",
        //                                    curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurStepNo.ToString());

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_Job_Get_AfterStageSelectStep_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get cur StageSelect Can Use Stage List ]

        //        List<RobotStage> curStageSelectCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

        //        if (curStageSelectCanUseStageList == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find curStageSelectCanUseStageList!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.RobotWIP.CurLocation_StageID);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find curStageSelectCanUseStageList!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                                        curBcsJob.RobotWIP.CurLocation_StageID);

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ Get Current Check Step StageList ]

        //        List<RobotStage> curCheckStepStageList =new List<RobotStage>(); 
        //        string[] curCheckStepStageIDList = curCheckRouteStep.Data.STAGEIDLIST.Split(',');

        //        for (int i = 0; i < curCheckStepStageIDList.Length; i++)
        //        {

        //            #region [ Check Stage is Exist ]

        //            RobotStage curStage;

        //            curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curCheckStepStageIDList[i]);

        //            //找不到 Robot Stage 還是要繼續往下做
        //            if (curStage == null)
        //            {

        //                #region[DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
        //                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curCheckStepStageIDList[i]);

        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }

        //                #endregion

        //                return false;
        //            }

        //            //存在於curStageSelectCanUseStageList才可以加入判斷
        //            if (curStageSelectCanUseStageList.Contains(curStage) == true)
        //            {
        //                if(curCheckStepStageList.Contains(curStage)==false)
        //                {
        //                   curCheckStepStageList.Add(curStage);
        //                }
        //            }

        //            #endregion

        //        }

        //        //找不到任一個Stage則回覆異常
        //        if (curCheckStepStageList == null || curCheckStepStageList.Count == 0)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5})!",
        //                                    curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_Get_CheckStepStageList_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }              

        //        #endregion

        //        #region [ Check Step Active Must Is PUT ]

        //        if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
        //        {

        //            #region  [DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
        //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
        //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            //不影響curStageSelectCanUseStageList所以直接回傳True

        //            return true;

        //        }

        //        #endregion
                
        //        List<RobotStage> curRecipeNotByPassStageList=new List<RobotStage>(); 

        //        #region [ 如果有多個Stage 要通通by Pass才可以by Pass ]

        //        foreach (RobotStage curCheckStage in curCheckStepStageList)
        //        {

        //            if (CheckStageIsRecipeByPass(curCheckStage, curBcsJob) == false)
        //            {
        //                //Add to Not Recipe By Pass List
        //                if(curRecipeNotByPassStageList.Contains(curCheckStage)==false)
        //                {
        //                    curRecipeNotByPassStageList.Add(curCheckStage);
        //                }

        //            }

        //        }

        //        if(curRecipeNotByPassStageList.Count != 0)
        //        {

        //            //重新附值Can Use Stage
        //            curStageSelectCanUseStageList =curRecipeNotByPassStageList;
        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
        //            robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
        //            return true;
        //        }

        //        #endregion

        //        #region [ 找到下一個不是Recipe By Pass的PUT Step ]

        //        int newStepNo = GetNextPutStepStageList_IsNotRecipeByPass(curRobot, curBcsJob, curJobStageSelectInfo.AfterStageSelect_StepNo, curStageSelectCanUseStageList);

        //        if(newStepNo == 0)
        //        {
                    
        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) AfterStageSelect_StepNo({4}) is RecipeByPass But Other Step can not Get not Recipe By Pass Stage!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.RobotWIP.CurStepNo.ToString());

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) AfterStageSelect_StepNo({4}) is RecipeByPass But Other Step can not Get not Recipe By Pass Stage!",
        //                                    curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurStepNo.ToString());

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_Job_Get_AfterStageSelectStep_Fail);
        //            robotConText.SetReturnMessage(errMsg);
        //            return false;

        //        }

        //        #endregion

        //        //更新After StageSelect StepNo
        //        curJobStageSelectInfo.AfterStageSelect_StepNo = newStepNo;

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

        private int GetNextPutStepStageList_IsNotRecipeByPass(Robot curRobot, Job curBcsJob, int curAfterStageSelectStepNo, List<RobotStage> curStageSelectCanUseStageList)
        {

            int notRecipeByPassStepNo = 0;
            string strlog = string.Empty;

            try
            {

                for (int stepIdx = 0; stepIdx < curBcsJob.RobotWIP.RobotRouteStepList.Count; stepIdx++)
                {

                    if (curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.STEPID > curAfterStageSelectStepNo)
                    {

                        #region [ 下一個Step如果是PUT才需要判斷是否有Recipe by Pass,如果by Pass則繼續往下找尋非by Pass 的Stage ]

                        if (curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT)
                        {

                            #region [ Get Current Check Step StageList ]

                            List<RobotStage> curCheckStepStageList = new List<RobotStage>();
                            string[] curCheckStepStageIDList = curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.STAGEIDLIST.Split(',');

                            for (int stageIDIdx = 0; stageIDIdx < curCheckStepStageIDList.Length; stageIDIdx++)
                            {

                                #region [ Check Stage is Exist ]

                                RobotStage curStage;

                                curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curCheckStepStageIDList[stageIDIdx]);

                                //找不到 Robot Stage 表示有異常回復0 
                                if (curStage == null)
                                {

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curCheckStepStageIDList[stageIDIdx]);

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    return 0;
                                }

                                if (curCheckStepStageList.Contains(curStage) == false)
                                {
                                    curCheckStepStageList.Add(curStage);
                                }

                                #endregion

                            }

                            //找不到任一個Stage則回覆異常
                            if (curCheckStepStageList == null || curCheckStepStageList.Count == 0)
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5})!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.STEPID.ToString(), curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.STAGEIDLIST);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                return 0;

                            }

                            #endregion

                            List<RobotStage> curRecipeNotByPassStageList = new List<RobotStage>();

                            #region [ 如果有多個Stage 要通通by Pass才可以by Pass ]

                            foreach (RobotStage curCheckStage in curCheckStepStageList)
                            {

                                if (CheckStageIsRecipeByPass(curCheckStage, curBcsJob) == false)
                                {
                                    //Add to Not Recipe By Pass List
                                    if (curRecipeNotByPassStageList.Contains(curCheckStage) == false)
                                    {
                                        curRecipeNotByPassStageList.Add(curCheckStage);
                                    }

                                }

                            }

                            if (curRecipeNotByPassStageList.Count != 0)
                            {

                                //找到Recipe Not By Pass 重新附值Can Use Stage
                                curStageSelectCanUseStageList = curRecipeNotByPassStageList;
                                return curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.STEPID;
                            }

                            #endregion
                            
                        }

                        #endregion

                    }

                }

                return notRecipeByPassStepNo;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return 0;
            }

        }

        /// <summary> Check WIP in current Stage Recipe ID Is 00 (ByPass)
        ///
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

        //20151021 mark 移至RouteStepJump
        //public bool StageSelect_VCRDisable(IRobotContext robotConText)
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

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_curRobot_Is_Null);
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
        //                                    curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name);

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_curBcsJob_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Check Robot Arm Type ]

        //        if (curRobot.Data.ARMJOBQTY != 1)
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
        //                                    curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ Get Current StageSelect Info Entity ]

        //        JobStageSelectInfo curJobStageSelectInfo = (JobStageSelectInfo)robotConText[eRobotContextParameter.StageSelectInfo];

        //        //找不到 Job StageSelectInfo 回NG
        //        if (curJobStageSelectInfo == null)
        //        {

        //            #region[DebugLog]
        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get JobStageSelectInfo!",
        //                                        "L1", MethodBase.GetCurrentMethod().Name);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }
        //            #endregion

        //            errMsg = string.Format("[{0}] can not Get JobStageSelectInfo!",
        //                                    MethodBase.GetCurrentMethod().Name);

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_curJobStageSelectInfo_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get Current Check Step Entity from Job StageSelectInfo afterStageSelect_Step ]

        //        RobotRouteStep curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curJobStageSelectInfo.AfterStageSelect_StepNo];

        //        //找不到 CurStep Route 回NG
        //        if (curCheckRouteStep == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get AfterStageSelect_StepNo({4})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.RobotWIP.CurStepNo.ToString());

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get AfterStageSelect_StepNo({4})!",
        //                                    curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurStepNo.ToString());

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_Job_Get_AfterStageSelectStep_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get cur StageSelect Can Use Stage List ]

        //        List<RobotStage> curStageSelectCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

        //        if (curStageSelectCanUseStageList == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find curStageSelectCanUseStageList!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.RobotWIP.CurLocation_StageID);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StageIDList({6}) can not Find curStageSelectCanUseStageList!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                                        curBcsJob.RobotWIP.CurLocation_StageID);

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ Get Current Check Step StageList ]

        //        List<RobotStage> curCheckStepStageList = new List<RobotStage>();
        //        string[] curCheckStepStageIDList = curCheckRouteStep.Data.STAGEIDLIST.Split(',');

        //        for (int i = 0; i < curCheckStepStageIDList.Length; i++)
        //        {

        //            #region [ Check Stage is Exist ]

        //            RobotStage curStage;

        //            curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curCheckStepStageIDList[i]);

        //            //找不到 Robot Stage 還是要繼續往下做
        //            if (curStage == null)
        //            {

        //                #region[DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
        //                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curCheckStepStageIDList[i]);

        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }

        //                #endregion

        //                return false;
        //            }

        //            //存在於curStageSelectCanUseStageList才可以加入判斷
        //            if (curStageSelectCanUseStageList.Contains(curStage) == true)
        //            {
        //                if (curCheckStepStageList.Contains(curStage) == false)
        //                {
        //                    curCheckStepStageList.Add(curStage);
        //                }
        //            }

        //            #endregion

        //        }

        //        //找不到任一個Stage則回覆異常
        //        if (curCheckStepStageList == null || curCheckStepStageList.Count == 0)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5})!",
        //                                    curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_Get_CheckStepStageList_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Check Step Active Must Is PUT ]

        //        if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT)
        //        {

        //            #region  [DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Action({5}) is not PUT!",
        //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
        //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            //不影響curStageSelectCanUseStageList所以直接回傳True

        //            return true;

        //        }

        //        #endregion


        //        #region [ 找到下一個不是Recipe By Pass的PUT Step ]

        //        int newStepNo = GetNextPutStepStageList_IsVCRDisable(curRobot, curBcsJob, curJobStageSelectInfo.AfterStageSelect_StepNo, curStageSelectCanUseStageList);

        //        if (newStepNo == 0)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) AfterStageSelect_StepNo({4}) is VCR Enable!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.RobotWIP.CurStepNo.ToString());

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) AfterStageSelect_StepNo({4}) is VCR Enable!",
        //                                    curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurStepNo.ToString());

        //            robotConText.SetReturnCode(eJobStageSelect_ReturnCode.NG_Job_Get_AfterStageSelectStep_Fail);
        //            robotConText.SetReturnMessage(errMsg);
        //            return false;

        //        }

        //        #endregion

        //        //更新After StageSelect StepNo
        //        curJobStageSelectInfo.AfterStageSelect_StepNo = newStepNo;

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

        public bool CheckStageIsVCRDisable(RobotStage curStage, Job curJob)
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
                if (stageEQP.File.VcrMode == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3})can not find EQP VCRMode by EQPNo({4})!",
                                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGENAME, curStage.Data.STAGEID, curStage.Data.NODENO);
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
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageNo({2}) StageName({3}) VCR is Disable",
                                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME);
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
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageNo({2}) StageName({3}) VCR is Enable",
                                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME);
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

        private int GetNextPutStepStageList_IsVCRDisable(Robot curRobot, Job curBcsJob, int curAfterStageSelectStepNo, List<RobotStage> curStageSelectCanUseStageList)
        {

            int VCREnableStepNo = 0;
            string strlog = string.Empty;

            try
            {

                for (int stepIdx = 0; stepIdx < curBcsJob.RobotWIP.RobotRouteStepList.Count; stepIdx++)
                {

                    if (curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.STEPID > curAfterStageSelectStepNo)
                    {

                        #region [ 下一個Step如果是PUT才需要判斷是否有Recipe by Pass,如果by Pass則繼續往下找尋非by Pass 的Stage ]

                        if (curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT)
                        {

                            #region [ Get Current Check Step StageList ]

                            List<RobotStage> curCheckStepStageList = new List<RobotStage>();
                            string[] curCheckStepStageIDList = curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.STAGEIDLIST.Split(',');

                            for (int stageIDIdx = 0; stageIDIdx < curCheckStepStageIDList.Length; stageIDIdx++)
                            {

                                #region [ Check Stage is Exist ]

                                RobotStage curStage;

                                curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curCheckStepStageIDList[stageIDIdx]);

                                //找不到 Robot Stage 表示有異常回復0 
                                if (curStage == null)
                                {

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curCheckStepStageIDList[stageIDIdx]);

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    return 0;
                                }

                                if (curCheckStepStageList.Contains(curStage) == false)
                                {
                                    curCheckStepStageList.Add(curStage);
                                }

                                #endregion

                            }

                            //找不到任一個Stage則回覆異常
                            if (curCheckStepStageList == null || curCheckStepStageList.Count == 0)
                            {

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5})!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.STEPID.ToString(), curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.STAGEIDLIST);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                return 0;

                            }

                            #endregion

                            List<RobotStage> curVCRDisableStageList = new List<RobotStage>();

                            #region [ 如果有多個Stage 要通通by Pass才可以by Pass ]

                            foreach (RobotStage curCheckStage in curCheckStepStageList)
                            {

                                if (CheckStageIsVCRDisable(curCheckStage, curBcsJob) == true)
                                {
                                    //Add to VCR Disale List
                                    if (curVCRDisableStageList.Contains(curCheckStage) == false)
                                    {
                                        curVCRDisableStageList.Add(curCheckStage);
                                    }

                                }

                            }

                            if (curVCRDisableStageList.Count != 0)
                            {

                                //找到VCR Enable 重新附值Can Use Stage
                                curStageSelectCanUseStageList = curVCRDisableStageList;
                                return curBcsJob.RobotWIP.RobotRouteStepList[stepIdx].Data.STEPID;
                            }

                            #endregion

                        }

                        #endregion

                    }

                }

                return VCREnableStepNo;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return 0;
            }

        }

    }

}
