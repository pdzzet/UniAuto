using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniRCS.Core;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    class RobotCommandService : AbstractRobotService
    {
        /// <summary>
        /// 紀錄是否顯示Debug Log
        /// </summary>
        private bool _showDeBugLogFlag = true;

        #region RobotControlCommand Transaction Define
        string strRobotControlCommandEventGroup = "{0}_EG_RobotControlCommand";
        string strRobotControlCommandWEvent = "{0}_W_RobotControlCommandBlock";
        string strRobotControlCommandBEvent = "{0}_B_RobotControlCommand";
        #endregion

        #region CellSpecialRobotControlCommand Transaction Define
        string strCellSpecialRobotControlCommandEventGroup = "{0}_EG_CellSpecialRobotControlCommand";
        string strCellSpecialRobotControlCommandWEvent = "{0}_W_CellSpecialRobotControlCommandBlock";
        string strCellSpecialRobotControlCommandBEvent = "{0}_B_CellSpecialRobotControlCommand";
        #endregion

        public override bool Init()
        {
            return true;
        }

        public void Destroy()
        {

        }

        #region RobotControlCommand ==========================================================================================================================================================

        public bool RobotControlCommandSend(Robot curRobot, RobotCmdInfo curCmd)
        {
            try
            {
                if (curRobot.File.CmdSendCondition) return false;  //yang 2017/4/18

                if (Workbench.LineType == eLineType.ARRAY.OVNSD_VIATRON &&
                    StaticContext.ContainsKey(eRobotContextParameter.TCOVN_SD_RobotParam) &&
                    StaticContext[eRobotContextParameter.TCOVN_SD_RobotParam] is TCOVN_SD_RobotParam)
                {
                    // 下 Robot Control Command, 將 OVN SD 用來控制 Get Get, Put Put 的等待時間歸零
                    ((TCOVN_SD_RobotParam)StaticContext[eRobotContextParameter.TCOVN_SD_RobotParam]).ResetDateTime();
                }
                else if (Workbench.LineType == eLineType.CF.FCSRT_TYPE1 &&
                    StaticContext.ContainsKey(eRobotContextParameter.FCSRT_RobotParam) &&
                    StaticContext[eRobotContextParameter.FCSRT_RobotParam] is FCSRT_RobotParam)
                {
                    // 下 Robot Control Command, 將 FCSRT VCR 用來控制 Get Get, Put Put 的等待時間歸零
                    ((FCSRT_RobotParam)StaticContext[eRobotContextParameter.FCSRT_RobotParam]).ResetDateTime();
                }
            }
            catch { }

            try
            {

                string eqpNo = curRobot.Data.NODENO;
                Equipment eqp;
                string strlog = string.Empty;


                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    #region  [DebugLog]

                    if (_showDeBugLogFlag == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBM] Robot({1}) can not find EquipmentNo({2}) in EquipmentEntity!",
                                                                eqp, curRobot.Data.ROBOTNAME, eqp);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;

                }

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBM] CIM Mode(OFF),can not send Robot Control Command!", eqp.Data.NODENO);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return false;
                }
                string strNodeNo = curRobot.Data.NODENO;
                string strEventGroup = string.Format(strRobotControlCommandEventGroup, eqpNo);//Event Group;
                string strWEvent = string.Format(strRobotControlCommandWEvent, eqpNo);//Event
                string strBEvent = string.Format(strRobotControlCommandBEvent, eqpNo);//Event

                string strItemCmd01_Command = "RobotCommand#01";
                string strItemCmd01_ArmSelect = "ArmSelect#01";
                string strItemCmd01_TargetSlotNo = "TargetSlotNo#01";
                string strItemCmd01_TargetPosition = "TargetPosition#01";

                string strItemCmd02_Command = "RobotCommand#02";
                string strItemCmd02_ArmSelect = "ArmSelect#02";
                string strItemCmd02_TargetSlotNo = "TargetSlotNo#02";
                string strItemCmd02_TargetPosition = "TargetPosition#02";

                string strItemBitCommand = "RobotControlCommand";
                 //SEMI MODE 不需要check job 
                 //用curCmd的Job来做接下来的判断 20160902 钊祁,Yang
                 //Cmd1和Cmd2 的job 有可能不是同一片Job 
                 //mark by yang,2017/2/24 取job外移,后面丢对应片的error会用到
                if (curCmd.Cmd01_Command == eRobot_ControlCommand.GET_READY)//add by hujunpeng 20190527 for cvd300 get ready 12
                {
                    string trxName1 = string.Format("{0}_RobotControlCommand", eqpNo);

                    Trx outputdata1 = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName1) as Trx;
                    outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_Command].Value = curCmd.Cmd01_Command.ToString();
                    outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_ArmSelect].Value = curCmd.Cmd01_ArmSelect.ToString();
                    outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_TargetSlotNo].Value = curCmd.Cmd01_TargetSlotNo.ToString();
                    outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_TargetPosition].Value = curCmd.Cmd01_TargetPosition.ToString();

                    //20160504 modify 如果Cmd02 Action=0,則把ArmSelect,TargetSlotNo,TargetPosition都清成0,即不下Cmd  
                    if (curCmd.Cmd02_Command == 0)
                    {
                        outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_Command].Value = "0";
                        outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_ArmSelect].Value = "0";
                        outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_TargetSlotNo].Value = "0";
                        outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_TargetPosition].Value = "0";
                    }
                    else
                    {
                        outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_Command].Value = curCmd.Cmd02_Command.ToString();
                        outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_ArmSelect].Value = curCmd.Cmd02_ArmSelect.ToString();
                        outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_TargetSlotNo].Value = curCmd.Cmd02_TargetSlotNo.ToString();
                        outputdata1.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_TargetPosition].Value = curCmd.Cmd02_TargetPosition.ToString();
                    }
                    if (curCmd.Cmd01_Command != 0)  //20160624 在Arm上PutReady是Cmd01,避免重複,會清成NONE,不要下Command
                    {
                        outputdata1.EventGroups[strEventGroup].Events[strBEvent].Items[strItemBitCommand].Value = ((int)eBitResult.ON).ToString();
                        outputdata1.EventGroups[strEventGroup].Events[strBEvent].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                        outputdata1.TrackKey = UtilityMethod.GetAgentTrackKey();

                        if (curRobot.File.CmdSendCondition) return false; //yang 2017/6/6
                        SendPLCData(outputdata1);
                        #region [ add Monitor Robot Control Command TimeOut ]

                        string timeName = string.Format("{0}_{1}_{2}", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_TIMEOUT);

                        if (_timerManager.IsAliveTimer(timeName))
                        {
                            _timerManager.TerminateTimer(timeName);
                        }

                        _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(RobotControlCommandReplyTimeout), outputdata1.TrackKey);

                        #endregion

                        #region [ Robot Command Send後開始計算Robot Command Active TimeOut (Robot Status Idle -> Running) ]

                        string waitActivetimeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                        if (_timerManager.IsAliveTimer(waitActivetimeName))
                        {
                            _timerManager.TerminateTimer(waitActivetimeName);
                        }

                        //防止Config設定異常,預設5分鐘300000ms
                        int robotCmdActiveTimeOut = 300000;

                        try
                        {
                            robotCmdActiveTimeOut = ParameterManager[eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_CONSTANT_KEY].GetInteger();
                        }
                        catch (Exception ex1)
                        {
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex1);
                        }

                        _timerManager.CreateTimer(waitActivetimeName, false, robotCmdActiveTimeOut, new System.Timers.ElapsedEventHandler(RobotControlCommandActiveTimeout), outputdata1.TrackKey);

                        #endregion

                        #region [ Robot Command Send後開始計算Robot Command RT2 TimeOut(EQP Report RobotCommand Result report) ]

                        string waitRT2timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_RT2_TIMEOUT_EVENTNAME);

                        if (_timerManager.IsAliveTimer(waitRT2timeName))
                        {
                            _timerManager.TerminateTimer(waitRT2timeName);
                        }

                        //防止Config設定異常,預設300sec,300000ms
                        int robotCmdRT2TimeOut = 300000;

                        try
                        {
                            robotCmdRT2TimeOut = ParameterManager[eRobotCommonConst.ROBOT_CONTROL_COMMAND_RT2_TIMEOUT_CONSTANT_KEY].GetInteger();
                        }
                        catch (Exception ex1)
                        {
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex1);
                        }

                        _timerManager.CreateTimer(waitRT2timeName, false, robotCmdRT2TimeOut, new System.Timers.ElapsedEventHandler(RobotControlCommandRT2Timeout), outputdata1.TrackKey);

                        #endregion

                        strlog = string.Format("[EQUIPMENT={0}] [RCS -> EQP][{1}] {2} Mode Set RobotCommand#01({3}) ArmSelect({4}) TargetPos({5}) TargetSlotNo({6}) RobotCommand#02({7}) ArmSelect({8}) TargetPos({9}) TargetSlotNo({10}) Set Bit (ON)",
                                                curRobot.Data.NODENO, outputdata1.TrackKey, curRobot.File.curRobotRunMode,
                                                curCmd.Cmd01_Command, curCmd.Cmd01_ArmSelect, curCmd.Cmd01_TargetPosition, curCmd.Cmd01_TargetSlotNo.ToString(),
                                                curCmd.Cmd02_Command, curCmd.Cmd02_ArmSelect, curCmd.Cmd02_TargetPosition, curCmd.Cmd02_TargetSlotNo.ToString());


                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ Update Robot Command Status ]

                        if (curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus != eRobot_ControlCommandStatus.CREATE)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Change Robot Control Command Status from ({2}) to ({3}) !",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus, eRobot_ControlCommandStatus.CREATE);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            lock (curRobot)
                            {
                                curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus = eRobot_ControlCommandStatus.CREATE;

                                curRobot.CurRealTimeSetCommandInfo.CmdCreateDateTime = DateTime.Now;
                                curRobot.CurRealTimeSetCommandInfo.Cmd01_ArmSelect = curCmd.Cmd01_ArmSelect;
                                curRobot.CurRealTimeSetCommandInfo.Cmd01_Command = curCmd.Cmd01_Command;
                                curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetPosition = curCmd.Cmd01_TargetPosition;
                                curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetSlotNo = curCmd.Cmd01_TargetSlotNo;
                                //curRobot.CurRealTimeSetCommandInfo.Cmd01_CSTSeq = curCmd.Cmd01_CSTSeq;
                                //curRobot.CurRealTimeSetCommandInfo.Cmd01_JobSeq = curCmd.Cmd01_JobSeq;
                                //curRobot.CurRealTimeSetCommandInfo.Cmd01_JobKey = curCmd.Cmd01_JobKey;

                                curRobot.CurRealTimeSetCommandInfo.Cmd02_ArmSelect = curCmd.Cmd02_ArmSelect;
                                curRobot.CurRealTimeSetCommandInfo.Cmd02_Command = curCmd.Cmd02_Command;
                                curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition = curCmd.Cmd02_TargetPosition;
                                curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetSlotNo = curCmd.Cmd02_TargetSlotNo;
                                //curRobot.CurRealTimeSetCommandInfo.Cmd02_CSTSeq = curCmd.Cmd02_CSTSeq;
                                //curRobot.CurRealTimeSetCommandInfo.Cmd02_JobSeq = curCmd.Cmd02_JobSeq;
                                //curRobot.CurRealTimeSetCommandInfo.Cmd02_JobKey = curCmd.Cmd02_JobKey;

                                //Clear Cmd Result
                                curRobot.CurRealTimeSetCommandInfo.CmdResult01 = 0;
                                curRobot.CurRealTimeSetCommandInfo.CmdResult02 = 0;
                                curRobot.CurRealTimeSetCommandInfo.CmdResult_CurPosition = 0;

                                //Set EQ Reply Status
                                curRobot.CurRealTimeSetCommandInfo.CmdEQReply = "WaitReply";

                            }

                            //Real Time Data 不需要Save File

                            //Send OPI Cmd Msg 20150830 Work end 注意要參考T2來修正
                            SendRobotCommandInfoMessageToOPI(curRobot, "RobotControlCommandSend");

                        }

                        #endregion
                        return true;
                    }
                }
                Job curCmd1Job = ObjectManager.JobManager.GetJob(curCmd.Cmd01_CSTSeq.ToString(), curCmd.Cmd01_JobSeq.ToString());
                Job curCmd2Job = new Job();
                if (curCmd.Cmd02_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    curCmd2Job= ObjectManager.JobManager.GetJob(curCmd.Cmd02_CSTSeq.ToString(), curCmd.Cmd02_JobSeq.ToString());
                if (curRobot.File.curRobotRunMode == eRobot_RunMode.AUTO_MODE)
                {
                    //20160815 如果Cmd01 Get的stage跟Cmd02 Put的stage是同一個,就不要下Cmd,避免設定或條件沒卡住,做了同stage的取又放
                    Line curline = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);                
                    
                    if (curCmd1Job == null ||curCmd2Job == null)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get Job", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                            if (curCmd1Job==null)
                                string.Format(" by  curCmd1Job({0},{1})", curCmd.Cmd01_CSTSeq.ToString(), curCmd.Cmd01_JobSeq.ToString());
                            if (curCmd.Cmd01_CSTSeq == curCmd.Cmd02_CSTSeq && curCmd.Cmd01_JobSeq == curCmd.Cmd01_JobSeq && curCmd2Job == null)
                                strlog += string.Format("by  curCmd1Job({0},{1})", curCmd.Cmd02_CSTSeq, curCmd.Cmd02_JobSeq);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        } 
                        #endregion
                       
                        return false;
                    }
                    string fail_ReasonCode = string.Empty;
                    string failMsg = string.Empty;
                    #region [Cm01 Get Position 與 Cm02 Put Position一樣 代表都是同一個stage,則不下Command]
                    if (curCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET && curCmd.Cmd02_Command == eRobot_Trx_CommandAction.ACTION_PUT)
                    {
                        fail_ReasonCode = eJob_CheckFail_Reason.Job_FromStage_And_TargetStage_Are_Same_Fail;
                        if (curCmd.Cmd01_TargetPosition == curCmd.Cmd02_TargetPosition)
                        {
                            if (!curCmd1Job.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                            {
                                strlog = string.Format("[{0}] Cmd01:Job({1}_{2}) Form stage({3}) and Target stage({4}) are can't Same!", MethodBase.GetCurrentMethod().Name,
                                                        curCmd1Job.CassetteSequenceNo, curCmd1Job.JobSequenceNo, curCmd.Cmd01_TargetPosition.ToString(), curCmd.Cmd02_TargetPosition.ToString());
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                                failMsg = string.Format("RtnCode({0})  RtnMsg({1})", fail_ReasonCode, strlog);

                                AddJobCheckFailMsg(curCmd1Job, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                #endregion
                            }
                            return false;
                        }
                        else
                        {
                            RemoveJobCheckFailMsg(curCmd1Job, fail_ReasonCode);
                        }
                    }
                    #endregion

                    //20160812 
                    #region [Both port,如果跑進Prefetch流程會skip Filter跟Orderby,會導致本來在Filter_LDRQ卡Both原出原進失效,放到其他的port,這裡強制把cmd終止]
                    RobotStage targetstage = null;
                    Port targetport = null;
                    if (curline != null )
                    {
                        fail_ReasonCode = eJob_CheckFail_Reason.Job_FromCST_To_TargetCST_Is_Fail;
                        //在Arm上,Cmd01只會是Put
                        if (curCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_PUT)
                        {
                            targetport = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, curCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));
                            targetstage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));
                            if (targetstage != null && targetstage.Data.STAGETYPE == eStageType.PORT)
                            {
                                if (targetport != null && targetport.File.Type == ePortType.BothPort)
                                {
                                    if (curCmd1Job != null && (curCmd1Job.FromCstID.Trim() != targetport.File.CassetteID.Trim() || curCmd1Job.CassetteSequenceNo != targetport.File.CassetteSequenceNo))
                                    {
                                        if (!curCmd1Job.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                                        {
                                            strlog = string.Format("[{0}] Cmd01:Job({1}_{2}) Form CSTID({3}) is not Target CSTID({4})!", MethodBase.GetCurrentMethod().Name,
                                                                    curCmd1Job.CassetteSequenceNo, curCmd1Job.JobSequenceNo, curCmd1Job.FromCstID.Trim(), targetport.File.CassetteID.Trim());
                                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                                            failMsg = string.Format("RtnCode({0})  RtnMsg({1})", fail_ReasonCode, strlog);

                                            AddJobCheckFailMsg(curCmd1Job, fail_ReasonCode, failMsg);
                                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                            #endregion
                                        }
                                        return false;
                                    }
                                    else
                                    {
                                        RemoveJobCheckFailMsg(curCmd1Job, fail_ReasonCode);
                                    }
                                }
                            }
                        }
                        //在EQP stage裡,Cmd02是Put
                        if (curCmd.Cmd02_Command == eRobot_Trx_CommandAction.ACTION_PUT)
                        {
                            targetport = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, curCmd.Cmd02_TargetPosition.ToString().PadLeft(2, '0'));
                            targetstage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curCmd.Cmd02_TargetPosition.ToString().PadLeft(2, '0'));
                            if (targetstage != null && targetstage.Data.STAGETYPE == eStageType.PORT)
                            {
                                if (targetport != null && targetport.File.Type == ePortType.BothPort)
                                {
                                    if (curCmd2Job != null && (curCmd2Job.FromCstID.Trim() != targetport.File.CassetteID.Trim() || curCmd2Job.CassetteSequenceNo != targetport.File.CassetteSequenceNo))
                                    {
                                        if (!curCmd2Job.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                                        {
                                            strlog = string.Format("[{0}] Cmd02:Job({1}_{2}) Form CSTID({3}) is not Target CSTID({4})!", MethodBase.GetCurrentMethod().Name,
                                                                    curCmd2Job.CassetteSequenceNo, curCmd2Job.JobSequenceNo, curCmd2Job.FromCstID.Trim(), targetport.File.CassetteID.Trim());
                                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                                            failMsg = string.Format("RtnCode({0})  RtnMsg({1})", fail_ReasonCode, strlog);

                                            AddJobCheckFailMsg(curCmd2Job, fail_ReasonCode, failMsg);
                                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                            #endregion
                                        }
                                        return false;
                                    }
                                    else
                                    {
                                        RemoveJobCheckFailMsg(curCmd2Job, fail_ReasonCode);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }

                string trxName = string.Format("{0}_RobotControlCommand", eqpNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                #region [Trx Structure]
                //    <eventgroup name="L2_EG_RobotControlCommand" dir="B2E">
                //      <event name="L2_W_RobotControlCommandBlock" />
                //      <event name="L2_B_RobotControlCommand" trigger="true" />
                //    </eventgroup>
                //<itemgroup name="RobotControlCommandBlock">
                //    <item name="RobotCommand#01" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="ASCII" />
                //    <item name="ArmSelect#01" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="ASCII" />
                //    <item name="TargetPosition#01" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="ASCII" />
                //    <item name="TargetSlotNo#01" woffset="3" boffset="0" wpoints="2" bpoints="32" expression="ASCII" />
                //    <item name="RobotCommand#02" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="ASCII" />
                //    <item name="ArmSelect#02" woffset="6" boffset="0" wpoints="1" bpoints="16" expression="ASCII" />
                //    <item name="TargetPosition#02" woffset="7" boffset="0" wpoints="1" bpoints="16" expression="ASCII" />
                //    <item name="TargetSlotNo#02" woffset="8" boffset="0" wpoints="2" bpoints="32" expression="ASCII" />
                //</itemgroup>
                #endregion
               
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_Command].Value = curCmd.Cmd01_Command.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_ArmSelect].Value = curCmd.Cmd01_ArmSelect.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_TargetSlotNo].Value = curCmd.Cmd01_TargetSlotNo.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_TargetPosition].Value = curCmd.Cmd01_TargetPosition.ToString();

                //20160504 modify 如果Cmd02 Action=0,則把ArmSelect,TargetSlotNo,TargetPosition都清成0,即不下Cmd  
                if (curCmd.Cmd02_Command == 0)
                {
                    outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_Command].Value = "0";
                    outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_ArmSelect].Value = "0";
                    outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_TargetSlotNo].Value = "0";
                    outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_TargetPosition].Value = "0";
                }
                else
                {
                    outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_Command].Value = curCmd.Cmd02_Command.ToString();
                    outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_ArmSelect].Value = curCmd.Cmd02_ArmSelect.ToString();
                    outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_TargetSlotNo].Value = curCmd.Cmd02_TargetSlotNo.ToString();
                    outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_TargetPosition].Value = curCmd.Cmd02_TargetPosition.ToString();
                }
                if (curCmd.Cmd01_Command != 0)  //20160624 在Arm上PutReady是Cmd01,避免重複,會清成NONE,不要下Command
                {
                    outputdata.EventGroups[strEventGroup].Events[strBEvent].Items[strItemBitCommand].Value = ((int)eBitResult.ON).ToString();
                    outputdata.EventGroups[strEventGroup].Events[strBEvent].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();

                    if (curRobot.File.CmdSendCondition) return false; //yang 2017/6/6
                    SendPLCData(outputdata);

                    #region OVNITO提前开门逻辑
                    //add by hujunpeng 20180929
                    if(curRobot.Data.LINEID=="TCOVN400"||curRobot.Data.LINEID=="TCOVN500") 
                    {
                        if (curCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET&&curCmd.Cmd01_TargetPosition<11)
                        {
                            string trxName1 = string.Format("L3_OVN{0}MoveInGlassOpenTheDoor", curCmd1Job.RobotWIP.OvnOpenTheDoorPriority);
                            Trx outputdata1 = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName1) as Trx;
                            outputdata1.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.ON).ToString();
                            outputdata1.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                            outputdata1.TrackKey = UtilityMethod.GetAgentTrackKey();
                            SendPLCData(outputdata1);
                            strlog = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] , SET OVN{2}MoveIn BIT={3}.", eqp.Data.NODENO,
                            outputdata.TrackKey, curCmd1Job.RobotWIP.OvnOpenTheDoorPriority, outputdata1.EventGroups[0].Events[0].Items[0].Value);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        
                        if(curCmd.Cmd01_Command==eRobot_Trx_CommandAction.ACTION_GET&&curCmd.Cmd01_TargetPosition>=11)
                        {
                            switch (curCmd.Cmd01_TargetPosition)
                            { 
                                case 11:
                                    string trxName2 = "L3_OVN1MoveOutGlassOpenTheDoor";
                                    Trx outputdata2 = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName2) as Trx;
                                    outputdata2.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.ON).ToString();
                                    outputdata2.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                                    outputdata2.TrackKey = DateTime.Now.ToString("yyyyMMddHHmmss");
                                    SendPLCData(outputdata2);
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] , SET OVN1MoveOut BIT={2}.", eqp.Data.NODENO,
                                    outputdata.TrackKey, outputdata2.EventGroups[0].Events[0].Items[0].Value);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    break;
                                case 13:
                                    string trxName3 = "L3_OVN2MoveOutGlassOpenTheDoor";
                                    Trx outputdata3 = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName3) as Trx;
                                    outputdata3.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.ON).ToString();
                                    outputdata3.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                                    outputdata3.TrackKey = DateTime.Now.ToString("yyyyMMddHHmmss");
                                    SendPLCData(outputdata3);
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] , SET OVN2MoveOut BIT={2}.", eqp.Data.NODENO,
                                    outputdata.TrackKey, outputdata3.EventGroups[0].Events[0].Items[0].Value);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    break;
                                default:
                                    break;
                            }
                        }
                        
                    }
                    #endregion

                    #region [ add Monitor Robot Control Command TimeOut ]

                    string timeName = string.Format("{0}_{1}_{2}", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_TIMEOUT);

                    if (_timerManager.IsAliveTimer(timeName))
                    {
                        _timerManager.TerminateTimer(timeName);
                    }

                    _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(RobotControlCommandReplyTimeout), outputdata.TrackKey);

                    #endregion

                    #region [ Robot Command Send後開始計算Robot Command Active TimeOut (Robot Status Idle -> Running) ]

                    string waitActivetimeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                    if (_timerManager.IsAliveTimer(waitActivetimeName))
                    {
                        _timerManager.TerminateTimer(waitActivetimeName);
                    }

                    //防止Config設定異常,預設5分鐘300000ms
                    int robotCmdActiveTimeOut = 300000;

                    try
                    {
                        robotCmdActiveTimeOut = ParameterManager[eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_CONSTANT_KEY].GetInteger();
                    }
                    catch (Exception ex1)
                    {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex1);
                    }

                    _timerManager.CreateTimer(waitActivetimeName, false, robotCmdActiveTimeOut, new System.Timers.ElapsedEventHandler(RobotControlCommandActiveTimeout), outputdata.TrackKey);

                    #endregion

                    #region [ Robot Command Send後開始計算Robot Command RT2 TimeOut(EQP Report RobotCommand Result report) ]

                    string waitRT2timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_RT2_TIMEOUT_EVENTNAME);

                    if (_timerManager.IsAliveTimer(waitRT2timeName))
                    {
                        _timerManager.TerminateTimer(waitRT2timeName);
                    }

                    //防止Config設定異常,預設300sec,300000ms
                    int robotCmdRT2TimeOut = 300000;

                    try
                    {
                        robotCmdRT2TimeOut = ParameterManager[eRobotCommonConst.ROBOT_CONTROL_COMMAND_RT2_TIMEOUT_CONSTANT_KEY].GetInteger();
                    }
                    catch (Exception ex1)
                    {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex1);
                    }

                    _timerManager.CreateTimer(waitRT2timeName, false, robotCmdRT2TimeOut, new System.Timers.ElapsedEventHandler(RobotControlCommandRT2Timeout), outputdata.TrackKey);

                    #endregion

                    strlog = string.Format("[EQUIPMENT={0}] [RCS -> EQP][{1}] {2} Mode Set RobotCommand#01({3}) ArmSelect({4}) TargetPos({5}) TargetSlotNo({6}) RobotCommand#02({7}) ArmSelect({8}) TargetPos({9}) TargetSlotNo({10}) Set Bit (ON)",
                                            curRobot.Data.NODENO, outputdata.TrackKey, curRobot.File.curRobotRunMode,
                                            curCmd.Cmd01_Command, curCmd.Cmd01_ArmSelect, curCmd.Cmd01_TargetPosition, curCmd.Cmd01_TargetSlotNo.ToString(),
                                            curCmd.Cmd02_Command, curCmd.Cmd02_ArmSelect, curCmd.Cmd02_TargetPosition, curCmd.Cmd02_TargetSlotNo.ToString());


                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    #region [ Update Robot Command Status ]

                    if (curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus != eRobot_ControlCommandStatus.CREATE)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Change Robot Control Command Status from ({2}) to ({3}) !",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus, eRobot_ControlCommandStatus.CREATE);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curRobot)
                        {
                            curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus = eRobot_ControlCommandStatus.CREATE;

                            curRobot.CurRealTimeSetCommandInfo.CmdCreateDateTime = DateTime.Now;
                            curRobot.CurRealTimeSetCommandInfo.Cmd01_ArmSelect = curCmd.Cmd01_ArmSelect;
                            curRobot.CurRealTimeSetCommandInfo.Cmd01_Command = curCmd.Cmd01_Command;
                            curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetPosition = curCmd.Cmd01_TargetPosition;
                            curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetSlotNo = curCmd.Cmd01_TargetSlotNo;
                            curRobot.CurRealTimeSetCommandInfo.Cmd01_CSTSeq = curCmd.Cmd01_CSTSeq;
                            curRobot.CurRealTimeSetCommandInfo.Cmd01_JobSeq = curCmd.Cmd01_JobSeq;
                            curRobot.CurRealTimeSetCommandInfo.Cmd01_JobKey = curCmd.Cmd01_JobKey;

                            curRobot.CurRealTimeSetCommandInfo.Cmd02_ArmSelect = curCmd.Cmd02_ArmSelect;
                            curRobot.CurRealTimeSetCommandInfo.Cmd02_Command = curCmd.Cmd02_Command;
                            curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition = curCmd.Cmd02_TargetPosition;
                            curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetSlotNo = curCmd.Cmd02_TargetSlotNo;
                            curRobot.CurRealTimeSetCommandInfo.Cmd02_CSTSeq = curCmd.Cmd02_CSTSeq;
                            curRobot.CurRealTimeSetCommandInfo.Cmd02_JobSeq = curCmd.Cmd02_JobSeq;
                            curRobot.CurRealTimeSetCommandInfo.Cmd02_JobKey = curCmd.Cmd02_JobKey;

                            //Clear Cmd Result
                            curRobot.CurRealTimeSetCommandInfo.CmdResult01 = 0;
                            curRobot.CurRealTimeSetCommandInfo.CmdResult02 = 0;
                            curRobot.CurRealTimeSetCommandInfo.CmdResult_CurPosition = 0;

                            //Set EQ Reply Status
                            curRobot.CurRealTimeSetCommandInfo.CmdEQReply = "WaitReply";

                        }

                        //Real Time Data 不需要Save File

                        //Send OPI Cmd Msg 20150830 Work end 注意要參考T2來修正
                        SendRobotCommandInfoMessageToOPI(curRobot, "RobotControlCommandSend");

                    }

                    #endregion

                    if(curCmd1Job.EQPJobID.Equals(curRobot.WaitGlass)||curCmd2Job.EQPJobID.Equals(curRobot.WaitGlass))  //yang 2017/3/13
                    {
                        curRobot.WaitGlass = string.Empty;
                        curRobot.WaitGlassTimeSpan = string.Empty;
                    }
                    if (curRobot.File.curRobotRunMode == eRobot_RunMode.AUTO_MODE&&curRobot.CheckErrorList.Count()!=0)  //add by yang,auto cmd Send Clear Job Error
                    {
                        Invoke(eServiceName.EvisorService, "AppErrorClear", new object[] { curRobot.Data.LINEID, curRobot.CheckErrorList, curCmd1Job.EQPJobID });
                        if (curCmd2Job != null && !curCmd2Job.Equals(curCmd1Job))
                            Invoke(eServiceName.EvisorService, "AppErrorClear", new object[] { curRobot.Data.LINEID, curRobot.CheckErrorList, curCmd2Job.EQPJobID });
                    }

                    return true;
                }
                return false;  //20160624  在Arm上PutReady是Cmd01,避免重複,會清成NONE,不要下Command

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        public void RobotControlCommandReply(Trx inputData)
        {
            try
            {

                string strlog = string.Empty;
                string cmdMsg = string.Empty;

                #region [拆出PLCAgent Data]

                #region  [ Trx Structure ]
                //<trx name="L2_RobotControlCommandReply" triggercondition="change">
                //  <eventgroup name="L2_EG_RobotControlCommandReply" dir="E2B">
                //    <event name="L2_W_RobotControlCommandReplyBlock" />
                //    <event name="L2_B_RobotControlCommandReply" trigger="true" />
                //  </eventgroup>
                //</trx>
                //<itemgroup name="RobotControlCommandReplyBlock">
                //  <item name="ReturnCode" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>
                #endregion

                if (inputData.IsInitTrigger) return;
                string strNodeNo = inputData.Metadata.NodeNo;
                string strTransactionGroup = string.Format("{0}_EG_RobotControlCommandReply", strNodeNo);//Event Group
                string strWEvent = string.Format("{0}_W_RobotControlCommandReplyBlock", strNodeNo);//Event
                string strBEvent = string.Format("{0}_B_RobotControlCommandReply", strNodeNo);
                string strItemReturnCode = "ReturnCode";
                string strItemBitReport = "RobotControlCommandReply";

                string strItemReturnCodeValue = inputData[strTransactionGroup][strWEvent][strItemReturnCode].Value;

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[strTransactionGroup].Events[strBEvent].Items[strItemBitReport].Value);
                #endregion

                #region [ Get Robot Entity ]

                Robot curRobot = ObjectManager.RobotManager.GetRobot(strNodeNo);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP] Can not Get Robot by EQPNo({1})!",
                                                            inputData.Metadata.NodeNo, inputData.Metadata.NodeNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                #endregion

                #region [ 取得Robot物件後更新RobotControlCommandEQPReplyBitFlag ]

                if (triggerBit == eBitResult.OFF)
                {
                    lock (curRobot.File)
                    {
                        curRobot.File.RobotControlCommandEQPReplyBitFlag = false;

                    }

                    //存入Robot File
                    ObjectManager.RobotManager.EnqueueSave(curRobot.File);

                    return;
                }
                else
                {
                    lock (curRobot.File)
                    {
                        curRobot.File.RobotControlCommandEQPReplyBitFlag = true;

                    }

                    //存入Robot File
                    ObjectManager.RobotManager.EnqueueSave(curRobot.File);
                }

                #endregion

                string timeName = string.Format("{0}_{1}_{2}", strNodeNo, curRobot.Data .ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_TIMEOUT);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                SetRobotControlCommandBit(strNodeNo, eBitResult.OFF, inputData.TrackKey);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [RCS -> EQP][{1}] EQP Reply ReturnCode({2}),Robot Control Command Set Bit (OFF)."
                    , strNodeNo, inputData.TrackKey, (eRobotCmdReturnCode)int.Parse(strItemReturnCodeValue)));
                                
                #region 針對Robot Control Command Reply做處理

                eRobotCmdReturnCode retCode = (eRobotCmdReturnCode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                string curCmdStatus=string.Empty;
                string cmdreply = string.Empty;

                switch (retCode)
                {
                    case eRobotCmdReturnCode.OK:

                        #region [ Handle EQ Reply OK ]

                        curCmdStatus = eRobot_ControlCommandStatus.EQREPLY_OK;

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP] Robot({1}) Robot Command EQP returnCode({2}), Robot Command Status({3})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, retCode.ToString(), curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        cmdreply = string.Format("{0}:{1}", ((int)retCode).ToString(), retCode.ToString());

                        //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                        cmdMsg = string.Format("{0} - RETURN_CODE={1}, CommandStatus={2}",
                             "RobotControlCommandReply".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "OK", curCmdStatus);

                        Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.NormalType });

                        strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                        Logger.LogTrxWrite(this.LogName, strlog);

                        //只記錄Log 不更新狀態以免出現同時上報 Command Reply 與 Command Result Report造成誤解
                        return;

                        #endregion

                    case eRobotCmdReturnCode.BUSY_TRY_AGAIN:
                    case eRobotCmdReturnCode.CIMMODE_IS_OFFLINE:
                    case eRobotCmdReturnCode.ALREADY_EXECUTING:
                    case eRobotCmdReturnCode.CANNOT_PERFORM_NOW:
                    case eRobotCmdReturnCode.BC_Cmd_No_Err:
                    case eRobotCmdReturnCode.BC_Position_No_Err:
                    case eRobotCmdReturnCode.BC_Arm_No_Err:
                    case eRobotCmdReturnCode.BC_Slot_No_Err:
                    case eRobotCmdReturnCode.BC_Cmd_Hold:
                    case eRobotCmdReturnCode.Indexer_EQ_Status_NotRun:
                    case eRobotCmdReturnCode.Robot_NotIdle:
                    case eRobotCmdReturnCode.Robot_Arm_Abnormal:
                    case eRobotCmdReturnCode.CST_Cmd_CannotAct:
                    case eRobotCmdReturnCode.CST_Port_Status_Abnormal:
                    case eRobotCmdReturnCode.EQ_Cmd_CannotAct:
                    case eRobotCmdReturnCode.EQ_PIO_SignalErr:
                    case eRobotCmdReturnCode.EQ_GLS_DataErr:
                    case eRobotCmdReturnCode.EQ_LinkSignalErr:

                        #region [ EQ Reply NG ]

                        curCmdStatus = eRobot_ControlCommandStatus.CANCEL;

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP] Robot({1}) Robot Command EQP returnCode({2}), Change Robot Cmd Status from ({3}) to ({4}) !",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, retCode.ToString(), curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus,
                                                curCmdStatus);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        cmdreply = string.Format("{0}:{1}", ((int)retCode).ToString(), retCode.ToString());

                        //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                        cmdMsg = string.Format("{0} - RETURN_CODE={1}({2}, CommandStatus={3}",
                            "RobotControlCommandReply".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "NG", retCode.ToString(), eRobot_ControlCommandStatus.CANCEL);

                        Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.AlarmType });

                        strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                        Logger.LogTrxWrite(this.LogName, strlog);

                        break;

                        #endregion

                    default:

                        #region [ EQ Reply NG ]

                        //Reserved 6~99
                        curCmdStatus = eRobot_ControlCommandStatus.CANCEL;
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP] Robot({1}) Robot Command EQP returnCode({2}) , Change Robot Cmd Status from ({3}) to ({4}) !",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, retCode.ToString(), curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus, 
                                                curCmdStatus);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        cmdreply = string.Format("{0}:{1}", ((int)retCode).ToString(), "Reserved");

                        //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                        cmdMsg = string.Format("{0} - RETURN_CODE={1}({2}, CommandStatus={3}",
                                           "RobotControlCommandReply".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "NG", retCode.ToString(), eRobot_ControlCommandStatus.CANCEL);

                        Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.AlarmType });

                        strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                        Logger.LogTrxWrite(this.LogName, strlog);

                        break;

                        #endregion

                }

                #region [ Robot Command Status = Cancel must Reset RT2 and Wait Acitve TimeOut ]

                if (curCmdStatus == eRobot_ControlCommandStatus.CANCEL)
                {
                    //Reset RT2 TimeOut
                    ResetRobotCommandRT2TimeOut(curRobot);

                    #region [ 清除RobotCmd Active TimeOut ]

                    timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                    if (_timerManager.IsAliveTimer(timeName))
                    {
                        _timerManager.TerminateTimer(timeName);
                    }

                    #endregion

                }

                #endregion

                //Update RealTime Robot Command Status
                if (curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus != curCmdStatus)
                {

                    lock (curRobot.File)
                    {
                        curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus = curCmdStatus;
                        curRobot.CurRealTimeSetCommandInfo.CmdStatusChangeDateTime = DateTime.Now;
                        curRobot.CurRealTimeSetCommandInfo.CmdEQReply = cmdreply;
                    }

                    //即時值不需要寫入File

                }
                
                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary>Reset RobotCommandRT2TimeOut Cell Special 與 Normal 共用
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        private void ResetRobotCommandRT2TimeOut(Robot curRobot)
        {
            try
            {
                //Key Robot Node_RobotName_TimeKey
                string timeName = string.Format("{0}_{1}_{2}", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_RT2_TIMEOUT_EVENTNAME);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RobotControlCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string strlog = string.Empty;
                string cmdMsg = string.Empty;

                //20141101 add RobotName   Node_RobotName_TimeoutKey
                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], eRobotCommonConst.ROBOT_CONTROL_COMMAND_TIMEOUT);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                strlog = string.Format("[EQUIPMENT={0}] [RCS -> RCS][{1}] EQP Reply,Robot({2}) Control Command Reply Timeout Set Value(OFF).", sArray[0], trackKey, sArray[1]);
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                SetRobotControlCommandBit(sArray[0], eBitResult.OFF, trackKey);

                #region [ 取得Robot並將Command Status Reset掉 ]

                Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(sArray[1]);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Can not Get Robot by RobotName({1})!",
                                            sArray[0], sArray[1]);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                if (curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus != eRobot_ControlCommandStatus.CANCEL)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Robot({1}) Robot Command is TimeOut , Change Robot Cmd Status from ({2}) to ({3}) !",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus, eRobot_ControlCommandStatus.CANCEL);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    lock (curRobot.File)
                    {
                        curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus = eRobot_ControlCommandStatus.CANCEL;
                        curRobot.CurRealTimeSetCommandInfo.CmdStatusChangeDateTime = DateTime.Now;
                        curRobot.CurRealTimeSetCommandInfo.CmdEQReply = "ReplyTimeOut";
                    }

                    //RealTime Date 不需要Save File;

                }

                //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                cmdMsg = string.Format("{0} - RETURN_CODE={1}({2}), CommandStatus={3}",
                                        "RobotControlCommandReplyTimeout".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "NG", "TIMEOUT", eRobot_ControlCommandStatus.CANCEL);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.AlarmType });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #endregion

                //Reset RT2 TimeOut
                ResetRobotCommandRT2TimeOut(curRobot);

                #region [ 清除RobotCmd Active TimeOut ]

                timeName = string.Format("{0}_{1}_{2}", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        private void SetRobotControlCommandBit(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_RobotControlCommand", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                string strEventGroup = string.Format(strRobotControlCommandEventGroup, eqpNo);//Event Group;
                string strWordEvent = string.Format(strRobotControlCommandWEvent, eqpNo);//Event
                string strBitEvent = string.Format(strRobotControlCommandBEvent, eqpNo);//Event
                string strItemBitCommand = "RobotControlCommand";

                outputdata.EventGroups[strEventGroup].Events[strWordEvent].IsDisable = true;
                outputdata.EventGroups[strEventGroup].Events[strBitEvent].Items[strItemBitCommand].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);                

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //Monitor WaitActive TimeOut(Send RobotCommand後到Robot Status變成Running)
        private void RobotControlCommandActiveTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string strlog = string.Empty;
                string cmdMsg = string.Empty;

                //Node_RobotName_TimeoutKey
                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Command TimeOutStatus({2}) , Robot Control Command Active Timeout Set Value(OFF).",
                                        sArray[0], sArray[1], trackKey);
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #region 取得Robot並將CurCmd Status Reset掉!

                Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(sArray[1]);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Can not Get Robot by RobotName({1})!", sArray[0], sArray[1]);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                if (curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus != eRobot_ControlCommandStatus.CANCEL)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) RobotCommand ActiveTimeOut , Change Robot Command Status from({2}) to ({3}) !",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus, eRobot_ControlCommandStatus.CANCEL);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    lock (curRobot)
                    {
                        curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus = eRobot_ControlCommandStatus.CANCEL;
                        curRobot.CurRealTimeSetCommandInfo.CmdStatusChangeDateTime = DateTime.Now;
                        curRobot.CurRealTimeSetCommandInfo.CmdEQReply = "ActiveTimeOut";
                    }

                    //更新RealTime Command 不須Update File

                }

                //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                cmdMsg = string.Format("{0} - Robot {1}, CommandStatus={2}",
                                       "RobotControlCommandActiveTimeout".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "ACTIVETIMEOUT", eRobot_ControlCommandStatus.CANCEL);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.AlarmType });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //Monitor RT2 TimeOut(Send Robot Control Commnd後到EQP Report RobotCmd Result) 
        private void RobotControlCommandRT2Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string strlog = string.Empty;
                string cmdMsg = string.Empty;

                //Node_RobotName_TimeoutKey
                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], eRobotCommonConst.ROBOT_CONTROL_COMMAND_RT2_TIMEOUT_EVENTNAME);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Command TimeOutStatus({2}) , Robot Control Command RT2 Timeout Set Value(OFF).",
                                       sArray[0], sArray[1], trackKey);
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #region [ 取得Robot並將CurCmd Status Reset掉! ]

                Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(sArray[1]);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Can not Get Robot by RobotName({1})!",
                                            sArray[0], sArray[1]);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                if (curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus != eRobot_ControlCommandStatus.CANCEL)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Robot Command is RT2 TimeOut , Change Robot Cmd Status from({2}) to ({3}) !",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus, eRobot_ControlCommandStatus.CANCEL);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    lock (curRobot)
                    {
                        curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus = eRobot_ControlCommandStatus.CANCEL;
                        curRobot.CurRealTimeSetCommandInfo.CmdStatusChangeDateTime = DateTime.Now;
                        curRobot.CurRealTimeSetCommandInfo.CmdEQReply = "RT2TIMEOUT";
                    }

                    //更新RealTime Command 不須Update File

                }

                //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                cmdMsg = string.Format("{0} - Robot {1}, CommandStatus={2}",
                                       "RobotControlCommandRT2Timeout".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "RT2TIMEOUT", curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.WarningType });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region CellSpecialRobotControlCommand  ===============================================================================================================================================

        public bool CellSpecialRobotControlCommandSend(Robot curRobot, CellSpecialRobotCmdInfo curCmd)
        {
            try
            {

                string eqpNo = curRobot.Data.NODENO;
                Equipment eqp;
                string strlog = string.Empty;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    #region  [DebugLog]

                    if (_showDeBugLogFlag == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBM] Robot({1}) can not find EquipmentNo({2}) in EquipmentEntity!",
                                                                eqp, curRobot.Data.ROBOTNAME, eqp);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;

                }

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RBM] CIM Mode(OFF),can not send Robot Control Command!", eqp.Data.NODENO);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return false;
                }


                string trxName = string.Format("{0}_CellSpecialRobotControlCommand", eqpNo);

                IServerAgent agent= GetServerAgent(eAgentName.PLCAgent);

                Trx outputdata = agent.GetTransactionFormat(trxName) as Trx;

                #region [Trx Structure]
                             
                //<trx name="L2_CellSpecialRobotControlCommand" triggercondition="change">
                //  <eventgroup name="L2_EG_CellSpecialRobotControlCommand" dir="B2E">
                //    <event name="L2_W_CellSpecialRobotControlCommandBlock" />
                //    <event name="L2_B_CellSpecialRobotControlCommand" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<itemgroup name="CellSpecialRobotControlCommandBlock">
                //  <item name="RobotCommand#01" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ArmSelect#01" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="TargetPosition#01" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="TargetSlotNo#01" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="RobotCommand#02" woffset="4" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ArmSelect#02" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="TargetPosition#02" woffset="6" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="TargetSlotNo#02" woffset="7" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="RobotCommand#03" woffset="8" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ArmSelect#03" woffset="9" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="TargetPosition#03" woffset="10" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="TargetSlotNo#03" woffset="11" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="RobotCommand#04" woffset="12" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="ArmSelect#04" woffset="13" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="TargetPosition#04" woffset="14" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="TargetSlotNo#04" woffset="15" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>
                #endregion
                //string strNodeNo = curRobot.Data.NODENO;
                string strEventGroup = string.Format(strCellSpecialRobotControlCommandEventGroup, eqpNo);//Event Group;
                string strWEvent = string.Format(strCellSpecialRobotControlCommandWEvent, eqpNo);//Event
                string strBEvent = string.Format(strCellSpecialRobotControlCommandBEvent, eqpNo);//Event

                string strItemCmd01_Command = "RobotCommand#01";
                string strItemCmd01_ArmSelect = "ArmSelect#01";
                string strItemCmd01_TargetSlotNo = "TargetSlotNo#01";
                string strItemCmd01_TargetPosition = "TargetPosition#01";

                string strItemCmd02_Command = "RobotCommand#02";
                string strItemCmd02_ArmSelect = "ArmSelect#02";
                string strItemCmd02_TargetSlotNo = "TargetSlotNo#02";
                string strItemCmd02_TargetPosition = "TargetPosition#02";

                string strItemCmd03_Command = "RobotCommand#03";
                string strItemCmd03_ArmSelect = "ArmSelect#03";
                string strItemCmd03_TargetSlotNo = "TargetSlotNo#03";
                string strItemCmd03_TargetPosition = "TargetPosition#03";

                string strItemCmd04_Command = "RobotCommand#04";
                string strItemCmd04_ArmSelect = "ArmSelect#04";
                string strItemCmd04_TargetSlotNo = "TargetSlotNo#04";
                string strItemCmd04_TargetPosition = "TargetPosition#04";

                string strItemBitCommand = "CellSpecialRobotControlCommand";

                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_Command].Value = curCmd.Cmd01_Command.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_ArmSelect].Value = curCmd.Cmd01_ArmSelect.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_TargetSlotNo].Value = curCmd.Cmd01_TargetSlotNo.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd01_TargetPosition].Value = curCmd.Cmd01_TargetPosition.ToString();

                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_Command].Value = curCmd.Cmd02_Command.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_ArmSelect].Value = curCmd.Cmd02_ArmSelect.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_TargetSlotNo].Value = curCmd.Cmd02_TargetSlotNo.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd02_TargetPosition].Value = curCmd.Cmd02_TargetPosition.ToString();

                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd03_Command].Value = curCmd.Cmd03_Command.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd03_ArmSelect].Value = curCmd.Cmd03_ArmSelect.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd03_TargetSlotNo].Value = curCmd.Cmd03_TargetSlotNo.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd03_TargetPosition].Value = curCmd.Cmd03_TargetPosition.ToString();

                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd04_Command].Value = curCmd.Cmd04_Command.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd04_ArmSelect].Value = curCmd.Cmd04_ArmSelect.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd04_TargetSlotNo].Value = curCmd.Cmd04_TargetSlotNo.ToString();
                outputdata.EventGroups[strEventGroup].Events[strWEvent].Items[strItemCmd04_TargetPosition].Value = curCmd.Cmd04_TargetPosition.ToString();

                outputdata.EventGroups[strEventGroup].Events[strBEvent].Items[strItemBitCommand].Value = ((int)eBitResult.ON).ToString();
                outputdata.EventGroups[strEventGroup].Events[strBEvent].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();

                SendPLCData(outputdata);

                //20151202 add for Cell Special
                #region [ add Monitor Robot Control Command TimeOut ]

                string timeName = string.Format("{0}_{1}_{2}", eqpNo, curRobot.Data.ROBOTNAME, eRobotCommonConst.CELL_SPECIAL_ROBOT_CONTROL_COMMAND_TIMEOUT);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(CellSpecialRobotControlCommandReplyTimeout), outputdata.TrackKey);

                #endregion

                #region [ Robot Command Send後開始計算Robot Command Active TimeOut (Robot Status Idle -> Running) ]

                string waitActivetimeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                if (_timerManager.IsAliveTimer(waitActivetimeName))
                {
                    _timerManager.TerminateTimer(waitActivetimeName);
                }

                //防止Config設定異常,預設5分鐘300000ms
                int robotCmdActiveTimeOut = 300000;

                try
                {
                    robotCmdActiveTimeOut = ParameterManager[eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_CONSTANT_KEY].GetInteger();
                }
                catch (Exception ex1)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex1);
                }

                _timerManager.CreateTimer(waitActivetimeName, false, robotCmdActiveTimeOut, new System.Timers.ElapsedEventHandler(CellSpecialRobotControlCommandActiveTimeout), outputdata.TrackKey);

                #endregion

                #region [ Robot Command Send後開始計算Robot Command RT2 TimeOut(EQP Report RobotCommand Result report) ]

                string waitRT2timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_RT2_TIMEOUT_EVENTNAME);

                if (_timerManager.IsAliveTimer(waitRT2timeName))
                {
                    _timerManager.TerminateTimer(waitRT2timeName);
                }

                //防止Config設定異常,預設300sec,300000ms
                int robotCmdRT2TimeOut = 300000;

                try
                {
                    robotCmdRT2TimeOut = ParameterManager[eRobotCommonConst.ROBOT_CONTROL_COMMAND_RT2_TIMEOUT_CONSTANT_KEY].GetInteger();
                }
                catch (Exception ex1)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex1);
                }

                _timerManager.CreateTimer(waitRT2timeName, false, robotCmdRT2TimeOut, new System.Timers.ElapsedEventHandler(CellSpecialRobotControlCommandRT2Timeout), outputdata.TrackKey);

                #endregion


                strlog = string.Format(@"[EQUIPMENT={0}] [RCS -> EQP][{1}] {2} Mode Set 
                                          RobotCommand#01({3}) ArmSelect({4}) TargetPos({5}) TargetSlotNo({6}), RobotCommand#02({7}) ArmSelect({8}) TargetPos({9}) TargetSlotNo({10}), RobotCommand#03({11}) ArmSelect({12}) TargetPos({13}) TargetSlotNo({14}), RobotCommand#04({15}) ArmSelect({16}) TargetPos({17}) TargetSlotNo({18})Set Bit (ON)",
                                        curRobot.Data.NODENO, outputdata.TrackKey, curRobot.File.curRobotRunMode,
                                        curCmd.Cmd01_Command, curCmd.Cmd01_ArmSelect, curCmd.Cmd01_TargetPosition, curCmd.Cmd01_TargetSlotNo.ToString(),
                                        curCmd.Cmd02_Command, curCmd.Cmd02_ArmSelect, curCmd.Cmd02_TargetPosition, curCmd.Cmd02_TargetSlotNo.ToString(),
                                        curCmd.Cmd03_Command, curCmd.Cmd03_ArmSelect, curCmd.Cmd03_TargetPosition, curCmd.Cmd03_TargetSlotNo.ToString(),
                                        curCmd.Cmd04_Command, curCmd.Cmd04_ArmSelect, curCmd.Cmd04_TargetPosition, curCmd.Cmd04_TargetSlotNo.ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #region [ Update Robot Command Status ]

                if (curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus != eRobot_ControlCommandStatus.CREATE)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Change Robot Control Command Status from ({2}) to ({3}) !",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus, eRobot_ControlCommandStatus.CREATE);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    lock (curRobot)
                    {

                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus = eRobot_ControlCommandStatus.CREATE;

                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdCreateDateTime = DateTime.Now;

                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_ArmSelect = curCmd.Cmd01_ArmSelect;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_Command = curCmd.Cmd01_Command;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_TargetPosition = curCmd.Cmd01_TargetPosition;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_TargetSlotNo = curCmd.Cmd01_TargetSlotNo;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontCSTSeq = curCmd.Cmd01_FrontCSTSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontJobSeq = curCmd.Cmd01_FrontJobSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontJobKey = curCmd.Cmd01_FrontJobKey;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackCSTSeq = curCmd.Cmd01_BackCSTSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackJobSeq = curCmd.Cmd01_BackJobSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackJobKey = curCmd.Cmd01_BackJobKey;

                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_ArmSelect = curCmd.Cmd02_ArmSelect;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_Command = curCmd.Cmd02_Command;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_TargetPosition = curCmd.Cmd02_TargetPosition;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_TargetSlotNo = curCmd.Cmd02_TargetSlotNo;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_FrontCSTSeq = curCmd.Cmd02_FrontCSTSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_FrontJobSeq = curCmd.Cmd02_FrontJobSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_FrontJobKey = curCmd.Cmd02_FrontJobKey;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_BackCSTSeq = curCmd.Cmd02_BackCSTSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_BackJobSeq = curCmd.Cmd02_BackJobSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_BackJobKey = curCmd.Cmd02_BackJobKey;

                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_ArmSelect = curCmd.Cmd03_ArmSelect;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_Command = curCmd.Cmd03_Command;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_TargetPosition = curCmd.Cmd03_TargetPosition;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_TargetSlotNo = curCmd.Cmd03_TargetSlotNo;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_FrontCSTSeq = curCmd.Cmd03_FrontCSTSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_FrontJobSeq = curCmd.Cmd03_FrontJobSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_FrontJobKey = curCmd.Cmd03_FrontJobKey;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_BackCSTSeq = curCmd.Cmd03_BackCSTSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_BackJobSeq = curCmd.Cmd03_BackJobSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_BackJobKey = curCmd.Cmd03_BackJobKey;

                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_ArmSelect = curCmd.Cmd04_ArmSelect;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_Command = curCmd.Cmd04_Command;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_TargetPosition = curCmd.Cmd04_TargetPosition;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_TargetSlotNo = curCmd.Cmd04_TargetSlotNo;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_FrontCSTSeq = curCmd.Cmd04_FrontCSTSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_FrontJobSeq = curCmd.Cmd04_FrontJobSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_FrontJobKey = curCmd.Cmd04_FrontJobKey;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_BackCSTSeq = curCmd.Cmd04_BackCSTSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_BackJobSeq = curCmd.Cmd04_BackJobSeq;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_BackJobKey = curCmd.Cmd04_BackJobKey;

                        //Clear Cmd Result
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdResult01 = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdResult02 = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdResult03 = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdResult04 = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdResult_CurPosition = 0;

                        //Set EQ Reply Status
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdEQReply = "WaitReply";

                    }

                    //Real Time Data 不需要Save File
                    //Send OPI Cmd Msg
                    SendCellSpecialRobotCommandInfoMessageToOPI(curRobot, "CellSpecialRobotControlCommandSend");

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

        public void CellSpecialRobotControlCommandReply(Trx inputData)
        {
            try
            {

                string strlog = string.Empty;
                string cmdMsg = string.Empty;

                #region [拆出PLCAgent Data]

                #region  [ Trx Structure ]
                //<trx name="L2_CellSpecialRobotControlCommandReply" triggercondition="change">
                //  <eventgroup name="L2_EG_CellSpecialRobotControlCommandReply" dir="E2B">
                //    <event name="L2_W_CellSpecialRobotControlCommandReplyBlock" />
                //    <event name="L2_B_CellSpecialRobotControlCommandReply" trigger="true" />
                //  </eventgroup>
                //</trx>
                //<itemgroup name="CellSpecialRobotControlCommandReplyBlock">
                //  <item name="ReturnCode" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>
                #endregion

                if (inputData.IsInitTrigger) return;
                string strNodeNo = inputData.Metadata.NodeNo;
                string strTransactionGroup = string.Format("{0}_EG_CellSpecialRobotControlCommandReply", strNodeNo);//Event Group
                string strWEvent = string.Format("{0}_W_CellSpecialRobotControlCommandReplyBlock", strNodeNo);//Event
                string strBEvent = string.Format("{0}_B_CellSpecialRobotControlCommandReply", strNodeNo);
                string strItemReturnCode = "ReturnCode";
                string strItemBitReport = "CellSpecialRobotControlCommandReply";

                string strItemReturnCodeValue = inputData[strTransactionGroup][strWEvent][strItemReturnCode].Value;

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[strTransactionGroup].Events[strBEvent].Items[strItemBitReport].Value);
                #endregion

                #region [ Get Robot Entity ]

                Robot curRobot = ObjectManager.RobotManager.GetRobot(inputData.Metadata.NodeNo);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP] Can not Get Robot by EQPNo({1})!",
                                                            inputData.Metadata.NodeNo, inputData.Metadata.NodeNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                #endregion

                #region [ 取得Robot物件後更新RobotControlCommandEQPReplyBitFlag ]

                if (triggerBit == eBitResult.OFF)
                {
                    lock (curRobot.File)
                    {
                        curRobot.File.RobotControlCommandEQPReplyBitFlag = false;

                    }

                    //存入Robot File
                    ObjectManager.RobotManager.EnqueueSave(curRobot.File);

                    return;
                }
                else
                {
                    lock (curRobot.File)
                    {
                        curRobot.File.RobotControlCommandEQPReplyBitFlag = true;

                    }

                    //存入Robot File
                    ObjectManager.RobotManager.EnqueueSave(curRobot.File);
                }

                #endregion

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, curRobot.Data.ROBOTNAME, eRobotCommonConst.CELL_SPECIAL_ROBOT_CONTROL_COMMAND_TIMEOUT);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                SetCellSpecialRobotControlCommandBit(strNodeNo, eBitResult.OFF, inputData.TrackKey);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [RCS -> EQP][{1}] EQP Reply ReturnCode({2}),Cell Special Robot Control Command Set Bit (OFF)."
                                    , strNodeNo, inputData.TrackKey, (eRobotCmdReturnCode)int.Parse(strItemReturnCodeValue)));

                #region [ 針對Robot Control Command Reply做處理 ]

                eRobotCmdReturnCode retCode = (eRobotCmdReturnCode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                string curCmdStatus = string.Empty;
                string cmdreply = string.Empty;

                switch (retCode)
                {
                    case eRobotCmdReturnCode.OK:

                        #region [ Handle EQ Reply OK ]

                        curCmdStatus = eRobot_ControlCommandStatus.EQREPLY_OK;

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP] Robot({1}) Robot Command EQP returnCode({2}), Robot Command Status({3})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, retCode.ToString(), curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        cmdreply = string.Format("{0}:{1}", ((int)retCode).ToString(), retCode.ToString());

                        //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                        cmdMsg = string.Format("{0} - RETURN_CODE={1}, CommandStatus={2}",
                             "RobotControlCommandReply".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "OK", curCmdStatus);

                        Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.NormalType });

                        strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                        Logger.LogTrxWrite(this.LogName, strlog);

                        //只記錄Log 不更新狀態以免出現同時上報 Command Reply 與 Command Result Report造成誤解
                        return;

                        #endregion

                    case eRobotCmdReturnCode.BUSY_TRY_AGAIN:
                    case eRobotCmdReturnCode.CIMMODE_IS_OFFLINE:
                    case eRobotCmdReturnCode.ALREADY_EXECUTING:
                    case eRobotCmdReturnCode.CANNOT_PERFORM_NOW:
                    case eRobotCmdReturnCode.BC_Cmd_No_Err:
                    case eRobotCmdReturnCode.BC_Position_No_Err:
                    case eRobotCmdReturnCode.BC_Arm_No_Err:
                    case eRobotCmdReturnCode.BC_Slot_No_Err:
                    case eRobotCmdReturnCode.BC_Cmd_Hold:
                    case eRobotCmdReturnCode.Indexer_EQ_Status_NotRun:
                    case eRobotCmdReturnCode.Robot_NotIdle:
                    case eRobotCmdReturnCode.Robot_Arm_Abnormal:
                    case eRobotCmdReturnCode.CST_Cmd_CannotAct:
                    case eRobotCmdReturnCode.CST_Port_Status_Abnormal:
                    case eRobotCmdReturnCode.EQ_Cmd_CannotAct:
                    case eRobotCmdReturnCode.EQ_PIO_SignalErr:
                    case eRobotCmdReturnCode.EQ_GLS_DataErr:
                    case eRobotCmdReturnCode.EQ_LinkSignalErr:

                        #region [ EQ Reply NG ]

                        curCmdStatus = eRobot_ControlCommandStatus.CANCEL;

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP] Robot({1}) Robot Command EQP returnCode({2}), Change Robot Cmd Status from ({3}) to ({4}) !",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, retCode.ToString(), curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus,
                                                curCmdStatus);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        cmdreply = string.Format("{0}:{1}", ((int)retCode).ToString(), retCode.ToString());

                        //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                        cmdMsg = string.Format("{0} - RETURN_CODE={1}({2}, CommandStatus={3}",
                            "RobotControlCommandReply".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "NG", retCode.ToString(), eRobot_ControlCommandStatus.CANCEL);

                        Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.AlarmType });

                        strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                        Logger.LogTrxWrite(this.LogName, strlog);

                        break;

                        #endregion

                    default:

                        #region [ EQ Reply NG ]

                        //Reserved 6~99
                        curCmdStatus = eRobot_ControlCommandStatus.CANCEL;
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP] Robot({1}) Robot Command EQP returnCode({2}) , Change Robot Cmd Status from ({3}) to ({4}) !",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, retCode.ToString(), curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus,
                                                curCmdStatus);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        cmdreply = string.Format("{0}:{1}", ((int)retCode).ToString(), "Reserved");

                        //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                        cmdMsg = string.Format("{0} - RETURN_CODE={1}({2}, CommandStatus={3}",
                                           "RobotControlCommandReply".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "NG", retCode.ToString(), eRobot_ControlCommandStatus.CANCEL);

                        Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.AlarmType });

                        strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                        Logger.LogTrxWrite(this.LogName, strlog);

                        break;

                        #endregion

                }

                #region [ Robot Command Status = Cancel must Reset RT2 and Wait Acitve TimeOut ]

                if (curCmdStatus == eRobot_ControlCommandStatus.CANCEL)
                {
                    //Reset RT2 TimeOut
                    ResetRobotCommandRT2TimeOut(curRobot);

                    #region [ 清除RobotCmd Active TimeOut Notmal/Cell Special共用 ]

                    timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                    if (_timerManager.IsAliveTimer(timeName))
                    {
                        _timerManager.TerminateTimer(timeName);
                    }

                    #endregion

                }

                #endregion

                //Update RealTime Robot Command Status
                if (curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus != curCmdStatus)
                {

                    lock (curRobot.File)
                    {
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus = curCmdStatus;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdStatusChangeDateTime = DateTime.Now;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdEQReply = cmdreply;
                    }

                    //即時值不需要寫入File

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CellSpecialRobotControlCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string strlog = string.Empty;
                string cmdMsg = string.Empty;

                //20141101 add RobotName   Node_RobotName_TimeoutKey
                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], eRobotCommonConst.CELL_SPECIAL_ROBOT_CONTROL_COMMAND_TIMEOUT);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                strlog = string.Format("[EQUIPMENT={0}] [RCS -> EQP][{1}] EQP Reply,Robot({2}) Control Command Reply Timeout Set Value(OFF).", sArray[0], trackKey, sArray[1]);
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                SetCellSpecialRobotControlCommandBit(sArray[0], eBitResult.OFF, trackKey);

                //20151202 add
                #region [ 取得Robot並將Command Status Reset掉 ]

                Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(sArray[1]);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Can not Get Robot by RobotName({1})!",
                                            sArray[0], sArray[1]);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                if (curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus != eRobot_ControlCommandStatus.CANCEL)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Robot({1}) Robot Command is TimeOut , Change Robot Cmd Status from ({2}) to ({3}) !",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus, eRobot_ControlCommandStatus.CANCEL);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    lock (curRobot.File)
                    {
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus = eRobot_ControlCommandStatus.CANCEL;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdStatusChangeDateTime = DateTime.Now;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdEQReply = "ReplyTimeOut";
                    }

                    //RealTime Date 不需要Save File;

                }

                //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                cmdMsg = string.Format("{0} - RETURN_CODE={1}({2}), CommandStatus={3}",
                                        "RobotControlCommandReplyTimeout".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "NG", "TIMEOUT", eRobot_ControlCommandStatus.CANCEL);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.AlarmType });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #endregion

                //Reset RT2 TimeOut
                ResetRobotCommandRT2TimeOut(curRobot);

                #region [ 清除RobotCmd Active TimeOut ]

                timeName = string.Format("{0}_{1}_{2}", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void SetCellSpecialRobotControlCommandBit(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_CellSpecialRobotControlCommand", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                string strEventGroup = string.Format(strCellSpecialRobotControlCommandEventGroup, eqpNo);//Event Group;
                string strWordEvent = string.Format(strCellSpecialRobotControlCommandWEvent, eqpNo);//Event
                string strBitEvent = string.Format(strCellSpecialRobotControlCommandBEvent, eqpNo);//Event
                string strItemBitCommand = "CellSpecialRobotControlCommand";

                outputdata.EventGroups[strEventGroup].Events[strWordEvent].IsDisable = true;
                outputdata.EventGroups[strEventGroup].Events[strBitEvent].Items[strItemBitCommand].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //20151201 add for Cell Special Monitor WaitActive TimeOut(Send RobotCommand後到Robot Status變成Running)
        private void CellSpecialRobotControlCommandActiveTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string strlog = string.Empty;
                string cmdMsg = string.Empty;

                //Node_RobotName_TimeoutKey
                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Command TimeOutStatus({2}) , Robot Control Command Active Timeout Set Value(OFF).",
                                        sArray[0], sArray[1], trackKey);
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #region 取得Robot並將CurCmd Status Reset掉!

                Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(sArray[1]);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Can not Get Robot by RobotName({1})!", sArray[0], sArray[1]);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                if (curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus != eRobot_ControlCommandStatus.CANCEL)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) RobotCommand ActiveTimeOut , Change Robot Command Status from({2}) to ({3}) !",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus, eRobot_ControlCommandStatus.CANCEL);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    lock (curRobot)
                    {
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus = eRobot_ControlCommandStatus.CANCEL;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdStatusChangeDateTime = DateTime.Now;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdEQReply = "ActiveTimeOut";
                    }

                    //更新RealTime Command 不須Update File

                }

                //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                cmdMsg = string.Format("{0} - Robot {1}, CommandStatus={2}",
                                       "RobotControlCommandActiveTimeout".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "ACTIVETIMEOUT", eRobot_ControlCommandStatus.CANCEL);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.AlarmType });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //20151201 Monitor RT2 TimeOut(Send Robot Control Commnd後到EQP Report RobotCmd Result) 
        private void CellSpecialRobotControlCommandRT2Timeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string strlog = string.Empty;
                string cmdMsg = string.Empty;

                //Node_RobotName_TimeoutKey
                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], eRobotCommonConst.ROBOT_CONTROL_COMMAND_RT2_TIMEOUT_EVENTNAME);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Command TimeOutStatus({2}) , Robot Control Command RT2 Timeout Set Value(OFF).",
                                       sArray[0], sArray[1], trackKey);
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #region [ 取得Robot並將CurCmd Status Reset掉! ]

                Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(sArray[1]);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Can not Get Robot by RobotName({1})!",
                                            sArray[0], sArray[1]);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                if (curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus != eRobot_ControlCommandStatus.CANCEL)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Robot Command is RT2 TimeOut , Change Robot Cmd Status from({2}) to ({3}) !",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus, eRobot_ControlCommandStatus.CANCEL);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    lock (curRobot)
                    {
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus = eRobot_ControlCommandStatus.CANCEL;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdStatusChangeDateTime = DateTime.Now;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdEQReply = "RT2TIMEOUT";
                    }

                    //更新RealTime Command 不須Update File

                }

                //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP回應 EX:RETURN_CODE=OK
                cmdMsg = string.Format("{0} - Robot {1}, CommandStatus={2}",
                                       "CellSpecialRobotControlCommandRT2Timeout".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), "RT2TIMEOUT", curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.WarningType });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region [ RobotCommandResultReport ]

        /// <summary>Command Result Report
        ///  
        /// </summary>
        /// <param name="inputData"></param>
        public void RobotCommandResultReport(Trx inputData)
        {

            try
            {
                string strlog = string.Empty;
                string armJobKey = string.Empty;
                //for Both second(Low Arm) Use
                string secondArmJobKey = string.Empty;
                string cmdMsg = string.Empty;

                #region [拆出PLCAgent Data]

                #region [Trx Structure]

                //<trx name="L2_RobotControlCommandResultReport" triggercondition="change">
                //  <eventgroup name="L2_EG_RobotControlCommandResultReport" dir="E2B">
                //    <event name="L2_W_RobotControlCommandResultDataBlock" />
                //    <event name="L2_B_RobotControlCommandResultReport" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<itemgroup name="RobotControlCommandResultDataBlock">
                //  <item name="CommandResult#01" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="CommandResult#02" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="CurrentPosition" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>
                #endregion
                string eqpNo = inputData.Metadata.NodeNo;
                string strEventGroup = string.Format("{0}_EG_RobotControlCommandResultReport", eqpNo);
                string strWEnent = string.Format("{0}_W_RobotControlCommandResultDataBlock", eqpNo);
                string strBEvent = string.Format("{0}_B_RobotControlCommandResultReport", eqpNo);

                string strItemCommandRst01 = "CommandResult#01";
                string strItemCommandRst02 = "CommandResult#02";
                string strItemCurPosition = "CurrentPosition";

                string strItemBitReport = "RobotControlCommandResultReport";

                string strItemCommandRst01Value = inputData.EventGroups[strEventGroup].Events[strWEnent].Items[strItemCommandRst01].Value;
                string strItemCommandRst02Value = inputData.EventGroups[strEventGroup].Events[strWEnent].Items[strItemCommandRst02].Value;
                string strItemCurPositionValue = inputData.EventGroups[strEventGroup].Events[strWEnent].Items[strItemCurPosition].Value;

                int result01 = 0;
                int result02 = 0;
                int resultPosition = 0;

                int.TryParse(strItemCommandRst01Value, out result01);
                int.TryParse(strItemCommandRst02Value, out result02);
                int.TryParse(strItemCurPositionValue, out resultPosition);

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[strEventGroup].Events[strBEvent].Items[strItemBitReport].Value);

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP][{1}] Robot Command Result bit({2}) Command01 Result({3}) Command02 Result({4}) curPosition({5})."
                                        , inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), 
                                        strItemCommandRst01Value, strItemCommandRst02Value, strItemCurPositionValue);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #endregion

                #region BC Reply Setting

                if (triggerBit == eBitResult.OFF)
                {

                    RobotCommandResultReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                RobotCommandResultReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);

                #endregion

                #region [ Get Robot by NodeNo ]

                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Can't find Robot by EqpNo({2}) in RobotEntity!", eqpNo, inputData.TrackKey, eqpNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                #endregion

                //Reset Robot RT2 TimeOut
                ResetRobotCommandRT2TimeOut(curRobot);

                #region [ 更新 Robot Command狀態 ]

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Clear CurRobot CommandInfo,Change curCommand Status from ({3}) to (COMPLETE)!",
                                        eqpNo, inputData.TrackKey, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                lock (curRobot.File)
                {

                    curRobot.CurRealTimeSetCommandInfo.CmdResult01 = result01;
                    curRobot.CurRealTimeSetCommandInfo.CmdResult02 = result02;
                    curRobot.CurRealTimeSetCommandInfo.CmdResult_CurPosition = resultPosition;
                    //Complete表示Cmd執行結束 部論是OK還是NG
                    curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus = eRobot_ControlCommandStatus.COMPLETE;
                    //curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus = DateTime.Now;
                }

                //Real Info不需要Save File

                string cmd1_desc = GetRobotResult_Desc(result01);
                string cmd2_desc = GetRobotResult_Desc(result02);

                //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP接受後執行完畢 EX:1'st=OK, 2'nd=OK, RobotPosition=4
                cmdMsg = string.Format("{0} - 1'st={1}, 2'nd={2}, RobotPosition={3}, CommandStatus={4}",
                                        "RobotCommandResultReport".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                        cmd1_desc, cmd2_desc, resultPosition.ToString(), eRobot_ControlCommandStatus.COMPLETE);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.NormalType });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #endregion

                #region [ 清除RobotCmd Active TimeOut ]

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RobotCommandResultReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string strlog = string.Empty;

                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_RobotControlCommandResultReply") as Trx;
                #region [Trx Structure]
                    //<trx name="L2_RobotControlCommandResultReply" triggercondition="change">
                    //  <eventgroup name="L2_EG_RobotControlCommandResultReply" dir="B2E">
                    //    <event name="L2_B_RobotControlCommandResultReply" trigger="true" />
                    //  </eventgroup>
                    //</trx>
                #endregion
                string strEventGroup = string.Format ("{0}_EG_RobotControlCommandResultReply",eqpNo );
                string strEvent = string.Format("{0}_B_RobotControlCommandResultReply", eqpNo);
                string strItem = "RobotControlCommandResultReply";

                outputdata.EventGroups[strEventGroup].Events[strEvent].Items[strItem].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + eRobotCommonConst.ROBOT_COMMAND_RESULT_REPORT_TIMEOUT))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + eRobotCommonConst.ROBOT_COMMAND_RESULT_REPORT_TIMEOUT);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + eRobotCommonConst.ROBOT_COMMAND_RESULT_REPORT_TIMEOUT, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(RobotCommandResultReportReplyTimeout), trackKey);
                }

                strlog = string.Format("[EQUIPMENT={0}] [RCS -> EQP][{1}] Robot Command Result Report Reply Set Bit ({2}).", eqpNo, trackKey, value.ToString());

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RobotCommandResultReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string strlog = string.Empty;
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                strlog = string.Format("[EQUIPMENT={0}] [RCS -> EQP][{1}] EQP Reply, Equipment Status Change Reply Timeout Report Timeout Set Bit (OFF).", sArray[0], trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                RobotCommandResultReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //20151107 add Robot Result Code To OPI
        private string GetRobotResult_Desc(int resultCode)
        {
            string strDesc = string.Format("{0}({1})",resultCode.ToString(),"Exception Error");
            string errMsg = string.Empty;

            try
            {
                switch (resultCode)
                {
                    case 0:

                        errMsg = "None";
                        break;

                    case 1 :
                        
                        errMsg = "OK";
                        break;

                    default:

                        errMsg = "Other Error";
                        break;
                }

                strDesc = string.Format("{0}({1})", resultCode.ToString(), errMsg);

                return strDesc;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return strDesc;
            }
        }

        #endregion

        #region CellSpecialRobotControlCommandResultReport

        /// <summary>CellSpecialCommand Result Report
        ///  
        /// </summary>
        /// <param name="inputData"></param>
        public void CellSpecialRobotCommandResultReport(Trx inputData)
        {

            try
            {
                string strlog = string.Empty;
                string armJobKey = string.Empty;
                //for Both second(Low Arm) Use
                string secondArmJobKey = string.Empty;
                string cmdMsg = string.Empty;

                #region [拆出PLCAgent Data]

                #region [Trx Structure]

                //<trx name="L2_CellSpecialRobotControlCommandResultReport" triggercondition="change">
                //  <eventgroup name="L2_EG_CellSpecialRobotControlCommandResultReport" dir="E2B">
                //    <event name="L2_W_CellSpecialRobotControlCommandResultDataBlock" />
                //    <event name="L2_B_CellSpecialRobotControlCommandResultReport" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<itemgroup name="CellSpecialRobotControlCommandResultDataBlock">
                //  <item name="CommandResult#01" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="CommandResult#02" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="CommandResult#03" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="CommandResult#04" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //  <item name="CurrentPosition" woffset="4" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                //</itemgroup>
                #endregion
                string eqpNo = inputData.Metadata.NodeNo;
                string strEventGroup = string.Format("{0}_EG_CellSpecialRobotControlCommandResultReport", eqpNo);
                string strWEnent = string.Format("{0}_W_CellSpecialRobotControlCommandResultDataBlock", eqpNo);
                string strBEvent = string.Format("{0}_B_CellSpecialRobotControlCommandResultReport", eqpNo);

                string strItemCommandRst01 = "CommandResult#01";
                string strItemCommandRst02 = "CommandResult#02";
                string strItemCommandRst03 = "CommandResult#03";
                string strItemCommandRst04 = "CommandResult#04";
                string strItemCurPosition = "CurrentPosition";

                string strItemBitReport = "CellSpecialRobotControlCommandResultReport";

                string strItemCommandRst01Value = inputData.EventGroups[strEventGroup].Events[strWEnent].Items[strItemCommandRst01].Value;
                string strItemCommandRst02Value = inputData.EventGroups[strEventGroup].Events[strWEnent].Items[strItemCommandRst02].Value;
                string strItemCommandRst03Value = inputData.EventGroups[strEventGroup].Events[strWEnent].Items[strItemCommandRst03].Value;
                string strItemCommandRst04Value = inputData.EventGroups[strEventGroup].Events[strWEnent].Items[strItemCommandRst04].Value;
                string strItemCurPositionValue = inputData.EventGroups[strEventGroup].Events[strWEnent].Items[strItemCurPosition].Value;

                int result01 = 0;
                int result02 = 0;
                int result03 = 0;
                int result04 = 0;
                int resultPosition = 0;

                int.TryParse(strItemCommandRst01Value, out result01);
                int.TryParse(strItemCommandRst02Value, out result02);
                int.TryParse(strItemCommandRst03Value, out result03);
                int.TryParse(strItemCommandRst04Value, out result04);
                int.TryParse(strItemCurPositionValue, out resultPosition);

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[strEventGroup].Events[strBEvent].Items[strItemBitReport].Value);

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- EQP][{1}] Cell Special Robot Command Result bit({2}) Command01 Result({3}) Command02 Result({4}) Command01 Result({5}) Command02 Result({6})curPosition({7})."
                                        , inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(),
                                        strItemCommandRst01Value, strItemCommandRst02Value,strItemCommandRst03Value,strItemCommandRst04Value, strItemCurPositionValue);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #endregion

                #region BC Reply Setting

                if (triggerBit == eBitResult.OFF)
                {

                    CellSpecialRobotCommandResultReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                CellSpecialRobotCommandResultReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);

                #endregion

                #region [ Get Robot by NodeNo ]

                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Can't find Robot by EqpNo({2}) in RobotEntity!", eqpNo, inputData.TrackKey, eqpNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                #endregion

                //Reset Robot RT2 TimeOut
                ResetRobotCommandRT2TimeOut(curRobot);

                #region [ 更新 Robot Command狀態 ]

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Clear CurRobot CommandInfo,Change curCommand Status from ({3}) to (COMPLETE)!",
                                        eqpNo, inputData.TrackKey, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                lock (curRobot.File)
                {

                    curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdResult01 = result01;
                    curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdResult02 = result02;
                    curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdResult03 = result03;
                    curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdResult04 = result04;
                    curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdResult_CurPosition = resultPosition;

                    //Complete表示Cmd執行結束 部論是OK還是NG
                    curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus = eRobot_ControlCommandStatus.COMPLETE;

                }

                //Real Info不需要Save File

                string cmd1_desc = GetRobotResult_Desc(result01);
                string cmd2_desc = GetRobotResult_Desc(result02);
                string cmd3_desc = GetRobotResult_Desc(result03);
                string cmd4_desc = GetRobotResult_Desc(result04);

                //通知OPI更新RobotCommand狀態 下Robot Cmd andEQP接受後執行完畢 EX:1'st=OK, 2'nd=OK, RobotPosition=4
                cmdMsg = string.Format("{0} - 1'st={1}, 2'nd={2}, 3'rd={3}, 4'th={4}, RobotPosition={5}, CommandStatus={6}",
                                        "RobotCommandResultReport".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                        cmd1_desc, cmd2_desc, cmd3_desc, cmd4_desc, 
                                        resultPosition.ToString(), eRobot_ControlCommandStatus.COMPLETE);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.NormalType });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

                #endregion

                #region [ 清除RobotCmd Active TimeOut(Normal/Cell Special共用 ]

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CellSpecialRobotCommandResultReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string strlog = string.Empty;

                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_CellSpecialRobotControlCommandResultReply") as Trx;

                #region [Trx Structure]
                //<trx name="L2_CellSpecialRobotControlCommandResultReply" triggercondition="change">
                //  <eventgroup name="L2_EG_CellSpecialRobotControlCommandResultReply" dir="B2E">
                //    <event name="L2_B_CellSpecialRobotControlCommandResultReply" trigger="true" />
                //  </eventgroup>
                //</trx>
                #endregion
                string strEventGroup = string.Format("{0}_EG_CellSpecialRobotControlCommandResultReply", eqpNo);
                string strEvent = string.Format("{0}_B_CellSpecialRobotControlCommandResultReply", eqpNo);
                string strItem = "CellSpecialRobotControlCommandResultReply";

                outputdata.EventGroups[strEventGroup].Events[strEvent].Items[strItem].Value = ((int)value).ToString();

                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + eRobotCommonConst.CELL_SPECIAL_ROBOT_COMMAND_RESULT_REPORT_TIMEOUT))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + eRobotCommonConst.CELL_SPECIAL_ROBOT_COMMAND_RESULT_REPORT_TIMEOUT);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + eRobotCommonConst.CELL_SPECIAL_ROBOT_COMMAND_RESULT_REPORT_TIMEOUT, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(CellSpecialRobotCommandResultReportReplyTimeout), trackKey);
                }

                strlog = string.Format("[EQUIPMENT={0}] [RCS -> EQP][{1}] Cell Special Robot Command Result Report Reply Set Bit ({2}).", eqpNo, trackKey, value.ToString());

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CellSpecialRobotCommandResultReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string strlog = string.Empty;
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                strlog = string.Format("[EQUIPMENT={0}] [RCS -> EQP][{1}] EQP Reply, Equipment Status Change Reply Timeout Report Timeout Set Bit (OFF).", sArray[0], trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                CellSpecialRobotCommandResultReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        /// <summary> 將目前所下的Robot Control Command 資訊送給OPI顯示並記錄Log
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="funcName"></param>
        private void SendRobotCommandInfoMessageToOPI(Robot curRobot, string funcName)
        {
            try
            {
                string cmdMsg = string.Empty;
                string strlog = string.Empty;

                string cmd01_Action = "NONE";
                string cmd01_UseArm = "NONE";
                string cmd01_TargetPositionID = "Unknown";
                string cmd01_TargetPositionName = "Unknown";
                string cmd01_TargetSlotNo = "NONE";

                string cmd02_Action = "NONE";
                string cmd02_UseArm = "NONE";
                string cmd02_TargetPositionID = "Unknown";
                string cmd02_TargetPositionName = "Unknown";
                string cmd02_TargetSlotNo = "NONE";

                #region [ 定義1st Command 顯示資訊 ]

                #region [取得CurCommand 1st Command Stage entity ]

                RobotStage cmd01_StageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                if (cmd01_StageEntity == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1st Command can not Find RobotStage by 1st Command TargetPosition({2})!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    cmd01_TargetPositionID = "Unknown";
                    cmd01_TargetPositionName = "Unknown";
                }
                else
                {
                    cmd01_TargetPositionID = cmd01_StageEntity.Data.STAGEID;
                    cmd01_TargetPositionName = cmd01_StageEntity.Data.STAGENAME; ;

                }

                #endregion

                #region [ 取得CurCommand 1st Command TargetSlotNo資訊 ]

                if (curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetSlotNo.ToString() != string.Empty && curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetSlotNo != 0)
                {
                    cmd01_TargetSlotNo = curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetSlotNo.ToString();
                }

                #endregion

                #endregion

                #region [ 定義2nd Command 顯示資訊 ]

                if (curRobot.CurRealTimeSetCommandInfo.Cmd02_Command == 0)
                {
                    cmd02_Action = "NONE";
                    cmd02_UseArm = "NONE";
                    cmd02_TargetPositionID = "NONE";
                    cmd02_TargetPositionName = "NONE";
                    cmd02_TargetSlotNo = "NONE";

                }
                else
                {

                    #region [ 取得CurCommand 2nd Command Stage entity ]

                    RobotStage cmd02_StageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition.ToString().PadLeft(2, '0'));

                    if (cmd02_StageEntity == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 2nd Command can not Find RobotStage by 1st Command TargetPosition({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition.ToString().PadLeft(2, '0'));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        cmd02_TargetPositionID = "Unknown";
                        cmd02_TargetPositionName = "Unknown";
                    }
                    else
                    {
                        cmd02_TargetPositionID = cmd02_StageEntity.Data.STAGEID;
                        cmd02_TargetPositionName = cmd02_StageEntity.Data.STAGENAME; ;
                    }

                    #endregion

                    #region [ 取得CurCommand 2nd Command TargetSlotNo資訊 ]

                    if (curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetSlotNo.ToString() != string.Empty && curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetSlotNo != 0)
                    {
                        cmd02_TargetSlotNo = curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetSlotNo.ToString();
                    }
                    ////取得Log資訊 20160504 modify,cmd02_Action,cmd02_UseArm搬到else(Cmd02_Command != 0)裡面
                    cmd02_Action = GetRobotCommandActionDesc(curRobot.CurRealTimeSetCommandInfo.Cmd02_Command);
                    cmd02_UseArm = GetRobotCommandUseArmDesc(curRobot.CurRealTimeSetCommandInfo.Cmd02_ArmSelect, false);

                    #endregion
                }

                #endregion

                //取得Log資訊 20160504 modify,cmd02_Action,cmd02_UseArm搬到上面else (Cmd02_Command != 0)
                cmd01_Action = GetRobotCommandActionDesc(curRobot.CurRealTimeSetCommandInfo.Cmd01_Command);
                cmd01_UseArm = GetRobotCommandUseArmDesc(curRobot.CurRealTimeSetCommandInfo.Cmd01_ArmSelect, false);

                TimeSpan cmdResponseTime;

                cmdResponseTime = curRobot.CurRealTimeSetCommandInfo.CmdCreateDateTime.Subtract(curRobot.AutoModeStartDateTime);

                //20160412 MULTI_GET與MULTI_PUT 另外show log 
                //20160511 增加MULTIRECIPEGROUPEND_PUT
                if (curRobot.CurRealTimeSetCommandInfo.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_MULTI_GET && curRobot.CurRealTimeSetCommandInfo.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_MULTI_PUT &&
                    curRobot.CurRealTimeSetCommandInfo.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT)
                {
                    //下Robot Command 成功
                    ////通知OPI更新RobotCommand狀態 下Robot Cmd 並等候回應 EX:1'st[(3263,30),GET,LOWER,CM4(4),30,NONE] 2'nd[(3263,31),GET,UPPER,CM4(4),31,NONE], Success. 
                    cmdMsg = string.Format("{0} - {19} Mode 1'st[({1},{2}),{3},{4},{5}({6}),{7}] 2'nd[({8},{9}),{10},{11},{12}({13}),{14}], {15}, CommandStatus={16}. AutoMode StartTime({17}) Use ({18}) ms",
                                            funcName.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                            curRobot.CurRealTimeSetCommandInfo.Cmd01_CSTSeq, curRobot.CurRealTimeSetCommandInfo.Cmd01_JobSeq, cmd01_Action,
                                            cmd01_UseArm, cmd01_TargetPositionName, cmd01_TargetPositionID, cmd01_TargetSlotNo,
                                            curRobot.CurRealTimeSetCommandInfo.Cmd02_CSTSeq, curRobot.CurRealTimeSetCommandInfo.Cmd02_JobSeq, cmd02_Action,
                                            cmd02_UseArm, cmd02_TargetPositionName, cmd02_TargetPositionID, cmd02_TargetSlotNo,
                                            "Success", eRobot_ControlCommandStatus.CREATE, curRobot.AutoModeStartDateTime.ToString("yyyy-MM-dd hh:mm:ss.FFFF"), cmdResponseTime.Milliseconds,
                                            curRobot.File.curRobotRunMode);
                }
                else
                {
                    //下Robot Command 成功
                    ////通知OPI更新RobotCommand狀態 下Robot Cmd 並等候回應 EX:1'st[(3263,30),(3263,31),MULTI_GET,UPPER,CM4(4),30] 2'nd[NONE,NONE,NONE,NONE,NONE,NONE], Success. 
                    cmdMsg = string.Format("{0} - {14} Mode 1'st[({1},{2}),({3},{4}),{5},{6},{7}({8}),{9}] 2'nd[NONE,NONE,NONE,NONE,NONE,NONE], {10}, CommandStatus={11}. AutoMode StartTime({12}) Use ({13}) ms",
                        funcName.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                        curRobot.CurRealTimeSetCommandInfo.Cmd01_CSTSeq, curRobot.CurRealTimeSetCommandInfo.Cmd01_JobSeq,
                        curRobot.CurRealTimeSetCommandInfo.Cmd02_CSTSeq, curRobot.CurRealTimeSetCommandInfo.Cmd02_JobSeq, cmd01_Action,
                        cmd01_UseArm, cmd01_TargetPositionName, cmd01_TargetPositionID, cmd01_TargetSlotNo,
                        "Success", eRobot_ControlCommandStatus.CREATE, curRobot.AutoModeStartDateTime.ToString("yyyy-MM-dd hh:mm:ss.FFFF"), cmdResponseTime.Milliseconds,
                        curRobot.File.curRobotRunMode);
                }

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.NormalType });

                //20151016 add Send To OPI RobotRealTimeCommand to OPI
                Invoke(eServiceName.UIService, eInvokeOPIFunction.SendToOPI_RealTimeRobotCommandInfo, new object[] { curRobot });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> 將目前所下的Cell Special Robot Control Command 資訊送給OPI顯示並記錄Log
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="funcName"></param>
        private void SendCellSpecialRobotCommandInfoMessageToOPI(Robot curRobot, string funcName)
        {
            try
            {
                string cmdMsg = string.Empty;
                string strlog = string.Empty;

                string cmd01_Action = "NONE";
                string cmd01_UseArm = "NONE";
                string cmd01_TargetPositionID = "Unknown";
                string cmd01_TargetPositionName = "Unknown";
                string cmd01_TargetSlotNo = "NONE";

                string cmd02_Action = "NONE";
                string cmd02_UseArm = "NONE";
                string cmd02_TargetPositionID = "Unknown";
                string cmd02_TargetPositionName = "Unknown";
                string cmd02_TargetSlotNo = "NONE";

                string cmd03_Action = "NONE";
                string cmd03_UseArm = "NONE";
                string cmd03_TargetPositionID = "Unknown";
                string cmd03_TargetPositionName = "Unknown";
                string cmd03_TargetSlotNo = "NONE";

                string cmd04_Action = "NONE";
                string cmd04_UseArm = "NONE";
                string cmd04_TargetPositionID = "Unknown";
                string cmd04_TargetPositionName = "Unknown";
                string cmd04_TargetSlotNo = "NONE";


                #region [ 定義1st Command 顯示資訊 ]

                #region [取得CurCommand 1st Command Stage entity ]

                RobotStage cmd01_StageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                if (cmd01_StageEntity == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1st Command can not Find RobotStage by 1st Command TargetPosition({2})!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    cmd01_TargetPositionID = "Unknown";
                    cmd01_TargetPositionName = "Unknown";
                }
                else
                {
                    cmd01_TargetPositionID = cmd01_StageEntity.Data.STAGEID;
                    cmd01_TargetPositionName = cmd01_StageEntity.Data.STAGENAME; ;

                }

                #endregion

                #region [ 取得CurCommand 1st Command TargetSlotNo資訊 ]

                if (curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_TargetSlotNo.ToString() != string.Empty && curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_TargetSlotNo != 0)
                {
                    cmd01_TargetSlotNo = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_TargetSlotNo.ToString();
                }

                #endregion

                #endregion

                #region [ 定義2nd Command 顯示資訊 ]

                if (curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_Command == 0)
                {
                    cmd02_Action = "NONE";
                    cmd02_UseArm = "NONE";
                    cmd02_TargetPositionID = "NONE";
                    cmd02_TargetPositionName = "NONE";
                    cmd02_TargetSlotNo = "NONE";

                }
                else
                {

                    #region [ 取得CurCommand 2nd Command Stage entity ]

                    RobotStage cmd02_StageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_TargetPosition.ToString().PadLeft(2, '0'));

                    if (cmd02_StageEntity == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 2nd Command can not Find RobotStage by 2nd Command TargetPosition({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_TargetPosition.ToString().PadLeft(2, '0'));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        cmd02_TargetPositionID = "Unknown";
                        cmd02_TargetPositionName = "Unknown";
                    }
                    else
                    {
                        cmd02_TargetPositionID = cmd02_StageEntity.Data.STAGEID;
                        cmd02_TargetPositionName = cmd02_StageEntity.Data.STAGENAME; ;
                    }

                    #endregion

                    #region [ 取得CurCommand 2nd Command TargetSlotNo資訊 ]

                    if (curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_TargetSlotNo.ToString() != string.Empty && curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_TargetSlotNo != 0)
                    {
                        cmd02_TargetSlotNo = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_TargetSlotNo.ToString();
                    }


                    #endregion
                }

                #endregion

                #region [ 定義3rd Command 顯示資訊 ]

                if (curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_Command == 0)
                {
                    cmd03_Action = "NONE";
                    cmd03_UseArm = "NONE";
                    cmd03_TargetPositionID = "NONE";
                    cmd03_TargetPositionName = "NONE";
                    cmd03_TargetSlotNo = "NONE";

                }
                else
                {

                    #region [ 取得CurCommand 3rd Command Stage entity ]

                    RobotStage cmd03_StageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_TargetPosition.ToString().PadLeft(2, '0'));

                    if (cmd03_StageEntity == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 3rd Command can not Find RobotStage by 3rd Command TargetPosition({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_TargetPosition.ToString().PadLeft(2, '0'));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        cmd03_TargetPositionID = "Unknown";
                        cmd03_TargetPositionName = "Unknown";
                    }
                    else
                    {
                        cmd03_TargetPositionID = cmd03_StageEntity.Data.STAGEID;
                        cmd03_TargetPositionName = cmd03_StageEntity.Data.STAGENAME; ;
                    }

                    #endregion

                    #region [ 取得CurCommand 3rd Command TargetSlotNo資訊 ]

                    if (curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_TargetSlotNo.ToString() != string.Empty && curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_TargetSlotNo != 0)
                    {
                        cmd03_TargetSlotNo = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_TargetSlotNo.ToString();
                    }

                    #endregion
                  
                }

                #endregion

                #region [ 定義4th Command 顯示資訊 ]

                if (curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_Command == 0)
                {
                    cmd04_Action = "NONE";
                    cmd04_UseArm = "NONE";
                    cmd04_TargetPositionID = "NONE";
                    cmd04_TargetPositionName = "NONE";
                    cmd04_TargetSlotNo = "NONE";

                }
                else
                {

                    #region [ 取得CurCommand 4th Command Stage entity ]

                    RobotStage cmd04_StageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_TargetPosition.ToString().PadLeft(2, '0'));

                    if (cmd04_StageEntity == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 4th Command can not Find RobotStage by 4th Command TargetPosition({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_TargetPosition.ToString().PadLeft(2, '0'));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        cmd04_TargetPositionID = "Unknown";
                        cmd04_TargetPositionName = "Unknown";
                    }
                    else
                    {
                        cmd04_TargetPositionID = cmd04_StageEntity.Data.STAGEID;
                        cmd04_TargetPositionName = cmd04_StageEntity.Data.STAGENAME; ;
                    }

                    #endregion

                    #region [ 取得CurCommand 4th Command TargetSlotNo資訊 ]

                    if (curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_TargetSlotNo.ToString() != string.Empty && curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_TargetSlotNo != 0)
                    {
                        cmd04_TargetSlotNo = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_TargetSlotNo.ToString();
                    }

                    #endregion

                }

                #endregion

                //取得Log資訊
                cmd01_Action = GetRobotCommandActionDesc(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_Command);
                cmd01_UseArm = GetRobotCommandUseArmDesc(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_ArmSelect, true);
                cmd02_Action = GetRobotCommandActionDesc(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_Command);
                cmd02_UseArm = GetRobotCommandUseArmDesc(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_ArmSelect, true);
                cmd03_Action = GetRobotCommandActionDesc(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_Command);
                cmd03_UseArm = GetRobotCommandUseArmDesc(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_ArmSelect, true);
                cmd04_Action = GetRobotCommandActionDesc(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_Command);
                cmd04_UseArm = GetRobotCommandUseArmDesc(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_ArmSelect, true);

                TimeSpan cmdResponseTime;

                cmdResponseTime = curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdCreateDateTime.Subtract(curRobot.AutoModeStartDateTime);

                //下Robot Command 成功
                ////通知OPI更新RobotCommand狀態 下Robot Cmd 並等候回應 EX:AUTO Mode 1'st[Front(1098,3), Back(1098,4), GET,UP,CM2(02),3] 2'nd[(1098,3),PUT,UP,PM1(11),1], Success, 
                cmdMsg = string.Format("{0} - {1} Mode 1'st[Front({2},{3}), Back({4},{5}), {6}, {7}, {8}({9}), {10}], 2'nd[Front({11},{12}), Back({13},{14}), {15}, {16}, {17}({18}), {19}], 3'rd[Front({20},{21}), Back({22},{23}), {24}, {25}, {26}({27}), {28}], 4'th[Front({29},{30}), Back({31},{32}), {33}, {34}, {35}({36}), {37}], {38}, CommandStatus={39}. AutoMode StartTime({40}) Use ({41}) ms",
                                        funcName.PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '), curRobot.File.curRobotRunMode,
                                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontCSTSeq, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontJobSeq,
                                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackCSTSeq, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackJobSeq,
                                        cmd01_Action, cmd01_UseArm, cmd01_TargetPositionName, cmd01_TargetPositionID, cmd01_TargetSlotNo,
                                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_FrontCSTSeq, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_FrontJobSeq,
                                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_BackCSTSeq, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_BackJobSeq,
                                        cmd02_Action, cmd02_UseArm, cmd02_TargetPositionName, cmd02_TargetPositionID, cmd02_TargetSlotNo,
                                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_FrontCSTSeq, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_FrontJobSeq,
                                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_BackCSTSeq, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_BackJobSeq,
                                        cmd03_Action, cmd03_UseArm, cmd03_TargetPositionName, cmd03_TargetPositionID, cmd03_TargetSlotNo,
                                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_FrontCSTSeq, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_FrontJobSeq,
                                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_BackCSTSeq, curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_BackJobSeq,
                                        cmd04_Action, cmd04_UseArm, cmd04_TargetPositionName, cmd04_TargetPositionID, cmd04_TargetSlotNo,
                                        "Success", eRobot_ControlCommandStatus.CREATE, curRobot.AutoModeStartDateTime.ToString("yyyy-MM-dd hh:mm:ss.FFFF"), cmdResponseTime.Milliseconds);

                Invoke(eServiceName.UIService, "RobotCommandReport", new object[] { curRobot, cmdMsg, eSendToOPIMsgType.NormalType });

                //20151016 add Send To OPI RobotRealTimeCommand to OPI
                Invoke(eServiceName.UIService, eInvokeOPIFunction.SendToOPI_RealTimeRobotCommandInfo, new object[] { curRobot });

                strlog = string.Format("[{0}] {1}", "RobotCommandService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '), cmdMsg);
                Logger.LogTrxWrite(this.LogName, strlog);

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

    }
}
