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

namespace UniAuto.UniRCS.Core
{
    public partial class AbstractRobotService
    {
        #region [ Check Cell Special Bypass for SlotBlockInfo ]
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
        protected bool CheckSlotBlockInfo_AllRouteStepByPassConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, ref List<RobotStage> curBeforeFilterStageList)
        {
            IRobotContext _temp = null;
            return CheckSlotBlockInfo_AllRouteStepByPassConditionByStepNo(curRobot, curSlotBlockInfo, checkSlotBlockInfoStepNo, ref curBeforeFilterStageList, false, ref _temp);
        }
        protected bool CheckSlotBlockInfo_AllRouteStepByPassConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, ref List<RobotStage> curBeforeFilterStageList, bool _is2ndCmdFlag)
        {
            IRobotContext _temp = null;
            return CheckSlotBlockInfo_AllRouteStepByPassConditionByStepNo(curRobot, curSlotBlockInfo, checkSlotBlockInfoStepNo, ref curBeforeFilterStageList, _is2ndCmdFlag, ref _temp);
        }
        protected bool CheckSlotBlockInfo_AllRouteStepByPassConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, ref List<RobotStage> curBeforeFilterStageList, ref IRobotContext _robotContext)
        {
            return CheckSlotBlockInfo_AllRouteStepByPassConditionByStepNo(curRobot, curSlotBlockInfo, checkSlotBlockInfoStepNo, ref curBeforeFilterStageList, false, ref _robotContext);
        }
        protected bool CheckSlotBlockInfo_AllRouteStepByPassConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, ref List<RobotStage> curBeforeFilterStageList, bool _is2ndCmdFlag, ref IRobotContext _robotContext)
        {
            IRobotContext robotConText = null;
            string strlog = string.Empty;
            string failMsg = string.Empty;
            Job curBcsJob = null;
            string _szJobLog = string.Empty;

            string errMsg = string.Empty;
            string errCode = string.Empty;
            string fail_ReasonCode = string.Empty;

            try
            {
                if (curSlotBlockInfo.CurBlockCanControlJobList == null) return false;

                #region 没有可控基板block!!
                if (curSlotBlockInfo.CurBlockCanControlJobList.Count <= 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) SlotBlockInfo is empty.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }
                #endregion

                string _armSubLocation = string.Empty;
                Job curFrontJob = null;
                Job curBackJob = null;

                foreach (Job jobEntity in curSlotBlockInfo.CurBlockCanControlJobList)
                {
                    #region [ Get SlotBlockInfo裡面的Front/Back的 Job ]

                    if (jobEntity == null) continue;

                    _szJobLog = string.Format("Job Entity[CST={0}, Slot={1}]", jobEntity.CassetteSequenceNo.ToString(), jobEntity.JobSequenceNo.ToString());

                    switch (jobEntity.RobotWIP.CurSubLocation)
                    {
                        case eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION:
                            _armSubLocation = "Front";
                            curFrontJob = jobEntity;

                            break;
                        case eRobotCommonConst.ROBOT_ARM_BACK_LOCATION:
                            _armSubLocation = "Back";
                            curBackJob = jobEntity;
                            break;
                        default:
                            _armSubLocation = string.Empty;

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2}'s Sub Location[{3}] is invalid.",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    jobEntity.RobotWIP.CurSubLocation.ToString());
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            continue;
                    }

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Get {2} {3}'s information.",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _armSubLocation,
                                    _szJobLog);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    curBcsJob = jobEntity;

                    #endregion
                }

                #region [ Check Front and Back Job ByPass Condition ]
                robotConText = (_robotContext == null ? new RobotContext() : _robotContext);

                //先Check Front Job
                if (curFrontJob != null)
                {
                    if (!CheckAllRouteStepByPassCondition2(curRobot, curFrontJob, checkSlotBlockInfoStepNo, ref curBeforeFilterStageList, _is2ndCmdFlag, ref robotConText))
                    {
                        //有其中之一NG 則是為整個Block NG
                        return false;
                    }
                }

                //後Check Back Job
                if (curBackJob != null)
                {
                    if (!CheckAllRouteStepByPassCondition2(curRobot, curBackJob, checkSlotBlockInfoStepNo, ref curBeforeFilterStageList, _is2ndCmdFlag, ref robotConText))
                    {
                        //有其中之一NG 則是為整個Block NG
                        return false;
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
        #endregion
        #region [ Check Cell Special Jump for SlotBlockInfo ]
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
        protected bool CheckSlotBlockInfo_AllRouteStepJumpConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, ref List<RobotStage> curBeforeFilterStageList)
        {
            IRobotContext _temp = null;
            return CheckSlotBlockInfo_AllRouteStepJumpConditionByStepNo(curRobot, curSlotBlockInfo, checkSlotBlockInfoStepNo, ref curBeforeFilterStageList, false, ref _temp);
        }
        protected bool CheckSlotBlockInfo_AllRouteStepJumpConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, ref List<RobotStage> curBeforeFilterStageList, bool _is2ndCmdFlag)
        {
            IRobotContext _temp = null;
            return CheckSlotBlockInfo_AllRouteStepJumpConditionByStepNo(curRobot, curSlotBlockInfo, checkSlotBlockInfoStepNo, ref curBeforeFilterStageList, _is2ndCmdFlag, ref _temp);
        }
        protected bool CheckSlotBlockInfo_AllRouteStepJumpConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, ref List<RobotStage> curBeforeFilterStageList, ref IRobotContext _robotContext)
        {
            return CheckSlotBlockInfo_AllRouteStepJumpConditionByStepNo(curRobot, curSlotBlockInfo, checkSlotBlockInfoStepNo, ref curBeforeFilterStageList, false, ref _robotContext);
        }
        protected bool CheckSlotBlockInfo_AllRouteStepJumpConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, ref List<RobotStage> curBeforeFilterStageList, bool _is2ndCmdFlag, ref IRobotContext _robotContext)
        {
            IRobotContext robotConText = null;
            string strlog = string.Empty;
            string failMsg = string.Empty;
            Job curBcsJob = null;
            string _szJobLog = string.Empty;

            string errMsg = string.Empty;
            string errCode = string.Empty;
            string fail_ReasonCode = string.Empty;

            try
            {
                if (curSlotBlockInfo.CurBlockCanControlJobList == null) return false;

                #region 没有可控基板block!!
                if (curSlotBlockInfo.CurBlockCanControlJobList.Count <= 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) SlotBlockInfo is empty.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }
                #endregion

                string _armSubLocation = string.Empty;
                Job curFrontJob = null;
                Job curBackJob = null;

                foreach (Job jobEntity in curSlotBlockInfo.CurBlockCanControlJobList)
                {

                    #region [ Get SlotBlockInfo裡面的Front/Back的 Job ]

                    if (jobEntity == null) continue;

                    _szJobLog = string.Format("Job Entity[CST={0}, Slot={1}]", jobEntity.CassetteSequenceNo.ToString(), jobEntity.JobSequenceNo.ToString());

                    switch (jobEntity.RobotWIP.CurSubLocation)
                    {
                        case eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION:
                            _armSubLocation = "Front";
                            curFrontJob = jobEntity;

                            break;
                        case eRobotCommonConst.ROBOT_ARM_BACK_LOCATION:
                            _armSubLocation = "Back";
                            curBackJob = jobEntity;
                            break;
                        default:
                            _armSubLocation = string.Empty;

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2}'s Sub Location[{3}] is invalid.",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _szJobLog,
                                    jobEntity.RobotWIP.CurSubLocation.ToString());
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            continue;
                    }

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Get {2} {3}'s information.",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _armSubLocation,
                                    _szJobLog);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    curBcsJob = jobEntity;

                    #endregion

                }

                #region [ Check Front and Back Job Jump Condition ]
                robotConText = (_robotContext == null ? new RobotContext() : _robotContext);

                //先Check Front Job
                if (curFrontJob != null)
                {
                    if (!CheckAllRouteStepJumpCondition2(curRobot, curFrontJob, checkSlotBlockInfoStepNo, ref curBeforeFilterStageList, _is2ndCmdFlag, ref robotConText))
                    {
                        //有其中之一NG 則是為整個Block NG
                        return false;
                    }
                }

                //後Check Back Job
                if (curBackJob != null)
                {
                    if (!CheckAllRouteStepJumpCondition2(curRobot, curBackJob, checkSlotBlockInfoStepNo, ref curBeforeFilterStageList, _is2ndCmdFlag, ref robotConText))
                    {
                        //有其中之一NG 則是為整個Block NG
                        return false;
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
        #endregion
        #region [ Check Cell Special Filter for SlotBlockInfo ]
        //20151229 add
        /// <summary> Check SlotBlockInfo All Filter Condition. 目前SlotBlock不允許Front/Back 不同Route , Step , Action , UseArm.需要都相同才可以處理
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curSlotBlockInfo"></param>
        /// <param name="checkSlotBlockInfoStepNo"></param>
        /// <param name="cur1stDefineCmd"></param>
        /// <param name="cur2ndDefineCmd"></param>
        /// <param name="curFilterStageList"></param>
        /// <param name="_is2ndCmdFlag"></param>
        /// <param name="_robotContext"></param>
        /// <returns></returns>
        protected bool CheckSlotBlockInfo_AllFilterConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefineCmd, ref List<RobotStage> curFilterStageList)
        {
            IRobotContext _temp = null;
            return CheckSlotBlockInfo_AllFilterConditionByStepNo(curRobot, curSlotBlockInfo, checkSlotBlockInfoStepNo, cur1stDefineCmd, cur2ndDefineCmd, ref curFilterStageList, false, ref _temp);
        }
        protected bool CheckSlotBlockInfo_AllFilterConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefineCmd, ref List<RobotStage> curFilterStageList, bool _is2ndCmdFlag)
        {
            IRobotContext _temp = null;
            return CheckSlotBlockInfo_AllFilterConditionByStepNo(curRobot, curSlotBlockInfo, checkSlotBlockInfoStepNo, cur1stDefineCmd, cur2ndDefineCmd, ref curFilterStageList, _is2ndCmdFlag, ref _temp);
        }
        protected bool CheckSlotBlockInfo_AllFilterConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefineCmd, ref List<RobotStage> curFilterStageList, ref IRobotContext _robotContext)
        {
            return CheckSlotBlockInfo_AllFilterConditionByStepNo(curRobot, curSlotBlockInfo, checkSlotBlockInfoStepNo, cur1stDefineCmd, cur2ndDefineCmd, ref curFilterStageList, false, ref _robotContext);
        }
        protected bool CheckSlotBlockInfo_AllFilterConditionByStepNo(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, int checkSlotBlockInfoStepNo, DefineNormalRobotCmd cur1stDefineCmd, DefineNormalRobotCmd cur2ndDefineCmd, ref List<RobotStage> curFilterStageList, bool _is2ndCmdFlag, ref IRobotContext _robotContext)
        {
            IRobotContext robotConText = null;
            string strlog = string.Empty;
            string failMsg = string.Empty;
            Job curBcsJob = null;
            string _szJobLog = string.Empty;

            DefineNormalRobotCmd curFrontJob1stDefineCmd = new DefineNormalRobotCmd();
            DefineNormalRobotCmd curFrontJob2ndDefineCmd = new DefineNormalRobotCmd();
            DefineNormalRobotCmd curBackJob1stDefineCmd = new DefineNormalRobotCmd();
            DefineNormalRobotCmd curBackJob2ndDefineCmd = new DefineNormalRobotCmd();

            string errMsg = string.Empty;
            string errCode = string.Empty;
            string fail_ReasonCode = string.Empty;

            try
            {
                if (curSlotBlockInfo.CurBlockCanControlJobList == null) return false;

                #region 没有可控基板block!!
                if (curSlotBlockInfo.CurBlockCanControlJobList.Count <= 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) SlotBlockInfo is empty.", curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }
                #endregion

                string _armSubLocation = string.Empty;
                Job curFrontJob = null;
                Job curBackJob = null;

                foreach (Job jobEntity in curSlotBlockInfo.CurBlockCanControlJobList)
                {

                    #region [ Get SlotBlockInfo裡面的Front/Back的 Job ]

                    if (jobEntity == null) continue;

                    _szJobLog = string.Format("Job Entity[CST={0}, Slot={1}]", jobEntity.CassetteSequenceNo.ToString(), jobEntity.JobSequenceNo.ToString());

                    switch (jobEntity.RobotWIP.CurSubLocation)
                    {
                    case eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION:
                        _armSubLocation = "Front";
                        curFrontJob = jobEntity;

                        break;
                    case eRobotCommonConst.ROBOT_ARM_BACK_LOCATION:
                        _armSubLocation = "Back";
                        curBackJob = jobEntity;
                        break;
                    default:
                        _armSubLocation = string.Empty;

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) {2}'s Sub Location[{3}] is invalid.",
                                curRobot.Data.NODENO,
                                curRobot.Data.ROBOTNAME,
                                _szJobLog,
                                jobEntity.RobotWIP.CurSubLocation.ToString());
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        continue;
                    }

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Get {2} {3}'s information.",
                                    curRobot.Data.NODENO,
                                    curRobot.Data.ROBOTNAME,
                                    _armSubLocation,
                                    _szJobLog);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    curBcsJob = jobEntity;

                    #endregion

                }

                if (curFrontJob != null && curBackJob != null)
                {
                    //20160219 add for Front/Back afterFilter target StageList
                    List<RobotStage> curFrontFilterStageList = new List<UniBCS.Entity.RobotStage>();
                    List<RobotStage> curBackFilterStageList = new List<UniBCS.Entity.RobotStage>();
                    List<RobotStage> curJudgeFilterStageList = new List<UniBCS.Entity.RobotStage>();

                    curFrontFilterStageList = curFilterStageList;
                    curBackFilterStageList = curFilterStageList;

                    #region [ 要Check Front and Back Job Filter Condition ]

                    //先Check Front Job
                    robotConText = (_robotContext == null ? new RobotContext() : _robotContext);

                    //20160219 modify for Front/Back afterFilter target StageList
                    //if (!CheckAllFilterConditionByStepNo2(curRobot, curFrontJob, checkSlotBlockInfoStepNo, curFrontJob1stDefineCmd, curFrontJob2ndDefineCmd, ref curFilterStageList, _is2ndCmdFlag, ref robotConText))
                    if (!CheckAllFilterConditionByStepNo2(curRobot, curFrontJob, checkSlotBlockInfoStepNo, curFrontJob1stDefineCmd, curFrontJob2ndDefineCmd, ref curFrontFilterStageList, _is2ndCmdFlag, ref robotConText))
                    {
                        //20160126 add Set Wait CST Event
                        Cell_SendWaitCassetteStatusToBCS(MethodBase.GetCurrentMethod().Name, curRobot, curFrontJob, false);

                        //有其中之一NG 則是為整個Block NG
                        return false;

                    }

                    //後Check Back Job
                    //20160219 modify for Front/Back afterFilter target StageList
                    //if (!CheckAllFilterConditionByStepNo2(curRobot, curBackJob, checkSlotBlockInfoStepNo, curBackJob1stDefineCmd, curBackJob2ndDefineCmd, ref curFilterStageList, _is2ndCmdFlag, ref robotConText))
                    if (!CheckAllFilterConditionByStepNo2(curRobot, curBackJob, checkSlotBlockInfoStepNo, curBackJob1stDefineCmd, curBackJob2ndDefineCmd, ref curBackFilterStageList, _is2ndCmdFlag, ref robotConText))
                    {
                        //20160126 add Set Wait CST Event
                        Cell_SendWaitCassetteStatusToBCS(MethodBase.GetCurrentMethod().Name, curRobot, curBackJob, false);

                        //有其中之一NG 則是為整個Block NG
                        return false;

                    }

                    //20160126 add 當Front and BackJob 都通過Filter時已FrontJob來通知BCS Clear Wait CST Event
                    Cell_SendWaitCassetteStatusToBCS(MethodBase.GetCurrentMethod().Name, curRobot, curFrontJob, true);
                    
                    #region [ 20160219 add for Judge Front/Back afterFilter target StageList ]

                    if(curFrontFilterStageList.Count > 0 && curBackFilterStageList.Count > 0)
                    {

                        for (int frontIdx = 0; frontIdx < curFrontFilterStageList.Count; frontIdx++)
                        {
                            for (int backIdx = 0; backIdx < curBackFilterStageList.Count; backIdx++)
                            {

                                if (curFrontFilterStageList[frontIdx].Data.STAGEID == curBackFilterStageList[backIdx].Data.STAGEID)
                                {
                                    if (curJudgeFilterStageList.Contains(curFrontFilterStageList[frontIdx]) == false)
                                    {
                                        curJudgeFilterStageList.Add(curFrontFilterStageList[frontIdx]);
                                    }

                                }
                            
                            }

                        }

                    }

                    curFilterStageList = curJudgeFilterStageList;

                    if (curFilterStageList.Count < 1)
                    {
                        //前後片Target不同時 則視為整個Block NG
                        return false;
                    }

                    #endregion


                    #endregion

                    #region [ Judge 1st and 2nd Command Action ]

                    //Judge Front/Back 1st Cmd Action
                    if (curFrontJob1stDefineCmd.Cmd01_Command != curBackJob1stDefineCmd.Cmd01_Command)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) 1stCmd Action({4}) but BackJob({5},{6}) 1stCmd Action({7}) is different!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    curFrontJob1stDefineCmd.Cmd01_Command, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, curBackJob1stDefineCmd.Cmd01_Command);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    //Judge Front/Back 2nd Cmd Action
                    if (curFrontJob2ndDefineCmd.Cmd01_Command != curBackJob2ndDefineCmd.Cmd01_Command)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) 2ndCmd Action({4}) but BackJob({5},{6}) 2ndCmd Action({7}) is different!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    curFrontJob2ndDefineCmd.Cmd01_Command, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, curBackJob2ndDefineCmd.Cmd01_Command);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        return false;
                    }

                    #endregion

                    #region [ Set CurSlotBlockInfo 1st and 2nd Cmd Action ]

                    cur1stDefineCmd.Cmd01_Command = curFrontJob1stDefineCmd.Cmd01_Command;
                    cur2ndDefineCmd.Cmd01_Command = curFrontJob2ndDefineCmd.Cmd01_Command;

                    #endregion

                    #region [ Check Use Arm ]

                    //20160113 add Use FrontJob curStepNo
                    fail_ReasonCode = string.Format("{0}_{1}_{2}", "JobFilterService", "Filter_CurStepCellSpecialUseArmByJobLocation", curFrontJob.RobotWIP.CurStepNo.ToString());

                    if (Filter_CurStepCellSpecialUseArmByJobLocation(curRobot, curSlotBlockInfo, cur1stDefineCmd, cur2ndDefineCmd, false, out errMsg, out errCode) == false)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #region [ Add To Check Fail Message To FrontJob ]

                        if (!curFrontJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("Robot({0}) FrontJob({1},{2}) BackJob({3},{4}) MethodName({5}) Fail! FailCode({6}) FailMsg({7})!",
                            //                        curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                            //                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                            failMsg = string.Format("RtnCode({0}) RtnMsg({1})!",errCode, errMsg);

                            AddJobCheckFailMsg(curFrontJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion
                        }
                        #endregion

                        #region [ Add To Check Fail Message To BackJob ]

                        if (!curBackJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("Robot({0}) FrontJob({1},{2}) BackJob({3},{4}) MethodName({5}) Fail! FailCode({6}) FailMsg({7})!",
                            //                        curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                            //                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                            failMsg = string.Format("RtnCode({0}) RtnMsg({1})!",errCode, errMsg);

                            AddJobCheckFailMsg(curBackJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion
                        }

                        #endregion

                        return false;
                    }
                    else
                    {
                        RemoveJobCheckFailMsg(curFrontJob, fail_ReasonCode);
                        RemoveJobCheckFailMsg(curBackJob, fail_ReasonCode);
                    }

                    #endregion

                    #region [ Check 2nd Use Arm ]

                    //要有2nd Cmd 才需要Check 2nd UseArm
                    if (cur2ndDefineCmd.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    {
                        //20160113 add Use FrontJob NextStepNo
                        fail_ReasonCode = string.Format("{0}_{1}_{2}", "JobFilterService", "Filter_CurStepCellSpecialUseArmByJobLocation", curFrontJob.RobotWIP.NextStepNo.ToString());

                        if (Filter_CurStepCellSpecialUseArmByJobLocation(curRobot, curSlotBlockInfo, cur1stDefineCmd, cur2ndDefineCmd, true, out errMsg, out errCode) == false)
                        {

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add To Check Fail Message To FrontJob ]

                            if (!curFrontJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("Robot({0}) FrontJob({1},{2}) BackJob({3},{4}) MethodName({5}) Fail! FailCode({6}) FailMsg({7})!",
                                //                        curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                //                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                                failMsg = string.Format("FrontJob({0},{1}) BackJob({2},{3}) MethodName({4}) Fail! FailCode({5}) FailMsg({6})!",
                                                        curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                                AddJobCheckFailMsg(curFrontJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion
                            }
                            #endregion

                            #region [ Add To Check Fail Message To BackJob ]

                            if (!curBackJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("Robot({0}) FrontJob({1},{2}) BackJob({3},{4}) MethodName({5}) Fail! FailCode({6}) FailMsg({7})!",
                                //                        curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                //                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                                failMsg = string.Format("FrontJob({0},{1}) BackJob({2},{3}) MethodName({4}) Fail! FailCode({5}) FailMsg({6})!",
                                                        curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                                AddJobCheckFailMsg(curBackJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion
                            }

                            #endregion

                            return false;
                        }
                        else
                        {
                            RemoveJobCheckFailMsg(curFrontJob, fail_ReasonCode);
                            RemoveJobCheckFailMsg(curBackJob, fail_ReasonCode);
                        }

                    }

                    #endregion


                }
                else if (curFrontJob != null && curBackJob == null)
                {

                    #region [ 要Check Front Job Filter Condition ]

                    if (!CheckAllFilterConditionByStepNo2(curRobot, curFrontJob, checkSlotBlockInfoStepNo, cur1stDefineCmd, cur2ndDefineCmd, ref curFilterStageList, _is2ndCmdFlag, ref robotConText))
                    {
                        //20160126 add Set Wait CST Event
                        Cell_SendWaitCassetteStatusToBCS(MethodBase.GetCurrentMethod().Name, curRobot, curFrontJob, false);

                        //有其中之一NG 則是為整個Block NG
                        return false;

                    }

                    //20160126 add Clear Wait CST Event
                    Cell_SendWaitCassetteStatusToBCS(MethodBase.GetCurrentMethod().Name, curRobot, curFrontJob, true);

                    #endregion

                    #region [ Check Use Arm ]

                    //20160113 add Use FrontJob CurStepNo
                    fail_ReasonCode = string.Format("{0}_{1}_{2}", "JobFilterService", "Filter_CurStepCellSpecialUseArmByJobLocation", curFrontJob.RobotWIP.CurStepNo.ToString());

                    if (Filter_CurStepCellSpecialUseArmByJobLocation(curRobot, curSlotBlockInfo, cur1stDefineCmd, cur2ndDefineCmd, false, out errMsg, out errCode) == false)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    "0", "0", "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #region [ Add To Check Fail Message curFrontJob ]

                        if (!curFrontJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    "0", "0", "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("Robot({0}) FrontJob({1},{2}) BackJob({3},{4}) MethodName({5}) Fail! FailCode({6}) FailMsg({7})!",
                            //                        curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                            //                        "0", "0", "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                            failMsg = string.Format("FrontJob({0},{1}) BackJob({2},{3}) MethodName({4}) Fail! FailCode({5}) FailMsg({6})!",
                                                    curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    "0", "0", "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                            AddJobCheckFailMsg(curFrontJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion
                        }

                        #endregion

                        return false;
                    }
                    else
                    {
                        RemoveJobCheckFailMsg(curFrontJob, fail_ReasonCode);
                    }

                    #endregion

                    #region [ Check 2nd Use Arm ]

                    //要有2nd Cmd 才需要Check 2nd UseArm
                    if (cur2ndDefineCmd.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    {
                        //20160113 add Use FrontJob NextStepNo
                        fail_ReasonCode = string.Format("{0}_{1}_{2}", "JobFilterService", "Filter_CurStepCellSpecialUseArmByJobLocation", curFrontJob.RobotWIP.NextStepNo.ToString());

                        if (Filter_CurStepCellSpecialUseArmByJobLocation(curRobot, curSlotBlockInfo, cur1stDefineCmd, cur2ndDefineCmd, true, out errMsg, out errCode) == false)
                        {

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                        "0", "0", "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            #region [ Add To Check Fail Message To FrontJob ]

                            if (!curFrontJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                        "0", "0", "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("Robot({0}) FrontJob({1},{2}) BackJob({3},{4}) MethodName({5}) Fail! FailCode({6}) FailMsg({7})!",
                                //                        curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                //                        "0", "0", "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                                failMsg = string.Format("FrontJob({0},{1}) BackJob({2},{3}) MethodName({4}) Fail! FailCode({5}) FailMsg({6})!",
                                                        curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                        "0", "0", "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                                AddJobCheckFailMsg(curFrontJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion
                            }
                            #endregion

                            return false;
                        }
                        else
                        {
                            RemoveJobCheckFailMsg(curFrontJob, fail_ReasonCode);
                        }

                    }

                    #endregion

                }
                else if (curFrontJob == null && curBackJob != null)
                {

                    #region [ 要Check Back Job Filter Condition ]

                    if (!CheckAllFilterConditionByStepNo2(curRobot, curBackJob, checkSlotBlockInfoStepNo, cur1stDefineCmd, cur2ndDefineCmd, ref curFilterStageList, _is2ndCmdFlag, ref robotConText))
                    {
                        //20160126 add Set Wait CST Event
                        Cell_SendWaitCassetteStatusToBCS(MethodBase.GetCurrentMethod().Name, curRobot, curBackJob, false);

                        //有其中之一NG 則是為整個Block NG
                        return false;

                    }

                    //20160126 add Clear Wait CST Event
                    Cell_SendWaitCassetteStatusToBCS(MethodBase.GetCurrentMethod().Name, curRobot, curBackJob, true);

                    #endregion

                    #region [ Check Use Arm ]

                    //20160113 add Use BackJob CurStepNo
                    fail_ReasonCode = string.Format("{0}_{1}_{2}", "JobFilterService", "Filter_CurStepCellSpecialUseArmByJobLocation", curBackJob.RobotWIP.CurStepNo.ToString());

                    if (Filter_CurStepCellSpecialUseArmByJobLocation(curRobot, curSlotBlockInfo, cur1stDefineCmd, cur2ndDefineCmd, false, out errMsg, out errCode) == false)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, "0", "0",
                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        #region [ Add To Check Fail Message curFrontJob ]

                        if (!curBackJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, "0", "0",
                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //failMsg = string.Format("Robot({0}) FrontJob({1},{2}) BackJob({3},{4}) MethodName({5}) Fail! FailCode({6}) FailMsg({7})!",
                            //                        curRobot.Data.ROBOTNAME, "0", "0",
                            //                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                            failMsg = string.Format("FrontJob({0},{1}) BackJob({2},{3}) MethodName({4}) Fail! FailCode({5}) FailMsg({6})!",
                                                    "0", "0",
                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                            AddJobCheckFailMsg(curBackJob, fail_ReasonCode, failMsg);
                            SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                            #endregion
                        }

                        #endregion

                        return false;
                    }
                    else
                    {
                        RemoveJobCheckFailMsg(curBackJob, fail_ReasonCode);
                    }

                    #endregion

                    #region [ Check 2nd Use Arm ]

                    //要有2nd Cmd 才需要Check 2nd UseArm
                    if (cur2ndDefineCmd.Cmd01_Command != eRobot_Trx_CommandAction.ACTION_NONE)
                    {
                        //20160113 add Use BackJob NextStepNo
                        fail_ReasonCode = string.Format("{0}_{1}_{2}", "JobFilterService", "Filter_CurStepCellSpecialUseArmByJobLocation", curBackJob.RobotWIP.NextStepNo.ToString());

                        if (Filter_CurStepCellSpecialUseArmByJobLocation(curRobot, curSlotBlockInfo, cur1stDefineCmd, cur2ndDefineCmd, true, out errMsg, out errCode) == false)
                        {

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, "0", "0",
                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion                

                            #region [ Add To Check Fail Message To BackJob ]

                            if (!curBackJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) FrontJob({2},{3}) BackJob({4},{5}) MethodName({6}) Fail! FailCode({7}) FailMsg({8})!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, "0", "0",
                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                //failMsg = string.Format("Robot({0}) FrontJob({1},{2}) BackJob({3},{4}) MethodName({5}) Fail! FailCode({6}) FailMsg({7})!",
                                //                        curRobot.Data.ROBOTNAME, "0", "0",
                                //                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                                failMsg = string.Format("FrontJob({0},{1}) BackJob({2},{3}) MethodName({4}) Fail! FailCode({5}) FailMsg({6})!",
                                                        "0", "0",
                                                        curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, "Filter_CurStepCellSpecialUseArmByJobLocation", errCode, errMsg);

                                AddJobCheckFailMsg(curBackJob, fail_ReasonCode, failMsg);
                                SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                #endregion
                            }

                            #endregion

                            return false;
                        }
                        else
                        {
                            RemoveJobCheckFailMsg(curBackJob, fail_ReasonCode);
                        }

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

        private bool Filter_CurStepCellSpecialUseArmByJobLocation(Robot curRobot, RobotCanControlSlotBlockInfo curSlotBlockInfo, DefineNormalRobotCmd cur1stRobotCmd, DefineNormalRobotCmd cur2ndRobotCmd, bool is2ndCmdFlag, out string errMsg, out string errCode)
        {
            string strlog = string.Empty;
            errMsg = string.Empty;
            errCode = string.Empty;

            try
            {

                #region [ Get curSlotBlockInfo FrontJob and BackJob Entity ]

                if (curSlotBlockInfo.CurBlockCanControlJobList.Count == 0)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get curSlotBlockInfo JobInfo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get curSlotBlockInfo JobInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    errCode = eJobFilter_ReturnCode.NG_SlotBlockInfo_Job_Is_Null;

                    return false;
                }

                Job curFrontJob = null;
                Job curBackJob = null;

                foreach (Job jobEntity in curSlotBlockInfo.CurBlockCanControlJobList)
                {
                    if (jobEntity.RobotWIP.CurSubLocation == eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION)
                    {
                        curFrontJob = jobEntity;

                    }
                    if (jobEntity.RobotWIP.CurSubLocation == eRobotCommonConst.ROBOT_ARM_BACK_LOCATION)
                    {
                        curBackJob = jobEntity;

                    }
                }

                if (curFrontJob == null && curBackJob == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get curSlotBlockInfo Front and Back jobInfo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get curSlotBlockInfo  Front and Back jobInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    errCode = eJobFilter_ReturnCode.NG_SlotBlockInfo_Job_Is_Null;

                    return false;

                }

                #endregion

                #region [ Get Defind 1st NormalRobotCommand ]

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

                    errCode = eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail;

                    return false;
                }

                #endregion

                #region [ Get Defind 2nd NormalRobotCommand ]

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

                    errCode = eJobFilter_ReturnCode.NG_Get_2nd_DefineNormalRobotCommandInfo_Is_Fail;

                    return false;
                }

                #endregion

                #region [ Check UseArm by is2ndCmdFlag(根據是CurStep or NextStep來確認Arm是否可以使用 ]

                //DB定義 //'UP':Upper Arm, 'LOW':Lower Arm, 'ANY':Any Arm, 'ALL':All Arm
                int tmpFrontStepNo = 0;
                string tmpFrontStageID = string.Empty;
                int tmpFrontLocation_SlotNo = 0;
                string tmpFrontSubLocation = string.Empty;
                string tmpFrontUseArm = string.Empty;

                int tmpBackStepNo = 0;
                string tmpBackStageID = string.Empty;
                int tmpBackLocation_SlotNo = 0;
                string tmpBackSubLocation = string.Empty;
                string tmpBackUseArm = string.Empty;

                //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                string tmpFront_CstSeq = "0";
                string tmpFront_JobSeq = "0";
                string tmpBack_CstSeq = "0";
                string tmpBack_JobSeq = "0";

                #region [ 取得目前Robot Arm上經過運算的資訊 ]

                RobotArmDoubleSubstrateInfo[] tmpRobotArmInfo = new RobotArmDoubleSubstrateInfo[curRobot.CurTempArmDoubleJobInfoList.Length];

                string funcName = string.Empty;

                //先與目前Arm上運算的資訊做同步
                for (int i = 0; i < tmpRobotArmInfo.Length; i++)
                {
                    tmpRobotArmInfo[i] = new RobotArmDoubleSubstrateInfo();
                    //與Robot同步 
                    tmpRobotArmInfo[i].ArmFrontJobExist = curRobot.CurTempArmDoubleJobInfoList[i].ArmFrontJobExist;
                    tmpRobotArmInfo[i].ArmBackJobExist = curRobot.CurTempArmDoubleJobInfoList[i].ArmBackJobExist;
                    tmpRobotArmInfo[i].ArmDisableFlag = curRobot.CurTempArmDoubleJobInfoList[i].ArmDisableFlag;
                    //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                    tmpRobotArmInfo[i].ArmFrontCSTSeq = curRobot.CurTempArmDoubleJobInfoList[i].ArmFrontCSTSeq;
                    tmpRobotArmInfo[i].ArmFrontJobSeq = curRobot.CurTempArmDoubleJobInfoList[i].ArmFrontJobSeq;
                    tmpRobotArmInfo[i].ArmBackCSTSeq = curRobot.CurTempArmDoubleJobInfoList[i].ArmBackCSTSeq;
                    tmpRobotArmInfo[i].ArmBackJobSeq = curRobot.CurTempArmDoubleJobInfoList[i].ArmBackJobSeq;
                }

                #endregion

                #region [ Get Front/Back Job Step Check Parameter by is2ndCmdFlag ]

                if (is2ndCmdFlag == false)
                {

                    #region [ Is 1st Cmd Check ]

                    funcName = eRobotCommonConst.LOG_Check_1stCmd_Desc;

                    //根據Cur StepNo 定義目前Front/Back Job的位置
                    if (curFrontJob != null && curBackJob != null)
                    {
                        tmpFrontStepNo = curFrontJob.RobotWIP.CurStepNo;
                        tmpFrontStageID = curFrontJob.RobotWIP.CurLocation_StageID;
                        tmpFrontLocation_SlotNo = curFrontJob.RobotWIP.CurLocation_SlotNo;
                        tmpFrontSubLocation = curFrontJob.RobotWIP.CurSubLocation;
              

                        tmpBackStepNo = curBackJob.RobotWIP.CurStepNo;
                        tmpBackStageID = curBackJob.RobotWIP.CurLocation_StageID;
                        tmpBackLocation_SlotNo = curBackJob.RobotWIP.CurLocation_SlotNo;
                        tmpBackSubLocation = curBackJob.RobotWIP.CurSubLocation;
                        

                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                        tmpFront_CstSeq = curFrontJob.CassetteSequenceNo;
                        tmpFront_JobSeq = curFrontJob.JobSequenceNo;
                        tmpBack_CstSeq = curBackJob.CassetteSequenceNo;
                        tmpBack_JobSeq = curBackJob.JobSequenceNo;

                    }
                    else if (curFrontJob != null && curBackJob == null)
                    {
                        //Only Front
                        tmpFrontStepNo = curFrontJob.RobotWIP.CurStepNo;
                        tmpFrontStageID = curFrontJob.RobotWIP.CurLocation_StageID;
                        tmpFrontLocation_SlotNo = curFrontJob.RobotWIP.CurLocation_SlotNo;
                        tmpFrontSubLocation = curFrontJob.RobotWIP.CurSubLocation;

                        //tmpBackStepNo = tmpFrontStepNo;
                        tmpBackStageID = tmpFrontStageID;
                        //tmpBackLocation_SlotNo = tmpFrontLocation_SlotNo;
                        //tmpBackSubLocation = curBackJob.RobotWIP.CurSubLocation;
                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                        tmpFront_CstSeq = curFrontJob.CassetteSequenceNo;
                        tmpFront_JobSeq = curFrontJob.JobSequenceNo;

                    }
                    else if (curFrontJob == null && curBackJob != null)
                    {
                        //Only Back
                        tmpBackStepNo = curBackJob.RobotWIP.CurStepNo;
                        tmpBackStageID = curBackJob.RobotWIP.CurLocation_StageID;
                        tmpBackLocation_SlotNo = curBackJob.RobotWIP.CurLocation_SlotNo;
                        tmpBackSubLocation = curBackJob.RobotWIP.CurSubLocation;

                        //tmpFrontStepNo = curFrontJob.RobotWIP.CurStepNo;
                        tmpFrontStageID = tmpBackStageID;
                        //tmpFrontLocation_SlotNo = curFrontJob.RobotWIP.CurLocation_SlotNo;
                        //tmpFrontSubLocation = curFrontJob.RobotWIP.CurSubLocation;
                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                        tmpBack_CstSeq = curBackJob.CassetteSequenceNo;
                        tmpBack_JobSeq = curBackJob.JobSequenceNo;

                    }

                    #endregion

                }
                else
                {

                    #region [ Is 1st Cmd Check. 根據Next StepNo 預測目前1st Cmd後front/Back Job的位置 tmpFrontLocation_SlotNo(1,3,5,7) and tmpBackLocation_SlotNo(2,4,6,8) ]

                    funcName = eRobotCommonConst.LOG_Check_2ndCmd_Desc;

                    if (curFrontJob != null)
                    {
                        tmpFrontStepNo = curFrontJob.RobotWIP.NextStepNo;
                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                        tmpFront_CstSeq = curFrontJob.CassetteSequenceNo;
                        tmpFront_JobSeq = curFrontJob.JobSequenceNo;
                    }

                    if (curBackJob != null)
                    {
                        tmpBackStepNo = curBackJob.RobotWIP.NextStepNo;
                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                        tmpBack_CstSeq = curBackJob.CassetteSequenceNo;
                        tmpBack_JobSeq = curBackJob.JobSequenceNo;
                    }

                    #region [ by 1st Cmd Define Job Location(curStageID) and ArmInfo(robotArmInfo[4]) ]

                    //SPEC定義1Arm 2Job要
                    //0: None      //1: Put          //2: Get 
                    //4: Exchange  //8: Put Ready    //16: Get Ready       //32: Get/Put                 
                    switch (cur1stRobotCmd.Cmd01_Command)
                    {
                        case 1:  //PUT
                        case 4:  //Exchange
                        case 32: //Get/Put

                            #region [ 1st Cmd 是PUT相關則要清空相對應的Arm資訊 ]

                            //有可能只有Front or Back Job 所以只要其中一片存在則同步更新
                            if (curFrontJob != null || curBackJob != null)
                            {
                                //Local Stage is Stage
                                tmpFrontStageID = cur1stRobotCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0');
                                tmpBackStageID = cur1stRobotCmd.Cmd01_TargetPosition.ToString().PadLeft(2, '0'); ;
                            }

                            #region [ by 1st Cmd Use Arm UpDate ArmInfo and tmpLocation_SlotNo ]

                            //SPEC定義1Arm 2Job
                            //0: None              1: Upper/Left Arm       2: Lower/Left Arm     3: Left Both Arm
                            //4: Upper/Right Arm   5: Upper Both Arm       8: Lower/Right Arm   10: Lower Both Arm
                            //12: Right Both Arm
                            switch (cur1stRobotCmd.Cmd01_ArmSelect)
                            {
                                case 1: //Upper/Left Arm  Arm#01

                                    //1st Cmd 是PUT相關則 Arm前後通通要淨空
                                    tmpRobotArmInfo[0].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[0].ArmBackJobExist = eGlassExist.NoExist;

                                    break;

                                case 2: //Lower/Left Arm  Arm#02

                                    //1st Cmd 是PUT相關則 Arm前後通通要淨空
                                    tmpRobotArmInfo[1].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[1].ArmBackJobExist = eGlassExist.NoExist;

                                    break;

                                case 3: //Left Both Arm  Arm#01 & Arm#02


                                    tmpRobotArmInfo[0].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[1].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[0].ArmBackJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[1].ArmBackJobExist = eGlassExist.NoExist;

                                    break;

                                case 4: //Upper/Right Arm  Arm#03

                                    //1st Cmd 是PUT相關則 Arm前後通通要淨空
                                    tmpRobotArmInfo[2].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[2].ArmBackJobExist = eGlassExist.NoExist;

                                    break;

                                case 5: //Upper Both Arm  Arm#01 & Arm#03

                                    //1st Cmd 是PUT相關則 Arm前後通通要淨空
                                    tmpRobotArmInfo[0].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[2].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[0].ArmBackJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[2].ArmBackJobExist = eGlassExist.NoExist;

                                    break;

                                case 8: //Lower/Right Arm Arm#04

                                    //1st Cmd 是PUT相關則 Arm前後通通要淨空
                                    tmpRobotArmInfo[3].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[3].ArmBackJobExist = eGlassExist.NoExist;

                                    break;

                                case 10: //Lower Both Arm  Arm#02 & Arm#04

                                    //1st Cmd 是PUT相關則 Arm前後通通要淨空
                                    tmpRobotArmInfo[0].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[2].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[0].ArmBackJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[2].ArmBackJobExist = eGlassExist.NoExist;

                                    break;

                                case 12: //Right Both Arm  Arm#03 & Arm#04

                                    //1st Cmd 是PUT相關則 Arm前後通通要淨空
                                    tmpRobotArmInfo[2].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[3].ArmFrontJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[2].ArmBackJobExist = eGlassExist.NoExist;
                                    tmpRobotArmInfo[3].ArmBackJobExist = eGlassExist.NoExist;

                                    break;

                                default:

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {
                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) 1st defineNormalRobotCmd Action({3}) but UseArm({4}) is out of Range!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] can not Get 1st defineNormalRobotCmd Action({1}) but UseArm({2}) is out of Range!",
                                                            MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

                                    errCode = eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail;

                                    return false;

                            }

                            #endregion

                            if (curFrontJob != null)
                            {
                                tmpFrontLocation_SlotNo = cur1stRobotCmd.Cmd01_TargetSlotNo;
                                //Sub Loction 還是出片前的位置
                                tmpFrontSubLocation = curFrontJob.RobotWIP.CurSubLocation;
                            }

                            if (curBackJob != null)
                            {
                                tmpBackLocation_SlotNo = cur1stRobotCmd.Cmd01_TargetSlotNo + 1;
                                //Sub Loction 還是出片前的位置
                                tmpBackSubLocation = curBackJob.RobotWIP.CurSubLocation;
                            }

                            #endregion

                            break;

                        case 2:  //Get
                        case 8:  //Put Ready
                        case 16: //Get Ready

                            #region [ 1st Cmd 是Get相關則要更新相對應的Arm資訊 ]

                            //Local Stage is Stage
                            tmpFrontStageID = eRobotCommonConst.ROBOT_HOME_STAGEID;
                            tmpBackStageID = eRobotCommonConst.ROBOT_HOME_STAGEID;

                            #region [ by 1st Cmd Use Arm UpDate ArmInfo ]

                            //SPEC定義1Arm 2Job
                            //0: None              1: Upper/Left Arm       2: Lower/Left Arm     3: Left Both Arm
                            //4: Upper/Right Arm   5: Upper Both Arm       8: Lower/Right Arm   10: Lower Both Arm
                            //12: Right Both Arm
                            switch (cur1stRobotCmd.Cmd01_ArmSelect)
                            {
                                case 1: //Upper/Left Arm  Arm#01

                                    //根據Job 前後位置來決定更新Arm運算資訊
                                    if (curFrontJob != null)
                                    {
                                        tmpRobotArmInfo[0].ArmFrontJobExist = eGlassExist.Exist;
                                        tmpFrontLocation_SlotNo = eCellSpecialArmSlotNo.UpperLeft_Front; //Arm#01 Front

                                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                                        tmpRobotArmInfo[0].ArmFrontCSTSeq = curFrontJob.CassetteSequenceNo;
                                        tmpRobotArmInfo[0].ArmFrontJobSeq = curFrontJob.JobSequenceNo;

                                    }


                                    if (curBackJob != null)
                                    {
                                        tmpRobotArmInfo[0].ArmBackJobExist = eGlassExist.Exist;
                                        tmpBackLocation_SlotNo = eCellSpecialArmSlotNo.UpperLeft_Back;  //Arm#01 Back

                                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                                        tmpRobotArmInfo[0].ArmBackCSTSeq = curBackJob.CassetteSequenceNo;
                                        tmpRobotArmInfo[0].ArmBackJobSeq = curBackJob.JobSequenceNo;
                                    }

                                    break;

                                case 2: //Lower/Left Arm Arm#02

                                    //根據Job 前後位置來決定更新Arm運算資訊
                                    if (curFrontJob != null)
                                    {
                                        tmpRobotArmInfo[1].ArmFrontJobExist = eGlassExist.Exist;
                                        tmpFrontLocation_SlotNo = eCellSpecialArmSlotNo.LowerLeft_Front; //Arm#02 Front

                                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                                        tmpRobotArmInfo[1].ArmFrontCSTSeq = curFrontJob.CassetteSequenceNo;
                                        tmpRobotArmInfo[1].ArmFrontJobSeq = curFrontJob.JobSequenceNo;

                                    }

                                    if (curBackJob != null)
                                    {
                                        tmpRobotArmInfo[1].ArmBackJobExist = eGlassExist.Exist;
                                        tmpBackLocation_SlotNo = eCellSpecialArmSlotNo.LowerLeft_Back; //Arm#02 back

                                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                                        tmpRobotArmInfo[1].ArmBackCSTSeq = curBackJob.CassetteSequenceNo;
                                        tmpRobotArmInfo[1].ArmBackJobSeq = curBackJob.JobSequenceNo;
                                    }

                                    break;

                                //case 3: //Left Both Arm //硬體不支援 同Other異常處理

                                case 4: //Upper/Right Arm  Arm#03

                                    //根據Job 前後位置來決定更新Arm運算資訊
                                    if (curFrontJob != null)
                                    {
                                        tmpRobotArmInfo[2].ArmFrontJobExist = eGlassExist.Exist;
                                        tmpFrontLocation_SlotNo = eCellSpecialArmSlotNo.UpperRight_Front; //Arm#03 Front

                                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                                        tmpRobotArmInfo[2].ArmFrontCSTSeq = curFrontJob.CassetteSequenceNo;
                                        tmpRobotArmInfo[2].ArmFrontJobSeq = curFrontJob.JobSequenceNo;
                                    }

                                    if (curBackJob != null)
                                    {
                                        tmpRobotArmInfo[2].ArmBackJobExist = eGlassExist.Exist;
                                        tmpBackLocation_SlotNo = eCellSpecialArmSlotNo.UpperRight_Back; //Arm#03 back

                                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                                        tmpRobotArmInfo[2].ArmBackCSTSeq = curBackJob.CassetteSequenceNo;
                                        tmpRobotArmInfo[2].ArmBackJobSeq = curBackJob.JobSequenceNo;

                                    }

                                    break;

                                //case 5: //Upper Both Arm Arm#01 & Arm#03 //設定目前不支援UpperBoth 異常處理

                                case 8: //Lower/Right Arm  Arm#04

                                    //根據Job 前後位置來決定更新Arm運算資訊
                                    if (curFrontJob != null)
                                    {
                                        tmpRobotArmInfo[3].ArmFrontJobExist = eGlassExist.Exist;
                                        tmpFrontLocation_SlotNo = eCellSpecialArmSlotNo.LowerRight_Front; //Arm#04 Front

                                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                                        tmpRobotArmInfo[3].ArmFrontCSTSeq = curFrontJob.CassetteSequenceNo;
                                        tmpRobotArmInfo[3].ArmFrontJobSeq = curFrontJob.JobSequenceNo;
                                    }

                                    if (curBackJob != null)
                                    {
                                        tmpRobotArmInfo[3].ArmBackJobExist = eGlassExist.Exist;
                                        tmpBackLocation_SlotNo = eCellSpecialArmSlotNo.LowerRight_Back; //Arm#04 back

                                        //20160112 add OnArm還要比對Arm上的CstSeq跟JobSeq
                                        tmpRobotArmInfo[3].ArmBackCSTSeq = curBackJob.CassetteSequenceNo;
                                        tmpRobotArmInfo[3].ArmBackJobSeq = curBackJob.JobSequenceNo;
                                    }

                                    break;


                                //case 10: //Lower Both Arm  Arm#02 & Arm#04 //設定目前不支援UpperBoth 異常處理

                                //case 12: //Right Both Arm  Arm#03 & Arm#04 //設定目前不支援UpperBoth 異常處理

                                default:

                                    if (curFrontJob != null)

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {
                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) 1st defineNormalRobotCmd Action({3}) but UseArm({4}) is out of Range!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                    errMsg = string.Format("[{0}] can not Get 1st defineNormalRobotCmd Action({1}) but UseArm({2}) is out of Range!",
                                                            MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString(), cur1stRobotCmd.Cmd01_ArmSelect.ToString());

                                    errCode = eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail;

                                    return false;

                            }

                            #endregion

                            if (curFrontJob != null)
                            {
                                //Sub Loction 還是出片前的位置
                                tmpFrontSubLocation = curFrontJob.RobotWIP.CurSubLocation;
                            }

                            if (curBackJob != null)
                            {
                                //Sub Loction 還是出片前的位置
                                tmpBackSubLocation = curBackJob.RobotWIP.CurSubLocation;
                            }


                            #endregion

                            break;

                        default:

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) 1st defineNormalRobotCmd Action({3}) is out of Range!",
                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, cur1stRobotCmd.Cmd01_Command.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("[{0}] can not Get 1st defineNormalRobotCmd Action({1}) is out of Range!",
                                                    MethodBase.GetCurrentMethod().Name, cur1stRobotCmd.Cmd01_Command.ToString());

                            errCode = eJobFilter_ReturnCode.NG_Get_1st_DefineNormalRobotCommandInfo_Is_Fail;

                            return false;

                    }

                    #endregion

                    #endregion

                }

                #endregion

                #region [ Get Front/Back Job Check Step StageID, UseArm and Action ]

                string tmpFrontJobStepUseArm = string.Empty;
                string tmpFrontJobStepAction = string.Empty;
                string tmpBackJobStepUseArm = string.Empty;
                string tmpBackJobStepAction = string.Empty;

                if (curFrontJob != null && curBackJob != null)
                {
                    tmpFrontJobStepUseArm = curFrontJob.RobotWIP.RobotRouteStepList[tmpFrontStepNo].Data.ROBOTUSEARM.ToString().Trim();
                    tmpFrontJobStepAction = curFrontJob.RobotWIP.RobotRouteStepList[tmpFrontStepNo].Data.ROBOTACTION.ToString().Trim();
                    tmpBackJobStepUseArm = curBackJob.RobotWIP.RobotRouteStepList[tmpBackStepNo].Data.ROBOTUSEARM.ToString().Trim();
                    tmpBackJobStepAction = curBackJob.RobotWIP.RobotRouteStepList[tmpBackStepNo].Data.ROBOTACTION.ToString().Trim();

                    #region [ 比對Front and Back Job StageID, UseArm與Action必須要相同才行 ]

                    if (tmpFrontStageID != tmpBackStageID)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) frontJob({2},{3}) Check StepID({4}) StageID({5}) But BackJob({6},{7}) Check StepID({8}) StageID({9}) is different!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    tmpFrontStepNo, tmpFrontStageID, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
                                                    tmpBackStepNo, tmpBackStageID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("frontJob({0},{1}) Check StepID({2}) StageID({3}) But BackJob({4},{5}) Check StepID({6}) StageID({7}) is different!",
                                                    curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    tmpFrontStepNo, tmpFrontStageID, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
                                                    tmpBackStepNo, tmpBackStageID);

                        errCode = eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBack_LocationStageID_Is_Different;

                        return false;

                    }

                    if (tmpFrontJobStepAction != tmpBackJobStepAction)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) frontJob({2},{3}) Check StepID({4}) Action({5}) But BackJob({6},{7}) Check StepID({8}) Action({9}) is different!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    tmpFrontStepNo, tmpFrontJobStepAction, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
                                                    tmpBackStepNo, tmpBackJobStepAction);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("frontJob({0},{1}) Check StepID({2}) Action({3}) But BackJob({4},{5}) Check StepID({6}) Action({7}) is different!",
                                                    curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    tmpFrontStepNo, tmpFrontJobStepAction, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
                                                    tmpBackStepNo, tmpBackJobStepAction);

                        errCode = eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBack_Action_Is_Different;

                        return false;

                    }

                    if (tmpFrontJobStepUseArm != tmpBackJobStepUseArm)
                    {

                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) frontJob({2},{3}) Check StepID({4}) UseArm({5}) But BackJob({6},{7}) Check StepID({8}) UseArm({9}) is different!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    tmpFrontStepNo, tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
                                                    tmpBackStepNo, tmpBackJobStepUseArm);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("frontJob({0},{1}) Check StepID({2}) UseArm({3}) But BackJob({4},{5}) Check StepID({6}) UseArm({7}) is different!",
                                                    curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    tmpFrontStepNo, tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
                                                    tmpBackStepNo, tmpBackJobStepUseArm);

                        errCode = eJobFilter_ReturnCode.NG_SlotBlockInfo_FrontBack_UseArm_Is_Different;

                        return false;

                    }

                    #endregion

                }
                else if (curFrontJob != null && curBackJob == null)
                {
                    //Back is Empty 等同Front
                    tmpFrontJobStepUseArm = curFrontJob.RobotWIP.RobotRouteStepList[tmpFrontStepNo].Data.ROBOTUSEARM.ToString().Trim();
                    tmpFrontJobStepAction = curFrontJob.RobotWIP.RobotRouteStepList[tmpFrontStepNo].Data.ROBOTACTION.ToString().Trim();
                    tmpBackJobStepUseArm = tmpFrontJobStepUseArm;
                    tmpBackJobStepAction = tmpFrontJobStepAction;
                }
                else if (curFrontJob == null && curBackJob != null)
                {
                    //Front is Empty 等同Back
                    tmpBackJobStepUseArm = curBackJob.RobotWIP.RobotRouteStepList[tmpBackStepNo].Data.ROBOTUSEARM.ToString().Trim();
                    tmpBackJobStepAction = curBackJob.RobotWIP.RobotRouteStepList[tmpBackStepNo].Data.ROBOTACTION.ToString().Trim();
                    tmpFrontJobStepUseArm = tmpBackJobStepUseArm;
                    tmpFrontJobStepAction = tmpBackJobStepAction;
                }

                #endregion

                //定義最後選擇的Arm資訊
                string curAfterCheckUseArm = string.Empty;

                #region [ by Front/Back curStep Location Check Use Arm ]

                //Spec對應
                //0: None               //2: Lower/Left Arm  //4: Upper/Right Arm
                //1: Upper/Left Arm     //3: Left Both Arm   //8: Lower/Right Arm
                //12: Right Both Arm
                //20160127 add 5 and 10
                //5: Upper Both Arm 
                //10: Lower Both Arm
                //當沒有FrontJob時 會以BcakJob StageID來填入值
                if (tmpFrontStageID != eRobotCommonConst.ROBOT_HOME_STAGEID)
                {
                    //Check Not On Arm相關
                    #region [ Job Loaction Not On Arm. Not On Arm:Only:Use Arm must Empty ]
                    RobotStage stagefront = ObjectManager.RobotStageManager.GetRobotStagebyStageID(tmpFrontStageID);
                    RobotStage stageback = ObjectManager.RobotStageManager.GetRobotStagebyStageID(tmpBackStageID);
                    //Front/Back都是相同所以以Front處理即可
                    switch (tmpFrontJobStepUseArm)
                    {
                        case eDBRobotUseArmCode.UPPER_ARM:  //Upper Left/Right(Arm#01 or Arm#03)其中之一為空即可 
                            //20160602
                            //由左到右Arm判斷NoExist
                            if ((stagefront.Data.REMARKS == null && stageback.Data.REMARKS == null) ||
                                (stagefront.Data.REMARKS == string.Empty && stageback.Data.REMARKS == string.Empty) ||
                                (stagefront.Data.REMARKS.Split(',')[0] == string.Empty && stageback.Data.REMARKS.Split(',')[0] == string.Empty) ||
                                (stagefront.Data.REMARKS.Split(',')[0].Equals("L") && stageback.Data.REMARKS.Split(',')[0].Equals("L")))
                            {
                                #region [ StageJob Route Use Upper Arm But Upper Left(Arm#01) and Upper Right(Arm#03) Arm Job Exist ]

                                //Arm#01 or Arm#03上不可以有片
                                //先判斷Arm01是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                       (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("UL") && stageback.Data.REMARKS.Split(',')[1].Equals("UL"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                        break;
                                    }
                                    //}

                                }
                                //再判斷Arm03是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    ////Stage Job Check LowArm is Empty
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                        (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("UR") && stageback.Data.REMARKS.Split(',')[1].Equals("UR"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                        break;
                                    }
                                }
                                else
                                {
                                    //Stage Job but Upper Left Arm and Upper Right Arm is not Empty
                                    if (curFrontJob != null && curBackJob != null)
                                    {

                                        #region [ Front Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm01 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm03 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm02 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm04 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob != null && curBackJob == null)
                                    {

                                        #region [ Front Exist , Back Not Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Not Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob == null && curBackJob != null)
                                    {
                                        #region [ Front Not Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Not Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }

                                    errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist;

                                    return false;

                                }

                                #endregion
                            }
                            else
                            {
                                //由右到左Arm判斷NoExist
                                #region [ StageJob Route Use Upper Arm But Upper Right(Arm#03) and Upper Left(Arm#01) Arm Job Exist ]

                                //Arm#01 or Arm#03上不可以有片

                                //先判斷Arm03是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    ////Stage Job Check LowArm is Empty
                                    //curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                       (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("UR") && stageback.Data.REMARKS.Split(',')[1].Equals("UR"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                        break;
                                    }
                                }
                                //再判斷Arm01是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                       (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("UL") && stageback.Data.REMARKS.Split(',')[1].Equals("UL"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                        break;
                                    }

                                }
                                else
                                {
                                    //Stage Job but Upper Left Arm and Upper Right Arm is not Empty
                                    if (curFrontJob != null && curBackJob != null)
                                    {

                                        #region [ Front Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm01 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm03 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm02 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob != null && curBackJob == null)
                                    {

                                        #region [ Front Exist , Back Not Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Not Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob == null && curBackJob != null)
                                    {
                                        #region [ Front Not Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Not Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }

                                    errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist;

                                    return false;

                                }

                                #endregion
                            }


                            break;

                        case eDBRobotUseArmCode.LOWER_ARM:
                            //20160602
                            //由左到右Arm判斷NoExist
                            if ((stagefront.Data.REMARKS == null && stageback.Data.REMARKS == null) ||
                                (stagefront.Data.REMARKS == string.Empty && stageback.Data.REMARKS == string.Empty) ||
                                (stagefront.Data.REMARKS.Split(',')[0] == string.Empty && stageback.Data.REMARKS.Split(',')[0] == string.Empty) ||
                                (stagefront.Data.REMARKS.Split(',')[0].Equals("L") && stageback.Data.REMARKS.Split(',')[0].Equals("L")))
                            {
                                #region [ StageJob Route Use Upper Arm But Upper Left(Arm#01) and Upper Right(Arm#03) Arm Job Exist ]

                                //在判斷Arm#02 or Arm#04上不可以有片
                                if (tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                       (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("LL") && stageback.Data.REMARKS.Split(',')[1].Equals("LL"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                        break;
                                    }

                                }
                                //再判斷Arm04是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                       (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("LR") && stageback.Data.REMARKS.Split(',')[1].Equals("LR"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                        break;
                                    }
                                }
                                else
                                {
                                    //Stage Job but Upper Left Arm and Upper Right Arm is not Empty
                                    if (curFrontJob != null && curBackJob != null)
                                    {

                                        #region [ Front Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm01 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm03 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob CassetteSequenceNo({7}) JobSequenceNo({8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm02 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm04 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob != null && curBackJob == null)
                                    {

                                        #region [ Front Exist , Back Not Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Not Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({18},{19}) JobExist({20}) ArmDisable({21})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob == null && curBackJob != null)
                                    {
                                        #region [ Front Not Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, 
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Not Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }

                                    errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist;

                                    return false;

                                }

                                #endregion
                            }
                            else
                            {
                                //由右到左Arm判斷NoExist
                                #region [ StageJob Route Use Upper Arm But Upper Right(Arm#03) and Upper Left(Arm#01) Arm Job Exist ]

                                //再判斷Arm04是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    //Stage Job Check LowArm is Empty
                                    //curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                       (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("LR") && stageback.Data.REMARKS.Split(',')[1].Equals("LR"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                        break;
                                    }
                                }

                                //在判斷Arm#02 or Arm#04上不可以有片
                                if (tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                       (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("LL") && stageback.Data.REMARKS.Split(',')[1].Equals("LL"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                        break;
                                    }

                                }
                                else
                                {
                                    //Stage Job but Upper Left Arm and Upper Right Arm is not Empty
                                    if (curFrontJob != null && curBackJob != null)
                                    {

                                        #region [ Front Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm01 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm03 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm02 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm04 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob != null && curBackJob == null)
                                    {

                                        #region [ Front Exist , Back Not Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Not Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob == null && curBackJob != null)
                                    {
                                        #region [ Front Not Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({20}) ArmDisable({21})!",
                                                                MethodBase.GetCurrentMethod().Name,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Not Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, 
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }

                                    errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist;

                                    return false;

                                }

                                #endregion
                            }

                            break;

                        case eDBRobotUseArmCode.ANY_ARM:
                            //由左到右Arm判斷NoExist
                            if ((stagefront.Data.REMARKS == null && stageback.Data.REMARKS == null) ||
                                (stagefront.Data.REMARKS == string.Empty && stageback.Data.REMARKS == string.Empty) ||
                                (stagefront.Data.REMARKS.Split(',')[0] == string.Empty && stageback.Data.REMARKS.Split(',')[0] == string.Empty) ||
                                (stagefront.Data.REMARKS.Split(',')[0].Equals("L") && stageback.Data.REMARKS.Split(',')[0].Equals("L")))
                            {
                                #region [ StageJob Route Use Upper Arm But Upper Left(Arm#01) and Upper Right(Arm#03) Arm Job Exist ]
                                //Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                                //Arm#01 or Arm#03上不可以有片
                                //先判斷Arm01是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    //if (line.Data.LINETYPE != eLineType.CELL.CCCHN)
                                    //if(line.Data.LINEID != "CCCHN100")
                                    //{
                                    //    //Stage Job Check UpArm is Empty
                                    //    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                    //    break;
                                    //}
                                    ////else if (line.Data.LINETYPE == eLineType.CELL.CCCHN && tmpFrontStageID != "15" && tmpBackStageID != "15")
                                    //else if (line.Data.LINEID == "CCCHN100" && tmpFrontStageID != "15" && tmpBackStageID != "15")
                                    //{
                                    //    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                    //    break;
                                    //}

                                    //20160516 目前又不需要判斷CST最後一列(slot >= 501)用右臂了
                                    //if ((stagefront.Data.STAGETYPE == eRobotStageType.PORT && curSlotBlockInfo.CurBlock_RobotCmdSlotNo < 501) || (stagefront.Data.STAGETYPE != eRobotStageType.PORT))
                                    //{
                                        if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                           (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                        {
                                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                            break;
                                        }
                                        else if (stagefront.Data.REMARKS.Split(',')[1].Equals("UL") && stageback.Data.REMARKS.Split(',')[1].Equals("UL"))
                                        {
                                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                            break;
                                        }
                                    //}

                                }
                                //再判斷Arm03是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    ////Stage Job Check LowArm is Empty
                                    //curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                        (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("UR") && stageback.Data.REMARKS.Split(',')[1].Equals("UR"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                        break;
                                    }
                                }
                                //在判斷Arm#02 or Arm#04上不可以有片
                                if (tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    //if (line.Data.LINEID != "CCCHN100")
                                    //{
                                    //    //Stage Job Check UpArm is Empty
                                    //    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                    //    break;
                                    //}
                                    ////else if (line.Data.LINETYPE == eLineType.CELL.CCCHN && tmpFrontStageID != "15" && tmpBackStageID != "15")
                                    //else if (line.Data.LINEID == "CCCHN100" && tmpFrontStageID != "15" && tmpBackStageID != "15")
                                    //{
                                    //    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                    //    break;
                                    //}

                                    //20160516 目前又不需要判斷CST最後一列(slot >= 501)用右臂了
                                    //if ((stagefront.Data.STAGETYPE == eRobotStageType.PORT && curSlotBlockInfo.CurBlock_RobotCmdSlotNo < 501) || (stagefront.Data.STAGETYPE != eRobotStageType.PORT))
                                    //{
                                        if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                           (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                        {
                                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                            break;
                                        }
                                        else if (stagefront.Data.REMARKS.Split(',')[1].Equals("LL") && stageback.Data.REMARKS.Split(',')[1].Equals("LL"))
                                        {
                                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                            break;
                                        }
                                    //}

                                }
                                //再判斷Arm04是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    //Stage Job Check LowArm is Empty
                                    //curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                       (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("LR") && stageback.Data.REMARKS.Split(',')[1].Equals("LR"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                        break;
                                    }
                                }
                                else
                                {
                                    //Stage Job but Upper Left Arm and Upper Right Arm is not Empty
                                    if (curFrontJob != null && curBackJob != null)
                                    {

                                        #region [ Front Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm01 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm03 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm02 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm04 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob != null && curBackJob == null)
                                    {

                                        #region [ Front Exist , Back Not Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Not Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob == null && curBackJob != null)
                                    {
                                        #region [ Front Not Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Not Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }

                                    errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist;

                                    return false;

                                }

                                #endregion
                            }
                            else
                            {
                                //由右到左Arm判斷NoExist
                                #region [ StageJob Route Use Upper Arm But Upper Right(Arm#03) and Upper Left(Arm#01) Arm Job Exist ]
                                //Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                                //Arm#01 or Arm#03上不可以有片

                                //先判斷Arm03是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    ////Stage Job Check LowArm is Empty
                                    //curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                       (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("UR") && stageback.Data.REMARKS.Split(',')[1].Equals("UR"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                        break;
                                    }
                                }
                                //再判斷Arm01是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    //if (line.Data.LINETYPE != eLineType.CELL.CCCHN)
                                    //if(line.Data.LINEID != "CCCHN100")
                                    //{
                                    //    //Stage Job Check UpArm is Empty
                                    //    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                    //    break;
                                    //}
                                    ////else if (line.Data.LINETYPE == eLineType.CELL.CCCHN && tmpFrontStageID != "15" && tmpBackStageID != "15")
                                    //else if (line.Data.LINEID == "CCCHN100" && tmpFrontStageID != "15" && tmpBackStageID != "15")
                                    //{
                                    //    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                    //    break;
                                    //}

                                    //20160516 目前又不需要判斷CST最後一列(slot >= 501)用右臂了
                                    //if ((stagefront.Data.STAGETYPE == eRobotStageType.PORT && curSlotBlockInfo.CurBlock_RobotCmdSlotNo < 501) || (stagefront.Data.STAGETYPE != eRobotStageType.PORT))
                                    //{
                                        if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                           (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                        {
                                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                            break;
                                        }
                                        else if (stagefront.Data.REMARKS.Split(',')[1].Equals("UL") && stageback.Data.REMARKS.Split(',')[1].Equals("UL"))
                                        {
                                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                            break;
                                        }
                                    //}

                                }
                                //再判斷Arm04是否可收(沒片且有Enable)
                                if (tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.NoExist &&
                                         tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    //Stage Job Check LowArm is Empty
                                    //curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                    if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                       (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                        break;
                                    }
                                    else if (stagefront.Data.REMARKS.Split(',')[1].Equals("LR") && stageback.Data.REMARKS.Split(',')[1].Equals("LR"))
                                    {
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                        break;
                                    }
                                }

                                //在判斷Arm#02 or Arm#04上不可以有片
                                if (tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.NoExist &&
                                    tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                    //if (line.Data.LINEID != "CCCHN100")
                                    //{
                                    //    //Stage Job Check UpArm is Empty
                                    //    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                    //    break;
                                    //}
                                    ////else if (line.Data.LINETYPE == eLineType.CELL.CCCHN && tmpFrontStageID != "15" && tmpBackStageID != "15")
                                    //else if (line.Data.LINEID == "CCCHN100" && tmpFrontStageID != "15" && tmpBackStageID != "15")
                                    //{
                                    //    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                    //    break;
                                    //}

                                    //20160516 目前又不需要判斷CST最後一列(slot >= 501)用右臂了
                                    //if ((stagefront.Data.STAGETYPE == eRobotStageType.PORT && curSlotBlockInfo.CurBlock_RobotCmdSlotNo < 501) || (stagefront.Data.STAGETYPE != eRobotStageType.PORT))
                                    //{
                                        if (stagefront.Data.REMARKS.Split(',').Length == 1 && stageback.Data.REMARKS.Split(',').Length == 1 ||
                                           (stagefront.Data.REMARKS.Split(',')[1] == string.Empty && stageback.Data.REMARKS.Split(',')[1] == string.Empty))
                                        {
                                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                            break;
                                        }
                                        else if (stagefront.Data.REMARKS.Split(',')[1].Equals("LL") && stageback.Data.REMARKS.Split(',')[1].Equals("LL"))
                                        {
                                            curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                            break;
                                        }
                                    //}

                                }
                                else
                                {
                                    //Stage Job but Upper Left Arm and Upper Right Arm is not Empty
                                    if (curFrontJob != null && curBackJob != null)
                                    {

                                        #region [ Front Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm01 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm03 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm02 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm04 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                                MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob != null && curBackJob == null)
                                    {

                                        #region [ Front Exist , Back Not Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Exist , Back Not Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                    curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                    tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }
                                    else if (curFrontJob == null && curBackJob != null)
                                    {
                                        #region [ Front Not Exist , Back Exist Check UpArm(1,3) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        #endregion

                                        #region [ Front Not Exist , Back Exist Check LowArm(2,4) Log ]

                                        #region[DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                    curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                    curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                    tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                    tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                    tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                    tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                    tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                                MethodBase.GetCurrentMethod().Name, 
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        #endregion

                                    }

                                    errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_GlassExist;

                                    return false;

                                }

                                #endregion
                            }
                            break;

                        //case eDBRobotUseArmCode.ALL_ARM: //設定不支援視同異常處理

                        default:

                            #region [ DB Setting Illegal ]

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) is illegal!",
                                                        curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                        tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction, tmpFrontJobStepUseArm);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}) is illegal!",
                                                    MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                    tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction, tmpFrontJobStepUseArm);

                            errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_Setting_Fail;

                            return false;

                            #endregion

                    }

                    #endregion

                }
                else
                {
                    //20160104 Check Arm相關
                    #region [ Job Location on Arm. Must Check Use Arm(Job LocationSlot has Job).tmpFrontLocation_SlotNo and tmpBackLocation_SlotNo ]

                    switch (tmpFrontJobStepUseArm)
                    {
                        case eDBRobotUseArmCode.UPPER_ARM:  //Upper Left/Right(Arm#01 or Arm#03)其中之一不為空即可 by tmpFrontLocation_SlotNo and tmpBackLocation_SlotNo 

                            #region [ StageJob Route Use Upper Arm But Upper Left(Arm#01) and Upper Right(Arm#03) Arm Job NotExist ]

                            if (curFrontJob != null && curBackJob != null)
                            {

                                #region [ Front and Back Job Exist ]

                                //Arm#01 front or Back JobExist. 還要比對Arm上的CstSeq跟JobSeq
                                if (((tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Front) &&
                                     (tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Back) &&
                                     (tmpRobotArmInfo[0].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[0].ArmFrontJobSeq == tmpFront_JobSeq) &&
                                     (tmpRobotArmInfo[0].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[0].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                    tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;

                                }
                                //Arm#03 front or Back JobExist. 還要比對Arm上的CstSeq跟JobSeq
                                else if (((tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Front) &&
                                          (tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Back) &&
                                          (tmpRobotArmInfo[2].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[2].ArmFrontJobSeq == tmpFront_JobSeq) &&
                                          (tmpRobotArmInfo[2].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[2].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                         tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;

                                }
                                else
                                {

                                    #region [ Front Exist , Back Exist Log ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    //20160303 modify Log bug
                                    //errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                    //                        MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                    //                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                    //                        tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                    //                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                    //                        tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                    //                        tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                    //                        tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                    //                        tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                    //                        tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                    errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm01 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm03 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                            MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                            curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                            tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                            tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                            tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                    #endregion

                                    errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;

                                    return false;
                                }

                                #endregion

                            }
                            else if (curFrontJob != null && curBackJob == null)
                            {

                                #region [ Front Exist and Back Job Notexist ]

                                //Arm#01 front Exist Back Notexist
                                if (((tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Front) &&
                                     (tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.NoExist) &&
                                     (tmpRobotArmInfo[0].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[0].ArmFrontJobSeq == tmpFront_JobSeq)) &&
                                    tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;

                                }
                                //Arm#03 front or Back JobExist
                                else if (((tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Front) &&
                                          (tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.NoExist) &&
                                          (tmpRobotArmInfo[2].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[2].ArmFrontJobSeq == tmpFront_JobSeq)) &&
                                         tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;

                                }
                                else
                                {

                                    #region [ Front Exist , Back Not Exist Log ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                            MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                            curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                            tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                            tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                    #endregion

                                    errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;

                                    return false;
                                }

                                #endregion

                            }
                            //20160608 寫錯了,是Front沒Job,Back有Job才對
                            //else if (curFrontJob != null && curBackJob == null)
                            else if (curFrontJob == null && curBackJob != null)
                            {

                                #region [ Front Not exist and Back Job Exist ]

                                //Arm#01 front or Back JobExist
                                if (((tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.NoExist) &&
                                     (tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Back) &&
                                     (tmpRobotArmInfo[0].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[0].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                    tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;

                                }
                                //Arm#03 front or Back JobExist
                                else if (((tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.NoExist) &&
                                          (tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Back) &&
                                          (tmpRobotArmInfo[2].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[2].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                         tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;

                                }
                                else
                                {

                                    #region [ Front Not Exist , Back Exist Log ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                            MethodBase.GetCurrentMethod().Name,
                                                            curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                            tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                            tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                    #endregion

                                    errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;

                                    return false;
                                }

                                #endregion

                            }
                            else
                            {

                                #region [ Front and Back Job Not Exist ]

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get curSlotBlockInfo Front and Back jobInfo!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] can not Get curSlotBlockInfo Front and Back jobInfo!",
                                                        MethodBase.GetCurrentMethod().Name);

                                errCode = eJobFilter_ReturnCode.NG_SlotBlockInfo_Job_Is_Null;

                                return false;

                                #endregion
                            }

                            #endregion

                            break;

                        case eDBRobotUseArmCode.LOWER_ARM:  //Lower Left/Right(Arm#02 or Arm#04)其中之一不為空即可 by tmpFrontLocation_SlotNo and tmpBackLocation_SlotNo 

                            #region [ StageJob Route Use Upper Arm But Lower Left(Arm#02) and Upper Right(Arm#04) Arm Job NotExist ]

                            if (curFrontJob != null && curBackJob != null)
                            {

                                #region [ Front and Back Job Exist ]

                                //Arm#02 front or Back JobExist
                                if (((tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.LowerLeft_Front) &&
                                     (tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.LowerLeft_Back) &&
                                     (tmpRobotArmInfo[1].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[1].ArmFrontJobSeq == tmpFront_JobSeq) &&
                                     (tmpRobotArmInfo[1].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[1].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                    tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;

                                }
                                //Arm#04 front or Back JobExist
                                else if (((tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.LowerRight_Front) &&
                                          (tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.LowerRight_Back) &&
                                          (tmpRobotArmInfo[3].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[3].ArmFrontJobSeq == tmpFront_JobSeq) &&
                                          (tmpRobotArmInfo[3].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[3].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                         tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;

                                }
                                else
                                {

                                    #region [ Front Exist , Back Exist Log ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    //20160303 fix Log bug
                                    //errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                    //                        MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                    //                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                    //                        tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                    //                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                    //                        tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                    //                        tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                    //                        tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                    //                        tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                    //                        tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                    errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm02 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm04 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                            MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                            curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                            tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                            tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                            tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                    #endregion

                                    errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;

                                    return false;
                                }

                                #endregion

                            }
                            else if (curFrontJob != null && curBackJob == null)
                            {

                                #region [ Front Exist and Back Job Notexist ]

                                //Arm#02 front Exist Back Notexist
                                if (((tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.LowerLeft_Front) &&
                                     (tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.NoExist) &&
                                     (tmpRobotArmInfo[1].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[1].ArmFrontJobSeq == tmpFront_JobSeq)) &&
                                    tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;

                                }
                                //Arm#04 front or Back JobExist
                                else if (((tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.LowerRight_Front) &&
                                          (tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.NoExist) &&
                                     (tmpRobotArmInfo[3].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[3].ArmFrontJobSeq == tmpFront_JobSeq)) &&
                                         tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;

                                }
                                else
                                {

                                    #region [ Front Exist , Back Not Exist Log ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                            MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                            curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                            tmpFrontJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                            tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                    #endregion

                                    errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;

                                    return false;
                                }

                                #endregion

                            }
                            else if (curFrontJob != null && curBackJob == null)
                            {

                                #region [ Front Not exist and Back Job Exist ]

                                //Arm#02 front or Back JobExist
                                if (((tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.NoExist) &&
                                     (tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Back) &&
                                     (tmpRobotArmInfo[1].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[1].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                    tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;

                                }
                                //Arm#04 front or Back JobExist
                                else if (((tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.NoExist) &&
                                          (tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Back) &&
                                          (tmpRobotArmInfo[3].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[3].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                         tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;

                                }
                                else
                                {

                                    #region [ Front Not Exist , Back Exist Log ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                            MethodBase.GetCurrentMethod().Name,
                                                            curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                            tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                            tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                    #endregion

                                    errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;

                                    return false;
                                }

                                #endregion

                            }
                            else
                            {

                                #region [ Front and Back Job Not Exist ]

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get curSlotBlockInfo Front and Back jobInfo!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] can not Get curSlotBlockInfo  Front and Back jobInfo!",
                                                        MethodBase.GetCurrentMethod().Name);

                                errCode = eJobFilter_ReturnCode.NG_SlotBlockInfo_Job_Is_Null;

                                return false;

                                #endregion
                            }

                            #endregion

                            break;

                        case eDBRobotUseArmCode.ANY_ARM:

                            #region [ StageJob Route Use Upper Arm But Upper Left(Arm#01) and Upper Right(Arm#03) Arm Job NotExist ]
                            Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                            if (curFrontJob != null && curBackJob != null)
                            {

                                #region [ Front and Back Job Exist ]

                                //Arm#01 front or Back JobExist. 還要比對Arm上的CstSeq跟JobSeq
                                if (((tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Front) &&
                                     (tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Back) &&
                                     (tmpRobotArmInfo[0].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[0].ArmFrontJobSeq == tmpFront_JobSeq) &&
                                     (tmpRobotArmInfo[0].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[0].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                    tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                        //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                        break;

                                }
                                //Arm#03 front or Back JobExist. 還要比對Arm上的CstSeq跟JobSeq
                                if (((tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Front) &&
                                          (tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Back) &&
                                          (tmpRobotArmInfo[2].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[2].ArmFrontJobSeq == tmpFront_JobSeq) &&
                                          (tmpRobotArmInfo[2].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[2].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                         tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                    break;

                                }
                                //Arm#02 front or Back JobExist
                                if (((tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.LowerLeft_Front) &&
                                         (tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.LowerLeft_Back) &&
                                         (tmpRobotArmInfo[1].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[1].ArmFrontJobSeq == tmpFront_JobSeq) &&
                                         (tmpRobotArmInfo[1].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[1].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                        tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                        //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                        break;
                                }
                                //Arm#04 front or Back JobExist
                                if (((tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.LowerRight_Front) &&
                                          (tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.LowerRight_Back) &&
                                          (tmpRobotArmInfo[3].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[3].ArmFrontJobSeq == tmpFront_JobSeq) &&
                                          (tmpRobotArmInfo[3].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[3].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                         tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                    break;
                                }
                                else
                                {

                                    #region [ Front Exist , Back Exist Log for UpperArm(1,3) ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    //20160303 fix log bug
                                    //errMsg = string.Format("[{0}][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm01 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm03 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                    //                        MethodBase.GetCurrentMethod().Name, funcName, curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                    //                        curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                    //                        tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                    //                        tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                    //                        tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                    //                        tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                    //                        tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                    //                        tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                    //                        tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                    errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm01 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm03 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                            MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                            curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                            tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                            tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                            tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                    #endregion

                                    #region [ Front Exist , Back Exist Log for LowerArm(2,4) ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), BackJob CassetteSequenceNo({9}) JobSequenceNo({10}) StageID({11}) StepNo({12}) setting Action({13}) UseArm({14}), but Robot Arm02 FrontJob({15},{16}) JobExist({17}), BackJob({18},{19}) JobExist({20}) ArmDisable({21}) and Arm04 FrontJob({22},{23}) JobExist({24}), BackJob({25},{26}) JobExist({27}) ArmDisable({28})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), BackJob({7}_{8}) StageID({9}) StepNo({10}) setting Action({11}) UseArm({12}), but Robot Arm02 FrontJob({13},{14}) JobExist({15}), BackJob({16},{17}) JobExist({18}) ArmDisable({19}) and Arm04 FrontJob({20},{21}) JobExist({22}), BackJob({23},{24}) JobExist({25}) ArmDisable({26})!",
                                                            MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                            curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                            tmpFrontJobStepUseArm, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                            tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                            tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                    #endregion

                                    errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;

                                    return false;
                                }

                                #endregion

                            }
                            else if (curFrontJob != null && curBackJob == null)
                            {

                                #region [ Front Exist and Back Job Notexist ]

                                //Arm#01 front Exist Back Notexist
                                if (((tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Front) &&
                                     (tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.NoExist) &&
                                     (tmpRobotArmInfo[0].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[0].ArmFrontJobSeq == tmpFront_JobSeq)) &&
                                    tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                        //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                        break;

                                }
                                //Arm#03 front or Back JobExist
                                if (((tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Front) &&
                                          (tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.NoExist) &&
                                          (tmpRobotArmInfo[2].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[2].ArmFrontJobSeq == tmpFront_JobSeq)) &&
                                         tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                    break;

                                }
                                //Arm#02 front Exist Back Notexist
                                if (((tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.LowerLeft_Front) &&
                                     (tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.NoExist) &&
                                     (tmpRobotArmInfo[1].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[1].ArmFrontJobSeq == tmpFront_JobSeq)) &&
                                    tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                        //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                        break;

                                }
                                //Arm#04 front or Back JobExist
                                if (((tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.Exist && tmpFrontLocation_SlotNo == eCellSpecialArmSlotNo.LowerRight_Front) &&
                                          (tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.NoExist) &&
                                     (tmpRobotArmInfo[3].ArmFrontCSTSeq == tmpFront_CstSeq && tmpRobotArmInfo[3].ArmFrontJobSeq == tmpFront_JobSeq)) &&
                                         tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;
                                    break;

                                }
                                else
                                {

                                    #region [ Front Exist , Back Not Exist Log ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                            MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                            curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                            tmpFrontJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                            tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                    #endregion

                                    #region [ Front Exist , Back Not Exist Log ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), Back is empty, but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm04 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo,
                                                                curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                                tmpFrontJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] FrontJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), Back is empty, but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                            MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo,
                                                            curFrontJob.JobSequenceNo, tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction,
                                                            tmpFrontJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                            tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                    #endregion

                                    errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;

                                    return false;
                                }

                                #endregion

                            }
                            else if (curFrontJob == null && curBackJob != null)
                            {

                                #region [ Front Not exist and Back Job Exist ]

                                //Arm#01 front or Back JobExist
                                if (((tmpRobotArmInfo[0].ArmFrontJobExist == eGlassExist.NoExist) &&
                                     (tmpRobotArmInfo[0].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperLeft_Back) &&
                                     (tmpRobotArmInfo[0].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[0].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                    tmpRobotArmInfo[0].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                        //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_LEFT_ARM01;
                                        break;

                                }
                                //Arm#03 front or Back JobExist
                                if (((tmpRobotArmInfo[2].ArmFrontJobExist == eGlassExist.NoExist) &&
                                          (tmpRobotArmInfo[2].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.UpperRight_Back) &&
                                          (tmpRobotArmInfo[2].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[2].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                         tmpRobotArmInfo[2].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_UPPER_RIGHT_ARM03;
                                    break;

                                }
                                //Arm#02 front or Back JobExist
                                if (((tmpRobotArmInfo[1].ArmFrontJobExist == eGlassExist.NoExist) &&
                                     (tmpRobotArmInfo[1].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.LowerLeft_Back) &&
                                     (tmpRobotArmInfo[1].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[1].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                    tmpRobotArmInfo[1].ArmDisableFlag == eArmDisableStatus.Enable)
                                {
                                        //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                        curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_LEFT_ARM02;
                                        break;

                                }
                                //Arm#04 front or Back JobExist
                                if (((tmpRobotArmInfo[3].ArmFrontJobExist == eGlassExist.NoExist) &&
                                          (tmpRobotArmInfo[3].ArmBackJobExist == eGlassExist.Exist && tmpBackLocation_SlotNo == eCellSpecialArmSlotNo.LowerRight_Back) &&
                                          (tmpRobotArmInfo[3].ArmBackCSTSeq == tmpBack_CstSeq && tmpRobotArmInfo[3].ArmBackJobSeq == tmpBack_JobSeq)) &&
                                         tmpRobotArmInfo[3].ArmDisableFlag == eArmDisableStatus.Enable)
                                {

                                    //Arm Has Job , and UpArm Has Job 要考慮Job位置!
                                    curAfterCheckUseArm = eDBRobotUseArmCode.CELL_SPECIAL_LOWER_RIGHT_ARM04;

                                }
                                else
                                {

                                    #region [ Front Not Exist , Back Exist Log ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm01 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                                tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm01 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm03 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                            MethodBase.GetCurrentMethod().Name, 
                                                            curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                            tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[0].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[0].ArmFrontJobSeq, tmpRobotArmInfo[0].ArmFrontJobExist.ToString(), tmpRobotArmInfo[0].ArmBackCSTSeq, tmpRobotArmInfo[0].ArmBackJobSeq,
                                                            tmpRobotArmInfo[0].ArmBackJobExist.ToString(), tmpRobotArmInfo[0].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[2].ArmFrontCSTSeq, tmpRobotArmInfo[2].ArmFrontJobSeq, tmpRobotArmInfo[2].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmBackCSTSeq, tmpRobotArmInfo[2].ArmBackJobSeq, tmpRobotArmInfo[2].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[2].ArmDisableFlag.ToString());

                                    #endregion

                                    #region [ Front Not Exist , Back Exist Log ]

                                    #region[DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) FrontJob is empty, BackJob CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}), but Robot Arm02 FrontJob({9},{10}) JobExist({11}), BackJob({12},{13}) JobExist({14}) ArmDisable({15}) and Arm03 FrontJob({16},{17}) JobExist({18}), BackJob({19},{20}) JobExist({21}) ArmDisable({22})!",
                                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME,
                                                                curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                                tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                                tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                                tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                                tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                                tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] FrontJob is empty, BackJob({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}), but Robot Arm02 FrontJob({7},{8}) JobExist({9}), BackJob({10},{11}) JobExist({12}) ArmDisable({13}) and Arm04 FrontJob({14},{15}) JobExist({16}), BackJob({17},{18}) JobExist({19}) ArmDisable({20})!",
                                                            MethodBase.GetCurrentMethod().Name, 
                                                            curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo, tmpBackStageID,
                                                            tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm, tmpRobotArmInfo[1].ArmFrontCSTSeq,
                                                            tmpRobotArmInfo[1].ArmFrontJobSeq, tmpRobotArmInfo[1].ArmFrontJobExist.ToString(), tmpRobotArmInfo[1].ArmBackCSTSeq, tmpRobotArmInfo[1].ArmBackJobSeq,
                                                            tmpRobotArmInfo[1].ArmBackJobExist.ToString(), tmpRobotArmInfo[1].ArmDisableFlag.ToString(),
                                                            tmpRobotArmInfo[3].ArmFrontCSTSeq, tmpRobotArmInfo[3].ArmFrontJobSeq, tmpRobotArmInfo[3].ArmFrontJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmBackCSTSeq, tmpRobotArmInfo[3].ArmBackJobSeq, tmpRobotArmInfo[3].ArmBackJobExist.ToString(),
                                                            tmpRobotArmInfo[3].ArmDisableFlag.ToString());

                                    #endregion

                                    errCode = eJobFilter_ReturnCode.NG_ArmJob_RouteUseArm_GlassNotExist;

                                    return false;
                                }

                                #endregion

                            }
                            else
                            {

                                #region [ Front and Back Job Not Exist ]

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get curSlotBlockInfo Front and Back jobInfo!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] can not Get curSlotBlockInfo Front and Back jobInfo!",
                                                        MethodBase.GetCurrentMethod().Name);

                                errCode = eJobFilter_ReturnCode.NG_SlotBlockInfo_Job_Is_Null;

                                return false;

                                #endregion
                            }

                            #endregion

                            break;

                        //case eDBRobotUseArmCode.ALL_ARM:  //設定不支援視同異常處理
                        //     break;

                        default:

                            if (curFrontJob != null)
                            {
                                //以FrontJob資訊為主
                                #region [ DB Setting Illegal ]

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Front Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) is illegal!",
                                                            curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                            tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction, tmpFrontJobStepUseArm);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] Front Job({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}) is illegal!",
                                                        MethodBase.GetCurrentMethod().Name, curFrontJob.CassetteSequenceNo, curFrontJob.JobSequenceNo,
                                                        tmpFrontStageID, tmpFrontStepNo.ToString(), tmpFrontJobStepAction, tmpFrontJobStepUseArm);

                                #endregion
                            }
                            else
                            {
                                //以BackJob資訊為主
                                #region [ DB Setting Illegal ]

                                #region[DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Back Job CassetteSequenceNo({3}) JobSequenceNo({4}) StageID({5}) StepNo({6}) setting Action({7}) UseArm({8}) is illegal!",
                                                            curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
                                                            tmpBackStageID, tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                errMsg = string.Format("[{0}] Back Job({1}_{2}) StageID({3}) StepNo({4}) setting Action({5}) UseArm({6}) is illegal!",
                                                        MethodBase.GetCurrentMethod().Name, curBackJob.CassetteSequenceNo, curBackJob.JobSequenceNo,
                                                        tmpBackStageID, tmpBackStepNo.ToString(), tmpBackJobStepAction, tmpBackJobStepUseArm);

                                #endregion

                            }

                            errCode = eJobFilter_ReturnCode.NG_StageJob_RouteUseArm_Setting_Fail;

                            return false;

                    }

                    #endregion

                }

                #endregion

                #region [ Set Robot UseArm To DefindNormalRobotCommand by is2ndCmdFlag ]

                if (is2ndCmdFlag == false)
                {
                    if (curFrontJob != null)
                    {
                        cur1stRobotCmd.Cmd01_ArmSelect = GetRobotUseArmCode(curRobot, curFrontJob, curAfterCheckUseArm);
                    }
                    else
                    {
                        cur1stRobotCmd.Cmd01_ArmSelect = GetRobotUseArmCode(curRobot, curBackJob, curAfterCheckUseArm);

                    }

                }
                else
                {
                    if (curFrontJob != null)
                    {
                        cur2ndRobotCmd.Cmd01_ArmSelect = GetRobotUseArmCode(curRobot, curFrontJob, curAfterCheckUseArm);
                    }
                    else
                    {
                        cur2ndRobotCmd.Cmd01_ArmSelect = GetRobotUseArmCode(curRobot, curBackJob, curAfterCheckUseArm);
                    }

                }

                #endregion

                #endregion


                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                errCode = eJobFilter_ReturnCode.NG_Exception;
                errMsg = ex.Message;

                return false;
            }

        }
        #endregion



        /// <summary>
        /// 回傳False 為Normal Job,True才是檢查到TTP 機台的Job有Daily Check EQP Flag!
        /// </summary>
        /// <param name="curBcsJob">要取放的WIP</param>
        /// <returns>False為Normal Job; True 為EQP Flag Job Daily Check Bit is ON!</returns>
        public bool Check_JobTTPDailyCheck_BitON(Job curBcsJob)
        {
            string fabType = string.Empty;
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string tempJobDataEQPFlag = string.Empty;

            try
            {
                #region 區分 Array or CF
                if (Workbench.ServerName.Length > 2)
                {
                    string prefix = Workbench.ServerName.Substring(0, 2).ToUpper();
                    switch (prefix)
                    {
                    case "TC":
                        fabType = eFabType.ARRAY.ToString();
                        break;
                    case "FC":
                        fabType = eFabType.CF.ToString();
                        break;
                    default:
                        //重要錯誤無法不顯示！
                        strlog = string.Format("ServerName({0})  Error !! Can not find FabType Trx Setting Value !", Workbench.ServerName);
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        return false;
                    }
                }
                #endregion

                #region [ Check Daily Check Flag]
                IDictionary<string, string> subItem = new Dictionary<string, string>();

                if (fabType == eFabType.ARRAY.ToString())
                    subItem = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPFLAG, "EQPFlag");
                else
                    subItem = ObjectManager.SubJobDataManager.Decode(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutEQPRESERVATIONS, "EQPReservations");


                if (subItem != null)
                {
                    #region GOTO SubChamber EQP Flag bit 'ON'
                    if (subItem.ContainsKey("ToTotalPitchSubChamber"))
                    {
                        if (subItem["ToTotalPitchSubChamber"] == ((int)eBitResult.OFF).ToString())
                        {
                            return false;
                        }
                        else
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] HAVE TTP DailyCheck, [EQUIPMENT={2}] ToTotalPitchSubChamber Bit ON!",
                                    curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.CurrentEQPNo));
                            }
                            #endregion
                            return true; //GOTO Stage 11 (TTP)
                        }
                    }
                    else
                    {
                        return false;
                    }
                    #endregion
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

        /// <summary>
        /// Watson add 20151219 For TTP DailyCheck Bit
        /// Get Trx EQ2EQ Interlock Bit
        /// </summary>
        /// <param name="curRobot"></param>
        /// <returns></returns>
        public eBitResult Check_TTP_EQInterlock_DailyCheckBit(Robot curRobot)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string fabType = string.Empty;
            try
            {
                string trxID = string.Empty;

                #region [ Get Line ]


                #endregion

                #region 區分 Array or CF
                if (curRobot.Data.SERVERNAME.Length > 2)
                {
                    string prefix = curRobot.Data.SERVERNAME.Substring(0, 2).ToUpper();
                    switch (prefix)
                    {
                    case "TC":
                        fabType = eFabType.ARRAY.ToString();
                        break;
                    case "FC":
                        fabType = eFabType.CF.ToString();
                        break;
                    default:
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ServerName({2})  Error!!  Can not find FabType Trx Setting Value!",
                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.SERVERNAME);
                        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        return eBitResult.OFF;
                    }
                }
                #endregion
                if (fabType == eFabType.ARRAY.ToString())
                {
                    #region Array TTP Line L3_EQtoEQInterlockSetForEQP#03 -> L3_B_DailyCheckRequest

                    try   //預防檔案內無此值
                    {
                        //取得SendOut的TrxID
                        trxID = ParameterManager[eParameterXMLConstant.TTP_DAILYCHECK_TRX].GetString();
                    }
                    catch (Exception ex)
                    { }

                    if (trxID == string.Empty)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ParameterManager Read Parameters.xml Error! can not find Array Line TTP_DAILYCHECK_TRX Setting Value ({2})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, eParameterXMLConstant.TTP_DAILYCHECK_TRX);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        trxID = "L3_EQtoEQInterlockSetForEQP#03";
                    }

                    #endregion
                }
                else
                {
                    #region CF MQCTTP_Line L3_EQInterlockSetForEQP#MQCDCRPitchtoIndexer -> L3_B_DailyCheckRequestTotalPitchtoIndexer

                    try   //預防檔案內無此值
                    {
                        //取得SendOut的TrxID
                        trxID = ParameterManager[eParameterXMLConstant.CF_TTP_DAILYCHECK_TRX].GetString();
                    }
                    catch (Exception ex)
                    { }

                    if (trxID == string.Empty)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ParameterManager Read Parameters.xml Error! can not find CF Line TTP_DAILYCHECK_TRX Setting Value ({2})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, eParameterXMLConstant.TTP_DAILYCHECK_TRX);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        trxID = "L3_EQInterlockSetForEQP#MQCDCRPitchtoIndexer";
                    }

                    #endregion
                }

                #region  real time Get Trx by EQtoEQInterlock
                Trx GetJobData_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                if (GetJobData_Trx == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not find TrxID({4})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return eBitResult.OFF;
                }
                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L3_EQtoEQInterlockSetForEQP#03" triggercondition="change">
                //  <eventgroup name="L3_EG_EQtoEQInterlockSetForEQP#03" dir="E2B">
                //    <event name="L3_B_DailyCheckRequest" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<itemgroup name="EQtoEQInterlockBlock">
                //  <item name="EquipmentStatustype" woffset="0" boffset="0" wpoints="0" bpoints="2" expression="BIT" />
                //  <item name="MaterialStatus" woffset="0" boffset="2" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="DailyCheckRequest" woffset="0" boffset="3" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#03" woffset="0" boffset="4" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#04" woffset="0" boffset="5" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#05" woffset="0" boffset="6" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="CleanerSendJobReserveforLocal#02" woffset="0" boffset="7" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="AbnormalForceCleanOut" woffset="0" boffset="8" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="LoadLock1CleanOut" woffset="0" boffset="9" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="LoadLock2CleanOut" woffset="0" boffset="10" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="ProductInfoNotify" woffset="0" boffset="11" wpoints="0" bpoints="1" expression="BIT" />
                //</itemgroup>

                string dailyCheck_Request = string.Empty;
                if (fabType == eFabType.ARRAY.ToString())
                {
                    dailyCheck_Request = GetJobData_Trx.EventGroups[0].Events[0].Items["DailyCheckRequest"].Value;
                }
                else
                {
                    dailyCheck_Request = GetJobData_Trx.EventGroups[0].Events[0].Items["DailyCheckRequestTotalPitchtoIndexer"].Value;
                }

                #endregion

                #endregion


                if (dailyCheck_Request == "1")
                {
                    return eBitResult.ON;
                }
                else
                {
                    return eBitResult.OFF;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return eBitResult.OFF;
            }

        }

        /// <summary>
        /// Sorter Mode, 檢查Job Grade與Unloader Mapping Grade
        /// </summary>
        /// <param name="L2"></param>
        /// <param name="ports"></param>
        /// <param name="stages"></param>
        /// <param name="job"></param>
        /// <param name="unloaderReady">有至少一個InProcess或WaitForProcess的ByGrade Unloader</param>
        /// <returns>與Job Grade相同的Unloader</returns>
        public List<PortStage> SorterMode_JobGradeUnloaderGrade(Equipment L2, List<Port> ports, List<RobotStage> stages, Job job, ref bool unloaderReady)
        {
            List<PortStage> mapping_ports = new List<PortStage>();
            foreach (Port port in ports)
            {
                if (port.File.Type != ePortType.UnloadingPort)
                    continue;
                if (port.File.Mode != UniBCS.Entity.ePortMode.ByGrade)
                    continue;
                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING)
                    continue;

                unloaderReady = true;

                // 當 EQ ProductTypeCheckMode 為 Enable, Job Grade 和 Port Grade 相同, 且 Job ProductType 和 Port ProductType 相同
                // 當 EQ ProductTypeCheckMode 為 Disable, 只需 Job Grade 和 Port Grade 相同
                if ((L2.File.ProductTypeCheckMode == eEnableDisable.Enable && job.JobGrade == port.File.MappingGrade && (string.IsNullOrEmpty(port.File.ProductType) || port.File.ProductType == "0" || job.ProductType.Value.ToString() == port.File.ProductType)) ||
                    (L2.File.ProductTypeCheckMode == eEnableDisable.Disable && job.JobGrade == port.File.MappingGrade))
                {
                    // 找到 Port 之後要確認 Port Stage 有 Empty Slot
                    foreach (RobotStage stage in stages)
                    {
                        if (stage.Data.STAGETYPE == eRobotStageType.PORT &&
                            stage.Data.STAGEID == port.Data.PORTNO &&
                            stage.curLDRQ_EmptySlotList.Count > 0)
                        {
                            mapping_ports.Add(new PortStage(port, stage));
                            break;
                        }
                    }
                }
            }
            return mapping_ports;
        }


        //20160126 add for Cell WaitCST Event Use
        /// <summary> 當Job CurStep or Check NextStep 為Put  To Port時,如果Filter條件不成立則要通知BCS Wait Cassette Event.當條件成立的時候則要清除Wait Cassette Event
        /// 
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="curRobot"></param>
        /// <param name="curBcsJob"></param>
        /// <param name="is2ndCmdFlag"></param>
        /// <param name="isClearFlag"></param>
        protected void Cell_SendWaitCassetteStatusToBCS(string funcName, Robot curRobot, Job curBcsJob, bool isClearFlag)
        {
            string strlog = string.Empty;

            try
            {
                string curTrxID = this.CreateTrxID();          

                #region [ Get Line Itme Check is Cell ]

                Line curline = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (curline == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Get Line entity by LineID({5})!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, Workbench.ServerName);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;

                }             

                if (curline.Data.FABTYPE != eFabType.CELL.ToString())
                {
                    //只有Cell 才需要送Wait CST Event To BCS
                    return;
                }

                #endregion

                #region [ 如果產生1stJob Cmd後 Check 2stJob的 Set Wait Cassette 則不需要Set 直接離開 ]

                int firstJobCommand = eRobot_Trx_CommandAction.ACTION_NONE;

                if (curRobot.Data.ARMJOBQTY != 2)
                {
                    //for Cell Normal
                    RobotCoreService.cur1stJob_CommandInfo cur1stCmdInfo = (RobotCoreService.cur1stJob_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stJob_CommandInfo];

                    if (cur1stCmdInfo != null)
                    {
                        firstJobCommand = cur1stCmdInfo.cur1stJob_Command.Cmd01_Command;
                    }
                }
                else
                {
                    //for Cell Special
                    RobotCoreService.cur1stSlotBlock_CommandInfo cur1stBlockCmdInfo = (RobotCoreService.cur1stSlotBlock_CommandInfo)curRobot.Context[eRobotContextParameter.Cur1stSlotBlock_CommandInfo];

                    if (cur1stBlockCmdInfo != null)
                    {
                        firstJobCommand = cur1stBlockCmdInfo.cur1stJob_Command.Cmd01_Command;
                    }

                }


                if (firstJobCommand != eRobot_Trx_CommandAction.ACTION_NONE && isClearFlag == false) 
                {

                    //如果是2nd Job Command 且是要Send Wait Cassette Event 則不需要通知BC Wait Cassette (因為會執行1st Job Command)                   
                    return;
                }

                #endregion

                #region [ Check CurStep Condition ]

                #region [ Get curStep Entity ]

                RobotRouteStep curRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.CurStepNo];

                //找不到 CurStep Route 回NG
                if (curRouteStep == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Get curRouteStep({5})!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return ;
                }

                #endregion

                if (curRouteStep.Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT)
                {

                    #region [ Check Check Step Target Stage Is Port ]

                    string[] stageList = curRouteStep.Data.STAGEIDLIST.Split(',');

                    for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                    {

                        #region [ Get StepStage Entity ]

                        RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);

                        if (curStepStage == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) curCheckStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5}) can not Send Wait Cassette!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curRouteStep.Data.STEPID,
                                                        curRouteStep.Data.STAGEIDLIST, stageList[stageIdx]);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //設定有問題則紀錄Log換下一個
                            continue;
                        }

                        #endregion

                        #region [ 判斷Current Step Stage 是否為Port Type ]

                        if (curStepStage.Data.STAGETYPE == eRobotStageType.PORT)
                        {

                            if (isClearFlag == true)
                            {

                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) curCheckStep({3}) StageID({4}) is Port and Send Clear Wait Cassette to BCS.",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curRouteStep.Data.STEPID,
                                                            curStepStage.Data.STAGEID);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                //通知BCS Clear Wait Cassette Event
                                Invoke(eServiceName.EquipmentService, "WaitCassetteStatusReportForRobot", new object[] { curTrxID, curRobot.Data.NODENO, (int)eWaitCassetteStatus.NotWaitCassette });
                                return;

                            }
                            else
                            {

                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) curCheckStep({3}) StageID({4}) is Port and Send Set Wait Cassette to BCS!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, curRouteStep.Data.STEPID,
                                                            curStepStage.Data.STAGEID);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                //通知BCS Wait Cassette
                                Invoke(eServiceName.EquipmentService, "WaitCassetteStatusReportForRobot", new object[] { curTrxID, curRobot.Data.NODENO, (int)eWaitCassetteStatus.W_CST });
                                return;
                            }

                        }

                        #endregion

                    }

                    #endregion

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteStep({5}) targetStageList({6}) is not PortType and can not Handle Send Wait Cassette.",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.STAGEIDLIST);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //CurRouteStep 是PUT但Target Stage不是PortType則結束判斷
                    return;
                }

                #region[DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteStep({5}) Action({6}) is not PUT can not Handle Send Wait Cassette.",
                                            curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString(), curRouteStep.Data.ROBOTACTION);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #endregion

                #region [ Check NextStep Condition ]

                #region [ Get NextStep Entity ]

                RobotRouteStep nextRouteStep = curBcsJob.RobotWIP.RobotRouteStepList[curBcsJob.RobotWIP.NextStepNo];

                //找不到 CurStep Route 回NG
                if (nextRouteStep == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) can not Get nextRouteStep({5})!",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;
                }

                #endregion

                if (nextRouteStep.Data.ROBOTACTION == eRobot_DB_CommandAction.ACTION_PUT)
                {

                    #region [ Check Check Step Target Stage Is Port ]

                    string[] stageList = nextRouteStep.Data.STAGEIDLIST.Split(',');

                    for (int stageIdx = 0; stageIdx < stageList.Length; stageIdx++)
                    {

                        #region [ Get StepStage Entity ]

                        RobotStage curStepStage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(stageList[stageIdx]);

                        if (curStepStage == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) curCheckStep({3}) can not Find RobotStage by StageNoList({4}) Stage({5}) can not Send Wait Cassette!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, nextRouteStep.Data.STEPID,
                                                        nextRouteStep.Data.STAGEIDLIST, stageList[stageIdx]);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //設定有問題則紀錄Log換下一個
                            continue;
                        }

                        #endregion

                        #region [ 判斷Current Step Stage 是否為Port Type ]

                        if (curStepStage.Data.STAGETYPE == eRobotStageType.PORT)
                        {

                            if (isClearFlag == true)
                            {

                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) curCheckStep({3}) StageID({4}) is Port and Send Clear Wait Cassette to BCS.",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, nextRouteStep.Data.STEPID,
                                                            curStepStage.Data.STAGEID);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                //通知BCS Clear Wait Cassette Event
                                Invoke(eServiceName.EquipmentService, "WaitCassetteStatusReportForRobot", new object[] { curTrxID, curRobot.Data.NODENO, (int)eWaitCassetteStatus.NotWaitCassette });
                                return;

                            }
                            else
                            {

                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) JobKey({2}) curCheckStep({3}) StageID({4}) is Port and Send Set Wait Cassette to BCS!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.JobKey, nextRouteStep.Data.STEPID,
                                                            curStepStage.Data.STAGEID);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                //通知BCS Wait Cassette
                                Invoke(eServiceName.EquipmentService, "WaitCassetteStatusReportForRobot", new object[] { curTrxID, curRobot.Data.NODENO, (int)eWaitCassetteStatus.W_CST });
                                return;
                            }

                        }

                        #endregion

                    }

                    #endregion

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) nextRouteStep({5}) TargetStageList({6}) is not Port Type and can not Handle Send Wait Cassette.",
                                                curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                                curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.NextStepNo.ToString(), nextRouteStep.Data.STAGEIDLIST);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //NextRouteStep 是PUT但Target Stage不是PortType則結束判斷
                    return;
                }

                #region[DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) curRouteStep({5}) Action({6}) is not PUT can not Handle Send Wait Cassette.",
                                            curRobot.Data.NODENO, funcName, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo,
                                            curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurStepNo.ToString(), nextRouteStep.Data.ROBOTACTION);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }


        }
        /// <summary>
        /// Add by Yang For CVD Chamber Clean LoadLock1CleanOut BitON->CVD only send out job 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <returns></returns>
        public eBitResult Check_CVD_EQInterLock_LoadLock1CleanOutBit(Robot curRobot)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string trxID1 = string.Empty;

            try
               {
                    #region [real time read trx only for CVD L4_EQtoEQInterlockSetForEQP#09]

                    try   //預防檔案內無此值
                    {
                        //取得SendOut的TrxID
                        trxID1 = ParameterManager[eParameterXMLConstant.CVD_LL1CLEANOUT_TRX].GetString();
                    }
                    catch (Exception ex)
                    { }

                    if (trxID1 == string.Empty)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ParameterManager Read Parameters.xml Error! can not find Array Line CVD_CHAMBERCLEAN_TRX Setting Value ({2})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, eParameterXMLConstant.CVD_LL1CLEANOUT_TRX);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                        trxID1= "L4_EQtoEQInterlockSetForEQP#09";
                    }

                    #endregion
                                

                #region  real time Get Trx by EQtoEQInterlock

                Trx GetLL1_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID1, false }) as Trx;

                  if (GetLL1_Trx == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not find TrxID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME,trxID1 );
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return eBitResult.OFF;
                }
                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L4_EQtoEQInterlockSetForEQP#09" triggercondition="change">
                //  <eventgroup name="L4_EG_EQtoEQInterlockSetForEQP#09" dir="E2B">
                //    <event name="L4_B_LoadLock1CleanOut" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<trx name="L4_EQtoEQInterlockSetForEQP#10" triggercondition="change">
                //  <eventgroup name="L4_EG_EQtoEQInterlockSetForEQP#09" dir="E2B">
                //    <event name="L4_B_LoadLock2CleanOut" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<itemgroup name="EQtoEQInterlockBlock">
                //  <item name="EquipmentStatustype" woffset="0" boffset="0" wpoints="0" bpoints="2" expression="BIT" />
                //  <item name="MaterialStatus" woffset="0" boffset="2" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="DailyCheckRequest" woffset="0" boffset="3" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#03" woffset="0" boffset="4" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#04" woffset="0" boffset="5" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#05" woffset="0" boffset="6" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="CleanerSendJobReserveforLocal#02" woffset="0" boffset="7" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="AbnormalForceCleanOut" woffset="0" boffset="8" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="LoadLock1CleanOut" woffset="0" boffset="9" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="LoadLock2CleanOut" woffset="0" boffset="10" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="ProductInfoNotify" woffset="0" boffset="11" wpoints="0" bpoints="1" expression="BIT" />
                //</itemgroup>

                #endregion

                  string LoadLock1CleanOut = string.Empty;
                if (curRobot.Data.LINETYPE.Contains("CVD_"))
                {
                    LoadLock1CleanOut = GetLL1_Trx.EventGroups[0].Events[0].Items["LoadLock1CleanOut"].Value;

                }
                #endregion


                if (LoadLock1CleanOut == "1")
                {
                    return eBitResult.ON;
                }
                else
                {
                    return eBitResult.OFF;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return eBitResult.OFF;
            }
        }


        /// <summary>
        /// Add by Yang For CVD Chamber Clean LoadLock1CleanOut BitON->CVD only send out job 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <returns></returns>
        public eBitResult Check_CVD_EQInterLock_LoadLock2CleanOutBit(Robot curRobot)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            string trxID2 = string.Empty;

            try
            {
                #region [real time read trx only for CVD L4_EQtoEQInterlockSetForEQP#10]

                try   //預防檔案內無此值
                {
                    //取得SendOut的TrxID
                    trxID2 = ParameterManager[eParameterXMLConstant.CVD_LL2CLEANOUT_TRX].GetString();
                }
                catch (Exception ex)
                { }

                if (trxID2 == string.Empty)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ParameterManager Read Parameters.xml Error! can not find Array Line CVD_CHAMBERCLEAN_TRX Setting Value ({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, eParameterXMLConstant.CVD_LL1CLEANOUT_TRX);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    trxID2 = "L4_EQtoEQInterlockSetForEQP#10";
                }

                #endregion


                #region  real time Get Trx by EQtoEQInterlock

                Trx GetLL2_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID2, false }) as Trx;

                if (GetLL2_Trx == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not find TrxID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, trxID2);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return eBitResult.OFF;
                }
                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L4_EQtoEQInterlockSetForEQP#09" triggercondition="change">
                //  <eventgroup name="L4_EG_EQtoEQInterlockSetForEQP#09" dir="E2B">
                //    <event name="L4_B_LoadLock1CleanOut" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<trx name="L4_EQtoEQInterlockSetForEQP#10" triggercondition="change">
                //  <eventgroup name="L4_EG_EQtoEQInterlockSetForEQP#09" dir="E2B">
                //    <event name="L4_B_LoadLock2CleanOut" trigger="true" />
                //  </eventgroup>
                //</trx>

                //<itemgroup name="EQtoEQInterlockBlock">
                //  <item name="EquipmentStatustype" woffset="0" boffset="0" wpoints="0" bpoints="2" expression="BIT" />
                //  <item name="MaterialStatus" woffset="0" boffset="2" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="DailyCheckRequest" woffset="0" boffset="3" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#03" woffset="0" boffset="4" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#04" woffset="0" boffset="5" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="IndexerSendJobReserveforLocal#05" woffset="0" boffset="6" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="CleanerSendJobReserveforLocal#02" woffset="0" boffset="7" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="AbnormalForceCleanOut" woffset="0" boffset="8" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="LoadLock1CleanOut" woffset="0" boffset="9" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="LoadLock2CleanOut" woffset="0" boffset="10" wpoints="0" bpoints="1" expression="BIT" />
                //  <item name="ProductInfoNotify" woffset="0" boffset="11" wpoints="0" bpoints="1" expression="BIT" />
                //</itemgroup>

                #endregion

                string LoadLock2CleanOut = string.Empty;
                if (curRobot.Data.LINETYPE.Contains("CVD_"))
                {
                    LoadLock2CleanOut = GetLL2_Trx.EventGroups[0].Events[0].Items["LoadLock2CleanOut"].Value;

                }
                #endregion


                if (LoadLock2CleanOut == "1")
                {
                    return eBitResult.ON;
                }
                else
                {
                    return eBitResult.OFF;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return eBitResult.OFF;
            }
        }

    }
}
