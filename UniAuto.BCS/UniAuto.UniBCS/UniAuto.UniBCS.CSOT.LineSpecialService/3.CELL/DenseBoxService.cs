using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UniAuto.UniBCS.Log;
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

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
    public partial class DenseBoxService : AbstractService
    {
        enum eSourceLine
        {
            UNKNOW = 0,
            DPK = 1,
            DPS = 2,
        }

        public override bool Init()
        {
            return true;
        }
        #region [DenseBoxLabelInformationRequest]
        private const string DenseBoxInfoTimeout = "DenseBoxInfoTimeout";
        /// <summary>
        /// TimeID : eqp.Data.NodeID + "_DenseBoxInfoTimeout" : "L4_DenseBoxInfoTimeout"
        /// </summary>
        /// <param name="inputData"></param>
        public void DenseBoxLabelInformationRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                    DenseBoxLabelInformationRequestReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, eReturnCode1.NG, string.Empty, string.Empty, string.Empty, 0);
                    return;
                }


                #region [拆出PLCAgent Data]  Word
                string denseBoxCount = inputData.EventGroups[0].Events[0].Items[0].Value;
                string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[1].Value;
                string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[2].Value;
                string Weight = inputData.EventGroups[0].Events[0].Items[3].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,Dense Box Label Information Request Count  =[{4}] ,BoxID1 =[{5}] ,BoxID2 =[{6}] ,Weight =[{7}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, denseBoxCount, denseBoxID1, denseBoxID2, Weight));

                #region Save History
                //RecordJobHistory(job, eJobEvent.Sent);
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, denseBoxCount, denseBoxID1,
                    denseBoxID2, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", Weight, line.File.HostMode.ToString());
                #endregion
                #region MES Data Dense box Label Info Request
                object[] _data = new object[5]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODENO,
                    eqp.Data.NODEID,
                    denseBoxID1               /*2 boxID */
                };
                //Send MES Data
                object retVal = base.Invoke(eServiceName.MESService, "BoxLabelInformationRequest", _data);

                #endregion



            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    DenseBoxLabelInformationRequestReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey, eReturnCode1.NG, string.Empty, string.Empty, string.Empty, 0);
            }
        }
        /// <summary>
        /// TimeID : eqp.Data.NodeID + "_DenseBoxInfoTimeout" : "L4_DenseBoxInfoTimeout"
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <param name="value"></param>
        /// <param name="trackKey"></param>
        /// <param name="returnCode"></param>
        /// <param name="denseBoxID"></param>
        /// <param name="modelName"></param>
        /// <param name="modelVersion"></param>
        /// <param name="caseID"></param>
        /// <param name="qty"></param>
        /// <param name="weekCode"></param>
        public void DenseBoxLabelInformationRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 returnCode, string denseBoxID, string modelName, string modelVersion, int qty)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_DenseBoxLabelInformationRequestReply") as Trx;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                }
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)returnCode).ToString();
                outputdata.EventGroups[0].Events[0].Items[1].Value = denseBoxID;
                outputdata.EventGroups[0].Events[0].Items[2].Value = modelName;
                outputdata.EventGroups[0].Events[0].Items[3].Value = modelVersion;
                //outputdata.EventGroups[0].Events[0].Items[4].Value = caseID;
                outputdata.EventGroups[0].Events[0].Items[4].Value = qty.ToString();
                //outputdata.EventGroups[0].Events[0].Items[6].Value = weekCode; //Modify for T3 MES SPEC

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();

                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxInfoTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxInfoTimeout);
                }
                if (value == eBitResult.ON)
                {
                    _timerManager.CreateTimer(eqpNo + "_" + DenseBoxInfoTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxLabelInformationRequestReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,Dense Box Label Information Request Reply Set Bit =[{2}].",
                    eqpNo, trackKey, value.ToString()));
                #region Save History
                //RecordJobHistory(job, eJobEvent.Sent);
                RecordPPKEventHistory(outputdata.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "", denseBoxID,
                    "", "", "", "", "", "", "", "", returnCode.ToString(), "", "", "", "", "", "", "", "", "", line.File.HostMode.ToString());
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void DenseBoxLabelInformationRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, Dense Box Label Information Request Reply Timeout Set Bit =[OFF].", sArray[0], trackKey));

                DenseBoxLabelInformationRequestReply(sArray[0], eBitResult.OFF, trackKey, eReturnCode1.NG, string.Empty, string.Empty, string.Empty, 0);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [RemoveDenseBoxReport]
        private const string RemoveDenseBoxTimeout = "RemoveDenseBoxTimeout";
        /// <summary>
        /// TimeID : eqp.Data.NodeID + "_RemoveDenseBoxTimeout" : "L4_RemoveDenseBoxTimeout"
        /// </summary>
        /// <param name="inputData"></param>
        public void RemoveDenseBoxReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    RemoveDenseBoxReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);

                    return;
                }

                //if (_timerManager.IsAliveTimer(eqp.Data.NODENO + "_" + RemoveDenseBoxTimeout))
                //{
                //    _timerManager.TerminateTimer(eqp.Data.NODENO + "_" + RemoveDenseBoxTimeout);
                //}

                //_timerManager.CreateTimer(eqp.Data.NODENO + "_" + RemoveDenseBoxTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(RemoveDenseBoxReportReplyTimeout), inputData.TrackKey);

                #region [拆出PLCAgent Data]  Word
                string denseBoxCount = inputData.EventGroups[0].Events[0].Items[0].Value;
                string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                string stageorPortorPallet = inputData.EventGroups[0].Events[0].Items[3].Value;
                string stageNo = inputData.EventGroups[0].Events[0].Items[4].Value.PadLeft(2, '0');
                string portNo = inputData.EventGroups[0].Events[0].Items[5].Value.PadLeft(2, '0');
                string palletNo = inputData.EventGroups[0].Events[0].Items[6].Value.PadLeft(2, '0');
                string removeReasonFlag = inputData.EventGroups[0].Events[0].Items[7].Value;//"1：Weight NG Box   ,2：MES Return NG ,3：Normal Remove"

                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,RemoveDenseBoxReportBlock Count =[{4}] ,BoxID1 =[{5}] ,BoxID2 =[{6}] ,StageorPortorPallet =[{7}],stageNo =[{8}] ,portNo =[{9}] ,palletNo =[{10}],removeReasonFlag =[{11}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, denseBoxCount, denseBoxID1, denseBoxID2,
                    stageorPortorPallet, stageNo, portNo, palletNo, removeReasonFlag));

                RemoveDenseBoxReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                Port port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, inputData.Metadata.NodeNo, portNo);
                string PortID = string.Empty;
                if (port != null)
                {
                    PortID = port.Data.PORTID;
                }

                //Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line != null)
                {
                    switch (line.Data.LINETYPE)
                    {
                        case eLineType.CELL.CCQPP:
                            if (stageorPortorPallet == "2" && portNo != "00")
                            {
                                #region Update Port Object
                                lock (port)
                                {
                                    port.File.PortBoxID1 = string.Empty;
                                    port.File.PortBoxID2 = string.Empty;
                                }
                                Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                                #endregion
                            }

                            List<string> boxidList = new List<string>();
                            if (denseBoxID1 != string.Empty) boxidList.Add(denseBoxID1);
                            if (denseBoxID2 != string.Empty) boxidList.Add(denseBoxID2);
                            List<Cassette> boxList = new List<Cassette>();
                            #region [Report to MES  BoxProcessCanceled]
                            // MES Data
                            Invoke(eServiceName.MESService, "BoxProcessCanceled_QPP", new object[] {inputData.TrackKey,line.Data.LINEID,PortID, boxidList,
                            removeReasonFlag,removeReasonFlag=="1"? "Weight NG Box":  removeReasonFlag=="2" ?"MES Return NG" :"Normal Remove"});
                            #endregion
                            Cassette cst1 = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                            if (cst1 != null) ObjectManager.CassetteManager.DeleteBox(denseBoxID1);
                            Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                            if (cst2 != null) ObjectManager.CassetteManager.DeleteBox(denseBoxID2);

                            break;
                        case eLineType.CELL.CBPPK:
                            //TODO : PPK Line Remove 需要上報BoxProcessCancel
                            //如果沒有報過就需要幫機台上報BoxProcessAbort or BoxProcessCancel
                            if (stageorPortorPallet == "2" && portNo != "00")
                            {
                                #region Update Port Object
                                lock (port)
                                {
                                    port.File.PortBoxID1 = string.Empty;
                                    port.File.PortBoxID2 = string.Empty;
                                }
                                Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                                #endregion
                            }

                            if (denseBoxID1 != string.Empty)
                            {
                                #region [Report to MES  BoxProcessCanceled]
                                // MES Data
                                Invoke(eServiceName.MESService, "BoxProcessCanceled", new object[] {inputData.TrackKey,line.Data.LINEID,PortID, denseBoxID1,
                                    removeReasonFlag,removeReasonFlag=="1"? "Weight NG Box":  removeReasonFlag=="2" ?"MES Return NG" :"Normal Remove"});
                                #endregion
                            }
                            if (denseBoxID2 != string.Empty)
                            {
                                #region [Report to MES  BoxProcessCanceled]
                                // MES Data
                                //string trxID, string lineID,string portID,string boxID, string reasonCode, string reasonText)
                                Invoke(eServiceName.MESService, "BoxProcessCanceled", new object[] {inputData.TrackKey,line.Data.LINEID,PortID, denseBoxID2,
                                    removeReasonFlag,removeReasonFlag=="1"? "Weight NG Box":  removeReasonFlag=="2" ?"MES Return NG" :"Normal Remove"});
                                #endregion
                            }

                            break;
                        case eLineType.CELL.CBDPI:
                            DenseBoxCassetteService db = new DenseBoxCassetteService();
                            IList<Job> jobs = new List<Job>();
                            Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { "L2_DP#03CassetteStatusChangeReport", false }) as Trx;
                            db.GetPortJobData(trx, port, ref jobs);
                            Port port2 = ObjectManager.PortManager.GetPortByDPI(eqp.Data.NODENO, portNo);
                            IList<Job> jobs2 = new List<Job>();
                            trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { "L2_DP#04CassetteStatusChangeReport", false }) as Trx;
                            db.GetPortJobData(trx, port2, ref jobs2);
                            if (port2.File.CassetteStatus < eCassetteStatus.IN_PROCESSING)
                                port2.File.DPISampligFlag = eBitResult.OFF;
                            Invoke(eServiceName.MESService, "BoxProcessEndByDPIRemove", new object[] {
                                        inputData.TrackKey,port,port2,jobs, jobs2,new List<string> {denseBoxID1,denseBoxID2} });
                            ObjectManager.JobManager.DeleteJobs(jobs);
                            break;
                        default:
                            Cassette cst = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                            if (cst == null) break;
                            //TODO : DP Port 在做Remove前，BC需要檢查是否DP已Process Complete，
                            if (cst.CellBoxProcessed == eboxReport.NOProcess)
                            {
                                #region [Report to MES  BoxProcessCanceled]
                                object[] obj2 = new object[]
                                {
                                inputData.TrackKey,
                                line.Data.LINEID,cst.PortID,denseBoxID1, removeReasonFlag,
                                removeReasonFlag=="1"? "Weight NG Box":  removeReasonFlag=="2" ?"MES Return NG" :"Normal Remove"
                                };
                                Invoke(eServiceName.MESService, "BoxProcessCanceled", obj2);
                                #endregion
                            }

                            if (cst.CellBoxProcessed == eboxReport.Processing)
                            {
                                #region [Report to MES  BoxProcessEnd]
                                object[] obj2 = new object[]
                                {
                                inputData.TrackKey,
                                line.Data.LINEID,
                                PortID,
                                cst,
                                FindInBoxJobs(denseBoxID1)
                                };
                                Invoke(eServiceName.MESService, "BoxProcessEnd", obj2);
                                #endregion
                            }
                            break;
                    }
                }

                if (line.Data.LINETYPE == eLineType.CELL.CBPPK || line.Data.LINETYPE == eLineType.CELL.CCQPP)
                {
                    ObjectManager.CassetteManager.DeleteBox(denseBoxID1);
                    ObjectManager.CassetteManager.DeleteBox(denseBoxID2);
                }
                Pallet pallet = ObjectManager.PalletManager.GetPalletByNo(palletNo);
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, denseBoxCount, denseBoxID1,
                    denseBoxID2, "", stageorPortorPallet, stageNo, portNo, pallet==null?"":pallet.File.PalletID,
                    palletNo, "", "", "", "", "", "", "", "", "", "", removeReasonFlag, line.File.HostMode.ToString());

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    RemoveDenseBoxReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
            }
        }
        /// <summary>
        /// TimeID : eqp.Data.NodeID + "_RemoveDenseBoxTimeout" : "L4_RemoveDenseBoxTimeout"
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <param name="value"></param>
        /// <param name="trackKey"></param>
        private void RemoveDenseBoxReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_RemoveDenseBoxReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();

                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + RemoveDenseBoxTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + RemoveDenseBoxTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + RemoveDenseBoxTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(RemoveDenseBoxReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,RemovedDenseBoxReportReply Set Bit =[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void RemoveDenseBoxReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, RemovedDenseBoxReportReplyTimeout Set Bit =[OFF].", sArray[0], trackKey));

                RemoveDenseBoxReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [FetchOutDenseBoxReport]
        private const string FetchOutDenseBoxTimeout = "FetchOutDenseBoxTimeout";
        /// <summary>
        /// TimeID : eqp.Data.NodeID + "_FetchedOutDenseBoxTimeout" : "L4_FetchedOutDenseBoxTimeout"
        /// </summary>
        /// <param name="inputData"></param>
        public void FetchOutDenseBoxReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    FetchOutDenseBoxReportReply(eqp.Data.NODENO, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                //shihyang add palletID 20151109
                #region [拆出PLCAgent Data]  Word
                string denseBoxCount = inputData.EventGroups[0].Events[0].Items[0].Value;
                string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[1].Value;
                string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[2].Value;
                string stageorPortorPallet = inputData.EventGroups[0].Events[0].Items[3].Value;
                string stageNo = inputData.EventGroups[0].Events[0].Items[4].Value.PadLeft(2, '0');
                string portNo = inputData.EventGroups[0].Events[0].Items[5].Value.PadLeft(2, '0');
                string palletID = inputData.EventGroups[0].Events[0].Items[6].Value;
                string palletNo = inputData.EventGroups[0].Events[0].Items[7].Value.PadLeft(2, '0');
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,FetchDenseBoxReport Count =[{4}] ,BoxID1 =[{5}] ,BoxID2 =[{6}] ,StageorPortorPallet =[{7}],stageNo =[{8}] ,portNo =[{9}] ,palletID =[{10}],palletNo =[{11}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, denseBoxCount, denseBoxID1, denseBoxID2,
                    stageorPortorPallet, stageNo, portNo, palletID, palletNo));

                FetchOutDenseBoxReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey);

                //Jun Add 20141229
                //Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                List<Cassette> boxList = new List<Cassette>();
                if (line != null)
                {
                    #region [CCQPP]
                    if (line.Data.LINETYPE == eLineType.CELL.CCQPP)
                    {      //20151202 MES Test No need                 
                        #region [ProductOut]
                        //if (portNo.PadLeft(2, '0') != "00")
                        //{
                        //    Cassette cst1 = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                        //    if (cst1 != null)
                        //    {
                        //        cst1.IsProcessed = true;
                        //        boxList.Add(cst1);
                        //    }
                        //    Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                        //    if (cst2 != null)
                        //    {
                        //        cst2.IsProcessed = true;
                        //        boxList.Add(cst2);
                        //    }
                        //    foreach (Cassette Box in boxList)
                        //    {
                        //        //Job job = NewJob();
                        //        string currentEQPID = eqp.Data.NODEID;
                        //        string unitID = "";
                        //        Invoke(eServiceName.MESService, "ProductOutForPallet", new object[7] { inputData.TrackKey, eqp.Data.LINEID, Box, currentEQPID, portNo, unitID, eMESTraceLevel.P });
                        //    }
                        //}
                        #endregion
                        #region [CCQPP]
                        if (line.Data.LINETYPE == eLineType.CELL.CCQPP)
                        {
                            if (stageorPortorPallet == "2" && portNo.PadLeft(2, '0') != "00")
                            {
                                //2016101 sy modify 羅俊&工程確認 將觸發點調整至Fetch
                                //List<Cassette> boxList = new List<Cassette>();
                                //bool reportMes = true;
                                Cassette cst1 = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                                if (cst1 != null)
                                {
                                    cst1.IsProcessed = true;
                                    boxList.Add(cst1);
                                }
                                else if (!string.IsNullOrEmpty(denseBoxID1))
                                {
                                    cst1 = new Cassette();
                                    cst1.CassetteID = denseBoxID1.Trim();
                                    cst1.IsProcessed = true;
                                    boxList.Add(cst1);
                                }
                                Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                                if (cst2 != null)
                                {
                                    cst2.IsProcessed = true;
                                    boxList.Add(cst2);
                                }
                                else if (!string.IsNullOrEmpty(denseBoxID2))
                                {
                                    cst2 = new Cassette();
                                    cst2.CassetteID = denseBoxID2.Trim();
                                    cst2.IsProcessed = true;
                                    boxList.Add(cst2);
                                }
                                //if (reportMes)
                                    Invoke(eServiceName.MESService, "BoxProcessStarted_PPK", new object[] { inputData.TrackKey, line, boxList });
                            }
                            //20151202 MES Test No need
                            #region [ProductIn]
                            //if (portNo.PadLeft(2, '0') != "00")
                            //{
                            //    List<Cassette> boxList = new List<Cassette>();

                            //    Cassette cst1 = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                            //    if (cst1 != null)
                            //    {
                            //        cst1.IsProcessed = true;
                            //        boxList.Add(cst1);
                            //    }
                            //    Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                            //    if (cst2 != null)
                            //    {
                            //        cst2.IsProcessed = true;
                            //        boxList.Add(cst2);
                            //    }
                            //    foreach (Cassette Box in boxList)
                            //    {
                            //        //Job job = NewJob();
                            //        string processtime = "";
                            //        string currentEQPID = eqp.Data.NODEID;
                            //        string unitID = "";
                            //        Invoke(eServiceName.MESService, "ProductInForPallet", new object[8] { inputData.TrackKey, eqp.Data.LINEID, Box, currentEQPID, portNo, unitID, eMESTraceLevel.P, processtime });
                            //    }
                            //}
                            #endregion
                        }
                        #endregion
                    }
                    #endregion
                    #region [CBPPK]
                    if (line.Data.LINETYPE == eLineType.CELL.CBPPK)
                    {
                        if (stageorPortorPallet == "2" && portNo != "00")
                        {
                            //Jun Modify 20150406 因為從Port Fetch出來有二種可能，一種是進Stage，一種是進Car，所以不能在這時候上報BoxProcessStart
                            //Cassette cst1 = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                            //if (cst1 != null) boxList.Add(cst1);
                            //Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                            //if (cst2 != null) boxList.Add(cst2);

                            //Invoke(eServiceName.MESService, "BoxProcessStarted_PPK", new object[] { inputData.TrackKey, line, boxList });

                            #region Update Port Object
                            Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2, '0'));
                            if (port != null)
                            {
                                lock (port)
                                {
                                    port.File.PortBoxID1 = string.Empty;
                                    port.File.PortBoxID2 = string.Empty;
                                }
                            }
                            Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                            #endregion
                        }
                    }
                    #endregion
                    #region [CBDPI]
                    if (line.Data.LINETYPE == eLineType.CELL.CBDPI)
                    {
                        if (stageorPortorPallet == "2" && portNo != "00")
                        {
                            Port port = ObjectManager.PortManager.GetDPPort(eqp.Data.NODENO, portNo);
                            if (port != null)
                            {
                                if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)
                                {
                                    Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                                    if (cst2 != null)
                                        Invoke(eServiceName.MESService, "BoxProcessStarted", new object[] { inputData.TrackKey, port, cst2 });
                                    else
                                        throw new Exception(string.Format("Can't find Box ID =[{0}] in CassetteEntity!", denseBoxID2));
                                }
                                else
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                   string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,FetchedOutDenseBoxReport Report BoxID2 =[{4}],CASSETTE STATUS[{5}] Already Report BOXPROCESSSTART!!",
                                   eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, denseBoxID2, port.File.CassetteStatus.ToString()));
                            }
                        }
                    #endregion
                        else if (stageorPortorPallet == "2" && portNo == "00")
                            throw new Exception(string.Format("Can't find Port No =[{0}] in PortEntity!", portNo));
                    }
                }
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, denseBoxCount, denseBoxID1,
                    denseBoxID2, "", stageorPortorPallet, stageNo, portNo, palletID,
                    palletNo, "", "", "", "", "", "", "", "", "", "", "", line.File.HostMode.ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// TimeID : eqp.Data.NodeID + "_FetchedOutDenseBoxTimeout" : "L4_FetchedOutDenseBoxTimeout"
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <param name="value"></param>
        /// <param name="trackKey"></param>
        private void FetchOutDenseBoxReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_FetchOutDenseBoxReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();

                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + FetchOutDenseBoxTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + FetchOutDenseBoxTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + FetchOutDenseBoxTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(FetchOutDenseBoxReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,FetchOutDenseBoxReportReply Set Bit =[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void FetchOutDenseBoxReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, FetchedOutDenseBoxReportReplyTimeout Set Bit =[OFF].", sArray[0], trackKey));

                FetchOutDenseBoxReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [StoreDenseBoxReport]
        private const string StoreOutDenseBoxTimeout = "StoreOutDenseBoxTimeout";
        /// <summary>
        /// /// TimeID : eqp.Data.NodeID + "_StoreOutDenseBoxTimeout" : "L4_StoreOutDenseBoxTimeout"
        /// </summary>
        /// <param name="inputData"></param>
        public void StoreDenseBoxReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    StoreDenseBoxReportReply(eqp.Data.NODENO, eBitResult.OFF, inputData.TrackKey);
                    return;
                }
                //shihyang add palletID 20151109
                #region [拆出PLCAgent Data]  Word
                string denseBoxCount = inputData.EventGroups[0].Events[0].Items[0].Value;
                string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[1].Value;
                string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[2].Value;
                string stageorPortorPallet = inputData.EventGroups[0].Events[0].Items[3].Value;
                string stageNo = inputData.EventGroups[0].Events[0].Items[4].Value.PadLeft(2, '0');
                string portNo = inputData.EventGroups[0].Events[0].Items[5].Value.PadLeft(2, '0');
                string palletID = inputData.EventGroups[0].Events[0].Items[6].Value;
                string palletNo = inputData.EventGroups[0].Events[0].Items[7].Value.PadLeft(2, '0');
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,StoreDenseBoxReport Count =[{4}] ,BoxID1 =[{5}] ,BoxID2 =[{6}] ,StageorPortorPallet =[{7}],stageNo =[{8}] ,portNo =[{9}] ,palletID =[{10}],palletNo =[{11}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, denseBoxCount, denseBoxID1, denseBoxID2,
                    stageorPortorPallet, stageNo, portNo, palletID, palletNo));

                StoreDenseBoxReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey);
                #region [CCQPP]
                if (line.Data.LINETYPE == eLineType.CELL.CCQPP)
                {
                    if (stageorPortorPallet == "2" && portNo.PadLeft(2, '0') != "00")
                    {
                        #region Update Port Object
                        Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo.PadLeft(2, '0'));
                        if (port != null)
                        {
                            lock (port)
                            {
                                port.File.PortBoxID1 = denseBoxID1;
                                port.File.PortBoxID2 = denseBoxID2;
                            }
                        }
                        Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                        #endregion

                        //2016101 sy modify 羅俊&工程確認 將觸發點調整至Fetch
                        //List<Cassette> boxList = new List<Cassette>();
                        //bool reportMes = true;
                        //Cassette cst1 = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                        //if (cst1 != null)
                        //{
                        //    cst1.IsProcessed = true;
                        //    boxList.Add(cst1);
                        //    if (cst1.Empty) reportMes = false;
                        //}
                        //Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                        //if (cst2 != null)
                        //{
                        //    cst2.IsProcessed = true;
                        //    boxList.Add(cst2);
                        //    if (cst2.Empty) reportMes = false;
                        //}
                        //if (reportMes)
                        //    Invoke(eServiceName.MESService, "BoxProcessStarted_PPK", new object[] { inputData.TrackKey, line, boxList });
                    }
                    //20151202 MES Test No need
                    #region [ProductIn]
                    //if (portNo.PadLeft(2, '0') != "00")
                    //{
                    //    List<Cassette> boxList = new List<Cassette>();

                    //    Cassette cst1 = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                    //    if (cst1 != null)
                    //    {
                    //        cst1.IsProcessed = true;
                    //        boxList.Add(cst1);
                    //    }
                    //    Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                    //    if (cst2 != null)
                    //    {
                    //        cst2.IsProcessed = true;
                    //        boxList.Add(cst2);
                    //    }
                    //    foreach (Cassette Box in boxList)
                    //    {
                    //        //Job job = NewJob();
                    //        string processtime = "";
                    //        string currentEQPID = eqp.Data.NODEID;
                    //        string unitID = "";
                    //        Invoke(eServiceName.MESService, "ProductInForPallet", new object[8] { inputData.TrackKey, eqp.Data.LINEID, Box, currentEQPID, portNo, unitID, eMESTraceLevel.P, processtime });
                    //    }
                    //}
                    #endregion
                }
                #endregion
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, denseBoxCount, denseBoxID1,
                    denseBoxID2, "", stageorPortorPallet, stageNo, portNo, palletID,
                    palletNo, "", "", "", "", "", "", "", "", "", "", "", line.File.HostMode.ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        /// <summary>
        /// TimeID : eqp.Data.NodeID + "_StoreOutDenseBoxTimeout" : "L4_StoreOutDenseBoxTimeout"
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <param name="value"></param>
        /// <param name="trackKey"></param>
        private void StoreDenseBoxReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_StoreDenseBoxReportReply") as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();

                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + StoreOutDenseBoxTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + StoreOutDenseBoxTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + StoreOutDenseBoxTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(StoreDenseBoxReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,FetchedOutDenseBoxReportReply Set Bit =[{2}].",
                    eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void StoreDenseBoxReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, StoreDenseBoxReportReplyTimeout Set Bit =[OFF].", sArray[0], trackKey));

                StoreDenseBoxReportReply(sArray[0], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [DenseBoxDataRequest]
        private const string DenseBoxDataReqTimeout = "DenseBoxDataReqTimeout";
        public void DenseBoxDataRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                #region [拆出PLCAgent Data]  Word
                string portNo = inputData.EventGroups[0].Events[0].Items[0].Value.PadLeft(2, '0');
                string denseBoxCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[3].Value.Trim();
                string packmode = inputData.EventGroups[0].Events[0].Items[4].Value.Trim();
                string emptyFlag = inputData.EventGroups[0].Events[0].Items[5].Value.Trim();
                #endregion

                Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);
                if (port == null) throw new Exception(string.Format("Can't find Port No =[{0}] in PortEntity!", portNo));
                lock (port)
                {
                    port.File.PortDBDataRequest = ((int)bitResult).ToString();
                    port.File.Mes_ValidateBoxReply = string.Empty;
                    port.File.PortBoxID1 = denseBoxID1;
                    port.File.PortBoxID2 = denseBoxID2;
                    port.File.Empty = emptyFlag == "1" ? true : false;
                }

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    DenseBoxDataRequestReply(eqp.Data.NODENO, eBitResult.OFF, inputData.TrackKey, null);

                    Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                    return;
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,DenseBoxDataRequest CarNo =[{4}] ,DenseBoxCount  =[{5}] ,DenseBoxID##001 =[{6}] ,DenseBoxID#002 =[{7}],PackUnpackMod  =[{8}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, portNo, denseBoxCount, denseBoxID1, denseBoxID2, packmode));

                List<string> boxlist = new List<string>();
                if (denseBoxID1 != string.Empty)
                    boxlist.Add(denseBoxID1);
                if (denseBoxID2 != string.Empty)
                    boxlist.Add(denseBoxID2);

                //Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line != null)
                {
                    if (line.File.HostMode != eHostMode.OFFLINE)
                    {
                        #region MES Data ValidateBoxRequest
                        object[] _data = new object[3]
                        { 
                            inputData.TrackKey,  /*0 TrackKey*/
                            port,                          /*2 PortID*/
                            boxlist
                        };
                        #region Add Reply Key
                        //MES Reply no Mechine Name (PLC Write Key),BC Add Repository 自行處理加入倉庫
                        string key = keyBoxReplyPLCKey.DenseBoxDataRequestReply;
                        string rep = eqp.Data.NODENO;
                        if (Repository.ContainsKey(key))
                            Repository.Remove(key);
                        Repository.Add(key, rep);
                        #endregion
                        object retVal = base.Invoke(eServiceName.MESService, "ValidateBoxRequest", _data);
                        #endregion
                    }
                    else
                    {
                        if (emptyFlag == "1")
                        {
                            List<string> replylist = new List<string>();
                            replylist.Add("0");//OK
                            for (int i = 0; i < 11 - 1; i++) { replylist.Add("0"); }
                            DenseBoxDataRequestReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey, replylist);
                        }
                        else
                        Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                    }
                }
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, denseBoxCount, denseBoxID1,
                    denseBoxID2, "", "2", "", portNo, "", "", "", "", "", "", "", "", "", "", "", "", "", line.File.HostMode.ToString());

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    DenseBoxDataRequestReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey, null);
            }
        }
        public void DenseBoxDataRequestReply(string eqpNo, eBitResult value, string trackKey, List<string> replylist)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "DenseBoxDataRequestReply") as Trx;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxDataReqTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxDataReqTimeout);
                    }
                    return;
                }

                if (replylist.Count < 11) throw new Exception(string.Format("Reply Item Count not enough in DenseBoxDataRequestReply!", replylist.Count.ToString()));
                if (replylist[0].Trim() == "0")
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eReturnCode1.OK).ToString();
                else
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eReturnCode1.NG).ToString();// returncode;  //ReturnCode

                outputdata.EventGroups[0].Events[0].Items[1].Value = replylist[1];//portno;  //PortNo
                outputdata.EventGroups[0].Events[0].Items[2].Value = replylist[2];//boxid1;  //DenseBox#01
                outputdata.EventGroups[0].Events[0].Items[3].Value = replylist[3];//boxid2;  //DenseBox#02
                outputdata.EventGroups[0].Events[0].Items[4].Value = replylist[4];//producttype;  //ProductType
                outputdata.EventGroups[0].Events[0].Items[5].Value = replylist[5];//grade1;  //Grade#01
                outputdata.EventGroups[0].Events[0].Items[6].Value = replylist[6];//grade2;  //Grade#02
                outputdata.EventGroups[0].Events[0].Items[7].Value = replylist[7];//cstsetcode1;  //CassetteSettingCode#01
                outputdata.EventGroups[0].Events[0].Items[8].Value = replylist[8];//cstsetcode2; //CassetteSettingCode#02
                outputdata.EventGroups[0].Events[0].Items[9].Value = replylist[9];//boxqty1;  //BoxGlassCount#01
                outputdata.EventGroups[0].Events[0].Items[10].Value = replylist[10];//boxqty2;  //BoxGlassCount#02

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxDataReqTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxDataReqTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + DenseBoxDataReqTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxDataRequestReplyTimeout), trackKey);
                   
                    int boxCount = 0;
                    if (replylist[2] != string.Empty) boxCount++;
                    if (replylist[3] != string.Empty) boxCount++;
                    RecordPPKEventHistory(outputdata.TrackKey, MethodBase.GetCurrentMethod().Name, eqp,  boxCount.ToString(), replylist[2],
                        replylist[3], "", "", "", replylist[1], "","", "", replylist[0].Trim() == "0"?eReturnCode1.OK.ToString():eReturnCode1.NG.ToString(),
                        replylist[4], replylist[5], replylist[6],replylist[7], replylist[8], replylist[9], replylist[10], "", "",line.File.HostMode.ToString());
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,DenseBoxDataRequestReply Set Bit =[{2}].",
                    eqpNo, trackKey, value.ToString()));

                Port port = ObjectManager.PortManager.GetPort(replylist[1]);
                if (port != null)
                    lock (port) port.File.Mes_ValidateBoxReply = string.Empty;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void DenseBoxDataRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, DenseBoxDataRequest Timeout Set Bit [OFF].", sArray[0], trackKey));

                DenseBoxDataRequestReply(sArray[0], eBitResult.OFF, trackKey, null);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [CassetteSettingCodeChangeReport]
        private const string CstSetCodeReplyTimeout = "CstSetCodeReplyTimeout";
        public void CassetteSettingCodeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                #region [拆出PLCAgent Data] Bit
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion

                int intstart = inputData.Metadata.Name.IndexOf('#') + 1;
                if (intstart.Equals(0)) throw new Exception(string.Format("Can't find Trx Name=[{0}]!", inputData.Metadata.Name));
                string portno = inputData.Metadata.Name.Substring(intstart, 2);

                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    CassetteSettingCodeChangeReportReply(eqp.Data.NODENO, portno, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [拆出PLCAgent Data]  Word
                string setCode = inputData.EventGroups[0].Events[0].Items[11].Value;
                #endregion


                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,CassetteSettingCodeChangeReport Setting Code =[{4}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, setCode));


                #region Update Port

                Port port = ObjectManager.PortManager.GetDPPort(eqp.Data.NODEID, portno);
                if (port == null) throw new Exception(string.Format("Can't find Trx Name Port No =[{0}] in PortEntity!", inputData.Metadata.Name));
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
                #endregion

                //一種：
                //_Port#03CassetteSettingCodeChangeReport
                //_Port#03CassetteSettingCodeChangeReportReply
                //二種：
                //_DP#01CassetteSettingCodeChangeReport
                //_DP#01CassetteSettingCodeChangeReportReply
                CassetteSettingCodeChangeReportReply(eqp.Data.NODENO, portno, eBitResult.ON, inputData.TrackKey);                
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    string portno = inputData.Metadata.Name.Substring(inputData.Metadata.Name.IndexOf('#') + 1, 2);
                    CassetteSettingCodeChangeReportReply(inputData.Name.Split('_')[0], portno, eBitResult.ON, inputData.TrackKey);
                }
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
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "DP#" + portno + "CassetteSettingCodeChangeReportReply") as Trx;

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
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,PortNo=[{2}] CassetteSettingCodeChangeReportReply Set Bit =[{3}].",
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
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, PortNo=[{2}] CassetteSettingCodeChangeReportReply Timeout Set Bit [OFF].", sArray[0], trackKey, sArray[1]));

                CassetteSettingCodeChangeReportReply(sArray[0], sArray[1], eBitResult.OFF, trackKey);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        
        #endregion
        #region [DenseBoxIDCheckReport]
        private const string DenseBoxIDCheckReplyTimeout = "DenseBoxIDCheckReplyTimeout";
        public void DenseBoxIDCheckReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                #endregion
                #region [拆出PLCAgent Data]  Word and Bit
                #region [PLC Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    DenseBoxIDCheckReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey,null,null);
                    return;
                }
                #endregion
                #region [PLC Word]
                string boxcount = inputData.EventGroups[0].Events[0].Items[0].Value;
                string id1 = inputData.EventGroups[0].Events[0].Items[1].Value;   //CSOT 20141106 福杰say: 一次只能上一個
                string id2 = inputData.EventGroups[0].Events[0].Items[2].Value;   //CSOT 20141106 福杰say: 一次只能上一個

                string portNo = inputData.EventGroups[0].Events[0].Items[3].Value; 
                #endregion
                #endregion

                #region Add Reply Key
                //MES Reply no Mechine Name (PLC Write Key),BC Add Repository 自行處理加入倉庫
                string key = keyBoxReplyPLCKey.DenseBoxIDCheckReportReply;
                string rep = eqp.Data.NODENO;
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion

                if (line.Data.LINETYPE == eLineType.CELL.CCQPP)
                {
                    if (line.File.HostMode == eHostMode.OFFLINE)
                    {
                        DenseBoxIDCheckReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey, "1", portNo);
                    }
                    else
                    {
                        string keyPortNo = keyBoxReplyPLCKey.DenseBoxIDCheckReportReply+"_PortNo";
                        string repportNo = portNo;
                        if (Repository.ContainsKey(keyPortNo))
                            Repository.Remove(keyPortNo);
                        Repository.Add(keyPortNo, repportNo);
                        Invoke(eServiceName.MESService, "CheckPairBoxRequest", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, id1.Trim(), id2.Trim() });
                    }                    
                }
                else
                {
                    #region [T2]
                    if (ObjectManager.LineManager.GetLine(ServerName).Data.LINEID.Contains(eLineType.CELL.CBPPK))
                    {
                        string portno = inputData.EventGroups[0].Events[0].Items[3].Value;
                        string packMode = inputData.EventGroups[0].Events[0].Items[4].Value;
                        string sourceline = inputData.EventGroups[0].Events[0].Items[5].Value;

                        eSourceLine eSourceLine;
                        Enum.TryParse<eSourceLine>(sourceline, out eSourceLine);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode=[{2}], Node=[{3}] ,DenseBoxIDCheckReport Dense Box Count =[{4}]" +
                                        "Dense Box ID#01	=[{5}] ,Dense Box ID#02, =[{6}],Port No =[{7}],Pack/Unpack Mode =[{8}],Source Line Type =[{9}]",
                                                eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, boxcount, id1, id2, portno, packMode, sourceline));

                        Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portno.PadLeft(2, '0'));
                        if (port == null) throw new Exception(string.Format("Can't find Port No =[{0}] in PortEntity!", portno.PadLeft(2, '0')));

                        #region MES Data BoxProcessLineRequest
                        Invoke(eServiceName.MESService, "BoxProcessLineRequest", new object[] { inputData.TrackKey, eqp.Data.LINEID, eSourceLine.ToString(), 
                                            id1.Trim(), port.Data.PORTID });
                        #endregion

                        #region Update Port Object
                        lock (port)
                        {
                            port.File.PortUnPackSource = sourceline;
                        }
                        Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                        #endregion
                    }
                    else
                    {
                        string weight = inputData.EventGroups[0].Events[0].Items[3].Value;

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode=[{2}], Node=[{3}] ,DenseBoxIDCheckReport Dense Box Count =[{4}]" +
                                        "Dense Box ID#01	=[{5}] ,Dense Box ID#02, =[{6}],Weight =[{7}]",
                                                eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, boxcount, id1, id2, weight));

                        //Watson Add 20150429 For DPS Line MES 是不會回MES_BoxProcessLineReply的，所以bc直接回覆OK給機台
                        if (eqp.Data.LINEID.Contains(eLineType.CELL.CBDPS))
                        {
                            DenseBoxIDCheckReportReply(eqp.Data.NODENO, eBitResult.ON, inputData.TrackKey, "1", "");
                        }
                        else
                        {
                            #region MES Data BoxProcessLineRequest
                            Invoke(eServiceName.MESService, "BoxProcessLineRequest", new object[] { inputData.TrackKey, eqp.Data.LINEID, "", id1.Trim(), "" });
                            #endregion
                        }
                    }
                    #endregion
                }



            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    DenseBoxIDCheckReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey, "2", null);
            }
        }
        public void DenseBoxIDCheckReportReply(string eqpNo, eBitResult value, string trackKey,string rtncode,string portNo)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "DenseBoxIDCheckReportReply") as Trx;

                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                if (rtncode == null)
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                else
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = rtncode;
                    //if (ObjectManager.LineManager.GetLine(ServerName).Data.LINEID.Contains(eLineType.CELL.CBPPK))
                    //    outputdata.EventGroups[0].Events[0].Items[1].Value = portNo;
                    if (ObjectManager.LineManager.GetLine(ServerName).Data.LINEID.Contains(eLineType.CELL.CCQPP))
                        outputdata.EventGroups[0].Events[0].Items[1].Value = portNo;
                }
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxIDCheckReplyTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxIDCheckReplyTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + DenseBoxIDCheckReplyTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxIDCheckReportReplyTimeout), trackKey);
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,DenseBoxIDCheckReportReply Set Bit =[{2}]."
                    , eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void DenseBoxIDCheckReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, DenseBoxIDCheckReportReply Set Bit =[OFF].", sArray[0], trackKey));

                DenseBoxIDCheckReportReply(sArray[0], eBitResult.OFF, trackKey,null,null);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [DenseBoxWeightCheckReport]
        private const string DenseBoxWeightCheckReplyTimeout = "DenseBoxWeightCheckReplyTimeout";
        public void DenseBoxWeightCheckReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                //[拆出PLCAgent Data]  Word and Bit
                #region [PLC Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    DenseBoxWeightCheckReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, null,null);
                    return;
                }
                #endregion
                #region [PLC Word]
                string boxcount = inputData.EventGroups[0].Events[0].Items[0].Value;
                string id1 = inputData.EventGroups[0].Events[0].Items[1].Value;
                string id2 = inputData.EventGroups[0].Events[0].Items[2].Value;
                string weight = inputData.EventGroups[0].Events[0].Items[3].Value;
                string portno = inputData.EventGroups[0].Events[0].Items[4].Value;
                #endregion

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode=[{2}], Node=[{3}] ,DenseBoxWeightCheckReport Dense Box Count =[{4}]," +
                                "Dense Box ID#01	=[{5}] ,Dense Box ID#02 =[{6}] ,Weight =[{7}],Port No=[{8}]",
                                        eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, boxcount, id1, id2,weight ,portno));
                RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, boxcount, id1,
                    id2, "", "2", "", portno, "", "", "", "", "", "", "", "", "", "", "", weight, "", line.File.HostMode.ToString());
                #region MES Data ValidateBoxWeightRequest
                //(string trxID, string lineName, string boxtotalweight, IList boxlist)
                List<string> boxlist = new List<string>();
                if (id1.Trim() !=string.Empty) boxlist.Add(id1);
                if (id2.Trim() != string.Empty) boxlist.Add(id2);
                
                #region Add Reply Key
                //MES Reply no Mechine Name (PLC Write Key),BC Add Repository 自行處理加入倉庫
                string key = keyBoxReplyPLCKey.DenseBoxWeightCheckReportReply;
                string rep = eqp.Data.NODENO + ";" + portno;  //僅為了回覆PLC, MES完全不會用到
                
                if (Repository.ContainsKey(key))
                    Repository.Remove(key);
                Repository.Add(key, rep);
                #endregion

                Invoke(eServiceName.MESService, "ValidateBoxWeightRequest", new object[] { inputData.TrackKey, eqp.Data.LINEID,weight, boxlist,eqp.Data.NODENO});

                #endregion

                #region Update Port Object
                Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portno.PadLeft(2, '0'));
                if (port != null)
                {
                    lock (port)
                    {
                        port.File.PortBoxID1 = id1.Trim();
                        port.File.PortBoxID2 = id2.Trim();
                    }
                }
                Invoke(eServiceName.UIService, "DenseStatusReport", new object[] { port });
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    DenseBoxWeightCheckReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey, "2", "0");
            }
        }
        public  void DenseBoxWeightCheckReportReply(string eqpNo, eBitResult value, string trackKey, string rtncode,string portNo)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "DenseBoxWeightCheckReportReply") as Trx;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, eqpNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();

                if (rtncode == null)
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                else
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = rtncode;
                    outputdata.EventGroups[0].Events[0].Items[1].Value = portNo;
                }
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxWeightCheckReplyTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxWeightCheckReplyTimeout);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(eqpNo + "_" + DenseBoxWeightCheckReplyTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxWeightCheckReportReplyTimeout), trackKey);


                    RecordPPKEventHistory(outputdata.TrackKey, MethodBase.GetCurrentMethod().Name, eqp,"","","", "", "2", "",portNo, "","",
                        "", rtncode.ToString(), "", "", "", "", "", "", "", "", "", line.File.HostMode.ToString());
                }

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,DenseBoxWeightCheckReportReply Set Bit =[{2}]."
                    , eqpNo, trackKey, value.ToString()));
                
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void DenseBoxWeightCheckReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] EQP Reply, DenseBoxWeightCheckReportReply Set Bit =[OFF].", sArray[0], trackKey));

                DenseBoxWeightCheckReportReply(sArray[0], eBitResult.OFF, trackKey, null,null);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        #region [LabelInformationforBoxRequest]
        private const string LabelInformationforBoxRequestTimeout = "LabelInformationforBoxRequestTimeout";
        public void LabelInformationforBoxRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                #region[Get EQP & LINE]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                #endregion
                #region [PLCAgent Data Bit]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                #region[If Bit Off->Return]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                    LabelInformationforBoxRequestReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, eReturnCode1.Unknown, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                    return;
                }
                #endregion
                #region [PLCAgent Data Word]
                string boxID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                string boxType = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                #endregion
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM Mode =[{2}], Node =[{3}] ,BoxID =[{4}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, boxID));
                //依設定 回機台OK/NG
                eReturnCode1 rtncode = ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean() == true ? eReturnCode1.OK : eReturnCode1.NG;
                #region[If OFFLINE -> Return]
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));
                    LabelInformationforBoxRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, rtncode, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
                    return;
                }
                #endregion 
                switch (line.Data.LINETYPE)
                {
                    case eLineType.CELL.CCPCS:
                        boxType = "DPBox";
                        break;
                    default:
                        break;
                }
                #region [MES Data box Label Info Request]
                List<Cassette> subBoxList = new List<Cassette>();
                object[] _data = new object[]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODENO,
                    eqp.Data.NODEID,
                    boxID,             /* boxID */
                    boxType,             /* boxType */
                    subBoxList       /* subBoxList */
                };
                //Send MES Data
                Invoke(eServiceName.MESService, "BoxLabelInformationRequest", _data);

                #endregion

                #region Save History
                //RecordJobHistory(job, eJobEvent.Sent);
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                LabelInformationforBoxRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, eReturnCode1.NG, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");                    
            }
        }
        public void LabelInformationforBoxRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode,  string itemName1, string itemValue1,
        string itemName2, string itemValue2,string itemName3, string itemValue3,string itemName4, string itemValue4,string itemName5, string itemValue5,
            string itemName6, string itemValue6,string itemName7, string itemValue7,string itemName8, string itemValue8,string itemName9, string itemValue9,
            string itemName10, string itemValue10,string itemName11, string itemValue11)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "LabelInformationforBoxRequestReply") as Trx;
                #region[If Bit Off->Return]
                if (value == eBitResult.OFF)
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    #region[Log]
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,LabelInformationforBoxRequestReply Set Bit =[{2}]).",
                        eqpNo, trackKey, value.ToString()));
                    #endregion
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + LabelInformationforBoxRequestTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + LabelInformationforBoxRequestTimeout);
                    }
                    return;
                }
                #endregion
                #region[MES Data ]
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)rtncode).ToString();  // returnCode(INT);
                outputdata.EventGroups[0].Events[0].Items[1].Value = itemName1;  // SHIPID
                outputdata.EventGroups[0].Events[0].Items[2].Value = itemValue1;  // SHIPID;
                outputdata.EventGroups[0].Events[0].Items[3].Value = itemName2;  // QUANTITY;
                outputdata.EventGroups[0].Events[0].Items[4].Value = itemValue2;  // QUANTITY;
                outputdata.EventGroups[0].Events[0].Items[5].Value = itemName3;  // BOMVERSION;
                outputdata.EventGroups[0].Events[0].Items[6].Value = itemValue3;  // BOMVERSION;
                outputdata.EventGroups[0].Events[0].Items[7].Value = itemName4;  // MODELNAME;
                outputdata.EventGroups[0].Events[0].Items[8].Value = itemValue4;  // MODELNAME;
                outputdata.EventGroups[0].Events[0].Items[9].Value = itemName5;  // MODELVERSION;
                outputdata.EventGroups[0].Events[0].Items[10].Value = itemValue5;  // MODELVERSION;
                outputdata.EventGroups[0].Events[0].Items[11].Value = itemName6;  // ENVIRONMENTFLAG;
                outputdata.EventGroups[0].Events[0].Items[12].Value = itemValue6;  // ENVIRONMENTFLAG;
                outputdata.EventGroups[0].Events[0].Items[13].Value = itemName7;  // PARTID;
                outputdata.EventGroups[0].Events[0].Items[14].Value = itemValue7;  // PARTID;
                outputdata.EventGroups[0].Events[0].Items[15].Value = itemName8;  // CARRIERNAME;
                outputdata.EventGroups[0].Events[0].Items[16].Value = itemValue8;  // CARRIERNAME;
                outputdata.EventGroups[0].Events[0].Items[17].Value = itemName9;  // NOTE;
                outputdata.EventGroups[0].Events[0].Items[18].Value = itemValue9;  // NOTE;
                outputdata.EventGroups[0].Events[0].Items[19].Value = itemName10;  // COUNTRY;
                outputdata.EventGroups[0].Events[0].Items[20].Value = itemValue10;  // COUNTRY;
                outputdata.EventGroups[0].Events[0].Items[21].Value = itemName11;  // WT;
                outputdata.EventGroups[0].Events[0].Items[22].Value = itemValue11;  // WT;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString(); //Write Word dely 200 ms then Bit On
                outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
                #endregion
                #region[Create Timeout Timer]
                if (_timerManager.IsAliveTimer(eqpNo + "_" + LabelInformationforBoxRequestTimeout))
                {
                    _timerManager.TerminateTimer(eqpNo + "_" + LabelInformationforBoxRequestTimeout);
                }
                _timerManager.CreateTimer(eqpNo + "_" + LabelInformationforBoxRequestTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(LabelInformationforBoxRequestReplyTimeout), trackKey);
                #endregion
                #region[Log]
                string datalog = string.Format("SHIPID=[{0}]:[{1}],QUANTITY=[{2}]:[{3}],BOMVERSION=[{4}]:[{5}],MODELNAME=[{6}]:[{7}],MODELVERSION=[{8}]:[{9}],ENVIRONMENTFLAG=[{10}]:[{11}],PARTID=[{12}]:[{13}],CARRIERNAME=[{14}]:[{15}],NOTE=[{16}]:[{17}],COUNTRY=[{18}]:[{19}],WT=[{20}]:[{21}]",
                   itemName1, itemValue1, itemName2, itemValue2, itemName3, itemValue3, itemName4, itemValue4, itemName5, itemValue5, itemName6, itemValue6, itemName7, itemValue7, itemName8, itemValue8, itemName9, itemValue9, itemName10, itemValue10, itemName11, itemValue11);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,LabelInformationforBoxRequestReply Set Bit =[{2}],Rtncode =[{3}],[{4}])",
                    eqpNo, trackKey, value.ToString(), rtncode.ToString(), datalog));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        private void LabelInformationforBoxRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, PaperBoxLabelInformationRequestReply Timeout Set Bit (OFF).", sArray[0], trackKey));

                LabelInformationforBoxRequestReply(sArray[0], eBitResult.OFF, trackKey, eReturnCode1.Unknown, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        
        #endregion
        #region [Common Function]
        private void RecordPPKEventHistory(string trxID, string eventName, Equipment eqp, string boxCount, string boxID1,
            string boxID2, string boxType, string stageorPortorPalletorCar, string stageNo, string portNo, string palletId,
            string palletNo, string carNo, string returnCode, string productType, string grade1, string grade2,
            string cstSettingCode1, string cstSettingCode2, string glassCount1, string glassCount2, string weight, string removeFlag, string remark)
        {
            try
            {
                PPKEVENTHISTORY ppkHis = new PPKEVENTHISTORY();
                ppkHis.UPDATETIME = DateTime.Now;
                ppkHis.TRANSACTIONID = trxID;
                ppkHis.EVENTNAME = eventName;
                ppkHis.NODENO = eqp.Data.NODENO;
                ppkHis.NODEID = eqp.Data.NODEID;
                ppkHis.BOXCOUNT = boxCount;
                ppkHis.BOXID01 = boxID1;
                ppkHis.BOXID02 = boxID2;
                ppkHis.BOXTYPE = boxType;
                ppkHis.STAGEORPORTORPALLETORCAR = stageorPortorPalletorCar == "1" ? "STAGE" : stageorPortorPalletorCar == "2" ? "PORT"
                    : stageorPortorPalletorCar == "3" ? "PALLET" : stageorPortorPalletorCar == "3" ? "CAR" : "";
                ppkHis.STAGENO = stageNo;
                ppkHis.PORTNO = portNo;
                ppkHis.PALLETID = palletId;
                ppkHis.PALLETNO = palletNo;
                ppkHis.CARNO = carNo;
                ppkHis.RETURNCODE = returnCode;
                ppkHis.PRODUCTTYPE = productType;
                ppkHis.GRADE01 = grade1;
                ppkHis.GRADE02 = grade2;
                ppkHis.CASSETTESETTINGCODE01 = cstSettingCode1;
                ppkHis.CASSETTESETTINGCODE02 = cstSettingCode2;
                ppkHis.BOXGLASSCOUNT01 = glassCount1;
                ppkHis.BOXGLASSCOUNT02 = glassCount2; 
                ppkHis.WEIGHT = weight;
                ppkHis.REMOVEREASONFLAG = removeFlag;
                ppkHis.REMARK = remark;
                ObjectManager.PalletManager.RecordPPKEventHistory(ppkHis);
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

        //Watson Add For DenseBox Remove
        private IList<Job> FindInBoxJobs(string BoxID)
        {
            try
            {
                IList<Job> joblist;  //InBox Store In.
                joblist =  ObjectManager.JobManager.GetJobs().Where(j => j.ToCstID == BoxID && j.RemoveFlag == false).ToList();
                if (joblist == null) //InBox Not Fetch out
                     joblist =  ObjectManager.JobManager.GetJobs().Where(j => j.FromCstID ==BoxID && j.JobProcessFlows.Count == 0 && j.RemoveFlag == false).ToList();
                 return joblist;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        private void MES_BOXPROCESSSTARTBYDPI(Line line, Equipment eqp, Cassette cst, Cassette cst2, string trxID)
        {
            try
            {
                if (line.Data.LINETYPE != eLineType.CELL.CBDPI)
                    return;
                string denseBoxID1 = cst.CassetteID;
                string denseBoxID2 = cst2.CassetteID;
                object[] obj3 = new object[]
                {
                    trxID,
                    line,
                    cst,
                    new List<string> {denseBoxID1,denseBoxID2}
                };
                Invoke(eServiceName.MESService, "BoxProcessStartedDPI", obj3);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
    }
}
