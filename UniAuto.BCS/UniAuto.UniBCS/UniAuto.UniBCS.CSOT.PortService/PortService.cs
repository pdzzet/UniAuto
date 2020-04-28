using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.OpiSpec;
using System.Diagnostics;

namespace UniAuto.UniBCS.CSOT.PortService
{
    public partial class PortService : AbstractService
    {
        #region Timeout的常量變數
        private const string PortStsChangeTimeout = "PortStsChangeTimeout";
        private const string PortTypeChangeTimeout = "PortTypeChangeTimeout";
        private const string PortTypeChangeCommandTimeout = "PortTypeChangeCommandTimeout";
        private const string PortModeChangeTimeout = "PortModeChangeTimeout";
        private const string PortModeChangeCommandTimeout = "PortModeChangeCommandTimeout";
        private const string PortTransferModeChangeTimeout = "PortTransferModeChangeTimeout";
        private const string PortTransferModeChangeCommandTimeout = "PortTransferModeChangeCommandTimeout";
        private const string PortEnableModeChangeTimeout = "PortEnableModeChangeTimeout";
        private const string PortEnableModeChangeCommandTimeout = "PortEnableModeChangeCommandTimeout";
        private const string PortDownReportTimeout = "PortDownReportTimeout";
        private const string FileInfoNotifyReportReplyTimeOut = "FileInfoNotifyReportReplyByEQPTimeOut";
        private const string PortOperModeChangeTimeout = "PortOperModeChangeTimeout";
        #endregion

        public override bool Init()
        {
            return true;
        }

        #region [Port Status Change]
        object _obj = new object();

        /// <summary>
        /// 更新PortStatus
        /// </summary>
        /// <param name="inputData">PLC Trx物件</param>
        /// <param name="log">觸發者名稱(記Log用)</param>
        public void PortStatusChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                #region [取得EQP及Port資訊]
                Line line; Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_INFORMATION=[{0}] IN LINEENTITY!", ServerName));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));
                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion
                ePortStatus oldstatus = port.File.Status;
                string log;
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    RefreshPortStatus_CELL(line, port, inputData, out log);
                }
                else
                {
                    RefreshPortStatus_AC(line, port, inputData, out log);
                }

                if (port.File.Status != oldstatus)
                {
                    ReportMESPortStatusChange(port, inputData);
                    RecordPortHistory(inputData.TrackKey, eqp, port);
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA &&
                    port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP)
                {
                    lock (port) port.File.OPI_SubCstState = eOPISubCstState.NONE;
                }

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}]{3}.", eqp.Data.NODENO, sourceMethod, inputData.TrackKey, log));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortStatusChangeReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [取得EQP及Port資訊]
                Line line; Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_INFORMATION=[{0}] IN LINEENTITY!", ServerName));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortStatus oldstatus = port.File.Status;
                if (inputData.IsInitTrigger)
                {
                    PortStatusChangeReportUpdate(inputData, "PortStatusChangeReport_Initial");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF] PORT=[{2}] CSTID=[{3}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, port.Data.PORTNO, port.File.CassetteID));
                    PortStarusChangeReportReply(eqpNo, portNo, port.File.CassetteID, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                string log;
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    RefreshPortStatus_CELL(line, port, inputData, out log);
                }
                else
                {
                    RefreshPortStatus_AC(line, port, inputData, out log);
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}]{3}.", eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, log));

                //2014111 Tom
                PortStarusChangeReportReply(eqpNo, portNo, port.File.CassetteID, eBitResult.ON, inputData.TrackKey);

                //20150819 cc.kuang, for Port Type change at LDRQ
                // Modify by box.zhai 2017/03/01 Online不需要自动切换port type add "&&line.File.HostMode!==eHostMode.REMOTE"
               
                if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE && line.File.HostMode!=eHostMode.REMOTE)
                {
                   #region [online先不考虑自动切port type]

                    if (port.File.Status == ePortStatus.LR)
                    {                        
                        bool? bSourceCST = null;
                        string PlanID = string.Empty;
                        IList<SLOTPLAN> Plans;
                        //if (port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.UnloadingPort)
                            //port.File.CassetteID = port.File.PlannedCassetteID;

                        Plans = ObjectManager.PlanManager.GetProductPlansStandby(out PlanID);
                        if (Plans != null && Plans.FirstOrDefault(plan => plan.SOURCE_CASSETTE_ID.Trim().Equals(port.File.PlannedCassetteID)) != null)
                            bSourceCST = true;
                        if (Plans != null && Plans.FirstOrDefault(plan => plan.TARGET_CASSETTE_ID.Trim().Equals(port.File.PlannedCassetteID)) != null)
                            bSourceCST = false;

                        Plans = ObjectManager.PlanManager.GetProductPlans(out PlanID);
                        if (Plans != null && Plans.FirstOrDefault(plan => plan.SOURCE_CASSETTE_ID.Trim().Equals(port.File.PlannedCassetteID)) != null)
                            bSourceCST = true;
                        if (Plans != null && Plans.FirstOrDefault(plan => plan.TARGET_CASSETTE_ID.Trim().Equals(port.File.PlannedCassetteID)) != null)
                            bSourceCST = false;

                        if (bSourceCST.HasValue)
                        {
                            PortCommandRequest msg = new PortCommandRequest();
                            msg.HEADER.TRANSACTIONID = inputData.TrackKey;
                            msg.BODY.LINENAME = line.Data.LINEID;
                            msg.BODY.EQUIPMENTNO = eqp.Data.NODENO;
                            msg.BODY.PORTNO = port.Data.PORTNO;

                            if (bSourceCST.Value == true)
                            {
                                if (port.File.Type != ePortType.LoadingPort)
                                {
                                    msg.BODY.PORTCOMMAND = "1";
                                    PortTypeChangeCommand(line.Data.LINEID, msg);
                                }
                            }
                            else
                            {
                                if (port.File.Type != ePortType.UnloadingPort)
                                {
                                    msg.BODY.PORTCOMMAND = "2";
                                    PortTypeChangeCommand(line.Data.LINEID, msg);
                                }
                            }
                        }
                    }
                }
                #endregion
                else
                {
                    port.File.PlannedCassetteID = string.Empty;
                }              

                switch (port.File.Status)
                {
                    case ePortStatus.LR:
                        if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                        {
                            ChangerPlanResetCheck(line, eqp);
                        }

                        if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE)
                        {
                            if(port.File.Type == ePortType.UnloadingPort && port.File.Mode == ePortMode.ByGrade)    //20160715 Modify by Frank
                                AssignNewGrade(inputData, eqp, port);
                        }
                        break;
                    case ePortStatus.LC:
                        #region [Changer Plan Create(Loading Port)  && Whether Quit Cst(Unloading Port)]  Modify by box.zhai 2016 12/26

                        #region Online Case 说明
                        //改由在ChangerPlanCheck里面针对于Port Type做不同处理
                        //Loading Port: Check 当前Plan Count 是否已经为两个，是的话退Cst；否的话跟MES要Plan
                        //Unloading Port: Check 当前Plan Count 是否已经为两个，是的话，不在这两个Plan中直接退Cst；否的话，即使Target Cst不在Current Plan内，一样不会退Cst
                        #endregion

                       //     if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE && port.File.Type == ePortType.LoadingPort
                         //       && eqp.File.CIMMode == eBitResult.ON)
                         //   {
                       //         if (line.File.HostMode != eHostMode.OFFLINE) //offline don't get new plan
                       //             ChangerPlanCheck(line, eqp, port, inputData);
                      //      }                                                                                      //chn_old

                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE && eqp.File.CIMMode == eBitResult.ON)
                            {
                                if (line.File.HostMode != eHostMode.OFFLINE) //offline don't get new plan
                                    ChangerPlanCheckForOnline(line, eqp, port, inputData);
                            }

                        #endregion
                        Invoke(eServiceName.RobotCoreService, "RemoveSameEQMap", new object[2] { eqpNo, port.File.CassetteSequenceNo });
                        break;                        
                    case ePortStatus.UR:
                        { 
                            #region Delete CST DATA
                                    {
                                        Cassette cst = null;
                                        cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));
                                        if (cst != null)
                                        {
                                            lock (ObjectManager.CassetteManager) ObjectManager.CassetteManager.DeleteCassette(port.File.CassetteSequenceNo);
                                            cst.WriteFlag = false;
                                            ObjectManager.CassetteManager.EnqueueSave(cst);
                                        }
                                    }
                                    #endregion 
                        }
                        break;
                        if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                        {
                            ChangerPlanResetCheck(line, eqp);
                        }
                        break;
                    case ePortStatus.UC:
                        {
                            
                        }
                        break;
                }

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                ReportMESPortStatusChange(port, inputData);
                RecordPortHistory(inputData.TrackKey, eqp, port);

                //CF Special_9.2.28	Exposure Mask Check Command用
                //if (line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1 ||
                //    line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1 ||
                //    line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1)
                //{
                //    if (port.File.Type == ePortType.LoadingPort)
                //    {
                //        if (port.File.Status == ePortStatus.LC)
                //        {
                
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                    string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                    PortStarusChangeReportReply(inputData.Name.Split('_')[0], portNo, "", eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void PortStarusChangeReportReply(string eqpNo, string portNo, string cstID, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_Port#{1}PortStatusChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, PortStsChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                { 
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PortStarusChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] CSTID=[{3}], SET BIT=[{4}].",
                    eqpNo, trackKey, portNo, cstID, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PortStarusChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                PortStarusChangeReportReply(sArray[0], sArray[1], "", eBitResult.OFF, trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] PORT STATUS CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 更新資料到Port的變數資料
        /// </summary>
        /// <param name="line"></param>
        /// <param name="port"></param>
        /// <param name="inputData"></param>
        /// <param name="log"></param>Onl
        private void RefreshPortStatus_AC(Line line, Port port, Trx inputData, out string log)
        {
            log = string.Format(" PORT=[{0}]", port.Data.PORTNO);
            try
            {
                Event evt = inputData.EventGroups[0].Events[0];
                lock (port.File)
                {
                    port.File.CassetteID = evt.Items[3].Value;
                    log += string.Format(" CSTID=[{0}]", port.File.CassetteID);

                    port.File.Status = (ePortStatus)int.Parse(evt.Items[0].Value);
                    log += string.Format(" PORT_STATUS=[{0}]({1})", evt.Items[0].Value, port.File.Status.ToString());

                    port.File.CassetteStatus = (eCassetteStatus)int.Parse(evt.Items[1].Value);
                    log += string.Format(" CST_STATUS=[{0}]({1})", evt.Items[1].Value, port.File.CassetteStatus.ToString());

                    port.File.CassetteSequenceNo = evt.Items[2].Value;
                    log += string.Format(" CST_SEQNO=[{0}]", port.File.CassetteSequenceNo);

                    port.File.JobCountInCassette = evt.Items[4].Value;
                    log += string.Format(" JOB_COUNT_IN_CST=[{0}]", port.File.JobCountInCassette);

                    port.File.CompletedCassetteData = (eCompletedCassetteData)int.Parse(evt.Items[5].Value);
                    log += string.Format(" COMPLETED_CST_DATA=[{0}]({1})", evt.Items[5].Value, port.File.CompletedCassetteData.ToString());

                    port.File.OperationID = evt.Items[6].Value;
                    log += string.Format(" OPERATIONID=[{0}]", port.File.OperationID);

                    if (line.Data.LINETYPE != eLineType.CF.FCUPK_TYPE1.ToString())//UPK 增加到600片,設備不上報
                    {
                        port.File.JobExistenceSlot = evt.Items[7].Value.Substring(0, port.Data.MAXCOUNT);
                        log += string.Format(" JOB_EXISTENCE_SLOT=[{0}]", port.File.JobExistenceSlot);
                    }

                    port.File.LoadingCassetteType = (eLoadingCstType)int.Parse(evt.Items[8].Value);
                    log += string.Format(" LOADING_CST_TYPE=[{0}]({1})", evt.Items[8].Value, port.File.LoadingCassetteType.ToString());

                    port.File.QTimeFlag = (eQTime)int.Parse(evt.Items[9].Value);
                    log += string.Format(" Q_TIMEFLAG=[{0}]({1})", evt.Items[9].Value, port.File.QTimeFlag.ToString());

                    port.File.PartialFullFlag = (eParitalFull)int.Parse(evt.Items[10].Value);
                    log += string.Format(" PARTIAL_FULL_FLAG=[{0}]({1})", evt.Items[10].Value, port.File.PartialFullFlag.ToString());

                    if (line.Data.LINETYPE == eLineType.ARRAY.CAC_MYTEK)
                    {
                        //port.File.BACVByPassFlag = (eBACV_ByPass)int.Parse(evt.Items[11].Value);
                        //log += string.Format(" BACV_BYPASS_FLAG=[{0}]({1})", evt.Items[11].Value, port.File.BACVByPassFlag.ToString());

                        port.File.DistortionFlag = (eDistortion)int.Parse(evt.Items[13].Value);
                        log += string.Format(" DISTORTION_FLAG=[{0}]({1})", evt.Items[13].Value, port.File.DistortionFlag.ToString());

                        port.File.DirectionFlag = (eDirection)int.Parse(evt.Items[14].Value);
                        log += string.Format(" DIRECTION_FLAG=[{0}]({1})", evt.Items[14].Value, port.File.DirectionFlag.ToString());

                        port.File.GlassExist = (eGlassExist)int.Parse(evt.Items[15].Value);
                        log += string.Format(" GLASS_EXIST=[{0}]({1})", evt.Items[15].Value, port.File.GlassExist.ToString());
                    }

                    //if (line.Data.LINETYPE == eLineType.CF.FCSRT_TYPE1)
                    //{
                    //    port.File.MappingGrade = evt.Items["GradeType"].Value;
                    //    log += string.Format(" MAPPING_GRADE=[{0}]", port.File.MappingGrade);

                    //    port.File.ProductType = evt.Items["ProductType"].Value;
                    //    log += string.Format(" PRODUCT_TYPE=[{0}]", port.File.ProductType);
                    //}
                }
                ObjectManager.PortManager.EnqueueSave(port.File);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ChangerPlanCheck(Line line, Equipment eqp, Port port, Trx inputData)
        {
            string currPlanID = string.Empty;
            IList<SLOTPLAN> currPlans;
            IList<SLOTPLAN> standbyPlans;
            bool ret = false;

            try
            {
                currPlans = ObjectManager.PlanManager.GetProductPlans(out currPlanID);
                standbyPlans = ObjectManager.PlanManager.GetProductPlansStandby(out currPlanID);
                
                if (currPlans == null || currPlans.Count == 0)
                {
                    line.File.CurrentPlanID = string.Empty;
                    line.File.PlanStatus = ePLAN_STATUS.NO_PLAN;
                    ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                    ObjectManager.PlanManager.RemoveChangePlanStandby();
                }
                if (line.File.CurrentPlanID.Trim().Length == 0 || (line.File.PlanStatus == ePLAN_STATUS.END || line.File.PlanStatus == ePLAN_STATUS.CANCEL || line.File.PlanStatus == ePLAN_STATUS.NO_PLAN))
                {
                    line.File.PlanStatus = ePLAN_STATUS.NO_PLAN;
                    ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                    ObjectManager.PlanManager.RemoveChangePlan();
                    ObjectManager.PlanManager.RemoveChangePlanStandby();
                }

                //Recheck again
                currPlans = ObjectManager.PlanManager.GetProductPlans(out currPlanID);
                standbyPlans = ObjectManager.PlanManager.GetProductPlansStandby(out currPlanID);
                if ((currPlans == null || currPlans.Count == 0) && (standbyPlans == null || standbyPlans.Count == 0))
                {
                    foreach(Port p in ObjectManager.PortManager.GetPorts())
                        p.File.PlannedCassetteID = string.Empty;
                }

                //Check cst is Assigned, just notice but not get Plan !
                if (port.File.CassetteID.Trim().Equals(port.File.PlannedCassetteID.Trim()))
                {
                    this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + port.File.CassetteID + " Match the Port Assigned Cassette ID : " + port.File.PlannedCassetteID);
                    return;
                }
                else if (port.File.CassetteID.Trim().Length > 0 && port.File.PlannedCassetteID.Trim().Length > 0)
                {
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + port.File.CassetteID + " Not Match the Port Assigned Cassette ID : " + port.File.PlannedCassetteID);
                    return;
                }

                //check currPlan/standbyPlan, if has Plan, not reload check            
                if (currPlans == null || currPlans.Count == 0) //檢查是否有新Plan
                {
                    if (line.File.HostMode == eHostMode.OFFLINE)
                    {
                        ret = ObjectManager.PlanManager.ReloadOfflinePlan(eqp.Data.LINEID, port.File.CassetteID.Trim());
                        if (ret == false)
                        {
                            this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "[OFFLINE] Can't find " + port.File.CassetteID + " changer plan Data in Database!");
                            return;
                        }
                        currPlans = ObjectManager.PlanManager.GetProductPlans(out currPlanID);
                        if ((currPlans == null || currPlans.Count == 0))
                        {
                            this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "[OFFLINE] Load Curr Plan Data Error!");
                            return;
                        }
                        line.File.CurrentPlanID = currPlanID;
                        line.File.PlanStatus = ePLAN_STATUS.REQUEST;
                        ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                        ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                        Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                        CassetteBondToPort(line, currPlans, true, inputData.TrackKey);
                        ObjectManager.PlanManager.SavePlanStatusInDB(currPlanID, ePLAN_STATUS.REQUEST);
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        "[OFFLINE] Create " + port.File.CassetteID + " CurrPlan " + currPlanID + "!");
                    }
                    else
                    {
                        Invoke(eServiceName.MESMessageService, "ChangePlanRequest",
                            new object[] { inputData.TrackKey, line.Data.LINEID, port.File.CassetteID.Trim(), "" });
                    }
                    return;
                }

                if (standbyPlans == null || standbyPlans.Count == 0)　//檢查是否有新Plan
                {
                    bool incurrPlan = false;
                    if (currPlans != null && currPlans.Count > 0) //if in currplan, not need get standby plan
                        if (null != currPlans.FirstOrDefault(p => p.SOURCE_CASSETTE_ID.Trim().Equals(port.File.CassetteID.Trim()) ||
                                p.TARGET_CASSETTE_ID.Trim().Equals(port.File.CassetteID.Trim()) ))
                            incurrPlan = true;
                    if (!incurrPlan)
                    {
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                            ret = ObjectManager.PlanManager.ReloadOfflinePlanStandby(eqp.Data.LINEID, port.File.CassetteID.Trim());
                            if (ret == false)
                            {
                                this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    "[OFFLINE] Can't find " + port.File.CassetteID + " changer plan Data in Database!");
                                return;
                            }
                            standbyPlans = ObjectManager.PlanManager.GetProductPlansStandby(out currPlanID);
                            if ((standbyPlans == null || standbyPlans.Count == 0))
                            {
                                this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    "[OFFLINE] Load Standby Plan Data Error!");
                                return;
                            }
                            CassetteBondToPort(line, standbyPlans, false, inputData.TrackKey);
                            ObjectManager.PlanManager.SavePlanStatusInDB(currPlanID, ePLAN_STATUS.REQUEST);
                            this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            "[OFFLINE] Create " + port.File.CassetteID + " StandPlan " + currPlanID + "!");
                        }
                        else
                        {
                            Invoke(eServiceName.MESMessageService, "ChangePlanRequest",
                                new object[] { inputData.TrackKey, line.Data.LINEID, port.File.CassetteID.Trim(), "" });
                        }
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ChangerPlanCheckForOnline(Line line, Equipment eqp, Port port, Trx inputData)
        {
            string currPlanID = string.Empty;
            IList<SLOTPLAN> currPlans;
            IList<SLOTPLAN> standbyPlans;
            bool ret = false;

            try
            {
                currPlans = ObjectManager.PlanManager.GetProductPlans(out currPlanID);
                standbyPlans = ObjectManager.PlanManager.GetProductPlansStandby(out currPlanID);

                if (currPlans == null || currPlans.Count == 0)
                {
                    line.File.CurrentPlanID = string.Empty;
                    line.File.PlanStatus = ePLAN_STATUS.NO_PLAN;
                    ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                    ObjectManager.PlanManager.RemoveChangePlanStandby();
                }
                if (line.File.CurrentPlanID.Trim().Length == 0 || (line.File.PlanStatus == ePLAN_STATUS.END || line.File.PlanStatus == ePLAN_STATUS.CANCEL || line.File.PlanStatus == ePLAN_STATUS.NO_PLAN))
                {
                    line.File.PlanStatus = ePLAN_STATUS.NO_PLAN;
                    ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                    ObjectManager.PlanManager.RemoveChangePlan();
                    ObjectManager.PlanManager.RemoveChangePlanStandby();
                }

                //Recheck again
                currPlans = ObjectManager.PlanManager.GetProductPlans(out currPlanID);
                standbyPlans = ObjectManager.PlanManager.GetProductPlansStandby(out currPlanID);

                #region Loading Port
                if (port.File.Type==ePortType.LoadingPort)
                {           
                    if (currPlans == null || currPlans.Count == 0) //檢查是否有新Plan
                    {
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                            ret = ObjectManager.PlanManager.ReloadOfflinePlan(eqp.Data.LINEID, port.File.CassetteID.Trim());
                            if (ret == false)
                            {
                                this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    "[OFFLINE] Can't find " + port.File.CassetteID + " changer plan Data in Database!");
                                return;
                            }
                            currPlans = ObjectManager.PlanManager.GetProductPlans(out currPlanID);
                            if ((currPlans == null || currPlans.Count == 0))
                            {
                                this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    "[OFFLINE] Load Curr Plan Data Error!");
                                return;
                            }
                            line.File.CurrentPlanID = currPlanID;
                            line.File.PlanStatus = ePLAN_STATUS.REQUEST;
                            ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                            Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                            CassetteBondToPort(line, currPlans, true, inputData.TrackKey);
                            ObjectManager.PlanManager.SavePlanStatusInDB(currPlanID, ePLAN_STATUS.REQUEST);
                            this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            "[OFFLINE] Create " + port.File.CassetteID + " CurrPlan " + currPlanID + "!");
                        }
                        else
                        {
                            Invoke(eServiceName.MESMessageService, "ChangePlanRequest",
                                new object[] { inputData.TrackKey, line.Data.LINEID, port.File.CassetteID.Trim(), "" });
                        }
                        return;
                    }

                   if (standbyPlans == null || standbyPlans.Count == 0)　//檢查是否有新Plan
                    {
                        bool incurrPlan = false;
                        if (currPlans != null && currPlans.Count > 0) //if Source CST in current plan, not need get standby plan
                            if (null != currPlans.FirstOrDefault(p => p.SOURCE_CASSETTE_ID.Trim().Equals(port.File.CassetteID.Trim())))
                                incurrPlan = true;
                        if (!incurrPlan)
                        {
                            if (line.File.HostMode == eHostMode.OFFLINE)
                            {
                                ret = ObjectManager.PlanManager.ReloadOfflinePlanStandby(eqp.Data.LINEID, port.File.CassetteID.Trim());
                                if (ret == false)
                                {
                                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        "[OFFLINE] Can't find " + port.File.CassetteID + " changer plan Data in Database!");
                                    return;
                                }
                                standbyPlans = ObjectManager.PlanManager.GetProductPlansStandby(out currPlanID);
                                if ((standbyPlans == null || standbyPlans.Count == 0))
                                {
                                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        "[OFFLINE] Load Standby Plan Data Error!");
                                    return;
                                }
                                CassetteBondToPort(line, standbyPlans, false, inputData.TrackKey);
                                ObjectManager.PlanManager.SavePlanStatusInDB(currPlanID, ePLAN_STATUS.REQUEST);
                                this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "[OFFLINE] Create " + port.File.CassetteID + " StandPlan " + currPlanID + "!");
                            }
                            else
                            {
                                Invoke(eServiceName.MESMessageService, "ChangePlanRequest",
                                    new object[] { inputData.TrackKey, line.Data.LINEID, port.File.CassetteID.Trim(), "" });
                            }
                        }
                        return;
                    }
                    //如果当前已经有两个Plan，新上来的Source Cst 要掉退
                    if ((standbyPlans != null && standbyPlans.Count > 0) && (currPlans != null || currPlans.Count > 0)) 
                    {
                        bool incurr0rStandbyPlan = false;
                        if (currPlans != null && currPlans.Count > 0) //if Source CST in current or  Standby plan,not quit CST
                            if (null != currPlans.FirstOrDefault(p => p.SOURCE_CASSETTE_ID.Trim().Equals(port.File.CassetteID.Trim())) || null != standbyPlans.FirstOrDefault(p => p.SOURCE_CASSETTE_ID.Trim().Equals(port.File.CassetteID.Trim())))
                                incurr0rStandbyPlan = true;
                        if (!incurr0rStandbyPlan)
                        {
                            switch (port.File.CassetteStatus)
                            {
                                case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                                case eCassetteStatus.WAITING_FOR_START_COMMAND:
                                case eCassetteStatus.WAITING_FOR_PROCESSING:
                                    {
                                        Invoke(eServiceName.CassetteService, "CassetteProcessCancel",
                                        new object[] { port.Data.NODENO, port.Data.PORTNO });
                                    }
                                    break;
                                case eCassetteStatus.IN_ABORTING:
                                    {
                                        Invoke(eServiceName.CassetteService, "CassetteProcessAbort",
                                        new object[] { port.Data.NODENO, port.Data.PORTNO });
                                    }
                                    break;
                                case eCassetteStatus.IN_PROCESSING:
                                    {
                                        Invoke(eServiceName.CassetteService, "CassetteProcessEnd",
                                        new object[] { port.Data.NODENO, port.Data.PORTNO });
                                    }
                                    break;
                            }    
                        }

                    }
                }
                #endregion

                #region Unloading Port
                else if(port.File.Type==ePortType.UnloadingPort)
                {
                    if(currPlans!=null&&currPlans.Count>0)
                    {
                        if(standbyPlans!=null&&standbyPlans.Count>0)
                        {
                            List<string> lstTargetCstID = new List<string>();
                            List<string> lstCurrentTargetCstID = new List<string>();
                            List<string> lstStandbyTargetCstID = new List<string>();
                            var lsCurrentTargetCassette = from slot in currPlans group slot by slot.TARGET_CASSETTE_ID into g select g.First().TARGET_CASSETTE_ID;
                            var lsStandbyTargetCassette = from slot in standbyPlans group slot by slot.TARGET_CASSETTE_ID into g select g.First().TARGET_CASSETTE_ID;
                            foreach (string cstID in lsCurrentTargetCassette)
                            {
                                lstCurrentTargetCstID.Add(cstID);
                            }
                            foreach (string cstID in lstStandbyTargetCstID)
                            {
                                lstStandbyTargetCstID.Add(cstID);
                            }
                            lstTargetCstID.AddRange(lstCurrentTargetCstID);
                            lstTargetCstID.AddRange(lstStandbyTargetCstID);

                            if(!lstTargetCstID.Contains(port.File.CassetteID.Trim()))
                            {
                                switch (port.File.CassetteStatus)
                                {
                                    case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                                    case eCassetteStatus.WAITING_FOR_START_COMMAND:
                                    case eCassetteStatus.WAITING_FOR_PROCESSING:
                                        {
                                            Invoke(eServiceName.CassetteService, "CassetteProcessCancel",
                                            new object[] {port.Data.NODENO, port.Data.PORTNO});
                                        }
                                        break;
                                    case eCassetteStatus.IN_ABORTING:
                                        {
                                            Invoke(eServiceName.CassetteService, "CassetteProcessAbort",
                                            new object[] {port.Data.NODENO, port.Data.PORTNO});
                                        }
                                        break;
                                    case eCassetteStatus.IN_PROCESSING:
                                        {
                                            Invoke(eServiceName.CassetteService, "CassetteProcessEnd",
                                            new object[] {port.Data.NODENO, port.Data.PORTNO});
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ChangerPlanResetCheck(Line line, Equipment eqp)
        {
            List<Port> lstPort;
            bool resetPlan = true;
            try
            {
                return;
                lstPort = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID);
                foreach (Port port in lstPort)
                {
                    if (line.File.CurrentPlanID.Trim().Length == 0)
                        port.File.PlannedCassetteID = string.Empty;

                    if (port.File.Status == ePortStatus.LC)
                        resetPlan = false;
                }

                if (resetPlan)
                {
                    line.File.CurrentPlanID = string.Empty;
                    line.File.PlanStatus = ePLAN_STATUS.NO_PLAN;
                    ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                    ObjectManager.PlanManager.RemoveChangePlan();
                    ObjectManager.PlanManager.RemoveChangePlanStandby();
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //public void CassetteBondToPort(Line line, Equipment chkEqp, Port chkPort, Trx chkinputData, IList<SLOTPLAN> plans, bool quitCST, string traceid)
        public void CassetteBondToPort(Line line, IList<SLOTPLAN> plans, bool quitCST, string traceid)
        {
            List<Port> lsPorts;            
            Equipment eqp = null;
            Trx inputData = null;
            string trxName;
            bool bPlanReadyStatus = true;

            try
            {
                eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                if (eqp == null)
                {
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            "EQ Object Can't Find !");
                    return;
                }

                lsPorts = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID);
                if (lsPorts == null || lsPorts.Count == 0)
                {
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            "Port Object Can't Find !");
                    return;
                }

                foreach (Port p in lsPorts)
                {
                    //不是curr Plan, 不會去退CST
                    if (!quitCST)
                        break;
                    //跟curr Plan相同的CST ID不會退CST
                    if (p.File.Status == ePortStatus.LC && p.File.Type == ePortType.LoadingPort && plans.FirstOrDefault(slot => slot.SOURCE_CASSETTE_ID.Trim().Equals(p.File.CassetteID.Trim())) != null)
                    {
                        p.File.PlannedCassetteID = p.File.CassetteID.Trim();
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Source Cassette ID : " + p.File.CassetteID + " is exist on Port" + p.Data.PORTNO + "!");
                        continue;
                    }
                    if (p.File.Status == ePortStatus.LC && p.File.Type == ePortType.UnloadingPort && plans.FirstOrDefault(slot => slot.TARGET_CASSETTE_ID.Trim().Equals(p.File.CassetteID.Trim())) != null)
                    {
                        p.File.PlannedCassetteID = p.File.CassetteID.Trim();
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Target Cassette ID : " + p.File.CassetteID + " is exist on Port" + p.Data.PORTNO + "!");
                        continue;
                    }
                    //新curr Plan產生時，不在Plan中的CST會清除
                    p.File.PlannedCassetteID = string.Empty;
                    if (p.File.Status == ePortStatus.LC)
                    {
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(p.File.CassetteSequenceNo));
                        if (cst == null)
                            continue;
                        if (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel || cst.CassetteControlCommand == eCstControlCmd.ProcessAbort || cst.CassetteControlCommand == eCstControlCmd.ProcessEnd)
                            continue;

                        if (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING)
                        {
                            //Invoke(eServiceName.CassetteService, "CassetteProcessAbort", new object[] { eqp.Data.NODENO, p.Data.PORTNO });
                            Invoke(eServiceName.CassetteService, "CassetteProcessEnd", new object[] { eqp.Data.NODENO, p.Data.PORTNO });
                        }
                        else
                        {
                            Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { eqp.Data.NODENO, p.Data.PORTNO });
                        }
                    }
                }

                //取得source CST & target CST List for Assign to Each Port
                var lsSourceCassette = from slot in plans group slot by slot.SOURCE_CASSETTE_ID into g select g.First().SOURCE_CASSETTE_ID;
                var lsTargetCassette = from slot in plans group slot by slot.TARGET_CASSETTE_ID into g select g.First().TARGET_CASSETTE_ID;
                Port pt = null;
                //Assign source CSTID to Each port
                foreach (string cstid in lsSourceCassette)
                {
                    if (lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Equals(cstid.Trim())) != null) //已經Bonding不再Assign
                    {
                        if (ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                                            p => p.File.CassetteID.Trim().Equals(cstid.Trim()) && p.File.Status == ePortStatus.LC
                                            && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)) == null)
                        {
                            bPlanReadyStatus = false;
                        }
                        continue;
                    }
                    bPlanReadyStatus = false;
                    //Same CSTID Port check first,
                    pt = lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Length == 0 && p.File.CassetteID.Trim().Equals(cstid.Trim()));
                    if (pt != null)
                    {
                        pt.File.PlannedCassetteID = cstid;
                        pt.File.CassetteID = cstid;
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + cstid + " Bonding to Port" + pt.Data.PORTNO + " & With Same Cassette ID");
                        continue;
                    }
                    pt = lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Length == 0 && p.File.Type == ePortType.LoadingPort && p.File.Status == ePortStatus.LR);
                    if (pt != null)
                    {
                        pt.File.PlannedCassetteID = cstid;
                        pt.File.CassetteID = cstid;
                        trxName = string.Format("{0}_Port#{1}PortStatusChangeReport", eqp.Data.NODENO, pt.Data.PORTNO);
                        inputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                        inputData.TrackKey = traceid;
                        ReportMESPortStatusChange(pt, inputData); //send LDRQ with CST ID to MES
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + cstid + " Bonding to Port" + pt.Data.PORTNO + " & Send LDRQ to MES");
                        continue;
                    }
                    pt = lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Length == 0 && p.File.Type == ePortType.LoadingPort);
                    if (pt != null)
                    {
                        pt.File.PlannedCassetteID = cstid;
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + cstid + " Bonding to Port" + pt.Data.PORTNO + " & Wait Port Change to LDRQ");
                        continue;
                    }
                    pt = lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Length == 0 && p.File.Status == ePortStatus.LR);
                    if (pt != null)
                    {
                        pt.File.PlannedCassetteID = cstid;
                        trxName = string.Format("{0}_Port#{1}PortTypeChangeCommand", eqp.Data.NODENO, pt.Data.PORTNO);
                        Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                        outputdata.EventGroups[0].Events[0].Items[0].Value = "1";
                        outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                        outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                        outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                        SendPLCData(outputdata);
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + cstid + " Bonding to Port" + pt.Data.PORTNO + " & Send Port Type Change to EQ");

                        string timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, pt.Data.PORTNO, PortTypeChangeCommandTimeout);
                        if (_timerManager.IsAliveTimer(timeName))
                        {
                            _timerManager.TerminateTimer(timeName);
                        }
                        _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(PortTypeChangeCommandReplyTimeout), outputdata.TrackKey);

                        continue;
                    }
                    pt = lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Length == 0);
                    if (pt != null)
                    {
                        pt.File.PlannedCassetteID = cstid;
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + cstid + " Bonding to Port" + pt.Data.PORTNO + " & Wait Port Change to LDRQ then Send Port Type Change Command");
                        continue;
                    }

                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            "Source Cassette ID : " + cstid + " Can't Bonding to Port!");
                }

                //Assign target CSTID to Each port
                foreach (string cstid in lsTargetCassette)
                {
                    if (lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Equals(cstid.Trim())) != null) //已經Bonding不再Assign
                    {
                        if (ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                                            p => p.File.CassetteID.Trim().Equals(cstid.Trim()) && p.File.Status == ePortStatus.LC
                                            && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)) == null)
                        {
                            bPlanReadyStatus = false;
                        }
                        continue;
                    }
                    bPlanReadyStatus = false;
                    //Same CSTID Port check first,
                    pt = lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Length == 0 && p.File.CassetteID.Trim().Equals(cstid.Trim()));
                    if (pt != null)
                    {
                        pt.File.PlannedCassetteID = cstid;
                        pt.File.CassetteID = cstid;
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + cstid + " Bonding to Port" + pt.Data.PORTNO + " & With Same Cassette ID");
                        continue;
                    }
                    pt = lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Length == 0 && p.File.Type == ePortType.UnloadingPort && p.File.Status == ePortStatus.LR);
                    if (pt != null)
                    {
                        pt.File.PlannedCassetteID = cstid;
                        pt.File.CassetteID = cstid;
                        trxName = string.Format("{0}_Port#{1}PortStatusChangeReport", eqp.Data.NODENO, pt.Data.PORTNO);
                        inputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                        inputData.TrackKey = traceid;
                        ReportMESPortStatusChange(pt, inputData); //send LDRQ with CST ID to MES
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + cstid + " Bonding to Port" + pt.Data.PORTNO + " & Send LDRQ to MES");
                        continue;
                    }
                    pt = lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Length == 0 && p.File.Type == ePortType.UnloadingPort);
                    if (pt != null)
                    {
                        pt.File.PlannedCassetteID = cstid;
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + cstid + " Bonding to Port" + pt.Data.PORTNO + " & Wait Port Change to LDRQ");
                        continue;
                    }
                    pt = lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Length == 0 && p.File.Status == ePortStatus.LR);
                    if (pt != null)
                    {
                        pt.File.PlannedCassetteID = cstid;
                        trxName = string.Format("{0}_Port#{1}PortTypeChangeCommand", eqp.Data.NODENO, pt.Data.PORTNO);
                        Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                        outputdata.EventGroups[0].Events[0].Items[0].Value = "2";
                        outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                        outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                        outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                        SendPLCData(outputdata);
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + cstid + " Bonding to Port" + pt.Data.PORTNO + " & Send Port Type Change to EQ");

                        string timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, pt.Data.PORTNO, PortTypeChangeCommandTimeout);
                        if (_timerManager.IsAliveTimer(timeName))
                        {
                            _timerManager.TerminateTimer(timeName);
                        }
                        _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(PortTypeChangeCommandReplyTimeout), outputdata.TrackKey);

                        continue;
                    }
                    pt = lsPorts.FirstOrDefault(p => p.File.PlannedCassetteID.Trim().Length == 0);
                    if (pt != null)
                    {
                        pt.File.PlannedCassetteID = cstid;
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                "Cassette ID : " + cstid + " Bonding to Port" + pt.Data.PORTNO + " & Wait Port Change to LDRQ then Send Port Type Change Command");
                        continue;
                    }

                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            "Target Cassette ID : " + cstid + " Can't Bonding to Port!");
                }

                //curr Plan & All Plan CST on Port
                if (quitCST == true && bPlanReadyStatus)
                {
                    line.File.PlanStatus = ePLAN_STATUS.READY;
                    ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                    Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Cancel NG Plan Port
        /// </summary>
        /// <param name="sourcePlans"></param>
        /// <param name="targetPlans"></param>
        /// <param name="cancelType">1:cancel all cassette in the plan, 2:cancel all cassete not in plan</param>
        /// <param name="traceid"></param>
        public void CassettePlanCancel(List<string> sourceCassettes, List<string> targetCassettes, int cancelType, string traceid)
        {
            try
            {
                foreach( Port p in ObjectManager.PortManager.GetPorts())
                {
                    string cstid = p.File.CassetteID.Trim();
                    bool bPlaned = false;

                    if (sourceCassettes.Contains(cstid) || targetCassettes.Contains(cstid))
                    {
                        p.File.PlannedCassetteID = string.Empty;
                    }

                    if (p.File.Status == ePortStatus.LC)
                    {                        
                        if (sourceCassettes.Contains(cstid) || targetCassettes.Contains(cstid))
                            bPlaned = true;

                        if ((cancelType == 1 && bPlaned == true) || (cancelType == 2 && bPlaned == false))
                        {
                            if (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING)
                            {
                                //Invoke(eServiceName.CassetteService, "CassetteProcessAbort", new object[] { p.Data.NODENO, p.Data.PORTNO });
                                Invoke(eServiceName.CassetteService, "CassetteProcessEnd", new object[] { p.Data.NODENO, p.Data.PORTNO });
                            }
                            else
                            {
                                Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { p.Data.NODENO, p.Data.PORTNO });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RefreshPortStatus_CELL(Line line, Port port, Trx inputData, out string log)
        {
            log = string.Format(" PORT=[{0}]", port.Data.PORTNO);

            try
            {
                Event evt = inputData.EventGroups[0].Events[0];
                lock (port.File)
                {
                    port.File.CassetteID = evt.Items[3].Value;
                    log += string.Format(" CSTID=[{0}]", port.File.CassetteID);

                    port.File.Status = (ePortStatus)int.Parse(evt.Items[0].Value);
                    log += string.Format(" PORT_STATUS=[{0}]({1})", evt.Items[0].Value, port.File.Status.ToString());

                    port.File.CassetteStatus = (eCassetteStatus)int.Parse(evt.Items[1].Value);
                    log += string.Format(" CST_STATUS=[{0}]({1})", evt.Items[1].Value, port.File.CassetteStatus.ToString());

                    port.File.CassetteSequenceNo = evt.Items[2].Value;
                    log += string.Format(" CST_SEQNO=[{0}]", port.File.CassetteSequenceNo);

                    port.File.JobCountInCassette = evt.Items[4].Value;
                    log += string.Format(" JOB_COUNT_IN_CST=[{0}]", port.File.JobCountInCassette);

                    port.File.CompletedCassetteData = (eCompletedCassetteData)int.Parse(evt.Items[5].Value);
                    log += string.Format(" COMPLETED_CST_DATA=[{0}]({1})", evt.Items[5].Value, port.File.CompletedCassetteData.ToString());

                    port.File.OperationID = evt.Items[6].Value;
                    log += string.Format(" OPERATIONID=[{0}]", port.File.OperationID);

                    port.File.JobExistenceSlot = evt.Items[7].Value.Substring(0, port.Data.MAXCOUNT);
                    log += string.Format(" JOB_EXISTENCE_SLOT=[{0}]", port.File.JobExistenceSlot);

                    port.File.LoadingCassetteType = (eLoadingCstType)int.Parse(evt.Items[8].Value);
                    log += string.Format(" LOADING_CST_TYPE=[{0}]({1})", evt.Items[8].Value, port.File.LoadingCassetteType.ToString());

                    port.File.QTimeFlag = (eQTime)int.Parse(evt.Items[9].Value);
                    log += string.Format(" Q_TIMEFLAG=[{0}]({1})", evt.Items[9].Value, port.File.QTimeFlag.ToString());

                    port.File.PartialFullFlag = (eParitalFull)int.Parse(evt.Items[10].Value);
                    log += string.Format(" PARTIAL_FULL_FLAG=[{0}]({1})", evt.Items[10].Value, port.File.PartialFullFlag.ToString());

                    port.File.CassetteSetCode = evt.Items[11].Value; 
                    log += string.Format(" CST_SETCODE=[{0}]", port.File.CassetteSetCode.ToString());

                    if (evt.Items["MaxSlotCount"] != null)
                    {
                        port.File.MaxSlotCount = evt.Items["MaxSlotCount"].Value;
                        log += string.Format(" MAX_SLOT_COUNT=[{0}]", port.File.MaxSlotCount);
                    }
                    #region [CCCLN USE]
                    if (evt.Items["DistortionFlag"] != null)
                    {
                        port.File.DistortionFlag = (eDistortion)int.Parse(evt.Items["DistortionFlag"].Value);
                        log += string.Format(" DistortionFlag=[{0}]", port.File.DistortionFlag.ToString());
                    }
                    if (evt.Items["DirectionFlag"] != null)
                    {
                        port.File.DirectionFlag = (eDirection)int.Parse(evt.Items["DirectionFlag"].Value);
                        log += string.Format("DirectionFlag=[{0}]", port.File.DirectionFlag.ToString());
                    }
                    if (evt.Items["GlassExistFlag"] != null)
                    {
                        port.File.GlassExist = (eGlassExist)int.Parse(evt.Items["GlassExistFlag"].Value);
                        log += string.Format("GlassExistFlag=[{0}]", port.File.DirectionFlag.ToString());
                    }
                    #endregion
                    #region T2 USE
                    //port.File.CompletedCassetteReason = (eCompleteCassetteReason)int.Parse(evt.Items[12].Value);
                    //log += string.Format(" COMPLETE_CST_REASON=[{0}]({1})", evt.Items[12].Value, port.File.CompletedCassetteReason.ToString());

                    //if (line.Data.LINETYPE == eLineType.CELL.CBPOL_1 || line.Data.LINETYPE == eLineType.CELL.CBPOL_2 || line.Data.LINETYPE == eLineType.CELL.CBPOL_3)  //Jun Modify 20141225 使來Line Type來判斷 
                    //{
                    //    port.File.AutoClaveByPass = (eBitResult)int.Parse(evt.Items[13].Value);
                    //    log += string.Format(" AUTO_CLAVE_BYPASS=[{0}]({1})", evt.Items[13].Value, port.File.AutoClaveByPass.ToString());

                    //    port.File.MappingGrade = evt.Items[14].Value;
                    //    log += string.Format(" MAPPING_GRADE=[{0}]", port.File.MappingGrade);
                    //}
                    ////Watson Add 20150319 使用Line Type來判斷 
                    //if (line.Data.LINETYPE == eLineType.CELL.CBSOR_1 || line.Data.LINETYPE == eLineType.CELL.CBSOR_2 || line.Data.LINETYPE == eLineType.CELL.CBPRM || line.Data.LINETYPE == eLineType.CELL.CBPMT) 
                    //{
                    //    port.File.MaxSlotCount = evt.Items[13].Value;
                    //    log += string.Format(" MAX_SLOT_COUNT=[{0}]", port.File.MaxSlotCount);
                    //}
                    #endregion

                }
                ObjectManager.PortManager.EnqueueSave(port.File);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ReportMESPortStatusChange(Port port, Trx inputData)
        {
            try
            {
                // MES Data
                object[] _data = new object[3]
                {
                    inputData.TrackKey,     /*0 TrackKey*/
                    port.Data.LINEID,       /*1 LineName*/
                    port                  /*2 Port*/
                };
                Invoke(eServiceName.MESService, "PortTransferStateChanged", _data);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void AssignNewGrade(Trx inputData, Equipment eqp, Port port)
        {
            try
            {
                IList<Port> souPorts = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).Where(p => p.File.Type == ePortType.LoadingPort).ToList();
                List<Sorter> grade = new List<Sorter>();
                string toMESGrade = string.Empty;

                foreach (Port p in souPorts)
                {
                    if (!p.File.SorterJobGrade.Equals(null))
                    {
                        for (int i = 0; i < p.File.SorterJobGrade.Count; i++)
                        {
                            grade.Add(p.File.SorterJobGrade[i]);
                        }
                    }
                }

                if (grade.Equals(null))
                {
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] THERE IS NO SOURCE LIST IN SOURCE PORT."
                                                        , inputData.Metadata.NodeNo, inputData.TrackKey));
                    return;
                }
                else
                    grade = grade.OrderByDescending(x => x.PriorityGrade).ToList();

                for (int i = 0; i < grade.Count; i++)
                {
                    if (!grade[i].ToMES)
                    {
                        if (port.File.MappingGrade.Equals(string.Empty))
                        {
                            lock (port)
                            {
                                port.File.MappingGrade = grade[i].SorterGrade;
                                port.File.ProductType = grade[i].ProductType.ToString();
                            }
                            Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });
                            RecordPortHistory(inputData.TrackKey, eqp, port);
                            ReportMESPortStatusChange(port, inputData);
                            grade[i].ToMES = true;
                            toMESGrade = grade[i].SorterGrade;
                            Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                          string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT_NO=[{2}], PORT_GRADE=[{3}], PORT_PRODUCTTYPE=[{4}].",
                                                          inputData.Metadata.NodeNo, inputData.TrackKey, port.Data.PORTNO, port.File.MappingGrade, port.File.ProductType));
                        }
                        else
                        {
                            if (eqp.File.ProductTypeCheckMode == eEnableDisable.Disable)
                            {
                                if (grade[i].SorterGrade == toMESGrade)
                                    grade[i].ToMES = true;
                            }
                        }                           
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion ===================
        // ===================
        #region [Port Type Change]
        #region **[Port Type Change Report]

        /// <summary>
        /// 更新PortType
        /// </summary>
        /// <param name="inputData">PLC Trx物件</param>
        /// <param name="log">觸發者名稱(記Log用)</param>
        public void PortTypeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortType oldtype = port.File.Type;

                lock (port.File)
                {
                    port.File.Type = (ePortType)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                }

                if (port.File.Type != oldtype)
                {
                    ReportMESPortTypeChange(port, inputData);
                }

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] PORT=[{3}] PORT_TYPE=[{4}].", 
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, port.Data.PORTNO, port.File.Type));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortTypeChangeReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                    
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortType oldtype = port.File.Type;
                if (inputData.IsInitTrigger)
                {
                    PortTypeChangeReportUpdate(inputData, "PortTypeChangeReport_Initial");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF] PORT=[{2}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, port.Data.PORTNO));
                    PortTypeChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                lock (port.File)
                {
                    port.File.Type = (ePortType)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                }

                if (port.File.Type != oldtype)
                {
                    ReportMESPortTypeChange(port, inputData);
                }

                if (port.File.Type == ePortType.BothPort && port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP)
                {
                    lock (port) port.File.OPI_SubCstState = eOPISubCstState.WAREMAPEDIT;
                }

                if (ObjectManager.LineManager.GetLine(eqp.Data.LINEID).File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                {
                    if (port.File.Status == ePortStatus.LR)
                    {
                        port.File.CassetteID = port.File.PlannedCassetteID;
                        ReportMESPortStatusChange(port, inputData);
                    }
                }

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] PORT=[{3}] PORT_TYPE=[{4}]({5}).",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, portNo, (int)port.File.Type, port.File.Type.ToString()));

                PortTypeChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

                RecordPortHistory(inputData.TrackKey, eqp, port);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                    string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                    PortTypeChangeReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void PortTypeChangeReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_Port#{1}PortTypeChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, PortTypeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PortTypeChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}], SET BIT=[{3}].", eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PortTypeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] PORT TYPE CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));

                PortTypeChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ReportMESPortTypeChange(Port port, Trx inputData)
        {
            try
            {
                // MES Data
                IList<Port> lstPort = new List<Port>();
                lstPort.Add(port);
                object[] _data = new object[3]
                {
                    inputData.TrackKey,        /*0 TrackKey*/
                    port.Data.LINEID,          /*1 LineName*/
                    lstPort                     /*2 Port*/
                };
                Invoke(eServiceName.MESService, "PortTypeChanged", _data);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region **[Port Type Change Command]
        public void PortTypeChangeCommand(string lineID,PortCommandRequest msg)
        {
            try
            {
                string err = string.Empty;
                
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortTypeChangeCommand] " + err });
                    throw new Exception(err);
                }

                //Watson Modify Servername != LineName
                port = ObjectManager.PortManager.GetPort(lineID, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);
                //port = ObjectManager.PortManager.GetPort(msg.BODY.LINENAME, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN LINE[{1}] AND EQUIPMENT=[{2}]!", msg.BODY.PORTNO,msg.BODY.LINENAME , msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortTypeChangeCommand] " + err });

                    throw new Exception(err);
                }
                #endregion

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CIM_MODE=[OFF], CAN'T CHANGE PORT TYPE!", eqp.Data.NODENO, port.Data.PORTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                // Port上有卡匣不能改
                if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST && 
                    ParameterManager[eREPORT_SWITCH.PORT_FUNCTION_CHECK_NO_CST].GetBoolean())
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), THERE IS A CASSETTE ON THE PORT, CAN NOT CHANGE PORT TYPE!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus,port.File.CassetteStatus.ToString());

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                string trxName = string.Format("{0}_Port#{1}PortTypeChangeCommand", eqp.Data.NODENO, port.Data.PORTNO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = msg.BODY.PORTCOMMAND;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On 
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={1}] [BCS -> EQP][{0}] PORT_TYPE=[{1}], SET BIT=[ON].",
                        outputdata.TrackKey, outputdata.Metadata.NodeNo, msg.BODY.PORTCOMMAND));

                string timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, port.Data.PORTNO, PortTypeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(PortTypeChangeCommandReplyTimeout), outputdata.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortTypeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] PORT=[{3}] RETURNCODE=[{4}]({5}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), portNo, (int)retCode, retCode.ToString()));

                if (triggerBit == eBitResult.OFF) return;

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, portNo, PortTypeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                string trxName = string.Format("{0}_Port#{1}PortTypeChangeCommand", inputData.Metadata.NodeNo, portNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "PortTypeChangeCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}], SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, portNo));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                        string.Format("EQUIPMENT=[{0}] PORT=[{1}] EQP REPLY PORT TYPE CHANGE \"{2}\"!", eqp.Data.NODENO, portNo, retCode)});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PortTypeChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], PortTypeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_Port#{1}PortTypeChangeCommand", sArray[0], sArray[1]);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] PORT TYPE CHANGE COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion
        // ===================
        #region [Port Mode Change]
        #region **[Port Mode Change Report]
        /// <summary>
        /// 更新PortMode
        /// </summary>
        /// <param name="inputData">PLC Trx物件</param>
        /// <param name="log">觸發者名稱(記Log用)</param>
        public void PortModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortMode oldmode = port.File.Mode;

                lock (port.File)
                    port.File.Mode = (ePortMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                ObjectManager.PortManager.EnqueueSave(port.File);

                if (port.File.Mode != oldmode)
                {
                    ReportMESPortModeChange(port, inputData);
                }

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] PORT=[{3}] PORT_MODE=[{4}].",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, port.Data.PORTNO, port.File.Mode));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void PortModeChangeReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortMode oldmode = port.File.Mode;
                if (inputData.IsInitTrigger)
                {
                    PortModeChangeReportUpdate(inputData, "PortModeChangeReport_Initial");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF] PORT=[{2}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, port.Data.PORTNO));
                    PortModeChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                lock (port.File) port.File.Mode = (ePortMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                ObjectManager.PortManager.EnqueueSave(port.File);
                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] PORT=[{3}] PORT_MODE=[{4}]({5}).",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, portNo, (int)port.File.Mode, port.File.Mode.ToString()));

                if (port.File.Mode != oldmode) ReportMESPortModeChange(port, inputData);

                PortModeChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

                RecordPortHistory(inputData.TrackKey, eqp, port);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                    string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                    PortModeChangeReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void PortModeChangeReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_Port#{1}PortModeChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, PortModeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(PortModeChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}], SET BIT=[{3}].", eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PortModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] PORT MODE CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));

                PortModeChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ReportMESPortModeChange(Port port, Trx inputData)
        {
            try
            {
                // MES Data
                IList<Port> lstPort = new List<Port>();
                lstPort.Add(port);
                object[] _data = new object[3]
                {
                    inputData.TrackKey,     /*0 TrackKey*/
                    port.Data.LINEID,       /*1 LineName*/
                    lstPort                  /*2 Port*/
                };
                Invoke(eServiceName.MESService, "PortUseTypeChanged", _data);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion
        #region **[Port Mode Change Command]
        public void PortModeChangeCommand(string trxID, string lineID,PortCommandRequest msg)
        {
            try
            {
                string err = string.Empty;

                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { trxID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortModeChangeCommand] " + err });
                    throw new Exception(err);
                }

                //Watson Modify Servername != LineName
                port = ObjectManager.PortManager.GetPort(lineID, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);
                //port = ObjectManager.PortManager.GetPort(msg.BODY.LINENAME, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);
                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { trxID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortModeChangeCommand] " + err });
                    throw new Exception(err);
                }
                #endregion

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CIM_MODE=[OFF], CAN'T CHANGE PORT MODE!", eqp.Data.NODENO, port.Data.PORTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                // Port上有卡匣不能改
                if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST &&
                    ParameterManager[eREPORT_SWITCH.PORT_FUNCTION_CHECK_NO_CST].GetBoolean())
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}], THERE IS A CASSETTE ON THE PORT, CAN NOT CHANGE PORT MODE!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, port.File.Mode);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                string trxName = string.Format("{0}_Port#{1}PortModeChangeCommand", eqp.Data.NODENO, port.Data.PORTNO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = msg.BODY.PORTCOMMAND;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={1}] [BCS -> EQP][{0}] PORT_MODE=[{1}], SET BIT=[ON].",
                    outputdata.TrackKey, outputdata.Metadata.NodeNo, msg.BODY.PORTCOMMAND));

                string timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, port.Data.PORTNO, PortModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(PortModeChangeCommandReplyTimeout), outputdata.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortModeChangeCommandFromBCS(string trackKey, string lineID, string eqpNo, string portNo, string portCommand)
        {
            try
            {
                string err = string.Empty;

                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { trackKey, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortTypeChangeCommand] " + err });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(lineID, eqpNo, portNo);
                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN LINE[{1}] AND EQUIPMENT=[{2}]!", portNo, lineID, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { trackKey, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortTypeChangeCommand] " + err });

                    throw new Exception(err);
                }
                #endregion

                #region CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CIM_MODE=[OFF], CAN'T CHANGE PORT TYPE!", eqp.Data.NODENO, port.Data.PORTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { trackKey, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                // Port上有卡匣不能改
                if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST &&
                    ParameterManager[eREPORT_SWITCH.PORT_FUNCTION_CHECK_NO_CST].GetBoolean())
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), THERE IS A CASSETTE ON THE PORT, CAN NOT CHANGE PORT TYPE!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { trackKey, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                string trxName = string.Format("{0}_Port#{1}PortModeChangeCommand", eqp.Data.NODENO, port.Data.PORTNO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = portCommand;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On 
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={1}] [BCS -> EQP][{0}] PORT_MODE=[{1}], SET BIT=[ON].",
                        outputdata.TrackKey, outputdata.Metadata.NodeNo, portCommand));

                string timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, port.Data.PORTNO, PortModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(PortModeChangeCommandReplyTimeout), outputdata.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortModeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] PORT=[{3}] RETURNCODE=[{4}]({5}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), portNo, (int)retCode, retCode.ToString()));

                if (triggerBit == eBitResult.OFF) return;

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, portNo, PortModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                string trxName = string.Format("{0}_Port#{1}PortModeChangeCommand", inputData.Metadata.NodeNo, portNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "PortModeChangeCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}], SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, portNo));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                        string.Format("EQUIPMENT=[{0}] PORT=[{1}] EQP REPLY PORT MODE CHANGE \"{2}\"!", eqp.Data.NODENO, portNo, retCode)});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PortModeChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], PortModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_Port#{1}PortModeChangeCommand", sArray[0], sArray[1]);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] PORT MODE CHANGE COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion
        // ===================
        #region [Port Transfer Mode Change]
        #region **[Port Transfer Mode Change Report]

        /// <summary>
        /// 更新PortTransferMode
        /// </summary>
        /// <param name="inputData">PLC Trx物件</param>
        /// <param name="log">觸發者名稱(記Log用)</param>
        public void PortTransferModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortTransferMode oldmode = port.File.TransferMode;

                lock (port.File) 
                    port.File.TransferMode = (ePortTransferMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                ObjectManager.PortManager.EnqueueSave(port.File);

                if (port.File.TransferMode != oldmode)
                {
                    ReportMESPortTransferChange(port, inputData);
                }

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] PORT=[{3}] TRANSFER_MODE=[{4}].",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, port.Data.PORTNO, port.File.TransferMode));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortTransferModeChangeReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortTransferMode oldmode = port.File.TransferMode;
                if (inputData.IsInitTrigger)
                {
                    PortTransferModeChangeReportUpdate(inputData, "PortTransferModeChangeReport");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF] PORT=[{2}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, port.Data.PORTNO));
                    PortTransferChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                
                lock (port.File) port.File.TransferMode = (ePortTransferMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                ObjectManager.PortManager.EnqueueSave(port.File);
                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                if (port.File.TransferMode != oldmode) ReportMESPortTransferChange(port, inputData);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] PORT=[{3}] TRANSFER_MODE=[{4}]({5}).",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, portNo, (int)port.File.TransferMode, port.File.TransferMode.ToString()));

                PortTransferChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

                RecordPortHistory(inputData.TrackKey, eqp, port);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                    string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                    PortTransferChangeReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void PortTransferChangeReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_Port#{1}PortTransferModeChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, PortTransferModeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(PortTransferChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}], SET BIT=[{3}].", eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PortTransferChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] PORT TRANSFER CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));

                PortTransferChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ReportMESPortTransferChange(Port port, Trx inputData)
        {
            try
            {
                // MES Data
                IList<Port> lstPort = new List<Port>();
                lstPort.Add(port);
                object[] _data = new object[3]
                {
                    inputData.TrackKey,     /*0 TrackKey*/
                    port.Data.LINEID,       /*1 LineName*/
                    lstPort                  /*2 Port*/
                };
                Invoke(eServiceName.MESService, "PortAccessModeChanged", _data);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region **[Port Transfer Mode Change Command]
        public void PortTransferModeChangeCommand(string lineID,PortCommandRequest msg)
        {
            try
            {
                string err = string.Empty;
                
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortTransferModeChangeCommand] " + err });
                    throw new Exception(err);
                }

                //Watson Modify Servername != LineName
                port = ObjectManager.PortManager.GetPort(lineID, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);
                //port = ObjectManager.PortManager.GetPort(msg.BODY.LINENAME, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortTransferModeChangeCommand] " + err });

                    throw new Exception(err);
                }
                #endregion

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CIM_MODE=[OFF], CAN'T CHANGE PORT TRANSFER MODE!", eqp.Data.NODENO, port.Data.PORTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //mark by rebecca -- 2015-12-24 玉明提出不判斷cassette status
                //// Port上有卡匣不能改
                //if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST &&
                //    ParameterManager[eREPORT_SWITCH.PORT_FUNCTION_CHECK_NO_CST].GetBoolean())
                //{
                //    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), THERE IS A CASSETTE ON THE PORT, CAN NOT CHANGE PORT TRANSFER MODE!",
                //        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus,port.File.CassetteStatus.ToString());

                //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                //        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                //    return;
                //}

                string trxName = string.Format("{0}_Port#{1}PortTransferModeChangeCommand", eqp.Data.NODENO, port.Data.PORTNO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = msg.BODY.PORTCOMMAND;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={1}] [BCS -> EQP][{0}] TRANSFER_MODE=[{1}], SET BIT=[ON].",
                    outputdata.TrackKey, outputdata.Metadata.NodeNo, msg.BODY.PORTCOMMAND));


                string timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, port.Data.PORTNO, PortTransferModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(PortTransferModeChangeCommandReplyTimeout), outputdata.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortTransferModeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] PORT=[{3}] RETURNCODE=[{4}]({5}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), portNo, (int)retCode, retCode.ToString()));

                if (triggerBit == eBitResult.OFF) return;

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, portNo, PortTransferModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                string trxName = string.Format("{0}_Port#{1}PortTransferModeChangeCommand", inputData.Metadata.NodeNo, portNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "PortTransferModeChangeCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}], SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, portNo));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                        string.Format("EQUIPMENT=[{0}] PORT=[{1}] EQP REPLY PORT TRANSFER MODE CHANGE \"{2}\"!", eqp.Data.NODENO, portNo, retCode)});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PortTransferModeChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], PortTransferModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_Port#{1}PortTransferModeChangeCommand", sArray[0], sArray[1]);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] PORT TRANSFER MODE CHANGE COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion
        // ===================
        #region [Port Enable Mode Change]
        #region **[Port Enable Mode Change Report]

        /// <summary>
        /// 更新PortEnableMode
        /// </summary>
        /// <param name="inputData">PLC Trx物件</param>
        /// <param name="log">觸發者名稱(記Log用)</param>
        public void PortEnableModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port; Line line;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}][{2}]!", portNo, eqp.Data.NODENO,eqp.Data.NODEID));
                #endregion

                ePortEnableMode oldmode = port.File.EnableMode;

                lock (port.File) port.File.EnableMode = (ePortEnableMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                ObjectManager.PortManager.EnqueueSave(port.File);

                if (port.File.EnableMode != oldmode)
                {
                    ReportMESPortEnableChange(port, inputData);
                }

                //Jun Modify 20150416 PPK雖然IO是開Port，但實際上要用DenseStatusReport給OPI，不然OPI會出現錯誤。
                //20160105 cy:T3的PPK 跟 QPP 有Dense
                if ((line.Data.LINETYPE == eLineType.CELL.CCPPK || line.Data.LINETYPE == eLineType.CELL.CCQPP) & 
                      (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX_MANUAL))
                    Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                else
                    Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });
                                
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] PORT=[{3}] PORT_ENABLE_MODE=[{4}].",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, port.Data.PORTNO, port.File.EnableMode));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortEnableModeChangeReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}][{2}]!", portNo, eqp.Data.NODENO,eqp.Data.NODEID));
                #endregion

                ePortEnableMode oldmode = port.File.EnableMode;
                if (inputData.IsInitTrigger)
                {
                    PortEnableModeChangeReportUpdate(inputData, "PortEnableModeChangeReport");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF] PORT=[{2}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, port.Data.PORTNO));
                    PortEnableChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                lock (port.File) port.File.EnableMode = (ePortEnableMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                ObjectManager.PortManager.EnqueueSave(port.File);
                
                //Jun Modify 20120228 
                //20160105 cy:T3的PPK 跟 QPP 有Dense
                if ((port.Data.LINEID.Contains(keyCellLineType.PPK) || port.Data.LINEID.Contains(keyCellLineType.QPP)) &
                      (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX_MANUAL))
                    Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                else
                    Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] PORT=[{3}] PORT_ENABLE_MODE=[{4}]({5}).",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, portNo, (int)port.File.EnableMode, port.File.EnableMode.ToString()));

                if (port.File.EnableMode != oldmode) ReportMESPortEnableChange(port, inputData);

                PortEnableChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

                RecordPortHistory(inputData.TrackKey, eqp, port);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                    string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                    PortEnableChangeReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void PortEnableChangeReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_Port#{1}PortEnableModeChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, PortEnableModeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(PortEnableChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}], SET BIT=[{3}].", eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PortEnableChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] PORT ENABLE MODE CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));

                PortEnableChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ReportMESPortEnableChange(Port port, Trx inputData)
        {
            try
            {
                // MES Data
                IList<Port> lstPort = new List<Port>();
                lstPort.Add(port);
                object[] _data = new object[3]
                {
                    inputData.TrackKey,     /*0 TrackKey*/
                    port.Data.LINEID,       /*1 LineName*/
                    lstPort                  /*2 Port*/
                };
                Invoke(eServiceName.MESService, "PortEnableChanged", _data);

                //Port Enable Mode Change Report PortTransferStateChanged 20141115 Tom
                if (port.File.EnableMode == ePortEnableMode.Enabled)
                {
                    Invoke(eServiceName.MESService, "PortTransferStateChanged", new object[]{inputData.TrackKey,port.Data.LINEID,port});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region **[Port Enable Mode Change Command]
        public void PortEnableModeChangeCommand(string lineID,PortCommandRequest msg)
        {
            try
            {
                string err = string.Empty;

                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortEnableModeChangeCommand] " + err });
                    throw new Exception(err);
                }

                //Watson Modify Servername != LineName
                port = ObjectManager.PortManager.GetPort(lineID, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);
                //port = ObjectManager.PortManager.GetPort(msg.BODY.LINENAME, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                            new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", "[PortEnableModeChangeCommand] " + err });
                    throw new Exception(err);
                }
                #endregion

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CIM_MODE=[OFF], CAN'T CHANGE PORT ENABLE MODE!", eqp.Data.NODENO, port.Data.PORTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                // Port上有卡匣不能改
                if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST &&
                    ParameterManager[eREPORT_SWITCH.PORT_FUNCTION_CHECK_NO_CST].GetBoolean())
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}], THERE IS A CASSETTE ON THE PORT, CAN NOT CHANGE PORT ENABLE MODE!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, port.File.Mode);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                string trxName = string.Format("{0}_Port#{1}PortEnableModeChangeCommand", eqp.Data.NODENO, port.Data.PORTNO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = msg.BODY.PORTCOMMAND;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={1}] [BCS -> EQP][{0}] PORT_ENABLE_MODE=[{1}], SET BIT=[ON].",
                    outputdata.TrackKey, outputdata.Metadata.NodeNo, msg.BODY.PORTCOMMAND));


                string timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, port.Data.PORTNO, PortEnableModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(PortEnableModeChangeCommandReplyTimeout), outputdata.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortEnableModeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] PORT=[{3}] RETURNCODE=[{4}]({5}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), portNo, (int)retCode, retCode.ToString()));

                if (triggerBit == eBitResult.OFF) return;

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, portNo, PortEnableModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                string trxName = string.Format("{0}_Port#{1}PortEnableModeChangeCommand", inputData.Metadata.NodeNo, portNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, "PortEnableModeChangeCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}], SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, portNo));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp != null)
                {
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID,
                        string.Format("EQUIPMENT=[{0}] PORT=[{1}] EQP REPLY PORT ENABLE MODE CHANGE \"{2}\"!", eqp.Data.NODENO, portNo, retCode)});
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PortEnableModeChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], PortEnableModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_Port#{1}PortEnableModeChangeCommand", sArray[0], sArray[1]);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Port=[{2}) Port Enable Mode Change Command Reply Timeout Set Bit (OFF).",
                    sArray[0], trackKey, sArray[1]));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion
        // ===================
        #region [Port Down]
        /// <summary>
        /// 更新PortDown
        /// </summary>
        /// <param name="inputData">PLC Trx物件</param>
        /// <param name="log">觸發者名稱(記Log用)</param>
        public void PortDownReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                #region [取得Line、EQP、Port資訊]
                Line line; Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                // T3 的 port.File.DownStatus 全部由 PortDownRealStatusBlock 更新
                //// AC廠的 Port Down 改由 PortDownRealStatusBlock 更新
                //if (line.Data.FABTYPE == eFabType.CELL.ToString())
                //{
                //    lock (port.File) port.File.DownStatus = (ePortDown)int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value);
                //    ObjectManager.PortManager.EnqueueSave(port.File);
                //    Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });
                //}

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] PORT=[{3}] PORT_DOWN=[{4}]({5}).",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, portNo, (int)port.File.DownStatus, port.File.DownStatus.ToString()));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortDownReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [取得Line、EQP、Port資訊]
                Line line; Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", ServerName));

                string portNo = inputData.EventGroups[0].Events[0].Items[0].Value.PadLeft(2, '0');
               
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT_NO=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF] PORT=[{2}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, port.Data.PORTNO));
                    PortDownReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                // AC廠的 Port Down 改由 PortDownRealStatusBlock 更新//T3全廠使用PortDownRealStatusBlock
                //if (line.Data.FABTYPE == eFabType.CELL.ToString())
                //{
                //    lock (port.File) port.File.DownStatus = (ePortDown)int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value);
                //    ObjectManager.PortManager.EnqueueSave(port.File);
                //    Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });
                //}

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] PORT=[{3}] PORT_DOWN=[{4}]({5}).",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, portNo, (int)port.File.DownStatus, port.File.DownStatus.ToString()));

                PortDownReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                    string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                    PortDownReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void PortDownReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_PortDownReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, PortDownReportTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(PortDownReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}], SET BIT=[{3}].", eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PortDownReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                PortDownReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] PORT DOWN REPORT REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        // ===================
        #region [Port Down Real Status]
        /// <summary>
        /// 更新Port Down Real Status
        /// </summary>
        /// <param name="inputData">PLC Trx物件</param>
        /// <param name="log">觸發者名稱(記Log用)</param>
        public void PortDownRealStatusBlock(Trx inputData)
        {
            try
            {
                #region [取得EQP]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                //取出 Port Real Status
                string portDownList = inputData.EventGroups[0].Events[0].Items[0].Value;

                List<Port> portList = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID);
                foreach (Port port in portList)
                {
                    int portPosition = int.Parse(port.Data.PORTNO) - 1;
                    if (portDownList.Substring(portPosition, 1) == "0")
                        lock (port.File) port.File.DownStatus = ePortDown.Down;
                    else
                        lock (port.File) port.File.DownStatus = ePortDown.Normal;

                    ObjectManager.PortManager.EnqueueSave(port.File);

                    Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] PORT_DOWNREALSTATUS=[{4}]({5}).",
                        eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, port.Data.PORTNO, (int)port.File.DownStatus, port.File.DownStatus.ToString()));
                }

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        // ===================
        #region **[Port Oper Mode Change Report] for CELL 
        public void PortOperModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortOperMode oldmode = port.File.OperMode;

                // 1. 更新Port mode 資料
                lock (port.File)
                {
                    port.File.OperMode = (ePortOperMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                    port.File.PortPackMode = (ePalletMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                }
                ObjectManager.PortManager.EnqueueSave(port.File);
                // 2. 重啟BCS時, 如果新的資料跟之前的不同, 補報給MES
                if (port.File.OperMode != oldmode)
                {
                    ReportMESPortOperModeChange(port, inputData);
                }
                //  等新的OPI TRANSACTION 來通知
                //Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] PORT=[{3}] PORT_OPERATION_MODE=[{4}].",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, port.Data.PORTNO, port.File.OperMode));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void PortOperModeChangeReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortOperMode oldmode = port.File.OperMode;
                if (inputData.IsInitTrigger)
                {
                    PortOperModeChangeReportUpdate(inputData, MethodBase.GetCurrentMethod().Name + "_Initial");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF] PORT=[{2}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, port.Data.PORTNO));
                    PortOperModeChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                lock (port.File)
                {
                    port.File.OperMode = (ePortOperMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                    port.File.PortPackMode = (ePalletMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                }
                ObjectManager.PortManager.EnqueueSave(port.File);
                Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] PORT=[{3}] PORT_OPERATION_MODE=[{4}]({5}).",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, portNo, (int)port.File.OperMode, port.File.OperMode.ToString()));

                //只上報，不管MES Reply
                if (port.File.OperMode != oldmode) ReportMESPortOperModeChange(port, inputData);
                //不管MES Reply, 回覆給機台
                PortOperModeChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

                RecordPortHistory(inputData.TrackKey, eqp, port);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                    string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                    PortOperModeChangeReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void PortOperModeChangeReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_Port#{1}PortPackingModeChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, PortOperModeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(PortOperModeChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}], SET BIT=[{3}].", eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void PortOperModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                PortOperModeChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] PORT OPERATION MODE CHANGE REPORT REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ReportMESPortOperModeChange(Port port, Trx inputData)
        {
            try
            {
                // MES Data
                IList<Port> lstPort = new List<Port>();
                lstPort.Add(port);
                object[] _data = new object[3]
                {
                    inputData.TrackKey,     /*0 TrackKey*/
                    port.Data.LINEID,       /*1 LineName*/
                    lstPort                  /*2 Port*/
                };
                Invoke(eServiceName.MESService, "PortOperModeChanged", _data);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        // ===================
        #region File Info Notify Report For EQP
        //Modify by Frank For T3 No Used 20150720
        //public void FileInfoNotifyReportForEQP(Trx inputData)
        //{
        //    try
        //    {
        //        string eqpNo = inputData.Metadata.NodeNo;
        //        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
        //        string cstSeq = inputData.EventGroups[0].Events[0].Items[0].Value;
        //        if (inputData.IsInitTrigger) return;
        //
        //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("[EQUIPMENT={0}][IDX -> EQP][{1}] BIT=[{2}] CASSETTE_SEQUENCE_NO=[{3}].",
        //                inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), cstSeq));
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        //public void FileInfoNotifyReportReplyByEQP(Trx inputData)
        //{
        //    try
        //    {
        //        string eqpNo = inputData.Metadata.NodeNo;
        //        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
        //        if (inputData.IsInitTrigger) return;
        //
        //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("[EQUIPMENT={0}][IDX <- EQP][{1}] REPLY BIT=[{2}].", 
        //                inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString()));
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        #endregion

        #region CELL Special Cassette Setting Code Report
        private const string CstSetCodeReplyTimeout = "CstSetCodeReplyTimeout";
        public void CassetteSettingCodeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                int intstart = inputData.Metadata.Name.IndexOf('#') + 1;
                if (intstart.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));
                string portno = inputData.Metadata.Name.Substring(intstart, 2);

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Bit (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    CassetteSettingCodeChangeReportReply(eqp.Data.NODENO, portno, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string setCode = inputData.EventGroups[0].Events[0].Items[11].Value;
                #endregion


                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode =[{2}], Node =[{3}],CassetteSettingCodeChangeReport Setting Code =[{4}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, setCode));


                #region Update Port

                Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portno);
                if (port == null) throw new Exception(string.Format("Can't find Trx Name Port No =[{0}) in PortEntity!", inputData.Metadata.Name));
                port.File.CassetteSetCode = setCode;
                lock (port.File)
                {
                    ObjectManager.PortManager.EnqueueSave(port.File);
                }
                #endregion

                #region MES Report
                List<Port> portlist = new List<Port>();
                portlist.Add(port);
                object[] _data = new object[3]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    portlist
                };
                Invoke(eServiceName.MESService, "PortCarrierSetCodeChanged", _data);
                //PortCarrierSetCodeChanged(string trxID, string lineName,IList<Port> portList)
                #endregion

                CassetteSettingCodeChangeReportReply(eqp.Data.NODENO, portno, eBitResult.ON, inputData.TrackKey);

                RecordPortHistory(inputData.TrackKey, eqp, port);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void CassetteSettingCodeChangeReportReply(string eqpNo, string portno, eBitResult value, string trackKey)
        {
            try
            {
                //一種：
                //_Port#03CassetteSettingCodeChangeReport
                //_Port#03CassetteSettingCodeChangeReportReply
                //二種：
                //_DP#01CassetteSettingCodeChangeReport
                //_DP#01CassetteSettingCodeChangeReportReply
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "Port#" + portno + "CassetteSettingCodeChangeReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + portno + "_" + CstSetCodeReplyTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + portno + "_" + CstSetCodeReplyTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + portno + "_" + CstSetCodeReplyTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(CassetteSettingCodeChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,PortNo[{2}] CassetteSettingCodeChangeReportReply Set Bit [{3}].",
                    eqpNo, trackKey, portno, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void CassetteSettingCodeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, PortNo[{2}] CassetteSettingCodeChangeReportReply Timeout Set Bit [OFF].", sArray[0], trackKey, sArray[1]));

                CassetteSettingCodeChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        /// <summary>
        /// 記錄Port各種狀態變更
        /// </summary>
        public void RecordPortHistory(string trxID,Equipment eqp, Port port)
        {
            try
            {
                // Save DB
                PORTHISTORY his = new PORTHISTORY();
                his.UPDATETIME = DateTime.Now;
                his.LINEID = eqp.Data.LINEID;
                his.NODEID = eqp.Data.NODEID;
                his.PORTID = port.Data.PORTID;
                his.PORTNO = int.Parse(port.Data.PORTNO);
                his.PORTTYPE = port.File.Type.ToString();
                his.PORTMODE = port.File.Mode.ToString();
                his.PORTENABLEMODE = port.File.EnableMode.ToString();
                his.PORTTRANSFERMODE = port.File.TransferMode.ToString();
                his.PORTSTATUS = port.File.Status.ToString();
                his.CASSETTESEQNO = int.Parse(port.File.CassetteSequenceNo);
                his.CASSETTESTATUS = port.File.CassetteStatus.ToString();
                his.QTIMEFLAG = port.File.QTimeFlag.ToString();
                his.TRANSACTIONID = trxID;
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line != null)
                {
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())
                        his.GRADE = port.File.UseGrade;
                    else if (line.Data.FABTYPE == eFabType.CF.ToString())
                        his.GRADE = port.File.MappingGrade;
                    else
                        his.GRADE = port.File.Grade;
                }
                else
                {
                    his.GRADE = "NULL";
                }
                his.PRODUCTTYPE = port.File.ProductType;
                his.CASSETTESETCODE = port.File.CassetteSetCode;

                ObjectManager.PortManager.InsertDB(his);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void JobCountInCassette(Trx inputData)
        {
            try
            {
                #region [取得EQP及Port資訊]
                Line line; Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_INFORMATION=[{0}] IN LINEENTITY!", ServerName));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}][{2}]!", portNo, eqp.Data.NODENO,eqp.Data.NODEID));
                #endregion

                lock (port.File)
                {
                    port.File.JobCountInCassette = inputData.EventGroups[0].Events[0].Items[0].Value;
                }
                ObjectManager.PortManager.EnqueueSave(port.File);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] PORT=[{4}] CIM_MODE=[{2}]{3}.", eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, string.Format(" JOB_COUNT_IN_CST=[{0}]", port.File.JobCountInCassette), port.Data.PORTNO));

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });
                //add by qiumin 20180106 CheckUpkCstBuffer 
                if (line.Data.LINEID.Contains("UPK") && eqp.Data.NODENO == "L5" && eqp.File.EquipmentRunMode != "RE-CLEAN" && (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING || port.File.CassetteStatus == eCassetteStatus.IN_PROCESSING))
                {
                    Invoke(eServiceName.CFSpecialService, "CheckUpkCstBuffer",new object[]{inputData.TrackKey,eqp });
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
