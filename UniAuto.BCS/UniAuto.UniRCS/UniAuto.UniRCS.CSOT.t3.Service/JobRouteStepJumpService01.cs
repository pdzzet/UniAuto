using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.MISC;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;


namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class JobRouteStepJumpService
    {

//RouteStepJump Funckey = "JP" + XXXX(序列號)

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0004")]
        public bool RouteStepJump_ForTTPDailyCheck(IRobotContext robotConText)
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
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot Line Entity ]
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

                #region [Check Daily Check Flag] 
                if (robotLine.Data.FABTYPE == eFabType.ARRAY.ToString())
                {
                    if (int.Parse(curBcsJob.CassetteSequenceNo) < eTTPDailCheckGlassCSTSEQ.Array_CSTSEQ)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}] < Daily check Glass Cassette Seq No=[{1}] Not Change Daily check route.",
                                curBcsJob.CassetteSequenceNo, eTTPDailCheckGlassCSTSEQ.Array_CSTSEQ.ToString(), curRobot.Data.NODENO));
                        }
                        #endregion
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                        return true;
                    }
                    else
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] is DailyCheck Glass!",
                                curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                        }
                        #endregion
                    }
                }

                if (robotLine.Data.FABTYPE == eFabType.CF.ToString())
                {
                    if (int.Parse(curBcsJob.CassetteSequenceNo) > eTTPDailCheckGlassCSTSEQ.CF_CSTSEQ_Max)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}] > Daily check Glass Cassette Seq No=[{1}] Not Change Daily check route.",
                                curBcsJob.CassetteSequenceNo, eTTPDailCheckGlassCSTSEQ.CF_CSTSEQ_Max.ToString(), curRobot.Data.NODENO));
                        }
                        #endregion
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                        return true;
                    }

                    if (int.Parse(curBcsJob.CassetteSequenceNo) < eTTPDailCheckGlassCSTSEQ.CF_CSTSEQ_Min)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}] < Daily check Glass Cassette Seq No=[{1}] Not Change Daily check route.",
                                curBcsJob.CassetteSequenceNo, eTTPDailCheckGlassCSTSEQ.CF_CSTSEQ_Max.ToString(), curRobot.Data.NODENO));
                        }
                        #endregion
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                        return true;
                    }

                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] is DailyCheck Glass!",
                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                    }
                    #endregion
                }

                if (Check_TTP_EQInterlock_DailyCheckBit(curRobot) == eBitResult.ON)  //表示DailyCheck
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] will Change Daily check Route, [EQUIPMENT={2}] DailyCheck Bit ON!",
                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                    }
                    #endregion
                }
                else
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}] ,JOB_SEQ_NO=[{1}] Daily Check Bit Not 'ON' ,not Change Daily Check Route.",
                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion


                //IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG, "EQPFlag");
                //if (subItem != null)
                //{
                //    #region GOTO SubChamber EQP Flag bit 'ON'
                //    if (subItem.ContainsKey("ToTotalPitchSubChamber"))
                //    {
                //        if (subItem["ToTotalPitchSubChamber"] == ((int)eBitResult.OFF).ToString())
                //        {
                //            robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                //            robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                //            return true;
                //        }
                //        else
                //        {
                //            #region  [DebugLog]
                //            if (IsShowDetialLog == true)
                //            {
                //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //                    string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] HAVE TTP DailyCheck, [EQUIPMENT={2}] ToTotalPitchSubChamber Bit ON!",
                //                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.CurrentEQPNo));
                //            }
                //            #endregion
                //        }
                //    }
                //    else
                //    {
                //        #region  [DebugLog]
                //        if (IsShowDetialLog == true)
                //        {
                //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //                string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] HAVE DailyCheckGlassUse, [EQUIPMENT={2}] CAN NOT FIND  TO ToTotalPitchSubChamber.",
                //                curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.CurrentEQPNo));
                //        }
                //        #endregion
                //        errMsg = string.Format("[{0}] Robot({1}) can not Find EQPFlag = ToTotalPitchSubChamber!", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name);
                //        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPFlag_SubItem_Is_Null);
                //        robotConText.SetReturnMessage(errMsg);
                //        return false;
                //    }
                //    #endregion
                //    #region 不使用 Goto TTP EQP Flag bit 'ON'
                //    //if (subItem.ContainsKey("ToTotalPitch(ForDailyCheckGlassUse)"))
                //    //{
                //    //    if (subItem["ToTotalPitch(ForDailyCheckGlassUse)"] == ((int)eBitResult.OFF).ToString())
                //    //    {
                //    //        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                //    //        robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                //    //        return true;
                //    //    }
                //    //    else
                //    //    {
                //    //        #region  [DebugLog]
                //    //        if (IsShowDetialLog == true)
                //    //        {
                //    //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //    //                string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] HAVE TTP DailyCheck, [EQUIPMENT={2}] ToTotalPitch(ForDailyCheckGlassUse). Bit ON!",
                //    //                curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.CurrentEQPNo));
                //    //        }
                //    //        #endregion
                //    //    }
                //    //}
                //    //else
                //    //{
                //    //    #region  [DebugLog]
                //    //    if (IsShowDetialLog == true)
                //    //    {
                //    //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //    //            string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] HAVE DailyCheckGlassUse, [EQUIPMENT={2}] CAN NOT FIND  TO ToTotalPitch(ForDailyCheckGlassUse).",
                //    //            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.CurrentEQPNo));
                //    //    }
                //    //    #endregion
                //    //    errMsg = string.Format("[{0}] Robot({1}) can not Find EQPFlag = ToTotalPitch(ForDailyCheckGlassUse)!", curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name);
                //    //    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPFlag_SubItem_Is_Null);
                //    //    robotConText.SetReturnMessage(errMsg);
                //    //    return false;
                //    //}
                //    #endregion
                //}


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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
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
                        errMsg = string.Format("[{0}]  Job({1}_{2}) can not get RouteStepEntity by curStepNo({3})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                #region [ Get TTP Daily Check GotoStepID ]

                int DailyCheckGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (DailyCheckGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Daily Check But GotoStepID({4}) is Fail!",
                                             MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheck_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(DailyCheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Daily Check But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheck_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Daily Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != DailyCheckGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is TTP Daily Check Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = DailyCheckGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is TTP Daily Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != DailyCheckGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = DailyCheckGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(DailyCheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                DailyCheckGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID];

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

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }


        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0007")]
        public bool RouteStepJump_ELABackUp(IRobotContext robotConText)
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
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check BackUp Mode ]
                string sLineBackUpMode;
                string trxName = "L2_LineBackupMode";
                Trx trxLineBackupMode = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                if (trxLineBackupMode == null)
                {
                    errMsg = string.Format("[{0}] can not Find Trasaction({1}) !", MethodBase.GetCurrentMethod().Name, trxName);
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                sLineBackUpMode = trxLineBackupMode.EventGroups[0].Events[0].Items[0].Value;
                if (sLineBackUpMode.Equals("0"))
                {
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
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
                                            curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                #region [ Get BackUp Mode GotoStepID ]

                int BackUpModeGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (BackUpModeGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is ELA BackUp Check But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELABackUp_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(BackUpModeGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is ELA BackUp Check But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is ELA BackUp Check But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELABackUp_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Daily Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != BackUpModeGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[BackUpModeGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is TTP Daily Check Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[BackUpModeGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = BackUpModeGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[BackUpModeGoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is TTP Daily Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELABackUp_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != BackUpModeGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = BackUpModeGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(BackUpModeGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                BackUpModeGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            BackUpModeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[BackUpModeGoToStepID];

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

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }


        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0008")]
        public bool RouteStepJump_ELABackUpNG_RTC(IRobotContext robotConText)
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
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check BackUp Mode & real RTC Flag on By Indexer & cst sequence ]
                Line line;
                bool line_Odd;
                string sLineBackUpMode;
                string realRTC;
                string trxName = "L2_LineBackupMode";

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

                Trx trxLineBackupMode = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                if (trxLineBackupMode == null)
                {
                    errMsg = string.Format("[{0}] can not Find Trasaction({1}) !", MethodBase.GetCurrentMethod().Name, trxName);
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                sLineBackUpMode = trxLineBackupMode.EventGroups[0].Events[0].Items[0].Value;
                if (!sLineBackUpMode.Equals("1")) //Not BackUp Mode, can't RTC at this step
                    return true;

                realRTC = curBcsJob.ArraySpecial.RtcFlag;
                if (!realRTC.Equals("1"))
                {
                    return true;
                }
                else
                {
                    if (line_Odd) 
                    {
                        if (int.Parse(curBcsJob.CassetteSequenceNo) > 4000) //Cross line glass, can't RTC
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (int.Parse(curBcsJob.CassetteSequenceNo) < 4000) //Cross line glass, can't RTC
                        {
                            return true;
                        }
                    }
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                #region [ Get BackUp Mode's RTC GotoStepID ]

                int BackUpModeGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (BackUpModeGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Daily Check But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELABackUp_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(BackUpModeGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Daily Check But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELABackUp_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Daily Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != BackUpModeGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[BackUpModeGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is TTP Daily Check Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[BackUpModeGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = BackUpModeGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[BackUpModeGoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is TTP Daily Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELABackUp_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != BackUpModeGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), BackUpModeGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = BackUpModeGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(BackUpModeGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                BackUpModeGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            BackUpModeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[BackUpModeGoToStepID];

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

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }


        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0009")]
        public bool RouteStepJump_ELAEQDown_RTC(IRobotContext robotConText)
        {
            
            string strlog = string.Empty;
            string errMsg = string.Empty;

            try
            {
                //no use(maybe link signal err, but indexer put to cst after err reset)
                return true;

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
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check EQ Down & cst sequence ]
                Line line;
                bool line_Odd;
                bool EQsDown = true;

                // if last stage send data is space, not need check Q-Time
                if (curBcsJob.RobotWIP.LastSendStageID.Trim().Length == 0)
                    return true;

                if (curBcsJob.ArraySpecial.ProcessType.ToUpper() != "NORMAL" && curBcsJob.ArraySpecial.ProcessType != "0")
                    return true; 

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

                List<RobotStage> curStageSelectCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                foreach (RobotStage curCheckStage in curStageSelectCanUseStageList)
                {
                    Equipment eq = ObjectManager.EquipmentManager.GetEQP(curCheckStage.Data.NODENO);
                    if (eq.File.EquipmentRunMode.ToUpper().Equals("NORMAL") && (eq.File.Status == eEQPStatus.RUN || eq.File.Status == eEQPStatus.IDLE))
                    {
                        EQsDown = false;
                    }
                }

                

                if (!EQsDown)
                {
                    return true;
                }
                else
                {
                    if (line_Odd)
                    {
                        if (int.Parse(curBcsJob.CassetteSequenceNo) > 4000) //Cross line glass, can't RTC
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (int.Parse(curBcsJob.CassetteSequenceNo) < 4000) //Cross line glass, can't RTC
                        {
                            return true;
                        }
                    }
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                #region [ Get EQ Down's RTC GotoStepID ]

                int EQDownGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (EQDownGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQDownGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is EQ Down RTC Check But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQDownGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELABackUp_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(EQDownGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is EQ Down RTC Check But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQDownGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is EQ Down RTC Check But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQDownGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELABackUp_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is EQ Down RTC Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQDownGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is EQ Down RTC Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQDownGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != EQDownGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[EQDownGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is EQ Down RTC Check Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQDownGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[EQDownGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = EQDownGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[EQDownGoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is EQ Down RTC Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQDownGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is EQ Down RTC Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQDownGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELABackUp_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != EQDownGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is EQ Down RTC Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQDownGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = EQDownGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(EQDownGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                EQDownGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            EQDownGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[EQDownGoToStepID];

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

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }


        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0010")]
        public bool RouteStepJump_ELAMQC(IRobotContext robotConText)
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
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check Job's MQC && Indexer's Stage status && L2 recipe ]
                string sStageRecv;
                string sStageExgPossible;
                string trxName = "L2_Stage#01SinglePositionReport";
                Trx trxStageStatus = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                if (trxStageStatus == null)
                {
                    errMsg = string.Format("[{0}] can not Find Trasaction({1}) !", MethodBase.GetCurrentMethod().Name, trxName);
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //add for X1 recipe, glass send to CST 2016/05/13 cc.kuang
                if (curBcsJob.PPID.Length > 2)
                {
                    if (curBcsJob.PPID[1] == '0')
                    {
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                        return true;
                    }
                }

                sStageRecv = trxStageStatus.EventGroups[0].Events[1].Items[0].Value;
                sStageExgPossible = trxStageStatus.EventGroups[0].Events[3].Items[0].Value;
                if (sStageRecv.Equals("0") && sStageExgPossible.Equals("0"))
                {
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }

                if (!curBcsJob.ArraySpecial.ProcessType.Equals("1"))
                {
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                #region [ Get MQC GotoStepID ]

                int MQCGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (MQCGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), MQCGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Daily Check But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), MQCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELAMQC_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(MQCGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), MQCGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Daily Check But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), MQCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELAMQC_GotoStepNo_Is_Fail); 
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), MQCGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Daily Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), MQCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != MQCGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[MQCGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is TTP Daily Check Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), MQCGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[MQCGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = MQCGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[MQCGoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), MQCGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is TTP Daily Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), MQCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELAMQC_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != MQCGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), MQCGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = MQCGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(MQCGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                MQCGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                             MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            MQCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[MQCGoToStepID];

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

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }

        //20151202 add FuncKey=>20160223 modify To JP0025
        //add by yang 2017/5/24
        //add : ELA cleaned glass temporal RTC
        //add : RTC Limit Count  , RTC Limit Count which glass can fetch out from cst 
        [UniAuto.UniBCS.OpiSpec.Help("JP0025")]
        public bool RouteStepJump_ELACleanOverQTime_RTC(IRobotContext robotConText)
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
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                if (curBcsJob.RobotWIP.EQPRTCFlag || curBcsJob.RobortRTCFlag) return true;   //avoid 2n check, timeout again &  can not fetch out from cst

                #region [ Check Clean Send out QTime ]
                Line line;
                bool line_Odd;

                // if last stage send data is space, not need check Q-Time
                if (curBcsJob.RobotWIP.LastSendStageID.Trim().Length == 0)
                    return true;

                double time;
                double overQTime_Default = 0;
                double overQTime_Recipe = 0;
                double overQTime; // = (double)ParameterManager["ELA_Clean_OverQTime"].GetInteger();
                double overQTimeWarring = 0;

                //get Max from QTime Def for L3 Send Event 
                /*
                foreach (QtimeEntityData L3SendEvent in ObjectManager.QtimeManager._entitiesDB.Values)
                {
                    if (L3SendEvent.ENABLED.Equals("Y") && L3SendEvent.STARTNODENO.Equals("L3") && L3SendEvent.STARTEVENTMSG.Trim().Equals("SEND")
                        && L3SendEvent.ENDNODENO.Equals("L3") && L3SendEvent.ENDEVENTMSG.Trim().Equals("SEND"))
                    {
                        //if (L3SendEvent.STARTNODERECIPEID.Trim().Length == 0)
                        //{
                        if (L3SendEvent.SETTIMEVALUE > (int)overQTime_Default)
                            {
                                overQTime_Default = L3SendEvent.SETTIMEVALUE;
                            }
                        //}
                    }
                }

                if (overQTime_Default > 0)
                    overQTime = overQTime_Default;

                if (overQTime < 1.00)
                    return true;
                */

                time = (DateTime.Now - Convert.ToDateTime(curBcsJob.RobotWIP.LastSendOnTime)).TotalSeconds;

                line = ObjectManager.LineManager.GetLines()[0];
                
                overQTime = line.File.SendOverTimeAlarm;
                overQTimeWarring = line.File.SendOverTimeWarring;
                if (overQTime < 1.00)
                    return true;

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

                if (overQTimeWarring > 0)
                {
                    if (time > overQTimeWarring && !curBcsJob.ArrayELAOverQtimeWarring)
                    {
                        curBcsJob.ArrayELAOverQtimeWarring = true;
                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { UtilityMethod.GetAgentTrackKey(), "L2", "Job(" + curBcsJob.CassetteSequenceNo + "," + 
                        curBcsJob.JobSequenceNo + ") QTime is Over Warring Time : " + overQTimeWarring, "BC" });
                    }
                }

                if (time < overQTime && curRobot.CLNRTCWIP == false)  //if has temporal RTC job, other jobs also need RTC ,by yang
                {
                    return true;
                }
                else
                {
                    if (line_Odd)
                    {
                        if (int.Parse(curBcsJob.CassetteSequenceNo) > 4000) //Cross line glass, can't RTC
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (int.Parse(curBcsJob.CassetteSequenceNo) < 4000) //Cross line glass, can't RTC
                        {
                            return true;
                        }
                    }
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                #region [ Get Clean Send Over QTime RTC GotoStepID ]

                int EQOverQTimeGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (EQOverQTimeGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Over Q-Time RTC Check But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQOverQTimeGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Over Q-Time RTC Check But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQOverQTimeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELAOverQTime_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(EQOverQTimeGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Over Q-Time RTC Check But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQOverQTimeGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Over Q-Time RTC Check But get GotoStepID({4}) Entity Fail",
                                             MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQOverQTimeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELAOverQTime_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Over Q-Time RTC Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQOverQTimeGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Over Q-Time RTC Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQOverQTimeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != EQOverQTimeGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[EQOverQTimeGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Over Q-Time RTC Check Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQOverQTimeGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[EQOverQTimeGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = EQOverQTimeGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[EQOverQTimeGoToStepID].Data.NEXTSTEPID;

                            if (!curBcsJob.ArrayELAOverQtimeFlag)
                            {
                                curBcsJob.ArrayELAOverQtimeFlag = true;

                                if (curBcsJob.RobotWIP.CurStepNo.ToString().Equals("81"))   //temporal EQPRTC,EQPRTC must step =81
                                {
                                    curBcsJob.RobotWIP.EQPRTCFlag = true;
                                    curBcsJob.RobortRTCFlag = true;
                                }
                                else
                                {
                                    HoldInfo hold = new HoldInfo()
                                    {
                                        NodeNo = "L1",
                                        NodeID = "BC",
                                        UnitNo = "0",
                                        UnitID = string.Empty,
                                        HoldReason = "Clean Over QTime " + overQTime,
                                        OperatorID = "BC",
                                    };
                                    ObjectManager.JobManager.HoldEventRecord(curBcsJob, hold);
                                }                            
                            }
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }
                else
                {
                    #region [ Update NextStepID by JumpGotoSTEPID Setting ]  [modify for temporal RTC] 

                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Over Q-Time RTC Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQOverQTimeGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is Over Q-Time RTC Check Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQOverQTimeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ELABackUp_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != EQOverQTimeGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Over Q-Time RTC Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQOverQTimeGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = EQOverQTimeGoToStepID;
                            if (!curBcsJob.ArrayELAOverQtimeFlag)
                            {
                                curBcsJob.ArrayELAOverQtimeFlag = true;

                                if (curBcsJob.RobotWIP.NextStepNo.ToString().Equals("81"))   //temporal EQPRTC,EQPRTC must step =81
                                {
                                    curBcsJob.RobotWIP.EQPRTCFlag = true;
                                    curBcsJob.RobortRTCFlag = true;
                                }
                                else
                                {
                                    HoldInfo hold = new HoldInfo()
                                    {
                                        NodeNo = "L1",
                                        NodeID = "BC",
                                        UnitNo = "0",
                                        UnitID = string.Empty,
                                        HoldReason = "Clean Over QTime " + overQTime,
                                        OperatorID = "BC",
                                    };
                                    ObjectManager.JobManager.HoldEventRecord(curBcsJob, hold);
                                }
                            }
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(EQOverQTimeGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                EQOverQTimeGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            EQOverQTimeGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[EQOverQTimeGoToStepID];

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

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }


        }

        [UniAuto.UniBCS.OpiSpec.Help("JP0019")]
        public bool RouteStepJump_ForTTPAgingEnable(IRobotContext robotConText)
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
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
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
                                            curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [Daily Check Mode don't Care Anging Enable/Disable Jumb Rule]
                if (Check_TTP_EQInterlock_DailyCheckBit(curRobot) == eBitResult.ON)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("Aging Enable Mode ,[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] Aging Enable, but Daily Check Mode not care  any Jump Rule. ",
                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }

                #endregion

                #region [ Check Aging Enable/Disable]
                Equipment eqp = GetTTPEQP();
                if (eqp.File.EquipmentRunMode == eTTPEQPRunMode.AGING_ENABLE)
                {
                      #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("Aging Enable Mode ,[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] Glass will go to Sub Chamber. ",
                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                    }
                    #endregion
                }
                else
                {
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                #region Check Tracking Data 是否已經去過Subchamber? 去過就不用再跳Step 2(Put Stage 11)

                #region [ Get Robot Line Entity ]
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

                if (robotLine.Data.FABTYPE == eFabType.ARRAY.ToString())
                {
                    #region [ Array TTP Decode Job TrackingData(代表進入TTP Sub Chamber這個Stage的TrackingData) ]
                    IDictionary<string, string> dicJobTrackingData = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData, eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

                    if (dicJobTrackingData == null)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by current Job Send out TrackingData({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.TrackingData);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        errMsg = string.Format("[{0}] Job({1}_{2}) can not Decode TrackingData Info by current Job  Send out TrackingData({3})!",
                                                curRobot.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                 curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                    #endregion

                    #region [ Array TTP Processed by Sub chamber (Tracing data)
                    if (dicJobTrackingData.ContainsKey("Unit#01-SubChamber"))
                    {
                        if (dicJobTrackingData["Unit#01-SubChamber"] != "0")
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass Tracking Data already processed by Sub Chamber. Don't Jump to SubChamber ",
                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                            }
                            #endregion
                            robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                            robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                            return true;
                        }
                    }
                    else
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass  Tracking Data No Sub Chamber define!!",
                                curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                        }
                        #endregion
                        errMsg = string.Format("[{0}] Tracking Data No Sub Chamber define[Unit#01-SubChamber]!!");
                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                    #endregion
                }
                else if (robotLine.Data.FABTYPE == eFabType.CF.ToString())
                {
                    if (curBcsJob.RobotWIP.CF_MQCTTP_SubChamberProcessedFlag)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass Tracking Data already processed by Sub Chamber. Don't Jump to SubChamber ",
                                curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                        }
                        #endregion
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                        return true;
                    }
                }

                #endregion

                #region [ Get TTP Daily Check GotoStepID ]

                int DailyCheckGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (DailyCheckGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Enable Mode But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheck_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(DailyCheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Daily Check But get GotoStepID({4}) Entity Fail",
                                             MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheck_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Aging Enable Mode Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is TTP Aging Enable Mode Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != DailyCheckGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Aging Enable Mode Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = DailyCheckGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is Aging Enable Mode Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != DailyCheckGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Aging Enable Mode Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = DailyCheckGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(DailyCheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                DailyCheckGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID];

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

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("JP0020")]
        public bool RouteStepJump_ForTTPAgingDisable(IRobotContext robotConText)
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
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                //Daily check Mode Priority is higher than Aging Enable or Aging Disable
                #region [Daily Check Mode don't Care Anging Enable/Disable Jumb Rule]
                if (Check_TTP_EQInterlock_DailyCheckBit(curRobot) == eBitResult.ON)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("Aging Enable Mode ,[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] Aging Enable, but Daily Check Mode not care  any Jump Rule. ",
                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }

                #endregion

                #region [ Check Aging Enable/Disable]
                Equipment eqp = GetTTPEQP();
                if (eqp.File.EquipmentRunMode == "")
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] Aging Mode not Report ,Aging Enable !!", curRobot.Data.NODENO));
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }


                if (eqp.File.EquipmentRunMode == eTTPEQPRunMode.AGING_DISABLE)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("Aging Disable Mode ,[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] Glass will go to TTP not go to Sub Chamber. ",
                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                    }
                    #endregion
                }
                else
                {
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_RobotArmType_IsNot_1Arm1Job);
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_CurStepRoute_Is_Fail);
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

                #region 目前無用 Check Tracking Data 是否已經去過Subchamber? 去過就不用再跳Step 2(Put Stage 11)

                //#region [ Decode Job TrackingData(代表進入TTP Sub Chamber這個Stage的TrackingData) ]
                //IDictionary<string, string> dicJobTrackingData = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData, eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

                //if (dicJobTrackingData == null)
                //{
                //    #region  [DebugLog]
                //    if (IsShowDetialLog == true)
                //    {
                //        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by current Job Send out TrackingData({4})!",
                //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                //                                curBcsJob.TrackingData);
                //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //    }
                //    #endregion
                //    errMsg = string.Format("[{0}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by current Job  Send out TrackingData({4})!",
                //                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                //                             curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData);

                //    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
                //    robotConText.SetReturnMessage(errMsg);
                //    return false;
                //}
                //#endregion

                //if (dicJobTrackingData.ContainsKey("Equipment-TotalPitch"))
                //{
                //    if (dicJobTrackingData["Equipment-TotalPitch"] != "0")
                //    {
                //        #region  [DebugLog]
                //        if (IsShowDetialLog == true)
                //        {
                //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //                string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass  Tracking Data already processed by TTP. Don't Jump to TTP ",
                //                curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                //        }
                //        #endregion
                //        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                //        robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                //        return true;
                //    }
                //}
                //else
                //{
                //    #region  [DebugLog]
                //    if (IsShowDetialLog == true)
                //    {
                //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                //            string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass Tracking Data No TotalPitch define!!",
                //            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                //    }
                //    #endregion
                //    errMsg = string.Format("[{0}] Tracking Data No Sub Chamber define[Equipment-TotalPitch]!!");
                //    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
                //    robotConText.SetReturnMessage(errMsg);
                //    return false;
                //}
                #endregion

                #region [ Get TTP Daily Check GotoStepID ]

                int DailyCheckGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (DailyCheckGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheck_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(DailyCheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheck_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != DailyCheckGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Aging Disable Mode Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = DailyCheckGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is TTP Daily Check Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is Aging Disable Mode Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != DailyCheckGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Aging Disable Mode Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), DailyCheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = DailyCheckGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(DailyCheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                DailyCheckGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            DailyCheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[DailyCheckGoToStepID];

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

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }


        }

        private Equipment GetTTPEQP()
        {
            try
            {
                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
                {
                    if (eqp.Data.NODENAME.ToUpper().IndexOf("TTP") >= 0)
                        return eqp;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
   
    }
}
