using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniRCS.Core
{
    public abstract partial class AbstractRobotService : AbstractService
    {
        public class PortStage
        {
            public Port Port { get; private set; }
            public RobotStage Stage { get; private set; }
            public PortStage(Port port, RobotStage stage)
            {
                Port = port;
                Stage = stage;
            }
        }

        private const string SHOWDETIALLOG = "SHOW_ROBOT_DETIAL_LOG";
        private const string SHOWCANNOTISSUEROBOTCMDMSGG = "SHOW_CANNOT_ISSUE_ROBOTCMD_MSG";
        //預設2000 * 0.3ms =600s
        public static int SHOWDETIALLOG_ERROR_TIME = 2000; //Error count 

        private static RobotContext _context = new RobotContext();

        /// <summary>
        /// 拜託不要清成NULL
        /// </summary>
        public static RobotContext StaticContext
        {
            get { return _context; }
        }

        //20151223 modify 紀錄2nd
        private static int _errorCircleTime;

        /// <summary>
        /// 记录Error Circle 的次
        /// </summary>
        public int ErrorCircleTime
        {
            get { return _errorCircleTime; }
            set { _errorCircleTime = value; }
        }

        /// <summary> Stauge Change時要顯示detail Log
        /// 
        /// </summary>
        private bool _robotStatusChangeFlag = false;

        /// <summary> Update Robot Status Change Flag 以決定是否顯示Detail Log
        /// 
        /// </summary>
        /// <param name="upDateFlag"></param>
        protected void UpdateRobotStatusChangeShowDetailLogFlag(bool upDateFlag)
        {

            _robotStatusChangeFlag = upDateFlag;

        }

        /// <summary> Get Show Robot Detial Log Flag
        /// Show Detial Log 开启或者  扫描次数发送100 后就会记录 Detial Log 
        /// </summary>
        protected bool IsShowDetialLog
        {
            get
            {
                return ParameterManager[SHOWDETIALLOG].GetBoolean() || (ErrorCircleTime >= SHOWDETIALLOG_ERROR_TIME);
            }
        }

        /// <summary> Get Send cannot Issus Robot Command Message to OPI Flag
        ///
        /// </summary>
        protected bool IsShowCanNotIssueRobotCmdMsg
        {
            get { return ParameterManager[SHOWCANNOTISSUEROBOTCMDMSGG].GetBoolean(); }
        }

        /// <summary> 將Job無法被搬送的原因送到OPI顯示
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="failCode"></param>
        /// <param name="msgInfo"></param>
        /// <param name="msgType"></param>
        protected void SendCanNotIssueCmdMsgToOPI(Robot curRobot, string failCode, string msgInfo, string msgType)
        {
            string cmdMsg = string.Empty;


            try
            {

                if (IsShowCanNotIssueRobotCmdMsg == true)
                {

                    //cmdMsg = string.Format("{0} - FailCode={1} , FailDesc={2}",
                    //                       "CanNotIssueRobotCommad".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                    //                       failCode.PadRight(eRobotCommonConst.LOG_FAILCODE_LENGTH, ' '),
                    //                       msgInfo);

                    cmdMsg = string.Format("{0} - FailDesc={1}",
                                           "CanNotIssueRobotCommad".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                           msgInfo);


                    Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, msgType });
                }
                /*
                //20170208 Menghui     Error Code 写入Monitor文件夹：eJob_CheckFail_Reason/eRobot_CheckFail_Reason 传至BMS
                if (failCode.Length == 10)
                {
                    string a = failCode.Substring(0, 1);
                    if (a == "R" || a == "J")
                    {
                        Equipment curEQP = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
                       
                        Invoke(eServiceName.EvisorService, "AppErrorReport", new object[] { curEQP.Data.LINEID, failCode, msgInfo });
                    }
                }
                 */

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        #region [ Update Stage Stutas Function List ] ===============================================================================================================================

        /// <summary> Update Stage Status 相關資訊 for Normal Get,Put.
        ///
        /// </summary>
        /// <param name="curStage"></param>
        /// <param name="newStageStatus"></param>
        /// <param name="funcName">由哪個method調用,以便Trace</param>
        /// <param name="sendOutCstSeq">當狀態UDRQ or UDRQ_LDRQ時填入SendOut的CST Seq,如果不是則填String.empty</param>
        /// <param name="sendOutJobSeq">當狀態UDRQ or UDRQ_LDRQ時填入SendOut的JOB Seq,如果不是則填String.empty</param>
        protected void UpdateStageStatus(RobotStage curStage, string newStageStatus, string funcName, string sendOutCstSeq, string sendOutJobSeq)
        {
            string strlog = string.Empty;

            try
            {

                #region [ 20151006 add Check Send Out Job is Change ]

                bool sendOutJobChangeFlag = false;
                string new1stSlotSendOutCstSeq = string.Empty;
                string new1stSlotSendOutJobSeq = string.Empty;

                if ((curStage.File.CurStageStatus == newStageStatus) &&
                    (curStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY ||
                     curStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY))
                {

                    if (curStage.curUDRQ_SlotList.Count > 0)
                    {
                        string old1stSendOutJobKey = string.Format("{0}_{1}", curStage.File.CurSendOut_CSTSeq, curStage.File.CurSendOut_JobSeq);

                        foreach (int slotNo in curStage.curUDRQ_SlotList.Keys)
                        {
                            //判斷目前1st SendOut SlotNo 內容是否與之前最後紀錄的1st SendOut的job 相同, 如果不同則要更新
                            if (curStage.curUDRQ_SlotList[slotNo] != string.Empty && curStage.curUDRQ_SlotList[slotNo] != old1stSendOutJobKey)
                            {
                                string[] newSendOutJobInfo = curStage.curUDRQ_SlotList[slotNo].Split('_');

                                if (newSendOutJobInfo.Length > 1)
                                {
                                    new1stSlotSendOutCstSeq = newSendOutJobInfo[0];
                                    new1stSlotSendOutJobSeq = newSendOutJobInfo[1];
                                    sendOutJobChangeFlag = true;
                                }
                            }
                            break;
                        }

                    }

                }

                #endregion

                #region [ Stage Status Change才需要Update ]

                if (curStage.File.CurStageStatus != newStageStatus || sendOutJobChangeFlag == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageNo({3}) StageName({4}) status change from ({5}) to ({6}) , UDRQ Job CassetteSequenceNo({7}) JobSequenceNo({8})!",
                                                    curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                    curStage.Data.STAGENAME, curStage.File.CurStageStatus, newStageStatus, sendOutCstSeq,
                                                    sendOutJobSeq);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //add for Log Quick Trace
                    strlog = string.Format("[{0}] {1} - {2}({3}) Stage Status Change From({4}) to ({5}) ,sendOut Job({6},{7}) by [{8}]",
                                            "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                            MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                            curStage.Data.STAGENAME,
                                            curStage.Data.STAGEID, curStage.File.CurStageStatus, newStageStatus, sendOutCstSeq,
                                            sendOutJobSeq, funcName);

                    Logger.LogTrxWrite(this.LogName, strlog);

                    #region [ Update Robot Stage Entity ]

                    lock (curStage.File)
                    {
                        curStage.File.CurStageStatus = newStageStatus;

                        //20151006 Modify
                        if (sendOutJobChangeFlag == true)
                        {
                            curStage.File.CurSendOut_CSTSeq = new1stSlotSendOutCstSeq;
                            curStage.File.CurSendOut_JobSeq = new1stSlotSendOutJobSeq;

                        }
                        else
                        {
                            curStage.File.CurSendOut_CSTSeq = sendOutCstSeq;
                            curStage.File.CurSendOut_JobSeq = sendOutJobSeq;
                        }

                        curStage.File.StatusChangeFlag = true;
                    }

                    #endregion

                }

                #endregion

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {

                    //Get Current Stage Info To Log
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageNo({3}) StageName({4}) status is ({5}) , UDRQ Job CassetteSequenceNo({6}) JobSequenceNo({7})!",
                                                                curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                                curStage.Data.STAGENAME, newStageStatus, sendOutCstSeq, sendOutJobSeq);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> 更新Stage UDRQ Status and LDRQ Cassette Status Priority for [ Normal ] Get,Put Use
        ///
        /// </summary>
        /// <param name="curStage"></param>
        /// <param name="newUDRQStageStatus"></param>
        /// <param name="funcName"></param>
        /// <param name="sendOutCstSeq"></param>
        /// <param name="sendOutJobSeq"></param>
        protected void UpdateStage_UDRQ_Status(RobotStage curStage, string newUDRQStageStatus, string funcName, string sendOutCstSeq, string sendOutJobSeq)
        {
            string strlog = string.Empty;

            try
            {

                #region [ 20151006 add Check Send Out Job is Change ]

                bool sendOutJobChangeFlag = false;
                string new1stSlotSendOutCstSeq = string.Empty;
                string new1stSlotSendOutJobSeq = string.Empty;



                if ((curStage.File.CurStageStatus == newUDRQStageStatus) &&
                    (curStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY ||
                     curStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY))
                {

                    if (curStage.curUDRQ_SlotList.Count > 0)
                    {
                        string old1stSendOutJobKey = string.Format("{0}_{1}", curStage.File.CurSendOut_CSTSeq, curStage.File.CurSendOut_JobSeq);

                        foreach (int slotNo in curStage.curUDRQ_SlotList.Keys)
                        {
                            //判斷目前1st SendOut SlotNo 內容是否與之前最後紀錄的1st SendOut的job 相同, 如果不同則要更新
                            if (curStage.curUDRQ_SlotList[slotNo] != string.Empty && curStage.curUDRQ_SlotList[slotNo] != old1stSendOutJobKey)
                            {
                                string[] newSendOutJobInfo = curStage.curUDRQ_SlotList[slotNo].Split('_');

                                if (newSendOutJobInfo.Length > 1)
                                {
                                    new1stSlotSendOutCstSeq = newSendOutJobInfo[0];
                                    new1stSlotSendOutJobSeq = newSendOutJobInfo[1];
                                    sendOutJobChangeFlag = true;
                                }
                            }
                            break;
                        }

                    }

                }

                #endregion

                #region [ Stage Status Change才需要Update ]

                if (curStage.File.Stage_UDRQ_Status != newUDRQStageStatus || sendOutJobChangeFlag == true)
                {
                    //20151231 Update Log Error
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) UDRQ status change from ({4}) to ({5}) , UDRQ Job CassetteSequenceNo({6}) JobSequenceNo({7}) by [{8}]",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                    curStage.Data.STAGENAME, curStage.File.Stage_UDRQ_Status, newUDRQStageStatus, sendOutCstSeq,
                                                    sendOutJobSeq, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //add for Log Quick Trace
                    strlog = string.Format("[{0}] {1} - {2}({3}) Stage UDRQ Status Change From({4}) to ({5}) ,sendOut Job({6},{7}) by [{8}]",
                                            "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                            MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                            curStage.Data.STAGENAME,
                                            curStage.Data.STAGEID, curStage.File.Stage_UDRQ_Status, newUDRQStageStatus, sendOutCstSeq,
                                            sendOutJobSeq, funcName);

                    Logger.LogTrxWrite(this.LogName, strlog);

                    #region [ Update Robot Stage Entity ]

                    lock (curStage.File)
                    {
                        curStage.File.Stage_UDRQ_Status = newUDRQStageStatus;

                        //20151006 modify 如果SendOut First Slot Job 資訊變化則以更新後的來做First資訊
                        if (sendOutJobChangeFlag == true)
                        {
                            curStage.File.CurSendOut_CSTSeq = new1stSlotSendOutCstSeq;
                            curStage.File.CurSendOut_JobSeq = new1stSlotSendOutJobSeq;
                        }
                        else
                        {
                            curStage.File.CurSendOut_CSTSeq = sendOutCstSeq;
                            curStage.File.CurSendOut_JobSeq = sendOutJobSeq;
                        }

                        curStage.File.StatusChangeFlag = true;
                    }

                    #endregion

                }

                #endregion

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {

                    //Get Current Stage Info To Log
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) status is ({4}) , UDRQ Job CassetteSequenceNo({5}) JobSequenceNo({6}) by [{7}].",
                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                            curStage.Data.STAGENAME, newUDRQStageStatus, sendOutCstSeq, sendOutJobSeq, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> 更新Stage UDRQ Status and LDRQ Cassette Status Priority for [ Normal ] Get,Put Use
        ///
        /// </summary>
        /// <param name="curStage"></param>
        /// <param name="newUDRQStageStatus"></param>
        /// <param name="funcName"></param>
        /// <param name="sendOutCstSeq"></param>
        /// <param name="sendOutJobSeq"></param>
        protected void UpdateStage_UDRQ_Status_for1Arm2Job(RobotStage curStage, string newUDRQStageStatus, string funcName)
        {
            string strlog = string.Empty;

            try
            {
                #region [ 20151006 add Check Send Out Job is Change ]

                //bool sendOutJobChangeFlag = false;
                //string new1stSlotSendOutCstSeq = string.Empty;
                //string new1stSlotSendOutJobSeq = string.Empty;

                //if ((curStage.File.CurStageStatus == newUDRQStageStatus) &&
                //    (curStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY ||
                //     curStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY))
                //{

                //    if (curStage.curUDRQ_SlotBlockInfoList.Count > 0)
                //    {
                //        string old1stSendOutJobKey = string.Format("{0}_{1}", curStage.File.CurSendOut_CSTSeq, curStage.File.CurSendOut_JobSeq);

                //        foreach (int slotNo in curStage.curUDRQ_SlotList.Keys)
                //        {
                //            //判斷目前1st SendOut SlotNo 內容是否與之前最後紀錄的1st SendOut的job 相同, 如果不同則要更新
                //            if (curStage.curUDRQ_SlotList[slotNo] != string.Empty && curStage.curUDRQ_SlotList[slotNo] != old1stSendOutJobKey)
                //            {
                //                string[] newSendOutJobInfo = curStage.curUDRQ_SlotList[slotNo].Split('_');

                //                if (newSendOutJobInfo.Length > 1)
                //                {
                //                    new1stSlotSendOutCstSeq = newSendOutJobInfo[0];
                //                    new1stSlotSendOutJobSeq = newSendOutJobInfo[1];
                //                    sendOutJobChangeFlag = true;
                //                }
                //            }
                //            break;
                //        }
                //    }
                //}

                #endregion

                #region [ Stage Status Change才需要Update ]

                if (curStage.File.Stage_UDRQ_Status != newUDRQStageStatus) // || sendOutJobChangeFlag == true)
                {
                    //20151231 Update Log Error
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) UDRQ status change from ({4}) to ({5}) by [{6}]",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                    curStage.Data.STAGENAME, curStage.File.Stage_UDRQ_Status, newUDRQStageStatus, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //add for Log Quick Trace
                    strlog = string.Format("[{0}] {1} - {2}({3}) Stage UDRQ Status Change From({4}) to ({5}) by [{6}]",
                                            "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                            MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                            curStage.Data.STAGENAME,
                                            curStage.Data.STAGEID, curStage.File.Stage_UDRQ_Status, newUDRQStageStatus, funcName);

                    Logger.LogTrxWrite(this.LogName, strlog);

                    #region [ Update Robot Stage Entity ]

                    lock (curStage.File)
                    {
                        curStage.File.Stage_UDRQ_Status = newUDRQStageStatus;

                        //20151006 modify 如果SendOut First Slot Job 資訊變化則以更新後的來做First資訊
                        //if (sendOutJobChangeFlag == true)
                        //{
                        //    curStage.File.CurSendOut_CSTSeq = new1stSlotSendOutCstSeq;
                        //    curStage.File.CurSendOut_JobSeq = new1stSlotSendOutJobSeq;
                        //}
                        //else
                        //{
                        //    curStage.File.CurSendOut_CSTSeq = sendOutCstSeq;
                        //    curStage.File.CurSendOut_JobSeq = sendOutJobSeq;
                        //}

                        curStage.File.StatusChangeFlag = true;
                    }

                    #endregion

                }

                #endregion

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {

                    //Get Current Stage Info To Log
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) status is ({4}) by [{5}].",
                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                            curStage.Data.STAGENAME, newUDRQStageStatus, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> 更新Stage LDRQ Status and LDRQ Cassette Status Priority for Normal Get,Put Use
        ///
        /// </summary>
        /// <param name="curStage"></param>
        /// <param name="newStageStatus"></param>
        /// <param name="portCstStatusPriority"></param>
        protected void UpdateStage_LDRQ_Status(RobotStage curStage, string newStageStatus, string portCstStatusPriority, string funcName)
        {
            string strlog = string.Empty;
            string udrqJobKey = string.Empty;

            try
            {

                if (curStage.File.Stage_LDRQ_Status != newStageStatus ||
                    curStage.File.LDRQ_CstStatusPriority != portCstStatusPriority)
                {

                    #region [ 有變化才紀錄Log ]

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) LDRQ status change from ({4}) to ({5}) , Port CST Stautus Priority form ({6}) to ({7})) by [{8}]!",
                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                            curStage.Data.STAGENAME, curStage.File.Stage_LDRQ_Status, newStageStatus, curStage.File.LDRQ_CstStatusPriority,
                                            portCstStatusPriority, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //add for Log Trace to Quick Check
                    strlog = string.Format("[{0}] {1} - {2}({3}) Stage LDRQ Status Change From({4}) to ({5}) ,sendOut Job({6},{7}). by [{8}]",
                                            "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                            MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                            curStage.Data.STAGENAME,
                                            curStage.Data.STAGEID,
                                            curStage.File.Stage_LDRQ_Status, newStageStatus, string.Empty, string.Empty, funcName);

                    Logger.LogTrxWrite(this.LogName, strlog);

                    lock (curStage.File)
                    {

                        curStage.File.Stage_LDRQ_Status = newStageStatus;
                        curStage.File.LDRQ_CstStatusPriority = portCstStatusPriority;
                        curStage.File.StatusChangeFlag = true;

                    }

                    #endregion

                }

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) LDRQ status change from ({4}) to ({5}) , Port CST Stautus Priority form ({6}) to ({7})) by [{8}].",
                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                            curStage.Data.STAGENAME, curStage.File.CurStageStatus, newStageStatus, curStage.File.LDRQ_CstStatusPriority,
                                            portCstStatusPriority, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> for Update Unload Stage Only ,更新Stage Status and ULD Receive Status Priority
        ///
        /// </summary>
        /// <param name="curStage"></param>
        /// <param name="newStageStatus"></param>
        /// <param name="portCstStatusPriority"></param>
        protected void UpdateUnloadStageStatus(RobotStage curStage, string newStageStatus, string portCstStatusPriority, string funcName)
        {
            string strlog = string.Empty;
            string udrqJobKey = string.Empty;

            try
            {

                if (curStage.File.CurStageStatus != newStageStatus || curStage.File.LDRQ_CstStatusPriority != portCstStatusPriority)
                {

                    #region [ 有變化才紀錄Log ]

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageNo({3}) StageName({4}) status change from ({5}) to ({6}) , Port CST Stautus Priority form ({7}) to ({8}))!",
                                            curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                            curStage.Data.STAGENAME, curStage.File.CurStageStatus, newStageStatus, curStage.File.LDRQ_CstStatusPriority,
                                            portCstStatusPriority);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //add for Log Trace
                    strlog = string.Format("[{0}] {1} - {2}({3}) Stage Status Change From({4}) to ({5}) ,sendOut Job({6},{7})",
                                            funcName.PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                            "UpdateStageStatus".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                            curStage.Data.STAGENAME,
                                            curStage.Data.STAGEID,
                                            curStage.File.CurStageStatus, newStageStatus, string.Empty, string.Empty);

                    Logger.LogTrxWrite(this.LogName, strlog);

                    lock (curStage.File)
                    {

                        curStage.File.CurStageStatus = newStageStatus;
                        curStage.File.LDRQ_CstStatusPriority = portCstStatusPriority;
                        curStage.File.StatusChangeFlag = true;

                    }

                    #endregion

                }

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) StageNo({3}) StageName({4}) status change from ({5}) to ({6}) , Port CST Stautus Priority form ({7}) to ({8}))!",
                                            curStage.Data.NODENO, funcName, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                            curStage.Data.STAGENAME, curStage.File.CurStageStatus, newStageStatus, curStage.File.LDRQ_CstStatusPriority,
                                            portCstStatusPriority);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> Update Stage Status 相關資訊 for GetGet PutPut Use Only
        ///
        /// </summary>
        /// <param name="curStage"></param>
        /// <param name="newStageStatus"></param>
        /// <param name="funcName">由哪個method調用,以便Trace</param>
        /// <param name="sendOutCstSeq">當狀態UDRQ or UDRQ_LDRQ時填入第一筆SendOut的CST Seq,如果不是則填String.empty</param>
        /// <param name="sendOutJobSeq">當狀態UDRQ or UDRQ_LDRQ時填入第一筆SendOut的JOB Seq,如果不是則填String.empty</param>
        /// <param name="sendOutCstSeq02">當狀態UDRQ or UDRQ_LDRQ時填入第二筆SendOut的CST Seq,如果不是則填String.empty</param>
        /// <param name="sendOutJobSeq02">當狀態UDRQ or UDRQ_LDRQ時填入第二筆SendOut的JOB Seq,如果不是則填String.empty</param>
        protected void UpdateStageStatus(RobotStage curStage, string newStageStatus, string funcName, string sendOutCstSeq, string sendOutJobSeq, string sendOutCstSeq02, string sendOutJobSeq02)
        {
            string strlog = string.Empty;

            try
            {

                #region [ Stage Status Change才需要Update ]

                if (curStage.File.CurStageStatus != newStageStatus)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) status change from ({4}) to ({5}), UDRQ 1st Job CassetteSequenceNo({6}) JobSequenceNo({7}), 2nd Job CassetteSequenceNo({8}) JobSequenceNo({9}) by [{10}].",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                    curStage.Data.STAGENAME, curStage.File.CurStageStatus, newStageStatus, sendOutCstSeq,
                                                    sendOutJobSeq, sendOutCstSeq02, sendOutJobSeq02, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //add for Log Quick Trace
                    strlog = string.Format("[{0}] {1} - {2}({3}) Stage Status Change From({4}) to ({5}) ,sendOut 1st Job({6},{7}), 2nd Job({8},{9}) by [{10}]",
                                            "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                            MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                            curStage.Data.STAGENAME,
                                            curStage.Data.STAGEID, curStage.File.CurStageStatus, newStageStatus, sendOutCstSeq,
                                            sendOutJobSeq, sendOutCstSeq02, sendOutJobSeq02, funcName);

                    Logger.LogTrxWrite(this.LogName, strlog);

                    #region [ Update Robot Stage Entity ]

                    lock (curStage.File)
                    {
                        curStage.File.CurStageStatus = newStageStatus;
                        curStage.File.CurSendOut_CSTSeq = sendOutCstSeq;
                        curStage.File.CurSendOut_JobSeq = sendOutJobSeq;
                        curStage.File.CurSendOut_CSTSeq02 = sendOutCstSeq02;
                        curStage.File.CurSendOut_JobSeq02 = sendOutJobSeq02;
                        curStage.File.StatusChangeFlag = true;
                    }

                    #endregion

                }

                #endregion

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {

                    //Get Current Stage Info To Log
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) status is ({4}) , UDRQ Job CassetteSequenceNo({5}) JobSequenceNo({6}) by [{7}].",
                                            curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                            curStage.Data.STAGENAME, newStageStatus, sendOutCstSeq, sendOutJobSeq, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> 更新Stage LDRQ Status and LDRQ Cassette Status Priority for [ GetGetPutPut ] Use
        ///
        /// </summary>
        /// <param name="curStage"></param>
        /// <param name="newUDRQStageStatus"></param>
        /// <param name="funcName"></param>
        /// <param name="sendOutCstSeq"></param>
        /// <param name="sendOutJobSeq"></param>
        /// <param name="sendOutCstSeq02"></param>
        /// <param name="sendOutJobSeq02"></param>
        protected void UpdateStage_UDRQ_Status(RobotStage curStage, string newUDRQStageStatus, string funcName, string sendOutCstSeq, string sendOutJobSeq,
                                                 string sendOutCstSeq02, string sendOutJobSeq02)
        {

            string strlog = string.Empty;

            try
            {

                #region [ Stage Status Change才需要Update ]

                if (curStage.File.Stage_UDRQ_Status != newUDRQStageStatus ||
                    curStage.File.CurSendOut_CSTSeq != sendOutCstSeq ||
                    curStage.File.CurSendOut_JobSeq != sendOutJobSeq ||
                    curStage.File.CurSendOut_CSTSeq02 != sendOutCstSeq02 ||
                    curStage.File.CurSendOut_JobSeq02 != sendOutJobSeq02)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) UDRQ status change from ({4}) to ({5}) , UDRQ Job CassetteSequenceNo({6}) JobSequenceNo({7}),  Job02 CassetteSequenceNo({8}) JobSequenceNo({9}) by [{10}].",
                                                    curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                    curStage.Data.STAGENAME, curStage.File.Stage_UDRQ_Status, newUDRQStageStatus, sendOutCstSeq,
                                                    sendOutJobSeq, sendOutCstSeq02, sendOutJobSeq02, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //add for Log Quick Trace
                    strlog = string.Format("[{0}] {1} - {2}({3}) Stage UDRQ Status Change From({4}) to ({5}) ,sendOut Job({6},{7}), Job02({8},{9}) by [{10}]",
                                            "RobotSelectJobService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                            MethodBase.GetCurrentMethod().Name.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                            curStage.Data.STAGENAME,
                                            curStage.Data.STAGEID, curStage.File.Stage_UDRQ_Status, newUDRQStageStatus, sendOutCstSeq,
                                            sendOutJobSeq, sendOutCstSeq02, sendOutJobSeq02, funcName);

                    Logger.LogTrxWrite(this.LogName, strlog);

                    #region [ Update Robot Stage Entity ]

                    lock (curStage.File)
                    {
                        curStage.File.Stage_UDRQ_Status = newUDRQStageStatus;
                        curStage.File.CurSendOut_CSTSeq = sendOutCstSeq;
                        curStage.File.CurSendOut_JobSeq = sendOutJobSeq;
                        curStage.File.CurSendOut_CSTSeq02 = sendOutCstSeq02;
                        curStage.File.CurSendOut_JobSeq02 = sendOutJobSeq02;
                        curStage.File.StatusChangeFlag = true;
                    }

                    //Real Time Update不需要存入檔案以免造成IO過於頻繁

                    #endregion

                }

                #endregion

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {

                    //Get Current Stage Info To Log
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) StageName({3}) UDRQ status is ({4}) , UDRQ Job CassetteSequenceNo({5}) JobSequenceNo({6}), Job02 CassetteSequenceNo({7}) JobSequenceNo({8}) by [{9}].",
                                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                                curStage.Data.STAGENAME, newUDRQStageStatus, sendOutCstSeq, sendOutJobSeq,
                                                                sendOutCstSeq02, sendOutJobSeq02, funcName);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion  [ Update Stage Stutas Function List ] ==========================================================================================================================

        #region [ Add/Remove Job Check Fail Message List Function List =============================================================================================================

        /// <summary> 根據errCode新增Job內的Check Fail Message.False 表示有相同的Key 或是不啟用此功能 , 將不會記錄Log
        ///
        /// </summary>
        /// <param name="curBcsJob"></param>
        /// <param name="errCode"></param>
        protected void AddJobCheckFailMsg(Job curBcsJob, string errCode, string failMsg)
        {
            try
            {
                if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(errCode) == true)
                {
                    //有相同的Key則不需要新增
                    return;
                }

                lock (curBcsJob)
                {
                    curBcsJob.RobotWIP.CheckFailMessageList.Add(errCode, failMsg);
                }

            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }

        /// <summary> 根據errCode移除Job內的Check Fail Message
        ///
        /// </summary>
        /// <param name="curBcsJob"></param>
        /// <param name="errCode"></param>
        protected void RemoveJobCheckFailMsg(Job curBcsJob, string errCode)
        {
            try
            {

                if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(errCode) == false)
                {
                    //找不到有相同的Key則不需要移除
                    return;
                }

                lock (curBcsJob)
                {
                    curBcsJob.RobotWIP.CheckFailMessageList.Remove(errCode);
                }

            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// 删除当前Filter 所有errCode
        /// </summary>
        /// <param name="curBcsJob"></param>
        /// <param name="errCode"></param>
        protected void RemoveCurFilterAllJobCheckFailMsg(Job curBcsJob, string errCode)
        {
            try
            {

                if (curBcsJob.RobotWIP.CheckFailMessageList.Count <= 0)
                {
                    return;
                }

                lock (curBcsJob)
                {

                    Dictionary<string, string> DicCheckFailMessageList = new Dictionary<string, string>(curBcsJob.RobotWIP.CheckFailMessageList);

                    foreach (string failReturnCode in DicCheckFailMessageList.Keys)
                    {
                        if (failReturnCode.Contains(errCode))
                            curBcsJob.RobotWIP.CheckFailMessageList.Remove(failReturnCode);
                    }

                }

            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //20151031 add for 清除Job 所有CheckFailMsg
        /// <summary> 移除Job內所有的Check Fail Message .變更Step or Robot移動時需要更新
        /// 
        /// 
        /// </summary>
        /// <param name="curBcsJob"></param>
        protected void RemoveJobAllCheckFailMsg(Job curBcsJob)
        {
            try
            {

                if (curBcsJob.RobotWIP.CheckFailMessageList.Count == 0)
                {
                    //沒有Key則不需要清空
                    return;
                }

                lock (curBcsJob)
                {
                    curBcsJob.RobotWIP.CheckFailMessageList.Clear();
                }

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        #endregion [ Add/Remove Job Check Fail Message List Function List ==========================================================================================================

        #region [ Add/Remove Robot Check Fail Message List Function List =========================================================================================================

        /// <summary> 根據errCode新增Robot內的Check Fail Message.False 表示有相同的Key 或是不啟用此功能 , 將不會記錄Log
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="errCode"></param>
        /// <param name="failMsg"></param>
        protected void AddRobotCheckFailMsg(Robot curRobot, string errCode, string failMsg)
        {
            try
            {
                if (curRobot.CheckFailMessageList.ContainsKey(errCode) == true)
                {
                    //有相同的Key則不需要新增
                    return;
                }

                lock (curRobot)
                {
                    curRobot.CheckFailMessageList.Add(errCode, failMsg);
                }

            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }

        /// <summary> 根據errCode移除Robot內的Check Fail Message
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="errCode"></param>
        protected void RemoveRobotCheckFailMsg(Robot curRobot, string errCode)
        {
            try
            {

                if (curRobot.CheckFailMessageList.ContainsKey(errCode) == false)
                {
                    //找不到有相同的Key則不需要新增
                    return;
                }

                lock (curRobot)
                {
                    curRobot.CheckFailMessageList.Remove(errCode);
                }

            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> 移除Robot內所有的Check Fail Message
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        protected void RemoveRobotAllCheckFailMsg(Robot curRobot)
        {
            try
            {

                if (curRobot.CheckFailMessageList.Count == 0)
                {
                    //沒有Key則不需要清空
                    return;
                }

                lock (curRobot)
                {
                    curRobot.CheckFailMessageList.Clear();
                }

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        #endregion  [ Add/Remove Robot Check Fail Message List Function List ====================================================================================================

        #region Send PLC Data By Trx Object

        protected void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }
        #endregion

        /// <summary> 判斷Both,Loader Port 是否可以出片的Common Function
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curPort_Entity"></param>
        /// <returns></returns>
        protected bool CheckPortTypeStageUDRQ_CommonCondition(Robot curRobot, RobotStage curStage, out Port curPort_Entity)
        {

            Equipment stageEQP;
            string strlog = string.Empty;
            string errMsg = string.Empty;
            curPort_Entity = null;

            try
            {

                #region [ Check Stage 是否為STOP ]

                stageEQP = ObjectManager.EquipmentManager.GetEQP(curStage.Data.NODENO);

                if (stageEQP == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) RobotStage({2}) StageID({3})can not find EQP by EQPNo({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGENAME, curStage.Data.STAGEID,
                                                curStage.Data.NODENO);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }

                if (stageEQP.File.Status == eEQPStatus.STOP)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) RobotStage({2}) StageID({3}) EQP Status is (STOP)!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGENAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }

                #endregion

                #region [ 透過StageID取得Port Entity 找不到 Stage Status =NOREQ ]

                Port curPort = ObjectManager.PortManager.GetPort(curStage.Data.STAGEID);

                if (curPort == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) RobotStage({2}) StageID({3})can not get Port Entity!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGENAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //找不到Port Entity,則需更新Stage Status並紀錄Log
                    UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                    return false;
                }

                #endregion

                #region [ 判斷Port是否為Enable ]

                if (curPort.File.EnableMode != ePortEnableMode.Enabled)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Port EnableMode is ({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curPort.File.EnableMode);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //Port Disable時則需更新Stage Status並紀錄Log
                    UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);

                    return false;
                }

                #endregion

                #region [ 判斷Port是否為Normal不為Down ]
                if (curPort.File.DownStatus != ePortDown.Normal)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Port DownStatus is ({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curPort.File.DownStatus);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //Port Down時則需更新Stage Status並紀錄Log
                    UpdateStageStatus(curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                    return false;

                }
                #endregion

                curPort_Entity = curPort;

                return true;

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        #region [ 將DB設定值轉換成Robot Command 相對應數值 Function List ]

        /// <summary> 將設定的值轉換成Robot Command中Command Action對應的int數值.
        /// 
        /// </summary>
        /// <param name="settingValue"></param>
        /// <returns></returns>
        protected int GetRobotCommandActionCode(Robot curRobot, Job curBcsJob, string settingValue)
        {
            int transferCmdKey = 0;
            string strlog = string.Empty;

            //SPEC定義[ Wait_Proc_00028 ] 1Arm 2Job要額外處理
            //0: None      //1: Put          //2: Get
            //4: Exchange  //8: Put Ready    //16: Get Ready       //32: Get/Put
            try
            {
                switch (settingValue)
                {

                    case eRobot_DB_CommandAction.ACTION_PUT:

                        transferCmdKey = 1;
                        break;

                    case eRobot_DB_CommandAction.ACTION_GET:

                        transferCmdKey = 2;
                        break;

                    case eRobot_DB_CommandAction.ACTION_EXCHANGE:

                        transferCmdKey = 4;
                        break;

                    case eRobot_DB_CommandAction.ACTION_PUTREADY:

                        transferCmdKey = 8;
                        break;
                    case eRobot_DB_CommandAction.ACTION_GETREADY:

                        transferCmdKey = 16;
                        break;

                    case eRobot_DB_CommandAction.ACTION_GETPUT:

                        transferCmdKey = 32;
                        break;

                    default:

                        transferCmdKey = 0;

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) RouteStep DB Setting RobotAction({4}) is out of range!",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                    settingValue);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        break;
                }

                //int.TryParse(settingValue, out transferCmdKey);

                //如有特殊狀況在此更新

                return transferCmdKey;

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return transferCmdKey;
            }

        }

        /// <summary> 將設定的值轉換成Robot Command中Select Arm對應的int數值
        /// 
        /// </summary>
        /// <param name="settingValue"></param>
        /// <returns></returns>
        protected int GetRobotUseArmCode(Robot curRobot, Job curBcsJob, string settingValue)
        {
            int transferUseArmKey = 0;
            string strlog = string.Empty;

            //SPEC定義
            //0: None               //1: Upper/Left Arm    //2: Lower/Left Arm   //3: Left Both Arm 
            //4: Upper/Right Arm    //8: Lower/Right Arm   //12: Right Both Arm
            try
            {
                //20160105 add 1Arm2Job
                if (curRobot.Data.ARMJOBQTY != 2)
                {

                    #region [ Normal 1Arm1Job ]

                    switch (settingValue)
                    {

                        case eDBRobotUseArmCode.UPPER_ARM:

                            transferUseArmKey = 1;
                            break;

                        case eDBRobotUseArmCode.LOWER_ARM:

                            transferUseArmKey = 2;
                            break;

                        case eDBRobotUseArmCode.ALL_ARM:

                            transferUseArmKey = 3;
                            break;

                        default:

                            transferUseArmKey = 0;

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) RouteStep DB Setting USEArm({4}) is out of range!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                        settingValue);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            break;
                    }

                    #endregion

                }
                else
                {

                    #region [ Cell Special 1Arm2Job ]

                    switch (settingValue)
                    {

                        case eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01:   //1: Upper/Left Arm

                            transferUseArmKey = 1;
                            break;

                        case eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02:   //2: Lower/Left Arm 

                            transferUseArmKey = 2;
                            break;

                        case eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03:  //4: Upper/Right Arm 

                            transferUseArmKey = 4;
                            break;

                        case eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04:  //8: Lower/Right Arm

                            transferUseArmKey = 8;
                            break;

                        case eDBRobotUseArmCode.CELL_SPECIAL_BOTH_LEFT:   //3: Left Both Arm  

                            transferUseArmKey = 3;
                            break;

                        //20160127 add UpperBoth
                        case eDBRobotUseArmCode.CELL_SPECIAL_BOTH_UPPER:   //5: Upper Both Arm 

                            transferUseArmKey = 5;
                            break;

                        //20160127 addLowerBoth
                        case eDBRobotUseArmCode.CELL_SPECIAL_BOTH_LOWER:   //10: Lower Both Arm

                            transferUseArmKey = 10;
                            break;

                        case eDBRobotUseArmCode.CELL_SPECIAL_BOTH_RIGHT:  //12: Right Both Arm

                            transferUseArmKey = 12;
                            break;

                        default:

                            transferUseArmKey = 0;

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) RouteStep DB Setting UseArm({4}) is out of range!",
                                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                        settingValue);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            break;
                    }

                    #endregion

                }

                //如有特殊狀況在此更新

                return transferUseArmKey;

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return transferUseArmKey;
            }

        }

        #endregion

        #region [ 將Command的數值轉換成說明 ]

        /// <summary> 將Command的Action數值轉換成說明
        /// 
        /// </summary>
        /// <param name="activeCode"></param>
        /// <returns></returns>
        protected string GetRobotCommandActionDesc(int activeCode)
        {

            string strlog = string.Empty;
            string tmpRtnCode = string.Empty;

            try
            {
                //SPEC定義[ Wait_Proc_00028 ] 1Arm 2Job要額外處理
                //0: None      //1: Put          //2: Get
                //4: Exchange  //8: Put Ready    //16: Get Ready       //32: Get/Put
                switch (activeCode)
                {
                    case 0:

                        tmpRtnCode = "NONE";
                        break;

                    case 1:

                        tmpRtnCode = eRobot_DB_CommandAction.ACTION_PUT;
                        break;

                    case 2:

                        tmpRtnCode = eRobot_DB_CommandAction.ACTION_GET;
                        break;


                    case 4:

                        tmpRtnCode = eRobot_DB_CommandAction.ACTION_EXCHANGE;
                        break;

                    case 8:

                        tmpRtnCode = eRobot_DB_CommandAction.ACTION_PUTREADY;
                        break;

                    case 16:

                        tmpRtnCode = eRobot_DB_CommandAction.ACTION_GETREADY;
                        break;

                    case 32:

                        tmpRtnCode = eRobot_DB_CommandAction.ACTION_GETPUT;
                        break;

                    //20151025 add Mulit-Get/Put 64: Multi-Put  128:Multi-Get
                    case 64:

                        tmpRtnCode = eRobot_DB_CommandAction.ACTION_MULTI_PUT;
                        break;

                    case 128:

                        tmpRtnCode = eRobot_DB_CommandAction.ACTION_MULTI_GET;
                        break;


                    //20151230 add RTC_PUT 256
                    case 256:

                        tmpRtnCode = eRobot_DB_CommandAction.ACTION_RTC_PUT;
                        break;

                    //20160511
                    case 512:

                        tmpRtnCode = eRobot_DB_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                        break;

                    //20160511
                    case 1024:

                        tmpRtnCode = eRobot_DB_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT;
                        break;

                    default:

                        tmpRtnCode = "UNKNOWN";

                        break;
                }

                return tmpRtnCode;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }

        }

        /// <summary> 將Command的UseArm數值轉換成說明 20160113 Modify
        /// 
        /// </summary>
        /// <param name="useArmCode"></param>
        /// <returns></returns>
        protected string GetRobotCommandUseArmDesc(int useArmCode, bool isCellSpecpial)
        {

            string strlog = string.Empty;
            string tmpRtnCode = string.Empty;

            try
            {
                //SPEC定義
                //0: None               //1: Upper/Left Arm    //2: Lower/Left Arm   //3: Left Both Arm 
                //4: Upper/Right Arm    //8: Lower/Right Arm   //12: Right Both Arm
                //20160127 add 5 and 10
                //5: Upper Both Arm 
                //10: Lower Both Arm

                if (isCellSpecpial == true)
                {
                    #region [ Cell Special Spec ]

                    switch (useArmCode)
                    {
                        case 0:

                            tmpRtnCode = "NONE";
                            break;

                        case 1:

                            tmpRtnCode = eCellSpecialRobotCmdArmSelectCode.UP_LEFT.ToString();
                            break;

                        case 2:

                            tmpRtnCode = eCellSpecialRobotCmdArmSelectCode.LOW_LEFT.ToString();
                            break;

                        case 3:

                            tmpRtnCode = eCellSpecialRobotCmdArmSelectCode.BOTH_LEFT.ToString();
                            break;

                        case 4:

                            tmpRtnCode = eCellSpecialRobotCmdArmSelectCode.UP_RIGHT.ToString();
                            break;

                        case 8:

                            tmpRtnCode = eCellSpecialRobotCmdArmSelectCode.LOW_RIGHT.ToString();
                            break;

                        case 12:

                            tmpRtnCode = eCellSpecialRobotCmdArmSelectCode.BOTH_RIGHT.ToString();
                            break;

                        //20160127 add 5 and 10
                        case 5:

                            tmpRtnCode = eCellSpecialRobotCmdArmSelectCode.UPBOTH.ToString();
                            break;

                        case 10:

                            tmpRtnCode = eCellSpecialRobotCmdArmSelectCode.LOWBOTH.ToString();
                            break;

                        default:

                            tmpRtnCode = "UNKNOWN";

                            break;
                    }

                    #endregion

                }
                else
                {
                    #region [ Normal Spec ]

                    switch (useArmCode)
                    {
                        case 0:

                            tmpRtnCode = "NONE";
                            break;

                        case 1:

                            tmpRtnCode = eRobotCmdArmSelectCode.UP.ToString();
                            break;

                        case 2:

                            tmpRtnCode = eRobotCmdArmSelectCode.LOW.ToString();
                            break;

                        case 3:

                            tmpRtnCode = eRobotCmdArmSelectCode.BOTH.ToString();
                            break;

                        case 4:

                            tmpRtnCode = eRobotCmdArmSelectCode.UP2.ToString();
                            break;

                        case 8:

                            tmpRtnCode = eRobotCmdArmSelectCode.LOW2.ToString();
                            break;

                        case 12:

                            tmpRtnCode = eRobotCmdArmSelectCode.BOTH2.ToString();
                            break;


                        default:

                            tmpRtnCode = "UNKNOWN";

                            break;
                    }

                    #endregion

                }

                return tmpRtnCode;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }

        }

        #endregion

        ///// <summary>
        ///// 檢查BC Server下的所有Line是否都切ChangerMode
        ///// </summary>
        ///// <returns>true:表示所有Line都切ChangerMode</returns>
        //protected bool IsAllLineChangerMode()
        //{
        //    List<Line> lines = ObjectManager.LineManager.GetLines();
        //    if (lines.Count <= 0) return false;

        //    bool ret = true;
        //    foreach (Line line in lines)
        //    {
        //        if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.CHANGER_MODE)
        //        {
        //            ret = false;
        //            break;
        //        }
        //    }
        //    return ret;
        //}

        /// <summary>
        /// 檢查BC Server下的所有Line Indexer是否都是相同Mode
        /// </summary>
        /// <returns>true:表示所有Line都是相同Indexer Mode</returns>
        protected bool IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE IndexOperMode)
        {
            List<Line> lines = ObjectManager.LineManager.GetLines();
            if (lines.Count <= 0) return false;

            bool ret = true;
            foreach (Line line in lines)
            {
                if (line.File.IndexOperMode != IndexOperMode)
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }

        protected SLOTPLAN GetSlotPlanByJob(IList<SLOTPLAN> SlotPlans, Job curBcsJob)
        {
            if (SlotPlans == null || SlotPlans.Count == 0 || curBcsJob == null)
                return null;

            SLOTPLAN ret = null;
            foreach (SLOTPLAN slot_plan in SlotPlans)
            {
                if (curBcsJob.FromCstID == slot_plan.SOURCE_CASSETTE_ID)
                {
                    if ((slot_plan.SLOTNO > 0 && curBcsJob.FromSlotNo == slot_plan.SLOTNO.ToString()) ||
                        (slot_plan.SLOTNO <= 0 && slot_plan.PRODUCT_NAME == curBcsJob.MesProduct.PRODUCTNAME))
                    //(slot_plan.SLOTNO <= 0 && slot_plan.PRODUCT_NAME == curBcsJob.GlassChipMaskCutID))
                    {
                        ret = slot_plan;
                        break;
                    }
                }
            }
            return ret;
        }

        #region [[ CHECK function for Pre-Fetch ]]
        /// <summary> 检查是不是有启用 预取 功能 (by RobotStage)
        /// 
        /// </summary>
        /// <param name="_robotContext"></param>
        /// <returns>true=有启用预取, false=关闭预取</returns>
        protected bool CheckPrefetchFlag(IRobotContext _robotContext)
        {
            bool _flag = false;
            Robot _robot = null;
            Job _job = null;
            RobotStage _stage = null;

            try
            {
                if (_robotContext.ContainsKey(eRobotContextParameter.CanUsePreFetchFlag))
                {
                    _flag = (_robotContext[eRobotContextParameter.CanUsePreFetchFlag].ToString() == "Y" ? true : false);
                }
                else
                {
                    _robot = (Robot)_robotContext[eRobotContextParameter.CurRobotEntity];
                    if (_robot == null) return _flag;

                    _job = (Job)_robotContext[eRobotContextParameter.CurJobEntity];
                    if (_job == null) return _flag;

                    string _stageId = _job.RobotWIP.CurLocation_StageID;
                    if (_stageId == string.Empty) return _flag;
                    if (_stageId == eRobotCommonConst.ROBOT_HOME_STAGEID) return _flag;

                    _stage = (RobotStage)ObjectManager.RobotStageManager.GetRobotStagebyStageID(_stageId);
                    if (_stage == null)
                    {
                        _flag = CheckPrefetchFlag(_robot);
                    }
                    else
                    {
                        _flag = CheckPrefetchFlag(_robot, _stage);
                    }
                }
            }
            catch
            {
                _flag = false;
            }

            return _flag;
        }
        /// <summary> 检查是不是有启用 预取 功能 (by RobotStage)
        /// 
        /// </summary>
        /// <param name="_curRobot"></param>
        /// <returns>true=有启用预取, false=关闭预取</returns>
        protected bool CheckPrefetchFlag(Robot _curRobot)
        {
            bool _flag = false;

            try
            {
                if (_curRobot == null) return _flag;
                if (!_curRobot.Context.ContainsKey(eRobotContextParameter.CanUsePreFetchFlag)) return _flag;

                _flag = (_curRobot.Context[eRobotContextParameter.CanUsePreFetchFlag].ToString() == "Y" ? true : false);
            }
            catch
            {
                _flag = false;
            }

            return _flag;
        }
        /// <summary> 检查是不是有启用 预取 功能 (by RobotStage)
        /// 
        /// </summary>
        /// <param name="_curRobot"></param>
        /// <param name="_curStage"></param>
        /// <returns>true=有启用预取, false=关闭预取</returns>
        protected bool CheckPrefetchFlag(Robot _curRobot, RobotStage _curStage)
        {
            bool _flag = false;

            try
            {
                if (_curRobot == null) return _flag;

                if (_curStage == null) return _flag;
                //20160624
                //if(_curStage.Data.STAGETYPE != eRobotStageType.PORT) return _flag;
                _curRobot.Context.AddParameter(eRobotContextParameter.CanUsePreFetchFlag, _curStage.Data.PREFETCHFLAG.ToString().ToUpper());

                if (!_curRobot.Context.ContainsKey(eRobotContextParameter.CanUsePreFetchFlag)) return _flag;

                _flag = (_curRobot.Context[eRobotContextParameter.CanUsePreFetchFlag].ToString() == "Y" ? true : false);
            }
            catch
            {
                _flag = false;
            }

            return _flag;
        }
        #endregion

        #region [[ CHECK functions (ByPass/Jump/Filter) for Job ]]
        /// <summary>根據RouteStepByPass條件判斷特定StepNo是否出現變化
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkStepNo"></param>
        /// <param name="curStageSelectInfo"></param>
        /// <param name="curBeforeFilterStageList"></param>
        /// <param name="_is2ndCmdFlag">false=1st Command, true=2nd Command</param>
        /// <param name="_robotContext">null=use local object variable, _robotContext=refer the external object variable!!</param>
        /// <returns></returns>
        protected bool CheckAllRouteStepByPassCondition2(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList)
        {
            IRobotContext _temp = null;
            return CheckAllRouteStepByPassCondition2(curRobot, curBcsJob, checkStepNo, ref curBeforeFilterStageList, false, ref _temp);
        }
        protected bool CheckAllRouteStepByPassCondition2(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList, bool _is2ndCmdFlag)
        {
            IRobotContext _temp = null;
            return CheckAllRouteStepByPassCondition2(curRobot, curBcsJob, checkStepNo, ref curBeforeFilterStageList, _is2ndCmdFlag, ref _temp);
        }
        protected bool CheckAllRouteStepByPassCondition2(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList, ref IRobotContext _robotContext)
        {
            return CheckAllRouteStepByPassCondition2(curRobot, curBcsJob, checkStepNo, ref curBeforeFilterStageList, false, ref _robotContext);
        }
        protected bool CheckAllRouteStepByPassCondition2(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList, bool _is2ndCmdFlag, ref IRobotContext _robotContext)
        {
            IRobotContext robotConText = null;
            string fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;
            List<RobotStage> curCanUseStageList = new List<RobotStage>();
            string _szJobLog = string.Empty;

            try
            {
                #region [ Job Log ]
                string _armSubLocation = string.Empty;
                switch (curBcsJob.RobotWIP.CurSubLocation)
                {
                    case eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION:
                        _armSubLocation = "SlotBlockInfo Front";
                        break;
                    case eRobotCommonConst.ROBOT_ARM_BACK_LOCATION:
                        _armSubLocation = "SlotBlockInfo Back";
                        break;
                    default: break;
                }
                _szJobLog = string.Format("{0}Job Entity[CST={1}, Slot={2}]", _armSubLocation + " ", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString());
                #endregion

                List<RobotRuleRouteStepByPass> curRouteStepByPassList = new List<RobotRuleRouteStepByPass>();

                curRouteStepByPassList = ObjectManager.RobotManager.GetRuleRouteStepByPass(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, checkStepNo);

                int ruleCount = 0;

                if (curRouteStepByPassList != null) ruleCount = curRouteStepByPassList.Count;

                #region[DebugLog][ Start Job All RouteStepByPass Function ]
                if (IsShowDetialLog)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} curStepNo({3}) nextStepNo({4}) {5} Rule Job RouteStepByPass ListCount({6}) Start {7}",
                        curRobot.Data.NODENO,
                        curRobot.Data.ROBOTNAME,
                        _szJobLog,
                        curBcsJob.RobotWIP.CurStepNo.ToString(),
                        curBcsJob.RobotWIP.NextStepNo.ToString(),
                        (!_is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc),
                        ruleCount.ToString(),
                        new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion

                #region [Check CurStep All RouteStepByPass Condition ]
                //正常是 new RobotContext 物件, 但是如果从外面带进来的 _robotConext 物件不为空时!!
                //则是要再去判断是不是 有包含 SlotBlockInfoEntity 的物件!? 如果没有, 则是一样 new RobotContext 物件!!
                //如果有包含, 则是直接指向 _robotContext 物件!! 20160108-002-dd
                robotConText = (_robotContext == null ? new RobotContext() : _robotContext);

                #region [ Initial RouteStepByPass Rule List RobotConText Info. 搭配針對File Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);

                //1st Cmd is2ndCmdFlag is false, otherwise true (for 2nd Cmd)
                robotConText.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, _is2ndCmdFlag);

                #region  [ RouteStepByPass前先預設目前Step都是符合條件的 ]

                //增加防呆
                if (!curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(checkStepNo))
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) {7} Rule Job RouteStepByPass Current StepNo({4}) but the Job Route max StepNo is {5} End{6}",
                            curRobot.Data.NODENO,
                            curRobot.Data.ROBOTNAME,
                            curBcsJob.CassetteSequenceNo,
                            curBcsJob.JobSequenceNo,
                            checkStepNo.ToString(),
                            curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString(),
                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH),
                            (!_is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    return false;
                }

                string[] curStepCanUseStageList = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST.Split(',');

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

                    if (curCanUseStageList.Contains(curStage) == false)
                    {

                        curCanUseStageList.Add(curStage);

                    }

                    #endregion

                }

                #endregion

                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curCanUseStageList);

                //if (_robotContext != null)
                //{
                //    _robotContext.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, _is2ndCmdFlag);
                //    _robotContext.AddParameter(eRobotContextParameter.StepCanUseStageList, curCanUseStageList);
                //}

                #endregion =======================================================================================================================================================

                #region [ 如果沒有任何StepByPass則直接回覆True ]
                if (curRouteStepByPassList == null)
                {
                    #region[DebugLog][ Start Job All RouteStepByPass Function ]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} {3} Rule Job RouteStepByPass ListCount({4}) End {5}",
                            curRobot.Data.NODENO,
                            curRobot.Data.ROBOTNAME,
                            _szJobLog,
                            ruleCount.ToString(),
                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH),
                            (!_is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    //取得Stage Selct後的Can Use Stages List
                    //curBeforeFilterStageList = (List<RobotStage>)(_robotContext != null ? _robotContext[eRobotContextParameter.StepCanUseStageList] : robotConText[eRobotContextParameter.StepCanUseStageList]);
                    curBeforeFilterStageList = (List<RobotStage>)(robotConText[eRobotContextParameter.StepCanUseStageList]);

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
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepByPass object({3}) MethodName({4}) IsEnable({5}) Start {6}",
                            curRobot.Data.NODENO,
                            curRobot.Data.ROBOTNAME,
                            _szJobLog,
                            curRouteStepByPassCondition.Data.OBJECTNAME,
                            curRouteStepByPassCondition.Data.METHODNAME,
                            curRouteStepByPassCondition.Data.ISENABLED,
                            new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    if (curRouteStepByPassCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                    {
                        //將ByPass後的GOTOSTEPID
                        robotConText.AddParameter(eRobotContextParameter.RouteStepByPassGotoStepNo, curRouteStepByPassCondition.Data.GOTOSTEPID);

                        checkFlag = (bool)Invoke(curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, new object[] { robotConText });

                        if (!checkFlag)
                        {
                            #region[DebugLog][ End Rule Job RouteStepByPass Function ]
                            if (IsShowDetialLog)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepByPass Fail, object({3}) MethodName({4}) RtnCode({5}) RtnMsg({6}]!",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    curRouteStepByPassCondition.Data.OBJECTNAME,
                                    curRouteStepByPassCondition.Data.METHODNAME,
                                    robotConText.GetReturnCode(),
                                    robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepByPass object({3}) MethodName({4}) IsEnable({5}) End {6}",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    curRouteStepByPassCondition.Data.OBJECTNAME,
                                    curRouteStepByPassCondition.Data.METHODNAME,
                                    curRouteStepByPassCondition.Data.ISENABLED,
                                    new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0005 ]
                            if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepByPass Fail, object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6}]!",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    curRouteStepByPassCondition.Data.OBJECTNAME,
                                    curRouteStepByPassCondition.Data.METHODNAME,
                                    robotConText.GetReturnCode(),
                                    robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                                //failMsg = string.Format("Robot({0}) {1} object({2}) MethodName({3}) RtnCode({4})  RtnMsg({5})!", 
                                //    curRobot.Data.ROBOTNAME, 
                                //    _szJobLog,
                                //    curRouteStepByPassCondition.Data.OBJECTNAME, 
                                //    curRouteStepByPassCondition.Data.METHODNAME, 
                                //    robotConText.GetReturnCode(), 
                                //    robotConText.GetReturnMessage());

                                failMsg = string.Format("RtnCode({0})  RtnMsg({1})",
                                     robotConText.GetReturnCode(),
                                     robotConText.GetReturnMessage());

                                AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                #endregion
                            }
                            #endregion

                            #region[DebugLog][ End Job All StageSelct Function ]
                            if (IsShowDetialLog)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} {3} Rule Job RouteStepByPass ListCount({4}) End {5}",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    (!_is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc),
                                    ruleCount.ToString(),
                                    new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            //有重大異常直接結束RouteStepByPass邏輯回復NG
                            return false;

                        }
                        else
                        {
                            //Clear[ Robot_Fail_Case_E0004 ]
                            RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                            #region[DebugLog][ End Rule Job RouteStepByPass Function ]
                            if (IsShowDetialLog)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepByPass object({3}) MethodName({4}) IsEnable({5}) End {6}",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    curRouteStepByPassCondition.Data.OBJECTNAME,
                                    curRouteStepByPassCondition.Data.METHODNAME,
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
                        if (IsShowDetialLog)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepByPass object({3}) MethodName({4}) IsEnable({5}) End {6}",
                                curRobot.Data.NODENO,
                                curRobot.Data.ROBOTNAME,
                                _szJobLog,
                                curRouteStepByPassCondition.Data.OBJECTNAME,
                                curRouteStepByPassCondition.Data.METHODNAME,
                                curRouteStepByPassCondition.Data.ISENABLED,
                                new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                    }
                }
                #endregion

                #endregion

                #region[DebugLog][ Start Job All RouteStepByPass Function ]
                if (IsShowDetialLog)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} {3} Rule Job RouteStepByPass ListCount({4}) End {5}",
                        curRobot.Data.NODENO,
                        curRobot.Data.ROBOTNAME,
                        _szJobLog,
                        (!_is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc),
                        ruleCount.ToString(),
                        new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion

                //取得Stage Selct後的Can Use Stages List
                curBeforeFilterStageList = (List<RobotStage>)(_robotContext != null ? _robotContext[eRobotContextParameter.StepCanUseStageList] : robotConText[eRobotContextParameter.StepCanUseStageList]);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>根據RouteStepJump條件判斷特定StepNo是否出現變化
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkStepNo"></param>
        /// <param name="curBeforeFilterStageList"></param>
        /// <param name="_is2ndCmdFlag">false=1st Command, true=2nd Command</param>
        /// <param name="_robotContext">null=use local object variable, _robotContext=refer the external object variable!!</param>
        /// <returns></returns>
        protected bool CheckAllRouteStepJumpCondition2(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList)
        {
            IRobotContext _temp = null;
            return CheckAllRouteStepJumpCondition2(curRobot, curBcsJob, checkStepNo, ref curBeforeFilterStageList, false, ref _temp);
        }
        protected bool CheckAllRouteStepJumpCondition2(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList, bool _is2ndCmdFlag)
        {
            IRobotContext _temp = null;
            return CheckAllRouteStepJumpCondition2(curRobot, curBcsJob, checkStepNo, ref curBeforeFilterStageList, _is2ndCmdFlag, ref _temp);
        }
        protected bool CheckAllRouteStepJumpCondition2(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList, ref IRobotContext _robotContext)
        {
            return CheckAllRouteStepJumpCondition2(curRobot, curBcsJob, checkStepNo, ref curBeforeFilterStageList, false, ref _robotContext);
        }
        protected bool CheckAllRouteStepJumpCondition2(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList, bool _is2ndCmdFlag, ref IRobotContext _robotContext)
        {
            IRobotContext robotConText = null;
            string fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;
            //List<RobotStage> curCanUseStageList = new List<RobotStage>();
            List<RobotRuleRouteStepJump> curRouteStepJumpList = new List<RobotRuleRouteStepJump>();
            string _szJobLog = string.Empty;

            try
            {
                #region [ Job Log ]
                string _armSubLocation = string.Empty;
                switch (curBcsJob.RobotWIP.CurSubLocation)
                {
                    case eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION:
                        _armSubLocation = "SlotBlockInfo Front";
                        break;
                    case eRobotCommonConst.ROBOT_ARM_BACK_LOCATION:
                        _armSubLocation = "SlotBlockInfo Back";
                        break;
                    default: break;
                }
                _szJobLog = string.Format("{0}Job Entity[CST={1}, Slot={2}]", _armSubLocation + " ", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString());
                #endregion

                curRouteStepJumpList = ObjectManager.RobotManager.GetRuleRouteStepJump(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, checkStepNo);

                int ruleCount = 0;

                if (curRouteStepJumpList != null) ruleCount = curRouteStepJumpList.Count;

                #region[DebugLog][ Start Job All RouteStepJump Function ]
                if (IsShowDetialLog)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} curStepNo({3}) nextStepNo({4}) {5} Rule Job RouteStepJump ListCount({6}) Start {7}",
                        curRobot.Data.NODENO,
                        curRobot.Data.ROBOTNAME,
                        _szJobLog,
                        curBcsJob.RobotWIP.CurStepNo.ToString(),
                        curBcsJob.RobotWIP.NextStepNo.ToString(),
                        (!_is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc),
                        ruleCount.ToString(),
                        new string(eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR_LENGTH));
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion

                #region [Check CurStep All RouteStepJump Condition ]
                //正常是 new RobotContext 物件, 但是如果从外面带进来的 _robotConext 物件不为空时!!
                //则是要再去判断是不是 有包含 SlotBlockInfoEntity 的物件!? 如果没有, 则是一样 new RobotContext 物件!!
                //如果有包含, 则是直接指向 _robotContext 物件!! 20160108-002-dd
                robotConText = (_robotContext == null ? new RobotContext() : _robotContext);

                #region [ Initial RouteStepJump Rule List RobotConText Info. 搭配針對File Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);

                //1st Cmd is2ndCmdFlag is false, otherwise true (for 2nd Cmd)
                robotConText.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, _is2ndCmdFlag);

                //拿RuleRouteStepByPass之後的StageIDList來做後續處理
                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curBeforeFilterStageList);

                //if (_robotContext != null)
                //{
                //    _robotContext.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, _is2ndCmdFlag);
                //    _robotContext.AddParameter(eRobotContextParameter.StepCanUseStageList, curBeforeFilterStageList);
                //}

                #endregion =======================================================================================================================================================

                #region [ 如果沒有任何StepJump則直接回覆True ]

                if (curRouteStepJumpList == null)
                {
                    #region[DebugLog][ Start Job All RouteStepJump Function ]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} {3} Rule Job RouteStepJump ListCount({4}) End {5}",
                            curRobot.Data.NODENO,
                            curRobot.Data.ROBOTNAME,
                            _szJobLog,

                            (!_is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc),
                            ruleCount.ToString(),
                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR_LENGTH));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    //取得RouteStepJump後的Can Use Stages List(沒Jump沿用傳入的CanUseStageList, 如果改變則要更新為新Step對應的CanUseStageList
                    //curBeforeFilterStageList = (List<RobotStage>)(_robotContext != null ? _robotContext[eRobotContextParameter.StepCanUseStageList] : robotConText[eRobotContextParameter.StepCanUseStageList]);

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
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepJump object({3}) MethodName({4}) IsEnable({5}) Start {6}",
                            curRobot.Data.NODENO,
                            curRobot.Data.ROBOTNAME,
                            _szJobLog,
                            curRouteStepJumpCondition.Data.OBJECTNAME,
                            curRouteStepJumpCondition.Data.METHODNAME,
                            curRouteStepJumpCondition.Data.ISENABLED,
                            new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    if (curRouteStepJumpCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                    {
                        //將Jump後的GOTOSTEPID送入處理
                        robotConText.AddParameter(eRobotContextParameter.RouteStepJumpGotoStepNo, curRouteStepJumpCondition.Data.GOTOSTEPID);

                        checkFlag = (bool)Invoke(curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, new object[] { robotConText });

                        if (!checkFlag)
                        {
                            #region[DebugLog][ End Rule Job RouteStepJump Function ]
                            if (IsShowDetialLog)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepJump Fail, object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6}]!",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    curRouteStepJumpCondition.Data.OBJECTNAME,
                                    curRouteStepJumpCondition.Data.METHODNAME,
                                    robotConText.GetReturnCode(),
                                    robotConText.GetReturnMessage());
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepJump object({3}) MethodName({4}) IsEnable({5}) End {6}",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    curRouteStepJumpCondition.Data.OBJECTNAME,
                                    curRouteStepJumpCondition.Data.METHODNAME,
                                    curRouteStepJumpCondition.Data.ISENABLED,
                                    new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0005 ]
                            if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepJump Fail, object({3}) MethodName({4}) RtnCode({5}) RtnMsg({6}]!",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    curRouteStepJumpCondition.Data.OBJECTNAME,
                                    curRouteStepJumpCondition.Data.METHODNAME,
                                    robotConText.GetReturnCode(),
                                    robotConText.GetReturnMessage());
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                                //failMsg = string.Format("Robot({0}) {1} object({2}) MethodName({3}) RtnCode({4}) RtnMsg({5})!",
                                //    curRobot.Data.ROBOTNAME,
                                //    _szJobLog,
                                //    curRouteStepJumpCondition.Data.OBJECTNAME,
                                //    curRouteStepJumpCondition.Data.METHODNAME,
                                //    robotConText.GetReturnCode(),
                                //    robotConText.GetReturnMessage());

                                failMsg = string.Format("RtnCode({0}) RtnMsg({1})!",
                                    robotConText.GetReturnCode(),
                                    robotConText.GetReturnMessage());

                                AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                #endregion
                            }
                            #endregion

                            #region[DebugLog][ End Job All StageSelct Function ]
                            if (IsShowDetialLog)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} {3} Rule Job RouteStepJump ListCount({4}) End {5}",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    (!_is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc),
                                    ruleCount.ToString(),
                                    new string(eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_END_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_END_CHAR_LENGTH));
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            //有重大異常直接結束RouteStepJump邏輯回復NG
                            return false;
                        }
                        else
                        {
                            //Clear[ Robot_Fail_Case_E0004 ]
                            RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                            #region[DebugLog][ End Rule Job RouteStepJump Function ]
                            if (IsShowDetialLog)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepJump object({3}) MethodName({4}) IsEnable({5}) End {6}",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    curRouteStepJumpCondition.Data.OBJECTNAME,
                                    curRouteStepJumpCondition.Data.METHODNAME,
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
                        if (IsShowDetialLog)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job RouteStepJump object({3}) MethodName({4}) IsEnable({5}) End {6}",
                                curRobot.Data.NODENO,
                                curRobot.Data.ROBOTNAME,
                                _szJobLog,
                                curRouteStepJumpCondition.Data.OBJECTNAME,
                                curRouteStepJumpCondition.Data.METHODNAME,
                                curRouteStepJumpCondition.Data.ISENABLED,
                                new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                    }
                }

                #endregion

                #endregion

                #region[DebugLog][ Start Job All RouteStepJump Function ]
                if (IsShowDetialLog)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} {3} Rule Job RouteStepJump ListCount({4}) End {5}",
                        curRobot.Data.NODENO,
                        curRobot.Data.ROBOTNAME,
                        _szJobLog,
                        (!_is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc),
                        ruleCount.ToString(),
                        new string(eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_END_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_END_CHAR_LENGTH));
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion

                //取得RouteStepJump後的Can Use Stages List(沒Jump沿用傳入的CanUseStageList, 如果改變則要更新為新Step對應的CanUseStageList
                //curBeforeFilterStageList = (List<RobotStage>)(_robotContext != null ? _robotContext[eRobotContextParameter.StepCanUseStageList] : robotConText[eRobotContextParameter.StepCanUseStageList]);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary> //判斷特定StepNo 所有的Filter條件是否成立.
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkStepNo or checkNextStepNo">Current Step No (1st Cmd) or Next Step No (2nd Cmd)</param>
        /// <param name="cur1stDefineCmd"></param>
        /// <param name="cur2ndDefindCmd"></param>
        /// <param name="curCanUseStageList"></param>
        /// <param name="_is2ndCmdFlag">false=1st Command, true=2nd Command</param>
        /// <param name="_robotContext">null=use local object variable, _robotContext=refer the external object variable!!</param>
        /// <returns></returns>
        protected bool CheckAllFilterConditionByStepNo2(Robot curRobot, Job curBcsJob, int checkStepNo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefineCmd, ref List<RobotStage> curFilterStageList)
        {
            IRobotContext _temp = null;
            return CheckAllFilterConditionByStepNo2(curRobot, curBcsJob, checkStepNo, cur1stDefineCmd, cur2ndDefineCmd, ref curFilterStageList, false, ref _temp);
        }
        protected bool CheckAllFilterConditionByStepNo2(Robot curRobot, Job curBcsJob, int checkStepNo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefineCmd, ref List<RobotStage> curFilterStageList, bool _is2ndCmdFlag)
        {
            IRobotContext _temp = null;
            return CheckAllFilterConditionByStepNo2(curRobot, curBcsJob, checkStepNo, cur1stDefineCmd, cur2ndDefineCmd, ref curFilterStageList, _is2ndCmdFlag, ref _temp);
        }
        protected bool CheckAllFilterConditionByStepNo2(Robot curRobot, Job curBcsJob, int checkStepNo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefineCmd, ref List<RobotStage> curFilterStageList, ref IRobotContext _robotContext)
        {
            return CheckAllFilterConditionByStepNo2(curRobot, curBcsJob, checkStepNo, cur1stDefineCmd, cur2ndDefineCmd, ref curFilterStageList, false, ref _robotContext);
        }
        protected bool CheckAllFilterConditionByStepNo2(Robot curRobot, Job curBcsJob, int checkStepNo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefineCmd, ref List<RobotStage> curFilterStageList, bool _is2ndCmdFlag, ref IRobotContext _robotContext)
        {
            IRobotContext robotConText = null;
            string fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;
            List<RobotStage> curLDRQStageList = new List<RobotStage>(); //20150831 add 
            List<RobotRuleFilter> curFilterList = new List<RobotRuleFilter>();
            string _szJobLog = string.Empty;

            try
            {
                #region [ Job Log ]
                string _armSubLocation = string.Empty;
                switch (curBcsJob.RobotWIP.CurSubLocation)
                {
                    case eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION:
                        _armSubLocation = "SlotBlockInfo Front";
                        break;
                    case eRobotCommonConst.ROBOT_ARM_BACK_LOCATION:
                        _armSubLocation = "SlotBlockInfo Back";
                        break;
                    default: break;
                }
                _szJobLog = string.Format("{0}Job Entity[CST={1}, Slot={2}]", _armSubLocation + " ", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString());
                #endregion

                #region [ Check CurStep All Filter Condition ]
                //正常是 new RobotContext 物件, 但是如果从外面带进来的 _robotConext 物件不为空时!!
                //则是要再去判断是不是 有包含 SlotBlockInfoEntity 的物件!? 如果没有, 则是一样 new RobotContext 物件!!
                //如果有包含, 则是直接指向 _robotContext 物件!! 20160108-002-dd
                robotConText = (_robotContext == null ? new RobotContext() : _robotContext);

                #region [ Initial Filter Rule List RobotConText Info. 搭配針對File Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =============

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);

                if (!_is2ndCmdFlag)
                {
                    //DB Spec define : 'PUT' / 'GET' / 'PUTREADY' / 'GETREADY'
                    cur1stDefineCmd.Cmd01_DBRobotAction = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION; //checkNextStepNo
                    //DB Spec define : 'UP':Upper Arm 'LOW':Lower Arm 'ANY':Any Arm 'ALL':All Arm
                    cur1stDefineCmd.Cmd01_DBUseArm = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTUSEARM; //checkNextStepNo 
                    cur1stDefineCmd.Cmd01_DBStageIDList = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST; //checkNextStepNo
                }
                else
                {
                    //DB Spec define : 'PUT' / 'GET' / 'PUTREADY' / 'GETREADY'
                    cur2ndDefineCmd.Cmd01_DBRobotAction = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION; //checkNextStepNo
                    //DB Spec define : 'UP':Upper Arm 'LOW':Lower Arm 'ANY':Any Arm 'ALL':All Arm
                    cur2ndDefineCmd.Cmd01_DBUseArm = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTUSEARM; //checkNextStepNo
                    cur2ndDefineCmd.Cmd01_DBStageIDList = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST; //checkNextStepNo
                }
                robotConText.AddParameter(eRobotContextParameter.Define_1stNormalRobotCommandInfo, cur1stDefineCmd);
                robotConText.AddParameter(eRobotContextParameter.Define_2ndNormalRobotCommandInfo, cur2ndDefineCmd);

                robotConText.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, _is2ndCmdFlag); //1st Cmd is2ndCmdFlag is false, otherwise true (for 2nd Cmd)

                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curFilterStageList);
                //1.加在step1 EX:STEP1 CHECK STEP2的stage可以提高预取的job和下游交换片的概率2.这种Filter加在step3可以防止Indexer和L3交换片玩后手臂上的job L4 不能收，EX：L3出的job提前Check Recipe，确保能送到L4
                if (!_is2ndCmdFlag && cur1stDefineCmd.Cmd01_DBRobotAction == eRobot_DB_CommandAction.ACTION_GET && curBcsJob.RobotWIP.CurLocation_StageID != eRobotCommonConst.ROBOT_HOME_STAGEID)
                {
                    robotConText.AddParameter(eRobotContextParameter.NextStepCanUseStageList, getNextStepCanUseStageList(curBcsJob,curRobot));
                }

                //if (_robotContext != null) //不为null, 并且没包含 SlotBlockInfoEntity 物件, 才需要再指派!!
                //{
                //    _robotContext.AddParameter(eRobotContextParameter.Define_1stNormalRobotCommandInfo, cur1stDefineCmd);
                //    _robotContext.AddParameter(eRobotContextParameter.Define_2ndNormalRobotCommandInfo, cur2ndDefineCmd);

                //    _robotContext.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, _is2ndCmdFlag); //1st Cmd is2ndCmdFlag is false, otherwise true (for 2nd Cmd)

                //    _robotContext.AddParameter(eRobotContextParameter.StepCanUseStageList, curFilterStageList);
                //}

                #endregion =======================================================================================================================================================

                //bool _skipFilterCheck = (!_is2ndCmdFlag ? false : CheckPrefetchFlag((_robotContext != null ? _robotContext : robotConText)));


                bool _skipFilterCheck = (!_is2ndCmdFlag ? false : CheckPrefetchFlag(curRobot));

                //20160727
                curBcsJob.RobotWIP.SkipFilterCheck = _skipFilterCheck;

                //20160802
                curBcsJob.RobotWIP.RunFilterCheckOK = false;

                if (_skipFilterCheck) //如果有Pre-Fetch, 直接bypass filter檢查!!
                {
                    #region Pre-Fetch Command & ArmSelect check
                    if (!_is2ndCmdFlag)
                    {
                        //switch (cur1stDefineCmd.Cmd01_DBRobotAction)
                        //{
                        //    case eRobot_DB_CommandAction.ACTION_PUT:
                        //        cur1stDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_PUT;
                        //        break;
                        //    case eRobot_DB_CommandAction.ACTION_MULTI_PUT:
                        //        cur1stDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTI_PUT;
                        //        break;
                        //    case eRobot_DB_CommandAction.ACTION_GETPUT:
                        //        cur1stDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_GETPUT;
                        //        break;
                        //    case eRobot_DB_CommandAction.ACTION_EXCHANGE:
                        //        cur1stDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_EXCHANGE;
                        //        break;
                        //    default:
                        //        cur1stDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE; //都沒有符合, 所以直接給 ACTION_NONE (0)
                        //        break;
                        //}
                        //cur1stDefineCmd.Cmd01_ArmSelect = cur1stDefineCmd.Cmd01_ArmSelect;
                    }
                    else
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
                            //20160511
                            case eRobot_DB_CommandAction.ACTION_RECIPEGROUPEND_PUT:
                                cur2ndDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                break;
                            case eRobot_DB_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT:
                                cur2ndDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT;
                                break;
                            default:
                                cur2ndDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE; //都沒有符合, 所以直接給 ACTION_NONE (0)
                                break;
                        }
                        cur2ndDefineCmd.Cmd01_ArmSelect = cur1stDefineCmd.Cmd01_ArmSelect;
                    }
                    #endregion
                }
                else
                {
                    curFilterList = ObjectManager.RobotManager.GetRuleFilter(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, checkStepNo); //checkNextStepNo

                    #region[DebugLog][ Start Job All Filter Function ]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} {3} Rule Job Filter ListCount({4}) Start {5}",
                            curRobot.Data.NODENO,
                            curRobot.Data.ROBOTNAME,
                            _szJobLog,
                            (!_is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc),
                            curFilterList.Count.ToString(),
                            new string(eRobotCommonConst.ALL_RULE_FILTER_START_CHAR, eRobotCommonConst.ALL_RULE_FILTER_START_CHAR_LENGTH));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    foreach (RobotRuleFilter curFilterCondition in curFilterList)
                    {
                        //Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_E0004 ] ,以Rule Job Filter 的ObjectName與MethodName為Key來決定是否紀錄Log!? 因為會出現同Job 確認不同Step所以FailCode要補上StepNo.
                        fail_ReasonCode = string.Format("{0}_{1}_{2}", curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, checkStepNo.ToString());

                        #region[DebugLog][ Start Rule Job Filter Function ]
                        if (IsShowDetialLog)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job Filter object({3}) MethodName({4}) IsEnable({5}) Start {6}",
                                curRobot.Data.NODENO,
                                curRobot.Data.ROBOTNAME,
                                _szJobLog,
                                curFilterCondition.Data.OBJECTNAME,
                                curFilterCondition.Data.METHODNAME,
                                curFilterCondition.Data.ISENABLED,
                                new string(eRobotCommonConst.RULE_FILTER_START_CHAR, eRobotCommonConst.RULE_FILTER_START_CHAR_LENGTH));
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        if (curFilterCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                        {
                            #region [ Check Enable Filter Function ]
                            checkFlag = (bool)Invoke(curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, new object[] { robotConText });

                            if (!checkFlag)
                            {
                                #region[DebugLog][ End Rule Job Filter Function ]
                                if (IsShowDetialLog)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job Filter Fail, object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6})!",
                                        curRobot.Data.NODENO,
                                        curRobot.Data.ROBOTNAME,
                                        _szJobLog,
                                        curFilterCondition.Data.OBJECTNAME,
                                        curFilterCondition.Data.METHODNAME,
                                        robotConText.GetReturnCode(),
                                        robotConText.GetReturnMessage());
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job Filter object({3}) MethodName({4}) IsEnable({5}) End {6}",
                                        curRobot.Data.NODENO,
                                        curRobot.Data.ROBOTNAME,
                                        _szJobLog,
                                        curFilterCondition.Data.OBJECTNAME,
                                        curFilterCondition.Data.METHODNAME,
                                        curFilterCondition.Data.ISENABLED,
                                        new string(eRobotCommonConst.RULE_FILTER_END_CHAR, eRobotCommonConst.RULE_FILTER_END_CHAR_LENGTH));
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                                fail_ReasonCode += "_" + robotConText.GetReturnCode();
                                #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0004 ]
                                if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job Filter Fail, object({3}) MethodName({4}) RtnCode({5}) RtnMsg({6})!",
                                        curRobot.Data.NODENO,
                                        curRobot.Data.ROBOTNAME,
                                        _szJobLog,
                                        curFilterCondition.Data.OBJECTNAME,
                                        curFilterCondition.Data.METHODNAME,
                                        robotConText.GetReturnCode(),
                                        robotConText.GetReturnMessage());
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                                    //failMsg = string.Format("Robot({0}) {1} object({2}) MethodName({3}) RtnCode({4}) RtnMsg({5})!",
                                    //    curRobot.Data.ROBOTNAME,
                                    //    _szJobLog,
                                    //    curFilterCondition.Data.OBJECTNAME,
                                    //    curFilterCondition.Data.METHODNAME,
                                    //    robotConText.GetReturnCode(),
                                    //    robotConText.GetReturnMessage());

                                    failMsg = string.Format("RtnCode({0}) RtnMsg({1})!",
                                        robotConText.GetReturnCode(),
                                        robotConText.GetReturnMessage());

                                    //curBcsJob.RobotWIP.CheckFailMessageList.Clear();
                                    AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                    #endregion
                                }

                                #endregion

                                #region[DebugLog][ Start Job All Filter Function ]

                                if (IsShowDetialLog)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Check 1st Command Rule Job Filter ListCount({3}) End {4}",
                                        curRobot.Data.NODENO,
                                        curRobot.Data.ROBOTNAME,
                                        _szJobLog,
                                        curFilterList.Count.ToString(),
                                        new string(eRobotCommonConst.ALL_RULE_SELECT_END_CHAR, eRobotCommonConst.ALL_RULE_FILTER_END_CHAR_LENGTH));
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                return false; //有重大異常直接結束filter邏輯回復NG
                            }
                            else
                            {
                                RemoveCurFilterAllJobCheckFailMsg(curBcsJob, fail_ReasonCode); //Clear[ Robot_Fail_Case_E0004 ]

                                #region[DebugLog][ End Rule Job Filter Function ]
                                if (IsShowDetialLog)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job Filter object({3}) MethodName({4}) IsEnable({5}) End {6}",
                                        curRobot.Data.NODENO,
                                        curRobot.Data.ROBOTNAME,
                                        _szJobLog,
                                        curFilterCondition.Data.OBJECTNAME,
                                        curFilterCondition.Data.METHODNAME,
                                        curFilterCondition.Data.ISENABLED,
                                        new string(eRobotCommonConst.RULE_FILTER_END_CHAR, eRobotCommonConst.RULE_FILTER_END_CHAR_LENGTH));
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                            }
                            #endregion
                        }
                        else
                        {
                            #region[DebugLog][ End Rule Job Filter Function ]
                            if (IsShowDetialLog)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} Rule Job Filter object({3}) MethodName({4}) IsEnable({5}) End {6}",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    curFilterCondition.Data.OBJECTNAME,
                                    curFilterCondition.Data.METHODNAME,
                                    curFilterCondition.Data.ISENABLED,
                                    new string(eRobotCommonConst.RULE_FILTER_END_CHAR, eRobotCommonConst.RULE_FILTER_END_CHAR_LENGTH));
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                    }
                    //20160802
                    curBcsJob.RobotWIP.RunFilterCheckOK = true;
                }

                #region[DebugLog][ Start Job All Filter Function ]
                if (IsShowDetialLog)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2} {3} Rule Job Filter ListCount({4}) End {5}",
                        curRobot.Data.NODENO,
                        curRobot.Data.ROBOTNAME,
                        _szJobLog,
                        (!_is2ndCmdFlag ? eRobotCommonConst.LOG_Check_1stCmd_Desc : eRobotCommonConst.LOG_Check_2ndCmd_Desc),
                        curFilterList.Count.ToString(),
                        new string(eRobotCommonConst.ALL_RULE_FILTER_END_CHAR, eRobotCommonConst.ALL_RULE_FILTER_END_CHAR_LENGTH));
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion
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
        #endregion
        private List<RobotStage> getNextStepCanUseStageList(Job curBcsJob, Robot curRobot)
        {

            try
            {
                string[] curNextStepCanUseStages = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo].Data.STAGEIDLIST.Split(',');
                List<RobotStage> curNextStepCanUseStageList = new List<RobotStage>();
                for (int i = 0; i < curNextStepCanUseStages.Length; i++)
                {

                    #region [ Check Stage is Exist ]

                    RobotStage curStage;

                    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curNextStepCanUseStages[i]);

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            string strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                                                      curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curNextStepCanUseStages[i]);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion


                    }

                    if (curNextStepCanUseStageList.Contains(curStage) == false)
                    {

                        curNextStepCanUseStageList.Add(curStage);

                    }

                    #endregion

                }
                return curNextStepCanUseStageList;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return new List<RobotStage>();
            }
        }
        //20160118 add Check SlotBlock Front/Back Job RouteID , CurStepID, NextStepID must Match for Cell Special Arm 20160216 add 新增比對Front/Back Grade如果不同則僅能去MIX Grade Unload
        /// <summary>確認SlotBlockInfo 內Front/Back 的RouteID, CurStepID, NextStepID 必須相同. 20160216 add 新增比對Front/Back Grade如果不同則僅能去MIX Grade Unload
        /// 
        /// </summary>
        /// <param name="curRobotStageSlotBlockInfo"></param>
        /// <returns></returns>
        protected bool CheckSlotBlockInfoJobRouteCondition(Robot curRobot, RobotCanControlSlotBlockInfo curRobotStageSlotBlockInfo, string funcName)
        {

            Job frontJob = null;
            Job backJob = null;
            string strlog = string.Empty;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;

            try
            {

                if (curRobotStageSlotBlockInfo.CurBlockCanControlJobList.Count < 2)
                {
                    //20160216 add for Front Back Job Grade不同時要On Only To MixGrade Unload Flag
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo curStageID({3}) cmdSlotNo({4}) curStepID({5}) only One Job and Set Block OnlyToMixGradeULDFlag (Off)!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curRobotStageSlotBlockInfo.CurBlock_Location_StageID,
                                                curRobotStageSlotBlockInfo.CurBlock_RobotCmdSlotNo, curRobotStageSlotBlockInfo.CurBlock_StepID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    UpdateSlotBlockInfoJob_OnlyToMixGradeULDFlag(curRobot, curRobotStageSlotBlockInfo, false);

                    //只有一片則不需比對回OK
                    return true;
                }

                #region [ Get Front/Back Job ]

                foreach (Job job in curRobotStageSlotBlockInfo.CurBlockCanControlJobList)
                {
                    switch (job.RobotWIP.CurSubLocation)
                    {
                        case eRobotCommonConst.ROBOT_ARM_BACK_LOCATION:
                            backJob = job;
                            break;

                        case eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION:
                            frontJob = job;
                            break;
                    }
                }

                if (frontJob == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo curStageID({3}) cmdSlotNo({4}) curStepID({5}) FrontJob is Null!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curRobotStageSlotBlockInfo.CurBlock_Location_StageID,
                                                curRobotStageSlotBlockInfo.CurBlock_RobotCmdSlotNo, curRobotStageSlotBlockInfo.CurBlock_StepID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                }

                if (backJob == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo curStageID({3}) cmdSlotNo({4}) curStepID({5}) BackJob is Null!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curRobotStageSlotBlockInfo.CurBlock_Location_StageID,
                                                curRobotStageSlotBlockInfo.CurBlock_RobotCmdSlotNo, curRobotStageSlotBlockInfo.CurBlock_StepID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                }

                if (frontJob == null || backJob == null)
                {

                    //20160216 add for Front Back Job Grade不同時要On Only To MixGrade Unload Flag.找不到WIP視同只能去MIX Port
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo curStageID({3}) cmdSlotNo({4}) curStepID({5}) One Job is Null and Set Block OnlyToMixGradeULDFlag (On)!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curRobotStageSlotBlockInfo.CurBlock_Location_StageID,
                                                curRobotStageSlotBlockInfo.CurBlock_RobotCmdSlotNo, curRobotStageSlotBlockInfo.CurBlock_StepID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    UpdateSlotBlockInfoJob_OnlyToMixGradeULDFlag(curRobot, curRobotStageSlotBlockInfo, true);

                    return false;
                }

                #endregion

                #region [ 20160216 add 取得Front and BackJob後開始比對Grade ]

                if (frontJob.JobGrade != backJob.JobGrade)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo curStageID({3}) cmdSlotNo({4}) curStepID({5}) FrontJob({6},{7}) Grade({8}) but BackJob({9},{10}) Grade({11})! Set Block OnlyToMixGradeULDFlag (On)!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curRobotStageSlotBlockInfo.CurBlock_Location_StageID,
                                                curRobotStageSlotBlockInfo.CurBlock_RobotCmdSlotNo, curRobotStageSlotBlockInfo.CurBlock_StepID, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.JobGrade, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.JobGrade);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    UpdateSlotBlockInfoJob_OnlyToMixGradeULDFlag(curRobot, curRobotStageSlotBlockInfo, true);

                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo curStageID({3}) cmdSlotNo({4}) curStepID({5}) FrontJob({6},{7}) Grade({8}) and BackJob({9},{10}) Grade({11}). Set Block OnToMixGradeULDFlag (Off)!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curRobotStageSlotBlockInfo.CurBlock_Location_StageID,
                                                curRobotStageSlotBlockInfo.CurBlock_RobotCmdSlotNo, curRobotStageSlotBlockInfo.CurBlock_StepID, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.JobGrade, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.JobGrade);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    UpdateSlotBlockInfoJob_OnlyToMixGradeULDFlag(curRobot, curRobotStageSlotBlockInfo, false);

                }

                #endregion

                #region [ Check Front/Back Job RouteID ]

                fail_ReasonCode = string.Format("{0}_{1}_{2}", funcName, "CheckSlotBlockInfoJobRouteCondition", "CheckRouteID");

                if (frontJob.RobotWIP.CurRouteID != backJob.RobotWIP.CurRouteID)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) curRouteID({5}) but BackJob({6},{7}) curRouteID({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.CurRouteID, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurRouteID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Add To Check Fail Message To Front Job ]

                    if (!frontJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) curRouteID({5}) but BackJob({6},{7}) curRouteID({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.CurRouteID, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurRouteID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) curRouteID({2}) but BackJob({3},{4}) curRouteID({5}) is different! RtnCode({6})",
                                                 frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                 frontJob.RobotWIP.CurRouteID, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurRouteID);
                        failMsg = string.Format("RtnCode({0})RtnMsg([{1}]{2})", eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_RouteID_NotMatch, MethodBase.GetCurrentMethod(), failMsg);
                        AddJobCheckFailMsg(frontJob, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #endregion

                    #region [ Add To Check Fail Message To Back Job ]

                    if (!backJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) curRouteID({5}) but BackJob({6},{7}) curRouteID({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.CurRouteID, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurRouteID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) curRouteID({2}) but BackJob({3},{4}) curRouteID({5}) is different!",
                                                 frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                 frontJob.RobotWIP.CurRouteID, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurRouteID);
                        failMsg = string.Format("RtnCode({0})RtnMsg([{1}]{2})", eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_RouteID_NotMatch, MethodBase.GetCurrentMethod(), failMsg);
                        AddJobCheckFailMsg(backJob, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #endregion

                    return false;
                }
                else
                {
                    //Clear Front/Back Job Fail Msg
                    RemoveJobCheckFailMsg(frontJob, fail_ReasonCode);
                    RemoveJobCheckFailMsg(backJob, fail_ReasonCode);
                }

                #endregion

                #region [ Check Front/Back Job curStepID ]

                fail_ReasonCode = string.Format("{0}_{1}_{2}", funcName, "CheckSlotBlockInfoJobRouteCondition", "CurStepNo");

                if (frontJob.RobotWIP.CurStepNo != backJob.RobotWIP.CurStepNo)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) CurStepNo({5}) but BackJob({6},{7}) CurStepNo({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.CurStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurStepNo.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Add To Check Fail Message To Front Job ]

                    if (!frontJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) CurStepNo({5}) but BackJob({6},{7}) CurStepNo({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.CurStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurStepNo.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) CurStepNo({4}) but BackJob({5},{6}) CurStepNo({7}) is different! RtnCode({8})",
                        //                         funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                        //                         frontJob.RobotWIP.CurStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurStepNo.ToString(),
                        //                         eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_CurStepID_NotMatch);

                        failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) CurStepNo({2}) but BackJob({3},{4}) CurStepNo({5}) is different!",
                                                 frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                 frontJob.RobotWIP.CurStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurStepNo.ToString());
                        failMsg = string.Format("RtnCode({0})RtnMsg([{1}]{2})", eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_CurStepID_NotMatch, MethodBase.GetCurrentMethod().Name, failMsg);
                        AddJobCheckFailMsg(frontJob, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #endregion

                    #region [ Add To Check Fail Message To Back Job ]

                    if (!backJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) CurStepNo({5}) but BackJob({6},{7}) CurStepNo({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.CurStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurStepNo.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) CurStepNo({4}) but BackJob({5},{6}) CurStepNo({7}) is different! RtnCode({8})",
                        //                         funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                        //                         frontJob.RobotWIP.CurStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurStepNo.ToString(),
                        //                         eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_CurStepID_NotMatch);

                        failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) CurStepNo({2}) but BackJob({3},{4}) CurStepNo({5}) is different! ",
                                                 frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                 frontJob.RobotWIP.CurStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.CurStepNo.ToString());
                        failMsg = string.Format("RtnCode({0})RtnMsg([{1}]{2})", eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_CurStepID_NotMatch, MethodBase.GetCurrentMethod().Name, failMsg);
                        AddJobCheckFailMsg(backJob, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #endregion

                    return false;
                }
                else
                {
                    //Clear Front/Back Job Fail Msg
                    RemoveJobCheckFailMsg(frontJob, fail_ReasonCode);
                    RemoveJobCheckFailMsg(backJob, fail_ReasonCode);
                }

                #endregion

                #region [ Check Front/Back Job NextStepID ]

                fail_ReasonCode = string.Format("{0}_{1}_{2}", funcName, "CheckSlotBlockInfoJobRouteCondition", "NextStepID");

                if (frontJob.RobotWIP.NextStepNo != backJob.RobotWIP.NextStepNo)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) NextStepNo({5}) but BackJob({6},{7}) NextStepNo({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.NextStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.NextStepNo.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Add To Check Fail Message To Front Job ]

                    if (!frontJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) NextStepNo({5}) but BackJob({6},{7}) NextStepNo({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.NextStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.NextStepNo.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) NextStepNo({4}) but BackJob({5},{6}) NextStepNo({7}) is different! RtnCode({8})",
                        //                         funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                        //                         frontJob.RobotWIP.NextStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.NextStepNo.ToString(),
                        //                         eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_NextStepID_NotMatch);

                        failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) NextStepNo({2}) but BackJob({3},{4}) NextStepNo({5}) is different!",
                                                 frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                 frontJob.RobotWIP.NextStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.NextStepNo.ToString());
                        failMsg = string.Format("RtnCode({0})RtnMsg([{1}]{2})", eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_NextStepID_NotMatch, MethodBase.GetCurrentMethod().Name, failMsg);
                        AddJobCheckFailMsg(frontJob, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #endregion

                    #region [ Add To Check Fail Message To back Job ]

                    if (!backJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) NextStepNo({5}) but BackJob({6},{7}) NextStepNo({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.RobotWIP.NextStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.NextStepNo.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) NextStepNo({4}) but BackJob({5},{6}) NextStepNo({7}) is different! RtnCode({8})",
                        //                         funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                        //                         frontJob.RobotWIP.NextStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.NextStepNo.ToString(),
                        //                         eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_NextStepID_NotMatch);

                        failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) NextStepNo({2}) but BackJob({3},{4}) NextStepNo({5}) is different! RtnCode({6})",
                                               frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                               frontJob.RobotWIP.NextStepNo.ToString(), backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.RobotWIP.NextStepNo.ToString());
                        failMsg = string.Format("RtnCode({0})RtnMsg([{1}]{2})", eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_NextStepID_NotMatch, MethodBase.GetCurrentMethod().Name, failMsg);
                        AddJobCheckFailMsg(backJob, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #endregion

                    return false;
                }
                else
                {
                    //Clear Front/Back Job Fail Msg
                    RemoveJobCheckFailMsg(frontJob, fail_ReasonCode);
                    RemoveJobCheckFailMsg(backJob, fail_ReasonCode);
                }

                #endregion

                #region [ 20160301 add Check Front/Back SamplingFlag ]

                fail_ReasonCode = string.Format("{0}_{1}_{2}", funcName, "CheckSlotBlockInfoJobRouteCondition", "SamplingFlag");

                if (frontJob.SamplingSlotFlag != backJob.SamplingSlotFlag)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) SamplingFlag({5}) but BackJob({6},{7}) SamplingFlag({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.SamplingSlotFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.SamplingSlotFlag);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Add To Check Fail Message To Front Job ]

                    if (!frontJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) SamplingFlag({5}) but BackJob({6},{7}) SamplingFlag({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.SamplingSlotFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.SamplingSlotFlag);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) SamplingFlag({4}) but BackJob({5},{6}) SamplingFlag({7}) is different! RtnCode({8})",
                        //                         funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                        //                         frontJob.SamplingSlotFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.SamplingSlotFlag,
                        //                         eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_SamplingFlag_NotMatch);

                        failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) SamplingSlotFlag({2}) but BackJob({3},{4}) SamplingSlotFlag({5}) is different!",
                                            frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                            frontJob.SamplingSlotFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.SamplingSlotFlag);
                        failMsg = string.Format("RtnCode({0})RtnMsg([{1}]{2})", eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_SamplingFlag_NotMatch, MethodBase.GetCurrentMethod().Name, failMsg);
                        AddJobCheckFailMsg(frontJob, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #endregion

                    #region [ Add To Check Fail Message To back Job ]

                    if (!backJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) SamplingFlag({5}) but BackJob({6},{7}) SamplingFlag({8}) is different!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                frontJob.SamplingSlotFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.SamplingSlotFlag);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) SamplingSlotFlag({4}) but BackJob({5},{6}) SamplingSlotFlag({7}) is different! RtnCode({8})",
                        //                         funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                        //                         frontJob.SamplingSlotFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.SamplingSlotFlag,
                        //                         eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_SamplingFlag_NotMatch);

                        failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) SamplingSlotFlag({2}) but BackJob({3},{4}) SamplingSlotFlag({5}) is different!",
                         frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                         frontJob.SamplingSlotFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, backJob.SamplingSlotFlag);
                        failMsg = string.Format("RtnCode({0})RtnMsg([{1}]{2})", eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_SamplingFlag_NotMatch, MethodBase.GetCurrentMethod().Name, failMsg);
                        AddJobCheckFailMsg(backJob, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }

                    #endregion

                    return false;

                }
                else
                {
                    //Clear Front/Back Job Fail Msg
                    RemoveJobCheckFailMsg(frontJob, fail_ReasonCode);
                    RemoveJobCheckFailMsg(backJob, fail_ReasonCode);
                }

                #endregion

                #region [ 20160318 add SOR Line Flag Mode Check Front/Back EQPFlag ]

                if (Workbench.LineType == eLineType.CELL.CCSOR)
                {
                    fail_ReasonCode = string.Format("{0}_{1}_{2}", funcName, "CheckSlotBlockInfoJobRouteCondition", "SOR_EQPFlag");

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

                        #region [ Add To Check Fail Message To Front Job ]

                        if (!frontJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) BackJob({5},{6}) can not Get EQP by NODENO({7})!",
                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                    backJob.CassetteSequenceNo, backJob.JobSequenceNo, curRobot.Data.NODENO);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) BackJob({4},{5}) can not Get EQP by NODENO({6})! RtnCode({7}",
                            //                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                            //                        backJob.CassetteSequenceNo, backJob.JobSequenceNo, curRobot.Data.NODENO, eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_GetEQP_Fail);

                            failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) BackJob({2},{3}) can not Get EQP by NODENO({4})! RtnCode({5}",
                                                    frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                    backJob.CassetteSequenceNo, backJob.JobSequenceNo, curRobot.Data.NODENO, eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_GetEQP_Fail);

                            AddJobCheckFailMsg(frontJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion
                        }

                        #endregion

                        #region [ Add To Check Fail Message To back Job ]

                        if (!backJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) BackJob({5},{6}) can not Get EQP by NODENO({7})!",
                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                    backJob.CassetteSequenceNo, backJob.JobSequenceNo, curRobot.Data.NODENO);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) BackJob({4},{5}) can not Get EQP by NODENO({6})! RtnCode({7}",
                            //                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                            //                        backJob.CassetteSequenceNo, backJob.JobSequenceNo, curRobot.Data.NODENO, eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_GetEQP_Fail);

                            failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) BackJob({2},{3}) can not Get EQP by NODENO({4})! RtnCode({5}",
                                                    frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                    backJob.CassetteSequenceNo, backJob.JobSequenceNo, curRobot.Data.NODENO, eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_GetEQP_Fail);

                            AddJobCheckFailMsg(backJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion
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

                        #region [ Add To Check Fail Message To Front Job ]

                        if (!frontJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) BackJob({5},{6}) can not Get Line by LINEID({7})!",
                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                    backJob.CassetteSequenceNo, backJob.JobSequenceNo, curEQP.Data.LINEID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) BackJob({4},{5}) can not Get Line by LINEID({6})! RtnCode({7}",
                            //                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                            //                        backJob.CassetteSequenceNo, backJob.JobSequenceNo, curEQP.Data.LINEID, eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_GetLine_Fail);

                            failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) BackJob({2},{3}) can not Get Line by LINEID({4})! RtnCode({5}",
                                                    frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                    backJob.CassetteSequenceNo, backJob.JobSequenceNo, curEQP.Data.LINEID, eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_GetLine_Fail);

                            AddJobCheckFailMsg(frontJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion
                        }

                        #endregion

                        #region [ Add To Check Fail Message To back Job ]

                        if (!backJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) BackJob({5},{6}) can not Get Line by LINENO({7})!",
                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                    backJob.CassetteSequenceNo, backJob.JobSequenceNo, curEQP.Data.LINEID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) BackJob({4},{5}) can not Get Line by LINEID({6})! RtnCode({7}",
                            //                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                            //                        backJob.CassetteSequenceNo, backJob.JobSequenceNo, curEQP.Data.LINEID, eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_GetLine_Fail);

                            failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) BackJob({2},{3}) can not Get Line by LINEID({4})! RtnCode({5}",
                                                    frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                    backJob.CassetteSequenceNo, backJob.JobSequenceNo, curEQP.Data.LINEID, eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_GetLine_Fail);

                            AddJobCheckFailMsg(backJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion
                        }

                        #endregion

                        return false;

                    }

                    #endregion

                    #region [ Check EQP current RunMode(desc)

                    //1 : Grade Mode,  2 : Flag Mode
                    string runModeDesc = string.Empty;
                    string eqpRunMode = GetCellRunMode(line, curEQP.Data.NODEID, "2", out runModeDesc);

                    if (curEQP.File.EquipmentRunMode == eqpRunMode)
                    {

                        #region [ SOR Line FlagMode才比對EQP Flag ]

                        IDictionary<string, string> front_eqpflagInfo = ObjectManager.SubJobDataManager.Decode(frontJob.EQPFlag, eJOBDATA.EQPFlag);
                        string front_DCRandSorterFlag = front_eqpflagInfo["DCRandSorterFlag"];

                        IDictionary<string, string> back_eqpflagInfo = ObjectManager.SubJobDataManager.Decode(backJob.EQPFlag, eJOBDATA.EQPFlag);
                        string back_DCRandSorterFlag = back_eqpflagInfo["DCRandSorterFlag"];

                        if (((front_DCRandSorterFlag == "2" || front_DCRandSorterFlag == "3") && (back_DCRandSorterFlag == "2" || back_DCRandSorterFlag == "3")) ||
                            ((front_DCRandSorterFlag == "0" || front_DCRandSorterFlag == "1") && (back_DCRandSorterFlag == "0" || back_DCRandSorterFlag == "1")))
                        {
                            //Clear Front/Back Job Fail Msg
                            RemoveJobCheckFailMsg(frontJob, fail_ReasonCode);
                            RemoveJobCheckFailMsg(backJob, fail_ReasonCode);
                        }
                        else
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) DCRandSorterFlag({5}) but BackJob({6},{7}) DCRandSorterFlag({8}) is notMatch!",
                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                        front_DCRandSorterFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, back_DCRandSorterFlag);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add To Check Fail Message To Front Job ]

                            if (!frontJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) DCRandSorterFlag({5}) but BackJob({6},{7}) DCRandSorterFlag({8}) is notMatch!",
                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                        front_DCRandSorterFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, back_DCRandSorterFlag);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) DCRandSorterFlag({4}) but BackJob({5},{6}) DCRandSorterFlag({7}) is notMatch! RtnCode({8})",
                                //                         funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                //                         front_DCRandSorterFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, back_DCRandSorterFlag,
                                //                         eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_NotMatch);

                                failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) DCRandSorterFlag({2}) SorterFlag({3}) but BackJob({4},{5}) DCRandSorterFlag({6}) SorterFlag({7}) is notMatch!",
                                                        frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                        front_DCRandSorterFlag, getDetailSortFlag(front_DCRandSorterFlag), backJob.CassetteSequenceNo, backJob.JobSequenceNo, back_DCRandSorterFlag, getDetailSortFlag(back_DCRandSorterFlag));
                                failMsg = string.Format("RtnCode({0})RtnMsg([{1}]{2})", eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_NotMatch, MethodBase.GetCurrentMethod().Name, failMsg);
                                AddJobCheckFailMsg(frontJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion
                            }

                            #endregion

                            #region [ Add To Check Fail Message To back Job ]

                            if (!backJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) SlotBlockInfo FrontJob({3},{4}) DCRandSorterFlag({5}) but BackJob({6},{7}) DCRandSorterFlag({8}) is notMatch!",
                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                        front_DCRandSorterFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, back_DCRandSorterFlag);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("[{0}] Robot({1}) SlotBlockInfo FrontJob({2},{3}) DCRandSorterFlag({4}) but BackJob({5},{6}) DCRandSorterFlag({7}) is notMatch! RtnCode({8})",
                                //                         funcName, curRobot.Data.ROBOTNAME, frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                //                         front_DCRandSorterFlag, backJob.CassetteSequenceNo, backJob.JobSequenceNo, back_DCRandSorterFlag,
                                //                         eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_NotMatch);

                                failMsg = string.Format("SlotBlockInfo FrontJob({0},{1}) DCRandSorterFlag({2}) SorterFlag({3}) but BackJob({4},{5}) DCRandSorterFlag({6}) SorterFlag({7}) is notMatch!",
                                                        frontJob.CassetteSequenceNo, frontJob.JobSequenceNo,
                                                        front_DCRandSorterFlag, getDetailSortFlag(front_DCRandSorterFlag), backJob.CassetteSequenceNo, backJob.JobSequenceNo, back_DCRandSorterFlag, getDetailSortFlag(back_DCRandSorterFlag));
                                failMsg = string.Format("RtnCode({0})RtnMsg([{1}]{2})", eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBackJob_EQPFlag_NotMatch, MethodBase.GetCurrentMethod().Name, failMsg);
                                AddJobCheckFailMsg(backJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion
                            }

                            #endregion

                            return false;

                        }

                        #endregion

                    }
                    else
                    {

                        //非Flag Mode則Clear Front/Back Job Fail Msg
                        RemoveJobCheckFailMsg(frontJob, fail_ReasonCode);
                        RemoveJobCheckFailMsg(backJob, fail_ReasonCode);
                    }

                    #endregion

                }

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }


        }

        protected void UpdateSlotBlockInfoJob_OnlyToMixGradeULDFlag(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, bool onlyToMixGradeULDFlag)
        {
            string strlog = string.Empty;

            try
            {
                if (curSlotBlockInfo == null)
                {
                    return;
                }

                if (curSlotBlockInfo.CurBlockCanControlJobList.Count < 1)
                {

                    return;
                }

                for (int i = 0; i < curSlotBlockInfo.CurBlockCanControlJobList.Count; i++)
                {
                    if (curSlotBlockInfo.CurBlockCanControlJobList[i].RobotWIP.OnlyToMixGradeULDFlag != onlyToMixGradeULDFlag)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) SlotBlockInfo OnlyToMixGradeULDFlag({2}) Update Job({3},{4}) Grade({5}) OnlyToMixGradeULDFlag from ({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, onlyToMixGradeULDFlag,
                                                curSlotBlockInfo.CurBlockCanControlJobList[i].CassetteSequenceNo, curSlotBlockInfo.CurBlockCanControlJobList[i].JobSequenceNo,
                                                curSlotBlockInfo.CurBlockCanControlJobList[i].JobGrade,
                                                curSlotBlockInfo.CurBlockCanControlJobList[i].RobotWIP.OnlyToMixGradeULDFlag, onlyToMixGradeULDFlag);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curSlotBlockInfo.CurBlockCanControlJobList[i])
                        {

                            curSlotBlockInfo.CurBlockCanControlJobList[i].RobotWIP.OnlyToMixGradeULDFlag = onlyToMixGradeULDFlag;

                        }
                    }

                }


            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //20160318 add Get Cell Run Desc (目前EQP是記錄成Desc)
        private string GetCellRunMode(Line line, string eqpNo, string value, out string description)
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
        //20160719 add For Log
        protected string getDetailJobJudge(string CurSendOutJobJudge)
        {

            //Job Judge
            //0：Inspection Skip or No Judge
            //1：OK
            //2：NG - Insp. Result 
            //3：RW - Required Rework
            //4：PD –Pending judge
            //5：RP – Required Repair
            //6：IR–Ink Repair
            //7：Other
            //8：RV –PI Reivew
            string strDetailJobGrade = CurSendOutJobJudge;
            switch (CurSendOutJobJudge)
            {
                case "0":
                    strDetailJobGrade = "Inspection Skip or No Judge";
                    break;
                case "1":
                    strDetailJobGrade = "OK";
                    break;
                case "2":
                    strDetailJobGrade = "NG - Insp. Result";
                    break;
                case "3":
                    strDetailJobGrade = "RW - Required Rework";
                    break;
                case "4":
                    strDetailJobGrade = "PD –Pending judge";
                    break;
                case "5":
                    strDetailJobGrade = "RP – Required Repair";
                    break;
                case "6":
                    strDetailJobGrade = "IR–Ink Repair";
                    break;
                case "7":
                    strDetailJobGrade = "Other";
                    break;
                case "8":
                    strDetailJobGrade = "RV –PI Reivew";
                    break;

            }
            return strDetailJobGrade;
        }
        /// <summary>
        /// EqpFlag_DCRandSorterFlag 获取SortFlag
        /// </summary>
        /// <returns></returns>
        protected bool getDetailSortFlag(string EqpFlag_DCRandSorterFlag)
        {
            //EqpFlag_DCRandSorterFlag：
            //(1) 0:No Flag Glass或1:DCR Flag Glass 的Sorter Flag 为false
            //(2) 2:Sorter Flag Glass或3:DCR and Sorter Flag Glass的Sorter Flag 为true
            bool bSortFlag = false;
            switch (EqpFlag_DCRandSorterFlag)
            {
                case "0":
                case "1":
                    bSortFlag = false;
                    break;
                case "2":
                case "3":
                    bSortFlag = true;
                    break;
            }
            return bSortFlag;
        }
    }
}
