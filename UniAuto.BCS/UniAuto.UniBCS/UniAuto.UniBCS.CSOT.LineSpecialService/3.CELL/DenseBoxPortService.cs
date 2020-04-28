using System;
using System.Reflection;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.OpiSpec;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Collections.Generic;

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
    public class DenseBoxPortService : AbstractService
    {
        private const string DPPortStsChangeTimeout = "DPPortStsChangeTimeout";
        private const string DPPortTypeChangeTimeout = "DPPortTypeChangeTimeout";
        private const string DPPortTypeChangeCommandTimeout = "DPPortTypeChangeCommandTimeout";
        private const string DPPortModeChangeTimeout = "DPPortModeChangeTimeout";
        private const string DPPortModeChangeCommandTimeout = "DPPortModeChangeCommandTimeout";
        private const string DPPortTransferModeChangeTimeout = "DPPortTransferModeChangeTimeout";
        private const string DPPortTransferModeChangeCommandTimeout = "DPPortTransferModeChangeCommandTimeout";
        private const string DPPortEnableModeChangeTimeout = "DPPortEnableModeChangeTimeout";
        private const string DPPortEnableModeChangeCommandTimeout = "DPPortEnableModeChangeCommandTimeout";

        public override bool Init()
        {
            return true;
        }

        #region [Port Status Change Report] ================================================
        /// <summary>
        /// 更新PortStatus
        /// </summary>
        /// <param name="inputData">PLC Trx物件</param>
        /// <param name="log">觸發者名稱(記Log用)</param>
        public void DPPortStatusChangeReportUpdate(Trx inputData, string sourceMethod)
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
                RefreshPortStatus_CELL(line, port, inputData,out log);
           
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
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] PORT=[{3}] PORT_STATUS=[{4}] PORT_CASSETTESTATUS=[{5}]({6}).",
                        eqp.Data.NODENO, sourceMethod, inputData.TrackKey, port.Data.PORTNO, port.File.Status,(int)port.File.CassetteStatus,port.File.CassetteStatus.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void DPPortStatusChangeReport(Trx inputData)
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
                if (line == null) throw new Exception(string.Format("Can't find Line Information [{0}] in LineEntity!", ServerName));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);

                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortStatus oldstatus = port.File.Status;
                if (inputData.IsInitTrigger)
                {
                    DPPortStatusChangeReportUpdate(inputData, "DPPortStatusChangeReport_Initial");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    DPPortStarusChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                string log;
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    RefreshPortStatus_CELL(line,  port, inputData,out log);
                }

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode [{2}], Port Status={3}, Bit [ON]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, port.File.Status.ToString()));

                ReportMESPortStatusChange(port, inputData);
                RecordPortHistory(inputData.TrackKey, eqp, port);

                DPPortStarusChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

                switch (port.File.Status)
                {
                    case ePortStatus.LR: break;
                    case ePortStatus.LC: break;
                    case ePortStatus.UR:
                        { /*刪Coolrun的資料改到UDCM*/ }
                        break;
                    case ePortStatus.UC:
                        {
                            //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                            Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));
                            if (cst != null)
                            {
                                lock (ObjectManager.CassetteManager) ObjectManager.CassetteManager.DeleteBox(port.File.CassetteID);  //DeleteCassette(port.File.CassetteSequenceNo);
                                cst.WriteFlag = false;
                                ObjectManager.CassetteManager.EnqueueSave(cst);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                    string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                    DPPortStarusChangeReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void DPPortStarusChangeReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_DP#{1}PortStatusChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPPortStsChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DPPortStarusChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,DP#{2} Port Status Change Report Reply Set Bit [{3}].",
                    eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DPPortStarusChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                DPPortStarusChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, DP#{2} Port Status Change Report Timeout Set Bit [OFF].",
                    sArray[0], trackKey, sArray[1]));
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

                    port.File.CompletedCassetteReason = (eCompleteCassetteReason)int.Parse(evt.Items[12].Value);
                    log += string.Format(" COMPLETE_CST_REASON=[{0}]({1})", evt.Items[12].Value, port.File.CompletedCassetteReason.ToString());

                    if (line.Data.LINETYPE == eLineType.CELL.CBDPK)
                    {
                        port.File.DPISampligFlag = (eBitResult)int.Parse(evt.Items[13].Value);
                        log += string.Format(" DPI_SAMPLING_FLAG=[{0}])({1})", evt.Items[13].Value, port.File.DPISampligFlag.ToString());
                    }

                    if (line.Data.LINETYPE == eLineType.CELL.CBPOL_1 || line.Data.LINETYPE == eLineType.CELL.CBPOL_2 || line.Data.LINETYPE == eLineType.CELL.CBPOL_3)  //Jun Modify 20141225 使來Line Type來判斷 
                    {
                        port.File.AutoClaveByPass = (eBitResult)int.Parse(evt.Items[13].Value);
                        log += string.Format(" AUTO_CLAVE_BYPASS=[{0}]({1})", evt.Items[13].Value, port.File.AutoClaveByPass.ToString());

                        port.File.MappingGrade = evt.Items[14].Value;
                        log += string.Format(" MAPPING_GRADE=[{0}]", port.File.MappingGrade);
                    }
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
                if (port.File.Status == ePortStatus.LR || port.File.Status == ePortStatus.UR)
                    return;

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

        public void RecordPortHistory(string trxID, Equipment eqp, Port port)
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
        #endregion

        #region [Port Type Change Report] ==================================================
        #region [**Port Type Change Report**]
        public void DPPortTypeChangeReportUpdate(Trx inputData, string sourceMethod)
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

        public void DPPortTypeChangeReport(Trx inputData)
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
                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);

                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortType oldtype = port.File.Type;
                if (inputData.IsInitTrigger)
                {
                    DPPortTypeChangeReportUpdate(inputData, "DPPortTypeChangeReport_Initial");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    DPPortTypeChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                lock (port.File) port.File.Type = (ePortType)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                ObjectManager.PortManager.EnqueueSave(port.File);
                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode [{2}], DP#{3} Port Type={4}, Bit [ON]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, portNo, port.File.Type.ToString()));

                ReportMESPortTypeChange(port, inputData);

                DPPortTypeChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

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

                    DPPortTypeChangeReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void DPPortTypeChangeReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_DP#{1}PortTypeChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPPortTypeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DPPortTypeChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,DP#{2} Port Type Change Report Reply Set Bit [{3}].",
                    eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DPPortTypeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                DPPortTypeChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, DP#{2} Port Type Change Report Timeout Set Bit [OFF].",
                    sArray[0], sArray[1], trackKey));
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
        #region [**Port Type Change Command**]
        public void DPPortTypeChangeCommand(PortCommandRequest msg)
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

                port = ObjectManager.PortManager.GetPort(msg.BODY.LINENAME, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

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
                if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}], THERE IS A CASSETTE ON THE PORT, CAN NOT CHANGE PORT TYPE!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, port.File.CassetteStatus);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                string trxName = string.Format("{0}_DP#{1}PortTypeChangeCommand", eqp.Data.NODENO, port.Data.PORTNO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = msg.BODY.PORTCOMMAND;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[BCS -> EQP][{0}] Port Type Change Command.", outputdata.TrackKey));

                string timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, port.Data.PORTNO, DPPortTypeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPPortTypeChangeCommandReplyTimeout), outputdata.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void DPPortTypeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                if (triggerBit == eBitResult.OFF) return;
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, portNo, DPPortTypeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                string trxName = string.Format("{0}_DP#{1}PortTypeChangeCommand", inputData.Metadata.NodeNo, portNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,DP#{2} Port Type Change Command Set Bit [OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, portNo));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DPPortTypeChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], DPPortTypeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_DP#{1}PortTypeChangeCommand", sArray[0], sArray[1]);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, DP#{2} Port Type Change Command Reply Timeout Set Bit [OFF].",
                    sArray[0], sArray[1], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion

        #region [Port Mode Change] =========================================================
        #region [**Port Mode Change Report**]
        /// <summary>
        /// 更新DPPortMode
        /// </summary>
        /// <param name="inputData">PLC Trx物件</param>
        /// <param name="log">觸發者名稱(記Log用)</param>
        public void DPPortModeChangeReportUpdate(Trx inputData, string sourceMethod)
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

        public void DPPortModeChangeReport(Trx inputData)
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
                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);

                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortMode oldmode = port.File.Mode;
                if (inputData.IsInitTrigger)
                {
                    DPPortModeChangeReportUpdate(inputData, "DPPortModeChangeReport_Initial");
                    return;
                }


                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    DPPortModeChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                lock (port.File) port.File.Mode = (ePortMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                ObjectManager.PortManager.EnqueueSave(port.File);
                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode [{2}], DP#{3} Port Mode={4}, Bit [ON]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, portNo, port.File.Mode.ToString()));

                ReportMESPortModeChange(port, inputData);

                DPPortModeChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

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

                    DPPortModeChangeReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void DPPortModeChangeReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_DP#{1}PortModeChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPPortModeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(DPPortModeChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,DP#{2} Port Mode Change Report Reply Set Bit [{3}].",
                    eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DPPortModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                DPPortModeChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, DP#{2} Port Mode Change Report Timeout Set Bit [OFF].",
                    sArray[0], sArray[1], trackKey));
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
        #region [**Port Mode Change Command**]
        public void DPPortModeChangeCommand(string trxID, PortCommandRequest msg)
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

                port = ObjectManager.PortManager.GetPort(msg.BODY.LINENAME, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);
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
                if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}], THERE IS A CASSETTE ON THE PORT, CAN NOT CHANGE PORT MODE!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, port.File.Mode);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                string trxName = string.Format("{0}_DP#{1}PortModeChangeCommand", eqp.Data.NODENO, port.Data.PORTNO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = msg.BODY.PORTCOMMAND;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, port.Data.PORTNO, DPPortModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPPortModeChangeCommandReplyTimeout), outputdata.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void DPPortModeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                if (triggerBit == eBitResult.OFF) return;
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, portNo, DPPortModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                string trxName = string.Format("{0}_DP#{1}PortModeChangeCommand", inputData.Metadata.NodeNo, portNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,DP#{2} Port Mode Change Command Set Bit [OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, portNo));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DPPortModeChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], DPPortModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_DP#{1}PortModeChangeCommand", sArray[0], sArray[1]);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, DP#{2} Port Mode Change Command Reply Timeout Set Bit [OFF].",
                    sArray[0], sArray[1], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion

        #region [Port Transfer Mode Change] ================================================
        #region [**Port Transfer Mode Change Report**]

        /// <summary>
        /// 更新DPPortTransferMode
        /// </summary>
        /// <param name="inputData">PLC Trx物件</param>
        /// <param name="log">觸發者名稱(記Log用)</param>
        public void DPPortTransferModeChangeReportUpdate(Trx inputData, string sourceMethod)
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

        public void DPPortTransferModeChangeReport(Trx inputData)
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
                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);

                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortTransferMode oldmode = port.File.TransferMode;
                if (inputData.IsInitTrigger)
                {
                    DPPortTransferModeChangeReportUpdate(inputData, "DPPortTransferModeChangeReport");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    DPPortTransferChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                lock (port.File) port.File.TransferMode = (ePortTransferMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                ObjectManager.PortManager.EnqueueSave(port.File);
                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });


                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode [{2}], DP#{3} Port Transfer Mode={4}, Bit [ON]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, portNo, port.File.TransferMode.ToString()));

                ReportMESPortTransferChange(port, inputData);

                DPPortTransferChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

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

                    DPPortTransferChangeReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void DPPortTransferChangeReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_DP#{1}PortTransferModeChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPPortTransferModeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(DPPortTransferChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,DP#=[{2}) Port Transfer Mode Change Report Reply Set Bit [{3}].",
                    eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DPPortTransferChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                DPPortTransferChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, DP#{2} Port Transfer Mode Change Report Timeout Set Bit [OFF].",
                    sArray[0], sArray[1], trackKey));
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
        public void DPPortTransferModeChangeCommand(PortCommandRequest msg)
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

                port = ObjectManager.PortManager.GetPort(msg.BODY.LINENAME, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

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
                //if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST)
                //{
                //    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}], THERE IS A CASSETTE ON THE PORT, CAN NOT CHANGE PORT TRANSFER MODE!",
                //        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, port.File.CassetteStatus);

                //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                //        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });

                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                //    return;
                //}

                string trxName = string.Format("{0}_DP#{1}PortTransferModeChangeCommand", eqp.Data.NODENO, port.Data.PORTNO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = msg.BODY.PORTCOMMAND;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);

                string timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, port.Data.PORTNO, DPPortTransferModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPPortTransferModeChangeCommandReplyTimeout), outputdata.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void DPPortTransferModeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                if (triggerBit == eBitResult.OFF) return;
                eReturnCode1 retCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, portNo, DPPortTransferModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                string trxName = string.Format("{0}_DP#{1}PortTransferModeChangeCommand", inputData.Metadata.NodeNo, portNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,DP#{2} Port Transfer Mode Change Command Set Bit [OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, portNo));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DPPortTransferModeChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], DPPortTransferModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_DP#{1}PortTransferModeChangeCommand", sArray[0], sArray[1]);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, DP#{2} Port Transfer Mode Change Command Reply Timeout Set Bit [OFF].",
                    sArray[0], sArray[1], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion

        #region [Port Enable Mode Change] ==================================================
        #region [**Port Enable Mode Change Report**]
        /// <summary>
        /// 更新PortEnableMode
        /// </summary>
        /// <param name="inputData">PLC Trx物件</param>
        /// <param name="log">觸發者名稱(記Log用)</param>
        public void DPPortEnableModeChangeReportUpdate(Trx inputData, string sourceMethod)
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

                ePortEnableMode oldmode = port.File.EnableMode;

                lock (port.File) port.File.EnableMode = (ePortEnableMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                ObjectManager.PortManager.EnqueueSave(port.File);

                if (port.File.EnableMode != oldmode)
                {
                    ReportMESPortEnableChange(port, inputData);
                }

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

        public void DPPortEnableModeChangeReport(Trx inputData)
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
                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);

                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                ePortEnableMode oldmode = port.File.EnableMode;
                if (inputData.IsInitTrigger)
                {
                    DPPortEnableModeChangeReportUpdate(inputData, "DPPortEnableModeChangeReport_Initial");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    DPPortEnableChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                lock (port.File) port.File.EnableMode = (ePortEnableMode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                ObjectManager.PortManager.EnqueueSave(port.File);
                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });


                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode [{2}], DP#{3} Port Enable Mode={4}, Bit [ON]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, portNo, port.File.EnableMode.ToString()));

                ReportMESPortEnableChange(port, inputData);

                DPPortEnableChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

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

                    DPPortEnableChangeReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void DPPortEnableChangeReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_DP#{1}PortEnableModeChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPPortEnableModeChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(DPPortEnableChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,DP#{2} Port Enable Mode Change Report Reply Set Bit [{3}].",
                    eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DPPortEnableChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                DPPortEnableChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, DP#{2} Port Enable Mode Change Report Timeout Set Bit [OFF].",
                    sArray[0], sArray[1], trackKey));
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
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region **[Port Enable Mode Change Command]
        public void DPPortEnableModeChangeCommand(PortCommandRequest msg)
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

                port = ObjectManager.PortManager.GetPort(msg.BODY.LINENAME, msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

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
                if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}], THERE IS A CASSETTE ON THE PORT, CAN NOT CHANGE PORT ENABLE MODE!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, port.File.Mode);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID, "BCS PLC COMMAND - REJECT", err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                string trxName = string.Format("{0}_DP#{1}PortEnableModeChangeCommand", eqp.Data.NODENO, port.Data.PORTNO);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = msg.BODY.PORTCOMMAND;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);

                string timeName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, port.Data.PORTNO, DPPortEnableModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPPortEnableModeChangeCommandReplyTimeout), outputdata.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void DPPortEnableModeChangeCommandReply(Trx inputData)
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

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, portNo, DPPortEnableModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                string trxName = string.Format("{0}_DP#{1}PortEnableModeChangeCommand", inputData.Metadata.NodeNo, portNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
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

        private void DPPortEnableModeChangeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], DPPortEnableModeChangeCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_DP#{1}PortEnableModeChangeCommand", sArray[0], sArray[1]);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, DP#{2} Port Enable Mode Change Command Reply Timeout Set Bit [OFF].",
                    sArray[0], sArray[1], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #endregion

        private void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }
    }
}
