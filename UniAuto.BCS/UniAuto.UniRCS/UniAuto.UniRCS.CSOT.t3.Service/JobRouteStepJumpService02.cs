using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class JobRouteStepJumpService
    {

//RouteStepJump Funckey = "JP" + XXXX(序列號)

		/// <summary>
		///  <br>CFREW line normal mode</br>
		///  <br>goto to rework again if job rework realcount &lt maxcount</br>
		/// </summary>
		/// <param name="robotConText">robot context object</param>
		/// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0011")]
		public bool RouteStepJump_Rework(IRobotContext robotConText) {
			string strlog = string.Empty;
			string errMsg = string.Empty;

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

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion
				#region [ Get curBcsJob Entity ]

				Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

				//找不到 Job 回NG
				if (curBcsJob == null) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
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
				#region [ Get 2nd Command Check Flag ]

				bool is2ndCmdFlag = false;

				try {
					is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
				} catch (Exception) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
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

				#region [* 檢查Jump條件:job rework realcount < maxcount]
				int realcnt = 0;
				int maxcnt = 0;
				int.TryParse(curBcsJob.RobotWIP.ReworkRealCount, out realcnt);
				int.TryParse(curBcsJob.CfSpecial.ReworkMaxCount, out maxcnt);
				if (realcnt >= maxcnt) {
					//不用再rework,所以不jump,直接Reply True
					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
					return true;
				}
				#endregion
				#region [* 檢查Jump條件:indexer ReworkForceToUnloaderCST OFF]
				Equipment indexer = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
				if (indexer != null) {
					if (indexer.File.ReworkForceToUnloaderCST == eBitResult.ON) {
						//ForceToUnloaderCST on不用再rework,所以不jump,直接Reply True
						robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
						robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
						return true;
					}
				}
				#endregion
				#region [* 檢查Jump條件:stripper not down]
				Equipment stripper = ObjectManager.EquipmentManager.GetEQP("L4");
				if (stripper != null) {
					if (stripper.File.Status != eEQPStatus.RUN && stripper.File.Status != eEQPStatus.IDLE) {
						//stripper down不用再rework,所以不jump,直接Reply True
						robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
						robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
						return true;
					}
				}
				#endregion

				#region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

				RobotRouteStep curCheckRouteStep = null;

				if (is2ndCmdFlag == false) {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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

					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

					}
				} else {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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
					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
					}

				}

				#endregion

				#region [ Check CheckStep Action Must Put ]

				if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT) {
					#region  [DebugLog]

					if (IsShowDetialLog == true) {
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

				#region [ Get GotoStepID 來更新 NextStepNO]

				int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

				if (GoToStepID == 0) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				//Get Change StepID 後的NextStepNO
				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;

				}

				#endregion

				#region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

				if (is2ndCmdFlag == false) {

					#region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

					//Step 切換一定要紀錄Log 
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
						curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5}), NextStepNo({6}) to ({7})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
												curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.CurStepNo = GoToStepID;
							curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				} else {

					#region [ Update NextStepID by JumpGotoSTEPID Setting ]

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion


					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.NextStepNo != GoToStepID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.NextStepNo = GoToStepID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				}

				#region [ Get Jump GoTo Step Entity ]

				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
										    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion

				#region [ Get Jump GoTo Step Can Use StageList ]

				RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

				string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

				//取得目前Step的CurCanUseStageList做關聯後清除
				List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

				if (curJumpGotoStepCanUseStageList != null) {
					curJumpGotoStepCanUseStageList.Clear();
				} else {
					curJumpGotoStepCanUseStageList = new List<RobotStage>();
				}


				for (int i = 0; i < curStepCanUseStageList.Length; i++) {

					#region [ Check Stage is Exist ]

					RobotStage curStage;

					curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

					//找不到 Robot Stage 回NG
					if (curStage == null) {

						#region[DebugLog]

						if (IsShowDetialLog == true) {
							strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
													curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

							Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
						}

						#endregion

						return false;
					}

					if (curJumpGotoStepCanUseStageList.Contains(curStage) == false) {

						curJumpGotoStepCanUseStageList.Add(curStage);

					}

					#endregion

				}
				#endregion
				#endregion

				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
				robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
				return true;
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

		/// <summary>
		///  <br>CFMAC line through/random mode , CFAOI line through mode</br>
		///  <br>direct goto to unloading port if loading port job no sampling</br>
		/// </summary>
		/// <param name="robotConText">robot context object</param>
		/// <returns>true=no error</returns>		
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0012")]
		public bool RouteStepJump_NoSampling(IRobotContext robotConText) {
			string strlog = string.Empty;
			string errMsg = string.Empty;

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

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion
				#region [ Get curBcsJob Entity ]

				Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

				//找不到 Job 回NG
				if (curBcsJob == null) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
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
				#region [ Get 2nd Command Check Flag ]

				bool is2ndCmdFlag = false;

				try {
					is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
				} catch (Exception) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
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

				#region [* 檢查Jump條件:job no sampling]
				if (curBcsJob.SamplingSlotFlag == "1") {
					//要抽檢，所以不jump,直接Reply True
					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
					return true;
				}
				#endregion

				#region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

				RobotRouteStep curCheckRouteStep = null;

				if (is2ndCmdFlag == false) {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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

					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

					}
				} else {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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
					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
					}

				}

				#endregion

				#region [ Check CheckStep Action Must Put ]

				if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT) {
					#region  [DebugLog]

					if (IsShowDetialLog == true) {
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

				#region [ Get GotoStepID 來更新 NextStepNO]

				int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

				if (GoToStepID == 0) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				//Get Change StepID 後的NextStepNO
				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;

				}

				#endregion

				#region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

				if (is2ndCmdFlag == false) {

					#region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

					//Step 切換一定要紀錄Log 
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
						curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5}), NextStepNo({6}) to ({7})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
												curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.CurStepNo = GoToStepID;
							curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				} else {

					#region [ Update NextStepID by JumpGotoSTEPID Setting ]

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion


					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.NextStepNo != GoToStepID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.NextStepNo = GoToStepID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				}

				#region [ Get Jump GoTo Step Entity ]

				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
										    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion

				#region [ Get Jump GoTo Step Can Use StageList ]

				RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

				string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

				//取得目前Step的CurCanUseStageList做關聯後清除
				List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

				if (curJumpGotoStepCanUseStageList != null) {
					curJumpGotoStepCanUseStageList.Clear();
				} else {
					curJumpGotoStepCanUseStageList = new List<RobotStage>();
				}


				for (int i = 0; i < curStepCanUseStageList.Length; i++) {

					#region [ Check Stage is Exist ]

					RobotStage curStage;

					curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

					//找不到 Robot Stage 回NG
					if (curStage == null) {

						#region[DebugLog]

						if (IsShowDetialLog == true) {
							strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
													curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

							Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
						}

						#endregion

						return false;
					}

					if (curJumpGotoStepCanUseStageList.Contains(curStage) == false) {

						curJumpGotoStepCanUseStageList.Add(curStage);

					}

					#endregion

				}
				#endregion
				#endregion

				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
				robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
				return true;
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

		/// <summary>
		///  <br>CFMAC line fix mode</br>
		///  <br>direct goto to unloading port if loading port job no sampling or is buffering</br>
		/// </summary>
		/// <param name="robotConText">robot context object</param>
		/// <returns>true=no error</returns>		
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0013")]
		public bool RouteStepJump_NoSamplingOrIsBuffering(IRobotContext robotConText) {
			string strlog = string.Empty;
			string errMsg = string.Empty;

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

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion
				#region [ Get curBcsJob Entity ]

				Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

				//找不到 Job 回NG
				if (curBcsJob == null) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
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
				#region [ Get 2nd Command Check Flag ]

				bool is2ndCmdFlag = false;

				try {
					is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
				} catch (Exception) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
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

				#region [* 檢查Jump條件:job no sampling or is buffering]
				//if (curBcsJob.SamplingSlotFlag == "1" || curBcsJob.CfSpecial.RCSBufferingFlag != "1") 
                if (curBcsJob.SamplingSlotFlag == "1" && curBcsJob.CfSpecial.RCSBufferingFlag != "1")
                {
					//要抽檢或不是buffering，所以不jump,直接Reply True
					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
					return true;
				}
				#endregion

				#region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

				RobotRouteStep curCheckRouteStep = null;

				if (is2ndCmdFlag == false) {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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

					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

					}
				} else {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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
					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
					}

				}

				#endregion

				#region [ Check CheckStep Action Must Put ]

				if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT) {
					#region  [DebugLog]

					if (IsShowDetialLog == true) {
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

				#region [ Get GotoStepID 來更新 NextStepNO]

				int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

				if (GoToStepID == 0) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
										    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				//Get Change StepID 後的NextStepNO
				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
										    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;

				}

				#endregion

				#region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

				if (is2ndCmdFlag == false) {

					#region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

					//Step 切換一定要紀錄Log 
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
						curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5}), NextStepNo({6}) to ({7})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
												curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.CurStepNo = GoToStepID;
							curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				} else {

					#region [ Update NextStepID by JumpGotoSTEPID Setting ]

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion


					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.NextStepNo != GoToStepID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.NextStepNo = GoToStepID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				}

				#region [ Get Jump GoTo Step Entity ]

				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion

				#region [ Get Jump GoTo Step Can Use StageList ]

				RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

				string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

				//取得目前Step的CurCanUseStageList做關聯後清除
				List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

				if (curJumpGotoStepCanUseStageList != null) {
					curJumpGotoStepCanUseStageList.Clear();
				} else {
					curJumpGotoStepCanUseStageList = new List<RobotStage>();
				}


				for (int i = 0; i < curStepCanUseStageList.Length; i++) {

					#region [ Check Stage is Exist ]

					RobotStage curStage;

					curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

					//找不到 Robot Stage 回NG
					if (curStage == null) {

						#region[DebugLog]

						if (IsShowDetialLog == true) {
							strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
													curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

							Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
						}

						#endregion

						return false;
					}

					if (curJumpGotoStepCanUseStageList.Contains(curStage) == false) {

						curJumpGotoStepCanUseStageList.Add(curStage);

					}

					#endregion

				}
				#endregion
				#endregion

				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
				robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
				return true;
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

		/// <summary>
		///  <br>CF line Normal mode</br>
		///  <br>direct goto to unloading port if loading port job is buffering</br>
		/// </summary>
		/// <param name="robotConText">robot context object</param>
		/// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0014")]
		public bool RouteStepJump_IsBuffering(IRobotContext robotConText) {
			string strlog = string.Empty;
			string errMsg = string.Empty;

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

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion
				#region [ Get curBcsJob Entity ]

				Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

				//找不到 Job 回NG
				if (curBcsJob == null) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
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
				#region [ Get 2nd Command Check Flag ]

				bool is2ndCmdFlag = false;

				try {
					is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
				} catch (Exception) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
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

				#region [* 檢查Jump條件:is buffering]
				if (curBcsJob.CfSpecial.RCSBufferingFlag != "1") {
					//不是buffering，所以不jump,直接Reply True
					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
					return true;
				}
				#endregion

				#region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

				RobotRouteStep curCheckRouteStep = null;

				if (is2ndCmdFlag == false) {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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

					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

					}
				} else {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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
					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
					}

				}

				#endregion

				#region [ Check CheckStep Action Must Put ]

				if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT) {
					#region  [DebugLog]

					if (IsShowDetialLog == true) {
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

				#region [ Get GotoStepID 來更新 NextStepNO]

				int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

				if (GoToStepID == 0) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				//Get Change StepID 後的NextStepNO
				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;

				}

				#endregion

				#region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

				if (is2ndCmdFlag == false) {

					#region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

					//Step 切換一定要紀錄Log 
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion
				
					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
						curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5}), NextStepNo({6}) to ({7})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
												curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.CurStepNo = GoToStepID;
							curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				} else {

					#region [ Update NextStepID by JumpGotoSTEPID Setting ]

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion
					

					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.NextStepNo != GoToStepID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.NextStepNo = GoToStepID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				}

				#region [ Get Jump GoTo Step Entity ]

				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion

				#region [ Get Jump GoTo Step Can Use StageList ]

				RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

				string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

				//取得目前Step的CurCanUseStageList做關聯後清除
				List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

				if (curJumpGotoStepCanUseStageList != null) {
					curJumpGotoStepCanUseStageList.Clear();
				} else {
					curJumpGotoStepCanUseStageList = new List<RobotStage>();
				}


				for (int i = 0; i < curStepCanUseStageList.Length; i++) {

					#region [ Check Stage is Exist ]

					RobotStage curStage;

					curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

					//找不到 Robot Stage 回NG
					if (curStage == null) {

						#region[DebugLog]

						if (IsShowDetialLog == true) {
							strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
													curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

							Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
						}

						#endregion

						return false;
					}

					if (curJumpGotoStepCanUseStageList.Contains(curStage) == false) {

						curJumpGotoStepCanUseStageList.Add(curStage);

					}

					#endregion

				}
				#endregion
				#endregion

				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
				robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
				return true;
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

		/// <summary>
		///  <br>CFREP line through/fix/random mode</br>
		///  <br>direct goto to unloading port if loading port job judge is not repair(not RP/IR)</br>
		/// </summary>
		/// <param name="robotConText">robot context object</param>
		/// <returns>true=no error</returns>		
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0015")]
		public bool RouteStepJump_NoRepair(IRobotContext robotConText) {
			string strlog = string.Empty;
			string errMsg = string.Empty;

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

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion
				#region [ Get curBcsJob Entity ]

				Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

				//找不到 Job 回NG
				if (curBcsJob == null) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
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
				#region [ Get 2nd Command Check Flag ]

				bool is2ndCmdFlag = false;

				try {
					is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
				} catch (Exception) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
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

				#region [* 檢查Jump條件:job judge is no repair]
                if (curBcsJob.RobotWIP.CurSendOutJobJudge == "5" || curBcsJob.RobotWIP.CurSendOutJobJudge == "6")
                {
					//judge是RP或IR要修，所以不jump,直接Reply True
					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
					return true;
				}
				#endregion

				#region [ Get Current Check Step Entity from Job curStep/NextStep by Is2nd ]

				RobotRouteStep curCheckRouteStep = null;

				if (is2ndCmdFlag == false) {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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

					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

					}
				} else {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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
					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
					}

				}

				#endregion

				#region [ Check CheckStep Action Must Put ]

				if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT) {
					#region  [DebugLog]

					if (IsShowDetialLog == true) {
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

				#region [ Get GotoStepID 來更新 NextStepNO]

				int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

				if (GoToStepID == 0) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				//Get Change StepID 後的NextStepNO
				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;

				}

				#endregion

				#region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

				if (is2ndCmdFlag == false) {

					#region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

					//Step 切換一定要紀錄Log 
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
						curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5}), NextStepNo({6}) to ({7})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
												curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.CurStepNo = GoToStepID;
							curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				} else {

					#region [ Update NextStepID by JumpGotoSTEPID Setting ]

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion


					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.NextStepNo != GoToStepID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.NextStepNo = GoToStepID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				}

				#region [ Get Jump GoTo Step Entity ]

				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion

				#region [ Get Jump GoTo Step Can Use StageList ]

				RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

				string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

				//取得目前Step的CurCanUseStageList做關聯後清除
				List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

				if (curJumpGotoStepCanUseStageList != null) {
					curJumpGotoStepCanUseStageList.Clear();
				} else {
					curJumpGotoStepCanUseStageList = new List<RobotStage>();
				}


				for (int i = 0; i < curStepCanUseStageList.Length; i++) {

					#region [ Check Stage is Exist ]

					RobotStage curStage;

					curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

					//找不到 Robot Stage 回NG
					if (curStage == null) {

						#region[DebugLog]

						if (IsShowDetialLog == true) {
							strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
													curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

							Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
						}

						#endregion

						return false;
					}

					if (curJumpGotoStepCanUseStageList.Contains(curStage) == false) {

						curJumpGotoStepCanUseStageList.Add(curStage);

					}

					#endregion

				}
				#endregion
				#endregion

				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
				robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
				return true;
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

		/// <summary>
		///  <br>CFPSH line</br>
		///  <br>direct goto to unloading port if job no forcepsh </br>
		/// </summary>
		/// <param name="robotConText">robot context object</param>
		/// <returns>true=no error</returns>		
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0016")]
		public bool RouteStepJump_NoForcePSH(IRobotContext robotConText) {
			string strlog = string.Empty;
			string errMsg = string.Empty;

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

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curRobot_Is_Null);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion
				#region [ Get curBcsJob Entity ]

				Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

				//找不到 Job 回NG
				if (curBcsJob == null) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
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
				#region [ Get 2nd Command Check Flag ]

				bool is2ndCmdFlag = false;

				try {
					is2ndCmdFlag = (bool)robotConText[eRobotContextParameter.Is2ndCmdCheckFlag];
				} catch (Exception) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
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

				if (is2ndCmdFlag == false) {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.CurStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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

					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

					}
				} else {
					if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(curBcsJob.RobotWIP.NextStepNo) == false) {

						//找不到 CurStep Route 回NG
						#region[DebugLog]

						if (IsShowDetialLog == true) {
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
					} else {
						curCheckRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];
					}

				}

				#endregion

				#region [* 檢查Jump條件:job no forcepsh]
				if (curBcsJob.RobotWIP.CfSpecial.CFSpecialReserved.ForcePSHbit == "1") 
				{
					//是ForcePSH，所以不jump,直接Reply True,但要額外檢查有可進的PSH
					#region check exists valid target stage
					//get current step stage id list
					string[] curStepStageIDList = curCheckRouteStep.Data.STAGEIDLIST.Split(',');
					
					//get job trackingdata					
					var Trackinglist = ObjectManager.SubJobDataManager.Decode2(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData, "TrackingData");					
					foreach (var id in curStepStageIDList) 
					{
						RobotStage stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(id);
						if (stage == null) {
							continue;
						}

						if (stage.Data.STAGETYPE != eRobotStageType.EQUIPMENT) {
							continue;
						}

						//排除巳在目前位置
						if (stage.Data.STAGEID == curBcsJob.RobotWIP.CurLocation_StageID) {
							continue;
						}

						#region check eq recipe id is not full of 0
						Equipment stageEQP = ObjectManager.EquipmentManager.GetEQP(stage.Data.NODENO);
						if (stageEQP == null) {
							continue;
						}
						string curNodePPID = curBcsJob.PPID.Substring(stageEQP.Data.RECIPEIDX, stageEQP.Data.RECIPELEN);
						string curByPassPPID = new string('0', stageEQP.Data.RECIPELEN);
						if (curNodePPID == curByPassPPID) {
							continue;
						}
						#endregion

						#region check eq is not tracked
						//get stage's tracking
						int offset;
						bool done = int.TryParse(stage.Data.TRACKDATASEQLIST, out offset);
						if (!done) {
							continue;
						}
						var Tracking = Trackinglist.Find(t => t.Item2 == offset);
						if (Tracking == null) {
							continue;
						}
						//check stage is not tracked
						if (Tracking.Item4.Contains('1')) {
							continue;
						}
						#endregion

						//有找到可進的PSH,所以不jump
						robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
						robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
						return true;
					}
					#endregion
				}
				#endregion

				#region [ Check CheckStep Action Must Put ]

				if (curCheckRouteStep.Data.ROBOTACTION != eRobot_DB_CommandAction.ACTION_PUT) {
					#region  [DebugLog]

					if (IsShowDetialLog == true) {
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

				#region [ Get GotoStepID 來更新 NextStepNO]

				int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

				if (GoToStepID == 0) {

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				//Get Change StepID 後的NextStepNO
				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;

				}

				#endregion

				#region [ Jump條件成立要回True並以新Step更新CurCanUseJobList的處理 ]

				if (is2ndCmdFlag == false) {

					#region [ Update CurStepID by JumpGotoSTEPID Setting ] 注意!因為CurStepNo變動同時也要更新NextStepNo

					//Step 切換一定要紀錄Log 
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
						curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5}), NextStepNo({6}) to ({7})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
												curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.CurStepNo = GoToStepID;
							curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				} else {

					#region [ Update NextStepID by JumpGotoSTEPID Setting ]

					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion


					//有變化才記Log並存檔
					if (curBcsJob.RobotWIP.NextStepNo != GoToStepID) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

						Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

						lock (curBcsJob) {
							curBcsJob.RobotWIP.NextStepNo = GoToStepID;
						}

						//Save File
						ObjectManager.JobManager.EnqueueSave(curBcsJob);

					}

					#endregion

				}

				#region [ Get Jump GoTo Step Entity ]

				if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false) {
					#region[DebugLog]

					if (IsShowDetialLog == true) {
						strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
												curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
												GoToStepID.ToString());

						Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
					}

					#endregion

					errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
											MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
											GoToStepID.ToString());

					robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
					robotConText.SetReturnMessage(errMsg);

					return false;
				}

				#endregion

				#region [ Get Jump GoTo Step Can Use StageList ]

				RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

				string[] curStepCanUseStageList = curJumpGotoRouteStep.Data.STAGEIDLIST.Split(',');

				//取得目前Step的CurCanUseStageList做關聯後清除
				List<RobotStage> curJumpGotoStepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

				if (curJumpGotoStepCanUseStageList != null) {
					curJumpGotoStepCanUseStageList.Clear();
				} else {
					curJumpGotoStepCanUseStageList = new List<RobotStage>();
				}


				for (int i = 0; i < curStepCanUseStageList.Length; i++) {

					#region [ Check Stage is Exist ]

					RobotStage curStage;

					curStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(curStepCanUseStageList[i]);

					//找不到 Robot Stage 回NG
					if (curStage == null) {

						#region[DebugLog]

						if (IsShowDetialLog == true) {
							strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get RobotStageInfo by StageID({3})!",
													curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curStepCanUseStageList[i]);

							Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
						}

						#endregion

						return false;
					}

					if (curJumpGotoStepCanUseStageList.Contains(curStage) == false) {

						curJumpGotoStepCanUseStageList.Add(curStage);

					}

					#endregion

				}
				#endregion
				#endregion

				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
				robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
				return true;
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}


        /// <summary>
        ///  <br>不適用於CF REP，適用於CF AOI MAC。Sampling Flag ON時必出片到EQ；OFF時只出非OK的片放到Mix Unloader</br>
        /// </summary>
        /// <param name="robotConText">robot context object</param>
        /// <returns>true=no error</returns>		
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0021")]
        public bool RouteStepJump_ThroughMode_NoSamplingToULD(IRobotContext robotConText)
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

                #region [* 檢查Jump條件:job no sampling]
                if (curBcsJob.SamplingSlotFlag == "1" || curBcsJob.RobotWIP.CurSendOutJobJudge == "1")
                {
                    //要抽檢，所以不jump,直接Reply True
                    //Judge OK，ThroughMode時留在Loader Port
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion
                // No Sampling 且 Judge不OK，ThroughMode時Jump到Unloader
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

                #region [ Get GotoStepID 來更新 NextStepNO]

                int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (GoToStepID == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = GoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion


                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != GoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = GoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }

                #region [ Get Jump GoTo Step Entity ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

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

        /// <summary>
        ///  <br>不適用於CF REP，適用於CF AOI MAC。Sampling Flag ON時必出片到EQ；OFF時出OK的片到OK Unloader，出非OK的片放到Mix Unloader</br>
        /// </summary>
        /// <param name="robotConText">robot context object</param>
        /// <returns>true=no error</returns>		
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0022")]
        public bool RouteStepJump_FixMode_NoSamplingToULD(IRobotContext robotConText)
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

                #region [* 檢查Jump條件:job no sampling]
                if (curBcsJob.SamplingSlotFlag == "1")
                {
                    //要抽檢，所以不jump,直接Reply True
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion
                // No Sampling，FixMode時Jump到Unloader
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

                #region [ Get GotoStepID 來更新 NextStepNO]

                int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (GoToStepID == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = GoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion


                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != GoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = GoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }

                #region [ Get Jump GoTo Step Entity ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

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

        /// <summary>
        ///  <br>適用於CF REP，不適用於CF AOI MAC。Sampling Flag ON時只出IR,RP的片到EQ, OK的片直接去OK Unloader, 其他的片到Mix Unloader；OFF時出OK的片到OK Unloader，出非OK的片放到Mix Unloader</br>
        /// </summary>
        /// <param name="robotConText">robot context object</param>
        /// <returns>true=no error</returns>		
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("JP0023")]
        public bool RouteStepJump_NoSamplingOrIsBufferingOrJudgeOK(IRobotContext robotConText)
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

                #region [* 檢查Jump條件:job judge and eqp flag]
                if (curBcsJob.SamplingSlotFlag == "1" && curBcsJob.CfSpecial.RCSBufferingFlag != "1" && curBcsJob.RobotWIP.CurSendOutJobJudge == "5")//5:RP
                {
                    IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(curBcsJob.EQPFlag, "EQPFlag");
                    if (subItem["RepairGlass"] == ((int)eBitResult.ON).ToString())
                    {
                        //Sampling ON, Judge RP, EQPFlag RepairGlass ON, 此片要修補, 所以不jump,直接Reply True
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                        return true;
                    }
                }
                if (curBcsJob.SamplingSlotFlag == "1" && curBcsJob.CfSpecial.RCSBufferingFlag != "1" && curBcsJob.RobotWIP.CurSendOutJobJudge == "6")//6:IR
                {
                    IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(curBcsJob.EQPFlag, "EQPFlag");
                    if (subItem["InkRepairGlass"] == ((int)eBitResult.ON).ToString())
                    {
                        //Sampling ON, Judge IR, EQPFlag InkRepairGlass ON, 此片要修補, 所以不jump,直接Reply True
                        robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                        return true;
                    }
                }
                #endregion

                // Sampling Off 或 RCSBufferingFlag ON 或 CurSendOutJobJudge 非IR,RP，FixMode時Jump到Unloader
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

                #region [ Get GotoStepID 來更新 NextStepNO]

                int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (GoToStepID == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_ForceCleanOut_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = GoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion


                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != GoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = GoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }

                #region [ Get Jump GoTo Step Entity ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

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

        /// <summary>//Yang Add CVD 20160830 //暂时不用了
        /// CVD LDRQ,CST no galss to CVD->jump to CVD,CVD HDC_CVD Route新增step,遵循先洗完的Glass先喂给CVD
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("JP0027")]
        public bool RouteStepJump_CVDnotRTC(IRobotContext robotConText)
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

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region[Check CVD stage status]

                RobotStage stage13 = ObjectManager.RobotStageManager.GetRobotStagebyStageID("13");
                RobotStage stage14 = ObjectManager.RobotStageManager.GetRobotStagebyStageID("14");
                if (stage13.File.CurStageStatus == eRobotStageStatus.NO_REQUEST && stage14.File.CurStageStatus == eRobotStageStatus.NO_REQUEST)
                {
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                    //CVD不要片,不需要jump
                }

                #endregion

                #region [ Check Goto CVD or Return To CST ]

                if (curRobot.CLNRTCWIP == true)
                {
                    // CST 里有要去CVD的玻璃,优先出CST里的,不做jump,直接回true
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
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

                #region [ Get GotoStepID 來更新 NextStepNO]

                int GoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (GoToStepID == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is Force Clean Out But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_CVD_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is GotoCVD But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_CVD_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != GoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = GoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion


                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != GoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), GoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = GoToStepID;
                        }

                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);

                    }

                    #endregion

                }

                #region [ Get Jump GoTo Step Entity ]

                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(GoToStepID) == false)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                GoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            GoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[GoToStepID];

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

        /// <summary>
        /// add by yang 20161002  CVD RTC glass jump to next EQP
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("")]
        public bool RouteStepJump_ForEQPRTC(IRobotContext robotConText)
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

                #region Check Tracking Data 是否已经去过CLN,去过再来考虑jump

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
               
                    #region [ Decode Job TrackingData(代表進入 CLN 這個Stage的TrackingData) ]
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

                    #region [Processed by CLN (Tracing data)]
                    if (robotLine.Data.LINETYPE.Contains("CVD_"))
                    {
                        if (dicJobTrackingData.ContainsKey("CDC/DHF"))
                        {
                            if (dicJobTrackingData["CDC/DHF"] == "0")
                            {
                                #region  [DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass Tracking Data not processed by CLN. Don't Jump to CVD ",
                                        curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                                }
                                #endregion
                                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                                return true;
                            }
                        }
                    }
                    else if (robotLine.Data.LINETYPE.Contains("ELA_"))
                    {
                        if (dicJobTrackingData.ContainsKey("Cleanfor1st"))
                        {
                            if (dicJobTrackingData["Cleanfor1st"] == "0")
                            {
                                #region  [DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass Tracking Data not processed by CLN. Don't Jump to ELA ",
                                        curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                                }
                                #endregion
                                robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                                robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass Tracking Data No CLN!!",
                                curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                        }
                        #endregion
                        errMsg = string.Format("[{0}] Tracking Data No define!!", curBcsJob);
                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }

                    #endregion           

                #endregion

                #region Check EQPRTCFlag
                if(!curBcsJob.RobotWIP.EQPRTCFlag)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Flag({5}) is not RTC!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.EQPRTCFlag);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return true; //不做jump
                }
                #endregion

                #region [ Get EQP Jump GotoStepID ]

                int EQPRTCGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (EQPRTCGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is EQP RTC Jump To Next EQP But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is  EQP RTC (Need Jump To Next EQP) But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTC_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(EQPRTCGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is EQP RTC Jump To Next EQP But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is EQP RTC Jump To Next EQP But get GotoStepID({4}) Entity Fail",
                                             MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTC_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is  EQP RTC (Need Jump To Next EQP) Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is  EQP RTC (Need Jump To Next EQP) Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTCJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != EQPRTCGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is EQP RTC (Need Jump To Next EQP) Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = EQPRTCGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is EQP RTC (Need Jump To Next EQP) Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is EQP RTC (Need Jump To Next EQP) Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTCJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != EQPRTCGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is  EQP RTC (Need Jump To Next EQP) Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = EQPRTCGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(EQPRTCGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                EQPRTCGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID];

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

        /// <summary>
        /// add by qiumin 20161229 MQC RTC glass jump to next EQP
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("JP0030")]
        public bool RouteStepJump_ForCFEQPRTC(IRobotContext robotConText)
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

                #region Check Tracking Data 是否已经去过MQC other stage,去过再来考虑jump
                /*
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

                if (robotLine.Data.LINETYPE.Contains("MQC"))
                {
                    #region [ MQC Decode Job TrackingData(代表進入 MQC  Other Stage的TrackingData) ]
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

                    #region [MQC Processed by otehrEQ (Tracing data)]

                    if (dicJobTrackingData.ContainsKey("MCPD") || dicJobTrackingData.ContainsKey("SP"))
                    {
                        if (dicJobTrackingData["MCPD"] == "0" && dicJobTrackingData["SP"] == "0")
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass Tracking Data not processed by MCPD or SP. Don't Jump to TTP ",
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
                                string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass Tracking Data No QMP/QSP!!",
                                curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curRobot.Data.NODENO));
                        }
                        #endregion
                        errMsg = string.Format("[{0}] Tracking Data No define[MCPD/SP]!!");
                        robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_DecodeTrackingData_Fail);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                }

                    #endregion */

                #endregion

                #region Check EQPRTCFlag
                if (!curBcsJob.RobotWIP.EQPRTCFlag)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) Check Step({4}) Flag({5}) is not RTC!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                        curCheckRouteStep.Data.STEPID.ToString(), curBcsJob.RobotWIP.EQPRTCFlag);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return true; //不做jump
                }
                #endregion

                #region [ Get EQP Jump GotoStepID ]

                int EQPRTCGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (EQPRTCGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is EQP RTC Jump To Next EQP But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is  EQP RTC (Need Jump To Next EQP) But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTC_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(EQPRTCGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is EQP RTC Jump To Next EQP But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is EQP RTC Jump To Next EQP But get GotoStepID({4}) Entity Fail",
                                             MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTC_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is  EQP RTC (Need Jump To Next EQP) Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is  EQP RTC (Need Jump To Next EQP) Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTCJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != EQPRTCGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is EQP RTC (Need Jump To Next EQP) Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = EQPRTCGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is EQP RTC (Need Jump To Next EQP) Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is EQP RTC (Need Jump To Next EQP) Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTCJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != EQPRTCGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is  EQP RTC (Need Jump To Next EQP) Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = EQPRTCGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(EQPRTCGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                EQPRTCGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID];

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

        //20170314 BY QIUMIN
        [UniAuto.UniBCS.OpiSpec.Help("JP0031")]
        public bool RouteStepJump_ForARRAYDRYCHAMPERMISRTC(IRobotContext robotConText)
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

                #region[ Check MIX_MODE ]
                if (robotLine.File.IndexOperMode != eINDEXER_OPERATION_MODE.MIX_MODE) //if !MIX Run,return true,not check chamber mode
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Indexer Oper Mode Is Not MIX_MODE , No Need Check Chamber Mode",
                                        curRobot.Data.NODENO);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }
                    return true;

                    #endregion
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

                #region [ Check Dry MixNochambermode can receive job,if can ,needn't jump ]
                string _curUnitNo = (string)curRobot.Context[eRobotContextParameter.UnitNo];
                string MixNochambermode = (string)curRobot.Context[eRobotContextParameter.chambermode];
                #region [Check curUnit]
                if (string.IsNullOrEmpty(_curUnitNo))
                {
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curUnit is Null!",
                                                "L4", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    errMsg = string.Format("[{0}] curUnit is Null!",
                                            MethodBase.GetCurrentMethod().Name);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_No_Can_Use_Chamber);
                    robotConText.SetReturnMessage(errMsg);

                    /*errCode = eJobFilter_ReturnCode.NG_No_Can_Use_Chamber;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;*/
                }

                Unit curUnit = ObjectManager.UnitManager.GetUnit("L4", _curUnitNo);
                if (curUnit == null)
                {
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Find Unit!",
                                                "L4", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    errMsg = string.Format("[{0}] can not Find Unit!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_No_Can_Use_Chamber);
                    robotConText.SetReturnMessage(errMsg);

                    errCode = eJobFilter_ReturnCode.NG_No_Can_Use_Chamber;//add for BMS Error Monitor
                    if (!curRobot.CheckErrorList.ContainsKey(errCode))
                        curRobot.CheckErrorList.Add(errCode, Tuple.Create(errMsg, curBcsJob.EQPJobID, "0", "ROBOT"));
                    return false;
                }
                #endregion

                if (curBcsJob.ArraySpecial.ProcessType.Equals(MixNochambermode) && (curUnit.File.Status == eEQPStatus.RUN || curUnit.File.Status == eEQPStatus.IDLE))
                {
                    
                    return true ;
                }
                #endregion

                #region [ Get EQP Jump GotoStepID ]

                int EQPRTCGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (EQPRTCGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is EQP RTC Jump To Next EQP But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is  EQP RTC (Need Jump To Next EQP) But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTC_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(EQPRTCGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is EQP RTC Jump To Next EQP But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is EQP RTC Jump To Next EQP But get GotoStepID({4}) Entity Fail",
                                             MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTC_GotoStepNo_Is_Fail);
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is  EQP RTC (Need Jump To Next EQP) Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is  EQP RTC (Need Jump To Next EQP) Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTCJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != EQPRTCGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is EQP RTC (Need Jump To Next EQP) Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = EQPRTCGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID].Data.NEXTSTEPID;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is EQP RTC (Need Jump To Next EQP) Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is EQP RTC (Need Jump To Next EQP) Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_EQPWaitRTCJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != EQPRTCGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is  EQP RTC (Need Jump To Next EQP) Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), EQPRTCGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = EQPRTCGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(EQPRTCGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                EQPRTCGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            EQPRTCGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[EQPRTCGoToStepID];

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

        //20170904 BY QIUMIN
        [UniAuto.UniBCS.OpiSpec.Help("JP0032")]
        public bool RouteStepJump_ForATSTurnByPass(IRobotContext robotConText)
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


                #region [ Check Job PPID last No]
                String turnRciepNo=curBcsJob.PPID.Substring(13,1) ;
                if (turnRciepNo == "")
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}]  Job PPID length error !!", curRobot.Data.NODENO));
                    }
                    #endregion
                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobRouteStepJump_ReturnMessage.OK_Message);
                    return true;
                }


                if (turnRciepNo == "E")
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("PPID Last No='E' ,[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] Glass will go to ATS not go to Turn Table. ",
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

                #region [ Get TTP Daily Check GotoStepID ]

                int CheckGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (CheckGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheck_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(CheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

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
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != CheckGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Aging Disable Mode Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = CheckGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID].Data.NEXTSTEPID;
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
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is Aging Disable Mode Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != CheckGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Aging Disable Mode Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = CheckGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(CheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                CheckGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID];

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


        //20170904 BY QIUMIN
        [UniAuto.UniBCS.OpiSpec.Help("JP0033")]
        public bool RouteStepJump_ForATSJobJumpInCycle(IRobotContext robotConText)
        {

            string strlog = string.Empty;
            string errMsg = string.Empty;
            string curBcsJobEqpFlag_TurnFlag = string.Empty;
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


                #region [ Check Job TurnFlag]
                IDictionary<string, string> curBcsJobEqpFlag = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG, eJOBDATA.EQPFlag);
                curBcsJobEqpFlag_TurnFlag = curBcsJobEqpFlag["TurnFlag"];
                // 0: go to CST;1: go to T/T
                if (curBcsJobEqpFlag_TurnFlag == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobTurnFlag!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] can not Get JobTurnFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJobTurnFlag_Is_Null);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                if (curBcsJobEqpFlag_TurnFlag == "0")
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO ({5})Job TurnFlag is not ON ,glass return to CST !",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
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

                #region [ Get  Check GotoStepID ]

                int CheckGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (CheckGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheck_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(CheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

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
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != CheckGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Aging Disable Mode Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = CheckGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID].Data.NEXTSTEPID;
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
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is TurnOn Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != CheckGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is  TurnOn Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = CheckGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(CheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                CheckGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID];

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

        //20170904 BY QIUMIN
        [UniAuto.UniBCS.OpiSpec.Help("JP0034")]
        public bool RouteStepJump_ForATSJobJumpOutCycle(IRobotContext robotConText)
        {

            string strlog = string.Empty;
            string errMsg = string.Empty;
            string curBcsJobEqpFlag_TurnFlag = string.Empty;
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


                #region [ Check Job TurnFlag]
                IDictionary<string, string> curBcsJobEqpFlag = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG, eJOBDATA.EQPFlag);
                curBcsJobEqpFlag_TurnFlag = curBcsJobEqpFlag["TurnFlag"];
                // 0: go to CST;1: go to T/T
                if (curBcsJobEqpFlag_TurnFlag == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobTurnFlag!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] can not Get JobTurnFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_curBcsJobTurnFlag_Is_Null);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                if (curBcsJobEqpFlag_TurnFlag == "1")
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNO ({5})Job TurnFlag is  ON ,glass go to TurnTable !",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
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

                #region [ Get  Check GotoStepID ]

                int CheckGoToStepID = (int)robotConText[eRobotContextParameter.RouteStepJumpGotoStepNo];

                if (CheckGoToStepID == 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But GotoStepID({5}) is Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode But GotoStepID({4}) is Fail!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheck_GotoStepNo_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                //Get Change StepID 後的NextStepNO
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(CheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) checkStepNo({4}) is TTP Daily Check But get GotoStepID({5}) Entity Fail!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode But get GotoStepID({4}) Entity Fail",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is  TurnOff Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curStepNo({3}) is Aging Disable Mode Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.CurStepNo != CheckGoToStepID ||
                        curBcsJob.RobotWIP.NextStepNo != curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID].Data.NEXTSTEPID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) curStepNo({4}) is Aging Disable Mode Jump Step to ({5}), NextStepNo({6}) to ({7})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString(), curBcsJob.RobotWIP.NextStepNo.ToString(),
                                                curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID].Data.NEXTSTEPID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.CurStepNo = CheckGoToStepID;
                            curBcsJob.RobotWIP.NextStepNo = curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID].Data.NEXTSTEPID;
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
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) NextStepNo({3}) is Aging Disable Mode Jump Step to ({4})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_TTPDailyCheckJumpNewStepID);
                    robotConText.SetReturnMessage(errMsg);

                    //有變化才記Log並存檔
                    if (curBcsJob.RobotWIP.NextStepNo != CheckGoToStepID)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) NextStepNo({4}) is Aging Disable Mode Jump Step to ({5})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curCheckRouteStep.Data.STEPID.ToString(), CheckGoToStepID.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        lock (curBcsJob)
                        {
                            curBcsJob.RobotWIP.NextStepNo = CheckGoToStepID;
                        }
                        //Save File
                        ObjectManager.JobManager.EnqueueSave(curBcsJob);
                    }
                    #endregion
                }

                #region [ Get Jump GoTo Step Entity ]
                if (curBcsJob.RobotWIP.RobotRouteStepList.ContainsKey(CheckGoToStepID) == false)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not get RouteStepEntity by JumpGotoSTEPID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                CheckGoToStepID.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] Job({1}_{2}) can not get RouteStepEntity by JumpGotoSTEPID({3})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            CheckGoToStepID.ToString());

                    robotConText.SetReturnCode(eJobRouteStepJump_ReturnCode.NG_Get_JumpGotoStepRoute_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region [ Get Jump GoTo Step Can Use StageList ]

                RobotRouteStep curJumpGotoRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[CheckGoToStepID];

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

        public string errCode { get; set; }
    }
}

#region [Job Judge]
//0：Inspection Skip or No Judge
//1：OK
//2：NG - Insp. Result 
//3：RW - Required Rework
//4：PD –Pending judge
//5：RP – Required Repair
//6：IR–Ink Repair
//7：Other
//8：RV –PI Reivew"
#endregion
#region [Unloading Port Mode]
//1：TFT Mode
//2：CF Mode
//3：Dummy Mode
//4：MQC Mode
//5：HT Mode
//6：LT Mode
//7：ENG Mode
//8：IGZO Mode
//9：ILC Mode
//10：FLC Mode
//11：Through Dummy Mode
//12：Thickness Dummy Mode
//13：UV Mask Mode
//14：By Grade Mode 
//15：OK Mode
//16：NG Mode
//17：MIX Mode
//18：EMP Mode (Empty Cassette)
//19：RW Mode
//20：Mismatch Mode
//21: PD Mode
//22: IR Mode
//23: RP Mode
//24: Re-Judge Mode
#endregion