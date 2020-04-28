using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.OpiSpec;

namespace UniAuto.UniRCS.CSOT.t3.Service
{

    public partial class JobFilterService : AbstractRobotService
	{

//Filter Funckey = "FL" + XXXX(序列號)

		/// <summary>
		/// <br>CFREP/CFMAC/CFAOI through mode</br>
		/// <br>filter out job if job judge is OK in through mode,job can't be fetched</br>
		/// </summary>
		/// <param name="robotConText">robot context object</param>
		/// <returns>false=Filter out</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0025")]
		public bool Filter_PortFetchOutThroughJobJudge(IRobotContext robotConText) 
		{
			
			string strlog = string.Empty;
			string errMsg = string.Empty;
            string errCode = string.Empty;

			try {

				#region [ Get curRobot Entity ]

				Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

				//找不到 Robot 回NG
				if (curRobot == null) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
												"L1", MethodBase.GetCurrentMethod().Name);

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] can not Get Robot!",
											MethodBase.GetCurrentMethod().Name);

					robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion

				#region [ Get Robot 所屬Line Entity ]

				Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

				if (robotLine == null) {

					#region  [DebugLog]

					if (IsShowDetialLog == true) {
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

				
				#region [Filter判斷條件:through mode Job is OK Judge]	
				if (robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.THROUGH_MODE) 
				{
					if (curBcsJob.RobotWIP.CurSendOutJobJudge == "1") {
						#region[DebugLog]

						if (IsShowDetialLog == true) {
							strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) IndexOperMode({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) curStepNo({6}) jobjudge is OK,can not Fetch Out!",
													curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, robotLine.File.IndexOperMode.ToString(), curBcsJob.CassetteSequenceNo,
                                                    curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

							Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
						}

						#endregion

                        errMsg = string.Format("[{0}] IndexOperMode({1}) Job({2}_{3}) curRouteID({4}) curStepNo({5}) jobjudge is OK,can not Fetch Out!",
												MethodBase.GetCurrentMethod().Name, robotLine.File.IndexOperMode.ToString(), curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ThroughMode);
						robotConText.SetReturnMessage(errMsg);

                        errCode = eJobFilter_ReturnCode.NG_ThroughMode;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

						return false;
					}
				}
				#endregion

				robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
				robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

				return true;
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

				robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);

				return false;
			}

		}


		/// <summary>
		/// <br>CFREW line</br>
		/// <br>filter out job if indexer ReworkForceToUnloaderCST ON ,job can't be fetched</br>
		/// </summary>
		/// <param name="robotConText">robot context object</param>
		/// <returns>false=Filter out</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("FL0026")]
		public bool Filter_PortFetchOutReworkForceToUnloaderCST(IRobotContext robotConText) 
		{
			string strlog = string.Empty;
			string errMsg = string.Empty;
            string errCode = string.Empty;

			try {

				#region [ Get curRobot Entity ]

				Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

				//找不到 Robot 回NG
				if (curRobot == null) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
												"L1", MethodBase.GetCurrentMethod().Name);

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] can not Get Robot!",
											MethodBase.GetCurrentMethod().Name);

					robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

				#region [Filter判斷條件:indexer ReworkForceToUnloaderCST ON]
				Equipment indexer = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
				if (indexer.File.ReworkForceToUnloaderCST == eBitResult.ON) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job({2}_{3}) curRouteID({4}) curStepNo({5}) Indexer ReworkForceToUnloaderCST ON ,can not Fetch Out!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());
													

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) Indexer ReworkForceToUnloaderCST ON ,can not Fetch Out!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());
												

					robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_PortFetchOutNotFroceCleanOut_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_PortFetchOutNotFroceCleanOut_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

					return false;					
				}
				#endregion

				robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
				robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

				return true;
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

				robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);

				return false;
			}
		}

        [UniAuto.UniBCS.OpiSpec.Help("FL0029")]
        public bool Filter_PortFetchOutMappingGrade(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;
            List<RobotStage> curFilterCanUseStageList = null;
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion
                curBcsJob.RobotWIP.SorterMode_GradeMatch = false;// false表示Grade mismatch, Unloader可能要退Cassette
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get Defind 1st NormalRobotCommand ]

                DefineNormalRobotCmd cur1stRobotCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.Define_1stNormalRobotCommandInfo];

                //找不到 1st defineNormalRobotCmd 回NG
                if (cur1stRobotCmd == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get defineNormalRobotCmd!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get defineNormalRobotCmd!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Defind 2nd NormalRobotCommand ]

                DefineNormalRobotCmd cur2ndRobotCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.Define_2ndNormalRobotCommandInfo];

                //找不到 2nd defineNormalRobotCmd 回NG
                if (cur2ndRobotCmd == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get 2nd defineNormalRobotCmd!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get 2nd defineNormalRobotCmd!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_2nd_DefineNormalRobotCommandInfo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                string tmpStageID = string.Empty;
                string tmpStepAction = string.Empty;
                int tmpStepNo = 0;
                string funcName = string.Empty;

                #region [ check Step by is2ndCmdFlag ]

                if (is2ndCmdFlag == false)
                {

                    tmpStepNo = curBcsJob.RobotWIP.CurStepNo;
                    funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                }
                else
                {
                    //20151014 Modity NextStep由WIP來取得
                    tmpStepNo = curBcsJob.RobotWIP.NextStepNo;// curBcsJob.RobotWIP.CurStepNo + 1;
                    funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;
                }

                #endregion

                #region [ Get tmp Step Entity ]

                RobotRouteStep tmpRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[tmpStepNo];

                //找不到 CurStep Route 回NG
                if (tmpRouteStep == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Get curRouteStep({5})!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, tmpStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job CassetteSequenceNo({1}) JobSequenceNo({2}) can not Get curRouteStep({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo, tmpStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Get_RouteStep_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Set Parameter by is2ndCmdFlag ]

                if (is2ndCmdFlag == false)
                {
                    tmpStageID = curBcsJob.RobotWIP.CurLocation_StageID;

                }
                else
                {

                    #region [ by 1st Cmd Define Job Location(curStageID) and ArmInfo(robotArmInfo[2]) ]

                    //SPEC定義[ Wait_Proc_00028 ] 1Arm 2Job要額外處理
                    //0: None      //1: Put          //2: Get
                    //4: Exchange  //8: Put Ready    //16: Get Ready       //32: Get/Put                 
                    switch (cur1stRobotCmd.Cmd01_Command)
                    {
                    case 1:  //PUT
                    case 4:  //Exchange
                    case 32: //Get/Put

                        //Local Stage is Stage
                        tmpStageID = cur1stRobotCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0');
                        break;

                    case 2:  //Get
                    case 8:  //Put Ready
                    case 16: //Get Ready

                        //Local Stage is Stage
                        tmpStageID = eRobotCommonConst.ROBOT_HOME_STAGEID;
                        break;

                    default:

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) ({3}) 1st defineNormalRobotCmd Action({4}) is out of Range!",
                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not Get 1st defineNormalRobotCmd Action({1}) is out of Range!",
                                                MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString());

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
                        robotConText.SetReturnMessage(errMsg);

                        errCode = eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                        return false;

                    }

                    #endregion

                }

                //DB定義 'PUT' / 'GET' / 'PUTREADY' / 'GETREADY'
                tmpStepAction = tmpRouteStep.Data.ROBOTACTION.ToString();

                #endregion

                #region [ Get LDRQ Stage List ]

                curFilterCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                if (curFilterCanUseStageList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStageID({4}) StepNo({5}) StepCanUseStageList is null",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                tmpStageID, tmpStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStageID({3}) StepNo({4}) StepCanUseStageList is null",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            tmpStageID, tmpStepNo.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_Get_LDRQStageList_Is_Fail;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;

                }

                #endregion

                #region Get L2 Equipment
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
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

                List<Port> ports = ObjectManager.PortManager.GetPortsByLine(curRobot.Data.LINEID);
                bool unloader_ready = false;
                List<PortStage> mapping_ports = SorterMode_JobGradeUnloaderGrade(eqp, ports, curFilterCanUseStageList, curBcsJob, ref unloader_ready);

                if (mapping_ports.Count <= 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) There is no Port mapping with JobGrade[{6}] JobProductType[{7}] EqpCheckMode[{8}],please check 1.EQP.File.ProductTypeCheckMode is Disable,job.JobGrade = port.File.MappingGrade 2.EQP.File.ProductTypeCheckMode is Enable,port.File.ProductType is 0 or job.ProductType == port.File.ProductType,job.JobGrade = port.File.MappingGrade",
                                                "L1", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.JobGrade, curBcsJob.ProductType, (int)eqp.File.ProductTypeCheckMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) There is no Port mapping with JobGrade[{5}] JobProductType[{6}] EqpCheckMode[{7}],please check 1.EQP.File.ProductTypeCheckMode is Disable,job.JobGrade = port.File.MappingGrade 2.EQP.File.ProductTypeCheckMode is Enable,port.File.ProductType is 0 or job.ProductType == port.File.ProductType,job.JobGrade = port.File.MappingGrade",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.JobGrade, curBcsJob.ProductType, (int)eqp.File.ProductTypeCheckMode);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_No_Match_Port_Grade);
                    robotConText.SetReturnMessage(errMsg);

                    //return false;
                    // return true, 跑其他 Filter, 確認 Job 是否可出片
                    // 可出片但 Grade mismatch 則需要退 Cassette
                    return true;
                }

                #region[DebugLog]
                if (IsShowDetialLog == true)
                {
                    StringBuilder port_str = new StringBuilder();
                    foreach (PortStage port_stage in mapping_ports) port_str.AppendFormat("{0},", port_stage.Port.Data.PORTNO);
                    if (port_str.Length > 0) port_str.Remove(port_str.Length - 1, 1);

                    StringBuilder grade_str = new StringBuilder();
                    foreach (PortStage port_stage in mapping_ports) grade_str.AppendFormat("{0},", port_stage.Port.File.MappingGrade);
                    if (grade_str.Length > 0) grade_str.Remove(grade_str.Length - 1, 1);

                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Port[{2}] Mapping Grade[{3}] match with Job Grade[{4}]",
                                            "L1", MethodBase.GetCurrentMethod().Name, port_str, grade_str, curBcsJob.JobGrade);

                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion
                curBcsJob.RobotWIP.SorterMode_GradeMatch = true;// true表示Grade match, Unloader可能不用退Cassette
                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("FL0032")]
        public bool Filter_PortFetchOutFixTargetStage(IRobotContext robotConText)
        {
            // Filter_PortFetchOutFixTargetStage
            // 若 TimeoutState == CLEAR, 則計算時間差檢查是否Timeout, 若尚未Timeout則繼續等待FixStage的ReceiveAble; 若已Timeout則不等待
            // 若 TimeoutState == WAIT_FOR_TIMEOUT, 表示已經檢查過時間差且結果是尚未Timeout, 因此繼續等待FixStage的ReceiveAble
            // 若 TimeoutState == TIMEOUT, 表示已經檢查過時間差且結果是已經Timeout, 因此不等待FixStage的ReceiveAble
            string strlog = string.Empty;
            string errMsg = string.Empty;
            RobotStage fix_stage = null;
            string errCode = string.Empty;
            try
            {
                #region FixTargetStage_RobotParam fix_param
                FixTargetStage_RobotParam fix_param = null;
                if (!StaticContext.ContainsKey(eRobotContextParameter.FixTargetStage_RobotParam))
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] StaticContext is not Contains FixTargetStage_RobotParam",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] StaticContext is not Contains FixTargetStage_RobotParam",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                if (!(StaticContext[eRobotContextParameter.FixTargetStage_RobotParam] is FixTargetStage_RobotParam))
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] StaticContext is not Contains FixTargetStage_RobotParam",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] StaticContext is not Contains FixTargetStage_RobotParam",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                fix_param = (FixTargetStage_RobotParam)StaticContext[eRobotContextParameter.FixTargetStage_RobotParam];
                #endregion

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                if (string.IsNullOrEmpty(fix_param.STAGEID))
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "FixTargetStage is Empty");
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }

                if (fix_param.TimeoutState == FixTargetStage_RobotParam.TIMEOUT_STATE.CLEAR)
                {
                    if ((DateTime.Now - fix_param.FixDateTime).TotalMilliseconds >= fix_param.TimeoutMS)
                        fix_param.TimeoutState = FixTargetStage_RobotParam.TIMEOUT_STATE.TIMEOUT;
                    else
                    {
                        List<RobotStage> curRobotStages = ObjectManager.RobotStageManager.GetRobotStages(curRobot.Data.ROBOTNAME);
                        fix_stage = curRobotStages.Find(s => s.Data.STAGEID == fix_param.STAGEID);
                        if (fix_stage != null && fix_stage.curLDRQ_EmptySlotList.Count > 0)
                            fix_param.TimeoutState = FixTargetStage_RobotParam.TIMEOUT_STATE.RECEIVE_ABLE;
                        else
                            fix_param.TimeoutState = FixTargetStage_RobotParam.TIMEOUT_STATE.WAIT_FOR_TIMEOUT;
                    }
                }

                if (fix_param.TimeoutState == FixTargetStage_RobotParam.TIMEOUT_STATE.TIMEOUT)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("FixTargetStage[{0}] is Timeout", fix_param.STAGEID);
                        Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }
                else if (fix_param.TimeoutState == FixTargetStage_RobotParam.TIMEOUT_STATE.RECEIVE_ABLE)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("FixTargetStage[{0}] curLDRQ_EmptySlotList[{1}]", fix_param.STAGEID, fix_stage.curLDRQ_EmptySlotList.Count);
                        Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }
                else if (fix_param.TimeoutState == FixTargetStage_RobotParam.TIMEOUT_STATE.WAIT_FOR_TIMEOUT)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) FixTargetStage[{6}] is not Timeout, Wait for Timeout.",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), fix_param.STAGEID);

                        Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) FixTargetStage[{5}] is not Timeout, Wait for Timeout.",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), fix_param.STAGEID);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_FixTargetNotTimeOut);
                    robotConText.SetReturnMessage(errMsg);

                    //errCode = eJobFilter_ReturnCode.NG_FixTargetNotTimeOut;//add for BMS Error Monitor
                    //if (!curRobot.CheckErrorList.ContainsKey(errCode))
                    //    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                #region[DebugLog]
                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) FixTargetStage[{6}] TimeoutState[{7}] Error",
                                            "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), fix_param.STAGEID, fix_param.TimeoutState);

                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion

                errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) FixTargetStage[{5}] TimeoutState[{6}] Error",
                                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), fix_param.STAGEID, fix_param.TimeoutState);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                robotConText.SetReturnMessage(errMsg);

                errCode = eJobFilter_ReturnCode.NG_curRobot_Is_Null;//add for BMS Error Monitor
                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("FL0035")]
        public bool Filter_PortFetchOutThroughMode_SamplingFlagAndJobJudge(IRobotContext robotConText)
        {
            //Through Mode：
            //Sampling Flag ON時必出片到EQ；OFF時只出非OK的片放到Mix Unloader
            //但在 REP Line，OK的片不用修補，所以Sampling Flag ON時只出非OK的片到EQ，OFF時也出非OK的片放到Mix Unloader
            //因此 Filter_ThroughMode_SamplingFlagAndJobJudge 不適用於 REP Line
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                if (robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.THROUGH_MODE &&
                    curBcsJob.SamplingSlotFlag != "1" && curBcsJob.RobotWIP.CurSendOutJobJudge == "1")
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] IndexerMode[{2}] Job[{3}_{4}] curRouteID({5}) curStepNo({6}) SamplingFlag[{7}] JobJudge[{8}](Job Judge=OK,but Sampling Slot Flag is OFF)",
                                                "L2", MethodBase.GetCurrentMethod().Name, robotLine.File.IndexOperMode.ToString(), curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.SamplingSlotFlag, curBcsJob.RobotWIP.CurSendOutJobJudge);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] IndexerMode[{1}] Job[{2}_{3}] curRouteID({4}) curStepNo({5}) SamplingFlag[{6}] But JobJudge[{7}] Can Not Fetch Out To UnloadingPort",
                                            MethodBase.GetCurrentMethod().Name, robotLine.File.IndexOperMode.ToString(), curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.SamplingSlotFlag, "OK");

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_ThroughMode);
                    robotConText.SetReturnMessage(errMsg);
                    errCode = eJobFilter_ReturnCode.NG_ThroughMode;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                    return false;
                }

                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("FL0036")]
        public bool Filter_JobJudgeEQPFlagIsMatch(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion
                #region [Check JobJudge與EQPFlag是否Match]
                //JobJudge=5(RP),JobJudge=6(IR),EQPFlag=1(Ink Repair Glass),EQPFlag=2(Repair Glass)
                //JobJudge為RP且EQP Flag要為2才能出片,JobJudge為IR且EQP Flag要為1才能出片
                //if (curBcsJob.RobotWIP.CurSendOutJobJudge == "5" && curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG.Substring(1, 1) == "1")
                //{
                //    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                //    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                //    return true;
                //}
                //else if (curBcsJob.RobotWIP.CurSendOutJobJudge == "6" && curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG.Substring(0, 1) == "1")
                //{
                //    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                //    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                //    return true;
                //}
                switch (curBcsJob.RobotWIP.CurSendOutJobJudge)
                {
                    case "0":
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "7":
                    case "8":
                        robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                        return true;
                    case "5":
                        if (curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG.Substring(1, 1) == "1")
                        {
                            robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                            robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                            return true;
                        }
                        break;
                    case "6":
                        if (curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG.Substring(0, 1) == "1")
                        {
                            robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                            robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                            return true;
                        }
                        break;
                }
                #endregion
                errMsg = string.Format("[{0}] Job[{1}_{2}] curRouteID({3}) curStepNo({4}) [JobJudge={5}] and [EQPFlag={6}] is not Match.(JobJudge:5.RP,6.IR, EQPFlag:1.Repair mode,2.Ink Repair mode)",
                        MethodBase.GetCurrentMethod().Name,curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString()
                        , curBcsJob.RobotWIP.CurSendOutJobJudge, curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG);
                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_JobJudgeAndEQPFlagIsNotMatch);
                robotConText.SetReturnMessage(errMsg);

                errCode = eJobFilter_ReturnCode.NG_JobJudgeAndEQPFlagIsNotMatch;//add for BMS Error Monitor
                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }
        [UniAuto.UniBCS.OpiSpec.Help("FL0037")]
        public bool Filter_PortFetchOutNormal_MQCJobJudge(IRobotContext robotConText)
        {

            string strlog = string.Empty;
            string errMsg = string.Empty;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion


                #region [Filter判斷條件:through mode Job is OK Judge]
                if (robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE || robotLine.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE)
                {
                    if (curBcsJob.RobotWIP.CurSendOutJobJudge == "1")
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) IndexOperMode({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) curStepNo({6}) jobjudge is OK,can not Fetch Out!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, robotLine.File.IndexOperMode.ToString(), curBcsJob.CassetteSequenceNo,
                                                    curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] IndexOperMode({1}) Job({2}_{3}) curRouteID({4}) curStepNo({5}) jobjudge is OK,can not Fetch Out!",
                                                MethodBase.GetCurrentMethod().Name, robotLine.File.IndexOperMode.ToString(), curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Normal_MQCMode);
                        robotConText.SetReturnMessage(errMsg);

                        //errCode = eJobFilter_ReturnCode.NG_Normal_MQCMode;//add for BMS Error Monitor
                        //if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        //    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));

                        return false;
                    }
                }
                #endregion

                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }


        #region [ Cell Special Rule ]
        [UniAuto.UniBCS.OpiSpec.Help("FL0039")]
        public bool Filter_JobNotOnRobotArmByJobLocation_For1Arm2Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            RobotCanControlSlotBlockInfo _curSlotBlockInfo = null;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check Robot Arm Type / for CELL special case ]

                if (curRobot.Data.ARMJOBQTY != 2)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Arm Job Qty({1}) is illegal!", MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm2Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion             

                #region [ Get Current Can Control SlotBlockInfo List ]
                _curSlotBlockInfo = (RobotCanControlSlotBlockInfo)robotConText[eRobotContextParameter.CurSlotBlockInfoEntity];

                //當取不到值時則要回NG
                if (_curSlotBlockInfo == null || _curSlotBlockInfo.CurBlockCanControlJobList.Count <= 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get Current CanControlSlotBlockInfoList entity!", curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Current CanControlSlotBlockInfoList entity!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curSlotBlockInfo_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                foreach (Job _job in _curSlotBlockInfo.CurBlockCanControlJobList)
                {
                    if (_job.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) curStageID({6}) is Robot Arm({7})!",
                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, _job.CassetteSequenceNo, _job.JobSequenceNo,
                                _job.RobotWIP.CurRouteID,_job.RobotWIP.CurStepNo.ToString(), _job.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) curStageID({5}) is Robot Arm({6})!",
                            MethodBase.GetCurrentMethod().Name, _job.CassetteSequenceNo, _job.JobSequenceNo, _job.RobotWIP.CurRouteID,
                            _job.RobotWIP.CurStepNo.ToString(), _job.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Location_Is_Robot);
                        robotConText.SetReturnMessage(errMsg);

                        errCode = eJobFilter_ReturnCode.NG_Job_Location_Is_Robot;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, _job.EQPJobID, "0", "ROBOT"));

                        return false;
                    }
                }

                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }



        [UniAuto.UniBCS.OpiSpec.Help("FL0040")]
        public bool Filter_JobOnRobotArmByJobLocation_For1Arm2Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            RobotCanControlSlotBlockInfo _curSlotBlockInfo = null;
            string errCode = string.Empty;

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

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check Robot Arm Type / for CELL special case ]

                if (curRobot.Data.ARMJOBQTY != 2)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Arm Job Qty({2}) is illegal!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Arm Job Qty({1}) is illegal!", MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_RobotArmType_IsNot_1Arm2Job);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Current Can Control SlotBlockInfo List ]
                _curSlotBlockInfo = (RobotCanControlSlotBlockInfo)robotConText[eRobotContextParameter.CurSlotBlockInfoEntity];

                //當取不到值時則要回NG
                if (_curSlotBlockInfo == null || _curSlotBlockInfo.CurBlockCanControlJobList.Count <= 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get Current CanControlSlotBlockInfoList entity!", curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Current CanControlSlotBlockInfoList entity!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curSlotBlockInfo_Is_Null);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Set Is2ndCmdCheckFlag!", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_NoSet_2ndCmdCheckFlag_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ by 2nd Command Check Flag define Robot Location StageID ]

                string curStageID = string.Empty;
                string funcName = string.Empty;

                foreach (Job _job in _curSlotBlockInfo.CurBlockCanControlJobList)
                {
                    if (!is2ndCmdFlag)
                    {
                        curStageID = _job.RobotWIP.CurLocation_StageID;
                        funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;
                    }
                    else
                    {
                        funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;

                        #region [ Get Defind 1st Normal Robot Command ]
                        DefineNormalRobotCmd cur1stRobotCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.Define_1stNormalRobotCommandInfo];

                        //找不到 defineNormalRobotCmd 回NG
                        if (cur1stRobotCmd == null)
                        {
                            #region[DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get 1st defineNormalRobotCmd!", curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion

                            errMsg = string.Format("[{0}] can not Get 1st defineNormalRobotCmd!", MethodBase.GetCurrentMethod().Name);

                            robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }
                        #endregion
                        #region [ by 1st Cmd Define Job Location ]

                        //SPEC定義[ Wait_Proc_00028 ] 1Arm 2Job要額外處理
                        //0: None      //1: Put          //2: Get
                        //4: Exchange  //8: Put Ready    //16: Get Ready       //32: Get/Put
                        switch (cur1stRobotCmd.Cmd01_Command)
                        {
                            case 1:  //PUT
                            case 4:  //Exchange
                            case 32: //Get/Put

                                //Local Stage is Stage
                                curStageID = cur1stRobotCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0');
                                break;

                            case 2:  //PUT
                            case 8:  //Exchange
                            case 16: //Get/Put

                                //Local Stage is Stage
                                curStageID = eRobotCommonConst.ROBOT_HOME_STAGEID;
                                break;

                            default:
                                #region[DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) 1st defineNormalRobotCmd Action({3}) is out of Range!", curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion

                                errMsg = string.Format("[{0}] can not Get 1st defineNormalRobotCmd Action({1}) is out of Range!", MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString());

                                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail);
                                robotConText.SetReturnMessage(errMsg);

                                errCode = eJobFilter_ReturnCode.NG_ProductTypeCheck_Fail;//add for BMS Error Monitor
                                if (!curRobot.CheckErrorList.ContainsKey(errCode))
                                    curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, _job.EQPJobID, "0", "ROBOT"));

                                return false;
                        }
                        #endregion
                    }

                    if (curStageID != eRobotCommonConst.ROBOT_HOME_STAGEID)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteID({5}) curStepNo({6}) curStageID({7}) is not Robot Arm({8})!",
                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, _job.CassetteSequenceNo, _job.JobSequenceNo,
                                _job.RobotWIP.CurRouteID, _job.RobotWIP.CurStepNo.ToString(), curStageID, eRobotCommonConst.ROBOT_HOME_STAGEID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) curStageID({5}) is not Robot Arm({6})!!",
                            MethodBase.GetCurrentMethod().Name, _job.CassetteSequenceNo, _job.JobSequenceNo,
                            _job.RobotWIP.CurRouteID, _job.RobotWIP.CurStepNo.ToString(), curStageID, eRobotCommonConst.ROBOT_HOME_STAGEID);

                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Job_Location_IsNot_Robot);
                        robotConText.SetReturnMessage(errMsg);

                        errCode = eJobFilter_ReturnCode.NG_Job_Location_IsNot_Robot;//add for BMS Error Monitor
                        if (!curRobot.CheckErrorList.ContainsKey(errCode))
                            curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, _job.EQPJobID, "0", "ROBOT"));

                        return false;
                    }
                }
                #endregion

                robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }



        #endregion












    }
}