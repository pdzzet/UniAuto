using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.MesSpec;
using System.Reflection;
using UniAuto.UniBCS.MISC;
using System;
using UniAuto.UniBCS.OpiSpec;

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
    

    public partial class DenseBoxCassetteService : AbstractService
    {
        const string Key_DPPortStatus = "L2_DP#{0}PortStatusChangeReport";
        const string Key_DPCstStatus = "L2_DP#{0}JobEachCassetteSlotPositionBlock";
        #region [Cassette Status Change] ================================================
        /// <summary>
        /// 更新記億體資料
        /// </summary>
        /// <param name="inputData">同CassetteStatusChangeReport(Trx inputData)</param>
        /// <param name="log">記錄動作者</param>
        public void DPCassetteStatusChangeReportUpdateDPI(Trx inputData, string sourceMethod)
        {
            try
            {
                #region [取得EQP及Port資訊]
                Line line; Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, ServerName));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_PORT, portNo));

                Port port2 = ObjectManager.PortManager.GetPortByDPI(eqp.Data.NODENO, portNo);

                if (port2 == null) throw new Exception(string.Format("Can't find DPI Port No=[{0}] in Equipment No=[{1}]!", portNo, inputData.Metadata.NodeNo));
         
                #endregion

                string log;
                RefreshPortStatus_DPI(line, port,port2, inputData, out log);

                if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                {
                    lock (port) port.File.OPI_SubCstState = eOPISubCstState.WACSTEDIT;
                }

                if (port2.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                {
                    lock (port2) port2.File.OPI_SubCstState = eOPISubCstState.WACSTEDIT;
                }


                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });
                //Watson Modify 20150411 For DPI UPDATE OPI LAYOUT PORT
                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port2 });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}]{3}.", eqp.Data.NODENO, sourceMethod, inputData.TrackKey, log));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteStatusChangeReportDPI(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[1].Events[1].Items[0].Value);
                #endregion

                #region [取得EQP及Port資訊]
                Line line; Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("Can't find Line Information =[{0}] in LineEntity!", ServerName));

                #region [Port資訊]
                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("Can't find Port No. in Trx Name=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null) throw new Exception(string.Format("Can't find Port No=[{0}] in Equipment No=[{1}]!", portNo, inputData.Metadata.NodeNo));

                Port port2 = ObjectManager.PortManager.GetPortByDPI(eqp.Data.NODENO, portNo);

                if (port2 == null) throw new Exception(string.Format("Can't find DPI Port No=[{0}] in Equipment No=[{1}]!", portNo, inputData.Metadata.NodeNo));
         
                #endregion
                #endregion


                //在EquipmentService/ CIMModeUpdateAllStatus做掉
                if (inputData.IsInitTrigger)
                {
                    DPCassetteStatusChangeReportUpdateDPI(inputData, "DPCassetteStatusChangeReport_Initial");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    DPCassetteStatusChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                DPCassetteStatusChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

                string log;
                RefreshPortStatus_DPI(line, port, port2,inputData,out log);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Bit [ON] " + log,
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode ));

                Cassette cst = null;
                Cassette cst2 = null;
                //收集資料而且上報mes
                HandleCstStatusDPI(inputData.TrackKey, line, eqp, port, port2, inputData, out cst, out cst2);

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });
                //Watson Modify 20150411 For DPI UPDATE OPI LAYOUT PORT
                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port2 });

                // 處理完記Cassette History
                RecordCassetteHistory(inputData.TrackKey, eqp, port, cst, string.Empty, string.Empty, string.Empty);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[1].Events[1].Items[0].Value.Equals("1"))
                {
                    int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                    string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                    DPCassetteStatusChangeReportReply(inputData.Name.Split('_')[0], portNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void DPCassetteStatusChangeReportReply2(string eqpNo, string portNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_DP#{1}CassetteStatusChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPCstStsChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(DPCassetteStatusChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,DP#{2} Cassette Status Change Report Reply Set Bit =[{3}].",
                    eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DPCassetteStatusChangeReportReplyTimeout2(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, DP#{2} Cassette Status Change Report Timeout Set Bit [OFF].",
                    sArray[0], sArray[1], trackKey));

                DPCassetteStatusChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void HandleCstStatusDPI(string trxID, Line line, Equipment eqp, Port port, Port port2, Trx inputData, out Cassette cst, out Cassette cst2)
        {
            string err = string.Empty;
            cst = null;
            cst2 = null;
            IList<Job> jobs = new List<Job>();
            IList<Job> jobs2 = new List<Job>();
            string reasonCode;
            string reasonText;
            try
            {
                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim()); 
                cst2 = ObjectManager.CassetteManager.GetCassette(port2.File.CassetteID.Trim());

                lock (port) port.File.OPI_SubCstState = eOPISubCstState.NONE;
                lock (port2) port2.File.OPI_SubCstState = eOPISubCstState.NONE;

                switch (port.File.CassetteStatus)
                {
                    case eCassetteStatus.NO_CASSETTE_EXIST:
                        {
                            //Nothing
                        }
                        break;
                    case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                        {
                            if (port.File.Status != ePortStatus.LC)
                            {
                                err = string.Format("EQUIPMENT={0} PORTNO#{1}, Port Status={2} and Cassette Status={3} is Invalid,  ",
                                    port.Data.NODEID, port.Data.PORTNO, port.File.Status.ToString(), port.File.CassetteStatus.ToString());
                                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                DPCassetteProcessCancel(port.Data.NODENO, port.Data.PORTNO);
                            }
                            #region [新增卡匣到Cassette Entity]
                            cst = new Cassette()
                            {
                                LineID = port.Data.LINEID.Trim(),
                                NodeID = port.Data.NODEID.Trim(),
                                NodeNo = port.Data.NODENO.Trim(),
                                PortID = port.Data.PORTID.Trim(),
                                PortNo = port.Data.PORTNO.Trim(),
                                CassetteID = port.File.CassetteID.Trim(),
                                CassetteSequenceNo = port.File.CassetteSequenceNo,
                                CassetteControlCommand =  eCstControlCmd.Load,
                                LoadTime = DateTime.Now,
                            };
                            ObjectManager.CassetteManager.CreateBox(cst); //Watson modify 20150130 by Box ID Create
                            #endregion

                            #region [新增卡匣到Cassette Entity]
                            cst2 = new Cassette()
                            {
                                LineID = port2.Data.LINEID.Trim(),
                                NodeID = port2.Data.NODEID.Trim(),
                                NodeNo = port2.Data.NODENO.Trim(),
                                PortID = port2.Data.PORTID.Trim(),
                                PortNo = port2.Data.PORTNO.Trim(),
                                CassetteID = port2.File.CassetteID.Trim(),
                                CassetteSequenceNo = port2.File.CassetteSequenceNo,
                                CassetteControlCommand = eCstControlCmd.Load,
                                LoadTime = DateTime.Now,
                            };
                            ObjectManager.CassetteManager.CreateBox(cst2); //Watson modify 20150130 by Box ID Create
                            #endregion

                            if (line.File.HostMode == eHostMode.OFFLINE)
                            {
                                //Watson Add 20150411 以Port1為主，不需要再更動Port2 閃爍，也不需要再停止
                                lock (port) port.File.OPI_SubCstState = eOPISubCstState.WACSTEDIT;

                                //Watson Add 20150411 以Port2為主，不需要再更動Port2 閃爍，也不需要再停止
                                lock (port2) port2.File.OPI_SubCstState = eOPISubCstState.WACSTEDIT;
                            }
                            else
                            {
                                List<string> boxlist = new List<string>();
                                if (port.File.CassetteID.Trim() != string.Empty)
                                    boxlist.Add(port.File.CassetteID.Trim());
                                if (port2.File.CassetteID.Trim() != string.Empty)
                                    boxlist.Add(port2.File.CassetteID.Trim());

                                #region 上報給MES Data
                                object[] _data = new object[3]
                                {
                                inputData.TrackKey,          /*0 TrackKey*/
                                port,
                                boxlist
                                };
                                // MES Data
                                Invoke(eServiceName.MESService, "ValidateBoxRequest", _data);
                                #endregion
                            }

                        }
                        break;
                    case eCassetteStatus.WAITING_FOR_START_COMMAND:
                        {
                            if (line.File.HostMode == eHostMode.REMOTE)
                            {
                                if (cst == null)
                                {
                                    err = string.Format("CAN'T FIND BOX INFORMATION, BOX ID =[{0}] ,CST_SEQNO=[{1}] IN CASSETTE OBJECT!", port.File.CassetteID,port.File.CassetteSequenceNo);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[] { inputData.TrackKey, eqp.Data.LINEID, "[DPCassetteStatusChangeReport] " + err });
                                    throw new Exception(err);
                                }
                                //TODO: Process Start有分 Normal 及ByCount
                                if (port.Data.PROCESSSTARTTYPE.Equals("0"))
                                {
                                    DPCassetteProcessStart(port.Data.NODENO, port.Data.PORTNO);
                                }
                            }
                            else // Offline及Local要由UI下命令
                            {
                                port.File.OPI_SubCstState = eOPISubCstState.WASTART;
                                port2.File.OPI_SubCstState = eOPISubCstState.WASTART;
                                ObjectManager.PortManager.EnqueueSave(port.File);
                                ObjectManager.PortManager.EnqueueSave(port2.File);
                            }
                        }
                        break;
                    case eCassetteStatus.WAITING_FOR_PROCESSING:
                        {
                            if (line.File.HostMode == eHostMode.REMOTE)
                            {
                                string timeoutName = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);
                                if (_timerManager.IsAliveTimer(timeoutName))
                                {
                                    _timerManager.TerminateTimer(timeoutName);
                                }
                            }

                            if (cst == null)
                            {
                                err = string.Format("CAN'T FIND BOX INFORMATION, BOX ID =[{0}] ,CST_SEQNO=[{1}] IN CASSETTE OBJECT!", port.File.CassetteID, port.File.CassetteSequenceNo);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { inputData.TrackKey, eqp.Data.LINEID, "[DPCassetteStatusChangeReport] " + err });
                                throw new Exception(err);
                            }
                            else
                            {
                                lock (cst) cst.StartTime = DateTime.Now;
                            }

                            if (cst2 == null)
                            {
                                err = string.Format("CAN'T FIND BOX2 INFORMATION, BOX2 ID =[{0}] ,CST_SEQNO=[{1}] IN CASSETTE OBJECT!", port2.File.CassetteID, port2.File.CassetteSequenceNo);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { inputData.TrackKey, eqp.Data.LINEID, "[DPCassetteStatusChangeReport] " + err });
                                throw new Exception(err);
                            }
                            else
                            {
                                lock (cst2) cst2.StartTime = DateTime.Now;
                            }
                        }
                        break;
                    case eCassetteStatus.IN_PROCESSING:
                            if ((port.File.Type == ePortType.UnloadingPort) || (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.VirtualPort))
                                Invoke(eServiceName.MESService, "BoxProcessStarted", new object[] { inputData.TrackKey, port, cst });
                            //Watson Add 20150102 For Box Remove
                            cst.CellBoxProcessed = eboxReport.Processing;
                        break;
                    case eCassetteStatus.PROCESS_PAUSED:
                        {
                            // Nothing
                        }
                        break;
                    case eCassetteStatus.PROCESS_COMPLETED:
                        {
                            if (cst == null)
                            {
                                err = string.Format("CAN'T FIND BOX INFORMATION, BOXID=[{0}] CST_SEQNO=[{1}] IN CASSETTE OBJECT!", port.File.CassetteID,port.File.CassetteSequenceNo);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { inputData.TrackKey, eqp.Data.LINEID, "[DPCassetteStatusChangeReport] " + err });
                                throw new Exception(err);
                            }
                            else
                            {
                                lock (cst) cst.EndTime = DateTime.Now;
                            }

                            GetPortJobData(inputData, port, ref jobs);

                            if (cst2 == null)
                            {
                                err = string.Format("CAN'T FIND BOX2 INFORMATION,  BOXID=[{0}] CST_SEQNO=[{0}] IN CASSETTE OBJECT!", port2.File.CassetteID, port2.File.CassetteSequenceNo);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { inputData.TrackKey, eqp.Data.LINEID, "[DPCassetteStatusChangeReport] " + err });
                                throw new Exception(err);
                            }
                            else
                            {
                                lock (cst2) cst2.EndTime = DateTime.Now;
                            }
                            GetPortJobData(DPI_READPLCDdata(string.Format(Key_DPCstStatus, port2.Data.PORTNO)), port2, ref jobs2);

                            if (line.File.HostMode != eHostMode.OFFLINE)
                            {
                                #region Report MES
                                switch (port.File.CompletedCassetteData)
                                {
                                    case eCompletedCassetteData.NormalComplete:
                                        {
                                            #region [Report to MES LotProcessEnd or BoxProcessEnd]
                                            // MES Data
                                            MES_BOXPROCESSENDBYDPI(line, eqp, port,port2,jobs,jobs2, inputData.TrackKey);
                                            #endregion
                                            //Watson Add 20150102 For Box Remove
                                            cst.CellBoxProcessed = eboxReport.NOReport;
                                            cst2.CellBoxProcessed = eboxReport.NOReport;
                                        }
                                        break;
                                    case eCompletedCassetteData.BCForcedToAbort:
                                    case eCompletedCassetteData.EQAutoAbort:
                                    case eCompletedCassetteData.OperatorForcedToAbort:
                                        {
                                            GetReasonCodeAndText(port, cst, out reasonCode, out reasonText);
                                            #region [Report to MES BoxProcessEnd]
                                            MES_BOXPROCESSENDBYDPI(line, eqp, port,port2, jobs,jobs2,inputData.TrackKey);
                                            #endregion
                                            //Watson Add 20150102 For Box Remove
                                            cst.CellBoxProcessed = eboxReport.NOReport;
                                            cst2.CellBoxProcessed = eboxReport.NOReport;
                                        }
                                        break;
                                    case eCompletedCassetteData.BCForcedToCancel:
                                    case eCompletedCassetteData.EQAutoCancel:
                                    case eCompletedCassetteData.OperatorForcedToCancel:
                                        {
                                            GetReasonCodeAndText(port, cst, out reasonCode, out reasonText);
                                            #region [Report to MES LotProcessCanceled or BoxProcessCanceled]
                                            // MES Data
                                            object[] obj = new object[]
                                            {
                                                inputData.TrackKey,
                                                line.Data.LINEID,
                                                port.Data.PORTID,
                                                port.File.CassetteID,
                                                reasonCode,
                                                reasonText
                                            };
                                            Invoke(eServiceName.MESService, "BoxProcessCanceled", obj);
                                            #endregion
                                            //Watson Add 20150102 For Box Remove
                                            cst.CellBoxProcessed = eboxReport.NOReport;
                                            cst2.CellBoxProcessed = eboxReport.NOReport;
                                        }
                                        break;
                                }
                                #endregion
                            }

                            if (!jobs.Count().Equals(0))
                            {
                                ObjectManager.JobManager.DeleteJobs(jobs);
                                //Watson Add 20150418 For Save History.
                                ObjectManager.JobManager.RecordJobsHistory(jobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Delete_CST_Complete.ToString(), inputData.TrackKey);
   
                            }

                            if (!jobs2.Count().Equals(0))
                            {
                                ObjectManager.JobManager.DeleteJobs(jobs2);
                                //Watson Add 20150418 For Save History.
                                ObjectManager.JobManager.RecordJobsHistory(jobs2, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Delete_CST_Complete.ToString(), inputData.TrackKey);
   
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RefreshPortStatus_DPI(Line line, Port port,Port port2, Trx inputData, out string log)
        {
            log = string.Format(" PORT=[{0}]", port.Data.PORTNO);

            try
            {
                Event evt = inputData.EventGroups[1].Events[0];
                #region Port1 
                lock (port.File)
                {
                    port.File.CassetteID = evt.Items[3].Value;
                    log += string.Format(" BOXID=[{0}]", port.File.CassetteID);

                    port.File.Status = (ePortStatus)int.Parse(evt.Items[0].Value);
                    log += string.Format(" PORT_STATUS=[{0}]({1})", evt.Items[0].Value, port.File.Status.ToString());

                    port.File.CassetteStatus = (eCassetteStatus)int.Parse(evt.Items[1].Value);
                    log += string.Format(" CST_STATUS=[{0}]({1})", evt.Items[1].Value, port.File.CassetteStatus.ToString());

                    port.File.CassetteSequenceNo = evt.Items[2].Value;
                    log += string.Format(" BOX_SEQNO=[{0}]", port.File.CassetteSequenceNo);

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

                    port.File.CassetteSetCode = inputData.EventGroups[1].Events[0].Items[11].Value;
                    log += string.Format(" CST_SETCODE=[{0}]", port.File.CassetteSetCode.ToString());

                    port.File.CompletedCassetteReason = (eCompleteCassetteReason)int.Parse(inputData.EventGroups[0].Events[0].Items[12].Value);
                    log += string.Format(" COMPLETE_CST_REASON=[{0}]({1})", evt.Items[12].Value, port.File.CompletedCassetteReason.ToString());

                }
                ObjectManager.PortManager.EnqueueSave(port.File);
                #endregion

                #region Port2
                //L2_W_DP#02PortandCassetteStatusBlock
                Trx inputData2 = DPI_READPLCDdata(string.Format(Key_DPPortStatus, port2.Data.PORTNO));
                if (inputData2 == null)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Event Key Error!![" + string.Format(Key_DPPortStatus, port2.Data.PORTNO) + "]");
                }
                Event evt2 = inputData2.EventGroups[0].Events[0];

                log += string.Format(" PORT=[{0}]", port2.Data.PORTNO);

                lock (port2.File)
                {
                    port2.File.CassetteID = evt2.Items[3].Value;
                    log += string.Format(" BOX2ID=[{0}]", port2.File.CassetteID);

                    port2.File.Status = (ePortStatus)int.Parse(evt2.Items[0].Value);
                    log += string.Format(" PORT_STATUS=[{0}]({1})", evt2.Items[0].Value, port2.File.Status.ToString());

                    port2.File.CassetteStatus = (eCassetteStatus)int.Parse(evt2.Items[1].Value);
                    log += string.Format(" CST_STATUS=[{0}]({1})", evt2.Items[1].Value, port2.File.CassetteStatus.ToString());

                    port2.File.CassetteSequenceNo = evt2.Items[2].Value;
                    log += string.Format(" BOX_SEQNO=[{0}]", port2.File.CassetteSequenceNo);

                    port2.File.JobCountInCassette = evt2.Items[4].Value;
                    log += string.Format(" JOB_COUNT_IN_CST=[{0}]", port2.File.JobCountInCassette);

                    port2.File.CompletedCassetteData = (eCompletedCassetteData)int.Parse(evt2.Items[5].Value);
                    log += string.Format(" COMPLETED_CST_DATA=[{0}]({1})", evt2.Items[5].Value, port2.File.CompletedCassetteData.ToString());

                    port2.File.OperationID = evt2.Items[6].Value;
                    log += string.Format(" OPERATIONID=[{0}]", port2.File.OperationID);

                    port2.File.JobExistenceSlot = evt2.Items[7].Value.Substring(0, port2.Data.MAXCOUNT);
                    log += string.Format(" JOB_EXISTENCE_SLOT=[{0}]", port2.File.JobExistenceSlot);

                    port2.File.LoadingCassetteType = (eLoadingCstType)int.Parse(evt2.Items[8].Value);
                    log += string.Format(" LOADING_CST_TYPE=[{0}]({1})", evt2.Items[8].Value, port2.File.LoadingCassetteType.ToString());

                    port2.File.QTimeFlag = (eQTime)int.Parse(evt2.Items[9].Value);
                    log += string.Format(" Q_TIMEFLAG=[{0}]({1})", evt2.Items[9].Value, port2.File.QTimeFlag.ToString());

                    port2.File.PartialFullFlag = (eParitalFull)int.Parse(evt2.Items[10].Value);
                    log += string.Format(" PARTIAL_FULL_FLAG=[{0}]({1})", evt2.Items[10].Value, port2.File.PartialFullFlag.ToString());

                    port2.File.CassetteSetCode = inputData.EventGroups[1].Events[0].Items[11].Value;
                    log += string.Format(" CST_SETCODE=[{0}]", port2.File.CassetteSetCode.ToString());

                    port2.File.CompletedCassetteReason = (eCompleteCassetteReason)int.Parse(inputData.EventGroups[0].Events[0].Items[12].Value);
                    log += string.Format(" COMPLETE_CST_REASON=[{0}]({1})", evt2.Items[12].Value, port2.File.CompletedCassetteReason.ToString());
                }
                ObjectManager.PortManager.EnqueueSave(port2.File);
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region [Cassette Control Command]
        //TODO: 還沒寫完//T3 都走正常上portFlow
        public void DPCassetteProcessStart2(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", eqpNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);

                if (port == null)
                {
                    err = string.Format("Can't find Port No=[{0}] in Equipment No=[{1}]!", portNo, eqpNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, portNo, port.File.CassetteSequenceNo);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Start\" command!",
                        eqp.Data.NODENO, portNo);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Start\" command!",
                        eqp.Data.NODENO, portNo, port.File.Status.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Start\" command!",
                        eqp.Data.NODENO, portNo, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqpNo, portNo);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessStart).ToString();
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessStart;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessStart).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Start\" command, Set Bit [ON]",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessStart.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessStart_UI2(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);
                    throw new Exception(err);
                }

                //IList<Port> portList = new List<Port>();
                //portList.Add
                //    ObjectManager.PortManager.GetPortByDPI("L2", msg.BODY.PORTNO);


                foreach (Port port in ObjectManager.PortManager.GetPorts())
                {
                    
                    if (port == null)
                    {
                        err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                            string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                        throw new Exception(err);
                    }

                    //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                    if (cst == null)
                    {
                        err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOXID=[{2}].",
                            eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);

                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                            string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                        throw new Exception(err);
                    }
                    #endregion

                    #region Data and Status Check
                    // CIM MODE OFF 不能改
                    if (eqp.File.CIMMode == eBitResult.OFF)
                    {
                        err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Start\" command!",
                            eqp.Data.NODENO, msg.BODY.PORTNO);
                        //TODO: 打訊息到OPI
                        throw new Exception(err);
                    }

                    if (port.File.Status != ePortStatus.LC)
                    {
                        err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Start\" command!",
                            eqp.Data.NODENO, msg.BODY.PORTNO, port.File.Status.ToString());
                        //TODO: 打訊息到OPI
                        throw new Exception(err);
                    }

                    if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                    {
                        err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Start\" command!",
                            eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
                        //TODO: 打訊息到OPI
                        throw new Exception(err);
                    }
                    #endregion

                    string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
                    Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                    outputData.ClearTrxWith0();

                    outputData.EventGroups[0].IsDisable = true;
                    outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessStart).ToString();
                    outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                    //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                    outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                    SendPLCData(outputData);
                    lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessStart;
                    ObjectManager.CassetteManager.EnqueueSave(cst);

                    string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, DPCstControlCommandTimeout);
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessStart).ToString());

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Start\" command, Set Bit [ON]",
                        eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                    RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessStart.ToString(), string.Empty, msg.BODY.OPERATORID);

                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessStartByCount2(string eqpNo, string portNo, int count)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", eqpNo);
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqpNo, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND BOX_DATA FROM BOXID=[{2}].", eqp.Data.NODENO,
                        port.Data.PORTNO, port.File.CassetteID);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, portNo);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, portNo, port.File.Status.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, portNo, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqpNo, portNo);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessStartByCount).ToString();
                outputData.EventGroups[1].Events[0].Items[2].Value = count.ToString();
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessStartByCount;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessStartByCount).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Start By Count\" command, Set Bit [ON]",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessStartByCount.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessStartByCount_UI2(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", msg.BODY.EQUIPMENTNO);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("Can't find Port No=[{0}] in Equipment No=[{1}]!", msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.Status.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessStartByCount).ToString();
                outputData.EventGroups[1].Events[0].Items[2].Value = msg.BODY.PROCESSCOUNT;
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessStartByCount;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessStartByCount).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Start By Count\" command, Set Bit [ON]",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessStartByCount.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessPause2(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", eqpNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqpNo, portNo);

                if (port == null)
                {
                    err = string.Format("Can't find Port No=[{0}] in Equipment No=[{1}]!", portNo, eqpNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, portNo, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Pause\" command!",
                        eqp.Data.NODENO, portNo);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Pause\" command!",
                        eqp.Data.NODENO, portNo, port.File.Status.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Pause\" command!",
                        eqp.Data.NODENO, portNo, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqpNo, portNo);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessPause).ToString();
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessPause;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessPause).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Pause\" command, Set Bit [ON]",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessPause.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessPause_UI2(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", msg.BODY.EQUIPMENTNO);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("Can't find Port No=[{0}] in Equipment No=[{1}]!", msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Pause\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Pause\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.Status.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Pause\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                #endregion

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessPause).ToString();
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessPause;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessPause).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Pause\" command, Set Bit [ON]",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessPause.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessResume2(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", eqpNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqpNo, portNo);

                if (port == null)
                {
                    err = string.Format("Can't find Port No=[{0}] in Equipment No=[{1}]!", portNo, eqpNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, portNo, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Resume\" command!",
                        eqp.Data.NODENO, portNo);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Resume\" command!",
                        eqp.Data.NODENO, portNo, port.File.Status.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Resume\" command!",
                        eqp.Data.NODENO, portNo, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqpNo, portNo);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessResume).ToString();
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessResume;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessResume).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Resume\" command, Set Bit [ON]",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessResume.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessResume_UI2(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", msg.BODY.EQUIPMENTNO);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("Can't find Port No=[{0}] in Equipment No=[{1}]!", msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Resume\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Resume\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.Status.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Resume\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessResume).ToString();
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessResume;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessResume).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Resume\" command, Set Bit [ON]",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessResume.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessAbort2(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", eqpNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqpNo, portNo);

                if (port == null)
                {
                    err = string.Format("Can't find Port No=[{0}] in Equipment No=[{1}]!", portNo, eqpNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, portNo, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Abort\" command!",
                        eqp.Data.NODENO, portNo);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Abort\" command!",
                        eqp.Data.NODENO, portNo, port.File.Status.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Abort\" command!",
                        eqp.Data.NODENO, portNo, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqpNo, portNo);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessAbort).ToString();
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessAbort;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessAbort).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Abort\" command, Set Bit [ON]",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessAbort.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessAbort_UI2(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", msg.BODY.EQUIPMENTNO);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("Can't find Port No=[{0}] in Equipment No=[{2}]!", msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Abort\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Abort\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.Status.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Abort\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                #endregion

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessAbort).ToString();
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);

                lock (cst)
                {
                    cst.CassetteControlCommand = eCstControlCmd.ProcessAbort;
                    cst.ReasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_BC_Abort :
                        MES_ReasonCode.Loader_BC_Abort;
                }
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessAbort).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Resume\" command, Set Bit [ON]",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessAbort.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessCancel2(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", eqpNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);

                if (port == null)
                {
                    err = string.Format("Can't find Port No=[{0}] in Equipment No=[{1}]!", portNo, eqpNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, portNo, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Cancel\" command!",
                        eqp.Data.NODENO, portNo);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Cancel\" command!",
                        eqp.Data.NODENO, portNo, port.File.Status.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Cancel\" command!",
                        eqp.Data.NODENO, portNo, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqpNo, portNo);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();
                outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessCancel).ToString();
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessCancel;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessCancel).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Cancel\" command, Set Bit [ON]",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessCancel.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessCancel_UI2(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", msg.BODY.EQUIPMENTNO);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("Can't find Port No=[{0}] in Equipment No=[{1}]!", msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    //throw new Exception(err); modify for t2,t3 sync 2016/04/14 cc.kuang
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Process Cancel\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO);
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Process Cancel\" command!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.Status.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING)
                {
                    err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Process Cancel\" command!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteStatus.ToString());
                    //TODO: 打訊息到OPI
                    throw new Exception(err);
                }
                #endregion

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessCancel).ToString();
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst)
                {
                    cst.CassetteControlCommand = eCstControlCmd.ProcessCancel;
                    cst.ReasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_BC_Cancel_From_BC_Client :
                        MES_ReasonCode.Loader_BC_Cancel_From_BC_Client;
                }
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessCancel).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Process Cancel\" command, Set Bit [ON]",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessCancel.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteControlCommandReply2(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                if (triggerBit == eBitResult.OFF) return;
                eCstCmdRetCode retCode = (eCstCmdRetCode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("Can't find Port No. in Trx Name=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, portNo, DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", inputData.Metadata.NodeNo, portNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].IsDisable = true;
                outputdata.EventGroups[1].Events[0].IsDisable = true;
                outputdata.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                string err = string.Empty;
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null)
                {
                    err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity! Skip record Cassette History.", inputData.Metadata.NodeNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqp.Data.NODENO, portNo);

                if (port == null)
                {
                    err = string.Format("Can't find Port No=[{0}] in Equipment No=[{1}]! Skip record Cassette History", eqp.Data.NODENO, portNo);

                    //TODO: 打訊息到OPI
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                string methodName = MethodBase.GetCurrentMethod().Name + "()";

                if (cst != null)
                {
                    methodName = string.Format("Cassette{0}Reply()", cst.CassetteControlCommand.ToString());
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, methodName,
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,DP#{2} Cassette Control Command Set Bit [OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, portNo));

                RecordCassetteHistory(inputData.TrackKey, eqp, port, cst, string.Empty, retCode.ToString(), string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DPCassetteControlCommandTimeout2(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string[] obj = timer.State.ToString().Split('_');
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], DPCstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", sArray[0], sArray[1]);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].IsDisable = true;
                outputdata.EventGroups[1].Events[0].IsDisable = true;
                outputdata.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = obj[0];
                SendPLCData(outputdata);
                eCstControlCmd cmd = (eCstControlCmd)int.Parse(obj[1]);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, DP#=[{2}] Cassette Control Command=[{3}] Reply Timeout Set Bit [OFF].",
                    sArray[0], obj[0], sArray[1], cmd.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        //T3 都走正常上portFlow
        public void DPCassetteMapDownloadByDPI(Equipment eqp, Port port, IList<Job> slotData)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Line line;

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqp.Data.NODENO);
                    throw new Exception(err);
                }

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null)
                {
                    err = string.Format("CAN'T FIND LINE_INFORMATION=[{0}] IN LINEENTITY!", eqp.Data.LINEID);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT IN EQUIPMENT=[{0}]!", eqp.Data.NODENO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].", eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                #region Data and Status Check

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Map Download\" COMMAND!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Map Download\" COMMAND!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA &&
                    port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Map Download\" COMMAND!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                #endregion
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.MapDownload;

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", port.Data.NODENO, port.Data.PORTNO);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                string slotLog = string.Empty;
                StringBuilder slotExist = new StringBuilder(new string('0', port.Data.MAXCOUNT));

                outputData.EventGroups[0].IsMergeEvent = true;  //Watson Add 20150130 先組成寫入的Raw Data再一次性寫入，可以減少很多時間。

                foreach (Job j in slotData)
                {
                    if (!string.IsNullOrEmpty(slotLog)) slotLog += "\r\n";
                    slotLog += string.Format("\t\t\tCREATE JOB DATA: CST_SEQNO=[{0}] JOB_SEQNO=[{1}] GROUP_INDEX=[{2}] PRODUCT_TYPE=[{3}] GlassChipMaskBlockID=[{4}] SAMPLING_SLOG_FLAG=[{5}];",
                        j.CassetteSequenceNo, j.JobSequenceNo, j.GroupIndex, j.ProductType.Value, j.GlassChipMaskBlockID, j.SamplingSlotFlag);
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[0].Value = j.CassetteSequenceNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[1].Value = j.JobSequenceNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[2].Value = j.GroupIndex;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[3].Value = j.ProductType.Value.ToString();
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[4].Value = ((int)j.CSTOperationMode).ToString();
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[5].Value = ((int)j.SubstrateType).ToString();
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[6].Value = ((int)j.CIMMode).ToString();
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[7].Value = ((int)j.JobType).ToString();
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[8].Value = j.JobJudge;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[9].Value = j.SamplingSlotFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[10].Value = j.OXRInformationRequestFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[11].Value = j.FirstRunFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[12].Value = j.JobGrade;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[13].Value = j.GlassChipMaskBlockID;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[14].Value = j.PPID;
                    slotExist.Replace('0', '1', int.Parse(j.JobSequenceNo) - 1, 1);
                }

                outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.MapDownload).ToString();
                if (slotData.Count() > 0)
                {
                    outputData.EventGroups[1].Events[0].Items[1].Value = slotExist.ToString();  // slotData[0].MesCstBody.SELECTEDPOSITIONMAP;
                }
                //Jun Add 20150205 For Cell Loader Cassette Setting Code
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[3].Value = cst.LDCassetteSettingCode;

                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();

                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();

                string log = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] PORT_TYPE=[{4}] CSTID=[{5}] \"Cassette Map Download\" COMMAND JOB_EXISTENCE=[{6}] JOB_COUNT=[0]{7} CST_SETTINGCODE=[{8}], SET BIT=[ON].\r\n",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.Type, port.File.CassetteID, slotExist, string.Empty, cst.LDCassetteSettingCode);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", log + slotLog);

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

                object[] _obj = new object[]
                {
                    eqp,
                    port,
                    slotData,
                    outputData
                };

                Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_DPI", _obj);

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.MapDownload.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void MES_BOXPROCESSENDBYDPI(Line line, Equipment eqp, Port port, Port port2, IList<Job>jobs,IList<Job>jobs2,string trxID)
        {
            try
            {
                if (line.Data.LINETYPE != eLineType.CELL.CBDPI)
                    return;
                string denseBoxID1 = port.File.CassetteID;
                string denseBoxID2 = ObjectManager.PortManager.GetPortByDPI(eqp.Data.NODENO, port.Data.PORTNO).File.CassetteID;
                object[] obj3 = new object[]
                {
                    trxID,
                    port, 
                    port2,
                    jobs,
                    jobs2,
                    new List<string> {denseBoxID1,denseBoxID2}
                };
                Invoke(eServiceName.MESService, "BoxProcessEndByDPI", obj3);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public Trx DPI_READPLCDdata(string trxkey)
        {
             if (GetServerAgent(eAgentName.PLCAgent).ConnectedState == eAGENT_STATE.DISCONNECTED)
                {
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "PLC Connectd State is Disconnected");
                    return null;
                }

            //取得PLC资料
             Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxkey, false }) as Trx;
             return trx;
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
