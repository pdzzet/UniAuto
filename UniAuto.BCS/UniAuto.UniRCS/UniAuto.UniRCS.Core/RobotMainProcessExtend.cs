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
        //For Check Use Function List -=======================================================================================================================================

        /// <summary> 根據目前Job產生的命令來做OrderByCondition 以決定TargetPistion與TargetSlotNo
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="cur1stDefineCmd"></param>
        /// <param name="cur2ndDefindCmd"></param>
        /// <param name="curFilterLDRQStageList"></param>
        /// <returns></returns>
        private bool CheckAllOrderByConditionByCommand(Robot curRobot, Job curBcsJob, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefindCmd, List<RobotStage> curFilterLDRQStageList)
        {
            string strlog = string.Empty;
            int intStageNo = 0;
            string funcName = string.Empty;
            int checkStepNo = 0;
            //20151026 add
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;

            try
            {
                //20151026 add Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_E0010 ] ,以ObjectName與MethodName為Key來決定是否紀錄Log
                fail_ReasonCode = string.Format("{0}_{1}_{2}", "RobotCoreService", "CheckAllOrderByConditionByCommand", "1stCommand");

                #region [ Check 1stCmd Action and define TargetPosition and TargetSlotNo ]

                funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                checkStepNo = curBcsJob.RobotWIP.CurStepNo;
                int cur1stCmdGetLDRQEmptySlotNo = 0;

                //0: None
                //1: Put          //2: Get          //4: Exchange
                //8: Put Ready    //16: Get Ready   //32: Get/Put
                switch (cur1stDefineCmd.Cmd01_Command)
                {
                    case eRobot_Trx_CommandAction.ACTION_NONE:  //None 沒命令則直接不做Orderby回傳Fail

                        //20160624 //20160706 PutReady,Filter_CurStepActionByJobLocation_For1Arm1Job後判斷,就會NONE
                        //List<RobotStage> _putreadystages = ObjectManager.RobotStageManager.GetRobotStages();
                        List<RobotStage> _putreadystages = new List<RobotStage>();
                        foreach (string putreadystage in curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST.Split(','))
                        {
                            _putreadystages.Add(ObjectManager.RobotStageManager.GetRobotStagebyStageID(putreadystage.PadLeft(2, '0')));
                        }
                        int.TryParse(curBcsJob.RobotWIP.CurLocation_StageID, out intStageNo);
                        bool _isputready = false;
                        if (intStageNo == 0)
                        {
                            int TargetPosition = 0;
                            //int.TryParse(curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST.Split(',')[0], out TargetPosition);
                            foreach (RobotStage _putreadystage in _putreadystages)
                            {
                                if (_putreadystage.Data.STAGETYPE == eRobotStageType.EQUIPMENT && _putreadystage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y")
                                {
                                    int.TryParse(_putreadystage.Data.STAGEID, out TargetPosition);
                                    cur1stDefineCmd.Cmd01_TargetPosition = TargetPosition;
                                    cur1stDefineCmd.Cmd01_TargetSlotNo = 1;
                                    if (curRobot.CurRealTimeArmSingleJobInfoList[0].ArmJobExist == eGlassExist.Exist && curRobot.CurRealTimeArmSingleJobInfoList[1].ArmJobExist == eGlassExist.NoExist)
                                    {
                                        cur1stDefineCmd.Cmd01_ArmSelect = 1;
                                        _isputready = true;
                                        break;
                                    }
                                    else if (curRobot.CurRealTimeArmSingleJobInfoList[1].ArmJobExist == eGlassExist.Exist && curRobot.CurRealTimeArmSingleJobInfoList[0].ArmJobExist == eGlassExist.NoExist)
                                    {
                                        cur1stDefineCmd.Cmd01_ArmSelect = 2;
                                        _isputready = true;
                                        break;
                                    }
                                    else
                                    {
                                        //2Arm都沒片,or 2Arm都有片,往下跑到return false 
                                    }
                                }
                            }
                        }
                        #region[DebugLog]
                        
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command Action({9}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur1stDefineCmd.Cmd01_Command.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion
                        if (!_isputready)
                            return false;
                        break;
                    case eRobot_Trx_CommandAction.ACTION_PUT:  //PUT
                    case eRobot_Trx_CommandAction.ACTION_EXCHANGE:  //Exchange <---PUT轉化
                    case eRobot_Trx_CommandAction.ACTION_GETPUT: //Get/PUT <---PUT轉化

                        #region [ 1stCmd需要做OrderBy處理 :Order by 1st Cmd LDRQ StageList ]

                        //#region [ PUT/Exchange/Get&Put 需要 Check has LDRQ Stage List

                        //if (curFilterLDRQStageList.Count == 0)
                        //{
                        //    //20160706 PutReady,Filter_CurStepActionByJobLocation_For1Arm1Job先判斷就會PUT
                        //    //_putreadystages = ObjectManager.RobotStageManager.GetRobotStages();
                        //    _putreadystages = new List<RobotStage>();
                        //    _putreadystages.Clear();
                        //    foreach (string putreadystage in curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST.Split(','))
                        //    {
                        //        _putreadystages.Add(ObjectManager.RobotStageManager.GetRobotStagebyStageID(putreadystage.PadLeft(2, '0')));
                        //    }
                        //    int TargetPosition = 0;
                        //    int.TryParse(curBcsJob.RobotWIP.CurLocation_StageID, out intStageNo);
                        //    _isputready = false;
                        //    if (intStageNo == 0) //在Arm上
                        //    {
                        //        foreach (RobotStage _putreadystage in _putreadystages)
                        //        {
                        //            if (_putreadystage.Data.STAGETYPE == eRobotStageType.EQUIPMENT && _putreadystage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y")
                        //            {
                        //                int.TryParse(_putreadystage.Data.STAGEID, out TargetPosition);
                        //                cur1stDefineCmd.Cmd01_TargetPosition = TargetPosition;
                        //                cur1stDefineCmd.Cmd01_TargetSlotNo = 1;
                        //                _isputready = true;   //如果有找到第一個PutReady的stage,Cmd01 TargetPosition跟TargetSlotNo塞完,就break跳出foreach
                        //                break;
                        //            }
                        //        }
                        //    }

                        //    //沒有LDRQ Stage List 
                        //    #region[DebugLog]

                        //    if (IsShowDetialLog == true)
                        //    {
                        //        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) can not Find Status is LDRQ!",
                        //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        //                                curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                        //                                curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                        //                                curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST);

                        //        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        //    }

                        //    #endregion
                        //    if (!_isputready)
                        //        return false;
                        //    break;
                        //}

                        //#endregion

                        if (CheckAllOrderByConditionByStepNo(curRobot, curBcsJob, checkStepNo, cur1stDefineCmd, ref curFilterLDRQStageList,false) == false)
                        {
                            bool _openJobOnArmMove = false;
                            if (ParameterManager.Parameters.ContainsKey("ROBOT_JOBONARM_MOVE"))
                               bool.TryParse(ParameterManager.Parameters["ROBOT_JOBONARM_MOVE"].Value.ToString(), out _openJobOnArmMove);
                            //下面这段意思是robot 手臂上的job 当orderby check 失败，skip orderby 然后下putReady ，但是在带片跑的情况下，不能让手臂上的job 下putReady ，要走到带片跑
                            if (!_openJobOnArmMove)
                            {
                                //20160801
                                bool _skipOrderbyCheckPutReady = false;
                                string[] _stages = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST.Split(',');
                                if (_stages == null || _stages.Length == 0)
                                {
                                    _skipOrderbyCheckPutReady = false;
                                }
                                foreach (string _stage in _stages)
                                {
                                    RobotStage stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(_stage);
                                    if (stage == null)
                                    {
                                        _skipOrderbyCheckPutReady = false;
                                    }
                                    if (stage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y")
                                    {
                                        _skipOrderbyCheckPutReady = true;
                                        break;
                                    }
                                    else
                                        _skipOrderbyCheckPutReady = false;
                                }
                                curBcsJob.RobotWIP.SkipOrderbyCheck = _skipOrderbyCheckPutReady;
                                if (!_skipOrderbyCheckPutReady)
                                    //OrderBy失敗,會包含有target stage/slot選擇失敗，所以還是不能下命令
                                    return false;
                            }
                            return false;
                        }

                        #region [ Get Target Position ]

                        if (cur1stDefineCmd.Cmd01_TargetPosition == 0)
                        {
                            //20160801
                            if (curFilterLDRQStageList.Count > 0)
                            {

                                //不管OrderBy是否成功都得以第一個StageInfo為主
                                int.TryParse(curFilterLDRQStageList[0].Data.STAGEID, out intStageNo);
                                cur1stDefineCmd.Cmd01_TargetPosition = intStageNo;
                            }
                            else
                            {
                                //20160801
                                _isputready = false;
                                //if (curBcsJob.RobotWIP.SkipOrderbyCheck)
                                if ((curBcsJob.RobotWIP.SkipFilterCheck && curBcsJob.RobotWIP.SkipOrderbyCheck) || (curBcsJob.RobotWIP.SkipFilterCheck && curBcsJob.RobotWIP.RunOrderbyCheckOK) || (curBcsJob.RobotWIP.SkipOrderbyCheck && curBcsJob.RobotWIP.RunFilterCheckOK))
                                {
                                    string[] _stages = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST.Split(',');
                                    foreach (string _putreadystage in _stages)
                                    {
                                        RobotStage putreadystage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(_putreadystage);
                                        if (putreadystage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y")
                                        {
                                            int.TryParse(putreadystage.Data.STAGEID, out intStageNo);
                                            cur1stDefineCmd.Cmd01_TargetPosition = intStageNo;
                                            cur1stDefineCmd.Cmd01_TargetSlotNo = 1;
                                            cur1stDefineCmd.Cmd01_CstSeq = curBcsJob.CassetteSequenceNo;
                                            cur1stDefineCmd.Cmd01_JobSeq = curBcsJob.JobSequenceNo;
                                            if (curRobot.CurRealTimeArmSingleJobInfoList[0].ArmJobExist == eGlassExist.Exist && curRobot.CurRealTimeArmSingleJobInfoList[1].ArmJobExist == eGlassExist.NoExist)
                                            {
                                                cur1stDefineCmd.Cmd01_ArmSelect = 1;
                                                _isputready = true;
                                                break;
                                            }
                                            else if (curRobot.CurRealTimeArmSingleJobInfoList[1].ArmJobExist == eGlassExist.Exist && curRobot.CurRealTimeArmSingleJobInfoList[0].ArmJobExist == eGlassExist.NoExist)
                                            {
                                                cur1stDefineCmd.Cmd01_ArmSelect = 2;
                                                _isputready = true;
                                                break;
                                            }
                                            else
                                            {
                                                //2Arm都沒片,or 2Arm都有片,什麼都不做,繼續判斷下一個stage是不是PutReady                                           
                                            }
                                            
                                        }
                                        else
                                            continue;
                                    }
                                    if (!_isputready)
                                        return false;
                                    else
                                        return true;  //表示已經Skip Orderby,回傳true
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            //TargetPosition已經在Order Rule裡決定
                        }

                        #endregion

                        #region [ Get Target SlotNo ]

                        if (cur1stDefineCmd.Cmd01_TargetSlotNo == 0)
                        {
                            //取得LDRQ Slot(預設為1)
                            if (curFilterLDRQStageList[0].File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                            {
                                cur1stDefineCmd.Cmd01_TargetSlotNo = GetLDRQStageEmptySlotNo(curRobot, curBcsJob, checkStepNo, curFilterLDRQStageList[0], 0, true);
                            }
                            else if (cur1stDefineCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_PUT && curFilterLDRQStageList[0].File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY)
                            {
                                //20160106 add for 新需求 MAC RecipeID最後一碼(第12碼)為"1"不可以Exchange
                                //if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag == true || curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_EXCHANGE)
                                if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag == true && curFilterLDRQStageList[0].MacCanNotExchangeFlag == false)
                                {
                                    //UDRQ and Exchange Request=>Cmd PUT change to EX =>要判斷是否可Exchange條件
                                    cur1stDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_EXCHANGE;
                                }
                                else if (curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_GETPUT)
                                {
                                    //UDRQ and Exchange Request=>Cmd PUT change to Get/Put =>要判斷是否可Get/Put條件
                                    cur1stDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_GETPUT;
                                }

                                //Exchange TargetSlotNo 應該是UDRQ Job 的SlotNo才對=>要判斷是否可Exchange條件
                                if (curFilterLDRQStageList[0].curUDRQ_SlotList.Count != 0)
                                {
                                    foreach (int slotKey in curFilterLDRQStageList[0].curUDRQ_SlotList.Keys)
                                    {
                                        cur1stDefineCmd.Cmd01_TargetSlotNo = slotKey;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                cur1stDefineCmd.Cmd01_TargetSlotNo = GetLDRQStageEmptySlotNo(curRobot, curBcsJob, checkStepNo, curFilterLDRQStageList[0], 0, false);
                            }
                        }
                        else
                        {
                            //TargetSlotNo已經在Order Rule裡決定
                            //20160706
                            #region 將 RobotCommand 改成 Exchange 或 GetPut, 判斷 _canUsePreFetchFlag
                            if (cur1stDefineCmd.Cmd01_Command == eRobot_ControlCommand.PUT && curFilterLDRQStageList[0].File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY)
                            {
                                //20160106 add for 新需求 MAC RecipeID最後一碼(第12碼)為"1"不可以Exchange
                                //if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag || curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_EXCHANGE)
                                if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag && curFilterLDRQStageList[0].MacCanNotExchangeFlag == false)
                                {
                                    //UDRQ and Exchange Request=>Cmd PUT change to EX =>要判斷是否可Exchange條件
                                    cur1stDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_EXCHANGE;
                                }
                                else if (curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_GETPUT)
                                {
                                    //UDRQ and Exchange Request=>Cmd PUT change to EX =>要判斷是否可Get/Put條件
                                    cur1stDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_GETPUT;
                                }
                            }
                            #endregion
                        }

                        #endregion

                        //cur1stDefineCmd.Cmd01_TargetSlotNo = GetLDRQStageEmptySlotNo(curRobot, curBcsJob, checkStepNo, curFilterLDRQStageList[0], 0,false);

                        //因為1st Cmd 為PUT相關. 此為Target SlotNo .會佔用Target Stage LDRQ EmptySlotNo
                        cur1stCmdGetLDRQEmptySlotNo = cur1stDefineCmd.Cmd01_TargetSlotNo;

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) ({7}) Cmd TargetPosition({8}) TargetSlotNo({9}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    funcName, cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #endregion

                        break;

                    default:

                        //Other Action GET,GetReady,PutReady則以Job目前的StageID為TargetPosition
                        int.TryParse(curBcsJob.RobotWIP.CurLocation_StageID, out intStageNo);
                        cur1stDefineCmd.Cmd01_TargetPosition = intStageNo;
                        cur1stDefineCmd.Cmd01_TargetSlotNo = curBcsJob.RobotWIP.CurLocation_SlotNo;

                        //因為1st Cmd 為GET相關. 此為Source SlotNo .不佔用Target Stage LDRQ EmptySlotNo
                        cur1stCmdGetLDRQEmptySlotNo = 0;

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) {7} Cmd TargetPosition({8}) TargetSlotNo({9}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    funcName,
                                                    cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        break;
                }

                cur1stDefineCmd.Cmd01_CstSeq = curBcsJob.CassetteSequenceNo;
                cur1stDefineCmd.Cmd01_JobSeq = curBcsJob.JobSequenceNo;

                #endregion

                #region [ Check 1st Cmd TargetPosition and TargetSlotNo 不可為0 ]

                if ((cur1stDefineCmd.Cmd01_TargetPosition == 0 || cur1stDefineCmd.Cmd01_TargetSlotNo == 0))
                {
                    if (!curBcsJob.RobotWIP.RTCReworkFlag)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command targetPositon({9}) or TargetSlotNo({10}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0010 ]

                        if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command targetPositon({9}) or TargetSlotNo({10}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) StageID({3}) StepNo({4}) Action({5}) StageIDList({6}) {7} Command targetPositon({8}) or TargetSlotNo({9}) is illegal",
                            //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //                        curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                            //                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                            //                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                            //                        funcName, cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());

                            failMsg = string.Format("Job({0}_{1}) StageID({2}) StepNo({3}) Action({4}) StageIDList({5}) {6} Command targetPositon({7}) or TargetSlotNo({8}) is illegal",
                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());

                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion

                        }

                        #endregion
                    }

                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_E0010 ]
                    RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                }


                #endregion

                //20151026 add Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_E0011 ] ,以ObjectName與MethodName為Key來決定是否紀錄Log
                fail_ReasonCode = string.Format("{0}_{1}_{2}", "RobotCoreService", "CheckAllOrderByConditionByCommand", "2ndCommand");

                #region [ Check 2ndCmd Action and define TargetPosition and TargetSlotNo  ]

                funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;
                //20151014 Modity NextStep由WIP來取得
                checkStepNo = curBcsJob.RobotWIP.NextStepNo; // curBcsJob.RobotWIP.CurStepNo + 1;

                //0: None
                //1: Put          //2: Get          //4: Exchange
                //8: Put Ready    //16: Get Ready   //32: Get/Put
                switch (cur2ndDefindCmd.Cmd01_Command)
                {
                    case 0:  //None 2nd沒命令則直接不做Orderby回傳true

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command Action({9}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur2ndDefindCmd.Cmd01_Command.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //Clear[ Robot_Fail_Case_E0011 ] 沒有第二個命令則直接移除掉相關的Fail Code
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                        return true;

                    case 1:  //PUT
                    case 4:  //Exchange <---PUT轉化
                    case 32: //Get/PUT <---PUT轉化

                        #region [ 2ndCmd需要做OrderBy處理 :Order by 2nd Cmd LDRQ StageList ]

                        #region [ Check curFilterLDRQStageList.Count]

                        if (curFilterLDRQStageList.Count == 0)
                        {
                            //沒有LDRQ Stage List 
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) can not Find Status is LDRQ!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        #endregion

                        //Order by 2nd Cmd LDRQ StageList
                        if (CheckAllOrderByConditionByStepNo(curRobot, curBcsJob, checkStepNo, cur2ndDefindCmd, ref curFilterLDRQStageList,true) == false)
                        {
                            //OrderBy失敗,會包含有target stage/slot選擇失敗，所以還是不能下命令
                            return false;
                        }

                        #region [ Get Target Position ]

                        if (cur2ndDefindCmd.Cmd01_TargetPosition == 0)
                        {
                            //不管OrderBy是否成功都得以第一個StageInfo為主
                            int.TryParse(curFilterLDRQStageList[0].Data.STAGEID, out intStageNo);
                            cur2ndDefindCmd.Cmd01_TargetPosition = intStageNo;

                            //20160801 因為會進到Prefetch的CheckRobotStageJobRouteCondition_ForGetGetPutPut
                            //所以skip Filter跟skip Orderby會同時true才對,因此也可以只判斷SkipOrderbyCheck==true就行
                            if (curBcsJob.RobotWIP.SkipFilterCheck && curBcsJob.RobotWIP.SkipOrderbyCheck)
                            {
                                foreach (RobotStage _putreadystage in curFilterLDRQStageList)
                                {
                                    if (_putreadystage.Data.PUTREADYFLAG.ToString().ToUpper() == "Y")
                                    {
                                        int.TryParse(_putreadystage.Data.STAGEID, out intStageNo);
                                        cur2ndDefindCmd.Cmd01_TargetPosition = intStageNo;
                                        break;
                                    }
                                    else
                                        continue;
                                }
                            }
                        }
                        else
                        {
                            //TargetPosition已經在Order Rule裡決定
                        }

                        #endregion


                        #region Get Pre-Fetch
                        bool _canUsePreFetchFlag = false;
                        if (curRobot.Context.ContainsKey(eRobotContextParameter.CanUsePreFetchFlag))
                        {
                            _canUsePreFetchFlag = (curRobot.Context[eRobotContextParameter.CanUsePreFetchFlag].ToString() == "Y" ? true : false);
                        }
                        #endregion


                        #region [ Get Target SlotNo ]

                        if (cur2ndDefindCmd.Cmd01_TargetSlotNo == 0)
                        {
                            #region 決定 SlotNo, 將 RobotCommand 改成 Exchange 或 GetPut, 判斷 _canUsePreFetchFlag
                            //非预取状况决定Target Slot 和 装换命令 
                            if (_canUsePreFetchFlag == false)
                            {
                                //取得LDRQ Slot(預設為1)
                                if (curFilterLDRQStageList[0].File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                                {
                                    //取得原Slot資訊. SendControl Command 也要補上Cst Seq與Job Seq資訊
                                    cur2ndDefindCmd.Cmd01_TargetSlotNo = GetLDRQStageEmptySlotNo(curRobot, curBcsJob, checkStepNo, curFilterLDRQStageList[0], cur1stCmdGetLDRQEmptySlotNo, true);
                                }
                                else if (cur2ndDefindCmd.Cmd01_Command == eRobot_ControlCommand.PUT && curFilterLDRQStageList[0].File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY)
                                {
                                    //20160106 add for 新需求 MAC RecipeID最後一碼(第12碼)為"1"不可以Exchange
                                    //if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag || curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_EXCHANGE)
                                    if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag && curFilterLDRQStageList[0].MacCanNotExchangeFlag == false)
                                    {
                                        //UDRQ and Exchange Request=>Cmd PUT change to EX =>要判斷是否可Exchange條件
                                        cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_EXCHANGE;
                                    }
                                    else if (curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_GETPUT)
                                    {
                                        //UDRQ and Exchange Request=>Cmd PUT change to EX =>要判斷是否可Get/Put條件
                                        cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_GETPUT;
                                    }

                                    //cur2ndDefindCmd.Cmd01_TargetSlotNo = GetLDRQStageEmptySlotNo(curRobot, curBcsJob, checkStepNo, curFilterLDRQStageList[0], cur1stCmdGetLDRQEmptySlotNo, false);
                                    //只會與Slot01作Exchage
                                    //cur2ndDefindCmd.Cmd01_TargetSlotNo = 1;
                                    //Exchange TargetSlotNo 應該是UDRQ Job 的SlotNo才對 =>要判斷是否可Exchange或是Get/Put條件
                                    if (curFilterLDRQStageList[0].curUDRQ_SlotList.Count != 0)
                                    {
                                        foreach (int slotKey in curFilterLDRQStageList[0].curUDRQ_SlotList.Keys)
                                        {
                                            cur2ndDefindCmd.Cmd01_TargetSlotNo = slotKey;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    cur2ndDefindCmd.Cmd01_TargetSlotNo = GetLDRQStageEmptySlotNo(curRobot, curBcsJob, checkStepNo, curFilterLDRQStageList[0], cur1stCmdGetLDRQEmptySlotNo, false);
                                }
                            }
                            else//如果有 預取 並且 有 Put Ready 功能, 則 預設給1
                            {
                                cur2ndDefindCmd.Cmd01_TargetSlotNo = 1;
                                //if (curFilterLDRQStageList[0].Data.PUTREADYFLAG.ToString().ToUpper() != "Y") cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE; //取消命令!!
                                //20160801 
                                RobotStage _stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(cur2ndDefindCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));
                                if (_stage == null)
                                {
                                    if (curFilterLDRQStageList[0].Data.PUTREADYFLAG.ToString().ToUpper() != "Y") cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE; //取消命令!!
                                }
                                else
                                {
                                    if (_stage.Data.PUTREADYFLAG.ToString().ToUpper() != "Y") cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE; //取消命令!!
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            //分成非预取和预取分开处理
                            if (_canUsePreFetchFlag==false)
                            {
                                //TargetSlotNo已經在Order Rule裡決定
                                #region 將 RobotCommand 改成 Exchange 或 GetPut, 判斷 _canUsePreFetchFlag
                                if (cur2ndDefindCmd.Cmd01_Command == eRobot_ControlCommand.PUT && curFilterLDRQStageList[0].File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY)
                                {
                                    //20160106 add for 新需求 MAC RecipeID最後一碼(第12碼)為"1"不可以Exchange
                                    //if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag || curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_EXCHANGE)
                                    if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag && curFilterLDRQStageList[0].MacCanNotExchangeFlag == false)
                                    {
                                        //UDRQ and Exchange Request=>Cmd PUT change to EX =>要判斷是否可Exchange條件
                                        cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_EXCHANGE;
                                    }
                                    else if (curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_GETPUT)
                                    {
                                        //UDRQ and Exchange Request=>Cmd PUT change to EX =>要判斷是否可Get/Put條件
                                        cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_GETPUT;
                                    }
                                }
                            }
                            else  //如果有 預取 並且 有 Put Ready 功能, 則 預設給1
                            {
                                cur2ndDefindCmd.Cmd01_TargetSlotNo = 1;
                                //if (curFilterLDRQStageList[0].Data.PUTREADYFLAG.ToString().ToUpper() != "Y") cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE; //取消命令!!
                                //20160801 
                                RobotStage _stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(cur2ndDefindCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'));
                                if (_stage == null)
                                {
                                    if (curFilterLDRQStageList[0].Data.PUTREADYFLAG.ToString().ToUpper() != "Y") cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE; //取消命令!!
                                }
                                else
                                {
                                    if (_stage.Data.PUTREADYFLAG.ToString().ToUpper() != "Y") cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE; //取消命令!!
                                }
                            }
                            #endregion
                        }

                        #endregion

                        //cur2ndDefindCmd.Cmd01_TargetSlotNo = GetLDRQStageEmptySlotNo(curRobot, curBcsJob, checkStepNo, curFilterLDRQStageList[0], cur1stCmdGetLDRQEmptySlotNo);

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) ({7}) Cmd TargetPosition({8}) TargetSlotNo({9}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    funcName, cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #endregion

                        break;

                    default:

                        //Other Action GET,GetReady,PutReady 則以Job目前的StageID為TargetPosition
                        int.TryParse(curBcsJob.RobotWIP.CurLocation_StageID, out intStageNo);
                        cur2ndDefindCmd.Cmd01_TargetPosition = intStageNo;
                        cur2ndDefindCmd.Cmd01_TargetSlotNo = curBcsJob.RobotWIP.CurLocation_SlotNo;

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) {7} Cmd TargetPosition({8}) TargetSlotNo({9}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    funcName,
                                                    cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        break;
                }

                cur2ndDefindCmd.Cmd01_CstSeq = curBcsJob.CassetteSequenceNo;
                cur2ndDefindCmd.Cmd01_JobSeq = curBcsJob.JobSequenceNo;

                #endregion

                #region [ Check 2nd Cmd TargetPosition and TargetSlotNo 不可為0 ]

                if ((cur2ndDefindCmd.Cmd01_TargetPosition == 0 || cur2ndDefindCmd.Cmd01_TargetSlotNo == 0))
                {
                    if (!curBcsJob.RobotWIP.RTCReworkFlag)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command targetPositon({9}) or TargetSlotNo({10}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0011 ]

                        if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command targetPositon({9}) or TargetSlotNo({10}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) StageID({3}) StepNo({4}) Action({5}) StageIDList({6}) {7} Command targetPositon({8}) or TargetSlotNo({9}) is illegal",
                            //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //                        curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                            //                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                            //                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                            //                        funcName, cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());

                            failMsg = string.Format("Job({0}_{1})[{2}] StageID({3}) StepNo({4}) Action({5}) StageIDList({6}) {7} Command targetPositon({8}) or TargetSlotNo({9}) is illegal",
                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,MethodBase.GetCurrentMethod().Name,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());
                            failMsg = string.Format("RtnCode({0})RtnMsg({1})", eJobOrderBy_ReturnCode.NG_targetPositonOrTargetSlotNoIsillegal, failMsg);
                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion

                        }

                        #endregion
                    }

                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_E0011 ]
                    RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
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

        /// <summary> 根據Step來決定OrderBy Function
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkStepNo"></param>
        /// <param name="curDefineCmd"></param>
        /// <param name="curAfterFilterCanUseStageList"></param>
        /// <returns></returns>
        private bool CheckAllOrderByConditionByStepNo(Robot curRobot, Job curBcsJob, int checkStepNo, DefineNormalRobotCmd curDefineCmd, ref List<RobotStage> curAfterFilterCanUseStageList, bool _is2ndCmdFlag)
        {

            IRobotContext robotConText = new RobotContext();
            string fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;
            bool after1stOrderByFlag = false;
            //IOrderedEnumerable<RobotStage> afterOrderByResultInfo = null;

            try
            {

                List<RobotRuleOrderby> curOrderByList = ObjectManager.RobotManager.GetRuleOrderby(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, checkStepNo);

                if (curOrderByList == null)
                {
                    #region[DebugLog]

                    #region[DebugLog][ Start Rule Job OrderBy Function ]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not find any Rule Job OrderBy function!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #endregion

                    //找不到任何orderby rule，就不用orderby，直接回ok
                    return true;
                }

                #region [Check CurStep All OrderBy Condition ]

                #region [ robotConText内添加curRobot等参数 ] =====================================================================

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);
                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curAfterFilterCanUseStageList);
                robotConText.AddParameter(eRobotContextParameter.DefineNormalRobotCmd, curDefineCmd);
                //判斷是否已經做過第一次的OrderByCondition
                robotConText.AddParameter(eRobotContextParameter.Afrer1stOrderByCheckFlag, after1stOrderByFlag);
                //robotConText.AddParameter(eRobotContextParameter.AfrerOrderByResultInfo, afterOrderByResultInfo);

                #endregion =======================================================================================================================================================
                //20160801
                bool _skipOrderbyCheck = (!_is2ndCmdFlag ? false : CheckPrefetchFlag(curRobot));
                curBcsJob.RobotWIP.SkipOrderbyCheck = _skipOrderbyCheck;
                //20160802
                curBcsJob.RobotWIP.RunOrderbyCheckOK = false;

                if (!_skipOrderbyCheck)
                {
                    foreach (RobotRuleOrderby curOrderByCondition in curOrderByList)
                    {
                        //Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_E0005 ] ,以Rule Job OrderBy 的ObjectName與MethodName為Key來決定是否紀錄Log
                        fail_ReasonCode = string.Format("{0}_{1}", curOrderByCondition.Data.OBJECTNAME, curOrderByCondition.Data.METHODNAME);

                        #region[DebugLog][ Start Rule Job OrderBy Function ]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job OrderBy object({4}) MethodName({5}) IsEnable({6}) orderByAction({7}) Start {8}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curOrderByCondition.Data.OBJECTNAME, curOrderByCondition.Data.METHODNAME, curOrderByCondition.Data.ISENABLED, curOrderByCondition.Data.ORDERBY,
                                                    new string(eRobotCommonConst.RULE_ORDERBY_START_CHAR, eRobotCommonConst.RULE_ORDERBY_START_CHAR_LENGTH));

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //排序方式 DESC 數字越大越優先
                        robotConText.AddParameter(eRobotContextParameter.OrderByAction, curOrderByCondition.Data.ORDERBY);

                        if (curOrderByCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                        {

                            checkFlag = (bool)Invoke(curOrderByCondition.Data.OBJECTNAME, curOrderByCondition.Data.METHODNAME, new object[] { robotConText });

                            if (checkFlag == false)
                            {

                                #region[DebugLog][ End Rule Job OrderBy Function ]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job OrderBy Fail, object({4}) MethodName({5}) RtnCode({4})  RtnMsg({6}]!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curOrderByCondition.Data.OBJECTNAME, curOrderByCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job OrderBy object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curOrderByCondition.Data.OBJECTNAME, curOrderByCondition.Data.METHODNAME,
                                                            curOrderByCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_ORDERBY_END_CHAR, eRobotCommonConst.RULE_ORDERBY_END_CHAR_LENGTH));

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0005 ]

                                if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job OrderBy Fail, object({4}) MethodName({5}) RtnCode({6})  RtnMsg({7}]!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curOrderByCondition.Data.OBJECTNAME, curOrderByCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                    //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6}]!",
                                    //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curOrderByCondition.Data.OBJECTNAME,
                                    //                        curOrderByCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    failMsg = string.Format("RtnCode({0})  RtnMsg({1}]!",
                                                            robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                    #endregion

                                }

                                #endregion

                                //有重大異常直接結束orderBy邏輯回復NG
                                return false;

                            }
                            else
                            {

                                //Clear[ Robot_Fail_Case_E0005 ]
                                RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                                #region[DebugLog][ End Rule Job OrderBy Function ]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job OrderBy object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                            curOrderByCondition.Data.OBJECTNAME, curOrderByCondition.Data.METHODNAME,
                                                            curOrderByCondition.Data.ISENABLED,
                                                            new string(eRobotCommonConst.RULE_ORDERBY_END_CHAR, eRobotCommonConst.RULE_ORDERBY_END_CHAR_LENGTH));

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                ////20151002 add 如果OrderBy成功表示第一次Orderby已經做完要將Flag變動
                                if (after1stOrderByFlag == false)
                                {
                                    after1stOrderByFlag = true;
                                    //更新after1stOrderByFlag
                                    robotConText.AddParameter(eRobotContextParameter.Afrer1stOrderByCheckFlag, after1stOrderByFlag);
                                }

                            }
                        }
                        else
                        {

                            #region[DebugLog][ End Rule Job OrderBy Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job OrderBy object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curOrderByCondition.Data.OBJECTNAME, curOrderByCondition.Data.METHODNAME, curOrderByCondition.Data.ISENABLED,
                                                        new string(eRobotCommonConst.RULE_ORDERBY_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                        }

                    }
                    curBcsJob.RobotWIP.RunOrderbyCheckOK = true;
                }
                #endregion

                //Get AfterOrderBy結果
                //取得最新curAfterFilterCanUseStageList20190121
                IOrderedEnumerable<RobotStage> afterAllOrderByResult = (IOrderedEnumerable<RobotStage>)robotConText[eRobotContextParameter.AfrerOrderByResultInfo];
                if (afterAllOrderByResult != null)
                    curAfterFilterCanUseStageList = afterAllOrderByResult.ToList();

                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) CurAfterFiterCanUseStageList.count({2}),CurAfterFiterCanUseStageList First Index StageId({3})",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curAfterFilterCanUseStageList.Count,curAfterFilterCanUseStageList[0].Data.STAGEID);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                return true;

            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }

        /// <summary> 根據目前Job產生的命令來做OrderByCondition 以決定TargetPistion與TargetSlotNo
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="cur1stDefineCmd"></param>
        /// <param name="cur2ndDefindCmd"></param>
        /// <param name="curFilterLDRQStageList"></param>
        /// <returns></returns>
        private bool CheckSlotBlockInfo_AllOrderByConditionByCommand(Robot curRobot, RobotCanControlSlotBlockInfo curRobotStageSlotBlockInfo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefindCmd, List<RobotStage> curFilterLDRQStageList)
        {
            string strlog = string.Empty;
            int intStageNo = 0;
            string funcName = string.Empty;
            int checkStepNo = 0;
            string fail_ReasonCode = string.Empty;
            string failMsg = string.Empty;

            try
            {
                //SlotBlockInfo已經做過Filter , 證明最多兩片的目的同樣所以只要提出其中之一的Job OrderBy
                //取得SlotBlockInfo中第一筆Data
                Job curBcsJob = curRobotStageSlotBlockInfo.CurBlockCanControlJobList[0];

                //Set want To Check Function Fail_ReasonCode, 以ObjectName與MethodName為Key來決定是否紀錄Log
                fail_ReasonCode = string.Format("{0}_{1}_{2}", "RobotCoreService", "CheckSlotBlockInfo_AllOrderByConditionByCommand", "1stCommand");

                #region [ Check 1stCmd Action and define TargetPosition and TargetSlotNo ]

                funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                checkStepNo = curBcsJob.RobotWIP.CurStepNo;
                int cur1stCmdGetLDRQEmptySlotNo = 0;

                //0: None
                //1: Put          //2: Get          //4: Exchange
                //8: Put Ready    //16: Get Ready   //32: Get/Put
                switch (cur1stDefineCmd.Cmd01_Command)
                {
                    case eRobot_Trx_CommandAction.ACTION_NONE:  //None 沒命令則直接不做Orderby回傳Fail

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command Action({9}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur1stDefineCmd.Cmd01_Command.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;

                    case eRobot_Trx_CommandAction.ACTION_PUT:        //PUT
                    case eRobot_Trx_CommandAction.ACTION_EXCHANGE:   //Exchange <---PUT轉化
                    case eRobot_Trx_CommandAction.ACTION_GETPUT:     //Get/PUT <---PUT轉化

                        #region [ 1stCmd需要做OrderBy處理 :Order by 1st Cmd LDRQ StageList ]

                        #region [ PUT/Exchange/Get&Put 需要 Check has LDRQ Stage List ]

                        if (curFilterLDRQStageList.Count == 0)
                        {
                            //沒有LDRQ Stage List 
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) can not Find Status is LDRQ!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        #endregion

                        if (CheckAllOrderByConditionByStepNo(curRobot, curBcsJob, checkStepNo, cur1stDefineCmd, ref curFilterLDRQStageList,false) == false)
                        {
                            //OrderBy失敗,會包含有target stage/slot選擇失敗，所以還是不能下命令
                            return false;
                        }

                        #region [ Get Target Position ]

                        if (cur1stDefineCmd.Cmd01_TargetPosition == 0)
                        {
                            //不管OrderBy是否成功都得以第一個StageInfo為主
                            int.TryParse(curFilterLDRQStageList[0].Data.STAGEID, out intStageNo);
                            cur1stDefineCmd.Cmd01_TargetPosition = intStageNo;
                        }
                        else
                        {
                            //TargetPosition已經在Order Rule裡決定
                        }

                        #endregion

                        #region [ Get Target SlotNo ]

                        if (cur1stDefineCmd.Cmd01_TargetSlotNo == 0)
                        {
                            //取得LDRQ Slot(預設為1)
                            if (curFilterLDRQStageList[0].File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                            {
                                cur1stDefineCmd.Cmd01_TargetSlotNo = GetLDRQStageEmptySlotBlockInfoSlotNo(curRobot, curBcsJob, checkStepNo, curFilterLDRQStageList[0], 0, true);
                            }
                            else if (cur1stDefineCmd.Cmd01_Command == eRobot_Trx_CommandAction.ACTION_PUT && curFilterLDRQStageList[0].File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY)
                            {
                                #region  [ Cell Special Arm不支援Exchange ]
                                ////20160106 add for 新需求 MAC RecipeID最後一碼(第12碼)為"1"不可以Exchange
                                //if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag == true && curFilterLDRQStageList[0].MacCanNotExchangeFlag == false)
                                //{
                                //    //UDRQ and Exchange Request=>Cmd PUT change to EX=>要判斷是否可Exchange條件
                                //    cur1stDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_EXCHANGE;
                                //}
                                //else if (curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_GETPUT)
                                //{
                                //    //UDRQ and Exchange Request=>Cmd PUT change to Get/Put =>要判斷是否可Get/Put條件
                                //    cur1stDefineCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_GETPUT;
                                //}

                                ////Exchange TargetSlotNo 應該是UDRQ Job 的SlotNo才對 =>要判斷是否可Exchange條件
                                //if (curFilterLDRQStageList[0].curUDRQ_SlotList.Count != 0)
                                //{
                                //    foreach (int slotKey in curFilterLDRQStageList[0].curUDRQ_SlotList.Keys)
                                //    {
                                //        cur1stDefineCmd.Cmd01_TargetSlotNo = slotKey;
                                //        break;
                                //    }
                                //}
                                #endregion
                            }
                            else
                            {
                                cur1stDefineCmd.Cmd01_TargetSlotNo = GetLDRQStageEmptySlotBlockInfoSlotNo(curRobot, curBcsJob, checkStepNo, curFilterLDRQStageList[0], 0, false);
                            }
                        }
                        else
                        {
                            //TargetSlotNo已經在Order Rule裡決定
                        }

                        #endregion

                        //因為1st Cmd 為PUT相關. 此為Target SlotNo .會佔用Target Stage LDRQ EmptySlotNo
                        cur1stCmdGetLDRQEmptySlotNo = cur1stDefineCmd.Cmd01_TargetSlotNo;

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) ({7}) Cmd TargetPosition({8}) TargetSlotNo({9}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    funcName, cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #endregion

                        break;

                    default:

                        //Other Action GET,GetReady,PutReady則以Job目前的StageID為TargetPosition
                        int.TryParse(curBcsJob.RobotWIP.CurLocation_StageID, out intStageNo);
                        cur1stDefineCmd.Cmd01_TargetPosition = intStageNo;
                        //20160112 add must Use SlotBlockInfo cmdSlotNo
                        cur1stDefineCmd.Cmd01_TargetSlotNo = curRobotStageSlotBlockInfo.CurBlock_RobotCmdSlotNo;

                        //因為1st Cmd 為GET相關. 此為Source SlotNo .不佔用Target Stage LDRQ EmptySlotNo
                        cur1stCmdGetLDRQEmptySlotNo = 0;

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) {7} Cmd TargetPosition({8}) TargetSlotNo({9}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    funcName,
                                                    cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        break;
                }

                cur1stDefineCmd.Cmd01_CstSeq = curBcsJob.CassetteSequenceNo;
                cur1stDefineCmd.Cmd01_JobSeq = curBcsJob.JobSequenceNo;

                #endregion

                #region [ Check 1st Cmd TargetPosition and TargetSlotNo 不可為0 ]

                if ((cur1stDefineCmd.Cmd01_TargetPosition == 0 || cur1stDefineCmd.Cmd01_TargetSlotNo == 0))
                {
                    if (!curBcsJob.RobotWIP.RTCReworkFlag)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command targetPositon({9}) or TargetSlotNo({10}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0010 ]

                        if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command targetPositon({9}) or TargetSlotNo({10}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) StageID({3}) StepNo({4}) Action({5}) StageIDList({6}) {7} Command targetPositon({8}) or TargetSlotNo({9}) is illegal",
                            //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //                        curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                            //                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                            //                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                            //                        funcName, cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());

                            failMsg = string.Format("Job({0}_{1}) StageID({2}) StepNo({3}) Action({4}) StageIDList({5}) {6} Command targetPositon({7}) or TargetSlotNo({8}) is illegal",
                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur1stDefineCmd.Cmd01_TargetPosition.ToString(), cur1stDefineCmd.Cmd01_TargetSlotNo.ToString());

                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion

                        }

                        #endregion
                    }

                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_E0010 ]
                    RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                }


                #endregion

                //Set want To Check Function Fail_ReasonCode,以ObjectName與MethodName為Key來決定是否紀錄Log
                fail_ReasonCode = string.Format("{0}_{1}_{2}", "RobotCoreService", "CheckSlotBlockInfo_AllOrderByConditionByCommand", "2ndCommand");

                #region [ Check 2ndCmd Action and define TargetPosition and TargetSlotNo  ]

                funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;
                //NextStep由WIP來取得
                checkStepNo = curBcsJob.RobotWIP.NextStepNo;

                //0: None
                //1: Put          //2: Get          //4: Exchange
                //8: Put Ready    //16: Get Ready   //32: Get/Put
                switch (cur2ndDefindCmd.Cmd01_Command)
                {
                    case 0:  //None 2nd沒命令則直接不做Orderby回傳true

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command Action({9}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur2ndDefindCmd.Cmd01_Command.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //Clear[ Robot_Fail_Case_E0011 ] 沒有第二個命令則直接移除掉相關的Fail Code
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                        return true;

                    case 1:  //PUT
                    case 4:  //Exchange <---PUT轉化
                    case 32: //Get/PUT <---PUT轉化

                        #region [ 2ndCmd需要做OrderBy處理 :Order by 2nd Cmd LDRQ StageList ]

                        #region [ PUT/Exchange/Get&Put 需要 Check has LDRQ Stage List

                        if (curFilterLDRQStageList.Count == 0)
                        {
                            //沒有LDRQ Stage List 
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) can not Find Status is LDRQ!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            return false;
                        }

                        #endregion

                        //Order by 2nd Cmd LDRQ StageList
                        if (CheckAllOrderByConditionByStepNo(curRobot, curBcsJob, checkStepNo, cur2ndDefindCmd, ref curFilterLDRQStageList,true) == false)
                        {
                            //OrderBy失敗,會包含有target stage/slot選擇失敗，所以還是不能下命令
                            return false;
                        }

                        #region [ Get Target Position ]

                        if (cur2ndDefindCmd.Cmd01_TargetPosition == 0)
                        {
                            //不管OrderBy是否成功都得以第一個StageInfo為主
                            int.TryParse(curFilterLDRQStageList[0].Data.STAGEID, out intStageNo);
                            cur2ndDefindCmd.Cmd01_TargetPosition = intStageNo;
                        }
                        else
                        {
                            //TargetPosition已經在Order Rule裡決定
                        }

                        #endregion


                        #region Get Pre-Fetch
                        bool _canUsePreFetchFlag = false;
                        if (curRobot.Context.ContainsKey(eRobotContextParameter.CanUsePreFetchFlag))
                        {
                            _canUsePreFetchFlag = (curRobot.Context[eRobotContextParameter.CanUsePreFetchFlag].ToString() == "Y" ? true : false);
                        }
                        #endregion


                        #region [ Get Target SlotNo ]

                        if (cur2ndDefindCmd.Cmd01_TargetSlotNo == 0)
                        {
                            #region 決定 SlotNo, 將 RobotCommand 改成 Exchange 或 GetPut, 判斷 _canUsePreFetchFlag
                            //取得LDRQ Slot(預設為1)
                            if (curFilterLDRQStageList[0].File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                            {
                                //取得原Slot資訊. SendControl Command 也要補上Cst Seq與Job Seq資訊
                                cur2ndDefindCmd.Cmd01_TargetSlotNo = GetLDRQStageEmptySlotBlockInfoSlotNo(curRobot, curBcsJob, checkStepNo, curFilterLDRQStageList[0], cur1stCmdGetLDRQEmptySlotNo, true);
                            }
                            else if (cur2ndDefindCmd.Cmd01_Command == eRobot_ControlCommand.PUT && curFilterLDRQStageList[0].File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY)
                            {
                                #region  [ Cell Special Arm不支援Exchange ]
                                ////20160106 add for 新需求 MAC RecipeID最後一碼(第12碼)為"1"不可以Exchange
                                ////if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag || curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_EXCHANGE)
                                //if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag && curFilterLDRQStageList[0].MacCanNotExchangeFlag == false)
                                //{
                                //    //UDRQ and Exchange Request=>Cmd PUT change to EX=>要判斷是否可Exchange條件
                                //    cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_EXCHANGE;
                                //}
                                //else if (curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_GETPUT)
                                //{
                                //    //UDRQ and Exchange Request=>Cmd PUT change to EX =>要判斷是否可Get/Put條件
                                //    cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_GETPUT;
                                //}

                                ////Exchange TargetSlotNo 應該是UDRQ Job 的SlotNo才對 =>要判斷是否可Exchange或是Get/Put條件
                                //if (curFilterLDRQStageList[0].curUDRQ_SlotList.Count != 0)
                                //{
                                //    foreach (int slotKey in curFilterLDRQStageList[0].curUDRQ_SlotList.Keys)
                                //    {
                                //        cur2ndDefindCmd.Cmd01_TargetSlotNo = slotKey;
                                //        break;
                                //    }
                                //}
                                #endregion
                            }
                            else if (_canUsePreFetchFlag) //如果有 預取 並且 有 Put Ready 功能, 則 預設給1
                            {
                                cur2ndDefindCmd.Cmd01_TargetSlotNo = 1;
                                if (curFilterLDRQStageList[0].Data.PUTREADYFLAG.ToString().ToUpper() != "Y") cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE; //取消命令!!
                            }
                            else
                            {

                                cur2ndDefindCmd.Cmd01_TargetSlotNo = GetLDRQStageEmptySlotBlockInfoSlotNo(curRobot, curBcsJob, checkStepNo, curFilterLDRQStageList[0], cur1stCmdGetLDRQEmptySlotNo, false);
                            }
                            #endregion
                        }
                        else
                        {
                            //TargetSlotNo已經在Order Rule裡決定
                            #region 將 RobotCommand 改成 Exchange 或 GetPut, 判斷 _canUsePreFetchFlag
                            if (cur2ndDefindCmd.Cmd01_Command == eRobot_ControlCommand.PUT && curFilterLDRQStageList[0].File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY)
                            {
                                //20160106 add for 新需求 MAC RecipeID最後一碼(第12碼)為"1"不可以Exchange
                                //if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag || curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_EXCHANGE)
                                if (curFilterLDRQStageList[0].File.DownStreamExchangeReqFlag && curFilterLDRQStageList[0].MacCanNotExchangeFlag == false)
                                {
                                    //UDRQ and Exchange Request=>Cmd PUT change to EX =>要判斷是否可Exchange條件
                                    cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_EXCHANGE;
                                }
                                else if (curFilterLDRQStageList[0].Data.EXCHANGETYPE == eRobot_DB_CommandAction.ACTION_GETPUT)
                                {
                                    //UDRQ and Exchange Request=>Cmd PUT change to EX =>要判斷是否可Get/Put條件
                                    cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_GETPUT;
                                }
                            }
                            else if (_canUsePreFetchFlag) //如果有 預取 並且 有 Put Ready 功能, 則 預設給1
                            {
                                cur2ndDefindCmd.Cmd01_TargetSlotNo = 1;
                                if (curFilterLDRQStageList[0].Data.PUTREADYFLAG.ToString().ToUpper() != "Y") cur2ndDefindCmd.Cmd01_Command = eRobot_Trx_CommandAction.ACTION_NONE; //取消命令!!
                            }
                            #endregion
                        }

                        #endregion

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) ({7}) Cmd TargetPosition({8}) TargetSlotNo({9}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    funcName, cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #endregion

                        break;

                    default:

                        //Other Action GET,GetReady,PutReady 則以Job目前的StageID為TargetPosition
                        int.TryParse(curBcsJob.RobotWIP.CurLocation_StageID, out intStageNo);
                        cur2ndDefindCmd.Cmd01_TargetPosition = intStageNo;
                        //20160112 add must Use SlotBlockInfo cmdSlotNo
                        cur2ndDefindCmd.Cmd01_TargetSlotNo = curRobotStageSlotBlockInfo.CurBlock_RobotCmdSlotNo;

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) {7} Cmd TargetPosition({8}) TargetSlotNo({9}).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    funcName,
                                                    cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        break;
                }

                cur2ndDefindCmd.Cmd01_CstSeq = curBcsJob.CassetteSequenceNo;
                cur2ndDefindCmd.Cmd01_JobSeq = curBcsJob.JobSequenceNo;

                #endregion

                #region [ Check 2nd Cmd TargetPosition and TargetSlotNo 不可為0 ]

                if ((cur2ndDefindCmd.Cmd01_TargetPosition == 0 || cur2ndDefindCmd.Cmd01_TargetSlotNo == 0))
                {
                    if (!curBcsJob.RobotWIP.RTCReworkFlag)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command targetPositon({9}) or TargetSlotNo({10}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0011 ]

                        if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) {8} Command targetPositon({9}) or TargetSlotNo({10}) is illegal",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) StageID({3}) StepNo({4}) Action({5}) StageIDList({6}) {7} Command targetPositon({8}) or TargetSlotNo({9}) is illegal",
                            //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //                        curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                            //                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                            //                        curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                            //                        funcName, cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());

                            failMsg = string.Format("Job({0}_{1}) StageID({2}) StepNo({3}) Action({4}) StageIDList({5}) {6} Command targetPositon({7}) or TargetSlotNo({8}) is illegal",
                                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curBcsJob.RobotWIP.CurLocation_StageID, curBcsJob.RobotWIP.CurStepNo.ToString(),
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                    curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                    funcName, cur2ndDefindCmd.Cmd01_TargetPosition.ToString(), cur2ndDefindCmd.Cmd01_TargetSlotNo.ToString());

                            AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion

                        }

                        #endregion
                    }

                    return false;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_E0011 ]
                    RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
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




        //For BCS Use Function List -=======================================================================================================================================

        /// <summary> 建立Job Robot WIP Relation Infomation
        ///
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <param name="curBcsJob"></param>
        /// <returns></returns>
        public bool CreateJobRobotWIPInfo(string eqpNo, Job curBcsJob, ref string errMsg)
        {

            string strlog = string.Empty;
            List<string> canUseRouteIDList = new List<string>();
            string canUseRouteID = string.Empty;
            errMsg = string.Empty;

            try
            {

                #region [ Get Robot Entity ]
                if (!_robotServiceIsEnable)
                    return false;

                //Watson Add 20151021 For On Line Validate Cassette Reply Will Create Robot WIP But No EveryLine has Robot. 
                //If No Robot don't show error.
                if (ObjectManager.RobotManager.GetRobots().Count <= 0)
                    return false;


                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);

                if (curRobot == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Can not Find Robot Entity by EQPNo({1}) !", eqpNo, eqpNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //For Special Line Robot屬於L2 但是上貨的Port在其他Node找不到,所以透過ServerName來取得
                    curRobot = ObjectManager.RobotManager.GetRobotbySeverName(Workbench.ServerName);

                    if (curRobot == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Can not Find Robot by ServerName({1}) !", eqpNo, Workbench.ServerName);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        errMsg = string.Format("Can not Find Robot by ServerName({1}) !", eqpNo, Workbench.ServerName);
                        return false;
                    }

                }

                #endregion

                #region [ Get Can Use RouteID ]

                ////Get Can Use RouteID List
                canUseRouteIDList = GetCanUseRouteIDList(eqpNo, curRobot, curBcsJob);

                #region [ Check Can Use RouteID is Exist ]

                if (canUseRouteIDList == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get Can Use Route!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    errMsg = string.Format("Robot({1}) can not Get Can Use Route!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                    return false;
                }

                if (canUseRouteIDList.Count == 0)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get Can Use Route!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    errMsg = string.Format("Robot({1}) can not Get Can Use Route!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                    return false;

                }

                #endregion

                #region [ Get Can Use RouteID ]

                if (canUseRouteIDList.Count != 1)
                {
                    //[ Wait_For_Proc_00023 ] 當有多組Route符合條件時 如何排序
                    canUseRouteID = canUseRouteIDList[0];
                }
                else
                {
                    canUseRouteID = canUseRouteIDList[0];
                }

                #endregion

                #endregion

                #region Create New RobotWIP

                //lock (curBcsJob)  //會在CELL下貨時(600 slot)花費太多的時間
                //{

                //BCS呼叫此Function表示下貨完成等候抽片 
                curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.WAIT_PROC;
                //Step從1開始
                curBcsJob.RobotWIP.CurStepNo = 1;
                curBcsJob.RobotWIP.CurLocation_StageID = curBcsJob.SourcePortID;
                curBcsJob.RobotWIP.LastInPutTrackingData = new string('0', 32);
                curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData = new string('0', 32);
                //Watson Add 20151208 For CF Tracking Data 16 Bit
                if (Workbench.ServerName.Length > 2)
                {
                    if (Workbench.ServerName.Substring(0, 2) == "FC")
                    {
                        curBcsJob.RobotWIP.LastInPutTrackingData = new string('0', 16);
                        curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData = new string('0', 16);
                        #region For CF MQC
                        if ((Workbench.LineType == eLineType.CF.FCMQC_TYPE1) || (Workbench.LineType == eLineType.CF.FCMQC_TYPE2))
                        {
                            if (curBcsJob.CfSpecial.FlowPriorityInfo.Trim() == string.Empty)  //沒空表示Offline下貨
                            {
                                SetCFMQC_DefaultInspecPriority(curRobot, curBcsJob.SourcePortID);
                                if (curRobot.File.CurMQCPortDefaultInspPriority.ContainsKey(curBcsJob.SourcePortID))
                                    curBcsJob.CfSpecial.FlowPriorityInfo = FlowPriority(curRobot.File.CurMQCPortDefaultInspPriority[curBcsJob.SourcePortID]);
                                else
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] RobotJob Job({1},{2}) Current MQC Port[{3}] Default Inspection Priority is Empty!",
                                        eqpNo, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.SourcePortID);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                            }
                        }
                        #endregion
                    }
                }

                //Set Robot Route by can Use RouteID
                curBcsJob.RobotWIP.CurRouteID = canUseRouteID;
                //Ste Job This RouteID all StepInfo
                curBcsJob.RobotWIP.RobotRouteStepList = ObjectManager.RobotManager.GetRouteStepList(curRobot.Data.ROBOTNAME, canUseRouteID);

                //Watson Add 20151005 For TTP DailyCheck Route 已不使用
                //curBcsJob.RobotWIP.DailyCheckRouteStepList = ObjectManager.RobotManager.GetDailyCheckRouteStepList(curRobot.Data.ROBOTNAME, canUseRouteID);
                curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG = curBcsJob.EQPFlag;   //Watson Add 20151027 update 
                curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPRESERVATIONS = curBcsJob.EQPReservations;

                #region [ 判斷STEP設定不可以少於2步 ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.Count < 2)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Robot({1}) Can Use RouteID({2}) get RouteStepList Count({3}) is illegal!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, canUseRouteID, curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString());
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    errMsg = string.Format("Robot({1}) Can Use RouteID({2}) get RouteStepList Count({3}) is illegal!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, canUseRouteID, curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString()); ;
                    return false;

                }

                #endregion

                //[ Wait_For_Proc_00022 ] 建立RobotWIP相關資訊
                //robotJob.NotRecipeByPassStepNo = 0;

                #region [ Get Current 1stStep Entity ]

                RobotRouteStep cur1stRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                //找不到 CurStep Route 回NG
                if (cur1stRouteStep == null)
                {
                    #region[DebugLog]
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    #endregion
                    errMsg = string.Format("Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString()); ;
                    return false;
                }

                #endregion

                //set Next Step
                curBcsJob.RobotWIP.NextStepNo = cur1stRouteStep.Data.NEXTSTEPID;


                //                }

                //Save File  Watson 20151106 Modify 因為外部使用此Function都會存檔了，所以在此不用再存檔!!
                //ObjectManager.JobManager.EnqueueSave(curBcsJob);

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] RobotJob Job({1},{2}) is Create! Add RouteID({3}) RouteStepCount({4}) !",
                                            eqpNo, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, canUseRouteID,
                                            curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString());
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #endregion

                return true;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                errMsg = ex.ToString();
                return false;
            }

        }
        public bool CreateJobRobotWIPInfo(string eqpNo, Job curBcsJob, ref string errMsg, string routeID, int curStepID, int nextStepID)
        {
            string strlog = string.Empty;
            List<string> canUseRouteIDList = new List<string>();
            string CurLocation_StageID = string.Empty;
            errMsg = string.Empty;

            try
            {

                #region [ Get Robot Entity ]
                if (!_robotServiceIsEnable)
                    return false;

                //Watson Add 20151021 For On Line Validate Cassette Reply Will Create Robot WIP But No EveryLine has Robot. 
                //If No Robot don't show error.
                if (ObjectManager.RobotManager.GetRobots().Count <= 0)
                    return false;


                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);

                if (curRobot == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Can not Find Robot Entity by EQPNo({1}) !", eqpNo, eqpNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //For Special Line Robot屬於L2 但是上貨的Port在其他Node找不到,所以透過ServerName來取得
                    curRobot = ObjectManager.RobotManager.GetRobotbySeverName(Workbench.ServerName);

                    if (curRobot == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Can not Find Robot by ServerName({1}) !", eqpNo, Workbench.ServerName);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        errMsg = string.Format("Can not Find Robot by ServerName({1}) !", eqpNo, Workbench.ServerName);
                        return false;
                    }

                }

                #endregion

                #region Create New RobotWIP

                curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.INPROCESS;

                curBcsJob.RobotWIP.CurStepNo = curStepID;

                #region Tracking data
                //Watson Add 20151208 For CF Tracking Data 16 Bit
                if (Workbench.ServerName.Length > 2)
                {
                    if (Workbench.ServerName.Substring(0, 2) == "FC")
                    {
                        curBcsJob.RobotWIP.LastInPutTrackingData = curBcsJob.TrackingData.Substring(0, 16);
                        curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData = curBcsJob.TrackingData.Substring(0, 16);
                        #region For CF MQC
                        if ((Workbench.LineType == eLineType.CF.FCMQC_TYPE1) || (Workbench.LineType == eLineType.CF.FCMQC_TYPE2))
                        {
                            if (curBcsJob.CfSpecial.FlowPriorityInfo.Trim() == string.Empty)  //沒空表示Offline下貨
                            {
                                SetCFMQC_DefaultInspecPriority(curRobot, curBcsJob.SourcePortID);
                                if (curRobot.File.CurMQCPortDefaultInspPriority.ContainsKey(curBcsJob.SourcePortID))
                                    curBcsJob.CfSpecial.FlowPriorityInfo = FlowPriority(curRobot.File.CurMQCPortDefaultInspPriority[curBcsJob.SourcePortID]);
                                else
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] RobotJob Job({1},{2}) Current MQC Port[{3}] Default Inspection Priority is Empty!",
                                        eqpNo, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.SourcePortID);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {

                        curBcsJob.RobotWIP.LastInPutTrackingData = curBcsJob.TrackingData.Substring(0, 32);
                        curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData = curBcsJob.TrackingData.Substring(0, 32);

                    }
                }
                #endregion
                //Set Robot Route by can Use RouteID
                curBcsJob.RobotWIP.CurRouteID = routeID;
                //Ste Job This RouteID all StepInfo
                curBcsJob.RobotWIP.RobotRouteStepList = ObjectManager.RobotManager.GetRouteStepList(curRobot.Data.ROBOTNAME, routeID);
                #region [ Check STEP設定不可以少於2步 ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.Count < 2)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Robot({1}) Can Use RouteID({2}) get RouteStepList Count({3}) is illegal!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, routeID, curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString());
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    errMsg = string.Format("Robot({1}) Can Use RouteID({2}) get RouteStepList Count({3}) is illegal!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, routeID, curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString()); ;
                    return false;

                }

                #endregion
                #region [ Get Current Step Entity ]

                RobotRouteStep curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                //找不到 CurStep Route 回NG
                if (curRouteStep == null)
                {
                    #region[DebugLog]
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    #endregion
                    errMsg = string.Format("Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString()); ;
                    return false;
                }

                #endregion
                //set Next Step
                if (nextStepID > curStepID)
                    curBcsJob.RobotWIP.NextStepNo = nextStepID;
                else
                    curBcsJob.RobotWIP.NextStepNo = curRouteStep.Data.NEXTSTEPID;
                #region 获取stageID
                string[] stageIDList = curRouteStep.Data.STAGEIDLIST.Split(',');
                for (int i = 0; i < stageIDList.Length; i++)
                {
                    RobotStage rStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageIDList[i]);
                    if (string.IsNullOrEmpty(curBcsJob.CurrentUNITNo))//job 没有UNITNo ，则只比较NodeNO
                    {
                        if (curBcsJob.CurrentEQPNo == rStage.Data.UPSTREAMPATHTRXNAME.Substring(0, 2))
                        {
                            CurLocation_StageID = stageIDList[i]; break;
                        }
                    }
                    else
                    {//若job 含有UNITNo ，则比较NodeNO和 UnitNO

                        if (curBcsJob.CurrentEQPNo == rStage.Data.UPSTREAMPATHTRXNAME.Substring(0, 2) && curBcsJob.CurrentUNITNo == rStage.Data.UPSTREAMPATHTRXNAME.Substring(rStage.Data.UPSTREAMPATHTRXNAME.Length - 1, 1))
                            CurLocation_StageID = stageIDList[i]; break;
                    }

                }
                #endregion
                curBcsJob.RobotWIP.CurLocation_StageID = CurLocation_StageID;

                //Watson Add 20151005 For TTP DailyCheck Route 已不使用
                //curBcsJob.RobotWIP.DailyCheckRouteStepList = ObjectManager.RobotManager.GetDailyCheckRouteStepList(curRobot.Data.ROBOTNAME, canUseRouteID);
                curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG = curBcsJob.EQPFlag;
                curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPRESERVATIONS = curBcsJob.EQPReservations;
                //[ Wait_For_Proc_00022 ] 建立RobotWIP相關資訊
                //robotJob.NotRecipeByPassStepNo = 0;
                //Save File  Watson 20151106 Modify 因為外部使用此Function都會存檔了，所以在此不用再存檔!!
                ObjectManager.JobManager.EnqueueSave(curBcsJob);

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] RobotJob Job({1},{2}) is Create! Add RouteID({3}) RouteStepCount({4}) !",
                                            eqpNo, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, routeID,
                                            curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString());
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #endregion

                return true;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                errMsg = ex.ToString();
                return false;
            }

        }
        //20160603
        public bool CreateAbnormalForceCleanOutJobRobotWIPInfo(string eqpNo, Job curBcsJob, int curStepID, int nextStepID, ref string errMsg)
        {

            string strlog = string.Empty;
            List<string> canUseRouteIDList = new List<string>();
            string canUseRouteID = string.Empty;
            errMsg = string.Empty;

            try
            {

                #region [ Get Robot Entity ]
                if (!_robotServiceIsEnable)
                    return false;

                //Watson Add 20151021 For On Line Validate Cassette Reply Will Create Robot WIP But No EveryLine has Robot. 
                //If No Robot don't show error.
                if (ObjectManager.RobotManager.GetRobots().Count <= 0)
                    return false;


                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);

                if (curRobot == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Can not Find Robot Entity by EQPNo({1}) !", eqpNo, eqpNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    //For Special Line Robot屬於L2 但是上貨的Port在其他Node找不到,所以透過ServerName來取得
                    curRobot = ObjectManager.RobotManager.GetRobotbySeverName(Workbench.ServerName);

                    if (curRobot == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Can not Find Robot by ServerName({1}) !", eqpNo, Workbench.ServerName);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        errMsg = string.Format("Can not Find Robot by ServerName({1}) !", eqpNo, Workbench.ServerName);
                        return false;
                    }

                }

                #endregion

                #region [ Get Can Use RouteID ]

                ////Get Can Use RouteID List
                //canUseRouteIDList = GetCanUseRouteIDList(eqpNo, curRobot, curBcsJob);

                //#region [ Check Can Use RouteID is Exist ]

                //if (canUseRouteIDList == null)
                //{
                //    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get Can Use Route!",
                //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                //    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //    errMsg = string.Format("Robot({1}) can not Get Can Use Route!",
                //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                //    return false;
                //}

                //if (canUseRouteIDList.Count == 0)
                //{
                //    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get Can Use Route!",
                //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                //    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //    errMsg = string.Format("Robot({1}) can not Get Can Use Route!",
                //                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                //    return false;

                //}

                //#endregion

                //#region [ Get Can Use RouteID ]

                //if (canUseRouteIDList.Count != 1)
                //{
                //    //[ Wait_For_Proc_00023 ] 當有多組Route符合條件時 如何排序
                //    canUseRouteID = canUseRouteIDList[0];
                //}
                //else
                //{
                //    canUseRouteID = canUseRouteIDList[0];
                //}

                //#endregion

                #endregion
                canUseRouteID = "AbnormalForceCleanOut";

                #region Create New RobotWIP

                //lock (curBcsJob)  //會在CELL下貨時(600 slot)花費太多的時間
                //{

                //BCS呼叫此Function表示下貨完成等候抽片 
                curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.WAIT_PROC;
                //Step從1開始
                curBcsJob.RobotWIP.CurStepNo = curStepID;
                curBcsJob.RobotWIP.CurLocation_StageID = curBcsJob.SourcePortID;
                curBcsJob.RobotWIP.LastInPutTrackingData = new string('0', 32);
                curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData = new string('0', 32);
                //Watson Add 20151208 For CF Tracking Data 16 Bit
                if (Workbench.ServerName.Length > 2)
                {
                    if (Workbench.ServerName.Substring(0, 2) == "FC")
                    {
                        curBcsJob.RobotWIP.LastInPutTrackingData = new string('0', 16);
                        curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData = new string('0', 16);
                        #region For CF MQC
                        if ((Workbench.LineType == eLineType.CF.FCMQC_TYPE1) || (Workbench.LineType == eLineType.CF.FCMQC_TYPE2))
                        {
                            if (curBcsJob.CfSpecial.FlowPriorityInfo.Trim() == string.Empty)  //沒空表示Offline下貨
                            {
                                SetCFMQC_DefaultInspecPriority(curRobot, curBcsJob.SourcePortID);
                                if (curRobot.File.CurMQCPortDefaultInspPriority.ContainsKey(curBcsJob.SourcePortID))
                                    curBcsJob.CfSpecial.FlowPriorityInfo = FlowPriority(curRobot.File.CurMQCPortDefaultInspPriority[curBcsJob.SourcePortID]);
                                else
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] RobotJob Job({1},{2}) Current MQC Port[{3}] Default Inspection Priority is Empty!",
                                        eqpNo, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.SourcePortID);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                            }
                        }
                        #endregion
                    }
                }

                //Set Robot Route by can Use RouteID
                curBcsJob.RobotWIP.CurRouteID = canUseRouteID;
                //Ste Job This RouteID all StepInfo
                curBcsJob.RobotWIP.RobotRouteStepList = ObjectManager.RobotManager.GetRouteStepList(curRobot.Data.ROBOTNAME, canUseRouteID);

                //Watson Add 20151005 For TTP DailyCheck Route 已不使用
                //curBcsJob.RobotWIP.DailyCheckRouteStepList = ObjectManager.RobotManager.GetDailyCheckRouteStepList(curRobot.Data.ROBOTNAME, canUseRouteID);
                curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG = curBcsJob.EQPFlag;   //Watson Add 20151027 update 
                curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPRESERVATIONS = curBcsJob.EQPReservations;

                #region [ 判斷STEP設定不可以少於2步 ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.Count < 2)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] Robot({1}) Can Use RouteID({2}) get RouteStepList Count({3}) is illegal!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, canUseRouteID, curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString());
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    errMsg = string.Format("Robot({1}) Can Use RouteID({2}) get RouteStepList Count({3}) is illegal!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, canUseRouteID, curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString()); ;
                    return false;

                }

                #endregion

                //[ Wait_For_Proc_00022 ] 建立RobotWIP相關資訊
                //robotJob.NotRecipeByPassStepNo = 0;

                #region [ Get Current 1stStep Entity ]

                RobotRouteStep cur1stRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                //找不到 CurStep Route 回NG
                if (cur1stRouteStep == null)
                {
                    #region[DebugLog]
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    #endregion
                    errMsg = string.Format("Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString()); ;
                    return false;
                }

                #endregion

                //set Next Step
                curBcsJob.RobotWIP.NextStepNo = nextStepID;


                //                }

                //Save File  Watson 20151106 Modify 因為外部使用此Function都會存檔了，所以在此不用再存檔!!
                //ObjectManager.JobManager.EnqueueSave(curBcsJob);

                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RCS] RobotJob Job({1},{2}) is Create! Add RouteID({3}) RouteStepCount({4}) !",
                                            eqpNo, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, canUseRouteID,
                                            curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString());
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                #endregion

                return true;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                errMsg = ex.ToString();
                return false;
            }

        }
        private List<string> GetCanUseRouteIDList(string eqpNo, Robot curRobot, Job curBcsJob)
        {
            string strlog = string.Empty;
            List<string> getRouteIDList = new List<string>();
            List<string> RouteCreateFaildMessage = new List<string>(); //Watson add 20160226
            try
            {
                Dictionary<string, List<RobotRouteCondition>> curRouteIDs_Conditions = ObjectManager.RobotManager.GetRouteConditionsByRobotName(curRobot.Data.ROBOTNAME);

                #region [ Check Robot Route Condition Exist ]

                if (curRouteIDs_Conditions == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not find Route Condition Setting!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return null;

                }

                if (curRouteIDs_Conditions.Count == 0)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Route Condition Conut is 0!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    return null;

                }

                #endregion

                #region [ Get Check Success RouteIDList ]

                foreach (string routeID in curRouteIDs_Conditions.Keys)
                {
                    bool checkFlag = false;
                    IRobotContext robotConText = new RobotContext();
                    string fail_ReasonCode = string.Empty;
                    string failMsg = string.Empty;
                    bool checkRouteIDFlag = true;
                    List<RobotRouteCondition> curRouteIDConditions = curRouteIDs_Conditions[routeID];

                    #region [ Initial Rule Route Condition List RobotConText Info. 搭配針對Rule Condition會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================
                    robotConText.AddParameter(eRobotContextParameter.EquipmentNo, eqpNo);
                    robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                    robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);

                    #endregion =======================================================================================================================================================

                    #region[DebugLog][ Start Check RouteID Route Condition Function ]

                    //if (IsShowDetialLog == true)
                    //{

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Check RouteID({2}) Conditions Start. {3}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, routeID, new string('=', 160));

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    //}

                    #endregion

                    #region [ 根據Route Condition判斷是否符合這個RouteID ]

                    foreach (RobotRouteCondition curCondition in curRouteIDConditions)
                    {

                        //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_E0002 ] ,以Route Condition 的ObjectName與MethodName為Key來決定是否紀錄Log
                        fail_ReasonCode = string.Format("{0}_{1}", curCondition.Data.OBJECTNAME, curCondition.Data.METHODNAME);

                        #region[DebugLog][ Start Route Condition Function ]

                        //if (IsShowDetialLog == true)
                        //{

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Route Condition object({2}) MethodName({3}) IsEnable({4}) Start {5}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curCondition.Data.OBJECTNAME, curCondition.Data.METHODNAME,
                                                curCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_START_CHAR, eRobotCommonConst.RULE_SELECT_START_CHAR_LENGTH));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        //}

                        #endregion

                        if (curCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                        {
                            checkFlag = (bool)Invoke(curCondition.Data.OBJECTNAME, curCondition.Data.METHODNAME, new object[] { robotConText });

                            if (checkFlag == false)
                            {

                                #region[DebugLog][ End Route Condition Function ]

                                //if (IsShowDetialLog == true)
                                //{

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Route Condition Fail, object({2}) MethodName({3}) RtnCode({4})  RtnMsg({5}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curCondition.Data.OBJECTNAME, curCondition.Data.METHODNAME,
                                                        robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                //Watson Add 20160227 For Failed Reason
                                RouteCreateFaildMessage.Add(robotConText.GetReturnMessage());

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Route Condition object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curCondition.Data.OBJECTNAME, curCondition.Data.METHODNAME,
                                                        curCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                //}

                                #endregion

                                #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_E0002 ]

                                if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Route Condition Fail, object({2}) MethodName({3}) RtnCode({4})  RtnMsg({5}]!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curCondition.Data.OBJECTNAME, curCondition.Data.METHODNAME,
                                                            robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                    //failMsg = string.Format("Robot({0}) object({1}) MethodName({2}) RtnCode({3})  RtnMsg({4}]!",
                                    //                        curRobot.Data.ROBOTNAME, curCondition.Data.OBJECTNAME, curCondition.Data.METHODNAME, robotConText.GetReturnCode(),
                                    //                        robotConText.GetReturnMessage());

                                    failMsg = string.Format("RtnCode({0})  RtnMsg({1})!",
                                                            robotConText.GetReturnCode(),
                                                            robotConText.GetReturnMessage());
                                    //既然能找到Route，就不需要把其他RouteCondition失败的原因show出来
                                    // AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                    //Watson Modify 20151021 For Robot RouteconditionService 不要記Fail 改成check result
                                    //SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                    #endregion

                                }

                                #endregion

                                //有重大異常直接結束本RouteID Check跳到下一個RouteID處理
                                checkRouteIDFlag = false;
                                break;
                            }
                            else
                            {
                                //Clear[ Robot_Fail_Case_E0002 ]
                               // RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                                #region[DebugLog][ End Rule Job Select Function ]

                                //if (IsShowDetialLog == true)
                                //{

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Route Condition object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curCondition.Data.OBJECTNAME, curCondition.Data.METHODNAME,
                                                        curCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                //}

                                #endregion

                            }
                        }
                        else
                        {

                            #region[DebugLog][ End Rule Job Select Function ]

                            //if (IsShowDetialLog == true)
                            //{

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curCondition.Data.OBJECTNAME, curCondition.Data.METHODNAME,
                                                    curCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            //}

                            #endregion

                        }

                    }

                    #endregion

                    if (checkRouteIDFlag == true)
                    {
                        if (getRouteIDList.Contains(routeID) == false)
                        {
                            getRouteIDList.Add(routeID);

                        }
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Check RouteID({2}) Conditions Success.",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, routeID);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #region[DebugLog][ End Check RouteID Route Condition Function ]

                    //if (IsShowDetialLog == true)
                    //{

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Check RouteID({2}) Conditions End. {3}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, routeID, new string('=', 160));

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    //}

                    #endregion

                }

                #endregion
                #region Watson Add 20160226 [For Router Create Failed Popu OPI Message.]
                if (getRouteIDList.Count <= 0)
                {
                    string trxID = CreateTrxID();
                    string err = "Get Robot Route Failed !! No any Route can be use , Beaucse =  \r\n";
                    foreach (string failedmsg in RouteCreateFaildMessage)
                        err += failedmsg + " ; \r\n";
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trxID, curRobot.Data.LINEID, err });
                }
                #endregion

                return getRouteIDList;

            }
            catch (Exception ex)
            {

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return getRouteIDList;
            }


        }





        //For Type I Normal Robot Arm Use Function List 目前不使用. TypeI改用CheckRobotControlCommand_For_TypeI_ForGetGetPutPut======================================================

        /// <summary> for Robot TypeI[ One Robot has 2 Arm,Arm#01(Upper),Arm#02(Lower) ,One Arm has One Job Position.
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotAllStageList"></param>
        private void CheckRobotControlCommand_For_TypeI(Robot curRobot, List<RobotStage> curRobotAllStageList)
        {

            string strlog = string.Empty;
            List<Job> robotArmCanControlJobList_OrderBy = new List<Job>();
            List<Job> robotStageCanControlJobList_OrderBy = new List<Job>();
            try
            {

                #region [ 1. Check Can Issue Command ]
                if (!CheckCanIssueRobotCommand(curRobot)) return;
                #endregion

                #region [ 2. Get Arm Can Control Job List, Stage Can Control Job List and Update StageInfo ]

                //One Robot Only One Select Rule,如有MIX Route則在Check FetchOut與Filter後 先照Route Priority排序再照STEP排序 以達到優先處理XX Route.如有其他特殊選片邏輯在特別處理
                #region [ Handle Robot Current Rule Job Select Function List ]

                #region [ Clear All Stage UDRQ And LDRQ Stage SlotNoList Info ]

                foreach (RobotStage stageItem in curRobotAllStageList)
                {
                    lock (stageItem)
                    {
                        stageItem.curLDRQ_EmptySlotList.Clear();
                        stageItem.curUDRQ_SlotList.Clear();
                    }
                }

                #endregion

                Dictionary<string, List<RobotRuleSelect>> curRuleJobSelectList = ObjectManager.RobotManager.GetRouteSelect(curRobot.Data.ROBOTNAME);

                bool checkFlag = false;
                IRobotContext robotConText = new RobotContext();
                curRobot.Context = robotConText;
                string fail_ReasonCode = string.Empty;
                string failMsg = string.Empty;

                #region [ Check Select Rule Exist ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00010 ] 
                fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_SELECTRULE_IS_NULL;

                if (curRuleJobSelectList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any Select Rule!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00010 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any cSelect Rule!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) can not get any Select Rule!",
                        //                         curRobot.Data.ROBOTNAME);

                        failMsg = string.Format("Can not get any Select Rule!");

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

                //此時Robot無法得知要跑哪種Route,所以只會有一筆
                foreach (string routeID in curRuleJobSelectList.Keys)
                {

                    #region [ 根據RuleJobSelect選出Can Control Job List ]

                    #region [ Initial Select Rule List RobotConText Info. 搭配針對Select Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                    robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                    robotConText.AddParameter(eRobotContextParameter.CurRobotAllStageListEntity, curRobotAllStageList);

                    #endregion =======================================================================================================================================================

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

                                    failMsg = string.Format("RtnCode({0})  RtnMsg({1})!",
                                                            robotConText.GetReturnCode(),
                                                            robotConText.GetReturnMessage());

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

                    //目前只處理第一筆
                    break;

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

                #region [ 3. Update OPI Stage Display Info ]

                bool sendToOPI = false;

                foreach (RobotStage stage_entity in curRobotAllStageList)
                {
                    if (stage_entity.File.StatusChangeFlag == true)
                    {
                        sendToOPI = true;

                        lock (stage_entity.File)
                        {
                            stage_entity.File.StatusChangeFlag = false;
                        }
                    }

                }

                if (sendToOPI == true)
                {
                    //通知OPI更新LayOut畫面 //20151126 add by Robot Arm Qty來區分送給OPI的狀態訊息
                    Invoke(eServiceName.UIService, "RobotStageInfoReport", new object[] { curRobot.Data.LINEID, curRobot });
                }

                #endregion

                #region [ 如果是SEMI Mode只需做到取得目前可控制Job並更新資訊即可 ]

                if (curRobot.File.curRobotRunMode == eRobot_RunMode.SEMI_MODE)
                {
                    return;
                }

                #endregion

                #region [ 更新OPI畫面後 Check Can Control Job Exist ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00006 ] 
                fail_ReasonCode = eRobot_CheckFail_Reason.GET_CAN_CONTROL_JOB_FAIL;

                if (robotArmCanControlJobList.Count == 0 && robotStageCanControlJobList.Count == 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any can control Job!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00006 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any can control Job!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) can not get any can control Job!",
                        //                         curRobot.Data.ROBOTNAME);

                        failMsg = string.Format("Can not get any can control Job!");

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    return;

                }
                else
                {
                    //Clear[ Robot_Fail_Case_00006 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                #region [ Handle Robot Arm Job List First ]

                if (robotArmCanControlJobList.Count != 0)
                {
                    //排序 以Step越小, PortStatus In_Prcess為優先處理
                    robotArmCanControlJobList_OrderBy = robotArmCanControlJobList.OrderByDescending(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();

                    foreach (Job curRobotArmJob in robotArmCanControlJobList_OrderBy)
                    {
                        DefineNormalRobotCmd cur1stDefindCmd = null, cur2ndDefindCmd = null;
                        if (CheckRobotArmJobRouteCondition(curRobot, curRobotArmJob, out cur1stDefindCmd, out cur2ndDefindCmd) == true)
                        {
                            RobotCmdInfo curRobotCommand = NewRobotCmdInfo(cur1stDefindCmd, cur2ndDefindCmd);
                            bool sendCmdResult = false;
                            if (curRobotCommand != null)
                                sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curRobotCommand });
                            if (sendCmdResult)
                                return;//命令寫入成功 則離開
                        }
                    }
                }
                else
                {
                    robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();

                    if (Workbench.LineType == eLineType.ARRAY.CVD_ULVAC || Workbench.LineType == eLineType.ARRAY.CVD_AKT)
                    {
                        //Watson Add CVD 20151001
                        Invoke("RobotSpecialService", "CheckCVDProportionalType", new object[] { curRobot, robotStageCanControlJobList_OrderBy });
                    }

                    foreach (Job curRobotStageJob in robotStageCanControlJobList_OrderBy)
                    {
                        DefineNormalRobotCmd cur1stDefindCmd = null, cur2ndDefindCmd = null;
                        if (CheckRobotStageJobRouteCondition(curRobot, curRobotStageJob, out cur1stDefindCmd, out cur2ndDefindCmd) == true)
                        {
                            RobotCmdInfo curRobotCommand = NewRobotCmdInfo(cur1stDefindCmd, cur2ndDefindCmd);
                            bool sendCmdResult = false;
                            if (curRobotCommand != null) sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curRobotCommand });
                            if (sendCmdResult) return; //命令寫入成功 則離開
                        }
                    }

                    #region [ 判斷Pre-Fetch功能 ]
                    robotStageCanControlJobList_OrderBy = robotStageCanControlJobList_OrderBy.Where(s => s.RobotWIP.CurLocation_StageType == eStageType.PORT).ToList();
                    if (robotStageCanControlJobList_OrderBy.Count() > 0)
                    {
                        RobotStage _curStage = null;
                        foreach (Job curRobotStageJob in robotStageCanControlJobList_OrderBy)
                        {
                            _curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curRobotStageJob.RobotWIP.CurLocation_StageID);

                            if (_curStage.Data.PREFETCHFLAG.ToString().ToUpper() == "N") continue; //如果為N, 就是沒有開啟Pre-Fetch功能!
                            curRobot.Context.AddParameter(eRobotContextParameter.CanUsePreFetchFlag, _curStage.Data.PREFETCHFLAG.ToString().ToUpper());

                            DefineNormalRobotCmd cur1stDefindCmd = null, cur2ndDefindCmd = null;
                            if (CheckRobotStageJobRouteCondition(curRobot, curRobotStageJob, out cur1stDefindCmd, out cur2ndDefindCmd))
                            {
                                RobotCmdInfo curRobotCommand = NewRobotCmdInfo(cur1stDefindCmd, cur2ndDefindCmd);
                                bool sendCmdResult = false;
                                if (curRobotCommand != null) sendCmdResult = (bool)Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curRobotCommand });
                                if (sendCmdResult) return; //命令寫入成功 則離開
                            }
                        }
                    }
                    #endregion
                }
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



        /// <summary> Check Arm上Job 目前Step的所有Filter條件是否成立
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotArmJob"></param>
        /// <returns></returns>
        private bool CheckRobotArmJobRouteCondition(Robot curRobot, Job curRobotArmJob, out DefineNormalRobotCmd cur1stDefindCmd, out DefineNormalRobotCmd cur2ndDefindCmd)
        {
            string strlog = string.Empty;
            List<RobotStage> curFilterStageList = new List<RobotStage>();
            cur1stDefindCmd = cur2ndDefindCmd = null;
            try
            {

                #region [ 20151015 add Check CurStep RouteStepByPass Condition and 準備變更curStep ]

                if (CheckAllRouteStepByPassCondition(curRobot, curRobotArmJob, curRobotArmJob.RobotWIP.CurStepNo, ref curFilterStageList) == false)
                {
                    //StepByPass條件有問題則回覆NG
                    return false;
                }

                #endregion

                #region [ 20151017 add Check CurStep All RouteStepJump Condition and 準備變更curStep ]

                if (CheckAllRouteStepJumpCondition(curRobot, curRobotArmJob, curRobotArmJob.RobotWIP.CurStepNo, ref curFilterStageList) == false)
                {
                    //StageSelect條件有問題則回覆NG
                    return false;
                }

                #endregion

                #region [ Check CurStep All Filter Condition ]

                cur1stDefindCmd = new DefineNormalRobotCmd();
                cur2ndDefindCmd = new DefineNormalRobotCmd();

                //Arm Job Only Check curStep Filter
                if (CheckAllFilterConditionByStepNo(curRobot, curRobotArmJob, curRobotArmJob.RobotWIP.CurStepNo, cur1stDefindCmd, cur2ndDefindCmd, ref curFilterStageList) == false)
                {
                    //Filter條件有問題則回覆NG
                    return false;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) StageID({4}) StepNo({5}) Action({6}) StageIDList({7}) Command Action({9}) is illegal",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobotArmJob.CassetteSequenceNo, curRobotArmJob.JobSequenceNo,
                                                curRobotArmJob.RobotWIP.CurLocation_StageID, curRobotArmJob.RobotWIP.CurStepNo.ToString(),
                                                curRobotArmJob.RobotWIP.RobotRouteStepList[curRobotArmJob.RobotWIP.CurStepNo].Data.ROBOTACTION,
                                                curRobotArmJob.RobotWIP.RobotRouteStepList[curRobotArmJob.RobotWIP.CurStepNo].Data.STAGEIDLIST,
                                                cur1stDefindCmd.Cmd01_Command.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return false;
                }

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




                    #region [ 20151025 add Check Multi-Single Condition ] 20151102 Mark .只有對Mulit Type EQP 存取2片時才會下Multi命令

                    ////Check 1st Command
                    //CheckMultiSingleCommandCondition(curRobot, curRobotArmJob, cur1stDefindCmd, false);
                    ////Check 2nd Command
                    //CheckMultiSingleCommandCondition(curRobot, curRobotArmJob, cur2ndDefindCmd, true);

                    #endregion
                }
                else
                {
                    //Create 1 Arm 2 Substrate
                    //[ Wait_For_Proc_00027 ] Arm Job 針對  1Arm 2Job處理
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

        /// <summary> Check Stage上Job 目前Step的所有Filter條件是否成立
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotStageJob"></param>
        /// <returns></returns>
        private bool CheckRobotStageJobRouteCondition(Robot curRobot, Job curRobotStageJob, out DefineNormalRobotCmd cur1stDefindCmd, out DefineNormalRobotCmd cur2ndDefindCmd)
        {
            string strlog = string.Empty;
            List<RobotStage> curFilterStageList = new List<RobotStage>();
            cur1stDefindCmd = cur2ndDefindCmd = null;
            try
            {
                #region [ 20151015 add Check CurStep RouteStepByPass Condition and 準備變更curStep ]

                if (CheckAllRouteStepByPassCondition(curRobot, curRobotStageJob, curRobotStageJob.RobotWIP.CurStepNo, ref curFilterStageList) == false)
                {
                    //StepByPass條件有問題則回覆NG
                    return false;
                }

                #endregion

                #region [ 20151017 add Check CurStep RouteStepJump Condition and 準備變更curStep ]

                if (CheckAllRouteStepJumpCondition(curRobot, curRobotStageJob, curRobotStageJob.RobotWIP.CurStepNo, ref curFilterStageList) == false)
                {
                    //StepJump條件有問題則回覆NG
                    return false;
                }

                #endregion

                #region [ Check CurStep All Filter Condition ]

                cur1stDefindCmd = new DefineNormalRobotCmd();
                cur2ndDefindCmd = new DefineNormalRobotCmd();

                //Arm Job Only Check curStep Filter
                if (CheckAllFilterConditionByStepNo(curRobot, curRobotStageJob, curRobotStageJob.RobotWIP.CurStepNo, cur1stDefindCmd, cur2ndDefindCmd, ref curFilterStageList) == false)
                {
                    //Filter條件有問題則回覆NG
                    return false;
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

                #region [ by RobotArm Qty Create Command ]

                if (curRobot.Data.ARMJOBQTY == 1)
                {

                    #region [ 20151022 add Check First Glass Command ]

                    //Check 1st Command
                    if (PortFetchOut_FirstGlassCheck(curRobot, curRobotStageJob, cur1stDefindCmd, false) == false)
                    {

                        return false;
                    }

                    //Check 2nd Command
                    if (PortFetchOut_FirstGlassCheck(curRobot, curRobotStageJob, cur2ndDefindCmd, true) == false)
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
                    CheckPutReadyCommandCondition(curRobot, curRobotStageJob, cur1stDefindCmd, cur2ndDefindCmd, true); //Check 2nd Command
                    #endregion

                    #region [ 20151230 add Check RTCPUT Condition ]
                    //GlobalAssemblyVersion v1.0.0.9-20151230
                    CheckRtcPutCommandCondition(curRobot, curRobotStageJob, cur1stDefindCmd); //Check 1st Command
                    CheckRtcPutCommandCondition(curRobot, curRobotStageJob, cur2ndDefindCmd, true); //Check 2nd Command
                    #endregion

                    #region [ 20151025 add Check Multi-Single Condition ] 20151102 Mark .只有對Mulit Type EQP 存取2片時才會下Multi命令

                    ////Check 1st Command
                    //CheckMultiSingleCommandCondition(curRobot, curRobotStageJob, cur1stDefindCmd, false);
                    ////Check 2nd Command
                    //CheckMultiSingleCommandCondition(curRobot, curRobotStageJob, cur2ndDefindCmd, true);

                    #endregion

                }
                else
                {
                    //Create 1 Arm 2 Substrate
                    //[ Wait_For_Proc_00027 ] Arm Job 針對  1Arm 2Job處理
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





        //Other Function =============================================================================================================================================================


        /// <summary>
        /// Set CF MQC Port Default Inspec Priority
        /// </summary>
        /// <param name="curRobot">Robot Entity</param>
        /// <param name="PortNO">Port No or ID</param>
        /// <returns></returns>
        private string SetCFMQC_DefaultInspecPriority(Robot curRobot, string PortNO)
        {
            string strlog = string.Empty;
            string _priority = string.Empty;
            try
            {
                if (!curRobot.File.CurMQCPortDefaultInspPriority.ContainsKey(PortNO))
                {
                    #region [ 目前沒有設定Priority 則以目前的Node List做預設 ]
                    List<Equipment> curAllNodeList = ObjectManager.EquipmentManager.GetEQPsByLine(Workbench.ServerName);
                    if (curAllNodeList.Count == 0)
                    {
                        //找不到則回覆錯誤訊息
                        strlog = string.Format("[LINENAME={0}] [BCS <- RCS]  Can't find NodeEntity by WorkBench ServerName[{1}] !", Workbench.ServerName, Workbench.ServerName);
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", strlog);
                        return string.Empty;
                    }
                    else
                    {
                        #region [ 設定All Inspeciton default Priority ]
                        foreach (Equipment curEQP in curAllNodeList)
                        {
                            if (curEQP.Data.NODEATTRIBUTE == "IN")
                            {
                                //只需要設定所有檢測機的Priority
                                _priority += curEQP.Data.NODENO.Replace('L', '0');
                            }
                        }
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI] Robot({1}) add default Port({2}) Inspection Priority({3}))",
                                Workbench.ServerName, curRobot.Data.ROBOTNAME, PortNO, _priority.ToString()));
                        curRobot.File.CurMQCPortDefaultInspPriority.Add(PortNO, _priority.ToString());
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    _priority = curRobot.File.CurMQCPortDefaultInspPriority[PortNO];
                }

                ObjectManager.RobotManager.EnqueueSave(curRobot.File);
                return _priority;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_flowpriority"></param>
        /// <returns></returns>
        private string FlowPriority(string _flowpriority)
        {
            try
            {
                int a1 = int.Parse(_flowpriority.Substring(0, 2));
                int a2 = int.Parse(_flowpriority.Substring(2, 2));
                int a3 = int.Parse(_flowpriority.Substring(4, 2));
                int Sum1 = 0;
                int Sum2 = 0;
                int Sum3 = 0;
                long Sum4 = 0;

                Sum1 = a1;
                Sum2 = a2 << 4; //往高位元Shift 4 bits
                Sum3 = a3 << 8; //往高位元Shift 8 bits
                Sum4 = Sum1 + Sum2 + Sum3 + Sum4;

                return Sum4.ToString();
            }
            catch
            {
                return "0";
            }
        }


        #region 已经不使用了, 请改使用 AbstractRobotService 那边的2版本!!

        /// <summary>根據RouteStepByPass條件判斷特定StepNo是否出現變化
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="checkStepNo"></param>
        /// <param name="curStageSelectInfo"></param>
        /// <param name="curBeforeFilterStageList"></param>
        /// <returns></returns>
        private bool CheckAllRouteStepByPassCondition(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList)
        {
            IRobotContext robotConText = new RobotContext();
            string fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;
            List<RobotStage> curCanUseStageList = new List<RobotStage>();

            try
            {

                List<RobotRuleRouteStepByPass> curRouteStepByPassList = new List<RobotRuleRouteStepByPass>();

                curRouteStepByPassList = ObjectManager.RobotManager.GetRuleRouteStepByPass(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, checkStepNo);

                int ruleCount = 0;

                if (curRouteStepByPassList != null)
                {

                    ruleCount = curRouteStepByPassList.Count;

                }

                #region[DebugLog][ Start Job All RouteStepByPass Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) nextStepNo({5}) Check 1st Command Rule Job RouteStepByPass ListCount({6}) Start {7}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(), ruleCount.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #region [Check CurStep All RouteStepByPass Condition ]

                #region [ Initial RouteStepByPass Rule List RobotConText Info. 搭配針對File Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);

                //1st Cmd is2ndCmdFlag is false
                robotConText.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, false);

                #region  [ RouteStepByPass前先預設目前Step都是符合條件的 ]

                //增加防呆
                if (!curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(checkStepNo))
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 1st Command Rule Job RouteStepByPass Current StepNo({4}) but the Job Route max StepNo is {5} End{6}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                               checkStepNo.ToString(), curBcsJob.RobotWIP.RobotRouteStepList.Count.ToString(),
                                                new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    return false;
                }

                string[] curStepCanUseStageList = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST.Split(',');

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

                    if (curCanUseStageList.Contains(curStage) == false)
                    {

                        curCanUseStageList.Add(curStage);

                    }

                    #endregion

                }

                #endregion

                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curCanUseStageList);

                #endregion =======================================================================================================================================================

                #region [ 如果沒有任何StepByPass則直接回覆True ]

                if (curRouteStepByPassList == null)
                {
                    #region[DebugLog][ Start Job All RouteStepByPass Function ]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 1st Command Rule Job RouteStepByPass ListCount({4}) End {5}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                ruleCount.ToString(),
                                                new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //取得Stage Selct後的Can Use Stages List
                    curBeforeFilterStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

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

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass object({4}) MethodName({5}) IsEnable({6}) Start {7}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, curRouteStepByPassCondition.Data.ISENABLED,
                                                new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    if (curRouteStepByPassCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                    {
                        //將ByPass後的GOTOSTEPID
                        robotConText.AddParameter(eRobotContextParameter.RouteStepByPassGotoStepNo, curRouteStepByPassCondition.Data.GOTOSTEPID);

                        checkFlag = (bool)Invoke(curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, new object[] { robotConText });

                        if (checkFlag == false)
                        {

                            #region[DebugLog][ End Rule Job RouteStepByPass Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass Fail, object({4}) MethodName({5}) RtnCode({4})  RtnMsg({6}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME,
                                                        curRouteStepByPassCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0005 ]

                            if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass Fail, object({4}) MethodName({5}) RtnCode({6})  RtnMsg({7}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6})!",
                                //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRouteStepByPassCondition.Data.OBJECTNAME,
                                //                        curRouteStepByPassCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                failMsg = string.Format("Job({0}_{1}) RtnCode({2})  RtnMsg({3})!",
                                                        curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion

                            }

                            #endregion

                            #region[DebugLog][ End Job All StageSelct Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 1st Command Rule Job RouteStepByPass ListCount({4}) End {5}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
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

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME,
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

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepByPass object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curRouteStepByPassCondition.Data.OBJECTNAME, curRouteStepByPassCondition.Data.METHODNAME, curRouteStepByPassCondition.Data.ISENABLED,
                                                    new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                    }

                }

                #endregion

                #endregion

                #region[DebugLog][ Start Job All RouteStepByPass Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 1st Command Rule Job RouteStepByPass ListCount({4}) End {5}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            ruleCount.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                //取得Stage Selct後的Can Use Stages List
                curBeforeFilterStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

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
        /// <returns></returns>
        private bool CheckAllRouteStepJumpCondition(Robot curRobot, Job curBcsJob, int checkStepNo, ref List<RobotStage> curBeforeFilterStageList)
        {
            IRobotContext robotConText = new RobotContext();
            string fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;
            //List<RobotStage> curCanUseStageList = new List<RobotStage>();

            try
            {

                List<RobotRuleRouteStepJump> curRouteStepJumpList = new List<RobotRuleRouteStepJump>();

                curRouteStepJumpList = ObjectManager.RobotManager.GetRuleRouteStepJump(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, checkStepNo);

                int ruleCount = 0;

                if (curRouteStepJumpList != null)
                {

                    ruleCount = curRouteStepJumpList.Count;

                }

                #region[DebugLog][ Start Job All RouteStepJump Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) nextStepNo({5}) Check 1st Command Rule Job RouteStepJump ListCount({6}) Start {7}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(), ruleCount.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR_LENGTH));

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #region [Check CurStep All RouteStepJump Condition ]

                #region [ Initial RouteStepJump Rule List RobotConText Info. 搭配針對File Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);

                //1st Cmd is2ndCmdFlag is false
                robotConText.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, false);

                //拿RuleRouteStepByPass之後的StageIDList來做後續處理
                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curBeforeFilterStageList);

                #endregion =======================================================================================================================================================

                #region [ 如果沒有任何StepByPass則直接回覆True ]

                if (curRouteStepJumpList == null)
                {
                    #region[DebugLog][ Start Job All RouteStepJump Function ]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 1st Command Rule Job RouteStepJump ListCount({4}) End {5}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                ruleCount.ToString(),
                                                new string(eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_START_CHAR_LENGTH));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //取得RouteStepJump後的Can Use Stages List(沒Jump沿用傳入的CanUseStageList, 如果改變則要更新為新Step對應的CanUseStageList
                    //curBeforeFilterStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

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

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump object({4}) MethodName({5}) IsEnable({6}) Start {7}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, curRouteStepJumpCondition.Data.ISENABLED,
                                                new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_START_CHAR_LENGTH));

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    if (curRouteStepJumpCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                    {
                        //將Jump後的GOTOSTEPID送入處理
                        robotConText.AddParameter(eRobotContextParameter.RouteStepJumpGotoStepNo, curRouteStepJumpCondition.Data.GOTOSTEPID);

                        checkFlag = (bool)Invoke(curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, new object[] { robotConText });

                        if (checkFlag == false)
                        {

                            #region[DebugLog][ End Rule Job RouteStepJump Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump Fail, object({4}) MethodName({5}) RtnCode({4})  RtnMsg({6}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME,
                                                        curRouteStepJumpCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0005 ]

                            if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump Fail, object({4}) MethodName({5}) RtnCode({6})  RtnMsg({7}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6})!",
                                //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRouteStepJumpCondition.Data.OBJECTNAME,
                                //                        curRouteStepJumpCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                failMsg = string.Format("Job({0}_{1}) RtnCode({2})  RtnMsg({3})!",
                                                        curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion

                            }

                            #endregion

                            #region[DebugLog][ End Job All StageSelct Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 1st Command Rule Job RouteStepJump ListCount({4}) End {5}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
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

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME,
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

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job RouteStepJump object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curRouteStepJumpCondition.Data.OBJECTNAME, curRouteStepJumpCondition.Data.METHODNAME, curRouteStepJumpCondition.Data.ISENABLED,
                                                    new string(eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR, eRobotCommonConst.RULE_ROUTESTEPBYPASS_END_CHAR_LENGTH));

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                    }

                }

                #endregion

                #endregion

                #region[DebugLog][ Start Job All RouteStepJump Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 1st Command Rule Job RouteStepJump ListCount({4}) End {5}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            ruleCount.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_END_CHAR, eRobotCommonConst.ALL_RULE_ROUTESTEPJUMP_END_CHAR_LENGTH));

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                //取得RouteStepJump後的Can Use Stages List(沒Jump沿用傳入的CanUseStageList, 如果改變則要更新為新Step對應的CanUseStageList
                //curBeforeFilterStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

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
        /// <param name="checkStepNo"></param>
        /// <param name="cur1stDefineCmd"></param>
        /// <param name="cur2ndDefindCmd"></param>
        /// <param name="curCanUseStageList"></param>
        /// <returns></returns>
        private bool CheckAllFilterConditionByStepNo(Robot curRobot, Job curBcsJob, int checkStepNo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefindCmd, ref List<RobotStage> curCanUseStageList)
        {

            IRobotContext robotConText = new RobotContext();
            string fail_ReasonCode = string.Empty;
            string strlog = string.Empty;
            bool checkFlag = false;
            string failMsg = string.Empty;
            //20150831 add 
            List<RobotStage> curLDRQStageList = new List<RobotStage>();

            try
            {

                List<RobotRuleFilter> curFilterList = new List<RobotRuleFilter>();

                curFilterList = ObjectManager.RobotManager.GetRuleFilter(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID, checkStepNo);

                #region[DebugLog][ Start Job All Filter Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 1st Command Rule Job Filter ListCount({4}) Start {5}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curFilterList.Count.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_FILTER_START_CHAR, eRobotCommonConst.ALL_RULE_FILTER_START_CHAR_LENGTH));

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #region [Check CurStep All Filter Condition ]

                #region [ Initial Filter Rule List RobotConText Info. 搭配針對File Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                robotConText.AddParameter(eRobotContextParameter.CurJobEntity, curBcsJob);

                //DB Spec define : 'PUT' / 'GET' / 'PUTREADY' / 'GETREADY'
                cur1stDefineCmd.Cmd01_DBRobotAction = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTACTION;
                //DB Spec define : 'UP':Upper Arm 'LOW':Lower Arm 'ANY':Any Arm 'ALL':All Arm
                cur1stDefineCmd.Cmd01_DBUseArm = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.ROBOTUSEARM;
                cur1stDefineCmd.Cmd01_DBStageIDList = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST;
                robotConText.AddParameter(eRobotContextParameter.Define_1stNormalRobotCommandInfo, cur1stDefineCmd);
                robotConText.AddParameter(eRobotContextParameter.Define_2ndNormalRobotCommandInfo, cur2ndDefindCmd);
                //1st Cmd is2ndCmdFlag is false
                robotConText.AddParameter(eRobotContextParameter.Is2ndCmdCheckFlag, false);

                #region  [ Filter前先預設目前Step都是符合條件的 ] 20151002 Mark 已經改在StageSelect取得了

                //string[] curStepCanUseStageList = curBcsJob.RobotWIP.RobotRouteStepList[checkStepNo].Data.STAGEIDLIST.Split(',');

                //for (int i = 0; i < curStepCanUseStageList.Length; i++)
                //{

                //    #region [ Check Stage is Exist ]

                //    RobotStage curStage;

                //    curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

                //    //找不到 Robot Stage 回NG
                //    if (curStage == null)
                //    {

                //        #region[DebugLog]

                //        if (IsShowDetialLog == true)
                //        {
                //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
                //                                    curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

                //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                //        }

                //        #endregion

                //        return false;
                //    }

                //    if (curLDRQStageList.Contains(curStage) == false)
                //    {

                //        curLDRQStageList.Add(curStage);

                //    }

                //    #endregion

                //}

                //robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curLDRQStageList);

                #endregion

                robotConText.AddParameter(eRobotContextParameter.StepCanUseStageList, curCanUseStageList);

                #endregion =======================================================================================================================================================

                foreach (RobotRuleFilter curFilterCondition in curFilterList)
                {
                    //Set want To Check Function Fail_ReasonCode[ Job_Fail_Case_E0004 ] ,以Rule Job Filter 的ObjectName與MethodName為Key來決定是否紀錄Log
                    //因為會出現同Job 確認不同Step所以FailCode要補上StepNo
                    fail_ReasonCode = string.Format("{0}_{1}_{2}", curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, checkStepNo.ToString());

                    #region[DebugLog][ Start Rule Job Filter Function ]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Filter object({4}) MethodName({5}) IsEnable({6}) Start {7}",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, curFilterCondition.Data.ISENABLED,
                                                new string(eRobotCommonConst.RULE_FILTER_START_CHAR, eRobotCommonConst.RULE_FILTER_START_CHAR_LENGTH));

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    if (curFilterCondition.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                    {

                        checkFlag = (bool)Invoke(curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, new object[] { robotConText });

                        if (checkFlag == false)
                        {

                            #region[DebugLog][ End Rule Job Filter Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Filter Fail, object({4}) MethodName({5}) RtnCode({4})  RtnMsg({6}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Filter object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME,
                                                        curFilterCondition.Data.ISENABLED, new string(eRobotCommonConst.RULE_FILTER_END_CHAR, eRobotCommonConst.RULE_FILTER_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add To Check Fail Message To Job ][ Job_Fail_Case_E0004 ]

                            if (curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Filter Fail, object({4}) MethodName({5}) RtnCode({6})  RtnMsg({7}]!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("Robot({0}) Job CassetteSequenceNo({1}) JobSequenceNo({2}) object({3}) MethodName({4}) RtnCode({5})  RtnMsg({6})!",
                                //                        curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curFilterCondition.Data.OBJECTNAME,
                                //                        curFilterCondition.Data.METHODNAME, robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                failMsg = string.Format("Job({0}_{1}) RtnCode({2})  RtnMsg({3})!",
                                                        curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion

                            }

                            #endregion

                            #region[DebugLog][ Start Job All Filter Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 1st Command Rule Job Filter ListCount({4}) End {5}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curFilterList.Count.ToString(),
                                                        new string(eRobotCommonConst.ALL_RULE_SELECT_END_CHAR, eRobotCommonConst.ALL_RULE_FILTER_END_CHAR_LENGTH));

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //有重大異常直接結束filter邏輯回復NG
                            return false;

                        }
                        else
                        {

                            //Clear[ Robot_Fail_Case_E0004 ]
                            RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);

                            #region[DebugLog][ End Rule Job Filter Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Filter object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME,
                                                        curFilterCondition.Data.ISENABLED,
                                                        new string(eRobotCommonConst.RULE_FILTER_END_CHAR, eRobotCommonConst.RULE_FILTER_END_CHAR_LENGTH));

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

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Rule Job Filter object({4}) MethodName({5}) IsEnable({6}) End {7}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                    curFilterCondition.Data.OBJECTNAME, curFilterCondition.Data.METHODNAME, curFilterCondition.Data.ISENABLED,
                                                    new string(eRobotCommonConst.RULE_FILTER_END_CHAR, eRobotCommonConst.RULE_FILTER_END_CHAR_LENGTH));

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                    }

                }

                #endregion

                #region[DebugLog][ Start Job All Filter Function ]

                if (IsShowDetialLog == true)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check 1st Command Rule Job Filter ListCount({4}) End {5}",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curFilterList.Count.ToString(),
                                            new string(eRobotCommonConst.ALL_RULE_FILTER_END_CHAR, eRobotCommonConst.ALL_RULE_FILTER_END_CHAR_LENGTH));

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                //取得Filter後的LDRQ Staus List
                curCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                return false;
            }


        }

        #endregion


        /// <summary>ELA Special Rule for Prefetch
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <returns></returns>
        private bool CheckStagePrefetchSpecialCondition_ELA(Robot _curRobot, RobotStage _curStage)
        {
            Line line;
            List<Port> lstPort;
            IList<Job> lstJob;
            Equipment ela1, ela2;
            bool rtn = true;
            string processtype = string.Empty;
            try
            {
                line = ObjectManager.LineManager.GetLines()[0];
                if (line == null)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Line Object is Null");
                    return false;
                }

                //if all glass in cst is only ELA, return true                
                lstPort = ObjectManager.PortManager.GetPorts("L2");
                foreach (Port port in lstPort)
                {
                    if (port.File.Status != ePortStatus.LC || port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING || port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING)
                        continue;

                    lstJob = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo);
                    if (lstJob == null || lstJob.Count == 0)
                        continue;

                    foreach (Job jb in lstJob)
                    {
                        //Mix Mode Can't Prefetch
                        if (processtype.Length == 0)
                        {
                            processtype = jb.ArraySpecial.ProcessType;
                        }
                        else
                        {
                            if (!processtype.Equals(jb.ArraySpecial.ProcessType))
                            {
                                return false;
                            }
                        }

                        //if not ELA step only, Can't Prefetch
                        if (!jb.ArraySpecial.GlassFlowType.Equals("2") && !jb.ArraySpecial.GlassFlowType.Equals("4")
                            && !jb.ArraySpecial.GlassFlowType.Equals("6"))
                        {
                            return false;
                        }
                    }
                }

                //check EQ Mode & Status, if Mode & Status Not Match, return false
                ela1 = ObjectManager.EquipmentManager.GetEQP("L4");
                ela2 = ObjectManager.EquipmentManager.GetEQP("L5");
                if (ela1 == null || ela2 == null)
                    return false;

                if (processtype.Equals("1"))
                    processtype = "MQC";
                else
                    processtype = "NORMAL";

                if (ela1.File.EquipmentRunMode.ToUpper().Equals(processtype) && ela2.File.EquipmentRunMode.ToUpper().Equals(processtype))
                {
                    if ((ela1.File.Status != eEQPStatus.IDLE && ela1.File.Status != eEQPStatus.RUN) && (ela2.File.Status != eEQPStatus.IDLE && ela2.File.Status != eEQPStatus.RUN))
                        return false;
                }
                else if (ela1.File.EquipmentRunMode.ToUpper().Equals(processtype))
                {
                    if (ela1.File.Status != eEQPStatus.IDLE && ela1.File.Status != eEQPStatus.RUN)
                        return false;
                }
                else if (ela2.File.EquipmentRunMode.ToUpper().Equals(processtype))
                {
                    if (ela2.File.Status != eEQPStatus.IDLE && ela2.File.Status != eEQPStatus.RUN)
                        return false;
                }
                else
                {
                    return false;
                }

                return rtn;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }

        }


    }
}
