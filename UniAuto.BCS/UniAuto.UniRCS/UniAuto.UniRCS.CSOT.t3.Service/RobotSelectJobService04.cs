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
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class RobotSelectJobService
    {
        #region [ 20160104 add for Cell Special 1Arm2Job / Select_EQPTypeStage_For1Cmd_1Arm_2Job ]
        [UniAuto.UniBCS.OpiSpec.Help("SL0007")]
        public bool Select_EQPTypeStage_For1Cmd_1Arm_2Job(IRobotContext robotConText)
        {

            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curRobotStageList = null;
            List<RobotCanControlSlotBlockInfo> robotStageCanControlSlotBlockList;

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

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check Currnet Robot is Cell Special ]

                if (curRobot.Data.ARMJOBQTY != 2)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) ArmQty({3}) is not Cell Special!",
                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] ArmQty({1}) is not Cell Special!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_Robot_Is_Not_Cell_Special);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get CurRobot All Stage List ]

                curRobotStageList = (List<RobotStage>)robotConText[eRobotContextParameter.CurRobotAllStageListEntity];

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

                    errMsg = string.Format("[{0}] can not Get RobotStageInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_RobotStageList_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
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

                    errMsg = string.Format("[{0}] Get RobotStageInfo is Empty!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_RobotStageList_Is_Empty);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Current Stage Can Control SlotBlockInfo List ]

                robotStageCanControlSlotBlockList = (List<RobotCanControlSlotBlockInfo>)robotConText[eRobotContextParameter.StageCanControlSlotBlockInfoList]; ;

                //當取不到值時則要回NG
                if (robotStageCanControlSlotBlockList == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get StageCanControlSlotBlockInfoList entity!",
                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get StageCanControlSlotBlockInfoList entity!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_StageCanControlSlotBlockInfoList_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                foreach (RobotStage stage_entity in curRobotStageList)
                {
                    //非Stage Type Stage則不判斷
                    if (stage_entity.Data.STAGETYPE.ToUpper().Trim() != eRobotStageType.EQUIPMENT) continue;

                    Get_EqpTypeStageStatus(curRobot, stage_entity, robotStageCanControlSlotBlockList);
                }

                robotConText.AddParameter(eRobotContextParameter.StageCanControlJobList, robotStageCanControlSlotBlockList);
                robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eRobotSelectJob_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }
        private void Get_EqpTypeStageStatus(Robot curRobot, RobotStage curStage, List<RobotCanControlSlotBlockInfo> robotStageCanControlSlotBlockList)
        {
            string tmpStageStatus = string.Empty;

            try
            {
                //預設為NoReq
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                //1. Get Stage UDRQ Status and CanControlJobList
                Get_EqpTypeSingleSlot_CanControlJobList_For1Arm2Job(curRobot, curStage, robotStageCanControlSlotBlockList);

                //2. Get Stage LDRQ Status
                Get_EqpTypeSignal_LDRQStauts_For1Arm2Job(curRobot, curStage);

                //3. Judge Main Status by UDRQ & LDRQ Status
                JudgeEQPStage_UDRQ_LDRQStatus(curStage);
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        private void Get_EqpTypeSingleSlot_CanControlJobList_For1Arm2Job(Robot curRobot, RobotStage curStage, List<RobotCanControlSlotBlockInfo> robotStageCanControlSlotBlockList)
        {
            EqpStageInterfaceInfo _stage = null;

            try
            {
                _stage = new EqpStageInterfaceInfo(this, curRobot, curStage);
                _stage.robotStageCanControlSlotBlockList = robotStageCanControlSlotBlockList;
                _stage.RefreshUpstreamPathInfo(); //UDRQ 看上游 Linksignal
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void Get_EqpTypeSignal_LDRQStauts_For1Arm2Job(Robot curRobot, RobotStage curStage)
        {
            EqpStageInterfaceInfo _stage = null;

            try
            {
                _stage = new EqpStageInterfaceInfo(this, curRobot, curStage);
                _stage.RefreshDownstreamPathInfo(); //LDRQ 看下游 Linksignal
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private enum eInterfaceType
        {
            Unknown = 1,
            Upstream,
            Downstream
        }
        private class InterfaceInfo
        {
            private Trx _trx = null;
            private RobotStage _stage = null;
            private eInterfaceType _interfaceType = eInterfaceType.Unknown;
            private UniAuto.UniBCS.Log.ILogManager Logger = null;
            private string LogName = string.Empty;

            public InterfaceInfo(Trx _t, RobotStage _curStage, eInterfaceType _type, UniAuto.UniBCS.Log.ILogManager _logger, string _logName)
            {
                _trx = _t;
                _stage = _curStage;
                _interfaceType = _type;
                Logger = _logger;
                LogName = _logName;
                RefreshData();

            }

            #region [ Interface Declaration ]
            private bool _01_UpstreamInline_DownstreamInline = false;
            private bool _02_UpstreamTrouble_DownstreamTrouble = false;
            private bool _03_SendReady_ReceiveAble = false;
            private bool _04_Send_Receive = false;
            private bool _05_JobTransfer = false;
            private bool _06_SendCancel_ReceiveCancel = false;
            private bool _07_ExchangeExecute_ExchangePossible = false;
            private bool _08_DoubleGlass = false;
            private bool _09_SendJobReserve_ReceiveJobServe = false;
            private bool _10_SendOK_Spare_ReceiveOK = false;
            private bool _11_Spare_PinUpRequest_PinUpComplete_TransferStopRequest = false;
            private bool _12_Spare_PinDownComplete_PinDownRequest_DummyGlassRequest = false;
            private bool _13_Spare_GlassExist = false;
            private int _14_SlotNumber = 0;
            private int _15_GlassCount = 0;
            private bool _16_PanelPosition01 = false;
            private bool _16_PanelPosition02 = false;
            private bool _16_PanelPosition03 = false;
            private bool _16_PanelPosition04 = false;
            private bool _16_PanelPosition05 = false;
            private bool _16_PanelPosition06 = false;
            private bool _16_PanelPosition07 = false;
            private bool _16_PanelPosition08 = false;
            private System.Collections.Hashtable _htPancelPos = new System.Collections.Hashtable();
            #endregion

            private void RefreshData()
            {
                switch (_interfaceType)
                {
                    case eInterfaceType.Upstream:
                        #region Upstream
                        _01_UpstreamInline_DownstreamInline = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}UpstreamInline", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _02_UpstreamTrouble_DownstreamTrouble = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}UpstreamTrouble", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _03_SendReady_ReceiveAble = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}SendReady", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _04_Send_Receive = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}Send", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _05_JobTransfer = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}JobTransfer", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _06_SendCancel_ReceiveCancel = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}SendCancel", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _07_ExchangeExecute_ExchangePossible = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}ExchangeExecute", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _08_DoubleGlass = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}DoubleGlass", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);

                        if (_trx.EventGroups[0].Events[0].Items.AllKeys.Contains(string.Format("{0}SendJobReserve", _trx.Name.Split('_')[1].ToString())))
                        {
                            _09_SendJobReserve_ReceiveJobServe = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}SendJobReserve", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        }
                        else
                        {
                            _09_SendJobReserve_ReceiveJobServe = false;
                        }

                        _10_SendOK_Spare_ReceiveOK = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}SendOK", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);

                        if (_trx.EventGroups[0].Events[0].Items.AllKeys.Contains(string.Format("{0}PinUpRequest", _trx.Name.Split('_')[1].ToString())))
                        {
                            _11_Spare_PinUpRequest_PinUpComplete_TransferStopRequest = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PinUpRequest", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        }
                        else
                        {
                            _11_Spare_PinUpRequest_PinUpComplete_TransferStopRequest = false;
                        }

                        if (_trx.EventGroups[0].Events[0].Items.AllKeys.Contains(string.Format("{0}PinDownComplete", _trx.Name.Split('_')[1].ToString())))
                        {
                            _12_Spare_PinDownComplete_PinDownRequest_DummyGlassRequest = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PinDownComplete", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        }
                        else
                        {
                            _12_Spare_PinDownComplete_PinDownRequest_DummyGlassRequest = false;
                        }

                        _13_Spare_GlassExist = false;

                        try
                        {
                            _14_SlotNumber = Convert.ToInt16(string.Format("{5}{4}{3}{2}{1}{0}",
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#01", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#02", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#03", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#04", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#05", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#06", _trx.Name.Split('_')[1].ToString())].Value), 2);
                        }
                        catch
                        {
                            _14_SlotNumber = 0;
                        }
                        try
                        {
                            _15_GlassCount = Convert.ToInt16(string.Format("{4}{3}{2}{1}{0}",
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}GlassCount#01", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}GlassCount#02", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}GlassCount#03", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}GlassCount#04", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}GlassCount#05", _trx.Name.Split('_')[1].ToString())].Value), 2);
                        }
                        catch
                        {
                            _15_GlassCount = 0;
                        };


                        _htPancelPos.Clear();
                        _16_PanelPosition01 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#01", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(1, (_16_PanelPosition01 ? true : false));
                        _16_PanelPosition02 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#02", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(2, (_16_PanelPosition02 ? true : false));
                        _16_PanelPosition03 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#03", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(3, (_16_PanelPosition03 ? true : false));
                        _16_PanelPosition04 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#04", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(4, (_16_PanelPosition04 ? true : false));
                        _16_PanelPosition05 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#05", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(5, (_16_PanelPosition05 ? true : false));
                        _16_PanelPosition06 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#06", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(6, (_16_PanelPosition06 ? true : false));
                        _16_PanelPosition07 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#07", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(7, (_16_PanelPosition07 ? true : false));
                        _16_PanelPosition08 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#08", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(8, (_16_PanelPosition08 ? true : false));
                        #endregion
                        break;
                    case eInterfaceType.Downstream:
                        #region Downstream
                        _01_UpstreamInline_DownstreamInline = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}DownstreamInline", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _02_UpstreamTrouble_DownstreamTrouble = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}DownstreamTrouble", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _03_SendReady_ReceiveAble = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}ReceiveAble", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _04_Send_Receive = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}Receive", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _05_JobTransfer = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}JobTransfer", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _06_SendCancel_ReceiveCancel = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}ReceiveCancel", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _07_ExchangeExecute_ExchangePossible = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}ExchangePossible", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _08_DoubleGlass = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}DoubleGlass", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);

                        if (_trx.EventGroups[0].Events[0].Items.AllKeys.Contains(string.Format("{0}ReceiveJobServe", _trx.Name.Split('_')[1].ToString())))
                        {
                            _09_SendJobReserve_ReceiveJobServe = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}ReceiveJobServe", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        }
                        else
                        {
                            _09_SendJobReserve_ReceiveJobServe = false;
                        }

                        if (_trx.EventGroups[0].Events[0].Items.AllKeys.Contains(string.Format("{0}ReceiveOK", _trx.Name.Split('_')[1].ToString())))
                        {
                            _10_SendOK_Spare_ReceiveOK = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}ReceiveOK", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        }
                        else
                        {
                            _10_SendOK_Spare_ReceiveOK = false;
                        }

                        if (_trx.EventGroups[0].Events[0].Items.AllKeys.Contains(string.Format("{0}PinUpComplete", _trx.Name.Split('_')[1].ToString())))
                        {
                            _11_Spare_PinUpRequest_PinUpComplete_TransferStopRequest = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PinUpComplete", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        }
                        else if (_trx.EventGroups[0].Events[0].Items.AllKeys.Contains(string.Format("{0}TransferStopRequest", _trx.Name.Split('_')[1].ToString())))
                        {
                            _11_Spare_PinUpRequest_PinUpComplete_TransferStopRequest = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}TransferStopRequest", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        }
                        else
                        {
                            _11_Spare_PinUpRequest_PinUpComplete_TransferStopRequest = false;
                        }


                        if (_trx.EventGroups[0].Events[0].Items.AllKeys.Contains(string.Format("{0}PinDownRequest", _trx.Name.Split('_')[1].ToString())))
                        {
                            _12_Spare_PinDownComplete_PinDownRequest_DummyGlassRequest = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PinDownRequest", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        }
                        else if (_trx.EventGroups[0].Events[0].Items.AllKeys.Contains(string.Format("{0}DummyGlassRequest", _trx.Name.Split('_')[1].ToString())))
                        {
                            _12_Spare_PinDownComplete_PinDownRequest_DummyGlassRequest = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}DummyGlassRequest", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        }
                        else
                        {
                            _12_Spare_PinDownComplete_PinDownRequest_DummyGlassRequest = false;
                        }

                        if (_trx.EventGroups[0].Events[0].Items.AllKeys.Contains(string.Format("{0}GlassExist", _trx.Name.Split('_')[1].ToString())))
                        {
                            _13_Spare_GlassExist = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}GlassExist", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        }
                        else
                        {
                            _13_Spare_GlassExist = false;
                        }

                        try
                        {
                            _14_SlotNumber = Convert.ToInt16(string.Format("{5}{4}{3}{2}{1}{0}",
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#01", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#02", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#03", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#04", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#05", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}SlotNumber#06", _trx.Name.Split('_')[1].ToString())].Value), 2);
                        }
                        catch
                        {
                            _14_SlotNumber = 0;
                        }
                        try
                        {
                            _15_GlassCount = Convert.ToInt16(string.Format("{4}{3}{2}{1}{0}",
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}GlassCount#01", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}GlassCount#02", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}GlassCount#03", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}GlassCount#04", _trx.Name.Split('_')[1].ToString())].Value,
                                                    _trx.EventGroups[0].Events[0].Items[string.Format("{0}GlassCount#05", _trx.Name.Split('_')[1].ToString())].Value), 2);
                        }
                        catch
                        {
                            _15_GlassCount = 0;
                        }

                        _htPancelPos.Clear();
                        _16_PanelPosition01 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#01", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(1, (_16_PanelPosition01 ? true : false));
                        _16_PanelPosition02 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#02", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(2, (_16_PanelPosition02 ? true : false));
                        _16_PanelPosition03 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#03", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(3, (_16_PanelPosition03 ? true : false));
                        _16_PanelPosition04 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#04", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(4, (_16_PanelPosition04 ? true : false));
                        _16_PanelPosition05 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#05", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(5, (_16_PanelPosition05 ? true : false));
                        _16_PanelPosition06 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#06", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(6, (_16_PanelPosition06 ? true : false));
                        _16_PanelPosition07 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#07", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(7, (_16_PanelPosition07 ? true : false));
                        _16_PanelPosition08 = (_trx.EventGroups[0].Events[0].Items[string.Format("{0}PanelPosition#08", _trx.Name.Split('_')[1].ToString())].Value == "1" ? true : false);
                        _htPancelPos.Add(8, (_16_PanelPosition08 ? true : false));
                        #endregion
                        break;
                }
            }

            #region [ Interfce Properties - Upstream ]
            public bool UpstreamInline
            {
                get { return _01_UpstreamInline_DownstreamInline; }
            }
            public bool UpstreamTrouble
            {
                get { return _02_UpstreamTrouble_DownstreamTrouble; }
            }
            public bool UpstreamSendReady
            {
                get { return _03_SendReady_ReceiveAble; }
            }
            public bool UpstreamSend
            {
                get { return _04_Send_Receive; }
            }
            public bool UpstreamJobTransfer
            {
                get { return _05_JobTransfer; }
            }
            public bool UpstreamSendCancel
            {
                get { return _06_SendCancel_ReceiveCancel; }
            }
            public bool UpstreamExchangeExecute
            {
                get { return _07_ExchangeExecute_ExchangePossible; }
            }
            public bool UpstreamDoubleGlass
            {
                get { return _08_DoubleGlass; }
            }
            public bool UpstreamSendJobReserve
            {
                get { return _09_SendJobReserve_ReceiveJobServe; }
            }
            public bool UpstreamSendOK
            {
                get { return _10_SendOK_Spare_ReceiveOK; }
            }

            public bool UpstreamPinUpRequest
            {
                get { return _11_Spare_PinUpRequest_PinUpComplete_TransferStopRequest; }
            }
            public bool UpstreamPinDownComplete
            {
                get { return _12_Spare_PinDownComplete_PinDownRequest_DummyGlassRequest; }
            }

            public int UpstreamSlotNo
            {
                get { return _14_SlotNumber; }
            }
            public bool UpstreamPanelPos01
            {
                get { return _16_PanelPosition01; }
            }
            public bool UpstreamPanelPos02
            {
                get { return _16_PanelPosition02; }
            }
            public bool UpstreamPanelPos03
            {
                get { return _16_PanelPosition03; }
            }
            public bool UpstreamPanelPos04
            {
                get { return _16_PanelPosition04; }
            }
            public bool UpstreamPanelPos05
            {
                get { return _16_PanelPosition05; }
            }
            public bool UpstreamPanelPos06
            {
                get { return _16_PanelPosition06; }
            }
            public bool UpstreamPanelPos07
            {
                get { return _16_PanelPosition07; }
            }
            public bool UpstreamPanelPos08
            {
                get { return _16_PanelPosition08; }
            }
            #endregion
            #region [ Interfce Properties - Downstream ]
            public bool DownstreamInline
            {
                get { return _01_UpstreamInline_DownstreamInline; }
            }
            public bool DownstreamTrouble
            {
                get { return _02_UpstreamTrouble_DownstreamTrouble; }
            }
            public bool DownstreamReceiveAble
            {
                get { return _03_SendReady_ReceiveAble; }
            }
            public bool DownstreamReceive
            {
                get { return _04_Send_Receive; }
            }
            public bool DownstreamJobTransfer
            {
                get { return _05_JobTransfer; }
            }
            public bool DownstreamReceiveCancel
            {
                get { return _06_SendCancel_ReceiveCancel; }
            }
            public bool DownstreamExchangePossible
            {
                get { return _07_ExchangeExecute_ExchangePossible; }
            }
            public bool DownstreamDoubleGlass
            {
                get { return _08_DoubleGlass; }
            }
            public bool DownstreamReceiveJobReserve
            {
                get { return _09_SendJobReserve_ReceiveJobServe; }
            }

            public bool DownstreamReceiveOK
            {
                get { return _10_SendOK_Spare_ReceiveOK; }
            }

            public bool DownstreamPinUpComplete
            {
                get { return _11_Spare_PinUpRequest_PinUpComplete_TransferStopRequest; }
            }
            public bool DownstreamPinDownRequest
            {
                get { return _12_Spare_PinDownComplete_PinDownRequest_DummyGlassRequest; }
            }

            public bool DownstreamTransferStopRequest
            {
                get { return _11_Spare_PinUpRequest_PinUpComplete_TransferStopRequest; }
            }
            public bool DownstreamDummyGlassRequest
            {
                get { return _12_Spare_PinDownComplete_PinDownRequest_DummyGlassRequest; }
            }
            public bool DownstreamGlassExist
            {
                get { return _13_Spare_GlassExist; }
            }
            public int DownstreamSlotNo
            {
                get { return _14_SlotNumber; }
            }
            public int DownstreamGlassCount
            {
                get { return _15_GlassCount; }
            }
            public bool DownstreamPanelPos01
            {
                get { return _16_PanelPosition01; }
            }
            public bool DownstreamPanelPos02
            {
                get { return _16_PanelPosition02; }
            }
            public bool DownstreamPanelPos03
            {
                get { return _16_PanelPosition03; }
            }
            public bool DownstreamPanelPos04
            {
                get { return _16_PanelPosition04; }
            }
            public bool DownstreamPanelPos05
            {
                get { return _16_PanelPosition05; }
            }
            public bool DownstreamPanelPos06
            {
                get { return _16_PanelPosition06; }
            }
            public bool DownstreamPanelPos07
            {
                get { return _16_PanelPosition07; }
            }
            public bool DownstreamPanelPos08
            {
                get { return _16_PanelPosition08; }
            }
            #endregion

            #region [ Interface Method ]
            public bool AllPanelPosNoReq()
            {
                if (!_16_PanelPosition01 && !_16_PanelPosition02 && !_16_PanelPosition03 && !_16_PanelPosition04 &&
                    !_16_PanelPosition05 && !_16_PanelPosition06 && !_16_PanelPosition07 && !_16_PanelPosition08)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public List<int> GetCanUsePanelPosList()
            {
                List<int> _lst = new List<int>();
                _lst.Clear();

                try
                {
                    if (_16_PanelPosition01) _lst.Add(1);
                    if (_16_PanelPosition02) _lst.Add(2);
                    if (_16_PanelPosition03) _lst.Add(3);
                    if (_16_PanelPosition04) _lst.Add(4);
                    if (_16_PanelPosition05) _lst.Add(5);
                    if (_16_PanelPosition06) _lst.Add(6);
                    if (_16_PanelPosition07) _lst.Add(7);
                    if (_16_PanelPosition08) _lst.Add(8);
                }
                catch (Exception ex)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                    _lst.Clear();
                }
                return _lst;
            }
            public Dictionary<int, Dictionary<int,bool>> GetCanUseSlotBlockList()
            {
                Dictionary<int, Dictionary<int, bool>> _dic = new Dictionary<int, Dictionary<int, bool>>(); ;
                Dictionary<int, bool> _dicPanelPos = null;

                for (int i = 1; i <= _stage.Data.SLOTMAXCOUNT; i += 2)
                {
                    _dicPanelPos = new Dictionary<int, bool>();
                    _dicPanelPos.Clear();
                    _dicPanelPos.Add(i, (bool)_htPancelPos[i]);
                    _dicPanelPos.Add(i + 1, (bool)_htPancelPos[i + 1]);

                    _dic.Add(i, _dicPanelPos);
                }

                return _dic;
            }
            #endregion
        }
        private class EqpStageInterfaceInfo
        {
            private Robot _curRobot = null;
            private RobotStage _curStage = null;
            private RobotSelectJobService _service = null;

            List<RobotCanControlSlotBlockInfo> _robotStageCanControlSlotBlockList = null;

            private eFabType _fabType = eFabType.Unknown;

            public EqpStageInterfaceInfo(RobotSelectJobService _service, Robot _robot, RobotStage _stage)
            {
                this._service = _service;
                _curRobot = _robot;
                _curStage = _stage;
            }

            public void RefreshUpstreamPathInfo()
            {
                try
                {
                    if (_curStage.Data.UPSTREAMPATHTRXNAME == string.Empty)
                        return;

                    string _svLog = string.Empty;
                    string[] _upStreamTrxList = _curStage.Data.UPSTREAMPATHTRXNAME.Split(',');

                    string _trxId = string.Empty;
                    Trx _upStreamTrx = null;
                    InterfaceInfo _upstream = null;
                    for (int i = 0; i < _upStreamTrxList.Length; i++)
                    {
                        _trxId = _upStreamTrxList[i];

                        #region [ real time Get Interface Trx - Upstream ]
                        _upStreamTrx = _service.Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { _trxId, false }) as Trx;

                        _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot[{1}] Stage[ID={2}, Name={3}] ", _curStage.Data.NODENO, _curStage.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME);

                        if (_upStreamTrx == null)
                        {
                            #region  [DebugLog]
                            if (_service.IsShowDetialLog)
                            {
                                _svLog += string.Format("can not find TrxID[{0}]!", _trxId);
                                _service.Logger.LogWarnWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                            }
                            #endregion
                            continue;
                        }
                        #region [DebugLog]
                        if (_service.IsShowDetialLog)
                        {
                            _svLog += string.Format("get Trx by TrxID[{0}]!", _trxId);
                            _service.Logger.LogInfoWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                        }
                        #endregion
                        #endregion

                        _upstream = new InterfaceInfo(_upStreamTrx, _curStage, eInterfaceType.Upstream, _service.Logger, _service.LogName);


                        if (_upstream.UpstreamInline && _upstream.UpstreamSendReady && _upstream.UpstreamSend)
                        {
                            List<RobotCanControlSlotBlockInfo> _curSendOutSlotBlockList = null;

                            if (_upstream.AllPanelPosNoReq())
                            {
                                #region  [DebugLog]
                                if (_service.IsShowDetialLog)
                                {
                                    _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report all Panel Positions = 0!",
                                        _curStage.Data.NODENO, _curRobot.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME, _trxId);
                                    _service.Logger.LogWarnWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                                }
                                #endregion
                                return;
                            }
                            else
                            {
                                _curSendOutSlotBlockList = new List<RobotCanControlSlotBlockInfo>();
                                _curSendOutSlotBlockList.Clear();

                                RobotCanControlSlotBlockInfo _curSlotBlockInfo = null;
                                foreach (int _cmdSlotNo in _upstream.GetCanUseSlotBlockList().Keys)
                                {
                                    Dictionary<int, bool> _block = _upstream.GetCanUseSlotBlockList()[_cmdSlotNo];

                                    //_curSlotBlockInfo =  new RobotCanControlSlotBlockInfo();

                                    foreach (int _slotNo in _block.Keys)
                                    {
                                        bool _exist = (bool)_block[_slotNo];

                                        if (!_exist)
                                        {
                                            if (_curSlotBlockInfo != null && !_curSendOutSlotBlockList.Contains(_curSlotBlockInfo)) _curSendOutSlotBlockList.Add(_curSlotBlockInfo);
                                            _curSlotBlockInfo = null;
                                            continue;
                                        }

                                        string _JobsubLoc = (_slotNo % 2 == 0) ? eRobotCommonConst.ROBOT_ARM_BACK_LOCATION : eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION;

                                        if (!RefreshUpstreamJobDataPathInfo(_slotNo, _JobsubLoc, ref _curSlotBlockInfo))
                                        {
                                            //failed, debug log!!



                                        }
                                        else
                                        {
                                            if (_curSlotBlockInfo != null && !_curSendOutSlotBlockList.Contains(_curSlotBlockInfo)) _curSendOutSlotBlockList.Add(_curSlotBlockInfo);

                                        }


                                        //if (_JobsubLoc == eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION)
                                        //{
                                        //    if (_curSlotBlockInfo != null && !_curSendOutSlotBlockList.Contains(_curSlotBlockInfo)) _curSendOutSlotBlockList.Add(_curSlotBlockInfo);
                                        //    _curSlotBlockInfo = null;
                                        //    continue;
                                        //}



                                    }
                                }


                                if (_curSendOutSlotBlockList != null && _curSendOutSlotBlockList.Count > 0)
                                {
                                    _robotStageCanControlSlotBlockList.AddRange(_curSendOutSlotBlockList);
                                    #region [ curStage.curUDRQ_SlotBlockInfoList.Add ]

                                    //20150930 add for SendOut Job SlotNo
                                    lock (_curStage)
                                    {
                                        foreach (RobotCanControlSlotBlockInfo curSendOutSlotBlock in _curSendOutSlotBlockList)
                                        {
                                            //Check CmdSlotNo Exist
                                            if (_curStage.curUDRQ_SlotBlockInfoList.ContainsKey(curSendOutSlotBlock.CurBlock_RobotCmdSlotNo) == false)
                                            {
                                                Dictionary<int, string> curTmp2UDRQJobList = new Dictionary<int, string>();
                                                foreach (Job job in curSendOutSlotBlock.CurBlockCanControlJobList)
                                                {
                                                    switch (job.RobotWIP.CurSubLocation)
                                                    {
                                                        case eRobotCommonConst.ROBOT_ARM_BACK_LOCATION: curTmp2UDRQJobList.Add(2, job.JobKey); break;
                                                        case eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION: curTmp2UDRQJobList.Add(1, job.JobKey); break;
                                                    }
                                                }
                                                _curStage.curUDRQ_SlotBlockInfoList.Add(curSendOutSlotBlock.CurBlock_RobotCmdSlotNo, curTmp2UDRQJobList);
                                            }
                                        }
                                    }

                                    //Update Status UDRQ Stage Change To UDRQ
                                    _service.UpdateStage_UDRQ_Status_for1Arm2Job(_curStage, eRobotStageStatus.SEND_OUT_READY, MethodBase.GetCurrentMethod().Name);
                                    #endregion
                                }
                                else
                                {
                                    #region  [DebugLog]
                                    if (_service.IsShowDetialLog)
                                    {
                                        _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) can not get SendOut JobData, Stage UDRQ Status change from ({3}) to ({4})!",
                                            _curRobot.Data.NODENO, _curRobot.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.File.CurStageStatus, eRobotStageStatus.NO_REQUEST);
                                        _service.Logger.LogWarnWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                                    }
                                    #endregion
                                    if (_curRobot.File.curRobotRunMode == eRobot_RunMode.AUTO_MODE)
                                    {
                                        //無SendOut Job Info Status UDRQ Stage Change To NOREQ
                                        _service.UpdateStage_UDRQ_Status_for1Arm2Job(_curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name);
                                    }
                                    else
                                    {
                                        //SEMI Mode 如果找不到WIP還是視同可以出片.Update Status UDRQ Stage Change To UDRQ
                                        _service.UpdateStage_UDRQ_Status_for1Arm2Job(_curStage, eRobotStageStatus.SEND_OUT_READY, MethodBase.GetCurrentMethod().Name);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Monitor 条件不符合的状态 Status UDRQ Stage change to NOREQ
                            #region  [ DebugLog ]
                            if (_service.IsShowDetialLog)
                            {
                                _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}), Stage UDRQ Status can not change to (UDRQ)!",
                                    _curRobot.Data.NODENO, _curRobot.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME, _trxId, (_upstream.UpstreamInline ? "1" : "0"), (_upstream.UpstreamSendReady ? "1" : "0"), (_upstream.UpstreamSend ? "1" : "0"));
                                _service.Logger.LogWarnWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                            }
                            #endregion
                            _service.UpdateStage_UDRQ_Status(_curStage, eRobotStageStatus.NO_REQUEST, MethodBase.GetCurrentMethod().Name, string.Empty, string.Empty);
                        }
                    }
                }
                catch (Exception ex)
                {

                    _service.Logger.LogErrorWrite(_service.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
                
            }
            private bool RefreshUpstreamJobDataPathInfo(int _slotNo, string _jobSubLoc, ref RobotCanControlSlotBlockInfo _curSlotBlockInfo)
            {
                try
                {
                    string _svLog = string.Empty;
                    string[] _upStreamJobDataTrxList = _curStage.Data.UPSTREAMJOBDATAPATHTRXNAME.Split(',');

                    string _trxId = string.Empty;
                    //20160204 Modify mark
                    //int _slot = 0;
                    Trx _jobData = null;
                    //for (int i = 0; i < _upStreamJobDataTrxList.Length; i++)
                    //{
                    //_trxId = _upStreamJobDataTrxList[i];
                    //_slot = i + 1;
                    //20160204 Modify  SlotNo=2  在陣列中的位置要-1
                    _trxId = _upStreamJobDataTrxList[_slotNo - 1];
                    //20160204 Modify mark
                    //_slot = _slotNo + 1;

                    #region [ real time Get Interface Trx - Upstream ]
                    _jobData = _service.Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { _trxId, false }) as Trx;

                    _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot[{1}] Stage[ID={2}, Name={3}] ", _curStage.Data.NODENO, _curStage.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME);

                    if (_jobData == null)
                    {
                        #region  [DebugLog]
                        if (_service.IsShowDetialLog)
                        {
                            _svLog += string.Format("can not find TrxID[{0}]!", _trxId);
                            _service.Logger.LogWarnWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                        }
                        #endregion
                        return false;
                    }
                    #region [DebugLog]
                    if (_service.IsShowDetialLog)
                    {
                        _svLog += string.Format("get Trx by TrxID[{0}]!", _trxId);
                        _service.Logger.LogInfoWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                    }
                    #endregion
                    #endregion

                    #region [ Trx Structure ]
                    //<itemgroup name="JobData">
                    //  <item name="CassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //  <item name="JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //  <item name="GroupIndex" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //  <item name="ProductType" woffset="3" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //  <item name="CSTOperationMode" woffset="4" boffset="0" wpoints="1" bpoints="1" expression="INT" />
                    //  <item name="SubstrateType" woffset="4" boffset="1" wpoints="1" bpoints="2" expression="INT" />
                    //  <item name="CIMMode" woffset="4" boffset="3" wpoints="1" bpoints="1" expression="INT" />
                    //  <item name="JobType" woffset="4" boffset="4" wpoints="1" bpoints="4" expression="INT" />
                    //  <item name="JobJudge" woffset="4" boffset="8" wpoints="1" bpoints="4" expression="INT" />
                    //  <item name="SamplingSlotFlag" woffset="4" boffset="12" wpoints="1" bpoints="1" expression="INT" />
                    //  <item name="FirstRunFlag" woffset="4" boffset="15" wpoints="1" bpoints="1" expression="INT" />
                    //  <item name="JobGrade" woffset="5" boffset="0" wpoints="1" bpoints="16" expression="ASCII" />
                    //  <item name="Glass/Chip/MaskID/BlockID" woffset="6" boffset="0" wpoints="10" bpoints="160" expression="ASCII" />
                    //  <item name="PPID" woffset="16" boffset="0" wpoints="20" bpoints="320" expression="ASCII" />
                    //  <item name="INSPReservations" woffset="36" boffset="0" wpoints="1" bpoints="6" expression="BIN" />
                    //  <item name="EQPReservations" woffset="36" boffset="6" wpoints="1" bpoints="6" expression="BIN" />
                    //  <item name="LastGlassFlag" woffset="36" boffset="12" wpoints="1" bpoints="1" expression="BIN" />
                    //  <item name="Insp.JudgedData" woffset="37" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
                    //  <item name="TrackingData" woffset="39" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
                    //  <item name="EQPFlag" woffset="41" boffset="0" wpoints="2" bpoints="32" expression="BIN" />
                    //  <item name="ChipCount" woffset="43" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //  <item name="ProductID" woffset="44" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //  <item name="CassetteSettingCode" woffset="45" boffset="0" wpoints="2" bpoints="32" expression="ASCII" />
                    //  <item name="DotRepairCount" woffset="47" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //  <item name="LineRepairCount" woffset="48" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //  <item name="DefectCode" woffset="49" boffset="0" wpoints="3" bpoints="48" expression="ASCII" />
                    //</itemgroup>
                    //<itemgroup name="JobDataforCellCST">
                    //  <item name="CassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //  <item name="JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                    //</itemgroup>
                    #endregion

                    string cstSeq = _jobData.EventGroups[0].Events[0].Items["CassetteSequenceNo"].Value;
                    string jobSeq = _jobData.EventGroups[0].Events[0].Items["JobSequenceNo"].Value;
                    string sendOut_TrackingData = _jobData.EventGroups[0].Events[0].Items["TrackingData"].Value;
                    string sendOut_JobJudge = _jobData.EventGroups[0].Events[0].Items["JobJudge"].Value;
                    string sendOut_JobGrade = _jobData.EventGroups[0].Events[0].Items["JobGrade"].Value;

                    string _jobKey = string.Empty;
                    if (cstSeq != "0" && jobSeq != "0")
                    {
                        _jobKey = string.Format("{0}_{1}", cstSeq.ToString(), jobSeq.ToString());

                        #region  [DebugLog]
                        if (_service.IsShowDetialLog)
                        {
                            _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4}) ,CSTSeq({5}) JobSeq({6}) JobKey({7}).",
                                _curStage.Data.NODENO, _curStage.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME, _trxId, cstSeq, jobSeq, _jobKey);
                            _service.Logger.LogInfoWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                        }
                        #endregion

                        Job _job = ObjectManager.JobManager.GetJob(cstSeq, jobSeq);
                        #region [ Check Job is Exist ]
                        if (_job == null)
                        {
                            #region  [DebugLog]
                            if (_service.IsShowDetialLog)
                            {
                                _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RBMRCS Robot({1}) StageID({2}) StageName({3}) Can not Get Job by CSTSeq({4}) JobSeq({5})!",
                                    _curStage.Data.NODENO, _curStage.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME, cstSeq, jobSeq);
                                _service.Logger.LogWarnWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                            }
                            #endregion
                            if (_slotNo % 2 != 0) _slotNo++;
                            _curSlotBlockInfo = null;
                            return false;
                        }
                        #endregion

                        #region [ Update Job RobotWIP ]
                        //有變化才紀錄Log   LinkSignal要看Send Out JobData的TrackingData .Route Priority目前直接參照ROBOTWIP內Route資訊排序即可
                        if (_job.RobotWIP.CurLocation_StageID != _curStage.Data.STAGEID ||
                            _job.RobotWIP.CurLocation_SlotNo != _slotNo ||
                            _job.RobotWIP.CurLocation_StageType != eRobotStageType.EQUIPMENT ||
                            _job.RobotWIP.EqpReport_linkSignalSendOutTrackingData != sendOut_TrackingData ||
                            _job.RobotWIP.CurPortCstStatusPriority != eLoaderPortSendOutStatus.NOT_IN_PORT ||
                            _job.RobotWIP.CurSubLocation != _jobSubLoc ||
                            _job.RobotWIP.CurSendOutJobGrade != sendOut_JobGrade)
                        {
                            _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Job CassetteSequenceNo({1}) JobSequenceNo({2}) Update RobotWIP curStageType from ({3}) to ({4}), curStageID from ({5}) to ({6}), curSlotNo From ({7}) to ({8}), SendOut TrackingData from ({9}) to ({10}), PortCSTStatusPriority from ({11}) to ({12}),SendOut JobGrade from({13}) to ({14}).",
                                _curStage.Data.NODENO, _job.CassetteSequenceNo, _job.JobSequenceNo, _job.RobotWIP.CurLocation_StageType, eRobotStageType.EQUIPMENT,
                                _job.RobotWIP.CurLocation_StageID, _curStage.Data.STAGEID, _job.RobotWIP.CurLocation_SlotNo.ToString(), _slotNo.ToString(),
                                _job.RobotWIP.EqpReport_linkSignalSendOutTrackingData, sendOut_TrackingData, _job.RobotWIP.CurPortCstStatusPriority, eLoaderPortSendOutStatus.NOT_IN_PORT,
                                _job.RobotWIP.CurSendOutJobGrade, sendOut_JobGrade);
                            _service.Logger.LogInfoWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);

                            lock (_job)
                            {
                                _job.RobotWIP.CurLocation_StageID = _curStage.Data.STAGEID;
                                _job.RobotWIP.CurLocation_SlotNo = _slotNo;
                                _job.RobotWIP.CurLocation_StageType = eRobotStageType.EQUIPMENT;
                                _job.RobotWIP.EqpReport_linkSignalSendOutTrackingData = sendOut_TrackingData;
                                _job.RobotWIP.CurPortCstStatusPriority = eLoaderPortSendOutStatus.NOT_IN_PORT;
                                _job.RobotWIP.CurSubLocation = _jobSubLoc;
                                _job.RobotWIP.CurSendOutJobGrade = sendOut_JobGrade;
                            }

                            //Save File
                            ObjectManager.JobManager.EnqueueSave(_job);
                        }
                        #endregion
                        #region [ Update SlotBlockInfo ]
                        if (_curSlotBlockInfo == null)
                        {
                            _curSlotBlockInfo = new RobotCanControlSlotBlockInfo();
                            _curSlotBlockInfo.CurBlock_Location_StageID = _job.RobotWIP.CurLocation_StageID;
                            _curSlotBlockInfo.CurBlock_Location_StagePriority = _job.RobotWIP.CurLocation_StagePriority;
                            _curSlotBlockInfo.CurBlock_PortCstStatusPriority = _job.RobotWIP.CurPortCstStatusPriority;
                            _curSlotBlockInfo.CurBlock_RobotCmdSlotNo = (_slotNo % 2 == 0 ? _slotNo - 1 : _slotNo); //CurBlock_RobotCmdSlotNo是單數SlotNo
                            _curSlotBlockInfo.CurBlock_StepID = _job.RobotWIP.CurStepNo;
                            _curSlotBlockInfo.CurBlock_Location_StageType = _job.RobotWIP.CurLocation_StageType;
                        }
                        _curSlotBlockInfo.CurBlockCanControlJobList.Add(_job);
                        if (_curSlotBlockInfo.CurBlockCanControlJobList.Count == 1)
                        {
                            if (_slotNo % 2 == 0) _curSlotBlockInfo.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_EMPTY_BACK_EXIST;
                            else _curSlotBlockInfo.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_EXIST_BACK_EMPTY;
                        }
                        else if (_curSlotBlockInfo.CurBlockCanControlJobList.Count == 2)
                        {
                            _curSlotBlockInfo.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_BACK_EXIST;
                        }
                        #endregion
                    }
                    else
                    {
                        #region [DebugLog]
                        if (_service.IsShowDetialLog)
                        {
                            _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4}) ,CSTSeq({5}) JobSeq({6}) Job is not Exist!",
                                                    _curStage.Data.NODENO, _curStage.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME, _trxId, cstSeq, jobSeq);
                            _service.Logger.LogWarnWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                        }
                        #endregion
                        if (_slotNo % 2 != 0) _slotNo++;
                        _curSlotBlockInfo = null;
                        return false;
                    }
                    //}

                    return true;
                }
                catch (Exception ex)
                {

                    _service.Logger.LogErrorWrite(_service.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                    return false;
                }
                
            }

            public void RefreshDownstreamPathInfo()
            {
                try
                {
                    if (_curStage.Data.DOWNSTREAMPATHTRXNAME == string.Empty)
                        return;

                    string _svLog = string.Empty;
                    string[] _downStreamTrxList = _curStage.Data.DOWNSTREAMPATHTRXNAME.Split(',');

                    string _trxId = string.Empty;
                    Trx _downStreamTrx = null;
                    InterfaceInfo _downstream = null;
                    for (int i = 0; i < _downStreamTrxList.Length; i++)
                    {
                        _trxId = _downStreamTrxList[i];

                        #region [ real time Get Interface Trx - Downstream ]
                        _downStreamTrx = _service.Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { _trxId, false }) as Trx;

                        _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot[{1}] Stage[ID={2}, Name={3}] ", _curStage.Data.NODENO, _curStage.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME);

                        if (_downStreamTrx == null)
                        {
                            #region  [DebugLog]
                            if (_service.IsShowDetialLog)
                            {
                                _svLog += string.Format("can not find TrxID[{0}]!", _trxId);
                                _service.Logger.LogWarnWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                            }
                            #endregion
                            continue;
                        }
                        #region [DebugLog]
                        if (_service.IsShowDetialLog)
                        {
                            _svLog += string.Format("get Trx by TrxID[{0}]!", _trxId);
                            _service.Logger.LogInfoWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                        }
                        #endregion
                        #endregion

                        _downstream = new InterfaceInfo(_downStreamTrx, _curStage, eInterfaceType.Downstream, _service.Logger, _service.LogName);

                        if (_downstream.DownstreamInline && _downstream.DownstreamReceiveAble && !_downstream.DownstreamReceive)
                        {
                            lock (_curStage)
                            {
                                foreach (int _cmdSlotNo in _downstream.GetCanUseSlotBlockList().Keys) //基本上4个Forks, 所以应该最多只检查4次!!
                                {
                                    Dictionary<int, bool> _block = _downstream.GetCanUseSlotBlockList()[_cmdSlotNo];

                                    bool _exist = false; //是不是有基板在robot arm/fork上!! 不管 FRONT-SITE 或 BACK-SITE, 只要有存在就为 true
                                    foreach (int _slotNo in _block.Keys) //基本上, FRONT-SITE + BACK-SITE, 所以应该只会检查2次!!
                                    {
                                        _exist = (bool)_block[_slotNo];
                                        if (_exist) break; //如果有基板存在, 就不需要检查! 直接离开!!
                                    }
                                    if (_exist) continue; //有基板!
                                    //20160602
                                    _curStage.curLDRQ_EmptySlotBlockInfoList.Add(_cmdSlotNo, new CellSlotBlock("0", "0", "0", "0",0));
                                }
                            }

                            #region  [DebugLog]
                            if (_service.IsShowDetialLog)
                            {
                                _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveAble({5}) ExchangePossible({6}), Stage LDRQ Status change to (LDRQ).",
                                    _curRobot.Data.NODENO, _curRobot.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME, _trxId, (_downstream.DownstreamReceiveAble ? "ON" : "OFF"), (_downstream.DownstreamExchangePossible ? "ON" : "OFF"));
                                _service.Logger.LogInfoWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                            }
                            #endregion
                            //只須更新Stage LDRQ Status即可
                            if (_curStage.curLDRQ_EmptySlotBlockInfoList.Count > 0)
                            {
                                _service.UpdateStage_LDRQ_Status(_curStage, eRobotStageStatus.RECEIVE_READY, _curStage.File.LDRQ_CstStatusPriority, MethodBase.GetCurrentMethod().Name);
                            }
                            else
                            {
                                _service.UpdateStage_LDRQ_Status(_curStage, eRobotStageStatus.NO_REQUEST, _curStage.File.LDRQ_CstStatusPriority, MethodBase.GetCurrentMethod().Name);
                            }


                        }
                        else
                        {
                            //Monitor 條件不合的狀態 Status LDRQ Stage Change To NOREQ
                            #region  [DebugLog]
                            if (_service.IsShowDetialLog)
                            {
                                _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveAble({5}) ExchangePossible({6}), Stage LDRQ Status can not change to (LDRQ)!",
                                    _curRobot.Data.NODENO, _curRobot.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME, _trxId, (_downstream.DownstreamReceiveAble ? "ON" : "OFF"), (_downstream.DownstreamExchangePossible ? "ON" : "OFF"));
                                _service.Logger.LogWarnWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                            }
                            #endregion

                            //只須更新Stage LDRQ Status即可
                            _service.UpdateStage_LDRQ_Status(_curStage, eRobotStageStatus.NO_REQUEST, _curStage.File.LDRQ_CstStatusPriority, MethodBase.GetCurrentMethod().Name);
                        }

                        #region [ 更新 Stage 状态 Exchange Possible ]
                        #region [DebugLog]
                        if (_service.IsShowDetialLog)
                        {
                            _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5})!",
                                _curRobot.Data.NODENO, _curRobot.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME, _trxId, (_downstream.DownstreamExchangePossible ? "ON" : "OFF"));
                            _service.Logger.LogInfoWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                        }
                        #endregion
                        lock (_curStage.File)
                        {
                            _curStage.File.DownStreamExchangeReqFlag = _downstream.DownstreamExchangePossible;
                        }
                        #endregion
                        #region [ 更新 Stage 状态 Receive Able ]
                        #region [DebugLog]
                        if (_service.IsShowDetialLog)
                        {
                            _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveAble({5})!",
                                _curRobot.Data.NODENO, _curRobot.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME, _trxId, (_downstream.DownstreamReceiveAble ? "ON" : "OFF"));
                            _service.Logger.LogInfoWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                        }
                        #endregion
                        lock (_curStage.File)
                        {
                            _curStage.File.DownStreamReceiveAbleSignal = _downstream.DownstreamReceiveAble;
                        }
                        #endregion;
                        #region [ 更新 Stage 状态 Receive Job Reserve ]
                        #region [DebugLog]
                        if (_service.IsShowDetialLog)
                        {
                            _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveAble({5})!",
                                _curRobot.Data.NODENO, _curRobot.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME, _trxId, (_downstream.DownstreamReceiveJobReserve ? "ON" : "OFF"));
                            _service.Logger.LogInfoWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                        }
                        #endregion
                        lock (_curStage.File)
                        {
                            _curStage.File.DownStreamReceiveJobReserveSignal = _downstream.DownstreamReceiveJobReserve;
                        }
                        #endregion;
                        #region [ 更新 Stage 状态 Transfer Stop Request ]
                        #region  [DebugLog]
                        if (_service.IsShowDetialLog)
                        {
                            _svLog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) TransferStopRequest({5}),Update RobotStage Transfer Stop Request Flag(True)!",
                                _curRobot.Data.NODENO, _curRobot.Data.ROBOTNAME, _curStage.Data.STAGEID, _curStage.Data.STAGENAME, _trxId, (_downstream.DownstreamTransferStopRequest ? "ON" : "OFF"));
                            _service.Logger.LogInfoWrite(_service.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", _svLog);
                        }
                        #endregion
                        lock (_curStage.File)
                        {
                            _curStage.File.DownStreamTransferStopRequestFlag = _downstream.DownstreamTransferStopRequest;
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    _service.Logger.LogErrorWrite(_service.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }

            public eFabType FabType
            {
                get
                {
                    if (_fabType == eFabType.Unknown)
                    {
                        Line _line = ObjectManager.LineManager.GetLine(_curRobot.Data.LINEID);
                        switch (_line.Data.FABTYPE.ToString().ToUpper())
                        {
                            case "ARRAY": _fabType = eFabType.ARRAY; break;
                            case "CF": _fabType = eFabType.ARRAY; break;
                            case "CELL": _fabType = eFabType.ARRAY; break;
                            case "MODULE": _fabType = eFabType.ARRAY; break;
                        }
                    }
                    return _fabType;
                }
            }
            public List<RobotCanControlSlotBlockInfo> robotStageCanControlSlotBlockList
            {
                get { return _robotStageCanControlSlotBlockList; }
                set { _robotStageCanControlSlotBlockList = value; }
            }
        }
        #endregion


        /// <summary> Select All Robot Arm Can Control Job List by One Command control One Arm(Two Job)
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("SL0008")]
        public bool Select_AllRobotArm_for1Cmd_1Arm_2Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            //20160119 modify 不用New寫法
            List<RobotCanControlSlotBlockInfo> robotArmCanControlSlotBlockList;// = new List<RobotCanControlSlotBlockInfo>();
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

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                //20160119 modify 不用New寫法
                #region [ Get Current Stage Can Control SlotBlockInfo List ]

                robotArmCanControlSlotBlockList = (List<RobotCanControlSlotBlockInfo>)robotConText[eRobotContextParameter.ArmCanControlSlotBlockInfoList]; ;

                //當取不到值時則要回NG
                if (robotArmCanControlSlotBlockList == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get ArmCanControlSlotBlockInfoList entity!",
                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get ArmCanControlSlotBlockInfoList entity!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_ArmCanControlSlotBlockInfoList_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ 20151208 addBy RealTime ArmInfo list ArmNo Get Can Control Job List ]

                for (int armNoIndex = 0; armNoIndex < curRobot.CurTempArmDoubleJobInfoList.Length; armNoIndex++)
                {
                    RobotCanControlSlotBlockInfo slot_block_info = new RobotCanControlSlotBlockInfo();
                    int front_slot_no = ((armNoIndex * 2) + 1), back_slot_no = ((armNoIndex * 2) + 2);
                    if (curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmFrontJobExist == eGlassExist.Exist)
                    {
                        #region [ Get cur Job Entity ]

                        Job armFrontJob = ObjectManager.JobManager.GetJob(curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmFrontCSTSeq, curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmFrontJobSeq);

                        //找不到 BcsJob 回NG
                        if (armFrontJob == null)
                        {
                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob by ArmFront CSTSeq({2}),JobSeq({3})!",
                                                        "L1", MethodBase.GetCurrentMethod().Name, curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmFrontCSTSeq, curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmFrontJobSeq);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("[{0}] can not Get BcsJob by ArmFront CSTSeq({1}),JobSeq({2})!",
                                                    MethodBase.GetCurrentMethod().Name,
                                                    curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmFrontCSTSeq, curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmFrontJobSeq);

                            robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_curBcsJob_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion

                        #region [ 有變化才 Update JOB WIP Location Info ]

                        if (armFrontJob.RobotWIP.CurLocation_StageID != eRobotCommonConst.ROBOT_HOME_STAGEID ||
                            armFrontJob.RobotWIP.CurLocation_SlotNo != front_slot_no ||
                            armFrontJob.RobotWIP.CurLocation_StageType != eRobotStageType.ROBOTARM ||
                            armFrontJob.RobotWIP.CurSubLocation != eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ArmNo({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) Update RobotJobWIP curLocation StageID from ({5}) to ({6}) ,StageType for ({7}) to ({8}) ,SlotNo from ({9}) to ({10}).",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, (armNoIndex + 1).ToString(), armFrontJob.CassetteSequenceNo,
                                                                    armFrontJob.JobSequenceNo, armFrontJob.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID, armFrontJob.RobotWIP.CurLocation_StageType,
                                                                    eRobotStageType.ROBOTARM, armFrontJob.RobotWIP.CurLocation_SlotNo, front_slot_no);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            lock (armFrontJob)
                            {
                                armFrontJob.RobotWIP.CurLocation_StageID = eRobotCommonConst.ROBOT_HOME_STAGEID;
                                armFrontJob.RobotWIP.CurLocation_SlotNo = front_slot_no;
                                armFrontJob.RobotWIP.CurLocation_StageType = eRobotStageType.ROBOTARM;
                                armFrontJob.RobotWIP.CurSubLocation = eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION;
                            }

                            //Save File
                            ObjectManager.JobManager.EnqueueSave(armFrontJob);
                        }

                        #endregion

                        #region [ //20151209 add Check Froce Return CST Without LDRQ Status ]

                        if (armFrontJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status == eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_READY)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ArmNo({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) Update RobotJobWIP FroceReturnCSTWithoutLDRQ_Status from ({5}) to ({6}).",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, (armNoIndex + 1).ToString(), armFrontJob.CassetteSequenceNo,
                                                                    armFrontJob.JobSequenceNo, armFrontJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status, eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_START);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            lock (armFrontJob)
                            {
                                armFrontJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_START;
                                armFrontJob.RobotWIP.ForceReturnCSTWithoutLDRQ_MonitorStartTime = DateTime.Now;
                            }

                            //Save File
                            ObjectManager.JobManager.EnqueueSave(armFrontJob);
                        }

                        #endregion

                        //add To Arm CanControl Job List
                        slot_block_info.CurBlockCanControlJobList.Add(armFrontJob);
                        slot_block_info.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_EXIST_BACK_EMPTY;
                        slot_block_info.CurBlock_Location_StageID = armFrontJob.RobotWIP.CurLocation_StageID;
                        slot_block_info.CurBlock_Location_StagePriority = armFrontJob.RobotWIP.CurLocation_StagePriority;
                        slot_block_info.CurBlock_PortCstStatusPriority = armFrontJob.RobotWIP.CurPortCstStatusPriority;
                        slot_block_info.CurBlock_RobotCmdSlotNo = front_slot_no;
                        slot_block_info.CurBlock_StepID = armFrontJob.RobotWIP.CurStepNo;
                        //20160119 add SlotBlockInfo Stage Type
                        slot_block_info.CurBlock_Location_StageType = armFrontJob.RobotWIP.CurLocation_StageType;
                    }
                    if (curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmBackJobExist == eGlassExist.Exist)
                    {
                        #region [ Get cur Job Entity ]

                        Job armBackJob = ObjectManager.JobManager.GetJob(curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmBackCSTSeq, curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmBackJobSeq);

                        //找不到 BcsJob 回NG
                        if (armBackJob == null)
                        {

                            #region[DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get BcsJob by ArmBack CSTSeq({2}),JobSeq({3})!",
                                                        "L1", MethodBase.GetCurrentMethod().Name, curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmBackCSTSeq, curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmBackJobSeq);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            errMsg = string.Format("[{0}] can not Get BcsJob by ArmBack CSTSeq({1}),JobSeq({2})!",
                                                    MethodBase.GetCurrentMethod().Name,
                                                    curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmBackCSTSeq, curRobot.CurTempArmDoubleJobInfoList[armNoIndex].ArmBackJobSeq);

                            robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_curBcsJob_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion

                        #region [ 有變化才 Update JOB WIP Location Info ]

                        if (armBackJob.RobotWIP.CurLocation_StageID != eRobotCommonConst.ROBOT_HOME_STAGEID ||
                            armBackJob.RobotWIP.CurLocation_SlotNo != back_slot_no ||
                            armBackJob.RobotWIP.CurLocation_StageType != eRobotStageType.ROBOTARM ||
                            armBackJob.RobotWIP.CurSubLocation != eRobotCommonConst.ROBOT_ARM_BACK_LOCATION)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ArmNo({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) Update RobotJobWIP curLocation StageID from ({5}) to ({6}) ,StageType for ({7}) to ({8}) ,SlotNo from ({9}) to ({10}).",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, (armNoIndex + 1).ToString(), armBackJob.CassetteSequenceNo,
                                                                    armBackJob.JobSequenceNo, armBackJob.RobotWIP.CurLocation_StageID, eRobotCommonConst.ROBOT_HOME_STAGEID, armBackJob.RobotWIP.CurLocation_StageType,
                                                                    eRobotStageType.ROBOTARM, armBackJob.RobotWIP.CurLocation_SlotNo, back_slot_no);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            lock (armBackJob)
                            {
                                armBackJob.RobotWIP.CurLocation_StageID = eRobotCommonConst.ROBOT_HOME_STAGEID;
                                armBackJob.RobotWIP.CurLocation_SlotNo = back_slot_no;
                                armBackJob.RobotWIP.CurLocation_StageType = eRobotStageType.ROBOTARM;
                                armBackJob.RobotWIP.CurSubLocation = eRobotCommonConst.ROBOT_ARM_BACK_LOCATION;
                            }

                            //Save File
                            ObjectManager.JobManager.EnqueueSave(armBackJob);
                        }

                        #endregion

                        #region [ //20151209 add Check Froce Return CST Without LDRQ Status ]

                        if (armBackJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status == eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_READY)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) ArmNo({2}) Job CassetteSequenceNo({3}) JobSequenceNo({4}) Update RobotJobWIP FroceReturnCSTWithoutLDRQ_Status from ({5}) to ({6}).",
                                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, (armNoIndex + 1).ToString(), armBackJob.CassetteSequenceNo,
                                                                    armBackJob.JobSequenceNo, armBackJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status, eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_START);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            lock (armBackJob)
                            {
                                armBackJob.RobotWIP.ForceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_START;
                                armBackJob.RobotWIP.ForceReturnCSTWithoutLDRQ_MonitorStartTime = DateTime.Now;
                            }

                            //Save File
                            ObjectManager.JobManager.EnqueueSave(armBackJob);
                        }

                        #endregion

                        //add To Arm CanControl Job List
                        slot_block_info.CurBlockCanControlJobList.Add(armBackJob);
                        if (slot_block_info.CurBlock_JobExistStatus == eRobot_SlotBlock_JobsExistStatus.FRONT_EXIST_BACK_EMPTY)
                            slot_block_info.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_BACK_EXIST;
                        else
                        {

                            slot_block_info.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_EMPTY_BACK_EXIST;
                            slot_block_info.CurBlock_Location_StageID = armBackJob.RobotWIP.CurLocation_StageID;
                            slot_block_info.CurBlock_Location_StagePriority = armBackJob.RobotWIP.CurLocation_StagePriority;
                            slot_block_info.CurBlock_PortCstStatusPriority = armBackJob.RobotWIP.CurPortCstStatusPriority;
                            slot_block_info.CurBlock_RobotCmdSlotNo = front_slot_no;
                            slot_block_info.CurBlock_StepID = armBackJob.RobotWIP.CurStepNo;
                            //20160119 add SlotBlockInfo Stage Type
                            slot_block_info.CurBlock_Location_StageType = armBackJob.RobotWIP.CurLocation_StageType;

                        }
                    }
                    if (slot_block_info.CurBlock_JobExistStatus != eRobot_SlotBlock_JobsExistStatus.FRONT_BACK_EMPTY)
                    {
                        robotArmCanControlSlotBlockList.Add(slot_block_info);
                    }
                }

                #endregion

                //20160119 modify 不用New寫法 不用回傳值
                //robotConText.AddParameter(eRobotContextParameter.ArmCanControlSlotBlockInfoList, robotArmCanControlSlotBlockList);
                robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eRobotSelectJob_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("SL0006")]
        public bool Select_StageTypeStage_For1Cmd_1Arm_2Job(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            List<RobotStage> curRobotStageList = null;
            List<RobotCanControlSlotBlockInfo> robotStageCanControlSlotBlockList;
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

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Check Currnet Robot is Cell Special ]

                if (curRobot.Data.ARMJOBQTY != 2)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) ArmQty({3}) is not Cell Special!",
                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.ARMJOBQTY.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] ArmQty({1}) is not Cell Special!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ARMJOBQTY.ToString());

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_Robot_Is_Not_Cell_Special);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Get CurRobot All Stage List ]

                curRobotStageList = (List<RobotStage>)robotConText[eRobotContextParameter.CurRobotAllStageListEntity];

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

                    errMsg = string.Format("[{0}] can not Get RobotStageInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_RobotStageList_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
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

                    errMsg = string.Format("[{0}] Get RobotStageInfo is Empty!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_RobotStageList_Is_Empty);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Current Stage Can Control SlotBlockInfo List ]

                robotStageCanControlSlotBlockList = (List<RobotCanControlSlotBlockInfo>)robotConText[eRobotContextParameter.StageCanControlSlotBlockInfoList];

                //當取不到值時則要回NG
                if (robotStageCanControlSlotBlockList == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get StageCanControlSlotBlockInfoList entity!",
                                                curRobot.Data.NODENO, MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get StageCanControlSlotBlockInfoList entity!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Get_StageCanControlSlotBlockInfoList_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                foreach (RobotStage stage_entity in curRobotStageList)
                {

                    //非Stage Type Stage則不判斷
                    if (stage_entity.Data.STAGETYPE.ToUpper().Trim() != eRobotStageType.STAGE) continue;

                    Get_StageTypeStageStatus(curRobot, stage_entity, robotStageCanControlSlotBlockList);
                }

                //[ Wait_Proc_0009 ] 後續處理
                robotConText.AddParameter(eRobotContextParameter.StageCanControlJobList, robotStageCanControlSlotBlockList);
                robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eRobotSelectJob_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {

                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eRobotSelectJob_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }

        /// <summary>Get Singal/Mulit Slot Stage Type Stage Status
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        private void Get_StageTypeStageStatus(Robot curRobot, RobotStage curStage, List<RobotCanControlSlotBlockInfo> robotStageCanControlSlotBlockList)
        {
            string tmpStageStatus = string.Empty;

            try
            {
                //預設為NoReq
                tmpStageStatus = eRobotStageStatus.NO_REQUEST;

                //1. Get Stage UDRQ Status and CanControlJobList
                Get_StageTypeSingleSlot_CanControlJobList_For1Arm2Job(curRobot, curStage, robotStageCanControlSlotBlockList);

                //2. Get Stage LDRQ Status
                Get_StageTypeSignal_LDRQStauts_For1Arm2Job(curRobot, curStage);

                //3. Judge Main Status by UDRQ & LDRQ Status
                JudgeIndexerStage_UDRQ_LDRQStatus(curStage);

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary> 取得Stage Type Stage,Slot is Single Can ControlJobList for 1 Arm 1Job Use . 
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        private void Get_StageTypeSingleSlot_CanControlJobList_For1Arm2Job(Robot curRobot, RobotStage curStage, List<RobotCanControlSlotBlockInfo> robotStageCanControlSlotBlockList)
        {
            string trxID = string.Empty;
            string strlog = string.Empty;

            try
            {
                string bitOn = "1";
                string funcName = MethodBase.GetCurrentMethod().Name;

                #region [ Check Trx Setting ]
                if (curStage.Data.STAGEREPORTTRXNAME.Trim() == string.Empty)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find STAGEREPORTTRXNAME setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return;
                }
                #endregion

                //interface時Stage出片要看Upstream .透過LinkSignal已經得知JobData是否填好 Send Signal On =JobData Exist
                string upStreamTrx = curStage.Data.STAGEREPORTTRXNAME;

                #region  real time Get Interface Upstream

                trxID = upStreamTrx;

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
                    return;
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
                //<trx name="L2_Stage#01SinglePositionReport" triggercondition="change">
                //    <eventgroup name="L2_EG_Stage#01SinglePositionReport" dir="E2B">
                //      <event name="L2_B_Stage#01SendReady" trigger="true" />
                //      <event name="L2_B_Stage#01ReceiveReady" trigger="true" />
                //      <event name="L2_B_Stage#01DoubleGlassExist" trigger="true" />
                //      <event name="L2_B_Stage#01ExchangePossible" trigger="true" />
                //    </eventgroup>
                //</trx>
                #endregion
                #region [variable declare]
                string SendReady = upStream_Trx.EventGroups[0].Events[0].Items[0].Value;
                string ReceiveReady = upStream_Trx.EventGroups[0].Events[1].Items[0].Value;
                string DoubleGlassExist = upStream_Trx.EventGroups[0].Events[2].Items[0].Value;
                string ExchangePossible = upStream_Trx.EventGroups[0].Events[3].Items[0].Value;
                #endregion
                #endregion

                if (SendReady == bitOn || ExchangePossible == bitOn)
                {
                    List<RobotCanControlSlotBlockInfo> curSendOutSlotBlockList = null;
                    #region  [ 取得JobData ]
                    Get_StageSendOutJobInfo_For1Arm2Job(curRobot, curStage, out curSendOutSlotBlockList);
                    if (curSendOutSlotBlockList != null && curSendOutSlotBlockList.Count > 0)
                    {
                        robotStageCanControlSlotBlockList.AddRange(curSendOutSlotBlockList);
                        #region [ curStage.curUDRQ_SlotBlockInfoList.Add ]

                        //20150930 add for SendOut Job SlotNo
                        lock (curStage)
                        {
                            foreach (RobotCanControlSlotBlockInfo curSendOutSlotBlock in curSendOutSlotBlockList)
                            {
                                //Check CmdSlotNo Exist
                                if (curStage.curUDRQ_SlotBlockInfoList.ContainsKey(curSendOutSlotBlock.CurBlock_RobotCmdSlotNo) == false)
                                {
                                    Dictionary<int, string> curTmp2UDRQJobList = new Dictionary<int, string>();
                                    foreach (Job job in curSendOutSlotBlock.CurBlockCanControlJobList)
                                    {
                                        switch (job.RobotWIP.CurSubLocation)
                                        {
                                        case eRobotCommonConst.ROBOT_ARM_BACK_LOCATION: curTmp2UDRQJobList.Add(2, job.JobKey); break;
                                        case eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION: curTmp2UDRQJobList.Add(1, job.JobKey); break;
                                        }
                                    }
                                    curStage.curUDRQ_SlotBlockInfoList.Add(curSendOutSlotBlock.CurBlock_RobotCmdSlotNo, curTmp2UDRQJobList);
                                }
                            }
                        }

                        //Update Status UDRQ Stage Change To UDRQ
                        UpdateStage_UDRQ_Status_for1Arm2Job(curStage, eRobotStageStatus.SEND_OUT_READY, funcName);

                        #endregion
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

                        if (curRobot.File.curRobotRunMode == eRobot_RunMode.AUTO_MODE)
                        {
                            //無SendOut Job Info Status UDRQ Stage Change To NOREQ
                            UpdateStage_UDRQ_Status_for1Arm2Job(curStage, eRobotStageStatus.NO_REQUEST, funcName);
                        }
                        else
                        {
                            //SEMI Mode 如果找不到WIP還是視同可以出片.Update Status UDRQ Stage Change To UDRQ
                            UpdateStage_UDRQ_Status_for1Arm2Job(curStage, eRobotStageStatus.SEND_OUT_READY, funcName);
                        }
                    }
                    #endregion
                }
                else
                {
                    //Monitor 條件不合的狀態 Status UDRQ Stage Change To NOREQ
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        //strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}), Stage UDRQ Status can not change to (UDRQ)!",
                        //curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                        //trxID, (eBitResult)int.Parse(up_UpstreamInline), (eBitResult)int.Parse(up_SendReady), (eBitResult)int.Parse(up_Send));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    UpdateStage_UDRQ_Status_for1Arm2Job(curStage, eRobotStageStatus.NO_REQUEST, funcName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void Get_StageTypeSignal_LDRQStauts_For1Arm2Job(Robot curRobot, RobotStage curStage)
        {
            string trxID = string.Empty;
            string strlog = string.Empty;
            try
            {
                string bitOn = "1";
                string funcName = MethodBase.GetCurrentMethod().Name;

                #region [ Check Trx Setting ]

                if (curStage.Data.STAGEREPORTTRXNAME.Trim() == string.Empty)
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

                #region  real time Get Stage LDRQ

                trxID = curStage.Data.STAGEREPORTTRXNAME.Trim();

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

                    return;
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

                //<trx name="L2_Stage#01SinglePositionReport" triggercondition="change">
                //    <eventgroup name="L2_EG_Stage#01SinglePositionReport" dir="E2B">
                //      <event name="L2_B_Stage#01SendReady" trigger="true" />
                //      <event name="L2_B_Stage#01ReceiveReady" trigger="true" />
                //      <event name="L2_B_Stage#01DoubleGlassExist" trigger="true" />
                //      <event name="L2_B_Stage#01ExchangePossible" trigger="true" />
                //    </eventgroup>
                //</trx>

                //<event name="L2_B_Stage#01SendReady" devicecode="B" address="0x0000CEC" points="1">
                //  <item name="Stage#01SendReady" offset="0" points="1" expression="BIT" />
                //</event>

                #endregion
                #region [variable declare]
                string down_SendReady = downStream_Trx.EventGroups[0].Events[0].Items[0].Value;
                string down_ReceiveReady = downStream_Trx.EventGroups[0].Events[1].Items[0].Value;
                string down_DoubleGlassExist = downStream_Trx.EventGroups[0].Events[2].Items[0].Value;
                string down_ExchangePossible = downStream_Trx.EventGroups[0].Events[3].Items[0].Value;
                #endregion
                #endregion

                //Stage LDRQ
                if (down_ReceiveReady == bitOn)
                {
                    #region [ Check Trx Setting ]

                    if (curStage.Data.STAGEJOBDATATRXNAME.Trim() == string.Empty)
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

                    #region  real time Get Stage JobKey and JobExist
                    trxID = curStage.Data.STAGEJOBDATATRXNAME.Trim();
                    Trx downStreamPos_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                    if (downStreamPos_Trx == null)
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
                        return;
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
                    string[,] each_pos = new string[4, 3];
                    each_pos[0, 0] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkFrontEndStageJobCassetteSequenceNumber"].Value;
                    each_pos[0, 1] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkFrontEndStageJobSlotSequenceNumber"].Value;
                    each_pos[0, 2] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkFrontEndStageJobExist"].Value;
                    each_pos[1, 0] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkBackEndStageJobCassetteSequenceNumber"].Value;
                    each_pos[1, 1] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkBackEndStageJobSlotSequenceNumber"].Value;
                    each_pos[1, 2] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkBackEndStageJobExist"].Value;
                    each_pos[2, 0] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkFrontEndStageJobCassetteSequenceNumber"].Value;
                    each_pos[2, 1] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkFrontEndStageJobSlotSequenceNumber"].Value;
                    each_pos[2, 2] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkFrontEndStageJobExist"].Value;
                    each_pos[3, 0] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkBackEndStageJobCassetteSequenceNumber"].Value;
                    each_pos[3, 1] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkBackEndStageJobSlotSequenceNumber"].Value;
                    each_pos[3, 2] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkBackEndStageJobExist"].Value;
                    #endregion

                    lock (curStage)
                    {
                        for (int cmdSlotNo = 1; cmdSlotNo <= curStage.Data.SLOTMAXCOUNT; cmdSlotNo += 2)
                        {
                            if (each_pos[cmdSlotNo - 1, 0] == "0"  &&
                                each_pos[cmdSlotNo - 1, 1] == "0" &&
                                each_pos[cmdSlotNo - 1, 2] == "1" &&//1:NoExist, 2:Exist
                                each_pos[cmdSlotNo, 0] == "0" &&
                                each_pos[cmdSlotNo, 1] == "0" &&
                                each_pos[cmdSlotNo, 2] == "1")//1:NoExist, 2:Exist
                            {
                                //Front & Back 都是Empty才能收片
                                //20160602
                                curStage.curLDRQ_EmptySlotBlockInfoList.Add(cmdSlotNo, new CellSlotBlock("0", "0", "0", "0",0));
                            }
                        }
                    }
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveReady({5}) ExchangePossible({6}), Stage LDRQ Status change to (LDRQ).",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, (eBitResult)int.Parse(down_ReceiveReady), (eBitResult)int.Parse(down_ExchangePossible));
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //只須更新Stage LDRQ Status即可
                    if (curStage.curLDRQ_EmptySlotBlockInfoList.Count > 0)
                        UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.RECEIVE_READY, curStage.File.LDRQ_CstStatusPriority, funcName);
                    else
                        UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, curStage.File.LDRQ_CstStatusPriority, funcName);
                }
                else
                {
                    //Monitor 條件不合的狀態 Status LDRQ Stage Change To NOREQ
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveReady({5}) ExchangePossible({6}), Stage LDRQ Status can not change to (LDRQ)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, (eBitResult)int.Parse(down_ReceiveReady), (eBitResult)int.Parse(down_ExchangePossible));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //只須更新Stage LDRQ Status即可
                    UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, curStage.File.LDRQ_CstStatusPriority, funcName);

                }

                #region [ 如果Exchange Possible On 則要更新Stage狀態 ]

                if (down_ExchangePossible == bitOn)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Exchange Request Flag(True)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, eBitResult.ON.ToString());
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                    //CELL Unit Stage不支援Exchange
                    //lock (curStage.File)
                    //{
                    //    curStage.File.DownStreamExchangeReqFlag = true;
                    //}
                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Exchange Request Flag(False)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamExchangeReqFlag = false;
                    }

                }

                #endregion

                #region [ 如果 Receive Able On(refrence ReceiveReady if stage is indexer's inner stage) 則要更新Stage狀態 ]

                if (down_ReceiveReady == bitOn)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveReady({5}),Update RobotStage Receive Able Signal(True)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, eBitResult.ON.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamReceiveAbleSignal = true;
                    }

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ReceiveReady({5}),Update RobotStage Receive Able Signal(False)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamReceiveAbleSignal = false;
                    }

                }

                #endregion;

                #region [ 如果 Receive Job Reserve On 則要更新Stage狀態 ]
                if (down_ExchangePossible == bitOn)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Receive Job Reserve Signal(True)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, eBitResult.ON.ToString());
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    //CELL Unit Stage不支援Exchange
                    //lock (curStage.File)
                    //{
                    //    curStage.File.DownStreamReceiveJobReserveSignal = true;
                    //}
                }
                else
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) ExchangePossible({5}),Update RobotStage Receive Job Reserve Signal(False)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    lock (curStage.File)
                    {
                        curStage.File.DownStreamReceiveJobReserveSignal = false;
                    }
                }
                #endregion;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> 取得Stage SendOut的 JobData for Stage is Single Slot
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// <param name="curStageCanControlJobList"></param>
        /// <param name="sendOutSlotNo"></param>
        /// <param name="curSendOutJob"></param>
        /// <returns></returns>
        private void Get_StageSendOutJobInfo_For1Arm2Job(Robot curRobot, RobotStage curStage, out List<RobotCanControlSlotBlockInfo> curSendOutSlotBlockList)
        {
            string strlog = string.Empty;
            string jobKey = string.Empty;
            curSendOutSlotBlockList = new List<RobotCanControlSlotBlockInfo>();
            try
            {
                string funcName = MethodBase.GetCurrentMethod().Name;

                #region [ Check Trx Setting Exist ]

                if (curStage.Data.STAGEJOBDATATRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) can not find SendOutJobData TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                    return;
                }

                //取得SendOut的TrxID
                string trxID = curStage.Data.STAGEJOBDATATRXNAME.Trim();
                if (trxID == string.Empty)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) can not find SendOutJobData TrxID({4})!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                curStage.Data.UPSTREAMJOBDATAPATHTRXNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return;
                }

                #endregion

                #region  real time Get Stage JobKey and JobExist
                trxID = curStage.Data.STAGEJOBDATATRXNAME.Trim();
                Trx downStreamPos_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                if (downStreamPos_Trx == null)
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
                    return;
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

                string[,] each_pos = new string[4, 3];
                #region [拆出PLCAgent Data]
                each_pos[0, 0] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkFrontEndStageJobCassetteSequenceNumber"].Value;
                each_pos[0, 1] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkFrontEndStageJobSlotSequenceNumber"].Value;
                each_pos[0, 2] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkFrontEndStageJobExist"].Value;
                each_pos[1, 0] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkBackEndStageJobCassetteSequenceNumber"].Value;
                each_pos[1, 1] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkBackEndStageJobSlotSequenceNumber"].Value;
                each_pos[1, 2] = downStreamPos_Trx.EventGroups[0].Events[0].Items["LeftForkBackEndStageJobExist"].Value;
                each_pos[2, 0] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkFrontEndStageJobCassetteSequenceNumber"].Value;
                each_pos[2, 1] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkFrontEndStageJobSlotSequenceNumber"].Value;
                each_pos[2, 2] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkFrontEndStageJobExist"].Value;
                each_pos[3, 0] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkBackEndStageJobCassetteSequenceNumber"].Value;
                each_pos[3, 1] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkBackEndStageJobSlotSequenceNumber"].Value;
                each_pos[3, 2] = downStreamPos_Trx.EventGroups[0].Events[0].Items["RightForkBackEndStageJobExist"].Value;
                #endregion

                RobotCanControlSlotBlockInfo curSlotBlockInfo = null;
                for (int slot_no = 1; slot_no <= curStage.Data.SLOTMAXCOUNT; slot_no++)
                {
                    string cstSeq = each_pos[slot_no - 1, 0];
                    string jobSeq = each_pos[slot_no - 1, 1];
                    string jobexist = each_pos[slot_no - 1, 2];
                    string sub_loc = (slot_no % 2 == 0) ? eRobotCommonConst.ROBOT_ARM_BACK_LOCATION : eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION;
                    if (sub_loc == eRobotCommonConst.ROBOT_ARM_FRONT_LOCATION)
                    {
                        if (curSlotBlockInfo != null)
                            curSendOutSlotBlockList.Add(curSlotBlockInfo);
                        curSlotBlockInfo = null;
                    }
                    if (jobexist == "2" && cstSeq != "0" && jobSeq != "0")
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
                            // Stage Slot有帳無料
                            if (slot_no % 2 != 0) slot_no++;// Front Slot有帳無料, 跳過 Back Slot
                            curSlotBlockInfo = null;// Back Slot有帳無料, 清掉可能存在的curSlotBlockInfo
                        }
                        else
                        {
                            #region [ Update Job RobotWIP ]

                            //有變化才紀錄Log
                            if (curBcsJob.RobotWIP.CurLocation_StageID != curStage.Data.STAGEID ||
                                curBcsJob.RobotWIP.CurLocation_SlotNo != slot_no ||
                                curBcsJob.RobotWIP.CurLocation_StageType != eRobotStageType.STAGE ||
                                curBcsJob.RobotWIP.CurPortCstStatusPriority != eLoaderPortSendOutStatus.NOT_IN_PORT ||
                                curBcsJob.RobotWIP.CurSubLocation != sub_loc)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Job CassetteSequenceNo({1}) JobSequenceNo({2}) Update RobotWIP curStageType from ({3}) to ({4}), curStageID from ({5}) to ({6}), curSlotNo From ({7}) to ({8}).",
                                                        curStage.Data.NODENO, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurLocation_StageType,
                                                        eRobotStageType.EQUIPMENT, curBcsJob.RobotWIP.CurLocation_StageID, curStage.Data.STAGEID, curBcsJob.RobotWIP.CurLocation_SlotNo.ToString(),
                                                        slot_no.ToString());

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                lock (curBcsJob)
                                {
                                    curBcsJob.RobotWIP.CurLocation_StageID = curStage.Data.STAGEID;
                                    curBcsJob.RobotWIP.CurLocation_SlotNo = slot_no;
                                    curBcsJob.RobotWIP.CurLocation_StageType = eRobotStageType.STAGE;
                                    curBcsJob.RobotWIP.CurPortCstStatusPriority = eLoaderPortSendOutStatus.NOT_IN_PORT;
                                    curBcsJob.RobotWIP.CurSubLocation = sub_loc;
                                }

                                //Save File
                                ObjectManager.JobManager.EnqueueSave(curBcsJob);
                            }

                            #endregion

                            if (curSlotBlockInfo == null)
                            {
                                curSlotBlockInfo = new RobotCanControlSlotBlockInfo();
                                curSlotBlockInfo.CurBlock_Location_StageID = curBcsJob.RobotWIP.CurLocation_StageID;
                                curSlotBlockInfo.CurBlock_Location_StagePriority = curBcsJob.RobotWIP.CurLocation_StagePriority;
                                curSlotBlockInfo.CurBlock_PortCstStatusPriority = curBcsJob.RobotWIP.CurPortCstStatusPriority;
                                curSlotBlockInfo.CurBlock_RobotCmdSlotNo = (slot_no % 2 == 0) ? slot_no - 1 : slot_no;//CurBlock_RobotCmdSlotNo是單數SlotNo
                                curSlotBlockInfo.CurBlock_StepID = curBcsJob.RobotWIP.CurStepNo;
                                //20160119 add SlotBlockInfo Stage Type
                                curSlotBlockInfo.CurBlock_Location_StageType = curBcsJob.RobotWIP.CurLocation_StageType;
                            }
                            curSlotBlockInfo.CurBlockCanControlJobList.Add(curBcsJob);
                            if (curSlotBlockInfo.CurBlockCanControlJobList.Count == 1)
                            {
                                if (slot_no % 2 == 0)
                                    curSlotBlockInfo.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_EMPTY_BACK_EXIST;
                                else
                                    curSlotBlockInfo.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_EXIST_BACK_EMPTY;
                            }
                            else if (curSlotBlockInfo.CurBlockCanControlJobList.Count == 2)
                            {
                                curSlotBlockInfo.CurBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_BACK_EXIST;
                            }
                        }
                    }
                }
                if (curSlotBlockInfo != null)
                    curSendOutSlotBlockList.Add(curSlotBlockInfo);
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

     

    }
}
