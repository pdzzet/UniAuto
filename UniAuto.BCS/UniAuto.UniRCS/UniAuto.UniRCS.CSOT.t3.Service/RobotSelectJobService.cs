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
using System.Threading; //Add By Yangzhenteng 2019102217

namespace UniAuto.UniRCS.CSOT.t3.Service
{

    /// <summary>
    /// Must add to Config\Objects\Business.xml
    /// </summary>
    [UniAuto.UniBCS.OpiSpec.Help("RobotSelectJobService")]
    public partial class RobotSelectJobService : AbstractRobotService
    {

        public override bool Init()
        {
            return true;
        }

        //All Robot Select Stage Job Condition Function List [ Method Name = "Select" + "_" + 對象Stage + "_" + 狀態(Job or Stage Status) ]======================================================================================================================================
        //Select Funckey = "SL" + XXXX(序列號)

        #region [ 20151109 Mark_004 目前沒用到 ]

        /// <summary> Select All Stage Can Control Job List by One Command control One Arm(One Job)
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //public bool Select_AllStage_For1Cmd_1Arm_1Job(IRobotContext robotConText)
        //{


        //    try
        //    {
        //        //[ Wait_Proc_0009 ] 後續處理
                
        //        robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.Result_Is_OK);
        //        robotConText.SetReturnMessage(eRobotSelectJob_ReturnMessage.OK_Message);

        //        #region for Test

        //        //List<string> testCanControlJobList = new List<string>();

        //        //testCanControlJobList.Add("1_1");
        //        //testCanControlJobList.Add("1_2");
        //        //testCanControlJobList.Add("1_3");

        //        //List<string> curtestCanControlJobList;
        //        //curtestCanControlJobList = (List<string>)robotConText[eRobotContextParameter.StageTestStringList];

        //        //if (curtestCanControlJobList == null)
        //        //{
        //        //    curtestCanControlJobList = new List<string>();
        //        //}

        //        //curtestCanControlJobList.AddRange(testCanControlJobList);

        //        //robotConText.AddParameter(eRobotContextParameter.StageTestStringList, curtestCanControlJobList);

        //        #endregion

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {

        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

        //        robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Exception);
        //        robotConText.SetReturnMessage(ex.Message);

        //        return false;
        //    }

        //}

        #endregion

        /// <summary> Select All Robot Arm Can Control Job List by One Command control One Arm(One Job)
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("SL0001")]
        public bool Select_AllRobotArm_for1Cmd_1Arm_1Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<Job> robotArmCanControlJobList=new List<Job>() ;

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

                #region [ By ArmNo Get Can Control Job List ] 20151208 mark old

                //for (int armNoIndex = 0; armNoIndex < curRobot.File.ArmSignalSubstrateInfoList.Length; armNoIndex++)
                //{

                //    if (curRobot.File.ArmSignalSubstrateInfoList[armNoIndex].ArmJobExist == eGlassExist.Exist)
                //    {

                //        #region [ Get cur Job Entity ]

                //        Job curArmJob = ObjectManager.JobManager.GetJob(curRobot.File.ArmSignalSubstrateInfoList[armNoIndex].ArmCSTSeq, curRobot.File.ArmSignalSubstrateInfoList[armNoIndex].ArmJobSeq);

                //        //找不到 BcsJob 回NG
                //        if (curArmJob == null)
                //        {

                //            #region[DebugLog]

                //            if (IsShowDetialLog == true)
                //            {

                //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob by Arm CSTSeq({2}),JobSeq({3})!",
                //                                        "L1", MethodBase.GetCurrentMethod().Name, curRobot.File.ArmSignalSubstrateInfoList[armNoIndex].ArmCSTSeq, curRobot.File.ArmSignalSubstrateInfoList[armNoIndex].ArmJobSeq);

                //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //            }

                //            #endregion

                //            errMsg = string.Format("[{0}] can not Get BcsJob by Arm CSTSeq({1}),JobSeq({2})!",
                //                                    MethodBase.GetCurrentMethod().Name, 
                //                                    curRobot.File.ArmSignalSubstrateInfoList[armNoIndex].ArmCSTSeq, curRobot.File.ArmSignalSubstrateInfoList[armNoIndex].ArmJobSeq);

                //            robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_curBcsJob_Is_Null);
                //            robotConText.SetReturnMessage(errMsg);

                //            return false;
                //        }

                //        #endregion

                //        #region [ 有變化才 Update JOB WIP Location Info ]

                //        if (curArmJob.RobotWIP.CurLocation_StageID != eRobotCommonConst.ROBOT_HOME_STAGEID || 
                //            curArmJob.RobotWIP.CurLocation_SlotNo != (armNoIndex + 1) ||
                //            curArmJob.RobotWIP.CurLocation_StageType != eRobotStageType.ROBOTARM)
                //        {

                //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ArmNo({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) Update RobotJobWIP curLocation StageID from ({5}) to ({6}) ,StageType for ({7}) to ({8}) ,SlotNo from ({9}) to ({10}).",
                //                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, (armNoIndex + 1).ToString(), curArmJob.CassetteSequenceNo,
                //                                                    curArmJob.JobSequenceNo, curArmJob.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID, curArmJob.RobotWIP.CurLocation_StageType,
                //                                                    eRobotStageType.ROBOTARM, curArmJob.RobotWIP.CurLocation_SlotNo, (armNoIndex + 1).ToString());
                //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                //            lock (curArmJob)
                //            {

                //                curArmJob.RobotWIP.CurLocation_StageID = eRobotCommonConst.ROBOT_HOME_STAGEID;
                //                curArmJob.RobotWIP.CurLocation_SlotNo = (armNoIndex + 1);
                //                curArmJob.RobotWIP.CurLocation_StageType = eRobotStageType.ROBOTARM;
                //            }

                //            //Save File
                //            ObjectManager.JobManager.EnqueueSave(curArmJob);

                //        }

                //        #endregion

                //        //add To Arm CanControl Job List
                //        robotArmCanControlJobList.Add(curArmJob);

                //    }

                //}

                #endregion

                #region [ 20151208 addBy RealTime ArmInfo list ArmNo Get Can Control Job List ]

                for (int armNoIndex = 0; armNoIndex < curRobot.CurTempArmSingleJobInfoList.Length; armNoIndex++)
                {

                    if (curRobot.CurTempArmSingleJobInfoList[armNoIndex].ArmJobExist == eGlassExist.Exist)
                    {

                        #region [ Get cur Job Entity ]

                        Job curArmJob = ObjectManager.JobManager.GetJob(curRobot.CurTempArmSingleJobInfoList[armNoIndex].ArmCSTSeq, curRobot.CurTempArmSingleJobInfoList[armNoIndex].ArmJobSeq);

                        //找不到 BcsJob 回NG
                        if (curArmJob == null)
                        {

                            //20160603
                            #region [當curBcsJob找不到時,必須再判斷是不是AbnormalForceOut,產生新Job]
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


                                return false;
                            }

                            #endregion

                            if (robotLine.Data.FABTYPE == eFabType.ARRAY.ToString() && robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.ABNORMAL_FORCE_CLEAN_OUT_MODE)
                            {
                                //沒有帳,產生一片新Job帳
                                object[] parameters = null;
                                parameters = new object[] { robotLine.Data.LINEID, curRobot.CurTempArmSingleJobInfoList[armNoIndex].ArmCSTSeq, curRobot.CurTempArmSingleJobInfoList[armNoIndex].ArmJobSeq, null };
                                curArmJob = (Job)Invoke(eServiceName.JobService, "NewJob", parameters, new Type[] { typeof(string), typeof(string), typeof(string), typeof(Trx) });
                                if (curArmJob == null)
                                {
                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("StageID(00) Can not Get Job by CSTSeq({0}) JobSeq({1})!",
                                                                curRobot.CurTempArmSingleJobInfoList[armNoIndex].ArmCSTSeq, curRobot.CurTempArmSingleJobInfoList[armNoIndex].ArmJobSeq);
                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    return false;
                                }
                                else
                                {
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                                    object[] _parameters = null;
                                    bool result = false;
                                    _parameters = new object[] { eqp.Data.NODENO, curArmJob, 2, 65535, errMsg };
                                    result = (bool)Invoke(eServiceName.RobotCoreService, "CreateAbnormalForceCleanOutJobRobotWIPInfo", _parameters,
                                              new Type[] { typeof(string), typeof(Job), typeof(int), typeof(int), typeof(string).MakeByRefType() });
                                    if(!result)
                                    {
                                        return false;
                                    }
                                }
                            }
                            #endregion

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob by Arm CSTSeq({2}),JobSeq({3})!",
                                                        "L1", MethodBase.GetCurrentMethod().Name, curRobot.CurTempArmSingleJobInfoList[armNoIndex].ArmCSTSeq, curRobot.CurTempArmSingleJobInfoList[armNoIndex].ArmJobSeq);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("[{0}] can not Get BcsJob by Arm CSTSeq({1}),JobSeq({2})!",
                                                    MethodBase.GetCurrentMethod().Name,
                                                    curRobot.CurTempArmSingleJobInfoList[armNoIndex].ArmCSTSeq, curRobot.CurTempArmSingleJobInfoList[armNoIndex].ArmJobSeq);

                            robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_curBcsJob_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion

                        #region [ 有變化才 Update JOB WIP Location Info ]
                       
                        if (curArmJob.RobotWIP.CurLocation_StageID != eRobotCommonConst.ROBOT_HOME_STAGEID ||
                            curArmJob.RobotWIP.CurLocation_SlotNo != (armNoIndex + 1) ||
                            curArmJob.RobotWIP.CurLocation_StageType != eRobotStageType.ROBOTARM)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ArmNo({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) Update RobotJobWIP curLocation StageID from ({5}) to ({6}) ,StageType for ({7}) to ({8}) ,SlotNo from ({9}) to ({10}).",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, (armNoIndex + 1).ToString(), curArmJob.CassetteSequenceNo,
                                                                    curArmJob.JobSequenceNo, curArmJob.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID, curArmJob.RobotWIP.CurLocation_StageType,
                                                                    eRobotStageType.ROBOTARM, curArmJob.RobotWIP.CurLocation_SlotNo, (armNoIndex + 1).ToString());
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            lock (curArmJob)
                            {

                                curArmJob.RobotWIP.CurLocation_StageID = eRobotCommonConst.ROBOT_HOME_STAGEID;
                                curArmJob.RobotWIP.CurLocation_SlotNo = (armNoIndex + 1);
                                curArmJob.RobotWIP.CurLocation_StageType = eRobotStageType.ROBOTARM;
                            }

                            //Save File
                            ObjectManager.JobManager.EnqueueSave(curArmJob);

                        }

                        #endregion

                        #region [ //20151209 add Check Froce Return CST Without LDRQ Status ]

                        if (curArmJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status == eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_READY)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ArmNo({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) Update RobotJobWIP FroceReturnCSTWithoutLDRQ_Status from ({5}) to ({6}).",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, (armNoIndex + 1).ToString(), curArmJob.CassetteSequenceNo,
                                                                    curArmJob.JobSequenceNo, curArmJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status, eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_START);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            lock (curArmJob)
                            {
                                curArmJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_START;
                                curArmJob.RobotWIP.ForceReturnCSTWithoutLDRQ_MonitorStartTime = DateTime.Now;

                            }

                            //Save File
                            ObjectManager.JobManager.EnqueueSave(curArmJob);

                        }

                        #endregion

                        #region[add CVD Check Force Return CST Without LDRQ Status][逻辑暂时不用pending]
                        //CVD arm上基板 stay timeout,要RTC,防止锁住RB  Yang 20161002
                        /*
                        if (curRobot.Data.LINETYPE.Contains("CVD")&&curArmJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status == eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_NOTCHECK)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ArmNo({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) Update RobotJobWIP FroceReturnCSTWithoutLDRQ_Status from ({5}) to ({6}).",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, (armNoIndex + 1).ToString(), curArmJob.CassetteSequenceNo,
                                                                    curArmJob.JobSequenceNo, curArmJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status, eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_START);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            lock (curArmJob)
                            {
                                curArmJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_START;
                                curArmJob.RobotWIP.ForceReturnCSTWithoutLDRQ_MonitorStartTime = DateTime.Now;

                            }

                            //Save File
                            ObjectManager.JobManager.EnqueueSave(curArmJob);

                        }
                        */
                        #endregion

                        //add To Arm CanControl Job List
                        robotArmCanControlJobList.Add(curArmJob);

                    }

                }

                #endregion


                robotConText.AddParameter(eRobotContextParameter.ArmCanControlJobList, robotArmCanControlJobList);
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

        /// <summary> Select All Port Type Stage Can Control Job List by One Command control One Arm(One Job)
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("SL0002")]
        public bool Select_PortTypeStage_For1Cmd_1Arm_1Job(IRobotContext robotConText)
        {

            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curRobotStageList = null;
            List<Job> robotStageCanControlJobList;

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

                #region [ Get Current Stage Can Control Job List ]

                robotStageCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.StageCanControlJobList];

                //當沒有值時則要建立
                if (robotStageCanControlJobList == null)
                {
                    robotStageCanControlJobList = new List<Job>();
                }

                #endregion

                
                foreach (RobotStage stage_entity in curRobotStageList)
                {

                    //非Port Type Stage則不判斷
                    if (stage_entity.Data.STAGETYPE.ToUpper().Trim() != eRobotStageType.PORT)
                    {
                        continue;
                    }

                    Get_AllPortTypeStage_CanControlJobList_For1Cmd_1Arm_1Job(curRobot, stage_entity, robotStageCanControlJobList);

                    //20160511 將可控Job,依照RobotStage(要出片的CST),把Job塞到AllJobRecipeGroupNoList,便於之後邏輯判斷用
                    foreach (Job job in robotStageCanControlJobList)
                    {
                        if (job.SourcePortID == stage_entity.Data.STAGEID && job.SamplingSlotFlag == "1" && job.RobotWIP.CurStepNo == 1)
                            stage_entity.AllJobRecipeGroupNoList.Add(job);
                    }
                 

                    CheckJobEachCassetteSlotPositionBlock(stage_entity);
                }

                ////只要有CST 处于Waiting fot MES Reply FirstGlassCheck 状态，就不出片 Modified by Zhangwei 20161104
                //foreach (RobotStage stage_entity in curRobotStageList)
                //{
                //    if (stage_entity.File.CstFirstGlassCheckResult == "C2")
                //        return false;
                //}



                robotConText.AddParameter(eRobotContextParameter.StageCanControlJobList, robotStageCanControlJobList);
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

        private void CheckJobEachCassetteSlotPositionBlock(RobotStage curStage)
        {
            try
            {
                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotPositionBlock
                string strlog = string.Empty;
                string trxID = GetTrx_CSTSlotJobEachPosition(curStage.Data.NODENO, curStage.Data.STAGEID);

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
                    curStage.PortCassetteEmpty = RobotStage.PORTCSTEMPTY.ERROR;
                    curStage.UnloaderSamplingFlag = RobotStage.UNLOADER_SAMPLING_FLAG.ERROR;
                    return;
                }
                #region Trx
                //<TrxBody name="L2_Port#01JobEachCassetteSlotPositionBlock">
                //  <EventGroup name="L2_EG_Port#01JobEachCassetteSlotPositionBlock">
                //    <Event name="L2_W_Port#01JobEachCassetteSlotPositionBlock" device="W" address="0001445" points="56" ENABLE="1">
                //      <Item name="SlotPosition#001CassetteSequenceNo" type="INT" points="1" offset="0" VALUE="0" />
                //      <Item name="SlotPosition#001JobSequenceNo" type="INT" points="1" offset="1" VALUE="0" />
                //      <Item name="SlotPosition#002CassetteSequenceNo" type="INT" points="1" offset="2" VALUE="0" />
                //      <Item name="SlotPosition#002JobSequenceNo" type="INT" points="1" offset="3" VALUE="0" />
                #endregion
                curStage.PortCassetteEmpty = RobotStage.PORTCSTEMPTY.EMPTY;
                curStage.UnloaderSamplingFlag = RobotStage.UNLOADER_SAMPLING_FLAG.EMPTY;
                for (int slot_no = 1; slot_no <= get_CSTSlot_JobInfo_Trx.EventGroups[0].Events[0].Items.Count / 2; slot_no++)
                {
                    string cst_seq_no = get_CSTSlot_JobInfo_Trx.EventGroups[0].Events[0].Items[(slot_no - 1) * 2].Value;
                    string job_seq_no = get_CSTSlot_JobInfo_Trx.EventGroups[0].Events[0].Items[(slot_no - 1) * 2 + 1].Value;
                    if (cst_seq_no != "0" || job_seq_no != "0")
                    {
                        Job job = ObjectManager.JobManager.GetJob(cst_seq_no, job_seq_no);
                        if (job != null)
                        {
                            curStage.PortCassetteEmpty = RobotStage.PORTCSTEMPTY.NOT_EMPTY;

                            if (job.SamplingSlotFlag == "1") curStage.UnloaderSamplingFlag = RobotStage.UNLOADER_SAMPLING_FLAG.SAMPLING_FLAG_ON;
                            else curStage.UnloaderSamplingFlag = RobotStage.UNLOADER_SAMPLING_FLAG.SAMPLING_FLAG_OFF;
                        }
                        else
                        {
                            curStage.PortCassetteEmpty = RobotStage.PORTCSTEMPTY.ERROR;
                            curStage.UnloaderSamplingFlag = RobotStage.UNLOADER_SAMPLING_FLAG.ERROR;
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("Trx({0}) Slot({1}) Job({2}_{3}) is not Exist in JobManager", get_CSTSlot_JobInfo_Trx.Name, slot_no, cst_seq_no, job_seq_no);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                curStage.PortCassetteEmpty = RobotStage.PORTCSTEMPTY.ERROR;
                curStage.UnloaderSamplingFlag = RobotStage.UNLOADER_SAMPLING_FLAG.ERROR;
            }
        }

        /// <summary> Select All EQP Type Stage Can Control Job List by One Command control One Arm(One Job)
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("SL0003")]
        public bool Select_EQPTypeStage_For1Cmd_1Arm_1Job(IRobotContext robotConText)
        {

            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curRobotStageList = null;
            List<Job> robotStageCanControlJobList;

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

                #region [ Get Current Stage Can Control Job List ]

                robotStageCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.StageCanControlJobList];

                //當沒有值時則要建立
                if (robotStageCanControlJobList == null)
                {
                    robotStageCanControlJobList = new List<Job>();
                }

                #endregion

                foreach (RobotStage stage_entity in curRobotStageList)
                {

                    //非EQP Type Stage則不判斷
                    if (stage_entity.Data.STAGETYPE.ToUpper().Trim() != eRobotStageType.EQUIPMENT) continue;

                    //20151024 add for Get Mulit Slot EQP Type Stage Status
                    //Get_EqpTypeStageStatus_For1Arm1Job(curRobot, stage_entity, robotStageCanControlJobList);
                    Get_EqpTypeStageStatus(curRobot, stage_entity, robotStageCanControlJobList);
                }

                //[ Wait_Proc_0009 ] 後續處理
                robotConText.AddParameter(eRobotContextParameter.StageCanControlJobList, robotStageCanControlJobList);
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

        [UniAuto.UniBCS.OpiSpec.Help("SL0004")]
        public bool Select_StageTypeStage_For1Cmd_1Arm_1Job(IRobotContext robotConText)
        {

            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curRobotStageList = null;
            List<Job> robotStageCanControlJobList;

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

                #region [ Get Current Stage Can Control Job List ]

                robotStageCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.StageCanControlJobList];

                //當沒有值時則要建立
                if (robotStageCanControlJobList == null)
                {
                    robotStageCanControlJobList = new List<Job>();
                }

                #endregion

                foreach (RobotStage stage_entity in curRobotStageList)
                {

                    //非Stage Type Stage則不判斷
                    if (stage_entity.Data.STAGETYPE.ToUpper().Trim() != eRobotStageType.STAGE) continue;

                    Get_StageTypeStageStatus(curRobot, stage_entity, robotStageCanControlJobList);
                }

                //[ Wait_Proc_0009 ] 後續處理
                robotConText.AddParameter(eRobotContextParameter.StageCanControlJobList, robotStageCanControlJobList);
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

        #region [ Get Port Type Can Control Joblist Function List ===========================================================================================================================

        private void ReadSlotInfo(RobotStage curStage)
        {
            try
            {
                string strName = string.Format("{0}_Port#{1}CassetteStatusChangeReport", curStage.Data.NODENO, curStage.Data.STAGEIDBYNODE);
                Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName, false }) as Trx;
                curStage.PortSlotInfos.Clear();
                for (int i = 0; i < trx.EventGroups[0].Events[0].Items.Count; i += 2)
                {
                    RobotStage_PortSlotInfo slot_info = new RobotStage_PortSlotInfo();
                    slot_info.slotCSTSeq = trx.EventGroups[0].Events[0].Items[i].Value;
                    slot_info.slotJobSeq = trx.EventGroups[0].Events[0].Items[i + 1].Value;
                    slot_info.slotGlassExist = (slot_info.slotCSTSeq == "0" || slot_info.slotJobSeq == "0") ? "0" : "1";
                    curStage.PortSlotInfos.Add(slot_info);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> Get PortType Stage Can Control Joblist ,for Normal Robot(Upper and Lower Arm),One Cmd One Arm One Job Use 
        ///
        /// </summary>
        private void Get_AllPortTypeStage_CanControlJobList_For1Cmd_1Arm_1Job(Robot curRobot, RobotStage curStage, List<Job> curCanCtlJobList)
        {
            string tmpStageStatus = string.Empty;
            string strlog = string.Empty;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;

            try
            {
                
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                #region [ Get Port Entity by StageID , 如果找不到則 Stage Status =NOREQ ]

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

                    UpdateStageStatus(curStage, tmpStageStatus,MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                    return;
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

                        //strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Port Enable Mode is ({4})!",
                        //                        curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                        //                        curPort.File.EnableMode);

                        //Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

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
                        else if (curPort.File.Type == ePortType.BothPort)
                        {
                            if (CheckCSTHasJobUDRQ(curStage) || CheckEmptySlotCanLDRQ(curStage))
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Port Enable Mode is ({4})!",
                        curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                        curPort.File.EnableMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        #endregion

                    }

                    #endregion

                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00013 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                }
                #endregion

                #region [ Check Port Down Status Is Normal(not down) ]
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
                        else if (curPort.File.Type == ePortType.BothPort)
                        {
                            if (CheckCSTHasJobUDRQ(curStage) || CheckEmptySlotCanLDRQ(curStage))
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

                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00014 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

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
                    return;
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
                        if (robotLine.Data.FABTYPE == eFabType.CF.ToString() &&
                            (robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.THROUGH_MODE || robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.FIX_MODE || robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.RANDOM_MODE
                            //20160525
                            || ((Workbench.LineType == eLineType.CF.FCMQC_TYPE1 || Workbench.LineType == eLineType.CF.FCMQC_TYPE2) && (robotLine.File.IndexOperMode != eINDEXER_OPERATION_MODE.SORTER_MODE && robotLine.File.IndexOperMode != eINDEXER_OPERATION_MODE.CHANGER_MODE))))
                        {
                            //For Both Port Get Can control Joblist
                            //Step01 Check UDRQ Status
                            if (Get_LoaderPort_CanControlJoblist_CFThroughMode(curRobot, curStage, curCanCtlJobList, curPort))
                            {
                                //Step02 Check LDRQ Status
                                Get_UnloadPort_StageStatueInfo(curRobot, curStage, curPort);
                            }
                        }
                        else
                        {
                            //Step01 Loader Port Get Can control Joblist
                            Get_LoaderPort_CanControlJoblist(curRobot, curStage, curCanCtlJobList, curPort);

                            if (robotLine.Data.FABTYPE == eFabType.CF.ToString() &&
                                robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE &&
                                StaticContext.ContainsKey(eRobotContextParameter.SorterMode_RobotParam))
                            {
                                SorterMode_RobotParam srt_param = (SorterMode_RobotParam)StaticContext[eRobotContextParameter.SorterMode_RobotParam];
                                if (curCanCtlJobList.Count > 0)
                                {
                                    Dictionary<string, List<Job>> grade_job_list = new Dictionary<string, List<Job>>();//依Grade分JobList
                                    #region 依Grade分JobList
                                    foreach (Job job in curCanCtlJobList)
                                    {
                                        if (grade_job_list.ContainsKey(job.JobGrade))
                                        {
                                            grade_job_list[job.JobGrade].Add(job);
                                        }
                                        else
                                        {
                                            List<Job> jobs = new List<Job>();
                                            jobs.Add(job);
                                            grade_job_list.Add(job.JobGrade, jobs);
                                        }
                                    }
                                    #endregion
                                    List<Job> first_priority = null;
                                    if (grade_job_list.ContainsKey(srt_param.LastGrade))
                                    {
                                        first_priority = grade_job_list[srt_param.LastGrade];//與上次相同Grade的Job優先, 若沒有"上次"則預設以OK優先
                                        grade_job_list.Remove(srt_param.LastGrade);
                                    }
                                    curCanCtlJobList.Clear();
                                    if (first_priority != null)
                                        curCanCtlJobList.AddRange(first_priority);//與上次相同Grade的Job優先, 若沒有"上次"則預設以OK優先
                                    foreach (string grade in grade_job_list.Keys)
                                    {
                                        curCanCtlJobList.AddRange(grade_job_list[grade]);//將其他Grade加入, curCanCtlJobList會依Grade排序, 相同Grade則以Slot排序(EqpRptCSTFetchSeqMode)
                                    }
                                }
                                //curCanCtlJobList
                            }
                        }
                    }
                    else if (curPort.File.Type == ePortType.UnloadingPort)
                    {
                        //For Unload Port Get Can control Joblist
                        Get_UnloadPort_StageStatueInfo(curRobot, curStage, curPort);
                    }
                    else if (curPort.File.Type == ePortType.BothPort)
                    {
                        //For Both Port Get Can control Joblist
                        //Step01 Check UDRQ Status
                        Get_BothPort_CanControlJoblist(curRobot, curStage, curCanCtlJobList, curPort);

                        //Step02 Check LDRQ Status
                        Get_BothPort_StageLDRQStatueInfo(curRobot, curStage, curPort);

                        //Step03 Judge Stage Status
                        JudgePortStage_UDRQ_LDRQStatus(curStage, curPort);
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

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00023 ]

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
                        else if (curPort.File.Type == ePortType.BothPort)
                        {
                            if (CheckCSTHasJobUDRQ(curStage) || CheckEmptySlotCanLDRQ(curStage))
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

                    #endregion

                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                    #endregion

                }

                #endregion               

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        #region [ For Loader Port Use Function ] ===============================================================================================================================

        /// <summary> 確認Loader Port Slot是否有JOB存在並加入到Can Control List
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curCanCtlJobList"></param>
        /// <param name="curPort"></param>
        private void Get_LoaderPort_CanControlJoblist(Robot curRobot, RobotStage curStage, List<Job> curCanCtlJobList, Port curPort)
        {

            string tmpStageStatus = string.Empty;
            string trxID = string.Empty;
            string strlog = string.Empty;
            string checkReasonCode = string.Empty;
            bool hasJobUDRQFlag = false;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;

            try
            {

                //預設為NoReq
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                #region [ 20151029 add Aborting不可出片只可收片 ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00013 ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_FETCHPORT_BUT_STATUS_IS_ABORTING, curPort.Data.PORTID);

                if (curPort.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                {
                    //Port Status Aborting 不能取片(UDRQ)則需更新Stage Status並紀錄Log
                    UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

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

                #region [ Check FirstGlsssCheck, Fail表示不可出片 ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00009 ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_FETCHPORT_WAIT_FIRSTGLASSCHECK, curPort.Data.PORTID);

                if (CheckFirstGlassCheckCondition(curRobot, curPort, curStage) == false)
                {
                    //Port上CST 尚未做完FirstGlass Check.不可以取片(UDRQ)則需更新Stage Status並紀錄Log
                    UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

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
                trxID = GetTrx_CSTSlotJobExistInfo(curStage.Data.NODENO, curStage.Data.STAGEID);

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

                    UpdateStageStatus(curStage, tmpStageStatus,MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

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

                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                    return;

                }

                #endregion

                //根據Robot設定的取片順序來決定要從Slot01開始抽還是SlotMax開始抽 ASC 从小到大(Priority 01>02>03>…) ,DEC 从大到小 (Priority 01<02<03>…), default : ASC 从小到大 
                //20151125 for CSOT要求以EQ上報為準 
                //if (curRobot.Data.SLOTFETCHSEQ == eRobotCommonConst.DB_ORDER_BY_ASC)
                if (curRobot.EqpRptCSTFetchSeqMode == eRobotCommonConst.DB_ORDER_BY_ASC)
                {

                    #region [ 抽片順序為ASC SlotNo由小到大 ]

                    for (int i = 1; i <= curStage.Data.SLOTMAXCOUNT; i++)
                    {
                        checkReasonCode = Get_CSTSlot_CanControlJoblist(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList, curPort, curRobot);

                        #region [ 根據Port CST Type來決定是否要判斷下一片 ]

                        if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                        {

                            #region [ 非循序CST的處理,可跳Slot ]

                            switch (checkReasonCode)
                            {
                                case ePortJobUDRQReason.REASON_OK:

                                    //已在Get_CSTSlot_CanControlJoblist更新過Stage Status 所以直接Return即可 20150922 mark for Get Port All UDRQ Job List
                                    hasJobUDRQFlag = true;
                                    break;
                                //return;

                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

                                    //確認是空Slot則判斷下一個Slot
                                    break;

                                default:

                                    //其他錯誤是為找不到 則跳下一個Slot判斷
                                    break;
                            }

                            #endregion

                        }
                        else
                        {

                            #region [ 循序CST的處理,不可跳Slot ]

                            switch (checkReasonCode)
                            {
                                case ePortJobUDRQReason.REASON_OK:

                                    //已在Get_CSTSlot_CanControlJoblist更新過Stage Status 所以直接Return即可
                                    return;

                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

                                    //確認是空Slot則判斷下一個Slot
                                    break;

                                default:

                                    //其他錯誤視為找不到或是帳料有問題需更新Stage Status並紀錄Log
                                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                                    return;
                            }

                            #endregion

                        }

                        #endregion

                    }

                    //有其中一個Slot UDRQ則為True
                    if (hasJobUDRQFlag == false)
                    {
                        //Stage內最後的Slot為空or有其他錯誤視為找不到or帳料有問題,則需更新Stage Status並紀錄Log
                        UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                    }

                    #endregion

                }
                else
                {

                    #region [ 抽片順序為DEC SlotNo由大到小 ]

                    for (int i = curStage.Data.SLOTMAXCOUNT; i > 0; i--)
                    {
                        checkReasonCode = Get_CSTSlot_CanControlJoblist(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList, curPort, curRobot);

                        #region [ 根據Port CST Type來決定是否要判斷下一片 ]

                        if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                        {

                            #region [ 非循序CST的處理,可跳Slot ]

                            switch (checkReasonCode)
                            {
                                case ePortJobUDRQReason.REASON_OK:

                                    //已在Get_CSTSlot_CanControlJoblist更新過Stage Status 所以直接Return即可 20150922 mark for Get Port All UDRQ Job List
                                    hasJobUDRQFlag = true;
                                    break;
                                //return;

                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

                                    //確認是空Slot則判斷下一個Slot
                                    break;

                                default:

                                    //其他錯誤是為找不到 則跳下一個Slot判斷
                                    break;
                            }

                            #endregion

                        }
                        else
                        {

                            #region [ 循序CST的處理,不可跳Slot ]

                            switch (checkReasonCode)
                            {
                                case ePortJobUDRQReason.REASON_OK:

                                    //已在Get_CSTSlot_CanControlJoblist更新過Stage Status 所以直接Return即可
                                    return;

                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

                                    //確認是空Slot則判斷下一個Slot
                                    break;

                                default:

                                    //其他錯誤視為找不到或是帳料有問題需更新Stage Status並紀錄Log
                                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                                    return;
                            }

                            #endregion

                        }

                        #endregion

                    }

                    //有其中一個Slot UDRQ則為True
                    if (hasJobUDRQFlag == false)
                    {
                        //Stage內最後的Slot為空or有其他錯誤視為找不到or帳料有問題,則需更新Stage Status並紀錄Log
                        UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                    }

                    #endregion

                }

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        /// <summary> 確認Loader Port Slot是否有JOB存在並加入到Can Control List; true:需要繼續找LDRQ Slot
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curCanCtlJobList"></param>
        /// <param name="curPort"></param>
        /// <returns>true:沒有錯誤卻被設定為NO_REQUEST, 需要繼續找LDRQ Slot; false:因為錯誤被設定為NO_REQUEST, 或者不是NO_REQUEST</returns>
        private bool Get_LoaderPort_CanControlJoblist_CFThroughMode(Robot curRobot, RobotStage curStage, List<Job> curCanCtlJobList, Port curPort)
        {
            string trxID = string.Empty;
            string strlog = string.Empty;
            string checkReasonCode = string.Empty;
            bool hasJobUDRQFlag = false;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;

            try
            {
                #region [ 20151029 add Aborting不可出片只可收片 ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00013 ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_FETCHPORT_BUT_STATUS_IS_ABORTING, curPort.Data.PORTID);

                if (curPort.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                {
                    //Port Status Aborting 不能取片(UDRQ)則需更新Stage Status並紀錄Log
                    UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

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

                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00013 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                #region [ Check FirstGlsssCheck, Fail表示不可出片 ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00009 ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_FETCHPORT_WAIT_FIRSTGLASSCHECK, curPort.Data.PORTID);

                if (CheckFirstGlassCheckCondition(curRobot, curPort, curStage) == false)
                {
                    //Port上CST 尚未做完FirstGlass Check.不可以取片(UDRQ)則需更新Stage Status並紀錄Log
                    UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

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

                    return false;
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
                trxID = GetTrx_CSTSlotJobExistInfo(curStage.Data.NODENO, curStage.Data.STAGEID);

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

                    UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                    return false;

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

                    UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                    return false;

                }

                #endregion

                //根據Robot設定的取片順序來決定要從Slot01開始抽還是SlotMax開始抽 ASC 从小到大(Priority 01>02>03>…) ,DEC 从大到小 (Priority 01<02<03>…), default : ASC 从小到大 
                //20151125 for CSOT要求以EQ上報為準 
                //if (curRobot.Data.SLOTFETCHSEQ == eRobotCommonConst.DB_ORDER_BY_ASC)
                if (curRobot.EqpRptCSTFetchSeqMode == eRobotCommonConst.DB_ORDER_BY_ASC)
                {

                    #region [ 抽片順序為ASC SlotNo由小到大 ]

                    for (int i = 1; i <= curStage.Data.SLOTMAXCOUNT; i++)
                    {
                        checkReasonCode = Get_CSTSlot_CanControlJoblist(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList, curPort, curRobot);

                        #region [ 根據Port CST Type來決定是否要判斷下一片 ]

                        if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                        {
                            #region [ 非循序CST的處理,可跳Slot ]

                            switch (checkReasonCode)
                            {
                            case ePortJobUDRQReason.REASON_OK:

                                //已在Get_CSTSlot_CanControlJoblist更新過Stage Status 所以直接Return即可 20150922 mark for Get Port All UDRQ Job List
                                hasJobUDRQFlag = true;
                                break;
                            //return;

                            case ePortJobUDRQReason.IS_EMPTY_SLOT:

                                //確認是空Slot則判斷下一個Slot
                                break;

                            default:

                                //其他錯誤是為找不到 則跳下一個Slot判斷
                                break;
                            }

                            #endregion
                        }
                        else
                        {
                            UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                            return false;// CF Through Mode 不能是 SeqCST, return false 不要繼續找 LDRQ Slot
                        }

                        #endregion

                    }

                    //有其中一個Slot UDRQ則為True
                    if (hasJobUDRQFlag == false)
                    {
                        //Stage內最後的Slot為空or有其他錯誤視為找不到or帳料有問題,則需更新Stage Status並紀錄Log
                        //UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                        return true;// CF Through Mode Loader 找不到出片, return true 繼續找 LDRQ Slot
                    }

                    #endregion

                }
                else
                {

                    #region [ 抽片順序為DEC SlotNo由大到小 ]

                    for (int i = curStage.Data.SLOTMAXCOUNT; i > 0; i--)
                    {
                        checkReasonCode = Get_CSTSlot_CanControlJoblist(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList, curPort, curRobot);

                        #region [ 根據Port CST Type來決定是否要判斷下一片 ]

                        if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                        {
                            #region [ 非循序CST的處理,可跳Slot ]

                            switch (checkReasonCode)
                            {
                            case ePortJobUDRQReason.REASON_OK:

                                //已在Get_CSTSlot_CanControlJoblist更新過Stage Status 所以直接Return即可 20150922 mark for Get Port All UDRQ Job List
                                hasJobUDRQFlag = true;
                                break;
                            //return;

                            case ePortJobUDRQReason.IS_EMPTY_SLOT:

                                //確認是空Slot則判斷下一個Slot
                                break;

                            default:

                                //其他錯誤是為找不到 則跳下一個Slot判斷
                                break;
                            }

                            #endregion
                        }
                        else
                        {
                            UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                            return false;// CF Through Mode 不能是 SeqCST, return false 不要繼續找 LDRQ Slot
                        }

                        #endregion

                    }

                    //有其中一個Slot UDRQ則為True
                    if (hasJobUDRQFlag == false)
                    {
                        //Stage內最後的Slot為空or有其他錯誤視為找不到or帳料有問題,則需更新Stage Status並紀錄Log
                        //UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                        return true;// CF Through Mode Loader 找不到出片, return true 繼續找 LDRQ Slot
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return false;
        }

        /// <summary> 取得Loader Port中存在且有帳的Job and Add to Can Control Job
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
        private string Get_CSTSlot_CanControlJoblist(RobotStage curStage, Trx trxExistInfo, Trx trxJobInfo, int slotKey, List<Job> curCanControlJobList, Port curPort, Robot curRobot)
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

            try
            {

                #region [ Check Slot Job Exist Status ]

                #region [ Port#XXJobEachCassetteSlotExistsBlock Structure ]

                //會根據不同的Line有不同的長度
                //<event name="L2_W_Port#01JobEachCassetteSlotExistsBlock" devicecode="W" address="0x00015CC" points="4">
                //  <itemgroup name="Port#01JobEachCassetteSlotExistsBlock" />
                //</event>

                //<itemgroup name="Port#01JobEachCassetteSlotExistsBlock">
                //  <item name="JobExistence" woffset="0" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
                //</itemgroup>

                #endregion

                allSlotExistInfo = trxExistInfo.EventGroups[0].Events[0].Items[0].Value;

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

                    Job curBcsJob = ObjectManager.JobManager.GetJob(job_CstSeq.ToString(), job_JobSeq.ToString());

                    lock(curBcsJob)
                    {
                        if (Workbench.ServerName.Contains("TCFLR100"))
                        { 
                           Equipment EQP = ObjectManager.EquipmentManager.GetEQP("L4");
                           int CheckDatalength=curBcsJob.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME.Length;
                           string CheckData = curBcsJob.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME.Substring(CheckDatalength-2,1);
                           //string CheckData = curBcsJob.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME.Substring(1, 1);
                           if (CheckData.Contains("B") && (EQP.File.Status==eEQPStatus.IDLE|| EQP.File.Status==eEQPStatus.RUN))
                           //if (CheckData.Contains("5"))
                           {
                               curBcsJob.ArraySpecial.FLRFirstGlassSendOutFlag = "2";
                               //strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Check Process Operation Name =[{1}]),This Port Glass[{2}] Need First Fetch Out!!!",EQP.Data.NODENO,curBcsJob.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME,curBcsJob.GlassChipMaskBlockID);
                               //strlog = string.Format("[RCS <- RCS] Check Process Operation Name =[{1}]),This Port Glass[{2}] Need First Fetch Out!!!", curBcsJob.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME, curBcsJob.GlassChipMaskBlockID);
                               // Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                           }
                           else
                           {
                               curBcsJob.ArraySpecial.FLRFirstGlassSendOutFlag = "1";
                           }
                        }                   
                    }
                                       
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

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) PortType({5}) But SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) is Process Complete, CurStepNo({10}) >= Complete StepNo({11}) not In Process!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    curStage.Data.CASSETTETYPE, curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo,
                                                    curBcsJob.JobSequenceNo, job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_00002 ]

                        if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) PortType({5}) But SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) is Process Complete , CurStepNo({10}) >= Complete StepNo({11}) not In Process!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    curStage.Data.CASSETTETYPE, curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo,
                                                    curBcsJob.JobSequenceNo, job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Job FailMsg ]

                            //failMsg = string.Format("Robot({0}) StageNo({1}) StageName({2}) StageCSTType({3}) PortType({4}) But SlotNo({5}) Job({6},{7}) Exist({8}) is Process Complete,  CurStepNo({9}) >= Complete StepNo({10})",
                            //                        curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, curStage.Data.CASSETTETYPE,
                            //                        curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //                        job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO);

                            failMsg = string.Format("StageNo({0}) StageName({1}) StageCSTType({2}) PortType({3}) But SlotNo({4}) Job({5},{6}) Exist({7}) is Process Complete,  CurStepNo({8}) >= Complete StepNo({9})",
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

                        //20160110 add for return NG
                        return ePortJobUDRQReason.CANNOT_FIND_ROUTE;
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

                    #region [ by Route Check Fetch Out Condition ] 20150922 Mark.所有確認條件改到Filter一併確認

                    //string checkFailCode = string.Empty;
                    //string checkFailMsg = string.Empty;

                    ////Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_00003 ]
                    //fail_ReasonCode = eJob_CheckFail_Reason.Get_CstSlotExistJob_CheckFetchOut_Condition_Fail;

                    //if (CheckPortFetchOutCondition(curRobot, curStage, curBcsJob, out checkFailCode, out checkFailMsg) == false)
                    //{
                    //    #region  [DebugLog]

                    //    if (IsShowDetialLog == true)
                    //    {

                    //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) PortType({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) CurStepNo({10}) can not FetchOut! FailCode({11}) FailMsg({12})!",
                    //                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                    //                                curStage.Data.CASSETTETYPE, curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo,
                    //                                curBcsJob.JobSequenceNo, job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), checkFailCode, checkFailMsg);

                    //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    //    }

                    //    #endregion

                    //    #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_00003 ]

                    //    if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    //    {

                    //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) PortType({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) CurStepNo({10}) can not FetchOut! FailCode({11}) FailMsg({12})!",
                    //                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                    //                                curStage.Data.CASSETTETYPE, curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo,
                    //                                curBcsJob.JobSequenceNo, job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), checkFailCode, checkFailMsg);

                    //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //        #region [ 記錄Fail Msg To OPI and Job FailMsg ]

                    //        failMsg = string.Format("Robot({0}) StageNo({1}) StageName({2}) StageCSTType({3}) PortType({4}) SlotNo({5}) Job({6},{7}) Exist({8}) CurStepNo({9}) can not FetchOut! FailCode({10}) FailMsg({11})!",
                    //                                curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, curStage.Data.CASSETTETYPE,
                    //                                curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                    //                                job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), checkFailCode, checkFailMsg);

                    //        AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                    //        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                    //        #endregion

                    //    }

                    //    #endregion

                    //    return ePortJobUDRQReason.OTHERS;

                    //}
                    //else
                    //{
                    //    //Clear[ Job_Fail_Case_00003 ]
                    //    RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                    //}

                    #endregion

                    #region [ Update RobotJob WIP ]

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
                        curBcsJob.RobotWIP.CurSendOutJobJudge != curBcsJob.JobJudge)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] CassetteSequenceNo({1}) JobSequenceNo({2}) Update RobotJobWIP curStageNo from ({3}) to ({4}), curSlotNo from ({5}) to ({6}) ,curStageType from ({7}) to ({8}), PortCSTStatusPriority from ({9}) to ({10}) sendOutJobJudge from ({11}) to ({12})!",
                                                curStage.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurLocation_StageID,
                                                curStage.Data.STAGEID, curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(), slotKey.ToString(), curBcsJob.RobotWIP.CurLocation_StageType,
                                                eRobotStageType.PORT, curBcsJob.RobotWIP.CurPortCstStatusPriority, tmpPortCstStatusPriority, curBcsJob.RobotWIP.CurSendOutJobJudge,
                                                curBcsJob.JobJudge);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {

                            curBcsJob.RobotWIP.CurLocation_StageID = curStage.Data.STAGEID;
                            curBcsJob.RobotWIP.CurLocation_SlotNo = slotKey;
                            curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.PORT;
                            curBcsJob.RobotWIP.CurPortCstStatusPriority = tmpPortCstStatusPriority;
                            curBcsJob.RobotWIP.CurSendOutJobJudge = curBcsJob.JobJudge;

                            if (Workbench.LineType.ToString().Contains("FCREP_"))//Added by Zhangwei 20161010
                            {
                                if (curBcsJob.JobJudge == "5")
                                    curBcsJob.RobotWIP.RepairPriority = eRepairPriority.NORMAL_REPAIR;
                                else if (curBcsJob.JobJudge == "6")
                                    curBcsJob.RobotWIP.RepairPriority = eRepairPriority.INK_REPAIR;
                                else
                                    curBcsJob.RobotWIP.RepairPriority = eRepairPriority.OTHER;
                            }

                        }

                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                    #region [ Add To Can Control Joblist and Update Stage Status ]

                    if (AddToCanControlJoblistCondition(curRobot, curStage, curBcsJob, curCanControlJobList, jobKey, MethodBase.GetCurrentMethod().Name) == true)
                    {
                        #region [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) add CassetteSequenceNo({4}) JobSequenceNo({5}) CurRouteStepNo({6}) CurSlotNo({7}) to Can control List!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.CurLocation_SlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //20150922 add UDRQ SlotNo To UDRQ_SlotNoList
                        lock (curStage)
                        {
                            if (curStage.curUDRQ_SlotList.ContainsKey(slotKey) == false)
                            {
                                curStage.curUDRQ_SlotList.Add(slotKey, jobKey);
                            }
                        }

                        //BothPort 通通是為LDRQ_UDRQ 狀態
                        if (curPort.File.Type == ePortType.BothPort)
                        {
                            UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                        }
                        else
                        {
                            //Loader Port只需確認之後直接更新Stage Main Status 不需要Judge
                            UpdateStageStatus(curStage, eRobotStageStatus.SEND_OUT_READY, MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
                        }

                        #region Update Stage Route ID For CF MQC TTP Line Watson Add 20150111
                        if (curBcsJob.SamplingSlotFlag == "Y" || curBcsJob.SamplingSlotFlag == "1") //add for port asign CurRouteID 2016/06/18 cc.kuang
                        {
                            if (curBcsJob.RobotWIP.CurRouteID.Trim() == curRobot.Cur_CFMQCTTP_Flow_Route.Trim())
                                curStage.File.CurRouteID = curBcsJob.RobotWIP.CurRouteID;
                        }
                        #endregion


                        #region Update Port Route dictionary Info Watson Add 20160104
                        if (curRobot.CurPortRouteIDInfo.ContainsKey(curPort.Data.PORTNO))
                            curRobot.CurPortRouteIDInfo.Remove(curPort.Data.PORTNO);
                        curRobot.CurPortRouteIDInfo.Add(curPort.Data.PORTNO, curBcsJob.RobotWIP.CurRouteID);
                        #endregion   


                        #region [ 20160303 add for Array 需要紀錄目前Port Stage有多少組RecipeGroupNo ]

                        if (curBcsJob.SamplingSlotFlag == "1" && curStage.CurRecipeGroupNoList.Find(s => s==curBcsJob.ArraySpecial.RecipeGroupNumber.Trim()) == null)
                        {
                            lock (curStage)
                            {

                                curStage.CurRecipeGroupNoList.Add(curBcsJob.ArraySpecial.RecipeGroupNumber.Trim());
                                
                            }         

                        }

                        #endregion

                        return ePortJobUDRQReason.REASON_OK;

                    }
                    else
                    {
                        return ePortJobUDRQReason.OTHERS;
                    }

                    #endregion

                    #endregion

                }
                else if (job_CstSeq > 0 && job_JobSeq > 0 && job_ExistInfo == ePortSlotExistInfo.JOB_NO_EXIST)
                {

                    #region [ 有帳無料 Has Job Info but No Exist ]

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(has JobInfo but glass not exist)!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    return ePortJobUDRQReason.JOBINFO_EXIST_JOB_NOT_EXIST;

                    #endregion

                }
                else if (job_CstSeq == 0 && job_JobSeq == 0 && job_ExistInfo == ePortSlotExistInfo.JOB_EXIST)
                {

                    #region [ 無帳有料 Has Job Exist but No Job Info ]

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(glass Exist but no JobInfo)!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    return ePortJobUDRQReason.JOBINFO_NOT_EXIST_JOB_EXIST;

                    #endregion

                }
                else if (job_CstSeq == 0 && job_JobSeq == 0 && job_ExistInfo == ePortSlotExistInfo.JOB_NO_EXIST)
                {

                    #region [ 無帳無料 No Job Info and Job not Exist ]

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(No JobInfo and glass not Exist)!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    return ePortJobUDRQReason.IS_EMPTY_SLOT;

                    #endregion

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(JobKey or Glass exist status is Illegal!Please Check)!",
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

        /// <summary> 判斷是否符合Port取片條件
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotStage"></param>
        /// <param name="curBcsJob"></param>
        /// <returns></returns>
        private bool CheckPortFetchOutCondition(Robot curRobot, RobotStage curRobotStage, Job curBcsJob, out string failCode, out string failMsg)
        {
            string tmpFailCode = string.Empty;
            string tmpFailMsg = string.Empty;

            try
            {
                failCode = tmpFailCode;
                failMsg = tmpFailMsg;
                //by Line Check Fetch Out Condition [ Wait_Proc_0013 ]
                return true;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                failCode = "Exception";
                failMsg = ex.Message;
                return false;
            }

        }

        /// <summary> 判斷是否可已加入到Can Control Job List的條件.不可出現重複Job
        ///
        /// </summary>
        /// <param name="curCanControlJobList"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        private bool AddToCanControlJoblistCondition(Robot curRobot, RobotStage curStage, Job curBcsJob, List<Job> curCanControlJobList, string jobKey, string funcName)
        {
            string strlog = string.Empty;

            try
            {
                if (curCanControlJobList == null)
                    curCanControlJobList = new List<Job>();

                if (curCanControlJobList.Count == 0)
                {
                    curCanControlJobList.Add(curBcsJob);
                }
                else
                {
                    Job duplicateJob = curCanControlJobList.FirstOrDefault(j => j.JobKey == jobKey);

                    if (duplicateJob != null)
                    {
                        curCanControlJobList.Remove(duplicateJob);
                        curCanControlJobList.Add(curBcsJob);

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}]Robot({2}) add Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageNo({5}) StageSlot({6}) to Can Control Job list is duplicate! duplicate Job CassetteSequenceNo({7}) JobSequenceNo({8}) StageNo({9}) StageSlot({10}) is removed!",
                                                    curStage.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                    curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurLocation_SlotNo, duplicateJob.CassetteSequenceNo,
                                                    duplicateJob.JobSequenceNo, duplicateJob.RobotWIP.CurLocation_StageID, duplicateJob.RobotWIP.CurLocation_SlotNo);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                    }
                    else
                    {
                        curCanControlJobList.Add(curBcsJob);

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

        #endregion [ For Loader Port Use Function ] ===============================================================================================================================

        #region [ For Both Port Use Function ] =================================================================================================================================

        /// <summary>
        /// 確認Both Port Slot是否有JOB存在並加入到Can Control List and Update UDRQ Status
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curCanCtlJobList"></param>
        /// <param name="curPort"></param>
        private void Get_BothPort_CanControlJoblist(Robot curRobot, RobotStage curStage, List<Job> curCanCtlJobList, Port curPort)
        {

            string tmpStageStatus = string.Empty;
            string trxID = string.Empty;
            string strlog = string.Empty;
            string checkReasonCode = string.Empty;
            bool hasJobUDRQFlag = false;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;

            try
            {

                //預設為NoReq
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                #region [ 20151029 add Aborting不可出片只可收片 ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00013 ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_FETCHPORT_BUT_STATUS_IS_ABORTING, curPort.Data.PORTID);

                if (curPort.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                {
                    //Port Status Aborting 不能取片(UDRQ)則需更新Stage Status並紀錄Log
                    UpdateStage_UDRQ_Status(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

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

                #region [ 不可出片只可收片,Port Can Fetch Out Check,it is cycle stop Fetch Out,match IO/Parameters:PortCanFetchOutCheckFlag ]//Deng,20190828
                bool PortCanFetchOutCheckFlag = true;
                if (Workbench.LineType == "ELA_JSW")
                {
                    switch (curStage.Data.STAGEID.Trim())
                    {
                        case "01":
                            PortCanFetchOutCheckFlag = ParameterManager.ContainsKey("Port#1PortCanFetchOutCheckFlag") ? ParameterManager["Port#1PortCanFetchOutCheckFlag"].GetBoolean() : true;
                            break;

                        case "02":
                            PortCanFetchOutCheckFlag = ParameterManager.ContainsKey("Port#2PortCanFetchOutCheckFlag") ? ParameterManager["Port#2PortCanFetchOutCheckFlag"].GetBoolean() : true;
                            break;

                        case "03":
                            PortCanFetchOutCheckFlag = ParameterManager.ContainsKey("Port#3PortCanFetchOutCheckFlag") ? ParameterManager["Port#3PortCanFetchOutCheckFlag"].GetBoolean() : true;
                            break;

                        case "04":
                            PortCanFetchOutCheckFlag = ParameterManager.ContainsKey("Port#4PortCanFetchOutCheckFlag") ? ParameterManager["Port#4PortCanFetchOutCheckFlag"].GetBoolean() : true;
                            break;

                        default:
                            break;
                    }
                }

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_R000000024 ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.IO_PARAMETERS_PORT_CAN_FETCH_OUT_CHECKFLAG_IS_DISABLE, curPort.Data.PORTID);

                if (!PortCanFetchOutCheckFlag)
                {
                    //不能取片(UDRQ)則需更新Stage Status並紀錄Log
                    UpdateStage_UDRQ_Status(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_R000000024 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) IO Parameter PortCanFetchOutCheckFlag is (Disable) can not Fetch Out Job!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("Port({0}) IO Parameter PortCanFetchOutCheckFlag is (Disable) can not Fetch Out Job!",
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
                    //Clear[ Robot_Fail_Case_R000000024 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                //Add By Yangzhenteng 20191108 FOr CVD700 Fetch Out Delay
                #region[CVD700 Mix Run Port 出片延时]                                         
                if (Workbench.ServerName.Contains("TCCVD700"))
                {                    
                    int CheckDelayTime=0;
                    switch(curPort.File.SamplingCount)
                    {
                        case 21:
                        case 14:
                        case 7:
                            CheckDelayTime = 660000;
                            break;
                        case 28:
                            CheckDelayTime = 0;
                            break;
                        default:
                            CheckDelayTime = 300000;
                            break;
                    }
                    if (CheckDelayTime != 0)
                    {
                        //CheckDealyTime不为0,且时间小于设置的出片时间
                        if (new TimeSpan(DateTime.Now.Ticks - curPort.File.PortLastGlassFetchOutTime.Ticks).TotalMilliseconds < CheckDelayTime)
                        {
                            fail_ReasonCode = string.Format("{0}_Glass Sampling Count={1},Dealy={2}Seconds,PortStage Change To UDRQ ", curPort.Data.PORTID);
                            //Stage不能更新为UD_RQ;
                            UpdateStage_UDRQ_Status(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                            if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}),Last Glass Fetch Out Time={3}, Now Time={4}, can not Fetch Out Job!!!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, curPort.File.PortLastGlassFetchOutTime.ToString(), DateTime.Now.ToString());
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                failMsg = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}),Last Glass Fetch Out Time={3}, Now Time={4}, can not Fetch Out Job!!!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, curPort.File.PortLastGlassFetchOutTime.ToString(), DateTime.Now.ToString());
                                AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                            }
                            return;
                        }
                        else
                        {
                            //Clear[ Robot_Fail_Case_00013 ]
                            RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                        }
                    }
                    else
                    {
                        //Clear[ Robot_Fail_Case_00013 ]
                        RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                    } 
                }
                #endregion
                               
                #region [ Check FirstGlsssCheck, Fail表示不可出片 ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00009 ]
                fail_ReasonCode = string.Format("{0}_{1}", eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_FETCHPORT_WAIT_FIRSTGLASSCHECK, curPort.Data.PORTID);


                if (CheckFirstGlassCheckCondition(curRobot, curPort, curStage) == false)
                {

                    //Stage內最後的Slot為空or有其他錯誤視為找不到or帳料有問題,則需更新Stage Status並紀錄Log
                    UpdateStage_UDRQ_Status(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

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
                trxID = GetTrx_CSTSlotJobExistInfo(curStage.Data.NODENO, curStage.Data.STAGEID);

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

                    UpdateStage_UDRQ_Status(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

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

                    UpdateStage_UDRQ_Status(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                    return;

                }

                #endregion

                //根據Robot設定的取片順序來決定要從Slot01開始抽還是SlotMax開始抽 ASC 从小到大(Priority 01>02>03>…) ,DEC 从大到小 (Priority 01<02<03>…), default : ASC 从小到大
                //20151125 for CSOT要求以EQ上報為準 
                //if (curRobot.Data.SLOTFETCHSEQ == eRobotCommonConst.DB_ORDER_BY_ASC)
                if (curRobot.EqpRptCSTFetchSeqMode == eRobotCommonConst.DB_ORDER_BY_ASC)
                {
                    #region [ 抽片順序為ASC SlotNo由小到大 ]

                    for (int i = 1; i <= curStage.Data.SLOTMAXCOUNT; i++)
                    {
                        checkReasonCode = Get_CSTSlot_CanControlJoblist(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList, curPort, curRobot);

                        #region [ 根據Port CST Type來決定是否要判斷下一片 ]

                        if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                        {

                            #region [ 非循序CST的處理,可跳Slot ]

                            switch (checkReasonCode)
                            {
                                case ePortJobUDRQReason.REASON_OK:

                                    //已在Get_CSTSlot_CanControlJoblist更新過Stage Status 所以直接Return即可 20150922 mark for Get Port All UDRQ Job List
                                    hasJobUDRQFlag = true;
                                    break;
                                    //return;

                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

                                    //確認是空Slot則判斷下一個Slot
                                    break;

                                default:

                                    //其他錯誤是為找不到 則跳下一個Slot判斷
                                    break;
                            }

                            #endregion

                        }
                        else
                        {

                            #region [ 循序CST的處理,不可跳Slot ]

                            switch (checkReasonCode)
                            {
                                case ePortJobUDRQReason.REASON_OK:

                                    //已在Get_CSTSlot_CanControlJoblist更新過Stage Status 所以直接Return即可
                                    return;

                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

                                    //確認是空Slot則判斷下一個Slot
                                    break;

                                default:

                                    //其他錯誤視為找不到或是帳料有問題需更新Stage Status並紀錄Log
                                    UpdateStage_UDRQ_Status(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                                    return;
                            }

                            #endregion

                        }

                        #endregion

                    }

                    //有其中一個Slot UDRQ則為True
                    if (hasJobUDRQFlag == false)
                    {
                        //Stage內最後的Slot為空or有其他錯誤視為找不到or帳料有問題,則需更新Stage Status並紀錄Log
                        UpdateStage_UDRQ_Status(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                    }

                    #endregion
                }
                else
                {

                    #region [ 抽片順序為DEC SlotNo由大到小 ]

                    for (int i = curStage.Data.SLOTMAXCOUNT; i > 0; i--)
                    {
                        checkReasonCode = Get_CSTSlot_CanControlJoblist(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList, curPort, curRobot);

                        #region [ 根據Port CST Type來決定是否要判斷下一片 ]

                        if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                        {

                            #region [ 非循序CST的處理,可跳Slot ]

                            switch (checkReasonCode)
                            {
                                case ePortJobUDRQReason.REASON_OK:

                                    //已在Get_CSTSlot_CanControlJoblist更新過Stage Status 所以直接Return即可 20150922 mark for Get Port All UDRQ Job List
                                    hasJobUDRQFlag = true;
                                    break;
                                    //return;

                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

                                    //確認是空Slot則判斷下一個Slot
                                    break;

                                default:

                                    //其他錯誤是為找不到 則跳下一個Slot判斷
                                    break;
                            }

                            #endregion

                        }
                        else
                        {

                            #region [ 循序CST的處理,不可跳Slot ]

                            switch (checkReasonCode)
                            {
                                case ePortJobUDRQReason.REASON_OK:

                                    //已在Get_CSTSlot_CanControlJoblist更新過Stage Status 所以直接Return即可
                                    return;

                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

                                    //確認是空Slot則判斷下一個Slot
                                    break;

                                default:

                                    //其他錯誤視為找不到或是帳料有問題需更新Stage Status並紀錄Log
                                    UpdateStage_UDRQ_Status(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                                    return;
                            }

                            #endregion

                        }

                        #endregion
                    }

                    //有其中一個Slot UDRQ則為True
                    if (hasJobUDRQFlag == false)
                    {
                        //Stage內最後的Slot為空or有其他錯誤視為找不到or帳料有問題,則需更新Stage Status並紀錄Log
                        UpdateStage_UDRQ_Status(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                    }

                    #endregion
                }

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 確認Both Port Slot是否有空Slot存在並更新Stage LDRQ Status
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curPort"></param>
        private void Get_BothPort_StageLDRQStatueInfo(Robot curRobot, RobotStage curStage, Port curPort)
        {
            string jobKey = string.Empty;
            string trxID = string.Empty;
            string strlog = string.Empty;
            string tmpStageStatus = string.Empty;
            string tmpCstStatusPriority = string.Empty;
            bool findEmptySlotFlag = false;

            try
            {

                //預設為NoReq
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                #region [ Set Unload and Both Port Receive Job Priority ]

                if (curPort.File.Type == ePortType.BothPort)
                {
                    //20151030 add Aboring > InProcess > Wait ForProcess
                    if (curPort.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                    {
                        tmpCstStatusPriority = eUnloadPortReceiveStatus.BOTH_PORT_IN_ABORTING;
                    }
                    else if (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)
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

                #region  [ Real time Get Port Slot Exist Info Trx ]

                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotExistsBlock    
                trxID = GetTrx_CSTSlotJobExistInfo(curStage.Data.NODENO, curStage.Data.STAGEID);

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

                    UpdateStage_LDRQ_Status(curStage, tmpStageStatus,tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

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

                    UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

                    return;

                }

                #endregion

                //根據Robot設定的放片順序來決定要從Slot01開始放在還是SlotMax開始放 ASC 从小到大(Priority 01>02>03>…) ,DEC 从大到小 (Priority 01<02<03>…), default : ASC 从小到大          
                if (curRobot.Data.SLOTSTORESEQ == "DESC")
                {

                    #region [ 放片順序為DEC SlotNo由大到小 ]

                    for (int i = curStage.Data.SLOTMAXCOUNT; i > 0; i--)
                    {

                        #region [ Check Unlaod Port Status LDRQ ]

                        if (Check_CSTSlot_IsEmpty(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i) == true)
                        {

                            //此時已經將Empty SlotNo add To Stage Entity Dic
                            findEmptySlotFlag = true;

                        }
                        else
                        {
                            #region [ 目前Slot不是Empty Slot 的處理 ]

                            if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                            {

                                #region [ CSType不為循序,可以選擇下一個Slot判斷是否為空Slot ]

                                //一旦發現不是空Slot 則判斷Flag是否在之前的Slot內已經找到Empty Slot
                                if (findEmptySlotFlag == true)
                                {
                                    //20150914 add 一旦發現不是空slot 則判斷是否已經將Status改為LDRQ如果沒有在Update狀態為LDRQ
                                    if (tmpStageStatus != eRobotStageStatus.RECEIVE_READY)
                                    {
                                        //已找到有Empty Slot 則Status=LDRQ and Update Stage Status
                                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

                                        UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);
                                    }

                                    //要找出所有Empty Slot
                                    //return;

                                }

                                #endregion

                            }
                            else
                            {

                                #region [ CSTType為循序,不可以選擇下一個Slot ]

                                //一旦發現不是空Slot 則看Flag是否已經準備收片
                                if (findEmptySlotFlag == true)
                                {

                                    tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

                                    UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

                                    return;

                                }
                                else
                                {
                                    tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                                    //非Empty Slot 則視為NOREQ 並將Priority設為Others
                                    UpdateStage_LDRQ_Status(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);

                                    return;

                                }

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

                        UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

                    }
                    else
                    {
                        
                        //最後一片非Empty Slot 則視為NOREQ 並將Priority設為Others
                        UpdateStage_LDRQ_Status(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);
                        
                    }

                    #endregion

                    #endregion

                }
                else
                {

                    #region [ 放片順序為ASC SlotNo由小到大 ]

                    for (int i = 1; i <= curStage.Data.SLOTMAXCOUNT; i++)
                    {

                        #region [ Check Unlaod Port Status LDRQ ]

                        if (Check_CSTSlot_IsEmpty(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i) == true)
                        {

                            //此時已經將Empty SlotNo add To Stage Entity Dic
                            findEmptySlotFlag = true;

                        }
                        else
                        {
                            #region [ 目前Slot不是Empty Slot 的處理 ]

                            if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                            {
                                
                                #region [ CSType不為循序,可以選擇下一個Slot判斷是否為空Slot ]

                                //一旦發現不是空Slot 則判斷Flag是否在之前的Slot內已經找到Empty Slot
                                if (findEmptySlotFlag == true)
                                {
                                    //20150914 add 一旦發現不是空slot 則判斷是否已經將Status改為LDRQ如果沒有在Update狀態為LDRQ
                                    if (tmpStageStatus != eRobotStageStatus.RECEIVE_READY)
                                    {
                                        //已找到有Empty Slot 則Status=LDRQ and Update Stage Status
                                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

                                        UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);
                                    }

                                    //要找出所有Empty Slot
                                    //return;

                                }

                                #endregion

                            }
                            else
                            {

                                #region [ CSTType為循序,不可以選擇下一個Slot ]

                                //一旦發現不是空Slot 則看Flag是否已經準備收片
                                if (findEmptySlotFlag == true)
                                {

                                    tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

                                    UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

                                    return;

                                }
                                else
                                {
                                    tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                                    //非Empty Slot 則視為NOREQ 並將Priority設為Others
                                    UpdateStage_LDRQ_Status(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);

                                    return;

                                }

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

                        UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

                    }
                    else
                    {
                        
                        //最後一片非Empty Slot 則視為NOREQ 並將Priority設為Others
                        UpdateStage_LDRQ_Status(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);
                        
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

        #endregion [ For Both Port Use Function ] ==============================================================================================================================

        #region [ For Unload Port Use Function ] ===============================================================================================================================

        /// <summary>
        /// 確認Unload Port Slot是否有空Slot存在並更新Stage Status
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curPort"></param>
        private void Get_UnloadPort_StageStatueInfo(Robot curRobot, RobotStage curStage, Port curPort)
        {
            string jobKey = string.Empty;
            string trxID = string.Empty;
            string strlog = string.Empty;
            string tmpStageStatus = string.Empty;
            string tmpCstStatusPriority = string.Empty;
            bool findEmptySlotFlag = false;

            try
            {

                //預設為NoReq
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

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

                #region  [ Real time Get Port Slot Exist Info Trx ]

                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotExistsBlock    
                trxID = GetTrx_CSTSlotJobExistInfo(curStage.Data.NODENO, curStage.Data.STAGEID);

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

                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

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

                        #region [ Check Unlaod Port Status LDRQ ]

                        if (Check_CSTSlot_IsEmpty(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i)==true)
                        {

                            //此時已經將SlotNo更新到Stage內
                            findEmptySlotFlag = true;

                        }
                        else
                        {
                            #region [ 目前Slot不是Empty Slot 的處理 ]

                            if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
                            {

                                #region [ CSType不為循序,可以選擇下一個Slot判斷是否為空Slot ]

                                //一旦發現不是空Slot 則判斷Flag是否在之前的Slot內已經找到Empty Slot
                                if (findEmptySlotFlag == true)
                                {

                                    //已找到有Empty Slot 則Status=LDRQ and Update Stage Status
                                    tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

                                    UpdateUnloadStageStatus(curStage, tmpStageStatus, tmpCstStatusPriority,MethodBase.GetCurrentMethod().Name);

                                    return;

                                }

                                #endregion

                            }
                            else
                            {
                                //20160114 modify 如果是循序CST且從SlotMax開始放則只要清空curStage.curLDRQ_EmptySlotList 即可
                                #region [ CSTType為循序,且從SlotMax開始放則只要清空curStage.curLDRQ_EmptySlotList ]

                                curStage.curLDRQ_EmptySlotList.Clear();
                                findEmptySlotFlag = false;
                                ////一旦發現不是空Slot 則看Flag是否已經準備收片
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

                    #region [ 放片順序為ASC SlotNo由小到大 ]

                    for (int i = 1; i <= curStage.Data.SLOTMAXCOUNT; i++)
                    {

                        #region [ Check Unlaod Port Status LDRQ ]

                        if (Check_CSTSlot_IsEmpty(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i) == true)
                        {

                            //此時已經將SlotNo更新到Stage內
                            findEmptySlotFlag = true;

                        }
                        else
                        {
                            #region [ 目前Slot不是Empty Slot 的處理 ]

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

                                #region [ CSTType為循序,不可以選擇下一個Slot ]

                                //一旦發現不是空Slot 則看Flag是否已經準備收片
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
        private bool Check_CSTSlot_IsEmpty(RobotStage curStage, Trx trxExistInfo, Trx trxJobInfo, int slotKey)
        {

            string strlog = string.Empty;
            string allSlotExistInfo = string.Empty;
            int job_ExistInfo = ePortSlotExistInfo.JOB_NO_EXIST;
            string jobTrx_CstSeqkey = string.Empty;
            string jobTrx_JobSeqkey = string.Empty;
            string jobTrx_GroupName = string.Empty;
            string jobTrx_EventName = string.Empty;
            int job_CstSeq = 0;
            int job_JobSeq = 0;

            try
            {

                #region [ Check Slot Job Exist Status ]

                #region [ Port#XXJobEachCassetteSlotExistsBlock Structure ]

                //會根據不同的Line有不同的長度
                //<event name="L2_W_Port#01JobEachCassetteSlotExistsBlock" devicecode="W" address="0x00015CC" points="4">
                //  <itemgroup name="Port#01JobEachCassetteSlotExistsBlock" />
                //</event>

                //<itemgroup name="Port#01JobEachCassetteSlotExistsBlock">
                //  <item name="JobExistence" woffset="0" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
                //</itemgroup>

                #endregion

                allSlotExistInfo = trxExistInfo.EventGroups[0].Events[0].Items[0].Value;

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

                    return false;

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

                    return false;
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

                if (job_CstSeq == 0 && job_JobSeq == 0 && job_ExistInfo == ePortSlotExistInfo.JOB_NO_EXIST)
                {

                    #region [ 無帳無料 ]

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CassetteSequenceNo({7}) JobSequenceNo({8}) GlassExist({9}) is Empty!(Job Info and Glass Exist are not exist)",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    //Update Current Stage LDRQ Empty Slot
                    lock (curStage)
                    {
                        curStage.CurLDRQ_EmptySlotNo = slotKey.ToString().PadLeft(3, '0');

                        //add Empty SlotNo To EmptySlotNoList
                        if (curStage.curLDRQ_EmptySlotList.ContainsKey(slotKey) == false)
                        {
                            curStage.curLDRQ_EmptySlotList.Add(slotKey, string.Empty);
                        }

                    }

                    return true;

                    #endregion

                }
                else if (job_CstSeq > 0 && job_JobSeq > 0 && job_ExistInfo == ePortSlotExistInfo.JOB_EXIST)
                {

                    #region [ 有帳有料 ]

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CassetteSequenceNo({7}) JobSequenceNo({8}) GlassExist({9}) is not Empty!(Job Info and Glass Exist are exist)",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    return false;

                    #endregion 

                }
                else
                {

                    #region [ 帳料異常 ]

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CassetteSequenceNo({7}) JobSequenceNo({8}) GlassExist({9}) is abnormal(Job Info and Glass Exist are not match)!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

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

        #endregion [ For Unload Port Use Function ] ============================================================================================================================

        /// <summary> 取得Port的CST Slot Position 所有Job CstSeq ,JobSeq資訊的Trx
        ///
        /// </summary>
        /// <param name="nodeNo"></param>
        /// <param name="portNo"></param>
        /// <returns></returns>
        private string GetTrx_CSTSlotJobEachPosition(string nodeNo, string portNo)
        {
            string trxID = string.Empty;

            try
            {
                ////StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotPositionBlock  
                trxID = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", nodeNo, portNo.PadLeft(0,'2'));

                return trxID;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return trxID;
            }

        }

        /// <summary> 取得Port的CST Slot 中所有Job Exist資訊的Trx
        ///
        /// </summary>
        /// <param name="nodeNo"></param>
        /// <param name="portNo"></param>
        /// <returns></returns>
        private string GetTrx_CSTSlotJobExistInfo(string nodeNo, string portNo)
        {
            string trxID = string.Empty;

            try
            {
                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotExistsBlock 
                trxID = string.Format("{0}_Port#{1}JobEachCassetteSlotExistsBlock", nodeNo, portNo.PadLeft(2, '0'));

                return trxID;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return trxID;
            }

        }

        /// <summary>判斷First Glass Check狀態來決定目前Loader or Both Port是否可以出片(C2 Can Not Select! Other Can Select CST Job)
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curPort"></param>
        /// <returns></returns>
        private bool CheckFirstGlassCheckCondition(Robot curRobot, Port curPort , RobotStage curStage)
        {
            string strlog = string.Empty;

            try
            {

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

                #region [ Get CST Entity by Port CSTID ]

                Cassette curCST = ObjectManager.CassetteManager.GetCassette(curPort.File.CassetteID);

                //找不到 CST 回NG
                if (curCST == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get CST Entity by Port({2}) CstID({3})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, curPort.File.CassetteID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }

                #endregion

                #region [ 同步CST First Glass Check Report ]

                if (curStage.File.CstFirstGlassCheckResult != curCST.FirstGlassCheckReport)
                {

                    lock (curStage)
                    {
                        curStage.File.CstFirstGlassCheckResult = curCST.FirstGlassCheckReport;
                    }

                }
                if (curStage.Data.LINEID == "TCCVD100" || curStage.Data.LINEID == "TCCVD200") //add by qiumin 20180917 CVD100 FIRST CHECK By pass
                {
                    lock (curStage)
                    {
                        curStage.File.CstFirstGlassCheckResult = "Y";
                    }
                }

                #endregion

                #region [ Check First Glass Check Mode. C2 Can Not Select! Other Can Select CST Job ]

                // Y:OK, Robort can start fetch glass from cst
                if (curCST.FirstGlassCheckReport == "Y")
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) CstID({3}) First Glass Check Report({4}).",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, curPort.File.CassetteID,
                                                curCST.FirstGlassCheckReport);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return true;
                }
                else if (curCST.FirstGlassCheckReport == "C2" || curCST.FirstGlassCheckReport == "N") //C2:before fetch glass from cst, invoke MES.LotProcessStartRequest
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) CstID({3}) First Glass Check Report({4}) can not Fetch Out!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, curPort.File.CassetteID,
                                                curCST.FirstGlassCheckReport);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }
                else
                {
                    //尚未Send First Glass Check 必須要透過準備取第一片時再做FirstGlass Check 所以要回True

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) CstID({3}) First Glass Check Report({4}) must First Glass Check!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, curPort.File.CassetteID,
                                                curCST.FirstGlassCheckReport);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return true;

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        #endregion ==========================================================================================================================================================================

        #region Get EQP Type Stage Single Slot Can Control JobList Function List ==========================================================================================================

        //20151024 mark
        //private void Get_EqpTypeStageStatus_For1Arm1Job(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList)
        //{
        //    string tmpStageStatus = string.Empty;

        //    try
        //    {
        //        //預設為NoReq
        //        tmpStageStatus = eRobotStageStatus.NO_REQUEST;

        //        //判斷是Signal Slot Stage還是Mulit Slot Stage
        //        if (curStage.Data.ISMULTISLOT != "Y")
        //        {
        //            //For Signal Slot Stage [ Wait_For_Proc_00025 ] 針對Signal Slot Stage Function
        //            //1. Get Stage UDRQ Status and CanControlJobList
        //            Get_EqpTypeSignal_CanControlJobList_For1Arm1Job(curRobot, curStage, curStageCanControlJobList);

        //            //2. Get Stage LDRQ Status
        //            Get_EqpTypeSignal_LDRQStauts_For1Arm1Job(curRobot, curStage);


        //            //3. Judge Main Status by UDRQ & LDRQ Status
        //            JudgeEQPStage_UDRQ_LDRQStatus(curStage);
        //        }
        //        else
        //        {
        //            //for Mulit Slot Stage [ Wait_For_Proc_00034 ]針對Get EQP Type CtlJobList 重寫版 對應Mulit Slot要特別寫

        //            //#region [ Get Stage UDRQ Status and CanControlJobList ]

        //            //Get_EqpType_CanControlJobList_For1Arm1Job(curRobot, curStage, curStageCanControlJobList);

        //            //#endregion

        //            //#region [ Get Stage LDRQ Status ]

        //            //Get_EqpTypeMuliSlot_LDRQStauts_ForGetGetPutPut(curRobot, curStage);

        //            //#endregion

        //            //JudgeEQPStage_UDRQ_LDRQStatus_ForGetGetPutPut(curStage);

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }

        //}

        //20151024 add for Get Mulit Slot EQP Type Stage Status

        /// <summary>Get Singal/Mulit Slot EQP Type Stage Status
        /// ISMULTISLOT = N => 不可同時LDRQ&UDRQ  ; ISMULTISLOT = Y & EQROBOTIFTYPE = MULTI(即MULTI_SINGLE)=>stage不可同時LDRQ&UDRQ ; ISMULTISLOT = Y & EQROBOTIFTYPE = NORMAL/BOTH(即MULTI_DAUL)=>stage可同時LDRQ&UDRQ
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        private void Get_EqpTypeStageStatus(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList)
        {
            string tmpStageStatus = string.Empty;

            try
            {
                //預設為NoReq
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                //判斷是Signal Slot Stage還是Mulit Slot Stage
                if (curStage.Data.ISMULTISLOT != "Y")
                {
                    //For Signal Slot Stage [ Wait_For_Proc_00025 ] 針對Signal Slot Stage Function

                    //20151111 modify  OVEN_SD Stage Not Multi-Slot
                    if (Workbench.LineType == eLineType.ARRAY.OVNSD_VIATRON)
                    {
                        Get_EqpTypeMulitSlot_CanControlJobList_ForTCOVN_SD(curRobot, curStage, curStageCanControlJobList);

                        Get_EqpTypeMuliSlot_LDRQStauts_ForTCOVN_SD(curRobot, curStage);
                    }
                    else
                    {
                        
                        //1. Get Stage UDRQ Status and CanControlJobList
                        Get_EqpTypeSingleSlot_CanControlJobList_For1Arm1Job(curRobot, curStage, curStageCanControlJobList);

                        //2. Get Stage LDRQ Status
                        Get_EqpTypeSignal_LDRQStauts_For1Arm1Job(curRobot, curStage);
                    }

                    //3. Judge Main Status by UDRQ & LDRQ Status
                    JudgeEQPStage_UDRQ_LDRQStatus(curStage);

                }
                else
                {
                    //for Mulit Slot Stage [ Wait_For_Proc_00034 ] 針對Mulit Slot Stage Functio

                    switch (curStage.Data.EQROBOTIFTYPE)
                    {
                        case eRobotStage_RobotInterfaceType.MULTI_SINGLE: //For IMP Mulit-Signal InterFace

                            //1. Get Stage UDRQ Status and CanControlJobList
                            Get_EqpTypeMulitSlot_CanControlJobList_ForMulitSingle(curRobot, curStage, curStageCanControlJobList);

                            //2. Get Stage LDRQ Status
                            Get_EqpTypeMuliSlot_LDRQStauts_ForMulitSingle(curRobot, curStage);

                            JudgeEQPStage_UDRQ_LDRQStatus_ForMulitSingle(curStage);

                            break;

                        case eRobotStage_RobotInterfaceType.MULTI_DAUL: //For OVEN Mulit-Daul InterFace

                            if (Workbench.LineType == eLineType.ARRAY.OVNSD_VIATRON)
                            {
                                Get_EqpTypeMulitSlot_CanControlJobList_ForTCOVN_SD(curRobot, curStage, curStageCanControlJobList);

                                Get_EqpTypeMuliSlot_LDRQStauts_ForTCOVN_SD(curRobot, curStage);
                            }
                            else
                            {
                                Get_EqpTypeMulitSlot_CanControlJobList_ForDUAL(curRobot, curStage, curStageCanControlJobList);

                                Get_EqpTypeMuliSlot_LDRQStauts_ForDUAL(curRobot, curStage);
                            }
                            JudgeEQPStage_UDRQ_LDRQStatus(curStage);
                            break;

                        case eRobotStage_RobotInterfaceType.NORMAL: //For Normal Get , Put(1slot IO) and Has Multi-Slot 等同Signal Mode處理

                            //For Signal Slot Stage [ Wait_For_Proc_00025 ] 針對Signal Slot Stage Function
                            //1. Get Stage UDRQ Status and CanControlJobList
                            Get_EqpTypeSingleSlot_CanControlJobList_For1Arm1Job(curRobot, curStage, curStageCanControlJobList);

                            //2. Get Stage LDRQ Status
                            Get_EqpTypeSignal_LDRQStauts_For1Arm1Job(curRobot, curStage);

                            //3. Judge Main Status by UDRQ & LDRQ Status
                            JudgeEQPStage_UDRQ_LDRQStatus(curStage);

                            break;

                        default:
                            break;
                    }
 

                }

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary>Get Singal/Mulit Slot Stage Type Stage Status
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        private void Get_StageTypeStageStatus(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList)
        {
            string tmpStageStatus = string.Empty;

            try
            {
                //預設為NoReq
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                //1. Get Stage UDRQ Status and CanControlJobList
                Get_StageTypeSingleSlot_CanControlJobList_For1Arm1Job(curRobot, curStage, curStageCanControlJobList);

                //2. Get Stage LDRQ Status
                Get_StageTypeSignal_LDRQStauts_For1Arm1Job(curRobot, curStage);

                //3. Judge Main Status by UDRQ & LDRQ Status
                JudgeIndexerStage_UDRQ_LDRQStatus(curStage);

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary> 取得EQP Type Stage,Slot is Single Can ControlJobList for 1 Arm 1Job Use . 
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        private void Get_EqpTypeSingleSlot_CanControlJobList_For1Arm1Job(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList)
        {
            string trxID = string.Empty;
            string strlog = string.Empty;
            //20160720
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;

            try
            {
                string bitOn = "1";
                string bitOff = "0";
                string funcName = MethodBase.GetCurrentMethod().Name;

				#region [Get line fabtyep]
				string fabtype = eFabType.ARRAY.ToString();
				Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
				if (line != null) {
					fabtype = line.Data.FABTYPE;
				}
				#endregion

                #region [ Check Trx Setting ]

                if (curStage.Data.UPSTREAMPATHTRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find UpStream TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;

                }

                #endregion

                //interface時Stage出片要看Upstream .透過LinkSignal已經得知JobData是否填好 Send Signal On =JobData Exist
                string[] upStreamTrxList = curStage.Data.UPSTREAMPATHTRXNAME.Split(',');
                string strSlotNoBin = string.Empty;
                string strGlassCountBin = string.Empty;
                int slotNo = 0;
                int anotherSlotNo = 0;
                int glassCount = 0;

                for (int i = 0; i < upStreamTrxList.Length; i++)
                {

                    #region  real time Get Interface Upstream

                    trxID = upStreamTrxList[i];

                    Trx upStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                    if (upStream_Trx == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                    curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        continue;
                    }

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #endregion

                    #region [拆出PLCAgent Data]

                    #region [ Trx Structure ]

                    //<trx name="L3_UpstreamPath#01" triggercondition="change">
                    //    <eventgroup name="L3_EG_UpstreamPath#01" dir="E2B">
                    //      <event name="L3_B_UpstreamPath#01" trigger="true" />
                    //    </eventgroup>
                    //</trx>

                    //<event name="L3_B_UpstreamPath#01" devicecode="B" address="0x0000B00" points="32">
                    //  <item name="UpstreamPath#01UpstreamInline" offset="0" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01UpstreamTrouble" offset="1" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SendReady" offset="2" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01Send" offset="3" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01JobTransfer" offset="4" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SendCancel" offset="5" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01ExchangeExecute" offset="6" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01DoubleGlass" offset="7" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SendJobReserve" offset="8" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SendOK" offset="9" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01PinUpRequest" offset="13" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01PinDownComplete" offset="14" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#01" offset="16" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#02" offset="17" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#03" offset="18" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#04" offset="19" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#05" offset="20" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#06" offset="21" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01GlassCount#01" offset="22" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01GlassCount#02" offset="23" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01GlassCount#03" offset="24" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01GlassCount#04" offset="25" points="1" expression="BIT" />
                    //</event>

                    #endregion
					#region [variable declare]
					string up_UpstreamInline = "0";
					string up_UpstreamTrouble = "0";
					string up_SendReady = "0";
					string up_Send = "0";
					string up_JobTransfer = "0";
					string up_SendCancel = "0";
					string up_ExchangeExecute = "0";
					string up_DoubleGlass = "0";
					string up_SendJobReserve = "0";
					string up_SendOK = "0";
					string up_ReceiveOK = "0";
					string up_PinUpRequest = "0";
					string up_PinDownComplete = "0";
					string up_SlotNumber01 = "0";
					string up_SlotNumber02 = "0";
					string up_SlotNumber03 = "0";
					string up_SlotNumber04 = "0";
					string up_SlotNumber05 = "0";
					string up_SlotNumber06 = "0";
					string up_GlassCount01 = "0";
					string up_GlassCount02 = "0";
					string up_GlassCount03 = "0";
					string up_GlassCount04 = "0";
					#endregion
					if (fabtype == eFabType.CF.ToString())
                    {
						#region CF
						up_UpstreamInline = upStream_Trx.EventGroups[0].Events[0].Items[0].Value;
						up_UpstreamTrouble = upStream_Trx.EventGroups[0].Events[0].Items[1].Value;
						up_SendReady = upStream_Trx.EventGroups[0].Events[0].Items[2].Value;
						up_Send = upStream_Trx.EventGroups[0].Events[0].Items[3].Value;
						up_JobTransfer = upStream_Trx.EventGroups[0].Events[0].Items[4].Value;
						up_SendCancel = upStream_Trx.EventGroups[0].Events[0].Items[5].Value;
						up_ExchangeExecute = upStream_Trx.EventGroups[0].Events[0].Items[6].Value;
						up_DoubleGlass = upStream_Trx.EventGroups[0].Events[0].Items[7].Value;
						up_SendJobReserve = upStream_Trx.EventGroups[0].Events[0].Items[8].Value;
						up_ReceiveOK = upStream_Trx.EventGroups[0].Events[0].Items[9].Value;
						up_SlotNumber01 = upStream_Trx.EventGroups[0].Events[0].Items[10].Value;
						up_SlotNumber02 = upStream_Trx.EventGroups[0].Events[0].Items[11].Value;
						up_SlotNumber03 = upStream_Trx.EventGroups[0].Events[0].Items[12].Value;
						up_SlotNumber04 = upStream_Trx.EventGroups[0].Events[0].Items[13].Value;
						up_SlotNumber05 = upStream_Trx.EventGroups[0].Events[0].Items[14].Value;
						up_SlotNumber06 = upStream_Trx.EventGroups[0].Events[0].Items[15].Value;
						#endregion

					}
                    //20160204 add for Cell CDR LinkSignal Trx
                    else if (Workbench.LineType == eLineType.CELL.CCPDR || Workbench.LineType == eLineType.CELL.CCGAP || Workbench.LineType == eLineType.CELL.CCPTH
                    || Workbench.LineType == eLineType.CELL.CCTAM)
                    {
                        #region [ Cell PDR,GAP,PTH,TAM Use ]

                        #region [ Trx Structure ]

                        //<trx name="L3_UpstreamPath#01" triggercondition="change">
                        //    <eventgroup name="L3_EG_UpstreamPath#01" dir="E2B">
                        //      <event name="L3_B_UpstreamPath#01" trigger="true" />
                        //    </eventgroup>
                        //  </trx>

                        //<event name="L3_B_UpstreamPath#01" devicecode="B" address="0x00011F0" points="32">
                        //0   <item name="UpstreamPath#01UpstreamInline" offset="0" points="1" expression="BIT" />
                        //1   <item name="UpstreamPath#01UpstreamTrouble" offset="1" points="1" expression="BIT" />
                        //2   <item name="UpstreamPath#01SendReady" offset="2" points="1" expression="BIT" />
                        //3   <item name="UpstreamPath#01Send" offset="3" points="1" expression="BIT" />
                        //4   <item name="UpstreamPath#01JobTransfer" offset="4" points="1" expression="BIT" />
                        //5   <item name="UpstreamPath#01SendCancel" offset="5" points="1" expression="BIT" />
                        //6   <item name="UpstreamPath#01ExchangeExecute" offset="6" points="1" expression="BIT" />
                        //7   <item name="UpstreamPath#01DoubleGlass" offset="7" points="1" expression="BIT" />
                        //8   <item name="UpstreamPath#01SendOK" offset="9" points="1" expression="BIT" />
                        //9   <item name="UpstreamPath#01PinUpRequest" offset="10" points="1" expression="BIT" />
                        //10  <item name="UpstreamPath#01PinDownComplete" offset="11" points="1" expression="BIT" />
                        //11  <item name="UpstreamPath#01SlotNumber#01" offset="13" points="1" expression="BIT" />
                        //12  <item name="UpstreamPath#01SlotNumber#02" offset="14" points="1" expression="BIT" />
                        //13  <item name="UpstreamPath#01SlotNumber#03" offset="15" points="1" expression="BIT" />
                        //14  <item name="UpstreamPath#01SlotNumber#04" offset="16" points="1" expression="BIT" />
                        //15  <item name="UpstreamPath#01SlotNumber#05" offset="17" points="1" expression="BIT" />
                        //16  <item name="UpstreamPath#01SlotNumber#06" offset="18" points="1" expression="BIT" />
                        //17  <item name="UpstreamPath#01PanelPosition#01" offset="24" points="1" expression="BIT" />
                        //18  <item name="UpstreamPath#01PanelPosition#02" offset="25" points="1" expression="BIT" />
                        //19  <item name="UpstreamPath#01PanelPosition#03" offset="26" points="1" expression="BIT" />
                        //20  <item name="UpstreamPath#01PanelPosition#04" offset="27" points="1" expression="BIT" />
                        //21  <item name="UpstreamPath#01PanelPosition#05" offset="28" points="1" expression="BIT" />
                        //22  <item name="UpstreamPath#01PanelPosition#06" offset="29" points="1" expression="BIT" />
                        //23  <item name="UpstreamPath#01PanelPosition#07" offset="30" points="1" expression="BIT" />
                        //24  <item name="UpstreamPath#01PanelPosition#08" offset="31" points="1" expression="BIT" />
                        //</event>

                        #endregion

                        up_UpstreamInline = upStream_Trx.EventGroups[0].Events[0].Items[0].Value;
                        up_UpstreamTrouble = upStream_Trx.EventGroups[0].Events[0].Items[1].Value;
                        up_SendReady = upStream_Trx.EventGroups[0].Events[0].Items[2].Value;
                        up_Send = upStream_Trx.EventGroups[0].Events[0].Items[3].Value;
                        up_JobTransfer = upStream_Trx.EventGroups[0].Events[0].Items[4].Value;
                        up_SendCancel = upStream_Trx.EventGroups[0].Events[0].Items[5].Value;
                        up_ExchangeExecute = upStream_Trx.EventGroups[0].Events[0].Items[6].Value;
                        up_DoubleGlass = upStream_Trx.EventGroups[0].Events[0].Items[7].Value;
                        //PDR 無此Item
                        //up_SendJobReserve = upStream_Trx.EventGroups[0].Events[0].Items[8].Value;
                        up_SendOK = upStream_Trx.EventGroups[0].Events[0].Items[9].Value;
                        up_PinUpRequest = upStream_Trx.EventGroups[0].Events[0].Items[10].Value;
                        up_PinDownComplete = upStream_Trx.EventGroups[0].Events[0].Items[11].Value;
                        up_SlotNumber01 = upStream_Trx.EventGroups[0].Events[0].Items[13].Value;
                        up_SlotNumber02 = upStream_Trx.EventGroups[0].Events[0].Items[14].Value;
                        up_SlotNumber03 = upStream_Trx.EventGroups[0].Events[0].Items[15].Value;
                        up_SlotNumber04 = upStream_Trx.EventGroups[0].Events[0].Items[16].Value;
                        up_SlotNumber05 = upStream_Trx.EventGroups[0].Events[0].Items[17].Value;
                        up_SlotNumber06 = upStream_Trx.EventGroups[0].Events[0].Items[18].Value;
                        //PDR 無此Item
                        //up_GlassCount01 = upStream_Trx.EventGroups[0].Events[0].Items[18].Value;
                        //up_GlassCount02 = upStream_Trx.EventGroups[0].Events[0].Items[19].Value;
                        //up_GlassCount03 = upStream_Trx.EventGroups[0].Events[0].Items[20].Value;
                        //up_GlassCount04 = upStream_Trx.EventGroups[0].Events[0].Items[21].Value;

                        #endregion
                    }
                    else
                    {
                        #region [default]
                        up_UpstreamInline = upStream_Trx.EventGroups[0].Events[0].Items[0].Value;
                        up_UpstreamTrouble = upStream_Trx.EventGroups[0].Events[0].Items[1].Value;
                        up_SendReady = upStream_Trx.EventGroups[0].Events[0].Items[2].Value;
                        up_Send = upStream_Trx.EventGroups[0].Events[0].Items[3].Value;
                        up_JobTransfer = upStream_Trx.EventGroups[0].Events[0].Items[4].Value;
                        up_SendCancel = upStream_Trx.EventGroups[0].Events[0].Items[5].Value;
                        up_ExchangeExecute = upStream_Trx.EventGroups[0].Events[0].Items[6].Value;
                        up_DoubleGlass = upStream_Trx.EventGroups[0].Events[0].Items[7].Value;
                        up_SendJobReserve = upStream_Trx.EventGroups[0].Events[0].Items[8].Value;
                        up_SendOK = upStream_Trx.EventGroups[0].Events[0].Items[9].Value;
                        up_PinUpRequest = upStream_Trx.EventGroups[0].Events[0].Items[10].Value;
                        up_PinDownComplete = upStream_Trx.EventGroups[0].Events[0].Items[11].Value;
                        up_SlotNumber01 = upStream_Trx.EventGroups[0].Events[0].Items[12].Value;
                        up_SlotNumber02 = upStream_Trx.EventGroups[0].Events[0].Items[13].Value;
                        up_SlotNumber03 = upStream_Trx.EventGroups[0].Events[0].Items[14].Value;
                        up_SlotNumber04 = upStream_Trx.EventGroups[0].Events[0].Items[15].Value;
                        up_SlotNumber05 = upStream_Trx.EventGroups[0].Events[0].Items[16].Value;
                        up_SlotNumber06 = upStream_Trx.EventGroups[0].Events[0].Items[17].Value;
                        up_GlassCount01 = upStream_Trx.EventGroups[0].Events[0].Items[18].Value;
                        up_GlassCount02 = upStream_Trx.EventGroups[0].Events[0].Items[19].Value;
                        up_GlassCount03 = upStream_Trx.EventGroups[0].Events[0].Items[20].Value;
                        up_GlassCount04 = upStream_Trx.EventGroups[0].Events[0].Items[21].Value;
                        #endregion
                    }
                    #endregion
                    if (line.Data.LINEID == "TCCVD300" || line.Data.LINEID == "TCCVD400")//add by hujunpeng 20190527 for CVD300 get ready PM12
                    {
                        if (curStage.Data.STAGEID == "12" &&curRobot.CurRealTimeArmSingleJobInfoList[0].ArmJobExist == eGlassExist.NoExist && curRobot.CurRealTimeArmSingleJobInfoList[1].ArmJobExist == eGlassExist.NoExist)
                        {
                            if (up_UpstreamInline == bitOn && up_SendReady == bitOn&&up_Send==bitOff&&curRobot.File.CurRobotPosition!="12")
                            {
                                RobotCmdInfo curRobotCommand = new RobotCmdInfo();
                                curRobotCommand.Cmd01_Command = eRobot_ControlCommand.GET_READY;
                                curRobotCommand.Cmd01_ArmSelect = eRobot_ArmSelect.UPPER;
                                curRobotCommand.Cmd01_TargetPosition = 12;
                                curRobotCommand.Cmd01_TargetSlotNo = 1;
                                curRobotCommand.Cmd02_Command = eRobot_ControlCommand.NONE;
                                Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curRobotCommand });
                                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("get ready for 12,cur robot command[{0}],cur robot arm[{1}],cur robot target position[{2}]", curRobotCommand.Cmd01_Command, curRobotCommand.Cmd01_ArmSelect, curRobotCommand.Cmd01_TargetPosition));
                            }
                        }  
                    }
                    if (up_UpstreamInline == bitOn && up_SendReady == bitOn && up_Send == bitOn)
                    {

                        //Edit By Yangzhenteng 20191022 For CVD Send Delay N Seconds
                        // #region[CVD_ULVAC CVD Send ON Dealy N Seconds]
                        // bool CVD_ULVAC_DELAYUSEFLAG = ParameterManager.ContainsKey("CVD_ULVAC_DELAYUSEFLAG") ? ParameterManager["CVD_ULVAC_DELAYUSEFLAG"].GetBoolean() : false;
                        //if ((line.Data.LINEID.Contains("TCCVD100") || line.Data.LINEID.Contains("TCCVD200")) && CVD_ULVAC_DELAYUSEFLAG)
                        //{
                        //    int CVDDelayTime = ParameterManager.ContainsKey("CVD_ULVAC_DELAYTIME") ? ParameterManager["CVD_ULVAC_DELAYTIME"].GetInteger() : 0;
                        //    if ((curStage.Data.STAGEID == "13" || curStage.Data.STAGEID == "14") && (curRobot.CurRealTimeArmSingleJobInfoList[0].ArmJobExist == eGlassExist.Exist || curRobot.CurRealTimeArmSingleJobInfoList[1].ArmJobExist == eGlassExist.Exist))
                        //    {
                        //        Thread.Sleep(CVDDelayTime);
                        //        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("CVD_ULVAC CVD Send And Send Ready Bit On,Delay [{0}] Seconds UpdateStatus Change",CVDDelayTime));
                        //    }
                        // }
                        //#endregion

                        #region [ 20151026 與京光討論後有上報則以有上報SlotNo為準,沒有上報則以1為準 ]

                        #region [ 將SlotNo Bit 轉成Int ]

                        strSlotNoBin = up_SlotNumber06 + up_SlotNumber05 + up_SlotNumber04 + up_SlotNumber03 + up_SlotNumber02 + up_SlotNumber01;

                        try
                        {
                            slotNo = Convert.ToInt32(strSlotNoBin, 2);
                        }
                        catch (Exception ex1)
                        {
                            slotNo = 0;
                        }

                        #endregion

                        //20160720
                        fail_ReasonCode = eRobot_CheckFail_Reason.UPSTREAM_SLOTNO_IS_ZERO;
                        if (slotNo == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report SlotNo is 0!",
                                                        curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                        trxID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //20160720  TTP Buffer ,stage12 SlotNo=0,不要給預設1,跳failMsg警告
                            if (curStage.Data.STAGEID.Trim() == "12" && (Workbench.LineType == eLineType.CF.FCMQC_TYPE1) ) //qiumin 20180203
                            {
                                if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report SlotNo is 0!",
                                                            curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                            trxID);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    failMsg = string.Format("RtnCode({0}) RtnMsg([{1}] StageID({2}) StageName({3}) TrxID({4}) SlotNo is 0!)",
                                                            fail_ReasonCode, MethodBase.GetCurrentMethod().Name, curStage.Data.STAGEID, curStage.Data.STAGENAME, trxID);
                                    AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                }
                            }
                            else
                            {
                                //沒有上報則預設為1
                                slotNo = 1;
                            }
                        }
                        //20160720 清除SlotNo=0的failMsg
                        else
                        {
                            RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                        }

                        #endregion

                        //LinkSignal Bit符合出片條件
                        #region  [ 取得JobData ]

                        Job curSendOutJob = new Job();

                        if (Get_LinkSignalSendOutJobInfo_ForSingleSlot(curRobot, curStage, curStageCanControlJobList, slotNo, out curSendOutJob) == true)
                        {

                            #region [ EQP SendOut 1 Job ]

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}) SendOutSlotNo({8}),Stage UDRQ Status change to (UDRQ)!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                        trxID, eBitResult.ON.ToString(), eBitResult.ON.ToString(), eBitResult.ON.ToString(), slotNo.ToString());
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //20150930 add for SendOut Job SlotNo
                            lock (curStage)
                            {
                                //20151026 modify 改為根據SendOut SlotNo來決定
                                //if (curStage.curUDRQ_SlotList.ContainsKey(curSendOutJob.RobotWIP.CurLocation_SlotNo) == false)
                                //{
                                //    curStage.curUDRQ_SlotList.Add(curSendOutJob.RobotWIP.CurLocation_SlotNo, string.Format("{0}_{1}", curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo));
                                //}
                                if (curStage.curUDRQ_SlotList.ContainsKey(slotNo) == false)
                                {
                                    curStage.curUDRQ_SlotList.Add(slotNo, string.Format("{0}_{1}", curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo));
                                }
                            }

                            //Update Status UDRQ Stage Change To UDRQ
                           
                            UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo);

                            #endregion

                            return;

                        }
                        else
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) can not get SendOut JobData, Stage UDRQ Status change from ({3}) to ({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.File.CurStageStatus,
                                                        eRobotStageStatus.NO_REQUEST);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //
                            if (curRobot.File.curRobotRunMode == eRobot_RunMode.AUTO_MODE)
                            {
                                //無SendOut Job Info Status UDRQ Stage Change To NOREQ
                                UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty);
                            }
                            else
                            {

                                //SEMI Mode 如果找不到WIP還是視同可以出片.Update Status UDRQ Stage Change To UDRQ
                                UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, string.Empty, string.Empty);

                            }

                        }

                        #endregion

                    }
                    else
                    {

                        //Monitor 條件不合的狀態 Status UDRQ Stage Change To NOREQ
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}), Stage UDRQ Status can not change to (UDRQ)!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID, (eBitResult)int.Parse(up_UpstreamInline), (eBitResult)int.Parse(up_SendReady), (eBitResult)int.Parse(up_Send));
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty);

                    }

                }

            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }


        }

        /// <summary> 取得Stage Type Stage,Slot is Single Can ControlJobList for 1 Arm 1Job Use . 
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        private void Get_StageTypeSingleSlot_CanControlJobList_For1Arm1Job(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList)
        {
            string trxID = string.Empty;
            string strlog = string.Empty;

            FCSRT_RobotParam srt_param = null;
            if (Workbench.LineType == eLineType.CF.FCSRT_TYPE1)
            {
                #region Check Static Context
                if (!StaticContext.ContainsKey(eRobotContextParameter.FCSRT_RobotParam))
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "StaticContext is not Contains FCSRT_RobotParam");
                    return;
                }
                if (!(StaticContext[eRobotContextParameter.FCSRT_RobotParam] is FCSRT_RobotParam))
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "StaticContext is not Contains FCSRT_RobotParam");
                    return;
                }
                #endregion
                srt_param = (FCSRT_RobotParam)StaticContext[eRobotContextParameter.FCSRT_RobotParam];
            }
            
            try
            {
                string bitOn = "1";
                string bitOff = "0";
                string funcName = MethodBase.GetCurrentMethod().Name;

                #region [Get line fabtyep]
                string fabtype = eFabType.ARRAY.ToString();
                Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if (line != null)
                {
                    fabtype = line.Data.FABTYPE;
                }
                #endregion

                #region [ Check Trx Setting ]
                if (curStage.Data.STAGEREPORTTRXNAME.Trim() == string.Empty)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find STAGEREPORTTRXNAME setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return;
                }
                #endregion

                //interface時Stage出片要看Upstream .透過LinkSignal已經得知JobData是否填好 Send Signal On =JobData Exist
                string upStreamTrx = curStage.Data.STAGEREPORTTRXNAME;
                int slotNo = 0;
                int glassCount = 0;

                #region  real time Get Interface Upstream

                trxID = upStreamTrx;

                Trx upStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                if (upStream_Trx == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                }

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4})!",
                                            curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                            trxID);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L2_Stage#01SinglePositionReport" triggercondition="change">
                //    <eventgroup name="L2_EG_Stage#01SinglePositionReport" dir="E2B">
                //      <event name="L2_B_Stage#01SendReady" trigger="true" />
                //      <event name="L2_B_Stage#01ReceiveReady" trigger="true" />
                //      <event name="L2_B_Stage#01DoubleGlassExist" trigger="true" />
                //      <event name="L2_B_Stage#01ExchangePossible" trigger="true" />
                //    </eventgroup>
                //</trx>

                #endregion
                #region [variable declare]
                string SendReady = "0";
                string ReceiveReady = "0";
                string DoubleGlassExist = "0";
                string ExchangePossible = "0";
                #endregion
                if (fabtype == eFabType.CF.ToString() || fabtype == eFabType.ARRAY.ToString())
                {
                    SendReady = upStream_Trx.EventGroups[0].Events[0].Items[0].Value;
                    ReceiveReady = upStream_Trx.EventGroups[0].Events[1].Items[0].Value;
                    DoubleGlassExist = upStream_Trx.EventGroups[0].Events[2].Items[0].Value;
                    ExchangePossible = upStream_Trx.EventGroups[0].Events[3].Items[0].Value;
                }
                else
                {
                    ;
                }
                #endregion

                if (SendReady == bitOn || ExchangePossible == bitOn)
                {
                    slotNo = 1;

                    //LinkSignal Bit符合出片條件
                    #region  [ 取得JobData ]

                    Job curSendOutJob = new Job();

                    if (Get_StageSendOutJobInfo_ForSingleSlot(curRobot, curStage, curStageCanControlJobList, slotNo, out curSendOutJob) == true)
                    {

                        #region [ EQP SendOut 1 Job ]

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}) SendOutSlotNo({8}),Stage UDRQ Status change to (UDRQ)!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID, eBitResult.ON.ToString(), eBitResult.ON.ToString(), eBitResult.ON.ToString(), slotNo.ToString());
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        if (Workbench.LineType == eLineType.CF.FCSRT_TYPE1)
                        {
                            #region FCSRT 需要判斷兩個 VCR Stage 是否可以 GetGet
                            if (srt_param.GetSendReadyOnTime(curStage.Data.STAGEID) == DateTime.MinValue)
                                srt_param.SetSendReadyOnTime(curStage.Data.STAGEID, DateTime.Now);

                            if (srt_param.CheckVCRStageGetGet())
                            {
                                //20150930 add for SendOut Job SlotNo
                                lock (curStage)
                                {
                                    if (curStage.curUDRQ_SlotList.ContainsKey(slotNo) == false)
                                    {
                                        curStage.curUDRQ_SlotList.Add(slotNo, string.Format("{0}_{1}", curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo));
                                    }
                                }
                            }
                            else
                            {
                                // VCR Stage 還不能 Get Get 且尚未 Timeout, StageCanControlJobList 暫時移除
                                // 等到可以 Get Get 或 Timeout 時才加入
                                if (curSendOutJob != null)
                                    curStageCanControlJobList.Remove(curSendOutJob);
                            }
                            #endregion
                        }
                        else
                        {
                            //20150930 add for SendOut Job SlotNo
                            lock (curStage)
                            {
                                if (curStage.curUDRQ_SlotList.ContainsKey(slotNo) == false)
                                {
                                    curStage.curUDRQ_SlotList.Add(slotNo, string.Format("{0}_{1}", curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo));
                                }
                            }
                        }

                        //Update Status UDRQ Stage Change To UDRQ
                        UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo);

                        #endregion

                        return;
                    }
                    else
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) can not get SendOut JobData, Stage UDRQ Status change from ({3}) to ({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.File.CurStageStatus,
                                                    eRobotStageStatus.NO_REQUEST);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        if (curRobot.File.curRobotRunMode == eRobot_RunMode.AUTO_MODE)
                        {
                            //無SendOut Job Info Status UDRQ Stage Change To NOREQ
                            UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty);
                        }
                        else
                        {
                            //SEMI Mode 如果找不到WIP還是視同可以出片.Update Status UDRQ Stage Change To UDRQ
                            UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, string.Empty, string.Empty);
                        }
                    }
                    #endregion
                }
                else
                {
                    //Monitor 條件不合的狀態 Status UDRQ Stage Change To NOREQ
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        //strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}), Stage UDRQ Status can not change to (UDRQ)!",
                                                //curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                //trxID, (eBitResult)int.Parse(up_UpstreamInline), (eBitResult)int.Parse(up_SendReady), (eBitResult)int.Parse(up_Send));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> 取得LinkSignal SendOut的 JobData for Stage is Single Slot
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        /// <param name="sendOutSlotNo"></param>
        /// <param name="curSendOutJob"></param>
        /// <returns></returns>
        private bool Get_LinkSignalSendOutJobInfo_ForSingleSlot(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList, int sendOutSlotNo, out Job curSendOutJob)
        {
            string strlog = string.Empty;
            string jobKey = string.Empty;
            Job returnJob = new Job();
            curSendOutJob = returnJob;
            //Signal SlotNo 通通為1
            //20151026 Modify 改為看SlotNo Bit
            //int sendOutSlotNo = 1;

            try
            {

                string funcName = MethodBase.GetCurrentMethod().Name;

                #region [ Check Trx Setting Exist ]

                if (curStage.Data.UPSTREAMJOBDATAPATHTRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) can not find SendOutJobData TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;

                }

                //取得SendOut的TrxID
                string trxID = curStage.Data.UPSTREAMJOBDATAPATHTRXNAME.Trim();

                if (trxID == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) can not find SendOutJobData TrxID({4})!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.UPSTREAMJOBDATAPATHTRXNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }

                #endregion

                #region  real time Get Trx by sendOutSlotNo

                Trx GetJobData_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                if (GetJobData_Trx == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;

                }

                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L3_SendOutJobDataReport#01" triggercondition="change">
                //    <eventgroup name="L3_EG_SendOutJobDataReport#01" dir="E2B">
                //      <event name="L3_W_SendOutJobDataBlock_JobData1" />
                //      <event name="L3_B_SendOutJobDataReport#01" trigger="true" />
                //    </eventgroup>
                //  </trx>

                //<itemgroup name="JobData">
                //    <item name="CassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //    <item name="JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //    <item name="GroupIndex" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //    <item name="ProductType" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //    <item name="CSTOperationMode" woffset="4" boffset="0" wpoints="1" bpoints="2" expression="INT" />
                //    <item name="SubstrateType" woffset="4" boffset="2" wpoints="1" bpoints="2" expression="INT" />
                //    <item name="CIMMode" woffset="4" boffset="4" wpoints="1" bpoints="1" expression="INT" />
                //    <item name="JobType" woffset="4" boffset="5" wpoints="1" bpoints="4" expression="INT" />
                //    <item name="JobJudge" woffset="4" boffset="9" wpoints="1" bpoints="4" expression="INT" />
                //    <item name="SamplingSlotFlag" woffset="4" boffset="13" wpoints="1" bpoints="1" expression="BIN" />
                //    <item name="FirstRunFlag" woffset="4" boffset="14" wpoints="1" bpoints="1" expression="BIN" />
                //    <item name="JobGrade" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="ASCII" />
                //    <item name="Glass/Chip/MaskID/BlockID" woffset="6" boffset="0" wpoints="10" bpoints="160" expression="ASCII" />
                //    <item name="PPID" woffset="16" boffset="0" wpoints="25" bpoints="400" expression="ASCII" />
                //    <item name="GlassFlowType" woffset="41" boffset="0" wpoints="1" bpoints="6" expression="INT" />
                //    <item name="ProcessType" woffset="41" boffset="6" wpoints="1" bpoints="6" expression="INT" />
                //    <item name="LastGlassFlag" woffset="41" boffset="12" wpoints="1" bpoints="1" expression="BIN" />
                //    <item name="RTCFlag" woffset="41" boffset="13" wpoints="1" bpoints="1" expression="BIN" />
                //    <item name="MainEQInFlag" woffset="41" boffset="14" wpoints="1" bpoints="1" expression="BIN" />
                //    <item name="Insp.JudgedData" woffset="42" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
                //    <item name="TrackingData" woffset="44" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
                //    <item name="EQPFlag" woffset="46" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
                //    <item name="ChipCount" woffset="48" boffset="0" wpoints="1" bpoints="9" expression="INT" />
                //    <item name="RecipeGroupNumber" woffset="48" boffset="10" wpoints="1" bpoints="6" expression="INT" />
                //    <item name="SourcePortNo" woffset="49" boffset="0" wpoints="1" bpoints="8" expression="INT" />
                //    <item name="TargetPortNo" woffset="49" boffset="8" wpoints="1" bpoints="8" expression="INT" />
                //</itemgroup>

                string cstSeq = GetJobData_Trx.EventGroups[0].Events[0].Items["CassetteSequenceNo"].Value;
                string jobSeq = GetJobData_Trx.EventGroups[0].Events[0].Items["JobSequenceNo"].Value;
                string sendOut_TrackingData = GetJobData_Trx.EventGroups[0].Events[0].Items["TrackingData"].Value;
                string sendOut_JobJudge = GetJobData_Trx.EventGroups[0].Events[0].Items["JobJudge"].Value;
                string eqp_FLAG = GetJobData_Trx.EventGroups[0].Events[0].Items["EQPFlag"].Value;   //Watson add 20151019 For TTP DailyCheck
                string rework_real_count = (GetJobData_Trx.EventGroups[0].Events[0].Items["ReworkRealCount"] != null) ? GetJobData_Trx.EventGroups[0].Events[0].Items["ReworkRealCount"].Value : string.Empty;
                string eqp_Reservations = string.Empty;
                string cfSpecialReserved_ForcePSHbit= string.Empty;
                if (Workbench.ServerName.Length > 2)
                {
                    if (Workbench.ServerName.Substring(0, 2).ToUpper() == "FC")
                    {
                        eqp_Reservations = GetJobData_Trx.EventGroups[0].Events[0].Items["EQPReservations"].Value;   //Watson add 20151207 For CF MQC_TTP DailyCheck
                       string cf_Special_Reserved= cf_Special_Reserved = GetJobData_Trx.EventGroups[0].Events[0].Items["CFSpecialReserved"].Value;
                       cfSpecialReserved_ForcePSHbit= string.IsNullOrEmpty(cf_Special_Reserved) ? "0" : cf_Special_Reserved.Substring(0,1);
                    }

                }
                #endregion

                #endregion

                if (cstSeq != "0" && jobSeq != "0")
                {
                    jobKey = string.Format("{0}_{1}", cstSeq.ToString(), jobSeq.ToString());

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4}) ,CSTSeq({5}) JobSeq({6}) JobKey({7}).",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, cstSeq, jobSeq, jobKey);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Check Job is Exist ]

                    Job curBcsJob = ObjectManager.JobManager.GetJob(cstSeq, jobSeq);

                    if (curBcsJob == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBMRCS Robot({1}) StageID({2}) StageName({3}) Can not Get Job by CSTSeq({4}) JobSeq({5})!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    cstSeq, jobSeq);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //20160603
                        #region [當curBcsJob找不到時,必須再判斷是不是AbnormalForceOut,產生新Job]
                        string errMsg = "";
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

                            errMsg = string.Format("[{0}] Robot({1}) can not Get Line Entity!",
                                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);


                            return false;
                        }

                        #endregion

                        if (robotLine.Data.FABTYPE == eFabType.ARRAY.ToString() && robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.ABNORMAL_FORCE_CLEAN_OUT_MODE)
                        {
                            //沒有帳,產生一片新Job帳
                            curBcsJob = (Job)Invoke(eServiceName.JobService, "NewJob", new object[] { robotLine.Data.LINEID, cstSeq, jobSeq, GetJobData_Trx });
                            if (curBcsJob == null)
                            {
                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBMRCS Robot({1}) StageID({2}) StageName({3}) Can not Get Job by CSTSeq({4}) JobSeq({5})!",
                                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                            cstSeq, jobSeq);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                return false;
                            }
                            else
                            {
                                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curStage.Data.NODENO);
                                object[] parameters = null;
                                bool result =false;
                                if (curStage.Data.STAGETYPE == eRobotStageType.ROBOTARM)
                                {
                                    parameters = new object[] { eqp.Data.NODENO, curBcsJob,2,65535,errMsg};
                                    result = (bool)Invoke(eServiceName.RobotCoreService, "CreateAbnormalForceCleanOutJobRobotWIPInfo", parameters,
                                    new Type[] { typeof(string), typeof(Job),typeof(int),typeof(int),typeof(string).MakeByRefType()});

                                }
                                else if (curStage.Data.STAGETYPE == eRobotStageType.EQUIPMENT || curStage.Data.STAGETYPE == eRobotStageType.STAGE)
                                {
                                    parameters = new object[] { eqp.Data.NODENO, curBcsJob,1,2,errMsg};
                                    result = (bool)Invoke(eServiceName.RobotCoreService, "CreateAbnormalForceCleanOutJobRobotWIPInfo", parameters,
                                    new Type[] { typeof(string), typeof(Job),typeof(int),typeof(int),typeof(string).MakeByRefType()});
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        #endregion
                        return false; //Not In WIP
  
                    }

                    #endregion

                    #region [ Update Job RobotWIP ]

                    //有變化才紀錄Log   LinkSignal要看Send Out JobData的TrackingData .Route Priority目前直接參照ROBOTWIP內Route資訊排序即可
                    if (curBcsJob.RobotWIP.CfSpecial == null) 
                        curBcsJob.RobotWIP.CfSpecial = new JobCfSpecial();//由于之前的程式没有这个obj，更新程式如果没有重新下货或有异常
                    if (curBcsJob.RobotWIP.CurLocation_StageID != curStage.Data.STAGEID ||
                        curBcsJob.RobotWIP.CurLocation_SlotNo != sendOutSlotNo ||
                        curBcsJob.RobotWIP.CurLocation_StageType != eRobotStageType.EQUIPMENT ||
                        curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData != sendOut_TrackingData ||
                        curBcsJob.RobotWIP.CurPortCstStatusPriority != eLoaderPortSendOutStatus.NOT_IN_PORT ||
                        curBcsJob.RobotWIP.CurSendOutJobJudge != sendOut_JobJudge ||
                        curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG != eqp_FLAG ||
                        curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPRESERVATIONS != eqp_Reservations ||
                        curBcsJob.RobotWIP.ReworkRealCount != rework_real_count ||
                        curBcsJob.RobotWIP.CfSpecial.CFSpecialReserved.ForcePSHbit != cfSpecialReserved_ForcePSHbit
                        )
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Job CassetteSequenceNo({1}) JobSequenceNo({2}) Update RobotWIP curStageType from ({3}) to ({4}), curStageID from ({5}) to ({6}), curSlotNo From ({7}) to ({8}), SendOut TrackingData from ({9}) to ({10}),sendOutJobJudge from ({11}) to ({12}) PortCSTStatusPriority from ({13}) to ({14}), SendOut EQPFlag from ({15}) to ({16}) , SendOut EQPReservationse from ({17}) to ({18}).",
                                                curStage.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurLocation_StageType,
                                                eRobotStageType.EQUIPMENT, curBcsJob.RobotWIP.CurLocation_StageID, curStage.Data.STAGEID, curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(),
                                                sendOutSlotNo.ToString(), curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData, sendOut_TrackingData, curBcsJob.RobotWIP.CurSendOutJobJudge,
                                                sendOut_JobJudge, curBcsJob.RobotWIP.CurPortCstStatusPriority, eLoaderPortSendOutStatus.NOT_IN_PORT, curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG,eqp_FLAG,curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPRESERVATIONS,eqp_Reservations);
                        if (Workbench.LineType == eLineType.CF.FCPSH_TYPE1)
                        {
                            strlog = string.Format(strlog.Substring(0, strlog.Length-1) + ",SendOut cfSpecialReserved_ForcePSHbit from ({0}) to ({1}).", curBcsJob.RobotWIP.CfSpecial.CFSpecialReserved.ForcePSHbit, cfSpecialReserved_ForcePSHbit);
                        }
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {

                            curBcsJob.RobotWIP.CurLocation_StageID = curStage.Data.STAGEID;
                            curBcsJob.RobotWIP.CurLocation_SlotNo = sendOutSlotNo;
                            curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.EQUIPMENT;
                            curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData = sendOut_TrackingData;
                            curBcsJob.RobotWIP.CurPortCstStatusPriority = eLoaderPortSendOutStatus.NOT_IN_PORT;
                            curBcsJob.RobotWIP.CurSendOutJobJudge = sendOut_JobJudge;
                            curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG = eqp_FLAG;//Watson add 20151019 For TTP DailyCheck
                            curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPRESERVATIONS = eqp_Reservations; //Watson Add 20151207 For CF TTP DailyCheck
                            curBcsJob.RobotWIP.ReworkRealCount = rework_real_count;
                            curBcsJob.RobotWIP.CfSpecial.CFSpecialReserved.ForcePSHbit = cfSpecialReserved_ForcePSHbit;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                    #region [ Add to Can Control Job List ]

                    if (AddToCanControlJoblistCondition(curRobot, curStage, curBcsJob, curStageCanControlJobList, jobKey, funcName) == true)
                    {

                        #region [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) add CassetteSequenceNo({4}) JobSequenceNo({5}) CurRouteStepNo({6}) CurSlotNo({7}) to Can control List!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.CurLocation_SlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        curSendOutJob = curBcsJob;

                        return true;

                    }
                    else
                    {
                        return false;
                    }

                    #endregion

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4}) ,CSTSeq({5}) JobSeq({6}) Job is not Exist!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, cstSeq, jobSeq);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
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

        /// <summary> 取得Stage SendOut的 JobData for Stage is Single Slot
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        /// <param name="sendOutSlotNo"></param>
        /// <param name="curSendOutJob"></param>
        /// <returns></returns>
        private bool Get_StageSendOutJobInfo_ForSingleSlot(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList, int sendOutSlotNo, out Job curSendOutJob)
        {
            string strlog = string.Empty;
            string jobKey = string.Empty;
            Job returnJob = new Job();
            curSendOutJob = returnJob;
            try
            {

                string funcName = MethodBase.GetCurrentMethod().Name;

                #region [ Check Trx Setting Exist ]

                if (curStage.Data.STAGEJOBDATATRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) can not find SendOutJobData TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;

                }

                //取得SendOut的TrxID
                string trxID = curStage.Data.STAGEJOBDATATRXNAME.Trim();

                if (trxID == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) can not find SendOutJobData TrxID({4})!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.UPSTREAMJOBDATAPATHTRXNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }

                #endregion

                #region  real time Get Trx by sendOutSlotNo
                Trx GetJobData_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                if (GetJobData_Trx == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }
                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L2_Stage#01PositionInfoReportBlock" triggercondition="change">
                //    <eventgroup name="L2_EG_Stage#01PositionInfoReportBlock" dir="E2B">
                //      <event name="L2_W_Stage#01PositionInfoReportBlock" />
                //    </eventgroup>
                //</trx>

                //<itemgroup name="Stage#01PositionInfoReportBlock">
                //    <item name="StageJobCassetteSequenceNumber" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //    <item name="StageJobSlotSequenceNumber" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //    <item name="StageJobExist" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>

                string cstSeq = GetJobData_Trx.EventGroups[0].Events[0].Items[0].Value;
                string jobSeq = GetJobData_Trx.EventGroups[0].Events[0].Items[1].Value;
                string jobexist = GetJobData_Trx.EventGroups[0].Events[0].Items[2].Value;
                #endregion

                #endregion

                if (jobexist == "2" && cstSeq != "0" && jobSeq != "0")
                {
                    jobKey = string.Format("{0}_{1}", cstSeq.ToString(), jobSeq.ToString());

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4}) ,CSTSeq({5}) JobSeq({6}) JobKey({7}).",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, cstSeq, jobSeq, jobKey);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Check Job is Exist ]

                    Job curBcsJob = ObjectManager.JobManager.GetJob(cstSeq, jobSeq);

                    if (curBcsJob == null)
                    {
                        //20160603
                        #region [當curBcsJob找不到時,必須再判斷是不是AbnormalForceOut,產生新Job]
                        string errMsg = "";
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

                            errMsg = string.Format("[{0}] Robot({1}) can not Get Line Entity!",
                                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);


                            return false;
                        }

                        #endregion

                        if (robotLine.Data.FABTYPE == eFabType.ARRAY.ToString() && robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.ABNORMAL_FORCE_CLEAN_OUT_MODE)
                        {
                            //沒有帳,產生一片新Job帳
                            curBcsJob = (Job)Invoke(eServiceName.JobService, "NewJob", new object[] { robotLine.Data.LINEID, cstSeq, jobSeq, GetJobData_Trx });
                            if (curBcsJob == null)
                            {
                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBMRCS Robot({1}) StageID({2}) StageName({3}) Can not Get Job by CSTSeq({4}) JobSeq({5})!",
                                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                            cstSeq, jobSeq);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                return false;
                            }
                            else
                            {
                                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curStage.Data.NODENO);
                                object[] parameters = null;
                                bool result = false;
                                if (curStage.Data.STAGETYPE == eRobotStageType.ROBOTARM)
                                {
                                    parameters = new object[] { eqp.Data.NODENO, curBcsJob, 2, 65535, errMsg };
                                    result = (bool)Invoke(eServiceName.RobotCoreService, "CreateAbnormalForceCleanOutJobRobotWIPInfo", parameters,
                                    new Type[] { typeof(string), typeof(Job), typeof(int), typeof(int), typeof(string).MakeByRefType() });

                                }
                                else if (curStage.Data.STAGETYPE == eRobotStageType.EQUIPMENT || curStage.Data.STAGETYPE == eRobotStageType.STAGE)
                                {
                                    parameters = new object[] { eqp.Data.NODENO, curBcsJob, 1, 2, errMsg };
                                    result = (bool)Invoke(eServiceName.RobotCoreService, "CreateAbnormalForceCleanOutJobRobotWIPInfo", parameters,
                                    new Type[] { typeof(string), typeof(Job), typeof(int), typeof(int), typeof(string).MakeByRefType() });
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        #endregion
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBMRCS Robot({1}) StageID({2}) StageName({3}) Can not Get Job by CSTSeq({4}) JobSeq({5})!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    cstSeq, jobSeq);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false; //Not In WIP

                    }

                    #endregion

                    #region [ Update Job RobotWIP ]

                    //有變化才紀錄Log
                    if (curBcsJob.RobotWIP.CurLocation_StageID != curStage.Data.STAGEID ||
                        curBcsJob.RobotWIP.CurLocation_SlotNo != sendOutSlotNo ||
                        curBcsJob.RobotWIP.CurLocation_StageType != eRobotStageType.STAGE ||
                        curBcsJob.RobotWIP.CurPortCstStatusPriority != eLoaderPortSendOutStatus.NOT_IN_PORT)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Job CassetteSequenceNo({1}) JobSequenceNo({2}) Update RobotWIP curStageType from ({3}) to ({4}), curStageID from ({5}) to ({6}), curSlotNo From ({7}) to ({8}).",
                                                curStage.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurLocation_StageType,
                                                eRobotStageType.EQUIPMENT, curBcsJob.RobotWIP.CurLocation_StageID, curStage.Data.STAGEID, curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(),
                                                sendOutSlotNo.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurLocation_StageID = curStage.Data.STAGEID;
                            curBcsJob.RobotWIP.CurLocation_SlotNo = sendOutSlotNo;
                            curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.STAGE;
                            curBcsJob.RobotWIP.CurPortCstStatusPriority = eLoaderPortSendOutStatus.NOT_IN_PORT;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                    #region [ Add to Can Control Job List ]

                    if (AddToCanControlJoblistCondition(curRobot, curStage, curBcsJob, curStageCanControlJobList, jobKey, funcName) == true)
                    {

                        #region [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) add CassetteSequenceNo({4}) JobSequenceNo({5}) CurRouteStepNo({6}) CurSlotNo({7}) to Can control List!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.CurLocation_SlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        curSendOutJob = curBcsJob;

                        return true;

                    }
                    else
                    {
                        return false;
                    }

                    #endregion

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4}) ,CSTSeq({5}) JobSeq({6}) Job is not Exist!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, cstSeq, jobSeq);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
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

        /// <summary> 取得EQP Type Stage,Slot is Signal status LDRQ的狀態與EmptySlotInfo
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        private void Get_EqpTypeSignal_LDRQStauts_For1Arm1Job(Robot curRobot, RobotStage curStage)
        {

            string trxID = string.Empty;
            string strlog = string.Empty;
            //20160720
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;

            try
            {
                string bitOn = "1";
                string bitOff = "0";
                string funcName = MethodBase.GetCurrentMethod().Name;

				#region [Get line fabtyep]
				string fabtype=eFabType.ARRAY.ToString();
				Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
				if (line != null) {
					fabtype = line.Data.FABTYPE;
				}
				#endregion

				#region [ Check Trx Setting ]

				if (curStage.Data.DOWNSTREAMPATHTRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find DownStream TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,curStage.Data.STAGENAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;

                }

                #endregion

                string strSlotNoBin = string.Empty;
                //Signal SlotNo=1
                int slotNo =0;

                #region  real time Get Interface downstream

                trxID = curStage.Data.DOWNSTREAMPATHTRXNAME.Trim();

                Trx downStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                if (downStream_Trx == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;
                }

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4})!",
                                            curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                            trxID);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L3_DownstreamPath#01" triggercondition="change">
                //    <eventgroup name="L3_EG_DownstreamPath#01" dir="E2B">
                //      <event name="L3_B_DownstreamPath#01" trigger="true" />
                //    </eventgroup>
                //  </trx>

                //<event name="L3_B_DownstreamPath#01" devicecode="B" address="0x0000A00" points="32">
                //  <item name="DownstreamPath#01DownstreamInline" offset="0" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01DownstreamTrouble" offset="1" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveAble" offset="2" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01Receive" offset="3" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01JobTransfer" offset="4" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveCancel" offset="5" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ExchangePossible" offset="6" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01DoubleGlass" offset="7" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveJobReserve" offset="8" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveOK" offset="9" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01TransferStopRequest" offset="10" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01DummyGlassRequest" offset="11" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01GlassExist" offset="12" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01PinUpComplete" offset="13" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01PinDownRequest" offset="14" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#01" offset="16" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#02" offset="17" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#03" offset="18" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#04" offset="19" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#05" offset="20" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#06" offset="21" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01GlassCount#01" offset="22" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01GlassCount#02" offset="23" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01GlassCount#03" offset="24" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01GlassCount#04" offset="25" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#01" offset="26" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#02" offset="27" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#03" offset="28" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#04" offset="29" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#05" offset="30" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#06" offset="31" points="1" expression="BIT" />
                //</event>

                #endregion
				#region [variable declare]
				string down_DownstreamInline = "0";
				string down_DownstreamTrouble = "0";
				string down_ReceiveAble = "0";
				string down_Receive = "0";
				string down_JobTransfer = "0";
				string down_ReceiveCancel = "0";
				string down_ExchangePossible = "0";
				string down_DoubleGlass = "0";
				string down_ReceiveJobReserve = "0";
				string down_ReceiveOK = "0";
				string down_TransferStopRequest = "0";
				string down_DummyGlassRequest = "0";
				string down_GlassExist = "0";
				string down_PinUpComplete = "0";
				string down_PinDownRequest = "0";
				string down_SlotNumber01 = "0";
				string down_SlotNumber02 = "0";
				string down_SlotNumber03 = "0";
				string down_SlotNumber04 = "0";
				string down_SlotNumber05 = "0";
				string down_SlotNumber06 = "0";
				string down_GlassCount01 = "0";
				string down_GlassCount02 = "0";
				string down_GlassCount03 = "0";
				string down_GlassCount04 = "0";
				string down_ReceiveType01 = "0";
				string down_ReceiveType02 = "0";
				string down_ReceiveType03 = "0";
				string down_ReceiveType04 = "0";
				string down_ReceiveType05 = "0";
				string down_ReceiveType06 = "0";
				string down_GlassCount05 = "0";
				string down_PreparationPermission = "0";
				string down_InspectionResultUpdate = "0";
				string down_ReturnMode = "0";
				#endregion
				if (fabtype==eFabType.CF.ToString())
                {
					#region CF
					down_DownstreamInline = downStream_Trx.EventGroups[0].Events[0].Items[0].Value;
					down_DownstreamTrouble = downStream_Trx.EventGroups[0].Events[0].Items[1].Value;
					down_ReceiveAble = downStream_Trx.EventGroups[0].Events[0].Items[2].Value;
					down_Receive = downStream_Trx.EventGroups[0].Events[0].Items[3].Value;
					down_JobTransfer = downStream_Trx.EventGroups[0].Events[0].Items[4].Value;
					down_ReceiveCancel = downStream_Trx.EventGroups[0].Events[0].Items[5].Value;
					down_ExchangePossible = downStream_Trx.EventGroups[0].Events[0].Items[6].Value;
					down_DoubleGlass = downStream_Trx.EventGroups[0].Events[0].Items[7].Value;
					down_ReceiveJobReserve = downStream_Trx.EventGroups[0].Events[0].Items[8].Value;					
					down_TransferStopRequest = downStream_Trx.EventGroups[0].Events[0].Items[9].Value;
					down_DummyGlassRequest = downStream_Trx.EventGroups[0].Events[0].Items[10].Value;
					down_GlassExist = downStream_Trx.EventGroups[0].Events[0].Items[11].Value;
					down_SlotNumber01 = downStream_Trx.EventGroups[0].Events[0].Items[12].Value;
					down_SlotNumber02 = downStream_Trx.EventGroups[0].Events[0].Items[13].Value;
					down_SlotNumber03 = downStream_Trx.EventGroups[0].Events[0].Items[14].Value;
					down_SlotNumber04 = downStream_Trx.EventGroups[0].Events[0].Items[15].Value;
					down_SlotNumber05 = downStream_Trx.EventGroups[0].Events[0].Items[16].Value;
					down_SlotNumber06 = downStream_Trx.EventGroups[0].Events[0].Items[17].Value;
					down_GlassCount01 = downStream_Trx.EventGroups[0].Events[0].Items[18].Value;
					down_GlassCount02 = downStream_Trx.EventGroups[0].Events[0].Items[19].Value;
					down_GlassCount03 = downStream_Trx.EventGroups[0].Events[0].Items[20].Value;
					down_GlassCount04 = downStream_Trx.EventGroups[0].Events[0].Items[21].Value;
					down_GlassCount05 = downStream_Trx.EventGroups[0].Events[0].Items[22].Value;
					down_PreparationPermission = downStream_Trx.EventGroups[0].Events[0].Items[23].Value;
					down_InspectionResultUpdate = downStream_Trx.EventGroups[0].Events[0].Items[24].Value;
					down_ReturnMode = downStream_Trx.EventGroups[0].Events[0].Items[25].Value;
					#endregion
				}
                //20160204 add for Cell CDR LinkSignal Trx
                else if (Workbench.LineType == eLineType.CELL.CCPDR || Workbench.LineType == eLineType.CELL.CCGAP || Workbench.LineType == eLineType.CELL.CCPTH
                    || Workbench.LineType == eLineType.CELL.CCTAM)
                {
                    #region [ Cell Common Use ,因為1Arm1Job ,Cell 由Robot Control 只有這幾條line ]

                    #region [ Trx Structure ]

                    //<trx name="L3_DownstreamPath#01" triggercondition="change">
                    //<eventgroup name="L3_EG_DownstreamPath#01" dir="E2B">
                    //    <event name="L3_B_DownstreamPath#01" trigger="true" />
                    //</eventgroup>
                    //</trx>

                    //<event name="L3_B_DownstreamPath#01" devicecode="B" address="0x0001130" points="32">
                    //0   <item name="DownstreamPath#01DownstreamInline" offset="0" points="1" expression="BIT" />
                    //1   <item name="DownstreamPath#01DownstreamTrouble" offset="1" points="1" expression="BIT" />
                    //2   <item name="DownstreamPath#01ReceiveAble" offset="2" points="1" expression="BIT" />
                    //3   <item name="DownstreamPath#01Receive" offset="3" points="1" expression="BIT" />
                    //4   <item name="DownstreamPath#01JobTransfer" offset="4" points="1" expression="BIT" />
                    //5   <item name="DownstreamPath#01ReceiveCancel" offset="5" points="1" expression="BIT" />
                    //6   <item name="DownstreamPath#01ExchangePossible" offset="6" points="1" expression="BIT" />
                    //7   <item name="DownstreamPath#01DoubleGlass" offset="7" points="1" expression="BIT" />
                    //8   <item name="DownstreamPath#01ReceiveOK" offset="9" points="1" expression="BIT" />
                    //9   <item name="DownstreamPath#01PinUpComplete" offset="10" points="1" expression="BIT" />
                    //10  <item name="DownstreamPath#01PinDownRequest" offset="11" points="1" expression="BIT" />
                    //11  <item name="DownstreamPath#01SlotNumber#01" offset="13" points="1" expression="BIT" />
                    //12  <item name="DownstreamPath#01SlotNumber#02" offset="14" points="1" expression="BIT" />
                    //13  <item name="DownstreamPath#01SlotNumber#03" offset="15" points="1" expression="BIT" />
                    //14  <item name="DownstreamPath#01SlotNumber#04" offset="16" points="1" expression="BIT" />
                    //15  <item name="DownstreamPath#01SlotNumber#05" offset="17" points="1" expression="BIT" />
                    //16  <item name="DownstreamPath#01SlotNumber#06" offset="18" points="1" expression="BIT" />
                    //17  <item name="DownstreamPath#01PanelPosition#01" offset="24" points="1" expression="BIT" />
                    //18  <item name="DownstreamPath#01PanelPosition#02" offset="25" points="1" expression="BIT" />
                    //19  <item name="DownstreamPath#01PanelPosition#03" offset="26" points="1" expression="BIT" />
                    //20  <item name="DownstreamPath#01PanelPosition#04" offset="27" points="1" expression="BIT" />
                    //21  <item name="DownstreamPath#01PanelPosition#05" offset="28" points="1" expression="BIT" />
                    //22  <item name="DownstreamPath#01PanelPosition#06" offset="29" points="1" expression="BIT" />
                    //23  <item name="DownstreamPath#01PanelPosition#07" offset="30" points="1" expression="BIT" />
                    //24  <item name="DownstreamPath#01PanelPosition#08" offset="31" points="1" expression="BIT" />
                    //</event>

                    #endregion

                    down_DownstreamInline = downStream_Trx.EventGroups[0].Events[0].Items[0].Value;
                    down_DownstreamTrouble = downStream_Trx.EventGroups[0].Events[0].Items[1].Value;
                    down_ReceiveAble = downStream_Trx.EventGroups[0].Events[0].Items[2].Value;
                    down_Receive = downStream_Trx.EventGroups[0].Events[0].Items[3].Value;
                    down_JobTransfer = downStream_Trx.EventGroups[0].Events[0].Items[4].Value;
                    down_ReceiveCancel = downStream_Trx.EventGroups[0].Events[0].Items[5].Value;
                    down_ExchangePossible = downStream_Trx.EventGroups[0].Events[0].Items[6].Value;
                    down_DoubleGlass = downStream_Trx.EventGroups[0].Events[0].Items[7].Value;
                    //PDR無此Item
                    //down_ReceiveJobReserve = downStream_Trx.EventGroups[0].Events[0].Items[8].Value;                    
                    down_ReceiveOK = downStream_Trx.EventGroups[0].Events[0].Items[9].Value;
                    //PDR無此Item
                    //down_TransferStopRequest = downStream_Trx.EventGroups[0].Events[0].Items[10].Value;
                    //down_DummyGlassRequest = downStream_Trx.EventGroups[0].Events[0].Items[11].Value;
                    //down_GlassExist = downStream_Trx.EventGroups[0].Events[0].Items[12].Value;
                    down_PinUpComplete = downStream_Trx.EventGroups[0].Events[0].Items[10].Value;
                    down_PinDownRequest = downStream_Trx.EventGroups[0].Events[0].Items[11].Value;
                    down_SlotNumber01 = downStream_Trx.EventGroups[0].Events[0].Items[13].Value;
                    down_SlotNumber02 = downStream_Trx.EventGroups[0].Events[0].Items[14].Value;
                    down_SlotNumber03 = downStream_Trx.EventGroups[0].Events[0].Items[15].Value;
                    down_SlotNumber04 = downStream_Trx.EventGroups[0].Events[0].Items[16].Value;
                    down_SlotNumber05 = downStream_Trx.EventGroups[0].Events[0].Items[17].Value;
                    down_SlotNumber06 = downStream_Trx.EventGroups[0].Events[0].Items[18].Value;
                    //目前不處理Panel Position
                    //PDR無此Item
                    //down_GlassCount01 = downStream_Trx.EventGroups[0].Events[0].Items[21].Value;
                    //down_GlassCount02 = downStream_Trx.EventGroups[0].Events[0].Items[22].Value;
                    //down_GlassCount03 = downStream_Trx.EventGroups[0].Events[0].Items[23].Value;
                    //down_GlassCount04 = downStream_Trx.EventGroups[0].Events[0].Items[24].Value;
                    //down_ReceiveType01 = downStream_Trx.EventGroups[0].Events[0].Items[25].Value;
                    //down_ReceiveType02 = downStream_Trx.EventGroups[0].Events[0].Items[26].Value;
                    //down_ReceiveType03 = downStream_Trx.EventGroups[0].Events[0].Items[27].Value;
                    //down_ReceiveType04 = downStream_Trx.EventGroups[0].Events[0].Items[28].Value;
                    //down_ReceiveType05 = downStream_Trx.EventGroups[0].Events[0].Items[29].Value;
                    //down_ReceiveType06 = downStream_Trx.EventGroups[0].Events[0].Items[30].Value;

                    
                    #endregion

                }
                else
                {
                    #region default
                    down_DownstreamInline = downStream_Trx.EventGroups[0].Events[0].Items[0].Value;
                    down_DownstreamTrouble = downStream_Trx.EventGroups[0].Events[0].Items[1].Value;
                    down_ReceiveAble = downStream_Trx.EventGroups[0].Events[0].Items[2].Value;
                    down_Receive = downStream_Trx.EventGroups[0].Events[0].Items[3].Value;
                    down_JobTransfer = downStream_Trx.EventGroups[0].Events[0].Items[4].Value;
                    down_ReceiveCancel = downStream_Trx.EventGroups[0].Events[0].Items[5].Value;
                    down_ExchangePossible = downStream_Trx.EventGroups[0].Events[0].Items[6].Value;
                    down_DoubleGlass = downStream_Trx.EventGroups[0].Events[0].Items[7].Value;
                    down_ReceiveJobReserve = downStream_Trx.EventGroups[0].Events[0].Items[8].Value;
                    down_ReceiveOK = downStream_Trx.EventGroups[0].Events[0].Items[9].Value;
                    down_TransferStopRequest = downStream_Trx.EventGroups[0].Events[0].Items[10].Value;
                    down_DummyGlassRequest = downStream_Trx.EventGroups[0].Events[0].Items[11].Value;
                    down_GlassExist = downStream_Trx.EventGroups[0].Events[0].Items[12].Value;
                    down_PinUpComplete = downStream_Trx.EventGroups[0].Events[0].Items[13].Value;
                    down_PinDownRequest = downStream_Trx.EventGroups[0].Events[0].Items[14].Value;
                    down_SlotNumber01 = downStream_Trx.EventGroups[0].Events[0].Items[15].Value;
                    down_SlotNumber02 = downStream_Trx.EventGroups[0].Events[0].Items[16].Value;
                    down_SlotNumber03 = downStream_Trx.EventGroups[0].Events[0].Items[17].Value;
                    down_SlotNumber04 = downStream_Trx.EventGroups[0].Events[0].Items[18].Value;
                    down_SlotNumber05 = downStream_Trx.EventGroups[0].Events[0].Items[19].Value;
                    down_SlotNumber06 = downStream_Trx.EventGroups[0].Events[0].Items[20].Value;
                    down_GlassCount01 = downStream_Trx.EventGroups[0].Events[0].Items[21].Value;
                    down_GlassCount02 = downStream_Trx.EventGroups[0].Events[0].Items[22].Value;
                    down_GlassCount03 = downStream_Trx.EventGroups[0].Events[0].Items[23].Value;
                    down_GlassCount04 = downStream_Trx.EventGroups[0].Events[0].Items[24].Value;
                    down_ReceiveType01 = downStream_Trx.EventGroups[0].Events[0].Items[25].Value;
                    down_ReceiveType02 = downStream_Trx.EventGroups[0].Events[0].Items[26].Value;
                    down_ReceiveType03 = downStream_Trx.EventGroups[0].Events[0].Items[27].Value;
                    down_ReceiveType04 = downStream_Trx.EventGroups[0].Events[0].Items[28].Value;
                    down_ReceiveType05 = downStream_Trx.EventGroups[0].Events[0].Items[29].Value;
                    down_ReceiveType06 = downStream_Trx.EventGroups[0].Events[0].Items[30].Value;
                    #endregion
                }
                #endregion

                //Stage GetGet表示是走ST to RB Mulit Slot Signal Mode
                if (down_DownstreamInline == bitOn && down_ReceiveAble == bitOn && down_Receive == bitOff)
                {

                    #region [ 20151026 與京光討論後有上報則以有上報SlotNo為準,沒有上報則以1為準 ]

                    #region [ 將SlotNo Bit 轉成Int ]

                    strSlotNoBin = down_SlotNumber06 + down_SlotNumber05 + down_SlotNumber04 + down_SlotNumber03 + down_SlotNumber02 + down_SlotNumber01;

                    try
                    {
                        slotNo = Convert.ToInt32(strSlotNoBin, 2);
                    }
                    catch (Exception ex1)
                    {
                        slotNo = 0;
                    }

                    #endregion
                    //20160720
                    fail_ReasonCode = eRobot_CheckFail_Reason.DOWNSTREAM_SLOTNO_IS_ZERO;
                    if (slotNo == 0)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report SlotNo is 0!",
                                                    curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //20160720 TTP Buffer ,stage12 SlotNo=0,不要給預設1,跳failMsg警告
                     // if (curStage.Data.STAGEID.Trim() == "12" && (Workbench.LineType == eLineType.CF.FCMQC_TYPE1 || Workbench.LineType == eLineType.CF.FCMQC_TYPE2))
                        if (curStage.Data.STAGEID.Trim() == "12" && (Workbench.LineType == eLineType.CF.FCMQC_TYPE1))  // 20180203 qiumin  MQC200  STAGE12 ONLY ONE SLOT no check slot no 
                        {
                            if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report SlotNo is 0!",
                                                        curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                        trxID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                failMsg = string.Format("RtnCode({0}) RtnMsg([{1}] StageID({2}) StageName({3}) TrxID({4}) SlotNo is 0!)",
                                                        fail_ReasonCode, MethodBase.GetCurrentMethod().Name, curStage.Data.STAGEID, curStage.Data.STAGENAME, trxID);
                                AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                            }

                        }
                        else
                        {
                            //沒有上報則視為1 
                            slotNo = 1;
                        }
                    }
                    //20160720 清除SlotNo=0的failMsg
                    else
                    {
                        RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                    }

                    #endregion

                    //LinkSignal Bit符合收片條件
                    //only 1 EmptySlot, 更新Current Stage LDRQ Empty Slot
                    lock (curStage)
                    {
                        curStage.CurLDRQ_EmptySlotNo = slotNo.ToString().PadLeft(2, '0');

                        //add Empty SlotNo To EmptySlotNoList
                        if (curStage.curLDRQ_EmptySlotList.ContainsKey(slotNo) == false)
                        {
                            curStage.curLDRQ_EmptySlotList.Add(slotNo, string.Empty);
                        }

                    }

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) DownstreamInline({5}) ReceiveAble({6}) Receive({7}) DoubleGlass({8}) EmptySlotNo({9}), Stage LDRQ Status change to (LDRQ).",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, (eBitResult)int.Parse(down_DownstreamInline), (eBitResult)int.Parse(down_ReceiveAble), (eBitResult)int.Parse(down_Receive),
                                                (eBitResult)int.Parse(down_DoubleGlass), curStage.CurLDRQ_EmptySlotNo);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //只須更新Stage LDRQ Status即可
                    UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.RECEIVE_READY, curStage.File.LDRQ_CstStatusPriority, funcName);

                }
                else
                {
                    //Monitor 條件不合的狀態 Status LDRQ Stage Change To NOREQ
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) DownstreamInline({5}) ReceiveAble({6}) Receive({7}), Stage LDRQ Status can not change to (LDRQ)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, (eBitResult)int.Parse(down_DownstreamInline), (eBitResult)int.Parse(down_ReceiveAble), (eBitResult)int.Parse(down_Receive));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //只須更新Stage LDRQ Status即可
                    UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, curStage.File.LDRQ_CstStatusPriority, funcName);

                }

                #region [ 如果Exchange Possible On 則要更新Stage狀態 ]

                if (down_ExchangePossible == bitOn)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Exchange Request Flag(True)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, eBitResult.ON.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamExchangeReqFlag = true;

                        //20160106 add for 新需求:如MAC设备中有Turn的基板，MAC不能要求Exchange，BC不能下达Exchange Command给Robot;
                        if (curStage.Data.EXCHANGETYPE == eRobotStage_ExchangeType.MAC_EXCHANGE)
                        {
                            if (CheckMAC_Can_ExchangeFlag(curStage) == true)
                            {
                                curStage.MacCanNotExchangeFlag = false;
                            }
                            else
                            {
                                curStage.MacCanNotExchangeFlag = true;

                            }
                        }
                        else
                        {
                            //非MAC 只要條件符合即可Exchange
                            curStage.MacCanNotExchangeFlag = false;
                        }

                    }

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Exchange Request Flag(False)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, 
                                                curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamExchangeReqFlag = false;
                    }

                }

                #endregion

                #region 更新 DRY LinkSignal Receive Type 信息
                //GlobalAssemblyVersion v1.0.0.26-20151015, added by dade
                switch (Workbench.LineType)
                {
                    case "DRY_ICD":
                    case "DRY_YAC":
                    case "DRY_TEL":
                        if (curStage.Data.NODENO == eRobotContextParameter.DRYNodeNo) // DRY equipment
                        {
                            switch (curStage.Data.STAGEID)
                            {
                                case "12": // 12 = Lower Load Lock (Array shop / DRY line / DRY equipment)
                                case "13": // 13 = Upper Load Lock (Array shop / DRY line / DRY equipment) 
                                    int _receiveType = 0; //0 = Product (NOREQ)
                                    _receiveType = Convert.ToInt16(string.Format("{5}{4}{3}{2}{1}{0}",
                                        down_ReceiveType01,
                                        down_ReceiveType02,
                                        down_ReceiveType03,
                                        down_ReceiveType04,
                                        down_ReceiveType05,
                                        down_ReceiveType06), 2);

                                    //curStage.File.DownStreamLoadLockReceiveType = (down_ReceiveAble == "1" ? _receiveType : 0);
                                    curStage.File.DownStreamLoadLockReceiveType = _receiveType; //不管 ReceiveAble signal 是不是有 ON, 直接給值!!

                                    if (down_DownstreamInline == "1")
                                    {
                                        if (down_ReceiveAble == "1") curStage.File.DryKeptLoadLockReceiveType = _receiveType;
                                        //{
                                        //    if (curStage.File.DryKeptLoadLockReceiveType <= 0)
                                        //    {
                                        //        curStage.File.DryKeptLoadLockReceiveType = _receiveType;
                                        //    }
                                        //    else
                                        //    {
                                        //        if (curStage.File.DryKeptLoadLockReceiveType != _receiveType) curStage.File.DryKeptLoadLockReceiveType = _receiveType;
                                        //    }
                                        //}
                                    }
                                    else
                                    {
                                        if (down_DownstreamTrouble == "0") curStage.File.DryKeptLoadLockReceiveType = 0; //如果Inline signal OFF, 就reset!
                                    }
                                    break;
                            }
                        }
                        break;
                }
                #endregion

                #region [ 如果 Receive Able On 則要更新Stage狀態 ]

                if (down_ReceiveAble == bitOn)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveAble({5}),Update RobotStage Receive Able Signal(True)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, eBitResult.ON.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamReceiveAbleSignal = true;
                    }

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Receive Able Signal(False)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamReceiveAbleSignal = false;
                    }

                }

                #endregion;

                #region [ 如果 Receive Job Reserve On 則要更新Stage狀態 ]
                if (down_ReceiveJobReserve == bitOn)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveAble({5}),Update RobotStage Receive Job Reserve Signal(True)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, eBitResult.ON.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    lock (curStage.File)
                    {
                        curStage.File.DownStreamReceiveJobReserveSignal = true;
                    }
                }
                else
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Receive Job Reserve Signal(False)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    lock (curStage.File)
                    {
                        curStage.File.DownStreamReceiveJobReserveSignal = false;
                    }
                }
                #endregion;

                #region [ 20151215 add 如果Transfer Stop Request On 則要更新Stage狀態 ]

                if (down_TransferStopRequest == bitOn)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) TransferStopRequest({5}),Update RobotStage Transfer Stop Request Flag(True)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, eBitResult.ON.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamTransferStopRequestFlag = true;
                    }

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) TransferStopRequest({5}),Update RobotStage Transfer Stop Request Flag(False)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamTransferStopRequestFlag = false;
                    }

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        private void Get_StageTypeSignal_LDRQStauts_For1Arm1Job(Robot curRobot, RobotStage curStage)
        {

            string trxID = string.Empty;
            string strlog = string.Empty;

            FCSRT_RobotParam srt_param = null;
            if (Workbench.LineType == eLineType.CF.FCSRT_TYPE1)
            {
                #region Check Static Context
                if (!StaticContext.ContainsKey(eRobotContextParameter.FCSRT_RobotParam))
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "StaticContext is not Contains FCSRT_RobotParam");
                    return;
                }
                if (!(StaticContext[eRobotContextParameter.FCSRT_RobotParam] is FCSRT_RobotParam))
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "StaticContext is not Contains FCSRT_RobotParam");
                    return;
                }
                #endregion
                srt_param = (FCSRT_RobotParam)StaticContext[eRobotContextParameter.FCSRT_RobotParam];
            }

            try
            {
                string bitOn = "1";
                string bitOff = "0";
                string funcName = MethodBase.GetCurrentMethod().Name;

                #region [Get line fabtyep]
                string fabtype = eFabType.ARRAY.ToString();
                Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if (line != null)
                {
                    fabtype = line.Data.FABTYPE;
                }
                #endregion

                #region [ Check Trx Setting ]

                if (curStage.Data.STAGEREPORTTRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find DownStream TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;

                }

                #endregion

                //Signal SlotNo=1
                int slotNo = 1;

                #region  real time Get Stage LDRQ

                trxID = curStage.Data.STAGEREPORTTRXNAME.Trim();

                Trx downStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                if (downStream_Trx == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;
                }

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4})!",
                                            curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                            trxID);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L2_Stage#01SinglePositionReport" triggercondition="change">
                //    <eventgroup name="L2_EG_Stage#01SinglePositionReport" dir="E2B">
                //      <event name="L2_B_Stage#01SendReady" trigger="true" />
                //      <event name="L2_B_Stage#01ReceiveReady" trigger="true" />
                //      <event name="L2_B_Stage#01DoubleGlassExist" trigger="true" />
                //      <event name="L2_B_Stage#01ExchangePossible" trigger="true" />
                //    </eventgroup>
                //</trx>

                //<event name="L2_B_Stage#01SendReady" devicecode="B" address="0x0000CEC" points="1">
                //  <item name="Stage#01SendReady" offset="0" points="1" expression="BIT" />
                //</event>

                #endregion
                #region [variable declare]
                string down_SendReady = "0";
                string down_ReceiveReady = "0";
                string down_DoubleGlassExist = "0";
                string down_ExchangePossible = "0";
                #endregion
                if (fabtype == eFabType.CF.ToString() || fabtype == eFabType.ARRAY.ToString())
                {
                    #region Array
                    down_SendReady = downStream_Trx.EventGroups[0].Events[0].Items[0].Value;
                    down_ReceiveReady = downStream_Trx.EventGroups[0].Events[1].Items[0].Value;
                    down_DoubleGlassExist = downStream_Trx.EventGroups[0].Events[2].Items[0].Value;
                    down_ExchangePossible = downStream_Trx.EventGroups[0].Events[3].Items[0].Value;
                    #endregion
                }
                else
                {
                    #region default
                    ;
                    #endregion
                }
                #endregion

                //Stage LDRQ
                if (down_ReceiveReady == bitOn)
                {
                    if (Workbench.LineType == eLineType.CF.FCSRT_TYPE1)
                    {
                        #region FCSRT 需要判斷兩個 VCR Stage 是否可以 PutPut
                        if (srt_param.GetReceiveAbleOnTime(curStage.Data.STAGEID) == DateTime.MinValue)
                            srt_param.SetReceiveAbleOnTime(curStage.Data.STAGEID, DateTime.Now);

                        if (srt_param.CheckVCRStagePutPut())
                        {
                            //only 1 EmptySlot, 更新Current Stage LDRQ Empty Slot
                            lock (curStage)
                            {
                                curStage.CurLDRQ_EmptySlotNo = slotNo.ToString().PadLeft(2, '0');

                                //add Empty SlotNo To EmptySlotNoList
                                if (curStage.curLDRQ_EmptySlotList.ContainsKey(slotNo) == false)
                                {
                                    curStage.curLDRQ_EmptySlotList.Add(slotNo, string.Empty);
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        //only 1 EmptySlot, 更新Current Stage LDRQ Empty Slot
                        lock (curStage)
                        {
                            curStage.CurLDRQ_EmptySlotNo = slotNo.ToString().PadLeft(2, '0');

                            //add Empty SlotNo To EmptySlotNoList
                            if (curStage.curLDRQ_EmptySlotList.ContainsKey(slotNo) == false)
                            {
                                curStage.curLDRQ_EmptySlotList.Add(slotNo, string.Empty);
                            }
                        }
                    }
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveReady({5}) ExchangePossible({6}), Stage LDRQ Status change to (LDRQ).",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, (eBitResult)int.Parse(down_ReceiveReady), (eBitResult)int.Parse(down_ExchangePossible));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //只須更新Stage LDRQ Status即可
                    UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.RECEIVE_READY, curStage.File.LDRQ_CstStatusPriority, funcName);

                }
                else
                {
                    //Monitor 條件不合的狀態 Status LDRQ Stage Change To NOREQ
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveReady({5}) ExchangePossible({6}), Stage LDRQ Status can not change to (LDRQ)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, (eBitResult)int.Parse(down_ReceiveReady), (eBitResult)int.Parse(down_ExchangePossible));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //只須更新Stage LDRQ Status即可
                    UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, curStage.File.LDRQ_CstStatusPriority, funcName);

                }

                #region [ 如果Exchange Possible On 則要更新Stage狀態 ]

                if (down_ExchangePossible == bitOn)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Exchange Request Flag(True)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, eBitResult.ON.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamExchangeReqFlag = true;
                    }

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Exchange Request Flag(False)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamExchangeReqFlag = false;
                    }

                }

                #endregion

                #region [ 如果 Receive Able On(refrence ReceiveReady if stage is indexer's inner stage) 則要更新Stage狀態 ]

                if (down_ReceiveReady == bitOn)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveReady({5}),Update RobotStage Receive Able Signal(True)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, eBitResult.ON.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamReceiveAbleSignal = true;
                    }

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveReady({5}),Update RobotStage Receive Able Signal(False)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamReceiveAbleSignal = false;
                    }

                }

                #endregion;

                #region [ 如果 Receive Job Reserve On 則要更新Stage狀態 ]
                if (down_ExchangePossible == bitOn)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Receive Job Reserve Signal(True)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, eBitResult.ON.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    lock (curStage.File)
                    {
                        curStage.File.DownStreamReceiveJobReserveSignal = true;
                    }
                }
                else
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Receive Job Reserve Signal(False)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    lock (curStage.File)
                    {
                        curStage.File.DownStreamReceiveJobReserveSignal = false;
                    }
                }
                #endregion;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        private void JudgeEQPStage_UDRQ_LDRQStatus(RobotStage curStage)
        {

            string strlog = string.Empty;
            string tmpStageStatus = string.Empty;

            try
            {

                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                #region [ 比對UDRQ and LDRQ Stage Status 判斷最終狀態 ]

                if (curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_READY || curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                {
                    if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                    {
                        //20151209 add Mulit Slot Stage允許出現LDRQ_UDRQ
                        if (curStage.Data.ISMULTISLOT == "Y")
                        {
                            tmpStageStatus = eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY;
                        }
                        else
                        {
                            //可出片同時可收片是有問題的!!! EQP Stage不能同時出現LDRQ and UDRQ
                            tmpStageStatus = eRobotStageStatus.NO_REQUEST;
                        }
                    }
                    else
                    {
                        //可出片 不可收片
                        tmpStageStatus = eRobotStageStatus.SEND_OUT_READY;

                    }

                }
                else
                {
                    if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                    {
                        //不可出片 可收片
                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

                    }
                    else
                    {
                        //不可出片 不可收片
                        tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                    }

                }

                #endregion

                #region [ 更新Stage Status ]

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Stage UDRQ Status({4}), LDRQ Status({5}) , Judge Stage Status({6})!",
                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                            curStage.File.Stage_UDRQ_Status, curStage.File.Stage_LDRQ_Status, tmpStageStatus);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }

                #endregion

                if (tmpStageStatus == eRobotStageStatus.SEND_OUT_READY || tmpStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                {

                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, curStage.File.CurSendOut_CSTSeq, curStage.File.CurSendOut_JobSeq, curStage.File.CurSendOut_CSTSeq02, curStage.File.CurSendOut_JobSeq02);
                }
                else
                {
                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty, string.Empty, string.Empty);
                }

                #endregion
            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        private void JudgeIndexerStage_UDRQ_LDRQStatus(RobotStage curStage)
        {

            string strlog = string.Empty;
            string tmpStageStatus = string.Empty;

            try
            {

                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                #region [ 比對UDRQ and LDRQ Stage Status 判斷最終狀態 ]

                if (curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_READY || curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                {
                    if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                    {
                        //可出片同時可收片是有問題的!!! EQP Stage不能同時出現LDRQ and UDRQ
                        tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                    }
                    else
                    {
                        //可出片 不可收片
                        tmpStageStatus = eRobotStageStatus.SEND_OUT_READY;

                    }

                }
                else
                {
                    if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                    {
                        //不可出片 可收片
                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

                    }
                    else
                    {
                        //不可出片 不可收片
                        tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                    }

                }

                #endregion

                #region [ 更新Stage Status ]

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Stage UDRQ Status({4}), LDRQ Status({5}) , Judge Stage Status({6})!",
                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                            curStage.File.Stage_UDRQ_Status, curStage.File.Stage_LDRQ_Status, tmpStageStatus);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }

                #endregion

                if (tmpStageStatus == eRobotStageStatus.SEND_OUT_READY || tmpStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                {

                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, curStage.File.CurSendOut_CSTSeq, curStage.File.CurSendOut_JobSeq, curStage.File.CurSendOut_CSTSeq02, curStage.File.CurSendOut_JobSeq02);
                }
                else
                {
                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty, string.Empty, string.Empty);
                }

                #endregion
            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        #endregion ==========================================================================================================================================================================

        #region Get EQP Type Stage Mulit Slot(Multi-Single) Can Control JobList Function List ===============================================================================================

        /// <summary> 取得Mulit Slot Signal Mode Stage Can ControlJobList for GetGetPutPut Use . Only For Mulit Slot Signal Mode
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        private void Get_EqpTypeMulitSlot_CanControlJobList_ForMulitSingle(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList)
        {
            string trxID = string.Empty;
            string strlog = string.Empty;

            try
            {
                string bitOn = "1";
                string bitOff = "0";
                string funcName = MethodBase.GetCurrentMethod().Name;

				#region [Get line fabtyep]
				string fabtype = eFabType.ARRAY.ToString();
				Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
				if (line != null) {
					fabtype = line.Data.FABTYPE;
				}
				#endregion

                #region [ Check Trx Setting ]

                if (curStage.Data.UPSTREAMPATHTRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find UpStream TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;

                }

                #endregion

                //interface時Stage出片要看Upstream .透過LinkSignal已經得知JobData是否填好 Send Signal On =JobData Exist
                string[] upStreamTrxList = curStage.Data.UPSTREAMPATHTRXNAME.Split(',');
                string strSlotNoBin = string.Empty;
                string strGlassCountBin = string.Empty;
                int slotNo = 0;
                int anotherSlotNo = 0;
                int glassCount = 0;

                for (int i = 0; i < upStreamTrxList.Length; i++)
                {

                    #region  real time Get Interface Upstream

                    trxID = upStreamTrxList[i];

                    Trx upStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                    if (upStream_Trx == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                    curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        continue;
                    }

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #endregion

                    #region [拆出PLCAgent Data]

                    #region [ Trx Structure ]

                    //<trx name="L3_UpstreamPath#01" triggercondition="change">
                    //    <eventgroup name="L3_EG_UpstreamPath#01" dir="E2B">
                    //      <event name="L3_B_UpstreamPath#01" trigger="true" />
                    //    </eventgroup>
                    //</trx>

                    //<event name="L3_B_UpstreamPath#01" devicecode="B" address="0x0000B00" points="32">
                    //  <item name="UpstreamPath#01UpstreamInline" offset="0" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01UpstreamTrouble" offset="1" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SendReady" offset="2" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01Send" offset="3" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01JobTransfer" offset="4" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SendCancel" offset="5" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01ExchangeExecute" offset="6" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01DoubleGlass" offset="7" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SendJobReserve" offset="8" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SendOK" offset="9" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01PinUpRequest" offset="13" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01PinDownComplete" offset="14" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#01" offset="16" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#02" offset="17" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#03" offset="18" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#04" offset="19" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#05" offset="20" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01SlotNumber#06" offset="21" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01GlassCount#01" offset="22" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01GlassCount#02" offset="23" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01GlassCount#03" offset="24" points="1" expression="BIT" />
                    //  <item name="UpstreamPath#01GlassCount#04" offset="25" points="1" expression="BIT" />
                    //</event>

                    #endregion
					#region [variable declare]
					string up_UpstreamInline = "0";
					string up_UpstreamTrouble = "0";
					string up_SendReady = "0";
					string up_Send = "0";
					string up_JobTransfer = "0";
					string up_SendCancel = "0";
					string up_ExchangeExecute = "0";
					string up_DoubleGlass = "0";
					string up_SendJobReserve = "0";
					string up_SendOK = "0";
					string up_ReceiveOK = "0";
					string up_PinUpRequest = "0";
					string up_PinDownComplete = "0";
					string up_SlotNumber01 = "0";
					string up_SlotNumber02 = "0";
					string up_SlotNumber03 = "0";
					string up_SlotNumber04 = "0";
					string up_SlotNumber05 = "0";
					string up_SlotNumber06 = "0";
					string up_GlassCount01 = "0";
					string up_GlassCount02 = "0";
					string up_GlassCount03 = "0";
					string up_GlassCount04 = "0";
					#endregion
					if (fabtype == eFabType.CF.ToString()) {
						#region CF
						up_UpstreamInline = upStream_Trx.EventGroups[0].Events[0].Items[0].Value;
						up_UpstreamTrouble = upStream_Trx.EventGroups[0].Events[0].Items[1].Value;
						up_SendReady = upStream_Trx.EventGroups[0].Events[0].Items[2].Value;
						up_Send = upStream_Trx.EventGroups[0].Events[0].Items[3].Value;
						up_JobTransfer = upStream_Trx.EventGroups[0].Events[0].Items[4].Value;
						up_SendCancel = upStream_Trx.EventGroups[0].Events[0].Items[5].Value;
						up_ExchangeExecute = upStream_Trx.EventGroups[0].Events[0].Items[6].Value;
						up_DoubleGlass = upStream_Trx.EventGroups[0].Events[0].Items[7].Value;
						up_SendJobReserve = upStream_Trx.EventGroups[0].Events[0].Items[8].Value;
						up_ReceiveOK = upStream_Trx.EventGroups[0].Events[0].Items[9].Value;
						up_SlotNumber01 = upStream_Trx.EventGroups[0].Events[0].Items[10].Value;
						up_SlotNumber02 = upStream_Trx.EventGroups[0].Events[0].Items[11].Value;
						up_SlotNumber03 = upStream_Trx.EventGroups[0].Events[0].Items[12].Value;
						up_SlotNumber04 = upStream_Trx.EventGroups[0].Events[0].Items[13].Value;
						up_SlotNumber05 = upStream_Trx.EventGroups[0].Events[0].Items[14].Value;
						up_SlotNumber06 = upStream_Trx.EventGroups[0].Events[0].Items[15].Value;
						#endregion
					} else {
						#region [default]
						up_UpstreamInline = upStream_Trx.EventGroups[0].Events[0].Items[0].Value;
						up_UpstreamTrouble = upStream_Trx.EventGroups[0].Events[0].Items[1].Value;
						up_SendReady = upStream_Trx.EventGroups[0].Events[0].Items[2].Value;
						up_Send = upStream_Trx.EventGroups[0].Events[0].Items[3].Value;
						up_JobTransfer = upStream_Trx.EventGroups[0].Events[0].Items[4].Value;
						up_SendCancel = upStream_Trx.EventGroups[0].Events[0].Items[5].Value;
						up_ExchangeExecute = upStream_Trx.EventGroups[0].Events[0].Items[6].Value;
						up_DoubleGlass = upStream_Trx.EventGroups[0].Events[0].Items[7].Value;
						up_SendJobReserve = upStream_Trx.EventGroups[0].Events[0].Items[8].Value;
						up_SendOK = upStream_Trx.EventGroups[0].Events[0].Items[9].Value;
						up_PinUpRequest = upStream_Trx.EventGroups[0].Events[0].Items[10].Value;
						up_PinDownComplete = upStream_Trx.EventGroups[0].Events[0].Items[11].Value;
						up_SlotNumber01 = upStream_Trx.EventGroups[0].Events[0].Items[12].Value;
						up_SlotNumber02 = upStream_Trx.EventGroups[0].Events[0].Items[13].Value;
						up_SlotNumber03 = upStream_Trx.EventGroups[0].Events[0].Items[14].Value;
						up_SlotNumber04 = upStream_Trx.EventGroups[0].Events[0].Items[15].Value;
						up_SlotNumber05 = upStream_Trx.EventGroups[0].Events[0].Items[16].Value;
						up_SlotNumber06 = upStream_Trx.EventGroups[0].Events[0].Items[17].Value;
						up_GlassCount01 = upStream_Trx.EventGroups[0].Events[0].Items[18].Value;
						up_GlassCount02 = upStream_Trx.EventGroups[0].Events[0].Items[19].Value;
						up_GlassCount03 = upStream_Trx.EventGroups[0].Events[0].Items[20].Value;
						up_GlassCount04 = upStream_Trx.EventGroups[0].Events[0].Items[21].Value;
						#endregion
					}
                    #endregion

                    //Stage Mulit Single表示是走ST to RB Mulit Slot Signal Mode
                    if (up_UpstreamInline == bitOn && up_SendReady == bitOn && up_Send == bitOn)
                    {
                        //LinkSignal Bit符合出片條件 Mulit Slot=> SlotNo= SendOutJobData No
                        //注意!!! Array  IMP Stage  SlotNo=1是指上層. 
                        #region [ 將SlotNo Bit 轉成Int ]

                        strSlotNoBin = up_SlotNumber06 + up_SlotNumber05 + up_SlotNumber04 + up_SlotNumber03 + up_SlotNumber02 + up_SlotNumber01;

                        try
                        {
                            slotNo = Convert.ToInt32(strSlotNoBin, 2);
                        }
                        catch (Exception ex1)
                        {
                            slotNo = 0;
                        }

                        #endregion

                        if (slotNo == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report SlotNo is 0!",
                                                        curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                        trxID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //SlotNo沒填無法確認如何出片
                            UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty, string.Empty, string.Empty);

                            break;
                        }

                        #region  [ 先根據根據SlotNo 取得JobData ]

                        Job curSendOutJob = new Job();

                        //Mulit Signal 1bit 2Word
                        //不管一片還是兩片 第一片都會顯示在JobData Send#1.
                        if (Get_LinkSignalSendOutJobInfo_ForMulitSingle(curRobot, curStage, curStageCanControlJobList, slotNo, 1, out curSendOutJob) == true)
                        {
                            //找到符合SendOut的Job ,判斷是SendOut 2片還是一片
                            if (up_DoubleGlass != bitOn)
                            {

                                #region [ EQP Only SendOut 1 Job ]

                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}) ,Stage UDRQ Status change to (UDRQ)!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                            trxID, eBitResult.ON.ToString(), eBitResult.ON.ToString(), eBitResult.ON.ToString());
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                //20151024 add for SendOut Job SlotNo
                                lock (curStage)
                                {
                                    if (curStage.curUDRQ_SlotList.ContainsKey(curSendOutJob.RobotWIP.CurLocation_SlotNo) == false)
                                    {
                                        curStage.curUDRQ_SlotList.Add(curSendOutJob.RobotWIP.CurLocation_SlotNo, string.Format("{0}_{1}", curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo));
                                    }
                                }

                                //Update Status UDRQ Stage Change To UDRQ
                                UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo, string.Empty, string.Empty);

                                #endregion

                                return;

                            }
                            else
                            {

                                #region [ EQP SendOut 2 Job 20150819與京光討論結果. 第二片就算有問題還是要將第一片取出 ]

                                #region [ Get Another SlotNo ]

                                //Mulit Slot Signal Mode for IMP 只有2各Slot
                                if (slotNo == 1)
                                {
                                    anotherSlotNo = 2;
                                }
                                else
                                {
                                    anotherSlotNo = 1;
                                }

                                #endregion

                                #region  [ 先根據根據 AnotherSlotNo 取得JobData02 ]

                                Job curSendOutJob02 = new Job();

                                if (Get_LinkSignalSendOutJobInfo_ForMulitSingle(curRobot, curStage, curStageCanControlJobList, anotherSlotNo, 2, out curSendOutJob02) == true)
                                {

                                    #region [ EQP SendOut 2 Job ]

                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}) ,Stage UDRQ Status change to (UDRQ)!",
                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                                trxID, eBitResult.ON.ToString(), eBitResult.ON.ToString(), eBitResult.ON.ToString());
                                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    //20151024 add for SendOut Job SlotNo
                                    lock (curStage)
                                    {
                                        if (curStage.curUDRQ_SlotList.ContainsKey(curSendOutJob.RobotWIP.CurLocation_SlotNo) == false)
                                        {
                                            curStage.curUDRQ_SlotList.Add(curSendOutJob.RobotWIP.CurLocation_SlotNo, string.Format("{0}_{1}", curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo));
                                        }

                                        if (curStage.curUDRQ_SlotList.ContainsKey(curSendOutJob02.RobotWIP.CurLocation_SlotNo) == false)
                                        {
                                            curStage.curUDRQ_SlotList.Add(curSendOutJob02.RobotWIP.CurLocation_SlotNo, string.Format("{0}_{1}", curSendOutJob02.CassetteSequenceNo, curSendOutJob02.JobSequenceNo));
                                        }
                                    }

                                    //Update Status UDRQ Stage Change To UDRQ
                                    UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo,
                                                            curSendOutJob02.CassetteSequenceNo, curSendOutJob02.JobSequenceNo);

                                    #endregion

                                    return;
                                }
                                else
                                {
                                    #region [ EQP Only SendOut 1 Job ]

                                    //Only Get Job01 UDRQ Stage Change To UDRQ
                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) can not get SendOut JobData02, Stage UDRQ Status change from ({3}) to ({4})!",
                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.File.CurStageStatus,
                                                                eRobotStageStatus.SEND_OUT_READY);
                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    //20151024 add for SendOut Job SlotNo
                                    lock (curStage)
                                    {
                                        if (curStage.curUDRQ_SlotList.ContainsKey(curSendOutJob.RobotWIP.CurLocation_SlotNo) == false)
                                        {
                                            curStage.curUDRQ_SlotList.Add(curSendOutJob.RobotWIP.CurLocation_SlotNo, string.Format("{0}_{1}", curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo));
                                        }
                                    }

                                    //Update Status UDRQ Stage Change To UDRQ
                                    UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo, string.Empty, string.Empty);

                                    #endregion

                                    return;
                                }

                                #endregion

                                #endregion
                            }
                        }
                        else
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) can not get SendOut JobData, Stage UDRQ Status change from ({3}) to ({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.File.CurStageStatus,
                                                        eRobotStageStatus.NO_REQUEST);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //無SendOut Job Info Status UDRQ Stage Change To NOREQ
                            UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty, string.Empty, string.Empty);

                        }

                        #endregion

                    }
                    else
                    {

                        //Monitor 條件不合的狀態 Status UDRQ Stage Change To NOREQ
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}), Stage UDRQ Status can not change to (UDRQ)!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID, (eBitResult)int.Parse(up_UpstreamInline), (eBitResult)int.Parse(up_SendReady), (eBitResult)int.Parse(up_Send));
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty, string.Empty, string.Empty);

                    }

                }

            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }


        }

        /// <summary> 取得MulitSingle LinkSignal SendOut的 JobData by SeqNo
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        /// <param name="sendOutSlotNo">SendOut的SlotNo</param>
        /// <param name="sendDataSeqNo">SendOutJobData的位置</param>
        /// <param name="curSendOutJob"></param>
        /// <returns></returns>
        private bool Get_LinkSignalSendOutJobInfo_ForMulitSingle(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList, int sendOutSlotNo,int sendDataSeqNo, out Job curSendOutJob)
                                                                    
        {
            string strlog = string.Empty;
            string jobKey = string.Empty;
            Job returnJob = new Job();
            curSendOutJob = returnJob;

            try
            {

                string funcName = MethodBase.GetCurrentMethod().Name;

                #region [ Check Trx Setting Exist ]

                if (curStage.Data.UPSTREAMJOBDATAPATHTRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) can not find SendOutJobData TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;

                }

                //Mulit-Single 指對應2各Slot(01,02)
                //不管一片還是兩片 第一片都會顯示在JobData Send#1
                //取得所有SendOut的TrxID
                string[] sendOutTrxList = curStage.Data.UPSTREAMJOBDATAPATHTRXNAME.Split(',');
                string trxID = string.Empty;

                #region [ 20151025 mark 改成照Seq取得TrxID ]
                 
                //string strJobDataSeqNoTrxKey = string.Format("{0}_SendOutJobDataReport#{1}", curStage.Data.NODENO, sendDataSeqNo.ToString().PadLeft(2, '0'));             

                //for (int i = 0; i < sendOutTrxList.Length; i++)
                //{

                //    if (strJobDataSeqNoTrxKey == sendOutTrxList[i])
                //    {
                //        trxID = sendOutTrxList[i];
                //        break;

                //    }

                //}

                //if (trxID == string.Empty)
                //{

                //    #region  [DebugLog]

                //    if (IsShowDetialLog == true)
                //    {
                //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) sendout SlotNo({4}) but can not find SendOutJobData TrxID({5}) by setting({6})!",
                //                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                //                                sendOutSlotNo.ToString(), strJobDataSeqNoTrxKey, curStage.Data.UPSTREAMJOBDATAPATHTRXNAME);
                //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //    }

                //    #endregion

                //    return false;
                //}

                #endregion

                if (sendOutTrxList.Length < sendDataSeqNo)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) sendout SlotNo({4}) JobDataSeqNo({5}) but can not find SendOutJobData TrxID by setting({6})!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                sendOutSlotNo.ToString(), sendDataSeqNo.ToString(), curStage.Data.UPSTREAMJOBDATAPATHTRXNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;

                }
                else
                {

                    trxID = sendOutTrxList[sendDataSeqNo-1];
                }

                #endregion

                #region  real time Get Trx by sendOutSlotNo and JobDataSeqNo

                Trx GetJobData_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                if (GetJobData_Trx == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;

                }

                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L3_SendOutJobDataReport#01" triggercondition="change">
                //    <eventgroup name="L3_EG_SendOutJobDataReport#01" dir="E2B">
                //      <event name="L3_W_SendOutJobDataBlock_JobData1" />
                //      <event name="L3_B_SendOutJobDataReport#01" trigger="true" />
                //    </eventgroup>
                //  </trx>

                //<itemgroup name="JobData">
                //    <item name="CassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //    <item name="JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //    <item name="GroupIndex" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //    <item name="ProductType" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //    <item name="CSTOperationMode" woffset="4" boffset="0" wpoints="1" bpoints="2" expression="INT" />
                //    <item name="SubstrateType" woffset="4" boffset="2" wpoints="1" bpoints="2" expression="INT" />
                //    <item name="CIMMode" woffset="4" boffset="4" wpoints="1" bpoints="1" expression="INT" />
                //    <item name="JobType" woffset="4" boffset="5" wpoints="1" bpoints="4" expression="INT" />
                //    <item name="JobJudge" woffset="4" boffset="9" wpoints="1" bpoints="4" expression="INT" />
                //    <item name="SamplingSlotFlag" woffset="4" boffset="13" wpoints="1" bpoints="1" expression="BIN" />
                //    <item name="FirstRunFlag" woffset="4" boffset="14" wpoints="1" bpoints="1" expression="BIN" />
                //    <item name="JobGrade" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="ASCII" />
                //    <item name="Glass/Chip/MaskID/BlockID" woffset="6" boffset="0" wpoints="10" bpoints="160" expression="ASCII" />
                //    <item name="PPID" woffset="16" boffset="0" wpoints="25" bpoints="400" expression="ASCII" />
                //    <item name="GlassFlowType" woffset="41" boffset="0" wpoints="1" bpoints="6" expression="INT" />
                //    <item name="ProcessType" woffset="41" boffset="6" wpoints="1" bpoints="6" expression="INT" />
                //    <item name="LastGlassFlag" woffset="41" boffset="12" wpoints="1" bpoints="1" expression="BIN" />
                //    <item name="RTCFlag" woffset="41" boffset="13" wpoints="1" bpoints="1" expression="BIN" />
                //    <item name="MainEQInFlag" woffset="41" boffset="14" wpoints="1" bpoints="1" expression="BIN" />
                //    <item name="Insp.JudgedData" woffset="42" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
                //    <item name="TrackingData" woffset="44" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
                //    <item name="EQPFlag" woffset="46" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
                //    <item name="ChipCount" woffset="48" boffset="0" wpoints="1" bpoints="9" expression="INT" />
                //    <item name="RecipeGroupNumber" woffset="48" boffset="10" wpoints="1" bpoints="6" expression="INT" />
                //    <item name="SourcePortNo" woffset="49" boffset="0" wpoints="1" bpoints="8" expression="INT" />
                //    <item name="TargetPortNo" woffset="49" boffset="8" wpoints="1" bpoints="8" expression="INT" />
                //</itemgroup>

                string cstSeq = GetJobData_Trx.EventGroups[0].Events[0].Items["CassetteSequenceNo"].Value;
                string jobSeq = GetJobData_Trx.EventGroups[0].Events[0].Items["JobSequenceNo"].Value;
                string sendOut_TrackingData = GetJobData_Trx.EventGroups[0].Events[0].Items["TrackingData"].Value;
                string sendOut_JobJudge = GetJobData_Trx.EventGroups[0].Events[0].Items["JobJudge"].Value;
                string eqp_FLAG = GetJobData_Trx.EventGroups[0].Events[0].Items["EQPFlag"].Value;   //Watson add 20151019 For TTP DailyCheck 

                #endregion

                #endregion

                if (cstSeq != "0" && jobSeq != "0")
                {
                    jobKey = string.Format("{0}_{1}", cstSeq.ToString(), jobSeq.ToString());

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4}) ,CSTSeq({5}) JobSeq({6}) JobKey({7}).",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, cstSeq, jobSeq, jobKey);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Check Job is Exist ]

                    Job curBcsJob = ObjectManager.JobManager.GetJob(cstSeq, jobSeq);

                    if (curBcsJob == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBMRCS Robot({1}) StageID({2}) StageName({3}) Can not Get Job by CSTSeq({4}) JobSeq({5})!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    cstSeq, jobSeq);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false; //Not In WIP

                    }

                    #endregion

                    #region [ Update Job RobotWIP ]

                    //有變化才紀錄Log   LinkSignal要看Send Out JobData的TrackingData .Route Priority目前直接參照ROBOTWIP內Route資訊排序即可
                    if (curBcsJob.RobotWIP.CurLocation_StageID != curStage.Data.STAGEID ||
                        curBcsJob.RobotWIP.CurLocation_SlotNo != sendOutSlotNo ||
                        curBcsJob.RobotWIP.CurLocation_StageType != eRobotStageType.EQUIPMENT ||
                        curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData != sendOut_TrackingData ||
                        curBcsJob.RobotWIP.CurPortCstStatusPriority != eLoaderPortSendOutStatus.NOT_IN_PORT ||
                        curBcsJob.RobotWIP.CurSendOutJobJudge != sendOut_JobJudge ||
                        curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG != eqp_FLAG)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Job CassetteSequenceNo({1}) JobSequenceNo({2}) Update RobotWIP curStageType from ({3}) to ({4}), curStageID from ({5}) to ({6}), curSlotNo From ({7}) to ({8}), SendOut TrackingData from ({9}) to ({10}),sendOutJobJudge from ({11}) to ({12}) PortCSTStatusPriority from ({13}) to ({14}), SendOut EQPFlag from ({15}) to ({16}).",
                                                curStage.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurLocation_StageType,
                                                eRobotStageType.EQUIPMENT, curBcsJob.RobotWIP.CurLocation_StageID, curStage.Data.STAGEID, curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(),
                                                sendOutSlotNo.ToString(), curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData, sendOut_TrackingData, curBcsJob.RobotWIP.CurSendOutJobJudge,
                                                sendOut_JobJudge, curBcsJob.RobotWIP.CurPortCstStatusPriority, eLoaderPortSendOutStatus.NOT_IN_PORT, 
                                                curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG, eqp_FLAG);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {

                            curBcsJob.RobotWIP.CurLocation_StageID = curStage.Data.STAGEID;
                            curBcsJob.RobotWIP.CurLocation_SlotNo = sendOutSlotNo;
                            curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.EQUIPMENT;
                            curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData = sendOut_TrackingData;
                            curBcsJob.RobotWIP.CurPortCstStatusPriority = eLoaderPortSendOutStatus.NOT_IN_PORT;
                            curBcsJob.RobotWIP.CurSendOutJobJudge = sendOut_JobJudge;
                            curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG = eqp_FLAG;//Watson add 20151019 For TTP DailyCheck

                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                    #region [ Add to Can Control Job List ]

                    if (AddToCanControlJoblistCondition(curRobot, curStage, curBcsJob, curStageCanControlJobList, jobKey, funcName) == true)
                    {

                        #region [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) add CassetteSequenceNo({4}) JobSequenceNo({5}) CurRouteStepNo({6}) CurSlotNo({7}) to Can control List!",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.CurLocation_SlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        curSendOutJob = curBcsJob;

                        //GetGetPutPut 在決定好所有SendOutJob後再一次更新Status

                        return true;

                    }
                    else
                    {
                        return false;
                    }

                    #endregion

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4}) ,CSTSeq({5}) JobSeq({6}) Job is not Exist!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, cstSeq, jobSeq);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
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

        /// <summary> 取得Muit Slot Single Mode Stage LDRQ的狀態與EmptySlotInfo
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        private void Get_EqpTypeMuliSlot_LDRQStauts_ForMulitSingle(Robot curRobot, RobotStage curStage)
        {

            string trxID = string.Empty;
            string strlog = string.Empty;

            try
            {
                string bitOn = "1";
                string bitOff = "0";
                string funcName = MethodBase.GetCurrentMethod().Name;

				#region [Get line fabtyep]
				string fabtype = eFabType.ARRAY.ToString();
				Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
				if (line != null) {
					fabtype = line.Data.FABTYPE;
				}
				#endregion

                #region [ Check Trx Setting ]

                if (curStage.Data.DOWNSTREAMPATHTRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find DownStream TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;

                }

                #endregion

                string[] downStreamTrxList = curStage.Data.DOWNSTREAMPATHTRXNAME.Split(',');
                string strSlotNoBin = string.Empty;
                int slotNo = 0;
                int anotherSlotNo = 0;

                for (int i = 0; i < downStreamTrxList.Length; i++)
                {

                    #region  real time Get Interface downstream

                    trxID = downStreamTrxList[i];

                    Trx downStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                    if (downStream_Trx == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                    curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        continue;
                    }

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #endregion

                    #region [拆出PLCAgent Data]

                    #region [ Trx Structure ]

                    //<trx name="L3_DownstreamPath#01" triggercondition="change">
                    //    <eventgroup name="L3_EG_DownstreamPath#01" dir="E2B">
                    //      <event name="L3_B_DownstreamPath#01" trigger="true" />
                    //    </eventgroup>
                    //  </trx>

                    //<event name="L3_B_DownstreamPath#01" devicecode="B" address="0x0000A00" points="32">
                    //  <item name="DownstreamPath#01DownstreamInline" offset="0" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01DownstreamTrouble" offset="1" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveAble" offset="2" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01Receive" offset="3" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01JobTransfer" offset="4" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveCancel" offset="5" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ExchangePossible" offset="6" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01DoubleGlass" offset="7" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveJobReserve" offset="8" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveOK" offset="9" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01TransferStopRequest" offset="10" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01DummyGlassRequest" offset="11" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01GlassExist" offset="12" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01PinUpComplete" offset="13" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01PinDownRequest" offset="14" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#01" offset="16" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#02" offset="17" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#03" offset="18" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#04" offset="19" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#05" offset="20" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#06" offset="21" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01GlassCount#01" offset="22" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01GlassCount#02" offset="23" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01GlassCount#03" offset="24" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01GlassCount#04" offset="25" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#01" offset="26" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#02" offset="27" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#03" offset="28" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#04" offset="29" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#05" offset="30" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#06" offset="31" points="1" expression="BIT" />
                    //</event>

                    #endregion
					#region [variable declare]
					string down_DownstreamInline = "0";
					string down_DownstreamTrouble = "0";
					string down_ReceiveAble = "0";
					string down_Receive = "0";
					string down_JobTransfer = "0";
					string down_ReceiveCancel = "0";
					string down_ExchangePossible = "0";
					string down_DoubleGlass = "0";
					string down_ReceiveJobReserve = "0";
					string down_ReceiveOK = "0";
					string down_TransferStopRequest = "0";
					string down_DummyGlassRequest = "0";
					string down_GlassExist = "0";
					string down_PinUpComplete = "0";
					string down_PinDownRequest = "0";
					string down_SlotNumber01 = "0";
					string down_SlotNumber02 = "0";
					string down_SlotNumber03 = "0";
					string down_SlotNumber04 = "0";
					string down_SlotNumber05 = "0";
					string down_SlotNumber06 = "0";
					string down_GlassCount01 = "0";
					string down_GlassCount02 = "0";
					string down_GlassCount03 = "0";
					string down_GlassCount04 = "0";
					string down_ReceiveType01 = "0";
					string down_ReceiveType02 = "0";
					string down_ReceiveType03 = "0";
					string down_ReceiveType04 = "0";
					string down_ReceiveType05 = "0";
					string down_ReceiveType06 = "0";
					string down_GlassCount05 = "0";
					string down_PreparationPermission = "0";
					string down_InspectionResultUpdate = "0";
					string down_ReturnMode = "0";
					#endregion
					if (fabtype == eFabType.CF.ToString()) {
						#region CF
						down_DownstreamInline = downStream_Trx.EventGroups[0].Events[0].Items[0].Value;
						down_DownstreamTrouble = downStream_Trx.EventGroups[0].Events[0].Items[1].Value;
						down_ReceiveAble = downStream_Trx.EventGroups[0].Events[0].Items[2].Value;
						down_Receive = downStream_Trx.EventGroups[0].Events[0].Items[3].Value;
						down_JobTransfer = downStream_Trx.EventGroups[0].Events[0].Items[4].Value;
						down_ReceiveCancel = downStream_Trx.EventGroups[0].Events[0].Items[5].Value;
						down_ExchangePossible = downStream_Trx.EventGroups[0].Events[0].Items[6].Value;
						down_DoubleGlass = downStream_Trx.EventGroups[0].Events[0].Items[7].Value;
						down_ReceiveJobReserve = downStream_Trx.EventGroups[0].Events[0].Items[8].Value;
						down_TransferStopRequest = downStream_Trx.EventGroups[0].Events[0].Items[9].Value;
						down_DummyGlassRequest = downStream_Trx.EventGroups[0].Events[0].Items[10].Value;
						down_GlassExist = downStream_Trx.EventGroups[0].Events[0].Items[11].Value;
						down_SlotNumber01 = downStream_Trx.EventGroups[0].Events[0].Items[12].Value;
						down_SlotNumber02 = downStream_Trx.EventGroups[0].Events[0].Items[13].Value;
						down_SlotNumber03 = downStream_Trx.EventGroups[0].Events[0].Items[14].Value;
						down_SlotNumber04 = downStream_Trx.EventGroups[0].Events[0].Items[15].Value;
						down_SlotNumber05 = downStream_Trx.EventGroups[0].Events[0].Items[16].Value;
						down_SlotNumber06 = downStream_Trx.EventGroups[0].Events[0].Items[17].Value;
						down_GlassCount01 = downStream_Trx.EventGroups[0].Events[0].Items[18].Value;
						down_GlassCount02 = downStream_Trx.EventGroups[0].Events[0].Items[19].Value;
						down_GlassCount03 = downStream_Trx.EventGroups[0].Events[0].Items[20].Value;
						down_GlassCount04 = downStream_Trx.EventGroups[0].Events[0].Items[21].Value;
						down_GlassCount05 = downStream_Trx.EventGroups[0].Events[0].Items[22].Value;
						down_PreparationPermission = downStream_Trx.EventGroups[0].Events[0].Items[23].Value;
						down_InspectionResultUpdate = downStream_Trx.EventGroups[0].Events[0].Items[24].Value;
						down_ReturnMode = downStream_Trx.EventGroups[0].Events[0].Items[25].Value;

						#endregion
					} else {
						#region default
						down_DownstreamInline = downStream_Trx.EventGroups[0].Events[0].Items[0].Value;
						down_DownstreamTrouble = downStream_Trx.EventGroups[0].Events[0].Items[1].Value;
						down_ReceiveAble = downStream_Trx.EventGroups[0].Events[0].Items[2].Value;
						down_Receive = downStream_Trx.EventGroups[0].Events[0].Items[3].Value;
						down_JobTransfer = downStream_Trx.EventGroups[0].Events[0].Items[4].Value;
						down_ReceiveCancel = downStream_Trx.EventGroups[0].Events[0].Items[5].Value;
						down_ExchangePossible = downStream_Trx.EventGroups[0].Events[0].Items[6].Value;
						down_DoubleGlass = downStream_Trx.EventGroups[0].Events[0].Items[7].Value;
						down_ReceiveJobReserve = downStream_Trx.EventGroups[0].Events[0].Items[8].Value;
						down_ReceiveOK = downStream_Trx.EventGroups[0].Events[0].Items[9].Value;
						down_TransferStopRequest = downStream_Trx.EventGroups[0].Events[0].Items[10].Value;
						down_DummyGlassRequest = downStream_Trx.EventGroups[0].Events[0].Items[11].Value;
						down_GlassExist = downStream_Trx.EventGroups[0].Events[0].Items[12].Value;
						down_PinUpComplete = downStream_Trx.EventGroups[0].Events[0].Items[13].Value;
						down_PinDownRequest = downStream_Trx.EventGroups[0].Events[0].Items[14].Value;
						down_SlotNumber01 = downStream_Trx.EventGroups[0].Events[0].Items[15].Value;
						down_SlotNumber02 = downStream_Trx.EventGroups[0].Events[0].Items[16].Value;
						down_SlotNumber03 = downStream_Trx.EventGroups[0].Events[0].Items[17].Value;
						down_SlotNumber04 = downStream_Trx.EventGroups[0].Events[0].Items[18].Value;
						down_SlotNumber05 = downStream_Trx.EventGroups[0].Events[0].Items[19].Value;
						down_SlotNumber06 = downStream_Trx.EventGroups[0].Events[0].Items[20].Value;
						down_GlassCount01 = downStream_Trx.EventGroups[0].Events[0].Items[21].Value;
						down_GlassCount02 = downStream_Trx.EventGroups[0].Events[0].Items[22].Value;
						down_GlassCount03 = downStream_Trx.EventGroups[0].Events[0].Items[23].Value;
						down_GlassCount04 = downStream_Trx.EventGroups[0].Events[0].Items[24].Value;
						down_ReceiveType01 = downStream_Trx.EventGroups[0].Events[0].Items[25].Value;
						down_ReceiveType02 = downStream_Trx.EventGroups[0].Events[0].Items[26].Value;
						down_ReceiveType03 = downStream_Trx.EventGroups[0].Events[0].Items[27].Value;
						down_ReceiveType04 = downStream_Trx.EventGroups[0].Events[0].Items[28].Value;
						down_ReceiveType05 = downStream_Trx.EventGroups[0].Events[0].Items[29].Value;
						down_ReceiveType06 = downStream_Trx.EventGroups[0].Events[0].Items[30].Value;
						#endregion
					}
                    #endregion

                    //Stage GetGet表示是走ST to RB Mulit Slot Signal Mode
                    if (down_DownstreamInline == bitOn && down_ReceiveAble == bitOn && down_Receive == bitOff)
                    {
                        //LinkSignal Bit符合收片條件 Mulit Slot=> SlotNo= 1st empty SlotNo
                        //注意!!! Array  IMP Stage  SlotNo=1是指上層. 

                        #region [ 將SlotNo Bit 轉成Int ]

                        strSlotNoBin = down_SlotNumber06 + down_SlotNumber05 + down_SlotNumber04 + down_SlotNumber03 + down_SlotNumber02 + down_SlotNumber01;

                        try
                        {
                            slotNo = Convert.ToInt32(strSlotNoBin, 2);
                        }
                        catch (Exception ex1)
                        {
                            slotNo = 0;
                        }

                        #endregion

                        if (slotNo == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report SlotNo is 0!",
                                                        curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                        trxID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //無法確認則視為無法收片 只須更新Stage LDRQ Status即可
                            UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, curStage.File.LDRQ_CstStatusPriority, funcName);

                            break;
                        }

                        #region  [ 根據根據SlotNo與DoubleGlass取得Stage EmptySlotNo ]

                        if (down_DoubleGlass != bitOn)
                        {

                            #region only 1 EmptySlot, 更新Current Stage LDRQ Empty Slot

                            lock (curStage)
                            {
                                curStage.CurLDRQ_EmptySlotNo = slotNo.ToString().PadLeft(2, '0');
                                curStage.CurLDRQ_EmptySlotNo02 = string.Empty;

                                //add Empty SlotNo To EmptySlotNoList
                                if (curStage.curLDRQ_EmptySlotList.ContainsKey(slotNo) == false)
                                {
                                    curStage.curLDRQ_EmptySlotList.Add(slotNo, string.Empty);
                                }
                            }

                            #endregion

                        }
                        else
                        {

                            #region [ has 2 EmptySlot, 更新Current Stage LDRQ Empty Slot ]

                            #region [ Get Another SlotNo ]

                            //Mulit Slot Signal Mode for IMP 只有2各Slot
                            if (slotNo == 1)
                            {
                                anotherSlotNo = 2;
                            }
                            else
                            {
                                anotherSlotNo = 1;
                            }

                            #endregion
                    
                            lock (curStage)
                            {
                                curStage.CurLDRQ_EmptySlotNo = slotNo.ToString().PadLeft(2, '0');

                                //add Empty SlotNo To EmptySlotNoList
                                if (curStage.curLDRQ_EmptySlotList.ContainsKey(slotNo) == false)
                                {
                                    curStage.curLDRQ_EmptySlotList.Add(slotNo, string.Empty);
                                }

                                curStage.CurLDRQ_EmptySlotNo02 = anotherSlotNo.ToString().PadLeft(2, '0');

                                //add Empty SlotNo To EmptySlotNoList
                                if (curStage.curLDRQ_EmptySlotList.ContainsKey(anotherSlotNo) == false)
                                {
                                    curStage.curLDRQ_EmptySlotList.Add(anotherSlotNo, string.Empty);
                                }
                            }

                            #endregion

                        }

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) DownstreamInline({5}) ReceiveAble({6}) Receive({7}) DoubleGlass({8}) EmptySlotNo01({9})  EmptySlotNo02({10}), Stage LDRQ Status change to (LDRQ).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID, (eBitResult)int.Parse(down_DownstreamInline), (eBitResult)int.Parse(down_ReceiveAble), (eBitResult)int.Parse(down_Receive),
                                                    (eBitResult)int.Parse(down_DoubleGlass), curStage.CurLDRQ_EmptySlotNo, curStage.CurLDRQ_EmptySlotNo02);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }


                        #endregion

                        //只須更新Stage LDRQ Status即可
                        UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.RECEIVE_READY, curStage.File.LDRQ_CstStatusPriority, funcName);

                        #endregion

                    }
                    else
                    {
                        //Monitor 條件不合的狀態 Status LDRQ Stage Change To NOREQ
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) DownstreamInline({5}) ReceiveAble({6}) Receive({7}), Stage LDRQ Status can not change to (LDRQ)!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID, (eBitResult)int.Parse(down_DownstreamInline), (eBitResult)int.Parse(down_ReceiveAble), (eBitResult)int.Parse(down_Receive));
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //只須更新Stage LDRQ Status即可
                        UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, curStage.File.LDRQ_CstStatusPriority, funcName);

                    }

                    #region [ 20151215 add 如果Transfer Stop Request On 則要更新Stage狀態 ]

                    if (down_TransferStopRequest == bitOn)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) TransferStopRequest({5}),Update RobotStage Transfer Stop Request Flag(True)!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID, eBitResult.ON.ToString());
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        lock (curStage.File)
                        {
                            curStage.File.DownStreamTransferStopRequestFlag = true;
                        }

                    }
                    else
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) TransferStopRequest({5}),Update RobotStage Transfer Stop Request Flag(False)!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                    curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        lock (curStage.File)
                        {
                            curStage.File.DownStreamTransferStopRequestFlag = false;
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

        private void JudgeEQPStage_UDRQ_LDRQStatus_ForMulitSingle(RobotStage curStage)
        {

            string strlog = string.Empty;
            string tmpStageStatus = string.Empty;

            try
            {

                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                #region [ 比對UDRQ and LDRQ Stage Status 判斷最終狀態 ]

                if (curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_READY || curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                {
                    if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                    {
                        //可出片同時可收片是有問題的!!! EQP Stage不能同時出現LDRQ and UDRQ
                        tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                    }
                    else
                    {
                        //可出片 不可收片
                        tmpStageStatus = eRobotStageStatus.SEND_OUT_READY;

                    }

                }
                else
                {
                    if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                    {
                        //不可出片 可收片
                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

                    }
                    else
                    {
                        //不可出片 不可收片
                        tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                    }

                }

                #endregion

                #region [ 更新Stage Status ]

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Stage UDRQ Status({4}), LDRQ Status({5}) , Judge Stage Status({6})!",
                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                            curStage.File.Stage_UDRQ_Status, curStage.File.Stage_LDRQ_Status, tmpStageStatus);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }

                #endregion

                if (tmpStageStatus == eRobotStageStatus.SEND_OUT_READY || tmpStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                {

                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, curStage.File.CurSendOut_CSTSeq, curStage.File.CurSendOut_JobSeq, curStage.File.CurSendOut_CSTSeq02, curStage.File.CurSendOut_JobSeq02);
                }
                else
                {
                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty, string.Empty, string.Empty);
                }

                #endregion
            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        #endregion ==========================================================================================================================================================================


        /// <summary> for PortType Stage Only,因為可上多種形態的CST,所以要根據目前Port上報的Max Slot Count 來更新PortType Stage的Max Slot Count
        ///
        /// </summary>
        /// <param name="curStage"></param>
        /// <param name="curPort"></param>
        private void UpdatePortStageMaxSlotCount(RobotStage curStage, Port curPort)
        {
            int portReportMaxCount = 0;
            int tmpCount = 0;
            string strlog = string.Empty;

            try
            {
                #region [ 數量相符時則不更新 ]

                int.TryParse(curPort.File.MaxSlotCount, out portReportMaxCount);

                if (curStage.Data.SLOTMAXCOUNT == portReportMaxCount)
                {
                    return;
                }

                #endregion

                #region [ EQ上報數量為0時則不更新 ]

                if (portReportMaxCount == 0)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) StageCSTType({4}) Setting SlotCount({5}) and Port MaxSlotCount({6}) can not update Stage MaxSlotCount!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.CASSETTETYPE, curStage.Data.SLOTMAXCOUNT, portReportMaxCount);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;
                }

                #endregion

                #region [ Update Stage Info ]

                tmpCount = curStage.Data.SLOTMAXCOUNT;

                lock (curStage)
                {
                    curStage.Data.SLOTMAXCOUNT = portReportMaxCount;
                }

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Update Setting SlotCount from ({5}) to Port MaxSlotCount({6}).",
                                        curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                        curStage.Data.CASSETTETYPE, tmpCount, portReportMaxCount);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #endregion

            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> 整合Stage 的UDRQ and LDRQ Status 來定義Stage的最終定義
        ///
        /// </summary>
        /// <param name="curStage"></param>
        private void JudgePortStage_UDRQ_LDRQStatus(RobotStage curStage , Port curPort)
        {
            string strlog = string.Empty;
            string tmpStageStatus = string.Empty;

            try
            {
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                #region [ 比對UDRQ and LDRQ Stage Status 判斷最終狀態 ]

                if (curPort.File.Type == ePortType.BothPort)
                {

                    #region [ 20151029 add Is Both Port 只要有UDRQ 或者 LDRQ則通通視為SEND_OUT_AND_RECEIVE_READY ]

                    if (curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_READY || curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                    {
                        if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                        {
                            //可出片 可收片
                            tmpStageStatus = eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY;

                        }
                        else
                        {
                            //可出片 不可收片
                            tmpStageStatus = eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY;

                        }

                    }
                    else
                    {
                        if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                        {
                            //不可出片 可收片
                            tmpStageStatus = eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY;

                        }
                        else
                        {
                            //不可出片 不可收片
                            tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                        }

                    } 

                    #endregion

                }
                else
                {

                    #region [ Not Both Port ]

                    if (curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_READY || curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                    {
                        if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                        {
                            //可出片 可收片
                            tmpStageStatus = eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY;

                        }
                        else
                        {
                            //可出片 不可收片
                            tmpStageStatus = eRobotStageStatus.SEND_OUT_READY;

                        }

                    }
                    else
                    {
                        if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                        {
                            //不可出片 可收片
                            tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

                        }
                        else
                        {
                            //不可出片 不可收片
                            tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                        }

                    }

                    #endregion

                }

                #endregion

                #region [ 更新Stage Status ]

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) PortType({4}) CassetteStatus({5}) Stage UDRQ Status({6}), LDRQ Status({7}) , Judge Stage Status({8})!",
                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                            curPort.File.Type, curPort.File.CassetteStatus, curStage.File.Stage_UDRQ_Status, curStage.File.Stage_LDRQ_Status,
                                            tmpStageStatus);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }

                #endregion

                if (tmpStageStatus == eRobotStageStatus.SEND_OUT_READY || tmpStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                {
                    
                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, curStage.File.CurSendOut_CSTSeq, curStage.File.CurSendOut_JobSeq);
                }
                else
                {
                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                }

                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>20160106 add for 新需求 MAC RecipeID最後一碼(第12碼)為"1"不可以Exchange
        /// 
        /// </summary>
        /// <param name="curStage"></param>
        /// <returns></returns>
        private bool CheckMAC_Can_ExchangeFlag(RobotStage curStage)
        {
            string strlog = string.Empty;

            try
            {

                #region [Get EQP Entity by Stage NodeNo ]

                Equipment stageEQP = ObjectManager.EquipmentManager.GetEQP(curStage.Data.NODENO);

                if (stageEQP == null)
                {

                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) can not find EQP by EQPNo({4})!",
                                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGENAME, curStage.Data.STAGEID, curStage.Data.NODENO);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    return false;
                }

                #endregion

                #region [ Check EQP Current RecipeID Length 等於設定長度 ]

                if ((stageEQP.File.CurrentRecipeID == string.Empty) || (stageEQP.File.CurrentRecipeID.Length != stageEQP.Data.RECIPELEN))
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) find EQPNo({4}) currentRecipeID({5}) length is not ({6})!",
                                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGENAME, curStage.Data.STAGEID,
                                                                curStage.Data.NODENO, stageEQP.File.CurrentRecipeID, stageEQP.Data.RECIPELEN.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    
                    #endregion

                    return false;

                }

                #endregion

                string macRecipeLastCode = string.Empty;

                macRecipeLastCode = stageEQP.File.CurrentRecipeID.Substring(stageEQP.Data.RECIPELEN - 1, 1);

                if (macRecipeLastCode == "1")
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) find EQPNo({4}) currentRecipeID({5}) Last Code is 1 can not Exchange!",
                                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGENAME, curStage.Data.STAGEID,
                                                                curStage.Data.NODENO, stageEQP.File.CurrentRecipeID, stageEQP.Data.RECIPELEN.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) RobotStage({2}) StageNo({3}) find EQPNo({4}) currentRecipeID({5}) Last Code is not 1 can Exchange!",
                                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGENAME, curStage.Data.STAGEID,
                                                                curStage.Data.NODENO, stageEQP.File.CurrentRecipeID, stageEQP.Data.RECIPELEN.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                }

                return true;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        //20160805 只要找到有可出片的Job就傳回true
        private bool CheckCSTHasJobUDRQ(RobotStage curStage)
        {
            #region  [ Real time Get Port Slot Exist Info Trx ]

            //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
            //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotExistsBlock    
            string trxID = GetTrx_CSTSlotJobExistInfo(curStage.Data.NODENO, curStage.Data.STAGEID);
            string strlog = string.Empty;
            Trx get_CSTSlot_ExistInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
            string jobTrx_GroupName = string.Empty;
            string jobTrx_EventName = string.Empty;
            string jobTrx_CstSeqkey = string.Empty;
            string jobTrx_JobSeqkey = string.Empty;

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

                //UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                return false;

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

                //UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                return false;

            }

            #endregion
            bool checkCSTHasJobUDRQ = false;
            int job_CstSeq = 0;
            int job_JobSeq = 0;
            int job_ExistInfo = ePortSlotExistInfo.JOB_NO_EXIST;
            string allSlotExistInfo = get_CSTSlot_ExistInfo_Trx.EventGroups[0].Events[0].Items[0].Value;
            
            for (int i = 1; i <= curStage.Data.SLOTMAXCOUNT; i++)
            {
                //checkReasonCode = Get_CSTSlot_CanControlJoblist(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList, curPort, curRobot);
                jobTrx_GroupName = string.Format("{0}_EG_Port#{1}JobEachCassetteSlotPositionBlock", curStage.Data.NODENO, curStage.Data.STAGEID.PadLeft(2, '0'));
                jobTrx_EventName = string.Format("{0}_W_Port#{1}JobEachCassetteSlotPositionBlock", curStage.Data.NODENO, curStage.Data.STAGEID.PadLeft(2, '0'));
                jobTrx_CstSeqkey = string.Format("SlotPosition#{0}CassetteSequenceNo", i.ToString().PadLeft(3, '0'));
                jobTrx_JobSeqkey = string.Format("SlotPosition#{0}JobSequenceNo", i.ToString().PadLeft(3, '0'));
                job_CstSeq = int.Parse(get_CSTSlot_JobInfo_Trx.EventGroups[jobTrx_GroupName].Events[jobTrx_EventName].Items[jobTrx_CstSeqkey].Value);
                job_JobSeq = int.Parse(get_CSTSlot_JobInfo_Trx.EventGroups[jobTrx_GroupName].Events[jobTrx_EventName].Items[jobTrx_JobSeqkey].Value);
                job_ExistInfo = int.Parse(allSlotExistInfo.Substring(i - 1, 1));
                if (job_CstSeq > 0 && job_JobSeq > 0 && job_ExistInfo == ePortSlotExistInfo.JOB_EXIST)
                {
                    checkCSTHasJobUDRQ = true;
                    break;
                }
                else
                {
                    continue;
                }

            }
            return checkCSTHasJobUDRQ;
        }

        //20160805 只要有找到空的Slot就傳回true
        private bool CheckEmptySlotCanLDRQ(RobotStage curStage)
        {
            #region  [ Real time Get Port Slot Exist Info Trx ]

            //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
            //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotExistsBlock    
            string trxID = GetTrx_CSTSlotJobExistInfo(curStage.Data.NODENO, curStage.Data.STAGEID);
            string strlog = string.Empty;
            Trx get_CSTSlot_ExistInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
            string jobTrx_GroupName = string.Empty;
            string jobTrx_EventName = string.Empty;
            string jobTrx_CstSeqkey = string.Empty;
            string jobTrx_JobSeqkey = string.Empty;

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

                //UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                return false;

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

                //UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                return false;

            }
            #endregion
            bool checkEmptySlotCanLDRQ = false;
            int job_CstSeq = 0;
            int job_JobSeq = 0;
            int job_ExistInfo = ePortSlotExistInfo.JOB_NO_EXIST;
            string allSlotExistInfo = get_CSTSlot_ExistInfo_Trx.EventGroups[0].Events[0].Items[0].Value;
            for (int i = 1; i <= curStage.Data.SLOTMAXCOUNT; i++)
            {
                //checkReasonCode = Get_CSTSlot_CanControlJoblist(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList, curPort, curRobot);
                jobTrx_GroupName = string.Format("{0}_EG_Port#{1}JobEachCassetteSlotPositionBlock", curStage.Data.NODENO, curStage.Data.STAGEID.PadLeft(2, '0'));
                jobTrx_EventName = string.Format("{0}_W_Port#{1}JobEachCassetteSlotPositionBlock", curStage.Data.NODENO, curStage.Data.STAGEID.PadLeft(2, '0'));
                jobTrx_CstSeqkey = string.Format("SlotPosition#{0}CassetteSequenceNo", i.ToString().PadLeft(3, '0'));
                jobTrx_JobSeqkey = string.Format("SlotPosition#{0}JobSequenceNo", i.ToString().PadLeft(3, '0'));
                job_CstSeq = int.Parse(get_CSTSlot_JobInfo_Trx.EventGroups[jobTrx_GroupName].Events[jobTrx_EventName].Items[jobTrx_CstSeqkey].Value);
                job_JobSeq = int.Parse(get_CSTSlot_JobInfo_Trx.EventGroups[jobTrx_GroupName].Events[jobTrx_EventName].Items[jobTrx_JobSeqkey].Value);
                job_ExistInfo = int.Parse(allSlotExistInfo.Substring(i - 1, 1));
                if (job_CstSeq == 0 && job_JobSeq == 0 && job_ExistInfo == ePortSlotExistInfo.JOB_NO_EXIST)
                {
                    checkEmptySlotCanLDRQ = true;
                    break;
                }
                else
                {
                    continue;
                }

            }
            return checkEmptySlotCanLDRQ;
        }

    }
}
