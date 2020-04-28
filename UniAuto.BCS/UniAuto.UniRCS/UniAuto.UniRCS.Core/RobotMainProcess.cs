using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Threading;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.MISC;


namespace UniAuto.UniRCS.Core
{
    public partial class RobotCoreService
    {

        /// <summary>判斷是否可以下Robot Control Command
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        private void RobotMainProcess(Robot curRobot)
        {
            bool _isProc = true;
            int autoShowDetailLogCount = 100;
            string strlog = string.Empty;
            string cmdUsePortID = string.Empty;
            const string AUTOSHOWDETAILLOGINTERVAL="ROBOT_AUTO_SHOW_DETIAL_LOG_INTERVAL";
            List<RobotStage> curRobotStages = null;
            IList<string> cmdPortNos = new List<string>();

            try
            {
                if (ObjectManager.RobotManager.GetRobots() == null) return;
                    

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Robot({1}) Main Process Start", "L1", curRobot.Data.ROBOTNAME);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                switch(Workbench.LineType)
                {
                case eLineType.ARRAY.OVNSD_VIATRON:
                    StaticContext.AddParameter(eRobotContextParameter.TCOVN_SD_RobotParam, new TCOVN_SD_RobotParam());
                    break;
                case eLineType.CF.FCSRT_TYPE1:
                    StaticContext.AddParameter(eRobotContextParameter.FCSRT_RobotParam, new FCSRT_RobotParam());
                    break;
                }
                StaticContext.AddParameter(eRobotContextParameter.JobSendToSameEQ_RobotParam, new JobSendToSameEQ_RobotParam());
                StaticContext.AddParameter(eRobotContextParameter.FixTargetStage_RobotParam, new FixTargetStage_RobotParam());
                StaticContext.AddParameter(eRobotContextParameter.SorterMode_RobotParam, new SorterMode_RobotParam());

                #region [ 20151208 add Initial RobotArm Info ]

                //Robot has 2Arm ,One Arm has 1 Job
                for (int i = 0; i < curRobot.CurTempArmSingleJobInfoList.Length; i++)
                {
                    curRobot.CurTempArmSingleJobInfoList[i] = new RobotArmSignalSubstrateInfo();
                    curRobot.CurTempArmSingleJobInfoList[i].ArmCSTSeq = "0";
                    curRobot.CurTempArmSingleJobInfoList[i].ArmJobSeq = "0";
                    curRobot.CurTempArmSingleJobInfoList[i].ArmJobExist = eGlassExist.NoExist; //1:No Exist. 2:Exist
                    curRobot.CurTempArmSingleJobInfoList[i].ArmDisableFlag = eArmDisableStatus.Disable;
                }

                for (int i2 = 0; i2 < curRobot.CurRealTimeArmSingleJobInfoList.Length; i2++)
                {
                    curRobot.CurRealTimeArmSingleJobInfoList[i2] = new RobotArmSignalSubstrateInfo();
                    curRobot.CurRealTimeArmSingleJobInfoList[i2].ArmCSTSeq = "0";
                    curRobot.CurRealTimeArmSingleJobInfoList[i2].ArmJobSeq = "0";
                    curRobot.CurRealTimeArmSingleJobInfoList[i2].ArmJobExist = eGlassExist.NoExist; //1:No Exist. 2:Exist
                    curRobot.CurRealTimeArmSingleJobInfoList[i2].ArmDisableFlag = eArmDisableStatus.Disable;
                }

                //Robot has 4Arm, One Arm has 2 Job
                for (int j = 0; j < curRobot.CurTempArmDoubleJobInfoList.Length; j++)
                {
                    curRobot.CurTempArmDoubleJobInfoList[j] = new RobotArmDoubleSubstrateInfo();
                    curRobot.CurTempArmDoubleJobInfoList[j].ArmFrontCSTSeq = "0";
                    curRobot.CurTempArmDoubleJobInfoList[j].ArmFrontJobSeq = "0";
                    curRobot.CurTempArmDoubleJobInfoList[j].ArmFrontJobExist = eGlassExist.NoExist; //1:No Exist. 2:Exist
                    curRobot.CurTempArmDoubleJobInfoList[j].ArmBackCSTSeq = "0";
                    curRobot.CurTempArmDoubleJobInfoList[j].ArmBackJobSeq = "0";
                    curRobot.CurTempArmDoubleJobInfoList[j].ArmBackJobExist = eGlassExist.NoExist; //1:No Exist. 2:Exist
                    curRobot.CurTempArmDoubleJobInfoList[j].ArmDisableFlag = eArmDisableStatus.Disable;
                }

                for (int j2 = 0; j2 < curRobot.CurRealTimeArmDoubleJobInfoList.Length; j2++)
                {
                    curRobot.CurRealTimeArmDoubleJobInfoList[j2] = new RobotArmDoubleSubstrateInfo();
                    curRobot.CurRealTimeArmDoubleJobInfoList[j2].ArmFrontCSTSeq = "0";
                    curRobot.CurRealTimeArmDoubleJobInfoList[j2].ArmFrontJobSeq = "0";
                    curRobot.CurRealTimeArmDoubleJobInfoList[j2].ArmFrontJobExist = eGlassExist.NoExist; //1:No Exist. 2:Exist
                    curRobot.CurRealTimeArmDoubleJobInfoList[j2].ArmBackCSTSeq = "0";
                    curRobot.CurRealTimeArmDoubleJobInfoList[j2].ArmBackJobSeq = "0";
                    curRobot.CurRealTimeArmDoubleJobInfoList[j2].ArmBackJobExist = eGlassExist.NoExist; //1:No Exist. 2:Exist
                    curRobot.CurRealTimeArmDoubleJobInfoList[j2].ArmDisableFlag = eArmDisableStatus.Disable;
                }

                #endregion

                curRobot.File.RobotControlCommandEQPReplyBitFlag = false; //add by yang 2017/4/17 这边做下initial
                curRobot.File.CmdSendCondition = false; //add by yang 2017/4/18 这边做下initial

                while (_isProc)
                {
                    Thread.Sleep(eRobotCommonConst.ROBOT_MAIN_PROCESS_SLEEP);

                    try
                    {
                        if (Workbench.LineType == eLineType.ARRAY.OVNSD_VIATRON && StaticContext.ContainsKey(eRobotContextParameter.TCOVN_SD_RobotParam))
                        {
                            int timeout_ms = 500, tmp = 0;
                            if (ParameterManager.Parameters.ContainsKey("STAGE_GETGET_PUTPUT_WAIT_TIME") &&
                                int.TryParse(ParameterManager.Parameters["STAGE_GETGET_PUTPUT_WAIT_TIME"].Value.ToString(), out tmp))
                            {
                                timeout_ms = tmp;
                            }
                            ((TCOVN_SD_RobotParam)StaticContext[eRobotContextParameter.TCOVN_SD_RobotParam]).TimeoutMS = timeout_ms;
                        }
                        else if (Workbench.LineType == eLineType.CF.FCSRT_TYPE1 && StaticContext.ContainsKey(eRobotContextParameter.FCSRT_RobotParam))
                        {
                            int timeout_ms = 500, tmp = 0;
                            if (ParameterManager.Parameters.ContainsKey("STAGE_GETGET_PUTPUT_WAIT_TIME") &&
                                int.TryParse(ParameterManager.Parameters["STAGE_GETGET_PUTPUT_WAIT_TIME"].Value.ToString(), out tmp))
                            {
                                timeout_ms = tmp;
                            }
                            ((FCSRT_RobotParam)StaticContext[eRobotContextParameter.FCSRT_RobotParam]).TimeoutMS = timeout_ms;
                        }
                        if (StaticContext.ContainsKey(eRobotContextParameter.FixTargetStage_RobotParam))
                        {
                            int timeout_ms = 500, tmp = 0;
                            if (ParameterManager.Parameters.ContainsKey("FIX_STAGE_WAIT_TIME") &&
                                int.TryParse(ParameterManager.Parameters["FIX_STAGE_WAIT_TIME"].Value.ToString(), out tmp))
                            {
                                timeout_ms = tmp;
                            }
                            FixTargetStage_RobotParam fix_param = (FixTargetStage_RobotParam)StaticContext[eRobotContextParameter.FixTargetStage_RobotParam];
                            fix_param.TimeoutMS = timeout_ms;
                            fix_param.TimeoutState = FixTargetStage_RobotParam.TIMEOUT_STATE.CLEAR;//每次都恢復初始, Filter_PortFetchOutFixTargetStage會改值
                        }
                        ((JobSendToSameEQ_RobotParam)StaticContext[eRobotContextParameter.JobSendToSameEQ_RobotParam]).SameEQFlag = (curRobot.File.curRobotSameEQFlag == "Y");//檢測機, Cassette裡第一片上檢測機01, Cassette裡每一片都要進檢測機01
                        #region SorterMode_RobotParam
                        {
                            ((SorterMode_RobotParam)StaticContext[eRobotContextParameter.SorterMode_RobotParam]).NeedToCallCassetteService = SorterMode_RobotParam.NEED_TO_CALL_CST_SERVICE.NONE;//每次都恢復初始, CheckRobotControlCommand_For_TypeI_ForGetGetPutPut會改值
                            bool tmp = ((SorterMode_RobotParam)StaticContext[eRobotContextParameter.SorterMode_RobotParam]).EnableCallCassetteService;
                            if (ParameterManager.Parameters.ContainsKey("ROBOT_ENABLE_CALL_CASSETTE_SERVICE") &&
                                bool.TryParse(ParameterManager.Parameters["ROBOT_ENABLE_CALL_CASSETTE_SERVICE"].Value.ToString(), out tmp))
                            {
                                ((SorterMode_RobotParam)StaticContext[eRobotContextParameter.SorterMode_RobotParam]).EnableCallCassetteService = tmp;
                            }
                        }
                        #endregion
                        #region [ Get Robot Stages by Robot Name ]
                        if (curRobotStages == null)
                        {
                            curRobotStages = ObjectManager.RobotStageManager.GetRobotStages(curRobot.Data.ROBOTNAME);
                            if (curRobotStages == null) continue;
                            #region [Insert Empty Input DateTime]
                            //foreach (RobotStage rs in curRobotStages)
                            //{
                            //    if (rs.File.InputDateTime.ToString() == string.Empty)
                            //    {
                            //        rs.File.InputDateTime = DateTime.Now;
                            //    }
                            //}
                            #endregion
                        }
                        #endregion

                        if (curRobot.RobotStatusChangeFlag)
                        {
                            UpdateRobotStatusChangeShowDetailLogFlag(true);
                            curRobot.RobotStatusChangeFlag = false;
                        }

                        #region [ 取得Paramater 定義多少Cycle Show Detail Log ]
                        try
                        {
                            autoShowDetailLogCount = ParameterManager[AUTOSHOWDETAILLOGINTERVAL].GetInteger();
                        }
                        catch //(Exception ex1)
                        {
                            autoShowDetailLogCount = 0;
                        }
                        #endregion

                        #region [ 根據設定來計數 ]

                        if (autoShowDetailLogCount == 0)
                        {
                            //停止計算不Show Log
                            ErrorCircleTime = 0;
                            //上限恢復預設值
                            SHOWDETIALLOG_ERROR_TIME = 2000;
                        }
                        else
                        {
                            if (SHOWDETIALLOG_ERROR_TIME != autoShowDetailLogCount)
                            {
                                //變更上限
                                SHOWDETIALLOG_ERROR_TIME = autoShowDetailLogCount;
                            }

                            if (ErrorCircleTime < SHOWDETIALLOG_ERROR_TIME)
                            {
                                ErrorCircleTime++;
                            }
                            else
                            {
                                ErrorCircleTime = 0;
                            }
                        }

                        #endregion                      

                        //Real Time Get Robot Status
                        GetRobotStatusBlock(curRobot);

                        //Real Time Get Arm Job Info
                        GetRobotArmInfo(curRobot);

                        //Real Time Get Arm Job Info
                        GetEQLastSendOnTime(curRobot);

                        //Real Time ReSet Stage Priority
                        ReSetEQStagePriority(curRobot);                        

                        //20151125 add for Real Time 取得目前Eqp上報的CST Fetch Out SeqMode
                        GetEqpReportRobotFetchSeqMode(curRobot);

                        bool _openJobOnArmMove = false;

                        if (curRobot.File.curRobotRunMode == eRobot_RunMode.AUTO_MODE)
                        {
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Robot({1}) Auto Mode Start! {2}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, new string(eRobotCommonConst.MODE_START_CHAR, eRobotCommonConst.MODE_START_CHAR_LENGTH));
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            //Update Auto Mode StartTime
                            lock (curRobot)
                            {
                                curRobot.AutoModeStartDateTime = DateTime.Now;
                            }

                            #region [ By Robot Type Handle Auto Mode ][ Wait_Proc_0006 ]
                            //20151107 Modify Common改為ForGetGetPutPut
                            //if (Workbench.LineType == eLineType.ARRAY.IMP_NISSIN ||
                            //    Workbench.LineType == eLineType.ARRAY.CDO_VTECKMAC ||
                            //    Workbench.LineType == eLineType.ARRAY.CLS_MACAOH ||
                            //    Workbench.LineType == eLineType.ARRAY.CLS_PROCDO ||
                            //    Workbench.LineType == eLineType.ARRAY.OVNSD_VIATRON ||
                            //    Workbench.LineType == eLineType.ARRAY.MAC_CONTREL)
                            //{
                            //    CheckRobotControlCommand_For_TypeI_ForGetGetPutPut(curRobot, curRobotStages);
                            //}
                            //else if (Workbench.LineType == eLineType.ARRAY.OVNITO_CSUN ||
                            //         Workbench.LineType == eLineType.ARRAY.OVNPL_YAC)
                            //{
                            //    CheckRobotControlCommand_For_TCOVN(curRobot, curRobotStages);
                            //}
                            //else
                            //{
                            //    //for Robot Type I:Robot has 2 Arm ,each Arm only One Job.
                            //    CheckRobotControlCommand_For_TypeI(curRobot, curRobotStages);

                            //}

                            switch (Workbench.LineType)
                            {
                            case eLineType.ARRAY.OVNITO_CSUN:
                            case eLineType.ARRAY.OVNPL_YAC:
                                CheckRobotControlCommand_For_TCOVN(curRobot, curRobotStages);
                                break;

                            //20160118 add for Cell Special add to TypeII
                            case eLineType.CELL.CCRWT:
                            case eLineType.CELL.CCCHN:
                            case eLineType.CELL.CCSOR:
                            case eLineType.CELL.CCCRP:   //mark by yang 2017/2/27,all changed to ITC
                            case eLineType.CELL.CCCRP_2:   //mark by yang 2017/2/27,all changed to ITC
                                    if (ParameterManager.Parameters.ContainsKey("ROBOT_CELLSPECIALLINE_JOBONARM_MOVE")) 
                                    {
                                        bool.TryParse(ParameterManager.Parameters["ROBOT_CELLSPECIALLINE_JOBONARM_MOVE"].Value.ToString(), out _openJobOnArmMove);
                                        if(_openJobOnArmMove)
                                            CheckRobotControlCommand_For_TypeII_JobOnArmMove(curRobot, curRobotStages); //帶片跑
                                        else
                                            CheckRobotControlCommand_For_TypeII(curRobot, curRobotStages);
                                    }
                                    else
                                        CheckRobotControlCommand_For_TypeII(curRobot, curRobotStages);
                                //CheckRobotControlCommand_For_TypeII(curRobot, curRobotStages);
                                break;

                            default:
                                //for Robot Type I:Robot has 2 Arm ,each Arm only One Job.
                                //CheckRobotControlCommand_For_TypeI_ForGetGetPutPut(curRobot, curRobotStages);
                                if (ParameterManager.Parameters.ContainsKey("ROBOT_JOBONARM_MOVE"))
                                {
                                    bool.TryParse(ParameterManager.Parameters["ROBOT_JOBONARM_MOVE"].Value.ToString(), out _openJobOnArmMove);
                                    if (_openJobOnArmMove)
                                        CheckRobotControlCommand_For_TypeI_ForGetGetPutPutOnArmMove(curRobot, curRobotStages); //帶片跑
                                    else
                                        CheckRobotControlCommand_For_TypeI_ForGetGetPutPut(curRobot, curRobotStages);
                                }
                                else
                                    CheckRobotControlCommand_For_TypeI_ForGetGetPutPut(curRobot, curRobotStages);
                                break;
                            }
                            #endregion

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Robot({1}) Auto Mode End! {2}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, new string(eRobotCommonConst.MODE_END_CHAR, eRobotCommonConst.MODE_END_CHAR_LENGTH));
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                        }
                        else
                        {
                            #region [ Handle SEMI Mode ][ Wait_Proc_0014 ]
                            #region [ 1. Check Can Issue Command ]
                            //CheckRobotControlCommand_For_TypeI(curRobot, curRobotStages);

                            switch (Workbench.LineType)
                            {
                            case eLineType.ARRAY.OVNITO_CSUN:
                            case eLineType.ARRAY.OVNPL_YAC:
                                CheckRobotControlCommand_For_TCOVN(curRobot, curRobotStages);
                                break;

                            //20160118 add for Cell Special add to TypeII
                            case eLineType.CELL.CCRWT:
                            case eLineType.CELL.CCCHN:
                            case eLineType.CELL.CCSOR:
                            case eLineType.CELL.CCCRP:
                            case eLineType.CELL.CCCRP_2:
                                if (ParameterManager.Parameters.ContainsKey("ROBOT_CELLSPECIALLINE_JOBONARM_MOVE"))
                                {
                                    bool.TryParse(ParameterManager.Parameters["ROBOT_CELLSPECIALLINE_JOBONARM_MOVE"].Value.ToString(), out _openJobOnArmMove);
                                    if (_openJobOnArmMove)
                                        CheckRobotControlCommand_For_TypeII_JobOnArmMove(curRobot, curRobotStages); //帶片跑
                                    else
                                        CheckRobotControlCommand_For_TypeII(curRobot, curRobotStages);
                                }
                                else
                                    CheckRobotControlCommand_For_TypeII(curRobot, curRobotStages);
                                //CheckRobotControlCommand_For_TypeII(curRobot, curRobotStages);

                            break;

                            default:
                                //for Robot Type I:Robot has 2 Arm ,each Arm only One Job.
                                CheckRobotControlCommand_For_TypeI_ForGetGetPutPut(curRobot, curRobotStages);
                                break;
                            }
                            #endregion

                            #endregion
                        }

                        #region [ 根據目前Robot Real Time Command 取得因被Command鎖定而不可被退Port的Port List ]

                        cmdPortNos.Clear();

                        //Get 1st Command Use Port. Spec 定義Port Stage 1~10
                        if (curRobot.CurRealTimeSetCommandInfo.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE &&
                            (curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetPosition > 0 &&
                             curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetPosition < 11))
                        {
                            cmdUsePortID = curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetPosition.ToString().PadLeft(2, '0');

                            if (cmdPortNos.Contains(cmdUsePortID) == false)
                            {
                                cmdPortNos.Add(cmdUsePortID);
                            }

                        }

                        //Get 2nd Command Use Port. Spec 定義Port Stage 1~10
                        if (curRobot.CurRealTimeSetCommandInfo.Cmd02_Command != eRobot_Trx_CommandAction.ACTION_NONE &&
                            (curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition > 0 &&
                             curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition < 11))
                        {
                            cmdUsePortID = curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition.ToString().PadLeft(2, '0');

                            if (cmdPortNos.Contains(cmdUsePortID) == false)
                            {
                                cmdPortNos.Add(cmdUsePortID);
                            }

                        }

                        #endregion

                        //每次結束前通知BC判斷是否退Port                      
                        Invoke(eServiceName.CassetteService, "CassetteProcessEndScan", new object[] { cmdPortNos });
                    }
                    catch (Exception ex1)
                    {
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex1);
                        //異常結束時將Status Change Flag 清空
                        UpdateRobotStatusChangeShowDetailLogFlag(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> Get Robot Current Arm Info by Robot Arm Job Count
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        private void GetRobotArmInfo(Robot curRobot)
        {
            string trxID = string.Empty;
            string strlog = string.Empty;
            string strArmNo = string.Empty;

            try
            {
                if (curRobot.Data.ARMJOBQTY != 2)
                {

                    #region [ Arm has One Job Case ]

                    #region [ Trx Structure ]

                    // <trx name="L2_Arm#01SingleSubstrateInfoBlock" triggercondition="change">
                    //  <eventgroup name="L2_EG_Arm#01SingleSubstrateInfoBlock" dir="E2B">
                    //    <event name="L2_W_Arm#01SingleSubstrateInfoBlock" trigger="true" />
                    //  </eventgroup>
                    //</trx>

                    //<itemgroup name="Arm#02SingleSubstrateInfoBlock">
                    //  <item name="JobCassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //  <item name="JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //  <item name="JobExist" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //</itemgroup>

                    #endregion

                    Trx robotArmInfo_Trx = null;
                    string curRBArmCSTSeq = string.Empty;
                    string curRBArmJobSeq = string.Empty;
                    eGlassExist curRBArmExist;
                    eArmDisableStatus curRBArmDisableFlag;
                    int curRptArmJobExistDisableInfo = 0;

                    #region [ 20151208 add Get RealTimeArmInfo ]

                    for (int armIndex = 0; armIndex < curRobot.CurTempArmSingleJobInfoList.Length; armIndex++)
                    {

                        #region  [ Real time Get Trx ]

                        strArmNo = (armIndex + 1).ToString().PadLeft(2, '0');
                        trxID = string.Format("{0}_Arm#{1}SingleSubstrateInfoBlock", curRobot.Data.NODENO, strArmNo);
                        robotArmInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                        if (robotArmInfo_Trx == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not find TrxID({2})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, trxID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            continue;

                        }

                        #endregion

                        #region [拆出PLCAgent Data]

                        //<itemgroup name="Arm#02SingleSubstrateInfoBlock">
                        //  <item name="JobCassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                        //  <item name="JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                        //  <item name="JobExist" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                        //</itemgroup>

                        string trxEventGroupName = string.Format("{0}_EG_Arm#{1}SingleSubstrateInfoBlock", curRobot.Data.NODENO, strArmNo);
                        string trxEventName = string.Format("{0}_W_Arm#{1}SingleSubstrateInfoBlock", curRobot.Data.NODENO, strArmNo);
                        string trxItem_CSTSeq = "JobCassetteSequenceNo";
                        string trxItem_JobSeq = "JobSequenceNo";
                        string trxItem_JobExist = "JobExist";

                        curRBArmCSTSeq = robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_CSTSeq].Value;
                        curRBArmJobSeq = robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_JobSeq].Value;

                        #region [ 20151015 Modify Glass Exist 有變動所以要改 ]
                        //0：Unknown
                        //1：No Exist(bit0)
                        //2：Exist(bit1)
                        //4：Arm Disabled(bit2)
                        //5：Arm Disabled & No Exist Job
                        //6：Arm Disable & Exist Job

                        switch (int.Parse(robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_JobExist].Value))
                        {
                            case 0:

                                curRBArmExist = eGlassExist.Unknown;
                                curRBArmDisableFlag = eArmDisableStatus.Enable;

                                break;

                            case 1:

                                curRBArmExist = eGlassExist.NoExist;
                                curRBArmDisableFlag = eArmDisableStatus.Enable;

                                break;

                            case 2:

                                curRBArmExist = eGlassExist.Exist;
                                curRBArmDisableFlag = eArmDisableStatus.Enable;

                                break;

                            case 4:

                                curRBArmExist = eGlassExist.Unknown;
                                curRBArmDisableFlag = eArmDisableStatus.Disable;

                                break;

                            case 5:

                                curRBArmExist = eGlassExist.NoExist;
                                curRBArmDisableFlag = eArmDisableStatus.Disable;

                                break;

                            case 6:

                                curRBArmExist = eGlassExist.Exist;
                                curRBArmDisableFlag = eArmDisableStatus.Disable;

                                break;

                            default:

                                //超出SPEC定義範圍視為不啟用+未知
                                curRBArmExist = eGlassExist.Unknown;
                                curRBArmDisableFlag = eArmDisableStatus.Disable;
                                break;
                        }

                        //curRBArmExist = (eGlassExist)int.Parse(robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_JobExist].Value);

                        curRptArmJobExistDisableInfo = int.Parse(robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_JobExist].Value);

                        #endregion

                        #endregion

                        //同步更新運算用Arm資訊
                        lock (curRobot)
                        {
                            curRobot.CurTempArmSingleJobInfoList[armIndex].ArmCSTSeq = curRBArmCSTSeq;
                            curRobot.CurTempArmSingleJobInfoList[armIndex].ArmJobSeq = curRBArmJobSeq;
                            curRobot.CurTempArmSingleJobInfoList[armIndex].ArmJobExist = curRBArmExist;
                            curRobot.CurTempArmSingleJobInfoList[armIndex].ArmDisableFlag = curRBArmDisableFlag;
                            curRobot.CurTempArmSingleJobInfoList[armIndex].CurRptArmJobExistDisableInfo = curRptArmJobExistDisableInfo;
                        }

                        #region [ Real Time有變化才更新資料 ]

                        if (curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmCSTSeq != curRBArmCSTSeq ||
                            curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmJobSeq != curRBArmJobSeq ||
                            curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmJobExist != curRBArmExist ||
                            curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmDisableFlag != curRBArmDisableFlag ||
                            curRobot.CurRealTimeArmSingleJobInfoList[armIndex].CurRptArmJobExistDisableInfo != curRptArmJobExistDisableInfo)
                        {
                            //紀錄Log 20151231 Update Log error
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}), Arm({2}) Info Change! CassetteSequenceNo({3}) to ({4}), JobSequenceNo({5}) to ({6}), Job Exist Status({7}) to ({8}), Arm Disable Status({9}) to ({10})",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, strArmNo, curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmCSTSeq,
                                                    curRBArmCSTSeq, curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmJobSeq, curRBArmJobSeq, curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmJobExist,
                                                    curRBArmExist.ToString(), curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmDisableFlag, curRBArmDisableFlag.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            //add for Log Trace 20151231 Update Log error
                            strlog = string.Format("[{0}][{1} - Robot({2}), Arm({3}) Info Change! CassetteSequenceNo({4}) to ({5}), JobSequenceNo({6}) to ({7}), Job Exist Status({8}) to ({9}), Arm Disable Status({10}) to ({11})",
                                                    "RobotCoreService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                                    "GetRobotArmInfo".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                                    curRobot.Data.ROBOTNAME,
                                                    strArmNo,
                                                    curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmCSTSeq, curRBArmCSTSeq, curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmJobSeq, curRBArmJobSeq,
                                                    curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmJobExist, curRBArmExist.ToString(),
                                                    curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmDisableFlag, curRBArmDisableFlag.ToString());

                            Logger.LogTrxWrite(this.LogName, strlog);

                            lock (curRobot)
                            {
                                curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmCSTSeq = curRBArmCSTSeq;
                                curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmJobSeq = curRBArmJobSeq;
                                curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmJobExist = curRBArmExist;
                                curRobot.CurRealTimeArmSingleJobInfoList[armIndex].ArmDisableFlag = curRBArmDisableFlag;
                                curRobot.CurRealTimeArmSingleJobInfoList[armIndex].CurRptArmJobExistDisableInfo = curRptArmJobExistDisableInfo;
                            }

                            //Real Time資訊不須存入檔案

                            //通知OPI更新LayOut畫面
                            Invoke(eServiceName.UIService, "RobotCurrentModeReport", new object[] { curRobot });

                        }
                       
                        #endregion

                    }

                    #endregion

                    #endregion

                }
                else
                {

                    #region [ Arm has Two Job Case ] 

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

                    Trx robotArmInfo_Trx = null;
                    string curRBArmFrontCSTSeq = string.Empty;
                    string curRBArmFrontJobSeq = string.Empty;
                    eGlassExist curRBArmFrontExist;
                    string curRBArmBackCSTSeq = string.Empty;
                    string curRBArmBackJobSeq = string.Empty;
                    eGlassExist curRBArmBackExist;
                    eArmDisableStatus curRBArmFrontDisableFlag;
                    eArmDisableStatus curRBArmBackDisableFlag;
                    int curRptArmFrontJobExistDisableInfo = 0;
                    int curRptArmBackJobExistDisableInfo = 0;
                    eArmDisableStatus curRBArmDisableFlag;

                    for (int armIndex = 0; armIndex < curRobot.CurTempArmDoubleJobInfoList.Length; armIndex++)
                    {

                        #region  [ Real time Get Trx ]

                        strArmNo = (armIndex + 1).ToString().PadLeft(2, '0');
                        trxID = string.Format("{0}_Arm#{1}DoubleSubstrateInfoBlock", curRobot.Data.NODENO, strArmNo);
                        robotArmInfo_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                        if (robotArmInfo_Trx == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not find TrxID({2})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, trxID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            continue;

                        }

                        #endregion

                        #region [ 拆出PLCAgent Data ]

                        //<itemgroup name="Arm#02DoubleSubstrateInfoBlock">
                        //  <item name="ForkFrontEndJobCassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                        //  <item name="ForkFrontEndJobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                        //  <item name="ForkFrontEndJobExist" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                        //  <item name="ForkBackEndJobCassetteSequenceNo" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                        //  <item name="ForkBackEndJobSequenceNo" woffset="4" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                        //  <item name="ForkBackEndJobExist" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                        //</itemgroup>

                        string trxEventGroupName = string.Format("{0}_EG_Arm#{1}DoubleSubstrateInfoBlock", curRobot.Data.NODENO, strArmNo);
                        string trxEventName = string.Format("{0}_W_Arm#{1}DoubleSubstrateInfoBlock", curRobot.Data.NODENO, strArmNo);
                        string trxItem_Front_CSTSeq = "ForkFrontEndJobCassetteSequenceNo";
                        string trxItem_Front_JobSeq = "ForkFrontEndJobSequenceNo";
                        string trxItem_Front_JobExist = "ForkFrontEndJobExist";
                        string trxItem_Back_CSTSeq = "ForkBackEndJobCassetteSequenceNo";
                        string trxItem_Back_JobSeq = "ForkBackEndJobSequenceNo";
                        string trxItem_Back_JobExist = "ForkBackEndJobExist";

                        curRBArmFrontCSTSeq = robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_Front_CSTSeq].Value;
                        curRBArmFrontJobSeq = robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_Front_JobSeq].Value;
                        curRBArmFrontExist = (eGlassExist)int.Parse(robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_Front_JobExist].Value);

                        curRBArmBackCSTSeq = robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_Back_CSTSeq].Value;
                        curRBArmBackJobSeq = robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_Back_JobSeq].Value;
                        curRBArmBackExist = (eGlassExist)int.Parse(robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_Back_JobExist].Value);

                        #region [ Get Exist and Disable Info ]

                        //0：Unknown
                        //1：No Exist(bit0)
                        //2：Exist(bit1)
                        //4：Arm Disabled(bit2)
                        //5：Arm Disabled & No Exist Job
                        //6：Arm Disable & Exist Job

                        #region [ Update Front Job ]

                        switch (int.Parse(robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_Front_JobExist].Value))
                        {
                            case 0:

                                curRBArmFrontExist = eGlassExist.Unknown;
                                curRBArmFrontDisableFlag = eArmDisableStatus.Enable;

                                break;

                            case 1:

                                curRBArmFrontExist = eGlassExist.NoExist;
                                curRBArmFrontDisableFlag = eArmDisableStatus.Enable;

                                break;

                            case 2:

                                curRBArmFrontExist = eGlassExist.Exist;
                                curRBArmFrontDisableFlag = eArmDisableStatus.Enable;

                                break;

                            case 4:

                                curRBArmFrontExist = eGlassExist.Unknown;
                                curRBArmFrontDisableFlag = eArmDisableStatus.Disable;

                                break;

                            case 5:

                                curRBArmFrontExist = eGlassExist.NoExist;
                                curRBArmFrontDisableFlag = eArmDisableStatus.Disable;

                                break;

                            case 6:

                                curRBArmFrontExist = eGlassExist.Exist;
                                curRBArmFrontDisableFlag = eArmDisableStatus.Disable;

                                break;

                            default:

                                //超出SPEC定義範圍視為不啟用+未知
                                curRBArmFrontExist = eGlassExist.Unknown;
                                curRBArmFrontDisableFlag = eArmDisableStatus.Disable;
                                break;
                        }

                        //紀錄即時值
                        curRptArmFrontJobExistDisableInfo = int.Parse(robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_Front_JobExist].Value);

                        #endregion

                        #region [ Update Back Job ]

                        switch (int.Parse(robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_Back_JobExist].Value))
                        {
                            case 0:

                                curRBArmBackExist = eGlassExist.Unknown;
                                curRBArmBackDisableFlag = eArmDisableStatus.Enable;

                                break;

                            case 1:

                                curRBArmBackExist = eGlassExist.NoExist;
                                curRBArmBackDisableFlag = eArmDisableStatus.Enable;

                                break;

                            case 2:

                                curRBArmBackExist = eGlassExist.Exist;
                                curRBArmBackDisableFlag = eArmDisableStatus.Enable;

                                break;

                            case 4:

                                curRBArmBackExist = eGlassExist.Unknown;
                                curRBArmBackDisableFlag = eArmDisableStatus.Disable;

                                break;

                            case 5:

                                curRBArmBackExist = eGlassExist.NoExist;
                                curRBArmBackDisableFlag = eArmDisableStatus.Disable;

                                break;

                            case 6:

                                curRBArmBackExist = eGlassExist.Exist;
                                curRBArmBackDisableFlag = eArmDisableStatus.Disable;

                                break;

                            default:

                                //超出SPEC定義範圍視為不啟用+未知
                                curRBArmBackExist = eGlassExist.Unknown;
                                curRBArmBackDisableFlag = eArmDisableStatus.Disable;
                                break;
                        }

                        //紀錄即時值
                        curRptArmBackJobExistDisableInfo = int.Parse(robotArmInfo_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_Back_JobExist].Value);

                        #endregion

                        //20160113 modify Front and Back 需要都Enable才算是Enable
                        if((curRBArmFrontDisableFlag == eArmDisableStatus.Enable) && (curRBArmBackDisableFlag == eArmDisableStatus.Enable))
                        {

                            curRBArmDisableFlag= eArmDisableStatus.Enable;
                        }
                        else
                        {
                            curRBArmDisableFlag= eArmDisableStatus.Disable;
                        }
                        
                        #endregion

                        #endregion

                        //同步更新運算用Arm資訊
                        lock (curRobot)
                        {

                            curRobot.CurTempArmDoubleJobInfoList[armIndex].ArmFrontCSTSeq = curRBArmFrontCSTSeq;
                            curRobot.CurTempArmDoubleJobInfoList[armIndex].ArmFrontJobSeq = curRBArmFrontJobSeq;
                            curRobot.CurTempArmDoubleJobInfoList[armIndex].ArmFrontJobExist = curRBArmFrontExist;
                            curRobot.CurTempArmDoubleJobInfoList[armIndex].CurRptArmFrontJobExistDisableInfo = curRptArmFrontJobExistDisableInfo;
                            curRobot.CurTempArmDoubleJobInfoList[armIndex].ArmBackCSTSeq = curRBArmBackCSTSeq;
                            curRobot.CurTempArmDoubleJobInfoList[armIndex].ArmBackJobSeq = curRBArmBackJobSeq;
                            curRobot.CurTempArmDoubleJobInfoList[armIndex].ArmBackJobExist = curRBArmBackExist;
                            curRobot.CurTempArmDoubleJobInfoList[armIndex].CurRptArmBackJobExistDisableInfo = curRptArmBackJobExistDisableInfo;
                            curRobot.CurTempArmDoubleJobInfoList[armIndex].ArmDisableFlag = curRBArmDisableFlag;

                        }

                        #region [ 有變化才更新資料 ]

                        //[ Wait_Proc_00050 ] 尚未針對Arm 2片的處理
                        if (curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontCSTSeq != curRBArmFrontCSTSeq ||
                            curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontJobSeq != curRBArmFrontJobSeq ||
                            curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontJobExist != curRBArmFrontExist ||
                            curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackCSTSeq != curRBArmBackCSTSeq ||
                            curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackJobSeq != curRBArmBackJobSeq ||
                            curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackJobExist != curRBArmBackExist ||
                            curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].CurRptArmFrontJobExistDisableInfo != curRptArmFrontJobExistDisableInfo ||
                            curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].CurRptArmBackJobExistDisableInfo != curRptArmBackJobExistDisableInfo)
                        {

                            //紀錄Log
                            strlog = string.Format("[EQUIPMENT=[{0}] [RCS <- RCS] Robot({1}), Arm({2}) Info Change! FrontEnd CassetteSequenceNo({3}) to ({4}), JobSequenceNo({5}) to ({6}), Job Exist Status({7}) to ({8}), ExistandDisableInfo({9}) to ({10}), BackEnd CassetteSequenceNo({11}) to ({12}), JobSequenceNo({13}) to ({14}), Job Exist Status({15}) to ({16}), ExistandDisableInfo({17}) to ({18}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, strArmNo, curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontCSTSeq,
                                                    curRBArmFrontCSTSeq, curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontJobSeq, curRBArmFrontJobSeq, curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontJobExist,
                                                    curRBArmFrontExist.ToString(), curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].CurRptArmFrontJobExistDisableInfo, curRptArmFrontJobExistDisableInfo, 
                                                    curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackCSTSeq, curRBArmBackCSTSeq, curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackJobSeq,
                                                    curRBArmBackJobSeq, curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackJobExist, curRBArmBackExist.ToString(), 
                                                    curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].CurRptArmBackJobExistDisableInfo, curRptArmBackJobExistDisableInfo);

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            //add for Log Trace //20160303 fix Log bug
                            strlog = string.Format("{0}][{1} - Robot({2}), Arm({3}) Info Change! FrontEnd CassetteSequenceNo({4}) to ({5}), JobSequenceNo({6}) to ({7}), Job Exist Status({8}) to ({9}), ExistandDisableInfo({10}) to ({11}), BackEnd CassetteSequenceNo({12}) to ({13}), JobSequenceNo({14}) to ({15}), Job Exist Status({16}) to ({17}), ExistandDisableInfo({18}) to ({19}).",
                                                    "RobotCoreService".PadRight(eRobotCommonConst.LOG_SERVICE_LENGTH, ' '),
                                                    "GetRobotArmInfo".PadRight(eRobotCommonConst.LOG_FUNCTION_LENGTH, ' '),
                                                    curRobot.Data.ROBOTNAME,
                                                    strArmNo,
                                                    curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontCSTSeq,
                                                    curRBArmFrontCSTSeq, curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontJobSeq, curRBArmFrontJobSeq, curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontJobExist.ToString(),
                                                    curRBArmFrontExist.ToString(), curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].CurRptArmFrontJobExistDisableInfo.ToString(), curRptArmFrontJobExistDisableInfo, 
                                                    curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackCSTSeq, curRBArmBackCSTSeq, curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackJobSeq,
                                                    curRBArmBackJobSeq, curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackJobExist, curRBArmBackExist.ToString(),
                                                    curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].CurRptArmBackJobExistDisableInfo.ToString(), curRptArmBackJobExistDisableInfo.ToString());

                            Logger.LogTrxWrite(this.LogName, strlog);

                            lock (curRobot.File)
                            {

                                curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontCSTSeq = curRBArmFrontCSTSeq;
                                curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontJobSeq = curRBArmFrontJobSeq;
                                curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmFrontJobExist = curRBArmFrontExist;
                                curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackCSTSeq = curRBArmBackCSTSeq;
                                curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackJobSeq = curRBArmBackJobSeq;
                                curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].ArmBackJobExist = curRBArmBackExist;
                                curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].CurRptArmFrontJobExistDisableInfo = curRptArmFrontJobExistDisableInfo;
                                curRobot.CurRealTimeArmDoubleJobInfoList[armIndex].CurRptArmBackJobExistDisableInfo = curRptArmBackJobExistDisableInfo;
                            }

                            //Real Time 不存入File
                            //ObjectManager.RobotManager.EnqueueSave(curRobot.File);

                            //通知OPI更新LayOut畫面
                            Invoke(eServiceName.UIService, "RobotCurrentModeReport", new object[] { curRobot });

                        }

                        #endregion

                    }

                    #endregion

                }

            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        private void GetRobotStatusBlock(Robot curRobot)
        {
            try
            {
                string trxID = string.Empty;
                string strlog = string.Empty;
                Trx robotStatusBlock_Trx = null;
                string robotStatusChangeData = string.Empty;
                string robotHasCommand = string.Empty;
                trxID = string.Format("{0}_RobotStatusBlock", curRobot.Data.NODENO);
                robotStatusBlock_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                if (robotStatusBlock_Trx == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not find TrxID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, trxID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                }
                string trxEventGroupName = string.Format("{0}_EG_RobotStatusBlock", curRobot.Data.NODENO);
                string trxEventName = string.Format("{0}_W_RobotStatusBlock", curRobot.Data.NODENO);
                string trxItem_RobotStatusChangeData = "RobotStatusChangeData";
                string trxItem_RobotHasCommand = "RobotHasCommand";
                robotStatusChangeData = robotStatusBlock_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_RobotStatusChangeData].Value;
                robotHasCommand = robotStatusBlock_Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_RobotHasCommand].Value;
                eRobotStatus newrobotStatusChangeData = (eRobotStatus)int.Parse(robotStatusChangeData);
                eRobotHasCommandStatus newrobotHasCommand = (eRobotHasCommandStatus)int.Parse(robotHasCommand);

                #region [ 當Status變成Running時要清除RobotCmd Active TimeOut ]

                if (curRobot.File.Status == eRobotStatus.RUNNING && curRobot.File.Status != newrobotStatusChangeData)
                {
                    string timeName = string.Format("{0}_{1}_{2}", robotStatusBlock_Trx.Metadata.NodeNo, curRobot.Data.ROBOTNAME, eRobotCommonConst.ROBOT_CONTROL_COMMAND_ACTIVE_TIMEOUT_EVENTNAME);

                    if (_timerManager.IsAliveTimer(timeName))
                    {
                        _timerManager.TerminateTimer(timeName);
                    }

                }
                #endregion
                if (curRobot.File.Status != newrobotStatusChangeData || curRobot.File.RobotHasCommandstatus != newrobotHasCommand)
                {
                    //紀錄Log 20151231 Update Log error
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Status Change From ({2}) to ({3}), has Command Status from ({4}) to ({5})", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.Status, newrobotStatusChangeData, curRobot.File.RobotHasCommandstatus, newrobotHasCommand);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //add for Log Trace 20151231 Update Log error
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Status Change From ({2}) to ({3}), has Command Status from ({4}) to ({5})", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.Status, newrobotStatusChangeData, curRobot.File.RobotHasCommandstatus, newrobotHasCommand);

                    Logger.LogTrxWrite(this.LogName, strlog);

                    lock (curRobot)
                    {
                        curRobot.File.Status = newrobotStatusChangeData;
                        curRobot.File.RobotHasCommandstatus = newrobotHasCommand;
                    }
                    //通知OPI更新LayOut畫面
                    Invoke(eServiceName.UIService, "RobotCurrentModeReport", new object[] { curRobot });
                }
            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        public class cur1stJob_CommandInfo
        {
            /// <summary>目前第一片Job產生的所有Job資訊
            /// 
            /// </summary>
            public RobotCmdInfo cur1stJob_Command;
            /// <summary>目前第一片Job 第一個命令的動作(GET/PUT...)
            /// 
            /// </summary>
            public string cur1stJob_1stCommand_DBActionCode;
            /// <summary>目前第一片Job 第一個命令存取的RobotStage資訊
            /// 
            /// </summary>
            public RobotStage cur1stJob_1stCommand_TargetStageEntity;
            /// <summary>目前第一片Job 第二個命令的動作(GET/PUT...)
            /// 
            /// </summary>
            public string cur1stJob_2ndCommand_DBActionCode;
            /// <summary>目前第一片Job 第二個命令存取的RobotStage資訊
            /// 
            /// </summary>
            public RobotStage cur1stJob_2ndCommand_TargetStageEntity;

            //20151110 add for 1st/2nd Cmd CrossStageFlag
            public string cur1stJob_1stCommand_CrossStageFlag;
            public string cur1stJob_2ndCommand_CrossStageFlag;

            public cur1stJob_CommandInfo()
            {
                cur1stJob_Command = new RobotCmdInfo();
                cur1stJob_1stCommand_DBActionCode = string.Empty;
                cur1stJob_1stCommand_TargetStageEntity = null;
                cur1stJob_2ndCommand_DBActionCode = string.Empty;
                cur1stJob_2ndCommand_TargetStageEntity = null;
                cur1stJob_1stCommand_CrossStageFlag = "N";
                cur1stJob_2ndCommand_CrossStageFlag = "N";
            }
        }


        /// <summary> 判斷Robot目前是否可以下命令
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <returns></returns>
        private bool CheckCanIssueRobotCommand(Robot curRobot)
        {
            string strlog = string.Empty;
            string cmdMsg = string.Empty;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
            bool clearFlag = false;

            try
            {
                #region [Check Robot Command Status]
                //20151204 add for Cell Special Use
                if (curRobot.Data.ARMJOBQTY == 2)
                {

                    #region [ Check Cell Special Robot Control Command Status is Can Issue Command Status(EMPTY ot COMPLETE or CLEAR)

                    //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00002 ]
                    fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOTCMD_STATUS_FAIL;

                    if (curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus.Trim() != eRobot_ControlCommandStatus.EMPTY &&
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus.Trim() != eRobot_ControlCommandStatus.COMPLETE &&
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus.Trim() != eRobot_ControlCommandStatus.CANCEL &&
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus.Trim() != string.Empty)
                    {

                        //Abnormal:假如Robot曾經變成Running再變成Idle但是沒清除CmdStatus(沒上報Result)則等到RT2 TimeOut視為異常結束再清空狀態.

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) Command Status is ({3}) can not issue Robot Command!)",
                                                    curRobot.Data.NODENO, fail_ReasonCode, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        #region  [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00002 ]

                        if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                        {

                            //strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Command Status is ({2}) can not issue Robot Command!",
                            //                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus);
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) Command Status is ({3}) can not issue Robot Command!)",
                                                    curRobot.Data.NODENO, fail_ReasonCode, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("Robot({0}) Command Status is ({1})!",
                            //                        curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus);
                            failMsg = string.Format("RtnCode({0}) RtnMsg([{1}] Robot({2}) Command Status is ({3})!)",
                                                    fail_ReasonCode,MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.CurCellSpecialRealTimeSetCommandInfo.CurRobotCommandStatus);

                            AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion

                        }

                        #endregion

                        return false;

                    }
                    else
                    {
                        //Clear[ Robot_Fail_Case_00002 ]
                        RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                    }

                    #endregion

                }
                else
                {

                    #region [ Check Robot Control Command Status is Can Issue Command Status(EMPTY ot COMPLETE or CLEAR)

                    //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00002 ]
                    fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOTCMD_STATUS_FAIL;

                    if (curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus.Trim() != eRobot_ControlCommandStatus.EMPTY &&
                        curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus.Trim() != eRobot_ControlCommandStatus.COMPLETE &&
                        curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus.Trim() != eRobot_ControlCommandStatus.CANCEL &&
                        curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus.Trim() != string.Empty)
                    {

                        //Abnormal:假如Robot曾經變成Running再變成Idle但是沒清除CmdStatus(沒上報Result)則等到RT2 TimeOut視為異常結束再清空狀態.

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) Command Status is ({3}) can not issue Robot Command!)",
                                                    curRobot.Data.NODENO, fail_ReasonCode, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        #region  [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00002 ]

                        if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) Command Status is ({3}) can not issue Robot Command!)",
                                                    curRobot.Data.NODENO, fail_ReasonCode, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            failMsg = string.Format("RtnCode({0}) RtnMsg([{1}] Robot({2}) Command Status is ({3})!)",
                                                    fail_ReasonCode,MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.CurRealTimeSetCommandInfo.CurRobotCommandStatus);

                            AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion

                        }

                        #endregion

                        return false;

                    }
                    else
                    {
                        //Clear[ Robot_Fail_Case_00002 ]
                        RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                    }

                    #endregion

                }
                #endregion

                #region [ Check Robot Status is Idle ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00001 ]
                fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_STATUS_FAIL;

                if (curRobot.File.Status != eRobotStatus.IDLE)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) Status is ({3}) is not (IDLE) can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.File.Status.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00001 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) Status is ({3}) is not (IDLE) can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode, curRobot.Data.ROBOTNAME, curRobot.File.Status.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("RtnCode({0}) RtnMsg([{1}] Robot({2}) Status({3}) is not (IDLE)!)",
                                                fail_ReasonCode,MethodBase.GetCurrentMethod().Name,curRobot.Data.ROBOTNAME, curRobot.File.Status.ToString());

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00001 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                #region [ Check No Command On Robot ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00007 ]
                fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_NO_COMMAND_ON_ROBOT_FAIL;

                if (curRobot.File.RobotHasCommandstatus != eRobotHasCommandStatus.NO_COMMAND_ON_ROBOT)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) has Command Status is ({3}) is not (NO_COMMAND_ON_ROBOT) can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.File.RobotHasCommandstatus.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00007 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) has Command Status is ({3}) is not (NO_COMMAND_ON_ROBOT) can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.File.RobotHasCommandstatus.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("RtnCode({0}) Robot({1}) has Command Status({2}) is not (NO_COMMAND_ON_ROBOT)!)",
                                                fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.File.RobotHasCommandstatus.ToString());

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00007 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                #region [ Check Robot Docking EQP is Exist ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00003 ]
                fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_FIND_ROBOT_EQP_FAIL;

                Equipment curRobotEQP = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                if (curRobotEQP == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) can not Find EQP Entity by Robot NodeNo({3}) and can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00003 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) can not Find EQP Entity by Robot NodeNo({3}) and can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("RtnCode({0}) RtnMsg([{1}] can not Find EQP({2}) Entity!)",
                                                 fail_ReasonCode,MethodBase.GetCurrentMethod().Name,curRobot.Data.NODENO);

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00003 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                #region [ Check Robot Docking EQP is CIM On ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00004 ]
                fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_EQP_CIM_ON_FAIL;

                if (curRobotEQP.File.CIMMode != eBitResult.ON)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) NodeNo({3}) CIM Mode is({4}) and can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.Data.NODENO, curRobotEQP.File.CIMMode.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00004 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) NodeNo({3}) CIM Mode is({4}) and can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.Data.NODENO, curRobotEQP.File.CIMMode.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("RtnCode({0}) RtnMsg([{1}]EQP({2}) CIM Mode is ({3})!)",
                                                 fail_ReasonCode,MethodBase.GetCurrentMethod().Name,curRobot.Data.NODENO, curRobotEQP.File.CIMMode.ToString());

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00004 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                #region [ Check Indexer Status  is Down ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00012 ]
                fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_FETCHPORT_BUT_INDEXER_STATUS_IS_DOWN;

                if (curRobotEQP.File.Status == eEQPStatus.STOP)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) NodeNo({3}) EQP Status is 'STOP' (Down) and can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00012 ]
                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) NodeNo({3}) EQP Status is STOP (DOWN) and can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                        failMsg = string.Format("RtnCode({0}) RtnMsg([{1}]EQP({2}) Status is STOP (Down)!)",
                                                 fail_ReasonCode,MethodBase.GetCurrentMethod().Name,curRobot.Data.NODENO);
                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                        #endregion
                    }
                    #endregion
                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00004 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                #region [ Check Robot Docking EQP Operation Mode is not Manual ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00008 ]
                fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_EQP_OPERATIONMODE_IS_MANUAL;

                if (curRobotEQP.File.EquipmentOperationMode == eEQPOperationMode.MANUAL)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) NodeNo({3}) Operation Mode is({4}) and can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.Data.NODENO, curRobotEQP.File.EquipmentOperationMode.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00008 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) NodeNo({3}) Operation Mode is({4}) and can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME, curRobot.Data.NODENO, curRobotEQP.File.EquipmentOperationMode.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("RtnCode({0}) RtnMsg([{1}]EQP({2}) Operation Mode is ({3})!)",
                                                 fail_ReasonCode,MethodBase.GetCurrentMethod().Name,curRobot.Data.NODENO, curRobotEQP.File.EquipmentOperationMode.ToString());

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00008 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                #region [ Check Robot Control Command Send Reply Bit must Off ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00005 ]
                fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_CMD_REPLY_BIT_OFF_FAIL;

                if (curRobot.File.RobotControlCommandEQPReplyBitFlag == true)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) Robot Control Command EQP Reply Bit is (ON) and can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00005 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] RtnCode({1}) RtnMsg(Robot({2}) Robot Control Command EQP Reply Bit is (ON) and can not issue Robot Command!)",
                                                curRobot.Data.NODENO, fail_ReasonCode,curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("RtnCode({0}) RtnMsg([{1}]Robot Control Command EQP Reply Bit is (ON)!)", fail_ReasonCode,MethodBase.GetCurrentMethod().Name);

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00005 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                //20151204 add for Cell Special Use
                if (curRobot.Data.ARMJOBQTY == 2)
                {

                    #region [ 符合Can Issue Command 後要Cleaner curRobot預期下的Cell Special Command的資訊 ]

                    //如果尚未清空則要送清空Cmd的MSG給OPI
                    if (curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_Command != 0 ||
                       curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_Command != 0 ||
                       curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_Command != 0 ||
                       curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_Command != 0)
                    {
                        clearFlag = true;
                    }

                    lock (curRobot)
                    {

                        //Reset Set Command
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_Command = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_ArmSelect = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_TargetPosition = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_TargetSlotNo = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontCSTSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontJobSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontJobKey = string.Empty;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackCSTSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackJobSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackJobKey = string.Empty;

                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_Command = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_ArmSelect = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_TargetPosition = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_TargetSlotNo = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_FrontCSTSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_FrontJobSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_FrontJobKey = string.Empty;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_BackCSTSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_BackJobSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_BackJobKey = string.Empty;

                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_Command = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_ArmSelect = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_TargetPosition = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_TargetSlotNo = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_FrontCSTSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_FrontJobSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_FrontJobKey = string.Empty;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_BackCSTSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_BackJobSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_BackJobKey = string.Empty;

                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_Command = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_ArmSelect = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_TargetPosition = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_TargetSlotNo = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontCSTSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontJobSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontJobKey = string.Empty;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackCSTSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackJobSeq = 0;
                        curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackJobKey = string.Empty;

                    }

                    if (clearFlag == true)
                    {
                        //20151016 add Send To OPI RobotRealTimeCommand to OPI
                        Invoke(eServiceName.UIService, eInvokeOPIFunction.SendToOPI_RealTimeRobotCommandInfo, new object[] { curRobot });
                    }

                    #endregion

                }
                else
                {

                    #region [ 符合Can Issue Command 後要Cleaner curRobot預期下的Command的資訊 ]

                    //20151016 add 如果尚未清空則要送清空Cmd的MSG給OPI
                    if (curRobot.CurRealTimeSetCommandInfo.Cmd01_Command != 0 ||
                       curRobot.CurRealTimeSetCommandInfo.Cmd02_Command != 0)
                    {
                        clearFlag = true;
                    }

                    lock (curRobot)
                    {

                        //Reset Set Command
                        curRobot.CurRealTimeSetCommandInfo.Cmd01_Command = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd01_ArmSelect = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetPosition = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetSlotNo = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd01_CSTSeq = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd01_JobSeq = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd01_JobKey = string.Empty;

                        curRobot.CurRealTimeSetCommandInfo.Cmd02_Command = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd02_ArmSelect = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetSlotNo = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd02_CSTSeq = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd02_JobSeq = 0;
                        curRobot.CurRealTimeSetCommandInfo.Cmd02_JobKey = string.Empty;

                    }

                    if (clearFlag == true)
                    {
                        //20151016 add Send To OPI RobotRealTimeCommand to OPI
                        Invoke(eServiceName.UIService, eInvokeOPIFunction.SendToOPI_RealTimeRobotCommandInfo, new object[] { curRobot });
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

        private void GetEqpReportRobotFetchSeqMode(Robot curRobot)
        {
            string strlog = string.Empty;

            try
            {

                Line curLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (curLine == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get Line Entity by Robot LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //找不到預設為ASC
                    curRobot.EqpRptCSTFetchSeqMode = eRobotCommonConst.DB_ORDER_BY_ASC;

                    return;

                }

                //1  : ASC  2:DESC
                if (curLine.File.RobotFetchSeqMode == "2")
                {

                    curRobot.EqpRptCSTFetchSeqMode = eRobotCommonConst.DB_ORDER_BY_DESC;

                }
                else
                {
                    curRobot.EqpRptCSTFetchSeqMode = eRobotCommonConst.DB_ORDER_BY_ASC;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                //有問題預設為ASC
                curRobot.EqpRptCSTFetchSeqMode = eRobotCommonConst.DB_ORDER_BY_ASC;
            }

        }

        //For Type I Normal Robot Arm GetGet-PutPut Use Function List -=======================================================================================================================================

        /// <summary> for Robot Type I[ One Robot has 2 Arm,Arm#01(Upper),Arm#02(Lower) ,One Arm has One Job Position.Can GetGetPutPut
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotAllStageList"></param>
        private void CheckRobotControlCommand_For_TypeI_ForGetGetPutPut(Robot curRobot, List<RobotStage> curRobotAllStageList)
        {
            bool checkFlag = false;
            string strlog = string.Empty;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
            List<Job> robotArmCanControlJobList_OrderBy = new List<Job>();
            List<Job> robotStageCanControlJobList_OrderBy = new List<Job>();

            try
            {
                #region [ 1. Check Can Issue Command ]
                if (!CheckCanIssueRobotCommand(curRobot)) return;
                #endregion

                #region [ 2. Get Arm Can Control Job List, Stage Can Control Job List and Update StageInfo ][ Wait_Proc_0003 ]

                #region [ Clear All Stage UDRQ And LDRQ Stage SlotNoList Info ]
                foreach (RobotStage stageItem in curRobotAllStageList)
                {
                    lock (stageItem)
                    {
                        stageItem.CassetteStartTime = DateTime.MinValue;
                        stageItem.UnloaderSamplingFlag = RobotStage.UNLOADER_SAMPLING_FLAG.UNKOWN;
                        stageItem.curLDRQ_EmptySlotList.Clear();
                        stageItem.curUDRQ_SlotList.Clear();
                        #region [ Clear All Port Route Info ] Watson 20160104
                        if (stageItem.Data.STAGETYPE == eRobotStageType.PORT)
                        {
                            if (curRobot.CurPortRouteIDInfo.ContainsKey(stageItem.Data.STAGEID))
                                curRobot.CurPortRouteIDInfo.Remove(stageItem.Data.STAGEID);
                            curRobot.CurPortRouteIDInfo.Add(stageItem.Data.STAGEID, eRobotCommonConst.ROBOT_ROUTE_NOUSE_NOCHECK);
                        }
                        #endregion

                        //20160302 add for Array Only
                        stageItem.CurRecipeGroupNoList.Clear();

                        //20160511 將每個RobotStage的可控Job紀錄的RecipeGroup清除
                        stageItem.AllJobRecipeGroupNoList.Clear();

                        //20160618 add for reset port's CurRouteID, assign in select function cc.kuang
                        stageItem.File.CurRouteID = string.Empty;
                    }
                }
                #endregion



                //One Robot Only One Select Rule,如有MIX Route則在Check FetchOut與Filter後 先照Route Priority排序再照STEP排序 以達到優先處理XX Route.如有其他特殊選片邏輯在特別處理
                #region [ Handle Robot Current Rule Job Select Function List ]
                Dictionary<string, List<RobotRuleSelect>> curRuleJobSelectList = ObjectManager.RobotManager.GetRouteSelect(curRobot.Data.ROBOTNAME);
                #region [ Check Select Rule Exist ]
                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00010 ] 
                fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_SELECTRULE_IS_NULL;

                if (curRuleJobSelectList == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any Select Rule!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00010 ]
                    if (!curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any cSelect Rule!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                        failMsg = string.Format("can not get any Select Rule!");

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                        #endregion
                    }
                    #endregion
                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00010 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }
                #endregion


                //20151027 add Set Robot Context
                IRobotContext robotConText = null;
                curRobot.Context = robotConText = new RobotContext();
                //Set 1st Job Command Info
                cur1stJob_CommandInfo cur1StJobCommandInfo = new cur1stJob_CommandInfo();
                curRobot.Context.AddParameter(eRobotContextParameter.Cur1stJob_CommandInfo, cur1StJobCommandInfo);

                //2016/01/26 add for reset eRobotContextParameter when need cc.kuang
                ReSetRobotContextParameter(curRobot); 

                #region [ Initial Select Rule List RobotConText Info. 搭配針對Select Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] ===========
                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurRobotAllStageListEntity, curRobotAllStageList);
                #endregion =========================================================================================================================================================

                //此時Robot無法得知要跑哪種Route,所以只會有一筆[ Wait_For_Proc_00026 ] 之後Table要拿掉RouteID以免誤解的相關處理
                foreach (string routeID in curRuleJobSelectList.Keys)
                {
                    #region [ 根據RuleJobSelect選出Can Control Job List ]
                    foreach (RobotRuleSelect curRuleJobSelect in curRuleJobSelectList[routeID])
                    {
                        //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_E0001 ] ,以Rule Job Select 的ObjectName與MethodName為Key來決定是否紀錄Log
                        fail_ReasonCode = string.Format("{0}_{1}", curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME);

                        #region[DebugLog][ Start Rule Job Select Function ]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) Start {5}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                    curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_START_CHAR, eRobotCommonConst.RULE_SELECT_START_CHAR_LENGTH));

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        if (curRuleJobSelect.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                        {
                            checkFlag = (bool)Invoke(curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME, new object[] { robotConText });

                            if (checkFlag == false)
                            {
                                #region[DebugLog][ End Rule Job Select Function ]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select Fail, object({2}) MethodName({3}) RtnCode({4})  RtnMsg({5}]!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_E0001 ]

                                if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select Fail, object({2}) MethodName({3}) RtnCode({4})  RtnMsg({5}]!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                    //failMsg = string.Format("Robot({0}) object({1}) MethodName({2}) RtnCode({3})  RtnMsg({4}]!",
                                    //                        curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME, robotConText.GetReturnCode(),
                                    //                        robotConText.GetReturnMessage());

                                    failMsg = string.Format("RtnCode({0})  RtnMsg({1})!",robotConText.GetReturnCode(),robotConText.GetReturnMessage());

                                    AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                    #endregion

                                }

                                #endregion

                                //有重大異常直接結束配片邏輯要求人員介入處理
                                //20160114 modify SEMI Mode 還是要可以執行下一個Select 條件.不須結束配片邏輯
                                if (curRobot.File.curRobotRunMode == eRobot_RunMode.AUTO_MODE)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                //Clear[ Robot_Fail_Case_E0001 ]
                                RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                                #region[DebugLog][ End Rule Job Select Function ]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

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

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                        curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion
                        }

                    }

                    #endregion
                    break; //目前只處理第一筆
                }
                #endregion

                #region [ Get Arm Can Control Job List ]

                List<Job> robotArmCanControlJobList;

                robotArmCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.ArmCanControlJobList];

                //當沒有設定參數時會回傳NULL,需防呆
                if (robotArmCanControlJobList == null)
                {
                    robotArmCanControlJobList = new List<Job>();
                }

                #endregion

                #region [ Get Stage Can Control Job List ]

                List<Job> robotStageCanControlJobList;

                robotStageCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.StageCanControlJobList];

                //當沒有設定參數時會回傳NULL,需防呆
                if (robotStageCanControlJobList == null)
                {
                    robotStageCanControlJobList = new List<Job>();
                }

                #endregion

                #endregion

                #region [ 3. Update OPI Stage Display Info ][ Wait_Proc_0005 ]
                bool sendToOPI = false;

                foreach (RobotStage stage_entity in curRobotAllStageList)
                {
                    if (stage_entity.File.StatusChangeFlag)
                    {
                        sendToOPI = true;

                        lock (stage_entity.File)
                        {
                            stage_entity.File.StatusChangeFlag = false;
                        }
                    }
                }

                if (sendToOPI)
                {
                    //通知OPI更新LayOut畫面, //20151126 add by Robot Arm Qty來區分送給OPI的狀態訊息 
                    Invoke(eServiceName.UIService, "RobotStageInfoReport", new object[] { curRobot.Data.LINEID, curRobot });
                }
                #endregion

                #region [ 如果是SEMI Mode只需做到取得目前可控制Job並更新資訊即可 (下面邏輯不需再處理了!) ]
                if (curRobot.File.curRobotRunMode == eRobot_RunMode.SEMI_MODE)
                {
                    #region Array Special for DRY
                    //20160107-001-dd
                    if (Workbench.LineType.ToString().Contains("DRY_"))
                    {
                        curRobot.File.DryLastProcessType = string.Empty; //reset
                        curRobot.File.DryCycleCnt = 0;
                        curRobot.File.DRYLastEnterStageID = 0;
                    }
                    #endregion

                    return;
                }
                #endregion

                #region [  Check Can Control Job Exist ]
                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00006 ] 
                fail_ReasonCode = eRobot_CheckFail_Reason.GET_CAN_CONTROL_JOB_FAIL;

                if (robotArmCanControlJobList.Count == 0 && robotStageCanControlJobList.Count == 0) //都為0 沒有可以處理的基板, 結束這回合!!
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any can control Job!(Please check 1.Robot Arm would have Job 2.CST(Load/Both port = wait for process/In process) is Ready and Exist(JobEachCassetteSlotExists and JobEachCassetteSlotPosition) 3.Upstream EQP would SendOut Job)",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00006 ]
                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any can control Job!(Please check 1.Robot Arm would have Job 2.CST(Load/Both port = wait for process/In process) is Ready and Exist(JobEachCassetteSlotExists and JobEachCassetteSlotPosition) 3.Upstream EQP would SendOut Job)", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("can not get any can control Job!(Please check 1.Robot Arm would have Job 2.CST(Load/Both port = wait for process/In process) is Ready and Exist(JobEachCassetteSlotExists and JobEachCassetteSlotPosition) 3.Upstream EQP wound SendOut Job)");
                        failMsg = string.Format("RtnCode({0}) RtnMsg({1})", fail_ReasonCode, "can not get any can control Job!(Please check 1.Robot Arm would have Job 2.CST(Load/Both port = wait for process/In process) is Ready and Exist(JobEachCassetteSlotExists and JobEachCassetteSlotPosition) 3.Upstream EQP would SendOut Job");
                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion
                    }
                    #endregion

                    #region Array Special for DRY
                    //20160107-001-dd
                    if (Workbench.LineType.ToString().Contains("DRY_"))
                    {
                        curRobot.File.DryLastProcessType = string.Empty; //reset
                        curRobot.File.DryCycleCnt = 0;
                        curRobot.ReCheck = false;
                        curRobot.MixNo = 1;//Add Yang 20160907
                    }
                    #endregion

                    #region CF Sorter Mode, Port上無片可抽, 回到以 Grade OK 優先
                    {
                        if (StaticContext.ContainsKey(eRobotContextParameter.SorterMode_RobotParam))
                        {
                            SorterMode_RobotParam srt_param = (SorterMode_RobotParam)StaticContext[eRobotContextParameter.SorterMode_RobotParam];
                            srt_param.LastGrade = SorterMode_RobotParam.DEFAULT_FIRST_PRIORITY_GRADE;
                        }
                    }
                    #endregion

                    #region [ ELA ONE BY ONE CHECK ]
                    //Add  by qiumin 20171017
                    if (Workbench.LineType == eLineType.ARRAY.ELA_JSW && (curRobot.File.CurELAEQPChangeflag == "Y" || robotStageCanControlJobList.Count == 0))//add by qiumin 20171121  when port have no glass ,scan again
                        //Add  by qiumin 20171017
                        Invoke("RobotSpecialService", "CheckELAOneByOneRun", new object[] { curRobot, robotStageCanControlJobList_OrderBy });
                    #endregion

                    //[Robot CheckErrorList Clear]
                    curRobot.CheckErrorList.Clear();  //add by yang 2017/2/24

                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00006 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }
                #endregion

                curRobot.Context.AddParameter(eRobotContextParameter.IsType2Flag, false); //代表是要走 TypeI 的逻辑!! 20160108-002-dd

                #region [ Handle Robot Arm Job List First ]
                if (robotArmCanControlJobList.Count != 0)
                {
                    #region [ Robot Arm上有片的處理 ]

                    //20151110 add 取得Job所在Stage的Priority
                    UpdateStagePriority(robotArmCanControlJobList);

                    //排序 以Step越小, PortStatus In_Prcess為優先處理 .因都在Robot Arm上所以不需by Job Location StageID排序
                    robotArmCanControlJobList_OrderBy = robotArmCanControlJobList.OrderByDescending(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();

                    foreach (Job curRobotArmJob in robotArmCanControlJobList_OrderBy)
                    {
                        if (CheckRobotArmJobRouteCondition_ForGetGetPutPut(curRobot, curRobotArmJob)) return; //True表示命令已產生則結束本次cycle
                    }

                    #region [ 判斷是否上有1st Job Command尚未下命令 ]
                    cur1stJob_CommandInfo curJudgeCommandInfo = (cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo];

                    if (curJudgeCommandInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE) 
                    {
                        //有1stJob Command 則下命令Send Robot Control Command                       
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curJudgeCommandInfo.cur1stJob_Command });
                        return; //不管有沒有, 都直接return;
                    }

                    #endregion
                    #endregion
                }
                else
                {
                    #region [ Robot Arm上無片的處理 ]

                    //20151110 add 取得Job所在Stage的Priority
                    UpdateStagePriority(robotStageCanControlJobList);

                        //DRY job fetch out by UnitNo(only for DRY),for dry的orderby
                    Line _line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                    if (Workbench.LineType.ToString().Contains("DRY_") && _line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                    {
                        //Yang 20160819
                        Invoke("RobotSpecialService", "CheckDRYMixFetchOutByUnitNo", new object[] { curRobot, robotStageCanControlJobList });
                        robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurStepNo).ThenByDescending(s => s.RobotWIP.dryprocesstypepriority).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ThenBy(s => s.WaitForProcessTime).ToList();
                    }
                    else if (Workbench.LineType.ToString().Contains("FCREP_")) //Added by Zhangwei 20161010 IR 优先级高于RP
                    {
                        robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ThenByDescending(s => s.RobotWIP.RepairPriority).ThenBy(s => s.WaitForProcessTime).ToList();
                    }

                    //Add By Yangzhenteng 20191107 For FLR100 B Type Glass First Fetch Out
                    else if (Workbench.LineType.ToString().Contains("FLR100")) 
                    {
                        robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ThenByDescending(s => s.ArraySpecial.FLRFirstGlassSendOutFlag).ThenBy(s => s.WaitForProcessTime).ToList();
                    }
                    else
                    {
                        //20151110 Add For先依Stage Priority排序越大越優先, 再依Step排序越小越優先, 最後依CurPortCstStatusPriority排序越小越優先
                        //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();
                        //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();
                        //add sort by cst waitforstarttime 2016/03/29 cc.kuang
                        robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ThenBy(s=>s.JobLotPriority).ThenBy(s => s.WaitForProcessTime).ToList();

                    }
                    #region [ CVD Proportional Typ Check (Array Special for CVD) ]
                    //Add  for CVD 20160830
                    if (Workbench.LineType == eLineType.ARRAY.CVD_ULVAC || Workbench.LineType == eLineType.ARRAY.CVD_AKT)
                        //Watson Add CVD 20151001
                        Invoke("RobotSpecialService", "CheckCVDProportionalType", new object[] { curRobot, robotStageCanControlJobList_OrderBy });
                    #endregion

                    #region [ ELA ONE BY ONE CHECK ]
                    //Add  by qiumin 20171017
                    if (Workbench.LineType == eLineType.ARRAY.ELA_JSW && (curRobot.File.CurELAEQPChangeflag == "Y" || robotStageCanControlJobList.Count==0))//add by qiumin 20171121  when port have no glass ,scan again
                        //Add  by qiumin 20171017
                        Invoke("RobotSpecialService", "CheckELAOneByOneRun", new object[] { curRobot, robotStageCanControlJobList_OrderBy });
                    #endregion
                        //Yang Add CVD 20161001
                        //遵循先洗完的Glass先喂给CVD
                        // mark by yang 2017/5/25 做成cfg,方便之后维护CLN+mainEQP 这种布局的line,进行temporal EQP RTC
                    //以后整理的时候可以转移下位置->special service or 做成filter(max priority)
                    #region[special temporal EQP(CLN) RTC]
                    if (ConstantManager.ContainsKey(eNoNeedSendToCLN.NONEEDSENDTOCLN))
                    {
                        if(ConstantManager[eNoNeedSendToCLN.NONEEDSENDTOCLN].Values.ContainsKey(curRobot.Data.LINEID))
                        {
                            if (ConstantManager[eNoNeedSendToCLN.NONEEDSENDTOCLN].Values.Where(s => s.Key.Equals(curRobot.Data.LINEID)).FirstOrDefault().Value.Value.Equals("true"))                 
                            {
                                //if (curRobot.Data.LINEID.Contains("300") || curRobot.Data.LINEID.Contains("400"))
                                //{

                                    //have EQP RTC, check EQP RTC glass can fetch out from cst currently
                                if (robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.EQPRTCFlag == true).Count() > 0)
                                {
                                    curRobot.CLNRTCWIP = true;
                                    Job job1 = robotStageCanControlJobList_OrderBy.FirstOrDefault();
                                    string currentrouteid = job1.RobotWIP.CurRouteID;
                                    RobotRouteCondition currentroute = ObjectManager.RobotManager.GetRouteCondition(curRobot.Data.ROBOTNAME, currentrouteid).FirstOrDefault();
                                    #region[这种dir的写法可做参考]
                                    //  Dictionary<string,List<RobotRouteCondition>> routeconditions = ObjectManager.RobotManager.GetRouteConditionsByRobotName(curRobot.Data.ROBOTNAME);

                                    //  for(int i=0;i<=routeconditions.Count();i++)
                                    //{
                                    //    List<RobotRouteCondition> checkroute = routeconditions.Values.Where(s => string.IsNullOrEmpty(s[i].Data.REMARKS)).FirstOrDefault();
                                    //    if (checkroute.Count() > 0) ;
                                    //}
                                    #endregion

                                    string _limitcount = string.Empty;
                                    string _fetchcount = string.Empty;
                                    int limitcount;
                                    int fetchcount;
                                    if (currentroute.Data.REMARKS.Contains(',')) //first value: limit RTC Count , second value:limit RTC Count which glass can fetch out from cst
                                    {
                                        _limitcount = currentroute.Data.REMARKS.Trim().Split(',')[0];
                                        _fetchcount = currentroute.Data.REMARKS.Trim().Split(',')[1];
                                    }
                                    else _limitcount = currentroute.Data.REMARKS.Trim();

                                    if (int.TryParse(_limitcount, out limitcount))  //for stop fetch out
                                    {
                                        if (robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.EQPRTCFlag == true).Count() >= limitcount)
                                            curRobot.noSendToCLN = true;
                                        else
                                            curRobot.noSendToCLN = false;
                                    }
                                    else curRobot.noSendToCLN = false;
                                    if (int.TryParse(_fetchcount, out fetchcount))   //for continue fetch out 
                                    {
                                        if (robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.EQPRTCFlag == true).Count() <= fetchcount)
                                            curRobot.fetchforRTC = true;
                                        else
                                            curRobot.fetchforRTC = false;
                                    }
                                    else curRobot.fetchforRTC = true;
                                }
                                else
                                {
                                    curRobot.CLNRTCWIP = false;
                                    curRobot.fetchforRTC = true;  //add
                                    curRobot.noSendToCLN = false;
                                }
                                }
                            //}
                        }
                    }
                    #endregion
                    List<Job> _tempRobotStageCanControlJobList = new List<Job>();
                    _tempRobotStageCanControlJobList.Clear();
                    foreach (Job curRobotStageJob in robotStageCanControlJobList_OrderBy)
                    {
                        //if (curRobotStageJob.RobotWIP.RTCReworkFlag && !_tempRobotStageCanControlJobList.Contains(curRobotStageJob)) //有做过RTC的基板, 先不处理, 优先处理正常为出片的基板!!
                        //{
                        //    _tempRobotStageCanControlJobList.Add(curRobotStageJob);
                        //    continue;
                        //}
                        if (CheckRobotStageJobRouteCondition_ForGetGetPutPut(curRobot, curRobotStageJob)) return; //True表示命令已產生則結束本次cycle

                        //Cassette cassett = ObjectManager.CassetteManager.GetCassette(curRobotStageJob.FromCstID);

                        //只要有CST 处于Waiting fot MES Reply FirstGlassCheck 状态，就不出片 Modified by Zhangwei 20161104
                        //if (cassett.FirstGlassCheckReport == "C2") return;

                    }
                    //if (_tempRobotStageCanControlJobList.Count > 0)
                    //{
                    //    _tempRobotStageCanControlJobList = _tempRobotStageCanControlJobList.OrderBy(s => s.RobotWIP.PreFetchFlag).ToList();
                    //    foreach (Job curRobotStageJob in _tempRobotStageCanControlJobList)
                    //    {
                    //        if (CheckRobotStageJobRouteCondition_ForGetGetPutPut(curRobot, curRobotStageJob)) return; //True表示命令已產生則結束本次cycle
                    //    }
                    //}

                    #region [ 判斷是否上有1st Job Command尚未下命令 ]
                    cur1stJob_CommandInfo curJudgeCommandInfo = (cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo];

                    if (curJudgeCommandInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    {
                        //有1stJob Command 則下命令Send Robot Control Command                       
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curJudgeCommandInfo.cur1stJob_Command });
                        return; //不管有沒有, 都直接return;
                    }
                    #endregion
                    #region Prefetch
                    #region DRY Prefetch

                    //20160104, by dade, 新增逻辑, 针对DRY line的MIX mode, 不管有没有启动预取功能, 预设都是不作动!!
                    //20160819,add by Yang,DRY修改为by unit顺序出片，MIX mode下可以预取
                    /* switch (Workbench.LineType.ToUpper())
                     {
                         case eLineType.ARRAY.DRY_ICD:
                         case eLineType.ARRAY.DRY_YAC:
                             Line _line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                             if (_line == null) _doPrefetchFlag = false; //not-found, skip Prefetch
                             if (_line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE) _doPrefetchFlag = false; //MIX, skip Prefetch
                             //if (_line.File.LineOperMode.ToUpper() == "MIX" || _line.File.LineOperMode.ToUpper() == "MIXEDRUNMODE") _doPrefetchFlag = false; //MIX, skip Prefetch
                             break;
                         default: break;
                     }
                     */
                    #endregion

                    bool _doPrefetchFlag = true;
                    if (_doPrefetchFlag && (bool)Invoke("RobotSpecialService", "Check_PreFetch_DelayTime_For1Arm1Job", new object[] { robotConText })) //第一次或是超过delay time就可以考虑预取!!
                    {
                        //上述都没有可以跑的基板, 正常的部分check完成后, 接下来就要去判断...有没有要预取的基板 (前提条件要开启 预取 功能!)
                        //虽然有开启 预取 功能, 但是仍然需要去判断其他的项目是不是有启动!! 如果有启动, 则视同 预取 没作动!! 2015-12-26
                        bool _runPrefetchFlag = false;

                        //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.CurLocation_StageType == eStageType.PORT).ToList();
                        //20160624 加入EQP預取,CurPortCstStatusPriority要判斷,愈小的排前面,Inprocess > Waitforprocess
                        //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.CurLocation_StageType == eStageType.PORT || s.RobotWIP.CurLocation_StageType == eStageType.EQUIPMENT).OrderBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();
                        //增加插队卡夹的 Prefetch 优先级Priority高于Inprocessing Modified by Zhangwei 20161022
                        robotStageCanControlJobList_OrderBy = robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.CurLocation_StageType == eStageType.PORT || s.RobotWIP.CurLocation_StageType == eStageType.EQUIPMENT).OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ThenBy(s => s.WaitForProcessTime).ToList();
                        if (robotStageCanControlJobList_OrderBy.Count() > 0)
                        {
                            _tempRobotStageCanControlJobList.Clear();

                            RobotStage _curStage = null;
                            foreach (Job curRobotStageJob in robotStageCanControlJobList_OrderBy)
                            {
                                _curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotStageJob.RobotWIP.CurLocation_StageID);
                                if (!CheckPrefetchFlag(curRobot, _curStage)) continue; //如果為false, 就是沒有開啟Pre-Fetch功能!

                                _runPrefetchFlag = (bool)Invoke("RobotSpecialService", "Check_Stage_Prefetch_SpecialCondition_For1Arm1Job", new object[] { robotConText, _curStage, curRobotStageJob }); //判断是不是要跑 预取 功能!!
                                if (!_runPrefetchFlag) continue; //如果为true, 才是真的要做 预取 功能!!

                                //if (curRobotStageJob.RobotWIP.RTCReworkFlag && !_tempRobotStageCanControlJobList.Contains(curRobotStageJob)) //有做过RTC的基板, 先不处理, 优先处理正常为出片的基板!!
                                //{
                                //    _tempRobotStageCanControlJobList.Add(curRobotStageJob);
                                //    continue;
                                //}
                                if (CheckRobotStageJobRouteCondition_ForGetGetPutPut(curRobot, curRobotStageJob)) return; //True表示命令已產生則結束本次cycle

                                //Cassette cassett = ObjectManager.CassetteManager.GetCassette(curRobotStageJob.FromCstID);

                                //只要有CST 处于Waiting fot MES Reply FirstGlassCheck 状态，就不出片 Modified by Zhangwei 20161104
                                //if (cassett.FirstGlassCheckReport == "C2") return;

                            }
                            //if (_tempRobotStageCanControlJobList.Count > 0)
                            //{
                            //    _tempRobotStageCanControlJobList = _tempRobotStageCanControlJobList.OrderBy(s => s.RobotWIP.PreFetchFlag).ToList();
                            //    foreach (Job curRobotStageJob in _tempRobotStageCanControlJobList)
                            //    {
                            //        if (CheckRobotStageJobRouteCondition_ForGetGetPutPut(curRobot, curRobotStageJob)) return; //True表示命令已產生則結束本次cycle
                            //    }
                            //}
                        }
                    }
                    #endregion
                    //20160812 正常一個stage一個slot,沒slot,馬上下Command;但是可能有多個slot可去,例如CST slot,不會馬上下Command
                    //接著判斷第二片的條件,產生Command,做Judge,才下Command,這是正常情況
                    //但是有可能只有一片Job,Target有多個slot,結果Command就沒機會下;或是第一片Job判斷OK,第二片開始都判斷Fail,例如設定使用Low Arm
                    //結果是即使第一片Command OK,但是第二片開始都Fail,掃出Foreach後,Command沒機會下,所以在這補上如果有COmmand存在就send Command

                    #region [ 判斷是否上有1st Job Command尚未下命令 ]
                    curJudgeCommandInfo = (cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo];

                    if (curJudgeCommandInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    {
                        //有1stJob Command 則下命令Send Robot Control Command                       
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curJudgeCommandInfo.cur1stJob_Command });
                        return; //不管有沒有, 都直接return;
                    }
                    #endregion

                    #region Array Special for DRY
                    //20160107-001-dd
                    if (Workbench.LineType.ToString().Contains("DRY_")) if (curRobot.File.DryLastProcessType != string.Empty) curRobot.File.DryCycleCnt++;
                    #endregion

                    #region Sorter Mode
                    {
                        //程式碼跑到這裡表示沒有下RobotCommand
                        Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                        if (line.Data.FABTYPE == eFabType.CF.ToString() &&
                            line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE &&
                            StaticContext.ContainsKey(eRobotContextParameter.SorterMode_RobotParam))
                        {
                            SorterMode_RobotParam srt_param = (SorterMode_RobotParam)StaticContext[eRobotContextParameter.SorterMode_RobotParam];
                            if (robotStageCanControlJobList.Count > 0)
                            {
                                // Sorter Mode 下有找到 StageCanControlJob, 但程式碼卻跑到這裡, 表示有 StageJob 沒出片
                                // 檢查StageJob, Job Grade是否與Unloader Mapping Grade相同
                                // 如果全部Job Grade都沒有與Unloader Mapping Grade相同, 就必須呼叫CassetteService
                                #region NeedToCallCassetteService
                                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
                                List<Port> ports = ObjectManager.PortManager.GetPortsByLine(curRobot.Data.LINEID);
                                bool unloader_ready = false;//有至少一個InProcess或WaitForProcess的Unloader
                                List<PortStage> mapping_port_stages = null;//與當前JobGrade符合的Port
                                Job stage_job = null;//有找到mapping port的job
                                foreach (Job job in robotStageCanControlJobList)
                                {
                                    if (!job.RobotWIP.SorterMode_OtherFilterOK)
                                        continue;// 不考慮因為 Grade 以外的 Filter 而被過濾掉的 Job, 表示不能出片的 Job 不考慮
                                    // 能出片則繼續判斷是否要退Cassette
                                    mapping_port_stages = SorterMode_JobGradeUnloaderGrade(eqp, ports, curRobotAllStageList, job, ref unloader_ready);
                                    if (mapping_port_stages.Count <= 0)
                                    {
                                        //找不到 mapping port
                                        if (unloader_ready)
                                        {
                                            //當 Unloader 有 Cassette 且 InProcess 或 WaitForProcess 時, 但仍然找不到 mapping port, 就需要呼叫 Cassette Serivce 做退 Cassette
                                            srt_param.NeedToCallCassetteService = SorterMode_RobotParam.NEED_TO_CALL_CST_SERVICE.NEED_TO_CALL;
                                            stage_job = job;
                                        }
                                    }
                                    else
                                    {
                                        srt_param.NeedToCallCassetteService = SorterMode_RobotParam.NEED_TO_CALL_CST_SERVICE.ONE_JOB_MATCH;//有找到Job Grade相同的Unloader, 不需要呼叫CassetteService
                                        stage_job = job;
                                        break;
                                    }
                                }
                                #endregion
                                if (srt_param.EnableCallCassetteService && srt_param.NeedToCallCassetteService == SorterMode_RobotParam.NEED_TO_CALL_CST_SERVICE.NEED_TO_CALL)
                                {
                                    srt_param.EnableCallCassetteService = false;//直到ProcResult_JobMoveToRobotArm_1Arm1Job_forFCSRT, 才會再變為true
                                    if (ParameterManager.Parameters.ContainsKey("ROBOT_ENABLE_CALL_CASSETTE_SERVICE"))
                                    {
                                        ParameterManager.Parameters["ROBOT_ENABLE_CALL_CASSETTE_SERVICE"].Value = srt_param.EnableCallCassetteService.ToString();
                                    }
                                    string method_name = "CassetteStoreQTimeProcessEnd";
                                    strlog = string.Format("Invoke CassetteService.{0}()", method_name);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    Invoke(eServiceName.CassetteService, method_name, new object[] { });
                                }
                                else
                                {
                                    #region Debug Log
                                    if (IsShowDetialLog == true)
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        sb.AppendFormat("Do Not Invoke CassetteStoreQTimeProcessEnd. EnableCallCassetteService({0}) NeedToCallCassetteService({1}) UnloaderReady({2})", srt_param.EnableCallCassetteService, srt_param.NeedToCallCassetteService, unloader_ready);
                                        if (stage_job != null) sb.AppendFormat("Source Job({0}, {1})", stage_job.JobKey, stage_job.JobGrade);
                                        if (mapping_port_stages != null)
                                        {
                                            sb.AppendFormat("Mapping Ports(");
                                            foreach(PortStage port_stage in mapping_port_stages)
                                                sb.AppendFormat("PortNo({0}) PortMode({1}) PortGrade({2}) EmptySlot({3}),", port_stage.Port.Data.PORTNO, port_stage.Port.File.Mode.ToString(), port_stage.Port.File.MappingGrade, port_stage.Stage.curLDRQ_EmptySlotList.Count);
                                            if (mapping_port_stages.Count > 0) sb.Remove(sb.Length - 1, 1);
                                            sb.AppendFormat(")");
                                        }
                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", sb.ToString());
                                    }
                                    #endregion
                                }
                            }
                            else
                            {
                                #region Debug Log
                                if (IsShowDetialLog == true)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendFormat("Do Not Invoke CassetteStoreQTimeProcessEnd. robotStageCanControlJobList.Count is 0");
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", sb.ToString());
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion

                    #endregion
                }

                //add by yang 2017/2/23
                if (curRobot.CheckErrorList.Where(s => s.Value.Item3.Equals("0")).Count() != 0)
                    Invoke(eServiceName.EvisorService, "AppErrorSet", new object[] { curRobot.Data.LINEID, curRobot.CheckErrorList });
               // Invoke(eServiceName.EvisorService, "AppErrorSet", new object[] { curRobot.Data.LINEID, curRobot.CheckErrorList.Where(s => s.Value.Item3.Equals("0")) });
                //Invoke里接了where会报错,先mark,待确认

                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            finally
            {
                curRobot.Context = null;
            }
        }

        /// <summary> Check Arm上Job 目前Step的所有Filter條件是否成立(最多2片同時成立)
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotArmJob"></param>
        /// <returns></returns>
        private bool CheckRobotArmJobRouteCondition_ForGetGetPutPut(Robot curRobot, Job curRobotArmJob)
        {
            string strlog = string.Empty;
            List<RobotStage> curFilterStageList = new List<RobotStage>();
            RobotCmdInfo curRobotCommand = new RobotCmdInfo();
            RobotRouteStep cur2ndJob_curRouteStep = null;
            //20160812
            StaticContext.AddParameter(eRobotContextParameter.CurJobEntity, curRobotArmJob);

            try
            {
                #region [ 20151015 add Check CurStep RouteStepByPass Condition and 準備變更curStep ]

                if (!CheckAllRouteStepByPassCondition2(curRobot, curRobotArmJob, curRobotArmJob.RobotWIP.CurStepNo, ref curFilterStageList))
                {
                    //StepByPass條件有問題則回覆NG
                    return false;
                }

                #endregion

                #region [ 20151017 add Check CurStep All RouteStepJump Condition and 準備變更curStep ]

                if (!CheckAllRouteStepJumpCondition2(curRobot, curRobotArmJob, curRobotArmJob.RobotWIP.CurStepNo, ref curFilterStageList))
                {
                    //StageSelect條件有問題則回覆NG
                    return false;
                }

                #endregion

                #region [ 20151028 add Check 2nd Job Command 1st Action & TargetPosition Rule by 1stJob Command Info ]

                cur1stJob_CommandInfo cur1stJobCmdInfo = (cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo];

                if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                {

                    #region [ 1st Job 1st Command Action 必須是PUT to Stage. 如果是1st Job 1st Command不是Put or Multi-Put則不需考慮2nd Job Command ]

                    //20160511 增加RECIPEGROUPEND_PUT,如果是Cmd01是RECIPEGROUPEND_PUT還要繼續判斷
                    if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_PUT &&
                        cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_MULTI_PUT &&
                        cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT)
                    {

                        //直接下命令並回true不需考慮2nd Job
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                        return true;

                    }

                    #endregion

                    //因1st Job and 2nd Job都在Arm上不需要確認Job Location是否相同.且不需要更新ArmInfo                  

                    #region [ Update 1stJob 1st Command Target StageID Empty Slotlist Info by 1stJob 1st Command Target SlotNo ]

                    if (cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotList.ContainsKey(cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo) == true)
                    {
                        //根據1st Job  1st Command Target Position/SlotNo 將Target Position的EmptySlotNo預約起來(排除引用)
                        cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotList.Remove(cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo);
                    }

                    if (cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotList.Count == 0)
                    {
                        //20151110 add 如果1st Job curStep設定可以CrossStage(1stJob的Target點)那還是要判斷2ndJob可以處理
                        if (cur1stJobCmdInfo.cur1stJob_1stCommand_CrossStageFlag != "Y")
                        {

                            //1st Job Command 之後就沒有空的Slot則不需要2ndJob Command
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job 1st Command TargetStageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            //直接下命令並回true不需考慮2nd Job
                            bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                            return true;

                        }
                    }

                    #endregion

                    #region [ Get Current 2ndJob curStep Entity ]

                    cur2ndJob_curRouteStep = curRobotArmJob.RobotWIP.RobotRouteStepList[curRobotArmJob.RobotWIP.CurStepNo];

                    //找不到 CurStep Route 回NG
                    if (cur2ndJob_curRouteStep == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                                                    curRobotArmJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    #endregion

                    //因1st Job and 2nd Job都在Arm上 所以不需要確認Source Stage是否相同

                    #region [ Check 2ndJob_1stCommand Action by 1stJob_1stCommand ActionCode ]

                    //Mulit-Put 在DB無法預設所以要有Temp轉換
                    string tmpAction = string.Empty;

                    if (cur1stJobCmdInfo.cur1stJob_1stCommand_DBActionCode == eRobot_DB_CommandAction.ACTION_MULTI_GET)
                    {
                        tmpAction = eRobot_DB_CommandAction.ACTION_GET;
                    }
                    else if (cur1stJobCmdInfo.cur1stJob_1stCommand_DBActionCode == eRobot_DB_CommandAction.ACTION_MULTI_PUT)
                    {
                        tmpAction = eRobot_DB_CommandAction.ACTION_PUT;
                    }
                    else
                    {
                        tmpAction = cur1stJobCmdInfo.cur1stJob_1stCommand_DBActionCode;
                    }

                    if (cur2ndJob_curRouteStep.Data.ROBOTACTION != tmpAction)
                    {
                        //2nd Job目前Step Action不和1stJob 1st Command Action 不相同 則不需要再考慮這一片
                        //因為是當下決定 所以直接記Log 可考慮不需要Debug
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) Action({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) curStepNo({7}) Action({8}) is different!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq.ToString(), cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq.ToString(),
                                                cur1stJobCmdInfo.cur1stJob_1stCommand_DBActionCode, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo, curRobotArmJob.RobotWIP.CurStepNo.ToString(),
                                                cur2ndJob_curRouteStep.Data.ROBOTACTION);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        return false;

                    }

                    #endregion

                }

                #endregion

                #region [ Check CurStep All Filter Condition ]

                DefineNormalRobotCmd cur1stDefindCmd = new DefineNormalRobotCmd();
                DefineNormalRobotCmd cur2ndDefindCmd = new DefineNormalRobotCmd();

                //20150825 work End 要考慮LDRQ Stage Type如果是Port 要特別處理!![ Wait_Proc_00029 ] Check Next Stage 要多考慮Port Type 處理
                //Arm Job Only Check curStep Filter
                if (!CheckAllFilterConditionByStepNo2(curRobot, curRobotArmJob, curRobotArmJob.RobotWIP.CurStepNo, cur1stDefindCmd, cur2ndDefindCmd, ref curFilterStageList))
                {
                    //Filter條件有問題則回覆NG
                    //20160127 add Set Wait CST Event
                    Cell_SendWaitCassetteStatusToBCS(MethodBase.GetCurrentMethod().Name, curRobot, curRobotArmJob, false);

                            bool _openJobOnArmMove = false;
                            if (ParameterManager.Parameters.ContainsKey("ROBOT_JOBONARM_MOVE"))
                               bool.TryParse(ParameterManager.Parameters["ROBOT_JOBONARM_MOVE"].Value.ToString(), out _openJobOnArmMove);
                            //下面这段意思是robot 手臂上的job 当Filter check 失败，skip Filter 然后下putReady ，但是在带片跑的情况下，不能让手臂上的job 下putReady ，要走到带片跑
                            if (!_openJobOnArmMove)
                            {
                                //20160624
                                bool _skipFilterCheckPutReady = false;
                                string[] _stages = curRobotArmJob.RobotWIP.RobotRouteStepList[curRobotArmJob.RobotWIP.CurStepNo].Data.STAGEIDLIST.Split(',');
                                if (_stages == null || _stages.Length == 0)
                                {
                                    _skipFilterCheckPutReady = false;
                                }
                                foreach (string _stage in _stages)
                                {
                                    RobotStage stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(_stage);
                                    if (stage == null)
                                    {
                                        _skipFilterCheckPutReady = false;
                                    }
                                    if (stage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y")
                                    {
                                        _skipFilterCheckPutReady = true;
                                        break;
                                    }
                                    else
                                        _skipFilterCheckPutReady = false;
                                }
                                //20160727 
                                curRobotArmJob.RobotWIP.SkipFilterCheck = _skipFilterCheckPutReady;

                                if (!_skipFilterCheckPutReady)
                                    return false;
                            }
                            return false;
                }

                //20160127 add 當通過Filter時通知BCS Clear Wait CST Event
                Cell_SendWaitCassetteStatusToBCS(MethodBase.GetCurrentMethod().Name, curRobot, curRobotArmJob, true);

                #endregion

                #region [ #region [ 20151028 add Check 2nd Job Command 1st Action & TargetPosition Rule(curStepNo)  ]

                if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                {
                    //找不到 CurStep Route 回NG
                    if (cur2ndJob_curRouteStep == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                                                    curRobotArmJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    //預設不允許同Step跨Stage放片 20151029 add
                    bool putJobtoAnotherStageFlag = false;

                    //20151110 add 比對1st Job 與2nd Job 是否可以Cross. Arm上Job為1st Cmd
                    if (cur1stJobCmdInfo.cur1stJob_1stCommand_CrossStageFlag == "Y" &&
                        cur2ndJob_curRouteStep.Data.CROSSSTAGEFLAG == "Y")
                    {
                        putJobtoAnotherStageFlag = true;
                    }

                    #region [ Check 2ndJob_1stCommand Can Use StageList by 1stJob_1stCommand TargetStageEntity ]

                    //如果2nd Job 1st Command 是PUT相關則表示 curFilterStageList是給2nd Job 1st Cmd用=>在此使用
                    //如果2nd Job 2nd Command 不是PUT相關則表示 curFilterStageList是給2nd Job 2nd Cmd用=>在Stage上有Job Function使用
                    if (curFilterStageList.Contains(cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity) == false)
                    {
                        //2ndJob的Target目標不包含1stJob的Target點時的處理

                        #region [ 判斷是否允許同Step跨Stage放片,允許的話則不更新curFilterStageList ]

                        if (putJobtoAnotherStageFlag == false)
                        {
                            //2nd Job 1st Command目前Use StageList不和1stJob 1st Command TargetPosition不相同 則不需要再考慮這一片
                            //因為是當下決定 所以直接記Log 可考慮不需要Debug
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) 2nd TargetPodition({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) curStepNo({7}) StageList({8}) is different!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq.ToString(), cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq.ToString(),
                                                    cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo, curRobotArmJob.RobotWIP.CurStepNo.ToString(),
                                                    cur2ndJob_curRouteStep.Data.STAGEIDLIST);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return false;
                        }

                        #endregion

                        //如果允許同Step跨Stage放片則維持原先的curFilterStageList

                    }
                    else
                    {
                        //2ndJob的Target目標包含1stJob的Target點時的處理

                        //20151110 add當2ndJob 1st Cmd可去Stage包含1stJob的1st Cmd Target Stage 且1stJob 1st Cmd Target Stage不能再放片時,2nd Job的Target點要排除掉沒有空Slot的Stage
                        if (cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotList.Count == 0)
                        {
                            curFilterStageList.Remove(cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity);
                            
                            //2ndJob'Target Stage same as 1stJob's,2ndJob Cmd Can't Send   --Yang tips
                            if (curFilterStageList.Count == 0)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) 2nd TargetPodition({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) nextStepNo({7}) StageList({8}) can not Receive!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq.ToString(), cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq.ToString(),
                                                    cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo, curRobotArmJob.RobotWIP.NextStepNo.ToString(),
                                                    cur2ndJob_curRouteStep.Data.STAGEIDLIST);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                return false;
                            }
                        }
                        else
                        {

                            //強制2nd Job只能進同Stage
                            curFilterStageList.Clear();
                            curFilterStageList.Add(cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity);

                        }

                    }

                    #endregion

                }

                #endregion

                #region [ Check All OrderBy Condition and define Target Position and SlotNo ]

                
                //20160624
                //Check 1st Cmd is Exist
                //if (cur1stDefindCmd.Cmd01_Command == 0)
                //{
                //    //沒有1st Command 則記Error 離開
                //    #region[DebugLog]

                //    if (IsShowDetialLog == true)
                //    {
                //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) Command Action({9}) is illegal",
                //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                //                                curRobotArmJob.RobotWIP.CurLocation_StageID, curRobotArmJob.RobotWIP.CurStepNo.ToString(),
                //                                curRobotArmJob.RobotWIP.RobotRouteStepList[curRobotArmJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                //                                curRobotArmJob.RobotWIP.RobotRouteStepList[curRobotArmJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                //                                cur1stDefindCmd.Cmd01_Command.ToString());

                //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //    }

                //    #endregion

                //    return false;
                //}

                //Order 1st & 2nd Cmd
                if (CheckAllOrderByConditionByCommand(curRobot, curRobotArmJob, cur1stDefindCmd, cur2ndDefindCmd, curFilterStageList) == false)
                {
                    //20151026 add Order By後Cmd 的TargetPosition or SlotNo有問題則回覆NG
                    return false;
                }

                #endregion

                #region [ by RobotArm Qty Create Command ]

                if (curRobot.Data.ARMJOBQTY == 1)
                {
                    #region [ 20151022 add Check First Glass Command ]

                    //Check 1st Command
                    if (PortFetchOut_FirstGlassCheck(curRobot, curRobotArmJob, cur1stDefindCmd, false) == false)
                    {

                        return false;
                    }

                    //Check 2nd Command
                    if (PortFetchOut_FirstGlassCheck(curRobot, curRobotArmJob, cur2ndDefindCmd, true) == false)
                    {

                        return false;
                    }

                    #endregion


                    
                    #region [ 20151230 add Check RTCPUT Condition ]
                    //GlobalAssemblyVersion v1.0.0.9-20151230
                    CheckRtcPutCommandCondition(curRobot, curRobotArmJob, cur1stDefindCmd); //Check 1st Command
                    CheckRtcPutCommandCondition(curRobot, curRobotArmJob, cur2ndDefindCmd, true); //Check 2nd Command
                    #endregion

                    //20160624
                    #region [ 20160624 add Check PUTREADY Condition ]
                    CheckOnArmPutReadyCommandCondition(curRobot, curRobotArmJob, cur1stDefindCmd); //Check 1st Command
                    CheckOnArmPutReadyCommandCondition(curRobot, curRobotArmJob, cur2ndDefindCmd, true); //Check 2nd Command
                    #endregion

                    #region [ 20151025 add Check Multi-Single Condition ] 20151102 Mark . 只有對Mulit Type EQP 存取2片時才會下Multi命令

                    ////Check 1st Command
                    //CheckMultiSingleCommandCondition(curRobot, curRobotArmJob, cur1stDefindCmd, false);
                    ////Check 2nd Command
                    //CheckMultiSingleCommandCondition(curRobot, curRobotArmJob, cur2ndDefindCmd, true);

                    #endregion

                    #region [ Create 1 Arm 1 Substrate ]

                    int int1stCstSeqNo = 0;
                    int int1stJobSeqNo = 0;
                    int int2ndCstSeqNo = 0;
                    int int2ndJobSeqNo = 0;

                    curRobotCommand.Cmd01_Command = cur1stDefindCmd.Cmd01_Command;
                    curRobotCommand.Cmd01_ArmSelect = cur1stDefindCmd.Cmd01_ArmSelect;
                    curRobotCommand.Cmd01_TargetPosition = cur1stDefindCmd.Cmd01_TargetPosition;
                    curRobotCommand.Cmd01_TargetSlotNo = cur1stDefindCmd.Cmd01_TargetSlotNo;
                    int.TryParse(cur1stDefindCmd.Cmd01_CstSeq, out int1stCstSeqNo);
                    curRobotCommand.Cmd01_CSTSeq = int1stCstSeqNo;
                    int.TryParse(cur1stDefindCmd.Cmd01_JobSeq, out int1stJobSeqNo);
                    curRobotCommand.Cmd01_JobSeq = int1stJobSeqNo;


                    curRobotCommand.Cmd02_Command = cur2ndDefindCmd.Cmd01_Command;
                    curRobotCommand.Cmd02_ArmSelect = cur2ndDefindCmd.Cmd01_ArmSelect;
                    curRobotCommand.Cmd02_TargetPosition = cur2ndDefindCmd.Cmd01_TargetPosition;
                    curRobotCommand.Cmd02_TargetSlotNo = cur2ndDefindCmd.Cmd01_TargetSlotNo;
                    int.TryParse(cur2ndDefindCmd.Cmd01_CstSeq, out int2ndCstSeqNo);
                    curRobotCommand.Cmd02_CSTSeq = int2ndCstSeqNo;
                    int.TryParse(cur2ndDefindCmd.Cmd01_JobSeq, out int2ndJobSeqNo);
                    curRobotCommand.Cmd02_JobSeq = int2ndJobSeqNo;

                    #endregion

                    #region [ #region [ 20151027 set cur 1st Job CommandInfo ]

                    if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_NONE)
                    {

                        #region [ Set 1stJob Command ]

                        cur1stJobCmdInfo.cur1stJob_Command = curRobotCommand;
                        cur1stJobCmdInfo.cur1stJob_1stCommand_DBActionCode = GetRobotCommandActionDesc(curRobotCommand.Cmd01_Command);
                        cur1stJobCmdInfo.cur1stJob_2ndCommand_DBActionCode = GetRobotCommandActionDesc(curRobotCommand.Cmd02_Command);

                        if (curRobotCommand.Cmd01_TargetPosition != 0)
                        {
                            RobotStage cur1stCmdStageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotCommand.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));
                            cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity = cur1stCmdStageEntity;
                        }

                        if (curRobotCommand.Cmd02_TargetPosition != 0)
                        {
                            RobotStage cur2ndCmdStageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotCommand.Cmd02_TargetPosition.ToString().PadLeft(2, '0'));
                            cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity = cur2ndCmdStageEntity;
                        }

                        #region [ 1st Job 1st Command Action 必須要是Put or Multi-Put且因為在Arm上不需要去預約ArmInfo. 不是Put or Multi-Put則不需考慮2nd Job Command ]

                        //1st Job Command 1st Action必須是Put to Stage.且Use Arm不需考慮如果是1st Job 1st Command 不是Put or Multi-Put則不需考慮2nd Job Command
                        if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_PUT &&
                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_MULTI_PUT)
                        {
                                //直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });  
                            //20160803 避免回傳true,跑不進帶片跑CheckRobotStageJobRouteCondition_ForGetGetPutPut,cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command==ACTION_NONE
                                if (!sendCmdResult)
                                    return false;
                                else
                                    return true;

                        }

                        #endregion

                        //Arm上Job只須更新 1stJob curStep CrossStageFlag
                        #region [ 20151110 add for Get 1stJob CurStep Entity and UpDate 1stJob 1stCmd CrossStageFlag ]

                        //Arm Job Only Check CurStepInfo
                        RobotRouteStep cur1stJob_CurRouteStepInfo = null;

                        if (curRobotArmJob.RobotWIP.RobotRouteStepList.ContainsKey(curRobotArmJob.RobotWIP.CurStepNo) == true)
                        {
                            cur1stJob_CurRouteStepInfo = curRobotArmJob.RobotWIP.RobotRouteStepList[curRobotArmJob.RobotWIP.CurStepNo];
                        }

                        //找不到 CurStep Route 回NG
                        if (cur1stJob_CurRouteStepInfo == null)
                        {

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                                                        curRobotArmJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        cur1stJobCmdInfo.cur1stJob_1stCommand_CrossStageFlag = cur1stJob_CurRouteStepInfo.Data.CROSSSTAGEFLAG;

                        #endregion

                        #region [ Update 1stJob 1st Command Target StageID Empty Slotlist Info by Target SlotNo ]

                        if (cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotList.ContainsKey(cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo) == true)
                        {
                            //根據1st Job 1st Command Target Position/SlotNo 將Target Position的EmptySlotNo預約起來(排除引用)
                            cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotList.Remove(cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo);
                        }

                        if (cur1stJobCmdInfo.cur1stJob_1stCommand_DBActionCode == eRobot_DB_CommandAction.ACTION_PUT &&//Robot只做 PUT
                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_ControlCommand.PUT &&//Robot只做 PUT
                            cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity == null &&//Robot只做 PUT
                            cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity != null &&
                            cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID == cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetPosition.ToString() &&//Robot Put的TargetStage
                            cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curUDRQ_SlotList.ContainsKey(cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo) &&//Robot Put的TargetSlot正要出片
                            cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.File.DownStreamExchangeReqFlag)//Robot PUT的TargetStage有Exchange
                        {
                            #region 撞片注意!!
                            // FCPSH_TYPE1的JobData ForcePSH bit ON時, Glass Flow為:
                            // Port->Stage11->Stage12->Port 或者 Port->Stage12->Stage11->Port
                            // 也就是Glass要走遍全部Stage,但不論走訪Stage的次序
                            //--------------------------------------------------
                            // 測試FCPSH_TYPE1時發現問題, 當Stage11,12先後LDRQ, Robot會做GET&PUT放滿Stage11,12
                            // 這時因為Stage11,12都有片, 所以Stage11,12發ExchangePossible
                            // Robot會做GET from Port & Exchange Stage11, Exchange之後竟然將Arm上Glass直接PUT Stage12導致撞片
                            //--------------------------------------------------
                            // 因此判斷, 1. 當first command是PUT且沒有second command
                            //          2. 當first command put的target slot是TargetStage的UDRQ Slot
                            //          3. TargetStage有Exchange
                            // 以上條件成立則將first command put改成exchange
                            #endregion

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}), Prevent Glass Collision, Convert Put to Exchange, RobotArm({3}) ArmJob({4}_{5}), TargetStage({6}), StageJob({7})",
                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME,
                                cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect, cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq, cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq,
                                cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetPosition, cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curUDRQ_SlotList[cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo]);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_ControlCommand.EXCHANGE;
                            bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                            return true;
                        }

                        if (cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.curLDRQ_EmptySlotList.Count == 0)
                        {
                            //20151110 add 如果1stJob curStep設定可以CrossStage 那還是要可以處理
                            if (cur1stJobCmdInfo.cur1stJob_1stCommand_CrossStageFlag != "Y")
                            {

                                //1st Job Command 之後就沒有空的Slot則不需要2ndJob Command
                                if (cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity != null)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job 2nd Command TargetStageID({3})!",
                                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);
                                }
                                else
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job and 2nd Command is null",
                                                            curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);
                                }

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [PUT change to RECIPEGROUPEND_PUT]
                                //20160511 將Cmd01 PUT 改成 RECIPEGROUPEND_PUT
                                if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_PUT &&
                                    cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGETYPE == eRobotStageType.EQUIPMENT &&
                                    Workbench.LineType == eLineType.ARRAY.IMP_NISSIN)
                                {
                                    RobotStage JobSourceStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotArmJob.SourcePortID); //取得目前可控的這片Job的來源stage(CST)
                                    List<RobotStage> stages = ObjectManager.RobotStageManager.GetRobotStages();  //取得所有的stages
                                    List<RobotStage> ports = new List<RobotStage>();
                                    List<Port> portsList = ObjectManager.PortManager.GetPorts().Where(p => p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING).ToList<Port>(); //取得所有IN_PROCESSING或WAITING_FOR_PROCESSING ports
                                    List<Job> JobList = new List<Job>();

                                    foreach (RobotStage stage in stages)
                                    {
                                        if (portsList.Find(p => p.Data.PORTNO == stage.Data.STAGEID) != null && stage.Data.STAGEID != JobSourceStage.Data.STAGEID)
                                            ports.Add(stage);  //去除來源CST,將目前IN_PROCESSING或WAITING_FOR_PROCESSING的CST加到ports
                                    }
                                    if (JobSourceStage != null)
                                    {

                                        if (JobSourceStage.AllJobRecipeGroupNoList.Find(s => s.ArraySpecial.RecipeGroupNumber.Trim() == curRobotArmJob.ArraySpecial.RecipeGroupNumber.Trim()) == null && JobSourceStage.AllJobRecipeGroupNoList.Count > 0)  //目前控制的Job,在自己的CST找不到一樣的RecipeGroupNumber的其他可控Job,且又不是最後一片,Cmd01 = ACTION_RECIPEGROUPEND_PUT
                                        {
                                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                        }
                                        if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count == 0)  //目前控制的Job是最後一片,且沒有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,Cmd01 = ACTION_RECIPEGROUPEND_PUT
                                        {
                                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                        }
                                        if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count > 0)  //目前控制的Job是最後一片,且有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,再繼續判斷
                                        {
                                            foreach (RobotStage port in ports)
                                            {
                                                if (port.AllJobRecipeGroupNoList.Count != 0)
                                                {
                                                    foreach (Job job in port.AllJobRecipeGroupNoList)
                                                    {
                                                        JobList.Add(job);  //將所有IN_PROCESSING或WAITING_FOR_PROCESSING的CST的Jobs加到JobList
                                                    }
                                                }
                                            }
                                            List<Job> JobListOrderBy = JobList.OrderBy(j => j.WaitForProcessTime).ToList<Job>();  //將JobList排序,依照WaitForProcessTime先後排
                                            if (JobListOrderBy.Count == 0)  //IN_PROCESSING或WAITING_FOR_PROCESSING的CST,因為排序後沒有Job Exist,就直接下RECIPEGROUPEND_PUT
                                            {
                                                cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                            }
                                            else
                                            {
                                                DateTime datetime = JobListOrderBy[0].WaitForProcessTime;  //取得第一個最早WaitForProcessTime的時間(因為同CST的Job的WaitForProcessTime都一樣)
                                                List<Job> SelectJobList = new List<Job>();  //同時二個以上的CST Waitforprocess時,用WaitForProcessTime把時間在後面的CST去掉
                                                foreach (Job J in JobListOrderBy)
                                                {
                                                    SelectJobList.Add(J);
                                                    if (J.WaitForProcessTime != datetime)
                                                        SelectJobList.Remove(J); //只保留同WaitForProcessTime的CST的Jobs
                                                }
                                                if (SelectJobList.Find(j => j.ArraySpecial.RecipeGroupNumber.Trim() == curRobotArmJob.ArraySpecial.RecipeGroupNumber.Trim()) == null) //目前控制的Job與另一個選到的IN_PROCESSING或WAITING_FOR_PROCESSING的CST的所有可控Job的RecipeGroupNumber都不一致時,Cmd01 = RECIPEGROUPEND_PUT
                                                {
                                                    cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                                //直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                                return true;
                            }

                        }

                        #endregion

                        #region [ 20160121 add for 如果Target Stage為Port Type且屬性是會根據收到第一片之後才會變化的則不判斷第二片Job馬上執行命令. EX:PortMode= EMP ]

                        //只要判斷1stJob 1stCmd target 目的地是否為Port
                        if (cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGETYPE == eRobotStageType.PORT)
                        {

                            #region [ Get Port Entity by 1stJob 2ndCmd Target Stage ]

                            Port curTargetPort = ObjectManager.PortManager.GetPort(cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID);

                            if (curTargetPort == null)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Target Port Entity By StageID({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                                                        cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                //找不到Port Entity則直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                                return true;
                            }

                            #endregion

                            if (curTargetPort.File.Mode == ePortMode.EMPMode && cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.PortCassetteEmpty == RobotStage.PORTCSTEMPTY.EMPTY)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Target StageID({4}) Port Mode is ({5}) and No Check 2ndJob Condition!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                                                        cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, ePortMode.EMPMode.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                //目的Port PortMode為EMP(會根據第一片變動影響後續配片邏輯)則直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                                return true;

                            }

                        }

                        #endregion

                        #endregion

                        #region [PUT change to RECIPEGROUPEND_PUT]
                        //20160511 將Cmd01 PUT 改成 RECIPEGROUPEND_PUT
                        if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_PUT &&
                            cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGETYPE == eRobotStageType.EQUIPMENT &&
                            Workbench.LineType == eLineType.ARRAY.IMP_NISSIN)
                        {
                            RobotStage JobSourceStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotArmJob.SourcePortID); //取得目前可控的這片Job的來源stage(CST)
                            List<RobotStage> stages = ObjectManager.RobotStageManager.GetRobotStages();  //取得所有的stages
                            List<RobotStage> ports = new List<RobotStage>();
                            List<Port> portsList = ObjectManager.PortManager.GetPorts().Where(p => p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING).ToList<Port>(); //取得所有IN_PROCESSING或WAITING_FOR_PROCESSING ports
                            List<Job> JobList = new List<Job>();

                            foreach (RobotStage stage in stages)
                            {
                                if (portsList.Find(p => p.Data.PORTNO == stage.Data.STAGEID) != null && stage.Data.STAGEID != JobSourceStage.Data.STAGEID)
                                    ports.Add(stage);  //去除來源CST,將目前IN_PROCESSING或WAITING_FOR_PROCESSING的CST加到ports
                            }
                            if (JobSourceStage != null)
                            {

                                if (JobSourceStage.AllJobRecipeGroupNoList.Find(s => s.ArraySpecial.RecipeGroupNumber.Trim() == curRobotArmJob.ArraySpecial.RecipeGroupNumber.Trim()) == null && JobSourceStage.AllJobRecipeGroupNoList.Count > 0)  //目前控制的Job,在自己的CST找不到一樣的RecipeGroupNumber的其他可控Job,且又不是最後一片,Cmd01 = ACTION_RECIPEGROUPEND_PUT
                                {
                                    cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                }
                                if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count == 0)  //目前控制的Job是最後一片,且沒有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,Cmd01 = ACTION_RECIPEGROUPEND_PUT
                                {
                                    cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                }
                                if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count > 0)  //目前控制的Job是最後一片,且有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,再繼續判斷
                                {
                                    foreach (RobotStage port in ports)
                                    {
                                        if (port.AllJobRecipeGroupNoList.Count != 0)
                                        {
                                            foreach (Job job in port.AllJobRecipeGroupNoList)
                                            {
                                                JobList.Add(job);  //將所有IN_PROCESSING或WAITING_FOR_PROCESSING的CST的Jobs加到JobList
                                            }
                                        }
                                    }
                                    List<Job> JobListOrderBy = JobList.OrderBy(j => j.WaitForProcessTime).ToList<Job>();  //將JobList排序,依照WaitForProcessTime先後排
                                    if (JobListOrderBy.Count == 0)  //IN_PROCESSING或WAITING_FOR_PROCESSING的CST,因為排序後沒有Job Exist,就直接下RECIPEGROUPEND_PUT
                                    {
                                        cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                    }
                                    else
                                    {
                                        DateTime datetime = JobListOrderBy[0].WaitForProcessTime;  //取得第一個最早WaitForProcessTime的時間(因為同CST的Job的WaitForProcessTime都一樣)
                                        List<Job> SelectJobList = new List<Job>();  //同時二個以上的CST Waitforprocess時,用WaitForProcessTime把時間在後面的CST去掉
                                        foreach (Job J in JobListOrderBy)
                                        {
                                            SelectJobList.Add(J);
                                            if (J.WaitForProcessTime != datetime)
                                                SelectJobList.Remove(J); //只保留同WaitForProcessTime的CST的Jobs
                                        }
                                        if (SelectJobList.Find(j => j.ArraySpecial.RecipeGroupNumber.Trim() == curRobotArmJob.ArraySpecial.RecipeGroupNumber.Trim()) == null) //目前控制的Job與另一個選到的IN_PROCESSING或WAITING_FOR_PROCESSING的CST的所有可控Job的RecipeGroupNumber都不一致時,Cmd01 = RECIPEGROUPEND_PUT
                                        {
                                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                    }
                    else
                    {

                        #region [ Judge 1st Job Command and 2nd Job Command ]

                        #region [ 20151025 add Check Multi-Single Condition ]

                        //20151102 Modify 如果對IMP是2片存取 Check 1st Command是否要改成Multi-Get/Multi-Put.
                        Check1stJob1stCommandMultiSingleCommandCondition(curRobot, curRobotArmJob, cur1stJobCmdInfo.cur1stJob_Command);

                        #endregion

                        RobotCmdInfo curJudgeRobotCommand = new RobotCmdInfo();

                        //20151102 Modify 如果對IMP是2片存取 Check 1st Command改成Multi-Get/Multi-Put且不需要2nd Command
                        if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_MULTI_GET ||
                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_MULTI_PUT)
                        {
                            #region [MULTI_PUT change to MULTIRECIPEGROUPEND_PUT]
                            //20160511
                            if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_MULTI_PUT && Workbench.LineType == eLineType.ARRAY.IMP_NISSIN)
                            {
                                RobotStage JobSourceStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotArmJob.SourcePortID); //取得目前可控的這片Job的來源stage(CST)
                                List<RobotStage> stages = ObjectManager.RobotStageManager.GetRobotStages();  //取得所有的stages
                                List<RobotStage> ports = new List<RobotStage>();
                                List<Port> portsList = ObjectManager.PortManager.GetPorts().Where(p => p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING).ToList<Port>();
                                List<Job> JobList = new List<Job>();

                                foreach(RobotStage stage in stages)
                                {
                                    if (portsList.Find(p => p.Data.PORTNO == stage.Data.STAGEID) != null && stage.Data.STAGEID != JobSourceStage.Data.STAGEID)
                                        ports.Add(stage); //去除來源CST,將目前IN_PROCESSING或WAITING_FOR_PROCESSING的CST加到ports
                                }
                                if (JobSourceStage != null)
                                {

                                    //mark by yang,注释改为->目前控制的Job与所在CST的所有可控Job的RecipeGroupNumber都不一致時,Cmd01 = RECIPEGROUPEND_PUT(不需要跨CST抽相同RecipeGroupNumber)
                                    if (JobSourceStage.AllJobRecipeGroupNoList.Find(s => s.ArraySpecial.RecipeGroupNumber.Trim() == curRobotArmJob.ArraySpecial.RecipeGroupNumber.Trim()) == null && JobSourceStage.AllJobRecipeGroupNoList.Count > 0) //目前控制的Job與另一個選到的IN_PROCESSING或WAITING_FOR_PROCESSING的CST的所有可控Job的RecipeGroupNumber都不一致時,Cmd01 = RECIPEGROUPEND_PUT
                                    {
                                        cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT;
                                    }
                                    if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count == 0) //目前控制的Job是最後一片,且沒有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,Cmd01 = ACTION_RECIPEGROUPEND_PUT
                                    {
                                        cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT;
                                    }
                                    if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count > 0) //目前控制的Job是最後一片,且有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,再繼續判斷
                                    {
                                        foreach (RobotStage port in ports)
                                        {
                                            if (port.AllJobRecipeGroupNoList.Count != 0)
                                            {
                                                foreach (Job job in port.AllJobRecipeGroupNoList)
                                                {
                                                    JobList.Add(job);  //將所有IN_PROCESSING或WAITING_FOR_PROCESSING的CST的Jobs加到JobList                                               
                                                }
                                            }
                                        }
                                        List<Job> JobListOrderBy = JobList.OrderBy(j => j.WaitForProcessTime).ToList<Job>();  //將JobList排序,依照WaitForProcessTime先後排
                                        if (JobListOrderBy.Count == 0)  //IN_PROCESSING或WAITING_FOR_PROCESSING的CST,因為排序後沒有Job Exist,就直接下MULTIRECIPEGROUPEND_PUT
                                        {
                                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT;
                                        }
                                        else
                                        {
                                            DateTime datetime = JobListOrderBy[0].WaitForProcessTime; //取得第一個最早WaitForProcessTime的時間(因為同CST的Job的WaitForProcessTime都一樣)
                                            List<Job> SelectJobList = new List<Job>();  //同時二個以上的CST Waitforprocess時,用WaitForProcessTime把時間在後面的CST去掉
                                            foreach (Job J in JobListOrderBy)
                                            {
                                                SelectJobList.Add(J);
                                                if (J.WaitForProcessTime != datetime)
                                                    SelectJobList.Remove(J); //只保留同WaitForProcessTime的CST的Jobs
                                            }
                                            if (SelectJobList.Find(j => j.ArraySpecial.RecipeGroupNumber.Trim() == curRobotArmJob.ArraySpecial.RecipeGroupNumber.Trim()) == null) //目前控制的Job與另一個選到的IN_PROCESSING或WAITING_FOR_PROCESSING的CST的所有可控Job的RecipeGroupNumber都不一致時,Cmd01 = RECIPEGROUPEND_PUT
                                            {
                                                cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            curJudgeRobotCommand.Cmd01_CSTSeq = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq;
                            curJudgeRobotCommand.Cmd01_JobSeq = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq;
                            curJudgeRobotCommand.Cmd01_JobKey = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobKey;
                            curJudgeRobotCommand.Cmd01_ArmSelect = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect;
                            curJudgeRobotCommand.Cmd01_Command = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command;
                            curJudgeRobotCommand.Cmd01_TargetPosition = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetPosition;
                            curJudgeRobotCommand.Cmd01_TargetSlotNo = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo;

                            //20160412 為了MULTI_GET與MULTI_PUT show log的關係,把Cmd02的CSTSeq,JobSeq,JobKey存起來
                            curJudgeRobotCommand.Cmd02_CSTSeq = curRobotCommand.Cmd01_CSTSeq;
                            curJudgeRobotCommand.Cmd02_JobSeq = curRobotCommand.Cmd01_JobSeq;
                            curJudgeRobotCommand.Cmd02_JobKey = curRobotCommand.Cmd01_JobKey;

                        }
                        else
                        {

                            //1st Job Command Exist and 2nd Job Command Exist , Judge PutPut Command.用2nd Job 1st Command 更新1st Job 2nd Command
                            curJudgeRobotCommand.Cmd01_CSTSeq = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq;
                            curJudgeRobotCommand.Cmd01_JobSeq = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq;
                            curJudgeRobotCommand.Cmd01_JobKey = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobKey;
                            curJudgeRobotCommand.Cmd01_ArmSelect = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect;
                            curJudgeRobotCommand.Cmd01_Command = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command;
                            curJudgeRobotCommand.Cmd01_TargetPosition = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetPosition;
                            curJudgeRobotCommand.Cmd01_TargetSlotNo = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo;

                            curJudgeRobotCommand.Cmd02_CSTSeq = curRobotCommand.Cmd01_CSTSeq;
                            curJudgeRobotCommand.Cmd02_JobSeq = curRobotCommand.Cmd01_JobSeq;
                            curJudgeRobotCommand.Cmd02_JobKey = curRobotCommand.Cmd01_JobKey;
                            curJudgeRobotCommand.Cmd02_ArmSelect = curRobotCommand.Cmd01_ArmSelect;
                            curJudgeRobotCommand.Cmd02_Command = curRobotCommand.Cmd01_Command;
                            curJudgeRobotCommand.Cmd02_TargetPosition = curRobotCommand.Cmd01_TargetPosition;
                            curJudgeRobotCommand.Cmd02_TargetSlotNo = curRobotCommand.Cmd01_TargetSlotNo;
                        }

                        //Send Robot Control Command
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curJudgeRobotCommand });
                       
                        if (sendCmdResult == false)
                        {
                            //無法下命令就結束
                            return false;
                        }
                        else
                        {
                            return true;//通知下完命令結束了
                        }

                        #endregion

                    }

                    #endregion

                }
                else
                {
                    //Create 1 Arm 2 Substrate
                    //[ Wait_For_Proc_00027 ] Arm Job 針對  1Arm 2Job處理
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

        /// <summary> Check Stage上Job目前Step的所有Filter條件是否成立(最多2片同時成立)
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotStageJob"></param>
        /// <returns></returns>
        private bool CheckRobotStageJobRouteCondition_ForGetGetPutPut(Robot curRobot, Job curRobotStageJob)
        {
            string strlog = string.Empty;
            List<RobotStage> curFilterStageList = new List<RobotStage>();
            RobotCmdInfo curRobotCommand = new RobotCmdInfo();
            //20160812
            StaticContext.AddParameter(eRobotContextParameter.CurJobEntity, curRobotStageJob);

            try
            {
                #region [ 20151015 add Check CurStep RouteStepByPass Condition and 準備變更curStep ]

                if (!CheckAllRouteStepByPassCondition2(curRobot, curRobotStageJob, curRobotStageJob.RobotWIP.CurStepNo, ref curFilterStageList)) return false; //StepByPass條件有問題則回覆NG
                #endregion

                #region [ 20151017 add Check CurStep RouteStepJump Condition and 準備變更curStep ]
                if (!CheckAllRouteStepJumpCondition2(curRobot, curRobotStageJob, curRobotStageJob.RobotWIP.CurStepNo, ref curFilterStageList)) return false; //StepJump條件有問題則回覆NG
                #endregion

                #region [ ***** 20151027 add Check 2nd Job Command 1st Action & TargetPosition Rule by 1stJob Command Info ]

                cur1stJob_CommandInfo cur1stJobCmdInfo = (cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo]; //取得 1st Job 的 Command 信息!

                if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE) //代表有命令需要處理!!
                {

                    #region [ 1st Job 2nd Command Action 必須是PUT to Stage. 如果是2nd Command不是Put or Multi-Put[此时命令是Exchange或Get/put]則不需考慮2nd Job Command ]

                    if (cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command != eRobot_Trx_CommandAction.ACTION_PUT && cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command != eRobot_Trx_CommandAction.ACTION_MULTI_PUT)
                    {
                        //直接下命令並回true不需考慮2nd Job
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                        return true;
                    }

                    #endregion

                    #region [ 1st Job 1st Command Action 必須要是Get or Multi-Get並決定1stJob會占用哪支Arm. 如果是1st Command不是Get or Multi-Get則不需考慮2nd Job Command ]

                    //1st Job Command 1st Action必須是Get from Stage.且Use Arm必須要是1 or 2如果是1st Command 不是Get or Multi-Get則不需考慮2nd Job Command
                    if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_GET &&
                        cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_MULTI_GET &&
                        cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != 1 &&
                        cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != 2)
                    {

                        //直接下命令並回true不需考慮2nd Job
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                        return true;
                    }

                    lock (curRobot)
                    {
                        //將1st Job Command 1st Command Action會用到的Arm先預約起來 20151208 modify by RealTimeArmInfo
                        //curRobot.File.ArmSignalSubstrateInfoList[cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 1].ArmJobExist = eGlassExist.Exist;
                        curRobot.CurTempArmSingleJobInfoList[cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 1].ArmJobExist = eGlassExist.Exist;
                    }

                    #endregion

                    #region [ Update 1stJob 2nd Command Target StageID Empty Slotlist Info by Target SlotNo ]

                    if (cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotList.ContainsKey(cur1stJobCmdInfo.cur1stJob_Command.Cmd02_TargetSlotNo) == true)
                    {
                        //根據1st Job  2nd Command Target Position/SlotNo 將Target Position的EmptySlotNo預約起來(排除引用)
                        cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotList.Remove(cur1stJobCmdInfo.cur1stJob_Command.Cmd02_TargetSlotNo);
                    }

                    if (cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotList.Count == 0)
                    {
                        //20151110 add 如果1st Job NextStep設定可以CrossStage(1stJob的Target點)那還是要可以處理
                        if (cur1stJobCmdInfo.cur1stJob_2ndCommand_CrossStageFlag != "Y")
                        {
                            //1st Job Command 之後就沒有空的Slot則不需要2ndJob Command
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job 2nd Command TargetStageID({3})!",
                                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            //直接下命令並回true不需考慮2nd Job
                            bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                            return true;
                        }

                    }

                    #endregion

                    #region [ Get 2ndJob Current Step Entity ]

                    RobotRouteStep cur2ndJob_curRouteStep = curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.CurStepNo];

                    //找不到 CurStep Route 回NG
                    if (cur2ndJob_curRouteStep == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                    curRobotStageJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    #endregion

                    #region [ Check 2ndJob_1stCommand Can Use StageList by 1stJob_1stCommand TargetStageEntity ]

                    //By Pass & Jump之後會取得2ndJob目前Step的所有StageList. 判斷1stJob的 Source Stage 是否有在此範圍內.
                    if (curFilterStageList.Contains(cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity) == false)
                    {
                        //2nd Job目前Use StageList不和1stJob 1st Command TargetPosition 不相同 則不需要再考慮這一片
                        //因為是當下決定 所以直接記Log 可考慮不需要Debug
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) 1st TargetPodition({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) curStepNo({7}) StageList({8}) is different!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq.ToString(), cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq.ToString(),
                                                cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo, curRobotStageJob.RobotWIP.CurStepNo.ToString(),
                                                cur2ndJob_curRouteStep.Data.STAGEIDLIST);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        return false;

                    }

                    #endregion

                    #region [ 判斷是否允許同Step跨Stage取片 ]

                    bool getJobFromAnotherStageFlag = false;

                    //20151110 add 比對1st Job 與2nd Job 是否可以Cross Stage 取片
                    if (cur1stJobCmdInfo.cur1stJob_1stCommand_CrossStageFlag == "Y" &&
                        cur2ndJob_curRouteStep.Data.CROSSSTAGEFLAG == "Y")
                    {
                        getJobFromAnotherStageFlag = true;
                    }

                    if (getJobFromAnotherStageFlag == false)
                    {
                        //目前2ndJob 所在的位置與1st Job 1st Command Target Stage不同且不允許跨Stage則不可一起出片
                        if ((curRobotStageJob.RobotWIP.CurLocation_StageID != cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID))
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) 1st TargetPodition({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) curKStepNo({7}) curLocation_StageID({8}) is different!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq.ToString(), cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq.ToString(),
                                                    cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo, curRobotStageJob.RobotWIP.CurStepNo.ToString(),
                                                    curRobotStageJob.RobotWIP.CurLocation_StageID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return false;

                        }
                    }

                    //允許跨Stage取片則不需比對Job1 and Job2 Location StageID是否相同

                    #endregion

                    #region [ Check 2ndJob_1stCommand Action by 1stJob_1stCommand ActionCode ]

                    //Mulit-PutGet 在DB無法預設所以要有Temp轉換
                    string tmpAction = string.Empty;

                    if (cur1stJobCmdInfo.cur1stJob_1stCommand_DBActionCode == eRobot_DB_CommandAction.ACTION_MULTI_GET)
                    {
                        tmpAction = eRobot_DB_CommandAction.ACTION_GET;
                    }
                    else if (cur1stJobCmdInfo.cur1stJob_1stCommand_DBActionCode == eRobot_DB_CommandAction.ACTION_MULTI_PUT)
                    {
                        tmpAction = eRobot_DB_CommandAction.ACTION_PUT;
                    }
                    else
                    {
                        tmpAction = cur1stJobCmdInfo.cur1stJob_1stCommand_DBActionCode;
                    }

                    if (cur2ndJob_curRouteStep.Data.ROBOTACTION != tmpAction)
                    {
                        //2nd Job目前Step Action不和1stJob 1st Command Action 不相同 則不需要再考慮這一片
                        //因為是當下決定 所以直接記Log 可考慮不需要Debug
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) Action({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) curStepNo({7}) Action({8}) is different!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq.ToString(), cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq.ToString(),
                                                cur1stJobCmdInfo.cur1stJob_1stCommand_DBActionCode, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo, curRobotStageJob.RobotWIP.CurStepNo.ToString(),
                                                cur2ndJob_curRouteStep.Data.ROBOTACTION);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        return false;

                    }

                    #endregion

                }

                #endregion

                DefineNormalRobotCmd cur1stDefindCmd = new DefineNormalRobotCmd();
                DefineNormalRobotCmd cur2ndDefindCmd = new DefineNormalRobotCmd();

                #region [ Check CurStep All Filter Condition ]
                //20150825 work End 要考慮LDRQ Stage Type如果是Port 要特別處理!![ Wait_Proc_00029 ] Check Next Stage 要多考慮Port Type 處理
                //Arm Job Only Check curStep Filter
                curRobotStageJob.RobotWIP.SorterMode_OtherFilterOK = true;


                if (!CheckAllFilterConditionByStepNo2(curRobot, curRobotStageJob, curRobotStageJob.RobotWIP.CurStepNo, cur1stDefindCmd, cur2ndDefindCmd, ref curFilterStageList))
                {
                    curRobotStageJob.RobotWIP.SorterMode_OtherFilterOK = false;//不能出片的Job, 不用考慮是否退Cassette

                    //20160127 add Set Wait CST Event
                    Cell_SendWaitCassetteStatusToBCS(MethodBase.GetCurrentMethod().Name, curRobot, curRobotStageJob, false);

                    return false; //Filter條件有問題則回覆NG
                }

                //20160127 add 當通過Filter時通知BCS Clear Wait CST Event
                Cell_SendWaitCassetteStatusToBCS(MethodBase.GetCurrentMethod().Name, curRobot, curRobotStageJob, true);

                #endregion

                #region [ ***** 20151027 add Check 2nd Job Command 1st Action & TargetPosition Rule ]

                if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                {
                    RobotRouteStep cur2ndJob_nextRouteStep = curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.NextStepNo];

                    #region [ Check 2ndJob_1stCommand Can Use StageList by 1stJob_1stCommand TargetStageEntity ]

                    //預設不允許同Step跨Stage放片20151029 add
                    bool putJobtoAnotherStageFlag = false;

                    //20151110 add 比對1st Job 與2nd Job 是否可以Cross
                    if (cur1stJobCmdInfo.cur1stJob_2ndCommand_CrossStageFlag == "Y" &&
                        cur2ndJob_nextRouteStep.Data.CROSSSTAGEFLAG == "Y")
                    {
                        putJobtoAnotherStageFlag = true;
                    }


                    //如果2nd Job 1st Command 是PUT相關則表示 curFilterStageList是給2nd Job 1st Cmd用=>在Arm上有Job Function使用
                    //如果2nd Job 2nd Command 是PUT相關則表示 curFilterStageList是給2nd Job 2nd Cmd用=>在此使用
                    if (curFilterStageList.Contains(cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity) == false)
                    {
                        //2ndJob的Target目標不包含1stJob的Target點時的處理

                        #region [ 判斷是否允許同Step跨Stage放片,允許的話則不更新curFilterStageList ]

                        if (putJobtoAnotherStageFlag == false)
                        {

                            //2nd Job 2nd Command目前Use StageList不和1stJob 2nd Command TargetPosition 不相同 則不需要再考慮這一片
                            //因為是當下決定 所以直接記Log 可考慮不需要Debug
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) 2nd TargetPodition({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) nextStepNo({7}) StageList({8}) is different!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq.ToString(), cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq.ToString(),
                                                    cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo, curRobotStageJob.RobotWIP.NextStepNo.ToString(),
                                                    cur2ndJob_nextRouteStep.Data.STAGEIDLIST);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return false;
                        }

                        #endregion

                        //如果允許同Step跨Stage放片則維持原先的curFilterStageList
                    }
                    else
                    {
                        //2ndJob的Target目標包含1stJob的Target點時的處理

                        //20151110 add當2ndJob 2nd Cmd可去Stage包含1stJob的2nd Cmd Target Stage 且1stJob 2nd Cmd Target Stage不能再放片時,2nd Job的Target點要排除掉沒有空Slot的Stage
                        if (cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotList.Count == 0)
                        {
                            curFilterStageList.Remove(cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity);

                            if (curFilterStageList.Count == 0)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) 1stJob CassetteSequenceNo({2}) JobSequenceNo({3}) 2nd TargetPodition({4}) But 2ndJob CassetteSequenceNo({5}) JobSequenceNo({6}) nextStepNo({7}) StageList({8}) can not Receive!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq.ToString(), cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq.ToString(),
                                                    cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity.Data.STAGEID, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo, curRobotStageJob.RobotWIP.NextStepNo.ToString(),
                                                    cur2ndJob_nextRouteStep.Data.STAGEIDLIST);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                return false;
                            }
                        }
                        else
                        {
                            //1stJob目的Stage還能收片則強制2nd Job只能進同樣Stage
                            curFilterStageList.Clear();
                            curFilterStageList.Add(cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity);
                        }
                    }

                    #endregion

                }
                #endregion

                #region [ Check All OrderBy Condition and define Target Position and SlotNo ]

                //Check 1st Cmd is Exist
                if (cur1stDefindCmd.Cmd01_Command == 0)
                {
                    //沒有1st Command 則記Error 離開
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) Command Action({8}) is illegal",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                curRobotStageJob.RobotWIP.CurLocation_StageID, curRobotStageJob.RobotWIP.CurStepNo.ToString(),
                                                curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                cur1stDefindCmd.Cmd01_Command.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }

                //Order 1st & 2nd Cmd
                if (CheckAllOrderByConditionByCommand(curRobot, curRobotStageJob, cur1stDefindCmd, cur2ndDefindCmd, curFilterStageList) == false)
                {

                    //20151026 add Order By後Cmd 的TargetPosition or SlotNo有問題則回覆NG
                    return false;

                }

                #endregion

                #region Line Special Rules by Line Type
                //20160109-001-dd::robot 内部的简易配片逻辑!! 目前还是先看 PROC_RESULT 那边的结果, 这边先注解!! 如果那边有问题, 再考虑这!!
                //Line _line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                //if (_line == null) return false;

                //switch (_line.Data.LINETYPE)
                //{
                //    case eLineType.ARRAY.DRY_ICD:
                //    case eLineType.ARRAY.DRY_YAC:
                //        curRobot.File.DryLastProcessType = curRobotStageJob.ArraySpecial.ProcessType.ToString();
                //        break;
                //    default: break;
                //}
                #endregion

                #region [ by RobotArm Qty Create Command ]

                if (curRobot.Data.ARMJOBQTY == 1)
                {

                    #region [ 20151022 add Check First Glass Command ]

                    //Check 1st Command
                    if (!PortFetchOut_FirstGlassCheck(curRobot, curRobotStageJob, cur1stDefindCmd, false))
                    {

                        return false;
                    }

                    //Check 2nd Command
                    if (!PortFetchOut_FirstGlassCheck(curRobot, curRobotStageJob, cur2ndDefindCmd, true))
                    {

                        return false;
                    }

                    #endregion

                    #region [ 20151102 add Check GET/PUT Condition ]
                    //GlobalAssemblyVersion v1.0.0.26-20151102
                    CheckGetPutCommandCondition(curRobot, curRobotStageJob, cur1stDefindCmd); //Check 1st Command
                    CheckGetPutCommandCondition(curRobot, curRobotStageJob, cur2ndDefindCmd, true); //Check 2nd Command
                    #endregion

                    #region [ 20151109 add Check PUTREADY Condition ]
                    //GlobalAssemblyVersion v1.0.0.26-20151109
                    //CheckPutReadyCommandCondition(curRobot, curRobotStageJob, cur1stDefindCmd, cur2ndDefindCmd); //Check 1st Command
                    CheckPutReadyCommandCondition(curRobot, curRobotStageJob, cur1stDefindCmd,cur2ndDefindCmd, true); //Check 2nd Command

                    //no cmd,return false
                    if (cur1stDefindCmd.Cmd01_Command == 0)
                    {
                        //沒有1st Command 則記Error 離開
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) Command Action({8}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                    curRobotStageJob.RobotWIP.CurLocation_StageID, curRobotStageJob.RobotWIP.CurStepNo.ToString(),
                                                    curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    cur1stDefindCmd.Cmd01_Command.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }
                    #endregion

                    #region [ 20151230 add Check RTCPUT Condition ] 
                    //这边不会触发, 但是还是有加入!
                    //GlobalAssemblyVersion v1.0.0.9-20151230
                    CheckRtcPutCommandCondition(curRobot, curRobotStageJob, cur1stDefindCmd); //Check 1st Command
                    CheckRtcPutCommandCondition(curRobot, curRobotStageJob, cur2ndDefindCmd, true); //Check 2nd Command
                    #endregion

                    #region [ 当command如果是RTC_PUT命令的话, 需要更新BCS JOB RobotRTCFlag=True ]
                    Invoke("RobotSpecialService", "Update_BCS_Job_RobotRTCFlag_For1Cmd_1Arm_1Job", new object[] { curRobot, curRobotStageJob, cur1stDefindCmd });
                    Invoke("RobotSpecialService", "Update_BCS_Job_RobotRTCFlag_For1Cmd_1Arm_1Job", new object[] { curRobot, curRobotStageJob, cur2ndDefindCmd });
                    #endregion

                    #region [ 20151025 add Check Multi-Single Condition ] 20151102 Mark . 只有對Mulit Type EQP 存取2片時才會下Multi命令

                    ////Check 1st Command
                    //CheckMultiSingleCommandCondition(curRobot, curRobotStageJob, cur1stDefindCmd, false);
                    ////Check 2nd Command
                    //CheckMultiSingleCommandCondition(curRobot, curRobotStageJob, cur2ndDefindCmd, true);

                    #endregion

                    #region [ Create 1 Arm 1 Substrate ]

                    int int1stCstSeqNo = 0;
                    int int1stJobSeqNo = 0;
                    int int2ndCstSeqNo = 0;
                    int int2ndJobSeqNo = 0;

                    curRobotCommand.Cmd01_Command = cur1stDefindCmd.Cmd01_Command;
                    curRobotCommand.Cmd01_ArmSelect = cur1stDefindCmd.Cmd01_ArmSelect;
                    curRobotCommand.Cmd01_TargetPosition = cur1stDefindCmd.Cmd01_TargetPosition;
                    curRobotCommand.Cmd01_TargetSlotNo = cur1stDefindCmd.Cmd01_TargetSlotNo;
                    int.TryParse(cur1stDefindCmd.Cmd01_CstSeq, out int1stCstSeqNo);
                    curRobotCommand.Cmd01_CSTSeq = int1stCstSeqNo;
                    int.TryParse(cur1stDefindCmd.Cmd01_JobSeq, out int1stJobSeqNo);
                    curRobotCommand.Cmd01_JobSeq = int1stJobSeqNo;

                    curRobotCommand.Cmd02_Command = cur2ndDefindCmd.Cmd01_Command;
                    curRobotCommand.Cmd02_ArmSelect = cur2ndDefindCmd.Cmd01_ArmSelect;
                    curRobotCommand.Cmd02_TargetPosition = cur2ndDefindCmd.Cmd01_TargetPosition;
                    curRobotCommand.Cmd02_TargetSlotNo = cur2ndDefindCmd.Cmd01_TargetSlotNo;
                    int.TryParse(cur2ndDefindCmd.Cmd01_CstSeq, out int2ndCstSeqNo);
                    curRobotCommand.Cmd02_CSTSeq = int2ndCstSeqNo;
                    int.TryParse(cur2ndDefindCmd.Cmd01_JobSeq, out int2ndJobSeqNo);
                    curRobotCommand.Cmd02_JobSeq = int2ndJobSeqNo;

                    #endregion

                    #region [ 20151027 set cur 1st Job CommandInfo ]

                    if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_NONE)
                    {

                        #region [ Set 1stJob Command Info ]

                        cur1stJobCmdInfo.cur1stJob_Command = curRobotCommand;
                        cur1stJobCmdInfo.cur1stJob_1stCommand_DBActionCode = GetRobotCommandActionDesc(curRobotCommand.Cmd01_Command);
                        cur1stJobCmdInfo.cur1stJob_2ndCommand_DBActionCode = GetRobotCommandActionDesc(curRobotCommand.Cmd02_Command);

                        if (curRobotCommand.Cmd01_TargetPosition != 0)
                        {
                            RobotStage cur1stCmdStageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotCommand.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));
                            cur1stJobCmdInfo.cur1stJob_1stCommand_TargetStageEntity = cur1stCmdStageEntity;
                        }

                        if (curRobotCommand.Cmd02_TargetPosition != 0)
                        {
                            RobotStage cur2ndCmdStageEntity = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotCommand.Cmd02_TargetPosition.ToString().PadLeft(2, '0'));
                            cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity = cur2ndCmdStageEntity;
                        }

                        #region [ 1st Job 1st Command Action 必須要是Get or Multi-Get並決定1stJob會占用哪支Arm. 如果是1stJob 1st Command不是Get or Multi-Get則不需考慮2nd Job Command ]

                        //1st Job Command 1st Action必須是Get from Stage.且Use Arm必須要是1 or 2如果是1st Command 不是Get or Multi-Get則不需考慮2nd Job Command
                        if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_GET &&
                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_MULTI_GET &&
                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != 1 &&
                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect != 2)
                        {

                            //直接下命令並回true不需考慮2nd Job
                            bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                            return true;
                        }

                        lock (curRobot)
                        {
                            //將1st Job Command 1st Command Action會用到的Arm先預約起來  20151208 modify by RealTimeArmInfo
                            //curRobot.File.ArmSignalSubstrateInfoList[cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 1].ArmJobExist = eGlassExist.Exist;
                            curRobot.CurTempArmSingleJobInfoList[cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect - 1].ArmJobExist = eGlassExist.Exist;
                        }

                        #endregion

                        #region [ 20151110 add for Get 1stJob CurStep Entity and UpDate 1stJob 1stCmd CrossStageFlag ]

                        //Stage Job Must Check CurStepInfo and NextStepInfo

                        RobotRouteStep cur1stJob_CurRouteStepInfo = null;

                        if (curRobotStageJob.RobotWIP.RobotRouteStepList.ContainsKey(curRobotStageJob.RobotWIP.CurStepNo) == true)
                        {
                            cur1stJob_CurRouteStepInfo = curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.CurStepNo];
                        }

                        //找不到 CurStep Route 回NG
                        if (cur1stJob_CurRouteStepInfo == null)
                        {

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                        curRobotStageJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        cur1stJobCmdInfo.cur1stJob_1stCommand_CrossStageFlag = cur1stJob_CurRouteStepInfo.Data.CROSSSTAGEFLAG;

                        #endregion

                        #region [ 20151110 add for Get 1stJob NextStep Entity and UpDate 1stJob 2ndCmd CrossStageFlag ]

                        //Stage Job Must Check CurStepInfo and NextStepInfo
                        RobotRouteStep cur1stJob_NextRouteStepInfo = null;

                        if (curRobotStageJob.RobotWIP.RobotRouteStepList.ContainsKey(curRobotStageJob.RobotWIP.NextStepNo) == true)
                        {
                            cur1stJob_NextRouteStepInfo = curRobotStageJob.RobotWIP.RobotRouteStepList[curRobotStageJob.RobotWIP.NextStepNo];
                        }

                        //找不到 CurStep Route 回NG
                        if (cur1stJob_NextRouteStepInfo == null)
                        {

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get NextRouteStep({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                        curRobotStageJob.RobotWIP.NextStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        cur1stJobCmdInfo.cur1stJob_2ndCommand_CrossStageFlag = cur1stJob_NextRouteStepInfo.Data.CROSSSTAGEFLAG;

                        #endregion

                        #region [ Update 1stJob 2nd Command Target StageID Empty Slotlist Info by Target SlotNo ]

                        if (cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotList.ContainsKey(cur1stJobCmdInfo.cur1stJob_Command.Cmd02_TargetSlotNo) == true)
                        {
                            //根據1st Job  2nd Command Target Position/SlotNo 將Target Position的EmptySlotNo預約起來(排除引用)
                            cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotList.Remove(cur1stJobCmdInfo.cur1stJob_Command.Cmd02_TargetSlotNo);
                        }
                        #region 是否判断第二片job 的依据
                        if (cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.curLDRQ_EmptySlotList.Count == 0)
                        {
                            //20151110 add 如果1stJob NextStep設定可以CrossStage 那還是要可以處理
                            if (cur1stJobCmdInfo.cur1stJob_2ndCommand_CrossStageFlag != "Y")
                            {

                                //1st Job Command 之後就沒有空的Slot則不需要2ndJob Command
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get empty SlotNo by 1st Job 2nd Command TargetStageID({3})!",
                                                        curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [PUT change to RECIPEGROUPEND_PUT]
                                //20160511
                                if (cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command == eRobot_Trx_CommandAction.ACTION_PUT &&
                                    cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGETYPE == eRobotStageType.EQUIPMENT &&
                                    Workbench.LineType == eLineType.ARRAY.IMP_NISSIN)
                                {
                                    RobotStage JobSourceStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotStageJob.SourcePortID); //取得目前可控的這片Job的來源stage(CST)
                                    List<RobotStage> stages = ObjectManager.RobotStageManager.GetRobotStages();  //取得所有stages
                                    List<RobotStage> ports = new List<RobotStage>();
                                    List<Port> portsList = ObjectManager.PortManager.GetPorts().Where(p => p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING).ToList<Port>();
                                    List<Job> JobList = new List<Job>();

                                    foreach (RobotStage stage in stages)
                                    {
                                        if (portsList.Find(p => p.Data.PORTNO == stage.Data.STAGEID) != null && stage.Data.STAGEID != JobSourceStage.Data.STAGEID)
                                            ports.Add(stage); //去除來源CST,將目前IN_PROCESSING或WAITING_FOR_PROCESSING的CST加到ports
                                    }
                                    if (JobSourceStage != null)
                                    {

                                        if (JobSourceStage.AllJobRecipeGroupNoList.Find(s => curRobotStageJob.ArraySpecial.RecipeGroupNumber.Trim() == s.ArraySpecial.RecipeGroupNumber.Trim()) == null && JobSourceStage.AllJobRecipeGroupNoList.Count > 0) //目前控制的Job與另一個選到的IN_PROCESSING或WAITING_FOR_PROCESSING的CST的所有可控Job的RecipeGroupNumber都不一致時,Cmd02 = RECIPEGROUPEND_PUT
                                        {
                                            cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                        }
                                        if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count == 0) //目前控制的Job是最後一片,且沒有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,Cmd02 = ACTION_RECIPEGROUPEND_PUT
                                        {
                                            cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                        }
                                        if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count > 0) //目前控制的Job是最後一片,且有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,再繼續做判斷
                                        {
                                            foreach (RobotStage port in ports)
                                            {
                                                if (port.AllJobRecipeGroupNoList.Count != 0)
                                                {
                                                    foreach (Job job in port.AllJobRecipeGroupNoList)
                                                    {
                                                        JobList.Add(job); //將所有IN_PROCESSING或WAITING_FOR_PROCESSING的CST的Jobs加到JobList
                                                    }
                                                }
                                            }
                                            List<Job> JobListOrderBy = JobList.OrderBy(j => j.WaitForProcessTime).ToList<Job>(); //將JobList排序,依照WaitForProcessTime先後排
                                            if (JobListOrderBy.Count == 0)  //IN_PROCESSING或WAITING_FOR_PROCESSING的CST,因為排序後沒有Job Exist,就直接下RECIPEGROUPEND_PUT
                                            {
                                                cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                            }
                                            else
                                            {
                                                DateTime datetime = JobListOrderBy[0].WaitForProcessTime; //取得第一個最早WaitForProcessTime的時間(因為同CST的Job的WaitForProcessTime都一樣)
                                                List<Job> SelectJobList = new List<Job>();  //同時二個以上的CST Waitforprocess時,用WaitForProcessTime把時間在後面的CST去掉
                                                foreach (Job J in JobListOrderBy)
                                                {
                                                    SelectJobList.Add(J);
                                                    if (J.WaitForProcessTime != datetime)
                                                        SelectJobList.Remove(J); //只保留同WaitForProcessTime的CST的Jobs
                                                }
                                                if (SelectJobList.Find(j => j.ArraySpecial.RecipeGroupNumber.Trim() == curRobotStageJob.ArraySpecial.RecipeGroupNumber.Trim()) == null) //目前控制的Job與另一個選到的IN_PROCESSING或WAITING_FOR_PROCESSING的CST的所有可控Job的RecipeGroupNumber都不一致時,Cmd01 = RECIPEGROUPEND_PUT
                                                {
                                                    cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                //直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                                return true;

                            }
                        }
                        #endregion
                        #endregion

                        #region [ 20160121 add for 如果Target Stage為Port Type且屬性是會根據收到第一片之後才會變化的則不判斷第二片Job馬上執行命令. EX:PortMode= EMP , PortGrade= EM ]

                        //1stJob 1stCmd在此時不可能是PUT所以只要判斷1stJob 2ndCmd是否為Put
                        if (cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command == eRobot_Trx_CommandAction.ACTION_PUT &&
                           cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGETYPE == eRobotStageType.PORT)
                        {

                            #region [ Get Port Entity by 1stJob 2ndCmd Target Stage ]

                            Port curTargetPort = ObjectManager.PortManager.GetPort(cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                            if (curTargetPort == null)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get Target Port Entity By StageID({4})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                        cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                //找不到Port Entity則直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                                return true;
                            }

                            #endregion

                            if (curTargetPort.File.Mode == ePortMode.EMPMode && cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.PortCassetteEmpty == RobotStage.PORTCSTEMPTY.EMPTY)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Target StageID({4}) Port Mode is ({5}) and No Check 2ndJob Condition!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotStageJob.CassetteSequenceNo, curRobotStageJob.JobSequenceNo,
                                                        cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGEID, ePortMode.EMPMode.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                //目的Port PortMode為EMP(會根據第一片變動影響後續配片邏輯)則直接下命令並回true不需考慮2nd Job
                                bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, cur1stJobCmdInfo.cur1stJob_Command });
                                return true;

                            }


                        }

                        #endregion

                        #region [PUT change to RECIPEGROUPEND_PUT]
                        //20160511
                        if (cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command == eRobot_Trx_CommandAction.ACTION_PUT &&
                            cur1stJobCmdInfo.cur1stJob_2ndCommand_TargetStageEntity.Data.STAGETYPE == eRobotStageType.EQUIPMENT && 
                            Workbench.LineType == eLineType.ARRAY.IMP_NISSIN)
                        {
                            RobotStage JobSourceStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotStageJob.SourcePortID); //取得目前可控的這片Job的來源stage(CST)
                            List<RobotStage> stages = ObjectManager.RobotStageManager.GetRobotStages();  //取得所有stages
                            List<RobotStage> ports = new List<RobotStage>();
                            List<Port> portsList = ObjectManager.PortManager.GetPorts().Where(p => p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING).ToList<Port>();
                            List<Job> JobList = new List<Job>();

                            foreach (RobotStage stage in stages)
                            {
                                if (portsList.Find(p => p.Data.PORTNO == stage.Data.STAGEID) != null && stage.Data.STAGEID != JobSourceStage.Data.STAGEID)
                                    ports.Add(stage); //去除來源CST,將目前IN_PROCESSING或WAITING_FOR_PROCESSING的CST加到ports
                            }
                            if (JobSourceStage != null)
                            {

                                if (JobSourceStage.AllJobRecipeGroupNoList.Find(s => curRobotStageJob.ArraySpecial.RecipeGroupNumber.Trim() == s.ArraySpecial.RecipeGroupNumber.Trim()) == null && JobSourceStage.AllJobRecipeGroupNoList.Count > 0) //目前控制的Job與另一個選到的IN_PROCESSING或WAITING_FOR_PROCESSING的CST的所有可控Job的RecipeGroupNumber都不一致時,Cmd02 = RECIPEGROUPEND_PUT
                                {
                                    cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                }
                                if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count == 0) //目前控制的Job是最後一片,且沒有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,Cmd02 = ACTION_RECIPEGROUPEND_PUT
                                {
                                    cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                }
                                if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count > 0) //目前控制的Job是最後一片,且有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,再繼續做判斷
                                {
                                    foreach (RobotStage port in ports)
                                    {
                                        if (port.AllJobRecipeGroupNoList.Count != 0)
                                        {
                                            foreach (Job job in port.AllJobRecipeGroupNoList)
                                            {
                                                JobList.Add(job); //將所有IN_PROCESSING或WAITING_FOR_PROCESSING的CST的Jobs加到JobList
                                            }
                                        }
                                    }
                                    List<Job> JobListOrderBy = JobList.OrderBy(j => j.WaitForProcessTime).ToList<Job>(); //將JobList排序,依照WaitForProcessTime先後排
                                    if (JobListOrderBy.Count == 0)  //IN_PROCESSING或WAITING_FOR_PROCESSING的CST,因為排序後沒有Job Exist,就直接下RECIPEGROUPEND_PUT
                                    {
                                        cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                    }
                                    else
                                    {
                                        DateTime datetime = JobListOrderBy[0].WaitForProcessTime; //取得第一個最早WaitForProcessTime的時間(因為同CST的Job的WaitForProcessTime都一樣)
                                        List<Job> SelectJobList = new List<Job>();  //同時二個以上的CST Waitforprocess時,用WaitForProcessTime把時間在後面的CST去掉
                                        foreach (Job J in JobListOrderBy)
                                        {
                                            SelectJobList.Add(J);
                                            if (J.WaitForProcessTime != datetime)
                                                SelectJobList.Remove(J); //只保留同WaitForProcessTime的CST的Jobs
                                        }
                                        if (SelectJobList.Find(j => j.ArraySpecial.RecipeGroupNumber.Trim() == curRobotStageJob.ArraySpecial.RecipeGroupNumber.Trim()) == null) //目前控制的Job與另一個選到的IN_PROCESSING或WAITING_FOR_PROCESSING的CST的所有可控Job的RecipeGroupNumber都不一致時,Cmd01 = RECIPEGROUPEND_PUT
                                        {
                                            cur1stJobCmdInfo.cur1stJob_Command.Cmd02_Command = eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #endregion

                    }
                    else
                    {

                        #region [ Judge 1st Job Command and 2nd Job Command ]

                        #region [ 20151025 add Check Multi-Single Condition ]

                        //20151102 Modify 如果對IMP是2片存取 Check 1st Command是否要改成Multi-Get/Multi-Put.
                        Check1stJob1stCommandMultiSingleCommandCondition(curRobot, curRobotStageJob, cur1stJobCmdInfo.cur1stJob_Command);

                        #endregion

                        RobotCmdInfo curJudgeRobotCommand = new RobotCmdInfo();

                        //20151102 Modify 如果對IMP是2片存取 Check 1st Command改成Multi-Get/Multi-Put且不需要2nd Command
                        if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_MULTI_GET ||
                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_MULTI_PUT)
                        {
                            #region [MULTI_PUT change to MULTIRECIPEGROUPEND_PUT]
                            //20160511
                            if (cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_MULTI_PUT && Workbench.LineType == eLineType.ARRAY.IMP_NISSIN)
                            {
                                RobotStage JobSourceStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotStageJob.SourcePortID); //取得目前可控的這片Job的來源stage(CST)
                                List<RobotStage> stages = ObjectManager.RobotStageManager.GetRobotStages();  //取得所有stages
                                List<RobotStage> ports = new List<RobotStage>();
                                List<Port> portsList = ObjectManager.PortManager.GetPorts().Where(p => p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING).ToList<Port>();
                                List<Job> JobList = new List<Job>();

                                foreach (RobotStage stage in stages)
                                {
                                    if (portsList.Find(p => p.Data.PORTNO == stage.Data.STAGEID) != null && stage.Data.STAGEID != JobSourceStage.Data.STAGEID)
                                        ports.Add(stage); //去除來源CST,將目前IN_PROCESSING或WAITING_FOR_PROCESSING的CST加到ports
                                }
                                if (JobSourceStage != null)
                                {

                                    if (JobSourceStage.AllJobRecipeGroupNoList.Find(s => s == curRobotStageJob && s.ArraySpecial.RecipeGroupNumber.Trim() == curRobotStageJob.ArraySpecial.RecipeGroupNumber.Trim()) == null && JobSourceStage.AllJobRecipeGroupNoList.Count > 0) //目前控制的Job與另一個選到的IN_PROCESSING或WAITING_FOR_PROCESSING的CST的所有可控Job的RecipeGroupNumber都不一致時,Cmd01 = RECIPEGROUPEND_PUT
                                    {
                                        cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT;
                                    }
                                    if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count == 0) //目前控制的Job是最後一片,且沒有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,Cmd01 = ACTION_RECIPEGROUPEND_PUT
                                    {
                                        cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT;
                                    }
                                    if (JobSourceStage.AllJobRecipeGroupNoList.Count == 0 && ports.Count > 0) //目前控制的Job是最後一片,且有其他IN_PROCESSING或WAITING_FOR_PROCESSING的CST,再繼續做判斷
                                    {
                                        foreach (RobotStage port in ports)
                                        {
                                            if (port.AllJobRecipeGroupNoList.Count != 0)
                                            {
                                                foreach (Job job in port.AllJobRecipeGroupNoList)
                                                {
                                                    JobList.Add(job); //將所有IN_PROCESSING或WAITING_FOR_PROCESSING的CST的Jobs加到JobList
                                                }
                                            }
                                        }
                                        List<Job> JobListOrderBy = JobList.OrderBy(j => j.WaitForProcessTime).ToList<Job>(); //將JobList排序,依照WaitForProcessTime先後排
                                        if (JobListOrderBy.Count == 0)  //IN_PROCESSING或WAITING_FOR_PROCESSING的CST,因為排序後沒有Job Exist,就直接下MULTIRECIPEGROUPEND_PUT
                                        {
                                            cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT;
                                        }
                                        else
                                        {
                                            DateTime datetime = JobListOrderBy[0].WaitForProcessTime; //取得第一個最早WaitForProcessTime的時間(因為同CST的Job的WaitForProcessTime都一樣)
                                            List<Job> SelectJobList = new List<Job>();  //同時二個以上的CST Waitforprocess時,用WaitForProcessTime把時間在後面的CST去掉
                                            foreach (Job J in JobListOrderBy)
                                            {
                                                SelectJobList.Add(J);
                                                if (J.WaitForProcessTime != datetime)
                                                    SelectJobList.Remove(J); //只保留同WaitForProcessTime的CST的Jobs
                                            }
                                            if (SelectJobList.Find(j => j.ArraySpecial.RecipeGroupNumber.Trim() == curRobotStageJob.ArraySpecial.RecipeGroupNumber.Trim()) == null) //目前控制的Job與另一個選到的IN_PROCESSING或WAITING_FOR_PROCESSING的CST的所有可控Job的RecipeGroupNumber都不一致時,Cmd01 = RECIPEGROUPEND_PUT
                                            {
                                                cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTIRECIPEGROUPEND_PUT;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            curJudgeRobotCommand.Cmd01_CSTSeq = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq;
                            curJudgeRobotCommand.Cmd01_JobSeq = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq;
                            curJudgeRobotCommand.Cmd01_JobKey = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobKey;
                            curJudgeRobotCommand.Cmd01_ArmSelect = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect;
                            curJudgeRobotCommand.Cmd01_Command = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command;
                            curJudgeRobotCommand.Cmd01_TargetPosition = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetPosition;
                            curJudgeRobotCommand.Cmd01_TargetSlotNo = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo;

                            //20160412 為了MULTI_GET與MULTI_PUT show log的關係,把Cmd02的CSTSeq,JobSeq,JobKey存起來
                            curJudgeRobotCommand.Cmd02_CSTSeq = curRobotCommand.Cmd01_CSTSeq;
                            curJudgeRobotCommand.Cmd02_JobSeq = curRobotCommand.Cmd01_JobSeq;
                            curJudgeRobotCommand.Cmd02_JobKey = curRobotCommand.Cmd01_JobKey;

                        }
                        else
                        {

                            //1st Job Command Exist and 2nd Job Command Exist , Judge GetGet Command.用2nd Job 1st Command 更新1st Job 2nd Command
                            curJudgeRobotCommand.Cmd01_CSTSeq = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_CSTSeq;
                            curJudgeRobotCommand.Cmd01_JobSeq = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobSeq;
                            curJudgeRobotCommand.Cmd01_JobKey = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_JobKey;
                            curJudgeRobotCommand.Cmd01_ArmSelect = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_ArmSelect;
                            curJudgeRobotCommand.Cmd01_Command = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_Command;
                            curJudgeRobotCommand.Cmd01_TargetPosition = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetPosition;
                            curJudgeRobotCommand.Cmd01_TargetSlotNo = cur1stJobCmdInfo.cur1stJob_Command.Cmd01_TargetSlotNo;

                            curJudgeRobotCommand.Cmd02_CSTSeq = curRobotCommand.Cmd01_CSTSeq;
                            curJudgeRobotCommand.Cmd02_JobSeq = curRobotCommand.Cmd01_JobSeq;
                            curJudgeRobotCommand.Cmd02_JobKey = curRobotCommand.Cmd01_JobKey;
                            curJudgeRobotCommand.Cmd02_ArmSelect = curRobotCommand.Cmd01_ArmSelect;
                            curJudgeRobotCommand.Cmd02_Command = curRobotCommand.Cmd01_Command;
                            curJudgeRobotCommand.Cmd02_TargetPosition = curRobotCommand.Cmd01_TargetPosition;
                            curJudgeRobotCommand.Cmd02_TargetSlotNo = curRobotCommand.Cmd01_TargetSlotNo;
                        }

                        //Send Robot Control Command
                        bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curJudgeRobotCommand });

                        if (sendCmdResult == false)
                        {
                            //無法下命令就結束
                            return false;
                        }
                        else
                        {
                            return true;
                        }

                        #endregion

                    }

                    #endregion

                }
                else
                {
                    //Create 1 Arm 2 Substrate
                    //[ Wait_For_Proc_00027 ] Arm Job 針對  1Arm 2Job處理
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





        //主要Rule 判斷邏輯 Function List =================================================================================================================================================================

        #region [ For Robot Semi Command Function List ]

        //20160121 add cell special semi command
        public bool RobotSemiCommandRequest(string trxID, string eqpId, string robotName, RobotCmdInfo semiCmd, CellSpecialRobotCmdInfo cellSpecial_SemiCmd)
        {
            try
            {

                //進入Semi Cmd Check Rule
                CheckSemiCmdCondition(eqpId, trxID, robotName, semiCmd, cellSpecial_SemiCmd);

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        //20160121 add cell special semi command
        private void CheckSemiCmdCondition(string eqpID, string trxID, string robotName, RobotCmdInfo semiCmd, CellSpecialRobotCmdInfo cellSpecial_SemiCmd)
        {
            string strlog = string.Empty;
            List<RobotStage> curRobotStages = null;

            try
            {

                #region [ 取得Robot 物件 ]

                Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(robotName);

                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Can't find Robot by RobotName({2}) in RobotEntity!",
                                            eqpID, trxID, robotName);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                #endregion

                #region [ Get Robot Stages by Robot Name ]

                if (curRobotStages == null)
                {
                    curRobotStages = ObjectManager.RobotStageManager.GetRobotStages(curRobot.Data.ROBOTNAME);
                    if (curRobotStages == null)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Can't find any RobotStages by RobotName({2}) in RobotStageEntity!",
                                                eqpID, trxID, robotName);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        return;
                    }
                }

                #endregion

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Semi Mode Start! ***********************************************************************************",
                                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #region [ Check Robot RunMode is SEMI ]

                if (curRobot.File.curRobotRunMode == eRobot_RunMode.AUTO_MODE)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Cur Robot({2})  Run Mode is(AUTO) Can't Handle Semi Command!",
                                            eqpID, trxID, robotName);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return;
                }

                #endregion

                #region [ Check Can Issue Robot Command ]

                if (CheckCanIssueRobotCommand(curRobot) == false)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Semi Mode End! ***********************************************************************************",
                                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return;
                }

                #endregion

                #region [ by RobotArm Qty Create Command ]

                //取得各Stage UDRQand LDRQ狀態 20160121 Modify要by Robot ArmJob QTY
                if (curRobot.Data.ARMJOBQTY == 2)
                {

                    #region [ for Cell Special 1Arm2Job ]

                    CheckRobotControlCommand_For_TypeII(curRobot, curRobotStages);

                    #region [ 確認Stage狀態是否符合 Cell Special Semi Cmd ]

                    //Check Cmd1 TargetPos StageStatus
                    if (CheckSemiCmdStageStatus(curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cellSpecial_SemiCmd.Cmd01_Command, cellSpecial_SemiCmd.Cmd01_ArmSelect, cellSpecial_SemiCmd.Cmd01_TargetPosition, cellSpecial_SemiCmd.Cmd01_TargetSlotNo) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Semi Mode End! ***********************************************************************************",
                                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        return;
                    }

                    if (cellSpecial_SemiCmd.Cmd02_Command != 0)
                    {
                        //Check Cmd2 TargetPos StageStatus
                        if (CheckSemiCmdStageStatus(curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cellSpecial_SemiCmd.Cmd02_Command, cellSpecial_SemiCmd.Cmd02_ArmSelect, cellSpecial_SemiCmd.Cmd02_TargetPosition, cellSpecial_SemiCmd.Cmd02_TargetSlotNo) == false)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Semi Mode End! ***********************************************************************************",
                                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return;
                        }
                    }

                    if (cellSpecial_SemiCmd.Cmd03_Command != 0)
                    {
                        //Check Cmd03 TargetPos StageStatus
                        if (CheckSemiCmdStageStatus(curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cellSpecial_SemiCmd.Cmd03_Command, cellSpecial_SemiCmd.Cmd03_ArmSelect, cellSpecial_SemiCmd.Cmd03_TargetPosition, cellSpecial_SemiCmd.Cmd03_TargetSlotNo) == false)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Semi Mode End! ***********************************************************************************",
                                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return;
                        }
                    }

                    if (cellSpecial_SemiCmd.Cmd04_Command != 0)
                    {
                        //Check Cmd04 TargetPos StageStatus
                        if (CheckSemiCmdStageStatus(curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, cellSpecial_SemiCmd.Cmd04_Command, cellSpecial_SemiCmd.Cmd04_ArmSelect, cellSpecial_SemiCmd.Cmd04_TargetPosition, cellSpecial_SemiCmd.Cmd04_TargetSlotNo) == false)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Semi Mode End! ***********************************************************************************",
                                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return;
                        }
                    }

                    #endregion

                    #region [ 確認目前Robot Arm上在席資訊是否符合 Cell Special Semi Cmd ]

                    //Check Cmd1 RBArm Condition
                    //if (CheckRobotArmCondition(curRobot, semiCmd.rbArmSelect_1, semiCmd.rbCmd_1) == false)
                    //{
                    //    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Semi Mode End! ***********************************************************************************",
                    //                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                    //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //    return;
                    //}

                    //if (semiCmd.rbCmd_2 != string.Empty)
                    //{
                    //    //Check Cmd2 RBArm Condition 
                    //    if (CheckRobotArmCondition_For2ndCmd(curRobot, semiCmd.rbArmSelect_2, semiCmd.rbCmd_2, semiCmd) == false)
                    //    {
                    //        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Semi Mode End! ***********************************************************************************",
                    //                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                    //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //        return;
                    //    }
                    //}

                    #endregion

                    //20160121 add Cell Special Command
                    #region [ Create 1 Arm 2 Substrate ]

                    CellSpecialRobotCmdInfo curCellSpecialRobotCommand = new CellSpecialRobotCmdInfo();

                    curCellSpecialRobotCommand.Cmd01_Command = cellSpecial_SemiCmd.Cmd01_Command;
                    curCellSpecialRobotCommand.Cmd01_ArmSelect = cellSpecial_SemiCmd.Cmd01_ArmSelect;
                    curCellSpecialRobotCommand.Cmd01_TargetPosition = cellSpecial_SemiCmd.Cmd01_TargetPosition;
                    curCellSpecialRobotCommand.Cmd01_TargetSlotNo = cellSpecial_SemiCmd.Cmd01_TargetSlotNo;

                    curCellSpecialRobotCommand.Cmd02_Command = cellSpecial_SemiCmd.Cmd02_Command;
                    curCellSpecialRobotCommand.Cmd02_ArmSelect = cellSpecial_SemiCmd.Cmd02_ArmSelect;
                    curCellSpecialRobotCommand.Cmd02_TargetPosition = cellSpecial_SemiCmd.Cmd02_TargetPosition;
                    curCellSpecialRobotCommand.Cmd02_TargetSlotNo = cellSpecial_SemiCmd.Cmd02_TargetSlotNo;

                    curCellSpecialRobotCommand.Cmd03_Command = cellSpecial_SemiCmd.Cmd02_Command;
                    curCellSpecialRobotCommand.Cmd03_ArmSelect = cellSpecial_SemiCmd.Cmd02_ArmSelect;
                    curCellSpecialRobotCommand.Cmd03_TargetPosition = cellSpecial_SemiCmd.Cmd02_TargetPosition;
                    curCellSpecialRobotCommand.Cmd03_TargetSlotNo = cellSpecial_SemiCmd.Cmd02_TargetSlotNo;

                    curCellSpecialRobotCommand.Cmd03_Command = cellSpecial_SemiCmd.Cmd03_Command;
                    curCellSpecialRobotCommand.Cmd03_ArmSelect = cellSpecial_SemiCmd.Cmd03_ArmSelect;
                    curCellSpecialRobotCommand.Cmd03_TargetPosition = cellSpecial_SemiCmd.Cmd03_TargetPosition;
                    curCellSpecialRobotCommand.Cmd03_TargetSlotNo = cellSpecial_SemiCmd.Cmd03_TargetSlotNo;

                    curCellSpecialRobotCommand.Cmd04_Command = cellSpecial_SemiCmd.Cmd04_Command;
                    curCellSpecialRobotCommand.Cmd04_ArmSelect = cellSpecial_SemiCmd.Cmd04_ArmSelect;
                    curCellSpecialRobotCommand.Cmd04_TargetPosition = cellSpecial_SemiCmd.Cmd04_TargetPosition;
                    curCellSpecialRobotCommand.Cmd04_TargetSlotNo = cellSpecial_SemiCmd.Cmd04_TargetSlotNo;

                    #endregion

                    //有1stJob Command 則下命令Send Robot Control Command                       
                    bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "CellSpecialRobotControlCommandSend", new object[] { curRobot, curCellSpecialRobotCommand });
                    //不管是否成功都得繼續往下做

                    #endregion

                }
                else
                {

                    #region [ for Normal 1Arm1Job 20160121 modify 為for GetGetPutPut ]

                    CheckRobotControlCommand_For_TypeI_ForGetGetPutPut(curRobot, curRobotStages);
                    //CheckRobotControlCommand_For_TypeI(curRobot, curRobotStages);

                    #region [ 確認Stage狀態是否符合Semi Cmd ]

                    //Check Cmd1 TargetPos StageStatus
                    if (CheckSemiCmdStageStatus(curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, semiCmd.Cmd01_Command, semiCmd.Cmd01_ArmSelect, semiCmd.Cmd01_TargetPosition, semiCmd.Cmd01_TargetSlotNo) == false)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Semi Mode End! ***********************************************************************************",
                                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        return;
                    }

                    if (semiCmd.Cmd02_Command != 0)
                    {
                        //Check Cmd2 TargetPos StageStatus
                        if (CheckSemiCmdStageStatus(curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, semiCmd.Cmd02_Command, semiCmd.Cmd02_ArmSelect, semiCmd.Cmd02_TargetPosition, semiCmd.Cmd02_TargetSlotNo) == false)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Semi Mode End! ***********************************************************************************",
                                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return;
                        }
                    }

                    #endregion

                    #region [ 確認目前Robot Arm上在席資訊是否符合Semi Cmd ]

                    //Check Cmd1 RBArm Condition
                    //if (CheckRobotArmCondition(curRobot, semiCmd.rbArmSelect_1, semiCmd.rbCmd_1) == false)
                    //{
                    //    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Semi Mode End! ***********************************************************************************",
                    //                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                    //    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //    return;
                    //}

                    //if (semiCmd.rbCmd_2 != string.Empty)
                    //{
                    //    //Check Cmd2 RBArm Condition 
                    //    if (CheckRobotArmCondition_For2ndCmd(curRobot, semiCmd.rbArmSelect_2, semiCmd.rbCmd_2, semiCmd) == false)
                    //    {
                    //        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Semi Mode End! ***********************************************************************************",
                    //                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                    //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //        return;
                    //    }
                    //}

                    #endregion

                    #region [ Create 1 Arm 1 Substrate ]

                    RobotCmdInfo curRobotCommand = new RobotCmdInfo();

                    curRobotCommand.Cmd01_Command = semiCmd.Cmd01_Command;
                    curRobotCommand.Cmd01_ArmSelect = semiCmd.Cmd01_ArmSelect;
                    curRobotCommand.Cmd01_TargetPosition = semiCmd.Cmd01_TargetPosition;
                    curRobotCommand.Cmd01_TargetSlotNo = semiCmd.Cmd01_TargetSlotNo;

                    curRobotCommand.Cmd02_Command = semiCmd.Cmd02_Command;
                    curRobotCommand.Cmd02_ArmSelect = semiCmd.Cmd02_ArmSelect;
                    curRobotCommand.Cmd02_TargetPosition = semiCmd.Cmd02_TargetPosition;
                    curRobotCommand.Cmd02_TargetSlotNo = semiCmd.Cmd02_TargetSlotNo;

                    #endregion

                    //Send Robot Control Command
                    bool sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curRobotCommand });
                    //不管是否成功都得繼續往下做

                    #endregion

                }

                #endregion

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Semi Mode End! ***********************************************************************************",
                                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);


            }

        }

        private bool CheckSemiCmdStageStatus(string eqpID, string robotName, int cmdAction, int cmdUseArm, int cmdTargetStageID, int cmdTargetSlotNo)
        {
            string strlog = string.Empty;
            //20160720
            string trxID = string.Empty;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
            string up_SlotNumber01 = "0";
            string up_SlotNumber02 = "0";
            string up_SlotNumber03 = "0";
            string up_SlotNumber04 = "0";
            string up_SlotNumber05 = "0";
            string up_SlotNumber06 = "0";
            string down_SlotNumber01 = "0";
            string down_SlotNumber02 = "0";
            string down_SlotNumber03 = "0";
            string down_SlotNumber04 = "0";
            string down_SlotNumber05 = "0";
            string down_SlotNumber06 = "0";
            string strSlotNoBin = string.Empty;
            int slotNo = 0;

            try
            {

                #region [ Get TargetPos StageInfo ]

                RobotStage targetStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(cmdTargetStageID.ToString().PadLeft(2, '0'));

                if (targetStage == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Can't find RobotStage by TargetStageID({2}) in RobotStageEntity!", eqpID, robotName, cmdTargetStageID.ToString());
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return false;
                }

                #endregion

                string cmdActionCode = GetRobotCommandActionDesc(cmdAction);
                string armSelect = GetRobotCommandUseArmDesc(cmdUseArm,false);

                switch (cmdActionCode)
                {
                case eRobot_DB_CommandAction.ACTION_GET:
                case eRobot_DB_CommandAction.ACTION_MULTI_GET:

                    #region [ Check GET Cmd Condition ]

                    #region [ Check Stage Status ]

                    if (targetStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY || targetStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) SemiCmd({2}) ArmSelect({3}) TargetPos({4}) TargetSlotNo({5}) and RobotStageID({6}) StageName({7}) Status({8}) Check is success!",
                                                eqpID, robotName, cmdActionCode, armSelect,
                                                cmdTargetStageID, cmdTargetSlotNo, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                targetStage.File.CurStageStatus);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        #region[CF MQC Type需判斷Link Signal SlotNo與User下的SlotNo一致]
                        //20160720
                        if (targetStage.Data.STAGEID.Trim() == "12" && (Workbench.LineType == eLineType.CF.FCMQC_TYPE1 || Workbench.LineType == eLineType.CF.FCMQC_TYPE2))
                        {
                            trxID = targetStage.Data.UPSTREAMPATHTRXNAME.Trim();
                            Trx upStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                            Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(robotName);
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {

                                if (curRobot == null)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot ({2}) RunMode Change Fail ! Can't find Robot by RobotName ({3}) in RobotEntity!",
                                                            "L2", trxID, robotName, robotName);

                                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    return false;
                                }
                            }
                            #endregion

                            if (upStream_Trx == null)
                            {

                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                            targetStage.Data.NODENO, curRobot.Data.ROBOTNAME, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                            trxID);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                return false;
                            }
                            #region CF
                            up_SlotNumber01 = upStream_Trx.EventGroups[0].Events[0].Items[10].Value;
                            up_SlotNumber02 = upStream_Trx.EventGroups[0].Events[0].Items[11].Value;
                            up_SlotNumber03 = upStream_Trx.EventGroups[0].Events[0].Items[12].Value;
                            up_SlotNumber04 = upStream_Trx.EventGroups[0].Events[0].Items[13].Value;
                            up_SlotNumber05 = upStream_Trx.EventGroups[0].Events[0].Items[14].Value;
                            up_SlotNumber06 = upStream_Trx.EventGroups[0].Events[0].Items[15].Value;
                            #endregion
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
                            //fail_ReasonCode = eRobot_CheckFail_Reason.UPSTREAM_SLOTNO_IS_MISMATCH;
                            fail_ReasonCode = string.Format("{0}_{1}_{2}", eRobot_CheckFail_Reason.UPSTREAM_SLOTNO_IS_MISMATCH, slotNo.ToString(), cmdTargetSlotNo.ToString());
                            if (slotNo != cmdTargetSlotNo)
                            {                              
                                if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) SlotNo({5}) and cmdTargetSlotNo({6}) are not same!",
                                                            targetStage.Data.NODENO, curRobot.Data.ROBOTNAME, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                            trxID, slotNo.ToString(), cmdTargetSlotNo.ToString());
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    failMsg = string.Format("RtnCode({0}) RtnMsg([{1}] StageID({2}) StageName({3}) TrxID({4}) SlotNo({5}) and cmdTargetSlotNo({6}) are not same!)",
                                                            fail_ReasonCode, MethodBase.GetCurrentMethod().Name, targetStage.Data.STAGEID, targetStage.Data.STAGENAME, trxID,
                                                            slotNo.ToString(), cmdTargetSlotNo.ToString());
                                    AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                }

                                return false;
                            }
                            else
                            {
                                RemoveRobotAllCheckFailMsg(curRobot);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) SemiCmd({2}) ArmSelect({3}) TargetPos({4}) TargetSlotNo({5}) But RobotStageID({6}) StageName({7}) Status({8}) Check is fail!",
                                                eqpID, robotName, cmdActionCode, armSelect,
                                                cmdTargetStageID, cmdTargetSlotNo, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                targetStage.File.CurStageStatus);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        return false;
                    }

                    #endregion

                    #region [ Check Slot Status Exist ]

                    //[ Wair_Proc_00037 ] Check Stage Slot Exist/NoExist by Now Stage Info
                    //if (CheckSemiCmdStageSlotExist(eqpID, robotName, targetStage, cmdTargetSlotNo, true) == true)
                    //{
                    return true;
                //}
                //else
                //{
                //    return false;
                //}

                    #endregion

                    #endregion

                case eRobot_DB_CommandAction.ACTION_PUT:
                case eRobot_DB_CommandAction.ACTION_MULTI_PUT:
                case eRobot_DB_CommandAction.ACTION_RTC_PUT:

                    #region [ Check PUT Cmd Condition ]

                    #region [ Check Stage Status ]
                    if (targetStage.File.CurStageStatus == eRobotStageStatus.RECEIVE_READY || targetStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) SemiCmd({2}) ArmSelect({3}) TargetPos({4}) TargetSlotNo({5}) and RobotStageID({6}) StageName({7}) Status({8}) Check is success!",
                                                eqpID, robotName, cmdActionCode, armSelect,
                                                cmdTargetStageID, cmdTargetSlotNo, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                targetStage.File.CurStageStatus);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        #region[CF MQC Type需判斷Link Signal SlotNo與User下的SlotNo一致]
                        //20160720
                        if (targetStage.Data.STAGEID.Trim() == "12" && (Workbench.LineType == eLineType.CF.FCMQC_TYPE1 || Workbench.LineType == eLineType.CF.FCMQC_TYPE2))
                        {
                            trxID = targetStage.Data.DOWNSTREAMPATHTRXNAME.Trim();
                            Trx downStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                            Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(robotName);
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {

                                if (curRobot == null)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot ({2}) RunMode Change Fail ! Can't find Robot by RobotName ({3}) in RobotEntity!",
                                                            "L2", trxID, robotName, robotName);

                                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    return false;
                                }
                            }
                            #endregion
                            if (downStream_Trx == null)
                            {

                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                            targetStage.Data.NODENO, curRobot.Data.ROBOTNAME, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                            trxID);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                return false;
                            }
                            #region CF
                            down_SlotNumber01 = downStream_Trx.EventGroups[0].Events[0].Items[12].Value;
                            down_SlotNumber02 = downStream_Trx.EventGroups[0].Events[0].Items[13].Value;
                            down_SlotNumber03 = downStream_Trx.EventGroups[0].Events[0].Items[14].Value;
                            down_SlotNumber04 = downStream_Trx.EventGroups[0].Events[0].Items[15].Value;
                            down_SlotNumber05 = downStream_Trx.EventGroups[0].Events[0].Items[16].Value;
                            down_SlotNumber06 = downStream_Trx.EventGroups[0].Events[0].Items[17].Value;
                            #endregion
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
                            //fail_ReasonCode = eRobot_CheckFail_Reason.DOWNSTREAM_SLOTNO_IS_MISMATCH;
                            fail_ReasonCode = string.Format("{0}_{1}_{2}", eRobot_CheckFail_Reason.DOWNSTREAM_SLOTNO_IS_MISMATCH, slotNo.ToString(), cmdTargetSlotNo.ToString());
                            if (slotNo != cmdTargetSlotNo)
                            {

                                if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) SlotNo({5}) and cmdTargetSlotNo({6}) are not same!",
                                                            targetStage.Data.NODENO, curRobot.Data.ROBOTNAME, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                            trxID,slotNo.ToString(),cmdTargetSlotNo.ToString());
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    failMsg = string.Format("RtnCode({0}) RtnMsg([{1}] StageID({2}) StageName({3}) TrxID({4}) SlotNo({5}) and cmdTargetSlotNo({6}) are not same!)",
                                                            fail_ReasonCode, MethodBase.GetCurrentMethod().Name, targetStage.Data.STAGEID, targetStage.Data.STAGENAME, trxID,
                                                            slotNo.ToString(), cmdTargetSlotNo.ToString());
                                    AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                }

                                return false;
                            }
                            else
                            {
                                RemoveRobotAllCheckFailMsg(curRobot);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) SemiCmd({2}) ArmSelect({3}) TargetPos({4}) TargetSlotNo({5}) But RobotStageID({6}) StageName({7}) Status({8}) Check is fail!",
                                                eqpID, robotName, cmdActionCode, armSelect,
                                                cmdTargetStageID, cmdTargetSlotNo, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                targetStage.File.CurStageStatus);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        return false;
                    }
                    #endregion

                    #region [ Check Slot Status Empty ]

                    //[ Wair_Proc_00037 ] Check Stage Slot Exist/NoExist by Now Stage Info
                    //if (CheckSemiCmdStageSlotExist(eqpID, robotName, targetStage, cmdTargetSlotNo, false) == true)
                    //{
                    return true;
                //}
                //else
                //{
                //    return false;
                //}

                    #endregion

                    #endregion

                case eRobot_DB_CommandAction.ACTION_EXCHANGE:
                case eRobot_DB_CommandAction.ACTION_GETPUT: //GlobalAssemblyVersion v1.0.0.26-20151028

                    #region [ Check Exchange Cmd Condition ]

                    #region [ Check Stage Status ]

                    if (targetStage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) SemiCmd({2}) ArmSelect({3}) TargetPos({4}) TargetSlotNo({5}) and RobotStageID({6}) StageName({7}) Status({8}) Check is success!",
                                                eqpID, robotName, cmdActionCode, armSelect,
                                                cmdTargetStageID, cmdTargetSlotNo, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                targetStage.File.CurStageStatus);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region[CF MQC Type需判斷Link Signal SlotNo與User下的SlotNo一致]
                        //20160720
                        if (targetStage.Data.STAGEID.Trim() == "12" && (Workbench.LineType == eLineType.CF.FCMQC_TYPE1 || Workbench.LineType == eLineType.CF.FCMQC_TYPE2))
                        {
                            trxID = targetStage.Data.UPSTREAMPATHTRXNAME.Trim();
                            Trx upStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                            Robot curRobot = ObjectManager.RobotManager.GetRobotByRobotName(robotName);
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {

                                if (curRobot == null)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Robot ({2}) RunMode Change Fail ! Can't find Robot by RobotName ({3}) in RobotEntity!",
                                                            "L2", trxID, robotName, robotName);

                                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    return false;
                                }
                            }
                            #endregion
                            if (upStream_Trx == null)
                            {

                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                            targetStage.Data.NODENO, curRobot.Data.ROBOTNAME, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                            trxID);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                return false;
                            }
                            #region CF
                            up_SlotNumber01 = upStream_Trx.EventGroups[0].Events[0].Items[10].Value;
                            up_SlotNumber02 = upStream_Trx.EventGroups[0].Events[0].Items[11].Value;
                            up_SlotNumber03 = upStream_Trx.EventGroups[0].Events[0].Items[12].Value;
                            up_SlotNumber04 = upStream_Trx.EventGroups[0].Events[0].Items[13].Value;
                            up_SlotNumber05 = upStream_Trx.EventGroups[0].Events[0].Items[14].Value;
                            up_SlotNumber06 = upStream_Trx.EventGroups[0].Events[0].Items[15].Value;
                            #endregion
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
                            //fail_ReasonCode = eRobot_CheckFail_Reason.UPSTREAM_SLOTNO_IS_MISMATCH;
                            fail_ReasonCode = string.Format("{0}_{1}_{2}", eRobot_CheckFail_Reason.UPSTREAM_SLOTNO_IS_MISMATCH, slotNo.ToString(), cmdTargetSlotNo.ToString());
                            if (slotNo != cmdTargetSlotNo)
                            {
                                if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) SlotNo({5}) and cmdTargetSlotNo({6}) are not same!",
                                                            targetStage.Data.NODENO, curRobot.Data.ROBOTNAME, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                            trxID, slotNo.ToString(), cmdTargetSlotNo.ToString());
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    failMsg = string.Format("RtnCode({0}) RtnMsg([{1}] StageID({2}) StageName({3}) TrxID({4}) SlotNo({5}) and cmdTargetSlotNo({6}) are not same!)",
                                                            fail_ReasonCode, MethodBase.GetCurrentMethod().Name, targetStage.Data.STAGEID, targetStage.Data.STAGENAME, trxID,
                                                            slotNo.ToString(), cmdTargetSlotNo.ToString());
                                    AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                                }

                                return false;
                            }
                            else
                            {
                                RemoveRobotAllCheckFailMsg(curRobot);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) SemiCmd({2}) ArmSelect({3}) TargetPos({4}) TargetSlotNo({5}) But RobotStageID({6}) StageName({7}) Status({8}) Check is fail!",
                                                eqpID, robotName, cmdActionCode, armSelect,
                                                cmdTargetStageID, cmdTargetSlotNo, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                                targetStage.File.CurStageStatus);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        return false;
                    }

                    #endregion

                    #region [ Check Slot Status Exist ]

                    //[ Wair_Proc_00037 ] Check Stage Slot Exist/NoExist by Now Stage Info
                    //if (CheckSemiCmdStageSlotExist(eqpID, robotName, targetStage, cmdTargetSlotNo, true) == true)
                    //{
                    return true;
                //}
                //else
                //{
                //    return false;
                //}

                    #endregion

                    #endregion

                case eRobot_DB_CommandAction.ACTION_GETREADY:
                case eRobot_DB_CommandAction.ACTION_PUTREADY:

                    #region [ Check Get/PUT Ready Cmd Condition ]

                    //Get/PUT Ready 不看StageStatus always回OK
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) SemiCmd({2}) ArmSelect({3}) TargetPos({4}) TargetSlotNo({5}) and RobotStageID({6}) StageName({7}) Status({8}) Check is success!",
                                            eqpID, robotName, cmdActionCode, armSelect,
                                            cmdTargetStageID, cmdTargetSlotNo, targetStage.Data.STAGEID, targetStage.Data.STAGENAME,
                                            targetStage.File.CurStageStatus);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return true;

                    #endregion

                default:

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Set Semi Cmd({2}) is Error!",
                                            eqpID, robotName, cmdActionCode);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return false;
                }

            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        private bool CheckSemiCmdStageSlotExist(string eqpID, string robotName, RobotStage targetStage, int targetSlotNo, bool checkExistFlag)
        {

            //string tmpSlotNo = targetSlotNo.PadLeft(2, '0');
            string strlog = string.Empty;
            string trxID = string.Empty;

            try
            {
                //[ Wair_Proc_00037 ] Check Stage Slot Exist/NoExist by Now Stage Info
                //if (targetStage.Data.STAGETYPE != eRobotStageType.PORT)
                //{
                //    //非PortStageTtpe 在確認UDRQ與LDRQ時已經要確保有無Exist資訊
                //    return true;
                //}

                //if (checkExistFlag == true)
                //{
                //    //Check Slot must Glass Exist
                //    return CheckSemiCmdPortStageSlotExist(eqpID, robotName, targetStage, targetSlotNo);
                //}
                //else
                //{
                //    //Check Slot must Glass NoExist
                //    return CheckSemiCmdPortStageSlotNoExist(eqpID, robotName, targetStage, targetSlotNo);
                //}

                return false;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        #endregion


        /// <summary> 取得各Stage Type LDRQ時的可用的SlotNo
        /// 
        /// </summary>
        /// <param name="curLDRQStage"></param>
        /// <returns></returns>
        private int GetLDRQStageEmptySlotNo(Robot curRobot, Job curBcsJob, int checkStepNo, RobotStage curLDRQStage, int cur1stCmdEmptySlotNo, bool findFromSlotNoFlag)
        {
            string strlog = string.Empty;
            int tmpFromSlotNo = 0;
            string tmpLog = string.Empty;
            bool isBothPortFlag = false;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;
            bool _isPutReady = false;

            try
            {
                //20151026 add Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_E0012 ] ,以ObjectName與MethodName為Key來決定是否紀錄Log
                fail_ReasonCode = string.Format("{0}_{1}", "RobotCoreService", "GetLDRQStageEmptySlotNo");

                #region [ Check 是否為Both Port ]

                if (curLDRQStage.Data.STAGETYPE == eRobotStageType.PORT)
                {
                    Port curPort = ObjectManager.PortManager.GetPort(curLDRQStage.Data.STAGEID);

                    if (curPort == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageNo({2}) RobotStageName({3}) can not get Port Entity!",
                                                                    curLDRQStage.Data.NODENO, curRobot.Data.ROBOTNAME, curLDRQStage.Data.STAGEID, curLDRQStage.Data.STAGENAME);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return tmpFromSlotNo;
                    }

                    if (curPort.File.Type == ePortType.BothPort)
                    {
                        isBothPortFlag = true;

                    }
                }

                #endregion

                #region [ 如果是Both Port要確認SourceSlot是否被占用 ]

                if (isBothPortFlag == true)
                {
                    int.TryParse(curBcsJob.FromSlotNo, out tmpFromSlotNo);

                    if (curLDRQStage.curLDRQ_EmptySlotList.ContainsKey(tmpFromSlotNo) == false)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) FromSlotNo({4}) can not find Empty SlotNo!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, tmpFromSlotNo);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //找不到如果找另外Log 則要特別處理
                        tmpLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) FromSlotNo({4}) can not find Empty SlotNo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, tmpFromSlotNo);

                    }
                    else if (curLDRQStage.curLDRQ_EmptySlotList[tmpFromSlotNo].Trim() == string.Empty)// && tmpFromSlotNo != cur1stCmdEmptySlotNo) //需在確認
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) CheckStepNo({5}) Action({6}) Get LDRQ StageID({7}) empty SlotNo({8}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID,
                                                    tmpFromSlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //Clear[ Robot_Fail_Case_E0012 ]
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                        return tmpFromSlotNo;
                    }
                    else
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) CheckStepNo({5}) Action({6}) Get LDRQ StageID({7}) empty Slot, But SlotNo({8}) JobKey({9}) is not Empty!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID,
                                                    tmpFromSlotNo.ToString(), curLDRQStage.curLDRQ_EmptySlotList[tmpFromSlotNo].Trim());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //找不到如果找另外Log 則要特別處理
                        tmpLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) CheckStepNo({5}) Action({6}) Get LDRQ StageID({7}) empty Slot, But SlotNo({8}) JobKey({9}) is not Empty!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID,
                                                    tmpFromSlotNo.ToString(), curLDRQStage.curLDRQ_EmptySlotList[tmpFromSlotNo].Trim());
                    }

                    //20151026 add BothPort此時找不到回原Slot 看是要記錄Error不可抽選其他Slot還是可選其他SlotNo!
                    bool canTargetAnotherSlotFlag = false;

                    try
                    {
                        canTargetAnotherSlotFlag = ParameterManager[eRobotCommonConst.BOTHPORTJOB_CAN_STORETO_NOTSOURCESLOTNO_CONSTANT_KEY].GetBoolean();
                    }
                    catch (Exception ex1)
                    {
                        //有問題則視為不啟用
                    }

                    if (canTargetAnotherSlotFlag == false)
                    {
                        //20151026 add BothPort此時找不到回原Slot 則要記錄Error不可抽選其他Slot!
                        #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0012 ]

                        if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                        {

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", tmpLog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) FromSlotNo({4}) can not find Empty SlotNo!",
                            //                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, tmpFromSlotNo);

                            failMsg = string.Format("Job({0}_{1}) FromSlotNo({2}) can not find Empty SlotNo!",
                                                  curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, tmpFromSlotNo);

                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion

                        }

                        #endregion

                        return 0;
                    }

                }

                #endregion

                #region [ BothPort選擇另一個空的SlotNo放片 or Other Stage找尋空的SlotNo放片 ]

                int curLDRQStageID = int.Parse(curLDRQStage.Data.STAGEID);
                foreach (int curSlotNo in curLDRQStage.curLDRQ_EmptySlotList.Keys)
                {
                    //判斷有空的Slot 且沒有被1st Cmd占用
                    if (curLDRQStage.curLDRQ_EmptySlotList[curSlotNo].Trim() == string.Empty && curSlotNo != cur1stCmdEmptySlotNo)
                    {
                        if (isBothPortFlag == true)
                        {
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", tmpLog);
                        }

                        if (curRobot.Context != null && curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam] is TCOVN_PL_ITO_RobotParam)
                        {
                            TCOVN_PL_ITO_RobotParam tmp = (TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam];
                            if ((tmp.Job01_Cmd01 != null && tmp.Job01_Cmd01.Cmd01_Command == eRobot_ControlCommand.PUT && curLDRQStageID == tmp.Job01_Cmd01.Cmd01_TargetPosition && curSlotNo == tmp.Job01_Cmd01.Cmd01_TargetSlotNo) ||
                                (tmp.Job01_Cmd02 != null && tmp.Job01_Cmd02.Cmd01_Command == eRobot_ControlCommand.PUT && curLDRQStageID == tmp.Job01_Cmd02.Cmd01_TargetPosition && curSlotNo == tmp.Job01_Cmd02.Cmd01_TargetSlotNo) ||
                                (tmp.Job02_Cmd01 != null && tmp.Job02_Cmd01.Cmd01_Command == eRobot_ControlCommand.PUT && curLDRQStageID == tmp.Job02_Cmd01.Cmd01_TargetPosition && curSlotNo == tmp.Job02_Cmd01.Cmd01_TargetSlotNo) ||
                                (tmp.Job02_Cmd02 != null && tmp.Job02_Cmd02.Cmd01_Command == eRobot_ControlCommand.PUT && curLDRQStageID == tmp.Job02_Cmd02.Cmd01_TargetPosition && curSlotNo == tmp.Job02_Cmd02.Cmd01_TargetSlotNo))
                            {
                                // OVN PL ITO 時的 Both Arm Put 或 Put Put
                                // 不可使用相同的 Target Slot No
                                continue;
                            }
                        }

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) CheckStepNo({5}) Action({6}) Get LDRQ StageID({7}) empty SlotNo({8}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID,
                                                    curSlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //Clear[ Robot_Fail_Case_E0012 ]
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                        return curSlotNo;
                    }
                    else
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) CheckStepNo({5}) Action({6}) Get LDRQ StageID({7}) empty Slot, But SlotNo({8}) JobKey({9}) is not Empty!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID,
                                                    curSlotNo.ToString(), curLDRQStage.curLDRQ_EmptySlotList[curSlotNo].Trim());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                    }
                }

                #endregion

                if (curLDRQStage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y") _isPutReady = true;

                #region[DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) CheckStepNo({5}) Action({6}) Get LDRQ StageID({7}) empty Slot Fail!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                            curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                //20151026 add BothPort此時找不到回原Slot 則要記錄Error不可抽選其他Slot!
                #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0012 ]

                if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) && !_isPutReady)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) CheckStepNo({5}) Action({6}) Get LDRQ StageID({7}) empty Slot Fail!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                                            curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", tmpLog);

                    #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                    //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) StageID({3}) CheckStepNo({4}) Action({5}) Get LDRQ StageID({6}) empty Slot Fail!",
                    //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                    //                        curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                    //                        curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID);

                    failMsg = string.Format("Job({0}_{1}) StageID({2}) CheckStepNo({3}) Action({4}) Get LDRQ StageID({5}) empty Slot Fail!",
                        curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        curBcsJob.RobotWIP.CurLocation_StageID, checkStepNo.ToString(),
                        curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION, curLDRQStage.Data.STAGEID);

                    AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                    #endregion

                }

                #endregion

                return 0;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return 0;
            }

        }

        /// <summary> Step 執行完畢時要做的Function List
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        public void HandleJobProcessResult(Robot curRobot, Job curBcsJob, string curLoadArmNo, string curUnloadArmNo, string curUnloadTargetStageID, string curUnloadTargetSlotNo)
        {
            string strlog = string.Empty;
            IRobotContext robotConText = new RobotContext();
            string fail_ReasonCode = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;

            try
            {
                List<RobotRouteResultHandle> curRouteResultHoadleList = ObjectManager.RobotManager.GetRouteResultHandle(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo);

                #region [Check CurStep All Route Result Handle Condition ]

                #region [ Initial Route Result Handle Rule List RobotConText Info. 搭配針對File Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);
                robotConText.AddParameter(eRobotContextParameter.LoadJobArmNo_For_1Arm_1Job, curLoadArmNo);
                robotConText.AddParameter(eRobotContextParameter.UnloadJobArmNo_For_1Arm_1Job, curUnloadArmNo);
                robotConText.AddParameter(eRobotContextParameter.TargetStageID, curUnloadTargetStageID);
                robotConText.AddParameter(eRobotContextParameter.TargetSlotNo, curUnloadTargetSlotNo);

                #endregion =======================================================================================================================================================

                foreach (RobotRouteResultHandle curRouteResultHandleCondition in curRouteResultHoadleList)
                {
                    //Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_E0006 ] ,以Rule Job Filter 的ObjectName與MethodName為Key來決定是否紀錄Log
                    //因為會出現同Job 確認不同Step所以FailCode要補上StepNo
                    fail_ReasonCode = string.Format("{0}_{1}_{2}", curRouteResultHandleCondition.Data.OBJECTNAME, curRouteResultHandleCondition.Data.METHODNAME, curBcsJob.RobotWIP.CurStepNo.ToString());

                    #region[DebugLog][ Start Rule Job Route Result Handle Function ]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Route Result Handle object({4}) MethodName({5}) IsEnable({6}) Start {7}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curRouteResultHandleCondition.Data.OBJECTNAME, curRouteResultHandleCondition.Data.METHODNAME, curRouteResultHandleCondition.Data.ISENABLED,
                                                new string(eRobotCommonConst.RULE_FILTER_START_CHAR, eRobotCommonConst.RULE_FILTER_START_CHAR_LENGTH));

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    if (curRouteResultHandleCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                    {

                        checkFlag = (bool)Invoke(curRouteResultHandleCondition.Data.OBJECTNAME, curRouteResultHandleCondition.Data.METHODNAME, new object[] { robotConText });

                        #region Log Invoke Result Handle
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Invoke Result Handle object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteResultHandleCondition.Data.OBJECTNAME, curRouteResultHandleCondition.Data.METHODNAME,
                                                        curRouteResultHandleCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_FILTER_END_CHAR, eRobotCommonConst.RULE_FILTER_END_CHAR_LENGTH));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        #endregion

                        if (checkFlag == false)
                        {

                            #region[DebugLog][ End Rule Job Filter Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Route Result Handle Fail, object({4}) MethodName({5}) RtnCode({4})  RtnMsg({6}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteResultHandleCondition.Data.OBJECTNAME, curRouteResultHandleCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Route Result Handle object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteResultHandleCondition.Data.OBJECTNAME, curRouteResultHandleCondition.Data.METHODNAME,
                                                        curRouteResultHandleCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_FILTER_END_CHAR, eRobotCommonConst.RULE_FILTER_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0006 ]

                            if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Route Result Handle Fail, object({4}) MethodName({5}) RtnCode({6})  RtnMsg({7}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteResultHandleCondition.Data.OBJECTNAME, curRouteResultHandleCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6}]!",
                                //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRouteResultHandleCondition.Data.OBJECTNAME,
                                //                        curRouteResultHandleCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                failMsg = string.Format("Job({0}_{1}) RtnCode({2})  RtnMsg({3}]!",
                                         curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion

                            }

                            #endregion

                            //有重大異常直接結束Route Result Handle邏輯回復NG
                            return;

                        }
                        else
                        {

                            //Clear[ Robot_Fail_Case_E0006 ]
                            RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                            #region[DebugLog][ End Rule Job Filter Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Route Result Handle object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteResultHandleCondition.Data.OBJECTNAME, curRouteResultHandleCondition.Data.METHODNAME,
                                                        curRouteResultHandleCondition.Data.ISENABLED,
                                                        new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                        }
                    }
                    else
                    {

                        #region[DebugLog][ End Rule Job Filter Function ]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Route Result Handle object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curRouteResultHandleCondition.Data.OBJECTNAME, curRouteResultHandleCondition.Data.METHODNAME, curRouteResultHandleCondition.Data.ISENABLED,
                                                    new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                    }

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary>當下Robot Command時如果是Port Stage 取片且是WaitForProcess時則要確認First Glass Check = "Y" 才可出片
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkDefineCmd"></param>
        /// <param name="is2ndCmdFlag"></param>
        /// <returns></returns>
        private bool PortFetchOut_FirstGlassCheck(Robot curRobot, Job curBcsJob, DefineNormalRobotCmd checkDefineCmd, bool is2ndCmdFlag)
        {
            string strlog = string.Empty;
            string funcName = string.Empty;

            try
            {

                if (is2ndCmdFlag == true)
                {
                    funcName = "2nd Command";
                }
                else
                {
                    funcName = "1st Command";
                }

                #region [ Check Command Action ]

                if (checkDefineCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET)
                {

                    #region [ Get TargetPosition Stage Entity ]

                    RobotStage curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(checkDefineCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get RobotStageInfo by ({2}) TargetPosition({3})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, funcName, checkDefineCmd.Cmd01_TargetPosition.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    #endregion

                    //Get Port Type 需要Check First Glass Check 
                    if (curStage.Data.STAGETYPE == eRobotStageType.PORT)
                    {

                        #region [ Get Port Entity by StageID , 如果找不到則回NG ]

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

                            return false;
                        }

                        #endregion

                        #region [ Get CST Entity by Job's CST Seq ]
                        int curCstSeq = 0;
                        int.TryParse(curBcsJob.CassetteSequenceNo, out curCstSeq);
                        Cassette curCST = ObjectManager.CassetteManager.GetCassette(curCstSeq);

                        //找不到 CST 回NG
                        if (curCST == null)
                        {

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get CST Entity by Job CstSeq({2})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        #endregion

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

                        #region [ Check First Glass Condition ] 20151024 modify 改為抓Stage Keep的FirstGlassCheck 的值以避免突然跳片問題

                        //if (curCST.FirstGlassCheckReport == "C2" || curCST.FirstGlassCheckReport == "N")                      
                        if (curStage.File.CstFirstGlassCheckResult == "C2" || curStage.File.CstFirstGlassCheckResult == "N")
                        {
                            //C2:before fetch glass from cst, invoke MES.LotProcessStartRequest
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ({4}) is GET StageID({5}) but CSTID({6}) First Glass Check Report({7}) can not Fetch Out!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        funcName, curStage.Data.STAGEID, curPort.File.CassetteID, curStage.File.CstFirstGlassCheckResult);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;

                        }
                        //else if (curCST.FirstGlassCheckReport == "Y")
                        else if (curStage.File.CstFirstGlassCheckResult == "Y")
                        {
                            // Y:OK, Robort can start fetch glass from cst
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) CstID({3}) First Glass Check Report({4}).",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, curPort.File.CassetteID,
                                                        curStage.File.CstFirstGlassCheckResult);

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return true;
                        }
                        else
                        {
                            //尚未Send First Glass Check 必須要透過準備取第一片時再做FirstGlass Check 所以要回NG
                            //Invoke MESService First Glass Check
                            string trxID = UtilityMethod.GetAgentTrackKey();

                            //LotProcessStartRequest(string trxID, Port port, Cassette cst, Job job)
                            Invoke(eServiceName.MESService, "LotProcessStartRequest", new object[] { trxID, curPort, curCST, curBcsJob });

                            //Offline 情况下BC默认给空值，check 不通过无法出片 20161101 Added by zhangwei
                            //if (curCST.FirstGlassCheckReport == "Y")
                            //    return true;
                            //marked by yang 2017/4/12 这边不要判断,防止跳片

                            if (curStage.Data.LINEID == "TCCVD100" || curStage.Data.LINEID == "TCCVD200") //add by qiumin 20180917 CVD100 FIRST CHECK By pass
                            {
                                return true;
                            }
                            //理論上只會送一次 所以不須用debug
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Port({2}) CstID({3}) First Glass Check Report({4}) must First Glass Check!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curPort.Data.PORTID, curPort.File.CassetteID,
                                                        curStage.File.CstFirstGlassCheckResult);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            return false;

                        }

                        #endregion

                    }
                }

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary>當下Robot Command時如果是EQP Stage 且是Multi Signal Type時如果是2片造成的GETGET or PUTPUT則要將Action 從GetGet/PutPut改成Multi-Get/Multi-Put
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkDefineCmd"></param>
        /// <param name="is2ndCmdFlag"></param>
        private void CheckMultiSingleCommandCondition(Robot curRobot, Job curBcsJob, DefineNormalRobotCmd checkDefineCmd, bool is2ndCmdFlag)
        {
            string strlog = string.Empty;
            string funcName = string.Empty;

            try
            {

                if (is2ndCmdFlag == true)
                {
                    funcName = "2nd Command";
                }
                else
                {
                    funcName = "1st Command";
                }

                #region [ Check Command Action and UseArm is Not 3(All Arm/Both Arm) ]

                if (((checkDefineCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET) ||
                    (checkDefineCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_PUT)) &&
                    checkDefineCmd.Cmd01_ArmSelect != 3)
                {

                    #region [ Get TargetPosition Stage Entity ]

                    RobotStage curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(checkDefineCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get RobotStageInfo by ({2}) TargetPosition({3})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, funcName, checkDefineCmd.Cmd01_TargetPosition.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return;
                    }

                    #endregion

                    //Get EQP Type and RobotInterfaceType為Multi才需要Check 是否變更Action 
                    if (curStage.Data.STAGETYPE == eRobotStageType.EQUIPMENT && curStage.Data.EQROBOTIFTYPE == eRobotStage_RobotInterfaceType.MULTI_SINGLE)
                    {

                        #region [ Update Command Action ]

                        if ((checkDefineCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET))
                        {

                            checkDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTI_GET;

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ({4}) Action is (GET) but StageID({5}) RobotIFType(MULTI_SINGLE) Change Action to (MULTI_GET).",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        funcName, curStage.Data.STAGEID);

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            return;

                        }

                        if ((checkDefineCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_PUT))
                        {

                            checkDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTI_PUT;

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ({4}) Action is (PUT) but StageID({5}) RobotIFType(MULTI_SINGLE) Change Action to (MULTI_PUT).",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        funcName, curStage.Data.STAGEID);

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            return;

                        }

                        #endregion

                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }

        }

        //20151102 add 
        /// <summary> 當下Robot Command時如果是2片造成的GETGET or PUTPUT且EQP Stage 且1st Command的TargetPosition是Multi Signal Type要將Action 從GetGet/PutPut改成Multi-Get/Multi-Put
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkDe1stJobCmd"></param>
        private void Check1stJob1stCommandMultiSingleCommandCondition(Robot curRobot, Job curBcsJob, RobotCmdInfo checkDe1stJobCmd)
        {
            string strlog = string.Empty;

            try
            {

                #region [ Check Command Action and UseArm is Not 3(All Arm/Both Arm) ]

                //20160511
                if (((checkDe1stJobCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET) ||
                    (checkDe1stJobCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_PUT) || 
                    checkDe1stJobCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT) &&
                    checkDe1stJobCmd.Cmd01_ArmSelect != 3)
                {

                    #region [ Get TargetPosition Stage Entity ]

                    RobotStage curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(checkDe1stJobCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                    //找不到 Robot Stage 回NG
                    if (curStage == null)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get RobotStageInfo by 1stJob 1stCommand TargetPosition({2})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, checkDe1stJobCmd.Cmd01_TargetPosition.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return;
                    }

                    #endregion

                    //Get EQP Type and RobotInterfaceType為Multi才需要Check 是否變更Action 
                    if (curStage.Data.STAGETYPE == eRobotStageType.EQUIPMENT && curStage.Data.EQROBOTIFTYPE == eRobotStage_RobotInterfaceType.MULTI_SINGLE)
                    {

                        #region [ Update Command Action ]

                        if ((checkDe1stJobCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_GET))
                        {

                            checkDe1stJobCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTI_GET;

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) 1stJob 1st Command Action is (GET) but StageID({4}) RobotIFType(MULTI_SINGLE) Change Action to (MULTI_GET).",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curStage.Data.STAGEID);

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            return;

                        }
                        //20160511
                        if ((checkDe1stJobCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_PUT) || (checkDe1stJobCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_RECIPEGROUPEND_PUT))
                        {

                            checkDe1stJobCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_MULTI_PUT;

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) 1stJob 1st Command Action is (PUT) but StageID({4}) RobotIFType(MULTI_SINGLE) Change Action to (MULTI_PUT).",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curStage.Data.STAGEID);

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            return;

                        }

                        #endregion

                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }

        }


        /// <summary>當下Robot Command時, 如果是EQP Stage 並且是Get/Put type時, 需要將 Action 從 Put 改成 Get/Put
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="chkDefineCmd"></param>
        /// <param name="is2ndCmdFlag">false=1st Command, Ture=2nd Command</param>
        private void CheckGetPutCommandCondition(Robot curRobot, Job curBcsJob, DefineNormalRobotCmd chkDefineCmd)
        {
            CheckGetPutCommandCondition(curRobot, curBcsJob, chkDefineCmd, false);
        }
        private void CheckGetPutCommandCondition(Robot curRobot, Job curBcsJob, DefineNormalRobotCmd chkDefineCmd, bool is2ndCmdFlag)
        {
            string strlog = string.Empty;
            string funcName = (!is2ndCmdFlag ? "1st Command" : "2nd Command");
            RobotStage curStage = null;

            try
            {
                #region [ Get TargetPosition Stage Entity ]
                curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(chkDefineCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                if (curStage == null) //找不到 Robot Stage 回NG
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get RobotStageInfo by ({2}) TargetPosition({3})!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, funcName, chkDefineCmd.Cmd01_TargetPosition.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return;
                }
                #endregion

                //[ Get Pre-Fetch condition and check if enable or disable for bypass the following logic ]
                bool _canUsePreFetchFlag = CheckPrefetchFlag(curRobot);

                switch (chkDefineCmd.Cmd01_Command)
                {
                case eRobot_Trx_CommandAction.ACTION_PUT:

                        // CVD can't receive job add by Yang
                   if (curRobot.Data.LINETYPE.Contains("CVD_"))
                   {
                       if (curStage.Data.REMARKS.Equals("LL1"))
                       {
                           if (Check_CVD_EQInterLock_LoadLock1CleanOutBit(curRobot) == eBitResult.ON) break;
                       }
                       else if(curStage.Data.REMARKS.Equals("LL2"))
                       {
                           if (Check_CVD_EQInterLock_LoadLock2CleanOutBit(curRobot) == eBitResult.ON) break;
                       }
                   }
                    if (chkDefineCmd.Cmd01_ArmSelect == 3) break; //All Arm or Both Arm ... break
                    if (curStage.Data.STAGETYPE != eRobotStageType.EQUIPMENT) break; //non-Equipment ... break
                    if (curStage.Data.EQROBOTIFTYPE != eRobotStage_RobotInterfaceType.NORMAL) break; //non-Normal ... break
                    if (curStage.Data.EXCHANGETYPE != eRobotStage_ExchangeType.GETPUT) break; //non-GETPUT ... break
                    if (curStage.File.CurStageStatus != eRobotStageStatus.SEND_OUT_READY) break; //non-UDRQ ... break
                    if (_canUsePreFetchFlag) break; //Pre-Fetch ... break

                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ({4}) Action is (PUT) but StageID({5}) ExchangeType(GETPUT) Change Action to (GETPUT).",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, funcName, curStage.Data.STAGEID);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    #region [ Update Command Action ]
                    chkDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_GETPUT;
                    #endregion
                    break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


        /// <summary>當下Robot Command時, 如果是EQP Stage 並且是有支持Pre-Fetch時, 需要將 Action 從 Put 改成 PutReady
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="chkDefineCmd"></param>
        /// <param name="is2ndCmdFlag">false=1st Command, Ture=2nd Command</param>
        private void CheckPutReadyCommandCondition(Robot curRobot, Job curBcsJob, DefineNormalRobotCmd chkDefineCmd01, DefineNormalRobotCmd chkDefineCmd02)
        {
            CheckPutReadyCommandCondition(curRobot, curBcsJob, chkDefineCmd01, chkDefineCmd02, false);
        }
        private void CheckPutReadyCommandCondition(Robot curRobot, Job curBcsJob, DefineNormalRobotCmd chkDefineCmd01, DefineNormalRobotCmd chkDefineCmd02, bool is2ndCmdFlag)
        {
            string strlog = string.Empty;
            string funcName = (!is2ndCmdFlag ? "1st Command" : "2nd Command");
            RobotStage curStage = null;
            string fail_ReasonCode = string.Empty;
            string fali_ReasonCode2 = string.Empty; //add by yang 20161003
            string failMsg = string.Empty;
            try
            {
                #region [ Get TargetPosition Stage Entity ]
                curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(chkDefineCmd02.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                if (curStage == null) //找不到 Robot Stage 回NG
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get RobotStageInfo by ({2}) TargetPosition({3})!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, funcName, chkDefineCmd02.Cmd01_TargetPosition.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return;
                }
                #endregion

                //[ Get Pre-Fetch & Put-Ready condition and check if enable or disable for bypass the following logic ]
                bool _canUsePreFetchFlag = CheckPrefetchFlag(curRobot);
                bool _canUsePutReadyFlag = (curStage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y" ? true : false);


                switch (chkDefineCmd02.Cmd01_Command)
                {
                  case eRobot_Trx_CommandAction.ACTION_NONE:
                  case eRobot_Trx_CommandAction.ACTION_PUT:
                    fail_ReasonCode = eJob_CheckFail_Reason.Job_PreFetch_TargetStage_Is_Cannot_Cassette_Fail;
                    fali_ReasonCode2 = eJob_CheckFail_Reason.Job_PreFetch_TargetStage_Is_Clean_Out_Fail;
                    
                    if (!_canUsePreFetchFlag)
                    {
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode); //清除fail_ReasonCode
                        break; //non-Pre-Fetch (沒有開啟Pre Fetch功能) ... break
                    }
                    //預取時,Target stage是port的話,就把預取Get Cmd清掉,不提供回CST有預取
                    if (curStage.Data.STAGETYPE == eRobotStageType.PORT)
                    {
                        if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {
                            strlog = string.Format("[{0}] Job({1}_{2})PreFetch Target Stage({3}) is can't Cassette!", MethodBase.GetCurrentMethod().Name,
                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curStage.Data.STAGEID);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                            failMsg = string.Format("RtnCode({0})  RtnMsg({1})", fail_ReasonCode, strlog);

                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);
                            #endregion
                        }
                        chkDefineCmd01.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE;
                    }
                    else
                    {
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                    }
                        //预取时,CVD由于Chamber自净,要把预取Get Cmd清掉,因为预取出来的玻璃CVD也无法做,还要带着基板去做GET From CVD,PUT To CST
                        //目前RTC也没太好处理,暂时放在这里去清掉cmd(有一个LL没有在自净,就可以预取)
                    if (curRobot.Data.LINETYPE.Contains("CVD_") && Check_CVD_EQInterLock_LoadLock1CleanOutBit(curRobot) == eBitResult.ON && Check_CVD_EQInterLock_LoadLock2CleanOutBit(curRobot) == eBitResult.ON)
                    {
                        if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fali_ReasonCode2))
                        {
                            strlog = string.Format("[{0}] Job({1}_{2})PreFetch Target Stage({3}) is clean out,can not receive!", MethodBase.GetCurrentMethod().Name,
                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curStage.Data.STAGEID);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                            failMsg = string.Format("RtnCode({0})  RtnMsg({1})", fali_ReasonCode2, strlog);

                            AddJobCheckFailMsg(curBcsJob, fali_ReasonCode2, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fali_ReasonCode2, failMsg, eSendToOPIMsgType.AlarmType);
                            #endregion
                        }

                            chkDefineCmd01.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE;
                    }
                    else
                    {
                        RemoveJobCheckFailMsg(curBcsJob, fali_ReasonCode2);
                    }
                    if (chkDefineCmd02.Cmd01_ArmSelect == 3) break; //All Arm or Both Arm ... break
                    //if (curStage.Data.STAGETYPE != eRobotStageType.EQUIPMENT) break; //non-Equipment ... break
                    if (curStage.Data.EQROBOTIFTYPE != eRobotStage_RobotInterfaceType.NORMAL) break; //non-Normal ... break
                    //if (curStage.Data.EXCHANGETYPE != eRobotStage_ExchangeType.GETPUT) break; //non-GETPUT ... break
                    //if (curStage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST) break; //non-NOREQ ... break (沒有需求才需要Put Ready!!)
                    //20160504 modify 避免stage不收片只出片,Command Action會是PUT(改為NONE,不下Command)
                    //if (curStage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && curStage.File.CurStageStatus != eRobotStageStatus.SEND_OUT_READY) break;

                    //20160727 改成用Filter有無skip來決定,SkipFilterCheck=true->表示Prefetch及PutReady,把cmd清掉,由下面決定是否把cmd變PutReady;SkipFilterCheck=false->表示沒有Prefetch及PutReady,維持cmd=Put,break掉
                    //第一次掃進CheckRobotStageJobRouteCondition_ForGetGetPutPut,check Filter都過了,會進CheckPutReadyCommandCondition,SkipFilterCheck會是false,Cmd就會直接下Put
                    //第一次掃進CheckRobotStageJobRouteCondition_ForGetGetPutPut,Filter不過,會跑進PreFetch,掃第二次CheckRobotStageJobRouteCondition_ForGetGetPutPut,
                    //接著因為有PreFetch所以SkipFilterCheck會是true,check Filter 直接跳過了,會跑進CheckPutReadyCommandCondition,因為SkipFilterCheck是true,所以Cmd會清掉,再重新判斷是否變PutReady
                    //增加Orderby skip,for CF某些line把Filter的判斷寫在Orderby裡,所以判斷到Orderby再決定要不要skip                 
                    //if (!curBcsJob.RobotWIP.SkipFilterCheck && !curBcsJob.RobotWIP.SkipOrderbyCheck) break;
                    if (!curBcsJob.RobotWIP.SkipFilterCheck) break;
                    else if (!curBcsJob.RobotWIP.SkipOrderbyCheck) break;
                    else chkDefineCmd02.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE;

                    if (!_canUsePutReadyFlag) break; //non-Put-Ready (沒有開啟Put Ready功能) ... break
                    //20160816 如果stage是CST且設定PutReady,不要真的做PutReady
                    if (curStage.Data.STAGETYPE == eRobotStageType.PORT) break;

                    //啟動條件: 前提是來源Port Stage要先有開啟 預取(Pre Fetch) 功能, 然後再來是目的EQP Stage也有開啟 Put Ready 功能才行!!
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ({4}) Action is (PUT) but StageID({5}) Pre-Fecth Change Action to (PUTREADY).",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, funcName, curStage.Data.STAGEID);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    //20160624 把在Arm上的PutReady Flag On起來,避免在Arm又下一次PutReady
                    curRobot.OnArmPutReadyFlag = 1;
                    curRobot.OnArmPutReady_StageID = chkDefineCmd02.Cmd01_TargetPosition.ToString();
                    #region [ Update Command Action ]
                    chkDefineCmd02.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_PUTREADY;
                    #endregion
                    break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //20160624
        private void CheckOnArmPutReadyCommandCondition(Robot curRobot, Job curBcsJob, DefineNormalRobotCmd chkDefineCmd)
        {
            CheckOnArmPutReadyCommandCondition(curRobot, curBcsJob, chkDefineCmd, false);
        }
        private void CheckOnArmPutReadyCommandCondition(Robot curRobot, Job curBcsJob, DefineNormalRobotCmd chkDefineCmd, bool is2ndCmdFlag)
        {
            string strlog = string.Empty;
            string funcName = (!is2ndCmdFlag ? "1st Command" : "2nd Command");
            RobotStage curStage = null;
            Line _line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
            try
            {
                #region [ Get TargetPosition Stage Entity ]
                curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(chkDefineCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                if (curStage == null) //找不到 Robot Stage 回NG
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get RobotStageInfo by ({2}) TargetPosition({3})!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, funcName, chkDefineCmd.Cmd01_TargetPosition.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return;
                }
                #endregion

                //[ Get Pre-Fetch & Put-Ready condition and check if enable or disable for bypass the following logic ]
                //bool _canUsePreFetchFlag = CheckPrefetchFlag(curRobot);
                bool _canUsePutReadyFlag = (curStage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y" ? true : false);


                switch (chkDefineCmd.Cmd01_Command)
                {
                    //case eRobot_Trx_CommandAction.ACTION_PUT:
                    //    if (chkDefineCmd.Cmd01_ArmSelect == 3) break; //All Arm or Both Arm ... break
                    //    if (curStage.Data.STAGETYPE != eRobotStageType.EQUIPMENT) break; //non-Equipment ... break
                    //    if (curStage.Data.EQROBOTIFTYPE != eRobotStage_RobotInterfaceType.NORMAL) break; //non-Normal ... break
                    //    //if (curStage.Data.EXCHANGETYPE != eRobotStage_ExchangeType.GETPUT) break; //non-GETPUT ... break
                    //    //if (curStage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST) break; //non-NOREQ ... break (沒有需求才需要Put Ready!!)
                    //    //20160504 modify 避免stage不收片只出片,Command Action會是PUT(改為NONE,不下Command)
                    //    if (curStage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && curStage.File.CurStageStatus != eRobotStageStatus.SEND_OUT_READY) break;
                    //    else chkDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE;
                    //    //if (!_canUsePreFetchFlag) break; //non-Pre-Fetch (沒有開啟Pre Fetch功能) ... break
                    //    if (!_canUsePutReadyFlag) break; //non-Put-Ready (沒有開啟Put Ready功能) ... break

                    //    //啟動條件: 前提是來源Port Stage要先有開啟 預取(Pre Fetch) 功能, 然後再來是目的EQP Stage也有開啟 Put Ready 功能才行!!
                    //    #region[DebugLog]
                    //    if (IsShowDetialLog == true)
                    //    {
                    //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ({4}) Action is (PUT) but StageID({5}) Pre-Fecth Change Action to (PUTREADY).",
                    //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, funcName, curStage.Data.STAGEID);
                    //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    //    }
                    //    #endregion
                    //    #region [ Update Command Action ]
                    //    chkDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_PUTREADY;
                    //    #endregion
                    //    break;
                    case eRobot_Trx_CommandAction.ACTION_NONE:
                    case eRobot_Trx_CommandAction.ACTION_PUT:  //20160706
                        if (chkDefineCmd.Cmd01_ArmSelect == 3) break;
                        //if (curStage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && curStage.File.CurStageStatus != eRobotStageStatus.SEND_OUT_READY) break;
                        //if (!curBcsJob.RobotWIP.SkipFilterCheck && !curBcsJob.RobotWIP.SkipOrderbyCheck) break;
                        //if (!curBcsJob.RobotWIP.SkipFilterCheck) break;
                        //else if (!curBcsJob.RobotWIP.SkipOrderbyCheck) break;
                        
                        //1.因為Job在stage上,有Prefetch的話,CheckRobotStageJobRouteCondition_ForGetGetPutPut會跑第二次,可以知道Skip的Filter跟Orderby,真正確認是跑到Prefetch的狀況
                        //2.可是Job在Arm上時,CheckRobotArmJobRouteCondition_ForGetGetPutPut只會跑一次,所以只能在Filter或Orderby失敗後,再判斷是不是要Skip Filter或Orderby,沒有完全把Prefetch狀況切開
                        //3.所以不能比照stage寫法,直接檢查SkipFilterCheck跟SkipOrderbyCheck
                        //4.有可能Skip Filter(Filter失敗,有開PutReady),可是Orderby卻是過的;或是Skip Orderby(Orderby失敗,有開PutReady),可是Filter卻是過的
                        //5.所以判斷式要多做以下判斷
                        //Filter失敗(即skip Filter)->OrderBy失敗(即skip Orderby)->結果Cmd清成ACTION_NONE,往下確認能不能變PutReady
                        //Filter失敗(即skip Filter)->Orderby成功->結果Cmd清成ACTION_NONE,往下確認能不能變PutReady
                        //OrderBy失敗(即skip Orderby)->Filter成功->結果Cmd清成ACTION_NONE,往下確認能不能變PutReady
                        //剩下的(即Filter成功,Orderby成功)->break,繼續下Put
                        if ((curBcsJob.RobotWIP.SkipFilterCheck && curBcsJob.RobotWIP.SkipOrderbyCheck) || (curBcsJob.RobotWIP.SkipFilterCheck && curBcsJob.RobotWIP.RunOrderbyCheckOK) || (curBcsJob.RobotWIP.SkipOrderbyCheck && curBcsJob.RobotWIP.RunFilterCheckOK))
                            chkDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE;
                        else
                            break;

                        //20160721 先註解掉,需要卡在Arm上,Dry line IndexOperMode=Mix 不做PutReady再打開
                        //if (_line != null && _line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE && (Workbench.LineType == eLineType.ARRAY.DRY_ICD || Workbench.LineType == eLineType.ARRAY.DRY_YAC)) break;

                        if (!_canUsePutReadyFlag) break;
                        //20160816 如果Target stage是CST且設定PutReady,則不做PutReady
                        if (curStage.Data.STAGETYPE == eRobotStageType.PORT) break;

                        //预取时,CVD由于Chamber自净,要把预取Get Cmd清掉,因为预取出来的玻璃CVD也无法做,还要带着基板去做GET From CVD,PUT To CST
                        //目前RTC也没太好处理,暂时放在这里去清掉cmd(有一个LL没有在自净,就可以预取) yang
                        if (curRobot.Data.LINETYPE.Contains("CVD_") && Check_CVD_EQInterLock_LoadLock1CleanOutBit(curRobot) == eBitResult.ON && Check_CVD_EQInterLock_LoadLock2CleanOutBit(curRobot) == eBitResult.ON)
                            break;

                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ({4}) Action is (NONE) but StageID({5}) Change Action to (PUTREADY).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, funcName, curStage.Data.STAGEID);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        #region [ Update Command Action ]
                        if (curRobot.OnArmPutReadyFlag == 0 && curRobot.OnArmPutReady_StageID == string.Empty) //在Arm上PutReady時,command只下1次,避免重複一直下PutReady
                        {
                            curRobot.OnArmPutReadyFlag = 1;
                            curRobot.OnArmPutReady_StageID = chkDefineCmd.Cmd01_TargetPosition.ToString();
                            chkDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_PUTREADY;
                        }
                        else
                        {
                            chkDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE;
                        }
                        #endregion
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>當下Robot Command時, 如果是PORT Stage 並且是Pre-Fetch出来并RTC的基板, 需要將 Action 從 Put 改成 RTC_Put
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="chkDefineCmd"></param>
        /// <param name="is2ndCmdFlag">false=1st Command, Ture=2nd Command</param>
        private void CheckRtcPutCommandCondition(Robot curRobot, Job curBcsJob, DefineNormalRobotCmd chkDefineCmd)
        {
            CheckRtcPutCommandCondition(curRobot, curBcsJob, chkDefineCmd, false);
        }
        private void CheckRtcPutCommandCondition(Robot curRobot, Job curBcsJob, DefineNormalRobotCmd chkDefineCmd, bool is2ndCmdFlag)
        {
            string strlog = string.Empty;
            string funcName = (!is2ndCmdFlag ? "1st Command" : "2nd Command");
            RobotStage curStage = null;

            try
            {
                #region [ Get TargetPosition Stage Entity ]
                curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(chkDefineCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));

                if (curStage == null) //找不到 Robot Stage 回NG
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get RobotStageInfo by ({2}) TargetPosition({3})!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, funcName, chkDefineCmd.Cmd01_TargetPosition.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return;
                }
                #endregion

                switch (chkDefineCmd.Cmd01_Command)
                {
                    case eRobot_Trx_CommandAction.ACTION_PUT:
                        if (chkDefineCmd.Cmd01_ArmSelect == 3) break; //All Arm or Both Arm ... break
                        if (curStage.Data.STAGETYPE != eRobotStageType.PORT) break; //non-Port ... break
                        if (curStage.Data.EQROBOTIFTYPE != eRobotStage_RobotInterfaceType.NORMAL) break; //non-Normal ... break
                        //20160525
                        if (!is2ndCmdFlag)
                        {
                            if (!CheckCurStepNoIsRtcOrNot(curBcsJob.RobotWIP.CurStepNo)) break; //non-RTC or non-Force RTC ... break
                        }
                        else
                        {
                            if (!CheckCurStepNoIsRtcOrNot(curBcsJob.RobotWIP.NextStepNo)) break;
                        }

                        //啟動條件: 前提是 一开始是 預取(Pre Fetch) 的基板, 然後再來是目的PORT Stage / 并且要做 RTC (91) 或 Force RTC (51) 的情况下才行!!    
                        //RTC(81)条件:下游设备不收片，上游出片先回cst暂放
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) ({4}) Action is (PUT) but StageID({5}) Pre-Fecth Change Action to (RTC_PUT)",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, funcName, curStage.Data.STAGEID);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        #region [ Update Command Action ]
                        chkDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_RTC_PUT;
                        #endregion
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private bool CheckCurStepNoIsRtcOrNot(int _step)
        {
            switch (_step)
            {
                default: return false;
                case 91: //RTC return
                case 51: //Force RTC return
                //20160525
                case 81: //EQP RTC return
                    return true;
            }
        }



        /// <summary> Get Robot Current Arm Info by Robot Arm Job Count
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        private void GetEQLastSendOnTime(Robot curRobot)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curRobotStageList = null;
            string trxID = string.Empty;

            try
            {
                #region [ Get CurRobot All Stage List ]

                curRobotStageList = ObjectManager.RobotStageManager.GetRobotStages(curRobot.Data.ROBOTNAME);

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

                    return;
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

                    return;
                }

                #endregion

                string bitOn = "1";
                string bitOff = "0";

                #region [Get line fabtyep]
                string fabtype = eFabType.ARRAY.ToString();
                Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if (line != null)
                {
                    fabtype = line.Data.FABTYPE;
                }
                #endregion

                foreach (RobotStage curStage in curRobotStageList)
                {
                    if (curStage.Data.STAGETYPE != eRobotStageType.EQUIPMENT)
                        continue;

                    if (curStage.Data.UPSTREAMPATHTRXNAME.Trim() == string.Empty)
                    {
                        continue;
                    }

                    if (line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW)
                    {
                        if (curStage.Data.STAGEID != "12")
                            continue;
                    }

                    string[] upStreamTrxList = curStage.Data.UPSTREAMPATHTRXNAME.Split(',');
                    string strSlotNoBin = string.Empty;
                    string strGlassCountBin = string.Empty;

                    for (int i = 0; i < upStreamTrxList.Length; i++)
                    {
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

                        if (up_UpstreamInline == bitOn && up_Send == bitOn)
                        {
                            if (curStage.Data.UPSTREAMJOBDATAPATHTRXNAME.Trim() == string.Empty)
                            {
                                continue;
                            }

                            //取得SendOut的TrxID
                            trxID = curStage.Data.UPSTREAMJOBDATAPATHTRXNAME.Split(',')[i].Trim();

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

                                continue;
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
                                string jobKey = string.Format("{0}_{1}", cstSeq.ToString(), jobSeq.ToString());

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

                                    continue; //Not In WIP
                                }

                                #endregion

                                #region [ Update Job LastSendOnTime, LastSendStageID ]

                                if (curBcsJob.RobotWIP.LastSendStageID != curStage.Data.STAGEID)
                                {
                                    lock (curBcsJob)
                                    {
                                        curBcsJob.RobotWIP.LastSendStageID = curStage.Data.STAGEID;
                                        curBcsJob.RobotWIP.LastSendOnTime = DateTime.Now;
                                    }
                                    //Save File
                                    ObjectManager.JobManager.EnqueueSave(curBcsJob);
                                }

                                #endregion

                            }
                        }
                    }
                }

                return;
            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                return;
            }

        }

        /// <summary> Get Robot Current Arm Info by Robot Arm Job Count
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        private void ReSetEQStagePriority(Robot curRobot)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curRobotStageList = null;
            bool ReSetPriority = false;

            try
            {
                #region [ Get CurRobot All Stage List ]

                curRobotStageList = ObjectManager.RobotStageManager.GetRobotStages(curRobot.Data.ROBOTNAME);

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

                    return;
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

                    return;
                }

                #endregion

                Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if (line == null)
                {
                    return;
                }

                if (line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW)
                {
                    bool bMQCMode = false;
                    bool bNormalMode = false;
                    string cleanlastReceiveProcessType = string.Empty;
                    Equipment eq1 = ObjectManager.EquipmentManager.GetEQP("L4");//1:normal, 2:MQC
                    Equipment eq2 = ObjectManager.EquipmentManager.GetEQP("L5");//1:normal, 2:MQC
                    Equipment cln = ObjectManager.EquipmentManager.GetEQP("L3");

                    if (eq1 == null || eq2 == null || cln == null)
                        return;

                    if ((eq1.File.EquipmentRunMode.ToUpper().Equals("NORMAL") && eq2.File.EquipmentRunMode.Equals("MQC")) ||
                        (eq1.File.EquipmentRunMode.Equals("MQC") && eq2.File.EquipmentRunMode.ToUpper().Equals("NORMAL")))
                        ReSetPriority = true;

                    if (ReSetPriority)
                    {
                        //check EQ Status, if Down, Low priority
                        if (eq1.File.EquipmentRunMode.ToUpper().Equals("NORMAL") && (eq1.File.Status == eEQPStatus.IDLE || eq1.File.Status == eEQPStatus.RUN))
                            bNormalMode = true;
                        if (eq1.File.EquipmentRunMode.Equals("MQC") && (eq1.File.Status == eEQPStatus.IDLE || eq1.File.Status == eEQPStatus.RUN))
                            bMQCMode = true;
                        if (eq2.File.EquipmentRunMode.ToUpper().Equals("NORMAL") && (eq2.File.Status == eEQPStatus.IDLE || eq2.File.Status == eEQPStatus.RUN))
                            bNormalMode = true;
                        if (eq2.File.EquipmentRunMode.Equals("MQC") && (eq2.File.Status == eEQPStatus.IDLE || eq2.File.Status == eEQPStatus.RUN))
                            bMQCMode = true;

                        cleanlastReceiveProcessType = cln.File.FinalReceiveGlassProcessType;
                        if (cleanlastReceiveProcessType == null || cleanlastReceiveProcessType.Length == 0) //set to Normal for MQC fetch first
                            cleanlastReceiveProcessType = "0";

                        if (cleanlastReceiveProcessType.Equals("0")) //last receive is Normal
                        {
                            foreach (RobotStage curStage in curRobotStageList)
                            {
                                Port port;

                                if (curStage.Data.STAGETYPE != eRobotStageType.PORT)
                                    continue;

                                port = ObjectManager.PortManager.GetPort(curStage.Data.STAGEID);
                                if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING || port.File.CassetteStatus == eCassetteStatus.IN_PROCESSING)
                                {
                                    IList<Job> lstjob = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo);
                                    if (lstjob.Count > 0)
                                    {
                                        Job jb = lstjob[0];
                                        if (jb.ArraySpecial.ProcessType != cleanlastReceiveProcessType)
                                        {
                                            if (jb.ArraySpecial.ProcessType.Equals("0") && bNormalMode) //if EQ not Down, high priority
                                            {
                                                curStage.Data.PRIORITY = 2;
                                            }
                                            else
                                            {
                                                if (!bMQCMode) //if other Mode EQ Down, high priority
                                                    curStage.Data.PRIORITY = 2;
                                                else
                                                    curStage.Data.PRIORITY = 1;
                                            }

                                            if (jb.ArraySpecial.ProcessType.Equals("1") && bMQCMode) //if EQ not Down, high priority
                                            {
                                                curStage.Data.PRIORITY = 2;
                                            }
                                            else
                                            {
                                                if (!bNormalMode) //if other Mode EQ Down, high priority
                                                    curStage.Data.PRIORITY = 2;
                                                else
                                                    curStage.Data.PRIORITY = 1;
                                            }
                                        }
                                        else
                                        {
                                            if (jb.ArraySpecial.ProcessType.Equals("0") && !bMQCMode) //if other Mode EQ Down, high priority
                                            {
                                                if (bNormalMode)
                                                    curStage.Data.PRIORITY = 2;
                                                else
                                                    curStage.Data.PRIORITY = 1;
                                            }
                                            else
                                            {
                                                curStage.Data.PRIORITY = 1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        curStage.Data.PRIORITY = 1;
                                    }
                                }
                                else
                                {
                                    curStage.Data.PRIORITY = 1;
                                }
                            }
                        }
                    }
                    else //if ELA not Mix Mode, ReSet all port priority is 1
                    {
                        foreach (RobotStage curStage in curRobotStageList)
                        {
                            if (curStage.Data.STAGETYPE != eRobotStageType.PORT)
                                continue;

                            curStage.Data.PRIORITY = 1;
                        }
                    }
                }

                return;
            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                return;
            }

        }

        /// <summary> Reset Robot eRobotContextParameter
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        private void ReSetRobotContextParameter(Robot curRobot)
        {
            try
            {
                curRobot.Context.AddParameter(eRobotContextParameter.IsRecvDelayTimeNGFlag, false);
                curRobot.Context.AddParameter(eRobotContextParameter.IsRecipeNGFlag, false);
                return;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return;
            }
        }
    }

}
