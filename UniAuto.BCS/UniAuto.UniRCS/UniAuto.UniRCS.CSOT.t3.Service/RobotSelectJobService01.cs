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
    public partial  class RobotSelectJobService
    {
        //All Robot Select Stage Job Condition Function List [ Method Name = "Select" + "_" + 對象Stage + "_" + 狀態(Job or Stage Status) ]======================================================================================================================================

        #region [ 20151125 mark 已廢除的Function ]

        #region [ 20151109 Mark_003 目前沒用到 ]

        /// <summary> Select All Stage Type(Internal) Stage Can Control Job List by GetGetPutPut
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //public bool Select_Stage_ForGetGetPutPut(IRobotContext robotConText)
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

        #region [ 20151109 Mark_002 目前沒用到 ]

        /// <summary> Select All Port Type Stage Can Control Job List by GetGetPutPut
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //public bool Select_Port_ForGetGetPutPut(IRobotContext robotConText)
        //{
        //    string strlog = string.Empty;
        //    string errMsg = string.Empty;
        //    List<RobotStage> curRobotStageList = null;
        //    List<Job> robotStageCanControlJobList;

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

        //            robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_curRobot_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get CurRobot All Stage List ]

        //        curRobotStageList = (List<RobotStage>)robotConText[eRobotContextParameter.CurRobotAllStageListEntity];

        //        //找不到 Robot Stage 回NG
        //        if (curRobotStageList == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo!",
        //                                        curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get RobotStageInfo!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_RobotStageList_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        if (curRobotStageList.Count == 0)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo!",
        //                                        curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Get RobotStageInfo is Empty!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_RobotStageList_Is_Empty);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get Current Stage Can Control Job List ]

        //        robotStageCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.StageCanControlJobList];

        //        //當沒有值時則要建立
        //        if (robotStageCanControlJobList == null)
        //        {
        //            robotStageCanControlJobList = new List<Job>();
        //        }

        //        #endregion

        //        foreach (RobotStage stage_entity in curRobotStageList)
        //        {

        //            //非Port Type Stage則不判斷
        //            if (stage_entity.Data.STAGETYPE.ToUpper().Trim() != eRobotStageType.PORT)
        //            {
        //                continue;
        //            }

        //            Get_PortTypeStage_CanControlJobList_ForGetGet(curRobot, stage_entity, robotStageCanControlJobList);

        //        }

        //        robotConText.AddParameter(eRobotContextParameter.StageCanControlJobList, robotStageCanControlJobList);
        //        robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.Result_Is_OK);
        //        robotConText.SetReturnMessage(eRobotSelectJob_ReturnMessage.OK_Message);


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

//        private void Get_PortTypeStage_CanControlJobList_ForGetGet(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList)
//        {
//             string tmpStageStatus = string.Empty;
//            string strlog = string.Empty;

//            try
//            {
//                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

//                #region [ Get Port Entity by StageID , 如果找不到則 Stage Status =NOREQ ]

//                Port curPort = ObjectManager.PortManager.GetPort(curStage.Data.STAGEID);

//                if (curPort == null)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) RobotStageName({3}) can not get Port Entity!",
//                                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME);

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                    }

//                    #endregion

//                    ClearStageStatusForGetGetPutPut(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name );

//                    return;
//                }

//                #endregion

//                #region [ Check Port Enable Mode Is Enable ]

//                if (curPort.File.EnableMode != ePortEnableMode.Enabled)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Port Enable Mode is ({4})!",
//                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curPort.File.EnableMode);

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                    }

//                    #endregion

//                    ClearStageStatusForGetGetPutPut(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

//                    return;
//                }

//                #endregion

//                #region [ Check Port Down Status Is Normal(not down) ]

//                if (curPort.File.DownStatus != ePortDown.Normal)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Port DownStatus is ({4})!",
//                                               curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                               curPort.File.DownStatus);

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                    }

//                    #endregion

//                    ClearStageStatusForGetGetPutPut(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

//                    return;
//                }

//                #endregion

//                UpdatePortStageMaxSlotCount(curStage, curPort);

//                #region [ by Port Update StageStatus and Get Can control Joblist ]

//                if (curPort.File.Type == ePortType.LoadingPort &&
//                    (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING || curPort.File.CassetteStatus == eCassetteStatus.IN_PROCESSING))
//                {

//                    //For Loader Port Get Can control Joblist
//                    Get_LoaderPort_CanControlJoblistForGetGetPutPut(curRobot, curStage, curStageCanControlJobList, curPort);

//                }
//                else if (curPort.File.Type == ePortType.UnloadingPort &&
//                    (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING || curPort.File.CassetteStatus == eCassetteStatus.IN_PROCESSING))
//                {
//                    //For Unload Port Get Can control Joblist
//                    Get_UnloadPort_StageStatusInfoForGetGetPutPut(curRobot, curStage, curPort);

//                }
//                else if (curPort.File.Type == ePortType.BothPort &&
//                    (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING || curPort.File.CassetteStatus == eCassetteStatus.IN_PROCESSING))
//                {
//                    //For Both Port Get Can control Joblist
//                    //Step01 Check UDRQ Status
//                    Get_BothPort_CanControlJoblistForGetGetPutPut(curRobot, curStage, curStageCanControlJobList, curPort);

//                    //Step02 Check LDRQ Status
//                    Get_BothPort_StageLDRQStatusInfoForGetGetPutPut(curRobot, curStage, curPort);

//                    //Step03 Judge Stage Status
//                    JudgePortStage_UDRQ_LDRQStatus(curStage, curPort);

//                }
//                else
//                {

//                    #region [ 狀態都不符合收送片條件時則視為NOREQ並更新Stage Status ]

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) PortType({4}) CassetteStatus({5}) is illegal!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curPort.File.Type, curPort.File.CassetteStatus);

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    ClearStageStatusForGetGetPutPut(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

//                    #endregion

//                }

//                #endregion

//            }
//            catch (Exception ex)
//            {
//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//            }
                
//        }

//        #region 获取Loader Port的可控制玻璃
//        private void Get_LoaderPort_CanControlJoblistForGetGetPutPut(Robot curRobot, RobotStage curStage, List<Job> curCanCtlJobList, Port curPort)
//        {
//            string tmpStageStatus = string.Empty;
//            string trxID = string.Empty;
//            string strlog = string.Empty;
//            string checkReasonCode = string.Empty;
//            Job TempJob = new Job();//记录临时的CanControlJob资料
//            int iCanControlJobCount = 0;//记录已经找到几个可控制的玻璃
//            Dictionary<int, Job> DicTempJobList = new Dictionary<int, Job>();//记录找到的可控制玻璃资讯
//            try
//            {
//                //預設為NoReq
//                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

//                #region  [ Real time Get Port Slot Exist Info Trx ]

//                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
//                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotExistsBlock    
//                trxID = GetTrx_CSTSlotJobExistInfo(curStage.Data.NODENO, curStage.Data.STAGEID);

//                Trx get_CSTSlot_ExistInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

//                if (get_CSTSlot_ExistInfo_Trx == null)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not Get TrxID({4})!",
//                                               curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                               trxID);
//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                    }

//                    #endregion

//                    ClearStageStatusForGetGetPutPut(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

//                    return;

//                }

//                #endregion

//                #region  [ Real time Get Port Slot Job Info Trx ]

//                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
//                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotPositionBlock               
//                trxID = GetTrx_CSTSlotJobEachPosition(curStage.Data.NODENO, curStage.Data.STAGEID);

//                Trx get_CSTSlot_JobInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

//                if (get_CSTSlot_JobInfo_Trx == null)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not Get TrxID({4})!",
//                                               curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                               trxID);
//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                    }

//                    #endregion

//                    ClearStageStatusForGetGetPutPut(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

//                    return;

//                }

//                #endregion

//                //根據Robot設定的取片順序來決定要從Slot01開始抽還是SlotMax開始抽 ASC 从小到大(Priority 01>02>03>…) ,DESC 从大到小 (Priority 01<02<03>…), default : ASC 从小到大          
//                if (curRobot.Data.SLOTFETCHSEQ == "ASC")
//                {

//                    #region [ 抽片順序為ASC SlotNo由小到大 ]

//                    for (int i = 1; i <= curStage.Data.SLOTMAXCOUNT; i++)
//                    {
//                        checkReasonCode = Get_CSTSlot_CanControlJoblistForGetGetPutPut(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList, TempJob, curPort, curRobot);

//                        #region [ 根據Port CST Type來決定是否要判斷下一片 ]

//                        if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
//                        {
//                            #region [ 非循序CST的處理,可跳Slot ]

//                            switch (checkReasonCode)
//                            {
//                                case ePortJobUDRQReason.REASON_OK:
//                                    iCanControlJobCount++;
//                                    DicTempJobList.Add(iCanControlJobCount, TempJob);
//                                    break ;

//                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

//                                    //確認是空Slot則判斷下一個Slot
//                                    break;

//                                default:

//                                    //其他錯誤是為找不到 則跳下一個Slot判斷
//                                    break;
//                            }

//                            #endregion
//                        }
//                        else
//                        {
//                            #region [ 循序CST的處理,不可跳Slot ]

//                            switch (checkReasonCode)
//                            {
//                                case ePortJobUDRQReason.REASON_OK:
//                                    iCanControlJobCount++;
//                                    DicTempJobList.Add(iCanControlJobCount, TempJob);
//                                    break;

//                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

//                                    //確認是空Slot則判斷下一個Slot
//                                    break;

//                                default:

//                                    //其他錯誤視為找不到或是帳料有問題需更新Stage Status並紀錄Log,或不满足两片Job的时候

//                                    UpdateStageStatusForGetGetPutPut(curStage,  MethodBase.GetCurrentMethod().Name, DicTempJobList);

//                                    return;
//                            }

//                            #endregion
//                        }

//                        #endregion

//                        if (iCanControlJobCount == 2)
//                        {
//                            //如果已经达到2个可控制的Job就退出循环
//                            break;
//                        }
//                    }


//                    //找不到可控制的玻璃、只找到一片可控制的玻璃、找到两片可控制的玻璃都要去检查 更新 Stage Status

//                    UpdateStageStatusForGetGetPutPut(curStage,  MethodBase.GetCurrentMethod().Name, DicTempJobList);


//                    #endregion

//                }
//                else
//                {

//                    #region [ 抽片順序為DESC SlotNo由大到小 ]

//                    for (int i = curStage.Data.SLOTMAXCOUNT; i > 0; i--)
//                    {
//                        checkReasonCode = Get_CSTSlot_CanControlJoblistForGetGetPutPut(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList, TempJob, curPort, curRobot);

//                        #region [ 根據Port CST Type來決定是否要判斷下一片 ]

//                        if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
//                        {

//                            #region [ 非循序CST的處理,可跳Slot ]

//                            switch (checkReasonCode)
//                            {
//                                case ePortJobUDRQReason.REASON_OK:
//                                    iCanControlJobCount++;
//                                    DicTempJobList.Add(iCanControlJobCount, TempJob);
//                                    break;

//                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

//                                    //確認是空Slot則判斷下一個Slot
//                                    break;

//                                default:

//                                    //其他錯誤是為找不到 則跳下一個Slot判斷
//                                    break;
//                            }

//                            #endregion

//                        }
//                        else
//                        {

//                            #region [ 循序CST的處理,不可跳Slot ]

//                            switch (checkReasonCode)
//                            {
//                                case ePortJobUDRQReason.REASON_OK:
//                                    iCanControlJobCount++;
//                                    DicTempJobList.Add(iCanControlJobCount, TempJob);
//                                    break;

//                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

//                                    //確認是空Slot則判斷下一個Slot
//                                    break;

//                                default:

//                                    //其他錯誤視為找不到或是帳料有問題需更新Stage Status並紀錄Log,或不满足两片Job的时候

//                                    UpdateStageStatusForGetGetPutPut(curStage,  MethodBase.GetCurrentMethod().Name, DicTempJobList);

//                                    return;
//                            }

//                            #endregion

//                        }

//                        if (iCanControlJobCount == 2)
//                        {
//                            //如果已经达到2个可控制的Job就退出循环
//                            break;
//                        }
//                        #endregion
//                    }

//                    //找不到可控制的玻璃、只找到一片可控制的玻璃、找到两片可控制的玻璃都要去检查 更新 Stage Status
//                    UpdateStageStatusForGetGetPutPut(curStage, MethodBase.GetCurrentMethod().Name, DicTempJobList);

//                    #endregion

//                }

//            }
//            catch (Exception ex)
//            {
//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        /// <summary>更新当前Stage Status
//        /// 
//        /// </summary>
//        /// <param name="curStage">当前Stage</param>
//        /// <param name="newStageStatus">当前StageStatus</param>
//        /// <param name="funcName">Call Function</param>
//        /// <param name="sendOutCstSeq">FirstCSTSeq</param>
//        /// <param name="sendOutJobSeq">FirstJobSeq</param>
//        /// <param name="IsUpdatesendOutJob02">是否更新Second Job Info</param>
//        /// <param name="sendOutCstSeq02">SecondCSTSeq</param>
//        /// <param name="sendOutJobSeq02">SecondJobSeq</param>
//        private void UpdateStageStatusForGetGetPutPut(RobotStage curStage, string funcName,Dictionary<int ,Job >dicCanControlJob)
//        {
//            string strlog = string.Empty;
//            string newStageStatus = eRobotStageStatus.NO_REQUEST;

//            try
//            {
//                string sendOutCstSeq01 = "";
//                string sendOutJobSeq01 = "";
//                string sendOutCstSeq02 = "";
//                string sendOutJobSeq02 = "";

//                switch (dicCanControlJob.Count)
//                {
//                    case 0:
//                        newStageStatus=eRobotStageStatus.NO_REQUEST;
//                        ClearStageStatusForGetGetPutPut(curStage, newStageStatus, funcName);
//                        return;
//                    case 1:
//                        newStageStatus = eRobotStageStatus.SEND_OUT_READY;
//                        sendOutCstSeq01 = dicCanControlJob[1].CassetteSequenceNo;
//                        sendOutJobSeq01 = dicCanControlJob[1].JobSequenceNo;
//                        break;
//                    case 2:
//                        newStageStatus = eRobotStageStatus.SEND_OUT_READY;
//                        sendOutCstSeq01 = dicCanControlJob[1].CassetteSequenceNo;
//                        sendOutJobSeq01 = dicCanControlJob[1].JobSequenceNo;
//                        sendOutCstSeq02 = dicCanControlJob[2].CassetteSequenceNo;
//                        sendOutJobSeq02 = dicCanControlJob[2].JobSequenceNo;
//                        break;
//                    default:
//                        break;
//                }

//                #region [ Stage Status Change才需要Update ]

//                if (curStage.File.CurStageStatus != newStageStatus)
//                {

//                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageNo({3}) StageName({4}) status change from ({5}) to ({6}) , UDRQ Job01 CassetteSequenceNo({7}) JobSequenceNo({8}) Job02 CassetteSequenceNo({9}) JobSequenceNo({10})",
//                                                    curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                                    curStage.Data.STAGENAME, curStage.File.CurStageStatus, newStageStatus, sendOutCstSeq01,
//                                                    sendOutJobSeq01,
//                                                    sendOutCstSeq02, sendOutJobSeq02).Replace(Environment.NewLine, ""); ;

//                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    //add for Log Quick Trace
//                    strlog = string.Format("[{0}] {1} - [{2}]{3}({4}) Stage Status Change From({5}) to ({6}) ,sendOut Job01({7},{8}),sendOut Job02({9},{10})",
//                                            "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
//                                            MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
//                                            funcName,
//                                            curStage.Data.STAGENAME,
//                                            curStage.Data.STAGEID, curStage.File.CurStageStatus, newStageStatus, sendOutCstSeq01,
//                                            sendOutJobSeq01,
//                                            sendOutCstSeq02, sendOutJobSeq02);

//                    Logger.LogTrxWrite(this.LogName, strlog);

//                    #region [ Update Robot Stage Entity ]

//                    lock (curStage.File)
//                    {
//                            curStage.File.CurStageStatus = newStageStatus;
//                            curStage.File.CurSendOut_CSTSeq = sendOutCstSeq01;
//                            curStage.File.CurSendOut_JobSeq = sendOutJobSeq01;
//                            curStage.File.CurSendOut_CSTSeq02 = sendOutCstSeq02;
//                            curStage.File.CurSendOut_JobSeq02 = sendOutJobSeq02;
//                            curStage.File.StatusChangeFlag = true;
//                    }

//                    #endregion

//                }

//                #endregion

//                #region  [DebugLog]

//                if (IsShowDetialLog == true)
//                {

//                    //Get Current Stage Info To Log
//                    strlog = string.Format(@"[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageNo({3}) StageName({4}) 
//status change from ({5}) to ({6}) , 
//UDRQ Job01 CassetteSequenceNo({7}) JobSequenceNo({8}) 
//Job02 CassetteSequenceNo({9}) JobSequenceNo({10})",
//                                                    curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                                    curStage.Data.STAGENAME, curStage.File.CurStageStatus, newStageStatus, sendOutCstSeq01,
//                                                    sendOutJobSeq01,
//                                                    sendOutCstSeq02, sendOutJobSeq02).Replace(Environment.NewLine, "");

//                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                }

//                #endregion

//            }
//            catch (Exception ex)
//            {
//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        private void ClearStageStatusForGetGetPutPut(RobotStage curStage, string newStageStatus, string funcName)
//        {
//            string strlog = string.Empty;

//            try
//            {

//                #region [ Stage Status Change才需要Update ]

//                if (curStage.File.CurStageStatus != newStageStatus)
//                {

//                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageNo({3}) StageName({4}) status change from ({5}) to ({6}) , UDRQ Job01 CassetteSequenceNo({7}) JobSequenceNo({8}),UDRQ Job02 CassetteSequenceNo({9}) JobSequenceNo({10})!",
//                                                    curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                                    curStage.Data.STAGENAME, curStage.File.CurStageStatus, newStageStatus, string.Empty ,
//                                                    string.Empty, string.Empty,string.Empty);

//                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    //add for Log Quick Trace
//                    strlog = string.Format("[{0}] {1} - [{2}]{3}({4}) Stage Status Change From({5}) to ({6}) ,sendOut Job01({7},{8}),sendOut Job02({9},{10})",
//                                            "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
//                                            MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
//                                            funcName,
//                                            curStage.Data.STAGENAME,
//                                            curStage.Data.STAGEID, curStage.File.CurStageStatus, newStageStatus, string.Empty,
//                                            string.Empty, 
//                                            string.Empty,string.Empty);

//                    Logger.LogTrxWrite(this.LogName, strlog);

//                    #region [ Update Robot Stage Entity ]

//                    lock (curStage.File)
//                    {

//                            curStage.File.CurStageStatus = newStageStatus;
//                            curStage.File.CurSendOut_CSTSeq = string.Empty;
//                            curStage.File.CurSendOut_JobSeq = string.Empty;
//                            curStage.File.CurSendOut_CSTSeq02 = string.Empty;
//                            curStage.File.CurSendOut_JobSeq02 = string.Empty;
//                            curStage.File.StatusChangeFlag = true;
//                    }

//                    #endregion

//                }

//                #endregion

//                #region  [DebugLog]

//                if (IsShowDetialLog == true)
//                {

//                    //Get Current Stage Info To Log
//                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageNo({3}) StageName({4}) status change from ({5}) to ({6}) , UDRQ Job01 CassetteSequenceNo({7}) JobSequenceNo({8}),UDRQ Job02 CassetteSequenceNo({9}) JobSequenceNo({10})!",
//                                                    curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                                    curStage.Data.STAGENAME, curStage.File.CurStageStatus, newStageStatus, string.Empty,
//                                                    string.Empty, string.Empty, string.Empty);

//                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                }

//                #endregion

//            }
//            catch (Exception ex)
//            {
//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        #endregion

//        #region UnloadPort_StageStatus


//        /// <summary>
//        /// 確認Unload Port Slot是否有空Slot存在並更新Stage Status
//        /// </summary>
//        /// <param name="curRobot"></param>
//        /// <param name="curStage"></param>
//        /// <param name="curPort"></param>
//        private void Get_UnloadPort_StageStatusInfoForGetGetPutPut(Robot curRobot, RobotStage curStage, Port curPort)
//        {
//            string jobKey = string.Empty;
//            string trxID = string.Empty;
//            string strlog = string.Empty;
//            string tmpStageStatus = string.Empty;
//            string tmpCstStatusPriority = string.Empty;
//            //bool findEmptySlotFlag = false;
//            int iCanReceiveJobCount = 0;//记录可收片的数量，至多找两片空的Slot就返回
//            try
//            {

//                //預設為NoReq
//                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

//                #region [ Set Unload Receive Job Priority ]

//                if (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)
//                {
//                    tmpCstStatusPriority = eUnloadPortReceiveStatus.ULD_PORT_WAIT_PROCESS;
//                }
//                else
//                {
//                    tmpCstStatusPriority = eUnloadPortReceiveStatus.ULD_PORT_IN_PROCESS;
//                }


//                #endregion

//                #region  [ Real time Get Port Slot Exist Info Trx ]

//                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
//                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotExistsBlock    
//                trxID = GetTrx_CSTSlotJobExistInfo(curStage.Data.NODENO, curStage.Data.STAGEID);

//                Trx get_CSTSlot_ExistInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

//                if (get_CSTSlot_ExistInfo_Trx == null)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not Get TrxID({4})!",
//                                               curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                               trxID);
//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                    }

//                    #endregion

//                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

//                    return;

//                }

//                #endregion

//                #region  [ Real time Get Port Slot Job Info Trx ]

//                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
//                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotPositionBlock               
//                trxID = GetTrx_CSTSlotJobEachPosition(curStage.Data.NODENO, curStage.Data.STAGEID);

//                Trx get_CSTSlot_JobInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

//                if (get_CSTSlot_JobInfo_Trx == null)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not Get TrxID({4})!",
//                                               curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                               trxID);
//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                    }

//                    #endregion

//                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

//                    return;

//                }

//                #endregion

//                //先将空Slot的讯息清空
//                curStage.CurLDRQ_EmptySlotNo = "";
//                curStage.CurLDRQ_EmptySlotNo02 = "";

//                //根據Robot設定的放片順序來決定要從Slot01開始放在還是SlotMax開始放 ASC 从小到大(Priority 01>02>03>…) ,DESC 从大到小 (Priority 01<02<03>…), default : ASC 从小到大          
//                if (curRobot.Data.SLOTSTORESEQ == "DESC")
//                {

//                    #region [ 放片順序為DESC SlotNo由大到小 ]

//                    for (int i = curStage.Data.SLOTMAXCOUNT; i > 0; i--)
//                    {

//                        #region [ Check Unlaod Port Status LDRQ ]

//                        if (Check_CSTSlot_IsEmptyForGetGetPutPut(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i) == true)
//                        {

//                            //Update Current Stage LDRQ Empty Slot
//                            iCanReceiveJobCount++;
//                            lock (curStage)
//                            {
//                                if (iCanReceiveJobCount == 1)
//                                {
//                                    curStage.CurLDRQ_EmptySlotNo = i.ToString().PadLeft(3, '0');

//                                    //add Empty SlotNo To EmptySlotNoList
//                                    if (curStage.curLDRQ_EmptySlotList.ContainsKey(i) == false)
//                                    {
//                                        curStage.curLDRQ_EmptySlotList.Add(i, string.Empty);
//                                    }

//                                }
//                                else
//                                {
//                                    curStage.CurLDRQ_EmptySlotNo02 = i.ToString().PadLeft(3, '0');

//                                    //add Empty SlotNo To EmptySlotNoList
//                                    if (curStage.curLDRQ_EmptySlotList.ContainsKey(i) == false)
//                                    {
//                                        curStage.curLDRQ_EmptySlotList.Add(i, string.Empty);
//                                    }

//                                }
//                            }
//                            if (iCanReceiveJobCount == 2)
//                            {
//                                //如果已经找到2个空Slot就退出循环
//                                break;
//                            }

//                        }
//                        else
//                        {
//                            #region [ 目前Slot不是Empty Slot 的處理 ]

//                            if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
//                            {

//                                #region [ CSType不為循序,可以選擇下一個Slot判斷是否為空Slot ]


//                                #endregion

//                            }
//                            else
//                            {

//                                #region [ CSTType為循序,不可以選擇下一個Slot ]

//                                if (iCanReceiveJobCount == 0)
//                                {
//                                    //如果还没有找到空的Slot的就继续找下去，直到循环结束
//                                    continue;
//                                }
//                                else
//                                {
//                                    tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

//                                    UpdateUnloadStageStatus(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

//                                    return;
//                                }


//                                #endregion

//                            }

//                            #endregion

//                        }

//                        #endregion
//                    }

//                    #region [ 循环结束后再根据空Slot的数量设置Stage的状态 ]

//                    if (iCanReceiveJobCount == 0)
//                    {
//                        tmpStageStatus = eRobotStageStatus.NO_REQUEST;

//                        //非Empty Slot 則視為NOREQ 並將Priority設為Others
//                        UpdateUnloadStageStatus(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);
//                        return;

//                    }
//                    else
//                    {
//                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

//                        UpdateUnloadStageStatus(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

//                        return;

//                    }

//                    #endregion

//                    #endregion

//                }
//                else
//                {

//                    #region [ 放片順序為ASC SlotNo由小到大 ]

//                    for (int i = 1; i <= curStage.Data.SLOTMAXCOUNT; i++)
//                    {

//                        #region [ Check Unlaod Port Status LDRQ ]

//                        if (Check_CSTSlot_IsEmptyForGetGetPutPut(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i) == true)
//                        {

//                            //Update Current Stage LDRQ Empty Slot
//                            iCanReceiveJobCount++;
//                            lock (curStage)
//                            {
//                                if (iCanReceiveJobCount == 1)
//                                {
//                                    curStage.CurLDRQ_EmptySlotNo = i.ToString().PadLeft(3, '0');

//                                    //add Empty SlotNo To EmptySlotNoList
//                                    if (curStage.curLDRQ_EmptySlotList.ContainsKey(i) == false)
//                                    {
//                                        curStage.curLDRQ_EmptySlotList.Add(i, string.Empty);
//                                    }

//                                }
//                                else
//                                {
//                                    curStage.CurLDRQ_EmptySlotNo02 = i.ToString().PadLeft(3, '0');

//                                    //add Empty SlotNo To EmptySlotNoList
//                                    if (curStage.curLDRQ_EmptySlotList.ContainsKey(i) == false)
//                                    {
//                                        curStage.curLDRQ_EmptySlotList.Add(i, string.Empty);
//                                    }

//                                }
//                            }
//                            if (iCanReceiveJobCount == 2)
//                            {
//                                //如果已经找到2个空Slot就退出循环
//                                break;
//                            }

//                        }
//                        else
//                        {
//                            #region [ 目前Slot不是Empty Slot 的處理 ]

//                            if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
//                            {

//                                #region [ CSType不為循序,可以選擇下一個Slot判斷是否為空Slot ]


//                                #endregion

//                            }
//                            else
//                            {

//                                #region [ CSTType為循序,不可以選擇下一個Slot ]

//                                if (iCanReceiveJobCount == 0)
//                                {
//                                    //如果还没有找到空的Slot的就继续找下去，直到循环结束
//                                    continue;
//                                }
//                                else
//                                {
//                                    tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

//                                    UpdateUnloadStageStatus(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

//                                    return;
//                                }

//                                #endregion

//                            }

//                            #endregion

//                        }

//                        #endregion

//                    }

//                    #region [ 循环结束后再根据空Slot的数量设置Stage的状态 ]

//                    if (iCanReceiveJobCount == 0)
//                    {
//                        tmpStageStatus = eRobotStageStatus.NO_REQUEST;

//                        //非Empty Slot 則視為NOREQ 並將Priority設為Others
//                        UpdateUnloadStageStatus(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);
//                        return;

//                    }
//                    else
//                    {
//                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

//                        UpdateUnloadStageStatus(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

//                        return;

//                    }

//                    #endregion

//                    #endregion

//                }

//            }
//            catch (Exception ex)
//            {

//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//            }

//        }

//        private bool Check_CSTSlot_IsEmptyForGetGetPutPut(RobotStage curStage, Trx trxExistInfo, Trx trxJobInfo, int slotKey)
//        {

//            string strlog = string.Empty;
//            string allSlotExistInfo = string.Empty;
//            int job_ExistInfo = ePortSlotExistInfo.JOB_NO_EXIST;
//            string jobTrx_CstSeqkey = string.Empty;
//            string jobTrx_JobSeqkey = string.Empty;
//            string jobTrx_GroupName = string.Empty;
//            string jobTrx_EventName = string.Empty;
//            int job_CstSeq = 0;
//            int job_JobSeq = 0;

//            try
//            {

//                #region [ Check Slot Job Exist Status ]

//                #region [ Port#XXJobEachCassetteSlotExistsBlock Structure ]

//                //會根據不同的Line有不同的長度
//                //<event name="L2_W_Port#01JobEachCassetteSlotExistsBlock" devicecode="W" address="0x00015CC" points="4">
//                //  <itemgroup name="Port#01JobEachCassetteSlotExistsBlock" />
//                //</event>

//                //<itemgroup name="Port#01JobEachCassetteSlotExistsBlock">
//                //  <item name="JobExistence" woffset="0" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
//                //</itemgroup>

//                #endregion

//                allSlotExistInfo = trxExistInfo.EventGroups[0].Events[0].Items[0].Value;

//                #region [ 判斷是否為空 ]

//                if (allSlotExistInfo.Trim() == string.Empty)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) JobExistInfo({6}) Fail! Reason(Job Exist Info is Empty)!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxExistInfo.Metadata.Name, allSlotExistInfo);

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    return false;

//                }

//                #endregion

//                #region [ Check Slot Lenth is Exist ]

//                if (slotKey > allSlotExistInfo.Trim().Length)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) JobExistInfo({6}) to Check SlotNo({7}) Exist Fail! Reason(Job Exist Info can not find this SlotNo)!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxExistInfo.Metadata.Name, allSlotExistInfo, slotKey);

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    return false;
//                }

//                #endregion

//                #endregion

//                //SlotKey從1開始 ,陣列從0開始
//                job_ExistInfo = int.Parse(allSlotExistInfo.Substring(slotKey - 1, 1));

//                #region [ Port#XXJobEachCassetteSlotPositionBlock Trx Structure ]

//                //<trx name="L2_Port#01JobEachCassetteSlotPositionBlock" triggercondition="none">
//                //    <eventgroup name="L2_EG_Port#01JobEachCassetteSlotPositionBlock" dir="E2B">
//                //        <event name="L2_W_Port#01JobEachCassetteSlotPositionBlock" trigger="true" />
//                //    </eventgroup>
//                //</trx>

//                //<event name="L2_W_Port#01JobEachCassetteSlotPositionBlock" devicecode="W" address="0x0001613" points="58">
//                //  <itemgroup name="Port#01JobEachCassetteSlotPositionBlock" />
//                //</event>

//                //<itemgroup name="Port#01JobEachCassetteSlotPositionBlock">
//                //  <item name="SlotPosition#001CassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
//                //  <item name="SlotPosition#001JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
//                //  <item name="SlotPosition#002CassetteSequenceNo" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
//                //  <item name="SlotPosition#002JobSequenceNo" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
//                //  <item name="SlotPosition#003CassetteSequenceNo" woffset="4" boffset="0" wpoints="1" bpoints="16" expression="INT" />
//                //  <item name="SlotPosition#003JobSequenceNo" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="INT" />

//                jobTrx_GroupName = string.Format("{0}_EG_Port#{1}JobEachCassetteSlotPositionBlock", curStage.Data.NODENO, curStage.Data.STAGEID.PadLeft(2, '0'));
//                jobTrx_EventName = string.Format("{0}_W_Port#{1}JobEachCassetteSlotPositionBlock", curStage.Data.NODENO, curStage.Data.STAGEID.PadLeft(2, '0'));
//                jobTrx_CstSeqkey = string.Format("SlotPosition#{0}CassetteSequenceNo", slotKey.ToString().PadLeft(3, '0'));
//                jobTrx_JobSeqkey = string.Format("SlotPosition#{0}JobSequenceNo", slotKey.ToString().PadLeft(3, '0'));

//                #endregion

//                job_CstSeq = int.Parse(trxJobInfo.EventGroups[jobTrx_GroupName].Events[jobTrx_EventName].Items[jobTrx_CstSeqkey].Value);
//                job_JobSeq = int.Parse(trxJobInfo.EventGroups[jobTrx_GroupName].Events[jobTrx_EventName].Items[jobTrx_JobSeqkey].Value);

//                #region [ Check Monitor SlotCSTSeq, SlotJOBSeq , SlotGlassExist ]

//                if (job_CstSeq == 0 && job_JobSeq == 0 && job_ExistInfo == ePortSlotExistInfo.JOB_NO_EXIST)
//                {

//                    #region [ 無帳無料 ]

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CassetteSequenceNo({7}) JobSequenceNo({8}) GlassExist({9}) is Empty!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
//                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

//                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    return true;

//                    #endregion

//                }
//                else if (job_CstSeq > 0 && job_JobSeq > 0 && job_ExistInfo == ePortSlotExistInfo.JOB_EXIST)
//                {

//                    #region [ 有帳有料 ]

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CassetteSequenceNo({7}) JobSequenceNo({8}) GlassExist({9}) is not Empty!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
//                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    return false;

//                    #endregion

//                }
//                else
//                {

//                    #region [ 帳料異常 ]

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CassetteSequenceNo({7}) JobSequenceNo({8}) GlassExist({9}) is not Empty!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
//                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    return false;

//                    #endregion

//                }

//                #endregion

//            }
//            catch (Exception ex)
//            {

//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//                return false;
//            }

//        }
//        #endregion

//        #region 获取Both Port的可控制玻璃

//        /// <summary> 確認Both Port Slot是否有JOB存在並加入到Can Control List and Update UDRQ Status
//        /// 確認Both Port Slot是否有JOB存在並加入到Can Control List and Update UDRQ Status
//        /// </summary>
//        /// <param name="curRobot"></param>
//        /// <param name="curStage"></param>
//        /// <param name="curCanCtlJobList"></param>
//        /// <param name="curPort"></param>
//        private void Get_BothPort_CanControlJoblistForGetGetPutPut(Robot curRobot, RobotStage curStage, List<Job> curCanCtlJobList, Port curPort)
//        {

//            string tmpStageStatus = string.Empty;
//            string trxID = string.Empty;
//            string strlog = string.Empty;
//            string checkReasonCode = string.Empty;
//            Job TempJob = new Job();//记录临时的CanControlJob资料
//            int iCanControlJobCount = 0;//记录已经找到几个可控制的玻璃
//            Dictionary<int, Job> DicTempJobList = new Dictionary<int, Job>();//记录找到的可控制玻璃资讯
//            try
//            {

//                //預設為NoReq
//                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

//                #region  [ Real time Get Port Slot Exist Info Trx ]

//                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
//                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotExistsBlock    
//                trxID = GetTrx_CSTSlotJobExistInfo(curStage.Data.NODENO, curStage.Data.STAGEID);

//                Trx get_CSTSlot_ExistInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

//                if (get_CSTSlot_ExistInfo_Trx == null)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not Get TrxID({4})!",
//                                               curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                               trxID);
//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                    }

//                    #endregion

//                    ClearBothPortStageUDRQStatusForGetGetPutPut(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

//                    return;

//                }

//                #endregion

//                #region  [ Real time Get Port Slot Job Info Trx ]

//                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
//                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotPositionBlock               
//                trxID = GetTrx_CSTSlotJobEachPosition(curStage.Data.NODENO, curStage.Data.STAGEID);

//                Trx get_CSTSlot_JobInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

//                if (get_CSTSlot_JobInfo_Trx == null)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not Get TrxID({4})!",
//                                               curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                               trxID);
//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                    }

//                    #endregion

//                    ClearBothPortStageUDRQStatusForGetGetPutPut(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name);

//                    return;

//                }

//                #endregion

//                //根據Robot設定的取片順序來決定要從Slot01開始抽還是SlotMax開始抽 ASC 从小到大(Priority 01>02>03>…) ,DESC 从大到小 (Priority 01<02<03>…), default : ASC 从小到大          
//                if (curRobot.Data.SLOTFETCHSEQ == "ASC")
//                {

//                    #region [ 抽片順序為ASC SlotNo由小到大 ]

//                    for (int i = 1; i <= curStage.Data.SLOTMAXCOUNT; i++)
//                    {
//                        checkReasonCode = Get_CSTSlot_CanControlJoblistForGetGetPutPut(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList,TempJob, curPort, curRobot);

//                        #region [ 根據Port CST Type來決定是否要判斷下一片 ]

//                        if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
//                        {

//                            #region [ 非循序CST的處理,可跳Slot ]

//                            switch (checkReasonCode)
//                            {
//                                case ePortJobUDRQReason.REASON_OK:
//                                    iCanControlJobCount++;
//                                    DicTempJobList.Add(iCanControlJobCount, TempJob);
//                                    break;

//                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

//                                    //確認是空Slot則判斷下一個Slot
//                                    break;

//                                default:

//                                    //其他錯誤是為找不到 則跳下一個Slot判斷
//                                    break;
//                            }

//                            #endregion

//                        }
//                        else
//                        {

//                            #region [ 循序CST的處理,不可跳Slot ]

//                            switch (checkReasonCode)
//                            {
//                                case ePortJobUDRQReason.REASON_OK:
//                                    iCanControlJobCount++;
//                                    DicTempJobList.Add(iCanControlJobCount, TempJob);
//                                    return;

//                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

//                                    //確認是空Slot則判斷下一個Slot
//                                    break;

//                                default:

//                                    //其他錯誤視為找不到或是帳料有問題需更新Stage Status並紀錄Log,或不满足两片Job的时候
//                                    UpdateBothPortStageUDRQStatusForGetGetPutPut(curStage, MethodBase.GetCurrentMethod().Name, DicTempJobList);
//                                    return;
//                            }

//                            #endregion

//                        }

//                        if (iCanControlJobCount == 2)
//                        {
//                            //如果已经达到2个可控制的Job就退出循环
//                            break;
//                        }
//                        #endregion

//                    }

//                    //找不到可控制的玻璃、只找到一片可控制的玻璃、找到两片可控制的玻璃都要去检查 更新 Stage Status
//                    UpdateBothPortStageUDRQStatusForGetGetPutPut(curStage, MethodBase.GetCurrentMethod().Name, DicTempJobList);

//                    #endregion

//                }
//                else
//                {

//                    #region [ 抽片順序為DESC SlotNo由大到小 ]

//                    for (int i = curStage.Data.SLOTMAXCOUNT; i > 0; i--)
//                    {
//                        checkReasonCode = Get_CSTSlot_CanControlJoblistForGetGetPutPut(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i, curCanCtlJobList,TempJob ,curPort, curRobot);

//                        #region [ 根據Port CST Type來決定是否要判斷下一片 ]

//                        if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
//                        {

//                            #region [ 非循序CST的處理,可跳Slot ]

//                            switch (checkReasonCode)
//                            {
//                                case ePortJobUDRQReason.REASON_OK:

//                                    iCanControlJobCount++;
//                                    DicTempJobList.Add(iCanControlJobCount, TempJob);
//                                    return;

//                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

//                                    //確認是空Slot則判斷下一個Slot
//                                    break;

//                                default:

//                                    //其他錯誤是為找不到 則跳下一個Slot判斷
//                                    break;
//                            }

//                            #endregion

//                        }
//                        else
//                        {

//                            #region [ 循序CST的處理,不可跳Slot ]

//                            switch (checkReasonCode)
//                            {
//                                case ePortJobUDRQReason.REASON_OK:

//                                    iCanControlJobCount++;
//                                    DicTempJobList.Add(iCanControlJobCount, TempJob);
//                                    return;

//                                case ePortJobUDRQReason.IS_EMPTY_SLOT:

//                                    //確認是空Slot則判斷下一個Slot
//                                    break;

//                                default:

//                                    //其他錯誤視為找不到或是帳料有問題需更新Stage Status並紀錄Log,或不满足两片Job的时候
//                                    UpdateBothPortStageUDRQStatusForGetGetPutPut(curStage, MethodBase.GetCurrentMethod().Name, DicTempJobList);
//                                    return;
//                            }

//                            #endregion

//                        }

//                        #endregion
//                    }

//                    //找不到可控制的玻璃、只找到一片可控制的玻璃、找到两片可控制的玻璃都要去检查 更新 Stage Status
//                    UpdateBothPortStageUDRQStatusForGetGetPutPut(curStage, MethodBase.GetCurrentMethod().Name, DicTempJobList);

//                    #endregion

//                }

//            }
//            catch (Exception ex)
//            {
//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        //20150818 Modify Both Port Stage主要透過JudgePortStage_UDRQ_LDRQStatus_ForGetGetPutPut來做更新,所以只更新Stage UDRQ Status 不可更新CurStatgeStatus
//        protected void UpdateBothPortStageUDRQStatusForGetGetPutPut(RobotStage curStage,  string funcName, Dictionary<int, Job> dicCanControlJob)
//        {
//            string strlog = string.Empty;
//            string newUDRQStageStatus = eRobotStageStatus.NO_REQUEST;
//            try
//            {
//                string sendOutCstSeq01 = "";
//                string sendOutJobSeq01 = "";
//                string sendOutCstSeq02 = "";
//                string sendOutJobSeq02 = "";

//                switch (dicCanControlJob.Count)
//                {
//                    case 0:
//                        newUDRQStageStatus = eRobotStageStatus.NO_REQUEST;
//                        ClearBothPortStageUDRQStatusForGetGetPutPut(curStage, newUDRQStageStatus, funcName);
//                        return;
//                    case 1:
//                        newUDRQStageStatus = eRobotStageStatus.SEND_OUT_READY;
//                        sendOutCstSeq01 = dicCanControlJob[1].CassetteSequenceNo;
//                        sendOutJobSeq01 = dicCanControlJob[1].JobSequenceNo;
//                        break;
//                    case 2:
//                        newUDRQStageStatus = eRobotStageStatus.SEND_OUT_READY;
//                        sendOutCstSeq01 = dicCanControlJob[1].CassetteSequenceNo;
//                        sendOutJobSeq01 = dicCanControlJob[1].JobSequenceNo;
//                        sendOutCstSeq02 = dicCanControlJob[2].CassetteSequenceNo;
//                        sendOutJobSeq02 = dicCanControlJob[2].JobSequenceNo;
//                        break;
//                    default:
//                        break;
//                }
//                #region [ Stage Status Change才需要Update ]

//                if (curStage.File.Stage_UDRQ_Status != newUDRQStageStatus)
//                {
//                    //20150818 Modify Both Port Stage主要透過JudgePortStage_UDRQ_LDRQStatus_ForGetGetPutPut來做更新,所以只更新Stage UDRQ Status 不可更新CurStatgeStatus
//                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageID({3}) StageName({4}) Both Port UDRQ status change from ({5}) to ({6}) , UDRQ Job01 CassetteSequenceNo({7}) JobSequenceNo({8}),Job02 CassetteSequenceNo({9}) JobSequenceNo({10})!",
//                                                    curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                                    curStage.Data.STAGENAME, curStage.File.Stage_UDRQ_Status, newUDRQStageStatus, sendOutCstSeq01,
//                                                    sendOutJobSeq01,sendOutCstSeq02,sendOutJobSeq02);

//                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    //add for Log Quick Trace
//                    strlog = string.Format("[{0}] {1} - [{2}]{3}({4}) Both Port Stage UDRQ Status Change From({5}) to ({6}) ,sendOut Job01({7},{8}),Job02 ({9},{10})",
//                                            "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
//                                            MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
//                                            funcName,
//                                            curStage.Data.STAGENAME,
//                                            curStage.Data.STAGEID, curStage.File.Stage_UDRQ_Status, newUDRQStageStatus, sendOutCstSeq01,
//                                                    sendOutJobSeq01, sendOutCstSeq02, sendOutJobSeq02);

//                    Logger.LogTrxWrite(this.LogName, strlog);

//                    #region [ Update Robot Stage Entity ]

//                    lock (curStage.File)
//                    {
//                        curStage.File.Stage_UDRQ_Status = newUDRQStageStatus;
//                        curStage.File.CurSendOut_CSTSeq = sendOutCstSeq01;
//                        curStage.File.CurSendOut_JobSeq = sendOutJobSeq01;
//                        curStage.File.CurSendOut_CSTSeq02 = sendOutCstSeq02;
//                        curStage.File.CurSendOut_JobSeq02 = sendOutJobSeq02;
//                        curStage.File.StatusChangeFlag = true;
//                    }

//                    #endregion

//                }

//                #endregion

//                #region  [DebugLog]

//                if (IsShowDetialLog == true)
//                {

//                    //Get Current Stage Info To Log
//                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageID({3}) StageName({4}) UDRQ status is ({5}) , UDRQ Job01 CassetteSequenceNo({6}) JobSequenceNo({7}),Job02 CassetteSequenceNo({8}) JobSequenceNo({9})!",
//                                                                curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                                                curStage.Data.STAGENAME, newUDRQStageStatus, sendOutCstSeq01, sendOutJobSeq01, sendOutCstSeq02, sendOutJobSeq02);

//                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                }

//                #endregion

//            }
//            catch (Exception ex)
//            {
//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        //20150818 Modify Both Port Stage主要透過JudgePortStage_UDRQ_LDRQStatus_ForGetGetPutPut來做更新,所以只更新Stage UDRQ Status 不可更新CurStatgeStatus
//        /// <summary>
//        /// Both Port Stage主要透過JudgePortStage_UDRQ_LDRQStatus_ForGetGetPutPut來做更新,所以只更新Stage UDRQ Status 不可更新CurStatgeStatus
//        /// </summary>
//        /// <param name="curStage"></param>
//        /// <param name="newStageStatus"></param>
//        /// <param name="funcName"></param>
//        private void ClearBothPortStageUDRQStatusForGetGetPutPut(RobotStage curStage, string newStageStatus, string funcName)
//        {
//            string strlog = string.Empty;

//            try
//            {

//                #region [ Stage Status Change才需要Update ]

//                //20150818 Modify Both Port Stage主要透過JudgePortStage_UDRQ_LDRQStatus_ForGetGetPutPut來做更新,所以只更新Stage UDRQ Status 不可更新CurStatgeStatus
//                if (curStage.File.Stage_UDRQ_Status != newStageStatus)
//                {

//                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageID({3}) StageName({4}) Both Port UDRQ status change from ({5}) to ({6}) , UDRQ Job01 CassetteSequenceNo({7}) JobSequenceNo({8}),UDRQ Job02 CassetteSequenceNo({9}) JobSequenceNo({10})!",
//                                                    curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                                    curStage.Data.STAGENAME, curStage.File.Stage_UDRQ_Status, newStageStatus, string.Empty,
//                                                    string.Empty, string.Empty, string.Empty);

//                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    //add for Log Quick Trace
//                    strlog = string.Format("[{0}] {1} - [{2}]{3}({4}) Stage Both Port UDRQ Status Change From({5}) to ({6}) ,sendOut Job01({7},{8}),sendOut Job02({9},{10})",
//                                            "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
//                                            MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
//                                            funcName,
//                                            curStage.Data.STAGENAME,
//                                            curStage.Data.STAGEID, curStage.File.Stage_UDRQ_Status, newStageStatus, string.Empty,
//                                            string.Empty,
//                                            string.Empty, string.Empty);

//                    Logger.LogTrxWrite(this.LogName, strlog);

//                    #region [ Update Robot Stage Entity ]

//                    lock (curStage.File)
//                    {
//                        //20150818 Modify Both Port Stage主要透過JudgePortStage_UDRQ_LDRQStatus_ForGetGetPutPut來做更新,所以只更新Stage UDRQ Status 不可更新CurStatgeStatus
//                        //curStage.File.CurStageStatus = newStageStatus;
//                        curStage.File.Stage_UDRQ_Status = newStageStatus;
//                        curStage.File.CurSendOut_CSTSeq = string.Empty;
//                        curStage.File.CurSendOut_JobSeq = string.Empty;
//                        curStage.File.CurSendOut_CSTSeq02 = string.Empty;
//                        curStage.File.CurSendOut_JobSeq02 = string.Empty;
//                        curStage.File.StatusChangeFlag = true;
//                    }

//                    #endregion

//                }

//                #endregion

//                #region  [DebugLog]

//                if (IsShowDetialLog == true)
//                {

//                    //Get Current Stage Info To Log
//                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageID({3}) StageName({4}) UDRQ status change from ({5}) to ({6}) , UDRQ Job01 CassetteSequenceNo({7}) JobSequenceNo({8}),UDRQ Job02 CassetteSequenceNo({9}) JobSequenceNo({10})!",
//                                                    curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                                    curStage.Data.STAGENAME, curStage.File.Stage_UDRQ_Status, newStageStatus, string.Empty,
//                                                    string.Empty, string.Empty, string.Empty);

//                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                }

//                #endregion

//            }
//            catch (Exception ex)
//            {
//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        #endregion

//        #region Both Port UnLoadStageStatus
//                /// <summary>
//        /// 確認Both Port Slot是否有空Slot存在並更新Stage LDRQ Status
//        /// </summary>
//        /// <param name="curRobot"></param>
//        /// <param name="curStage"></param>
//        /// <param name="curPort"></param>
//        private void Get_BothPort_StageLDRQStatusInfoForGetGetPutPut(Robot curRobot, RobotStage curStage, Port curPort)
//        {
//            string jobKey = string.Empty;
//            string trxID = string.Empty;
//            string strlog = string.Empty;
//            string tmpStageStatus = string.Empty;
//            string tmpCstStatusPriority = string.Empty;
//            //bool findEmptySlotFlag = false;
//            int iCanReceiveJobCount = 0;//记录可收片的数量，至多找两片空的Slot就返回
//            try
//            {

//                //預設為NoReq
//                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

//                #region [ Set Unload and Both Port Receive Job Priority ]

//                if (curPort.File.Type == ePortType.BothPort)
//                {
//                    if (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)
//                    {
//                        tmpCstStatusPriority = eUnloadPortReceiveStatus.BOTH_PORT_WAIT_PROCESS;
//                    }
//                    else
//                    {
//                        tmpCstStatusPriority = eUnloadPortReceiveStatus.BOTH_PORT_IN_PROCESS;
//                    }
//                }
//                else
//                {
//                    if (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)
//                    {
//                        tmpCstStatusPriority = eUnloadPortReceiveStatus.ULD_PORT_WAIT_PROCESS;
//                    }
//                    else
//                    {
//                        tmpCstStatusPriority = eUnloadPortReceiveStatus.ULD_PORT_IN_PROCESS;
//                    }
//                }

//                #endregion

//                #region  [ Real time Get Port Slot Exist Info Trx ]

//                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
//                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotExistsBlock    
//                trxID = GetTrx_CSTSlotJobExistInfo(curStage.Data.NODENO, curStage.Data.STAGEID);

//                Trx get_CSTSlot_ExistInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

//                if (get_CSTSlot_ExistInfo_Trx == null)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not Get TrxID({4})!",
//                                               curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                               trxID);
//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                    }

//                    #endregion

//                    UpdateStage_LDRQ_Status(curStage, tmpStageStatus,tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

//                    return;

//                }

//                #endregion

//                #region  [ Real time Get Port Slot Job Info Trx ]

//                //T3 Port Slot Info 分為//Port#01JobEachCassetteSlotPositionBlock(JobInfo)與Port#01JobEachCassetteSlotExistsBlock(ExistInfo).
//                //StageNo 01~10 為Port#01~10  EX:L2_Port#01JobEachCassetteSlotPositionBlock               
//                trxID = GetTrx_CSTSlotJobEachPosition(curStage.Data.NODENO, curStage.Data.STAGEID);

//                Trx get_CSTSlot_JobInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

//                if (get_CSTSlot_JobInfo_Trx == null)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {
//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not Get TrxID({4})!",
//                                               curStage.Data.NODENO, curStage.Data.STAGENAME, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
//                                               trxID);
//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                    }

//                    #endregion

//                    UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

//                    return;

//                }

//                #endregion

//                //先将空Slot的讯息清空
//                curStage.CurLDRQ_EmptySlotNo = "";
//                curStage.CurLDRQ_EmptySlotNo02 = "";

//                //根據Robot設定的放片順序來決定要從Slot01開始放在還是SlotMax開始放 ASC 从小到大(Priority 01>02>03>…) ,DEC 从大到小 (Priority 01<02<03>…), default : ASC 从小到大          
//                if (curRobot.Data.SLOTSTORESEQ == "DESC")
//                {

//                    #region [ 放片順序為DEC SlotNo由大到小 ]

//                    for (int i = curStage.Data.SLOTMAXCOUNT; i > 0; i--)
//                    {

//                        #region [ Check Unlaod Port Status LDRQ ]

//                        if (Check_CSTSlot_IsEmptyForGetGetPutPut(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i) == true)
//                        {

//                            iCanReceiveJobCount++;
//                            lock (curStage)
//                            {
//                                if (iCanReceiveJobCount == 1)
//                                {
//                                    curStage.CurLDRQ_EmptySlotNo = i.ToString().PadLeft(3, '0');

//                                    //add Empty SlotNo To EmptySlotNoList
//                                    if (curStage.curLDRQ_EmptySlotList.ContainsKey(i) == false)
//                                    {
//                                        curStage.curLDRQ_EmptySlotList.Add(i ,string.Empty);
//                                    }

//                                }
//                                else
//                                {
//                                    curStage.CurLDRQ_EmptySlotNo02 = i.ToString().PadLeft(3, '0');

//                                    //add Empty SlotNo To EmptySlotNoList
//                                    if (curStage.curLDRQ_EmptySlotList.ContainsKey(i) == false)
//                                    {
//                                        curStage.curLDRQ_EmptySlotList.Add(i, string.Empty);
//                                    }
//                                }
//                            }
//                            if (iCanReceiveJobCount == 2)
//                            {
//                                //如果已经找到2个空Slot就退出循环
//                                break;
//                            }

//                        }
//                        else
//                        {
//                            #region [ 目前Slot不是Empty Slot 的處理 ]

//                            if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
//                            {

//                                #region [ CSType不為循序,可以選擇下一個Slot判斷是否為空Slot ]


//                                #endregion

//                            }
//                            else
//                            {

//                                #region [ CSTType為循序,不可以選擇下一個Slot ]

//                                //一旦發現不是空Slot 則看Flag是否已經準備收片
//                                if (iCanReceiveJobCount == 0)
//                                {
//                                    //如果还没有找到空的Slot的就继续找下去，直到循环结束
//                                    continue;
//                                }
//                                else
//                                {
//                                    tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

//                                    UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

//                                    return;
//                                }

//                                #endregion

//                            }

//                            #endregion

//                        }

//                        #endregion
//                    }

//                    #region [ 處理最後一各Slot的判斷結果 ]

//                    //當最後一片才找到Empty CST時要更新Stage Stage
//                    if (iCanReceiveJobCount == 0)
//                    {
//                        tmpStageStatus = eRobotStageStatus.NO_REQUEST;

//                        //非Empty Slot 則視為NOREQ 並將Priority設為Others
//                        UpdateStage_LDRQ_Status(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);
//                        return;

//                    }
//                    else
//                    {
//                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

//                        UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

//                        return;

//                    }

//                    #endregion

//                    #endregion

//                }
//                else
//                {

//                    #region [ 放片順序為ASC SlotNo由小到大 ]

//                    for (int i = 1; i <= curStage.Data.SLOTMAXCOUNT; i++)
//                    {

//                        #region [ Check Unlaod Port Status LDRQ ]

//                        if (Check_CSTSlot_IsEmpty(curStage, get_CSTSlot_ExistInfo_Trx, get_CSTSlot_JobInfo_Trx, i) == true)
//                        {

//                            iCanReceiveJobCount++;
//                            lock (curStage)
//                            {
//                                if (iCanReceiveJobCount == 1)
//                                {
//                                    curStage.CurLDRQ_EmptySlotNo = i.ToString().PadLeft(3, '0');

//                                    //add Empty SlotNo To EmptySlotNoList
//                                    if (curStage.curLDRQ_EmptySlotList.ContainsKey(i) == false)
//                                    {
//                                        curStage.curLDRQ_EmptySlotList.Add(i, string.Empty);
//                                    }
//                                }
//                                else
//                                {
//                                    curStage.CurLDRQ_EmptySlotNo02 = i.ToString().PadLeft(3, '0');

//                                    //add Empty SlotNo To EmptySlotNoList
//                                    if (curStage.curLDRQ_EmptySlotList.ContainsKey(i) == false)
//                                    {
//                                        curStage.curLDRQ_EmptySlotList.Add(i, string.Empty);
//                                    }

//                                }
//                            }
//                            if (iCanReceiveJobCount == 2)
//                            {
//                                //如果已经找到2个空Slot就退出循环
//                                break;
//                            }

//                        }
//                        else
//                        {
//                            #region [ 目前Slot不是Empty Slot 的處理 ]

//                            if (curStage.Data.CASSETTETYPE == eRobotStageCSTType.RANDOM_CST)
//                            {

//                                #region [ CSType不為循序,可以選擇下一個Slot判斷是否為空Slot ]


//                                #endregion

//                            }
//                            else
//                            {

//                                #region [ CSTType為循序,不可以選擇下一個Slot ]

//                                //一旦發現不是空Slot 則看Flag是否已經準備收片
//                                #region [ 處理最後一各Slot的判斷結果 ]

//                                //當最後一片才找到Empty CST時要更新Stage Stage
//                                if (iCanReceiveJobCount == 0)
//                                {
//                                    continue;

//                                }
//                                else
//                                {
//                                    tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

//                                    UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

//                                    return;

//                                }

//                                #endregion

//                                #endregion

//                            }

//                            #endregion

//                        }

//                        #endregion

//                    }

//                    #region [ 處理最後一各Slot的判斷結果 ]

//                    //當最後一片才找到Empty CST時要更新Stage Stage
//                    if (iCanReceiveJobCount == 0)
//                    {
//                        tmpStageStatus = eRobotStageStatus.NO_REQUEST;

//                        //非Empty Slot 則視為NOREQ 並將Priority設為Others
//                        UpdateStage_LDRQ_Status(curStage, tmpStageStatus, eUnloadPortReceiveStatus.OTHERS, MethodBase.GetCurrentMethod().Name);
//                        return;

//                    }
//                    else
//                    {
//                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

//                        UpdateStage_LDRQ_Status(curStage, tmpStageStatus, tmpCstStatusPriority, MethodBase.GetCurrentMethod().Name);

//                        return;

//                    }

//                    #endregion

//                    #endregion

//                }

//            }
//            catch (Exception ex)
//            {

//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//            }

//        }
//        #endregion

//        #region 获取Cassette 可控制玻璃

//        /// <summary> 取得Loader Port中存在且有帳的Job and Add to Can Control Job
//        /// 取得Loader Port中存在且有帳的Job and Add to Can Control Job
//        /// </summary>
//        /// <param name="curStage"></param>
//        /// <param name="trxExistInfo">CST內每個Slot Position的Job Info</param>
//        /// <param name="trxJobInfo">CST內每個Slot內的Exist Info</param>
//        /// <param name="slotKey"></param>
//        /// <param name="curRobotJobList"></param>
//        /// <param name="curPort"></param>
//        /// <param name="curRobot"></param>
//        /// <returns></returns>
//        private string Get_CSTSlot_CanControlJoblistForGetGetPutPut(RobotStage curStage, Trx trxExistInfo, Trx trxJobInfo, int slotKey,
//            List<Job> curCanControlJobList, Job curReturnJob, Port curPort, Robot curRobot)
//        {
//            string jobKey = string.Empty;
//            string strlog = string.Empty;
//            string failMsg = string.Empty;
//            string allSlotExistInfo = string.Empty;
//            int job_ExistInfo = ePortSlotExistInfo.JOB_NO_EXIST;
//            string jobTrx_CstSeqkey = string.Empty;
//            string jobTrx_JobSeqkey = string.Empty;
//            string jobTrx_GroupName = string.Empty;
//            string jobTrx_EventName = string.Empty;
//            int job_CstSeq = 0;
//            int job_JobSeq = 0;
//            string fail_ReasonCode = string.Empty;
//            string tmpPortCstStatusPriority = string.Empty;
//            curReturnJob = new Job();
//            try
//            {

//                #region [ Check Slot Job Exist Status ]

//                #region [ Port#XXJobEachCassetteSlotExistsBlock Structure ]

//                //會根據不同的Line有不同的長度
//                //<event name="L2_W_Port#01JobEachCassetteSlotExistsBlock" devicecode="W" address="0x00015CC" points="4">
//                //  <itemgroup name="Port#01JobEachCassetteSlotExistsBlock" />
//                //</event>

//                //<itemgroup name="Port#01JobEachCassetteSlotExistsBlock">
//                //  <item name="JobExistence" woffset="0" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
//                //</itemgroup>

//                #endregion

//                allSlotExistInfo = trxExistInfo.EventGroups[0].Events[0].Items[0].Value;

//                #region [ 判斷是否為空 ]

//                if (allSlotExistInfo.Trim() == string.Empty)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) JobExistInfo({6}) Fail! Reason(Job Exist Info is Empty)!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxExistInfo.Metadata.Name, allSlotExistInfo);

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    return ePortJobUDRQReason.OTHERS;

//                }

//                #endregion

//                #region [ Check Slot Lenth is Exist ]

//                if (slotKey > allSlotExistInfo.Trim().Length)
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) JobExistInfo({6}) to Check SlotNo({7}) Exist Fail! Reason(Job Exist Info can not find this SlotNo)!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxExistInfo.Metadata.Name, allSlotExistInfo, slotKey);

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    return ePortJobUDRQReason.OTHERS;
//                }

//                #endregion

//                #endregion

//                //SlotKey從1開始 ,陣列從0開始
//                job_ExistInfo = int.Parse(allSlotExistInfo.Substring(slotKey - 1, 1));

//                #region [ Port#XXJobEachCassetteSlotPositionBlock Trx Structure ]

//                //<trx name="L2_Port#01JobEachCassetteSlotPositionBlock" triggercondition="none">
//                //    <eventgroup name="L2_EG_Port#01JobEachCassetteSlotPositionBlock" dir="E2B">
//                //        <event name="L2_W_Port#01JobEachCassetteSlotPositionBlock" trigger="true" />
//                //    </eventgroup>
//                //</trx>

//                //<event name="L2_W_Port#01JobEachCassetteSlotPositionBlock" devicecode="W" address="0x0001613" points="58">
//                //  <itemgroup name="Port#01JobEachCassetteSlotPositionBlock" />
//                //</event>

//                //<itemgroup name="Port#01JobEachCassetteSlotPositionBlock">
//                //  <item name="SlotPosition#001CassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
//                //  <item name="SlotPosition#001JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
//                //  <item name="SlotPosition#002CassetteSequenceNo" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
//                //  <item name="SlotPosition#002JobSequenceNo" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
//                //  <item name="SlotPosition#003CassetteSequenceNo" woffset="4" boffset="0" wpoints="1" bpoints="16" expression="INT" />
//                //  <item name="SlotPosition#003JobSequenceNo" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="INT" />

//                jobTrx_GroupName = string.Format("{0}_EG_Port#{1}JobEachCassetteSlotPositionBlock", curStage.Data.NODENO, curStage.Data.STAGEID.PadLeft(2, '0'));
//                jobTrx_EventName = string.Format("{0}_W_Port#{1}JobEachCassetteSlotPositionBlock", curStage.Data.NODENO, curStage.Data.STAGEID.PadLeft(2, '0'));
//                jobTrx_CstSeqkey = string.Format("SlotPosition#{0}CassetteSequenceNo", slotKey.ToString().PadLeft(3, '0'));
//                jobTrx_JobSeqkey = string.Format("SlotPosition#{0}JobSequenceNo", slotKey.ToString().PadLeft(3, '0'));

//                #endregion

//                job_CstSeq = int.Parse(trxJobInfo.EventGroups[jobTrx_GroupName].Events[jobTrx_EventName].Items[jobTrx_CstSeqkey].Value);
//                job_JobSeq = int.Parse(trxJobInfo.EventGroups[jobTrx_GroupName].Events[jobTrx_EventName].Items[jobTrx_JobSeqkey].Value);

//                #region [ Check Monitor SlotCSTSeq, SlotJOBSeq , SlotGlassExist ]

//                if (job_CstSeq > 0 && job_JobSeq > 0 && job_ExistInfo == ePortSlotExistInfo.JOB_EXIST)
//                {

//                    #region [ 有帳有料 ]

//                    jobKey = string.Format("{0}_{1}", job_CstSeq.ToString(), job_JobSeq.ToString());

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CassetteSequenceNo({7}) JobSequenceNo({8}) GlassExist({9}) JobKey=({10})!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
//                                                job_JobSeq.ToString(), job_ExistInfo.ToString(), jobKey);

//                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    #region [ Get Job Info by Slot CstSeq ,JobSeq ]

//                    Job curBcsJob = ObjectManager.JobManager.GetJob(job_CstSeq.ToString(), job_JobSeq.ToString());

//                    if (curBcsJob == null)
//                    {

//                        #region  [DebugLog]

//                        if (IsShowDetialLog == true)
//                        {

//                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) Can not Get Job by CassetteSequenceNo({4}) JobSequenceNo({5})!",
//                                                   curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                   job_CstSeq.ToString(), job_JobSeq.ToString());

//                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                        }

//                        #endregion

//                        return ePortJobUDRQReason.JOB_NOT_INWIP; //Has Glass Exist But Not In WIP

//                    }

//                    #endregion

//                    #region [ 判斷Route是否為有效Route 如果找不到Route則不可列入可控制Joblist ]

//                    RobotRouteStep curRouteStepInfo = null;

//                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == true)
//                    {
//                        curRouteStepInfo = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
//                    }

//                    //Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_00001 ]
//                    fail_ReasonCode = eJob_CheckFail_Reason.Get_CstSlotExistJob_Route_Is_Fail;

//                    if (curRouteStepInfo == null)
//                    {

//                        #region  [DebugLog]

//                        if (IsShowDetialLog == true)
//                        {

//                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) can not get RouteInfo!",
//                                                   curStage.Data.NODENO, curStage.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
//                                                   curBcsJob.RobotWIP.CurStepNo.ToString());

//                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                        }

//                        #endregion

//                        #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_00001 ]

//                        if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
//                        {

//                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) can not get RouteInfo!",
//                                                   curStage.Data.NODENO, curStage.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
//                                                   curBcsJob.RobotWIP.CurStepNo.ToString());

//                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                            #region [ 記錄Fail Msg To OPI and Job FailMsg ]

//                            failMsg = string.Format("[{0}]Robot({1}) Job({2},{3}) curStepNo({4}) can not get RouteInfo!",
//                                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
//                                                    curBcsJob.RobotWIP.CurStepNo.ToString());

//                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
//                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

//                            #endregion

//                        }

//                        #endregion

//                        return ePortJobUDRQReason.CANNOT_FIND_ROUTE;
//                    }
//                    else
//                    {
//                        //Clear[ Job_Fail_Case_00001 ]
//                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
//                    }

//                    #endregion

//                    #region [ 判斷Route StepNo 是否 < Max StepNo ]

//                    //Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_00002 ]
//                    fail_ReasonCode = eJob_CheckFail_Reason.Get_CstSlotExistJob_CurStepNo_OutofMaxStepNo;

//                    //20151014 Modify 大於等於65535則算Complete
//                    //if (curBcsJob.RobotWIP.CurStepNo >= curBcsJob.RobotWIP.RobotRouteStepList.Count)
//                    if (curBcsJob.RobotWIP.CurStepNo >= eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO)
//                    {

//                        #region  [DebugLog]

//                        if (IsShowDetialLog == true)
//                        {

//                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) StageCSTType({4}) PortType({5}) But SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) CurStepNo({10}) >= Max StepNo({11}) not In Process!",
//                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                    curStage.Data.CASSETTETYPE, curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo,
//                                                    curBcsJob.JobSequenceNo, job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString());

//                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                        }

//                        #endregion

//                        #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_00002 ]

//                        if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
//                        {

//                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) StageCSTType({4}) PortType({5}) But SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) CurStepNo({10}) >= Max StepNo({11}) not In Process!",
//                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                    curStage.Data.CASSETTETYPE, curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo,
//                                                    curBcsJob.JobSequenceNo, job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString());

//                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                            #region [ 記錄Fail Msg To OPI and Job FailMsg ]

//                            failMsg = string.Format("Robot({0}) StageID({1}) StageName({2}) StageCSTType({3}) PortType({4}) But SlotNo({5}) Job({6},{7}) Exist({8}) CurStepNo({9}) >= Max StepNo({10})",
//                                                    curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, curStage.Data.CASSETTETYPE,
//                                                    curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
//                                                    job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString());

//                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
//                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

//                            #endregion

//                        }

//                        #endregion

//                        return ePortJobUDRQReason.OTHERS;
//                    }
//                    else
//                    {
//                        //Clear[ Job_Fail_Case_00002 ]
//                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
//                    }

//                    #endregion

//                    #region [ by Route Check Fetch Out Condition ]

//                    string checkFailCode = string.Empty;
//                    string checkFailMsg = string.Empty;

//                    //Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_00003 ]
//                    fail_ReasonCode = eJob_CheckFail_Reason.Get_CstSlotExistJob_CheckFetchOut_Condition_Fail;

//                    if (CheckPortFetchOutCondition(curRobot, curStage, curBcsJob, out checkFailCode, out checkFailMsg) == false)
//                    {
//                        #region  [DebugLog]

//                        if (IsShowDetialLog == true)
//                        {

//                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) StageCSTType({4}) PortType({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) CurStepNo({10}) can not FetchOut! FailCode({11}) FailMsg({12})!",
//                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                    curStage.Data.CASSETTETYPE, curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo,
//                                                    curBcsJob.JobSequenceNo, job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), checkFailCode, checkFailMsg);

//                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                        }

//                        #endregion

//                        #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_00003 ]

//                        if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
//                        {

//                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) StageCSTType({4}) PortType({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) CurStepNo({10}) can not FetchOut! FailCode({11}) FailMsg({12})!",
//                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                    curStage.Data.CASSETTETYPE, curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo,
//                                                    curBcsJob.JobSequenceNo, job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), checkFailCode, checkFailMsg);

//                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                            #region [ 記錄Fail Msg To OPI and Job FailMsg ]

//                            failMsg = string.Format("Robot({0}) StageID({1}) StageName({2}) StageCSTType({3}) PortType({4}) SlotNo({5}) Job({6},{7}) Exist({8}) CurStepNo({9}) can not FetchOut! FailCode({10}) FailMsg({11})!",
//                                                    curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, curStage.Data.CASSETTETYPE,
//                                                    curPort.File.Type.ToString(), slotKey.ToString().PadLeft(3, '0'), curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
//                                                    job_ExistInfo.ToString(), curBcsJob.RobotWIP.CurStepNo.ToString(), checkFailCode, checkFailMsg);

//                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
//                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

//                            #endregion

//                        }

//                        #endregion

//                        return ePortJobUDRQReason.OTHERS;

//                    }
//                    else
//                    {
//                        //Clear[ Job_Fail_Case_00003 ]
//                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
//                    }

//                    #endregion

//                    #region [ Update RobotJob WIP ]

//                    //Update Port Job SendOut時的CST Status 以供排序 InProcess > WaitForProcess
//                    if (curPort.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)
//                    {
//                        tmpPortCstStatusPriority = eLoaderPortSendOutStatus.PORT_WAIT_PROCESS;
//                    }
//                    else
//                    {
//                        tmpPortCstStatusPriority = eLoaderPortSendOutStatus.PORT_IN_PROCESS;
//                    }

//                    //Update Robot Job WIP條件 Location變化 , Location Cst Status Priority變化 , SendOutJob Grade變化(Equipment Type時要抓LinkSignal 上報的SendOut JobData內的Grade而不是WIP Grade)
//                    if (curBcsJob.RobotWIP.CurLocation_StageID != curStage.Data.STAGEID ||
//                        curBcsJob.RobotWIP.CurLocation_SlotNo != slotKey ||
//                        curBcsJob.RobotWIP.CurLocation_StageType != eRobotStageType.PORT ||
//                        curBcsJob.RobotWIP.CurPortCstStatusPriority != tmpPortCstStatusPriority ||
//                        curBcsJob.RobotWIP.CurSendOutJobJudge != curBcsJob.JobJudge)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Job CassetteSequenceNo({1}) JobSequenceNo({2}) Update RobotJobWIP curStageNo from ({3}) to ({4}), curSlotNo from ({5}) to ({6}) ,curStageType from ({7}) to ({8}), PortCSTStatusPriority from ({9}) to ({10}) sendOutJobJudge from ({11}) to ({12})!",
//                                                curStage.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurLocation_StageID,
//                                                curStage.Data.STAGEID, curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(), slotKey.ToString(), curBcsJob.RobotWIP.CurLocation_StageType,
//                                                eRobotStageType.PORT, curBcsJob.RobotWIP.CurPortCstStatusPriority, tmpPortCstStatusPriority, curBcsJob.RobotWIP.CurSendOutJobJudge,
//                                                curBcsJob.JobJudge);

//                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                        lock (curBcsJob)
//                        {

//                            curBcsJob.RobotWIP.CurLocation_StageID = curStage.Data.STAGEID;
//                            curBcsJob.RobotWIP.CurLocation_SlotNo = slotKey;
//                            curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.PORT;
//                            curBcsJob.RobotWIP.CurPortCstStatusPriority = tmpPortCstStatusPriority;
//                            curBcsJob.RobotWIP.CurSendOutJobJudge = curBcsJob.JobJudge;

//                        }

//                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

//                    }

//                    #endregion

//                    #region [ Add To Can Control Joblist and Update Stage Status ]

//                    if (AddToCanControlJoblistCondition(curRobot, curStage, curBcsJob, curCanControlJobList, jobKey, MethodBase.GetCurrentMethod().Name) == true)
//                    {
//                        curReturnJob = curBcsJob;//将目前新增的Job资料记录下来返回回去处理

//                        #region [DebugLog]

//                        if (IsShowDetialLog == true)
//                        {
//                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) add CassetteSequenceNo({4}) JobSequenceNo({5}) CurRouteStepNo({6}) CurSlotNo({7}) to Can control List!",
//                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.CurLocation_SlotNo.ToString());

//                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
//                        }

//                        #endregion

//                        return ePortJobUDRQReason.REASON_OK;

//                    }
//                    else
//                    {
//                        return ePortJobUDRQReason.OTHERS;
//                    }

//                    #endregion

//                    #endregion

//                }
//                else if (job_CstSeq > 0 && job_JobSeq > 0 && job_ExistInfo == ePortSlotExistInfo.JOB_NO_EXIST)
//                {

//                    #region [ 有帳無料 Has Job Info but No Exist ]

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(has JobInfo but not exist)!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
//                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    return ePortJobUDRQReason.JOBINFO_EXIST_JOB_NOT_EXIST;

//                    #endregion

//                }
//                else if (job_CstSeq == 0 && job_JobSeq == 0 && job_ExistInfo == ePortSlotExistInfo.JOB_EXIST)
//                {

//                    #region [ 無帳有料 Has Job Exist but No Job Info ]

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(Job Exist but no JobInfo)!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
//                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    return ePortJobUDRQReason.JOBINFO_NOT_EXIST_JOB_EXIST;

//                    #endregion

//                }
//                else if (job_CstSeq == 0 && job_JobSeq == 0 && job_ExistInfo == ePortSlotExistInfo.JOB_NO_EXIST)
//                {

//                    #region [ 無帳無料 No Job Info and Job not Exist ]

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(No JobInfo and Job not Exist)!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
//                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    return ePortJobUDRQReason.IS_EMPTY_SLOT;

//                    #endregion

//                }
//                else
//                {

//                    #region  [DebugLog]

//                    if (IsShowDetialLog == true)
//                    {

//                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) StageCSTType({4}) Get trx by TrxID({5}) SlotNo({6}) CSTSeq({7}) JobSeq({8}) GlassExist({9}) Reason(JobKey or Glass exist status is Illegal!Please Check)!",
//                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                                curStage.Data.CASSETTETYPE, trxJobInfo.Metadata.Name, slotKey.ToString().PadLeft(3, '0'), job_CstSeq.ToString(),
//                                                job_JobSeq.ToString(), job_ExistInfo.ToString());

//                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                    }

//                    #endregion

//                    return ePortJobUDRQReason.OTHERS;

//                }

//                #endregion

//            }
//            catch (Exception ex)
//            {
//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//                return ePortJobUDRQReason.IS_EXCEPTION;
//            }
//        }

//        #endregion

//        /// <summary> 整合Stage 的UDRQ and LDRQ Status 來定義Stage的最終定義 for GetGetPutPut
//        ///
//        /// </summary>
//        /// <param name="curStage"></param>
//        private void JudgePortStage_UDRQ_LDRQStatus_ForGetGetPutPut(RobotStage curStage, Port curPort)
//        {

//            string strlog = string.Empty;
//            string tmpStageStatus = string.Empty;

//            try
//            {

//                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

//                #region [ 比對UDRQ and LDRQ Stage Status 判斷最終狀態 ]

//                if (curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_READY || curStage.File.Stage_UDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
//                {
//                    if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
//                    {
//                        //可出片 可收片
//                        tmpStageStatus = eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY;

//                    }
//                    else
//                    {
//                        //可出片 不可收片
//                        tmpStageStatus = eRobotStageStatus.SEND_OUT_READY;

//                    }

//                }
//                else
//                {
//                    if (curStage.File.Stage_LDRQ_Status == eRobotStageStatus.RECEIVE_READY || curStage.File.Stage_LDRQ_Status == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
//                    {
//                        //不可出片 可收片
//                        tmpStageStatus = eRobotStageStatus.RECEIVE_READY;

//                    }
//                    else
//                    {
//                        //不可出片 不可收片
//                        tmpStageStatus = eRobotStageStatus.NO_REQUEST;

//                    }

//                }

//                #endregion

//                #region [ 更新Stage Status ]

//                #region  [DebugLog]

//                if (IsShowDetialLog == true)
//                {
//                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) PortType({4}) CassetteStatus({5}) Stage UDRQ Status({6}), LDRQ Status({7}) , Judge Stage Status({8})!",
//                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
//                                            curPort.File.Type, curPort.File.CassetteStatus, curStage.File.Stage_UDRQ_Status, curStage.File.Stage_LDRQ_Status,
//                                            tmpStageStatus);

//                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

//                }

//                #endregion

//                if (tmpStageStatus == eRobotStageStatus.SEND_OUT_READY || tmpStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
//                {

//                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, curStage.File.CurSendOut_CSTSeq, curStage.File.CurSendOut_JobSeq, curStage.File.CurSendOut_CSTSeq02, curStage.File.CurSendOut_JobSeq02);
//                }
//                else
//                {
//                    UpdateStageStatus(curStage, tmpStageStatus, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty, string.Empty, string.Empty);
//                }

//                #endregion
//            }
//            catch (Exception ex)
//            {

//                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
//            }

//        }

        #endregion

        //================================================================================================================================================================================

        #region [ 20151109 Mark_001 目前沒用到 ]

        /// <summary> Select All EQP Type(LinkSignal) Stage Can Control Job List by GetGetPutPut
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //public bool Select_EQP_ForGetGetPutPut(IRobotContext robotConText)
        //{

        //    string strlog = string.Empty;
        //    string errMsg = string.Empty;
        //    List<RobotStage> curRobotStageList = null;
        //    List<Job> robotStageCanControlJobList ;

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

        //            robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_curRobot_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get CurRobot All Stage List ]

        //        curRobotStageList = (List<RobotStage>)robotConText[eRobotContextParameter.CurRobotAllStageListEntity];

        //        //找不到 Robot Stage 回NG
        //        if (curRobotStageList == null)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo!",
        //                                        curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) can not Get RobotStageInfo!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_RobotStageList_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        if (curRobotStageList.Count == 0)
        //        {

        //            #region[DebugLog]

        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo!",
        //                                        curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }

        //            #endregion

        //            errMsg = string.Format("[{0}] Robot({1}) Get RobotStageInfo is Empty!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

        //            robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_RobotStageList_Is_Empty);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get Current Stage Can Control Job List ]

        //        robotStageCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.StageCanControlJobList];

        //        //當沒有值時則要建立
        //        if (robotStageCanControlJobList == null)
        //        {
        //            robotStageCanControlJobList = new List<Job>();
        //        }

        //        #endregion

        //        foreach (RobotStage stage_entity in curRobotStageList)
        //        {

        //            //非EQP Type Stage則不判斷
        //            if (stage_entity.Data.STAGETYPE.ToUpper().Trim() != eRobotStageType.EQUIPMENT)
        //            {
        //                continue;
        //            }

        //            Get_EqpTypeStageStatus_ForGetGetPutPut(curRobot, stage_entity, robotStageCanControlJobList);

        //        }

        //        //[ Wait_Proc_0009 ] 後續處理
        //        robotConText.AddParameter(eRobotContextParameter.StageCanControlJobList, robotStageCanControlJobList);
        //        robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.Result_Is_OK);
        //        robotConText.SetReturnMessage(eRobotSelectJob_ReturnMessage.OK_Message);

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

        private void Get_EqpTypeStageStatus_ForGetGetPutPut(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList)
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
                    
                }
                else
                {
                    //for Mulit Slot Stage

                    #region [ Get Stage UDRQ Status and CanControlJobList ]
                    
                    Get_EqpTypeMulitSlot_CanControlJobList_ForGetGet(curRobot, curStage, curStageCanControlJobList);

                    #endregion

                    #region [ Get Stage LDRQ Status ]

                    Get_EqpTypeMuliSlot_LDRQStauts_ForGetGetPutPut(curRobot, curStage);

                    #endregion

                    JudgeEQPStage_UDRQ_LDRQStatus_ForGetGetPutPut(curStage);

                }  

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary> 取得Mulit Slot Signal Mode Stage Can ControlJobList for GetGetPutPut Use . Only For Mulit Slot Signal Mode
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        private void Get_EqpTypeMulitSlot_CanControlJobList_ForGetGet(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList)
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

                if(curStage.Data.UPSTREAMPATHTRXNAME.Trim() ==string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find UpStream TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return ;

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

                    //Stage GetGet表示是走ST to RB Mulit Slot Signal Mode
                    if (up_UpstreamInline == bitOn && up_SendReady== bitOn && up_Send == bitOn)
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

                        #region  [ 先根據根據SlotNo 取得JobData ]

                        Job curSendOutJob=new Job();

                        if (Get_LinkSignalSendOutJobInfo_ForGetGetPutPut(curRobot, curStage, curStageCanControlJobList, slotNo, out curSendOutJob) == true)
                        {
                            //找到符合SendOut的Job ,判斷是SendOut 2片還是一片
                            if (up_DoubleGlass != "1")
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

                                if (Get_LinkSignalSendOutJobInfo_ForGetGetPutPut(curRobot, curStage, curStageCanControlJobList, anotherSlotNo, out curSendOutJob02) == true)
                                {

                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}) ,Stage UDRQ Status change to (UDRQ)!",
                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                                trxID, eBitResult.ON.ToString(), eBitResult.ON.ToString(), eBitResult.ON.ToString());
                                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    //Update Status UDRQ Stage Change To UDRQ
                                    UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo, 
                                                            curSendOutJob02.CassetteSequenceNo, curSendOutJob02.JobSequenceNo);
                                    return;
                                }
                                else
                                {
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

                                    //Update Status UDRQ Stage Change To UDRQ
                                    UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo, string.Empty, string.Empty);
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

        /// <summary> 取得LinkSignal SendOut的 JobData
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        /// <param name="sendOutSlotNo"></param>
        /// <param name="curSendOutJob"></param>
        /// <returns></returns>
        private bool Get_LinkSignalSendOutJobInfo_ForGetGetPutPut(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList, int sendOutSlotNo , out Job curSendOutJob)
        {
            string strlog = string.Empty;
            string jobKey = string.Empty;
            Job returnJob =new Job();
            curSendOutJob = returnJob;

            try
            {
                
                string funcName = MethodBase.GetCurrentMethod().Name;

                #region [ Check Trx Setting Exist ]

                if (curStage.Data.UPSTREAMJOBDATAPATHTRXNAME.Trim() ==string.Empty)
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
                
                //取得所有SendOut的TrxID
                string[] sendOutTrxList = curStage.Data.UPSTREAMJOBDATAPATHTRXNAME.Split(',');
                string strSlotNoTrxKey = string.Format("{0}_SendOutJobDataReport#{1}",curStage.Data.NODENO, sendOutSlotNo.ToString().PadLeft(2, '0'));
                string trxID = string.Empty;

                for (int i = 0; i < sendOutTrxList.Length; i++)
                {

                    if (strSlotNoTrxKey == sendOutTrxList[i])
                    {
                        trxID = sendOutTrxList[i];
                        break;

                    }

                }

                if (trxID == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) sendout SlotNo({4}) but can not find SendOutJobData TrxID({5}) by setting({6})!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                sendOutSlotNo.ToString(), strSlotNoTrxKey, curStage.Data.UPSTREAMJOBDATAPATHTRXNAME);
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
                        curBcsJob.RobotWIP.CurSendOutJobJudge != sendOut_JobJudge)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Job CassetteSequenceNo({1}) JobSequenceNo({2}) Update RobotWIP curStageType from ({3}) to ({4}), curStageID from ({5}) to ({6}), curSlotNo From ({7}) to ({8}), SendOut TrackingData from ({9}) to ({10}),sendOutJobJudge from ({11}) to ({12}) PortCSTStatusPriority from ({13}) to ({14}).",
                                                curStage.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurLocation_StageType,
                                                eRobotStageType.EQUIPMENT, curBcsJob.RobotWIP.CurLocation_StageID, curStage.Data.STAGEID, curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(),
                                                sendOutSlotNo.ToString(), curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData, sendOut_TrackingData, curBcsJob.RobotWIP.CurSendOutJobJudge,
                                                sendOut_JobJudge, curBcsJob.RobotWIP.CurPortCstStatusPriority, eLoaderPortSendOutStatus.NOT_IN_PORT);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {

                            curBcsJob.RobotWIP.CurLocation_StageID = curStage.Data.STAGEID;
                            curBcsJob.RobotWIP.CurLocation_SlotNo = sendOutSlotNo;
                            curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.EQUIPMENT;
                            curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData = sendOut_TrackingData;
                            curBcsJob.RobotWIP.CurPortCstStatusPriority = eLoaderPortSendOutStatus.NOT_IN_PORT;
                            curBcsJob.RobotWIP.CurSendOutJobJudge = sendOut_JobJudge;

                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    //20150818 Work

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

        /// <summary> 取得Muit Slot Signal Mode Stage LDRQ的狀態與EmptySlotInfo
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        private void Get_EqpTypeMuliSlot_LDRQStauts_ForGetGetPutPut(Robot curRobot, RobotStage curStage)
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

                        #region  [ 根據根據SlotNo與DoubleGlass取得Stage EmptySlotNo ]

                        if (down_DoubleGlass != "1")
                        {
                            

                            //only 1 EmptySlot, 更新Current Stage LDRQ Empty Slot
                            lock (curStage)
                            {
                                curStage.CurLDRQ_EmptySlotNo = slotNo.ToString().PadLeft(2, '0');
                                curStage.CurLDRQ_EmptySlotNo02 = string.Empty;

                                //add Empty SlotNo To EmptySlotNoList
                                if (curStage.curLDRQ_EmptySlotList.ContainsKey(slotNo) == false)
                                {
                                    curStage.curLDRQ_EmptySlotList.Add(i, string.Empty);
                                }
                            }

                        }
                        else
                        {

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

                            //has 2 EmptySlot, 更新Current Stage LDRQ Empty Slot
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
                        UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST , curStage.File.LDRQ_CstStatusPriority, funcName);

                    }

                }

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }


        }

        private void JudgeEQPStage_UDRQ_LDRQStatus_ForGetGetPutPut(RobotStage curStage)
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


        //Watson Add 20151005 For TTP DailyCheck glass
        //private bool Replace_NoramlandDailyChk_RouteList(Job curJob)
        //{
        //    bool dcglass = false;
        //    SerializableDictionary<int, RobotRouteStep> normalStepLists = curJob.RobotWIP.NormalRobotCheckRouteStepList;
        //    SerializableDictionary<int, RobotRouteStep> dcStepLists = curJob.RobotWIP.DailyCheckRouteStepList;

        //    try
        //    {
        //        IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(curJob.EQPFlag, "EQPFlag");

        //        if (subItem != null)
        //        {
        //            if (subItem.ContainsKey("ToTotalPitchSubChamber"))
        //            {
        //                if (subItem["ToTotalPitchSubChamber"] == ((int)eBitResult.ON).ToString())
        //                {
        //                    dcglass = true;
        //                    #region  [DebugLog]
        //                    if (IsShowDetialLog == true)
        //                    {
        //                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                            string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] HAVE DailyCheckGlassUse, [EQUIPMENT={2}] CAN NOT FIND  TO ToTotalPitchSubChamber.",
        //                            curJob.CassetteSequenceNo, curJob.JobSequenceNo, curJob.CurrentEQPNo));
        //                    }
        //                    #endregion
        //                }
        //            }

        //            if (subItem.ContainsKey("ToTotalPitch(ForDailyCheckGlassUse)"))
        //            {
        //                if (subItem["ToTotalPitch(ForDailyCheckGlassUse)"] == ((int)eBitResult.ON).ToString())
        //                {
        //                    dcglass = true;
        //                    #region  [DebugLog]
        //                    if (IsShowDetialLog == true)
        //                    {
        //                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                            string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] HAVE DailyCheckGlassUse, [EQUIPMENT={2}] CAN NOT FIND  TO ToTotalPitch(ForDailyCheckGlassUse).",
        //                            curJob.CassetteSequenceNo, curJob.JobSequenceNo, curJob.CurrentEQPNo));
        //                    }
        //                    #endregion
        //                }
        //            }

        //        }

        //        if (dcglass)
        //        {
        //            if (!curJob.RobotWIP.RobotRouteStepList[curJob.RobotWIP.CurStepNo].Data.ROUTEID.ToUpper().Contains("DAILY_CHECK"))
        //            {
        //                curJob.RobotWIP.NormalRobotCheckRouteStepList = curJob.RobotWIP.RobotRouteStepList;
        //                curJob.RobotWIP.RobotRouteStepList = dcStepLists;
        //            }
        //        }
        //        else
        //        {
        //            if (curJob.RobotWIP.RobotRouteStepList[curJob.RobotWIP.CurStepNo].Data.ROUTEID.ToUpper().Contains("DAILY_CHECK"))
        //            {
        //                curJob.RobotWIP.DailyCheckRouteStepList = curJob.RobotWIP.RobotRouteStepList;
        //                curJob.RobotWIP.RobotRouteStepList = normalStepLists;
        //            }
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return false;
        //    }
        //}

        //public bool Select_TTPJob_ForDailyCheck(IRobotContext robotConText)
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

        //            robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_curRobot_Is_Null);
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
        //                curRobot.Data.ROBOTNAME, MethodBase.GetCurrentMethod().Name);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);
        //            return false;
        //        }

        //        #endregion

        //        #region Get EQP Flag
        //        IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(curBcsJob.EQPFlag, "EQPFlag");

        //        if (subItem != null)
        //        {
        //            if (subItem.ContainsKey("ToTotalPitchSubChamber"))
        //            {
        //                if (subItem["ToTotalPitchSubChamber"] == ((int)eBitResult.ON).ToString())
        //                {
        //                    #region  [DebugLog]
        //                    if (IsShowDetialLog == true)
        //                    {
        //                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                            string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] HAVE DailyCheckGlassUse, [EQUIPMENT={2}] CAN NOT FIND  TO ToTotalPitchSubChamber.",
        //                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.CurrentEQPNo));
        //                    }
        //                    #endregion
        //                    return true;
        //                }
        //            }

        //            if (subItem.ContainsKey("ToTotalPitch(ForDailyCheckGlassUse)"))
        //            {
        //                if (subItem["ToTotalPitch(ForDailyCheckGlassUse)"] == ((int)eBitResult.ON).ToString())
        //                {
        //                    #region  [DebugLog]
        //                    if (IsShowDetialLog == true)
        //                    {
        //                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                            string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] HAVE DailyCheckGlassUse, [EQUIPMENT={2}] CAN NOT FIND  TO ToTotalPitch(ForDailyCheckGlassUse).",
        //                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.CurrentEQPNo));
        //                    }
        //                    #endregion
        //                    return true;
        //                }
        //            }
        //        }
        //        #endregion

        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return false;
        //    }
        //}
        //private bool Get_LinkSignalSendOutJobInfo(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList, int sendOutSlotNo, out Job curSendOutJob)
        //{
        //    string strlog = string.Empty;
        //    string jobKey = string.Empty;
        //    Job returnJob = new Job();
        //    curSendOutJob = returnJob;

        //    try
        //    {

        //        string funcName = MethodBase.GetCurrentMethod().Name;

        //        #region [ Check Trx Setting Exist ]

        //        if (curStage.Data.UPSTREAMJOBDATAPATHTRXNAME.Trim() == string.Empty)
        //        {
        //            #region  [DebugLog]
        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) can not find SendOutJobData TrxID setting!",
        //                                        curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }
        //            #endregion
        //            return false;
        //        }

        //        //取得所有SendOut的TrxID
        //        string[] sendOutTrxList = curStage.Data.UPSTREAMJOBDATAPATHTRXNAME.Split(',');
        //        string strSlotNoTrxKey = string.Format("{0}_SendOutJobDataReport#{1}", curStage.Data.NODENO, sendOutSlotNo.ToString().PadLeft(2, '0'));
        //        string trxID = string.Empty;

        //        for (int i = 0; i < sendOutTrxList.Length; i++)
        //        {
        //            if (strSlotNoTrxKey == sendOutTrxList[i])
        //            {
        //                trxID = sendOutTrxList[i];
        //                break;
        //            }
        //        }

        //        if (trxID == string.Empty)
        //        {
        //            #region  [DebugLog]
        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) sendout SlotNo({4}) but can not find SendOutJobData TrxID({5}) by setting({6})!",
        //                                        curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
        //                                        sendOutSlotNo.ToString(), strSlotNoTrxKey, curStage.Data.UPSTREAMJOBDATAPATHTRXNAME);
        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }
        //            #endregion
        //            return false;
        //        }
        //        #endregion

        //        #region  real time Get Trx by sendOutSlotNo
        //        Trx GetJobData_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

        //        if (GetJobData_Trx == null)
        //        {
        //            #region  [DebugLog]
        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
        //                                        curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
        //                                        trxID);
        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }
        //            #endregion
        //            return false;
        //        }
        //        #endregion

        //        #region [拆出PLCAgent Data]

        //        #region [ Trx Structure ]
        //        //<trx name="L3_SendOutJobDataReport#01" triggercondition="change">
        //        //    <eventgroup name="L3_EG_SendOutJobDataReport#01" dir="E2B">
        //        //      <event name="L3_W_SendOutJobDataBlock_JobData1" />
        //        //      <event name="L3_B_SendOutJobDataReport#01" trigger="true" />
        //        //    </eventgroup>
        //        //  </trx>

        //        //<itemgroup name="JobData">
        //        //    <item name="CassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
        //        //    <item name="JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
        //        //    <item name="GroupIndex" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
        //        //    <item name="ProductType" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
        //        //    <item name="CSTOperationMode" woffset="4" boffset="0" wpoints="1" bpoints="2" expression="INT" />
        //        //    <item name="SubstrateType" woffset="4" boffset="2" wpoints="1" bpoints="2" expression="INT" />
        //        //    <item name="CIMMode" woffset="4" boffset="4" wpoints="1" bpoints="1" expression="INT" />
        //        //    <item name="JobType" woffset="4" boffset="5" wpoints="1" bpoints="4" expression="INT" />
        //        //    <item name="JobJudge" woffset="4" boffset="9" wpoints="1" bpoints="4" expression="INT" />
        //        //    <item name="SamplingSlotFlag" woffset="4" boffset="13" wpoints="1" bpoints="1" expression="BIN" />
        //        //    <item name="FirstRunFlag" woffset="4" boffset="14" wpoints="1" bpoints="1" expression="BIN" />
        //        //    <item name="JobGrade" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="ASCII" />
        //        //    <item name="Glass/Chip/MaskID/BlockID" woffset="6" boffset="0" wpoints="10" bpoints="160" expression="ASCII" />
        //        //    <item name="PPID" woffset="16" boffset="0" wpoints="25" bpoints="400" expression="ASCII" />
        //        //    <item name="GlassFlowType" woffset="41" boffset="0" wpoints="1" bpoints="6" expression="INT" />
        //        //    <item name="ProcessType" woffset="41" boffset="6" wpoints="1" bpoints="6" expression="INT" />
        //        //    <item name="LastGlassFlag" woffset="41" boffset="12" wpoints="1" bpoints="1" expression="BIN" />
        //        //    <item name="RTCFlag" woffset="41" boffset="13" wpoints="1" bpoints="1" expression="BIN" />
        //        //    <item name="MainEQInFlag" woffset="41" boffset="14" wpoints="1" bpoints="1" expression="BIN" />
        //        //    <item name="Insp.JudgedData" woffset="42" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
        //        //    <item name="TrackingData" woffset="44" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
        //        //    <item name="EQPFlag" woffset="46" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
        //        //    <item name="ChipCount" woffset="48" boffset="0" wpoints="1" bpoints="9" expression="INT" />
        //        //    <item name="RecipeGroupNumber" woffset="48" boffset="10" wpoints="1" bpoints="6" expression="INT" />
        //        //    <item name="SourcePortNo" woffset="49" boffset="0" wpoints="1" bpoints="8" expression="INT" />
        //        //    <item name="TargetPortNo" woffset="49" boffset="8" wpoints="1" bpoints="8" expression="INT" />
        //        //</itemgroup>

        //        string cstSeq = GetJobData_Trx.EventGroups[0].Events[0].Items["CassetteSequenceNo"].Value;
        //        string jobSeq = GetJobData_Trx.EventGroups[0].Events[0].Items["JobSequenceNo"].Value;
        //        string sendOut_TrackingData = GetJobData_Trx.EventGroups[0].Events[0].Items["TrackingData"].Value;
        //        string sendOut_JobJudge = GetJobData_Trx.EventGroups[0].Events[0].Items["JobJudge"].Value;
        //        string eqpFlag = GetJobData_Trx.EventGroups[0].Events[0].Items["EQPFlag"].Value;  //Watson Add 20151017
        //        #endregion

        //        #endregion

        //        if (cstSeq != "0" && jobSeq != "0")
        //        {
        //            jobKey = string.Format("{0}_{1}", cstSeq.ToString(), jobSeq.ToString());

        //            #region  [DebugLog]
        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4}) ,CSTSeq({5}) JobSeq({6}) JobKey({7}).",
        //                                        curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
        //                                        trxID, cstSeq, jobSeq, jobKey);
        //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }
        //            #endregion

        //            #region [ Check Job is Exist ]

        //            Job curBcsJob = ObjectManager.JobManager.GetJob(cstSeq, jobSeq);

        //            if (curBcsJob == null)
        //            {
        //                #region  [DebugLog]
        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBMRCS Robot({1}) StageID({2}) StageName({3}) Can not Get Job by CSTSeq({4}) JobSeq({5})!",
        //                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
        //                                            cstSeq, jobSeq);
        //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }
        //                #endregion
        //                return false; //Not In WIP
        //            }
        //            #endregion

        //            #region [ Update Job RobotWIP ]
        //            //有變化才紀錄Log   LinkSignal要看Send Out JobData的TrackingData .Route Priority目前直接參照ROBOTWIP內Route資訊排序即可
        //            if (curBcsJob.RobotWIP.CurLocation_StageID != curStage.Data.STAGEID ||
        //                curBcsJob.RobotWIP.CurLocation_SlotNo != sendOutSlotNo ||
        //                curBcsJob.RobotWIP.CurLocation_StageType != eRobotStageType.EQUIPMENT ||
        //                curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData != sendOut_TrackingData ||
        //                curBcsJob.RobotWIP.CurPortCstStatusPriority != eLoaderPortSendOutStatus.NOT_IN_PORT ||
        //                curBcsJob.RobotWIP.CurSendOutJobJudge != sendOut_JobJudge)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Job CassetteSequenceNo({1}) JobSequenceNo({2}) Update RobotWIP curStageType from ({3}) to ({4}), curStageID from ({5}) to ({6}), curSlotNo From ({7}) to ({8}), SendOut TrackingData from ({9}) to ({10}),sendOutJobJudge from ({11}) to ({12}) PortCSTStatusPriority from ({13}) to ({14}).",
        //                                        curStage.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurLocation_StageType,
        //                                        eRobotStageType.EQUIPMENT, curBcsJob.RobotWIP.CurLocation_StageID, curStage.Data.STAGEID, curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(),
        //                                        sendOutSlotNo.ToString(), curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData, sendOut_TrackingData, curBcsJob.RobotWIP.CurSendOutJobJudge,
        //                                        sendOut_JobJudge, curBcsJob.RobotWIP.CurPortCstStatusPriority, eLoaderPortSendOutStatus.NOT_IN_PORT);

        //                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //                lock (curBcsJob)
        //                {
        //                    curBcsJob.RobotWIP.CurLocation_StageID = curStage.Data.STAGEID;
        //                    curBcsJob.RobotWIP.CurLocation_SlotNo = sendOutSlotNo;
        //                    curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.EQUIPMENT;
        //                    curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData = sendOut_TrackingData;
        //                    curBcsJob.RobotWIP.CurPortCstStatusPriority = eLoaderPortSendOutStatus.NOT_IN_PORT;
        //                    curBcsJob.RobotWIP.CurSendOutJobJudge = sendOut_JobJudge;
        //                    curBcsJob.RobotWIP.CurEQPFLAG = eqpFlag;

        //                }
        //                //Save File
        //                ObjectManager.JobManager.EnqueueSave(curBcsJob);

        //            }
        //            #endregion

        //            #region [ Add to Can Control Job List ]

        //            if (AddToCanControlJoblistCondition(curRobot, curStage, curBcsJob, curStageCanControlJobList, jobKey, funcName) == true)
        //            {
        //                #region [DebugLog]
        //                if (IsShowDetialLog == true)
        //                {
        //                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) add CassetteSequenceNo({4}) JobSequenceNo({5}) CurRouteStepNo({6}) CurSlotNo({7}) to Can control List!",
        //                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
        //                                            curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.CurLocation_SlotNo.ToString());

        //                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //                }
        //                #endregion
        //                curSendOutJob = curBcsJob;
        //                //GetGetPutPut 在決定好所有SendOutJob後再一次更新Status
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //            #endregion

        //        }
        //        else
        //        {
        //            #region  [DebugLog]
        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4}) ,CSTSeq({5}) JobSeq({6}) Job is not Exist!",
        //                                        curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
        //                                        trxID, cstSeq, jobSeq);
        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }
        //            #endregion
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return false;
        //    }

        //}



        #region Array shop / DRY line
     


        #endregion


        //================================================================================================================================================================================

    }
}
