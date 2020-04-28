using System;
using System.Linq;
using System.Collections.Generic;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.OpiSpec;


namespace UniAuto.UniBCS.CSOT.CommonService
{
    public partial class EquipmentService : AbstractService
    {
        private const string EQPAliveTimeout = "EQPAliveTimeout";
        private const string CIMModeChangeTimeout = "CIMModeChangeCommandTimeout";
        private const string EQPOperationModeTimeout = "EQPOperationModeTimeout";
        private const string SendWaitTimeoutTimeout = "SendWaitTimeoutTimeout";
        private const string EquipmentStatusChangeTimeout = "EquipmentStatusChangeTimeout";
        private const string SamplingRuleChangeReportTimeout = "SamplingRuleChangeReportTimeout";
        private const string EQtoEQInterlockTimeout = "EQtoEQInterlockTimeout";
        private const string SamplingRuleChangeTimeout = "SamplingRuleChange";
        private const string ProcessPauseCommandTimeout = "ProcessPauseCommandTimeout";
        private const string TransferStopCommandTimeout = "TransferStopCommandTimeout";
        private const string MaterialStatusRequestTimeout = "MaterialStatusRequestTimeout";
        private const string CoolRunCountSetCommandTimeout = "CoolRunCountSetCommandTimeout";
        private const string CoolRunCountSetReportTimeout = "CoolRunCountSetReportTimeout";
        private const string ProcessStopCommandTimeout = "ProcessStopCommandTimeout";
        private const string EQPStatusChangeTimeout = "EQPStatusChangeTimeout";
        private const string EQPStatusStopTimeout = "EQPStatusStopTimeout";
        private const string ChangerPlanRequestTimeout = "ChangerPlanRequestTimeout";
        private const string ChangerPlanStatusTimeout = "ChangerPlanStatusTimeout";
        private const string ChangerPlanDownloadSetTimeout = "ChangerPlanDownloadSetTimeout";
        private const string OperatorLoginLogoutTimeout = "OperatorLoginLogoutTimeout"; //add common function by bruce 2015/7/3
        private Dictionary<string, string> mesProcPauseCmdKey = new Dictionary<string, string>();
        public override bool Init()
        {

            return true;
        }

        #region [Equipment Alive]
        /// <summary>
        /// Equipment Alive
        /// </summary>
        public void EquipmentAlive(Trx inputData)
        {
            try
            {
                bool aLiveTimeout = false;
                eBitResult alive = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                MISC.Repository.Add(inputData.Name, inputData);

                // 2015.01.23 CIM off 也要看
                //if (eqp.File.CIMMode == eBitResult.OFF) return;

                if (_timerManager.IsAliveTimer(inputData.Metadata.NodeNo + "_" + EQPAliveTimeout))
                {
                    _timerManager.TerminateTimer(inputData.Metadata.NodeNo + "_" + EQPAliveTimeout);
                }

                _timerManager.CreateTimer(inputData.Metadata.NodeNo + "_" + EQPAliveTimeout, false,
                    ParameterManager["EQPALIVE"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(EquipmentAliveTimeout), inputData.TrackKey);

                // 此機台之前曾Timeout過, 恢復時要對時 胡福杰講滴
                if (eqp.File.AliveTimeout)
                {
                    aLiveTimeout = true;
                    if (eqp.File.CIMMode != eBitResult.OFF)
                    {
                        Invoke(eServiceName.DateTimeService, "DateTimeSetCommand", new object[] { new List<Equipment>() { eqp } });
                    }
                }

                lock (eqp.File)
                {
                    if (aLiveTimeout)
                    {
                        eqp.File.AliveTimeout = false;                        
                    }                    
                    eqp.File.LastAliveTime = DateTime.Now;
                }
                if (aLiveTimeout)
                {
                    Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                    //add by bruce 20160331 當機台Alive 狀態變更時,同步更新line state
                    Invoke(eServiceName.LineService, "CheckLineState", new object[] { inputData.TrackKey, eqp.Data.LINEID });
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void EquipmentAliveTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], EQPAliveTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENT ALIVE TIMEOUT.", sArray[0], trackKey));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);

                if (eqp == null)
                {
                    throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", sArray[0]));
                }   
                if (eqp.Data.NODEATTRIBUTE == "LU" || eqp.Data.NODEATTRIBUTE == "LD" || eqp.Data.NODEATTRIBUTE == "UPK") //Add By Yangzhenteng
                {
                    lock (eqp.File)
                    {
                        eqp.File.AliveTimeout = true;
                        eqp.File.MESStatus = "DOWN";   // 20170920 add by qiumin ,EQ LOSS electric Then alive  off, CFM must show down                     
                    } 
                }              
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { trackKey, eqp });

                // Add Kasim in 20150525 - Alarm Report to MES
                object[] _data = new object[8]
                { 
                    trackKey,             /*0 TrackKey*/ 
                    eqp.Data.LINEID,      /*1 LineName*/
                    eqp.Data.NODEID,      /*2 EQPID*/
                    "",                   /*3 UnitID*/
                    "BC2MES-0002",        /*4 AlarmID　(BC2MES-XXXX)　*/ 
                    "W",                  /*5 AlarmLevel　(A)　*/
                    "SET",                /*6 AlarmState　(SET)　*/
                    "EQUIPMENT ALIVE TIME OUT", /*7 AlarmText*/
                };
                Invoke(eServiceName.MESService, "AlarmReport", _data);

                //add by bruce 20160331 當機台Alive 狀態變更時,同步更新line state
                Invoke(eServiceName.LineService, "CheckLineState", new object[] { trackKey, eqp.Data.LINEID });
                if (eqp.Data.NODEATTRIBUTE=="LU"||eqp.Data.NODEATTRIBUTE=="LD"||eqp.Data.NODEATTRIBUTE=="UPK")    // 20170920 add by qiumin ,FA EQ LOSS electric Then alive  off, CFM must show down 全厂导入20180830
                {
                    string alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    object[] obj = new object[]          
                        {
                            trackKey,                               /*0 TrackKey*/
                            eqp,                                    /*1 Equipment*/
                            "BC2MES-0002",                          /*2 alarmID*/
                            "EQUIPMENT ALIVE TIME OUT",             /*3 alarmText*/
                            alarmTime                               /*4 alarmTime*/
                        };
                    Invoke(eServiceName.MESService, "MachineStateChanged", obj);// 20170920 add by qiumin ,EQ LOSS electric Then alive  off, CFM must show down
                }

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        private Trx GetTrx(string eqpNo, string trxName, string trackKey)
        {
            string strName = string.Format("{0}_{1}", eqpNo, trxName);

            PLCDataModel plc = GetPropertyValue(eAgentName.PLCAgent, "Model") as PLCDataModel;
            var check = plc.Transaction[strName];

            if (check == null) return null;

            Trx trx = Invoke(eAgentName.PLCAgent, "GetTransactionFormat", new object[] { strName }) as Trx;
            if (trx == null) return null;
                
            trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName,true }) as Trx;

            if (trx == null) return null;
            return trx;
        }

        private List<Trx> GetPortTrxs(Equipment eqp, string trxName, string trackKey)
        {
            List<Trx> lsTrx = new List<Trx>();
            List<Port> lsPorts = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID);

            if (lsPorts == null || lsPorts.Count == 0)
                return lsTrx;

            PLCDataModel plc = GetPropertyValue(eAgentName.PLCAgent, "Model") as PLCDataModel;

            foreach (Port p in lsPorts)
            {
                string strName = string.Format("{0}_Port#{2}{1}", eqp.Data.NODENO, trxName, p.Data.PORTNO);
                var check = plc.Transaction[strName];

                if (check == null)
                    continue;

                Trx positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName,true }) as Trx;
                //positionTrx.TrackKey = trackKey;

                if (positionTrx == null)
                    continue;

                lsTrx.Add(positionTrx);
            }

            return lsTrx;
        }

        private List<Trx> GetCELLDPTrxs(Equipment eqp, string trxName, string trackKey)
        {
            List<Trx> lsTrx = new List<Trx>();
            List<Port> lsPorts = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID);

            if (lsPorts == null || lsPorts.Count == 0)
                return lsTrx;

            PLCDataModel plc = GetPropertyValue(eAgentName.PLCAgent, "Model") as PLCDataModel;

            foreach (Port p in lsPorts)
            {
                string strName = string.Format("{0}_DP#{2}{1}", eqp.Data.NODENO, trxName, p.Data.PORTNO);
                var check = plc.Transaction[strName];

                if (check == null)
                    continue;

                Trx positionTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { strName, true }) as Trx;
                //positionTrx.TrackKey = trackKey;

                if (positionTrx == null)
                    continue;

                lsTrx.Add(positionTrx);
            }

            return lsTrx;
        }

        private void InvokeTrxs(string serviceName, string trxName, List<Trx> lsTrx, string log)
        {
            if (lsTrx == null)
                return;

            foreach (Trx t in lsTrx)
                Invoke(serviceName, trxName, new object[] { t, log });
        }

        #region [CIM Mode]
        /// <summary>
        /// 更新記億體資料
        /// </summary>
        /// <param name="eqp"></param>
        /// <param name="trackKey"></param>
        private void CIMModeUpdateAllStatus(Equipment eqp, string trackKey)
        {
            string logName = "CIM Mode Status On,";
            try
            {
                if (eqp.Data.REPORTMODE == eReportMode.HSMS_PLC.ToString() || 
                    eqp.Data.REPORTMODE == eReportMode.HSMS_CSOT.ToString() ||
                    eqp.Data.REPORTMODE == eReportMode.HSMS_NIKON.ToString()) return;

                #region 更新資料
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] \"CIMModeUpdateAllStatus\" START.", eqp.Data.NODENO, trackKey));

                Trx EquipmentRunModeChangeReport = GetTrx(eqp.Data.NODENO, "EquipmentRunModeChangeReport", trackKey);

                if (EquipmentRunModeChangeReport != null)
                    Invoke(eServiceName.EquipmentService, "EquipmentRunModeChangeReportUpdate", new object[] { EquipmentRunModeChangeReport, logName });

                Trx CurrentRecipeIDChangeReport = GetTrx(eqp.Data.NODENO, "CurrentRecipeIDChangeReport", trackKey);

                if (CurrentRecipeIDChangeReport != null)
                    Invoke(eServiceName.RecipeService, "CurrentRecipeIDChangeReportUpdate", new object[] { CurrentRecipeIDChangeReport, logName });

                Trx IndexerOperationModeChangeReport = GetTrx(eqp.Data.NODENO, "IndexerOperationModeChangeReport", trackKey);

                if (IndexerOperationModeChangeReport != null)
                    Invoke(eServiceName.EquipmentService, "IndexerOperationModeChangeReportUpdate", new object[] { IndexerOperationModeChangeReport, logName });

                Trx EquipmentOperationModeReport = GetTrx(eqp.Data.NODENO, "EquipmentOperationModeReport", trackKey);

                if (EquipmentOperationModeReport != null)
                    Invoke(eServiceName.EquipmentService, "EquipmentOperationModeReportUpdate", new object[] { EquipmentOperationModeReport, logName });

                Trx CSTOperationModeChangeReport = GetTrx(eqp.Data.NODENO, "CSTOperationModeChangeReport", trackKey);

                if (CSTOperationModeChangeReport != null)
                    Invoke(eServiceName.CassetteService, "CSTOperationModeChangeReportUpdate", new object[] { CSTOperationModeChangeReport, logName });

                Trx EquipmentStatusChangeReport = GetTrx(eqp.Data.NODENO, "EquipmentStatusChangeReport", trackKey);

                if (EquipmentStatusChangeReport != null)
                    Invoke(eServiceName.EquipmentService, "EquipmentStatusChangeReportUpdate", new object[] { EquipmentStatusChangeReport, logName });

                Trx ShortCutModeChangeReport = GetTrx(eqp.Data.NODENO, "ShortCutModeChangeReport", trackKey);

                if (ShortCutModeChangeReport != null)
                    Invoke(eServiceName.CFSpecialService, "ShortCutModeChangeReportUpdate", new object[] { ShortCutModeChangeReport, logName });

                Trx InspectionIdleTimeSettingReport = GetTrx(eqp.Data.NODENO, "InspectionIdleTimeSettingReport", trackKey);

                if (InspectionIdleTimeSettingReport != null)
                    Invoke(eServiceName.EquipmentService, "InspectionIdleTimeSettingReportUpdate", new object[] { InspectionIdleTimeSettingReport, logName });

                Trx WaitCassetteStatusReport = GetTrx(eqp.Data.NODENO, "WaitCassetteStatusReport", trackKey);

                if (WaitCassetteStatusReport != null)
                    Invoke(eServiceName.EquipmentService, "WaitCassetteStatusReportUpdate", new object[] { WaitCassetteStatusReport, logName });

                #region CELL Special CIM OFF -> ON Update //Jun Add20150204 For Cell Special------------------------------------------------------
                Trx RunModeChangeReport = GetTrx(eqp.Data.NODENO, "RunModeChangeReport", trackKey);

                if (RunModeChangeReport != null)
                    Invoke(eServiceName.CELLSpecialService, "RunModeChangeReportUpdate", new object[] { RunModeChangeReport, logName });


                Trx RunModeChangeReport1 = GetTrx(eqp.Data.NODENO, "RunModeChangeReport#01", trackKey);
                if (RunModeChangeReport1 != null)
                    Invoke(eServiceName.CELLSpecialService, "RunModeChangeReportUpdate", new object[] { RunModeChangeReport1, logName });
                Trx RunModeChangeReport2 = GetTrx(eqp.Data.NODENO, "RunModeChangeReport#02", trackKey);
                if (RunModeChangeReport2 != null)
                    Invoke(eServiceName.CELLSpecialService, "RunModeChangeReportUpdate", new object[] { RunModeChangeReport2, logName });

                Trx TCVDispatchingRuleChangeReport = GetTrx(eqp.Data.NODENO, "TCVDispatchingRuleChangeReport", trackKey);

                if (TCVDispatchingRuleChangeReport != null)
                    Invoke(eServiceName.CELLSpecialService, "TCVDispatchingRuleChangeReportUpdate", new object[] { TCVDispatchingRuleChangeReport, logName });

                Trx TCVSamplingModeReport = GetTrx(eqp.Data.NODENO, "TCVSamplingModeReport", trackKey);

                if (TCVSamplingModeReport != null)
                    Invoke(eServiceName.CELLSpecialService, "TCVSamplingModeReportUpdate", new object[] { TCVSamplingModeReport, logName });

                Trx ATSLoaderOperationModeChangeReport = GetTrx(eqp.Data.NODENO, "ATSLoaderOperationModeChangeReport", trackKey);

                if (ATSLoaderOperationModeChangeReport != null)
                    Invoke(eServiceName.CELLSpecialService, "ATSLoaderOperationModeChangeReportUpdate", new object[] { ATSLoaderOperationModeChangeReport, logName });

                Trx UnloadDispatchingRuleReport = GetTrx(eqp.Data.NODENO, "UnloadDispatchingRuleReport", trackKey);

                if (UnloadDispatchingRuleReport != null)
                    Invoke(eServiceName.CELLSpecialService, "UnloadDispatchingRuleReportUpdate", new object[] { UnloadDispatchingRuleReport, logName });

                Trx PortAbnormalCodeCheckRuleChangeReport = GetTrx(eqp.Data.NODENO, "PortAbnormalCodeCheckRuleChangeReport", trackKey);

                if (PortAbnormalCodeCheckRuleChangeReport != null)
                    Invoke(eServiceName.CELLSpecialService, "PortAbnormalCodeCheckRuleChangeReportUpdate", new object[] { PortAbnormalCodeCheckRuleChangeReport, logName });

                Trx PortAssignmentInformationReport = GetTrx(eqp.Data.NODENO, "PortAssignmentInformationReport", trackKey);

                if (PortAssignmentInformationReport != null)
                    Invoke(eServiceName.CELLSpecialService, "PortAssignmentInformationReportUpdate", new object[] { PortAssignmentInformationReport, logName });

                Trx VirtualPortOperationModeChangeReport = GetTrx(eqp.Data.NODENO, "VirtualPortOperationModeChangeReport", trackKey);

                if (VirtualPortOperationModeChangeReport != null)
                    Invoke(eServiceName.CELLSpecialService, "VirtualPortOperationModeChangeReportUpdate", new object[] { VirtualPortOperationModeChangeReport, logName });

                Trx ForceVCRReadingModeChangeReport = GetTrx(eqp.Data.NODENO, "ForceVCRReadingModeChangeReport", trackKey);

                if (ForceVCRReadingModeChangeReport != null)
                    Invoke(eServiceName.CELLSpecialService, "ForceVCRReadingModeChangeReportUpdate", new object[] { ForceVCRReadingModeChangeReport, logName });
                #endregion //--------------------------------------------------------------------------------------

                List<Trx> PortTypeChangeReport = GetPortTrxs(eqp, "PortTypeChangeReport", trackKey);
                InvokeTrxs(eServiceName.PortService, "PortTypeChangeReportUpdate", PortTypeChangeReport, logName);

                List<Trx> PortModeChangeReport = GetPortTrxs(eqp, "PortModeChangeReport", trackKey);
                InvokeTrxs(eServiceName.PortService, "PortModeChangeReportUpdate", PortModeChangeReport, logName);

                List<Trx> PortTransferModeChangeReport = GetPortTrxs(eqp, "PortTransferModeChangeReport", trackKey);
                InvokeTrxs(eServiceName.PortService, "PortTransferModeChangeReportUpdate", PortTransferModeChangeReport, logName);

                List<Trx> PortEnableModeChangeReport = GetPortTrxs(eqp, "PortEnableModeChangeReport", trackKey);
                InvokeTrxs(eServiceName.PortService, "PortEnableModeChangeReportUpdate", PortEnableModeChangeReport, logName);

                List<Trx> PortStatusChangeReport = GetPortTrxs(eqp, "PortStatusChangeReport", trackKey);
                InvokeTrxs(eServiceName.PortService, "PortStatusChangeReportUpdate", PortStatusChangeReport, logName);
                List<Trx> PortDownReport = GetPortTrxs(eqp, "PortDownReport", trackKey);
                InvokeTrxs(eServiceName.PortService, "PortDownReportUpdate", PortStatusChangeReport, logName);
                //與PortStatusChangeReport更新相同物件，故CassetteStatusChangeReportUpdate不做
                //List<Trx> CassetteStatusChangeReport = GetPortTrxs(eqp, "CassetteStatusChangeReport", trackKey);
                //InvokeTrxs(eServiceName.CassetteService, "CassetteStatusChangeReportUpdate", CassetteStatusChangeReport, logName);

                List<Trx> PortOperModeChangeReport = GetPortTrxs(eqp, "PortOperModeChangeReport", trackKey);
                InvokeTrxs(eServiceName.PortService, "PortOperModeChangeReportUpdate", PortOperModeChangeReport, logName);

                #region [CELL DP Port]
                List<Trx> DPPortTypeChangeReport = GetCELLDPTrxs(eqp, "PortTypeChangeReport", trackKey);
                InvokeTrxs(eServiceName.DenseBoxPortService, "DPPortTypeChangeReportUpdate", DPPortTypeChangeReport, logName);

                List<Trx> DPPortModeChangeReport = GetCELLDPTrxs(eqp, "PortModeChangeReport", trackKey);
                InvokeTrxs(eServiceName.DenseBoxPortService, "DPPortModeChangeReportUpdate", DPPortModeChangeReport, logName);

                List<Trx> DPPortTransferModeChangeReport = GetCELLDPTrxs(eqp, "PortTransferModeChangeReport", trackKey);
                InvokeTrxs(eServiceName.DenseBoxPortService, "DPPortTransferModeChangeReportUpdate", DPPortTransferModeChangeReport, logName);

                List<Trx> DPPortEnableModeChangeReport = GetCELLDPTrxs(eqp, "PortEnableModeChangeReport", trackKey);
                InvokeTrxs(eServiceName.DenseBoxPortService, "DPPortEnableModeChangeReportUpdate", DPPortEnableModeChangeReport, logName);

                List<Trx> DPPortStatusChangeReport = GetCELLDPTrxs(eqp, "PortStatusChangeReport", trackKey);
                InvokeTrxs(eServiceName.DenseBoxPortService, "DPPortStatusChangeReportUpdate", DPPortStatusChangeReport, logName);

                #endregion

                #region VCR
                Trx UnitVCREventStatus = GetTrx(eqp.Data.NODENO, "UnitVCREnableStatus", trackKey);
                if (UnitVCREventStatus != null)
                    Invoke(eServiceName.VCRService, "UnitVCREnableStatusUpdate", new object[] { UnitVCREventStatus, logName});
                #endregion

                #region Chamber Mode
                Trx EquipmentChamberModeChangeReport = GetTrx(eqp.Data.NODENO, "EquipmentChamberModeChangeReport", trackKey);
                if (EquipmentChamberModeChangeReport != null)
                    Invoke(eServiceName.ArraySpecialService, "EquipmentChamberModeChangeReportUpdate", new object[] { EquipmentChamberModeChangeReport, logName });
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] \"CIMModeUpdateAllStatus\" END.", eqp.Data.NODENO, trackKey));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// CIM Mode Status
        /// </summary>
        public void CIMMode(Trx inputData)
        {
            try
            {
                eBitResult cimMode = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM_MODE=[{2}]({3}).", inputData.Metadata.NodeNo,
                    inputData.TrackKey, (int)cimMode, cimMode.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                if (inputData.IsInitTrigger)
                {
                    lock (eqp) eqp.File.CIMMode = cimMode;
                    return;
                }
                eBitResult oldmode = eqp.File.CIMMode;
                lock (eqp) eqp.File.CIMMode = cimMode;

                // CIM Off 切成CIM ON要對時
                if (oldmode == eBitResult.OFF && cimMode == eBitResult.ON)
                {
                    Invoke(eServiceName.DateTimeService, "DateTimeSetCommand", new object[] { new List<Equipment>() { eqp } });
                    CIMModeUpdateAllStatus(eqp, inputData.TrackKey);
                }
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });

                Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, cimMode });  //modify by yang 20161204

                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CIMModeChangeCommand(string eqpNo, eCIMModeCmd cimMode, string trackKey)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                eReportMode mode;
                Enum.TryParse<eReportMode>(eqp.Data.REPORTMODE, out mode);

                switch (mode)
                {
                    case eReportMode.HSMS_CSOT:
                    case eReportMode.HSMS_PLC:
                        {
                            if (cimMode == eCIMModeCmd.CIM_OFF)
                                Invoke(eServiceName.CSOTSECSService, "TS1F15_H_OfflineRequest", new object[] { eqpNo, eqp.Data.NODEID, string.Empty, trackKey });
                            else if (cimMode == eCIMModeCmd.CIM_ON)
                                Invoke(eServiceName.CSOTSECSService, "TS1F17_H_OnlineRequest", new object[] { eqpNo, eqp.Data.NODEID, string.Empty, trackKey });

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}], SEND HSMS=[{3}].", eqp.Data.NODENO,
                              trackKey, cimMode.ToString(), cimMode == eCIMModeCmd.CIM_ON ? "S1F17_ONLINE" : "S1F15_OFFLINE"));
                        }
                        break;
                    case eReportMode.HSMS_NIKON:
                        {
                            if (cimMode == eCIMModeCmd.CIM_OFF)
                                Invoke(eServiceName.NikonSECSService, "TS1F15_H_RequestOffLine", new object[] { eqpNo, eqp.Data.NODEID, string.Empty, trackKey });
                            else if (cimMode == eCIMModeCmd.CIM_ON)
                                Invoke(eServiceName.NikonSECSService, "TS1F17_H_RequestOnLine", new object[] { eqpNo, eqp.Data.NODEID, string.Empty, trackKey });

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}], SEND HSMS=[{3}].", eqp.Data.NODENO,
                              trackKey, cimMode.ToString(), cimMode == eCIMModeCmd.CIM_ON ? "S1F17_ONLINE" : "S1F15_OFFLINE"));
                        }
                        break;
                    case eReportMode.PLC:
                    case eReportMode.PLC_HSMS:
                        {
                            Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_CIMModeChangeCommand") as Trx;
                            if (outputData == null)
                            {
                                Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] CAN'T FIND \"CIM Mode Change Command\"!.", eqp.Data.NODENO));
                                return;
                            }
                            outputData.EventGroups[0].Events[0].Items[0].Value = ((int)cimMode).ToString();
                            outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                            //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                            outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                            outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                            SendPLCData(outputData);

                            string timeName = string.Format("{0}_{1}", eqpNo, CIMModeChangeTimeout);

                            if (_timerManager.IsAliveTimer(timeName))
                            {
                                _timerManager.TerminateTimer(timeName);
                            }
                            _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(CIMModeChangeCommandReplyTimeout), outputData.TrackKey);

                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}], SET BIT=[ON].", eqp.Data.NODENO,
                                    outputData.TrackKey, cimMode.ToString()));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CIMModeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eReturnCode2 retCode = (eReturnCode2)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] CIM MODE CHANGE COMMAND REPLY RETURN_CODE=[{3}]",
                    eqpNo, inputData.TrackKey, triggerBit.ToString(), retCode));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, CIMModeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));
                string err = string.Empty;

                switch (retCode)
                {
                    case eReturnCode2.Accept: break;
                    case eReturnCode2.AlreadyInDesiredStatus:
                        {
                            err = string.Format("[{0}] EQUIPMENT=[{1}] RETURN_CODE=[2](Already in Desired Status).", MethodBase.GetCurrentMethod().Name, eqpNo);
                            Logger.LogWarnWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, err });
                        }
                        break;
                    case eReturnCode2.NG:
                        {
                            err = string.Format("[{0}] EQUIPMENT=[{1}] RETURN_CODE=[3](NG).", MethodBase.GetCurrentMethod().Name, eqpNo);
                            Logger.LogWarnWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, err });
                        }
                        break;
                    default:
                        {
                            err = string.Format("[{0}] EQUIPMENT=[{1}] RETURN_CODE=[{2}](UNKNOWN) IS INVALID!",
                                MethodBase.GetCurrentMethod().Name, eqpNo, inputData.EventGroups[0].Events[0].Items[0].Value);
                            Logger.LogWarnWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, err });
                        }
                        break;
                }
                #region [Command Off]
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(inputData.Metadata.NodeNo + "_CIMModeChangeCommand") as Trx;
                outputData.EventGroups[0].Events[0].IsDisable = true;
                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputData.TrackKey = inputData.TrackKey;
                SendPLCData(outputData);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "CIMModeChangeCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].", eqpNo, inputData.TrackKey));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CIMModeChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], CIMModeChangeTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM MODE CHANGE COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_CIMModeChangeCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("CIM Mode Change Command Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion

        #region [Inline Mode & Local Alarm Status]

        public void UpstreamInlineMode(Trx inputData)
        {
            try
            {
                // CELL Robot Main 在看的
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] UNPSTREAM_INLINE_MODE=[{2}]({3}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, (int)triggerBit, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.UpstreamInlineMode = inputData.EventGroups[0].Events[0].Items[0].Value.Equals("1") ? eBitResult.ON : eBitResult.OFF;
                }
                if (inputData.IsInitTrigger) return;
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void DownstreamInlineMode(Trx inputData)
        {
            try
            {
                // CELL Robot Main 在看的
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] DOWNSTREAM_INLINE_MODE=[{2}]({3}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, (int)triggerBit, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.DownstreamInlineMode = inputData.EventGroups[0].Events[0].Items[0].Value.Equals("1") ? eBitResult.ON : eBitResult.OFF;
                }
                if (inputData.IsInitTrigger) return;
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void LocalAlarmStatus(Trx inputData)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult alarmExist = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] LOCAL_ALARM_STATUS=[{2}].",
                    eqpNo, inputData.TrackKey, alarmExist.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                if (eqp.File.LocalAlarmStatus != alarmExist && alarmExist == eBitResult.OFF)
                {
                    ObjectManager.AlarmManager.ClearAllHappeningAlarm(eqpNo);
                }
                lock (eqp) eqp.File.LocalAlarmStatus = alarmExist;
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                if (inputData.IsInitTrigger) return;
                #region OPI Service Send
                //回報OPI更新Status
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region [Job Data Check]
        public void JobDataCheckMode(Trx inputData)
        {
            try
            {
                eEnableDisable triggerBit = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] JOBDATA_CHECK_MODE=[{2}]({3}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, (int)triggerBit, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.JobDataCheckMode = triggerBit;
                }
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        // Recipe ID Check Mode => RecipeServices.cs
        public void ProductTypeCheckMode(Trx inputData)
        {
            try
            {
                eEnableDisable triggerBit = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] PRODUCT_TYPE_CHECK_MODE=[{2}]({3}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, (int)triggerBit, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.ProductTypeCheckMode = triggerBit;
                }
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void GroupIndexCheckMode(Trx inputData)
        {
            try
            {
                eEnableDisable triggerBit = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] GROUP_INDEX_CHECK_MODE=[{2}]({3}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, (int)triggerBit, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.GroupIndexCheckMode = triggerBit;
                }
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ProductIDCheckMode(Trx inputData)
        {
            try
            {
                eEnableDisable triggerBit = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] PRODUCT_ID_CHECK_MODE=[{2}]({3}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, (int)triggerBit, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.ProductIDCheckMode = triggerBit;
                }
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void JobDuplicateCheckMode(Trx inputData)
        {
            try
            {
                eEnableDisable triggerBit = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] JOB_DUPLICATE_CHECK_MODE=[{2}]({3}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, (int)triggerBit, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.JobDuplicateCheckMode = triggerBit;
                }
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void COAVersionCheckMode(Trx inputData)
        {
            try
            {
                eEnableDisable triggerBit = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] COA_VERSION_CHECK_MODE=[{2}]({3}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, (int)triggerBit, triggerBit.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                lock (eqp)
                {
                    eqp.File.COAVersionCheckMode = triggerBit;
                }
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Equipment Status Change Report]
        /// <summary>
        /// 更新記億體資料
        /// </summary>
        /// <param name="inputData">同EquipmentStatusChangeReport(Trx inputData)</param>
        /// <param name="log">記錄動作者</param>
        public void EquipmentStatusChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string log;
                string eqpNo = inputData.Metadata.NodeNo;
                AnalyseLog_EquipmentStatus(inputData, out log);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] {3}.", eqpNo, sourceMethod, inputData.TrackKey, log));

                Handle_EquipmentStatus(inputData);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void EquipmentStatusChangeReport(Trx inputData)
        {
            bool IsBFG = false;
            if (inputData.Name.Substring(inputData.Name.Length - 3, 3) == "BFG") IsBFG = true;
            try
            {
                if (inputData.IsInitTrigger)
                {
                    EquipmentStatusChangeReportUpdate(inputData, string.Format("EquipmentStatusChangeReport{0}_Initial", IsBFG ? "BFG": ""));
                    return;
                }

                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                string[] tmp = inputData.Name.Split('_');
                string name = tmp[tmp.Length - 1];

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
                    EquipmentStatusChangeReportReply(eqpNo, eBitResult.OFF, IsBFG, inputData.TrackKey);
                    return;
                }
                string log;
                AnalyseLog_EquipmentStatus(inputData, out log);

                Logger.LogInfoWrite(this.LogName, GetType().Name, name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] {2}", inputData.Metadata.NodeNo, inputData.TrackKey, log));

                Handle_EquipmentStatus(inputData);

                EquipmentStatusChangeReportReply(eqpNo, eBitResult.ON, IsBFG, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    EquipmentStatusChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, IsBFG, inputData.TrackKey);
                }
            }
        }

        // 多寫一層, 會有其他地方來要求更新
        public void Handle_EquipmentStatus(Trx inputData)
        {
            try
            {
                bool IsBFG = false;
                if (inputData.Name.Substring(inputData.Name.Length - 3, 3) == "BFG") IsBFG = true;

                string eqpNo = IsBFG ? "L3" : inputData.Metadata.NodeNo;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                eEQPStatus eqpStatus = (eEQPStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                string eqpAlarmID = inputData.EventGroups[0].Events[0].Items[1].Value;

                #region [Equipment Status]
                if (eqp.File.Status != eqpStatus)
                {
                    lock(eqp)
                        eqp.File.Status = eqpStatus;
                    // MachineStateChanged
                    if (eqp.File.MESStatus != EQPStatus2MESStatus(eqpStatus))
                    {
                        string alarmText = string.Empty;
                        string alarmTime = string.Empty;
                        #region [取得目前正在發生的Alarm]
                        if (!eqpAlarmID.Equals("0"))
                        {
                            HappeningAlarm happenAlarm = ObjectManager.AlarmManager.GetAlarm(eqp.Data.NODENO, eqpAlarmID);
                            if (happenAlarm == null)
                            {
                                AlarmEntityData alarm = ObjectManager.AlarmManager.GetAlarmProfile(eqpNo, "0", eqpAlarmID);
                                if (alarm != null)
                                {
                                    alarmText = alarm.ALARMTEXT;
                                }
                                alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            }
                            else
                            {
                                alarmText = happenAlarm.Alarm.ALARMTEXT;
                                alarmTime = happenAlarm.OccurDateTime.ToString("yyyyMMddHHmmss");
                            }
                        }
                        #endregion
                        lock (eqp)
                        {                            
                            eqp.File.CurrentAlarmCode = eqpAlarmID;
                            eqp.File.MESStatus = EQPStatus2MESStatus(eqpStatus);
                        }
                        // MES Data
                        object[] obj = new object[]
                        {
                            inputData.TrackKey,                     /*0 TrackKey*/
                            eqp,                                  /*1 Equipment*/
                            eqpAlarmID.Equals("0") ? "" : eqpAlarmID, /*2 alarmID*/
                            alarmText,                             /*3 alarmText*/
                            alarmTime                              /*4 alarmTime*/
                        };
                        Invoke(eServiceName.MESService, "MachineStateChanged", obj);
                    }
                    

                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                    ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
                    Invoke(eServiceName.LineService, "CheckLineState", new object[] { inputData.TrackKey, eqp.Data.LINEID });
                }
                #endregion

                #region [Units]
                IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqpNo);

                foreach (Unit u in units)
                {
                    if (u == null || u.Data.NODENO.Equals("0") || string.IsNullOrEmpty(u.Data.UNITID)) continue;

                    eEQPStatus unitStatus = GetUnitStatus(inputData, u.Data.UNITNO);
                    string uAlmCode = GetUnitAlarmCode(inputData, u.Data.UNITNO);

                    if (unitStatus != u.File.Status)
                    {
                        //AlarmEntityData alarm = ObjectManager.AlarmManager.GetAlarmProfile(eqpNo, u.Data.UNITNO, uAlmCode);
                        lock (u) u.File.Status = unitStatus;

                        if (u.File.MESStatus != EQPStatus2MESStatus(unitStatus))
                        {
                            lock (u) u.File.MESStatus = EQPStatus2MESStatus(unitStatus);                                                           
                            string alarmText = string.Empty;
                            string alarmTime = string.Empty;
                            #region [取得目前正在發生的Alarm]
                            if (!eqpAlarmID.Equals("0"))
                            {
                                HappeningAlarm happenAlarm = ObjectManager.AlarmManager.GetAlarm(eqp.Data.NODENO, uAlmCode);
                                if (happenAlarm == null)
                                {
                                    AlarmEntityData alarm = ObjectManager.AlarmManager.GetAlarmProfile(eqpNo, u.Data.UNITNO, eqpAlarmID);
                                    if (alarm != null)
                                    {
                                        alarmText = alarm.ALARMTEXT;
                                    }
                                    alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                                }
                                else
                                {
                                    alarmText = happenAlarm.Alarm.ALARMTEXT;
                                    alarmTime = happenAlarm.OccurDateTime.ToString("yyyyMMddHHmmss");
                                }
                            }
                            #endregion

                            #region [Report to MES]
                                object[] obj = new object[]
                                {
                                    inputData.TrackKey,                   /*0 TrackKey*/
                                    u,                                  /*1 Unit*/
                                    uAlmCode.Equals("0") ? "" : uAlmCode,   /*2 AlarmID*/
                                    alarmText,                           /*3 Alarm Text*/
                                    alarmTime,                           /*4 Alarm Time*/
                                };
                                Invoke(eServiceName.MESService, "UnitStateChanged", obj);
                                #endregion

                            lock (u) u.File.CurrentAlarmCode = uAlmCode;                            
                        }
                        ObjectManager.UnitManager.EnqueueSave(u.File);

                        ObjectManager.UnitManager.RecordUnitHistory(inputData.TrackKey,u);
                    }
                }
                #endregion

                #region PI ODF Loader Tack Time Check
                //if (eqp.Data.LINEID.Contains(eLineType.CELL.CBODF))
                if (eqp.Data.LINEID.Contains(eLineType.CELL.CCPIL) || eqp.Data.LINEID.Contains(keyCellLineType.ODF)) //sy add 20160907 
                    Invoke(eServiceName.CELLSpecialService, "LoaderTackTimeCheck", new object[] { eqp.Data.LINEID, inputData.TrackKey });
                #endregion

                #region OPI Service Send
                if (inputData.IsInitTrigger) return;
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
                #endregion

                // Add By Wangshengjun 20191030
                #region[PVD Line L4 Unit Status Change Modify CLN TactTime]
                //Add By Yangzhenteng20191030
                bool PVDLINECHANGECLEANERCURRENTTACTTIMEFLAG = ParameterManager.ContainsKey("PVDLINECHANGECLEANERCURRENTTACTTIMEFLAG") ? ParameterManager["PVDLINECHANGECLEANERCURRENTTACTTIMEFLAG"].GetBoolean() : false;
                if (eqp.Data.LINEID.Contains("TCMSP") && eqp.Data.NODENO == "L4" && PVDLINECHANGECLEANERCURRENTTACTTIMEFLAG)
                {
                    string[] eqpList = { "L2" };
                    string CurrentCheckRecipeName = string.Empty;
                    string Unit01status = inputData.EventGroups[0].Events[0].Items[2].Value;
                    string Unit02status = inputData.EventGroups[0].Events[0].Items[4].Value;
                    string Unit04status = inputData.EventGroups[0].Events[0].Items[8].Value;
                    string Unit05status = inputData.EventGroups[0].Events[0].Items[10].Value;
                    List<Job> Pvdcheckjobs = new List<Job>();
                    Pvdcheckjobs.Clear();
                    Pvdcheckjobs = ObjectManager.JobManager.GetSpecialJobsbyEQPList(eqpList.ToList<string>()).OrderBy(p => p.CreateTime).ToList();
                    List<string> Pvdcheckrecipelists = new List<string>();
                    Pvdcheckrecipelists.Clear();
                    foreach (Job j in Pvdcheckjobs)
                    {
                        if (!Pvdcheckrecipelists.Contains(j.LineRecipeName.Substring(4, 2)))
                        {
                            Pvdcheckrecipelists.Add(j.LineRecipeName.Substring(4, 2));
                            continue;
                        }
                    }
                    if (Pvdcheckrecipelists.Count != 0)
                    {
                        CurrentCheckRecipeName = Pvdcheckrecipelists[0];
                    }
                    if (CurrentCheckRecipeName == "LS")
                    {
                        #region[LS Mode]
                        //双Load双腔
                        if ((Unit01status == "4" || Unit01status == "5") && (Unit02status == "4" || Unit02status == "5") && (Unit04status == "4" || Unit04status == "5") && (Unit05status == "4" || Unit05status == "5"))
                        {
                            object[] obj = new object[]          
                        {     
                            "L3",
                            "0",
                            "55",
                            inputData.TrackKey
                        };
                            Invoke(eServiceName.ArraySpecialService, "CurrentRecipeChangeTactTimeTransferCommand", obj);
                        }
                        //双Load单腔
                        else if (((Unit01status == "4" || Unit01status == "5") && (Unit02status == "4" || Unit02status == "5") && (Unit04status != "4" && Unit04status != "5") && (Unit05status == "4" || Unit05status == "5")) ||
                                 ((Unit01status == "4" || Unit01status == "5") && (Unit02status == "4" || Unit02status == "5") && (Unit05status != "4" && Unit05status != "5") && (Unit04status == "4" || Unit04status == "5")))
                        {
                            object[] obj = new object[]          
                        {     
                            "L3",
                            "1",
                            "95",
                            inputData.TrackKey
                        };
                            Invoke(eServiceName.ArraySpecialService, "CurrentRecipeChangeTactTimeTransferCommand", obj);
                        }
                        //单Load双腔
                        else if (((Unit01status != "4" && Unit01status != "5") && (Unit02status == "4" || Unit02status == "5") && (Unit04status == "4" || Unit04status == "5") && (Unit05status == "4" || Unit05status == "5")) ||
                                 ((Unit01status == "4" || Unit01status == "5") && (Unit02status != "4" && Unit02status != "5") && (Unit05status == "4" || Unit05status == "5") && (Unit04status == "4" || Unit04status == "5")))
                        {
                            object[] obj = new object[]          
                        {     
                            "L3",
                            "1",
                            "95",
                            inputData.TrackKey
                        };
                            Invoke(eServiceName.ArraySpecialService, "CurrentRecipeChangeTactTimeTransferCommand", obj);
                        }
                        //单Load单腔
                        else if (((Unit01status != "4" && Unit01status != "5") && (Unit02status == "4" || Unit02status == "5") && (Unit04status != "4" && Unit04status != "5") && (Unit05status == "4" || Unit05status == "5")) ||
                                 ((Unit01status != "4" && Unit01status != "5") && (Unit02status == "4" || Unit02status == "5") && (Unit05status != "4" && Unit05status != "5") && (Unit04status == "4" || Unit04status == "5")) ||
                                 ((Unit02status != "4" && Unit02status != "5") && (Unit01status == "4" || Unit01status == "5") && (Unit04status != "4" && Unit04status != "5") && (Unit05status == "4" || Unit05status == "5")) ||
                                 ((Unit02status != "4" && Unit02status != "5") && (Unit01status == "4" || Unit01status == "5") && (Unit05status != "4" && Unit05status != "5") && (Unit04status == "4" || Unit04status == "5")))
                        {
                            object[] obj = new object[]          
                        {     
                            "L3",
                            "1",
                            "95",
                            inputData.TrackKey
                        };
                            Invoke(eServiceName.ArraySpecialService, "CurrentRecipeChangeTactTimeTransferCommand", obj);
                        }
                        //其他
                        else
                        { }
                        #endregion
                    }
                    else if (CurrentCheckRecipeName == "M3")
                    {
                        #region[M3 Mode]
                        //双Load双腔
                        if ((Unit01status == "4" || Unit01status == "5") && (Unit02status == "4" || Unit02status == "5") && (Unit04status == "4" || Unit04status == "5") && (Unit05status == "4" || Unit05status == "5"))
                        {
                            object[] obj = new object[]          
                        {     
                            "L3",
                            "0",
                            "75",
                            inputData.TrackKey
                        };
                            Invoke(eServiceName.ArraySpecialService, "CurrentRecipeChangeTactTimeTransferCommand", obj);
                        }
                        //双Load单腔
                        else if (((Unit01status == "4" || Unit01status == "5") && (Unit02status == "4" || Unit02status == "5") && (Unit04status != "4" && Unit04status != "5") && (Unit05status == "4" || Unit05status == "5")) ||
                                 ((Unit01status == "4" || Unit01status == "5") && (Unit02status == "4" || Unit02status == "5") && (Unit05status != "4" && Unit05status != "5") && (Unit04status == "4" || Unit04status == "5")))
                        {
                            object[] obj = new object[]          
                        {     
                            "L3",
                            "1",
                            "130",
                            inputData.TrackKey
                        };
                            Invoke(eServiceName.ArraySpecialService, "CurrentRecipeChangeTactTimeTransferCommand", obj);
                        }
                        //单Load双腔
                        else if (((Unit01status != "4" && Unit01status != "5") && (Unit02status == "4" || Unit02status == "5") && (Unit04status == "4" || Unit04status == "5") && (Unit05status == "4" || Unit05status == "5")) ||
                                 ((Unit01status == "4" || Unit01status == "5") && (Unit02status != "4" && Unit02status != "5") && (Unit05status == "4" || Unit05status == "5") && (Unit04status == "4" || Unit04status == "5")))
                        {
                            object[] obj = new object[]          
                        {     
                            "L3",
                            "1",
                            "130",
                            inputData.TrackKey
                        };
                            Invoke(eServiceName.ArraySpecialService, "CurrentRecipeChangeTactTimeTransferCommand", obj);
                        }
                        //单Load单腔
                        else if (((Unit01status != "4" && Unit01status != "5") && (Unit02status == "4" || Unit02status == "5") && (Unit04status != "4" && Unit04status != "5") && (Unit05status == "4" || Unit05status == "5")) ||
                                 ((Unit01status != "4" && Unit01status != "5") && (Unit02status == "4" || Unit02status == "5") && (Unit05status != "4" && Unit05status != "5") && (Unit04status == "4" || Unit04status == "5")) ||
                                 ((Unit02status != "4" && Unit02status != "5") && (Unit01status == "4" || Unit01status == "5") && (Unit04status != "4" && Unit04status != "5") && (Unit05status == "4" || Unit05status == "5")) ||
                                 ((Unit02status != "4" && Unit02status != "5") && (Unit01status == "4" || Unit01status == "5") && (Unit05status != "4" && Unit05status != "5") && (Unit04status == "4" || Unit04status == "5")))
                        {
                            object[] obj = new object[]          
                        {     
                            "L3",
                            "1",
                            "130",
                            inputData.TrackKey
                        };
                            Invoke(eServiceName.ArraySpecialService, "CurrentRecipeChangeTactTimeTransferCommand", obj);
                        }
                        //其他
                        else
                        { }
                        #endregion
                    }
                    else
                    { }
                #endregion
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void EquipmentStatusChangeReportReply(string eqpNo, eBitResult value, bool isBFG, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_EquipmentStatusChangeReportReply{1}", eqpNo, isBFG ? "BFG" : "");
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(trxName) as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + EquipmentStatusChangeTimeout + (isBFG ? "BFG" : "")))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + EquipmentStatusChangeTimeout + (isBFG ? "BFG" : ""));
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + EquipmentStatusChangeTimeout + (isBFG ? "BFG" : ""), false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(EquipmentStatusChangeReportReplyTimeout), trackKey + ";" + isBFG.ToString());
                }

                string[] tmp = outputdata.Name.Split('_');
                string name = tmp[tmp.Length - 1];

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, name + "()", 
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENT STATUS CHANGE REPORT REPLY{2}, SET BIT=[{3}].",
                    eqpNo, trackKey, isBFG ? " BFG" : "", value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void EquipmentStatusChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string[] acktiem = timer.State.ToString().Split(';');
                string trackKey = acktiem[0];
                bool isBFG = bool.Parse(acktiem[1]);
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENT STATUS CHANGE REPLY{2} TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, isBFG ? " BFG" : ""));

                EquipmentStatusChangeReportReply(sArray[0], eBitResult.OFF, isBFG, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private eEQPStatus GetUnitStatus(Trx inputData, string unitNo)
        {
            try
            {
                int _unitNO;
                int.TryParse(unitNo, out _unitNO);
                if (_unitNO.Equals("0")) return eEQPStatus.NOUNIT;

                return (eEQPStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[(_unitNO - 1) * 2 + 2].Value);
            }
            catch
            {
                return eEQPStatus.NOUNIT;
            }
        }

        private string GetUnitAlarmCode(Trx inputData, string unitNo)
        {
            try
            {
                int _unitNO;
                int.TryParse(unitNo, out _unitNO);
                if (_unitNO.Equals("0")) return "0";

                return inputData.EventGroups[0].Events[0].Items[(_unitNO - 1) * 2 + 3].Value;
            }
            catch
            {
                return "0";
            }
        }

        private string EQPStatus2MESStatus(eEQPStatus eqpStatus)
        {
            switch (eqpStatus)
            {
                case eEQPStatus.IDLE:
                case eEQPStatus.RUN: return eqpStatus.ToString();
                default: return "DOWN";
            }
        }

        private void AnalyseLog_EquipmentStatus(Trx inputData, out string log)
        {
            log = string.Empty;
            try
            {
                bool IsBFG = false;
                if (inputData.Name.Substring(inputData.Name.Length - 3, 3) == "BFG") IsBFG = true;

                string eqpNo = IsBFG ? "L3" : inputData.Metadata.NodeNo;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                eEQPStatus eqpStatus = (eEQPStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                string eqpAlarmID = inputData.EventGroups[0].Events[0].Items[1].Value;
                log = string.Format("CIM_MODE=[{0}] EQUIPMENT_STATUS=[{1}]({2}) ALARM_ID=[{3}]",
                    (IsBFG ? "NONE" : eqp.File.CIMMode.ToString()), (int)eqpStatus, eqpStatus.ToString(), eqpAlarmID);

                #region [Units]
                IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqpNo);

                foreach (Unit u in units)
                {
                    if (u == null || u.Data.NODENO.Equals("0") || string.IsNullOrEmpty(u.Data.UNITID)) continue;

                    eEQPStatus unitStatus = GetUnitStatus(inputData, u.Data.UNITNO);
                    string uAlmCode = GetUnitAlarmCode(inputData, u.Data.UNITNO);

                    log += string.Format(" UNIT#{0}{{STATUS=[{1}]({2}) ALARM_ID=[{3}]}}", u.Data.UNITNO, (int)unitStatus, unitStatus.ToString(), uAlmCode);
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region [Equipment Operation Mode]
        /// <summary>
        /// 更新記億體資料
        /// </summary>
        /// <param name="inputData">同EquipmentOperationModeReport(Trx inputData)</param>
        /// <param name="log">記錄動作者</param>
        public void EquipmentOperationModeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                eEQPOperationMode mode = (eEQPOperationMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                lock (eqp) eqp.File.EquipmentOperationMode = mode;
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] EQUIPMENT_OPERATION_MODE=[{3}].", eqp.Data.NODENO, sourceMethod, inputData.TrackKey, mode));

                if (inputData.IsInitTrigger) return;
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void EquipmentOperationModeReport(Trx inputData)
        {
            try
            {
                /*
                 * 1: Manual
                 * 2: Auto
                 * 3: Semi-Auto
                 * */
                if (inputData.IsInitTrigger)
                {
                    EquipmentOperationModeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    EquipmentOperationModeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                eEQPOperationMode mode = (eEQPOperationMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                lock (eqp) eqp.File.EquipmentOperationMode = mode;
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] EQUIPMENT_OPERATION_MODE=[{3}]({4}).",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, (int)mode, mode.ToString()));

                EquipmentOperationModeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);

                if (inputData.IsInitTrigger) return;
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    EquipmentOperationModeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void EquipmentOperationModeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_EquipmentOperationModeReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + EQPOperationModeTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + EQPOperationModeTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + EQPOperationModeTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(EquipmentOperationModeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", 
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENT OPERATION MODE REPORT REPLY, SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void EquipmentOperationModeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENT OPERATION MODE REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                EquipmentOperationModeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Send Wait Timeout Report]
        public void SendWaitTimeOutReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);
                string downstreameqpNo = "L" + inputData[0][0][0].Value;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) 
                    throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                if (triggerBit == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", eqpNo, inputData.TrackKey));
                    SendWaitTimeOutReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] DOWNSTREAM_EQUIPMENT_NO=[{3}]({4}).",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, inputData[0][0][0].Value, downstreameqpNo));

                Equipment downEQP = ObjectManager.EquipmentManager.GetEQP(downstreameqpNo);

                if (downEQP == null) //机台可能报错DownStream 的NodeNo BC记录Log 即可不用抛出Exception  20150307 Tom
                {
                    LogError(MethodBase.GetCurrentMethod().Name+"()",string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CAN'T FIND DOWNSTREAM_EQUIPMENT_NO=[{2}] IN EQUIPMENTENTITY!", eqpNo,inputData.TrackKey, downstreameqpNo));
                } 
                else
                {
                    if (downEQP.File.CIMMode == eBitResult.OFF)
                    {
                        LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("CIM MODE IS OFF FROM DOWNSTREAM_EQUIPMENT_NO=[{0}].", downstreameqpNo));
                    }
                    else
                    {

                        if (downEQP.File.Status == eEQPStatus.IDLE)
                        {
                            string msg = string.Format("AFTER EQUIPMENT=[{0}] NOTIFIES BC THAT SEND WAIT TIMEOUT HAS OCCURRED BETWEEN EQUIPMENT=[{0}] AND EQUIPMENT=[{1}], BCS SHOULD SEND \"Equipment Status Change Stop Command\" TO EQUIPMENT=[{1}].",
                                eqp.Data.NODENO, downstreameqpNo);
                            LogWarn(MethodBase.GetCurrentMethod().Name + "()", msg);

                            Invoke(eServiceName.UIService, "OPIFlashMessage", new object[] { inputData.TrackKey, eqpNo, downstreameqpNo });

                            if (ParameterManager[eREPORT_SWITCH.EQUIPMENT_STATUS_STOP_COMMAND].GetBoolean())//在Parameter.xml中设定是否需要BCS下Equipment Status Stop Command  Tom  20150213
                            {
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, msg });

                                EquipmentStatusStopCommand(downEQP.Data.NODENO, inputData.TrackKey);
                            }
                            else
                            {
                                string log = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENTSTATUSSTOPCOMMANDMODE is Disable BC do not Send Equipment Status Stop Command.", eqpNo, inputData.TrackKey);
                                LogWarn(MethodBase.GetCurrentMethod().Name + "()", log);
                            }
                        }
                    }
                }

                SendWaitTimeOutReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);

                // Add Kasim in 20150525 - Alarm Report to MES
                object[] _data = new object[8]
                { 
                    inputData.TrackKey,   /*0 TrackKey*/ 
                    eqp.Data.LINEID,      /*1 LineName*/
                    eqp.Data.NODEID,      /*2 EQPID*/
                    "",                   /*3 UnitID*/
                    "BC2MES-0001",        /*4 AlarmID　(BC2MES-XXXX)　*/ 
                    "W",                  /*5 AlarmLevel　(A)　*/
                    "SET",                /*6 AlarmState　(SET)　*/
                    "SEND WAIT TIME OUT", /*7 AlarmText*/
                };
                Invoke(eServiceName.MESService, "AlarmReport", _data);

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData[0][1][0].Value.Equals("1"))
                {
                    SendWaitTimeOutReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void SendWaitTimeOutReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_SendWaitTimeOutReportReply") as Trx;
                outputdata.EventGroups[0][0][0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + SendWaitTimeoutTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + SendWaitTimeoutTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + SendWaitTimeoutTimeout, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(SendWaitTimeOutReportReplyTimeout), trackKey);
                }
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SEDN WAIT TIMEOUT REPORT REPLY, SET=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void SendWaitTimeOutReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SEND WAIT TIMEOUT REPORT REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                SendWaitTimeOutReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [EquipmentStatusChangeStop]

        public void EquipmentStatusStopCommand(string eqpNo, string trackKey)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                string err = string.Empty;
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    throw new Exception(err);
                }

                #region[CIM MODE OFF 不能改]
                //20150709 Add Frank
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM_MODE=[OFF], CAN NOT CHAGE EQUIPMENT STATUS!", eqp.Data.NODENO);
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_EquipmentStatusStopCommand") as Trx;

                outputData[0][0][0].Value = ((int)eBitResult.ON).ToString();
                outputData.TrackKey = trackKey;//UtilityMethod.GetAgentTrackKey()
                SendPLCData(outputData);

                string timeName = string.Format("{0}_{1}", eqpNo, EQPStatusStopTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(EquipmentStatusStopCommandReplyTimeout), outputData.TrackKey);

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[ON].", eqp.Data.NODENO, outputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void EquipmentStatusStopCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eReturnCode3 retCode = (eReturnCode3)int.Parse(inputData[0][0][0].Value);
                eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                    eqpNo, inputData.TrackKey, triggerBit.ToString(), (int)retCode, retCode.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, EQPStatusStopTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));
                string err = string.Empty;

                switch (retCode)
                {
                    case eReturnCode3.Accept: break;
                    case eReturnCode3.NotAcceppt:
                        {
                            err = string.Format("[{0}] EQUIPMENT=[{1}] RETURN_CODE=[2](Not Accept).", MethodBase.GetCurrentMethod().Name, eqpNo);

                            LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);

                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, err });
                        }
                        break;
                    default:
                        {
                            err = string.Format("[{0}] EQUIPMENT=[{1}] RETURN_CODE=[{2}](UNKOWN) IS INVALID.",
                                MethodBase.GetCurrentMethod().Name, eqpNo, inputData.EventGroups[0].Events[0].Items[0].Value);

                            LogError(MethodBase.GetCurrentMethod().Name + "()", err);

                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, err });
                        }
                        break;
                }
                #region [Command Off]
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(inputData.Metadata.NodeNo + "_EquipmentStatusStopCommand") as Trx;
                outputData[0][0][0].Value = ((int)eBitResult.OFF).ToString();
                outputData.TrackKey = inputData.TrackKey;
                SendPLCData(outputData);
                LogInfo("EquipmentStatusStopCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].", eqpNo, inputData.TrackKey));
                #endregion
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void EquipmentStatusStopCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], EQPStatusStopTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQUIPMENT STATUS STOP COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_EquipmentStatusStopCommand") as Trx;
                outputdata[0][0][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region [Sampling Rule Change Report]
        public void SamplingRuleChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                
                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                eSamplingRule samplingRule = (eSamplingRule)int.Parse(inputData[0][0][0].Value);
                string samplingCount = inputData[0][0][1].Value;
                string sideUnit = inputData[0][0][2].Value;
                string unitInformation = string.Empty;
                string groupNo = string.Empty; //sy add 20160729
                string otherLog = string.Empty;//sy add 20160729
                for (int i = 0; i < sideUnit.Length; i++)
                {
                    string enable = sideUnit.Substring(i, 1);
                    if (enable == "1")
                    {
                        eSideUnit infor = (eSideUnit)i;
                        unitInformation = (unitInformation == string.Empty) ? infor.ToString() : unitInformation + "," + infor;
                    }
                }
                if (inputData[0][0]["GroupNo"] != null)
                {
                    groupNo = inputData[0][0]["GroupNo"].Value.Trim();
                    otherLog += string.Format("GroupNo=[{0}]", groupNo);
                }

                eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);
                if (triggerBit == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    SamplingRuleChangeReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                Repository.Add(inputData.Name, inputData);
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] SAMPLING_RULE=[{3}]({4}) SAMPLING_COUNT=[{5}] SAMPLING_UNIT=[{6}] {7}.",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, (int)samplingRule, samplingRule, samplingCount, unitInformation, otherLog));
                lock (eqp)
                {
                    eqp.File.SamplingRule = samplingRule;
                    eqp.File.SamplingCount = int.Parse(samplingCount);
                    eqp.File.SamplingUnit = unitInformation;
                    eqp.File.SamplingGroup = groupNo;
                }
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);//20150716 Add by Frank
                SamplingRuleChangeReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
                // Add by Kasim 20150510
                ObjectManager.EquipmentManager.RecordEquipmentHistory(inputData.TrackKey, eqp);
                //public void InspectionModeChanged(string trxID, string lineName, string inspMode, string eqpID, string pullmodeGrade, string waittime, string samplerate, string reasoncode)

                #region [CF Report MES - InspectionModeChanged]
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCOPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCSPH_TYPE1
                    )
                {
                    object[] _data = new object[8]
                    { 
                        inputData.TrackKey,                   /*0  TrackKey*/
                        eqp.Data.LINEID,                      /*1  LineName*/
                        eqp.File.SamplingRule.ToString(),     /*2  INSPMode*/
                        eqp.Data.NODEID,                      /*3  EQP ID*/
                        string.Empty,                         /*4  Pull Mode Grade (CF 不使用)*/
                        string.Empty,                         /*5  Wait Time (CF 不使用)*/
                        eqp.File.SamplingCount.ToString(),     /*6  SampleRatio (机台上报的Sampling Unit栏位值)*/
                        string.Empty                          /*7  Reason Code (CF 不使用)*/
                    };
                    //呼叫MES方法
                    Invoke(eServiceName.MESService, "InspectionModeChanged", _data);
                }
                #endregion

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void SamplingRuleChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_SamplingRuleChangeReportReply") as Trx;
                outputdata[0][0][0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + SamplingRuleChangeReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + SamplingRuleChangeReportTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + SamplingRuleChangeReportTimeout, false, ParameterManager["T2"].GetInteger(), 
                        new System.Timers.ElapsedEventHandler(SamplingRuleChangeReportReplyTimeout), trackKey);
                }
                LogInfo(MethodBase.GetCurrentMethod().Name + "()", 
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SAMPLING RULE CHANGE REPORT REPLY, SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void SamplingRuleChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SAMPLING RULE CHANGE REPORT TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                SamplingRuleChangeReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Sampling Rule Change Command]
        public void SamplingRuleChangeCommand(string eqpNo, string samplingRule, string samplingUnit, string sideInformation, string trackKey)
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });

                    throw new Exception(err);
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM_MODE=[OFF], CAN NOT CHAGE SAMPLING RULE!", eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                string trxName = string.Format("{0}_SamplingRuleChangeCommand", eqp.Data.NODENO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata[0][0][0].Value = samplingRule;
                outputdata[0][0][1].Value = samplingUnit;
                outputdata[0][0][2].Value = sideInformation;
                outputdata[0][1][0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata[0][1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey; //UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);

                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, SamplingRuleChangeTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(SamplingRuleChangeCommandReplyTimeout), outputdata.TrackKey);

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SAMPLING_RULE=[{2}] SAMPLING_UNIT=[{3}] SIDE_INFORMATION=[{4}], SET BIT=[ON].", eqp.Data.NODENO,
                        outputdata.TrackKey, samplingRule, samplingUnit, sideInformation));

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SamplingRuleChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData[0][0][0].Value);

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), (int)retCode, retCode.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, SamplingRuleChangeTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;

                string trxName = string.Format("{0}_SamplingRuleChangeCommand", inputData.Metadata.NodeNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata[0][0].IsDisable = true;
                outputdata[0][1][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Repository.Add(inputData.Name, inputData);

                LogInfo("SamplingRuleChangeCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void SamplingRuleChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], SamplingRuleChangeTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SAMPLING RULE CHANGE COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_SamplingRuleChangeCommand") as Trx;
                outputdata[0][0].IsDisable = true;
                outputdata[0][1][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion

        #region [Ionizer Fan Enable Mode Change]
        public void IonizerFanEnableModeChange(Trx inputData)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                char[] ionizerFanEnableMode = inputData.EventGroups[0].Events[0].Items[0].Value.ToArray();
                string log = string.Empty;

                for (int i = 0; i < ionizerFanEnableMode.Length; i++)
                {
                    log += string.Format(" FAN_NO#{0}=[{1}]({2})", (i + 1).ToString("00"), ionizerFanEnableMode[i],
                        (eEnableDisable)int.Parse(ionizerFanEnableMode[i].ToString()));
                }
                Repository.Add(inputData.Name, inputData);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}]{2}.", inputData.Metadata.NodeNo,
                    inputData.TrackKey, log));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion
        
        #region [EQ to EQ Interlock]
        public void EQtoEQInterlock(Trx inputData)
        {
            try
            {
                Repository.Add(inputData.Name, inputData);

                string log = string.Empty;
                string _name = inputData.EventGroups[0].Events[0].Name;
                string[] _data = _name.Split('_');

                if (_data.Length.Equals(3) && _data[2].Substring(0, _data[2].Length - 1).ToUpper().Equals("EQUIPMENTSTATUSTYPE"))
                {
                    char[] values = inputData.EventGroups[0].Events[0].Items[0].Value.ToArray();
                    if (_data[2].ToUpper().Equals("EQUIPMENTSTATUSTYPE1"))
                    {
                        log += string.Format(" NORMAL_STATUS(RUN/IDLE)=[{0}]", values[0]);
                        log += string.Format(" ABNORMAL_STATUS(STOP/PAUSE/SETUP)=[{0}].", values[1]);
                    }
                    else
                    {
                        log += string.Format(" NORMAL_STATUS(RUN/IDLE)=[{0}]", values[0]);
                        log += string.Format(" ABNORMAL_STATUS(STOP/SETUP)=[{0}]", values[1]);
                        log += string.Format(" PAUSE_STATUS(PAUSE)=[{0}].", values[2]);
                    }
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [EQtoEQInterlock{1}][{2}] {3}:{4}",
                        inputData.Metadata.NodeNo, inputData.IsInitTrigger ? "_Initial" : "", inputData.TrackKey,
                        inputData.EventGroups[0].Events[0].Items[0].Name, log));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [EQtoEQInterlock{1}][{2}] {3}=[{4}].",
                        inputData.Metadata.NodeNo, inputData.IsInitTrigger ? "_Initial" : "", inputData.TrackKey,
                        inputData.EventGroups[0].Events[0].Items[0].Name, inputData.EventGroups[0].Events[0].Items[0].Value));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Process Pause Command]
        public void ProcessPauseCommand(string eqpNo, string processPause, string unitNo, string trackKey)
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!",MethodBase.GetCurrentMethod().Name, eqpNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    throw new Exception(err);
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND PROCESS PAUSE COMMAND!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                string trxName = string.Format("{0}_ProcessPauseCommand", eqp.Data.NODENO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[ePLC.ProcessPauseCommand_ProcessPause].Value = processPause;
                outputdata.EventGroups[0].Events[0].Items[ePLC.ProcessPauseCommand_UnitNo].Value = unitNo;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, ProcessPauseCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), 
                    new System.Timers.ElapsedEventHandler(ProcessPauseCommandReplyTimeout), outputdata.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] UNIT_NO=[{2}] PROCESS_PAUSE=[{3}], SET BIT=[ON].", 
                        eqp.Data.NODENO, trackKey, unitNo, processPause));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ProcessPauseCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                if (retCode == eReturnCode1.NG)
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] 
                    { inputData.TrackKey, inputData.Metadata.NodeNo, string.Format("{0} Process Pause Command Reply NG !", inputData.Metadata.NodeNo) });

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), (int)retCode, retCode.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, ProcessPauseCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;

                string trxName = string.Format("{0}_ProcessPauseCommand", inputData.Metadata.NodeNo);

                #region MES Reply
                Equipment eqp = null;
                if (mesProcPauseCmdKey.ContainsKey(inputData.Metadata.NodeNo))
                {
                    //eqp.Data.NODENO + "," + unitNo + "," + processPauseCmd
                    string[] sArray = mesProcPauseCmdKey[inputData.Metadata.NodeNo].Split(',');
                    eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                    string mesPAUSECOMMAND = sArray[2];
                    string exeSULT = retCode == eReturnCode1.OK ? "Y" : "N";
                    mesProcPauseCmdKey.Remove(inputData.Metadata.NodeNo);
                    //MachinePauseCommandReply(string trxID,Equipment eqp,string mespausecmd,string exeresult)
                    Invoke(eServiceName.MESService, "MachinePauseCommandReply", new object[] { inputData.TrackKey, eqp, mesPAUSECOMMAND, exeSULT });
                }
                #endregion

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ePLC.BitResult_OFF;
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Repository.Add(inputData.Name, inputData);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey));                

                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                        string.Format("EQUIPMENT=[{0}] PROCESS PAUSE COMMAND REPLY RETURN CODE=[{1}]", eqp.Data.NODENO, retCode)});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ProcessPauseCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], ProcessPauseCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PROCESS PAUSE COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_ProcessPauseCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (mesProcPauseCmdKey.ContainsKey(sArray[0]))
                    mesProcPauseCmdKey.Remove(sArray[0]);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("Process Pause Command Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        /// <summary>
        /// 會記錄是pause或是Resume命令，因為必須回覆給MES  MachinePauseCommandReply
        /// </summary>
        /// <param name="trxid">MES TrxID</param>
        /// <param name="eqpNo">Eqp No</param>
        /// <param name="processPause">Pause or Resume</param>
        /// <param name="unitNo">command receive Unit No</param>
        public void MES_ProcessPauseCommand(string eqpNo, string processPause, string unitNo, string trxid )
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, eqpNo);
                    //MachinePauseCommandReply(string trxID,Equipment eqp,string mespausecmd,string exeresult)
                    Invoke(eServiceName.MESService, "MachinePauseCommandReply", new object[] { trxid, eqp, processPause, "N" });
                    throw new Exception(err);
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND PROCESS PAUSE COMMAND!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    //MachinePauseCommandReply(string trxID, Equipment eqp,string mespausecmd,string exeresult)
                    Invoke(eServiceName.MESService, "MachinePauseCommandReply", new object[] { trxid, eqp, processPause, "N" });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                string trxName = string.Format("{0}_ProcessPauseCommand", eqp.Data.NODENO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = processPause;
                outputdata.EventGroups[0].Events[0].Items[1].Value = unitNo;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trxid;
                SendPLCData(outputdata);

                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, ProcessPauseCommandTimeout);

                #region MES Reply
                string processPauseCmd = processPause == "1" ? "Y" : "N";
                if (mesProcPauseCmdKey.ContainsKey(eqp.Data.NODENO))
                    mesProcPauseCmdKey.Remove(eqp.Data.NODENO);
                mesProcPauseCmdKey.Add(eqp.Data.NODENO, eqp.Data.NODENO + "," + unitNo + "," + processPauseCmd);
                #endregion

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), 
                    new System.Timers.ElapsedEventHandler(ProcessPauseCommandReplyTimeout), outputdata.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PROCESS_PAUSE=[{2}] UNIT_NO=[{3}], SET BIT=[ON].",
                    eqp.Data.NODENO, trxid, processPause, unitNo));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Cool Run Count Report]
        public void CoolRunCountReport(Trx inputData)
        {
            try
            {
                
                string coolRunSetCount = inputData.EventGroups[0].Events[0].Items[0].Value;// SPEC 好像改了，不能用名称去找 20150206 Tom 
                string coolRunRemainCount = inputData.EventGroups[0].Events[0].Items[1].Value;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqpNo));

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] COOLRUN: SET_COUNT=[{2}] REMAIN_COUNT=[{3}].",
                    eqp.Data.NODENO, inputData.TrackKey, coolRunSetCount, coolRunRemainCount));

                lock (line)
                {
                    line.File.CoolRunSetCount = Int32.Parse(coolRunSetCount);
                    line.File.CoolRunRemainCount = Int32.Parse(coolRunRemainCount);
                }
                //增加上报Line Status Report 告诉OPI 现在的Cool Run Count
                Invoke(eServiceName.UIService, "LineStatusReport", new object[] { inputData.TrackKey, line });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Cool Run Count Set Report]
        public void CoolRunCountSetReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
                    CoolRunCountSetReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                int coolRunCount = int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] COOLRUN_COUNT_SET_REPORT=[{2}].", eqp.Data.NODENO,
                        inputData.TrackKey, coolRunCount));
                CoolRunCountSetReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CoolRunCountSetReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_CoolRunCountSetReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + CoolRunCountSetReportTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + CoolRunCountSetReportTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + CoolRunCountSetReportTimeout, false, ParameterManager["T2"].GetInteger(), 
                        new System.Timers.ElapsedEventHandler(CoolRunCountSetReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", 
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void CoolRunCountSetReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP Reply, Send Wait Timeout Report Timeout Set Bit (OFF).", sArray[0], trackKey));

                CoolRunCountSetReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region [Cool Run Count Set Command]
        public void CoolRunCountSetCommand(string eqpNo, int coolRunCount, string trackKey)
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, eqpNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    throw new Exception(err);
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND COOLRUN COUNT SET COMMAND!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                string trxName = string.Format("{0}_CoolRunCountSetCommand", eqp.Data.NODENO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = coolRunCount.ToString();
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);

                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, CoolRunCountSetCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), 
                    new System.Timers.ElapsedEventHandler(CoolRunCountSetCommandReplyTimeout), outputdata.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] COOL-RUN_COUNT=[{2}], SET BIT=[ON]", eqp.Data.NODENO,
                        outputdata.TrackKey, coolRunCount));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CoolRunCountSetCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "CoolRunCountSetCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), (int)retCode, retCode.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, CoolRunCountSetCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;

                string trxName = string.Format("{0}_CoolRunCountSetCommand", inputData.Metadata.NodeNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Repository.Add(inputData.Name, inputData);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "CoolRunCountSetCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, retCode));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CoolRunCountSetCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], CoolRunCountSetCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] COOL-RUN SET COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_CoolRunCountSetCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Transfer Stop Command]
        public void TransferStopCommand(string eqpNo, string transferStop, string trackKey)
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, eqpNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    throw new Exception(err);
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND TRANSFER STOP COMMAND!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                string trxName = string.Format("{0}_TransferStopCommand", eqp.Data.NODENO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = transferStop;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, TransferStopCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), 
                    new System.Timers.ElapsedEventHandler(TransferStopCommandReplyTimeout), outputdata.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] TRANSFER_STOP=[{2}], SET BIT=[ON].", eqp.Data.NODENO,
                        outputdata.TrackKey, transferStop));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void TransferStopCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), (int)retCode, retCode.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, TransferStopCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;
                string trxName = string.Format("{0}_TransferStopCommand", inputData.Metadata.NodeNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Repository.Add(inputData.Name, inputData);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                        string.Format("EQUIPMENT=[{0}] TRANSFER STOP COMMAND REPLY RETURN CODE=[{1}]", eqp.Data.NODENO, retCode)});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void TransferStopCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], TransferStopCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] TRANSFER STOP COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_TransferStopCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("Transfer Stop Command Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region Material Status Request add by hujunpeng 20180523
        public void MaterialStatusRequest(string eqpNo, string MaterialStatusRequest, string trackKey)
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, eqpNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    throw new Exception(err);
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND MATERIAL STATUS REQUEST !", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                string trxName = string.Format("{0}_MaterialStatusRequest", eqp.Data.NODENO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputdata.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, MaterialStatusRequestTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(MaterialStatusReplyTimeout), outputdata.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Material Status Reply=[{2}], SET BIT=[ON].", eqp.Data.NODENO,
                        outputdata.TrackKey, MaterialStatusRequest));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        public void MaterialStatusReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                //Equipment eqp2 = ObjectManager.EquipmentManager.GetEQP("L4");
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                if (triggerBit == eBitResult.ON)
                {
                    eMaterialStatus MaterialStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                    string MaterialValue = inputData.EventGroups[0].Events[0].Items[1].Value;
                    string materialValue2 = int.Parse(MaterialValue) > 20000 ? "20000" : MaterialValue;
                    switch (materialValue2.Length)
                    {
                        case 4:
                            materialValue2 = "0" + materialValue2;
                            break;

                        case 3:
                            materialValue2 = "00" + materialValue2;
                            break;

                        case 2:
                            materialValue2 = "000" + materialValue2;
                            break;

                        case 1:
                            materialValue2 = "0000" + materialValue2;
                            break;

                        default:
                            break;

                    }
                    string MaterialID = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                    if (!string.IsNullOrEmpty(eqp.File.PrID1))
                    {
                        if (eqp.File.PrID1.Substring(24, 1) == "I")
                        {
                            lock (eqp.File)
                            {
                                eqp.File.PrID1 = (MaterialID + (MaterialStatus.ToString()).Substring(0, 1) + materialValue2).ToUpper();
                                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                            }
                            List<MaterialEntity> materialList = new List<MaterialEntity>();
                            MaterialEntity materialE = new MaterialEntity();
                            materialE.MaterialWeight = materialValue2;
                            materialE.MaterialID = MaterialID;
                            materialE.MaterialStatus = MaterialStatus;
                            materialE.MaterialSlotNo = "1";
                            materialList.Add(materialE);
                            ObjectManager.MaterialManager.AddMaterial(materialE);
                            Invoke(eServiceName.UIService, "MaterialRealWeightReport", new object[] { inputData.TrackKey, materialList });

                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}].",
                            inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), eqp.File.PrID1));

                            //if (eqp != null)
                            //{
                            //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                            //string.Format("EQUIPMENT=[{0}] Material Status Reply RETURN CODE=[{1}]", eqp.Data.NODENO, eqp.Data.NODENAME)});
                            //}
                        }
                    }
                    if (!string.IsNullOrEmpty(eqp.File.PrID2))
                    {
                        if (eqp.File.PrID2.Substring(24, 1) == "I")
                        {
                            lock (eqp.File)
                            {
                                eqp.File.PrID2 = (MaterialID + (MaterialStatus.ToString()).Substring(0, 1) + materialValue2).ToUpper();
                                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                            }
                            List<MaterialEntity> materialList = new List<MaterialEntity>();
                            MaterialEntity materialE = new MaterialEntity();
                            materialE.MaterialWeight = materialValue2;
                            materialE.MaterialID = MaterialID;
                            materialE.MaterialStatus = MaterialStatus;
                            materialE.MaterialSlotNo = "2";
                            materialList.Add(materialE);
                            ObjectManager.MaterialManager.AddMaterial(materialE);
                            Invoke(eServiceName.UIService, "MaterialRealWeightReport", new object[] { inputData.TrackKey, materialList });

                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}].",
                            inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), eqp.File.PrID2));

                            //if (eqp != null)
                            //{
                            //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                            //string.Format("EQUIPMENT=[{0}] Material Status Reply RETURN CODE=[{1}]", eqp.Data.NODENO, eqp.Data.NODENAME)});
                            //}
                        }
                    }
                    //if (!string.IsNullOrEmpty(eqp.File.PrID3))
                    //{
                    //    if (eqp.File.PrID3.Substring(24, 1) == "I")
                    //    {
                    //        lock (eqp.File)
                    //        {
                    //            eqp.File.PrID3 = (MaterialID + (MaterialStatus.ToString()).Substring(0, 1) + materialValue2).ToUpper();
                    //            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                    //        }
                    //        List<MaterialEntity> materialList = new List<MaterialEntity>();
                    //        MaterialEntity materialE = new MaterialEntity();
                    //        materialE.MaterialWeight = materialValue2;
                    //        materialE.MaterialID = MaterialID;
                    //        materialE.MaterialStatus = MaterialStatus;
                    //        materialE.MaterialSlotNo = "3";
                    //        materialList.Add(materialE);
                    //        ObjectManager.MaterialManager.AddMaterial(materialE);
                    //        Invoke(eServiceName.UIService, "MaterialRealWeightReport", new object[] { inputData.TrackKey, materialList });

                    //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}].",
                    //        inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), eqp.File.PrID3));

                    //        //if (eqp != null)
                    //        //{
                    //        //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                    //        //string.Format("EQUIPMENT=[{0}] Material Status Reply RETURN CODE=[{1}]", eqp.Data.NODENO, eqp.Data.NODENAME)});
                    //        //}
                    //    }
                    //}
                    //if (!string.IsNullOrEmpty(eqp.File.PrID4))
                    //{
                    //    if (eqp.File.PrID4.Substring(24, 1) == "I")
                    //    {
                    //        lock (eqp.File)
                    //        {
                    //            eqp.File.PrID4 = (MaterialID + (MaterialStatus.ToString()).Substring(0, 1) + materialValue2).ToUpper();
                    //            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                    //        }
                    //        List<MaterialEntity> materialList = new List<MaterialEntity>();
                    //        MaterialEntity materialE = new MaterialEntity();
                    //        materialE.MaterialWeight = materialValue2;
                    //        materialE.MaterialID = MaterialID;
                    //        materialE.MaterialStatus = MaterialStatus;
                    //        materialE.MaterialSlotNo = "4";
                    //        materialList.Add(materialE);
                    //        ObjectManager.MaterialManager.AddMaterial(materialE);
                    //        Invoke(eServiceName.UIService, "MaterialRealWeightReport", new object[] { inputData.TrackKey, materialList });

                    //        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    //        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}].",
                    //        inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), eqp.File.PrID4));

                    //        //if (eqp != null)
                    //        //{
                    //        //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                    //        //string.Format("EQUIPMENT=[{0}] Material Status Reply RETURN CODE=[{1}]", eqp.Data.NODENO, eqp.Data.NODENAME)});
                    //        //}
                    //    }
                    //}
                    {
                        List<MaterialEntity> materialList = new List<MaterialEntity>();
                        MaterialEntity materialE = new MaterialEntity();
                        materialE.MaterialWeight = string.Empty;
                        materialE.MaterialID = string.Empty;
                        materialE.MaterialStatus = eMaterialStatus.NONE;
                        materialE.MaterialSlotNo = "0";
                        materialList.Add(materialE);
                        ObjectManager.MaterialManager.AddMaterial(materialE);
                        Invoke(eServiceName.UIService, "MaterialRealWeightReport", new object[] { inputData.TrackKey, materialList });
                    }


                }


                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, MaterialStatusRequestTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;
                string trxName = string.Format("{0}_MaterialStatusRequest", inputData.Metadata.NodeNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                // outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Repository.Add(inputData.Name, inputData);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void MaterialStatusReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], MaterialStatusRequestTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Material Status Reply Timeout, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_MaterialStatusRequest") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                //Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                //if (eqp != null)
                //{
                //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                //        string.Format("Material Status Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                //}
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [Process Stop Command]
        public void ProcessStopCommand(string eqpNo, string unitNo, string processStop, string trackKey)
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, eqpNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    throw new Exception(err);
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND PROCESS STOP COMMAND!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                string trxName = string.Format("{0}_ProcessStopCommand", eqp.Data.NODENO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = processStop;
                outputdata.EventGroups[0].Events[0].Items[1].Value = unitNo;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, ProcessStopCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), 
                    new System.Timers.ElapsedEventHandler(ProcessStopCommandReplyTimeout), outputdata.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] UNIT_NO=[{2}] PROCESS_STOP=[{3}], SET BIT=[ON]", eqp.Data.NODENO,
                        outputdata.TrackKey, unitNo, processStop));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ProcessStopCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                //if (retCode == eReturnCode1.NG)
                //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, inputData.Metadata.NodeNo, string.Format("{0} Process Stop Command Reply NG !", inputData.Metadata.NodeNo) });

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] RETURN_CODE=[{3}]({4}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), (int)retCode, retCode.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, ProcessStopCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                if (triggerBit == eBitResult.OFF) return;
                string trxName = string.Format("{0}_ProcessStopCommand", inputData.Metadata.NodeNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Repository.Add(inputData.Name, inputData);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "ProcessStopCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                        string.Format("EQUIPMENT=[{0}] PROCESS STOP COMMAND REPLY RETURN CODE=[{1}]", eqp.Data.NODENO, retCode)});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ProcessStopCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], ProcessStopCommandTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PROCESS STOP COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_ProcessStopCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("Process Stop Command Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion

        #region [Unit ENG Mode Status]
        public void UnitENGModeStatus(Trx inputData)
        {
            try
            {
                string status = inputData.EventGroups[0].Events[0].Items[0].Value;

                string eqpNo = inputData.Metadata.NodeNo;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No[{0}] in EquipmentEntity!", eqpNo));

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] UNIT_ENG_MODE_STATUS=[{2}].",
                    eqp.Data.NODENO, inputData.TrackKey, status));

                lock (eqp)
                {
                    eqp.File.UnitENGMode = status;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Changer Plan Request Report
        public void ChangerPlanRequestReport(Trx inputData)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                string currentPlanId = inputData.EventGroups[0].Events[0].Items[0].Value;
                string cassetteID = inputData.EventGroups[0].Events[0].Items[1].Value;
                eCASSETTE_MAP_FLAG flag = (eCASSETTE_MAP_FLAG)int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", eqpNo));

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    ChangerPlanRequestReportReply_Off(eqpNo, inputData.TrackKey);
                    return;
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CURRENT_PLAN_ID=[{3}] CASSETTE_ID=[{4}] CASSETTE_MAP_FLAG=[{5}]({6}).",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, currentPlanId, cassetteID, (int)flag, flag.ToString()));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    IList<string> sourceList = new List<string>();
                    IList<string> targetList = new List<string>();

                    if (string.IsNullOrEmpty(currentPlanId.Trim()) && flag == eCASSETTE_MAP_FLAG.CASSETTE_MAP_EXIST)
                    {
                        ChangerPlanRequestReportReply_On(eReturnCode1.NG, line.Data.LINEID, "", sourceList, targetList, inputData.TrackKey);
                        this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", 
                            "PLAN ID IS EMPTY!");
                    }
                    else
                    {
                        bool ret = false;
                        if (flag == eCASSETTE_MAP_FLAG.CASSETTE_MAP_EMPTY)
                            ret = ObjectManager.PlanManager.ReloadOfflinePlan(eqp.Data.LINEID, cassetteID.Trim());
                        else
                            ret = ObjectManager.PlanManager.ReloadOfflinePlan(eqp.Data.LINEID, cassetteID.Trim(), currentPlanId);

                        if (ret == false)
                        {
                            this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "[OFFLINE] Can't find changer plan Data in Database!");
                            ChangerPlanRequestReportReply_On(eReturnCode1.NG, line.Data.LINEID, currentPlanId, sourceList, targetList, inputData.TrackKey);
                        }
                        else
                        {
                            string nextPlanID = string.Empty;
                            IList<SLOTPLAN> plans = ObjectManager.PlanManager.GetProductPlans(out nextPlanID);

                            if (plans == null || plans.Count == 0)
                            {
                                this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    "[OFFLINE] Can't find changer plan Data in Database!");
                                ChangerPlanRequestReportReply_On(eReturnCode1.NG, line.Data.LINEID, currentPlanId, sourceList, targetList, inputData.TrackKey);
                            }
                            else
                            {
                                foreach (SLOTPLAN s in plans)
                                {
                                    if (sourceList.IndexOf(s.SOURCE_CASSETTE_ID) < 0)
                                    {
                                        if (!string.IsNullOrEmpty(s.SOURCE_CASSETTE_ID.Trim()))
                                        {
                                            sourceList.Add(s.SOURCE_CASSETTE_ID);
                                        }
                                    }

                                    if (targetList.IndexOf(s.TARGET_CASSETTE_ID) < 0)
                                    {
                                        if (!string.IsNullOrEmpty(s.TARGET_CASSETTE_ID.Trim()))
                                        {
                                            targetList.Add(s.TARGET_CASSETTE_ID);
                                        }
                                    }
                                }
                                if (sourceList.Count == 0 || targetList.Count == 0)
                                {
                                    sourceList.Clear();
                                    targetList.Clear();
                                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[OFFLINE] The Plan({0}) can't find Source Cst/Target Cst in Database!", nextPlanID));
                                    ChangerPlanRequestReportReply_On(eReturnCode1.NG, line.Data.LINEID, currentPlanId, sourceList, targetList, inputData.TrackKey);
                                }
                                else
                                    ChangerPlanRequestReportReply_On(eReturnCode1.OK, line.Data.LINEID, nextPlanID, sourceList, targetList, inputData.TrackKey);
                            }
                        }
                    }
                }
                else
                {
                    currentPlanId = (flag == eCASSETTE_MAP_FLAG.CASSETTE_MAP_EMPTY ? "" : currentPlanId);
                    Invoke(eServiceName.MESMessageService, "ChangePlanRequest",
                        new object[] { inputData.TrackKey, line.Data.LINEID, cassetteID,  currentPlanId});
                }

                //Add by Kasim 20150506
                ObjectManager.PlanManager.SavePlanStatusInDB(currentPlanId, ePLAN_STATUS.REQUEST);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    ChangerPlanRequestReportReply_On(eReturnCode1.NG, "", "", new List<string>(),
                        new List<string>(), inputData.TrackKey);
                }
            }
        }

        public void ChangerPlanRequestReportReply_On(eReturnCode1 retCode, string lineID, string planID, IList<string> sourceList,
            IList<string> targetList, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat("L2_ChangerPlanRequestReportReply") as Trx;
                outputdata.ClearTrxWith0();
                Event evt = outputdata.EventGroups[0].Events[0];
                string log = string.Empty;
                evt.Items[0].Value = ((int)retCode).ToString();
                evt.Items[1].Value = planID;

                log = "\r\n\t\t\tSOURCE_CSTID:";

                for (int i = 0; i < 10; i++)
                {
                    if (sourceList.Count > i)
                        evt.Items[2 + i].Value = sourceList[i];
                    else
                        evt.Items[2 + i].Value = string.Empty;

                    log += string.Format(" {0}=[{1}]", (i + 1).ToString("00"), evt.Items[2 + i].Value.PadRight(10));
                }

                log += "\r\n\t\t\tTARGET_CSTID:";
                for (int i = 0; i < 10; i++)
                {
                    if (targetList.Count > i)
                        evt.Items[12 + i].Value = targetList[i];
                    else
                        evt.Items[12 + i].Value = string.Empty;

                    log += string.Format(" {0}=[{1}]", (i + 1).ToString("00"), evt.Items[12 + i].Value.PadRight(10));
                }
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer("L2_" + ChangerPlanRequestTimeout))
                {
                    _timerManager.TerminateTimer("L2_" + ChangerPlanRequestTimeout);
                }

                _timerManager.CreateTimer("L2_" + ChangerPlanRequestTimeout, false,
                    ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ChangerPlanRequestReportReplyTimeout), trackKey);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT=L2] [BCS -> EQP][{0}] RETURN_CODE=[{1}] NEXT_PLAN_ID=[{2}], SET BIT=[ON].{3}",
                    trackKey, retCode, planID, log));

                //if (retCode == eReturnCode1.OK)
                //{
                //    Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { lineID });
                //}
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ChangerPlanRequestReportReply_Off(string eqpNo, string trackKey)
        {
            try
            {
                if (_timerManager.IsAliveTimer(eqpNo + "_" + ChangerPlanRequestTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ChangerPlanRequestTimeout);
                }

                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_ChangerPlanRequestReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)(eBitResult.OFF)).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET=[OFF].",
                    eqpNo, trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ChangerPlanRequestReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CHANGER PLAN REQUEST REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                ChangerPlanRequestReportReply_Off(sArray[0], trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region ChangerPlanStatusReport
        public void ChangerPlanStatusReport(Trx inputData)
        {
            try
            {
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                string planID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                ePLAN_STATUS status = (ePLAN_STATUS)int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                ePLAN_STATUS oldSts = line.File.PlanStatus;

                if (inputData.IsInitTrigger)
                {
                    if (oldSts != status)
                    {
                        if (status == ePLAN_STATUS.START)
                            Invoke(eServiceName.MESService, "ChangePlanStarted", new object[] { inputData.TrackKey, line.Data.LINEID, planID });
                        else if (status == ePLAN_STATUS.NO_PLAN)
                            ObjectManager.PlanManager.RemoveChangePlan();
                    }
                    line.File.PlanStatus = status;
                    line.File.CurrentPlanID = planID;
                    ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                    ObjectManager.PlanManager.SavePlanStatusInDB(planID, status);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [ChangerPlanStatusReport_Initial][{1}] PLAN_ID=[{2}] PLAN_STATUS=[{3}]({4}).", 
                        inputData.Metadata.NodeNo, inputData.TrackKey, planID, (int)status, status.ToString()));

                    Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                    return;
                }

                if (triggerBit == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));
                    ChangerPlanStatusReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                ObjectManager.PlanManager.SavePlanStatusInDB(planID, status);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CURRENT_PLAN_ID=[{3}] PLAN_STATUS=[{4}]({5}).",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, planID, (int)status, status.ToString()));

                ChangerPlanStatusReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);

                line.File.PlanStatus = status;
                line.File.CurrentPlanID = planID;
                ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                switch (status)
                {
                    case ePLAN_STATUS.NO_PLAN:
                        ObjectManager.PlanManager.RemoveChangePlan();
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Delete BCS All Plan!");
                        break;
                    case ePLAN_STATUS.READY: 
                        // 重下資料
                        #region 重下資料給Port
                        // 當機台回OK時, 要去找目前是WaitForStard,WaitForProcessing及Inprocessing的Port, 找看看有沒有在PLAN裡, 如果有要下資料
                        IList<Port> ports = ObjectManager.PortManager.GetPorts().Where(p =>
                            p.File.Type == ePortType.LoadingPort && (
                            p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING ||
                            p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND ||
                            p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING)).ToList<Port>();

                        foreach (Port port in ports)
                        {
                            string trxName2 = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", port.Data.NODENO, port.Data.PORTNO.PadLeft(2, '0'));
                            Trx portTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName2, false }) as Trx;

                            if (portTrx != null)
                            {
                                IList<Job> jobs = new List<Job>();
                                GetPortJobData(portTrx, port, ref jobs);

                                if (jobs.Count() > 0)
                                {
                                    // 資料來源由 PlanManager
                                    IList<SLOTPLAN> plans = ObjectManager.PlanManager.GetProductPlansByCstID(planID, port.File.CassetteID.Trim());
                                    if (plans.Count() > 0)
                                    {
                                        bool different = false;
                                        foreach (Job j in jobs)
                                        {
                                            SLOTPLAN plan = plans.FirstOrDefault(p => p.PRODUCT_NAME.Trim() == j.GlassChipMaskBlockID.Trim());
                                            if (plan != null)
                                            {
                                                if (j.TargetCSTID.Trim() != plan.TARGET_CASSETTE_ID.Trim())
                                                {
                                                    different = true;
                                                    j.TargetCSTID = plan.TARGET_CASSETTE_ID.Trim();
                                                    // 跟登京確認, 在Changer Mode只要有Plan就是要抽, 即使MES 的原資料是不抽, 也要turn On
                                                    if (!string.IsNullOrEmpty(j.TargetCSTID.Trim()))
                                                    {
                                                        j.SamplingSlotFlag = "1";
                                                    }
                                                }
                                                else
                                                    j.SamplingSlotFlag = "0";
                                                ObjectManager.JobManager.EnqueueSave(j);
                                            }
                                        }
                                        Equipment e = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                                        if (e != null && different)
                                        {
                                            Invoke(eServiceName.CassetteService, "CassetteMapDownload", new object[] { e, port, jobs, inputData.TrackKey });
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        break;
                    case ePLAN_STATUS.START:
                        Invoke(eServiceName.MESService, "ChangePlanStarted", new object[] { inputData.TrackKey, line.Data.LINEID, planID });
                        break;
                    case ePLAN_STATUS.END:
                         if (line.File.HostMode == eHostMode.OFFLINE)
                         {
                             ObjectManager.PlanManager.DeleteOffLinePlanInDB(planID);
                         }
                        break;
                }

                Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    ChangerPlanStatusReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void ChangerPlanStatusReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_ChangerPlanStatusReportReply") as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + ChangerPlanStatusTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + ChangerPlanStatusTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + ChangerPlanStatusTimeout, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(ChangerPlanStatusReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ChangerPlanStatusReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CHANGER PLAN STATUS REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                ChangerPlanStatusReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region Changer Plan Download Set Command
        public void ChangerPlanDownloadSetCommand(eReturnCode1 retCode, string eqpNo, string planID, IList<string> sourceList,
            IList<string> targetList, string trackKey)
        {
            try
            {
                Equipment eqp;
                string err = string.Empty;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("[{0}] CAN'T FIND EQUIPMENT_NO=[{1}] IN EQUIPMENTENTITY!", MethodBase.GetCurrentMethod().Name, eqpNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    throw new Exception(err);
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND CHANGER PLAN DOWNLOAD SET COMMAND!",
                        MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                string trxName = string.Format("{0}_ChangerPlanDownloadSetCommand", eqp.Data.NODENO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.ClearTrxWith0();
                Event evt = outputdata.EventGroups[0].Events[0];
                string log = string.Empty;
                evt.Items[0].Value = ((int)retCode).ToString();
                evt.Items[1].Value = planID;

                log = "\r\n\t\t\tSOURCE_CSTID:";

                for (int i = 0; i < 10; i++)
                {
                    if (sourceList.Count > i)
                        evt.Items[2 + i].Value = sourceList[i];
                    else
                        evt.Items[2 + i].Value = string.Empty;

                    log += string.Format(" {0}=[{1}]", (i + 1).ToString("00"), evt.Items[2 + i].Value.PadRight(10));
                }

                log += "\r\n\t\t\tTARGET_CSTID:";
                for (int i = 0; i < 10; i++)
                {
                    if (targetList.Count > i)
                        evt.Items[12 + i].Value = targetList[i];
                    else
                        evt.Items[12 + i].Value = string.Empty;

                    log += string.Format(" {0}=[{1}]", (i + 1).ToString("00"), evt.Items[12 + i].Value.PadRight(10));
                }

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, ChangerPlanDownloadSetTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(ChangerPlanDownloadSetCommandReplyTimeout), outputdata.TrackKey);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT=L2] [BCS -> EQP][{0}] RETURN_CODE=[{1}] NEXT_PLAN_ID=[{2}], SET BIT=[ON].{3}",
                    trackKey, retCode, planID, log));

                //Add by Kasim 20150506
                if (retCode == eReturnCode1.NG)
                    ObjectManager.PlanManager.SavePlanStatusInDB(planID, ePLAN_STATUS.CANCEL);

                /* 改在Ready時去判斷下資料
                if (retCode == eReturnCode1.OK)
                {
                    Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { eqp.Data.LINEID });
                    //ObjectManager.PlanManager.SavePlanStatusInDB(); 改抓機台的全部狀態

                    #region 重下資料給Port
                    // 當機台回OK時, 要去找目前是WaitForStard,WaitForProcessing及Inprocessing的Port, 找看看有沒有在PLAN裡, 如果有要下資料
                    IList<Port> ports = ObjectManager.PortManager.GetPorts().Where(p =>
                        p.File.Type == ePortType.LoadingPort && (
                        p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING ||
                        p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND ||
                        p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING)).ToList<Port>();

                    foreach (Port port in ports)
                    {
                        string trxName2 = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", port.Data.NODENO, port.Data.PORTNO.PadLeft(2, '0'));
                        Trx portTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName2, false }) as Trx;

                        if (portTrx != null)
                        {
                            IList<Job> jobs = new List<Job>();
                            GetPortJobData(portTrx, port, ref jobs);

                            if (jobs.Count() > 0)
                            {
                                // 資料來源由 PlanManager
                                IList<SLOTPLAN> plans = ObjectManager.PlanManager.GetProductPlansByCstID(planID, port.File.CassetteID.Trim());
                                if (plans.Count() > 0)
                                {
                                    foreach (Job j in jobs)
                                    {
                                        SLOTPLAN plan = plans.FirstOrDefault(p => p.PRODUCT_NAME.Trim() == j.GlassChipMaskBlockID.Trim());
                                        if (plan != null)
                                        {
                                            j.TargetCSTID = plan.TARGET_CASSETTE_ID.Trim();
                                            // 跟登京確認, 在Changer Mode只要有Plan就是要抽, 即使MES 的原資料是不抽, 也要turn On
                                            if (!string.IsNullOrEmpty(j.TargetCSTID.Trim()))
                                            {
                                                j.SamplingSlotFlag = "1";
                                            }
                                        }
                                    }
                                    Equipment e = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                                    Invoke(eServiceName.CassetteService, "CassetteMapDownload", new object[] { e, port, jobs });
                                }
                            }
                        }
                    }
                    #endregion
                }
                */
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ChangerPlanDownloadSetCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString()));

                string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, ChangerPlanDownloadSetTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;
                string trxName = string.Format("{0}_ChangerPlanDownloadSetCommand", inputData.Metadata.NodeNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ChangerPlanDownloadSetCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], ChangerPlanDownloadSetTimeout);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CHANGER PLAN DOWNLOAD SET COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_ChangerPlanDownloadSetCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey,  eqp.Data.LINEID, 
                        string.Format("Changer Plan Download Set Command Reply - EQUIPMENT=[{0}] \"T1 TIMEOUT\"", sArray[0])});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        // add common function by bruce 2015/7/3 原Cell special function 改為Common function 使用
        #region Operator Login Logout Report
        public void OperatorLoginLogoutReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    OperatorLoginLogoutReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string operatorID = inputData.EventGroups[0].Events[0].Items[0].Value;
                string touchPanelNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                string mode = inputData.EventGroups[0].Events[0].Items[2].Value;

                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE=[{2}], NODE=[{3}],OPERATORLOGINLOGOUTREPORT  OPERATORID =[{4}], TOUCHPANELNO =[{5}],MODE =[{6}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, operatorID, touchPanelNo, mode));

                OperatorLoginLogoutReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);


                #region MES MachineLoginReport 只有login 才要報
                if (mode != ((int)eLogInOutMode.Login).ToString())
                    return;

                object[] _data = new object[4]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,               /*2 machineName */
                    operatorID
                };
                //Send MES Data
                object retVal = base.Invoke(eServiceName.MESService, "MachineLoginReport", _data);
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    OperatorLoginLogoutReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        private void OperatorLoginLogoutReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_OperatorLoginLogoutReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + OperatorLoginLogoutTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + OperatorLoginLogoutTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + OperatorLoginLogoutTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(OperatorLoginLogoutReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void OperatorLoginLogoutReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] OPERATOR LOGIN LOGOUT REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                OperatorLoginLogoutReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion


        private void GetPortJobData(Trx inputData, Port port, ref IList<Job> jobs)
        {
            try
            {
                int slotNo = 0;
                for (int i = 0; i < inputData.EventGroups[0].Events[0].Items.Count; i += 2)
                {
                    slotNo++;
                    // Cassette Sequence No 為0時, 表示沒有資料, 找下一片
                    if (inputData.EventGroups[0].Events[0].Items[i].Value.Equals("0")) continue;
                    // Job Sequence No 為0時, 表示沒有資料, 找下一片
                    if (inputData.EventGroups[0].Events[0].Items[i + 1].Value.Equals("0")) continue;

                    Job job = ObjectManager.JobManager.GetJob(inputData.EventGroups[0].Events[0].Items[i].Value, inputData.EventGroups[0].Events[0].Items[i + 1].Value);

                    if (job == null)
                    {
                        //Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //    string.Format("[{0}]Can't find job information, Cassette Sequence No=[{1}], Job Sequence No=[{2}].",
                        //    port.File.CassetteStatus.ToString(), inputData.EventGroups[0].Events[0].Items[i].Value,
                        //    inputData.EventGroups[0].Events[0].Items[i + 1].Value));

                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] PORTNO=[{1}] STATUS=[{2}] JOB IS NULL , CAS_SEQ_NO=[{3}], JOB_SEQ_NO=[{4}] .",
                            inputData.Metadata.NodeNo, port.Data.PORTNO, port.File.CassetteStatus.ToString(), inputData.EventGroups[0].Events[0].Items[i].Value,
                            inputData.EventGroups[0].Events[0].Items[i + 1].Value));

                        continue;
                    }
                    if (int.Parse(job.ToSlotNo).Equals(0))
                    {
                        lock (job) job.ToSlotNo = slotNo.ToString();
                    }
                    jobs.Add(job);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }
    }
}

