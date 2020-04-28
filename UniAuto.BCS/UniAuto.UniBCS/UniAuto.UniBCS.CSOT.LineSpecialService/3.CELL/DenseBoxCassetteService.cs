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
        private const string DPCstStsChangeTimeout = "DPCassetteStsChangeTimeout";
        private const string DPCstControlCommandTimeout = "DPCstControlCommandTimeout";

        public override bool Init()
        {
            return true;
        }

        #region [Cassette Status Change] ================================================

        /// <summary>
        /// 更新記億體資料
        /// </summary>
        /// <param name="inputData">同CassetteStatusChangeReport(Trx inputData)</param>
        /// <param name="log">記錄動作者</param>
        public void DPCassetteStatusChangeReportUpdate(Trx inputData, string sourceMethod)
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
                #endregion

                string log;
                RefreshPortStatus_CELL(line, port, inputData,out log);

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}]{3}.", eqp.Data.NODENO, sourceMethod, inputData.TrackKey, log));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteStatusChangeReport(Trx inputData)
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

                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_INFORMATION=[{0}] IN LINEENTITY!", ServerName));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                if (inputData.IsInitTrigger)
                {
                    DPCassetteStatusChangeReportUpdate(inputData, "DPCassetteStatusChangeReport_Initial");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF] PORT=[{2}] CSTID=[{3}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, port.Data.PORTNO, port.File.CassetteID));
                    DPCassetteStatusChangeReportReply(eqpNo, portNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                DPCassetteStatusChangeReportReply(eqpNo, portNo, eBitResult.ON, inputData.TrackKey);

                string log = string.Empty;
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    RefreshPortStatus_CELL(line, port, inputData, out log);
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}]{3}",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode.ToString(), log));

                Cassette cst = null;

                HandleCstStatus(inputData.TrackKey, line, eqp, port, inputData, out cst);

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

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

        private void DPCassetteStatusChangeReportReply(string eqpNo, string portNo, eBitResult value, string trackKey)
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}], SET BIT=[{3}].",
                    eqpNo, trackKey, portNo, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void DPCassetteStatusChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] CASSETTE STATUS CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));

                DPCassetteStatusChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void HandleCstStatus(string trxID, Line line, Equipment eqp, Port port, Trx inputData, out Cassette cst)
        {
            string err = string.Empty;
            cst = null;
            IList<Job> jobs = new List<Job>();
            string reasonCode;
            string reasonText;
            try
            {
                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));  

                switch (port.File.CassetteStatus)
                {
                    case eCassetteStatus.NO_CASSETTE_EXIST:
                        {
                            //Nothing
                        }
                        break;
                    case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                        {
                            Cassette oldcst = null;
                            if (port.File.Status != ePortStatus.LC)
                            {
                                err = string.Format("EQUIPMENT=[{0}] PORTNO#=[{1}], PORT_STATUS=[{2}] AND CASSETTE_STATUS=[{3}] IS INVALID.",
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
                                LoadTime = DateTime.Now,
                            };
                            ObjectManager.CassetteManager.CreateBox(cst); //Watson modify 20150130
                            #endregion

                            if (line.File.HostMode == eHostMode.OFFLINE)
                            {
                                if (oldcst != null)
                                {
                                    if (oldcst.Jobs.Count() > 0)
                                    {
                                        ObjectManager.JobManager.DeleteJobs(oldcst.Jobs);

                                        // 使用此資料再重塞入到CST Data Map給EQ
                                        for (int i = 0; i < oldcst.Jobs.Count(); i++)
                                        {
                                            Job job = cst.Jobs[i];
                                            job.CassetteSequenceNo = port.File.CassetteSequenceNo;
                                            job.JobSequenceNo = job.ToSlotNo;
                                            job.SourcePortID = port.Data.PORTID;
                                            job.TargetPortID = "0";
                                            job.FromCstID = port.File.CassetteID.Trim();
                                            job.TargetCSTID = string.Empty;
                                            job.FromSlotNo = job.ToSlotNo;
                                            job.ToSlotNo = "0";
                                            job.JobJudge = "0";
                                            job.FirstRunFlag = "0";
                                            job.LastGlassFlag = "0";
                                            job.SamplingSlotFlag = "1";
                                            job.TrackingData = "0";
                                            job.InspJudgedData = "0";
                                            job.ArraySpecial.SourcePortNo = job.CfSpecial.SourcePortNo = port.Data.PORTNO;
                                            job.ArraySpecial.TargetPortNo = job.CfSpecial.TargetPortNo = "0";
                                            job.DefectCodes.Clear();
                                            job.HoldInforList.Clear();
                                            jobs.Add(job);
                                        }
                                        ObjectManager.JobManager.AddJobs(jobs);
                                        DPCassetteMapDownload(eqp, port, jobs);
                                        ObjectManager.CassetteManager.DeleteBox(cst.CassetteID); //Watson modify 20150130
                                    }
                                    return;
                                }
                                lock (port) port.File.OPI_SubCstState = eOPISubCstState.WACSTEDIT;
                            }
                            else
                            {
                                #region 上報給MES Data
                                object[] _data = new object[2]
                                    {
                                        inputData.TrackKey,          /*0 TrackKey*/
                                        port
                                    };
                                // MES Data
                                if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                                {
                                        Invoke(eServiceName.MESService, "ValidateBoxRequest", _data);
                                }
                                else
                                {
                                    Invoke(eServiceName.MESService, "ValidateCassetteRequest", _data);
                                }
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
                                    err = string.Format("CAN'T FIND CASSETTE INFORMATION, CST_ID=[{0}] IN CASSETTE OBJECT!", port.File.CassetteID);
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
                                ObjectManager.PortManager.EnqueueSave(port.File);
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
                                err = string.Format("CAN'T FIND CASSETTE INFORMATION, CST_ID=[{0}] IN CASSETTE OBJECT!", port.File.CassetteID);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { inputData.TrackKey, eqp.Data.LINEID, "[DPCassetteStatusChangeReport] " + err });
                                throw new Exception(err);
                            }
                            else
                            {
                                lock (cst) cst.StartTime = DateTime.Now;
                            }
                        }
                        break;
                    case eCassetteStatus.IN_PROCESSING:
                        {
                            if (line.File.HostMode == eHostMode.REMOTE)
                            {
                                string timeoutName = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);
                                if (_timerManager.IsAliveTimer(timeoutName))
                                {
                                    _timerManager.TerminateTimer(timeoutName);
                                }
                            }
                            //Watson Add 20141217 For CELL Virtual Port no First Glass Check
                            if ((port.File.Type == ePortType.UnloadingPort) || (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.VirtualPort) || 
                                (cst.FirstGlassCheckReport == "N"))  //Jun Add 20150404 Dense機台不會報First Glass Check Event
                            {
                                object[] _data = new object[3]
                                { 
                                    inputData.TrackKey,               /*0  TrackKey*/
                                    port,                             /*1  Port*/
                                    cst,                              /*2  Cassette*/
                                };

                                //呼叫MES方法
                                if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                                {
                                    Invoke(eServiceName.MESService, "BoxProcessStarted", _data);
                                }
                                else
                                {
                                    Invoke(eServiceName.MESService, "LotProcessStarted", _data);
                                    Invoke(eServiceName.APCService, "LotProcessStart", _data);
                                }
                            }
                            //Watson Add 20150102 For Box Remove
                            cst.CellBoxProcessed = eboxReport.Processing;
                        }
                        break;
                    case eCassetteStatus.PROCESS_PAUSED:
                        {
                            // Nothing
                        }
                        break;
                    case eCassetteStatus.PROCESS_COMPLETED:
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
                                err = string.Format("CAN'T FIND CASSETTE INFORMATION, CST_ID=[{0}] IN CASSETTE OBJECT!", port.File.CassetteID);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { inputData.TrackKey, eqp.Data.LINEID, "[DPCassetteStatusChangeReport] " + err });
                                throw new Exception(err);
                            }
                            else
                            {
                                lock (cst) cst.EndTime = DateTime.Now;
                            }

                            GetPortJobData(inputData, port, ref jobs);

                            if (line.File.HostMode != eHostMode.OFFLINE)
                            {
                                #region Report MES
                                switch (port.File.CompletedCassetteData)
                                {
                                    case eCompletedCassetteData.NormalComplete:
                                        {
                                            #region [Report to MES LotProcessEnd or BoxProcessEnd]
                                            // MES Data
                                            object[] obj = new object[]
                                            {
                                                inputData.TrackKey,
                                                line.Data.LINEID,
                                                port.Data.PORTID,
                                                cst,
                                                jobs
                                            };
                                            if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                                            {
                                                Invoke(eServiceName.MESService, "BoxProcessEnd", obj);
                                            }
                                            else
                                            {
                                                Invoke(eServiceName.MESService, "LotProcessEnd", obj);
                                                Invoke(eServiceName.APCService, "LotProcessEnd", obj);
                                            }
                                            #endregion
                                            //Watson Add 20150102 For Box Remove
                                            cst.CellBoxProcessed = eboxReport.NOReport;
                                        }
                                        break;
                                    case eCompletedCassetteData.BCForcedToAbort:
                                    case eCompletedCassetteData.EQAutoAbort:
                                    case eCompletedCassetteData.OperatorForcedToAbort:
                                        {
                                            GetReasonCodeAndText(port, cst, out reasonCode, out reasonText);
                                            #region [Report to MES BoxProcessEnd]
                                            object[] obj = new object[]
                                            {
                                            inputData.TrackKey,
                                            line.Data.LINEID,
                                            port.Data.PORTID,
                                            cst,
                                            jobs
                                            };
                                            if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                                            {
                                               Invoke(eServiceName.MESService, "BoxProcessEnd", obj);
                                            }
                                            else
                                            {
                                                Invoke(eServiceName.MESService, "LotProcessEnd", obj);
                                                Invoke(eServiceName.APCService, "LotProcessEnd", obj);
                                            }
                                            #endregion
                                            //Watson Add 20150102 For Box Remove
                                            cst.CellBoxProcessed = eboxReport.NOReport;
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
                                            if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                                                Invoke(eServiceName.MESService, "BoxProcessCanceled", obj);
                                            else
                                                Invoke(eServiceName.MESService, "LotProcessCanceled", obj);
                                            #endregion
                                            //Watson Add 20150102 For Box Remove
                                            cst.CellBoxProcessed = eboxReport.NOReport;
                                        }
                                        break;
                                }
                                #endregion
                            }

                            if (!jobs.Count().Equals(0))
                            {
                                ObjectManager.JobManager.DeleteJobs(jobs);
                                  //Watson Add 20150416 For Save History.
                                ObjectManager.JobManager.RecordJobsHistory(jobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Delete_CST_Complete.ToString(), inputData.TrackKey);  
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

        private void RefreshPortStatus_CELL(Line line, Port port, Trx inputData, out string log)
        {
            log = string.Format(" PORT=[{0}]", port.Data.PORTNO);

            try
            {
                Event evt = inputData.EventGroups[1].Events[0];
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

                    port.File.CassetteSetCode = inputData.EventGroups[1].Events[0].Items[11].Value;
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
                        log += string.Format(" AUTO_CLAVE_BYPASS=[{0}])({1})", evt.Items[13].Value, port.File.AutoClaveByPass.ToString());

                        port.File.MappingGrade = inputData.EventGroups[1].Events[0].Items[14].Value;
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

        public void GetPortJobData(Trx inputData, Port port, ref IList<Job> jobs)
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
                        //    string.Format("=[{0}]Can't find job information, Cassette Sequence No=[{1}], Job Sequence No=[{2}].",
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

        private void GetReasonCodeAndText(Port port, Cassette cst, out string reasonCode, out string reasonText)
        {
            reasonCode = reasonText = string.Empty;

            if (cst != null)
            {
                if (!string.IsNullOrEmpty(cst.ReasonCode))
                {
                    reasonCode = cst.ReasonCode;
                    reasonText = cst.ReasonText;
                    // 將資料有問題的更正
                    if (port.File.Type == ePortType.UnloadingPort)
                    {
                        reasonCode = reasonCode.Substring(0, 1).Equals("U") ? reasonCode : "U" + reasonCode.Substring(1);
                    }
                    else
                    {
                        reasonCode = reasonCode.Substring(0, 1).Equals("L") ? reasonCode : "L" + reasonCode.Substring(1);
                    }
                    return;
                }
            }

            switch (port.File.CompletedCassetteData)
            {
                case eCompletedCassetteData.EQAutoCancel:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_EQ_Cancel :
                        MES_ReasonCode.Loader_EQ_Cancel;
                    //TODO: 等CSOT整理好對應內容再填
                    reasonText = "";
                    break;
                case eCompletedCassetteData.BCForcedToCancel:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_BC_Cancel_From_BC_Client :
                        MES_ReasonCode.Loader_BC_Cancel_From_BC_Client;
                    //TODO: 等CSOT整理好對應內容再填
                    reasonText = "";
                    break;
                case eCompletedCassetteData.OperatorForcedToCancel:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_OP_Cancel :
                        MES_ReasonCode.Loader_OP_Cancel;
                    //TODO: 等CSOT整理好對應內容再填
                    reasonText = "";
                    break;
                case eCompletedCassetteData.EQAutoAbort:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_EQ_Abort :
                        MES_ReasonCode.Loader_EQ_Abort;
                    //TODO: 等CSOT整理好對應內容再填
                    reasonText = "";
                    break;
                case eCompletedCassetteData.BCForcedToAbort:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_BC_Abort :
                        MES_ReasonCode.Loader_BC_Abort;
                    //TODO: 等CSOT整理好對應內容再填
                    reasonText = "";
                    break;
                case eCompletedCassetteData.OperatorForcedToAbort:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_OP_Abort :
                        MES_ReasonCode.Unloader_OP_Abort;
                    //TODO: 等CSOT整理好對應內容再填
                    reasonText = "";
                    break;
            }
        }

        private void RecordCassetteHistory(string trxID, Equipment eqp, Port port, Cassette cst, string cstControlCmd, string cmdRetcode, string operID)
        {
            try
            {

                // Save DB
                CASSETTEHISTORY his = new CASSETTEHISTORY()
                {
                    UPDATETIME = DateTime.Now,
                    CASSETTEID = port.File.CassetteID,
                    CASSETTESEQNO = int.Parse(port.File.CassetteSequenceNo),
                    CASSETTESTATUS = port.File.CassetteStatus.ToString(),
                    NODEID = eqp.Data.NODEID,
                    JOBCOUNT = int.Parse(port.File.JobCountInCassette),
                    PORTID = port.Data.PORTID,
                    JOBEXISTENCE = port.File.JobExistenceSlot,
                    CASSETTECONTROLCOMMAND = cstControlCmd,
                    COMMANDRETURNCODE = cmdRetcode,
                    OPERATORID = operID,
                    COMPLETEDCASSETTEDATA = port.File.CompletedCassetteData.ToString(),
                    LOADINGCASSETTETYPE = port.File.LoadingCassetteType.ToString(),
                    QTIMEFLAG = (int)port.File.QTimeFlag,
                    PARTIALFULLFLAG = (int)port.File.PartialFullFlag,
                    CASSETTESETCODE = port.File.CassetteSetCode,
                    TRANSACTIONID = trxID
                };

                if (cst != null)
                {
                    his.LOADTIME = cst.LoadTime;
                    his.PROCESSSTARTTIME = cst.StartTime;
                    his.PROCESSENDTIME = cst.EndTime;
                }
                ObjectManager.CassetteManager.InsertDB(his);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Cassette Control Command]
        //TODO: 還沒寫完//T3 都走正常上portFlow
        public void DPCassetteProcessStart(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);

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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] CSTID=[{4}] \"Cassette Process Start\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessStart.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessStart_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].",
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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Start\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessStart.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessStartByCount(string eqpNo, string portNo, int count)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqpNo, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Start By Count\" command JOB_EXISTENCE=[{5}] JOB_COUNT=[{6}], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT), count));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessStartByCount.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessStartByCount_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].",
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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Start By Count\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[{6}], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT), msg.BODY.PROCESSCOUNT));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessStartByCount.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessPause(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] CSTID=[{4}] \"Cassette Process Pause\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessPause.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessPause_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].",
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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Pause\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessPause.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessResume(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND &&
                    port.File.CassetteStatus != eCassetteStatus.PROCESS_PAUSED)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] CSTID=[{4}] \"Cassette Process Resume\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessResume.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessResume_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].",
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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND &&
                    port.File.CassetteStatus != eCassetteStatus.PROCESS_PAUSED)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Resume\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessResume.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessAbort(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] CSTID=[{4}] \"Cassette Process Abort\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessAbort.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessAbort_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].",
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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Abort\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessAbort.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessCancel(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);

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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Cancel\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessCancel.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteProcessCancel_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

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
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    //throw new Exception(err); modify for t2,t3 sync 2016/04/14 cc.kuang
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Cancel\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessCancel.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //public void CassetteReload_UI(CassetteCommandRequest msg)
        //{
        //    string err = string.Empty;
        //    try
        //    {
        //        #region [取得EQP及Port資訊]
        //        Line line; Equipment eqp; Port port;

        //        eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

        //        if (eqp == null)
        //        {
        //            err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", msg.BODY.EQUIPMENTNO);

        //            //TODO: 打訊息到OPI
        //            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
        //            return;
        //        }

        //        line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
        //        if (line == null)
        //        {
        //            err = string.Format("Can't find Line Information=[{0}] in LineEntity!", eqp.Data.LINEID);

        //            //TODO: 打訊息到OPI
        //            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
        //            return;
        //        }

        //        port = ObjectManager.PortManager.GetPort(msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

        //        if (port == null)
        //        {
        //            err = string.Format("Can't find Port No=[{0}] in Equipment No=[{2}]!", msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

        //            //TODO: 打訊息到OPI
        //            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
        //            return;
        //        }

        //        Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

        //        if (cst == null)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }
        //        #endregion

        //        #region Data and Status Check
        //        // 對MES 為Offline才可下此Command
        //        if (line.File.HostMode != eHostMode.OFFLINE)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] Host Mode =[{1}] DP#{2}, can not download \"Cassette Reload\" command!",
        //                eqp.Data.NODENO, line.File.HostMode.ToString(), msg.BODY.PORTNO);
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }
        //        // CIM MODE OFF 不能改
        //        if (eqp.File.CIMMode == eBitResult.OFF)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Reload\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO);
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }

        //        if (port.File.Status != ePortStatus.UR)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Reload\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO, port.File.Status.ToString());
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }

        //        if (port.File.CassetteStatus != eCassetteStatus.PROCESS_COMPLETED)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Reload\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }

        //        #endregion

        //        string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
        //        Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
        //        outputData.ClearTrxWith0();

        //        outputData.EventGroups[0].IsDisable = true;
        //        outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.Reload).ToString();
        //        outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
        //        outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
        //        SendPLCData(outputData);
        //        cst.CassetteControlCommand = eCstControlCmd.Reload;
        //        ObjectManager.CassetteManager.EnqueueSave(cst);

        //        string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, DPCstControlCommandTimeout);
        //        if (_timerManager.IsAliveTimer(timeoutName))
        //        {
        //            _timerManager.TerminateTimer(timeoutName);
        //        }

        //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
        //            new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.Reload).ToString());

        //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //            string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Reload\" command, Set Bit [ON]",
        //            eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

        //        RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.Reload.ToString(), string.Empty, msg.BODY.OPERATORID);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        //public void CassetteLoad_UI(CassetteCommandRequest msg)
        //{
        //    string err = string.Empty;
        //    try
        //    {
        //        #region [取得EQP及Port資訊]
        //        Line line; Equipment eqp; Port port;

        //        eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

        //        if (eqp == null)
        //        {
        //            err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", msg.BODY.EQUIPMENTNO);

        //            //TODO: 打訊息到OPI
        //            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
        //            return;
        //        }

        //        line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
        //        if (line == null)
        //        {
        //            err = string.Format("Can't find Line Information=[{0}] in LineEntity!", eqp.Data.LINEID);

        //            //TODO: 打訊息到OPI
        //            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
        //            return;
        //        }

        //        port = ObjectManager.PortManager.GetPort(msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

        //        if (port == null)
        //        {
        //            err = string.Format("Can't find Port No=[{0}] in Equipment No=[{2}]!", msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

        //            //TODO: 打訊息到OPI
        //            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
        //            return;
        //        }
        //        #endregion

        //        #region Data and Status Check
        //        // 對MES 為Offline才可下此Command
        //        if (line.File.HostMode != eHostMode.OFFLINE)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] Host Mode =[{1}] DP#{2}, can not download \"Cassette Load\" command!",
        //                eqp.Data.NODENO, line.File.HostMode.ToString(), msg.BODY.PORTNO);
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }
        //        // CIM MODE OFF 不能改
        //        if (eqp.File.CIMMode == eBitResult.OFF)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Load\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO);
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }

        //        if (port.File.Status != ePortStatus.UC)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Load\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO, port.File.Status.ToString());
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }

        //        if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Load\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }

        //        #endregion

        //        string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
        //        Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
        //        outputData.ClearTrxWith0();

        //        outputData.EventGroups[0].IsDisable = true;
        //        outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.Load).ToString();
        //        outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
        //        outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
        //        SendPLCData(outputData);

        //        string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, DPCstControlCommandTimeout);
        //        if (_timerManager.IsAliveTimer(timeoutName))
        //        {
        //            _timerManager.TerminateTimer(timeoutName);
        //        }

        //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
        //            new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.Load).ToString());

        //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //            string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Load\" command, Set Bit [ON]",
        //            eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

        //        RecordCassetteHistory(outputData.TrackKey, eqp, port, null, eCstControlCmd.Load.ToString(), string.Empty, msg.BODY.OPERATORID);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        //public void CassetteReMap_UI(CassetteCommandRequest msg)
        //{
        //    string err = string.Empty;
        //    try
        //    {
        //        #region [取得EQP及Port資訊]
        //        Line line; Equipment eqp; Port port;

        //        eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

        //        if (eqp == null)
        //        {
        //            err = string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", msg.BODY.EQUIPMENTNO);

        //            //TODO: 打訊息到OPI
        //            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
        //            return;
        //        }

        //        line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
        //        if (line == null)
        //        {
        //            err = string.Format("Can't find Line Information=[{0}] in LineEntity!", eqp.Data.LINEID);

        //            //TODO: 打訊息到OPI
        //            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
        //            return;
        //        }

        //        port = ObjectManager.PortManager.GetPort(msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

        //        if (port == null)
        //        {
        //            err = string.Format("Can't find Port No=[{0}] in Equipment No=[{2}]!", msg.BODY.EQUIPMENTNO, msg.BODY.PORTNO);

        //            //TODO: 打訊息到OPI
        //            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
        //            return;
        //        }

        //        Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

        //        if (cst == null)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] DP#{1} Can't find Cassette Data from Cassette Sequence No=[{2}]. Can not download \"Cassette Process Start By Count\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }
        //        #endregion

        //        #region Data and Status Check
        //        // 對MES 為Remote不能下此Command
        //        if (line.File.HostMode == eHostMode.REMOTE)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] Host Mode =[{1}] DP#{2}, can not download \"Cassette Re-Map\" command!",
        //                eqp.Data.NODENO, line.File.HostMode.ToString(), msg.BODY.PORTNO);
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }
        //        // CIM MODE OFF 不能改
        //        if (eqp.File.CIMMode == eBitResult.OFF)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] CIM Mode [OFF] DP#{1}, can not download \"Cassette Re-Map\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO);
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }

        //        // Both Port才能下此Command
        //        if (port.File.Type != ePortType.BothPort)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Type =[{2}] is not Both Port, can not download \"Cassette Re-Map\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO, port.File.Type.ToString());
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }

        //        if (port.File.Status != ePortStatus.LC)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] DP#{1} Port Status =[{2}], can not download \"Cassette Re-Map\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO, port.File.Status.ToString());
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }

        //        if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING)
        //        {
        //            err = string.Format("EQUIPMENT=[{0}] DP#{1} Cassette Status =[{2}], can not download \"Cassette Re-Map\" command!",
        //                eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteStatus.ToString());
        //            //TODO: 打訊息到OPI
        //            throw new Exception(err);
        //        }
        //        #endregion

        //        string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
        //        Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
        //        outputData.ClearTrxWith0();

        //        outputData.EventGroups[0].IsDisable = true;
        //        outputData.EventGroups[1].Events[0].Items[0].Value = ((int)eCstControlCmd.ReMap).ToString();
        //        outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
        //        outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
        //        SendPLCData(outputData);
        //        cst.CassetteControlCommand = eCstControlCmd.ReMap;
        //        ObjectManager.CassetteManager.EnqueueSave(cst);

        //        string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, DPCstControlCommandTimeout);
        //        if (_timerManager.IsAliveTimer(timeoutName))
        //        {
        //            _timerManager.TerminateTimer(timeoutName);
        //        }

        //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
        //            new System.Timers.ElapsedEventHandler(DPCassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ReMap).ToString());

        //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //            string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] CIM Mode =[{2}], \"Cassette Re-Map\" command, Set Bit [ON]",
        //            eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));

        //        RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ReMap.ToString(), string.Empty, msg.BODY.OPERATORID);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        //T3 都走正常上portFlow
        public void DPCassetteMapDownload(Equipment eqp, Port port, IList<Job> slotData)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Line line;

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
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

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM BOX_ID=[{2}].", eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

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
                    //outputData.EventGroups[1].Events[0].Items[2].Value = slotData.Count().ToString();   //Watson Add 20150324 For JobCount
                }
                outputData.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();

                string log = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] PORT_TYPE=[{4}] CSTID=[{5}] \"Cassette Map Download\" COMMAND JOB_EXISTENCE=[{6}] JOB_COUNT=[0]{7} CST_SETTINGCODE=[{8}], SET BIT=[ON].\r\n",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.Type, port.File.CassetteID, slotExist, string.Empty, cst.LDCassetteSettingCode);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", log + slotLog);

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

                switch (fabType)
                {
                    case eFabType.ARRAY:

                        break;
                    case eFabType.CF:
                        
                        break;
                    case eFabType.CELL:
                        {
                            //Jun Add 20150205 For Cell Loader Cassette Setting Code
                            outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[3].Value = cst.LDCassetteSettingCode;
                            object[] _obj = new object[]
                            {
                                eqp,
                                port,
                                slotData,
                                outputData
                            };
                            switch (line.Data.JOBDATALINETYPE)
                            {
                                #region T2 USE
                                case eJobDataLineType.CELL.CBPIL: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_PIL", _obj); break;
                                case eJobDataLineType.CELL.CCODF: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_ODF", _obj); break;
                                case eJobDataLineType.CELL.CBHVA: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_HVA", _obj); break;
                                case eJobDataLineType.CELL.CBCUT: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_CUT", _obj); break;
                                case eJobDataLineType.CELL.CBPOL: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_POL", _obj); break;
                                case eJobDataLineType.CELL.CBDPK: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_DPK", _obj); break;
                                case eJobDataLineType.CELL.CBPPK: break;
                                case eJobDataLineType.CELL.CBPMT: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_PMT", _obj); break;
                                case eJobDataLineType.CELL.CBGAP: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_GAP", _obj); break;
                                case eJobDataLineType.CELL.CBPIS: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_PIS", _obj); break;
                                case eJobDataLineType.CELL.CBPRM: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_PRM", _obj); break;
                                case eJobDataLineType.CELL.CBGMO: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_GMO", _obj); break;
                                case eJobDataLineType.CELL.CBLOI: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_LOI", _obj); break;
                                case eJobDataLineType.CELL.CBNRP: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_NRP", _obj); break;
                                case eJobDataLineType.CELL.CBOLS: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_OLS", _obj); break;
                                case eJobDataLineType.CELL.CBSOR: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_SOR", _obj); break;
                                case eJobDataLineType.CELL.CBDPS: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_DPS", _obj); break;
                                case eJobDataLineType.CELL.CBATS: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_ATS", _obj); break;
                                case eJobDataLineType.CELL.CBDPI: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_DPI", _obj); break;
                                case eJobDataLineType.CELL.CBUVA: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_UVA", _obj); break;
                                #endregion
                                #region T3 USE
                                //case eJobDataLineType.CELL.CCPIL: Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_PIL", _obj); break;
                                default:
                                    Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_CELL", _obj); break;
                                #endregion
                            }
                        }
                        break;
                }

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.MapDownload.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        //T3 都走正常上portFlow
        public void DPCassetteControlCommandReply(Trx inputData)
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

                string err = string.Empty;
                string replyMsg = string.Empty;
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY! SKIP RECORD CASSETTE HISTORY.", inputData.Metadata.NodeNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, "[BOXControlCommandReply] " + err });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqp.Data.NODENO, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT_NO=[{0}] IN EQUIPMENT=[{1}]! SKIP RECORD CASSETTE HISTORY", eqp.Data.NODENO, portNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, "[BOXControlCommandReply] " + err });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //Jun Modify 20150126 Box會移動, 到每個Port的時候Cassette Sequence No都會不一樣, 所以使用Cassette ID取Cassette
                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());  //(int.Parse(port.File.CassetteSequenceNo));
                replyMsg = MethodBase.GetCurrentMethod().Name;
                string methodName = MethodBase.GetCurrentMethod().Name + "()";

                if (cst != null)
                {
                    
                    if (cst.CassetteControlCommand != eCstControlCmd.None)
                    {
                        methodName = string.Format("Cassette{0}Reply()", cst.CassetteControlCommand.ToString());
                    }
                    if (cst.CassetteControlCommand == eCstControlCmd.MapDownload)
                    {
                        if (port.Data.LINEID.Contains(eLineType.CELL.CBDPI))
                        {
                            Dictionary<Port, int> ports = new Dictionary<Port, int>();
                            ports.Add(port, (int)retCode);
                            Port port2 = ObjectManager.PortManager.GetPortByDPI(eqp.Data.NODENO, portNo);
                            if (port == null)
                            {
                                err = string.Format("DPI LINE CAN'T FIND PORT_NO=[{0}] IN EQUIPMENT=[{1}]!", eqp.Data.NODENO, portNo);

                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, "[BOXControlCommandReply] " + err });
                                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                return;
                            }
                            ports.Add(port2, (int)retCode);
                            if (portNo == "01")
                                Invoke(eServiceName.UIService, "CassetteMapDownloadResultReport_DPI", new object[] { ports });
                        }
                        else
                            Invoke(eServiceName.UIService, "CassetteMapDownloadResultReport", new object[] { port, (int)retCode });
                    }
                }


                Logger.LogInfoWrite(this.LogName, this.GetType().Name, methodName,
                 string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] PORT=[{3}] CSTID=[{4}] RETURNCODE=[{5}]({6}).",
                 inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), portNo, port.File.CassetteID, (int)retCode, retCode.ToString()));

                if (triggerBit == eBitResult.OFF) return;


                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, portNo, DPCstControlCommandTimeout);

                UserTimer timer = null;
                if (_timerManager.IsAliveTimer(timeName))
                {
                    timer = _timerManager.GetAliveTimer(timeName);
                    _timerManager.TerminateTimer(timeName);
                }

                if (timer != null)
                {
                    string[] obj = timer.State.ToString().Split('_');
                    eCstControlCmd cmd = (eCstControlCmd)int.Parse(obj[1]);
                    if (cmd != eCstControlCmd.None)
                        replyMsg = string.Format("Cassette{0}Reply", cmd.ToString());

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, 
                        string.Format("{0} - EQUIPMENT=[{1}] PORT=[{2}] RETURN CODE=[{3}]({4})", 
                        replyMsg, eqp.Data.NODENO, port.Data.PORTNO, (int)retCode, retCode) });

                    // 2015.2.5 依照CSOT 登京 , 在Cst Map Downlad NG時要Cancel CST
                    if (cmd == eCstControlCmd.MapDownload)
                    {
                        switch (retCode)
                        {
                            case eCstCmdRetCode.ALREADY_RECEIVED:
                            case eCstCmdRetCode.COMMAND_OK:
                            case eCstCmdRetCode.COMMAND_ERROR: break;
                            default:
                                if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                                {
                                    DPCassetteProcessCancel(eqp.Data.NODENO, port.Data.PORTNO);
                                }
                                break;
                        }
                    }
                }

                string trxName = string.Format("{0}_DP#{1}CassetteControlCommand", inputData.Metadata.NodeNo, portNo);

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].IsDisable = true;
                outputdata.EventGroups[1].Events[0].IsDisable = true;
                outputdata.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

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

        private void DPCassetteControlCommandTimeout(object subjet, System.Timers.ElapsedEventArgs e)
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
                Port port = ObjectManager.PortManager.GetPort(sArray[1]);  //Watson modify 20150206

                if (cmd == eCstControlCmd.MapDownload)
                {
                    if (port != null)
                    {
                        if (port.Data.LINEID.Contains(eLineType.CELL.CBDPI))
                        {
                            Dictionary<Port, int> ports = new Dictionary<Port, int>();
                            ports.Add(port, 99);
                            Port port2 = ObjectManager.PortManager.GetPortByDPI(sArray[0], sArray[1]);
                            if (port == null)
                            {
                                string err = string.Format("DPI LINE CAN'T FIND PORT_NO=[{0}] IN EQUIPMENT=[{1}]!", sArray[0], sArray[1]);

                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { UtilityMethod.GetAgentTrackKey(), ServerName, "[BOXControlCommandReply] " + err });
                                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                                return;
                            }
                            ports.Add(port2, 99);
                            Invoke(eServiceName.UIService, "CassetteMapDownloadResultReport_DPI", new object[] { ports });
                        }
                        else
                            Invoke(eServiceName.UIService, "CassetteMapDownloadResultReport", new object[] { port, 99 });
                    }
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, DP#=[{2}] Cassette Control Command=[{3}] Reply Timeout Set Bit [OFF].",
                    sArray[0], obj[0], sArray[1], cmd.ToString()));

                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { obj[0], ServerName, 
                        string.Format("Cassette{0}Reply - EQUIPMENT=[{1}] PORT=[{2}] \"T1 TIMEOUT\"", 
                        cmd, sArray[0], sArray[1])});
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
  
    }
}
