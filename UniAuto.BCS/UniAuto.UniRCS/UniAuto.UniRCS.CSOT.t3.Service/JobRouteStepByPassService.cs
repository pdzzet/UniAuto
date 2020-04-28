using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Core;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    [UniAuto.UniBCS.OpiSpec.Help("JobRouteStepByPassService")]
    public partial class JobRouteStepByPassService : AbstractRobotService
    {

        public override bool Init()
        {
            return true;
        }

//All Job Route Step ByPass Function List [ Method Name = "RouteStepByPass" + " _" +  "Condition Abbreviation" EX:"Filter_ForPVD" ]==============================================================
        //RouteStepByPass Funckey = "PS" + XXXX(序列號)

        /// <summary>CheckStep Is Recipe By Pass.If By Pass Change Job NextStepNo and Close This Robot Cycle
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="newStepNo"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("PS0001")]
        public bool RouteStepByPass_ReceipeByPass(IRobotContext robotConText)
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_curBcsJob_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

                RobotRouteStep curCheckRouteStep =null;

                if(is2ndCmdFlag ==false)
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

                    return false;

                }

                #endregion

                #region [ Get Current Check Step StageEntiy List ]

                List<RobotStage> curCheckStepStageList = new List<RobotStage>();
                string[] curCheckStepStageIDList = curCheckRouteStep.Data.STAGEIDLIST.Split(',');

                for (int i = 0; i < curCheckStepStageIDList.Length; i++)
                {

                    #region [ Check Stage is Exist ]

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

                    //存在於Common 的curStageSelectCanUseStageList才可以加入判斷L EX:預設3各Stage(Common).但是經過上一個ByPass後剩下2各(Common),那本次只要判斷Common這2各
                    if (curStageSelectCanUseStageList.Contains(curStage) == true)
                    {
                        if (curCheckStepStageList.Contains(curStage) == false)
                        {
                            curCheckStepStageList.Add(curStage);
                        }
                    }

                    #endregion

                }

                //找不到任一個符合的Stage則回覆異常
                if (curCheckStepStageList == null || curCheckStepStageList.Count == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Current Check Step({4}) Stage List({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not Get Current Check Step({3}) Stage List({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CheckStepStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check Step Active Must Is PUT ]

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

                    //不影響curStageSelectCanUseStageList所以直接回傳True

                    return true;

                }

                #endregion

                #region [ 如果有多個Stage 要通通by Pass才可以by Pass ]

                List<RobotStage> curRecipeNotByPassStageList = new List<RobotStage>();

                //20160525 找出目前所在的Stage
                //RobotStage curLocationStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curBcsJob.RobotWIP.CurLocation_StageID);

                foreach (RobotStage curCheckStage in curCheckStepStageList)
                {
                    if (CheckStageIsRecipeByPass(curCheckStage, curBcsJob, curRobot) == false)
                    {
                        //Add to Not Recipe By Pass List
                        if (curRecipeNotByPassStageList.Contains(curCheckStage) == false)
                        {
                            curRecipeNotByPassStageList.Add(curCheckStage);
                        }

                    }

                }

                if (curRecipeNotByPassStageList.Count != 0)//当前Step 有没有bypass的Stage 即不会Recipebypass 当前Step
                {

                    //重新附值Can Use Stage(Common) EX: 本次確認過程中有2各Stage但是其中一個設定PyPass .那最後符合條件的也剩下一個傳給下一個ByPass確認
                    curStageSelectCanUseStageList = curRecipeNotByPassStageList;//這裡只是兩個參數相等並沒有實際動到Context的東西要引用的值所以要重新add(如下)
                    robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curRecipeNotByPassStageList); //real modify StepCanUseStageList 2015/12/29 cc.kuang
                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }

                #endregion
                //走到此处即当前Step stage全部RecipeBypass
                #region [ Get Recipe Bypass GotoStepID ]

                int recipeByPassGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepByPassGotoStepNo];

                if (recipeByPassGoToStepID == 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass But GotoStepID({6}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) Stage List({4}) is RecipeByPass But GotoStepID({5}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(recipeByPassGoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass But GotoStepID({6}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) Stage List({4}) is RecipeByPass But GotoStepID({5}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ By Pass條件成立要回False 結束Job本Cycle的判斷.交由下一輪MainProcess來確認新Step的處理 ]

                if (is2ndCmdFlag == false)
                {
                    //注意!因為CurStepNo變動同時也要更新NextStepNo

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) Stage List({4}) is RecipeByPass to ({5})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != recipeByPassGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[recipeByPassGoToStepID].Data.NEXTSTEPID)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass to ({6}), NextStepNo({7}) to ({8})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID,
                                                curBcsJob.RobotWIP.NextStepNo.ToString(), curBcsJob.RobotWIP.RobotRouteStepList[recipeByPassGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = recipeByPassGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[recipeByPassGoToStepID].Data.NEXTSTEPID; //modify 2016/03/16 cc.kuang
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) Stage List({4}) is RecipeByPass to ({5})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != recipeByPassGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = recipeByPassGoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                }

                return false;

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }

        /// <summary> Check WIP in current Stage Recipe ID Is 00 (ByPass)
        ///
        /// </summary>
        /// <param name="curStage">Stage</param>
        /// <param name="curJob">Job</param>
        /// <returns>Recipe IS 00 By Pass</returns>
        private bool CheckStageIsRecipeByPass(RobotStage curStage, Job curJob, Robot curRobot)
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
                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                curNodePPID = curJob.PPID.Substring(stageEQP.Data.RECIPEIDX, stageEQP.Data.RECIPELEN);
                curByPassPPID = new string('0', stageEQP.Data.RECIPELEN);

                if (curNodePPID == curByPassPPID)
                {
                    // Daily Check的job,不可Recipe by pass,进Jump跳到daily check step
                    bool arrayDailyCheckJob = false; 
                    bool cfDailyCheckJob = false;
                    if (robotLine.Data.FABTYPE == eFabType.ARRAY.ToString() && int.Parse(curJob.CassetteSequenceNo) >= eTTPDailCheckGlassCSTSEQ.Array_CSTSEQ)
                        arrayDailyCheckJob = true;
                    if (robotLine.Data.FABTYPE == eFabType.CF.ToString() && eTTPDailCheckGlassCSTSEQ.CF_CSTSEQ_Min <= int.Parse(curJob.CassetteSequenceNo) && int.Parse(curJob.CassetteSequenceNo) <= eTTPDailCheckGlassCSTSEQ.CF_CSTSEQ_Max)
                        cfDailyCheckJob = true;
                    if (arrayDailyCheckJob || cfDailyCheckJob)
                    {
                        eBitResult eBit = Check_TTP_EQInterlock_DailyCheckBit(curRobot);
                        if (eBit == eBitResult.ON)  //有DailyCheck,不能Recipe by pass
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageNo({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) DailyCheck({5}),Recipe is not by Pass!",
                                                                        curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                                        curJob.CassetteSequenceNo, curJob.JobSequenceNo, eBit);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            return false;
                        }
                        else
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) StageName({2}) StageID({3}) RecipeIndex({4}) RecipeLen({5}) ,Job CassetteSequenceNo({6}) JobSequenceNo({7}) WIP PPID({8}) Stage PPID({9}) DailyCheck({10}), is by Pass!",
                                                                       curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGENAME, curStage.Data.STAGEID,
                                                                       stageEQP.Data.RECIPEIDX.ToString(), stageEQP.Data.RECIPELEN.ToString(), curJob.CassetteSequenceNo, curJob.JobSequenceNo, curJob.PPID, curNodePPID, eBit);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                            return true;
                        }
                    }
                    else
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
        //public bool RouteStepByPass_VCRDisable(IRobotContext robotConText)
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

        //            robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_curRobot_Is_Null);
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

        //            robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_curBcsJob_Is_Null);
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
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

        //            robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

        //            robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

        //        RobotRouteStep curCheckRouteStep = null;

        //        if (is2ndCmdFlag == false)
        //        {
        //            curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

        //            //找不到 CurStep Route 回NG
        //            if (curCheckRouteStep == null)
        //            {

        //                #region[DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by curStepNo({4})!",
        //                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                            curBcsJob.RobotWIP.CurStepNo.ToString());

        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }

        //                #endregion

        //                errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by curStepNo({4})!",
        //                                        MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.RobotWIP.CurStepNo.ToString());

        //                robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
        //                robotConText.SetReturnMessage(errMsg);

        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];

        //            //找不到 CurStep Route 回NG
        //            if (curCheckRouteStep == null)
        //            {

        //                #region[DebugLog]

        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by nextStepNo({4})!",
        //                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                            curBcsJob.RobotWIP.CurStepNo.ToString());

        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }

        //                #endregion

        //                errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by nextStepNo({4})!",
        //                                        MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.RobotWIP.CurStepNo.ToString());

        //                robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
        //                robotConText.SetReturnMessage(errMsg);

        //                return false;
        //            }

        //        }

        //        #endregion

        //        #region [ Get Comon Can Use Stage List ]

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

        //            robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_curStageSelectCanUseStageList_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ Get Current Check Step StageEntiy List ]

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

        //            //存在於Common 的curStageSelectCanUseStageList才可以加入判斷L EX:預設3各Stage(Common).但是經過上一個ByPass後剩下2各(Common),那本次只要判斷Common這2各
        //            if (curStageSelectCanUseStageList.Contains(curStage) == true)
        //            {
        //                if (curCheckStepStageList.Contains(curStage) == false)
        //                {
        //                    curCheckStepStageList.Add(curStage);
        //                }
        //            }

        //            #endregion

        //        }

        //        //找不到任一個符合的Stage則回覆異常
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
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST);

        //            robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CheckStepStageList_Is_Fail);
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

        //        #region [ 如果有多個Stage 要通通by Pass才可以by Pass ]

        //        List<RobotStage> curVCREnableStageList = new List<RobotStage>();

        //        foreach (RobotStage curCheckStage in curCheckStepStageList)
        //        {

        //            if (CheckStageIsVCRDisable(curCheckStage, curBcsJob) == false)
        //            {
        //                //Add to Not VCR Disable
        //                if (curVCREnableStageList.Contains(curCheckStage) == false)
        //                {
        //                    curVCREnableStageList.Add(curCheckStage);
        //                }

        //            }

        //        }

        //        if (curVCREnableStageList.Count != 0)
        //        {

        //            //重新附值Can Use Stage(Common) EX: 本次確認過程中有2各Stage但是其中一個設定PyPass .那最後符合條件的也剩下一個傳給下一個ByPass確認
        //            curStageSelectCanUseStageList = curVCREnableStageList;
        //            robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.Result_Is_OK);
        //            robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
        //            return true;
        //        }

        //        #endregion

        //        #region [ Get Recipe Bypass GotoStepID ]

        //        int vcrDisableByPassGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepByPassGotoStepNo];

        //        if (vcrDisableByPassGoToStepID == 0)
        //        {
        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass But GotoStepID({6}) is Fail!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, vcrDisableByPassGoToStepID.ToString());

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass But GotoStepID({6}) is Fail!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, vcrDisableByPassGoToStepID.ToString());

        //            robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepNo_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        //Get Change StepID 後的NextStepNO
        //        if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(vcrDisableByPassGoToStepID) == false)
        //        {
        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass But GotoStepID({6}) is Fail!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, vcrDisableByPassGoToStepID.ToString());

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass But GotoStepID({6}) is Fail!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, vcrDisableByPassGoToStepID.ToString());

        //            robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepNo_Is_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ By Pass條件成立要回False 結束Job本Cycle的判斷.交由下一輪MainProcess來確認新Step的處理 ]

        //        if (is2ndCmdFlag == false)
        //        {
        //            //注意!因為CurStepNo變動同時也要更新NextStepNo

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, vcrDisableByPassGoToStepID);

        //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, vcrDisableByPassGoToStepID);

        //            robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass);
        //            robotConText.SetReturnMessage(errMsg);

        //            //有變化才記Log並存檔
        //            if (curBcsJob.RobotWIP.CurStepNo != vcrDisableByPassGoToStepID ||
        //                curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[vcrDisableByPassGoToStepID].Data.NEXTSTEPID)
        //            {

        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass to ({6}), NextStepNo({7}) to ({8})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, vcrDisableByPassGoToStepID,
        //                                        curBcsJob.RobotWIP.NextStepNo.ToString(), curBcsJob.RobotWIP.RobotRouteStepList[vcrDisableByPassGoToStepID].Data.NEXTSTEPID.ToString());

        //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //                lock (curBcsJob)
        //                {
        //                    curBcsJob.RobotWIP.CurStepNo = vcrDisableByPassGoToStepID; ;
        //                }

        //                //Save File
        //                ObjectManager.JobManager.EnqueueSave(curBcsJob);

        //            }

        //        }
        //        else
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, vcrDisableByPassGoToStepID);

        //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, vcrDisableByPassGoToStepID);

        //            robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass);
        //            robotConText.SetReturnMessage(errMsg);

        //            //有變化才記Log並存檔
        //            if (curBcsJob.RobotWIP.NextStepNo != vcrDisableByPassGoToStepID)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, vcrDisableByPassGoToStepID);

        //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //                lock (curBcsJob)
        //                {
        //                    curBcsJob.RobotWIP.NextStepNo = vcrDisableByPassGoToStepID;
        //                }

        //                //Save File
        //                ObjectManager.JobManager.EnqueueSave(curBcsJob);

        //            }

        //        }

        //        return false;

        //        #endregion

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

        //        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Exception);
        //        robotConText.SetReturnMessage(ex.Message);

        //        return false;
        //    }

        //}

        private bool CheckStageIsVCRDisable(RobotStage curStage, Job curJob)
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("PS0002")]
        public bool RouteStepByPass_ReceipeByPass2ndClean(IRobotContext robotConText)
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_curBcsJob_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

                #region [ Check Step Active Must Is PUT ]

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

                    //不影響curStageSelectCanUseStageList所以直接回傳True

                    return true;

                }

                #endregion

                List<RobotStage> lstRBStage = ObjectManager.RobotStageManager.GetRobotStages();
                int iLineRecipeLength = 0;
                foreach (RobotStage curCheckStage in lstRBStage)
                {
                    Equipment stageEQP = ObjectManager.EquipmentManager.GetEQP(curCheckStage.Data.NODENO);

                    if (stageEQP == null)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3})can not find EQP by EQPNo({4})!",
                                                                    curCheckStage.Data.NODENO, curCheckStage.Data.ROBOTNAME, curCheckStage.Data.STAGENAME, curCheckStage.Data.STAGEID, curCheckStage.Data.NODENO);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            return false;
                        }
                        #endregion
                        return false;
                    }
                    else
                    {
                        if ((stageEQP.Data.RECIPEIDX + stageEQP.Data.RECIPELEN) > iLineRecipeLength)
                            iLineRecipeLength = stageEQP.Data.RECIPEIDX + stageEQP.Data.RECIPELEN;
                    }
                }

                if (iLineRecipeLength == 0)
                {
                    strlog = string.Format("[BCS <- RBM] Can not find Total Recipe Length!");
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return false;
                }

                string Clean2ndRecipe = curBcsJob.PPID.Substring(iLineRecipeLength, 4);
                if ((iLineRecipeLength + 4) > curBcsJob.PPID.Length)
                {
                    strlog = string.Format("[BCS <- RBM] Job({0},{1}) Total Recipe Length({2}) < Clean2ndRecipe Index+4({3})!", curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.PPID.Length, (iLineRecipeLength + 4));
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return false;
                }

                if (!Clean2ndRecipe.Equals("0000"))
                {
                    return true;
                }

                #region [ Get Recipe Bypass GotoStepID ]

                int recipeByPassGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepByPassGotoStepNo];

                if (recipeByPassGoToStepID == 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass But GotoStepID({6}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) Stage List({4}) is RecipeByPass But GotoStepID({5}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(recipeByPassGoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass But GotoStepID({6}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) Stage List({4}) is RecipeByPass But GotoStepID({5}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ By Pass條件成立要回False 結束Job本Cycle的判斷.交由下一輪MainProcess來確認新Step的處理 ]

                if (is2ndCmdFlag == false)
                {
                    //注意!因為CurStepNo變動同時也要更新NextStepNo

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) Stage List({4}) is RecipeByPass to ({5})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != recipeByPassGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[recipeByPassGoToStepID].Data.NEXTSTEPID)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass to ({6}), NextStepNo({7}) to ({8})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID,
                                                curBcsJob.RobotWIP.NextStepNo.ToString(), curBcsJob.RobotWIP.RobotRouteStepList[recipeByPassGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = recipeByPassGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[recipeByPassGoToStepID].Data.NEXTSTEPID; //modify 2016/03/16 cc.kuang
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) Stage List({4}) is RecipeByPass to ({5})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != recipeByPassGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, recipeByPassGoToStepID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = recipeByPassGoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                }
                #endregion

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }


        /// <summary>
        /// <br>CFMQC-TTP line</br>
        /// <br>bypass TTP buffer if TTP is aging disable mode or TTP recipeid is full of 0</br>
        /// </summary>
        /// <param name="robotConText">robot context object</param>
        /// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("PS0003")]
        public bool RouteStepByPass_TTPBufferBypass(IRobotContext robotConText)
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_curBcsJob_Is_Null);
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

                    errMsg = string.Format("[{0}]Arm Job Qty({1}) is illegal!",
                                            MethodBase.GetCurrentMethod().Name,curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

                #region [ Check Step Active Must Is PUT ]

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

                    //不影響curStageSelectCanUseStageList所以直接回傳True

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
                #region [* 檢查Bypass條件:TTP aging disable or TTP recipe is full of 0]
                bool bypass = false;
                foreach (RobotStage curCheckStage in curStageSelectCanUseStageList)
                {
                    Equipment stageEQP = ObjectManager.EquipmentManager.GetEQP(curCheckStage.Data.NODENO);
                    if (stageEQP == null)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3})can not find EQP by EQPNo({4})!",
                                                                    curCheckStage.Data.NODENO, curCheckStage.Data.ROBOTNAME, curCheckStage.Data.STAGENAME, curCheckStage.Data.STAGEID, curCheckStage.Data.NODENO);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            return false;
                        }
                        #endregion
                        return false;
                    }

                    //check eq runmode is aging enable
                    if (stageEQP.File.EquipmentRunMode != eTTPEQPRunMode.AGING_ENABLE)
                    {
                        bypass = true;
                        break;
                    }

                    //check eq recipeid is full of 0
                    string curNodePPID = curBcsJob.PPID.Substring(stageEQP.Data.RECIPEIDX, stageEQP.Data.RECIPELEN);
                    string curByPassPPID = new string('0', stageEQP.Data.RECIPELEN);
                    if (curNodePPID == curByPassPPID)
                    {
                        bypass = true;
                        break;
                    }

                }
                if (!bypass)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) TTP aging enalbe and TTP recipe is not full of 0!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.ROBOTACTION);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                    return true;
                }
                #endregion

                #region [ Get Bypass GotoStepID ]

                int ByPassGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepByPassGotoStepNo];

                if (ByPassGoToStepID == 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is TTPBufferByPass But GotoStepID({6}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ByPassGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) Stage List({4}) is TTPBufferByPass But GotoStepID({5}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ByPassGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(ByPassGoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is TTPBufferByPass But GotoStepID({6}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ByPassGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) Stage List({4}) is TTPBufferByPass But GotoStepID({5}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ByPassGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ By Pass條件成立要回False 結束Job本Cycle的判斷.交由下一輪MainProcess來確認新Step的處理 ]

                if (is2ndCmdFlag == false)
                {
                    //注意!因為CurStepNo變動同時也要更新NextStepNo

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is RecipeByPass to ({6})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ByPassGoToStepID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) Stage List({4}) is TTPBufferByPass to ({5})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ByPassGoToStepID);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != ByPassGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[ByPassGoToStepID].Data.NEXTSTEPID)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) Stage List({5}) is TTPBufferByPass to ({6}), NextStepNo({7}) to ({8})!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ByPassGoToStepID,
                                            curBcsJob.RobotWIP.NextStepNo.ToString(), curBcsJob.RobotWIP.RobotRouteStepList[ByPassGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = ByPassGoToStepID; ;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) Stage List({5}) is TTPBufferByPass to ({6})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ByPassGoToStepID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) Stage List({4}) is TTPBufferByPass to ({5})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ByPassGoToStepID);

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != ByPassGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) Stage List({5}) is TTPBufferByPass to ({6})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), curCheckRouteStep.Data.STAGEIDLIST, ByPassGoToStepID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = ByPassGoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

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

        //20160525 從EQP RTC回插回CST的Job,再次出片到要去的StepNo
        [UniAuto.UniBCS.OpiSpec.Help("PS0004")]
        public bool RouteStepByPass_RTC_Job_ReSendoutToEQP(IRobotContext robotConText)
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_Get_curRobot_Line_Is_Fail);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_curBcsJob_Is_Null);
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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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
                #region [ Check TempStepNo 與目前的要Put的Step是否一致,一樣就Jump]
                if (is2ndCmdFlag == false)
                {
                    if (curBcsJob.RobotWIP.TempStepNo == curBcsJob.RobotWIP.CurStepNo && curBcsJob.RobotWIP.EQPRTCFlag)
                    {
                        //curBcsJob.RobotWIP.EQPRTCFlag = false;
                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobRouteStepByPass_ReturnMessage.OK_Message);
                        return true;
                    }
                    else if (curBcsJob.RobotWIP.TempStepNo != curBcsJob.RobotWIP.CurStepNo && !curBcsJob.RobotWIP.EQPRTCFlag)
                    {
                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobRouteStepByPass_ReturnMessage.OK_Message);
                        return true;
                    }
                }
                else
                {
                    if (curBcsJob.RobotWIP.TempStepNo == curBcsJob.RobotWIP.NextStepNo && curBcsJob.RobotWIP.EQPRTCFlag)
                    {
                        //curBcsJob.RobotWIP.EQPRTCFlag = false;
                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobRouteStepByPass_ReturnMessage.OK_Message);
                        return true;
                    }
                    else if (curBcsJob.RobotWIP.TempStepNo != curBcsJob.RobotWIP.NextStepNo && !curBcsJob.RobotWIP.EQPRTCFlag)
                    {
                        robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobRouteStepByPass_ReturnMessage.OK_Message);
                        return true;
                    }
                }
                #endregion

                #region [ Get GotoStepID ]

                int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepByPassGotoStepNo];

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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepRoute_Is_Fail);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepRoute_Is_Fail);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepRoute_Is_Fail);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepRoute_Is_Fail);
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

                    robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_RecipeIsByPass_GotoStepRoute_Is_Fail);
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

                return false;  //為了連跳,所以不true,避免往下跑到filter
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepByPass_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }
    }
}
