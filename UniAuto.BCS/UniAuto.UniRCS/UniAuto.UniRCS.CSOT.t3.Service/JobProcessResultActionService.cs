using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    [UniAuto.UniBCS.OpiSpec.Help("JobProcessResultActionService")]
    public partial class JobProcessResultActionService : AbstractRobotService
    {

        public override bool Init()
        {
            return true;
        }

//ProcResult Funckey = "PR" + XXXX(序列號)

        /// <summary> Route Step "GET Stage Job" Process Result OK時的相關處理
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("PR0001")]
        public bool ProcResult_JobMoveToRobotArm_1Arm1Job(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                string strTmp = string.Empty;
                int oldStepNo = 0;
                int newStepNo = 0;

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBcsJob Entity ]

                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

                //找不到 BcsJob 回NG
                if (curBcsJob == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get BcsJob!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion
                
                #region [ Get curBCSJob Load ArmNo ]

                string curLoadArmNo = (string)robotConText[eRobotContextParameter.LoadJobArmNo_For_1Arm_1Job];
                
                //找不到 CurLoadArmNo 回NG
                if (curLoadArmNo == null || curLoadArmNo ==string.Empty)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Load ArmNo!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Load ArmNo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_CurLoadArmNo_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                int intArmNo = 0;
                 
                int.TryParse(curLoadArmNo,out intArmNo);

                #endregion

                #region [ Check Update RobotWIP Condition ]

                #region [ Check Job Location must At Not Robot Home Stage ]

                if (curBcsJob.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStageNo({4}) at RobotArm and can not Update CurRouteStepNo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);
                     
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStageNo({4}) at RobotArm and can not Update CurRouteStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_Loction_Not_RobotArm_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 由Job中透過目前的CurStep取得目前RouteInfo ]

                RobotRouteStep curRouteStep = null;

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == true)
                {
                    curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
                }

                if (curRouteStep == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurRouteStepNo({4}) can not get RouteInfo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());
                    
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurRouteStepNo({4}) can not get RouteInfo!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_RouteInfo_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ 確認目前RouteStepNo中的動作是否為GET ]

                switch (curRouteStep.Data.ROBOTACTION)
                {
                    case eRobot_DB_CommandAction.ACTION_GET:

                        //只有CurRoute Step Action為Get時才能在Job Move To Arm(Arm Load Job)時更新StepNo
                        break;

                    default:


                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) ACTION({5}) is not ({6}) and can not Update CurStepNo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_GET);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        errMsg = string.Format("[{0}]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurRouteStepNo({4}) ACTION({5}) is not ({6}) and can not Update CurStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_GET);

                        robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_JOB_CurStep_Action_IsNot_GET);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                }

                #endregion

                #endregion

                #region [ Update RobotWIP ]

                lock (curBcsJob)
                {

                    #region [ Update StepNo ]

                    //[ Wait_For_Proc_00015 ][ 針對Step Rule 要如何實作 ]
                    //[ Wait_For_Proc_00016 ][ 針對Recipe by Pass後Step Rule 要如何實作 ]


                    oldStepNo = curBcsJob.RobotWIP.CurStepNo;
                    //20151014 Modity NextStep由WIP來取得
                    newStepNo = curBcsJob.RobotWIP.NextStepNo; // curBcsJob.RobotWIP.CurStepNo + 1;

                    strTmp = string.Format("curStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), newStepNo.ToString());

                    //Update CurStepNo and NextStep
                    curBcsJob.RobotWIP.CurStepNo = newStepNo;

                    #region [ Get New Current Step Entity to Get NextStep ]

                    //RobotRouteStep newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];

                    RobotRouteStep newCurRouteStep = null;

                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(newStepNo) != false)
                    {

                        newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];
                    }

                    //找不到 CurStep Route 記Log 且將NextStep改為0(異常)
                    if (newCurRouteStep == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4}) Entity!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                newStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.NextStepNo = 0;
                    }
                    else
                    {
                        curBcsJob.RobotWIP.NextStepNo = newCurRouteStep.Data.NEXTSTEPID;

                    }


                    #endregion

                    strTmp = strTmp + string.Format("NextStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString());

                    #endregion

                    #region [ Update RobotJob Status PROCESS ]

                    if (newStepNo == 1)
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.WAIT_PROC);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.WAIT_PROC;

                    }
                    //20151014 Modify 大於65535則算Complete
                    //else if (newStepNo > curBcsJob.RobotWIP.RobotRouteStepList.Count) //最後的Step是準備放到Port 所以必須要是>而不是>=
                    else if (newStepNo >= eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO)
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.COMPLETE);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.COMPLETE;

                    }
                    else
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to ({1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.INPROCESS);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.INPROCESS;
                    }

                    #endregion

                    #region [ Update CurLocation Info ]

                    strTmp = strTmp + string.Format("CurLocation_StageType form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageType, eRobotStageType.ROBOTARM);

                    curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.ROBOTARM;

                    strTmp = strTmp + string.Format("CurLocation_StageID form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

                    string _preLocation_StageID = curBcsJob.RobotWIP.CurLocation_StageID;
                    curBcsJob.RobotWIP.CurLocation_StageID = eRobotCommonConst.ROBOT_HOME_STAGEID;

                    strTmp = strTmp + string.Format("CurLocation_SlotNo form ({0}) to ({1}).", curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(), intArmNo.ToString());

                    curBcsJob.RobotWIP.CurLocation_SlotNo = intArmNo;

                    #endregion

                    RobotStage _stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(_preLocation_StageID);

                    #region Update PreFetch Info //modify CVD EQP prefetch 也要更新PrefetchFlag,暂时只for CVD,其他pending  yang
                    if (_stage != null && (_stage.Data.STAGETYPE == eRobotStageType.PORT||_stage.Data.LINEID.Contains("CVD"))) if (_stage.Data.PREFETCHFLAG.ToString().ToUpper() == "Y") curBcsJob.RobotWIP.PreFetchFlag++;
                    #endregion

                    #region Update FetchOut DateTime Info
                    //if (_stage != null && _stage.Data.STAGETYPE == eRobotStageType.PORT && curBcsJob.RobotWIP.FetchOutDataTime != DateTime.MinValue)
                    //{
                    DateTime _now = DateTime.Now;
                    strTmp = strTmp + string.Format("FetchOut from ({0}) on ({1}).", _preLocation_StageID, _now.ToString("yyyy-MM-dd HH:mm:ss"));
                    curBcsJob.RobotWIP.FetchOutDataTime = _now;
                    //}
                    #endregion

                    curBcsJob.RobotWIP.PutReadyFlag = 0;
                    curBcsJob.RobotWIP.PutReady_StageID = string.Empty;
                    if (curBcsJob.RobotWIP.PreFetchFlag > 0) //有启动预取!
                    {
                        if (curRobot.CurRealTimeSetCommandInfo != null) //命令还没清空
                        {
                            if (curRobot.CurRealTimeSetCommandInfo.Cmd02_Command == eRobot_Trx_CommandAction.ACTION_PUTREADY)
                            {
                                curBcsJob.RobotWIP.PutReadyFlag = 1; //如果命令是putready, 则更新put ready flag!
                                curBcsJob.RobotWIP.PutReady_StageID = curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition.ToString(); //记录基本预设要去的stage!
                            }
                        }
                    }
                    //20160525 加入STAGETYPE == PORT判斷
                    if (curBcsJob.RobotWIP.RTCReworkFlag && _stage.Data.STAGETYPE == eRobotStageType.PORT)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job[{2}]'s RTCReworkFlag[ON], set OFF.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, string.Format("CST={0}, Slot={1}", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString()));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.RTCReworkFlag = false; //20160108-001-dd::回插再出片, 需要改为OFF并记LOG!!
                    }
                    //20160525 加入STAGETYPE == PORT判斷
                    if (curBcsJob.RobortRTCFlag && _stage.Data.STAGETYPE == eRobotStageType.PORT) curBcsJob.RobortRTCFlag = false; //抽出来要改为OFF

                    if (curBcsJob.RobotWIP.EQPRTCFlag&&_stage.Data.STAGETYPE == eRobotStageType.PORT)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job[{2}]'s RobotWIP.EQPRTCFlag[ON], set OFF.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, string.Format("CST={0}, Slot={1}", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString()));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.EQPRTCFlag = false; //抽出来要改为OFF  20161003 add by yang
                    }

                    #region Line Special Result by Line Type
                    //20160107-001-dd
                    Line _line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                    if (_line == null) return false;

                    switch (_line.Data.LINETYPE)
                    {
                        case eLineType.ARRAY.DRY_ICD:
                        case eLineType.ARRAY.DRY_YAC:
                        case eLineType.ARRAY.DRY_TEL:
                            if (_stage != null && _stage.Data.STAGETYPE == eRobotStageType.PORT) curRobot.File.DryLastProcessType = curBcsJob.ArraySpecial.ProcessType.ToString();
                            break;

                        default: break;
                    }
                    #endregion


                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP {4}",
                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, strTmp);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    #region [ 20151209 add For Set FroceRetrunCSTWithoutLDRQ status is NotCheck ]

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP FroceRetrunCSTWithoutLDRQ From({4}) to ({5})",
                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status, eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_NOTCHECK);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_NOTCHECK;

                    #endregion

                    //20160629
                    if(_stage != null)
                    {
                        if (_stage.Data.STAGETYPE == eRobotStageType.PORT)
                        {
                            curRobot.MoveToArm = eRobotStageType.PORT;
                        }
                        else //不是port type,用eRobotStageType.EQUIPMENT表示就好,只是為了區分到過port或EQP而已
                        {
                            curRobot.MoveToArm = eRobotStageType.EQUIPMENT;
                        }
                    }

                }

                //Save File
                ObjectManager.JobManager.EnqueueSave(curBcsJob);

                #endregion

                #region OVN提前开门逻辑
                //add by hujunpeng 20181001
                if (curRobot.Data.LINEID == "TCOVN400" || curRobot.Data.LINEID == "TCOVN500")
                {
                    if (curRouteStep.ToString() == "3" )
                    {
                    switch (curRobot.File.CurRobotPosition)
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
                }
                #endregion

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }

        /// <summary> Route Step "ArmJob to Put Stage" Process Result OK時的相關處理
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("PR0002")]
        public bool ProcResult_ArmJobMoveToStage_1Arm1Job(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                string strTmp = string.Empty;
                int oldStepNo = 0;
                int newStepNo = 0;
                RobotRouteStep oldStep = null;

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

				#region [ Get Robot Line Entity ]

				Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

				if (robotLine == null) {

					#region  [DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Robot({1}) can not Get Line Entity!",
											MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

					robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion

                #region [ Get curBcsJob Entity ]

                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

                //找不到 BcsJob 回NG
                if (curBcsJob == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get BcsJob!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBCSJob Unload ArmNo ]

                string curUnloadArmNo = (string)robotConText[eRobotContextParameter.UnloadJobArmNo_For_1Arm_1Job];

                //找不到 CurLoadArmNo 回NG
                if (curUnloadArmNo == null || curUnloadArmNo == string.Empty)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Unload ArmNo!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Unload ArmNo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_CurUnloadArmNo_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                int intArmNo = 0;

                int.TryParse(curUnloadArmNo, out intArmNo);

                #endregion

                #region [ Get Target Stage Entity ]

                string tmpTargetStageID = (string)robotConText[eRobotContextParameter.TargetStageID];

                RobotStage curTargetStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(tmpTargetStageID);

                //找不到 Target Stage 回NG
                if (curTargetStage == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get TargetStage!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get TargetStage!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_TargetStage_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Target SlotNo ]

                string tmpTargetSlotNo = (string)robotConText[eRobotContextParameter.TargetSlotNo];
                int intTargetSlotNo = 0;              
                int.TryParse(tmpTargetSlotNo, out intTargetSlotNo);

                //找不到 CurLoadArmNo 回NG
                if (intTargetSlotNo == 0)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Target SlotNo!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Target SlotNo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_TargetSlotNo_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check Update RobotWIP Condition ]

                #region [ Check Job Location must At Robot Home Stage ]

                if (curBcsJob.RobotWIP.CurLocation_StageID != eRobotCommonConst.ROBOT_HOME_STAGEID)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStageNo({4}) Not On RobotArm and can not Update CurRouteStepNo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] Job({1}_{2}) CurStageNo({3}) Not On RobotArm and can not Update CurRouteStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_Loction_On_RobotArm_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 由Job中透過目前的CurStep取得目前RouteInfo ]

                RobotRouteStep curRouteStep = null;

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == true)
                {
                    curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
                    oldStep = curRouteStep;
                }

                if (curRouteStep == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurRouteStepNo({4}) can not get RouteInfo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] Job({1}_{2}) CurRouteStepNo({3}) can not get RouteInfo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_RouteInfo_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ 確認目前RouteStepNo中的動作是否為PUT OR EXCHANGE ]

                switch (curRouteStep.Data.ROBOTACTION)
                {
                    case eRobot_DB_CommandAction.ACTION_PUT:
                    case eRobot_DB_CommandAction.ACTION_EXCHANGE:
                    case eRobot_DB_CommandAction.ACTION_GETPUT:
                        //只有CurRoute Step Action為PUT or Exchange(GET&PUT)時才能在Job Move To Stage(Arm Unload Job)時更新StepNo
                        break;

                    default:


                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) ACTION({5}) is not ({6}) or ({7}) or ({8}) and can not Update CurStepNo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_PUT, eRobot_DB_CommandAction.ACTION_EXCHANGE, eRobot_DB_CommandAction.ACTION_GETPUT);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        errMsg = string.Format("[{0}] Job({1}_{2}) CurRouteStepNo({3}) ACTION({4}) is not ({5}) or ({6}) or({7}) and can not Update CurStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_PUT, eRobot_DB_CommandAction.ACTION_EXCHANGE, eRobot_DB_CommandAction.ACTION_GETPUT);

                        robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_JOB_CurStep_Action_IsNot_PUT_EX);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                }

                #endregion

                #endregion

                #region [ Update RobotWIP ]

                lock (curBcsJob)
                {            

                    #region [ Update StepNo ]

                    //[ Wait_For_Proc_00015 ][ 針對Step Rule 要如何實作 ]
                    //[ Wait_For_Proc_00016 ][ 針對Recipe by Pass後Step Rule 要如何實作 ]


                    oldStepNo = curBcsJob.RobotWIP.CurStepNo;


					//20151014 Modity NextStep由WIP來取得
					newStepNo = curBcsJob.RobotWIP.NextStepNo; // curBcsJob.RobotWIP.CurStepNo + 1;

					#region [ special for CF buffering function,update NextStep to first step ]
					if (robotLine.Data.FABTYPE == eFabType.CF.ToString()) 
					{
						//target為port且job為buffering
						if (curTargetStage.Data.STAGETYPE == eStageType.PORT && curBcsJob.CfSpecial.RCSBufferingFlag == "1") 
						{
							int jobfromslot;
							int.TryParse(curBcsJob.FromSlotNo, out jobfromslot);
							
							Port curTargetPort = ObjectManager.PortManager.GetPort(curTargetStage.Data.STAGEID);
							if (curTargetPort != null) {
								//且回loading port 原cst原slot
								if (curTargetPort.File.Type == ePortType.LoadingPort &&
									curTargetPort.File.CassetteID == curBcsJob.FromCstID &&
									intTargetSlotNo == jobfromslot) {
									//改寫newStepNo為第一步
									newStepNo = curBcsJob.RobotWIP.RobotRouteStepList.OrderBy(s => s.Value.Data.STEPID).First().Value.Data.STEPID;

									strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) do buffering ,rewrite NextStep from({4}) to({5}) !",
												   curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.NextStepNo, newStepNo.ToString());

									Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
								} else {
									//update job rcsbufferingflag=0	
									lock (curBcsJob) {
										curBcsJob.CfSpecial.RCSBufferingFlag = "0";
									}
									ObjectManager.JobManager.EnqueueSave(curBcsJob);
								}							
							}							
						}
					}
					#endregion

					strTmp = string.Format("curStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), newStepNo.ToString());

                    //Update CurStepNo
                    curBcsJob.RobotWIP.CurStepNo = newStepNo;

                    #region [ Get New Current Step Entity to Get NextStep ]

                    RobotRouteStep newCurRouteStep = null;

                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(newStepNo) != false)
                    {

                        newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];
                    }

                    //找不到 CurStep Route 記Log 且將NextStep改為0(異常)
                    if (newCurRouteStep == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4}) Entity!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                newStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.NextStepNo = 0;
                    }
                    else
                    {
                        curBcsJob.RobotWIP.NextStepNo = newCurRouteStep.Data.NEXTSTEPID;

                    }


                    #endregion

                    strTmp = strTmp + string.Format("NextStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString());

                    #endregion

                    #region [ Update RobotJob Status PROCESS ]

                    if (newStepNo == 1)
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.WAIT_PROC);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.WAIT_PROC;

                    }
                    //20151014 Modify 大於65535則算Complete
                    //else if (newStepNo > curBcsJob.RobotWIP.RobotRouteStepList.Count)
                    else if (newStepNo >= eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO) 
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.COMPLETE);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.COMPLETE;

                    }
                    else
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to ({1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.INPROCESS);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.INPROCESS;
                    }

                    #endregion

                    #region [ Update CurLocation Info ]

                    strTmp = strTmp + string.Format("CurLocation_StageType form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageType, curTargetStage.Data.STAGETYPE);

                    curBcsJob.RobotWIP.CurLocation_StageType = curTargetStage.Data.STAGETYPE;

                    strTmp = strTmp + string.Format("CurLocation_StageID form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageID, curTargetStage.Data.STAGEID);

                    curBcsJob.RobotWIP.CurLocation_StageID = curTargetStage.Data.STAGEID;

                    strTmp = strTmp + string.Format("CurLocation_SlotNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(), intTargetSlotNo.ToString());

                    curBcsJob.RobotWIP.CurLocation_SlotNo = intTargetSlotNo;



                    curBcsJob.RobotWIP.FetchOutDataTime = DateTime.MinValue; //如果回插到STAGE(PORT/EQP)时, 需要清空, 待出片时, 再更新! 20151116-dd

                    #endregion

                    if (curTargetStage.Data.STAGETYPE == eRobotStageType.PORT)
                    {
                        if (curBcsJob.RobotWIP.PutReadyFlag == 1)
                        {
                            curBcsJob.RobotWIP.PutReadyFlag = 0;
                            curBcsJob.RobotWIP.PutReady_StageID = string.Empty;
                        }

                        //if (curBcsJob.RobotWIP.PreFetchFlag > 0 && curRobot.File.LastPreFetchReturnDateTime == DateTime.MinValue) curRobot.File.LastPreFetchReturnDateTime = DateTime.Now; 
                        if (curBcsJob.RobotWIP.PreFetchFlag > 0 && curBcsJob.RobotWIP.LastPreFetchFlag != curBcsJob.RobotWIP.PreFetchFlag)
                        {
                            curBcsJob.RobotWIP.LastPreFetchFlag = curBcsJob.RobotWIP.PreFetchFlag;
                            curRobot.File.LastPreFetchReturnDateTime = DateTime.Now;

                            if (curBcsJob.RobotWIP.RTCReworkFlag) curBcsJob.RobortRTCFlag = true; //更新RobotRTCFlag給BCS判断! //20160108-001-dd.有预取并且RTC才考虑
                        }
                        else
                        {
                            //20160525 如果RTCReworkFlag == true,代表有Jump RTC,所以RobortRTCFlag先不要false
                            //if (curBcsJob.RobortRTCFlag)
                            if (curBcsJob.RobortRTCFlag && !curBcsJob.RobotWIP.RTCReworkFlag)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job[{2}]'s RobotRTCFlag[ON] without Pre-Fetch and RTCReworkFlag[{3}], set OFF.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME,
                                    string.Format("CST={0}, Slot={1}", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString()), (curBcsJob.RobotWIP.RTCReworkFlag ? "ON" : "OFF"));
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                curBcsJob.RobortRTCFlag = false; //20160108-001-dd::没预取, 但有ON, 记LOG, 并设为OFF!!
                            }
                        }

                    }
                    else
                    {
                        if (curBcsJob.RobotWIP.PreFetchFlag > 0 && curBcsJob.RobotWIP.LastPreFetchFlag != curBcsJob.RobotWIP.PreFetchFlag) curBcsJob.RobotWIP.LastPreFetchFlag = curBcsJob.RobotWIP.PreFetchFlag;
                        curBcsJob.RobortRTCFlag = false; //20160126-001-dd::非PORT的stage预设要为false! (不管之前是什么!!)

                        //20160525
                        #region Update FetchOut DateTime Info
                        DateTime _now = DateTime.Now;
                        strTmp = strTmp + string.Format("FetchOut from ({0}) on ({1}).", curBcsJob.RobotWIP.CurLocation_StageID, _now.ToString("yyyy-MM-dd HH:mm:ss"));
                        curBcsJob.RobotWIP.StoreDateTime = _now;
                        #endregion

                        //20160525
                        //Yang tip;RTC基板出cst后,Flag会被设false,这里多加一次置false动作
                        if (curBcsJob.RobotWIP.EQPRTCFlag)
                            curBcsJob.RobotWIP.EQPRTCFlag = false;

                    }
                    //20160624
                    if (curRobot.OnArmPutReadyFlag == 1)
                    {
                        curRobot.OnArmPutReadyFlag = 0;
                        curRobot.OnArmPutReady_StageID = string.Empty;
                    }


                    #region [ 20151107 add Update Job Last Input TrackingData By Step RouteStep Rule ]

                    if (curRouteStep.Data.ROBOTRULE == eRobotRouteStepRule.SELECT)
                    {
                        string tmpTrackData = GetRouteStep_Select_InPutTrackingData(curTargetStage, curRouteStep.Data.INPUTTRACKDATA);

                        strTmp = strTmp + string.Format("LastInPutTrackingData form ({0}) to ({1}).", curBcsJob.RobotWIP.LastInPutTrackingData, tmpTrackData);

                        //因會出現多組(EX:11,12  00111100000000000....)所以直接抓Stage設定而不是取得RouteStep設定的TrackingData
                        curBcsJob.RobotWIP.LastInPutTrackingData = tmpTrackData;

                    }
                    else
                    {
                        strTmp = strTmp + string.Format("LastInPutTrackingData form ({0}) to ({1}).", curBcsJob.RobotWIP.LastInPutTrackingData, curRouteStep.Data.INPUTTRACKDATA);

                        //非Select只有單一設定,直接取得RouteStep設定的TrackingData
                        curBcsJob.RobotWIP.LastInPutTrackingData = curRouteStep.Data.INPUTTRACKDATA;

                    }

                    #endregion

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP {4}", 
                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, strTmp);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    #region [ 20151209 add For Set FroceRetrunCSTWithoutLDRQ status is NotCheck ]

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP FroceRetrunCSTWithoutLDRQ From({4}) to ({5})",
                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status, eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_NOTCHECK);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_NOTCHECK;

                    #endregion

                }

                //Save File
                ObjectManager.RobotManager.EnqueueSave(curRobot.File);
                ObjectManager.JobManager.EnqueueSave(curBcsJob);

                strlog = string.Format("Job({0},{1}) curStepID:{2}, job's last FetchOutDataTime:{3},job's LastUpdateTime:{4}",
                    curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString(), curBcsJob.RobotWIP.CurStepNo, curBcsJob.RobotWIP.FetchOutDataTime.ToString(), curBcsJob.LastUpdateTime.ToString());
                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                
                #endregion

                #region [ Update DRY Last Enter Stage ID ]
                if (Workbench.LineType.ToString().Contains("DRY_")) Invoke("RobotSpecialService", "Update_DRY_LastEnterStageID_For1Cmd_1Arm_1Job", new object[] { robotConText });
                #endregion

                if (StaticContext.ContainsKey(eRobotContextParameter.JobSendToSameEQ_RobotParam) && StaticContext[eRobotContextParameter.JobSendToSameEQ_RobotParam] is JobSendToSameEQ_RobotParam)
                {
                    JobSendToSameEQ_RobotParam param = (JobSendToSameEQ_RobotParam)StaticContext[eRobotContextParameter.JobSendToSameEQ_RobotParam];
                    if (param.SameEQFlag &&
                        oldStep.Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT &&
                        oldStep.Data.ROBOTRULE == eRobotRouteStepRule.SELECT &&
                        curTargetStage.Data.STAGETYPE == eRobotStageType.EQUIPMENT)
                    {
                        string node_no = string.Empty;
                        if (curRobot.File.CheckMap(curBcsJob.CassetteSequenceNo, oldStepNo, out node_no))
                        {
                            //do nothing
                            if (node_no != curTargetStage.Data.NODENO)
                            {
                                //曾經紀錄同一CST的Job在oldStepNo時進入node_no, 但是Robot Arm Unload時卻把curBcsJob放到不同Node的Stage裡
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Put into Node({4}) but Same EQ Map record Node({5})",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curTargetStage.Data.NODENO, node_no);
                                Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                        }
                        else
                        {
                            curRobot.File.AddToMap(curBcsJob.CassetteSequenceNo, oldStepNo, curTargetStage.Data.NODENO);
                            ObjectManager.RobotManager.EnqueueSave(curRobot.File);
                        }
                    }
                }
                #region OVN提前开门逻辑
                //add by hujunpeng 20181001
                if (curRobot.Data.LINEID=="TCOVN400"||curRobot.Data.LINEID=="TCOVN500")
                {
                    switch (curTargetStage.Data.STAGEID)
                    { 
                        case "11":
                            string trxName = "L3_OVN1MoveInGlassOpenTheDoor";
                            Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                            outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                            outputdata.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                            outputdata.TrackKey = DateTime.Now.ToString("yyyyMMddHHmmss");
                            SendPLCData(outputdata);
                            strlog = string.Format(" [BCS -> EQP][{0}] , SET OVN1MoveIn BIT={1}.", outputdata.TrackKey, outputdata.EventGroups[0].Events[0].Items[0].Value);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            break;
                        case "13":
                            string trxName1 = "L3_OVN2MoveInGlassOpenTheDoor";
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

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }

        private string GetRouteStep_Select_InPutTrackingData(RobotStage curTargetStage, string routeStepInputTrackData)
        {
            //預設通通是0
            string putInData = new string('0', 32);
            string strlog = string.Empty;

            try
            {
                if (curTargetStage.Data.TRACKDATASEQLIST == string.Empty)
                    return putInData;

                string[] dbStageSettingTrackSeqList = curTargetStage.Data.TRACKDATASEQLIST.Split(',');
                string dbRouteStepSettingInPutTrackSeq = string.Empty;
                //InPutTrackingData不計履歷只計當下,所以預設是0
                char[] wipInPutTrackData = new string('0', 32).ToCharArray(); // curBcsJob.RobotWIP.LastInPutTrackingData.ToCharArray();
                int bitSeq = 0;
                string newPutInTrackData = string.Empty;

                //Input Tracking Data只會紀錄PUT時當下預計要On的對應Bit而不是所有的履歷
                // EX: STEP01 要進3台 11100000 STEP03 要進另外一台 則是00010000 而不是11110000
                for (int i = 0; i < dbStageSettingTrackSeqList.Length; i++)  //ex:2,4
                {
                    //取得Stage對應在TrackingData的位置 EX: 2
                    bitSeq = int.Parse(dbStageSettingTrackSeqList[i]);

                    //透過Stage設定來找取得DB RouteStep Setting InputTrackData設定對應的設定值 EX: Check Step Setting TrackingData bit:2 (不論長度是1 or 2 都是會有1)
                    dbRouteStepSettingInPutTrackSeq = routeStepInputTrackData.Substring(bitSeq, 1);

                    if (dbRouteStepSettingInPutTrackSeq == "1")
                    {
                        wipInPutTrackData[bitSeq] = '1';
                    }

                }

                putInData = new string(wipInPutTrackData, 0, wipInPutTrackData.Length);

                return putInData;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return putInData;
            }

        }

        //Watson Add 20151221 [抽一片換一次]
        //改寫自ProcResult_JobMoveToRobotArm_1Arm1Job 運用在CVD的抽片比例上
        //在MIX MODE下才會在每抽一片就計算一次(Count -1) 且順便換種
        //其餘更新資料部份與ProcResult_JobMoveToRobotArm_1Arm1Job相同
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("PR0003")]
        public bool ProcResult_CVDFetchProportionalRule(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                string strTmp = string.Empty;
                int oldStepNo = 0;
                int newStepNo = 0;
                bool Force_ChangeCVDProportionalType = false;

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBcsJob Entity ]

                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

                //找不到 BcsJob 回NG
                if (curBcsJob == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get BcsJob!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBCSJob Load ArmNo ]

                string curLoadArmNo = (string)robotConText[eRobotContextParameter.LoadJobArmNo_For_1Arm_1Job];

                //找不到 CurLoadArmNo 回NG
                if (curLoadArmNo == null || curLoadArmNo == string.Empty)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Load ArmNo!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Load ArmNo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_CurLoadArmNo_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                int intArmNo = 0;

                int.TryParse(curLoadArmNo, out intArmNo);

                #endregion

                #region [ Get cur EQP ]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                //找不到 Node 回NG
                if (eqp == null)
                {

                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot Node No!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot Node No!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region [ Get Line ]
                Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                //找不到 Node 回NG
                if (line == null)
                {

                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot Line ID!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot Line ID!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LINE_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
                
                #region Get port
                Port port = ObjectManager.PortManager.GetPort(curBcsJob.SourcePortID);
                #endregion

                #region Force CVD Proportional Type Don't Care Tack Time.
                try
                {
                    if (ConstantManager[eConstantXML.CVD_ProportionalRule_Force][line.Data.LINEID].Value.ToUpper() == "TRUE")
                    {
                        Force_ChangeCVDProportionalType = true;
                    }
                }
                catch { }
                #endregion

                #region [ Check Update RobotWIP Condition ]

                #region [ Check Job Location must At Not Robot Home Stage ]

                if (curBcsJob.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStageNo({4}) at RobotArm and can not Update CurRouteStepNo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] Job({1}_{2}) CurStageNo({3}) at RobotArm and can not Update CurRouteStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_Loction_Not_RobotArm_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 由Job中透過目前的CurStep取得目前RouteInfo ]

                RobotRouteStep curRouteStep = null;

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == true)
                {
                    curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
                }

                if (curRouteStep == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurRouteStepNo({4}) can not get RouteInfo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] Job({1}_{2}) CurRouteStepNo({3}) can not get RouteInfo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_RouteInfo_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ 確認目前RouteStepNo中的動作是否為GET ]

                switch (curRouteStep.Data.ROBOTACTION)
                {
                    case eRobot_DB_CommandAction.ACTION_GET:

                        //只有CurRoute Step Action為Get時才能在Job Move To Arm(Arm Load Job)時更新StepNo
                        break;

                    default:


                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) ACTION({5}) is not ({6}) and can not Update CurStepNo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_GET);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        errMsg = string.Format("[{0}] Job({1}_{2}) CurRouteStepNo({3}) ACTION({4}) is not ({5}) and can not Update CurStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_GET);

                        robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_JOB_CurStep_Action_IsNot_GET);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                }

                #endregion

                #endregion
                #region [ Update RobotWIP ]
                lock (curBcsJob)
                {

                    #region [ Update StepNo ]

                    //[ Wait_For_Proc_00015 ][ 針對Step Rule 要如何實作 ]
                    //[ Wait_For_Proc_00016 ][ 針對Recipe by Pass後Step Rule 要如何實作 ]


                    oldStepNo = curBcsJob.RobotWIP.CurStepNo;
                    //20151014 Modity NextStep由WIP來取得
                    newStepNo = curBcsJob.RobotWIP.NextStepNo;// curBcsJob.RobotWIP.CurStepNo + 1;

                    strTmp = string.Format("curStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), newStepNo.ToString());

                    //Update CurStepNo
                    curBcsJob.RobotWIP.CurStepNo = newStepNo;

                    #region [ Get New Current Step Entity to Get NextStep ]

                    //RobotRouteStep newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];

                    RobotRouteStep newCurRouteStep = null;

                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(newStepNo) != false)
                    {

                        newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];
                    }

                    //找不到 CurStep Route 記Log 且將NextStep改為0(異常)
                    if (newCurRouteStep == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4}) Entity!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                newStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.NextStepNo = 0;
                    }
                    else
                    {
                        curBcsJob.RobotWIP.NextStepNo = newCurRouteStep.Data.NEXTSTEPID;

                    }


                    #endregion

                    strTmp = strTmp + string.Format("NextStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString());

                    #endregion

                    #region [ Update RobotJob Status PROCESS ]

                    if (newStepNo == 1)
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.WAIT_PROC);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.WAIT_PROC;

                    }
                    //20151014 Modify 大於65535則算Complete
                    //else if (newStepNo > curBcsJob.RobotWIP.RobotRouteStepList.Count) //最後的Step是準備放到Port 所以必須要是>而不是>=
                    else if (newStepNo >= eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO)
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.COMPLETE);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.COMPLETE;

                    }
                    else
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to ({1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.INPROCESS);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.INPROCESS;
                    }

                    #endregion

                    #region [ Update CurLocation Info ]

                    strTmp = strTmp + string.Format("CurLocation_StageType form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageType, eRobotStageType.ROBOTARM);

                    curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.ROBOTARM;

                    strTmp = strTmp + string.Format("CurLocation_StageID form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

                    string _preLocation_StageID = curBcsJob.RobotWIP.CurLocation_StageID;
                    curBcsJob.RobotWIP.CurLocation_StageID = eRobotCommonConst.ROBOT_HOME_STAGEID;

                    strTmp = strTmp + string.Format("CurLocation_SlotNo form ({0}) to ({1}).", curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(), intArmNo.ToString());

                    curBcsJob.RobotWIP.CurLocation_SlotNo = intArmNo;

                    #endregion

                    ////20160411 add
                    RobotStage _stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(_preLocation_StageID);

                    #region Update PreFetch Info
                    if (_stage != null && _stage.Data.STAGETYPE == eRobotStageType.PORT) if (_stage.Data.PREFETCHFLAG.ToString().ToUpper() == "Y") curBcsJob.RobotWIP.PreFetchFlag++;
                    #endregion

                    #region Update FetchOut DateTime Info
                    //if (_stage != null && _stage.Data.STAGETYPE == eRobotStageType.PORT && curBcsJob.RobotWIP.FetchOutDataTime != DateTime.MinValue)
                    //{
                    DateTime _now = DateTime.Now;
                    strTmp = strTmp + string.Format("FetchOut from ({0}) on ({1}).", _preLocation_StageID, _now.ToString("yyyy-MM-dd HH:mm:ss"));
                    curBcsJob.RobotWIP.FetchOutDataTime = _now;
                    //}
                    #endregion

                    curBcsJob.RobotWIP.PutReadyFlag = 0;
                    curBcsJob.RobotWIP.PutReady_StageID = string.Empty;
                    if (curBcsJob.RobotWIP.PreFetchFlag > 0) //有启动预取!
                    {
                        if (curRobot.CurRealTimeSetCommandInfo != null) //命令还没清空
                        {
                            if (curRobot.CurRealTimeSetCommandInfo.Cmd02_Command == eRobot_Trx_CommandAction.ACTION_PUTREADY)
                            {
                                curBcsJob.RobotWIP.PutReadyFlag = 1; //如果命令是putready, 则更新put ready flag!
                                curBcsJob.RobotWIP.PutReady_StageID = curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition.ToString(); //记录基本预设要去的stage!
                            }
                        }
                    }

                    if (curBcsJob.RobotWIP.RTCReworkFlag)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job[{2}]'s RTCReworkFlag[ON], set OFF.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, string.Format("CST={0}, Slot={1}", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString()));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.RTCReworkFlag = false; //20160108-001-dd::回插再出片, 需要改为OFF并记LOG!!
                    }
                    if (curBcsJob.RobortRTCFlag) curBcsJob.RobortRTCFlag = false; //抽出来要改为OFF

                    if (curBcsJob.RobotWIP.EQPRTCFlag)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job[{2}]'s RobotWIP.EQPRTCFlag[ON], set OFF.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, string.Format("CST={0}, Slot={1}", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString()));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.EQPRTCFlag = false; //抽出来要改为OFF  20161003 add by yang
                    }


                    #region Line Special Result by Line Type
                    //20160107-001-dd
                    Line _line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                    if (_line == null) return false;

                    switch (_line.Data.LINETYPE)
                    {
                        case eLineType.ARRAY.DRY_ICD:
                        case eLineType.ARRAY.DRY_YAC:
                        case eLineType.ARRAY.DRY_TEL:
                            if (_stage != null && _stage.Data.STAGETYPE == eRobotStageType.PORT) curRobot.File.DryLastProcessType = curBcsJob.ArraySpecial.ProcessType.ToString();
                            break;
                        default: break;
                    }
                    #endregion

                    ////
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP {4}",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                    strTmp);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    #region [ 20151209 add For Set FroceRetrunCSTWithoutLDRQ status is Ready ]

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP FroceRetrunCSTWithoutLDRQ From({4}) to ({5})",
                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status, eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_READY);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_READY;

                    #endregion

                }

                //Save File
                ObjectManager.JobManager.EnqueueSave(curBcsJob);

                #endregion


                //modify by hujunpeng 20190425 for CVD700新增一个product进行混run逻辑，Deng，20190823
                if (curRobot.Data.LINEID != "TCCVD700")
                {
                    #region 一种product和MQC混run
                    #region update CVD Fetch Proportional Rule Count 計算比例
                    eCVDIndexRunMode jobProcType = new eCVDIndexRunMode();
                    if (curBcsJob.ArraySpecial.ProcessType == "0")
                        jobProcType = eCVDIndexRunMode.PROD;
                    else
                        jobProcType = eCVDIndexRunMode.MQC;

                    if (jobProcType == curRobot.File.CurCVDProportionalRule.curProportionalType)
                    {
                        //不混抽、不需減片
                        //modify by yang 2016/11/18, CVD这边MIX RUN,不看MIX_MODE
                        // if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                        //  {
                        if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.MQC)
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalMQCCount--;
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                        }
                        else
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalPRODCount--;
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                        }
                        // }

                        #region 檢查是否已抽完？抽完換種，全抽完重新抽
                        //抽完PROD，要把MQC 抽完
                        if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount > 0))
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                        }

                        //抽完MQC、要把PROD抽完
                        if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount > 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0))
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                        }

                        //兩種都抽完了，重抽
                        if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0))
                        {
                            if (curRobot.File.CurCVDProportionalRule.FirstProportionaltype != null)
                                curRobot.File.CurCVDProportionalRule.curProportionalType = curRobot.File.CurCVDProportionalRule.FirstProportionaltype;
                            else
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                            curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                            curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. Reset CVD Proportional PROD:MQC = [{2}:{3}]",
                                                              curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalPRODCount, curRobot.File.CurCVDProportionalRule.curProportionalMQCCount);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }
                        #endregion


                    }
                    else
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) Robot Current Fetch Type({5})  is not Job Process Type({6})",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRobot.File.CurCVDProportionalRule.curProportionalType, curBcsJob.ArraySpecial.ProcessType);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        errMsg = string.Format("[{0}] Job({1}_{2}) CurStepNo({3}) Robot Current Fetch Type({4})  is not Job Process Type({5})",
                                                curRobot.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRobot.File.CurCVDProportionalRule.curProportionalType, curBcsJob.ArraySpecial.ProcessType);

                        robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_ProcessType_Not_RobotArm_ProcessType);
                        robotConText.SetReturnMessage(errMsg);

                        if (!Force_ChangeCVDProportionalType) //一般是需要care tack time, 如果有設定的話，就不再幫忙改變Fetch Type
                        {
                            //自動置換type 只要有單一卡匣就是要能抽，所以不需要管現在是什麼mode
                            if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD)
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                            }
                            else
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                            }
                        }
                        else
                        {
                            int cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD ? curRobot.File.CurCVDProportionalRule.curProportionalPRODCount : curRobot.File.CurCVDProportionalRule.curProportionalMQCCount;

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Force CVD Proportional Rule is True,  No glass could Fetch now !! Must be Wait Another Process Type[{2}] glass count[{3}] = 0 will Change other Type.",
                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalType.ToString(), cvdPropTypeCount);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        return false;
                    }
                    #endregion

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                    return true;
                    #endregion
                }
                else
                {
                    #region 两种product和MQC混run
                    #region update CVD Fetch Proportional Rule Count 計算比例
                    eCVDIndexRunMode jobProcType = new eCVDIndexRunMode();
                    if (curBcsJob.ArraySpecial.ProcessType == "0")
                        jobProcType = eCVDIndexRunMode.PROD;
                    else if (curBcsJob.ArraySpecial.ProcessType == "1")
                        jobProcType = eCVDIndexRunMode.MQC;
                    else if (curBcsJob.ArraySpecial.ProcessType == "2")
                        jobProcType = eCVDIndexRunMode.PROD1;
                    else
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Current jobPorcType{0} is unknow!", curBcsJob.ArraySpecial.ProcessType));

                    if (jobProcType == curRobot.File.CurCVDProportionalRule.curProportionalType)
                    {
                        //不混抽、不需減片
                        //modify by yang 2016/11/18, CVD这边MIX RUN,不看MIX_MODE
                        // if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                        //  {
                        if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount > 0 || curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count > 0) && curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0)
                        {
                            if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD)
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount--;
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                            }
                            else
                            {
                                curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count--;
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                switch (port.File.SamplingCount)
                                {
                                    case 21:
                                    case 14:
                                    case 7:
                                        curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = ParameterManager["CVDPROPORTIONALPRODCOUNT"].GetInteger();
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        else if (curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0 && (curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count > 0 || curRobot.File.CurCVDProportionalRule.curProportionalMQCCount > 0))
                        {
                            if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD1)
                            {
                                curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count--;
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                            }
                            else
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount--;
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                            }
                        }
                        else if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount > 0 || curRobot.File.CurCVDProportionalRule.curProportionalMQCCount > 0) && curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count <= 0)
                        {
                            if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD)
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount--;
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                            }
                            else
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount--;
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                            }
                        }
                        else
                        {
                            if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD)
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount--;
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                            }
                            else if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.MQC)
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount--;
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                            }
                            else
                            {
                                curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count--;
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                            }
                        }

                        // }



                        #region 檢查是否已抽完？抽完換種，全抽完重新抽
                        //抽完PROD，PORD1 要把MQC 抽完
                        if ((curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount > 0))
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                        }

                        //抽完MQC、PORD1 要把PROD抽完
                        if ((curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalPRODCount > 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0))
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                        }

                        //抽完MQC、PORD 要把PROD1抽完
                        if ((curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count > 0) && (curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0))
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                        }

                        //三種都抽完了，重抽
                        if ((curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0))
                        {
                            if (curRobot.File.CurCVDProportionalRule.FirstProportionaltype != null)
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalType = curRobot.File.CurCVDProportionalRule.FirstProportionaltype;
                                if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD) && curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1) && !curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                {
                                    //curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 0;
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
                                    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1];
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. Reset CVD Proportional PROD:PROD1 = [{2}:{3}]",
                                                              curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalPRODCount, curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                else if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD) && curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC) && !curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                                {
                                    //curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
                                    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 0;
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. Reset CVD Proportional PROD:MQC = [{2}:{3}]",
                                                              curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalPRODCount, curRobot.File.CurCVDProportionalRule.curProportionalMQCCount);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                else if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1) && curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC) && !curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                {
                                    //curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 0;
                                    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1];
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. Reset CVD Proportional PROD1:MQC = [{2}:{3}]",
                                                              curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count, curRobot.File.CurCVDProportionalRule.curProportionalMQCCount);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                else
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. But three  CVDProportionalRule types all contains.PROD:MQC:PROD1=[{2}:{3}:{4}]",
                                                              curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD], curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC], curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1]);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            else
                            {
                                if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD) && curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1) && !curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 0;
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
                                    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1];
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. Reset CVD Proportional PROD:PROD1 = [{2}:{3}]",
                                                              curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalPRODCount, curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                else if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD) && curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC) && !curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
                                    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 0;
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. Reset CVD Proportional PROD:MQC = [{2}:{3}]",
                                                              curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalPRODCount, curRobot.File.CurCVDProportionalRule.curProportionalMQCCount);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                else if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1) && curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC) && !curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 0;
                                    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1];
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. Reset CVD Proportional PROD1:MQC = [{2}:{3}]",
                                                              curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count, curRobot.File.CurCVDProportionalRule.curProportionalMQCCount);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                else
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. But three  CVDProportionalRule types all contains.PROD:MQC:PROD1=[{2}:{3}:{4}]",
                                                              curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD], curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC], curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1]);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) Robot Current Fetch Type({5})  is not Job Process Type({6})",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRobot.File.CurCVDProportionalRule.curProportionalType, curBcsJob.ArraySpecial.ProcessType);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        errMsg = string.Format("[{0}] Job({1}_{2}) CurStepNo({3}) Robot Current Fetch Type({4})  is not Job Process Type({5})",
                                                curRobot.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRobot.File.CurCVDProportionalRule.curProportionalType, curBcsJob.ArraySpecial.ProcessType);

                        robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_ProcessType_Not_RobotArm_ProcessType);
                        robotConText.SetReturnMessage(errMsg);

                        if (!Force_ChangeCVDProportionalType) //一般是需要care tack time, 如果有設定的話，就不再幫忙改變Fetch Type
                        {
                            //自動置換type 只要有單一卡匣就是要能抽，所以不需要管現在是什麼mode
                            if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD)
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                            }
                            else if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.MQC)
                            {
                                curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1];
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                            }
                            else
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                            }
                        }
                        else
                        {
                            int cvdPropTypeCount = 0;
                            switch (curRobot.File.CurCVDProportionalRule.curProportionalType)
                            {
                                case eCVDIndexRunMode.PROD:
                                    cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curProportionalPRODCount;
                                    break;
                                case eCVDIndexRunMode.MQC:
                                    cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curProportionalMQCCount;
                                    break;
                                case eCVDIndexRunMode.PROD1:
                                    cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count;
                                    break;
                                default:
                                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("curProportionalType{0} is unknow!", curRobot.File.CurCVDProportionalRule.curProportionalType.ToString()));
                                    break;
                            }

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Force CVD Proportional Rule is True,  No glass could Fetch now !! Must be Wait Another Process Type[{2}] glass count[{3}] = 0 will Change other Type.",
                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalType.ToString(), cvdPropTypeCount);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        return false;
                    }
                    #endregion

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                    return true;
                    /*
                    #region update CVD Fetch Proportional Rule Count 計算比例
                    eCVDIndexRunMode jobProcType = new eCVDIndexRunMode();
                    if (curBcsJob.ArraySpecial.ProcessType == "0")
                        jobProcType = eCVDIndexRunMode.PROD;
                    else if (curBcsJob.ArraySpecial.ProcessType == "1")
                        jobProcType = eCVDIndexRunMode.MQC;
                    else if (curBcsJob.ArraySpecial.ProcessType == "2")
                        jobProcType = eCVDIndexRunMode.PROD1;
                    else
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Current jobPorcType{0} is unknow!", curBcsJob.ArraySpecial.ProcessType));

                    
                    if (jobProcType == curRobot.File.CurCVDProportionalRule.curProportionalType)
                    {
                        //不混抽、不需減片
                        //modify by yang 2016/11/18, CVD这边MIX RUN,不看MIX_MODE
                        // if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                        //  {
                        if(curRobot.File.CVDProportionalRule!=null)
                        {
                            if(curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD]>0&&curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1]>0)
                            {
                                if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD)
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount--;
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                }
                                else
                                {
                                    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count--;
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                }
                                #region 檢查是否已抽完？抽完換種，全抽完重新抽
                                //抽完PROD，要把PROD1 抽完
                                if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0) && (curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count > 0))
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                }

                                //抽完PROD1、要把PROD抽完
                                if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount > 0) && (curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count <= 0))
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                }

                                //兩種都抽完了，重抽
                                if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0) && (curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count <= 0))
                                {
                                    if (curRobot.File.CurCVDProportionalRule.FirstProportionaltype != null)
                                        curRobot.File.CurCVDProportionalRule.curProportionalType = curRobot.File.CurCVDProportionalRule.FirstProportionaltype;
                                    else
                                        curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1];
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. Reset CVD Proportional PROD:PROD1 = [{2}:{3}]",
                                                                      curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalPRODCount, curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                }
                                #endregion
                            }
                            if(curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD]>0&&curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC]>0)
                            {
                                if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD)
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount--;
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                }
                                else
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount--;
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                }
                                #region 檢查是否已抽完？抽完換種，全抽完重新抽
                                //抽完PROD，要把MQC 抽完
                                if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount > 0))
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                }

                                //抽完MQC、要把PROD抽完
                                if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount > 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0))
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                }

                                //兩種都抽完了，重抽
                                if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0))
                                {
                                    if (curRobot.File.CurCVDProportionalRule.FirstProportionaltype != null)
                                        curRobot.File.CurCVDProportionalRule.curProportionalType = curRobot.File.CurCVDProportionalRule.FirstProportionaltype;
                                    else
                                        curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. Reset CVD Proportional PROD:MQC = [{2}:{3}]",
                                                                      curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalPRODCount, curRobot.File.CurCVDProportionalRule.curProportionalMQCCount);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                }
                                #endregion
                            }
                            if(curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1]>0&&curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC]>0)
                            {
                                if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD1)
                                {
                                    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count--;
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                }
                                else
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount--;
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                }
                                #region 檢查是否已抽完？抽完換種，全抽完重新抽
                                //抽完PROD1，要把MQC 抽完
                                if ((curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount > 0))
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                }

                                //抽完MQC、要把PROD1抽完
                                if ((curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count > 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0))
                                {
                                    curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                }

                                //兩種都抽完了，重抽
                                if ((curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0))
                                {
                                    if (curRobot.File.CurCVDProportionalRule.FirstProportionaltype != null)
                                        curRobot.File.CurCVDProportionalRule.curProportionalType = curRobot.File.CurCVDProportionalRule.FirstProportionaltype;
                                    else
                                        curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                                    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1];

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. Reset CVD Proportional PROD1:MQC = [{2}:{3}]",
                                                                      curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count, curRobot.File.CurCVDProportionalRule.curProportionalMQCCount);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                }
                                #endregion
                            }
                        }                      
                        // }
                    }
                    else
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) Robot Current Fetch Type({5})  is not Job Process Type({6})",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRobot.File.CurCVDProportionalRule.curProportionalType, curBcsJob.ArraySpecial.ProcessType);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        errMsg = string.Format("[{0}] Job({1}_{2}) CurStepNo({3}) Robot Current Fetch Type({4})  is not Job Process Type({5})",
                                                curRobot.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRobot.File.CurCVDProportionalRule.curProportionalType, curBcsJob.ArraySpecial.ProcessType);

                        robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_ProcessType_Not_RobotArm_ProcessType);
                        robotConText.SetReturnMessage(errMsg);

                        if (!Force_ChangeCVDProportionalType) //一般是需要care tack time, 如果有設定的話，就不再幫忙改變Fetch Type
                        {
                            //自動置換type 只要有單一卡匣就是要能抽，所以不需要管現在是什麼mode
                            if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD)
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                            }
                            else
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                            }
                        }
                        else
                        {
                            int cvdPropTypeCount = 0;
                            switch (curRobot.File.CurCVDProportionalRule.curProportionalType)
                            {
                                case eCVDIndexRunMode.PROD:
                                    cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curProportionalPRODCount;
                                    break;
                                case eCVDIndexRunMode.MQC:
                                    cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curProportionalMQCCount;
                                    break;
                                case eCVDIndexRunMode.PROD1:
                                    cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count;
                                    break;
                                default:
                                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("curProportionalType{0} is unknow!", curRobot.File.CurCVDProportionalRule.curProportionalType.ToString()));
                                    break;
                            }

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Force CVD Proportional Rule is True,  No glass could Fetch now !! Must be Wait Another Process Type[{2}] glass count[{3}] = 0 will Change other Type.",
                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalType.ToString(), cvdPropTypeCount);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        return false;
                    }
                    #endregion

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                    return true;
                      */
                    #endregion
                }

                #region 曾经代码
                /*
                #region update CVD Fetch Proportional Rule Count 計算比例
                eCVDIndexRunMode jobProcType = new eCVDIndexRunMode();
                if (curBcsJob.ArraySpecial.ProcessType == "0")
                    jobProcType = eCVDIndexRunMode.PROD;
                else
                    jobProcType = eCVDIndexRunMode.MQC;

                if (jobProcType == curRobot.File.CurCVDProportionalRule.curProportionalType)
                {
                    //不混抽、不需減片
                    //modify by yang 2016/11/18, CVD这边MIX RUN,不看MIX_MODE
                   // if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
                  //  {
                        if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.MQC)
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalMQCCount--;
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                        }
                        else
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalPRODCount--;
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                        }
                   // }

                    #region 檢查是否已抽完？抽完換種，全抽完重新抽
                        //抽完PROD，要把MQC 抽完
                        if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount > 0))
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                        }

                        //抽完MQC、要把PROD抽完
                        if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount > 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0))
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                        }

                        //兩種都抽完了，重抽
                        if ((curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0) && (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0))
                        {
                            if (curRobot.File.CurCVDProportionalRule.FirstProportionaltype != null)
                                curRobot.File.CurCVDProportionalRule.curProportionalType = curRobot.File.CurCVDProportionalRule.FirstProportionaltype;
                            else
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                            curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                            curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CVD Proportional Count is End. Reset CVD Proportional PROD:MQC = [{2}:{3}]",
                                                              curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalPRODCount, curRobot.File.CurCVDProportionalRule.curProportionalMQCCount);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }
                    #endregion


                }
                else
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) Robot Current Fetch Type({5})  is not Job Process Type({6})",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRobot.File.CurCVDProportionalRule.curProportionalType, curBcsJob.ArraySpecial.ProcessType);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] Job({1}_{2}) CurStepNo({3}) Robot Current Fetch Type({4})  is not Job Process Type({5})",
                                            curRobot.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRobot.File.CurCVDProportionalRule.curProportionalType, curBcsJob.ArraySpecial.ProcessType);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_ProcessType_Not_RobotArm_ProcessType);
                    robotConText.SetReturnMessage(errMsg);

                    if (!Force_ChangeCVDProportionalType) //一般是需要care tack time, 如果有設定的話，就不再幫忙改變Fetch Type
                    {
                        //自動置換type 只要有單一卡匣就是要能抽，所以不需要管現在是什麼mode
                        if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD)
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                        }
                        else
                        {
                            curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                        }
                    }
                    else
                    {
                        int cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD?curRobot.File.CurCVDProportionalRule.curProportionalPRODCount:curRobot.File.CurCVDProportionalRule.curProportionalMQCCount;
                        
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Force CVD Proportional Rule is True,  No glass could Fetch now !! Must be Wait Another Process Type[{2}] glass count[{3}] = 0 will Change other Type.",
                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalType.ToString(), cvdPropTypeCount);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    return false;
                }
                #endregion

                
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                return true;
                 */
                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }

        //Watson Add 20151030 [目前無用 抽完換種]
        //改寫自ProcResult_JobMoveToRobotArm_1Arm1Job 運用在CVD的抽片比例上
        //在MIX MODE下才會在每抽一片就計算一次(Count -1)
        //其餘更新資料部份與ProcResult_JobMoveToRobotArm_1Arm1Job相同
        //20151120 add FuncKey
        //[UniAuto.UniBCS.OpiSpec.Help("PR0003")]
        //public bool ProcResult_CVDFetchProportionalRule(IRobotContext robotConText)
        //{
        //    try
        //    {
        //        string errMsg = string.Empty;
        //        string strlog = string.Empty;
        //        string strTmp = string.Empty;
        //        int oldStepNo = 0;
        //        int newStepNo = 0;
        //        bool Force_ChangeCVDProportionalType = false;

        //        #region [ Get curRobot Entity ]

        //        Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

        //        //找不到 Robot 回NG
        //        if (curRobot == null)
        //        {

        //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
        //                                    "L1", MethodBase.GetCurrentMethod().Name);

        //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //            errMsg = string.Format("[{0}] can not Get Robot!",
        //                                    MethodBase.GetCurrentMethod().Name);

        //            robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curRobot_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get curBcsJob Entity ]

        //        Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

        //        //找不到 BcsJob 回NG
        //        if (curBcsJob == null)
        //        {

        //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob!",
        //                                    "L1", MethodBase.GetCurrentMethod().Name);

        //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //            errMsg = string.Format("[{0}] can not Get BcsJob!",
        //                                    MethodBase.GetCurrentMethod().Name);

        //            robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curBcsJob_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ Get curBCSJob Load ArmNo ]

        //        string curLoadArmNo = (string)robotConText[eRobotContextParameter.LoadJobArmNo_For_1Arm_1Job];

        //        //找不到 CurLoadArmNo 回NG
        //        if (curLoadArmNo == null || curLoadArmNo == string.Empty)
        //        {

        //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Load ArmNo!",
        //                                    "L1", MethodBase.GetCurrentMethod().Name);

        //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //            errMsg = string.Format("[{0}] can not Get Load ArmNo!",
        //                                    MethodBase.GetCurrentMethod().Name);

        //            robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_CurLoadArmNo_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        int intArmNo = 0;

        //        int.TryParse(curLoadArmNo, out intArmNo);

        //        #endregion

        //        #region [ Get cur EQP ]
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

        //        //找不到 Node 回NG
        //        if (eqp == null)
        //        {

        //            #region[DebugLog]
        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot Node No!",
        //                                        "L1", MethodBase.GetCurrentMethod().Name);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }
        //            #endregion

        //            errMsg = string.Format("[{0}] can not Get Robot Node No!",
        //                                    MethodBase.GetCurrentMethod().Name);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }
        //        #endregion

        //        #region [ Get Line ]
        //        Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

        //        //找不到 Node 回NG
        //        if (line == null)
        //        {

        //            #region[DebugLog]
        //            if (IsShowDetialLog == true)
        //            {
        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot Line ID!",
        //                                        "L1", MethodBase.GetCurrentMethod().Name);

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
        //            }
        //            #endregion

        //            errMsg = string.Format("[{0}] can not Get Robot Line ID!",
        //                                    MethodBase.GetCurrentMethod().Name);

        //            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LINE_Is_Null);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }
        //        #endregion

        //        #region Force CVD Proportional Type Don't Care Tack Time.
        //        try
        //        {
        //            if (ConstantManager[eConstantXML.CVD_ProportionalRule_Force][line.Data.LINEID].Value.ToUpper() == "TRUE")
        //            {
        //                Force_ChangeCVDProportionalType = true;
        //            }
        //        }
        //        catch { }
        //        #endregion

        //        #region [ Check Update RobotWIP Condition ]

        //        #region [ Check Job Location must At Not Robot Home Stage ]

        //        if (curBcsJob.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
        //        {

        //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStageNo({4}) at RobotArm and can not Update CurRouteStepNo!",
        //                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurLocation_StageID);

        //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //            errMsg = string.Format("[{0}]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStageNo({4}) at RobotArm and can not Update CurRouteStepNo!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurLocation_StageID);

        //            robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_Loction_Not_RobotArm_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;

        //        }

        //        #endregion

        //        #region [ 由Job中透過目前的CurStep取得目前RouteInfo ]

        //        RobotRouteStep curRouteStep = null;

        //        if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == true)
        //        {
        //            curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
        //        }

        //        if (curRouteStep == null)
        //        {

        //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurRouteStepNo({4}) can not get RouteInfo!",
        //                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurStepNo.ToString());

        //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //            errMsg = string.Format("[{0}]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurRouteStepNo({4}) can not get RouteInfo!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurStepNo.ToString());

        //            robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_RouteInfo_Fail);
        //            robotConText.SetReturnMessage(errMsg);

        //            return false;
        //        }

        //        #endregion

        //        #region [ 確認目前RouteStepNo中的動作是否為GET ]

        //        switch (curRouteStep.Data.ROBOTACTION)
        //        {
        //            case eRobot_DB_CommandAction.ACTION_GET:

        //                //只有CurRoute Step Action為Get時才能在Job Move To Arm(Arm Load Job)時更新StepNo
        //                break;

        //            default:


        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) ACTION({5}) is not ({6}) and can not Update CurStepNo!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_GET);
        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //                errMsg = string.Format("[{0}]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurRouteStepNo({4}) ACTION({5}) is not ({6}) and can not Update CurStepNo!",
        //                                    MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_GET);

        //                robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_JOB_CurStep_Action_IsNot_GET);
        //                robotConText.SetReturnMessage(errMsg);

        //                return false;
        //        }

        //        #endregion

        //        #endregion

        //        #region [ Update RobotWIP ]
        //        lock (curBcsJob)
        //        {

        //            #region [ Update StepNo ]

        //            //[ Wait_For_Proc_00015 ][ 針對Step Rule 要如何實作 ]
        //            //[ Wait_For_Proc_00016 ][ 針對Recipe by Pass後Step Rule 要如何實作 ]


        //            oldStepNo = curBcsJob.RobotWIP.CurStepNo;
        //            //20151014 Modity NextStep由WIP來取得
        //            newStepNo = curBcsJob.RobotWIP.NextStepNo;// curBcsJob.RobotWIP.CurStepNo + 1;

        //            strTmp = string.Format("curStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), newStepNo.ToString());

        //            //Update CurStepNo
        //            curBcsJob.RobotWIP.CurStepNo = newStepNo;

        //            #region [ Get New Current Step Entity to Get NextStep ]

        //            //RobotRouteStep newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];

        //            RobotRouteStep newCurRouteStep = null;

        //            if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(newStepNo) != false)
        //            {

        //                newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];
        //            }

        //            //找不到 CurStep Route 記Log 且將NextStep改為0(異常)
        //            if (newCurRouteStep == null)
        //            {

        //                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4}) Entity!",
        //                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                        newStepNo.ToString());

        //                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //                curBcsJob.RobotWIP.NextStepNo = 0;
        //            }
        //            else
        //            {
        //                curBcsJob.RobotWIP.NextStepNo = newCurRouteStep.Data.NEXTSTEPID;

        //            }


        //            #endregion

        //            strTmp = strTmp + string.Format("NextStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString());

        //            #endregion

        //            #region [ Update RobotJob Status PROCESS ]

        //            if (newStepNo == 1)
        //            {
        //                strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.WAIT_PROC);

        //                curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.WAIT_PROC;

        //            }
        //            //20151014 Modify 大於65535則算Complete
        //            //else if (newStepNo > curBcsJob.RobotWIP.RobotRouteStepList.Count) //最後的Step是準備放到Port 所以必須要是>而不是>=
        //            else if (newStepNo >= eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO) 
        //            {
        //                strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.COMPLETE);

        //                curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.COMPLETE;

        //            }
        //            else
        //            {
        //                strTmp = strTmp + string.Format("CurJobStatus form ({0}) to ({1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.INPROCESS);

        //                curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.INPROCESS;
        //            }

        //            #endregion

        //            #region [ Update CurLocation Info ]

        //            strTmp = strTmp + string.Format("CurLocation_StageType form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageType, eRobotStageType.ROBOTARM);

        //            curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.ROBOTARM;

        //            strTmp = strTmp + string.Format("CurLocation_StageID form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

        //            curBcsJob.RobotWIP.CurLocation_StageID = eRobotCommonConst.ROBOT_HOME_STAGEID;

        //            strTmp = strTmp + string.Format("CurLocation_SlotNo form ({0}) to ({1}).", curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(), intArmNo.ToString());

        //            curBcsJob.RobotWIP.CurLocation_SlotNo = intArmNo;

        //            #endregion

        //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP {4}",
        //                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                                            strTmp);
        //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //            #region [ 20151209 add For Set FroceRetrunCSTWithoutLDRQ status is NotCheck ]

        //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP FroceRetrunCSTWithoutLDRQ From({4}) to ({5})",
        //                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status, eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_NOTCHECK);
        //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //            curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_NOTCHECK;

        //            #endregion

        //        }

        //        //Save File
        //        ObjectManager.JobManager.EnqueueSave(curBcsJob);

        //        #endregion

        //        #region update CVD Fetch Proportional Rule Count 計算比例


        //        if (curBcsJob.ArraySpecial.ProcessType == curRobot.File.CurCVDProportionalRule.curProportionalType.ToString())
        //        {
        //            //不混抽、不需減片
        //            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MIX_MODE)
        //            {
        //                curRobot.File.CurCVDProportionalRule.curProportionalCount--;
        //            }

        //            if (curRobot.File.CurCVDProportionalRule.curProportionalCount <= 0)
        //            {
        //                if (curRobot.File.CurCVDProportionalRule.curProportionalType == (int)eCVDIndexRunMode.PROD)
        //                {
        //                    curRobot.File.CurCVDProportionalRule.curProportionalCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
        //                    curRobot.File.CurCVDProportionalRule.curProportionalType = (int)eCVDIndexRunMode.MQC;
        //                }
        //                else
        //                {
        //                    curRobot.File.CurCVDProportionalRule.curProportionalCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
        //                    curRobot.File.CurCVDProportionalRule.curProportionalType = (int)eCVDIndexRunMode.PROD;
        //                }
        //            }

        //        }
        //        else
        //        {
        //            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) ACTION({5}) Robot Current Fetch Type({6})  is not Job Process Type({7})",
        //                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurStepNo.ToString(), curRobot.File.CurCVDProportionalRule.curProportionalType, curBcsJob.ArraySpecial.ProcessType);
        //            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

        //            errMsg = string.Format("[{0}]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) ACTION({5}) Robot Current Fetch Type({6})  is not Job Process Type({7})",
        //                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
        //                                    curBcsJob.RobotWIP.CurStepNo.ToString(), curRobot.File.CurCVDProportionalRule.curProportionalType, curBcsJob.ArraySpecial.ProcessType);

        //            robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_ProcessType_Not_RobotArm_ProcessType);
        //            robotConText.SetReturnMessage(errMsg);

        //            if (!Force_ChangeCVDProportionalType) //一般是需要care tack time, 如果有設定的話，就不再幫忙改變Fetch Type
        //            {
        //                //自動置換type 只要有單一卡匣就是要能抽，所以不需要管現在是什麼mode
        //                if (curRobot.File.CurCVDProportionalRule.curProportionalType == (int)eCVDIndexRunMode.PROD)
        //                {
        //                    curRobot.File.CurCVDProportionalRule.curProportionalCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
        //                    curRobot.File.CurCVDProportionalRule.curProportionalType = (int)eCVDIndexRunMode.MQC;
        //                }
        //                else
        //                {
        //                    curRobot.File.CurCVDProportionalRule.curProportionalCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
        //                    curRobot.File.CurCVDProportionalRule.curProportionalType = (int)eCVDIndexRunMode.PROD;
        //                }
        //            }

        //            return false;
        //        }
        //        #endregion

        //        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
        //        robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

        //        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
        //        robotConText.SetReturnMessage(ex.Message);

        //        return false;
        //    }

        //}

        //20151209 add for FroceRetrunCSTWithoutLDRQ 


        /// <summary> Route Step "GET Port Type Stage Job" Process Result OK時的相關處理
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("PR0004")]
        public bool ProcResult_JobMoveToRobotArm_1Arm1Job_forCSTFetch(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                string strTmp = string.Empty;
                int oldStepNo = 0;
                int newStepNo = 0;

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBcsJob Entity ]

                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

                //找不到 BcsJob 回NG
                if (curBcsJob == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get BcsJob!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBCSJob Load ArmNo ]

                string curLoadArmNo = (string)robotConText[eRobotContextParameter.LoadJobArmNo_For_1Arm_1Job];

                //找不到 CurLoadArmNo 回NG
                if (curLoadArmNo == null || curLoadArmNo == string.Empty)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Load ArmNo!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Load ArmNo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_CurLoadArmNo_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                int intArmNo = 0;

                int.TryParse(curLoadArmNo, out intArmNo);

                #endregion

                #region [ Check Update RobotWIP Condition ]

                #region [ Check Job Location must At Not Robot Home Stage ]

                if (curBcsJob.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStageNo({4}) at RobotArm and can not Update CurRouteStepNo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] Job({1}_{2}) CurStageNo({3}) at RobotArm and can not Update CurRouteStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_Loction_Not_RobotArm_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 由Job中透過目前的CurStep取得目前RouteInfo ]

                RobotRouteStep curRouteStep = null;

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == true)
                {
                    curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
                }

                if (curRouteStep == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurRouteStepNo({4}) can not get RouteInfo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] Job({1}_{2}) CurRouteStepNo({3}) can not get RouteInfo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_RouteInfo_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ 確認目前RouteStepNo中的動作是否為GET ]

                switch (curRouteStep.Data.ROBOTACTION)
                {
                    case eRobot_DB_CommandAction.ACTION_GET:

                        //只有CurRoute Step Action為Get時才能在Job Move To Arm(Arm Load Job)時更新StepNo
                        break;

                    default:


                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) ACTION({5}) is not ({6}) and can not Update CurStepNo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_GET);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        errMsg = string.Format("[{0}] Job({1}_{2}) CurRouteStepNo({3}) ACTION({4}) is not ({5}) and can not Update CurStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_GET);

                        robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_JOB_CurStep_Action_IsNot_GET);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                }

                #endregion

                #endregion

                #region [ Update RobotWIP ]

                lock (curBcsJob)
                {

                    #region [ Update StepNo ]

                    //[ Wait_For_Proc_00015 ][ 針對Step Rule 要如何實作 ]
                    //[ Wait_For_Proc_00016 ][ 針對Recipe by Pass後Step Rule 要如何實作 ]


                    oldStepNo = curBcsJob.RobotWIP.CurStepNo;
                    //20151014 Modity NextStep由WIP來取得
                    newStepNo = curBcsJob.RobotWIP.NextStepNo; // curBcsJob.RobotWIP.CurStepNo + 1;

                    strTmp = string.Format("curStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), newStepNo.ToString());

                    //Update CurStepNo and NextStep
                    curBcsJob.RobotWIP.CurStepNo = newStepNo;

                    #region [ Get New Current Step Entity to Get NextStep ]

                    //RobotRouteStep newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];

                    RobotRouteStep newCurRouteStep = null;

                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(newStepNo) != false)
                    {

                        newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];
                    }

                    //找不到 CurStep Route 記Log 且將NextStep改為0(異常)
                    if (newCurRouteStep == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4}) Entity!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                newStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.NextStepNo = 0;
                    }
                    else
                    {
                        curBcsJob.RobotWIP.NextStepNo = newCurRouteStep.Data.NEXTSTEPID;

                    }


                    #endregion

                    strTmp = strTmp + string.Format("NextStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString());

                    #endregion

                    #region [ Update RobotJob Status PROCESS ]

                    if (newStepNo == 1)
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.WAIT_PROC);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.WAIT_PROC;

                    }
                    //20151014 Modify 大於65535則算Complete
                    //else if (newStepNo > curBcsJob.RobotWIP.RobotRouteStepList.Count) //最後的Step是準備放到Port 所以必須要是>而不是>=
                    else if (newStepNo >= eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO)
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.COMPLETE);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.COMPLETE;

                    }
                    else
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to ({1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.INPROCESS);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.INPROCESS;
                    }

                    #endregion

                    #region [ Update CurLocation Info ]

                    strTmp = strTmp + string.Format("CurLocation_StageType form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageType, eRobotStageType.ROBOTARM);

                    curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.ROBOTARM;

                    strTmp = strTmp + string.Format("CurLocation_StageID form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

                    string _preLocation_StageID = curBcsJob.RobotWIP.CurLocation_StageID;
                    curBcsJob.RobotWIP.CurLocation_StageID = eRobotCommonConst.ROBOT_HOME_STAGEID;

                    strTmp = strTmp + string.Format("CurLocation_SlotNo form ({0}) to ({1}).", curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(), intArmNo.ToString());

                    curBcsJob.RobotWIP.CurLocation_SlotNo = intArmNo;

                    #endregion

                    RobotStage _stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(_preLocation_StageID);

                    #region Update PreFetch Info
                    if (_stage != null && _stage.Data.STAGETYPE == eRobotStageType.PORT) if (_stage.Data.PREFETCHFLAG.ToString().ToUpper() == "Y") curBcsJob.RobotWIP.PreFetchFlag++;
                    #endregion

                    #region Update FetchOut DateTime Info
                    //if (_stage != null && _stage.Data.STAGETYPE == eRobotStageType.PORT && curBcsJob.RobotWIP.FetchOutDataTime != DateTime.MinValue)
                    //{
                    DateTime _now = DateTime.Now;
                    strTmp = strTmp + string.Format("FetchOut from ({0}) on ({1}).", _preLocation_StageID, _now.ToString("yyyy-MM-dd HH:mm:ss"));
                    curBcsJob.RobotWIP.FetchOutDataTime = _now;
                    //}
                    #endregion

                    curBcsJob.RobotWIP.PutReadyFlag = 0;
                    curBcsJob.RobotWIP.PutReady_StageID = string.Empty;
                    if (curBcsJob.RobotWIP.PreFetchFlag > 0) //有启动预取!
                    {
                        if (curRobot.CurRealTimeSetCommandInfo != null) //命令还没清空
                        {
                            if (curRobot.CurRealTimeSetCommandInfo.Cmd02_Command == eRobot_Trx_CommandAction.ACTION_PUTREADY)
                            {
                                curBcsJob.RobotWIP.PutReadyFlag = 1; //如果命令是putready, 则更新put ready flag!
                                curBcsJob.RobotWIP.PutReady_StageID = curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition.ToString(); //记录基本预设要去的stage!
                            }
                        }
                    }

                    if (curBcsJob.RobotWIP.RTCReworkFlag)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job[{2}]'s RTCReworkFlag[ON], set OFF.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, string.Format("CST={0}, Slot={1}", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString()));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.RTCReworkFlag = false; //20160108-001-dd::回插再出片, 需要改为OFF并记LOG!!
                    }
                    if (curBcsJob.RobortRTCFlag) curBcsJob.RobortRTCFlag = false; //抽出来要改为OFF

                    if (curBcsJob.RobotWIP.EQPRTCFlag)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job[{2}]'s RobotWIP.EQPRTCFlag[ON], set OFF.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, string.Format("CST={0}, Slot={1}", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString()));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.EQPRTCFlag = false; //抽出来要改为OFF  20161003 add by yang
                    }

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP {4}",
                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, strTmp);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    #region [ 20151209 add For Set FroceRetrunCSTWithoutLDRQ status is Ready ]

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP FroceRetrunCSTWithoutLDRQ From({4}) to ({5})",
                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status, eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_READY);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_READY;

                    #endregion

                }
                if (curRobot.Data.LINETYPE== eLineType.ARRAY.ELA_JSW) //add by qiumin 20171017 ELA one by one run
                {
                    if (curBcsJob.ArraySpecial.ProcessType.Trim().Equals("1"))
                    {
                        curRobot.File.CurELAEQPType = "M";
                        curRobot.File.CurELAEQPChangeflag = "Y";
                        strlog = string.Format("Robot step one filish  CurELAEQPType change to({0}),ELA EQP Change flag({1})!", curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }
                    else
                    {
                        curRobot.File.CurELAEQPType = curBcsJob.ArraySpecial.ELA1BY1Flag;
                        curRobot.File.CurELAEQPChangeflag = "Y";
                        strlog = string.Format("Robot step one filish  CurELAEQPType change to({0}),ELA EQP Change flag({1})!", curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                }

                //Save File
                ObjectManager.JobManager.EnqueueSave(curBcsJob);

                #endregion

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }

        [UniAuto.UniBCS.OpiSpec.Help("PR0005")]
        public bool ProcResult_ArmJobMoveToStage_1Arm1Job_forFixTargetStage(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                string strTmp = string.Empty;

                #region FixTargetStage_RobotParam fix_target_stage
                FixTargetStage_RobotParam fix_param = null;
                if (StaticContext.ContainsKey(eRobotContextParameter.FixTargetStage_RobotParam) &&
                    StaticContext[eRobotContextParameter.FixTargetStage_RobotParam] is FixTargetStage_RobotParam)
                {
                    fix_param = (FixTargetStage_RobotParam)StaticContext[eRobotContextParameter.FixTargetStage_RobotParam];
                }
                if (fix_param == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] StaticContext has no FixTargetStage",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion

                #region [ Get Target Stage Entity ]
                RobotStage curTargetStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(robotConText[eRobotContextParameter.TargetStageID].ToString());

                //找不到 Target Stage 回NG
                if (curTargetStage == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get TargetStage!", "L1", MethodBase.GetCurrentMethod().Name);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get TargetStage!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_TargetStage_Is_Null);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                fix_param.STAGEID = curTargetStage.Data.STAGEID;
                fix_param.SetFixDateTime = true;
                
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }

        /// <summary> 
        /// 如果有經過Sub Chamber，該Step就一定要設定，不然會無法判斷
        /// For CF MQCTTP Route Step "ArmJob to Put Stage" Process Result OK時的相關處理
        /// MQC TTP processed by sub Chamber not update Tracking Data.
        /// 但robot 需要以此資料為判斷之依據，所以新增
        ///  即使未經Sub Chamber機台的Step也設定此result也沒關係，
        ///  因為Glass Flow也不會有此機台. 
        /// 此bit只在Aging Enable/Disable 時判斷要不要多增/減Sub Chamber的Flow.
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151211 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("PR0006")]
        public bool ProcResult_ArmJobMoveToStage_1Arm1Job_forCFMQCTTPChamber(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                string strTmp = string.Empty;
                int oldStepNo = 0;
                int newStepNo = 0;
                RobotRouteStep oldStep = null;

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curRobot_Is_Null);
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

                #region [ Get curBcsJob Entity ]

                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

                //找不到 BcsJob 回NG
                if (curBcsJob == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get BcsJob!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                #endregion

                #region [ Get curBCSJob Unload ArmNo ]

                string curUnloadArmNo = (string)robotConText[eRobotContextParameter.UnloadJobArmNo_For_1Arm_1Job];

                //找不到 CurLoadArmNo 回NG
                if (curUnloadArmNo == null || curUnloadArmNo == string.Empty)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Unload ArmNo!",
                                            "L1", MethodBase.GetCurrentMethod().Name);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Unload ArmNo!",
                                            MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_CurUnloadArmNo_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                int intArmNo = 0;

                int.TryParse(curUnloadArmNo, out intArmNo);

                #endregion

                #region [ Get Target Stage Entity ]

                string tmpTargetStageID = (string)robotConText[eRobotContextParameter.TargetStageID];

                RobotStage curTargetStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(tmpTargetStageID);

                //找不到 Target Stage 回NG
                if (curTargetStage == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get TargetStage!",
                                            "L1", MethodBase.GetCurrentMethod().Name);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get TargetStage!",
                                            MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_TargetStage_Is_Null);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                #endregion

                #region [ Get Target SlotNo ]

                string tmpTargetSlotNo = (string)robotConText[eRobotContextParameter.TargetSlotNo];
                int intTargetSlotNo = 0;
                int.TryParse(tmpTargetSlotNo, out intTargetSlotNo);

                //找不到 CurLoadArmNo 回NG
                if (intTargetSlotNo == 0)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Target SlotNo!",
                                            "L1", MethodBase.GetCurrentMethod().Name);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Target SlotNo!",
                                            MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_TargetSlotNo_Is_Null);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                #endregion

                #region [ Check Update RobotWIP Condition ]

                #region [ Check Job Location must At Robot Home Stage ]

                if (curBcsJob.RobotWIP.CurLocation_StageID != eRobotCommonConst.ROBOT_HOME_STAGEID)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStageNo({4}) Not On RobotArm and can not Update CurRouteStepNo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] Job({1}_{2}) CurStageNo({3}) Not On RobotArm and can not Update CurRouteStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_Loction_On_RobotArm_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                #endregion

                #region [ 由Job中透過目前的CurStep取得目前RouteInfo ]

                RobotRouteStep curRouteStep = null;

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == true)
                {
                    curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
                    oldStep = curRouteStep;
                }

                if (curRouteStep == null)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurRouteStepNo({4}) can not get RouteInfo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] Job({1}_{2}) CurRouteStepNo({3}) can not get RouteInfo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_RouteInfo_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                #endregion

                #region [ 確認目前RouteStepNo中的動作是否為PUT OR EXCHANGE ]

                switch (curRouteStep.Data.ROBOTACTION)
                {
                    case eRobot_DB_CommandAction.ACTION_PUT:
                    case eRobot_DB_CommandAction.ACTION_EXCHANGE:
                    case eRobot_DB_CommandAction.ACTION_GETPUT:
                        //只有CurRoute Step Action為PUT or Exchange(GET&PUT)時才能在Job Move To Stage(Arm Unload Job)時更新StepNo
                        break;

                    default:
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) ACTION({5}) is not ({6}) or ({7}) or ({8}) and can not Update CurStepNo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_PUT, eRobot_DB_CommandAction.ACTION_EXCHANGE, eRobot_DB_CommandAction.ACTION_GETPUT);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        errMsg = string.Format("[{0}] Job({1}_{2}) CurRouteStepNo({3}) ACTION({4}) is not ({5}) or ({6}) or({7}) and can not Update CurStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_PUT, eRobot_DB_CommandAction.ACTION_EXCHANGE, eRobot_DB_CommandAction.ACTION_GETPUT);

                        robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_JOB_CurStep_Action_IsNot_PUT_EX);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                }

                #endregion

                #endregion

                #region [ Update RobotWIP ]

                lock (curBcsJob)
                {

                    #region [ Update StepNo ]

                    //[ Wait_For_Proc_00015 ][ 針對Step Rule 要如何實作 ]
                    //[ Wait_For_Proc_00016 ][ 針對Recipe by Pass後Step Rule 要如何實作 ]
                    oldStepNo = curBcsJob.RobotWIP.CurStepNo;

                    //20151014 Modity NextStep由WIP來取得
                    newStepNo = curBcsJob.RobotWIP.NextStepNo; // curBcsJob.RobotWIP.CurStepNo + 1;

                    #region [ special for CF buffering function,update NextStep to first step ]
                    if (robotLine.Data.FABTYPE == eFabType.CF.ToString())
                    {
                        //target為port且job為buffering
                        if (curTargetStage.Data.STAGETYPE == eStageType.PORT && curBcsJob.CfSpecial.RCSBufferingFlag == "1")
                        {
                            int jobfromslot;
                            int.TryParse(curBcsJob.FromSlotNo, out jobfromslot);

                            Port curTargetPort = ObjectManager.PortManager.GetPort(curTargetStage.Data.STAGEID);
                            if (curTargetPort != null)
                            {
                                //且回loading port 原cst原slot
                                if (curTargetPort.File.Type == ePortType.LoadingPort &&
                                    curTargetPort.File.CassetteID == curBcsJob.FromCstID &&
                                    intTargetSlotNo == jobfromslot)
                                {
                                    //改寫newStepNo為第一步
                                    newStepNo = curBcsJob.RobotWIP.RobotRouteStepList.OrderBy(s => s.Value.Data.STEPID).First().Value.Data.STEPID;

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) do buffering ,rewrite NextStep from({4}) to({5}) !",
                                                   curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.NextStepNo, newStepNo.ToString());

                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                else
                                {
                                    //update job rcsbufferingflag=0	
                                    lock (curBcsJob)
                                    {
                                        curBcsJob.CfSpecial.RCSBufferingFlag = "0";
                                    }
                                    ObjectManager.JobManager.EnqueueSave(curBcsJob);
                                }
                            }
                        }
                    }
                    #endregion

                    strTmp = string.Format("curStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), newStepNo.ToString());

                    //Update CurStepNo
                    curBcsJob.RobotWIP.CurStepNo = newStepNo;

                    #region [ Get New Current Step Entity to Get NextStep ]

                    RobotRouteStep newCurRouteStep = null;

                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(newStepNo) != false)
                    {

                        newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];
                    }

                    //找不到 CurStep Route 記Log 且將NextStep改為0(異常)
                    if (newCurRouteStep == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4}) Entity!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                newStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.NextStepNo = 0;
                    }
                    else
                    {
                        curBcsJob.RobotWIP.NextStepNo = newCurRouteStep.Data.NEXTSTEPID;

                    }


                    #endregion

                    strTmp = strTmp + string.Format("NextStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString());

                    #endregion

                    #region [ Update RobotJob Status PROCESS ]

                    if (newStepNo == 1)
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.WAIT_PROC);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.WAIT_PROC;

                    }
                    //20151014 Modify 大於65535則算Complete
                    //else if (newStepNo > curBcsJob.RobotWIP.RobotRouteStepList.Count)
                    else if (newStepNo >= eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO)
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.COMPLETE);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.COMPLETE;

                    }
                    else
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to ({1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.INPROCESS);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.INPROCESS;
                    }

                    #endregion

                    #region [ Update CurLocation Info ]

                    strTmp = strTmp + string.Format("CurLocation_StageType form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageType, curTargetStage.Data.STAGETYPE);

                    curBcsJob.RobotWIP.CurLocation_StageType = curTargetStage.Data.STAGETYPE;

                    strTmp = strTmp + string.Format("CurLocation_StageID form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageID, curTargetStage.Data.STAGEID);

                    curBcsJob.RobotWIP.CurLocation_StageID = curTargetStage.Data.STAGEID;

                    strTmp = strTmp + string.Format("CurLocation_SlotNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(), intTargetSlotNo.ToString());

                    curBcsJob.RobotWIP.CurLocation_SlotNo = intTargetSlotNo;



                    curBcsJob.RobotWIP.FetchOutDataTime = DateTime.MinValue; //如果回插到STAGE(PORT/EQP)时, 需要清空, 待出片时, 再更新! 20151116-dd

                    #endregion

                    if (curTargetStage.Data.STAGETYPE == eRobotStageType.PORT)
                    {
                        if (curBcsJob.RobotWIP.PutReadyFlag == 1)
                        {
                            curBcsJob.RobotWIP.PutReadyFlag = 0;
                            curBcsJob.RobotWIP.PutReady_StageID = string.Empty;
                        }
                        //20160624
                        if (curRobot.OnArmPutReadyFlag == 1)
                        {
                            curRobot.OnArmPutReadyFlag = 0;
                            curRobot.OnArmPutReady_StageID = string.Empty;
                        }

                        //if (curBcsJob.RobotWIP.PreFetchFlag > 0 && curRobot.File.LastPreFetchReturnDateTime == DateTime.MinValue) curRobot.File.LastPreFetchReturnDateTime = DateTime.Now; 
                        if (curBcsJob.RobotWIP.PreFetchFlag > 0 && curBcsJob.RobotWIP.LastPreFetchFlag != curBcsJob.RobotWIP.PreFetchFlag)
                        {
                            curBcsJob.RobotWIP.LastPreFetchFlag = curBcsJob.RobotWIP.PreFetchFlag;
                            curRobot.File.LastPreFetchReturnDateTime = DateTime.Now;

                            if (curBcsJob.RobotWIP.RTCReworkFlag) curBcsJob.RobortRTCFlag = true; //更新RobotRTCFlag給BCS判断! //20160108-001-dd.有预取并且RTC才考虑
                        }
                        else
                        {
                            //20160525
                            //if (curBcsJob.RobortRTCFlag)
                            if (curBcsJob.RobortRTCFlag && !curBcsJob.RobotWIP.RTCReworkFlag)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job[{2}]'s RobotRTCFlag[ON] without Pre-Fetch and RTCReworkFlag[{3}], set OFF.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME,
                                    string.Format("CST={0}, Slot={1}", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString()), (curBcsJob.RobotWIP.RTCReworkFlag ? "ON" : "OFF"));
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                curBcsJob.RobortRTCFlag = false; //20160108-001-dd::没预取, 但有ON, 记LOG, 并设为OFF!!
                            }
                        }

                    }
                    else
                    {
                        if (curBcsJob.RobotWIP.PreFetchFlag > 0 && curBcsJob.RobotWIP.LastPreFetchFlag != curBcsJob.RobotWIP.PreFetchFlag) curBcsJob.RobotWIP.LastPreFetchFlag = curBcsJob.RobotWIP.PreFetchFlag;

                        //20160525
                        #region Update FetchOut DateTime Info
                        DateTime _now = DateTime.Now;
                        strTmp = strTmp + string.Format("FetchOut from ({0}) on ({1}).", curBcsJob.RobotWIP.CurLocation_StageID, _now.ToString("yyyy-MM-dd HH:mm:ss"));
                        curBcsJob.RobotWIP.StoreDateTime = _now;
                        #endregion

                        //20160525
                        if (curBcsJob.RobotWIP.EQPRTCFlag)
                            curBcsJob.RobotWIP.EQPRTCFlag = false;
                    }

                    #region [ 20151107 add Update Job Last Input TrackingData By Step RouteStep Rule ]

                    if (curRouteStep.Data.ROBOTRULE == eRobotRouteStepRule.SELECT)
                    {
                        string tmpTrackData = GetRouteStep_Select_InPutTrackingData(curTargetStage, curRouteStep.Data.INPUTTRACKDATA);

                        strTmp = strTmp + string.Format("LastInPutTrackingData form ({0}) to ({1}).", curBcsJob.RobotWIP.LastInPutTrackingData, tmpTrackData);

                        //因會出現多組(EX:11,12  00111100000000000....)所以直接抓Stage設定而不是取得RouteStep設定的TrackingData
                        curBcsJob.RobotWIP.LastInPutTrackingData = tmpTrackData;

                    }
                    else
                    {
                        strTmp = strTmp + string.Format("LastInPutTrackingData form ({0}) to ({1}).", curBcsJob.RobotWIP.LastInPutTrackingData, curRouteStep.Data.INPUTTRACKDATA);

                        //非Select只有單一設定,直接取得RouteStep設定的TrackingData
                        curBcsJob.RobotWIP.LastInPutTrackingData = curRouteStep.Data.INPUTTRACKDATA;

                    }

                    #endregion

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP {4}",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                                    strTmp);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    #region [ 20151209 add For Set FroceRetrunCSTWithoutLDRQ status is NotCheck ]

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP FroceRetrunCSTWithoutLDRQ From({4}) to ({5})",
                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status, eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_NOTCHECK);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_NOTCHECK;

                    #endregion

                    #region [20151211 add For CF MQC TTP Update Flag = Tracking Data to Sub Chamber]
                    ///即使未經Sub Chamber機台也Turn ON也沒關係，因為Glass Flow也不會有此機台. 
                    ///此bit只在Aging Enable/Disable 時要不要多加Sub Chamber的Flow.
                    ///但如果有經過Sub Chamber，就一定要設定，不然會無法判斷
                    curBcsJob.RobotWIP.CF_MQCTTP_SubChamberProcessedFlag  = true;
                    #endregion
                }

                //Save File
                ObjectManager.JobManager.EnqueueSave(curBcsJob);

                #endregion

                #region [ Update DRY Last Enter Stage ID ]
                if (Workbench.LineType.ToString().Contains("DRY_")) Invoke("RobotSpecialService", "Update_DRY_LastEnterStageID_For1Cmd_1Arm_1Job", new object[] { robotConText });
                #endregion

                if (StaticContext.ContainsKey(eRobotContextParameter.JobSendToSameEQ_RobotParam) &&
                    StaticContext[eRobotContextParameter.JobSendToSameEQ_RobotParam] is JobSendToSameEQ_RobotParam)
                {
                    JobSendToSameEQ_RobotParam param = (JobSendToSameEQ_RobotParam)StaticContext[eRobotContextParameter.JobSendToSameEQ_RobotParam];
                    if (param.SameEQFlag &&
                        oldStep.Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT &&
                        oldStep.Data.ROBOTRULE == eRobotRouteStepRule.SELECT &&
                        curTargetStage.Data.STAGETYPE == eRobotStageType.EQUIPMENT)
                    {
                        string node_no = string.Empty;
                        if (curRobot.File.CheckMap(curBcsJob.CassetteSequenceNo, oldStepNo, out node_no))
                        {
                            //do nothing
                            if (node_no != curTargetStage.Data.NODENO)
                            {
                                //曾經紀錄同一CST的Job在oldStepNo時進入node_no, 但是Robot Arm Unload時卻把curBcsJob放到不同Node的Stage裡
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Put into Node({4}) but Same EQ Map record Node({5})",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curTargetStage.Data.NODENO, node_no);
                                Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                        }
                        else
                        {
                            curRobot.File.AddToMap(curBcsJob.CassetteSequenceNo, oldStepNo, curTargetStage.Data.NODENO);
                            ObjectManager.RobotManager.EnqueueSave(curRobot.File);
                        }
                    }
                }

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }

        [UniAuto.UniBCS.OpiSpec.Help("PR0007")]
        public bool ProcResult_JobMoveToRobotArm_1Arm1Job_forSorterMode(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                string strTmp = string.Empty;

                #region [ Get curBcsJob Entity ]

                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

                //找不到 BcsJob 回NG
                if (curBcsJob == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get BcsJob!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region SorterMode_RobotParam param

                SorterMode_RobotParam param = null;

                if (StaticContext.ContainsKey(eRobotContextParameter.SorterMode_RobotParam) &&
                    StaticContext[eRobotContextParameter.SorterMode_RobotParam] is SorterMode_RobotParam)
                {
                    param = (SorterMode_RobotParam)StaticContext[eRobotContextParameter.SorterMode_RobotParam];
                }
                if (param == null)
                {
                    //#region[DebugLog]

                    //if (IsShowDetialLog == true)
                    //{
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] StaticContext has no FCSRT_RobotParam",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    //}

                    //#endregion

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }

                #endregion

                param.EnableCallCassetteService = true;//Port出片後重新將Flag on起來. 下次若找不到Unloader就要呼叫CassetteService
                param.LastGrade = curBcsJob.JobGrade;//Port出片, 記錄JobGrade, 之後相同Grade的Job要優先處理

                if (ParameterManager.Parameters.ContainsKey("ROBOT_ENABLE_CALL_CASSETTE_SERVICE"))
                {
                    ParameterManager.Parameters["ROBOT_ENABLE_CALL_CASSETTE_SERVICE"].Value = param.EnableCallCassetteService.ToString();
                }

                //20160302 add log
                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] StaticContext FCSRT_RobotParam Update EnableCallCassetteService=(True), LastGrade=({2})",
                                       "L1", MethodBase.GetCurrentMethod().Name, param.LastGrade);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }

        /// <summary> 從卡匣抽片後的更新資料 MQC TTP Current Route 記錄 
        /// 一樣會做ProcResult_JobMoveToRobotArm_1Arm1Job_forCSTFetch的事
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("PR0008")]
        public bool ProcResult_JobMoveToRobotArm_1Arm1Job_forUpdateRoute(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                string strTmp = string.Empty;
                int oldStepNo = 0;
                int newStepNo = 0;

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBcsJob Entity ]

                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

                //找不到 BcsJob 回NG
                if (curBcsJob == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get BcsJob!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBCSJob Load ArmNo ]

                string curLoadArmNo = (string)robotConText[eRobotContextParameter.LoadJobArmNo_For_1Arm_1Job];

                //找不到 CurLoadArmNo 回NG
                if (curLoadArmNo == null || curLoadArmNo == string.Empty)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Load ArmNo!",
                                            "L1", MethodBase.GetCurrentMethod().Name);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] can not Get Load ArmNo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_CurLoadArmNo_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                int intArmNo = 0;

                int.TryParse(curLoadArmNo, out intArmNo);

                #endregion

                #region [ Check Update RobotWIP Condition ]

                #region [ Check Job Location must At Not Robot Home Stage ]

                if (curBcsJob.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStageNo({4}) at RobotArm and can not Update CurRouteStepNo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] Job({1}_{2}) CurStageNo({3}) at RobotArm and can not Update CurRouteStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurLocation_StageID);

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Check_curBcsJob_Loction_Not_RobotArm_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 由Job中透過目前的CurStep取得目前RouteInfo ]

                RobotRouteStep curRouteStep = null;

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == true)
                {
                    curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];
                }

                if (curRouteStep == null)
                {

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurRouteStepNo({4}) can not get RouteInfo!",
                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    errMsg = string.Format("[{0}] Job({1}_{2}) CurRouteStepNo({3}) can not get RouteInfo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_Get_RouteInfo_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ 確認目前RouteStepNo中的動作是否為GET ]

                switch (curRouteStep.Data.ROBOTACTION)
                {
                    case eRobot_DB_CommandAction.ACTION_GET:

                        //只有CurRoute Step Action為Get時才能在Job Move To Arm(Arm Load Job)時更新StepNo
                        break;

                    default:


                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) CassetteSequenceNo({2}) JobSequenceNo({3}) CurStepNo({4}) ACTION({5}) is not ({6}) and can not Update CurStepNo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_GET);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        errMsg = string.Format("[{0}] Job({1}_{2}) CurRouteStepNo({3}) ACTION({4}) is not ({5}) and can not Update CurStepNo!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION, eRobot_DB_CommandAction.ACTION_GET);

                        robotConText.SetReturnCode(eProcResultAction_ReturnCode.NG_JOB_CurStep_Action_IsNot_GET);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                }

                #endregion

                #endregion

                #region [ Update RobotWIP ]

                lock (curBcsJob)
                {

                    #region [ Update StepNo ]

                    //[ Wait_For_Proc_00015 ][ 針對Step Rule 要如何實作 ]
                    //[ Wait_For_Proc_00016 ][ 針對Recipe by Pass後Step Rule 要如何實作 ]


                    oldStepNo = curBcsJob.RobotWIP.CurStepNo;
                    //20151014 Modity NextStep由WIP來取得
                    newStepNo = curBcsJob.RobotWIP.NextStepNo; // curBcsJob.RobotWIP.CurStepNo + 1;

                    strTmp = string.Format("curStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), newStepNo.ToString());

                    //Update CurStepNo and NextStep
                    curBcsJob.RobotWIP.CurStepNo = newStepNo;

                    #region [ Get New Current Step Entity to Get NextStep ]

                    //RobotRouteStep newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];

                    RobotRouteStep newCurRouteStep = null;

                    if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(newStepNo) != false)
                    {

                        newCurRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[newStepNo];
                    }

                    //找不到 CurStep Route 記Log 且將NextStep改為0(異常)
                    if (newCurRouteStep == null)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Get curRouteStep({4}) Entity!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                newStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.NextStepNo = 0;
                    }
                    else
                    {
                        curBcsJob.RobotWIP.NextStepNo = newCurRouteStep.Data.NEXTSTEPID;

                    }


                    #endregion

                    strTmp = strTmp + string.Format("NextStepNo form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString());

                    #endregion

                    #region [ Update RobotJob Status PROCESS ]

                    if (newStepNo == 1)
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.WAIT_PROC);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.WAIT_PROC;

                    }
                    //20151014 Modify 大於65535則算Complete
                    //else if (newStepNo > curBcsJob.RobotWIP.RobotRouteStepList.Count) //最後的Step是準備放到Port 所以必須要是>而不是>=
                    else if (newStepNo >= eRobotCommonConst.ROBOT_ROUTE_COMPLETE_STEPNO)
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to {1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.COMPLETE);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.COMPLETE;

                    }
                    else
                    {
                        strTmp = strTmp + string.Format("CurJobStatus form ({0}) to ({1}), ", curBcsJob.RobotWIP.RouteProcessStatus, eRobot_RouteProcessStatus.INPROCESS);

                        curBcsJob.RobotWIP.RouteProcessStatus = eRobot_RouteProcessStatus.INPROCESS;
                    }

                    #endregion

                    #region [ Update CurLocation Info ]

                    strTmp = strTmp + string.Format("CurLocation_StageType form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageType, eRobotStageType.ROBOTARM);

                    curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.ROBOTARM;

                    strTmp = strTmp + string.Format("CurLocation_StageID form ({0}) to ({1}), ", curBcsJob.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

                    string _preLocation_StageID = curBcsJob.RobotWIP.CurLocation_StageID;
                    curBcsJob.RobotWIP.CurLocation_StageID = eRobotCommonConst.ROBOT_HOME_STAGEID;

                    strTmp = strTmp + string.Format("CurLocation_SlotNo form ({0}) to ({1}).", curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(), intArmNo.ToString());

                    curBcsJob.RobotWIP.CurLocation_SlotNo = intArmNo;

                    #endregion

                    RobotStage _stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(_preLocation_StageID);

                    #region Update PreFetch Info
                    if (_stage != null && _stage.Data.STAGETYPE == eRobotStageType.PORT) if (_stage.Data.PREFETCHFLAG.ToString().ToUpper() == "Y") curBcsJob.RobotWIP.PreFetchFlag++;
                    #endregion

                    #region Update FetchOut DateTime Info
                    //if (_stage != null && _stage.Data.STAGETYPE == eRobotStageType.PORT && curBcsJob.RobotWIP.FetchOutDataTime != DateTime.MinValue)
                    //{
                    DateTime _now = DateTime.Now;
                    strTmp = strTmp + string.Format("FetchOut from ({0}) on ({1}).", _preLocation_StageID, _now.ToString("yyyy-MM-dd HH:mm:ss"));
                    curBcsJob.RobotWIP.FetchOutDataTime = _now;
                    //}
                    #endregion

                    curBcsJob.RobotWIP.PutReadyFlag = 0;
                    curBcsJob.RobotWIP.PutReady_StageID = string.Empty;
                    if (curBcsJob.RobotWIP.PreFetchFlag > 0) //有启动预取!
                    {
                        if (curRobot.CurRealTimeSetCommandInfo != null) //命令还没清空
                        {
                            if (curRobot.CurRealTimeSetCommandInfo.Cmd02_Command == eRobot_Trx_CommandAction.ACTION_PUTREADY)
                            {
                                curBcsJob.RobotWIP.PutReadyFlag = 1; //如果命令是putready, 则更新put ready flag!
                                curBcsJob.RobotWIP.PutReady_StageID = curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition.ToString(); //记录基本预设要去的stage!
                            }
                        }
                    }

                    //20150525 加入STAGETYPE == PORT判斷
                    if (curBcsJob.RobotWIP.RTCReworkFlag && _stage.Data.STAGETYPE == eRobotStageType.PORT)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job[{2}]'s RTCReworkFlag[ON], set OFF.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, string.Format("CST={0}, Slot={1}", curBcsJob.CassetteSequenceNo.ToString(), curBcsJob.JobSequenceNo.ToString()));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        curBcsJob.RobotWIP.RTCReworkFlag = false; //20160108-001-dd::回插再出片, 需要改为OFF并记LOG!!
                    }
                    //20150525 加入STAGETYPE == PORT判斷
                    if (curBcsJob.RobortRTCFlag && _stage.Data.STAGETYPE == eRobotStageType.PORT) curBcsJob.RobortRTCFlag = false; //抽出来要改为OFF

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP {4}",
                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, strTmp);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    #region [ 20151209 add For Set FroceRetrunCSTWithoutLDRQ status is Ready ]

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Update RobotJobWIP FroceRetrunCSTWithoutLDRQ From({4}) to ({5})",
                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                        curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status, eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_READY);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    curBcsJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_READY;

                    #endregion

                    #region 20160104 Add For MQC TTP Update Current  Route Info
                    curRobot.Cur_CFMQCTTP_Flow_Route = curBcsJob.RobotWIP.CurRouteID;
                    #endregion

                }

                //Save File
                ObjectManager.JobManager.EnqueueSave(curBcsJob);

                #endregion

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }

    }
}
