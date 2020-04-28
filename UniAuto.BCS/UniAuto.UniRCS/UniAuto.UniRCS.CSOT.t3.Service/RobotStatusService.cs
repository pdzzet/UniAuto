using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.EntityManager;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class RobotStatusService : AbstractRobotService
    {

        //抽象類別的實作
        public override bool Init()
        {
            return true;
        }

        public void Destroy()
        {
            
        }

//Function List ======================================================================================================================================================================

        /// <summary>
        /// Handle Robot Status change Report Event
        /// </summary>
        /// <param name="inputData"></param>
        public void RobotStatusBlock(Trx inputData)
        {

            string strlog = string.Empty;
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]
                //<trx name="L2_RobotStatusBlock" triggercondition="change">
                //  <eventgroup name="L2_EG_RobotStatusBlock" dir="E2B">
                //    <event name="L2_W_RobotStatusBlock" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<event name="L2_W_RobotStatusBlock" devicecode="W" address="0x0001792" points="5">
                //  <itemgroup name="RobotStatusBlock" />
                //</event>

                //<itemgroup name="RobotStatusBlock">
                //  <item name="RobotStatusChangeData" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="RobotHasCommand" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>

                #endregion

                string strTransactionGroup = string.Format("{0}_EG_RobotStatusBlock", eqpNo);//Event Group
                string strTransactionEvent = string.Format("{0}_W_RobotStatusBlock", eqpNo);//Event
                string strItemRobotStatusChangeData = "RobotStatusChangeData";
                string strItemRobotHasCommand = "RobotHasCommand";

                string strItemRobotStatusChangeDataValue = inputData[strTransactionGroup][strTransactionEvent][strItemRobotStatusChangeData].Value;
                string strItemRobotHasCommandValue = inputData[strTransactionGroup][strTransactionEvent][strItemRobotHasCommand].Value;

                //string robotStatus = inputData.EventGroups[0].Events[0].Items[0].Value;
                //eRobotStatus curStatus = (eRobotStatus)int.Parse(robotStatus);

                #endregion

                //[ Wait_Proc_0007 ] 後續處理

                eRobotStatus newRobotStatus = (eRobotStatus)int.Parse(strItemRobotStatusChangeDataValue);
                eRobotHasCommandStatus newRobothasCmdStatus = (eRobotHasCommandStatus)int.Parse(strItemRobotHasCommandValue);

                #region [ Get Robot by TrxNodeNo ]

                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP][{1}] can not find Robot entity by Trx EQPID({2})! ",
                                            eqpNo, inputData.TrackKey, eqpNo);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return;
                }

                #endregion

                eRobotStatus oldRobotStatus = curRobot.File.Status;
                eRobotHasCommandStatus oldRobothasCmdStatus = curRobot.File.RobotHasCommandstatus;

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Robot({2}) Status Change from ({3}) to ({4}), has Command Status from ({5}) to ({6}).",
                                        eqpNo, inputData.TrackKey, curRobot.Data.ROBOTNAME, oldRobotStatus.ToString(),
                                        newRobotStatus.ToString(), oldRobothasCmdStatus.ToString(), newRobothasCmdStatus.ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #region [ Status change to Update Robot Entity ]

                //if (newRobotStatus != oldRobotStatus || newRobothasCmdStatus != oldRobothasCmdStatus)
                //{
                //    strlog = string.Format("[{0}] {1} - Robot({2}) Status Change From ({3}) to ({4}), has Command Status from ({5}) to ({6}).",
                //                            "RobotStatusService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                //                            "RobotStatusReport".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                //                            curRobot.Data.ROBOTNAME,
                //                            oldRobotStatus.ToString(), newRobotStatus.ToString(),
                //                            oldRobothasCmdStatus.ToString(), newRobothasCmdStatus.ToString());

                //    Logger.LogTrxWrite(this.LogName, strlog);

                //    lock (curRobot)
                //    {
                //        curRobot.File.Status = newRobotStatus;
                //        curRobot.File.RobotHasCommandstatus = newRobothasCmdStatus;

                //    }

                //    //存入Robot File
                //    ObjectManager.RobotManager.EnqueueSave(curRobot.File);

                //    //通知OPI更新
                //    Invoke(eServiceName.UIService, "RobotCurrentModeReport", new object[] { curRobot });

                //    //更新Status Change Flag通知RCS顯示detail Log
                //    curRobot.RobotStatusChangeFlag = true;

                //    //Status Change則移除所有Robot Check Fail Message
                //    RemoveRobotAllCheckFailMsg(curRobot);

                //}

                #endregion

                //#region [ 當Status變成Running時要清除RobotCmd Active TimeOut ]

                //if (curRobot.File.Status == eRobotStatus.RUNNING)
                //{
                //    string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                //    if (_timerManager.IsAliveTimer(timeName))
                //    {
                //        _timerManager.TerminateTimer(timeName);
                //    }

                //}

                //#endregion

            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        public void RobotCurrentPositionReportBlock(Trx inputData)
        {
            
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                string strlog=string.Empty;

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L2_RobotCurrentPositionReportBlock" triggercondition="change">
                //  <eventgroup name="L2_EG_RobotCurrentPositionReportBlock" dir="E2B">
                //    <event name="L2_W_RobotCurrentPositionReportBlock" trigger="true" />
                //  </eventgroup>
                //</trx>
                //<itemgroup name="RobotCurrentPositionReportBlock">
                //  <item name="CurrentPosition" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>

                #endregion

                string strTransactionGroup = string.Format("{0}_EG_RobotCurrentPositionReportBlock", eqpNo);//Event Group
                string strTransactionEvent = string.Format("{0}_W_RobotCurrentPositionReportBlock", eqpNo);//Event
                string strItemCurrentPosition = "CurrentPosition";

                string curRobotPosition = inputData[strTransactionGroup][strTransactionEvent][strItemCurrentPosition].Value;

                #endregion
              
                #region [ Get Robot by TrxNodeNo ]

                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);
                
                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP][{1}] can not find Robot entity by Trx EQPID({2})! ",
                                            eqpNo, inputData.TrackKey, eqpNo);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return;
                }

                #endregion

                string strRobotPosition = curRobotPosition.ToString().PadLeft(2,'0');

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Robot({2}) curPosition Change from ({3}) to ({4}).",
                                        eqpNo, inputData.TrackKey, curRobot.Data.ROBOTNAME, curRobot.File.CurRobotPosition,
                                        strRobotPosition);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #region [ 有變化才更新資訊 ]

                if (curRobot.File.CurRobotPosition != strRobotPosition)
                {
                    strlog = string.Format("[{0}] {1} - Robot({2}) curPosition Change From ({3}) to ({4}).",
                                            "RobotStatusService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                            "RobotCurrentPositionReportBlock".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                            curRobot.Data.ROBOTNAME,
                                            curRobot.File.CurRobotPosition, 
                                            strRobotPosition);

                    Logger.LogTrxWrite(this.LogName, strlog);

                    lock (curRobot)
                    {
                        curRobot.File.CurRobotPosition = strRobotPosition;

                    }

                    //存入Robot File
                    ObjectManager.RobotManager.EnqueueSave(curRobot.File);

                    //通知OPI更新//[ Wait_Proc_0008 ] 要新增此Funtion Item給OPI
                    Invoke(eServiceName.UIService, "RobotCurrentModeReport", new object[] { curRobot });

                }

                #endregion

            }
            catch (Exception ex)
            {
                
                 Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }


        }

        /// <summary> Single Substrate Robot Arm Report For CF or Array
        /// 
        /// </summary>
        /// <param name="inputData"></param>
        public void SingleSubstrateInfoBlock(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]


                #region [ Trx Structure ]

                // <trx name="L2_Arm#01SingleSubstrateInfoBlock" triggercondition="change">
                //  <eventgroup name="L2_EG_Arm#01SingleSubstrateInfoBlock" dir="E2B">
                //    <event name="L2_W_Arm#01SingleSubstrateInfoBlock" trigger="true" />
                //  </eventgroup>
                //</trx>

                //20150812 Modify Trx Item Name
                //<itemgroup name="Arm#02SingleSubstrateInfoBlock">
                //  <item name="JobCassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="JobExist" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>

                #endregion

                string strArmNo = inputData.Name.Split('#')[1].Substring(0, 2);// Split Current Trx Arm No By '#'
                string strTransactionGroup = string.Format("{0}_EG_Arm#{1}SingleSubstrateInfoBlock", inputData.Metadata.NodeNo, strArmNo);//Event Group
                string strTransactionEvent =  string.Format ("{0}_W_Arm#{1}SingleSubstrateInfoBlock",inputData.Metadata.NodeNo,strArmNo );//Event
                string strItemJobCstNO = "JobCassetteSequenceNo";
                string strItemJobSlotNO = "JobSequenceNo";
                string strItemJobExist = "JobExist";

                string strItemJobCstNOValue = inputData[strTransactionGroup][strTransactionEvent][strItemJobCstNO].Value;
                string strItemJobSlotNOValue = inputData[strTransactionGroup][strTransactionEvent][strItemJobSlotNO].Value;
                string strItemJobExistValue = inputData[strTransactionGroup][strTransactionEvent][strItemJobExist].Value;

                #endregion
            }
            catch (Exception ex)
            {
                
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> Double Substrate Robot Arm Report For Cell
        /// 
        /// </summary>
        /// <param name="inputData"></param>
        public void DoubleSubstrateInfoBlock(Trx inputData)
        {
            //Wait Cell trx File
            try
            {
                #region [拆出PLCAgent Data]

                
                #region [ Trx Structure ]

                //<trx name="L2_Arm#02DoubleSubstrateInfoBlock" triggercondition="change">
                //    <eventgroup name="L2_EG_Arm#02DoubleSubstrateInfoBlock" dir="E2B">
                //        <event name="L2_W_Arm#02DoubleSubstrateInfoBlock" trigger="true" />
                //    </eventgroup>
                //</trx>

                //<itemgroup name="Arm#02DoubleSubstrateInfoBlock">
                //  <item name="ForkFrontEndJobCassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ForkFrontEndJobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ForkFrontEndJobExist" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ForkBackEndJobCassetteSequenceNo" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ForkBackEndJobSequenceNo" woffset="4" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ForkBackEndJobExist" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>

                #endregion

                string strArmNo = inputData.Name.Split('#')[1].Substring(0, 2);// Split Current Trx Arm No By '#'
                string strTransactionGroup = string.Format("{0}_EG_Arm#{1}DoubleSubstrateInfoBlock", inputData.Metadata.NodeNo, strArmNo);//Event Group
                string strTransactionEvent = string.Format("{0}_W_Arm#{1}DoubleSubstrateInfoBlock", inputData.Metadata.NodeNo, strArmNo);//Event
                string strItemForkFrontJobCstNO = "ForkFrontEndJobCassetteSequenceNo";
                string strItemForkFrontJobSlotNO = "ForkFrontEndJobSequenceNo";
                string strItemForkFrontJobExist = "ForkFrontEndJobExist";
                string strItemForkBackJobCstNO = "ForkBackEndJobCassetteSequenceNo";
                string strItemForkBackJobSlotNO = "ForkBackEndJobSequenceNo";
                string strItemForkBackJobExist = "ForkBackEndJobExist";

                string strItemForkFrontJobCstNOValue = inputData[strTransactionGroup][strTransactionEvent][strItemForkFrontJobCstNO].Value;
                string strItemForkFrontJobSlotNOValue = inputData[strTransactionGroup][strTransactionEvent][strItemForkFrontJobSlotNO].Value;
                string strItemForkFrontJobExistValue = inputData[strTransactionGroup][strTransactionEvent][strItemForkFrontJobExist].Value;
                string strItemForkBackJobCstNOValue = inputData[strTransactionGroup][strTransactionEvent][strItemForkBackJobCstNO].Value;
                string strItemForkBackJobSlotNOValue = inputData[strTransactionGroup][strTransactionEvent][strItemForkBackJobSlotNO].Value;
                string strItemForkBackJobExistValue = inputData[strTransactionGroup][strTransactionEvent][strItemForkBackJobExist].Value;
                #endregion
            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>Stage Position Report By Singe Position
        /// 
        /// </summary>
        /// <param name="inputData"></param>
        public void SinglePositionReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]

                #region  [ Trx Structure ]

                //<trx name="L2_Stage#01SinglePositionReport" triggercondition="change">
                //  <eventgroup name="L2_EG_Stage#01SinglePositionReport" dir="E2B">
                //    <event name="L2_B_Stage#01SendReady" trigger="true" />
                //    <event name="L2_B_Stage#01ReceiveReady" trigger="true" />
                //    <event name="L2_B_Stage#01DoubleGlassExist" trigger="true" />
                //    <event name="L2_B_Stage#01ExchangePossible" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<itemgroup name="Stage#01SinglePositionReport">
                //  <item name="Stage#01SendReady" woffset="0" boffset="0" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="Stage#01ReceiveReady" woffset="0" boffset="1" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="Stage#01DoubleGlassExist" woffset="0" boffset="2" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="Stage#01ExchangePossible" woffset="0" boffset="3" wpoints="0" bpoints="1" expression="BIT" />
                //</itemgroup>
                string strStageNo = inputData.Name.Split('#')[1].Substring(0, 2);// Split Current Trx Stage No By '#'
                string strTransactionGroup = string.Format("{0}_EG_Stage#{1}SinglePositionReport", inputData.Metadata.NodeNo, strStageNo);//Event Group
                string strSendReadyEvent = string.Format("{0}_B_Stage#{1}SendReady", inputData.Metadata.NodeNo, strStageNo);//Event
                string strReceiveReadyEvent = string.Format("{0}_B_Stage#{1}ReceiveReady", inputData.Metadata.NodeNo, strStageNo);//Event
                string strDoubleGlassExistEvent = string.Format("{0}_B_Stage#{1}DoubleGlassExist", inputData.Metadata.NodeNo, strStageNo);//Event
                string strExchangePossibleEvent = string.Format("{0}_B_Stage#{1}ExchangePossible", inputData.Metadata.NodeNo, strStageNo);//Event

                string strItemSendReady = string.Format("Stage#{0}SendReady", strStageNo);
                string strItemReceiveReady = string.Format("Stage#{0}ReceiveReady", strStageNo);
                string strItemDoubleGlassExist = string.Format("Stage#{0}DoubleGlassExist", strStageNo);
                string strItemExchangePossible = string.Format("Stage#{0}ExchangePossible", strStageNo);

                string strItemSendReadyValue = inputData[strTransactionGroup][strSendReadyEvent][strItemSendReady].Value;
                string strItemReceiveReadyValue = inputData[strTransactionGroup][strReceiveReadyEvent][strItemReceiveReady].Value;
                string strItemDoubleGlassExistValue = inputData[strTransactionGroup][strDoubleGlassExistEvent][strItemDoubleGlassExist].Value;
                string strItemExchangePossibleValue = inputData[strTransactionGroup][strExchangePossibleEvent][strItemExchangePossible].Value;

                #endregion

                #endregion
            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region [SingleSubstrateLoadDataReport]

        public void SingleSubstrateLoadDataReport(Trx inputData)
        {
            string strlog = string.Empty;
            string cmdMsg = string.Empty;
            string errMsg = string.Empty;

            try
            {

                #region [拆出PLCAgent Data]

                #region  [ Trx Structure ]

                //<trx name="L2_Arm#01SingleSubstrateLoadDataReport" triggercondition="change">
                //  <eventgroup name="L2_EG_Arm#01SingleSubstrateLoadDataReport" dir="E2B">
                //    <event name="L2_W_Arm#01SingleSubstrateLoadDataReportBlock" />
                //    <event name="L2_B_Arm#01SingleSubstrateLoadDataReport" trigger="true" />
                //  </eventgroup>
                //</trx>

                // <itemgroup name="Arm#01SingleSubstrateLoadDataReportBlock">
                //  <item name="JobCassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ArmNo" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="CurrentPosition" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>

                //<event name="L2_B_Arm#01SingleSubstrateLoadDataReport" devicecode="B" address="0x00008ED" points="1">
                //  <item name="Arm#01SingleSubstrateLoadDataReport" offset="0" points="1" expression="BIT" />
                //</event>

                #endregion

                string eqpNo = inputData.Metadata.NodeNo;
                string strArmNo = inputData.Name.Split('#')[1].Substring(0, 2);// Split Current Trx Arm No By '#'
                string strTransactionGroup = string.Format("{0}_EG_Arm#{1}SingleSubstrateLoadDataReport", eqpNo, strArmNo);//Event Group
                string strWEvent = string.Format("{0}_W_Arm#{1}SingleSubstrateLoadDataReportBlock", eqpNo, strArmNo);//Event
                string strBEvent = string.Format("{0}_B_Arm#{1}SingleSubstrateLoadDataReport", eqpNo, strArmNo);//Event

                string strItemJobCstNO = "JobCassetteSequenceNo";
                string strItemJobSlotNO = "JobSequenceNo";
                string strItemArmNo = "ArmNo";
                string strItemCurrentPosition = "CurrentPosition";

                string strItemBitReport = string.Format("Arm#{0}SingleSubstrateLoadDataReport", strArmNo);

                string strItemJobCstNOValue = inputData[strTransactionGroup][strWEvent][strItemJobCstNO].Value;
                string strItemJobSlotNOValue = inputData[strTransactionGroup][strWEvent][strItemJobSlotNO].Value;
                string strItemArmNoValue = inputData[strTransactionGroup][strWEvent][strItemArmNo].Value;
                //20150922 modify Position要補足2碼否則會取不到
                string strItemCurrentPositionValue = inputData[strTransactionGroup][strWEvent][strItemCurrentPosition].Value.PadLeft(2,'0');

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[strTransactionGroup].Events[strBEvent].Items[strItemBitReport].Value);

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP][{1}] Single Substrate LoadReport Bit({2}) CassetteSequenceNo({3}) JobSequenceNo({4}) UseArmNo({5}) CurPosition({6}) .", 
                                        inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), strItemJobCstNOValue,
                                        strItemJobSlotNOValue, strItemArmNoValue, strItemCurrentPositionValue);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
         
                #region BC Reply Setting

                if (triggerBit == eBitResult.OFF)
                {
                    //Reply SingleSubstrateLoadDataReport
                    SingleSubstrateLoadDataReportReply(eqpNo, strArmNo,eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                //Reply SingleSubstrateLoadDataReport
                SingleSubstrateLoadDataReportReply(eqpNo, strArmNo, eBitResult.ON, inputData.TrackKey);

                #endregion         

                #endregion

                #region [ Get Robot by EQPNO ]

                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);
                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Can't find Robot by EqpNo({2}) in RobotEntity!", 
                                            eqpNo, inputData.TrackKey, eqpNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                #endregion

                //通知OPI更新RobotCommand狀態 下Robot Load Job EX:LoadEvent,Upper=[0,0],Lower=[3263,30],Arm=LOWER,RobotPosition=4
                cmdMsg = string.Format("{0} - LoadEvent, Arm#{1}=[{2},{3}] RobotPosition={4}, CommandStatus={5}",
                                        "ArmSubstrateLoadEventReport".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), strItemArmNoValue,
                                        strItemJobCstNOValue, strItemJobSlotNOValue, strItemCurrentPositionValue, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.NormalType });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #region [ Get Arm BcsJob Entity ]

                Job curArmJob = ObjectManager.JobManager.GetJob(strItemJobCstNOValue, strItemJobSlotNOValue);

                //找不到 BcsJob 回NG
                if (curArmJob == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob by Arm CSTSeq({2}),JobSeq({3})!",
                                                eqpNo, MethodBase.GetCurrentMethod().Name, strItemJobCstNOValue, strItemJobSlotNOValue);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;
                }

                #endregion

                #region [ Get CurStage Entity ]
                RobotStage curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(strItemCurrentPositionValue);
                if (curStage == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Current Stage by Current Position Value({2})!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, strItemCurrentPositionValue);
                      Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                }
                #endregion

                #region OVN提前开门逻辑
                //add by hujunpeng 20181001          
                if (curRobot.Data.LINEID == "TCOVN400" || curRobot.Data.LINEID == "TCOVN500")
                {
                    switch (curStage.Data.STAGEID)
                    {
                        case "11":
                            string trxName = "L3_OVN1MoveOutGlassOpenTheDoor";
                            Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                            outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                            outputdata.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                            outputdata.TrackKey = DateTime.Now.ToString("yyyyMMddHHmmss");
                            SendPLCData(outputdata);
                            strlog = string.Format(" [BCS -> EQP][{0}] , SET OVN1MoveIn BIT={1}.", outputdata.TrackKey, outputdata.EventGroups[0].Events[0].Items[0].Value);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            break;
                        case "13":
                            string trxName1 = "L3_OVN2MoveOutGlassOpenTheDoor";
                            Trx outputdata1 = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName1) as Trx;
                            outputdata1.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                            outputdata1.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                            outputdata1.TrackKey = DateTime.Now.ToString("yyyyMMddHHmmss");
                            SendPLCData(outputdata1);
                            strlog = string.Format(" [BCS -> EQP][{0}] , SET OVN2MoveIn BIT={1}.", outputdata1.TrackKey, outputdata1.EventGroups[0].Events[0].Items[0].Value);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            break;
                        default:
                            break;
                    }
                }
                #endregion

                //Watson Add 20151027 Save Stage Last Glass Send out Date Time
                curStage.File.OutputDateTime = DateTime.Now;  //Stage 最後出片的時間
                ObjectManager.RobotStageManager.EnqueueSave(curStage.File);
                
                //20151031 add Job移動時要清空所有的CheckFailMsg
                RemoveJobAllCheckFailMsg(curArmJob);

                //进行MixNo +1 for DRY fetchout by UnitNo
                //Yang
                int _stageid = int.Parse(curStage.Data.STAGEID);
                Line _line = ObjectManager.LineManager.GetLine(curStage.Data.SERVERNAME);
                if (Workbench.LineType.ToString().Contains("DRY_") && _stageid >= 1 && _stageid <= 10 && curArmJob.RobotWIP.CurRouteID.Contains("TCDRY") && _line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)              
                {
                    Invoke(eServiceName.RobotSpecialService, "MixNoAdd", new object[] { curRobot});
                }

                //執行Step Result的設定
                Invoke(eServiceName.RobotCoreService, "HandleJobProcessResult", new object[] { curRobot, curArmJob, strItemArmNoValue, string.Empty, string.Empty, string.Empty });

                #region [ 20160304 add for Array Only Arm Load如果是Port則需要更新Robot CurRecipeGroupNo ]

                if (curRobot.File.CurFetchOutJobRecipeGroupNo != curArmJob.ArraySpecial.RecipeGroupNumber)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP][{1}] Robot({2}) Update CurFetchOutJobRecipeGroupNo form ({3}) to ({4}) by CassetteSequenceNo({5}) JobSequenceNo({6}) recipeGroupNo",
                                            inputData.Metadata.NodeNo, inputData.TrackKey, curRobot.Data.ROBOTNAME, curRobot.File.CurFetchOutJobRecipeGroupNo,
                                            curArmJob.ArraySpecial.RecipeGroupNumber, curArmJob.CassetteSequenceNo, curArmJob.JobSequenceNo);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                    lock (curRobot)
                    {
                        curRobot.File.CurFetchOutJobRecipeGroupNo = curArmJob.ArraySpecial.RecipeGroupNumber;

                    }

                    ObjectManager.RobotManager.EnqueueSave(curRobot.File);

                    
                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SingleSubstrateLoadDataReportReply(string eqpNo, string ArmNo, eBitResult value, string trackKey)
        {
            try
            {
                string strlog = string.Empty;
                string SingleSubstrateLoadDataReportTimeOut = "SingleSubstrateLoadDataReportTimeOut";

                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + string.Format("_Arm#{0}SingleSubstrateLoadDataReportReply", ArmNo)) as Trx;

                string strTransactionGroup = string.Format("{0}_EG_Arm#{1}SingleSubstrateLoadDataReportReply", eqpNo, ArmNo);
                string strTransactionEvent = string.Format("{0}_B_Arm#{1}SingleSubstrateLoadDataReportReply", eqpNo, ArmNo);
                string strItemReportReply = string.Format("Arm#{0}SingleSubstrateLoadDataReportReply", ArmNo);

                outputdata.EventGroups[strTransactionGroup].Events[strTransactionEvent].Items[strItemReportReply].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_"+ArmNo +"_" + SingleSubstrateLoadDataReportTimeOut))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ArmNo + "_" + SingleSubstrateLoadDataReportTimeOut);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo  + "_"+ArmNo +"_"+ SingleSubstrateLoadDataReportTimeOut, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(SingleSubstrateLoadDataReportReplyTimeOut), trackKey);
                }

                strlog = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Arm Single Substrate Load Report Reply Set Bit ({2}).", eqpNo, trackKey, value.ToString());

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SingleSubstrateLoadDataReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string strlog = string.Empty;
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                strlog = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Equipment Status Change Reply Timeout Report Timeout Set Bit (OFF).", sArray[0], trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                SingleSubstrateLoadDataReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        #endregion

        #region [SingleSubstrateUnloadDataReport]

        public void SingleSubstrateUnloadDataReport(Trx inputData)
        {
            string strlog = string.Empty;
            string cmdMsg = string.Empty;

            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                #region [拆出PLCAgent Data]

                #region  [ Trx Structure ]

                //<trx name="L2_Arm#01SingleSubstrateUnloadDataReport" triggercondition="change">
                //  <eventgroup name="L2_EG_Arm#01SingleSubstrateUnloadDataReport" dir="E2B">
                //    <event name="L2_W_Arm#01SingleSubstrateUnloadDataReportBlock" />
                //    <event name="L2_B_Arm#01SingleSubstrateUnloadDataReport" trigger="true" />
                //  </eventgroup>
                //</trx>

                // <itemgroup name="Arm#01SingleSubstrateUnloadDataReportBlock">
                //  <item name="JobCassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ArmNo" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="CurrentPosition" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>

                //<event name="L2_B_Arm#01SingleSubstrateLoadDataReport" devicecode="B" address="0x00008ED" points="1">
                //  <item name="Arm#01SingleSubstrateLoadDataReport" offset="0" points="1" expression="BIT" />
                //</event>

                #endregion

                string eqpNo = inputData.Metadata.NodeNo;
                string strArmNo = inputData.Name.Split('#')[1].Substring(0, 2);// Split Current Trx Arm No By '#'
                string strTransactionGroup = string.Format("{0}_EG_Arm#{1}SingleSubstrateUnloadDataReport", eqpNo, strArmNo);//Event Group
                string strWEvent = string.Format("{0}_W_Arm#{1}SingleSubstrateUnloadDataReportBlock", eqpNo, strArmNo);//Event
                string strBEvent = string.Format("{0}_B_Arm#{1}SingleSubstrateUnloadDataReport", eqpNo, strArmNo);//Event

                string strItemJobCstNO = "JobCassetteSequenceNo";
                string strItemJobSlotNO = "JobSequenceNo";
                string strItemArmNo = "ArmNo";
                string strItemCurrentPosition = "CurrentPosition";

                string strItemBitReport = string.Format("Arm#{0}SingleSubstrateUnloadDataReport", strArmNo);

                string strItemJobCstNOValue = inputData[strTransactionGroup][strWEvent][strItemJobCstNO].Value;
                string strItemJobSlotNOValue = inputData[strTransactionGroup][strWEvent][strItemJobSlotNO].Value;
                string strItemArmNoValue = inputData[strTransactionGroup][strWEvent][strItemArmNo].Value;
                //20150922 modify Position要補足2碼否則會取不到
                string strItemCurrentPositionValue = inputData[strTransactionGroup][strWEvent][strItemCurrentPosition].Value.PadLeft(2, '0');

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[strTransactionGroup].Events[strBEvent].Items[strItemBitReport].Value);

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP][{1}] Single Substrate UnloadReport Bit({2}) CassetteSequenceNo({3}) JobSequenceNo({4}) UseArmNo({5}) CurPosition({6}) .",
                                        inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), strItemJobCstNOValue,
                                        strItemJobSlotNOValue, strItemArmNoValue, strItemCurrentPositionValue);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                #region BC Reply Setting

                if (triggerBit == eBitResult.OFF)
                {
                    //Reply SingleSubstrateLoadDataReport
                    SingleSubstrateUnloadDataReportReply(eqpNo, strArmNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                //Reply SingleSubstrateLoadDataReport
                SingleSubstrateUnloadDataReportReply(eqpNo, strArmNo, eBitResult.ON, inputData.TrackKey);

                #endregion
                
                #endregion

                #region [ Get Robot by EQPNO ]

                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);
                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Can't find Robot by EqpNo({2}) in RobotEntity!",
                                            eqpNo, inputData.TrackKey, eqpNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                #endregion

                //通知OPI更新RobotCommand狀態 下Robot Load Job EX:LoadEvent,Upper=[0,0],Lower=[3263,30],Arm=LOWER,RobotPosition=4
                cmdMsg = string.Format("{0} - UnloadEvent, Arm#{1}=[{2},{3}] RobotPosition={4}, CommandStatus={5}",
                                        "ArmSubstrateUnloadEventReport".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), strItemArmNoValue,
                                        strItemJobCstNOValue, strItemJobSlotNOValue, strItemCurrentPositionValue, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.NormalType });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #region [ Get Arm BcsJob Entity ]

                Job curArmJob = ObjectManager.JobManager.GetJob(strItemJobCstNOValue, strItemJobSlotNOValue);

                //找不到 BcsJob 回NG
                if (curArmJob == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob by Arm CSTSeq({2}),JobSeq({3})!",
                                                eqpNo, MethodBase.GetCurrentMethod().Name, strItemJobCstNOValue, strItemJobSlotNOValue);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;
                }

                #endregion

                #region [ Get CurStage Entity ]
                RobotStage curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(strItemCurrentPositionValue);
                if (curStage == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Current Stage by Current Position Value({2})!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, strItemCurrentPositionValue);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                }
                #endregion
                //Watson Add 20151027 Save Stage Last Glass Receive Date Time
                curStage.File.InputDateTime = DateTime.Now;  //Stage 最後出片的時間
                ObjectManager.RobotStageManager.EnqueueSave(curStage.File);

                //20151031 add Job移動時要清空所有的CheckFailMsg
                RemoveJobAllCheckFailMsg(curArmJob);

                //執行Step Result的設定
                Invoke(eServiceName.RobotCoreService, "HandleJobProcessResult", new object[] { curRobot, curArmJob, string.Empty, strItemArmNoValue, strItemCurrentPositionValue, strItemJobSlotNOValue });

                //更新ELA stage最後收到時間 by tom.su
                if (line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW && strItemCurrentPositionValue == "15")
                {
                    eqp.File.FinalELAStageReceiveTime = string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now);
                }
                
                
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SingleSubstrateUnloadDataReportReply(string eqpNo, string ArmNo, eBitResult value, string trackKey)
        {
            try
            {
                string strlog = string.Empty;
                string SingleSubstrateUnloadDataReportTimeOut = "SingleSubstrateUnloadDataReportTimeOut";

                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + string.Format("_Arm#{0}SingleSubstrateUnloadDataReportReply", ArmNo)) as Trx;

                string strTransactionGroup = string.Format("{0}_EG_Arm#{1}SingleSubstrateUnloadDataReportReply", eqpNo, ArmNo);
                string strTransactionEvent = string.Format("{0}_B_Arm#{1}SingleSubstrateUnloadDataReportReply", eqpNo, ArmNo);
                string strItemReportReply = string.Format("Arm#{0}SingleSubstrateUnloadDataReportReply", ArmNo);

                outputdata.EventGroups[strTransactionGroup].Events[strTransactionEvent].Items[strItemReportReply].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ArmNo + "_" + SingleSubstrateUnloadDataReportTimeOut))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ArmNo + "_" + SingleSubstrateUnloadDataReportTimeOut);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ArmNo + "_" + SingleSubstrateUnloadDataReportTimeOut, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(SingleSubstrateUnloadDataReportReplyTimeOut), trackKey);
                }

                strlog = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Arm Single Substrate Unload Report Reply Set Bit ({2}).", eqpNo, trackKey, value.ToString());

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SingleSubstrateUnloadDataReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string strlog = string.Empty;
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                strlog = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Equipment Status Change Reply Timeout Report Timeout Set Bit (OFF).", sArray[0], trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                SingleSubstrateUnloadDataReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        #endregion

        #region [DoubleSubstrateLoadDataReport]

        public void DoubleSubstrateLoadDataReport(Trx inputData)
        {
            string strlog = string.Empty;
            string cmdMsg = string.Empty;

            try
            {

                #region [拆出PLCAgent Data]

                #region [Trx Structure]

                //<trx name="L2_Arm#01DoubleSubstrateLoadDataReport" triggercondition="change">
                //  <eventgroup name="L2_EG_Arm#01DoubleSubstrateLoadDataReport" dir="E2B">
                //    <event name="L2_W_Arm#01DoubleSubstrateLoadDataReportBlock" />
                //    <event name="L2_B_Arm#01DoubleSubstrateLoadDataReport" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<event name="L2_B_Arm#01DoubleSubstrateLoadDataReport" devicecode="B" address="0x00008ED" points="1">
                //  <item name="Arm#01DoubleSubstrateLoadDataReport" offset="0" points="1" expression="BIT" />
                //</event>

                // <itemgroup name="Arm#01DoubleSubstrateLoadDataReportBlock">
                //  <item name="ForkFrontEndJobCassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ForkFrontEndJobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ForkBackEndJobCassetteSequenceNo" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ForkBackEndJobSequenceNo" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ArmNo" woffset="4" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="CurrentPosition" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>
                #endregion

                string eqpNo = inputData.Metadata.NodeNo;
                string strArmNo = inputData.Name.Split('#')[1].Substring(0, 2);// Split Current Trx Arm No By '#'
                string strTransactionGroup = string.Format("{0}_EG_Arm#{1}DoubleSubstrateLoadDataReport", eqpNo, strArmNo);//Event Group
                string strWEvent = string.Format("{0}_W_Arm#{1}DoubleSubstrateLoadDataReportBlock", eqpNo, strArmNo);//Event
                string strBEvent = string.Format("{0}_B_Arm#{1}DoubleSubstrateLoadDataReport", eqpNo, strArmNo);//Event

                string strItemForkFrontJobCstNO = "ForkFrontEndJobCassetteSequenceNo";
                string strItemForkFrontJobSlotNO = "ForkFrontEndJobSequenceNo";
                string strItemForkBackJobCstNO = "ForkBackEndJobCassetteSequenceNo";
                string strItemForkBackJobSlotNO = "ForkBackEndJobSequenceNo";

                string strItemArmNo = "ArmNo";
                string strItemCurrentPosition = "CurrentPosition";

                string strItemBitReport = string.Format("Arm#{0}DoubleSubstrateLoadDataReport", strArmNo);

                string strItemForkFrontJobCstNOValue = inputData[strTransactionGroup][strWEvent][strItemForkFrontJobCstNO].Value;
                string strItemForkFrontJobSlotNOValue = inputData[strTransactionGroup][strWEvent][strItemForkFrontJobSlotNO].Value;
                string strItemForkBackJobCstNOValue = inputData[strTransactionGroup][strWEvent][strItemForkBackJobCstNO].Value;
                string strItemForkBackJobSlotNOValue = inputData[strTransactionGroup][strWEvent][strItemForkBackJobSlotNO].Value;
                string strItemArmNoValue = inputData[strTransactionGroup][strWEvent][strItemArmNo].Value;
                string strItemCurrentPositionValue = inputData[strTransactionGroup][strWEvent][strItemCurrentPosition].Value.PadLeft(2, '0');

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[strTransactionGroup].Events[strBEvent].Items[strItemBitReport].Value);

                //20160121 ArmNo從strItemArmNoValue改成以目前TrxArmNo為準
                strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP][{1}] Double Substrate LoadReport Bit({2}) Front CassetteSequenceNo({3}) JobSequenceNo({4}) Back CassetteSequenceNo({5}) JobSequenceNo({6}) UseArmNo({7}) CurPosition({8}).",
                                        inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), strItemForkFrontJobCstNOValue,
                                        strItemForkFrontJobSlotNOValue, strItemForkBackJobCstNOValue, strItemForkBackJobSlotNOValue, strArmNo, 
                                        strItemCurrentPositionValue);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #region BC Reply Setting

                if (triggerBit == eBitResult.OFF)
                {
                    //Reply SingleSubstrateLoadDataReport
                    DoubleSubstrateLoadDataReportReply(eqpNo, strArmNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                //Reply SingleSubstrateLoadDataReport
                DoubleSubstrateLoadDataReportReply(eqpNo, strArmNo, eBitResult.ON, inputData.TrackKey);

                #endregion         
                
                #endregion

                //20151202 add For Double Glass
                #region [ Get Robot by EQPNO ]

                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);
                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Can't find Robot by EqpNo({2}) in RobotEntity!",
                                            eqpNo, inputData.TrackKey, eqpNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                #endregion

                //通知OPI顯示履歷 ArmNo從strItemArmNoValue改成以目前TrxArmNo為準
                cmdMsg = string.Format("{0} - LoadEvent, Arm#{1}=Front[{2},{3}], Back[{4},{5}] RobotPosition={6}, CommandStatus={7}",
                                        "ArmSubstrateLoadEventReport".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), strArmNo,
                                        strItemForkFrontJobCstNOValue, strItemForkFrontJobSlotNOValue, strItemForkBackJobCstNOValue, strItemForkBackJobSlotNOValue, 
                                        strItemCurrentPositionValue, curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.NormalType });

                //記錄到Trace Log
                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #region [ Get CurStage Entity ]

                RobotStage curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(strItemCurrentPositionValue);
                
                if (curStage == null)
                {
                
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Current Stage by Current Position Value({2})!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, strItemCurrentPositionValue);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                }

                #endregion
                
                //Watson Add 20151027 Save Stage Last Glass Send out Date Time
                curStage.File.OutputDateTime = DateTime.Now;  //Stage 最後出片的時間
                ObjectManager.RobotStageManager.EnqueueSave(curStage.File);

                #region [ Check 不可以都沒有Job資訊 ]

                if (strItemForkFrontJobCstNOValue == "0" && strItemForkFrontJobSlotNOValue == "0" && strItemForkBackJobCstNOValue == "0" && strItemForkBackJobSlotNOValue == "0")
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Arm Front CSTSeq({2}),JobSeq({3}) and Back CSTSeq({4}),JobSeq({5}) is can not Find BCSJob!",
                                                eqpNo, MethodBase.GetCurrentMethod().Name, strItemForkFrontJobCstNOValue, strItemForkFrontJobSlotNOValue,
                                                strItemForkBackJobCstNOValue, strItemForkBackJobSlotNOValue);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return;

                }

                #endregion

                #region [ Get Arm Front BcsJob Entity and Update StepIngo ]

                Job curArmFrontJob = ObjectManager.JobManager.GetJob(strItemForkFrontJobCstNOValue, strItemForkFrontJobSlotNOValue);

                //找不到 Front BcsJob 回NG
                if (curArmFrontJob == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob by Arm Front CSTSeq({2}),JobSeq({3})!",
                                            eqpNo, MethodBase.GetCurrentMethod().Name, strItemForkFrontJobCstNOValue, strItemForkFrontJobSlotNOValue);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
  
                }
                else
                {

                    //20151031 add Job移動時要清空所有的CheckFailMsg
                    RemoveJobAllCheckFailMsg(curArmFrontJob);

                    //執行Front Step Result的設定 //20160121改成以目前TrxArmNo為準
                    Invoke(eServiceName.RobotCoreService, "HandleJobProcessResult", new object[] { curRobot, curArmFrontJob, strArmNo, string.Empty, string.Empty, string.Empty });
                    //Invoke(eServiceName.RobotCoreService, "HandleJobProcessResult", new object[] { curRobot, curArmFrontJob, strItemArmNoValue, string.Empty, string.Empty, string.Empty });
                }

                #endregion          

                #region [ Get Arm Back BcsJob Entity and Update StepIngo ]

                Job curArmBackJob = ObjectManager.JobManager.GetJob(strItemForkBackJobCstNOValue, strItemForkBackJobSlotNOValue);

                //找不到 Back BcsJob 回NG
                if (curArmBackJob == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob by Arm Back CSTSeq({2}),JobSeq({3})!",
                                            eqpNo, MethodBase.GetCurrentMethod().Name, strItemForkBackJobCstNOValue, strItemForkBackJobSlotNOValue);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }
                else
                {
                    RemoveJobAllCheckFailMsg(curArmBackJob);

                    //執行Back Step Result的設定 //20160121改成以目前TrxArmNo為準
                    Invoke(eServiceName.RobotCoreService, "HandleJobProcessResult", new object[] { curRobot, curArmBackJob, strArmNo, string.Empty, string.Empty, string.Empty });
                    //Invoke(eServiceName.RobotCoreService, "HandleJobProcessResult", new object[] { curRobot, curArmBackJob, strItemArmNoValue, string.Empty, string.Empty, string.Empty });

                }

                #endregion       

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void DoubleSubstrateLoadDataReportReply(string eqpNo, string ArmNo, eBitResult value, string trackKey)
        {
            try
            {
                string strlog = string.Empty;
                string DoubleSubstrateLoadDataReportTimeOut = "DoubleSubstrateLoadDataReportTimeOut";

                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + string.Format("_Arm#{0}DoubleSubstrateLoadDataReportReply", ArmNo)) as Trx;

                string strTransactionGroup = string.Format("{0}_EG_Arm#{1}DoubleSubstrateLoadDataReportReply", eqpNo, ArmNo);
                string strTransactionEvent = string.Format("{0}_B_Arm#{1}DoubleSubstrateLoadDataReportReply", eqpNo, ArmNo);
                string strItemReportReply = string.Format("Arm#{0}DoubleSubstrateLoadDataReportReply", ArmNo);

                outputdata.EventGroups[strTransactionGroup].Events[strTransactionEvent].Items[strItemReportReply].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ArmNo + "_" + DoubleSubstrateLoadDataReportTimeOut))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ArmNo + "_" + DoubleSubstrateLoadDataReportTimeOut);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ArmNo + "_" + DoubleSubstrateLoadDataReportTimeOut, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DoubleSubstrateLoadDataReportReplyTimeOut), trackKey);
                }

                strlog = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Arm Double Substrate Load Report Reply Set Bit ({2}).", eqpNo, trackKey, value.ToString());

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void DoubleSubstrateLoadDataReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string strlog = string.Empty;
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                strlog = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Equipment Status Change Reply Timeout Report Timeout Set Bit (OFF).", sArray[0], trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                DoubleSubstrateLoadDataReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region [DoubleSubstrateUnloadDataReport]

        public void DoubleSubstrateUnloadDataReport(Trx inputData)
        {
            string strlog = string.Empty;
            string cmdMsg = string.Empty;

            try
            {

                #region [拆出PLCAgent Data]

                #region [Trx Structure]

                //<trx name="L2_Arm#01DoubleSubstrateUnloadDataReport" triggercondition="change">
                //  <eventgroup name="L2_EG_Arm#01DoubleSubstrateUnloadDataReport" dir="E2B">
                //    <event name="L2_W_Arm#01DoubleSubstrateUnloadDataReportBlock" />
                //    <event name="L2_B_Arm#01DoubleSubstrateUnloadDataReport" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<event name="L2_B_Arm#01DoubleSubstrateUnloadDataReport" devicecode="B" address="0x00008ED" points="1">
                //  <item name="Arm#01DoubleSubstrateUnloadDataReport" offset="0" points="1" expression="BIT" />
                //</event>

                // <itemgroup name="Arm#01DoubleSubstrateUnloadDataReportBlock">
                //  <item name="ForkFrontEndJobCassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ForkFrontEndJobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ForkBackEndJobCassetteSequenceNo" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ForkBackEndJobSequenceNo" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ArmNo" woffset="4" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="CurrentPosition" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>
                #endregion

                string eqpNo = inputData.Metadata.NodeNo;
                string strArmNo = inputData.Name.Split('#')[1].Substring(0, 2);// Split Current Trx Arm No By '#'
                string strTransactionGroup = string.Format("{0}_EG_Arm#{1}DoubleSubstrateUnloadDataReport", eqpNo, strArmNo);//Event Group
                string strWEvent = string.Format("{0}_W_Arm#{1}DoubleSubstrateUnloadDataReportBlock", eqpNo, strArmNo);//Event
                string strBEvent = string.Format("{0}_B_Arm#{1}DoubleSubstrateUnloadDataReport", eqpNo, strArmNo);//Event

                string strItemForkFrontJobCstNO = "ForkFrontEndJobCassetteSequenceNo";
                string strItemForkFrontJobSlotNO = "ForkFrontEndJobSequenceNo";
                string strItemForkBackJobCstNO = "ForkBackEndJobCassetteSequenceNo";
                string strItemForkBackJobSlotNO = "ForkBackEndJobSequenceNo";

                string strItemArmNo = "ArmNo";
                string strItemCurrentPosition = "CurrentPosition";

                string strItemBitReport = string.Format("Arm#{0}DoubleSubstrateUnloadDataReport", strArmNo);

                string strItemForkFrontJobCstNOValue = inputData[strTransactionGroup][strWEvent][strItemForkFrontJobCstNO].Value;
                string strItemForkFrontJobSlotNOValue = inputData[strTransactionGroup][strWEvent][strItemForkFrontJobSlotNO].Value;
                string strItemForkBackJobCstNOValue = inputData[strTransactionGroup][strWEvent][strItemForkBackJobCstNO].Value;
                string strItemForkBackJobSlotNOValue = inputData[strTransactionGroup][strWEvent][strItemForkBackJobSlotNO].Value;
                string strItemArmNoValue = inputData[strTransactionGroup][strWEvent][strItemArmNo].Value;
                
                //Position要補足2碼否則會取不到
                string strItemCurrentPositionValue = inputData[strTransactionGroup][strWEvent][strItemCurrentPosition].Value.PadLeft(2, '0');

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[strTransactionGroup].Events[strBEvent].Items[strItemBitReport].Value);

                //20160121 ArmNo從strItemArmNoValue改成以目前TrxArmNo為準
                strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP][{1}] Double Substrate UnloadReport Bit({2}) Front CassetteSequenceNo({3}) JobSequenceNo({4}) Back CassetteSequenceNo({5}) JobSequenceNo({6})UseArmNo({7}) CurPosition({8}).",
                                        inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), strItemForkFrontJobCstNOValue,
                                        strItemForkFrontJobSlotNOValue, strItemForkBackJobCstNOValue, strItemForkBackJobSlotNOValue, strArmNo, 
                                        strItemCurrentPositionValue);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #region BC Reply Setting

                if (triggerBit == eBitResult.OFF)
                {
                    //Reply SingleSubstrateLoadDataReport
                    DoubleSubstrateUnloadDataReportReply(eqpNo, strArmNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                //Reply SingleSubstrateLoadDataReport
                DoubleSubstrateUnloadDataReportReply(eqpNo, strArmNo, eBitResult.ON, inputData.TrackKey);

                #endregion

                #endregion

                #region [ Get Robot by EQPNO ]

                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);
                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Can't find Robot by EqpNo({2}) in RobotEntity!",
                                            eqpNo, inputData.TrackKey, eqpNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                #endregion

                //通知OPI更新RobotCommand狀態 下Robot Load Job EX:LoadEvent,Upper=[0,0],Lower=[3263,30],Arm=LOWER,RobotPosition=4
                //20160121 ArmNo從strItemArmNoValue改成以目前TrxArmNo為準
                cmdMsg = string.Format("{0} - UnloadEvent, Arm#{1}=Front[{2},{3}], Back[{4},{5}] RobotPosition={6}, CommandStatus={7}",
                                        "ArmSubstrateUnloadEventReport".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), strArmNo,
                                        strItemForkFrontJobCstNOValue, strItemForkFrontJobSlotNOValue, strItemForkBackJobCstNOValue, strItemForkBackJobSlotNOValue, 
                                        strItemCurrentPositionValue, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.NormalType });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #region [ Get CurStage Entity ]

                RobotStage curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(strItemCurrentPositionValue);

                if (curStage == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Current Stage by Current Position Value({2})!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, strItemCurrentPositionValue);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                }

                #endregion

                //Watson Add 20151027 Save Stage Last Glass Receive Date Time
                curStage.File.InputDateTime = DateTime.Now;  //Stage 最後出片的時間
                ObjectManager.RobotStageManager.EnqueueSave(curStage.File);

                #region [ Check 不可以都沒有Job資訊 ]

                if (strItemForkFrontJobCstNOValue == "0" && strItemForkFrontJobSlotNOValue == "0" && strItemForkBackJobCstNOValue == "0" && strItemForkBackJobSlotNOValue == "0")
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Arm Front CSTSeq({2}),JobSeq({3}) and Back CSTSeq({4}),JobSeq({5}) is can not Find BCSJob!",
                                                eqpNo, MethodBase.GetCurrentMethod().Name, strItemForkFrontJobCstNOValue, strItemForkFrontJobSlotNOValue,
                                                strItemForkBackJobCstNOValue, strItemForkBackJobSlotNOValue);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return;

                }

                #endregion

                #region [ Get Arm BcsJob Entity and Update StepInfo ]

                Job curFrontArmJob = ObjectManager.JobManager.GetJob(strItemForkFrontJobCstNOValue, strItemForkFrontJobSlotNOValue);

                //找不到 BcsJob 回NG
                if (curFrontArmJob == null)
                {
                   
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob by Arm Front CSTSeq({2}),JobSeq({3})!",
                                            eqpNo, MethodBase.GetCurrentMethod().Name, strItemForkFrontJobCstNOValue, strItemForkFrontJobSlotNOValue);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                }
                else
                {
                    //20151031 add Job移動時要清空所有的CheckFailMsg
                    RemoveJobAllCheckFailMsg(curFrontArmJob);

                    //執行Step Result的設定 //20160121改成以目前TrxArmNo為準
                    Invoke(eServiceName.RobotCoreService, "HandleJobProcessResult", new object[] { curRobot, curFrontArmJob, string.Empty, strArmNo, strItemCurrentPositionValue, strItemForkFrontJobSlotNOValue });
                    //Invoke(eServiceName.RobotCoreService, "HandleJobProcessResult", new object[] { curRobot, curFrontArmJob, string.Empty, strItemArmNoValue, strItemCurrentPositionValue, strItemForkFrontJobSlotNOValue });
                }

                #endregion

                #region [ Get Arm BcsJob Entity and Update StepInfo ]

                Job curBackArmJob = ObjectManager.JobManager.GetJob(strItemForkBackJobCstNOValue, strItemForkBackJobSlotNOValue);

                //找不到 BcsJob 回NG
                if (curBackArmJob == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob by Arm Back CSTSeq({2}),JobSeq({3})!",
                                            eqpNo, MethodBase.GetCurrentMethod().Name, strItemForkBackJobCstNOValue, strItemForkBackJobSlotNOValue);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                }
                else
                {
                    //20151031 add Job移動時要清空所有的CheckFailMsg
                    RemoveJobAllCheckFailMsg(curBackArmJob);

                    //執行Step Result的設定 //20160121改成以目前TrxArmNo為準
                    Invoke(eServiceName.RobotCoreService, "HandleJobProcessResult", new object[] { curRobot, curBackArmJob, string.Empty, strArmNo, strItemCurrentPositionValue, strItemForkBackJobSlotNOValue });
                    //Invoke(eServiceName.RobotCoreService, "HandleJobProcessResult", new object[] { curRobot, curBackArmJob, string.Empty, strItemArmNoValue, strItemCurrentPositionValue, strItemForkBackJobSlotNOValue });

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void DoubleSubstrateUnloadDataReportReply(string eqpNo, string ArmNo, eBitResult value, string trackKey)
        {
            try
            {
                string strlog = string.Empty;
                string DoubleSubstrateUnloadDataReportTimeOut = "DoubleSubstrateUnloadDataReportTimeOut";

                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + string.Format("_Arm#{0}DoubleSubstrateUnloadDataReportReply", ArmNo)) as Trx;

                string strTransactionGroup = string.Format("{0}_EG_Arm#{1}DoubleSubstrateUnloadDataReportReply", eqpNo, ArmNo);
                string strTransactionEvent = string.Format("{0}_B_Arm#{1}DoubleSubstrateUnloadDataReportReply", eqpNo, ArmNo);
                string strItemReportReply = string.Format("Arm#{0}DoubleSubstrateUnloadDataReportReply", ArmNo);

                outputdata.EventGroups[strTransactionGroup].Events[strTransactionEvent].Items[strItemReportReply].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ArmNo + "_" + DoubleSubstrateUnloadDataReportTimeOut))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ArmNo + "_" + DoubleSubstrateUnloadDataReportTimeOut);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ArmNo + "_" + DoubleSubstrateUnloadDataReportTimeOut, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DoubleSubstrateLoadDataReportReplyTimeOut), trackKey);
                }

                strlog = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Arm Double Substrate Unload Report Reply Set Bit ({2}).", eqpNo, trackKey, value.ToString());

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void DoubleSubstrateUnloadDataReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string strlog = string.Empty;
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                strlog = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Equipment Status Change Reply Timeout Report Timeout Set Bit (OFF).", sArray[0], trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                DoubleSubstrateLoadDataReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region 與OPI之間的相關Function ====================================================================================================================================================

        /// <summary> Robot Status Service:OPI Send Robot Mode Change Request
        ///
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="eqpNo"></param>
        /// <param name="robotName"></param>
        /// <param name="robotRunMode">"SEMI","AUTO"</param>
        /// <returns></returns>
        public bool RobotModeChangeRequest(string trxID, string eqpNo, string robotName, string robotRunMode)
        {
            try
            {

                string strlog = string.Empty;
                string oldMode = string.Empty;

                if (robotRunMode != eRobot_RunMode.AUTO_MODE && robotRunMode != eRobot_RunMode.SEMI_MODE)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot ({2}) RunMode Change Fail ! Can't find Robot RunMode ({3}) define !",
                                            eqpNo, trxID, robotName, robotRunMode);

                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return false;
                }

                //Get Robot by RobotName
                Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(robotName);

                if (curRobot == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot ({2}) RunMode Change Fail ! Can't find Robot by RobotName ({3}) in RobotEntity!",
                                            eqpNo, trxID, robotName, robotName);

                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return false;
                }

                //[ Wait_Proc_0018 ] 後續處理 add for Robot Run Mode Change to Clear Robot Fail ReasonCode List
                //RobotClearAllFailInfo(curRobot);

                oldMode = curRobot.File.curRobotRunMode;

                //更新資料
                lock (curRobot.File)
                {

                    curRobot.File.curRobotRunMode = robotRunMode;

                }

                //存入Robot File
                ObjectManager.RobotManager.EnqueueSave(curRobot.File);

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot({2}) RunMode Change from ({3}) to ({4}) ",
                                        eqpNo, trxID, curRobot.Data.ROBOTNAME, oldMode, robotRunMode);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                //report OPI Robot Run Mode is Change
                Invoke(eServiceName.UIService, "RobotCurrentModeReport", new object[] { curRobot });

                //add for Log Trace
                strlog = string.Format("[{0}] {1} - Robot({2}) RunMode Change From ({3}) to ({4}).",
                                        "RobotStatusService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), 
                                        "RobotCurrentModeReport".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                        curRobot.Data.ROBOTNAME, oldMode, robotRunMode);

                Logger.LogTrxWrite(this.LogName, strlog);

                return true;

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary> Robot Status Service:OPI Send Robot Same EQ Flag Change Request
        ///
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="eqpNo"></param>
        /// <param name="robotName"></param>
        /// <param name="robotRunMode">"SEMI","AUTO"</param>
        /// <returns></returns>
        public bool RobotJobSendToSameEQRequest(string trxID, string eqpNo, string robotName, string sameEQFlag)
        {
            try
            {

                string strlog = string.Empty;
                string oldMode = string.Empty;

                if (sameEQFlag != eRobot_SameEQFlag.NO && sameEQFlag != eRobot_SameEQFlag.YES)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot ({2}) RunMode Change Fail ! Can't find Robot RunMode ({3}) define !",
                                            eqpNo, trxID, robotName, sameEQFlag);

                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return false;
                }

                //Get Robot by RobotName
                Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(robotName);

                if (curRobot == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot ({2}) RunMode Change Fail ! Can't find Robot by RobotName ({3}) in RobotEntity!",
                                            eqpNo, trxID, robotName, robotName);

                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return false;
                }

                //[ Wait_Proc_0018 ] 後續處理 add for Robot Run Mode Change to Clear Robot Fail ReasonCode List
                //RobotClearAllFailInfo(curRobot);

                oldMode = curRobot.File.curRobotRunMode;

                //更新資料
                lock (curRobot.File)
                {

                    curRobot.File.curRobotSameEQFlag = sameEQFlag;

                }

                //存入Robot File
                ObjectManager.RobotManager.EnqueueSave(curRobot.File);

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot({2}) RunMode Change from ({3}) to ({4}) ",
                                        eqpNo, trxID, curRobot.Data.ROBOTNAME, oldMode, sameEQFlag);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                //report OPI Robot Run Mode is Change
                Invoke(eServiceName.UIService, "RobotCurrentModeReport", new object[] { curRobot });

                //add for Log Trace
                strlog = string.Format("[{0}] {1} - Robot({2}) RunMode Change From ({3}) to ({4}).",
                                        "RobotStatusService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                        "RobotCurrentModeReport".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                        curRobot.Data.ROBOTNAME, oldMode, sameEQFlag);

                Logger.LogTrxWrite(this.LogName, strlog);

                return true;

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary> Robot Status Service:OPI Send Robot Entity Change Request
        ///
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotRequest()
        {
            string strlog = string.Empty;

            try
            {
                //重新Load 各DB Robot 設定 [ Wait_Proc_0020 ] 後續處理
                //ObjectManager.RobotManage.RobotStageManager.ReloadRobotStage();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Entity!","L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> Robot Status Service:OPI Send Robot Stage Entity Change Request
        ///
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotStagRequest()
        {
            string strlog = string.Empty;

            try
            {
                //重新Load 各DB Robot Stage設定
                ObjectManager.RobotStageManager.ReloadRobotStage();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Stage!", "L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> Robot Status Service:OPI Send Robot Route Condition Entity Change Request
        ///
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotRouteConditionRequest()
        {
            string strlog = string.Empty;

            try
            {
                //重新Load 各DB Robot Route Condition設定
                ObjectManager.RobotManager.ReloadRobotRouteCondition();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Route Condition!", "L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> Robot Status Service:OPI Send Robot Route Step Entity Change Request
        ///
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotRouteStepRequest()
        {
            string strlog = string.Empty;

            try
            {
                //重新Load 各DB Robot RouteStep設定
                ObjectManager.RobotManager.ReloadALLRobotRouteStep();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Route Step!", "L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> Robot Status Service:OPI Send Robot Route Master Entity Change Request
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotRouteMasterRequest()
        {
            string strlog = string.Empty;

            try
            {
                //重新Load 各DB Robot Route Master設定
                ObjectManager.RobotManager.ReloadAllRobotRouteMaster();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Route Master!", "L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> Robot Status Service:OPI Send Robot Rule Select Entity Change Request
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotRuleSelectRequest()
        {
            string strlog = string.Empty;

            try
            {
                //重新Load 各DB Robot Rule Select設定
                ObjectManager.RobotManager.ReloadAllRobotRuleSelect();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Rule Select!", "L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> Robot Status Service:OPI Send Robot Rule Filter Entity Change Request
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotRuleFilterRequest()
        {
            string strlog = string.Empty;

            try
            {
                //重新Load 各DB Robot Rule Filter設定
                ObjectManager.RobotManager.ReloadAllRobotRuleFilter();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Rule Filter!", "L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> Robot Status Service:OPI Send Robot Rule Stage Select Entity Change Request
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotRuleStageSelectRequest()
        {
            string strlog = string.Empty;

            try
            {
                //重新Load 各DB Robot Rule StageSelect設定
                ObjectManager.RobotManager.ReloadAllRobotRuleStageSelect();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Rule Filter!", "L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> Robot Status Service:OPI Send Robot Process Result Entity Change Request
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotRouteResultHandleRequest()
        {
            string strlog = string.Empty;

            try
            {
                //重新Load 各DB Robot Route Result Handle設定
                ObjectManager.RobotManager.ReloadAllRobotRouteResultHandle();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Route Result Handle!", "L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> Robot Status Service:OPI Send Robot Rule RouteStepByPass Entity Change Request
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotRuleRouteStepByPassRequest()
        {
            string strlog = string.Empty;

            try
            {
                //重新Load 各DB Robot Rule RouteStepByPass設定
                ObjectManager.RobotManager.ReloadAllRobotRuleRouteStepByPass();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Rule RouteStepByPass!", "L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> Robot Status Service:OPI Send Robot Rule RouteStepJump Entity Change Request
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotRuleRouteStepJumpRequest()
        {
            string strlog = string.Empty;

            try
            {
                //重新Load 各DB Robot Rule RouteStepByPass設定
                ObjectManager.RobotManager.ReloadAllRobotRuleRouteStepJump();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Rule RouteStepJump!", "L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> Robot Status Service:OPI Send Robot Hold Status Change Request
        ///
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="eqpNo"></param>
        /// <param name="robotName"></param>
        /// <param name="robotHoldStatus">"SEMI","AUTO"</param>
        /// <returns></returns>
        public bool RobotHoldStatusChangeRequest(string trxID, string eqpNo, string robotName, string robotHoldStatus)
        {
            try
            {

                string strlog = string.Empty;
                string oldHoldStatus = string.Empty;

                if (robotHoldStatus != eRobot_HoldStatus.HOLD_STATUS && robotHoldStatus != eRobot_HoldStatus.RELEASE_STATUS)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot ({2}) Hold Status Change Fail ! Can't find Robot Hold Status ({3}) define !",
                                            eqpNo, trxID, robotName, robotHoldStatus);

                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return false;
                }

                //Get Robot by RobotName
                Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(robotName);

                if (curRobot == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot ({2}) Hold Status Change Fail ! Can't find Robot by RobotName ({3}) in RobotEntity!",
                                            eqpNo, trxID, robotName, robotName);

                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return false;
                }

                oldHoldStatus = curRobot.File.CurRobotHoldStatus;

                //更新資料
                lock (curRobot.File)
                {

                    curRobot.File.CurRobotHoldStatus = robotHoldStatus;

                }

                //存入Robot File
                ObjectManager.RobotManager.EnqueueSave(curRobot.File);

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot({2}) Hold Status Change from ({3}) to ({4}) ",
                                        eqpNo, trxID, curRobot.Data.ROBOTNAME, oldHoldStatus, robotHoldStatus);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                //report OPI Robot Run Mode is Change
                Invoke(eServiceName.UIService, "RobotCurrentModeReport", new object[] { curRobot });

                //add for Log Trace
                strlog = string.Format("[{0}] {1} - Robot({2}) Hold Status Change From ({3}) to ({4}).",
                                        "RobotStatusService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                        "RobotCurrentModeReport".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                        curRobot.Data.ROBOTNAME, oldHoldStatus, robotHoldStatus);

                Logger.LogTrxWrite(this.LogName, strlog);

                //通知EQP Robot Hold Status Change
                RobotControlCommandHoldStatus(curRobot, robotHoldStatus);

                return true;

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        private void RobotControlCommandHoldStatus(Robot curRobot, string holdFlag)
        {

            try
            {

                string eqpNo = curRobot.Data.NODENO;
                Equipment eqp;
                string strlog = string.Empty;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBM] Robot({1}) can not find EquipmentNo({2}) in EquipmentEntity!",
                                                            eqp, curRobot.Data.ROBOTNAME, eqp);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return;

                }

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBM] CIM Mode(OFF),can not send Robot Hold Status!", eqp.Data.NODENO);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return ;
                }

                string trxName = string.Format("{0}_RobotControlCommandHoldStatus", eqpNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                #region [Trx Structure]
                //<eventgroup name="L2_EG_RobotControlCommandHoldStatus" dir="B2E">
                //  <event name="L2_B_RobotControlCommandHoldStatus" trigger="true" />
                //</eventgroup>
                //<event name="L2_B_RobotControlCommandHoldStatus" devicecode="B" address="0x000028A" points="1">
                //  <item name="RobotControlCommandHoldStatus" offset="0" points="1" expression="BIT" />
                //</event>
                #endregion

                string strRobotControlCommandHoldEventGroup = "{0}_EG_RobotControlCommandHoldStatus";
                string strRobotControlCommandHoldBEvent = "{0}_B_RobotControlCommandHoldStatus";

                string strNodeNo = curRobot.Data.NODENO;
                string strEventGroup = string.Format(strRobotControlCommandHoldEventGroup, eqpNo);//Event Group;
                string strBEvent = string.Format(strRobotControlCommandHoldBEvent, eqpNo);//Event

                string strItemBitCommand = "RobotControlCommandHoldStatus";
                string tmpStatus = string.Empty;

                if (holdFlag == eRobot_HoldStatus.HOLD_STATUS)
                {
                    outputdata.EventGroups[strEventGroup].Events[strBEvent].Items[strItemBitCommand].Value = ((int)eBitResult.ON).ToString();
                    tmpStatus = eBitResult.ON.ToString();
                }
                else
                {
                    outputdata.EventGroups[strEventGroup].Events[strBEvent].Items[strItemBitCommand].Value = ((int)eBitResult.OFF).ToString();
                    tmpStatus = eBitResult.OFF.ToString();
                }

                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();

                SendPLCData(outputdata);     

                strlog = string.Format("[EQUIPMENT={0}] [RCS -> EQP][{1}] Set Robot({2}) Hold Status({3})!",
                                        curRobot.Data.NODENO, outputdata.TrackKey, curRobot.Data.ROBOTNAME, tmpStatus);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }

        //20151030 add Robot Status Service:OPI Send Change Job CurStep/Next Step Change Request
        public string OPIChangeStepID(Job curBcsJob, string newCurStepID, string newNextStepID)
        {
            string errMsg = string.Empty;
            string strlog = string.Empty;
            int intNewCurStepNo=0;
            int intNewNextStepNo=0;
            bool curStepNochangeFlag = false;
            bool nextStepNochangeFlag = false;

            try
            {
                int.TryParse(newCurStepID , out intNewCurStepNo);
                int.TryParse(newNextStepID, out intNewNextStepNo);

                #region [ Check NewCurStepID Change ]

                if (curBcsJob.RobotWIP.CurStepNo != intNewCurStepNo)
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(intNewCurStepNo) == true)
                    {

                        string newProcStatus = curBcsJob.RobotWIP.RouteProcessStatus;

                        //New StepNo is Exist
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI] Job CassetteSequenceNo({1}) JobSequenceNo({2}) curStepNo({3}) Change to NewCurStepNo({4}), ",
                                               "L2", curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString(), intNewCurStepNo);                       

                        if (intNewCurStepNo == 1)
                        {
                            strlog = strlog + string.Format("CurRouteProcessStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.WAIT_PROC);

                            newProcStatus = eRobot_RouteProcessStatus.WAIT_PROC;

                        }
                        else if (intNewCurStepNo >= 65535)
                        {
                            strlog = strlog + string.Format("CurRouteProcessStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.COMPLETE);
                            newProcStatus = eRobot_RouteProcessStatus.COMPLETE;
                        }
                        else
                        {
                            strlog = strlog + string.Format("CurJobStatus form ({0}) to ({1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.INPROCESS);

                            newProcStatus = eRobot_RouteProcessStatus.INPROCESS;

                        }

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = intNewCurStepNo;
                            curBcsJob.RobotWIP.RouteProcessStatus = newProcStatus;
                        }

                        curStepNochangeFlag = true;
                    }
                    else if (intNewCurStepNo >= 65535)
                    {

                        //New StepNo is Exist
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI] Job CassetteSequenceNo({1}) JobSequenceNo({2}) curStepNo({3}) Change to NewCurStepNo({4}), CurRouteProcessStatus form ({5}) to ({6}) ",
                                               "L2", curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString(), intNewCurStepNo, 
                                               curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.COMPLETE);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = intNewCurStepNo;
                            curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.COMPLETE;
                        }

                        curStepNochangeFlag = true;

                    }
                    else
                    {
                        //New StepNo not Exist
                        errMsg = string.Format("NewCurStep({0}) is not Exist.", intNewCurStepNo.ToString());
                        return errMsg;
                    }

                }

                #endregion

                #region [ Check NewNextStepID Change ]

                if (curBcsJob.RobotWIP.NextStepNo != intNewNextStepNo)
                {
                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(intNewNextStepNo) == true)
                    {
                        //New StepNo is Exist
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI] Job CassetteSequenceNo({1}) JobSequenceNo({2}) NextStepNo({3}) Change to NewNextStepNo({4}).",
                                               "L2", curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.NextStepNo.ToString(), intNewNextStepNo);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = intNewNextStepNo;
                        }

                        nextStepNochangeFlag = true;
                        
                    }
                    else if (intNewNextStepNo >= 65535)
                    {
                        //New StepNo is Exist
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI] Job CassetteSequenceNo({1}) JobSequenceNo({2}) NextStepNo({3}) Change to NewNextStepNo({4}).",
                                               "L2", curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.NextStepNo.ToString(), intNewNextStepNo);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = intNewNextStepNo;
                        }

                        nextStepNochangeFlag = true;
                    }
                    else
                    {
                        //New StepNo not Exist
                        errMsg = string.Format("NewNextStepNo({0}) is not Exist.", intNewNextStepNo.ToString());
                        return errMsg;
                    }

                }

                #endregion

                if (curStepNochangeFlag == true || nextStepNochangeFlag ==true)
                {
                    //Save RobobJob Info
                    ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    //Clear Job ChekFailMsg
                    RemoveJobAllCheckFailMsg(curBcsJob);
                }

                return errMsg;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return ex.Message;
            }

        }

        //20151201 add for Reloader OrderBy
        /// <summary> Robot Status Service:OPI Send Robot Rule OrderBy Entity Change Request
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReloadRobotRuleOrderByRequest()
        {
            string strlog = string.Empty;

            try
            {
                //20151201 add for Reloader OrderBy
                ObjectManager.RobotManager.ReloadAllRobotRuleOrderBy();

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI]OPI set Robot Rule OrderBy!", "L2");

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        #endregion

    }
}
